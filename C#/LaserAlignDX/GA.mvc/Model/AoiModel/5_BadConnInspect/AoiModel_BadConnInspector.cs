#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-08-01 開始重整 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzAoiEmptyTrayInspector.Model.Aoi;
using JetEazy.OpenCV;
using JetEazy.QMath;
using JetEazy.QvMath;
using JetEazy.Utils;
using LaserAlignDX.OPSpace;
using LaserAlignDX.OPSpace.RecipeSpace;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using Traveller106;

namespace LaserAlignDX.AoiModel.V3
{
    /// <summary>
    /// 連襟檢測
    /// </summary>
    public class AoiModel_BadConnInpector : AoiModelBase, IAoiBadConnInspector
    {
        #region CONFIG
        static bool N_THREADS_ENABLED => GlobalConfig.N_THREADS_ENABLED;
        static int N_THREADS => GlobalConfig.N_THREADS;
        #endregion

        #region GLOBAL_MESS
        InspectX3ParaClass _xInspect => base._xRecipe.InspectParams;
        #endregion

        #region KERNEL_MEMBERS
        //MvdDefectDetector[] _detectors = new MvdDefectDetector[0];
        #endregion

        #region RUNTIME_DATA
        GaCellsGroup[] _cellGroups;
        #endregion

        public override void Dispose()
        {
            //var oldItems = _detectors;
            //_detectors = new MvdDefectDetector[0];
            //foreach(var item in oldItems)
            //    item?.Dispose();
        }

        public void SetCellGroups(GaCellsGroup[] cellGroups)
        {
            this._cellGroups = cellGroups;
        }

        public override void Run(Bitmap sceneBmp = null)
        {
            bool go = _xInspect.BadConnsCount > 0;
            if (!go)
                return;

            try
            {
                fire_AoiBegin();
                markRunStart();

                Bitmap bmpFullfov = LineScanCamImageHolder.PeekBitmap();
                using (var bmpWork = CreateMaskedBitmap(bmpFullfov))
                {
                    RunBadConnsInspection(bmpWork);
                }

                markRunEnd(true);
                fire_AoiEnd();
            }
            catch (Exception ex)
            {
                // 2025-08-28 LETIAN: 巨圖統一由 LineScanCamImageHolder 管理其生命週期
                // 在此無需釋放 巨圖
                base.HandleAoiException(ex);
            }
        }

        /// <summary>
        /// 參數調試用
        /// </summary>
        public bool TryApplyBadConnFilters(Bitmap bmpRegionTemplate, out Bitmap bmpDisp, IEnumerable<RectangleF> rects)
        {
            try
            {
                if (bmpRegionTemplate != null)
                {
                    using (var bridge = new QxImageBridge(bmpRegionTemplate))
                    {
                        var roiRects = Array.ConvertAll(_xInspect.BadConnsRects.ToArray(),
                                        rc => JetEazy.Qcvt.CV(Rectangle.Round(rc)));

                        var imgDisp = applyFilters(bridge.Image, roiRects, out var _, true);
                        bmpDisp = imgDisp?.ToBitmap();
                        return true;
                    }
                }
                bmpDisp = null;
                return false;
            }
            catch (Exception ex)
            {
                _LOG_ERROR(ex, $"{GetType().Name} TryApplyBadConnFilters 異常");
                bmpDisp = null;
                throw;
            }
        }

        #region PRIVATE_INSPECTION_FUNCTIONS
        private void prepareDetectors(int NThreads)
        {
            //if (NThreads > _detectors.Length)
            //{
            //    var lst = new List<MvdDefectDetector>(_detectors);
            //    for (int i = _detectors.Length; i < NThreads; i++)
            //    {
            //        lst.Add(new MvdDefectDetector());
            //    }
            //    _detectors = lst.ToArray();
            //}
            //foreach (var detector in _detectors)
            //    detector?.Init();
        }

