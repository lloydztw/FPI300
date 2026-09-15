#region AUTHOR
/*
 * 
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *        2026-09-10 Created FtpUploader (by JetEazy Team)
 *        2026-09-10 Added TestConnection Method (by JetEazy Team)
 *        2026-09-10 Optimized Lock Scope & Directory Cache (by JetEazy Team)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Traveller106.Ini.V1;

namespace JetEazy.Utils
{
    public class FtpUploader
    {
        #region PRIVATE_DATA
        private readonly DtoFtpSettings _settings;

        // 使用記憶體快取已建立過的目錄，大幅提升多執行緒效能
        private static readonly ConcurrentDictionary<string, bool> _createdDirectories = new ConcurrentDictionary<string, bool>();
        #endregion

        #region LOCKS
        private static readonly object _dirLock = new object();
        private readonly object _logLock = new object();
        #endregion

        #region PROPERTIES
        /// <summary>
        /// 日誌輸出委派 (可用於綁定 UI 控制項或 Log 檔案)
        /// </summary>
        public Action<string> OnLog { get; set; }

        public bool Enabled
        {
            get => _settings != null && _settings.Enabled;
        }
        #endregion

        public FtpUploader(DtoFtpSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _settings.Normalize();
        }

        #region PUBLIC_METHODS

        /// <summary>
        /// 測試 FTP 伺服器連線與認證是否正常 (同步)
        /// </summary>
        /// <param name="timeoutMs">連線超時時間 (毫秒)，預設 5000ms</param>
        public bool TestConnection(int timeoutMs = 5000)
        {
            if (!_settings.Enabled)
            {
                Log("[連線測試] FTP 功能未啟用 (Enabled = false)。");
                return false;
            }

            try
            {
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
        /// 同步上傳單一檔案至 FTP 伺服器 (Thread-Safe)
        /// </summary>
        public bool UploadFile(string srcFileName, string cateName, DateTime time, string lotID, int timeoutMs = 15000)
        {
            if (!_settings.Enabled)
            {
                Log("FTP 功能未啟用 (Enabled = false)，取消上傳。");
                return false;
            }

            if (!File.Exists(srcFileName))
            {
                Log($"[錯誤] 本地檔案不存在: {srcFileName}");
                return false;
            }

            string remoteFileName = Path.GetFileName(srcFileName);
            string remoteSubFolder = $"{time:yyyyMMdd}/{lotID}";
            string remoteFolder = _settings.GetSubDstFolder(cateName, remoteSubFolder);

            try
            {
                // 1. 確保遠端目標目錄存在
                EnsureDirectoryExists(remoteFolder);

                // 2. 組合完整的 FTP 檔案 URI
                string uploadUrl = CombineUrl(_settings.IpAddress, remoteFolder, remoteFileName);
                Log($"開始上傳檔案: {srcFileName} -> {uploadUrl}");

                // 3. 建立 FtpWebRequest 請求
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uploadUrl);
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.Credentials = new NetworkCredential(_settings.Account, _settings.Password);
                request.UseBinary = true;
                request.UsePassive = true;
                request.KeepAlive = false;
                request.Timeout = timeoutMs;
                request.ReadWriteTimeout = timeoutMs;

                // 4. 寫入檔案串流
                using (FileStream fileStream = File.OpenRead(srcFileName))
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
        public async Task<bool> UploadFileAsync(string srcFileName, string subName, DateTime time, string lotID, int timeoutMs = 15000)
        {
            return await Task.Run(() => UploadFile(srcFileName, subName, time, lotID, timeoutMs));
        }

        #endregion

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
                request.Timeout = timeoutMs; // 修正：補上 Timeout 設定

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
        /// 遞迴確保 FTP 遠端多層資料夾存在，帶有快取機制與跨執行緒保護
        /// </summary>
        private void EnsureDirectoryExists(string remoteFolderPath)
        {
            if (string.IsNullOrWhiteSpace(remoteFolderPath))
                return;

            string fullFolderPath = CombineUrl(_settings.IpAddress, remoteFolderPath);

            // 快取命中：若該目錄已經在本次程序生命週期中建立過，直接跳過 (大幅增加併發傳輸效能)
            if (_createdDirectories.ContainsKey(fullFolderPath))
                return;

            lock (_dirLock)
            {
                // 二次檢查 (Double-check Locking)
                if (_createdDirectories.ContainsKey(fullFolderPath))
                    return;

                string[] folders = remoteFolderPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                string currentPath = _settings.IpAddress;

                foreach (string folder in folders)
                {
                    currentPath = CombineUrl(currentPath, folder);

                    if (_createdDirectories.ContainsKey(currentPath))
                        continue;

                    try
                    {
                        FtpWebRequest request = (FtpWebRequest)WebRequest.Create(currentPath);
                        request.Method = WebRequestMethods.Ftp.MakeDirectory;
                        request.Credentials = new NetworkCredential(_settings.Account, _settings.Password);
                        request.UsePassive = true;
                        request.KeepAlive = false;
                        request.Timeout = 5000;

                        using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                        {
                            Log($"遠端建立目錄成功: {currentPath}");
                        }
                    }
                    catch (WebException ex)
                    {
                        if (ex.Response is FtpWebResponse response)
                        {
                            // 550 代表目錄已存在，屬於正常狀況
                            if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                            {
                                // 標記已存在，不再重複檢查
                                _createdDirectories.TryAdd(currentPath, true);
                                continue;
                            }
                        }
                        Log($"[提示] 建立目錄流程訊息 ({folder}): {ex.Message}");
                    }

                    _createdDirectories.TryAdd(currentPath, true);
                }
            }
        }

        /// <summary>
        /// 遞迴確保 FTP 遠端多層資料夾存在，帶有快取機制與跨執行緒保護
        /// </summary>
        private void EnsureDirectoryExists_001(string remoteFolderPath)
        {
            if (string.IsNullOrWhiteSpace(remoteFolderPath))
                return;

            string fullFolderPath = CombineUrl(_settings.IpAddress, remoteFolderPath);

            if (_createdDirectories.ContainsKey(fullFolderPath))
                return;

            lock (_dirLock)
            {
                if (_createdDirectories.ContainsKey(fullFolderPath))
                    return;

                string[] folders = remoteFolderPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                string currentPath = _settings.IpAddress;

                foreach (string folder in folders)
                {
                    currentPath = CombineUrl(currentPath, folder);

                    if (_createdDirectories.ContainsKey(currentPath))
                        continue;

                    bool isCreatedOrExists = false;

                    try
                    {
                        FtpWebRequest request = (FtpWebRequest)WebRequest.Create(currentPath);
                        request.Method = WebRequestMethods.Ftp.MakeDirectory;
                        request.Credentials = new NetworkCredential(_settings.Account, _settings.Password);
                        request.UsePassive = true;
                        request.KeepAlive = false;
                        request.Timeout = 5000;

                        using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                        {
                            Log($"遠端建立目錄成功: {currentPath}");
                            isCreatedOrExists = true;
                        }
                    }
                    catch (WebException ex)
                    {
                        if (ex.Response is FtpWebResponse response &&
                            response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                        {
                            // 550 代表目錄已存在
                            Log($"[提示] 目錄已存在: {currentPath}");
                            isCreatedOrExists = true;
                        }
                        else
                        {
                            Log($"[警告] 建立目錄失敗 ({folder}): {ex.Message}");
                            // 發生其他非預期錯誤時，不應將其加入快取，並可視需求決定是否直接 throw 停止後續動作
                            break;
                        }
                    }

                    if (isCreatedOrExists)
                    {
                        _createdDirectories.TryAdd(currentPath, true);
                    }
                }

                // 若整條完整路徑都確認成功，把完整路徑也加入快取
                _createdDirectories.TryAdd(fullFolderPath, true);
            }
        }

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
            lock (_logLock)
            {
                OnLog?.Invoke($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [FtpUploader] {message}");
            }
        }
        #endregion
    }
}
