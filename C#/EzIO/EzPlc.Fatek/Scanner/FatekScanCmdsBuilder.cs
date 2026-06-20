#region AUTHOR
/*
 * EzPlc.Fatek
 * 
 * Copyright (C) 2026
 * 
 * 2026-04-05 LeTian Chang: Integration with EzIO
 * 2022-09-16 LeTian Chang: 重整
 * 2009-11-30 LeTian Chang: Creation.
 * 
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzIO.Mem;
using EzPlc.Fatek.Comm;
using System;
using System.Collections.Generic;
using System.Linq;


namespace EzPlc.Fatek.AutoScan
{
    public class FatekScanCmdsBuilder
    {
        #region NLOG
        NLog.Logger _LOG => EzLog.LOG;
        #endregion

        /// <summary>
        /// 組建成 專用於 通訊指令 的 區塊組 <br/>
        /// 位址跳號數 大於 gap 就會自動拆分成不同通訊區塊.
        /// </summary>
        public FkCmd[] BuildCmds(IoDevice device, IEnumerable<IoPoint> ioRegs, int gap = 0)
        {
            var ioMem = device.GetFatekIoMemory();
            var banks = BuildBanks(ioMem, ioRegs, gap);

            var fkCmds = new List<FkCmd>();

            if (true)
            {
                // 連續位址讀取 (每一條指令 大約消耗 8 ms)
                foreach (var bank in banks)
                {
                    int N = bank.Span;

                    var addr = bank.AddressBase.ToFatekAddr();

                    var fkCmd = new FkCmd_ReadContiRegisters(
                                    addr, N,
                                    bank.UpdateCacheByIndex,
                                    addr.StationID);

                    fkCmds.Add(fkCmd);
                }
            }
            else
            {
#if (OPT_RESERVED)
                // 混合點位讀取
                var addrs = new List<IFkAddress>();
                var regs = new List<IoPointReg>();
                foreach(var bank in banks)
                {
                    foreach (var reg in bank)
                    {
                        if (reg != null && reg.Address != null)
                        {
                            addrs.Add(reg.Address.ToFatekAddr());
                            regs.Add(reg);
                        }
                    }
                }
                
                int stationID = regs[0].Address.StationID;
                var fkCmd = new FkCmd_ReadMixRegisters(addrs.ToArray(), (a, i, value) =>
                {
                    regs[i].Write(value, IoPriority.InternalUpdate);
                }, 
                stationID);

                fkCmds.Add(fkCmd);
#endif
            }

            return fkCmds.ToArray();
        }
        
        /// <summary>
        /// 組建成 專用於 通訊指令 的 區塊組 <br/>
        /// 位址跳號數 大於 gap 就會自動拆分成不同通訊區塊.
        /// </summary>
        public IoMemoryBank[] BuildBanks(IoMemory host, IEnumerable<IoPointReg> ioRegs, int gap = 0)
        {
            if (gap <= 0) gap = 16;
            gap = Math.Min(gap, 128);

            if (ioRegs == null || !ioRegs.Any())
                return Array.Empty<IoMemoryBank>();

            var resultBanks = new List<IoMemoryBank>();

            // 挑出不為 null 的 regs, 並用 CateID 分類之..
            var validRegs = ioRegs.Where(p => p != null);
            var groups = validRegs.GroupBy(p => p.Address.CateID);

            foreach (var group in groups)
            {
                var sortedRegs = group.OrderBy(p => p.Address.Address).ToList();
                if (!sortedRegs.Any())
                    continue;

                var firstReg = sortedRegs[0];
                //FatekAddr.ParseCategory(firstReg.Address.Category, out var bits, out var digits, out var alignment, out var maxAddr);
                var fkCategory = (FatekCateEnum)firstReg.Address.CateID;
                FatekAddr.ParseCategory(fkCategory, out var bits, out var stride, out var digits, out var alignment, out var maxAddr);

                // Fatek 允取 連續讀取的最大 站存器數量
                int maxContiCount = bits == IoBitsEnum.Bits_16 ? 0x40 : 0x20;

                int i = 0;
                while (i < sortedRegs.Count)
                {
                    var startReg = sortedRegs[i];
                    int startAddrN = startReg.Address.Address;

                    // 地址對齊檢查
                    if (startAddrN % alignment != 0)
                    {
                        throw new Exception($"位址 {startAddrN} 必須對齊 {alignment} 的倍數!");
                    }

                    int lastAddrN = startAddrN;
                    int iNext = i;
                    while (iNext < sortedRegs.Count)
                    {
                        var currentReg = sortedRegs[iNext];
                        int currentAddrN = currentReg.Address.Address;

                        // 計算間距
                        int currentGap = (currentAddrN - lastAddrN) / alignment;
                        int count = ((currentAddrN - startAddrN) / alignment) + 1;

                        // 判斷是否併入 Bank
                        if (currentGap <= gap && count <= maxContiCount)
                        {
                            lastAddrN = currentAddrN;
                            iNext++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    // 建立 Bank 實例
                    int finalSpan = ((lastAddrN - startAddrN) / alignment) + 1;
                    var baseAddr = startReg.Address.JumpTo(startAddrN);
                    var bank = host.GetBank(baseAddr, finalSpan);
                    //var bankTest = new IoMemoryBank(host, baseAddr, finalSpan, alignment);

                    //----------------------------------------------------------------
                    // 自動填充
                    for (int k = i; k < iNext; k++)
                    {
                        var dummy = bank[sortedRegs[k].Address.Address];
                    }
                    //----------------------------------------------------------------

                    resultBanks.Add(bank);
                    i = iNext;
                }
            }

            return resultBanks.ToArray();
        }

        /// <summary>
        /// 組建成 專用於 通訊指令 的 區塊組 <br/>
        /// 位址跳號數 大於 gap 就會自動拆分成不同通訊區塊.
        /// </summary>
        public IoMemoryBank[] BuildBanks(IoMemory host, IEnumerable<IoPoint> ioPoints, int gap = 0)
        {
            // OfType 會自動剔除 null 並確保型別符合
            var ioRegs = ioPoints.OfType<IoPointReg>();
            return BuildBanks(host, ioRegs, gap);
        }
    }
}
