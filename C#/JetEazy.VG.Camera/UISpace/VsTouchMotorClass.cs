using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

using ComtactAnglePlus.FromCommon;
using JetEazy.ControlSpace.MotionSpace;
using JetEazy.BasicSpace;
using JetEazy.FormSpace;

namespace Common
{
    enum MotionTag : int
    {
        SPEED_AUTO = 0,
        SPEED_HAND,
        SPEED_HOME_HIGH,
        SPEED_HOME_LOWER,

        PARAS_READ,
        PARAS_WRITE,
    }
    enum ParaPositionTag : int
    {
        COUNT = 1,
        /// <summary>
        /// 待机位置
        /// </summary>
        POS1_READY = 0,
    }

    public class MotionTouchPanelUIClass
    {
        const int MSDuriation = 100;
        string _PositionFormat = "0.00";

        //VersionEnum VERSION;
        //OptionEnum OPTION;

        //触摸屏UI 
        //MotionTouchPanelUI m_MotionTouchPanel;
        VsTouchMotorUI m_MotionTouchPanel;
        VsHandleMotorUI m_MotionHanlePanel;

        #region DEFINE

        VsMessageBox m_msg;

        Button btnForward;
        Button btnBackward;
        Label lblPositionNow;
        Label lblPositionRulerNow;

        Button btnHome;

        NumericUpDown numGoPosition;

        Button btnAdd;
        Button btnSub;

        Button btnGo;

        CheckBox chkIsSlow;

        //Button btnSetReadyPosition;
        //Button btnGoReadyPosition;
        //Label lblReadyPosition;
        Label lblReload;

        JzTimes myTime;

        PLCMotionClass MOTION;

        GroupBox grpMotion;
        GroupBox grpMotionState;
        Label lblMotionName;

        //Status Labels
        Label lblBreak;
        Label lblSVOn;
        Label lblError;
        Label lblUpperLimit;
        Label lblLowerLimit;
        Label lblHome;

        Label lblCurrentSpeed;

        Label lblSpeedAuto;
        Label lblSpeedHand;
        Label lblSpeedHomeHigh;
        Label lblSpeedHomeLower;

        NumericUpDown numSpeedAutoValue;
        NumericUpDown numSpeedHandValue;
        NumericUpDown numSpeedHomeHighValue;
        NumericUpDown numSpeedHomeLowerValue;

        Button btnSetSpeedAuto;
        Button btnSetSpeedHand;
        Button btnSetSpeedHigh;
        Button btnSetSpeedLower;

        Button btnMotionParasRead;
        Button btnMotionParasWrite;

        PropertyGrid MotionParasPropertyGrid;

        TextBox[] txtSetPosition = new TextBox[(int)ParaPositionTag.COUNT];
        Button[] btnSetPosition = new Button[(int)ParaPositionTag.COUNT];
        Button[] btnGOPosition = new Button[(int)ParaPositionTag.COUNT];


        Label lblUnit;

        #endregion

        //Label lblRulerDescription;
        //Label lblRulerPositionNow;

        bool IsNeedToChange = false;
        VsMotorParasClass m_MotorParas = null;

        //public MotionTouchPanelUIClass(MotionTouchPanelUI eMotionTouchPanelUI)
        //{
        //    m_MotionTouchPanel = eMotionTouchPanelUI;
        //}
        public MotionTouchPanelUIClass(VsTouchMotorUI eMotionTouchPanelUI)
        {
            m_MotionTouchPanel = eMotionTouchPanelUI;
            InitialInternal();

        }
        public MotionTouchPanelUIClass(VsHandleMotorUI eMotionTouchPanelUI)
        {
            m_MotionHanlePanel = eMotionTouchPanelUI;
            InitialInternal2();

        }

