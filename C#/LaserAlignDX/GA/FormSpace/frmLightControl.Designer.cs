namespace LaserAlignDX.FormSpace
{
    partial class frmLightControl
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
            this.label1 = new System.Windows.Forms.Label();
            this.num0 = new System.Windows.Forms.NumericUpDown();
            this.btn0Open = new System.Windows.Forms.Button();
            this.btn0Close = new System.Windows.Forms.Button();
            this.btn1Close = new System.Windows.Forms.Button();
            this.btn1Open = new System.Windows.Forms.Button();
            this.num1 = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.num0)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.num1)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(36, 79);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(206, 21);
            this.label1.TabIndex = 0;
            this.label1.Text = "通道1-红光(0~255)";
            // 
            // num0
            // 
            this.num0.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.num0.Location = new System.Drawing.Point(260, 76);
            this.num0.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.num0.Name = "num0";
            this.num0.Size = new System.Drawing.Size(104, 31);
            this.num0.TabIndex = 1;
            // 
            // btn0Open
            // 
            this.btn0Open.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btn0Open.Location = new System.Drawing.Point(385, 66);
            this.btn0Open.Name = "btn0Open";
            this.btn0Open.Size = new System.Drawing.Size(114, 47);
            this.btn0Open.TabIndex = 2;
            this.btn0Open.Text = "设置";
            this.btn0Open.UseVisualStyleBackColor = true;
            this.btn0Open.Click += new System.EventHandler(this.btn0Open_Click);
            // 
            // btn0Close
            // 
            this.btn0Close.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btn0Close.Location = new System.Drawing.Point(505, 65);
            this.btn0Close.Name = "btn0Close";
            this.btn0Close.Size = new System.Drawing.Size(114, 47);
            this.btn0Close.TabIndex = 3;
            this.btn0Close.Text = "关闭";
            this.btn0Close.UseVisualStyleBackColor = true;
            this.btn0Close.Visible = false;
            this.btn0Close.Click += new System.EventHandler(this.btn0Close_Click);
            // 
            // btn1Close
            // 
            this.btn1Close.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btn1Close.Location = new System.Drawing.Point(505, 118);
            this.btn1Close.Name = "btn1Close";
            this.btn1Close.Size = new System.Drawing.Size(114, 47);
            this.btn1Close.TabIndex = 7;
            this.btn1Close.Text = "关闭";
            this.btn1Close.UseVisualStyleBackColor = true;
            this.btn1Close.Visible = false;
            this.btn1Close.Click += new System.EventHandler(this.btn1Close_Click);
            // 
            // btn1Open
            // 
            this.btn1Open.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btn1Open.Location = new System.Drawing.Point(385, 119);
            this.btn1Open.Name = "btn1Open";
            this.btn1Open.Size = new System.Drawing.Size(114, 47);
            this.btn1Open.TabIndex = 6;
            this.btn1Open.Text = "设置";
            this.btn1Open.UseVisualStyleBackColor = true;
            this.btn1Open.Click += new System.EventHandler(this.btn1Open_Click);
            // 
            // num1
            // 
            this.num1.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.num1.Location = new System.Drawing.Point(260, 129);
            this.num1.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.num1.Name = "num1";
            this.num1.Size = new System.Drawing.Size(104, 31);
            this.num1.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(36, 132);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(206, 21);
            this.label2.TabIndex = 4;
            this.label2.Text = "通道2-白光(0~255)";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.Location = new System.Drawing.Point(36, 229);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(432, 21);
            this.label3.TabIndex = 8;
            this.label3.Text = "注:关闭灯光设置0 打开灯光设置数值 即可";
            // 
            // frmLightControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btn1Close);
            this.Controls.Add(this.btn1Open);
            this.Controls.Add(this.num1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btn0Close);
            this.Controls.Add(this.btn0Open);
            this.Controls.Add(this.num0);
            this.Controls.Add(this.label1);
            this.Name = "frmLightControl";
            this.Text = "frmLightControl";
            ((System.ComponentModel.ISupportInitialize)(this.num0)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.num1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown num0;
        private System.Windows.Forms.Button btn0Open;
        private System.Windows.Forms.Button btn0Close;
        private System.Windows.Forms.Button btn1Close;
        private System.Windows.Forms.Button btn1Open;
        private System.Windows.Forms.NumericUpDown num1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
    }
}