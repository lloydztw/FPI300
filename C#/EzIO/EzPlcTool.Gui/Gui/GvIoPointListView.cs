using System.Windows.Forms;

namespace TestDemo_EzPlc_Fatek.Gui
{
    public partial class GvIoPointListView : UserControl
    {
        public GvIoPointListView()
        {
            InitializeComponent();
        }

        public int Columns
        {
            get;
            set;
        }
    }
}
