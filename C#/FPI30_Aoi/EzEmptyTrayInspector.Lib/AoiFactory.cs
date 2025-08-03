#region AUTHOR
/*
 * 
 * Copyright (c) 2024 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2024-09-25 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System.Windows.Forms;

namespace EzAoiEmptyTrayInspector
{
    public class AoiFactory
    {
        public static Form OpenEmptyTrayInspectorTool(Form owner = null, string recipeFileName = null)
        {
            var frm = EzApp.Instance.Build();
            frm.Parent = owner;
            return frm;
        }
    }
}
