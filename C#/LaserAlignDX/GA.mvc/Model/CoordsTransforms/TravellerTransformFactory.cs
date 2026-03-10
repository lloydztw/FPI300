using JetEazy.Match;
using JetEazy.QMath;
using JetEazy.Utils;
using LeTian.AoiLib;
using System;
using QMicroChipTransform = LaserAlignDX.Model.Coords.V33.QMicroChipTransform;
using TravellerTransforms = LaserAlignDX.Model.Coords.V33.TravellerTransforms;

namespace LaserAlignDX.Model.Coords
{
    public partial class TravellerTransformFactory
    {
        #region CONSTS
        public static bool OPT_CALIB_GRID_USING_MOTOR_COORD => TravellerTransforms.OPT_CALIB_GRID_USING_MOTOR_COORD;
        public static int N_CALIB_POINTS => TravellerTransforms.N_CALIB_POINTS;
        #endregion

        public static IMicroChipTransform CreateMicroChipTransform(string name)
        {
            return new QMicroChipTransform(name);
        }

        /// <summary>
        /// 所有參數共用基礎 的 座標轉換系統
        /// </summary>
        public static ITravellerTransforms CommonBase
        {
            get
            {
                return Instance("$CommonBase$");
            }
        }

        /// <summary>
        /// 個別參數 的 座標轉換系統
        /// </summary>
        public static ITravellerTransforms Instance(string name)
        {
            QVector.Percision = 12;
            return TravellerTransforms.Instance(name);
        }

        /// <summary>
        /// 卸載所有 座標轉換系統
        /// </summary> 
        public static void DisposeAll()
        {
            TravellerTransforms.DisposeAll();
            QMicroChipTransform.DisposeAll();
        }

        /// <summary>
        /// 只保留 name (與 $CommonBase$) (與IMicroChipTransform) 座標轉換系統 其餘都卸載
        /// </summary>
        public static void Keep(string name)
        {
            TravellerTransforms.DisposeAll(name, "$CommonBase$");
        }
    }


    partial class TravellerTransformFactory
    {
        static bool OPT_CLONE_BY_LOAD_FILE => true;

        #region NLOG
        internal static NLog.ILogger _LOG => TravellerTransforms._LOG;
        #endregion

        /// <summary>
        /// 根據 新的相機格點 newCamGrid, 格點節距 newPitch 以 CommonBase 為基礎,
        /// 進行線性遷移 生成新的 座標轉換系統
        /// </summary>
        public static ITravellerTransforms CreateLinearMigration(string name, CarrierEnum carrierID, EzBlocsGrid newCamGrid, QVector newPitch)
        {
            if (string.IsNullOrEmpty(name) || newCamGrid == null || newPitch == null)
                return null;

            try
            {
                GaUtil.LOG($"線性遷移 [{name}] [{carrierID}] : 開始 : (R,C)=({newCamGrid.Rows},{newCamGrid.Cols}) : PitchX= {newPitch.X} : PitchY= {newPitch.Y}");

                //(0) CommonBase
                var commonBaseTrf = CommonBase;

                //(1) Instance
                var newTrf = TravellerTransformFactory.Instance(name);

                //(2) 簡單透過檔案進行 Clone
                if (OPT_CLONE_BY_LOAD_FILE)
                {
                    //var tmpPath = System.IO.Path.GetTempPath();
                    //var tmpFile = System.IO.Path.Combine(tmpPath, $"jx_clone_transform.ini");
                    //var clonedTrf = TravellerTransformFactory.Instance(name);
                    //commonBaseTrf.Save(tmpFile);
                    newTrf.Load(GaMvcPaths.COMMON_BASE_TRANSFORMS_INI_FILE);
                }
                else
                {
                    commonBaseTrf.Load(GaMvcPaths.COMMON_BASE_TRANSFORMS_INI_FILE);
                    commonBaseTrf.BuildAll();
                }

                //(3) [線性遷移] Camera-ToWorld 座標轉換系統 : 重新設定 rows, cols, pitchX, pitchY 布局
                newTrf.ConfigWorldGridPoints(newCamGrid.Rows, newCamGrid.Cols, newPitch.X, newPitch.Y);

                //(4) [線性遷移] Camera-ToWorld 座標轉換系統 : 重新設定 相機格點
                newTrf.SetCalibCamGrid(carrierID, newCamGrid);

                //(5) [線性遷移] Camera-Motor 座標轉換系統
                if (!OPT_CLONE_BY_LOAD_FILE)
                {
                    // 因為 Camera-Motor 座標轉換系統 的 校正點位, 沿用 CommonBase 沒有變動.
                    // 所以 !OPT_CLONE_BY_LOAD_FILE 成立時, 以下才需要執行.
                    foreach (SuckerRowEnum suckerID in Enum.GetValues(typeof(SuckerRowEnum)))
                    {
                        var srcTrfCM = commonBaseTrf.GetCameraMotorTransform(carrierID, suckerID);
                        var srcCalib = srcTrfCM.GetCalibGridPoints();
                        int rows = srcCalib.Rows;
                        int cols = srcCalib.Cols;
                        var camPts = new QVector[rows, cols];
                        var motorPts = new QVector[rows, cols];
                        for (int r = 0; r < rows; r++)
                            for (int c = 0; c < cols; c++)
                                srcCalib.Get(r, c, out camPts[r, c], out motorPts[r, c]);

                        var dstTrfCM = newTrf.GetCameraMotorTransform(carrierID, suckerID);
                        var dstCalib = dstTrfCM.GetCalibGridPoints();
                        dstCalib.SetAll(camPts, motorPts);
                    }
                }

                //(6) Build
                newTrf.BuildAll();

                GaUtil.LOG($"線性遷移 [{name}] [{carrierID}] : 完成");
                return newTrf;
            }
            catch (Exception ex)
            {
                GaUtil.LOG_ERROR(ex, $"線性遷移 [{name}] [{carrierID}] 異常");
                throw;
            }
        }
    }
}
