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

using EzIO.Device;
using EzIO.Gui;
using EzIO.Mem;
using JetEazy.QUtilities;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace EzIO.Ctrl
{
    public class AutoScanUiCtrl
    {
        #region PRIVATE_DATA
        QFrameRateMeter _fps = new QFrameRateMeter();
        #endregion

        public void Attach(IoPointsView view, IEnumerable<IoPoint> ioPoints, IAutoScan autoScan)
        {
            if (view == null || ioPoints == null)
                return;

            view.Update(ioPoints);

            if (autoScan == null)
                return;

            autoScan.OnScanned += (s, e) =>
            {
                var wnd = view.Window;
                if (wnd.IsHandleCreated)
                {
                    wnd.Invoke(new Action(() =>
                    {
                        view.Update(ioPoints);
                        updateSamplingRate(view.lblSamplingRate);
                    }));
                }
            };
        }

        #region PRIVATE_DATA
        void updateSamplingRate(Control lblSamplingRate)
        {
            if (lblSamplingRate == null || !lblSamplingRate.Visible)
                return;

            uint count = (++_fps).TotalFramesCount;
            if (count % 10 == 0)
            {
                lblSamplingRate.Text = $"Scan Interval = {1000.0 / _fps.FrameRate:0.0} ms";
            }

            if (count > 50000000)
                _fps.Reset();
        }
        #endregion
    }
}


