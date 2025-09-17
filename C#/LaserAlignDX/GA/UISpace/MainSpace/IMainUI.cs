#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-09-16 重整 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using Eazy_Project_III;
using JetEazy.Interface;
using System;
using System.Windows.Forms;

namespace LaserAlignDX.UISpace
{
    public interface IMainUI : IxTickable
    {
        event EventHandler<MainUiStateEventArgs> OnStateChanged;

        Control Window { get; }

        void Init();
        void ChangeRecipe();
        void SetEnable(bool enabled);
        void SetEnableState(bool enabled);
    }


    public class MainUiStateEventArgs : EventArgs
    {
        public MainS1State Status;
        public object Tag;
        public MainUiStateEventArgs(MainS1State status, object tag)
        {
            this.Status = status;
            this.Tag = tag;
        }
    }
}
