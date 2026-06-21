namespace EzDualMatch.GUI
{
    partial class FormDemoCameraViewer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormDemoCameraViewer));
            this.panel1 = new System.Windows.Forms.Panel();
            this.jxCameraPropertyPanel1 = new JetEazy.GUI.GjxCameraPropertyPanel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.lblBlinker = new System.Windows.Forms.Label();
            this.lblCoordInfo = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.qzCameraViewer1 = new EzCamera.GUI.QzCameraViewer();
            this.panel1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.Control;
            this.panel1.Controls.Add(this.jxCameraPropertyPanel1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(962, 50);
            this.panel1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(438, 901);
            this.panel1.TabIndex = 15;
            // 
            // jxCameraPropertyPanel1
            // 
            this.jxCameraPropertyPanel1.BackColor = System.Drawing.SystemColors.ControlLight;
            this.jxCameraPropertyPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.jxCameraPropertyPanel1.Location = new System.Drawing.Point(0, 0);
            this.jxCameraPropertyPanel1.Name = "jxCameraPropertyPanel1";
            this.jxCameraPropertyPanel1.Padding = new System.Windows.Forms.Padding(10, 10, 10, 10);
            this.jxCameraPropertyPanel1.Size = new System.Drawing.Size(438, 901);
            this.jxCameraPropertyPanel1.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.SystemColors.ControlDark;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 444F));
            this.tableLayoutPanel1.Controls.Add(this.panel1, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.panel2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblTitle, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.qzCameraViewer1, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 48F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1403, 953);
            this.tableLayoutPanel1.TabIndex = 16;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.Black;
            this.panel2.Controls.Add(this.lblBlinker);
            this.panel2.Controls.Add(this.lblCoordInfo);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(3, 2);
            this.panel2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(953, 44);
            this.panel2.TabIndex = 17;
            // 
            // lblBlinker
            // 
            this.lblBlinker.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblBlinker.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblBlinker.ForeColor = System.Drawing.Color.White;
            this.lblBlinker.Location = new System.Drawing.Point(13, 10);
            this.lblBlinker.Name = "lblBlinker";
            this.lblBlinker.Size = new System.Drawing.Size(22, 21);
            this.lblBlinker.TabIndex = 18;
            // 
            // lblCoordInfo
            // 
            this.lblCoordInfo.BackColor = System.Drawing.Color.Transparent;
            this.lblCoordInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCoordInfo.Font = new System.Drawing.Font("微軟正黑體", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblCoordInfo.ForeColor = System.Drawing.Color.Lime;
            this.lblCoordInfo.Location = new System.Drawing.Point(0, 0);
            this.lblCoordInfo.Name = "lblCoordInfo";
            this.lblCoordInfo.Padding = new System.Windows.Forms.Padding(44, 0, 0, 0);
            this.lblCoordInfo.Size = new System.Drawing.Size(953, 44);
            this.lblCoordInfo.TabIndex = 17;
            this.lblCoordInfo.Text = "Camera Viewer";
            this.lblCoordInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblTitle
            // 
            this.lblTitle.BackColor = System.Drawing.Color.Black;
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitle.Font = new System.Drawing.Font("微軟正黑體", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblTitle.ForeColor = System.Drawing.Color.Lime;
            this.lblTitle.Location = new System.Drawing.Point(962, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(438, 48);
            this.lblTitle.TabIndex = 3;
            this.lblTitle.Text = "fps = 0.0";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // qzCameraViewer1
            // 
            this.qzCameraViewer1.BackColor = System.Drawing.Color.Black;
            this.qzCameraViewer1.ClearLiveBackgroundEnabled = true;
            this.qzCameraViewer1.CrosshairsColor = System.Drawing.Color.Gold;
            this.qzCameraViewer1.CrosshairsVisible = false;
            this.qzCameraViewer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.qzCameraViewer1.ForeColor = System.Drawing.Color.Black;
            this.qzCameraViewer1.GridLineColor = System.Drawing.Color.LightGray;
            this.qzCameraViewer1.GridLinesVisible = false;
            this.qzCameraViewer1.Image = null;
            this.qzCameraViewer1.Location = new System.Drawing.Point(5, 53);
            this.qzCameraViewer1.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.qzCameraViewer1.Name = "qzCameraViewer1";
            this.qzCameraViewer1.RulerVisible = false;
            this.qzCameraViewer1.Size = new System.Drawing.Size(949, 895);
            this.qzCameraViewer1.TabIndex = 16;
            // 
            // FormDemoCameraViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1403, 953);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "FormDemoCameraViewer";
            this.Text = "Demo QzCameraViewer";
            this.panel1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private EzCamera.GUI.QzCameraViewer qzCameraViewer1;
        private System.Windows.Forms.Label lblCoordInfo;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label lblBlinker;
        private JetEazy.GUI.GjxCameraPropertyPanel jxCameraPropertyPanel1;
    }
}

