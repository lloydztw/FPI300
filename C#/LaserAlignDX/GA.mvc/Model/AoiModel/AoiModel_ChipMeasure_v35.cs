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

using JetEazy.OpenCV;
using JetEazy.QMath;
using JetEazy.QvMath;
using JetEazy.Utils;
using LaserAlignDX.BasicSpace;
using LaserAlignDX.Model;
using LaserAlignDX.Model.Coords;
using LaserAlignDX.OPSpace;
using LaserAlignDX.OPSpace.RecipeSpace;
using LeTian.AoiLib;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisionDesigner;
using ErrorCodes = LaserAlignDX.Mvc.Model.ErrorCodes;
using MvdFindLineClass = LaserAlignDX.BasicSpace.MvdFindLineClass;

namespace LaserAlignDX.AoiModel.V35
{
    /// <summary>
    /// 晶粒尺寸量測
    /// </summary>
    public class AoiModel_ChipMeasure : AoiModelBase, IAoiChipMeasurer
    {
        #region CONFIG
        static bool N_THREADS_ENABLED => GlobalConfig.N_THREADS_ENABLED && true;
        static int N_THREADS => GlobalConfig.N_THREADS;
        static int N_CHUNKS = 7;
        #endregion

        #region GLOBAL_MESS
        InspectX3ParaClass _xInspect => base._xRecipe.InspectParams;
        #endregion

        #region KERNEL_MEMBERS
        /// <summary>
        /// Coordinates Transform
        /// </summary>
        IMicroChipTransform _microTransform;
        /// <summary>
        /// MVD Line Finders
        /// </summary>
        MvdFindLineClass[] _mvdLineFinders;
        #endregion

        #region GOLDEN_DATA
        Dictionary<EdgeBorder, QvQuad2D> _goldenLineBorderQuads;
        Dictionary<EdgeBorder, EzLSD.LineSegment> _goldenLines;
        Dictionary<EdgeBorder, Mat[]> _goldenChunkImages;
        //Dictionary<EdgeBorder, QVector[]> _goldenChunkPoints;
        #endregion

        #region RUNTIME_DATA
        GaCellsGroup[] _cellGroups;
        bool _is2ndRun;
        #endregion

        public override void Dispose()
        {
            base.Dispose();
            disposeMvdLineFinders();
            disposeGoldenData();
        }

        public void SetCellGroups(GaCellsGroup[] cellGroups)
        {
            this._cellGroups = cellGroups;
        }

        public override void Run(Bitmap sceneBmp = null)
        {
            bool go = _xInspect.optChipMeasurement; // || _xInspect.optChipDefectsInspect || QrUsed;
            if (!go)
                return;

            try
            {
                fire_AoiBegin();
                markRunStart();

                ////(1) 取得 Micro Transform
                //var activeCarrierID = getActiveCarrierID();
                ////>>> _worldTransform = _sysModel.TransformsModel.GetCameraPhysicTransform(activeCarrierID);
                //_microTransform = _sysModel.GetMicroTransform(activeCarrierID);
                //if (_microTransform == null)
                //    throw new Exception($"無法取得 Micro Transform ({activeCarrierID})");

                //(1) 尺寸量測前置準備
                if (!PrepareMeasurementData())
                    return;

                //(2) bmpFullfov
                Bitmap bmpFullfov = LineScanCamImageHolder.PeekBitmap();

                //(3) Run Measurements
                RunChipsMeasurement(bmpFullfov);

                //(4) Run Measurements (2nd)
                //RunChipsMeasurement2ndForNGs(bmpFullfov);

                markRunEnd(true);
                fire_AoiEnd();
            }
            catch (Exception ex)
            {
                // 2025-08-28 LETIAN: 巨圖統一由 LineScanCamImageHolder 管理其生命週期
                // 在此無需釋放 巨圖
#if (OPT_OLD_CODE)
                markRunEnd(false);
                fire_AoiEnd();
                var errCode = ErrorCodes.EXCEPTION_AT_AOI_RUN;
                string errMsg = GaUtil.GetEnumDescription(errCode)
                                + "\n\r" + GetType().Name
                                + "\n\r\n\r" + GetDeepExceptionMessage(ex);
                GaUtil.LOG(errMsg, Color.Red);
                _LOG_ERROR(ex, $"異常 @ {GetType().Name}.Run");
                fire_AoiError(errCode, errMsg);
#endif
                base.HandleAoiException(ex);
            }
        }

        public void TryMeasureOneChip(RegionCellX3Class cell, Bitmap cellBmp, ref RectangleF cellRoi)
        {
            //if (_microTransform == null)
            //    _microTransform = _sysModel.GetMicroTransform(getActiveCarrierID());
            //if (_microTransform == null)
            //    return;

            if (!PrepareMeasurementData(silent: true))
                return;

            RunOneChipMeasurement(cell, cellBmp, ref cellRoi, 0);
        }

        public bool TryFindLineSegment(EdgeBorder eBorder, Bitmap bmpSrc, RectangleF roiRect, out CMvdLineSegmentF resultLine)
        {
            //--------------------------------------------------------------
            // 海康邊線 準確度 深受 前景背景 對比 影響
            //--------------------------------------------------------------
            prepareMvdLineFinders(N_THREADS);
            var lineSegFinder = _mvdLineFinders[0];
            lineSegFinder.Background = _xInspect.xCarrierBackground;
            resultLine = lineSegFinder.Run(bmpSrc, roiRect, (int)eBorder);
            return resultLine != null;
        }

        public void AnalyzeGoldenData()
        {
            prepareGoldenData(true);
        }

        #region PRIVATE_RUN_FUNCTIONS
        private bool PrepareMeasurementData(bool silent = false)
        {
            //(1) Micro Transform
            if (_microTransform == null)
                _microTransform = _sysModel.GetMicroTransform(getActiveCarrierID());

            if (_microTransform == null)
            {
                if (!silent)
                    throw new Exception($"無法取得 Micro Transform ({getActiveCarrierID()})");
                return false;
            }

            //(2) MvdFinders
            prepareMvdLineFinders(N_THREADS);

            //(3) Golden Data
            prepareGoldenData();

            // Success
            return true;
        }

        /// <summary>
        /// LETIAN: 晶粒定位 與 尺寸量測 
        /// </summary>
        private void RunChipsMeasurement(Bitmap bmpFullfov)
        {
            if (!_xInspect.optChipMeasurement)
                return;

            _is2ndRun = false;

            fire_AoiBegin("晶粒尺寸量測");

            //_TM.RESET_ACCUM();
            #region 準備_CELL_GROUPS
            int N_GROUPS = _cellGroups != null ? _cellGroups.Length : MvdCompositeChipMatcher.N_CHANNLS;
            var groups = _cellGroups != null ? _cellGroups : GaCellsGroup.CollectGroups(N_GROUPS, _xRecipe, bmpFullfov);
            #endregion

            if (groups == null || groups.Length == 0)
                return;

            if (!N_THREADS_ENABLED)
            {
                // 單線程 (驗證用)
                for (int gid = 0; gid < groups.Length; gid++)
                {
                    RunChipsMeasurementOneT(gid, groups[gid]);
                }
            }
            else
            {
                // 多線程
                Parallel.For(0, groups.Length, gid =>
                {
                    RunChipsMeasurementOneT(gid, groups[gid]);
                });
            }

            #region CLEAN_UP
            if (groups != _cellGroups)
            {
                GaCellsGroup.DisposeAll(groups);
            }
            #endregion

            //_TM.DUMP_ACCUM();
        }

