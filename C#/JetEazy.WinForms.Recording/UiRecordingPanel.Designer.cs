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

namespace JetEazy.WinForms.Recording
{
    partial class UiRecordingPanel
    {
        #region PRIVATE_DATA

        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TableLayoutPanel _layout;
        private System.Windows.Forms.FlowLayoutPanel _buttons;
        private System.Windows.Forms.Label _instructions;
        private System.Windows.Forms.Label _status;
        private System.Windows.Forms.Button _start;
        private System.Windows.Forms.Button _stop;
        private System.Windows.Forms.Button _save;
        private System.Windows.Forms.Button _play;

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ReleaseRecorder();
                if (components != null) components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UiRecordingPanel));
            this._layout = new System.Windows.Forms.TableLayoutPanel();
            this._buttons = new System.Windows.Forms.FlowLayoutPanel();
            this._start = new System.Windows.Forms.Button();
            this._stop = new System.Windows.Forms.Button();
            this._save = new System.Windows.Forms.Button();
            this._play = new System.Windows.Forms.Button();
            this._instructions = new System.Windows.Forms.Label();
            this._status = new System.Windows.Forms.Label();
            this._layout.SuspendLayout();
            this._buttons.SuspendLayout();
            this.SuspendLayout();
            // 
            // _layout
            // 
            this._layout.ColumnCount = 1;
            this._layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._layout.Controls.Add(this._buttons, 0, 0);
            this._layout.Controls.Add(this._instructions, 0, 1);
            this._layout.Controls.Add(this._status, 0, 2);
            this._layout.Dock = System.Windows.Forms.DockStyle.Fill;
            this._layout.Location = new System.Drawing.Point(0, 0);
            this._layout.Name = "_layout";
            this._layout.Padding = new System.Windows.Forms.Padding(12);
            this._layout.RowCount = 3;
            this._layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this._layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this._layout.Size = new System.Drawing.Size(640, 254);
            this._layout.TabIndex = 0;
            // 
            // _buttons
            // 
            this._buttons.Controls.Add(this._start);
            this._buttons.Controls.Add(this._stop);
            this._buttons.Controls.Add(this._save);
            this._buttons.Controls.Add(this._play);
            this._buttons.Dock = System.Windows.Forms.DockStyle.Fill;
            this._buttons.Location = new System.Drawing.Point(15, 15);
            this._buttons.Name = "_buttons";
            this._buttons.Size = new System.Drawing.Size(610, 44);
            this._buttons.TabIndex = 0;
            // 
            // _start
            // 
            this._start.AutoSize = true;
            this._start.Location = new System.Drawing.Point(3, 3);
            this._start.Name = "_start";
            this._start.Size = new System.Drawing.Size(77, 27);
            this._start.TabIndex = 0;
            this._start.Text = "開始錄製";
            this._start.Click += new System.EventHandler(this.StartRecording);
            // 
            // _stop
            // 
            this._stop.AutoSize = true;
            this._stop.Location = new System.Drawing.Point(86, 3);
            this._stop.Name = "_stop";
            this._stop.Size = new System.Drawing.Size(75, 27);
            this._stop.TabIndex = 1;
            this._stop.Text = "停止";
            this._stop.Click += new System.EventHandler(this.StopRecording);
            // 
            // _save
            // 
            this._save.AutoSize = true;
            this._save.Location = new System.Drawing.Point(167, 3);
            this._save.Name = "_save";
            this._save.Size = new System.Drawing.Size(92, 27);
            this._save.TabIndex = 2;
            this._save.Text = "儲存錄製…";
            this._save.Click += new System.EventHandler(this.SaveRecording);
            // 
            // _play
            // 
            this._play.AutoSize = true;
            this._play.Location = new System.Drawing.Point(265, 3);
            this._play.Name = "_play";
            this._play.Size = new System.Drawing.Size(122, 27);
            this._play.TabIndex = 3;
            this._play.Text = "選擇檔案重播…";
            this._play.Click += new System.EventHandler(this.PlayRecording);
            // 
            // _instructions
            // 
            this._instructions.Dock = System.Windows.Forms.DockStyle.Fill;
            this._instructions.Location = new System.Drawing.Point(15, 62);
            this._instructions.Name = "_instructions";
            this._instructions.Size = new System.Drawing.Size(610, 142);
            this._instructions.TabIndex = 1;
            this._instructions.Text = "Ctrl+Shift+R：開啟此面板\r\n\r\nCtrl+Shift+F12：停止錄製 / 重播\r\n\r\n支援按鈕、文字、下拉選項、單選清單、數值與分頁。密碼不錄製。\r" +
    "\n不支援 AOI 畫布拖曳、原生檔案對話框、系統訊息框及自訂快捷鍵。\r\n重播前請回到錄製開始時的畫面，並使用相同語言與資料。";
            // 
            // _status
            // 
            this._status.Dock = System.Windows.Forms.DockStyle.Fill;
            this._status.Location = new System.Drawing.Point(15, 204);
            this._status.Name = "_status";
            this._status.Size = new System.Drawing.Size(610, 38);
            this._status.TabIndex = 2;
            // 
            // UiRecordingPanel
            // 
            this.ClientSize = new System.Drawing.Size(640, 254);
            this.Controls.Add(this._layout);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(640, 240);
            this.Name = "UiRecordingPanel";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "操作錄製 / 重播";
            this.TopMost = true;
            this._layout.ResumeLayout(false);
            this._buttons.ResumeLayout(false);
            this._buttons.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
    }
}
