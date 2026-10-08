#region AUTHOR
/*
 *
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 *
 * REVISION:
 *      2026-10-08 使用 ChatGPT Codex  重整
 *
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 *
 */
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace JetEazy.WinForms.Recording
{
    /// <summary>Records and replays standard controls on the host's WinForms UI thread.</summary>
    public sealed class UiActionRecorder : IMessageFilter, IDisposable
    {
        #region PRIVATE_DATA

        readonly Form _main;
        readonly Form _panel;
        readonly Timer _timer = new Timer { Interval = 100 };
        readonly Stopwatch _clock = Stopwatch.StartNew();
        readonly Dictionary<Control, string> _observed = new Dictionary<Control, string>();
        UiRecording _recording;
        UiRecording _playback;
        Control _mouseDown;
        int _index;
        int _generation;
        long _lastAction;
        long _nextAction;
        bool _disposed;

        #endregion

        /// <param name="main">Host form. Create and use this object on its UI thread.</param>
        /// <param name="panel">Optional recorder UI excluded from recorded actions.</param>
        public UiActionRecorder(Form main, Form panel = null)
        {
            if (main == null) throw new ArgumentNullException(nameof(main));
            if (main.IsDisposed) throw new ObjectDisposedException(nameof(main));
            if (main.IsHandleCreated && main.InvokeRequired)
                throw new InvalidOperationException("請在主視窗的 UI 執行緒建立錄製器。");
            _main = main;
            _panel = panel;
            _main.Disposed += MainDisposed;
            _timer.Tick += Tick;
            _timer.Start();
            Application.AddMessageFilter(this);
        }

        public bool IsRecording { get; private set; }
        public bool IsPlaying => _playback != null;
        public UiRecording Recording => _recording;
        public string Status { get; private set; } = "尚未錄製。";
        public event EventHandler Changed;
        public event EventHandler ShowPanel;

        public void CapturePending() { if (IsRecording) ObserveFocused(true); }

        public void StartRecording()
        {
            ThrowIfDisposed();
            Stop();
            _recording = new UiRecording { ApplicationVersion = Application.ProductVersion };
            _observed.Clear();
            _lastAction = _clock.ElapsedMilliseconds;
            IsRecording = true;
            ObserveFocused(false);
            SetStatus("錄製中；Ctrl+Shift+F12 停止。密碼欄位不錄製。");
        }

        public void Stop()
        {
            if (IsRecording) ObserveFocused(true);
            IsRecording = false;
            _playback = null;
            _mouseDown = null;
            _generation++;
            SetStatus("已停止。錄製步數：" + (_recording?.Actions.Count ?? 0));
        }

        public void Play(UiRecording recording)
        {
            ThrowIfDisposed();
            if (recording == null) throw new ArgumentNullException(nameof(recording));
            recording.Validate();
            if (recording.Actions.Count == 0) throw new InvalidOperationException("錄製檔沒有操作。");
            Stop();
            _playback = recording;
            _index = 0;
            _nextAction = _clock.ElapsedMilliseconds + recording.Actions[0].DelayMs;
            SetStatus("重播中；Ctrl+Shift+F12 隨時停止。");
        }

        public bool PreFilterMessage(ref Message message)
        {
            if (_disposed) return false;
            if (message.Msg == 0x100 && Control.ModifierKeys == (Keys.Control | Keys.Shift))
            {
                var key = (Keys)message.WParam.ToInt32();
                if (key == Keys.F12) { Stop(); ShowPanel?.Invoke(this, EventArgs.Empty); return true; }
                if (key == Keys.R) { CapturePending(); ShowPanel?.Invoke(this, EventArgs.Empty); return true; }
            }
            if (!IsRecording) return false;
            var control = Control.FromChildHandle(message.HWnd);
            if (control == null || control.IsDisposed || control.InvokeRequired || control.FindForm() == null || control.FindForm() == _panel) return false;
            if (message.Msg == 0x201 || message.Msg == 0x100) ObserveFocused(true);
            if (message.Msg == 0x100) ObserveValue(control.Parent is NumericUpDown ? control.Parent : control, true);
            if (message.Msg == 0x201) _mouseDown = control;
            if (message.Msg == 0x202 && control == _mouseDown)
            {
                _mouseDown = null;
                if (control is ButtonBase && control.Enabled && control.ClientRectangle.Contains(control.PointToClient(Control.MousePosition)))
                    Add(UiControlLocator.Capture(control, UiActionKind.Click, null));
            }
            if (message.Msg == 0x100 && Control.ModifierKeys == Keys.None)
            {
                var form = control.FindForm();
                var key = (Keys)message.WParam.ToInt32();
                Control button = null;
                bool consumesEnter = (control is TextBox text && text.Multiline && text.AcceptsReturn) ||
                    (control is ComboBox combo && combo.DroppedDown);
                if (key == Keys.Enter && !consumesEnter) button = control is Button ? control : form?.AcceptButton as Control;
                if (key == Keys.Escape) button = form?.CancelButton as Control;
                if (button != null && button.Enabled) Add(UiControlLocator.Capture(button, UiActionKind.Click, null));
            }
            if (message.Msg == 0x101 && message.WParam.ToInt32() == (int)Keys.Space && control is ButtonBase)
                Add(UiControlLocator.Capture(control, UiActionKind.Click, null));
            return false;
        }

        public void Dispose()
        {
            if (_disposed) return;
            Stop();
            _disposed = true;
            _main.Disposed -= MainDisposed;
            Application.RemoveMessageFilter(this);
            _timer.Stop();
            _timer.Tick -= Tick;
            _timer.Dispose();
            _observed.Clear();
            Changed = null;
            ShowPanel = null;
        }

        #region PRIVATE_METHODS

        void MainDisposed(object sender, EventArgs args) => Dispose();

        void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(UiActionRecorder));
        }

        void Tick(object sender, EventArgs args)
        {
            if (IsRecording)
            {
                ObserveFocused(true);
                foreach (var control in _observed.Keys.Where(c => c.IsDisposed).ToArray()) _observed.Remove(control);
            }
            if (_playback == null || _clock.ElapsedMilliseconds < _nextAction) return;
            try
            {
                var action = _playback.Actions[_index];
                var control = UiControlLocator.Find(Application.OpenForms.Cast<Form>().Where(f => f != _panel && !f.InvokeRequired), action);
                if (control == null || !control.Visible || !control.Enabled || !control.CanSelect ||
                    (Form.ActiveForm != null && Form.ActiveForm.Modal && Form.ActiveForm != control.FindForm()))
                {
                    if (_clock.ElapsedMilliseconds - _nextAction > 10000)
                        throw new InvalidOperationException("等待控制項逾時：" + action.FormType + " / " + action.Path.Last().Name);
                    return;
                }
                int generation = _generation;
                _index++;
                bool last = _index == _playback.Actions.Count;
                _nextAction = last ? long.MaxValue : _clock.ElapsedMilliseconds + _playback.Actions[_index].DelayMs;
                // Queue the click and return from Timer.Tick before a handler opens ShowDialog.
                // The nested dialog message loop can then replay its own subsequent actions.
                _main.BeginInvoke(new Action(() =>
                {
                    if (generation != _generation || _disposed) return;
                    try
                    {
                        if (control.IsDisposed || !control.Visible || !control.Enabled)
                            throw new InvalidOperationException("目標控制項已失效。");
                        Apply(control, action);
                        if (last && generation == _generation) { Stop(); SetStatus("重播完成。"); }
                    }
                    catch (Exception ex) { Stop(); SetStatus("重播停止：" + ex.Message); ShowPanel?.Invoke(this, EventArgs.Empty); }
                }));
                SetStatus("重播步驟 " + _index + " / " + _playback.Actions.Count);
            }
            catch (Exception ex) { Stop(); SetStatus("重播停止：" + ex.Message); ShowPanel?.Invoke(this, EventArgs.Empty); }
        }

        void ObserveFocused(bool capture)
        {
            foreach (var form in Application.OpenForms.Cast<Form>().ToArray())
            {
                if (form == _panel || form.InvokeRequired || !form.ContainsFocus) continue;
                ObserveControls(form, capture);
            }
        }

        void ObserveControls(Control parent, bool capture)
        {
            foreach (Control control in parent.Controls)
            {
                ObserveValue(control, capture && control.ContainsFocus);
                if (!(control is NumericUpDown)) ObserveControls(control, capture);
            }
        }

        void ObserveValue(Control control, bool capture)
        {
            UiActionKind kind;
            var value = State(control, out kind);
            if (value == null) return;
            string previous;
            bool hadValue = _observed.TryGetValue(control, out previous);
            _observed[control] = value;
            if (capture && hadValue && previous != value) Add(UiControlLocator.Capture(control, kind, value));
        }

        static string State(Control control, out UiActionKind kind)
        {
            kind = UiActionKind.Text;
            if (IsPassword(control)) return null;
            if (control is TextBoxBase text) return text.ReadOnly ? null : text.Text;
            if (control is NumericUpDown numeric) { kind = UiActionKind.Number; return numeric.Value.ToString(CultureInfo.InvariantCulture); }
            if (control is ComboBox combo)
            {
                kind = combo.DropDownStyle == ComboBoxStyle.DropDownList ? UiActionKind.Selection : UiActionKind.Text;
                return kind == UiActionKind.Selection ? combo.SelectedIndex.ToString(CultureInfo.InvariantCulture) : combo.Text;
            }
            if (control is ListBox list && !(list is CheckedListBox) && list.SelectionMode == SelectionMode.One)
            { kind = UiActionKind.Selection; return list.SelectedIndex.ToString(CultureInfo.InvariantCulture); }
            if (control is TabControl tabs) { kind = UiActionKind.Tab; return tabs.SelectedIndex.ToString(CultureInfo.InvariantCulture); }
            return null;
        }

        void Add(UiRecordedAction action)
        {
            if (_recording.Actions.Count >= 2000) { IsRecording = false; SetStatus("已達 2000 步上限，請儲存錄製。"); return; }
            var previous = _recording.Actions.LastOrDefault();
            if (action.Kind != UiActionKind.Click && previous != null && previous.Kind == action.Kind && UiControlLocator.SameTarget(previous, action))
            {
                previous.Value = action.Value;
                previous.ExpectedItem = action.ExpectedItem;
            }
            else
            {
                action.DelayMs = (int)Math.Min(30000, _clock.ElapsedMilliseconds - _lastAction);
                _recording.Actions.Add(action);
            }
            _lastAction = _clock.ElapsedMilliseconds;
            SetStatus("錄製中：" + _recording.Actions.Count + " 步；Ctrl+Shift+F12 停止。");
        }

        void SetStatus(string status) { Status = status; Changed?.Invoke(this, EventArgs.Empty); }

        #endregion

        internal static void Apply(Control control, UiRecordedAction action)
        {
            control.Focus();
            switch (action.Kind)
            {
                case UiActionKind.Click:
                    if (control is Button button) button.PerformClick();
                    else if (control is CheckBox || control is RadioButton) control.AccessibilityObject.DoDefaultAction();
                    else throw new InvalidOperationException("不支援的按鈕類型。");
                    break;
                case UiActionKind.Text:
                    if (IsPassword(control))
                        throw new InvalidOperationException("不重播密碼欄位。");
                    if (control is TextBoxBase text && !text.ReadOnly) text.Text = action.Value ?? "";
                    else if (control is ComboBox combo && combo.DropDownStyle != ComboBoxStyle.DropDownList) combo.Text = action.Value ?? "";
                    else throw new InvalidOperationException("目標不是可編輯文字欄位。");
                    break;
                case UiActionKind.Number:
                    var numeric = control as NumericUpDown;
                    decimal value;
                    if (numeric == null || !decimal.TryParse(action.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out value) || value < numeric.Minimum || value > numeric.Maximum)
                        throw new InvalidOperationException("數值超出控制項範圍。");
                    numeric.Value = value;
                    break;
                case UiActionKind.Selection:
                case UiActionKind.Tab:
                    int index;
                    if (!int.TryParse(action.Value, out index)) throw new InvalidOperationException("選項索引無效。");
                    if (action.Kind == UiActionKind.Tab && control is TabControl tabs && index >= 0 && index < tabs.TabCount)
                    { CheckItem(action, tabs.TabPages[index].Text); tabs.SelectedIndex = index; }
                    else if (action.Kind == UiActionKind.Selection && control is ComboBox selection && index >= -1 && index < selection.Items.Count)
                    { CheckItem(action, index < 0 ? null : selection.GetItemText(selection.Items[index])); selection.SelectedIndex = index; }
                    else if (action.Kind == UiActionKind.Selection && control is ListBox list && !(list is CheckedListBox) && list.SelectionMode == SelectionMode.One && index >= -1 && index < list.Items.Count)
                    { CheckItem(action, index < 0 ? null : list.GetItemText(list.Items[index])); list.SelectedIndex = index; }
                    else throw new InvalidOperationException("選項已不存在或控制項類型不同。");
                    break;
                default: throw new InvalidOperationException("不支援的操作。");
            }
        }

        #region PRIVATE_METHODS

        static bool IsPassword(Control control)
        {
            return (control is TextBox box && (box.UseSystemPasswordChar || box.PasswordChar != '\0')) ||
                (control is MaskedTextBox masked && (masked.UseSystemPasswordChar || masked.PasswordChar != '\0'));
        }

        static void CheckItem(UiRecordedAction action, string item)
        {
            if (action.ExpectedItem != null && action.ExpectedItem != item)
                throw new InvalidOperationException("選項內容已改變，已停止重播。");
        }

        #endregion
    }
}
