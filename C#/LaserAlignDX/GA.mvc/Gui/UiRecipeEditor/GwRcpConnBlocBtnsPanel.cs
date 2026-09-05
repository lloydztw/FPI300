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
            alignCenterV(btnAdd);
            alignCenterV(btnDelete);
            alignCenterV(btnClear);

            //var owner = btnAdd.Parent;
            //var span = btnClear.Right - btnAdd.Left;
            //var x0 = (owner.ClientSize.Width - span) / 2;
            //var dx = x0 - btnAdd.Left;
            //btnAdd.Left += dx;
            //btnDelete.Left += dx;
            //btnClear.Left += dx;
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
