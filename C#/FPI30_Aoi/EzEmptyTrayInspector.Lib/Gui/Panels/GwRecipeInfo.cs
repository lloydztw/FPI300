using System.Windows.Forms;

namespace EzAoiEmptyTrayInspector.Gui.Panels
{
    public partial class GwRecipeInfo : UserControl, IvRecipeBriefView
    {
        public GwRecipeInfo()
        {
            InitializeComponent();
        }

        public Control Window => this;
        public Control lblInfo => label1;
        public PictureBox picThumbnail => pictureBox1;
        ComboBox IvRecipeBriefView.cboRecipeNames => this.cboRecipeNames;
    }
}
