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
using CompactAddress = EzIO.Mem.Support.CompactE.CompactAddress;

namespace EzIO.Mem
{
    /// <summary>
    /// 通用型 定址
    /// </summary>
    public abstract class IoAddress : CompactAddress, IAddress, ICloneable
    {
        #region DATA_MEMBERS
        byte _stationId;
        #endregion

        protected IoAddress(Enum cate, int address, int stationId = 0, int bits = 16, int stride = 1, int offset = 0)
                    : base(address, bits, offset, stride)
        {
            //if (stride <= 0 && bits < 16)
            //{
            //    // 拋出異常，防止後續所有計算出錯
            //    throw new ArgumentException("對於 Bit 類型的點位，BitStride 必須大於 0");
            //}

            CateID = Convert.ToInt32(cate);
            StationID = (byte)stationId;
        }
        protected IoAddress()
        {
        }

        #region COPY_AND_CLONE
        public virtual void CopyFrom(IAddress src)
        {
            if (src == null) return;

            StationID = src.StationID;
            Description = src.Description;

            if (src is CompactAddress aSrc)
            {
                this.CopyFrom(aSrc);
            }
            else
            {
                CateID = src.CateID;
                Address = src.Address;
                Bits = src.Bits;
                BitStride = src.BitStride;
                BitOffset = src.BitOffset;
            }
        }
        public abstract object Clone();
        #endregion


        /// <summary>
        /// 索引字串 : 默認格式為 $"{StationID}:{Category}{Address}.{BitOffset}"
        /// </summary>
        /// <remarks>
        /// Station <= 0 時, 前置詞 $"{StationID}:" 可以省略。
        /// </remarks>
        public virtual string KeyName
        {
            get
            {
                string prefix = (StationID > MinStationID) ? $"{StationID}:" : "";
                //string postfix = (Bits == 1 && BitStride > 1) ? $".{BitOffset}" : "";
                //return $"{prefix}{Category}{Address}{postfix}";
                return $"{prefix}{Category}{Address}";
            }
        }

        public string Description
        {
            get;
            set;
        }

        public byte StationID
        {
            get
            {
                return _stationId;
            }
            protected set
            {
                _stationId = Math.Max(value, MinStationID);
            }
        }

        protected virtual byte MinStationID => 0;

        /// <summary>
        /// 注意使用 Enum 邊際效應 : a1.Cate == a2.Cate 可能會比對失敗, 即便 a1.CategoryID == a2.CategoryID !!!
        /// </summary>
        protected abstract Enum CateEnum { get; }

        public string Category
        {
            get => CateEnum.ToString();
        }

        public virtual ModbusCateEnum GetModbusCategory()
        {
            return Bits == 1 ? ModbusCateEnum.COIL : ModbusCateEnum.HOLDING_REGISTER;
        }

        /// <summary>
        /// 根據 Cate 類別, 規格化 Bits, Stride, 與 Offset
        /// </summary>
        protected virtual int NormalizeBitsInfo(Enum cate, out IoBitsEnum bits, out int stride, int offset = 0)
        {
            bits = IoBitsEnum.Bits_16;
            stride = 1;
            return offset;
        }

        #region PUBLIC_STRING_FUNCTIONS_字串格式轉換
        /// <summary>
        /// 通訊用字串
        /// </summary>
        public virtual string ToCommString()
        {
            return KeyName;
        }

        /// <summary>
        /// 顯示用字串 : 默認值 等於 <see cref="IoAddress.KeyName"/> 
        /// </summary>
        public override string ToString()
        {
            return KeyName;
        }
        #endregion

        #region PUBLIC_COMPARE_FUNCTIONS_位址比較
        public static int CompareOrder(IAddress a, IAddress b)
        {
            // 不為 null 往前排
            if (a == null || b == null)
                return (a == b) ? 0 : (a != null ? -1 : 1);

            // Category
            //int cc = string.Compare(a.Category, b.Category);
            int cc = a.CateID - b.CateID;
            if (cc != 0)
                return cc;

            // 大 Bits 往前排
            if (a.Bits > b.Bits)
                return -1;
            if (a.Bits < b.Bits)
                return 1;

            // 大 Address 往後排
            if (a.Address > b.Address)
                return 1;
            if (a.Address < b.Address)
                return -1;

            // 比對 BitOffset, 大 BitOffset 往後排
            if (a.Bits == 1)
            {
                if (a.BitOffset > b.BitOffset)
                    return 1;
                if (a.BitOffset < b.BitOffset)
                    return -1;
            }

            // 相等 位置不動!
            return 0;
        }
        public static bool AreEqual(IAddress a, IAddress b)
        {
            return CompareOrder(a, b) == 0;
        }
        #endregion

        #region PUBLIC_CALC_FUNCTIONS_位址運算
        public virtual int UniqueID
        {
            get
            {
                // 邏輯維持不變，但直接存取實體成員
                return (Bits < 16)
                        ? (Address * BitStride + BitOffset)
                        : Address;
            }
        }

        public static int BitsDiff(IAddress a, IAddress b)
        {
            System.Diagnostics.Trace.Assert(a.CateID == b.CateID, $"Category : {a.Category}, {b.Category} 必須相同 !");

            return a.UniqueID - b.UniqueID;
        }
        public static int PointsDiff(IAddress a, IAddress b)
        {
            System.Diagnostics.Trace.Assert(a.CateID == b.CateID, $"Category : {a.Category}, {b.Category} 必須相同 !");
            System.Diagnostics.Trace.Assert(a.Bits == b.Bits, $"Bits : {a.Bits}, {b.Bits} 必須相同 !");

            if (a.Bits >= 16)
                return a.Address - b.Address;
            else
                return BitsDiff(a, b);
        }

#if(false)
        IAddress IAddress.Offset(int offset, bool inplace)
        {
            return Offset(offset, inplace);
        }
        IAddress IAddress.JumpTo(int address, bool inplace)
        {
            return JumpTo(address, inplace);
        }
#endif

        public virtual IAddress Offset(int offset = 1, bool inplace = false)
        {
            var addr = inplace ? this : (IoAddress)this.Clone();
            if (offset <= 0)
                return addr;

            int newAddress;
            int newBitOffset;

            if (this.Bits >= 16)
            {
                newAddress = this.Address + offset;
                newBitOffset = 0;
            }
            else
            {
                int stride = this.BitStride;
                if (stride == 0)
                    stride = 8;

                int A = this.UniqueID + offset;
                newAddress = A / stride;
                newBitOffset = A % stride;
            }

            //> var addr = inplace ? this : new EzAddress(this);
            addr.Address = (ushort)newAddress;
            addr.BitOffset = (byte)newBitOffset;
            return addr;
        }
        public virtual IAddress JumpTo(int address, bool inplace = false)
        {
            var addr = inplace ? this : (IoAddress)this.Clone();
            addr.Address = (ushort)address;

            //> 如果有需求的話, 由 caller 自己 reset
            //>   BitOffset = 0

            return addr;
        }

        public static IoAddress operator +(IoAddress addr, int offset)
        {
            return addr.Offset(offset, inplace: false) as IoAddress;
        }
        public static int operator -(IoAddress a, IoAddress b)
        {
            return PointsDiff(a, b);
        }
        #endregion
    }
}
