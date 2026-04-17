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
        bool _enabled = true;
        string _forbiddenReason;
        #endregion

        public void Attach(IvMotorJogView view, IAxis motor)
        {
            _ui = view;
            _motor = motor as PLCMotionClass;

            var frm = view.Window.FindForm();
            if (!frm.IsHandleCreated)
                frm.Load += (s, e) => Init();
            else
                Init();

            frm.FormClosed += (s, e) => CleanUp();
        }

        public bool JogDirInverted
        {
            get;
            set;
        }

        public void SetEnable(bool enable, string forbiddenReason = null)
        {
            _enabled = enable;
            _forbiddenReason = forbiddenReason;
        }

        public void BeginMoveTo(double targetPos, bool silent = true)
        {
            _ui.Window.BeginInvoke(new Action(() =>
            {
                numGoPosition.Value = (decimal)targetPos;
                MoveMotorTo((float)targetPos, silent);
            }));
        }

        public void Tick()
        {
            PollingMotorStatus();
            UpdateMotorStatus();
        }

        #region PRIVATE_FUNCTIONS
        void Init()
        {
            UpdateMotorNameUnit();

            btnForward.MouseDown += (s, e) => JogForward();
            btnForward.MouseUp += (s, e) => StopMotor();
            btnBackward.MouseDown += (s, e) => JogBackward();
            btnBackward.MouseUp += (s, e) => StopMotor();

            btnHome.Click += (s, e) => Home();
            btnAdd.Click += (s, e) => MoveMotorDelta((float)Math.Abs(numGoPosition.Value));
            btnSub.Click += (s, e) => MoveMotorDelta(-(float)Math.Abs(numGoPosition.Value));
            btnGo.Click += (s, e) => MoveMotorTo((float)numGoPosition.Value);

            lblError.DoubleClick += (s, e) => ResetMotor();
        }
        bool CheckEnabled()
        {
            if (!_enabled)
            {
                string msg = !string.IsNullOrEmpty(_forbiddenReason) ? _forbiddenReason : "Disabled!";
                VsMessageBox.Warning(msg);
            }
            return _enabled;
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
            if (!CheckEnabled()) return;
            if (JogDirInverted)
                _motor?.Backward();
            else
                _motor?.Forward();
            _isMoving = true;
        }
        void JogBackward()
        {
            if (!CheckEnabled()) return;
            if (JogDirInverted)
                _motor?.Forward();
            else
                _motor?.Backward();
            _isMoving = true;
        }
        void Home()
        {
            if (!CheckEnabled()) return;

            string msg = GaUtil.GetEnumDescription(Prompts.Question_Motor_Home);
            string displayName = _ui.lblAxisName.Text;
            msg += $"\n\r\n\r{displayName} To HOME";

            //var ret = MessageBox.Show(msg, "Motor Control", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            //if (ret != DialogResult.Yes)
            //    return;

            if (VsMessageBox.Question(msg) != DialogResult.OK)
                return;

            _motor.Home();
            _isMoving = true;
        }
        void MoveMotorTo(float pos, bool silent = false)
        {
            if (!CheckEnabled()) return;

            if (!silent)
            {
                string displayName = _ui.lblAxisName.Text;
                if (GaBasicMotorUtil.PromptMoveTo(_motor, pos, displayName, silent))
                    _isMoving = true;
            }
            else
            {
                _motor?.Go(pos, 0);
                _isMoving = true;
            }
        }
        void MoveMotorDelta(float delta)
        {
            if (!CheckEnabled()) return;

            // 防呆保護: 5 mm 以上 之相對位移, 會顯示提視窗.
            bool silent = Math.Abs(delta) <= 5.0;

            double pos = _motor.GetPos() + delta;
            MoveMotorTo((float)pos, silent);
        }
        void PollingMotorStatus()
        {
            // RESERVED
        }
        void CleanUp()
        {
            // RESERVED
        }
        #endregion

        void UpdateMotorStatus()
        {
            var motor = _motor;
            if (motor == null)
                return;

            // 讀取馬達設備變量 (耗CPU時間)
            bool isError = motor.IsError;
            bool isOK = motor.IsOK;
            bool isINP = motor.IsOnSite;
            bool hasBeenHome = motor.IsHome;     // 這裡的 IsHome 貌似指 曾經成功 回HOME

            bool isAtLowerLimit = motor.IsReachLowerBound;
            bool isAtUpperLimit = motor.IsReachUpperBound;
            bool isAtOrg = motor.IsReachHomeBound;

            var currentPos = motor.PositionNow;
            var currentSpeed = motor.GetSpeed(SpeedTypeEnum.GO);

            if (isINP)
                _isMoving = false;

            var isMoving = _isMoving && !isError;
            var isReady = isOK && !isError;

            updateBackColor(_ui.lblStatusMoving, isMoving ? Color.Lime : _normalColor);
            updateBackColor(_ui.lblStatusReady, isReady ? Color.Lime : _normalColor);
            updateBackColor(_ui.lblStatusError, isError ? Color.Pink : _normalColor);
            updateBackColor(btnHome, hasBeenHome ? _normalColor : Color.Pink);

            setEnable(btnHome, isReady && !isMoving);
            setEnable(btnGo, isReady && !isMoving);
            setEnable(btnAdd, isReady && !isMoving);
            setEnable(btnSub, isReady && !isMoving);
            setEnable(btnForward, !isError);
            setEnable(btnBackward, !isError);

            updateBackColor(_ui.lblSignalLimitP, isAtUpperLimit ? Color.Pink : _normalColor);
            updateBackColor(_ui.lblSignalLimitN, isAtLowerLimit ? Color.Pink : _normalColor);
            updateBackColor(_ui.lblSignalOrg, isAtOrg ? Color.Gold : _normalColor);
            updateBackColor(_ui.lblSignalINP, isINP ? Color.Lime : _normalColor);
            
            updateText(lblPositionNow, $"{currentPos:0.000}");
            updateText(lblCurrentSpeed, $"{currentSpeed:0.000}");
            updateForeColor(_ui.lblCurrentSpeed, isMoving ? Color.Lime : Color.White);
        }

        #region PRIVATE_GUI_UPDATE_FUNCTIONS
        Color _normalColor = SystemColors.Control;
        void UpdateMotorNameUnit()
        {
            // 注意: MOTIONALIAS 使用中文 INI 會有亂碼.
            //string name0 = GaBasicMotorUtil.GetDisplayName(_motor);
            //string name1 = _ui?.Window?.FindForm()?.Text;
            //string name = !string.IsNullOrEmpty(name1) ? name1 : name0;
            string unit = GaBasicMotorUtil.GetUnit(_motor);
            //updateText(_ui?.lblAxisName, name);
            updateText(_ui?.lblUnit, unit);
        }
        void setEnable(Control c, bool enabled)
        {
            if (c != null)
                c.Enabled = enabled;
        }
        void updateText(Control c, string text)
        {
            if (c != null && text != null)
                c.Text = text;
        }
        void updateBackColor(Control c, Color color)
        {
            if (c != null)
                c.BackColor = color;
        }
        void updateForeColor(Control c, Color color)
        {
            if (c != null)
                c.ForeColor = color;
        }
        #endregion
    }
}
