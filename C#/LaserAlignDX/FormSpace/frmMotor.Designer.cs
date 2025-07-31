namespace Traveller106.FormSpace
{
    partial class frmMotor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMotor));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.vsTouchMotorUI2 = new Common.VsTouchMotorUI();
            this.vsTouchMotorUI3 = new Common.VsTouchMotorUI();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.vsTouchMotorUI1 = new Common.VsTouchMotorUI();
            this.vsTouchMotorUI4 = new Common.VsTouchMotorUI();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnTopMost = new System.Windows.Forms.ToolStripButton();
            this.btnExit = new System.Windows.Forms.ToolStripButton();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.ItemSize = new System.Drawing.Size(100, 30);
            this.tabControl1.Location = new System.Drawing.Point(0, 26);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(4);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1699, 789);
            this.tabControl1.SizeMode = System.Windows.Forms.TabSizeMode.FillToRight;
            this.tabControl1.TabIndex = 4;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.vsTouchMotorUI2);
            this.tabPage1.Controls.Add(this.vsTouchMotorUI3);
            this.tabPage1.Location = new System.Drawing.Point(4, 34);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage1.Size = new System.Drawing.Size(1691, 751);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "AXIS012";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // vsTouchMotorUI2
            // 
            this.vsTouchMotorUI2.BackColor = System.Drawing.SystemColors.Control;
            this.vsTouchMotorUI2.Location = new System.Drawing.Point(568, 16);
            this.vsTouchMotorUI2.Margin = new System.Windows.Forms.Padding(5);
            this.vsTouchMotorUI2.Name = "vsTouchMotorUI2";
            this.vsTouchMotorUI2.Size = new System.Drawing.Size(553, 716);
            this.vsTouchMotorUI2.TabIndex = 7;
            // 
            // vsTouchMotorUI3
            // 
            this.vsTouchMotorUI3.BackColor = System.Drawing.SystemColors.Control;
            this.vsTouchMotorUI3.Location = new System.Drawing.Point(7, 16);
            this.vsTouchMotorUI3.Margin = new System.Windows.Forms.Padding(5);
            this.vsTouchMotorUI3.Name = "vsTouchMotorUI3";
            this.vsTouchMotorUI3.Size = new System.Drawing.Size(553, 716);
            this.vsTouchMotorUI3.TabIndex = 6;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.vsTouchMotorUI1);
            this.tabPage2.Controls.Add(this.vsTouchMotorUI4);
            this.tabPage2.Location = new System.Drawing.Point(4, 34);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Size = new System.Drawing.Size(1691, 751);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // vsTouchMotorUI1
            // 
            this.vsTouchMotorUI1.BackColor = System.Drawing.SystemColors.Control;
            this.vsTouchMotorUI1.Location = new System.Drawing.Point(568, 16);
            this.vsTouchMotorUI1.Margin = new System.Windows.Forms.Padding(5);
            this.vsTouchMotorUI1.Name = "vsTouchMotorUI1";
            this.vsTouchMotorUI1.Size = new System.Drawing.Size(553, 716);
            this.vsTouchMotorUI1.TabIndex = 9;
            // 
            // vsTouchMotorUI4
            // 
            this.vsTouchMotorUI4.BackColor = System.Drawing.SystemColors.Control;
            this.vsTouchMotorUI4.Location = new System.Drawing.Point(7, 16);
            this.vsTouchMotorUI4.Margin = new System.Windows.Forms.Padding(5);
            this.vsTouchMotorUI4.Name = "vsTouchMotorUI4";
            this.vsTouchMotorUI4.Size = new System.Drawing.Size(553, 716);
            this.vsTouchMotorUI4.TabIndex = 7;
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnTopMost,
            this.btnExit});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1699, 26);
            this.toolStrip1.TabIndex = 5;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // btnTopMost
            // 
            this.btnTopMost.Image = ((System.Drawing.Image)(resources.GetObject("btnTopMost.Image")));
            this.btnTopMost.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnTopMost.Name = "btnTopMost";
            this.btnTopMost.Size = new System.Drawing.Size(63, 24);
            this.btnTopMost.Text = "置顶";
            this.btnTopMost.Visible = false;
            // 
            // btnExit
            // 
            this.btnExit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.btnExit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnExit.Image = ((System.Drawing.Image)(resources.GetObject("btnExit.Image")));
            this.btnExit.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(43, 23);
            this.btnExit.Text = "退出";
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click_1);
            // 
            // frmMotor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1699, 815);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.toolStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "frmMotor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "frmMotor";
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private Common.VsTouchMotorUI vsTouchMotorUI2;
        private Common.VsTouchMotorUI vsTouchMotorUI3;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnTopMost;
        private System.Windows.Forms.ToolStripButton btnExit;
        private System.Windows.Forms.TabPage tabPage2;
        private Common.VsTouchMotorUI vsTouchMotorUI4;
        private Common.VsTouchMotorUI vsTouchMotorUI1;
    }
}