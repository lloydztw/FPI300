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
using System.Drawing.Imaging;
using System.Text;
using System.Threading.Tasks;
using Traveller106;
using VisionDesigner;
using ErrorCodes = LaserAlignDX.Mvc.Model.ErrorCodes;


namespace LaserAlignDX.AoiModel.V3
{
    /// <summary>
    /// 晶粒尺寸量測
    /// </summary>
    public class AoiModel_ChipMeasure : AoiModelBase
    {
        #region GLOBAL_MESS
        InspectX3ParaClass _xInspect => base._xRecipe.InspectParams;
        #endregion

        #region KERNEL_MEMBERS
        //ITransform _worldTransform;
        QMicroChipTransform _microTransform;
        #endregion

        #region RUNTIME_DATA
        GaCellsGroup[] _cellGroups;
        bool _is2ndRun;
        #endregion

        public bool QrUsed
        {
            get;
            set;
        }
        public bool QrJudged
        {
            get;
            set;
        }

        public void SetCellGroups(GaCellsGroup[] cellGroups)
        {
            this._cellGroups = cellGroups;
        }

        public override void Run()
        {
            bool go = _xInspect.optChipMeasurement || _xInspect.optChipDefectsInspect || QrUsed;
            if (!go)
                return;

            try
            {
                fire_AoiBegin();
                markRunStart();

                var activeCarrierID = getActiveCarrierID();
                //>>> _worldTransform = _sysModel.TransformsModel.GetCameraPhysicTransform(activeCarrierID);
                _microTransform = _sysModel.GetMicroTransform(activeCarrierID);
                if (_microTransform == null)
                    throw new Exception($"無法取得 Micro Transform ({activeCarrierID})");

                Bitmap bmpFullfov = LineScanCamImageHolder.PeekBitmap();
                RunChipsMeasurement(bmpFullfov);
                RunChipsMeasurement2ndForNGs(bmpFullfov);
                RunDefectsAndQrCode(bmpFullfov, LineScanCamImageHolder.PeekMvdImage());

                markRunEnd(true);
                fire_AoiEnd();
            }
            catch (Exception ex)
            {
                // 2025-08-28 LETIAN: 巨圖統一由 LineScanCamImageHolder 管理其生命週期
                // 在此無需釋放 巨圖
                markRunEnd(false);
                fire_AoiEnd();
                var errCode = ErrorCodes.EXCEPTION_AT_AOI_RUN;
                string errMsg = GaUtil.GetEnumDescription(errCode) + "\n\r" + ex.Message;
                GaUtil.LOG(errMsg, Color.Red);
                _LOG_ERROR(ex, $"異常 @ {GetType().Name}.Run");
                fire_AoiError(errCode, errMsg);
            }
        }

        /// <summary>
        /// 調試 使用
        /// </summary>
        internal void TryMeasureOneChip(RegionCellX3Class cell, Bitmap cellBmp, ref RectangleF cellRoi)
        {
            if (_microTransform == null)
                _microTransform = _sysModel.GetMicroTransform(getActiveCarrierID());
            if (_microTransform == null)
                return;

            RunOneChipMeasurement(cell, cellBmp, ref cellRoi);
        }

        #region PRIVATE_FUNCTIONS
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

            bool usingMultiThread = Universal.N_THREADS_ENABLED;

