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
using LaserAlignDX.Mvc.Model;


//using FormCalibrationTool = LaserAlignDX.FormSpace.FPI30Form.frmCalibration;
using FormRcpEditorTool = LaserAlignDX.FormSpace.frmFPIRecipe;
using FormCalibrationTool = LaserAlignDX.Mvc.Gui.FormCalibrationTool;
//using FormRcpEditorTool = LaserAlignDX.Mvc.Gui.FormRcpEditorTool;
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
        public static int TOTAL_FLY_CAMERAS => 4;

        #region PRIVATE_DATA
        static TravellerSysModel _sysModel;
        #endregion

        // MODEL ----------------------------------------------------
        public static ITravelerModel SysModel
        {
            get
            {
                if (_sysModel == null)
                {
                    var aoiModel = InstanceAoiModel();
                    _sysModel = TravellerSysModel.Instance(aoiModel);
                }
                return _sysModel;
            }
        }
        private static IProcessRunFPI InstanceAoiModel()
        {
            // AoiModel 使用 V2
            return AoiModel.V25.ProcessRunFPIClass.Instance;
        }
        public static IxReportBuilder CreateReportBuilder()
        {
            // 使用力成報表
            return new PowerTechReportBuilder();
        }

        // CTRL ----------------------------------------------------
        public static GaMainCtrl CreateMainCtrl()
        {
            // GaMainCtrl 使用 V2
            return new global::LaserAlignDX.Mvc.Ctrl.V2.GaMainCtrl();
        }

        // VIEW ----------------------------------------------------
        public static void OpenRecipeEditor()
        {
            using (var dlg = new FormRcpEditorTool())
            {
                dlg.ShowDialog();
            }

            //為安全起見, 重新再次載入 RecipeCombo
            SysModel.ApplyRecipe();
        }
        public static void OpenCalibrationTool()
        {
            using (var dlg = new FormCalibrationTool())
            {
                dlg.ShowDialog();
            }

            //為安全起見, 重新再次載入 RecipeCombo
            SysModel.ApplyRecipe();
        }

        // Dispose -------------------------------------------------
        public static void DisposeAll()
        {
            _sysModel?.Dispose();
            _sysModel = null;
        }
    }
}