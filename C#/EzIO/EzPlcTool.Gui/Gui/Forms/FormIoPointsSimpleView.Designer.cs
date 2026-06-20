namespace TestDemo_EzPlc_Fatek.Gui
{
    partial class FormIoPointsSimpleView
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormIoPointsSimpleView));
            this.gvIoPointsSimpleView1 = new EzIO.Gui.GvIoPointsSimpleView();
            this.SuspendLayout();
            // 
            // gvIoPointsSimpleView1
            // 
            this.gvIoPointsSimpleView1.BackColor = System.Drawing.Color.Silver;
            this.gvIoPointsSimpleView1.CellCols = 3;
            this.gvIoPointsSimpleView1.CellRows = 1;
            this.gvIoPointsSimpleView1.CellsActiveBackColor = System.Drawing.Color.Lime;
            this.gvIoPointsSimpleView1.CellsActiveForeColor = System.Drawing.Color.Black;
            this.gvIoPointsSimpleView1.CellsBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.gvIoPointsSimpleView1.CellsForeColor = System.Drawing.Color.White;
            this.gvIoPointsSimpleView1.CellsInvertedBackColor = System.Drawing.Color.Red;
            this.gvIoPointsSimpleView1.CellsInvertedForeColor = System.Drawing.Color.White;
            this.gvIoPointsSimpleView1.CellsMargin = new System.Windows.Forms.Padding(3);
            this.gvIoPointsSimpleView1.CellWidth = 0;
            this.gvIoPointsSimpleView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gvIoPointsSimpleView1.Location = new System.Drawing.Point(0, 0);
            this.gvIoPointsSimpleView1.Name = "gvIoPointsSimpleView1";
            this.gvIoPointsSimpleView1.Padding = new System.Windows.Forms.Padding(2);
            this.gvIoPointsSimpleView1.Size = new System.Drawing.Size(486, 389);
            this.gvIoPointsSimpleView1.TabIndex = 0;
            // 
            // FormIoPointsSimpleView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(486, 389);
            this.Controls.Add(this.gvIoPointsSimpleView1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormIoPointsSimpleView";
            this.Text = "Io Points";
            this.ResumeLayout(false);

        }

        #endregion

        private EzIO.Gui.GvIoPointsSimpleView gvIoPointsSimpleView1;
    }
}