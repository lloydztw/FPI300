namespace LaserAlignDX.Mvc.Gui
{
    partial class FormPadThresholdsEditor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormPadThresholdsEditor));
            this.label1 = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.numBinaryThreshold = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.jezTransImageViewPanel1 = new LaserAlignDX.Mvc.Gui.JezTransImageViewPanel();
            this.numDistTransThreshold = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.numBinaryThreshold)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDistTransThreshold)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(488, 47);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 27);
            this.label1.TabIndex = 0;
            this.label1.Text = "門限";
            // 
            // btnOK
            // 
            this.btnOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnOK.Font = new System.Drawing.Font("微软雅黑", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOK.Location = new System.Drawing.Point(628, 334);
            this.btnOK.Margin = new System.Windows.Forms.Padding(4);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(156, 74);
            this.btnOK.TabIndex = 0;
            this.btnOK.Text = "寫回 參數檔";
            this.btnOK.UseVisualStyleBackColor = false;
            // 
            // numBinaryThreshold
            // 
            this.numBinaryThreshold.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.numBinaryThreshold.Location = new System.Drawing.Point(632, 45);
            this.numBinaryThreshold.Margin = new System.Windows.Forms.Padding(4);
            this.numBinaryThreshold.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numBinaryThreshold.Name = "numBinaryThreshold";
            this.numBinaryThreshold.Size = new System.Drawing.Size(152, 33);
            this.numBinaryThreshold.TabIndex = 2;
            this.numBinaryThreshold.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(488, 109);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(112, 27);
            this.label2.TabIndex = 39;
            this.label2.Text = "去刮痕閥值";
            // 
            // jezTransImageViewPanel1
            // 
            this.jezTransImageViewPanel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.jezTransImageViewPanel1.Location = new System.Drawing.Point(8, 8);
            this.jezTransImageViewPanel1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.jezTransImageViewPanel1.Name = "jezTransImageViewPanel1";
            this.jezTransImageViewPanel1.Size = new System.Drawing.Size(447, 424);
            this.jezTransImageViewPanel1.TabIndex = 40;
            // 
            // numDistTransThreshold
            // 
            this.numDistTransThreshold.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.numDistTransThreshold.Location = new System.Drawing.Point(632, 109);
            this.numDistTransThreshold.Margin = new System.Windows.Forms.Padding(4);
            this.numDistTransThreshold.Name = "numDistTransThreshold";
            this.numDistTransThreshold.Size = new System.Drawing.Size(152, 33);
            this.numDistTransThreshold.TabIndex = 41;
            this.numDistTransThreshold.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // FormPadThresholdsEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.LightSteelBlue;
            this.ClientSize = new System.Drawing.Size(811, 432);
            this.Controls.Add(this.numDistTransThreshold);
            this.Controls.Add(this.jezTransImageViewPanel1);
            this.Controls.Add(this.numBinaryThreshold);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormPadThresholdsEditor";
            this.Padding = new System.Windows.Forms.Padding(8, 8, 8, 0);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "晶粒 Pad 閥值設定";
            ((System.ComponentModel.ISupportInitialize)(this.numBinaryThreshold)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDistTransThreshold)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.NumericUpDown numBinaryThreshold;
        private System.Windows.Forms.Label label2;
        private JezTransImageViewPanel jezTransImageViewPanel1;
        private System.Windows.Forms.NumericUpDown numDistTransThreshold;
    }
}