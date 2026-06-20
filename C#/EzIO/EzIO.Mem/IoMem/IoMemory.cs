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
using System.Collections.Concurrent;
using System.Collections.Generic;


namespace EzIO.Mem
{
    /// <summary>
    /// IoMemory 記憶體管理池 抽象類別
    /// <br/> 負責管理所有 <see cref="IoPoint"/> 的映射關係與生命週期.
    /// <br/> 其核心職責包括：
    /// <list type="bullet">
    /// <item>將 位址字串 (如 "1:M100") 解析並映射為 IoPoint 點位.</item>
    /// <item>維護 點位快取 ，確保相同位址請求返回同一個實例.</item>
    /// <item>支援 點位枚舉，以便進行批次通訊掃描或診斷.</item>
    /// <item>提供 索引子 (Indexer) 的便捷訪問方式.</item>
    /// </list>
    /// </summary>
    public abstract class IoMemory : IMemory
    {
        #region PROTECTED_DATA
        protected readonly ConcurrentDictionary<string, IoPoint> _points = new ConcurrentDictionary<string, IoPoint>();
        #endregion

        /// <summary>
        /// 建構式
        /// </summary>
        protected IoMemory(IoDevice device = null)
        {
            IoDevice = device;
        }

        public abstract IoPoint GetPoint(string ezIoName);
        public IoMemoryBank GetBank(string ezIoName, int span)
        {
            var addr = GetPoint(ezIoName)?.Address;
            return GetBank(addr, span);
        }
        public virtual IoMemoryBank GetBank(IAddress baseAddr, int span)
        {
            return new IoMemoryBank(this, baseAddr, span);
        }

        public IoPoint this[string ezIoName] => GetPoint(ezIoName);
        public IoPoint this[IAddress addr] => GetPoint(addr?.KeyName);
        public IoMemoryBank this[string ezIoName, int span] => GetBank(ezIoName, span);
        public IoMemoryBank this[IAddress addr, int span] => GetBank(addr?.KeyName, span);

        public IoDevice IoDevice
        {
            get;
            protected set;
        }
        public virtual void BindDevice(IoDevice device)
        {
            if (IoDevice != device && device != null)
            {
                IoDevice = device;
                //foreach (var ioReg in this)
                //{
                //    if (ioReg is IoDevPointReg devReg)
                //        devReg.BindDevice(device);
                //}
                foreach (var p in IterPoints())
                {
                    if (p is IoDevPointReg reg)
                        reg.BindDevice(device);
                }
            }
        }

        #region PROTECTED_HELP_FUNCTIONS
        /// <summary>
        /// 提供繼承者使用的 IoPoint 快取邏輯
        /// </summary>
        protected IoPoint GetOrAddPoint(string key, Func<string, IoPoint> factory)
        {
            return _points.GetOrAdd(key, factory);
        }
        #endregion

        #region 枚舉函式
        public IEnumerable<IoPoint> IterPoints()
        {
            foreach (var kv in _points)
                yield return kv.Value;
        }
        public IEnumerable<IoPointReg> IterRegs()
        {
            foreach (var p in IterPoints())
                if (p is IoPointReg reg)
                    yield return reg;
        }
        public List<IoPoint> GetCatePoints(int cateID, bool sort = true)
        {
            var points = new List<IoPoint>();
            foreach (var p in IterPoints())
            {
                if (p.Address.CateID == cateID)
                    points.Add(p);
            }
            if (sort && points.Count > 1)
                points.Sort(CompareOrder);
            return points;
        }
        public List<IoPoint> GetAllPoints(bool sort = true)
        {
            var points = new List<IoPoint>();
            foreach (var p in IterPoints())
            {
                points.Add(p);
            }
            if (sort && points.Count > 1)
                points.Sort(CompareOrder);
            return points;
        }
        protected virtual int CompareOrder(IoPoint p1,  IoPoint p2)
        {
            return IoAddress.CompareOrder(p1?.Address, p2?.Address);
        }
        #endregion
    }
}

