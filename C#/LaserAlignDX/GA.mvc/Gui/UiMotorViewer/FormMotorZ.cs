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
        PLCMotionClass _motor;
        GaCommonMotorJogCtrl _jogCtrl;
        #endregion

        public FormMotorZ()
        {
            InitializeComponent();
        }
        
        public void Attach(IAxis motor)
        {
            var view = gvPaneMotorJogZ1.GetJogView();

            _motor = motor as PLCMotionClass;
            _jogCtrl = new GaCommonMotorJogCtrl();
            _jogCtrl.Attach(view, _motor);

            btnOK.Click += (s, e) => DoConfirm();
            btnCancel.Click += (s, e) => DoCancel();
            timer1.Tick += (s, e) => _jogCtrl?.Tick();

            this.Load += (s, e) => timer1.Start();
        }

        #region PRIVATE_FUNCTIONS
        void CleanUp()
        {
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
