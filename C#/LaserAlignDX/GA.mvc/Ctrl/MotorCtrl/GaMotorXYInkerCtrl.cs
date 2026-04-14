#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2026-04-13 LeTian Chang, Creation
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using AX.Gui;
using JetEazy.ControlSpace.MotionSpace;
using JetEazy.Interface;
using Traveller106;
using Universal = Traveller106.Universal;


namespace LaserAlignDX.Mvc.Ctrl
{
    public class GaMotorXYInkerCtrl : IxTickable
    {
        #region PRIVATE_MODEL_DATA
        PLCMotionClass _motorX;
        PLCMotionClass _motorY;
        PLCMotionClass _inkerMotorZ;
        double _backupInkerMotorPos;
        #endregion

        #region PRIVATE_CHILD_CTRLS
        GaCommonMotorJogCtrl _jogCtrlX;
        GaCommonMotorJogCtrl _jogCtrlY;
        GaMotorZCtrl _inkerDownCtrl;
        #endregion

        #region GUI_LINKS
        IvMotorJogView _viewX;
        IvMotorJogView _viewY;
        GwMotorSimpleGoPanel _inkerUpPanel;
        GwMotorSimpleGoPanel _inkerDownPanel;
        SuckerRowEnum _activeInkerID;
        #endregion

        #region RUNTIME_DATA
        bool _isInkerPosModified = false;
        #endregion

        public void Attach(IvMotorJogView viewX,
                           IvMotorJogView viewY,
                           GwMotorSimpleGoPanel inkerUpPanel,
                           GwMotorSimpleGoPanel inkerDownPanel)
        {
            // VIEW
            _viewX = viewX;
            _viewY = viewY;
            _inkerUpPanel = inkerUpPanel;
            _inkerDownPanel = inkerDownPanel;
        }

        public void SetJogTargets(CarrierEnum C, SuckerRowEnum S)
        {
            // MODEL
            _motorX = Universal.GetMotorX(S);
            _motorY = Universal.GetMotorY(C);
            _inkerMotorZ = Universal.GetInkerMotor(S);
            _backupInkerMotorPos = _inkerMotorZ.GetPos();
            _activeInkerID = S;

            // XY JOG CONTROL (只允許 attach 一次)
            if (_jogCtrlX == null)
            {
                _jogCtrlX = new GaCommonMotorJogCtrl();
                _jogCtrlY = new GaCommonMotorJogCtrl();
                _jogCtrlX.Attach(_viewX, _motorX);
                _jogCtrlY.Attach(_viewY, _motorY);
            }

            // Inker Down CONTROL (只允許 attach 一次)
            if (_inkerDownCtrl == null)
            {
                _inkerDownCtrl = new GaMotorZCtrl();
                _inkerDownCtrl.Attach(_inkerDownPanel);
                _inkerDownCtrl.SetDataSrc(S);
                _inkerDownCtrl.OnPosDataSrcModified += (s, e) => _isInkerPosModified = true;

                // Inker Up Panel (只簡單保存當下的 motor pos)
                _inkerUpPanel.btnSettings.Visible = false;
                _inkerUpPanel.lblCurrentMotorPos.Text = $"{_backupInkerMotorPos:0.000}";
                _inkerUpPanel.btnMotorGo.Click += (s, e) => RestoreInkerMotorPos();
            }
        }

        public void Tick()
        {
            _jogCtrlX?.Tick();
            _jogCtrlY?.Tick();
        }

        public void RestoreInkerMotorPos()
        {
            string displayName = $"{_activeInkerID} Inker 馬達";
            _inkerMotorZ?.PromptMoveTo(_backupInkerMotorPos, displayName);
        }
        public void SaveModification()
        {
            if (_isInkerPosModified)
            {
                _isInkerPosModified = false;
                INI.Instance.Save();
            }
        }
    }
}
