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

namespace EzIO.Mem.Support
{
    public class CompactAddress15
    {
        #region CONSTANTS_BIT_DEFINITIONS
        // 位移量 (Shift)
        private const int SHIFT_ADDR = 0;
        private const int SHIFT_OFFSET = 15;
        private const int SHIFT_STRIDE = 20;
        private const int SHIFT_BITS = 26;

        // 位元遮罩 (Mask)
        private const int MASK_ADDR = 0x00007FFF;
        private const int MASK_OFFSET = 0x000F8000;
        private const int MASK_STRIDE = 0x03F00000;
        private const int MASK_BITS = unchecked((int)0xFC000000);
        #endregion

        #region PRIVATE_DATA
        // 使用 int 儲存所有資訊，支援 Interlocked 原子操作
        private int _data;
        #endregion

        public CompactAddress15(int addr, int bits, int offset, int stride)
        {
            NormalizeBitsInfo(ref bits, ref offset, ref stride);

            int d = 0;
            d |= (addr & 0x7FFF) << SHIFT_ADDR;
            d |= (offset & 0x1F) << SHIFT_OFFSET;
            d |= (stride & 0x3F) << SHIFT_STRIDE;
            d |= (bits & 0x3F) << SHIFT_BITS;
            _data = d;
        }

        public ushort Address
        {
            get => (ushort)((Thread.VolatileRead(ref _data) & MASK_ADDR) >> SHIFT_ADDR);
            set => UpdateData(MASK_ADDR, (value & 0x7FFF) << SHIFT_ADDR);
        }

        public byte BitOffset
        {
            get => (byte)((Thread.VolatileRead(ref _data) & MASK_OFFSET) >> SHIFT_OFFSET);
            set => UpdateData(MASK_OFFSET, (value & 0x1F) << SHIFT_OFFSET);
        }

        public byte BitStride
        {
            get => (byte)((Thread.VolatileRead(ref _data) & MASK_STRIDE) >> SHIFT_STRIDE);
            set => UpdateData(MASK_STRIDE, (value & 0x3F) << SHIFT_STRIDE);
        }

        public byte Bits
        {
            get
            {
                // 轉 uint 處理，確保最高 6 位元位移時不會因符號位擴展出錯
                uint d = unchecked((uint)Thread.VolatileRead(ref _data));
                return (byte)((d & unchecked((uint)MASK_BITS)) >> SHIFT_BITS);
            }
            set => UpdateData(MASK_BITS, (value & 0x3F) << SHIFT_BITS);
        }

        /// <summary>
        /// Lock-free 原子更新區段資料 (CAS 機制)
        /// </summary>
        private void UpdateData(int mask, int shiftedValue)
        {
            int initialValue, newValue;
            do
            {
                initialValue = _data;
                // 清除目標區段並填入新值
                newValue = (initialValue & ~mask) | shiftedValue;
            }
            while (Interlocked.CompareExchange(ref _data, newValue, initialValue) != initialValue);
        }

        protected virtual void NormalizeBitsInfo(ref int bits, ref int offset, ref int stride)
        {
            if (bits >= 32)
            {
                bits = 32;      // 目前暫時, 支援到 32-bit.
                offset = 0;
                //stride = 0;
                stride = 1;
            }
            else if (bits >= 16)
            {
                bits = 16;
                offset = 0;
                //stride = 0;
                stride = 1;
            }
            else
            {
                // BitStride 處理邏輯
                stride = (stride / 8) * 8;
                stride = Math.Min(Math.Max(stride, 8), 63);
                offset = Math.Min(Math.Max(offset, 0), 31);
                bits = 1;
            }
        }
    }
}
