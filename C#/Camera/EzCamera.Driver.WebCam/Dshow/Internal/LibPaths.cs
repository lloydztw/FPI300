#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-05-26 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion


using JetEazy.DShow.Media.ffmeg;
using System;

namespace JetEazy.DShow
{
    public class LibPaths
    {
        public static string DEFAULT_FFMEG_EXE => "D:\\AUTOMATION\\FFmpeg\\bin\\ffmpeg.exe";
        public static string DEFAULT_INI_FILE => get_default_dshow_ini_file();
        public static string FFMEG_EXE_FILE => search_ffmeg_exe_file();
        public static string VIDEO_EXT => Mp4Recorder.EXT;

        //private static string ROOT_PATH = "D:\\AUTOMATION\\Eazy Video Recorder";
        //private static string DUMP_PATH => System.IO.Path.Combine(ROOT_PATH, "work", "dump");
        //private static string INI_PATH => System.IO.Path.Combine(ROOT_PATH, "ini");
        //private static string INI_FILE => System.IO.Path.Combine(ROOT_PATH, "ini", "JetEazy.DShow.Camera.ini");

#if (false)
        public static string VIDEO_PATH => System.IO.Path.Combine(ROOT_PATH, ".video");
        public static string AUTO_VIDEO_FILE_NAME
        {
            get
            {
                var tm = DateTime.Now;
                string path = System.IO.Path.Combine(VIDEO_PATH, tm.ToString("yyyyMMdd"));
                string fname = "vid_" + tm.ToString("yyyyMMdd_HHmmss") + VIDEO_EXT;
                JetEazy.IO.QxPathUtility.InitDirectory(path);
                return System.IO.Path.Combine(path, fname);
            }
        }
        public static string AUTO_DUMP_VIDEO_FILE_NAME
        {
            get
            {
                var tm = DateTime.Now;
                string path = System.IO.Path.Combine(DUMP_PATH, "video", tm.ToString("yyyyMMdd"));
                string fname = "dump_" + tm.ToString("yyyyMMdd_HHmmss") + VIDEO_EXT;
                JetEazy.IO.QxPathUtility.InitDirectory(path);
                return System.IO.Path.Combine(path, fname);
            }
        }
        public static string AUTO_DUMP_IMAGE_FILE_NAME
        {
            get
            {
                var tm = DateTime.Now;
                string path = System.IO.Path.Combine(DUMP_PATH, "image", tm.ToString("yyyyMMdd"));
                string fname = "dump_" + tm.ToString("yyyyMMdd_HHmmss") + ".jpg";
                JetEazy.IO.QxPathUtility.InitDirectory(path);
                return System.IO.Path.Combine(path, fname);
            }
        }
        public static void InitFolders()
        {
            //JetEazy.IO.QxPathUtility.InitDirectory(INI_PATH);
            //JetEazy.IO.QxPathUtility.InitDirectory(DUMP_PATH);
        }
#endif

        #region PRIVATE_FUNCTIONS
        static string search_ffmeg_exe_file()
        {
            if (System.IO.File.Exists(DEFAULT_FFMEG_EXE))
                return DEFAULT_FFMEG_EXE;
            string path = AppDomain.CurrentDomain.BaseDirectory;
            string fname = System.IO.Path.GetFileName(DEFAULT_FFMEG_EXE);
            string fileName = System.IO.Path.Combine(path, "Bin", fname);
            if (System.IO.File.Exists(fileName))
                return fileName;
            return DEFAULT_FFMEG_EXE;
        }
        static string get_default_dshow_ini_file()
        {
            string path = System.IO.Path.GetTempPath();
            string iniFile = System.IO.Path.Combine(path, "JetEazy.DShow.Camera.ini");
            return iniFile;
        }
        #endregion
    }
}
