namespace JetEazy.FormSpace
{
    partial class AccountForm
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

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改這個方法的內容。
        ///
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.chkUseFactorySettings = new System.Windows.Forms.CheckBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.chkAllowRecipeEditting = new System.Windows.Forms.CheckBox();
            this.chkAllowAccountManagement = new System.Windows.Forms.CheckBox();
            this.chkAllowSystemSetup = new System.Windows.Forms.CheckBox();
            this.lblPasswordTag = new System.Windows.Forms.Label();
            this.lblAccountNameTag = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.btnExit = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnDelAccount = new System.Windows.Forms.Button();
            this.btnModify = new System.Windows.Forms.Button();
            this.btnAddAccount = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.chkUseFactorySettings);
            this.groupBox1.Controls.Add(this.comboBox1);
            this.groupBox1.Controls.Add(this.chkAllowRecipeEditting);
            this.groupBox1.Controls.Add(this.chkAllowAccountManagement);
            this.groupBox1.Controls.Add(this.chkAllowSystemSetup);
            this.groupBox1.Controls.Add(this.lblPasswordTag);
            this.groupBox1.Controls.Add(this.lblAccountNameTag);
            this.groupBox1.Controls.Add(this.textBox2);
            this.groupBox1.Controls.Add(this.textBox1);
            this.groupBox1.Location = new System.Drawing.Point(7, -3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(404, 209);
            this.groupBox1.TabIndex = 85;
            this.groupBox1.TabStop = false;
            // 
            // chkUseFactorySettings
            // 
            this.chkUseFactorySettings.AutoSize = true;
            this.chkUseFactorySettings.Location = new System.Drawing.Point(103, 163);
            this.chkUseFactorySettings.Name = "chkUseFactorySettings";
            this.chkUseFactorySettings.Size = new System.Drawing.Size(120, 16);
            this.chkUseFactorySettings.TabIndex = 92;
            this.chkUseFactorySettings.Text = "指定使用工廠系統";
            this.chkUseFactorySettings.UseVisualStyleBackColor = true;
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.Font = new System.Drawing.Font("新細明體", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(153, 21);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(169, 21);
            this.comboBox1.TabIndex = 89;
            // 
            // chkAllowRecipeEditting
            // 
            this.chkAllowRecipeEditting.AutoSize = true;
            this.chkAllowRecipeEditting.Location = new System.Drawing.Point(103, 141);
            this.chkAllowRecipeEditting.Name = "chkAllowRecipeEditting";
            this.chkAllowRecipeEditting.Size = new System.Drawing.Size(96, 16);
            this.chkAllowRecipeEditting.TabIndex = 91;
            this.chkAllowRecipeEditting.Text = "准許更改參數";
            this.chkAllowRecipeEditting.UseVisualStyleBackColor = true;
            // 
            // chkAllowAccountManagement
            // 
            this.chkAllowAccountManagement.AutoSize = true;
            this.chkAllowAccountManagement.Location = new System.Drawing.Point(103, 119);
            this.chkAllowAccountManagement.Name = "chkAllowAccountManagement";
            this.chkAllowAccountManagement.Size = new System.Drawing.Size(96, 16);
            this.chkAllowAccountManagement.TabIndex = 90;
            this.chkAllowAccountManagement.Text = "准許帳號管理";
            this.chkAllowAccountManagement.UseVisualStyleBackColor = true;
            // 
            // chkAllowSystemSetup
            // 
            this.chkAllowSystemSetup.AutoSize = true;
            this.chkAllowSystemSetup.Location = new System.Drawing.Point(103, 97);
            this.chkAllowSystemSetup.Name = "chkAllowSystemSetup";
            this.chkAllowSystemSetup.Size = new System.Drawing.Size(72, 16);
            this.chkAllowSystemSetup.TabIndex = 89;
            this.chkAllowSystemSetup.Text = "淮許設定";
            this.chkAllowSystemSetup.UseVisualStyleBackColor = true;
            // 
            // lblPasswordTag
            // 
            this.lblPasswordTag.Location = new System.Drawing.Point(57, 54);
            this.lblPasswordTag.Name = "lblPasswordTag";
            this.lblPasswordTag.Size = new System.Drawing.Size(74, 11);
            this.lblPasswordTag.TabIndex = 88;
            this.lblPasswordTag.Text = "密碼";
            // 
            // lblAccountNameTag
            // 
            this.lblAccountNameTag.Location = new System.Drawing.Point(57, 26);
            this.lblAccountNameTag.Name = "lblAccountNameTag";
            this.lblAccountNameTag.Size = new System.Drawing.Size(74, 11);
            this.lblAccountNameTag.TabIndex = 87;
            this.lblAccountNameTag.Text = "帳號";
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(153, 48);
            this.textBox2.Name = "textBox2";
            this.textBox2.PasswordChar = '*';
            this.textBox2.Size = new System.Drawing.Size(169, 22);
            this.textBox2.TabIndex = 86;
            this.textBox2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(153, 20);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(169, 22);
            this.textBox1.TabIndex = 85;
            this.textBox1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // btnExit
            // 
            this.btnExit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.btnExit.Location = new System.Drawing.Point(335, 212);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(76, 45);
            this.btnExit.TabIndex = 98;
            this.btnExit.Text = "離開";
            this.btnExit.UseVisualStyleBackColor = false;
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.btnCancel.Location = new System.Drawing.Point(335, 212);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(76, 45);
            this.btnCancel.TabIndex = 97;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = false;
            // 
            // btnOK
            // 
            this.btnOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnOK.Location = new System.Drawing.Point(253, 212);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(76, 45);
            this.btnOK.TabIndex = 96;
            this.btnOK.Text = "確定";
            this.btnOK.UseVisualStyleBackColor = false;
            // 
            // btnDelAccount
            // 
            this.btnDelAccount.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnDelAccount.Location = new System.Drawing.Point(171, 212);
            this.btnDelAccount.Name = "btnDelAccount";
            this.btnDelAccount.Size = new System.Drawing.Size(76, 45);
            this.btnDelAccount.TabIndex = 95;
            this.btnDelAccount.Text = "刪除";
            this.btnDelAccount.UseVisualStyleBackColor = false;
            // 
            // btnModify
            // 
            this.btnModify.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnModify.Location = new System.Drawing.Point(89, 212);
            this.btnModify.Name = "btnModify";
            this.btnModify.Size = new System.Drawing.Size(76, 45);
            this.btnModify.TabIndex = 94;
            this.btnModify.Text = "修改";
            this.btnModify.UseVisualStyleBackColor = false;
            // 
            // btnAddAccount
            // 
            this.btnAddAccount.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnAddAccount.Location = new System.Drawing.Point(7, 212);
            this.btnAddAccount.Name = "btnAddAccount";
            this.btnAddAccount.Size = new System.Drawing.Size(76, 45);
            this.btnAddAccount.TabIndex = 93;
            this.btnAddAccount.Text = "新增";
            this.btnAddAccount.UseVisualStyleBackColor = false;
            // 
            // AccountForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.LightSteelBlue;
            this.ClientSize = new System.Drawing.Size(417, 263);
            this.ControlBox = false;
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnAddAccount);
            this.Controls.Add(this.btnDelAccount);
            this.Controls.Add(this.btnModify);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.btnCancel);
            this.Font = new System.Drawing.Font("新細明體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "AccountForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "帳號設定";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Label lblPasswordTag;
        private System.Windows.Forms.Label lblAccountNameTag;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.CheckBox chkUseFactorySettings;
        private System.Windows.Forms.CheckBox chkAllowRecipeEditting;
        private System.Windows.Forms.CheckBox chkAllowAccountManagement;
        private System.Windows.Forms.CheckBox chkAllowSystemSetup;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnDelAccount;
        private System.Windows.Forms.Button btnModify;
        private System.Windows.Forms.Button btnAddAccount;
    }
}