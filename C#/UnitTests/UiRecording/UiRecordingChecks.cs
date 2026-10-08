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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using JetEazy.WinForms.Recording;

internal static class UiRecordingChecks
{
    #region PRIVATE_METHODS

    [STAThread]
    static int Main()
    {
        int exit = 1;
        var allowed = new[] { "mscorlib", "System", "System.Core", "System.Drawing", "System.Windows.Forms", "System.Xml" };
        Check(typeof(UiRecording).Assembly.GetName().Name == "JetEazy.WinForms.Recording", "tests use the built DLL");
        Check(typeof(UiRecording).Assembly.GetReferencedAssemblies().All(a => allowed.Contains(a.Name)), "recording DLL references framework only");
        Application.EnableVisualStyles();
        using (var main = new Form { Name = "testMain", ShowInTaskbar = false, StartPosition = FormStartPosition.Manual, Location = new Point(-30000, -30000) })
        using (var panel = new Form())
        using (var recorder = new UiActionRecorder(main, panel))
        {
            var input = new TextBox { Name = "input", Text = "initial", Top = 10 };
            var open = new Button { Name = "open", Text = "Open", Top = 45 };
            main.Controls.AddRange(new Control[] { input, open });
            main.AcceptButton = open;
            int confirms = 0;
            string dialogValue = null;
            open.Click += (s, e) =>
            {
                using (var dialog = new RecordingTestDialog())
                {
                    dialog.Confirm.Click += (a, b) => { confirms++; dialogValue = dialog.Input.Text; dialog.Close(); };
                    dialog.Shown += (a, b) =>
                    {
                        if (!recorder.IsRecording) return;
                        dialog.Input.Focus();
                        Feed(recorder, dialog.Input, Keys.A);
                        dialog.Input.Text = "對話框輸入";
                        Feed(recorder, dialog.Input, Keys.Enter);
                        dialog.Confirm.PerformClick();
                    };
                    dialog.ShowDialog(main);
                }
            };
            main.Shown += async (s, e) =>
            {
                try
                {
                    using (var owner = new Form())
                    {
                        var standalone = new UiActionRecorder(owner);
                        var recordingPanel = new UiRecordingPanel(owner);
                        owner.Dispose();
                        Check(recordingPanel.IsDisposed, "host disposal closes recording panel");
                        Throws<ObjectDisposedException>(() => standalone.StartRecording(), "host disposal releases engine");
                        standalone.Dispose();
                        recordingPanel.Dispose();
                    }
                    input.Focus();
                    recorder.StartRecording();
                    Feed(recorder, input, Keys.A);
                    input.Text = "手動錄製";
                    Feed(recorder, input, Keys.Enter);
                    open.PerformClick();
                    recorder.Stop();
                    Check(recorder.Recording.Actions.Count == 4, "record text / open / dialog text / confirm ordering: " +
                        string.Join("; ", recorder.Recording.Actions.Select(a => a.Kind + ":" + a.FormName + ":" + a.Value)));
                    Check(recorder.Recording.Actions.Select(a => a.Kind).SequenceEqual(new[] { UiActionKind.Text, UiActionKind.Click, UiActionKind.Text, UiActionKind.Click }), "semantic action kinds");

                    var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test.fpiui.xml");
                    recorder.Recording.Save(path);
                    var loaded = UiRecording.Load(path);
                    Check(loaded.Actions[0].Value == "手動錄製", "Unicode round trip");
                    File.Delete(path);
                    input.Text = "reset";
                    dialogValue = null;
                    recorder.Play(loaded);
                    for (int i = 0; i < 150 && recorder.IsPlaying; i++) await Task.Delay(20);
                    Check(!recorder.IsPlaying && recorder.Status == "重播完成。", "modal replay completes");
                    Check(confirms == 2 && input.Text == "手動錄製" && dialogValue == "對話框輸入", "replay actions reach correct controls");

                    var delayed = new UiRecording();
                    var click = UiControlLocator.Capture(open, UiActionKind.Click, null);
                    click.DelayMs = 300;
                    delayed.Actions.Add(click);
                    recorder.Play(delayed);
                    recorder.Stop();
                    await Task.Delay(400);
                    Check(confirms == 2, "stop cancels pending actions");

                    using (var check = new CheckBox { Name = "check", Top = 80 })
                    {
                        main.Controls.Add(check);
                        UiActionRecorder.Apply(check, new UiRecordedAction { Kind = UiActionKind.Click });
                        Check(check.Checked, "checkbox default action");
                    }

                    var missing = new UiRecording();
                    var missingClick = UiControlLocator.Capture(open, UiActionKind.Click, null);
                    missingClick.Path.Last().Name = "doesNotExist";
                    missing.Actions.Add(missingClick);
                    recorder.Play(missing);
                    for (int i = 0; i < 600 && recorder.IsPlaying; i++) await Task.Delay(20);
                    Check(!recorder.IsPlaying && recorder.Status.Contains("逾時") && confirms == 2, "missing target stops without clicking another control");

                    using (var duplicate = new Form { Name = main.Name })
                        Throws<InvalidOperationException>(() => UiControlLocator.Find(new[] { main, duplicate }, click), "ambiguous form rejected");
                    using (var password = new TextBox { UseSystemPasswordChar = true })
                        Throws<InvalidOperationException>(() => UiActionRecorder.Apply(password, new UiRecordedAction { Kind = UiActionKind.Text, Value = "secret" }), "password replay rejected");
                    using (var password = new MaskedTextBox { PasswordChar = '*' })
                        Throws<InvalidOperationException>(() => UiActionRecorder.Apply(password, new UiRecordedAction { Kind = UiActionKind.Text, Value = "secret" }), "masked password replay rejected");
                    using (var numeric = new NumericUpDown { Maximum = 10 })
                        Throws<InvalidOperationException>(() => UiActionRecorder.Apply(numeric, new UiRecordedAction { Kind = UiActionKind.Number, Value = "11" }), "numeric bounds checked");
                    using (var combo = new ComboBox())
                    {
                        combo.Items.Add("changed");
                        Throws<InvalidOperationException>(() => UiActionRecorder.Apply(combo, new UiRecordedAction { Kind = UiActionKind.Selection, Value = "0", ExpectedItem = "original" }), "changed options rejected");
                    }
                    loaded.Version = 99;
                    Throws<InvalidDataException>(() => loaded.Validate(), "unknown recording version rejected");
                    exit = 0;
                    Check(!AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name == "FPI30AOIX3"), "host application not loaded");
                    Console.WriteLine("PASS standalone recording DLL, host lifetime, Unicode storage, modal replay, cancellation, identity and value validation");
                }
                catch (Exception ex) { Console.Error.WriteLine(ex); }
                finally { recorder.Stop(); main.Close(); }
            };
            Application.Run(main);
        }
        return exit;
    }

    static void Feed(UiActionRecorder recorder, Control control, Keys key)
    {
        var message = Message.Create(control.Handle, 0x100, new IntPtr((int)key), IntPtr.Zero);
        recorder.PreFilterMessage(ref message);
    }

    static void Check(bool result, string message)
    {
        if (!result) throw new Exception(message);
    }

    static void Throws<T>(Action action, string message) where T : Exception
    {
        try { action(); } catch (T) { return; }
        throw new Exception(message);
    }

    #endregion
}

internal sealed class RecordingTestDialog : Form
{
    public readonly TextBox Input = new TextBox { Name = "dialogInput", Top = 10 };
    public readonly Button Confirm = new Button { Name = "confirm", Text = "OK", Top = 45 };

    public RecordingTestDialog()
    {
        Name = "testDialog";
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;
        Location = new Point(-30000, -30000);
        Controls.AddRange(new Control[] { Input, Confirm });
        AcceptButton = Confirm;
    }
}
