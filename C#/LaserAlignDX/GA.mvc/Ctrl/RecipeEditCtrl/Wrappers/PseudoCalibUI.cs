#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-09-09 重新設計校正架構 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using LaserAlignDX.Mvc.Gui;
using System.Windows.Forms;

namespace LaserAlignDX.Mvc.Ctrl.Rcp
{
    /// <summary>
    /// 對 IvRecipeEditorUI 進行包裝 (Wrapper),
    /// 以便 重複利用 GaCalibCtrl
    /// </summary>
    class PseudoCalibUI : IvCalibToolUI
    {
        #region PRIVATE_DATA
        IvRecipeEditorUI _imp;
        #endregion

        public PseudoCalibUI(GaRecipeEditCtrl rcpCtrl)
        {
            _imp = rcpCtrl._rcpEditUI;
        }

        #region WRAPPERS
        Control IvCalibToolUI.Window => _imp.Window;

        JezTransImageViewPanel IvCalibToolUI.ImgViewer => _imp.ImgViewer;
        Button IvCalibToolUI.btnLoadImage => _imp.btnLoadImage;
        Button IvCalibToolUI.btnGrabImage => _imp.btnGrabImage;

        Button IvCalibToolUI.btnPickupGolden => null;
        RadioButton[] IvCalibToolUI.rdoCarriers => null;
        RadioButton[] IvCalibToolUI.rdoSuckerRows => null;
        GvCalibPointsDataGridView IvCalibToolUI.dgvCalibPointsListView => null;
        Control IvCalibToolUI.wndVisionSettingsPanel => null; // _imp.wndVisionSettingsPanel;

        Button IvCalibToolUI.btnRunAutoFetch => null;
        Button IvCalibToolUI.btnBuildCalib => null;
        Button IvCalibToolUI.btnCancel => null;
        Button IvCalibToolUI.btnOK => null;
        #endregion
    }
}
