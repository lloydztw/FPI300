#region AUTHOR
/*
 * EzGlueDispenser.IO.S3
 * 
 * Copyright (C) 2026
 * 
 * 2026-04-22 revised by LeTian Chang
 * 
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzComm;
using EzIO.Device;
using EzIO.Mem;
using EzPlc.Hcfa;
using System;
using S3PointEnum = EzGlueDispenser.IO.S3.EzPointEnum;

namespace EzGlueDispenser.IO.S3
{
    /// <summary>
    /// 簡單模擬 點膠機 S3 所用到的 IO點位 Interface
    /// </summary>
    public interface IGlueDispenserS3
    {
        IoPoint this[S3PointEnum id] { get; }
        IoPoint IoEMG { get; }
        IoPoint IoGlueTestStart { get; }
        IoPoint IoGlueTime { get; }
        IoPoint IoUVTime { get; }
    }


    /// <summary>
    /// 簡單模擬 點膠機 S3 IO點位 載體 (PLC)
    /// </summary>
    public class EzGlueDispenserIoS3 : IGlueDispenserS3, IDisposable
    {
        #region PRIVATE_DEVICE_DATA
        IoDevice _ioDevice;
        IAutoScan _autoScan;
        #endregion

        #region IO_POINTS
        S3_IoTable _ioTable;
        #endregion

        public EzGlueDispenserIoS3(EzTcpIpSettings settings)
        {
            // PLC Device
            _ioDevice = EzHcfaFactory.OpenDevice(settings);

            // IoMemory
            this.IoMem = _ioDevice.GetHcfaIoMemory();

            // 建構 IoPoints
            buildPoints(this.IoMem);

            // 使用 AutoScan
            _autoScan = _ioDevice.InstanceAutoScan(null, 512);
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
        public IoMemory IoMem
        {
            get; 
            private set;
        }
        public IAutoScan AutoScan
        {
            get => _autoScan;
        }

        void buildPoints(IoMemory ioMem)
        {
            // 建構 IoPoints
            _ioTable = new S3_IoTable();
            _ioTable.Init(ioMem);

            // 保存常用的 點位

            // 急停
            IoEMG = _ioTable[S3PointEnum.EMG];

            // 一鍵點膠測試 (ON启动 OFF完成)
            IoGlueTestStart = _ioTable[S3PointEnum.QB1551];
            IoGlueTime = _ioTable[S3PointEnum.MW1091];
            IoUVTime = _ioTable[S3PointEnum.MW1092];
        }

        public IoPoint this[S3PointEnum id]
        {
            get => _ioTable[id];
        }
        public IoPoint IoEMG
        {
            get; 
            private set;
        }
        public IoPoint IoGlueTestStart
        {
            get;
            private set;
        }
        public IoPoint IoGlueTime
        {
            get;
            private set;
        }
        public IoPoint IoUVTime
        {
            get;
            private set;
        }
    }
}
