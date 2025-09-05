namespace LaserAlignDX.UISpace.ChipCellsViewer
{
    partial class JezChipCellsViewPanel
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
            this.jezTransImageViewPanel1 = new global::GA.Mvc.Gui.JezTransImageViewPanel();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuLoadImage = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.menuTestChipInspect = new System.Windows.Forms.ToolStripMenuItem();
            this.menuTestQRCode = new System.Windows.Forms.ToolStripMenuItem();
            this.menuTestEmptyTrayInspect = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // jezTransImageViewPanel1
            // 
            this.jezTransImageViewPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.jezTransImageViewPanel1.Location = new System.Drawing.Point(0, 0);
            this.jezTransImageViewPanel1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.jezTransImageViewPanel1.Name = "jezTransImageViewPanel1";
            this.jezTransImageViewPanel1.Size = new System.Drawing.Size(541, 790);
            this.jezTransImageViewPanel1.TabIndex = 0;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuLoadImage,
            this.toolStripSeparator1,
            this.menuTestChipInspect,
            this.menuTestQRCode,
            this.menuTestEmptyTrayInspect});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(211, 134);
            // 
            // menuLoadImage
            // 
            this.menuLoadImage.Name = "menuLoadImage";
            this.menuLoadImage.Size = new System.Drawing.Size(210, 24);
            this.menuLoadImage.Text = "加載圖檔";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(207, 6);
            // 
            // menuTestChipInspect
            // 
            this.menuTestChipInspect.Name = "menuTestChipInspect";
            this.menuTestChipInspect.Size = new System.Drawing.Size(210, 24);
            this.menuTestChipInspect.Text = "離線測試 晶粒檢測";
            // 
            // menuTestQRCode
            // 
            this.menuTestQRCode.Name = "menuTestQRCode";
            this.menuTestQRCode.Size = new System.Drawing.Size(210, 24);
            this.menuTestQRCode.Text = "離線測試 QRCode";
            // 
            // menuTestEmptyTrayInspect
            // 
            this.menuTestEmptyTrayInspect.Name = "menuTestEmptyTrayInspect";
            this.menuTestEmptyTrayInspect.Size = new System.Drawing.Size(210, 24);
            this.menuTestEmptyTrayInspect.Text = "離線測試 空盤檢測";
            // 
            // JezChipCellsViewPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.jezTransImageViewPanel1);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "JezChipCellsViewPanel";
            this.Size = new System.Drawing.Size(541, 790);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private global::GA.Mvc.Gui.JezTransImageViewPanel jezTransImageViewPanel1;
        public System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        public System.Windows.Forms.ToolStripMenuItem menuLoadImage;
        public System.Windows.Forms.ToolStripMenuItem menuTestChipInspect;
        public System.Windows.Forms.ToolStripMenuItem menuTestQRCode;
        public System.Windows.Forms.ToolStripMenuItem menuTestEmptyTrayInspect;
        public System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
    }
}
