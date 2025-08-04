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

using EzAoiEmptyTrayInspector.Model;
using System.Windows.Forms;
using AoiFactory = EzAoiEmptyTrayInspector.AoiFactory;

namespace Traveller106
{
    public class LtAoiFactory
    {
        public static string GetActiveRecipeNameAtFPI30()
        {
            return Universal.RCPDB?.RCPItemNow?.Name;
        }

        public static Form OpenEmptyTrayInspectorTool(Form parent)
        {
            var recipeName = GetActiveRecipeNameAtFPI30();
            var frm = AoiFactory.OpenEmptyTrayInspectorTool(parent, recipeName);
            frm?.Show();
            return frm;
        }

        public static IxEmptyTrayInspector InstanceModel()
        {
            var recipeName = GetActiveRecipeNameAtFPI30();
            var aoiModel = AoiFactory.InstanceModel(recipeName);
            return aoiModel;
        }

        public static void DisposeAll()
        {
            AoiFactory.DisposeAll();
        }
    }
}
