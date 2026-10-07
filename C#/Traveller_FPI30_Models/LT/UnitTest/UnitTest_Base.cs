using JetEazy.EzImage;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;

namespace UnitTest_FP130
{
    public abstract class Test_Base
    {
        // D:\\AUTOMATION\\Eazy FPI30\\_BIN_\\LASER-MAIN_FPIX3\\PIC\\00005\\bmpDefectTemplate.bmp";
        protected static string PATH_FILE_RECIPE(string number, string fname)
        {
            return System.IO.Path.Combine(Traveller106.Universal.RCPPATH, number, fname);
        }
        protected static string FILE_GOLDEN_IMG => PATH_FILE_RECIPE("00005", "bmpDefectTemplate.bmp"); 
        protected static string FILE_CELL_REGIONS => "D:\\paso.log\\region_cells.txt";
        protected static string PATH_IMAGE_LOG_ROOT => "D:\\log\\MAIN_FPIX3\\Images";
        protected static string PATH_IMAGE_LOG(string date = null, string time = null)
        {
            if (date != null && date.Length >= 14)
            {
                time = date.Substring(8, 6);
                date = date.Substring(0, 8);
            }
            else
            {
                if (date == null)
                    date = DateTime.Now.ToString("yyyyMMdd");
                if (time == null)
                    time = DateTime.Now.ToString("HHmmss");
            }
            return System.IO.Path.Combine(PATH_IMAGE_LOG_ROOT, date, date + time, "PositionFix");
        }

        protected string[] FAILED_NAMES = { };
        protected virtual bool IsGoFor(string file)
        {
            if (FAILED_NAMES.Length == 0)
                return true;
            foreach (var tag in FAILED_NAMES)
                if (file.Contains(tag))
                    return true;
            return false;
        }

        protected IEnumerable<string> IterImageFiles(string path = null, string ext = ".jpg")
        {
            if (path == null)
                path = PATH_IMAGE_LOG_ROOT; // PATH_IMAGE_LOG;

            var folders = System.IO.Directory.GetDirectories(path);
            foreach (var folder in folders)
            {
                foreach (var file in IterImageFiles(folder, ext))
                    yield return file;
            }

            var files = System.IO.Directory.GetFiles(path, "*" + ext);
            Array.Sort(files, (f1, f2) =>
            {
                int d = f1.Length - f2.Length;
                if (d == 0)
                    return string.Compare(f1, f2);
                return d;
            });

            foreach (string file in files)
                yield return file;
        }
        protected IEzImage LoadImage(string file)
        {
            var ezImage = new EzQuickImage();
            ezImage.Load(file, bits: 8);
            return ezImage;
        }
        
        protected List<Rect> LoadCellRects()
        {
            string file = FILE_CELL_REGIONS;

            var cellRects = new List<Rect>();
            using(var stm = new StreamReader(file))
            {
                while (!stm.EndOfStream)
                {
                    string line = stm.ReadLine();
                    line = line.Trim('\n', ';');
                    var strs = line.Split(',');
                    if (strs.Length < 4) continue;
                    int x = int.Parse(strs[0]);
                    int y = int.Parse(strs[1]);
                    int w = int.Parse(strs[2]);
                    int h = int.Parse(strs[3]);
                    cellRects.Add(new Rect(x, y, w, h));
                }
                stm.Close();
            }
            return cellRects;
        }
        protected IEnumerable<(string, Mat)> IterCells(string srcPath)
        {
            bool isFullFov = !srcPath.Contains("PositionFix");
            string ext = ".bmp";

            if (isFullFov)
            {
                var cellRects = LoadCellRects().ToArray();
                foreach (var fileName in IterImageFiles(srcPath, ext))
                {
                    if (!IsGoFor(fileName))
                        continue;

                    string fname = System.IO.Path.GetFileName(fileName);

                    using (IEzImage ezBigImage = LoadImage(fileName))
                    {
                        Mat imgFullFov = ezBigImage.Image as Mat;
                        Rect boundary = new Rect(0, 0, imgFullFov.Width, imgFullFov.Height);

                        for (int idx = 0; idx < cellRects.Length; idx++)
                        {
                            var cellRoi = cellRects[idx];
                            JetEazy.Qcvt.ClipBoundary(ref cellRoi, ref boundary);

                            var imgCell = imgFullFov[cellRoi];
                            var cellName = $"{fname} [{idx}]";
                            yield return (cellName, imgCell);
                        }
                    }
                }
            }
            else
            {
                foreach (var fileName in IterImageFiles(srcPath, ext))
                {
                    if (!IsGoFor(fileName))
                        continue;

                    string fname = System.IO.Path.GetFileName(fileName);
                    using (IEzImage ezImage = LoadImage(fileName))
                    {
                        Mat imgCell = ezImage.Image as Mat;
                        yield return (fname, imgCell);
                    }
                }
            }
        }

        public abstract void Run();
    }
}
