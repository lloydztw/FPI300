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
            this.gwMotorSimpleGoPanel2 = new AX.Gui.GwMotorSimpleGoPanel();
            this.gwMotorSimpleGoPanel1 = new AX.Gui.GwMotorSimpleGoPanel();
            this.gvPaneMotorJogXY1 = new AX.Gui.GvPaneMotorJogXY();
            this.btnSaveLT = new System.Windows.Forms.Button();
            this.btnSaveRT = new System.Windows.Forms.Button();
            this.btnSaveLB = new System.Windows.Forms.Button();
            this.btnSaveRB = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.LightSteelBlue;
            this.panel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panel1.Controls.Add(this.btnOK);
            this.panel1.Controls.Add(this.btnCancel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 511);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(782, 62);
            this.panel1.TabIndex = 76;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnOK.Font = new System.Drawing.Font("Microsoft YaHei UI", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOK.Location = new System.Drawing.Point(390, 11);
            this.btnOK.Margin = new System.Windows.Forms.Padding(4);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(172, 38);
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
            this.btnCancel.Location = new System.Drawing.Point(570, 11);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(172, 38);
            this.btnCancel.TabIndex = 75;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = false;
            // 
            // gwMotorSimpleGoPanel2
            // 
            this.gwMotorSimpleGoPanel2.AxisName = "點墨 下壓 Z";
            this.gwMotorSimpleGoPanel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.gwMotorSimpleGoPanel2.Location = new System.Drawing.Point(555, 403);
            this.gwMotorSimpleGoPanel2.Name = "gwMotorSimpleGoPanel2";
            this.gwMotorSimpleGoPanel2.Padding = new System.Windows.Forms.Padding(0, 2, 0, 5);
            this.gwMotorSimpleGoPanel2.Size = new System.Drawing.Size(212, 88);
            this.gwMotorSimpleGoPanel2.TabIndex = 78;
            // 
            // gwMotorSimpleGoPanel1
            // 
            this.gwMotorSimpleGoPanel1.AxisName = "點墨 歸位 Z";
            this.gwMotorSimpleGoPanel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.gwMotorSimpleGoPanel1.Location = new System.Drawing.Point(555, 315);
            this.gwMotorSimpleGoPanel1.Name = "gwMotorSimpleGoPanel1";
            this.gwMotorSimpleGoPanel1.Padding = new System.Windows.Forms.Padding(0, 2, 0, 5);
            this.gwMotorSimpleGoPanel1.Size = new System.Drawing.Size(212, 88);
            this.gwMotorSimpleGoPanel1.TabIndex = 77;
            // 
            // gvPaneMotorJogXY1
            // 
            this.gvPaneMotorJogXY1.BackColor = System.Drawing.Color.Gray;
            this.gvPaneMotorJogXY1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("gvPaneMotorJogXY1.BackgroundImage")));
            this.gvPaneMotorJogXY1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.gvPaneMotorJogXY1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gvPaneMotorJogXY1.Location = new System.Drawing.Point(0, 0);
            this.gvPaneMotorJogXY1.Name = "gvPaneMotorJogXY1";
            this.gvPaneMotorJogXY1.Padding = new System.Windows.Forms.Padding(0, 8, 12, 8);
            this.gvPaneMotorJogXY1.Size = new System.Drawing.Size(782, 511);
            this.gvPaneMotorJogXY1.TabIndex = 3;
            // 
            // btnSaveLT
            // 
            this.btnSaveLT.BackColor = System.Drawing.Color.Silver;
            this.btnSaveLT.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnSaveLT.Font = new System.Drawing.Font("Arial Narrow", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSaveLT.Location = new System.Drawing.Point(542, 23);
            this.btnSaveLT.Margin = new System.Windows.Forms.Padding(4);
            this.btnSaveLT.Name = "btnSaveLT";
            this.btnSaveLT.Size = new System.Drawing.Size(57, 54);
            this.btnSaveLT.TabIndex = 79;
            this.btnSaveLT.Text = "左上\r\n記入";
            this.btnSaveLT.UseVisualStyleBackColor = false;
            // 
            // btnSaveRT
            // 
            this.btnSaveRT.BackColor = System.Drawing.Color.Silver;
            this.btnSaveRT.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnSaveRT.Font = new System.Drawing.Font("Arial Narrow", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSaveRT.Location = new System.Drawing.Point(716, 23);
            this.btnSaveRT.Margin = new System.Windows.Forms.Padding(4);
            this.btnSaveRT.Name = "btnSaveRT";
            this.btnSaveRT.Size = new System.Drawing.Size(57, 54);
            this.btnSaveRT.TabIndex = 80;
            this.btnSaveRT.Text = "右上\r\n記入";
            this.btnSaveRT.UseVisualStyleBackColor = false;
            // 
            // btnSaveLB
            // 
            this.btnSaveLB.BackColor = System.Drawing.Color.Silver;
            this.btnSaveLB.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnSaveLB.Font = new System.Drawing.Font("Arial Narrow", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSaveLB.Location = new System.Drawing.Point(542, 194);
            this.btnSaveLB.Margin = new System.Windows.Forms.Padding(4);
            this.btnSaveLB.Name = "btnSaveLB";
            this.btnSaveLB.Size = new System.Drawing.Size(57, 54);
            this.btnSaveLB.TabIndex = 81;
            this.btnSaveLB.Text = "左下\r\n記入";
            this.btnSaveLB.UseVisualStyleBackColor = false;
            // 
            // btnSaveRB
            // 
            this.btnSaveRB.BackColor = System.Drawing.Color.Silver;
            this.btnSaveRB.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnSaveRB.Font = new System.Drawing.Font("Arial Narrow", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSaveRB.Location = new System.Drawing.Point(716, 194);
            this.btnSaveRB.Margin = new System.Windows.Forms.Padding(4);
            this.btnSaveRB.Name = "btnSaveRB";
            this.btnSaveRB.Size = new System.Drawing.Size(57, 54);
            this.btnSaveRB.TabIndex = 82;
            this.btnSaveRB.Text = "右下\r\n記入";
            this.btnSaveRB.UseVisualStyleBackColor = false;
            // 
            // FormMotors_CarierSuckerXY
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.ClientSize = new System.Drawing.Size(782, 573);
            this.Controls.Add(this.btnSaveRB);
            this.Controls.Add(this.btnSaveLB);
            this.Controls.Add(this.btnSaveRT);
            this.Controls.Add(this.btnSaveLT);
            this.Controls.Add(this.gwMotorSimpleGoPanel2);
            this.Controls.Add(this.gwMotorSimpleGoPanel1);
            this.Controls.Add(this.gvPaneMotorJogXY1);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormMotors_CarierSuckerXY";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Motors CarrierY SuckerX";
            this.panel1.ResumeLayout(false);
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
    }
}