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


using JetEazy.Win32;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;


namespace JetEazy.DShow.Media.ffmeg
{
    public class Mp4Recorder : IDisposable
    {
        public const int PROCESS_STOP_TIMEOUT = 2000;

        /// <summary>
        /// 注意: 如果有 AUDIO, 必須要用 .mkv 當副檔名, 否則無法錄製 !!!
        /// </summary>
        public const string EXT = ".mkv";
        public static string FILE_NAME_FORMAT => $"vid_%Y%m%d_%H%M%S.{EXT}";
        public static string FFMEG_EXE_FILE => LibPaths.FFMEG_EXE_FILE;

        #region NLOG
        static NLog.Logger _singleton = null;
        public static NLog.Logger _LOG
        {
            get
            {
                if (_singleton == null)
                    _singleton = NLog.LogManager.GetCurrentClassLogger();
                return _singleton;
            }
        }
        #endregion

        #region PRIVATE_DATA
        private Process _ffmpegProcess;
        private Stream _ffmpegStdin => _ffmpegProcess?.StandardInput.BaseStream;
        private int _width, _height, _fps;
        private int _segment_seconds = 0;
        #endregion

        #region PRIVATE_RUNTIME_DATA
        private bool _isStarted = false;
        private string _lastError = null;
        private DateTime? _firstFrameTime = null;
        private long _framesPushed = 0;
        private int _skipCountToStart = 0;
        private byte[] _videoBuffer = null; // 類別成員，避免重複建立
        #endregion

        #region FFMPEG_ARGS
        private int _gopSize => _fps * 2;       // QSV 建議 GOP 設定為 FPS 的 1~2  倍左右，有助於串流與切片的穩定
        #endregion

        public event EventHandler<string> OnFFmpegError;

        public void Init(string iniFileName, string outputFile, Size targetSize, double fps, Guid? mediaSubType = null, string audioDeviceName = null, int segment_minutes = 0)
        {
            if (targetSize != Size.Empty)
            {
                _width = targetSize.Width;
                _height = targetSize.Height;
            }
            if (fps > 0)
            {
                _fps = (int)Math.Round(fps);
            }

            _segment_seconds = (segment_minutes > 0) ? segment_minutes * 60 : 0;

            System.Diagnostics.Trace.Assert(_isStarted == false, $"{GetType().Name} 重複 Init() !");

            start_ffmpeg(outputFile, audioDeviceName, iniFileName);
        }

        #region PRIVATE_FUNCTIONS

        private void start_ffmpeg(string outputFile, string audioDeviceName, string jsonFileName = null)
        {
            _lastError = null;

            if (!System.IO.File.Exists(FFMEG_EXE_FILE))
            {
                string errMsg = $"檔案不存在: {FFMEG_EXE_FILE}";
                // System.Windows.Forms.MessageBox.Show(errMsg, $"[C#] {GetType().Name}.StartFfmpeg");
                throw new Exception(errMsg);
            }

            string args = compose_ffmpeg_args(outputFile, audioDeviceName, jsonFileName);

            _LOG.Info("ffmpeg args:\n" + args);

            var startInfo = new ProcessStartInfo
            {
                FileName = FFMEG_EXE_FILE,
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
                //>>> CreateNoWindow = false // Set this to false for debugging!
            };

            _ffmpegProcess = new Process { StartInfo = startInfo };
            _ffmpegProcess.Start();

            //_ffmpegStdin = new BinaryWriter(_ffmpegProcess.StandardInput.BaseStream);
            _isStarted = true;

            // 選擇性：背景讀取 stderr，避免緩衝區塞滿
            _ffmpegProcess.ErrorDataReceived += _ffmpegProcess_ErrorDataReceived;
            _ffmpegProcess.BeginErrorReadLine();
        }

