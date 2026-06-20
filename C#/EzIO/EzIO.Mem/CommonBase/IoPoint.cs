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

using EzIO.Mem.Utils;
using System;
using System.ComponentModel;

namespace EzIO.Mem
{
    /// <summary>
    /// 泛型 IO 點位 (可以是 1-Bit 點位, 16-bit or 32-bit 暫存器)
    /// </summary>
    public abstract class IoPoint
    {
        #region CONFIG
        public static bool OPT_WRITE_THROUGH => false;
        #endregion

        public abstract IAddress Address 
        { 
            get; 
        }
        public virtual int Bits
        {
            get => Address != null ? Address.Bits : 1;
        }

        /// <summary>
        /// 是否為反向 (常閉)
        /// </summary>
        public virtual bool Inverted
        {
            get;
            private set;
        }
        /// <summary>
        /// 1-Bit 單點 IO 讀值 (Read)
        /// <br/> 讀取快取內存(cache)之數據是否為On. reads data from the cache memory.
        /// </summary>
        public bool IsOn
        {
            get => (Inverted ? Data == 0 : Data != 0) && IsValid;
        }
        /// <summary>
        /// 1-Bit 單點 IO 讀值 (Read)
        /// <br/> 讀取快取內存(cache)之數據是否為Off. reads data from the cache memory.
        /// </summary>
        public bool IsOff
        {
            get => (Inverted ? Data != 0 : Data == 0) && IsValid;
        }
        /// <summary>
        /// 32-bit 暫存器 讀寫
        /// <br/>也可套用於 16-bit (caller 必須搭配 EzConvert 進行位元長度轉換)
        /// <br/>Read: 從快取內存讀取資數據. reads data from the cache memory.
        /// <br/>Write: 只寫入硬件設備(PLC), 其對映的 快取內存數據 不會被更動. 
        /// writes data to the hardware 
        /// (the cache will NOT be changed immediately!)
        /// </summary>
        public uint Data
        {
            get => Read();
            set => Write(value);
        }

        #region 根據型別來設定數值
        public void Set(bool on, IoPriority p = IoPriority.Normal)
        {
            if (Inverted) on = !on;
            Write(on ? 1u : 0u, p);
        }
        public void Set(UInt32 data, IoPriority p = IoPriority.Normal)
        {
            Write(data, p);
        }
        public void Set(Int32 data, IoPriority p = IoPriority.Normal)
        {
            Set(EzConvert.U32(data), p);
        }
        public void Set(UInt16 data, IoPriority p = IoPriority.Normal)
        {
            Set(EzConvert.U32(data), p);
        }
        public void Set(Int16 data, IoPriority p = IoPriority.Normal)
        {
            Set(EzConvert.U32(data), p);
        }
        #endregion

        #region 數據型別轉換運算子
        public static implicit operator UInt32(IoPoint ioPoint)
        {
            return ioPoint != null ? ioPoint.Data : 0;
        }
        public static implicit operator Int32(IoPoint ioPoint)
        {
            return ioPoint != null ? (Int32)ioPoint.Data : 0;
        }
        public static implicit operator UInt16(IoPoint ioPoint)
        {
            return ioPoint != null ? EzConvert.U16(ioPoint.Data) : (UInt16)0;
        }
        public static implicit operator Int16(IoPoint ioPoint)
        {
            return ioPoint != null ? EzConvert.I16(ioPoint.Data) : (Int16)0;
        }
        public static implicit operator bool(IoPoint ioPoint)
        {
            return ioPoint != null && ioPoint.IsOn;
        }
        #endregion

        #region 預留之進階設計
        /// <summary>
        /// 目前的數據是否過時, 是否需要等待下一次 scan 被更新
        /// </summary>
        public virtual bool IsValid => true;
        /// <summary>
        /// 將 目前的 快取內存數據 標記為 已經老舊過時
        /// </summary>
        public virtual void Invalidate()
        {
        }
        #endregion

        /// <summary>
        /// 繼承者必須實作
        /// </summary>
        public abstract void Write(uint data, IoPriority priority = IoPriority.Normal);

        /// <summary>
        /// 繼承者必須實作
        /// </summary>
        public abstract uint Read(IoPriority priority = IoPriority.Normal);

        #region 字串相關函式
        public string KeyName
        {
            get
            {
                return Address != null ? Address.KeyName : GetType().Name;
            }
        }
        public string Description
        {
            get
            {
                return Address != null ? Address.Description : "";
            }
        }
        public IoPoint SetDescription(string description)
        {
            if (Address != null) Address.Description = description;
            return this;
        }
        public override string ToString()
        {
            var desc = Description;
            return string.IsNullOrEmpty(desc) ? KeyName : $"{KeyName} ({desc})";
        }
        #endregion

        #region MISC_FUNCTIONS
        /// <summary>
        /// 將 1-bit 型態的點位, 設定為 反向(常閉) 或 正向(常開) 
        /// </summary>
        public IoPoint SetInverted(bool inverted = true)
        {
            Inverted = inverted;
            return this;
        }
        #endregion

        #region HOST_POINT
        /// <summary>
        /// 宿駐暫存器 (用於階層狀結構的點位管理, 例如永宏)
        /// </summary>
        public virtual IoPoint Host
        {
            get => null;
        }
        #endregion

        #region DEVICE_BINDING
        /// <summary>
        /// 綁定實體裝置
        /// </summary>
        public virtual void BindDevice(IoDevice device)
        {
        }
        #endregion
    }
}
