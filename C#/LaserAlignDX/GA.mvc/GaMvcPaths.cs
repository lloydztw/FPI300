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

        /// <summary>
        /// 大校正板 像測調教 參數檔
        /// </summary>
        public static string CALIB_RECIPE_FILE(CarrierEnum C, object dummy = null)
        {
            string fileName = System.IO.Path.Combine(GA_WORK_PATH, "Calibration", $"Jx_Calib_Recipe@{C}.json");
            return fileName;
        }
        
        /// <summary>
        /// 基礎座標轉換系統 保存檔案 (所有參數檔共用)
        /// </summary>
        public static string COMMON_BASE_TRANSFORMS_INI_FILE
        {
            get => System.IO.Path.Combine(GA_WORK_PATH, "Calibration", "Jx_Calib_Transforms.ini");
        }

        /// <summary>
        /// 個別參數 座標轉換系統 保存檔案
        /// </summary>
        public static string TRANSFORMS_INI_FILE(string fname)
        {
            string path = System.IO.Path.GetDirectoryName(fname);
            string iniFile = System.IO.Path.Combine(path, "Jx_Transforms.ini");
            return iniFile;
        }
    }
}