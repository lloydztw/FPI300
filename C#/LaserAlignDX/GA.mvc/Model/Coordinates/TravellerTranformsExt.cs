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
    public static class TravellerTransformsExt
    {
        const int N_COMBINES = 4;
        const int N_POINTS = QTransform.N_POINTS;

        #region GLOBAL_PATH
        static string GA_WORK_PATH => Traveller106.Universal.WORKPATH;
        static string GA_CALIB_INI_FILE(int index, bool useFullPath = false)
        {
            var fname = $"Calibrate_default_info{index}.ini";
            if (useFullPath)
                return System.IO.Path.Combine(GA_WORK_PATH, "Calibration", fname);
            return fname;
        }
        #endregion

        /// <summary>
        /// 載入 Gaara 舊的 $"Calibrate_default_info{i}.ini" 參數檔
        /// </summary>
        public static void LoadGaaraIniFile(this TravellerTransforms trfs)
        {
            for (int i = 0; i < N_COMBINES; i++)
            {
                CarrierEnum c = (CarrierEnum)(i / 2);
                SuckerRowEnum s = (SuckerRowEnum)(i % 2);
                var transform = trfs.GetCameraMotorTransform(c, s);
                loadGaaraIni(transform, GA_CALIB_INI_FILE(i, true));
            }
        }

        /// <summary>
        /// 存入 Gaara 舊的 $"Calibrate_default_info{i}.ini" 參數檔
        /// </summary>
        public static void SaveGaaraIniFile(this TravellerTransforms trfs)
        {
            return;
            for (int i = 0; i < N_COMBINES; i++)
            {
                CarrierEnum c = (CarrierEnum)(i / 2);
                SuckerRowEnum s = (SuckerRowEnum)(i % 2);
                var transform = trfs.GetCameraMotorTransform(c, s);
                saveGaaraIni(transform, GA_CALIB_INI_FILE(i, true));
            }
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

        static void loadGaaraIni(QTransform trf, string iniFileName)
        {
            //[Tray]
            //; 虚拟点
            //ptsview_0 = 4178.286,4237.869     //LT
            //ptsview_1 = 11840.27,4216.04      //RT
            //ptsview_2 = 4385.45,34822.98      //LB
            //ptsview_3 = 12050,34803.5         //RB

            //; 实际点
            //ptsworld_0 = -1016.96,-213.238
            //ptsworld_1 = -959.27,-213.238
            //ptsworld_2 = -1017.018,6.835
            //ptsworld_3 = -959.54,6.835

            string sectName = "Tray";

            bool loadPoint(string keyName, out double x, out double y)
            {
                x = 0;
                y = 0;
                string str = "";
                JetEazy.Win32.Win32Ini.Load(ref str, iniFileName, sectName, keyName);
                if (string.IsNullOrEmpty(str))
                    return false;
                var strs = str.Split(',');
                if(strs.Length >=2 && double.TryParse(strs[0].Trim(), out x) && double.TryParse(strs[1].Trim(), out y))
                    return true;
                return false;
            };

            int[] gIndexes = new[] { 0, 1, 3, 2 }; 

            for (int i = 0; i < N_POINTS; i++)
            {
                var pt = trf.GetSrcRef(i);
                var gi = gIndexes[i];

                if (loadPoint($"ptsview_{gi}", out double x, out double y))
                {
                    pt.X = x;
                    pt.Y = y;
                    trf.SetSrcRef(i, pt);
                }
            }

            for (int i = 0; i < N_POINTS; i++)
            {
                var pt = trf.GetDstRef(i);
                var gi = gIndexes[i];

                if (loadPoint($"ptsworld_{gi}", out double x, out double y))
                {
                    pt.X = x;
                    pt.Y = y;
                    trf.SetDstRef(i, pt);
                }
            }
        }
        static void saveGaaraIni(QTransform trf, string iniFileName)
        {
            //[Tray]
            //; 虚拟点
            //ptsview_0 = 4178.286,4237.869
            //ptsview_1 = 11840.27,4216.04
            //ptsview_2 = 4385.45,34822.98
            //ptsview_3 = 12050,34803.5

            //; 实际点
            //ptsworld_0 = -1016.96,-213.238
            //ptsworld_1 = -959.27,-213.238
            //ptsworld_2 = -1017.018,6.835
            //ptsworld_3 = -959.54,6.835

            string sectName = "Tray";
            int[] gIndexes = new[] { 0, 1, 3, 2 };

            for (int i = 0; i < N_POINTS; i++)
            {
                var pt = trf.GetSrcRef(i);
                var str = $" {pt.X:0.000},{pt.Y:0.000}";

                int gi = gIndexes[i];
                JetEazy.Win32.Win32Ini.Save(str, iniFileName, sectName, $"ptsview_{gi}");
            }

            for (int i = 0; i < N_POINTS; i++)
            {
                var pt = trf.GetDstRef(i);
                var str = $" {pt.X:0.000},{pt.Y:0.000}";

                int gi = gIndexes[i];
                JetEazy.Win32.Win32Ini.Save(str, iniFileName, sectName, $"ptsworld_{gi}");
            }
        }
    }
}
