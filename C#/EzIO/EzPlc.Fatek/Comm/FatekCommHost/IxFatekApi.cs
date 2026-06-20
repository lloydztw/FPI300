#region AUTHOR
/*
 * EzPlc.Fatek
 * Copyright (C) 2026
 * 2026-04-05 LeTian Chang: Integration with EzIO
 * 2012-12-04 LeTian Chang: ReOpen UART when communication failed.
 * 2012-06-22 LeTian Chang: Revised for more robust over RS232 connection.
 * 2008-07-01 LeTian Chang: Creation.
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System;


namespace EzPlc.Fatek.Comm
{
    /// <summary>
    /// 舊有的 Fatek 通訊介面 (blocked calls)
    /// </summary>
    public interface IxFatekApi
    {
        /// <summary>
        /// 0x4E
        /// </summary>
        string Echo(string message);

        /// <summary>
        /// 0x40
        /// </summary>
        bool IsRunning();

        /// <summary>
        /// 0x41
        /// </summary>
        void Run();

        /// <summary>
        /// 0x41
        /// </summary>
        void Stop();

        /// <summary>
        /// {X/Y/M}1234
        /// </summary>
        /// <param name="address">{X/Y/M}1234</param>
        void SetSinglePoint(string address, bool on);

        /// <summary>
        /// {X/Y/M}1234
        /// </summary>
        /// <param name="address">{X/Y/M}1234</param>
        bool ReadSinglePoint(string address);

        /// <summary>
        /// [D]W{X/Y/M}1234 or [D]{R/D}12345
        /// </summary>
        /// <param name="address">[D]W{X/Y/M}1234 or [D]{R/D}12345</param>
        void WriteRegister(string address, uint data);

        /// <summary>
        /// [D]W{X/Y/M}1234 or [D]{R/D}12345
        /// </summary>
        /// <param name="address">[D]W{X/Y/M}1234 or [D]{R/D}12345</param>
        void WriteRegisters(string address, uint[] data);

        /// <summary>
        /// [D]W{X/Y/M}1234 or [D]{R/D}12345
        /// </summary>
        /// <param name="address">[D]W{X/Y/M}1234 or [D]{R/D}12345</param>
        uint ReadRegister(string address);

        /// <summary>
        /// [D]{R/D}12345
        /// </summary>
        short ReadRegisterI16(string address);

        /// <summary>
        /// [D]W{X/Y/M}1234 or [D]{R/D}12345
        /// </summary>
        void ReadRegisters(string address, int number, Action<int, uint> updateFunc);

        /// REV_20191125 測試 ReadMixPoints
        /// <summary>
        /// [DW/W]{X/Y/M}1234 or [D]{R/D}12345
        /// </summary>
        void ReadMixPoints(string[] addressGrp, Action<object, int, uint> updateFunc);
    }
}
