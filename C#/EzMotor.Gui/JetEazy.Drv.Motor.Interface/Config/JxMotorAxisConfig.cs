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
    [Serializable]
    public class JxMotorAxisConfig
    {
        public int AxisID = 0;                          //>>> Software Logic ID
        public short ChannelID = -1;                    //>>> Hardware Channel ID
        public string Unit = "mm";                      //>>> physical unit name

        public double Pitch = 2.0;                      //>>> physical units/rev (mm/rev)
        public double PulsePerRev = 20000.0;            //>>> pulses/rev
        public double PulsePerUnit
        {
            get { return (double)PulsePerRev / Pitch; }
        }

        public MotorPulseFormat PulseFormat = MotorPulseFormat.CW_CCW_FallingEdge;
        public int PulseWidth = 250;                    //>>> (ns)
        public int HomeMode = 0;

        public MotorLogic AlarmLogic = MotorLogic.ActiveHigh;
        public MotorLogic InpLogic = MotorLogic.ActiveLow;
        public MotorLogic LimitLogic = MotorLogic.ActiveLow;
        public MotorLogic OrgLogic = MotorLogic.ActiveLow;

        public bool AlarmEnabled
        {
            get
            {
                return AlarmLogic != MotorLogic.Disabled;
            }
#if(OPT_RESERVED)
            set
            {
                if (!value)
                {
                    N_ALARM_LOGIC = MotorLogic.Disabled;
                }
                else if (N_ALARM_LOGIC == MotorLogic.Disabled)
                {
                    N_ALARM_LOGIC = MotorLogic.ActiveHight;
                }
                else
                {
                }
            }
#endif
        }
        public bool InpEnabled
        {
            get
            {
                return (InpLogic != MotorLogic.Disabled);
            }
#if(OPT_RESERVED)
            set
            {
                if (!value)
                {
                    N_INP_LOGIC = MotorLogic.Disabled;
                }
                else if (N_INP_LOGIC == MotorLogic.Disabled)
                {
                    N_INP_LOGIC = MotorLogic.ActiveLow;
                }
                else
                {
                }
            }
#endif
        }

        public double ACC_secs = 0.1;                   // (ms)
        public double DEC_secs = 0.1;                   // (ms)
        public double RPM_max = 3000.0;                 // (rpm)

        public double LIMIT_SESNOR_POS_MIN = -50.0;     // hardware sensor limit (mm)
        public double LIMIT_SESNOR_POS_MAX = 500.0;     // hardware sensor limit (mm) 
        public double RATIO_ENCODER_TO_CMD = 1.0;       // Specific to servo-motor
        public object O_EXT = null;                     // extention data (reserved)

        public JxMotorAxisConfig Clone()
        {
            var dst = (JxMotorAxisConfig)MemberwiseClone();
            return dst;
        }
    }
}
