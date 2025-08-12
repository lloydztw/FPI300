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
using JetEazy.EzImage;
using JetEazy.Match;
using System;
using System.Drawing;
using System.Windows.Forms;
using AoiFactory = EzAoiEmptyTrayInspector.AoiFactory;


namespace Traveller106
{
    public class LtAoiFactory
    {
        /// <summary>
        /// 要求線掃影像
        /// </summary>
        public static event EventHandler OnLineScanRequested
        {
            add => AoiFactory.OnLineScanRequested += value;
            remove => AoiFactory.OnLineScanRequested -= value;
        }

        /// <summary>
        /// 取得 GA 目前參數
        /// </summary>
        public static string GetActiveRecipeNameAtFPI30()
        {
            return Universal.RCPDB?.RCPItemNow?.Name;
            //return "000_default";
        }

        /// <summary>
        /// 直接取用 AoiModel
        /// </summary>
        public static IxEmptyTrayInspector InstanceModel(string recipeName = null)
        {
            if (recipeName == null)
                recipeName = GetActiveRecipeNameAtFPI30();

            var aoiModel = AoiFactory.InstanceModel(recipeName);
            return aoiModel;
        }

        /// <summary>
        /// 開啟 Tool Window
        /// </summary>
        public static Form OpenEmptyTrayInspectorTool(Form owner, string recipeName = null)
        {
            if (recipeName == null)
                recipeName = GetActiveRecipeNameAtFPI30();

            var frm = AoiFactory.OpenEmptyTrayInspectorTool(owner, recipeName);
            frm?.Show();
            return frm;
        }

        /// <summary>
        /// 推送影像到 Tool Window
        /// - AoiFactory 負責接手管控 image 生命週期
        /// - name 為標記名稱
        /// </summary>
        public static void PushImage(IEzImage image, string name)
        {
            AoiFactory.PushImage(image, name);
        }

        public static EzBlocsGrid DetectGrid(Bitmap fullfovBmp)
        {
            var aoiModel = InstanceModel();
            aoiModel.RunAll(fullfovBmp, wait: true);
            var result = aoiModel.GetResult();
            return result?.Grid;
        }

        /// <summary>
        /// 釋放所有資源
        /// </summary>
        public static void DisposeAll()
        {
            AoiFactory.DisposeAll();
        }
    }
}
