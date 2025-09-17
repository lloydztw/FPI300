using JetEazy;
using LaserAlignDX.UISpace;
using System;
using System.Drawing;
using System.Windows.Forms;
using VsCommon.ControlSpace.MachineSpace;

namespace Eazy_Project_III.UISpace
{
    public partial class MainControlUI : UserControl, IMainUI
    {
        public event EventHandler<MainUiStateEventArgs> OnStateChanged;

        ///<summary>
        ///以下元件改由 MainUiFactory 生成
        ///<br/> LaserAlignDX.UISpace.MainSpace.MainX1UI mainX1;
        ///<br/> LaserAlignDX.UISpace.MainSpace.MainX2UI mainX2;
        ///<br/> LaserAlignDX.UISpace.MainSpace.MainX3UI mainX3;
        ///</summary>
        IMainUI _mainUI;

        public MainControlUI()
        {
            InitializeComponent();
        }

        public void Initial(VersionEnum version, OptionEnum option, GeoMachineClass machine)
        {
            //-------------------------------------------------------------------
            // 把所有 MainXxUI 抽出共通 Interface, 並把生成的代碼集中至此,
            // version + option 的 switch case 在此寫一次就好,
            // 不要凌亂的散落各處.
            //-------------------------------------------------------------------

            _mainUI = MainUiFactory.CreateMainUI(version, option, machine);

            if (_mainUI != null)
            {
                _mainUI.Init();
                _mainUI.Window.Location = new Point(0, 0);
                this.Controls.Add(_mainUI.Window);
                _mainUI.Window.Dock = DockStyle.Fill;
                _mainUI.OnStateChanged += (s, e) => FireChangeState(s, e);
            }
        }

        #region IMainUI_實作
        Control IMainUI.Window => this;
        void IMainUI.Init()
        {
        }
        #endregion

        #region NOT_USED_CODE
#if (false)
        private void OnFireChangeState(MainS1State status, object tag = null)
        {
            FireChangeState(this, new MainUIStateChangedEventArgs(status, tag));
        }
        private void UI_OnChangeState(MainS1State status)
        {
            FireChangeState(this, new MainUIStateChangedEventArgs(status, null));
        }
#endif
        #endregion

        public void Close()
        {
            //switch (VERSION)
            //{
            //    case VersionEnum.PROJECT:
            //        switch (OPTION)
            //        {
            //            case OptionEnum.DISPENSING:
            //                //mainX3.Close();
            //                break;
            //            case OptionEnum.DISPENSINGX1:
            //                //mainX1.Close();
            //                break;
            //        }
            //        break;
            //}
        }
        public void SetEnable(bool isenable)
        {
            //switch (VERSION)
            //{
            //    case VersionEnum.LASER:
            //        switch (OPTION)
            //        {
            //            case OptionEnum.MAIN_X1:
            //                mainX1.SetEnable(isenable);
            //                break;
            //            case OptionEnum.MAIN_FPIX3:
            //                mainX3.SetEnable(isenable);
            //                break;
            //        }
            //        break;
            //    case VersionEnum.AOI:
            //        switch (OPTION)
            //        {
            //            case OptionEnum.MAIN_X2:
            //                mainX2.SetEnable(isenable);
            //                break;
            //        }
            //        break;
            //}

            _mainUI?.SetEnable(isenable);
        }
        public void SetEnableState(bool isenable)
        {
            //switch (VERSION)
            //{
            //    case VersionEnum.LASER:
            //        switch (OPTION)
            //        {
            //            case OptionEnum.MAIN_X1:
            //                mainX1.SetEnableState(isenable);
            //                break;
            //            case OptionEnum.MAIN_FPIX3:
            //                mainX3.SetEnableState(isenable);
            //                break;
            //        }
            //        break;
            //    case VersionEnum.AOI:
            //        switch (OPTION)
            //        {
            //            case OptionEnum.MAIN_X2:
            //                mainX2.SetEnableState(isenable);
            //                break;
            //        }
            //        break;
            //}

            _mainUI?.SetEnableState(isenable);
        }
        public void ChangeRecipe()
        {
            //switch (VERSION)
            //{
            //    case VersionEnum.AOI:
            //        switch (OPTION)
            //        {
            //            case OptionEnum.MAIN_X2:
            //                mainX2.MappingInit();
            //                break;
            //        }
            //        break;
            //}

            _mainUI?.ChangeRecipe();
        }
        public void Tick()
        {
            //switch (VERSION)
            //{
            //    case VersionEnum.LASER:
            //        switch (OPTION)
            //        {
            //            case OptionEnum.MAIN_X1:
            //                mainX1.Tick();
            //                break;
            //            case OptionEnum.MAIN_FPIX3:
            //                mainX3.Tick();
            //                break;
            //        }
            //        break;
            //    case VersionEnum.AOI:
            //        switch (OPTION)
            //        {
            //            case OptionEnum.MAIN_X2:
            //                mainX2.Tick();
            //                break;
            //        }
            //        break;
            //}

            _mainUI?.Tick();
        }

        protected void FireChangeState(object sender, MainUiStateEventArgs e)
        {
            OnStateChanged?.Invoke(sender, e);
        }
    }
}
