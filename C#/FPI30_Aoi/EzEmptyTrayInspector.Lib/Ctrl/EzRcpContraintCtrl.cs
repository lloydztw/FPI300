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


using AwFramework;
using AwFramework.Gui;
using AwFramework.Util;
using EzAoiEmptyTrayInspector.Gui;
using EzAoiEmptyTrayInspector.Gui.Panels;
using LeTian.JxRecipesTool;
using LeTian.JxRecipesTool.Ctrl;
using LeTian.JxRecipesTool.Gui;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using RecipeClassT = EzAoiEmptyTrayInspector.Model.JxAoiRecipe;


namespace EzAoiEmptyTrayInspector.Ctrl
{
    /// <summary>
    /// Recipe Editor 限制器, 
    /// 用來限制 一些 AwFramework 標準功能
    /// </summary>
    internal class EzRcpContraintCtrl
    {
        #region PRIVATE_RUNTIME_DATA
        IRecipesMgrCtrl _recipesMgr;
        #endregion

        #region PRIVATE_GUI_LINKS
        Form _frmMain;
        FormAwMain _frmAwMain => _frmMain as FormAwMain;
        GvProductionPanel _wndProductionPanel => _frmAwMain?.OpDocker.FindPanel<GvProductionPanel>();
        IvFuncButtonsPanel _funcButtonsPanel => _wndProductionPanel?.FuncButtonsPanel;
        Label _wndRecipeInfo => _wndProductionPanel?.lblRecipeInfo;
        #endregion

        public static readonly EzRcpContraintCtrl Instance = new EzRcpContraintCtrl();
        
        public bool Bypass = false;
        public void Constraint(Form frmMain)
        {
            _frmMain = frmMain;
            var app = EzAppForDll.Instance;
            _recipesMgr = app.recipesMgrCtrl;
            _recipesMgr.OnRecipeBrowsing += _recipesMgr_OnRecipeBrowsing;
            _recipesMgr.OnRecipeEditting += _recipesMgr_OnRecipeEditting;
        }
        public void ConstraintRcp(IView rcpPanel)
        {
            if (Bypass)
                return;

            adjust_recipe_panel(rcpPanel);
        }

        #region EVENT_HANDLERS
        private void _recipesMgr_OnRecipeEditting(object sender, EventArgs e)
        {
            swap_func_buttons_panel(true);
        }
        private void _recipesMgr_OnRecipeBrowsing(object sender, EventArgs e)
        {
            swap_func_buttons_panel(false);
        }
        #endregion

        #region PRIVATE_RCP_EDITOR_ADJUST_FUNCTIONS
        /// <summary>
        /// 強制只顯示 單一特定 Recipe
        /// </summary>
        /// <param name="panel"></param>
        void adjust_recipe_panel(IView panel)
        {
            Control wndPanel = panel?.Window;
            if (wndPanel == null)
                return;

            _frmMain?.BeginInvoke(new Action<Control>((wnd) =>
            {
                var view = AppUtil.SearchGui<GpRecipesMgrView>(wnd, null);
                adjust_recipe_panel(view, true);
            }), wndPanel);
        }
        void adjust_recipe_panel(GpRecipesMgrView panel, bool hookEventHandler = false)
        {
            if (panel == null)
                return;

            panel.OptShowList = false;
            panel.btnAdd.Visible = false;
            panel.btnCopy.Visible = false;
            panel.btnDelete.Visible = false;
            panel.gwRcpmEditorPanel.txtActiveRecipeName.Enabled = false;

            if (hookEventHandler)
            {
                panel.btnOK.Click += (s, e) => adjust_recipe_panel(panel, false);
                panel.btnCancel.Click += (s, e) => adjust_recipe_panel(panel, false);
                panel.btnModify.Click += (s, e) => adjust_recipe_panel(panel, false);
            }
        }
        #endregion

        #region PRIVATE_SWAP_FUNCTIONS
        void swap_func_buttons_panel(bool toTop)
        {
            var funcPanel = _funcButtonsPanel?.Window;
            var logoPanel = _frmAwMain?.wndLogoPanel;

            if (logoPanel == null || funcPanel == null)
                return;

            if (toTop && funcPanel.Parent == logoPanel)
                return;

            if (!toTop && funcPanel.Parent != logoPanel)
                return;

            _frmAwMain?.BeginInvoke(new Action(() =>
            {
                AppUtil.SwapGui(funcPanel, logoPanel.picLogo);
            }));
        }
        #endregion

