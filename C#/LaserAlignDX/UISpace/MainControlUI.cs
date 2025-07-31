using JetEazy;
using NeedleX.UISpace.MainSpace;
using System.Drawing;
using System.Windows.Forms;
using VsCommon.ControlSpace.MachineSpace;

namespace Eazy_Project_III.UISpace
{
    public partial class MainControlUI : UserControl
    {
        VersionEnum VERSION;
        OptionEnum OPTION;

        LaserAlignDX.UISpace.MainSpace.MainX1UI mainX1;
        LaserAlignDX.UISpace.MainSpace.MainX2UI mainX2;
        LaserAlignDX.UISpace.MainSpace.MainX3UI mainX3;
        public MainControlUI()
        {
            InitializeComponent();
            InitialInternal();
        }

        void InitialInternal()
        {

        }

        public void Initial(VersionEnum version, OptionEnum option, GeoMachineClass machine)
        {
            VERSION = version;
            OPTION = option;

            switch (VERSION)
            {
                case VersionEnum.LASER:
                    switch (OPTION)
                    {
                        case OptionEnum.MAIN_X1:
                            mainX1 = new LaserAlignDX.UISpace.MainSpace.MainX1UI();
                            mainX1.Init();
                            mainX1.Location = new Point(0, 0);
                            this.Controls.Add(mainX1);
                            mainX1.Dock = DockStyle.Fill;
                            mainX1.OnChangeState += OnFireChangeState;
                            break;
                        case OptionEnum.MAIN_FPIX3:
                            mainX3 = new LaserAlignDX.UISpace.MainSpace.MainX3UI();
                            mainX3.Init();
                            mainX3.Location = new Point(0, 0);
                            this.Controls.Add(mainX3);
                            mainX3.Dock = DockStyle.Fill;
                            mainX3.OnChangeState += OnFireChangeState;
                            break;
                    }
                    break;
                case VersionEnum.AOI:
                    switch (OPTION)
                    {
                        case OptionEnum.MAIN_X2:
                            mainX2 = new LaserAlignDX.UISpace.MainSpace.MainX2UI();
                            mainX2.Init();
                            mainX2.Location = new Point(0, 0);
                            this.Controls.Add(mainX2);
                            mainX2.Dock = DockStyle.Fill;
                            mainX2.OnChangeState += OnFireChangeState;
                            break;
                    }
                    break;
            }
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
            switch (VERSION)
            {
                case VersionEnum.PROJECT:
                    switch (OPTION)
                    {
                        case OptionEnum.DISPENSING:
                            //mainX3.Close();
                            break;
                        case OptionEnum.DISPENSINGX1:
                            //mainX1.Close();
                            break;
                    }
                    break;
            }
        }
        public void SetEnable(bool isenable)
        {
            switch (VERSION)
            {
                case VersionEnum.LASER:
                    switch (OPTION)
                    {
                        case OptionEnum.MAIN_X1:
                            mainX1.SetEnable(isenable);
                            break;
                        case OptionEnum.MAIN_FPIX3:
                            mainX3.SetEnable(isenable);
                            break;
                    }
                    break;
                case VersionEnum.AOI:
                    switch (OPTION)
                    {
                        case OptionEnum.MAIN_X2:
                            mainX2.SetEnable(isenable);
                            break;
                    }
                    break;
            }
        }
        public void SetEnableState(bool isenable)
        {
            switch (VERSION)
            {
                case VersionEnum.LASER:
                    switch (OPTION)
                    {
                        case OptionEnum.MAIN_X1:
                            mainX1.SetEnableState(isenable);
                            break;
                        case OptionEnum.MAIN_FPIX3:
                            mainX3.SetEnableState(isenable);
                            break;
                    }
                    break;
                case VersionEnum.AOI:
                    switch (OPTION)
                    {
                        case OptionEnum.MAIN_X2:
                            mainX2.SetEnableState(isenable);
                            break;
                    }
                    break;
            }
        }
        public void ChangeRecipe()
        {
            switch (VERSION)
            {
                case VersionEnum.AOI:
                    switch (OPTION)
                    {
                        case OptionEnum.MAIN_X2:
                            mainX2.MappingInit();
                            break;
                    }
                    break;
            }
        }

        public void Tick()
        {
            switch (VERSION)
            {
                case VersionEnum.LASER:
                    switch (OPTION)
                    {
                        case OptionEnum.MAIN_X1:
                            mainX1.Tick();
                            break;
                        case OptionEnum.MAIN_FPIX3:
                            mainX3.Tick();
                            break;
                    }
                    break;
                case VersionEnum.AOI:
                    switch (OPTION)
                    {
                        case OptionEnum.MAIN_X2:
                            mainX2.Tick();
                            break;
                    }
                    break;
            }
        }
        public delegate void ChangeStateHandler(MainS1State status, object tag = null);
        public event ChangeStateHandler OnChangeState;
        protected void FireChangeState(MainS1State status, object tag = null)
        {
            if (OnChangeState != null)
            {
                OnChangeState(status, tag);
            }
        }

    }
}