        /// <summary>
        /// LETIAN: 晶粒定位 與 尺寸量測 (針對 NG 進行二次量測)
        /// </summary>
        private void RunChipsMeasurement2ndForNGs(Bitmap bmpFullfov)
        {
            if (!_xInspect.optChipMeasurement)
                return;

            _is2ndRun = true;

            fire_AoiBegin("晶粒尺寸量測 (二次補測)");

            //_TM.RESET_ACCUM();

            #region 準備_CELL_GROUPS_FOR_第二次補測
            int N = _cellGroups != null ? _cellGroups.Length : MvdCompositeChipMatcher.N_CHANNLS;
            var ngGroups = GaCellsGroup.CollectGroups(N, _xRecipe, bmpFullfov, option: "NG_DIM_ONLY");
            #endregion

            if (ngGroups == null || ngGroups.Length == 0)
                return;

            if (!N_THREADS_ENABLED)
            {
                // 單線程 (驗證用)
                for (int gid = 0; gid < ngGroups.Length; gid++)
                {
                    RunChipsMeasurementOneT(gid, ngGroups[gid]);
                }
            }
            else
            {
                // 多線程
                Parallel.For(0, ngGroups.Length, gid =>
                {
                    RunChipsMeasurementOneT(gid, ngGroups[gid]);
                });
            }

            #region CLEAN_UP
            if (ngGroups != null)
            {
                GaCellsGroup.DisposeAll(ngGroups);
            }
            #endregion

            //_TM.DUMP_ACCUM();
        }

        /// <summary>
        /// LETIAN: 晶粒定位 與 尺寸量測 (區域) (限用於同一線程內)
        /// </summary>
        private void RunChipsMeasurementOneT(int threadIdx, GaCellsGroup cellsGroup)
        {
            var fullFovSize = cellsGroup.FullFovRect.Size;
            var debugSB = new StringBuilder();

            foreach (var gaCell in cellsGroup)
            {
                RegionCellX3Class cell = gaCell.Cell;
                Bitmap cellBmp = gaCell.CellBmp;
                RectangleF cellRoi = gaCell.CellRoi;

                //(1) 進度條事件
                fire_AoiProgressing(cell);

                //>>> bool go = cell.ChipData.ChipQuad2D != null && cell.inspectReason == InspectReason.PASS;
                bool go = cell.ChipData.ChipQuad2D != null && cell.IsResultPass();
                if (go)
                {
                    //(2) 量測單一晶粒
                    RunOneChipMeasurement(cell, cellBmp, ref cellRoi, threadIdx);
                    PackMeasureResult(cell);
                    //_TM.END("OneChipMeasurement");
                }
            }
        }

        /// <summary>
        /// 量測單一晶粒 (使用海康搜尋直線)
        /// </summary>
        private void RunOneChipMeasurement_000(RegionCellX3Class cell, Bitmap cellBmp, ref RectangleF cellRoi, int threadId)
        {
            // 取得 上一輪 晶粒定位 的結果 (chipData)
            var chipData = cell?.ChipData;
            if (chipData == null || chipData.ChipQuad2D == null)
                return;

            // 海康直線蒐尋器
            var mvdLineFinder = _mvdLineFinders[threadId % _mvdLineFinders.Length];

            #region 邊線處理
            EdgeBorder eBorder = EdgeBorder.Left;
            try
            {
                var lineBorderQuads = CalcRuntimeLocalLineBorderQuads(cell, cellRoi, cellBmp);

                if (lineBorderQuads == null)
                    return;

                for (int borderIdx = 0, N = lineBorderQuads.Length; borderIdx < N; borderIdx++)
                {
                    //(0) enum
                    eBorder = (EdgeBorder)borderIdx;

                    //(1) borderQuad
                    var borderQuad = lineBorderQuads[borderIdx];
                    var mvdRoi = borderQuad.ToCMvdRectangleF();

                    //(2.0) 海康線檢 I
                    CMvdLineSegmentF mvdLine;
                    if (true)   // if (!_is2ndRun)
                    {
                        // 海康線檢
                        mvdLine = _RunMvdLineFinder(mvdLineFinder, borderIdx, cellBmp, mvdRoi, chipData.ChipQuad2D.Angle);
                    }
                    //(2.1) 海康線檢 II (暫時不使用)
                    else
                    {
                        using (Bitmap cellBmp2 = (Bitmap)cellBmp.Clone())
                        {
                            // 塗掉中段 (1/3) 
                            fill_border_mid_area(cellBmp2, borderQuad, eBorder, Scalar.Black);
                            // 海康線檢(輸出為 cell.cMvdLineSegmentFsOut)
                            mvdLine = _RunMvdLineFinder(mvdLineFinder, borderIdx, cellBmp2, mvdRoi, chipData.ChipQuad2D.Angle);
                        }

                        // 如果塗掉中段 (1/3) 仍然抓不到, 回過頭使用 原來的方法
                        if (mvdLine == null)
                        {
                            mvdLine = _RunMvdLineFinder(mvdLineFinder, borderIdx, cellBmp, mvdRoi, chipData.ChipQuad2D.Angle);
                        }
                    }

                    //(3) 將 CMvdLine 轉換成 EzLSD.LineSegment
                    var line = mvdLine?.ToLineSegment();
                    //(3.1) 加回 ROI Offset
                    line?.Offset(cellRoi.X, cellRoi.Y);
                    //(3.2) 記入 cell.ChipData
                    cell.ChipData.LineSegments[borderIdx] = line;

                    //(4) 更新 LineBorderBoxes
                    //(4.1) 加回 ROI Offset
                    borderQuad.Offset(cellRoi.X, cellRoi.Y);
                    //(4.2) 更新 LineBorderBoxes;
                    chipData.LineBorderBoxes[borderIdx] = borderQuad.ToBox2D();

                    ////(4.3) 更新到 cell 舊的 Gaara Data (廢除)
                    //cell.cMvdShapesForFindLineRegion[borderIdx] = borderQuad.ToCMvdRectangleF();
                }

                AsyncDumpLineSegmentsData(cell, ref cellRoi);
            }
            catch (Exception ex)
            {
                string borderName = JetEazy.QxNums.GetEnumDescription(eBorder);
                _LOG_ERROR(ex, $"{borderName} 定位異常");
                throw;
            }
            #endregion

            #region 尺寸長寬量測
            try
            {
                #region OLD_CODE
                ////(0) 將 CMvdLine 轉換成 EzLSD.LineSegment
                //var lines = Array.ConvertAll(cell.cMvdLineSegmentFsOut, mvdLine => mvdLine?.ToLineSegment());
                //for (int i = 0, len = lines.Length; i < len; i++)
                //{
                //    //(1.1) 加回 ROI Offset
                //    lines[i]?.Offset(cellRoi.X, cellRoi.Y);
                //    //(1.2) 記入 cell.ChipData
                //    chipData.LineSegments[i] = lines[i];
                //}
                #endregion

                //(1) LineSegments (Camera Coordinates) (單位 pixels)
                var lines = chipData.LineSegments;

                //(2) 使用 Micro Transform 計算 尺寸 與 邊隙
                //    (結果會直接存入 cell.ChipData 內)
                bool toMeasureGaps = _xInspect.optPadEdgeGapsMeasurement && _xInspect.xAlgorithm == MatchAlgorithmEnum.GridMatch;
                var err = _microTransform.CalcChipDimension(out SizeF dimension, lines, chipData, toMeasureGaps);

                //(3) 記入結果
                cell.RunWidth = dimension.Width;
                cell.RunHeight = dimension.Height;

                //(4) 異常
                if (err != ErrorCodes.OK)
                    throw new Exception(GaUtil.GetEnumDescription(err));
            }
            catch (Exception ex)
            {
                _LOG_ERROR(ex, "晶粒 尺寸量測 異常");
                return;
            }
            #endregion
        }

