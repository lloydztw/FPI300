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

using System;
using System.Threading;

namespace EzIO.Mem.Support.CompactE
{
    //==========================================================================================
    //  欄位	            長度 (Bits)   範圍 / 說明
    //  Address	        14 bits	儲存  0 ~ 16383 (足夠應付 Fatek/Hcfa 的 9999 定址)
    //  BitOffset	    5 bits	儲存  0 ~ 31
    //  BitStride	    6 bits	儲存  0 ~ 63
    //  BitsEnum	    3 bits	儲存  0 ~ 7    (對應 BitsEnum 的 index)
    //  CategoryEnum	4 bits	儲存  0 ~ 15   (對應具體 PLC 的 Enum Index)
    //==========================================================================================

    public class CompactAddress
    {
        #region CONSTANTS
        // 位移量重新定義
        private const int SHIFT_ADDR = 0;
        private const int SHIFT_OFFSET = 14;    // 14
        private const int SHIFT_STRIDE = 19;    // 5
        private const int SHIFT_BITS = 25;      // 3
        private const int SHIFT_CATE = 28;      // 4

        // 遮罩定義
        private const uint MASK_ADDR = 0x00003FFFu;
        private const uint MASK_OFFSET = 0x0007C000u;
        private const uint MASK_STRIDE = 0x01F80000u;
        private const uint MASK_BITS = 0x0E000000u;
        private const uint MASK_CATE = 0xF0000000u;
        #endregion

        #region PRIVATE_DATA
        private int _data; // 為了 Interlocked，必須宣告為 int
        #endregion

        public const int ADDRESS_END = (1 << 14);

        public CompactAddress(int addr, IoBitsEnum bits, int offset, int stride)
        {
            System.Diagnostics.Debug.Assert(addr < ADDRESS_END);

            //// 在初始化時進行規格化檢查
            //NormalizeBitsInfo(ref bits, ref offset, ref stride);

            uint d = 0;
            d |= ((uint)addr & 0x3FFF) << SHIFT_ADDR;
            d |= ((uint)offset & 0x1F) << SHIFT_OFFSET;
            d |= ((uint)stride & 0x3F) << SHIFT_STRIDE;
            d |= ((uint)bits & 0x07) << SHIFT_BITS;
            //d |= ((uint)cateValue & 0x0F) << SHIFT_CATE;

            _data = (int)d;
        }
        public CompactAddress(int addr, int bits, int offset, int stride)
                : this(addr, ToBitsEnum(bits), offset, stride)
        {
        }
        protected CompactAddress()
        {
        }

        public void CopyFrom(CompactAddress src)
        {
            if (src != null)
            {
                // 建議使用 VolatileRead 確保讀取到的是 src 最新且完整的 _data 狀態
                int val = Thread.VolatileRead(ref src._data);

                // 如果此物件可能被其他執行緒同時修改，建議使用 Interlocked.Exchange 進行原子替換
                Interlocked.Exchange(ref this._data, val);
            }
        }

        /// <summary>
        /// 這裡儲存 CateEnum 的 Int 數值，由子類別決定如何解釋
        /// </summary>
        public int CateID
        {
            get => (int)((unchecked((uint)Thread.VolatileRead(ref _data)) & (uint)MASK_CATE) >> SHIFT_CATE);
            protected set => UpdateData(MASK_CATE, ((uint)value & 0xF) << SHIFT_CATE);
        }
        
        public IoBitsEnum BitsEnum
        {
            get => (IoBitsEnum)((Thread.VolatileRead(ref _data) & MASK_BITS) >> SHIFT_BITS);
            set => UpdateData(MASK_BITS, ((uint)value & 0x7u) << SHIFT_BITS);
        }

        public ushort Address
        {
            get => (ushort)((Thread.VolatileRead(ref _data) & MASK_ADDR) >> SHIFT_ADDR);
            set => UpdateData(MASK_ADDR, ((uint)value & 0x3FFFu) << SHIFT_ADDR);
        }

        public byte BitOffset
        {
            get => (byte)((Thread.VolatileRead(ref _data) & MASK_OFFSET) >> SHIFT_OFFSET);
            set => UpdateData(MASK_OFFSET, ((uint)value & 0x1Fu) << SHIFT_OFFSET);
        }

        public byte BitStride
        {
            get => (byte)((Thread.VolatileRead(ref _data) & MASK_STRIDE) >> SHIFT_STRIDE);
            set => UpdateData(MASK_STRIDE, ((uint)value & 0x3Fu) << SHIFT_STRIDE);
        }

        public byte Bits
        {
            get => (byte)(1 << (int)this.BitsEnum);
            protected set
            {
                this.BitsEnum = ToBitsEnum((int)value);
            }
        }

        /// <summary>
        /// Lock-free 原子更新區段資料 (CAS 機制)
        /// </summary>
        private void UpdateData(uint mask, uint shiftedValue)
        {
            int initialValue, newValue;
            do
            {
                // 1. 讀取目前的 int
                initialValue = _data;

                // 2. 將其視為 uint 進行位元運算，確保安全
                uint currentUint = (uint)initialValue;
                uint nextUint = (currentUint & ~mask) | shiftedValue;

                // 3. 轉回 int 準備寫入
                newValue = (int)nextUint;
            }
            // 4. 使用 int 重載版本進行 CAS
            while (Interlocked.CompareExchange(ref _data, newValue, initialValue) != initialValue);
        }

#if(OPT_RESERVED)
        /// <summary>
        /// 根據 Bits 類別規格化 Offset 與 Stride
        /// </summary>
        protected virtual void NormalizeBitsInfo(ref BitsEnum bits, ref int offset, ref int stride)
        {
            // 針對 32-bit 以上的 DWord 型點位
            if (bits >= BitsEnum.Bits_32)
            {
                bits = BitsEnum.Bits_32;
                offset = 0;  // DWord 型位址不應有 BitOffset
                stride = 2;  // DWord 單位步進通常為 2
            }
            // 針對 16-bit 以上的 Word 型點位
            else if (bits >= BitsEnum.Bits_16)
            {
                bits = BitsEnum.Bits_16;
                offset = 0;  // Word 型位址不應有 BitOffset
                stride = 1;  // Word 單位步進通常為 1
            }
            // 針對 8-bit 的 Byte/Group 型點位 (如 Hcfa QB)
            else if (bits == BitsEnum.Bits_8)
            {
                bits = BitsEnum.Bits_8;
                stride = (stride < 8) ? 8 : stride; // 最少為 8 進位
                offset = offset % stride;
            }
            // 針對 1-bit 型點位
            else
            {
                // 確保 Stride 合理 (預設 8 或採用傳入值)
                bits = BitsEnum.Bits_1;
                if (stride <= 0) 
                    stride = 8;
                offset = offset % stride;
            }

            CheckBoundary(ref offset, ref stride);
        }
#endif
        protected void CheckBoundary(ref int offset, ref int stride)
        {
            // 最終邊界安全檢查
            offset = Math.Max(0, Math.Min(offset, 31));
            stride = Math.Max(1, Math.Min(stride, 63));
        }

        public static IoBitsEnum ToBitsEnum(int bits)
        {
            // 採用最簡單明瞭的 if 縮排即可，不需 else 讓視覺更乾淨
            if (bits >= 32) return IoBitsEnum.Bits_32;
            if (bits >= 16) return IoBitsEnum.Bits_16;
            if (bits >= 8) return IoBitsEnum.Bits_8;
            return IoBitsEnum.Bits_1;
        }
    }
}
