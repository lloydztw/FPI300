namespace LaserAlignDX.GA.FormSpace
{
    partial class FormMotor
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.vsTouchMotorUI3 = new Common.VsTouchMotorUI();
            this.vsTouchMotorUI2 = new Common.VsTouchMotorUI();
            this.vsTouchMotorUI1 = new Common.VsTouchMotorUI();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.vsTouchMotorUI4 = new Common.VsTouchMotorUI();
            this.vsTouchMotorUI5 = new Common.VsTouchMotorUI();
            this.vsTouchMotorUI6 = new Common.VsTouchMotorUI();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1247, 599);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.vsTouchMotorUI3);
            this.tabPage1.Controls.Add(this.vsTouchMotorUI2);
            this.tabPage1.Controls.Add(this.vsTouchMotorUI1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1239, 573);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Axis0~2";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // vsTouchMotorUI3
            // 
            this.vsTouchMotorUI3.BackColor = System.Drawing.SystemColors.Control;
            this.vsTouchMotorUI3.Location = new System.Drawing.Point(823, 0);
            this.vsTouchMotorUI3.Name = "vsTouchMotorUI3";
            this.vsTouchMotorUI3.Size = new System.Drawing.Size(415, 573);
            this.vsTouchMotorUI3.TabIndex = 2;
            // 
            // vsTouchMotorUI2
            // 
            this.vsTouchMotorUI2.BackColor = System.Drawing.SystemColors.Control;
            this.vsTouchMotorUI2.Location = new System.Drawing.Point(412, 0);
            this.vsTouchMotorUI2.Name = "vsTouchMotorUI2";
            this.vsTouchMotorUI2.Size = new System.Drawing.Size(415, 573);
            this.vsTouchMotorUI2.TabIndex = 1;
            // 
            // vsTouchMotorUI1
            // 
            this.vsTouchMotorUI1.BackColor = System.Drawing.SystemColors.Control;
            this.vsTouchMotorUI1.Location = new System.Drawing.Point(0, 0);
            this.vsTouchMotorUI1.Name = "vsTouchMotorUI1";
            this.vsTouchMotorUI1.Size = new System.Drawing.Size(415, 573);
            this.vsTouchMotorUI1.TabIndex = 0;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.vsTouchMotorUI4);
            this.tabPage2.Controls.Add(this.vsTouchMotorUI5);
            this.tabPage2.Controls.Add(this.vsTouchMotorUI6);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1239, 573);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Axis3~5";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // vsTouchMotorUI4
            // 
            this.vsTouchMotorUI4.BackColor = System.Drawing.SystemColors.Control;
            this.vsTouchMotorUI4.Location = new System.Drawing.Point(823, 0);
            this.vsTouchMotorUI4.Name = "vsTouchMotorUI4";
            this.vsTouchMotorUI4.Size = new System.Drawing.Size(415, 573);
            this.vsTouchMotorUI4.TabIndex = 5;
            // 
            // vsTouchMotorUI5
            // 
            this.vsTouchMotorUI5.BackColor = System.Drawing.SystemColors.Control;
            this.vsTouchMotorUI5.Location = new System.Drawing.Point(412, 0);
            this.vsTouchMotorUI5.Name = "vsTouchMotorUI5";
            this.vsTouchMotorUI5.Size = new System.Drawing.Size(415, 573);
            this.vsTouchMotorUI5.TabIndex = 4;
            // 
            // vsTouchMotorUI6
            // 
            this.vsTouchMotorUI6.BackColor = System.Drawing.SystemColors.Control;
            this.vsTouchMotorUI6.Location = new System.Drawing.Point(0, 0);
            this.vsTouchMotorUI6.Name = "vsTouchMotorUI6";
            this.vsTouchMotorUI6.Size = new System.Drawing.Size(415, 573);
            this.vsTouchMotorUI6.TabIndex = 3;
            // 
            // FormMotor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1247, 599);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.Name = "FormMotor";
            this.Text = "FormMotor";
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private Common.VsTouchMotorUI vsTouchMotorUI1;
        private System.Windows.Forms.TabPage tabPage2;
        private Common.VsTouchMotorUI vsTouchMotorUI3;
        private Common.VsTouchMotorUI vsTouchMotorUI2;
        private Common.VsTouchMotorUI vsTouchMotorUI4;
        private Common.VsTouchMotorUI vsTouchMotorUI5;
        private Common.VsTouchMotorUI vsTouchMotorUI6;
    }
}