        /// <summary>
        /// 量測單一晶粒 (使用分段 Template Match)
        /// </summary>
        private void RunOneChipMeasurement(RegionCellX3Class cell, Bitmap cellBmp, ref RectangleF cellRoi, int threadId)
        {
            RunOneChipMeasurement_000(cell, cellBmp, ref cellRoi, threadId);
            return;

            // 取得 上一輪 晶粒定位 的結果 (chipData)
            var chipData = cell?.ChipData;
            if (chipData == null || chipData.ChipQuad2D == null)
                return;

            #region 邊線處理
            EdgeBorder eBorder = EdgeBorder.Left;
            try
            {
                var runtimeBorderQuads = CalcRuntimeLocalLineBorderQuads(cell, cellRoi, cellBmp);
                if (runtimeBorderQuads == null)
                    return;

                #region DEBUG_DUMP
                if (false)
                {
                    using (var bridge = new QxImageBridge(cellBmp))
                    using (Mat canvas = bridge.Image.CvtColor(ColorConversionCodes.GRAY2BGR))
                    {
                        for (int i = 0, len = runtimeBorderQuads.Length; i < len; i++)
                            DrawQuad(canvas, runtimeBorderQuads[i].Corners, Scalar.BlueViolet);
                        canvas.SaveImage("d:\\paso.log\\runtimeBorderQuads.png");
                    }
                }
                #endregion

                using (var bridge = new QxImageBridge(cellBmp))
                {
                    Mat cellImg = bridge.Image;

                    for (int borderIdx = 0, N = runtimeBorderQuads.Length; borderIdx < N; borderIdx++)
                    {
                        //(0) enum
                        eBorder = (EdgeBorder)borderIdx;

                        //(1) goldenBorderQuad
                        var goldenBorderQuad = _goldenLineBorderQuads[eBorder];
                        var goldenLine = _goldenLines[eBorder];
                        if (goldenBorderQuad == null || goldenLine == null)
                            continue;

                        //(2) borderQuad
                        var runtimeBorderQuad = runtimeBorderQuads[borderIdx];
                        runtimeBorderQuad.Sort();
                        goldenBorderQuad.Sort();

                        //(3) 投影
                        var gRect = goldenBorderQuad.BoundaryRect;
                        //(3.1) SRC: 直接使用 Runtime 在 cellBmp 上的座標
                        Point2f[] src = Array.ConvertAll(runtimeBorderQuad.Corners, c => new Point2f((float)c.X, (float)c.Y));
                        //(3.2) DST: 將 Golden 頂點扣除 BoundaryRect 的 X/Y 偏移，拉回 (0,0) 開始的局部畫布座標
                        Point2f[] dst = Array.ConvertAll(goldenBorderQuad.Corners, c =>
                            new Point2f((float)(c.X - gRect.X), (float)(c.Y - gRect.Y))
                        );
                        
                        //(4) Chunk Template Matching
                        using (Mat transToGolden = Cv2.GetPerspectiveTransform(src, dst))
                        using (Mat transToRuntime = Cv2.GetPerspectiveTransform(dst, src))
                        using (Mat runtimeBorderImg = new Mat())
                        {
                            //(3.3.1) dstSize
                            var dstSize = new OpenCvSharp.Size((int)gRect.Width, (int)gRect.Height);
                            //(3.3.2) 執行透視變換
                            Cv2.WarpPerspective(bridge.Image, runtimeBorderImg, transToGolden, dstSize);
                            //(3.3.3) 存檔驗證 (此時印出來的 perspective_*.png 應該就是拉直且清晰的局部邊緣圖像了)
                            runtimeBorderImg.SaveImage($"d:\\paso.log\\perspective_{eBorder}.png");

                            #region DEBUG_DUMP
                            if (false)
                            {
                                //(3.3.4) 印出 src 與 dst 座標，確認數值範圍與順序
                                System.Diagnostics.Trace.WriteLine($"SRC: {string.Join(" | ", src.Select(p => $"({p.X:0.0},{p.Y:0.0})"))}");
                                System.Diagnostics.Trace.WriteLine($"DST: {string.Join(" | ", dst.Select(p => $"({p.X},{p.Y})"))}");
                                //(3.3.5) 檢查輸入圖與輸出圖尺寸
                                System.Diagnostics.Trace.WriteLine($"Input Image Size: {bridge.Image.Width}x{bridge.Image.Height}");
                                System.Diagnostics.Trace.WriteLine($"Target dsize: {dstSize.Width}x{dstSize.Height}");
                            }
                            #endregion

                            var boundRect = new Rect(0, 0, runtimeBorderImg.Width, runtimeBorderImg.Height);
                            var goldenTemplates = _goldenChunkImages[eBorder];
                            var linePoints = new List<Point2f>();
                            var scores = new List<double>();
                            var offsetX = (int)gRect.X;
                            var offsetY = (int)gRect.Y;
                            foreach (var roi in iterChunkRoiRects(eBorder, goldenBorderQuad, goldenLine, true))
                            {
                                var roiC = roi;
                                roiC.X -= offsetX;
                                roiC.Y -= offsetY;
                                JetEazy.Qcvt.ClipBoundary(ref roiC, ref boundRect);

                                int idx = linePoints.Count;
                                if (idx >= goldenTemplates.Length)
                                    break;

                                var scene = runtimeBorderImg[roiC];
                                var template = goldenTemplates[idx];

                                using (Mat similarity = new Mat())
                                {
                                    Cv2.MatchTemplate(scene, template, similarity, TemplateMatchModes.CCorrNormed);
                                    similarity.MinMaxLoc(out double minVal, out double maxVal, out OpenCvSharp.Point minLoc, out OpenCvSharp.Point maxLoc);
                                    Point2f ptInWarped = new Point2f(maxLoc.X + roiC.X, maxLoc.Y + roiC.Y);
                                    linePoints.Add(ptInWarped);
                                    scores.Add(maxVal);
                                }
                            }

                            var ptsInRuntime = Cv2.PerspectiveTransform(linePoints, transToRuntime);
                            using (Mat canvas = cellImg.CvtColor(ColorConversionCodes.GRAY2BGR))
                            {
                                DrawDots(canvas, ptsInRuntime, Scalar.Red);
                                canvas.SaveImage($"d:\\paso.log\\templ_chuck_line_{eBorder}.png");
                            }
                        }

                        //var mvdRoi = runtimeBorderQuad.ToCMvdRectangleF();

                        ////(2.0) 海康線檢 I
                        //CMvdLineSegmentF mvdLine;
                        //if (true)   // if (!_is2ndRun)
                        //{
                        //    // 海康線檢
                        //    mvdLine = _RunMvdLineFinder(mvdLineFinder, borderIdx, cellBmp, mvdRoi, chipData.ChipQuad2D.Angle);
                        //}

                        ////(3) 將 CMvdLine 轉換成 EzLSD.LineSegment
                        //var line = mvdLine?.ToLineSegment();
                        ////(3.1) 加回 ROI Offset
                        //line?.Offset(cellRoi.X, cellRoi.Y);
                        ////(3.2) 記入 cell.ChipData
                        //cell.ChipData.LineSegments[borderIdx] = line;

                        ////(4) 更新 LineBorderBoxes
                        ////(4.1) 加回 ROI Offset
                        //runtimeBorderQuad.Offset(cellRoi.X, cellRoi.Y);
                        ////(4.2) 更新 LineBorderBoxes;
                        //chipData.LineBorderBoxes[borderIdx] = runtimeBorderQuad.ToBox2D();
                    }
                }

                AsyncDumpLineSegmentsData(cell, ref cellRoi);
            }
            catch (Exception ex)
            {
                string borderName = JetEazy.QxNums.GetEnumDescription(eBorder);
                _LOG_ERROR(ex, $"{borderName} 定位異常");
                throw;
            }
            #endregion

            #region 尺寸長寬量測
            try
            {
                //(1) LineSegments (Camera Coordinates) (單位 pixels)
                var lines = chipData.LineSegments;

                //(2) 使用 Micro Transform 計算 尺寸 與 邊隙
                //    (結果會直接存入 cell.ChipData 內)
                bool toMeasureGaps = _xInspect.optPadEdgeGapsMeasurement && _xInspect.xAlgorithm == MatchAlgorithmEnum.GridMatch;
                var err = _microTransform.CalcChipDimension(out SizeF dimension, lines, chipData, toMeasureGaps);

                //(3) 記入結果
                cell.RunWidth = dimension.Width;
                cell.RunHeight = dimension.Height;

                //(4) 異常
                if (err != ErrorCodes.OK)
                    throw new Exception(GaUtil.GetEnumDescription(err));
            }
            catch (Exception ex)
            {
                _LOG_ERROR(ex, "晶粒 尺寸量測 異常");
                return;
            }
            #endregion
        }

