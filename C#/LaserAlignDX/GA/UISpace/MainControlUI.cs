using JetEazy;
using LaserAlignDX.UISpace;
using System.Drawing;
using System.Windows.Forms;
using VsCommon.ControlSpace.MachineSpace;

namespace Eazy_Project_III.UISpace
{
    public partial class MainControlUI : UserControl
    {
        public event ChangeStateHandler OnChangeState;

        //VersionEnum VERSION;
        //OptionEnum OPTION;
        //LaserAlignDX.UISpace.MainSpace.MainX1UI mainX1;
        //LaserAlignDX.UISpace.MainSpace.MainX2UI mainX2;
        //LaserAlignDX.UISpace.MainSpace.MainX3UI mainX3;

        IMainUI mainUI;

        public MainControlUI()
        {
            InitializeComponent();
        }

        public void Initial(VersionEnum version, OptionEnum option, GeoMachineClass machine)
        {
            mainUI = createMainUI(version, option, machine);
            if (mainUI != null)
            {
                mainUI.Init();
                mainUI.Window.Location = new Point(0, 0);
                this.Controls.Add(mainUI.Window);
                mainUI.Window.Dock = DockStyle.Fill;
                mainUI.OnChangeState += OnFireChangeState;
            }
        }

        IMainUI createMainUI(VersionEnum version, OptionEnum option, GeoMachineClass machine)
        {
            // 抽出共通的 Interface IMainUI ,
            // switch case 集中在此寫一次就好.
            // 不要散亂在各處

            switch (version)
            {
                case VersionEnum.LASER:
                    switch (option)
                    {
                        //case OptionEnum.MAIN_X1:
                        //    return new LaserAlignDX.UISpace.MainSpace.MainX1UI();
                        case OptionEnum.MAIN_FPIX3:
                            return new LaserAlignDX.UISpace.MainSpace.MainX3UI();
                    }
                    break;
                case VersionEnum.AOI:
                    //switch (option)
                    //{
                    //    case OptionEnum.MAIN_X2:
                    //        return new LaserAlignDX.UISpace.MainSpace.MainX2UI();
                    //}
                    break;
            }
            return null;
        }

        private void OnFireChangeState(MainS1State status, object tag = null)
        {
            FireChangeState(status, tag);
        }
        private void UI_OnChangeState(MainS1State status)
        {
            FireChangeState(status);
        }

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

            mainUI?.SetEnable(isenable);
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

            mainUI?.SetEnableState(isenable);
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

            mainUI?.ChangeRecipe();
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

            mainUI?.Tick();
        }

        protected void FireChangeState(MainS1State status, object tag = null)
        {
            if (OnChangeState != null)
            {
                OnChangeState(status, tag);
            }
        }
    }
}
