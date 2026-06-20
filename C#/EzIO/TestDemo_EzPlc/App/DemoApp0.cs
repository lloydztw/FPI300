#region AUTHOR
/*
 * EzIO GUI
 * Copyright (C) 2026
 * 2026-04-11 created by LeTian Chang
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzComm;
using EzIO.Gui;
using EzIO.Mem;
using EzPlc.Fatek;
using System.Windows.Forms;

namespace TestDemo_EzPLC
{
    /// <summary>
    /// 簡單裸奔的演示
    /// </summary>
    internal class DemoApp0
    {
        public Form BuildFatekDemo(int comPort, bool isSim)
        {
            // RS232 settings
            var settings = new EzUartSettings { ComPort = comPort, IsSim = isSim, UsingRandomSim = isSim };

            // Fatek PLC Device
            var ioDevice = EzFatekFactory.OpenDevice(settings);

            // IoMemory
            var ioMem = ioDevice.GetFatekIoMemory();

            // 頻繁使用的點位, 用變量存起來, 可以提高效率.
            IoPoint ioEMG;          // 急停 
            IoPoint ioGrossForce;   // 壓力傳感器量測毛值

            // 建構 IoPoints
            var ioPoints = new[]
            {
                ioEMG = ioMem["M0"].SetDescription("急停").SetInverted(),
                ioMem["M1"].SetDescription("過壓保護"),
                ioMem["M3"].SetDescription("開始起測"),
                ioMem["M4"].SetDescription("光幕曾經被觸發"),
                ioMem["M5"].SetDescription("光幕 bypass"),
                ioMem["M9"].SetDescription("壓條感應 Sensor"),
                ioMem["R700"].SetDescription("壓力保護值"),
                ioGrossForce = ioMem["R2840"].SetDescription("傳感器毛值"),
            };

            // 強制關掉 急停
            ioEMG.Set(false);
            System.Diagnostics.Trace.WriteLine($"EMG = {ioEMG.IsOn}");

            // 讀取傳感器毛值
            uint grossForce = ioGrossForce.Data;
            System.Diagnostics.Trace.WriteLine($"GrossForce = {grossForce}");

            // 使用 AutoScan
            var autoScan = ioDevice.InstanceAutoScan(ioPoints);

            // GUI
            var frm = new FormIoPointsSimpleView();

            // EventHandlers
            frm.Load += (s, e) =>
            {
                frm.Attach(ioPoints, autoScan);
                autoScan?.Start();
            };

            frm.FormClosed += (s, e) =>
            {
                autoScan?.Stop();
                autoScan = null;
                ioDevice?.Dispose();
                ioDevice = null;
            };

            return frm;
        }
    }
}
