using System.Windows.Forms;

namespace LaserAlignDX.GA.mvc.Gui.UiRecipeEditor
{
    public partial class GwRcpQrCodeBtnsPanel : UserControl
    {
        public GwRcpQrCodeBtnsPanel()
        {
            InitializeComponent();
            //groupBox1.SizeChanged += (s, e) => autoLayout();
        }

#if (OPT_由_ANCHOR_設定來自動_LAYOUT)
        void autoLayout()
        {
            alignCenterV(btnTryQrCode);
            alignCenterV(rtbCodeContent);
            rtbCodeContent.Width = ClientSize.Width - btnTryQrCode.Left - rtbCodeContent.Left;
        }

        void alignCenterV(Control c)
        {
            var ccSize = c.Parent.ClientSize;
            var y = (ccSize.Height - c.Height) / 2;
            c.Top = y;
        }
#endif
    }
}
