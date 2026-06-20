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

using EzIO.Device;
using System;
using System.Collections.Generic;

namespace EzIO.Mem
{
    /// <summary>
    /// IO 點位載體 的 通訊介面
    /// </summary>
    public interface IoDevice : IDisposable
    {
        event EventHandler OnFinalDisposing;

        /// <summary>
        /// 是否為離線模擬裝置
        /// </summary>
        bool IsSim { get; }

        /// <summary>
        /// 用來作 索引 的字串.
        /// </summary>
        string KeyName { get; }

        /// <summary>
        /// 站別
        /// </summary>
        byte StationID { get; }

        /// <summary>
        /// Bit 單筆寫入
        /// </summary>
        void WriteBit(IAddress addr, bool value);

        /// <summary>
        /// Reg 單筆寫入
        /// </summary>
        void WriteReg(IAddress addr, uint value);

        /// <summary>
        /// Reg 連續寫入
        /// </summary>
        void WriteRegs(IAddress addr, uint[] values);

        /// <summary>
        /// Bit 單筆讀出
        /// </summary>
        bool ReadBit(IAddress addr);

        /// <summary>
        /// Reg 單筆讀出
        /// </summary>
        uint ReadReg(IAddress addr);

        /// <summary>
        /// 連續讀出 N 個 Bits
        /// </summary>
        int ReadBits(IAddress addr, int N, ReadCallback<int, bool> callback, object arg);

        /// <summary>
        /// 連續讀出 N 個 Regs
        /// </summary>
        int ReadRegs(IAddress addr, int N, ReadCallback<int, uint> callback, object arg);

        /// <summary>
        /// 混合讀出 Regs
        /// </summary>
        int ReadRegs(IEnumerable<IAddress> mixAddrs, ReadCallback<IAddress, uint> callback, object arg);

        ///// <summary>
        ///// 向 IoDevice 發送立即的通訊指令 (sync blocked)
        ///// </summary>
        ///// <remark>
        ///// cmdArg 的具體型別, 由各廠家 IoDevice 實作 自行轉型.
        ///// </remark>        
        //void SendCmd(object cmdArg);

        /// <summary>
        /// 取得 IO點位 自動掃描器, 如果該設備不支援, 會返回 null.
        /// </summary>
        /// <remark>
        /// args 的具體型別, 由各廠家 IoDevice 實作 自行轉型.
        /// </remark>
        IAutoScan InstanceAutoScan(params object[] args);
    }

    /// <summary>
    /// 連續讀值之 callback function
    /// </summary>
    public delegate void ReadCallback<K,V>(K key, V value, object arg);
}