        /// <summary>
        /// 整理打包 尺寸计算 的 最後判定结果
        /// </summary>
        /// <returns>true:OK false:NG</returns>
        private void PackMeasureResult(RegionCellX3Class cell)
        {
            if (cell == null)
                return;

            bool ok = true;

            if (_xInspect.optChipMeasurement)
            {
                //(1) 判定 長寬 是否達標
                var dimResults = new bool[2];
                ok &= (dimResults[0] = !(cell.RunWidth < _xInspect.mWidthStandMin || cell.RunWidth > _xInspect.mWidthStandMax));
                ok &= (dimResults[1] = !(cell.RunHeight < _xInspect.mHeightStandMin || cell.RunHeight > _xInspect.mHeightStandMax));

                var chipDim = cell.ChipData?.ChipDimension;
                if (chipDim != null)
                    chipDim.PassNgResults = dimResults;

                if (!ok)
                {
                    cell.MarkResult(InspectReason.NG_CUT);
                }

                //(2) 判定 邊隙 是否達標
                if (_xInspect.optPadEdgeGapsMeasurement && _xInspect.xAlgorithm == MatchAlgorithmEnum.GridMatch)
                {
                    var gaps = cell.ChipData?.PadEdgeGaps;
                    if (gaps == null)
                    {
                        ok = false;
                    }
                    else
                    {
                        var min = new QVector(_xInspect.PadEdgeGapX_Min, _xInspect.PadEdgeGapY_Min);
                        var max = new QVector(_xInspect.PadEdgeGapX_Max, _xInspect.PadEdgeGapY_Max);
                        var maxDiff = _xInspect.PadEdgeX_Diff_Upper;
                        gaps.Check(out ok, min, max, maxDiff);
                    }

                    if (!ok)
                    {
                        cell.MarkResult(InspectReason.NG_EDGE_GAP);
                    }
                }
            }

            // 強制設定 Stampede (踩腳)
            if (_xInspect.optTiltDetectEnabled)
            {
                var chipData = cell.ChipData;
                bool isStampede = (chipData != null && chipData.ChipCoords.IsStampede);
                if (isStampede)
                    cell.MarkResult(InspectReason.NG_AMBIGUOUS_BLOC);
            }
        }

        /// <summary>
        /// 計算 選轉&平移 後的 邊線框 (左, 上, 右, 下)
        /// </summary>
        private QvQuad2D[] CalcRuntimeLocalLineBorderQuads(RegionCellX3Class cell, RectangleF cellRoi, Bitmap cellBmp = null, bool debug = false)
        {
            //------------------------------------------------------------------------------
            // REV_2026-03-09 整合海康 Template Match
            //------------------------------------------------------------------------------

            //(0) 取得 上一輪 晶粒定位 的結果 (chipData)
            var chipData = cell?.ChipData;

            //(0.1) 複製 goldenQuad 與 chipQuad
            var chipQuad = chipData?.ChipQuad2D?.Clone();
            var goldenQuad = chipData?.GoldenQuad2D?.Clone();
            if (chipQuad == null || goldenQuad == null)
                return null;

            EdgeBorder eBorder = EdgeBorder.Left;
            try
            {
                int NP = 4;

                //(1) lineBorders : 位於 goldenRegionRect (_xRecipe.xRectRegionPrint) 內部
                var lineBorderQuads = new QvQuad2D[]
                {
                    QvQuad2D.From(ref _xRecipe.xLineLeft),
                    QvQuad2D.From(ref _xRecipe.xLineTop),
                    QvQuad2D.From(ref _xRecipe.xLineRight),
                    QvQuad2D.From(ref _xRecipe.xLineBottom),
                };

                //(2) 將 goldenQuad 平移到 goldenRegionRect (_xRecipe.xRectRegionPrint) 坐標系
                var goldenChipTemplateRoi = _xRecipe.xRegionTrain;
                goldenQuad.Offset(goldenChipTemplateRoi.X, goldenChipTemplateRoi.Y);

                //(2.1) DEBUG_DUMP
                #region DEBUG_DUMP
                if (debug)
                {
                    if (EzPadsGridFinder.VISUAL_DEBUG)
                    {
                        using (var bmpTmp = (Bitmap)_xRecipe.bmpprinttemplate.Clone())
                        using (var bridge = new QxImageBridge(bmpTmp))
                        using (var img = new Mat())
                        {
                            Cv2.CvtColor(bridge.Image, img, ColorConversionCodes.GRAY2BGR);
                            img.Rectangle(JetEazy.Qcvt.CV(Rectangle.Round(goldenQuad.BoundaryRect)), Scalar.Lime, 3);
                            img.SaveImage("d:\\paso.log\\lineBorderQuads_g.png");

                            var boxes = new List<QvBox2D>(Array.ConvertAll(lineBorderQuads, lb => lb.ToBox2D()));
                            boxes.Add(goldenQuad.ToBox2D());
                            VxDebugDrawer.Draw(img, boxes, OpenCvSharp.Scalar.Orange, shrink: 1);
                            img.SaveImage("d:\\paso.log\\lineBorderQuads_g.png");
                        }
                    }
                    if (EzPadsGridFinder.VISUAL_DEBUG)
                    {
                        using (var bmpG = (Bitmap)cellBmp.Clone())
                        using (var bridge = new QxImageBridge(bmpG))
                        using (var img = new Mat())
                        {
                            Cv2.CvtColor(bridge.Image, img, ColorConversionCodes.GRAY2BGR);

                            img.Rectangle(JetEazy.Qcvt.CV(Rectangle.Round(chipQuad.BoundaryRect)), Scalar.Lime, 3);
                            img.SaveImage("d:\\paso.log\\cellBmp.png");

                            var boxes = new List<QvBox2D>();
                            //foreach (var line in lineBorderQuads)
                            //    boxes.Add(line.ToBox2D());
                            boxes.Add(chipQuad.ToBox2D());

                            VxDebugDrawer.Draw(img, boxes, OpenCvSharp.Scalar.Orange, shrink: 1);
                            img.SaveImage("d:\\paso.log\\cellBmp.png");
                        }
                    }
                }
                #endregion

                //(3) 計算 lineBorder 與 goldenQuad 邊線, 相對距離
                for (int i = 0; i < NP; i++)
                {
                    int iPrev = (i == 0) ? (NP - 1) : (i - 1);

                    var lineBorderQuad = lineBorderQuads[i];

                    var goldenLineMid = (goldenQuad.Corners[i] + goldenQuad.Corners[iPrev]) / 2.0;
                    var v = UnitVect(goldenLineMid, goldenQuad.Center);
                    var u = UnitVect(goldenLineMid, goldenQuad.Corners[i]);
                    var lbCorners = Array.ConvertAll(lineBorderQuad.Corners, lc => lc - goldenLineMid);
                    var lbCornersU = Array.ConvertAll(lbCorners, c => c * u);
                    var lbCornersV = Array.ConvertAll(lbCorners, c => c * v);

                    var runtimeLineMid = (chipQuad.Corners[i] + chipQuad.Corners[iPrev]) / 2.0;
                    var V = UnitVect(runtimeLineMid, chipQuad.Center);
                    var U = UnitVect(runtimeLineMid, chipQuad.Corners[i]);
                    //debugPoints.Add(runtimeMid);

                    for (int k = 0, N = lbCorners.Length; k < 4; k++)
                    {
                        lbCorners[k] = runtimeLineMid + (U * lbCornersU[k]) + (V * lbCornersV[k]);
                    }

                    lineBorderQuad.Corners = lbCorners;
                    lineBorderQuad.Sort();
                }

                foreach (var line in lineBorderQuads)
                    line.Offset(-cellRoi.X, -cellRoi.Y);

                //(3.1) DEBUG_DUMP_II
                #region DEBUG_DUMP_II
                if (debug && cellBmp != null)
                {
                    using (var bmpTmp = (Bitmap)cellBmp.Clone())
                    using (var bridge = new QxImageBridge(bmpTmp))
                    using (var img = new Mat())
                    {
                        Cv2.CvtColor(bridge.Image, img, ColorConversionCodes.GRAY2BGR);
                        var boxes = Array.ConvertAll(lineBorderQuads, lb => lb.ToBox2D());
                        VxDebugDrawer.Draw(img, boxes, OpenCvSharp.Scalar.Orange, shrink: 1);
                        img.SaveImage("d:\\paso.log\\lineBorderQuads.png");
                    }
                }
                #endregion

                return lineBorderQuads;
            }
            catch (Exception ex)
            {
                string borderName = JetEazy.QxNums.GetEnumDescription(eBorder);
                _LOG_ERROR(ex, $"{borderName} 無法計算 LineBorderQuads!");
                throw ex;
            }
        }

