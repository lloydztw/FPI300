#region AUTHOR
/*
 * 
 * Copyright (c) 2024 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2024-10-19 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using OpenCvSharp;
using System;
using System.Collections.Generic;


namespace EzAoiChipLocQC.Model.Aoi
{
    public abstract class EzAoiBase
    {
        public event EventHandler<ProgressEventArgs> OnProgress;

        #region NLOG
        // NOTE:
        // 不要做靜態初始化，而是要等到窗體的Load事件時才初始化Logger物件，
        // 且保證該窗體是 【首個使用】NLog 的 Class !!!
        // 這是因為NLog是在首次被使用時，才載入配置文件的。
        static NLog.ILogger _logger = null;
        protected static NLog.ILogger _LOG
        {
            get
            {
                if (_logger == null)
                    _logger = NLog.LogManager.GetCurrentClassLogger();
                return _logger;
            }
        }
        #endregion

        #region PROTECTED_DATA
        protected int _shrinkFactor = 1;
        protected bool _usingBlur = true;
        #endregion

        public int ShrinkFactor
        {
            get { return _shrinkFactor; }
            set { _shrinkFactor = Math.Max(1, value); }
        }

        #region PROTECTED_FUNCTIONS
        protected void normalizeGolden(Mat golden, Mat image, out Mat newGolden, List<IDisposable> garbagesCan)
        {
            // 注意: 使用 CvtColor 會影響比對 !!!
            if (golden.Channels() > image.Channels())
            {
                _NOTIFY("[Warning] golden 與 image 格式不一致!");
                Mat[] oldArr = golden.Split();
                Mat[] newArr = new Mat[image.Channels()];
                Array.Copy(oldArr, newArr, newArr.Length);
                newGolden = new Mat();
                Cv2.Merge(newArr, newGolden);
                garbagesCan.Add(newGolden);
            }
            else
            {
                newGolden = golden;
            }

            if (newGolden.Channels() != image.Channels())
                throw new Exception("golden 與 image 格式不一致!");
        }
        protected void apply_shrink(Mat image, out Mat newImage, List<IDisposable> garbagesCan)
        {
            //var interpo = InterpolationFlags.Nearest;
            var interpo = InterpolationFlags.Linear;

            //newGolden = golden;
            newImage = image;

            if (_shrinkFactor > 1)
            {
                _NOTIFY("Factoring");
                var size = image.Size();
                size.Width /= _shrinkFactor;
                size.Height /= _shrinkFactor;
                if (size.Width > 2 && size.Height > 2)
                {
                    newImage = image.Resize(size, 0, 0, interpo);
                    garbagesCan.Add(newImage);
                }
                else
                {
                    _shrinkFactor = 1;
                }
            }

            _DUMP_SHRINK(image, null);
        }
        protected void apply_shrink(Mat golden, Mat image, out Mat newGolden, out Mat newImage, List<IDisposable> garbagesCan)
        {
            //var interpo = InterpolationFlags.Nearest;
            var interpo = InterpolationFlags.Linear;

            newGolden = golden;
            newImage = image;

            if (_shrinkFactor > 1)
            {
                _NOTIFY("Factoring");
                var size = golden.Size();
                size.Width /= _shrinkFactor;
                size.Height /= _shrinkFactor;
                if (size.Width > 2 && size.Height > 2)
                {
                    newGolden = golden.Resize(size, 0, 0, interpo);
                    garbagesCan.Add(newGolden);

                    size = image.Size();
                    size.Width /= _shrinkFactor;
                    size.Height /= _shrinkFactor;
                    newImage = image.Resize(size, 0, 0, interpo);
                    garbagesCan.Add(newImage);
                }
                else
                {
                    _shrinkFactor = 1;
                }
            }

            _DUMP_SHRINK(image, golden);
        }
        protected void apply_filters(Mat golden, Mat image, out Mat newGolden, out Mat newImage, List<IDisposable> garbagesCan)
        {
            if (_usingBlur)
            {
                _NOTIFY("Filtering");
                newGolden = apply_filters(golden);
                newImage = apply_filters(image);
                garbagesCan.Add(newGolden);
                garbagesCan.Add(newImage);
                _DUMP_FILTERED(image, golden);
            }
            else
            {
                newGolden = golden;
                newImage = image;
            }
        }
        protected Mat apply_filters(Mat src)
        {
            //return src.MedianBlur(7);
            //return src.GaussianBlur(sz, 1.0);

            var sz = new OpenCvSharp.Size(3, 3);
            return src.Blur(sz);

            //var sz = new OpenCvSharp.Size(3, 3);
            //Mat tmp = new Mat();
            //Mat tmp2 = new Mat();
            //Cv2.Blur(src, tmp, sz);
            //Cv2.Canny(tmp, tmp2, 100, 300);
            //tmp.Dispose();
            //return tmp2;
        }
        #endregion

        public string DumpPath
        {
            get
            {
                return _dumpPath;
            }
            set
            {
                _DUMP_INIT(value);
            }
        }

        #region EVENT_FUNCTIONS
        protected void _NOTIFY(string message)
        {
            OnProgress?.Invoke(this, new ProgressEventArgs(message));
        }
        #endregion

        #region DUMP_FUNCTIONS
        bool _isDumpEnabled = false;
        string _dumpPath = null;
        protected void _DUMP_INIT(string pathStem)
        {
            _dumpPath = pathStem;
            _isDumpEnabled = pathStem != null;
        }
        protected void _DUMP_SHRINK(Mat image, Mat golden)
        {
            if (_isDumpEnabled) // && !BmpUtil.IsLarge(image))
            {
                //image?.SaveImage(System.IO.Path.Combine(_dumpPathStem, "match_image_src.png"));
                //golden?.SaveImage(System.IO.Path.Combine(_dumpPathStem, "match_golden.png"));
                _DUMP(image, "_1-shrink-image.png");
                _DUMP(golden, "_1-shrink-golden.png");
            }
        }
        protected void _DUMP_FILTERED(Mat image, Mat golden)
        {
            if (_isDumpEnabled) // && !BmpUtil.IsLarge(image))
            {
                _DUMP(image, "_2-filter-image.png");
                _DUMP(golden, "_2-filter-golden.png");
            }
        }
        protected void _DUMP_RUNTIME_GOLDEN(Mat golden)
        {
            if (_isDumpEnabled)
            {
                _DUMP(golden, "_3-runtime-golden.png");
            }
        }
        protected void _DUMP_SIMULARITY(Mat simularity)
        {
            if (_isDumpEnabled)
            {
                _DUMP(simularity, "_4-simulariy.png");
            }
        }
        protected void _DUMP_CC_BLOBs(ConnectedComponents cc)
        {
            if (_isDumpEnabled && cc != null)
            {
                using (Mat canvas = new Mat())
                {
                    cc.RenderBlobs(canvas);
                    //canvas.SaveImage(System.IO.Path.Combine(_dumpPathStem, "match_blobs.png"));
                    _DUMP(canvas, "_5-ccBlobs.png");
                }
            }
        }
        protected void _DUMP(Mat image, string dumpFile)
        {
            try
            {
                if (image == null)
                    return;
                JetEazy.IO.QxPathUtility.InitDirectory(_dumpPath);
                string stem = System.IO.Path.GetFileName(_dumpPath);
                string size_tag = $"-{image.Width}x{image.Height}.png";
                dumpFile = System.IO.Path.Combine(_dumpPath, stem + dumpFile + size_tag);
                image?.SaveImage(dumpFile);
            }
            catch
            {
                _isDumpEnabled = false;
            }
        }
        #endregion
    }
}
