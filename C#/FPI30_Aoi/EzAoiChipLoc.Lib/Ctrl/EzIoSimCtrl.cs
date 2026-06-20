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

using EzAoiChipLocQC.Drivers.IO;
using EzIO.Gui;
using EzIO.Sim;
using System;
using System.Windows.Forms;

namespace EzAoiChipLocQC.Ctrl
{
    internal class EzIoSimulation
    {
        #region PRIVATE_KERNEL_DATA
        Atm20_IO _IO;
        IDisposable _randomSimulator;
        #endregion

        //IoPoint IoSync => _model?.GetBaseIO().bSyncClock;
        //IoPoint IoStart => _model?.GetBaseIO().bSoftwareReady;
        //OmronIoVarFloat32 IoFPOS => _model?.GetAxisIO(0).rFPOS;
        //OmronIoVarFloat32 IoRPOS => _model?.GetAxisIO(0).rRPOS;

        public void Init(Form frmOwner, IoPointsView view)
        {
            var plc = Global.Machine.PLC as Atm20_PLC;
            _IO = (Atm20_IO)plc;

            frmOwner.FormClosing += (s, e) => CloseFormElegantly(e);

            bindIoPoints(view);
        }

        void bindIoPoints(IoPointsView view)
        {
            //var machine = Global.Machine;
            //var plc = Global.Machine?.PLC;
            //if (plc == null)
            //    return;

            var plc = _IO;
            var ioMem = plc.IoMem;
            var ioPoints = plc.IoMem.GetAllPoints();
            var autoScan = plc.AutoScan;

            // 只顯示 1-Bit 的 點位 
            ioPoints.RemoveAll(p => p.Address.Bits != 1);
            
            // 不顯示馬達軸態
            ioPoints.RemoveAll(p => p.Description!=null && p.Description.Contains("軸"));

            view.Attach(ioPoints, autoScan);

            //var autoScan = _IO?.AutoScan;

            if (autoScan != null)
            {
                //autoScan.OnScanned += (s, e) => frmMain.Invoke(new Action(() => UpdatePlcDataToGui()));
                autoScan.Start();

                // 如果是離線模擬, 啟動隨機亂數模擬器
                if (isOfflineSimulation())
                    startRandomSim();
            }

            autoScan?.Start();
        }

#if (false)
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
            //// 更新 Sync 顏色
            //lblSync.BackColor = IoSync.IsOn ? Color.Lime : frmMain.BackColor;

            //// 如果已經開始, 則更新 GlueTime 與 UVTime 到 GUI 畫面
            //bool isStarted = IoStart.IsOn;
            //if (isStarted || numFPOS.Tag == null)
            //{
            //    setNum(numRPOS, IoFPOS.Value);
            //    setNum(numFPOS, IoRPOS.Value);
            //    numFPOS.Tag = "InitOK";
            //    numRPOS.Tag = "InitOK";
            //}

            //// 如果是離線模擬, 啟動隨機亂數模擬器
            //if (isStarted && isOfflineSimulation())
            //    startRandomSim();
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
#endif

        private void CleanUp()
        {
            stopRandomSim();
            _IO?.AutoScan?.Stop();
            _IO?.Dispose();
            _IO = null;
        }

        /// <summary>
        /// 優雅的關閉主視窗
        /// </summary>
        private void CloseFormElegantly(FormClosingEventArgs e)
        {
            var autoScan = _IO?.AutoScan;

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

#if (false)
        private void updateAutoScanInfo(IAutoScan autoScan)
        {
            if (autoScan == null) return;
            var msg = "AutoScan 所用到的 IoPoints :\n";
            msg += autoScan.ToString();
            _ui.lblInfo.Text = msg;
        }
#endif
        private bool isOfflineSimulation()
        {
            var device = _IO?.Device;
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
                var randomSim = new IoRandomSimulator(_IO.IoMem.IterPoints());
                _randomSimulator = randomSim;
                randomSim.Start(100);
            }
        }
        private void stopRandomSim()
        {
            _randomSimulator?.Dispose();
            _randomSimulator = null;
        }
    }
}
