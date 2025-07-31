using EzImageViewer = EzCamera.GUI.QzCameraViewer;

namespace JzDisplay.UISpace
{
    partial class DispUI
    {
        /// <summary> 
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置 Managed 資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 元件設計工具產生的程式碼

        /// <summary> 
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改
        /// 這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.ezImageViewer = new EzCamera.GUI.QzCameraViewer();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.label1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label1.Font = new System.Drawing.Font("新細明體", 9F);
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(1480, 34);
            this.label1.TabIndex = 1;
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(258, 123);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(390, 60);
            this.label2.TabIndex = 2;
            this.label2.Text = "label2";
            this.label2.Visible = false;
            // 
            // ezImageViewer
            // 
            this.ezImageViewer.BackColor = System.Drawing.Color.Black;
            this.ezImageViewer.ClearLiveBackgroundEnabled = true;
            this.ezImageViewer.CrosshairsColor = System.Drawing.Color.Gold;
            this.ezImageViewer.CrosshairsVisible = false;
            this.ezImageViewer.GridLineColor = System.Drawing.Color.LightGray;
            this.ezImageViewer.GridLinesVisible = false;
            this.ezImageViewer.Image = null;
            this.ezImageViewer.Location = new System.Drawing.Point(25, 62);
            this.ezImageViewer.Margin = new System.Windows.Forms.Padding(6);
            this.ezImageViewer.Name = "ezImageViewer";
            this.ezImageViewer.RulerVisible = false;
            this.ezImageViewer.Size = new System.Drawing.Size(848, 373);
            this.ezImageViewer.TabIndex = 3;
            // 
            // DispUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ezImageViewer);
            this.Cursor = System.Windows.Forms.Cursors.Default;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "DispUI";
            this.Size = new System.Drawing.Size(912, 488);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private EzImageViewer ezImageViewer;
    }
}