        void InitialInternal()
        {

            lblUnit = m_MotionTouchPanel.label39;

            numGoPosition = m_MotionTouchPanel.numericUpDown4;

            btnForward = m_MotionTouchPanel.button16;
            btnBackward = m_MotionTouchPanel.button18;

            btnHome = m_MotionTouchPanel.button1;

            btnAdd = m_MotionTouchPanel.button26;
            btnSub = m_MotionTouchPanel.button25;
            btnGo = m_MotionTouchPanel.button19;

            //btnSetReadyPosition = m_MotionTouchPanel.button5;
            //btnGoReadyPosition = m_MotionTouchPanel.button6;

            lblMotionName = m_MotionTouchPanel.label12;
            grpMotionState = m_MotionTouchPanel.groupBox4;
            grpMotion = m_MotionTouchPanel.groupBox9;
            lblReload = m_MotionTouchPanel.label7;

            lblCurrentSpeed = m_MotionTouchPanel.label10;
            lblSpeedAuto = m_MotionTouchPanel.label2;
            lblSpeedHand = m_MotionTouchPanel.label3;
            lblSpeedHomeHigh = m_MotionTouchPanel.label5;
            lblSpeedHomeLower = m_MotionTouchPanel.label19;

            numSpeedAutoValue = m_MotionTouchPanel.numericUpDown1;
            numSpeedHandValue = m_MotionTouchPanel.numericUpDown2;
            numSpeedHomeHighValue = m_MotionTouchPanel.numericUpDown3;
            numSpeedHomeLowerValue = m_MotionTouchPanel.numericUpDown5;

            btnSetSpeedAuto = m_MotionTouchPanel.button2;
            btnSetSpeedHand = m_MotionTouchPanel.button3;
            btnSetSpeedHigh = m_MotionTouchPanel.button4;
            btnSetSpeedLower = m_MotionTouchPanel.button7;

            btnMotionParasRead = m_MotionTouchPanel.button21;
            btnMotionParasWrite = m_MotionTouchPanel.button22;

            MotionParasPropertyGrid = m_MotionTouchPanel.pgMotionParas;

            txtSetPosition[(int)ParaPositionTag.POS1_READY] = m_MotionTouchPanel.textBox1;

            btnSetPosition[(int)ParaPositionTag.POS1_READY] = m_MotionTouchPanel.button5;
            btnSetPosition[(int)ParaPositionTag.POS1_READY].Tag = ParaPositionTag.POS1_READY;

            btnGOPosition[(int)ParaPositionTag.POS1_READY] = m_MotionTouchPanel.button6;
            btnGOPosition[(int)ParaPositionTag.POS1_READY].Tag = ParaPositionTag.POS1_READY;

            numSpeedAutoValue.Tag = MotionTag.SPEED_AUTO;
            numSpeedHandValue.Tag = MotionTag.SPEED_HAND;
            numSpeedHomeHighValue.Tag = MotionTag.SPEED_HOME_HIGH;
            numSpeedHomeLowerValue.Tag = MotionTag.SPEED_HOME_LOWER;

            btnSetSpeedAuto.Tag = MotionTag.SPEED_AUTO;
            btnSetSpeedHand.Tag = MotionTag.SPEED_HAND;
            btnSetSpeedHigh.Tag = MotionTag.SPEED_HOME_HIGH;
            btnSetSpeedLower.Tag = MotionTag.SPEED_HOME_LOWER;

            btnMotionParasRead.Tag = MotionTag.PARAS_READ;
            btnMotionParasWrite.Tag = MotionTag.PARAS_WRITE;


            //txtSetPosition[(int)ParaPositionTag.POS1_READY].Tag = ParaPositionTag.POS1_READY;

            btnSetSpeedAuto.Click += Btn_Click;
            btnSetSpeedHand.Click += Btn_Click;
            btnSetSpeedHigh.Click += Btn_Click;
            btnSetSpeedLower.Click += Btn_Click;
            btnMotionParasRead.Click += Btn_Click;
            btnMotionParasWrite.Click += Btn_Click;


            btnForward.MouseDown += new MouseEventHandler(btnForward_MouseDown);
            btnForward.MouseUp += new MouseEventHandler(btnForward_MouseUp);
            btnBackward.MouseDown += new MouseEventHandler(btnBackward_MouseDown);
            btnBackward.MouseUp += new MouseEventHandler(btnBackward_MouseUp);

            btnHome.Click += new EventHandler(btnHome_Click);

            btnAdd.Click += new EventHandler(btnAdd_Click);
            btnSub.Click += new EventHandler(btnSub_Click);
            btnGo.Click += new EventHandler(btnGo_Click);

            btnSetPosition[(int)ParaPositionTag.POS1_READY].Click += MotionTouchPanelUIBtnSetPositionClass_Click;
            btnGOPosition[(int)ParaPositionTag.POS1_READY].Click += MotionTouchPanelUIBtnGOPositionClass_Click;

            //btnSetReadyPosition.Click += new EventHandler(btnSetReadyPosition_Click);
            //btnGoReadyPosition.Click += new EventHandler(btnGoReadyPosition_Click);
            lblReload.DoubleClick += LblReload_DoubleClick;

            chkIsSlow = m_MotionTouchPanel.checkBox1;
            chkIsSlow.CheckedChanged += new EventHandler(chkIsSlow_CheckedChanged);

            lblPositionNow = m_MotionTouchPanel.label13;
            lblPositionRulerNow = m_MotionTouchPanel.label26;
            //lblReadyPosition = label25;

            //lblRulerDescription = label7;
            //lblRulerPositionNow = label6;

            lblBreak = m_MotionTouchPanel.label9;
            lblSVOn = m_MotionTouchPanel.label8;
            lblError = m_MotionTouchPanel.label18;
            lblUpperLimit = m_MotionTouchPanel.label15;
            lblLowerLimit = m_MotionTouchPanel.label17;
            lblHome = m_MotionTouchPanel.label16;

            lblSVOn.DoubleClick += LblSVOn_DoubleClick;
            lblBreak.DoubleClick += LblBreak_DoubleClick;
            lblError.DoubleClick += LblError_DoubleClick;

            numGoPosition.Click += NumClick;
            numSpeedAutoValue.Click += NumClick;
            numSpeedHandValue.Click += NumClick;
            numSpeedHomeHighValue.Click += NumClick;
            numSpeedHomeLowerValue.Click += NumClick;

            myTime = new JzTimes();
            myTime.Cut();

        }
        void InitialInternal2()
        {

            lblUnit = m_MotionHanlePanel.label39;

            numGoPosition = m_MotionHanlePanel.numericUpDown4;

            btnForward = m_MotionHanlePanel.button16;
            btnBackward = m_MotionHanlePanel.button18;

            btnHome = m_MotionHanlePanel.button1;

            btnAdd = m_MotionHanlePanel.button26;
            btnSub = m_MotionHanlePanel.button25;
            btnGo = m_MotionHanlePanel.button19;

            //btnSetReadyPosition = m_MotionTouchPanel.button5;
            //btnGoReadyPosition = m_MotionTouchPanel.button6;

            lblMotionName = m_MotionHanlePanel.label12;
            grpMotionState = m_MotionHanlePanel.groupBox4;
            grpMotion = m_MotionHanlePanel.groupBox9;
            //lblReload = m_MotionHanlePanel.label7;

            lblCurrentSpeed = m_MotionHanlePanel.label10;
            //lblSpeedAuto = m_MotionHanlePanel.label2;
            //lblSpeedHand = m_MotionHanlePanel.label3;
            //lblSpeedHomeHigh = m_MotionHanlePanel.label5;
            //lblSpeedHomeLower = m_MotionHanlePanel.label19;

            //numSpeedAutoValue = m_MotionHanlePanel.numericUpDown1;
            //numSpeedHandValue = m_MotionHanlePanel.numericUpDown2;
            //numSpeedHomeHighValue = m_MotionHanlePanel.numericUpDown3;
            //numSpeedHomeLowerValue = m_MotionHanlePanel.numericUpDown5;

            //btnSetSpeedAuto = m_MotionHanlePanel.button2;
            //btnSetSpeedHand = m_MotionHanlePanel.button3;
            //btnSetSpeedHigh = m_MotionHanlePanel.button4;
            //btnSetSpeedLower = m_MotionHanlePanel.button7;

            btnMotionParasRead = m_MotionHanlePanel.button21;
            btnMotionParasWrite = m_MotionHanlePanel.button22;

            MotionParasPropertyGrid = m_MotionHanlePanel.pgMotionParas;

            //txtSetPosition[(int)ParaPositionTag.POS1_READY] = m_MotionHanlePanel.textBox1;

            //btnSetPosition[(int)ParaPositionTag.POS1_READY] = m_MotionHanlePanel.button5;
            //btnSetPosition[(int)ParaPositionTag.POS1_READY].Tag = ParaPositionTag.POS1_READY;

            //btnGOPosition[(int)ParaPositionTag.POS1_READY] = m_MotionHanlePanel.button6;
            //btnGOPosition[(int)ParaPositionTag.POS1_READY].Tag = ParaPositionTag.POS1_READY;

            //numSpeedAutoValue.Tag = MotionTag.SPEED_AUTO;
            //numSpeedHandValue.Tag = MotionTag.SPEED_HAND;
            //numSpeedHomeHighValue.Tag = MotionTag.SPEED_HOME_HIGH;
            //numSpeedHomeLowerValue.Tag = MotionTag.SPEED_HOME_LOWER;

            //btnSetSpeedAuto.Tag = MotionTag.SPEED_AUTO;
            //btnSetSpeedHand.Tag = MotionTag.SPEED_HAND;
            //btnSetSpeedHigh.Tag = MotionTag.SPEED_HOME_HIGH;
            //btnSetSpeedLower.Tag = MotionTag.SPEED_HOME_LOWER;

            btnMotionParasRead.Tag = MotionTag.PARAS_READ;
            btnMotionParasWrite.Tag = MotionTag.PARAS_WRITE;


            //txtSetPosition[(int)ParaPositionTag.POS1_READY].Tag = ParaPositionTag.POS1_READY;

            //btnSetSpeedAuto.Click += Btn_Click;
            //btnSetSpeedHand.Click += Btn_Click;
            //btnSetSpeedHigh.Click += Btn_Click;
            //btnSetSpeedLower.Click += Btn_Click;
            btnMotionParasRead.Click += Btn_Click;
            btnMotionParasWrite.Click += Btn_Click;


            btnForward.MouseDown += new MouseEventHandler(btnForward_MouseDown);
            btnForward.MouseUp += new MouseEventHandler(btnForward_MouseUp);
            btnBackward.MouseDown += new MouseEventHandler(btnBackward_MouseDown);
            btnBackward.MouseUp += new MouseEventHandler(btnBackward_MouseUp);

            btnHome.Click += new EventHandler(btnHome_Click);

            btnAdd.Click += new EventHandler(btnAdd_Click);
            btnSub.Click += new EventHandler(btnSub_Click);
            btnGo.Click += new EventHandler(btnGo_Click);

            //btnSetPosition[(int)ParaPositionTag.POS1_READY].Click += MotionTouchPanelUIBtnSetPositionClass_Click;
            //btnGOPosition[(int)ParaPositionTag.POS1_READY].Click += MotionTouchPanelUIBtnGOPositionClass_Click;

            //btnSetReadyPosition.Click += new EventHandler(btnSetReadyPosition_Click);
            //btnGoReadyPosition.Click += new EventHandler(btnGoReadyPosition_Click);
            //lblReload.DoubleClick += LblReload_DoubleClick;

            chkIsSlow = m_MotionHanlePanel.checkBox1;
            chkIsSlow.CheckedChanged += new EventHandler(chkIsSlow_CheckedChanged);

            lblPositionNow = m_MotionHanlePanel.label13;
            lblPositionRulerNow = m_MotionHanlePanel.label26;
            //lblReadyPosition = label25;

            //lblRulerDescription = label7;
            //lblRulerPositionNow = label6;

            lblBreak = m_MotionHanlePanel.label9;
            lblSVOn = m_MotionHanlePanel.label8;
            lblError = m_MotionHanlePanel.label18;
            lblUpperLimit = m_MotionHanlePanel.label15;
            lblLowerLimit = m_MotionHanlePanel.label17;
            lblHome = m_MotionHanlePanel.label16;

            lblSVOn.DoubleClick += LblSVOn_DoubleClick;
            lblBreak.DoubleClick += LblBreak_DoubleClick;
            lblError.DoubleClick += LblError_DoubleClick;

            //numGoPosition.Click += NumClick;
            //numSpeedAutoValue.Click += NumClick;
            //numSpeedHandValue.Click += NumClick;
            //numSpeedHomeHighValue.Click += NumClick;
            //numSpeedHomeLowerValue.Click += NumClick;

            myTime = new JzTimes();
            myTime.Cut();

        }

