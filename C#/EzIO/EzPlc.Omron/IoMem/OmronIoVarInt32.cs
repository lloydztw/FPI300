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
    public class OmronIoVarInt32 : OmronIoVar
    {
        #region PRIVATE_CACHE_DATA
        private int _cacheI32;
        #endregion

        public OmronIoVarInt32(OmronAddress address) : base(address)
        {
        }

        #region OVERRIDES
        /// <summary>
        /// 如果有聯結 device 加上 IoPriority.Directly 模式, 才會直接 從 device 讀取數據.
        /// 否則只從 cache 讀取數據.
        /// </summary>
        public override uint Read(IoPriority priority = IoPriority.Normal)
        {
            // NOTE: IoPriority.Directly 模式, 每次讀取會阻塞當下的 thread.
            if (_omron != null && priority == IoPriority.Directly && !_omron.IsSim)
            {
                var value = base.ReadVar(_address, priority);
                var i32 = Convert.ToInt32(value);
                Thread.VolatileWrite(ref _cacheI32, i32);
                return (uint)i32;
            }
            else
            {
                return (uint)Thread.VolatileRead(ref _cacheI32);
            }
        }

        /// <summary>
        /// 如果有聯結 device, 使用 Write-Through 策略寫入 device 與 cache.
        /// 如果沒有 device, 則直接寫入 cache.
        /// </summary>
        public override void Write(uint data, IoPriority priority = IoPriority.Normal)
        {
            // 如果有聯結 device.
            if (_omron != null && priority != IoPriority.InternalUpdate)
            {
                base.WriteVar(_address, (int)data, priority);

                if (OPT_WRITE_THROUGH)
                {
                    // 使用 Write-Through 策略寫入 cache.
                    Thread.VolatileWrite(ref _cacheI32, (int)data);
                }
            }
            else
            {
                // 如果沒有 device, 則直接寫入 cache.
                Thread.VolatileWrite(ref _cacheI32, (int)data);
            }
        }
        #endregion

        public int Value
        {
            get => (int)Read();
            set => Write((uint)value);
        }

        public void Set(int value)
        {
            Value = value;
        }
    }
}