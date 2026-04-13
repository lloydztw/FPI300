namespace AX.Gui
{
    partial class GwMotorSimpleJogV
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
            this.grpMotor = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.btnJogForward = new System.Windows.Forms.Button();
            this.btnJogHome = new System.Windows.Forms.Button();
            this.btnJogBackward = new System.Windows.Forms.Button();
            this.lblPos = new System.Windows.Forms.Label();
            this.grpMotor.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpMotor
            // 
            this.grpMotor.BackColor = System.Drawing.Color.Transparent;
            this.grpMotor.Controls.Add(this.lblPos);
            this.grpMotor.Controls.Add(this.tableLayoutPanel1);
            this.grpMotor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpMotor.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.grpMotor.ForeColor = System.Drawing.Color.White;
            this.grpMotor.Location = new System.Drawing.Point(0, 0);
            this.grpMotor.Margin = new System.Windows.Forms.Padding(0);
            this.grpMotor.Name = "grpMotor";
            this.grpMotor.Padding = new System.Windows.Forms.Padding(0);
            this.grpMotor.Size = new System.Drawing.Size(85, 213);
            this.grpMotor.TabIndex = 312;
            this.grpMotor.TabStop = false;
            this.grpMotor.Text = "Z1";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Controls.Add(this.btnJogForward, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.btnJogHome, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.btnJogBackward, 0, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(19, 35);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(1);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(45, 128);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // btnJogForward
            // 
            this.btnJogForward.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnJogForward.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnJogForward.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnJogForward.ForeColor = System.Drawing.Color.Black;
            this.btnJogForward.Location = new System.Drawing.Point(2, 86);
            this.btnJogForward.Margin = new System.Windows.Forms.Padding(2);
            this.btnJogForward.Name = "btnJogForward";
            this.btnJogForward.Size = new System.Drawing.Size(41, 40);
            this.btnJogForward.TabIndex = 56;
            this.btnJogForward.Text = "↓";
            this.btnJogForward.UseVisualStyleBackColor = false;
            // 
            // btnJogHome
            // 
            this.btnJogHome.BackColor = System.Drawing.Color.DimGray;
            this.btnJogHome.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnJogHome.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnJogHome.ForeColor = System.Drawing.Color.Black;
            this.btnJogHome.Location = new System.Drawing.Point(2, 44);
            this.btnJogHome.Margin = new System.Windows.Forms.Padding(2);
            this.btnJogHome.Name = "btnJogHome";
            this.btnJogHome.Size = new System.Drawing.Size(41, 38);
            this.btnJogHome.TabIndex = 0;
            this.btnJogHome.UseVisualStyleBackColor = false;
            // 
            // btnJogBackward
            // 
            this.btnJogBackward.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnJogBackward.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnJogBackward.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnJogBackward.ForeColor = System.Drawing.Color.Black;
            this.btnJogBackward.Location = new System.Drawing.Point(2, 2);
            this.btnJogBackward.Margin = new System.Windows.Forms.Padding(2);
            this.btnJogBackward.Name = "btnJogBackward";
            this.btnJogBackward.Size = new System.Drawing.Size(41, 38);
            this.btnJogBackward.TabIndex = 53;
            this.btnJogBackward.Text = "↑";
            this.btnJogBackward.UseVisualStyleBackColor = false;
            // 
            // lblPos
            // 
            this.lblPos.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblPos.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblPos.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblPos.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPos.ForeColor = System.Drawing.Color.Lime;
            this.lblPos.Location = new System.Drawing.Point(0, 185);
            this.lblPos.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPos.Name = "lblPos";
            this.lblPos.Size = new System.Drawing.Size(85, 28);
            this.lblPos.TabIndex = 314;
            this.lblPos.Text = "0";
            this.lblPos.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // GwMotorSimpleJogV
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.RoyalBlue;
            this.Controls.Add(this.grpMotor);
            this.Name = "GwMotorSimpleJogV";
            this.Size = new System.Drawing.Size(85, 213);
            this.grpMotor.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        public System.Windows.Forms.GroupBox grpMotor;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        public System.Windows.Forms.Button btnJogBackward;
        public System.Windows.Forms.Button btnJogHome;
        public System.Windows.Forms.Button btnJogForward;
        public System.Windows.Forms.Label lblPos;
    }
}
