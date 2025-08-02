namespace EzEmptyTrayInspector.Gui.Panels
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
            this.btnHome = new System.Windows.Forms.Button();
            this.btnSwapMapView = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.cboRecipeNames = new System.Windows.Forms.ComboBox();
            this.label10 = new System.Windows.Forms.Label();
            this.panel4 = new System.Windows.Forms.Panel();
            this.gwLogPanel1 = new EzEmptyTrayInspector.Gui.Panels.GwLogPanel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.btnTryRun = new System.Windows.Forms.Button();
            this.btnProductionRun = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel5 = new System.Windows.Forms.Panel();
            this.lblUserName = new System.Windows.Forms.Label();
            this.btnLogin = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.txtSerialNo = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.panel2.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel5.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnHome
            // 
            this.btnHome.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.btnHome.Font = new System.Drawing.Font("Arial", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnHome.Location = new System.Drawing.Point(514, 265);
            this.btnHome.Margin = new System.Windows.Forms.Padding(2);
            this.btnHome.Name = "btnHome";
            this.btnHome.Size = new System.Drawing.Size(74, 40);
            this.btnHome.TabIndex = 304;
            this.btnHome.Text = "Home";
            this.btnHome.UseVisualStyleBackColor = false;
            this.btnHome.Visible = false;
            // 
            // btnSwapMapView
            // 
            this.btnSwapMapView.BackColor = System.Drawing.Color.Yellow;
            this.btnSwapMapView.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnSwapMapView.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnSwapMapView.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSwapMapView.Location = new System.Drawing.Point(482, 277);
            this.btnSwapMapView.Name = "btnSwapMapView";
            this.btnSwapMapView.Size = new System.Drawing.Size(19, 18);
            this.btnSwapMapView.TabIndex = 305;
            this.btnSwapMapView.UseVisualStyleBackColor = false;
            this.btnSwapMapView.Visible = false;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.panel2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.cboRecipeNames);
            this.panel2.Controls.Add(this.label10);
            this.panel2.Location = new System.Drawing.Point(0, 128);
            this.panel2.Margin = new System.Windows.Forms.Padding(2);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(350, 64);
            this.panel2.TabIndex = 300;
            // 
            // cboRecipeNames
            // 
            this.cboRecipeNames.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cboRecipeNames.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboRecipeNames.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboRecipeNames.FormattingEnabled = true;
            this.cboRecipeNames.Location = new System.Drawing.Point(136, 18);
            this.cboRecipeNames.Margin = new System.Windows.Forms.Padding(2);
            this.cboRecipeNames.Name = "cboRecipeNames";
            this.cboRecipeNames.Size = new System.Drawing.Size(191, 26);
            this.cboRecipeNames.TabIndex = 306;
            // 
            // label10
            // 
            this.label10.BackColor = System.Drawing.Color.Transparent;
            this.label10.Font = new System.Drawing.Font("微軟正黑體", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label10.ForeColor = System.Drawing.Color.White;
            this.label10.Location = new System.Drawing.Point(16, 22);
            this.label10.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(151, 17);
            this.label10.TabIndex = 305;
            this.label10.Text = "Recipe (參數)";
            // 
            // panel4
            // 
            this.panel4.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panel4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel4.Controls.Add(this.gwLogPanel1);
            this.panel4.Location = new System.Drawing.Point(0, 255);
            this.panel4.Margin = new System.Windows.Forms.Padding(2);
            this.panel4.Name = "panel4";
            this.panel4.Padding = new System.Windows.Forms.Padding(9, 0, 9, 10);
            this.panel4.Size = new System.Drawing.Size(350, 324);
            this.panel4.TabIndex = 302;
            // 
            // gwLogPanel1
            // 
            this.gwLogPanel1.BackColor = System.Drawing.Color.Transparent;
            this.gwLogPanel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.gwLogPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gwLogPanel1.Location = new System.Drawing.Point(9, 0);
            this.gwLogPanel1.Margin = new System.Windows.Forms.Padding(2);
            this.gwLogPanel1.Name = "gwLogPanel1";
            this.gwLogPanel1.Padding = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.gwLogPanel1.Size = new System.Drawing.Size(330, 312);
            this.gwLogPanel1.TabIndex = 0;
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.panel3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel3.Controls.Add(this.btnTryRun);
            this.panel3.Controls.Add(this.btnProductionRun);
            this.panel3.Controls.Add(this.btnStop);
            this.panel3.ForeColor = System.Drawing.Color.Black;
            this.panel3.Location = new System.Drawing.Point(0, 192);
            this.panel3.Margin = new System.Windows.Forms.Padding(2);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(350, 64);
            this.panel3.TabIndex = 146;
            // 
            // btnTryRun
            // 
            this.btnTryRun.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnTryRun.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnTryRun.Font = new System.Drawing.Font("微軟正黑體", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnTryRun.Location = new System.Drawing.Point(27, 12);
            this.btnTryRun.Margin = new System.Windows.Forms.Padding(2);
            this.btnTryRun.Name = "btnTryRun";
            this.btnTryRun.Size = new System.Drawing.Size(88, 40);
            this.btnTryRun.TabIndex = 305;
            this.btnTryRun.Text = "Try Run";
            this.btnTryRun.UseVisualStyleBackColor = false;
            // 
            // btnProductionRun
            // 
            this.btnProductionRun.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.btnProductionRun.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnProductionRun.Font = new System.Drawing.Font("微軟正黑體", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnProductionRun.Location = new System.Drawing.Point(128, 12);
            this.btnProductionRun.Margin = new System.Windows.Forms.Padding(2);
            this.btnProductionRun.Name = "btnProductionRun";
            this.btnProductionRun.Size = new System.Drawing.Size(88, 40);
            this.btnProductionRun.TabIndex = 0;
            this.btnProductionRun.Text = "Start";
            this.btnProductionRun.UseVisualStyleBackColor = false;
            this.btnProductionRun.Visible = false;
            // 
            // btnStop
            // 
            this.btnStop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.btnStop.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnStop.Font = new System.Drawing.Font("微軟正黑體", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnStop.Location = new System.Drawing.Point(230, 12);
            this.btnStop.Margin = new System.Windows.Forms.Padding(2);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(88, 40);
            this.btnStop.TabIndex = 1;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = false;
            this.btnStop.Visible = false;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.panel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.panel5);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.txtSerialNo);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(350, 128);
            this.panel1.TabIndex = 147;
            // 
            // panel5
            // 
            this.panel5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.panel5.BackColor = System.Drawing.Color.Transparent;
            this.panel5.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel5.Controls.Add(this.lblUserName);
            this.panel5.Controls.Add(this.btnLogin);
            this.panel5.Location = new System.Drawing.Point(136, 63);
            this.panel5.Margin = new System.Windows.Forms.Padding(2);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(191, 52);
            this.panel5.TabIndex = 311;
            // 
            // lblUserName
            // 
            this.lblUserName.BackColor = System.Drawing.Color.Transparent;
            this.lblUserName.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUserName.ForeColor = System.Drawing.Color.Black;
            this.lblUserName.Location = new System.Drawing.Point(54, 16);
            this.lblUserName.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblUserName.Name = "lblUserName";
            this.lblUserName.Size = new System.Drawing.Size(120, 20);
            this.lblUserName.TabIndex = 149;
            this.lblUserName.Text = "Administrator";
            this.lblUserName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnLogin
            // 
            this.btnLogin.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnLogin.BackgroundImage")));
            this.btnLogin.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnLogin.Location = new System.Drawing.Point(9, 5);
            this.btnLogin.Margin = new System.Windows.Forms.Padding(2);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(40, 42);
            this.btnLogin.TabIndex = 150;
            this.btnLogin.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.Font = new System.Drawing.Font("微軟正黑體", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.MediumBlue;
            this.label3.Location = new System.Drawing.Point(16, 79);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(91, 17);
            this.label3.TabIndex = 310;
            this.label3.Text = "OP (帳號)";
            // 
            // txtSerialNo
            // 
            this.txtSerialNo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSerialNo.BackColor = System.Drawing.Color.WhiteSmoke;
            this.txtSerialNo.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSerialNo.Location = new System.Drawing.Point(136, 21);
            this.txtSerialNo.Margin = new System.Windows.Forms.Padding(2);
            this.txtSerialNo.Name = "txtSerialNo";
            this.txtSerialNo.Size = new System.Drawing.Size(191, 26);
            this.txtSerialNo.TabIndex = 302;
            this.txtSerialNo.Text = "Auto-0123456789";
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("微軟正黑體", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.MediumBlue;
            this.label2.Location = new System.Drawing.Point(16, 23);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(115, 22);
            this.label2.TabIndex = 307;
            this.label2.Text = "Serial (序號)";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // GvProductionPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.Controls.Add(this.btnSwapMapView);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.btnHome);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panel3);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "GvProductionPanel";
            this.Size = new System.Drawing.Size(418, 734);
            this.panel2.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel5.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        public System.Windows.Forms.Button btnProductionRun;
        public System.Windows.Forms.Button btnStop;
        public System.Windows.Forms.TextBox txtSerialNo;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Panel panel4;
        public System.Windows.Forms.ComboBox cboRecipeNames;
        public System.Windows.Forms.Button btnHome;
        public System.Windows.Forms.Button btnTryRun;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.Button btnSwapMapView;
        //public JetEazy.ImageViewerEx.CvBmpViewer cvBmpViewer1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Panel panel5;
        public System.Windows.Forms.Label lblUserName;
        public System.Windows.Forms.Button btnLogin;
        private EzEmptyTrayInspector.Gui.Panels.GwLogPanel gwLogPanel1;
    }
}
