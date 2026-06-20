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

using System.Text;
using EzComm.Utils;


namespace EzPlc.Fatek.Comm
{
    /// <summary>
    /// <para>CMD: [STX] [PlcID:2] "41" ["1"] [ChkSum:2] [ETX]</para>
    /// <para>RET: [STX] [PlcID:2] "41" ["0"] [ChkSum:2] [ETX]</para>
    /// </summary>
    public class FkCmd_RunStopPlc : FkCmd
    {
        public FkCmd_RunStopPlc(bool start, int stationID = 1)
        {
            StringBuilder sb = new StringBuilder();

            // STX
            sb.Append(C_STX);
            // PLC_ID
            sb.Append(_PID(stationID));
            // COMMAND
            sb.Append("41");
            // CONTEXT
            sb.Append(start ? "1" : "0");
            // CHECKSUM
            appendCheckSum(sb);
            // ETX
            sb.Append(C_ETX);

            CmdBytes = Conversion.ToBytes(sb);
        }
    }
}
