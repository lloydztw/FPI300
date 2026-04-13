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
    public partial class FormMotorXY : Form
    {
        #region PRIVATE_DATA
        GaXYInkerMotorsJogCtrl _motorsCtrl;
        #endregion

        public FormMotorXY()
        {
            InitializeComponent();
            FormClosed += (s, e) => CleanUp();
            btnOK.Click += (s, e) => DoConfirm();
            btnCancel.Click += (s, e) => DoCancel();
        }
        public void Attach(IAxis motorX, IAxis motorY, IAxis focusMotorZ, IAxis inkerMotorZ)
        {
            _motorsCtrl = new GaXYInkerMotorsJogCtrl();
            var viewX = gvPaneMotorJogXY1.GetJogViewX();
            var viewY = gvPaneMotorJogXY1.GetJogViewY();
            _motorsCtrl.AttachMotorXY(motorX, motorY, viewX, viewY);

            var focusPanel = gwMotorSimpleGoPanel1;
            var inkerPanel = gwMotorSimpleGoPanel2;
            _motorsCtrl.AttachFocusMotorZ(focusMotorZ, focusPanel);
            _motorsCtrl.AttachInkerMotorZ(inkerMotorZ, inkerPanel);

            timer1.Tick += (s, e) => DoTick();
            this.Load += (s, e) =>
            {
                timer1.Enabled = true;
            };
        }

        #region PRIVATE_FUNCTIONS
        void CleanUp()
        {
            timer1.Stop();
            _motorsCtrl = null;
        }
        void DoTick()
        {
            _motorsCtrl?.Tick();
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
