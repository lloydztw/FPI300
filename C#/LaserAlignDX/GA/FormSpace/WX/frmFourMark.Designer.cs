namespace LaserAlignDX.FormSpace.WX
{
    partial class frmFourMark
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.pgPara = new System.Windows.Forms.PropertyGrid();
            this.panel2 = new System.Windows.Forms.Panel();
            this.button3 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.cboMarkIndex = new System.Windows.Forms.ComboBox();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.DS = new JzDisplay.UISpace.DispUI();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.pgPara);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(349, 641);
            this.panel1.TabIndex = 0;
            // 
            // pgPara
            // 
            this.pgPara.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pgPara.Location = new System.Drawing.Point(0, 174);
            this.pgPara.Name = "pgPara";
            this.pgPara.Size = new System.Drawing.Size(349, 467);
            this.pgPara.TabIndex = 1;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.button3);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Controls.Add(this.cboMarkIndex);
            this.panel2.Controls.Add(this.button2);
            this.panel2.Controls.Add(this.button1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(349, 174);
            this.panel2.TabIndex = 0;
            // 
            // button3
            // 
            this.button3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.button3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button3.Location = new System.Drawing.Point(12, 143);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(90, 25);
            this.button3.TabIndex = 46;
            this.button3.Text = "保存参数";
            this.button3.UseVisualStyleBackColor = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 12);
            this.label1.TabIndex = 45;
            this.label1.Text = "定位点(Mark)";
            // 
            // cboMarkIndex
            // 
            this.cboMarkIndex.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboMarkIndex.FormattingEnabled = true;
            this.cboMarkIndex.Items.AddRange(new object[] {
            "Mark1",
            "Mark2",
            "Mark3",
            "Mark4"});
            this.cboMarkIndex.Location = new System.Drawing.Point(14, 30);
            this.cboMarkIndex.Name = "cboMarkIndex";
            this.cboMarkIndex.Size = new System.Drawing.Size(121, 20);
            this.cboMarkIndex.TabIndex = 44;
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button2.Location = new System.Drawing.Point(12, 97);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(90, 25);
            this.button2.TabIndex = 43;
            this.button2.Text = "检查";
            this.button2.UseVisualStyleBackColor = false;
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button1.Location = new System.Drawing.Point(12, 66);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(90, 25);
            this.button1.TabIndex = 42;
            this.button1.Text = "获取图像";
            this.button1.UseVisualStyleBackColor = false;
            // 
            // DS
            // 
            this.DS.Cursor = System.Windows.Forms.Cursors.Default;
            this.DS.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DS.Location = new System.Drawing.Point(349, 0);
            this.DS.Name = "DS";
            this.DS.Size = new System.Drawing.Size(612, 641);
            this.DS.TabIndex = 1;
            // 
            // frmFourMark
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(961, 641);
            this.Controls.Add(this.DS);
            this.Controls.Add(this.panel1);
            this.Name = "frmFourMark";
            this.Text = "frmFourMark";
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PropertyGrid pgPara;
        private System.Windows.Forms.Panel panel2;
        private JzDisplay.UISpace.DispUI DS;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cboMarkIndex;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
    }
}