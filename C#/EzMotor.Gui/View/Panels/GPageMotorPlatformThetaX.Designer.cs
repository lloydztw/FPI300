namespace AX.Gui
{
    partial class GPageMotorPlatformThetaX
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
            this.gwMotorAxisStatusAndCtrl1 = new AX.Gui.GPaneMotorAxisStatusAndCtrl();
            this.gwMotorAxisStatusAndCtrl2 = new AX.Gui.GPaneMotorAxisStatusAndCtrl();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.gwMotorAxisStatusAndCtrl1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.gwMotorAxisStatusAndCtrl2, 0, 1);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(530, 700);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // gwMotorAxisStatusAndCtrl1
            // 
            this.gwMotorAxisStatusAndCtrl1.AxisName = "X";
            this.gwMotorAxisStatusAndCtrl1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.gwMotorAxisStatusAndCtrl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gwMotorAxisStatusAndCtrl1.Location = new System.Drawing.Point(3, 3);
            this.gwMotorAxisStatusAndCtrl1.Name = "gwMotorAxisStatusAndCtrl1";
            this.gwMotorAxisStatusAndCtrl1.Option = AX.Gui.JogStickOption.XY;
            this.gwMotorAxisStatusAndCtrl1.Size = new System.Drawing.Size(524, 344);
            this.gwMotorAxisStatusAndCtrl1.TabIndex = 3;
            // 
            // gwMotorAxisStatusAndCtrl2
            // 
            this.gwMotorAxisStatusAndCtrl2.AxisName = "θ";
            this.gwMotorAxisStatusAndCtrl2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.gwMotorAxisStatusAndCtrl2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gwMotorAxisStatusAndCtrl2.Location = new System.Drawing.Point(3, 353);
            this.gwMotorAxisStatusAndCtrl2.Name = "gwMotorAxisStatusAndCtrl2";
            this.gwMotorAxisStatusAndCtrl2.Option = AX.Gui.JogStickOption.XY;
            this.gwMotorAxisStatusAndCtrl2.Size = new System.Drawing.Size(524, 344);
            this.gwMotorAxisStatusAndCtrl2.TabIndex = 4;
            // 
            // GPageMotorPlatformThetaX
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Silver;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "GPageMotorPlatformThetaX";
            this.Size = new System.Drawing.Size(597, 700);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        public GPaneMotorAxisStatusAndCtrl gwMotorAxisStatusAndCtrl1;
        public GPaneMotorAxisStatusAndCtrl gwMotorAxisStatusAndCtrl2;
    }
}
