

namespace JetEazy.OpenCV.Viewer.Develop
{
    using ImageViewerClassT = CvMatViewer;

    partial class CvzQuickImageViewPanel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CvzQuickImageViewPanel));
            this.tableLayoutPanel0 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tblTitleWidgetsGrp = new System.Windows.Forms.TableLayoutPanel();
            this.btnOpen = new System.Windows.Forms.Button();
            this.picIcon = new System.Windows.Forms.PictureBox();
            this.lblTitle = new System.Windows.Forms.Label();
            this.cvMatViewer1 = new JetEazy.OpenCV.Viewer.CvMatViewer();
            this.panel2 = new System.Windows.Forms.Panel();
            this.lblBlinker = new System.Windows.Forms.Label();
            this.lblCoordInfo = new System.Windows.Forms.Label();
            this.tableLayoutPanel0.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tblTitleWidgetsGrp.SuspendLayout();
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
            this.tableLayoutPanel0.Controls.Add(this.cvMatViewer1, 0, 1);
            this.tableLayoutPanel0.Controls.Add(this.panel2, 0, 2);
            this.tableLayoutPanel0.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel0.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel0.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.tableLayoutPanel0.Name = "tableLayoutPanel0";
            this.tableLayoutPanel0.RowCount = 3;
            this.tableLayoutPanel0.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel0.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel0.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel0.Size = new System.Drawing.Size(812, 524);
            this.tableLayoutPanel0.TabIndex = 17;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Black;
            this.panel1.Controls.Add(this.tblTitleWidgetsGrp);
            this.panel1.Controls.Add(this.picIcon);
            this.panel1.Controls.Add(this.lblTitle);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 2);
            this.panel1.Margin = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(0, 6, 9, 6);
            this.panel1.Size = new System.Drawing.Size(812, 56);
            this.panel1.TabIndex = 18;
            // 
            // tblTitleWidgetsGrp
            // 
            this.tblTitleWidgetsGrp.BackColor = System.Drawing.Color.Transparent;
            this.tblTitleWidgetsGrp.ColumnCount = 1;
            this.tblTitleWidgetsGrp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tblTitleWidgetsGrp.Controls.Add(this.btnOpen, 0, 0);
            this.tblTitleWidgetsGrp.Dock = System.Windows.Forms.DockStyle.Right;
            this.tblTitleWidgetsGrp.Location = new System.Drawing.Point(703, 6);
            this.tblTitleWidgetsGrp.Margin = new System.Windows.Forms.Padding(0);
            this.tblTitleWidgetsGrp.Name = "tblTitleWidgetsGrp";
            this.tblTitleWidgetsGrp.RowCount = 1;
            this.tblTitleWidgetsGrp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblTitleWidgetsGrp.Size = new System.Drawing.Size(100, 44);
            this.tblTitleWidgetsGrp.TabIndex = 19;
            // 
            // btnOpen
            // 
            this.btnOpen.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnOpen.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnOpen.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnOpen.Location = new System.Drawing.Point(2, 2);
            this.btnOpen.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(98, 40);
            this.btnOpen.TabIndex = 20;
            this.btnOpen.TabStop = false;
            this.btnOpen.Text = "Open";
            this.btnOpen.UseVisualStyleBackColor = false;
            // 
            // picIcon
            // 
            this.picIcon.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.picIcon.ErrorImage = null;
            this.picIcon.Image = ((System.Drawing.Image)(resources.GetObject("picIcon.Image")));
            this.picIcon.InitialImage = null;
            this.picIcon.Location = new System.Drawing.Point(6, 6);
            this.picIcon.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.picIcon.Name = "picIcon";
            this.picIcon.Size = new System.Drawing.Size(40, 44);
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
            this.lblTitle.Location = new System.Drawing.Point(0, 6);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Padding = new System.Windows.Forms.Padding(56, 0, 0, 0);
            this.lblTitle.Size = new System.Drawing.Size(803, 44);
            this.lblTitle.TabIndex = 17;
            this.lblTitle.Text = "Source Name";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cvMatViewer1
            // 
            this.cvMatViewer1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.cvMatViewer1.ClearLiveBackgroundEnabled = true;
            this.cvMatViewer1.CrosshairsColor = System.Drawing.Color.Gold;
            this.cvMatViewer1.CrosshairsVisible = false;
            this.cvMatViewer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cvMatViewer1.ForeColor = System.Drawing.Color.Black;
            this.cvMatViewer1.GridLineColor = System.Drawing.Color.LightGray;
            this.cvMatViewer1.GridLinesVisible = false;
            this.cvMatViewer1.Image = null;
            this.cvMatViewer1.Location = new System.Drawing.Point(0, 61);
            this.cvMatViewer1.Margin = new System.Windows.Forms.Padding(0, 1, 0, 1);
            this.cvMatViewer1.Name = "cvMatViewer1";
            this.cvMatViewer1.RulerVisible = false;
            this.cvMatViewer1.Size = new System.Drawing.Size(812, 402);
            this.cvMatViewer1.TabIndex = 16;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.Black;
            this.panel2.Controls.Add(this.lblBlinker);
            this.panel2.Controls.Add(this.lblCoordInfo);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 466);
            this.panel2.Margin = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(812, 56);
            this.panel2.TabIndex = 17;
            // 
            // lblBlinker
            // 
            this.lblBlinker.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblBlinker.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblBlinker.ForeColor = System.Drawing.Color.White;
            this.lblBlinker.Location = new System.Drawing.Point(15, 14);
            this.lblBlinker.Name = "lblBlinker";
            this.lblBlinker.Size = new System.Drawing.Size(24, 24);
            this.lblBlinker.TabIndex = 18;
            // 
            // lblCoordInfo
            // 
            this.lblCoordInfo.BackColor = System.Drawing.Color.Transparent;
            this.lblCoordInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCoordInfo.Font = new System.Drawing.Font("微軟正黑體", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblCoordInfo.ForeColor = System.Drawing.Color.Lime;
            this.lblCoordInfo.Location = new System.Drawing.Point(0, 0);
            this.lblCoordInfo.Name = "lblCoordInfo";
            this.lblCoordInfo.Padding = new System.Windows.Forms.Padding(56, 0, 0, 0);
            this.lblCoordInfo.Size = new System.Drawing.Size(812, 56);
            this.lblCoordInfo.TabIndex = 17;
            this.lblCoordInfo.Text = "Coordinate Info";
            this.lblCoordInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // CvzQuickImageViewPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel0);
            this.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.Name = "CvzQuickImageViewPanel";
            this.Size = new System.Drawing.Size(812, 524);
            this.tableLayoutPanel0.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.tblTitleWidgetsGrp.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picIcon)).EndInit();
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel0;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel1;
        public System.Windows.Forms.Label lblBlinker;
        public System.Windows.Forms.Label lblCoordInfo;
        public System.Windows.Forms.Label lblTitle;
        public System.Windows.Forms.PictureBox picIcon;
        public System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.TableLayoutPanel tblTitleWidgetsGrp;
        private ImageViewerClassT cvMatViewer1;
    }
}
