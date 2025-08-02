namespace LaserAlignDX.UISpace.UIMVC
{
    partial class MVSUI
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.mvdRenderActivex1 = new VisionDesigner.MVDRenderActivex();
            this.SuspendLayout();
            // 
            // mvdRenderActivex1
            // 
            this.mvdRenderActivex1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.mvdRenderActivex1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mvdRenderActivex1.EnableRetainShape = false;
            this.mvdRenderActivex1.InteractType = VisionDesigner.MVDRenderInteractType.Standard;
            this.mvdRenderActivex1.Location = new System.Drawing.Point(0, 0);
            this.mvdRenderActivex1.MenuLanguage = VisionDesigner.MVDRenderMenuLangType.Default;
            this.mvdRenderActivex1.Name = "mvdRenderActivex1";
            this.mvdRenderActivex1.Size = new System.Drawing.Size(373, 364);
            this.mvdRenderActivex1.TabIndex = 10;
            // 
            // MVSUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.mvdRenderActivex1);
            this.Name = "MVSUI";
            this.Size = new System.Drawing.Size(373, 364);
            this.ResumeLayout(false);

        }

        #endregion

        public VisionDesigner.MVDRenderActivex mvdRenderActivex1;
    }
}
