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
using OMRON.Compolet.CIPCompolet64;

namespace EzPlc.Omron
{
    public abstract class OmronIoVar : IoPoint
    {
        #region PROTECTED_DATA
        protected IAddress _address;
        protected object _cache;
        #endregion

        #region PROTECTED_DEVICE_MEMBER
        protected OmronIoDevice _omron;
        protected NJCompolet _compolet => _omron?.Compolet;
        #endregion

        public OmronIoVar(IAddress address)
        {
            OptAlwaysDirectlyRead = true;
            _address = address;
        }
        public override IAddress Address
        {
            get => _address;
        }
        public override void BindDevice(IoDevice device)
        {
            _omron = device as OmronIoDevice;
        }

        #region IoPoint_UINT
        public override uint Read(IoPriority priority = IoPriority.Normal)
        {
            // DUMMY
            return 0;
        }
        public override void Write(uint data, IoPriority priority = IoPriority.Normal)
        {
            // DUMMY
        }
        #endregion

        /// <summary>
        /// 是否使用 "立即通訊" 的方式 來讀取 點位數據 (目前默認為 true)
        /// </summary>
        public bool OptAlwaysDirectlyRead
        {
            get;
            set;
        }

        public virtual object ReadVar(IAddress addr, IoPriority priority = IoPriority.Normal)
        {
            // 目前 OMRON 默認使用 "立即通訊" 讀取

            if (_compolet != null && !_omron.IsSim && (OptAlwaysDirectlyRead || priority == IoPriority.Directly))
            {
                // 從 device 讀取數據 (每次讀取會阻塞當下的 thread.)
                var data = _compolet.ReadVariable(_address.ToCommString());

                // Lock-free 寫入 cache
                System.Threading.Volatile.Write(ref _cache, data);
                return data;
            }
            else
            {
                // Lock-free 讀取 cache
                return System.Threading.Volatile.Read(ref _cache);
            }
        }

        public virtual void WriteVar(IAddress addr, object value, IoPriority priority = IoPriority.Normal)
        {
            if (_compolet != null && priority != IoPriority.InternalUpdate)
            {
                // 如果有聯結 device, 寫入 device.
                _compolet.WriteVariable(addr.ToCommString(), value);

                if (OPT_WRITE_THROUGH)
                {
                    // Lock-free 寫入 cache
                    System.Threading.Volatile.Write(ref _cache, value);
                }
            }
            else
            {
                // 如果沒有 device, Lock-free 寫入 cache
                System.Threading.Volatile.Write(ref _cache, value);
            }
        }
    }
}