using Traveller106;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Traveller106.FormSpace;
using JetEazy.BasicSpace;
using JetEazy;
using VsCommon.ControlSpace.MachineSpace;

namespace NeedleX.UISpace.CtrlSpace
{
    public partial class NeedleCtrl : UserControl
    {
        VersionEnum VERSION;
        OptionEnum OPTION;
        JzTransparentPanel tpnlCover;
        JzTimes myTime;

        Label lblAXIS;
        Label lblTestPoint;
        Label lblIO;

        Label lblVacc;

        NeedleMachineClass MACHINE;

        public NeedleCtrl()
        {
            InitializeComponent();
        }
        public void Initial(VersionEnum version, OptionEnum option, NeedleMachineClass machine)
        {
            VERSION = version;
            OPTION = option;
            MACHINE = machine;

            lblVacc = label4;
            lblIO = label3;
            //lblLIGHT = label1;
            lblAXIS = label2;
            //lblCamDpiSetup = label1;
            lblTestPoint = label1;
            lblTestPoint.Visible = false;
            numericUpDown1.Visible = false;

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

            lblTestPoint.DoubleClick += LblTestPoint_DoubleClick;
            lblIO.Visible = false;

            myTime = new JzTimes();
            myTime.Cut();

            SetEnable(false);
        }

        frmIO mIOForm = null;
        private void LblIO_DoubleClick(object sender, EventArgs e)
        {
            if (!INI.Instance.IsOpenIOWindows)
            {
                INI.Instance.IsOpenIOWindows = true;
                mIOForm = new frmIO();
                mIOForm.Show();
            }
        }

        private void LblTestPoint_DoubleClick(object sender, EventArgs e)
        {
            string add = "0:QB" + (numericUpDown1.Value).ToString("0000.0");
            bool ison = MACHINE.PLCIO.GetQXQB(add);
            lblTestPoint.BackColor = (ison ? Color.Green : Control.DefaultBackColor);
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
            }
            lblVacc.BackColor = (MACHINE.PLCIO.ADR_ISVACC ? Color.Green : Color.Black);
        }
    }
}
