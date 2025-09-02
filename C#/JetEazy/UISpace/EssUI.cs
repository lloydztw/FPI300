using JetEazy.BasicSpace;
using JetEazy.DBSpace;
using JetEazy.FormSpace;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace JetEazy.UISpace
{
    public partial class EssUI : UserControl
    {
        enum TagEnum
        {
            LOGIN,
            LOGOUT,

            ACCOUNTMANAGEMENT,
            RECIPESELECTION,

            CLEARPASS,
            CLEARFAIL,

            FASTCAL,

            RUN,
            RECIPE,
            SETUP,

            GATEOPEN,
            GATECLOSE,

            PRESSUP,
            PRESSDOWN,

            LOAD,
            UNLOAD,
            ISUNMASK,
            RESET,

            CHANGERECIPE,
        }

        //Language Setup
        JzLanguageClass myLanguage = new JzLanguageClass();

        PictureBox picExit;

        Button btnLogin;
        Button btnLogout;

        Label lblDateTime;
        Label lblAccountName;
        Label lblMainStatus;
        Label lblRecipeName;

        Label lblPassCount;
        Label lblFailCount;

        Button btnAccountManagement;

        Button btnClearPass;
        Button btnClearFail;

        Button btnRun;
        Button btnRecipe;
        Button btnSetup;

        Button btnFastCal;

        Button btnReset;

        Label lblVer;

        ComboBox cboChangeRecipe;
        bool IsNeedToChange = true;
        /// <summary>
        /// 判断是否在RUN的状态 如果在则开始计时 否则停止计时
        /// </summary>
        bool m_IsRunState = false;
        bool m_IsRunTempState = false;
        Stopwatch m_RunWatch = new Stopwatch();
        int m_RunWatchTime = 30;

        public int RunWatchTime
        {
            get { return m_RunWatchTime; }
            set { m_RunWatchTime = value; }
        }

        JzTimes myTimes = new JzTimes();

        AccDBClass ACCDB;
        EsssDBClass ESSDB;
        string UIPath = "";
        int LanguageIndex = 0;

        VersionEnum VER = VersionEnum.STEROPES;
        OptionEnum OPT = OptionEnum.MAIN;

        int SamplingTimems = 1000;

        public EssUI()
        {
            InitializeComponent();
            SizeChanged += EssUI_SizeChanged;
        }

        public void Initial(EsssDBClass essdb,
            AccDBClass accdb,
            string uipath,
            int langindex,
            VersionEnum ver,
            OptionEnum opt,
            int samplingtimems)
        {
            ESSDB = essdb;
            ACCDB = accdb;
            UIPath = uipath;
            LanguageIndex = langindex;
            VER = ver;
            OPT = opt;

            myLanguage.Initial(UIPath + "\\ESSUI.jdb", LanguageIndex, this);

            picExit = pictureBox1;

            lblDateTime = label2;
            lblAccountName = label3;
            lblMainStatus = label4;
            lblRecipeName = label5;

            lblPassCount = label8;
            lblFailCount = label9;
            lblVer = label10;

            btnLogin = button2;
            btnLogin.Tag = TagEnum.LOGIN;
            btnLogout = button1;
            btnLogout.Tag = TagEnum.LOGOUT;

            btnAccountManagement = button3;
            btnAccountManagement.Tag = TagEnum.ACCOUNTMANAGEMENT;

            btnReset = button15;
            btnReset.Tag = TagEnum.RESET;

            btnClearPass = button5;
            btnClearPass.Tag = TagEnum.CLEARPASS;
            btnClearFail = button6;
            btnClearFail.Tag = TagEnum.CLEARFAIL;

            btnRun = button7;
            btnRun.Tag = TagEnum.RUN;
            btnRecipe = button8;
            btnRecipe.Tag = TagEnum.RECIPE;
            btnSetup = button9;
            btnSetup.Tag = TagEnum.SETUP;

            cboChangeRecipe = comboBox1;
            cboChangeRecipe.Tag = TagEnum.CHANGERECIPE;

            btnLogin.Click += new EventHandler(btn_Click);
            btnLogout.Click += new EventHandler(btn_Click);

            btnAccountManagement.Click += new EventHandler(btn_Click);

            btnClearPass.Click += new EventHandler(btn_Click);
            btnClearFail.Click += new EventHandler(btn_Click);

            btnRun.Click += new EventHandler(btn_Click);
            btnRecipe.Click += new EventHandler(btn_Click);
            btnSetup.Click += new EventHandler(btn_Click);

            btnReset.Click += new EventHandler(btn_Click);

            btnFastCal = button10;
            btnFastCal.Tag = TagEnum.FASTCAL;
            btnFastCal.Click += new EventHandler(btn_Click);

            cboChangeRecipe.SelectedIndexChanged += new EventHandler(cbo_SelectedIndexChanged);

            picExit.DoubleClick += new EventHandler(picExit_DoubleClick);
                        
            FillDisplay();

            SamplingTimems = samplingtimems;

            myTimes.Cut();
            //lblAccountName.Text = "";

            LOGINStatus = ESSStatusEnum.LOGOUT;
            MAINStatus = ESSStatusEnum.RUN;

            _auto_layout();
        }


        void cbo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!IsNeedToChange)
                return;

            TagEnum TAG = (TagEnum)((ComboBox)sender).Tag;

            switch (TAG)
            {
                case TagEnum.CHANGERECIPE:

                    string[] selectedrecipe = cboChangeRecipe.Text.Split(']');

                    selectedrecipe[0] = selectedrecipe[0].Replace("[", "");
                    ESSDB.LastRecipeIndex = int.Parse(selectedrecipe[0]);

                    ESSDB.Save();

                    OnTrigger(ESSStatusEnum.RECIPESELECTED);
                    break;
            }

        }
        public void Tick()
        {
            if (myTimes.msDuriation > SamplingTimems)
            {
                lblDateTime.Text = JzTimes.DateTimeString;

                myTimes.Cut();
            }

            switch (VER)
            {
                case VersionEnum.LASER:

                    switch (OPT)
                    {
                        case OptionEnum.MAIN_X1:
                            m_IsRunState = lblMainStatus.Text == "RUN";
                            if (m_IsRunState && m_RunWatchTime > 0)
                            {
                                if (m_RunWatchTime < 10)
                                    m_RunWatchTime = 10;

                                if (!m_IsRunTempState)
                                {
                                    if (!string.IsNullOrEmpty(lblAccountName.Text) && lblAccountName.Text.ToUpper() != "OP")
                                    {
                                        m_IsRunTempState = true;
                                        m_RunWatch.Restart();
                                    }
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(lblAccountName.Text) && lblAccountName.Text.ToUpper() != "OP")
                                    {
                                        if (lblAccountName.BackColor != Color.Yellow)
                                            lblAccountName.BackColor = Color.Yellow;
                                        else
                                            lblAccountName.BackColor = Color.FromArgb(255, 255, 192);

                                        if (m_RunWatch.ElapsedMilliseconds > m_RunWatchTime * 1000)
                                        {
                                            m_IsRunTempState = false;
                                            m_RunWatch.Stop();

                                            lblAccountName.BackColor = Color.FromArgb(255, 255, 192);

                                            ACCDB.Indicator = -1;
                                            LOGINStatus = ESSStatusEnum.LOGOUT;
                                            MAINStatus = ESSStatusEnum.RUN;

                                        }
                                    }
                                    else
                                    {
                                        m_IsRunTempState = false;
                                        m_RunWatch.Stop();

                                        lblAccountName.BackColor = Color.FromArgb(255, 255, 192);
                                    }
                                }
                            }
                            else
                            {
                                m_RunWatch.Stop();
                                m_IsRunTempState = false;
                                lblAccountName.BackColor = Color.FromArgb(255, 255, 192);
                            }

                            break;
                    }

                    break;
            }
        }
        public void SetLanguage(int langindex)
        {
            myLanguage.SetControlLanguage(this, langindex);
        }

        void picExit_DoubleClick(object sender, EventArgs e)
        {
            if (LOGINStatus != ESSStatusEnum.LOGOUT)
            {
                JetEazy.BasicSpace.VsMSG.Instance.Warning("請登出!\n回到跑線正常狀態,\n才能退出程式!");
                return;
            }

            if (JetEazy.BasicSpace.VsMSG.Instance.Question("是否要关闭系统？") == DialogResult.OK)
            //if (MessageBox.Show(myLanguage.Messages("msg1", LanguageIndex), "SYS", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                OnTrigger(ESSStatusEnum.EXIT);
            }
        }

        void btn_Click(object sender, EventArgs e)
        {
            TagEnum TAG = (TagEnum)((Button)sender).Tag;

            switch (TAG)
            {
                case TagEnum.LOGIN:
                    Login();
                    break;
                case TagEnum.LOGOUT:
                    Logout();
                    break;
                case TagEnum.ACCOUNTMANAGEMENT:
                    AccountManagement();
                    break;
                case TagEnum.CLEARPASS:
                    ClearPass();
                    break;
                case TagEnum.CLEARFAIL:
                    ClearFail();
                    break;
                case TagEnum.RUN:
                    MAINStatus = ESSStatusEnum.RUN;
                    OnTrigger(MAINStatus);
                    break;
                case TagEnum.RECIPE:
                    MAINStatus = ESSStatusEnum.RECIPE;
                    OnTrigger(MAINStatus);
                    break;
                case TagEnum.SETUP:
                    MAINStatus = ESSStatusEnum.SETUP;
                    OnTrigger(MAINStatus);
                    break;
                case TagEnum.FASTCAL:
                    OnTrigger(ESSStatusEnum.FASTCAL);
                    break;
                case TagEnum.LOAD:
                    OnTrigger(ESSStatusEnum.LOAD);
                    break;
                case TagEnum.UNLOAD:
                    OnTrigger(ESSStatusEnum.UNLOAD);
                    break;
                case TagEnum.RESET:
                    OnTrigger(ESSStatusEnum.RESET);
                    break;
            }
        }

        public void SetCheatTag(bool IsCheat)
        {
            btnClearFail.Text = (IsCheat ? "O" : "0");
        }

        public void ShowPLC_RxTime(string str)
        {
            lblVer.Text = str.Trim(',');
            lblVer.Refresh();
        }

        public void AutoLogin()
        {
            if (ACCDB.Check("admin", "888", true))
            {
                OnTrigger(ESSStatusEnum.LOGIN);
                LOGINStatus = ESSStatusEnum.LOGIN;
                MAINStatus = ESSStatusEnum.RUN;
                OnTrigger(ESSStatusEnum.LOGINCOMPLETE);
            }
        }

        //public void 

        #region Button Fuctions

        LoginForm LOGINFRM;
        void Login()
        {
            OnTrigger(ESSStatusEnum.LOGIN);

            LOGINFRM = new LoginForm(ACCDB, UIPath, LanguageIndex);

            if (LOGINFRM.ShowDialog() == DialogResult.OK)
            {
                LOGINStatus = ESSStatusEnum.LOGIN;
                MAINStatus = ESSStatusEnum.RUN;
            }

            LOGINFRM.Dispose();

            OnTrigger(ESSStatusEnum.LOGINCOMPLETE);
        }
        void Logout()
        {
            if (JetEazy.BasicSpace.VsMSG.Instance.Question("是否要登出账户？") == DialogResult.OK)
            //if (MessageBox.Show(myLanguage.Messages("msg2",LanguageIndex), "SYS", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                ACCDB.Indicator = -1;
                LOGINStatus = ESSStatusEnum.LOGOUT;
                MAINStatus = ESSStatusEnum.RUN;
            }
        }
        void ClearPass()
        {
            ESSDB.Reset(true);
            FillDisplay();
        }
        void ClearFail()
        {
            ESSDB.Reset(false);
            FillDisplay();
        }

        AccountForm ACCOUNTFRM;
        void AccountManagement()
        {
            OnTrigger(ESSStatusEnum.ACCOUNTMANAGE);

            ACCOUNTFRM = new AccountForm(ACCDB, UIPath, LanguageIndex);
            ACCOUNTFRM.ShowDialog();

            ACCOUNTFRM.Dispose();

            OnTrigger(ESSStatusEnum.ACCOUNTMANAGECOMPLETE);
        }

        #endregion

        public void FillDisplay()
        {
            lblPassCount.Text = ESSDB.PassCount.ToString();
            lblFailCount.Text = ESSDB.FailCount.ToString();
        }

        public void Set111(string filestr)
        {
            Bitmap bmp = new Bitmap(filestr);

            picExit.Image = new Bitmap(bmp);

            bmp.Dispose();
        }

        ESSStatusEnum myLOGINStatus = ESSStatusEnum.LOGOUT;
        ESSStatusEnum LOGINStatus
        {
            get
            {
                return myLOGINStatus;
            }
            set
            {
                myLOGINStatus = value;
                lblAccountName.Text = ACCDB.AccNow.NAME;

                switch (myLOGINStatus)
                {
                    case ESSStatusEnum.LOGIN:

                        btnLogin.Visible = false;
                        btnLogout.Visible = true;

                        btnAccountManagement.Enabled = ACCDB.AccNow.IsAllowManageAccount;

                        OnTrigger(ESSStatusEnum.LOGIN);

                        break;
                    case ESSStatusEnum.LOGOUT:

                        btnLogin.Visible = true;
                        btnLogout.Visible = false;

                        btnAccountManagement.Enabled = false;

                        OnTrigger(ESSStatusEnum.LOGOUT);

                        break;
                }
            }
        }

        ESSStatusEnum LastMainStatus = ESSStatusEnum.RUN;

        public bool Disable
        {
            set
            {
                this.Enabled = !value;

                if (value)
                {
                    if (MAINStatus == ESSStatusEnum.EDIT)
                        return;

                    LastMainStatus = MAINStatus;
                    MAINStatus = ESSStatusEnum.EDIT;
                }
                else
                {
                    MAINStatus = LastMainStatus;
                }
            }
        }
        public void FocusButton()
        {
            btnFastCal.Focus();
        }

        public void SetMainStatus(ESSStatusEnum status)
        {
            MAINStatus = status;
        }
        public ESSStatusEnum GetMainStatus()
        {
            return MAINStatus;
        }

        public void SetRecipeCombo(List<string> recipelist)
        {
            int i = 0;
            int selectedindex = 0;

            IsNeedToChange = false;

            cboChangeRecipe.Items.Clear();

            foreach (string str in recipelist)
            {
                string[] strs = str.Split('?');

                cboChangeRecipe.Items.Add(strs[0]);

                if (int.Parse(strs[1]) == ESSDB.LastRecipeIndex)
                    selectedindex = i;

                i++;
            }

            cboChangeRecipe.SelectedIndex = selectedindex;

            IsNeedToChange = true;
        }
        public void ChangeRecipe(int eRecipeNameIndex)
        {
            //IsNeedToChange = false;

            cboChangeRecipe.SelectedIndex = eRecipeNameIndex;

            //IsNeedToChange = true;
        }


        ESSStatusEnum myMAINStatus = ESSStatusEnum.RUN;
        ESSStatusEnum MAINStatus
        {
            get
            {
                return myMAINStatus;
            }
            set
            {
                myMAINStatus = value;

                OnTrigger(myMAINStatus);

                lblMainStatus.Text = myMAINStatus.ToString();

                switch (myMAINStatus)
                {
                    case ESSStatusEnum.RUN:
                        btnRun.BackColor = JzToolsClass.UsedColor;
                        btnRecipe.BackColor = JzToolsClass.NormalColor;
                        btnSetup.BackColor = JzToolsClass.NormalColor;

                        btnRun.Enabled = true;
                        btnRecipe.Enabled = ACCDB.AccNow.IsAllowSetupRecipe;
                        btnSetup.Enabled = ACCDB.AccNow.IsAllowSetupINI;
                        btnFastCal.Enabled = true;

                        break;
                    case ESSStatusEnum.RECIPE:
                        btnRun.BackColor = JzToolsClass.NormalColor;
                        btnRecipe.BackColor = JzToolsClass.UsedColor;
                        btnSetup.BackColor = JzToolsClass.NormalColor;

                        btnRun.Enabled = true;
                        btnRecipe.Enabled = true;
                        btnSetup.Enabled = ACCDB.AccNow.IsAllowSetupINI;
                        btnFastCal.Enabled = false;

                        break;
                    case ESSStatusEnum.SETUP:
                        btnRun.BackColor = JzToolsClass.NormalColor;
                        btnRecipe.BackColor = JzToolsClass.NormalColor;
                        btnSetup.BackColor = JzToolsClass.UsedColor;

                        btnRun.Enabled = true;
                        btnRecipe.Enabled = ACCDB.AccNow.IsAllowSetupRecipe;
                        btnSetup.Enabled = true;
                        btnFastCal.Enabled = false;

                        break;
                    case ESSStatusEnum.EDIT:
                        btnRun.Enabled = false;
                        btnRecipe.Enabled = false;
                        btnSetup.Enabled = false;
                        btnFastCal.Enabled = false;

                        break;
                }
            }
        }

        //當 MainStatus 變化時，產生OnTrigger
        public delegate void TriggerHandler(ESSStatusEnum status);
        public event TriggerHandler TriggerAction;
        public void OnTrigger(ESSStatusEnum status)
        {
            if (TriggerAction != null)
            {
                TriggerAction(status);
            }
        }


        #region AUTO_LAYOUT_FUNCTIONS
        void EssUI_SizeChanged(object sender, EventArgs e)
        {
            try
            {
                _auto_layout();
            }
            catch
            {

            }
        }

        void _auto_layout()
        {
#if OPT_LETIAN_AUTO_LAYOUT
            if (pictureBox1.Tag == null)
            {
                pictureBox1.BackgroundImageLayout = ImageLayout.Zoom;
                pictureBox1.Dock = DockStyle.Top;
                using (Bitmap bmp = new Bitmap(pictureBox1.BackgroundImage))
                {
                    pictureBox1.BackColor = bmp.GetPixel(2, 2);
                }
                pictureBox1.Tag = this;
            }
            //
            var rcc = ClientRectangle;
            int pad = 0;
            int w = (rcc.Width - pad * 4) / 3;
            int x = pad;

            //foreach (var c in new Control[] { btnLogin, lblAccountName, btnAccountManagement })
            foreach (var c in new Control[] { button2, label3, button3 })
            {
                c.Left = x;
                c.Width = w;
                x += w + pad;
            }
            button1.Location = button2.Location;
            button1.Size = button2.Size;

            //foreach (var c in new Control[] { lblMainStatus, cboChangeRecipe })
            foreach (var c in new Control[] { label4, comboBox1 })
            {
                c.Left = pad;
                c.Width = rcc.Width - pad * 2;
            }

            x = pad;
            //foreach (var c in new Control[] { btnRun, btnRecipe, btnSetup })
            foreach (var c in new Control[] { button7, button8, button9 })
            {
                c.Left = x;
                c.Width = w;
                x += w + pad;
            }

            label1.Left = pad;
            label1.Width = w;

            //lblDateTime
            label2.Left = label1.Right + pad;
            label2.Width = rcc.Width - label2.Left - pad;

            //lblPassCount;
            //lblFailCount;
#endif
        }
        #endregion
    }
}
