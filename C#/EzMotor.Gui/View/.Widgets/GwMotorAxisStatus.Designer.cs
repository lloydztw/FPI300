namespace AX.Gui
{
    partial class GwMotorAxisStatus
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
            this.lblSpeedPPS = new System.Windows.Forms.Label();
            this.lblSpeed = new System.Windows.Forms.Label();
            this.labelS = new System.Windows.Forms.Label();
            this.lblStateError = new System.Windows.Forms.Label();
            this.lblPosPS = new System.Windows.Forms.Label();
            this.lblPosition = new System.Windows.Forms.Label();
            this.labelP = new System.Windows.Forms.Label();
            this.lblStateMoving = new System.Windows.Forms.Label();
            this.lblStateReady = new System.Windows.Forms.Label();
            this.lblAxisName = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.lblSignalSlow = new System.Windows.Forms.Label();
            this.lblSignalINP = new System.Windows.Forms.Label();
            this.lblSignalLimitP = new System.Windows.Forms.Label();
            this.lblSignalORG = new System.Windows.Forms.Label();
            this.lblSignalLimitN = new System.Windows.Forms.Label();
            this.tableLayoutPanel0 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel0.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblSpeedPPS
            // 
            this.lblSpeedPPS.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.lblSpeedPPS.Font = new System.Drawing.Font("Arial Narrow", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSpeedPPS.ForeColor = System.Drawing.Color.Black;
            this.lblSpeedPPS.Location = new System.Drawing.Point(275, 38);
            this.lblSpeedPPS.Margin = new System.Windows.Forms.Padding(3);
            this.lblSpeedPPS.Name = "lblSpeedPPS";
            this.lblSpeedPPS.Size = new System.Drawing.Size(45, 29);
            this.lblSpeedPPS.TabIndex = 92;
            this.lblSpeedPPS.Text = "00000";
            this.lblSpeedPPS.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblSpeedPPS.Visible = false;
            // 
            // lblSpeed
            // 
            this.lblSpeed.BackColor = System.Drawing.Color.Black;
            this.lblSpeed.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblSpeed.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSpeed.Font = new System.Drawing.Font("微軟正黑體", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblSpeed.ForeColor = System.Drawing.Color.Lime;
            this.lblSpeed.Location = new System.Drawing.Point(112, 37);
            this.lblSpeed.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.lblSpeed.Name = "lblSpeed";
            this.lblSpeed.Padding = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.lblSpeed.Size = new System.Drawing.Size(157, 32);
            this.lblSpeed.TabIndex = 91;
            this.lblSpeed.Text = "0.000";
            this.lblSpeed.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // labelS
            // 
            this.labelS.AutoSize = true;
            this.labelS.BackColor = System.Drawing.Color.Transparent;
            this.labelS.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelS.Font = new System.Drawing.Font("微軟正黑體", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.labelS.Location = new System.Drawing.Point(4, 35);
            this.labelS.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelS.Name = "labelS";
            this.labelS.Size = new System.Drawing.Size(101, 36);
            this.labelS.TabIndex = 90;
            this.labelS.Text = "Speed";
            this.labelS.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblStateError
            // 
            this.lblStateError.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.lblStateError.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblStateError.Font = new System.Drawing.Font("微軟正黑體", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblStateError.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblStateError.Location = new System.Drawing.Point(16, 89);
            this.lblStateError.Margin = new System.Windows.Forms.Padding(16, 2, 0, 0);
            this.lblStateError.Name = "lblStateError";
            this.lblStateError.Size = new System.Drawing.Size(68, 22);
            this.lblStateError.TabIndex = 89;
            this.lblStateError.Text = "Alarm";
            this.lblStateError.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblPosPS
            // 
            this.lblPosPS.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.lblPosPS.Font = new System.Drawing.Font("Arial Narrow", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPosPS.ForeColor = System.Drawing.Color.Black;
            this.lblPosPS.Location = new System.Drawing.Point(275, 3);
            this.lblPosPS.Margin = new System.Windows.Forms.Padding(3);
            this.lblPosPS.Name = "lblPosPS";
            this.lblPosPS.Size = new System.Drawing.Size(45, 29);
            this.lblPosPS.TabIndex = 88;
            this.lblPosPS.Text = "00000";
            this.lblPosPS.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblPosPS.Visible = false;
            // 
            // lblPosition
            // 
            this.lblPosition.BackColor = System.Drawing.Color.Black;
            this.lblPosition.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblPosition.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPosition.Font = new System.Drawing.Font("微軟正黑體", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblPosition.ForeColor = System.Drawing.Color.Lime;
            this.lblPosition.Location = new System.Drawing.Point(112, 2);
            this.lblPosition.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.lblPosition.Name = "lblPosition";
            this.lblPosition.Padding = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.lblPosition.Size = new System.Drawing.Size(157, 31);
            this.lblPosition.TabIndex = 87;
            this.lblPosition.Text = "0.000";
            this.lblPosition.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // labelP
            // 
            this.labelP.AutoSize = true;
            this.labelP.BackColor = System.Drawing.Color.Transparent;
            this.labelP.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelP.Font = new System.Drawing.Font("微軟正黑體", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.labelP.Location = new System.Drawing.Point(4, 0);
            this.labelP.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelP.Name = "labelP";
            this.labelP.Size = new System.Drawing.Size(101, 35);
            this.labelP.TabIndex = 86;
            this.labelP.Text = "Position";
            this.labelP.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblStateMoving
            // 
            this.lblStateMoving.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.lblStateMoving.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblStateMoving.Font = new System.Drawing.Font("微軟正黑體", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblStateMoving.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblStateMoving.Location = new System.Drawing.Point(16, 63);
            this.lblStateMoving.Margin = new System.Windows.Forms.Padding(16, 2, 0, 0);
            this.lblStateMoving.Name = "lblStateMoving";
            this.lblStateMoving.Size = new System.Drawing.Size(68, 22);
            this.lblStateMoving.TabIndex = 84;
            this.lblStateMoving.Text = "Moving";
            this.lblStateMoving.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblStateReady
            // 
            this.lblStateReady.BackColor = System.Drawing.Color.Lime;
            this.lblStateReady.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblStateReady.Font = new System.Drawing.Font("微軟正黑體", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblStateReady.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblStateReady.Location = new System.Drawing.Point(16, 37);
            this.lblStateReady.Margin = new System.Windows.Forms.Padding(16, 2, 0, 0);
            this.lblStateReady.Name = "lblStateReady";
            this.lblStateReady.Size = new System.Drawing.Size(68, 22);
            this.lblStateReady.TabIndex = 85;
            this.lblStateReady.Text = "Ready";
            this.lblStateReady.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblAxisName
            // 
            this.lblAxisName.AutoSize = true;
            this.lblAxisName.BackColor = System.Drawing.Color.Transparent;
            this.lblAxisName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblAxisName.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAxisName.Location = new System.Drawing.Point(0, 0);
            this.lblAxisName.Margin = new System.Windows.Forms.Padding(0);
            this.lblAxisName.Name = "lblAxisName";
            this.lblAxisName.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.lblAxisName.Size = new System.Drawing.Size(168, 35);
            this.lblAxisName.TabIndex = 100;
            this.lblAxisName.Text = "X Axis Status";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.lblAxisName, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblStateError, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.lblStateReady, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblStateMoving, 0, 2);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 31F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 23F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 23F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 23F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(168, 116);
            this.tableLayoutPanel1.TabIndex = 101;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 3;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.Controls.Add(this.labelP, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.labelS, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.lblPosition, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.lblSpeed, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.lblPosPS, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.lblSpeedPPS, 2, 1);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(169, 42);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(324, 71);
            this.tableLayoutPanel2.TabIndex = 102;
            // 
            // lblSignalSlow
            // 
            this.lblSignalSlow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.lblSignalSlow.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblSignalSlow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSignalSlow.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSignalSlow.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblSignalSlow.Location = new System.Drawing.Point(114, 0);
            this.lblSignalSlow.Margin = new System.Windows.Forms.Padding(0);
            this.lblSignalSlow.Name = "lblSignalSlow";
            this.lblSignalSlow.Size = new System.Drawing.Size(38, 22);
            this.lblSignalSlow.TabIndex = 98;
            this.lblSignalSlow.Text = "S";
            this.lblSignalSlow.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblSignalINP
            // 
            this.lblSignalINP.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.lblSignalINP.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblSignalINP.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSignalINP.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSignalINP.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblSignalINP.Location = new System.Drawing.Point(152, 0);
            this.lblSignalINP.Margin = new System.Windows.Forms.Padding(0);
            this.lblSignalINP.Name = "lblSignalINP";
            this.lblSignalINP.Size = new System.Drawing.Size(38, 22);
            this.lblSignalINP.TabIndex = 99;
            this.lblSignalINP.Text = "IN";
            this.lblSignalINP.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblSignalLimitP
            // 
            this.lblSignalLimitP.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.lblSignalLimitP.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblSignalLimitP.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSignalLimitP.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblSignalLimitP.Location = new System.Drawing.Point(76, 0);
            this.lblSignalLimitP.Margin = new System.Windows.Forms.Padding(0);
            this.lblSignalLimitP.Name = "lblSignalLimitP";
            this.lblSignalLimitP.Size = new System.Drawing.Size(38, 22);
            this.lblSignalLimitP.TabIndex = 95;
            this.lblSignalLimitP.Text = "L+";
            this.lblSignalLimitP.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblSignalORG
            // 
            this.lblSignalORG.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.lblSignalORG.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblSignalORG.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSignalORG.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSignalORG.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblSignalORG.Location = new System.Drawing.Point(38, 0);
            this.lblSignalORG.Margin = new System.Windows.Forms.Padding(0);
            this.lblSignalORG.Name = "lblSignalORG";
            this.lblSignalORG.Size = new System.Drawing.Size(38, 22);
            this.lblSignalORG.TabIndex = 94;
            this.lblSignalORG.Text = "O";
            this.lblSignalORG.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblSignalLimitN
            // 
            this.lblSignalLimitN.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.lblSignalLimitN.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblSignalLimitN.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSignalLimitN.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSignalLimitN.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblSignalLimitN.Location = new System.Drawing.Point(0, 0);
            this.lblSignalLimitN.Margin = new System.Windows.Forms.Padding(0);
            this.lblSignalLimitN.Name = "lblSignalLimitN";
            this.lblSignalLimitN.Size = new System.Drawing.Size(38, 22);
            this.lblSignalLimitN.TabIndex = 93;
            this.lblSignalLimitN.Text = "L-";
            this.lblSignalLimitN.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tableLayoutPanel0
            // 
            this.tableLayoutPanel0.ColumnCount = 5;
            this.tableLayoutPanel0.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel0.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel0.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel0.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel0.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel0.Controls.Add(this.lblSignalLimitN, 0, 0);
            this.tableLayoutPanel0.Controls.Add(this.lblSignalORG, 1, 0);
            this.tableLayoutPanel0.Controls.Add(this.lblSignalLimitP, 2, 0);
            this.tableLayoutPanel0.Controls.Add(this.lblSignalINP, 4, 0);
            this.tableLayoutPanel0.Controls.Add(this.lblSignalSlow, 3, 0);
            this.tableLayoutPanel0.Location = new System.Drawing.Point(300, 3);
            this.tableLayoutPanel0.Name = "tableLayoutPanel0";
            this.tableLayoutPanel0.RowCount = 1;
            this.tableLayoutPanel0.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel0.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel0.Size = new System.Drawing.Size(190, 22);
            this.tableLayoutPanel0.TabIndex = 103;
            // 
            // GwMotorAxisStatus
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.WhiteSmoke;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.Controls.Add(this.tableLayoutPanel2);
            this.Controls.Add(this.tableLayoutPanel0);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Font = new System.Drawing.Font("新細明體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.Name = "GwMotorAxisStatus";
            this.Size = new System.Drawing.Size(500, 120);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.tableLayoutPanel0.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.Label lblSpeedPPS;
        internal System.Windows.Forms.Label lblSpeed;
        internal System.Windows.Forms.Label labelS;
        internal System.Windows.Forms.Label lblStateError;
        internal System.Windows.Forms.Label lblPosPS;
        internal System.Windows.Forms.Label lblPosition;
        internal System.Windows.Forms.Label labelP;
        internal System.Windows.Forms.Label lblStateMoving;
        internal System.Windows.Forms.Label lblStateReady;
        internal System.Windows.Forms.Label lblAxisName;
        internal System.Windows.Forms.Label lblSignalSlow;
        internal System.Windows.Forms.Label lblSignalINP;
        internal System.Windows.Forms.Label lblSignalLimitP;
        internal System.Windows.Forms.Label lblSignalORG;
        internal System.Windows.Forms.Label lblSignalLimitN;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel0;
    }
}
