using JetEazy.BasicSpace;
using JetEazy.Interface;
using JzDisplay;
using NeedleX.FormSpace;
using NeedleX.ProcessSpace;
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

namespace NeedleX.UISpace.MainSpace
{
    public partial class NeedleMainUI : UserControl
    {
        //ICam CAM0
        //{
        //    get { return Universal.CAMERAS[0]; }
        //}

        protected MachineCollectionClass MACHINECollection
        {
            get
            {
                return Universal.MACHINECollection;
            }
        }
        protected NeedleMachineClass MACHINE
        {
            get { return (NeedleMachineClass)Universal.MACHINECollection.MACHINE; }
        }

        Button btnAutoFocus;
        Button btnStart;
        Button btnStop;
        Button btnReset;
        Button btnClearAlarm;
        Button btnMute;
        Button btnManualAuto;

        public NeedleMainUI()
        {
            InitializeComponent();
        }
        public void Init()
        {
            init_Display();
            update_Display();
            CommonLogClass.Instance.SetRichTextBox(richTextBox1);

            btnAutoFocus = button2;
            btnAutoFocus.Click += BtnAutoFocus_Click;

            btnManualAuto = button1;

            btnStart = button6;
            btnStop = button4;
            btnReset = button7;

            btnStart.Click += BtnStart_Click;
            btnStop.Click += BtnStop_Click;
            btnReset.Click += BtnReset_Click;

            InitAllProcesses();
            StopAllProcesses("INIT");

        }

        BaseProcess m_BuzzerProcess
        {
            get { return BuzzerProcess.Instance; }
        }
        BaseProcess m_resetprocess
        {
            get { return ResetProcess.Instance; }
        }

        void StopAllProcesses(string reason = "")
        {
            m_resetprocess.Stop();

            switch (reason)
            {
                case "INIT":
                    break;
                case "USERSTOP":
                    m_BuzzerProcess.Stop();
                    break;
                default:
                    break;
            }
        }
        void InitAllProcesses()
        {
            //----------------------------------------------------------------
            // (1) 大部的 Processes 應該可以當成 MainProcess 的 Child Process,
            //      可以集中由 MainProcess 管理, 形成一體 Model.
            // (2) 以下對 Process Event Handler 的掛載.
            //      在 Model-View-Control 的架構規範下, 屬於 Control.
            //      ~ 以後再從 GUI(MainGdx3UI) 抽離出來.
            //----------------------------------------------------------------
            //m_mainprocess.OnCompleted += process_OnCompleted;
            // Buzzer 的結束 用來檢視是否有 NG 發生.
            m_BuzzerProcess.OnCompleted += buzzer_OnCompleted;
            m_resetprocess.OnCompleted += process_OnCompleted;
            
        }
        void TickAllProcesses()
        {
            m_resetprocess.Tick();
        }


