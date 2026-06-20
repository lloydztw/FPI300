#region AUTHOR
/*
 * EzPlc.Fatek
 * Copyright (C) 2026
 * 2026-04-05 revised by LeTian Chang
 * 2013-07-11 created by LeTian Chang
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion


using System.ComponentModel;

namespace EzIO.Mem
{
    public enum ModbusCateEnum : int
    {
        [Description("Coils (1-bit) 讀寫")]
        COIL = 00001,

        [Description("Discrete Inputs (1-bit) 唯讀")]
        DISCRETE_INPUT = 10001,

        [Description("Input Registers (16-bit) 唯讀")]
        INPUT_REGISTER = 30001,

        [Description("Holding Registers (16-bit) 讀寫")]
        HOLDING_REGISTER = 40001,
    }
}
