#region AUTHOR
/*
 * 
 * Copyright (c) 2023 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2024-10-03 ªì½Z (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using AwFramework;
using EzDualMatch.Ctrl;
using System.Windows.Forms;


namespace EzDualMatch.Gui.Panels
{
    public partial class GvDualMatchTabView : UserControl, IView
    {
        #region PRIVATE_DATA
        CviMouseSync _cvSyncBox;
        #endregion

        public GvDualMatchTabView()
        {
            InitializeComponent();
            gvSingleMatchViewPanel1.ViewID = 0;
            gvSingleMatchViewPanel2.ViewID = 1;

#if (OPT_SINGLE_SITE)
            gvSingleMatchViewPanel2.Visible = false;
            this.tabControl1.TabPages.RemoveAt(1);
            MatchViews = new IvSingleMatchView[] 
            { 
                gvSingleMatchViewPanel1 
            };
#else
            MatchViews = new IvSingleMatchView[]
            {
                gvSingleMatchViewPanel1,
                gvSingleMatchViewPanel2,
            };
            _cvSyncBox = new CviMouseSync();
            _cvSyncBox.Attach(MatchViews[0].ImageViewer, MatchViews[1].ImageViewer);
#endif
        }

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
