using System.Windows.Forms;

namespace LaserAlignDX.GA.mvc.Gui.UiRecipeEditor
{
    public partial class GwRcpConnBlocBtnsPanel : UserControl
    {
        public GwRcpConnBlocBtnsPanel()
        {
            InitializeComponent();
            groupBox1.SizeChanged += (s, e) => autoLayout();
        }

        #region PRIVATE_FUNCTIONS
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
        #endregion
    }
}
