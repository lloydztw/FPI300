namespace LaserAlignDX.FormSpace.FPI30Form
{
    partial class FormCalibration
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tabCtrlRight = new System.Windows.Forms.TabControl();
            this.tabPage01 = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel0 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.rdoCarrier2 = new System.Windows.Forms.RadioButton();
            this.rdoCarrier1 = new System.Windows.Forms.RadioButton();
            this.panel2 = new System.Windows.Forms.Panel();
            this.rdoSucker2 = new System.Windows.Forms.RadioButton();
            this.rdoSucker1 = new System.Windows.Forms.RadioButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnRunCalib = new System.Windows.Forms.Button();
            this.btnBrowseImage = new System.Windows.Forms.Button();
            this.btnGrabImage = new System.Windows.Forms.Button();
            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            this.btnRefRB = new System.Windows.Forms.Button();
            this.btnRefLB = new System.Windows.Forms.Button();
            this.btnRefRT = new System.Windows.Forms.Button();
            this.btnRefLT = new System.Windows.Forms.Button();
            this.rtbVerifyResult = new System.Windows.Forms.RichTextBox();
            this.btnVerify = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.tabCtrlLeft = new System.Windows.Forms.TabControl();
            this.tabPage00 = new System.Windows.Forms.TabPage();
            this.panel3 = new System.Windows.Forms.Panel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.jezTransImageViewPanel1 = new LaserAlignDX.Mvc.Gui.JezTransImageViewPanel();
            this.calibPointsTable = new LaserAlignDX.FormSpace.FPI30Form.CalibrationUI();
            this.tabCtrlRight.SuspendLayout();
            this.tabPage01.SuspendLayout();
            this.tableLayoutPanel0.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tabCtrlLeft.SuspendLayout();
            this.tabPage00.SuspendLayout();
            this.panel3.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabCtrlRight
            // 
            this.tabCtrlRight.Controls.Add(this.tabPage01);
            this.tabCtrlRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.tabCtrlRight.Location = new System.Drawing.Point(526, 0);
            this.tabCtrlRight.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabCtrlRight.Name = "tabCtrlRight";
            this.tabCtrlRight.SelectedIndex = 0;
            this.tabCtrlRight.Size = new System.Drawing.Size(906, 952);
            this.tabCtrlRight.TabIndex = 0;
            // 
            // tabPage01
            // 
            this.tabPage01.Controls.Add(this.tableLayoutPanel0);
            this.tabPage01.Location = new System.Drawing.Point(4, 25);
            this.tabPage01.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPage01.Name = "tabPage01";
            this.tabPage01.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPage01.Size = new System.Drawing.Size(898, 923);
            this.tabPage01.TabIndex = 0;
            this.tabPage01.Text = "校正设定";
            this.tabPage01.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel0
            // 
            this.tableLayoutPanel0.ColumnCount = 1;
            this.tableLayoutPanel0.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel0.Controls.Add(this.panel3, 0, 3);
            this.tableLayoutPanel0.Controls.Add(this.tableLayoutPanel1, 0, 0);
            this.tableLayoutPanel0.Controls.Add(this.groupBox2, 0, 1);
            this.tableLayoutPanel0.Controls.Add(this.groupBox1, 0, 2);
            this.tableLayoutPanel0.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel0.Location = new System.Drawing.Point(4, 3);
            this.tableLayoutPanel0.Name = "tableLayoutPanel0";
            this.tableLayoutPanel0.RowCount = 4;
            this.tableLayoutPanel0.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tableLayoutPanel0.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tableLayoutPanel0.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel0.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tableLayoutPanel0.Size = new System.Drawing.Size(890, 917);
            this.tableLayoutPanel0.TabIndex = 31;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel2, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(884, 114);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.rdoCarrier2);
            this.panel1.Controls.Add(this.rdoCarrier1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(86, 8, 86, 8);
            this.panel1.Size = new System.Drawing.Size(436, 108);
            this.panel1.TabIndex = 29;
            // 
            // rdoCarrier2
            // 
            this.rdoCarrier2.AutoSize = true;
            this.rdoCarrier2.Dock = System.Windows.Forms.DockStyle.Right;
            this.rdoCarrier2.Font = new System.Drawing.Font("微軟正黑體", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.rdoCarrier2.ForeColor = System.Drawing.Color.DimGray;
            this.rdoCarrier2.Location = new System.Drawing.Point(232, 8);
            this.rdoCarrier2.Name = "rdoCarrier2";
            this.rdoCarrier2.Size = new System.Drawing.Size(116, 90);
            this.rdoCarrier2.TabIndex = 8;
            this.rdoCarrier2.Text = "載台 2";
            this.rdoCarrier2.UseVisualStyleBackColor = true;
            // 
            // rdoCarrier1
            // 
            this.rdoCarrier1.AutoSize = true;
            this.rdoCarrier1.Checked = true;
            this.rdoCarrier1.Dock = System.Windows.Forms.DockStyle.Left;
            this.rdoCarrier1.Font = new System.Drawing.Font("微軟正黑體", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.rdoCarrier1.Location = new System.Drawing.Point(86, 8);
            this.rdoCarrier1.Name = "rdoCarrier1";
            this.rdoCarrier1.Size = new System.Drawing.Size(116, 90);
            this.rdoCarrier1.TabIndex = 7;
            this.rdoCarrier1.TabStop = true;
            this.rdoCarrier1.Text = "載台 1";
            this.rdoCarrier1.UseVisualStyleBackColor = true;
            // 
            // panel2
            // 
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.rdoSucker2);
            this.panel2.Controls.Add(this.rdoSucker1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(445, 3);
            this.panel2.Name = "panel2";
            this.panel2.Padding = new System.Windows.Forms.Padding(56, 8, 56, 8);
            this.panel2.Size = new System.Drawing.Size(436, 108);
            this.panel2.TabIndex = 30;
            // 
            // rdoSucker2
            // 
            this.rdoSucker2.AutoSize = true;
            this.rdoSucker2.Dock = System.Windows.Forms.DockStyle.Right;
            this.rdoSucker2.Font = new System.Drawing.Font("微軟正黑體", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.rdoSucker2.ForeColor = System.Drawing.Color.DimGray;
            this.rdoSucker2.Location = new System.Drawing.Point(234, 8);
            this.rdoSucker2.Name = "rdoSucker2";
            this.rdoSucker2.Size = new System.Drawing.Size(144, 90);
            this.rdoSucker2.TabIndex = 10;
            this.rdoSucker2.Text = "吸嘴排 2";
            this.rdoSucker2.UseVisualStyleBackColor = true;
            // 
            // rdoSucker1
            // 
            this.rdoSucker1.AutoSize = true;
            this.rdoSucker1.Checked = true;
            this.rdoSucker1.Dock = System.Windows.Forms.DockStyle.Left;
            this.rdoSucker1.Font = new System.Drawing.Font("微軟正黑體", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.rdoSucker1.Location = new System.Drawing.Point(56, 8);
            this.rdoSucker1.Name = "rdoSucker1";
            this.rdoSucker1.Size = new System.Drawing.Size(144, 90);
            this.rdoSucker1.TabIndex = 9;
            this.rdoSucker1.TabStop = true;
            this.rdoSucker1.Text = "吸嘴排 1";
            this.rdoSucker1.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnRunCalib);
            this.groupBox2.Controls.Add(this.btnBrowseImage);
            this.groupBox2.Controls.Add(this.btnGrabImage);
            this.groupBox2.Controls.Add(this.propertyGrid1);
            this.groupBox2.Controls.Add(this.btnRefRB);
            this.groupBox2.Controls.Add(this.btnRefLB);
            this.groupBox2.Controls.Add(this.btnRefRT);
            this.groupBox2.Controls.Add(this.btnRefLT);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(4, 123);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox2.Size = new System.Drawing.Size(882, 467);
            this.groupBox2.TabIndex = 28;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "像測操作";
            // 
            // btnRunCalib
            // 
            this.btnRunCalib.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRunCalib.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.btnRunCalib.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnRunCalib.Location = new System.Drawing.Point(24, 390);
            this.btnRunCalib.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnRunCalib.Name = "btnRunCalib";
            this.btnRunCalib.Size = new System.Drawing.Size(304, 52);
            this.btnRunCalib.TabIndex = 25;
            this.btnRunCalib.Text = "重新校正";
            this.btnRunCalib.UseVisualStyleBackColor = false;
            // 
            // btnBrowseImage
            // 
            this.btnBrowseImage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnBrowseImage.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnBrowseImage.Location = new System.Drawing.Point(188, 25);
            this.btnBrowseImage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnBrowseImage.Name = "btnBrowseImage";
            this.btnBrowseImage.Size = new System.Drawing.Size(148, 52);
            this.btnBrowseImage.TabIndex = 22;
            this.btnBrowseImage.Text = "载入图像";
            this.btnBrowseImage.UseVisualStyleBackColor = false;
            // 
            // btnGrabImage
            // 
            this.btnGrabImage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnGrabImage.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnGrabImage.Location = new System.Drawing.Point(32, 25);
            this.btnGrabImage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnGrabImage.Name = "btnGrabImage";
            this.btnGrabImage.Size = new System.Drawing.Size(148, 52);
            this.btnGrabImage.TabIndex = 21;
            this.btnGrabImage.Text = "取像";
            this.btnGrabImage.UseVisualStyleBackColor = false;
            // 
            // propertyGrid1
            // 
            this.propertyGrid1.Dock = System.Windows.Forms.DockStyle.Right;
            this.propertyGrid1.Location = new System.Drawing.Point(357, 21);
            this.propertyGrid1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.Size = new System.Drawing.Size(521, 443);
            this.propertyGrid1.TabIndex = 20;
            // 
            // btnRefRB
            // 
            this.btnRefRB.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnRefRB.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnRefRB.Location = new System.Drawing.Point(188, 170);
            this.btnRefRB.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnRefRB.Name = "btnRefRB";
            this.btnRefRB.Size = new System.Drawing.Size(148, 52);
            this.btnRefRB.TabIndex = 19;
            this.btnRefRB.Text = "点4(载台右下)";
            this.btnRefRB.UseVisualStyleBackColor = false;
            // 
            // btnRefLB
            // 
            this.btnRefLB.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnRefLB.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnRefLB.Location = new System.Drawing.Point(32, 170);
            this.btnRefLB.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnRefLB.Name = "btnRefLB";
            this.btnRefLB.Size = new System.Drawing.Size(148, 52);
            this.btnRefLB.TabIndex = 18;
            this.btnRefLB.Text = "点3(载台左下)";
            this.btnRefLB.UseVisualStyleBackColor = false;
            // 
            // btnRefRT
            // 
            this.btnRefRT.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnRefRT.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnRefRT.Location = new System.Drawing.Point(188, 112);
            this.btnRefRT.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnRefRT.Name = "btnRefRT";
            this.btnRefRT.Size = new System.Drawing.Size(148, 52);
            this.btnRefRT.TabIndex = 17;
            this.btnRefRT.Text = "点2(载台右上)";
            this.btnRefRT.UseVisualStyleBackColor = false;
            // 
            // btnRefLT
            // 
            this.btnRefLT.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnRefLT.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnRefLT.Location = new System.Drawing.Point(32, 112);
            this.btnRefLT.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnRefLT.Name = "btnRefLT";
            this.btnRefLT.Size = new System.Drawing.Size(148, 52);
            this.btnRefLT.TabIndex = 16;
            this.btnRefLT.Text = "点1(载台左上)";
            this.btnRefLT.UseVisualStyleBackColor = false;
            // 
            // rtbVerifyResult
            // 
            this.rtbVerifyResult.Location = new System.Drawing.Point(189, 19);
            this.rtbVerifyResult.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.rtbVerifyResult.Name = "rtbVerifyResult";
            this.rtbVerifyResult.ReadOnly = true;
            this.rtbVerifyResult.Size = new System.Drawing.Size(290, 52);
            this.rtbVerifyResult.TabIndex = 27;
            this.rtbVerifyResult.Text = "";
            // 
            // btnVerify
            // 
            this.btnVerify.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnVerify.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnVerify.Location = new System.Drawing.Point(33, 19);
            this.btnVerify.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnVerify.Name = "btnVerify";
            this.btnVerify.Size = new System.Drawing.Size(148, 52);
            this.btnVerify.TabIndex = 26;
            this.btnVerify.Text = "框选验证";
            this.btnVerify.UseVisualStyleBackColor = false;
            // 
            // btnExit
            // 
            this.btnExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.btnExit.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnExit.Location = new System.Drawing.Point(722, 47);
            this.btnExit.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(148, 52);
            this.btnExit.TabIndex = 24;
            this.btnExit.Text = "退出";
            this.btnExit.UseVisualStyleBackColor = false;
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnSave.Location = new System.Drawing.Point(566, 47);
            this.btnSave.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(148, 52);
            this.btnSave.TabIndex = 23;
            this.btnSave.Text = "保存资料";
            this.btnSave.UseVisualStyleBackColor = false;
            // 
            // tabCtrlLeft
            // 
            this.tabCtrlLeft.Controls.Add(this.tabPage00);
            this.tabCtrlLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabCtrlLeft.Location = new System.Drawing.Point(0, 0);
            this.tabCtrlLeft.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabCtrlLeft.Name = "tabCtrlLeft";
            this.tabCtrlLeft.SelectedIndex = 0;
            this.tabCtrlLeft.Size = new System.Drawing.Size(526, 952);
            this.tabCtrlLeft.TabIndex = 1;
            // 
            // tabPage00
            // 
            this.tabPage00.Controls.Add(this.jezTransImageViewPanel1);
            this.tabPage00.Location = new System.Drawing.Point(4, 25);
            this.tabPage00.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPage00.Name = "tabPage00";
            this.tabPage00.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPage00.Size = new System.Drawing.Size(518, 923);
            this.tabPage00.TabIndex = 0;
            this.tabPage00.Text = "显示图像";
            this.tabPage00.UseVisualStyleBackColor = true;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.btnVerify);
            this.panel3.Controls.Add(this.btnExit);
            this.panel3.Controls.Add(this.rtbVerifyResult);
            this.panel3.Controls.Add(this.btnSave);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(3, 799);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(884, 115);
            this.panel3.TabIndex = 32;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.calibPointsTable);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(4, 596);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(10, 5, 10, 5);
            this.groupBox1.Size = new System.Drawing.Size(882, 197);
            this.groupBox1.TabIndex = 29;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "校正點";
            // 
            // jezTransImageViewPanel1
            // 
            this.jezTransImageViewPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.jezTransImageViewPanel1.Location = new System.Drawing.Point(4, 3);
            this.jezTransImageViewPanel1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.jezTransImageViewPanel1.Name = "jezTransImageViewPanel1";
            this.jezTransImageViewPanel1.Size = new System.Drawing.Size(510, 917);
            this.jezTransImageViewPanel1.TabIndex = 0;
            // 
            // calibPointsTable
            // 
            this.calibPointsTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.calibPointsTable.Location = new System.Drawing.Point(10, 23);
            this.calibPointsTable.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.calibPointsTable.Name = "calibPointsTable";
            this.calibPointsTable.Size = new System.Drawing.Size(862, 169);
            this.calibPointsTable.TabIndex = 2;
            // 
            // FormCalibration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1432, 952);
            this.Controls.Add(this.tabCtrlLeft);
            this.Controls.Add(this.tabCtrlRight);
            this.Font = new System.Drawing.Font("新細明體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "FormCalibration";
            this.Text = "Calibration";
            this.tabCtrlRight.ResumeLayout(false);
            this.tabPage01.ResumeLayout(false);
            this.tableLayoutPanel0.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.tabCtrlLeft.ResumeLayout(false);
            this.tabPage00.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabCtrlRight;
        private System.Windows.Forms.TabPage tabPage01;
        private System.Windows.Forms.TabControl tabCtrlLeft;
        private System.Windows.Forms.TabPage tabPage00;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.RichTextBox rtbVerifyResult;
        private System.Windows.Forms.Button btnVerify;
        private global::LaserAlignDX.Mvc.Gui.JezTransImageViewPanel jezTransImageViewPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnRunCalib;
        private System.Windows.Forms.Button btnBrowseImage;
        private System.Windows.Forms.Button btnGrabImage;
        private System.Windows.Forms.PropertyGrid propertyGrid1;
        private System.Windows.Forms.Button btnRefRB;
        private System.Windows.Forms.Button btnRefLB;
        private System.Windows.Forms.Button btnRefRT;
        private System.Windows.Forms.Button btnRefLT;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.RadioButton rdoSucker2;
        private System.Windows.Forms.RadioButton rdoSucker1;
        private System.Windows.Forms.RadioButton rdoCarrier2;
        private System.Windows.Forms.RadioButton rdoCarrier1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel0;
        private CalibrationUI calibPointsTable;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}