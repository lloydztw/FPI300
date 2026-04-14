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
using JetEazy.Utils;
using LaserAlignDX.Mvc.Ctrl;
using System;
using System.Windows.Forms;

namespace LaserAlignDX.Mvc.Gui
{
    public partial class FormMotorXY : Form, IvMotorXYInkerUI
    {
        public event EventHandler<InkerCoordsEventArgs> OnInkerCoordsUpdated;

        #region PRIVATE_DATA
        GaMotorXYInkerCtrl _motorsCtrl;
        #endregion

        public FormMotorXY()
        {
            InitializeComponent();

            if (!DesignMode)
                InitGui();
        }

        #region GUI_LINKS
        Control IvMotorXYInkerUI.Window => this;
        IvMotorJogView IvMotorXYInkerUI.JogViewX => gvPaneMotorJogXY1.GetJogViewX();
        IvMotorJogView IvMotorXYInkerUI.JogViewY => gvPaneMotorJogXY1.GetJogViewY();
        GwMotorSimpleGoPanel IvMotorXYInkerUI.InkerUpPanel => gwMotorSimpleGoPanel1;
        GwMotorSimpleGoPanel IvMotorXYInkerUI.InkerDownPanel => gwMotorSimpleGoPanel2;
        Button[] IvMotorXYInkerUI.InkerCornerUpdateButtons => new[]
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

            _motorsCtrl = new GaMotorXYInkerCtrl();
            _motorsCtrl.Attach(this);
            _motorsCtrl.OnInkerCoordsUpdated += (s, e) => OnInkerCoordsUpdated?.Invoke(s, e);

            timer1.Tick += (s, e) => DoTick();
            Load += (s, e) => timer1.Start();
        }

        public void SetJogTargets(CarrierEnum C,  SuckerRowEnum S)
        {
            var yName = GaUtil.GetEnumDescription(C);
            var xName = GaUtil.GetEnumDescription(S);
            Text = $"軸控 (X={xName}, Y={yName})";

            _motorsCtrl.SetJogTargets(C, S);
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
