#region AUTHOR
/*
 * 
 * Copyright (c) 2023 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2024-10-03 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using AwFramework;
using System.Windows.Forms;


namespace EzAoiEmptyTrayInspector.Gui
{
    public interface IvFuncButtonsPanel : IView
    {
        Button btnRunAll { get; }
        Button btnOpenFile { get; }
        Button btnSnapshot { get; }
        Button btnResetClear { get; }
        Button btnPickGolden { get; }
        void HookTo(IvFuncButtonsPanel panel);
    }
}
