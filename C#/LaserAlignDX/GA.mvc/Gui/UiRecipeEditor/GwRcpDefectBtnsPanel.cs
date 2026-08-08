using System.Windows.Forms;

namespace LaserAlignDX.GA.mvc.Gui.UiRecipeEditor
{
    public partial class GwRcpDefectBtnsPanel : UserControl
    {
        public GwRcpDefectBtnsPanel()
        {
            InitializeComponent();
            groupBox1.SizeChanged += (s, e) => autoLayout();
        }

        void autoLayout()
        {
            alignCenterV(btnDefectAdd);
            alignCenterV(btnDefectDelete);
            alignCenterV(btnDefectClear);

            var owner = btnDefectAdd.Parent;
            var span = btnDefectClear.Right - btnDefectAdd.Left;
            var x0 = (owner.ClientSize.Width - span) / 2;
            var dx = x0 - btnDefectAdd.Left;
            btnDefectAdd.Left += dx;
            btnDefectDelete.Left += dx;
            btnDefectClear.Left += dx;
        }

        void alignCenterV(Control c)
        {
            var ccSize = c.Parent.ClientSize;
            var y = (ccSize.Height - c.Height) / 2;
            c.Top = y;
        }
    }
}
