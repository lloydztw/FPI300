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
using System.Collections.Generic;
using VsCommon.ControlSpace.MachineSpace;
using Universal = Traveller106.Universal;

namespace LaserAlignDX.Mvc.Ctrl
{
    public class GaMotorSafetyChecker : IxTickable
    {
        public event EventHandler OnPosChanged;

        const int N_MOTORS_PER_ROW = 4;

        #region RUNTIME_DATA
        Dictionary<SuckerRowEnum, double[]> _suckerCurrentZs = new Dictionary<SuckerRowEnum, double[]>();
        Dictionary<SuckerRowEnum, double[]> _suckerSafeZs = new Dictionary<SuckerRowEnum, double[]>();
        #endregion

        public GaMotorSafetyChecker()
        {
            var plcIO = ((MainFPIX3MachineClass)Traveller106.Universal.MACHINECollection?.MACHINE)?.PLCIO;
            foreach (SuckerRowEnum S in Enum.GetValues(typeof(SuckerRowEnum)))
            {
                _suckerCurrentZs.Add(S, new double[N_MOTORS_PER_ROW]);
                _suckerSafeZs.Add(S, new double[N_MOTORS_PER_ROW]);
                for (int idx = 0; idx < N_MOTORS_PER_ROW; idx++)
                    _suckerSafeZs[S][idx] = plcIO.GetSafeZ(S, idx);
            }
        }

        public void Tick()
        {
            bool isChanged = pollingMotorsPos();
            if (isChanged)
                OnPosChanged?.Invoke(this, null);
        }

        /// <summary>
        /// 是否 全部吸嘴 都在 安全位置
        /// </summary>
        /// <returns></returns>
        public bool IsAllSafe()
        {
            return IsAboveSafePos(SuckerRowEnum.S1) && IsAboveSafePos(SuckerRowEnum.S2);
        }
        /// <summary>
        /// 是否 全部吸嘴 高於(或等於) Safe Pos
        /// </summary>
        public bool IsAboveSafePos(SuckerRowEnum S)
        {
            bool allSafe = true;
            for (int i = 0; i < N_MOTORS_PER_ROW; i++)
            {
                var currentZ = _suckerCurrentZs[S][i];
                var safeZ = _suckerSafeZs[S][i];
                bool isSafe = (currentZ <= safeZ) || GaBasicMotorUtil.AreProximityEqual(currentZ, safeZ);
                if (!isSafe)
                    allSafe = false;
            }
            return !allSafe;
        }
        /// <summary>
        /// 是否 點墨吸嘴 剛好在 Safe Pos
        /// </summary>
        public bool IsAtSafePos(SuckerRowEnum S)
        {
            int idx = 0;
            return GaBasicMotorUtil.AreProximityEqual(_suckerCurrentZs[S][idx], _suckerSafeZs[S][idx]);

        }
        /// <summary>
        /// 是否 點墨吸嘴 剛好在 下壓位置
        /// </summary>
        public bool IsAtDownPos(SuckerRowEnum S)
        {
            int idx = 0;
            return GaBasicMotorUtil.AreProximityEqual(_suckerCurrentZs[S][idx], getDownPosFromRecipe(S));
        }

        /// <summary>
        /// 下壓 (點墨吸嘴)
        /// </summary>
        public void MoveDown(SuckerRowEnum S, bool silent = false)
        {
            int idx = 0;
            var motor = Universal.GetSuckerMotor(S, idx);
            double pos = getDownPosFromRecipe(S);
            string name = $"Z ({GaUtil.GetEnumDescription(S)})";
            GaBasicMotorUtil.PromptMoveTo(motor, pos, name, silent);
        }
        /// <summary>
        /// 回升 (所有吸嘴)
        /// </summary>
        public void MoveUp(SuckerRowEnum S, bool silent = false, bool mustDo = false)
        {
            for (int idx = 0; idx < N_MOTORS_PER_ROW; idx++)
            {
                var motor = Universal.GetSuckerMotor(S, idx);
                double pos = _suckerSafeZs[S][idx];
                string name = $"Z ({GaUtil.GetEnumDescription(S)})";
                GaBasicMotorUtil.PromptMoveTo(motor, pos, name, silent, mustDo);
            }
        }

        /// <summary>
        /// 取得 點墨吸嘴 的安全位置
        /// </summary>
        public double GetSafePos(SuckerRowEnum S)
        {
            int idx = 0;
            return _suckerSafeZs[S][idx];
        }
        /// <summary>
        /// 取得 點墨吸嘴 的下壓位置
        /// </summary>
        public double GetDownZ(SuckerRowEnum S)
        { 
            return getDownPosFromRecipe(S);
        }

        #region PRIVATE_FUNCTIONS
        bool pollingMotorsPos()
        {
            bool isAnyChanged = false;
            foreach (SuckerRowEnum S in Enum.GetValues(typeof(SuckerRowEnum)))
            {
                for (int i = 0; i < N_MOTORS_PER_ROW; i++)
                {
                    var motor = Universal.GetSuckerMotor(S, i);
                    var currentZ = motor.GetPos();

                    if (!GaBasicMotorUtil.AreProximityEqual(currentZ, _suckerCurrentZs[S][i]))
                        isAnyChanged = true;

                    _suckerCurrentZs[S][i] = currentZ; 

                }
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