        private Bitmap CreateMaskedBitmap(Bitmap bmpFullfov, int morphs = 16)
        {
            Bitmap bmpWork = (Bitmap)bmpFullfov.Clone();
            using (var bridge = new QxImageBridge(bmpWork))
            using (var bridgeSrc = new QxImageBridge(bmpFullfov))
            using (var collections = new RegionCellsDataCollection(_xRecipe.xRegionCells))
            {
                var imgWork = bridge.Image;
                imgWork.SetTo(Scalar.White);

                foreach (var cell in collections.IterFinalCells())
                {
                    var quad = cell?.ChipData?.ChipQuad2D;
                    if (quad == null) continue;
                    var pts = Array.ConvertAll(quad.Corners, c => new OpenCvSharp.Point((int)c.X, (int)c.Y));
                    imgWork.FillPoly(new[] { pts }, Scalar.Black);
                }

                Cv2.Erode(imgWork, imgWork, null, null, morphs);
                Cv2.BitwiseAnd(bridgeSrc.Image, imgWork, imgWork);
            }
            return bmpWork;
        }

        /// <summary>
        /// 全域 連襟檢測
        /// </summary>
        private void RunBadConnsInspection(Bitmap bmpFullfov)
        {
            //if (!_xInspect.optChipDefectsInspect)
            //    return;

            fire_AoiBegin("連襟檢測");

            // 暫時強制使用 single thread
            bool usingMultiThread = N_THREADS_ENABLED;

            #region 準備_CELL_GROUPS
            int N_GROUPS = _cellGroups != null ? _cellGroups.Length : N_THREADS;
            var groups = _cellGroups != null ? _cellGroups : GaCellsGroup.CollectGroups(N_GROUPS, _xRecipe, bmpFullfov);
            if (groups == null || groups.Length == 0)
                return;
            prepareDetectors(groups.Length);
            #endregion

            if (!usingMultiThread)
            {
                // 單線程 (驗證用)
                for (int gid = 0; gid < groups.Length; gid++)
                {
                    RunGroupBadConnectInspectionOneT(gid, groups[gid]);
                }
            }
            else
            {
                // 多線程
                Parallel.For(0, groups.Length, gid =>
                {
                    RunGroupBadConnectInspectionOneT(gid, groups[gid]);
                });
            }

            #region CLEAN_UP
            if (groups != _cellGroups)
            {
                GaCellsGroup.DisposeAll(groups);
            }
            #endregion
        }

        /// <summary>
        /// 連襟檢測 (數群晶粒) (限用於同一線程內)
        /// </summary>
        private void RunGroupBadConnectInspectionOneT(int threadIdx, GaCellsGroup cellsGroup)
        {
            var fullFovSize = cellsGroup.FullFovRect.Size;

            foreach (var gaCell in cellsGroup)
            {
                RegionCellX3Class cell = gaCell.Cell;
                Bitmap cellBmp = gaCell.CellBmp;
                RectangleF cellRoi = gaCell.CellRoi;

                //(1) 進度條事件
                fire_AoiProgressing(cell);

                //(2.1) 條件
                bool go = cell.ChipData?.ChipQuad2D != null;

                //(2.2) 條件: 是否為 Bypass
                if (cell.ByPass && !INI.Instance.IsForceInspect)
                    go = false;

                //(2.3) 條件: 是否為 空白 或錯 誤區塊
                if (cell.IsEmptyPlaceHold() || cell.IsAmbiguousBloc())
                    go = false;

                if (go)
                {
                    //(3) 單一晶粒 瑕疵檢測
                    RunOneBadConnectInspect(cell, cellBmp, ref cellRoi, threadIdx);
                }
            }
        }