        #endregion

        #region MVD_LINE_SEGMENTS_FUNCTIONS
        private void disposeMvdLineFinders()
        {
            if (_mvdLineFinders != null)
            {
                foreach (var finder in _mvdLineFinders)
                    finder?.Dispose();
                _mvdLineFinders = null;
            }
        }
        private void prepareMvdLineFinders(int NThreads)
        {
            if (_mvdLineFinders == null || _mvdLineFinders.Length < NThreads)
            {
                disposeMvdLineFinders();
                _mvdLineFinders = new MvdFindLineClass[NThreads];
                for (int i = 0; i < NThreads; i++)
                    _mvdLineFinders[i] = new MvdFindLineClass();
            }
        }

        /// <summary>
        /// 使用海康套件尋找直線
        /// </summary>
        CMvdLineSegmentF _RunMvdLineFinder(IMvdLineFinder mvdFindLineClass, int borderIndex, Bitmap bmp, CMvdRectangleF roi, double angleRef = 0)
        {
            int NP = 4;

            //if (mvdFindLineClass == null)
            //    mvdFindLineClass = new MvdFindLineClass();
            //borderIndex %= NP;
            //cMvdLineSegmentFsOut[borderIndex] = null;

            //>>> 根據 angleRef 將 borderIndex 正規化
            int sideIndex;
            if (angleRef > 70.0)
            {
                sideIndex = (borderIndex + 1) % NP;
            }
            else if (angleRef < -70.0)
            {
                sideIndex = borderIndex == 0 ? NP - 1 : (borderIndex - 1) % NP;
            }
            else
            {
                sideIndex = borderIndex;
            }

            // 左
            if (sideIndex == 0)
            {
                mvdFindLineClass.bPositive = _xInspect.bPositive0;
                mvdFindLineClass.bFindOrient = true;
                mvdFindLineClass.bEdgePolarity = _xInspect.bEdgePolarity0;
            }
            // 上
            else if (sideIndex == 1)
            {
                mvdFindLineClass.bPositive = _xInspect.bPositive1;
                mvdFindLineClass.bFindOrient = false;
                mvdFindLineClass.bEdgePolarity = _xInspect.bEdgePolarity1;
            }
            // 右
            else if (sideIndex == 2)
            {
                mvdFindLineClass.bPositive = _xInspect.bPositive2;
                mvdFindLineClass.bFindOrient = true;
                mvdFindLineClass.bEdgePolarity = _xInspect.bEdgePolarity2;
            }
            // 下
            else if (sideIndex == 3)
            {
                mvdFindLineClass.bPositive = _xInspect.bPositive3;
                mvdFindLineClass.bFindOrient = false;
                mvdFindLineClass.bEdgePolarity = _xInspect.bEdgePolarity3;
            }

            mvdFindLineClass.Background = _xInspect.xCarrierBackground;
            var resultLine = mvdFindLineClass.Run(bmp, roi, sideIndex);

            //cMvdLineSegmentFsOut[borderIndex] = resultLine;
            return resultLine;
        }
        
