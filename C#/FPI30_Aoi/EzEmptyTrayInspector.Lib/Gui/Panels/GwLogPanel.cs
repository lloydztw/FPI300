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

namespace EzAoiEmptyTrayInspector.Gui.Panels
{
    public partial class GwLogPanel : UserControl, IView
    {
        public GwLogPanel()
        {
            InitializeComponent();

            btnClear.Visible = false;
            btnClear.Click += (s, e) =>
            {
                // 貌似被 NLOG 占用, 無法清除 !!!
                // richTextBox1.Text = "";
                richTextBox1.Clear();
            };
        }

        //Form IView.frmOwner => FindForm();
        Control IView.Window => this;
    }
}
