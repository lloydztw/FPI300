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


using JetEazy.QMath;
using JetEazy.QvMath;
using JetEazy.Utils;
using LaserAlignDX.BasicSpace;
using LaserAlignDX.Model.Coords;
using LaserAlignDX.OPSpace;
using LaserAlignDX.OPSpace.RecipeSpace;
using LeTian.AoiLib;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Threading.Tasks;
using Traveller106;
using VisionDesigner;
using ErrCodes = LaserAlignDX.Mvc.Model.ErrCodes;


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
                RunDefectsAndQrCode(bmpFullfov);

                markRunEnd(true);
                fire_AoiEnd();
            }
            catch (Exception ex)
            {
                // 2025-08-28 LETIAN: 巨圖統一由 LineScanCamImageHolder 管理其生命週期
                // 在此無需釋放 巨圖
                markRunEnd(false);
                fire_AoiEnd();
                var errCode = ErrCodes.EXCEPTION_AT_AOI_RUN;
                string errMsg = GaUtil.GetEnumDescription(errCode) + "\n\r" + ex.Message;
                GaUtil.LOG(errMsg, Color.Red);
                _LOG_ERROR(ex, $"異常 @ {GetType().Name}.Run");
                fire_AoiError(errCode, errMsg);
            }
        }

        #region PRIVATE_FUNCTIONS
        /// <summary>
        /// LETIAN: 晶粒定位 與 尺寸量測 
        /// </summary>
        private void RunChipsMeasurement(Bitmap bmpFullfov)
        {
            if (!_xInspect.optChipMeasurement)
                return;

            fire_AoiBegin("晶粒尺寸量測");

            //_TM.RESET_ACCUM();

            bool usingMultiThread = Universal.N_THREADS_ENABLED;

            #region 準備_CELL_GROUPS
            int N_GROUPS = _cellGroups != null ? _cellGroups.Length : MvdCompositeChipMatcher.N_CHANNLS;
            var groups = _cellGroups != null ? _cellGroups : GaCellsGroup.CollectGroups(N_GROUPS, _xRecipe, bmpFullfov);
            #endregion

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
                Parallel.For(0, N_GROUPS, gid =>
                {
                    if (gid < groups.Length)
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

                bool go = cell.ChipData.ChipQuad2D != null && cell.inspectReason == InspectReason.PASS;
                if (go)
                {
                    //(2) 量測單一晶粒
                    RunOneChipMeasurement(cell, cellBmp, cellRoi);
                    cell.PackMeasureResult();
                    //_TM.END("OneChipMeasurement");
                }
            }
        }
        /// <summary>
        /// 量測單一晶粒 (直线寻找)
        /// </summary>
        private void RunOneChipMeasurement_000(RegionCellX3Class cell, Bitmap cellBmp, RectangleF cellRoi)
        {
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
        }
        /// <summary>
        /// 量測單一晶粒 (直线寻找)
        /// </summary>
        private void RunOneChipMeasurement(RegionCellX3Class cell, Bitmap cellBmp, RectangleF cellRoi)
        {
            // 取得 上一輪 晶粒定位 的結果 (chipData)
            var chipData = cell?.ChipData;
            if (chipData == null)
                return;

            #region 邊線處理
            EdgeBorder eBorder = EdgeBorder.Left;
            try
            {
                var lineBorderQuads = CalcRuntimeLocalLineBorderQuads(cell, cellRoi);
                if (lineBorderQuads == null)
                    return;

                for (int borderIdx = 0, N = lineBorderQuads.Length; borderIdx < N; borderIdx++)
                {
                    //(0) enum
                    eBorder = (EdgeBorder)borderIdx;

                    //(1) borderQuad
                    var borderQuad = lineBorderQuads[borderIdx];
                    var mvdRoi = borderQuad.ToCMvdRectangleF();

                    //(2) 海康線檢 (輸出為 cell.cMvdLineSegmentFsOut)
                    cell.LineSegmentRun(borderIdx, cellBmp, mvdRoi);

                    //(3) 將 CMvdLine 轉換成 EzLSD.LineSegment
                    var lines = Array.ConvertAll(cell.cMvdLineSegmentFsOut, mvdLine => mvdLine?.ToLineSegment());
                    for (int i = 0, len = lines.Length; i < len; i++)
                    {
                        //(3.1) 加回 ROI Offset
                        lines[i]?.Offset(cellRoi.X, cellRoi.Y);
                        //(3.2) 記入 cell.ChipData
                        cell.ChipData.LineSegments[i] = lines[i];
                    }

                    //(4) 更新 LineBorderBoxes
                    //(4.1) 加回 ROI Offset
                    borderQuad.Offset(cellRoi.X, cellRoi.Y);
                    //(4.2) 更新 LineBorderBoxes;
                    chipData.LineBorderBoxes[borderIdx] = borderQuad.ToBox2D();
                    //(4.3) 更新到 cell 舊的 Gaara Data
                    cell.cMvdShapesForFindLineRegion[borderIdx] = borderQuad.ToCMvdRectangleF();
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
        }
        private QvQuad2D[] CalcRuntimeLocalLineBorderQuads(RegionCellX3Class cell, RectangleF cellRoi)
        {
            // 取得 上一輪 晶粒定位 的結果 (chipData)
            var chipData = cell?.ChipData;
            var chipQuad = chipData?.ChipQuad2D?.Clone();
            var goldenQuad = chipData?.GoldenQuad2D?.Clone();
            if (chipQuad == null || goldenQuad == null)
                return null;

            EdgeBorder eBorder = EdgeBorder.Left;
            try
            {
                var goldenChipRect = _xRecipe.xRegionTrain;
                var goldenChipCenter = JetEazy.Qcvt.CenterF(ref goldenChipRect);
                var runtimeLocalCenter = chipQuad.Center - new QVector(cellRoi.X, cellRoi.Y);
                var borderToLocalOffset = runtimeLocalCenter - new QVector(goldenChipCenter.X, goldenChipCenter.Y);

                var gcc = goldenQuad.Center;
                goldenQuad.Offset(-gcc.X, -gcc.Y);
                var cc = chipQuad.Center;
                chipQuad.Offset(-cc.X, -cc.Y);

                var lineBorderQuads = new QvQuad2D[]
                {
                    QvQuad2D.From(ref _xRecipe.xLineLeft),
                    QvQuad2D.From(ref _xRecipe.xLineTop),
                    QvQuad2D.From(ref _xRecipe.xLineRight),
                    QvQuad2D.From(ref _xRecipe.xLineBottom),
                };

                var runtimeQuad = chipQuad.Clone();

                for (int i = 0, N = lineBorderQuads.Length; i < N; i++)
                {
                    int k = i == 0 ? (N - 1) : (i - 1);

                    var goldenMidPt = (goldenQuad.Corners[i] + goldenQuad.Corners[k]) / 2.0;
                    var runtimeMitPt = (runtimeQuad.Corners[i] + runtimeQuad.Corners[k]) / 2.0;
                    var shiftV = runtimeMitPt - goldenMidPt;

                    var goldenVect = goldenQuad.Corners[i] - goldenQuad.Corners[k];
                    var runtimeVect = runtimeQuad.Corners[i] - runtimeQuad.Corners[k];
                    var thetaG = Math.Atan2(goldenVect.Y, goldenVect.X);
                    var thetaT = Math.Atan2(runtimeVect.Y, runtimeVect.X);
                    var theta = thetaT - thetaG;

                    var borderQuad = lineBorderQuads[i];
                    var borderCenter = borderQuad.Center + borderToLocalOffset + shiftV;
                    borderQuad.SetCenter(borderCenter);
                    QvQuad2D.Rotate(borderQuad, borderCenter, theta, inplace: true);
                }

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
        private void RunDefectsAndQrCode(Bitmap bmpFullfov)
        {
            bool go = QrUsed || _xInspect.optChipDefectsInspect;
            if (!go)
                return;

            fire_AoiBegin("晶粒瑕疵 與 QR CODE");

            var cells = _xRecipe.xRegionCells;

            foreach (var cell in cells)
            {
                fire_AoiProgressing(cell);

                if (cell.ByPass && !INI.Instance.IsForceInspect)
                    continue;

                if (cell.inspectReason == InspectReason.INS_ALIGNERR)
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

                        var bmpTemplate = _xRecipe.bmpDefectTemplate;
                        var bmpMask = _xRecipe.bmpprintmask;
                        var roi = _xRecipe.xRegionTrain;
                        roi.X += regionRoi.X;
                        roi.Y += regionRoi.Y;

                        using (var bmpRun = bmpFullfov.Clone(roi, PixelFormat.Format8bppIndexed))
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
    }
}
