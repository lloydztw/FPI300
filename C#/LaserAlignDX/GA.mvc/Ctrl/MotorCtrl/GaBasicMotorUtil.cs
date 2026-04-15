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
using JetEazy.FormSpace;
using JetEazy.Interface;
using JetEazy.Utils;
using System;
using System.Windows.Forms;

namespace LaserAlignDX.Mvc.Ctrl
{
    public static class GaBasicMotorUtil
    {
        public static bool IsTinyDelta(double delta)
        {
            return Math.Abs(delta) < Traveller106.Universal.MOTOR_TINY_DELTA;
        }

        public static bool PromptMoveTo(this IAxis motor, double targetPos, string displayName = null, bool silent = false)
        {
            if (motor == null)
                return false;

            var delta = targetPos - motor.GetPos();
            if (IsTinyDelta(delta))
                return false;

            if (!silent)
            {
                if (string.IsNullOrEmpty(displayName))
                    displayName = GetDisplayName(motor);

                var msg = GaUtil.GetEnumDescription(Prompts.Question_Motor_GoTo_Pos);
                msg += $"\n\r\n\r{displayName} To {targetPos:0.000} mm";

                //var ret = MessageBox.Show(msg, "Motor Control", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                //if (ret != DialogResult.Yes)
                //    return false;

                if (VsMessageBox.Question(msg) != DialogResult.OK)
                    return false;
            }

            try
            {
                motor.Go(targetPos, 0);
                return true;
            }
            catch (Exception ex)
            {
                var err = "Motor Error:\n\r" + ex.ToString();
                MessageBox.Show(err, "Motor Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public static string GetDisplayName(this IAxis motor)
        {
            if (motor is PLCMotionClass pMotor)
            {
                // 注意: MOTIONALIAS 使用中文 INI 會有亂碼.
                string name = pMotor.MOTIONALIAS;
                if (true || string.IsNullOrEmpty(name))
                    name = pMotor.MOTIONNAME.ToString();
                return name;
            }
            return motor != null ? motor.ToString() : "";
        }

        public static string GetUnit(this IAxis motor)
        {
            if (motor is PLCMotionClass pMotor)
            {
                return pMotor.MOTIONUNIT;
            }
            return "";
        }
    }
}
