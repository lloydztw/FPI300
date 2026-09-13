using System;
using System.Windows.Forms;

namespace LaserAlignDX.GA.mvc.Gui.UiRecipeEditor
{
    public partial class GwRcpLineGapBorderBtnsPanel : UserControl
    {
        public event EventHandler OnActivePageChanged;

        public GwRcpLineGapBorderBtnsPanel()
        {
            InitializeComponent();
            HandleCreated += (s, e) => initTabPages();
            BackColorChanged += (s, e) => syncBackColors();
            tabControl1.SelectedIndexChanged += (s, e) => OnActivePageChanged?.Invoke(s, e);
        }

        public int ActivePageID
        {
            get => tabControl1.SelectedIndex;
        }

        #region PRIVATE_FUNCTIONS
        void initTabPages()
        {
            foreach (Control tp in tabControl1.TabPages)
            {
                tp.BackColor = BackColor;
                tp.Padding = new Padding(0);
                tp.Margin = new Padding(0);
            }
        }
        void syncBackColors()
        {
            foreach(Control tp in tabControl1.TabPages)
                tp.BackColor = BackColor;
        }
        #endregion
    }
}
