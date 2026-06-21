#region AUTHOR
/*
 * 
 * Copyright (c) 2024 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2024-10-03 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzAoiChipLocQC.Gui;
using EzAoiChipLocQC.Model;
using LeTian.JxRecipesTool.Ctrl;
using System;
using System.Windows.Forms;

namespace EzAoiChipLocQC.Ctrl
{
    internal class EzQcTrayCtrl : BaseUtil
    {
        #region PRIVATE_RECIPE_DATA
        IRecipesMgrCtrl _recipesMgr;
        JxQcRecipe _activeRecipe
        {
            get => _recipesMgr?.ActiveRecipe as JxQcRecipe;
        }
        JxTrayDimSettings _jxTraySettings
        {
            get => _activeRecipe?.TrayDimSettings;
        }
        #endregion

        #region PRIVATE_GUI_MEMBERS
        Form _frmOwner;
        IvQcTrayView _trayView;
        #endregion

        public EzQcTrayCtrl(IvQcTrayView trayView, IRecipesMgrCtrl recipesMgr)
        {
            _frmOwner = trayView.Window.FindForm();
            _trayView = trayView;
            _recipesMgr = recipesMgr;
            init_event_handlers();
        }

        #region EVENT_HANDLERS
        void init_event_handlers()
        {
            // RECIPE_MGR_EVENT_HANDLERS
            _recipesMgr.OnRecipeSelectionChanged += _recipesMgr_OnRecipeSelectionChanged;

            // TrayDimSettings
            if (_jxTraySettings != null)
                _jxTraySettings.OnModified += _trayDimSettings_OnModified;

            _frmOwner.FormClosed += (s, e) => cleanUp();

            _frmOwner.BeginInvoke(new Action(() =>
            {
                _trayView.UpdateSettings(_jxTraySettings);
                _trayView.Window.Refresh();
            }));
        }
        void disconnect_event_handlers()
        {
            // RECIPE_MGR_EVENT_HANDLERS
            _recipesMgr.OnRecipeSelectionChanged -= _recipesMgr_OnRecipeSelectionChanged;

            // TrayDimSettings
            if (_jxTraySettings != null)
                _jxTraySettings.OnModified -= _trayDimSettings_OnModified;
        }
        private void _recipesMgr_OnRecipeSelectionChanged(object sender, EventArgs e)
        {
            _trayView?.UpdateSettings(_jxTraySettings);
        }
        private void _trayDimSettings_OnModified(object sender, EventArgs e)
        {
            _trayView?.UpdateSettings(_jxTraySettings);
        }
        #endregion

        #region PRIVATE_FUNCTIONS
        void cleanUp()
        {
            try
            {
                disconnect_event_handlers();
            }
            catch
            {

            }
        }
        #endregion
    }
}
