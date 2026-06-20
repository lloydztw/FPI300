#region AUTHOR
/*
 * 
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2026-06-20 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using AwFramework;
using System;
using System.Windows.Forms;

namespace EzAoiChipLocQC.Gui.Panels
{
    public partial class GvMajorClientPanel : UserControl, IView
    {
        public GvMajorClientPanel()
        {
            InitializeComponent();

            if (!DesignMode)
            {
                SizeChanged += (s, e) => BeginInvoke((Action)loadSplitterX);
                HandleCreated += (s, e) => BeginInvoke((Action)loadSplitterX);
                HandleDestroyed += (s, e) => saveSplitterX();
            }
        }

        public Control Window => this;
        public IvQcTrayView QcTrayView => this.gvQcTrayViewPanel1;
        public IvQcImageView CameraPanel => this.gvSingleMatchViewPanel2;

        #region PRIVATE_DATA
        void loadSplitterX()
        {
            var settings = Properties.Settings.Default;
            try
            {
                int x = settings.SplitterDistance;
                x = Math.Max(x, splitContainer1.Panel1MinSize);
                x = Math.Min(x, ClientSize.Width - splitContainer1.Panel2MinSize);
                splitContainer1.SplitterDistance = x;
            }
            catch
            {
            }
        }
        void saveSplitterX()
        {
            var settings = Properties.Settings.Default;
            if (settings.SplitterDistance != splitContainer1.SplitterDistance)
            {
                settings.SplitterDistance = splitContainer1.SplitterDistance;
                settings.Save();
            }
        }
        #endregion
    }
}
