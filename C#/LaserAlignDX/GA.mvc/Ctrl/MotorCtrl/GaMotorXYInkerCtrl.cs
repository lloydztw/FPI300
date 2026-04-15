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
using JetEazy.Interface;
using JetEazy.QMath;
using JetEazy.Utils;
using LaserAlignDX.Mvc.Gui;
using System;
using System.Windows.Forms;
using Traveller106;
using Universal = Traveller106.Universal;

namespace LaserAlignDX.Mvc.Ctrl
{
    public partial class GaMotorXYInkerCtrl : IxTickable
    {
        public event EventHandler<InkerCoordsEventArgs> OnInkerCoordsUpdated;

        #region PRIVATE_MODEL_DATA
        IAxis _motorX;
        IAxis _motorY;
        IAxis _inkerMotorZ;
        double _backupInkerMotorPos;
        SuckerRowEnum _activeInkerID;
        #endregion

        #region PRIVATE_CHILD_CTRLS
        GaCommonMotorJogCtrl _jogCtrlX;
        GaCommonMotorJogCtrl _jogCtrlY;
        GaMotorZCtrl _inkerDownCtrl;
        #endregion

        #region GUI_LINKS
        IvMotorXYInkerUI _ui;
        IvMotorJogView _viewX => _ui?.JogViewX;
        IvMotorJogView _viewY => _ui?.JogViewY;
        GwMotorSimpleGoPanel _inkerUpPanel => _ui.InkerUpPanel;
        GwMotorSimpleGoPanel _inkerDownPanel => _ui.InkerDownPanel;
        #endregion

        #region RUNTIME_DATA
        double _inkerPos;
        bool _isInkerPosModified = false;
        #endregion

        public void Attach(IvMotorXYInkerUI view)
        {
            if (_ui != null) return;
            _ui = view;
            connectEventHandlers(_ui.InkerCornerUpdateButtons);
        }

        #region PRIVATE_INIT_FUNCTIONS
        void connectEventHandlers(Button[] cornerUpdateButtons)
        {
            if (cornerUpdateButtons == null) return;
            System.Diagnostics.Debug.Assert(cornerUpdateButtons.Length == 4);

            int cornerID = 0;
            foreach (Button btnUpdateCorner in cornerUpdateButtons)
            {
                btnUpdateCorner.Tag = cornerID++;
                btnUpdateCorner.Click += BtnUpdateCorner_Click;
            }
        }
        #endregion

        #region EVENT_HANDLERS
        private void BtnUpdateCorner_Click(object sender, EventArgs e)
        {
            if (_motorX == null || _motorY == null || OnInkerCoordsUpdated == null)
                return;

            if (sender is Button btn && btn.Tag is int id)
            {
                var x = _motorX.GetPos();
                var y = _motorY.GetPos();
                var motorCoord = new QVector2(x, y);
                var ev = new InkerCoordsEventArgs() { CornerID = id, MotorCoord = motorCoord };
                OnInkerCoordsUpdated(this, ev);
            }
        }
        #endregion

        public void SetJogTargets(CarrierEnum C, SuckerRowEnum S)
        {
            // MODEL
            _motorX = Universal.GetMotorX(S);
            _motorY = Universal.GetMotorY(C);
            _inkerMotorZ = Universal.GetInkerMotor(S);
            _backupInkerMotorPos = _inkerMotorZ.GetPos();
            _activeInkerID = S;

            // XY JOG CONTROL (只允許 attach 一次)
            if (_jogCtrlX == null)
            {
                _jogCtrlX = new GaCommonMotorJogCtrl();
                _jogCtrlY = new GaCommonMotorJogCtrl();
                _jogCtrlX.Attach(_viewX, _motorX);
                _jogCtrlY.Attach(_viewY, _motorY);
            }

            // Inker Down CONTROL (只允許 attach 一次)
            if (_inkerDownCtrl == null)
            {
                _inkerDownCtrl = new GaMotorZCtrl();
                _inkerDownCtrl.Attach(_inkerDownPanel);
                _inkerDownCtrl.SetDataSrc(S);
                _inkerDownCtrl.OnPosDataSrcModified += (s, e) => _isInkerPosModified = true;

                // Inker Up Panel (只簡單保存當下的 motor pos)
                _inkerUpPanel.btnSettings.Visible = false;
                _inkerUpPanel.lblCurrentMotorPos.Text = $"{_backupInkerMotorPos:0.000}";
                _inkerUpPanel.btnMotorGo.Click += (s, e) => RestoreInkerMotorPos();
            }

            // Axis Name
            _viewX.lblAxisName.Text = $"X Axis ({GaUtil.GetEnumDescription(S)})";
            _viewY.lblAxisName.Text = $"Y Axis ({GaUtil.GetEnumDescription(C)})";
        }

        public void RestoreInkerMotorPos()
        {
            string displayName = $"{_activeInkerID} Inker 馬達";
            _inkerMotorZ?.PromptMoveTo(_backupInkerMotorPos, displayName);
        }

        public void SaveModification()
        {
            if (_isInkerPosModified)
            {
                _isInkerPosModified = false;
                INI.Instance.Save();
            }
        }

        public void Tick()
        {
            checkInkerSafety();
            _jogCtrlX?.Tick();
            _jogCtrlY?.Tick();
        }

        void checkInkerSafety()
        {
            // 檢查 Inker 如果在下位, 就禁止移動 XY

            var inkerPos = _inkerMotorZ.GetPos();

            if (!GaBasicMotorUtil.IsTinyDelta(_inkerPos - inkerPos))
            {
                _inkerPos = inkerPos;

                if (_inkerPos > _backupInkerMotorPos)
                {
                    var reason = GaUtil.GetEnumDescription(Prompts.Warning_MotorXY_Disabled_By_Inker_Down);
                    _jogCtrlX.SetEnable(false, reason);
                    _jogCtrlY.SetEnable(false, reason);
                }
                else
                {
                    _jogCtrlX.SetEnable(true);
                    _jogCtrlY.SetEnable(true);
                }
            }
        }
    }
}
