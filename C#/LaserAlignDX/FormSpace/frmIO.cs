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
using Traveller106.UISpace.IOSpace;
using VsCommon.ControlSpace;
using VsCommon.ControlSpace.MachineSpace;

namespace Traveller106.FormSpace
{
    public partial class frmIO : Form
    {

        Button btnIO00;
        Button btnIO01;
        Button btnIO02;
        Button btnIO03;
        Button btnIO04;
        Button btnExit;

        LSIOUI mLsIoUI;

        Timer tIOTimer = null;
        int m_IndexIO = 0;

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

        public frmIO()
        {
            InitializeComponent();

            this.Load += FrmIO_Load;
            this.FormClosed += FrmIO_FormClosed;
        }

        private void FrmIO_FormClosed(object sender, FormClosedEventArgs e)
        {
            INI.Instance.IsOpenIOWindows = false;
        }

        private void FrmIO_Load(object sender, EventArgs e)
        {
            Init();
        }

        void Init()
        {
            this.Text = "监控IO窗体";

            mLsIoUI = lsioui1;

            btnIO00 = button1;
            btnIO01 = button2;
            btnIO02 = button3;
            btnIO03 = button4;
            btnIO04 = button5;
            btnExit = button6;

            btnIO00.Text = "輸入輸出狀態";
            btnIO01.Text = "嚴重報警狀態";
            btnIO02.Text = "常規報警狀態";
            btnIO03.Text = "IO模组三\nIX12.0-IX13.7\nQX12.0-QX13.7";
            btnIO04.Text = "IO模组四\nIX14.0-IX15.7\nQX14.0-QX15.7";

            btnIO03.Visible = false;
            label4.Visible = false;
            btnIO04.Visible = false;
            label5.Visible = false;

            btnExit.Click += BtnExit_Click;

            btnIO00.Click += BtnIO00_Click;
            btnIO01.Click += BtnIO01_Click;
            btnIO02.Click += BtnIO02_Click;
            btnIO03.Click += BtnIO03_Click;
            btnIO04.Click += BtnIO04_Click;

            tIOTimer=new Timer();
            tIOTimer.Interval = 50;
            tIOTimer.Enabled = true;
            tIOTimer.Tick += TIOTimer_Tick;

            _Ls_io_ui_init();
        }

        private void BtnIO04_Click(object sender, EventArgs e)
        {
            m_IndexIO = 4; _Ls_io_ui_init();
        }

        private void BtnIO03_Click(object sender, EventArgs e)
        {
            m_IndexIO = 3; _Ls_io_ui_init();
        }

        private void BtnIO02_Click(object sender, EventArgs e)
        {
            m_IndexIO = 2; _Ls_io_ui_init();
        }

        private void BtnIO01_Click(object sender, EventArgs e)
        {
            m_IndexIO = 1; _Ls_io_ui_init();
        }

        private void BtnIO00_Click(object sender, EventArgs e)
        {
            m_IndexIO = 0; _Ls_io_ui_init();
        }

        private void TIOTimer_Tick(object sender, EventArgs e)
        {
            btnIO00.BackColor = ((m_IndexIO == 0) ? Color.Green : Control.DefaultBackColor);
            btnIO01.BackColor = ((m_IndexIO == 1) ? Color.Green : Control.DefaultBackColor);
            btnIO02.BackColor = ((m_IndexIO == 2) ? Color.Green : Control.DefaultBackColor);
            btnIO03.BackColor = ((m_IndexIO == 3) ? Color.Green : Control.DefaultBackColor);
            btnIO04.BackColor = ((m_IndexIO == 4) ? Color.Green : Control.DefaultBackColor);

            mLsIoUI.Tick();
        }

        private void BtnExit_Click(object sender, EventArgs e)
        {
            tIOTimer.Enabled = false;
            this.Close();
        }

        void _Ls_io_ui_init()
        {
            mLsIoUI.Initial(Universal.WORKPATH + "\\" + Machine_EA.MAIN_MINIX6.ToString() + "\\IOUI\\IOIXQX_" + m_IndexIO.ToString() + ".csv", MACHINE);
        }
    }
}
