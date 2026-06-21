using System.Windows.Forms;

namespace EzAoiChipLocQC.Gui.Panels
{
    public partial class GwRecipeInfo : UserControl, IvRecipeBriefView
    {
        public GwRecipeInfo()
        {
            InitializeComponent();
        }

        public Control Window => this;
        public Control lblInfo => label2;
        public PictureBox picThumbnail => pictureBox1;
        ComboBox IvRecipeBriefView.cboRecipeNames => this.cboRecipeNames;
    }
}
