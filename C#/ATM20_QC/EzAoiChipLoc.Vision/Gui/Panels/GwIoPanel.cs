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

using System;
using System.Windows.Forms;
using AwFramework;
using EzIO.Gui;

namespace EzAoiChipLocQC.Gui.Panels
{
    public partial class GwIoPanel : UserControl, IView
    {
        public GwIoPanel()
        {
            InitializeComponent();
        }

        #region GUI_LINKS
        Control IView.Window => this;
        public IoPointsView IoViewer => gvIoPointsSimpleView1;
        #endregion

        void bindIoPoints()
        {
            var plc = Global.Machine?.PLC;
            if (plc != null)
            {
                IoViewer.Attach(plc.IterIoPoints(true), plc?.AutoScan);
            }
        }
    }
}
