namespace LaserAlignDX.Mvc.Gui
{
    partial class FormChipTemplateDim
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormChipTemplateDim));
            this.label1 = new System.Windows.Forms.Label();
            this.numChipHeight = new System.Windows.Forms.NumericUpDown();
            this.btnOK = new System.Windows.Forms.Button();
            this.numChipWidth = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.lblInfo = new System.Windows.Forms.RichTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.numChipHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numChipWidth)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(59, 170);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(90, 27);
            this.label1.TabIndex = 0;
            this.label1.Text = "尺寸X (mm)";
            // 
            // numChipHeight
            // 
            this.numChipHeight.DecimalPlaces = 3;
            this.numChipHeight.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.numChipHeight.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.numChipHeight.Location = new System.Drawing.Point(196, 208);
            this.numChipHeight.Margin = new System.Windows.Forms.Padding(4);
            this.numChipHeight.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.numChipHeight.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.numChipHeight.Name = "numChipHeight";
            this.numChipHeight.Size = new System.Drawing.Size(184, 33);
            this.numChipHeight.TabIndex = 3;
            this.numChipHeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numChipHeight.Value = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            // 
            // btnOK
            // 
            this.btnOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnOK.Font = new System.Drawing.Font("微软雅黑", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOK.Location = new System.Drawing.Point(464, 167);
            this.btnOK.Margin = new System.Windows.Forms.Padding(4);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(156, 74);
            this.btnOK.TabIndex = 0;
            this.btnOK.Text = "寫回 參數檔";
            this.btnOK.UseVisualStyleBackColor = false;
            // 
            // numChipWidth
            // 
            this.numChipWidth.DecimalPlaces = 3;
            this.numChipWidth.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.numChipWidth.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.numChipWidth.Location = new System.Drawing.Point(196, 167);
            this.numChipWidth.Margin = new System.Windows.Forms.Padding(4);
            this.numChipWidth.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.numChipWidth.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.numChipWidth.Name = "numChipWidth";
            this.numChipWidth.Size = new System.Drawing.Size(184, 33);
            this.numChipWidth.TabIndex = 2;
            this.numChipWidth.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numChipWidth.Value = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(60, 211);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(90, 27);
            this.label2.TabIndex = 39;
            this.label2.Text = "尺寸Y (mm)";
            // 
            // lblInfo
            // 
            this.lblInfo.BackColor = System.Drawing.Color.LightGoldenrodYellow;
            this.lblInfo.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblInfo.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInfo.Location = new System.Drawing.Point(8, 8);
            this.lblInfo.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.ReadOnly = true;
            this.lblInfo.Size = new System.Drawing.Size(656, 130);
            this.lblInfo.TabIndex = 41;
            this.lblInfo.Text = "1\n2\n3\n4\n5";
            // 
            // FormChipTemplateDim
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.LightSteelBlue;
            this.ClientSize = new System.Drawing.Size(672, 272);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.numChipWidth);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.numChipHeight);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormChipTemplateDim";
            this.Padding = new System.Windows.Forms.Padding(8, 8, 8, 0);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "晶粒樣本尺寸";
            ((System.ComponentModel.ISupportInitialize)(this.numChipHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numChipWidth)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numChipHeight;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.NumericUpDown numChipWidth;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RichTextBox lblInfo;
    }
}