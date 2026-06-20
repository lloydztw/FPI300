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

using System;

namespace JetEazy.DShow.Api
{
    public interface IDshowVideoRecorder : IDisposable
    {
        event EventHandler<string> OnFFmpegError;

        void Create(IntPtr wndHost, string iniFileName);
        void ResizeChildControlToFill(IntPtr wndHost);

        bool IsLiveMode();
        void StartLiveMode();
        void StopLiveMode();
        void Snapshot(string imgFileName);

        bool IsRecording();
        void StartRecord(string imgFileName, double fps = 0, int segment_minutes = 0);
        void StopRecord();

        int GetTargetFrameWidth();
        int GetTargetFrameHeight();

        double GetTargetFps();
        double GetCurrentFps();
        double GetSuggestFps();
    }
}