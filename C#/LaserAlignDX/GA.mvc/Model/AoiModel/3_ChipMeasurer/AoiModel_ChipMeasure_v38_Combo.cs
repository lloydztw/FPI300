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
using LaserAlignDX.Mvc.Model.Recipe;
using LaserAlignDX.OPSpace;
using LaserAlignDX.OPSpace.RecipeSpace;
using LeTian.AoiLib;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using Traveller106;
using VisionDesigner;
using ErrorCodes = LaserAlignDX.Mvc.Model.ErrorCodes;
using MvdFindLineClass = LaserAlignDX.BasicSpace.MvdFindLineClass;

namespace LaserAlignDX.AoiModel.V38.Combo
{
    /// <summary>
    /// 晶粒尺寸量測
    /// </summary>
    public class AoiModel_ChipMeasure : AoiModelBase, IAoiChipMeasurer
    {
        #region CONFIG
        static bool N_THREADS_ENABLED => GlobalConfig.N_THREADS_ENABLED;
        static int N_THREADS => GlobalConfig.N_THREADS;
        #endregion

        #region RECIPE_PARAMS
        InspectX3ParaClass _xInspectParams => _xRecipe.InspectParams;
        DtoX3LineBorderParams _xLineBorderParams => _xRecipe.LineBorderParams;
        DtoX3LineGapBorderParams _xGapBorderParams => _xRecipe.GapBorderParams;
        bool _needsToMeasureGaps => _xInspectParams.optPadEdgeGapsMeasurement
                                 && _xInspectParams.xAlgorithm == MatchAlgorithmEnum.GridMatch;
        #endregion

        #region KERNEL_MEMBERS
        IMicroChipTransform _microTransform;
        #endregion

        #region MVD_TOOLS
        MvdFindLineClass[] _mvdLineFinders;
        #endregion

        #region RUNTIME_DATA
        GaCellsGroup[] _cellGroups;
        bool _is2ndRun;
        #endregion

        public override void Dispose()
        {
            base.Dispose();
            disposeMvdLineFinders();
        }

        /// <summary>
        /// 設定需要量測的 Cells 群組
        /// </summary>
        public void SetCellGroups(GaCellsGroup[] cellGroups)
        {
            this._cellGroups = cellGroups;
        }

        public override void Run(Bitmap sceneBmp = null)
        {
            bool go = _xRecipe.InspectParams.optChipMeasurement; 
            if (!go)
                return;

            try
            {
                fire_AoiBegin();
                markRunStart();

                //(1) MicroTransform
                var activeCarrierID = getActiveCarrierID();
                _microTransform = _sysModel.GetMicroTransform(activeCarrierID);
                if (_microTransform == null)
                    throw new Exception($"無法取得 Micro Transform ({activeCarrierID})");

                //(2) FullFov Bitmap
                Bitmap bmpFullfov = LineScanCamImageHolder.PeekBitmap();
                
                //(3) 第一次量測尺寸
                RunChipsMeasurement(bmpFullfov);

                //(4) 再次量測尺寸
                RunChipsMeasurement2ndForNGs(bmpFullfov);

                markRunEnd(true);
                fire_AoiEnd();
            }
            catch (Exception ex)
            {
                // 2025-08-28 LETIAN: 巨圖統一由 LineScanCamImageHolder 管理其生命週期, 在此無需釋放 巨圖!!!
                base.HandleAoiException(ex);
            }
        }

        #region 參數調適用函式
        public void TryMeasureOneChip(RegionCellX3Class cell, Bitmap cellBmp, ref RectangleF cellRoi)
        {
            if (_microTransform == null)
                _microTransform = _sysModel.GetMicroTransform(getActiveCarrierID());
            
            if (_microTransform == null)
                return;

            RunOneChipMeasurement(cell, cellBmp, ref cellRoi, 0);
        }

        public bool TryFindLineSegment(EdgeBorder eBorder, Bitmap bmpSrc, RectangleF roiRect, out CMvdLineSegmentF resultLine)
        {
            ////--------------------------------------------------------------
            //// 海康邊線 準確度 深受 前景背景 對比 影響
            ////--------------------------------------------------------------
            
            prepareMvdLineFinders(N_THREADS);

            var lineSegFinder = _mvdLineFinders[0];
            lineSegFinder.Background = _xInspectParams.xCarrierBackground;

            if (TryApplyLineFilters(bmpSrc, out Bitmap bmpWork))
            {
                resultLine = lineSegFinder.Run(bmpWork, roiRect, (int)eBorder);
                bmpWork.Dispose();
            }
            else
            {
                resultLine = lineSegFinder.Run(bmpSrc, roiRect, (int)eBorder);
            }

            return resultLine != null;
        }

        public bool TryApplyLineFilters(Bitmap bmpSrc, out Bitmap bmpResult, Color? backGroundColor = null)
        {
            //>>> bmpResult = null;
            //>>> return false;

            if (needsToApplyGrayLimits() && bmpSrc != null && bmpSrc.Width > 2 && bmpSrc.Height > 2)
            {
                bmpResult = (Bitmap)bmpSrc.Clone();
                applyFiltersForGrayLimits(bmpResult, backGroundColor);
                return true;
            }
            else
            {
                bmpResult = null;
                return false;
            }
        }

        public void AnalyzeGoldenData()
        {
            // RESERVED
        }
        #endregion

        #region PRIVATE_GROUP_MEASURE_FUNCTIONS

