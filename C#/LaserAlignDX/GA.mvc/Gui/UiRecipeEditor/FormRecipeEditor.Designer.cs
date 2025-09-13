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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormRecipeEditor));
            this.panelTop = new System.Windows.Forms.Panel();
            this.btnWriteCoordsToPlc = new System.Windows.Forms.Button();
            this.btnSaveImage = new System.Windows.Forms.Button();
            this.btnOpenFlyCamRcpWindow = new System.Windows.Forms.Button();
            this.btnOpenLightCtrlWindow = new System.Windows.Forms.Button();
            this.btnOpenEmptyTrayWindow = new System.Windows.Forms.Button();
            this.btnGrabImage = new System.Windows.Forms.Button();
            this.btnCreateCellRegions = new System.Windows.Forms.Button();
            this.rdoCarrier2 = new System.Windows.Forms.RadioButton();
            this.rdoCarrier1 = new System.Windows.Forms.RadioButton();
            this.btnOpenTemplateMatchWindow = new System.Windows.Forms.Button();
            this.btnPickGoldenRegion = new System.Windows.Forms.Button();
            this.btnLoadImage = new System.Windows.Forms.Button();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            this.tblayoutRight = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.tblayoutMajor = new System.Windows.Forms.TableLayoutPanel();
            this.jezTransImageViewPanel1 = new LaserAlignDX.Mvc.Gui.JezTransImageViewPanel();
            this.gvCalibPointsDataGridView1 = new LaserAlignDX.Mvc.Gui.GvCalibPointsDataGridView();
            this.panelTop.SuspendLayout();
            this.panelBottom.SuspendLayout();
            this.tblayoutRight.SuspendLayout();
            this.tblayoutMajor.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelTop
            // 
            this.panelTop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.panelTop.Controls.Add(this.gvCalibPointsDataGridView1);
            this.panelTop.Controls.Add(this.btnWriteCoordsToPlc);
            this.panelTop.Controls.Add(this.btnSaveImage);
            this.panelTop.Controls.Add(this.btnOpenFlyCamRcpWindow);
            this.panelTop.Controls.Add(this.btnOpenLightCtrlWindow);
            this.panelTop.Controls.Add(this.btnOpenEmptyTrayWindow);
            this.panelTop.Controls.Add(this.btnGrabImage);
            this.panelTop.Controls.Add(this.btnCreateCellRegions);
            this.panelTop.Controls.Add(this.rdoCarrier2);
            this.panelTop.Controls.Add(this.rdoCarrier1);
            this.panelTop.Controls.Add(this.btnOpenTemplateMatchWindow);
            this.panelTop.Controls.Add(this.btnPickGoldenRegion);
            this.panelTop.Controls.Add(this.btnLoadImage);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Margin = new System.Windows.Forms.Padding(4);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(1262, 148);
            this.panelTop.TabIndex = 6;
            // 
            // btnWriteCoordsToPlc
            // 
            this.btnWriteCoordsToPlc.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.btnWriteCoordsToPlc.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnWriteCoordsToPlc.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnWriteCoordsToPlc.Location = new System.Drawing.Point(601, 15);
            this.btnWriteCoordsToPlc.Margin = new System.Windows.Forms.Padding(4);
            this.btnWriteCoordsToPlc.Name = "btnWriteCoordsToPlc";
            this.btnWriteCoordsToPlc.Size = new System.Drawing.Size(115, 108);
            this.btnWriteCoordsToPlc.TabIndex = 37;
            this.btnWriteCoordsToPlc.Text = "將座標寫入\r\nPLC";
            this.btnWriteCoordsToPlc.UseVisualStyleBackColor = false;
            // 
            // btnSaveImage
            // 
            this.btnSaveImage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnSaveImage.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnSaveImage.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
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
            this.btnOpenFlyCamRcpWindow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnOpenFlyCamRcpWindow.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnOpenFlyCamRcpWindow.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOpenFlyCamRcpWindow.Location = new System.Drawing.Point(460, 54);
            this.btnOpenFlyCamRcpWindow.Margin = new System.Windows.Forms.Padding(4);
            this.btnOpenFlyCamRcpWindow.Name = "btnOpenFlyCamRcpWindow";
            this.btnOpenFlyCamRcpWindow.Size = new System.Drawing.Size(120, 31);
            this.btnOpenFlyCamRcpWindow.TabIndex = 35;
            this.btnOpenFlyCamRcpWindow.Text = "飞拍界面";
            this.btnOpenFlyCamRcpWindow.UseVisualStyleBackColor = false;
            // 
            // btnOpenLightCtrlWindow
            // 
            this.btnOpenLightCtrlWindow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.btnOpenLightCtrlWindow.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnOpenLightCtrlWindow.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOpenLightCtrlWindow.Location = new System.Drawing.Point(460, 92);
            this.btnOpenLightCtrlWindow.Margin = new System.Windows.Forms.Padding(4);
            this.btnOpenLightCtrlWindow.Name = "btnOpenLightCtrlWindow";
            this.btnOpenLightCtrlWindow.Size = new System.Drawing.Size(120, 31);
            this.btnOpenLightCtrlWindow.TabIndex = 34;
            this.btnOpenLightCtrlWindow.Text = "控制灯光";
            this.btnOpenLightCtrlWindow.UseVisualStyleBackColor = false;
            // 
            // btnOpenEmptyTrayWindow
            // 
            this.btnOpenEmptyTrayWindow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnOpenEmptyTrayWindow.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnOpenEmptyTrayWindow.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOpenEmptyTrayWindow.Location = new System.Drawing.Point(314, 15);
            this.btnOpenEmptyTrayWindow.Margin = new System.Windows.Forms.Padding(4);
            this.btnOpenEmptyTrayWindow.Name = "btnOpenEmptyTrayWindow";
            this.btnOpenEmptyTrayWindow.Size = new System.Drawing.Size(120, 31);
            this.btnOpenEmptyTrayWindow.TabIndex = 33;
            this.btnOpenEmptyTrayWindow.Text = "設定 空載台";
            this.btnOpenEmptyTrayWindow.UseVisualStyleBackColor = false;
            // 
            // btnGrabImage
            // 
            this.btnGrabImage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnGrabImage.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnGrabImage.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnGrabImage.Location = new System.Drawing.Point(16, 15);
            this.btnGrabImage.Margin = new System.Windows.Forms.Padding(4);
            this.btnGrabImage.Name = "btnGrabImage";
            this.btnGrabImage.Size = new System.Drawing.Size(120, 31);
            this.btnGrabImage.TabIndex = 32;
            this.btnGrabImage.Text = "取像";
            this.btnGrabImage.UseVisualStyleBackColor = false;
            // 
            // btnCreateCellRegions
            // 
            this.btnCreateCellRegions.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnCreateCellRegions.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnCreateCellRegions.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCreateCellRegions.Location = new System.Drawing.Point(314, 92);
            this.btnCreateCellRegions.Margin = new System.Windows.Forms.Padding(4);
            this.btnCreateCellRegions.Name = "btnCreateCellRegions";
            this.btnCreateCellRegions.Size = new System.Drawing.Size(120, 31);
            this.btnCreateCellRegions.TabIndex = 30;
            this.btnCreateCellRegions.Text = "自動 生成陣列";
            this.btnCreateCellRegions.UseVisualStyleBackColor = false;
            // 
            // rdoCarrier2
            // 
            this.rdoCarrier2.AutoSize = true;
            this.rdoCarrier2.Font = new System.Drawing.Font("微軟正黑體", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.rdoCarrier2.Location = new System.Drawing.Point(165, 74);
            this.rdoCarrier2.Margin = new System.Windows.Forms.Padding(4);
            this.rdoCarrier2.Name = "rdoCarrier2";
            this.rdoCarrier2.Size = new System.Drawing.Size(109, 40);
            this.rdoCarrier2.TabIndex = 28;
            this.rdoCarrier2.Text = "載台2";
            this.rdoCarrier2.UseVisualStyleBackColor = true;
            // 
            // rdoCarrier1
            // 
            this.rdoCarrier1.AutoSize = true;
            this.rdoCarrier1.Checked = true;
            this.rdoCarrier1.Font = new System.Drawing.Font("微軟正黑體", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.rdoCarrier1.Location = new System.Drawing.Point(165, 26);
            this.rdoCarrier1.Margin = new System.Windows.Forms.Padding(4);
            this.rdoCarrier1.Name = "rdoCarrier1";
            this.rdoCarrier1.Size = new System.Drawing.Size(109, 40);
            this.rdoCarrier1.TabIndex = 27;
            this.rdoCarrier1.TabStop = true;
            this.rdoCarrier1.Text = "載台1";
            this.rdoCarrier1.UseVisualStyleBackColor = true;
            // 
            // btnOpenTemplateMatchWindow
            // 
            this.btnOpenTemplateMatchWindow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnOpenTemplateMatchWindow.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnOpenTemplateMatchWindow.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOpenTemplateMatchWindow.Location = new System.Drawing.Point(460, 15);
            this.btnOpenTemplateMatchWindow.Margin = new System.Windows.Forms.Padding(4);
            this.btnOpenTemplateMatchWindow.Name = "btnOpenTemplateMatchWindow";
            this.btnOpenTemplateMatchWindow.Size = new System.Drawing.Size(120, 31);
            this.btnOpenTemplateMatchWindow.TabIndex = 26;
            this.btnOpenTemplateMatchWindow.Text = "模板界面";
            this.btnOpenTemplateMatchWindow.UseVisualStyleBackColor = false;
            // 
            // btnPickGoldenRegion
            // 
            this.btnPickGoldenRegion.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnPickGoldenRegion.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnPickGoldenRegion.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPickGoldenRegion.Location = new System.Drawing.Point(314, 54);
            this.btnPickGoldenRegion.Margin = new System.Windows.Forms.Padding(4);
            this.btnPickGoldenRegion.Name = "btnPickGoldenRegion";
            this.btnPickGoldenRegion.Size = new System.Drawing.Size(120, 31);
            this.btnPickGoldenRegion.TabIndex = 23;
            this.btnPickGoldenRegion.Text = "框選 晶粒區域";
            this.btnPickGoldenRegion.UseVisualStyleBackColor = false;
            // 
            // btnLoadImage
            // 
            this.btnLoadImage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnLoadImage.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnLoadImage.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLoadImage.Location = new System.Drawing.Point(16, 54);
            this.btnLoadImage.Margin = new System.Windows.Forms.Padding(4);
            this.btnLoadImage.Name = "btnLoadImage";
            this.btnLoadImage.Size = new System.Drawing.Size(120, 31);
            this.btnLoadImage.TabIndex = 22;
            this.btnLoadImage.Text = "加载图片";
            this.btnLoadImage.UseVisualStyleBackColor = false;
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.btnOK);
            this.panelBottom.Controls.Add(this.btnCancel);
            this.panelBottom.Location = new System.Drawing.Point(4, 733);
            this.panelBottom.Margin = new System.Windows.Forms.Padding(4);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(315, 86);
            this.panelBottom.TabIndex = 7;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnOK.Location = new System.Drawing.Point(14, 11);
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
            this.btnCancel.Location = new System.Drawing.Point(162, 11);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(140, 62);
            this.btnCancel.TabIndex = 16;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = false;
            // 
            // propertyGrid1
            // 
            this.propertyGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid1.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.propertyGrid1.Location = new System.Drawing.Point(4, 42);
            this.propertyGrid1.Margin = new System.Windows.Forms.Padding(4);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.Size = new System.Drawing.Size(315, 683);
            this.propertyGrid1.TabIndex = 0;
            // 
            // tblayoutRight
            // 
            this.tblayoutRight.BackColor = System.Drawing.Color.LightSteelBlue;
            this.tblayoutRight.ColumnCount = 1;
            this.tblayoutRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblayoutRight.Controls.Add(this.panelBottom, 0, 2);
            this.tblayoutRight.Controls.Add(this.propertyGrid1, 0, 1);
            this.tblayoutRight.Controls.Add(this.label1, 0, 0);
            this.tblayoutRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblayoutRight.Location = new System.Drawing.Point(936, 3);
            this.tblayoutRight.Name = "tblayoutRight";
            this.tblayoutRight.RowCount = 3;
            this.tblayoutRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.tblayoutRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblayoutRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tblayoutRight.Size = new System.Drawing.Size(323, 823);
            this.tblayoutRight.TabIndex = 9;
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Font = new System.Drawing.Font("微软雅黑", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.label1.Size = new System.Drawing.Size(317, 38);
            this.label1.TabIndex = 8;
            this.label1.Text = "參數設定";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tblayoutMajor
            // 
            this.tblayoutMajor.ColumnCount = 2;
            this.tblayoutMajor.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblayoutMajor.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tblayoutMajor.Controls.Add(this.jezTransImageViewPanel1, 0, 0);
            this.tblayoutMajor.Controls.Add(this.tblayoutRight, 1, 0);
            this.tblayoutMajor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblayoutMajor.Location = new System.Drawing.Point(0, 148);
            this.tblayoutMajor.Name = "tblayoutMajor";
            this.tblayoutMajor.RowCount = 1;
            this.tblayoutMajor.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblayoutMajor.Size = new System.Drawing.Size(1262, 829);
            this.tblayoutMajor.TabIndex = 10;
            // 
            // jezTransImageViewPanel1
            // 
            this.jezTransImageViewPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.jezTransImageViewPanel1.Location = new System.Drawing.Point(3, 4);
            this.jezTransImageViewPanel1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.jezTransImageViewPanel1.Name = "jezTransImageViewPanel1";
            this.jezTransImageViewPanel1.Size = new System.Drawing.Size(927, 821);
            this.jezTransImageViewPanel1.TabIndex = 0;
            // 
            // gvCalibPointsDataGridView1
            // 
            this.gvCalibPointsDataGridView1.Location = new System.Drawing.Point(734, 12);
            this.gvCalibPointsDataGridView1.Name = "gvCalibPointsDataGridView1";
            this.gvCalibPointsDataGridView1.SelectedIndex = 0;
            this.gvCalibPointsDataGridView1.Size = new System.Drawing.Size(516, 111);
            this.gvCalibPointsDataGridView1.TabIndex = 38;
            // 
            // FormRecipeEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1262, 977);
            this.Controls.Add(this.tblayoutMajor);
            this.Controls.Add(this.panelTop);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "FormRecipeEditor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "參數設定";
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.panelBottom.ResumeLayout(false);
            this.tblayoutRight.ResumeLayout(false);
            this.tblayoutMajor.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Button btnGrabImage;
        private System.Windows.Forms.Button btnCreateCellRegions;
        private System.Windows.Forms.RadioButton rdoCarrier2;
        private System.Windows.Forms.RadioButton rdoCarrier1;
        private System.Windows.Forms.Button btnOpenTemplateMatchWindow;
        private System.Windows.Forms.Button btnPickGoldenRegion;
        private System.Windows.Forms.Button btnLoadImage;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.PropertyGrid propertyGrid1;
        private System.Windows.Forms.Button btnOpenEmptyTrayWindow;
        private System.Windows.Forms.Button btnOpenLightCtrlWindow;
        private System.Windows.Forms.Button btnOpenFlyCamRcpWindow;
        private System.Windows.Forms.Button btnSaveImage;
        private JezTransImageViewPanel jezTransImageViewPanel1;
        private System.Windows.Forms.Button btnWriteCoordsToPlc;
        private GvCalibPointsDataGridView gvCalibPointsDataGridView1;
        private System.Windows.Forms.TableLayoutPanel tblayoutRight;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TableLayoutPanel tblayoutMajor;
    }
}