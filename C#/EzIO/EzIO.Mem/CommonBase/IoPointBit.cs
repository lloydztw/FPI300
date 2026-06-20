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

namespace EzIO.Mem
{
    /// <summary>
    /// IO 點位 (1-Bit 點位)
    /// </summary>
    public class IoPointBit : IoPoint
    {
        #region PRIVATE_DATA
        private readonly IoPointReg _host;
        private readonly int _bitOffset;
        private bool _bitDataOne;
        #endregion

        #region PROTECTED_DATA
        /// <summary>
        /// 儲存 PLC 原始的位址，例如 M18, QB1080.5
        /// </summary>
        protected readonly IAddress _originalAddress;
        #endregion

        #region 內部建構子
        internal protected IoPointBit(IAddress originalAddress, IoPointReg host, int bitOffset)
        {
            _originalAddress = originalAddress;
            _host = host;
            _bitOffset = bitOffset;
        }
        #endregion

        public override IoPoint Host
        {
            get => _host;
        }
        public override IAddress Address
        {
            get
            {
                if (_originalAddress != null)
                    return _originalAddress;
                else
                    return _host.Address?.Offset(_bitOffset);
            }
        }

        /// <summary>
        /// 這表示 與 Host 的 BitOffset 之間的位元位移關係,
        /// Host不同, BitOffset 也有所不同.
        /// 與 IAddress.BitOffset 不一定相同值 !!!
        /// </summary>
        public int BitOffset
        {
            get => _bitOffset;
        }

        public override uint Read(IoPriority priority = IoPriority.Normal)
        {
            // 如果沒有 Host, 就用自己的資料
            if (_host == null)
            {
                return _bitDataOne ? 1u : 0u;
            }
            else
            {
                uint data = _host.Read(priority);
                data = data & (1u << _bitOffset);
                return data != 0 ? 1u : 0u;
            }
        }
        public override void Write(uint data, IoPriority priority = IoPriority.Normal)
        {
            bool one = data != 0;
            _host?.AtomicUpdate(_bitOffset, one);
            _bitDataOne = one;
        }
    }
}