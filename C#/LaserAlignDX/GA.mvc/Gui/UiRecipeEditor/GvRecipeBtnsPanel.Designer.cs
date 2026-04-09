namespace LaserAlignDX.Mvc.Gui
{
    partial class GvRecipeBtnsPanel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GvRecipeBtnsPanel));
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.btnWriteCoordsToPlc = new System.Windows.Forms.Button();
            this.btnOpenFlyCamRcpWindow = new System.Windows.Forms.Button();
            this.btnOpenLightCtrlWindow = new System.Windows.Forms.Button();
            this.btnSwitchToEmptyTray = new System.Windows.Forms.Button();
            this.btnSwitchToChipTemplate = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label0 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.gvCalibPointsDataGridView1 = new LaserAlignDX.Mvc.Gui.GvCalibPointsDataGridView();
            this.panelFocusZ = new System.Windows.Forms.Panel();
            this.lblFocusMotorZ = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btnFocusMotorGo = new System.Windows.Forms.Button();
            this.btnFocusMotorSettings = new System.Windows.Forms.Button();
            this.btnSaveImage = new System.Windows.Forms.Button();
            this.btnGrabImage = new System.Windows.Forms.Button();
            this.rdoCarrier2 = new System.Windows.Forms.RadioButton();
            this.rdoCarrier1 = new System.Windows.Forms.RadioButton();
            this.btnLoadImage = new System.Windows.Forms.Button();
            this.btnOpenTemplateMatchWindow = new System.Windows.Forms.Button();
            this.btnPickGoldenRegion = new System.Windows.Forms.Button();
            this.btnCreateCellRegions = new System.Windows.Forms.Button();
            this.btnOpenEmptyTrayWindow = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panelFocusZ.SuspendLayout();
            this.SuspendLayout();
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "sysSettings.png");
            // 
            // btnWriteCoordsToPlc
            // 
            this.btnWriteCoordsToPlc.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.btnWriteCoordsToPlc.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnWriteCoordsToPlc.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnWriteCoordsToPlc.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnWriteCoordsToPlc.Location = new System.Drawing.Point(1109, 6);
            this.btnWriteCoordsToPlc.Margin = new System.Windows.Forms.Padding(12, 6, 18, 3);
            this.btnWriteCoordsToPlc.Name = "btnWriteCoordsToPlc";
            this.btnWriteCoordsToPlc.Size = new System.Drawing.Size(161, 33);
            this.btnWriteCoordsToPlc.TabIndex = 4;
            this.btnWriteCoordsToPlc.Text = "將座標寫入 PLC";
            this.btnWriteCoordsToPlc.UseVisualStyleBackColor = false;
            // 
            // btnOpenFlyCamRcpWindow
            // 
            this.btnOpenFlyCamRcpWindow.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnOpenFlyCamRcpWindow.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnOpenFlyCamRcpWindow.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOpenFlyCamRcpWindow.Location = new System.Drawing.Point(444, 6);
            this.btnOpenFlyCamRcpWindow.Margin = new System.Windows.Forms.Padding(12, 6, 12, 3);
            this.btnOpenFlyCamRcpWindow.Name = "btnOpenFlyCamRcpWindow";
            this.btnOpenFlyCamRcpWindow.Size = new System.Drawing.Size(120, 31);
            this.btnOpenFlyCamRcpWindow.TabIndex = 2;
            this.btnOpenFlyCamRcpWindow.Text = "飞拍界面";
            this.btnOpenFlyCamRcpWindow.UseVisualStyleBackColor = false;
            // 
            // btnOpenLightCtrlWindow
            // 
            this.btnOpenLightCtrlWindow.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnOpenLightCtrlWindow.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnOpenLightCtrlWindow.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOpenLightCtrlWindow.Location = new System.Drawing.Point(588, 6);
            this.btnOpenLightCtrlWindow.Margin = new System.Windows.Forms.Padding(12, 6, 12, 3);
            this.btnOpenLightCtrlWindow.Name = "btnOpenLightCtrlWindow";
            this.btnOpenLightCtrlWindow.Size = new System.Drawing.Size(120, 31);
            this.btnOpenLightCtrlWindow.TabIndex = 3;
            this.btnOpenLightCtrlWindow.Text = "控制灯光";
            this.btnOpenLightCtrlWindow.UseVisualStyleBackColor = false;
            // 
            // btnSwitchToEmptyTray
            // 
            this.btnSwitchToEmptyTray.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.btnSwitchToEmptyTray.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnSwitchToEmptyTray.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSwitchToEmptyTray.Location = new System.Drawing.Point(156, 6);
            this.btnSwitchToEmptyTray.Margin = new System.Windows.Forms.Padding(12, 6, 12, 3);
            this.btnSwitchToEmptyTray.Name = "btnSwitchToEmptyTray";
            this.btnSwitchToEmptyTray.Size = new System.Drawing.Size(120, 31);
            this.btnSwitchToEmptyTray.TabIndex = 0;
            this.btnSwitchToEmptyTray.Text = "空盤設定";
            this.btnSwitchToEmptyTray.UseVisualStyleBackColor = false;
            // 
            // btnSwitchToChipTemplate
            // 
            this.btnSwitchToChipTemplate.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnSwitchToChipTemplate.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnSwitchToChipTemplate.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSwitchToChipTemplate.Location = new System.Drawing.Point(300, 6);
            this.btnSwitchToChipTemplate.Margin = new System.Windows.Forms.Padding(12, 6, 12, 3);
            this.btnSwitchToChipTemplate.Name = "btnSwitchToChipTemplate";
            this.btnSwitchToChipTemplate.Size = new System.Drawing.Size(120, 31);
            this.btnSwitchToChipTemplate.TabIndex = 1;
            this.btnSwitchToChipTemplate.Text = "晶粒模板";
            this.btnSwitchToChipTemplate.UseVisualStyleBackColor = false;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.LightSteelBlue;
            this.tableLayoutPanel1.ColumnCount = 6;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.label0, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnOpenLightCtrlWindow, 4, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnWriteCoordsToPlc, 5, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnOpenFlyCamRcpWindow, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnSwitchToChipTemplate, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnSwitchToEmptyTray, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(12, 6, 12, 3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1288, 42);
            this.tableLayoutPanel1.TabIndex = 56;
            // 
            // label0
            // 
            this.label0.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label0.Location = new System.Drawing.Point(12, 6);
            this.label0.Margin = new System.Windows.Forms.Padding(12, 6, 12, 3);
            this.label0.Name = "label0";
            this.label0.Size = new System.Drawing.Size(120, 31);
            this.label0.TabIndex = 55;
            this.label0.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.gvCalibPointsDataGridView1);
            this.panel2.Controls.Add(this.panelFocusZ);
            this.panel2.Controls.Add(this.btnSaveImage);
            this.panel2.Controls.Add(this.btnGrabImage);
            this.panel2.Controls.Add(this.rdoCarrier2);
            this.panel2.Controls.Add(this.rdoCarrier1);
            this.panel2.Controls.Add(this.btnLoadImage);
            this.panel2.Controls.Add(this.btnOpenTemplateMatchWindow);
            this.panel2.Controls.Add(this.btnPickGoldenRegion);
            this.panel2.Controls.Add(this.btnCreateCellRegions);
            this.panel2.Controls.Add(this.btnOpenEmptyTrayWindow);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(0, 42);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1288, 138);
            this.panel2.TabIndex = 58;
            // 
            // gvCalibPointsDataGridView1
            // 
            this.gvCalibPointsDataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gvCalibPointsDataGridView1.Location = new System.Drawing.Point(720, 13);
            this.gvCalibPointsDataGridView1.Name = "gvCalibPointsDataGridView1";
            this.gvCalibPointsDataGridView1.SelectedIndex = 0;
            this.gvCalibPointsDataGridView1.Size = new System.Drawing.Size(550, 108);
            this.gvCalibPointsDataGridView1.TabIndex = 70;
            // 
            // panelFocusZ
            // 
            this.panelFocusZ.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelFocusZ.Controls.Add(this.lblFocusMotorZ);
            this.panelFocusZ.Controls.Add(this.label1);
            this.panelFocusZ.Controls.Add(this.btnFocusMotorGo);
            this.panelFocusZ.Controls.Add(this.btnFocusMotorSettings);
            this.panelFocusZ.Location = new System.Drawing.Point(444, 13);
            this.panelFocusZ.Name = "panelFocusZ";
            this.panelFocusZ.Size = new System.Drawing.Size(251, 91);
            this.panelFocusZ.TabIndex = 68;
            // 
            // lblFocusMotorZ
            // 
            this.lblFocusMotorZ.BackColor = System.Drawing.Color.Black;
            this.lblFocusMotorZ.Font = new System.Drawing.Font("Microsoft YaHei UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFocusMotorZ.ForeColor = System.Drawing.Color.Lime;
            this.lblFocusMotorZ.Location = new System.Drawing.Point(20, 46);
            this.lblFocusMotorZ.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblFocusMotorZ.Name = "lblFocusMotorZ";
            this.lblFocusMotorZ.Size = new System.Drawing.Size(120, 31);
            this.lblFocusMotorZ.TabIndex = 47;
            this.lblFocusMotorZ.Text = "999.000";
            this.lblFocusMotorZ.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(16, 12);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(124, 20);
            this.label1.TabIndex = 3;
            this.label1.Text = "相機對焦 Z (mm)";
            // 
            // btnFocusMotorGo
            // 
            this.btnFocusMotorGo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.btnFocusMotorGo.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnFocusMotorGo.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnFocusMotorGo.Location = new System.Drawing.Point(167, 46);
            this.btnFocusMotorGo.Margin = new System.Windows.Forms.Padding(4);
            this.btnFocusMotorGo.Name = "btnFocusMotorGo";
            this.btnFocusMotorGo.Size = new System.Drawing.Size(58, 31);
            this.btnFocusMotorGo.TabIndex = 16;
            this.btnFocusMotorGo.Text = "Go";
            this.btnFocusMotorGo.UseVisualStyleBackColor = false;
            // 
            // btnFocusMotorSettings
            // 
            this.btnFocusMotorSettings.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnFocusMotorSettings.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnFocusMotorSettings.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnFocusMotorSettings.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnFocusMotorSettings.ImageIndex = 0;
            this.btnFocusMotorSettings.ImageList = this.imageList1;
            this.btnFocusMotorSettings.Location = new System.Drawing.Point(167, 9);
            this.btnFocusMotorSettings.Margin = new System.Windows.Forms.Padding(4);
            this.btnFocusMotorSettings.Name = "btnFocusMotorSettings";
            this.btnFocusMotorSettings.Size = new System.Drawing.Size(58, 27);
            this.btnFocusMotorSettings.TabIndex = 15;
            this.btnFocusMotorSettings.UseVisualStyleBackColor = false;
            // 
            // btnSaveImage
            // 
            this.btnSaveImage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnSaveImage.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnSaveImage.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSaveImage.Location = new System.Drawing.Point(156, 90);
            this.btnSaveImage.Margin = new System.Windows.Forms.Padding(4);
            this.btnSaveImage.Name = "btnSaveImage";
            this.btnSaveImage.Size = new System.Drawing.Size(120, 31);
            this.btnSaveImage.TabIndex = 9;
            this.btnSaveImage.Text = "另存图片";
            this.btnSaveImage.UseVisualStyleBackColor = false;
            // 
            // btnGrabImage
            // 
            this.btnGrabImage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnGrabImage.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnGrabImage.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnGrabImage.Location = new System.Drawing.Point(156, 13);
            this.btnGrabImage.Margin = new System.Windows.Forms.Padding(4);
            this.btnGrabImage.Name = "btnGrabImage";
            this.btnGrabImage.Size = new System.Drawing.Size(120, 31);
            this.btnGrabImage.TabIndex = 7;
            this.btnGrabImage.Text = "取像";
            this.btnGrabImage.UseVisualStyleBackColor = false;
            // 
            // rdoCarrier2
            // 
            this.rdoCarrier2.Appearance = System.Windows.Forms.Appearance.Button;
            this.rdoCarrier2.FlatAppearance.CheckedBackColor = System.Drawing.Color.Lime;
            this.rdoCarrier2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.rdoCarrier2.Font = new System.Drawing.Font("微软雅黑", 16.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rdoCarrier2.ForeColor = System.Drawing.Color.DimGray;
            this.rdoCarrier2.Location = new System.Drawing.Point(13, 69);
            this.rdoCarrier2.Margin = new System.Windows.Forms.Padding(4);
            this.rdoCarrier2.Name = "rdoCarrier2";
            this.rdoCarrier2.Size = new System.Drawing.Size(118, 52);
            this.rdoCarrier2.TabIndex = 6;
            this.rdoCarrier2.Text = "載台2";
            this.rdoCarrier2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rdoCarrier2.UseVisualStyleBackColor = true;
            // 
            // rdoCarrier1
            // 
            this.rdoCarrier1.Appearance = System.Windows.Forms.Appearance.Button;
            this.rdoCarrier1.Checked = true;
            this.rdoCarrier1.FlatAppearance.CheckedBackColor = System.Drawing.Color.Lime;
            this.rdoCarrier1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.rdoCarrier1.Font = new System.Drawing.Font("微软雅黑", 16.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rdoCarrier1.Location = new System.Drawing.Point(13, 13);
            this.rdoCarrier1.Margin = new System.Windows.Forms.Padding(4);
            this.rdoCarrier1.Name = "rdoCarrier1";
            this.rdoCarrier1.Size = new System.Drawing.Size(118, 52);
            this.rdoCarrier1.TabIndex = 5;
            this.rdoCarrier1.TabStop = true;
            this.rdoCarrier1.Text = "載台1";
            this.rdoCarrier1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rdoCarrier1.UseVisualStyleBackColor = true;
            // 
            // btnLoadImage
            // 
            this.btnLoadImage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnLoadImage.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnLoadImage.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLoadImage.Location = new System.Drawing.Point(156, 52);
            this.btnLoadImage.Margin = new System.Windows.Forms.Padding(4);
            this.btnLoadImage.Name = "btnLoadImage";
            this.btnLoadImage.Size = new System.Drawing.Size(120, 31);
            this.btnLoadImage.TabIndex = 8;
            this.btnLoadImage.Text = "加载图片";
            this.btnLoadImage.UseVisualStyleBackColor = false;
            // 
            // btnOpenTemplateMatchWindow
            // 
            this.btnOpenTemplateMatchWindow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnOpenTemplateMatchWindow.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnOpenTemplateMatchWindow.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOpenTemplateMatchWindow.Location = new System.Drawing.Point(300, 52);
            this.btnOpenTemplateMatchWindow.Margin = new System.Windows.Forms.Padding(4);
            this.btnOpenTemplateMatchWindow.Name = "btnOpenTemplateMatchWindow";
            this.btnOpenTemplateMatchWindow.Size = new System.Drawing.Size(120, 31);
            this.btnOpenTemplateMatchWindow.TabIndex = 13;
            this.btnOpenTemplateMatchWindow.Text = "模板界面";
            this.btnOpenTemplateMatchWindow.UseVisualStyleBackColor = false;
            // 
            // btnPickGoldenRegion
            // 
            this.btnPickGoldenRegion.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnPickGoldenRegion.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnPickGoldenRegion.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPickGoldenRegion.Location = new System.Drawing.Point(300, 13);
            this.btnPickGoldenRegion.Margin = new System.Windows.Forms.Padding(4);
            this.btnPickGoldenRegion.Name = "btnPickGoldenRegion";
            this.btnPickGoldenRegion.Size = new System.Drawing.Size(120, 31);
            this.btnPickGoldenRegion.TabIndex = 12;
            this.btnPickGoldenRegion.Text = "框選區域";
            this.btnPickGoldenRegion.UseVisualStyleBackColor = false;
            // 
            // btnCreateCellRegions
            // 
            this.btnCreateCellRegions.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnCreateCellRegions.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnCreateCellRegions.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCreateCellRegions.Location = new System.Drawing.Point(300, 52);
            this.btnCreateCellRegions.Margin = new System.Windows.Forms.Padding(4);
            this.btnCreateCellRegions.Name = "btnCreateCellRegions";
            this.btnCreateCellRegions.Size = new System.Drawing.Size(120, 31);
            this.btnCreateCellRegions.TabIndex = 11;
            this.btnCreateCellRegions.Text = "生成陣列";
            this.btnCreateCellRegions.UseVisualStyleBackColor = false;
            // 
            // btnOpenEmptyTrayWindow
            // 
            this.btnOpenEmptyTrayWindow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnOpenEmptyTrayWindow.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnOpenEmptyTrayWindow.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOpenEmptyTrayWindow.Location = new System.Drawing.Point(300, 13);
            this.btnOpenEmptyTrayWindow.Margin = new System.Windows.Forms.Padding(18, 6, 18, 6);
            this.btnOpenEmptyTrayWindow.Name = "btnOpenEmptyTrayWindow";
            this.btnOpenEmptyTrayWindow.Size = new System.Drawing.Size(120, 31);
            this.btnOpenEmptyTrayWindow.TabIndex = 10;
            this.btnOpenEmptyTrayWindow.Text = "空盤檢測";
            this.btnOpenEmptyTrayWindow.UseVisualStyleBackColor = false;
            // 
            // GvRecipeBtnsPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "GvRecipeBtnsPanel";
            this.Size = new System.Drawing.Size(1288, 180);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panelFocusZ.ResumeLayout(false);
            this.panelFocusZ.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panelFocusZ;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label0;
        public System.Windows.Forms.Button btnWriteCoordsToPlc;
        public System.Windows.Forms.Button btnOpenFlyCamRcpWindow;
        public System.Windows.Forms.Button btnOpenLightCtrlWindow;
        public System.Windows.Forms.Button btnSwitchToEmptyTray;
        public System.Windows.Forms.Button btnSwitchToChipTemplate;
        public System.Windows.Forms.Label lblFocusMotorZ;
        public System.Windows.Forms.Button btnFocusMotorGo;
        public System.Windows.Forms.Button btnFocusMotorSettings;
        public System.Windows.Forms.Button btnSaveImage;
        public System.Windows.Forms.Button btnGrabImage;
        public System.Windows.Forms.RadioButton rdoCarrier2;
        public System.Windows.Forms.RadioButton rdoCarrier1;
        public System.Windows.Forms.Button btnLoadImage;
        public System.Windows.Forms.Button btnCreateCellRegions;
        public System.Windows.Forms.Button btnOpenEmptyTrayWindow;
        public GvCalibPointsDataGridView gvCalibPointsDataGridView1;
        public System.Windows.Forms.Button btnOpenTemplateMatchWindow;
        public System.Windows.Forms.Button btnPickGoldenRegion;
    }
}
