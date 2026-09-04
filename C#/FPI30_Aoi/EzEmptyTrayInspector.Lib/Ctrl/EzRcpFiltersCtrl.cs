#region AUTHOR
/*
 * 
 * Copyright (c) 2024 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2024-10-03 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzAoiEmptyTrayInspector.Gui;
using EzAoiEmptyTrayInspector.Model;
using EzCamera.Driver.Utils;
using JetEazy.EzImage;
using OpenCvSharp;
using System;
using System.Windows.Forms;

namespace EzAoiEmptyTrayInspector.Ctrl
{
    internal class EzRcpFiltersCtrl : BaseUtil, IDisposable
    {
        #region PRIVATE_RUNTIME_DATA
        JxImagePreSettings _jxImagePreSettings;
        IEzImage _imgSource;
        bool _isRcpEdittingMode = false;
        #endregion

        #region PRIVATE_GUI_MEMBERS
        Control _wndRcpHostPanel;
        FormImageViewer _frmImageViewer;
        Timer _restoreTimer;
        #endregion

        #region PRIVATE_KERNEL_MEMBERS
        EzImageProcess _imgProcesser = new EzImageProcess();
        #endregion

        public EzRcpFiltersCtrl(Control wndHost, JxImagePreSettings jxImagePreSettings)
        {
            _wndRcpHostPanel = wndHost;
            _jxImagePreSettings = jxImagePreSettings;
            init_event_handlers();
        }
        public void Dispose()
        {
            CleanUp();
        }

        public void AttachImageSource(IEzImage imgSrc)
        {
            _imgSource = imgSrc;
        }
        public bool IsEditting
        {
            get => _isRcpEdittingMode;
            set
            {
                _isRcpEdittingMode= value;
            }
        }

        #region EVENT_HANDLERS
        void init_event_handlers()
        {
            connect_recipe_prop_handlers();
            _wndRcpHostPanel.HandleDestroyed += (s,e) => CleanUp();
        }
        void connect_recipe_prop_handlers()
        {
            if(_jxImagePreSettings!=null)
            {
                _jxImagePreSettings.OnModified += _jxImagePreSettings_OnModified;
            }
        }
        void disconnect_recipe_prop_handlers()
        {
            if (_jxImagePreSettings != null)
            {
                _jxImagePreSettings.OnModified -= _jxImagePreSettings_OnModified;
                _jxImagePreSettings = null;
            }
        }
        void _jxImagePreSettings_OnModified(object sender, EventArgs e)
        {
            if (!_isRcpEdittingMode)
                return;
            ApplyFilters(_imgSource?.Image as Mat);
        }
        #endregion

        void CleanUp()
        {
            disconnect_recipe_prop_handlers();
            closeViewer(0);
            disposeRestoreTimer();
        }
        void ApplyFilters(Mat imgSrc)
        {
            if (imgSrc == null)
            {
                closeViewer(0);
                return;
            }

            int b = (int)_jxImagePreSettings.Brightness.Value;
            int c = (int)_jxImagePreSettings.Contrast.Value;
            var imgFiltered = imgSrc.Clone();
            _imgProcesser.ApplyBrightnessContrast(imgFiltered, b, c);
            showViewer(imgFiltered, "Filtered Image", true);
        }

        #region PRIVATE_FUNCTIONS
        void showViewer(Mat img, string name, bool disposeSrc, int autoCloseDelay = 5000)
        {
            if (img == null)
            {
                closeViewer(0);
            }
            else
            {
                if (_frmImageViewer == null)
                {
                    _frmImageViewer = new FormImageViewer { TopMost = true };
                    _frmImageViewer.Show(_wndRcpHostPanel);
                }
                _wndRcpHostPanel.BeginInvoke(new Action(() =>
                {
                    _frmImageViewer.UpdateImage(img, name, disposeSrc);
                    closeViewer(autoCloseDelay);
                }));
            }
        }
        void closeViewer(int delay = 5000)
        {
            if (delay <= 0)
            {
                _frmImageViewer?.Dispose();
                disposeRestoreTimer();
                return;
            }

            if (_restoreTimer == null)
            {
                _restoreTimer = new Timer();
                _restoreTimer.Tick += (s, e) =>
                {
                    _restoreTimer.Stop();
                    _frmImageViewer?.Dispose();
                };
            }

            _restoreTimer?.Stop();
            _restoreTimer.Interval = delay;
            _restoreTimer?.Start();
        }
        void disposeRestoreTimer()
        {
            _restoreTimer?.Stop();
            _restoreTimer?.Dispose();
            _restoreTimer = null;
        }
        #endregion
    }
}
