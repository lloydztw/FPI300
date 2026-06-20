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

using EzComm.Utils;
using System;
using System.Text;
using FkResult = EzComm.Utils.ExResult;


namespace EzPlc.Fatek.Comm
{
    /// <summary>
    /// 永宏 PLC 命令 基礎類別<br/>
    /// <para>CMD= [STX] [plcID:2] [CmdCode:2] {Context:N} [ChkSum:2] [ETX]</para>
    /// <para>RET= [STX] [plcID:2] [CmdCode:2] [Err] {Context:NN} [ChkSum:2] [ETX]</para>
    /// </summary>
    public partial class FkCmd
    {
        public static bool OPT_INTERNAL_CHKSUM = true;
        public static bool OPT_RSP_INCLUDE_ETX = false;

        public override string ToString()
        {
            var str = Conversion.ToReadableStr(CmdBytes, 0, 3);
            str += " ";
            str += Conversion.ToReadableStr(CmdBytes, 3, 2);
            str += " ";
            str += Conversion.ToReadableStr(CmdBytes, 5, -1);
            return str;
        }

        /// <summary>
        /// CMD= [STX] + [plcID:2] + [CmdCode:2] + {Context:N} + [ChkSum:2] + [ETX]
        /// </summary>
        public byte[] CmdBytes
        {
            get;
            protected set;
        }

        /// <summary>
        /// 非同步執行時的, 執行優先等級 <br/>
        /// (請參閱 enum CmdPriority)
        /// </summary>
        public byte Priority
        {
            get;
            set;
        }

        #region INTERNAL_FUNCTIONS
        internal void changeStationID(int stationID)
        {
            var buf = CmdBytes;
            if (buf != null && buf.Length > 3)
            {
                var encode = new ASCIIEncoding();
                var bytes = encode.GetBytes(_PID(stationID));
                buf[1] = bytes[0];
                buf[2] = bytes[1];
            }
        }
        internal byte stationID
        {
            get
            {
                var buf = CmdBytes;
                if (buf != null && buf.Length >= 3)
                {
                    var str = string.Format("{0}{1}", buf[1], buf[2]);
                    uint id;
                    if (Conversion.TryParse(str, out id))
                        return (byte)id;
                }
                // No Command Data
                return (byte)1;
            }
        }
        internal static int getCmdCode(byte[] cmdBytes)
        {
            //int hex = (int)(cmdBytes[3] - '0');
            ////hex *= 16;
            //hex <<= 4;
            //hex += (int)(cmdBytes[4] - '0');
            int hex = Convert.ToInt32(Conversion.ToReadableStr(cmdBytes, 3, 2), 16);
            return hex;
        }
        internal static string getCmdCode(string responseStr)
        {
            var cmdCode = responseStr.Substring(3, 2);
            return cmdCode;
        }
        #endregion

        /// <summary>
        /// 處理 返回字串, 將其轉換為 FkResult
        /// </summary>
        public virtual FkResult HandleResponse(string responseStr)
        {
            var result = parseResponse(CmdBytes, responseStr);
            return result;
        }

        #region PROTECTED_FUCNTIONS

        protected FkResult parseResponse(byte[] cmdBytes, string responseStr)
        {
            bool hasEtx = OPT_RSP_INCLUDE_ETX;

            try
            {
                // (1) verify the length of the response
                if (!examLength(responseStr, hasEtx))
                    return _MAKE_ERR_RESULT(FatekCommandErr.Err_ResponseLength, responseStr, cmdBytes, null);

                // (2) verify check sum
                if (OPT_INTERNAL_CHKSUM)
                {
                    if (!examCheckSum(responseStr, hasEtx))
                        return _MAKE_ERR_RESULT(FatekCommandErr.Err_ResponseChkSum, responseStr, cmdBytes, null);
                }

                // (3) verify the first 5 characters
                if (cmdBytes != null && !examHeader(responseStr, cmdBytes, 5))
                    return _MAKE_ERR_RESULT(FatekCommandErr.Err_ResponseHeader, responseStr, cmdBytes, null);

                // Context
                int len = hasEtx ? (responseStr.Length - 8) : (responseStr.Length - 7);
                var context = responseStr.Substring(5, len);

                // Handle the Fatek error code
                //var cmdCode = getCmdCode(responseStr);
                //var cmdCode = getCmdCode(cmdBytes);
                //bool isEcho = ("4E" == getCmdCode(responseStr));
                bool isEcho = (0x4E == getCmdCode(cmdBytes));

                if (!isEcho)
                {
                    char errCode = responseStr[5];
                    if (errCode != '0')
                    {
                        return _MAKE_ERR_RESULT((FatekCommandErr)(errCode - '0'), responseStr, cmdBytes, null);
                    }
                }

                return new FkResult(0, context);
            }
            catch (Exception ex)
            {
                return _MAKE_ERR_RESULT(FatekCommandErr.Err_ReponseParsing_Exception, responseStr, CmdBytes, null, ex);
            }
        }

        protected FkResult _MAKE_ERR_RESULT(FatekCommandErr errCode, string respStr, byte[] cmdBytes, string auxStr = null, Exception ex = null)
        {
            if (errCode != 0)
            {
                var errMsg = new StringBuilder();

                if (cmdBytes != null)
                {
                    errMsg.Append(" : Cmd= ");
                    errMsg.Append(Conversion.ToReadableStr(cmdBytes));
                }

                if(respStr!=null)
                {
                    errMsg.Append(" : Ret= ");
                    errMsg.Append(Conversion.ToReadableStr(respStr));
                }

                if (auxStr != null)
                {
                    errMsg.Append(" : Msg= ");
                    errMsg.Append(auxStr);
                }

                return new FkResult((int)errCode, errMsg.ToString(), ex);
            }

            return FkResult.OK;
        }

        private void _STRESS_TEST()
        {
        }

        #endregion
    }
}
