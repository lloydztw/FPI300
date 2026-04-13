#region AUTHOR
/*
 * 
 * Copyright (c) 2023 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2023-09-11 LeTian Chang, Revision.
 *      2008-12-01 LeTian Chang, Creation.
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using AX.Gui;
using System.Windows.Forms;

namespace EzMotor.Gui.View.Wrappers
{
    internal class UiMotorJogView : IvMotorJogView
    {
        public UiMotorJogView(GwMotorAxisStatusCmd cmdPanel, GwMotorJogSticks jogPanel, JogStickOption option)
        {
            _cmdPanel = cmdPanel;
            _jogPanel = jogPanel;
            
            switch(option)
            {
                case JogStickOption.Vert:
                    btnJogBackward = _jogPanel.btnStickUp;
                    btnJogForward = _jogPanel.btnStickDown;
                    break;
                case JogStickOption.Horiz:
                    btnJogBackward = _jogPanel.btnStickLeft;
                    btnJogForward = _jogPanel.btnStickRight;
                    break;
                default:
                    btnJogBackward = _jogPanel.btnBackward;
                    btnJogForward = _jogPanel.btnForward;
                    break;
            }
        }

        GwMotorAxisStatusCmd _cmdPanel;
        GwMotorJogSticks _jogPanel;
        GwMotorAxisStatus _statusPanel => _cmdPanel.gwMotorAxisStatus1;

        Control IvMotorJogView.Window => _cmdPanel;
        Control IvMotorJogView.lblAxisName => _statusPanel.lblAxisName;
        Control IvMotorJogView.lblStatusReady => _statusPanel.lblStateReady;
        Control IvMotorJogView.lblStatusMoving => _statusPanel.lblStateMoving;
        Control IvMotorJogView.lblStatusError => _statusPanel.lblStateError;
        Control IvMotorJogView.lblSignalLimitN => _statusPanel.lblSignalLimitN;
        Control IvMotorJogView.lblSignalLimitP => _statusPanel.lblSignalLimitP;
        Control IvMotorJogView.lblSignalOrg => _statusPanel.lblSignalORG;
        Control IvMotorJogView.lblSignalSlow => _statusPanel.lblSignalSlow;
        Control IvMotorJogView.lblSignalINP => _statusPanel.lblSignalINP;

        Control IvMotorJogView.lblCurrentPos => _statusPanel.lblPosition;
        Control IvMotorJogView.lblCurrentSpeed => _statusPanel.lblSpeed;

        NumericUpDown IvMotorJogView.numDist => _cmdPanel.numDist;
        Button IvMotorJogView.btnMoveTo => _cmdPanel.btnMoveTo;
        Button IvMotorJogView.btnMoveDeltaN => _cmdPanel.btnMoveDeltaN;
        Button IvMotorJogView.btnMoveDeltaP => _cmdPanel.btnMoveDeltaP;
        Control IvMotorJogView.lblUnit => _cmdPanel.lblUnit;

        Button IvMotorJogView.btnHome => _jogPanel.btnStickHome;

        public Button btnJogForward
        {
            get; 
            private set;
        }
        public Button btnJogBackward
        {
            get;
            private set;
        }
    }
}
