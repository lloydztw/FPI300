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


using EzAoiEmptyTrayInspector.Model;
using JetEazy.Match;
using JetEazy.OpenCV;
using JetEazy.QMath;
using JetEazy.QvMath;
using JetEazy.Transform;
using JetEazy.Utils;
using LaserAlignDX.Model;
using LaserAlignDX.Model.Coords;
using LaserAlignDX.OPSpace;
using LeTian.AoiLib;
using NLog.LayoutRenderers;
using OpenCvSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Traveller106;
using _TM = LeTian.AoiLib.LtDebug;


namespace LaserAlignDX.AoiModel.V3
{
    /// <summary>
    /// 晶粒定位
    /// </summary>
    public class AoiModel_ChipLoc : AoiModelBase
    {
        #region GLOBAL_MESS
        #endregion

        #region KERNEL_MEMBERS
        /// <summary>
        /// 2025-09-10 新座標轉換
        /// </summary>
        TravellerTransforms _transformModel => _sysModel.TransformsModel;
        ITransform _transCP;
        ITransform _transCS1;
        ITransform _transCS2;
        #endregion

        #region RUNTIME_DATA
        GaCellsGroup[] _cellGroups;
        EzEmptyTrayResult _preEmptyTrayResult;
        #endregion


        public GaCellsGroup[] CellGroups
        {
            get { return _cellGroups; }
        }

        public void DisposeCellGroups()
        {
            var old = _cellGroups;
            _cellGroups = null;
            GaCellsGroup.DisposeAll(old);
        }

