using Eazy_Project_III;
using JetEazy.Interface;
using System;

namespace LaserAlignDX.UISpace
{
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
        event EventHandler<MainUIStateChangedEventArgs> OnStateChanged;
        //Control Window { get; }

        void Init();
        void ChangeRecipe();
        void SetEnable(bool enabled);
        void SetEnableState(bool enabled);
    }
}
