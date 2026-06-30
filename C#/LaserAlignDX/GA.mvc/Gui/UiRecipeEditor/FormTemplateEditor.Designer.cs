namespace LaserAlignDX.Mvc.Gui
{
    partial class FormTemplateEditor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormTemplateEditor));
            this.tbLayoutSubM = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            this.rtbCodeContent = new System.Windows.Forms.RichTextBox();
            this.tbLayoutA = new System.Windows.Forms.TableLayoutPanel();
            this.DS1 = new JzDisplay.UISpace.DispUI();
            this.DS2 = new JzDisplay.UISpace.DispUI();
            this.DS3 = new JzDisplay.UISpace.DispUI();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnBuildMircoTrf = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.numSpanRatio = new System.Windows.Forms.NumericUpDown();
            this.labelA = new System.Windows.Forms.Label();
            this.numBorderIndent = new System.Windows.Forms.NumericUpDown();
            this.labelB = new System.Windows.Forms.Label();
            this.numBorderSize = new System.Windows.Forms.NumericUpDown();
            this.btnAutoLineBorders = new System.Windows.Forms.Button();
            this.tbLayoutSubL = new System.Windows.Forms.TableLayoutPanel();
            this.lblActiveCarrierID = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.btnTryQrCode = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radioButtonLn = new System.Windows.Forms.RadioButton();
            this.radioButtonG = new System.Windows.Forms.RadioButton();
            this.radioButtonQr = new System.Windows.Forms.RadioButton();
            this.btnPickGolden = new System.Windows.Forms.Button();
            this.tbLayoutB = new System.Windows.Forms.TableLayoutPanel();
            this.tbLayoutSubR = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnDefectDelete = new System.Windows.Forms.Button();
            this.btnDefectClear = new System.Windows.Forms.Button();
            this.btnDefectAdd = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnTrain = new System.Windows.Forms.Button();
            this.tbLayoutSubM.SuspendLayout();
            this.tbLayoutA.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numSpanRatio)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBorderIndent)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBorderSize)).BeginInit();
            this.tbLayoutSubL.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tbLayoutB.SuspendLayout();
            this.tbLayoutSubR.SuspendLayout();
            this.SuspendLayout();
            // 
            // tbLayoutSubM
            // 
            this.tbLayoutSubM.BackColor = System.Drawing.Color.LightSteelBlue;
            this.tbLayoutSubM.ColumnCount = 1;
            this.tbLayoutSubM.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tbLayoutSubM.Controls.Add(this.label1, 0, 0);
            this.tbLayoutSubM.Controls.Add(this.propertyGrid1, 0, 1);
            this.tbLayoutSubM.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbLayoutSubM.Location = new System.Drawing.Point(567, 4);
            this.tbLayoutSubM.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tbLayoutSubM.Name = "tbLayoutSubM";
            this.tbLayoutSubM.RowCount = 2;
            this.tbLayoutSubM.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.tbLayoutSubM.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tbLayoutSubM.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tbLayoutSubM.Size = new System.Drawing.Size(552, 569);
            this.tbLayoutSubM.TabIndex = 73;
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.LightSteelBlue;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Padding = new System.Windows.Forms.Padding(9, 0, 0, 0);
            this.label1.Size = new System.Drawing.Size(546, 46);
            this.label1.TabIndex = 8;
            this.label1.Text = "像測參數設定";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // propertyGrid1
            // 
            this.propertyGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid1.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.propertyGrid1.Location = new System.Drawing.Point(4, 51);
            this.propertyGrid1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.Size = new System.Drawing.Size(544, 513);
            this.propertyGrid1.TabIndex = 0;
            // 
            // rtbCodeContent
            // 
            this.rtbCodeContent.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rtbCodeContent.BackColor = System.Drawing.Color.Ivory;
            this.rtbCodeContent.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbCodeContent.Location = new System.Drawing.Point(25, 40);
            this.rtbCodeContent.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.rtbCodeContent.Name = "rtbCodeContent";
            this.rtbCodeContent.ReadOnly = true;
            this.rtbCodeContent.Size = new System.Drawing.Size(321, 99);
            this.rtbCodeContent.TabIndex = 52;
            this.rtbCodeContent.Text = "";
            // 
            // tbLayoutA
            // 
            this.tbLayoutA.BackColor = System.Drawing.SystemColors.Control;
            this.tbLayoutA.ColumnCount = 3;
            this.tbLayoutA.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tbLayoutA.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tbLayoutA.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tbLayoutA.Controls.Add(this.DS1, 0, 0);
            this.tbLayoutA.Controls.Add(this.DS2, 1, 0);
            this.tbLayoutA.Controls.Add(this.DS3, 2, 0);
            this.tbLayoutA.Dock = System.Windows.Forms.DockStyle.Top;
            this.tbLayoutA.Location = new System.Drawing.Point(0, 0);
            this.tbLayoutA.Margin = new System.Windows.Forms.Padding(0);
            this.tbLayoutA.Name = "tbLayoutA";
            this.tbLayoutA.Padding = new System.Windows.Forms.Padding(1);
            this.tbLayoutA.RowCount = 1;
            this.tbLayoutA.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tbLayoutA.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tbLayoutA.Size = new System.Drawing.Size(1420, 533);
            this.tbLayoutA.TabIndex = 1;
            // 
            // DS1
            // 
            this.DS1.Cursor = System.Windows.Forms.Cursors.Default;
            this.DS1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DS1.Location = new System.Drawing.Point(7, 6);
            this.DS1.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.DS1.Name = "DS1";
            this.DS1.Size = new System.Drawing.Size(460, 521);
            this.DS1.TabIndex = 0;
            // 
            // DS2
            // 
            this.DS2.Cursor = System.Windows.Forms.Cursors.Default;
            this.DS2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DS2.Location = new System.Drawing.Point(479, 6);
            this.DS2.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.DS2.Name = "DS2";
            this.DS2.Size = new System.Drawing.Size(460, 521);
            this.DS2.TabIndex = 1;
            // 
            // DS3
            // 
            this.DS3.Cursor = System.Windows.Forms.Cursors.Default;
            this.DS3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DS3.Location = new System.Drawing.Point(951, 6);
            this.DS3.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.DS3.Name = "DS3";
            this.DS3.Size = new System.Drawing.Size(462, 521);
            this.DS3.TabIndex = 2;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnBuildMircoTrf);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.numSpanRatio);
            this.groupBox2.Controls.Add(this.labelA);
            this.groupBox2.Controls.Add(this.numBorderIndent);
            this.groupBox2.Controls.Add(this.labelB);
            this.groupBox2.Controls.Add(this.numBorderSize);
            this.groupBox2.Controls.Add(this.btnAutoLineBorders);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox2.Location = new System.Drawing.Point(9, 220);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(9, 10, 9, 10);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox2.Size = new System.Drawing.Size(537, 167);
            this.groupBox2.TabIndex = 75;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "自動框選 邊線";
            // 
            // btnBuildMircoTrf
            // 
            this.btnBuildMircoTrf.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBuildMircoTrf.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.btnBuildMircoTrf.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnBuildMircoTrf.Location = new System.Drawing.Point(367, 92);
            this.btnBuildMircoTrf.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnBuildMircoTrf.Name = "btnBuildMircoTrf";
            this.btnBuildMircoTrf.Size = new System.Drawing.Size(130, 55);
            this.btnBuildMircoTrf.TabIndex = 74;
            this.btnBuildMircoTrf.Text = "精算尺寸";
            this.btnBuildMircoTrf.UseVisualStyleBackColor = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(56, 116);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(115, 24);
            this.label2.TabIndex = 73;
            this.label2.Text = "跨度比例 (%)";
            // 
            // numSpanRatio
            // 
            this.numSpanRatio.DecimalPlaces = 1;
            this.numSpanRatio.Location = new System.Drawing.Point(189, 113);
            this.numSpanRatio.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.numSpanRatio.Minimum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numSpanRatio.Name = "numSpanRatio";
            this.numSpanRatio.Size = new System.Drawing.Size(138, 30);
            this.numSpanRatio.TabIndex = 72;
            this.numSpanRatio.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numSpanRatio.Value = new decimal(new int[] {
            90,
            0,
            0,
            0});
            // 
            // labelA
            // 
            this.labelA.AutoSize = true;
            this.labelA.Location = new System.Drawing.Point(56, 37);
            this.labelA.Name = "labelA";
            this.labelA.Size = new System.Drawing.Size(112, 24);
            this.labelA.TabIndex = 71;
            this.labelA.Text = "內緣 (pixels)";
            // 
            // numBorderIndent
            // 
            this.numBorderIndent.Location = new System.Drawing.Point(189, 34);
            this.numBorderIndent.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.numBorderIndent.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numBorderIndent.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numBorderIndent.Name = "numBorderIndent";
            this.numBorderIndent.Size = new System.Drawing.Size(138, 30);
            this.numBorderIndent.TabIndex = 70;
            this.numBorderIndent.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numBorderIndent.Value = new decimal(new int[] {
            18,
            0,
            0,
            0});
            // 
            // labelB
            // 
            this.labelB.AutoSize = true;
            this.labelB.Location = new System.Drawing.Point(56, 77);
            this.labelB.Name = "labelB";
            this.labelB.Size = new System.Drawing.Size(112, 24);
            this.labelB.TabIndex = 69;
            this.labelB.Text = "外緣 (pixels)";
            // 
            // numBorderSize
            // 
            this.numBorderSize.Location = new System.Drawing.Point(189, 73);
            this.numBorderSize.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.numBorderSize.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numBorderSize.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numBorderSize.Name = "numBorderSize";
            this.numBorderSize.Size = new System.Drawing.Size(138, 30);
            this.numBorderSize.TabIndex = 68;
            this.numBorderSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numBorderSize.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // btnAutoLineBorders
            // 
            this.btnAutoLineBorders.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAutoLineBorders.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnAutoLineBorders.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnAutoLineBorders.Location = new System.Drawing.Point(367, 30);
            this.btnAutoLineBorders.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnAutoLineBorders.Name = "btnAutoLineBorders";
            this.btnAutoLineBorders.Size = new System.Drawing.Size(130, 55);
            this.btnAutoLineBorders.TabIndex = 49;
            this.btnAutoLineBorders.Text = "一鍵框選";
            this.btnAutoLineBorders.UseVisualStyleBackColor = false;
            // 
            // tbLayoutSubL
            // 
            this.tbLayoutSubL.BackColor = System.Drawing.SystemColors.Control;
            this.tbLayoutSubL.ColumnCount = 1;
            this.tbLayoutSubL.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tbLayoutSubL.Controls.Add(this.lblActiveCarrierID, 0, 0);
            this.tbLayoutSubL.Controls.Add(this.groupBox3, 0, 3);
            this.tbLayoutSubL.Controls.Add(this.groupBox2, 0, 2);
            this.tbLayoutSubL.Controls.Add(this.groupBox1, 0, 1);
            this.tbLayoutSubL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbLayoutSubL.Location = new System.Drawing.Point(8, 6);
            this.tbLayoutSubL.Margin = new System.Windows.Forms.Padding(6, 6, 1, 6);
            this.tbLayoutSubL.Name = "tbLayoutSubL";
            this.tbLayoutSubL.RowCount = 4;
            this.tbLayoutSubL.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 43F));
            this.tbLayoutSubL.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 32F));
            this.tbLayoutSubL.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 36F));
            this.tbLayoutSubL.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 32F));
            this.tbLayoutSubL.Size = new System.Drawing.Size(555, 565);
            this.tbLayoutSubL.TabIndex = 76;
            // 
            // lblActiveCarrierID
            // 
            this.lblActiveCarrierID.BackColor = System.Drawing.Color.Black;
            this.lblActiveCarrierID.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblActiveCarrierID.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblActiveCarrierID.ForeColor = System.Drawing.Color.Lime;
            this.lblActiveCarrierID.Location = new System.Drawing.Point(0, 0);
            this.lblActiveCarrierID.Margin = new System.Windows.Forms.Padding(0);
            this.lblActiveCarrierID.Name = "lblActiveCarrierID";
            this.lblActiveCarrierID.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.lblActiveCarrierID.Size = new System.Drawing.Size(555, 43);
            this.lblActiveCarrierID.TabIndex = 77;
            this.lblActiveCarrierID.Text = "載台 1";
            this.lblActiveCarrierID.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.btnTryQrCode);
            this.groupBox3.Controls.Add(this.rtbCodeContent);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox3.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox3.Location = new System.Drawing.Point(9, 407);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(9, 10, 9, 10);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(14, 4, 3, 22);
            this.groupBox3.Size = new System.Drawing.Size(537, 148);
            this.groupBox3.TabIndex = 76;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "測試掃碼";
            // 
            // btnTryQrCode
            // 
            this.btnTryQrCode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnTryQrCode.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnTryQrCode.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnTryQrCode.Location = new System.Drawing.Point(367, 72);
            this.btnTryQrCode.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnTryQrCode.Name = "btnTryQrCode";
            this.btnTryQrCode.Size = new System.Drawing.Size(130, 55);
            this.btnTryQrCode.TabIndex = 49;
            this.btnTryQrCode.Text = "掃碼";
            this.btnTryQrCode.UseVisualStyleBackColor = false;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radioButtonLn);
            this.groupBox1.Controls.Add(this.radioButtonG);
            this.groupBox1.Controls.Add(this.radioButtonQr);
            this.groupBox1.Controls.Add(this.btnPickGolden);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(9, 53);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(9, 10, 9, 10);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox1.Size = new System.Drawing.Size(537, 147);
            this.groupBox1.TabIndex = 74;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "框選區塊";
            // 
            // radioButtonLn
            // 
            this.radioButtonLn.AutoSize = true;
            this.radioButtonLn.Location = new System.Drawing.Point(75, 73);
            this.radioButtonLn.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.radioButtonLn.Name = "radioButtonLn";
            this.radioButtonLn.Size = new System.Drawing.Size(148, 28);
            this.radioButtonLn.TabIndex = 65;
            this.radioButtonLn.TabStop = true;
            this.radioButtonLn.Text = "邊線 檢測範圍";
            this.radioButtonLn.UseVisualStyleBackColor = true;
            // 
            // radioButtonG
            // 
            this.radioButtonG.AutoSize = true;
            this.radioButtonG.Checked = true;
            this.radioButtonG.Location = new System.Drawing.Point(75, 35);
            this.radioButtonG.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.radioButtonG.Name = "radioButtonG";
            this.radioButtonG.Size = new System.Drawing.Size(148, 28);
            this.radioButtonG.TabIndex = 62;
            this.radioButtonG.TabStop = true;
            this.radioButtonG.Text = "晶粒 匹配模板";
            this.radioButtonG.UseVisualStyleBackColor = true;
            // 
            // radioButtonQr
            // 
            this.radioButtonQr.AutoSize = true;
            this.radioButtonQr.Location = new System.Drawing.Point(75, 112);
            this.radioButtonQr.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.radioButtonQr.Name = "radioButtonQr";
            this.radioButtonQr.Size = new System.Drawing.Size(130, 28);
            this.radioButtonQr.TabIndex = 63;
            this.radioButtonQr.TabStop = true;
            this.radioButtonQr.Text = "二維碼 範圍";
            this.radioButtonQr.UseVisualStyleBackColor = true;
            // 
            // btnPickGolden
            // 
            this.btnPickGolden.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPickGolden.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnPickGolden.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnPickGolden.Location = new System.Drawing.Point(367, 59);
            this.btnPickGolden.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnPickGolden.Name = "btnPickGolden";
            this.btnPickGolden.Size = new System.Drawing.Size(130, 55);
            this.btnPickGolden.TabIndex = 49;
            this.btnPickGolden.Text = "擷取模板";
            this.btnPickGolden.UseVisualStyleBackColor = false;
            // 
            // tbLayoutB
            // 
            this.tbLayoutB.BackColor = System.Drawing.Color.LightSteelBlue;
            this.tbLayoutB.ColumnCount = 3;
            this.tbLayoutB.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 562F));
            this.tbLayoutB.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tbLayoutB.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tbLayoutB.Controls.Add(this.tbLayoutSubR, 2, 0);
            this.tbLayoutB.Controls.Add(this.tbLayoutSubM, 1, 0);
            this.tbLayoutB.Controls.Add(this.tbLayoutSubL, 0, 0);
            this.tbLayoutB.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tbLayoutB.Location = new System.Drawing.Point(0, 591);
            this.tbLayoutB.Margin = new System.Windows.Forms.Padding(0);
            this.tbLayoutB.Name = "tbLayoutB";
            this.tbLayoutB.Padding = new System.Windows.Forms.Padding(2, 0, 2, 4);
            this.tbLayoutB.RowCount = 1;
            this.tbLayoutB.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tbLayoutB.Size = new System.Drawing.Size(1420, 581);
            this.tbLayoutB.TabIndex = 1;
            // 
            // tbLayoutSubR
            // 
            this.tbLayoutSubR.BackColor = System.Drawing.Color.LightSteelBlue;
            this.tbLayoutSubR.Controls.Add(this.btnCancel);
            this.tbLayoutSubR.Controls.Add(this.btnDefectDelete);
            this.tbLayoutSubR.Controls.Add(this.btnDefectClear);
            this.tbLayoutSubR.Controls.Add(this.btnDefectAdd);
            this.tbLayoutSubR.Controls.Add(this.btnSave);
            this.tbLayoutSubR.Controls.Add(this.btnTrain);
            this.tbLayoutSubR.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbLayoutSubR.Location = new System.Drawing.Point(1126, 5);
            this.tbLayoutSubR.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tbLayoutSubR.Name = "tbLayoutSubR";
            this.tbLayoutSubR.Size = new System.Drawing.Size(288, 567);
            this.tbLayoutSubR.TabIndex = 77;
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnCancel.Font = new System.Drawing.Font("Microsoft YaHei UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel.Location = new System.Drawing.Point(29, 469);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(237, 62);
            this.btnCancel.TabIndex = 73;
            this.btnCancel.Text = "取消 修改";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Visible = false;
            // 
            // btnDefectDelete
            // 
            this.btnDefectDelete.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.btnDefectDelete.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnDefectDelete.Font = new System.Drawing.Font("Microsoft YaHei UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDefectDelete.Location = new System.Drawing.Point(29, 113);
            this.btnDefectDelete.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnDefectDelete.Name = "btnDefectDelete";
            this.btnDefectDelete.Size = new System.Drawing.Size(237, 62);
            this.btnDefectDelete.TabIndex = 72;
            this.btnDefectDelete.Text = "缺陷區域 刪除";
            this.btnDefectDelete.UseVisualStyleBackColor = false;
            // 
            // btnDefectClear
            // 
            this.btnDefectClear.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.btnDefectClear.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnDefectClear.Font = new System.Drawing.Font("Microsoft YaHei UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDefectClear.Location = new System.Drawing.Point(29, 185);
            this.btnDefectClear.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnDefectClear.Name = "btnDefectClear";
            this.btnDefectClear.Size = new System.Drawing.Size(237, 62);
            this.btnDefectClear.TabIndex = 71;
            this.btnDefectClear.Text = "缺陷區域 清空";
            this.btnDefectClear.UseVisualStyleBackColor = false;
            // 
            // btnDefectAdd
            // 
            this.btnDefectAdd.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.btnDefectAdd.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnDefectAdd.Font = new System.Drawing.Font("Microsoft YaHei UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDefectAdd.Location = new System.Drawing.Point(29, 41);
            this.btnDefectAdd.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnDefectAdd.Name = "btnDefectAdd";
            this.btnDefectAdd.Size = new System.Drawing.Size(237, 62);
            this.btnDefectAdd.TabIndex = 70;
            this.btnDefectAdd.Text = "缺陷區域 添加";
            this.btnDefectAdd.UseVisualStyleBackColor = false;
            // 
            // btnSave
            // 
            this.btnSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnSave.Font = new System.Drawing.Font("Microsoft YaHei UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSave.Location = new System.Drawing.Point(29, 397);
            this.btnSave.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(237, 62);
            this.btnSave.TabIndex = 69;
            this.btnSave.Text = "保存 參數";
            this.btnSave.UseVisualStyleBackColor = false;
            // 
            // btnTrain
            // 
            this.btnTrain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnTrain.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnTrain.Font = new System.Drawing.Font("Microsoft YaHei UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTrain.Location = new System.Drawing.Point(29, 325);
            this.btnTrain.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnTrain.Name = "btnTrain";
            this.btnTrain.Size = new System.Drawing.Size(237, 62);
            this.btnTrain.TabIndex = 68;
            this.btnTrain.Text = "訓練 模板";
            this.btnTrain.UseVisualStyleBackColor = false;
            this.btnTrain.Visible = false;
            // 
            // FormTemplateEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1420, 1172);
            this.Controls.Add(this.tbLayoutB);
            this.Controls.Add(this.tbLayoutA);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MinimizeBox = false;
            this.Name = "FormTemplateEditor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "匹配模板設定";
            this.tbLayoutSubM.ResumeLayout(false);
            this.tbLayoutA.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numSpanRatio)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBorderIndent)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBorderSize)).EndInit();
            this.tbLayoutSubL.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tbLayoutB.ResumeLayout(false);
            this.tbLayoutSubR.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.RichTextBox rtbCodeContent;
        private System.Windows.Forms.TableLayoutPanel tbLayoutA;
        private JzDisplay.UISpace.DispUI DS1;
        private JzDisplay.UISpace.DispUI DS2;
        private JzDisplay.UISpace.DispUI DS3;
        private System.Windows.Forms.TableLayoutPanel tbLayoutSubM;
        private System.Windows.Forms.PropertyGrid propertyGrid1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnAutoLineBorders;
        private System.Windows.Forms.TableLayoutPanel tbLayoutSubL;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button btnTryQrCode;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radioButtonG;
        private System.Windows.Forms.RadioButton radioButtonQr;
        private System.Windows.Forms.Button btnPickGolden;
        private System.Windows.Forms.TableLayoutPanel tbLayoutB;
        private System.Windows.Forms.Panel tbLayoutSubR;
        private System.Windows.Forms.Button btnDefectDelete;
        private System.Windows.Forms.Button btnDefectClear;
        private System.Windows.Forms.Button btnDefectAdd;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnTrain;
        private System.Windows.Forms.Label labelB;
        private System.Windows.Forms.NumericUpDown numBorderSize;
        private System.Windows.Forms.RadioButton radioButtonLn;
        private System.Windows.Forms.Label labelA;
        private System.Windows.Forms.NumericUpDown numBorderIndent;
        private System.Windows.Forms.Label lblActiveCarrierID;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numSpanRatio;
        private System.Windows.Forms.Button btnBuildMircoTrf;
    }
}