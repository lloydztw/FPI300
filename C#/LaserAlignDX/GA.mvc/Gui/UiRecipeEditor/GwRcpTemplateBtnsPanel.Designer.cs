namespace LaserAlignDX.GA.mvc.Gui.UiRecipeEditor
{
    partial class GwRcpTemplateBtnsPanel
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
            this.btnPickGolden = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnRotateGolden = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnPickGolden
            // 
            this.btnPickGolden.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnPickGolden.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnPickGolden.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnPickGolden.Location = new System.Drawing.Point(205, 38);
            this.btnPickGolden.Margin = new System.Windows.Forms.Padding(4);
            this.btnPickGolden.Name = "btnPickGolden";
            this.btnPickGolden.Size = new System.Drawing.Size(160, 52);
            this.btnPickGolden.TabIndex = 50;
            this.btnPickGolden.Text = "擷取模板";
            this.btnPickGolden.UseVisualStyleBackColor = false;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnRotateGolden);
            this.groupBox1.Controls.Add(this.btnPickGolden);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(8, 0);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(8, 0, 8, 8);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.groupBox1.Size = new System.Drawing.Size(413, 118);
            this.groupBox1.TabIndex = 77;
            this.groupBox1.TabStop = false;
            // 
            // btnRotateGolden
            // 
            this.btnRotateGolden.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnRotateGolden.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnRotateGolden.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnRotateGolden.Location = new System.Drawing.Point(32, 38);
            this.btnRotateGolden.Margin = new System.Windows.Forms.Padding(4);
            this.btnRotateGolden.Name = "btnRotateGolden";
            this.btnRotateGolden.Size = new System.Drawing.Size(160, 52);
            this.btnRotateGolden.TabIndex = 51;
            this.btnRotateGolden.Text = "轉正";
            this.btnRotateGolden.UseVisualStyleBackColor = false;
            // 
            // GwRcpTemplateBtnsPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.Name = "GwRcpTemplateBtnsPanel";
            this.Padding = new System.Windows.Forms.Padding(8, 0, 8, 2);
            this.Size = new System.Drawing.Size(429, 120);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.Button btnPickGolden;
        private System.Windows.Forms.GroupBox groupBox1;
        public System.Windows.Forms.Button btnRotateGolden;
    }
}
