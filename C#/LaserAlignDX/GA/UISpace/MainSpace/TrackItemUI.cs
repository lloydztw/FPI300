using BSA.ControlSpace;
using Common.RecipeSpace;
using Eazy_Project_III;
//using JetEazy;

//using JetEazy;
using JetEazy.BasicSpace;
using NeedleX.ProcessSpace;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Traveller106;
using Traveller106.ControlSpace.MachineSpace;
//using Traveller106.OPSpace;
using TravellerMINIX6;
using TravellerMINIX6.OPSpace;
using TravellerMINIX6.ProcessSpace;
using VsCommon.ControlSpace.MachineSpace;

namespace TravellerMINIX6.UISpace.MainSpace
{
    public partial class TrackItemUI : UserControl
    {
        TrayClass TrayUI;

        ChannelBarcodeClass BarcodeClass
        {
            get { return Universal.ChannelBarcode[(int)TRACKAREA]; }
        }

        protected RecipeMiniX6Class myRecipe
        {
            get { return RecipeMiniX6Class.Instance; }
        }

        BaseProcess m_LineScanProcess
        {
            get { return LineScanProcess.Instance; }
        }

        #region DEFINE UI

        Panel pnlInputArea;
        Panel pnlTrayInspectArea;
        Panel pnlPickArea;
        Panel pnlLineScanArea;
        Panel pnlOutputArea;
        Panel pnlBindui;

        Panel pnlChipPickUI;

        Button btnFeed1;
        Button btnFeed2;
        Button btnPut1;
        Button btnPut2;

        Button btnTestLine;
        Button btnUp;
        Button btnDown;
        Label lblCurrent;
        Label lblCount;
        Label lblBarcode;

        Button btnLineTest2;
        Label lblplctopcSign;
        Label lblpctoplcSign;

        Button btnRetryGetBarcode;
        Button btnRetryBind;

        //Label lblLoadText;
        Label lblSaveText;

        ListBox lstLoadData;
        ListBox lstSaveData;

        #endregion

        JetEazy.VersionEnum VERSION = JetEazy.VersionEnum.TRAVELLER;
        JetEazy.OptionEnum OPTION = JetEazy.OptionEnum.MAIN_LS;

        MiniX6MachineClass MACHINE;
        TrackArea TRACKAREA;
        DataGridView dgvDataUI1;
        DataGridView dgvDataUI2;

        public TrackItemUI()
        {
            InitializeComponent();
            InitUI();
        }

        void InitUI()
        {
            lblCurrent = label6;
            lblCount = label7;
            btnUp = button6;
            btnDown = button7;
            lblBarcode = label8;

            pnlInputArea = panel6;
            pnlTrayInspectArea = panel3;
            pnlPickArea = panel2;
            pnlLineScanArea = panel1;
            pnlOutputArea = panel5;

            pnlChipPickUI = panel4;
            pnlBindui = panel7;

            btnFeed1 = button1;
            btnFeed2 = button3;
            btnPut1 = button2;
            btnPut2 = button4;
            btnTestLine = button5;
            btnRetryGetBarcode = button8;
            btnRetryBind = button12;
            btnLineTest2 = button9;
            lblplctopcSign = label10;
            lblpctoplcSign = label12;

            //lblLoadText = label8;
            lblSaveText = label9;

            //dgvDataUI1 = dataGridView1;
            //dgvDataUI2 = dataGridView2;

            lstLoadData = listBox1;
            lstSaveData = listBox2;

            btnFeed1.Click += BtnFeed1_Click;
            btnFeed2.Click += BtnFeed2_Click;
            btnPut1.Click += BtnPut1_Click;
            btnPut2.Click += BtnPut2_Click;
            btnTestLine.Click += BtnTestLine_Click;

            lblp1.DoubleClick += Lblp1_DoubleClick;

            btnUp.Click += BtnUp_Click;
            btnDown.Click += BtnDown_Click;

            lblBarcode.DoubleClick += LblBarcode_DoubleClick;
            btnRetryGetBarcode.Click += BtnRetryGetBarcode_Click;
            btnRetryBind.Click += BtnRetryBind_Click;
            btnLineTest2.Click += BtnLineTest2_Click;

            TrayUI = new TrayClass();

            label3.DoubleClick += Label3_DoubleClick;
        }

