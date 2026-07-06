using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using JetEazy;
using JetEazy.BasicSpace;
//using Allinone.UISpace;
using JetEazy.ControlSpace;
using Traveller106;
using VsCommon.ControlSpace;
using Eazy_Project_III;
using LaserAlignDX;
using JetEazy.Lang;

namespace PhotoMachine.UISpace
{
    public partial class IniUI : UserControl
    {
        enum TagEnum
        {
            EDIT,
            OK,
            CANCEL,
        }
        //AdjUI ADJUI;

        PropertyGrid propertyGrid;

        GroupBox grpSetup;

        //Button btnLanguage;
        //TextBox txtMachineName;
        //NumericUpDown numDelayTime;

        //Label lblSavePath;
        //Button btnPathPicker;

        Button btnEdit;
        Button btnOK;
        Button btnCancel;

        //Language Setup
        //JzLanguageClass myLanguage = new JzLanguageClass();

        JzTransparentPanel tpnlCover;

        //CCDCollectionClass CCDCollection
        //{
        //    get
        //    {
        //        return Universal.CCDCollection;
        //    }

        //}

        //string UIPath = "";
        //int LanguageIndex = 0;

        VersionEnum VER = VersionEnum.STEROPES;
        OptionEnum OPT = OptionEnum.MAIN;

        public IniUI()
        {
            InitializeComponent();
            SizeChanged += IniUI_SizeChanged;
        }

        public void Initial(string uipath,
            int langindex,
            VersionEnum ver,
            OptionEnum opt)
        {
            //UIPath = uipath;
            //LanguageIndex = langindex;
            
            VER = ver;
            OPT = opt;

            //myLanguage.Initial(UIPath + "\\IniUI.jdb", LanguageIndex, this);

            //ADJUI = adjUI1;

            propertyGrid = propertyGrid1;

            grpSetup = groupBox1;

            btnEdit = button1;
            btnOK = button4;
            btnCancel = button6;

            btnEdit.Tag = TagEnum.EDIT;
            btnOK.Tag = TagEnum.OK;
            btnCancel.Tag = TagEnum.CANCEL;

            btnEdit.Click += new EventHandler(btn_Click);
            btnOK.Click += new EventHandler(btn_Click);
            btnCancel.Click += new EventHandler(btn_Click);
            //btnLanguage.Click += new EventHandler(btn_Click);
            //btnPathPicker.Click += new EventHandler(btn_Click);

            //ADJUI.Initial(CCDCollection.CCDRectRelateIndexList, CCDCollection.bmpBackGround.Width > 1);
            //ADJUI.TriggerMoveScreen += ADJUI_TriggerMoveScreen;
            //ADJUI.Enabled = false;
            btnUpdateData.Visible = false;

            tpnlCover = new JzTransparentPanel();
            tpnlCover.BackColor = System.Drawing.Color.Transparent;
            tpnlCover.Location = new System.Drawing.Point(propertyGrid.Location.X, propertyGrid.Location.Y);
            tpnlCover.Name = "panel1";

            propertyGrid.PropertyValueChanged += PG_PropertyValueChanged;


            if (propertyGrid.VerticalScroll.Visible)
                tpnlCover.Size = new Size(propertyGrid.Width - 15, propertyGrid.Height);
            else
                tpnlCover.Size = new Size(propertyGrid.Width, propertyGrid.Height);

            tpnlCover.TabIndex = 0;
            grpSetup.Controls.Add(tpnlCover);
            tpnlCover.BringToFront();
            
            DBStatus = DBStatusEnum.NONE;

            // 延後註冊多語系支援
            BeginInvoke((Action)PostInitLanguage);
            QMSG.Lang().LanguageChanged += (s, e) => PostInitLanguage();
        }

        private void PostInitLanguage()
        {
            EzPropertyGridTranslator.Register(INI.Instance);
            QMSG.Translate(this);
            updatePropertyGrid();
        }

