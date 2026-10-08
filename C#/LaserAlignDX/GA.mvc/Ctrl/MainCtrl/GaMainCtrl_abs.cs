#region AUTHOR
/*
 *
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 *
 * REVISION:
 *      2026-10-08 使用 ChatGPT Codex  重整 (by LeTian Chang)
 *
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 *
 */
#endregion

using System;
using System.Drawing;
using System.Windows.Forms;
using Eazy_Project_III;
using JetEazy.BasicSpace;
using JetEazy.Interface;
using LaserAlignDX.UISpace;

namespace LaserAlignDX.Mvc.Ctrl.Abs
{
    public abstract class GaMainCtrl : IxTickable, IDisposable
    {
        public event EventHandler<MainUiStateEventArgs> OnStateChanged;

        public abstract void Tick();

        public virtual void Dispose()
        {
            OnStateChanged = null;
        }

        public abstract void Attach(Control[] DsMains, Control[] DsFlys, Control lblFlyCameraSerialNo, Control lblMemoryUsage);

        #region 對上層 MainControlUI 所需要的接口
        public virtual void ChangeRecipe()
        {
        }
        public virtual void SetEnable(bool isendable)
        {
        }
        public virtual void SetEnableState(bool isendable)
        {
        }
        #endregion

        #region PROTECTED_HELPER_FUNCTIONS
        protected void FireChangeState(MainS1State status, object tag = null)
        {
            OnStateChanged?.Invoke(this, new MainUiStateEventArgs(status, tag));
        }
        protected void _LOG(string msg, Color color)
        {
            //>>> GaUtil.LOG(msg, args);
            CommonLogClass.Instance.LogMessage(msg, color);
        }
        #endregion
    }
}
