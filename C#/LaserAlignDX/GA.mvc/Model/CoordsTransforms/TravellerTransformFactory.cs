#region AUTHOR
/*
 * 
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2026-04-02 Added V35
 *      2025-08-13 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzAoiEmptyTrayInspector.Model;
using JetEazy.Match;
using JetEazy.QMath;
using JetEazy.Utils;
using System;
using System.Collections.Generic;

#if (OPT_TRANSFORM_V36)
using QMicroChipTransform = LaserAlignDX.Model.Coords.V36.QMicroChipTransform;
using TravellerTransforms = LaserAlignDX.Model.Coords.V36.TravellerTransforms;
#else
using QMicroChipTransform = LaserAlignDX.Model.Coords.V33.QMicroChipTransform;
using TravellerTransforms = LaserAlignDX.Model.Coords.V33.TravellerTransforms;
#endif

namespace LaserAlignDX.Model.Coords
{
    public partial class TravellerTransformFactory
    {
        #region CONSTS
        /// <summary>
        /// 馬達校正點數 (使用者所需要輸入的馬達座標點數)
        /// </summary>
        public static int N_CALIB_MOTOR_POINTS => TravellerTransforms.N_CALIB_MOTOR_POINTS;
        /// <summary>
        /// VER_3.3.0.0 線性遷移 使用 WORLD_COORD
        /// <br/> OPT_CALIB_GRID_USING_MOTOR_COORD == false
        /// </summary>
        public static bool OPT_CALIB_GRID_USING_MOTOR_COORD => TravellerTransforms.OPT_CALIB_GRID_USING_MOTOR_COORD;
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
            TravellerTransforms.DisposeAll(name, "$CommonBase$", "C1_Micro", "C2_Micro");
        }
    }


    partial class TravellerTransformFactory
    {
        #region NLOG
        internal static NLog.ILogger _LOG => TravellerTransforms._LOG;
        #endregion

        /// <summary>
        /// 根據 新的相機格點 newCamGrid, 格點節距 newPitch 以 CommonBase 為基礎,
        /// 進行線性遷移 生成新的 座標轉換系統
        /// </summary>
        public static ITravellerTransforms CreateLinearMigration(string name, CarrierEnum carrierID, EzBlocsGrid newCamGrid, QVector newPitch, IList<JxTraySegItem> segsList)
        {
            if (string.IsNullOrEmpty(name) || newCamGrid == null || newPitch == null)
                return null;

            try
            {
                GaUtil.LOG($"線性遷移 [{name}] [{carrierID}] : 開始 : (R,C)=({newCamGrid.Rows},{newCamGrid.Cols}) : PitchX= {newPitch.X} : PitchY= {newPitch.Y}");

                //(0) CommonBase
                var commonBaseTrf = CommonBase;
                commonBaseTrf.Load(GaMvcPaths.COMMON_BASE_TRANSFORMS_INI_FILE);
                commonBaseTrf.BuildAll();

                //(1) Instance
                var newTrf = TravellerTransformFactory.Instance(name);

                //(2) [線性遷移] Camera-World 座標轉換系統 : 重新設定 rows, cols, pitchX, pitchY 布局
                newTrf.ConfigWorldGridPoints(newCamGrid.Rows, newCamGrid.Cols, newPitch.X, newPitch.Y);

                //(3) [線性遷移] Camera-World 座標轉換系統 : 重新設定 相機格點
                newTrf.SetCalibCamGrid(carrierID, newCamGrid, segsList);

                //(4) [線性遷移] Camera-Motor 座標轉換系統 (使用 commonBaseTrf)
                foreach (SuckerRowEnum suckerID in Enum.GetValues(typeof(SuckerRowEnum)))
                {
                    var srcTrfCM = commonBaseTrf.GetCameraMotorTransform(carrierID, suckerID);
                    var dstTrfCM = newTrf.GetCameraMotorTransform(carrierID, suckerID);
                    var srcCalib = srcTrfCM.GetCalibGridPoints();
                    var dstCalib = dstTrfCM.GetCalibGridPoints();

                    int rows = srcCalib.Rows;
                    int cols = srcCalib.Cols;
                    var camPts = new QVector[rows, cols];
                    var motorPts = new QVector[rows, cols];
                    for (int r = 0; r < rows; r++)
                        for (int c = 0; c < cols; c++)
                            srcCalib.Get(r, c, out camPts[r, c], out motorPts[r, c]);

                    dstCalib.SetAll(camPts, motorPts);
                }

                //(5) Build
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
