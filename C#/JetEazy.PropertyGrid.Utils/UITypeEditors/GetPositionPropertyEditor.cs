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
using System.Windows.Forms.Design;

namespace JetEazy.PropertyGrid
{
    public class GetPositionPropertyEditor : UITypeEditor
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
}
