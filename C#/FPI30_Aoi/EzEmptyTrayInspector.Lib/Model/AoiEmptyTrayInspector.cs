#region AUTHOR
/*
 * 
 * Copyright (c) 2024 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2024-10-03 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzEmptyTrayInspector.Model.Aoi;
using JetEazy.EzImage;
using JetEazy.Match;
using JetEazy.OpenCV;
using NLog.LayoutRenderers;
using OpenCvSharp;
using System;
using System.Drawing;
using System.Threading;
using System.Windows.Media.Media3D;
using AOI_RESULT = EzEmptyTrayInspector.Model.EzEmptyTrayResult;


namespace EzEmptyTrayInspector.Model
{
    public partial class AoiEmptyTrayInspector : IxEmptyTrayInspector
    {
        #region NLOG
        // NOTE:
        // 不要做靜態初始化，而是要等到窗體的Load事件時才初始化Logger物件，
        // 且保證該窗體是 【首個使用】NLog 的 Class !!!
        // 這是因為NLog是在首次被使用時，才載入配置文件的。
        static NLog.ILogger _logger = null;
        static NLog.ILogger _LOG
        {
            get
            {
                if (_logger == null)
                    _logger = NLog.LogManager.GetCurrentClassLogger();
                return _logger;
            }
        }
        #endregion

        #region CONSTs
        static readonly int N_SIDES = Enum.GetValues(typeof(SideID)).Length - 1;
        #endregion

        public event EventHandler OnError;
        public event EventHandler OnStateChanged;
        public event EventHandler<MatchResultEventArgs> OnMatched;
        public event EventHandler<AoiResultEventArgs> OnFinalResulted;

        #region SINGLETON
        static AoiEmptyTrayInspector _singleton = null;
        protected AoiEmptyTrayInspector()
        {
        }
        int IxCommonModel.ID
        {
            get => 12892414;
        }
        #endregion

        public static AoiEmptyTrayInspector Instance
        {
            get
            {
                if (_singleton == null)
                    _singleton = new AoiEmptyTrayInspector();
                return _singleton;
            }
        }

        public void Dispose()
        {
            _singleton = null;
        }

        #region PRIVATE_STATE_MEMBERS
        string _state = "Ready";
        void changeState(string state, SideID sideId = SideID.A, bool force = false)
        {
            if (state == null)
                state = "Unknown";

            if (_state != state || force)
            {
                _state = state;

                bool isErr = IsError();
                if (!isErr)
                    _LOG.Info("[{0}] {1}", sideId, _state);
                else
                    _LOG.Error("[{0}] {1}", sideId, _state);

                var e = new MatchStateEventArgs(sideId, _state);
                OnStateChanged?.Invoke(this, e);

                if (isErr && OnError != null)
                    OnError.Invoke(this, e);
            }
        }
        #endregion

        public object State
        {
            get => _state;
        }
        public bool IsReady()
        {
            return _state == "Ready";
        }
        public bool IsError()
        {
            return _state.Contains("Error");
        }
        public bool IsSafeToExit()
        {
            return IsReady() || IsError();
        }

        #region ERROR_HANDLER
        void _ERROR(ErrCodes err, SideID sideId = SideID.A, Exception ex = null)
        {
            string msg;
            if (ex == null)
                msg = $"Error : {JetEazy.QxNums.GetEnumDescription(err)}";
            else
                msg = $"Error : {JetEazy.QxNums.GetEnumDescription(err)} : {ex.Message}";
            changeState(msg, sideId);
        }
        #endregion
    }


    partial class AoiEmptyTrayInspector
    {
        #region PRIVATE_DATA
        JxAoiRecipe _recipe;
        #endregion

        #region PRIVATE_RESULT_DATA
        MatchResult[] _matchResults = new MatchResult[N_SIDES];
        AOI_RESULT _finalResult = null;
        #endregion

        #region PRIVATE_RUNTIME_DATA
        string _dumpPath = null;
        #endregion


        public void ResetAndClear(SideID sideId)
        {
            int id = (int)sideId;
            if (0 <= id && id < N_SIDES)
            {
                clear_result(sideId, true);
                if (IsError())
                    changeState("Ready", sideId, force: true);
            }
            else
            {
                bool isErr = IsError();
                for (id = 0; id < N_SIDES; id++)
                {
                    clear_result(sideId, true);
                    if (isErr)
                        changeState("Ready", sideId, force: true);
                }
            }
        }

        public void SetRecipe(JxAoiRecipe recipe)
        {
            if (!IsReady())
                return;

            if (recipe != null)
            {
                _recipe = recipe;
                //_recipe?.Dispose();
                //_recipe = (JxDualMatchRecipe)recipe.Clone();
            }
        }

        public void TryApplyFilters(SideID sideId, IEzImage largeImg, JxRotAngleSettings settings, out object result)
        {
            var finder = new EzRotAngleFinder();
            finder.ApplyFilters(largeImg, settings, true, out var rotRects);
            result = rotRects;
        }

        public bool CropGoldenTemplate(SideID sideId, IEzImage largeImg, Rectangle rect)
        {
            var sideSettings = _recipe.GetSiteSettings((int)sideId);
            if (sideSettings == null)
                return false;

            var matchSettings = sideSettings.Match;
            var golden = ImageUtil.CropBmp(largeImg, rect);
            matchSettings.GoldenBox.Value = rect;
            matchSettings.GoldenBmp.Value = golden;

            try
            {
                var finder = new EzRotAngleFinder() { OptForceFind = true };
                double angleG = finder.ApplyFilters(largeImg, sideSettings.RotAngle, true, out var rotRests);
                matchSettings.GoldenRefAngle.Value = (decimal)angleG;
                return true;
            }
            catch (Exception ex)
            {
                _LOG.Error(ex);
                matchSettings.GoldenRefAngle.Value = (decimal)0;
                return false;
            }
        }


        #region PUBLIC_MATCH_FUNCTIONS
        public ErrCodes CanMatch(SideID sideId, IEzImage largeImg)
        {
            if (_recipe == null)
                return ErrCodes.NO_RECIPE;

            if (largeImg?.Image == null)
                return (ErrCodes)((int)ErrCodes.NO_IMAGE_A + (int)sideId);

            //if (sideId == SideID.A)
            //{
            //    if (ImageUtil.GetCombinedMark(largeImg))
            //        return ErrCodes.HAS_BEEN_COMBINED;
            //}

            return ErrCodes.OK;
        }
        
        public void RunMatch(SideID sideId, IEzImage largeImg, string dumpPath = null)
        {
            // Sync and Blocked

            _dumpPath = dumpPath;

            if (0 <= (int)sideId && (int)sideId < N_SIDES)
            {
                if (largeImg == null)
                {
                    clear_result(sideId, true);
                }
                else if (largeImg.IsOpenCV)
                {
                    changeState("MATCH", sideId);
                    clear_result(sideId, true);
                    run_match(sideId, largeImg.Image as Mat, dumpPath);
                }
                else
                {
                    changeState("MATCH (bmp)", sideId);
                    clear_result(sideId, true);
                    run_match(sideId, largeImg?.Bitmap, dumpPath);
                }
            }
        }
        
        public MatchResult GetMatchResult(SideID sideId)
        {
            return _matchResults[(int)sideId];
        }

        public bool AreAllSidesMatched()
        {
            //>>> return _matchResults[0] != null && _matchResults[1] != null;
            foreach (var result in _matchResults)
                if (result == null)
                    return false;
            return true;
        }
        #endregion


        #region PUBLIC_COMBINE_FUNCTIONS
#if (OPT_RESERVED)
        public EzDualTransform BuildDualTransform()
        {
            if (!AreAllSidesMatched())
                return null;
            try
            {
                _dualTransform = null;
                _dualTransform = new EzDualTransform(_matchResults, _recipe);
                return _dualTransform;
            }
            catch(Exception ex)
            {
                _ERROR(ErrCodes.BUILD_DUAL_TRANSFORM_ERROR, ex: ex);
                return null;
            }
        }
        
        public EzDualTransform GetDualTransform()
        {
            return _dualTransform;
        }
        
        public ErrCodes CanCombine(IEzImage imgA, IEzImage imgB)
        {
            if (_recipe == null)
                return ErrCodes.NO_RECIPE;

            var matA = imgA?.Image as Mat;
            var matB = imgB?.Image as Mat;
            
            if (matA == null)
                return ErrCodes.NO_IMAGE_A;
            if (matB == null)
                return ErrCodes.NO_IMAGE_B;

            if (imgA.Width != imgB.Width || imgA.Height != imgB.Height)
                return ErrCodes.IMAGES_SIZE_NOT_EQUAL;

            if (ImageUtil.GetCombinedMark(imgA))
                return ErrCodes.HAS_BEEN_COMBINED;

            if (_matchResults[0] == null)
                return ErrCodes.NOT_MATCHED_YET_A;
            if (_matchResults[1] == null)
                return ErrCodes.NOT_MATCHED_YET_B;

            if (_matchResults[0].Grid == null)
                return ErrCodes.NO_MATCH_GRID_A;
            if (_matchResults[1].Grid == null)
                return ErrCodes.NO_MATCH_GRID_B;

            return ErrCodes.OK;
        }

        public int Combine(IEzImage largeImgA, IEzImage largeImgB, string outputFileName = null)
        {
            // Sync and Blocked

            var err = CanCombine(largeImgA, largeImgB);
            if (err != ErrCodes.OK)
            {
                _ERROR(err);
                return 0;
            }

            var tm0 = DateTime.Now;
            int count;

            if (largeImgA.IsOpenCV)
            {
                var imgA = largeImgA.Image as Mat;
                var imgB = largeImgB.Image as Mat;

                changeState("COMBINE");
                count = run_combine(largeImgA, imgA, imgB);

                if (count > 0)
                {
                    if (!string.IsNullOrEmpty(outputFileName))
                        create_combined_file(imgA, outputFileName);
                }
            }
            else
            {
                var bmpA = largeImgA.Bitmap;
                var bmpB = largeImgB.Bitmap;
                count = run_combine(largeImgA, bmpA, bmpB);

                if (count > 0)
                {
                    if (!string.IsNullOrEmpty(outputFileName))
                        create_combined_file(bmpA, outputFileName);
                }
            }

            var ts = DateTime.Now - tm0;
            changeState("Ready");

            _dualMatchResult = count > 0 ?
                new EzDualMatchResult(_matchResults, _dualTransform, ts.TotalSeconds, outputFileName) :
                null;
            OnCombined?.Invoke(this, new DualMatchResultEventArgs(_dualMatchResult));

            return count;
        }
#endif
        #endregion


        #region PUBLIC_RUN_ALLS
        public ErrCodes CanRunAll(IEzImage imgA, IEzImage imgB)
        {
            if (_recipe == null)
                return ErrCodes.NO_RECIPE;

            var matA = imgA?.Image as Mat;
            var matB = imgB?.Image as Mat;

            if (matA == null)
                return ErrCodes.NO_IMAGE_A;

#if (OPT_DUAL_MATCH)
            if (matB == null)
                return ErrCodes.NO_IMAGE_B;

            if (imgA.Width != imgB.Width || imgA.Height != imgB.Height)
                return ErrCodes.IMAGES_SIZE_NOT_EQUAL;

            if (ImageUtil.GetCombinedMark(imgA))
                return ErrCodes.HAS_BEEN_COMBINED;
#endif
            return ErrCodes.OK;
        }
        public void RunAll(IEzImage largeImgA, IEzImage largeImgB, string outputFile = null, string dumpPath = null, bool wait = false)
        {
            // Async !!!
            //>>> System.Diagnostics.Debug.Assert(largeImgA != largeImgB);

            var err = CanRunAll(largeImgA, largeImgB);
            if (err != ErrCodes.OK)
            {
                _ERROR(err);
                return;
            }
            _dumpPath = dumpPath;

            changeState("RUN ALL", force: true);
            clear_result(SideID.A, true);
            clear_result(SideID.B, true);

            var evCompleted = wait ? new ManualResetEvent(false) : null;

            ThreadPool.QueueUserWorkItem((arg) =>
            {
                var args = (object[])arg;
                var A = (IEzImage)args[0];
                var B = (IEzImage)args[1];
                var F = (string)args[2];
                run_all(A, A?.Image as Mat, B?.Image as Mat, F);
                evCompleted?.Set();
            },
            new object[] {
                largeImgA,
                largeImgB,
                outputFile
            });

            if (wait)
                evCompleted.WaitOne(1000 * 60 * 5);
        }
        AOI_RESULT IxEmptyTrayInspector.GetResult()
        {
            return _finalResult;
        }
        #endregion


        #region PRIVATE_FUNCTIONS
        void clear_result(SideID sideId, bool notify)
        {
            _finalResult = null;
            _matchResults[(int)sideId] = null;
            if (notify)
                OnMatched?.Invoke(this, new MatchResultEventArgs(sideId, null));
        }
        #endregion


        #region PRIVATE_MATCH_FUNCTIONS
        void run_match(SideID siteId, Bitmap srcBmp, string dumpPath = null)
        {
            if (srcBmp != null)
            {
                using (var bridge = new QxImageBridge(srcBmp))
                {
                    run_match(siteId, bridge.Image, dumpPath);
                }
            }
            else
            {
                run_match(siteId, (Mat)null, dumpPath);
            }
        }
        void run_match(SideID sideId, Mat srcImg, string dumpPath = null)
        {
            MatchResult matchResult = null;
            Exception errEx = null;

            try
            {
                JxTempMatchSettings matchSettings = _recipe.GetMatchSettings((int)sideId);
                if (srcImg == null || matchSettings == null)
                    return;

                var threshold = (double)matchSettings.ScoreThres.Value;
                var goldenBmp = (Bitmap)matchSettings.GoldenBmp.Value;
                if (goldenBmp == null)
                    return;

                double rotateAngle = 0;
                var rotSettings = _recipe.GetSiteSettings((int)sideId).RotAngle;
                if (rotSettings != null && rotSettings.Enabled)
                {
                    changeState("MATCH : finding angle", sideId);
                    var finder = new EzRotAngleFinder();
                    rotateAngle = finder.ApplyFilters(srcImg, rotSettings, true, out var rotRects);
                    _LOG.Info("[{0}] MATCH : angle = {1:0.00}°", sideId, rotateAngle);
                }

                using (var bridge = new QxImageBridge(goldenBmp))
                {
                    var tm0 = DateTime.Now;
                    var goldenImg = bridge.Image;

                    var matcher = new EzTemplateMatcher(EzAoiBaseUtil.GetShrinkFactor(srcImg.Width, srcImg.Height));
                    matcher.SetRecipe(matchSettings);
                    matcher.DumpPath = dumpPath;
                    matcher.OnProgress += (s, e) => _LOG.Info("[{0}] {1}", sideId, e.Message);

                    _LOG.Info("[{0}] Matching", sideId);
                    var blocs = matcher.FindBlocs(srcImg, goldenImg);

                    EzBlocsGrid grid = null;
                    if (matchSettings.UseGrid)
                    {
                        //changeState("Gridding", sideId);
                        _LOG.Info("[{0}] Gridding", sideId);
                        var builder = new EzBlocsGridBuilder();
                        grid = builder.Build(blocs);
                    }

                    var ts = DateTime.Now - tm0;
                    double secs = ts.TotalSeconds;
                    matchResult = new MatchResult((int)sideId, grid, blocs: blocs, totalSeconds: secs);
                    matchResult.RotateAngle = rotateAngle;
                }
            }
            catch (Exception ex)
            {
                errEx = ex;
            }
            finally
            {
                if (errEx != null)
                {
                    _ERROR(ErrCodes.MATCH_ERROR, sideId, errEx);
                }
                else
                {
                    changeState("Ready", sideId);
                    update_one_match_result(sideId, matchResult, notify: true);
                }
            }
        }
        void update_one_match_result(SideID sideId, MatchResult result, bool notify = false)
        {
            _matchResults[(int)sideId] = result;
            if (notify && OnMatched != null)
                OnMatched(this, new MatchResultEventArgs(sideId, result));
        }
        #endregion


        #region PRIVATE_COMBINE_FUNCTIONS
#if(OPT_RESERVED)
        int run_combine(IEzImage largeImgA, Bitmap bmpA, Bitmap bmpB)
        {
            using (var bridgeA = new QxImageBridge(bmpA))
            using (var bridgeB = new QxImageBridge(bmpB))
            {
                return run_combine(largeImgA, bridgeA.Image, bridgeB.Image);
            }
        }
        int run_combine(IEzImage largeImgA, Mat imgA, Mat imgB)
        {
            if (_matchResults == null)
                return 0;
            var gridA = _matchResults[0]?.Grid;
            var gridB = _matchResults[1]?.Grid;
            if (gridA == null || gridA == null)
                return 0;

            // Rotate Angle (此處都用 degree 為單位!)
            double rotAngleD = 0;
            bool rotEnabled = _recipe.Vision.RotAngle.Enabled && _recipe.SideB.RotAngle.Enabled;
            if (rotEnabled)
            {
                double goldenAngle = (double)_recipe.Vision.Match.GoldenRefAngle.Value;
                double angleA = _matchResults[0].RotateAngle - goldenAngle;
                double angleB = _matchResults[1].RotateAngle;
                rotAngleD = angleB - angleA;
            }
            if (Math.Abs(rotAngleD) <= 0.01)
            {
                rotEnabled = false;
                rotAngleD = 0;
            }

            //var tm0 = DateTime.Now;
            int count = 0;
            if (true)
            {
                Mat[] channelsA = null;
                Mat[] channelsB = null;

                //var boundA = new Rect(0, 0, imgA.Width, imgA.Height);
                //var boundB = new Rect(0, 0, imgB.Width, imgB.Height);

                int CH = imgA.Channels();
                if (CH >= 3)
                {
                    channelsA = Cv2.Split(imgA);  // imgA 分為 B, G, R 三個通道
                    channelsB = Cv2.Split(imgB);  // imgB 分為 B, G, R 三個通道
                    imgA = channelsA[2];
                    imgB = channelsB[2];
                }

                void combine_one(int r, int c)
                {
                    var blobA = gridA.Get(r, c);
                    var blobB = gridB.Get(r, c);
                    if (blobA == null || blobB == null)
                        return;

                    int ww = Math.Max(blobA.Rect.Width, blobB.Rect.Width);
                    int hh = Math.Max(blobA.Rect.Height, blobB.Rect.Height);
                    var roiA = JetEazy.Qcvt.CvCreateCenterRect(blobA.CenterX, blobA.CenterY, ww, hh);
                    var roiB = JetEazy.Qcvt.CvCreateCenterRect(blobB.CenterX, blobB.CenterY, ww, hh);
                    var boundA = new Rect(0, 0, imgA.Width, imgA.Height);
                    var boundB = new Rect(0, 0, imgB.Width, imgB.Height);

                    if (JetEazy.Qcvt.ClipBoundary(ref roiA, ref boundA) ||
                        JetEazy.Qcvt.ClipBoundary(ref roiB, ref boundB))
                    {
                        // 切到邊界 暫時不處理 !!!
                        return;
                    }

                    var imgB0 = imgB[roiB];
                    var imgBR = rotEnabled ? rotate_crop(imgB, roiB, rotAngleD) : imgB0;
                    if (imgBR == null)
                        imgBR = imgB0;

                    if (channelsA == null)
                    {
                        // 8-bit Single Channel
                        imgA[roiA] = imgA[roiA] / 3 * 2 + imgBR / 3;
                    }
                    else
                    {
                        // 24-bit 3-Channel
                        imgA[roiA] = imgBR;
                    }

                    // clean up
                    if (imgBR != imgB0)
                        imgBR?.Dispose();
                }

                int rows = gridA.Rows;
                int cols = gridA.Cols;
                int rowDelta = 4;
                int TN = (rows + rowDelta - 1) / rowDelta;
                TN = Math.Min(TN, 8);
                System.Diagnostics.Debug.Assert(TN > 0);

                //changeState($"Combining ({rows}x{cols}) [threads={TN}] [A={rotAngleD:0.00}°]", (int)SideID.A);
                _LOG.Info("Combining ({0}x{1}) [TN={2}] [A={3:0.00}°]", rows, cols, TN, rotAngleD);

                Parallel.For(0, TN, (idx) =>
                //for (int idx = 0; idx < TN; idx++)
                {
                    int rowStart = idx * rowDelta;
                    int rowEnd = (idx == TN - 1) ? rows : rowStart + rowDelta;
                    _LOG.Debug($"Combining [row = {rowStart} to {rowEnd}]");

                    for (int ro = rowStart; ro < rowEnd; ro++)
                    {
                        for (int co = 0; co < cols; co++)
                        {
                            //>>> _LOG.Debug($"Combining [{ro},{co}]");
                            combine_one(ro, co);
                            Interlocked.Increment(ref count);
                        }
                    }
                }
                );

                if (count > 0 && channelsA != null)
                    Cv2.Merge(channelsA, imgA);

                if (count > 0)
                    ImageUtil.SetCombinedMark(largeImgA);

                ImageUtil.MarkDirtyPixel(imgA);
            }
            return count;
        }
        Mat rotate_crop(Mat src, Rect roi, double angleD)
        {
            //// 計算中心點
            //// 建立 RotatedRect，使用 roi 的大小，並旋轉指定角度
            //// 使用 RotatedRect 的 BoundingRect 方法計算外接矩形
            Point2f center = new Point2f(roi.X + roi.Width / 2.0f, roi.Y + roi.Height / 2.0f);
            //RotatedRect rotRect = new RotatedRect(center, new Size2f(roi.Width, roi.Height), (float)angleD);
            //Rect rotBound = rotRect.BoundingRect();
            Rect rotBound = roi;
            // 提取 roiB 區域的子圖像
            Mat subImg = new Mat(src, rotBound);
            // 生成旋轉矩陣
            center = new Point2f(rotBound.Width / 2.0f, rotBound.Height / 2.0f);
            Mat rotationMatrix = Cv2.GetRotationMatrix2D(center, angleD, 1.0);  // 旋轉角度和縮放比例(1.0 表示不縮放)
            // 計算旋轉後的整體圖像大小（可以選擇擴展邊界或裁剪）
            var boundingSize = rotBound.Size;
            // 進行旋轉並儲存到新影像
            Mat rotatedImg = new Mat();
            Cv2.WarpAffine(subImg, rotatedImg, rotationMatrix, boundingSize, InterpolationFlags.Linear, BorderTypes.Constant, Scalar.All(0));
            subImg?.Dispose();
            return rotatedImg;
        }

        void create_combined_file(Bitmap bmp, string fileName)
        {
            if (bmp != null)
            {
                using (var bridge = new QxImageBridge(bmp))
                {
                    create_combined_file(bridge.Image, fileName);
                }
            }
        }
        void create_combined_file(Mat img, string fileName)
        {
            if (img == null || fileName == null)
                return;

            try
            {
                _LOG.Info("[生成合併圖檔]");
                var tm0 = DateTime.Now;

                string path = System.IO.Path.GetDirectoryName(fileName);
                JetEazy.IO.QxPathUtility.InitDirectory(path);

                img.SaveImage(fileName);

                var ts = DateTime.Now - tm0;
                _LOG.Info("[生成合併圖檔完成 {0} ms]", (int)ts.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                _ERROR(ErrCodes.SAVE_COMBINED_FILE_ERROR, ex: ex);
            }
        }
#endif
        #endregion


        #region PRIVATE_RUN_ALL
        void run_all(IEzImage largeImgA, Mat imgA, Mat imgB, string outputFileName)
        {
#if (OPT_DUAL_MATCH)
            _LOG.Info("[一鍵執行] 開始 ... ");
            EzDualMatchResult result = null;

            // 初始化一個 CountdownEvent，計數器設置為 2，表示要等待兩個工作完成
            CountdownEvent countdownEvent = new CountdownEvent(2);
            try
            {
                var tm0 = DateTime.Now;

                bool[] doneFlags = new bool[N_SIDES];
                
                for (int idx = 0; idx < N_SIDES; idx++)
                {
                    // 使用 ThreadPool 啟動線呈
                    ThreadPool.QueueUserWorkItem((arg) =>
                    {
                        var args = (object[])arg;
                        var sideId = (int)args[0];
                        var img = (Mat)args[sideId + 1];

                        run_match((SideID)sideId, img, _dumpPath);
                        
                        doneFlags[sideId] = true;
                        countdownEvent.Signal(); // 完成後減少計數器
                    },
                    new object[]
                    {
                        idx,
                        imgA,
                        imgB,
                    });
                }

                // 等待所有工作完成
                bool ok = countdownEvent.Wait(1000 * 60);

                if (ok)
                {
                    _LOG.Info("[一鍵執行] building transform");
                    _dualTransform = BuildDualTransform();

                    _LOG.Info("[一鍵執行] 合併");
                    int count = run_combine(largeImgA, imgA, imgB);

                    if (count > 0)
                    {
                        if (outputFileName != null)
                            create_combined_file(imgA, outputFileName);

                        var ts = DateTime.Now - tm0;
                        //_LOG.Info("[一鍵執行完成] 總耗時 {0} ms", (int)ts.TotalMilliseconds);
                        result = new EzDualMatchResult(_matchResults, _dualTransform, ts.TotalSeconds, outputFileName);
                    }
                }
                else
                {
                    // 異常
                    for (int id = 0; id < N_SIDES; id++)
                    {
                        if (!doneFlags[id])
                        {
                            _ERROR((ErrCodes)((int)ErrCodes.MATCH_TIMEOUT_A + id), (SideID)id);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _ERROR(ErrCodes.RUN_ALL_EXCEPTION, ex: ex);
            }
            finally
            {
                countdownEvent?.Dispose();
                changeState("Ready");
                _dualMatchResult = result;
                OnAllRunCompleted?.Invoke(this, new DualMatchResultEventArgs(_dualMatchResult));
            }
#endif
            _LOG.Info("[AOI 空盤檢測] 開始 ... ");
            AOI_RESULT result = null;

            try
            {
                var tm0 = DateTime.Now;
                
                run_match(SideID.A, imgA, _dumpPath);

                var ts = DateTime.Now - tm0;

                result = new AOI_RESULT(_matchResults[0], ts.TotalSeconds)
                {
                    FullRows = _recipe.TrayMiscSettings.FullRows,
                    FullCols = _recipe.TrayMiscSettings.FullCols,
                };

                dump_result_image(outputFileName, imgA, result);

            }
            catch (Exception ex)
            {
                _ERROR(ErrCodes.RUN_ALL_EXCEPTION, ex: ex);
            }
            finally
            {
                _finalResult = result;
                changeState("Ready");
                OnFinalResulted?.Invoke(this, new AoiResultEventArgs(_finalResult));
            }
        }
        void dump_result_image(string outputFileName, Mat imgA, EzEmptyTrayResult result)
        {
            // 暫時不支援
            // 原因: 圖形太大, 轉換成 24-bit 太耗資源 !!!
            return;

            try
            {
                if (imgA != null && result != null && !string.IsNullOrEmpty(outputFileName))
                {
                    foreach (var bloc in result.IterSuckerBlocs())
                    {
                        
                    }
                    foreach (var bloc in result.IterAbnormalBlocs())
                    {

                    }
                }
            }
            catch (Exception ex)
            {
                _LOG.Error(ex);
            }
        }
        #endregion
    }
}
