#region AUTHOR
/*
 * 
 * Copyright (c) 2023 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2024-10-03 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using AwFramework;
using EzDualMatch.Ctrl;
using JetEazy;
using System.Windows.Forms;

namespace EzDualMatch.Gui.Panels
{
    public partial class GvDualMatchLRView : UserControl, IvDualMatchView
    {
        #region PRIVATE_DATA
        CviMouseSync _cvSyncBox;
        int _splitDisplayMode = -1;
        #endregion

        public GvDualMatchLRView()
        {
            InitializeComponent();
            gvSingleMatchView1.ViewID = 0;
            gvSingleMatchView2.ViewID = 1;

#if (OPT_SINGLE_SITE)
            MatchViews = new IvSingleMatchView[] 
            {
                gvSingleMatchView1
            };
            setSplitDisplayMode(0);
            setSplitDisplayMode(1);
#else
            MatchViews = new IvSingleMatchView[]
            {
                gvSingleMatchView1,
                gvSingleMatchView2,
            };
            _cvSyncBox = new CviMouseSync();
            _cvSyncBox.Attach(MatchViews[0].ImageViewer, MatchViews[1].ImageViewer);

            gvSingleMatchView1.picIcon.Click += (s, e) => toggleSplitPanelA();
            gvSingleMatchView2.picIcon.Click += (s, e) => toggleSplitPanelB();
            setSplitDisplayMode(0);
#endif
        }

        #region PRIVATE_FUNCTIONS
        void setSplitDisplayMode(int mode)
        {
            mode %= 3;
            if (_splitDisplayMode != mode)
            {
                _splitDisplayMode = mode % 3;
                switch (_splitDisplayMode)
                {
                    case 1:
                        splitContainer1.SplitterDistance = splitContainer1.Width;
                        //splitContainer1.Panel1Collapsed = false;
                        //splitContainer1.Panel2Collapsed = true;
                        rebuildViewport(MatchViews[0]);
                        break;
                    case 2:
                        splitContainer1.SplitterDistance = 0;
                        //splitContainer1.Panel2Collapsed = false;
                        //splitContainer1.Panel1Collapsed = true;
                        rebuildViewport(MatchViews[1]);
                        break;
                    default:
                        splitContainer1.SplitterDistance = splitContainer1.Width / 2;
                        splitContainer1.Panel1MinSize = 1;
                        splitContainer1.Panel2MinSize = 1;
                        splitContainer1.SplitterWidth = 2;
                        //splitContainer1.Panel1Collapsed = false;
                        //splitContainer1.Panel2Collapsed = false;
                        splitContainer1.IsSplitterFixed = false;
                        splitContainer1.Dock = DockStyle.Fill;
                        rebuildViewport(MatchViews[0]);
                        if (MatchViews.Length > 1)
                            rebuildViewport(MatchViews[1]);
                        break;
                }
            }
        }
        void toggleSplitPanelA()
        {
            if (_splitDisplayMode == 1)
                setSplitDisplayMode(0);
            else
                setSplitDisplayMode(1);
        }
        void toggleSplitPanelB()
        {
            if (_splitDisplayMode == 2)
                setSplitDisplayMode(0);
            else
                setSplitDisplayMode(2);
        }
        void rebuildViewport(IvSingleMatchView view)
        {
            var rect = view.ImageViewer.GetViewportRect();
            var pt = Qcvt.Center(ref rect);
            view.ImageViewer.ZoomIn(pt);
            view.ImageViewer.ZoomOut(pt);
        }
        #endregion

        //Form IView.frmOwner => FindForm();
        Control IView.Window => this;
        public IvSingleMatchView[] MatchViews
        {
            get;
            private set;
        }
        public CviMouseSync SyncBox
        {
            get => _cvSyncBox;
        }
    }
}
