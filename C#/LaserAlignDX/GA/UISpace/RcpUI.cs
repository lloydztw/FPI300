using JetEazy;
using JetEazy.BasicSpace;
using JetEazy.DBSpace;
using JetEazy.FormSpace;
using LaserAlignDX.OPSpace.RecipeSpace;
using System;
using System.Windows.Forms;
using Traveller106;

//using FormRecipeEditor = LaserAlignDX.FormSpace.frmFPIRecipe;
using FormRecipeEditor = LaserAlignDX.Mvc.Gui.FormRecipeEditor;


namespace PhotoMachine.UISpace
{
    public partial class RcpUI : UserControl
    {
        enum TagEnum
        {
            ADD,
            MODIFY,

            OK,
            CANCEL,

            DETIAL,
        }

        //VIEWClass VIEW;
        RCPDBClass RCPDB;
        RCPItemClass RCPItemNow
        {
            get
            {
                return RCPDB.RCPItemNow;
            }
        }

        GroupBox grpRcpData;

        TextBox txtName;
        TextBox txtVersion;

        //StpUI STPUI;

        Button btnAdd;
        Button btnModify;

        Button btnOK;
        Button btnCancel;

        Button btnDetial;

        Label lblModifyDateTime;

        RichTextBox rtbComment;
        
        //Language Setup
        JzLanguageClass myLanguage = new JzLanguageClass();

        string UIPath = "";
        int LanguageIndex = 0;

        VersionEnum VER = VersionEnum.STEROPES;
        OptionEnum OPT = OptionEnum.MAIN;


        //IRecipe IxRecipe
        //{
        //    get { return RecipeCHClass.Instance; }
        //}
        protected RecipeFPIX3Class xRecipe
        {
            get { return RecipeFPIX3Class.Instance; }
        }

        JetEazy.FormSpace.VsMessageBox vsMessageBox = null;

        public RcpUI()
        {
            InitializeComponent();
            SizeChanged += RcpUI_SizeChanged;
        }

        public void Initial(string uipath,
            int langindex,
            VersionEnum ver,
            OptionEnum opt,
            RCPDBClass rcpdb)
            //VIEWClass view)
        {
            RCPDB = rcpdb;
            //VIEW = view;

            UIPath = uipath;
            LanguageIndex = langindex;
            VER = ver;
            OPT = opt;

            myLanguage.Initial(UIPath + "\\RcpUI.jdb", LanguageIndex, this);

            grpRcpData = groupBox1rcp;
            rtbComment = richTextBox1rcp;

            txtName = textBox1rcp;
            txtVersion = textBox2Rcp;

            lblModifyDateTime = label4rcp;

            btnAdd = button1rcp;
            btnModify = button2rcp;
            btnOK = button4rcp;
            btnCancel = button6rcp;
            btnDetial = button3rcp;

            btnAdd.Tag = TagEnum.ADD;
            btnModify.Tag = TagEnum.MODIFY;
            btnOK.Tag = TagEnum.OK;
            btnCancel.Tag = TagEnum.CANCEL;
            btnDetial.Tag = TagEnum.DETIAL;


            btnAdd.Click += new EventHandler(btn_Click);
            btnModify.Click += new EventHandler(btn_Click);
            btnOK.Click += new EventHandler(btn_Click);
            btnCancel.Click += new EventHandler(btn_Click);
            btnDetial.Click += new EventHandler(btn_Click);
            btnDel.Click += BtnDel_Click;
            
            //SizeChanged += RcpUI_SizeChanged;

            //STPUI = stpUI1;
            //STPUI.Initial(UIPath,LanguageIndex,VER,OPT,VIEW);
            //STPUI.TriggerAction += new StpUI.TriggerHandler(STPUI_TriggerAction);
            //STPUI.TriggerActionForSetupDetail += new StpUI.TriggerHandlerForSetupDetail(STPUI_TriggerActionForSetupDetail);

            FillDisplay(true);
            DBStatus = DBStatusEnum.NONE;

            //HandleCreated += (s, e) => LtAoiFactory.RcpCheckActive();
            BeginInvoke(new Action(() => LtAoiFactory.RcpCheckActive()));
        }

        private void BtnDel_Click(object sender, EventArgs e)
        {
            Delete();
        }
        void Delete()
        {
            if (RCPItemNow.Index == 0)
            {
                JetEazy.BasicSpace.VsMSG.Instance.Warning("系统参数无法删除！");
                return;
            }

            if (JetEazy.BasicSpace.VsMSG.Instance.Question("是否要删除参数？") == DialogResult.OK)
            {
                int i = 0;

                foreach (RCPItemClass rcpitem in RCPDB.RCPItemList)
                {
                    if (rcpitem.Index == RCPDB.Indicator)
                    {
                        var item = RCPDB.RCPItemList[i];
                        RCPDB.RCPItemList.RemoveAt(i);
                        LtAoiFactory.RcpDelete(item.Name);
                        break;
                    }
                    i++;
                }

                if (RCPDB.RCPItemList.Count == i)
                {
                    RCPDB.Indicator = RCPDB.RCPItemLast.Index;
                }
                else
                {
                    RCPDB.Indicator = RCPDB.RCPItemList[i].Index;
                }
                FillDisplay(true);

                OnTrigger(RCPStatusEnum.MODIFYCOMPLETE);
                OnTrigger(RCPStatusEnum.DELETE);
            }
        }

