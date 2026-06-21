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


namespace EzAoiChipLocQC.Gui.Panels
{
    public partial class GvRecipeDockPanelExt : UserControl, IView
    {
        public GvRecipeDockPanelExt(Control originalRcpPanel = null)
        {
            InitializeComponent();

            if (!DesignMode)
            {
                var wnd = originalRcpPanel;
                if (wnd != null)
                {
                    panel2.Controls.Add(wnd);
                    wnd.Parent = panel2;
                    wnd.Dock = DockStyle.Fill;
                    wnd.Visible = true;
                }
            }
        }

        //Form IView.frmOwner => FindForm();
        Control IView.Window => this;
    }
}
