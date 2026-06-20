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


namespace EzPlc.Fatek.Comm
{
    partial class FkCmd
    {
        public static char C_STX
        {
            get { return Convert.ToChar(FatekCommConsts.STX); }
        }
        public static char C_ETX
        {
            get { return Convert.ToChar(FatekCommConsts.ETX); }
        }

        protected static string _PID(int stationID)
        {
            string str = string.Format("{0:X2}", (byte)stationID);
            return str;
        }

        public static void appendCheckSum(StringBuilder sb)
        {
            //sb.Append(string.Format("{0:X2}", calcCheckSum(sb)));
            EzComm.Utils.ChkSum.appendCheckSum(sb);
        }
        public static int calcCheckSum(StringBuilder sb)
        {
            //int chkSum = 0;
            //int len = sb.Length;
            //for (int i = 0; i < len; i++)
            //{
            //    chkSum = (chkSum + Convert.ToByte(sb[i])) % 256;
            //}
            //return chkSum;
            return EzComm.Utils.ChkSum.calcCheckSum(sb);
        }
        public static int calcCheckSum(string str, int offset, int number = int.MaxValue)
        {
            //int len = Math.Min(offset + number, str.Length);
            //int chkSum = 0;
            //for (int i = offset; i < len; i++)
            //{
            //    chkSum = (chkSum + Convert.ToByte(str[i])) % 256;
            //}
            //return chkSum;
            return EzComm.Utils.ChkSum.calcCheckSum(str, offset, number);
        }

        /// <summary>
        /// [STX] {plcID:2} {cmdCode:2} [***] {chkSum:2} [ETX]
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool examCheckSum(string responseStr, bool hasETX)
        {
            /*
            int pos = hasETX ? responseStr.Length - 3 : responseStr.Length - 2;

            string chkSumStr = responseStr.Substring(pos, 2);
            //> System.Diagnostics.Trace.WriteLine("@@@ ChkSum= " + strChkSum);

            ////int sum = 0;
            ////for (int i = 0; i < pos; i++)
            ////    sum += (int)responseStr[i];
            ////sum &= 0xFF;
            ////string chkSumCalc = string.Format("{0:X2}", sum);

            var chkSumCalc = calcCheckSum(responseStr, 0, pos).ToString("X2");

            return (chkSumStr == chkSumCalc);
            */

            return EzComm.Utils.ChkSum.examCheckSum(responseStr, hasETX);
        }
        /// <summary>
        /// [STX] {plcID:2} {cmdCode:2} [***] {chkSum:2} [ETX]
        /// </summary>
        public static bool examLength(string responseStr, bool hasETX)
        {
            return hasETX ? (responseStr.Length >= 8) : (responseStr.Length >= 7);
        }
        /// <summary>
        /// [STX] {plcID:2} {cmdCode:2} [*****]
        /// </summary>
        public static bool examHeader(string responseStr, byte[] cmdBytes, int N = 5)
        {
            if (responseStr == null || cmdBytes == null ||
                responseStr.Length < N || cmdBytes.Length < N)
                return false;

            for (int i = 0; i < N; i++)
            {
                if (Convert.ToChar(cmdBytes[i]) != responseStr[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
