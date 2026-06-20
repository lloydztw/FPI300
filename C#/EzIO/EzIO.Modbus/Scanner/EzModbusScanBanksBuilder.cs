#region AUTHOR
/*
 * EzIO.Modbus.AutoScan
 * 
 * Copyright (C) 2026
 * 
 * 2026-04-21 LeTian Chang: Integration with EzIO
 * 
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzIO.Mem;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EzIO.Modbus.AutoScan
{
    public class EzModbusScanBanksBuilder
    {
        #region NLOG
        //NLog.Logger _LOG => EzLog.LOG;
        #endregion

        /// <summary>
        /// 組建成 專用於 通訊指令 的 區塊組 <br/>
        /// 位址跳號數 大於 gap 就會自動拆分成不同通訊區塊.
        /// </summary>
        public List<IoMemoryBank> BuildBanks(IoMemory host, IEnumerable<IoPoint> ioPoints, int gap = 0)
        {
            var totalBanks = new List<IoMemoryBank>();

            // 挑出不為 null 的 ioPoints.
            // 並用 ModbusCategory 分類之.
            var validPoints = ioPoints.Where(p => p != null);
            var groups = validPoints.GroupBy(p => p.Address.GetModbusCategory());

            foreach (var grp in groups)
            {
                var banks = BuildModbusBanks(host, grp, gap);
                if (banks != null && banks.Count > 0)
                    totalBanks.AddRange(banks);
            }

            return totalBanks;
        }

        /// <summary>
        /// 組建成 專用於 通訊指令 的 區塊組 <br/>
        /// 位址跳號數 大於 gap 就會自動拆分成不同通訊區塊.
        /// </summary>
        List<IoMemoryBank> BuildModbusBanks(IoMemory host, IEnumerable<IoPoint> ioPoints, int gap = 0)
        {
            var resultBanks = new List<IoMemoryBank>();
            if (ioPoints == null || !ioPoints.Any())
                return resultBanks;

            // 目前連續數量暫時無上限
            int maxContiCount = int.MaxValue;

            // 可允許跳號數
            if (gap <= 0) gap = 16;
            //gap = Math.Min(gap, 128);

            // 挑出不為 null 的 points, 並用 CateID 分類之.
            var validPoints = ioPoints.Where(p => p != null);
            var groups = validPoints.GroupBy(p => p.Address.CateID);

            foreach (var group in groups)
            {
                // 以位址號排序
                var sortedPoints = group.OrderBy(p => p.Address.Address).ToList();
                if (!sortedPoints.Any())
                    continue;

                var moCate = sortedPoints[0].Address.GetModbusCategory();

                int i = 0;
                while (i < sortedPoints.Count)
                {
                    var startPoint = sortedPoints[i];
                    var startAddr = startPoint.Address;

                    var lastAddr = startAddr;
                    int iNext = i;
                    while (iNext < sortedPoints.Count)
                    {
                        var currentReg = sortedPoints[iNext];
                        var currentAddr = currentReg.Address;

                        // 計算間距
                        int currentGap = IoAddress.PointsDiff(currentAddr, lastAddr);
                        int count = IoAddress.PointsDiff(currentAddr, startAddr);

                        // 判斷是否併入 Bank
                        if (currentGap <= gap && count <= maxContiCount)
                        {
                            lastAddr = currentAddr;
                            iNext++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    // 建立 Bank 實例
                    int finalSpan = IoAddress.PointsDiff(lastAddr, startAddr) + 1;
                    var bank = host.GetBank(startAddr, finalSpan);

                    // 自動填充
                    for (int k = i; k < iNext; k++)
                    {
                        var pt = sortedPoints[k];

                        int bankIndex;

                        if (moCate == ModbusCateEnum.HOLDING_REGISTER)
                        {
                            // 根據 IoMemoryBank.smartGetReg 的設計，這裡要傳入「絕對位址」
                            // 例如 pt.Address.Address 是 102
                            bankIndex = pt.Address.Address;
                        }
                        else
                        {
                            // 根據 IoMemoryBank.smartGetBitPoint 的設計，這裡要傳入「相對偏移 (Bit 差值)」
                            // 使用 IoAddress.BitsDiff 邏輯或直接計算地址差
                            bankIndex = IoAddress.BitsDiff(pt.Address, bank.AddressBase);
                        }

                        // 這會觸發 IoMemoryBank 內部的 smartGetReg 或 smartGetBitPoint
                        // 進而呼叫 _host.GetPoint 並完成點位與 Bank 陣列的掛載
                        var dummy = bank[bankIndex];
                    }

                    resultBanks.Add(bank);
                    i = iNext;
                }
            }

            return resultBanks;
        }
    }
}
