namespace LaserAlignDX.Mvc.Gui
{
    partial class GvFocusSettingPanel
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GvFocusSettingPanel));
            this.lblFocusMotorZ = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btnFocusMotorGo = new System.Windows.Forms.Button();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.btnFocusMotorSettings = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblFocusMotorZ
            // 
            this.lblFocusMotorZ.BackColor = System.Drawing.Color.Black;
            this.lblFocusMotorZ.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFocusMotorZ.Font = new System.Drawing.Font("Microsoft YaHei UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFocusMotorZ.ForeColor = System.Drawing.Color.Lime;
            this.lblFocusMotorZ.Location = new System.Drawing.Point(8, 39);
            this.lblFocusMotorZ.Margin = new System.Windows.Forms.Padding(8, 3, 8, 3);
            this.lblFocusMotorZ.Name = "lblFocusMotorZ";
            this.lblFocusMotorZ.Size = new System.Drawing.Size(125, 32);
            this.lblFocusMotorZ.TabIndex = 51;
            this.lblFocusMotorZ.Text = "999.000";
            this.lblFocusMotorZ.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(4, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(133, 36);
            this.label1.TabIndex = 48;
            this.label1.Text = "相機對焦 Z (mm)";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnFocusMotorGo
            // 
            this.btnFocusMotorGo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.btnFocusMotorGo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnFocusMotorGo.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnFocusMotorGo.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnFocusMotorGo.Location = new System.Drawing.Point(145, 40);
            this.btnFocusMotorGo.Margin = new System.Windows.Forms.Padding(4);
            this.btnFocusMotorGo.Name = "btnFocusMotorGo";
            this.btnFocusMotorGo.Size = new System.Drawing.Size(52, 30);
            this.btnFocusMotorGo.TabIndex = 50;
            this.btnFocusMotorGo.Text = "Go";
            this.btnFocusMotorGo.UseVisualStyleBackColor = false;
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "sysSettings.png");
            // 
            // btnFocusMotorSettings
            // 
            this.btnFocusMotorSettings.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnFocusMotorSettings.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnFocusMotorSettings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnFocusMotorSettings.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnFocusMotorSettings.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnFocusMotorSettings.ImageIndex = 0;
            this.btnFocusMotorSettings.ImageList = this.imageList1;
            this.btnFocusMotorSettings.Location = new System.Drawing.Point(145, 4);
            this.btnFocusMotorSettings.Margin = new System.Windows.Forms.Padding(4);
            this.btnFocusMotorSettings.Name = "btnFocusMotorSettings";
            this.btnFocusMotorSettings.Size = new System.Drawing.Size(52, 28);
            this.btnFocusMotorSettings.TabIndex = 49;
            this.btnFocusMotorSettings.UseVisualStyleBackColor = false;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnFocusMotorGo, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblFocusMotorZ, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.btnFocusMotorSettings, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(201, 74);
            this.tableLayoutPanel1.TabIndex = 52;
            // 
            // GvFocusSettingPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "GvFocusSettingPanel";
            this.Size = new System.Drawing.Size(201, 74);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.Label lblFocusMotorZ;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.Button btnFocusMotorGo;
        public System.Windows.Forms.Button btnFocusMotorSettings;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}
