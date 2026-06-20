#region AUTHOR
/*
 * EzIO.Mem.IoAddress
 * Copyright (C) 2023
 * 2013-07-11 created by LeTian Chang
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion


using System;

namespace EzIO.Mem
{
    /// <summary>
    /// IO 點位的定址
    /// </summary>   
    public interface IAddress
    {
        /// <summary>
        /// 用來作 索引 的字串.
        /// </summary>
        string KeyName { get; }

        /// <summary>
        /// Friendly Name
        /// <br/> which can be assigned to any string by caller.
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// 搭配 PLC站號 (Fatek 以 1 為基底).
        /// </summary>
        byte StationID { get; }

        /// <summary>
        /// 位址型別編碼
        /// </summary>
        int CateID { get; }

        ///// <summary>
        ///// 位址型別
        ///// </summary>
        ///// <remarks>
        ///// <br/> Fatek: X, Y, M, R, (WX, WY, WM)
        ///// <br/> HCFA: I, Q, M, (IX, QB, MW)
        ///// </remarks>
        string Category { get; }

        /// <summary>
        /// 對應 Modbus 的屬性
        /// </summary>
        ModbusCateEnum GetModbusCategory();

        /// <summary>
        /// 位址號數
        /// </summary>
        ushort Address { get; }

        /// <summary>
        /// 內容物的位元長度
        /// </summary>
        byte Bits { get; }

        /// <summary>
        /// 1-Bit 點位 之 位元偏移數
        /// </summary>
        byte BitOffset { get; }

        /// <summary>
        /// 1-Bit 點位 之 位元進位數 
        /// </summary>
        /// <remarks>
        /// 每當 BitOffset >= BitStride,
        /// Address = Address + (BitOffset / BitStride)
        /// BitOffset = BitOffset % BitStride
        /// </remarks>
        byte BitStride { get; }

        /// <summary>
        /// 通訊用字串
        /// </summary>
        string ToCommString();

        /// <summary>
        /// 在同一個 Category 內的唯一序號
        /// (根據 Address, Bits, BitStride, BitOffset 運算出來)
        /// </summary>
        int UniqueID { get; }

        /// <summary>
        /// 位移定址
        /// </summary>
        IAddress Offset(int offset = 1, bool inplace = false);

        /// <summary>
        /// 跳躍定址
        /// </summary>
        IAddress JumpTo(int address, bool inplace = false);
    }
}
