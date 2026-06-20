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
using FkResult = EzComm.Utils.ExResult;


namespace EzPlc.Fatek.Comm
{
    /// <summary>
    /// NOT VERIFIED YET!
    /// <para>CMD= [STX] [plcID:2] "44" [N:2] "M0000" [ChkSum:2] [ETX]</para>
    /// <para>RET= [STX] [plcID:2] "44" [Err] {D0 + D1 +... + DN} [ChkSum:2] [ETX]</para>
    /// </summary>
    public class FkCmd_ReadContiSinglePoints : FkCmd
    {
        #region RUNTIME_BUF
        private int m_number;
        private bool[] m_dstBuf;
        private Action<int, bool> m_updateFunc = null;
        #endregion

        private void init(ADDR addr, int number, int stationID = 1)
        {
            //=====================================================================
            System.Diagnostics.Trace.Assert(addr.Bits == 1);
            //=====================================================================
            m_number = Math.Min(255, number);

            var sb = new StringBuilder();
            // STX
            sb.Append(C_STX);
            // PLC_ID
            sb.Append(_PID(stationID));
            // COMMAND
            sb.Append("44");

            // CONTEXT
            // num
            sb.Append(string.Format("{0:X2}", m_number));
            // address
            sb.Append(addr.ToCommString());

            // CHECKSUM
            appendCheckSum(sb);
            // ETX
            sb.Append(C_ETX);

            // Convert
            CmdBytes = Conversion.ToBytes(sb);
        }
        public FkCmd_ReadContiSinglePoints(ADDR addr, bool[] dstBuf, int stationID = 1)
        {
#if(false)
            //=====================================================================
            System.Diagnostics.Trace.Assert(addr.Bits == 1);
            //=====================================================================
            m_number = Math.Min(255, dstBuf.Length);
            m_dstBuf = dstBuf;

            var sb = new StringBuilder();
            // STX
            sb.Append(C_STX);
            // PLC_ID
            sb.Append(_PID(stationID));
            // COMMAND
            sb.Append("44");

            // CONTEXT
            // num
            sb.Append(string.Format("{0:X2}", m_number));
            // address
            sb.Append(addr.ToFatekString());

            // CHECKSUM
            appendCheckSum(sb);
            // ETX
            sb.Append(C_ETX);

            // Convert
            CmdBytes = Conversion.ToBytes(sb);
#endif
            m_dstBuf = dstBuf;
            m_updateFunc = null;
            init(addr, dstBuf.Length, stationID);
        }
        public FkCmd_ReadContiSinglePoints(ADDR addr, int number, Action<int, bool> updateFunc, int stationID = 1)
        {
            m_updateFunc = updateFunc;
            init(addr, number, stationID);
        }
        public override FkResult HandleResponse(string responseStr)
        {
            var result = parseResponse(CmdBytes, responseStr);

            if (result.Error == 0)
            {
                var context = result.Context;
                int contextLen = context.Length;
                int number = m_number;

                if (number > contextLen - 1)
                {
                    result = _MAKE_ERR_RESULT(FatekCommandErr.Err_ReadContiSinglePoints_WrongDataLength, responseStr, CmdBytes, null);
                    number = contextLen - 1;
                }

                //<1> Using Callback
                if (m_updateFunc != null)
                {
                    for (int i = 0; i < number; i++)
                    {
                        bool one = (context[i + 1] == '1');
                        m_updateFunc(i, one);
                    }
                }
                else
                {
                    for (int i = 0; i < number; i++)
                        m_dstBuf[i] = (context[i + 1] == '1');
                }
            }

            return result;
        }
    }
}
