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
using JetEazy.FormSpace;
using JetEazy.Interface;
using JetEazy.QMath;
using JetEazy.Utils;
using LaserAlignDX.AoiModel.Calib;
using LaserAlignDX.Mvc.Gui;
using System;
using System.Drawing;
using System.Windows.Forms;
using Universal = Traveller106.Universal;

namespace LaserAlignDX.Mvc.Ctrl
{
    public partial class GaMotorsXYInkerCtrl : IxTickable
    {
        public event EventHandler<InkerCoordsEventArgs> OnInkerCoordsUpdated;
        public event EventHandler<InkerCoordsEventArgs> OnQueryInkerCoords;

        #region INKER_SETTINGS
        JxInkerMotorSettings _inkerSettings => JxInkerMotorSettings.Instance;
        #endregion

        #region PRIVATE_MODEL_DATA
        IAxis _motorX;
        IAxis _motorY;
        IAxis _motorSuckerZ;
        #endregion

        #region PRIVATE_CHILD_CTRLS
        GaCommonMotorJogCtrl _jogCtrlX;
        GaCommonMotorJogCtrl _jogCtrlY;
        GaMotorZCtrl _inkerDownCtrl;
        #endregion

        #region GUI_LINKS
        IvMotorsXYInkerUI _ui;
        IvMotorJogView _viewX => _ui?.JogViewX;
        IvMotorJogView _viewY => _ui?.JogViewY;
        GwMotorSimpleGoPanel _inkerUpPanel => _ui.InkerUpPanel;
        GwMotorSimpleGoPanel _inkerDownPanel => _ui.InkerDownPanel;
        #endregion

        #region RUNTIME_DATA
        CarrierEnum _activeCarrierID;
        SuckerRowEnum _activeSuckerID;
        double _suckerSafePosZ;
        double _suckerCurrentPos;
        bool _needsToAutoClose = false;
        #endregion

        public void Attach(IvMotorsXYInkerUI view)
        {
            if (_ui != null) return;
            _ui = view;
            connectEventHandlers();
        }

