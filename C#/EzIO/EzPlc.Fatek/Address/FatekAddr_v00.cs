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

using EzIO.Mem;
using EzIO.Mem.Utils;
using System;
using IFkAddress = EzPlc.Fatek.Comm.IFkAddress;
using IoAddress = EzIO.Mem.IoAddress;


namespace EzPlc.Fatek.V00
{
    /// <summary>
    /// FatekAddr
    /// <br/> 負責 永宏PLC 位址 格式轉換
    /// </summary>
    public partial class FatekAddr : IoAddress, IFkAddress
    {
        #region NLOG
        //static NLog.Logger _LOG => EzLog.LOG;
        #endregion

        /// <summary>
        /// ezIoName 格式: $"{StationID}:{Category}{Address}"
        /// </summary>
        public FatekAddr(string ezIoName, string description = null)
        {
            EzAddressParser.Parse(ezIoName, out int sid, out string cate, out int address, out int bitOffset);

            this.StationID = (byte)sid;
            this.Category = cate;
            this.Address = (ushort)address;
            this.Description = description;

            //Fatek 專屬的限制檢查
            ApplyFatekConstraints();
        }
        public FatekAddr(IAddress src)
        {
            CopyFrom(src);
        }
        public override object Clone()
        {
            return MemberwiseClone();
        }

        #region STRING_CONVERT_FUNCTIONS_字串格式轉換
        /// <summary>
        /// 轉換成 Fatek 通訊字串格式
        /// </summary>
        public override string ToCommString()
        {
            int len = ParseCategory(
                        Category,
                        out ushort usbits,
                        out ushort digits,
                        out ushort alignment,
                        out int maxAddr);

            if (len == 0)
                return "";

            string fmt = new string('0', digits);
            string str = Category + Address.ToString(fmt);
            return str;
        }

        /// <summary>
        /// 嘗試由 ezIoName 生成 FatekAddr
        /// </summary>
        /// <remarks>
        /// 通用格式: $"{StationID}:{Category}{Address}.{BitOffset}"
        /// </remarks>
        public static FatekAddr TryParse(string ezIoName)
        {
            try
            {
                var addr = new FatekAddr(ezIoName);
                return addr;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 根據 category, 來取得 Fatek 的 address 限制
        /// </summary>
        public static int ParseCategory(string category, out ushort bits, out ushort digits, out ushort alignment, out int maxAddr)
        {
            alignment = 1;

            switch (category)
            {
                // [M]1234 (a group of 16-point M,X,Y data)
                case "M":
                case "X":
                case "Y":
                    bits = 1;
                    digits = 4;
                    maxAddr = 9999;
                    return 5;

                // [R]12345 (16-bit R/D register)
                case "R":
                case "D":
                    bits = 16;
                    digits = 5;
                    maxAddr = 65535;
                    return 6;

                // W[M]1234 (a group of 16-point M,X,Y data)
                case "WM":
                case "WX":
                case "WY":
                    bits = 16;
                    digits = 4;
                    maxAddr = 9984;
                    alignment = 16;
                    return 6;

                // D[R]12345 (32-bit R register)
                case "DR":
                case "DD":
                    bits = 32;
                    digits = 5;
                    maxAddr = 65534;
                    alignment = 2;
                    return 7;

                // DW[M]1234 (a group of 32-point M,X,Y data)
                case "DWM":
                case "DWX":
                case "DWY":
                    bits = 32;
                    digits = 4;
                    maxAddr = 9968;
                    alignment = 32;
                    return 7;

                default:
                    bits = 0;
                    digits = 0;
                    maxAddr = 0;
                    return 0;
            }
        }
        #endregion

        #region ASSERTIONS_異常檢查
        /// <summary>
        /// 實作 Fatek 專屬約束校驗與修正
        /// </summary>
        void ApplyFatekConstraints()
        {
            // 1. 取得該類型的限制參數
            int len = ParseCategory(
                            this.Category,
                            out ushort usbits,
                            out ushort digits,
                            out ushort alignment,
                            out int maxAddr);

            // 1. 檢查 Category
            if (len == 0)
                ASSERT_ERR_TYPE(this.Category);

            // 2. 強制校驗最大位址限制
            ASSERT_ADDRESS_MAX(this, this.Address, maxAddr);

            // 3. 強制執行對齊限制 (例如 32-bit 必須是 2 的倍數)
            ASSERT_ALIGNMENT(this, this.Address, alignment);


            // 4. 同步更新 IoAddress 的內部屬性
            this.Bits = (byte)usbits;    // 根據 M/R/DR 設定 1, 16, 32
            this.BitStride = (byte)1;    // Fatek 標準間距
            this.BitOffset = (byte)0;    // 初始偏移為 0
        }
        static bool ASSERT_FORMAT(IoAddress addr)
        {
            int len = ParseCategory(addr.Category,
                        out ushort usbits,
                        out ushort digits,
                        out ushort alignment,
                        out int maxAddr);

            if (len > 0 &&
                ASSERT_ADDRESS_MAX(addr, addr.Address, maxAddr) &&
                ASSERT_ALIGNMENT(addr, addr.Address, alignment))
                return true;

            return false;
        }
        static bool ASSERT_ERR_TYPE(string errTag)
        {
            if (errTag != null)
                throw new ApplicationException($"FatekAddr 不支援的型別 : {errTag}");
            return true;
        }
        static bool ASSERT_ERR_FORMAT(string errTag)
        {
            if (errTag != null)
                throw new ApplicationException($"FatekAddr 格式錯誤 : {errTag}");
            return true;
        }
        static bool ASSERT_LENGTH(string str, int len)
        {
            if (str.Length != len)
                throw new ApplicationException($"FatekAddr 長度必須是 {len} : {str}");
            return true;
        }
        static bool ASSERT_ALIGNMENT(object tag, int address, int alignBits)
        {
            if (address % alignBits != 0)
                throw new ApplicationException($"FatekAddr 位址必須對齊 {alignBits} 的倍數 : {tag}");
            return true;
        }
        static bool ASSERT_ADDRESS_MAX(object tag, int address, int max)
        {
            if (address > max)
            {
                throw new ApplicationException($"FatekAddr 位置不能超過 {max} : {tag}");
            }
            else if (address < 0)
            {
                ASSERT_ERR_FORMAT(tag.ToString());
                return false;
            }
            else
            {
                return true;
            }
        }
        #endregion
    }
}