        /// <summary>
        /// LETIAN: 晶粒定位 與 尺寸量測 
        /// </summary>
        private void RunChipsMeasurement(Bitmap bmpFullfov)
        {
            if (!_xInspectParams.optChipMeasurement)
                return;

            _is2ndRun = false;

            fire_AoiBegin("晶粒尺寸量測");

            //_TM.RESET_ACCUM();

            bool usingMultiThread = N_THREADS_ENABLED;

            #region 準備_CELL_GROUPS
            int N_GROUPS = _cellGroups != null ? _cellGroups.Length : MvdCompositeChipMatcher.N_CHANNLS;
            var groups = _cellGroups != null ? _cellGroups : GaCellsGroup.CollectGroups(N_GROUPS, _xRecipe, bmpFullfov);
            #endregion

            prepareMvdLineFinders(Universal.N_THREADS);

            if (groups == null || groups.Length == 0)
                return;

            if (!usingMultiThread)
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
            if (!_xInspectParams.optChipMeasurement)
                return;

            _is2ndRun = true;

            fire_AoiBegin("晶粒尺寸量測 (二次補測)");

            //_TM.RESET_ACCUM();

            bool usingMultiThread = Universal.N_THREADS_ENABLED;

            #region 準備_CELL_GROUPS
            int N = _cellGroups != null ? _cellGroups.Length : MvdCompositeChipMatcher.N_CHANNLS;
            var ngGroups = GaCellsGroup.CollectGroups(N, _xRecipe, bmpFullfov, option: "NG_DIM_ONLY");
            #endregion

            if (ngGroups == null || ngGroups.Length == 0)
                return;

            if (!usingMultiThread)
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
            //(0) 取得所有 尺寸量測 名稱
            var measureKeyNames = _xLineBorderParams.LineBorderPairs.Keys;

            foreach (var gaCell in cellsGroup)
            {
                RegionCellX3Class cell = gaCell?.Cell;

                //(1) 進度條事件
                fire_AoiProgressing(cell);

                //(2) 晶粒定位數據
                var chipData = cell?.ChipData;
                if (chipData == null)
                    continue;

                //(3) 重置 晶粒 尺寸量測 數據
                chipData.ChipDimension.Reset(measureKeyNames);
                chipData.LineBorderPairs.Clear();
                chipData.GapBorderPairs.Clear();

                //(4) 進行 晶粒 尺寸量測
                bool go = cell.ChipData.ChipQuad2D != null && cell.IsResultPass();
                if (go)
                {
                    //(4.0) cellBmp and cellRoi
                    Bitmap cellBmp = gaCell.CellBmp;
                    RectangleF cellRoi = gaCell.CellRoi;

                    //(4.1) 量測 單一晶粒
                    RunOneChipMeasurement(cell, cellBmp, ref cellRoi, threadIdx);

                    //(4.2) 打包 該晶粒 量測數據
                    PackMeasureResult(cell);
                }
            }
        }

        #endregion

        #region PRIVATE_CELL_MEASURE_FUNCTIONS
        /// <summary>
        /// 量測單一晶粒 (直线寻找)
        /// </summary>
        private void RunOneChipMeasurement(RegionCellX3Class cell, Bitmap cellBmp, ref RectangleF cellRoi, int threadId)
        {
#if (OPT_ORIGINAL_OK_CODE)
            // 取得 上一輪 晶粒定位 的結果 (chipData)
            var chipData = cell?.ChipData;
            if (chipData == null || chipData.ChipQuad2D == null)
                return;

            #region 邊線處理
            var mvdLineFinder = _mvdLineFinders[threadId % _mvdLineFinders.Length];
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
                        //using (Bitmap cellBmp2 = (Bitmap)cellBmp.Clone())
                        //{
                        //    // 塗掉中段 (1/3) 
                        //    fill_border_mid_area(cellBmp2, borderQuad, eBorder, Scalar.Black);
                        //    // 海康線檢(輸出為 cell.cMvdLineSegmentFsOut)
                        //    mvdLine = _RunMvdLineFinder(mvdLineFinder, borderIdx, cellBmp2, mvdRoi, chipData.ChipQuad2D.Angle);
                        //}

                        //// 如果塗掉中段 (1/3) 仍然抓不到, 回過頭使用 原來的方法
                        //if (mvdLine == null)
                        //{
                        //    mvdLine = _RunMvdLineFinder(mvdLineFinder, borderIdx, cellBmp, mvdRoi, chipData.ChipQuad2D.Angle);
                        //}
                    }

                    //(3) 將 CMvdLine 轉換成 EzLSD.LineSegment
                    var line = mvdLine?.ToLineSegment();
                    //(3.1) 轉換至 (Fullfov Camera Coorindates)
                    line?.Offset(cellRoi.X, cellRoi.Y);
                    //(3.2) 記入 cell.ChipData
                    chipData.LineBorderPairs.Set(eBorder, line);

                    //(4) 更新 chipData.LineBorderPairs
                    //(4.1) 轉換至 (Fullfov Camera Coorindates)
                    borderQuad.Offset(cellRoi.X, cellRoi.Y);
                    //(4.2) 記入 cell.ChipData
                    chipData.LineBorderPairs.Set(eBorder, borderQuad.ToBox2D());

                    ////(4.3) 更新到 cell 舊的 Gaara Data (廢除)
                    //cell.cMvdShapesForFindLineRegion[borderIdx] = borderQuad.ToCMvdRectangleF();
                }