        private void BtnLineTest2_Click(object sender, EventArgs e)
        {
            string onStrMsg = "是否要进行线扫测试？";
            string offStrMsg = "是否要停止线扫测试？";
            string msg = (m_LineScanProcess.IsOn ? offStrMsg : onStrMsg);

            if (VsMSG.Instance.Question(msg) == DialogResult.OK)
            {
                INI.Instance.HistoryDataPath = Traveller106.Universal.HISTORY + "\\" + JzTimes.DateSerialString;
                INI.Instance.HistoryDataBarcode = JzTimes.DateTimeSerialString;

                //LineScanProcess.Instance.SetTestMode(Eazy_Project_III.LinescanTestMode.Ls_NoTray);
                if (!m_LineScanProcess.IsOn)
                {
                    m_LineScanProcess.Start();
                }
                else
                {
                    m_LineScanProcess.Stop();
                }
            }
        }

        private void Label3_DoubleClick(object sender, EventArgs e)
        {
            //if (TRACKAREA == TrackArea.TrackINSPECT)
            //    return;
            BarcodeClass.Forcepass = !BarcodeClass.Forcepass;
        }

        private void BtnRetryBind_Click(object sender, EventArgs e)
        {
            if (!MACHINE.PLCIO.RetryBind)
                MACHINE.PLCIO.RetryBind = true;
        }

        private void BtnRetryGetBarcode_Click(object sender, EventArgs e)
        {
            if (!MACHINE.PLCIO.RetryGetBarcode)
                MACHINE.PLCIO.RetryGetBarcode = true;
        }

        private string m_AutoBarcode = string.Empty;
        public void SetBarcode(string ebarcode)
        {
            m_AutoBarcode = ebarcode;
            lblBarcode.Invoke(new Action(() =>
            {
                lblBarcode.Text = m_AutoBarcode;
            }));
            lstLoadData.Invoke(new Action(() =>
            {
                lstLoadData.Items.Add(ebarcode);
            }));
        }
        public string GetBarcode()
        {
            return m_AutoBarcode;
        }
        private void LblBarcode_DoubleClick(object sender, EventArgs e)
        {
            int i = (int)TRACKAREA;
            lblBarcode.Text = MACHINE.EzBarcodeM3DHelperCollection[i].Run();
        }
        //public void SetSaveTxt(string ebarcode)
        //{
        //    m_AutoBarcode = ebarcode;
        //    lblBarcode.Invoke(new Action(() =>
        //    {
        //        lblBarcode.Text = m_AutoBarcode;
        //    }));
        //    lstLoadData.Invoke(new Action(() =>
        //    {
        //        lstLoadData.Items.Add(ebarcode);
        //    }));
        //}
        List<AnalyzeClass> list = new List<AnalyzeClass>();
        int m_List_index = 0;
        public void SetList(List<AnalyzeClass> elist)
        {
            list.Clear();
            foreach (AnalyzeClass el in elist)
            {
                list.Add(el);
            }
        }
        private void BtnDown_Click(object sender, EventArgs e)
        {
            m_List_index++;
            if (m_List_index >= list.Count)
                m_List_index = 0;
            _updownlist(m_List_index);
         
        }

        private void BtnUp_Click(object sender, EventArgs e)
        {
            m_List_index--;
            if (m_List_index < 0)
                m_List_index = list.Count - 1;
            _updownlist(m_List_index);
            
        }

