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

using LaserAlignDX.Mvc.Ctrl;
using System.Windows.Forms;

namespace LaserAlignDX.Mvc.Gui
{
    public partial class FormMotorXY : Form
    {
        #region PRIVATE_DATA
        GaMotorXYInkerCtrl _motorsCtrl;
        #endregion

        public FormMotorXY()
        {
            InitializeComponent();
            if (!DesignMode)
                InitGui();
        }

        public void SetJogTargets(CarrierEnum C,  SuckerRowEnum S)
        {
            _motorsCtrl.SetJogTargets(C, S);
        }

        void InitGui()
        {
            FormClosed += (s, e) => CleanUp();
            btnOK.Click += (s, e) => DoConfirm();
            btnCancel.Click += (s, e) => DoCancel();

            var viewX = gvPaneMotorJogXY1.GetJogViewX();
            var viewY = gvPaneMotorJogXY1.GetJogViewY();
            var inkerUpPanel = gwMotorSimpleGoPanel1;
            var inkerDownPanel = gwMotorSimpleGoPanel2;

            _motorsCtrl = new GaMotorXYInkerCtrl();
            _motorsCtrl.Attach(viewX, viewY, inkerUpPanel, inkerDownPanel);

            timer1.Tick += (s, e) => DoTick();
            Load += (s, e) => timer1.Start();
        }

        #region PRIVATE_FUNCTIONS
        void CleanUp()
        {
            timer1.Stop();
            _motorsCtrl?.RestoreInkerMotorPos();
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
