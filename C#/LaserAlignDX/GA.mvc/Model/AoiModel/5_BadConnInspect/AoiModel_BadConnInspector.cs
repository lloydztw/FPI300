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
                RunBadConnsInspection(bmpFullfov);

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
        public bool TryApplyBadConnFilters(Bitmap bmpSrc, out Bitmap bmpDisp, IEnumerable<RectangleF> rects)
        {
            try
            {
                if (bmpSrc != null)
                {
                    using (var bridge = new QxImageBridge(bmpSrc))
                    {
                        var roiRects = Array.ConvertAll(_xInspect.BadConnsRects.ToArray(),
                                        rc => JetEazy.Qcvt.CV(Rectangle.Round(rc)));

                        var imgDisp = applyFilters(bridge.Image, roiRects, out var _, true);
                        bmpDisp = imgDisp?.ToBitmap();

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                bmpDisp = null;
                throw;
            }

            bmpDisp = null;
            return false;
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
        /// 瑕疵檢測 (數群晶粒) (限用於同一線程內)
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
        /// 量測單一晶粒 (使用海康搜尋直線)
        /// </summary>
        private void RunOneBadConnectInspect(RegionCellX3Class cell, Bitmap cellBmp, ref RectangleF cellRoi, int threadId)
        {
            //(0) 取得 上一輪 晶粒定位 的結果 (chipData)
            var chipData = cell?.ChipData;
            if (chipData == null || chipData.ChipQuad2D == null)
                return;

            //(1) Reset
            chipData.BadConnBlobRects = null;

            try
            {
                //(2) 取得檢測框 runtime detectQuads (Region Coordinates, 以 cellRoi 左上為 (0,0)) 
                var detectQuads = CalcRuntimeDetectQuads(cell, cellRoi, cellBmp);
                if (detectQuads == null || detectQuads.Length == 0)
                    return;

                //(3) Quad => Rect
                var roiRects = Array.ConvertAll(detectQuads, q => JetEazy.Qcvt.CV(Rectangle.Round(q.BoundaryRect)));

                //(4) applyFilters
                using (var bridge = new QxImageBridge(cellBmp))
                {
                    applyFilters(bridge.Image, roiRects, out var ngRects, false);

                    // 轉換到 Fullfov Coordinates
                    if (ngRects != null && ngRects.Count > 0)
                    {
                        for (int i = 0, N = ngRects.Count; i < N; i++)
                            ngRects[i].Offset((int)cellRoi.X, (int)cellRoi.Y);

                        // 記入 chipData
                        chipData.BadConnBlobRects = ngRects?.ToArray();

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
        /// 計算 選轉 & 平移 後的 檢測框
        /// </summary>
        QvQuad2D[] CalcRuntimeDetectQuads(RegionCellX3Class cell, RectangleF cellRoi, Bitmap cellBmp = null, bool debug = false)
        {
            int NP = _xInspect.BadConnsCount;
            var detectQuads = new QvQuad2D[NP];

            if (NP == 0)
                return detectQuads;

            //(1) 取得 上一輪 晶粒定位 的結果 (chipData)
            var chipData = cell?.ChipData;

            //(1.1) 複製 chipQuad (Fullfov Coordinates) (runtime)
            var chipQuad = chipData?.ChipQuad2D?.Clone();
            if (chipQuad == null)
                return null;

            //(1.2) 複製 goldenQuad (Region Coordinates)
            var goldenQuad = chipData?.GoldenQuad2D?.Clone();
            if (goldenQuad == null)
                return null;


            try
            {
                //(1) 將 chipQuad 平移到 Region Coordinates
                chipQuad.Offset(-cellRoi.X, -cellRoi.Y);
                _DUMP("RuntimeChipQuad", cell, cellBmp, chipQuad);

                //(2) 投影轉換矩陣
                var srcPts = Array.ConvertAll(goldenQuad.Corners, c => new Point2f((float)c.X, (float)c.Y));
                var dstPts = Array.ConvertAll(chipQuad.Corners, c => new Point2f((float)c.X, (float)c.Y));
                var transMat = Cv2.GetPerspectiveTransform(srcPts, dstPts);

                //(3) 投影轉換所有的 _xInspect.BadConnsRects (Region Coordinates)
                for (int i = 0; i < NP; i++)
                {
                    var srcCorners = getCorners(_xInspect.BadConnsRects[i]);
                    var dstCorners = Cv2.PerspectiveTransform(srcCorners, transMat);
                    var detectQuad = new QvQuad2D()
                    {
                        Corners = Array.ConvertAll(dstCorners, c => new QVector2(c.X, c.Y))
                    };
                    detectQuads[i] = detectQuad;

                    _DUMP("DetectQuad", cell, cellBmp, detectQuad);
                }

                return detectQuads;
            }
            catch (Exception ex)
            {
                _LOG_ERROR(ex, $"{GetType().Name} 無法計算 DetectQuads!");
                throw;
            }
        }

        Mat applyFilters(Mat imgSrc, Rect[] roiRects, out List<Rectangle> ngBlobRects, bool generateDispImg = false)
        {
            var gray = GaImageUtil.ToU8(imgSrc);
            var imgDisp = generateDispImg ? new Mat(gray.Size(), MatType.CV_8UC3) : null;
            imgDisp?.SetTo(Scalar.Black);

            if (_xInspect == null)
            {
                ngBlobRects = null;
                if (gray != imgSrc) gray.Dispose();
                return imgDisp;
            }

            ngBlobRects = new List<Rectangle>();

            var boundRect = new Rect(0, 0, imgSrc.Width, imgSrc.Height);
            var threshold = _xInspect.BadConnsThreshold;
            var minX = _xInspect.BadConnsMinX;
            var minY = _xInspect.BadConnsMinY;

            var blobFinder = new EzBlobFinder();
            blobFinder.MinSize = new OpenCvSharp.Size(minX, minY);
            blobFinder.OptFillBorder = false;

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
                        foreach (var b in blocs)
                        {
                            var ngRect = b.Rect;
                            ngRect.Offset(roi.X, roi.Y);
                            ngBlobRects.Add(ngRect);
                        }

                        // 4. 將異常區塊 塗紅
                        dispRoi?.SetTo(Scalar.Red, binary);
                    }
                }
            }

            if (gray != imgSrc)
                gray.Dispose();

            if (ngBlobRects.Count == 0)
                ngBlobRects = null;

            return imgDisp;
        }        
        #endregion

        #region PRIVATE_HELPER_FUNCTIONS
        Point2d[] getCorners(RectangleF rect)
        {
            return new []
            {
                new Point2d(rect.Left, rect.Top),
                new Point2d(rect.Right, rect.Top),
                new Point2d(rect.Right, rect.Bottom),
                new Point2d(rect.Left, rect.Bottom),
            };
        }
        #endregion

        void _DUMP(string tag, RegionCellX3Class cell, Bitmap cellBmp, QvQuad2D regionQuad)
        {
            if(cellBmp!=null)
            {
                string fname = $"dump_{tag}@{cell.CellRow}_{cell.CellCol}.png";
                using (var bridge = new QxImageBridge(cellBmp))
                using (var canvas = bridge.Image.CvtColor(ColorConversionCodes.GRAY2BGR))
                {
                    var rect = JetEazy.Qcvt.CV(Rectangle.Round(regionQuad.BoundaryRect));
                    canvas.Rectangle(rect, Scalar.Lime);
                    canvas.SaveImage($"d:\\paso.log\\{fname}");
                }
            }
        }
    }
}
