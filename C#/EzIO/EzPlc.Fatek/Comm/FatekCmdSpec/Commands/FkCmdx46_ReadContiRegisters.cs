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
    /// <para>CMD16: [STX] [ID:2] "46" [N:2] {"R12345"} [ChkSum:2] [ETX]</para>
    /// <para>CMD16: [STX] [ID:2] "46" [N:2] {"WM1234"} [ChkSum:2] [ETX]</para>
    /// <para>CMD32: [STX] [ID:2] "46" [N:2] {"DR12345"} [ChkSum:2] [ETX]</para>
    /// <para>CMD32: [STX] [ID:2] "46" [N:2] {"DWM1234"} [ChkSum:2] [ETX]</para>
    /// </summary>
    public class FkCmd_ReadContiRegisters : FkCmd
    {
        #region RUNTIME_DATA
        private ADDR m_addr;
        private int m_number;
        private uint[] m_dstBuf;
        private Action<int, uint> m_updateFunc;
        #endregion

        public FkCmd_ReadContiRegisters(ADDR addr, uint[] dstBuf, int stationID = 1)
        {
            System.Diagnostics.Trace.Assert(dstBuf != null && dstBuf.Length > 0);

            int number = dstBuf.Length;

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

            m_addr = addr;
            m_number = number;
            m_dstBuf = dstBuf;

            var sb = new StringBuilder();

            // STX
            sb.Append(C_STX);
            // PLC_ID
            sb.Append(_PID(stationID));

            // COMMAND
            sb.Append("46");

            // CONTEXT
            sb.Append(number.ToString("X2"));
            sb.Append(addr.ToCommString());

            // CHECKSUM
            appendCheckSum(sb);

            // ETX
            sb.Append(C_ETX);

            // convert
            CmdBytes = Conversion.ToBytes(sb);
        }

        public FkCmd_ReadContiRegisters(ADDR addr, int number, Action<int, uint> updateFunc, int stationID = 1)
            : this(addr, new uint[number], stationID)
        {
            m_updateFunc = updateFunc;
        }

        /// <summary>
        /// RET16= STX + <ID:2> + "46" + <err:1> + <D:4 x number> + <ChkSum:2> + ETX
        /// RET32= STX + <ID:2> + "46" + <err:1> + <D:8 x number> + <ChkSum:2> + ETX
        /// </summary>
        public override FkResult HandleResponse(string responseStr)
        {
            var result = parseResponse(CmdBytes, responseStr);

            if (result.Error != 0)
                return result;

            var context = result.Context;
            var addr = m_addr;
            var number = m_number;
            var dstBuf = m_dstBuf;
            var byteSize = (addr.Bits / 8);
            int stepSize = byteSize * 2;
            bool isAllOk = true;

            int pos = 1;
            for (int i = 0; i < number; i++, pos += stepSize)
            {
                try
                {
                    string hex = context.Substring(pos, stepSize);

                    dstBuf[i] = Convert.ToUInt32(hex, 16);
                }
                catch (Exception ex)
                {
                    dstBuf[i] = uint.MaxValue;

                    var tag = string.Format("[{0}/{1}] {2}", i, number, addr.ToCommString());

                    result = _MAKE_ERR_RESULT(FatekCommandErr.Err_ReadContiRegs_WrongData, responseStr, CmdBytes, tag, ex);

                    isAllOk = false;
                }
            }

            // System.Diagnostics.Trace.WriteLineIf(m_debugDumpHex, "");

            // 仍然 Update 好的數據.
            if (m_updateFunc != null)
            {
                for (int i = 0; i < number; i++)
                {
                    if (dstBuf[i] != uint.MaxValue)
                        m_updateFunc(i, dstBuf[i]);
                }
            }

            if (isAllOk)
            {
                System.Diagnostics.Debug.Assert(result.Error == 0);
            }

            return result;
        }
    }
}
