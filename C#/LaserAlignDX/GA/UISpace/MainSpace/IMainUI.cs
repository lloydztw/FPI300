using Eazy_Project_III;
using JetEazy.Interface;
using System;
using System.Windows.Forms;

namespace LaserAlignDX.UISpace
{
    public delegate void ChangeStateHandler(MainS1State status, object tag = null);

    public class MainUIStateChangedEventArgs : EventArgs
    {
        public MainS1State Status;
        public object Tag;
        public MainUIStateChangedEventArgs(MainS1State status, object tag)
        {
            this.Status = status;
            this.Tag = tag;
        }
    }

    public interface IMainUI : IxTickable
    {
        //event EventHandler<MainUIStateChangedEventArgs> OnStateChanged;
        event ChangeStateHandler OnChangeState;

        Control Window { get; }

        void Init();
        void ChangeRecipe();
        void SetEnable(bool enabled);
        void SetEnableState(bool enabled);
    }
}
