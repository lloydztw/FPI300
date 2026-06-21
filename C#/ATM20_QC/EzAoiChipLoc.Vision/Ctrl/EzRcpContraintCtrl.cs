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
using EzAoiChipLocQC.Gui;
using EzAoiChipLocQC.Gui.Panels;
using LeTian.JxRecipesTool;
using LeTian.JxRecipesTool.Ctrl;
using LeTian.JxRecipesTool.Gui;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using RecipeClassT = EzAoiChipLocQC.Model.JxQcRecipe;


namespace EzAoiChipLocQC.Ctrl
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
        //IvFuncButtonsPanel _funcButtonsPanel => _wndProductionPanel?.FuncButtonsPanel;
        IvRecipeBriefView _rcpBriefView => _wndProductionPanel?.RecipeBriefView;
        //ComboBox _cboRecipeNames => _rcpBriefView?.cboRecipeNames;
        //Control _wndRecipeInfo => _rcpBriefView?.lblInfo;
        #endregion

        public static readonly EzRcpContraintCtrl Instance = new EzRcpContraintCtrl();
        
        public bool IsContraintEnabled = true;
        public void Constraint(Form frmMain)
        {
            _frmMain = frmMain;
            var app = EzAppForDll.Instance;
            _recipesMgr = app.recipesMgrCtrl;
            _recipesMgr.OnRecipeSelectionChanged += _recipesMgr_OnRecipeSelectionChanged;

            _frmAwMain.optOpModesPanelVisible = !IsContraintEnabled;

            _frmMain.BeginInvoke(new Action(() =>
            {
                update_recipe_info(_recipesMgr.ActiveRecipe);
                if (IsContraintEnabled)
                {
                    app.opModesCtrl.OpMode = "Recipe";
                }
            }));
        }
        public void ConstraintRcp(IView rcpPanel)
        {
            if (IsContraintEnabled)
            {
                adjust_recipe_panel(rcpPanel);
            }
        }

        #region EVENT_HANDLERS
        private void _recipesMgr_OnRecipeSelectionChanged(object sender, EventArgs e)
        {
            update_recipe_info(_recipesMgr?.ActiveRecipe);
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
        void change_op_button_text()
        {
            var btn = _frmAwMain.GetOpModeButton("Production");
            if (btn != null)
            {
                btn.Text = "調適模式";
            }
        }
        #endregion

        /// <summary>
        /// 外部指定 Recipe
        /// </summary>
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

            //_frmAwMain?.BeginInvoke(new Action(() =>
            //{
            //    update_recipe_info(activeRecipe);
            //}));
            
            clean_garbages();

            return name;
        }
        public void CleanGarbages()
        {
            clean_garbages();
        }

        #region PRIVATE_GUI_FUNCTIONS
        void update_recipe_info(object rcp)
        {
            #region SAFE_INVOKE
            if (_frmMain != null)
            {
                if (_frmMain.InvokeRequired)
                {
                    _frmMain.BeginInvoke(new Action(() => update_recipe_info(rcp)));
                    return;
                }
            }
            else
            {
                return;
            }
            #endregion

            var view = this._rcpBriefView;
            if (view == null)
                return;

            var recipe = rcp as RecipeClassT;
            var lblInfo = view?.lblInfo;
            var cboRecipeNames = view?.cboRecipeNames;
            var picThumbnail = view?.picThumbnail;

            string info = "";
            if (recipe != null)
            {
                int W = recipe.TrayDimSettings.FovWidth;
                int H = recipe.TrayDimSettings.FovHeight;
                int rows = recipe.TrayDimSettings.FullRows;
                int cols = recipe.TrayDimSettings.FullCols;
                info += $"目標圖形: {W} x {H}";
                info += $"\n目標格點: {rows} x {cols}";
            }

            setEnable(cboRecipeNames, !IsContraintEnabled);
            setText(lblInfo, info);

            //if (BypassConstraint)
            //{
            //    setText(lblInfo, "參數");
            //}
            //else
            //{
            //    setEnable(cboRecipeNames, false);
            //    setText(lblInfo, "參數 = " + recipe?.Name);
            //}

            if (picThumbnail != null)
            {
                var goldenImage = (Image)recipe?.VisionSettings.Match.GoldenBmp.Value?.Clone();
                picThumbnail.BackgroundImage?.Dispose();
                picThumbnail.BackgroundImage = goldenImage;
                picThumbnail.Refresh();
            }
        }
        void setVisible(Control c, bool visible)
        {
            if (c != null)
                c.Visible = visible;
        }
        void setEnable(Control c, bool enable)
        {
            if (c != null)
                c.Enabled = enable;
        }
        void setText(Control c, string text)
        {
            if (c != null)
            {
                c.Text = text;
                c.Refresh();
            }
        }
        #endregion

        #region PRIVATE_GC_FUNCIONS
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
        }
        #endregion
    }
}
