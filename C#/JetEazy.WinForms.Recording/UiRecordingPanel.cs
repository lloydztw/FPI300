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
using System.Windows.Forms;

namespace JetEazy.WinForms.Recording
{
    /// <summary>Ready-to-use recording UI; disposed automatically with the host form.</summary>
    public sealed partial class UiRecordingPanel : Form
    {
        #region PRIVATE_DATA

        readonly Form _main;
        readonly UiActionRecorder _recorder;
        bool _saved = true;
        bool _disposing;

        #endregion

        public UiRecordingPanel(Form main)
        {
            if (main == null) throw new ArgumentNullException(nameof(main));
            if (main.IsDisposed) throw new ObjectDisposedException(nameof(main));
            if (main.IsHandleCreated && main.InvokeRequired)
                throw new InvalidOperationException("請在主視窗的 UI 執行緒建立錄製面板。");
            _main = main;
            InitializeComponent();
            _recorder = new UiActionRecorder(main, this);
            _main.Disposed += MainDisposed;
            _recorder.Changed += UpdateStatus;
            _recorder.ShowPanel += ShowRequested;
            UpdateStatus(this, EventArgs.Empty);
        }

        public void OpenPanel()
        {
            if (_disposing) return;
            _recorder.CapturePending();
            Show();
            Activate();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!_disposing && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
            base.OnFormClosing(e);
        }

        #region PRIVATE_METHODS

        void ReleaseRecorder()
        {
            if (_disposing) return;
            _disposing = true;
            if (_main != null) _main.Disposed -= MainDisposed;
            _recorder?.Dispose();
        }

        void StopRecording(object sender, EventArgs args) => _recorder.Stop();

        void MainDisposed(object sender, EventArgs args) => Dispose();

        void ShowRequested(object sender, EventArgs args) => OpenPanel();

        void UpdateStatus(object sender, EventArgs args)
        {
            _status.Text = _recorder.Status;
            _start.Enabled = !_recorder.IsRecording && !_recorder.IsPlaying;
            _play.Enabled = _start.Enabled;
            _stop.Enabled = _recorder.IsRecording || _recorder.IsPlaying;
            _save.Enabled = _start.Enabled && (_recorder.Recording?.Actions.Count ?? 0) > 0;
        }

        void StartRecording(object sender, EventArgs args)
        {
            if (!_saved && (_recorder.Recording?.Actions.Count ?? 0) > 0 &&
                MessageBox.Show(this, "目前錄製尚未儲存。要捨棄並重新錄製嗎？", Text, MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            Hide();
            _main.Activate();
            _recorder.StartRecording();
            _saved = false;
        }

        void SaveRecording(object sender, EventArgs args)
        {
            using (var dialog = new SaveFileDialog { Filter = "UI 操作錄製 (*.uirec.xml)|*.uirec.xml", DefaultExt = "uirec.xml", FileName = "操作錄製.uirec.xml", AddExtension = true })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                try { _recorder.Recording.Save(dialog.FileName); _saved = true; _status.Text = "已儲存：" + dialog.FileName; }
                catch (Exception ex) { MessageBox.Show(this, "儲存失敗：" + ex.Message, Text); }
            }
        }

        void PlayRecording(object sender, EventArgs args)
        {
            using (var dialog = new OpenFileDialog { Filter = "UI 操作錄製 (*.uirec.xml;*.fpiui.xml)|*.uirec.xml;*.fpiui.xml", CheckFileExists = true })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    var recording = UiRecording.Load(dialog.FileName);
                    _recorder.Play(recording);
                    Hide();
                    _main.Activate();
                }
                catch (Exception ex) { MessageBox.Show(this, "無法重播：" + ex.Message, Text); }
            }
        }

        #endregion

    }
}
