#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-08-13 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion


using LaserAlignDX.Model.Transforms;
using LaserAlignDX.OPSpace.RecipeSpace;

namespace LaserAlignDX.Model.Coords
{
    /// <summary>
    /// 為 TravellerTransforms 外掛轉換 Gaara 數據的函式
    /// </summary>
    public static class TravellerTransformsUtil
    {
        #region GLOBAL_PATH
        static string GA_WORK_PATH => Traveller106.Universal.WORKPATH;
        static string GA_CALIB_INI_FILE(int index)
        {
            return $"Calibrate_default_info{index}.ini";
        }
        #endregion

        /// <summary>
        /// 載入 Gaara 舊的 $"Calibrate_default_info{i}.ini" 參數檔
        /// </summary>
        public static void LoadGaaraIniFile(this TravellerTransforms trfs, string workPath = null)
        {
            int N_COMBINES = 4;

            var gaCalibs = Traveller106.Universal.LineScanCalibrateClasses;
            if (gaCalibs == null)
                gaCalibs = new LineScanCalibrateClass[N_COMBINES];

            if (workPath == null)
                workPath = GA_WORK_PATH;

            for (int i = 0; i < N_COMBINES; i++)
            {
                if (gaCalibs[i] == null)
                {
                    gaCalibs[i] = new LineScanCalibrateClass();
                    gaCalibs[i].Initial(workPath, 0, GA_CALIB_INI_FILE(i));
                    gaCalibs[i].Load();
                }
            }

            trfs.ConvertFromGaara(gaCalibs);
        }

        /// <summary>
        /// 轉換 Gaara 舊的 LineScanCalibrateClass 數據
        /// </summary>
        public static void ConvertFromGaara(this TravellerTransforms trfs, LineScanCalibrateClass[] gaCalibs)
        {
            for (int i = 0, len = gaCalibs.Length; i < len; i++)
            {
                CarrierEnum c = (CarrierEnum)(i / 2);
                SuckerRowEnum s = (SuckerRowEnum)(i % 2);
                var transform = trfs.GetCameraMotorTransform(c, s);
                transform.ConvertFromGaara(gaCalibs[i]);
            }
        }

        /// <summary>
        /// 轉換 Gaara 舊的 LineScanCalibrateClass 數據
        /// </summary>
        public static void ConvertFromGaara(this QTransform trf, LineScanCalibrateClass gaCalib)
        {
            int N_POINTS = QTransform.N_POINTS;

            for (int i = 0; i < N_POINTS; i++)
            {
                var camCoord = trf.GetSrcRef(i);
                var motorCoord = trf.GetDstRef(i);
                var camPt = gaCalib.ptsview[i];
                var motorPt = gaCalib.ptsworld[i];
                camCoord.X = System.Math.Round(camPt.X);
                camCoord.Y = System.Math.Round(camPt.Y);
                motorCoord.X = motorPt.X;
                motorCoord.Y = motorPt.Y;
                trf.SetSrcRef(i, camCoord);
                trf.SetDstRef(i, motorCoord);
            }
        }
    }
}
