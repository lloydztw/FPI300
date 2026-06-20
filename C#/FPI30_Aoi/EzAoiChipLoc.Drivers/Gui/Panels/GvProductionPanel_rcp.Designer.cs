namespace EzEmptyTrayInspector.Gui.Panels.Cbo
{
    partial class GvProductionPanel
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

        #region 元件設計工具產生的程式碼

        /// <summary> 
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改這個方法的內容。
        ///
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GvProductionPanel));
            this.panel2 = new System.Windows.Forms.Panel();
            this.cboRecipeNames = new System.Windows.Forms.ComboBox();
            this.panel4 = new System.Windows.Forms.Panel();
            this.gwLogPanel1 = new EzAoiChipLocQC.Gui.Panels.GwLogPanel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.btnTryRun = new System.Windows.Forms.Button();
            this.btnProductionRun = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.txtSerialNo = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tableLayoutPanel0 = new System.Windows.Forms.TableLayoutPanel();
            this.lblDeviceInfo = new System.Windows.Forms.Label();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.btnLiveMode = new System.Windows.Forms.Button();
            this.btnSnapshot = new System.Windows.Forms.Button();
            this.btnMatch = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.btnOpenCamera = new System.Windows.Forms.Button();
            this.cboAvailableCameras = new System.Windows.Forms.ComboBox();
            this.gwPanePropsViewer1 = new LeTian.JxProps.Gui.GwPanePropsViewer();
            this.panel4.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tableLayoutPanel0.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.panel2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Location = new System.Drawing.Point(0, 87);
            this.panel2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(466, 153);
            this.panel2.TabIndex = 300;
            // 
            // cboRecipeNames
            // 
            this.cboRecipeNames.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cboRecipeNames.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboRecipeNames.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboRecipeNames.FormattingEnabled = true;
            this.cboRecipeNames.Location = new System.Drawing.Point(1039, 103);
            this.cboRecipeNames.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cboRecipeNames.Name = "cboRecipeNames";
            this.cboRecipeNames.Size = new System.Drawing.Size(92, 31);
            this.cboRecipeNames.TabIndex = 306;
            // 
            // panel4
            // 
            this.panel4.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panel4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel4.Controls.Add(this.gwLogPanel1);
            this.panel4.Location = new System.Drawing.Point(0, 319);
            this.panel4.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panel4.Name = "panel4";
            this.panel4.Padding = new System.Windows.Forms.Padding(12, 0, 12, 12);
            this.panel4.Size = new System.Drawing.Size(466, 404);
            this.panel4.TabIndex = 302;
            // 
            // gwLogPanel1
            // 
            this.gwLogPanel1.BackColor = System.Drawing.Color.Transparent;
            this.gwLogPanel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.gwLogPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gwLogPanel1.Location = new System.Drawing.Point(12, 0);
            this.gwLogPanel1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.gwLogPanel1.Name = "gwLogPanel1";
            this.gwLogPanel1.Padding = new System.Windows.Forms.Padding(5, 8, 5, 8);
            this.gwLogPanel1.Size = new System.Drawing.Size(440, 390);
            this.gwLogPanel1.TabIndex = 0;
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.panel3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel3.Controls.Add(this.btnTryRun);
            this.panel3.Controls.Add(this.btnProductionRun);
            this.panel3.Controls.Add(this.btnStop);
            this.panel3.ForeColor = System.Drawing.Color.Black;
            this.panel3.Location = new System.Drawing.Point(0, 240);
            this.panel3.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(466, 80);
            this.panel3.TabIndex = 146;
            // 
            // btnTryRun
            // 
            this.btnTryRun.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnTryRun.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnTryRun.Font = new System.Drawing.Font("微軟正黑體", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnTryRun.Location = new System.Drawing.Point(36, 15);
            this.btnTryRun.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnTryRun.Name = "btnTryRun";
            this.btnTryRun.Size = new System.Drawing.Size(117, 50);
            this.btnTryRun.TabIndex = 305;
            this.btnTryRun.Text = "Try Run";
            this.btnTryRun.UseVisualStyleBackColor = false;
            // 
            // btnProductionRun
            // 
            this.btnProductionRun.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.btnProductionRun.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnProductionRun.Font = new System.Drawing.Font("微軟正黑體", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnProductionRun.Location = new System.Drawing.Point(171, 15);
            this.btnProductionRun.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnProductionRun.Name = "btnProductionRun";
            this.btnProductionRun.Size = new System.Drawing.Size(117, 50);
            this.btnProductionRun.TabIndex = 0;
            this.btnProductionRun.Text = "Start";
            this.btnProductionRun.UseVisualStyleBackColor = false;
            this.btnProductionRun.Visible = false;
            // 
            // btnStop
            // 
            this.btnStop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.btnStop.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnStop.Font = new System.Drawing.Font("微軟正黑體", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnStop.Location = new System.Drawing.Point(307, 15);
            this.btnStop.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(117, 50);
            this.btnStop.TabIndex = 1;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = false;
            this.btnStop.Visible = false;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.panel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.txtSerialNo);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(466, 83);
            this.panel1.TabIndex = 147;
            // 
            // txtSerialNo
            // 
            this.txtSerialNo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSerialNo.BackColor = System.Drawing.Color.WhiteSmoke;
            this.txtSerialNo.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSerialNo.Location = new System.Drawing.Point(153, 26);
            this.txtSerialNo.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtSerialNo.Name = "txtSerialNo";
            this.txtSerialNo.ReadOnly = true;
            this.txtSerialNo.Size = new System.Drawing.Size(281, 31);
            this.txtSerialNo.TabIndex = 302;
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("微軟正黑體", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.MediumBlue;
            this.label2.Location = new System.Drawing.Point(21, 27);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(153, 28);
            this.label2.TabIndex = 307;
            this.label2.Text = "Recipe (參數)";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tableLayoutPanel0
            // 
            this.tableLayoutPanel0.ColumnCount = 1;
            this.tableLayoutPanel0.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel0.Controls.Add(this.lblDeviceInfo, 0, 1);
            this.tableLayoutPanel0.Controls.Add(this.tableLayoutPanel2, 0, 4);
            this.tableLayoutPanel0.Controls.Add(this.tableLayoutPanel1, 0, 0);
            this.tableLayoutPanel0.Controls.Add(this.gwPanePropsViewer1, 0, 2);
            this.tableLayoutPanel0.Location = new System.Drawing.Point(556, 27);
            this.tableLayoutPanel0.Name = "tableLayoutPanel0";
            this.tableLayoutPanel0.RowCount = 6;
            this.tableLayoutPanel0.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel0.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel0.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel0.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8F));
            this.tableLayoutPanel0.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel0.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8F));
            this.tableLayoutPanel0.Size = new System.Drawing.Size(480, 683);
            this.tableLayoutPanel0.TabIndex = 303;
            // 
            // lblDeviceInfo
            // 
            this.lblDeviceInfo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.lblDeviceInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDeviceInfo.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblDeviceInfo.ForeColor = System.Drawing.Color.Black;
            this.lblDeviceInfo.Location = new System.Drawing.Point(3, 42);
            this.lblDeviceInfo.Name = "lblDeviceInfo";
            this.lblDeviceInfo.Padding = new System.Windows.Forms.Padding(9, 0, 0, 0);
            this.lblDeviceInfo.Size = new System.Drawing.Size(474, 100);
            this.lblDeviceInfo.TabIndex = 20;
            this.lblDeviceInfo.Text = "Info";
            this.lblDeviceInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.btnBrowse, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.button1, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.btnLiveMode, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.btnSnapshot, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnMatch, 1, 2);
            this.tableLayoutPanel2.Controls.Add(this.btnClear, 0, 2);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 553);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 3;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 34F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(474, 119);
            this.tableLayoutPanel2.TabIndex = 18;
            // 
            // btnBrowse
            // 
            this.btnBrowse.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnBrowse.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnBrowse.Location = new System.Drawing.Point(3, 2);
            this.btnBrowse.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(231, 35);
            this.btnBrowse.TabIndex = 14;
            this.btnBrowse.TabStop = false;
            this.btnBrowse.Text = "Browse";
            this.btnBrowse.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.button1.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.button1.Location = new System.Drawing.Point(3, 41);
            this.button1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(231, 35);
            this.button1.TabIndex = 15;
            this.button1.TabStop = false;
            this.button1.Text = "Stop";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // btnLiveMode
            // 
            this.btnLiveMode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnLiveMode.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnLiveMode.Location = new System.Drawing.Point(240, 41);
            this.btnLiveMode.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnLiveMode.Name = "btnLiveMode";
            this.btnLiveMode.Size = new System.Drawing.Size(231, 35);
            this.btnLiveMode.TabIndex = 17;
            this.btnLiveMode.TabStop = false;
            this.btnLiveMode.Text = "Live";
            this.btnLiveMode.UseVisualStyleBackColor = true;
            // 
            // btnSnapshot
            // 
            this.btnSnapshot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSnapshot.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnSnapshot.Location = new System.Drawing.Point(240, 2);
            this.btnSnapshot.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnSnapshot.Name = "btnSnapshot";
            this.btnSnapshot.Size = new System.Drawing.Size(231, 35);
            this.btnSnapshot.TabIndex = 11;
            this.btnSnapshot.TabStop = false;
            this.btnSnapshot.Text = "Snapshot";
            this.btnSnapshot.UseVisualStyleBackColor = true;
            // 
            // btnMatch
            // 
            this.btnMatch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnMatch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnMatch.Font = new System.Drawing.Font("微軟正黑體", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnMatch.Location = new System.Drawing.Point(240, 80);
            this.btnMatch.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnMatch.Name = "btnMatch";
            this.btnMatch.Size = new System.Drawing.Size(231, 37);
            this.btnMatch.TabIndex = 18;
            this.btnMatch.Text = "Match";
            this.btnMatch.UseVisualStyleBackColor = false;
            // 
            // btnClear
            // 
            this.btnClear.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.btnClear.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnClear.Font = new System.Drawing.Font("微軟正黑體", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnClear.Location = new System.Drawing.Point(3, 80);
            this.btnClear.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(231, 37);
            this.btnClear.TabIndex = 19;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = false;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.btnOpenCamera, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.cboAvailableCameras, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(474, 36);
            this.tableLayoutPanel1.TabIndex = 4;
            // 
            // btnOpenCamera
            // 
            this.btnOpenCamera.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnOpenCamera.Font = new System.Drawing.Font("微軟正黑體", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnOpenCamera.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOpenCamera.ImageIndex = 0;
            this.btnOpenCamera.Location = new System.Drawing.Point(3, 2);
            this.btnOpenCamera.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnOpenCamera.Name = "btnOpenCamera";
            this.btnOpenCamera.Size = new System.Drawing.Size(231, 32);
            this.btnOpenCamera.TabIndex = 13;
            this.btnOpenCamera.TabStop = false;
            this.btnOpenCamera.Text = "相機 ...";
            this.btnOpenCamera.UseVisualStyleBackColor = true;
            // 
            // cboAvailableCameras
            // 
            this.cboAvailableCameras.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboAvailableCameras.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboAvailableCameras.Font = new System.Drawing.Font("Consolas", 10.90909F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboAvailableCameras.FormattingEnabled = true;
            this.cboAvailableCameras.Location = new System.Drawing.Point(240, 2);
            this.cboAvailableCameras.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cboAvailableCameras.Name = "cboAvailableCameras";
            this.cboAvailableCameras.Size = new System.Drawing.Size(231, 30);
            this.cboAvailableCameras.TabIndex = 12;
            this.cboAvailableCameras.Visible = false;
            // 
            // gwPanePropsViewer1
            // 
            this.gwPanePropsViewer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gwPanePropsViewer1.Editable = true;
            this.gwPanePropsViewer1.ImageList = null;
            this.gwPanePropsViewer1.Location = new System.Drawing.Point(3, 146);
            this.gwPanePropsViewer1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.gwPanePropsViewer1.Name = "gwPanePropsViewer1";
            this.gwPanePropsViewer1.Size = new System.Drawing.Size(474, 392);
            this.gwPanePropsViewer1.TabIndex = 19;
            // 
            // GvProductionPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.Controls.Add(this.cboRecipeNames);
            this.Controls.Add(this.tableLayoutPanel0);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panel3);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "GvProductionPanel";
            this.Size = new System.Drawing.Size(1152, 918);
            this.panel4.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.tableLayoutPanel0.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        public System.Windows.Forms.Button btnProductionRun;
        public System.Windows.Forms.Button btnStop;
        public System.Windows.Forms.TextBox txtSerialNo;
        private System.Windows.Forms.Panel panel4;
        public System.Windows.Forms.ComboBox cboRecipeNames;
        public System.Windows.Forms.Button btnTryRun;
        private System.Windows.Forms.Label label2;
        private EzAoiChipLocQC.Gui.Panels.GwLogPanel gwLogPanel1;
        internal System.Windows.Forms.TableLayoutPanel tableLayoutPanel0;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        internal System.Windows.Forms.Button btnBrowse;
        internal System.Windows.Forms.Button button1;
        internal System.Windows.Forms.Button btnLiveMode;
        internal System.Windows.Forms.Button btnSnapshot;
        public System.Windows.Forms.Button btnMatch;
        public System.Windows.Forms.Button btnClear;
        internal System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        public System.Windows.Forms.Button btnOpenCamera;
        internal System.Windows.Forms.ComboBox cboAvailableCameras;
        private LeTian.JxProps.Gui.GwPanePropsViewer gwPanePropsViewer1;
        public System.Windows.Forms.Label lblDeviceInfo;
    }
}
