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
using System.Drawing;
using System.Windows.Forms;


namespace JetEazy.DShow.GUI
{
    internal interface IDshowCameraViewer
    {
        event EventHandler<string> OnFFmpegError;

        Control Window { get; }

        void OpenCamera(int dshowCamIndex, string iniFileName = null);
        void CloseCamera();

        int CamIndex { get; }
        string FourCC { get; }
        Size TargetFrameSize { get; set; }
        double TargetFrameRate { get; set; }

        void LoadAllSettings(string fileName, int dshowCamIndex = -1);
        void SaveAllSettings(string fileName);

        DialogResult ShowCameraCtrlDlg();
        DialogResult ShowPinConfigDlg();

        bool IsLiveMode();
        void StartLiveMode();
        void StopLiveMode();
        Bitmap Snapshot();

        bool IsRecording();
        void StartRecord(string fileName = null, double fps = 0, int segment_minutes = 0);
        void StopRecord();
    }
}