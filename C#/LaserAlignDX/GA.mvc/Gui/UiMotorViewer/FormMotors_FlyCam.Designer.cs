namespace LaserAlignDX.Mvc.Gui
{
    partial class FormMotors_FlyCam
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMotors_FlyCam));
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.gwMotorSimpleGoPanel1 = new AX.Gui.GwMotorSimpleGoPanel();
            this.gvPaneMotorJogXY1 = new AX.Gui.GvPaneMotorJogXY();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.panel0 = new System.Windows.Forms.Panel();
            this.button3 = new System.Windows.Forms.Button();
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
            this.panel1.Location = new System.Drawing.Point(0, 570);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(782, 65);
            this.panel1.TabIndex = 76;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnOK.Font = new System.Drawing.Font("Microsoft YaHei UI", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOK.Location = new System.Drawing.Point(390, 14);
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
            this.btnCancel.Location = new System.Drawing.Point(570, 14);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(172, 38);
            this.btnCancel.TabIndex = 75;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = false;
            // 
            // gwMotorSimpleGoPanel1
            // 
            this.gwMotorSimpleGoPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.gwMotorSimpleGoPanel1.AxisName = "飛拍 焦距 Z";
            this.gwMotorSimpleGoPanel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.gwMotorSimpleGoPanel1.Location = new System.Drawing.Point(553, 462);
            this.gwMotorSimpleGoPanel1.Name = "gwMotorSimpleGoPanel1";
            this.gwMotorSimpleGoPanel1.Padding = new System.Windows.Forms.Padding(0, 2, 0, 5);
            this.gwMotorSimpleGoPanel1.Size = new System.Drawing.Size(212, 88);
            this.gwMotorSimpleGoPanel1.TabIndex = 77;
            // 
            // gvPaneMotorJogXY1
            // 
            this.gvPaneMotorJogXY1.AxisName1 = "飛拍 X (Sucker1)";
            this.gvPaneMotorJogXY1.AxisName2 = "飛拍 Y (Camera)";
            this.gvPaneMotorJogXY1.BackColor = System.Drawing.Color.Gray;
            this.gvPaneMotorJogXY1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("gvPaneMotorJogXY1.BackgroundImage")));
            this.gvPaneMotorJogXY1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.gvPaneMotorJogXY1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gvPaneMotorJogXY1.Location = new System.Drawing.Point(0, 74);
            this.gvPaneMotorJogXY1.Name = "gvPaneMotorJogXY1";
            this.gvPaneMotorJogXY1.Padding = new System.Windows.Forms.Padding(0, 8, 12, 8);
            this.gvPaneMotorJogXY1.Size = new System.Drawing.Size(782, 496);
            this.gvPaneMotorJogXY1.TabIndex = 3;
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button1.Font = new System.Drawing.Font("Microsoft YaHei UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(53, 18);
            this.button1.Margin = new System.Windows.Forms.Padding(4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(212, 38);
            this.button1.TabIndex = 76;
            this.button1.Text = "GoTo 觸發位 (X)";
            this.button1.UseVisualStyleBackColor = false;
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button2.Font = new System.Drawing.Font("Microsoft YaHei UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button2.Location = new System.Drawing.Point(285, 18);
            this.button2.Margin = new System.Windows.Forms.Padding(4);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(212, 38);
            this.button2.TabIndex = 78;
            this.button2.Text = "GoTo 飛拍位 (Y)";
            this.button2.UseVisualStyleBackColor = false;
            // 
            // panel0
            // 
            this.panel0.BackColor = System.Drawing.Color.LightSteelBlue;
            this.panel0.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panel0.Controls.Add(this.button3);
            this.panel0.Controls.Add(this.button2);
            this.panel0.Controls.Add(this.button1);
            this.panel0.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel0.Location = new System.Drawing.Point(0, 0);
            this.panel0.Name = "panel0";
            this.panel0.Size = new System.Drawing.Size(782, 74);
            this.panel0.TabIndex = 79;
            // 
            // button3
            // 
            this.button3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.button3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button3.Font = new System.Drawing.Font("Microsoft YaHei UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button3.Location = new System.Drawing.Point(517, 18);
            this.button3.Margin = new System.Windows.Forms.Padding(4);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(212, 38);
            this.button3.TabIndex = 79;
            this.button3.Text = "真空 ON";
            this.button3.UseVisualStyleBackColor = false;
            // 
            // FormMotors_FlyCam
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.ClientSize = new System.Drawing.Size(782, 635);
            this.Controls.Add(this.gwMotorSimpleGoPanel1);
            this.Controls.Add(this.gvPaneMotorJogXY1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panel0);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormMotors_FlyCam";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Motors FlyCam";
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
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Panel panel0;
        private System.Windows.Forms.Button button3;
    }
}