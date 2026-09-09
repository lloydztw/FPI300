#region AUTHOR
/*
 * 
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2026-09-10 重整 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace JetEazy.PropertyGrid
{
    /// <summary>
    /// 開啟資料夾
    /// </summary>
    public class FolderBrowserPropertyEditor : UITypeEditor
    {
        #region PUBLIC_EVENT_ARGS
        public class FolderDialogEventArgs : EventArgs
        {
            public FolderDialogEventArgs(string propName)
            {
                PropName = propName;
            }
            public readonly string PropName;
            public string Description;
        }
        #endregion

        #region PROTECTED_MEMBERS
        protected EventHandler<FolderDialogEventArgs> _OnOpenFolderDialogCallback;
        #endregion

        public FolderBrowserPropertyEditor(EventHandler<FolderDialogEventArgs> OnOpenFolderDialogCallback)
        {
            _OnOpenFolderDialogCallback = OnOpenFolderDialogCallback;
        }
        public FolderBrowserPropertyEditor() : this(null)
        {
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext pContext)
        {
            if (pContext != null && pContext.Instance != null)
            {
                //以「...」按鈕的方式顯示
                //UITypeEditorEditStyle.DropDown    下拉選單
                //UITypeEditorEditStyle.None        預設的輸入欄位
                return UITypeEditorEditStyle.Modal;
            }
            return base.GetEditStyle(pContext);
        }
        public override object EditValue(ITypeDescriptorContext pContext, IServiceProvider pProvider, object pValue)
        {
            IWindowsFormsEditorService editorService = null;
            if (pContext != null && pContext.Instance != null && pProvider != null)
            {
                editorService = (IWindowsFormsEditorService)pProvider.GetService(typeof(IWindowsFormsEditorService));
                if (editorService != null)
                {
                    using (FolderBrowserDialog dlg = new FolderBrowserDialog())
                    {
                        dlg.ShowNewFolderButton = false;

                        #region LEGACY_CODE
                        ////dlg.Description = Description;
                        ////dlg.SelectedPath = DefaultPath;
                        //switch (pContext.PropertyDescriptor.Name)
                        //{
                        //    case "LaserSharePath":
                        //        dlg.SelectedPath = (string)pValue;
                        //        dlg.Description = "请选择与镭雕机共享文件路径";
                        //        break;
                        //}
                        #endregion

                        var ev = new FolderDialogEventArgs(pContext.PropertyDescriptor.Name);
                        OnOpenFolderDialog(dlg, ev);

                        if (!string.IsNullOrEmpty(ev.Description))
                            dlg.Description = ev.Description;

                        var lastFolder = pValue as string;
                        if (!string.IsNullOrEmpty(lastFolder) && System.IO.Directory.Exists(lastFolder))
                            dlg.SelectedPath = lastFolder;

                        if (dlg.ShowDialog().Equals(DialogResult.OK))
                        {
                            if (dlg.SelectedPath != "")
                                pValue = dlg.SelectedPath;
                        }
                    }
                }
            }
            return pValue;
        }

        protected virtual void OnOpenFolderDialog(object sender, FolderDialogEventArgs e)
        {
            _OnOpenFolderDialogCallback?.Invoke(sender, e);
        }
    }
}
