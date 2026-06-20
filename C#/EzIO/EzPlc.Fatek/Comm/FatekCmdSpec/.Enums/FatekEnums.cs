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


namespace EzPlc.Fatek.Comm
{
    //public enum MemoryType : int
    //{
    //    M,
    //    X,
    //    Y,

    //    R,
    //    D,

    //    WM,
    //    WX,
    //    WY,

    //    DR,
    //    DD,

    //    DWM,
    //    DWX,
    //    DWY,
    //}

    public enum FatekCommConsts : byte
    {
        STX = 0x02,
        ETX = 0x03,
    }

    public enum FatekIoCtrlCode : int
    {
        Disable = (int)'1',
        Enable = (int)'2',
        Set = (int)'3',
        Reset = (int)'4',
    }

    public enum FatekStatusBits : int
    {
        Running = (1 << 0),                 //RUN / STOP
        BatteryLow = (1 << 1),              //BAT LOW / 正常
        LadderChkSumErr = (1 << 2),         //Ladder checksum error / 正常
        UsingMemoryPack = (1 << 3),         //使用MEMORY PACK / 未使用
        WatchDogTimeout = (1 << 4),         //WDT Timeout / 正常
        IDSet = (1 << 5),                   //設定ID / 未設ID
        EmgStop = (1 << 6),                 //緊急停機 / 正常
        Reserved = (1 << 7),                //保留.
    }
}
