namespace LaserAlignDX.Mvc.Gui.Calib.V25
{
    partial class FormCalibrationTool
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormCalibrationTool));
            this.tblayoutTopNav = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.rdoCalibInkView = new System.Windows.Forms.RadioButton();
            this.rdoCalibGridView = new System.Windows.Forms.RadioButton();
            this.panel1 = new System.Windows.Forms.TableLayoutPanel();
            this.rdoCarrier2 = new System.Windows.Forms.RadioButton();
            this.rdoCarrier1 = new System.Windows.Forms.RadioButton();
            this.panel2 = new System.Windows.Forms.TableLayoutPanel();
            this.rdoSucker2 = new System.Windows.Forms.RadioButton();
            this.rdoSucker1 = new System.Windows.Forms.RadioButton();
            this.tbLayoutMain = new System.Windows.Forms.TableLayoutPanel();
            this.tbLayoutDockRight = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.gwPanePropsViewer1 = new LeTian.JxProps.Gui.GwPanePropsViewer();
            this.panelB = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.panelBtns = new System.Windows.Forms.Panel();
            this.btnLoadImage = new System.Windows.Forms.Button();
            this.btnGrabImage = new System.Windows.Forms.Button();
            this.btnAutoFetchInkMarks = new System.Windows.Forms.Button();
            this.btnBuildCalib = new System.Windows.Forms.Button();
            this.btnAutoFetchGrid = new System.Windows.Forms.Button();
            this.panelViewers = new System.Windows.Forms.Panel();
            this.gvCalibPointsDataGridView1 = new LaserAlignDX.Mvc.Gui.GvCalibPointsDataGridView();
            this.jezTransImageViewPanel2 = new LaserAlignDX.Mvc.Gui.JezTransImageViewPanel();
            this.jezTransImageViewPanel1 = new LaserAlignDX.Mvc.Gui.JezTransImageViewPanel();
            this.tblayoutTopNav.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.tbLayoutMain.SuspendLayout();
            this.tbLayoutDockRight.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.panelB.SuspendLayout();
            this.panelBtns.SuspendLayout();
            this.panelViewers.SuspendLayout();
            this.SuspendLayout();
            // 
            // tblayoutTopNav
            // 
            this.tblayoutTopNav.BackColor = System.Drawing.Color.Silver;
            this.tblayoutTopNav.ColumnCount = 3;
            this.tblayoutTopNav.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblayoutTopNav.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tblayoutTopNav.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tblayoutTopNav.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tblayoutTopNav.Controls.Add(this.panel1, 1, 0);
            this.tblayoutTopNav.Controls.Add(this.panel2, 2, 0);
            this.tblayoutTopNav.Dock = System.Windows.Forms.DockStyle.Top;
            this.tblayoutTopNav.Location = new System.Drawing.Point(2, 0);
            this.tblayoutTopNav.Name = "tblayoutTopNav";
            this.tblayoutTopNav.RowCount = 1;
            this.tblayoutTopNav.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblayoutTopNav.Size = new System.Drawing.Size(1258, 100);
            this.tblayoutTopNav.TabIndex = 32;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.BackColor = System.Drawing.Color.LightSteelBlue;
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.rdoCalibInkView, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.rdoCalibGridView, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 94F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(623, 94);
            this.tableLayoutPanel2.TabIndex = 31;
            // 
            // rdoCalibInkView
            // 
            this.rdoCalibInkView.Appearance = System.Windows.Forms.Appearance.Button;
            this.rdoCalibInkView.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.rdoCalibInkView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rdoCalibInkView.FlatAppearance.CheckedBackColor = System.Drawing.Color.Cyan;
            this.rdoCalibInkView.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.rdoCalibInkView.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rdoCalibInkView.ForeColor = System.Drawing.Color.Black;
            this.rdoCalibInkView.Location = new System.Drawing.Point(314, 5);
            this.rdoCalibInkView.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.rdoCalibInkView.Name = "rdoCalibInkView";
            this.rdoCalibInkView.Size = new System.Drawing.Size(306, 84);
            this.rdoCalibInkView.TabIndex = 9;
            this.rdoCalibInkView.Text = "2. 點墨校正";
            this.rdoCalibInkView.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rdoCalibInkView.UseVisualStyleBackColor = false;
            // 
            // rdoCalibGridView
            // 
            this.rdoCalibGridView.Appearance = System.Windows.Forms.Appearance.Button;
            this.rdoCalibGridView.Checked = true;
            this.rdoCalibGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rdoCalibGridView.FlatAppearance.CheckedBackColor = System.Drawing.Color.Cyan;
            this.rdoCalibGridView.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.rdoCalibGridView.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rdoCalibGridView.ForeColor = System.Drawing.Color.Black;
            this.rdoCalibGridView.Location = new System.Drawing.Point(3, 5);
            this.rdoCalibGridView.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.rdoCalibGridView.Name = "rdoCalibGridView";
            this.rdoCalibGridView.Size = new System.Drawing.Size(305, 84);
            this.rdoCalibGridView.TabIndex = 7;
            this.rdoCalibGridView.TabStop = true;
            this.rdoCalibGridView.Text = "1. 大校正板";
            this.rdoCalibGridView.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rdoCalibGridView.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.LightSteelBlue;
            this.panel1.ColumnCount = 2;
            this.panel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.panel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.panel1.Controls.Add(this.rdoCarrier2, 1, 0);
            this.panel1.Controls.Add(this.rdoCarrier1, 0, 0);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(632, 3);
            this.panel1.Name = "panel1";
            this.panel1.RowCount = 1;
            this.panel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.panel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 94F));
            this.panel1.Size = new System.Drawing.Size(308, 94);
            this.panel1.TabIndex = 29;
            // 
            // rdoCarrier2
            // 
            this.rdoCarrier2.Appearance = System.Windows.Forms.Appearance.Button;
            this.rdoCarrier2.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.rdoCarrier2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rdoCarrier2.FlatAppearance.CheckedBackColor = System.Drawing.Color.Lime;
            this.rdoCarrier2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.rdoCarrier2.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rdoCarrier2.ForeColor = System.Drawing.Color.Black;
            this.rdoCarrier2.Location = new System.Drawing.Point(157, 5);
            this.rdoCarrier2.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.rdoCarrier2.Name = "rdoCarrier2";
            this.rdoCarrier2.Size = new System.Drawing.Size(148, 84);
            this.rdoCarrier2.TabIndex = 9;
            this.rdoCarrier2.Text = "載台 2";
            this.rdoCarrier2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rdoCarrier2.UseVisualStyleBackColor = false;
            // 
            // rdoCarrier1
            // 
            this.rdoCarrier1.Appearance = System.Windows.Forms.Appearance.Button;
            this.rdoCarrier1.Checked = true;
            this.rdoCarrier1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rdoCarrier1.FlatAppearance.CheckedBackColor = System.Drawing.Color.Lime;
            this.rdoCarrier1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.rdoCarrier1.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rdoCarrier1.ForeColor = System.Drawing.Color.Black;
            this.rdoCarrier1.Location = new System.Drawing.Point(3, 5);
            this.rdoCarrier1.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.rdoCarrier1.Name = "rdoCarrier1";
            this.rdoCarrier1.Size = new System.Drawing.Size(148, 84);
            this.rdoCarrier1.TabIndex = 7;
            this.rdoCarrier1.TabStop = true;
            this.rdoCarrier1.Text = "載台 1";
            this.rdoCarrier1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rdoCarrier1.UseVisualStyleBackColor = true;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.LightSteelBlue;
            this.panel2.ColumnCount = 2;
            this.panel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.panel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.panel2.Controls.Add(this.rdoSucker2, 1, 0);
            this.panel2.Controls.Add(this.rdoSucker1, 0, 0);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(946, 3);
            this.panel2.Name = "panel2";
            this.panel2.RowCount = 1;
            this.panel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.panel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 94F));
            this.panel2.Size = new System.Drawing.Size(309, 94);
            this.panel2.TabIndex = 30;
            // 
            // rdoSucker2
            // 
            this.rdoSucker2.Appearance = System.Windows.Forms.Appearance.Button;
            this.rdoSucker2.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.rdoSucker2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rdoSucker2.FlatAppearance.CheckedBackColor = System.Drawing.Color.Lime;
            this.rdoSucker2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.rdoSucker2.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rdoSucker2.ForeColor = System.Drawing.Color.Black;
            this.rdoSucker2.Location = new System.Drawing.Point(157, 5);
            this.rdoSucker2.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.rdoSucker2.Name = "rdoSucker2";
            this.rdoSucker2.Size = new System.Drawing.Size(149, 84);
            this.rdoSucker2.TabIndex = 11;
            this.rdoSucker2.Text = "吸嘴排 2";
            this.rdoSucker2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rdoSucker2.UseVisualStyleBackColor = false;
            // 
            // rdoSucker1
            // 
            this.rdoSucker1.Appearance = System.Windows.Forms.Appearance.Button;
            this.rdoSucker1.Checked = true;
            this.rdoSucker1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rdoSucker1.FlatAppearance.CheckedBackColor = System.Drawing.Color.Lime;
            this.rdoSucker1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.rdoSucker1.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rdoSucker1.ForeColor = System.Drawing.Color.Black;
            this.rdoSucker1.Location = new System.Drawing.Point(3, 5);
            this.rdoSucker1.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.rdoSucker1.Name = "rdoSucker1";
            this.rdoSucker1.Size = new System.Drawing.Size(148, 84);
            this.rdoSucker1.TabIndex = 9;
            this.rdoSucker1.TabStop = true;
            this.rdoSucker1.Text = "吸嘴排 1";
            this.rdoSucker1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rdoSucker1.UseVisualStyleBackColor = true;
            // 
            // tbLayoutMain
            // 
            this.tbLayoutMain.ColumnCount = 2;
            this.tbLayoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tbLayoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tbLayoutMain.Controls.Add(this.tbLayoutDockRight, 1, 0);
            this.tbLayoutMain.Controls.Add(this.panelViewers, 0, 0);
            this.tbLayoutMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbLayoutMain.Location = new System.Drawing.Point(2, 100);
            this.tbLayoutMain.Name = "tbLayoutMain";
            this.tbLayoutMain.RowCount = 1;
            this.tbLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tbLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tbLayoutMain.Size = new System.Drawing.Size(1258, 875);
            this.tbLayoutMain.TabIndex = 33;
            // 
            // tbLayoutDockRight
            // 
            this.tbLayoutDockRight.BackColor = System.Drawing.Color.WhiteSmoke;
            this.tbLayoutDockRight.ColumnCount = 1;
            this.tbLayoutDockRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tbLayoutDockRight.Controls.Add(this.groupBox1, 0, 2);
            this.tbLayoutDockRight.Controls.Add(this.gvCalibPointsDataGridView1, 0, 0);
            this.tbLayoutDockRight.Controls.Add(this.panelB, 0, 3);
            this.tbLayoutDockRight.Controls.Add(this.panelBtns, 0, 1);
            this.tbLayoutDockRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbLayoutDockRight.Location = new System.Drawing.Point(629, 0);
            this.tbLayoutDockRight.Margin = new System.Windows.Forms.Padding(0);
            this.tbLayoutDockRight.Name = "tbLayoutDockRight";
            this.tbLayoutDockRight.RowCount = 4;
            this.tbLayoutDockRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tbLayoutDockRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tbLayoutDockRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tbLayoutDockRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.tbLayoutDockRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tbLayoutDockRight.Size = new System.Drawing.Size(629, 875);
            this.tbLayoutDockRight.TabIndex = 32;
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
            this.groupBox1.Size = new System.Drawing.Size(621, 498);
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
            this.gwPanePropsViewer1.Size = new System.Drawing.Size(605, 450);
            this.gwPanePropsViewer1.TabIndex = 0;
            // 
            // panelB
            // 
            this.panelB.BackColor = System.Drawing.Color.LightSteelBlue;
            this.panelB.Controls.Add(this.btnCancel);
            this.panelB.Controls.Add(this.btnOK);
            this.panelB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelB.Location = new System.Drawing.Point(0, 795);
            this.panelB.Margin = new System.Windows.Forms.Padding(0);
            this.panelB.Name = "panelB";
            this.panelB.Size = new System.Drawing.Size(629, 80);
            this.panelB.TabIndex = 32;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnCancel.Font = new System.Drawing.Font("微软雅黑", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel.Location = new System.Drawing.Point(467, 12);
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
            this.btnOK.Font = new System.Drawing.Font("微软雅黑", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOK.Location = new System.Drawing.Point(311, 12);
            this.btnOK.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(148, 52);
            this.btnOK.TabIndex = 23;
            this.btnOK.Text = "保存资料";
            this.btnOK.UseVisualStyleBackColor = false;
            // 
            // panelBtns
            // 
            this.panelBtns.BackColor = System.Drawing.Color.LightSteelBlue;
            this.panelBtns.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelBtns.Controls.Add(this.btnLoadImage);
            this.panelBtns.Controls.Add(this.btnGrabImage);
            this.panelBtns.Controls.Add(this.btnBuildCalib);
            this.panelBtns.Controls.Add(this.btnAutoFetchGrid);
            this.panelBtns.Controls.Add(this.btnAutoFetchInkMarks);
            this.panelBtns.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelBtns.Location = new System.Drawing.Point(4, 180);
            this.panelBtns.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.panelBtns.Name = "panelBtns";
            this.panelBtns.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.panelBtns.Size = new System.Drawing.Size(621, 108);
            this.panelBtns.TabIndex = 28;
            this.panelBtns.Text = "像測操作";
            // 
            // btnLoadImage
            // 
            this.btnLoadImage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnLoadImage.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnLoadImage.Font = new System.Drawing.Font("微软雅黑", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLoadImage.Location = new System.Drawing.Point(55, 53);
            this.btnLoadImage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnLoadImage.Name = "btnLoadImage";
            this.btnLoadImage.Size = new System.Drawing.Size(150, 38);
            this.btnLoadImage.TabIndex = 22;
            this.btnLoadImage.Text = "加載圖片";
            this.btnLoadImage.UseVisualStyleBackColor = false;
            // 
            // btnGrabImage
            // 
            this.btnGrabImage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnGrabImage.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnGrabImage.Font = new System.Drawing.Font("微软雅黑", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnGrabImage.Location = new System.Drawing.Point(55, 11);
            this.btnGrabImage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnGrabImage.Name = "btnGrabImage";
            this.btnGrabImage.Size = new System.Drawing.Size(150, 38);
            this.btnGrabImage.TabIndex = 21;
            this.btnGrabImage.Text = "取像";
            this.btnGrabImage.UseVisualStyleBackColor = false;
            // 
            // btnAutoFetchInkMarks
            // 
            this.btnAutoFetchInkMarks.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.btnAutoFetchInkMarks.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnAutoFetchInkMarks.Font = new System.Drawing.Font("微软雅黑", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAutoFetchInkMarks.Location = new System.Drawing.Point(238, 11);
            this.btnAutoFetchInkMarks.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnAutoFetchInkMarks.Name = "btnAutoFetchInkMarks";
            this.btnAutoFetchInkMarks.Size = new System.Drawing.Size(150, 80);
            this.btnAutoFetchInkMarks.TabIndex = 28;
            this.btnAutoFetchInkMarks.Text = "自動抓取 墨點";
            this.btnAutoFetchInkMarks.UseVisualStyleBackColor = false;
            this.btnAutoFetchInkMarks.Visible = false;
            // 
            // btnBuildCalib
            // 
            this.btnBuildCalib.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.btnBuildCalib.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnBuildCalib.Font = new System.Drawing.Font("微软雅黑", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnBuildCalib.Location = new System.Drawing.Point(413, 11);
            this.btnBuildCalib.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnBuildCalib.Name = "btnBuildCalib";
            this.btnBuildCalib.Size = new System.Drawing.Size(150, 80);
            this.btnBuildCalib.TabIndex = 25;
            this.btnBuildCalib.Text = "執行校正";
            this.btnBuildCalib.UseVisualStyleBackColor = false;
            this.btnBuildCalib.Visible = false;
            // 
            // btnAutoFetchGrid
            // 
            this.btnAutoFetchGrid.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.btnAutoFetchGrid.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnAutoFetchGrid.Font = new System.Drawing.Font("微软雅黑", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAutoFetchGrid.Location = new System.Drawing.Point(238, 11);
            this.btnAutoFetchGrid.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnAutoFetchGrid.Name = "btnAutoFetchGrid";
            this.btnAutoFetchGrid.Size = new System.Drawing.Size(150, 81);
            this.btnAutoFetchGrid.TabIndex = 26;
            this.btnAutoFetchGrid.Text = "自動抓取 格點";
            this.btnAutoFetchGrid.UseVisualStyleBackColor = false;
            // 
            // panelViewers
            // 
            this.panelViewers.Controls.Add(this.jezTransImageViewPanel2);
            this.panelViewers.Controls.Add(this.jezTransImageViewPanel1);
            this.panelViewers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelViewers.Location = new System.Drawing.Point(4, 3);
            this.panelViewers.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.panelViewers.Name = "panelViewers";
            this.panelViewers.Size = new System.Drawing.Size(621, 869);
            this.panelViewers.TabIndex = 2;
            // 
            // gvCalibPointsDataGridView1
            // 
            this.gvCalibPointsDataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gvCalibPointsDataGridView1.Font = new System.Drawing.Font("微软雅黑", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gvCalibPointsDataGridView1.Location = new System.Drawing.Point(3, 4);
            this.gvCalibPointsDataGridView1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.gvCalibPointsDataGridView1.Name = "gvCalibPointsDataGridView1";
            this.gvCalibPointsDataGridView1.Padding = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.gvCalibPointsDataGridView1.SelectedIndex = 0;
            this.gvCalibPointsDataGridView1.Size = new System.Drawing.Size(623, 169);
            this.gvCalibPointsDataGridView1.TabIndex = 0;
            // 
            // jezTransImageViewPanel2
            // 
            this.jezTransImageViewPanel2.Dock = System.Windows.Forms.DockStyle.Right;
            this.jezTransImageViewPanel2.Location = new System.Drawing.Point(371, 0);
            this.jezTransImageViewPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.jezTransImageViewPanel2.Name = "jezTransImageViewPanel2";
            this.jezTransImageViewPanel2.Size = new System.Drawing.Size(250, 869);
            this.jezTransImageViewPanel2.TabIndex = 4;
            // 
            // jezTransImageViewPanel1
            // 
            this.jezTransImageViewPanel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.jezTransImageViewPanel1.Location = new System.Drawing.Point(0, 0);
            this.jezTransImageViewPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.jezTransImageViewPanel1.Name = "jezTransImageViewPanel1";
            this.jezTransImageViewPanel1.Size = new System.Drawing.Size(250, 869);
            this.jezTransImageViewPanel1.TabIndex = 2;
            // 
            // FormCalibrationTool
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Gray;
            this.ClientSize = new System.Drawing.Size(1262, 976);
            this.Controls.Add(this.tbLayoutMain);
            this.Controls.Add(this.tblayoutTopNav);
            this.Font = new System.Drawing.Font("新細明體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "FormCalibrationTool";
            this.Padding = new System.Windows.Forms.Padding(2, 0, 2, 1);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "校正設定";
            this.tblayoutTopNav.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.tbLayoutMain.ResumeLayout(false);
            this.tbLayoutDockRight.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.panelB.ResumeLayout(false);
            this.panelBtns.ResumeLayout(false);
            this.panelViewers.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TableLayoutPanel tblayoutTopNav;
        private System.Windows.Forms.TableLayoutPanel panel1;
        private System.Windows.Forms.RadioButton rdoCarrier2;
        private System.Windows.Forms.RadioButton rdoCarrier1;
        private System.Windows.Forms.TableLayoutPanel panel2;
        private System.Windows.Forms.RadioButton rdoSucker2;
        private System.Windows.Forms.RadioButton rdoSucker1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.RadioButton rdoCalibInkView;
        private System.Windows.Forms.RadioButton rdoCalibGridView;
        private System.Windows.Forms.TableLayoutPanel tbLayoutMain;
        private System.Windows.Forms.Panel panelViewers;
        private JezTransImageViewPanel jezTransImageViewPanel2;
        private JezTransImageViewPanel jezTransImageViewPanel1;
        private System.Windows.Forms.TableLayoutPanel tbLayoutDockRight;
        private System.Windows.Forms.GroupBox groupBox1;
        private LeTian.JxProps.Gui.GwPanePropsViewer gwPanePropsViewer1;
        private GvCalibPointsDataGridView gvCalibPointsDataGridView1;
        private System.Windows.Forms.Panel panelB;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Panel panelBtns;
        private System.Windows.Forms.Button btnBuildCalib;
        private System.Windows.Forms.Button btnLoadImage;
        private System.Windows.Forms.Button btnGrabImage;
        private System.Windows.Forms.Button btnAutoFetchGrid;
        private System.Windows.Forms.Button btnAutoFetchInkMarks;
    }
}