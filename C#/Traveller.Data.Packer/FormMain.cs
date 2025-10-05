using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Traveller.Data.Packer
{
    public partial class FormMain : Form
    {
        class Global
        {
            public const string S_FILE_7Z_EXE = @"C:\Program Files\7-Zip\7z.exe";
            public const string S_PATH_APP = "D:\\Automation\\Eazy FPI30\\";
            public static string S_PATH_RECIPE_ROOT => System.IO.Path.Combine(S_PATH_APP, "_V03_", "LASER-MAIN_FPIX3");
            public static string GetDairyPath(string path)
            {
                if (!System.IO.Directory.Exists(path))
                    System.IO.Directory.CreateDirectory(path);

                path = System.IO.Path.Combine(path, DateTime.Now.ToString("yyyyMMdd"));

                if (!System.IO.Directory.Exists(path))
                    System.IO.Directory.CreateDirectory(path);

                if (!path.EndsWith("\\"))
                    path += "\\";

                return path;
            }
        }

        #region PUBLIC_DATA
            public DateTime m_tmStartTime;
            public DateTime m_tmEndTime;
            public bool m_optCopyLogs = true;
        #endregion

        public FormMain()
        {
            InitializeComponent();
            var ts = new TimeSpan(1, 0, 0);
            m_tmEndTime = DateTime.Now;
            var tm = m_tmEndTime - ts;
            m_tmStartTime = new DateTime(tm.Year, tm.Month, tm.Day, tm.Hour, 0, 0);
            _updateData(false);
            _check7z();
        }
        private void btnPack_Click(object sender, EventArgs e)
        {
            btnPack.Enabled = false;
            _updateData(true);

            Action a = _doPackData;
            a.BeginInvoke(null, null);
        }

        private void _updateData(bool bToKernel)
        {
            if (bToKernel)
            {
                m_tmStartTime = tmPickerBegin.Value;
                m_tmEndTime = tmPickerEnd.Value;
                m_optCopyLogs = chkCopyLogs.Checked;
                Properties.Settings.Default.MachineNo = (int)numMachineID.Value;
                Properties.Settings.Default.Save();
            }
            else
            {
                tmPickerBegin.Value = m_tmStartTime;
                tmPickerEnd.Value = m_tmEndTime;
                chkCopyLogs.Checked = m_optCopyLogs;
                numMachineID.Value = Properties.Settings.Default.MachineNo;
            }
        }
        private void _doPackData()
        {
            int machineID = (int)numMachineID.Value;
            string deskTopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string dstPath = System.IO.Path.Combine(deskTopPath, $"Machine_#{machineID:00}_" + DateTime.Now.ToString("yyyyMMdd"));

            Action<string> funcEnd = new Action<string>((string err) =>
            {
                string msg;
                if (string.IsNullOrEmpty(err))
                    msg = "數據已經打包到 " + dstPath.Replace(deskTopPath, "[桌面] ")
                        + "\n\r\n\r(如果有安裝 7z, 數據檔案會自動生成 zip 壓縮檔.)";
                else
                    msg = "異常: " + err;

                MessageBox.Show(msg);
                _trace(msg);

                btnPack.Enabled = true;
                this.Visible = true;
            });

            try
            {
                _clearPath(dstPath);
                _checkPath(dstPath);

                string dbFile = System.IO.Path.Combine(Global.S_PATH_RECIPE_ROOT, "DB", "ESSDB.jdb");
                JetEazy.Utils.WinIni.Read(dbFile, "ESSDB", "LastRecipeIndex", 5, out int rcpIndex);
                string rcpFolder = $"{rcpIndex:00000}";

                _doCopyFile(System.IO.Path.Combine(Global.S_PATH_RECIPE_ROOT, "CONFIG.ini"), dstPath);
                _doCopyFolder(System.IO.Path.Combine(Global.S_PATH_RECIPE_ROOT, "DB"), dstPath);
                _doCopyFolder(System.IO.Path.Combine(Global.S_PATH_RECIPE_ROOT, "PIC", rcpFolder), dstPath);

                string workPath = System.IO.Path.Combine(Global.S_PATH_RECIPE_ROOT, "WORK");
                _doCopyFile(System.IO.Path.Combine(workPath, "CAMERA.ini"), dstPath);
                _doCopyFile(System.IO.Path.Combine(workPath, "MAIN_FPIX3", "IO.INI"), dstPath);
                _doCopyFile(System.IO.Path.Combine(workPath, "MAIN_FPIX3", "LightCONTROL0.INI"), dstPath);
                _doCopyFile(System.IO.Path.Combine(workPath, "MAIN_FPIX3", "LightCONTROL1.INI"), dstPath);
                _doCopyFile(System.IO.Path.Combine(workPath, "MAIN_FPIX3", "PLCCONTROL0.INI"), dstPath);
                _doCopyFolder(System.IO.Path.Combine(Global.S_PATH_RECIPE_ROOT, "EmptyTrayAoi", "Ini"), dstPath);
                _doCopyFolder(System.IO.Path.Combine(Global.S_PATH_RECIPE_ROOT, "EmptyTrayAoi", "Recipes"), dstPath);

                _doZip(dstPath);
                BeginInvoke(funcEnd, new object[] { "" });
            }
            catch (Exception ex)
            {
                BeginInvoke(funcEnd, new object[] { ex.Message });
            }
        }
        
        private void _checkPath(string path)
        {
            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);
        }
        private void _doCopyFile(string srcFile, string dstPathRoot)
        {
            try
            {
                _trace("copy: " + System.IO.Path.GetFileName(srcFile));
                var srcPath = System.IO.Path.GetDirectoryName(srcFile);
                var dstPath = System.IO.Path.Combine(dstPathRoot, srcPath.Replace(Global.S_PATH_APP, ""));

                _checkPath(dstPath);
                var dstFile = System.IO.Path.Combine(dstPath, System.IO.Path.GetFileName(srcFile));

                System.IO.File.Copy(srcFile, dstFile, true);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        private void _doCopyFolder(string srcPath, string dstPathRoot)
        {
            try
            {
                var dstPath = System.IO.Path.Combine(dstPathRoot, srcPath.Replace(Global.S_PATH_APP, ""));
                _checkPath(dstPath);

                var files = System.IO.Directory.GetFiles(srcPath, "*.*");
                foreach (var srcFile in files)
                {
                    _trace("copy: " + System.IO.Path.GetFileName(srcFile));
                    var dstFile = System.IO.Path.Combine(dstPath, System.IO.Path.GetFileName(srcFile));
                    System.IO.File.Copy(srcFile, dstFile, true);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void _doCopyFolderTm(string srcPath, string dstPathRoot, bool optReplacingAppPath = true, string pattern = null)
        {
            if (!System.IO.Directory.Exists(srcPath))
                return;

            var dstPath = optReplacingAppPath ?
                System.IO.Path.Combine(dstPathRoot, srcPath.Replace(Global.S_PATH_APP, "")) :
                dstPathRoot;

            if (pattern == null)
                _doCopyFolderTmB(srcPath, dstPath);
            else
                _doCopyFolderTmA(srcPath, dstPath, pattern);
        }
        private void _doCopyFolderTmA(string srcPath, string dstPath, string pattern)
        {
            try
            {
                if (!System.IO.Directory.Exists(srcPath))
                    return;

                int count = 0;
                //> _checkPath(dstPath);

                var oneHour = new TimeSpan(1, 0, 0);
                var tmBegin = new DateTime(m_tmStartTime.Year, m_tmStartTime.Month, m_tmStartTime.Day, m_tmStartTime.Hour, 0, 0);
                var tmEnd = new DateTime(m_tmEndTime.Year, m_tmEndTime.Month, m_tmEndTime.Day, m_tmEndTime.Hour, 0, 0);
                tmEnd += new TimeSpan(1, 0, 0);


                var files = System.IO.Directory.GetFiles(srcPath, pattern);
                foreach (var srcFile in files)
                {
                    var tm = System.IO.File.GetCreationTime(srcFile);
                    if (!(tmBegin <= tm && tm < tmEnd))
                        continue;

                    if (count == 0)
                        _checkPath(dstPath);

                    _trace("copy: " + System.IO.Path.GetFileName(srcFile));
                    var dstFile = System.IO.Path.Combine(dstPath, System.IO.Path.GetFileName(srcFile));
                    System.IO.File.Copy(srcFile, dstFile, true);

                    count++;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void _doCopyFolderTmB(string srcPath, string dstPath)
        {
            try
            {
                if (!System.IO.Directory.Exists(srcPath))
                    return;

                int count = 0;
                //> _checkPath(dstPath);

                var oneHour = new TimeSpan(1, 0, 0);
                var tmBegin = new DateTime(m_tmStartTime.Year, m_tmStartTime.Month, m_tmStartTime.Day, m_tmStartTime.Hour, 0, 0);
                var tmEnd = new DateTime(m_tmEndTime.Year, m_tmEndTime.Month, m_tmEndTime.Day, m_tmEndTime.Hour, 0, 0);
                for (var tm = tmBegin; tm <= tmEnd; tm += oneHour)
                {
                    var pattern = tm.ToString("*yyyyMMdd_HH*.*");
                    var files = System.IO.Directory.GetFiles(srcPath, pattern);
                    foreach (var srcFile in files)
                    {
                        if (count == 0)
                            _checkPath(dstPath);

                        if (!m_optCopyLogs)
                        {
                            if (string.Compare(System.IO.Path.GetExtension(srcFile), ".jpg", true) == 0)
                                continue;
                        }

                        _trace("copy: " + System.IO.Path.GetFileName(srcFile));
                        var dstFile = System.IO.Path.Combine(dstPath, System.IO.Path.GetFileName(srcFile));
                        System.IO.File.Copy(srcFile, dstFile, true);

                        count++;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void _doCopyFolderDir(string srcPath, string dstPathRoot, bool optTmSubFolder = true)
        {
            try
            {
                if (!System.IO.Directory.Exists(srcPath))
                    return;

                if (optTmSubFolder)
                {
                    _doCopyFolderTm(srcPath, dstPathRoot);

                    var oneDay = new TimeSpan(1, 0, 0, 0);
                    var tmBegin = new DateTime(m_tmStartTime.Year, m_tmStartTime.Month, m_tmStartTime.Day);
                    var tmEnd = new DateTime(m_tmEndTime.Year, m_tmEndTime.Month, m_tmEndTime.Day);
                    for (var tm = tmBegin; tm <= tmEnd; tm += oneDay)
                    {
                        var srcPath2 = System.IO.Path.Combine(srcPath, tm.ToString("yyyyMMdd"));
                        _doCopyFolderTm(srcPath2, dstPathRoot);
                    }
                }
                else
                {
                    var subFolders = System.IO.Directory.GetDirectories(srcPath);
                    foreach (var dir in subFolders)
                    {
                        var srcPath2 = dir;
                        var dstPath2 = dir.Replace(srcPath, dstPathRoot);
                        _doCopyFolderTm(srcPath2, dstPath2, false);
                        _doCopyFolderDir(srcPath2, dstPath2, false);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void _doZip(string srcPath)
        {
            try
            {
                if (!System.IO.File.Exists(Global.S_FILE_7Z_EXE))
                    return;

                string zipFileName = srcPath + ".zip";
                if (zipFileName.EndsWith("\\.zip"))
                    zipFileName = zipFileName.Replace("\\.zip", ".zip");

                // 刪除已存在之 zip
                if (System.IO.File.Exists(zipFileName))
                    System.IO.File.Delete(zipFileName);

                string cmdStr = string.Format("\"{0}\"", Global.S_FILE_7Z_EXE);
                // 注意: 舊版 7z.exe 不支援 -sdel 參數.
                // string argStr = string.Format("a {0} -r -sdel {1}", zipFileName, srcPath);
                string argStr = string.Format("a {0} -r {1}", zipFileName, srcPath);
                var ps = System.Diagnostics.Process.Start(cmdStr, argStr);
                ps.WaitForExit();

                // 如果產生 zip, 就刪除原資料夾.
                if (System.IO.File.Exists(zipFileName))
                {
                    _trace("等待壓縮完成...");
                    System.Threading.Thread.Sleep(2000);
                    _clearPath(srcPath);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void _check7z()
        {
            if (System.IO.File.Exists(Global.S_FILE_7Z_EXE))
            {
                _trace("7z 已支援.");
                lblStatus.ForeColor = System.Drawing.Color.Lime;
            }
            else
            {
                _trace("建議: 請安裝 7z !");
                lblStatus.ForeColor = System.Drawing.Color.Red;
            }
        }
        private void _clearPath(string rootPath)
        {
            if (!System.IO.Directory.Exists(rootPath))
                return;

            var files = System.IO.Directory.GetFiles(rootPath, "*.*");
            foreach (var f in files)
            {
                System.IO.File.Delete(f);
            }

            var paths = System.IO.Directory.GetDirectories(rootPath);
            foreach (var p in paths)
            {
                _clearPath(p);
            }

            try
            {
                System.IO.Directory.Delete(rootPath, true);
            }
            catch
            {
            }
        }
        private void _trace(string msg)
        {
            if (InvokeRequired)
            {
                Action<string> a = _trace;
                BeginInvoke(a, new object[] { msg });
            }
            else
            {
                lblStatus.Text = msg;
                lblStatus.Refresh();
            }
        }
    }
}
