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

            //HandleCreated += (s, e) =>
            //{
            //    new Action(() =>
            //    {
            //        System.Threading.Thread.Sleep(3000);
            //        this.BeginInvoke((Action)bindIoPoints);
            //    }).BeginInvoke(null, null);
            //};
        }

        Control IView.Window => this;
        public IoPointsView IoViewer => gvIoPointsSimpleView1;

        void bindIoPoints()
        {
            //var machine = Global.Machine;
            //var plc = Global.Machine?.PLC;
            //if (plc == null)
            //    return;

            //var ioMem = plc.IoMem;
            //var ioPoints = plc.IoMem.GetAllPoints();
            //var autoScan = plc.AutoScan;

            //// 只顯示 1-Bit 的 點位 
            //ioPoints.RemoveAll(p => p.Address.Bits != 1);
            //this.gvIoPointsSimpleView1.Attach(ioPoints, autoScan);

            //autoScan.Start();
        }
    }
}
