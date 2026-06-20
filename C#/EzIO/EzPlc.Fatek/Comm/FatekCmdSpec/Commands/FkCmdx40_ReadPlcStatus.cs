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
using FkResult = EzComm.Utils.ExResult;


namespace EzPlc.Fatek.Comm
{
    /// <summary>
    /// 0x40
    /// </summary>
    public class FkCmd_ReadPlcStatus : FkCmd
    {
        #region RUNTIME_DATA
        /// <summary>
        /// Runtime Buf for output
        /// </summary>
        private byte[] m_dstBuf;
        #endregion

        /// <summary>
        /// CMD= STX + <ID:2> + "40" + <ChkSum:2> + ETX
        /// RET= STX + <ID:2> + "40" + <ErrCode> + [S1:2] + [S1:2] + [S1:2] + <ChkSum:2> + ETX 
        /// </summary>
        /// <param name="stationID"></param>
        public FkCmd_ReadPlcStatus(byte[] dstBuf, int stationID = 1)
        {
            m_dstBuf = dstBuf;

            StringBuilder sb = new StringBuilder();
            // STX
            sb.Append(C_STX);
            // PLC_ID
            sb.Append(_PID(stationID));
            // COMMAND
            sb.Append("40");
            // CHECKSUM
            appendCheckSum(sb);
            // ETX
            sb.Append(C_ETX);

            CmdBytes = Conversion.ToBytes(sb);

            //>>> string s = ToString();
        }
        public override FkResult HandleResponse(string responseStr)
        {
            var result = parseResponse(CmdBytes, responseStr);

            if (result.Error == 0)
            {
                if (m_dstBuf != null && m_dstBuf.Length > 0)
                {
                    var context = result.Context;
                    var hexStr = context.Substring(1, 2);
                    uint status = 0;

                    if (Conversion.TryParse(hexStr, out status))
                    {
                        m_dstBuf[0] = (byte)status;
                    }
                    else
                    {
                        result = _MAKE_ERR_RESULT(FatekCommandErr.Err_ReadStatus_WrongStr, responseStr, CmdBytes, null);
                    }
                }
            }

            return result;
        }
    }
}
