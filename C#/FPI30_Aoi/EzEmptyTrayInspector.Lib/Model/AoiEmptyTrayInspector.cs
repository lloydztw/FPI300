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

using EzAoiEmptyTrayInspector.Model.Aoi;
using JetEazy.EzImage;
using JetEazy.Match;
using JetEazy.OpenCV;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using AOI_RESULT = EzAoiEmptyTrayInspector.Model.EzEmptyTrayResult;
using CvPoint = OpenCvSharp.Point;


namespace EzAoiEmptyTrayInspector.Model
{
    public partial class AoiEmptyTrayInspector : LeTian.JxProps.Ptr.QxPtr, IxEmptyTrayInspector
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

        protected override void OnDisposing()
        {
            clear_golden_grid_cache();
            ReleaseRecipe();
            _singleton = null;
            _LOG.Info($"[AOI Model] {GetType().Name} 卸載!");
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
                    clear_result((SideID)id, true);
                    if (isErr)
                        changeState("Ready", (SideID)id, force: true);
                }
            }
        }

        public void SetRecipe(JxAoiRecipe recipe)
        {
            if (!IsReady())
                return;

            // 強制清理 Clear Cache (暫時)
            clear_golden_grid_cache();

            if (recipe != null)
            {
                if (recipe != _recipe || recipe.Name != _recipe?.Name)
                    _LOG.Debug("[AOI Model] 設定參數 = {0}", recipe.Name);

                //=============================================================================
                // NOTE:
                // 舊版: 上層的 RecipeManager 負責處理 recipe 的生命週期.
                // 目前: 改為 AoiModel 負責 Recipe 生命週期, 以便長時間讓 Gaara 使用 AoiModel
                //=============================================================================
                // _recipe = recipe;
                // return;

                if (_recipe != recipe)
                {
                    var old = _recipe;
                    _recipe = recipe;
                    _recipe?.AddRef();  // AddRef 代表 _recipe 被 AoiModel 持有使用中. 
                    old?.Release();
                }
            }
        }

        void ReleaseRecipe()
        {
            _recipe?.Dispose();
            _recipe = null;
        }


        public void TryApplyFilters(SideID sideId, IEzImage largeImg, JxRotAngleSettings settings, out object result)
        {
            var finder = new EzRotAngleFinder();
            finder.ApplyFilters(largeImg, settings, true, out var rotRects);
            result = rotRects;
        }

        public bool CropGoldenTemplate(SideID sideId, IEzImage largeImg, Rectangle goldenRect)
        {
            var sideSettings = _recipe.GetSiteSettings((int)sideId);
            if (sideSettings == null)
                return false;

            var matchSettings = sideSettings.Match;
            var golden = ImageUtil.CropBmp(largeImg, goldenRect);
            matchSettings.GoldenBox.Value = goldenRect;
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

        public ErrCodes BuildGoldenGridTemplate(SideID sideId, IEzImage largeImg)
        {
            if (!_recipe.VisionSettings.Match.UseGrid.Value)
            {
                _LOG.Warn("Match.UseGrid 沒啟用");
                return ErrCodes.OK;
            }

            var err = CanMatch(sideId, largeImg);
            if (err != ErrCodes.OK)
            {
                return err;
            }

            RunMatch(sideId, largeImg);
            var matchResult = GetMatchResult(sideId);
            var matchGrid = matchResult?.Grid;
            if (matchGrid == null)
            {
                err = ErrCodes.NO_MATCH_GRID;
                return err;
            }

            _LOG.Info("[AOI] 建立 Golden Grid ... ");
            int goldenRowsByUser = _recipe.TrayMiscSettings.FullRows;
            int goldenColsByUser = _recipe.TrayMiscSettings.FullCols;
            int goldenRows = matchGrid.Rows;
            int goldenCols = matchGrid.Cols;

            // 防呆檢查 
            if (goldenRowsByUser != goldenRows || goldenColsByUser != goldenCols)
            {
                err = ErrCodes.GRID_ROWS_COLS_ARE_NOT_THE_SAME_AS_USER_INPUT;
            }

            if (err == ErrCodes.OK)
            {
                // 更新到 Recipe
                _recipe.TrayMiscSettings.SetGoldenGrid(matchGrid);
                _recipe.TrayMiscSettings.FovWidth.Value = largeImg.Width;
                _recipe.TrayMiscSettings.FovHeight.Value = largeImg.Height;

                // large Golden Grid image
                _largeGoldenGridImage?.Dispose();
                _largeGoldenGridImage = rebuild_golden_grid_image();
                _DUMP_GOLDEN_GRID_IMAGE(_largeGoldenGridImage, null);
            }

            ResetAndClear(sideId);      //@<<< BuildGoldenGridTemplate
            return err;
        }


        #region PUBLIC_MATCH_FUNCTIONS
        public ErrCodes CanMatch(SideID sideId, IEzImage largeImg)
        {
            if (_recipe == null)
                return ErrCodes.NO_RECIPE;

            if (largeImg?.Image == null)
                return ErrCodes.NO_IMAGE;

#if (OPT_DUAL_MATCH)
            if (largeImg?.Image == null)
                return (ErrCodes)((int)ErrCodes.NO_IMAGE_A + (int)sideId);

            if (sideId == SideID.A)
            {
                if (ImageUtil.GetCombinedMark(largeImg))
                    return ErrCodes.HAS_BEEN_COMBINED;
            }
#endif
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


        #region PUBLIC_RUN_ALLS_DUAL
        public ErrCodes CanRunAll(IEzImage imgA, IEzImage imgB)
        {
            if (_recipe == null)
                return ErrCodes.NO_RECIPE;

            var matA = imgA?.Image as Mat;
            if (matA == null)
                return ErrCodes.NO_IMAGE;

#if (OPT_DUAL_MATCH)
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
        #endregion


        public void RunAll(Bitmap largeBmp, bool wait = false)
        {
            //// Async !!!
            ////>>> System.Diagnostics.Debug.Assert(largeImgA != largeImgB);

            //var err = CanRunAll(largeImgA, largeImgB);
            //if (err != ErrCodes.OK)
            //{
            //    _ERROR(err);
            //    return;
            //}
            //_dumpPath = dumpPath;

            if (largeBmp == null)
            {
                _ERROR(ErrCodes.NO_IMAGE);
                return;
            }

            changeState("RUN ALL (BMP)", force: true);
            clear_result(SideID.A, true);
            clear_result(SideID.B, true);

            var evCompleted = wait ? new ManualResetEvent(false) : null;

            ThreadPool.QueueUserWorkItem((arg) =>
            {
                var args = (object[])arg;
                var bmp = (Bitmap)args[0];
                //var A = (IEzImage)args[0];
                //var B = (IEzImage)args[1];
                //var F = (string)args[2];
                //run_all(A, A?.Image as Mat, B?.Image as Mat, F);

                using (var bridge = new QxImageBridge(bmp))
                {
                    run_all(null, bridge.Image, null, null);
                }

                evCompleted?.Set();
            },
            new object[] {
                largeBmp,
            });

            if (wait)
                evCompleted.WaitOne(1000 * 60 * 5);
        }
        public AOI_RESULT GetResult()
        {
            return _finalResult;
        }


        #region PRIVATE_FUNCTIONS
        void clear_result(SideID sideId, bool notify)
        {
            _finalResult = null;

            if ((int)sideId < _matchResults.Length)
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

                double globalRotAngle = 0;
                if (false)
                {
                    var rotSettings = _recipe.GetSiteSettings((int)sideId).RotAngle;
                    if (rotSettings != null && rotSettings.Enabled)
                    {
                        changeState("MATCH : finding angle", sideId);
                        var finder = new EzRotAngleFinder();
                        globalRotAngle = finder.ApplyFilters(srcImg, rotSettings, true, out var rotRects);
                        _LOG.Info("[{0}] MATCH : angle = {1:0.00}°", sideId, globalRotAngle);
                    }
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
                    matchResult.RotateAngle = globalRotAngle;
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

                    // For 空盤檢測: 仍然回報 空的 MatchResult
                    if (matchResult == null)
                        matchResult = new MatchResult((int)sideId, null, new List<EzBloc>(), 0);
                    changeState("Ready", sideId);
                    update_one_match_result(sideId, matchResult, notify: true);
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


        #region PRIVATE_POST_PREDICT_GRID_NG_BLOCs
        Mat _largeGoldenGridImage;
        void run_on_grid_ng_predict(SideID sideId, Mat srcImg, MatchResult matchResult, string dumpPath = null, bool force = false)
        {
            Exception errEx = null;

            try
            {
                changeState("POST Grid NG Matching", sideId);

                // Null Condition
                if (srcImg == null || _recipe == null || matchResult == null)
                    return;

                // FULL rows and cols
                var fullRows = _recipe.TrayMiscSettings.FullRows;
                var fullCols = _recipe.TrayMiscSettings.FullCols;

                // INPUT GRID 檢查是否已經滿盤定位
                var inputGrid = matchResult.Grid;
                if (inputGrid != null && inputGrid.Rows >= fullRows && inputGrid.Cols >= fullCols && !force)
                    return;

                // NOTE: goldenGrid 是由 recipe runtime deSerialize 
                var goldenGrid = _recipe.TrayMiscSettings.GetGoldenGrid();
                if (goldenGrid == null)
                    return;
                _LOG.Info($"GoldenGrid = {goldenGrid.Rows}x{goldenGrid.Cols}");

                // LARGE GOLDEN GRID IMAGE (rebuilt from recipe)
                if (_largeGoldenGridImage == null)
                    _largeGoldenGridImage = rebuild_golden_grid_image();
                if (_largeGoldenGridImage == null)
                {
                    _LOG.Warn("[AOI] largetGoldenGridImage 無重建!");
                    return;
                }
                _DUMP_GOLDEN_GRID_IMAGE(_largeGoldenGridImage, goldenGrid, dumpPath);

                // OFFSET 
                int offset_x = 0;
                int offset_y = 0;
                if (inputGrid != null)
                    find_golden_grid_offset(srcImg, _largeGoldenGridImage, goldenGrid, inputGrid, out offset_x, out offset_y);

                // PSEUDO BLOCs (built by goldGrid + offset)
                var pseudoBlocs = new List<EzBloc>();
                foreach (var b in goldenGrid.IterBlocs())
                {
                    if (b != null)
                    {
                        var rect = b.Rect;
                        rect.Offset(offset_x, offset_y);
                        var bloc = new EzBloc(rect, score: SCORES.NG_PSEUDO);
                        pseudoBlocs.Add(bloc);
                    }
                }

                // EXISTING BLOCs (排除 null condition)
                var existingBlocs = matchResult.Blocs;
                if (existingBlocs == null)
                    existingBlocs = matchResult.Blocs = new List<EzBloc>();

                // PSEUDO BLOCs (remove overlaps)
                pseudoBlocs.RemoveAll(new Predicate<EzBloc>((pseudo) =>
                {
                    foreach (var bloc in existingBlocs)
                    {
                        if (bloc != null && bloc.Rect.Contains(pseudo.CenterX, pseudo.CenterY))
                            return true;
                    }
                    return false;
                }));
                pseudoBlocs.AddRange(existingBlocs);

                // 重建 grid
                var builder = new EzBlocsGridBuilder();
                var newGrid = builder.Build(pseudoBlocs);
                if (newGrid != null)
                {
                    foreach (var b in newGrid.IterBlocs())
                    {
                        if (b != null && b.Score <= SCORES.NG_PSEUDO)
                            b.Tag = "NG_PSEUDO";
                    }
                }

                // UPDATE to existing matchResult
                matchResult.Grid = newGrid;
            }
            catch (Exception ex)
            {
                errEx = ex;
            }
            finally
            {
                if (errEx != null)
                {
                    _ERROR(ErrCodes.ON_GRID_TEMPLATE_MATCH_ERROR, sideId, errEx);
                }
                else
                {
                    changeState("Ready", sideId);
                    update_one_match_result(sideId, matchResult, notify: true);
                }
            }
        }
        void find_golden_grid_offset(Mat srcImg, Mat goldenGridImage, EzBlocsGrid goldenGrid, EzBlocsGrid inputGrid, out int offset_x, out int offset_y)
        {
            // LARGE GOLDEN TEMPLATE
            var ggRect = JetEazy.Qcvt.CV(goldenGrid.GetBoundary());
            var ggCenter = JetEazy.Qcvt.Center(ref ggRect);
            var ggTemplate = goldenGridImage[ggRect];

            // MATCH
            int shrink = EzAoiBaseUtil.GetShrinkFactor(srcImg.Width, srcImg.Height);
            var matcher = new EzTemplateMatcher(shrink);
            var settings = new JxTempMatchSettings();
            settings.ScoreThres.Value = 0.2m;
            matcher.SetRecipe(settings);

            // SEARCHING points
            var bestBloc = matcher.FindBestBloc(srcImg, ggTemplate, iterate_possible_offsets(goldenGrid, inputGrid));
            if (bestBloc != null && bestBloc.Score > 0.01)
            {
                var newCenter = bestBloc.Center;
                offset_x = (int)(newCenter.X - ggCenter.X);
                offset_y = (int)(newCenter.Y - ggCenter.Y);
            }
            else
            {
                offset_x = 0;
                offset_y = 0;
            }
        }
        IEnumerable<CvPoint> iterate_possible_offsets_000(EzBlocsGrid goldenGrid, EzBlocsGrid inputGrid)
        {
            int tryCount = 0;
            foreach (var ib in inputGrid.IterBlocs())
            {
                if(ib == null) continue;
                foreach(var gb in goldenGrid.IterBlocs())
                {
                    if(gb == null) continue;
                    var offset = ib.Center - gb.Center;
                    int left = (int)(ib.Rect.Left - offset.X);
                    int top = (int)(ib.Rect.Top - offset.Y);
                    yield return new CvPoint(left, top);
                }
                if (++tryCount >= 1000)
                    break;
            }
        }
        IEnumerable<CvPoint> iterate_possible_offsets_001(EzBlocsGrid goldenGrid, EzBlocsGrid inputGrid)
        {
            int fovWidth = _recipe.TrayMiscSettings.FovWidth;
            int fovHeight = _recipe.TrayMiscSettings.FovHeight;
            var boundary = new Rect(0, 0, fovWidth, fovHeight);
            var roi = JetEazy.Qcvt.CV(goldenGrid.GetBoundary());

            int tryCount = 0;
            foreach (var ib in inputGrid.IterBlocs())
            {
                if (ib == null) continue;
                foreach (var gb in goldenGrid.IterBlocs())
                {
                    if (gb == null) continue;
                    var offset = ib.Center - gb.Center;
                    roi.Left = (int)(ib.Rect.Left - offset.X);
                    roi.Top = (int)(ib.Rect.Top - offset.Y);
                    if (boundary.Contains(roi))
                        yield return roi.TopLeft;
                }
                if (++tryCount >= 1000)
                    break;
            }
        }
        IEnumerable<CvPoint> iterate_possible_offsets(EzBlocsGrid goldenGrid, EzBlocsGrid inputGrid)
        {
            int deltaRows = goldenGrid.Rows - inputGrid.Rows;
            int deltaCols = goldenGrid.Cols - inputGrid.Cols;
            for (int dRow = 0; dRow < deltaRows; dRow++)
            {
                for (int dCol = 0; dCol < deltaCols; dCol++)
                {
                    for (int inRow = 0; inRow < inputGrid.Rows; inRow++)
                    {
                        for (int inCol = 0; inCol < inputGrid.Cols; inCol++)
                        {
                            var inBloc = inputGrid.Get(inRow, inCol);
                            if (inBloc == null) continue;

                            int ggRow = inRow + dRow;
                            int ggCol = inCol + dCol;
                            var ggBloc = goldenGrid.Get(inRow, ggCol);
                            if (ggBloc == null) continue;

                            var offset = inBloc.Center - ggBloc.Center;
                            int left = (int)(inBloc.Rect.Left - offset.X);
                            int top = (int)(inBloc.Rect.Top - offset.Y);
                            yield return new CvPoint(left, top);
                        }
                    }
                }
            }
        }
        Mat rebuild_golden_grid_image(bool useColorFill = false)
        {
            if (_recipe == null)
                return null;

            var goldenGrid = _recipe.TrayMiscSettings.GetGoldenGrid();
            if (goldenGrid == null)
                return null;

            var W = _recipe.TrayMiscSettings.FovWidth;
            var H = _recipe.TrayMiscSettings.FovHeight;
            if (W < 10 || H < 10)
                return null;

            var largeGG = new Mat(H, W, MatType.CV_8UC1);
            largeGG.SetTo(Scalar.White);

            var goldenBmp = _recipe.VisionSettings.Match.GoldenBmp.Value as Bitmap;
            if (goldenBmp == null || useColorFill)
            {
                foreach (var bloc in goldenGrid.IterBlocs())
                {
                    if (bloc != null)
                    {
                        var rc = JetEazy.Qcvt.CV(bloc.Rect);
                        rc.Inflate(-8, -8);
                        largeGG.Rectangle(rc, Scalar.Gray, -1);
                    }
                }
            }
            else
            {
                using (var bridge = new QxImageBridge(goldenBmp))
                {
                    var goldenImg = bridge.Image;
                    int gw = goldenBmp.Width;
                    int gh = goldenBmp.Height;

                    foreach (var bloc in goldenGrid.IterBlocs())
                    {
                        if (bloc != null)
                        {
                            int x = bloc.CenterX - gw / 2;
                            int y = bloc.CenterY - gh / 2;
                            int x2 = x + gw;
                            int y2 = y + gh;
                            x = Math.Max(x, 0);
                            y = Math.Max(y, 0);
                            x2 = Math.Min(x2, W);
                            y2 = Math.Min(y2, H);
                            int ww = x2 - x;
                            int hh = y2 - y;
                            largeGG[y, y2, x, x2] = goldenImg[0, hh, 0, ww];
                        }
                    }
                }
            }

            //var ggBoundary = JetEazy.Qcvt.CV(goldenGrid.GetBoundary());
            //largeGG.Rectangle(ggBoundary, Scalar.Black, 5);

            return largeGG;
        }
        void clear_golden_grid_cache()
        {
            _recipe?.TrayMiscSettings.GetGoldenGrid(true);
            _largeGoldenGridImage?.Dispose();
            _largeGoldenGridImage = null;
        }
        void _DUMP_GOLDEN_GRID_IMAGE(Mat largeGoldenGridImage, EzBlocsGrid goldenGrid, string dumpPath = null)
        {
            if (largeGoldenGridImage == null)
                return;

            if (dumpPath == null)
                return;

            JetEazy.IO.QxPathUtility.InitDirectory(dumpPath);

            largeGoldenGridImage.SaveImage($"{dumpPath}\\large_golden_grid.jpg");

            if (goldenGrid != null)
            {
                var roi = JetEazy.Qcvt.CV(goldenGrid.GetBoundary());
                largeGoldenGridImage[roi].SaveImage($"{dumpPath}\\large_golden_grid_template.jpg");
            }
        }
        #endregion


        #region POST_FIND_OUT_GRID_NG_BLOCs
        void run_off_grid_ng_detect(SideID sideId, Mat srcImg, MatchResult matchResult, string dumpPath, out List<EzBloc> ngBlocs)
        {
            ngBlocs = null;
            Exception errEx = null;

            try
            {
                var matchedGrid = matchResult?.Grid;

                // Null Condition
                if (srcImg == null || _recipe == null)
                    return;

                changeState("檢測 OutGrid NG Blocs", sideId);

                if (matchedGrid == null)
                {
                    _LOG.Warn("matchedGrid is NULL !");
                    return;
                }

                var detector = new EzOutGridNgBlocsDetector(EzAoiBaseUtil.GetShrinkFactor(srcImg));
                
                detector.SetRecipe(_recipe);

                ngBlocs = detector.FindNgBlocs(srcImg, matchedGrid);
            }
            catch (Exception ex)
            {
                errEx = ex;
            }
            finally
            {
                if (errEx != null)
                {
                    _ERROR(ErrCodes.OUT_GRID_NG_BLOCS_DETECT_ERROR, sideId, errEx);
                }
                else
                {
                    changeState("Ready", sideId);
                    update_one_match_result(sideId, matchResult, notify: true);
                }
            }
        }
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
            AOI_RESULT finalResult = null;

            try
            {
                var tm0 = DateTime.Now;
                
                run_match(SideID.A, imgA, _dumpPath);

                var matchResult = _matchResults[0];
                if (matchResult != null && _recipe.VisionSettings.FindAllFailBlocs.Value && _recipe.VisionSettings.Match.UseGrid)
                {
                    run_on_grid_ng_predict(SideID.A, imgA, matchResult, _dumpPath);
                    run_off_grid_ng_detect(SideID.A, imgA, matchResult, _dumpPath, out List<EzBloc> ngBloc);
                    matchResult.OutGridBlocs = ngBloc;
                }

                var ts = DateTime.Now - tm0;

                finalResult = new AOI_RESULT(matchResult, ts.TotalSeconds)
                {
                    FullRows = _recipe.TrayMiscSettings.FullRows,
                    FullCols = _recipe.TrayMiscSettings.FullCols,
                };

                dump_result_image(outputFileName, imgA, finalResult);

            }
            catch (Exception ex)
            {
                _ERROR(ErrCodes.RUN_ALL_EXCEPTION, ex: ex);
            }
            finally
            {
                _finalResult = finalResult;
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
