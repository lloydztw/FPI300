#region AUTHOR
/*
 * EzIO Demo
 * 
 * Copyright (C) 2026
 * 
 * 2026-04-25 created by LeTian Chang
 * 
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzIO.Device;
using EzIO.Gui;
using EzIO.Mem;
using EzIO.Sim;
using EzPlc.Omron;
using System;
using System.Drawing;
using System.Windows.Forms;
using TestDemo_EzPLC.Gui;
using Traveller106.IO;

namespace TestDemo_EzPLC.Ctrl
{
    internal class EzTravellerDemoCtrl
    {
        #region GUI_LINKS
        IvTravellerDemoView _ui;
        Form frmMain => _ui.frmMain;
        Control lblSync => _ui.lblSync;
        Button btnStart => _ui.btnStartTest;
        Button btnStop => _ui.btnStopTest;
        Button btnWrite => _ui.btnWrite;
        NumericUpDown numFPOS => _ui.numFPOS;
        NumericUpDown numRPOS => _ui.numRPOS;
        #endregion

        #region PRIVATE_KERNEL_DATA
        FPIX3_IO _model;
        IDisposable _randomSimulator;
        #endregion

        public void Attach(IvTravellerDemoView view, FPIX3_IO model)
        {
            _ui = view;
            _model = model;
            frmMain.Load += (s, e) => Init();
        }

        IoPoint IoSync => _model.GetBaseIO().bSyncClock;
        IoPoint IoStart => _model.GetBaseIO().bSoftwareReady;
        OmronIoVarFloat32 IoFPOS => _model.GetAxisIO(0).rFPOS;
        OmronIoVarFloat32 IoRPOS => _model.GetAxisIO(0).rRPOS;

        private void Init()
        {
            OpenChildWindows();

            btnStart.Click += (s, e) => StartTest();
            btnStop.Click += (s, e) => StopTest();
            btnWrite.Click += (s, e) => WritePositions();
            frmMain.FormClosing += (s, e) => CloseFormElegantly(e);

            var autoScan = _model.AutoScan;
            if (autoScan != null)
            {
                autoScan.OnScanned += (s, e) => frmMain.Invoke(new Action(() => UpdatePlcDataToGui()));
                autoScan.Start();
            }

            updateAutoScanInfo(autoScan);
        }
        private void OpenChildWindows()
        {
            var frm1 = new FormIoPointsDataGridView();
            var frm2 = new FormIoPointsSimpleView();

            var ioMem = _model.IoMem;
            var ioPoints = _model.IoMem.GetAllPoints();

            // 只顯示 1-Bit 的 點位 
            ioPoints.RemoveAll(p => p.Address.Bits != 1);

            frm1.Attach(ioPoints, _model.AutoScan);
            frm2.Attach(ioPoints, _model.AutoScan);

            frm1.Show(frmMain);
            frm2.Show(frmMain);

            frm1.Location = new Point(frmMain.Right, frmMain.Top);
            frm1.Height = frmMain.Height;
            frm2.Location = new Point(frm1.Right, frm1.Top);
            frm2.Height = frmMain.Height;
        }

        private void ResetIoPoints()
        {
            foreach (var ioPoint in _model.IoMem.IterPoints())
            {
                if (ioPoint != IoSync && ioPoint != IoFPOS && ioPoint != IoRPOS)
                    ioPoint.Set(0u);
            }
        }
        private void UpdatePlcDataToGui()
        {
            // 更新 Sync 顏色
            lblSync.BackColor = IoSync.IsOn ? Color.Lime : frmMain.BackColor;

            // 如果已經開始, 則更新 GlueTime 與 UVTime 到 GUI 畫面
            bool isStarted = IoStart.IsOn;
            if (isStarted || numFPOS.Tag == null)
            {
                setNum(numRPOS, IoFPOS.Value);
                setNum(numFPOS, IoRPOS.Value);
                numFPOS.Tag = "InitOK";
                numRPOS.Tag = "InitOK";
            }

            // 如果是離線模擬, 啟動隨機亂數模擬器
            if (isStarted && isOfflineSimulation())
                startRandomSim();
        }
        private void StartTest()
        {
            IoStart?.Set(true);
        }
        private void StopTest()
        {
            IoStart?.Set(false);

            if (isRandomSimRunning())
            {
                System.Threading.Thread.Sleep(500);
                stopRandomSim();
            }

            ResetIoPoints();
        }
        private void WritePositions()
        {
            IoFPOS?.Set((float)numFPOS.Value);
            IoRPOS?.Set((float)numRPOS.Value);
        }
        private void CleanUp()
        {
            stopRandomSim();
            _model?.AutoScan?.Stop();
            _model?.Dispose();
            _model = null;
        }

        /// <summary>
        /// 優雅的關閉主視窗
        /// </summary>
        private void CloseFormElegantly(FormClosingEventArgs e)
        {
            var autoScan = _model?.AutoScan;

            if (autoScan != null && autoScan.IsRunning())
            {
                // 使用另一個線呈 執行 autoScan.Stop(), 讓前景感覺不到阻塞.
                new Action(() => autoScan.Stop()).BeginInvoke(null, null);
                
                // 訊息窗
                var ret = MessageBox.Show("請等待 IoDevice 自動掃描 結束後, 再次關閉.", "Demo", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                
                // 如果訊息窗結束後還沒停止
                if (autoScan.IsRunning() && ret != DialogResult.Cancel)
                {
                    if (ret == DialogResult.OK)
                    {
                        // 取消關閉程序.
                        e.Cancel = true;
                        return;
                    }
                    else
                    {
                        // 等待 1 秒後, 直接進行關閉.
                        System.Threading.Thread.Sleep(1000);
                    }
                }
            }

            CleanUp();
        }

        private void updateAutoScanInfo(IAutoScan autoScan)
        {
            if (autoScan == null) return;
            var msg = "AutoScan 所用到的 IoPoints :\n";
            msg += autoScan.ToString();
            _ui.lblInfo.Text = msg;
        }
        private bool isOfflineSimulation()
        {
            var device = _model?.Device;
            return device == null || device.IsSim;
        }
        private bool isRandomSimRunning()
        {
            return _randomSimulator != null;
        }
        private void startRandomSim()
        {
            if (_randomSimulator == null)
            {
                var randomSim = new IoRandomSimulator(_model.IoMem.IterPoints());
                _randomSimulator = randomSim;
                randomSim.Start(100);
            }
        }
        private void stopRandomSim()
        {
            _randomSimulator?.Dispose();
            _randomSimulator = null;
        }

        private void setNum(NumericUpDown num, float value)
        {
            if (num == null) return;
            value = Math.Max((float)num.Minimum, value);
            value = Math.Min((float)num.Maximum, value);
            num.Value = (decimal)value;
        }
    }
}
