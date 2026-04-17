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

using JetEazy.ControlSpace.MotionSpace;
using JetEazy.Interface;
using LaserAlignDX.Mvc.Ctrl;
using System.Windows.Forms;

namespace LaserAlignDX.Mvc.Gui
{
    public partial class FormMotorZ : Form
    {
        #region PRIVATE_DATA
        GaCommonMotorJogCtrl _jogCtrl;
        IAxis _motor;
        #endregion

        public FormMotorZ()
        {
            InitializeComponent();
        }
        public void Attach(IAxis motor)
        {
            var view = gvPaneMotorJogZ1.GetJogView();
            view.lblAxisName.Text = Text;

            _jogCtrl = new GaCommonMotorJogCtrl();
            _jogCtrl.Attach(view, motor);
            _motor = motor;

            btnServo.Click += (s, e) => ToggleServo();
            btnOK.Click += (s, e) => DoConfirm();
            btnCancel.Click += (s, e) => DoCancel();
            timer1.Tick += (s, e) => _jogCtrl?.Tick();

            this.Load += (s, e) => timer1.Start();
        }
        public bool ServoOnVisible
        {
            get => btnServo.Visible;
            set => btnServo.Visible = value;
        }

        #region PRIVATE_FUNCTIONS
        void ToggleServo()
        {
        }
        void SetServo(bool on)
        {
            //if(_motor is PLCMotionClass plcMotor)
            //{
            //}
        }
        void CleanUp()
        {
            SetServo(true);
            timer1.Enabled = false;
            _jogCtrl = null;
        }
        void DoConfirm()
        {
            DialogResult = DialogResult.OK;
            CleanUp();
            Close();
        }
        void DoCancel()
        {
            DialogResult = DialogResult.Cancel;
            CleanUp();
            Close();
        }
        #endregion
    }
}