        //DigitalKeyboardForm keyboardForm;
        private void NumClick(object sender, EventArgs e)
        {
            //if (keyboardForm == null)
            //{
            //    NumericUpDown num = sender as NumericUpDown;
            //    keyboardForm = new DigitalKeyboardForm(num);
            //    keyboardForm.ShowDialog();
            //    keyboardForm.Dispose();
            //    keyboardForm = null;
            //}
        }

        public void Initial(PLCMotionClass motion, bool isupdown = true,bool ischinese =false)
        //public void Initial(PLCMotionClass motion, VersionEnum version, OptionEnum option, bool isupdown = true)
        {

            //VERSION = version;
            //OPTION = option;
            MOTION = motion;

            if(ischinese)
            {
                btnForward.Text = "正转";
                btnBackward.Text = "反转";
            }
            else
            {
                if (isupdown)
                {
                    //btnForward.Text = "↑";
                    //btnBackward.Text = "↓";

                    btnForward.Text = "↓";
                    btnBackward.Text = "↑";
                }
                else
                {
                    btnForward.Text = "→";
                    btnBackward.Text = "←";
                }
            }

            //btnAdd.Visible = false;
            //btnSub.Visible = false;

            if (MOTION == null)
                return;

            m_MotorParas = new VsMotorParasClass(MOTION);

            //Set Default Speed
            MOTION.SetSpeed(SpeedTypeEnum.HOMESLOW);
            MOTION.SetSpeed(SpeedTypeEnum.HOMEHIGH);
            MOTION.SetSpeed(SpeedTypeEnum.MANUAL);
            MOTION.SetSpeed(SpeedTypeEnum.GO);

            MOTION.LoadData();
            FillDisplay();
        }

