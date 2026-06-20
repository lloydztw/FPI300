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

// DirectShowLib (LGPL v2.1)
using DirectShowLib;
using System;
using System.Drawing;
using System.Threading;


// JetEazy
using Mp4Recorder = JetEazy.DShow.Media.ffmeg.Mp4Recorder;
using QFrameRateGauge = JetEazy.QUtilities.QFrameRateGauge;


namespace Camera_NET
{
    partial class SampleGrabberHelper
    {
        const double TIMEOUT_SECS = 3;

        #region PRIVATE_DATA
        QFrameRateGauge _fpsGauge = new QFrameRateGauge();
        Guid _mediaSubType = MediaSubType.RGB24;
        Size _targetFrameSize = new Size(640, 480);
        uint _targetFpsCount = 30;
        bool _recordRunFlag = false;
        Mp4Recorder _recorder = null;
        string _recArgsIniFile;
        ISampleGrabberCB _extraGrabber = null;
        #endregion

        public event EventHandler<string> OnFFmpegError;
        public event EventHandler OnFrameRateUpdated;
        public event EventHandler OnFrameGrabbed;

        protected SampleGrabberHelper()
        {
            _checkTimeout(reset: true);
        }

        public string RecArgsIniFileName
        {
            get
            {
                return _recArgsIniFile;
            }
            set
            {
                string path;
                if(string.IsNullOrEmpty(value))
                    path = System.IO.Path.GetTempPath();
                else
                    path = System.IO.Path.GetDirectoryName(value);
                _recArgsIniFile = System.IO.Path.Combine(path, "recorder_args.ini");
            }
        }
        public double TargetFrameRate
        {
            get;
            private set;
        }
        public double CurrentFrameRate
        {
            get
            {
                return _fpsGauge.FrameRate;
            }
        }
        public double GetSuggestFps()
        {
            var fps = CurrentFrameRate > 2
                    ? Math.Min(CurrentFrameRate, TargetFrameRate)
                    : TargetFrameRate;
            return Math.Round(fps, 1);
        }

        public bool IsRecording()
        {
            if (_recordRunFlag)
            {
                if (_checkTimeout() && !_isTimeoutNotified)
                {
                    _isTimeoutNotified = true;
                    OnFFmpegError?.Invoke(this, "Record.TimeOut");
                }
            }
            return _recordRunFlag;
        }
        public void StartRecord(string videoFileName, double recFps, string audioDeviceName = null, int segment_minutes = 0)
        {
            if (_recordRunFlag)
                return;

            if (recFps <= 0)
            {
                recFps = GetSuggestFps();
            }

            _checkTimeout(reset: true);

            _recorder?.Dispose();
            _recorder = new Mp4Recorder();

            _recorder.OnFFmpegError += (s, e) => OnFFmpegError?.Invoke(s, e);
            _recorder.Init(_recArgsIniFile, videoFileName, _targetFrameSize, recFps, _mediaSubType, audioDeviceName, segment_minutes);

            _checkTimeout(reset: true);
            _recordRunFlag = true;
        }
        public void StopRecord()
        {
            if (_recordRunFlag && _recorder != null)
            {
                var old = _recorder;
                _recorder = null;

                System.Threading.Thread.Sleep(1500);

                old?.Finish();
                old?.Dispose();

                _recordRunFlag = false;
                _checkTimeout(reset: true);
            }
        }
        public void SetExtraBufferCB(ISampleGrabberCB cb)
        {
            _extraGrabber = cb;
        }

        private void _updateTargetFrameRate(VideoInfoHeader vih, Guid? mediaSubType)
        {
            if (vih != null)
            {
                _targetFrameSize = new Size(vih.BmiHeader.Width, vih.BmiHeader.Height);
                double fps = 1e7 / vih.AvgTimePerFrame;
                this.TargetFrameRate = Math.Round(fps, 1);
                this._targetFpsCount = (uint)Math.Max(1, Math.Round(fps));
                this._fpsGauge.Reset();
            }
            if (mediaSubType != null)
            {
                _mediaSubType = mediaSubType.Value;
            }
        }
        private void _OnBufferCB(double SampleTime, IntPtr pBuffer, int BufferLen)
        {
            //>>> OnFrameGrabbed?.Invoke(this, null);

            _fpsGauge++;

            _checkTimeout(reset: true);

            if ((_fpsGauge.TotalFramesCount % _targetFpsCount == 0) && _fpsGauge.FrameRate > 0)
            { 
                //OnFrameRateUpdated?.Invoke(this, null);
                OnFrameRateUpdated?.BeginInvoke(this, null, null, null);
            }

            if (_recordRunFlag)
            {
                //NOTE: pBuffer 是指向 RGB24 !!!
                _recorder?.PushFrame(pBuffer, BufferLen);
            }

            //NOTE: pBuffer 是指向 RGB24 !!!
            _extraGrabber?.BufferCB(SampleTime, pBuffer, BufferLen);
        }

        #region PRIVATE_CHECK_TIMEOUT_FUNCTIONS
        private long _lastCheckTicks = DateTime.UtcNow.Ticks;
        private bool _isTimeoutNotified = false;
        private bool _checkTimeout(bool reset = false)
        {
            long nowTicks = DateTime.UtcNow.Ticks;

            if (reset)
            {
                Interlocked.Exchange(ref _lastCheckTicks, nowTicks);
                _isTimeoutNotified = false;
                return false;
            }

            // 讀取當前的 Ticks
            long lastTicks = Interlocked.Read(ref _lastCheckTicks);
            var ts = TimeSpan.FromTicks(nowTicks - lastTicks);

            return ts.TotalSeconds > TIMEOUT_SECS;
        }
        #endregion
    }
}
