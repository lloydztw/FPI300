#region AUTHOR
/*
 * Traveller106.IO
 * 
 * Copyright (C) 2026
 * 
 * 2026-04-24 created by LeTian Chang
 * 
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzComm;
using EzIO.Device;
using EzIO.Mem;
using EzPlc.Omron;
using System;

namespace Traveller106.IO
{
    /// <summary>
    /// Traveller106 IO 點位 載體 (Omron PLC)
    /// </summary>
    public partial class FPIX3_IO : IDisposable
    {
        #region PRIVATE_DEVICE_MEMBERS
        IoDevice _ioDevice;
        IAutoScan _autoScan;
        #endregion

        public FPIX3_IO(EzTcpIpSettings settings)
        {
            // PLC Device
            _ioDevice = EzOmronFactory.OpenDevice(settings);

            // IoMemory
            IoMem = _ioDevice.GetOmronIoMemory();

            // 建構 IoPoints
            BuildIoPoints(IoMem);

            // 建立 AutoScan
            _autoScan = _ioDevice.InstanceAutoScan(GetAutoScanPoints());
        }
        public void Dispose()
        {
            _autoScan?.Stop();
            _autoScan = null;

            _ioDevice?.Dispose();
            _ioDevice = null;
        }

        public IoDevice Device
        {
            get => _ioDevice;
        }
        public IAutoScan AutoScan
        {
            get => _autoScan;
        }
        public IoMemory IoMem
        {
            get;
            private set;
        }

        #region OMRON_SPECIFIC_FUNCTIONS
        /// <summary>
        /// Omron Specific Functions (歐姆龍直通函式)
        /// </summary>
        public void WriteArray<T>(string stemName, T[] array)
        {
            if (array == null || string.IsNullOrEmpty(stemName))
                return;

            stemName = DronVariables.VarName(stemName);
            if (!stemName.Contains("{0}"))
                stemName += "[{0}]";

            if (_ioDevice is OmronIoDevice omron)
            {
                for (int i = 0, N = array.Length; i < N; i++)
                {
                    var varName = string.Format(stemName, i);
                    omron.WriteVariable(varName, array[i].ToString());
                }
            }
        }
        /// <summary>
        /// Omron Specific Functions (歐姆龍直通函式)
        /// </summary>
        public void WriteOne<T>(string varName, T value)
        {
            if (_ioDevice is OmronIoDevice omron && !string.IsNullOrEmpty(varName))
            {
                varName = DronVariables.VarName(varName);
                omron.WriteVariable(varName, value);
            }
        }
        #endregion
    }
}
