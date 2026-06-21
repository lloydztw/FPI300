
namespace JetEazy.GUI
{
    partial class GjxCameraPropertyPanel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GjxCameraPropertyPanel));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.cboAvailableCameras = new System.Windows.Forms.ComboBox();
            this.tableLayoutPanel0 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnLiveMode = new System.Windows.Forms.Button();
            this.btnSnapshot = new System.Windows.Forms.Button();
            this.btnMatch = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.gwPanePropsViewer1 = new LeTian.JxProps.Gui.GwPanePropsViewer();
            this.lblDeviceInfo = new System.Windows.Forms.Label();
            this.btnOpenCamera = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel0.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.btnOpenCamera, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.cboAvailableCameras, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(447, 36);
            this.tableLayoutPanel1.TabIndex = 4;
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "camera.ico");
            // 
            // cboAvailableCameras
            // 
            this.cboAvailableCameras.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboAvailableCameras.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboAvailableCameras.Font = new System.Drawing.Font("Consolas", 10.90909F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboAvailableCameras.FormattingEnabled = true;
            this.cboAvailableCameras.Location = new System.Drawing.Point(226, 2);
            this.cboAvailableCameras.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cboAvailableCameras.Name = "cboAvailableCameras";
            this.cboAvailableCameras.Size = new System.Drawing.Size(218, 30);
            this.cboAvailableCameras.TabIndex = 12;
            this.cboAvailableCameras.Visible = false;
            // 
            // tableLayoutPanel0
            // 
            this.tableLayoutPanel0.ColumnCount = 1;
            this.tableLayoutPanel0.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel0.Controls.Add(this.tableLayoutPanel2, 0, 4);
            this.tableLayoutPanel0.Controls.Add(this.tableLayoutPanel1, 0, 0);
            this.tableLayoutPanel0.Controls.Add(this.gwPanePropsViewer1, 0, 2);
            this.tableLayoutPanel0.Controls.Add(this.lblDeviceInfo, 0, 1);
            this.tableLayoutPanel0.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel0.Location = new System.Drawing.Point(10, 10);
            this.tableLayoutPanel0.Name = "tableLayoutPanel0";
            this.tableLayoutPanel0.RowCount = 6;
            this.tableLayoutPanel0.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel0.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel0.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel0.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8F));
            this.tableLayoutPanel0.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel0.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8F));
            this.tableLayoutPanel0.Size = new System.Drawing.Size(453, 683);
            this.tableLayoutPanel0.TabIndex = 5;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.btnBrowse, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnStop, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.btnLiveMode, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.btnSnapshot, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnMatch, 1, 2);
            this.tableLayoutPanel2.Controls.Add(this.btnClear, 0, 2);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 553);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 3;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 34F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(447, 119);
            this.tableLayoutPanel2.TabIndex = 18;
            // 
            // btnBrowse
            // 
            this.btnBrowse.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnBrowse.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnBrowse.Location = new System.Drawing.Point(3, 2);
            this.btnBrowse.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(217, 35);
            this.btnBrowse.TabIndex = 14;
            this.btnBrowse.TabStop = false;
            this.btnBrowse.Text = "Browse";
            this.btnBrowse.UseVisualStyleBackColor = true;
            // 
            // btnStop
            // 
            this.btnStop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnStop.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnStop.Location = new System.Drawing.Point(3, 41);
            this.btnStop.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(217, 35);
            this.btnStop.TabIndex = 15;
            this.btnStop.TabStop = false;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            // 
            // btnLiveMode
            // 
            this.btnLiveMode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnLiveMode.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnLiveMode.Location = new System.Drawing.Point(226, 41);
            this.btnLiveMode.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnLiveMode.Name = "btnLiveMode";
            this.btnLiveMode.Size = new System.Drawing.Size(218, 35);
            this.btnLiveMode.TabIndex = 17;
            this.btnLiveMode.TabStop = false;
            this.btnLiveMode.Text = "Live";
            this.btnLiveMode.UseVisualStyleBackColor = true;
            // 
            // btnSnapshot
            // 
            this.btnSnapshot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSnapshot.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnSnapshot.Location = new System.Drawing.Point(226, 2);
            this.btnSnapshot.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnSnapshot.Name = "btnSnapshot";
            this.btnSnapshot.Size = new System.Drawing.Size(218, 35);
            this.btnSnapshot.TabIndex = 11;
            this.btnSnapshot.TabStop = false;
            this.btnSnapshot.Text = "Snapshot";
            this.btnSnapshot.UseVisualStyleBackColor = true;
            // 
            // btnMatch
            // 
            this.btnMatch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnMatch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnMatch.Font = new System.Drawing.Font("微軟正黑體", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnMatch.Location = new System.Drawing.Point(226, 80);
            this.btnMatch.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnMatch.Name = "btnMatch";
            this.btnMatch.Size = new System.Drawing.Size(218, 37);
            this.btnMatch.TabIndex = 18;
            this.btnMatch.Text = "Match";
            this.btnMatch.UseVisualStyleBackColor = false;
            // 
            // btnClear
            // 
            this.btnClear.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.btnClear.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnClear.Font = new System.Drawing.Font("微軟正黑體", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnClear.Location = new System.Drawing.Point(3, 80);
            this.btnClear.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(217, 37);
            this.btnClear.TabIndex = 19;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = false;
            // 
            // gwPanePropsViewer1
            // 
            this.gwPanePropsViewer1.BackColor = System.Drawing.Color.WhiteSmoke;
            this.gwPanePropsViewer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gwPanePropsViewer1.Editable = true;
            this.gwPanePropsViewer1.ImageList = null;
            this.gwPanePropsViewer1.Location = new System.Drawing.Point(3, 146);
            this.gwPanePropsViewer1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.gwPanePropsViewer1.Name = "gwPanePropsViewer1";
            this.gwPanePropsViewer1.Size = new System.Drawing.Size(447, 392);
            this.gwPanePropsViewer1.TabIndex = 19;
            // 
            // lblDeviceInfo
            // 
            this.lblDeviceInfo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.lblDeviceInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDeviceInfo.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblDeviceInfo.ForeColor = System.Drawing.Color.Black;
            this.lblDeviceInfo.Location = new System.Drawing.Point(3, 42);
            this.lblDeviceInfo.Name = "lblDeviceInfo";
            this.lblDeviceInfo.Padding = new System.Windows.Forms.Padding(9, 0, 0, 0);
            this.lblDeviceInfo.Size = new System.Drawing.Size(447, 100);
            this.lblDeviceInfo.TabIndex = 20;
            this.lblDeviceInfo.Text = "Info";
            this.lblDeviceInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnOpenCamera
            // 
            this.btnOpenCamera.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnOpenCamera.Font = new System.Drawing.Font("微軟正黑體", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnOpenCamera.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOpenCamera.ImageIndex = 0;
            this.btnOpenCamera.ImageList = this.imageList1;
            this.btnOpenCamera.Location = new System.Drawing.Point(3, 2);
            this.btnOpenCamera.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnOpenCamera.Name = "btnOpenCamera";
            this.btnOpenCamera.Size = new System.Drawing.Size(217, 32);
            this.btnOpenCamera.TabIndex = 13;
            this.btnOpenCamera.TabStop = false;
            this.btnOpenCamera.Text = "相機 ...";
            this.btnOpenCamera.UseVisualStyleBackColor = true;
            // 
            // GjxCameraPropertyPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.Controls.Add(this.tableLayoutPanel0);
            this.Name = "GjxCameraPropertyPanel";
            this.Padding = new System.Windows.Forms.Padding(10);
            this.Size = new System.Drawing.Size(473, 703);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel0.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        internal System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        internal System.Windows.Forms.ComboBox cboAvailableCameras;
        internal System.Windows.Forms.TableLayoutPanel tableLayoutPanel0;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        internal System.Windows.Forms.Button btnBrowse;
        internal System.Windows.Forms.Button btnStop;
        internal System.Windows.Forms.Button btnLiveMode;
        internal System.Windows.Forms.Button btnSnapshot;
        private LeTian.JxProps.Gui.GwPanePropsViewer gwPanePropsViewer1;
        public System.Windows.Forms.Label lblDeviceInfo;
        public System.Windows.Forms.Button btnOpenCamera;
        private System.Windows.Forms.ImageList imageList1;
        public System.Windows.Forms.Button btnMatch;
        public System.Windows.Forms.Button btnClear;
    }
}
