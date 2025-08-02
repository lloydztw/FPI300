using JetEazy;
using JetEazy.BasicSpace;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Traveller106;
using Traveller106.FormSpace;
using VsCommon.ControlSpace.MachineSpace;

namespace TravellerMINIX6.UISpace.CtrlSpace
{
    public partial class MiniCtrl : UserControl
    {

        VersionEnum VERSION;
        OptionEnum OPTION;
        JzTransparentPanel tpnlCover;
        JzTimes myTime;

        Label lblAXIS;
        Label lblTestPoint;
        Label lblIO;

        Label lblVacc;
        Label lblByPassDoor;

        MiniX6MachineClass MACHINE;

        public MiniCtrl()
        {
            InitializeComponent();
        }

        public void Initial(VersionEnum version, OptionEnum option, MiniX6MachineClass machine)
        {
            VERSION = version;
            OPTION = option;
            MACHINE = machine;

            //lblVacc = label4;
            lblIO = label3;
            //lblLIGHT = label1;
            lblAXIS = label2;
            //lblCamDpiSetup = label1;
            //lblTestPoint = label1;
            //lblTestPoint.Visible = false;
            //numericUpDown1.Visible = false;
            lblByPassDoor = label1;

            tpnlCover = new JzTransparentPanel();
            tpnlCover.BackColor = System.Drawing.Color.Transparent;
            tpnlCover.Location = new System.Drawing.Point(6, 30);
            tpnlCover.Name = "panel1";
            tpnlCover.Size = this.Size;
            tpnlCover.TabIndex = 0;
            this.Controls.Add(tpnlCover);
            tpnlCover.BringToFront();

            //lblLIGHT.DoubleClick += LblLIGHT_DoubleClick;
            lblAXIS.DoubleClick += LblAXIS_DoubleClick;
            //lblCamDpiSetup.DoubleClick += LblCamDpiSetup_DoubleClick;
            //lblCamDpiSetup.Visible = false;
            lblIO.DoubleClick += LblIO_DoubleClick;
            //btnByPassDoor.Click += BtnByPassDoor_Click;
            lblByPassDoor.DoubleClick += LblByPassDoor_DoubleClick;

            //lblTestPoint.DoubleClick += LblTestPoint_DoubleClick;
            //lblIO.Visible = false;

            myTime = new JzTimes();
            myTime.Cut();

            SetEnable(false);
        }

        private void LblByPassDoor_DoubleClick(object sender, EventArgs e)
        {
            MACHINE.PLCIO.BypassDoor = !MACHINE.PLCIO.BypassDoor;
        }

        frmIO mIOForm = null;
        private void LblIO_DoubleClick(object sender, EventArgs e)
        {
            if (!INI.Instance.IsOpenIOWindows)
            {
                INI.Instance.IsOpenIOWindows = true;
                mIOForm = new frmIO();
                mIOForm.TopMost = true;
                mIOForm.Show();
            }
        }

        private void LblTestPoint_DoubleClick(object sender, EventArgs e)
        {
            //string add = "0:QB" + (numericUpDown1.Value).ToString("0000.0");
            //bool ison = MACHINE.PLCIO.GetQXQB(add);
            //lblTestPoint.BackColor = (ison ? Color.Green : Control.DefaultBackColor);
        }

        frmMotor mMotorFrom = null;

        private void LblAXIS_DoubleClick(object sender, EventArgs e)
        {
            if (!INI.Instance.IsOpenMotorWindows)
            {
                //OnTrigger(ActionEnum.ACT_MOTOR_SETUP, "");

                //MACHINE.SetNormalTemp(true);

                INI.Instance.IsOpenMotorWindows = true;
                //MACHINE.PLCReadCmdNormalTemp(true);
                //System.Threading.Thread.Sleep(500);
                mMotorFrom = new frmMotor();
                mMotorFrom.Show();
            }
        }

        public void SetEnable(bool isendable)
        {
            tpnlCover.Visible = !isendable;

            Color fillcolor = SystemColors.Control;

            if (!isendable)
                fillcolor = Color.Silver;
        }

        public void SetEnable()
        {
            bool isenable = !tpnlCover.Visible;
            SetEnable(isenable);
            this.Invalidate();
        }

        public void Tick()
        {
            if (myTime.msDuriation > 100)
            {
                myTime.Cut();

                lblByPassDoor.BackColor = (MACHINE.PLCIO.BypassDoor ? Color.Red : Color.Green);
                lblByPassDoor.Text = (MACHINE.PLCIO.BypassDoor ? "門禁屏蔽" : "門禁打開");
            }
            //lblVacc.BackColor = (MACHINE.PLCIO.ADR_ISVACC ? Color.Green : Color.Black);
        }
    }
}