        void STPUI_TriggerActionForSetupDetail(RCPStatusEnum status, int setupindex)
        {
            switch (status)
            {
                default:
                    OnTrigger(status, setupindex);
                    break;
            }
        }
        void STPUI_TriggerAction(RCPStatusEnum status)
        {
            switch (status)
            {
                default:
                    OnTrigger(status);
                    break;
            }
        }


        void btn_Click(object sender, EventArgs e)
        {
            TagEnum KEYS = (TagEnum)((Button)sender).Tag;

            switch (KEYS)
            {
                case TagEnum.ADD:
                    AddAndCopy(false);
                    break;
                case TagEnum.MODIFY:
                    Modify();
                    break;
                case TagEnum.OK:
                    ModifyComplete();
                    break;
                case TagEnum.CANCEL:
                    ModifyCancel();
                    break;
                case TagEnum.DETIAL:
                    showRecipeDialogWindow();
                    break;
            }
        }
        void AddAndCopy(bool IsCopy)
        {
            OnTrigger(RCPStatusEnum.EDIT);

            RCPDB.AddAndCopy(IsCopy);
            FillDisplay(false || !IsCopy);

            DBStatus = DBStatusEnum.ADD;
        }
        void Modify()
        {
            OnTrigger(RCPStatusEnum.EDIT);

            RCPDB.Backup();
            DBStatus = DBStatusEnum.MODIFY;
        }
        void ModifyComplete()
        {
            vsMessageBox = new VsMessageBox($"保存参数中请稍后...", false);
            vsMessageBox.Show();
            vsMessageBox.Refresh();

            if (RCPDB.CheckDuplicate(txtName.Text.Trim() + txtVersion.Text.Trim(), RCPItemNow.Index))
            {

                JetEazy.BasicSpace.VsMSG.Instance.Warning("名称或版本已存在，请检查。");

                //MessageBox.Show(myLanguage.Messages("msg1", INI.LANGUAGE), "SYS", MessageBoxButtons.OK);
                txtName.Focus();
            }
            else
            {
                WriteBack(true);

                LtAoiFactory.RcpRename(RCPItemNow.Name);
                
                //VIEW.Save();

                //STPUI.ModifyComplete();

                //STPUI.ResetcboSetup();
                if (DBStatus == DBStatusEnum.ADD)
                {
                    //RecipeTrayClass.Instance.ChangeIndex(RCPDB.Indicator);
                    xRecipe.ChangeIndex(RCPDB.Indicator);
                }

                //RecipeCHClass.Instance.Save();
                //RecipeTrayClass.Instance.Save();
                //VisionTrayClass.Instance.Save();
                xRecipe.Save();
                FillDisplay(true);

                OnTrigger(RCPStatusEnum.MODIFYCOMPLETE);
                DBStatus = DBStatusEnum.NONE;
            }

            vsMessageBox.Close();
            vsMessageBox.Dispose();
        }
        void ModifyCancel()
        {
            vsMessageBox = new VsMessageBox($"取消中请稍后...", false);
            vsMessageBox.Show();
            vsMessageBox.Refresh();

            if (DBStatus == DBStatusEnum.ADD)
            {
                RCPDB.DeleteLast();
                //RecipeTrayClass.Instance.ChangeIndex(RCPDB.Indicator);
                xRecipe.ChangeIndex(RCPDB.Indicator);
            }
            else
                RCPDB.Restore();

            //VIEW.Load();
            ////VIEW.Train();

            //STPUI.ModifyCancel();

            //RecipeCHClass.Instance.Load();
            //RecipeTrayClass.Instance.Load();
            //VisionTrayClass.Instance.Load();
            xRecipe.ChangeIndex(RCPDB.Indicator);
            xRecipe.Load(true);
            FillDisplay(true);

            //STPUI.ResetcboSetup();

            OnTrigger(RCPStatusEnum.MODIFYCANCEL);
            DBStatus = DBStatusEnum.NONE;

            vsMessageBox.Close();
            vsMessageBox.Dispose();
        }

