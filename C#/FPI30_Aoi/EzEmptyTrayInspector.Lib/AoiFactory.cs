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
        #region PRIVATE_DATA
        static IxEmptyTrayInspector _directModelInstance = null;
        static Form _frmInstance = null;
        #endregion

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
                    // 延緩載入指定參數
                    if (recipeName != null)
                    {
                        frm.Load += (s, e) =>
                        {
                            frm.BeginInvoke(new Action(() => { EzRcpContraintCtrl.Instance.AssignOneRecipe(recipeName); }));
                        };
                    }

                    // 當 aoiModel 已經被 InstanceModel 先啟用.
                    // 對其 addRef()
                    if (_directModelInstance != null)
                    {
                        _directModelInstance.AddRef();
                    }
                }

                _frmInstance = frm;
                return _frmInstance;
            }
            else
            {
                return _frmInstance;
            }
        }

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

        public static void DisposeAll()
        {
            _directModelInstance?.Dispose();
            _directModelInstance = null;
            Global.Dispose();
        }
    }
}
