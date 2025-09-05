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


using LaserAlignDX.OPSpace.RecipeSpace;
using System;

namespace LaserAlignDX.Model.Coords
{
    /// <summary>
    /// 為 TravellerTransforms 外掛轉換 Gaara 數據的函式
    /// </summary>
    public static class TravellerTransformsUtil
    {
        /// <summary>
        /// 載入 Gaara 舊的 $"Calibrate_default_info{i}.ini" 參數檔
        /// </summary>
        public static void LoadGaaraIniFile(this TravellerTransforms trfs, string filename)
        {
            int N = 4;
            var gaCalibs = new LineScanCalibrateClass[N];

            string path = @"D:\AUTOMATION\Eazy FPI30\_BIN_\LASER-MAIN_FPIX3\WORK\Calibration";
            for (int i = 0; i < N; i++)
            {
                string fname = System.IO.Path.Combine(path, $"Calibrate_default_info{i}.ini");
                gaCalibs[i] = new LineScanCalibrateClass();
                gaCalibs[i].Load();
            }

            trfs.ConvertFromGaara(gaCalibs);
        }

        /// <summary>
        /// 轉換 Gaara 舊的 LineScanCalibrateClass 數據
        /// </summary>
        public static void ConvertFromGaara(this TravellerTransforms trfs, LineScanCalibrateClass[] gaCalibs)
        {
            throw new NotImplementedException();
        }
    }
}
