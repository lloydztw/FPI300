namespace AX.Gui
{
    partial class GPaneMotorSimpleJogs
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
            this.gwMotorSimpleJogH1 = new AX.Gui.GwMotorSimpleJogH();
            this.gwMotorSimpleJogV1 = new AX.Gui.GwMotorSimpleJogV();
            this.SuspendLayout();
            // 
            // gwMotorSimpleJogH1
            // 
            this.gwMotorSimpleJogH1.AxisName = "X1";
            this.gwMotorSimpleJogH1.BackColor = System.Drawing.Color.Transparent;
            this.gwMotorSimpleJogH1.Location = new System.Drawing.Point(169, 53);
            this.gwMotorSimpleJogH1.Name = "gwMotorSimpleJogH1";
            this.gwMotorSimpleJogH1.Size = new System.Drawing.Size(161, 107);
            this.gwMotorSimpleJogH1.TabIndex = 3;
            // 
            // gwMotorSimpleJogV1
            // 
            this.gwMotorSimpleJogV1.AxisName = "Z1";
            this.gwMotorSimpleJogV1.BackColor = System.Drawing.Color.Transparent;
            this.gwMotorSimpleJogV1.Location = new System.Drawing.Point(47, 21);
            this.gwMotorSimpleJogV1.Name = "gwMotorSimpleJogV1";
            this.gwMotorSimpleJogV1.Size = new System.Drawing.Size(78, 191);
            this.gwMotorSimpleJogV1.TabIndex = 2;
            // 
            // GPaneMotorSimpleJogs
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.Controls.Add(this.gwMotorSimpleJogH1);
            this.Controls.Add(this.gwMotorSimpleJogV1);
            this.Name = "GPaneMotorSimpleJogs";
            this.Size = new System.Drawing.Size(368, 247);
            this.ResumeLayout(false);

        }

        #endregion

        private GwMotorSimpleJogH gwMotorSimpleJogH1;
        private GwMotorSimpleJogV gwMotorSimpleJogV1;
    }
}
