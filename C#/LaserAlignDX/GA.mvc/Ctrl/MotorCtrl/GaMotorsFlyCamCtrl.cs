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
using JetEazy.Interface;
using JetEazy.Utils;
using LaserAlignDX.Mvc.Gui;
using System;
using System.Drawing;
using VsCommon.ControlSpace.IOSpace;
using Universal = Traveller106.Universal;

namespace LaserAlignDX.Mvc.Ctrl
{
    public partial class GaMotorsFlyCamCtrl : IxTickable
    {
        public event EventHandler<InkerCoordsEventArgs> OnInkerCoordsUpdated;

        #region PLC
        IPlcIoFPIX3 _plc;
        #endregion

        #region PRIVATE_MODEL_DATA
        IAxis _motorX;
        IAxis _motorY;
        IAxis _motorSuckerZ;
        double _suckerSafeZ;
        #endregion

        #region PRIVATE_CHILD_CTRLS
        GaCommonMotorJogCtrl _jogCtrlX;
        GaCommonMotorJogCtrl _jogCtrlY;
        GaMotorZCtrl _focusMotorCtrl;
        #endregion

        #region GUI_LINKS
        IvMotorsFlyCamUI _ui;
        IvMotorJogView _viewX => _ui?.JogViewX;
        IvMotorJogView _viewY => _ui?.JogViewY;
        GwMotorSimpleGoPanel _focusPanelZ => _ui.FocusPanelZ;
        #endregion

        #region RUNTIME_DATA
        bool _isFocusPosModified = false;
        bool _needsToAutoClose = false;
        #endregion

        public void Attach(IvMotorsFlyCamUI view)
        {
            if (_ui != null) return;
            _ui = view;
            initJogCtrls();
            connectEventHandlers();
        }

        #region PRIVATE_INIT_FUNCTIONS
        void initJogCtrls()
        {
            SuckerRowEnum suckerID = SuckerRowEnum.S1;

            // MODEL (motors)
            _motorSuckerZ = Universal.GetInkerMotor(suckerID);
            _motorX = Universal.GetMotorX(suckerID);
            _motorY = Universal.GetFlyCameraY();

            // MODEL (plc)
            _plc = GaBasicMotorUtil.PLCIO;
            
            // XY JOG CONTROL (只允許 attach 一次)
            if (_jogCtrlX == null)
            {
                _jogCtrlX = new GaCommonMotorJogCtrl();
                _jogCtrlY = new GaCommonMotorJogCtrl();
                _jogCtrlX.Attach(_viewX, _motorX);
                _jogCtrlY.Attach(_viewY, _motorY);
                _jogCtrlY.JogDirInverted = true;
            }

            // FlyCam Focus Motor Control (只允許 attach 一次)
            if (_focusMotorCtrl == null)
            {
                _focusMotorCtrl = new GaMotorZCtrl();
                _focusMotorCtrl.Attach(_focusPanelZ);
                _focusMotorCtrl.SetDataSrc(ZPosDataSrc.FlyCamFocusZ);
                _focusMotorCtrl.OnPosDataSrcModified += (s, e) => _isFocusPosModified = true;
            }

            // PLC : GetSafeZ
            if (_plc != null)
            {
                _suckerSafeZ = _plc.GetSafeZ(suckerID);
            }
        }
        void connectEventHandlers()
        {
            _ui.btnGoTriggerPosX.Click += (s, e) => MoveToTiggerPosX();
            _ui.btnGoCameraPosY.Click += (s, e) => MoveToCameraPosY();
            _ui.btnVacuum.Click += (s, e) => ToggleVacuum();
            _ui.Window.HandleCreated += (s, e) =>
            {
                bool on = _plc != null && _plc.VacuumSucker1;
                updateVacuumColor(on);
            };
        }
        #endregion

        public void MoveToTiggerPosX()
        {
            if (_plc == null)
                return;

            double pos = _plc.GetFlyCamTriggerX();
            _jogCtrlX.BeginMoveTo(pos, silent: false);
        }
        public void MoveToCameraPosY()
        {
            if (_plc == null)
                return;

            double pos = _plc.GetFlyCamSnapshotY();
            _jogCtrlY.BeginMoveTo(pos, silent: false);
        }
        public void ToggleVacuum()
        {
            if (_plc == null)
                return;

            var on = !_plc.VacuumSucker1;
            _plc.VacuumSucker1 = on;
            updateVacuumColor(on);
        }
        public void SaveModification()
        {
            if (_isFocusPosModified)
            {
                _isFocusPosModified = false;
                // RESERVED
            }
        }

        public void Tick()
        {
            checkInkerSafety();
            
            _jogCtrlX?.Tick();
            _jogCtrlY?.Tick();

            checkAutoCloseCondition();
        }

        #region PRIVATE_FUNCTIONS
        void updateVacuumColor(bool on)
        {
            _ui.btnVacuum.BackColor = on ? Color.Pink : _ui.Window.BackColor;

            if (on)
                _ui.btnVacuum.Text = _ui.btnVacuum.Text.Replace("OFF", "ON");
            else
                _ui.btnVacuum.Text = _ui.btnVacuum.Text.Replace("ON", "OFF");
        }
        void checkInkerSafety()
        {
            //>>> 檢查 Sucker1 如果在下位, 就禁止移動 XY

            var currentSuckerZ = _motorSuckerZ.GetPos();

            if (!GaBasicMotorUtil.IsTinyDelta(_suckerSafeZ - currentSuckerZ))
            {
                if (currentSuckerZ > _suckerSafeZ)
                {
                    var reason = GaUtil.GetEnumDescription(Prompts.Warning_MotorXY_Disabled_By_Inker_Down);
                    _jogCtrlX.SetEnable(false, reason);
                    _jogCtrlY.SetEnable(false, reason);
                }
                else
                {
                    _jogCtrlX.SetEnable(true);
                    _jogCtrlY.SetEnable(true);
                }
            }
        }
        void checkAutoCloseCondition()
        {
            if (_needsToAutoClose)
            {
                if (_motorX.IsOK && _motorY.IsOK)
                {
                    _ui?.Window?.FindForm()?.Close();
                }
            }
        }
        #endregion
    }
}
