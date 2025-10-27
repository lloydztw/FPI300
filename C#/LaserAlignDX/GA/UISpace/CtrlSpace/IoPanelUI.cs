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

using JetEazy;
using JetEazy.Interface;
using System.Windows.Forms;

namespace LaserAlignDX.UISpace.CtrlSpace
{
    public interface IoPanelUI : IxTickable
    {
        Control Window { get; }
        void Initial(VersionEnum version, OptionEnum option, object machine);
        void SetEnable(bool enabled);
    }
}