        private void MotionTouchPanelUIBtnGOPositionClass_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            ParaPositionTag tag = (ParaPositionTag)btn.Tag;
            switch (tag)
            {
                case ParaPositionTag.POS1_READY:

                    MOTION.Go(MOTION.READYPOSITION);

                    break;
            }
        }

        private void MotionTouchPanelUIBtnSetPositionClass_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            ParaPositionTag tag = (ParaPositionTag)btn.Tag;
            switch (tag)
            {
                case ParaPositionTag.POS1_READY:

                    MOTION.READYPOSITION = MOTION.PositionNow;
                    txtSetPosition[(int)tag].Text = MOTION.READYPOSITION.ToString(_PositionFormat);

                    MOTION.SaveData();

                    break;
            }
        }

        private void Btn_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            MotionTag tag = (MotionTag)btn.Tag;
            switch (tag)
            {
                case MotionTag.SPEED_AUTO:

                    MOTION.GOSPEED = (int)numSpeedAutoValue.Value;
                    MOTION.SetSpeed(SpeedTypeEnum.GO);
                    MOTION.SaveData(MotionAddressEnum.GOSPEED, MOTION.GOSPEED.ToString());

                    break;
                case MotionTag.SPEED_HAND:

                    MOTION.MANUALSPEED = (int)numSpeedHandValue.Value;
                    MOTION.SetSpeed(SpeedTypeEnum.MANUAL);
                    MOTION.SaveData(MotionAddressEnum.MANUALSPEED, MOTION.MANUALSPEED.ToString());
                    break;
                case MotionTag.SPEED_HOME_HIGH:

                    MOTION.HOMEHIGHSPEED = (int)numSpeedHomeHighValue.Value;
                    MOTION.SetSpeed(SpeedTypeEnum.HOMEHIGH);
                    MOTION.SaveData(MotionAddressEnum.HOMEHIGHSPEED, MOTION.HOMEHIGHSPEED.ToString());
                    break;
                case MotionTag.SPEED_HOME_LOWER:

                    MOTION.HOMESLOWSPEED = (int)numSpeedHomeLowerValue.Value;
                    MOTION.SetSpeed(SpeedTypeEnum.HOMESLOW);
                    MOTION.SaveData(MotionAddressEnum.HOMESLOWSPEED, MOTION.HOMESLOWSPEED.ToString());
                    break;

                case MotionTag.PARAS_READ:
                    ReadFromPLCParas();
                    break;
                case MotionTag.PARAS_WRITE:

                    WriteToPLCParas();

                    break;

            }
        }

        void ReadFromPLCParas()
        {

        }
        public void WriteToPLCParas()
        {
            MOTION.GOSPEED = m_MotorParas.GOSPEED;
            MOTION.SetSpeed(SpeedTypeEnum.GO);
            MOTION.SaveData(MotionAddressEnum.GOSPEED, MOTION.GOSPEED.ToString());

            MOTION.GOSLOWSPEED = m_MotorParas.GOSLOWSPEED;
            //MOTION.SetSpeed(SpeedTypeEnum.GOSLOW);
            MOTION.SaveData(MotionAddressEnum.GOSLOWSPEED, MOTION.GOSLOWSPEED.ToString());

            MOTION.MANUALSPEED = m_MotorParas.MANUALSPEED;
            MOTION.SetSpeed(SpeedTypeEnum.MANUAL);
            MOTION.SaveData(MotionAddressEnum.MANUALSPEED, MOTION.MANUALSPEED.ToString());

            MOTION.MANUALSLOWSPEED = m_MotorParas.MANUALSLOWSPEED;
            //MOTION.SetSpeed(SpeedTypeEnum.MANUALSLOW);
            MOTION.SaveData(MotionAddressEnum.MANUALSLOWSPEED, MOTION.MANUALSLOWSPEED.ToString());

            MOTION.HOMEHIGHSPEED = m_MotorParas.HOMEHIGHSPEED;
            MOTION.SetSpeed(SpeedTypeEnum.HOMEHIGH);
            MOTION.SaveData(MotionAddressEnum.HOMEHIGHSPEED, MOTION.HOMEHIGHSPEED.ToString());

            MOTION.HOMESLOWSPEED = m_MotorParas.HOMESLOWSPEED;
            MOTION.SetSpeed(SpeedTypeEnum.HOMESLOW);
            MOTION.SaveData(MotionAddressEnum.HOMESLOWSPEED, MOTION.HOMESLOWSPEED.ToString());

            MOTION.READYPOSITION = m_MotorParas.READYPOSITION;// MOTION.PositionNow;
            MOTION.TESTPOSITION = m_MotorParas.TESTPOSITION;
            MOTION.SaveData();
        }

        private void LblError_DoubleClick(object sender, EventArgs e)
        {
            MOTION.Reset();
        }

        private void LblBreak_DoubleClick(object sender, EventArgs e)
        {
            MOTION.Break();
        }

        private void LblSVOn_DoubleClick(object sender, EventArgs e)
        {
            MOTION.SVOn();
        }

        private void LblReload_DoubleClick(object sender, EventArgs e)
        {
            if (JetEazy.BasicSpace.VsMSG.Instance.Question("是否要载入原始设定？") == DialogResult.OK)
                //if (MessageBox.Show("是否要載入原始設定?", "SYSTEM", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                MOTION.LoadData();
                FillDisplay();
            }
        }

        void btnGoReadyPosition_Click(object sender, EventArgs e)
        {
            MOTION.Go(MOTION.READYPOSITION);
        }

        void btnSetReadyPosition_Click(object sender, EventArgs e)
        {
            MOTION.READYPOSITION = MOTION.PositionNow;
            //lblReadyPosition.Text = MOTION.READYPOSITION.ToString("0.000");
            //txtSetPosition[(int)ParaPositionTag.POS1_READY].Text

            MOTION.SaveData();
        }

        void chkIsSlow_CheckedChanged(object sender, EventArgs e)
        {
            if (!IsNeedToChange)
                return;

            if (chkIsSlow.Checked)
            {
                MOTION.SetSpeed(SpeedTypeEnum.MANUALSLOW);
                MOTION.SetSpeed(SpeedTypeEnum.GOSLOW);
            }
            else
            {
                MOTION.SetSpeed(SpeedTypeEnum.MANUAL);
                MOTION.SetSpeed(SpeedTypeEnum.GO);
            }
        }
        void ResetSlow()
        {
            chkIsSlow.Checked = false;
            MOTION.SetSpeed(SpeedTypeEnum.MANUAL);
        }
        void btnBackward_MouseUp(object sender, MouseEventArgs e)
        {
            MOTION.Stop();
        }

        void btnBackward_MouseDown(object sender, MouseEventArgs e)
        {
            MOTION.Backward();
        }

        void btnForward_MouseUp(object sender, MouseEventArgs e)
        {
            MOTION.Stop();
        }

        void btnForward_MouseDown(object sender, MouseEventArgs e)
        {
            MOTION.Forward();
        }
        void btnGo_Click(object sender, EventArgs e)
        {
            m_msg = new VsMessageBox("是否确定开始定位？",false);
            if (DialogResult.Cancel == m_msg.ShowDialog())
                return;

            MOTION.Go((float)numGoPosition.Value);
        }
        void btnSub_Click(object sender, EventArgs e)
        {
            MOTION.Go(MOTION.PositionNow - (float)numGoPosition.Value);
        }
        void btnAdd_Click(object sender, EventArgs e)
        {
            MOTION.Go((float)numGoPosition.Value + MOTION.PositionNow);
        }
        void btnHome_Click(object sender, EventArgs e)
        {
            m_msg = new VsMessageBox("是否确定回原点？",false);
            if (DialogResult.Cancel == m_msg.ShowDialog())
                return;

            MOTION.Home();
        }

        public void FillDisplay()
        {
            IsNeedToChange = false;

            //lblReadyPosition.Text = MOTION.READYPOSITION.ToString("0.000");

            grpMotion.Text = "轴编号-" + MOTION.MOTIONNAME.ToString();
            grpMotionState.Text = "轴-" + MOTION.MOTIONNAME.ToString() + " 状态";
            //lblMotionName.Text = "轴编号-" + MOTION.MOTIONNAME.ToString();
            lblMotionName.Text = "[ " + MOTION.MOTIONALIAS + " ]";
            lblUnit.Text = MOTION.MOTIONUNIT;

            //lblMotionName.Text = "轴-" + MOTION.MOTIONNAME.ToString() + "[ " + MOTION.MOTIONALIAS + " ]";


            //switch (MOTION.MOTIONNAME)
            //{
            //    case MotionEnum.M6:
            //    case MotionEnum.M7:
            //    case MotionEnum.M8:

            //        //grpMotion.Text = "马达Z-" + MOTION.MOTIONNAME.ToString();
            //        //grpMotionState.Text = "马达Z-" + MOTION.MOTIONNAME.ToString() + " 状态";
            //        //lblMotionName.Text = "马达Z编号-" + MOTION.MOTIONNAME.ToString();

            //        break;
            //    case MotionEnum.M1:

            //        //grpMotion.Text = "马达X-" + MOTION.MOTIONNAME.ToString();
            //        //grpMotionState.Text = "马达X-" + MOTION.MOTIONNAME.ToString() + " 状态";
            //        //lblMotionName.Text = "马达X编号-" + MOTION.MOTIONNAME.ToString();

            //        break;
            //    default:

            //        //grpMotion.Text = "马达-" + MOTION.MOTIONNAME.ToString();
            //        //grpMotionState.Text = "马达-" + MOTION.MOTIONNAME.ToString() + " 状态";
            //        //lblMotionName.Text = "马达编号-" + MOTION.MOTIONNAME.ToString();

            //        break;
            //}

            lblBreak.Visible = MOTION.IsHaveBreakOption;
            lblSVOn.Visible = MOTION.IsHaveSVOnOption;

            //txtSetPosition[(int)ParaPositionTag.POS1_READY].Text = MOTION.READYPOSITION.ToString(_PositionFormat);

            MotionParasPropertyGrid.SelectedObject = m_MotorParas;

            //lblRulerDescription.Visible = MOTION.IsHaveRulerOption;
            //lblRulerPositionNow.Visible = MOTION.IsHaveRulerOption;

            ResetSlow();

            IsNeedToChange = true;
        }


        public void Tick()
        {
            if (myTime.msDuriation < MSDuriation)
                return;

            btnHome.BackColor = (MOTION.IsHome ? SystemColors.Control : Color.Red);
            btnGo.BackColor = (MOTION.IsOK ? SystemColors.Control : Color.Red);
            btnAdd.BackColor = (MOTION.IsOK ? SystemColors.Control : Color.Red);
            btnSub.BackColor = (MOTION.IsOK ? SystemColors.Control : Color.Red);
            btnBackward.BackColor = (MOTION.IsOK ? SystemColors.Control : Color.Red);
            btnForward.BackColor = (MOTION.IsOK ? SystemColors.Control : Color.Red);

            lblUpperLimit.BackColor = (MOTION.IsReachUpperBound ? Color.Pink : SystemColors.Control);
            lblLowerLimit.BackColor = (MOTION.IsReachLowerBound ? Color.Pink : SystemColors.Control);
            lblHome.BackColor = (MOTION.IsHome ? Color.Gold : SystemColors.Control);

            if (lblBreak.Visible)
                lblBreak.BackColor = (MOTION.IsBreack ? Color.Red : SystemColors.Control);
            if (lblSVOn.Visible)
                lblSVOn.BackColor = (MOTION.IsSVOn ? Color.Lime : SystemColors.Control);
            if (lblError.Visible)
                lblError.BackColor = (MOTION.IsError ? Color.Red : SystemColors.Control);

            lblCurrentSpeed.Text = MOTION.GetSpeed(SpeedTypeEnum.GO).ToString();
            //lblSpeedAuto.Text = MOTION.GetSpeed(SpeedTypeEnum.GO).ToString();
            //lblSpeedHand.Text = MOTION.GetSpeed(SpeedTypeEnum.MANUAL).ToString();
            //lblSpeedHomeHigh.Text = MOTION.GetSpeed(SpeedTypeEnum.HOMEHIGH).ToString();
            //lblSpeedHomeLower.Text = MOTION.GetSpeed(SpeedTypeEnum.HOMESLOW).ToString();

            //if (lblRulerPositionNow.Visible)
            //    lblRulerPositionNow.Text = MOTION.RulerPositionNowString;

            lblPositionNow.Text = MOTION.PositionNowString;
            lblPositionRulerNow.Text = MOTION.RulerPositionNowString;

            myTime.Cut();

        }
    }
}
