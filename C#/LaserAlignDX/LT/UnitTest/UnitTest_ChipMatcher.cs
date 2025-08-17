using JetEazy.EzImage;
using JetEazy.Match;
using OpenCvSharp;
using System.Collections.Generic;

namespace LeTian.Match
{
    public static class Test
    {
        //static string PATH_IMAGE_LOG_ROOT => "D:\\log\\MAIN_FPIX3\\Images";
        //static string PATH_IMAGE_LOG_ONE => "D:\\log\\MAIN_FPIX3\\Images\\20250816\\20250816183713\\PositionFix";
        //static string PATH_IMAGE_LOG_ONE => "D:\\log\\MAIN_FPIX3\\Images\\20250817\\20250817205033\\PositionFix";
        static string PATH_IMAGE_LOG_ONE => @"D:\log\MAIN_FPIX3\Images\20250817\20250817212911\PositionFix";
        static string FILE_GOLDEN_IMG => "D:\\AUTOMATION\\Eazy FPI30\\_BIN_\\LASER-MAIN_FPIX3\\PIC\\00003\\bmpDefectTemplate.bmp";
        static string[] NG_FNAMES =
        {
            //"Fix_33",
            //"Fix_151",
            //"Fix_100"
            //"Fix_24"
        };

        static bool IsGoFor(string file)
        {
            if (NG_FNAMES.Length == 0)
                return true;
            foreach (var tag in NG_FNAMES)
                if (file.Contains(tag))
                    return true;
            return false;
        }
        static IEnumerable<string> IterImageFile(string path = null)
        {
            if (path == null)
                path = PATH_IMAGE_LOG_ONE; // PATH_IMAGE_LOG;

            var folders = System.IO.Directory.GetDirectories(path);
            foreach (var folder in folders)
            {
                foreach (var file in IterImageFile(folder))
                    yield return file;
            }

            var files = System.IO.Directory.GetFiles(path, "*.bmp");
            foreach (string file in files)
                yield return file;
        }
        static IEzImage loadImage(string file)
        {
            var ezImage = new EzQuickImage();
            ezImage.Load(file, bits: 8);
            return ezImage;
        }

        public static void Run()
        {
            var matcher = new EzRigidBodyGridMatcher(shrink: 1);

            // === 1) 準備 Golden 資料 ==================================================================
            using (var ezImage = loadImage(FILE_GOLDEN_IMG))
            {
                Mat img = ezImage.Image as Mat;
                matcher.SetGoldenTemplate(img);
            }

            // === 2) 測試資料 ==========================================================================
            foreach (var file in IterImageFile())
            {
                if (!IsGoFor(file))
                    continue;

                string fname = System.IO.Path.GetFileName(file);
                string dumpFile = System.IO.Path.Combine("d:\\paso.log", System.IO.Path.ChangeExtension(fname, ".png"));

                using (IEzImage ezImage2 = loadImage(file))
                {
                    Mat imgScene = ezImage2.Image as Mat;

                    var bestResult = matcher.FindBestMatch(imgScene, dumpFile);

                    if (bestResult != null)
                    {
                        var bestGrid = bestResult.Grid;
                        var box2d = bestResult.Box2D;
                        //EzPadsGridFinder.FindSpecialKeyPad(bestGrid, out int kr, out int kc, out int px);
                        //EzPadsGridFinder.FindSpecialKeyPad(imgScene, bestGrid, out int kr, out int kc, out double kSQ);
                        int kr = bestResult.KeyRow;
                        int kc = bestResult.KeyCol;
                        var kSQ = bestResult.KeySQRatio;
                        VxDebugDrawer.Draw(imgScene, box2d, bestGrid, kr, kc, Scalar.Lime, $"Best Grid [{bestGrid.Rows}x{bestGrid.Cols}] = {bestGrid.GetMajorCount()} @ {fname}");
                    }

                    Cv2.WaitKey();
                    Cv2.DestroyAllWindows();
                }
            }
        }
    }
}
