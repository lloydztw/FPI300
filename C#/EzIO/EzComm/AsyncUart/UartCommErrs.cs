#region AUTHOR
/****************************************************************************
 *                                                                          
 * Copyright (c) 2012 Jet Eazy Corp. All rights reserved.        
 *                                                                          
 ***************************************************************************/

/****************************************************************************
 *
 * VERSION
 *		$Revision:$
 *
 * HISTORY
 *      $Id:$
 *          20120622 LeTian Chang: Revised for the more robust connection.
 *	        20080701 LeTian Chang: Creation         
 *
 * DESCRIPTION
 *      
 *
 ***************************************************************************/
#endregion

using System.ComponentModel;


namespace EzComm.Uart
{
    public enum UartCommErr : int
    {
#if(false)
        [Description("通訊正常")]
        NoErr = 0,
        [Description("通訊異常: 逾時")]
        Err_Timeout = 0x81,
        [Description("通訊異常: 無回覆結果")]
        Err_NullResult = 0x82,
        [Description("通訊異常: 回覆長度錯誤")]
        Err_ResponseLength = 0x84,
        [Description("通訊異常: CheckSum錯誤")]
        Err_ResponseChkSum = 0x85,
        [Description("通訊異常: 標頭資料與命令不同")]
        Err_ResponseHeader = 0x86,
        [Description("通訊異常: 其他異常")]
        Err_Exception = 0x8F,
#endif

        [Description("通訊正常")]
        NoErr = 0,


        [Description("通訊異常: CheckSum錯誤")]
        Err_ResponseChkSum = 0x11,
        [Description("通訊異常: 回覆長度錯誤")]
        Err_ResponseLength = 0x12,
        [Description("通訊異常: 標頭資料與命令不同")]
        Err_ResponseHeader = 0x13,


        [Description("通訊異常: 無此裝置")]
        Device_NotExist = 0x70,
        [Description("通訊異常: com 被占用")]
        Device_Occupied,

        [Description("通訊異常: com 無法初始化")]
        Device_Comm_Init_Failed,
        [Description("通訊異常: com 無法解除")]
        Device_Comm_Dispose_Failed,

        [Description("通訊異常: com 無法開啟")]
        Device_Comm_Open_Failed,
        [Description("通訊異常: com 無法關閉")]
        Device_Comm_Close_Failed,

        [Description("通訊異常: com 無法寫出資料")]
        Device_Comm_Write_Failed,
        [Description("通訊異常: com 無法讀進資料")]
        Device_Comm_Read_Failed,

        [Description("通訊異常: com 執行緒 RX")]
        Device_Comm_Callback_Failed_RX,
        [Description("通訊異常: com 執行緒 TX")]
        Device_Comm_Callback_Failed_TX,
        [Description("通訊異常: com 執行緒 Handle RSP")]
        Device_Comm_Callback_Failed_Handle_RSP,


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
    }

}