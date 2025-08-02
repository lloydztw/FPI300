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

using System.Windows.Forms;
using AwFramework;


namespace EzDualMatch.Gui.Panels
{
    public partial class GwRecipeDockPanel : UserControl, IView
    {
        public GwRecipeDockPanel()
        {
            InitializeComponent();
        }

        //Form IView.frmOwner => FindForm();
        Control IView.Window => this;
    }
}
