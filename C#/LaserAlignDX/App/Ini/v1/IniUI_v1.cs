#region AUTHOR
/*
 * 
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2026-09-09 重寫 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy;
using JetEazy.Lang;
using LeTian.JxProps.Gui;
using System;
using System.Windows.Forms;
using Traveller106;

namespace LaserAlignDX.Mvc.Gui
{
    public partial class IniUI : UserControl
    {
        #region GUI_LINKS
        GwPanePropsViewer wndPropViewer => gwPanePropsViewer1;
        #endregion

        #region INI
        INI _INI => INI.Instance;
        #endregion

        public IniUI()
        {
            InitializeComponent();
            if (DesignMode)
                return;

            btnOK.Click += BtnOK_Click;
            btnCancel.Click += BtnCancel_Click;
            btnEdit.Click += BtnEdit_Click;
        }

        public void Initial(params object[] args)
        {
            // 延後註冊多語系支援
            QMSG.Lang().LanguageChanged += (s, e) => PostInitLanguage();
            BeginInvoke(new Action(() =>
            {
                wndPropViewer.Editable = false;
                UpdateJxPropsViewer();
                PostInitLanguage();
                UpdateGuiStatus();
            }));
        }

        #region EVENT_HANDLERS
        private void BtnEdit_Click(object sender, EventArgs e)
        {
            Modify();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            ModifyCancel();
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            ModifyComplete();
        }
        #endregion

        #region PRIVATE_FUNCTIONS
        void PostInitLanguage()
        {
            EzPropertyGridTranslator.Register(INI.Instance);
            QMSG.Translate(this);
        }

        void Modify()
        {
            wndPropViewer.Editable = true;
            UpdateGuiStatus();

            OnTrigger(INIStatusEnum.EDIT);
            //DBStatus = DBStatusEnum.MODIFY;
            //var jxViewer = gwPanePropsViewer1;
            //jxViewer.Editable = true;
        }
        void ModifyComplete()
        {
            _INI.Save();
            wndPropViewer.Editable = false;
            UpdateGuiStatus();

            OnTrigger(INIStatusEnum.EXIT);
        }
        void ModifyCancel()
        {
            _INI.Load();
            wndPropViewer.Editable = false;
            //UpdateJxPropsViewer();
            UpdateGuiStatus();

            //DBStatus = DBStatusEnum.NONE;
            //OnTrigger(INIStatusEnum.CHANGELANGUAGE);
            //CCDCollection.LoadCCDLocation();
            //UpdateJxPropsViewer();
            OnTrigger(INIStatusEnum.EXIT);
        }

        void UpdateJxPropsViewer()
        {
            wndPropViewer.BuildGuiCtrls(_INI.AppSettings);
        }
        void UpdateGuiStatus()
        {
            bool isEditting = wndPropViewer.Editable;
            btnEdit.Visible = !isEditting;
            btnCancel.Visible = isEditting;
            btnOK.Visible = isEditting;
        }
        #endregion

        #region OLD_UGLY_FUNCTIONS
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
        #endregion
    }
}
