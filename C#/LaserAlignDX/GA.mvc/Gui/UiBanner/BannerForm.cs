#region AUTHOR
/*
 * 
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2026-05-15 改版, 使用獨立 Thread 驅動動態 ProgressBar (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System.Threading;
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
            lblVersionDate.Text = Universal.VersionDate + $" (ver {Application.ProductVersion})"; // + Universal.OPTION;
        }

        public static bool IsShowing()
        {
            return _instance != null;
        }
        public static void ShowBanner()
        {
            if (_instance == null)
            {
#if(OPT_THREAD_BANNER)
                Thread bannerThread = new Thread(() =>
                {
                    _instance = new BannerForm
                    {
                        StartPosition = FormStartPosition.CenterScreen,
                        TopMost = true,
                    };
                    _instance.Load += (s,e) => _instance.startAnimation();

                    // 關鍵：使用 Application.Run 啟動該執行緒的專屬訊息循環
                    Application.Run(_instance);
                });

                bannerThread.SetApartmentState(ApartmentState.STA); // 必須是 STA
                bannerThread.IsBackground = true;
                bannerThread.Start();
#else
                _instance = new BannerForm();
                _instance.TopMost = true;
                _instance.ProBar.Visible = false;
                _instance.Cursor = Cursors.AppStarting;
                _instance.Show();
                _instance.Refresh();
                Application.UseWaitCursor = true;
#endif
            }
        }
        public static void CloseBanner()
        {
            if (_instance != null)
            {
                // 跨執行緒關閉視窗的正確做法
                if (_instance.InvokeRequired)
                {
                    _instance.Invoke(new MethodInvoker(() => CloseBanner()));
                }
                else
                {
                    _instance.Close();
                    Application.UseWaitCursor = false;
                }
                _instance = null;
            }
        }

        #region PRIVATE_FUNCTIONS
        void startAnimation()
        {
            dumpBkgndImage();
            ProBar.Style = ProgressBarStyle.Marquee; // 設定為跑馬燈
            ProBar.MarqueeAnimationSpeed = 30;       // 數值越小跑越快
            ProBar.Maximum = 100;
            ProBar.Visible = true;
            this.Cursor = Cursors.WaitCursor;
            this.Refresh();
        }
        void dumpBkgndImage()
        {
            //this.BackgroundImage.Save("d:\\paso.log\\banner.png");
        }
        #endregion
    }
}