        private void _ffmpegProcess_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                if (e.Data.Contains("error"))
                {
                    _LOG.Error(e.Data);

                    bool isFirstTime = _lastError == null;
                    if (isFirstTime)
                    {
                        _lastError = e.Data;
                        OnFFmpegError?.Invoke(this, _lastError);
                        //new Action(() => Finish()).BeginInvoke(null, null);
                    }
                }
                else
                {
                    //Debug.WriteLine(e.Data);
                    _LOG.Debug(e.Data);
                }
            }
        }

        #endregion

        #region PRIVATE_ARGS_FUNCTIONS
        private string compose_ffmpeg_seg_args(string outputFile)
        {
            //--------------------------------------------------------------------------------------------------
            // 注意: 如果有 AUDIO, 必須要用 .mkv 當副檔名, 否則無法錄製 !!!
            //--------------------------------------------------------------------------------------------------
            outputFile = System.IO.Path.ChangeExtension(outputFile, EXT);

            string args;

            if (_segment_seconds <= 0)
            {
                args = $" \"{outputFile}\"";
            }
            else
            {
                string path = System.IO.Path.GetDirectoryName(outputFile);
                string outputPattern = path.Replace("\\", "/") + "/" + FILE_NAME_FORMAT;
                args = $" -f segment -segment_time {_segment_seconds} " +
                       $"-segment_atclocktime 1 -strftime 1 -reset_timestamps 1 " +
                       $"-segment_format_options live=1 " + // 針對直播/分段優化，確保索引即時寫入
                       $"\"{outputPattern}\"";
            }

            return args;
        }
        private string compose_ffmpeg_args(string outputFile, string audioDeviceName, string iniFileName)
        {
            bool usingAudio = !string.IsNullOrEmpty(audioDeviceName);

            // 影像輸入設定
            string videoInputArg = $"-f rawvideo -pixel_format bgr24 -video_size {_width}x{_height} -r {_fps} -i - ";

            // 影像編碼設定參數
            string videoEncoderArg = _load_ffmpeg_video_encoder_arg(iniFileName);

            string args;

            if (usingAudio)
            {
                args = $"-y -use_wallclock_as_timestamps 1 " +         // 牆鐘同步
                        $"-f dshow -i audio=\"{audioDeviceName}\" " +   // 音訊源
                        videoInputArg +                                 // 影像源 (來自 Pipe)
                        $"-af \"aresample=async=1\" " +                 // 音訊重採樣同步
                        videoEncoderArg +                               // 硬體編碼器
                        $"-c:a aac -b:a 128k -map 0:a -map 1:v " +      // 強制影音映射
                        compose_ffmpeg_seg_args(outputFile);
            }
            else
            {
                args = $"-y " +
                        videoInputArg +
                        videoEncoderArg +
                        compose_ffmpeg_seg_args(outputFile);
            }

            return args;
        }
        private string _load_ffmpeg_video_encoder_arg(string iniFileName)
        {
            string videoEncoderArg = "";

            if (System.IO.File.Exists(iniFileName))
            {
                Win32Ini.Load(ref videoEncoderArg, iniFileName, "ffmpeg_args", "videoEncoderArg");
                videoEncoderArg = videoEncoderArg.Trim();
                if (string.IsNullOrEmpty(videoEncoderArg))
                    videoEncoderArg = "cpu";
            }

            switch (videoEncoderArg)
            {
                case "qsv":
                    // 影像編碼設定 (QSV)
                    videoEncoderArg = "-vf \"vflip,format=nv12\" -c:v h264_qsv -preset veryfast -look_ahead 0";
                    break;

                case "nvenc":
                    // 影像編碼設定 (NVENC 穩定版)
                    // 移除會導致 Eval 錯誤的 -tune 參數，直接靠 preset p1 和 delay 0 達成同步
                    videoEncoderArg = "-vf \"vflip\" -c:v h264_nvenc -preset p1 -delay 0 -rc vbr";
                    break;

                case "cpu":
                    videoEncoderArg = "-vf \"vflip\" -c:v libx264 -preset ultrafast -tune zerolatency";
                    break;
            }

            return videoEncoderArg + " ";
        }

        private string _compose_ffmpeg_args_cpu(string outputFile, string audioDeviceName)
        {
            bool usingAudio = !string.IsNullOrEmpty(audioDeviceName);

            string args;

            if (usingAudio)
            {
                args = $"-y " +
                        // 1. 同步與時間戳設定
                        $"-use_wallclock_as_timestamps 1 " +
                        // 2. 音訊輸入 (第一個輸入源: index 0)
                        $"-f dshow -i audio=\"{audioDeviceName}\" " +
                        // 3. 影像輸入 (第二個輸入源: index 1)
                        $"-f rawvideo -pixel_format bgr24 -video_size {_width}x{_height} -r {_fps} -i - " +
                        // 4. 濾鏡部分：音訊同步 + 影像垂直翻轉
                        $"-af \"aresample=async=1\" " +
                        $"-vf \"vflip\" " +
                        // 5. 編碼設定與映射
                        $"-c:v libx264 -preset ultrafast -tune zerolatency " +
                        $"-c:a aac -b:a 128k " +
                        $"-map 0:a -map 1:v " +
                        compose_ffmpeg_seg_args(outputFile);
            }
            else
            {
                args = $"-y " +
                        // 影像輸入 (第1個輸入源: index 0)
                        $"-f rawvideo -pixel_format bgr24 -video_size {_width}x{_height} -r {_fps} -i - " +
                        // 濾鏡部分：影像垂直翻轉
                        $"-vf \"vflip\" " +
                        // 編碼設定
                        $"-c:v libx264 -preset ultrafast -tune zerolatency " +
                        compose_ffmpeg_seg_args(outputFile);
            }

            return args;
        }
        private string _compose_ffmpeg_args_h264_qsv(string outputFile, string audioDeviceName)
        {
            bool usingAudio = !string.IsNullOrEmpty(audioDeviceName);

            // 影像輸入設定
            string videoInputArg = $"-f rawvideo -pixel_format bgr24 -video_size {_width}x{_height} -r {_fps} -i - ";

            // 影像編碼設定 (QSV)
            string videoEncoderArg = $"-vf \"vflip,format=nv12\" -c:v h264_qsv -preset veryfast -look_ahead 0 ";

            string args;
            if (usingAudio)
            {
                args = $"-y -use_wallclock_as_timestamps 1 " +
                        $"-f dshow -i audio=\"{audioDeviceName}\" " +
                        videoInputArg +
                        $"-af \"aresample=async=1\" " +
                        videoEncoderArg +
                        $"-c:a aac -b:a 128k -map 0:a -map 1:v " +
                        compose_ffmpeg_seg_args(outputFile);
            }
            else
            {
                args = $"-y " +
                        videoInputArg +
                        videoEncoderArg +
                        compose_ffmpeg_seg_args(outputFile);
            }

            return args;
        }
        private string _compose_ffmpeg_args_h264_nvenc(string outputFile, string audioDeviceName)
        {
            bool usingAudio = !string.IsNullOrEmpty(audioDeviceName);

            // 影像輸入設定
            string videoInputArg = $"-f rawvideo -pixel_format bgr24 -video_size {_width}x{_height} -r {_fps} -i - ";

            // 影像編碼設定 (NVENC 穩定版)
            // 移除會導致 Eval 錯誤的 -tune 參數，直接靠 preset p1 和 delay 0 達成同步
            string videoEncoderArg = $"-vf \"vflip\" -c:v h264_nvenc -preset p1 -delay 0 -rc vbr ";

            string args;
            if (usingAudio)
            {
                args = $"-y -use_wallclock_as_timestamps 1 " +         // 牆鐘同步
                        $"-f dshow -i audio=\"{audioDeviceName}\" " +   // 音訊源
                        videoInputArg +                                 // 影像源 (來自 Pipe)
                        $"-af \"aresample=async=1\" " +                 // 音訊重採樣同步
                        videoEncoderArg +                               // 硬體編碼器
                        $"-c:a aac -b:a 128k -map 0:a -map 1:v " +      // 強制影音映射
                        compose_ffmpeg_seg_args(outputFile);
            }
            else
            {
                args = $"-y " +
                        videoInputArg +
                        videoEncoderArg +
                        compose_ffmpeg_seg_args(outputFile);
            }

            return args;
        }
        #endregion

        public void PushFrame(IntPtr rgb24Ptr, int bufferSize)
        {
            if (!_isStarted || _lastError != null)
                return;

            if (false)
            {
                if (_skipCountToStart > 0)
                {
                    _skipCountToStart--;
                    return;
                }

                // 取得當前時間
                DateTime now = DateTime.Now;
                if (_firstFrameTime == null)
                    _firstFrameTime = now;

                // 計算「理論上」到現在為止應該推送多少幀
                // 總經過秒數 * 設定的 FPS
                double elapsedSeconds = (now - _firstFrameTime.Value).TotalSeconds;
                long targetFrameCount = (long)(elapsedSeconds * _fps);

                // 如果目前推送的總幀數已經超過了理論值，就跳過這幀 (丟包)
                // 這樣可以確保 C# 推送的速度永遠不會快於現實時間
                if (_framesPushed > targetFrameCount + 1)
                {
                    // 這裡可以 Log 一下，代表 C# 呼叫端跑太快了
                    _LOG.Warn("PushFrame too fast!");
                    return;
                }
            }

            try
            {
                // 1. 初始化或檢查緩衝區大小
                if (_videoBuffer == null || _videoBuffer.Length != bufferSize)
                {
                    _videoBuffer = new byte[bufferSize];
                }

                // 2. 快速複製記憶體
                Marshal.Copy(rgb24Ptr, _videoBuffer, 0, bufferSize);

                // 3. 寫入 Pipe (這是阻塞調用，如果 FFmpeg 處理太慢，這裡會卡住)
                _ffmpegStdin.Write(_videoBuffer, 0, _videoBuffer.Length);
            }
            catch(Exception ex)
            {
                _LOG.Error(ex);
            }
        }

        public void Finish()
        {
            if (_isStarted)
            {
                _ffmpegStdin?.Flush();
                _ffmpegStdin?.Close();
                //_ffmpegStdin?.Dispose();
                //_ffmpegStdin = null;

                if (_ffmpegProcess != null)
                {
                    try
                    {
                        bool ok = _ffmpegProcess.WaitForExit(PROCESS_STOP_TIMEOUT);
                        if (!ok)
                            _ffmpegProcess.Kill();
                    }
                    catch
                    {

                    }
                    _ffmpegProcess?.Dispose();
                    _ffmpegProcess = null;
                }

                _isStarted = false;
            }
        }

        public void Dispose()
        {
            Finish();
        }
    }
}
