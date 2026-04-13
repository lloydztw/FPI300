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
using System;

namespace LaserAlignDX.Mvc.Ctrl
{
    public class GaXYInkerMotorsJogCtrl : IxTickable
    {
        #region PRIVATE_DATA
        PLCMotionClass _motorX;
        PLCMotionClass _motorY;
        PLCMotionClass _focusMotorZ;
        PLCMotionClass _inkerMotorZ;
        GaCommonMotorJogCtrl _jogCtrlX;
        GaCommonMotorJogCtrl _jogCtrlY;
        GaSimpleMotorGoCtrl _focusMotorCtrl;
        GaSimpleMotorGoCtrl _inkerMotorCtrl;
        #endregion

        public void AttachMotorXY(IAxis motorX, IAxis motorY, IvMotorJogView viewX, IvMotorJogView viewY)
        {
            _motorX = motorX as PLCMotionClass;
            _motorY = motorY as PLCMotionClass;
            _jogCtrlX = new GaCommonMotorJogCtrl();
            _jogCtrlY = new GaCommonMotorJogCtrl();
            _jogCtrlX.Attach(viewX, _motorX);
            _jogCtrlY.Attach(viewY, _motorY);
        }
        public void AttachFocusMotorZ(IAxis focusMotorZ, GwMotorSimpleGoPanel panel)
        {
            var dataHolder = new GaSimpleMotorGoCtrl.MotorPosHolder
            {
                Get = getCameraFocusFromRecipe,
                Set = setCameraFocusToRecipe,
            };

            _focusMotorZ = focusMotorZ as PLCMotionClass;
            _focusMotorCtrl = new GaSimpleMotorGoCtrl();
            _focusMotorCtrl.Attach(_focusMotorZ, panel, dataHolder);
        }
        public void AttachInkerMotorZ(IAxis suckerMotorZ, GwMotorSimpleGoPanel panel)
        {
            var dataHolder = new GaSimpleMotorGoCtrl.MotorPosHolder
            {
                Get = getInkerZFromRecipe,
                Set = setInkerZToRecipe,
            };

            _inkerMotorZ = suckerMotorZ as PLCMotionClass;
            _inkerMotorCtrl = new GaSimpleMotorGoCtrl();
            _inkerMotorCtrl.Attach(_inkerMotorZ, panel, dataHolder);
        }
        public void Tick()
        {
            _jogCtrlX.Tick();
            _jogCtrlY.Tick();
            //_focusMotorCtrl.Tick();
            //_inkerMotorCtrl.Tick();
        }

        #region PRIVATE_FOCUS_MOTOR_FUNCTIONS
        bool isTinyDelta(double delta)
        {
            return Math.Abs(delta) < Traveller106.Universal.MOTOR_TINY_DELTA;
        }
        double getCameraFocusFromRecipe()
        {
            //var zFocus = _focusMode == FocusMode.FocusOnCarrier ?
            //                _xRecipe.zFocusOnCarrier :
            //                _xRecipe.zFocusOnChip;
            //return zFocus;
            return 0.0;
        }
        void setCameraFocusToRecipe(double motorPos)
        {
            //var old = getCameraFocusFromRecipe();
            //var delta = motorPos - old;
            //if (!isTinyDelta(delta))
            //{
            //    if (_focusMode == FocusMode.FocusOnCarrier)
            //    {
            //        _xRecipe.zFocusOnCarrier = (float)motorPos;
            //        _isModified = true;
            //    }
            //    else
            //    {
            //        _xRecipe.zFocusOnChip = (float)motorPos;
            //        _isModified = true;
            //    }
            //}
        }
        double getInkerZFromRecipe()
        {
            //var zFocus = _focusMode == FocusMode.FocusOnCarrier ?
            //                _xRecipe.zFocusOnCarrier :
            //                _xRecipe.zFocusOnChip;
            //return zFocus;
            return 0.0;
        }
        void setInkerZToRecipe(double motorPos)
        {
            //var old = getCameraFocusFromRecipe();
            //var delta = motorPos - old;
            //if (!isTinyDelta(delta))
            //{
            //    if (_focusMode == FocusMode.FocusOnCarrier)
            //    {
            //        _xRecipe.zFocusOnCarrier = (float)motorPos;
            //        _isModified = true;
            //    }
            //    else
            //    {
            //        _xRecipe.zFocusOnChip = (float)motorPos;
            //        _isModified = true;
            //    }
            //}
        }
        void syncFocusMotorCtrl()
        {
            _focusMotorCtrl?.UpdatePosHolderToGui();
        }
        #endregion
    }
}