        public override void Run()
        {
            try
            {
                fire_AoiBegin("晶粒定位");

                //(0) 效能追蹤
                _TM.Reset();

                //_xRecipe.AnalyzeDatasData();
                //_TM.Trace("_Inspect001 : xRecipe.AnalyzeDatasData()");

                //(1) 標記起始計時
                markRunStart();

                //(2) 準備資料夾
                string imgLogPath = GetLogPath(this.FileBarcodeStr);
                #region PREPARE_PATH
                if (INI.Instance.IsSaveTestImage)
                {
                    if (!Directory.Exists(imgLogPath))
                        Directory.CreateDirectory(imgLogPath);
                }
                #endregion

                //(3) 取得座標轉換
                var activeCarrierID = getActiveCarrierID();
                _transCP = _transformModel.GetCameraPhysicTransform(activeCarrierID);
                _transCS1 = _transformModel.GetCameraMotorTransform(activeCarrierID, SuckerRowEnum.S1);
                _transCS2 = _transformModel.GetCameraMotorTransform(activeCarrierID, SuckerRowEnum.S2);

                //(4) 取得線掃巨圖: 2025-08-28 LETIAN: 巨圖統一由 LineScanCamImageHolder 管理其生命週期
                Bitmap bmpFullfov = LineScanCamImageHolder.PeekBitmap();

                //(5) PreEmptyTray
                _PreCheckEmptyTray(bmpFullfov);

                //(6) 晶粒定位
                _RunChipsLocate(bmpFullfov, imgLogPath, out string debugCellCenterStr);
                _TM.Trace("_Inspect001 : 晶粒定位 & 量測 完成!");

                //(7) 異步輸出 Debug 數據
                markFileTimeTag();
                saveDebugDataAsync(bmpFullfov, debugCellCenterStr, imgLogPath);

                //(8) 標記終止計時
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
            finally
            {
                //// 以後如果 其他內部 IDisposable 物件生命週期管理 優化完成
                //// 可以 移除 GC 
                //GC.Collect();
                //GC.WaitForPendingFinalizers();
                //GC.Collect();
            }
        }

        /// <summary>
        /// 提供 給 參數編輯 使用
        /// </summary>
        internal bool LocateOneChip(Bitmap cellBmp, ref RectangleF cellRoi, out GaChipData chipData)
        {
            prepareChipMatcher(0, out var chipMatcher);
            return _LocateOneChip(cellBmp, ref cellRoi, out chipData, chipMatcher);
        }

        /// <summary>
        /// 調試用
        /// </summary>
        internal bool TryLocateOneChip(RegionCellX3Class cell, Bitmap cellBmp, ref RectangleF cellRoi)
        {
            if (cell == null) return false;
            var gaCell = new GaCell(cell, cellBmp, Rectangle.Round(cellRoi));
            _RunChipLocateOneT(0, new[] { gaCell }, null);
            bool ok = cell.ChipData?.ChipQuad2D != null;
            return ok;
        }


        #region PRIVATE_FUNCTIONS

        void _PreCheckEmptyTray(Bitmap bmpFullfov)
        {
            _preEmptyTrayResult = null;
            try
            {
                var aoi = _sysModel.EmptyTrayAoiModel;
                aoi.RunAll(bmpFullfov, wait: true);
                _preEmptyTrayResult = aoi.GetResult();
            }
            catch(Exception ex)
            {
                _LOG_ERROR(ex, "_PreCheckEmptyTray");
            }
        }

        /// <summary>
        /// LETIAN: 晶粒定位
        /// </summary>
        void _RunChipsLocate(Bitmap bmpFullfov, string imgPath, out string debugCellCenterStr)
        {
            _TM.RESET_ACCUM();

            bool usingMultiThread = Universal.N_THREADS_ENABLED;

            DisposeCellGroups();
            int N_GROUPS = MvdCompositeChipMatcher.N_CHANNLS;
            var groups = GaCellsGroup.CollectGroups(N_GROUPS, _xRecipe, bmpFullfov, _preEmptyTrayResult);
            _cellGroups = groups;

            string[] debugStrs = new string[groups.Length];

            //_InstanceBoxOverlapTools(N_GROUPS);

            if (!usingMultiThread)
            {
                // 單線程 (驗證用)
                for (int gid = 0; gid < groups.Length; gid++)
                {
                    debugStrs[gid] = _RunChipLocateOneT(gid, groups[gid], imgPath);
                }
            }
            else
            {
                Parallel.For(0, N_GROUPS, gid =>
                {
                    if (gid < groups.Length)
                        debugStrs[gid] = _RunChipLocateOneT(gid, groups[gid], imgPath);
                });
            }

            debugCellCenterStr = string.Join("", debugStrs);

            _TM.DUMP_ACCUM();
        }

        /// <summary>
        /// LETIAN: 晶粒定位 與 尺寸量測 (區域) (限用於同一線程內)
        /// </summary>
        string _RunChipLocateOneT(int threadIdx, IEnumerable<GaCell> cellsGroup, string imgPath)
        {
            prepareChipMatcher(threadIdx, out IMvdTemplateMatcher chipMatcher);
            //var fullFovSize = cellsGroup.FullFovRect.Size;
            var debugSB = new StringBuilder();

            foreach (var gaCell in cellsGroup)
            {
                RegionCellX3Class cell = gaCell.Cell;
                Bitmap cellBmp = gaCell.CellBmp;
                RectangleF cellRoi = gaCell.CellRoi;

                //(0) 進度條事件
                fire_AoiProgressing(cell);

                //(1) 清除上一次結果
                cell.Reset();

                //(2) 異步保存 Cell 圖像檔案
                if (INI.Instance.IsSaveTestImage && imgPath != null)
                {
                    cell.IsSaveDebugPicture = true;
                    cell.SaveDebugPath = imgPath;
                    saveCellBmpAsync(cellBmp, cell);
                }

                //(3) 像測 (使用 chipMatcher)
                bool ok = _LocateOneChip(cellBmp, ref cellRoi, out var chipData, chipMatcher);

                if (ok)
                {
                    //(4.1) 將 chipBox2D 存回 Gaara 使用的海康 CMvdRectangleF (為了相容舊版)
                    var chipQuad2D = chipData.ChipQuad2D;
                    var chipCentroid = chipQuad2D.Center;
                    cell.SetMvdRunPositionFix(chipQuad2D?.ToCMvdRectangleF());

                    //(4.2) DEBUG_STRING
                    #region 加入_DEBUG_STRING
                    if (true)
                    {
                        var debug_org_center_x = Math.Round(chipCentroid.X - cellRoi.X, 3);
                        var debug_org_center_y = Math.Round(chipCentroid.Y - cellRoi.Y, 3);
                        var debug_center_x = Math.Round(chipCentroid.X, 3);
                        var debug_center_y = Math.Round(chipCentroid.Y, 3);
                        debugSB.Append("INDEX:").Append(cell.Index).Append("#");
                        debugSB.Append("VIEW:").Append(cellRoi.X).Append(";").Append(cellRoi.Y).Append("#");
                        debugSB.Append("ORG:").Append(debug_org_center_x).Append(";").Append(debug_org_center_y).Append("#");
                        debugSB.Append("DES:").Append(debug_center_x).Append(";").Append(debug_center_y).AppendLine();
                    }
                    #endregion

                    //(5) 判定重疊區域比例
                    var xInspect = _xRecipe.InspectParams;
                    if (xInspect.xChipOverlap > 0)
                    {
                        //(5.1) 直接使用 QvBox2D 計算 重疊率
                        double overlapRatio = calcOverlap(gaCell, chipQuad2D, isLocalCoordinate: false);
                        //(5.2) 重疊率 判定結果
                        ok = overlapRatio >= xInspect.xChipOverlap;
                    }

                    if (ok)
                    {
                        //(6) 根據不同載台, 計算補償量
                        var activeCarrierID = getActiveCarrierID();
                        (var motorDelta, var worldDelta) = _transformModel.CalcPlcCompensation(activeCarrierID, chipCentroid, cell.CellRow, cell.CellCol);

                        //(6.1) Angle
                        double angle = _CalcAngle(chipData);

                        //(6.2) 記入 Runtime (Gaara) 所需要的數據
                        cell.RunAngle = (float)Math.Round((angle + INI.Instance.Cal_Bca), 3);
                        cell.RunX = (float)Math.Round((motorDelta.X + INI.Instance.Cal_Bcx), 3);
                        cell.RunY = (float)Math.Round((motorDelta.Y + INI.Instance.Cal_Bcy), 3);

                        //(6.3) 記入 Gaara Sur1 與 Sur2
                        if (_transCS1 != null)
                        {
                            var mp = _transCS1.Trans(chipCentroid);
                            cell.Sur1 = new PointF((float)mp.X, (float)mp.Y);
                        }
                        if (_transCS2 != null)
                        {
                            var mp = _transCS2.Trans(chipCentroid);
                            cell.Sur2 = new PointF((float)mp.X, (float)mp.Y);
                        }

                        //(6.4) 記入 ChipData
                        cell.ChipData = chipData;
                        cell.ChipData.ChipCoords.Angle = cell.RunAngle;
                        cell.ChipData.ChipCoords.Centroid = _transCP?.Trans(chipCentroid);
                    }
                }

                //(7) 設定 Inspect Result Code
                if (!ok)
                {
                    cell.inspectReason = InspectReason.INS_ALIGNERR;
                    cell.inspectReasons.Add(InspectReason.INS_ALIGNERR);
                }
                else
                {
                    cell.inspectReason = InspectReason.PASS;
                }

                //>>> 後面還要使用, 在此不要調用 cellBmp.Dispose() !!!
                //>>> cellBmp.Dispose();
            }

            return debugSB.ToString();
        }

        bool _LocateOneChip(Bitmap cellBmp, ref RectangleF cellRoi, out GaChipData chipData, IMvdTemplateMatcher chipMatcher)
        {
            chipData = null;

            //(1) Match
            bool ok = chipMatcher.RunMatch(cellBmp);

            if (ok)
            {
                //(2) offset
                var padsGrid = chipMatcher.GetResultPadsGrid();
                padsGrid?.Offset(cellRoi.X, cellRoi.Y);
                var chipQuad = chipMatcher.GetResultQuad2D();
                //>>> chipQuad 不需要再次 Offset
                //>>> chipQuad?.Offset(cellRoi.X, cellRoi.Y);

                //(3) 將定位結果記入 cell.ChipData
                chipData = new GaChipData();
                chipData.Roi = cellRoi;
                chipData.PadsGrid = padsGrid;
                chipData.ChipQuad2D = chipQuad;
                chipData.GoldenQuad2D = chipMatcher.GoldenQuad2D?.Clone();

                //(4) DEBUG data
                chipData.DebugRigidBodyData = chipMatcher.GetResultDetails();
            }

            return ok;
        }

        double _CalcAngle(GaChipData chipData)
        {
            var padsGrid = chipData.PadsGrid;
            if (padsGrid != null)
            {
                //-----------------------------------------
                // 0 1
                // 3 2
                //-----------------------------------------
                var corners = Array.ConvertAll(padsGrid.GetCornerBlocs(), b => b?.Center);
                if (corners[0] != null && corners[1] != null && corners[2] != null && corners[3] != null)
                {
                    var L = (corners[0] + corners[3]) / 2.0;
                    var R = (corners[1] + corners[2]) / 2.0;
                    var vect = R - L;
                    var theta = Math.Atan2(vect.Y, vect.X);
                    return theta * 180.0 / Math.PI;
                }
            }
            var chipQuad = chipData.ChipQuad2D;
            if (chipQuad != null)
                return chipQuad.Angle;
            return 0;
        }

        /// <summary>
        /// LETIAN: 非同步保存 cellBmp.
        /// caller 負責 cellBmp 生命
        /// </summary>
        void saveCellBmpAsync(Bitmap cellBmp, RegionCellX3Class cell)
        {
            if (cellBmp == null || cell == null)
                return;

            #region OLD_CODE
            //string posfixpath = cell.SaveDebugPath + "\\PositionFix";
            //if (!Directory.Exists(posfixpath))
            //    Directory.CreateDirectory(posfixpath);
            //cellBmp.Save(posfixpath + $"\\Fix_{cell.Index}_{cell.lblName}.bmp", ImageFormat.Bmp);
            #endregion

            string fname = $"Fix_{cell.Index}_{cell.lblName}.bmp";
            string fullFileName = System.IO.Path.Combine(cell.SaveDebugPath, "PositionFix", fname);

            ThreadPool.QueueUserWorkItem(arg =>
            {
                try
                {
                    object[] args = (object[])arg;
                    string fileName = args[1] as string;
                    using (Bitmap bmp = args[0] as Bitmap)
                    {
                        // 檢查 Path
                        string path = System.IO.Path.GetDirectoryName(fileName);
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);

                        // 保存檔案
                        GaImageUtil.SaveBigImage(fileName, bmp);
                    }
                }
                catch (Exception ex)
                {
                    _LOG_ERROR(ex, "_Inspect001_Async_SaveCellBmp");
                }
            },
                new object[] { cellBmp.Clone(), fullFileName }
            );
        }

