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


namespace JetEazy.DShow.Tool
{
    internal class ToolPaths
    {
        public static string ROOT_PATH = "D:\\paso.log\\JetEazy.DShow";
        public static string DUMP_PATH => System.IO.Path.Combine(ROOT_PATH, "work", "dump");
        public static string INI_PATH => System.IO.Path.Combine(ROOT_PATH, "ini");
        public static string INI_FILE => System.IO.Path.Combine(ROOT_PATH, "ini", "JetEazy.DShow.Camera.ini");

        public static string VIDEO_EXT => Mp4Recorder.EXT;
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
            JetEazy.IO.QxPathUtility.InitDirectory(INI_PATH);
            JetEazy.IO.QxPathUtility.InitDirectory(DUMP_PATH);
        }
    }
}