        private void PG_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            var itemName = e.ChangedItem?.PropertyDescriptor?.Name;
            switch (itemName)
            {
                case "mLangIndex":
                //case "ChangeLangIndex":
                    INI.Instance.mLangIndex = (LangIndex)e.ChangedItem.Value;
                    JetEazy.BasicSpace.LanguageExClass.Instance.LanguageIndex = (int)INI.Instance.mLangIndex;
                    OnTrigger(INIStatusEnum.CHANGELANGUAGE);
                    break;
            }
        }

        private void ADJUI_TriggerMoveScreen(string movestring)
        {
            //MoveString = movestring;
            OnTriggerString(movestring);
        }

        void btn_Click(object sender, EventArgs e)
        {
            TagEnum KEYS = (TagEnum)((Button)sender).Tag;

            switch (KEYS)
            {
                case TagEnum.EDIT:
                    Modify();
                    break;
                case TagEnum.OK:
                    ModifyComplete();
                    break;
                case TagEnum.CANCEL:
                    ModifyCancel();
                    break;
            }
        }

        #region Button Fuction

        void Modify()
        {
            OnTrigger(INIStatusEnum.EDIT);
            DBStatus = DBStatusEnum.MODIFY;
        }
        void ModifyComplete()
        {
            INI.Instance.Save();
            DBStatus = DBStatusEnum.NONE;
            OnTrigger(INIStatusEnum.EXIT);
            //CCDCollection.SaveCCDLocation();
            updatePropertyGrid();
        }
        void ModifyCancel()
        {
            INI.Instance.Load();

            DBStatus = DBStatusEnum.NONE;

            //OnTrigger(INIStatusEnum.CHANGELANGUAGE);
            OnTrigger(INIStatusEnum.EXIT);
            //CCDCollection.LoadCCDLocation();
            updatePropertyGrid();
        }

        

        //void OK()
        //{
            
        //    m_DispUI.SetDisplayType(DisplayTypeEnum.SHOW);

        //}
        //void Cancel()
        //{
          
        //    m_DispUI.SetDisplayType(DisplayTypeEnum.SHOW);

        //}

        //void SelectPath()
        //{   
        //    //string PathStr = JzToolsClass.PathPicker(myLanguage.Messages("msg1", LanguageIndex), INI.SHOPFLOORPATH);
        //    //if (PathStr != "")
        //    //{
        //    //    INI.SHOPFLOORPATH = PathStr;
        //    //}

        //    FillDisplay();
        //}
        void ChangeLanguage()
        {
            INI.Instance.LANGUAGE = 1 - INI.Instance.LANGUAGE;
            OnTrigger(INIStatusEnum.CHANGELANGUAGE);
        }

        void SetPickPosition()
        {
            //lblPickM1Position.Text = INI.PICKM1POSITION.ToString("0.000");
        }

        #endregion

        //public void SetLanguage(int langindex)
        //{
        //    //myLanguage.SetControlLanguage(this, langindex);
        //}

        void updatePropertyGrid()
        {
            propertyGrid.SelectedObject = INI.Instance;
        }

        //MachineCollectionClass MACHINECollection
        //{
        //    get
        //    {
        //        return Eazy_Project_III.Universal.MACHINECollection;
        //    }
        //}

        //Eazy_Project_III.ControlSpace.MachineSpace.DispensingX1MachineClass machineX1
        //{
        //    get { return (Eazy_Project_III.ControlSpace.MachineSpace.DispensingX1MachineClass)Eazy_Project_III.Universal.MACHINECollection.MACHINE; }
        //}

        //void writeX1()
        //{
        //    //寫入INI的位置
        //    machineX1.PLCIO.MotorSinglePosition(2, 1, INI.Instance.GetPos1.ToString());
        //    machineX1.PLCIO.MotorSinglePosition(2, 2, INI.Instance.GetPos2.ToString());
        //    machineX1.PLCIO.MotorSinglePosition(2, 3, INI.Instance.PutPos1.ToString());
        //    machineX1.PLCIO.MotorSinglePosition(2, 4, INI.Instance.PutPos2.ToString());
        //    machineX1.PLCIO.MotorSinglePosition(3, 1, INI.Instance.UVWorkPos.ToString());

