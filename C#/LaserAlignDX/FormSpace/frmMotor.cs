using Common;
using JetEazy;
using JetEazy.BasicSpace;
using JetEazy.ControlSpace.MotionSpace;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Traveller106.ControlSpace.MachineSpace;
using VsCommon.ControlSpace;
using VsCommon.ControlSpace.MachineSpace;

namespace Traveller106.FormSpace
{
    public partial class frmMotor : Form
    {
        const int AXIS_COUNT = 4;
        VsTouchMotorUI[] VSAXISUI = new VsTouchMotorUI[AXIS_COUNT];
        MotionTouchPanelUIClass[] AXISUI = new MotionTouchPanelUIClass[AXIS_COUNT];

        Timer mMotorTimer = null;
        Button btnLineScanTest;
        Button btnLineScanOnce;

        MachineCollectionClass MACHINECollection
        {
            get
            {
                return Universal.MACHINECollection;
            }
        }

        MiniX6MachineClass MACHINE
        {
            get { return (MiniX6MachineClass)MACHINECollection.MACHINE; }
        }

        public frmMotor()
        {
            InitializeComponent();

            //this.TopMost = true;

            this.Load += FrmMotor_Load;
            this.FormClosed += FrmMotor_FormClosed;
        }

        private void FrmMotor_FormClosed(object sender, FormClosedEventArgs e)
        {
            INI.Instance.IsOpenMotorWindows = false;
        }

        private void FrmMotor_Load(object sender, EventArgs e)
        {
            this.Text = "轴设定视窗";
            Init();
        }

        void Init()
        {
            #region 位置设定控件

            tabPage1.Text = "AXIS-XZ(上)";
            tabPage2.Text = "AXIS-YZ(下)";
            //tabPage2.Text = "轨道一 (AXIS 3-4)";
            //tabPage3.Text = "轨道二 (AXIS 5-6)";
            //tabPage4.Text = "轨道三 (AXIS 7-8)";
            //tabPage5.Text = "轨道四 (AXIS 9-10)";
            //tabPage6.Text = "线扫x轴";

            VSAXISUI[0] = vsTouchMotorUI4;
            VSAXISUI[1] = vsTouchMotorUI1;
            VSAXISUI[2] = vsTouchMotorUI3;
            VSAXISUI[3] = vsTouchMotorUI2;

            //VSAXISUI[3] = vsTouchMotorUI6;
            //VSAXISUI[4] = vsTouchMotorUI5;

            //VSAXISUI[5] = vsTouchMotorUI8;
            //VSAXISUI[6] = vsTouchMotorUI7;

            //VSAXISUI[7] = vsTouchMotorUI10;
            //VSAXISUI[8] = vsTouchMotorUI9;

            //VSAXISUI[9] = vsTouchMotorUI12;
            //VSAXISUI[10] = vsTouchMotorUI11;

            //VSAXISUI[11] = vsTouchMotorUI4;

            //VSAXISUI[11] = vsTouchMotorUI11;
            //VSAXISUI[12] = vsTouchMotorUI14;
            //VSAXISUI[13] = vsTouchMotorUI13;

            int i = 0;
            while (i < AXIS_COUNT)
            {
                AXISUI[i] = new MotionTouchPanelUIClass(VSAXISUI[i]);
                AXISUI[i].Initial(MACHINE.PLCMOTIONCollection[i], ischinese: true);
                //switch (i)
                //{
                //    case 0:
                //    case 2:
                //        AXISUI[i].Initial(MACHINE.PLCMOTIONCollection[i], false);
                //        break;
                //    default:
                      
                //        break;
                //}


                i++;
            }

            #endregion

            mMotorTimer = new Timer();
            mMotorTimer.Interval = 50;
            mMotorTimer.Enabled = true;
            mMotorTimer.Tick += MMotorTimer_Tick;

            MACHINE.TriggerAction += MACHINE_TriggerAction;

            //PG_PosSafe.SelectedObject = MotorConfig.XPropsInstance;

            //FillDisplay();

            //LanguageExClass.Instance.EnumControls(this);

            //btnLineScanTest = button1;
            //btnLineScanTest.Click += BtnLineScanTest_Click;

            //btnLineScanOnce=button2;
            //btnLineScanOnce.Click += BtnLineScanOnce_Click;
        }

        private void BtnLineScanOnce_Click(object sender, EventArgs e)
        {
            //MACHINE.PLCIO.TestLineScanOnceSpeed = (int)numericUpDown3.Value;
            //MACHINE.PLCIO.TestLineScanOnce = true;
        }

