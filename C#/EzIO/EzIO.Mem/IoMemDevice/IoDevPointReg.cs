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


namespace EzIO.Mem
{
    /// <summary>
    /// 具備實體設備連動功能的 IO 暫存器點位 (Register Device Proxy)。
    /// <br/> 繼承自 <see cref="IoPointReg"/>，提供實體設備與快取內存之間的數據同步。
    /// <list type="bullet">
    /// <item>【讀取】支援 <see cref="IoPriority.Directly"/> 模式，可跳過快取直接從實體設備讀取並同步至內存。</item>
    /// <item>【寫入】採用 Write-Through 策略，確保數據同時寫入實體設備與內存快取，維持數據一致性。</item>
    /// <item>【模擬】支援模擬模式，當未綁定設備或處於模擬狀態時，退化為純內存操作。</item>
    /// </list>
    /// </summary>
    public class IoDevPointReg : IoPointReg
    {
        #region PRIVATE_DATA
        protected IoDevice _device;
        #endregion

        public IoDevPointReg(IAddress address) : base(address)
        {
        }

        internal IoDevice Device
        {
            get => _device;
        }
        public override void BindDevice(IoDevice device)
        {
            _device = device;
        }
        protected override IoPointBit CreatePointBit(int bit)
        {
            // 建立一個具備設備通訊能力的 Bit 點位
            var bitPoint = new IoDevPointBit(null, this, bit);
            return bitPoint;
        }

        /// <summary>
        /// 如果有聯結 device 並且 IoPriority.Directly, 才會直接 從 device 讀取數據.
        /// 否則只從 cache 讀取數據.
        /// </summary>
        public override uint Read(IoPriority priority = IoPriority.Normal)
        {
            // NOTE: IoPriority.Directly 模式, 每次讀取會阻塞當下的 thread.
            if (_device != null && priority == IoPriority.Directly && !_device.IsSim)
            {
                // 從 device 讀取數據
                var data = _device.ReadReg(Address);
                // 更新 cache
                base.Write(data, IoPriority.InternalUpdate);
                // 返回 data
                return data;
            }
            else
            {
                return base.Read(priority);
            }
        }

        /// <summary>
        /// 如果有聯結 device, 使用 Write-Through 策略寫入 device 與 cache.
        /// 如果沒有 device, 則直接寫入 cache.
        /// </summary>
        public override void Write(uint data, IoPriority priority = IoPriority.Normal)
        {
            if (_device != null && priority != IoPriority.InternalUpdate)
            {
                // 如果有聯結 device, 寫入 device.
                _device.WriteReg(Address, data);

                if (OPT_WRITE_THROUGH || _device.IsSim)
                {
                    // 使用 Write-Through 策略寫入 cache.
                    base.Write(data, priority);
                }
            }
            else
            {
                // 如果沒有 device, 則直接寫入 cache.
                base.Write(data, priority);
            }
        }
    }
}