        //    //待命位置寫入
        //    machineX1.PLCIO.MotorDynamicPosition(5, INI.Instance.SafePosReady.ToString());
        //    machineX1.PLCIO.MotorDynamicPosition(7, INI.Instance.DispendingPosReady.ToString());

        //    //寫入點膠位置數據

        //    machineX1.PLCIO.SetMWIndex(Eazy_Project_III.ControlSpace.IOSpace.DX1_IOConstClass.MW1093, INI.Instance.DispensingX1_1PosList.Count);
        //    machineX1.PLCIO.SetMWIndex(Eazy_Project_III.ControlSpace.IOSpace.DX1_IOConstClass.MW1094, INI.Instance.DispensingX1_2PosList.Count);

        //    int dispensingindex = 0;
        //    foreach (string str in INI.Instance.DispensingX1_1PosList)
        //    {
        //        machineX1.PLCIO.MotorDispensing1Position(dispensingindex, str);
        //        dispensingindex++;
        //    }
        //    dispensingindex = 0;
        //    foreach (string str in INI.Instance.DispensingX1_2PosList)
        //    {
        //        machineX1.PLCIO.MotorDispensing2Position(dispensingindex, str);
        //        dispensingindex++;
        //    }
        //}

        //Eazy_Project_III.ControlSpace.MachineSpace.DispensingMachineClass machineX3
        //{
        //    get { return (Eazy_Project_III.ControlSpace.MachineSpace.DispensingMachineClass)Eazy_Project_III.Universal.MACHINECollection.MACHINE; }
        //}

        //void writeX3()
        //{
        //    Eazy_Project_III.Universal.MACHINECollection.SetIniPara();
        //}

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
                    case DBStatusEnum.MODIFY:

                        tpnlCover.Visible = false;

                        grpSetup.Enabled = true;

                        btnOK.Visible = true;
                        btnCancel.Visible = true;
                        btnEdit.Visible = false;

                        break;
                    case DBStatusEnum.NONE:

                        tpnlCover.Visible = true;

                        grpSetup.Enabled = true;

                        btnOK.Visible = false;
                        btnCancel.Visible = false;
                        btnEdit.Visible = true;

                        break;
                }
            }
        }

        public delegate void TriggerHandler(INIStatusEnum status);
        public event TriggerHandler TriggerAction;
        public void OnTrigger(INIStatusEnum status)
        {
            if (TriggerAction != null)
            {
                TriggerAction(status);
            }
        }

        public delegate void TriggerStringHandler(string statusstr);
        public event TriggerStringHandler TriggerStringAction;
        public void OnTriggerString(string statusstr)
        {
            if (TriggerStringAction != null)
            {
                TriggerStringAction(statusstr);
            }
        }

        private void btnUpdateData_Click(object sender, EventArgs e)
        {
            updatePropertyGrid();
        }


        #region AUTO_LAYOUT
        void IniUI_SizeChanged(object sender, EventArgs e)
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
            groupBox1.Dock = DockStyle.Top;
            propertyGrid1.Dock = DockStyle.Fill;

            var rcc = ClientRectangle;
            int pad = 1;
            int w = (rcc.Width - pad * 4) / 3;
            int x = pad;

            foreach (var c in new Control[] { button1, button4, button6 })
            {
                c.Top = rcc.Bottom - c.Height - pad * 2;
                c.Left = x;
                c.Width = w;
                x += w + pad;
            }

            groupBox1.Height = button1.Top - pad * 2 - groupBox1.Top;

            if (tpnlCover != null && propertyGrid != null)
            {
                tpnlCover.Location = propertyGrid.Location;
                tpnlCover.Size = new Size(propertyGrid.Width - 15, propertyGrid.Height);
            }
#endif
        }
        #endregion
    }
}
