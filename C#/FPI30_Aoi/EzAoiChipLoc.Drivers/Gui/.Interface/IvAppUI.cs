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
using AwFramework.Gui;
using System.Windows.Forms;


namespace EzAoiChipLocQC.Gui
{
    public interface IvAppUI : IView
    {
        FormAwMain frmAwMain { get; }
        IvFuncButtonsPanel FuncButtonsPanel { get; }
        IvRecipeBriefView RecipeBriefView { get; }
        IvSingleMatchView MatchView { get; }
        Control lblPassFail { get; }
    }
}
