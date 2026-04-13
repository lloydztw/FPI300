#region AUTHOR
/*
 * 
 * Copyright (c) 2023 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2023-09-11 LeTian Chang, Revision.
 *      2008-12-01 LeTian Chang, Creation.
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System;


namespace JetEazy.Drivers.Motor
{
    public delegate bool FUNC_MovementSafetyCheck(int iAxisID, double dMoveDst, double dMoveSpeed);

    public interface IDrvMotorAxis : IDisposable
    {
        //> event EventHandler OnMotorStateChanged;
        //> event EventHandler OnMotionCompleted;

        int ID { get; }
        string Name { get; set; }

        //=========================================================================
        // States
        //=========================================================================
        MotorState CurrentState { get; }
        string ErrorMessage { get; }
        int ErrorCode { get; }

        //=========================================================================
        // Counters
        //=========================================================================
        double EncoderPos { get; set; }      // unit: mm or rev
        double CurrentPos { get; }      // unit: mm or rev
        double TargetPos { get; }       // unit: mm or rev
        double CurrentSpeed { get; }    // unit: mm/s or rev/s
        double TargetSpeed { get; }     // unit: mm/s or rev/s

        int EncoderPulses { get; set; } // unit: pulses
        int CurrentPulses { get; }      // unit: pulses
        int TargetPulses { get; }       // unit: pulses
        double CurrentPPS { get; }      // unit: pulses/s
        double TargetPPS { get; }       // unit: pulses/s

        //=========================================================================
        // Senors
        //=========================================================================
        bool IsLimitSensed(MotorSensorID eSensorID);
        bool IsAnyLimitSensed();
        bool IsDriverAlarmed();                              // Fatal Error
        bool IsMotionCompleted(bool bSyncLock = true);       // MotionCompleted
        bool CheckSlowDownSignal(bool bSyncLock = true);

        //=========================================================================
        // General commands
        //=========================================================================
        bool ServoON { get; set; }
        void EmgStop();
        bool Stop();
        bool Home();
        void Reset();

        //=========================================================================
        // Speed-On-The-Fly move
        //=========================================================================
        bool EnableSpeedOnTheFlyMove(bool bEnable, double dSpeedMax = 0);
        bool ChangeSpeed(double dSpeed, double Tacc);

        //=========================================================================
        // Velocity Movement command
        //=========================================================================
        double GetVMoveSpeed(MotorUnitMode eUnit);
        bool SetVMoveSpeed(double dSpeed, MotorUnitMode eUnit);
        bool Move(double dSpeed, MotorUnitMode eUnit);

        //=========================================================================
        // Position Movement command
        //=========================================================================
        double GetPMoveSpeed(MotorUnitMode eUnit);
        bool SetPMoveSpeed(double dSpeed, MotorUnitMode eUnit);
        bool MoveTo(double dPos, MotorUnitMode eUnit);
        bool SyncMoveTo(double dPos1, double dPos2, IDrvMotorAxis drvSlave);

        //=========================================================================
        // Multi-Axes Movement
        //=========================================================================
        MultiAxesContext BeginMultiAxesMove(IDrvMotorAxis[] axes, bool bContinuousMove, MotorUnitMode eUnit);
        bool MultiAxesMove(MultiAxesContext axesContext, double[] arrPos, double Vstart, double Vmax, double Tacc, double Tdec);
        bool IsReadyForNextContinuousMove(MultiAxesContext axesContext);
        void EndMultiAxesMove(MultiAxesContext axesContext);

        //=========================================================================
        // Home Speed
        //=========================================================================
        void GetHomeSpeeds(out double dHighSpeed, out double dLowSpeed, MotorUnitMode eUnit);
        bool SetHomeSpeeds(double dHighSpeed, double dLowSpeed, MotorUnitMode eUnit);

        //=========================================================================
        // ACC and DEC
        //=========================================================================
        /// <summary>
        /// seconds
        /// </summary>
        double AccTime { get; set; }
        /// <summary>
        /// seconds
        /// </summary>
        double DecTime { get; set; }

        //=========================================================================
        // Cached Motion Speeds
        //=========================================================================
        void GetMotionSpeeds(out double dHighSpeed, out double dLowSpeed, MotorUnitMode eUnit);
        bool SetMotionSpeeds(double dHighSpeed, double dLowSpeed, MotorUnitMode eUnit);

        //=========================================================================
        // Limitation Settings
        //=========================================================================
        void GetLimitsPos(out double dMinPos, out double dMaxPos, MotorUnitMode eUnit);
        void GetSoftwareBoundary(out double dMinPos, out double dMaxPos, MotorUnitMode eUnit);
        void SetSoftwareBoundary(double dMinPos, double dMaxPos, MotorUnitMode eUnit);
        void StimulateLimit(MotorSensorID eSensorID, bool bOnOff);
        void SetSafetyCheck(FUNC_MovementSafetyCheck func);
        bool SafetyCheckEnabled { get; set; }

        //=========================================================================
        // Unit Convertion
        //=========================================================================
        string Unit { get; }
        double ToPulses(double dValue);
        double ToPhysical(double dPulses);

        //=========================================================================
        // DoTick
        //=========================================================================
        bool DoTick();

    }
}
