namespace AX.Gui
{
    partial class GwMotorMoveCmd
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
            this.numMoveTo1 = new System.Windows.Forms.NumericUpDown();
            this.lblAxisName = new System.Windows.Forms.Label();
            this.btnMoveTo = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numMoveTo1)).BeginInit();
            this.SuspendLayout();
            // 
            // numMoveTo1
            // 
            this.numMoveTo1.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.numMoveTo1.Location = new System.Drawing.Point(62, 10);
            this.numMoveTo1.Margin = new System.Windows.Forms.Padding(4);
            this.numMoveTo1.Name = "numMoveTo1";
            this.numMoveTo1.Size = new System.Drawing.Size(100, 27);
            this.numMoveTo1.TabIndex = 328;
            this.numMoveTo1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // lblAxisName
            // 
            this.lblAxisName.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblAxisName.ForeColor = System.Drawing.Color.White;
            this.lblAxisName.Location = new System.Drawing.Point(13, 11);
            this.lblAxisName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblAxisName.Name = "lblAxisName";
            this.lblAxisName.Size = new System.Drawing.Size(41, 24);
            this.lblAxisName.TabIndex = 327;
            this.lblAxisName.Text = "Z =";
            this.lblAxisName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnMoveTo
            // 
            this.btnMoveTo.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnMoveTo.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMoveTo.ForeColor = System.Drawing.Color.Black;
            this.btnMoveTo.Location = new System.Drawing.Point(62, 47);
            this.btnMoveTo.Margin = new System.Windows.Forms.Padding(4);
            this.btnMoveTo.Name = "btnMoveTo";
            this.btnMoveTo.Size = new System.Drawing.Size(100, 28);
            this.btnMoveTo.TabIndex = 326;
            this.btnMoveTo.Text = "Move";
            this.btnMoveTo.UseVisualStyleBackColor = false;
            // 
            // GwMotorMoveCmd
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Gray;
            this.Controls.Add(this.numMoveTo1);
            this.Controls.Add(this.lblAxisName);
            this.Controls.Add(this.btnMoveTo);
            this.Font = new System.Drawing.Font("新細明體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Name = "GwMotorMoveCmd";
            this.Size = new System.Drawing.Size(186, 84);
            ((System.ComponentModel.ISupportInitialize)(this.numMoveTo1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        public System.Windows.Forms.NumericUpDown numMoveTo1;
        public System.Windows.Forms.Label lblAxisName;
        public System.Windows.Forms.Button btnMoveTo;
    }
}
