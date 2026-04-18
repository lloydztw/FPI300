namespace LaserAlignDX.Mvc.Gui
{
    partial class FormMotors_CarierSuckerXY
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMotors_CarierSuckerXY));
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnSaveLT = new System.Windows.Forms.Button();
            this.btnSaveRT = new System.Windows.Forms.Button();
            this.btnSaveLB = new System.Windows.Forms.Button();
            this.btnSaveRB = new System.Windows.Forms.Button();
            this.panel0 = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.btnMoveLT = new System.Windows.Forms.Button();
            this.btnMoveRT = new System.Windows.Forms.Button();
            this.btnMoveRB = new System.Windows.Forms.Button();
            this.btnMoveLB = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.gwMotorSimpleGoPanel2 = new AX.Gui.GwMotorSimpleGoPanel();
            this.gwMotorSimpleGoPanel1 = new AX.Gui.GwMotorSimpleGoPanel();
            this.gvPaneMotorJogXY1 = new AX.Gui.GvPaneMotorJogXY();
            this.panel1.SuspendLayout();
            this.panel0.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.LightSteelBlue;
            this.panel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panel1.Controls.Add(this.btnOK);
            this.panel1.Controls.Add(this.btnCancel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(2, 566);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(806, 80);
            this.panel1.TabIndex = 76;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnOK.Font = new System.Drawing.Font("Microsoft YaHei UI", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOK.Location = new System.Drawing.Point(220, 15);
            this.btnOK.Margin = new System.Windows.Forms.Padding(4);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(172, 52);
            this.btnOK.TabIndex = 74;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = false;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnCancel.Font = new System.Drawing.Font("Microsoft YaHei UI", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel.Location = new System.Drawing.Point(410, 15);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(172, 52);
            this.btnCancel.TabIndex = 75;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = false;
            // 
            // btnSaveLT
            // 
            this.btnSaveLT.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSaveLT.BackColor = System.Drawing.Color.Silver;
            this.btnSaveLT.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnSaveLT.Font = new System.Drawing.Font("Arial", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSaveLT.ForeColor = System.Drawing.Color.Black;
            this.btnSaveLT.Location = new System.Drawing.Point(606, 127);
            this.btnSaveLT.Margin = new System.Windows.Forms.Padding(4);
            this.btnSaveLT.Name = "btnSaveLT";
            this.btnSaveLT.Size = new System.Drawing.Size(28, 28);
            this.btnSaveLT.TabIndex = 79;
            this.btnSaveLT.Text = "●";
            this.btnSaveLT.UseVisualStyleBackColor = false;
            // 
            // btnSaveRT
            // 
            this.btnSaveRT.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSaveRT.BackColor = System.Drawing.Color.Silver;
            this.btnSaveRT.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnSaveRT.Font = new System.Drawing.Font("Arial", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSaveRT.ForeColor = System.Drawing.Color.Black;
            this.btnSaveRT.Location = new System.Drawing.Point(734, 127);
            this.btnSaveRT.Margin = new System.Windows.Forms.Padding(4);
            this.btnSaveRT.Name = "btnSaveRT";
            this.btnSaveRT.Size = new System.Drawing.Size(28, 28);
            this.btnSaveRT.TabIndex = 80;
            this.btnSaveRT.Text = "●";
            this.btnSaveRT.UseVisualStyleBackColor = false;
            // 
            // btnSaveLB
            // 
            this.btnSaveLB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSaveLB.BackColor = System.Drawing.Color.Silver;
            this.btnSaveLB.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnSaveLB.Font = new System.Drawing.Font("Arial", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSaveLB.ForeColor = System.Drawing.Color.Black;
            this.btnSaveLB.Location = new System.Drawing.Point(606, 255);
            this.btnSaveLB.Margin = new System.Windows.Forms.Padding(4);
            this.btnSaveLB.Name = "btnSaveLB";
            this.btnSaveLB.Size = new System.Drawing.Size(28, 28);
            this.btnSaveLB.TabIndex = 81;
            this.btnSaveLB.Text = "●";
            this.btnSaveLB.UseVisualStyleBackColor = false;
            // 
            // btnSaveRB
            // 
            this.btnSaveRB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSaveRB.BackColor = System.Drawing.Color.Silver;
            this.btnSaveRB.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnSaveRB.Font = new System.Drawing.Font("Arial", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSaveRB.ForeColor = System.Drawing.Color.Black;
            this.btnSaveRB.Location = new System.Drawing.Point(736, 256);
            this.btnSaveRB.Margin = new System.Windows.Forms.Padding(4);
            this.btnSaveRB.Name = "btnSaveRB";
            this.btnSaveRB.Size = new System.Drawing.Size(28, 28);
            this.btnSaveRB.TabIndex = 82;
            this.btnSaveRB.Text = "●";
            this.btnSaveRB.UseVisualStyleBackColor = false;
            // 
            // panel0
            // 
            this.panel0.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.panel0.BackgroundImage = global::LaserAlignDX.Properties.Resources.CommonPanel;
            this.panel0.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panel0.Controls.Add(this.lblTitle);
            this.panel0.Controls.Add(this.button1);
            this.panel0.Controls.Add(this.button2);
            this.panel0.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel0.Location = new System.Drawing.Point(2, 0);
            this.panel0.Name = "panel0";
            this.panel0.Size = new System.Drawing.Size(806, 72);
            this.panel0.TabIndex = 83;
            // 
            // lblTitle
            // 
            this.lblTitle.BackColor = System.Drawing.Color.Transparent;
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblTitle.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(0, 0);
            this.lblTitle.Margin = new System.Windows.Forms.Padding(0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Padding = new System.Windows.Forms.Padding(6, 2, 0, 0);
            this.lblTitle.Size = new System.Drawing.Size(392, 72);
            this.lblTitle.TabIndex = 101;
            this.lblTitle.Text = "點墨待命位置X= -999.000\r\n點墨待命位置Y= -999.000";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button1.Font = new System.Drawing.Font("Microsoft YaHei UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(590, 16);
            this.button1.Margin = new System.Windows.Forms.Padding(4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(172, 38);
            this.button1.TabIndex = 74;
            this.button1.Text = "GoTo 待命位置";
            this.button1.UseVisualStyleBackColor = false;
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button2.Font = new System.Drawing.Font("Microsoft YaHei UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button2.Location = new System.Drawing.Point(410, 16);
            this.button2.Margin = new System.Windows.Forms.Padding(4);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(172, 38);
            this.button2.TabIndex = 75;
            this.button2.Text = "Set 待命位置";
            this.button2.UseVisualStyleBackColor = false;
            // 
            // btnMoveLT
            // 
            this.btnMoveLT.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMoveLT.BackColor = System.Drawing.Color.Silver;
            this.btnMoveLT.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnMoveLT.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMoveLT.ForeColor = System.Drawing.Color.Blue;
            this.btnMoveLT.Location = new System.Drawing.Point(580, 101);
            this.btnMoveLT.Margin = new System.Windows.Forms.Padding(4);
            this.btnMoveLT.Name = "btnMoveLT";
            this.btnMoveLT.Size = new System.Drawing.Size(28, 28);
            this.btnMoveLT.TabIndex = 84;
            this.btnMoveLT.Text = "↖";
            this.btnMoveLT.UseVisualStyleBackColor = false;
            // 
            // btnMoveRT
            // 
            this.btnMoveRT.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMoveRT.BackColor = System.Drawing.Color.Silver;
            this.btnMoveRT.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnMoveRT.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMoveRT.ForeColor = System.Drawing.Color.Blue;
            this.btnMoveRT.Location = new System.Drawing.Point(762, 100);
            this.btnMoveRT.Margin = new System.Windows.Forms.Padding(4);
            this.btnMoveRT.Name = "btnMoveRT";
            this.btnMoveRT.Size = new System.Drawing.Size(28, 28);
            this.btnMoveRT.TabIndex = 85;
            this.btnMoveRT.Text = "↗";
            this.btnMoveRT.UseVisualStyleBackColor = false;
            // 
            // btnMoveRB
            // 
            this.btnMoveRB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMoveRB.BackColor = System.Drawing.Color.Silver;
            this.btnMoveRB.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnMoveRB.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMoveRB.ForeColor = System.Drawing.Color.Blue;
            this.btnMoveRB.Location = new System.Drawing.Point(762, 284);
            this.btnMoveRB.Margin = new System.Windows.Forms.Padding(4);
            this.btnMoveRB.Name = "btnMoveRB";
            this.btnMoveRB.Size = new System.Drawing.Size(28, 28);
            this.btnMoveRB.TabIndex = 87;
            this.btnMoveRB.Text = "↘";
            this.btnMoveRB.UseVisualStyleBackColor = false;
            // 
            // btnMoveLB
            // 
            this.btnMoveLB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMoveLB.BackColor = System.Drawing.Color.Silver;
            this.btnMoveLB.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnMoveLB.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMoveLB.ForeColor = System.Drawing.Color.Blue;
            this.btnMoveLB.Location = new System.Drawing.Point(580, 285);
            this.btnMoveLB.Margin = new System.Windows.Forms.Padding(4);
            this.btnMoveLB.Name = "btnMoveLB";
            this.btnMoveLB.Size = new System.Drawing.Size(28, 28);
            this.btnMoveLB.TabIndex = 86;
            this.btnMoveLB.Text = "↙";
            this.btnMoveLB.UseVisualStyleBackColor = false;
            // 
            // gwMotorSimpleGoPanel2
            // 
            this.gwMotorSimpleGoPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.gwMotorSimpleGoPanel2.AxisName = "點墨 下壓 Z";
            this.gwMotorSimpleGoPanel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.gwMotorSimpleGoPanel2.Location = new System.Drawing.Point(578, 459);
            this.gwMotorSimpleGoPanel2.Name = "gwMotorSimpleGoPanel2";
            this.gwMotorSimpleGoPanel2.Padding = new System.Windows.Forms.Padding(0, 2, 0, 5);
            this.gwMotorSimpleGoPanel2.Size = new System.Drawing.Size(212, 88);
            this.gwMotorSimpleGoPanel2.TabIndex = 78;
            // 
            // gwMotorSimpleGoPanel1
            // 
            this.gwMotorSimpleGoPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.gwMotorSimpleGoPanel1.AxisName = "點墨 歸位 Z";
            this.gwMotorSimpleGoPanel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.gwMotorSimpleGoPanel1.Location = new System.Drawing.Point(578, 371);
            this.gwMotorSimpleGoPanel1.Name = "gwMotorSimpleGoPanel1";
            this.gwMotorSimpleGoPanel1.Padding = new System.Windows.Forms.Padding(0, 2, 0, 5);
            this.gwMotorSimpleGoPanel1.Size = new System.Drawing.Size(212, 88);
            this.gwMotorSimpleGoPanel1.TabIndex = 77;
            // 
            // gvPaneMotorJogXY1
            // 
            this.gvPaneMotorJogXY1.AxisName1 = "X Axis Status";
            this.gvPaneMotorJogXY1.AxisName2 = "Y Axis Status";
            this.gvPaneMotorJogXY1.BackColor = System.Drawing.Color.Gray;
            this.gvPaneMotorJogXY1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("gvPaneMotorJogXY1.BackgroundImage")));
            this.gvPaneMotorJogXY1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.gvPaneMotorJogXY1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gvPaneMotorJogXY1.Location = new System.Drawing.Point(2, 72);
            this.gvPaneMotorJogXY1.Name = "gvPaneMotorJogXY1";
            this.gvPaneMotorJogXY1.Padding = new System.Windows.Forms.Padding(0, 8, 12, 8);
            this.gvPaneMotorJogXY1.Size = new System.Drawing.Size(806, 494);
            this.gvPaneMotorJogXY1.TabIndex = 3;
            // 
            // FormMotors_CarierSuckerXY
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.LightSteelBlue;
            this.ClientSize = new System.Drawing.Size(810, 648);
            this.Controls.Add(this.btnMoveRB);
            this.Controls.Add(this.btnMoveLB);
            this.Controls.Add(this.btnMoveRT);
            this.Controls.Add(this.btnMoveLT);
            this.Controls.Add(this.btnSaveRB);
            this.Controls.Add(this.btnSaveLB);
            this.Controls.Add(this.btnSaveRT);
            this.Controls.Add(this.btnSaveLT);
            this.Controls.Add(this.gwMotorSimpleGoPanel2);
            this.Controls.Add(this.gwMotorSimpleGoPanel1);
            this.Controls.Add(this.gvPaneMotorJogXY1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panel0);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormMotors_CarierSuckerXY";
            this.Padding = new System.Windows.Forms.Padding(2, 0, 2, 2);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Motors CarrierY SuckerX";
            this.panel1.ResumeLayout(false);
            this.panel0.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private AX.Gui.GvPaneMotorJogXY gvPaneMotorJogXY1;
        private System.Windows.Forms.Panel panel1;
        private AX.Gui.GwMotorSimpleGoPanel gwMotorSimpleGoPanel1;
        private AX.Gui.GwMotorSimpleGoPanel gwMotorSimpleGoPanel2;
        public System.Windows.Forms.Button btnSaveLT;
        public System.Windows.Forms.Button btnSaveRT;
        public System.Windows.Forms.Button btnSaveLB;
        public System.Windows.Forms.Button btnSaveRB;
        private System.Windows.Forms.Panel panel0;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        public System.Windows.Forms.Button btnMoveLT;
        public System.Windows.Forms.Button btnMoveRT;
        public System.Windows.Forms.Button btnMoveRB;
        public System.Windows.Forms.Button btnMoveLB;
        private System.Windows.Forms.ToolTip toolTip1;
        internal System.Windows.Forms.Label lblTitle;
    }
}