using System;
using System.Windows.Forms;
using Traveller106;

namespace Eazy_Project_III.FormSpace
{
    public partial class BannerForm : Form
    {
        #region GUI_LINKS
        public Label lblVersionDate => label1;
        public ProgressBar ProBar => progressBar1;
        #endregion

        #region SINGLETON
        static BannerForm _instance;
        #endregion

        protected BannerForm()
        {
            InitializeComponent();
            ProBar.Visible = false;
            lblVersionDate.Text = Universal.VersionDate + "  " + Universal.OPTION;
            //Load += (s, e) => dumpBkgndImage();
        }

        public static bool IsShowing()
        {
            return _instance != null;
        }
        public static Form ShowBanner()
        {
            if (_instance == null)
            {
                _instance = new BannerForm
                {
                    StartPosition = FormStartPosition.CenterScreen,
                    TopMost = true,
                };
                _instance.Show();
                _instance.Refresh();
            }
            return _instance;
        }
        public static void CloseBanner()
        {
            _instance?.Close();
            _instance?.Dispose();
            _instance = null;
        }

        #region PRIVATE_FUNCTIONS
        void dumpBkgndImage()
        {
            this.BackgroundImage.Save("d:\\paso.log\\banner.png");
        }
        #endregion
    }
}
