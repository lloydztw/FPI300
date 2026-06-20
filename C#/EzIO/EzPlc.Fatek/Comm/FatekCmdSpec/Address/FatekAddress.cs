using System;

namespace EzPlc.Fatek.Comm
{
    public partial class FatekAddress : IFkAddress
    {
        #region ENUMS
        public enum MemoryType : byte
        {
            //-------------------------------------
            // bits= 1
            //-------------------------------------
            X,
            Y,
            M,

            ////S,
            ////T,
            ////C,

            //-------------------------------------
            // bits= 16
            //-------------------------------------
            WX,
            WY,
            WM,

            ////WS,
            ////WT,
            ////WC,
            ////RT,
            ////RC,

            R,
            D,
            //> F,

            //-------------------------------------
            // bits= 32
            //-------------------------------------
            DWX,
            DWY,
            DWM,

            ////DWS,
            ////DWT,
            ////DWC,
            ////DRT,
            ////DRC,

            DR,
            DD,
            //> DF
        }
        #endregion

        public MemoryType Type { get; private set; }
        public ushort Address { get; private set; }
        public int Bits
        {
            get
            {
                if (Type >= MemoryType.DWX)
                    return 32;
                else if (Type >= MemoryType.WX)
                    return 16;
                else
                    return 1;
            }
        }

        string IFkAddress.ToFatekString() => ToFatekString();
        string IFkAddress.Category => Type.ToString();
        byte IFkAddress.Bits => (byte)Bits;

        protected FatekAddress()
        {
        }
        public FatekAddress(string addressStr)
        {
            var a = From(addressStr);

            if (a == null)
            {
                ASSERT_ERR_FORMAT(addressStr);
                return;
            }

            Address = a.Address;
            Type = a.Type;
            //Bits = a.Bits;
        }
        public FatekAddress(MemoryType type, int address)
        {
            var a = From(type, address);

            if (a == null)
            {
                ASSERT_ERR_FORMAT(type.ToString() + address);
                return;
            }

            Address = a.Address;
            Type = a.Type;
            //Bits = a.Bits;
        }
        public FatekAddress(string type, int address)
            : this(type + address)
        {
        }

        public override string ToString()
        {
            return ToFatekString();
        }
        public string ToString(string fmt)
        {
            return Type.ToString() + Address.ToString(fmt);
        }
        public string ToFatekString()
        {
            switch (Type)
            {
                // [X/Y/M]1234 (a group of 16-point M,X,Y data)
                case MemoryType.M:
                case MemoryType.X:
                case MemoryType.Y:
                    return string.Format("{0}{1:0000}", Type.ToString(), Address);

                // [R/D]12345 (16-bit R/D register)
                case MemoryType.R:
                case MemoryType.D:
                    return string.Format("{0}{1:00000}", Type.ToString(), Address);

                // W[X/Y/M]1234 (a group of 16-point M,X,Y data)
                case MemoryType.WM:
                case MemoryType.WX:
                case MemoryType.WY:
                    return string.Format("{0}{1:0000}", Type.ToString(), Address);

                // D[R/D]12345 (32-bit R register)
                case MemoryType.DR:
                case MemoryType.DD:
                    return string.Format("{0}{1:00000}", Type.ToString(), Address);

                // DW[X]1234 (a group of 32-point M,X,Y data)
                case MemoryType.DWM:
                case MemoryType.DWX:
                case MemoryType.DWY:
                    return string.Format("{0}{1:0000}", Type.ToString(), Address);
            }
            return null;
        }

