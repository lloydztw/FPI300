using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Traveller106
{
    public class UITypeGetPositionPropertyEditor : UITypeEditor
    {
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
                    //將顯示得視窗放在這邊，並透過ShowDialog方式來呼叫
                    //取得到的值再回傳回去
                    //MessageBox.Show("sfsf");
                    //frmDataGridViewPosition msrdataform = new frmDataGridViewPosition(pContext.PropertyDescriptor.DisplayName,
                    //    pContext.PropertyDescriptor.Name,(string)pValue);
                    ////msrdataform.Show();
                    //if (msrdataform.ShowDialog() == DialogResult.OK)
                    //{
                    //    pValue = JzToolsClass.PassingString;
                    //}

                    //pValue = "FUCK YOU!";
                }
            }
            return pValue;
        }
    }

    public class UITypeGetFilePathPropertyEditor : UITypeEditor
    {
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

                    OpenFileDialog fileDlg = new OpenFileDialog();

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

                    if (fileDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        pValue = fileDlg.FileName;
                    }
                }
            }
            return pValue;
        }
    }

    public class UITypeSetFilePathPropertyEditor : UITypeEditor
    {
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

                    FolderBrowserDialog fd = new FolderBrowserDialog();
                    //fd.Description = Description;
                    fd.ShowNewFolderButton = false;
                    //fd.SelectedPath = DefaultPath;

                    switch (pContext.PropertyDescriptor.Name)
                    {
                        case "LaserSharePath":
                            fd.SelectedPath = (string)pValue;
                            fd.Description = "请选择与镭雕机共享文件路径";
                            break;
                    }

                    if (fd.ShowDialog().Equals(DialogResult.OK))
                    {
                        if (fd.SelectedPath != "")
                            pValue = fd.SelectedPath;
                    }
                }
            }
            return pValue;
        }
    }
}
