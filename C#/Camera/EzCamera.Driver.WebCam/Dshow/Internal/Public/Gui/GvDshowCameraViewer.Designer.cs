using Camera_NET;

namespace JetEazy.DShow.GUI
{
    partial class GvDshowCameraViewer
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
            this.lblRecBlinker = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblRecBlinker
            // 
            this.lblRecBlinker.BackColor = System.Drawing.Color.Transparent;
            this.lblRecBlinker.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblRecBlinker.ForeColor = System.Drawing.Color.Red;
            this.lblRecBlinker.Location = new System.Drawing.Point(9, 9);
            this.lblRecBlinker.Name = "lblRecBlinker";
            this.lblRecBlinker.Size = new System.Drawing.Size(32, 32);
            this.lblRecBlinker.TabIndex = 1;
            this.lblRecBlinker.Text = "●";
            this.lblRecBlinker.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblRecBlinker.Visible = false;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Black;
            this.panel1.Controls.Add(this.lblRecBlinker);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(383, 231);
            this.panel1.TabIndex = 2;
            // 
            // GvDshowCameraViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDark;
            this.Controls.Add(this.panel1);
            this.Name = "GvDshowCameraViewer";
            this.Size = new System.Drawing.Size(383, 231);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label lblRecBlinker;
        private System.Windows.Forms.Panel panel1;
    }
}
