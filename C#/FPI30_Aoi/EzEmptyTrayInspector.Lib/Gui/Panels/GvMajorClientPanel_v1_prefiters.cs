using AwFramework;
using System.Windows.Forms;

namespace EzAoiEmptyTrayInspector.Gui.Panels
{
    public partial class GvMajorClientPanel : UserControl, IView
    {
        public GvMajorClientPanel()
        {
            InitializeComponent();
            gvSingleMatchViewPanel1.ViewID = 0;
            gvSingleMatchViewPanel2.ViewID = 1;
            FilteredViewPanel.Window.VisibleChanged += FilteredViewPanel_VisibleChanged;
        }

        #region GUI_LINKS
        Control IView.Window => this;
        public IvSingleMatchView OrignalViewPanel => gvSingleMatchViewPanel1;
        public IvSingleMatchView FilteredViewPanel => gvSingleMatchViewPanel2;
        #endregion

        #region EVENT_HANDLERS
        private void FilteredViewPanel_VisibleChanged(object sender, System.EventArgs e)
        {
            OrignalViewPanel.Window.Visible = !FilteredViewPanel.Window.Visible;
        }
        #endregion
    }
}
