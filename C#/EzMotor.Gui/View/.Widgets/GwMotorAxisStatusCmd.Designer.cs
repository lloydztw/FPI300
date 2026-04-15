namespace AX.Gui
{
    partial class GwMotorAxisStatusCmd
    {
        /// <summary> 
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置 Managed 資源則為 true，否則為 false。</param>
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
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器
        /// 修改這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.panel1 = new System.Windows.Forms.Panel();
            this.numDist = new System.Windows.Forms.NumericUpDown();
            this.lblDist = new System.Windows.Forms.Label();
            this.lblUnit = new System.Windows.Forms.Label();
            this.tableLayoutPanelB = new System.Windows.Forms.TableLayoutPanel();
            this.btnMoveDeltaN = new System.Windows.Forms.Button();
            this.btnMoveTo = new System.Windows.Forms.Button();
            this.btnMoveDeltaP = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.gwMotorAxisStatus1 = new AX.Gui.GwMotorAxisStatus();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numDist)).BeginInit();
            this.tableLayoutPanelB.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Silver;
            this.panel1.Controls.Add(this.numDist);
            this.panel1.Controls.Add(this.lblDist);
            this.panel1.Controls.Add(this.lblUnit);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 122);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.panel1.Size = new System.Drawing.Size(492, 43);
            this.panel1.TabIndex = 201;
            // 
            // numDist
            // 
            this.numDist.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.numDist.DecimalPlaces = 3;
            this.numDist.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.numDist.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.numDist.Location = new System.Drawing.Point(98, 7);
            this.numDist.Margin = new System.Windows.Forms.Padding(4);
            this.numDist.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numDist.Minimum = new decimal(new int[] {
            10000,
            0,
            0,
            -2147483648});
            this.numDist.Name = "numDist";
            this.numDist.Size = new System.Drawing.Size(303, 32);
            this.numDist.TabIndex = 206;
            this.numDist.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // lblDist
            // 
            this.lblDist.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblDist.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblDist.Location = new System.Drawing.Point(3, 7);
            this.lblDist.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDist.Name = "lblDist";
            this.lblDist.Size = new System.Drawing.Size(87, 32);
            this.lblDist.TabIndex = 207;
            this.lblDist.Text = "Dis=";
            this.lblDist.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblUnit
            // 
            this.lblUnit.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblUnit.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblUnit.Location = new System.Drawing.Point(401, 7);
            this.lblUnit.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblUnit.Name = "lblUnit";
            this.lblUnit.Size = new System.Drawing.Size(88, 32);
            this.lblUnit.TabIndex = 208;
            this.lblUnit.Text = "mm";
            this.lblUnit.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tableLayoutPanelB
            // 
            this.tableLayoutPanelB.BackColor = System.Drawing.Color.Silver;
            this.tableLayoutPanelB.ColumnCount = 3;
            this.tableLayoutPanelB.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.tableLayoutPanelB.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 34F));
            this.tableLayoutPanelB.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.tableLayoutPanelB.Controls.Add(this.btnMoveDeltaN, 0, 0);
            this.tableLayoutPanelB.Controls.Add(this.btnMoveTo, 1, 0);
            this.tableLayoutPanelB.Controls.Add(this.btnMoveDeltaP, 2, 0);
            this.tableLayoutPanelB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelB.Location = new System.Drawing.Point(0, 165);
            this.tableLayoutPanelB.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanelB.Name = "tableLayoutPanelB";
            this.tableLayoutPanelB.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tableLayoutPanelB.RowCount = 1;
            this.tableLayoutPanelB.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelB.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelB.Size = new System.Drawing.Size(492, 52);
            this.tableLayoutPanelB.TabIndex = 205;
            // 
            // btnMoveDeltaN
            // 
            this.btnMoveDeltaN.BackColor = System.Drawing.SystemColors.Control;
            this.btnMoveDeltaN.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnMoveDeltaN.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnMoveDeltaN.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMoveDeltaN.Location = new System.Drawing.Point(7, 6);
            this.btnMoveDeltaN.Margin = new System.Windows.Forms.Padding(4);
            this.btnMoveDeltaN.Name = "btnMoveDeltaN";
            this.btnMoveDeltaN.Size = new System.Drawing.Size(152, 40);
            this.btnMoveDeltaN.TabIndex = 12;
            this.btnMoveDeltaN.Text = "-";
            this.btnMoveDeltaN.UseVisualStyleBackColor = false;
            // 
            // btnMoveTo
            // 
            this.btnMoveTo.BackColor = System.Drawing.SystemColors.Control;
            this.btnMoveTo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnMoveTo.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnMoveTo.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMoveTo.Location = new System.Drawing.Point(167, 6);
            this.btnMoveTo.Margin = new System.Windows.Forms.Padding(4);
            this.btnMoveTo.Name = "btnMoveTo";
            this.btnMoveTo.Size = new System.Drawing.Size(157, 40);
            this.btnMoveTo.TabIndex = 13;
            this.btnMoveTo.Text = "定位至";
            this.btnMoveTo.UseVisualStyleBackColor = false;
            // 
            // btnMoveDeltaP
            // 
            this.btnMoveDeltaP.BackColor = System.Drawing.SystemColors.Control;
            this.btnMoveDeltaP.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnMoveDeltaP.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnMoveDeltaP.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMoveDeltaP.Location = new System.Drawing.Point(332, 6);
            this.btnMoveDeltaP.Margin = new System.Windows.Forms.Padding(4);
            this.btnMoveDeltaP.Name = "btnMoveDeltaP";
            this.btnMoveDeltaP.Size = new System.Drawing.Size(153, 40);
            this.btnMoveDeltaP.TabIndex = 14;
            this.btnMoveDeltaP.Text = "+";
            this.btnMoveDeltaP.UseVisualStyleBackColor = false;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.gwMotorAxisStatus1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanelB, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(2, 2);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 43F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 52F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(492, 217);
            this.tableLayoutPanel1.TabIndex = 206;
            // 
            // gwMotorAxisStatus1
            // 
            this.gwMotorAxisStatus1.AxisName = "X Axis Status";
            this.gwMotorAxisStatus1.BackColor = System.Drawing.Color.WhiteSmoke;
            this.gwMotorAxisStatus1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.gwMotorAxisStatus1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gwMotorAxisStatus1.Font = new System.Drawing.Font("新細明體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.gwMotorAxisStatus1.Location = new System.Drawing.Point(0, 0);
            this.gwMotorAxisStatus1.Margin = new System.Windows.Forms.Padding(0);
            this.gwMotorAxisStatus1.Name = "gwMotorAxisStatus1";
            this.gwMotorAxisStatus1.Padding = new System.Windows.Forms.Padding(0, 0, 3, 3);
            this.gwMotorAxisStatus1.Size = new System.Drawing.Size(492, 122);
            this.gwMotorAxisStatus1.TabIndex = 200;
            // 
            // GwMotorAxisStatusCmd
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Gray;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "GwMotorAxisStatusCmd";
            this.Padding = new System.Windows.Forms.Padding(2);
            this.Size = new System.Drawing.Size(496, 221);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numDist)).EndInit();
            this.tableLayoutPanelB.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        internal GwMotorAxisStatus gwMotorAxisStatus1;
        internal System.Windows.Forms.NumericUpDown numDist;
        internal System.Windows.Forms.Label lblDist;
        internal System.Windows.Forms.Label lblUnit;
        internal System.Windows.Forms.Button btnMoveDeltaN;
        internal System.Windows.Forms.Button btnMoveTo;
        internal System.Windows.Forms.Button btnMoveDeltaP;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelB;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}
