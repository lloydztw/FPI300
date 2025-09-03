namespace Traveller106
{
    partial class frmMainDX
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMainDX));
            this.essUI1 = new JetEazy.UISpace.EssUI();
            this.rcpUI1 = new PhotoMachine.UISpace.RcpUI();
            this.iniUI1 = new PhotoMachine.UISpace.IniUI();
            this.mainControlUI1 = new Eazy_Project_III.UISpace.MainControlUI();
            this.ctrlUI1 = new PhotoMachine.UISpace.CtrlUI();
            this.runUI1 = new PhotoMachine.UISpace.RunUI();
            this.SuspendLayout();
            // 
            // essUI1
            // 
            this.essUI1.BackColor = System.Drawing.SystemColors.Control;
            this.essUI1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.essUI1.Location = new System.Drawing.Point(1403, 0);
            this.essUI1.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.essUI1.Name = "essUI1";
            this.essUI1.RunWatchTime = 30;
            this.essUI1.Size = new System.Drawing.Size(301, 327);
            this.essUI1.TabIndex = 1;
            // 
            // rcpUI1
            // 
            this.rcpUI1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.rcpUI1.Location = new System.Drawing.Point(779, 326);
            this.rcpUI1.Margin = new System.Windows.Forms.Padding(5);
            this.rcpUI1.Name = "rcpUI1";
            this.rcpUI1.Size = new System.Drawing.Size(303, 521);
            this.rcpUI1.TabIndex = 4;
            // 
            // iniUI1
            // 
            this.iniUI1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.iniUI1.Location = new System.Drawing.Point(1091, 326);
            this.iniUI1.Margin = new System.Windows.Forms.Padding(5);
            this.iniUI1.Name = "iniUI1";
            this.iniUI1.Size = new System.Drawing.Size(303, 521);
            this.iniUI1.TabIndex = 2;
            // 
            // mainControlUI1
            // 
            this.mainControlUI1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.mainControlUI1.Location = new System.Drawing.Point(0, 0);
            this.mainControlUI1.Margin = new System.Windows.Forms.Padding(5);
            this.mainControlUI1.Name = "mainControlUI1";
            this.mainControlUI1.Size = new System.Drawing.Size(1395, 1120);
            this.mainControlUI1.TabIndex = 5;
            // 
            // ctrlUI1
            // 
            this.ctrlUI1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.ctrlUI1.Location = new System.Drawing.Point(1403, 850);
            this.ctrlUI1.Margin = new System.Windows.Forms.Padding(5);
            this.ctrlUI1.Name = "ctrlUI1";
            this.ctrlUI1.Size = new System.Drawing.Size(297, 270);
            this.ctrlUI1.TabIndex = 3;
            // 
            // runUI1
            // 
            this.runUI1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.runUI1.Location = new System.Drawing.Point(1403, 329);
            this.runUI1.Margin = new System.Windows.Forms.Padding(5);
            this.runUI1.Name = "runUI1";
            this.runUI1.Size = new System.Drawing.Size(303, 521);
            this.runUI1.TabIndex = 0;
            // 
            // frmMainDX
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1707, 1125);
            this.Controls.Add(this.rcpUI1);
            this.Controls.Add(this.iniUI1);
            this.Controls.Add(this.mainControlUI1);
            this.Controls.Add(this.ctrlUI1);
            this.Controls.Add(this.essUI1);
            this.Controls.Add(this.runUI1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "frmMainDX";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private PhotoMachine.UISpace.RunUI runUI1;
        private JetEazy.UISpace.EssUI essUI1;
        private PhotoMachine.UISpace.IniUI iniUI1;
        private PhotoMachine.UISpace.CtrlUI ctrlUI1;
        private PhotoMachine.UISpace.RcpUI rcpUI1;
        private Eazy_Project_III.UISpace.MainControlUI mainControlUI1;
    }
}

