#region AUTHOR
/*
 * 
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2026-04-14 Creation (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using AX.Gui;
using JetEazy.QMath;
using System;
using System.Windows.Forms;

namespace LaserAlignDX.Mvc.Gui
{
    public interface IvMotorsXYInkerUI
    {
        event EventHandler<InkerCoordsEventArgs> OnInkerCoordsUpdated;

        Control Window { get; }
        IvMotorJogView JogViewX { get; }
        IvMotorJogView JogViewY { get; }
        GwMotorSimpleGoPanel InkerUpPanel { get; }
        GwMotorSimpleGoPanel InkerDownPanel { get; }
        Control lblInkerIdlePos { get; }
        Button btnMoveToInkerIdlePos { get; }
        Button btnSetInkerIdlePos { get; }
        Button[] InkerCornerUpdateButtons { get; }
        Button[] InkerCornerMoveToButtons { get; }
    }


    public class InkerCoordsEventArgs : EventArgs
    {
        public int CornerID;
        public QVector MotorCoord;
    }
}
