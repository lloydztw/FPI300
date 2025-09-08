namespace LaserAlignDX.Mvc.Gui
{
    partial class FormRecipeEditor
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
            this.pnlTop = new System.Windows.Forms.Panel();
            this.btnSaveImage = new System.Windows.Forms.Button();
            this.btnOpenFlyCamRcpWindow = new System.Windows.Forms.Button();
            this.btnOpenLightCtrlWindow = new System.Windows.Forms.Button();
            this.btnOpenEmptyTrayWindow = new System.Windows.Forms.Button();
            this.btnGrabImage = new System.Windows.Forms.Button();
            this.rdoMeasureNoTray = new System.Windows.Forms.RadioButton();
            this.btnCreateCellRegions = new System.Windows.Forms.Button();
            this.rdoMeasureBarcode = new System.Windows.Forms.RadioButton();
            this.rdoMeasureAOI = new System.Windows.Forms.RadioButton();
            this.btnOpenTemplateMatchWindow = new System.Windows.Forms.Button();
            this.btnPickGoldenRegion = new System.Windows.Forms.Button();
            this.btnLoadImage = new System.Windows.Forms.Button();
            this.pnlBottom = new System.Windows.Forms.Panel();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            this.tabControl2 = new System.Windows.Forms.TabControl();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.jezTransImageViewPanel1 = new LaserAlignDX.Mvc.Gui.JezTransImageViewPanel();
            this.pnlTop.SuspendLayout();
            this.pnlBottom.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabControl2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlTop
            // 
            this.pnlTop.Controls.Add(this.btnSaveImage);
            this.pnlTop.Controls.Add(this.btnOpenFlyCamRcpWindow);
            this.pnlTop.Controls.Add(this.btnOpenLightCtrlWindow);
            this.pnlTop.Controls.Add(this.btnOpenEmptyTrayWindow);
            this.pnlTop.Controls.Add(this.btnGrabImage);
            this.pnlTop.Controls.Add(this.rdoMeasureNoTray);
            this.pnlTop.Controls.Add(this.btnCreateCellRegions);
            this.pnlTop.Controls.Add(this.rdoMeasureBarcode);
            this.pnlTop.Controls.Add(this.rdoMeasureAOI);
            this.pnlTop.Controls.Add(this.btnOpenTemplateMatchWindow);
            this.pnlTop.Controls.Add(this.btnPickGoldenRegion);
            this.pnlTop.Controls.Add(this.btnLoadImage);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Location = new System.Drawing.Point(0, 0);
            this.pnlTop.Margin = new System.Windows.Forms.Padding(4);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Size = new System.Drawing.Size(1365, 156);
            this.pnlTop.TabIndex = 6;
            // 
            // btnSaveImage
            // 
            this.btnSaveImage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnSaveImage.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnSaveImage.Location = new System.Drawing.Point(16, 92);
            this.btnSaveImage.Margin = new System.Windows.Forms.Padding(4);
            this.btnSaveImage.Name = "btnSaveImage";
            this.btnSaveImage.Size = new System.Drawing.Size(120, 31);
            this.btnSaveImage.TabIndex = 36;
            this.btnSaveImage.Text = "另存图片";
            this.btnSaveImage.UseVisualStyleBackColor = false;
            // 
            // btnOpenFlyCamRcpWindow
            // 
            this.btnOpenFlyCamRcpWindow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnOpenFlyCamRcpWindow.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnOpenFlyCamRcpWindow.Location = new System.Drawing.Point(551, 92);
            this.btnOpenFlyCamRcpWindow.Margin = new System.Windows.Forms.Padding(4);
            this.btnOpenFlyCamRcpWindow.Name = "btnOpenFlyCamRcpWindow";
            this.btnOpenFlyCamRcpWindow.Size = new System.Drawing.Size(120, 31);
            this.btnOpenFlyCamRcpWindow.TabIndex = 35;
            this.btnOpenFlyCamRcpWindow.Text = "进入飞拍界面";
            this.btnOpenFlyCamRcpWindow.UseVisualStyleBackColor = false;
            // 
            // btnOpenLightCtrlWindow
            // 
            this.btnOpenLightCtrlWindow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnOpenLightCtrlWindow.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnOpenLightCtrlWindow.Location = new System.Drawing.Point(765, 15);
            this.btnOpenLightCtrlWindow.Margin = new System.Windows.Forms.Padding(4);
            this.btnOpenLightCtrlWindow.Name = "btnOpenLightCtrlWindow";
            this.btnOpenLightCtrlWindow.Size = new System.Drawing.Size(120, 31);
            this.btnOpenLightCtrlWindow.TabIndex = 34;
            this.btnOpenLightCtrlWindow.Text = "控制灯光";
            this.btnOpenLightCtrlWindow.UseVisualStyleBackColor = false;
            // 
            // btnOpenEmptyTrayWindow
            // 
            this.btnOpenEmptyTrayWindow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnOpenEmptyTrayWindow.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnOpenEmptyTrayWindow.Location = new System.Drawing.Point(551, 54);
            this.btnOpenEmptyTrayWindow.Margin = new System.Windows.Forms.Padding(4);
            this.btnOpenEmptyTrayWindow.Name = "btnOpenEmptyTrayWindow";
            this.btnOpenEmptyTrayWindow.Size = new System.Drawing.Size(120, 31);
            this.btnOpenEmptyTrayWindow.TabIndex = 33;
            this.btnOpenEmptyTrayWindow.Text = "进入空载台";
            this.btnOpenEmptyTrayWindow.UseVisualStyleBackColor = false;
            // 
            // btnGrabImage
            // 
            this.btnGrabImage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnGrabImage.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnGrabImage.Location = new System.Drawing.Point(16, 15);
            this.btnGrabImage.Margin = new System.Windows.Forms.Padding(4);
            this.btnGrabImage.Name = "btnGrabImage";
            this.btnGrabImage.Size = new System.Drawing.Size(120, 31);
            this.btnGrabImage.TabIndex = 32;
            this.btnGrabImage.Text = "取像";
            this.btnGrabImage.UseVisualStyleBackColor = false;
            // 
            // rdoMeasureNoTray
            // 
            this.rdoMeasureNoTray.AutoSize = true;
            this.rdoMeasureNoTray.Location = new System.Drawing.Point(159, 72);
            this.rdoMeasureNoTray.Margin = new System.Windows.Forms.Padding(4);
            this.rdoMeasureNoTray.Name = "rdoMeasureNoTray";
            this.rdoMeasureNoTray.Size = new System.Drawing.Size(73, 19);
            this.rdoMeasureNoTray.TabIndex = 31;
            this.rdoMeasureNoTray.Text = "空载台";
            this.rdoMeasureNoTray.UseVisualStyleBackColor = true;
            this.rdoMeasureNoTray.Visible = false;
            // 
            // btnCreateCellRegions
            // 
            this.btnCreateCellRegions.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnCreateCellRegions.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnCreateCellRegions.Location = new System.Drawing.Point(348, 54);
            this.btnCreateCellRegions.Margin = new System.Windows.Forms.Padding(4);
            this.btnCreateCellRegions.Name = "btnCreateCellRegions";
            this.btnCreateCellRegions.Size = new System.Drawing.Size(120, 31);
            this.btnCreateCellRegions.TabIndex = 30;
            this.btnCreateCellRegions.Text = "生成阵列";
            this.btnCreateCellRegions.UseVisualStyleBackColor = false;
            // 
            // rdoMeasureBarcode
            // 
            this.rdoMeasureBarcode.AutoSize = true;
            this.rdoMeasureBarcode.Location = new System.Drawing.Point(159, 46);
            this.rdoMeasureBarcode.Margin = new System.Windows.Forms.Padding(4);
            this.rdoMeasureBarcode.Name = "rdoMeasureBarcode";
            this.rdoMeasureBarcode.Size = new System.Drawing.Size(58, 19);
            this.rdoMeasureBarcode.TabIndex = 28;
            this.rdoMeasureBarcode.Text = "读码";
            this.rdoMeasureBarcode.UseVisualStyleBackColor = true;
            this.rdoMeasureBarcode.Visible = false;
            // 
            // rdoMeasureAOI
            // 
            this.rdoMeasureAOI.AutoSize = true;
            this.rdoMeasureAOI.Checked = true;
            this.rdoMeasureAOI.Location = new System.Drawing.Point(159, 20);
            this.rdoMeasureAOI.Margin = new System.Windows.Forms.Padding(4);
            this.rdoMeasureAOI.Name = "rdoMeasureAOI";
            this.rdoMeasureAOI.Size = new System.Drawing.Size(133, 19);
            this.rdoMeasureAOI.TabIndex = 27;
            this.rdoMeasureAOI.TabStop = true;
            this.rdoMeasureAOI.Text = "外观及尺寸检测";
            this.rdoMeasureAOI.UseVisualStyleBackColor = true;
            // 
            // btnOpenTemplateMatchWindow
            // 
            this.btnOpenTemplateMatchWindow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnOpenTemplateMatchWindow.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnOpenTemplateMatchWindow.Location = new System.Drawing.Point(551, 15);
            this.btnOpenTemplateMatchWindow.Margin = new System.Windows.Forms.Padding(4);
            this.btnOpenTemplateMatchWindow.Name = "btnOpenTemplateMatchWindow";
            this.btnOpenTemplateMatchWindow.Size = new System.Drawing.Size(120, 31);
            this.btnOpenTemplateMatchWindow.TabIndex = 26;
            this.btnOpenTemplateMatchWindow.Text = "进入模板界面";
            this.btnOpenTemplateMatchWindow.UseVisualStyleBackColor = false;
            // 
            // btnPickGoldenRegion
            // 
            this.btnPickGoldenRegion.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnPickGoldenRegion.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnPickGoldenRegion.Location = new System.Drawing.Point(348, 15);
            this.btnPickGoldenRegion.Margin = new System.Windows.Forms.Padding(4);
            this.btnPickGoldenRegion.Name = "btnPickGoldenRegion";
            this.btnPickGoldenRegion.Size = new System.Drawing.Size(120, 31);
            this.btnPickGoldenRegion.TabIndex = 23;
            this.btnPickGoldenRegion.Text = "框选特征";
            this.btnPickGoldenRegion.UseVisualStyleBackColor = false;
            // 
            // btnLoadImage
            // 
            this.btnLoadImage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnLoadImage.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnLoadImage.Location = new System.Drawing.Point(16, 54);
            this.btnLoadImage.Margin = new System.Windows.Forms.Padding(4);
            this.btnLoadImage.Name = "btnLoadImage";
            this.btnLoadImage.Size = new System.Drawing.Size(120, 31);
            this.btnLoadImage.TabIndex = 22;
            this.btnLoadImage.Text = "加载图片";
            this.btnLoadImage.UseVisualStyleBackColor = false;
            // 
            // pnlBottom
            // 
            this.pnlBottom.Controls.Add(this.btnOK);
            this.pnlBottom.Controls.Add(this.btnCancel);
            this.pnlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlBottom.Location = new System.Drawing.Point(0, 849);
            this.pnlBottom.Margin = new System.Windows.Forms.Padding(4);
            this.pnlBottom.Name = "pnlBottom";
            this.pnlBottom.Size = new System.Drawing.Size(1365, 151);
            this.pnlBottom.TabIndex = 7;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnOK.Location = new System.Drawing.Point(1064, 76);
            this.btnOK.Margin = new System.Windows.Forms.Padding(4);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(140, 62);
            this.btnOK.TabIndex = 15;
            this.btnOK.Text = "確定";
            this.btnOK.UseVisualStyleBackColor = false;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnCancel.Location = new System.Drawing.Point(1212, 76);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(140, 62);
            this.btnCancel.TabIndex = 16;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = false;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Right;
            this.tabControl1.Location = new System.Drawing.Point(930, 156);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(4);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(435, 693);
            this.tabControl1.TabIndex = 8;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.propertyGrid1);
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage1.Size = new System.Drawing.Size(427, 664);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "参数设定";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // propertyGrid1
            // 
            this.propertyGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid1.Location = new System.Drawing.Point(4, 4);
            this.propertyGrid1.Margin = new System.Windows.Forms.Padding(4);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.Size = new System.Drawing.Size(419, 656);
            this.propertyGrid1.TabIndex = 0;
            // 
            // tabControl2
            // 
            this.tabControl2.Controls.Add(this.tabPage3);
            this.tabControl2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl2.Location = new System.Drawing.Point(0, 156);
            this.tabControl2.Margin = new System.Windows.Forms.Padding(4);
            this.tabControl2.Name = "tabControl2";
            this.tabControl2.SelectedIndex = 0;
            this.tabControl2.Size = new System.Drawing.Size(930, 693);
            this.tabControl2.TabIndex = 9;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.jezTransImageViewPanel1);
            this.tabPage3.Location = new System.Drawing.Point(4, 25);
            this.tabPage3.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(922, 664);
            this.tabPage3.TabIndex = 0;
            this.tabPage3.Text = "外观及尺寸";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // jezTransImageViewPanel1
            // 
            this.jezTransImageViewPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.jezTransImageViewPanel1.Location = new System.Drawing.Point(0, 0);
            this.jezTransImageViewPanel1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.jezTransImageViewPanel1.Name = "jezTransImageViewPanel1";
            this.jezTransImageViewPanel1.Size = new System.Drawing.Size(922, 664);
            this.jezTransImageViewPanel1.TabIndex = 0;
            // 
            // FormRecipeEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1365, 1000);
            this.Controls.Add(this.tabControl2);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.pnlBottom);
            this.Controls.Add(this.pnlTop);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "FormRecipeEditor";
            this.Text = "frmFPIRecipe";
            this.pnlTop.ResumeLayout(false);
            this.pnlTop.PerformLayout();
            this.pnlBottom.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabControl2.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlTop;
        private System.Windows.Forms.Button btnGrabImage;
        private System.Windows.Forms.RadioButton rdoMeasureNoTray;
        private System.Windows.Forms.Button btnCreateCellRegions;
        private System.Windows.Forms.RadioButton rdoMeasureBarcode;
        private System.Windows.Forms.RadioButton rdoMeasureAOI;
        private System.Windows.Forms.Button btnOpenTemplateMatchWindow;
        private System.Windows.Forms.Button btnPickGoldenRegion;
        private System.Windows.Forms.Button btnLoadImage;
        private System.Windows.Forms.Panel pnlBottom;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.PropertyGrid propertyGrid1;
        private System.Windows.Forms.TabControl tabControl2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Button btnOpenEmptyTrayWindow;
        private System.Windows.Forms.Button btnOpenLightCtrlWindow;
        private System.Windows.Forms.Button btnOpenFlyCamRcpWindow;
        private System.Windows.Forms.Button btnSaveImage;
        private JezTransImageViewPanel jezTransImageViewPanel1;
    }
}