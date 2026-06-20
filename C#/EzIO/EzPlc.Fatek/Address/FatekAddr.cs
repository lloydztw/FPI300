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

namespace EzPlc.Fatek
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

        #region 建構子
        /// <summary>
        /// ezIoName 格式: $"{StationID}:{Category}{Address}"
        /// </summary>
        public FatekAddr(string ezIoName, string description = null)
        {
            EzAddressParser.Parse(ezIoName, out int stationID, out FatekCateEnum cate, out int address, out int bitOffset);

            // 更新 IoAddress 的內部屬性 (I)
            this.StationID = (byte)stationID;
            this.CateID = (int)cate;
            this.Address = (ushort)address;
            this.Description = description;

            // 取得 bits, stride, bitOffset
            bitOffset = NormalizeBitsInfo(cate, out var bits, out var stride, bitOffset);

            // 更新 IoAddress 的內部屬性 (II)
            this.BitsEnum = bits;
            this.BitStride = (byte)stride;
            this.BitOffset = (byte)0;    // FATEK : BitOffset 強制設為 0
        }
        public FatekAddr(IAddress src)
        {
            CopyFrom(src);
        }
        public override object Clone()
        {
            return MemberwiseClone();
        }
        #endregion

        #region IAddress 實作與複寫 (最小站號)
        protected override byte MinStationID => 1;
        #endregion

        #region IAddress 實作與複寫 (Category)
        //string IFkAddress.Category => ((FatekCateEnum)CategoryID).ToString();
        protected override Enum CateEnum => (FatekCateEnum)CateID;
        public FatekCateEnum FkCategory
        {
            get => (FatekCateEnum)CateID;
            private set => CateID = Convert.ToInt32(value);
        }
        #endregion

        #region IAddress 通訊字串 (Overrides)
        public override string ToCommString()
        {
            int len = ParseCategory(FkCategory, out var _, out var _, out var digits, out var _, out var _);
            if (len == 0)
                return "";

            string fmt = $"D{digits}";  // new string('0', digits);
            string str = FkCategory.ToString() + Address.ToString(fmt);
            return str;
        }
        #endregion

        #region 邏輯規格化 (Overrides)
        /// <summary>
        /// 針對 Fatek 的定址特性進行規格化
        /// </summary>
        protected override int NormalizeBitsInfo(Enum cate, out IoBitsEnum bits, out int stride, int offset)
        {
            int ret = ParseCategory(this.FkCategory, out bits, out stride, out var _, out var alignment, out var maxAddr);

            // FATEK : BitOffset 強制設為 0
            offset = 0;

            // 進行邊界處理
            CheckBoundary(ref offset, ref stride);

            VERY(ret, alignment, maxAddr);

            return offset;
        }
        #endregion

        #region 序列化 ID 擴展 (Overrides)
        /// <summary>
        /// 針對 Fatek 的 UniqueID 可能需要特別處理 (例如 WM 與 M 的轉換)
        /// </summary>
        public override int UniqueID
        {
            get
            {
                //var fkCategory = this.FkCategory;

                //// 如果是群組型 WM, 它的 SerialID 應該對應到 M 的位置
                //if (fkCategory == FatekCateEnum.WM ||
                //    fkCategory == FatekCateEnum.WX ||
                //    fkCategory == FatekCateEnum.WY)
                //{
                //    // WM0 = M0 ~ M15, 所以 WM 地址需 * 16
                //    return Address * 16;
                //}

                //// 如果是群組型 WM, 它的 SerialID 應該對應到 M 的位置
                //if (fkCategory == FatekCateEnum.DWM ||
                //    fkCategory == FatekCateEnum.DWX ||
                //    fkCategory == FatekCateEnum.DWY)
                //{
                //    // DWM0 = M0 ~ M31, 所以 WM 地址需 * 32
                //    return Address * 32;
                //}

                return base.UniqueID;
            }
        }
        #endregion

        #region STRING_CONVERT_FUNCTIONS_字串格式轉換
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
        public static int ParseCategory(FatekCateEnum category, out IoBitsEnum bits, out int stride, out int digits, out int alignment, out int maxAddr)
        {
            int len;
            switch (category)
            {
                // [M]1234 (a group of 16-point M,X,Y data)
                case FatekCateEnum.X:
                case FatekCateEnum.Y:
                case FatekCateEnum.M:
                    bits = IoBitsEnum.Bits_1;
                    stride = 10; // Fatek 常用 10 進位定址 (如 M10 代表下一碼)
                    digits = 4;
                    len = 5;
                    alignment = 1;
                    maxAddr = 9999;
                    return len;

                // W[M]1234 (a group of 16-point M,X,Y data)
                case FatekCateEnum.WX:
                case FatekCateEnum.WY:
                case FatekCateEnum.WM:
                    bits = IoBitsEnum.Bits_16; // 群組型本質是 Word
                    stride = 1;
                    digits = 4;
                    len = 6;
                    alignment = 16;
                    maxAddr = 9984;
                    return len;

                // DW[M]1234 (a group of 32-point M,X,Y data)
                case FatekCateEnum.DWM:
                case FatekCateEnum.DWX:
                case FatekCateEnum.DWY:
                    bits = IoBitsEnum.Bits_32;
                    stride = 2; // 32-bit 佔用兩個暫存器空間
                    digits = 4;
                    len = 7;
                    alignment = 32;
                    maxAddr = 9968;
                    return 7;

                // [R]12345 (16-bit R/D register)
                case FatekCateEnum.R:
                case FatekCateEnum.D:
                    bits = IoBitsEnum.Bits_16;
                    stride = 1;
                    digits = 5;
                    len = 6;
                    alignment = 1;
                    maxAddr = 65535;
                    return len;

                // D[R]12345 (32-bit R register)
                case FatekCateEnum.DR:
                case FatekCateEnum.DD:
                    bits = IoBitsEnum.Bits_32;
                    stride = 2; // 32-bit 佔用兩個暫存器空間
                    digits = 5;
                    len = 7;
                    alignment = 2;
                    maxAddr = 65534;
                    return len;

                default:
                    bits = 0;
                    stride = 1;
                    digits = 0;
                    len = 0;
                    alignment = 1;
                    maxAddr = 0;
                    return len;
            }
        }
        #endregion

        #region ASSERTIONS_異常檢查
        void VERY(int cmdLen, int alignment, int maxAddr)
        {
            // V1. 檢查 Category
            if (cmdLen == 0)
                ASSERT_ERR_TYPE(this.FkCategory.ToString());

            // V2. 強制檢查最大位址限制
            ASSERT_ADDRESS_MAX(this, Address, maxAddr);

            // V3. 強制執行對齊限制 (例如 32-bit 必須是 2 的倍數)
            ASSERT_ALIGNMENT(this, Address, alignment);
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
