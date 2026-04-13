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
using System.Windows.Forms;


namespace AX.Gui
{
    public partial class GwMotorSimpleJogV : UserControl, IvSimpleJogView, ICloneable
    {       
        public GwMotorSimpleJogV()
        {
            InitializeComponent();
            ++m_count;
            autoLayout();
            this.SizeChanged += (s, e) => autoLayout();
        }
        
        public string AxisName
        {
            get => grpMotor.Text;
            set => grpMotor.Text = value;
        }
        Button IvSimpleJogView.btnJogBackward => btnJogBackward;
        Button IvSimpleJogView.btnJogForward => btnJogForward;
        Button IvSimpleJogView.btnJogHome => btnJogHome;
        Control IvSimpleJogView.lblPos => lblPos;
        public GwMotorSimpleJogV Clone()
        {
            var pane = new GwMotorSimpleJogV();
            pane.BackColor = this.BackColor;
            pane.Location = this.Location;
            pane.Name = GetType().Name + m_count;
            pane.Size = this.Size;
            pane.TabIndex = 1000 + m_count;
            this.Parent.Controls.Add(pane);
            return pane;
        }
        object ICloneable.Clone() { return Clone(); }

        #region PRIVATE_MEMBER
        static int m_count = 0;
        void autoLayout()
        {
            grpMotor.Dock = DockStyle.Fill;
            tableLayoutPanel1.Dock = DockStyle.None;
            lblPos.Dock = DockStyle.Bottom;
            var rcc = this.ClientRectangle;
            int x = (rcc.Width - tableLayoutPanel1.Width) / 2;
            int y = (rcc.Height - tableLayoutPanel1.Height - lblPos.Height / 2) / 2;
            tableLayoutPanel1.Location = new System.Drawing.Point(x, y);
        }
        #endregion
    }
}
