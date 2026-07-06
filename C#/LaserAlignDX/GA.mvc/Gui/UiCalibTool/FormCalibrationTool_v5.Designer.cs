using AX.Gui;

namespace LaserAlignDX.Mvc.Gui.Calib.V5
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
            this.tableLayoutSuckers = new System.Windows.Forms.TableLayoutPanel();
            this.rdoSucker2 = new System.Windows.Forms.RadioButton();
            this.rdoSucker1 = new System.Windows.Forms.RadioButton();
            this.tableLayoutCarriers = new System.Windows.Forms.TableLayoutPanel();
            this.rdoCarrier2 = new System.Windows.Forms.RadioButton();
            this.rdoCarrier1 = new System.Windows.Forms.RadioButton();
            this.btnOpenMotorXY = new System.Windows.Forms.Button();
            this.tbLayoutMain = new System.Windows.Forms.TableLayoutPanel();
            this.tbLayoutDockRight = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.gwPanePropsViewer1 = new LeTian.JxProps.Gui.GwPanePropsViewer();
            this.panelB = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.panelBtns = new System.Windows.Forms.Panel();
            this.gvFocusSettingPanel1 = new AX.Gui.GwMotorSimpleGoPanel();
            this.btnLoadImage = new System.Windows.Forms.Button();
            this.btnGrabImage = new System.Windows.Forms.Button();
            this.btnBuildCalib = new System.Windows.Forms.Button();
            this.btnAutoFetchAll = new System.Windows.Forms.Button();
            this.jezTransImageViewPanel1 = new LaserAlignDX.Mvc.Gui.JezTransImageViewPanel();
            this.gvCalibPointsDataGridView1 = new LaserAlignDX.Mvc.Gui.GvCalibPointsDataGridView();
            this.tblayoutTopNav.SuspendLayout();
            this.tableLayoutSuckers.SuspendLayout();
            this.tableLayoutCarriers.SuspendLayout();
            this.tbLayoutMain.SuspendLayout();
            this.tbLayoutDockRight.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.panelB.SuspendLayout();
            this.panelBtns.SuspendLayout();
            this.SuspendLayout();
            // 
            // tblayoutTopNav
            // 
            this.tblayoutTopNav.BackColor = System.Drawing.Color.LightSteelBlue;
            this.tblayoutTopNav.ColumnCount = 3;
            this.tblayoutTopNav.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblayoutTopNav.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tblayoutTopNav.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tblayoutTopNav.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 15F));
            this.tblayoutTopNav.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 15F));
            this.tblayoutTopNav.Controls.Add(this.tableLayoutSuckers, 1, 0);
            this.tblayoutTopNav.Controls.Add(this.tableLayoutCarriers, 0, 0);
            this.tblayoutTopNav.Controls.Add(this.btnOpenMotorXY, 2, 0);
            this.tblayoutTopNav.Dock = System.Windows.Forms.DockStyle.Top;
            this.tblayoutTopNav.Location = new System.Drawing.Point(1, 0);
            this.tblayoutTopNav.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tblayoutTopNav.Name = "tblayoutTopNav";
            this.tblayoutTopNav.Padding = new System.Windows.Forms.Padding(0, 1, 0, 1);
            this.tblayoutTopNav.RowCount = 1;
            this.tblayoutTopNav.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblayoutTopNav.Size = new System.Drawing.Size(982, 80);
            this.tblayoutTopNav.TabIndex = 32;
            // 
            // tableLayoutSuckers
            // 
            this.tableLayoutSuckers.ColumnCount = 2;
            this.tableLayoutSuckers.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutSuckers.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutSuckers.Controls.Add(this.rdoSucker2, 1, 0);
            this.tableLayoutSuckers.Controls.Add(this.rdoSucker1, 0, 0);
            this.tableLayoutSuckers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutSuckers.Location = new System.Drawing.Point(491, 1);
            this.tableLayoutSuckers.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutSuckers.Name = "tableLayoutSuckers";
            this.tableLayoutSuckers.RowCount = 1;
            this.tableLayoutSuckers.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutSuckers.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 78F));
            this.tableLayoutSuckers.Size = new System.Drawing.Size(392, 78);
            this.tableLayoutSuckers.TabIndex = 40;
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
            this.rdoSucker2.Location = new System.Drawing.Point(198, 5);
            this.rdoSucker2.Margin = new System.Windows.Forms.Padding(2, 5, 2, 5);
            this.rdoSucker2.Name = "rdoSucker2";
            this.rdoSucker2.Size = new System.Drawing.Size(192, 68);
            this.rdoSucker2.TabIndex = 38;
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
            this.rdoSucker1.Location = new System.Drawing.Point(2, 5);
            this.rdoSucker1.Margin = new System.Windows.Forms.Padding(2, 5, 2, 5);
            this.rdoSucker1.Name = "rdoSucker1";
            this.rdoSucker1.Size = new System.Drawing.Size(192, 68);
            this.rdoSucker1.TabIndex = 37;
            this.rdoSucker1.TabStop = true;
            this.rdoSucker1.Text = "吸嘴排 1";
            this.rdoSucker1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rdoSucker1.UseVisualStyleBackColor = true;
            // 
            // tableLayoutCarriers
            // 
            this.tableLayoutCarriers.ColumnCount = 2;
            this.tableLayoutCarriers.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutCarriers.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutCarriers.Controls.Add(this.rdoCarrier2, 1, 0);
            this.tableLayoutCarriers.Controls.Add(this.rdoCarrier1, 0, 0);
            this.tableLayoutCarriers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutCarriers.Location = new System.Drawing.Point(0, 1);
            this.tableLayoutCarriers.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutCarriers.Name = "tableLayoutCarriers";
            this.tableLayoutCarriers.RowCount = 1;
            this.tableLayoutCarriers.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutCarriers.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 78F));
            this.tableLayoutCarriers.Size = new System.Drawing.Size(491, 78);
            this.tableLayoutCarriers.TabIndex = 39;
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
            this.rdoCarrier2.Location = new System.Drawing.Point(247, 5);
            this.rdoCarrier2.Margin = new System.Windows.Forms.Padding(2, 5, 2, 5);
            this.rdoCarrier2.Name = "rdoCarrier2";
            this.rdoCarrier2.Size = new System.Drawing.Size(242, 68);
            this.rdoCarrier2.TabIndex = 14;
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
            this.rdoCarrier1.Location = new System.Drawing.Point(2, 5);
            this.rdoCarrier1.Margin = new System.Windows.Forms.Padding(2, 5, 2, 5);
            this.rdoCarrier1.Name = "rdoCarrier1";
            this.rdoCarrier1.Size = new System.Drawing.Size(241, 68);
            this.rdoCarrier1.TabIndex = 13;
            this.rdoCarrier1.TabStop = true;
            this.rdoCarrier1.Text = "載台 1";
            this.rdoCarrier1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rdoCarrier1.UseVisualStyleBackColor = true;
            // 
            // btnOpenMotorXY
            // 
            this.btnOpenMotorXY.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnOpenMotorXY.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnOpenMotorXY.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOpenMotorXY.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOpenMotorXY.ImageIndex = 0;
            this.btnOpenMotorXY.Location = new System.Drawing.Point(885, 6);
            this.btnOpenMotorXY.Margin = new System.Windows.Forms.Padding(2, 5, 2, 5);
            this.btnOpenMotorXY.Name = "btnOpenMotorXY";
            this.btnOpenMotorXY.Size = new System.Drawing.Size(95, 68);
            this.btnOpenMotorXY.TabIndex = 38;
            this.btnOpenMotorXY.Text = "軸控XY";
            this.btnOpenMotorXY.UseVisualStyleBackColor = false;
            // 
            // tbLayoutMain
            // 
            this.tbLayoutMain.ColumnCount = 2;
            this.tbLayoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tbLayoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tbLayoutMain.Controls.Add(this.jezTransImageViewPanel1, 0, 0);
            this.tbLayoutMain.Controls.Add(this.tbLayoutDockRight, 1, 0);
            this.tbLayoutMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbLayoutMain.Location = new System.Drawing.Point(1, 80);
            this.tbLayoutMain.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tbLayoutMain.Name = "tbLayoutMain";
            this.tbLayoutMain.RowCount = 1;
            this.tbLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tbLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tbLayoutMain.Size = new System.Drawing.Size(982, 680);
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
            this.tbLayoutDockRight.Location = new System.Drawing.Point(491, 0);
            this.tbLayoutDockRight.Margin = new System.Windows.Forms.Padding(0);
            this.tbLayoutDockRight.Name = "tbLayoutDockRight";
            this.tbLayoutDockRight.RowCount = 4;
            this.tbLayoutDockRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tbLayoutDockRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tbLayoutDockRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tbLayoutDockRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 64F));
            this.tbLayoutDockRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 16F));
            this.tbLayoutDockRight.Size = new System.Drawing.Size(491, 680);
            this.tbLayoutDockRight.TabIndex = 32;
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.WhiteSmoke;
            this.groupBox1.Controls.Add(this.gwPanePropsViewer1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(3, 237);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(6, 15, 6, 9);
            this.groupBox1.Size = new System.Drawing.Size(485, 376);
            this.groupBox1.TabIndex = 29;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "像測參數";
            // 
            // gwPanePropsViewer1
            // 
            this.gwPanePropsViewer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gwPanePropsViewer1.Editable = false;
            this.gwPanePropsViewer1.ImageList = null;
            this.gwPanePropsViewer1.Location = new System.Drawing.Point(6, 30);
            this.gwPanePropsViewer1.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.gwPanePropsViewer1.Name = "gwPanePropsViewer1";
            this.gwPanePropsViewer1.Size = new System.Drawing.Size(473, 337);
            this.gwPanePropsViewer1.TabIndex = 0;
            // 
            // panelB
            // 
            this.panelB.BackColor = System.Drawing.Color.LightSteelBlue;
            this.panelB.Controls.Add(this.btnCancel);
            this.panelB.Controls.Add(this.btnOK);
            this.panelB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelB.Location = new System.Drawing.Point(0, 616);
            this.panelB.Margin = new System.Windows.Forms.Padding(0);
            this.panelB.Name = "panelB";
            this.panelB.Size = new System.Drawing.Size(491, 64);
            this.panelB.TabIndex = 32;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnCancel.Font = new System.Drawing.Font("微软雅黑", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel.Location = new System.Drawing.Point(369, 9);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(111, 41);
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
            this.btnOK.Location = new System.Drawing.Point(252, 9);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(111, 41);
            this.btnOK.TabIndex = 23;
            this.btnOK.Text = "保存资料";
            this.btnOK.UseVisualStyleBackColor = false;
            // 
            // panelBtns
            // 
            this.panelBtns.BackColor = System.Drawing.Color.LightSteelBlue;
            this.panelBtns.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelBtns.Controls.Add(this.gvFocusSettingPanel1);
            this.panelBtns.Controls.Add(this.btnLoadImage);
            this.panelBtns.Controls.Add(this.btnGrabImage);
            this.panelBtns.Controls.Add(this.btnBuildCalib);
            this.panelBtns.Controls.Add(this.btnAutoFetchAll);
            this.panelBtns.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelBtns.Location = new System.Drawing.Point(3, 144);
            this.panelBtns.Name = "panelBtns";
            this.panelBtns.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
            this.panelBtns.Size = new System.Drawing.Size(485, 87);
            this.panelBtns.TabIndex = 28;
            this.panelBtns.Text = "像測操作";
            // 
            // gvFocusSettingPanel1
            // 
            this.gvFocusSettingPanel1.AxisName = "相機 對焦 Z (mm)";
            this.gvFocusSettingPanel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.gvFocusSettingPanel1.Location = new System.Drawing.Point(5, 9);
            this.gvFocusSettingPanel1.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.gvFocusSettingPanel1.Name = "gvFocusSettingPanel1";
            this.gvFocusSettingPanel1.Padding = new System.Windows.Forms.Padding(1, 1, 0, 3);
            this.gvFocusSettingPanel1.Size = new System.Drawing.Size(165, 67);
            this.gvFocusSettingPanel1.TabIndex = 29;
            // 
            // btnLoadImage
            // 
            this.btnLoadImage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnLoadImage.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnLoadImage.Font = new System.Drawing.Font("微软雅黑", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLoadImage.Location = new System.Drawing.Point(175, 44);
            this.btnLoadImage.Name = "btnLoadImage";
            this.btnLoadImage.Size = new System.Drawing.Size(90, 31);
            this.btnLoadImage.TabIndex = 22;
            this.btnLoadImage.Text = "加載圖片";
            this.btnLoadImage.UseVisualStyleBackColor = false;
            // 
            // btnGrabImage
            // 
            this.btnGrabImage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnGrabImage.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnGrabImage.Font = new System.Drawing.Font("微软雅黑", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnGrabImage.Location = new System.Drawing.Point(175, 9);
            this.btnGrabImage.Name = "btnGrabImage";
            this.btnGrabImage.Size = new System.Drawing.Size(90, 31);
            this.btnGrabImage.TabIndex = 21;
            this.btnGrabImage.Text = "取像";
            this.btnGrabImage.UseVisualStyleBackColor = false;
            // 
            // btnBuildCalib
            // 
            this.btnBuildCalib.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.btnBuildCalib.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnBuildCalib.Font = new System.Drawing.Font("微软雅黑", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnBuildCalib.Location = new System.Drawing.Point(367, 9);
            this.btnBuildCalib.Name = "btnBuildCalib";
            this.btnBuildCalib.Size = new System.Drawing.Size(90, 66);
            this.btnBuildCalib.TabIndex = 25;
            this.btnBuildCalib.Text = "執行校正";
            this.btnBuildCalib.UseVisualStyleBackColor = false;
            // 
            // btnAutoFetchAll
            // 
            this.btnAutoFetchAll.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.btnAutoFetchAll.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnAutoFetchAll.Font = new System.Drawing.Font("微软雅黑", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAutoFetchAll.Location = new System.Drawing.Point(270, 9);
            this.btnAutoFetchAll.Name = "btnAutoFetchAll";
            this.btnAutoFetchAll.Size = new System.Drawing.Size(90, 66);
            this.btnAutoFetchAll.TabIndex = 28;
            this.btnAutoFetchAll.Text = "自動抓取\r\n校正點";
            this.btnAutoFetchAll.UseVisualStyleBackColor = false;
            // 
            // jezTransImageViewPanel1
            // 
            this.jezTransImageViewPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.jezTransImageViewPanel1.Location = new System.Drawing.Point(0, 0);
            this.jezTransImageViewPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.jezTransImageViewPanel1.Name = "jezTransImageViewPanel1";
            this.jezTransImageViewPanel1.Size = new System.Drawing.Size(491, 680);
            this.jezTransImageViewPanel1.TabIndex = 33;
            // 
            // gvCalibPointsDataGridView1
            // 
            this.gvCalibPointsDataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gvCalibPointsDataGridView1.Font = new System.Drawing.Font("微软雅黑", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gvCalibPointsDataGridView1.Location = new System.Drawing.Point(2, 3);
            this.gvCalibPointsDataGridView1.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.gvCalibPointsDataGridView1.Name = "gvCalibPointsDataGridView1";
            this.gvCalibPointsDataGridView1.Padding = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.gvCalibPointsDataGridView1.SelectedIndex = 0;
            this.gvCalibPointsDataGridView1.Size = new System.Drawing.Size(487, 135);
            this.gvCalibPointsDataGridView1.TabIndex = 0;
            // 
            // FormCalibrationTool
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Gray;
            this.ClientSize = new System.Drawing.Size(984, 761);
            this.Controls.Add(this.tbLayoutMain);
            this.Controls.Add(this.tblayoutTopNav);
            this.Font = new System.Drawing.Font("新細明體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormCalibrationTool";
            this.Padding = new System.Windows.Forms.Padding(1, 0, 1, 1);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "校正設定";
            this.tblayoutTopNav.ResumeLayout(false);
            this.tableLayoutSuckers.ResumeLayout(false);
            this.tableLayoutCarriers.ResumeLayout(false);
            this.tbLayoutMain.ResumeLayout(false);
            this.tbLayoutDockRight.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.panelB.ResumeLayout(false);
            this.panelBtns.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TableLayoutPanel tblayoutTopNav;
        private System.Windows.Forms.TableLayoutPanel tbLayoutMain;
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
        private System.Windows.Forms.Button btnAutoFetchAll;
        private GwMotorSimpleGoPanel gvFocusSettingPanel1;
        private JezTransImageViewPanel jezTransImageViewPanel1;
        private System.Windows.Forms.Button btnOpenMotorXY;
        private System.Windows.Forms.TableLayoutPanel tableLayoutCarriers;
        private System.Windows.Forms.RadioButton rdoCarrier2;
        private System.Windows.Forms.RadioButton rdoCarrier1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutSuckers;
        private System.Windows.Forms.RadioButton rdoSucker2;
        private System.Windows.Forms.RadioButton rdoSucker1;
    }
}