        /// <summary>
        /// 連襟檢測 (量測單一晶粒)
        /// </summary>
        private void RunOneBadConnectInspect(RegionCellX3Class cell, Bitmap cellBmp, ref RectangleF cellRoi, int threadId)
        {
            //(0) 取得 上一輪 晶粒定位 的結果 (chipData)
            var chipData = cell?.ChipData;
            if (chipData == null || chipData.ChipQuad2D == null)
                return;

            //(1) Reset
            chipData.BadConnBlocs = null;

            try
            {
                //(2) 取得檢測框 detectQuads (Cell Region Coordinates)
                var detectQuads = CalcRuntimeDetectQuads(cell, cellRoi, cellBmp);
                if (detectQuads == null || detectQuads.Length == 0)
                    return;

                //(3) 轉換 Quads => Rects (Cell Region Coordinates)
                var roiRects = Array.ConvertAll(detectQuads, q => JetEazy.Qcvt.CV(Rectangle.Round(q.BoundaryRect)));

                //(4) applyFilters
                using (var bridge = new QxImageBridge(cellBmp))
                {
                    var cellImg = bridge.Image;
                    applyFilters(cellImg, roiRects, out var ngQuads, false);

                    // 將 ngRects 從 Cell Region Coordinates 轉換到 Fullfov Coordinates
                    if (ngQuads != null && ngQuads.Count > 0)
                    {
                        // 記入 chipData
                        chipData.BadConnBlocs = ngQuads?.ToArray();

                        // DEBUG
                        if (_isDumpEnabled)
                            _DUMP("BadConnBlocs", cell, cellImg, null, chipData.BadConnBlocs);

                        // Offset
                        foreach(var q in chipData.BadConnBlocs)
                            q.Offset(cellRoi.X, cellRoi.Y);
                    }
                }
            }
            catch (Exception ex)
            {
                _LOG_ERROR(ex, $"{GetType().Name} 連筋檢測異常");
                throw;
            }
        }

        /// <summary>
        /// 計算 選轉 & 平移 後的 檢測框 (Runtime Cell Region Coordindates)
        /// </summary>
        QvQuad2D[] CalcRuntimeDetectQuads(RegionCellX3Class cell, RectangleF cellRoi, Bitmap cellBmp = null)
        {
            int NP = _xInspect.BadConnsCount;
            if (NP == 0)
                return null;

            //(1) 取得 上一輪 晶粒定位 的結果 (chipData) (runtime)
            var chipData = cell?.ChipData;

            //(1.1) 複製 chipQuad (Fullfov Coordinates) (runtime)
            var chipQuad = chipData?.ChipQuad2D?.Clone();
            if (chipQuad == null)
                return null;

            //(1.2) 複製 goldenQuad (Rcp Region Coordinates)
            var goldenQuad = QvQuad2D.From(_xRecipe.GoldenChipRect);
            if (goldenQuad == null)
                return null;

            try
            {
                //(2) 將 chipQuad 從 Fullfov Coordinates 平移到 Cell Region Coordinates
                chipQuad.Offset(-cellRoi.X, -cellRoi.Y);

                //(2.1) 準備 detectQuads (Rcp Region Coordinates)
                var detectQuads = Array.ConvertAll(_xInspect.BadConnsRects.ToArray(), rect => QvQuad2D.From(rect));

                //(2.2) 相對向量 (Rcp Region Coordinates)
                var vectors = Array.ConvertAll(detectQuads, q => q.Center - goldenQuad.Center);

                //(2.3) 把 detectQuads 從  (Rcp Region Coordinates) 轉換到 (Cell Region Coorindates)
                for (int i = 0; i < NP; i++)
                {
                    var pt = chipQuad.Center + vectors[i];
                    detectQuads[i].SetCenter(pt);
                }

                //(2.4) DUMP
                if (_isDumpEnabled)
                    _DUMP("DetectQuads", cell, cellBmp, chipQuad, detectQuads);

                return detectQuads;
            }
            catch (Exception ex)
            {
                _LOG_ERROR(ex, $"{GetType().Name} 無法計算 DetectQuads!");
                throw;
            }
        }

