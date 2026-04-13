namespace AX.Gui
{
    partial class GwSpeedHiLoButtons
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
            this.lblSpeed = new System.Windows.Forms.Label();
            this.rdoSpeedModeHI = new System.Windows.Forms.RadioButton();
            this.rdoSpeedModeLO = new System.Windows.Forms.RadioButton();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.btnConfig = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblSpeed
            // 
            this.lblSpeed.BackColor = System.Drawing.Color.Black;
            this.lblSpeed.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblSpeed.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSpeed.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSpeed.ForeColor = System.Drawing.Color.White;
            this.lblSpeed.Location = new System.Drawing.Point(4, 1);
            this.lblSpeed.Margin = new System.Windows.Forms.Padding(4, 1, 4, 3);
            this.lblSpeed.Name = "lblSpeed";
            this.lblSpeed.Size = new System.Drawing.Size(85, 21);
            this.lblSpeed.TabIndex = 327;
            this.lblSpeed.Text = "160";
            this.lblSpeed.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // rdoSpeedModeHI
            // 
            this.rdoSpeedModeHI.Appearance = System.Windows.Forms.Appearance.Button;
            this.rdoSpeedModeHI.BackColor = System.Drawing.SystemColors.Control;
            this.rdoSpeedModeHI.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.rdoSpeedModeHI.Checked = true;
            this.rdoSpeedModeHI.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rdoSpeedModeHI.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.rdoSpeedModeHI.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.rdoSpeedModeHI.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.rdoSpeedModeHI.ForeColor = System.Drawing.Color.Black;
            this.rdoSpeedModeHI.Location = new System.Drawing.Point(4, 27);
            this.rdoSpeedModeHI.Margin = new System.Windows.Forms.Padding(4, 2, 4, 1);
            this.rdoSpeedModeHI.Name = "rdoSpeedModeHI";
            this.rdoSpeedModeHI.Size = new System.Drawing.Size(85, 47);
            this.rdoSpeedModeHI.TabIndex = 324;
            this.rdoSpeedModeHI.TabStop = true;
            this.rdoSpeedModeHI.Text = "High";
            this.rdoSpeedModeHI.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rdoSpeedModeHI.UseVisualStyleBackColor = false;
            // 
            // rdoSpeedModeLO
            // 
            this.rdoSpeedModeLO.Appearance = System.Windows.Forms.Appearance.Button;
            this.rdoSpeedModeLO.BackColor = System.Drawing.SystemColors.Control;
            this.rdoSpeedModeLO.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.rdoSpeedModeLO.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rdoSpeedModeLO.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.rdoSpeedModeLO.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.rdoSpeedModeLO.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.rdoSpeedModeLO.ForeColor = System.Drawing.Color.Black;
            this.rdoSpeedModeLO.Location = new System.Drawing.Point(4, 76);
            this.rdoSpeedModeLO.Margin = new System.Windows.Forms.Padding(4, 1, 4, 1);
            this.rdoSpeedModeLO.Name = "rdoSpeedModeLO";
            this.rdoSpeedModeLO.Size = new System.Drawing.Size(85, 48);
            this.rdoSpeedModeLO.TabIndex = 325;
            this.rdoSpeedModeLO.Text = "Low";
            this.rdoSpeedModeLO.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rdoSpeedModeLO.UseVisualStyleBackColor = false;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.btnConfig, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.rdoSpeedModeHI, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.rdoSpeedModeLO, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.lblSpeed, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(93, 168);
            this.tableLayoutPanel1.TabIndex = 328;
            // 
            // btnConfig
            // 
            this.btnConfig.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnConfig.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnConfig.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnConfig.ForeColor = System.Drawing.Color.Black;
            this.btnConfig.Location = new System.Drawing.Point(4, 126);
            this.btnConfig.Margin = new System.Windows.Forms.Padding(4, 1, 4, 2);
            this.btnConfig.Name = "btnConfig";
            this.btnConfig.Size = new System.Drawing.Size(85, 40);
            this.btnConfig.TabIndex = 330;
            this.btnConfig.Text = "Set";
            this.btnConfig.UseVisualStyleBackColor = false;
            // 
            // GwSpeedHiLoButtons
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DimGray;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "GwSpeedHiLoButtons";
            this.Size = new System.Drawing.Size(308, 168);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        public System.Windows.Forms.Label lblSpeed;
        public System.Windows.Forms.RadioButton rdoSpeedModeHI;
        public System.Windows.Forms.RadioButton rdoSpeedModeLO;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        public System.Windows.Forms.Button btnConfig;
    }
}
