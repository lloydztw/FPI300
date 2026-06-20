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
    /// 具備實體設備連動功能的 IO 位元點位 (Bit Device Proxy)。
    /// <br/> 專用於處理 1-bit 訊號（如 X, Y, M 點）的通訊與極性轉換。
    /// <list type="bullet">
    /// <item>【通訊】透過綁定的 <see cref="IoDevice"/> 執行 <c>ReadBit</c> 與 <c>WriteBit</c> 操作。</item>
    /// <item>【極性】自動處理 <see cref="IoPoint.Inverted"/> 邏輯，確保軟體邏輯值與硬體物理電平正確映射。</item>
    /// <item>【同步】在直接讀取模式下，會自動將讀取結果同步回其所屬的 Host 暫存器快取中。</item>
    /// </list>
    /// </summary>
    public class IoDevPointBit : IoPointBit
    {
        #region PRIVATE_DATA
        private IoDevice _device;
        #endregion

        #region 內部建構子
        internal protected IoDevPointBit(IAddress originalAddress, IoPointReg host, int bitOffset) 
                            : base(originalAddress, host, bitOffset)
        {
            _device = (Host as IoDevPointReg)?.Device;
        }
        internal protected IoDevPointBit(IAddress originalAddress)
                            : base(originalAddress, null, 0)
        {
        }
        #endregion

        internal IoDevice Device
        {
            get => _device;
        }

        public override void BindDevice(IoDevice device)
        {
            _device = device;
        }

        /// <summary>
        /// 如果有聯結 device 加上 IoPriority.Directly 模式, 才會直接 從 device 讀取數據.
        /// 否則只從 cache 讀取數據.
        /// </summary>
        public override uint Read(IoPriority priority = IoPriority.Normal)
        {
            // NOTE: IoPriority.Directly 模式, 每次讀取會阻塞當下的 thread.
            if (_device != null && priority == IoPriority.Directly && !_device.IsSim)
            {
                // 從 device 讀取數據
                bool on = _device.ReadBit(Address);

                // on-off 轉換成 data
                if (Inverted) on = !on;
                uint data = on ? 1u : 0u;

                // 更新 cache
                base.Write(data, IoPriority.Normal);

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
            // 如果有聯結 device.
            if (_device != null && priority != IoPriority.InternalUpdate)
            {
                bool on = Inverted ? data == 0 : data != 0;
                // 如果有聯結 device, 寫入 device.
                _device.WriteBit(Address, on);

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
