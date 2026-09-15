#region AUTHOR
/*
 * 
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2026-09-09 重整 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.DTO;
using System;

namespace Traveller106.Ini.V1
{
    public class DtoFtpSettings : DtoBase
    {
        #region CONST
        const string _sectName = "Ftp";
        #endregion

        #region PRIVATE_DATA
        #endregion

        public bool Enabled = false;
        public string Account = "2.5D-LC";
        public string Password = "LC^tfnm8";
        public string IpAddress = "ftp://10.62.111.34";
        public string DstFolder = "D6400-5014/";

        public override void Load(string iniFile)
        {
            Read(iniFile, _sectName, "Enabled", Enabled, out Enabled);
            Read(iniFile, _sectName, "Account", Account, out Account);
            Read(iniFile, _sectName, "Password", Password, out Password);
            Read(iniFile, _sectName, "IpAddress", IpAddress, out IpAddress);
            Read(iniFile, _sectName, "DstFolder", DstFolder, out DstFolder);
            Normalize();
        }
        public override void Save(string iniFile)
        {
            Normalize();
            Write(iniFile, _sectName, "Enabled", Enabled);
            Write(iniFile, _sectName, "Account", Account);
            Write(iniFile, _sectName, "Password", Password);
            Write(iniFile, _sectName, "IpAddress", IpAddress);
            Write(iniFile, _sectName, "DstFolder", DstFolder);
        }
        public void Normalize()
        {
            if (string.IsNullOrEmpty(IpAddress) || !IpAddress.ToLower().StartsWith("ftp://"))
                IpAddress = "ftp://" + IpAddress;

            if (!string.IsNullOrEmpty(IpAddress) && DstFolder.Contains("\\"))
                DstFolder = DstFolder.Replace("\\", "/");

            if(!string.IsNullOrEmpty(IpAddress) && !DstFolder.EndsWith("/"))
                DstFolder += "/";
        }

        public string GetSubDstFolder(string cateName, string dateTag)
        {
            // 1. 基礎目錄格式化 (統一換成 '/' 並去除頭尾斜線)
            string folder = (DstFolder ?? "").Replace("\\", "/").Trim('/');
            string cleanCate = (cateName ?? "").Replace("\\", "/").Trim('/');
            string cleanTag = (dateTag ?? "").Replace("\\", "/").Trim('/');

            // 2. 檢查 DstFolder 尾部是否已經包含了 cateName，若有則先移除
            if (!string.IsNullOrEmpty(cleanCate))
            {
                // 檢查情境 A: DstFolder 完全等於 cateName (例如 "线扫")
                if (folder.Equals(cleanCate, StringComparison.OrdinalIgnoreCase))
                {
                    folder = "";
                }
                // 檢查情境 B: DstFolder 結尾為 "/cateName" (例如 "D6400-5014/线扫")
                else if (folder.EndsWith("/" + cleanCate, StringComparison.OrdinalIgnoreCase))
                {
                    folder = folder.Substring(0, folder.Length - (cleanCate.Length + 1));
                }
            }

            // 3. 安全拼接 subName
            if (!string.IsNullOrEmpty(cleanCate))
            {
                folder = string.IsNullOrEmpty(folder) ? cleanCate : folder + "/" + cleanCate;
            }

            // 4. 安全拼接 dateTag
            if (!string.IsNullOrEmpty(cleanTag))
            {
                folder = string.IsNullOrEmpty(folder) ? cleanTag : folder + "/" + cleanTag;
            }

            // 5. 確保 FTP 格式結尾帶有 '/'
            return string.IsNullOrEmpty(folder) ? "" : folder + "/";
        }
    }
}