        /// <summary>
        /// 寻找平行线
        /// </summary>
        /// <param name="iSideIndex">哪条边序号</param>
        /// <param name="bmp">输入图片</param>
        /// <param name="r">寻找的ROI</param>
        void _RunMvdPairLineFinder(int iSideIndex, Bitmap bmp, CMvdRectangleF r)
        {
            // 停用, 改用新的計算方式 !!!
#if (OPT_LEGACY)
            if (mvdPairLineClass == null)
                mvdPairLineClass = new MvdPairLineClass();
            cMvdLineSegmentFsOut[iSideIndex] = null;
            CPairLineFindResult cPairLineFindResult = null;
            if (iSideIndex == 0)
            {
                mvdPairLineClass.bPositive = xInspect.bPositive0;
                mvdPairLineClass.bFindOrient = true;
                //mvdPairLineClass.bEdgePolarity = xInspect.bEdgePolarity0;
                cPairLineFindResult = mvdPairLineClass.Run(bmp, r);
                if (cPairLineFindResult != null)
                {
                    #region 左边距

                    try
                    {
                        if (cPairLineFindResult.Line0 != null && cPairLineFindResult.Line1 != null)
                        {
                            // 使用 MVD VisionDesigner Tool
                            using (var cL2LMeasureToolObj = new VisionDesigner.L2LMeasure.CL2LMeasureTool())
                            {
                                cL2LMeasureToolObj.BasicParam.Line1 = cPairLineFindResult.Line0;
                                cL2LMeasureToolObj.BasicParam.Line2 = cPairLineFindResult.Line1;
                                cL2LMeasureToolObj.Run();
                                var cL2LMeasureRes = cL2LMeasureToolObj.Result;
                                DisLeft = cL2LMeasureRes.VerticalAbsDist * INI.Instance.ImageResolutionX;
                            }
                        }
                    }
                    catch (MvdException ex)
                    {
                        //Console.WriteLine("Fail with ErrorCode: 0x" + ex.ErrorCode.ToString("X"));
                        LtDebug.LOG.Error(ex, "左边距 異常: ErrorCode = 0x{0:X}", ex.ErrorCode);
                    }
                    catch (System.Exception ex)
                    {
                        //Console.WriteLine("Fail with error " + ex.Message);
                        LtDebug.LOG.Error(ex, "左边距 異常");
                    }

                    #endregion

                    if (xInspect.bPositive0)
                    {
                        cMvdLineSegmentFsOut[iSideIndex] = cPairLineFindResult.Line0;
                        cMvdLineSegmentFsInSide[iSideIndex] = cPairLineFindResult.Line1;
                    }
                    else
                    {
                        cMvdLineSegmentFsOut[iSideIndex] = cPairLineFindResult.Line1;
                        cMvdLineSegmentFsInSide[iSideIndex] = cPairLineFindResult.Line0;
                    }
                }
            }
            else if (iSideIndex == 1)
            {
                mvdPairLineClass.bPositive = xInspect.bPositive1;
                mvdPairLineClass.bFindOrient = false;
                //mvdPairLineClass.bEdgePolarity = xInspect.bEdgePolarity1;
                cPairLineFindResult = mvdPairLineClass.Run(bmp, r);
                if (cPairLineFindResult != null)
                {
                    #region 上边距

                    try
                    {
                        if (cPairLineFindResult.Line0 != null && cPairLineFindResult.Line1 != null)
                        {
                            // 使用 MVD VisionDesigner Tool
                            using (var cL2LMeasureToolObj = new VisionDesigner.L2LMeasure.CL2LMeasureTool())
                            {
                                cL2LMeasureToolObj.BasicParam.Line1 = cPairLineFindResult.Line0;
                                cL2LMeasureToolObj.BasicParam.Line2 = cPairLineFindResult.Line1;
                                cL2LMeasureToolObj.Run();
                                var cL2LMeasureRes = cL2LMeasureToolObj.Result;
                                DisTop = cL2LMeasureRes.VerticalAbsDist * INI.Instance.ImageResolutionY;
                            }
                        }
                    }
                    catch (MvdException ex)
                    {
                        //Console.WriteLine("Fail with ErrorCode: 0x" + ex.ErrorCode.ToString("X"));
                        LtDebug.LOG.Error(ex, "上边距 異常: ErrorCode = 0x{0:X}", ex.ErrorCode);
                    }
                    catch (System.Exception ex)
                    {
                        //Console.WriteLine("Fail with error " + ex.Message);
                        LtDebug.LOG.Error(ex, "上边距 異常");
                    }

                    #endregion
                    if (xInspect.bPositive1)
                    {
                        cMvdLineSegmentFsOut[iSideIndex] = cPairLineFindResult.Line0;
                        cMvdLineSegmentFsInSide[iSideIndex] = cPairLineFindResult.Line1;
                    }
                    else
                    {
                        cMvdLineSegmentFsOut[iSideIndex] = cPairLineFindResult.Line1;
                        cMvdLineSegmentFsInSide[iSideIndex] = cPairLineFindResult.Line0;
                    }
                }
            }
            else if (iSideIndex == 2)
            {
                mvdPairLineClass.bPositive = xInspect.bPositive2;
                mvdPairLineClass.bFindOrient = true;
                //mvdPairLineClass.bEdgePolarity = xInspect.bEdgePolarity2;
                cPairLineFindResult = mvdPairLineClass.Run(bmp, r);
                if (cPairLineFindResult != null)
                {
                    #region 右边距

                    try
                    {
                        if (cPairLineFindResult.Line0 != null && cPairLineFindResult.Line1 != null)
                        {
                            // 使用 MVD VisionDesigner Tool
                            using (var cL2LMeasureToolObj = new VisionDesigner.L2LMeasure.CL2LMeasureTool())
                            {
                                cL2LMeasureToolObj.BasicParam.Line1 = cPairLineFindResult.Line0;
                                cL2LMeasureToolObj.BasicParam.Line2 = cPairLineFindResult.Line1;
                                cL2LMeasureToolObj.Run();
                                var cL2LMeasureRes = cL2LMeasureToolObj.Result;
                                DisRight = cL2LMeasureRes.VerticalAbsDist * INI.Instance.ImageResolutionX;
                            }
                        }
                    }
                    catch (MvdException ex)
                    {
                        //Console.WriteLine("Fail with ErrorCode: 0x" + ex.ErrorCode.ToString("X"));
                        LtDebug.LOG.Error(ex, "右边距 異常: ErrorCode = 0x{0:X}", ex.ErrorCode);
                    }
                    catch (System.Exception ex)
                    {
                        //Console.WriteLine("Fail with error " + ex.Message);
                        LtDebug.LOG.Error(ex, "右边距 異常");
                    }

                    #endregion
                    if (xInspect.bPositive2)
                    {
                        cMvdLineSegmentFsOut[iSideIndex] = cPairLineFindResult.Line1;
                        cMvdLineSegmentFsInSide[iSideIndex] = cPairLineFindResult.Line0;
                    }
                    else
                    {
                        cMvdLineSegmentFsOut[iSideIndex] = cPairLineFindResult.Line0;
                        cMvdLineSegmentFsInSide[iSideIndex] = cPairLineFindResult.Line1;
                    }
                }
            }
            else if (iSideIndex == 3)
            {
                mvdPairLineClass.bPositive = xInspect.bPositive3;
                mvdPairLineClass.bFindOrient = false;
                //mvdPairLineClass.bEdgePolarity = xInspect.bEdgePolarity3;
                cPairLineFindResult = mvdPairLineClass.Run(bmp, r);
                if (cPairLineFindResult != null)
                {
                    #region 下边距

                    try
                    {
                        if (cPairLineFindResult.Line0 != null && cPairLineFindResult.Line1 != null)
                        {
                            // 使用 MVD VisionDesigner Tool
                            using (var cL2LMeasureToolObj = new VisionDesigner.L2LMeasure.CL2LMeasureTool())
                            {
                                cL2LMeasureToolObj.BasicParam.Line1 = cPairLineFindResult.Line0;
                                cL2LMeasureToolObj.BasicParam.Line2 = cPairLineFindResult.Line1;
                                cL2LMeasureToolObj.Run();
                                var cL2LMeasureRes = cL2LMeasureToolObj.Result;
                                DisBottom = cL2LMeasureRes.VerticalAbsDist * INI.Instance.ImageResolutionY;
                            }
                        }
                    }
                    catch (MvdException ex)
                    {
                        //Console.WriteLine("Fail with ErrorCode: 0x" + ex.ErrorCode.ToString("X"));
                        LtDebug.LOG.Error(ex, "下边距 異常: ErrorCode = 0x{0:X}", ex.ErrorCode);
                    }
                    catch (System.Exception ex)
                    {
                        //Console.WriteLine("Fail with error " + ex.Message);
                        LtDebug.LOG.Error(ex, "下边距 異常");
                    }

                    #endregion
                    if (xInspect.bPositive3)
                    {
                        cMvdLineSegmentFsOut[iSideIndex] = cPairLineFindResult.Line1;
                        cMvdLineSegmentFsInSide[iSideIndex] = cPairLineFindResult.Line0;
                    }
                    else
                    {
                        cMvdLineSegmentFsOut[iSideIndex] = cPairLineFindResult.Line0;
                        cMvdLineSegmentFsInSide[iSideIndex] = cPairLineFindResult.Line1;
                    }
                }
            }
#endif
        }
        #endregion

