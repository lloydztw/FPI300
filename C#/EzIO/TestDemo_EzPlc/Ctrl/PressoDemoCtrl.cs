#region AUTHOR
/*
 * EzIO Demo
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

using EzIO.Device;
using EzIO.Gui;
using EzIO.Mem;
using EzIO.Sim;
using System;
using System.Drawing;
using System.Windows.Forms;
using TestDemo_Data;
using TestDemo_EzPLC.Gui;

namespace TestDemo_EzPLC.Ctrl
{
    internal class PressoDemoCtrl
    {
        #region GUI_LINKS
        IvPressoDemoView _ui;
        Form frmMain => _ui.frmMain;
        Control lblEMG => _ui.lblEMG;
        Control lblForce => _ui.lblCurrentForce;
        Button btnStart => _ui.btnStartTest;
        Button btnStop => _ui.btnStopTest;
        Button btnWrite => _ui.btnWriteProtectForce;
        NumericUpDown numForceP => _ui.numProtectForce;
        #endregion

        #region PRIVATE_KERNEL_DATA
        PressoModel _model;
        IDisposable _randomSimulator;
        #endregion

        public void Attach(IvPressoDemoView view, PressoModel model)
        {
            _ui = view;
            _model = model;
            frmMain.Load += (s, e) => Init();
        }

        IoPoint IoEMG => _model.IoEMG;
        IoPoint IoStart => _model.IoStartTest;
        IoPoint IoProtectForce => _model.IoProtectForce;
        IoPoint IoGrossForce => _model.IoGrossForce;

        private void Init()
        {
            OpenChildWindows();

            btnStart.Click += (s, e) => StartTest();
            btnStop.Click += (s, e) => StopTest();
            btnWrite.Click += (s, e) => WriteForceP();
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

            frm1.Attach(_model.IoPoints, _model.AutoScan);
            frm2.Attach(_model.IoPoints, _model.AutoScan);

            frm1.Show(frmMain);
            frm2.Show(frmMain);

            //frm1.Location = new Point(frmMain.Left, frmMain.Bottom);
            //frm2.Location = new Point(frm1.Right, frm1.Top);
            frm1.Location = new Point(frmMain.Right, frmMain.Top);
            frm1.Height = frmMain.Height;
            frm2.Location = new Point(frm1.Right, frm1.Top);
            frm2.Height = frmMain.Height;
        }

        private void ResetIoPoints()
        {
            IoEMG.Set(false);
            foreach (var ioPoint in _model.IoPoints)
            {
                if (ioPoint != IoEMG && ioPoint != IoProtectForce)
                    ioPoint.Set(0u);
            }
        }
        private void UpdatePlcDataToGui()
        {
            // 更新 EMG 顏色
            lblEMG.BackColor = IoEMG.IsOn ? Color.Red : frmMain.BackColor;

            // 更新 目前壓力值
            int force = (int)IoGrossForce.Data;
            int forceP = (int)IoProtectForce.Data;
            lblForce.Text = $"{force}";
            lblForce.ForeColor = force > forceP ? Color.Red : Color.Black;

            // 如果已經開始, 則更新 PLC 保護壓力值 到 GUI 畫面
            bool isStarted = IoStart.IsOn;
            if (isStarted || numForceP.Tag == null)
            {
                numForceP.Value = forceP;
                numForceP.Tag = "InitOK";
            }

            // 如果是離線模擬, 啟動隨機亂數模擬器
            if (isStarted && isOfflineSimulation())
                startRandomSim();
        }
        private void WriteForceP()
        {
            var force = (uint)numForceP.Value;
            IoProtectForce.Write(force);
        }
        private void StartTest()
        {
            IoStart.Set(true);
        }
        private void StopTest()
        {
            IoStart.Set(false);

            if (isRandomSimRunning())
            {
                System.Threading.Thread.Sleep(500);
                stopRandomSim();
            }

            ResetIoPoints();
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
                // 使用另一個線呈執行 autoScan.Stop(), 讓前景感覺不到阻塞.
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
            var msg = "AutoScan 指令 (Fatek 每條通訊指令約耗時 8ms) :\n";
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
                var randomSim = new IoRandomSimulator(_model.IoPoints);
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
