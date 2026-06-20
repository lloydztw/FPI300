#region AUTHOR
/*
 * EzIO.Mem
 * Copyright (C) 2026
 * 2026-04-05 revised by LeTian Chang
 * 2013-07-11 created by LeTian Chang
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzIO.Mem;
using EzIO.Mem.Utils;
using System;
using System.ComponentModel;

namespace EzPlc.Hcfa
{
    /// <summary>
    /// HcfaAddress 
    /// <br/> 負責將種種 禾川 PLC 位址格式 轉換成 IoAddress
    /// <list type="bullet">
    /// <item>IX, QX, MX : 1-Bit,  8進位</item>
    /// <item>IB, QB, MB : 1-Bit,  8進位</item>    
    /// <item>IW, QW, MW : 16-Bit, 10進位</item>    
    /// <item>ID, QD, MD : 32-Bit, 10進位</item>    
    /// </list>    
    /// </summary>
    public class HcfaAddress : IoAddress
    {
        #region NLOG
        //static NLog.Logger _LOG => EzLog.LOG;
        #endregion

        #region PRIVATE_DATA
        string _modbusAddress;
        #endregion

        /// <summary>
        /// ezIoName 格式: $"{StationID}:{Category}{Address}"
        /// </summary>
        public HcfaAddress(string ezIoName, string description = null)
        {
            EzAddressParser.Parse(ezIoName, out int sid, out string cate, out int address, out int bitOffset);

            if (ParseCategory(cate, out int bits, out int bitStride, out int alignment))
            {
                // 關鍵修正：如果有點號偏移 (.5)，這就不再是 16-bit 暫存器，
                // 而是一個位於暫存器內的 1-bit 點。
                if (bitOffset >= 0)
                {
                    bits = 1;
                }

                this.StationID = (byte)sid;
                this.Category = cate;
                this.Address = (ushort)address;
                this.BitOffset = (byte)Math.Max(0, bitOffset);
                this.Bits = (byte)bits;
                this.BitStride = (byte)bitStride;

                // 重新計算通訊用的 Modbus Address
                _modbusAddress = ToModbusString();

                if (this.Bits == 32 && this.Address % 2 != 0)
                {
                    throw new ArgumentException($"Hcfa 32-bit 地址必須對齊偶數 (Byte Addressing): {ezIoName}");
                }
            }
            else
            {
                throw new ArgumentException($"Hcfa 字串格式有誤 : {ezIoName}");
            }
        }
        
        public override object Clone()
        {
            return MemberwiseClone();
        }

        public override string KeyName
        {
            get
            {
                // 如果您希望 StationID 為 1 時省略前綴，可以保留此邏輯
                string prefix = StationID == 1 ? "" : $"{StationID}:";

                // 針對位元類別 (Bits == 1)，強制顯示點號偏移
                if (this.Bits == 1)
                {
                    return $"{prefix}{Category}{Address}.{BitOffset}";
                }

                // 針對暫存器類別 (Bits > 1)，則維持原樣
                return $"{prefix}{Category}{Address}";
            }
        }
        public override string ToCommString()
        {
            return _modbusAddress;
        }

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

        bool ParseCategory_000(string category, out int bits, out int bitStride, out int alignment)
        {
            bits = 0;
            bitStride = 0;
            alignment = 1;

            if (string.IsNullOrEmpty(category)) return false;

            switch (category)
            {
                case "I":
                case "Q":
                case "IX":
                case "QX":
                    bits = 1;  bitStride = 8;  return true;
                case "IB":
                case "QB":
                    bits = 8; bitStride = 8; return true;
                case "IW":
                case "QW":
                    bits = 16; bitStride = 8; return true;
                case "ID":
                case "QD":
                    bits = 32; bitStride = 8; return true;

                case "M":
                case "MX":
                    // 如果 M0, M1, M2 是連號位元，Stride 應為 1
                    bits = 1; bitStride = 1; return true;
                case "MB":
                    bits = 8; bitStride = 8; return true;
                case "MW":
                    bits = 16; bitStride = 16; return true;
                case "MD":
                    bits = 32; bitStride = 32; return true;

                case "D":
                    bits = 16; bitStride = 0; return true;
                case "DD":
                    bits = 16; bitStride = 0; return true;

                default:
                    return false;
            }
        }

        public static bool ParseCategory(string category, out int bits, out int bitStride, out int alignment)
        {
            bits = 0;
            bitStride = 0;
            alignment = 1;

            if (string.IsNullOrEmpty(category)) 
                return false;

            switch (category.ToUpper())
            {
                //--- 位元類別 (Bit-Based) ---
                case "I":
                case "IX":
                case "Q":
                case "QX":
                case "M":
                case "MX":
                    bits = 1;
                    bitStride = 1; // 地址 +1 = +1 Bit
                    return true;

                //--- 位元組類別 (Byte-Based) ---
                case "IB":
                case "QB":
                case "MB":
                    bits = 8;
                    bitStride = 8; // 地址 +1 = +8 Bits (1 Byte)
                    return true;

                //--- 字元類別 (Word-Based / 10進位 Byte Address) ---
                case "IW":
                case "QW":
                case "MW":
                case "D":
                    bits = 16;
                    bitStride = 8; // 重要：因為是 Byte 定址，MW100 -> MW101 是跳 1 Byte (8 bits)
                    alignment = 1;
                    return true;

                //--- 雙字元類別 (DWord-Based / 10進位 Byte Address) ---
                case "ID":
                case "QD":
                case "MD":
                    bits = 32;
                    bitStride = 8; // 同上，MD100 -> MD101 是跳 1 Byte (雖然您有限制偶數，但 Stride 仍指單位增量)
                    alignment = 2; // 強制偶數對齊
                    return true;

                default:
                    return false;
            }
        }

        string ToModbusString()
        {
            //這裡我們將禾川的標籤（I, Q, M）映射到標準的 Modbus 區段：
            // (I / IX) →   1x (Discrete Input)
            // (Q / QX) →   0x (Coil)
            // (M / MX) →   0x (Internal Coil)
            // (IW / ID) →  3x (Input Register)
            // (MW / MD) →  4x (Holding Register)

            // 1. 基礎位址計算 (Base 1)
            // 注意：若 Hcfa 是 Byte 定址 (0, 2, 4...)，Modbus 需轉換為 Word 序號 (0, 1, 2...)
            // 只有暫存器類別 (16/32-bit) 才需要 / 2 轉換為 Word 序號
            int modbusAddr = (this.Bits >= 16 || this.BitOffset >= 0)
                             ? (this.Address / 2) + 1
                             : this.Address + 1; // 假設 MX 類別是 Bit 定址

            string prefix = "";

            // 2. 功能碼映射
            string cate = this.Category.ToUpper();
            switch (cate)
            {
                case "I":
                case "IX":
                case "IB":
                    prefix = "1"; break; // Discrete Input      (read-only)

                case "Q":
                case "QX":
                case "QB":
                case "M":
                case "MX":
                case "MB":
                    prefix = "0"; break; // Coil                (read-write)

                case "IW":
                case "ID":
                    prefix = "3"; break; // Input Register      (read-only)

                case "MW":
                case "MD":
                case "D":
                case "DD":
                    prefix = "4"; break; // Holding Register    (read-write)

                default:
                    return base.ToCommString();
            }

            // 3. 格式化主位址 (例如 40001)
            string finalAddr = $"{prefix}{modbusAddr:D4}";

            //--------------------------------------------------------------------
            // Modbus 通訊位址 應該不會有 "."
            //--------------------------------------------------------------------
            //
            // 範例: MW100.5 對應的 modbus 位址 是 40051
            // 
            // 邏輯層(IoMemory)：當底層把 40051 的數值（16 bits）讀回來後，
            // 程式會根據點位定義中的 BitOffset = 5，
            // 從這個 16 - bit 數值中取出第 5 個 bit。
            //
            //--------------------------------------------------------------------

            //// 4. 處理位元偏移 (Bit-in-Word)
            //// 條件：只要有 BitOffset 且它是寄存器類型 (3x, 4x)
            //if (this.BitOffset > 0 && (prefix == "3" || prefix == "4"))
            //{
            //    return $"{finalAddr}.{this.BitOffset}";
            //}

            return finalAddr;
        }
    }


    public enum HcfaCateEnum : int
    {
        [Description("輸入繼電器 (X) (1-bit) (8進制)")]
        I,
        [Description("輸出繼電器 (Y) (1-bit) (8進制)")]
        Q,
        [Description("輔助繼電器 (M) (1-bit) (10進制)")]
        M,
        [Description("資料暫存器 (D) (16-bit) (10進制)")]
        D,

        [Description("I單點 (1-bit) (8進制)")]
        IX = I,
        [Description("Q單點 (1-bit) (8進制)")]
        QX = Q,
        [Description("M單點 (1-bit) (10進制)")]
        MX = M,

        [Description("I群 (8-bit 打包, 8進制) 輸入繼電器 (X)")]
        IB,
        [Description("Q群 (8-bit 打包, 8進制) 輸出繼電器 (Y)")]
        QB,
        [Description("M (8-bit 打包, 10進制) 輔助繼電器 (M)")]
        MB,

        [Description("I群 (16-bit 打包, 8進制) 輸入繼電器 (X)")]
        IW,
        [Description("Q群 (16-bit 打包, 8進制) 輸出繼電器 (Y)")]
        QW,
        [Description("M群 (16-bit 打包, 10進制) 輔助繼電器 (M)")]
        MW,
    }
}
