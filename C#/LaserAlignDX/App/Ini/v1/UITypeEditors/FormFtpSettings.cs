#region AUTHOR
/*
 * 
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2026-09-10 重整 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.Lang;
using JetEazy.Utils;
using LeTian.JxProps;
using System;
using System.Security.Principal;
using System.Windows.Forms;
using Traveller106.Ini.V1;

namespace LaserAlignDX.Mvc.Gui
{
    public partial class FormFtpSettings : Form
    {
        public FormFtpSettings()
        {
            InitializeComponent();

            if (DesignMode)
                return;

            DialogResult = DialogResult.Cancel;

            Load += (s, e) => PostInit();
            btnOK.Click += (s, e) => GoConfirm();
            btnCancel.Click += (s, e) => GoCancel();
            btnTestConnection.Click += (s, e) => TestConnection();
        }

        public DtoFtpSettings Settings
        {
            get;
            set;
        }

        void PostInit()
        {
            QMSG.Translate(this);
            UpdateSettings(false);
        }
        void GoConfirm()
        {
            UpdateSettings(true);
            this.DialogResult = DialogResult.OK;
            Close();
        }
        void GoCancel()
        {
            this.DialogResult = DialogResult.Cancel;
            Close();
        }
        async void TestConnection()
        {
            // 1. 建立臨時的 DtoFtpSettings 並帶入目前 UI 上的輸入值
            DtoFtpSettings tempSettings = new DtoFtpSettings
            {
                Account = txtAccount.Text.Trim(),
                Password = txtPassword.Text.Trim(),
                IpAddress = txtIpAddress.Text.Trim(),
                DstFolder = txtDstFolder.Text.Trim(),
                Enabled = true // 測試連線時強制設為 true，避免 Enabled 未勾選時無法測試
            };

            // 2. 鎖定按鈕與設定等待游標，防止使用者重複點擊
            btnTestConnection.Enabled = false;
            btnCancel.Enabled = false;
            btnOK.Enabled = false;
            var oldCursor = GaUtil.SetCursor(this, Cursors.WaitCursor);

            try
            {
                // 3. 實例化 FtpUploader 並執行非同步測試 (逾時設為 5000ms)
                FtpUploader uploader = new FtpUploader(tempSettings);
                bool isConnected = await uploader.TestConnectionAsync(5000);

                // 4. 根據結果提示使用者
                if (isConnected)
                {
                    QMessageBox.Info("FTP Connection OK!", "FTP Connection", translate: false);
                }
                else
                {
                    QMessageBox.Error("FTP Connection Failed!", "FTP Connection", translate: false);
                }
            }
            catch (Exception ex)
            {
                QMessageBox.Error($"Error：\n{ex.Message}", "FTP Connection", translate: false);
            }
            finally
            {
                // 5. 恢復 UI 按鈕狀態與游標
                btnTestConnection.Enabled = true;
                btnCancel.Enabled = true;
                btnOK.Enabled = true;
                GaUtil.SetCursor(this, oldCursor);
            }
        }

        void UpdateSettings(bool toModel)
        {
            if (Settings == null)
                return;

            if (toModel)
            {
                Settings.Account = txtAccount.Text.Trim();
                Settings.Password = txtPassword.Text.Trim();
                Settings.IpAddress = txtIpAddress.Text.Trim();
                Settings.DstFolder = txtDstFolder.Text.Trim();
                Settings.Enabled = chkFtpUploadEnabled.Checked;
            }
            else
            {
                txtAccount.Text = Settings.Account;
                txtPassword.Text = Settings.Password;
                txtIpAddress.Text = Settings.IpAddress;
                txtDstFolder.Text = Settings.DstFolder;
                chkFtpUploadEnabled.Checked = Settings.Enabled;
            }
        }
    }
}
