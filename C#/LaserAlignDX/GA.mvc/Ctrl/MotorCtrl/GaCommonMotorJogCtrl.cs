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
using ComtactAnglePlus.FromCommon;
using JetEazy.ControlSpace.MotionSpace;
using JetEazy.FormSpace;
using JetEazy.Interface;
using JetEazy.Utils;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace LaserAlignDX.Mvc.Ctrl
{
    public class GaCommonMotorJogCtrl : IxTickable
    {
        #region MOTOR
        PLCMotionClass _motor;
        #endregion

        #region GUI_LINKS
        IvMotorJogView _ui;

        Control lblError => _ui.lblStatusError;
        Control lblPositionNow => _ui.lblCurrentPos;
        Control lblCurrentSpeed => _ui.lblCurrentSpeed;

        Button btnHome => _ui.btnHome;
        Button btnForward => _ui.btnJogForward;
        Button btnBackward => _ui.btnJogBackward;

        NumericUpDown numGoPosition => _ui.numDist;
        Button btnAdd => _ui.btnMoveDeltaP;
        Button btnSub => _ui.btnMoveDeltaN;
        Button btnGo => _ui.btnMoveTo;
        #endregion

        #region RUNTIME_DATA
        bool _isMoving = false;
        #endregion

        public void Attach(IvMotorJogView view, PLCMotionClass motor)
        {
            _ui = view;
            _motor = motor;

            var window = view.Window;
            if (!window.IsHandleCreated)
                window.HelpRequested += (s, e) => Init();
            else
                Init();

            window.HandleDestroyed += (s, e) => CleanUp();
        }

        void Init()
        {
            UpdateMotorName();

            btnForward.MouseDown += (s, e) => JogForward();
            btnForward.MouseUp += (s, e) => StopMotor();
            btnBackward.MouseDown += (s, e) => JogBackward();
            btnBackward.MouseUp += (s, e) => StopMotor();

            btnHome.Click += btnHome_Click;
            btnAdd.Click += btnAdd_Click;
            btnSub.Click += btnSub_Click;
            btnGo.Click += btnGo_Click;

            lblError.DoubleClick += (s, e) => ResetMotor();
        }

        void btnGo_Click(object sender, EventArgs e)
        {
            var ret = VsMessageBox.Question(GaUtil.GetEnumDescription(Prompts.Question_Motor_GoTo_Pos));
            if (DialogResult.Cancel == ret)
                return;

            _motor?.Go((float)numGoPosition.Value);
            _isMoving = true;
        }
        void btnSub_Click(object sender, EventArgs e)
        {
            _motor?.Go(_motor.PositionNow - (float)numGoPosition.Value);
            _isMoving = true;
        }
        void btnAdd_Click(object sender, EventArgs e)
        {
            _motor?.Go((float)numGoPosition.Value + _motor.PositionNow);
            _isMoving = true;
        }
        void btnHome_Click(object sender, EventArgs e)
        {
            var ret = VsMessageBox.Question(GaUtil.GetEnumDescription(Prompts.Question_Motor_Home));
            if (DialogResult.Cancel == ret)
                return;

            _motor.Home();
            _isMoving = true;
        }

        void WriteSettingsToPLC()
        {
            //_motor.GOSPEED = _motorSettings.GOSPEED;
            //_motor.SetSpeed(SpeedTypeEnum.GO);
            //_motor.SaveData(MotionAddressEnum.GOSPEED, _motor.GOSPEED.ToString());

            //_motor.GOSLOWSPEED = _motorSettings.GOSLOWSPEED;
            ////MOTION.SetSpeed(SpeedTypeEnum.GOSLOW);
            //_motor.SaveData(MotionAddressEnum.GOSLOWSPEED, _motor.GOSLOWSPEED.ToString());

            //_motor.MANUALSPEED = _motorSettings.MANUALSPEED;
            //_motor.SetSpeed(SpeedTypeEnum.MANUAL);
            //_motor.SaveData(MotionAddressEnum.MANUALSPEED, _motor.MANUALSPEED.ToString());

            //_motor.MANUALSLOWSPEED = _motorSettings.MANUALSLOWSPEED;
            ////MOTION.SetSpeed(SpeedTypeEnum.MANUALSLOW);
            //_motor.SaveData(MotionAddressEnum.MANUALSLOWSPEED, _motor.MANUALSLOWSPEED.ToString());

            //_motor.HOMEHIGHSPEED = _motorSettings.HOMEHIGHSPEED;
            //_motor.SetSpeed(SpeedTypeEnum.HOMEHIGH);
            //_motor.SaveData(MotionAddressEnum.HOMEHIGHSPEED, _motor.HOMEHIGHSPEED.ToString());

            //_motor.HOMESLOWSPEED = _motorSettings.HOMESLOWSPEED;
            //_motor.SetSpeed(SpeedTypeEnum.HOMESLOW);
            //_motor.SaveData(MotionAddressEnum.HOMESLOWSPEED, _motor.HOMESLOWSPEED.ToString());

            //_motor.READYPOSITION = _motorSettings.READYPOSITION;// MOTION.PositionNow;
            //_motor.TESTPOSITION = _motorSettings.TESTPOSITION;
            //_motor.SaveData();
        }
        void ResetMotor()
        {
            _motor?.Reset();
        }
        void StopMotor()
        {
            _motor?.Stop();
            _isMoving = false;
        }
        void JogForward()
        {
            _motor?.Forward();
        }
        void JogBackward()
        {
            _motor?.Backward();
        }

        void PollingMotorStatus()
        {
            // RESERVED
        }
        void CleanUp()
        {
            // RESERVED
        }

        public void Tick()
        {
            PollingMotorStatus();
            UpdateMotorStatus();
        }

        #region PRIVATE_GUI_UPDATE_FUNCTIONS
        Color _normalColor = SystemColors.Control;
        void UpdateMotorStatus()
        {
            var motor = _motor;
            if (motor == null)
                return;

            var isOK = motor.IsOK;
            var isError = motor.IsError;
            var isINP = motor.IsOnSite;
            
            if (isINP)
                _isMoving = false;

            var isMoving = _isMoving && isOK && !isError;
            var isReady = isOK && !isError;
            var isHome = motor.IsHome;
            var currentPos = motor.PositionNow;
            var currentSpeed = motor.GetSpeed(SpeedTypeEnum.GO);

            updateBackColor(_ui.lblStatusReady, isReady ? Color.Lime : _normalColor);
            updateBackColor(_ui.lblStatusMoving, isMoving ? Color.Lime : _normalColor);
            updateBackColor(_ui.lblStatusError, isError ? Color.Red : _normalColor);

            updateBackColor(btnHome, isHome ? _normalColor : Color.Red);
            updateBackColor(btnGo, isReady ? _normalColor : Color.Red);
            updateBackColor(btnAdd, isReady ? _normalColor : Color.Red);
            updateBackColor(btnSub, isReady ? _normalColor : Color.Red);
            updateBackColor(btnBackward, isReady ? _normalColor : Color.Red);
            updateBackColor(btnForward, isReady ? _normalColor : Color.Red);

            updateBackColor(_ui.lblSignalLimitP, motor.IsReachUpperBound ? Color.Pink : _normalColor);
            updateBackColor(_ui.lblSignalLimitN, motor.IsReachLowerBound ? Color.Pink : _normalColor);
            updateBackColor(_ui.lblSignalOrg, isHome ? Color.Gold : _normalColor);
            updateBackColor(_ui.lblSignalINP, isINP ? Color.Lime : _normalColor);

            updateText(lblPositionNow, $"{currentPos:0.000}");
            updateText(lblCurrentSpeed, $"{currentSpeed:0.000}");
        }
        void UpdateMotorName()
        {
            updateText(_ui?.lblAxisName, _motor?.MOTIONALIAS);
            updateText(_ui?.lblUnit, _motor?.MOTIONUNIT);
        }
        void updateText(Control c, string text)
        {
            if (c != null && text != null)
                c.Text = text;
        }
        void updateBackColor(Control c, Color backColor)
        {
            if (c != null)
                c.BackColor = backColor;
        }
        #endregion
    }
}
