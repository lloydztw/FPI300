#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-09-02 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion


using LaserAlignDX.Model.Coords;

namespace LaserAlignDX
{
    /// <summary>
    /// 全域資料夾與檔案
    /// NOTE: 使用 => 比較不會永久占用內存
    /// </summary>
    public static class GaMvcPaths
    {
        public static string GA_WORK_PATH => Traveller106.Universal.WORKPATH;
        public static string CALIB_RECIPE_FILE(CarrierEnum C, object dummy = null)
        {
            //string tag = "";
            //if (viewID > 0)
            //{
            //    tag = $"@{C}#{S}";
            //}
            //else
            //{
            //    if (C != CarrierEnum.C1)
            //        tag = $"@{C}";
            //}
            ////string ext = C != CarrierEnum.C1 ? $"@{C}.json" : ".json";
            //string ext = tag + ".json";
            //return System.IO.Path.Combine(GA_WORK_PATH, "Calibration", "Jx_Calib_Vision_Settings" + ext);

            string fileName = System.IO.Path.Combine(GA_WORK_PATH, "Calibration", $"Jx_Calib_Recipe@{C}.json");
            return fileName;
        }
        public static string CALIB_TRANSFORMS_FILE
        {
            // 共用一份
            get => System.IO.Path.Combine(GA_WORK_PATH, "Calibration", "Jx_Calib_Transforms.ini");
        }
    }
}