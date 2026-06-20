namespace EzIO.Gui
{
    partial class FormIoPointsDataGridView
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

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改
        /// 這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormIoPointsDataGridView));
            this.gvIoPointDataGridView1 = new EzIO.Gui.GvIoPointsDataGridView();
            this.SuspendLayout();
            // 
            // gvIoPointDataGridView1
            // 
            this.gvIoPointDataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gvIoPointDataGridView1.Location = new System.Drawing.Point(0, 0);
            this.gvIoPointDataGridView1.Name = "gvIoPointDataGridView1";
            this.gvIoPointDataGridView1.SelectedIndex = 0;
            this.gvIoPointDataGridView1.Size = new System.Drawing.Size(482, 389);
            this.gvIoPointDataGridView1.TabIndex = 0;
            // 
            // FormIoPointsDataGridView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(482, 389);
            this.Controls.Add(this.gvIoPointDataGridView1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormIoPointsDataGridView";
            this.Text = "Io Points";
            this.ResumeLayout(false);

        }

        #endregion

        private GvIoPointsDataGridView gvIoPointDataGridView1;
    }
}

