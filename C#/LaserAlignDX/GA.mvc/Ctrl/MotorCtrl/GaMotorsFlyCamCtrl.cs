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
using LaserAlignDX.AoiModel.Calib;
using LaserAlignDX.Mvc.Gui;
using System.Drawing;
using VsCommon.ControlSpace.IOSpace;
using Universal = Traveller106.Universal;

namespace LaserAlignDX.Mvc.Ctrl
{
    public partial class GaMotorsFlyCamCtrl : IxTickable
    {
        #region PLC
        IPlcIoFPIX3 _plc;
        #endregion

        #region PRIVATE_MODEL_DATA
        IAxis _motorX;
        IAxis _motorY;
        #endregion

        #region PRIVATE_CHILD_CTRLS
        GaMotorSafetyChecker _safetyCheckerZ;
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
            _motorX = Universal.GetMotorX(suckerID);
            _motorY = Universal.GetFlyCameraY();
            
            // MODEL (plc)
            _plc = GaBasicMotorUtil.PLCIO;

            // Safety Checker (Z軸安全代理) (只允許 attach 一次)
            if (_safetyCheckerZ == null)
            {
                _safetyCheckerZ = new GaMotorSafetyChecker();
                _safetyCheckerZ.OnPosChanged += (s, e) => updateGuiStatus(true);
            }
            
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
        }
        void connectEventHandlers()
        {
            _ui.btnGoTriggerPosX.Click += (s, e) => MoveToTiggerPosX();
            _ui.btnGoCameraPosY.Click += (s, e) => MoveToCameraPosY();
            _ui.btnVacuum.Click += (s, e) => ToggleVacuum();
            _ui.Window.HandleCreated += (s, e) => updateGuiStatus(true);   // updateVacuumColor(_plc != null && _plc.VacuumSucker1);
            _ui.Window.HandleDestroyed += (s, e) => CleanUp();
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

                // 保留將來寫回 PLC 配方內.
            }
        }

        public void Tick()
        {
            _safetyCheckerZ?.Tick();
            _jogCtrlX?.Tick();
            _jogCtrlY?.Tick();

            updateGuiStatus();
            checkAutoCloseCondition();
        }
        void CleanUp()
        {
            _safetyCheckerZ = null;
            _jogCtrlX = null;
            _jogCtrlY = null;
            JxInkerMotorSettings.Instance.Dispose();
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
        #endregion

        #region PRIVATE_FUNCTIONS
        void updateGuiStatus(bool updateDetails = false)
        {
            bool isAllSafe = _safetyCheckerZ.IsAllSafe();
            bool isReady = _motorX.IsOK && _motorY.IsOK && isAllSafe;

            _ui.btnGoTriggerPosX.Enabled = isReady;
            _ui.btnGoCameraPosY.Enabled = isReady;

            updateInkerZsColor();

            if (updateDetails)
            {
                updateVacuumColor(_plc != null && _plc.VacuumSucker1 == true);
                updateDetailsSafetyForXY();
            }
        }
        void updateInkerZsColor()
        {
            //bool atSafePos = _safetyCheckerZ.IsAtSafePos(_activeSuckerID);
            //bool atDownPos = _safetyCheckerZ.IsAtDownPos(_activeSuckerID);
            //_ui.InkerDownPanel.lblCurrentMotorPos.BackColor = atDownPos ? Color.Red : Color.Black;
            //_ui.InkerUpPanel.lblCurrentMotorPos.BackColor = atSafePos ? Color.Lime : Color.Black;
            //_ui.InkerUpPanel.lblCurrentMotorPos.ForeColor = atSafePos ? Color.Black : Color.White;
        }
        void updateDetailsSafetyForXY()
        {
            bool isAllSafe = _safetyCheckerZ.IsAllSafe();
            if (isAllSafe != _jogCtrlX.IsEnabled())
            {
                if (isAllSafe)
                {
                    _jogCtrlX?.SetEnable(true);
                    _jogCtrlY?.SetEnable(true);
                }
                else
                {
                    var reason = GaUtil.GetEnumDescription(Prompts.Warning_MotorXY_Disabled_By_Inker_Down);

                    if (!_safetyCheckerZ.IsAboveSafePos(SuckerRowEnum.S1))
                        reason += $"\n\r\n\r{GaUtil.GetEnumDescription(SuckerRowEnum.S1)}";
                    if (!_safetyCheckerZ.IsAboveSafePos(SuckerRowEnum.S2))
                        reason += $"\n\r\n\r{GaUtil.GetEnumDescription(SuckerRowEnum.S2)}";

                    _jogCtrlX?.SetEnable(false, reason);
                    _jogCtrlY?.SetEnable(false, reason);
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
