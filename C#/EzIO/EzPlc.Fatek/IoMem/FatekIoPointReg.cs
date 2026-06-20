#region AUTHOR
/*
 * EzPlc.Fatek
 * 
 * Copyright (C) 2026
 * 2026-04-05 revised by LeTian Chang
 * 2013-07-11 created by LeTian Chang
 * 
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzIO.Mem;

namespace EzPlc.Fatek
{
    public class FatekIoPointReg : IoDevPointReg
    {
        public FatekIoPointReg(FatekAddr address) : base(address)
        {
        }

        protected override IoPointBit CreatePointBit(int bit)
        {
            //string cate = this.Address?.Category?.ToUpper();
            //FatekAddr.ParseCategory(cate, out var bits, out var digits, out var align, out var maxAddr);
            var fkCategory = (FatekCateEnum)Address.CateID;
            FatekAddr.ParseCategory(fkCategory, out var eBits, out var stride, out var digits, out var align, out var maxAddr);

            int bits = 1 << (int)eBits;
            if (bits >= 16)
            {
                int addrNumber = this.Address.Address;

                System.Diagnostics.Trace.Assert(addrNumber % align == 0, $"addrNumber={addrNumber} 必須是 {align} 的倍數");
                System.Diagnostics.Trace.Assert(bit < bits, $"bit={bit} 必須是 0 ~ {bits - 1}");

                string cateStr = fkCategory.ToString();
                string cateStrB = bits == 32 ? cateStr.Substring(2) : cateStr.Substring(1);
                var stationID = Address.StationID;
                var pointAddr = new FatekAddr($"{stationID}:{cateStrB}{addrNumber + bit}");
                var point = new FatekIoPointBit(this, bit, pointAddr);

                return point;
            }
            else
            {
                return base.CreatePointBit(bit);
            }
        }
    }
}