        #region PRIVATE_GOLDEN_DATA_FUNCTIONS
        private void disposeGoldenData()
        {
            if (_goldenChunkImages != null)
            {
                foreach (var imgs in _goldenChunkImages.Values)
                {
                    if (imgs == null) continue;
                    foreach (var img in imgs)
                        img?.Dispose();
                }
                _goldenChunkImages.Clear();
                _goldenChunkImages = null;
            }
        }
        private void prepareGoldenData(bool force = false)
        {
            if (!force && _goldenChunkImages != null && _goldenLineBorderQuads != null && _goldenLines != null)
                return;

            disposeGoldenData();

            //(1) goldenLineBorderQuads : 位於 goldenRegionCellRect 內部
            //    (以 _xRecipe.GoldenRegionCellRect 左上為相對零點) 
            _goldenLineBorderQuads = new Dictionary<EdgeBorder, QvQuad2D>()
            {
                {EdgeBorder.Left, QvQuad2D.From(ref _xRecipe.xLineLeft) },
                {EdgeBorder.Top, QvQuad2D.From(ref _xRecipe.xLineTop) },
                {EdgeBorder.Right, QvQuad2D.From(ref _xRecipe.xLineRight) },
                {EdgeBorder.Bottom, QvQuad2D.From(ref _xRecipe.xLineBottom) },
            };
            var borderKeys = _goldenLineBorderQuads.Keys;

            //(2) goldenLines
            //    (以 _xRecipe.GoldenRegionCellRect 左上為相對零點) 
            var goldenRegionCellBmp = _xRecipe.GoldenRegionCellBmp;
            _goldenLines = new Dictionary<EdgeBorder, EzLSD.LineSegment>();
            foreach (EdgeBorder eb in borderKeys)
            {
                var borderRect = _goldenLineBorderQuads[eb].ToBox2D().BoundaryRect;
                TryFindLineSegment(eb, goldenRegionCellBmp, borderRect, out var mvdLine);
                var lineSegment = mvdLine?.ToLineSegment();
                //lineSegment?.Offset(borderRect.X, borderRect.Y);
                _goldenLines[eb] = lineSegment;
            }

            if (false)
            {
                #region DEBUG_DUMP_I
                using (var bridge = new QxImageBridge(goldenRegionCellBmp))
                using (Mat canvas = bridge.Image.CvtColor(ColorConversionCodes.GRAY2BGR))
                {
                    foreach (EdgeBorder eb in borderKeys)
                    {
                        var border = _goldenLineBorderQuads[eb];
                        var line = _goldenLines[eb];
                        DrawQuad(canvas, border.Corners, Scalar.Blue);
                        DrawLine(canvas, line, Scalar.Cyan);
                    }
                    canvas.SaveImage("d:\\paso.log\\golden_quad_lines.png");
                }
                #endregion

                #region DEBUG_DUMP_II
                using (var bridge = new QxImageBridge(_xRecipe.GoldenRegionCellBmp))
                using (Mat canvas = bridge.Image.CvtColor(ColorConversionCodes.GRAY2BGR))
                {
                    var boundRect = new Rect(0, 0, canvas.Width, canvas.Height);
                    foreach (EdgeBorder eb in borderKeys)
                    {
                        var border = _goldenLineBorderQuads[eb];
                        var lineSegment = _goldenLines[eb];
                        if (lineSegment == null) continue;

                        foreach (var roi in iterChunkRoiRects(eb, border, lineSegment))
                        {
                            var roiC = roi;
                            JetEazy.Qcvt.ClipBoundary(ref roiC, ref boundRect);
                            canvas.Rectangle(roiC, Scalar.LightBlue);
                        }
                    }
                    canvas?.SaveImage($"d:\\paso.log\\golden_chunks_roi.png");
                }
                #endregion
            }

            //(3) Gold Image Chunks
            //    (以 _xRecipe.GoldenRegionCellRect 左上為相對零點)
            _goldenChunkImages = new Dictionary<EdgeBorder, Mat[]>();
            using (var bridge = new QxImageBridge(_xRecipe.GoldenRegionCellBmp))
            {
                var goldenRegionCellImg = bridge.Image;
                var boundRect = new Rect(0, 0, goldenRegionCellImg.Width, goldenRegionCellImg.Height);

                foreach (EdgeBorder eb in borderKeys)
                {
                    var border = _goldenLineBorderQuads[eb];
                    var lineSegment = _goldenLines[eb];
                    if (lineSegment == null)
                    {
                        _goldenChunkImages[eb] = null;
                        continue;
                    }

                    var imgs = new List<Mat>();
                    foreach (var roi in iterChunkRoiRects(eb, border, lineSegment))
                    {
                        var roiC = roi;
                        JetEazy.Qcvt.ClipBoundary(ref roiC, ref boundRect);
                        var img = goldenRegionCellImg[roiC].Clone();
                        imgs.Add(img);
                        
                        // DUMP
                        //img.SaveImage($"d:\\paso.log\\golden_chuck_{eb}_{imgs.Count - 1}.png");
                    }

                    _goldenChunkImages[eb] = imgs.ToArray();
                }
            }
        }
        private IEnumerable<Rect> iterChunkRoiRects(EdgeBorder eb, QvQuad2D borderQuad, EzLSD.LineSegment lineSegment, bool fullThickness = false)
        {
            int NDivs = N_CHUNKS + 1;

            var borderRect = borderQuad.BoundaryRect;
            var bW = borderRect.Width;
            var bH = borderRect.Height;

            QVector beginPt;
            QVector endPt;
            switch (eb)
            {
                case EdgeBorder.Left:
                    beginPt = new QVector2(borderRect.Left, borderRect.Bottom);
                    endPt = new QVector2(borderRect.Left, borderRect.Top);
                    break;
                case EdgeBorder.Top:
                    beginPt = new QVector2(borderRect.Left, borderRect.Top);
                    endPt = new QVector2(borderRect.Right, borderRect.Top);
                    break;
                case EdgeBorder.Right:
                    beginPt = new QVector2(borderRect.Right, borderRect.Top);
                    endPt = new QVector2(borderRect.Right, borderRect.Bottom);
                    break;
                case EdgeBorder.Bottom:
                default:
                    beginPt = new QVector2(borderRect.Right, borderRect.Bottom);
                    endPt = new QVector2(borderRect.Left, borderRect.Bottom);
                    break;
            }

            bool isHorizontalBorder = (eb == EdgeBorder.Top || eb == EdgeBorder.Bottom);

            beginPt = lineSegment.CalcTheNearestPoint(beginPt);
            endPt = lineSegment.CalcTheNearestPoint(endPt);
            var vect = endPt - beginPt;
            var len = vect.NormLength;
            var U = vect / len;

            var strideSpan = len / NDivs;
            var cropSpan = Math.Max(strideSpan * 0.1, 32);

            double x1, y1, x2, y2;
            var center = beginPt + U * strideSpan;
            for (int i = 0; i < N_CHUNKS; i++)
            {
                var p1 = center - U * cropSpan;
                var p2 = center + U * cropSpan;

                if (isHorizontalBorder)
                {
                    x1 = p1.X;
                    x2 = p2.X;
                    y1 = fullThickness ? borderRect.Top : center.Y - bH / 4;
                    y2 = fullThickness ? borderRect.Bottom : center.Y + bH / 4;
                }
                else
                {
                    y1 = p1.Y;
                    y2 = p2.Y;
                    x1 = fullThickness ? borderRect.Left : center.X - bW / 4;
                    x2 = fullThickness ? borderRect.Right : center.X + bW / 4;
                }

                var ix1 = (int)Math.Min(x1, x2);
                var ix2 = (int)Math.Max(x1, x2);
                var iy1 = (int)Math.Min(y1, y2);
                var iy2 = (int)Math.Max(y1, y2);
                var roi = new Rect(ix1, iy1, ix2 - ix1, iy2 - iy1);
                yield return roi;
                center = center + U * strideSpan;
            }
        }
        #endregion

