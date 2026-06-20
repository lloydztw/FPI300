#region AUTHOR
/*
 * EzPlc.Hcfa
 * 
 * Copyright (C) 2026
 * 
 * 2026-04-05 LeTian Chang : Integration with EzIO
 * 
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzIO.Mem;
using EzIO.Mem.Utils;
using System;

namespace EzPlc.Hcfa
{
    /// <summary>
    /// HCFA (禾川) PLC 專用定址實作
    /// </summary>
    public class HcfaAddress : IoAddress
    {
        #region PRIVATE_DATA
        string _modbusAddressCacheStr;
        #endregion

        #region 建構子
        /// <summary>
        /// ezIoName 格式: $"{StationID}:{Category}{Address}.{BitOffset}"
        /// </summary>
        public HcfaAddress(string ezIoName, string description = null)
        {
            // 使用通用解析器取得基本資訊
            EzAddressParser.Parse(ezIoName, out int stationID, out HcfaCateEnum cate, out int address, out int bitOffset);

            // 更新 IoAddress 的內部屬性 (I)
            this.StationID = (byte)stationID;
            this.CateID = (int)cate;
            this.Address = (ushort)address;
            this.Description = description;

            // 根據 HCFA 特性初始化 Bits, Stride 等資訊
            bitOffset = NormalizeBitsInfo(cate, out var bits, out int stride, bitOffset);
            if (bitOffset < 0)
                throw new Exception($"格式有誤 @ {ezIoName}");

            // 更新 IoAddress 的內部屬性 (II)
            this.BitsEnum = bits;
            this.BitStride = (byte)stride;
            this.BitOffset = (byte)bitOffset;
        }

        public HcfaAddress(IAddress src)
        {
            CopyFrom(src);
        }

        protected HcfaAddress() : base() { }

        public override object Clone()
        {
            var clone = new HcfaAddress();
            clone.CopyFrom(this);
            return clone;
        }
        #endregion

        #region IAddress 實作與複寫 (最小站號)
        protected override byte MinStationID => 1;
        #endregion

        #region IAddress 實作與複寫 (Category)
        protected override Enum CateEnum => (HcfaCateEnum)CateID;
        public HcfaCateEnum HcfaCategory
        {
            get => (HcfaCateEnum)CateID;
            private set => CateID = Convert.ToInt32(value);
        }
        #endregion

        #region IAddress 實作與複寫 (KeyName 與 通訊字串)
        public override string KeyName
        {
            get
            {
                // 如果 StationID 為 MinStationID 時, 省略前綴.
                string prefix = StationID <= MinStationID ? "" : $"{StationID}:";
                var modCate = GetModbusCategory();
                if (modCate == ModbusCateEnum.DISCRETE_INPUT || modCate == ModbusCateEnum.COIL)
                {
                    // 強制顯示 點號偏移
                    return $"{prefix}{HcfaCategory}{Address}.{BitOffset}";
                }
                else
                {
                    // 16-bit 或 32-bit 不顯示 點號偏移
                    return $"{prefix}{HcfaCategory}{Address}";
                }
            }
        }
        public override string ToCommString()
        {
            if (string.IsNullOrEmpty(_modbusAddressCacheStr))
                _modbusAddressCacheStr = ToNModbus4String();
            return _modbusAddressCacheStr;
        }
        #endregion

        #region 邏輯規格化
        protected override int NormalizeBitsInfo(Enum cate, out IoBitsEnum bits, out int stride, int offset = 0)
        {
            return ParseCategory((HcfaCateEnum)cate, out bits, out stride, offset);
        }
        #endregion

        #region STRING_CONVERT_FUNCTIONS_字串格式轉換
        /// <summary>
        /// 嘗試由 ezIoName 生成 HcfaAddress
        /// 通用格式: $"{StationID}:{Category}{Address}.{BitOffset}"
        /// </summary>
        public static HcfaAddress TryParse(string ezIoName)
        {
            try
            {
                var addr = new HcfaAddress(ezIoName);
                return addr;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 根據 HCFA 類別解析並規格化位元資訊
        /// </summary>
        /// <returns>如果成功返回 bitOffset, 否則返回 -1</returns>
        public static int ParseCategory(HcfaCateEnum cate, out IoBitsEnum bits, out int stride, int bitOffset)
        {
            switch (cate)
            {
                // 1-bit 類型 (8進制: X, Y)
                case HcfaCateEnum.IX:
                case HcfaCateEnum.QX:
                    bits = IoBitsEnum.Bits_1;
                    stride = 8;
                    break;

                //// 1-bit 類型 (10進制)
                //case HcfaCateEnum.MX:
                //    bits = BitsEnum.Bits_1;
                //    stride = 10;
                //    break;

                // 8-bit Byte 類型 (8進制)
                //case HcfaCateEnum.IB:
                case HcfaCateEnum.QB:
                    bits = IoBitsEnum.Bits_8;
                    stride = 8;
                    //bitOffset = 0;        //允許保留 bitOffset
                    break;

                //// 8-bit Byte 類型 (10進制)
                //case HcfaCateEnum.MB:
                //    bits = BitsEnum.Bits_8;
                //    stride = 10;
                //    //bitOffset = 0;    //允許保留 bitOffset
                //    break;

                //// 16-bit Word 類型 (8進制)
                //case HcfaCateEnum.IW:
                //case HcfaCateEnum.QW:
                //    bits = BitsEnum.Bits_16;
                //    stride = 8;
                //    //bitOffset = 0;
                //    break;

                // 16-bit Word 類型 (10進制)
                case HcfaCateEnum.MW:
                case HcfaCateEnum.D:
                    bits = IoBitsEnum.Bits_16;
                    stride = 1;
                    //bitOffset = 0;    //允許保留 bitOffset
                    break;

#if(OPT_RESERVED_FOR_32_BIT)
                // 32-bit DWord 類型 (10進制)
                case HcfaCateEnum.ID:
                case HcfaCateEnum.QD:
                    bits = BitsEnum.Bits_32;
                    stride = 8;
                    //bitOffset = 0;
                    break;

                // 32-bit DWord 類型 (10進制)
                case HcfaCateEnum.MD:
                case HcfaCateEnum.DD:
                    bits = BitsEnum.Bits_32;
                    stride = 2;         // 佔用兩個 Word
                    //bitOffset = 0;    //允許保留 bitOffset
                    break;
#endif

                default:
                    bits = IoBitsEnum.Bits_16;
                    stride = 1;
                    throw new Exception($"目前不支援 Category={cate} !");
                    return -1;
            }

            // 這裡不直接呼叫 CheckBoundary，因為它是實體方法
            // 但我們可以在這裡做基本的數值鉗制
            bitOffset = Math.Max(0, Math.Min(bitOffset, 31));
            stride = Math.Max(1, Math.Min(stride, 63));

            return bitOffset;
        }
        #endregion

        #region MODBUS_位址轉換
        public override ModbusCateEnum GetModbusCategory()
        {
            switch (this.HcfaCategory)
            {
                case HcfaCateEnum.IX:
                    return ModbusCateEnum.DISCRETE_INPUT;

                case HcfaCateEnum.QX:
                case HcfaCateEnum.QB:
                    return ModbusCateEnum.COIL;

                case HcfaCateEnum.MW:
                case HcfaCateEnum.D:
                    return ModbusCateEnum.HOLDING_REGISTER;

                default:
                    return base.GetModbusCategory();
            }
        }
#if(OPT_RESERVED)
        string ToModbusString_Standard()
        {
            int modbusAddr;
            string prefix;

            var cate = (HcfaCateEnum)this.CategoryInt;

            // (1) 決定 Prefix 與 地址處理邏輯
            switch (cate)
            {
                // Discrete Input   (read-only)
                case HcfaCateEnum.IX:
                    prefix = "1";
                    modbusAddr = (this.Address * 8 + this.BitOffset) + 1;
                    break;

                // Coil             (read-write)
                case HcfaCateEnum.QX:
                case HcfaCateEnum.MX:
                    prefix = "0";
                    modbusAddr = (this.Address * 8 + this.BitOffset) + 1;
                    break;

                // Discrete Input   (read-only)
                case HcfaCateEnum.IB:
                case HcfaCateEnum.QB:
                    prefix = "1";
                    modbusAddr = (this.Address / 2) + 1;
                    break; // 若為 Byte 定址才需 / 2

                // Input Register   (read-only)
                case HcfaCateEnum.IW:
                    prefix = "3";
                    modbusAddr = this.Address + 1;
                    break;

                // Holding Register (read-write)
                case HcfaCateEnum.MW:
                case HcfaCateEnum.D:
                    prefix = "4";
                    modbusAddr = (this.Address / 2) + 1;
                    break;

#if(OPT_RESERVED_FOR_32_BIT)
                // Holding Register (read-write) (32-bit !!!)
                case HcfaCateEnum.MD:
                case HcfaCateEnum.DD:
                    prefix = "4";
                    modbusAddr = (this.Address / 2) + 1;
                    break;
#endif

                default:
                    throw new Exception($"{cate} 不支援 ToModbusString() !");
            }

            // (2) 格式化輸出
            // 加上 40000 / 30000 / 10000 / 0 等基底，或者維持 prefix + D4
            // 推薦使用 prefix + D4 (若確定不超過 9999) 或直接計算總值
            return $"{prefix}{modbusAddr:D4}";
        }
#endif
        internal string ToNModbus4String()
        {
            #region 定址_使用範例
            //========================================================================
            // 目前 Gaara 的 程式 只用到 IX, QX, QB, MW
            //------------------------------------------------------------------------
            //
            //  IX : (使用 modbusTcpClient.ReadDiscrete)
            //          IX0.0 ~ IX10.7         
            //
            //  QX : (使用 modbusTcpClient. ReadBool 與  modbusTcpClient. Write (bool) )
            //          QX0     (0*8)       + 1024    
            //          QX1016  (1016*8)    + 1024  
            //              ...
            //          QX1784  (1784*8)    + 1024  
            //
            //  QB : (使用 modbusTcpClient.ReadBool)
            //        QB1000.0
            //        QB1020.0
            //        QB1040.0
            //        QB1060.0
            //        QB1080.0
            //        QB1120.0
            //        QB1200.0
            //        QB1300.0
            //        QB1520.0
            //        QB1521.0
            //        QB1522.0
            //        QB1540.0
            //        QB1544.0
            //        QB1545.0
            //        QB1547.0  (也用於 input)
            //        QB1548.0  (也用於 input)
            //        QB1553.0
            //
            //  MW : (使用 modbusTcpClient. ReadInt16 與  modbusTcpClient. Write (int16) )    
            //        MW1000
            //        MW1020
            //        MW1040
            //        MW1060
            //        MW1100
            //        MW1300
            //        MW1340
            //
            //========================================================================
            #endregion

            int modbusAddr;
            var cate = (HcfaCateEnum)this.CateID;

            // (1) 決定 Prefix 與 地址處理邏輯
            switch (cate)
            {
                // Discrete Input
                // 使用 ReadDiscrete
                case HcfaCateEnum.IX:
                    modbusAddr = (this.Address * 8 + this.BitOffset);
                    break;

                // Coil
                // 使用 ReadBool
                // 使用 Write(bool isOn)
                case HcfaCateEnum.QX:
                    modbusAddr = (this.Address * 8 + this.BitOffset);
                    break;

                // Coils
                // 使用 ReadBool
                // 使用 Write(bool isOn)
                case HcfaCateEnum.QB:
                    modbusAddr = (this.Address * 8 + this.BitOffset);
                    break;

#if (OPT_RESERVED)
                // Input Register   (read-only)
                case HcfaCateEnum.IW:
                    modbusAddr = this.Address + 1;
                    break;
#endif
                // Holding Register (read-write)
                // 使用 ReadInt16
                // 使用 Write(int value)
                case HcfaCateEnum.MW:
                case HcfaCateEnum.D:
                    modbusAddr = this.Address;  // 十進位
                    break;

#if(OPT_RESERVED_FOR_32_BIT)
                // Holding Register (read-write) (32-bit !!!)
                case HcfaCateEnum.MD:
                case HcfaCateEnum.DD:
                    prefix = "4";
                    modbusAddr = (this.Address / 2) + 1;
                    break;
#endif

                default:
                    throw new Exception($"目前 {cate} 不支援 ToNModbus4String() !");
            }

            // (2) 格式化輸出
            return modbusAddr.ToString();
        }
        #endregion
    }
}
