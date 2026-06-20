#region AUTHOR
/*
 * EzIO.Mem
 * Copyright (C) 2023
 * 2013-07-11 created by LeTian Chang
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion


namespace EzIO.Mem.Utils
{
    public class EzConvert
    {
        public static uint U32(short i16)
        {
            return ((uint)i16) & 0xFFFF;
        }
        public static uint U32(ushort u16)
        {
            return ((uint)u16) & 0xFFFF;
        }
        public static uint U32(int i32)
        {
            return (uint)i32;
        }

        public static short S16(uint u32)
        {
            return I16(u32);
        }
        public static short I16(uint u32)
        {
            return (short)(u32 & 0xFFFF);
        }
        public static ushort U16(uint u32)
        {
            return (ushort)(u32 & 0xFFFF);
        }
        public static int I32(uint u32)
        {
            return (int)u32;
        }

        public static string HEX(short i16, int digits)
        {
            return i16.ToString("X" + digits);
        }
        public static string HEX(ushort u16, int digits)
        {
            return u16.ToString("X" + digits);
        }
        public static string HEX(int i32, int digits)
        {
            return i32.ToString("X" + digits);
        }
        public static string HEX(uint u32, int digits)
        {
            return u32.ToString("X" + digits);
        }
    }
}