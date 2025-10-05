namespace Traveller.Data.Packer
{
    partial class FormMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.panel1 = new System.Windows.Forms.Panel();
            this.numMachineID = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.chkCopyLogs = new System.Windows.Forms.CheckBox();
            this.btnTestLotReport = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tmPickerEnd = new System.Windows.Forms.DateTimePicker();
            this.tmPickerBegin = new System.Windows.Forms.DateTimePicker();
            this.label1 = new System.Windows.Forms.Label();
            this.btnPack = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMachineID)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Transparent;
            this.panel1.Controls.Add(this.numMachineID);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.chkCopyLogs);
            this.panel1.Controls.Add(this.btnTestLotReport);
            this.panel1.Controls.Add(this.btnClear);
            this.panel1.Controls.Add(this.groupBox2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(3, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(482, 235);
            this.panel1.TabIndex = 338;
            // 
            // numMachineID
            // 
            this.numMachineID.Font = new System.Drawing.Font("Microsoft YaHei UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.numMachineID.Location = new System.Drawing.Point(156, 161);
            this.numMachineID.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numMachineID.Name = "numMachineID";
            this.numMachineID.Size = new System.Drawing.Size(120, 29);
            this.numMachineID.TabIndex = 331;
            this.numMachineID.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numMachineID.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Black;
            this.label2.Location = new System.Drawing.Point(37, 161);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(92, 27);
            this.label2.TabIndex = 329;
            this.label2.Text = "機台編號";
            // 
            // chkCopyLogs
            // 
            this.chkCopyLogs.AutoSize = true;
            this.chkCopyLogs.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.chkCopyLogs.ForeColor = System.Drawing.Color.Black;
            this.chkCopyLogs.Location = new System.Drawing.Point(354, 165);
            this.chkCopyLogs.Name = "chkCopyLogs";
            this.chkCopyLogs.Size = new System.Drawing.Size(96, 23);
            this.chkCopyLogs.TabIndex = 330;
            this.chkCopyLogs.Text = "包括 LOG";
            this.chkCopyLogs.UseVisualStyleBackColor = true;
            this.chkCopyLogs.Visible = false;
            // 
            // btnTestLotReport
            // 
            this.btnTestLotReport.Font = new System.Drawing.Font("Verdana", 11.78182F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTestLotReport.Location = new System.Drawing.Point(576, 361);
            this.btnTestLotReport.Margin = new System.Windows.Forms.Padding(4);
            this.btnTestLotReport.Name = "btnTestLotReport";
            this.btnTestLotReport.Size = new System.Drawing.Size(260, 45);
            this.btnTestLotReport.TabIndex = 2;
            this.btnTestLotReport.Text = "Test Lot Report";
            this.btnTestLotReport.UseVisualStyleBackColor = true;
            this.btnTestLotReport.Visible = false;
            // 
            // btnClear
            // 
            this.btnClear.BackColor = System.Drawing.SystemColors.Control;
            this.btnClear.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnClear.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnClear.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClear.Location = new System.Drawing.Point(576, 418);
            this.btnClear.Margin = new System.Windows.Forms.Padding(4);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(164, 45);
            this.btnClear.TabIndex = 327;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = false;
            this.btnClear.Visible = false;
            // 
            // groupBox2
            // 
            this.groupBox2.BackColor = System.Drawing.Color.Transparent;
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.tmPickerEnd);
            this.groupBox2.Controls.Add(this.tmPickerBegin);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox2.ForeColor = System.Drawing.Color.Black;
            this.groupBox2.Location = new System.Drawing.Point(8, 8);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox2.Size = new System.Drawing.Size(472, 121);
            this.groupBox2.TabIndex = 329;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "日期時間";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Black;
            this.label3.Location = new System.Drawing.Point(31, 74);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 18);
            this.label3.TabIndex = 328;
            this.label3.Text = "End";
            // 
            // tmPickerEnd
            // 
            this.tmPickerEnd.CustomFormat = "yyyy/MM/dd - HH:mm:ss";
            this.tmPickerEnd.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.tmPickerEnd.Location = new System.Drawing.Point(148, 70);
            this.tmPickerEnd.Margin = new System.Windows.Forms.Padding(4);
            this.tmPickerEnd.Name = "tmPickerEnd";
            this.tmPickerEnd.Size = new System.Drawing.Size(304, 26);
            this.tmPickerEnd.TabIndex = 327;
            // 
            // tmPickerBegin
            // 
            this.tmPickerBegin.CustomFormat = "yyyy/MM/dd - HH:mm:ss";
            this.tmPickerBegin.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.tmPickerBegin.Location = new System.Drawing.Point(148, 36);
            this.tmPickerBegin.Margin = new System.Windows.Forms.Padding(4);
            this.tmPickerBegin.Name = "tmPickerBegin";
            this.tmPickerBegin.Size = new System.Drawing.Size(304, 26);
            this.tmPickerBegin.TabIndex = 323;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Black;
            this.label1.Location = new System.Drawing.Point(31, 40);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(45, 18);
            this.label1.TabIndex = 301;
            this.label1.Text = "Start";
            // 
            // btnPack
            // 
            this.btnPack.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPack.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnPack.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPack.Location = new System.Drawing.Point(113, 344);
            this.btnPack.Margin = new System.Windows.Forms.Padding(4);
            this.btnPack.Name = "btnPack";
            this.btnPack.Size = new System.Drawing.Size(262, 50);
            this.btnPack.TabIndex = 336;
            this.btnPack.Text = "打包數據";
            this.btnPack.UseVisualStyleBackColor = false;
            this.btnPack.Click += new System.EventHandler(this.btnPack_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.BackColor = System.Drawing.Color.Black;
            this.lblStatus.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblStatus.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblStatus.ForeColor = System.Drawing.Color.Lime;
            this.lblStatus.Location = new System.Drawing.Point(3, 235);
            this.lblStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(482, 92);
            this.lblStatus.TabIndex = 339;
            this.lblStatus.Text = "...";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.LightSteelBlue;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(488, 414);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnPack);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("細明體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "FormMain";
            this.Padding = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.Text = "Traveller106 參數打包";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMachineID)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnTestLotReport;
        public System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.DateTimePicker tmPickerEnd;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.DateTimePicker tmPickerBegin;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnPack;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.CheckBox chkCopyLogs;
        private System.Windows.Forms.NumericUpDown numMachineID;
        private System.Windows.Forms.Label label2;
    }
}