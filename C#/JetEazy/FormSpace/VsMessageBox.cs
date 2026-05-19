#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-09-17 重整 (by LeTian Chang)
 *
 */
#endregion

using JetEazy.BasicSpace;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace JetEazy.FormSpace
{
    public partial class VsMessageBox : Form
    {
        #region GUI_LINKS
        public Label lblMessage => label1;
        public Button btnCancel => button1;
        public Button btnOK => button2;
        public Control majorPanel => panel1;
        #endregion

        /// <summary>
        /// 舊接口
        /// </summary>
        public VsMessageBox(string message, bool isWarningOrQuestion) : this()
        {
            initWarningOrQuestion(message, isWarningOrQuestion);
        }
        /// <summary>
        /// 舊接口
        /// </summary>
        public VsMessageBox(string message, string dummy = "1") : this()
        {
            initInfoOrQuestion(message, true);
        }
        /// <summary>
        /// Default Constructor
        /// </summary>
        protected VsMessageBox()
        {
            InitializeComponent();
            btnOK.Text = "确定";
            btnCancel.Text = "取消";
            btnCancel.Click += BtnCancel_Click;
            btnOK.Click += BtnOK_Click;
            Load += VsMessageBox_Load;
            this.TopMost = true;
        }

        #region PRIVATE_FUNCTIONS
        void initWarningOrQuestion(string message, bool isWarning = false)
        {
            //this.Text = (isWarning ? "警告视窗" : "询问视窗");

            //panel1.BackColor = (isWarning ? Color.HotPink : Color.FromArgb(255, 255, 192));

            //lblMessageText.Text = message;

            ////btnOK.Click += BtnOK_Click;
            ////btnCancel.Click += BtnCancel_Click;
            ////btnOK.Text = "确定(OK)";
            ////btnCancel.Text = "取消(Cancel)";
            ////btnOK.Text = "确定";
            ////btnCancel.Text = "取消";
            ////this.TopMost = true;

            //if (isWarning)
            //{
            //    btnCancel.Visible = false;
            //    btnOK.Location = new Point(btnCancel.Location.X, btnCancel.Location.Y);
            //    btnOK.BackColor = SystemColors.ButtonFace;
            //}

            ////LanguageExClass.Instance.EnumControls(this);
            ///
            init(message, isWarning ? MessageBoxIcon.Warning : MessageBoxIcon.Question);
        }
        void initInfoOrQuestion(string message, bool isInfo = true)
        {
            //this.Text = (isInfo ? "提示视窗" : "询问视窗");

            ////panel1.BackColor = (iswarning ? Color.Red : Color.FromArgb(255, 255, 192));
            //panel1.BackColor = Color.FromArgb(255, 255, 192);

            ////lblMessageText = label1;
            ////btnCancel = button1;
            ////btnOK = button2;

            ////lblMessageText.Text = "提示信息:" + Environment.NewLine;
            //lblMessageText.Text = message;

            ////btnOK.Click += BtnOK_Click;
            ////btnCancel.Click += BtnCancel_Click;
            ////btnOK.Text = "确定(OK)";
            ////btnCancel.Text = "取消(Cancel)";
            ////btnOK.Text = "确定";
            ////btnCancel.Text = "取消";
            ////this.TopMost = true;

            //if (isInfo)
            //{
            //    btnCancel.Visible = false;
            //    btnOK.Location = new Point(btnCancel.Location.X, btnCancel.Location.Y);
            //}

            ////LanguageExClass.Instance.EnumControls(this);
           
            init(message, isInfo ? MessageBoxIcon.Information : MessageBoxIcon.Question);
        }
        void init(string msg, MessageBoxIcon icon)
        {
            if (icon == MessageBoxIcon.Warning)
            {
                this.Text = "警告视窗";
                this.lblMessage.Text = msg;
                pictureBox1.BackgroundImage = Properties.Resources.exclamation;
                panel1.BackColor = Color.HotPink;
                btnCancel.Visible = false;
                btnOK.Location = new Point(btnCancel.Location.X, btnCancel.Location.Y);
                btnOK.BackColor = SystemColors.ButtonFace;
            }
            else if (icon == MessageBoxIcon.Information)
            {
                this.Text = "提示视窗";
                this.lblMessage.Text = msg;
                pictureBox1.BackgroundImage = Properties.Resources.information;
                panel1.BackColor = Color.Ivory;
                btnCancel.Visible = false;
                btnOK.Location = new Point(btnCancel.Location.X, btnCancel.Location.Y);
            }
            else
            {
                this.Text = "询问视窗";
                this.lblMessage.Text = msg;
                pictureBox1.BackgroundImage = Properties.Resources.question;
                panel1.BackColor = Color.Ivory;
                btnOK.Text = "Yes";
                btnCancel.Text = "No";
            }
        }
        #endregion

        #region EVENT_HANDLERS
        private void VsMessageBox_Load(object sender, EventArgs e)
        {
            LanguageExClass.Instance.EnumControls(this);
        }
        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
        private void BtnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        #endregion

        public static DialogResult Info(string message)
        {
            using(var dlg = new VsMessageBox())
            {
                dlg.init(message, MessageBoxIcon.Information);
                return dlg.ShowDialog();
            }
        }
        public static DialogResult Warning(string message)
        {
            using (var dlg = new VsMessageBox())
            {
                dlg.init(message, MessageBoxIcon.Exclamation);
                return dlg.ShowDialog();
            }
        }
        public static DialogResult Question(string message, Color? bkColor = null)
        {
            using (var dlg = new VsMessageBox())
            {
                dlg.init(message, MessageBoxIcon.Question);
                if (bkColor != null)
                    dlg.panel1.BackColor = bkColor.Value;
                return dlg.ShowDialog();
            }
        }
    }
}
