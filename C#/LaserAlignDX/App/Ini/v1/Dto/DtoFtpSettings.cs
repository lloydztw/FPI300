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
        public string DstFolder = "D6400-5014/线扫/";

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
    }
}

