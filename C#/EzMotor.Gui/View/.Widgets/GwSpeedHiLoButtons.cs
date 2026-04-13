#region AUTHOR
/*
 * 
 * Copyright (c) 2023 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2023-09-11 LeTian Chang, Revision.
 *      2008-12-01 LeTian Chang, Creation.
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System.Windows.Forms;


namespace AX.Gui
{
    public partial class GwSpeedHiLoButtons : UserControl
    {
        public GwSpeedHiLoButtons()
        {
            InitializeComponent();
            //SizeChanged += GwSpeedHiLoButtons_SizeChanged;
            //PaddingChanged += GwSpeedHiLoButtons_PaddingChanged;
            tableLayoutPanel1.Dock = DockStyle.Fill;
        }

        private void GwSpeedHiLoButtons_PaddingChanged(object sender, System.EventArgs e)
        {
            autoLayout();
        }
        private void GwSpeedHiLoButtons_SizeChanged(object sender, System.EventArgs e)
        {
            autoLayout();
        }

        void autoLayout()
        {
            //var gap0 = 3;
            //var gap1 = 1;
            //var csz = ClientSize;

            //lblSpeed.Top = Padding.Top;
            //lblSpeed.Left = Padding.Left;
            //lblSpeed.Width = csz.Width - Padding.Right - Padding.Left;

            //int H = csz.Height - lblSpeed.Height - Padding.Top - Padding.Bottom;
            //var h = (H - gap0 - gap1 * 2) / 3;

            //var btns = new Control[]
            //{
            //    rdoSpeedModeHI,
            //    rdoSpeedModeLO,
            //    rdoMicroStep
            //};
            //int y = lblSpeed.Bottom + gap0;
            //foreach (var btn in btns)
            //{
            //    btn.Left = lblSpeed.Left;
            //    btn.Width = lblSpeed.Width;
            //    btn.Top = y;
            //    btn.Height = h;
            //    y = btn.Bottom + gap1;
            //}
        }
    }
}
