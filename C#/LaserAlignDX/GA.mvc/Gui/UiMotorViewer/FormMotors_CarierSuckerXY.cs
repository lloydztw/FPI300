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
using JetEazy.QMath;
using JetEazy.Utils;
using LaserAlignDX.Mvc.Ctrl;
using System;
using System.Windows.Forms;

namespace LaserAlignDX.Mvc.Gui
{
    public partial class FormMotors_CarierSuckerXY : Form, IvMotorsXYInkerUI
    {
        public event EventHandler<InkerCoordsEventArgs> OnInkerCoordsUpdated;

        #region PRIVATE_DATA
        GaMotorsXYInkerCtrl _motorsCtrl;
        #endregion

        public FormMotors_CarierSuckerXY()
        {
            InitializeComponent();

            if (!DesignMode)
                InitGui();
        }

        #region GUI_LINKS
        Control IvMotorsXYInkerUI.Window => this;
        IvMotorJogView IvMotorsXYInkerUI.JogViewX => gvPaneMotorJogXY1.GetJogViewX();
        IvMotorJogView IvMotorsXYInkerUI.JogViewY => gvPaneMotorJogXY1.GetJogViewY();
        GwMotorSimpleGoPanel IvMotorsXYInkerUI.InkerUpPanel => gwMotorSimpleGoPanel1;
        GwMotorSimpleGoPanel IvMotorsXYInkerUI.InkerDownPanel => gwMotorSimpleGoPanel2;
        Button[] IvMotorsXYInkerUI.InkerCornerUpdateButtons => new[]
        {
            btnSaveLT,  // 左上
            btnSaveRT,  // 右上
            btnSaveRB,  // 右下
            btnSaveLB,  // 左下
        };
        #endregion

        void InitGui()
        {
            FormClosed += (s, e) => CleanUp();
            btnOK.Click += (s, e) => DoConfirm();
            btnCancel.Click += (s, e) => DoCancel();

            _motorsCtrl = new GaMotorsXYInkerCtrl();
            _motorsCtrl.Attach(this);
            _motorsCtrl.OnInkerCoordsUpdated += (s, e) => OnInkerCoordsUpdated?.Invoke(s, e);

            timer1.Tick += (s, e) => DoTick();
            Load += (s, e) => timer1.Start();
        }

        public void SetJogTargets(CarrierEnum C,  SuckerRowEnum S, QVector directTargetPos = null)
        {
            var yName = GaUtil.GetEnumDescription(C);
            var xName = GaUtil.GetEnumDescription(S);
            Text = $"軸控 (X={xName}, Y={yName})";

            _motorsCtrl.SetJogTargets(C, S);

            if (directTargetPos != null)
            {
                Load += (s, e) =>
                {
                    _motorsCtrl.BeginMoveTo(directTargetPos);
                };
            }
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
