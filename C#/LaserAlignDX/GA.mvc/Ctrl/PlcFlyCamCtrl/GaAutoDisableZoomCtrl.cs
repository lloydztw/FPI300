#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-09-11 開始適用於 2.6.x.x 以後的版本
 *      2025-08-29 開始準備重整 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System;
using System.Windows.Forms;


namespace LaserAlignDX.Mvc.Ctrl.V3
{
    /// <summary>
    /// 飛拍過程中, 自動關閉 Zoom In Out
    /// </summary>
    public partial class GaAutoDisableZoomCtrl
    {
        #region CONFIG
        const int TIME_INTERVAL_SECONDS = 5;
        bool IS_AUTO_DISABLE => Traveller106.INI.Instance.IsAutoDisableZoom;
        #endregion

        #region GUI_MEMBERS
        Control _wndOwner;
        Control[] _viewers;
        Timer _timerPolling = new Timer();
        int _pollingCountDown = 0;
        #endregion

        public void Attach(GaPlcFlyCameraCtrl flyCtrl, params Control[] viewers)
        {
            _wndOwner = viewers[0].Parent;
            _viewers = viewers;

            flyCtrl.OnFlyStarted += _flyCtrl_OnFlyStarted;
            flyCtrl.OnFlyDone += _flyCtrl_OnFlyDone;
            _timerPolling.Tick += _timPolling_FlyStartOff;

            _timerPolling.Interval = 1000;
            _pollingCountDown = 1000;

            _wndOwner.HandleDestroyed += (s, e) =>
            {
                _timerPolling?.Stop();
                _timerPolling?.Dispose();
                _timerPolling = null;
            };
        }

        #region EVENT_HANDLERS
        private void _flyCtrl_OnFlyStarted(object sender, EventArgs e)
        {
            if (!IS_AUTO_DISABLE)
                return;

            if (_wndOwner == null)
                return;

            if (_wndOwner.InvokeRequired)
            {
                _wndOwner?.Invoke((EventHandler)_flyCtrl_OnFlyStarted);
            }
            else
            {
                foreach(var wnd in _viewers)
                    wnd.Enabled = false;
                _timerPolling.Enabled = false;
                _timerPolling.Enabled = true;
                _pollingCountDown = TIME_INTERVAL_SECONDS;
            }
        }
        private void _flyCtrl_OnFlyDone(object sender, EventArgs e)
        {
            if (!IS_AUTO_DISABLE)
                return;

            if (_wndOwner == null)
                return;

            if (_wndOwner.InvokeRequired)
            {
                _wndOwner?.Invoke((EventHandler)_flyCtrl_OnFlyDone);
            }
            else
            {
                _timerPolling.Enabled = true;
                _pollingCountDown = TIME_INTERVAL_SECONDS;
            }
        }
        private void _timPolling_FlyStartOff(object sender, EventArgs e)
        {
            if (!IS_AUTO_DISABLE)
                return;

            if (_pollingCountDown <= 0)
            {
                _timerPolling.Enabled = false;
                foreach (var wnd in _viewers)
                    wnd.Enabled = true;
            }
            else
            {
                _pollingCountDown--;
            }
        }
        #endregion
    }
}
