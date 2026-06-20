#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-09-07 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using BrightIdeasSoftware;
using System.Windows.Forms;

namespace LeTian.JxProps.Gui
{
    /// <summary>
    /// 擴增原有 LeTian.JxProps.Gui. GwPanePropsViewer 的功能
    /// </summary>
    public static class GwPanePropsViewerExtension
    {
        public static void ExpandAll(this GwPanePropsViewer viewer)
        {
            ExpandTrees(viewer);
        }
        public static void ExpandTrees(Control panel)
        {
            foreach (Control c in panel.Controls)
            {
                //System.Diagnostics.Debug.WriteLine(c.GetType().Name);
                if (c is TreeListView treeView)
                {
                    treeView.ExpandAll();
                    return;
                }
                ExpandTrees(c);
            }
        }
    }
}
