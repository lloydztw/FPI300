namespace EzAoiEmptyTrayInspector.Gui.Panels
{
    partial class GvMajorClientPanel
    {
        /// <summary> 
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置受控資源則為 true，否則為 false。</param>
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
            this.gvSingleMatchViewPanel1 = new EzAoiEmptyTrayInspector.Gui.Panels.GvSingleMatchViewPanel();
            this.gvSingleMatchViewPanel2 = new EzAoiEmptyTrayInspector.Gui.Panels.GvSingleMatchViewPanel();
            this.SuspendLayout();
            // 
            // gvSingleMatchViewPanel1
            // 
            this.gvSingleMatchViewPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gvSingleMatchViewPanel1.Location = new System.Drawing.Point(0, 0);
            this.gvSingleMatchViewPanel1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.gvSingleMatchViewPanel1.Name = "gvSingleMatchViewPanel1";
            this.gvSingleMatchViewPanel1.Size = new System.Drawing.Size(1110, 600);
            this.gvSingleMatchViewPanel1.TabIndex = 0;
            // 
            // gvSingleMatchViewPanel2
            // 
            this.gvSingleMatchViewPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gvSingleMatchViewPanel2.Location = new System.Drawing.Point(0, 0);
            this.gvSingleMatchViewPanel2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.gvSingleMatchViewPanel2.Name = "gvSingleMatchViewPanel2";
            this.gvSingleMatchViewPanel2.Size = new System.Drawing.Size(1110, 600);
            this.gvSingleMatchViewPanel2.TabIndex = 1;
            this.gvSingleMatchViewPanel2.Visible = false;
            // 
            // GvMajorClientPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gvSingleMatchViewPanel2);
            this.Controls.Add(this.gvSingleMatchViewPanel1);
            this.Name = "GvMajorClientPanel";
            this.Size = new System.Drawing.Size(1110, 600);
            this.ResumeLayout(false);

        }

        #endregion

        private GvSingleMatchViewPanel gvSingleMatchViewPanel1;
        private GvSingleMatchViewPanel gvSingleMatchViewPanel2;
    }
}
