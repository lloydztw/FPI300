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


namespace LaserAlignDX
{
    /// <summary>
    /// 全域資料夾與檔案
    /// NOTE: 使用 => 比較不會永久占用內存
    /// </summary>
    public static class GaMvcPaths
    {
        public static string GA_WORK_PATH => Traveller106.Universal.WORKPATH;
        public static string CALIB_VISION_FILE => System.IO.Path.Combine(GA_WORK_PATH, "Calibration", "Jx_Calib_Vision_Settings.json");
        public static string CALIB_TRANSFORMS_FILE => System.IO.Path.Combine(GA_WORK_PATH, "Calibration", "Jx_Calib_Transforms.ini");
    }
}