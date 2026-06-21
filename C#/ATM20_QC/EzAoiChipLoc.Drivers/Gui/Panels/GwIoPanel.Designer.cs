namespace EzAoiChipLocQC.Gui.Panels
{
    partial class GwIoPanel
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
            this.btnClear = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.gvIoPointsSimpleView1 = new EzIO.Gui.GvIoPointsSimpleView();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.gvIoPointsSimpleView1, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(6, 10);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(381, 651);
            this.tableLayoutPanel1.TabIndex = 309;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Transparent;
            this.panel1.Controls.Add(this.btnClear);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(4, 5);
            this.panel1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(373, 40);
            this.panel1.TabIndex = 310;
            // 
            // btnClear
            // 
            this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClear.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnClear.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnClear.Location = new System.Drawing.Point(257, 0);
            this.btnClear.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(112, 40);
            this.btnClear.TabIndex = 310;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = false;
            this.btnClear.Visible = false;
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Font = new System.Drawing.Font("微軟正黑體", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.MediumBlue;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.label1.Name = "label1";
            this.label1.Padding = new System.Windows.Forms.Padding(3, 5, 8, 5);
            this.label1.Size = new System.Drawing.Size(373, 40);
            this.label1.TabIndex = 309;
            this.label1.Text = "PLC IO";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // gvIoPointsSimpleView1
            // 
            this.gvIoPointsSimpleView1.BackColor = System.Drawing.Color.SlateGray;
            this.gvIoPointsSimpleView1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.gvIoPointsSimpleView1.CellCols = 3;
            this.gvIoPointsSimpleView1.CellRows = 1;
            this.gvIoPointsSimpleView1.CellsActiveBackColor = System.Drawing.Color.Lime;
            this.gvIoPointsSimpleView1.CellsActiveForeColor = System.Drawing.Color.Black;
            this.gvIoPointsSimpleView1.CellsBackColor = System.Drawing.Color.DimGray;
            this.gvIoPointsSimpleView1.CellsForeColor = System.Drawing.Color.White;
            this.gvIoPointsSimpleView1.CellsInvertedBackColor = System.Drawing.Color.Red;
            this.gvIoPointsSimpleView1.CellsInvertedForeColor = System.Drawing.Color.White;
            this.gvIoPointsSimpleView1.CellsMargin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.gvIoPointsSimpleView1.CellsRegActiveBackColor = System.Drawing.Color.Black;
            this.gvIoPointsSimpleView1.CellsRegActiveForeColor = System.Drawing.Color.Lime;
            this.gvIoPointsSimpleView1.CellsRegBackColor = System.Drawing.Color.Black;
            this.gvIoPointsSimpleView1.CellsRegForeColor = System.Drawing.Color.White;
            this.gvIoPointsSimpleView1.CellWidth = 0;
            this.gvIoPointsSimpleView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gvIoPointsSimpleView1.IsStatusPanelVisible = false;
            this.gvIoPointsSimpleView1.Location = new System.Drawing.Point(1, 51);
            this.gvIoPointsSimpleView1.Margin = new System.Windows.Forms.Padding(1, 1, 1, 3);
            this.gvIoPointsSimpleView1.Name = "gvIoPointsSimpleView1";
            this.gvIoPointsSimpleView1.Padding = new System.Windows.Forms.Padding(2);
            this.gvIoPointsSimpleView1.ReadOnly = false;
            this.gvIoPointsSimpleView1.Size = new System.Drawing.Size(379, 597);
            this.gvIoPointsSimpleView1.TabIndex = 311;
            // 
            // GwIoPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "GwIoPanel";
            this.Padding = new System.Windows.Forms.Padding(6, 10, 6, 10);
            this.Size = new System.Drawing.Size(393, 671);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Label label1;
        private EzIO.Gui.GvIoPointsSimpleView gvIoPointsSimpleView1;
    }
}
