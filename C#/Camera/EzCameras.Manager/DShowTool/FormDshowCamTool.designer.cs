namespace JetEazy.DShow.Tool
{
    partial class FormDshowCamTool
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該公開 Managed 資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改這個方法的內容。
        ///
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormDshowCamTool));
            this.btnRecStop = new System.Windows.Forms.Button();
            this.btnRecord = new System.Windows.Forms.Button();
            this.btnSaveSettings = new System.Windows.Forms.Button();
            this.btnLoadSettings = new System.Windows.Forms.Button();
            this.btnSnapshot = new System.Windows.Forms.Button();
            this.btnPinConfig = new System.Windows.Forms.Button();
            this.btnCamSettings = new System.Windows.Forms.Button();
            this.tableLayoutPanelAtLeft = new System.Windows.Forms.TableLayoutPanel();
            this.panelMedia = new System.Windows.Forms.Panel();
            this.cboAvailableAudios = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cboAvailableCameras = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.lblInfo = new System.Windows.Forms.Label();
            this.tableLayoutPanel0 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanelAtRight = new System.Windows.Forms.TableLayoutPanel();
            this.lblCurrentFps = new System.Windows.Forms.Label();
            this.gPaneUsbCameraViewer1 = new JetEazy.DShow.GUI.GvDshowCameraViewer();
            this.cboAvailableSizes = new System.Windows.Forms.ComboBox();
            this.numFps = new System.Windows.Forms.NumericUpDown();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanelAtLeft.SuspendLayout();
            this.panelMedia.SuspendLayout();
            this.tableLayoutPanel0.SuspendLayout();
            this.tableLayoutPanelAtRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numFps)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnRecStop
            // 
            this.btnRecStop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.btnRecStop.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnRecStop.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnRecStop.Location = new System.Drawing.Point(20, 335);
            this.btnRecStop.Margin = new System.Windows.Forms.Padding(20, 0, 20, 0);
            this.btnRecStop.Name = "btnRecStop";
            this.btnRecStop.Size = new System.Drawing.Size(227, 34);
            this.btnRecStop.TabIndex = 41;
            this.btnRecStop.Text = "停止 錄影";
            this.btnRecStop.UseVisualStyleBackColor = false;
            this.btnRecStop.Click += new System.EventHandler(this.btnRecStop_Click);
            // 
            // btnRecord
            // 
            this.btnRecord.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.btnRecord.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnRecord.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnRecord.Location = new System.Drawing.Point(20, 297);
            this.btnRecord.Margin = new System.Windows.Forms.Padding(20, 0, 20, 0);
            this.btnRecord.Name = "btnRecord";
            this.btnRecord.Size = new System.Drawing.Size(227, 34);
            this.btnRecord.TabIndex = 40;
            this.btnRecord.Text = "測試 錄影";
            this.btnRecord.UseVisualStyleBackColor = false;
            this.btnRecord.Click += new System.EventHandler(this.btnRecord_Click);
            // 
            // btnSaveSettings
            // 
            this.btnSaveSettings.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.btnSaveSettings.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnSaveSettings.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnSaveSettings.Location = new System.Drawing.Point(20, 411);
            this.btnSaveSettings.Margin = new System.Windows.Forms.Padding(20, 0, 20, 0);
            this.btnSaveSettings.Name = "btnSaveSettings";
            this.btnSaveSettings.Size = new System.Drawing.Size(227, 34);
            this.btnSaveSettings.TabIndex = 39;
            this.btnSaveSettings.Text = "保存 ini";
            this.btnSaveSettings.UseVisualStyleBackColor = false;
            this.btnSaveSettings.Click += new System.EventHandler(this.btnSaveSettings_Click);
            // 
            // btnLoadSettings
            // 
            this.btnLoadSettings.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.btnLoadSettings.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnLoadSettings.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnLoadSettings.Location = new System.Drawing.Point(20, 373);
            this.btnLoadSettings.Margin = new System.Windows.Forms.Padding(20, 0, 20, 0);
            this.btnLoadSettings.Name = "btnLoadSettings";
            this.btnLoadSettings.Size = new System.Drawing.Size(227, 34);
            this.btnLoadSettings.TabIndex = 38;
            this.btnLoadSettings.Text = "載入 ini";
            this.btnLoadSettings.UseVisualStyleBackColor = false;
            this.btnLoadSettings.Click += new System.EventHandler(this.btnLoadSettings_Click);
            // 
            // btnSnapshot
            // 
            this.btnSnapshot.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.btnSnapshot.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnSnapshot.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnSnapshot.Location = new System.Drawing.Point(20, 259);
            this.btnSnapshot.Margin = new System.Windows.Forms.Padding(20, 0, 20, 0);
            this.btnSnapshot.Name = "btnSnapshot";
            this.btnSnapshot.Size = new System.Drawing.Size(227, 34);
            this.btnSnapshot.TabIndex = 35;
            this.btnSnapshot.Text = "測試 拍照";
            this.btnSnapshot.UseVisualStyleBackColor = false;
            this.btnSnapshot.Click += new System.EventHandler(this.btnSnapshot_Click);
            // 
            // btnPinConfig
            // 
            this.btnPinConfig.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnPinConfig.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnPinConfig.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnPinConfig.Location = new System.Drawing.Point(20, 183);
            this.btnPinConfig.Margin = new System.Windows.Forms.Padding(20, 0, 20, 0);
            this.btnPinConfig.Name = "btnPinConfig";
            this.btnPinConfig.Size = new System.Drawing.Size(227, 34);
            this.btnPinConfig.TabIndex = 34;
            this.btnPinConfig.Text = "視頻 組態";
            this.btnPinConfig.UseVisualStyleBackColor = false;
            this.btnPinConfig.Click += new System.EventHandler(this.btnPinConfig_Click);
            // 
            // btnCamSettings
            // 
            this.btnCamSettings.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnCamSettings.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnCamSettings.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnCamSettings.Location = new System.Drawing.Point(20, 221);
            this.btnCamSettings.Margin = new System.Windows.Forms.Padding(20, 0, 20, 0);
            this.btnCamSettings.Name = "btnCamSettings";
            this.btnCamSettings.Size = new System.Drawing.Size(227, 34);
            this.btnCamSettings.TabIndex = 29;
            this.btnCamSettings.Text = "相機 設定";
            this.btnCamSettings.UseVisualStyleBackColor = false;
            this.btnCamSettings.Click += new System.EventHandler(this.btnCamSettings_Click);
            // 
            // tableLayoutPanelAtLeft
            // 
            this.tableLayoutPanelAtLeft.BackColor = System.Drawing.Color.LightSteelBlue;
            this.tableLayoutPanelAtLeft.ColumnCount = 1;
            this.tableLayoutPanelAtLeft.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelAtLeft.Controls.Add(this.panelMedia, 0, 0);
            this.tableLayoutPanelAtLeft.Controls.Add(this.btnSaveSettings, 0, 7);
            this.tableLayoutPanelAtLeft.Controls.Add(this.btnRecStop, 0, 5);
            this.tableLayoutPanelAtLeft.Controls.Add(this.btnLoadSettings, 0, 6);
            this.tableLayoutPanelAtLeft.Controls.Add(this.btnPinConfig, 0, 1);
            this.tableLayoutPanelAtLeft.Controls.Add(this.btnRecord, 0, 4);
            this.tableLayoutPanelAtLeft.Controls.Add(this.btnCamSettings, 0, 2);
            this.tableLayoutPanelAtLeft.Controls.Add(this.btnSnapshot, 0, 3);
            this.tableLayoutPanelAtLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelAtLeft.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanelAtLeft.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanelAtLeft.Name = "tableLayoutPanelAtLeft";
            this.tableLayoutPanelAtLeft.RowCount = 9;
            this.tableLayoutPanelAtLeft.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 183F));
            this.tableLayoutPanelAtLeft.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.tableLayoutPanelAtLeft.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.tableLayoutPanelAtLeft.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.tableLayoutPanelAtLeft.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.tableLayoutPanelAtLeft.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.tableLayoutPanelAtLeft.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.tableLayoutPanelAtLeft.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.tableLayoutPanelAtLeft.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelAtLeft.Size = new System.Drawing.Size(267, 556);
            this.tableLayoutPanelAtLeft.TabIndex = 40;
            // 
            // panelMedia
            // 
            this.panelMedia.BackColor = System.Drawing.Color.LightSteelBlue;
            this.panelMedia.Controls.Add(this.cboAvailableAudios);
            this.panelMedia.Controls.Add(this.label2);
            this.panelMedia.Controls.Add(this.cboAvailableCameras);
            this.panelMedia.Controls.Add(this.label1);
            this.panelMedia.Controls.Add(this.lblInfo);
            this.panelMedia.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMedia.Location = new System.Drawing.Point(3, 3);
            this.panelMedia.Name = "panelMedia";
            this.panelMedia.Padding = new System.Windows.Forms.Padding(8, 2, 8, 16);
            this.panelMedia.Size = new System.Drawing.Size(261, 177);
            this.panelMedia.TabIndex = 43;
            // 
            // cboAvailableAudios
            // 
            this.cboAvailableAudios.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboAvailableAudios.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.cboAvailableAudios.FormattingEnabled = true;
            this.cboAvailableAudios.Location = new System.Drawing.Point(71, 43);
            this.cboAvailableAudios.Name = "cboAvailableAudios";
            this.cboAvailableAudios.Size = new System.Drawing.Size(174, 27);
            this.cboAvailableAudios.TabIndex = 33;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Font = new System.Drawing.Font("微軟正黑體", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Black;
            this.label2.Location = new System.Drawing.Point(11, 45);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(44, 22);
            this.label2.TabIndex = 32;
            this.label2.Text = "音訊";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cboAvailableCameras
            // 
            this.cboAvailableCameras.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboAvailableCameras.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.cboAvailableCameras.FormattingEnabled = true;
            this.cboAvailableCameras.Location = new System.Drawing.Point(71, 15);
            this.cboAvailableCameras.Name = "cboAvailableCameras";
            this.cboAvailableCameras.Size = new System.Drawing.Size(174, 27);
            this.cboAvailableCameras.TabIndex = 31;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("微軟正黑體", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Black;
            this.label1.Location = new System.Drawing.Point(11, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 22);
            this.label1.TabIndex = 30;
            this.label1.Text = "相機";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblInfo
            // 
            this.lblInfo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.lblInfo.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblInfo.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblInfo.ForeColor = System.Drawing.Color.Black;
            this.lblInfo.Location = new System.Drawing.Point(8, 84);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.lblInfo.Size = new System.Drawing.Size(245, 77);
            this.lblInfo.TabIndex = 28;
            this.lblInfo.Text = "Format = \r\nSize = \r\nFps =";
            this.lblInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tableLayoutPanel0
            // 
            this.tableLayoutPanel0.ColumnCount = 2;
            this.tableLayoutPanel0.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 267F));
            this.tableLayoutPanel0.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel0.Controls.Add(this.tableLayoutPanelAtRight, 1, 0);
            this.tableLayoutPanel0.Controls.Add(this.tableLayoutPanelAtLeft, 0, 0);
            this.tableLayoutPanel0.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel0.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel0.Name = "tableLayoutPanel0";
            this.tableLayoutPanel0.RowCount = 1;
            this.tableLayoutPanel0.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel0.Size = new System.Drawing.Size(762, 556);
            this.tableLayoutPanel0.TabIndex = 41;
            // 
            // tableLayoutPanelAtRight
            // 
            this.tableLayoutPanelAtRight.ColumnCount = 1;
            this.tableLayoutPanelAtRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelAtRight.Controls.Add(this.lblCurrentFps, 0, 0);
            this.tableLayoutPanelAtRight.Controls.Add(this.gPaneUsbCameraViewer1, 0, 1);
            this.tableLayoutPanelAtRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelAtRight.Location = new System.Drawing.Point(267, 0);
            this.tableLayoutPanelAtRight.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanelAtRight.Name = "tableLayoutPanelAtRight";
            this.tableLayoutPanelAtRight.RowCount = 2;
            this.tableLayoutPanelAtRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanelAtRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelAtRight.Size = new System.Drawing.Size(495, 556);
            this.tableLayoutPanelAtRight.TabIndex = 43;
            // 
            // lblCurrentFps
            // 
            this.lblCurrentFps.BackColor = System.Drawing.Color.Black;
            this.lblCurrentFps.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCurrentFps.Font = new System.Drawing.Font("Consolas", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCurrentFps.ForeColor = System.Drawing.Color.Lime;
            this.lblCurrentFps.Location = new System.Drawing.Point(0, 0);
            this.lblCurrentFps.Margin = new System.Windows.Forms.Padding(0);
            this.lblCurrentFps.Name = "lblCurrentFps";
            this.lblCurrentFps.Padding = new System.Windows.Forms.Padding(5);
            this.lblCurrentFps.Size = new System.Drawing.Size(495, 50);
            this.lblCurrentFps.TabIndex = 44;
            this.lblCurrentFps.Text = "FPS = 0";
            this.lblCurrentFps.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // gPaneUsbCameraViewer1
            // 
            this.gPaneUsbCameraViewer1.BackColor = System.Drawing.Color.Black;
            this.gPaneUsbCameraViewer1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.gPaneUsbCameraViewer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gPaneUsbCameraViewer1.Location = new System.Drawing.Point(0, 50);
            this.gPaneUsbCameraViewer1.Margin = new System.Windows.Forms.Padding(0);
            this.gPaneUsbCameraViewer1.Name = "gPaneUsbCameraViewer1";
            this.gPaneUsbCameraViewer1.OptUseBlinker = false;
            this.gPaneUsbCameraViewer1.Size = new System.Drawing.Size(495, 506);
            this.gPaneUsbCameraViewer1.TabIndex = 0;
            // 
            // cboAvailableSizes
            // 
            this.cboAvailableSizes.Font = new System.Drawing.Font("微軟正黑體", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.cboAvailableSizes.FormattingEnabled = true;
            this.cboAvailableSizes.Location = new System.Drawing.Point(20, 104);
            this.cboAvailableSizes.Name = "cboAvailableSizes";
            this.cboAvailableSizes.Size = new System.Drawing.Size(247, 25);
            this.cboAvailableSizes.TabIndex = 32;
            // 
            // numFps
            // 
            this.numFps.Location = new System.Drawing.Point(20, 135);
            this.numFps.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numFps.Name = "numFps";
            this.numFps.Size = new System.Drawing.Size(247, 25);
            this.numFps.TabIndex = 33;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.numFps);
            this.groupBox1.Controls.Add(this.cboAvailableSizes);
            this.groupBox1.Location = new System.Drawing.Point(773, 23);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(292, 171);
            this.groupBox1.TabIndex = 42;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "隱藏區";
            this.groupBox1.Visible = false;
            // 
            // FormDshowCamTool
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1121, 628);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.tableLayoutPanel0);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "FormDshowCamTool";
            this.Padding = new System.Windows.Forms.Padding(2, 0, 2, 2);
            this.Text = "USB 錄影相機 調適工具";
            this.tableLayoutPanelAtLeft.ResumeLayout(false);
            this.panelMedia.ResumeLayout(false);
            this.panelMedia.PerformLayout();
            this.tableLayoutPanel0.ResumeLayout(false);
            this.tableLayoutPanelAtRight.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numFps)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private JetEazy.DShow.GUI.GvDshowCameraViewer gPaneUsbCameraViewer1;
        private System.Windows.Forms.Button btnCamSettings;
        private System.Windows.Forms.Button btnPinConfig;
        private System.Windows.Forms.Button btnSnapshot;
        private System.Windows.Forms.Button btnSaveSettings;
        private System.Windows.Forms.Button btnLoadSettings;
        private System.Windows.Forms.Button btnRecord;
        private System.Windows.Forms.Button btnRecStop;
        //private Paso.GUI.Common.GwPaneClock gwPaneClock1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelAtLeft;
        private System.Windows.Forms.Panel panelMedia;
        private System.Windows.Forms.ComboBox cboAvailableCameras;
        internal System.Windows.Forms.Label label1;
        internal System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel0;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelAtRight;
        internal System.Windows.Forms.Label lblCurrentFps;
        private System.Windows.Forms.ComboBox cboAvailableAudios;
        internal System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cboAvailableSizes;
        private System.Windows.Forms.NumericUpDown numFps;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}