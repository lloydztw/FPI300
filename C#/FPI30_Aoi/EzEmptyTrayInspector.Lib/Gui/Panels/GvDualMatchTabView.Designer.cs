namespace EzDualMatch.Gui.Panels
{
    partial class GvDualMatchTabView
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GvDualMatchTabView));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.gvSingleMatchViewPanel1 = new EzDualMatch.Gui.Panels.GvSingleMatchViewPanel();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.gvSingleMatchViewPanel2 = new EzDualMatch.Gui.Panels.GvSingleMatchViewPanel();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.tabControl1.ImageList = this.imageList1;
            this.tabControl1.ItemSize = new System.Drawing.Size(142, 36);
            this.tabControl1.Location = new System.Drawing.Point(0, 2);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.Padding = new System.Drawing.Point(0, 0);
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(567, 465);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.gvSingleMatchViewPanel1);
            this.tabPage1.ImageIndex = 1;
            this.tabPage1.Location = new System.Drawing.Point(4, 40);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(0);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Size = new System.Drawing.Size(559, 421);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "VisionSrc #1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // gvSingleMatchViewPanel1
            // 
            this.gvSingleMatchViewPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gvSingleMatchViewPanel1.Location = new System.Drawing.Point(0, 0);
            this.gvSingleMatchViewPanel1.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.gvSingleMatchViewPanel1.Name = "gvSingleMatchViewPanel1";
            this.gvSingleMatchViewPanel1.Size = new System.Drawing.Size(559, 421);
            this.gvSingleMatchViewPanel1.TabIndex = 0;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.gvSingleMatchViewPanel2);
            this.tabPage2.ImageIndex = 1;
            this.tabPage2.Location = new System.Drawing.Point(4, 40);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(0);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Size = new System.Drawing.Size(559, 421);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "VisionSrc #2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // gvSingleMatchViewPanel2
            // 
            this.gvSingleMatchViewPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gvSingleMatchViewPanel2.Location = new System.Drawing.Point(0, 0);
            this.gvSingleMatchViewPanel2.Margin = new System.Windows.Forms.Padding(2, 4, 2, 4);
            this.gvSingleMatchViewPanel2.Name = "gvSingleMatchViewPanel2";
            this.gvSingleMatchViewPanel2.Size = new System.Drawing.Size(559, 421);
            this.gvSingleMatchViewPanel2.TabIndex = 1;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.richTextBox1);
            this.tabPage3.ImageIndex = 3;
            this.tabPage3.Location = new System.Drawing.Point(4, 40);
            this.tabPage3.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(559, 421);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Log View";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // richTextBox1
            // 
            this.richTextBox1.BackColor = System.Drawing.Color.WhiteSmoke;
            this.richTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBox1.Location = new System.Drawing.Point(0, 0);
            this.richTextBox1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(559, 421);
            this.richTextBox1.TabIndex = 0;
            this.richTextBox1.Text = "";
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "Machine");
            this.imageList1.Images.SetKeyName(1, "Vision");
            this.imageList1.Images.SetKeyName(2, "Camera");
            this.imageList1.Images.SetKeyName(3, "notes.png");
            // 
            // GvDualMatchTabView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.Controls.Add(this.tabControl1);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "GvDualMatchTabView";
            this.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.Size = new System.Drawing.Size(567, 467);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.ResumeLayout(false);

        }


        #endregion
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        public System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage3;
        public System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.ImageList imageList1;
        private EzDualMatch.Gui.Panels.GvSingleMatchViewPanel gvSingleMatchViewPanel1;
        private GvSingleMatchViewPanel gvSingleMatchViewPanel2;
        //public EzPizza.App.Gui.GPizzaMachineView gPizzaMachineViewer1;
    }
}
