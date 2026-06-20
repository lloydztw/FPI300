using System;

namespace EzPlc.Fatek.Comm
{
    partial class FatekAddress
    {
        /// <summary>
        /// X0000 ~ X9999
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static FatekAddress X(int address)
        {
            return new FatekAddress(MemoryType.X, address);
        }
        /// <summary>
        /// Y0000 ~ Y9999
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static FatekAddress Y(int address)
        {
            return new FatekAddress(MemoryType.Y, address);
        }
        /// <summary>
        /// M0000 ~ M9999
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static FatekAddress M(int address)
        {
            return new FatekAddress(MemoryType.M, address);
        }
        /// <summary>
        /// WX0000～ WX9984
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static FatekAddress WX(int address)
        {
            return new FatekAddress(MemoryType.WX, address);
        }
        /// <summary>
        /// WY0000～ WY9984 (16 bit)
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static FatekAddress WY(int address)
        {
            return new FatekAddress(MemoryType.WY, address);
        }
        /// <summary>
        /// WM0000～ WM9984 (16 bit)
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static FatekAddress WM(int address)
        {
            return new FatekAddress(MemoryType.WM, address);
        }

        /// <summary>
        /// DWX0000～ DWX9968 (32 bit)
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static FatekAddress DWX(int address)
        {
            return new FatekAddress(MemoryType.DWX, address);
        }
        /// <summary>
        /// DWY0000～ DWY9968 (32 bit)
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static FatekAddress DWY(int address)
        {
            return new FatekAddress(MemoryType.DWY, address);
        }
        /// <summary>
        /// DWM0000～ DWM9968 (32 bit)
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static FatekAddress DWM(int address)
        {
            return new FatekAddress(MemoryType.DWM, address);
        }

        /// <summary>
        /// DR 資料暫存器: D00000～ D65535 (16 bit)
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static FatekAddress D(int address)
        {
            return new FatekAddress(MemoryType.D, address);
        }
        /// <summary>
        /// HR 資料暫存器: R00000～ R65535 (16 bit)
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static FatekAddress R(int address)
        {
            return new FatekAddress(MemoryType.R, address);
        }
        /// <summary>
        /// DR 資料暫存器: D00000～ D65534 (32 bit)
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static FatekAddress DD(int address)
        {
            return new FatekAddress(MemoryType.DD, address);
        }
        /// <summary>
        /// HR 資料暫存器: R00000～ R65534 (32 bit)
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static FatekAddress DR(int address)
        {
            return new FatekAddress(MemoryType.DR, address);
        }
    }
}
