namespace EzDualMatch.GUI
{
    partial class FormDualMatchMainWindow
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

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改
        /// 這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormDualMatchMainWindow));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.gPaneOpZone1 = new AwFramework.GUI.GPaneOpZone();
            this.gPaneClientZone1 = new AwFramework.GUI.GPaneClientZone();
            this.gvDualImageViewPanel1 = new EzDualMatch.GUI.Panels.GvDualMatchLRView();
            this.gPaneSysSettings1 = new AwFramework.GUI.GvSysSettingsPanel();
            this.gPaneProduction1 = new AwFramework.GUI.GvProductionPanel();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 456F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.gPaneOpZone1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.gPaneClientZone1, 1, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1247, 243);
            this.tableLayoutPanel1.TabIndex = 20;
            // 
            // gPaneOpZone1
            // 
            this.gPaneOpZone1.BackColor = System.Drawing.Color.DimGray;
            this.gPaneOpZone1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gPaneOpZone1.Location = new System.Drawing.Point(0, 0);
            this.gPaneOpZone1.LogoImage = null;
            this.gPaneOpZone1.LogoImageSizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.gPaneOpZone1.Margin = new System.Windows.Forms.Padding(0);
            this.gPaneOpZone1.Name = "gPaneOpZone1";
            this.gPaneOpZone1.OpModeVisible = true;
            this.gPaneOpZone1.Size = new System.Drawing.Size(456, 243);
            this.gPaneOpZone1.TabIndex = 18;
            // 
            // gPaneClientZone1
            // 
            this.gPaneClientZone1.BackColor = System.Drawing.Color.DimGray;
            this.gPaneClientZone1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gPaneClientZone1.Location = new System.Drawing.Point(456, 0);
            this.gPaneClientZone1.Margin = new System.Windows.Forms.Padding(0);
            this.gPaneClientZone1.Name = "gPaneClientZone1";
            this.gPaneClientZone1.Size = new System.Drawing.Size(791, 243);
            this.gPaneClientZone1.TabIndex = 19;
            // 
            // gvDualImageViewPanel1
            // 
            this.gvDualImageViewPanel1.Location = new System.Drawing.Point(632, 246);
            this.gvDualImageViewPanel1.Name = "gvDualImageViewPanel1";
            this.gvDualImageViewPanel1.Size = new System.Drawing.Size(615, 322);
            this.gvDualImageViewPanel1.TabIndex = 22;
            // 
            // gPaneSysSettings1
            // 
            this.gPaneSysSettings1.BackColor = System.Drawing.SystemColors.ControlLight;
            this.gPaneSysSettings1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("gPaneSysSettings1.BackgroundImage")));
            this.gPaneSysSettings1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.gPaneSysSettings1.Location = new System.Drawing.Point(473, 266);
            this.gPaneSysSettings1.Margin = new System.Windows.Forms.Padding(4);
            this.gPaneSysSettings1.Name = "gPaneSysSettings1";
            //this.gPaneSysSettings1.optHasMotors = true;
            this.gPaneSysSettings1.Padding = new System.Windows.Forms.Padding(8, 18, 8, 32);
            this.gPaneSysSettings1.Size = new System.Drawing.Size(401, 528);
            this.gPaneSysSettings1.TabIndex = 21;
            this.gPaneSysSettings1.Visible = false;
            // 
            // gPaneProduction1
            // 
            this.gPaneProduction1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.gPaneProduction1.BackgroundImage = global::EzDualMatch.Properties.Resources.CommonPageB;
            this.gPaneProduction1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.gPaneProduction1.Location = new System.Drawing.Point(0, 266);
            this.gPaneProduction1.Name = "gPaneProduction1";
            this.gPaneProduction1.Size = new System.Drawing.Size(456, 528);
            this.gPaneProduction1.TabIndex = 17;
            // 
            // FormDualMatchMainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1259, 786);
            this.Controls.Add(this.gPaneSysSettings1);
            this.Controls.Add(this.gPaneProduction1);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.gvDualImageViewPanel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "FormDualMatchMainWindow";
            this.Text = "JetEazy Dual Match";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private AwFramework.GUI.GvProductionPanel gPaneProduction1;
        private AwFramework.GUI.GPaneOpZone gPaneOpZone1;
        private AwFramework.GUI.GPaneClientZone gPaneClientZone1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private AwFramework.GUI.GvSysSettingsPanel gPaneSysSettings1;
        private EzDualMatch.GUI.Panels.GvDualMatchLRView gvDualImageViewPanel1;
    }
}

