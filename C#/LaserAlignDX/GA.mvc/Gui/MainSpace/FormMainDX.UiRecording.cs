#region AUTHOR
/*
 *
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 *
 * REVISION:
 *      2026-10-08 使用 ChatGPT Codex  重整 (by LeTian Chang)
 *
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 *
 */
#endregion

using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using JetEazy.WinForms.Recording;

namespace Traveller106
{
    public partial class FormMainDX
    {
        #region PRIVATE_DATA

        const int RecordingMenuId = 0x1F10;
        UiRecordingPanel _recordingPanel;

        #endregion

        #region PRIVATE_METHODS

        void InitializeUiRecording()
        {
            _recordingPanel = new UiRecordingPanel(this);
            if (components == null) components = new System.ComponentModel.Container();
            components.Add(_recordingPanel);
            AddRecordingMenu();
            HandleCreated += RecordingHandleCreated;
        }

        void RecordingHandleCreated(object sender, EventArgs args) => AddRecordingMenu();

        void DisposeUiRecording()
        {
            HandleCreated -= RecordingHandleCreated;
            _recordingPanel?.Dispose();
            _recordingPanel = null;
        }

        void AddRecordingMenu()
        {
            var menu = GetSystemMenu(Handle, false);
            AppendMenu(menu, 0x800, UIntPtr.Zero, null);
            AppendMenu(menu, 0, new UIntPtr(RecordingMenuId), "操作錄製 / 重播\tCtrl+Shift+R");
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetSystemMenu(IntPtr window, bool revert);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern bool AppendMenu(IntPtr menu, uint flags, UIntPtr id, string text);

        #endregion

        protected override void WndProc(ref Message message)
        {
            if (message.Msg == 0x112 && (message.WParam.ToInt64() & 0xFFF0) == RecordingMenuId)
            {
                _recordingPanel?.OpenPanel();
                return;
            }
            base.WndProc(ref message);
        }
    }
}
