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
using JetEazy.FormSpace;
using JetEazy.Match;
using JetEazy.OpenCV;
using JetEazy.Utils;
using LaserAlignDX;
using System;
using System.Drawing;
using System.Windows.Forms;
using AoiFactory = EzAoiEmptyTrayInspector.AoiFactory;


namespace Traveller106
{
    public partial class LtAoiFactory
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
        public static Form OpenEmptyTrayInspectorTool(Form owner, string recipeName = null, Bitmap bmpToShow = null)
        {
            if (recipeName == null)
                recipeName = GetActiveRecipeNameAtFPI30();

            var frm = AoiFactory.OpenEmptyTrayInspectorTool(owner, recipeName);
            if (frm == null)
                return null;

            if (bmpToShow != null)
            {
                frm.Load += (s, e) =>
                {
                    new Action(() =>
                    {
                        System.Threading.Thread.Sleep(2000);
                        PushBitmap(bmpToShow, "[參數] bmpOrg");
                    }).BeginInvoke(null, null);
                };
            }

            //frm.Show();
            frm.ShowDialog(owner);
            return frm;
        }

        /// <summary>
        /// 推送影像到 Tool Window
        /// - AoiFactory 負責接手管控 bmp 生命週期
        /// - name 為標記名稱
        /// </summary>
        public static void PushBitmap(Bitmap bmp, string name)
        {
            using (var bridge = new QxImageBridge(bmp))
            {
                var qImg = new EzQuickImage(bridge.Image, true);
                AoiFactory.PushImage(qImg, name);
            }
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

    partial class LtAoiFactory
    {
        #region PRIVATE_DATA
        static string _recipePath => AoiFactory.RecipePath;
        static string _activeRecipeName;
        #endregion

        public static string RcpGetRecipeFileName(string recipeName)
        {
            if (string.IsNullOrEmpty(recipeName))
            {
                RcpCheckActive(silent: true);
                recipeName = _activeRecipeName;
            }
            var fileName = System.IO.Path.Combine(_recipePath, recipeName + ".json");
            return fileName;
        }
        public static bool RcpCheckActive(bool silent = false)
        {
            var recipeName = _activeRecipeName = GetActiveRecipeNameAtFPI30();
            var fileName = System.IO.Path.Combine(_recipePath, recipeName + ".json");
            if (!System.IO.File.Exists(fileName))
            {
                if (!silent)
                    PromptWarning($"空盤檢測參數 {recipeName} 還沒建立!");
                return false;
            }
            return true;
        }
        public static void RcpSetActive(string recipeName)
        {
            // 當 Gaara Recipe Manager 發生變動
            if (_activeRecipeName != recipeName)
                _activeRecipeName = recipeName;
            var model = GaMvcConfig.SysModel;
            model.ApplyRecipe(_activeRecipeName);
        }
        public static void RcpRename(string recipeName)
        {
            if (_activeRecipeName != recipeName)
            {
                try
                {
                    var srcFile = System.IO.Path.Combine(_recipePath, _activeRecipeName + ".json");
                    var dstFile = System.IO.Path.Combine(_recipePath, recipeName + ".json");
                    if (System.IO.File.Exists(srcFile))
                        System.IO.File.Move(srcFile, dstFile);
                }
                catch(Exception ex)
                {
                    PromptWarning($"空盤檢測參數 無法改名:\n\r {_activeRecipeName} -> {recipeName}");
                }
            }
        }
        public static void RcpDelete(string recipeName)
        {
            try
            {
                var fileName = System.IO.Path.Combine(_recipePath, recipeName);
                System.IO.File.Delete(fileName);
            }
            catch
            {
                PromptWarning($"空盤檢測參數 {recipeName} 無法刪除!");
            }
        }
        static void PromptWarning(string message)
        {
            #region 暫時直接在此調用 GUI 元件
            GaUtil.LOG(message, Color.Red);
            var msgBox = new VsMessageBox(message, true);
            msgBox.FormClosed += (s, e) => (s as Form)?.Dispose();
            var frms = Application.OpenForms;
            var frm = frms.Count > 0 ? frms[0] : null;
            msgBox.ShowDialog(frm);
            #endregion
        }
    }
}
