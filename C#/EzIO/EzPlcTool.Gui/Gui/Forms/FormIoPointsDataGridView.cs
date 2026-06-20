#region AUTHOR
/*
 * EzIO GUI
 * Copyright (C) 2026
 * 2026-04-11 created by LeTian Chang
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzIO.Device;
using EzIO.Mem;
using System.Collections.Generic;
using System.Windows.Forms;


namespace EzIO.Gui
{
    public partial class FormIoPointsDataGridView : Form, IoPointsView
    {
        public FormIoPointsDataGridView()
        {
            InitializeComponent();
        }

        #region GUI_LINKS
        Control IoPointsView.Window => this;
        #endregion

        public void Attach(IEnumerable<IoPoint> ioPoints, IAutoScan autoScan)
        {
            gvIoPointDataGridView1.Attach(ioPoints, autoScan);
        }
        public void Update(IEnumerable<IoPoint> ioPoints)
        {
            gvIoPointDataGridView1.Update(ioPoints);
        }
    }
}
