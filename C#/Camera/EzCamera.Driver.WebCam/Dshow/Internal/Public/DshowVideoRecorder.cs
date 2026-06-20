#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-05-26 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.DShow.GUI;
using JetEazy.DShow.Utils;
using System;


namespace JetEazy.DShow.Api
{
    public class DshowVideoRecorder : IDshowVideoRecorder
    {
        #region PRIVATE_DATA
        private IntPtr _hwndHost;
        private GvDshowCameraViewer _dsCamViewer;
        #endregion

        public event EventHandler<string> OnFFmpegError;

        public void Dispose()
        {
            _dsCamViewer?.CloseCamera();
            _dsCamViewer?.Dispose();
            _dsCamViewer = null;
            _hwndHost = IntPtr.Zero;
        }

        public void Create(IntPtr hwndHost, string iniFileName)
        {
            if (_dsCamViewer != null)
                throw new Exception($"{GetType().Name}.Init: _dsCamViewer 已經存在!");

            if(!NativeWindowHelper.IsWindow(hwndHost))
                throw new Exception($"{GetType().Name}.Init: hwndHost 視窗不存在!");

            _dsCamViewer = new GvDshowCameraViewer();
            _dsCamViewer.BackColor = System.Drawing.Color.Black;
            _dsCamViewer.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            _dsCamViewer.Location = new System.Drawing.Point(285, 0);
            _dsCamViewer.Margin = new System.Windows.Forms.Padding(1);
            _dsCamViewer.Name = "_dsCamViewer";
            _dsCamViewer.Size = new System.Drawing.Size(640, 480);
            _dsCamViewer.TabIndex = 0;
            _dsCamViewer.OptUseBlinker = false;
            _dsCamViewer.OnFFmpegError += (s, e) => OnFFmpegError.Invoke(s, e);

            NativeWindowHelper.SetControlAsChildAndFill(_dsCamViewer, hwndHost);

            _hwndHost = hwndHost;

            _dsCamViewer.LoadAllSettings(iniFileName);
        }
        public void ResizeChildControlToFill(IntPtr wndHost)
        {
            if (_dsCamViewer != null && _hwndHost != IntPtr.Zero)
            {
                NativeWindowHelper.ResizeChildControlToFill(_dsCamViewer, _hwndHost);
            }
        }

        public bool IsLiveMode()
        {
            return _dsCamViewer != null && _dsCamViewer.IsLiveMode();
        }
        public void StartLiveMode()
        {
            _dsCamViewer?.StartLiveMode();
        }
        public void StopLiveMode()
        {
            _dsCamViewer?.StopLiveMode();
        }
        public void Snapshot(string imgFileName)
        {
            var bmp = _dsCamViewer?.Snapshot();
            bmp?.Save(imgFileName);
            if (bmp == null)
                throw new Exception("無法拍取相片!");
        }

        public bool IsRecording()
        {
            return _dsCamViewer != null && _dsCamViewer.IsRecording();
        }
        public void StartRecord(string videoFileName, double fps = 0, int segment_minutes = 0)
        {
            if (fps <= 0)
                fps = GetSuggestFps();
            _dsCamViewer?.StartRecord(videoFileName, fps, segment_minutes);
        }
        public void StopRecord()
        {
            _dsCamViewer?.StopRecord();
        }

        public int GetTargetFrameWidth()
        {
            return _dsCamViewer!=null ? _dsCamViewer.TargetFrameSize.Width : 1;
        }
        public int GetTargetFrameHeight()
        {
            return _dsCamViewer != null ? _dsCamViewer.TargetFrameSize.Height : 1;
        }
        public double GetTargetFps()
        {
            var dsCamera = _dsCamViewer?.DsCamera;
            return dsCamera != null ? dsCamera.TargetFrameRate : 1.0;
        }
        public double GetCurrentFps()
        {
            var dsCamera = _dsCamViewer?.DsCamera;
            return dsCamera != null ? dsCamera.CurrentFrameRate : 0.0;
        }
        public double GetSuggestFps()
        {
            var dsCamera = _dsCamViewer?.DsCamera;
            return dsCamera != null ? dsCamera.GetSuggestFps() : 1.0;
        }
    }
}
