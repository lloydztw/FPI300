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
        IPlcAtm20 _plc;
        IDisposable _randomSimulator;
        #endregion

        public void Init(Form frmOwner, IoPointsView view)
        {
            _plc = Global.Machine?.PLC;
            bindIoPoints(view);
            frmOwner.FormClosing += (s, e) => CloseFormElegantly(e);
        }

        void bindIoPoints(IoPointsView view)
        {
            var autoScan = _plc?.AutoScan;

            view.Attach(_plc.IterIoPoints(true), autoScan);

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

        private void CleanUp()
        {
            stopRandomSim();
            _plc?.AutoScan?.Stop();
            if (_plc is IDisposable d) 
                d.Dispose();
            _plc = null;
        }

        /// <summary>
        /// 優雅的關閉主視窗
        /// </summary>
        private void CloseFormElegantly(FormClosingEventArgs e)
        {
            var plc = Global.Machine.PLC;
            var autoScan = plc?.AutoScan;

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
            var device = _plc?.Device;
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
                var randomSim = new IoRandomSimulator(_plc.IoMem.IterPoints());
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
