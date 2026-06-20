#region AUTHOR
/*
 * 
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2026-06-20 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using AwFramework;
using EzAoiChipLocQC.Model;


namespace EzAoiChipLocQC.Gui
{
    public interface IvQcTrayView : IView
    {
        void UpdateSettings(JxTrayDimSettings traySettings);
    }
}
