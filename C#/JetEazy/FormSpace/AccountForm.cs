using JetEazy.BasicSpace;
using JetEazy.DBSpace;
using JetEazy.Lang;
using System;
using System.Windows.Forms;

namespace JetEazy.FormSpace
{
    public partial class AccountForm : Form
    {
        enum TagEnum
        {
            ADD,
            MODIFY,
            DEL,

            OK,
            CANCEL,
            EXIT,
        }

        #region LANGUAGE
        //Language
        JzLangPackage _lang;
        string _T(string text, string defaultText = null)
        {
            return _lang.Translate(text, defaultText);
        }
        #endregion

        #region LANGUAGE_OLD
        //JzLanguageClass myLanguage = new JzLanguageClass();
        string UIPath = "";
        //int LanguageIndex = 0;
        #endregion

        GroupBox grpACCData;
        ComboBox cboACCName;

        TextBox txtName;
        TextBox txtPassword;

        CheckBox chkAllowSetup;
        CheckBox chkAllowManageAccount;
        CheckBox chkAllowSetupRecipe;
        CheckBox chkAllowUseShopFloor;

        //Button btnAdd1;
        //Button btnModify1;
        //Button btnDelete1;
        //Button btnOK1;
        //Button btnCancel1;
        //Button btnExit1;

        bool IsNeedToChange;

        AccDBClass ACCDB;

        AccClass OperateDataNow
        {
            get
            {
                if (DBStatus == DBStatusEnum.ADD)
                    return ACCDB.AccLast;
                else
                    return ACCDB.AccList[cboACCName.SelectedIndex];
            }
        }

        public AccountForm(AccDBClass accdb, string uipath, int langindex)
        {
            _lang = new JzLangPackage("account_mgr", uipath);

            ACCDB = accdb;
            UIPath = uipath;
            //LanguageIndex = langindex;

            InitializeComponent();
            Initial();

            HandleCreated += (s, e) => _lang.Translate(this);
        }
        void Initial()
        {
            //myLanguage.Initial(UIPath + "\\AccountForm.jdb", LanguageIndex, this);

            grpACCData = groupBox1;
            cboACCName = comboBox1;
           
            txtName = textBox1;
            txtPassword = textBox2;

            chkAllowSetup = chkAllowSystemSetup;
            chkAllowManageAccount = chkAllowAccountManagement;
            chkAllowSetupRecipe = chkAllowRecipeEditting;
            chkAllowUseShopFloor = chkUseFactorySettings;
            
            //btnAddAccount = btnAddAccount;
            btnAddAccount.Tag = TagEnum.ADD;
            //btnModify = btnModify;
            btnModify.Tag = TagEnum.MODIFY;
            //btnDelAccount = btnDelAccount;
            btnDelAccount.Tag = TagEnum.DEL;
            //btnOK = btnOK;
            btnOK.Tag = TagEnum.OK;
            //btnCancel = btnCancel;
            btnCancel.Tag = TagEnum.CANCEL;
            //btnExit = btnExit;
            btnExit.Tag = TagEnum.EXIT;
            
            btnAddAccount.Click += new EventHandler(btn_Click);
            btnModify.Click += new EventHandler(btn_Click);
            btnDelAccount.Click += new EventHandler(btn_Click);
            btnOK.Click += new EventHandler(btn_Click);
            btnCancel.Click += new EventHandler(btn_Click);
            btnExit.Click+=new EventHandler(btn_Click);

            cboACCName.Items.Clear();

            foreach (AccClass acc in ACCDB.AccList)
            {
                cboACCName.Items.Add(acc.NAME);
            }

            cboACCName.SelectedIndexChanged += new EventHandler(cboACCName_SelectedIndexChanged);
            cboACCName.SelectedIndex = 0;

            IsNeedToChange = true;

            DBStatus = DBStatusEnum.NONE;

            FillDisplay();

            LanguageExClass.Instance.EnumControls(this);

            HandleCreated += (s, e) => _lang.Translate(this);
        }

