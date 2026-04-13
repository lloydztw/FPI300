namespace AX.Gui
{
    partial class GwMotorSimpleGoPanel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GwMotorSimpleGoPanel));
            this.lblCurrentMotorPos = new System.Windows.Forms.Label();
            this.lblAxisName = new System.Windows.Forms.Label();
            this.btnMotorGo = new System.Windows.Forms.Button();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.btnSettings = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCurrentMotorPos
            // 
            this.lblCurrentMotorPos.BackColor = System.Drawing.Color.Black;
            this.lblCurrentMotorPos.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCurrentMotorPos.Font = new System.Drawing.Font("Microsoft YaHei UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCurrentMotorPos.ForeColor = System.Drawing.Color.Lime;
            this.lblCurrentMotorPos.Location = new System.Drawing.Point(8, 39);
            this.lblCurrentMotorPos.Margin = new System.Windows.Forms.Padding(8, 3, 8, 3);
            this.lblCurrentMotorPos.Name = "lblCurrentMotorPos";
            this.lblCurrentMotorPos.Size = new System.Drawing.Size(125, 32);
            this.lblCurrentMotorPos.TabIndex = 51;
            this.lblCurrentMotorPos.Text = "999.000";
            this.lblCurrentMotorPos.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblAxisName
            // 
            this.lblAxisName.AutoSize = true;
            this.lblAxisName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblAxisName.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAxisName.Location = new System.Drawing.Point(4, 0);
            this.lblAxisName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblAxisName.Name = "lblAxisName";
            this.lblAxisName.Size = new System.Drawing.Size(133, 36);
            this.lblAxisName.TabIndex = 48;
            this.lblAxisName.Text = "Motor X (mm)";
            this.lblAxisName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnMotorGo
            // 
            this.btnMotorGo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.btnMotorGo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnMotorGo.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnMotorGo.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMotorGo.Location = new System.Drawing.Point(145, 40);
            this.btnMotorGo.Margin = new System.Windows.Forms.Padding(4);
            this.btnMotorGo.Name = "btnMotorGo";
            this.btnMotorGo.Size = new System.Drawing.Size(52, 30);
            this.btnMotorGo.TabIndex = 50;
            this.btnMotorGo.Text = "Go";
            this.btnMotorGo.UseVisualStyleBackColor = false;
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "sysSettings.png");
            // 
            // btnSettings
            // 
            this.btnSettings.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnSettings.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnSettings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSettings.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnSettings.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSettings.ImageIndex = 0;
            this.btnSettings.ImageList = this.imageList1;
            this.btnSettings.Location = new System.Drawing.Point(145, 4);
            this.btnSettings.Margin = new System.Windows.Forms.Padding(4);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(52, 28);
            this.btnSettings.TabIndex = 49;
            this.btnSettings.UseVisualStyleBackColor = false;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tableLayoutPanel1.Controls.Add(this.lblAxisName, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnMotorGo, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblCurrentMotorPos, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.btnSettings, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(201, 74);
            this.tableLayoutPanel1.TabIndex = 52;
            // 
            // GwMotorSimpleGoPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "GwMotorSimpleGoPanel";
            this.Size = new System.Drawing.Size(201, 74);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.Label lblCurrentMotorPos;
        public System.Windows.Forms.Button btnMotorGo;
        public System.Windows.Forms.Button btnSettings;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        public System.Windows.Forms.Label lblAxisName;
    }
}
