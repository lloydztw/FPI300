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

using AwFramework;
using AwFramework.Gui;
using EzEmptyTrayInspector.Ctrl;
using EzEmptyTrayInspector.Gui;
using System.Drawing;
using System.Windows.Forms;
using EzEmptyTrayInspector;
using RESOURCES = EzEmptyTrayInspector.Properties.Resources;

#region TEMPLATES
// 主要客戶區 目前有兩個 GUI Class 可供 編譯時期 選用
// (1) GvDualMatchLRView
// (2) GvDualMatchTabView
//using MajorClientPanelClassT = EzDualMatch.Gui.Panels.GvDualMatchTabView;
using MajorClientPanelClassT = EzEmptyTrayInspector.Gui.Panels.GvSingleMatchViewPanel;
using ProductionPanelClassT = EzEmptyTrayInspector.Gui.Panels.GvProductionPanel;
using RecipeClassT = EzEmptyTrayInspector.Model.JxAoiRecipe;
using AppSettingsClassT = EzEmptyTrayInspector.JxAppSettings;
#endregion


namespace FPI30_AOI
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
            frmMain.optOpModesPanelVisible = false;         // 隱藏 OpMode
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
        /// (5) 建構 EzDualMatch專案 的 主控模塊 (model-view-control 的 control)
        /// </summary>
        protected override void OnBuild_CustomizedCtrl(Form frmMain, out int splashDelay)
        {
            // 啟動圖片於 1000 ms 後自動關閉
            splashDelay = 1000;

            // 在此直接 return 可以只顯示 GUI, 以方便 DEBUG
            // return;

            var awMain = frmMain as FormAwMain;

            // 取得 試跑按鈕 (位於 跑線作業 視窗內)
            var wndProductionPanel = awMain.OpDocker.FindPanel<ProductionPanelClassT>();
            Button btnRunAll = wndProductionPanel?.btnTryRun;

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
            var view = awMain.ClientDocker.FindPanel<MajorClientPanelClassT>();
            var matchCtrl = new EzMatchCtrl(0, view, base.recipesMgrCtrl, btnRunAll);
            matchCtrl.PostInit();

            // 主視窗 關閉 事件
            awMain.FormClosed += (s,e) => Global.Dispose();
        }

        /// <summary>
        /// (6) AwFramwork 建構程序已完成, App 即將開始運行.
        /// </summary>
        protected override void OnApp_Start(Form frmMain)
        {
            //Reserved: 可以自行加掛 額外的客製化啟始程序
        }
    }
}
