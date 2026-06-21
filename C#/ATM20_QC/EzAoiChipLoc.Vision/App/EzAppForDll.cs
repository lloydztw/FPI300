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
using EzAoiChipLocQC.Ctrl;
using EzAoiChipLocQC.Gui;
using System.Drawing;
using System.Windows.Forms;


#region TEMPLATES
using MajorClientPanelClassT = EzAoiChipLocQC.Gui.Panels.GvMajorClientPanel;
using ProductionPanelClassT = EzAoiChipLocQC.Gui.Panels.GvProductionPanel;
using SetupPanelClassT = EzAoiChipLocQC.Gui.Panels.GvDummySetupPanel;
using RecipeClassT = EzAoiChipLocQC.Model.JxQcRecipe;
using AppSettingsClassT = EzAoiChipLocQC.JxAppSettings;
using RESOURCES = EzAoiChipLocQC.Properties.Resources;
using EzComm;
using System;
#endregion


namespace EzAoiChipLocQC
{
    public class EzAppForDll : AwFramework.AppBase<RecipeClassT>
    {
        #region SINGLETON
        static EzAppForDll _singleton = null;
        #endregion

        #region PRIVATE_DATA
        EzQcChipMatchCtrl _matchCtrl;
        #endregion

        public static EzAppForDll Instance
        {
            get
            {
                if (_singleton == null)
                    _singleton = new EzAppForDll();
                return _singleton;
            }
        }

        protected EzAppForDll()
        {
            // 設定 路徑
            AppPath = Global.APP_PATH;

            // 綁定 AppSettings
            ConfigAppSettings<AppSettingsClassT>();

            // 綁定 Splash 啟動畫面
            ConfigSplash<FormSplash>();
        }

        internal EzQcChipMatchCtrl MatchCtrl
        {
            get => _matchCtrl;
        }

        /// <summary>
        /// (1) 建構 Model 與其他硬體裝置, 如 Cameras, PLC IO, Motors 等等
        /// </summary>
        protected override void OnBuild_CustomizedModel(Form frmMain)
        {
            var appSettings = this.appSettings as AppSettingsClassT;

            //(1) Machine
            var machine = Global.Machine;
            string ip = appSettings.PlcIpAddress.Address.Value;
            int port = (int)appSettings.PlcIpAddress.Port.Value;
            var ipSettings = new EzTcpIpSettings(ip, port);
            ipSettings.IsSim = Global.IsSim;
            machine.Init(ipSettings);

            //(2) Model
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
            frmMain.picLogo.BackgroundImage = RESOURCES.App_logo;
            frmMain.picLogo.BackColor = Color.FromArgb(72, Color.Black);
            frmMain.wndLogoPanel.BackgroundImage = frmMain.wndOpStatusBar.BackgroundImage;
            
            // Display Configuration
            frmMain.wndTitlePanel.Visible = false;              // 隱藏 原有的 TitleBar
            frmMain.wndClientStatusBar.Visible = false;         // 隱藏 原有的 StatusBar
            //frmMain.optOpModesPanelVisible = false;           // 隱藏 OpMode
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
            EzRcpContraintCtrl.Instance.ConstraintRcp(panel);
            return panel;
        }

        /// <summary>
        /// (4.3) 調整 SetupPanel (不顯示)
        /// </summary>
        protected override IView OnCreate_OpPanelOfSetup(Form frmMain)
        {
            return base.OnCreate_OpPanelOfSetup(frmMain);
            //var panel = new SetupPanelClassT();
            //return panel;
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
            var funcButtonsPanel = wndProductionPanel?.FuncButtonsPanel;
            var lblPassFail = wndProductionPanel?.lblPassFail;

            // 取得 客體視窗 (位於 主要客戶區 視窗內)
            var wndMajorClientPanel = awMain.ClientDocker.FindPanel<MajorClientPanelClassT>();
            var camPanel = wndMajorClientPanel.CameraPanel;
            var matchCtrl = new EzQcChipMatchCtrl(camPanel, funcButtonsPanel, lblPassFail, base.recipesMgrCtrl);

            matchCtrl.PostInit();
            _matchCtrl = matchCtrl;

            // QC Tray Ctrl
            var qcTrayView = wndMajorClientPanel.QcTrayView;
            new EzQcTrayCtrl(qcTrayView, base.recipesMgrCtrl);

            // Random SIM IO
            var sim = new EzIoSimulation();
            var ioView = wndProductionPanel.IoViewer;
            sim.Init(frmMain, ioView);

            // 主視窗 關閉 事件
            awMain.FormClosed += (s, e) => CleanUp();
        }

        /// <summary>
        /// (6) AwFramwork 建構程序已完成, App 即將開始運行.
        /// </summary>
        protected override void OnApp_Start(Form frmMain)
        {
            // 加掛 額外的客製化啟始程序
            EzRcpContraintCtrl.Instance.Constraint(frmMain);

            //// 開始自動 scan PLC
            //Global.Machine?.PLC?.AutoScan?.Start();


        }

        private void CleanUp()
        {
            Global.Dispose();
            _singleton = null;
            _matchCtrl = null;
        }
    }
}
