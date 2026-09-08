namespace LaserAlignDX.GA.mvc.Gui.UiRecipeEditor
{
    partial class GwRcpConnBlocBtnsPanel
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
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnClearAll = new System.Windows.Forms.Button();
            this.btnAdd = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblMinY = new System.Windows.Forms.Label();
            this.lblMinX = new System.Windows.Forms.Label();
            this.lblThreshold = new System.Windows.Forms.Label();
            this.numMinX = new System.Windows.Forms.NumericUpDown();
            this.numBadConnThres = new System.Windows.Forms.NumericUpDown();
            this.numMinY = new System.Windows.Forms.NumericUpDown();
            this.lblDetectRegions = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMinX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBadConnThres)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMinY)).BeginInit();
            this.SuspendLayout();
            // 
            // btnDelete
            // 
            this.btnDelete.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnDelete.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.btnDelete.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnDelete.Font = new System.Drawing.Font("Microsoft YaHei UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDelete.Location = new System.Drawing.Point(429, 47);
            this.btnDelete.Margin = new System.Windows.Forms.Padding(4);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(143, 54);
            this.btnDelete.TabIndex = 75;
            this.btnDelete.Text = "刪除";
            this.btnDelete.UseVisualStyleBackColor = false;
            // 
            // btnClearAll
            // 
            this.btnClearAll.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnClearAll.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.btnClearAll.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnClearAll.Font = new System.Drawing.Font("Microsoft YaHei UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClearAll.Location = new System.Drawing.Point(594, 47);
            this.btnClearAll.Margin = new System.Windows.Forms.Padding(4);
            this.btnClearAll.Name = "btnClearAll";
            this.btnClearAll.Size = new System.Drawing.Size(143, 54);
            this.btnClearAll.TabIndex = 74;
            this.btnClearAll.Text = "清空";
            this.btnClearAll.UseVisualStyleBackColor = false;
            // 
            // btnAdd
            // 
            this.btnAdd.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnAdd.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnAdd.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnAdd.Font = new System.Drawing.Font("Microsoft YaHei UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAdd.Location = new System.Drawing.Point(266, 47);
            this.btnAdd.Margin = new System.Windows.Forms.Padding(4);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(143, 54);
            this.btnAdd.TabIndex = 73;
            this.btnAdd.Text = "添加";
            this.btnAdd.UseVisualStyleBackColor = false;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lblMinY);
            this.groupBox1.Controls.Add(this.lblMinX);
            this.groupBox1.Controls.Add(this.lblThreshold);
            this.groupBox1.Controls.Add(this.numMinX);
            this.groupBox1.Controls.Add(this.numBadConnThres);
            this.groupBox1.Controls.Add(this.numMinY);
            this.groupBox1.Controls.Add(this.lblDetectRegions);
            this.groupBox1.Controls.Add(this.btnDelete);
            this.groupBox1.Controls.Add(this.btnClearAll);
            this.groupBox1.Controls.Add(this.btnAdd);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(8, 0);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(8, 0, 8, 8);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.groupBox1.Size = new System.Drawing.Size(1205, 128);
            this.groupBox1.TabIndex = 78;
            this.groupBox1.TabStop = false;
            // 
            // lblMinY
            // 
            this.lblMinY.AutoSize = true;
            this.lblMinY.Location = new System.Drawing.Point(770, 94);
            this.lblMinY.Name = "lblMinY";
            this.lblMinY.Size = new System.Drawing.Size(106, 20);
            this.lblMinY.TabIndex = 85;
            this.lblMinY.Text = "Min Y (pixels)";
            this.lblMinY.Visible = false;
            // 
            // lblMinX
            // 
            this.lblMinX.AutoSize = true;
            this.lblMinX.Location = new System.Drawing.Point(770, 64);
            this.lblMinX.Name = "lblMinX";
            this.lblMinX.Size = new System.Drawing.Size(107, 20);
            this.lblMinX.TabIndex = 84;
            this.lblMinX.Text = "Min X (pixels)";
            this.lblMinX.Visible = false;
            // 
            // lblThreshold
            // 
            this.lblThreshold.AutoSize = true;
            this.lblThreshold.Location = new System.Drawing.Point(770, 35);
            this.lblThreshold.Name = "lblThreshold";
            this.lblThreshold.Size = new System.Drawing.Size(82, 20);
            this.lblThreshold.TabIndex = 82;
            this.lblThreshold.Text = "Threshold";
            this.lblThreshold.Visible = false;
            // 
            // numMinX
            // 
            this.numMinX.Location = new System.Drawing.Point(910, 61);
            this.numMinX.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numMinX.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numMinX.Name = "numMinX";
            this.numMinX.Size = new System.Drawing.Size(95, 27);
            this.numMinX.TabIndex = 80;
            this.numMinX.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numMinX.Value = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.numMinX.Visible = false;
            // 
            // numBadConnThres
            // 
            this.numBadConnThres.Location = new System.Drawing.Point(910, 31);
            this.numBadConnThres.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numBadConnThres.Name = "numBadConnThres";
            this.numBadConnThres.Size = new System.Drawing.Size(95, 27);
            this.numBadConnThres.TabIndex = 81;
            this.numBadConnThres.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numBadConnThres.Value = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numBadConnThres.Visible = false;
            // 
            // numMinY
            // 
            this.numMinY.Location = new System.Drawing.Point(910, 90);
            this.numMinY.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numMinY.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numMinY.Name = "numMinY";
            this.numMinY.Size = new System.Drawing.Size(95, 27);
            this.numMinY.TabIndex = 83;
            this.numMinY.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numMinY.Value = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.numMinY.Visible = false;
            // 
            // lblDetectRegions
            // 
            this.lblDetectRegions.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblDetectRegions.AutoSize = true;
            this.lblDetectRegions.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDetectRegions.Location = new System.Drawing.Point(20, 61);
            this.lblDetectRegions.Name = "lblDetectRegions";
            this.lblDetectRegions.Size = new System.Drawing.Size(155, 27);
            this.lblDetectRegions.TabIndex = 79;
            this.lblDetectRegions.Text = "Detect Regions";
            // 
            // GwRcpConnBlocBtnsPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.Name = "GwRcpConnBlocBtnsPanel";
            this.Padding = new System.Windows.Forms.Padding(8, 0, 8, 2);
            this.Size = new System.Drawing.Size(1221, 130);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMinX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBadConnThres)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMinY)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.Button btnDelete;
        public System.Windows.Forms.Button btnClearAll;
        public System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lblDetectRegions;
        private System.Windows.Forms.Label lblMinY;
        private System.Windows.Forms.Label lblMinX;
        private System.Windows.Forms.Label lblThreshold;
        public System.Windows.Forms.NumericUpDown numMinX;
        public System.Windows.Forms.NumericUpDown numBadConnThres;
        public System.Windows.Forms.NumericUpDown numMinY;
    }
}