        /// <summary>
        /// LETIAN: 非同步保存 Debug 數據 搬移至此.
        /// caller 負責 bmpFullfov 生命
        /// </summary>
        void saveDebugDataAsync(Bitmap bmpFullfov, string debugCellCenterStr, string debugDumpPath)
        {
            if (bmpFullfov == null)
                return;

            if (!INI.Instance.IsSaveTestImage && !INI.Instance.IsSaveDebugBMP && !INI.Instance.IsSaveDebugOrgBmp)
                return;

            ThreadPool.QueueUserWorkItem(arg =>
            {
                try
                {
                    using (Bitmap bmpBig = (Bitmap)arg)
                    {
                        //(1) SAVE debugCellCenterStr
                        if (INI.Instance.IsSaveTestImage && debugDumpPath != null && debugCellCenterStr != null)
                        {
                            //>>> GaUtil.SaveData(debugCellCenterStr, debugDumpPath + $"\\PositionFix\\DEBUG_{DateTime.Now.ToString("yyyyMMddHHmmss")}.txt");

                            if (!System.IO.Directory.Exists(debugDumpPath))
                                System.IO.Directory.CreateDirectory(debugDumpPath);

                            string fileName = System.IO.Path.Combine(debugDumpPath, GetLotFileName(LotId, ".txt"));
                            GaUtil.SaveData(debugCellCenterStr, fileName);
                        }

                        //(2) SAVE debug Bmp
                        if (INI.Instance.IsSaveDebugBMP)
                        {
                            //>>> GaImageUtil.SaveImageWithQuality(ezImage.Bitmap, $"{m_PicResultPath}\\{m_FileName}", INI.Instance.ImageQuality);
                            string fileName = GetDebugBmpFileName();
                            GaImageUtil.SaveImageWithQuality(bmpBig, fileName, INI.Instance.ImageQuality);
                        }

                        //(3) SAVE debug OrgBmp
                        if (INI.Instance.IsSaveDebugOrgBmp)
                        {
                            //>>> ezImage.Save($"{m_PicResultOrgPath}\\{m_FileName}");
                            string fileName = GetDebugOrgBmpFileName();
                            GaImageUtil.SaveBigImage(fileName, bmpBig);
                        }
                    }
                }
                catch (Exception ex)
                {
                    //_LOG($"异常捕获:{ex.Message}", Color.Red);
                    _LOG_ERROR(ex, "_Inspect001_Async_SaveDebugData");
                }
            },
                bmpFullfov.Clone()
            );
        }

