#region AUTHOR
/*
 * EzIO Demo
 * 
 * Copyright (C) 2026
 * 
 * 2026-04-11 created by LeTian Chang
 * 
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System.Windows.Forms;

namespace TestDemo_EzPLC.Gui
{
    internal interface IvPressoDemoView
    {
        Form frmMain { get; }
        Control lblInfo { get; }
        Control lblEMG {  get; }
        Control lblCurrentForce { get; }
        NumericUpDown numProtectForce { get; }
        Button btnWriteProtectForce { get; }
        Button btnStartTest { get; }
        Button btnStopTest { get; }
    }
}
