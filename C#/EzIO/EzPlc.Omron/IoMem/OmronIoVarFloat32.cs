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
using System.Threading;

namespace EzPlc.Omron
{
    public class OmronIoVarFloat32 : OmronIoVar
    {
        #region PRIVATE_DATA
        // 內部使用 int/uint 快取以支援 Lock-free 原理性
        private int _cacheI32;
        #endregion

        public OmronIoVarFloat32(OmronAddress address) : base(address)
        {
        }

        #region OVERRIDES

        public override uint Read(IoPriority priority = IoPriority.Normal)
        {
            if (_omron != null && priority == IoPriority.Directly && !_omron.IsSim)
            {
                var v = base.ReadVar(_address, priority);
                float f = (float)Convert.ToDouble(v);
                int i32 = BitConverter.ToInt32(BitConverter.GetBytes(f), 0);

                Thread.VolatileWrite(ref _cacheI32, i32);
                return (uint)i32;
            }
            return (uint)Thread.VolatileRead(ref _cacheI32);
        }

        public override void Write(uint data, IoPriority priority = IoPriority.Normal)
        {
            if (_omron != null && priority != IoPriority.InternalUpdate)
            {
                float f = BitConverter.ToSingle(BitConverter.GetBytes(data), 0);
                base.WriteVar(_address, f, priority);

                if (OPT_WRITE_THROUGH)
                {
                    Thread.VolatileWrite(ref _cacheI32, (int)data);
                }
            }
            else
            {
                Thread.VolatileWrite(ref _cacheI32, (int)data);
            }
        }

        #endregion

        public float Value
        {
            get => BitConverter.ToSingle(BitConverter.GetBytes(Read()), 0);
            set => Write(BitConverter.ToUInt32(BitConverter.GetBytes(value), 0));
        }
        
        public void Set(float value)
        {
            Value = value;
        }
    }
}