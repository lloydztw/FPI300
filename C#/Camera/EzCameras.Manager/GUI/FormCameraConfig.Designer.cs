namespace EzCamera.GUI
{
    partial class FormCameraConfig
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormCameraConfig));
            this.lstAppDeviceInfos = new System.Windows.Forms.ListBox();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnDel = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnDsCamSettings = new System.Windows.Forms.Button();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnDsVideoSettings = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnImport = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.lstGlobalDeviceInfos = new System.Windows.Forms.ListBox();
            this.btnMovUp = new System.Windows.Forms.Button();
            this.btnMovDn = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // lstAppDeviceInfos
            // 
            this.lstAppDeviceInfos.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.lstAppDeviceInfos.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lstAppDeviceInfos.FormattingEnabled = true;
            this.lstAppDeviceInfos.HorizontalScrollbar = true;
            this.lstAppDeviceInfos.ItemHeight = 25;
            this.lstAppDeviceInfos.Location = new System.Drawing.Point(19, 102);
            this.lstAppDeviceInfos.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.lstAppDeviceInfos.Name = "lstAppDeviceInfos";
            this.lstAppDeviceInfos.Size = new System.Drawing.Size(396, 354);
            this.lstAppDeviceInfos.TabIndex = 6;
            // 
            // btnAdd
            // 
            this.btnAdd.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnAdd.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.btnAdd.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnAdd.Font = new System.Drawing.Font("Calibri", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAdd.Location = new System.Drawing.Point(457, 155);
            this.btnAdd.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(67, 48);
            this.btnAdd.TabIndex = 308;
            this.btnAdd.Text = "←";
            this.btnAdd.UseVisualStyleBackColor = false;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.btnOK.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnOK.Font = new System.Drawing.Font("微軟正黑體", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnOK.Location = new System.Drawing.Point(53, 485);
            this.btnOK.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(157, 48);
            this.btnOK.TabIndex = 306;
            this.btnOK.Text = "Ok";
            this.btnOK.UseVisualStyleBackColor = false;
            // 
            // btnDel
            // 
            this.btnDel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnDel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.btnDel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnDel.Font = new System.Drawing.Font("Calibri", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDel.Location = new System.Drawing.Point(457, 206);
            this.btnDel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnDel.Name = "btnDel";
            this.btnDel.Size = new System.Drawing.Size(67, 48);
            this.btnDel.TabIndex = 307;
            this.btnDel.Text = "→";
            this.btnDel.UseVisualStyleBackColor = false;
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.btnDsCamSettings);
            this.panel1.Controls.Add(this.btnCancel);
            this.panel1.Controls.Add(this.lstAppDeviceInfos);
            this.panel1.Controls.Add(this.btnDsVideoSettings);
            this.panel1.Controls.Add(this.btnOK);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(8, 10);
            this.panel1.Margin = new System.Windows.Forms.Padding(4);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(16, 10, 16, 2);
            this.panel1.Size = new System.Drawing.Size(437, 555);
            this.panel1.TabIndex = 310;
            // 
            // btnDsCamSettings
            // 
            this.btnDsCamSettings.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btnDsCamSettings.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnDsCamSettings.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnDsCamSettings.Font = new System.Drawing.Font("微軟正黑體", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnDsCamSettings.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnDsCamSettings.ImageList = this.imageList1;
            this.btnDsCamSettings.Location = new System.Drawing.Point(247, 52);
            this.btnDsCamSettings.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnDsCamSettings.Name = "btnDsCamSettings";
            this.btnDsCamSettings.Padding = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.btnDsCamSettings.Size = new System.Drawing.Size(157, 32);
            this.btnDsCamSettings.TabIndex = 314;
            this.btnDsCamSettings.Text = "DS 相機 設定";
            this.btnDsCamSettings.UseVisualStyleBackColor = false;
            this.btnDsCamSettings.Visible = false;
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "open-folder");
            this.imageList1.Images.SetKeyName(1, "preferences.png");
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.btnCancel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnCancel.Font = new System.Drawing.Font("微軟正黑體", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnCancel.Location = new System.Drawing.Point(216, 485);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(157, 48);
            this.btnCancel.TabIndex = 312;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = false;
            // 
            // btnDsVideoSettings
            // 
            this.btnDsVideoSettings.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btnDsVideoSettings.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnDsVideoSettings.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnDsVideoSettings.Font = new System.Drawing.Font("微軟正黑體", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnDsVideoSettings.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnDsVideoSettings.ImageList = this.imageList1;
            this.btnDsVideoSettings.Location = new System.Drawing.Point(247, 21);
            this.btnDsVideoSettings.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnDsVideoSettings.Name = "btnDsVideoSettings";
            this.btnDsVideoSettings.Padding = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.btnDsVideoSettings.Size = new System.Drawing.Size(157, 32);
            this.btnDsVideoSettings.TabIndex = 313;
            this.btnDsVideoSettings.Text = "DS 視頻 組態";
            this.btnDsVideoSettings.UseVisualStyleBackColor = false;
            this.btnDsVideoSettings.Visible = false;
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.label1.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label1.Location = new System.Drawing.Point(16, 10);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(403, 70);
            this.label1.TabIndex = 310;
            this.label1.Text = "App Active Cameras\r\n目前可使用的相機";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panel2
            // 
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.btnImport);
            this.panel2.Controls.Add(this.label2);
            this.panel2.Controls.Add(this.lstGlobalDeviceInfos);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel2.Location = new System.Drawing.Point(539, 10);
            this.panel2.Margin = new System.Windows.Forms.Padding(4);
            this.panel2.Name = "panel2";
            this.panel2.Padding = new System.Windows.Forms.Padding(16, 10, 16, 2);
            this.panel2.Size = new System.Drawing.Size(454, 555);
            this.panel2.TabIndex = 311;
            // 
            // btnImport
            // 
            this.btnImport.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnImport.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.btnImport.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnImport.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnImport.Location = new System.Drawing.Point(143, 486);
            this.btnImport.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(157, 48);
            this.btnImport.TabIndex = 314;
            this.btnImport.Text = "Import (匯入)";
            this.btnImport.UseVisualStyleBackColor = false;
            // 
            // label2
            // 
            this.label2.Dock = System.Windows.Forms.DockStyle.Top;
            this.label2.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label2.Location = new System.Drawing.Point(16, 10);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(420, 70);
            this.label2.TabIndex = 310;
            this.label2.Text = "Sys Available Cameras\r\n系統可用的相機";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lstGlobalDeviceInfos
            // 
            this.lstGlobalDeviceInfos.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.lstGlobalDeviceInfos.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lstGlobalDeviceInfos.FormattingEnabled = true;
            this.lstGlobalDeviceInfos.HorizontalScrollbar = true;
            this.lstGlobalDeviceInfos.ItemHeight = 25;
            this.lstGlobalDeviceInfos.Location = new System.Drawing.Point(19, 102);
            this.lstGlobalDeviceInfos.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.lstGlobalDeviceInfos.Name = "lstGlobalDeviceInfos";
            this.lstGlobalDeviceInfos.Size = new System.Drawing.Size(414, 354);
            this.lstGlobalDeviceInfos.TabIndex = 6;
            // 
            // btnMovUp
            // 
            this.btnMovUp.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnMovUp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnMovUp.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnMovUp.Font = new System.Drawing.Font("Calibri", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMovUp.Location = new System.Drawing.Point(457, 258);
            this.btnMovUp.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnMovUp.Name = "btnMovUp";
            this.btnMovUp.Size = new System.Drawing.Size(67, 48);
            this.btnMovUp.TabIndex = 313;
            this.btnMovUp.Text = "↑";
            this.btnMovUp.UseVisualStyleBackColor = false;
            // 
            // btnMovDn
            // 
            this.btnMovDn.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnMovDn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnMovDn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnMovDn.Font = new System.Drawing.Font("Calibri", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMovDn.Location = new System.Drawing.Point(457, 309);
            this.btnMovDn.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnMovDn.Name = "btnMovDn";
            this.btnMovDn.Size = new System.Drawing.Size(67, 48);
            this.btnMovDn.TabIndex = 312;
            this.btnMovDn.Text = "↓";
            this.btnMovDn.UseVisualStyleBackColor = false;
            // 
            // FormCameraConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(1001, 575);
            this.Controls.Add(this.btnMovUp);
            this.Controls.Add(this.btnMovDn);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.btnDel);
            this.Font = new System.Drawing.Font("新細明體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormCameraConfig";
            this.Padding = new System.Windows.Forms.Padding(8, 10, 8, 10);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Cameras Config (相機組態)";
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ListBox lstAppDeviceInfos;
        public System.Windows.Forms.Button btnAdd;
        public System.Windows.Forms.Button btnOK;
        public System.Windows.Forms.Button btnDel;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListBox lstGlobalDeviceInfos;
        public System.Windows.Forms.Button btnMovUp;
        public System.Windows.Forms.Button btnMovDn;
        public System.Windows.Forms.Button btnImport;
        private System.Windows.Forms.ImageList imageList1;
        public System.Windows.Forms.Button btnCancel;
        public System.Windows.Forms.Button btnDsVideoSettings;
        public System.Windows.Forms.Button btnDsCamSettings;
    }
}