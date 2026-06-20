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

namespace EzPlc.Omron
{
    /// <summary>
    /// OMRON PLC 專用定址實作
    /// </summary>
    public class OmronAddress : IoAddress
    {
        #region NLOG
        static NLog.Logger _LOG => EzLog.LOG;
        #endregion

        #region PRIVATE_DATA
        string _omronVarName;
        #endregion

        #region 建構子
        /// <summary>
        /// ORMON 格式: $"{stationID}:{varPrefix}.{varSuffix}"
        /// </summary>
        public OmronAddress(string ezIoName, string description = null)
        {
            // 使用通用解析器取得基本資訊
            ParseOmron(ezIoName, out OmronCateEnum cate, out IoBitsEnum bits, out _omronVarName, out int stationID, out int virtualAddress);

            // 更新 IoAddress 的內部屬性 (I)
            this.StationID = (byte)stationID;
            this.CateID = (int)cate;
            this.Address = (ushort)virtualAddress;
            this.Description = description;

            // 更新 IoAddress 的內部屬性 (II)
            this.BitsEnum = bits;
            this.BitStride = (byte)1;
            this.BitOffset = (byte)0;
        }
        public OmronAddress(IAddress src)
        {
            CopyFrom(src);
        }
        protected OmronAddress() : base() { }
        public override object Clone()
        {
            var clone = new OmronAddress();
            clone.CopyFrom(this);
            return clone;
        }
        #endregion

        #region IAddress 實作與複寫 (Category)
        protected override Enum CateEnum => (OmronCateEnum)CateID;
        public OmronCateEnum OmronCategory
        {
            get => (OmronCateEnum)CateID;
            private set => CateID = Convert.ToInt32(value);
        }
        #endregion

        #region IAddress 實作與複寫 (KeyName 與 通訊字串)
        public override string ToString()
        {
            //if (_omronVarName != null && _omronVarName.Contains("."))
            //    return _omronVarName.Split('.')[1].Trim();
            //else
            return _omronVarName;
        }
        public override string KeyName
        {
            get => _omronVarName;
        }
        public override string ToCommString()
        {
            return _omronVarName;
        }
        #endregion

        #region 邏輯規格化
        protected override int NormalizeBitsInfo(Enum cate, out IoBitsEnum bits, out int stride, int offset = 0)
        {
            switch((OmronCateEnum)cate)
            {
                case OmronCateEnum.BOOL:
                    bits = IoBitsEnum.Bits_1;
                    break;
                default:
                    bits = IoBitsEnum.Bits_32;
                    break;
            }
            stride = 1;
            return offset;
        }
        #endregion

        #region STRING_CONVERT_FUNCTIONS_字串格式轉換
        /// <summary>
        /// ORMON 格式: $"{stationID}:{varPrefix}.{varSuffix}"
        /// </summary>
        public static OmronAddress TryParse(string ezIoName)
        {
            try
            {
                var addr = new OmronAddress(ezIoName);
                return addr;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// ORMON 格式: $"{stationID}:{varPrefix}.{varSuffix}"
        /// </summary>
        public static void ParseOmron(string ezIoName,
                                      out OmronCateEnum category,
                                      out IoBitsEnum bits,
                                      out string omronVarName,
                                      out int stationID,
                                      out int virtualAddress)
        {
            if (string.IsNullOrEmpty(ezIoName))
            {
                var ex = new Exception("ezIoName is empty!");
                throw ex;
            }

            string[] strs;

            #region PRE_CHECK
            if (ezIoName.Contains(","))
            {
                strs = ezIoName.Split(',');
                ezIoName = strs[0];
                var addr2 = strs[1].Trim();
                if (!string.IsNullOrEmpty(addr2))
                    _LOG.Warn("一行字串有第二位址點: {0}", strs[1]);
            }
            #endregion

            //(1) StationID (只是為了相容性)
            stationID = 0;
            if (ezIoName.Contains(":"))
            {
                strs = ezIoName.Split(':');
                ezIoName = strs[1].Trim();
                if (strs.Length >= 2)
                {
                    int.TryParse(strs[0], out stationID);
                }
            }

            //(2) 分離 varPrefix 與 varSuffix
            string varPrefix;
            string varSuffix;
            if (ezIoName.Contains("."))
            {
                strs = ezIoName.Split('.');
                if (strs.Length >= 2)
                {
                    varPrefix = strs[0].Trim();
                    varSuffix = strs[1].Trim();
                }
                else
                {
                    varPrefix = "";
                    varSuffix = strs[0].Trim();
                }
            }
            else
            {
                varPrefix = "";
                varSuffix = ezIoName.Trim();
            }

            //(3) Category 的部分
            //(3.1) "b" 開頭的變量 默認為 BOOL
            if (varSuffix.StartsWith("b"))
            {
                category = OmronCateEnum.BOOL;
                bits = IoBitsEnum.Bits_1;
            }
            //(3.2) "i" 開頭的為 INT32
            else if (varSuffix.StartsWith("i"))
            {
                category = OmronCateEnum.INT32;
                bits = IoBitsEnum.Bits_32;
            }
            //(3.3) "r" 開頭的為 FLOAT32
            else if (varSuffix.StartsWith("r"))
            {
                category = OmronCateEnum.FLOAT32;
                bits = IoBitsEnum.Bits_32;
            }
            //(3.4) "s" 開頭的變量 默認為 STR
            else if (varSuffix.StartsWith("s"))
            {
                category = OmronCateEnum.STR;
                bits = IoBitsEnum.Bits_32;
            }
            else
            {
                var ex = new Exception($"暫時尚未支援的 Category 類型: {ezIoName}");
                _LOG.Error(ex);
                throw ex;
            }

            //(4) Virtual Address (OMRON 沒有 address number 的設計, 暫時都填入 0)
            virtualAddress = 0;

            //(5) OMRON Name (不包含 $"{statioID}:")
            if (!string.IsNullOrEmpty(varPrefix))
                omronVarName = $"{varPrefix}.{varSuffix}";
            else
                omronVarName = $"{varSuffix}";
        }
        #endregion
    }
}
