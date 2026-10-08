#region AUTHOR
/*
 *
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 *
 * REVISION:
 *      2026-10-08 使用 ChatGPT Codex  重整 (by LeTian Chang)
 *      2025-09-02 初稿 (by LeTian Chang)
 *
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 *
 */
#endregion

using System;
using System.Drawing;
using System.Windows.Forms;
using EzAoiEmptyTrayInspector;
using JetEazy.EzImage;
using JetEazy.OpenCV;
using LaserAlignDX.AoiModel;
using LaserAlignDX.Mvc.Model;
using Traveller106;


#if (OPT_CALIB_V5)
using FormCalibrationTool = LaserAlignDX.Mvc.Gui.Calib.V5.FormCalibrationTool;
using FormRcpEditorTool = LaserAlignDX.Mvc.Gui.V35.FormRecipeEditor;
using FormTemplateEditor = LaserAlignDX.Mvc.Gui.V35.FormTemplateEditor;
#elif (OPT_CALIB_V4)
using FormCalibrationTool = LaserAlignDX.Mvc.Gui.Calib.V5.FormCalibrationTool;
using FormRcpEditorTool = LaserAlignDX.Mvc.Gui.V25.FormRecipeEditor;
#elif (OPT_CALIB_V3)
using FormCalibrationTool = LaserAlignDX.Mvc.Gui.Calib.V3.FormCalibrationTool;
using FormRcpEditorTool = LaserAlignDX.Mvc.Gui.V25.FormRecipeEditor;
#else
using FormCalibrationTool = LaserAlignDX.Mvc.Gui.Calib.V25.FormCalibrationTool;
using FormRcpEditorTool = LaserAlignDX.Mvc.Gui.V25.FormRecipeEditor;
#endif


namespace LaserAlignDX
{
    /// <summary>Owns modal editor creation; shared services are supplied by the application.</summary>
    internal sealed class AppDialogService
    {
        #region PRIVATE_DATA

        readonly Func<ITravelerModel> _getSystemModel;

        #endregion

        #region PRIVATE_PROPERTIES

        ITravelerModel _sysModel => _getSystemModel();

        #endregion

        internal AppDialogService(Func<ITravelerModel> getSystemModel)
        {
            _getSystemModel = getSystemModel ?? throw new ArgumentNullException(nameof(getSystemModel));
        }

        // VIEW ----------------------------------------------------
        public void OpenRecipeEditor()
        {
            try
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
            catch(Exception ex)
            {
                //LtDebug.LOG.Error(ex, "[OpenRecipeEditor] 異常");
            }
        }
        public void OpenTamplateEditor(CarrierEnum C)
        {
            using (var dlg = new FormTemplateEditor(C))
            {
                dlg.WindowState = FormWindowState.Maximized;
                dlg.ShowDialog();
            }
        }
        public void OpenCalibrationTool()
        {
            var backID = _sysModel.ActiveCarrierID;

            using (var dlg = new FormCalibrationTool())
            {
                dlg.WindowState = FormWindowState.Maximized;
                dlg.ShowDialog();
            }

            //為安全起見, 重新再次載入 Recipe
            _sysModel.ActiveCarrierID = backID;
            _sysModel.ApplyRecipe();
        }
        public void OpenEmptyTrayInspectTool(Form owner, string recipeName = null, Bitmap bmpToShow = null)
        {
            if (recipeName == null)
                recipeName = LtAoiFactory.GetActiveRecipeNameAtFPI30();

            var emptyAoiRecipeName = LtAoiFactory.RcpStemName(recipeName, _sysModel.ActiveCarrierID);
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
                        PushBitmapToEmptyTrayTool(bmpToShow, $"[Recipe] {recipeName} (bmpOrg)");
                    }).BeginInvoke(null, null);
                };
            }

            frm.ShowDialog(owner);

            // RESERVED 重新再次載入 RecipeCombo
            //_sysModel.ApplyRecipe();
        }
        public void PushBitmapToEmptyTrayTool(Bitmap bmp, string name)
        {
            using (var bridge = new QxImageBridge(bmp))
            {
                var qImg = new EzQuickImage(bridge.Image, true);
                AoiFactory.PushImage(qImg, name);
            }
        }

    }
}
