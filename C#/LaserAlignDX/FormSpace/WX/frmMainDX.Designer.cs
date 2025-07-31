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
            this.runUI1 = new PhotoMachine.UISpace.RunUI();
            this.essUI1 = new JetEazy.UISpace.EssUI();
            this.iniUI1 = new PhotoMachine.UISpace.IniUI();
            this.ctrlUI1 = new PhotoMachine.UISpace.CtrlUI();
            this.rcpUI1 = new PhotoMachine.UISpace.RcpUI();
            this.mainControlUI1 = new Eazy_Project_III.UISpace.MainControlUI();
            this.SuspendLayout();
            // 
            // runUI1
            // 
            this.runUI1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.runUI1.Location = new System.Drawing.Point(1052, 239);
            this.runUI1.Name = "runUI1";
            this.runUI1.Size = new System.Drawing.Size(228, 417);
            this.runUI1.TabIndex = 0;
            // 
            // essUI1
            // 
            this.essUI1.BackColor = System.Drawing.SystemColors.Control;
            this.essUI1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.essUI1.Location = new System.Drawing.Point(1052, 0);
            this.essUI1.Name = "essUI1";
            this.essUI1.RunWatchTime = 30;
            this.essUI1.Size = new System.Drawing.Size(226, 238);
            this.essUI1.TabIndex = 1;
            // 
            // iniUI1
            // 
            this.iniUI1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.iniUI1.Location = new System.Drawing.Point(818, 239);
            this.iniUI1.Name = "iniUI1";
            this.iniUI1.Size = new System.Drawing.Size(228, 417);
            this.iniUI1.TabIndex = 2;
            // 
            // ctrlUI1
            // 
            this.ctrlUI1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.ctrlUI1.Location = new System.Drawing.Point(1052, 657);
            this.ctrlUI1.Name = "ctrlUI1";
            this.ctrlUI1.Size = new System.Drawing.Size(223, 239);
            this.ctrlUI1.TabIndex = 3;
            // 
            // rcpUI1
            // 
            this.rcpUI1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.rcpUI1.Location = new System.Drawing.Point(584, 239);
            this.rcpUI1.Name = "rcpUI1";
            this.rcpUI1.Size = new System.Drawing.Size(228, 417);
            this.rcpUI1.TabIndex = 4;
            // 
            // mainControlUI1
            // 
            this.mainControlUI1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.mainControlUI1.Location = new System.Drawing.Point(0, 0);
            this.mainControlUI1.Name = "mainControlUI1";
            this.mainControlUI1.Size = new System.Drawing.Size(1046, 896);
            this.mainControlUI1.TabIndex = 5;
            // 
            // frmMainDX
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1280, 900);
            this.Controls.Add(this.rcpUI1);
            this.Controls.Add(this.iniUI1);
            this.Controls.Add(this.mainControlUI1);
            this.Controls.Add(this.ctrlUI1);
            this.Controls.Add(this.essUI1);
            this.Controls.Add(this.runUI1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
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

