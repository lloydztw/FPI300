namespace AX.Gui
{
    partial class GwMotorJogSticks
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
            this.btnStickLeft = new System.Windows.Forms.Button();
            this.btnStickUp = new System.Windows.Forms.Button();
            this.btnStickHome = new System.Windows.Forms.Button();
            this.btnStickRight = new System.Windows.Forms.Button();
            this.btnStickDown = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnStickLeft
            // 
            this.btnStickLeft.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnStickLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnStickLeft.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStickLeft.ForeColor = System.Drawing.Color.Black;
            this.btnStickLeft.Location = new System.Drawing.Point(3, 50);
            this.btnStickLeft.Name = "btnStickLeft";
            this.btnStickLeft.Size = new System.Drawing.Size(45, 43);
            this.btnStickLeft.TabIndex = 50;
            this.btnStickLeft.Text = "←";
            this.btnStickLeft.UseVisualStyleBackColor = false;
            // 
            // btnStickUp
            // 
            this.btnStickUp.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnStickUp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnStickUp.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStickUp.ForeColor = System.Drawing.Color.Black;
            this.btnStickUp.Location = new System.Drawing.Point(54, 3);
            this.btnStickUp.Name = "btnStickUp";
            this.btnStickUp.Size = new System.Drawing.Size(47, 41);
            this.btnStickUp.TabIndex = 52;
            this.btnStickUp.Text = "↑";
            this.btnStickUp.UseVisualStyleBackColor = false;
            // 
            // btnStickHome
            // 
            this.btnStickHome.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnStickHome.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnStickHome.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStickHome.ForeColor = System.Drawing.Color.Black;
            this.btnStickHome.Location = new System.Drawing.Point(54, 50);
            this.btnStickHome.Name = "btnStickHome";
            this.btnStickHome.Size = new System.Drawing.Size(47, 43);
            this.btnStickHome.TabIndex = 51;
            this.btnStickHome.Text = "H";
            this.btnStickHome.UseVisualStyleBackColor = false;
            // 
            // btnStickRight
            // 
            this.btnStickRight.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnStickRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnStickRight.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStickRight.ForeColor = System.Drawing.Color.Black;
            this.btnStickRight.Location = new System.Drawing.Point(107, 50);
            this.btnStickRight.Name = "btnStickRight";
            this.btnStickRight.Size = new System.Drawing.Size(47, 43);
            this.btnStickRight.TabIndex = 49;
            this.btnStickRight.Text = "→";
            this.btnStickRight.UseVisualStyleBackColor = false;
            // 
            // btnStickDown
            // 
            this.btnStickDown.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnStickDown.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnStickDown.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStickDown.ForeColor = System.Drawing.Color.Black;
            this.btnStickDown.Location = new System.Drawing.Point(54, 99);
            this.btnStickDown.Name = "btnStickDown";
            this.btnStickDown.Size = new System.Drawing.Size(47, 43);
            this.btnStickDown.TabIndex = 53;
            this.btnStickDown.Text = "↓";
            this.btnStickDown.UseVisualStyleBackColor = false;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 34F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.tableLayoutPanel1.Controls.Add(this.btnStickHome, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.btnStickRight, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.btnStickLeft, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.btnStickDown, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.btnStickUp, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 34F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(157, 145);
            this.tableLayoutPanel1.TabIndex = 54;
            // 
            // GwMotorJogSticks
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Gray;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "GwMotorJogSticks";
            this.Size = new System.Drawing.Size(157, 145);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        internal System.Windows.Forms.Button btnStickLeft;
        internal System.Windows.Forms.Button btnStickUp;
        internal System.Windows.Forms.Button btnStickHome;
        internal System.Windows.Forms.Button btnStickRight;
        internal System.Windows.Forms.Button btnStickDown;
    }
}
