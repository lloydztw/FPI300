namespace AX.Gui
{
    partial class GvPaneMotorJogXY
    {
        /// <summary> 
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置受控資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 元件設計工具產生的程式碼

        /// <summary> 
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改
        /// 這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.gwMotorAxisStatusCmd1 = new AX.Gui.GwMotorAxisStatusCmd();
            this.gwMotorAxisStatusCmd2 = new AX.Gui.GwMotorAxisStatusCmd();
            this.gwMotorJogSticks1 = new AX.Gui.GwMotorJogSticks();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.gwMotorAxisStatusCmd1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.gwMotorAxisStatusCmd2, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 8);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(470, 484);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Transparent;
            this.panel1.Controls.Add(this.gwMotorJogSticks1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel1.Location = new System.Drawing.Point(470, 8);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(0, 12, 0, 0);
            this.panel1.Size = new System.Drawing.Size(220, 484);
            this.panel1.TabIndex = 5;
            // 
            // gwMotorAxisStatusCmd1
            // 
            this.gwMotorAxisStatusCmd1.AxisName = "X Axis Status";
            this.gwMotorAxisStatusCmd1.BackColor = System.Drawing.Color.Transparent;
            this.gwMotorAxisStatusCmd1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gwMotorAxisStatusCmd1.Location = new System.Drawing.Point(16, 8);
            this.gwMotorAxisStatusCmd1.Margin = new System.Windows.Forms.Padding(16, 8, 16, 5);
            this.gwMotorAxisStatusCmd1.Name = "gwMotorAxisStatusCmd1";
            this.gwMotorAxisStatusCmd1.Padding = new System.Windows.Forms.Padding(2);
            this.gwMotorAxisStatusCmd1.Size = new System.Drawing.Size(438, 229);
            this.gwMotorAxisStatusCmd1.TabIndex = 0;
            // 
            // gwMotorAxisStatusCmd2
            // 
            this.gwMotorAxisStatusCmd2.AxisName = "Y Axis Status";
            this.gwMotorAxisStatusCmd2.BackColor = System.Drawing.Color.Transparent;
            this.gwMotorAxisStatusCmd2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gwMotorAxisStatusCmd2.Location = new System.Drawing.Point(16, 247);
            this.gwMotorAxisStatusCmd2.Margin = new System.Windows.Forms.Padding(16, 5, 16, 8);
            this.gwMotorAxisStatusCmd2.Name = "gwMotorAxisStatusCmd2";
            this.gwMotorAxisStatusCmd2.Padding = new System.Windows.Forms.Padding(2);
            this.gwMotorAxisStatusCmd2.Size = new System.Drawing.Size(438, 229);
            this.gwMotorAxisStatusCmd2.TabIndex = 1;
            // 
            // gwMotorJogSticks1
            // 
            this.gwMotorJogSticks1.BackColor = System.Drawing.Color.Transparent;
            this.gwMotorJogSticks1.Dock = System.Windows.Forms.DockStyle.Top;
            this.gwMotorJogSticks1.JogStickOption = AX.Gui.JogStickOption.XY;
            this.gwMotorJogSticks1.Location = new System.Drawing.Point(0, 12);
            this.gwMotorJogSticks1.Name = "gwMotorJogSticks1";
            this.gwMotorJogSticks1.Size = new System.Drawing.Size(220, 220);
            this.gwMotorJogSticks1.TabIndex = 4;
            // 
            // GvPaneMotorJogXY
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Gray;
            this.BackgroundImage = global::EzMotor.Gui.Properties.Resources.CommonPanel;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.panel1);
            this.Name = "GvPaneMotorJogXY";
            this.Padding = new System.Windows.Forms.Padding(0, 8, 12, 8);
            this.Size = new System.Drawing.Size(702, 500);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private GwMotorAxisStatusCmd gwMotorAxisStatusCmd1;
        private GwMotorAxisStatusCmd gwMotorAxisStatusCmd2;
        private System.Windows.Forms.Panel panel1;
        public GwMotorJogSticks gwMotorJogSticks1;
    }
}
