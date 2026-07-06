using JetEazy.BasicSpace;
using JetEazy.DBSpace;
using JetEazy.Lang;
using System;
using System.Linq.Expressions;
using System.Windows.Forms;

namespace JetEazy.FormSpace
{
    public partial class LoginForm : Form
    {
        enum TagEnum
        {
            NAME,
            PASSWORD,
        }

        TextBox txtName;
        TextBox txtPassword;

        //Button btnOK1;
        //Button btnCancel1;

        AccDBClass DataDB;


        #region LANGUAGE
        //Language
        JzLangPackage _lang;
        string _T(string text, string defaultText = null)
        {
            return _lang.Translate(text, defaultText);
        }
        #endregion

        #region LANGUAGE_OLD
        //Language Setup (old)
        string UIPath = "";
        int LanguageIndex = 0;
        JzLanguageClass myLanguage = new JzLanguageClass();
        #endregion

        //VsMessageBox MSGBOX = null;

        public LoginForm(AccDBClass accdb,string uipath,int langindex)
        {
            InitializeComponent();

            _lang = new JzLangPackage("account_mgr", uipath);

            DataDB = accdb;
            UIPath = uipath;
            LanguageIndex = langindex;
            Initial();

            HandleCreated += (s, e) => _lang.Translate(this);
        }
        
        void Initial()
        {
            myLanguage.Initial(UIPath + "\\LoginForm.jdb", LanguageIndex, this);

            txtName = textBox1;
            txtName.Tag = TagEnum.NAME;

            txtPassword = textBox2;
            txtPassword.Tag = TagEnum.PASSWORD;

            txtName.KeyDown += new KeyEventHandler(txt_KeyDown);
            txtPassword.KeyDown += new KeyEventHandler(txt_KeyDown);
            
            //btnOK = btnOK;
            //btnCancel = btnCancel;

            btnOK.Click += new EventHandler(btnOK_Click);
            btnCancel.Click += new EventHandler(btnCancel_Click);

            LanguageExClass.Instance.EnumControls(this);
        }

        void txt_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                TagEnum KEYS = (TagEnum)((TextBox)sender).Tag;

                switch (KEYS)
                {
                    case TagEnum.NAME:
                        txtPassword.Focus();
                        break;
                    case TagEnum.PASSWORD:
                        btnOK.PerformClick();
                        break;
                }
            }
        }

        void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        void btnOK_Click(object sender, EventArgs e)
        {
            if (DataDB.Check(txtName.Text.Trim(), txtPassword.Text.Trim(), true))
            {
                //JetEazy.LoggerClass.Instance.WriteLog("帐户登入: " + txtName.Text.Trim());
                this.DialogResult = DialogResult.OK;
            }
            else
            {

                //VsMSG.Instance.Warning("用户或密码错误，请重试。");
                
                VsMessageBox.Warning(_T("帳號或密碼錯誤!"));

                //MessageBox.Show(myLanguage.Messages("msg1", LanguageIndex), "SYS", MessageBoxButtons.OK);

                txtName.Focus();
                txtName.SelectAll();
            }
        }
    }
}
