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

namespace EzAoiEmptyTrayInspector
{
    public class AoiFactory
    {
        public static Form OpenEmptyTrayInspectorTool(Form parent = null, string recipeFileName = null)
        {
            //if (_aoiModelInUsed != null)
            //    _aoiModelInUsed.AddRef();

            var frm = EzAppForDll.Instance.Build(recipeFileName);
            if (parent != null)
            {
                var aoiModel = Global.AoiModel;
                aoiModel.AddRef();
            }

            return frm;
        }

        public static IxEmptyTrayInspector InstanceModel(string recipeFileName = null)
        {
            var model = Global.AoiModel;
            EzAppForDll.Instance.AssignOneRecipe(recipeFileName);
            return model;
        }

        public static void DisposeAll()
        {
            EzAppForDll.Instance.Dispose();
            Global.Dispose();
        }
    }
}
