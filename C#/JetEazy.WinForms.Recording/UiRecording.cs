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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace JetEazy.WinForms.Recording
{
    public enum UiActionKind { Click, Text, Selection, Number, Tab }

    public sealed class UiControlPart
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public int Ordinal { get; set; }
        public string Caption { get; set; }
    }

    public sealed class UiRecordedAction
    {
        public UiActionKind Kind { get; set; }
        public string FormType { get; set; }
        public string FormName { get; set; }
        public List<UiControlPart> Path { get; set; } = new List<UiControlPart>();
        public string Value { get; set; }
        public string ExpectedItem { get; set; }
        public int DelayMs { get; set; }
    }

    public sealed class UiRecording
    {
        public int Version { get; set; } = 1;
        public string ApplicationVersion { get; set; }
        public List<UiRecordedAction> Actions { get; set; } = new List<UiRecordedAction>();

        public void Save(string path)
        {
            Validate();
            using (var writer = XmlWriter.Create(path, new XmlWriterSettings { Indent = true }))
                new XmlSerializer(typeof(UiRecording)).Serialize(writer, this);
        }

        public static UiRecording Load(string path)
        {
            using (var reader = XmlReader.Create(path, new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null,
                MaxCharactersInDocument = 5 * 1024 * 1024
            }))
            {
                var recording = (UiRecording)new XmlSerializer(typeof(UiRecording)).Deserialize(reader);
                recording.Validate();
                return recording;
            }
        }

        public void Validate()
        {
            if (Version != 1 || Actions == null || Actions.Count > 2000)
                throw new InvalidDataException("錄製檔版本或操作數量不支援。");
            foreach (var action in Actions)
            {
                if (action == null || !Enum.IsDefined(typeof(UiActionKind), action.Kind) ||
                    string.IsNullOrEmpty(action.FormType) || action.Path == null ||
                    action.Path.Count == 0 || action.Path.Count > 50 ||
                    action.DelayMs < 0 || action.DelayMs > 30000 ||
                    (action.Value != null && action.Value.Length > 100000) ||
                    action.Path.Any(p => p == null || string.IsNullOrEmpty(p.Type) || p.Ordinal < 0))
                    throw new InvalidDataException("錄製檔包含無效操作。");
            }
        }
    }

    internal static class UiControlLocator
    {
        public static UiRecordedAction Capture(Control control, UiActionKind kind, string value)
        {
            var form = control.FindForm();
            if (form == null || control == form) throw new InvalidOperationException("找不到控制項所屬視窗。");
            var result = new UiRecordedAction { Kind = kind, Value = value, FormType = form.GetType().FullName, FormName = form.Name };
            if (kind == UiActionKind.Selection && control is ComboBox combo && combo.SelectedIndex >= 0)
                result.ExpectedItem = combo.GetItemText(combo.SelectedItem);
            if (kind == UiActionKind.Selection && control is ListBox list && list.SelectedIndex >= 0)
                result.ExpectedItem = list.GetItemText(list.SelectedItem);
            if (kind == UiActionKind.Tab && control is TabControl tabs && tabs.SelectedTab != null)
                result.ExpectedItem = tabs.SelectedTab.Text;
            for (var current = control; current != form; current = current.Parent)
            {
                var peers = current.Parent.Controls.Cast<Control>()
                    .Where(c => c.Name == current.Name && c.GetType() == current.GetType()).ToArray();
                result.Path.Insert(0, new UiControlPart
                {
                    Name = current.Name, Type = current.GetType().FullName,
                    Ordinal = Array.IndexOf(peers, current),
                    Caption = string.IsNullOrEmpty(current.Name) && current is ButtonBase ? current.Text : null
                });
            }
            return result;
        }

        public static Control Find(IEnumerable<Form> forms, UiRecordedAction action)
        {
            var matches = forms.Where(f => !f.IsDisposed && f.GetType().FullName == action.FormType && f.Name == action.FormName).ToArray();
            if (matches.Length > 1) throw new InvalidOperationException("同時存在多個相同視窗，無法安全定位操作。");
            Control current = matches.SingleOrDefault();
            foreach (var part in action.Path)
            {
                if (current == null) return null;
                var peers = current.Controls.Cast<Control>().Where(c => c.Name == part.Name && c.GetType().FullName == part.Type).ToArray();
                // Named controls must be unique; an index is only a fallback for unnamed controls.
                if (!string.IsNullOrEmpty(part.Name) && peers.Length > 1)
                    throw new InvalidOperationException("控制項名稱重複，請先為控制項設定唯一 Name。");
                current = part.Ordinal < peers.Length ? peers[part.Ordinal] : null;
                if (current != null && part.Caption != null && current.Text != part.Caption) return null;
            }
            return current;
        }

        public static bool SameTarget(UiRecordedAction first, UiRecordedAction second)
        {
            return first.FormType == second.FormType && first.FormName == second.FormName &&
                first.Path.Count == second.Path.Count && first.Path.Zip(second.Path,
                    (a, b) => a.Name == b.Name && a.Type == b.Type && a.Ordinal == b.Ordinal && a.Caption == b.Caption).All(x => x);
        }
    }
}
