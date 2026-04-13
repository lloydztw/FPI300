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

using System;
using System.Drawing;
using System.Windows.Forms;


namespace AX.Gui
{
    public partial class GwMotorJogSticks : UserControl
    {
        JogStickOption _option = JogStickOption.XY;

        public GwMotorJogSticks()
        {
            InitializeComponent();
            SizeChanged += (s, e) => autoLayout();
            autoLayout();
            updateStickSymbols();
        }

        public Button btnBackward { get; private set; }
        public Button btnForward { get; private set; }

        #region PRIVATE_FUNCTIONS
        private void updateStickSymbols()
        {
            btnStickUp.Text = "↑";
            btnStickDown.Text = "↓";
            
            if (_option == JogStickOption.Theta)
            {
                btnStickLeft.Text = "⭮";
                btnStickRight.Text = "⭯";
            }
            else
            {
                btnStickLeft.Text = "←";
                btnStickRight.Text = "→";
            }

            btnBackward = btnStickLeft;
            btnForward = btnStickRight;
            switch (_option)
            {
                case JogStickOption.XY:
                    btnStickUp.Visible = true;
                    btnStickDown.Visible = true;
                    btnStickLeft.Visible = true;
                    btnStickRight.Visible = true;
                    break;
                case JogStickOption.Vert:
                    btnStickUp.Visible = true;
                    btnStickDown.Visible = true;
                    btnStickLeft.Visible = false;
                    btnStickRight.Visible = false;
                    btnBackward = btnStickUp;
                    btnForward = btnStickDown;
                    break;
                case JogStickOption.Theta:
                    btnStickUp.Visible = false;
                    btnStickDown.Visible = false;
                    btnStickLeft.Visible = true;
                    btnStickRight.Visible = true;
                    btnBackward = btnStickRight;
                    btnForward = btnStickLeft;
                    break;
                case JogStickOption.Horiz:
                default:
                    btnStickUp.Visible = false;
                    btnStickDown.Visible = false;
                    btnStickLeft.Visible = true;
                    btnStickRight.Visible = true;
                    break;
            }
        }
        private void autoLayout()
        {
            int gap = 2;
            var csz = ClientSize;
            int H = Math.Min(csz.Width, csz.Height);
            int h = (H - gap * 2) / 3;
            var bsz = new Size(h, h);
            // Home
            btnStickHome.Size = bsz;
            btnStickHome.Top = (csz.Height - btnStickHome.Height) / 2;
            btnStickHome.Left = (csz.Width - btnStickHome.Width) / 2;
            // Up/Down
            btnStickUp.Size = bsz;
            btnStickUp.Left = btnStickHome.Left;
            btnStickUp.Top = btnStickHome.Top - btnStickUp.Height - gap;
            btnStickDown.Size = bsz;
            btnStickDown.Left = btnStickHome.Left;
            btnStickDown.Top = btnStickHome.Top + btnStickHome.Height + gap;
            // Left/Right
            btnStickLeft.Size = bsz;
            btnStickLeft.Left = btnStickHome.Left - btnStickDown.Width - gap;
            btnStickLeft.Top = btnStickHome.Top;
            btnStickRight.Size = bsz;
            btnStickRight.Left = btnStickHome.Left + btnStickHome.Width + gap;
            btnStickRight.Top = btnStickHome.Top;
        }
        #endregion

        public JogStickOption JogStickOption
        {
            get
            {
                return _option;
            }
            set
            {
                if (true || _option != value)
                {
                    _option = value;
                    if (true || IsHandleCreated)
                    {
                        autoLayout();
                        updateStickSymbols();
                    }
                }
            }
        }
    }
}
