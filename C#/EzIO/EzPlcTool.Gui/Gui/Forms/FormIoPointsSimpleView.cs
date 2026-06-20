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
using EzIO.Gui;
using EzIO.Mem;
using System.Collections.Generic;
using System.Windows.Forms;


namespace TestDemo_EzPlc_Fatek.Gui
{
    public partial class FormIoPointsSimpleView : Form, IoPointsView
    {
        public FormIoPointsSimpleView()
        {
            InitializeComponent();
            gvIoPointsSimpleView1.IsStatusPanelVisible = true;
        }

        #region GUI_LINKS
        Control IoPointsView.Window => this;
        IoPointsView _view => gvIoPointsSimpleView1;
        #endregion

        public void Attach(IEnumerable<IoPoint> ioPoints, IAutoScan autoScan)
        {
            _view.Attach(ioPoints, autoScan);
        }
        public void Update(IEnumerable<IoPoint> ioPoints)
        {
            _view.Update(ioPoints);
        }
    }
}
