#region AUTHOR
/*
 * 
 * Copyright (c) 2024 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2024-04-25 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion


using JetEazy.Win32;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace EzCamera.Driver.Sim
{
    public class EzFileUtil
    {
        public const string CMD_LAST_FILE = "[LAST_FILE]";
        public const string CMD_PREV_FILE = "[PREV_FILE]";
        public const string CMD_NEXT_FILE = "[NEXT_FILE]";
        public const string CMD_DEFAULT_FILE = "[DEFAULT]";

        #region PRIVATE_DATA
        string[] _exts = new string[] { ".jpg" };
        string _currentFileName;
        #endregion

        public EzFileUtil(string title, params string[] exts)
        {
            this.Title = title;
            _exts = exts;
        }

        public string Title
        {
            get;
            set;
        }
        public string ActiveFileName
        {
            get { return _currentFileName; }
        }
        public string Browse(string filePath, bool silent = false)
        {
            if (filePath == CMD_LAST_FILE)
            {
                return _currentFileName;
            }
            else if (filePath == CMD_NEXT_FILE)
            {
                filePath = getNextFileName(_currentFileName, 1);
                if (filePath == null)
                    return null;
            }
            else if (filePath == CMD_PREV_FILE)
            {
                filePath = getNextFileName(_currentFileName, -1);
                if (filePath == null)
                    return null;
            }

            if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
            {
                filePath = !silent ? openFileDialogBox() : null;
                if (filePath == null)
                {
                    return null;
                }
            }

            _currentFileName = filePath;
            return filePath;
        }
        public string GetSafeAvailableFile(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
                string fileFound = null;
                string fileSearh = filePath;
                string path = System.IO.Path.GetDirectoryName(fileSearh);
                if (System.IO.Directory.Exists(path))
                {
                    string stem = System.IO.Path.GetFileNameWithoutExtension(fileSearh);
                    foreach (var ext in _exts)
                    {
                        fileSearh = System.IO.Path.Combine(path, stem + ext);
                        if (System.IO.File.Exists(fileSearh))
                        {
                            fileFound = fileSearh;
                            break;
                        }
                    }
                }
                if (fileFound == null)
                {
                    throw new ApplicationException("找不到檔案: " + filePath);
                }
                filePath = fileFound;
            }
            return filePath;
        }

        #region PRIVATE_FUNCTIONS
        string openFileDialogBox()
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = $"Select {Title} File";
                dlg.Filter = makeFilterString(_exts);
                dlg.FileName = "*.*";

                if (!string.IsNullOrEmpty(_currentFileName))
                    dlg.InitialDirectory = System.IO.Path.GetDirectoryName(_currentFileName);

                if (DialogResult.OK == dlg.ShowDialog())
                {
                    //Console.WriteLine($"選了檔案 {dlg.FileName}");
                    return dlg.FileName;
                }
            }
            return null;
        }
        string makeFilterString(IEnumerable<string> exts)
        {
            var str = "";
            foreach (var ext in exts)
            {
                str += $"{ext.TrimStart('.').ToUpper()} Files(*{ext})|*{ext}|";
            }
            str += "All Files(*.*)|*.*";
            return str;
        }
        string getNextFileName(string fileName, int offset = 1)
        {
            if (fileName == null)
                return null;
            if (!System.IO.File.Exists(fileName))
                return null;
            string path = System.IO.Path.GetDirectoryName(fileName);
            string ext = System.IO.Path.GetExtension(fileName);
            var files = System.IO.Directory.GetFiles(path, "*" + ext);
            int idx = Array.IndexOf(files, fileName) + offset;
            if (idx >= 0 && idx < files.Length)
                return files[idx];
            return null;
        }
        #endregion

        #region PRIVATE_INI_FUNCTIONS
        static string defaultFriendlyName
        {
            get => System.IO.Path.GetFileNameWithoutExtension(AppDomain.CurrentDomain.FriendlyName);
        }
        public static string GetIniFileName(string tag = null, string ext = null)
        {
            string path = System.IO.Path.GetTempPath();
            string fname = !string.IsNullOrEmpty(tag)
                         ? System.IO.Path.GetFileNameWithoutExtension(tag)
                         : defaultFriendlyName;
            if (ext == null)
                ext = ".ini";
            return System.IO.Path.Combine(path, fname + ext);
        }
        public string GetIniSectionName(string tag = null)
        {
            if (tag == null)
            {
                return Title != null ? Title : "EzFileUtil";
            }
            return tag;
        }
        public void LoadIni(string fname = null, string sectName = null)
        {
#if (OPT_SHARE_CAMERA_INI)
            fname = "EzSimCamera";
#endif
            var iniFileName = GetIniFileName(fname);
            sectName = GetIniSectionName(sectName);
            Win32Ini.Load(ref _currentFileName, iniFileName, sectName, "lastFile");
        }
        public void SaveIni(string fname = null, string sectName = null)
        {
#if (OPT_SHARE_CAMERA_INI)
            fname = "EzSimCamera";
#endif
            var iniFileName = GetIniFileName(fname);
            sectName = GetIniSectionName(sectName);
            Win32Ini.Save(_currentFileName, iniFileName, sectName, "lastFile");
        }
        #endregion
    }
}