        void WriteBack(bool IsWithChange)
        {
            if (IsWithChange)
            {
                RCPItemNow.Name = txtName.Text;
                RCPItemNow.Version = txtVersion.Text;
                RCPItemNow.Comment = rtbComment.Text.Trim();

                RCPItemNow.ModifyDateTime = JzTimes.DateTimeString;
            }

            RCPDB.Save();
        }
        public void ChangeRecipe(bool IsLoad) //IsLoad is judge is the image is from file or memory
        {
            FillDisplay(IsLoad);

            // 檢查空盤參數是否存在 !!!
            LtAoiFactory.RcpCheckActive();
        }
        void FillDisplay(bool IsLoad)   //IsLoad is judge is the image is from file or memory
        {
            txtName.Text = RCPItemNow.Name;
            txtVersion.Text = RCPItemNow.Version;

            lblModifyDateTime.Text = RCPItemNow.ToModifyString();

            txtName.ReadOnly = RCPItemNow.Index == 0;
            txtVersion.ReadOnly = RCPItemNow.Index == 0;

            rtbComment.Text = RCPItemNow.Comment;

            //if (IsLoad)
            //    STPUI.ResetcboSetup();
            LtAoiFactory.RcpSetActive(RCPItemNow?.Name);

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

                        grpRcpData.Enabled = true;

                        btnAdd.Visible = false;
                        btnModify.Visible = false;
                        btnDel.Visible = false;

                        btnOK.Visible = true;
                        btnCancel.Visible = true;

                        break;
                    case DBStatusEnum.NONE:
                        grpRcpData.Enabled = false;

                        btnAdd.Visible = true;
                        btnModify.Visible = true;
                        btnDel.Visible = true;

                        btnOK.Visible = false;
                        btnCancel.Visible = false;

                        break;
                }
            }
        }

        public delegate void TriggerHandler(RCPStatusEnum status);
        public event TriggerHandler TriggerAction;
        public void OnTrigger(RCPStatusEnum status)
        {
            if (TriggerAction != null)
            {
                TriggerAction(status);
            }
        }

        public delegate void TriggerHandlerForSetupDetail(RCPStatusEnum status, int setupindex);
        public event TriggerHandlerForSetupDetail TriggerActionForSetupDetail;
        public void OnTrigger(RCPStatusEnum status, int setupindex)
        {
            if (TriggerActionForSetupDetail != null)
            {
                TriggerActionForSetupDetail(status, setupindex);
            }
        }

        //frmRecipe frmRecipeSetup = null;
        //frmX2Recipe frmRecipeSetup = null;
        //frmFPIRecipe frmRecipeSetup = null;
        void showRecipeDialogWindow()
        {
            using (var frmRecipeSetup = new FormRecipeEditor())
            {
                frmRecipeSetup.ShowDialog();
            }

            //int mode = -1;
            //using (var frm = new frmSelectScanInspectMode())
            //{
            //    if (frm.ShowDialog() != DialogResult.OK)
            //        return;
            //    mode = frm.SelectScanMode;
            //}

            //if (mode == 2)
            //{
            //    openEmptyTrayInspectorTool();
            //}
            //else
            //{
            //    using (var frmRecipeSetup = new frmFPIRecipe())
            //    {
            //        frmRecipeSetup.ShowDialog();
            //    }
            //}

            //frmRecipeSetup = new frmFPIRecipe();
            //frmRecipeSetup.ShowDialog();
            //frmRecipeSetup.Dispose();
            //frmRecipeSetup = null;

            //using (var frm = new frmRecipe())
            //{
            //    frm.ShowDialog();
            //}
        }

        //void openEmptyTrayInspectorTool()
        //{
        //    var frm = FindForm();
        //    LtAoiFactory.OpenEmptyTrayInspectorTool(frm);
        //}

        #region AUTO_LAYOUT
        void RcpUI_SizeChanged(object sender, EventArgs e)
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

            groupBox1rcp.Dock = DockStyle.Top;
            richTextBox1rcp.Dock = DockStyle.Bottom;

            var rcc = ClientRectangle;
            int pad = 3;
            int w = (rcc.Width - pad * 6) / 3;
            int x = pad * 2;

            //var ctrls = new Control[] { btnAdd, btnModify, btnCancel };
            var ctrls = new Control[] { button1rcp, button2rcp,btnDel };
            foreach (var c in ctrls)
            {
                c.Top = rcc.Bottom - c.Height - pad * 2;
                c.Left = x;
                c.Width = w;
                x += w + pad;
            }
            // btnOK
            button4rcp.Location = button2rcp.Location;
            button4rcp.Size = button2rcp.Size;

            // btnCancel
            button6rcp.Location = btnDel.Location;
            button6rcp.Size = btnDel.Size;

            // lblModifyDateTime
            label4rcp.Top = button4rcp.Top - label4rcp.Height - pad;
            label4rcp.Left = groupBox1rcp.Left;
            label4rcp.Width = rcc.Width - label4rcp.Left * 2;
            // groupBox1
            groupBox1rcp.Height = (label4rcp.Top - pad * 2) - groupBox1rcp.Top;
            // richbox1
            richTextBox1rcp.Height = richTextBox1rcp.Bottom - button3rcp.Bottom - pad * 2;
            // btnDetail
            rcc = groupBox1rcp.ClientRectangle;
            button3rcp.Left = rcc.Right - button3rcp.Width - pad * 5;
            textBox2Rcp.Width = button3rcp.Right - textBox2Rcp.Left;
#endif
        }
        #endregion
    }
}
