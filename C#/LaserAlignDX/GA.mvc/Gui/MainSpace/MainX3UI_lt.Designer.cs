using LaserAlignDX.Mvc.Gui;
using LaserAlignDX.UISpace.ChipCellsViewer;

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
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.lblFlySerialNumber = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.lblMemoryUsage = new System.Windows.Forms.Label();
            this.mvsui2 = new LaserAlignDX.UISpace.ChipCellsViewer.JezChipCellsViewPanel();
            this.mvsui1 = new LaserAlignDX.UISpace.ChipCellsViewer.JezChipCellsViewPanel();
            this.DSFly0 = new LaserAlignDX.Mvc.Gui.JezFlyViewPanel();
            this.DSFly1 = new LaserAlignDX.Mvc.Gui.JezFlyViewPanel();
            this.DSFly2 = new LaserAlignDX.Mvc.Gui.JezFlyViewPanel();
            this.DSFly3 = new LaserAlignDX.Mvc.Gui.JezFlyViewPanel();
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
            this.tabControl1.Margin = new System.Windows.Forms.Padding(0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(690, 999);
            this.tabControl1.TabIndex = 2;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.tableLayoutPanel2);
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(0);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Size = new System.Drawing.Size(682, 970);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "主界面";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.mvsui2, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.mvsui1, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(682, 970);
            this.tableLayoutPanel2.TabIndex = 21;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.groupBox1);
            this.tabPage2.Location = new System.Drawing.Point(4, 25);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.tabPage2.Size = new System.Drawing.Size(682, 970);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "日志";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.richTextBox1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 8);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox1.Size = new System.Drawing.Size(682, 962);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "日志记录";
            // 
            // richTextBox1
            // 
            this.richTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBox1.Location = new System.Drawing.Point(4, 22);
            this.richTextBox1.Margin = new System.Windows.Forms.Padding(4);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(674, 936);
            this.richTextBox1.TabIndex = 0;
            this.richTextBox1.Text = "";
            // 
            // lblFlySerialNumber
            // 
            this.lblFlySerialNumber.AutoSize = true;
            this.lblFlySerialNumber.BackColor = System.Drawing.Color.Transparent;
            this.lblFlySerialNumber.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFlySerialNumber.Font = new System.Drawing.Font("微軟正黑體", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblFlySerialNumber.Location = new System.Drawing.Point(3, 1);
            this.lblFlySerialNumber.Margin = new System.Windows.Forms.Padding(3, 1, 3, 0);
            this.lblFlySerialNumber.Name = "lblFlySerialNumber";
            this.lblFlySerialNumber.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.lblFlySerialNumber.Size = new System.Drawing.Size(330, 21);
            this.lblFlySerialNumber.TabIndex = 20;
            this.lblFlySerialNumber.Text = "飛拍序號:";
            this.lblFlySerialNumber.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.lblMemoryUsage, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.lblFlySerialNumber, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.DSFly0, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.DSFly1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.DSFly2, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.DSFly3, 0, 4);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(690, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(1);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.tableLayoutPanel1.RowCount = 6;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(336, 999);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // lblMemoryUsage
            // 
            this.lblMemoryUsage.BackColor = System.Drawing.Color.Black;
            this.lblMemoryUsage.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblMemoryUsage.Font = new System.Drawing.Font("微軟正黑體", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblMemoryUsage.ForeColor = System.Drawing.Color.Lime;
            this.lblMemoryUsage.Location = new System.Drawing.Point(3, 954);
            this.lblMemoryUsage.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.lblMemoryUsage.Name = "lblMemoryUsage";
            this.lblMemoryUsage.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.lblMemoryUsage.Size = new System.Drawing.Size(330, 38);
            this.lblMemoryUsage.TabIndex = 22;
            this.lblMemoryUsage.Text = "資源使用: 0%";
            this.lblMemoryUsage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // mvsui2
            // 
            this.mvsui2.CarrierID = LaserAlignDX.CarrierEnum.C1;
            this.mvsui2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mvsui2.IsActive = false;
            this.mvsui2.Location = new System.Drawing.Point(342, 1);
            this.mvsui2.Margin = new System.Windows.Forms.Padding(1);
            this.mvsui2.Name = "mvsui2";
            this.mvsui2.Size = new System.Drawing.Size(339, 968);
            this.mvsui2.TabIndex = 3;
            // 
            // mvsui1
            // 
            this.mvsui1.BackColor = System.Drawing.Color.Black;
            this.mvsui1.CarrierID = LaserAlignDX.CarrierEnum.C1;
            this.mvsui1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mvsui1.IsActive = false;
            this.mvsui1.Location = new System.Drawing.Point(1, 1);
            this.mvsui1.Margin = new System.Windows.Forms.Padding(1);
            this.mvsui1.Name = "mvsui1";
            this.mvsui1.Size = new System.Drawing.Size(339, 968);
            this.mvsui1.TabIndex = 2;
            // 
            // DSFly0
            // 
            this.DSFly0.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DSFly0.Location = new System.Drawing.Point(5, 24);
            this.DSFly0.Margin = new System.Windows.Forms.Padding(5, 2, 2, 2);
            this.DSFly0.Name = "DSFly0";
            this.DSFly0.Size = new System.Drawing.Size(329, 229);
            this.DSFly0.TabIndex = 1;
            // 
            // DSFly1
            // 
            this.DSFly1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DSFly1.Location = new System.Drawing.Point(2, 257);
            this.DSFly1.Margin = new System.Windows.Forms.Padding(2);
            this.DSFly1.Name = "DSFly1";
            this.DSFly1.Size = new System.Drawing.Size(332, 229);
            this.DSFly1.TabIndex = 2;
            // 
            // DSFly2
            // 
            this.DSFly2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DSFly2.Location = new System.Drawing.Point(2, 490);
            this.DSFly2.Margin = new System.Windows.Forms.Padding(2);
            this.DSFly2.Name = "DSFly2";
            this.DSFly2.Size = new System.Drawing.Size(332, 229);
            this.DSFly2.TabIndex = 3;
            // 
            // DSFly3
            // 
            this.DSFly3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DSFly3.Location = new System.Drawing.Point(2, 723);
            this.DSFly3.Margin = new System.Windows.Forms.Padding(2, 2, 2, 0);
            this.DSFly3.Name = "DSFly3";
            this.DSFly3.Size = new System.Drawing.Size(332, 231);
            this.DSFly3.TabIndex = 4;
            // 
            // MainX3UI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Silver;
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "MainX3UI";
            this.Size = new System.Drawing.Size(1026, 999);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private JezFlyViewPanel DSFly0;
        private JezFlyViewPanel DSFly1;
        private JezFlyViewPanel DSFly2;
        private JezFlyViewPanel DSFly3;
        //private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Label lblFlySerialNumber;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private JezChipCellsViewPanel mvsui2;
        private JezChipCellsViewPanel mvsui1;
        private System.Windows.Forms.Label lblMemoryUsage;
    }
}
