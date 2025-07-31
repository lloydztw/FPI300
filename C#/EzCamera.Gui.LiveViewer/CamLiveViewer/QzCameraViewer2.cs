#region AUTHOR
/*
 * 
 * Copyright (c) 2024 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2024-04-25 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzCamera.Interface;
using JetEazy.ImageViewerQx;
using JetEazy.QUtilities;
using System;
using System.ComponentModel;
using System.Drawing;

namespace JetEazy.GUI
{
    public class QzCameraViewer : QvImageViewer, IvCameraViewer
    {
        #region PRIVATE_MEMBERS
        private IEzCamera _camera = null;
        private QFrameRateGauge _frameRateGauge;
        #endregion

        public QzCameraViewer()
        {
            _frameRateGauge = base.FrameRateGauge;
        }

        [Browsable(false)]
        public IEzCamera Camera
        {
            get { return _camera; }
        }
        public void AttachLiveSource(IEzCamera camera)
        {
            try
            {
                if (_camera != camera && camera != null)
                {
                    DetachLiveSource();

                    bool bFlag = base.Enabled;
                    base.Enabled = false;

                    camera.OnLiveModeChanged += _camera_OnLiveModeChanged;
                    camera.OnLiveImage += _camera_OnLiveImage;
                    camera.OnError += _camera_OnError;
                    _camera = camera;

                    base.Enabled = bFlag;
                    _frameRateGauge.Reset();
                }
            }
            catch
            {
            }
        }
        public void DetachLiveSource()
        {
            try
            {
                var camera = _camera;

                _camera = null;
                if (camera != null)
                {
                    camera.StopLiveMode();
                    camera.OnLiveModeChanged -= _camera_OnLiveModeChanged;
                    camera.OnLiveImage -= _camera_OnLiveImage;
                    camera.OnError -= _camera_OnError;
                }

                //m_stillImageFps.Reset();
                //m_vZoomer.FrameRateGauge?.Reset();
                _frameRateGauge.Reset();
            }
            catch
            {
            }
        }

        #region CAMERA_EVENT_HANDLERS
        private void _camera_OnLiveModeChanged(object sender, EventArgs e)
        {
            BeginInvoke(new Action(() =>
            {
                if (_camera != null || !_camera.IsLiveMode())
                {
                    base.FrameRateGauge?.Reset();
                }
            }));
        }
        private void _camera_OnLiveImage(object sender, EzLiveImageEventArgs e)
        {
            try
            {
                if (!(e.LiveImage is Bitmap bmpLive))
                    return;
                base.CopyFrom(bmpLive);
                base.FrameRateGauge?.IncreaseFrame();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"{GetType().Name}._OnLiveImage: {ex.Message}");
            }
        }
        private void _camera_OnError(object sender, EzCameraErrorEventArgs e)
        {
            string errMsg = (e.ErrCode != EzCameraError.NoError) ?
                $"[{e.ErrCode}] {e.Message}" : null;
            base.ShowError(errMsg, 0);
        }
        protected override bool _isLive()
        {
            return _camera != null && _camera.IsLiveMode();
        }
        #endregion
    }
}
