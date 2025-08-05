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

using EzAoiEmptyTrayInspector.Ctrl;
using EzAoiEmptyTrayInspector.Model;
using System;
using System.Windows.Forms;

namespace EzAoiEmptyTrayInspector
{
    public class AoiFactory
    {
        public static Form OpenEmptyTrayInspectorTool(Form parent = null, string recipeFileName = null)
        {
            EzRcpContraintCtrl.Instance.IsContraintEnabled = parent != null;

            var frm = EzAppForDll.Instance.Build();

            if (parent != null)
            {
                var aoiModel = Global.AoiModel;
                aoiModel.AddRef();

                if (recipeFileName != null)
                {
                    frm.Load += (s, e) =>
                    {
                        frm.BeginInvoke(new Action(() =>
                        {
                            EzRcpContraintCtrl.Instance.AssignOneRecipe(recipeFileName);
                        }));
                    };
                }
            }

            return frm;
        }

        public static IxEmptyTrayInspector InstanceModel(string recipeFileName = null)
        {
            var model = Global.AoiModel;
            EzRcpContraintCtrl.Instance.AssignOneRecipe(recipeFileName);
            return model;
        }

        public static void DisposeAll()
        {
            Global.Dispose();
        }
    }
}