        void cboACCName_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsNeedToChange)
            {
                FillDisplay();
            }
        }

        void btn_Click(object sender, EventArgs e)
        {
            TagEnum KEY = (TagEnum)((Button)sender).Tag;

            switch (KEY)
            {
                case TagEnum.ADD:
                    //ACCDB.Add(cboACCName.SelectedIndex);
                    ACCDB.Add();
                    DBStatus = DBStatusEnum.ADD;
                    FillDisplay();
 
                    break;
                case TagEnum.MODIFY:
                    DBStatus = DBStatusEnum.MODIFY;
                    break;
                case TagEnum.DEL:
                    if (VsMessageBox.Question(_T("是否刪除帳號?")) == DialogResult.Yes)
                    {
                        int cboLast = cboACCName.SelectedIndex;

                        ACCDB.Delete(cboLast);

                        IsNeedToChange = false;
                        cboACCName.Items.RemoveAt(cboLast);
                        IsNeedToChange = true;

                        if (cboLast == cboACCName.Items.Count)
                            cboLast--;

                        cboACCName.SelectedIndex = cboLast;

                        //WriteBack(false);
                    }
                    break;
                case TagEnum.CANCEL:
                    if (DBStatus == DBStatusEnum.ADD)
                        ACCDB.DeleteLast();

                    DBStatus = DBStatusEnum.NONE;
                    
                    FillDisplay();

                    break;
                case TagEnum.OK:

                    if (ACCDB.CheckDuplicate(txtName.Text, OperateDataNow.Index))
                    {
                        //MessageBox.Show(myLanguage.Messages("msg2", LanguageIndex), "SYS", MessageBoxButtons.OK);
                        VsMessageBox.Warning(_T("帳號重複!"));
                        txtName.Focus();
                        break;
                    }
                    WriteBack(true);

                    FillDisplay();
                    RefreshComboBox();

                    DBStatus = DBStatusEnum.NONE;
                    break;
                case TagEnum.EXIT:
                    this.Close();
                    break;
            }
        }
        
        void RefreshComboBox()
        {
            int cboLast = cboACCName.SelectedIndex;

            IsNeedToChange = false;

            cboACCName.Items.Clear();

            foreach (AccClass acc in ACCDB.AccList)
            {
                cboACCName.Items.Add(acc.NAME);
            }

            if (DBStatus == DBStatusEnum.ADD)
                cboACCName.SelectedIndex = cboACCName.Items.Count - 1;
            else
                cboACCName.SelectedIndex = cboLast;

            IsNeedToChange = true;
        }

        void WriteBack(bool IsWithChange)
        {
            if (IsWithChange)
            {
                OperateDataNow.NAME = txtName.Text;
                OperateDataNow.PASSWORD = txtPassword.Text;

                OperateDataNow.IsAllowSetupINI = chkAllowSetup.Checked;
                OperateDataNow.IsAllowManageAccount = chkAllowManageAccount.Checked;
                OperateDataNow.IsAllowSetupRecipe = chkAllowSetupRecipe.Checked;
                OperateDataNow.ISAllowUseShopFloor = chkAllowUseShopFloor.Checked;
            }

            ACCDB.Save();
        }

        void FillDisplay()
        {
            txtName.Text = OperateDataNow.NAME;
            txtPassword.Text = OperateDataNow.PASSWORD;

            chkAllowSetup.Checked = OperateDataNow.IsAllowSetupINI;
            chkAllowManageAccount.Checked = OperateDataNow.IsAllowManageAccount;
            chkAllowSetupRecipe.Checked = OperateDataNow.IsAllowSetupRecipe;
            chkAllowUseShopFloor.Checked = OperateDataNow.ISAllowUseShopFloor;

            //btnDelete.Enabled = !OperateDataNow.IsAllowSetupINI && !OperateDataNow.IsOperating(ACCDB.DataNow.No);
            //btnModify.Enabled = !OperateDataNow.IsSuperUser;
        }

        DBStatusEnum myDBStatus = DBStatusEnum.NONE;
        DBStatusEnum DBStatus
        {
            get
            {
                return myDBStatus;
            }
            set
            {
                myDBStatus = value;

                switch (myDBStatus)
                {
                    case DBStatusEnum.ADD:
                    case DBStatusEnum.MODIFY:
                        grpACCData.Enabled = true;
                        cboACCName.Visible = false;

                        btnAddAccount.Visible = false;
                        btnModify.Visible = false;
                        btnDelAccount.Visible = false;

                        btnOK.Visible = true;
                        btnCancel.Visible = true;

                        btnExit.Visible = false;

                        break;
                    case DBStatusEnum.NONE:
                        grpACCData.Enabled = false;
                        cboACCName.Visible = true;

                        btnAddAccount.Visible = true;
                        btnModify.Visible = true;
                        btnDelAccount.Visible = true;

                        btnOK.Visible = false;
                        btnCancel.Visible = false;

                        btnExit.Visible = true;
                        
                        break;
                }
            }
        }



    }
}