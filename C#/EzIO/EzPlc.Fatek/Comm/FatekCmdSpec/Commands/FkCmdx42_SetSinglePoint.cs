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
using ADDR = EzPlc.Fatek.Comm.IFkAddress;


namespace EzPlc.Fatek.Comm
{
    /// <summary>
    /// <para>CMD= [STX] [plcID:2] "42" [CtrlCode:1] {"M0000"} [ChkSum:2] [ETX]</para>
    /// <para>RET= [STX] [plcID:2] "42" [ErrCode:1] [ChkSum:2] [ETX]</para>
    /// </summary>
    public class FkCmd_SetSinglePoint : FkCmd
    {
        public FkCmd_SetSinglePoint(ADDR addr, FatekIoCtrlCode ctrlCode, int stationID = 1)
        {
            //=====================================================================
            System.Diagnostics.Trace.Assert(addr.Bits == 1);
            //=====================================================================

            StringBuilder sb = new StringBuilder();
            // STX
            sb.Append(C_STX);
            // PLC_ID
            sb.Append(_PID(stationID));
            // COMMAND
            sb.Append("42");

            // CONTEXT= 'CtrlCode'
            sb.Append((char)ctrlCode);

            // ADDRESS= [M]0000 
            sb.Append(addr.ToCommString());

            // CHECKSUM
            appendCheckSum(sb);
            // ETX
            sb.Append(C_ETX);

            // convert
            CmdBytes = Conversion.ToBytes(sb);
        }

        public FkCmd_SetSinglePoint(ADDR addr, bool on, int stationID = 1) :
            this(addr, on ? FatekIoCtrlCode.Set : FatekIoCtrlCode.Reset, stationID)
        {
        }

#if(OPT_RESERVED)
        public FkCmd_SetSinglePoint(string address, bool on, int stationID = 1) :
            this(ADDR.From(address), on ? FatekIoCtrlCode.Set : FatekIoCtrlCode.Reset, stationID)
        {
        }
#endif
    }
}
