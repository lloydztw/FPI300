using Eazy_Project_III;
using JetEazy.BasicSpace;
using JetEazy.Interface;
using LaserAlignDX.UISpace;
using LaserAlignDX.UISpace.UIMVC;
using System;
using System.Drawing;
using System.Windows.Forms;


namespace LaserAlignDX.Mvc.Ctrl.Abs
{
    public abstract class GaMainCtrl : IxTickable
    {
        public event EventHandler<MainUiStateEventArgs> OnStateChanged;

        public abstract void Tick();

        public abstract void Attach(Control[] DsMains, Control[] DsFlys, Control lblFlyCameraSerialNo);

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
