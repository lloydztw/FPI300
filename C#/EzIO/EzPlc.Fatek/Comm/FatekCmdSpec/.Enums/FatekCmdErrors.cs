#region AUTHOR
/*
 * EzPlc.Fatek
 * Copyright (C) 2026
 * 2026-04-05 revised by LeTian Chang
 * 2013-07-11 created by LeTian Chang
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System.ComponentModel;


namespace EzPlc.Fatek.Comm
{
    public enum FatekCommandErr : int
    {
        [Description("通訊正常")]
        NoErr = 0,

        [Description("未定義1")]
        FatekErr1,
        [Description("不合法數值(如10 進制格式中有16 進制數字)")]
        InvalidHex = 2,
        [Description("未定義3")]
        FatekErr3,
        [Description("不合法之命令格式 (含不合法之命令碼), 或通訊命令無法執行")]
        WrongFormat = 4,
        [Description("不能啟動 (下RUN命令, 但 Ladder Checksum 不合)")]
        NotRun_Checksum = 5,
        [Description("不能啟動 (下RUN命令, 但 PLC ID ≠ Ladder ID)")]
        NotRun_PlcLadderID = 6,
        [Description("不能啟動 (下RUN命令, 但程式語法錯誤)")]
        NotRun_WrongProgram = 7,
        [Description("未定義8")]
        FatekErr8,
        [Description("不能啟動 (下RUN命令, 但Ladder之程式指令, PLC無法執行)")]
        NotRun_WrongLadderInstruct = 9,
        [Description("不合法之位址")]
        IllegalAddress = 0xA,


        //-----------------------------------------------------------------

        [Description("JetEazy OEM 擴增")]
        Extension = 0x10,

        [Description("通訊異常: CheckSum錯誤")]
        Err_ResponseChkSum = 0x11,
        [Description("通訊異常: 回覆長度錯誤")]
        Err_ResponseLength = 0x12,
        [Description("通訊異常: 標頭資料與命令不同")]
        Err_ResponseHeader = 0x13,

        [Description("讀回字串: 格式辨讀異常.")]
        Err_ReponseParsing_Exception,
        [Description("讀回PLC狀態: 字串格式異常.")]
        Err_ReadStatus_WrongStr,
        [Description("讀回連續單點: 數據長度有誤.")]
        Err_ReadContiSinglePoints_WrongDataLength,
        [Description("讀回連續多點暫存器: 數據有誤.")]
        Err_ReadContiRegs_WrongData,
        [Description("讀回混和多點暫存器: 數據有誤.")]
        Err_ReadMixRegs_WrongData,

        _END_,
    }
}
