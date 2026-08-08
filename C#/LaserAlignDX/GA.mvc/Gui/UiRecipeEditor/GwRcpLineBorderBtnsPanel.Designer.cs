namespace LaserAlignDX.GA.mvc.Gui.UiRecipeEditor
{
    partial class GwRcpLineBorderBtnsPanel
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnBuildMircoTrf = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.numSpanRatio = new System.Windows.Forms.NumericUpDown();
            this.labelA = new System.Windows.Forms.Label();
            this.numBorderIndent = new System.Windows.Forms.NumericUpDown();
            this.labelB = new System.Windows.Forms.Label();
            this.numBorderSize = new System.Windows.Forms.NumericUpDown();
            this.btnAutoLineBorders = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numSpanRatio)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBorderIndent)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBorderSize)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnBuildMircoTrf);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.numSpanRatio);
            this.groupBox1.Controls.Add(this.labelA);
            this.groupBox1.Controls.Add(this.numBorderIndent);
            this.groupBox1.Controls.Add(this.labelB);
            this.groupBox1.Controls.Add(this.numBorderSize);
            this.groupBox1.Controls.Add(this.btnAutoLineBorders);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(8, 2);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(8);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(487, 136);
            this.groupBox1.TabIndex = 76;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Line Borders";
            // 
            // btnBuildMircoTrf
            // 
            this.btnBuildMircoTrf.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnBuildMircoTrf.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.btnBuildMircoTrf.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnBuildMircoTrf.Location = new System.Drawing.Point(336, 77);
            this.btnBuildMircoTrf.Margin = new System.Windows.Forms.Padding(4);
            this.btnBuildMircoTrf.Name = "btnBuildMircoTrf";
            this.btnBuildMircoTrf.Size = new System.Drawing.Size(116, 46);
            this.btnBuildMircoTrf.TabIndex = 74;
            this.btnBuildMircoTrf.Text = "精算尺寸";
            this.btnBuildMircoTrf.UseVisualStyleBackColor = false;
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(50, 97);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(96, 20);
            this.label2.TabIndex = 73;
            this.label2.Text = "跨度比例 (%)";
            // 
            // numSpanRatio
            // 
            this.numSpanRatio.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.numSpanRatio.DecimalPlaces = 1;
            this.numSpanRatio.Location = new System.Drawing.Point(168, 94);
            this.numSpanRatio.Minimum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numSpanRatio.Name = "numSpanRatio";
            this.numSpanRatio.Size = new System.Drawing.Size(123, 27);
            this.numSpanRatio.TabIndex = 72;
            this.numSpanRatio.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numSpanRatio.Value = new decimal(new int[] {
            90,
            0,
            0,
            0});
            // 
            // labelA
            // 
            this.labelA.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelA.AutoSize = true;
            this.labelA.Location = new System.Drawing.Point(50, 31);
            this.labelA.Name = "labelA";
            this.labelA.Size = new System.Drawing.Size(95, 20);
            this.labelA.TabIndex = 71;
            this.labelA.Text = "內緣 (pixels)";
            // 
            // numBorderIndent
            // 
            this.numBorderIndent.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.numBorderIndent.Location = new System.Drawing.Point(168, 28);
            this.numBorderIndent.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numBorderIndent.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numBorderIndent.Name = "numBorderIndent";
            this.numBorderIndent.Size = new System.Drawing.Size(123, 27);
            this.numBorderIndent.TabIndex = 70;
            this.numBorderIndent.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numBorderIndent.Value = new decimal(new int[] {
            18,
            0,
            0,
            0});
            // 
            // labelB
            // 
            this.labelB.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelB.AutoSize = true;
            this.labelB.Location = new System.Drawing.Point(50, 64);
            this.labelB.Name = "labelB";
            this.labelB.Size = new System.Drawing.Size(95, 20);
            this.labelB.TabIndex = 69;
            this.labelB.Text = "外緣 (pixels)";
            // 
            // numBorderSize
            // 
            this.numBorderSize.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.numBorderSize.Location = new System.Drawing.Point(168, 61);
            this.numBorderSize.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numBorderSize.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numBorderSize.Name = "numBorderSize";
            this.numBorderSize.Size = new System.Drawing.Size(123, 27);
            this.numBorderSize.TabIndex = 68;
            this.numBorderSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numBorderSize.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // btnAutoLineBorders
            // 
            this.btnAutoLineBorders.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnAutoLineBorders.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnAutoLineBorders.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnAutoLineBorders.Location = new System.Drawing.Point(336, 25);
            this.btnAutoLineBorders.Margin = new System.Windows.Forms.Padding(4);
            this.btnAutoLineBorders.Name = "btnAutoLineBorders";
            this.btnAutoLineBorders.Size = new System.Drawing.Size(116, 46);
            this.btnAutoLineBorders.TabIndex = 49;
            this.btnAutoLineBorders.Text = "一鍵框選";
            this.btnAutoLineBorders.UseVisualStyleBackColor = false;
            // 
            // GwRcpLineBorderBtnsPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Name = "GwRcpLineBorderBtnsPanel";
            this.Padding = new System.Windows.Forms.Padding(8, 2, 12, 2);
            this.Size = new System.Drawing.Size(507, 140);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numSpanRatio)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBorderIndent)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBorderSize)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label labelA;
        private System.Windows.Forms.Label labelB;
        public System.Windows.Forms.Button btnBuildMircoTrf;
        public System.Windows.Forms.NumericUpDown numSpanRatio;
        public System.Windows.Forms.NumericUpDown numBorderIndent;
        public System.Windows.Forms.NumericUpDown numBorderSize;
        public System.Windows.Forms.Button btnAutoLineBorders;
    }
}
