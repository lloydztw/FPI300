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
    public class Conversion
    {
        public static bool TryParse(string hexStr, out uint number)
        {
            try
            {
                number = Convert.ToUInt32(hexStr, 16);
                return true;
            }
            catch
            {
                number = uint.MaxValue;
                return false;
            }
        }
        public static byte[] ToHexBytes(byte u8)
        {
            var str = string.Format("X2", u8);
            return ToBytes(str);
        }
        public static byte[] ToHexBytes(UInt16 u16)
        {
            var str = string.Format("X4", u16);
            return ToBytes(str);
        }
        public static byte[] ToHexBytes(UInt32 u32)
        {
            var str = string.Format("X8", u32);
            return ToBytes(str);
        }

        public static byte[] ToBytes(StringBuilder sb)
        {
            return ToBytes(sb.ToString());
        }
        public static byte[] ToBytes(string str)
        {
            var bytes = System.Text.Encoding.ASCII.GetBytes(str);
            return bytes;
        }
        public static string ToAsciiStr(byte[] bytes)
        {
            if (bytes != null)
                return System.Text.Encoding.ASCII.GetString(bytes);
            else
                return null;
        }
        public static string ToReadableStr(byte[] bytes, int start, int count)
        {
            string str = null;
            if (bytes != null)
            {
                if (count < 0)
                    count = bytes.Length;
                int end = Math.Min(bytes.Length, start + count);
                for (int i = start; i < end; i++)
                {
                    var b = bytes[i];
                    if (32 <= b && b < 127)
                        str += (char)b;
                    else
                        str += string.Format("<{0:X2}>", b);
                }
            }
            return str;
        }
        public static string ToReadableStr(byte[] bytes)
        {
            if (bytes != null)
            {
                return ToReadableStr(bytes, 0, bytes.Length);
            }
            return null;
        }
        public static string ToReadableStr(string src)
        {
            string str = null;
            if (src != null)
            {
                foreach (char b in src)
                {
                    if (32 <= ((int)b) && ((int)b) < 127)
                        str += b;
                    else
                        str += string.Format("<{0:X2}>", (int)b);
                }
            }
            return str;
        }
    }
}
