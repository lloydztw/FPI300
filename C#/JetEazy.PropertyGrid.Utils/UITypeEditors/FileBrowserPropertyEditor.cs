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
    /// 開啟檔案
    /// </summary>
    public class FileBrowserPropertyEditor : UITypeEditor
    {
        #region PUBLIC_EVENT_ARGS
        public class FileDialogEventArgs : EventArgs
        {
            public FileDialogEventArgs(string propName)
            {
                PropName = propName;
            }
            public readonly string PropName;
            public string Title;
            public string Filter;
            public string InitialDirectory;
        }
        #endregion

        #region PROTECTED_MEMBER
        protected EventHandler<FileDialogEventArgs> _OnOpenFileDialogCallback;
        #endregion

        public FileBrowserPropertyEditor(EventHandler<FileDialogEventArgs> OnOpenFileDialogCallback)
        {
            _OnOpenFileDialogCallback = OnOpenFileDialogCallback;
        }
        public FileBrowserPropertyEditor() : this(null)
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
                    using (OpenFileDialog dlg = new OpenFileDialog())
                    {
                        var e = new FileDialogEventArgs(pContext.PropertyDescriptor.Name);

                        OnOpenFileDialog(dlg, e);

                        if (!string.IsNullOrEmpty(e.Title))
                            dlg.Title = e.Title;

                        if (!string.IsNullOrEmpty(e.Filter))
                            dlg.Filter = e.Filter;

                        if (!string.IsNullOrEmpty(e.InitialDirectory))
                            dlg.InitialDirectory = e.InitialDirectory;

#if (OPT_LEGACY)
                    switch (pContext.PropertyDescriptor.Name)
                    {
                        case "CfgPath":
                            fileDlg.Filter = "控制器配置文件|*.dcfg";
                            fileDlg.Title = "请选择匹配的控制器配置文件";
                            break;
                        case "HWCPath":
                            fileDlg.Filter = "控制器位移校准文件|*.hwc";
                            fileDlg.Title = "请选择匹配的控制器位移校准文件";
                            break;
                        case "L1Path":
                        case "L2Path":
                            fileDlg.InitialDirectory = Traveller106.Universal.PATH_CALI;
                            fileDlg.Filter = "标定校准文件|*.msr";
                            fileDlg.Title = "请选择镭射头的标定校准文件";
                            break;
                        case "L3Path":
                            fileDlg.InitialDirectory = Traveller106.Universal.PATH_CALI;
                            fileDlg.Filter = "标定校准文件|*.msr";
                            fileDlg.Title = "请选择相机标定板校准文件";
                            break;
                        case "L4Path":
                            fileDlg.InitialDirectory = Traveller106.Universal.PATH_CALI;
                            fileDlg.Filter = "标定校准文件|*.msr";
                            fileDlg.Title = "请选择转换到镭射头校准文件";
                            break;
                        default:
                            break;
                    }
#endif

                        if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            pValue = dlg.FileName;
                        }
                    }
                }
            }
            return pValue;
        }
        
        protected virtual void OnOpenFileDialog(object sender, FileDialogEventArgs e)
        {
            _OnOpenFileDialogCallback?.Invoke(sender, e);
        }
    }
}
