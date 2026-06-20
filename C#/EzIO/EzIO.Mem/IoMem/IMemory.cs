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

using System.Collections.Generic;


namespace EzIO.Mem
{
    /// <summary>
    /// IMemory 記憶體管理池 介面
    /// <br/> 負責管理所有 <see cref="IoPoint"/> 映射關係.
    /// <br/> 其核心職責包括：
    /// <list type="bullet">
    /// <item>將 位址字串 (如 "1:M100") 解析並映射為 IoPoint 點位。</item>
    /// <item>維護 點位快取 ，確保相同位址請求返回同一個實例。</item>
    /// <item>支援 點位枚舉，以便進行批次通訊掃描或診斷。</item>
    /// <item>提供 索引子 (Indexer) 的便捷訪問方式。</item>
    /// </list>
    /// </summary>
    public interface IMemory //: IEnumerable<IoPointReg>
    {
        /// <summary>
        /// 根據字串位址取得對應的 IoPoint (Bit 或 Reg),
        /// 若該點位尚未建立，則會根據位址規則自動分配.
        /// </summary>
        /// <param name="ezIoName">位址格式: $"{StationID}:{Category}{Address}.{BitOffset}"</param>
        IoPoint GetPoint(string ezIoName);

        /// <summary>
        /// 取得連續區塊的點位集合 (IoMemoryBank).
        /// </summary>
        /// <param name="ezIoName">起始位址"</param>
        /// <param name="span">連續點位的數量</param>
        IoMemoryBank GetBank(string ezIoName, int span);

        /// <summary>
        /// 取得連續區塊的點位集合 (IoMemoryBank)。
        /// </summary>
        /// <param name="baseAddr">起始位址</param>
        /// <param name="span">連續點位的數量</param>
        IoMemoryBank GetBank(IAddress baseAddr, int span);

        /// <summary>
        /// 使用 位址字串 索引子 取得 IoPoint.
        /// </summary>
        IoPoint this[string ezIoName] { get; }

        /// <summary>
        /// 使用 位址介面 索引子 取得 IoPoint.
        /// </summary>
        IoPoint this[IAddress addr] { get; }

        /// <summary>
        /// 使用 位址字串 索引子 取得 IoMemoryBank.
        /// </summary>
        IoMemoryBank this[string ezIoName, int span] { get; }

        /// <summary>
        /// 使用 位址介面 索引子 取得 IoMemoryBank.
        /// </summary>
        IoMemoryBank this[IAddress addr, int span] { get; }

        /// <summary>
        /// 枚舉所有 已註冊的 IoPoints
        /// </summary>
        IEnumerable<IoPoint> IterPoints();

        /// <summary>
        /// 實體設備
        /// </summary>
        /// <remarks>
        /// 如果是 null, 代表 目前的 IoMemory 是虛擬內存/記憶體.
        /// </remarks>
        IoDevice IoDevice { get; }

        /// <summary>
        /// 綁定實體設備 
        /// </summary>
        /// <remarks>
        /// 可於 IoMemory 建立之後, 再事後綁定.
        /// </remarks>
        void BindDevice(IoDevice device);
    }
}

