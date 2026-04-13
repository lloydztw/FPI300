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


namespace JetEazy.Drivers.Motor
{
    public enum MotorPulseFormat : int
    {
        //---------------------------------------------------------------------------------
        //>>> N_PULSE_FORMAT
        //---------------------------------------------------------------------------------
        //  0 OUT/DIR OUT Falling edge, DIR+ is high level
        //  1 OUT/DIR OUT Rising edge, DIR+ is high level
        //  2 OUT/DIR OUT Falling edge, DIR+ is low level
        //  3 OUT/DIR OUT Rising edge, DIR+ is low level
        //  4 CW/CCW Falling edge
        //  5 CW/CCW Rising edge
        //  6 4XA/B Falling edge
        //  7 4XA/B Rising edge
        //---------------------------------------------------------------------------------
        PulseDir_FallingEdge_Dir_High = 0,
        PulseDir_RisingEdge_Dir_High = 1,
        PulseDir_FallingEdge_Dir_Low = 2,
        PulseDir_RisingEdge_Dir_Low = 3,
        CW_CCW_FallingEdge = 4,
        CW_CCW_RisingEdge = 5,
        ABX4_FallingEdge = 6,
        ABX4_RisingEdge = 7,
    }
}
