#region AUTHOR
/****************************************************************************
 *                                                                          
 * Copyright (c) 2013 LETIAN CHANG All rights reserved.        
 *                                                                          
 ***************************************************************************/

/****************************************************************************
 *
 * VERSION
 *		$Revision:$
 *
 * HISTORY
 *      $Id:$    
 *	        20130711 LeTian Chang : Creation         
 *
 * DESCRIPTION
 *      
 *
 ***************************************************************************/
#endregion

using System;
using System.Text;


namespace EzComm.Utils
{
    public class ChkSum
    {
        public static void appendCheckSum(StringBuilder sb)
        {
            sb.Append(string.Format("{0:X2}", calcCheckSum(sb)));
        }
        public static int calcCheckSum(StringBuilder sb)
        {
            int chkSum = 0;
            int len = sb.Length;
            for (int i = 0; i < len; i++)
            {
                chkSum = (chkSum + Convert.ToByte(sb[i])) % 256;
            }
            return chkSum;
        }
        public static int calcCheckSum(string str, int offset, int number = int.MaxValue)
        {
            int len = Math.Min(offset + number, str.Length);
            int chkSum = 0;
            for (int i = offset; i < len; i++)
            {
                chkSum = (chkSum + Convert.ToByte(str[i])) % 256;
            }
            return chkSum;
        }
        /// <summary>
        /// [***] {chkSum:2} [ETX]
        /// </summary>
        public static bool examCheckSum(string responseStr, bool hasETX)
        {
            int pos = hasETX ? responseStr.Length - 3 : responseStr.Length - 2;

            string chkSumStr = responseStr.Substring(pos, 2);

            var chkSumCalc = calcCheckSum(responseStr, 0, pos).ToString("X2");

            return (chkSumStr == chkSumCalc);
        }
    }
}