                AsyncDumpLineSegmentsData(cell, ref cellRoi);
            }
            catch (Exception ex)
            {
                string borderName = JetEazy.QxNums.GetEnumDescription(eBorder);
                _LOG_ERROR(ex, $"{borderName} 定位異常");
                throw ex;
            }
            #endregion

            #region 尺寸X_與_尺寸Y_量測計算
            try
            {
                //(1) LineSegments (Fullfov Camera Coordinates) 
                var lines = chipData.LineBorderPairs.GetQuadLineSegments(true);

                //(2) 使用 Micro Transform 計算 尺寸 與 邊隙
                //    (結果會直接存入 cell.ChipData 內)
                bool toMeasureGaps = _xInspect.optPadEdgeGapsMeasurement && _xInspect.xAlgorithm == MatchAlgorithmEnum.GridMatch;
                _microTransform.UseAveGaps4 = _xRecipe.GapBorderParams.UseAveGaps4;
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
            return;
#endif
            if (_xLineBorderParams.IsSimpleQuad)
            {
                // 抓取 4邊線 框線數據 
                bool go = FetchChipRuntimeLineBorders4(cell, cellBmp, ref cellRoi, threadId);

                // 抓取 8邊隙 框線數據 
                if (go && _needsToMeasureGaps)
                    FetchChipRuntimeGapBorders8(cell, cellBmp, ref cellRoi, threadId);

                // 尺寸量測計算 (尺寸X and 尺寸Y)
                if (go)
                    CalcDimensionXY(cell, cellBmp, ref cellRoi, _needsToMeasureGaps);
            }
            else
            {
                // 抓取 邊線 框線數據 (具名量測尺寸) 
                bool go = FetchChipRuntimeLineBordersG(cell, cellBmp, ref cellRoi, threadId);

                // 尺寸量測計算 (其他具名量測尺寸)
                if (go)
                    CalcNamedMeasurements(cell, cellBmp, ref cellRoi);
            }
        }
        
        /// <summary>
        /// 計算 尺寸X (寬) 與 尺寸Y (高)
        /// </summary>
        private bool CalcDimensionXY(RegionCellX3Class cell, Bitmap cellBmp, ref RectangleF cellRoi, bool includeGaps)
        {
            //(0) 取得 上一輪 晶粒定位 的結果 (chipData)
            var chipData = cell?.ChipData;
            if (chipData == null || chipData.ChipQuad2D == null)
                return false;

            try
            {
                //(1) LineSegments (Fullfov Camera Coordinates) 
                var lines = chipData.LineBorderPairs.GetQuadLineSegments(true);

                //(2) 使用 Micro Transform 計算 尺寸 與 邊隙
                //    (結果會直接存入 cell.ChipData 內)
                _microTransform.UseAveGaps4 = _xRecipe.GapBorderParams.UseAveGaps4;
                var err = _microTransform.CalcChipDimension(out SizeF dimension, lines, chipData, includeGaps);

                //(3) 記入結果
                cell.RunWidth = dimension.Width;
                cell.RunHeight = dimension.Height;

                //(4) 異常
                if (err != ErrorCodes.OK)
                    throw new Exception(GaUtil.GetEnumDescription(err));

                return true;
            }
            catch (Exception ex)
            {
                _LOG_ERROR(ex, "晶粒 尺寸量測 異常 @ CalcDimensionXY");
                return false;
            }
        }

        /// <summary>
        /// 計算 具名量測之尺寸
        /// </summary>
        private bool CalcNamedMeasurements(RegionCellX3Class cell, Bitmap cellBmp, ref RectangleF cellRoi)
        {
            //(0) 取得 上一輪 晶粒定位 的結果 (chipData)
            var chipData = cell?.ChipData;
            if (chipData == null || chipData.ChipQuad2D == null)
                return false;

            try
            {
                //(1) 使用 Micro Transform 計算 各種 具名量測 之尺寸
                var carrierID = _sysModel.ActiveCarrierID;
                var globalTrf = _sysModel.TransformsModel.GetCameraPhysicTransform(carrierID);

                var err = _microTransform.CalcChipMeasurements(out var results, chipData.LineBorderPairs, chipData, globalTrf);

                //(2) 異常
                if (err != ErrorCodes.OK)
                    throw new Exception(GaUtil.GetEnumDescription(err));

                return true;
            }
            catch (Exception ex)
            {
                _LOG_ERROR(ex, "晶粒 尺寸量測 異常 @ CalcNamedMeasurements");
                return false;
            }
        }

        /// <summary>
        /// 整理打包 尺寸计算 的 最後判定结果
        /// </summary>
        /// <returns>true:OK false:NG</returns>
        private void PackMeasureResult(RegionCellX3Class cell)
        {
            if (cell == null)
                return;

            bool isAllPass = true;

            // 檢查是否有啟用 尺寸量測
            if (_xInspectParams.optChipMeasurement)
            {
                var chipDim = cell.ChipData?.ChipDimension;

                //(A) 原有 判定方式 (長寬)
                if (_xRecipe.LineBorderParams.IsSimpleQuad)
                {
                    //(A1) 判定 長寬 是否達標
                    bool ok_x = !(cell.RunWidth < _xInspectParams.mWidthStandMin || cell.RunWidth > _xInspectParams.mWidthStandMax);
                    bool ok_y = !(cell.RunHeight < _xInspectParams.mHeightStandMin || cell.RunHeight > _xInspectParams.mHeightStandMax);

                    chipDim?.UpdateMeasurement("X", ok_x);
                    chipDim?.UpdateMeasurement("Y", ok_y);
                    isAllPass &= ok_x && ok_y;

                    if (!isAllPass)
                    {
                        cell.MarkResult(InspectReason.NG_CUT);
                    }

                    //(A2) 判定 邊隙 是否達標
                    if (_needsToMeasureGaps)
                    {
                        var gaps = cell.ChipData?.PadEdgeGaps;
                        if (gaps == null)
                        {
                            isAllPass = false;
                        }
                        else
                        {
                            var min = new QVector(_xInspectParams.PadEdgeGapX_Min, _xInspectParams.PadEdgeGapY_Min);
                            var max = new QVector(_xInspectParams.PadEdgeGapX_Max, _xInspectParams.PadEdgeGapY_Max);
                            var maxDiff = _xInspectParams.PadEdgeX_Diff_Upper;
                            gaps.Check(out isAllPass, min, max, maxDiff);
                        }

                        if (!isAllPass)
                        {
                            cell.MarkResult(InspectReason.NG_EDGE_GAP);
                        }
                    }
                }

                //(B) 其他 判定方式 (逐項)
                else
                {
                    foreach (var key in chipDim.Keys)
                    {
                        var measurement = chipDim[key];
                        if (measurement == null) continue;

                        var dist = measurement.Value;

                        bool ok_x = key.StartsWith("X")
                            ? !(dist < _xInspectParams.mWidthStandMin || dist > _xInspectParams.mWidthStandMax)
                            : !(dist < _xInspectParams.mHeightStandMin || dist > _xInspectParams.mHeightStandMax);

                        measurement.IsPass = ok_x;
                        isAllPass &= ok_x;
                    }

                    if (!isAllPass)
                    {
                        cell.MarkResult(InspectReason.NG_CUT);
                    }
                }
            }

            // 強制設定 Stampede (踩腳)
            if (_xInspectParams.optTiltDetectEnabled)
            {
                var chipData = cell.ChipData;
                bool isStampede = (chipData != null && chipData.ChipCoords.IsStampede);
                if (isStampede)
                    cell.MarkResult(InspectReason.NG_AMBIGUOUS_BLOC);
            }
        }
        #endregion

        #region PRIVATE_LINE_AND_GAP_BORDERS_FUNCTIONS
        /// <summary>
        /// 抓取 邊線 的 Runtime 具名 框線數據, 併記入 ChipData.LineBorderPairs 欄位內. 
        /// </summary>
        private bool FetchChipRuntimeLineBordersG(RegionCellX3Class cell, Bitmap cellBmp, ref RectangleF cellRoi, int threadId)
        {
            //(0) 取得 上一輪 晶粒定位 的結果 (chipData)
            var chipData = cell?.ChipData;
            if (chipData == null || chipData.ChipQuad2D == null)
                return false;

            //(1) 清除 Runtime 邊線框 數據
            chipData.LineBorderPairs.Clear();

            //(2) 海康 邊線定位器
            var mvdLineFinder = _mvdLineFinders[threadId % _mvdLineFinders.Length];

            string keyName = "";
            try
            {
                //(1) 計算出 Runtime 邊線框對
                var lineBorderPairs = CalcRuntimeLocalBorderPairs(_xLineBorderParams.LineBorderPairs, cell, cellRoi, cellBmp);

                //(2) Pair by pair
                foreach ((var key, var pair) in lineBorderPairs.IterPairs())
                {
                    keyName = key;

                    bool isHoriz = keyName.StartsWith("X");
                    int borderIdx = isHoriz ? 0 : 1;

                    for (int ib = 0; ib < 2; ib++, borderIdx += 2)
                    {
                        //(2.1) borderQuad
                        var borderQuad = QvQuad2D.From(pair.Borders[ib]);
                        var mvdRoi = borderQuad.ToCMvdRectangleF();

                        //(2.2) 海康線檢 I
                        CMvdLineSegmentF mvdLine = null;
                        if (true)   // if (!_is2ndRun)
                        {
                            // 海康線檢
                            mvdLine = _RunMvdLineFinder(mvdLineFinder, borderIdx, cellBmp, mvdRoi, chipData.ChipQuad2D.Angle);
                        }

                        //(3) 將 CMvdLine 轉換成 EzLSD.LineSegment
                        var line = mvdLine?.ToLineSegment();

                        //(4) 加回 ROI Offset (轉換到 FullFov Camera Coordinates)
                        line?.Offset(cellRoi.X, cellRoi.Y);
                        borderQuad.Offset(cellRoi.X, cellRoi.Y);

                        //(5) 記入 cell.ChipData 的 LineBorderPairs 欄位
                        chipData.LineBorderPairs[keyName] = pair;
                        pair.Borders[ib] = borderQuad.ToBox2D();
                        pair.LineSegments[ib] = line;
                    }
                }

                AsyncDumpLineSegmentsData(cell, ref cellRoi);
                return true;
            }
            catch (Exception ex)
            {
                _LOG_ERROR(ex, $"{keyName} 計算邊線異常");
                throw;
            }
        }

        /// <summary>
        /// 抓取 4邊線 的 Runtime 框線數據, 併記入 ChipData.LineBorderPairs 欄位內. 
        /// </summary>
        private bool FetchChipRuntimeLineBorders4(RegionCellX3Class cell, Bitmap cellBmp, ref RectangleF cellRoi, int threadId)
        {
            //(0) 取得 上一輪 晶粒定位 的結果 (chipData)
            var chipData = cell?.ChipData;
            if (chipData == null || chipData.ChipQuad2D == null)
                return false;

            //(1) 清除 Runtime 邊線框 數據
            chipData.LineBorderPairs.Clear();

            //(2) 海康 邊線定位器
            var mvdLineFinder = _mvdLineFinders[threadId % _mvdLineFinders.Length];

            EdgeBorder eBorder = EdgeBorder.Left;
            try
            {
                //(3) 計算出 Runtime 邊線框
                var lineBorderQuads = CalcRuntimeLocalLineBorderQuads(cell, cellRoi, cellBmp);
                if (lineBorderQuads == null)
                    return false;

                //(4) 巡訪每一個 EdgeBorder (順序: 左,上,右,下)
                for (int borderIdx = 0, N = lineBorderQuads.Length; borderIdx < N; borderIdx++)
                {
                    //(4.1) enum
                    eBorder = (EdgeBorder)borderIdx;

                    //(4.2) borderQuad
                    var borderQuad = lineBorderQuads[borderIdx];
                    var mvdRoi = borderQuad.ToCMvdRectangleF();

                    //(4.3) 海康線檢 I
                    CMvdLineSegmentF mvdLine;
                    if (true)   // if (!_is2ndRun)
                    {
                        // 海康線檢
                        mvdLine = _RunMvdLineFinder(mvdLineFinder, borderIdx, cellBmp, mvdRoi, chipData.ChipQuad2D.Angle);
                    }
                    //(4.3.*) 海康線檢 II (暫時不使用)
                    else
                    {
                        #region RESERVED_CODE
                        //using (Bitmap cellBmp2 = (Bitmap)cellBmp.Clone())
                        //{
                        //    // 塗掉中段 (1/3) 
                        //    fill_border_mid_area(cellBmp2, borderQuad, eBorder, Scalar.Black);
                        //    // 海康線檢(輸出為 cell.cMvdLineSegmentFsOut)
                        //    mvdLine = _RunMvdLineFinder(mvdLineFinder, borderIdx, cellBmp2, mvdRoi, chipData.ChipQuad2D.Angle);
                        //}

                        //// 如果塗掉中段 (1/3) 仍然抓不到, 回過頭使用 原來的方法
                        //if (mvdLine == null)
                        //{
                        //    mvdLine = _RunMvdLineFinder(mvdLineFinder, borderIdx, cellBmp, mvdRoi, chipData.ChipQuad2D.Angle);
                        //}
                        #endregion
                    }

                    //(4.4) 將 CMvdLineSegmentF 轉換成 EzLSD.LineSegment
                    var line = mvdLine?.ToLineSegment();

                    //(5) 將 抓到的線框 轉換至 Fullfov Camera Coorindates
                    line?.Offset(cellRoi.X, cellRoi.Y);
                    borderQuad?.Offset(cellRoi.X, cellRoi.Y);

                    //(6) 將 抓到的線框 記入到 cell.ChipData
                    chipData.LineBorderPairs.Set(eBorder, line);
                    chipData.LineBorderPairs.Set(eBorder, borderQuad.ToBox2D());
                }

                //(7) 非同步保存調適數據
                AsyncDumpLineSegmentsData(cell, ref cellRoi);
                return true;
            }
            catch (Exception ex)
            {
                string borderName = JetEazy.QxNums.GetEnumDescription(eBorder);
                _LOG_ERROR(ex, $"定位異常 @ FetchChipRuntimeLineBorders [{borderName}]");
                throw;
            }
        }

        /// <summary>
        /// 抓取 邊隙 的 Runtime 框線數據, 併記入 ChipData.GapBorderPairs 欄位內. 
        /// </summary>
        private bool FetchChipRuntimeGapBorders8(RegionCellX3Class cell, Bitmap cellBmp, ref RectangleF cellRoi, int threadId)
        {
            //(0) 取得 上一輪 晶粒定位 的結果 (chipData)
            var chipData = cell?.ChipData;
            if (chipData == null || chipData.ChipQuad2D == null)
                return false;

            //(1) 清除 Runtime 邊隙框 數據
            chipData.GapBorderPairs.Clear();

            //(1.*) 如果不需要量測邊隙, 直接返回 true
            if (!_needsToMeasureGaps)
                return true;

            //(2) 海康 邊線定位器
            var mvdLineFinder = _mvdLineFinders[threadId % _mvdLineFinders.Length];

            //(3) 計算出 Runtime 邊隙框
            var gapBorderPairs = CalcRuntimeLocalBorderPairs(_xGapBorderParams.GapBorderPairs, cell, cellRoi, cellBmp);
            if (gapBorderPairs == null)
                return false;

            //(4) 巡訪每一個 GapEnum
            foreach (GapEnum gap in Enum.GetValues(typeof(GapEnum)))
            {
                var key = gap.ToString();
                var runtimePair = gapBorderPairs[key];

                //(4.1) borderQuad
                var borderQuad = QvQuad2D.From(runtimePair.Borders[0]);

                //(4.2) 海康線檢
                CMvdLineSegmentF mvdLine = null;

                if (true)   // if (!_is2ndRun)
                {
                    // 海康線檢
                    int borderIdx = (int)gap.GetBorderID();
                    var mvdRoi = borderQuad.ToCMvdRectangleF();
                    mvdLine = _RunMvdLineFinder(mvdLineFinder, borderIdx, cellBmp, mvdRoi, chipData.ChipQuad2D.Angle, forceDarkBackgroud: true);
                }

                //(5) LineSegment
                //(5.0) 將 CMvdLineSegmentF 轉換至 EzLSD.LineSegment
                var line = mvdLine?.ToLineSegment();
                //(5.1) 轉換至 Fullfov Camera Coorindates
                line?.Offset(cellRoi.X, cellRoi.Y);
                runtimePair.LineSegments[0] = line;
                runtimePair.LineSegments[1] = null;

                //(6) BorderQuad
                //(6.1) 轉換至 Fullfov Camera Coorindates
                borderQuad.Offset(cellRoi.X, cellRoi.Y);
                //runtimePair.Borders[0] = borderQuad.ToBox2D();
                //runtimePair.Borders[0] = null;

                //(7) 將 抓到的線框 記入到 cell.ChipData 
                chipData.GapBorderPairs[key] = runtimePair;
            }

            return true;
        }

        /// <summary>
        /// 計算 Runtime (旋轉+平移) 後的 拉框對 (順序: 左,上,右,下) (Local Region Coordinates)
        /// </summary>
        private QvQuad2D[] CalcRuntimeLocalLineBorderQuads(RegionCellX3Class cell, RectangleF cellRoi, Bitmap cellBmp = null, bool debug = false)
        {
#if (OPT_ORIGINAL_OK)
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
                    QvQuad2D.From(_xRecipe.xLineLeft),
                    QvQuad2D.From(_xRecipe.xLineTop),
                    QvQuad2D.From(_xRecipe.xLineRight),
                    QvQuad2D.From(_xRecipe.xLineBottom),
                };

                //(2) 將 goldenQuad 平移到 goldenRegionRect (_xRecipe.xRectRegionPrint) 坐標系
                var goldenChipTemplateRoi = _xRecipe.xRegionTrain;
                goldenQuad.Offset(goldenChipTemplateRoi.X, goldenChipTemplateRoi.Y);

                //(2.1) DEBUG
                #region DEBUG
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

                //(3.1) DEBUG
                #region DEBUG
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