        void _updownlist(int eindex)
        {
            if (list.Count == 0)
                return;
            if (eindex >= list.Count || eindex < 0)
                return;
            //模拟测试的数据
            AnalyzeClass assignClass = list[eindex];
            //assignClass.SetCellResult(RecipeTrayClass.Instance.TrayRowCount, RecipeTrayClass.Instance.TrayColCount);
            //m_AutoAssignClassesTmp.Add(assignClass);
            //assignClass.Path = Traveller106.Universal.COLLECT + "\\tmp_" + m_CollectDataIndex.ToString("0000") + ".cfg";
            //assignClass.SaveCurrent();
            //switch (TRACKAREA)
            //{
            //    //case TrackArea.TrackPASS:
            //    //    break;
            //    //case TrackArea.TrackINSPECT:
            //    //    //assignClass.CopyState(RecipeTrayClass.Instance.TARCKASSIGN[(int)TrackAreaPosition.LEFT_T2], false);

            //    //    break;
            //    //case TrackArea.TrackNG1:
            //    //    break;
            //    //case TrackArea.TrackNG2:
            //    //    break;
            //}
        }

        private void Lblp1_DoubleClick(object sender, EventArgs e)
        {
            //模拟测试的数据
            AnalyzeClass assignClass = new AnalyzeClass();
            assignClass.SetCellResult(myRecipe.TrayRowCount, myRecipe.TrayColCount);
            //m_AutoAssignClassesTmp.Add(assignClass);
            //assignClass.Path = Traveller106.Universal.COLLECT + "\\tmp_" + m_CollectDataIndex.ToString("0000") + ".cfg";
            //assignClass.SaveCurrent();
            //switch (TRACKAREA)
            //{
            //    case TrackArea.TrackPASS:
            //        break;
            //    case TrackArea.TrackINSPECT:
            //        //assignClass.CopyState(RecipeTrayClass.Instance.TARCKASSIGN[(int)TrackAreaPosition.LEFT_T2], false);

            //        break;
            //    case TrackArea.TrackNG1:
            //        break;
            //    case TrackArea.TrackNG2:
            //        break;
            //}

        }

        private void BtnTestLine_Click(object sender, EventArgs e)
        {
            //switch (TRACKAREA)
            //{
            //    case TrackArea.TrackPASS:
            //        break;
            //    case TrackArea.TrackINSPECT:
            //        //if (Universal.Modules[(int)TrackModule.M2_LINE].Running)
            //        //    Universal.Modules[(int)TrackModule.M2_LINE].ForceStop = true;
            //        //else
            //        //    Universal.Modules[(int)TrackModule.M2_LINE].Start = !Universal.Modules[(int)TrackModule.M2_LINE].Running;
            //        break;
            //    case TrackArea.TrackNG1:
            //        break;
            //    case TrackArea.TrackNG2:
            //        break;
            //}
        }

