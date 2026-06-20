using DVPCameraType;
using EzCamera.Interface;
using System;
using System.ComponentModel;

namespace EzCamera.Driver.DVP
{
    public class Do3Error : EzCameraError
    {
        public Do3Error(int errCode, string message = null, string description = null)
            : base(errCode, message, description)
        {
            if (message == null)
                Message = GetErrorText(errCode);
            else if (description == null)
                Description = GetErrorText(errCode);
        }

        /// <summary>
        /// DVP 專屬的 ErrCode 對應字串
        /// </summary>
        public static string GetErrorText(int errCode)
        {
            var status = (dvpStatus)errCode;
            return GetEnumDescription(status);
        }

        /// <summary>
        /// 此通用函式可以取得 enum 的註解 (Description)
        /// </summary>
        public static string GetEnumDescription(Enum value)
        {
            var fieldInfo = value.GetType().GetField(value.ToString());
            var attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : value.ToString();
        }
    }
}
