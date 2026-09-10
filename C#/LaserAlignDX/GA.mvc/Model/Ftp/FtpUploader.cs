#region AUTHOR
/*
 * 
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *       2026-09-10 Created FtpUploader (by JetEazy Team)
 *       2026-09-10 Added TestConnection Method (by JetEazy Team)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Traveller106.Ini.V1;

namespace JetEazy.Utils
{
    public class FtpUploader
    {
        #region PRIVATE_DATA
        private readonly DtoFtpSettings _settings;
        #endregion

        #region LOCKS
        private static readonly object _dirLock = new object();
        private readonly object _logLock = new object();
        #endregion

        /// <summary>
        /// 日誌輸出委派 (可用於綁定 UI 控制項或 Log 檔案)
        /// </summary>
        public Action<string> OnLog { get; set; }

        public FtpUploader(DtoFtpSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _settings.Normalize();
        }

        public bool Enabled
        {
            get => _settings!=null && _settings.Enabled;
        }

        /// <summary>
        /// 測試 FTP 伺服器連線與認證是否正常 (同步)
        /// </summary>
        /// <param name="timeoutMs">連線超時時間 (毫秒)，預設 5000ms</param>
        /// <returns>連線成功返回 true，失敗返回 false</returns>
        public bool TestConnection(int timeoutMs = 5000)
        {
            if (!_settings.Enabled)
            {
                Log("[連線測試] FTP 功能未啟用 (Enabled = false)。");
                return false;
            }

            try
            {
                // 測試連線至根目錄或指定目標目錄
                string testUrl = CombineUrl(_settings.IpAddress, _settings.DstFolder);
                Log($"[連線測試] 開始測試連線至: {testUrl}");

                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(testUrl);
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.Credentials = new NetworkCredential(_settings.Account, _settings.Password);
                request.UsePassive = true;
                request.KeepAlive = false;
                request.Timeout = timeoutMs;

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    Log($"[連線測試成功] 伺服器回應: {response.StatusDescription.Trim()}");
                    return true;
                }
            }
            catch (WebException ex)
            {
                // 若目標資料夾不存在 (550)，改嘗試只測試 IP 根目錄
                if (ex.Response is FtpWebResponse response && response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                {
                    Log($"[連線測試提示] 目標目錄不存在，嘗試測試 FTP 根目錄...");
                    return TestBaseConnection(timeoutMs);
                }

                Log($"[連線測試失敗] WebException: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Log($"[連線測試失敗] Exception: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 測試 FTP 伺服器連線與認證是否正常 (非同步)
        /// </summary>
        public async Task<bool> TestConnectionAsync(int timeoutMs = 5000)
        {
            return await Task.Run(() => TestConnection(timeoutMs));
        }

        /// <summary>
        /// 同步上傳單一檔案至 FTP 伺服器
        /// </summary>
        public bool UploadFile(string localFilePath, string remoteFileName = "")
        {
            if (!_settings.Enabled)
            {
                Log("FTP 功能未啟用 (Enabled = false)，取消上傳。");
                return false;
            }

            if (!File.Exists(localFilePath))
            {
                Log($"[錯誤] 本地檔案不存在: {localFilePath}");
                return false;
            }

            if (string.IsNullOrEmpty(remoteFileName))
            {
                remoteFileName = Path.GetFileName(localFilePath);
            }

            try
            {
                // 1. 確保遠端目標目錄存在
                EnsureDirectoryExists(_settings.DstFolder);

                // 2. 組合完整的 FTP 檔案 URI
                string uploadUrl = CombineUrl(_settings.IpAddress, _settings.DstFolder, remoteFileName);
                Log($"開始上傳檔案: {localFilePath} -> {uploadUrl}");

                // 3. 建立 FtpWebRequest 請求
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uploadUrl);
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.Credentials = new NetworkCredential(_settings.Account, _settings.Password);
                request.UseBinary = true;
                request.UsePassive = true;
                request.KeepAlive = false;

                // 4. 寫入檔案串流
                using (FileStream fileStream = File.OpenRead(localFilePath))
                using (Stream requestStream = request.GetRequestStream())
                {
                    fileStream.CopyTo(requestStream);
                }

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    Log($"[成功] 檔案上傳完成，伺服器回應: {response.StatusDescription.Trim()}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log($"[例外] 上傳檔案失敗: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 非同步上傳單一檔案至 FTP 伺服器
        /// </summary>
        public async Task<bool> UploadFileAsync(string localFilePath, string remoteFileName = "")
        {
            return await Task.Run(() => UploadFile(localFilePath, remoteFileName));
        }

        #region PRIVATE_METHODS

        /// <summary>
        /// 僅測試 FTP 根目錄帳密連線
        /// </summary>
        private bool TestBaseConnection(int timeoutMs)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(_settings.IpAddress);
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.Credentials = new NetworkCredential(_settings.Account, _settings.Password);
                request.UsePassive = true;
                request.KeepAlive = false;
                request.Timeout = timeoutMs;

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    Log($"[連線測試成功] 根目錄連線正常，伺服器回應: {response.StatusDescription.Trim()}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log($"[連線測試失敗] 根目錄連線失敗: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 遞迴確保 FTP 遠端多層資料夾存在，若不存在則自動建立
        /// </summary>
        private void EnsureDirectoryExists_000(string remoteFolderPath)
        {
            if (string.IsNullOrWhiteSpace(remoteFolderPath))
                return;

            string[] folders = remoteFolderPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            string currentPath = _settings.IpAddress;

            foreach (string folder in folders)
            {
                currentPath = CombineUrl(currentPath, folder);

                try
                {
                    FtpWebRequest request = (FtpWebRequest)WebRequest.Create(currentPath);
                    request.Method = WebRequestMethods.Ftp.MakeDirectory;
                    request.Credentials = new NetworkCredential(_settings.Account, _settings.Password);
                    request.UsePassive = true;
                    request.KeepAlive = false;

                    using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                    {
                        Log($"遠端建立目錄成功: {currentPath}");
                    }
                }
                catch (WebException ex)
                {
                    if (ex.Response is FtpWebResponse response)
                    {
                        // 550 代表目錄已存在，忽略此錯誤繼續推進下一層
                        if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                        {
                            continue;
                        }
                    }
                    Log($"[提示] 建立目錄流程訊息 ({folder}): {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 遞迴確保 FTP 遠端多層資料夾存在，若不存在則自動建立
        /// </summary>
        private void EnsureDirectoryExists(string remoteFolderPath)
        {
            if (string.IsNullOrWhiteSpace(remoteFolderPath))
                return;

            // 2. 目錄建立過程加鎖，確保同一時間只有一個 Thread 在檢查/建立目錄
            lock (_dirLock)
            {
                string[] folders = remoteFolderPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                string currentPath = _settings.IpAddress;

                foreach (string folder in folders)
                {
                    currentPath = CombineUrl(currentPath, folder);

                    try
                    {
                        FtpWebRequest request = (FtpWebRequest)WebRequest.Create(currentPath);
                        request.Method = WebRequestMethods.Ftp.MakeDirectory;
                        request.Credentials = new NetworkCredential(_settings.Account, _settings.Password);
                        request.UsePassive = true;
                        request.KeepAlive = false;

                        using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                        {
                            Log($"遠端建立目錄成功: {currentPath}");
                        }
                    }
                    catch (WebException ex)
                    {
                        if (ex.Response is FtpWebResponse response)
                        {
                            if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                            {
                                continue;
                            }
                        }
                        Log($"[提示] 建立目錄流程訊息 ({folder}): {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// 安全組合 URL 路徑，避免斜線重複或遺漏
        /// </summary>
        private string CombineUrl(params string[] parts)
        {
            if (parts == null || parts.Length == 0)
                return string.Empty;

            string result = parts[0].TrimEnd('/');

            for (int i = 1; i < parts.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(parts[i]))
                    continue;

                string p = parts[i].Trim('/', '\\');
                if (!string.IsNullOrEmpty(p))
                {
                    result += "/" + p;
                }
            }

            return result;
        }

        private void Log(string message)
        {
            // 3. Log 觸發加鎖，避免多執行緒同時調用 OnLog
            lock (_logLock)
            {
                OnLog?.Invoke($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [FtpUploader] {message}");
            }
        }
        #endregion
    }
}