        /// <summary>
        /// 以 imgSrc 左上為 (0,0)
        /// </summary>
        Mat applyFilters(Mat imgSrc, Rect[] roiRects, out List<QvQuad2D> ngBlobQuads, bool generateDispImg = false)
        {
            var gray = GaImageUtil.ToU8(imgSrc);

            var imgDisp = generateDispImg ? new Mat(gray.Size(), MatType.CV_8UC3) : null;
            imgDisp?.SetTo(Scalar.Black);

            if (_xInspect == null)
            {
                ngBlobQuads = null;
                if (gray != imgSrc) 
                    gray.Dispose();
                return imgDisp;
            }

            var boundRect = new Rect(0, 0, imgSrc.Width, imgSrc.Height);
            var threshold = _xInspect.BadConnsThreshold;
            var minX = _xInspect.BadConnsMinX;
            var minY = _xInspect.BadConnsMinY;

            var blobFinder = new EzBlobFinder();
            blobFinder.MinSize = new OpenCvSharp.Size(minX, minY);
            blobFinder.OptFillBorder = false;

            // 收集 ngBlobQuads
            ngBlobQuads = new List<QvQuad2D>();
            for (int i = 0, N = roiRects.Length; i < N; i++)
            {
                var roi = roiRects[i];

                JetEazy.Qcvt.ClipBoundary(ref roi, ref boundRect);
                if (roi.Width < 5 || roi.Height < 5)
                    continue;

                // 使用 using 包覆 SubMat 切片，避免 Native 記憶體洩漏
                using (Mat binary = new Mat())
                using (Mat grayRoi = gray[roi])
                using (Mat dispRoi = imgDisp?[roi])
                {
                    // 1. Filters
                    Cv2.Threshold(grayRoi, binary, threshold, 255, ThresholdTypes.Binary);

                    // 2. 先將 disp 灰階影像轉成 BGR 底圖
                    if (dispRoi != null)
                        Cv2.CvtColor(grayRoi, dispRoi, ColorConversionCodes.GRAY2BGR);

                    // 3. Blobs
                    blobFinder.FindWhiteBlobs(binary, out var blocs);

                    if (blocs != null && blocs.Count > 0)
                    {
                        // 3.1 將異常區塊 塗紅
                        dispRoi?.SetTo(Scalar.Red, binary);

                        // 3.2 收集 blob
                        foreach (var b in blocs)
                        {
                            var ngQuad = QvQuad2D.From(b.Rect);

                            // 從 roi coordinates 轉換到 imgSrc coordinates
                            ngQuad.Offset(roi.X, roi.Y);
                            ngBlobQuads.Add(ngQuad);
                        }
                    }
                }
            }

            if (gray != imgSrc)
                gray.Dispose();

            if (ngBlobQuads.Count == 0)
                ngBlobQuads = null;

            return imgDisp;
        }
        #endregion

        #region PRIVATE_HELPER_FUNCTIONS
        Point2f[] getCorners(RectangleF rect)
        {
            return new[]
            {
                new Point2f(rect.Left, rect.Top),
                new Point2f(rect.Right, rect.Top),
                new Point2f(rect.Right, rect.Bottom),
                new Point2f(rect.Left, rect.Bottom),
            };
        }
        #endregion

        #region DEBUG_FUNCTIONS
        bool _isDumpEnabled = false;
        void _DUMP(string tag, RegionCellX3Class cell, Bitmap cellBmp, QvQuad2D chipQuad, QvQuad2D[] detectQuads)
        {
            if (cellBmp != null)
            {
                using (var bridge = new QxImageBridge(cellBmp))
                {
                    _DUMP(tag, cell, bridge.Image, chipQuad, detectQuads);
                }
            }
        }
        void _DUMP(string tag, RegionCellX3Class cell, Mat cellImg, QvQuad2D chipQuad, QvQuad2D[] detectQuads)
        {
            if (cellImg != null)
            {
                string fname = $"badConn_{tag}@{cell.CellRow}_{cell.CellCol}.png";
                using (var canvas = cellImg.CvtColor(ColorConversionCodes.GRAY2BGR))
                {
                    drawQuads(canvas, Scalar.Lime, chipQuad);
                    drawQuads(canvas, Scalar.OrangeRed, detectQuads);
                    canvas.SaveImage($"d:\\paso.log\\{fname}");
                }
            }
        }
        void drawQuads(Mat canvas, Scalar color, params QvQuad2D[] quads)
        {
            if (quads == null || quads.Length == 0)
                return;

            foreach (var quad in quads)
            {
                if (quad == null) continue;
                var pts = Array.ConvertAll(quad.Corners, c => new OpenCvSharp.Point((int)c.X, (int)c.Y));
                for (int i = 0, N = pts.Length; i < N; i++)
                {
                    int j = (i + 1) % N;
                    canvas.Line(pts[i], pts[j], color, 5);
                }
            }
        }
        #endregion
    }
}
