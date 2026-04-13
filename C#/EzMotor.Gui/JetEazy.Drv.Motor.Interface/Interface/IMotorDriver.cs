/****************************************************************************
 *                                                                          
 * Copyright (c) 2009 Jet Eazy Corp. All rights reserved.        
 *                                                                          
 ***************************************************************************/

/****************************************************************************
 *
 * VERSION
 *		$Revision:$
 *
 * HISTORY
 *      $Id:$    
 *	        20081201 LeTian Chang : Creation         
 *
 * DESCRIPTION
 *      
 *
 ***************************************************************************/

using System;
//using System.Drawing.Imaging;
//using System.Drawing;
//using System.Collections.Generic;

namespace JetEazy.Drivers.MotorFBs32
{
    public delegate void OnMotorMoving();
    public delegate void OnMotorPositioned();
    public delegate void OnMotorLimitSensed(AxisID eAxisID,MotorSensorID eSensorID);

    public enum AxisID : int
    {
        AxisX = 0,
        AxisY = 1,
    }
    public enum MotorState : int
    {
        Idle = 0,
        AutoMoving,
        ManualMoving,
    }
    public enum MotorSensorID : int
    {
        Top = 0,
        Base = 1,
        Home = 2,
        Min = Top,
        Max = Base,
    }

    public interface IMotorDriver : IDisposable
    {
        void Reset();

        MotorState GetState();

        void DoTick();

        // SENSORS
        #region SENSORS
            bool IsLimitSensed(AxisID eAxisID, MotorSensorID eSensorID);
            bool IsAutoMoveCompleted(AxisID eAxisID);
        #endregion

        // GEOMETRY
        #region GEOMETRY
            void GetAxisLimit(AxisID eAxisID, ref int iMin, ref int iMax);
            int GetCacheX();
            int GetCacheY();
            int GetCurX();
            int GetCurY();
            int GetTargetX();
            int GetTargetY();
        #endregion

        // Speed Configuration
        #region SPEED_CONTROL
            void GetSpeeds( AxisID eAxisID,
                            ref int iManualMoveSpeed,
                            ref int iAutoMoveSpeed,
                            ref int iFastHomeSpeed,
                            ref int iSlowHomeSpeed );
            void SetSpeeds( AxisID eAxisID,
                            int iManualMoveSpeed,
                            int iAutoMoveSpeed,
                            int iFastHomeSpeed,
                            int iSlowHomeSpeed
                          );
        #endregion

        // Move Control
        #region MOVE_CONTROL
            void Stop();
            void Stop(AxisID eAxis);
            // AutoMode
            void MoveTo(int x, int y, OnMotorPositioned callback);
            void Offset(int xDelta, int yDelta, OnMotorPositioned callback);
            void Home(AxisID eAxisID);
            // ManualMode
            void Forward(AxisID eAxisID);
            void Backward(AxisID eAxisID);
        #endregion

        //void SetOnMovingCallback(OnMotorMoving callback); 
        //void SetOnLimitSensedCallback(OnMotorLimitSensed callback);
    }
}
