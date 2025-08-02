namespace LaserAlignDX.FormSpace
{
    partial class frmMSR
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnVarFile = new System.Windows.Forms.Button();
            this.btnTestLaser = new System.Windows.Forms.Button();
            this.btnLaserBoard = new System.Windows.Forms.Button();
            this.btnLoadFileCali = new System.Windows.Forms.Button();
            this.btnCreateXml = new System.Windows.Forms.Button();
            this.btnLoadPointF = new System.Windows.Forms.Button();
            this.btnCreatePointF = new System.Windows.Forms.Button();
            this.btnRectYz = new System.Windows.Forms.Button();
            this.btnMarkCali = new System.Windows.Forms.Button();
            this.pgParas = new System.Windows.Forms.PropertyGrid();
            this.btnCalibrateTest = new System.Windows.Forms.Button();
            this.btnSaveCalibrateMsr = new System.Windows.Forms.Button();
            this.btnCalibrate = new System.Windows.Forms.Button();
            this.btnAutoFind = new System.Windows.Forms.Button();
            this.btnLoadImage = new System.Windows.Forms.Button();
            this.DS = new JzDisplay.UISpace.DispUI();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnVarFile);
            this.panel1.Controls.Add(this.btnTestLaser);
            this.panel1.Controls.Add(this.btnLaserBoard);
            this.panel1.Controls.Add(this.btnLoadFileCali);
            this.panel1.Controls.Add(this.btnCreateXml);
            this.panel1.Controls.Add(this.btnLoadPointF);
            this.panel1.Controls.Add(this.btnCreatePointF);
            this.panel1.Controls.Add(this.btnRectYz);
            this.panel1.Controls.Add(this.btnMarkCali);
            this.panel1.Controls.Add(this.pgParas);
            this.panel1.Controls.Add(this.btnCalibrateTest);
            this.panel1.Controls.Add(this.btnSaveCalibrateMsr);
            this.panel1.Controls.Add(this.btnCalibrate);
            this.panel1.Controls.Add(this.btnAutoFind);
            this.panel1.Controls.Add(this.btnLoadImage);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel1.Location = new System.Drawing.Point(802, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(228, 705);
            this.panel1.TabIndex = 0;
            // 
            // btnVarFile
            // 
            this.btnVarFile.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnVarFile.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnVarFile.Location = new System.Drawing.Point(117, 433);
            this.btnVarFile.Name = "btnVarFile";
            this.btnVarFile.Size = new System.Drawing.Size(105, 25);
            this.btnVarFile.TabIndex = 31;
            this.btnVarFile.Text = "验证校正档";
            this.btnVarFile.UseVisualStyleBackColor = false;
            // 
            // btnTestLaser
            // 
            this.btnTestLaser.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnTestLaser.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnTestLaser.Location = new System.Drawing.Point(117, 402);
            this.btnTestLaser.Name = "btnTestLaser";
            this.btnTestLaser.Size = new System.Drawing.Size(105, 25);
            this.btnTestLaser.TabIndex = 30;
            this.btnTestLaser.Text = "校正测试Laser";
            this.btnTestLaser.UseVisualStyleBackColor = false;
            // 
            // btnLaserBoard
            // 
            this.btnLaserBoard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnLaserBoard.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnLaserBoard.Location = new System.Drawing.Point(117, 371);
            this.btnLaserBoard.Name = "btnLaserBoard";
            this.btnLaserBoard.Size = new System.Drawing.Size(105, 25);
            this.btnLaserBoard.TabIndex = 29;
            this.btnLaserBoard.Text = "校正Laser板";
            this.btnLaserBoard.UseVisualStyleBackColor = false;
            // 
            // btnLoadFileCali
            // 
            this.btnLoadFileCali.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnLoadFileCali.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnLoadFileCali.Location = new System.Drawing.Point(117, 464);
            this.btnLoadFileCali.Name = "btnLoadFileCali";
            this.btnLoadFileCali.Size = new System.Drawing.Size(105, 25);
            this.btnLoadFileCali.TabIndex = 28;
            this.btnLoadFileCali.Text = "载入文档校正";
            this.btnLoadFileCali.UseVisualStyleBackColor = false;
            // 
            // btnCreateXml
            // 
            this.btnCreateXml.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnCreateXml.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnCreateXml.Location = new System.Drawing.Point(117, 677);
            this.btnCreateXml.Name = "btnCreateXml";
            this.btnCreateXml.Size = new System.Drawing.Size(105, 25);
            this.btnCreateXml.TabIndex = 27;
            this.btnCreateXml.Text = "生成xml";
            this.btnCreateXml.UseVisualStyleBackColor = false;
            // 
            // btnLoadPointF
            // 
            this.btnLoadPointF.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnLoadPointF.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnLoadPointF.Location = new System.Drawing.Point(117, 646);
            this.btnLoadPointF.Name = "btnLoadPointF";
            this.btnLoadPointF.Size = new System.Drawing.Size(105, 25);
            this.btnLoadPointF.TabIndex = 26;
            this.btnLoadPointF.Text = "加载坐标点档";
            this.btnLoadPointF.UseVisualStyleBackColor = false;
            // 
            // btnCreatePointF
            // 
            this.btnCreatePointF.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnCreatePointF.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnCreatePointF.Location = new System.Drawing.Point(117, 615);
            this.btnCreatePointF.Name = "btnCreatePointF";
            this.btnCreatePointF.Size = new System.Drawing.Size(105, 25);
            this.btnCreatePointF.TabIndex = 25;
            this.btnCreatePointF.Text = "生成坐标点";
            this.btnCreatePointF.UseVisualStyleBackColor = false;
            // 
            // btnRectYz
            // 
            this.btnRectYz.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnRectYz.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnRectYz.Location = new System.Drawing.Point(117, 584);
            this.btnRectYz.Name = "btnRectYz";
            this.btnRectYz.Size = new System.Drawing.Size(105, 25);
            this.btnRectYz.TabIndex = 24;
            this.btnRectYz.Text = "矩形验证";
            this.btnRectYz.UseVisualStyleBackColor = false;
            this.btnRectYz.Click += new System.EventHandler(this.btnRectYz_Click);
            // 
            // btnMarkCali
            // 
            this.btnMarkCali.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnMarkCali.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnMarkCali.Location = new System.Drawing.Point(6, 402);
            this.btnMarkCali.Name = "btnMarkCali";
            this.btnMarkCali.Size = new System.Drawing.Size(105, 25);
            this.btnMarkCali.TabIndex = 23;
            this.btnMarkCali.Text = "固定点校正";
            this.btnMarkCali.UseVisualStyleBackColor = false;
            this.btnMarkCali.Click += new System.EventHandler(this.btnMarkCali_Click);
            // 
            // pgParas
            // 
            this.pgParas.Dock = System.Windows.Forms.DockStyle.Top;
            this.pgParas.Location = new System.Drawing.Point(0, 0);
            this.pgParas.Name = "pgParas";
            this.pgParas.Size = new System.Drawing.Size(228, 365);
            this.pgParas.TabIndex = 22;
            // 
            // btnCalibrateTest
            // 
            this.btnCalibrateTest.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnCalibrateTest.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnCalibrateTest.Location = new System.Drawing.Point(6, 526);
            this.btnCalibrateTest.Name = "btnCalibrateTest";
            this.btnCalibrateTest.Size = new System.Drawing.Size(105, 25);
            this.btnCalibrateTest.TabIndex = 21;
            this.btnCalibrateTest.Text = "校正测试";
            this.btnCalibrateTest.UseVisualStyleBackColor = false;
            // 
            // btnSaveCalibrateMsr
            // 
            this.btnSaveCalibrateMsr.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnSaveCalibrateMsr.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnSaveCalibrateMsr.Location = new System.Drawing.Point(6, 495);
            this.btnSaveCalibrateMsr.Name = "btnSaveCalibrateMsr";
            this.btnSaveCalibrateMsr.Size = new System.Drawing.Size(105, 25);
            this.btnSaveCalibrateMsr.TabIndex = 20;
            this.btnSaveCalibrateMsr.Text = "保存校正档";
            this.btnSaveCalibrateMsr.UseVisualStyleBackColor = false;
            // 
            // btnCalibrate
            // 
            this.btnCalibrate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnCalibrate.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnCalibrate.Location = new System.Drawing.Point(6, 464);
            this.btnCalibrate.Name = "btnCalibrate";
            this.btnCalibrate.Size = new System.Drawing.Size(105, 25);
            this.btnCalibrate.TabIndex = 19;
            this.btnCalibrate.Text = "校正标定板";
            this.btnCalibrate.UseVisualStyleBackColor = false;
            // 
            // btnAutoFind
            // 
            this.btnAutoFind.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnAutoFind.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnAutoFind.Location = new System.Drawing.Point(6, 433);
            this.btnAutoFind.Name = "btnAutoFind";
            this.btnAutoFind.Size = new System.Drawing.Size(105, 25);
            this.btnAutoFind.TabIndex = 18;
            this.btnAutoFind.Text = "自动寻找";
            this.btnAutoFind.UseVisualStyleBackColor = false;
            // 
            // btnLoadImage
            // 
            this.btnLoadImage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnLoadImage.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnLoadImage.Location = new System.Drawing.Point(6, 371);
            this.btnLoadImage.Name = "btnLoadImage";
            this.btnLoadImage.Size = new System.Drawing.Size(105, 25);
            this.btnLoadImage.TabIndex = 17;
            this.btnLoadImage.Text = "载入图片";
            this.btnLoadImage.UseVisualStyleBackColor = false;
            // 
            // DS
            // 
            this.DS.Cursor = System.Windows.Forms.Cursors.Default;
            this.DS.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DS.Location = new System.Drawing.Point(0, 0);
            this.DS.Name = "DS";
            this.DS.Size = new System.Drawing.Size(802, 705);
            this.DS.TabIndex = 1;
            // 
            // frmMSR
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1030, 705);
            this.Controls.Add(this.DS);
            this.Controls.Add(this.panel1);
            this.Name = "frmMSR";
            this.Text = "frmMSR";
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private JzDisplay.UISpace.DispUI DS;
        private System.Windows.Forms.PropertyGrid pgParas;
        private System.Windows.Forms.Button btnCalibrateTest;
        private System.Windows.Forms.Button btnSaveCalibrateMsr;
        private System.Windows.Forms.Button btnCalibrate;
        private System.Windows.Forms.Button btnAutoFind;
        private System.Windows.Forms.Button btnLoadImage;
        private System.Windows.Forms.Button btnMarkCali;
        private System.Windows.Forms.Button btnRectYz;
        private System.Windows.Forms.Button btnCreatePointF;
        private System.Windows.Forms.Button btnCreateXml;
        private System.Windows.Forms.Button btnLoadPointF;
        private System.Windows.Forms.Button btnLoadFileCali;
        private System.Windows.Forms.Button btnLaserBoard;
        private System.Windows.Forms.Button btnTestLaser;
        private System.Windows.Forms.Button btnVarFile;
    }
}