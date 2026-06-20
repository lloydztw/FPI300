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

using System;


namespace EzIO.Mem.Utils
{
    public class EzAddressParser
    {
        #region NLOG
        static NLog.Logger _LOG => EzLog.LOG;
        #endregion

#if (OPT_LEGACY)
        /// <summary>
        /// 通用格式: $"{StationID}:{Category}{Address}.{BitOffset}"
        /// (如果有異常會拋出 Exception)
        /// </summary>
        public static void Parse(string ezIoName,
                                 out int stationId,
                                 out string category,
                                 out int address,
                                 out int bitOffset)
        {
            if (string.IsNullOrEmpty(ezIoName))
            {
                var ex = new Exception("ezIoName is empty!");
                _LOG.Error(ex);
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

            // Parse_Out_StationID (1-based)
            stationId = 0;
            if (ezIoName.Contains(":"))
            {
                strs = ezIoName.Split(':');
                ezIoName = strs[1].Trim();
                if (strs.Length >= 2)
                {
                    int.TryParse(strs[0], out stationId);
                }
            }

            // Category
            int idxN = FirstNumericIndex(ezIoName);
            if (idxN == -1)
            {
                var ex = new Exception("ezIoName 字串內必須含有數字!");
                _LOG.Error(ex);
                throw ex;
            }

            category = ezIoName.Substring(0, idxN).ToUpper().Trim();

            strs = ezIoName.Substring(idxN).Split('.');
            if (strs.Length >= 2)
            {
                address = int.Parse(strs[0]);
                bitOffset = int.Parse(strs[1]);
            }
            else
            {
                address = int.Parse(strs[0]);
                bitOffset = -1;
            }
        }

        public static bool TryParse(string ezIoName,
                                 out int stationId,
                                 out string category,
                                 out int address,
                                 out int bitOffset)
        {
            stationId = 0;
            category = "";
            address = 0;
            bitOffset = 0;

            try
            {
                Parse(ezIoName, out stationId, out category, out address, out bitOffset);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
#endif

        /// <summary>
        /// 通用格式: $"{StationID}:{Category}{Address}.{BitOffset}"
        /// (如果有異常會拋出 Exception)
        /// </summary>
        public static void Parse<T>(string ezIoName,
                                    out int stationId,
                                    out T category,
                                    out int address,
                                    out int bitOffset)
                                    where T : struct, Enum
        {
            if (string.IsNullOrEmpty(ezIoName))
            {
                var ex = new Exception("ezIoName is empty!");
                _LOG.Error(ex);
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

            // StationID
            stationId = 0;
            if (ezIoName.Contains(":"))
            {
                strs = ezIoName.Split(':');
                ezIoName = strs[1].Trim();
                if (strs.Length >= 2)
                {
                    int.TryParse(strs[0], out stationId);
                }
            }

            // 分離 Category String  與 Numeric String
            int idxN = FirstNumericIndex(ezIoName);
            if (idxN == -1)
            {
                var ex = new Exception("ezIoName 字串內必須含有數字!");
                _LOG.Error(ex);
                throw ex;
            }

            // Category 的部分
            string cateStr = ezIoName.Substring(0, idxN).ToUpper().Trim();
            if (!Enum.TryParse<T>(cateStr, true, out category))
            {
                var ex = new Exception($"無法識別的 Category 類型: {cateStr}");
                _LOG.Error(ex);
                throw ex;
            }

            // Numeric 的部分
            strs = ezIoName.Substring(idxN).Split('.');
            if (strs.Length >= 2)
            {
                address = int.Parse(strs[0]);
                bitOffset = int.Parse(strs[1]);
            }
            else
            {
                address = int.Parse(strs[0]);
                bitOffset = 0;
            }
        }

        public static bool TryParse<T>(string ezIoName,
                                     out int stationId,
                                     out T category,
                                     out int address,
                                     out int bitOffset)
                                     where T : struct, Enum
        {
            stationId = 0;
            category = default;
            address = 0;
            bitOffset = 0;

            try
            {
                Parse(ezIoName, out stationId, out category, out address, out bitOffset);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static int FirstNumericIndex(string str, int len = int.MaxValue)
        {
            if (str == null) return -1;

            int end = str.Length;
            if (len < end) end = len;

            for (int i = 0; i < end; i++)
            {
                char c = str[i];
                if (c >= '0' && c <= '9')
                    return i;
            }
            return -1;
        }
    }
}
