namespace EzAoiEmptyTrayInspector.Gui.Panels
{
    partial class GwFuncButtonsPanel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GwFuncButtonsPanel));
            this.tblButtonsGroup = new System.Windows.Forms.TableLayoutPanel();
            this.btnRunAll = new System.Windows.Forms.Button();
            this.btnPickGolden = new System.Windows.Forms.Button();
            this.btnResetClear = new System.Windows.Forms.Button();
            this.btnSnapshot = new System.Windows.Forms.Button();
            this.btnOpenFile = new System.Windows.Forms.Button();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.tblButtonsGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // tblButtonsGroup
            // 
            this.tblButtonsGroup.BackColor = System.Drawing.Color.Transparent;
            this.tblButtonsGroup.ColumnCount = 5;
            this.tblButtonsGroup.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tblButtonsGroup.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tblButtonsGroup.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tblButtonsGroup.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tblButtonsGroup.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tblButtonsGroup.Controls.Add(this.btnRunAll, 0, 0);
            this.tblButtonsGroup.Controls.Add(this.btnPickGolden, 4, 0);
            this.tblButtonsGroup.Controls.Add(this.btnResetClear, 3, 0);
            this.tblButtonsGroup.Controls.Add(this.btnSnapshot, 2, 0);
            this.tblButtonsGroup.Controls.Add(this.btnOpenFile, 1, 0);
            this.tblButtonsGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblButtonsGroup.Location = new System.Drawing.Point(20, 10);
            this.tblButtonsGroup.Margin = new System.Windows.Forms.Padding(0);
            this.tblButtonsGroup.Name = "tblButtonsGroup";
            this.tblButtonsGroup.RowCount = 1;
            this.tblButtonsGroup.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblButtonsGroup.Size = new System.Drawing.Size(549, 51);
            this.tblButtonsGroup.TabIndex = 20;
            // 
            // btnRunAll
            // 
            this.btnRunAll.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.btnRunAll.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnRunAll.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnRunAll.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnRunAll.ImageKey = "Run";
            this.btnRunAll.Location = new System.Drawing.Point(1, 1);
            this.btnRunAll.Margin = new System.Windows.Forms.Padding(1);
            this.btnRunAll.Name = "btnRunAll";
            this.btnRunAll.Size = new System.Drawing.Size(107, 49);
            this.btnRunAll.TabIndex = 23;
            this.btnRunAll.TabStop = false;
            this.btnRunAll.Text = "Test";
            this.btnRunAll.UseVisualStyleBackColor = false;
            // 
            // btnPickGolden
            // 
            this.btnPickGolden.BackColor = System.Drawing.Color.Gold;
            this.btnPickGolden.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnPickGolden.Enabled = false;
            this.btnPickGolden.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnPickGolden.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnPickGolden.ImageKey = "PickGolden";
            this.btnPickGolden.Location = new System.Drawing.Point(437, 1);
            this.btnPickGolden.Margin = new System.Windows.Forms.Padding(1);
            this.btnPickGolden.Name = "btnPickGolden";
            this.btnPickGolden.Size = new System.Drawing.Size(111, 49);
            this.btnPickGolden.TabIndex = 22;
            this.btnPickGolden.TabStop = false;
            this.btnPickGolden.Text = "Golden";
            this.btnPickGolden.UseVisualStyleBackColor = false;
            // 
            // btnResetClear
            // 
            this.btnResetClear.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnResetClear.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnResetClear.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnResetClear.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnResetClear.ImageKey = "Reset";
            this.btnResetClear.Location = new System.Drawing.Point(328, 1);
            this.btnResetClear.Margin = new System.Windows.Forms.Padding(1);
            this.btnResetClear.Name = "btnResetClear";
            this.btnResetClear.Size = new System.Drawing.Size(107, 49);
            this.btnResetClear.TabIndex = 21;
            this.btnResetClear.TabStop = false;
            this.btnResetClear.Text = "Clear";
            this.btnResetClear.UseVisualStyleBackColor = false;
            // 
            // btnSnapshot
            // 
            this.btnSnapshot.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnSnapshot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSnapshot.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnSnapshot.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSnapshot.ImageKey = "Scan";
            this.btnSnapshot.Location = new System.Drawing.Point(219, 1);
            this.btnSnapshot.Margin = new System.Windows.Forms.Padding(1);
            this.btnSnapshot.Name = "btnSnapshot";
            this.btnSnapshot.Size = new System.Drawing.Size(107, 49);
            this.btnSnapshot.TabIndex = 20;
            this.btnSnapshot.TabStop = false;
            this.btnSnapshot.Text = "Scan";
            this.btnSnapshot.UseVisualStyleBackColor = false;
            // 
            // btnOpenFile
            // 
            this.btnOpenFile.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnOpenFile.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnOpenFile.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnOpenFile.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOpenFile.ImageKey = "OpenFile";
            this.btnOpenFile.Location = new System.Drawing.Point(110, 1);
            this.btnOpenFile.Margin = new System.Windows.Forms.Padding(1);
            this.btnOpenFile.Name = "btnOpenFile";
            this.btnOpenFile.Size = new System.Drawing.Size(107, 49);
            this.btnOpenFile.TabIndex = 19;
            this.btnOpenFile.TabStop = false;
            this.btnOpenFile.Text = "File";
            this.btnOpenFile.UseVisualStyleBackColor = false;
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "Scan");
            this.imageList1.Images.SetKeyName(1, "Run");
            this.imageList1.Images.SetKeyName(2, "OpenFile");
            this.imageList1.Images.SetKeyName(3, "PickGolden");
            this.imageList1.Images.SetKeyName(4, "Reset");
            // 
            // GwFuncButtonsPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.Controls.Add(this.tblButtonsGroup);
            this.Name = "GwFuncButtonsPanel";
            this.Padding = new System.Windows.Forms.Padding(20, 10, 20, 10);
            this.Size = new System.Drawing.Size(589, 71);
            this.tblButtonsGroup.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tblButtonsGroup;
        public System.Windows.Forms.Button btnRunAll;
        public System.Windows.Forms.Button btnPickGolden;
        public System.Windows.Forms.Button btnResetClear;
        public System.Windows.Forms.Button btnSnapshot;
        public System.Windows.Forms.Button btnOpenFile;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}