        private void process_OnCompleted(object sender, ProcessEventArgs e)
        {
            if (sender == m_resetprocess)
            {
                if (m_resetprocess.RelateString == "CloseWindows")
                {
                    //執行的關閉流程 這裏則跳出
                    return;
                }
            }

            try
            {
                //if (sender == m_mainprocess)
                //{
                //    handle_main_process_completed(sender, e);
                //    if (e.Message == "PartialCompleted")
                //        return;
                //}

                // Do whatever message you want to show to the operators.
                string msg = $"程序 {((BaseProcess)sender).Name}, 已完成!\n";
                CommonLogClass.Instance.LogMessage(msg, Color.Black);
            }
            catch
            {
            }
        }
        private void buzzer_OnCompleted(object sender, ProcessEventArgs e)
        {
            if (InvokeRequired)
            {
                EventHandler<ProcessEventArgs> h = buzzer_OnCompleted;
                BeginInvoke(h, sender, e);
            }
            else
            {
                //if (m_mainprocess.LastNG != null)
                //{
                //    //>>> MessageBox.Show(m_mainprocess.LastNG);
                //    var errMsg = m_mainprocess.LastNG + "\n\r\n\r(後續可按 復位 排除NG態)";
                //    VsMSG.Instance.Warning(errMsg);
                //}
            }
        }
        private void handle_main_process_completed(object sender, ProcessEventArgs e)
        {
            if (InvokeRequired)
            {
                EventHandler<ProcessEventArgs> h = handle_main_process_completed;
                BeginInvoke(h, sender, e);
            }
            else
            {
                if (e.Message == "PartialCompleted")
                {
                    //var barcode = m_mainprocess.Barcode;
                    //var ngMsg = m_mainprocess.LastNG;
                    //int mirrorIdx = m_mainprocess.MainMirrorIndex;

                    //try
                    //{
                    //    var args = (object[])e.Tag;
                    //    mirrorIdx = (int)args[0];
                    //    ngMsg = (string)args[1];
                    //}
                    //catch (Exception ex)
                    //{
                    //    GdxGlobal.LOG.Warn(ex, "PartialCompleted Event 格式有誤!");
                    //    return;
                    //}

                    //var gen = new ZxReportGenerator();
                    //gen.GenerateReports(barcode, ngMsg, mirrorIdx);

                    //if (lblPassSign != null)
                    //{
                    //    lblPassSign.Text = (ngMsg == null) ? "PASS" : "NG";
                    //    lblPassSign.ForeColor = (ngMsg == null) ? Color.Green : Color.Red;
                    //}
                }
                else
                {
                    //// Final Completed 
                    //if (txtBarcode != null)
                    //    txtBarcode.Text = "";
                    //_sim_auto_barcode();
                }
            }
        }


        private void BtnReset_Click(object sender, EventArgs e)
        {
            string onStrMsg = "是否要进行复位？";
            string offStrMsg = "是否要停止复位流程？";
            string msg = (m_resetprocess.IsOn ? offStrMsg : onStrMsg);

            if (VsMSG.Instance.Question(msg) == DialogResult.OK)
            {
                if (!m_resetprocess.IsOn)
                {
                    m_resetprocess.Start();
                }
                else
                {
                    m_resetprocess.Stop();
                }
            }
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            
        }

        //frmAutoFocus FrmAutoFocus = null;
        private void BtnAutoFocus_Click(object sender, EventArgs e)
        {
            //if (!INI.Instance.IsOpenFocusWindows)
            //{
            //    INI.Instance.IsOpenFocusWindows = true;
            //    FrmAutoFocus = new frmAutoFocus();
            //    FrmAutoFocus.Show();
            //}
        }

        public void Tick()
        {
            //CAM0.Snap();
            //DS1.ReplaceDisplayImage(CAM0.GetSnap());

            btnReset.BackColor = (m_resetprocess.IsOn ? Color.Red : Color.FromArgb(192, 255, 192));
            btnManualAuto.BackColor = (MACHINE.PLCIO.ADR_ISAUTO_AND_MANUAL ? Color.Green : Color.FromArgb(192, 255, 192));
            btnManualAuto.Text = (MACHINE.PLCIO.ADR_ISAUTO_AND_MANUAL ? "自动" : "手动");

            TickAllProcesses();
        }
        public void SetEnable(bool isendable)
        {
            //pnlButtons.Enabled = isendable;
        }

        void init_Display()
        {
            DS1.Initial(100, 0.01f);
            DS1.SetDisplayType(DisplayTypeEnum.SHOW);
            DS2.Initial(100, 0.01f);
            DS2.SetDisplayType(DisplayTypeEnum.SHOW);
            DS3.Initial(100, 0.01f);
            DS3.SetDisplayType(DisplayTypeEnum.SHOW);
        }
        void update_Display()
        {
            DS1.Refresh();
            DS1.DefaultView();
            DS2.Refresh();
            DS2.DefaultView();
            DS3.Refresh();
            DS3.DefaultView();
        }

    }
}
