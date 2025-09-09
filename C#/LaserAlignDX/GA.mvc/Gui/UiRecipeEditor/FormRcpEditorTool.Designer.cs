using LaserAlignDX.FormSpace.FPI30Form;

namespace LaserAlignDX.Mvc.Gui
{
    partial class FormRcpEditorTool
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormRcpEditorTool));
            this.tbLayoutDockRight = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.gwPanePropsViewer1 = new LeTian.JxProps.Gui.GwPanePropsViewer();
            this.panelB = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.rdoCarrier2 = new System.Windows.Forms.RadioButton();
            this.rdoCarrier1 = new System.Windows.Forms.RadioButton();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panelBtns = new System.Windows.Forms.Panel();
            this.btnOpenTemplateMatch = new System.Windows.Forms.Button();
            this.btnOpenFlyCamRcpEditor = new System.Windows.Forms.Button();
            this.btnPickGoldenEmptyRegion = new System.Windows.Forms.Button();
            this.btnRunEmptyTrayInspect = new System.Windows.Forms.Button();
            this.btnOpenLightCtrl = new System.Windows.Forms.Button();
            this.btnLoadImage = new System.Windows.Forms.Button();
            this.btnGrabImage = new System.Windows.Forms.Button();
            this.btnPickGoldenChipRegion = new System.Windows.Forms.Button();
            this.panelDockLeft = new System.Windows.Forms.Panel();
            this.btnSaveImage = new System.Windows.Forms.Button();
            this.gvCalibPointsDataGridView1 = new LaserAlignDX.Mvc.Gui.GvCalibPointsDataGridView();
            this.jezTransImageViewPanel1 = new LaserAlignDX.Mvc.Gui.JezTransImageViewPanel();
            this.tbLayoutDockRight.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.panelB.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panelBtns.SuspendLayout();
            this.panelDockLeft.SuspendLayout();
            this.SuspendLayout();
            // 
            // tbLayoutDockRight
            // 
            this.tbLayoutDockRight.BackColor = System.Drawing.Color.WhiteSmoke;
            this.tbLayoutDockRight.ColumnCount = 1;
            this.tbLayoutDockRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tbLayoutDockRight.Controls.Add(this.groupBox1, 0, 3);
            this.tbLayoutDockRight.Controls.Add(this.gvCalibPointsDataGridView1, 0, 1);
            this.tbLayoutDockRight.Controls.Add(this.panelB, 0, 4);
            this.tbLayoutDockRight.Controls.Add(this.tableLayoutPanel1, 0, 0);
            this.tbLayoutDockRight.Controls.Add(this.panelBtns, 0, 2);
            this.tbLayoutDockRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.tbLayoutDockRight.Location = new System.Drawing.Point(707, 0);
            this.tbLayoutDockRight.Margin = new System.Windows.Forms.Padding(0);
            this.tbLayoutDockRight.Name = "tbLayoutDockRight";
            this.tbLayoutDockRight.RowCount = 5;
            this.tbLayoutDockRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.tbLayoutDockRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tbLayoutDockRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tbLayoutDockRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tbLayoutDockRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tbLayoutDockRight.Size = new System.Drawing.Size(723, 951);
            this.tbLayoutDockRight.TabIndex = 31;
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.WhiteSmoke;
            this.groupBox1.Controls.Add(this.gwPanePropsViewer1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(4, 294);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(8, 18, 8, 12);
            this.groupBox1.Size = new System.Drawing.Size(715, 554);
            this.groupBox1.TabIndex = 29;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "像測參數";
            // 
            // gwPanePropsViewer1
            // 
            this.gwPanePropsViewer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gwPanePropsViewer1.Editable = true;
            this.gwPanePropsViewer1.ImageList = null;
            this.gwPanePropsViewer1.Location = new System.Drawing.Point(8, 36);
            this.gwPanePropsViewer1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.gwPanePropsViewer1.Name = "gwPanePropsViewer1";
            this.gwPanePropsViewer1.Size = new System.Drawing.Size(699, 506);
            this.gwPanePropsViewer1.TabIndex = 0;
            // 
            // panelB
            // 
            this.panelB.BackColor = System.Drawing.Color.Transparent;
            this.panelB.Controls.Add(this.btnCancel);
            this.panelB.Controls.Add(this.btnOK);
            this.panelB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelB.Location = new System.Drawing.Point(0, 851);
            this.panelB.Margin = new System.Windows.Forms.Padding(0);
            this.panelB.Name = "panelB";
            this.panelB.Size = new System.Drawing.Size(723, 100);
            this.panelB.TabIndex = 32;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnCancel.Location = new System.Drawing.Point(561, 32);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(148, 52);
            this.btnCancel.TabIndex = 24;
            this.btnCancel.Text = "退出";
            this.btnCancel.UseVisualStyleBackColor = false;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnOK.Location = new System.Drawing.Point(405, 32);
            this.btnOK.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(148, 52);
            this.btnOK.TabIndex = 23;
            this.btnOK.Text = "保存资料";
            this.btnOK.UseVisualStyleBackColor = false;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.Silver;
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
            this.tableLayoutPanel1.Size = new System.Drawing.Size(717, 74);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.LightSteelBlue;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.rdoCarrier2);
            this.panel1.Controls.Add(this.rdoCarrier1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);
            this.panel1.Size = new System.Drawing.Size(352, 68);
            this.panel1.TabIndex = 29;
            // 
            // rdoCarrier2
            // 
            this.rdoCarrier2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.rdoCarrier2.Appearance = System.Windows.Forms.Appearance.Button;
            this.rdoCarrier2.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.rdoCarrier2.FlatAppearance.CheckedBackColor = System.Drawing.Color.Lime;
            this.rdoCarrier2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.rdoCarrier2.Font = new System.Drawing.Font("微軟正黑體", 16.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.rdoCarrier2.ForeColor = System.Drawing.Color.Black;
            this.rdoCarrier2.Location = new System.Drawing.Point(180, 8);
            this.rdoCarrier2.Name = "rdoCarrier2";
            this.rdoCarrier2.Size = new System.Drawing.Size(159, 50);
            this.rdoCarrier2.TabIndex = 8;
            this.rdoCarrier2.Text = "載台 2";
            this.rdoCarrier2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rdoCarrier2.UseVisualStyleBackColor = false;
            // 
            // rdoCarrier1
            // 
            this.rdoCarrier1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.rdoCarrier1.Appearance = System.Windows.Forms.Appearance.Button;
            this.rdoCarrier1.Checked = true;
            this.rdoCarrier1.FlatAppearance.CheckedBackColor = System.Drawing.Color.Lime;
            this.rdoCarrier1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.rdoCarrier1.Font = new System.Drawing.Font("微軟正黑體", 16.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.rdoCarrier1.ForeColor = System.Drawing.Color.Black;
            this.rdoCarrier1.Location = new System.Drawing.Point(12, 8);
            this.rdoCarrier1.Name = "rdoCarrier1";
            this.rdoCarrier1.Size = new System.Drawing.Size(159, 50);
            this.rdoCarrier1.TabIndex = 7;
            this.rdoCarrier1.TabStop = true;
            this.rdoCarrier1.Text = "載台 1";
            this.rdoCarrier1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rdoCarrier1.UseVisualStyleBackColor = true;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.LightSteelBlue;
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(361, 3);
            this.panel2.Name = "panel2";
            this.panel2.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);
            this.panel2.Size = new System.Drawing.Size(353, 68);
            this.panel2.TabIndex = 30;
            // 
            // panelBtns
            // 
            this.panelBtns.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.panelBtns.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelBtns.Controls.Add(this.btnSaveImage);
            this.panelBtns.Controls.Add(this.btnOpenTemplateMatch);
            this.panelBtns.Controls.Add(this.btnOpenFlyCamRcpEditor);
            this.panelBtns.Controls.Add(this.btnPickGoldenEmptyRegion);
            this.panelBtns.Controls.Add(this.btnRunEmptyTrayInspect);
            this.panelBtns.Controls.Add(this.btnOpenLightCtrl);
            this.panelBtns.Controls.Add(this.btnLoadImage);
            this.panelBtns.Controls.Add(this.btnGrabImage);
            this.panelBtns.Controls.Add(this.btnPickGoldenChipRegion);
            this.panelBtns.Location = new System.Drawing.Point(4, 144);
            this.panelBtns.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.panelBtns.Name = "panelBtns";
            this.panelBtns.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.panelBtns.Size = new System.Drawing.Size(715, 144);
            this.panelBtns.TabIndex = 28;
            this.panelBtns.Text = "像測操作";
            // 
            // btnOpenTemplateMatch
            // 
            this.btnOpenTemplateMatch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.btnOpenTemplateMatch.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnOpenTemplateMatch.Font = new System.Drawing.Font("微軟正黑體", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnOpenTemplateMatch.Location = new System.Drawing.Point(428, 75);
            this.btnOpenTemplateMatch.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnOpenTemplateMatch.Name = "btnOpenTemplateMatch";
            this.btnOpenTemplateMatch.Size = new System.Drawing.Size(120, 52);
            this.btnOpenTemplateMatch.TabIndex = 29;
            this.btnOpenTemplateMatch.Text = "晶粒模板";
            this.btnOpenTemplateMatch.UseVisualStyleBackColor = false;
            // 
            // btnOpenFlyCamRcpEditor
            // 
            this.btnOpenFlyCamRcpEditor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.btnOpenFlyCamRcpEditor.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnOpenFlyCamRcpEditor.Font = new System.Drawing.Font("微軟正黑體", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnOpenFlyCamRcpEditor.Location = new System.Drawing.Point(556, 17);
            this.btnOpenFlyCamRcpEditor.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnOpenFlyCamRcpEditor.Name = "btnOpenFlyCamRcpEditor";
            this.btnOpenFlyCamRcpEditor.Size = new System.Drawing.Size(120, 110);
            this.btnOpenFlyCamRcpEditor.TabIndex = 28;
            this.btnOpenFlyCamRcpEditor.Text = "飛拍參數";
            this.btnOpenFlyCamRcpEditor.UseVisualStyleBackColor = false;
            // 
            // btnPickGoldenEmptyRegion
            // 
            this.btnPickGoldenEmptyRegion.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnPickGoldenEmptyRegion.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnPickGoldenEmptyRegion.Font = new System.Drawing.Font("微軟正黑體", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnPickGoldenEmptyRegion.Location = new System.Drawing.Point(300, 17);
            this.btnPickGoldenEmptyRegion.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnPickGoldenEmptyRegion.Name = "btnPickGoldenEmptyRegion";
            this.btnPickGoldenEmptyRegion.Size = new System.Drawing.Size(120, 52);
            this.btnPickGoldenEmptyRegion.TabIndex = 27;
            this.btnPickGoldenEmptyRegion.Text = "框取空位";
            this.btnPickGoldenEmptyRegion.UseVisualStyleBackColor = false;
            // 
            // btnRunEmptyTrayInspect
            // 
            this.btnRunEmptyTrayInspect.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnRunEmptyTrayInspect.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnRunEmptyTrayInspect.Font = new System.Drawing.Font("微軟正黑體", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnRunEmptyTrayInspect.Location = new System.Drawing.Point(300, 75);
            this.btnRunEmptyTrayInspect.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnRunEmptyTrayInspect.Name = "btnRunEmptyTrayInspect";
            this.btnRunEmptyTrayInspect.Size = new System.Drawing.Size(120, 52);
            this.btnRunEmptyTrayInspect.TabIndex = 26;
            this.btnRunEmptyTrayInspect.Text = "空盤檢測";
            this.btnRunEmptyTrayInspect.UseVisualStyleBackColor = false;
            // 
            // btnOpenLightCtrl
            // 
            this.btnOpenLightCtrl.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.btnOpenLightCtrl.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnOpenLightCtrl.Font = new System.Drawing.Font("微軟正黑體", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnOpenLightCtrl.Location = new System.Drawing.Point(38, 75);
            this.btnOpenLightCtrl.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnOpenLightCtrl.Name = "btnOpenLightCtrl";
            this.btnOpenLightCtrl.Size = new System.Drawing.Size(120, 52);
            this.btnOpenLightCtrl.TabIndex = 25;
            this.btnOpenLightCtrl.Text = "燈光設定";
            this.btnOpenLightCtrl.UseVisualStyleBackColor = false;
            // 
            // btnLoadImage
            // 
            this.btnLoadImage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnLoadImage.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnLoadImage.Font = new System.Drawing.Font("微軟正黑體", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnLoadImage.Location = new System.Drawing.Point(172, 17);
            this.btnLoadImage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnLoadImage.Name = "btnLoadImage";
            this.btnLoadImage.Size = new System.Drawing.Size(120, 52);
            this.btnLoadImage.TabIndex = 22;
            this.btnLoadImage.Text = "加載圖片";
            this.btnLoadImage.UseVisualStyleBackColor = false;
            // 
            // btnGrabImage
            // 
            this.btnGrabImage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnGrabImage.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnGrabImage.Font = new System.Drawing.Font("微軟正黑體", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnGrabImage.Location = new System.Drawing.Point(38, 17);
            this.btnGrabImage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnGrabImage.Name = "btnGrabImage";
            this.btnGrabImage.Size = new System.Drawing.Size(120, 52);
            this.btnGrabImage.TabIndex = 21;
            this.btnGrabImage.Text = "取像";
            this.btnGrabImage.UseVisualStyleBackColor = false;
            // 
            // btnPickGoldenChipRegion
            // 
            this.btnPickGoldenChipRegion.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.btnPickGoldenChipRegion.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnPickGoldenChipRegion.Font = new System.Drawing.Font("微軟正黑體", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnPickGoldenChipRegion.Location = new System.Drawing.Point(428, 17);
            this.btnPickGoldenChipRegion.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnPickGoldenChipRegion.Name = "btnPickGoldenChipRegion";
            this.btnPickGoldenChipRegion.Size = new System.Drawing.Size(120, 52);
            this.btnPickGoldenChipRegion.TabIndex = 17;
            this.btnPickGoldenChipRegion.Text = "框取晶粒";
            this.btnPickGoldenChipRegion.UseVisualStyleBackColor = false;
            // 
            // panelDockLeft
            // 
            this.panelDockLeft.Controls.Add(this.jezTransImageViewPanel1);
            this.panelDockLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelDockLeft.Location = new System.Drawing.Point(2, 0);
            this.panelDockLeft.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.panelDockLeft.Name = "panelDockLeft";
            this.panelDockLeft.Size = new System.Drawing.Size(665, 951);
            this.panelDockLeft.TabIndex = 1;
            // 
            // btnSaveImage
            // 
            this.btnSaveImage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnSaveImage.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnSaveImage.Font = new System.Drawing.Font("微軟正黑體", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnSaveImage.Location = new System.Drawing.Point(172, 75);
            this.btnSaveImage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnSaveImage.Name = "btnSaveImage";
            this.btnSaveImage.Size = new System.Drawing.Size(120, 52);
            this.btnSaveImage.TabIndex = 30;
            this.btnSaveImage.Text = "另存圖片";
            this.btnSaveImage.UseVisualStyleBackColor = false;
            // 
            // gvCalibPointsDataGridView1
            // 
            this.gvCalibPointsDataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gvCalibPointsDataGridView1.Location = new System.Drawing.Point(3, 83);
            this.gvCalibPointsDataGridView1.Name = "gvCalibPointsDataGridView1";
            this.gvCalibPointsDataGridView1.Padding = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.gvCalibPointsDataGridView1.SelectedIndex = 0;
            this.gvCalibPointsDataGridView1.Size = new System.Drawing.Size(717, 55);
            this.gvCalibPointsDataGridView1.TabIndex = 0;
            // 
            // jezTransImageViewPanel1
            // 
            this.jezTransImageViewPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.jezTransImageViewPanel1.Location = new System.Drawing.Point(0, 0);
            this.jezTransImageViewPanel1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.jezTransImageViewPanel1.Name = "jezTransImageViewPanel1";
            this.jezTransImageViewPanel1.Padding = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.jezTransImageViewPanel1.Size = new System.Drawing.Size(665, 951);
            this.jezTransImageViewPanel1.TabIndex = 0;
            // 
            // FormRcpEditorTool
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Gray;
            this.ClientSize = new System.Drawing.Size(1432, 952);
            this.Controls.Add(this.tbLayoutDockRight);
            this.Controls.Add(this.panelDockLeft);
            this.Font = new System.Drawing.Font("新細明體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "FormRcpEditorTool";
            this.Padding = new System.Windows.Forms.Padding(2, 0, 2, 1);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "參數設定";
            this.tbLayoutDockRight.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.panelB.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panelBtns.ResumeLayout(false);
            this.panelDockLeft.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        //private System.Windows.Forms.TabPage tabPage01;
        private System.Windows.Forms.Panel panelDockLeft;
        //private System.Windows.Forms.TabPage tabPage00;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private global::LaserAlignDX.Mvc.Gui.JezTransImageViewPanel jezTransImageViewPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panelBtns;
        private System.Windows.Forms.Button btnOpenLightCtrl;
        private System.Windows.Forms.Button btnLoadImage;
        private System.Windows.Forms.Button btnGrabImage;
        private System.Windows.Forms.Button btnPickGoldenChipRegion;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.RadioButton rdoCarrier2;
        private System.Windows.Forms.RadioButton rdoCarrier1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tbLayoutDockRight;
        private System.Windows.Forms.Panel panelB;
        private System.Windows.Forms.GroupBox groupBox1;
        private LeTian.JxProps.Gui.GwPanePropsViewer gwPanePropsViewer1;
        private System.Windows.Forms.Button btnRunEmptyTrayInspect;
        private System.Windows.Forms.Button btnOpenFlyCamRcpEditor;
        private System.Windows.Forms.Button btnPickGoldenEmptyRegion;
        private System.Windows.Forms.Button btnOpenTemplateMatch;
        private GvCalibPointsDataGridView gvCalibPointsDataGridView1;
        private System.Windows.Forms.Button btnSaveImage;
    }
}