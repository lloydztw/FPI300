using Common;
using JetEazy.Interface;
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
using VsCommon.ControlSpace;
using VsCommon.ControlSpace.MachineSpace;

namespace NeedleX.FormSpace
{
    public partial class frmHandle : Form
    {
        MotionTouchPanelUIClass motionTouchPanel = null;
        Timer mTimer = null;
        int AxisCount
        {
            get
            {
                return MACHINE.PLCMOTIONCollection.Length;
            }
        }
        protected IAxis GetAxis(int axisID)
        {
            return ((NeedleMachineClass)MACHINECollection.MACHINE).PLCMOTIONCollection[axisID];
        }
        protected MachineCollectionClass MACHINECollection
        {
            get
            {
                return Universal.MACHINECollection;
            }
        }
        protected MiniX6MachineClass MACHINE
        {
            get { return (MiniX6MachineClass)Universal.MACHINECollection.MACHINE; }
        }

        private static frmHandle _instance = null;
        public static frmHandle Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new frmHandle();

                return _instance;
            }
        }

        protected frmHandle()
        {
            InitializeComponent();

            this.Load += FrmHandle_Load;
            this.FormClosed += FrmHandle_FormClosed;
        }

        private void FrmHandle_FormClosed(object sender, FormClosedEventArgs e)
        {
            mTimer.Enabled = false;
            System.Threading.Thread.Sleep(100);
            mTimer = null;
            _instance = null;
        }

        private void FrmHandle_Load(object sender, EventArgs e)
        {
            this.Text = "轴控手动界面";

            motionTouchPanel = new MotionTouchPanelUIClass(vsHandleMotorUI1);
            cboAxisList.Items.Clear();

            int i = 0;
            while (i < AxisCount)
            {
                var axis = MACHINE.PLCMOTIONCollection[i];
                cboAxisList.Items.Add(i.ToString() + "-" + axis.MOTIONNAME);

                i++;
            }

            if (cboAxisList.Items.Count > 0)
            {
                cboAxisList.SelectedIndex = 0;
                motionTouchPanel.Initial(MACHINE.PLCMOTIONCollection[0], ischinese: true);
            }
               
            cboAxisList.SelectedIndexChanged += CboAxisList_SelectedIndexChanged;

            mTimer = new Timer();
            mTimer.Interval = 50;
            mTimer.Enabled = true;
            mTimer.Tick += MTimer_Tick;
        }

        private void MTimer_Tick(object sender, EventArgs e)
        {
            if (cboAxisList.Items.Count <= 0)
                return;
            motionTouchPanel.Tick();
        }

        private void CboAxisList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboAxisList.Items.Count <= 0)
                return;
            int index = cboAxisList.SelectedIndex;
            motionTouchPanel.Initial(MACHINE.PLCMOTIONCollection[index], ischinese: true);
        }
    }
}
