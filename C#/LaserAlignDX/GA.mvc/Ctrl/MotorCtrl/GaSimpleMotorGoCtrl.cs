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
using JetEazy.ControlSpace.MotionSpace;
using JetEazy.FormSpace;
using JetEazy.Interface;
using JetEazy.Utils;
using LaserAlignDX.Mvc.Gui;
using System;
using System.Windows.Forms;

namespace LaserAlignDX.Mvc.Ctrl
{
    public class GaSimpleMotorGoCtrl
    {
        public class MotorPosHolder
        {
            public Func<double> Get;
            public Action<double> Set;
            public double Value
            {
                get => Get(); 
                set => Set(value);
            }
        }
        public class SimpleMotoriew
        {
            public Control lblMotorName;
            public Control lblMotorPos;
            public Button btnMotorGo;
            public Button btnSettings;
        }

        #region PRIVATE_KERNEL_DATA
        SimpleMotoriew _ui;
        PLCMotionClass _motor;
        MotorPosHolder _posHolder;
        #endregion

        public void Attach(IAxis motor, SimpleMotoriew view, MotorPosHolder posHolder)
        {
            _ui = view;
            _motor = motor as PLCMotionClass;
            _posHolder = posHolder;

            _ui.btnMotorGo.Click += BtnMotorGo_Click;
            _ui.btnSettings.Click += BtnSettings_Click;

            updateMotorPosToGui(_posHolder);
        }
        public void Attach(IAxis motor, GwMotorSimpleGoPanel panel, MotorPosHolder posHolder)
        {
            var view = new SimpleMotoriew
            {
                lblMotorName = panel.lblAxisName,
                lblMotorPos = panel.lblCurrentMotorPos,
                btnMotorGo = panel.btnMotorGo,
                btnSettings = panel.btnSettings
            };
            Attach(motor, view, posHolder);
        }

        #region EVENT_HANDLER
        private void BtnMotorGo_Click(object sender, EventArgs e)
        {
            if (_posHolder != null)
                MoveMotorTo(_posHolder.Value);
        }
        private void BtnSettings_Click(object sender, EventArgs e)
        {
            OpenMotorWindowZ();
        }
        #endregion

        public void UpdatePosHolderToGui()
        {
            updateMotorPosToGui(_posHolder);
        }
        public void MoveMotorTo(double targetPos, bool silent = false)
        {
            var motor = _motor;
            if (motor == null) return;

            var delta = targetPos - motor.GetPos();
            if (IsTinyDelta(delta))
                return;

            if (!silent)
            {
                var msg = GaUtil.GetEnumDescription(Prompts.Question_Motor_GoTo_Pos);
                msg += $"\n\r\n\r{GetMotorDisplayName(_motor)} To {targetPos:0.000} mm";

                bool ok = VsMessageBox.Question(msg) == DialogResult.OK;
                if (!ok)
                    return;
            }

            try
            {
                _motor?.Go((float)targetPos);
            }
            catch (Exception ex)
            {
                var err = "馬達異常:\n\r" + ex.ToString();
                MessageBox.Show(err, "Motor Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void OpenMotorWindowZ()
        {
            var motor = _motor;
            if (motor == null) return;

            // 備份當下 相機馬達 的位置
            double lastMotorPos = motor.GetPos();

            var frmOwner = _ui.lblMotorPos.FindForm();
            using (var dlg = new FormMotorZ())
            {
                dlg.Attach(_motor);
                dlg.Text = _ui.lblMotorName.Text;

                dlg.StartPosition = FormStartPosition.CenterParent;
                if (dlg.ShowDialog(frmOwner) == DialogResult.OK)
                {
                    syncMotorPos(_motor, _posHolder);
                    updateMotorPosToGui(_posHolder);
                }
                else
                {
                    // 返回 相機馬達 之前的位置
                    MoveMotorTo(lastMotorPos);
                }
            }
        }

        #region DATA_EXCHANGE_FUNCTIONS
        public static bool IsTinyDelta(double delta)
        {
            return Math.Abs(delta) < Traveller106.Universal.MOTOR_TINY_DELTA;
        }
        public static string GetMotorDisplayName(IAxis motor)
        {
            if (!(motor is PLCMotionClass pMotor))
                return "";
            string name = pMotor.MOTIONALIAS;
            if (string.IsNullOrEmpty(name))
                name = pMotor.MOTIONNAME.ToString();
            return name;
        }
        void syncMotorPos(IAxis src, MotorPosHolder dst)
        {
            if (src == null || dst == null)
                return;

            dst.Value = src.GetPos();
        }
        void updateMotorPosToGui(MotorPosHolder src)
        {
            var text = src != null ? $"{src.Get():0.000}" : "";
            updateText(_ui.lblMotorPos, text);
        }
        void updateText(Control c, string text)
        {
            if (c != null && text != null)
                c.Text = text;
        }
        #endregion
    }
}
