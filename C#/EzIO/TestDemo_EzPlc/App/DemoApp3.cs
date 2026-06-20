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

using EzComm;
using System.Windows.Forms;
using TestDemo_EzPLC.Ctrl;
using TestDemo_EzPLC.Gui;
using Traveller106.IO;

namespace TestDemo_EzPLC
{
    /// <summary>
    /// 演示 Omron IO 點位 (德龍 Traveller106)
    /// 遵循 Model-View-Control 框架的演示
    /// </summary>
    internal class DemoApp3
    {
        public Form BuildOmronDemo(int tcpIpPort, bool isSim)
        {
            var settings = new EzTcpIpSettings() { Port = tcpIpPort, IsSim = isSim };

            // Model
            var model = new FPIX3_IO(settings);

            // View
            var frmMain = new FormMainT3();

            // Ctrl
            var ctrl = new EzTravellerDemoCtrl();
            ctrl.Attach(frmMain, model);

            return frmMain;
        }
    }
}
