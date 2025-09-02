using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace JetEazy.BasicSpace
{
    public class JzTransparentPanel : Panel
    {
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x00000020; // WS_EX_TRANSPARENT
                return cp;
            }
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            //e.Graphics.FillRectangle(new SolidBrush(Color.Gray), this.ClientRectangle);
            //Rectangle rectangle = new Rectangle(this.ClientRectangle.X, this.ClientRectangle.Y, this.ClientRectangle.Width, this.ClientRectangle.Height);
            //rectangle.Inflate(-5, -5);
            //e.Graphics.FillRectangle(new SolidBrush(this.BackColor), rectangle);
            e.Graphics.FillRectangle(new SolidBrush(this.BackColor), this.ClientRectangle);
        }
    }
}