        #region PRIVATE_HELPER_FUNCTIONS
        void fill_border_mid_area(Bitmap cellBmp2, QvQuad2D borderQuad, EdgeBorder eBorder, Scalar color)
        {
            using (var bridge = new QxImageBridge(cellBmp2))
            {
                var cellImg = bridge.Image;

                var midRoi = JetEazy.Qcvt.CV(Rectangle.Round(borderQuad.BoundaryRect));
                if (eBorder == EdgeBorder.Left || eBorder == EdgeBorder.Right)
                {
                    var dh = midRoi.Height / 3;
                    midRoi.Inflate(0, -dh);
                }
                else
                {
                    var dw = midRoi.Width / 3;
                    midRoi.Inflate(-dw, 0);
                }

                GaUtil.Clip(ref midRoi, cellImg.Width, cellImg.Height);
                cellImg[midRoi].SetTo(color);
            }
        }
        QVector[] getChipDimCorners(GaChipData chipData)
        {
            if (chipData != null)
            {
                var points = new List<QVector>();
                var lines = chipData.LineSegments;
                if (lines != null && lines.Length >= 4)
                {
                    for (int i = 0, NP = lines.Length; i < NP; i++)
                    {
                        int j = (i + 1) % NP;
                        var line1 = lines[i];
                        var line2 = lines[j];
                        if (line1 == null || line2 == null) continue;
                        var pt = line1.CalcIntersectedPoint(line2);
                        if (pt == null) continue;
                        points.Add(pt);
                    }
                }

                if (points.Count >= 4)
                {
                    return points.ToArray();
                }
            }
            return null;
        }
        QVector UnitVect(QVector p1, QVector p2)
        {
            var v = p2 - p1;
            return v / v.NormLength;
        }
        void DrawQuad(Mat canvas, QVector[] points, Scalar color)
        {
            for (int i = 0, len = points.Length; i < len; i++)
            {
                int j = (i + 1) % len;
                int x1 = (int)points[i].X;
                int y1 = (int)points[i].Y;
                int x2 = (int)points[j].X;
                int y2 = (int)points[j].Y;
                canvas.Line(x1, y1, x2, y2, color);
            }
        }
        void DrawDots(Mat canvas, Point2f[] points, Scalar color)
        {
            foreach (var pt in points)
                Cv2.Circle(canvas, new OpenCvSharp.Point((int)pt.X, (int)pt.Y), 3, color, -1);
        }
        void DrawLine(Mat canvas, EzLSD.LineSegment line, Scalar color)
        {
            var p1 = line?.P1;
            var p2 = line?.P2;
            if (p1 != null && p2 != null)
            {
                int x1 = (int)p1.X;
                int y1 = (int)p1.Y;
                int x2 = (int)p2.X;
                int y2 = (int)p2.Y;
                canvas.Line(x1, y1, x2, y2, color);
            }
        }
        #endregion

        #region PRIVATE_DUMP_FUNCTIONS
        /// <summary>
        /// LETIAN: 非同步保存 邊框數據
        /// </summary>
        void AsyncDumpLineSegmentsData(RegionCellX3Class cell, ref RectangleF cellRoi)
        {
#if (OPT_LEGACY)
            if (cell == null)
                return;

            var lineBorderBoxes = cell?.ChipData?.LineBorderBoxes;
            var lineSegments = cell?.ChipData?.LineSegments;
            if (lineBorderBoxes == null)
                return;

            var lineBorderBoxesA = Array.ConvertAll(lineBorderBoxes, lb => lb?.Clone());
            var lineSegmentsA = lineSegments != null ?
                                Array.ConvertAll(lineSegments, ls => ls?.Clone()) :
                                null;

            string dumpFolder = System.IO.Path.Combine(RegionCellX3Class.SaveDebugPath, "PositionFix");
            string fname = $"Cell_{cell.Index}@{cell.CellRow}_{cell.CellCol}_lines.json";
            string fullFileName = System.IO.Path.Combine(dumpFolder, fname);

            ThreadPool.QueueUserWorkItem(arg =>
            {
                try
                {
                    object[] args = (object[])arg;
                    string fileName = args[0] as string;
                    lineBorderBoxesA = args[1] as QvBox2D[];
                    lineSegmentsA = args[2] as EzLSD.LineSegment[];
                    var roi = (RectangleF)args[3];

                    var lines = new List<string>();
                    if (lineBorderBoxesA != null)
                    {
                        int idx = 0;
                        foreach (var lb in lineBorderBoxesA)
                        {
                            var sb = new StringBuilder();
                            sb.Append($"\"lineBorderBox_{idx}\" : [");
                            if (lb != null)
                            {
                                // OFFSET
                                var center = lb.Center;
                                var cx = center.X - roi.X;
                                var cy = center.Y - roi.Y;
                                lb.SetCenter(cx, cy);
                                // dump CORNERS
                                var pts = lb.Corners;
                                foreach (var pt in pts)
                                {
                                    sb.Append(pt.X).Append(",").Append(pt.Y).Append(",");
                                }
                            }
                            lines.Add(sb.ToString().TrimEnd(',') + "],");
                            idx++;
                        }
                    }

                    if (lineSegmentsA != null)
                    {
                        int idx = 0;
                        foreach (var ls in lineSegmentsA)
                        {
                            var sb = new StringBuilder();
                            sb.Append($"\"lineSegment_{idx}\" : [");
                            if (ls != null)
                            {
                                // OFFSET
                                ls.Offset(-roi.X, -roi.Y);
                                // dump P1 P2
                                if (ls.P1 != null && ls.P2 != null)
                                {
                                    foreach (var pt in new[] {ls.P1, ls.P2})
                                    {
                                        sb.Append(pt.X).Append(",").Append(pt.Y).Append(",");
                                    }
                                }
                            }
                            lines.Add(sb.ToString().TrimEnd(',') + "],");
                            idx++;
                        }
                    }

                    if (lines.Count > 0)
                    {
                        lines[lines.Count - 1] = lines[lines.Count - 1].TrimEnd(',');
                        lines.Insert(0, "{");
                        lines.Add("}");
                    }

                    System.IO.File.WriteAllLines(fileName, lines, Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    _LOG_ERROR(ex, $"{GetType().Name}.AsyncDumpLineSegmentsData");
                }
            },
                new object[] { fullFileName, lineBorderBoxesA, lineSegmentsA, cellRoi }
            );
#endif
            _lotDataHolder.AsyncDumpLineSegmentsData(cell, ref cellRoi);
        }
        #endregion
    }
}