        /// <summary>
        /// 準備 Template Matchers
        /// </summary>
        void prepareChipMatcher(int threadIdx, out IMvdTemplateMatcher chipMatcher)
        {
            MvdCompositeChipMatcher matchers = _xRecipe.mvdprinttemp_Find;
            chipMatcher = matchers[threadIdx];
        }

        /// <summary>
        /// 計算覆蓋率
        /// </summary>
        double calcOverlap(GaCell gaCell, QvQuad2D chipQuad, bool isLocalCoordinate = false, bool debug = false)
        {
            var cell = gaCell?.Cell;
            if (cell == null)
                return 0;

            var cellRoi = gaCell.CellRoi;

            var goldenTemplateSize = _xRecipe.PrintTemplateSize;

            // 格點中心 (local coordinates)
            var gridCenter = new System.Drawing.Point(cellRoi.Width / 2, cellRoi.Height / 2);
            var gridRect = JetEazy.Qcvt.CvCreateCenterRect(gridCenter.X, gridCenter.Y, goldenTemplateSize.Width, goldenTemplateSize.Height);

            // polygonPts (local coordinates)
            int offsetX = isLocalCoordinate ? 0 : -cellRoi.X;
            var offsetY = isLocalCoordinate ? 0 : -cellRoi.Y;
            var polygonPts = Array.ConvertAll(chipQuad.Corners, c => new OpenCvSharp.Point((int)c.X + offsetX, (int)c.Y + offsetY));
            for (int i = 0, len = polygonPts.Length; i < len; i++)
                JetEazy.Qcvt.ClipBoundary(ref polygonPts[i], ref gridRect);

            double overlapArea = Cv2.ContourArea(polygonPts, oriented: false);
            double overlapRatio = overlapArea / (gridRect.Width * gridRect.Height + 0.001);

            #region DEBUG_DUMP
            if (debug)
            {
                // 轉至 local coordinate
                var quad = chipQuad;
                if (!isLocalCoordinate)
                {
                    quad = chipQuad.Clone();
                    var cc = quad.Center;
                    cc.X += offsetX;
                    cc.Y += offsetY;
                    quad.SetCenter(cc);
                }

                // Draw
                using (var bridge = new QxImageBridge(gaCell.CellBmp))
                using (Mat canvas = VxDebugDrawer.PrepareCanvas("DEBUG_OVERLAP", bridge.Image, shrink: 1))
                {
                    canvas.Rectangle(gridRect, Scalar.Blue, 3);
                    canvas.Polylines(new[] { polygonPts }, true, Scalar.Red, 3);
                    VxDebugDrawer.Draw(canvas, quad, Scalar.Lime);
                    canvas.SaveImage($"d:\\paso.log\\overlap_{cell.Index:000}.png");
                }
            }
            #endregion

            return overlapRatio;
        }
        /// <summary>
        /// 計算覆蓋率
        /// </summary>
        double calcOverlap(GaCell gaCell, QvBox2D chipBox2D, bool isLocalCoordinate = false, bool debug = false)
        {
            var quad = QvQuad2D.From(chipBox2D);
            return calcOverlap(gaCell, quad, isLocalCoordinate, debug);
        }

        #endregion


        #region HELPERS
        QvBox2D toBox2D(ref AUVision.xFindResult xResult, SizeF size, float offsetX = 0f, float offsetY = 0f)
        {
            var box = new QvBox2D();
            box.SetBox(PointF.Empty, size);
            box.SetCenter(xResult.fCenterX + offsetX, xResult.fCenterY + offsetY);
            box.SetTheta(xResult.fAngle / 180.0 * Math.PI);
            return box;
        }
        QvBox2D toBox2D(PointF center, SizeF size, double angle)
        {
            var box = new QvBox2D();
            box.SetBox(PointF.Empty, size);
            box.SetCenter(center);
            box.SetTheta(angle / 180.0 * Math.PI);
            return box;
        }
        void offset(EzBlocsGrid grid, float dx, float dy)
        {
            foreach(var bloc in grid)
            {
                if (bloc == null) continue;
                //
            }
        }
        #endregion
    }
}
