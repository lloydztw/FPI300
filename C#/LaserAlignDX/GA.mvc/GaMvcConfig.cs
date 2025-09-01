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

using LaserAlignDX.AoiModel;
using LaserAlignDX.Model;
using GaMainCtrl = LaserAlignDX.Mvc.Ctrl.Abs.GaMainCtrl;


namespace LaserAlignDX
{
    /// <summary>
    /// 由此控制 所使用的 優化階段 的 版本
    /// 達到最終優化階段後
    /// 只會留一個版本
    /// </summary>
    public static class GaMvcConfig
    {
        public static bool OPT_USE_LETIAN_CHIP_CELL_VIEWER = true;
        
        public static IProcessRunFPI InstanceAoiModel()
        {
            // AoiModel 使用 V2
            return AoiModel.V2.ProcessRunFPIClass.Instance;
        }
        
        public static GaMainCtrl CreateMainCtrl()
        {
            // GaMainCtrl 使用 V3 
            return new Mvc.Ctrl.V3.GaMainCtrl();
        }

        public static IxReportBuilder CreateReportBuilder()
        {
            // 使用力成報表
            return new PowerTechReportBuilder();
        }
    }
}