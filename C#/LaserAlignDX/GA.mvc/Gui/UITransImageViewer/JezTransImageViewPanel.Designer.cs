namespace LaserAlignDX.Mvc.Gui
{
    using GvImageViewerClassT = JetEazy.OpenCV.Viewer.CvMatViewer;

    partial class JezTransImageViewPanel
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
            if (disposing && !IsDisposed)
                cleanUp();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(JezTransImageViewPanel));
            this.tableLayoutPanel0 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.picIcon = new System.Windows.Forms.PictureBox();
            this.lblTitle = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.lblShadowInfo = new System.Windows.Forms.Label();
            this.lblBlinker = new System.Windows.Forms.Label();
            this.lblCoordInfo = new System.Windows.Forms.Label();
            this.cvMatViewer = new JetEazy.OpenCV.Viewer.CvMatViewer();
            this.tableLayoutPanel0.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picIcon)).BeginInit();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel0
            // 
            this.tableLayoutPanel0.BackColor = System.Drawing.SystemColors.ControlDark;
            this.tableLayoutPanel0.ColumnCount = 1;
            this.tableLayoutPanel0.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel0.Controls.Add(this.panel1, 0, 0);
            this.tableLayoutPanel0.Controls.Add(this.panel2, 0, 2);
            this.tableLayoutPanel0.Controls.Add(this.cvMatViewer, 0, 1);
            this.tableLayoutPanel0.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel0.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel0.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.tableLayoutPanel0.Name = "tableLayoutPanel0";
            this.tableLayoutPanel0.RowCount = 3;
            this.tableLayoutPanel0.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel0.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel0.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel0.Size = new System.Drawing.Size(541, 790);
            this.tableLayoutPanel0.TabIndex = 17;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Black;
            this.panel1.Controls.Add(this.picIcon);
            this.panel1.Controls.Add(this.lblTitle);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 1);
            this.panel1.Margin = new System.Windows.Forms.Padding(0, 1, 0, 1);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(8, 5, 8, 5);
            this.panel1.Size = new System.Drawing.Size(541, 38);
            this.panel1.TabIndex = 18;
            // 
            // picIcon
            // 
            this.picIcon.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("picIcon.BackgroundImage")));
            this.picIcon.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.picIcon.Dock = System.Windows.Forms.DockStyle.Left;
            this.picIcon.Location = new System.Drawing.Point(8, 5);
            this.picIcon.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.picIcon.Name = "picIcon";
            this.picIcon.Size = new System.Drawing.Size(26, 28);
            this.picIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picIcon.TabIndex = 18;
            this.picIcon.TabStop = false;
            // 
            // lblTitle
            // 
            this.lblTitle.BackColor = System.Drawing.Color.Transparent;
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitle.Font = new System.Drawing.Font("微軟正黑體", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(8, 5);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Padding = new System.Windows.Forms.Padding(49, 0, 0, 0);
            this.lblTitle.Size = new System.Drawing.Size(525, 28);
            this.lblTitle.TabIndex = 17;
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.Black;
            this.panel2.Controls.Add(this.lblShadowInfo);
            this.panel2.Controls.Add(this.lblBlinker);
            this.panel2.Controls.Add(this.lblCoordInfo);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 751);
            this.panel2.Margin = new System.Windows.Forms.Padding(0, 1, 0, 1);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(541, 38);
            this.panel2.TabIndex = 17;
            // 
            // lblShadowInfo
            // 
            this.lblShadowInfo.AutoSize = true;
            this.lblShadowInfo.Location = new System.Drawing.Point(364, 11);
            this.lblShadowInfo.Name = "lblShadowInfo";
            this.lblShadowInfo.Size = new System.Drawing.Size(41, 15);
            this.lblShadowInfo.TabIndex = 19;
            this.lblShadowInfo.Text = "label1";
            this.lblShadowInfo.Visible = false;
            // 
            // lblBlinker
            // 
            this.lblBlinker.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblBlinker.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblBlinker.ForeColor = System.Drawing.Color.White;
            this.lblBlinker.Location = new System.Drawing.Point(13, 10);
            this.lblBlinker.Name = "lblBlinker";
            this.lblBlinker.Size = new System.Drawing.Size(16, 16);
            this.lblBlinker.TabIndex = 18;
            // 
            // lblCoordInfo
            // 
            this.lblCoordInfo.BackColor = System.Drawing.Color.Transparent;
            this.lblCoordInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCoordInfo.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCoordInfo.ForeColor = System.Drawing.Color.Lime;
            this.lblCoordInfo.Location = new System.Drawing.Point(0, 0);
            this.lblCoordInfo.Name = "lblCoordInfo";
            this.lblCoordInfo.Padding = new System.Windows.Forms.Padding(39, 0, 0, 0);
            this.lblCoordInfo.Size = new System.Drawing.Size(541, 38);
            this.lblCoordInfo.TabIndex = 17;
            this.lblCoordInfo.Text = "Camera Viewer";
            this.lblCoordInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cvMatViewer
            // 
            this.cvMatViewer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.cvMatViewer.ClearLiveBackgroundEnabled = true;
            this.cvMatViewer.CrosshairsColor = System.Drawing.Color.Gold;
            this.cvMatViewer.CrosshairsVisible = false;
            this.cvMatViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cvMatViewer.Font = new System.Drawing.Font("微軟正黑體", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.cvMatViewer.ForeColor = System.Drawing.Color.Black;
            this.cvMatViewer.GridLineColor = System.Drawing.Color.LightGray;
            this.cvMatViewer.GridLinesVisible = false;
            this.cvMatViewer.Image = null;
            this.cvMatViewer.Location = new System.Drawing.Point(0, 41);
            this.cvMatViewer.Margin = new System.Windows.Forms.Padding(0, 1, 0, 1);
            this.cvMatViewer.Name = "cvMatViewer";
            this.cvMatViewer.RulerVisible = false;
            this.cvMatViewer.Size = new System.Drawing.Size(541, 708);
            this.cvMatViewer.TabIndex = 16;
            // 
            // JezTransImageViewPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel0);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "JezTransImageViewPanel";
            this.Size = new System.Drawing.Size(541, 790);
            this.tableLayoutPanel0.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picIcon)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel0;
        private System.Windows.Forms.Panel panel2;
        private GvImageViewerClassT cvMatViewer;
        private System.Windows.Forms.Panel panel1;
        public System.Windows.Forms.Label lblBlinker;
        public System.Windows.Forms.Label lblCoordInfo;
        public System.Windows.Forms.Label lblTitle;
        public System.Windows.Forms.PictureBox picIcon;
        private System.Windows.Forms.Label lblShadowInfo;
    }
}
