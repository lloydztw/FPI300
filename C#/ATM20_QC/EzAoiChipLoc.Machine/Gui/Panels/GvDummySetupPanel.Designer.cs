namespace EzAoiChipLocQC.Gui.Panels
{
    partial class GvDummySetupPanel
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
            this.lblDummy = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblDummy
            // 
            this.lblDummy.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.lblDummy.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDummy.Font = new System.Drawing.Font("微軟正黑體", 25.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblDummy.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.lblDummy.Location = new System.Drawing.Point(0, 0);
            this.lblDummy.Name = "lblDummy";
            this.lblDummy.Size = new System.Drawing.Size(484, 640);
            this.lblDummy.TabIndex = 308;
            this.lblDummy.Text = "請於主控程式設定";
            this.lblDummy.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // GvSetupPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.Controls.Add(this.lblDummy);
            this.Name = "GvSetupPanel";
            this.Size = new System.Drawing.Size(484, 640);
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.Label lblDummy;
    }
}
