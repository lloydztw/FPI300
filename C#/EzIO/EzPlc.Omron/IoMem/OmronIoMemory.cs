#region AUTHOR
/*
 * EzPlc.Omron
 * 
 * Copyright (C) 2026
 * 
 * 2026-04-24 LeTian Chang : Integration with EzIO
 * 
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzIO.Mem;
using System;
using System.Collections.Generic;

namespace EzPlc.Omron
{
    public class OmronIoMemory : IoMemory
    {
        public OmronIoMemory()
        {
        }

        public override IoPoint GetPoint(string ezIoName)
        {
            // 全註冊
            var addr = new OmronAddress(ezIoName);
            var ioPoint = GetOrAddPoint(addr.KeyName, (k) =>
            {
                switch((OmronCateEnum)addr.CateID)
                {
                    case OmronCateEnum.BOOL:
                        return new OmronIoVarBool(addr);
                    case OmronCateEnum.INT32:
                        return new OmronIoVarInt32(addr);
                    case OmronCateEnum.FLOAT32:
                        return new OmronIoVarFloat32(addr);
                    case OmronCateEnum.STR:
                    default:
                        return new OmronIoVarStr(addr);
                }
            });

            // 自動綁定硬體設備
            var device = base.IoDevice;
            if (device != null)
                ioPoint.BindDevice(device);

            return ioPoint;
        }

        public override IoMemoryBank GetBank(IAddress baseAddr, int span)
        {
            // 暫時不支援
            // return base.GetBank(baseAddr, span);
            return null;
        }

        /// <summary>
        /// 為所有 ORMON 變量, 生成虛擬位址編號.
        /// </summary>
        public void GenerateVirualAddresses(IEnumerable<IoPoint> points = null, int virtualBaseAddress = 0)
        {
            if (points != null)
            {
                var lst = new List<IoPoint>(points);

                // 先用 CateID 排序
                lst.Sort((p1, p2) => p1.Address.CateID - p2.Address.CateID);

                int virtualAddress = virtualBaseAddress;
                foreach (var p in lst)
                {
                    if (p.Address is OmronAddress omronAddress)
                        omronAddress.Address = (ushort)virtualAddress;
                    virtualAddress++;
                }
            }
            else
            {
                foreach (OmronCateEnum cate in Enum.GetValues(typeof(OmronCateEnum)))
                {
                    var ioPoints = GetCatePoints((int)cate);
                    int virtualAddress = ((int)cate) * 1000;

                    for (int i = 0, N = ioPoints.Count; i < N; i++)
                    {
                        if (ioPoints[i].Address is OmronAddress addr)
                        {
                            addr.Address = (ushort)(virtualBaseAddress + i);
                        }
                    }
                }
            }
        }

        protected override int CompareOrder(IoPoint p1, IoPoint p2)
        {
            var a1 = (int)p1.Address.Address;
            var a2 = (int)p2.Address.Address;
            return a1 - a2;
        }
    }
}

