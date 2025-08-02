namespace NeedleX.FormSpace
{
    partial class frmHandle
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
            this.label1 = new System.Windows.Forms.Label();
            this.cboAxisList = new System.Windows.Forms.ComboBox();
            this.vsHandleMotorUI1 = new Common.VsHandleMotorUI();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "轴选择";
            // 
            // cboAxisList
            // 
            this.cboAxisList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboAxisList.FormattingEnabled = true;
            this.cboAxisList.Location = new System.Drawing.Point(59, 6);
            this.cboAxisList.Name = "cboAxisList";
            this.cboAxisList.Size = new System.Drawing.Size(121, 20);
            this.cboAxisList.TabIndex = 1;
            // 
            // vsHandleMotorUI1
            // 
            this.vsHandleMotorUI1.Location = new System.Drawing.Point(12, 32);
            this.vsHandleMotorUI1.Name = "vsHandleMotorUI1";
            this.vsHandleMotorUI1.Size = new System.Drawing.Size(228, 299);
            this.vsHandleMotorUI1.TabIndex = 2;
            // 
            // frmHandle
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(246, 336);
            this.Controls.Add(this.vsHandleMotorUI1);
            this.Controls.Add(this.cboAxisList);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmHandle";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "frmHandle";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cboAxisList;
        private Common.VsHandleMotorUI vsHandleMotorUI1;
    }
}