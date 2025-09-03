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

using AwFramework.Gui;
using AwFramework.Util;
using EzAoiEmptyTrayInspector.Ctrl;
using EzAoiEmptyTrayInspector.Gui.Panels;
using EzAoiEmptyTrayInspector.Model;
using JetEazy.EzImage;
using System;
using System.Windows.Forms;

namespace EzAoiEmptyTrayInspector
{
    public class AoiFactory
    {
        #region PRIVATE_DATA
        static IxEmptyTrayInspector _directModelInstance = null;
        static Form _frmInstance = null;
        #endregion

        /// <summary>
        /// 要求線掃影像
        /// </summary>
        public static event EventHandler OnLineScanRequested;

        public static string RecipePath
        {
            get
            {
                return Global.APP_PATH.RecipePath;
            }
        }

        /// <summary>
        /// 直接取用 AoiModel
        /// </summary>
        public static IxEmptyTrayInspector InstanceModel(string recipeName = null)
        {
            if (_frmInstance != null)
            {
                MessageBox.Show("必須先關掉 Tool!");
                return null;
            }

            var model = Global.AoiModel;
            //載入指定參數
            EzRcpContraintCtrl.Instance.AssignOneRecipe(recipeName);
            _directModelInstance = model;
            return model;
        }

        /// <summary>
        /// 開啟 Tool Window
        /// </summary>
        public static Form OpenEmptyTrayInspectorTool(Form owner = null, string recipeName = null)
        {
            if (_frmInstance == null)
            {
                EzRcpContraintCtrl.Instance.IsContraintEnabled = owner != null;

                var frm = EzAppForDll.Instance.Build();

                frm.FormClosed += (s, e) => 
                {
                    _frmInstance = null;
                };

                if (owner != null)
                {
                    frm.Load += (s, e) =>
                    {
                        //延緩載入指定參數
                        if (recipeName != null)
                            frm.BeginInvoke(new Action(() => { EzRcpContraintCtrl.Instance.AssignOneRecipe(recipeName); }));

                        // LineScanButton
                        var btnScan = GetScanButton(frm);
                        if (btnScan != null)
                            btnScan.Click += (ss, ee) => OnLineScanRequested?.Invoke(ss, ee);
                    };

                    // 當 aoiModel 已經被 InstanceModel 先啟用.
                    // 對其 addRef()
                    if (_directModelInstance != null)
                        _directModelInstance.AddRef();
                }

                _frmInstance = frm;
                return _frmInstance;
            }
            else
            {
                return _frmInstance;
            }
        }

        /// <summary>
        /// 推送影像到 Tool Window
        /// - AoiFactory 負責接手管控 image 生命週期
        /// - name 為標記名稱
        /// </summary>
        public static void PushImage(IEzImage image, string name)
        {
            if (_frmInstance != null)
            {
                _frmInstance.BeginInvoke(new Action<IEzImage, string>((img, nam) =>
                {
                    var ctrl = EzAppForDll.Instance.MatchCtrl;
                    if (ctrl != null)
                        ctrl.SetImage(img, nam);
                    else
                        img?.Dispose();
                }), image, name);
            }
            else
            {
                image?.Dispose();
                image = null;
            }
        }

        /// <summary>
        /// 釋放所有資源
        /// </summary>
        public static void DisposeAll()
        {
            _directModelInstance?.Dispose();
            _directModelInstance = null;
            Global.Dispose();
        }

        #region PRIVATE_FUNCTIONS
        static Button GetScanButton(Form frm)
        {
            var btnScan = AppUtil.SearchGui<GwFuncButtonsPanel>(frm, null)?.btnSnapshot;
            return btnScan;
        }
        #endregion
    }
}
