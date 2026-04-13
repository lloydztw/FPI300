#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-08-31 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System.Windows.Forms;

namespace AX.Gui
{
    public interface IvMotorJogView
    {
        Control Window { get; }

        Control lblAxisName { get; }
        Control lblStatusReady { get; }
        Control lblStatusMoving { get; }
        Control lblStatusError { get; }
        Control lblSignalLimitN { get; }
        Control lblSignalLimitP { get; }
        Control lblSignalOrg { get; }
        Control lblSignalSlow { get; }
        Control lblSignalINP { get; }
        Control lblCurrentPos { get; }
        Control lblCurrentSpeed { get; }

        Button btnHome { get; }
        Button btnJogForward { get; }
        Button btnJogBackward { get; }

        NumericUpDown numDist { get; }
        Button btnMoveTo { get; }
        Button btnMoveDeltaN { get; }
        Button btnMoveDeltaP { get; }

        Control lblUnit { get; }
    }
}
