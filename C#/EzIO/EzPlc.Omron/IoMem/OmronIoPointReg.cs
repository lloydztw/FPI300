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

namespace EzPlc.Omron
{
    public class OmronIoPointReg : IoDevPointReg
    {
        public OmronIoPointReg(OmronAddress address) : base(address)
        {
        }

        protected override IoPointBit CreatePointBit(int bit)
        {
            // 請直接使用 new HcfaIoPointBit !!!
            throw new System.Exception("請直接使用 new OmronIoPointBit() !");
            return null;
        }

        public override uint Read(IoPriority priority = IoPriority.Normal)
        {
            // ORMON 暫時都 "直接通訊" 讀取
            if (priority != IoPriority.InternalUpdate)
                priority = IoPriority.Directly;
            return base.Read(priority);
        }
    }
}