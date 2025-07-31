namespace LaserAlignDX.UISpace.MainSpace
{
    partial class MainX3UI
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

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.label1 = new System.Windows.Forms.Label();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.mvsui2 = new LaserAlignDX.UISpace.UIMVC.MVSUI();
            this.mvsui1 = new LaserAlignDX.UISpace.UIMVC.MVSUI();
            this.button6 = new System.Windows.Forms.Button();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.DSFly0 = new LaserAlignDX.UISpace.UIMVC.MVSUI();
            this.DSFly1 = new LaserAlignDX.UISpace.UIMVC.MVSUI();
            this.DSFly2 = new LaserAlignDX.UISpace.UIMVC.MVSUI();
            this.DSFly3 = new LaserAlignDX.UISpace.UIMVC.MVSUI();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
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
            this.tabControl1.Size = new System.Drawing.Size(794, 896);
            this.tabControl1.TabIndex = 2;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Controls.Add(this.tableLayoutPanel2);
            this.tabPage1.Controls.Add(this.button6);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(786, 870);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "主界面";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.label1.Location = new System.Drawing.Point(3, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 12);
            this.label1.TabIndex = 20;
            this.label1.Text = "飞拍序号:";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.mvsui2, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.mvsui1, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(780, 864);
            this.tableLayoutPanel2.TabIndex = 21;
            // 
            // mvsui2
            // 
            this.mvsui2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mvsui2.Location = new System.Drawing.Point(393, 3);
            this.mvsui2.Name = "mvsui2";
            this.mvsui2.Size = new System.Drawing.Size(384, 858);
            this.mvsui2.TabIndex = 3;
            // 
            // mvsui1
            // 
            this.mvsui1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mvsui1.Location = new System.Drawing.Point(3, 3);
            this.mvsui1.Name = "mvsui1";
            this.mvsui1.Size = new System.Drawing.Size(384, 858);
            this.mvsui1.TabIndex = 2;
            // 
            // button6
            // 
            this.button6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.button6.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button6.Location = new System.Drawing.Point(600, 829);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(64, 38);
            this.button6.TabIndex = 18;
            this.button6.Text = "测试准备";
            this.button6.UseVisualStyleBackColor = false;
            this.button6.Visible = false;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.groupBox1);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Size = new System.Drawing.Size(786, 870);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "日志";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.richTextBox1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(786, 870);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "日志记录";
            // 
            // richTextBox1
            // 
            this.richTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBox1.Location = new System.Drawing.Point(3, 17);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(780, 850);
            this.richTextBox1.TabIndex = 0;
            this.richTextBox1.Text = "";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.DSFly0, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.DSFly1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.DSFly2, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.DSFly3, 0, 3);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(794, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(252, 896);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // DSFly0
            // 
            this.DSFly0.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DSFly0.Location = new System.Drawing.Point(3, 3);
            this.DSFly0.Name = "DSFly0";
            this.DSFly0.Size = new System.Drawing.Size(246, 218);
            this.DSFly0.TabIndex = 1;
            // 
            // DSFly1
            // 
            this.DSFly1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DSFly1.Location = new System.Drawing.Point(3, 227);
            this.DSFly1.Name = "DSFly1";
            this.DSFly1.Size = new System.Drawing.Size(246, 218);
            this.DSFly1.TabIndex = 2;
            // 
            // DSFly2
            // 
            this.DSFly2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DSFly2.Location = new System.Drawing.Point(3, 451);
            this.DSFly2.Name = "DSFly2";
            this.DSFly2.Size = new System.Drawing.Size(246, 218);
            this.DSFly2.TabIndex = 3;
            // 
            // DSFly3
            // 
            this.DSFly3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DSFly3.Location = new System.Drawing.Point(3, 675);
            this.DSFly3.Name = "DSFly3";
            this.DSFly3.Size = new System.Drawing.Size(246, 218);
            this.DSFly3.TabIndex = 4;
            // 
            // MainX3UI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "MainX3UI";
            this.Size = new System.Drawing.Size(1046, 896);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private UIMVC.MVSUI DSFly0;
        private UIMVC.MVSUI DSFly1;
        private UIMVC.MVSUI DSFly2;
        private UIMVC.MVSUI DSFly3;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private UIMVC.MVSUI mvsui2;
        private UIMVC.MVSUI mvsui1;
    }
}
