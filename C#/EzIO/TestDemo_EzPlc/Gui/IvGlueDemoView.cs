#region AUTHOR
/*
 * EzIO Demo
 * 
 * Copyright (C) 2026
 * 
 * 2026-04-23 created by LeTian Chang
 * 
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System.Windows.Forms;

namespace TestDemo_EzPLC.Gui
{
    internal interface IvGlueDemoView
    {
        Form frmMain { get; }
        Control lblInfo { get; }
        Control lblEMG {  get; }
        NumericUpDown numGlueTime { get; }
        NumericUpDown numUVTime { get; }
        Button btnStartTest { get; }
        Button btnStopTest { get; }
        Button btnWrite { get; }
    }
}