        #region PRIVATE_FUNCTIONS
        void update_recipe_info(string name)
        {
            var lblInfo = _wndRecipeInfo;
            if (lblInfo != null)
            {
                if (lblInfo.InvokeRequired)
                {
                    lblInfo.BeginInvoke(new Action<string>((s) =>
                    {
                        update_recipe_info(s);
                    }));
                }
                else
                {
                    lblInfo.Text = "參數 : " + name;
                    lblInfo.Refresh();
                }
            }
        }
        #endregion


        /// <summary>
        /// 指定 Recipe
        /// </summary>
        public string AssignOneRecipe_000(string targetName = null)
        {
            var app = EzAppForDll.Instance;
            var rcpCtrl = app?.recipesMgrCtrl;
            if (rcpCtrl == null)
            {
                if (_runtimeRcpMgr == null)
                {
                    var JB = new JxRecipesMgrBuilder<RecipeClassT>(Global.APP_PATH.RecipePath, ".json");
                    _runtimeRcpMgr = JB.InstanceCtrl();
                    add_to_garbagesCollector(_runtimeRcpMgr);
                }
                rcpCtrl = _runtimeRcpMgr;
            }

            var activeRecipe = rcpCtrl.ActiveRecipe as RecipeClassT;

            bool needToReload = false;
            if (activeRecipe != null)
            {
                if (!string.IsNullOrEmpty(targetName) && targetName != activeRecipe.Name)
                    needToReload = true;
            }
            else
            {
                if (string.IsNullOrEmpty(targetName))
                    targetName = "aoi_empty_tray_default";
                needToReload = true;
            }

            if (needToReload)
            {
                var mgr = rcpCtrl.GetManager();
                var list = mgr.GetRecipeNamesList(true);
                if (!list.Contains(targetName))
                {
                    activeRecipe = mgr.InstanciateRecipe(targetName) as RecipeClassT;
                    activeRecipe.Name = targetName;
                }
                mgr.UpdateRecipe(activeRecipe);
                rcpCtrl.LoadRecipe(targetName, true);
            }

            Global.AoiModel.SetRecipe(activeRecipe);
            var name = activeRecipe?.Name;

            update_recipe_info(name);
            return name;
        }
        public string AssignOneRecipe(string targetName = null)
        {
            var app = EzAppForDll.Instance;
            var rcpCtrl = app?.recipesMgrCtrl;

            if (rcpCtrl == null)
            {
                var JB = new JxRecipesMgrBuilder<RecipeClassT>(Global.APP_PATH.RecipePath, ".json");
                rcpCtrl = JB.InstanceCtrl();
                add_to_garbagesCollector(rcpCtrl);
            }

            var activeRecipe = rcpCtrl.ActiveRecipe as RecipeClassT;

            bool needToReload = false;
            if (activeRecipe != null)
            {
                if (!string.IsNullOrEmpty(targetName) && targetName != activeRecipe.Name)
                    needToReload = true;
            }
            else
            {
                if (string.IsNullOrEmpty(targetName))
                    targetName = "aoi_empty_tray_default";
                needToReload = true;
            }

            if (needToReload)
            {
                var mgr = rcpCtrl.GetManager();
                var list = mgr.GetRecipeNamesList(true);
                if (!list.Contains(targetName))
                {
                    activeRecipe = mgr.InstanciateRecipe(targetName) as RecipeClassT;
                    activeRecipe.Name = targetName;
                }
                mgr.UpdateRecipe(activeRecipe);
                rcpCtrl.LoadRecipe(targetName, true);
            }

            Global.AoiModel.SetRecipe(activeRecipe);
            
            var name = activeRecipe?.Name;
            update_recipe_info(name);

            clean_garbages();
            return name;
        }
        public void CleanGarbages()
        {
            clean_garbages();
        }

        #region PRIVATE_FUNCIONS
        IRecipesMgrCtrl _runtimeRcpMgr = null;
        List<IDisposable> _garbagesCollector;
        void add_to_garbagesCollector(IDisposable obj)
        {
            if (_garbagesCollector == null)
                _garbagesCollector = new List<IDisposable>();
            if (!_garbagesCollector.Contains(obj))
                _garbagesCollector.Add(obj);
        }
        void clean_garbages()
        {
            if (_garbagesCollector != null)
                foreach (var obj in _garbagesCollector)
                    obj?.Dispose();

            _garbagesCollector = null;
            _runtimeRcpMgr = null;
        }
        #endregion
    }
}
