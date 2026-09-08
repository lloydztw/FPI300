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
            alignCenterV(lblDetectRegions);
            alignCenterV(btnAdd);
            alignCenterV(btnDelete);
            alignCenterV(btnClearAll);
        }

        void alignCenterV(Control c)
        {
            var ccSize = c.Parent.ClientSize;
            var y = (ccSize.Height - c.Height) / 2;
            c.Top = y;
        }
    }
}
