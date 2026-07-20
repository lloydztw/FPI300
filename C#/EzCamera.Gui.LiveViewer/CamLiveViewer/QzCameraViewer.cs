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
using System;
using System.ComponentModel;
using System.Drawing;

namespace EzCamera.GUI
{
    public class QzCameraViewer : QzImageViewer, IvCameraViewer
    {
        #region PRIVATE_MEMBERS
        private IEzCamera _camera = null;
        #endregion

        public QzCameraViewer()
        {
            InitializeComponent();
            SizeChanged += (s, e) => adjustErrorLabelPos();
            lblWarning.DoubleClick += LblWarning_DoubleClick;
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
                    base.resetFrameRateGauge();
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
                base.resetFrameRateGauge();
            }
            catch
            {
            }
        }

        #region CAMERA_EVENT_HANDLERS
        private void _camera_OnLiveModeChanged(object sender, EventArgs e)
        {
            this.BeginInvoke(new Action(() =>
            {
                if(_camera!=null || !_camera.IsLiveMode())
                {
                    base.resetFrameRateGauge();
                    //this.Invalidate();
                }
            }));
        }
        private void _camera_OnLiveImage(object sender, EzLiveImageEventArgs e)
        {
            try
            {
                var bmpLive = (Bitmap)e.LiveImage;
                if (bmpLive == null)
                    return;

                //(1) Draw parent's Default Contents
                base.Lock();
                base.drawDefaultLiveContents(bmpLive);
                base.Unlock();

                //(2) Clone bmpLive to the display Buf
                CopyFrom(bmpLive);
                
            }
            catch(Exception ex)
            {
                //_WARNING("[{0}]._camera_OnLiveImage : {1}", this, ex);
            }
        }
        private void _camera_OnError(object sender, EzCameraErrorEventArgs e)
        {
            try
            {
                Invoke((EventHandler<EzCameraErrorEventArgs>)showError, sender, e);
            }
            catch (Exception ex)
            {
                //_WARNING("[{0}]._camera_OnError : {1}", this, ex);
            }
        }
        #endregion

        #region ERROR_LABEL_ADJUSTMENT
        private void LblWarning_DoubleClick(object sender, EventArgs e)
        {
            showError(null, null);
        }
        void showError(object sender, EzCameraErrorEventArgs e)
        {
            if (e == null || EzCameraError.IsNoError(e.Error))
            {
                lblWarning.Visible = false;
                return;
            }

            adjustErrorLabelPos();
            lblWarning.Text = e.Error.ToString();
            lblWarning.Visible = true;
        }
        void adjustErrorLabelPos()
        {
            var rect = ClientRectangle;
            if (rect.Width == 0 || rect.Height == 0)
                return;

            SuspendLayout();
            lblWarning.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Top;
            lblWarning.Width = rect.Width - 16;
            lblWarning.Left = (rect.Width - lblWarning.Width) / 2;
            lblWarning.Top = 32;
            ResumeLayout(true);
        }
        #endregion

        #region OVERRIDES
#if(OPT_DEBUG)
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
        }
        protected override bool _isLiveAndIgnorePaint()
        {
            // Always Return False
            // to allow post drawing / update
            // on still Image Buff
            return false;
        }
#endif
        protected override bool _isLive()
        {
            var cam = _camera;
            return cam != null && cam.IsLiveMode();
        }
        #endregion

        #region VISUAL_STUDIO_WIZARD_CODE
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // lblWarning
            // 
            this.lblWarning.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.lblWarning.Location = new System.Drawing.Point(24, 18);
            this.lblWarning.Size = new System.Drawing.Size(586, 103);
            // 
            // QzCameraViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.Name = "QzCameraViewer";
            this.Size = new System.Drawing.Size(637, 480);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion
    }
}
