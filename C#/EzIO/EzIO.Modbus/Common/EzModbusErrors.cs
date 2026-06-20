#region AUTHOR
/*
 * EzIO.Modbus
 * 
 * Copyright (C) 2026
 * 
 * 2026-04-21 Integration with EzIO by LeTian Chang
 * 2013-07-11 created by LeTian Chang
 * 
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System;
using System.ComponentModel;

namespace EzIO.Modbus
{
    public enum EzModbusError : int
    {
        [Description("通訊正常")]
        NoErr = 0,

#if (false)
        [Description("通訊異常: CheckSum錯誤")]
        Err_ResponseChkSum = 0x11,
        [Description("通訊異常: 回覆長度錯誤")]
        Err_ResponseLength = 0x12,
        [Description("通訊異常: 標頭資料與命令不同")]
        Err_ResponseHeader = 0x13,
#endif

        [Description("通訊異常: 尚未開啟裝置")]
        Device_NotOpen = 0x70,
        [Description("通訊異常: 無此裝置")]
        Device_NotExist,
        [Description("通訊異常: IP 無法開啟")]
        Device_IP_Open_Failed,
        [Description("通訊異常: IP 無法關閉")]
        Device_IP_Close_Failed,

        [Description("通訊異常: Com Port 無法開啟")]
        Device_ComPort_Open_Failed,
        [Description("通訊異常: Com Port 無法關閉")]
        Device_ComPort_Close_Failed,

#if(false)
        [Description("通訊異常: comm 無法初始化")]
        Device_Comm_Init_Failed,
        [Description("通訊異常: comm 無法解除")]
        Device_Comm_Dispose_Failed,
        [Description("通訊異常: comm 無法寫出資料")]
        Device_Comm_Write_Failed,
        [Description("通訊異常: comm 無法讀進資料")]
        Device_Comm_Read_Failed,
        [Description("通訊異常: comm 執行緒 RX")]
        Device_Comm_Callback_Failed_RX,
        [Description("通訊異常: comm 執行緒 TX")]
        Device_Comm_Callback_Failed_TX,
        [Description("通訊異常: comm 執行緒 Handle RSP")]
        Device_Comm_Callback_Failed_Handle_RSP,
#endif

        [Description("通訊異常: 掉線")]
        Device_Lost_Connection = 0x80,
        [Description("通訊異常: 無法與設備連線.")]
        Device_Connect_Failed = 0x81,
        [Description("通訊異常: 逾時")]
        Timeout = 0x82,
        [Description("通訊異常: 無回覆結果")]
        Null_Result = 0x83,
        [Description("通訊異常: 旨令超過重發次數")]
        CmdRetry_Overflow = 0x87,
        [Description("通訊其他異常")]
        Misc_Exception = 0x8F,

        [Description("位址型別錯誤")]
        Address_Category_Err = 0x100,
        [Description("位址位元錯誤")]
        Address_Bits_Err,
        [Description("位址超出範圍")]
        Address_Overflow,
    }


    public class EzModbusException : Exception
    {
        protected EzModbusException()
        {
        }

        public EzModbusException(EzModbusError err, 
                Exception ex, bool isFatal, string extraMsg = null)
                : base(JetEazy.QxNums.GetEnumDescription(err), ex)
        {
            Err = err;
            IsFatal = isFatal;
            ExtraMsg = extraMsg;
        }

        public EzModbusError Err
        {
            get;
            private set;
        }
        public bool IsFatal
        {
            get;
            protected set;
        }
        public string ExtraMsg
        {
            get;
            private set;
        }
        public Exception Excp
        {
            get { return this.InnerException; }
        }
    }


    public class EzModbusErrEventArgs : DoWorkEventArgs
    {
        public EzModbusErrEventArgs(EzModbusException ex) : base(ex)
        {
        }
        public EzModbusException Excp
        {
            get { return (EzModbusException)Argument; }
        }
        public EzModbusError Err
        {
            get { return Excp.Err; }
        }
        public bool IsFatal
        {
            get { return Excp.IsFatal; }
        }
        public string Message
        {
            get { return Excp.ExtraMsg; }
        }
    }
}
