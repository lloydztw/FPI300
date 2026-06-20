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
    /// <para>CMD= [STX] [plcID:2] "48" [N:2] { Addr1, Addr2, ..., AddrN } [ChkSum:2] [ETX]</para>
    /// <para>RET= [STX] [plcID:2] "48" [Err] { D1/D4/D8 x N } [ChkSum:2] [ETX]</para>
    /// </summary>
    public class FkCmd_ReadMixRegisters : FkCmd
    {
        #region PRIVATE_RUNTIME_DATA
        private int m_number;
        private ADDR[] m_addrGrp;
        private Action<object, int, uint> m_updateFunc;
        #endregion

        public FkCmd_ReadMixRegisters(ADDR[] addressGrp, Action<object, int, uint> updateFunc, int stationID = 1)
        {
            m_updateFunc = updateFunc;
            m_addrGrp = addressGrp;
            m_number = Math.Min(addressGrp.Length, 0x40);

            var sb = new StringBuilder();

            // STX
            sb.Append(C_STX);
            // PLC_ID
            sb.Append(_PID(stationID));

            // COMMAND
            sb.Append("48");

            // CONTEXT
            sb.Append(m_number.ToString("X2"));

            int dataBytes = 0;
            foreach (var addr in addressGrp)
            {
                dataBytes += _getDataBytesLen(addr); 
                sb.Append(addr);
            }
            //respLength = 9 + dataBytes;

            // CHECKSUM
            appendCheckSum(sb);

            // ETX
            sb.Append(C_ETX);

            // convert
            CmdBytes = Conversion.ToBytes(sb);
        }
        
        public override FkResult HandleResponse(string responseStr)
        {
            var result = parseResponse(CmdBytes, responseStr);

            if (result.Error != 0)
                return result;

            var context = result.Context;
            var addrGrp = m_addrGrp;
            var number = m_number;
            var dstBuf = new uint[number];

            bool isAllOk = true;
            int pos = 1;

            for (int i = 0; i < number; i++)
            {
                try
                {
                    int dataBytesLen = _getDataBytesLen(addrGrp[i]);
                    int dataPos = pos;
                    pos += dataBytesLen;

                    string hex = context.Substring(dataPos, dataBytesLen);
                    dstBuf[i] = Convert.ToUInt32(hex, 16);
                }
                catch (Exception ex)
                {
                    dstBuf[i] = uint.MaxValue;

                    string tag = string.Format("[{0}/{1}] {2}", i, number, addrGrp[i]);

                    result = _MAKE_ERR_RESULT(FatekCommandErr.Err_ReadMixRegs_WrongData, responseStr, CmdBytes, tag, ex);

                    isAllOk = false;
                }
            }

            //> System.Diagnostics.Trace.WriteLineIf(m_debugDumpHex, "");

            // 仍然 Update 好的數據.
            if (m_updateFunc != null)
            {
                for (int i = 0; i < number; i++)
                {
                    if (dstBuf[i] != uint.MaxValue)
                        m_updateFunc(addrGrp[i], i, dstBuf[i]);
                }
            }

            if (isAllOk)
            {
                System.Diagnostics.Debug.Assert(result.Error == 0);
            }


            return result;
        }

        private int _getDataBytesLen(ADDR addr)
        {
            return Math.Max(1, addr.Bits / 4);
        }
    }
}
