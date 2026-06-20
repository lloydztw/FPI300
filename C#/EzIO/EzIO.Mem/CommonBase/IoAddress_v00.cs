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

using EzIO.Mem.Support;
using System;


namespace EzIO.Mem.V0
{
    /// <summary>
    /// 通用型 定址
    /// </summary>
    public abstract class IoAddress : CompactAddress15, IAddress, ICloneable
    {
        #region DATA_MEMBERS
        byte _stationId;
        string _category;
        string _description;
        #endregion

        protected IoAddress(string cate, int address, int stationId = 1, int bits = 1, int stride = 1, int offset = 0)
                    : base(address, bits, offset, stride)
        {
            if (stride <= 0 && bits < 16)
            {
                // 拋出異常，防止後續所有計算出錯
                throw new ArgumentException("對於 Bit 類型的點位，BitStride 必須大於 0");
            }

            _description = null;
            _category = cate.ToUpper().Trim();
            StationID = (byte)stationId;
        }
        protected IoAddress() : base(0, 16, 0, 1)
        {
            StationID = 1;
        }
        public abstract object Clone();
        public virtual void CopyFrom(IAddress src)
        {
            if (src == null) return;

            _stationId = src.StationID;
            _category = src.Category;
            _description = src.Description;

            Address = src.Address;
            Bits = src.Bits;
            BitStride = src.BitStride;
            BitOffset = src.BitOffset;
        }

        public byte StationID
        {
            get
            {
                return _stationId;
            }
            protected set
            {
                _stationId = value > 1 ? value : (byte)1;
            }
        }
        public Enum Cate
        {
            get;
            set;
        }
        public string Category
        {
            get
            {
                return _category;
            }
            protected set
            {
                _category = value;
            }
        }
        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                _description = value;
            }
        }
        public virtual string KeyName
        {
            get
            {
                //使用 簡約格式: $"{StationID}:{Category}{Address}.{BitOffset}"
                string sid = (StationID > 1) ? $"{StationID}:" : "";
                string ext = (Bits < 16 && BitStride > 1) ? $".{BitOffset}" : "";
                return $"{sid}{Category}{Address}{ext}";
            }
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
        /// 簡約格式: $"{StationID}:{Category}{Address}.{BitOffset}"
        /// </summary>
        public override string ToString()
        {
            //string sid = (StationID > 1) ? $"{StationID}:" : "";
            //string ext = (Bits < 16 && BitStride > 1) ? $".{BitOffset}" : "";
            //return $"{sid}{Category}{Address}{ext}";
            return KeyName;
        }
        #endregion

        #region PUBLIC_COMPARE_FUNCTIONS_位址比較
        public static int CompareOrder(IAddress a, IAddress b)
        {
            // 不為 null 往前排
            if (a == null || b == null)
                return (a == b) ? 0 : (a != null ? -1 : 1);

            // 大 Bits 往前排
            if (a.Bits > b.Bits)
                return -1;
            if (a.Bits < b.Bits)
                return 1;

            // Category
            int cc = string.Compare(a.Category, b.Category);
            if (cc != 0)
                return cc;

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
        public int UniqueID
        {
            get
            {
                return (Bits < 16)
                    ? (Address * BitStride + BitOffset)
                    : Address;
            }
        }

        /// <summary>
        /// 取得在同一個 Category 內, 的獨一序號編碼.
        /// </summary>
        public static int UniqueSerialID(IAddress a)
        {
            if (a.Bits < 16)
                return a.Address * a.BitStride + a.BitOffset;
            else
                return a.Address;
        }
        public static int BitsDiff(IAddress a, IAddress b)
        {
            System.Diagnostics.Trace.Assert(a.Category == b.Category);
            return UniqueSerialID(a) - UniqueSerialID(b);
        }
        public static int PointsDiff(IAddress a, IAddress b)
        {
            System.Diagnostics.Trace.Assert(a.Category == b.Category);
            System.Diagnostics.Trace.Assert(a.Bits == b.Bits);
            if (a.Bits >= 16)
                return a.Address - b.Address;
            else
                return BitsDiff(a, b);
        }

        IAddress IAddress.Offset(int offset, bool inplace)
        {
            return Offset(offset, inplace);
        }
        IAddress IAddress.JumpTo(int address, bool inplace)
        {
            return JumpTo(address, inplace);
        }

        public virtual IoAddress Offset(int offset = 1, bool inplace = false)
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

                int A = UniqueSerialID(this) + offset;
                newAddress = A / stride;
                newBitOffset = A % stride;
            }

            //> var addr = inplace ? this : new EzAddress(this);
            addr.Address = (ushort)newAddress;
            addr.BitOffset = (byte)newBitOffset;
            return addr;
        }
        public virtual IoAddress JumpTo(int address, bool inplace = false)
        {
            var addr = inplace ? this : (IoAddress)this.Clone();
            addr.Address = (ushort)address;

            //> 如果有需求的話, 由 caller 自己 reset
            //>   BitOffset = 0

            return addr;
        }

        public static IoAddress operator +(IoAddress addr, int offset)
        {
            return addr.Offset(offset, inplace: false);
        }
        public static int operator -(IoAddress a, IoAddress b)
        {
            return PointsDiff(a, b);
        }
        #endregion
    }
}
