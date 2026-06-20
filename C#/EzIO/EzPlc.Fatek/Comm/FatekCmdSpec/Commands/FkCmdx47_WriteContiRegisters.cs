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

using System;
using System.Text;
using EzComm.Utils;
using ADDR = EzPlc.Fatek.Comm.IFkAddress;


namespace EzPlc.Fatek.Comm
{
    /// <summary>
    /// <para>CMD16: [STX] [ID:2] "47" [N:2] {"R12345"} {D:4 x N} [ChkSum:2] [ETX]</para>
    /// <para>CMD16: [STX] [ID:2] "47" [N:2] {"WM1234"} {D:4 x N} [ChkSum:2] [ETX]</para>
    /// <para>CMD32: [STX] [ID:2] "47" [N:2] {"DR12345"} {D:8 x N} [ChkSum:2] [ETX]</para>
    /// <para>CMD32: [STX] [ID:2] "47" [N:2] {"DWM1234"} {D:8 x N} [ChkSum:2] [ETX]</para>
    /// </summary>
    public class FkCmd_WriteContiRegisters : FkCmd
    {
        public FkCmd_WriteContiRegisters(ADDR addr, uint[] arrData, int stationID = 1)
        {
            int number = arrData.Length;

            // Number= 0x01 ~ 0x40 or 0x01 ~ 0x20
            if (addr.Bits == 16)
            {
                number = Math.Min(0x40, number);    // 16-bit (2-bytes)
            }
            else if (addr.Bits == 32)
            {
                number = Math.Min(0x20, number);    // 32-bit (4-bytes)
            }
            else
            {
                number = 0;
                throw new Exception("Wrong Address Type: " + addr);
            }
            
            StringBuilder sb = new StringBuilder();
            // STX
            sb.Append(C_STX);
            // PLC_ID
            sb.Append(_PID(stationID));
            // COMMAND
            sb.Append("47");

            // CONTEXT
            // number
            sb.Append(number.ToString("X2"));
            // address
            sb.Append(addr.ToCommString());
            // data
            if (addr.Bits == 16)
            {
                for (int i = 0; i < number; i++)
                    sb.Append(string.Format("{0:X4}", (ushort)arrData[i]));
            }
            else
            {
                for (int i = 0; i < number; i++)
                    sb.Append(string.Format("{0:X8}", (uint)arrData[i]));
            }

            // CHECKSUM
            appendCheckSum(sb);
            // ETX
            sb.Append(C_ETX);

            // convert
            CmdBytes = Conversion.ToBytes(sb);
        }
        public FkCmd_WriteContiRegisters(ADDR addr, uint data, int stationID = 1)
            : this(addr, new uint[] { data }, stationID)
        {
        }
    }
}
