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
using JetEazy.Transform;
using JetEazy.Utils;
using LaserAlignDX.BasicSpace;
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
using _TM = LeTian.AoiLib.LtDebug;


namespace LaserAlignDX.AoiModel.V3
{
    public class AoiModel_ChipMeasure : AoiModelBase
    {
        #region GLOBAL_MESS
        InspectX3ParaClass _xInspect => base._xRecipe.InspectParams;
        #endregion

        #region KERNEL_MEMBERS
        ITransform _transToWorld;
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
                _transToWorld = _sysModel.TransformsModel.GetCameraPhysicTransform(activeCarrierID);

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
                var errCode = Mvc.Model.ErrCodes.EXCEPTION_AT_AOI_RUN;
                string errMsg = GaUtil.GetEnumDescription(errCode) + "\n\r" + ex.Message;
                fire_AoiError(errCode, errMsg);
                GaUtil.LOG(errMsg, Color.Red);
                _LOG_ERROR(ex, $"異常 @ {GetType().Name}.Run");
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

                bool go = cell.ChipData.ChipBox2D != null && cell.inspectReason == InspectReason.PASS;
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
        private void RunOneChipMeasurement(RegionCellX3Class cell, Bitmap cellBmp, RectangleF cellRoi)
        {
            // 取得 上一輪 晶粒定位 的結果 (Box2D)
            var chipBox2D = cell.ChipData.ChipBox2D;

            #region 邊線處理
            EdgeBorder eBorder = EdgeBorder.Left;

            try
            {
                RectangleF[] rcpBorderRects = new RectangleF[]
                {
                    _xRecipe.xLineLeft,
                    _xRecipe.xLineTop,
                    _xRecipe.xLineRight,
                    _xRecipe.xLineBottom,
                };

                var xLocalResult = cell.xFindResult;
                xLocalResult.fCenterX -= cellRoi.X;
                xLocalResult.fCenterY -= cellRoi.Y;

                for (int borderIdx = 0, N = rcpBorderRects.Length; borderIdx < N; borderIdx++)
                {
                    eBorder = (EdgeBorder)borderIdx;

                    CMvdRectangleF mvdBorderRect = GaImageUtil.ToCMvdRectangleF(ref rcpBorderRects[borderIdx]);

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
                    cell.ChipData.LineBorderBoxes[borderIdx] = mvdBorderRotatedRect.ToBox2D();
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
                // 將 CMvdLine 轉換成 EzLSD.LineSegment
                var lines = Array.ConvertAll(cell.cMvdLineSegmentFsOut, mvdLine => mvdLine?.ToLineSegment());
                for (int i = 0, len = lines.Length; i < len; i++)
                {
                    // 記入 加回 ROI Offset
                    lines[i]?.Offset(cellRoi.X, cellRoi.Y);
                    // 記入 cell.ChipData
                    cell.ChipData.LineSegments[i] = lines[i];
                }
                CalcChipDimension(lines, out SizeF dimension, out var camMeasurePts, true);
                cell.RunWidth = dimension.Width;
                cell.RunHeight = dimension.Height;
                cell.ChipData.DimMeasurePoints = camMeasurePts;
            }
            catch (Exception ex)
            {
                _LOG_ERROR(ex, "晶粒 長寬量測 異常");
            }
            #endregion

            #region 計算格點型晶粒的邊緣寬度
            if (_xInspect.xAlgorithm == MatchAlgorithmEnum.GridMatch && _xInspect.optChipEdgesDiffCompare && chipBox2D != null)
            {
                try
                {
                    //晶格角點: 0左上, 1右上, 2右下, 3左下
                    var corners = Array.ConvertAll(chipBox2D.Corners, c => new QVector(c.X, c.Y));
                    //晶格左 點平均: (左上 + 左下) / 2
                    var left = (corners[0] + corners[3]) / 2;
                    //晶格右點 平均: (右上 + 右下) / 2
                    var right = (corners[1] + corners[2]) / 2;
                    //晶格上點 平均: (左上 + 右上) / 2
                    var top = (corners[0] + corners[1]) / 2;
                    //晶格下點 平均: (左下 + 右下) / 2
                    var bottom = (corners[2] + corners[3]) / 2;
                    //轉換到 world
                    left = ToWorld(left);
                    top = ToWorld(top);
                    right = ToWorld(right);
                    bottom = ToWorld(bottom);

                    var lines = cell.ChipData.LineSegments;
                    var lineL = lines[0];   // 左邊線
                    var lineT = lines[1];   // 上邊線
                    var lineR = lines[2];   // 右邊線
                    var lineB = lines[3];   // 下邊線

                    if (lineL != null && lineR != null)
                    {
                        //晶格 左邊緣 厚度 = 晶格左點 至 左邊線(line0) 距離
                        var edge_left = lineL.CalcDistance(left);
                        //晶格 右邊緣 厚度 = 晶格右點 至 右邊線(line2) 距離
                        var edge_right = lineR.CalcDistance(right);
                        //記入 結果
                        cell.PadEdgeSizes[(int)EdgeBorder.Left] = (float)Math.Round(edge_left, 3);
                        cell.PadEdgeSizes[(int)EdgeBorder.Right] = (float)Math.Round(edge_left, 3);
                    }

                    if (lineT != null && lineB != null)
                    {
                        //晶格 上邊緣 厚度 = 晶格上點 至 上邊線(line1) 距離
                        var edge_top = lineT.CalcDistance(top);
                        //晶格 下邊緣 厚度 = 晶格下點 至 下邊線(line3) 距離
                        var edge_bottom = lineB.CalcDistance(bottom);
                        //記入 結果
                        cell.PadEdgeSizes[(int)EdgeBorder.Top] = (float)Math.Round(edge_top, 3);
                        cell.PadEdgeSizes[(int)EdgeBorder.Bottom] = (float)Math.Round(edge_bottom, 3);
                    }
                }
                catch (MvdException ex)
                {
                    _LOG_ERROR(ex, "計算格點型晶粒的邊緣寬度 異常");
                }
            }
            #endregion
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

        public bool CalcChipDimension(EzLSD.LineSegment[] lines, out SizeF chipSize, out QVector[] camMeasurePts, bool usePostScale)
        {
            chipSize = SizeF.Empty;
            camMeasurePts = new QVector[4];

            #region 長度量測
            bool okLR = false;
            try
            {
                var line0 = lines[0];     //左邊線
                var line2 = lines[2];     //右邊線
                if (line0 != null && line2 != null)
                {
                    //(1) 轉換到 Physic Coordinates 重組 line segment, 再行計算距離.
                    var P1 = ToWorld(line0.P1);
                    var P2 = ToWorld(line0.P2);
                    var Q1 = ToWorld(line2.P1);
                    var Q2 = ToWorld(line2.P2);
                    line0 = new EzLSD.LineSegment(P1, P2);
                    line2 = new EzLSD.LineSegment(Q1, Q2);

                    //(2) 計算點線距離
                    var P = line0.GetMidPoint();
                    var Q = line2.CalcTheNearestPoint(P);
                    double dist = line2.CalcDistance(P);
                    
                    //(3) Result
                    chipSize.Width = (float)dist;   // Math.Round(dist, 3);
                    camMeasurePts[0] = ToCamera(P);
                    camMeasurePts[2] = ToCamera(Q);
                    okLR = true;
                }
            }
            catch (Exception ex)
            {
                _LOG_ERROR(ex, "長度量測 異常");
            }
            #endregion

            #region 寬度量測
            bool okTB = false;
            try
            {
                var line1 = lines[1];     //上邊線
                var line3 = lines[3];     //下邊線
                if (line1 != null && line3 != null)
                {
                    //(1) 轉換到 Physic Coordinates 重組 line segment, 再行計算距離.
                    var P1 = ToWorld(line1.P1);
                    var P2 = ToWorld(line1.P2);
                    var Q1 = ToWorld(line3.P1);
                    var Q2 = ToWorld(line3.P2);
                    P1 = ToWorld(line1.P1);
                    line1 = new EzLSD.LineSegment(P1, P2);
                    line3 = new EzLSD.LineSegment(Q1, Q2);

                    //(2) 計算點線距離
                    var P = line1.GetMidPoint();
                    var Q = line3.CalcTheNearestPoint(P);
                    double dist = line3.CalcDistance(P);

                    //(3) Result
                    chipSize.Height = (float)dist;  // Math.Round(dist, 3);
                    camMeasurePts[1] = ToCamera(P);
                    camMeasurePts[3] = ToCamera(Q);
                    okTB = true;
                }
            }
            catch (Exception ex)
            {
                _LOG_ERROR(ex, "寬度量測 異常");
            }
            #endregion

            if (usePostScale && false)
            {
                chipSize.Width = (float)Math.Round(chipSize.Width * _xInspect.xChipDimScaleW, 3);
                chipSize.Height = (float)Math.Round(chipSize.Height * _xInspect.xChipDimScaleH, 3);
            }
            else
            {
                chipSize.Width = (float)Math.Round(chipSize.Width, 3);
                chipSize.Height = (float)Math.Round(chipSize.Height, 3);
            }

            return okLR && okTB;
        }

        #region PRIVATE_TRANSFER_FUNCTIONS
        QVector ToWorld(QVector p)
        {
            if (p == null)
                return p;
            
            if (_transToWorld != null && false)
                return _transToWorld.Trans(p);

            var q = new QVector(p);
            q.X = p.X * Traveller106.INI.Instance.ImageResolutionX;
            q.Y = p.Y * Traveller106.INI.Instance.ImageResolutionY;
            return q;
        }
        QVector ToCamera(QVector p)
        {
            if (p == null)
                return p;

            if (_transToWorld != null && false)
                return _transToWorld.InvTrans(p);

            var q = new QVector(p);
            q.X = p.X / Traveller106.INI.Instance.ImageResolutionX;
            q.Y = p.Y / Traveller106.INI.Instance.ImageResolutionY;
            return q;
        }
        #endregion
    }
}
