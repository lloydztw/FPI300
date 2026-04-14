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

using JetEazy.Drivers.Motor;
using System.Drawing;
using System.Windows.Forms;


namespace AX.Gui
{
    public partial class GwMotorAxisStatus : UserControl, IvAxisStatusView
    {
        #region PRIVATE_MEMBERS
        MotorState _lastState = MotorState.Unknown;
        Control _activeStateLabel = null;
        int _blinkCount = 0;
        #endregion

        public GwMotorAxisStatus()
        {
            InitializeComponent();
            HandleCreated += (s, e) => autoLayout();
            SizeChanged += (s, e) => autoLayout();
        }
        public string AxisName
        {
            get
            {
                return lblAxisName.Text;
            }
            set
            {
                lblAxisName.Text = value;
            }
        }

        public void UpdateAxisStatus(IDrvMotorAxis axis)
        {
            updatePosSpeedAndSignals(axis);
            updateMotorState(axis);
        }
        public void UpdateAxisName(IDrvMotorAxis axis)
        {
            AxisName = axis.Name + " Status";
        }

        #region PRIVATE_FUNCTIONS
        void updatePosSpeedAndSignals(IDrvMotorAxis axis)
        {
            if (axis == null)
                return;

            // Position
            lblPosition.Text = axis.CurrentPos.ToString("0.000");
            lblPosPS.Text = axis.CurrentPulses.ToString();

            // Speed
            lblSpeed.Text = axis.CurrentSpeed.ToString("0.00");
            lblSpeedPPS.Text = System.Math.Abs(axis.CurrentPPS) < 10
                ? axis.CurrentPPS.ToString("0.000")
                : axis.CurrentPPS.ToString("0");

            //// External Encoder Position
            //if (lblEncoder.Visible)
            //{
            //    lblEncoder.Text = axis.EncoderPos.ToString("0.000");
            //    lblEncoderPS.Text = axis.EncoderPulses.ToString();
            //}

            // Limit Sensors
            lblSignalLimitN.BackColor = axis.IsLimitSensed(MotorSensorID.LimitMin) ? Color.Pink : SystemColors.ButtonFace;
            lblSignalLimitP.BackColor = axis.IsLimitSensed(MotorSensorID.LimitMax) ? Color.Pink : SystemColors.ButtonFace;
            lblSignalORG.BackColor = axis.IsLimitSensed(MotorSensorID.Home) ? Color.Gold : SystemColors.ButtonFace;
            lblSignalINP.BackColor = axis.IsMotionCompleted(false) ? Color.Lime : SystemColors.ButtonFace;
            lblSignalSlow.BackColor = axis.CheckSlowDownSignal(false) ? Color.Gold : SystemColors.ButtonFace;
        }
        void updateMotorState(IDrvMotorAxis axis)
        {
            // 目前應用狀況:
            // (1) MotorState.Emergency 內部都強制不使用, 統一交由外部 IoCtrl Emg Button 處理
            // (2) MotorState.Error 目前沒有用到!

            if (axis == null)
                return;

            bool isChanged = (_lastState != axis.CurrentState);
            _lastState = axis.CurrentState;

            // Status
            switch (axis.CurrentState)
            {
                case MotorState.Ready:
                    if (isChanged)
                        updateStateColor(lblStateReady, Color.Lime);
                    break;

                case MotorState.P_Moving:
                case MotorState.V_Moving:
                    if (isChanged)
                        updateStateColor(lblStateMoving, Color.Lime);
                    break;

                case MotorState.LimitedStop:
                    break;

                case MotorState.EmergencyStop:
                    if (isChanged)
                        lblStateError.Text = "EMG";
                    updateStateColor(lblStateError, Color.Red, true);
                    break;
                case MotorState.DriverAlarm:
                    if (isChanged)
                        lblStateError.Text = "Alarm";
                    updateStateColor(lblStateError, Color.Red, true);
                    break;
                case MotorState.Error:
                    if (isChanged)
                        lblStateError.Text = "Error";
                    updateStateColor(lblStateError, Color.Red, true);
                    break;

                default:
                    updateStateColor(null, Color.LightGray);
                    break;
            }
        }
        void updateStateColor(Control to, Color color, bool blink = false)
        {
            if (_activeStateLabel != null && _activeStateLabel != to)
                _activeStateLabel.BackColor = Color.LightGray;

            _activeStateLabel = to;

            if (to == null)
                return;

            if (!blink)
            {
                to.BackColor = color;
            }
            else
            {
                if (0 == _blinkCount)
                {
                    to.BackColor = to.BackColor != color
                                    ? color
                                    : Color.LightGray;
                }
                _blinkCount = (++_blinkCount) % 10;
            }
        }
        void updateStateColor_000(Control to, Color color, bool blink = false)
        {
            if (_activeStateLabel != null)
                _activeStateLabel.BackColor = Color.LightGray;

            _activeStateLabel = to;

            if (_activeStateLabel != null)
                _activeStateLabel.BackColor = color;
        }
        #endregion

        void autoLayout()
        {
            int pad = Padding.Right;
            var ccSize = ClientSize;
            foreach (var c in new[] { tableLayoutPanel0, tableLayoutPanel2 })
            {
                c.Left = ccSize.Width - pad - c.Width;
            }
            tableLayoutPanel1.Width = ccSize.Width - pad * 2;
        }
    }
}