            #region 準備_CELL_GROUPS
            int N_GROUPS = _cellGroups != null ? _cellGroups.Length : MvdCompositeChipMatcher.N_CHANNLS;
            var groups = _cellGroups != null ? _cellGroups : GaCellsGroup.CollectGroups(N_GROUPS, _xRecipe, bmpFullfov);
            #endregion

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
            if (!_xInspect.optChipMeasurement)
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
                    RunOneChipMeasurement(cell, cellBmp, ref cellRoi);
                    PackMeasureResult(cell);
                    //_TM.END("OneChipMeasurement");
                }
            }
        }

        /// <summary>
        /// 量測單一晶粒 (直线寻找)
        /// </summary>
        private void _RunOneChipMeasurement_000(RegionCellX3Class cell, Bitmap cellBmp, RectangleF cellRoi)
        {
#if(OPT_OLD_CODE)
            // 取得 上一輪 晶粒定位 的結果 (chipData)
            var chipData = cell?.ChipData;
            var chipQuad = chipData?.ChipQuad2D;
            if (chipQuad == null)
                return;

            #region 邊線處理
            EdgeBorder eBorder = EdgeBorder.Left;
            try
            {
                var lineBorderRects = new RectangleF[]
                {
                    _xRecipe.xLineLeft,
                    _xRecipe.xLineTop,
                    _xRecipe.xLineRight,
                    _xRecipe.xLineBottom,
                };

                //var xLocalResult = cell.xFindResult;
                //xLocalResult.fCenterX -= cellRoi.X;
                //xLocalResult.fCenterY -= cellRoi.Y;
                var xLocalResult = new AUVision.xFindResult();
                xLocalResult.fCenterX = (float)(chipQuad.Center.X - cellRoi.X);
                xLocalResult.fCenterY = (float)(chipQuad.Center.Y - cellRoi.Y);
                xLocalResult.fAngle = (float)chipQuad.Angle;

                for (int borderIdx = 0, N = lineBorderRects.Length; borderIdx < N; borderIdx++)
                {
                    eBorder = (EdgeBorder)borderIdx;

                    CMvdRectangleF mvdBorderRect = GaImageUtil.ToCMvdRectangleF(ref lineBorderRects[borderIdx]);

                    CMvdRectangleF mvdBorderRotatedRect = cell.PositionFixRun(
                                                            mvdBorderRect,
                                                            _xRecipe.xRegionTrain,
                                                            Rectangle.Round(cellRoi),
                                                            xLocalResult) as CMvdRectangleF;

                    //(1) 海康線檢 (輸出為 cell.cMvdLineSegmentFsOut)
                    cell.LineSegmentRun(borderIdx, cellBmp, mvdBorderRotatedRect);

                    //(2) 將 CMvdLine 轉換成 EzLSD.LineSegment
                    var lines = Array.ConvertAll(cell.cMvdLineSegmentFsOut, mvdLine => mvdLine?.ToLineSegment());
                    for (int i = 0, len = lines.Length; i < len; i++)
                    {
                        // 記入 加回 ROI Offset
                        lines[i]?.Offset(cellRoi.X, cellRoi.Y);
                        // 記入 cell.ChipData
                        cell.ChipData.LineSegments[i] = lines[i];
                    }

                    //(3) 更新 LineBorderBoxes
                    //(3.1) 加回 ROI Offset
                    mvdBorderRotatedRect.CenterX += cellRoi.X;
                    mvdBorderRotatedRect.CenterY += cellRoi.Y;

                    //(3.2) 更新 LineBorderBoxes;
                    chipData.LineBorderBoxes[borderIdx] = mvdBorderRotatedRect.ToBox2D();
                    //(3.3) 更新到 cell 舊的 Gaara Data
                    cell.cMvdShapesForFindLineRegion[borderIdx] = (CMvdShape)mvdBorderRotatedRect.Clone();
                }
            }
            catch (Exception ex)
            {
                string borderName = JetEazy.QxNums.GetEnumDescription(eBorder);
                _LOG_ERROR(ex, $"{borderName} 定位異常");
                throw ex;
            }
            #endregion

            #region 尺寸長寬量測
            try
            {
                //(1) 將 CMvdLine 轉換成 EzLSD.LineSegment
                var lines = Array.ConvertAll(cell.cMvdLineSegmentFsOut, mvdLine => mvdLine?.ToLineSegment());
                for (int i = 0, len = lines.Length; i < len; i++)
                {
                    //(1.1) 加回 ROI Offset
                    lines[i]?.Offset(cellRoi.X, cellRoi.Y);
                    //(1.2) 記入 cell.ChipData
                    chipData.LineSegments[i] = lines[i];
                }

                //(2) 使用 Micro Transform 計算 尺寸 與 邊隙
                //    (結果會直接存入 cell.ChipData 內)
                bool toMeasureGaps = _xInspect.optPadEdgeGapsMeasurement && _xInspect.xAlgorithm == MatchAlgorithmEnum.GridMatch;
                var err = _microTransform.CalcChipDimension(out SizeF dimension, lines, chipData, toMeasureGaps);

                //(3) 記入結果
                cell.RunWidth = dimension.Width;
                cell.RunHeight = dimension.Height;

                //(4) 異常
                if (err != ErrCodes.OK)
                    throw new Exception(GaUtil.GetEnumDescription(err));
            }
            catch (Exception ex)
            {
                _LOG_ERROR(ex, "晶粒 尺寸量測 異常");
                return;
            }
            #endregion
#endif
        }

        /// <summary>
        /// 量測單一晶粒 (直线寻找)
        /// </summary>
        private void RunOneChipMeasurement(RegionCellX3Class cell, Bitmap cellBmp, ref RectangleF cellRoi)
        {
            // 取得 上一輪 晶粒定位 的結果 (chipData)
            var chipData = cell?.ChipData;
            if (chipData == null || chipData.ChipQuad2D == null)
                return;

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
                        // 海康線檢(輸出為 cell.cMvdLineSegmentFsOut)
                        mvdLine = cell.LineSegmentRun(borderIdx, cellBmp, mvdRoi, chipData.ChipQuad2D.Angle);
                    }
                    //(2.1) 海康線檢 II (暫時不使用)
                    else
                    {
                        using (Bitmap cellBmp2 = (Bitmap)cellBmp.Clone())
                        {
                            // 塗掉中段 (1/3) 
                            fill_border_mid_area(cellBmp2, borderQuad, eBorder, Scalar.Black);
                            // 海康線檢(輸出為 cell.cMvdLineSegmentFsOut)
                            mvdLine = cell.LineSegmentRun(borderIdx, cellBmp2, mvdRoi, chipData.ChipQuad2D.Angle);
                        }

                        // 如果塗掉中段 (1/3) 仍然抓不到, 回過頭使用 原來的方法
                        if (mvdLine == null)
                        {
                            mvdLine = cell.LineSegmentRun(borderIdx, cellBmp, mvdRoi, chipData.ChipQuad2D.Angle);
                        }
                    }

                    //(3) 將 CMvdLine 轉換成 EzLSD.LineSegment
                    #region OLD_CODE
                    //var lines = Array.ConvertAll(cell.cMvdLineSegmentFsOut, mvdLine => mvdLine?.ToLineSegment());
                    //for (int i = 0, len = lines.Length; i < len; i++)
                    //{
                    //    //(3.1) 加回 ROI Offset
                    //    lines[i]?.Offset(cellRoi.X, cellRoi.Y);
                    //    //(3.2) 記入 cell.ChipData
                    //    cell.ChipData.LineSegments[i] = lines[i];
                    //}
                    #endregion
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
                    ////(4.3) 更新到 cell 舊的 Gaara Data
                    //cell.cMvdShapesForFindLineRegion[borderIdx] = borderQuad.ToCMvdRectangleF();
                }
            }
            catch (Exception ex)
            {
                string borderName = JetEazy.QxNums.GetEnumDescription(eBorder);
                _LOG_ERROR(ex, $"{borderName} 定位異常");
                throw ex;
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
                        gaps.Check(out ok, min, max);
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
        private QvQuad2D[] _CalcRuntimeLocalLineBorderQuads_001(RegionCellX3Class cell, RectangleF cellRoi)
        {
#if (OPT_OLD_CODE)
            // 取得 上一輪 晶粒定位 的結果 (chipData)
            var chipData = cell?.ChipData;

            //(0) 複製 goldenQuad 與 chipQuad
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

                //(2.1) DEBUG
                if (EzPadsGridFinder.VISUAL_DEBUG)
                {
                    using (var bridge = new QxImageBridge(_xRecipe.bmpprinttemplate))
                    {
                        var img = bridge.Image;
                        var boxes = new List<QvBox2D>();
                        boxes.Add(goldenQuad.ToBox2D());
                        foreach (var line in lineBorderQuads)
                            boxes.Add(line.ToBox2D());
                        VxDebugDrawer.Draw(img, boxes, OpenCvSharp.Scalar.Orange, shrink: 0, displayWindowName: "CalcRuntimeLocalLineBorderQuads");
                    }
                }

                for (int i = 0; i < NP; i++)
                {
                    int k = (i == 0) ? (NP - 1) : (i - 1);

                    var goldenCcMidPt = (goldenQuad.Corners[i] + goldenQuad.Corners[k]) / 2.0;
                    var goldenCcVect = goldenQuad.Corners[i] - goldenQuad.Corners[k];
                    var goldenTheta = Math.Atan2(goldenCcVect.Y, goldenCcVect.X);
                    var shiftVect = lineBorderQuads[i].Center - goldenCcMidPt;

                    var runtimeQuad = chipQuad;
                    var runtimeCcMidPt = (runtimeQuad.Corners[i] + runtimeQuad.Corners[k]) / 2.0;
                    var runtimeCcVect = runtimeQuad.Corners[i] - runtimeQuad.Corners[k];
                    var runtimeTheta = Math.Atan2(runtimeCcVect.Y, runtimeCcVect.X);
                    var theta = runtimeTheta - goldenTheta;

                    var borderQuad = lineBorderQuads[i];
                    var borderCenter = runtimeCcMidPt + shiftVect;
                    borderQuad.SetCenter(borderCenter);
                    QvQuad2D.Rotate(borderQuad, borderCenter, theta, inplace: true);
                }

                foreach (var line in lineBorderQuads)
                    line.Offset(-cellRoi.X, -cellRoi.Y);

                return lineBorderQuads;
            }
            catch (Exception ex)
            {
                string borderName = JetEazy.QxNums.GetEnumDescription(eBorder);
                _LOG_ERROR(ex, $"{borderName} 無法計算 LineBorderQuads!");
                throw ex;
            }
#endif
            return null;
        }

        /// <summary>
        /// 計算 選轉&平移 後的 邊線框 (左, 上, 右, 下)
        /// </summary>
        private QvQuad2D[] CalcRuntimeLocalLineBorderQuads_fine(RegionCellX3Class cell, RectangleF cellRoi)
        {
            // 取得 上一輪 晶粒定位 的結果 (chipData)
            var chipData = cell?.ChipData;

            //(0) 複製 goldenQuad 與 chipQuad
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

                //(2.1) DEBUG
                #region DEBUG
                if (false && EzPadsGridFinder.VISUAL_DEBUG)
                {
                    using (var bridge = new QxImageBridge(_xRecipe.bmpprinttemplate))
                    {
                        var img = bridge.Image;
                        var boxes = new List<QvBox2D>();
                        foreach (var line in lineBorderQuads)
                            boxes.Add(line.ToBox2D());
                        boxes.Add(goldenQuad.ToBox2D());
                        VxDebugDrawer.Draw(img, boxes, OpenCvSharp.Scalar.Orange, shrink: 0, displayWindowName: "CalcRuntimeLocalLineBorderQuads");
                    }
                }
                #endregion

                for (int i = 0; i < NP; i++)
                {
                    int k = (i == 0) ? (NP - 1) : (i - 1);

                    var lineBorderQuad = lineBorderQuads[i];
                    var goldenLine = new EzLSD.LineSegment(goldenQuad.Corners[i], goldenQuad.Corners[k]);
                    var dists = Array.ConvertAll(lineBorderQuad.Corners, c => goldenLine.CalcDistance(c));
                    var distMin = double.MaxValue;
                    var distMax = double.MinValue;
                    foreach(var d  in dists)
                    {
                        distMin = Math.Min(distMin, d);
                        distMax = Math.Max(distMax, d);
                    }
                    lineBorderQuad.GetMidSize(out SizeF bsize);
                    var borderSpan = Math.Max(bsize.Width, bsize.Height);

                    var runtimeQuad = chipQuad;
                    var runtimeCcMidPt = (runtimeQuad.Corners[i] + runtimeQuad.Corners[k]) / 2.0;
                    var runtimeCcVect = runtimeQuad.Corners[i] - runtimeQuad.Corners[k];
                    var U = runtimeCcVect / runtimeCcVect.NormLength;
                    var V = runtimeCcMidPt - runtimeQuad.Center;
                    V = V / V.NormLength;

                    var qExt = runtimeCcMidPt + V * distMax;
                    var qInd = runtimeCcMidPt + V * distMin;

                    var Q0 = qExt + U * borderSpan / 2;
                    var Q1 = qInd + U * borderSpan / 2;
                    var Q2 = qInd - U * borderSpan / 2;
                    var Q3 = qExt - U * borderSpan / 2;

                    lineBorderQuad.Corners = new[] { Q0, Q1, Q2, Q3 };
                    lineBorderQuad.Sort();
                }

                foreach (var line in lineBorderQuads)
                    line.Offset(-cellRoi.X, -cellRoi.Y);

                return lineBorderQuads;
            }
            catch (Exception ex)
            {
                string borderName = JetEazy.QxNums.GetEnumDescription(eBorder);
                _LOG_ERROR(ex, $"{borderName} 無法計算 LineBorderQuads!");
                throw ex;
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
                    var v = unitVect(goldenLineMid, goldenQuad.Center);
                    var u = unitVect(goldenLineMid, goldenQuad.Corners[i]);
                    var lbCorners = Array.ConvertAll(lineBorderQuad.Corners, lc => lc - goldenLineMid);
                    var lbCornersU = Array.ConvertAll(lbCorners, c => c * u);
                    var lbCornersV = Array.ConvertAll(lbCorners, c => c * v);

                    var runtimeLineMid = (chipQuad.Corners[i] + chipQuad.Corners[iPrev]) / 2.0;
                    var V = unitVect(runtimeLineMid, chipQuad.Center);
                    var U = unitVect(runtimeLineMid, chipQuad.Corners[i]);
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
        }

        /// <summary>
        /// LETIAN: 读码测试 搬移至此.
        /// caller 負責 bmpInputImage 生命
        /// </summary>
        private void RunDefectsAndQrCode(Bitmap bmpFullfov,CMvdImage cMvdImage)
        {
            bool go = QrUsed || _xInspect.optChipDefectsInspect;
            if (!go)
                return;

            fire_AoiBegin("晶粒瑕疵 與 QR CODE");

            //>>> var cells = _xRecipe.xRegionCells;

            foreach (var cell in PlcDataPacker.IterFinalResultCells())
            {
                fire_AoiProgressing(cell);

                if (cell.ByPass && !INI.Instance.IsForceInspect)
                    continue;

                //if (cell.inspectReason == InspectReason.INS_ALIGNERR)
                if (cell.IsEmptyPlaceHold() || cell.IsAmbiguousBloc())
                    continue;

                // Golden Region Size
                var regionSize = _xRecipe.bmpprinttemplate.Size;

                // 定位完成后裁切位置
                // RectangleF _crop = new RectangleF(
                //    cell.DrawResultRectF().CenterX - regionSize.Width / 2,
                //    cell.DrawResultRectF().CenterY - regionSize.Height / 2,
                //    regionSize.Width,
                //    regionSize.Height);

                var mvdRect = cell.DrawResultRectF();
                var regionRoi = JetEazy.Qcvt.CreateCenterRect(mvdRect.CenterX, mvdRect.CenterY, regionSize.Width, regionSize.Height);

                if (_xInspect.optChipDefectsInspect)
                {
                    try
                    {
                        //RectangleF _cropDefect = new RectangleF(
                        //    xRecipe.xRegionTrain.X + regionRoi.X,
                        //    xRecipe.xRegionTrain.Y + regionRoi.Y,
                        //    xRecipe.xRegionTrain.Width,
                        //    xRecipe.xRegionTrain.Height);
                        //cell.bmpItemRun?.Dispose();
                        //cell.bmpItemRun = bmpFullfov.Clone(_cropDefect, PixelFormat.Format8bppIndexed);
                        //cell.bmpItemMask?.Dispose();
                        //cell.bmpItemMask = xRecipe.bmpprintmask.Clone(
                        //    new Rectangle(0, 0, xRecipe.bmpprintmask.Width, xRecipe.bmpprintmask.Height),
                        //    PixelFormat.Format8bppIndexed);
                        //cell.DetectDefects(xRecipe.bmpDefectTemplate, cell.bmpItemRun, cell.bmpItemMask);

                        var templateSize = _xRecipe.bmpDefectTemplate.Size;
                        var chipCenter = cell.ChipData.ChipQuad2D.Center;
                        var chipAngle = cell.ChipData.ChipQuad2D.Angle;      //如果把 _xRecipe.bmpDefectTemplate 當成無角度的 rectangle 就不需要減 GoldenQuad2D.Angle
                        //var box2d = new QvBox2D();
                        //box2d.SetBox(PointF.Empty, templateSize);
                        //box2d.SetCenter((float)chipCenter.X, (float)chipCenter.Y);
                        //box2d.SetTheta((float)chipAngle * Math.PI / 180.0);
                        //var mvdRectX = GaMvdExt.ToCMvdRectangleF(box2d);

                        var mvdRectX = new CMvdRectangleF((float)chipCenter.X, (float)chipCenter.Y, templateSize.Width, templateSize.Height)
                        {
                            Angle = (float)chipAngle
                        };

                        var bmpTemplate = _xRecipe.bmpDefectTemplate;
                        var bmpMask = _xRecipe.bmpprintmask;
                        //var roi = _xRecipe.xRegionTrain;
                        //roi.X += regionRoi.X;
                        //roi.Y += regionRoi.Y;
                        
                        using (var bmpRun = cell.GetAffineTrainsFormRunBmp(cMvdImage, mvdRectX))
                        //using (var bmpRun = bmpFullfov.Clone(roi, PixelFormat.Format8bppIndexed))
                        {
                            cell.DetectDefects(bmpTemplate, bmpRun, bmpMask);
                        }
                    }
                    catch (Exception ex)
                    {
                        _LOG_ERROR(ex, "cell.DetectDefects 異常!");
                        _xInspect.optChipDefectsInspect = false;
                    }
                }

                if (QrUsed)
                {
                    try
                    {
                        //RectangleF _cropCode = new RectangleF(
                        //    xRecipe.xRectCodeRegion.X + regionRoi.X,
                        //    xRecipe.xRectCodeRegion.Y + regionRoi.Y,
                        //    xRecipe.xRectCodeRegion.Width,
                        //    xRecipe.xRectCodeRegion.Height);
                        //cell.bmpItemCodeRun?.Dispose();
                        //cell.bmpItemCodeRun = bmpFullfov.Clone(_cropCode, PixelFormat.Format8bppIndexed);

                        var roi = _xRecipe.xRectCodeRegion;
                        roi.X += regionRoi.X;
                        roi.Y += regionRoi.Y;
                        using (var bmpRun = bmpFullfov.Clone(roi, PixelFormat.Format8bppIndexed))
                        {
                            //cell.DeCode2D(cell.bmpItemCodeRun, _cropCode.Location, m_QrJudged);
                            cell.DeCode2D(bmpRun, roi.Location, QrJudged);
                        }
                    }
                    catch (Exception ex)
                    {
                        _LOG_ERROR(ex, "cell.DeCode2D 異常!");
                        //xInspect.m_QrUsed = false;
                    }
                }
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
        QVector unitVect(QVector p1, QVector p2)
        {
            var v = p2 - p1;
            return v / v.NormLength;
        }
        #endregion
    }
}
