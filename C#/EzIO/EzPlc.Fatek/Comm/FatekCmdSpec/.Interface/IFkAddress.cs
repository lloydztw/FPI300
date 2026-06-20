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
    public interface IFkAddress
    {
        /// <summary>
        /// 將 位址 轉換成 Fatek RS232 通訊格式
        /// </summary>
        string ToCommString();

        /// <summary>
        /// 類型 <br/>
        /// X, Y, M (1 bit)<br/>
        /// [D] R  (32/16 bit)<br/>
        /// [D] D  (32/16 bit)<br/>
        /// [D] WX (32/16 bit)<br/>
        /// [D] WY (32/16 bit)<br/> 
        /// [D] WM (32/16 bit)<br/>
        /// </summary>
        string Category { get; }

        ushort Address { get; }

        byte Bits { get; }
    }
}