        public static FatekAddress From(string str)
        {
            MemoryType type;
            int address;

            if (!TryParse(str, out type, out address))
            {
                ASSERT_ERR_TYPE(str);
                return null;
            }

            return From(type, address);
        }
        public static FatekAddress From(string type, int address)
        {
            return From(type + address);
        }
        public static FatekAddress From(MemoryType type, int address)
        {
            var addr = new FatekAddress();
            addr.Address = (ushort)address;
            addr.Type = type;

            ushort bits, digits, alignment;
            int len, maxAddr;
            len = getAddressConstraints(type, out bits, out digits, out alignment, out maxAddr);

            //> addr.Bits = bits;

            string tag = addr.ToString();
            if (ASSERT_ADDRESS_MAX(tag, address, maxAddr) &&
                ASSERT_ALIGNMENT(tag, address, alignment))
                return addr;

            return null;
        }
        public static FatekAddress GetAlignment(FatekAddress addr, int bits)
        {
            if (addr.Bits == bits)
                return addr;

            // Bits : 1 => 16 or 32
            if (addr.Bits == 1)
            {
                string prefix = (bits == 32) ? "DW" : "W";
                string type = prefix + addr.Type.ToString();
                int offset = (addr.Address % bits);
                int address = addr.Address - offset;
                return From(type, address);
            }
            // Bits : 16 or 32 => 1
            else if (bits == 1)
            {
                string prefix = (addr.Bits == 32) ? "DW" : "W";
                string type = addr.Type.ToString();
                if (!type.StartsWith(prefix))
                {
                    // R & D registers do not have the correspondent 1-bit points.
                    return addr;
                }
                type = type.Replace(prefix, "");
                int address = addr.Address;
                return From(type, address);
            }
            // Bits : 16 => 32
            else if (addr.Bits == 16 && bits == 32)
            {
                string type = "D" + addr.Type.ToString();
                int offset = (addr.Address % 2);
                int address = addr.Address - offset;
                return From(type, address);
            }
            // Bits : 32 => 16
            else if (addr.Bits == 32 && bits == 16)
            {
                string type = addr.Type.ToString().Replace("D", "");
                return From(type, addr.Address);
            }
            // Bits : others
            else
            {
                return addr;
            }
        }

#if(OPT_RESERVED)
        public static int BitOffset(FatekAddress a0, FatekAddress a2, bool is32Bits = false)
        {
            int offset = Offset(a0, a2);
            if (offset == 0)
            {
                return 0;
            }

            if (offset < 0)
            {
                var swap = a0;
                a0 = a2;
                a2 = swap;
                offset = -offset;
            }

            int nBase = is32Bits ? 32 : 16;
            System.Diagnostics.Trace.Assert(a0.Address % nBase == 0, string.Format("{0} 必須是{1}整數倍!", a0, nBase));
            System.Diagnostics.Trace.Assert(offset < nBase, string.Format("{0} 與 {1} 差異必須 < {2}", a0, a2, nBase));
            return offset;
        }
        public static int Offset(FatekAddress a0, FatekAddress a2)
        {
            try
            {
                if (a0 != null && a2 != null && a0.Type == a2.Type)
                {
                    return a2.Address - a0.Address;
                }
            }
            catch
            {
            }
            //string err = "不同種類的 IoPoint 無法比對位址 : " + a0 + " vs " + a2;
            //throw new ApplicationException(err);
            return int.MaxValue;
        }
        public bool Equals(FatekAddress a)
        {
            if (a == this)
                return true;

            if (a != null)
                return (a.Type == this.Type) && (a.Address == this.Address);

            return false;
        }
#endif

        /// <summary>
        /// 搭配 QxFatekPlcNode 使用
        /// </summary>
        public static bool TryParse(string addressName, out string typeStr, out int address, out int bits, out bool isSingleIoType)
        {
            if (addressName != null)
            {
                var a = From(addressName);
                if (a != null)
                {
                    typeStr = a.Type.ToString();
                    address = a.Address;
                    bits = a.Bits;
                    isSingleIoType = bits == 1;
                    return true;
                }
            }

            typeStr = null;
            address = 0;
            bits = 0;
            isSingleIoType = true;
            return false;
        }

        /// <summary>
        /// /// 搭配 QxFatekPlcNode 使用
        /// </summary>
        public static bool TryParseNodeAddr(string typeStr, int address, out string addressStr, out int bits, bool using32Bit = false)
        {
            addressStr = null;
            bits = 0;

            string addressStr0 = typeStr + "0";
            int addr0;
            bool isSingleIoType;

            bool ok = TryParse(addressStr0, out typeStr, out addr0, out bits, out isSingleIoType);

            if (isSingleIoType)
            {
                // [DW]{X,Y,M}1234
                if (using32Bit && bits < 32)
                {
                    addressStr = "DW" + typeStr + string.Format("{0:0000}", address);
                    bits = 32;
                }
                // [W]{X,Y,M}1234
                else if (bits < 16)
                {
                    addressStr = "W" + typeStr + string.Format("{0:0000}", address);
                    bits = 16;
                }
                else
                {
                    addressStr = typeStr + string.Format("{0:0000}", address);
                }
            }
            else
            {
                if (using32Bit && bits < 32)
                {
                    // [D]{D,R}12345
                    addressStr = "D" + typeStr + string.Format("{0:00000}", address);
                    bits = 32;
                }
                else
                {
                    // {D,R}12345
                    addressStr = typeStr + string.Format("{0:00000}", address);
                }
            }

            return ok;
        }

