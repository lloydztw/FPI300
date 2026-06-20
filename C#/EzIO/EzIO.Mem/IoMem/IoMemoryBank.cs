#region AUTHOR
/*
 * EzIO.Mem
 * Copyright (C) 2026
 * 2026-04-05 revised by LeTian Chang
 * 2013-07-11 created by LeTian Chang
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;


namespace EzIO.Mem
{
    /// <summary>
    /// 代表一組連續位址的 IO 點位集合 (Memory Bank)。
    /// <br/> 主要用於處理批次資料操作與區塊化通訊。
    /// <list type="bullet">
    /// <item>提供對連續暫存器或位元點位的統一訪問介面。</item>
    /// <item>封裝了從 <see cref="IoMemory"/> 取得的連續點位陣列。</item>
    /// <item>常用於最佳化通訊效率，減少單點讀寫產生的封包負荷。</item>
    /// </list>
    /// </summary>
    public class IoMemoryBank : IEnumerable<IoPoint>
    {
        #region PROTECTED_DATA
        protected readonly IoMemory _host;
        protected readonly IoPoint[] _ioPoints;
        protected readonly int _alignment;
        #endregion

        public IoMemoryBank(IoMemory host, IAddress baseAddr, int span, int alignment = 1)
        {
            AutoAllocateEnabled = true;
            _host = host ?? throw new ArgumentNullException(nameof(host));
            _ioPoints = new IoPoint[span];
            _alignment = alignment;
            AddressBase = baseAddr;
            Span = span;
        }
        public IAddress AddressBase 
        { 
            get;
            private set;
        }
        public int ActualNumber
        {
            get => _ioPoints.Count(r => r != null);
        }
        public int Span
        {
            get;
            private set;
        }

        /// <summary>
        /// 是否可以自動填充空缺
        /// </summary>
        public bool AutoAllocateEnabled
        {
            get;
            set;
        }

        /// <summary>
        /// 索引值 
        /// <br/> (1) 如果是 16/32-bit Reg, 則為 絕對位址; 
        /// <br/> (2) 如果是 1-bit Point, 則為 offset index.
        /// <br/> 默認 可以自動填充空缺 (in-span) 
        /// </summary>
        public IoPoint this[int absAddressOrIndex]
        {
            get
            {
                if (AddressBase.Bits >= 16)
                    return smartGetReg(absAddressOrIndex);
                else
                    return smartGetBitPoint(index: absAddressOrIndex);
            }
        }

        /// <summary>
        /// 使用相對 index
        /// </summary>
        public IoPoint AtOffset(int index)
        {
            var point = (0 <= index && index < Span) ? _ioPoints[index] : null;
            return point;
        }

        /// <summary>
        /// 使用 index 來更新對應 點位的 cache 
        /// </summary>
        public void UpdateCacheByIndex(int index, uint value)
        {
            var cache = AtOffset(index);
            cache?.Write(value, IoPriority.InternalUpdate);
        }
        /// <summary>
        /// 使用 index 來更新對應 點位的 cache 
        /// </summary>
        public void UpdateCacheByIndex(int index, bool value)
        {
            var cache = AtOffset(index);
            cache?.Set(value, IoPriority.InternalUpdate);
        }

        IoPoint smartGetReg(int absoluteAddress)
        {
            int index = (absoluteAddress - AddressBase.Address) / _alignment;

            if (0 <= index && index < Span)
            {
                var ioPoint = _ioPoints[index];

                if (ioPoint == null && AutoAllocateEnabled)
                {
                    // 向父層 Map 請求點位，讓 Map 決定建立什麼類型的 Reg
                    var targetAddr = AddressBase.JumpTo(absoluteAddress);
                    ioPoint = _host.GetPoint(targetAddr.KeyName);
                    // 將 Map 傳回的唯一實例快取到 Bank 的陣列中以提升效能
                    System.Threading.Interlocked.CompareExchange(ref _ioPoints[index], ioPoint, null);
                }

                return ioPoint;
            }

            return null;
        }

        IoPoint smartGetBitPoint(int index)
        {
            if (0 <= index && index < Span)
            {
                var ioPoint = _ioPoints[index];
                if (ioPoint == null && AutoAllocateEnabled)
                {
                    // 向父層 Map 請求點位，讓 Map 決定建立什麼類型的 Reg
                    var targetAddr = AddressBase.Offset(index);
                    ioPoint = _host.GetPoint(targetAddr.KeyName);
                    // 將 Map 傳回的唯一實例快取到 Bank 的陣列中以提升效能
                    System.Threading.Interlocked.CompareExchange(ref _ioPoints[index], ioPoint, null);
                }
                return ioPoint;
            }
            return null;
        }

        #region IEnumerator
        public IEnumerator<IoPoint> GetEnumerator() => _ioPoints.Where(r => r != null).GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion

        public override string ToString()
        {
            return $"IoMemBank[{AddressBase}] (Span={Span}, ActualNumber={ActualNumber})";
        }
    }
}