        private void BtnPut2_Click(object sender, EventArgs e)
        {
            switch (TRACKAREA)
            {
                //case TrackArea.TrackPASS:
                //    Universal.Modules[(int)TrackModule.M1_PUT].Start = !Universal.Modules[(int)TrackModule.M1_PUT].Running;
                //    break;
                case TrackArea.TrackINSPECT:

                    if(m_LineScanProcess.IsOn)
                    {
                        MessageBox.Show("綫掃流程中，請勿操作！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    Universal.Modules[(int)TrackSingleModule.CHL_PUT2].Start = !Universal.Modules[(int)TrackSingleModule.CHL_PUT2].Running;
                    break;
                //case TrackArea.TrackNG1:
                //    Universal.Modules[(int)TrackModule.M3_PUT].Start = !Universal.Modules[(int)TrackModule.M3_PUT].Running;
                //    break;
                //case TrackArea.TrackNG2:
                //    Universal.Modules[(int)TrackModule.M4_PUT].Start = !Universal.Modules[(int)TrackModule.M4_PUT].Running;
                //    break;
            }
        }

        private void BtnPut1_Click(object sender, EventArgs e)
        {
            switch (TRACKAREA)
            {
                //case TrackArea.TrackPASS:
                //    break;
                case TrackArea.TrackINSPECT:

                    if (m_LineScanProcess.IsOn)
                    {
                        MessageBox.Show("綫掃流程中，請勿操作！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    Universal.Modules[(int)TrackSingleModule.CHL_PUT1].Start = !Universal.Modules[(int)TrackSingleModule.CHL_PUT1].Running;
                    break;
                //case TrackArea.TrackNG1:
                //    break;
                //case TrackArea.TrackNG2:
                //    break;
            }
        }

        private void BtnFeed2_Click(object sender, EventArgs e)
        {
            switch (TRACKAREA)
            {
                //case TrackArea.TrackPASS:
                //    break;
                case TrackArea.TrackINSPECT:
                    if (m_LineScanProcess.IsOn)
                    {
                        MessageBox.Show("綫掃流程中，請勿操作！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    Universal.Modules[(int)TrackSingleModule.CHL_FEED2].Start = !Universal.Modules[(int)TrackSingleModule.CHL_FEED2].Running;
                    break;
                //case TrackArea.TrackNG1:
                //    break;
                //case TrackArea.TrackNG2:
                //    break;
            }
        }

        private void BtnFeed1_Click(object sender, EventArgs e)
        {
            switch (TRACKAREA)
            {
                //case TrackArea.TrackPASS:
                //    Universal.Modules[(int)TrackModule.M1_FEED].Start = !Universal.Modules[(int)TrackModule.M1_FEED].Running;
                //    break;
                case TrackArea.TrackINSPECT:

                    if (m_LineScanProcess.IsOn)
                    {
                        MessageBox.Show("綫掃流程中，請勿操作！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    Universal.Modules[(int)TrackSingleModule.CHL_FEED1].Start = !Universal.Modules[(int)TrackSingleModule.CHL_FEED1].Running;
                    break;
                    //case TrackArea.TrackNG1:
                    //    Universal.Modules[(int)TrackModule.M3_FEED].Start = !Universal.Modules[(int)TrackModule.M3_FEED].Running;
                    //    break;
                    //case TrackArea.TrackNG2:
                    //    Universal.Modules[(int)TrackModule.M4_FEED].Start = !Universal.Modules[(int)TrackModule.M4_FEED].Running;
                    //    break;
            }
        }

        public void Initial(JetEazy.VersionEnum version, JetEazy.OptionEnum option, MiniX6MachineClass machine, TrackArea trackarea)
        {
            VERSION = version;
            OPTION = option;
            MACHINE = machine;
            TRACKAREA = trackarea;

            pnlLineScanArea.Visible = false;
            //TrayUI.Initial(RecipeTrayClass.)

            switch (TRACKAREA)
            {
                //case TrackArea.TrackPASS:
                //    btnFeed1.Visible = true;
                //    btnPut2.Visible = true;

                //    label5.Text = "轨道一";
                //    //pnlBindui.Visible = false;
                //    //lsDataViewUI1.Initial(TrackAreaPosition.LEFT_T1, RecipeTrayClass.Instance.TARCKASSIGN[(int)TrackAreaPosition.LEFT_T1]);
                //    break;
                case TrackArea.TrackINSPECT:
                    label5.Text = "轨道检测";
                    lblCurrent.Visible = true;
                    lblCount.Visible = true;
                    btnDown.Visible = true;
                    btnUp.Visible = true;

                    pnlLineScanArea.Visible = false;
                    btnTestLine.Visible = true;

                    btnFeed1.Visible = true;
                    btnPut1.Visible = true;

                    btnFeed2.Visible = true;
                    btnPut2.Visible = true;
                    lblputp.Visible = true;
                    lblputh.Visible = true;

                    btnLineTest2.Visible = true;
                    lblpctoplcSign.Visible = true;
                    lblplctopcSign.Visible = true;

                    //BarcodeClass.Forcepass = true;
                    //pnlBindui.Visible = false;
                    //lsDirUI1.Visible = true;

                    //lsDataViewUI2.Visible = true;
                    //lsDataViewUI1.Initial(TrackAreaPosition.LEFT_T2, RecipeTrayClass.Instance.TARCKASSIGN[(int)TrackAreaPosition.LEFT_T2]);
                    //lsDataViewUI2.Initial(TrackAreaPosition.LEFT_T2, RecipeTrayClass.Instance.TARCKASSIGN[(int)TrackAreaPosition.LEFT_T2]);
                    break;
                //case TrackArea.TrackNG1:
                //    label5.Text = "轨道三";
                //    btnFeed1.Visible = true;
                //    btnPut2.Visible = true;

                //    lsDataViewUI1.Initial(TrackAreaPosition.RIGHT_T3, RecipeTrayClass.Instance.TARCKASSIGN[(int)TrackAreaPosition.RIGHT_T3]);
                //    break;
                //case TrackArea.TrackNG2:
                //    label5.Text = "轨道四";
                //    btnFeed1.Visible = true;
                //    btnPut2.Visible = true;

                //    lsDataViewUI1.Initial(TrackAreaPosition.RIGHT_T4, RecipeTrayClass.Instance.TARCKASSIGN[(int)TrackAreaPosition.RIGHT_T4]);
                //    break;
            }
            ChangeRecipe();
            BarcodeClass.OnChangeState += BarcodeClass_OnChangeState;
        }

        private void BarcodeClass_OnChangeState(string statusstr)
        {
            string[] vs = statusstr.Split('$');
            switch (vs[0])
            {
                case "1":
                    SetBarcode(vs[1]);
                    //lblBarcode.Text = vs[1].Trim();
                    //lblBarcode.Refresh();
                    break;
                case "2":
                    lblSaveText.Text = vs[1].Trim();
                    //存储资料
                    //创建轨道路径
                    string channelpath = INI.Instance.HistoryDataPath + "\\" + INI.Instance.HistoryDataBarcode + "\\Channel0";

                    if (!Directory.Exists(channelpath))
                        Directory.CreateDirectory(channelpath);
                    this.SaveChannelData(channelpath);

                    this.ClearTrayUI();
                    break;
            }
        }
        public void Enable(bool isenabe)
        {
            panel6.Enabled = isenabe;
            panel2.Enabled = isenabe;
            panel5.Enabled = isenabe;
        }
        public void ChangeRecipe()
        {
            InitTrayUI();
            //TrayUI.Initial(myRecipe.TrayColCount,
            //                    myRecipe.TrayRowCount,
            //                    pnlChipPickUI.Width,
            //                    pnlChipPickUI.Height,
            //                    9,
            //                    "",
            //                    false);
            //SetTray();
            //switch (TRACKAREA)
            //{
            //    case TrackArea.TrackPASS:
            //        lsDataViewUI1.Initial(TrackAreaPosition.LEFT_T1, RecipeTrayClass.Instance.TARCKASSIGN[(int)TrackAreaPosition.LEFT_T1]);
            //        break;
            //    case TrackArea.TrackINSPECT:
            //        lsDataViewUI1.Initial(TrackAreaPosition.LEFT_T2, RecipeTrayClass.Instance.TARCKASSIGN[(int)TrackAreaPosition.LEFT_T2]);
            //        lsDataViewUI2.Initial(TrackAreaPosition.LEFT_T2, RecipeTrayClass.Instance.TARCKASSIGN[(int)TrackAreaPosition.LEFT_T2]);
            //        break;
            //    case TrackArea.TrackNG1:
            //        lsDataViewUI1.Initial(TrackAreaPosition.RIGHT_T3, RecipeTrayClass.Instance.TARCKASSIGN[(int)TrackAreaPosition.RIGHT_T3]);
            //        break;
            //    case TrackArea.TrackNG2:
            //        lsDataViewUI1.Initial(TrackAreaPosition.RIGHT_T4, RecipeTrayClass.Instance.TARCKASSIGN[(int)TrackAreaPosition.RIGHT_T4]);
            //        break;
            //}
        }
        public void ResetData()
        {
            lblBarcode.Text = string.Empty;
            lblSaveText.Text = string.Empty;
            lstLoadData.Items.Clear();
            lstSaveData.Items.Clear();
        }
        public void Tick()
        {
            label3.BackColor = (BarcodeClass.Forcepass ? Color.Yellow : Control.DefaultBackColor);
            BarcodeClass.Tick();
            btnRetryGetBarcode.Enabled = MACHINE.PLCIO.IsWarningGetbarcode;
            btnRetryBind.Enabled = MACHINE.PLCIO.IsWarningBindbarcode;
            btnRetryGetBarcode.BackColor = (MACHINE.PLCIO.IsWarningGetbarcode ? Color.Red : Control.DefaultBackColor);
            btnRetryBind.BackColor = (MACHINE.PLCIO.IsWarningBindbarcode ? Color.Red : Control.DefaultBackColor);
            switch (TRACKAREA)
            {
                //case TrackArea.TrackPASS:
                //    btnFeed1.BackColor = (Universal.Modules[(int)TrackModule.M1_FEED].Running ? Color.Red : Color.FromArgb(192, 255, 192));
                //    btnPut2.BackColor = (Universal.Modules[(int)TrackModule.M1_PUT].Running ? Color.Red : Color.FromArgb(192, 255, 192));

                //    lblfeedp.BackColor = (MACHINE.PLCIO.t1sr_ishavenoproduct ? Color.Lime : Control.DefaultBackColor);
                //    lblfeedh.BackColor = (MACHINE.PLCIO.t1sr_ishavenoproduct_notice ? Color.Red : Control.DefaultBackColor);
                //    lblp1.BackColor = (MACHINE.PLCIO.t1sr_ishavenoproduct_carrier_1 ? Color.Lime : Control.DefaultBackColor);
                //    lblp2.BackColor = (MACHINE.PLCIO.t1sr_ishavenoproduct_carrier_2 ? Color.Lime : Control.DefaultBackColor);
                //    lblputh.BackColor = (MACHINE.PLCIO.t1sr_isfullproduct ? Color.Red : Control.DefaultBackColor);
                //    //lsDataViewUI1.Tick();
                //    break;
                case TrackArea.TrackINSPECT:
                    btnFeed1.BackColor = (Universal.Modules[(int)TrackSingleModule.CHL_FEED1].Running ? Color.Red : Color.FromArgb(192, 255, 192));
                    btnPut2.BackColor = (Universal.Modules[(int)TrackSingleModule.CHL_PUT2].Running ? Color.Red : Color.FromArgb(192, 255, 192));

                    btnFeed2.BackColor = (Universal.Modules[(int)TrackSingleModule.CHL_FEED2].Running ? Color.Red : Color.FromArgb(192, 255, 192));
                    btnPut1.BackColor = (Universal.Modules[(int)TrackSingleModule.CHL_PUT1].Running ? Color.Red : Color.FromArgb(192, 255, 192));

                    //btnTestLine.BackColor = (Universal.Modules[(int)TrackModule.M2_LINE].Running ? Color.Red : Color.FromArgb(192, 255, 192));

                    lblfeedp.BackColor = (MACHINE.PLCIO.t2sr_ishavenoproduct ? Color.Lime : Control.DefaultBackColor);
                    lblfeedh.BackColor = (MACHINE.PLCIO.t2sr_ishavenoproduct_notice ? Color.Red : Control.DefaultBackColor);
                    lblp1.BackColor = (MACHINE.PLCIO.t2sr_ishavenoproduct_carrier_1 ? Color.Lime : Control.DefaultBackColor);
                    lblp2.BackColor = (MACHINE.PLCIO.t2sr_ishavenoproduct_carrier_2 ? Color.Lime : Control.DefaultBackColor);
                    lblputh.BackColor = (MACHINE.PLCIO.t2sr_isfullproduct ? Color.Red : Control.DefaultBackColor);
                    lblputp.BackColor = (MACHINE.PLCIO.t2sr_isfullnoproduct ? Color.Lime : Control.DefaultBackColor);
                    //lsDataViewUI1.Tick();
                    //lsDataViewUI2.Tick();

                    lblplctopcSign.BackColor = (MACHINE.PLCIO.ADR_LinePCToPlcSign ? Color.Lime : Control.DefaultBackColor);
                    lblpctoplcSign.BackColor = (MACHINE.PLCIO.ADR_LinePCToPlcSign2 ? Color.Lime : Control.DefaultBackColor);
                    lblplctopcSign.Text = "plc完成";
                    lblpctoplcSign.Text = "pc完成";
                    //lsDirUI1.Tick();

                    //pnlLineScanArea.BackColor = (INI.Instance.traydata_sim ? Color.Orange : Control.DefaultBackColor);
                    //lblCurrent.Text = m_List_index.ToString();
                    //lblCount.Text = "总:" + list.Count.ToString();
                    break;
                //case TrackArea.TrackNG1:
                //    btnFeed1.BackColor = (Universal.Modules[(int)TrackModule.M3_FEED].Running ? Color.Red : Color.FromArgb(192, 255, 192));
                //    btnPut2.BackColor = (Universal.Modules[(int)TrackModule.M3_PUT].Running ? Color.Red : Color.FromArgb(192, 255, 192));

                //    lblfeedp.BackColor = (MACHINE.PLCIO.t3sr_ishavenoproduct ? Color.Lime : Control.DefaultBackColor);
                //    lblfeedh.BackColor = (MACHINE.PLCIO.t3sr_ishavenoproduct_notice ? Color.Red : Control.DefaultBackColor);
                //    lblp1.BackColor = (MACHINE.PLCIO.t3sr_ishavenoproduct_carrier_1 ? Color.Lime : Control.DefaultBackColor);
                //    lblp2.BackColor = (MACHINE.PLCIO.t3sr_ishavenoproduct_carrier_2 ? Color.Lime : Control.DefaultBackColor);
                //    lblputh.BackColor = (MACHINE.PLCIO.t3sr_isfullproduct ? Color.Red : Control.DefaultBackColor);
                //    //lsDataViewUI1.Tick();
                //    break;
                //case TrackArea.TrackNG2:
                //    btnFeed1.BackColor = (Universal.Modules[(int)TrackModule.M4_FEED].Running ? Color.Red : Color.FromArgb(192, 255, 192));
                //    btnPut2.BackColor = (Universal.Modules[(int)TrackModule.M4_PUT].Running ? Color.Red : Color.FromArgb(192, 255, 192));

                //    lblfeedp.BackColor = (MACHINE.PLCIO.t4sr_ishavenoproduct ? Color.Lime : Control.DefaultBackColor);
                //    lblfeedh.BackColor = (MACHINE.PLCIO.t4sr_ishavenoproduct_notice ? Color.Red : Control.DefaultBackColor);
                //    lblp1.BackColor = (MACHINE.PLCIO.t4sr_ishavenoproduct_carrier_1 ? Color.Lime : Control.DefaultBackColor);
                //    lblp2.BackColor = (MACHINE.PLCIO.t4sr_ishavenoproduct_carrier_2 ? Color.Lime : Control.DefaultBackColor);
                //    lblputh.BackColor = (MACHINE.PLCIO.t4sr_isfullproduct ? Color.Red : Control.DefaultBackColor);
                //    //lsDataViewUI1.Tick();
                //    break;
            }
        }
        public void SetPLC(bool ison,bool islinescan)
        {
            pnlLineScanArea.BackColor = (islinescan ? Color.Red : Control.DefaultBackColor);
            pnlOutputArea.BackColor = (ison ? Color.Lime : Control.DefaultBackColor);
        }

        public void InitTrayUI()
        {
            TrayUI.Initial(myRecipe.TrayColCount,
                                myRecipe.TrayRowCount,
                                pnlChipPickUI.Width,
                                pnlChipPickUI.Height,
                                9,
                                "",
                                false);
            SetTray();
        }
        //public void SetTray(TrayClass tray)
        //{
        //    this.BackgroundImage = tray.bmpTray;
        //}
        public void SetTray()
        {
            pnlChipPickUI.BackgroundImage = TrayUI.bmpTray;
            //pnlChipPickUI.BackgroundImage = TrayUI.bmpTray90;
        }
        public void ClearTray()
        {
            pnlChipPickUI.BackgroundImage = null;
        }
        public void SetTrayUI(int col, int row, string su, int suIndexStr)
        {
            TrayUI.SetTrayUI(col, row, su, suIndexStr);
            SetTray();
        }
        public void ClearTrayUI()
        {
            TrayUI.ClearTrayUI();
            SetTray();
        }
        public void SetTrayBinUI(string binStr)
        {
            binStr = binStr.Replace("@", ",");
            TrayUI.SetBinString(binStr);
            TrayUI.DrawMap();
            SetTray();
        }
        public void SaveChannelData(string epath)
        {
            //if (!System.IO.Directory.Exists(epath))
            //    System.IO.Directory.CreateDirectory(epath);

            string filename = JetEazy.BasicSpace.JzTimes.DateTimeSerialStringFFF;
            if (lstLoadData.Items.Count > 0)
            {
                filename = lstLoadData.Items[0].ToString();
                lstLoadData.Items.RemoveAt(0);
            }

            //string dataStr = string.Empty;
            //dataStr += TrayUI.myrow.ToString() + Environment.NewLine;
            //dataStr += TrayUI.mycol.ToString() + Environment.NewLine;
            //dataStr += TrayUI.GetBinString() + Environment.NewLine;

            //SaveData(dataStr, epath + "\\" + filename + ".txt");
            //SaveData(dataStr, epath + "\\" +
            //    filename + "-C" + ((int)TRACKAREA).ToString() + "T" + lstSaveData.Items.Count.ToString("00") + ".txt");

            //MAPPINGDATA php查询
            //SaveData(dataStr, Universal.MAPPINGDATA + "\\" + filename + ".txt");
            ////加上盘号
            //SaveData(dataStr, Universal.MAPPINGDATA + "\\" +
            //    filename + "-C" + TRACKAREA.ToString() + "T" + lstSaveData.Items.Count.ToString("00") + ".txt");

            //JzToolsClass jzToolsClass = new JzToolsClass();
            //string _binResonStr = lstSaveData.Items.Count.ToString();// string.Empty;
            ////LSTestResult lSTestResult = LSTestResult.ERR_NONE;
            ////if (TrayUI.GetBinFrist() == -1)
            ////    lSTestResult = LSTestResult.ERR_NONE;
            ////else
            ////    lSTestResult = (LSTestResult)TrayUI.GetBinFrist();
            ////_binResonStr = jzToolsClass.GetEnumDescription(lSTestResult);

            //lstSaveData.Items.Add(filename + "-" + _binResonStr);
            lstSaveData.Items.Add(filename);
        }
        void SaveData(string DataStr, string FileName)
        {
            File.WriteAllText(FileName, DataStr, Encoding.Default);
        }

        //private bool IsCheckLinescanProcess()
        //{
        //    return m_LineScanProcess.IsOn;
        //}
    }
}
