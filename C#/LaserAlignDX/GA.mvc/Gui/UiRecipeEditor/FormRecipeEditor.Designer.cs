namespace LaserAlignDX.Mvc.Gui
{
    partial class FormRecipeEditor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormRecipeEditor));
            this.panelBottom = new System.Windows.Forms.Panel();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            this.tblayoutRight = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.tblayoutMajor = new System.Windows.Forms.TableLayoutPanel();
            this.panelImageViews = new System.Windows.Forms.Panel();
            this.jezTransImageViewPanel1 = new LaserAlignDX.Mvc.Gui.JezTransImageViewPanel();
            this.panelTop = new System.Windows.Forms.Panel();
            this.gvRecipeBtnsPanel1 = new LaserAlignDX.Mvc.Gui.GvRecipeBtnsPanel();
            this.panelBottom.SuspendLayout();
            this.tblayoutRight.SuspendLayout();
            this.tblayoutMajor.SuspendLayout();
            this.panelImageViews.SuspendLayout();
            this.panelTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.btnOK);
            this.panelBottom.Controls.Add(this.btnCancel);
            this.panelBottom.Location = new System.Drawing.Point(4, 705);
            this.panelBottom.Margin = new System.Windows.Forms.Padding(4);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(315, 86);
            this.panelBottom.TabIndex = 7;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnOK.Location = new System.Drawing.Point(14, 11);
            this.btnOK.Margin = new System.Windows.Forms.Padding(4);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(140, 62);
            this.btnOK.TabIndex = 15;
            this.btnOK.Text = "確定";
            this.btnOK.UseVisualStyleBackColor = false;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnCancel.Location = new System.Drawing.Point(162, 11);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(140, 62);
            this.btnCancel.TabIndex = 16;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = false;
            // 
            // propertyGrid1
            // 
            this.propertyGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid1.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.propertyGrid1.Location = new System.Drawing.Point(4, 42);
            this.propertyGrid1.Margin = new System.Windows.Forms.Padding(4);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.Size = new System.Drawing.Size(315, 655);
            this.propertyGrid1.TabIndex = 0;
            // 
            // tblayoutRight
            // 
            this.tblayoutRight.BackColor = System.Drawing.Color.LightSteelBlue;
            this.tblayoutRight.ColumnCount = 1;
            this.tblayoutRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblayoutRight.Controls.Add(this.panelBottom, 0, 2);
            this.tblayoutRight.Controls.Add(this.propertyGrid1, 0, 1);
            this.tblayoutRight.Controls.Add(this.label1, 0, 0);
            this.tblayoutRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblayoutRight.Location = new System.Drawing.Point(936, 3);
            this.tblayoutRight.Name = "tblayoutRight";
            this.tblayoutRight.RowCount = 3;
            this.tblayoutRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.tblayoutRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblayoutRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tblayoutRight.Size = new System.Drawing.Size(323, 795);
            this.tblayoutRight.TabIndex = 9;
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Font = new System.Drawing.Font("微软雅黑", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.label1.Size = new System.Drawing.Size(317, 38);
            this.label1.TabIndex = 8;
            this.label1.Text = "參數設定";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tblayoutMajor
            // 
            this.tblayoutMajor.ColumnCount = 2;
            this.tblayoutMajor.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblayoutMajor.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tblayoutMajor.Controls.Add(this.panelImageViews, 0, 0);
            this.tblayoutMajor.Controls.Add(this.tblayoutRight, 1, 0);
            this.tblayoutMajor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblayoutMajor.Location = new System.Drawing.Point(0, 176);
            this.tblayoutMajor.Name = "tblayoutMajor";
            this.tblayoutMajor.RowCount = 1;
            this.tblayoutMajor.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblayoutMajor.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 801F));
            this.tblayoutMajor.Size = new System.Drawing.Size(1262, 801);
            this.tblayoutMajor.TabIndex = 10;
            // 
            // panelImageViews
            // 
            this.panelImageViews.BackColor = System.Drawing.Color.Gray;
            this.panelImageViews.Controls.Add(this.jezTransImageViewPanel1);
            this.panelImageViews.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelImageViews.Location = new System.Drawing.Point(0, 0);
            this.panelImageViews.Margin = new System.Windows.Forms.Padding(0);
            this.panelImageViews.Name = "panelImageViews";
            this.panelImageViews.Size = new System.Drawing.Size(933, 801);
            this.panelImageViews.TabIndex = 11;
            // 
            // jezTransImageViewPanel1
            // 
            this.jezTransImageViewPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.jezTransImageViewPanel1.Location = new System.Drawing.Point(0, 0);
            this.jezTransImageViewPanel1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.jezTransImageViewPanel1.Name = "jezTransImageViewPanel1";
            this.jezTransImageViewPanel1.Size = new System.Drawing.Size(933, 801);
            this.jezTransImageViewPanel1.TabIndex = 0;
            // 
            // panelTop
            // 
            this.panelTop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.panelTop.Controls.Add(this.gvRecipeBtnsPanel1);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Margin = new System.Windows.Forms.Padding(4);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(1262, 176);
            this.panelTop.TabIndex = 6;
            // 
            // gvRecipeBtnsPanel1
            // 
            this.gvRecipeBtnsPanel1.ActiveButtonIndex = 0;
            this.gvRecipeBtnsPanel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.gvRecipeBtnsPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gvRecipeBtnsPanel1.Location = new System.Drawing.Point(0, 0);
            this.gvRecipeBtnsPanel1.Name = "gvRecipeBtnsPanel1";
            this.gvRecipeBtnsPanel1.Size = new System.Drawing.Size(1262, 176);
            this.gvRecipeBtnsPanel1.TabIndex = 0;
            // 
            // FormRecipeEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1262, 977);
            this.Controls.Add(this.tblayoutMajor);
            this.Controls.Add(this.panelTop);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "FormRecipeEditor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "參數設定";
            this.panelBottom.ResumeLayout(false);
            this.tblayoutRight.ResumeLayout(false);
            this.tblayoutMajor.ResumeLayout(false);
            this.panelImageViews.ResumeLayout(false);
            this.panelTop.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.PropertyGrid propertyGrid1;
        private JezTransImageViewPanel jezTransImageViewPanel1;
        private System.Windows.Forms.TableLayoutPanel tblayoutRight;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TableLayoutPanel tblayoutMajor;
        private System.Windows.Forms.Panel panelTop;
        private GvRecipeBtnsPanel gvRecipeBtnsPanel1;
        private System.Windows.Forms.Panel panelImageViews;
    }
}