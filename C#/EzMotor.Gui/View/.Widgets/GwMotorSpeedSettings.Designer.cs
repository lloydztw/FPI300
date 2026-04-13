namespace AX.Gui
{
    partial class GwMotorSpeedSettings
    {
        /// <summary> 
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置 Managed 資源則為 true，否則為 false。</param>
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
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器
        /// 修改這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.numHomeLow = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.numHomeHigh = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.numLowSpeed = new System.Windows.Forms.NumericUpDown();
            this.label13 = new System.Windows.Forms.Label();
            this.numHighSpeed = new System.Windows.Forms.NumericUpDown();
            this.label12 = new System.Windows.Forms.Label();
            this.groupBox7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numHomeLow)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numHomeHigh)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLowSpeed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numHighSpeed)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox7
            // 
            this.groupBox7.BackColor = System.Drawing.Color.Transparent;
            this.groupBox7.Controls.Add(this.label3);
            this.groupBox7.Controls.Add(this.numHomeLow);
            this.groupBox7.Controls.Add(this.label4);
            this.groupBox7.Controls.Add(this.numHomeHigh);
            this.groupBox7.Controls.Add(this.label6);
            this.groupBox7.Controls.Add(this.label2);
            this.groupBox7.Controls.Add(this.numLowSpeed);
            this.groupBox7.Controls.Add(this.label13);
            this.groupBox7.Controls.Add(this.numHighSpeed);
            this.groupBox7.Controls.Add(this.label12);
            this.groupBox7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox7.Font = new System.Drawing.Font("Verdana", 9.163636F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox7.ForeColor = System.Drawing.Color.White;
            this.groupBox7.Location = new System.Drawing.Point(0, 0);
            this.groupBox7.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox7.Size = new System.Drawing.Size(459, 110);
            this.groupBox7.TabIndex = 321;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "X Axis Speed (mm/s)";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.label3.Location = new System.Drawing.Point(25, 68);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(64, 18);
            this.label3.TabIndex = 329;
            this.label3.Text = "Home :";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // numHomeLow
            // 
            this.numHomeLow.BackColor = System.Drawing.Color.White;
            this.numHomeLow.DecimalPlaces = 2;
            this.numHomeLow.ForeColor = System.Drawing.Color.Black;
            this.numHomeLow.Increment = new decimal(new int[] {
            5,
            0,
            0,
            131072});
            this.numHomeLow.Location = new System.Drawing.Point(336, 64);
            this.numHomeLow.Margin = new System.Windows.Forms.Padding(4);
            this.numHomeLow.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numHomeLow.Name = "numHomeLow";
            this.numHomeLow.Size = new System.Drawing.Size(97, 26);
            this.numHomeLow.TabIndex = 328;
            this.numHomeLow.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numHomeLow.Value = new decimal(new int[] {
            9,
            0,
            0,
            0});
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.label4.Location = new System.Drawing.Point(277, 66);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(59, 18);
            this.label4.TabIndex = 327;
            this.label4.Text = "Low =";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // numHomeHigh
            // 
            this.numHomeHigh.BackColor = System.Drawing.Color.White;
            this.numHomeHigh.DecimalPlaces = 1;
            this.numHomeHigh.ForeColor = System.Drawing.Color.Black;
            this.numHomeHigh.Location = new System.Drawing.Point(167, 64);
            this.numHomeHigh.Margin = new System.Windows.Forms.Padding(4);
            this.numHomeHigh.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numHomeHigh.Name = "numHomeHigh";
            this.numHomeHigh.Size = new System.Drawing.Size(97, 26);
            this.numHomeHigh.TabIndex = 326;
            this.numHomeHigh.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numHomeHigh.Value = new decimal(new int[] {
            9,
            0,
            0,
            0});
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.label6.Location = new System.Drawing.Point(103, 66);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(64, 18);
            this.label6.TabIndex = 325;
            this.label6.Text = "High =";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.label2.Location = new System.Drawing.Point(25, 34);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(60, 18);
            this.label2.TabIndex = 324;
            this.label2.Text = "Move :";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // numLowSpeed
            // 
            this.numLowSpeed.BackColor = System.Drawing.Color.White;
            this.numLowSpeed.DecimalPlaces = 2;
            this.numLowSpeed.ForeColor = System.Drawing.Color.Black;
            this.numLowSpeed.Increment = new decimal(new int[] {
            5,
            0,
            0,
            131072});
            this.numLowSpeed.Location = new System.Drawing.Point(336, 30);
            this.numLowSpeed.Margin = new System.Windows.Forms.Padding(4);
            this.numLowSpeed.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numLowSpeed.Name = "numLowSpeed";
            this.numLowSpeed.Size = new System.Drawing.Size(97, 26);
            this.numLowSpeed.TabIndex = 80;
            this.numLowSpeed.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numLowSpeed.Value = new decimal(new int[] {
            9,
            0,
            0,
            0});
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.label13.Location = new System.Drawing.Point(277, 32);
            this.label13.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(59, 18);
            this.label13.TabIndex = 79;
            this.label13.Text = "Low =";
            this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // numHighSpeed
            // 
            this.numHighSpeed.BackColor = System.Drawing.Color.White;
            this.numHighSpeed.DecimalPlaces = 1;
            this.numHighSpeed.ForeColor = System.Drawing.Color.Black;
            this.numHighSpeed.Location = new System.Drawing.Point(167, 30);
            this.numHighSpeed.Margin = new System.Windows.Forms.Padding(4);
            this.numHighSpeed.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numHighSpeed.Name = "numHighSpeed";
            this.numHighSpeed.Size = new System.Drawing.Size(97, 26);
            this.numHighSpeed.TabIndex = 78;
            this.numHighSpeed.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numHighSpeed.Value = new decimal(new int[] {
            9,
            0,
            0,
            0});
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.label12.Location = new System.Drawing.Point(103, 32);
            this.label12.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(64, 18);
            this.label12.TabIndex = 77;
            this.label12.Text = "High =";
            this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // GwMotorSpeedSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Gray;
            this.Controls.Add(this.groupBox7);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "GwMotorSpeedSettings";
            this.Size = new System.Drawing.Size(459, 110);
            this.groupBox7.ResumeLayout(false);
            this.groupBox7.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numHomeLow)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numHomeHigh)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLowSpeed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numHighSpeed)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.Label label3;
        public System.Windows.Forms.NumericUpDown numHomeLow;
        private System.Windows.Forms.Label label4;
        public System.Windows.Forms.NumericUpDown numHomeHigh;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.NumericUpDown numLowSpeed;
        private System.Windows.Forms.Label label13;
        public System.Windows.Forms.NumericUpDown numHighSpeed;
        private System.Windows.Forms.Label label12;
    }
}
