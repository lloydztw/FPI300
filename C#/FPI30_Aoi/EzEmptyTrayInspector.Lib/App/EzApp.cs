#region AUTHOR
/*
 * 
 * Copyright (c) 2024 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2024-09-25 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System;
using LeTian.JxRecipesTool.Gui;
using AwFramework.Util;
using AwFramework;
using AwFramework.Gui;
using EzAoiEmptyTrayInspector.Ctrl;
using EzAoiEmptyTrayInspector.Gui;
using System.Drawing;
using System.Windows.Forms;
using EzAoiEmptyTrayInspector;

#region TEMPLATES
// 主要客戶區 目前有兩個 GUI Class 可供 編譯時期 選用
// (1) GvDualMatchLRView
// (2) GvDualMatchTabView
//using MajorClientPanelClassT = EzDualMatch.Gui.Panels.GvDualMatchTabView;
using MajorClientPanelClassT = EzAoiEmptyTrayInspector.Gui.Panels.GvSingleMatchViewPanel;
using ProductionPanelClassT = EzAoiEmptyTrayInspector.Gui.Panels.GvProductionPanel;
using RecipeClassT = EzAoiEmptyTrayInspector.Model.JxAoiRecipe;
using AppSettingsClassT = EzAoiEmptyTrayInspector.JxAppSettings;
using RESOURCES = EzAoiEmptyTrayInspector.Properties.Resources;
#endregion


namespace EzAoiEmptyTrayInspector
{
    public class EzApp : AwFramework.AppBase<RecipeClassT>
    {
        #region SINGLETON
        static EzApp _singleton = null;
        #endregion

        public static EzApp Instance
        {
            get
            {
                if (_singleton == null)
                    _singleton = new EzApp();
                return _singleton;
            }
        }
        protected EzApp()
        {
            // 設定 路徑
            AppPath = Global.APP_PATH;

            // 綁定 AppSettings
            ConfigAppSettings<AppSettingsClassT>();

            // 綁定 Splash 啟動畫面
            ConfigSplash<FormSplash>();
        }

        /// <summary>
        /// (1) 建構 Model 與其他硬體裝置, 如 Cameras, PLC IO, Motors 等等
        /// </summary>
        protected override void OnBuild_CustomizedModel(Form frmMain)
        {
            var model = Global.AoiModel;
            var recipe = base.recipesMgrCtrl?.ActiveRecipe as RecipeClassT;
            model.SetRecipe(recipe);
        }

        /// <summary>
        /// (2) 調整 主視窗 (FormAwMain) 布局
        /// </summary>
        protected override void OnConfig_GuiMainForm(FormAwMain frmMain)
        {
            // 調整 FormAwMain
            base.OnConfig_GuiMainForm(frmMain);

            // 視窗標題
            frmMain.Text = Global.TITLE;

            // 設定 Icon
            frmMain.Icon = RESOURCES.icon33;

            // 設定 LOGO 
            frmMain.picLogo.BackgroundImage = RESOURCES.JetEazy_logo;
            frmMain.picLogo.BackColor = Color.FromArgb(72, Color.Black);
            frmMain.wndLogoPanel.BackgroundImage = frmMain.wndOpStatusBar.BackgroundImage;
            
            // Display Configuration
            frmMain.wndTitlePanel.Visible = false;          // 隱藏 原有的 TitleBar
            frmMain.wndClientStatusBar.Visible = false;     // 隱藏 原有的 StatusBar
            //frmMain.optOpModesPanelVisible = false;         // 隱藏 OpMode
        }

        /// <summary>
        /// (3) 建構 主區視窗 (MajorClientArea) 元件
        /// </summary>
        protected override IView OnCreate_MajorClientPanel(Form frmMain)
        {
            var panel = new MajorClientPanelClassT();
            return panel;
        }

        /// <summary>
        /// (4.1) 建構/動態生成 跑線作業 (Production) 視窗元件
        /// </summary>
        protected override IView OnCreate_OpPanelOfProduction(Form frmMain)
        {
            var panel = new ProductionPanelClassT();
            return panel;
        }

        /// <summary>
        /// (4.2) 調整 Recipe Editor
        /// </summary>
        protected override IView OnCreate_OpPanelOfRecipe(Form frmMain)
        {
            var panel = base.OnCreate_OpPanelOfRecipe(frmMain);
            adjust_recipe_panel(panel);
            return panel;
        }

        /// <summary>
        /// (4.3) 調整 SetupPanel (不顯示)
        /// </summary>
        protected override IView OnCreate_OpPanelOfSetup(Form frmMain)
        {
            //return base.OnCreate_OpPanelOfSetup(frmMain);
            frmAwMain.BeginInvoke(new Action(() =>
            {
                frmAwMain.GetOpModeButton("Production").PerformClick();
            }));
            return null;
        }

        /// <summary>
        /// (5) 建構 EzDualMatch專案 的 主控模塊 (model-view-control 的 control)
        /// </summary>
        protected override void OnBuild_CustomizedCtrl(Form frmMain, out int splashDelay)
        {
            // 啟動圖片於 1000 ms 後自動關閉
            splashDelay = 1000;

            // 在此直接 return 可以只顯示 GUI, 以方便 DEBUG
            // return;
            make_one_default_recipe();

            var awMain = frmMain as FormAwMain;

            // 取得 試跑按鈕 (位於 跑線作業 視窗內)
            var wndProductionPanel = awMain.OpDocker.FindPanel<ProductionPanelClassT>();
            var funcButtonsPanel = wndProductionPanel?.FuncButtonsPanel;
            Control lblPassFail = wndProductionPanel?.lblPassFail;

            // 取得 雙巨圖 視窗 (位於 主要客戶區 視窗內)
            //var wndDualImagePanel = awMain.ClientDocker.FindPanel<MajorClientPanelClassT>();
            //var matchViews = wndDualImagePanel.MatchViews;
            //// Mouse Move, ZoomIn, ZoomOut 同步控件
            //var syncBox = wndDualImagePanel.SyncBox;
            // 雙巨圖 各別的 MatchView 與 MatchCtrl
            //int sideId = 0;
            //foreach (var matchView in matchViews)
            //{
            //    var matchCtrl = new EzMatchCtrl(sideId, matchView, base.recipesMgrCtrl, btnRunAll);
            //    matchCtrl.PostInit();
            //    matchCtrl.Attach(syncBox);
            //    sideId++;
            //}

            // 取得 巨圖視窗 (位於 主要客戶區 視窗內)
            var matchView = awMain.ClientDocker.FindPanel<MajorClientPanelClassT>();
            var matchCtrl = new EzMatchCtrl(matchView, funcButtonsPanel, lblPassFail, base.recipesMgrCtrl);

            matchCtrl.PostInit();

            // 主視窗 關閉 事件
            awMain.FormClosed += (s,e) => Global.Dispose();

            // OpMode
            opModesCtrl.OnOpModeChanged += (s, e) =>
            {
                System.Diagnostics.Trace.WriteLine(opModesCtrl.OpMode);
                var opMode = opModesCtrl.OpMode;
                if (opMode == "Production" || opMode == "Recipe")
                    swap_func_buttons_panel();
            };
        }

        /// <summary>
        /// (6) AwFramwork 建構程序已完成, App 即將開始運行.
        /// </summary>
        protected override void OnApp_Start(Form frmMain)
        {
            //Reserved: 可以自行加掛 額外的客製化啟始程序
        }

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

            frmMain.BeginInvoke(new Action<Control>((wnd) =>
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

            //if (addFuncButtonsPanel)
            //{
            //    var panelTop = new GwFuncButtonsPanel();
            //    panelTop.Dock = DockStyle.Fill;
            //    panel.Controls.Add(panelTop);
            //}
        }
        void make_one_default_recipe(string name = null)
        {
            var ctrl = base.recipesMgrCtrl;
            var activeRecipe = ctrl.ActiveRecipe as RecipeClassT;

            if (string.IsNullOrEmpty(name))
                name = "aoi_empty_tray_default";

            if (activeRecipe == null || activeRecipe.Name != name)
            {
                var mgr = ctrl.GetManager();
                var list = mgr.GetRecipeNamesList();
                if (!list.Contains(name))
                {
                    var recipe = mgr.InstanciateRecipe(name);
                    recipe.Name = name;
                    mgr.UpdateRecipe(recipe);
                }
                ctrl.LoadRecipe(name, false);
            }
        }
        void swap_func_buttons_panel()
        {
            //>>> var frmAwMain = _frmOwner as FormAwMain;
            var logoPanel = frmAwMain?.wndLogoPanel;
            if (logoPanel == null)
                return;
            
            //>>> var panel = _funcButtonsPanel?.Window;
            var panelP = frmAwMain.OpDocker.FindPanel<ProductionPanelClassT>();
            var panel = panelP?.FuncButtonsPanel?.Window;
            if (panel != null && logoPanel != null)
            {
                AppUtil.SwapGui(panel, logoPanel.picLogo);
            }
        }
        #endregion
    }
}