        #region PRIVATE_INIT_FUNCTIONS
        void connectEventHandlers()
        {
            var cornerUpdateButtons = _ui.InkerCornerUpdateButtons;
            if (cornerUpdateButtons != null)
            {
                int cornerID = 0;
                foreach (Button btnUpdateCorner in cornerUpdateButtons)
                {
                    btnUpdateCorner.Tag = cornerID++;
                    btnUpdateCorner.Click += BtnUpdateCorner_Click;
                }
                System.Diagnostics.Debug.Assert(cornerUpdateButtons.Length == 4);
            }
            var cornerMoveToButtons = _ui.InkerCornerMoveToButtons;
            if (cornerMoveToButtons != null)
            {
                int cornerID = 0;
                foreach (Button btnMoveToCorner in cornerMoveToButtons)
                {
                    btnMoveToCorner.Tag = cornerID++;
                    btnMoveToCorner.Click += BtnMoveToCorner_Click;
                }
                System.Diagnostics.Debug.Assert(cornerMoveToButtons.Length == 4);
            }

            _ui.btnMoveToInkerIdlePos.Click += (s, e) => GoToInkerIdlePosXY();
            _ui.btnSetInkerIdlePos.Click += (s, e) => UpdateInkerIdlePosXY(true);
            _ui.Window.HandleDestroyed += (s, e) => CleanUp();
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
        private void BtnMoveToCorner_Click(object sender, EventArgs e)
        {
            if (_motorX == null || _motorY == null || OnQueryInkerCoords == null)
                return;

            if (sender is Button btn && btn.Tag is int id)
            {
                var ev = new InkerCoordsEventArgs() { CornerID = id, MotorCoord = null };
                OnQueryInkerCoords.Invoke(this, ev);
                var targetPos = ev.MotorCoord;
                if (targetPos != null)
                    BeginMoveTo(targetPos);
            }
        }
        #endregion

        /// <summary>
        /// 設定當下的載台與吸嘴排
        /// </summary>
        public void SetJogTargets(CarrierEnum C, SuckerRowEnum S)
        {
            // 載台與吸嘴排編號
            _activeCarrierID = C;
            _activeSuckerID = S;

            // MODEL (馬達群)
            _motorX = Universal.GetMotorX(S);
            _motorY = Universal.GetMotorY(C);
            _motorSuckerZ = Universal.GetInkerMotor(S);

            // Sucker Safe PosZ (吸嘴安全高度Z)
            var plc = GaBasicMotorUtil.PLCIO;
            _suckerSafePosZ = plc != null ? plc.GetSafeZ(S) : 0.0;

            // XY JOG CONTROL (只允許 attach 一次)
            if (_jogCtrlX == null)
            {
                _jogCtrlX = new GaCommonMotorJogCtrl();
                _jogCtrlY = new GaCommonMotorJogCtrl();
                _jogCtrlX.Attach(_viewX, _motorX);
                _jogCtrlY.Attach(_viewY, _motorY);
                _jogCtrlY.JogDirInverted = true;
            }

            // Inker Down CONTROL (只允許 attach 一次)
            if (_inkerDownCtrl == null)
            {
                _inkerDownCtrl = new GaMotorZCtrl();
                _inkerDownCtrl.Attach(_inkerDownPanel);
                _inkerDownCtrl.SetDataSrc(S);

                // Inker Up Panel (只簡單 顯示 吸嘴安全高度Z)
                _inkerUpPanel.btnSettings.Visible = false;
                _inkerUpPanel.lblCurrentMotorPos.ForeColor = Color.White;
                _inkerUpPanel.lblCurrentMotorPos.Text = $"{_suckerSafePosZ:0.000}";
                _inkerUpPanel.btnMotorGo.Click += (s, e) => RestoreInkerMotorPosZ();
            }

            // Axis Name
            _viewX.lblAxisName.Text = $"X Axis ({GaUtil.GetEnumDescription(S)})";
            _viewY.lblAxisName.Text = $"Y Axis ({GaUtil.GetEnumDescription(C)})";

            // Update GUI
            checkInkerSafety();
            UpdateInkerIdlePosXY(false);
        }
        /// <summary>
        /// 直接移動到指定位置 (X,Y)
        /// </summary>
        public void BeginMoveTo(QVector targetPos, bool silent = false, bool autoClose = false)
        {
            if (targetPos == null)
                return;

            bool go = true;

            if (!silent)
            {
                #region CHECK_IF_TINY
                var deltaX = targetPos.X - _motorX.GetPos();
                var deltaY = targetPos.Y - _motorY.GetPos();
                bool isTiny = GaBasicMotorUtil.IsTinyDelta(deltaX) && GaBasicMotorUtil.IsTinyDelta(deltaY);
                if (isTiny)
                    go = false;
                #endregion

                #region PROMPTS
                if (go)
                {
                    var msg = GaUtil.GetEnumDescription(Prompts.Question_Motor_GoTo_Pos);
                    var motorNameX = _viewX.lblAxisName.Text;
                    var motorNameY = _viewY.lblAxisName.Text;
                    msg += $"\n\r\n\r{motorNameX} To {targetPos.X:0.000}";
                    msg += $"\n\r\n\r{motorNameY} To {targetPos.Y:0.000}";
                    if (VsMessageBox.Question(msg) != DialogResult.OK)
                        go = false;
                }
                #endregion

                // 接下來的個別軸移動, 不需要再彈窗詢問了.
                silent = true;
            }

            if (go)
            {
                _jogCtrlY.BeginMoveTo(targetPos.Y, silent);
                _jogCtrlX.BeginMoveTo(targetPos.X, silent);
            }

            if (autoClose)
            {
                #region 延時設定自動關窗旗標
                new Action(() =>
                {
                    System.Threading.Thread.Sleep(200);
                    _needsToAutoClose = true;
                }).BeginInvoke(null, null);
                #endregion
            }
        }
        /// <summary>
        /// 將 Inker 移動到 待命位置 (X,Y)
        /// </summary>
        public void GoToInkerIdlePosXY()
        {
            var jxPos = _inkerSettings.GetInkerIdlePosXY(_activeCarrierID, _activeSuckerID);
            var targetPos = new QVector2(jxPos.Value.X, jxPos.Value.Y);
            BeginMoveTo(targetPos);
        }
        /// <summary>
        /// 更新 Inker 待命位置 (X,Y)
        /// </summary>
        /// <param name="toRecipe"></param>
        public void UpdateInkerIdlePosXY(bool toRecipe)
        {
            var jxPos = _inkerSettings.GetInkerIdlePosXY(_activeCarrierID, _activeSuckerID);

            if (toRecipe)
            {
                var x = _motorX.GetPos();
                var y = _motorY.GetPos();

                #region PROMPTS
                var msg = GaUtil.GetEnumDescription(Prompts.Question_Update_Motor_Coord_To_Calib);
                msg += $"?\n\r\n\r(X= {x:0.000}, Y= {y:0.000})";
                //msg += $"\n\r\n\rTo {targetName}";
                var ret = VsMessageBox.Question(msg);
                if (ret != DialogResult.OK)
                    return;
                #endregion

                jxPos.Value = new PointF((float)x, (float)y);
                _inkerSettings.Modified = true;
            }

            _ui.lblInkerIdlePos.Text = $"點墨待命位置 = ({jxPos.Value.X:0.000}, {jxPos.Value.Y:0.000})";
        }
        /// <summary>
        /// 回復 Sucker Z軸 到安全位置
        /// </summary>
        public void RestoreInkerMotorPosZ()
        {
            string displayName = $"{_activeSuckerID} Inker 馬達";
            _motorSuckerZ?.PromptMoveTo(_suckerSafePosZ, displayName);
        }
        /// <summary>
        /// 自動保存 已經變更的設定
        /// </summary>
        public void SaveModification()
        {
            if (_inkerSettings.Modified)
                _inkerSettings.Save(null);
        }

        public void Tick()
        {
            checkInkerSafety();
            
            _jogCtrlX?.Tick();
            _jogCtrlY?.Tick();

            updateGuiStatus();
            checkAutoCloseCondition();
        }
        void CleanUp()
        {
            JxInkerMotorSettings.Instance.Dispose();
        }

        #region PRIVATE_FUNCTIONS
        void updateGuiStatus()
        {
            bool isEnabled = _jogCtrlX.IsEnabled();
            bool isReady = _motorX.IsOK && _motorY.IsOK;
            _ui.btnMoveToInkerIdlePos.Enabled = isEnabled && isReady;
            _ui.btnSetInkerIdlePos.Enabled = isEnabled && isReady;
        }
        void updateInkerZsColor()
        {
            bool atSafePos = GaBasicMotorUtil.AreProximityEqual(_suckerCurrentPos, _suckerSafePosZ);
            bool atDownPos = GaBasicMotorUtil.AreProximityEqual(_suckerCurrentPos, _inkerSettings.GetInkerDownZ(_activeSuckerID));
            _ui.InkerDownPanel.lblCurrentMotorPos.BackColor = atDownPos ? Color.Red : Color.Black;
            _ui.InkerUpPanel.lblCurrentMotorPos.BackColor = atSafePos ? Color.Lime : Color.Black;
            _ui.InkerUpPanel.lblCurrentMotorPos.ForeColor = atSafePos ? Color.Black : Color.White;
        }
        void checkInkerSafety()
        {
            // 檢查 Inker 如果在下位, 就禁止移動 XY
            double currentZ = _motorSuckerZ.GetPos();

            if (!GaBasicMotorUtil.AreProximityEqual(_suckerCurrentPos, currentZ))
            {
                _suckerCurrentPos = currentZ;

                if (_suckerCurrentPos > _suckerSafePosZ)
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

                updateInkerZsColor();
            }
        }
        void checkAutoCloseCondition()
        {
            if (_needsToAutoClose)
            {
                if (_motorX.IsOK && _motorY.IsOK)
                {
                    _ui?.Window?.FindForm()?.Close();
                }
            }
        }
        #endregion
    }
}
