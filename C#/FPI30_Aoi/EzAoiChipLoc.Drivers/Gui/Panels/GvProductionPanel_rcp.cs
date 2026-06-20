#region AUTHOR
/*
 * 
 * Copyright (c) 2023 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2023-08-23 ªì½Z (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using AwFramework;
using System.Drawing;
using System.Windows.Forms;


namespace EzEmptyTrayInspector.Gui.Panels.Cbo
{
    public partial class GvProductionPanel : UserControl, IView
    {
        public GvProductionPanel()
        {
            InitializeComponent();
            _syncBkgndImages();
            _initEventHandlers();
            btnTryRun.Text = "¤@Áä°õ¦æ";
        }

        #region PRIVATE_LAYOUT_FUNCTIONS
        void _initEventHandlers()
        {
            SizeChanged += (s, e) => _autoLayout();
            if (DesignMode)
            {
                BackgroundImageChanged += (s, e) => _syncBkgndImages();
            }
        }
        void _syncBkgndImages()
        {
            panel2.BackgroundImage = this.BackgroundImage;
            panel3.BackgroundImage = this.BackgroundImage;
            panel4.BackgroundImage = this.BackgroundImage;
        }
        void _autoLayout()
        {
            Form frm = FindForm();
            if (frm != null)
            {
                if (frm.WindowState == FormWindowState.Minimized)
                    return;
            }

            Rectangle rcc = ClientRectangle;
            var panels = new Control[]
            {
                panel1, panel2, panel3, panel4
            };
            int y = 0;
            foreach ( Control panel in panels )
            {
                if (!panel.Visible)
                    continue;
                panel.Left = 0;
                panel.Width = rcc.Width;
                panel.Top = y;
                y = panel.Bottom;
            }
            panel4.Height = rcc.Bottom - panel4.Top - 1;
            
            _autoLayoutButtons();
            _autoLayoutListBox();
        }
        void _autoLayoutButtons()
        {
            var button1 = btnTryRun;
            var button2 = btnProductionRun;
            var button3 = btnStop;
            int w = this.ClientSize.Width;
            int iGap = button2.Left - button1.Right;
            iGap = iGap * 3 / 4;
            w = (w - button1.Left * 2 - iGap * 2) / 3;
            button1.Width = w;
            button2.Width = w;
            button2.Left = button1.Right + iGap;
            button3.Width = w;
            button3.Left = button2.Right + iGap;
        }
        void _autoLayoutListBox()
        {
            //int padding = 10;
            //var sz = listBlobs.Parent.ClientSize;
            //listBlobs.Height = sz.Height - listBlobs.Top - padding;
            //listBlobs.Width = sz.Width - listBlobs.Left * 2;
        }
        #endregion

        //Form IView.frmOwner => FindForm();
        Control IView.Window => this;
    }
}
