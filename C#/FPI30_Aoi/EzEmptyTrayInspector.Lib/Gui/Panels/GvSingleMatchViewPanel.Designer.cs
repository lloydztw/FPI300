namespace EzEmptyTrayInspector.Gui.Panels
{
    using GvImageViewerClassT = JetEazy.OpenCV.Viewer.CvMatViewer;

    partial class GvSingleMatchViewPanel
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
            this.tableLayoutPanel0 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tblButtonsGroup = new System.Windows.Forms.TableLayoutPanel();
            this.btnCombine = new System.Windows.Forms.Button();
            this.btnCatchGolden = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnMatch = new System.Windows.Forms.Button();
            this.btnOpen = new System.Windows.Forms.Button();
            this.picIcon = new System.Windows.Forms.PictureBox();
            this.lblTitle = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.lblBlinker = new System.Windows.Forms.Label();
            this.lblCoordInfo = new System.Windows.Forms.Label();
            this.cvMatViewer1 = new JetEazy.OpenCV.Viewer.CvMatViewer();
            this.tableLayoutPanel0.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tblButtonsGroup.SuspendLayout();
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
            this.tableLayoutPanel0.Controls.Add(this.cvMatViewer1, 0, 1);
            this.tableLayoutPanel0.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel0.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel0.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.tableLayoutPanel0.Name = "tableLayoutPanel0";
            this.tableLayoutPanel0.RowCount = 3;
            this.tableLayoutPanel0.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel0.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel0.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel0.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 16F));
            this.tableLayoutPanel0.Size = new System.Drawing.Size(769, 543);
            this.tableLayoutPanel0.TabIndex = 17;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Black;
            this.panel1.Controls.Add(this.tblButtonsGroup);
            this.panel1.Controls.Add(this.picIcon);
            this.panel1.Controls.Add(this.lblTitle);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 1);
            this.panel1.Margin = new System.Windows.Forms.Padding(0, 1, 0, 1);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(0, 4, 6, 4);
            this.panel1.Size = new System.Drawing.Size(769, 38);
            this.panel1.TabIndex = 18;
            // 
            // tblButtonsGroup
            // 
            this.tblButtonsGroup.BackColor = System.Drawing.Color.Transparent;
            this.tblButtonsGroup.ColumnCount = 5;
            this.tblButtonsGroup.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tblButtonsGroup.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tblButtonsGroup.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tblButtonsGroup.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tblButtonsGroup.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tblButtonsGroup.Controls.Add(this.btnCombine, 0, 0);
            this.tblButtonsGroup.Controls.Add(this.btnCatchGolden, 4, 0);
            this.tblButtonsGroup.Controls.Add(this.btnClear, 3, 0);
            this.tblButtonsGroup.Controls.Add(this.btnMatch, 2, 0);
            this.tblButtonsGroup.Controls.Add(this.btnOpen, 1, 0);
            this.tblButtonsGroup.Dock = System.Windows.Forms.DockStyle.Right;
            this.tblButtonsGroup.Location = new System.Drawing.Point(457, 4);
            this.tblButtonsGroup.Margin = new System.Windows.Forms.Padding(0);
            this.tblButtonsGroup.Name = "tblButtonsGroup";
            this.tblButtonsGroup.RowCount = 1;
            this.tblButtonsGroup.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblButtonsGroup.Size = new System.Drawing.Size(306, 30);
            this.tblButtonsGroup.TabIndex = 19;
            // 
            // btnCombine
            // 
            this.btnCombine.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnCombine.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnCombine.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnCombine.Location = new System.Drawing.Point(1, 1);
            this.btnCombine.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.btnCombine.Name = "btnCombine";
            this.btnCombine.Size = new System.Drawing.Size(59, 28);
            this.btnCombine.TabIndex = 23;
            this.btnCombine.TabStop = false;
            this.btnCombine.Text = "合併";
            this.btnCombine.UseVisualStyleBackColor = false;
            this.btnCombine.Visible = false;
            // 
            // btnCatchGolden
            // 
            this.btnCatchGolden.BackColor = System.Drawing.Color.Gold;
            this.btnCatchGolden.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnCatchGolden.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnCatchGolden.Location = new System.Drawing.Point(245, 1);
            this.btnCatchGolden.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.btnCatchGolden.Name = "btnCatchGolden";
            this.btnCatchGolden.Size = new System.Drawing.Size(60, 28);
            this.btnCatchGolden.TabIndex = 22;
            this.btnCatchGolden.TabStop = false;
            this.btnCatchGolden.Text = "Golden";
            this.btnCatchGolden.UseVisualStyleBackColor = false;
            // 
            // btnClear
            // 
            this.btnClear.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnClear.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnClear.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnClear.Location = new System.Drawing.Point(184, 1);
            this.btnClear.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(59, 28);
            this.btnClear.TabIndex = 21;
            this.btnClear.TabStop = false;
            this.btnClear.Text = "Reset";
            this.btnClear.UseVisualStyleBackColor = false;
            // 
            // btnMatch
            // 
            this.btnMatch.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnMatch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnMatch.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnMatch.Location = new System.Drawing.Point(123, 1);
            this.btnMatch.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.btnMatch.Name = "btnMatch";
            this.btnMatch.Size = new System.Drawing.Size(59, 28);
            this.btnMatch.TabIndex = 20;
            this.btnMatch.TabStop = false;
            this.btnMatch.Text = "Match";
            this.btnMatch.UseVisualStyleBackColor = false;
            // 
            // btnOpen
            // 
            this.btnOpen.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnOpen.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnOpen.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnOpen.Location = new System.Drawing.Point(62, 1);
            this.btnOpen.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(59, 28);
            this.btnOpen.TabIndex = 19;
            this.btnOpen.TabStop = false;
            this.btnOpen.Text = "Open";
            this.btnOpen.UseVisualStyleBackColor = false;
            // 
            // picIcon
            // 
            this.picIcon.Image = global::EzEmptyTrayInspector.Properties.Resources.file_png_icon;
            this.picIcon.Location = new System.Drawing.Point(4, 4);
            this.picIcon.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.picIcon.Name = "picIcon";
            this.picIcon.Size = new System.Drawing.Size(27, 29);
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
            this.lblTitle.Location = new System.Drawing.Point(0, 4);
            this.lblTitle.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Padding = new System.Windows.Forms.Padding(37, 0, 0, 0);
            this.lblTitle.Size = new System.Drawing.Size(763, 30);
            this.lblTitle.TabIndex = 17;
            this.lblTitle.Text = "Source Name";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.Black;
            this.panel2.Controls.Add(this.lblBlinker);
            this.panel2.Controls.Add(this.lblCoordInfo);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 504);
            this.panel2.Margin = new System.Windows.Forms.Padding(0, 1, 0, 1);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(769, 38);
            this.panel2.TabIndex = 17;
            // 
            // lblBlinker
            // 
            this.lblBlinker.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblBlinker.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblBlinker.ForeColor = System.Drawing.Color.White;
            this.lblBlinker.Location = new System.Drawing.Point(10, 9);
            this.lblBlinker.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblBlinker.Name = "lblBlinker";
            this.lblBlinker.Size = new System.Drawing.Size(17, 17);
            this.lblBlinker.TabIndex = 18;
            // 
            // lblCoordInfo
            // 
            this.lblCoordInfo.BackColor = System.Drawing.Color.Transparent;
            this.lblCoordInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCoordInfo.Font = new System.Drawing.Font("微軟正黑體", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblCoordInfo.ForeColor = System.Drawing.Color.Lime;
            this.lblCoordInfo.Location = new System.Drawing.Point(0, 0);
            this.lblCoordInfo.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblCoordInfo.Name = "lblCoordInfo";
            this.lblCoordInfo.Padding = new System.Windows.Forms.Padding(37, 0, 0, 0);
            this.lblCoordInfo.Size = new System.Drawing.Size(769, 38);
            this.lblCoordInfo.TabIndex = 17;
            this.lblCoordInfo.Text = "Camera Viewer";
            this.lblCoordInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
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
            this.cvMatViewer1.Location = new System.Drawing.Point(0, 41);
            this.cvMatViewer1.Margin = new System.Windows.Forms.Padding(0, 1, 0, 1);
            this.cvMatViewer1.Name = "cvMatViewer1";
            this.cvMatViewer1.RulerVisible = false;
            this.cvMatViewer1.Size = new System.Drawing.Size(769, 461);
            this.cvMatViewer1.TabIndex = 16;
            // 
            // GvSingleMatchViewPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel0);
            this.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.Name = "GvSingleMatchViewPanel";
            this.Size = new System.Drawing.Size(769, 543);
            this.tableLayoutPanel0.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.tblButtonsGroup.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picIcon)).EndInit();
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel0;
        private System.Windows.Forms.Panel panel2;
        private GvImageViewerClassT cvMatViewer1;
        private System.Windows.Forms.Panel panel1;
        public System.Windows.Forms.Button btnOpen;
        public System.Windows.Forms.Label lblBlinker;
        public System.Windows.Forms.Label lblCoordInfo;
        public System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.TableLayoutPanel tblButtonsGroup;
        public System.Windows.Forms.Button btnClear;
        public System.Windows.Forms.Button btnMatch;
        public System.Windows.Forms.Button btnCatchGolden;
        public System.Windows.Forms.Button btnCombine;
        public System.Windows.Forms.PictureBox picIcon;
    }
}
