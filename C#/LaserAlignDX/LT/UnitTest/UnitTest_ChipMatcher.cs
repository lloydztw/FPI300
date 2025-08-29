#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-08-18 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.EzImage;
using LeTian.AoiLib;
using OpenCvSharp;
using System;


namespace UnitTest_FP130
{
    /// <summary>
    /// 單元測試: 晶粒樣板比對 (Template Match)
    /// </summary>
    public class Test_ChipMatcher : Test_Base
    {
        //static string PATH_IMAGE_LOG_ONE => PATH_IMAGE_LOG("20250825171200");
        //static string PATH_IMAGE_LOG_ONE => PATH_IMAGE_LOG("20250825180739");
        static string PATH_IMAGE_LOG_ONE => PATH_IMAGE_LOG("20250828042243");   // BLACK
        static string PATH_DUMP => "d:\\paso.log\\chipLoc";

        int[] ng_numbers = new int[]
        {
            //83,
            //89,
            91,
        };

        public override void Run()
        {
            FAILED_NAMES = Array.ConvertAll(ng_numbers, x => $"_{x}_");

            EzPadsGridFinder.VISUAL_DEBUG = true;
            var matcher = new EzRigidBodyGridMatcher(shrink: 1);
            matcher.PadThreshold = 200;

            // (0) Directory
            JetEazy.IO.QxPathUtility.InitDirectory(PATH_DUMP);

            // (1) 準備 Golden 資料
            using (var ezImage = LoadImage(FILE_GOLDEN_IMG))
            {
                Mat img = ezImage.Image as Mat;
                matcher.SetGoldenTemplate(img);
            }

            // (2) 測試資料
            foreach (var file in IterImageFiles(PATH_IMAGE_LOG_ONE, ".bmp"))
            {
                if (!IsGoFor(file))
                    continue;

                string fname = System.IO.Path.GetFileName(file);
                string dumpFile = System.IO.Path.Combine(PATH_DUMP, System.IO.Path.ChangeExtension(fname, ".png"));

                using (IEzImage ezImage2 = LoadImage(file))
                {
                    Mat imgScene = ezImage2.Image as Mat;

                    var bestResult = matcher.FindBestMatch(imgScene, dumpFile);

                    if (bestResult != null)
                    {
                        var bestGrid = bestResult.Grid;
                        var box2d = bestResult.CalcBox2D();
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
