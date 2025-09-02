using JetEazy.BasicSpace;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace JetEazy.FormSpace
{
    public partial class VsMessageBox : Form
    {

        Label lblMessageText;
        Button btnOK;
        Button btnCancel;

        public VsMessageBox(string strMessage, bool iswarning=false)
        {
            InitializeComponent();
            Init(strMessage, iswarning);
        }
        public VsMessageBox(string strMessage, string str = "1")
        {
            InitializeComponent();
            InitTishi(strMessage, true);
        }

        void Init(string eStrMsg,bool iswarning=false)
        {
            this.Text = (iswarning ? "警告视窗" : "询问视窗");
            panel1.BackColor = (iswarning ? Color.Red : Color.FromArgb(255, 255, 192));

            lblMessageText = label1;
            btnCancel = button1;
            btnOK = button2;

            //lblMessageText.Text = "提示信息:" + Environment.NewLine;
            //lblMessageText.Text += eStrMsg;

            lblMessageText.Text = eStrMsg;

            btnOK.Click += BtnOK_Click;
            btnCancel.Click += BtnCancel_Click;

            //btnOK.Text = "确定(OK)";
            //btnCancel.Text = "取消(Cancel)";

            btnOK.Text = "确定";
            btnCancel.Text = "取消";

            this.TopMost = true;

            if(iswarning)
            {
                btnCancel.Visible = false;
                btnOK.Location = new Point(btnCancel.Location.X, btnCancel.Location.Y);
            }

            LanguageExClass.Instance.EnumControls(this);

        }
        void InitTishi(string eStrMsg, bool iswarning = true)
        {
            

            this.Text = (iswarning ? "提示视窗" : "询问视窗");
            //panel1.BackColor = (iswarning ? Color.Red : Color.FromArgb(255, 255, 192));

            panel1.BackColor = Color.FromArgb(255, 255, 192);

            lblMessageText = label1;
            btnCancel = button1;
            btnOK = button2;

            lblMessageText.Text = "提示信息:" + Environment.NewLine;
            lblMessageText.Text = eStrMsg;

            btnOK.Click += BtnOK_Click;
            btnCancel.Click += BtnCancel_Click;

            //btnOK.Text = "确定(OK)";
            //btnCancel.Text = "取消(Cancel)";

            btnOK.Text = "确定";
            btnCancel.Text = "取消";

            this.TopMost = true;

            if (iswarning)
            {
                btnCancel.Visible = false;
                btnOK.Location = new Point(btnCancel.Location.X, btnCancel.Location.Y);
            }

            LanguageExClass.Instance.EnumControls(this);

        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }
    }
}
