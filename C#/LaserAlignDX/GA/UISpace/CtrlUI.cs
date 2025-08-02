using JetEazy;
using LaserAlignDX.ControlSpace.MachineSpace;
using NeedleX.UISpace.CtrlSpace;
using System.Drawing;
using System.Security.Cryptography;
using System.Windows.Forms;
using Traveller106.ControlSpace.MachineSpace;
using VsCommon.ControlSpace.MachineSpace;

namespace PhotoMachine.UISpace
{
    public partial class CtrlUI : UserControl
    {
        VersionEnum VERSION;
        OptionEnum OPTION;

        LaserAlignDX.UISpace.CtrlSpace.MainX1Ctrl mainX1;
        LaserAlignDX.UISpace.CtrlSpace.MainX2Ctrl mainX2;
        LaserAlignDX.UISpace.CtrlSpace.MainFPIX3Ctrl mainX3;

        public CtrlUI()
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
                            mainX1 = new LaserAlignDX.UISpace.CtrlSpace.MainX1Ctrl();
                            mainX1.Initial(VERSION, OPTION, (MainX1MachineClass)machine);
                            mainX1.Location = new Point(0, 0);
                            this.Controls.Add(mainX1);
                            mainX1.Dock = DockStyle.Fill;
                            break;
                        case OptionEnum.MAIN_FPIX3:
                            mainX3 = new LaserAlignDX.UISpace.CtrlSpace.MainFPIX3Ctrl();
                            mainX3.Initial(VERSION, OPTION, (MainFPIX3MachineClass)machine);
                            mainX3.Location = new Point(0, 0);
                            this.Controls.Add(mainX3);
                            mainX3.Dock = DockStyle.Fill;
                            break;
                    }
                    break;
                case VersionEnum.AOI:
                    switch (OPTION)
                    {
                        case OptionEnum.MAIN_X2:
                            mainX2 = new LaserAlignDX.UISpace.CtrlSpace.MainX2Ctrl();
                            mainX2.Initial(VERSION, OPTION, (MainX2MachineClass)machine);
                            mainX2.Location = new Point(0, 0);
                            this.Controls.Add(mainX2);
                            mainX2.Dock = DockStyle.Fill;
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
        public void MyDispose()
        {
            switch (VERSION)
            {
                case VersionEnum.AOI:
                    switch (OPTION)
                    {
                        case OptionEnum.MAIN_X2:
                            mainX2.MyDispose();
                            break;
                        case OptionEnum.MAIN_FPIX3:
                            mainX3.MyDispose();
                            break;
                    }
                    break;
            }
        }

        public delegate void TriggerHandler(ActionEnum action, string opstr);
        public event TriggerHandler TriggerAction;
        public void OnTrigger(ActionEnum action, string opstr)
        {
            if (TriggerAction != null)
            {
                TriggerAction(action, opstr);
            }
        }

    }
}
