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

using System.Windows.Forms;
using TestDemo_Data;
using TestDemo_EzPLC.Ctrl;
using TestDemo_EzPLC.Gui;

namespace TestDemo_EzPLC
{
    /// <summary>
    /// 演示 Fatek IO 點位 (壓合機)
    /// 遵循 Model-View-Control 框架的演示
    /// </summary>
    internal class DemoApp1
    {
        public Form BuildFatekDemo(int comPort, bool isSim)
        {
            // Model
            var model = new PressoModel(comPort, isSim);

            // View
            var frmMain = new FormMain();

            // Ctrl
            var ctrl = new PressoDemoCtrl();
            ctrl.Attach(frmMain, model);

            return frmMain;
        }
    }
}
