namespace AX.Gui
{
    partial class GwMotorCommander
    {
        /// <summary> 
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置 Managed 資源則為 true，否則為 false。</param>
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
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器
        /// 修改這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.gwMotorJogSticks1 = new AX.Gui.GwMotorJogSticks();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.gwMotorMoveCmd1D = new AX.Gui.GwMotorMoveCmd();
            this.gwMotorMoveCmdXY = new AX.Gui.GwMotorMoveCmdXY();
            this.gwSpeedHiLoButtons1 = new AX.Gui.GwSpeedHiLoButtons();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox2
            // 
            this.groupBox2.BackColor = System.Drawing.Color.Transparent;
            this.groupBox2.Controls.Add(this.gwMotorJogSticks1);
            this.groupBox2.Font = new System.Drawing.Font("新細明體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox2.Location = new System.Drawing.Point(195, 5);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(3, 3, 3, 12);
            this.groupBox2.Size = new System.Drawing.Size(168, 160);
            this.groupBox2.TabIndex = 316;
            this.groupBox2.TabStop = false;
            // 
            // gwMotorJogSticks1
            // 
            this.gwMotorJogSticks1.BackColor = System.Drawing.Color.Transparent;
            this.gwMotorJogSticks1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gwMotorJogSticks1.Location = new System.Drawing.Point(3, 21);
            this.gwMotorJogSticks1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.gwMotorJogSticks1.Name = "gwMotorJogSticks1";
            this.gwMotorJogSticks1.JogStickOption = AX.Gui.JogStickOption.XY;
            this.gwMotorJogSticks1.Size = new System.Drawing.Size(162, 127);
            this.gwMotorJogSticks1.TabIndex = 323;
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.Transparent;
            this.groupBox1.Controls.Add(this.gwMotorMoveCmdXY);
            this.groupBox1.Controls.Add(this.gwMotorMoveCmd1D);
            this.groupBox1.Font = new System.Drawing.Font("Verdana", 9.163636F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.ForeColor = System.Drawing.Color.White;
            this.groupBox1.Location = new System.Drawing.Point(6, 5);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox1.Size = new System.Drawing.Size(188, 160);
            this.groupBox1.TabIndex = 315;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Move To";
            // 
            // gwMotorMoveCmd1D
            // 
            this.gwMotorMoveCmd1D.BackColor = System.Drawing.Color.Transparent;
            this.gwMotorMoveCmd1D.Font = new System.Drawing.Font("新細明體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.gwMotorMoveCmd1D.Location = new System.Drawing.Point(17, 39);
            this.gwMotorMoveCmd1D.Name = "gwMotorMoveCmd1D";
            this.gwMotorMoveCmd1D.Size = new System.Drawing.Size(186, 84);
            this.gwMotorMoveCmd1D.TabIndex = 321;
            // 
            // gwMotorMoveCmdXY
            // 
            this.gwMotorMoveCmdXY.BackColor = System.Drawing.Color.Transparent;
            this.gwMotorMoveCmdXY.Font = new System.Drawing.Font("新細明體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.gwMotorMoveCmdXY.Location = new System.Drawing.Point(17, 27);
            this.gwMotorMoveCmdXY.Margin = new System.Windows.Forms.Padding(4);
            this.gwMotorMoveCmdXY.Name = "gwMotorMoveCmdXY";
            this.gwMotorMoveCmdXY.Padding = new System.Windows.Forms.Padding(2);
            this.gwMotorMoveCmdXY.Size = new System.Drawing.Size(190, 114);
            this.gwMotorMoveCmdXY.TabIndex = 321;
            // 
            // gwSpeedHiLoButtons1
            // 
            this.gwSpeedHiLoButtons1.BackColor = System.Drawing.Color.Transparent;
            this.gwSpeedHiLoButtons1.Location = new System.Drawing.Point(367, 15);
            this.gwSpeedHiLoButtons1.Name = "gwSpeedHiLoButtons1";
            this.gwSpeedHiLoButtons1.Size = new System.Drawing.Size(75, 150);
            this.gwSpeedHiLoButtons1.TabIndex = 320;
            // 
            // GwMotorCommander
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DimGray;
            this.Controls.Add(this.gwSpeedHiLoButtons1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "GwMotorCommander";
            this.Padding = new System.Windows.Forms.Padding(2);
            this.Size = new System.Drawing.Size(469, 175);
            this.groupBox2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox1;
        private GwMotorMoveCmd gwMotorMoveCmd1D;
        public GwSpeedHiLoButtons gwSpeedHiLoButtons1;
        public GwMotorJogSticks gwMotorJogSticks1;
        public GwMotorMoveCmdXY gwMotorMoveCmdXY;
    }
}
