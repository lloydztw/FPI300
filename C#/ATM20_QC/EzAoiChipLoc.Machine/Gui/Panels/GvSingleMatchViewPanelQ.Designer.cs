namespace EzDualMatch.Gui.Panels
{
    partial class GvSingleMatchViewPanelQ
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GvSingleMatchViewPanelQ));
            this.cvzQuickImageViewPanel1 = new JetEazy.OpenCV.Viewer.CvzQuickImageViewPanel();
            this.SuspendLayout();
            // 
            // cvzQuickImageViewPanel1
            // 
            this.cvzQuickImageViewPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cvzQuickImageViewPanel1.Font = new System.Drawing.Font("新細明體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.cvzQuickImageViewPanel1.Icon = ((System.Drawing.Image)(resources.GetObject("cvzQuickImageViewPanel1.Icon")));
            this.cvzQuickImageViewPanel1.Image = null;
            this.cvzQuickImageViewPanel1.Location = new System.Drawing.Point(0, 0);
            this.cvzQuickImageViewPanel1.Margin = new System.Windows.Forms.Padding(1);
            this.cvzQuickImageViewPanel1.Name = "cvzQuickImageViewPanel1";
            this.cvzQuickImageViewPanel1.OptAutoPersistLastFile = false;
            this.cvzQuickImageViewPanel1.OptCoordInfoVisible = true;
            this.cvzQuickImageViewPanel1.OptTitleBarVisible = true;
            this.cvzQuickImageViewPanel1.Padding = new System.Windows.Forms.Padding(1);
            this.cvzQuickImageViewPanel1.Size = new System.Drawing.Size(657, 385);
            this.cvzQuickImageViewPanel1.TabIndex = 1;
            this.cvzQuickImageViewPanel1.ViewID = 0;
            // 
            // GvSingleMatchQuickViewPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cvzQuickImageViewPanel1);
            this.Name = "GvSingleMatchQuickViewPanel";
            this.Size = new System.Drawing.Size(657, 385);
            this.ResumeLayout(false);

        }

        #endregion

        private JetEazy.OpenCV.Viewer.CvzQuickImageViewPanel cvzQuickImageViewPanel1;
    }
}
