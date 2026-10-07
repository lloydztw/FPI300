namespace EzAoiEmptyTrayInspector.Gui
{
    partial class FormImageViewer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormImageViewer));
            this.jezImageViewPanel1 = new EzAoiEmptyTrayInspector.Gui.Panels.JezImageViewPanel();
            this.SuspendLayout();
            // 
            // jezImageViewPanel1
            // 
            this.jezImageViewPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.jezImageViewPanel1.Location = new System.Drawing.Point(0, 0);
            this.jezImageViewPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.jezImageViewPanel1.Name = "jezImageViewPanel1";
            this.jezImageViewPanel1.Size = new System.Drawing.Size(782, 552);
            this.jezImageViewPanel1.TabIndex = 0;
            // 
            // FormImageViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DimGray;
            this.ClientSize = new System.Drawing.Size(782, 552);
            this.Controls.Add(this.jezImageViewPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormImageViewer";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Image Viewer";
            this.ResumeLayout(false);

        }

        #endregion

        private Panels.JezImageViewPanel jezImageViewPanel1;
    }
}