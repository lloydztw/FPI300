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
    /// <para>CMD: [STX] [plcID:2] "4E" {echoMsg} [ETX]</para>
    /// <para>RET: [STX] [plcID:2] "4E" {echoMsg} [ETX]</para>
    /// </summary>
    public class FkCmd_Echo : FkCmd
    {
        public FkCmd_Echo(string msg, int stationID = 1)
        {
            StringBuilder sb = new StringBuilder();

            // STX
            sb.Append(C_STX);
            // PLC_ID
            sb.Append(_PID(stationID));
            // COMMAND
            sb.Append("4E");
            // CONTEXT
            sb.Append(msg);
            // CHECKSUM
            appendCheckSum(sb);
            // ETX
            sb.Append(C_ETX);

            CmdBytes = Conversion.ToBytes(sb);
        }
    }
}
