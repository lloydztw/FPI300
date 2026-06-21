#region AUTHOR
/*
 * 
 * Copyright (c) 2024 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2024-10-25 教學用初稿 (by LeTian Chang)
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
using System.Windows.Forms;
using TestDemo.Ctrl;
using TestDemo.Gui.Panels;
using AppSettingsClassT = EzPizza.App.Settings.JxAppSetings;
using FormSplashClassT = EzDualMatch.Gui.FormSplash_Cyborg;
using RecipeClassT = EzPizza.Model.JxPizzaRecipe;


namespace TestDemo
{
    internal class DemoApp2 : AwFramework.AppBase<RecipeClassT>
    {
        #region NLOG
        // NOTE:
        // 不要做靜態初始化，而是要等到窗體的Load事件時才初始化Logger物件，
        // 且保證該窗體是 【首個使用】NLog 的 Class !!!
        // 這是因為NLog是在首次被使用時，才載入配置文件的。
        static NLog.ILogger _logger = null;
        NLog.ILogger _LOG
        {
            get
            {
                if (_logger == null)
                    _logger = NLog.LogManager.GetCurrentClassLogger();
                return _logger;
            }
        }
        #endregion

        public DemoApp2()
        {
            // 改變 AppName
            AppUtil.AppName = "Demo Cyborgs";

            // 設定 路徑
            AppPath.RootPath = @"D:\paso.log\DempApp2";

            // 綁定 AppSettings
            ConfigAppSettings<AppSettingsClassT>();

            // 綁定 Splash 啟動畫面
            ConfigSplash<FormSplashClassT>();
        }

        protected override void OnConfig_GuiMainForm(FormAwMain frmAwMain)
        {
            // 重複使用基本設定
            base.OnConfig_GuiMainForm(frmAwMain);
            // 設定 新LOGO
            frmAwMain.picLogo.BackgroundImage = Properties.Resources.logo;
        }
        protected override IView OnCreate_MajorClientPanel(Form frmMain)
        {
            // 創建你的主客區視窗面板
            var panel = new GvDemoMajorClientPanel();
            return panel;
        }
        protected override IView OnCreate_OpPanelOfProduction(Form frmMain)
        {
            // 創建你的跑線作業視窗面板
            var panel = new GvProductionPanel();
            return panel;
        }
        protected override IView OnCreate_OpPanelOfSetup(Form frmMain)
        {
            // 加掛開啟馬達控制頁
            var panelView = (IvSysSettingView)base.OnCreate_OpPanelOfSetup(frmMain);
            panelView.OnRequestOpenMotorPanel += PanelView_OnRequestOpenMotorPanel;
            return panelView;
        }
        protected override void OnBuild_CustomizedCtrl(Form frmMain, out int splashDelay)
        {
            //--------------------------------------------------------
            // AwFramework 執行至此已經提供 默認的
            // 三作業模式 Production, Recipe, Setup, 與 Login 等框架
            // 基本管理機制
            //--------------------------------------------------------

            _LOG.Info("建立主控模塊 ...");
            _LOG.Trace("這時候可以使用 NLog 了!");

            // 此處只簡單 演示 啟動畫面的延遲
            // 個別專案需在此建立自己的主控模塊
            // (model-view-control 的 control)
            var ctrl = new DemoCtrl();
            var wndProductPanel = frmAwMain.OpDocker.FindPanel<GvProductionPanel>();
            ctrl.Attach(wndProductPanel.btnSwap);

            // 演示延遲
            splashDelay = 3000;
            _LOG.Warn("啟動畫面延遲 {0}ms 後關閉", splashDelay);
        }
        protected override void OnApp_Start(Form frmMain)
        {
            _LOG.Trace("AwFramework 已經完成建置程序 !");
            _LOG.Warn("登入用戶名稱是: {0}", loginCtrl.CurrentUser.Name);

            bool isRight = frmAwMain.optOpDockAtRightSide;
            _LOG.Debug("OP 作業面板 目前為 '{0}'", isRight ? "右駕" : "左駕");

            if (!isRight)
            {
                isRight = !isRight;
                frmAwMain.optOpDockAtRightSide = isRight;
                _LOG.Debug("OP 作業面板 改為 '{0}'", isRight ? "右駕" : "左駕");
            }

            _LOG.Error("Don't worry, 這只是演示異常的 log.");
            _LOG.Info("APP 準備開始囉 !");
        }
        
        private void PanelView_OnRequestOpenMotorPanel(object sender, System.EventArgs e)
        {
            MessageBox.Show("此處可擴充: 用來開啟馬達控制頁!");
        }
    }
}