        #region PROTECTED_FUNCTIONS
        protected static bool TryParse(string str, out MemoryType type, out int address)
        {
            address = -1;

            int pos = firstNumericIndex(str, 5);

            if (1 <= pos && pos <= 3)
            {
                var typeStr = str.Substring(0, pos);
                var addrStr = str.Substring(pos);
                bool ok = int.TryParse(addrStr, out address);

                // 只檢查 type 是否正確
                var typeValues = Enum.GetValues(typeof(MemoryType));
                foreach (MemoryType typeEnum in typeValues)
                {
                    if (typeStr == typeEnum.ToString())
                    {
                        type = typeEnum;
                        return true;
                    }
                }
            }

            // 只檢查 type 是否正確
            unchecked
            {
                type = (MemoryType)(-1);
            }

            return false;
        }
        protected static int getAddressConstraints(MemoryType type, out ushort bits, out ushort digits, out ushort alignment, out int maxAddr)
        {
            alignment = 1;

            switch (type)
            {
                // [M]1234 (a group of 16-point M,X,Y data)
                case MemoryType.M:
                case MemoryType.X:
                case MemoryType.Y:
                    bits = 1;
                    digits = 4;
                    maxAddr = 9999;
                    return 5;

                // [R]12345 (16-bit R/D register)
                case MemoryType.R:
                case MemoryType.D:
                    bits = 16;
                    digits = 5;
                    maxAddr = 65535;
                    return 6;

                // W[M]1234 (a group of 16-point M,X,Y data)
                case MemoryType.WM:
                case MemoryType.WX:
                case MemoryType.WY:
                    bits = 16;
                    digits = 4;
                    maxAddr = 9984;
                    alignment = 8;      // FATEK
                    alignment = 16;     // LETIAN
                    return 6;

                // D[R]12345 (32-bit R register)
                case MemoryType.DR:
                case MemoryType.DD:
                    bits = 32;
                    digits = 5;
                    maxAddr = 65534;
                    alignment = 2;      // LeTian
                    return 7;

                // DW[M]1234 (a group of 32-point M,X,Y data)
                case MemoryType.DWM:
                case MemoryType.DWX:
                case MemoryType.DWY:
                    bits = 32;
                    digits = 4;
                    maxAddr = 9968;
                    alignment = 8;      // FATEK
                    alignment = 32;     // LeTian
                    return 7;

                default:
                    bits = 0;
                    digits = 0;
                    maxAddr = 0;
                    return 0;
            }
        }
        protected static int firstNumericIndex(string str, int len = int.MaxValue)
        {
            char c;
            len = System.Math.Min(len, str.Length);
            for (int i = 0; i < len; i++)
            {
                c = str[i];
                if ('0' <= c && c <= '9')
                    return i;
            }
            return -1;
        }
        #endregion

        #region ASSERTIONS
        static bool ASSERT_ERR_TYPE(string str)
        {
            if (str != null)
                throw new ApplicationException(
                    string.Format("型別錯誤 : {0}", str));
            return true;
        }
        static bool ASSERT_ERR_FORMAT(string str)
        {
            if (str != null)
                throw new ApplicationException(
                    string.Format("格式錯誤 : {0}", str));
            return true;
        }
        static bool ASSERT_LENGTH(string str, int len)
        {
            if (str.Length != len)
                throw new ApplicationException(
                    string.Format("長度必須是 {0} : {1}", len, str));
            return true;
        }
        static bool ASSERT_ALIGNMENT(string str, int address, int alignBits)
        {
            if (address % alignBits != 0)
                throw new ApplicationException(
                    string.Format("位置必須對齊 {0} 倍數 : {1}", alignBits, str));
            return true;
        }
        static bool ASSERT_ADDRESS_MAX(string str, int address, int max)
        {
            if (address > max)
            {
                throw new ApplicationException(
                    string.Format("位置不能超過 {0} : {1}", max, str));
            }
            else if (address < 0)
            {
                ASSERT_ERR_FORMAT(str);
                return false;
            }
            else
            {
                return true;
            }
        }
        #endregion
    }
}
