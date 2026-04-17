#region AUTHOR
/*
 * 
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2026-04-17 Creation (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using AX.Gui;
using System;
using System.Windows.Forms;

namespace LaserAlignDX.Mvc.Gui
{
    public interface IvMotorsFlyCamUI
    {
        Control Window { get; }
        IvMotorJogView JogViewX { get; }
        IvMotorJogView JogViewY { get; }
        Button btnGoTriggerPosX { get; }
        Button btnGoCameraPosY { get; }
        Button btnVacuum { get; }
        GwMotorSimpleGoPanel FocusPanelZ { get; }
    }
}
