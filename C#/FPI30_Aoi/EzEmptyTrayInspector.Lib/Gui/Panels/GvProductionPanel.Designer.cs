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
            this.panel2 = new System.Windows.Forms.Panel();
            this.lblRecipeInfo = new System.Windows.Forms.Label();
            this.cboRecipeNames = new System.Windows.Forms.ComboBox();
            this.panel4 = new System.Windows.Forms.Panel();
            this.gwLogPanel1 = new EzAoiEmptyTrayInspector.Gui.Panels.GwLogPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblPassFail = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.gwFuncButtonsPanel1 = new EzAoiEmptyTrayInspector.Gui.Panels.GwFuncButtonsPanel();
            this.panel2.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.panel2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.lblRecipeInfo);
            this.panel2.Location = new System.Drawing.Point(0, 149);
            this.panel2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panel2.Name = "panel2";
            this.panel2.Padding = new System.Windows.Forms.Padding(20, 10, 20, 10);
            this.panel2.Size = new System.Drawing.Size(466, 91);
            this.panel2.TabIndex = 300;
            // 
            // lblRecipeInfo
            // 
            this.lblRecipeInfo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.lblRecipeInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblRecipeInfo.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblRecipeInfo.ForeColor = System.Drawing.Color.Black;
            this.lblRecipeInfo.Location = new System.Drawing.Point(20, 10);
            this.lblRecipeInfo.Name = "lblRecipeInfo";
            this.lblRecipeInfo.Padding = new System.Windows.Forms.Padding(9, 0, 0, 0);
            this.lblRecipeInfo.Size = new System.Drawing.Size(424, 69);
            this.lblRecipeInfo.TabIndex = 21;
            this.lblRecipeInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cboRecipeNames
            // 
            this.cboRecipeNames.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cboRecipeNames.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboRecipeNames.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboRecipeNames.FormattingEnabled = true;
            this.cboRecipeNames.Location = new System.Drawing.Point(999, 188);
            this.cboRecipeNames.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cboRecipeNames.Name = "cboRecipeNames";
            this.cboRecipeNames.Size = new System.Drawing.Size(117, 31);
            this.cboRecipeNames.TabIndex = 306;
            this.cboRecipeNames.Visible = false;
            // 
            // panel4
            // 
            this.panel4.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panel4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel4.Controls.Add(this.gwLogPanel1);
            this.panel4.Location = new System.Drawing.Point(0, 309);
            this.panel4.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panel4.Name = "panel4";
            this.panel4.Padding = new System.Windows.Forms.Padding(12, 0, 12, 12);
            this.panel4.Size = new System.Drawing.Size(466, 404);
            this.panel4.TabIndex = 302;
            // 
            // gwLogPanel1
            // 
            this.gwLogPanel1.BackColor = System.Drawing.Color.Transparent;
            this.gwLogPanel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.gwLogPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gwLogPanel1.Location = new System.Drawing.Point(12, 0);
            this.gwLogPanel1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.gwLogPanel1.Name = "gwLogPanel1";
            this.gwLogPanel1.Padding = new System.Windows.Forms.Padding(5, 8, 5, 8);
            this.gwLogPanel1.Size = new System.Drawing.Size(440, 390);
            this.gwLogPanel1.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.panel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.lblPassFail);
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(20, 20, 20, 10);
            this.panel1.Size = new System.Drawing.Size(466, 150);
            this.panel1.TabIndex = 147;
            // 
            // lblPassFail
            // 
            this.lblPassFail.BackColor = System.Drawing.Color.Black;
            this.lblPassFail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPassFail.Font = new System.Drawing.Font("微軟正黑體", 36F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblPassFail.ForeColor = System.Drawing.Color.Blue;
            this.lblPassFail.Location = new System.Drawing.Point(20, 20);
            this.lblPassFail.Name = "lblPassFail";
            this.lblPassFail.Size = new System.Drawing.Size(424, 118);
            this.lblPassFail.TabIndex = 307;
            this.lblPassFail.Text = "空盤檢測";
            this.lblPassFail.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.panel3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel3.Controls.Add(this.gwFuncButtonsPanel1);
            this.panel3.Location = new System.Drawing.Point(0, 238);
            this.panel3.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(466, 72);
            this.panel3.TabIndex = 307;
            // 
            // gwFuncButtonsPanel1
            // 
            this.gwFuncButtonsPanel1.BackColor = System.Drawing.Color.Transparent;
            this.gwFuncButtonsPanel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.gwFuncButtonsPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gwFuncButtonsPanel1.Location = new System.Drawing.Point(0, 0);
            this.gwFuncButtonsPanel1.Name = "gwFuncButtonsPanel1";
            this.gwFuncButtonsPanel1.Padding = new System.Windows.Forms.Padding(20, 10, 20, 10);
            this.gwFuncButtonsPanel1.Size = new System.Drawing.Size(464, 70);
            this.gwFuncButtonsPanel1.TabIndex = 1;
            // 
            // GvProductionPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.cboRecipeNames);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.panel1);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "GvProductionPanel";
            this.Size = new System.Drawing.Size(1169, 918);
            this.panel2.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel4;
        public System.Windows.Forms.ComboBox cboRecipeNames;
        public System.Windows.Forms.Label lblPassFail;
        private EzAoiEmptyTrayInspector.Gui.Panels.GwLogPanel gwLogPanel1;
        public System.Windows.Forms.Label lblRecipeInfo;
        private System.Windows.Forms.Panel panel3;
        private GwFuncButtonsPanel gwFuncButtonsPanel1;
    }
}