        private void BtnLineScanTest_Click(object sender, EventArgs e)
        {
            if (!m_LineScanTestProcess.IsOn)
                m_LineScanTestProcess.Start();
            else
                m_LineScanTestProcess.Stop();
        }

        bool IsEMCTriggered = false;

        private void MACHINE_TriggerAction(MachineEventEnum machineevent, object obj)
        {
            switch (machineevent)
            {
                case MachineEventEnum.ALARM_SERIOUS:
                    //IsAlarmsSeriousX = true;
                    //SetAbnormalLight();
                    break;
                case MachineEventEnum.ALARM_COMMON:
                    //IsAlarmsCommonX = true;
                    //SetAbnormalLight();
                    break;
                case MachineEventEnum.EMC:
                    IsEMCTriggered = true;
                    break;
            }
        }

        private void MMotorTimer_Tick(object sender, EventArgs e)
        {
            if (!Universal.IsNoUseIO)
            {
                if (IsEMCTriggered)
                {
                    //SetAbnormalLight();

                    IsEMCTriggered = false;
                    //StopAllProcess();
                    //OnTrigger(ActionEnum.ACT_ISEMC, "");
                }
            }

            //btnManualAuto.BackColor = (MACHINE.PLCIO.GetMWIndex(IOConstClass.MW1090) == 1 ? Color.Red : Color.Lime);
            //btnManualAuto.Text = (MACHINE.PLCIO.GetMWIndex(IOConstClass.MW1090) == 1 ? "自動模式" : "手動模式");
            //btnChangeValveOneKey.BackColor = (MACHINE.PLCIO.ADR_CHANHEVALVE_ONEKEYING ? Color.Red : Color.Lime);
            //btnChangeValveOneKey.Text = (MACHINE.PLCIO.ADR_CHANHEVALVE_ONEKEYING ? "一鍵換閥中" : "一鍵換閥");

            //btnBypassDoor.BackColor = (MACHINE.PLCIO.ADR_BYPASS_DOOR ? Color.Red : Color.Lime);
            //btnBypassScreen.BackColor = (MACHINE.PLCIO.ADR_BYPASS_SCREEN ? Color.Red : Color.Lime);

            int i = 0;
            while (i < AXIS_COUNT)
            {
                AXISUI[i].Tick();
                i++;
            }

            //_LineScanTestTick();
            //btnLineScanTest.BackColor = (m_LineScanTestProcess.IsOn ? Color.Red : Control.DefaultBackColor);
        }

        float m_distance_1 = 0;
        float m_distance_2 = 0;
        PLCMotionClass AXIS_MODULE2
        {
            get { return MACHINE.PLCMOTIONCollection[5]; }
        }

        ProcessClass m_LineScanTestProcess = new ProcessClass();
        /// <summary>
        /// 线扫测试程序
        /// </summary>
        void _LineScanTestTick()
        {
            ProcessClass Process = m_LineScanTestProcess;

            if (Process.IsOn)
            {
                switch (Process.ID)
                {
                    case 5:

                        //m_distance_1 = (float)numericUpDown1.Value;
                        //m_distance_2 = (float)numericUpDown2.Value;
                       
                        //CommonLogClass.Instance.LogMessage("", Color.Black);
                        Process.NextDuriation = 500;
                        Process.ID = 10;

                        break;
                    case 10:
                        if (Process.IsTimeup)
                        {
                            //MACHINE.PLCIO.SetQXQB("0:" + "QX0.6", true);

                            AXIS_MODULE2.Go(m_distance_1);

                            Process.NextDuriation = 500;
                            Process.ID = 20;
                        }
                        break;
                    case 20:
                        if (Process.IsTimeup)
                        {
                            if (AXIS_MODULE2.IsOnSite)
                            {
                                AXIS_MODULE2.Go(m_distance_2);

                                Process.NextDuriation = 500;
                                Process.ID = 30;
                            }
                        }
                        break;
                    case 30:
                        if (Process.IsTimeup)
                        {
                            if (AXIS_MODULE2.IsOnSite)
                            {
                                //MACHINE.PLCIO.SetQXQB("0:" + "QX0.6", false);
                                AXIS_MODULE2.Go(m_distance_1);

                                Process.NextDuriation = 500;
                                Process.ID = 40;
                            }
                        }
                        break;
                    case 40:
                        if (Process.IsTimeup)
                        {
                            if (AXIS_MODULE2.IsOnSite)
                            {
                                Process.Stop();
                            }
                        }
                        break;
                }
            }
        }

        private void btnExit_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
