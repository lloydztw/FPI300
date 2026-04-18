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
using System.Drawing;
using System.Windows.Forms;

namespace LaserAlignDX.Mvc.Gui
{
    public partial class FormMotors_CarierSuckerXY : Form, IvMotorsXYInkerUI
    {
        public event EventHandler<InkerCoordsEventArgs> OnInkerCoordsUpdated;
        public event EventHandler<InkerCoordsEventArgs> OnQueryInkerCoords;

        #region PRIVATE_DATA
        GaMotorsXYInkerCtrl _motorsCtrl;
        Button[] _cornerMoveButtons;
        Button[] _cornerSaveButtons;
        #endregion

        public FormMotors_CarierSuckerXY()
        {
            InitializeComponent();

            _cornerMoveButtons = new Button[] { btnMoveLT, btnMoveRT, btnMoveRB, btnMoveLB, };
            _cornerSaveButtons = new Button[] { btnSaveLT, btnSaveRT, btnSaveRB, btnSaveLB, };

            if (!DesignMode)
                Init();

            gvPaneMotorJogXY1.SizeChanged += (s, e) => autoLayout();
            autoLayout();
        }

        #region GUI_LINKS
        Control IvMotorsXYInkerUI.Window => this;
        IvMotorJogView IvMotorsXYInkerUI.JogViewX => gvPaneMotorJogXY1.GetJogViewX();
        IvMotorJogView IvMotorsXYInkerUI.JogViewY => gvPaneMotorJogXY1.GetJogViewY();
        GwMotorSimpleGoPanel IvMotorsXYInkerUI.InkerUpPanel => gwMotorSimpleGoPanel1;
        GwMotorSimpleGoPanel IvMotorsXYInkerUI.InkerDownPanel => gwMotorSimpleGoPanel2;
        Control IvMotorsXYInkerUI.lblInkerIdlePos => lblTitle;
        Button IvMotorsXYInkerUI.btnMoveToInkerIdlePos => button1;
        Button IvMotorsXYInkerUI.btnSetInkerIdlePos => button2;
        Button[] IvMotorsXYInkerUI.InkerCornerUpdateButtons => _cornerSaveButtons;
        Button[] IvMotorsXYInkerUI.InkerCornerMoveToButtons => _cornerMoveButtons;
        #endregion

        void Init()
        {
            FormClosed += (s, e) => CleanUp();
            btnOK.Click += (s, e) => DoConfirm();
            btnCancel.Click += (s, e) => DoCancel();

            _motorsCtrl = new GaMotorsXYInkerCtrl();
            _motorsCtrl.Attach(this);
            _motorsCtrl.OnInkerCoordsUpdated += (s, e) => OnInkerCoordsUpdated?.Invoke(s, e);
            _motorsCtrl.OnQueryInkerCoords += (s, e) => OnQueryInkerCoords?.Invoke(s, e);

            timer1.Tick += (s, e) => DoTick();
            Load += (s, e) => timer1.Start();

            InitToolTips();
        }
        void InitToolTips()
        {
            var tags = new[] { "左上", "右上", "右下", "左下" };
            for (int i = 0; i < 4; i++)
            {
                toolTip1.SetToolTip(_cornerMoveButtons[i], $"Move To {tags[i]}");
                toolTip1.SetToolTip(_cornerSaveButtons[i], $"Update To {tags[i]}");
            }
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
                    _motorsCtrl.BeginMoveTo(directTargetPos, silent: true, autoClose: true);
                };
            }
        }

        #region PRIVATE_FUNCTIONS
        void CleanUp()
        {
            timer1.Stop();
            _motorsCtrl?.RestoreInkerMotorPosZ();
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

        #region AUTO_LAYOUT
        void autoLayout()
        {
            if (WindowState == FormWindowState.Minimized)
                return;

            // 1. 鎖定佈局，防止在設定過程中頻繁重繪
            this.SuspendLayout();
            try
            {
                var jogPanel = gvPaneMotorJogXY1.gwMotorJogSticks1;
                var rect = jogPanel.ClientRectangle;
                var size = _cornerMoveButtons[0].Size;

                var xx = new[] { rect.Left, rect.Right, rect.Right, rect.Left };
                var yy = new[] { rect.Top, rect.Top, rect.Bottom, rect.Bottom };
                var xs = new[] { 0, -1, -1, 0 };
                var ys = new[] { 0, 0, -1, -1 };

                for (int i = 0; i < 4; i++)
                {
                    var btn = _cornerMoveButtons[i];
                    if (btn.Parent != jogPanel)
                        btn.Parent = jogPanel;

                    btn.Size = size;
                    int x = xx[i] + xs[i] * btn.Width;
                    int y = yy[i] + ys[i] * btn.Height;
                    btn.Location = new Point(x, y);
                    btn.BringToFront();
                }

                rect.Inflate(-size.Width, -size.Height);
                xx = new[] { rect.Left, rect.Right, rect.Right, rect.Left };
                yy = new[] { rect.Top, rect.Top, rect.Bottom, rect.Bottom };
                for (int i = 0; i < 4; i++)
                {
                    var btn = _cornerSaveButtons[i];
                    if (btn.Parent != jogPanel)
                        btn.Parent = jogPanel;

                    btn.Size = size;
                    int x = xx[i] + xs[i] * btn.Width;
                    int y = yy[i] + ys[i] * btn.Height;
                    btn.Location = new Point(x, y);
                    btn.BringToFront();
                }
            }
            catch
            {

            }
            finally
            {
                // 2. 恢復佈局並立即強制重新計算（傳入 true 代表執行佈局）
                this.ResumeLayout(true);
            }
        }
        #endregion
    }
}
