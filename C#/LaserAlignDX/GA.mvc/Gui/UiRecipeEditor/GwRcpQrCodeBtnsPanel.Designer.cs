namespace LaserAlignDX.GA.mvc.Gui.UiRecipeEditor
{
    partial class GwRcpQrCodeBtnsPanel
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
            this.btnTryQrCode = new System.Windows.Forms.Button();
            this.rtbCodeContent = new System.Windows.Forms.RichTextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnTryQrCode
            // 
            this.btnTryQrCode.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnTryQrCode.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnTryQrCode.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnTryQrCode.Location = new System.Drawing.Point(28, 37);
            this.btnTryQrCode.Margin = new System.Windows.Forms.Padding(4);
            this.btnTryQrCode.Name = "btnTryQrCode";
            this.btnTryQrCode.Size = new System.Drawing.Size(160, 52);
            this.btnTryQrCode.TabIndex = 53;
            this.btnTryQrCode.Text = "掃碼";
            this.btnTryQrCode.UseVisualStyleBackColor = false;
            // 
            // rtbCodeContent
            // 
            this.rtbCodeContent.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.rtbCodeContent.BackColor = System.Drawing.Color.Ivory;
            this.rtbCodeContent.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbCodeContent.Location = new System.Drawing.Point(198, 37);
            this.rtbCodeContent.Name = "rtbCodeContent";
            this.rtbCodeContent.ReadOnly = true;
            this.rtbCodeContent.Size = new System.Drawing.Size(470, 52);
            this.rtbCodeContent.TabIndex = 54;
            this.rtbCodeContent.Text = "";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnTryQrCode);
            this.groupBox1.Controls.Add(this.rtbCodeContent);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(8, 0);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(8, 0, 8, 8);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.groupBox1.Size = new System.Drawing.Size(698, 118);
            this.groupBox1.TabIndex = 78;
            this.groupBox1.TabStop = false;
            // 
            // GwRcpQrCodeBtnsPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.Name = "GwRcpQrCodeBtnsPanel";
            this.Padding = new System.Windows.Forms.Padding(8, 0, 8, 2);
            this.Size = new System.Drawing.Size(714, 120);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.Button btnTryQrCode;
        public System.Windows.Forms.RichTextBox rtbCodeContent;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}
