using Common;
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

namespace LaserAlignDX.GA.FormSpace
{
    public partial class FormMotor : Form
    {

        const int AXIS_COUNT = 6;
        VsTouchMotorUI[] VSAXISUI = new VsTouchMotorUI[AXIS_COUNT];
        MotionTouchPanelUIClass[] AXISUI = new MotionTouchPanelUIClass[AXIS_COUNT];

        Timer mMotorTimer = null;
        MachineCollectionClass MACHINECollection
        {
            get
            {
                return Universal.MACHINECollection;
            }
        }

        public FormMotor()
        {
            InitializeComponent();

            this.TopMost = true;

            this.Load += FormMotor_Load;
            this.FormClosed += FormMotor_FormClosed;
        }

        private void FormMotor_FormClosed(object sender, FormClosedEventArgs e)
        {
            Universal.IsOpenMotorWindows = false;
        }

        private void FormMotor_Load(object sender, EventArgs e)
        {
            this.Text = "轴设定视窗";
            Init();
        }
        void Init()
        {
            #region 位置设定控件

            VSAXISUI = new VsTouchMotorUI[] { vsTouchMotorUI1, vsTouchMotorUI2, vsTouchMotorUI3, vsTouchMotorUI6, vsTouchMotorUI5, vsTouchMotorUI4 };

            int i = 0;
            while (i < AXIS_COUNT)
            {
                AXISUI[i] = new MotionTouchPanelUIClass(VSAXISUI[i]);
                AXISUI[i].Initial(((MainFPIX3MachineClass)MACHINECollection.MACHINE).PLCMOTIONCollection[i]);
                i++;
            }

            #endregion

            mMotorTimer = new Timer();
            mMotorTimer.Interval = 50;
            mMotorTimer.Enabled = true;
            mMotorTimer.Tick += MMotorTimer_Tick;


        }
        private void MMotorTimer_Tick(object sender, EventArgs e)
        {
            int i = 0;
            while (i < AXIS_COUNT)
            {
                AXISUI[i].Tick();
                i++;
            }
        }
    }
}
