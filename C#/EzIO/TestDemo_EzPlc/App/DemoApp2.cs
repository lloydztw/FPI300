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
using EzGlueDispenser.IO.S3;
using System.Windows.Forms;
using TestDemo_EzPLC.Ctrl;
using TestDemo_EzPLC.Gui;

namespace TestDemo_EzPLC
{
    /// <summary>
    /// 演示 Hcfa IO 點位 (點膠機)
    /// 遵循 Model-View-Control 框架的演示
    /// </summary>
    internal class DemoApp2
    {
        public Form BuildHcfaDemo(int tcpIpPort, bool isSim)
        {
            var settings = new EzTcpIpSettings() { Port = tcpIpPort, IsSim = isSim };

            // Model
            var model = new EzGlueDispenserIoS3(settings);

            // View
            var frmMain = new FormMainS3();

            // Ctrl
            var ctrl = new EzGlueDemoCtrl();
            ctrl.Attach(frmMain, model);

            return frmMain;
        }
    }
}
