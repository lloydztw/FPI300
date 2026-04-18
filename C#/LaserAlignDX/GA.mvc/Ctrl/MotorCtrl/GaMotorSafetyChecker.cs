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

using JetEazy.Interface;
using JetEazy.Utils;
using LaserAlignDX.AoiModel.Calib;
using System;
using Universal = Traveller106.Universal;

namespace LaserAlignDX.Mvc.Ctrl
{
    public class GaMotorSafetyChecker : IxTickable
    {
        public event EventHandler OnPosChanged;

        #region PRIVATE_KERNEL_DATA
        IAxis[] _motorSuckers;
        #endregion

        #region RUNTIME_DATA
        double[] _suckerSafeZs;
        double[] _suckerCurrentZs;
        #endregion

        public GaMotorSafetyChecker()
        {
            _motorSuckers = new IAxis[]
            {
                Universal.GetInkerMotor(SuckerRowEnum.S1),
                Universal.GetInkerMotor(SuckerRowEnum.S2),
            };

            var plc = GaBasicMotorUtil.PLCIO;
            _suckerSafeZs = new double[]
            {
                plc!=null ? plc.GetSafeZ(SuckerRowEnum.S1) : 0,
                plc!=null ? plc.GetSafeZ(SuckerRowEnum.S2) : 0,
            };

            _suckerCurrentZs = new double[_motorSuckers.Length];
        }

        public void Tick()
        {
            bool isChanged = pollingMotorsPos();
            if (isChanged)
                OnPosChanged?.Invoke(this, null);
        }

        /// <summary>
        /// 是否 全部都在 安全位置
        /// </summary>
        /// <returns></returns>
        public bool IsAllSafe()
        {
            return IsAboveSafePos(SuckerRowEnum.S1) && IsAboveSafePos(SuckerRowEnum.S2);
        }
        /// <summary>
        /// 是否 高於(或等於) Safe Pos
        /// </summary>
        public bool IsAboveSafePos(SuckerRowEnum S)
        {
            int i = (int)(S - SuckerRowEnum.S1);
            if (_suckerCurrentZs[i] <= _suckerSafeZs[i])
                return true;
            else
                return GaBasicMotorUtil.AreProximityEqual(_suckerCurrentZs[i], _suckerSafeZs[i]);
        }
        /// <summary>
        /// 是否 剛好在 Safe Pos
        /// </summary>
        public bool IsAtSafePos(SuckerRowEnum S)
        {
            int i = (int)(S - SuckerRowEnum.S1);
            return GaBasicMotorUtil.AreProximityEqual(_suckerCurrentZs[i], _suckerSafeZs[i]);
        }
        /// <summary>
        /// 是否 剛好在 下壓位置
        /// </summary>
        public bool IsAtDownPos(SuckerRowEnum S)
        {
            int i = (int)(S - SuckerRowEnum.S1);
            return GaBasicMotorUtil.AreProximityEqual(_suckerCurrentZs[i], getDownPosFromRecipe(S));
        }
        
        /// <summary>
        /// 下壓
        /// </summary>
        public void MoveDown(SuckerRowEnum S, bool silent = false)
        {
            int i = (int)(S - SuckerRowEnum.S1);
            var motor = _motorSuckers[i];
            double pos = getDownPosFromRecipe(S);
            string name = $"Z ({GaUtil.GetEnumDescription(S)})";
            GaBasicMotorUtil.PromptMoveTo(motor, pos, name, silent);
        }
        /// <summary>
        /// 回升
        /// </summary>
        public void MoveUp(SuckerRowEnum S, bool silent = false, bool mustDo = false)
        {
            int i = (int)(S - SuckerRowEnum.S1);
            var motor = _motorSuckers[i];
            double pos = _suckerSafeZs[i];
            string name = $"Z ({GaUtil.GetEnumDescription(S)})";
            GaBasicMotorUtil.PromptMoveTo(motor, pos, name, silent, mustDo);
        }

        public double GetSafePos(SuckerRowEnum S)
        {
            int i = (int)(S - SuckerRowEnum.S1);
            return _suckerSafeZs[i];
        }
        public double GetDownZ(SuckerRowEnum S)
        { 
            return getDownPosFromRecipe(S);
        }

        #region PRIVATE_FUNCTIONS
        bool pollingMotorsPos()
        {
            bool isAnyChanged = false;

            int N = _motorSuckers.Length;
            for (int i = 0; i < N; i++)
            {
                var motor = _motorSuckers[i];
                var z = motor.GetPos();

                if (!GaBasicMotorUtil.AreProximityEqual(z, _suckerCurrentZs[i]))
                    isAnyChanged = true;

                _suckerCurrentZs[i] = z;
            }

            return isAnyChanged;
        }
        double getDownPosFromRecipe(SuckerRowEnum S)
        {
            var inkerSettings = JxInkerMotorSettings.Instance;
            var downZ = inkerSettings.GetInkerDownZ(S);
            return downZ;
        }
        #endregion
    }
}
