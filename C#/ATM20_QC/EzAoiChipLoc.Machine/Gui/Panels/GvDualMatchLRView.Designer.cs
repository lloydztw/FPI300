using SingleMatchViewPanelClassT = EzDualMatch.Gui.Panels.GvSingleMatchViewPanel;

namespace EzDualMatch.Gui.Panels
{
    partial class GvDualMatchLRView
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.gvSingleMatchView1 = new SingleMatchViewPanelClassT();
            this.gvSingleMatchView2 = new SingleMatchViewPanelClassT();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Location = new System.Drawing.Point(2, 12);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.gvSingleMatchView1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.gvSingleMatchView2);
            this.splitContainer1.Size = new System.Drawing.Size(782, 473);
            this.splitContainer1.SplitterDistance = 387;
            this.splitContainer1.SplitterWidth = 2;
            this.splitContainer1.TabIndex = 1;
            // 
            // gvSingleMatchView1
            // 
            this.gvSingleMatchView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gvSingleMatchView1.Location = new System.Drawing.Point(0, 0);
            this.gvSingleMatchView1.Margin = new System.Windows.Forms.Padding(0);
            this.gvSingleMatchView1.Name = "gvSingleMatchView1";
            this.gvSingleMatchView1.Padding = new System.Windows.Forms.Padding(2, 0, 1, 0);
            this.gvSingleMatchView1.Size = new System.Drawing.Size(387, 473);
            this.gvSingleMatchView1.TabIndex = 1;
            // 
            // gvSingleMatchView2
            // 
            this.gvSingleMatchView2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gvSingleMatchView2.Location = new System.Drawing.Point(0, 0);
            this.gvSingleMatchView2.Margin = new System.Windows.Forms.Padding(0);
            this.gvSingleMatchView2.Name = "gvSingleMatchView2";
            this.gvSingleMatchView2.Padding = new System.Windows.Forms.Padding(1, 0, 2, 0);
            this.gvSingleMatchView2.Size = new System.Drawing.Size(393, 473);
            this.gvSingleMatchView2.TabIndex = 2;
            // 
            // GvDualMatchLRView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "GvDualMatchLRView";
            this.Size = new System.Drawing.Size(812, 512);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.SplitContainer splitContainer1;
        public SingleMatchViewPanelClassT gvSingleMatchView1;
        public SingleMatchViewPanelClassT gvSingleMatchView2;
    }
}