#endif
            var rcpLineBorderPairs = _xRecipe.LineBorderParams.LineBorderPairs;
            var pairs = CalcRuntimeLocalBorderPairs(rcpLineBorderPairs, cell, cellRoi, cellBmp, debug);
            return new QvQuad2D[]
            {
                QvQuad2D.From(pairs["X"].Borders[0]),
                QvQuad2D.From(pairs["Y"].Borders[0]),
                QvQuad2D.From(pairs["X"].Borders[1]),
                QvQuad2D.From(pairs["Y"].Borders[1]),
            };
        }

        /// <summary>
        /// 計算 Runtime (旋轉+平移) 後的 拉框對 (Local Region Coordinates)
        /// </summary>
        /// <param name="rcpBorderPairs">輸入: 參數檔內的拉框數據對</param>
        /// <param name="cell">輸入: Region Cell 數據包</param>
        /// <param name="cellRoi">輸入: Region Cell 矩形範圍 (Fullfov Camera Coordinates)</param>
        /// <param name="cellBmp">輸入: Region Cell Bitmap</param>
        /// <param name="debug">輸入: 除錯旗標</param>
        /// <returns>旋轉+平移 後的 拉框對</returns>
        private LineBorderPairsCollection CalcRuntimeLocalBorderPairs(LineBorderPairsCollection rcpBorderPairs, RegionCellX3Class cell, RectangleF cellRoi, Bitmap cellBmp = null, bool debug = false)
        {
            if (rcpBorderPairs == null)
                return null;

            //(1) 取得 上一輪 晶粒定位 的結果 (Fullfov Camera Coorindates)
            //    並複製 chipQuad 與 goldenQuad 
            var chipData = cell?.ChipData;
            var chipQuad = chipData?.ChipQuad2D?.Clone();
            var goldenQuad = chipData?.GoldenQuad2D?.Clone();
            if (chipQuad == null || goldenQuad == null)
                return null;

            string keyName = "";

            try
            {
                //(2) 將 goldenQuad 平移到 Local Region Coordinate 坐標系
                var goldenChipBmpRoi = _xRecipe.GoldenChipRect;
                goldenQuad.Offset(goldenChipBmpRoi.X, goldenChipBmpRoi.Y);

                //(2.1) DEBUG_DUMP
                #region DEBUG_DUMP
                if (debug)
                {
                    var localChipQuad = chipQuad?.Clone();
                    localChipQuad.Offset(-cellRoi.X, -cellRoi.Y);
                    _DUMP("LB_runtime_chip_quad", cellBmp, chipQuad, Scalar.Lime);
                    _DUMP("LB_golden_quad", _xRecipe.GoldenRegionCellBmp, goldenQuad, Scalar.Gold);
                }
                #endregion

                //(3) 準備收納器
                var result = new LineBorderPairsCollection(true);

                int cornersNumber = 4;
                foreach (string key in rcpBorderPairs.Keys)
                {
                    keyName = key;
                    var pairNew = rcpBorderPairs[keyName].Clone();

                    //(3.1) 計算 Borders 與 goldenQuad 邊線, 相對距離
                    bool isHoriz = keyName.StartsWith("X") || keyName.EndsWith("X");

                    int cornerID = isHoriz ? 0 : 1;
                    for (int ib = 0; ib < 2; ib++, cornerID += 2)
                    {
                        int preCornerID = (cornerID == 0) ? (cornersNumber - 1) : (cornerID - 1);
                        
                        //(3.1.1) goldenQuad 邊線中點 (Local Region Coordinates)
                        var goldenLineMid = (goldenQuad.Corners[cornerID] + goldenQuad.Corners[preCornerID]) / 2.0;
                        var v = UnitVect(goldenLineMid, goldenQuad.Center);
                        var u = UnitVect(goldenLineMid, goldenQuad.Corners[cornerID]);

                        QvQuad2D borderNew = QvQuad2D.From(pairNew.Borders[ib]);
                        var lbCorners = Array.ConvertAll(borderNew.Corners, c => c - goldenLineMid);
                        var lbCornersU = Array.ConvertAll(lbCorners, c => c * u);
                        var lbCornersV = Array.ConvertAll(lbCorners, c => c * v);

                        //(3.1.2) chipQuad 邊線中點 (Fullfov Camera Coordinates)
                        var runtimeLineMid = (chipQuad.Corners[cornerID] + chipQuad.Corners[preCornerID]) / 2.0;
                        var V = UnitVect(runtimeLineMid, chipQuad.Center);
                        var U = UnitVect(runtimeLineMid, chipQuad.Corners[cornerID]);
                        //>>> debugPoints.Add(runtimeMid);

                        //(3.1.3) 把 lbCorners 映射到 runtimeLineMid 上面
                        for (int k = 0; k < cornersNumber; k++)
                        {
                            lbCorners[k] = runtimeLineMid + (U * lbCornersU[k]) + (V * lbCornersV[k]);
                        }
                        borderNew.Corners = lbCorners;
                        borderNew.Sort();

                        //(3.2) 轉換到 Local Region Coordinates
                        borderNew.Offset(-cellRoi.X, -cellRoi.Y);

                        //(3.3) 更新 pairNew
                        pairNew.Borders[ib] = borderNew.ToBox2D();
                    }

                    //(3.4) 更新 borderPairs
                    result[keyName] = pairNew;
                }

                //(4) DEBUG_DUMP_II
                #region DEBUG_DUMP_II
                if (debug)
                {
                    _DUMP("", cellBmp, result, Scalar.HotPink);
                }
                #endregion

                return result;
            }
            catch (Exception ex)
            {
                _LOG_ERROR(ex, $"CalcRuntimeLocalBorderPairs 計算異常 @ {keyName}");
                throw ex;
            }
        }
        #endregion

        #region FILTER_FUNCTIONS
        bool needsToApplyGrayLimits()
        {
            return _xInspectParams != null && _xInspectParams.GrayLimitHi < 255 || _xInspectParams.GrayLimitLo > 0;
        }
        void applyFiltersForGrayLimits(Bitmap bmpWork, Color? backGroundColor = null)
        {
            var grayLimitHi = (byte)Math.Max(_xInspectParams.GrayLimitHi, _xInspectParams.GrayLimitLo);
            var grayLimitLo = (byte)Math.Min(_xInspectParams.GrayLimitHi, _xInspectParams.GrayLimitLo);

            using (var bridge = new QxImageBridge(bmpWork))
            using (var mask = createGrayMask(bridge.Image, grayLimitLo, grayLimitHi))
            {
                var img = bridge.Image;
                var fillColor = backGroundColor != null ?
                    new Scalar(backGroundColor.Value.R, backGroundColor.Value.G, backGroundColor.Value.B) :
                    sampleBackgroundColor(img);
                img.SetTo(fillColor, mask);
            }
        }
        Mat createGrayMask(Mat src, byte grayLimitLo, byte grayLimitHi)
        {
            Mat srcGray;

            if (src.Type() == MatType.CV_8UC3)
            {
                srcGray = new Mat();
                Cv2.CvtColor(src, srcGray, ColorConversionCodes.BGR2GRAY);
            }
            else if (src.Type() == MatType.CV_8UC1)
            {
                srcGray = src;
            }
            else if (src.Type() == MatType.CV_8UC4)
            {
                srcGray = new Mat();
                Cv2.CvtColor(src, srcGray, ColorConversionCodes.BGRA2GRAY);
            }
            else
            {
                throw new ArgumentException("輸入影像必須為 CV_8UC1、CV_8UC3 或 CV_8UC4");
            }

            Mat mask = new Mat();

            // 將像素值在 [grayLimitLo, grayLimitHi] 範圍內的設為 255 (白色)，其餘設為 0 (黑色)
            Cv2.InRange(
                srcGray,
                Scalar.All(grayLimitLo),
                Scalar.All(grayLimitHi),
                mask
            );

            // 反向掩膜，使得在範圍內的像素為 0，範圍外的像素為 255
            Cv2.BitwiseNot(mask, mask);

            // CleanUp
            if (srcGray != src)
                srcGray?.Dispose();

            return mask;
        }
        Scalar sampleBackgroundColor(Mat src)
        {
            // 取樣區域的大小
            int wh = Math.Min(src.Width, src.Height);
            int sampleSize = Math.Max(16, wh / 50);
            sampleSize = Math.Min(sampleSize, wh / 4);
            if (sampleSize < 8)
                return Scalar.Black;

            // 計算取樣區域的 左上角
            Rect roi = new Rect(0, 0, sampleSize, sampleSize);
            Scalar mean1 = Cv2.Mean(src[roi]);
            // 計算取樣區域的 右上角
            roi = new Rect(src.Width - sampleSize, 0, sampleSize, sampleSize);
            Scalar mean2 = Cv2.Mean(src[roi]);
            // 計算取樣區域的 右下角
            roi = new Rect(src.Width - sampleSize, src.Height - sampleSize, sampleSize, sampleSize);
            Scalar mean3 = Cv2.Mean(src[roi]);
            // 計算取樣區域的 左下角
            roi = new Rect(0, src.Height - sampleSize, sampleSize, sampleSize);
            Scalar mean4 = Cv2.Mean(src[roi]);

            // 計算四個角落的平均顏色
            Scalar meanColor = new Scalar(
                (mean1.Val0 + mean2.Val0 + mean3.Val0 + mean4.Val0) / 4.0,
                (mean1.Val1 + mean2.Val1 + mean3.Val1 + mean4.Val1) / 4.0,
                (mean1.Val2 + mean2.Val2 + mean3.Val2 + mean4.Val2) / 4.0
            );
            return meanColor;
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
        CMvdLineSegmentF _RunMvdLineFinder(IMvdLineFinder mvdLineFinder, int borderIndex, Bitmap bmp, CMvdRectangleF roi, double angleRef = 0, bool forceDarkBackgroud = false)
        {
            int NP = 4;

            #region (1) 根據 angleRef 將 borderIndex 正規化
            // 根據 angleRef 將 borderIndex 正規化
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
            #endregion

            #region (2) 根據 sideIndex 設定 mvdLineFinder 的參數
            // 左
            if (sideIndex == 0)
            {
                mvdLineFinder.bPositive = _xInspectParams.bPositive0;
                mvdLineFinder.bFindOrient = true;
                mvdLineFinder.bEdgePolarity = _xInspectParams.bEdgePolarity0;
            }
            // 上
            else if (sideIndex == 1)
            {
                mvdLineFinder.bPositive = _xInspectParams.bPositive1;
                mvdLineFinder.bFindOrient = false;
                mvdLineFinder.bEdgePolarity = _xInspectParams.bEdgePolarity1;
            }
            // 右
            else if (sideIndex == 2)
            {
                mvdLineFinder.bPositive = _xInspectParams.bPositive2;
                mvdLineFinder.bFindOrient = true;
                mvdLineFinder.bEdgePolarity = _xInspectParams.bEdgePolarity2;
            }
            // 下
            else if (sideIndex == 3)
            {
                mvdLineFinder.bPositive = _xInspectParams.bPositive3;
                mvdLineFinder.bFindOrient = false;
                mvdLineFinder.bEdgePolarity = _xInspectParams.bEdgePolarity3;
            }
            #endregion

            //(3) 設定背景顏色
            mvdLineFinder.Background = 
                forceDarkBackgroud ? 
                EdgeBackGroundType.Dark :
                _xInspectParams.xCarrierBackground ;

            //(4) Filters
            if (!TryApplyLineFilters(bmp, out Bitmap bmpWork))
                bmpWork = bmp;

            //(5) 執行海康直線尋找
            var resultLine = mvdLineFinder.Run(bmpWork, roi, sideIndex);

            //(6) CleanUp
            if (bmpWork != bmp)
                bmpWork?.Dispose();

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

        #region PRIVATE_HELPER_FUNCTIONS
        QVector UnitVect(QVector p1, QVector p2)
        {
            var v = p2 - p1;
            return v / v.NormLength;
        }
        #endregion

        #region ASYNC_SAVING_FUNCTIONS
        /// <summary>
        /// LETIAN: 非同步保存 邊框數據
        /// </summary>
        void AsyncDumpLineSegmentsData(RegionCellX3Class cell, ref RectangleF cellRoi)
        {
            _lotDataHolder.AsyncDumpLineSegmentsData(cell, ref cellRoi);
        }
        #endregion

        #region DEBUG_DUMP_FUNCTIONS
        void _DUMP(string fileName, Bitmap bmpSrc, QvQuad2D quad, Scalar color)
        {
            if (bmpSrc == null || quad == null) 
                return;

            using (var bmpCanvas = (Bitmap)bmpSrc.Clone())
            using (var bridge = new QxImageBridge(bmpCanvas))
            using (var img = new Mat())
            {
                Cv2.CvtColor(bridge.Image, img, ColorConversionCodes.GRAY2BGR);
                img.Rectangle(JetEazy.Qcvt.CV(Rectangle.Round(quad.BoundaryRect)), color, 3);
                img.SaveImage($"d:\\paso.log\\{fileName}");

                if (EzPadsGridFinder.VISUAL_DEBUG)
                {
                    VxDebugDrawer.Draw(img, new[] { quad.ToBox2D() }, color, shrink: 1);
                    VxDebugDrawer.ShowWindow(fileName, img);
                }
            }
        }
        void _DUMP(string fileName, Bitmap bmpSrc, LineBorderPairsCollection borderPairs, Scalar color)
        {
            if (bmpSrc == null || borderPairs == null)
                return;

            using (var bmpCanvas = (Bitmap)bmpSrc.Clone())
            using (var bridge = new QxImageBridge(bmpCanvas))
            using (var img = new Mat())
            {
                Cv2.CvtColor(bridge.Image, img, ColorConversionCodes.GRAY2BGR);

                var boxes = new List<QvBox2D>();
                foreach ((var key, var pair) in borderPairs.IterPairs())
                {
                    foreach (var b in pair.Borders)
                        boxes.Add(b);
                }
                VxDebugDrawer.Draw(img, boxes, color, shrink: 1);
                img.SaveImage($"d:\\paso.log\\{fileName}");

                if (EzPadsGridFinder.VISUAL_DEBUG)
                {
                    VxDebugDrawer.ShowWindow(fileName, img);
                }
            }
        }
        #endregion
    }
}
