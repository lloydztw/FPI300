namespace LaserAlignDX.Mvc.Gui
{
    partial class FormLightControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormLightControl));
            this.lblLight2 = new System.Windows.Forms.Label();
            this.numLightValue = new System.Windows.Forms.NumericUpDown();
            this.lblLight1 = new System.Windows.Forms.Label();
            this.lblLight3 = new System.Windows.Forms.Label();
            this.cboLightChannels = new System.Windows.Forms.ComboBox();
            this.btnWriteToRecipe = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numLightValue)).BeginInit();
            this.SuspendLayout();
            // 
            // lblLight2
            // 
            this.lblLight2.AutoSize = true;
            this.lblLight2.Font = new System.Drawing.Font("Microsoft YaHei UI", 16.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLight2.Location = new System.Drawing.Point(62, 115);
            this.lblLight2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblLight2.Name = "lblLight2";
            this.lblLight2.Size = new System.Drawing.Size(250, 43);
            this.lblLight2.TabIndex = 0;
            this.lblLight2.Text = "設定值 (0~255)";
            // 
            // numLightValue
            // 
            this.numLightValue.Font = new System.Drawing.Font("Microsoft YaHei UI", 16.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.numLightValue.Location = new System.Drawing.Point(340, 114);
            this.numLightValue.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.numLightValue.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numLightValue.Name = "numLightValue";
            this.numLightValue.Size = new System.Drawing.Size(207, 49);
            this.numLightValue.TabIndex = 1;
            this.numLightValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // lblLight1
            // 
            this.lblLight1.AutoSize = true;
            this.lblLight1.Font = new System.Drawing.Font("Microsoft YaHei UI", 16.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLight1.Location = new System.Drawing.Point(62, 49);
            this.lblLight1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblLight1.Name = "lblLight1";
            this.lblLight1.Size = new System.Drawing.Size(151, 43);
            this.lblLight1.TabIndex = 4;
            this.lblLight1.Text = "光源通道";
            // 
            // lblLight3
            // 
            this.lblLight3.AutoSize = true;
            this.lblLight3.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLight3.Location = new System.Drawing.Point(73, 224);
            this.lblLight3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblLight3.Name = "lblLight3";
            this.lblLight3.Size = new System.Drawing.Size(463, 31);
            this.lblLight3.TabIndex = 8;
            this.lblLight3.Text = "註: 关闭灯光设置0 打开灯光设置数值 即可";
            // 
            // cboLightChannels
            // 
            this.cboLightChannels.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboLightChannels.FormattingEnabled = true;
            this.cboLightChannels.Location = new System.Drawing.Point(340, 51);
            this.cboLightChannels.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cboLightChannels.Name = "cboLightChannels";
            this.cboLightChannels.Size = new System.Drawing.Size(206, 39);
            this.cboLightChannels.TabIndex = 31;
            // 
            // btnWriteToRecipe
            // 
            this.btnWriteToRecipe.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.btnWriteToRecipe.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnWriteToRecipe.Font = new System.Drawing.Font("微软雅黑", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnWriteToRecipe.Location = new System.Drawing.Point(614, 48);
            this.btnWriteToRecipe.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnWriteToRecipe.Name = "btnWriteToRecipe";
            this.btnWriteToRecipe.Size = new System.Drawing.Size(176, 116);
            this.btnWriteToRecipe.TabIndex = 38;
            this.btnWriteToRecipe.Text = "寫回 參數檔";
            this.btnWriteToRecipe.UseVisualStyleBackColor = false;
            // 
            // FormLightControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.LightSteelBlue;
            this.ClientSize = new System.Drawing.Size(860, 326);
            this.Controls.Add(this.btnWriteToRecipe);
            this.Controls.Add(this.cboLightChannels);
            this.Controls.Add(this.lblLight3);
            this.Controls.Add(this.lblLight1);
            this.Controls.Add(this.numLightValue);
            this.Controls.Add(this.lblLight2);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormLightControl";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "光源控制界面";
            ((System.ComponentModel.ISupportInitialize)(this.numLightValue)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblLight2;
        private System.Windows.Forms.NumericUpDown numLightValue;
        private System.Windows.Forms.Label lblLight1;
        private System.Windows.Forms.Label lblLight3;
        private System.Windows.Forms.ComboBox cboLightChannels;
        private System.Windows.Forms.Button btnWriteToRecipe;
    }
}