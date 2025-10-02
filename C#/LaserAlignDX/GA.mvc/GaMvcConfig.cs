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

using EzAoiEmptyTrayInspector;
using JetEazy.EzImage;
using JetEazy.OpenCV;
using LaserAlignDX.AoiModel;
using LaserAlignDX.Model;
using LaserAlignDX.Mvc.Gui;
using LaserAlignDX.Mvc.Model;
using System;
using System.Drawing;
using System.Windows.Forms;
using Traveller106;

//using FormCalibrationTool = LaserAlignDX.FormSpace.FPI30Form.frmCalibration;
//using FormRcpEditorTool = LaserAlignDX.FormSpace.frmFPIRecipe;
using FormCalibrationTool = LaserAlignDX.Mvc.Gui.FormCalibrationTool;
using FormRcpEditorTool = LaserAlignDX.Mvc.Gui.FormRecipeEditor;
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
        #region CONFIG
        public static bool OPT_USE_LETIAN_CALIB = true;
        public static int TOTAL_FLY_FRAMES_COUNT => 4;
        #endregion

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
            // AoiModel 使用 V27
            return AoiModel.V27.ProcessRunFPIClass.Instance;
        }
        public static IxReportBuilder CreateReportBuilder()
        {
            // 使用力成報表
            return new PowerTechReportBuilder();
        }

        // CTRL ----------------------------------------------------
        public static GaMainCtrl CreateMainCtrl()
        {
            //// GaMainCtrl 使用 V2
            //return new global::LaserAlignDX.Mvc.Ctrl.V2.GaMainCtrl();
            // GaMainCtrl 使用 V3
            return new global::LaserAlignDX.Mvc.Ctrl.V3.GaMainCtrl();
        }

        // VIEW ----------------------------------------------------
        public static void OpenRecipeEditor()
        {
            var backID = _sysModel.ActiveCarrierID;

            using (var dlg = new FormRcpEditorTool())
            {
                dlg.ShowDialog();
            }

            //為安全起見, 重新再次載入 Recipe
            _sysModel.ActiveCarrierID = backID;
            _sysModel.ApplyRecipe();
        }
        public static void OpenTamplateEditor(CarrierEnum C)
        {
            using (var dlg = new FormTemplateEditor(C))
            {
                dlg.ShowDialog();
            }
        }
        public static void OpenCalibrationTool()
        {
            var backID = _sysModel.ActiveCarrierID;

            using (var dlg = new FormCalibrationTool())
            {
                dlg.ShowDialog();
            }

            //為安全起見, 重新再次載入 Recipe
            _sysModel.ActiveCarrierID = backID;
            _sysModel.ApplyRecipe();
        }
        public static void OpenEmptyTrayInspectTool(Form owner, string recipeName = null, Bitmap bmpToShow = null)
        {
            if (recipeName == null)
                recipeName = LtAoiFactory.GetActiveRecipeNameAtFPI30();

            var emptyAoiRecipeName = LtAoiFactory.RcpStemName(recipeName, GaMvcConfig.SysModel.ActiveCarrierID);
            var frm = AoiFactory.OpenEmptyTrayInspectorTool(owner, emptyAoiRecipeName);
            if (frm == null)
                return;

            if (bmpToShow != null)
            {
                frm.Load += (s, e) =>
                {
                    new Action(() =>
                    {
                        System.Threading.Thread.Sleep(2000);
                        PushBitmapToEmptyTrayTool(bmpToShow, $"[參數] {recipeName} (bmpOrg)");
                    }).BeginInvoke(null, null);
                };
            }

            frm.ShowDialog(owner);

            // RESERVED 重新再次載入 RecipeCombo
            //_sysModel.ApplyRecipe();
        }
        public static void PushBitmapToEmptyTrayTool(Bitmap bmp, string name)
        {
            using (var bridge = new QxImageBridge(bmp))
            {
                var qImg = new EzQuickImage(bridge.Image, true);
                AoiFactory.PushImage(qImg, name);
            }
        }

        // Dispose -------------------------------------------------
        public static void DisposeAll()
        {
            _sysModel?.Dispose();
            _sysModel = null;
        }
    }
}