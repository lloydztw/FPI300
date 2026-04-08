#region AUTHOR
/*
 * 
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2026-04-08 初稿 (by LeTian Chang)
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


namespace LaserAlignDX.Mvc.Gui
{
    public class PgCameraFocusCfgUI : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var editorService = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

            if (editorService != null)
            {
                //using (var dlg = new FormCameraConfig())
                //{
                //    PgJxivPropsDto.parseGidDeviceInfo(value?.ToString(), out int gid, out string devInfo);
                //    dlg.SetSelection(devInfo);

                //    if (editorService.ShowDialog(dlg) == DialogResult.OK)
                //    {
                //        gid = dlg.SelectedGlobalCamID;
                //        devInfo = dlg.SelectedDeviceInfo?.ToString();
                //        value = PgJxivPropsDto.makeGidDeviceInfo(gid, devInfo);
                //    }
                //}
            }

            return value;
        }
    }
}
