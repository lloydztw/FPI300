namespace EzAoiEmptyTrayInspector.Gui.Panels
{
    partial class GvProductionPanel
    {
        /// <summary> 
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該公開 Managed 資源則為 true，否則為 false。</param>
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
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改這個方法的內容。
        ///
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GvProductionPanel));
            this.panel3 = new System.Windows.Forms.Panel();
            this.gwFuncButtonsPanel1 = new EzAoiEmptyTrayInspector.Gui.Panels.GwFuncButtonsPanel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.gwRecipeInfo1 = new EzAoiEmptyTrayInspector.Gui.Panels.GwRecipeInfo();
            this.lblPassFail = new System.Windows.Forms.Label();
            this.gwLogPanel1 = new EzAoiEmptyTrayInspector.Gui.Panels.GwLogPanel();
            this.panel3.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.panel3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel3.Controls.Add(this.gwFuncButtonsPanel1);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(1, 271);
            this.panel3.Margin = new System.Windows.Forms.Padding(1);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(505, 78);
            this.panel3.TabIndex = 307;
            // 
            // gwFuncButtonsPanel1
            // 
            this.gwFuncButtonsPanel1.BackColor = System.Drawing.Color.Transparent;
            this.gwFuncButtonsPanel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.gwFuncButtonsPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gwFuncButtonsPanel1.Location = new System.Drawing.Point(0, 0);
            this.gwFuncButtonsPanel1.Margin = new System.Windows.Forms.Padding(1);
            this.gwFuncButtonsPanel1.Name = "gwFuncButtonsPanel1";
            this.gwFuncButtonsPanel1.Padding = new System.Windows.Forms.Padding(20, 10, 20, 10);
            this.gwFuncButtonsPanel1.Size = new System.Drawing.Size(503, 76);
            this.gwFuncButtonsPanel1.TabIndex = 1;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.gwRecipeInfo1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.panel3, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.lblPassFail, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.gwLogPanel1, 0, 3);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(507, 918);
            this.tableLayoutPanel1.TabIndex = 310;
            // 
            // gwRecipeInfo1
            // 
            this.gwRecipeInfo1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.gwRecipeInfo1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.gwRecipeInfo1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gwRecipeInfo1.Location = new System.Drawing.Point(1, 151);
            this.gwRecipeInfo1.Margin = new System.Windows.Forms.Padding(1);
            this.gwRecipeInfo1.Name = "gwRecipeInfo1";
            this.gwRecipeInfo1.Padding = new System.Windows.Forms.Padding(20, 8, 20, 8);
            this.gwRecipeInfo1.Size = new System.Drawing.Size(505, 118);
            this.gwRecipeInfo1.TabIndex = 310;
            // 
            // lblPassFail
            // 
            this.lblPassFail.BackColor = System.Drawing.Color.Black;
            this.lblPassFail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPassFail.Font = new System.Drawing.Font("微軟正黑體", 36F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblPassFail.ForeColor = System.Drawing.Color.Blue;
            this.lblPassFail.Location = new System.Drawing.Point(20, 20);
            this.lblPassFail.Margin = new System.Windows.Forms.Padding(20, 20, 20, 10);
            this.lblPassFail.Name = "lblPassFail";
            this.lblPassFail.Size = new System.Drawing.Size(467, 120);
            this.lblPassFail.TabIndex = 308;
            this.lblPassFail.Text = "空盤檢測";
            this.lblPassFail.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // gwLogPanel1
            // 
            this.gwLogPanel1.BackColor = System.Drawing.Color.Transparent;
            this.gwLogPanel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.gwLogPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gwLogPanel1.Location = new System.Drawing.Point(1, 351);
            this.gwLogPanel1.Margin = new System.Windows.Forms.Padding(1);
            this.gwLogPanel1.Name = "gwLogPanel1";
            this.gwLogPanel1.Padding = new System.Windows.Forms.Padding(20, 8, 20, 8);
            this.gwLogPanel1.Size = new System.Drawing.Size(505, 566);
            this.gwLogPanel1.TabIndex = 311;
            // 
            // GvProductionPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "GvProductionPanel";
            this.Size = new System.Drawing.Size(1220, 918);
            this.panel3.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel panel3;
        private GwFuncButtonsPanel gwFuncButtonsPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private GwRecipeInfo gwRecipeInfo1;
        public System.Windows.Forms.Label lblPassFail;
        private GwLogPanel gwLogPanel1;
    }
}
