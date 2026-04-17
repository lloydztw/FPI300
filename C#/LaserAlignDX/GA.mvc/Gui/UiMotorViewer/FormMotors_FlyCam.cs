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
using LaserAlignDX.Mvc.Ctrl;
using System;
using System.Windows.Forms;

namespace LaserAlignDX.Mvc.Gui
{
    public partial class FormMotors_FlyCam : Form, IvMotorsFlyCamUI
    {
        public event EventHandler<InkerCoordsEventArgs> OnInkerCoordsUpdated;

        #region PRIVATE_DATA
        GaMotorsFlyCamCtrl _motorsCtrl;
        #endregion

        public FormMotors_FlyCam()
        {
            InitializeComponent();

            if (!DesignMode)
                InitGui();
        }

        #region GUI_PROPERTIES
        public string AxisName1
        {
            get => gvPaneMotorJogXY1.AxisName1;
            set => gvPaneMotorJogXY1.AxisName1 = value;
        }
        public string AxisName2
        {
            get => gvPaneMotorJogXY1.AxisName2;
            set => gvPaneMotorJogXY1.AxisName2 = value;
        }
        #endregion

        #region GUI_LINKS
        Control IvMotorsFlyCamUI.Window => this;
        IvMotorJogView IvMotorsFlyCamUI.JogViewX => gvPaneMotorJogXY1.GetJogViewX();
        IvMotorJogView IvMotorsFlyCamUI.JogViewY => gvPaneMotorJogXY1.GetJogViewY();
        GwMotorSimpleGoPanel IvMotorsFlyCamUI.FocusPanelZ => gwMotorSimpleGoPanel1;
        Button IvMotorsFlyCamUI.btnGoTriggerPosX => button1;
        Button IvMotorsFlyCamUI.btnGoCameraPosY => button2;
        Button IvMotorsFlyCamUI.btnVacuum => button3;
        #endregion

        void InitGui()
        {
            FormClosed += (s, e) => CleanUp();
            btnOK.Click += (s, e) => DoConfirm();
            btnCancel.Click += (s, e) => DoCancel();

            _motorsCtrl = new GaMotorsFlyCamCtrl();
            _motorsCtrl.Attach(this);

            timer1.Tick += (s, e) => DoTick();
            Load += (s, e) => timer1.Start();
        }

        #region PRIVATE_FUNCTIONS
        void CleanUp()
        {
            timer1.Stop();
            _motorsCtrl?.RestoreFocusMotorPos();
            _motorsCtrl = null;
        }
        void DoTick()
        {
            _motorsCtrl?.Tick();
        }
        void DoConfirm()
        {
            _motorsCtrl?.SaveModification();
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
