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


namespace EzIO.Mem
{
    /// <summary>
    /// IO 暫存器 (16-bit or 32-bit), 每一個暫存器含有 N (16 or 32) 個 IoPointBit
    /// </summary>
    public class IoPointReg : IoPoint
    {
        #region PRIVATE_DATA
        private readonly IAddress _address;
        private readonly IoPointBit[] _bitCache;
        private int _data;
        #endregion

        public IoPointReg(IAddress address)
        {
            _address = address;
            _bitCache = new IoPointBit[Bits];
        }

        public override IAddress Address
        {
            get => _address;
        }
        public virtual IoPointBit this[int bit]
        {
            get
            {
                if (bit < 0 || bit >= Bits) 
                    throw new IndexOutOfRangeException();

                // 使用泛型版本，.NET 4.8 支援 object 的 CompareExchange
                if (_bitCache[bit] == null)
                {
                    var newBit = CreatePointBit(bit);
                    // 如果原本是 null，就換成 newBit
                    Interlocked.CompareExchange(ref _bitCache[bit], newBit, null);
                }
                return _bitCache[bit];
            }
        }

        public override uint Read(IoPriority priority = IoPriority.Normal)
        {
            // 使用 Thread.VolatileRead 確保讀取最新值
            return (uint)Thread.VolatileRead(ref _data);
        }
        public override void Write(uint data, IoPriority priority = IoPriority.Normal)
        {
            // 寫入時轉為 int 儲存
            Thread.VolatileWrite(ref _data, (int)data);
        }

        protected virtual IoPointBit CreatePointBit(int bit)
        {
            return new IoPointBit(null, this, bit);
        }

        #region INTERNAL_UPDATE_FUNCTIONS
        internal void AtomicUpdate(int bitOffset, bool set)
        {
            int initialValue, newValue;
            uint mask = 1u << bitOffset;

            do
            {
                initialValue = _data; // 讀取當前快照
                uint currentUint = (uint)initialValue;
                uint nextUint = set ? (currentUint | mask) : (currentUint & ~mask);
                newValue = (int)nextUint;

                // 修正：必須執行 CompareExchange 才能完成原子寫入
            } while (Interlocked.CompareExchange(ref _data, newValue, initialValue) != initialValue);
        }
        #endregion
    }
}