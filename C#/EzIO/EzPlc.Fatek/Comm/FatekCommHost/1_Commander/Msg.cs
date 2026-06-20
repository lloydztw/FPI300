#region AUTHOR
/*
 * EzPlc.Fatek
 * Copyright (C) 2026
 * 2026-04-05 LeTian Chang: Integration with EzIO
 * 2019-11-30 LeTian Chang: Creation.
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzComm.Uart;
using System;
using QxNums = JetEazy.QxNums;


namespace EzPlc.Fatek.Comm
{
    public class MSG
    {
        public static string FormatErrorMsg(CUartCmdResult result)
        {
            if (result == null)
            {
                result = CUartCmdResult.NullResult(null);
            }

            if (result.Error != 0)
            {
                var totalMsg = new System.Text.StringBuilder();

                int err = result.Error;

                string errTxt;
                if (IsFatekCommandErr(err))
                    errTxt = QxNums.GetEnumDescription((FatekCommandErr)err);
                else if (IsUartCommErr(err))
                    errTxt = QxNums.GetEnumDescription((UartCommErr)err);
                else
                    errTxt = string.Format("Err[{0:X2}h]", err);

                totalMsg.Append(errTxt);


                if (result.Ex != null)
                {
                    totalMsg.Append("\n\r\n\rException=");
                    totalMsg.Append(FormatErrorMsg(result.Ex));
                }
                else
                {
                    string cmdStr = result.OwnerCmd != null ? result.OwnerCmd.ToString() : null;
                    if (cmdStr != null)
                    {
                        totalMsg.Append("\n\r @ Cmd=");
                        totalMsg.Append(cmdStr);
                    }

                    if (result.Context != null)
                    {
                        string rspStr = result.Context.Replace(errTxt, "").Trim();
                        if (!string.IsNullOrEmpty(rspStr))
                        {
                            totalMsg.Append(" : Ret=");
                            totalMsg.Append(rspStr);
                        }
                    }
                }

                return totalMsg.ToString();
            }

            return null;
        }
        public static string FormatErrorMsg(Exception ex)
        {
            if (ex == null)
                return "";

            string msg = ex.Message;
            if (ex.StackTrace != null)
            {
                msg += ("\n\r");
                msg += ex.StackTrace.Replace("於", "\n\r @ ");
            }

            return msg;
        }

        public static bool IsFatekCommandErr(int err)
        {
            var values = Enum.GetValues(typeof(FatekCommandErr));
            return (Array.IndexOf(values, (FatekCommandErr)err) >= 0);
        }
        public static bool IsUartCommErr(int err)
        {
            var values = Enum.GetValues(typeof(UartCommErr));
            return (Array.IndexOf(values, (UartCommErr)err) >= 0);
        }

        #region PRIVATE_FUNCTIONS
        private string _dumpGetCallerName()
        {
            var stackTrace = new System.Diagnostics.StackTrace();
            int count = stackTrace.FrameCount;
            int baseIdx = Math.Min(1, count - 1);
            string baseName = stackTrace.GetFrame(baseIdx).GetMethod().Name;

            for (int i = baseIdx + 1; i < count; i++)
            {
                var methodName = stackTrace.GetFrame(i).GetMethod().Name;
                if (methodName != baseName)
                {
                    return methodName;
                }
            }

            return baseName;
        }
        #endregion
    }
}
