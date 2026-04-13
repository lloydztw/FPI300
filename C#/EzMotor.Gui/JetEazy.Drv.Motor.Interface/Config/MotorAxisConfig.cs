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
    public class MotorAxisConfig
    {
        public int AxisID = 0;                          //>>> Software Logic ID
        public short ChannelID = -1;                    //>>> Hardware Channel ID
        public string UNIT = "mm";                      //>>> physical unit name

        public double D_PITCH = 2.0;                    //>>> physical units/rev (mm/rev)
        public double D_PULSES_PER_REV = 20000.0;       //>>> pulses/rev
        public double D_PULSES_PER_UNIT
        {
            get { return (double)D_PULSES_PER_REV / D_PITCH; }
        }

        public MotorPulseFormat N_PULSE_FORMAT = MotorPulseFormat.CW_CCW_FallingEdge;
        public int N_PULSE_WIDTH = 250;                 //>>> (ns)

        public int HomeMode = 0;                                    
        public MotorLogic N_ALARM_LOGIC = MotorLogic.ActiveHigh;
        public MotorLogic N_INP_LOGIC = MotorLogic.ActiveLow;
        public MotorLogic B_LMT_LOGIC = MotorLogic.ActiveLow;
        public MotorLogic B_ORG_LOGIC = MotorLogic.ActiveLow;

        public bool B_ALARM_ENABLED
        {
            get
            {
                return N_ALARM_LOGIC != MotorLogic.Disabled;
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
        public bool B_INP_ENABLED
        {
            get
            {
                return (N_INP_LOGIC != MotorLogic.Disabled);
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

        public double D_LIMIT_SESNOR_POS_MIN = -50.0;   // hardware sensor limit (mm)
        public double D_LIMIT_SESNOR_POS_MAX = 500.0;   // hardware sensor limit (mm) 
        public double D_RATIO_ENCODER_TO_CMD = 1.0;     // Specific to servo-motor

        public double D_TIME_ACC = 0.1;                 // (seconds)
        public double D_TIME_DEC = 0.1;                 // (seconds)
        public double D_MAX_RPM = 3000.0;               // (rpm)

        public object O_EXT = null;                     // extention data (reserved)


        public MotorAxisConfig Clone()
        {
            MotorAxisConfig dst = (MotorAxisConfig)MemberwiseClone();
            return dst;
        }
        public void Load(string strIniFileName, string strAppName)
        {
            string str = null;
            int value = 0;

            JetEazy.Win32.Win32Ini.Load(ref str, strIniFileName, strAppName, "Unit");
            if (!string.IsNullOrEmpty(str)) UNIT = str.Trim();

            JetEazy.Win32.Win32Ini.Load(ref str, strIniFileName, strAppName, "Channel");
            if (!string.IsNullOrEmpty(str) && int.TryParse(str, out value)) ChannelID = (short)value;
            if (ChannelID < 0)
                ChannelID = (short)AxisID;

            JetEazy.Win32.Win32Ini.Load(ref str, strIniFileName, strAppName, "PulseFormat");
            if (!string.IsNullOrEmpty(str) && int.TryParse(str, out value))
                N_PULSE_FORMAT = (MotorPulseFormat)value;

            JetEazy.Win32.Win32Ini.Load(ref str, strIniFileName, strAppName, "PulseWidth");
            if (!string.IsNullOrEmpty(str)) int.TryParse(str, out N_PULSE_WIDTH);
            
            JetEazy.Win32.Win32Ini.Load(ref str, strIniFileName, strAppName, "Pulses");
            if (!string.IsNullOrEmpty(str)) double.TryParse(str, out D_PULSES_PER_REV);

            JetEazy.Win32.Win32Ini.Load(ref str, strIniFileName, strAppName, "Pitch");
            if (!string.IsNullOrEmpty(str)) double.TryParse(str, out D_PITCH);

            JetEazy.Win32.Win32Ini.Load(ref str, strIniFileName, strAppName, "OrgLogic");
            if (!string.IsNullOrEmpty(str) && int.TryParse(str, out value))
                B_ORG_LOGIC = (MotorLogic)value;

            JetEazy.Win32.Win32Ini.Load(ref str, strIniFileName, strAppName, "HomeMode");
            if (!string.IsNullOrEmpty(str) && int.TryParse(str, out value))
                HomeMode = value;

            JetEazy.Win32.Win32Ini.Load(ref str, strIniFileName, strAppName, "LmtLogic");
            if (!string.IsNullOrEmpty(str) && int.TryParse(str, out value))
                B_LMT_LOGIC = (MotorLogic)value;

            JetEazy.Win32.Win32Ini.Load(ref str, strIniFileName, strAppName, "AlarmLogic");
            if (!string.IsNullOrEmpty(str) && int.TryParse(str, out value))
                N_ALARM_LOGIC = (MotorLogic)value;

            JetEazy.Win32.Win32Ini.Load(ref str, strIniFileName, strAppName, "InpLogic");
            if (!string.IsNullOrEmpty(str) && int.TryParse(str, out value))
                N_INP_LOGIC = (MotorLogic)value;

            JetEazy.Win32.Win32Ini.Load(ref str, strIniFileName, strAppName, "TAcc");
            if (!string.IsNullOrEmpty(str)) double.TryParse(str, out D_TIME_ACC);

            JetEazy.Win32.Win32Ini.Load(ref str, strIniFileName, strAppName, "TDec");
            if (!string.IsNullOrEmpty(str)) double.TryParse(str, out D_TIME_DEC);

            JetEazy.Win32.Win32Ini.Load(ref str, strIniFileName, strAppName, "MaxRPM");
            if (!string.IsNullOrEmpty(str)) double.TryParse(str, out D_MAX_RPM);
        }
    }
}
