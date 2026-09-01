namespace LaserAlignDX.GA.mvc.Gui.UiRecipeEditor
{
    partial class GwRcpDefectBtnsPanel
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
            this.btnDefectDelete = new System.Windows.Forms.Button();
            this.btnDefectClear = new System.Windows.Forms.Button();
            this.btnDefectAdd = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnDefectDelete
            // 
            this.btnDefectDelete.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.btnDefectDelete.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnDefectDelete.Font = new System.Drawing.Font("Microsoft YaHei UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDefectDelete.Location = new System.Drawing.Point(268, 47);
            this.btnDefectDelete.Margin = new System.Windows.Forms.Padding(4);
            this.btnDefectDelete.Name = "btnDefectDelete";
            this.btnDefectDelete.Size = new System.Drawing.Size(212, 54);
            this.btnDefectDelete.TabIndex = 75;
            this.btnDefectDelete.Text = "缺陷區域 刪除";
            this.btnDefectDelete.UseVisualStyleBackColor = false;
            // 
            // btnDefectClear
            // 
            this.btnDefectClear.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.btnDefectClear.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnDefectClear.Font = new System.Drawing.Font("Microsoft YaHei UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDefectClear.Location = new System.Drawing.Point(497, 47);
            this.btnDefectClear.Margin = new System.Windows.Forms.Padding(4);
            this.btnDefectClear.Name = "btnDefectClear";
            this.btnDefectClear.Size = new System.Drawing.Size(212, 54);
            this.btnDefectClear.TabIndex = 74;
            this.btnDefectClear.Text = "缺陷區域 清空";
            this.btnDefectClear.UseVisualStyleBackColor = false;
            // 
            // btnDefectAdd
            // 
            this.btnDefectAdd.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnDefectAdd.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnDefectAdd.Font = new System.Drawing.Font("Microsoft YaHei UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDefectAdd.Location = new System.Drawing.Point(39, 47);
            this.btnDefectAdd.Margin = new System.Windows.Forms.Padding(4);
            this.btnDefectAdd.Name = "btnDefectAdd";
            this.btnDefectAdd.Size = new System.Drawing.Size(212, 54);
            this.btnDefectAdd.TabIndex = 73;
            this.btnDefectAdd.Text = "缺陷區域 添加";
            this.btnDefectAdd.UseVisualStyleBackColor = false;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnDefectDelete);
            this.groupBox1.Controls.Add(this.btnDefectClear);
            this.groupBox1.Controls.Add(this.btnDefectAdd);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(8, 0);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(8, 0, 8, 8);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.groupBox1.Size = new System.Drawing.Size(757, 128);
            this.groupBox1.TabIndex = 78;
            this.groupBox1.TabStop = false;
            // 
            // GwRcpDefectBtnsPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.Name = "GwRcpDefectBtnsPanel";
            this.Padding = new System.Windows.Forms.Padding(8, 0, 8, 2);
            this.Size = new System.Drawing.Size(773, 130);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.Button btnDefectDelete;
        public System.Windows.Forms.Button btnDefectClear;
        public System.Windows.Forms.Button btnDefectAdd;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}
