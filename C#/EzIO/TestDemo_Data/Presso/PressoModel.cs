#region AUTHOR
/*
 * EzIO Test Data
 * 
 * Copyright (C) 2026
 * 
 * 2026-04-11 created by LeTian Chang
 * 
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzComm;
using EzIO.Device;
using EzIO.Mem;
using EzPlc.Fatek;
using System;

namespace TestDemo_Data
{
    /// <summary>
    /// 簡單模擬 壓合機 所用到的 IO 點位 Interface
    /// </summary>
    public interface IPressoIO
    {
        IoPoint IoEMG { get; }
        IoPoint IoStartTest { get; }
        IoPoint IoProtectionEnable { get; }
        IoPoint IoLightGateDetected { get; }
        IoPoint IoLightGateBypass { get; }
        IoPoint IoPressBarSensor { get; }
        IoPoint IoGrossForce { get; }
        IoPoint IoProtectForce { get; }
    }


    /// <summary>
    /// 簡單模擬 壓合機 IO 點位 載體 (PLC)
    /// </summary>
    public class PressoModel : IPressoIO, IDisposable
    {
        #region PRIVATE_DATA
        IoDevice _ioDevice;
        IAutoScan _autoScan;
        #endregion

        public PressoModel(int comPort, bool isSim)
        {
            // RS232
            var settings = new EzUartSettings { ComPort = comPort, IsSim = isSim };

            // Fatek PLC Device
            _ioDevice = EzFatekFactory.OpenDevice(settings);

            // IoMemory
            var ioMem = _ioDevice.GetFatekIoMemory();
            this.IoMem = ioMem;

            // 建構 IoPoints
            buildPoints(ioMem);

            // 強制關掉 急停警報
            IoEMG.Set(false);

            // 使用 AutoScan
            _autoScan = _ioDevice.InstanceAutoScan(IoPoints);
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
            IoPoints = new[]
            {
                IoEMG =                 ioMem["M0"].SetDescription("急停").SetInverted(),
                IoProtectionEnable =    ioMem["M1"].SetDescription("過壓保護"),
                IoStartTest =           ioMem["M3"].SetDescription("開始起測"),
                IoLightGateDetected =   ioMem["M4"].SetDescription("光幕曾經被觸發"),
                IoLightGateBypass =     ioMem["M5"].SetDescription("光幕 bypass"),
                IoPressBarSensor =      ioMem["M9"].SetDescription("壓條感應 Sensor"),
                IoProtectForce =        ioMem["R700"].SetDescription("壓力保護值"),
                IoGrossForce =          ioMem["R2840"].SetDescription("傳感器毛值"),
            };
        }
        public IoPoint[] IoPoints
        {
            get;
            private set;
        }
        public IoPoint IoEMG
        {
            get;
            private set;
        }
        public IoPoint IoStartTest
        {
            get;
            private set;
        }
        public IoPoint IoProtectionEnable
        {
            get;
            private set;
        }
        public IoPoint IoLightGateDetected
        {
            get;
            private set;
        }
        public IoPoint IoLightGateBypass
        {
            get;
            private set;
        }
        public IoPoint IoPressBarSensor
        {
            get; private set;
        }
        public IoPoint IoGrossForce
        {
            get; private set;
        }
        public IoPoint IoProtectForce
        {
            get; private set;
        }
    }
}
