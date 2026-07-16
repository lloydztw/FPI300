using JetEazy;
using JetEazy.BasicSpace;
using JetEazy.DBSpace;
using JetEazy.FormSpace;
using JetEazy.Lang;
using LaserAlignDX;
using LaserAlignDX.OPSpace.RecipeSpace;
using System;
using System.Windows.Forms;
using Traveller106;

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

        //JetEazy.FormSpace.VsMessageBox vsMessageBox = null;

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

            lblModifyDateTime = lblRcp4;

            btnAdd = btnRcpAdd;
            btnModify = btnRcpEdit;
            btnOK = btnRcpOK;
            btnCancel = btnRcpCancel;
            btnDetial = btnRcpDetails;

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
            btnRcpDel.Click += BtnDel_Click;
            
            //SizeChanged += RcpUI_SizeChanged;

            //STPUI = stpUI1;
            //STPUI.Initial(UIPath,LanguageIndex,VER,OPT,VIEW);
            //STPUI.TriggerAction += new StpUI.TriggerHandler(STPUI_TriggerAction);
            //STPUI.TriggerActionForSetupDetail += new StpUI.TriggerHandlerForSetupDetail(STPUI_TriggerActionForSetupDetail);

            FillDisplay(true);
            DBStatus = DBStatusEnum.NONE;

            BeginInvoke(new Action(() => LtAoiFactory.RcpCheckActive()));            
            //BeginInvoke(new Action(() => QMSG.Dump(this)));
        }

        private void BtnDel_Click(object sender, EventArgs e)
        {
            Delete();
        }
        void Delete()
        {
            if (RCPItemNow.Index == 0)
            {
                //VsMessageBox.Warning("系统参数无法删除！");
                VsMessageBox.Warning(QMSG.Text(Prompts.Info_Can_NOT_Delete_Recipe));
                return;
            }

            //if (VsMessageBox.Question("是否要删除参数？") == DialogResult.OK)
            if (VsMessageBox.Question(QMSG.Text(Prompts.Quection_To_Delete_Recipe)) == DialogResult.Yes)
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
            var vsMessageBox = VsMessageBox.InfoForm(QMSG.Text(Prompts.Info_Recipe_Saving));
            vsMessageBox.Show();
            vsMessageBox.Refresh();

            if (RCPDB.CheckDuplicate(txtName.Text.Trim() + txtVersion.Text.Trim(), RCPItemNow.Index))
            {
                //VsMessageBox.Warning("名称或版本已存在，请检查。");
                VsMessageBox.Warning(QMSG.Text(Prompts.Info_Recipe_Already_Existing));
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
            var vsMessageBox = VsMessageBox.InfoForm(QMSG.Text(Prompts.Info_Recipe_Rollback));
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
                        btnRcpDel.Visible = false;

                        btnOK.Visible = true;
                        btnCancel.Visible = true;

                        break;
                    case DBStatusEnum.NONE:
                        grpRcpData.Enabled = false;

                        btnAdd.Visible = true;
                        btnModify.Visible = true;
                        btnRcpDel.Visible = true;

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
            //using (var frmRecipeSetup = new frmFPIRecipe())
            //{
            //    frmRecipeSetup.ShowDialog();
            //}

            GaMvcConfig.OpenRecipeEditor();

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
            var ctrls = new Control[] { btnRcpAdd, btnRcpEdit,btnRcpDel };
            foreach (var c in ctrls)
            {
                c.Top = rcc.Bottom - c.Height - pad * 2;
                c.Left = x;
                c.Width = w;
                x += w + pad;
            }
            // btnOK
            btnRcpOK.Location = btnRcpEdit.Location;
            btnRcpOK.Size = btnRcpEdit.Size;

            // btnCancel
            btnRcpCancel.Location = btnRcpDel.Location;
            btnRcpCancel.Size = btnRcpDel.Size;

            // lblModifyDateTime
            lblRcp4.Top = btnRcpOK.Top - lblRcp4.Height - pad;
            lblRcp4.Left = groupBox1rcp.Left;
            lblRcp4.Width = rcc.Width - lblRcp4.Left * 2;
            // groupBox1
            groupBox1rcp.Height = (lblRcp4.Top - pad * 2) - groupBox1rcp.Top;
            // richbox1
            richTextBox1rcp.Height = richTextBox1rcp.Bottom - btnRcpDetails.Bottom - pad * 2;
            // btnDetail
            rcc = groupBox1rcp.ClientRectangle;
            btnRcpDetails.Left = rcc.Right - btnRcpDetails.Width - pad * 5;
            textBox2Rcp.Width = btnRcpDetails.Right - textBox2Rcp.Left;
#endif
        }
        #endregion
    }
}
