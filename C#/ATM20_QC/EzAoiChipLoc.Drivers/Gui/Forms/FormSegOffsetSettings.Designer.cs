namespace EzAoiChipLocQC.Gui
{
    partial class FormSegOffsetSettings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormSegOffsetSettings));
            this.numSegsNumber = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.gwSegOffsetDataGridView1 = new EzAoiChipLocQC.Gui.Panels.GwSegOffsetDataGridView();
            this.panelB = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numSegsNumber)).BeginInit();
            this.panelB.SuspendLayout();
            this.SuspendLayout();
            // 
            // numSegsNumber
            // 
            this.numSegsNumber.Font = new System.Drawing.Font("微軟正黑體", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.numSegsNumber.Location = new System.Drawing.Point(141, 17);
            this.numSegsNumber.Maximum = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.numSegsNumber.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numSegsNumber.Name = "numSegsNumber";
            this.numSegsNumber.Size = new System.Drawing.Size(120, 30);
            this.numSegsNumber.TabIndex = 0;
            this.numSegsNumber.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numSegsNumber.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("微軟正黑體", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label1.Location = new System.Drawing.Point(43, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(78, 22);
            this.label1.TabIndex = 1;
            this.label1.Text = "群組數量";
            // 
            // gwSegOffsetDataGridView1
            // 
            this.gwSegOffsetDataGridView1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gwSegOffsetDataGridView1.Location = new System.Drawing.Point(5, 66);
            this.gwSegOffsetDataGridView1.Name = "gwSegOffsetDataGridView1";
            this.gwSegOffsetDataGridView1.SelectedIndex = 0;
            this.gwSegOffsetDataGridView1.Size = new System.Drawing.Size(300, 158);
            this.gwSegOffsetDataGridView1.TabIndex = 2;
            // 
            // panelB
            // 
            this.panelB.BackColor = System.Drawing.Color.LightSteelBlue;
            this.panelB.Controls.Add(this.btnCancel);
            this.panelB.Controls.Add(this.btnOK);
            this.panelB.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelB.Location = new System.Drawing.Point(5, 224);
            this.panelB.Margin = new System.Windows.Forms.Padding(0);
            this.panelB.Name = "panelB";
            this.panelB.Size = new System.Drawing.Size(300, 68);
            this.panelB.TabIndex = 33;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnCancel.Font = new System.Drawing.Font("微软雅黑", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel.Location = new System.Drawing.Point(152, 14);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(118, 38);
            this.btnCancel.TabIndex = 24;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = false;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnOK.Font = new System.Drawing.Font("微软雅黑", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOK.Location = new System.Drawing.Point(26, 14);
            this.btnOK.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(118, 38);
            this.btnOK.TabIndex = 23;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = false;
            // 
            // FormSegOffsetSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.LightSteelBlue;
            this.ClientSize = new System.Drawing.Size(310, 297);
            this.Controls.Add(this.gwSegOffsetDataGridView1);
            this.Controls.Add(this.panelB);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.numSegsNumber);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormSegOffsetSettings";
            this.Padding = new System.Windows.Forms.Padding(5, 0, 5, 5);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "群組設定";
            ((System.ComponentModel.ISupportInitialize)(this.numSegsNumber)).EndInit();
            this.panelB.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown numSegsNumber;
        private System.Windows.Forms.Label label1;
        private Panels.GwSegOffsetDataGridView gwSegOffsetDataGridView1;
        private System.Windows.Forms.Panel panelB;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
    }
}