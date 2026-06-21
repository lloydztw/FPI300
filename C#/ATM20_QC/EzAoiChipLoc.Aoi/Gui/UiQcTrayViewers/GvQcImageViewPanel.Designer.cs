namespace EzAoiChipLocQC.Gui.Panels
{
    using GvImageViewerClassT = JetEazy.OpenCV.Viewer.CvMatViewer;

    partial class GvQcImageViewPanel
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GvQcImageViewPanel));
            this.jxImageViewer1 = new AwFramework.Gui.JX.Gui.JXImageViewer();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.cvzQuickImageViewPanel1 = new JetEazy.OpenCV.Viewer.Develop.CvzQuickImageViewPanel();
            this.menuLoadImage = new System.Windows.Forms.ToolStripMenuItem();
            this.menuSnapshot = new System.Windows.Forms.ToolStripMenuItem();
            this.menuLiveMode = new System.Windows.Forms.ToolStripMenuItem();
            this.menuTestChipInspect = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // jxImageViewer1
            // 
            this.jxImageViewer1.JxiPath = "D:\\AUTOMATION\\Eazy FPI30\\Aoi.ATM20\\Ini";
            this.jxImageViewer1.JxSerialNumber = 0;
            this.jxImageViewer1.Location = new System.Drawing.Point(3, 129);
            this.jxImageViewer1.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.jxImageViewer1.Name = "jxImageViewer1";
            this.jxImageViewer1.Size = new System.Drawing.Size(487, 460);
            this.jxImageViewer1.TabIndex = 19;
            this.jxImageViewer1.Visible = false;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuLoadImage,
            this.menuSnapshot,
            this.menuLiveMode,
            this.toolStripSeparator1,
            this.menuTestChipInspect});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(271, 179);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(267, 6);
            // 
            // cvzQuickImageViewPanel1
            // 
            this.cvzQuickImageViewPanel1.Icon = ((System.Drawing.Image)(resources.GetObject("cvzQuickImageViewPanel1.Icon")));
            this.cvzQuickImageViewPanel1.Image = null;
            this.cvzQuickImageViewPanel1.Location = new System.Drawing.Point(496, 129);
            this.cvzQuickImageViewPanel1.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.cvzQuickImageViewPanel1.Name = "cvzQuickImageViewPanel1";
            this.cvzQuickImageViewPanel1.OptAutoPersistLastFile = true;
            this.cvzQuickImageViewPanel1.OptCoordInfoVisible = true;
            this.cvzQuickImageViewPanel1.OptTitleBarVisible = true;
            this.cvzQuickImageViewPanel1.Size = new System.Drawing.Size(482, 460);
            this.cvzQuickImageViewPanel1.TabIndex = 18;
            this.cvzQuickImageViewPanel1.ViewID = 0;
            // 
            // menuLoadImage
            // 
            this.menuLoadImage.Image = global::EzAoiChipLocQC.Properties.Resources.file_png_icon;
            this.menuLoadImage.Name = "menuLoadImage";
            this.menuLoadImage.Size = new System.Drawing.Size(270, 34);
            this.menuLoadImage.Text = "加載圖檔";
            // 
            // menuSnapshot
            // 
            this.menuSnapshot.Image = global::EzAoiChipLocQC.Properties.Resources.camera;
            this.menuSnapshot.Name = "menuSnapshot";
            this.menuSnapshot.Size = new System.Drawing.Size(270, 34);
            this.menuSnapshot.Text = "取像";
            // 
            // menuLiveMode
            // 
            this.menuLiveMode.Image = ((System.Drawing.Image)(resources.GetObject("menuLiveMode.Image")));
            this.menuLiveMode.Name = "menuLiveMode";
            this.menuLiveMode.Size = new System.Drawing.Size(270, 34);
            this.menuLiveMode.Text = "連續即時影像";
            // 
            // menuTestChipInspect
            // 
            this.menuTestChipInspect.Image = global::EzAoiChipLocQC.Properties.Resources.grid_3x3;
            this.menuTestChipInspect.Name = "menuTestChipInspect";
            this.menuTestChipInspect.Size = new System.Drawing.Size(270, 34);
            this.menuTestChipInspect.Text = "離線測試 晶粒檢測";
            // 
            // GvQcImageViewPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.Controls.Add(this.cvzQuickImageViewPanel1);
            this.Controls.Add(this.jxImageViewer1);
            this.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.Name = "GvQcImageViewPanel";
            this.Size = new System.Drawing.Size(1000, 800);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private JetEazy.OpenCV.Viewer.Develop.CvzQuickImageViewPanel cvzQuickImageViewPanel1;
        private AwFramework.Gui.JX.Gui.JXImageViewer jxImageViewer1;
        public System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        public System.Windows.Forms.ToolStripMenuItem menuLoadImage;
        public System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        public System.Windows.Forms.ToolStripMenuItem menuTestChipInspect;
        private System.Windows.Forms.ToolStripMenuItem menuSnapshot;
        private System.Windows.Forms.ToolStripMenuItem menuLiveMode;
    }
}
