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
using LaserAlignDX.Model.Coords;
using LaserAlignDX.OPSpace;
using LeTian.AoiLib;
using OpenCvSharp;
using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Traveller106;
using _TM = LeTian.AoiLib.LtDebug;


namespace LaserAlignDX.AoiModel.V3
{
    public class AoiModel_ChipLoc : AoiModelBase
    {
        #region GLOBAL_MESS
        #endregion

        #region KERNEL_MEMBERS
        /// <summary>
        /// 2025-09-10 新座標轉換
        /// </summary>
        TravellerTransforms _transformModel => _sysModel.TransformsModel;
        #endregion

        #region RUNTIME_DATA
        GaCellsGroup[] _cellGroups;
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
            fire_AoiBegin("晶粒定位");

            // 效能追蹤
            _TM.Reset();

            try
            {
                //_xRecipe.AnalyzeDatasData();
                //_TM.Trace("_Inspect001 : xRecipe.AnalyzeDatasData()");

                // 標記起始計時
                markRunStart();

                // 準備資料夾
                string imgLogPath = GetLogPath(this.FileBarcodeStr);

                #region PREPARE_PATH
                if (INI.Instance.IsSaveTestImage)
                {
                    if (!Directory.Exists(imgLogPath))
                        Directory.CreateDirectory(imgLogPath);
                }
                #endregion

                // 取得線掃巨圖: 2025-08-28 LETIAN: 巨圖統一由 LineScanCamImageHolder 管理其生命週期
                Bitmap bmpFullfov = LineScanCamImageHolder.PeekBitmap();

                // 晶粒定位
                RunChipsLocate(bmpFullfov, imgLogPath, out string debugCellCenterStr);
                _TM.Trace("_Inspect001 : 晶粒定位 & 量測 完成!");

                // 異步輸出 Debug 數據
                markFileTimeTag();
                saveDebugDataAsync(bmpFullfov, debugCellCenterStr, imgLogPath);

                //// PASS / NG
                //bool isPass = _Inpsect001_Check_TotalPass();

                // 標記終止計時
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

        #region PRIVATE_FUNCTIONS

        /// <summary>
        /// LETIAN: 晶粒定位
        /// </summary>
        void RunChipsLocate(Bitmap bmpFullfov, string imgPath, out string debugCellCenterStr)
        {
            _TM.RESET_ACCUM();

            bool usingMultiThread = Universal.N_THREADS_ENABLED;

            DisposeCellGroups();
            int N_GROUPS = MvdCompositeChipMatcher.N_CHANNLS;
            var groups = GaCellsGroup.CollectGroups(N_GROUPS, _xRecipe, bmpFullfov);
            _cellGroups = groups;

            string[] debugStrs = new string[groups.Length];

            //_InstanceBoxOverlapTools(N_GROUPS);

            if (!usingMultiThread)
            {
                // 單線程 (驗證用)
                for (int gid = 0; gid < groups.Length; gid++)
                {
                    debugStrs[gid] = RunChipLocateOneT(gid, groups[gid], imgPath);
                }
            }
            else
            {
                Parallel.For(0, N_GROUPS, gid =>
                {
                    if (gid < groups.Length)
                        debugStrs[gid] = RunChipLocateOneT(gid, groups[gid], imgPath);
                });
            }

            debugCellCenterStr = string.Join("", debugStrs);

            _TM.DUMP_ACCUM();
        }

        /// <summary>
        /// LETIAN: 晶粒定位 與 尺寸量測 (區域) (限用於同一線程內)
        /// </summary>
        string RunChipLocateOneT(int threadIdx, GaCellsGroup cellsGroup, string imgPath)
        {
            prepareChipMatcher(threadIdx, out IMvdTemplateMatcher chipMatcher);
            var fullFovSize = cellsGroup.FullFovRect.Size;
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

                #region DEBUG
                //if (cell.Index != 66)
                //    continue;
                #endregion

                //(2) 像測 (使用 chipMatcher)
                //>>> _TM.BEGIN("_RunChipTemplateMatch");
                bool bOK = chipMatcher.RunMatch(cellBmp);
                //>>> _TM.END("_RunChipTemplateMatch");

                //(3) 異步保存 Cell 圖像檔案
                if (INI.Instance.IsSaveTestImage)
                {
                    cell.IsSaveDebugPicture = true;
                    cell.SaveDebugPath = imgPath;
                    saveCellBmpAsync(cellBmp, cell);
                }

                if (bOK)
                {
                    //(4) 使用 xResults[0] 當 Chip Center
                    cell.xFindResult = chipMatcher.xResults[0];
                    var org_center_x = cell.xFindResult.fCenterX;
                    var org_center_y = cell.xFindResult.fCenterY;
                    cell.xFindResult.fCenterX += cellRoi.X;
                    cell.xFindResult.fCenterY += cellRoi.Y;

                    //(4.1) 晶粒定位中心點(camera coorindates)
                    var chipSize = _xRecipe.xRegionTrain.Size;
                    var chipBox2D = toBox2D(ref cell.xFindResult, chipSize);
                    var chipCentroid = new QVector(chipBox2D.Center.X, chipBox2D.Center.Y);
                    //(4.2) 將 chipBox2D 存回 Gaara 使用的海康 CMvdRectangleF
                    cell.SetMvdRunPositionFix(GaMvdExt.ToCMvdRectangleF(chipBox2D));
                    //(4.3) 記入 chipBox2D
                    cell.chipLocInCamera = chipBox2D;

                    #region DEBUG_STRING
                    //debugCellCenterStr += $"INDEX:{cell.Index}#";
                    //debugCellCenterStr += $"VIEW:{cellRoi.X};{cellRoi.Y}#";
                    //debugCellCenterStr += $"ORG:{cell.xFindResult.fCenterX};{cell.xFindResult.fCenterY}#";
                    //debugCellCenterStr += $"DES:{cell.xFindResult.fCenterX};{cell.xFindResult.fCenterY}{Environment.NewLine}";
                    debugSB.Append("INDEX:").Append(cell.Index).Append("#");
                    debugSB.Append("VIEW:").Append(cellRoi.X).Append(";").Append(cellRoi.Y).Append("#");
                    debugSB.Append("ORG:").Append(org_center_x).Append(";").Append(org_center_y).Append("#");
                    debugSB.Append("DES:").Append(cell.xFindResult.fCenterX).Append(";").Append(cell.xFindResult.fCenterY).AppendLine();
                    #endregion

                    //(5) 判定重疊區域比例
                    bool isOverlapOK = true;
                    var xInspect = _xRecipe.InspectParams;
                    if (xInspect.xChipOverlap > 0)
                    {
                        //(5.1) 使用 QvBox2D 計算 重疊率
                        double overlapRatio = calcOverlap(gaCell, chipBox2D, isLocalCoordinate: false);
                        //(5.2) 判定結果
                        isOverlapOK = overlapRatio >= xInspect.xChipOverlap;
                    }

                    if (isOverlapOK)
                    {
                        //(6) 根據不同載台, 計算補償量
                        var activeCarrierID = getActiveCarrierID();
                        (var motorDelta, var worldDelta) = _transformModel.CalcPlcCompensation(activeCarrierID, chipCentroid, cell.CellRow, cell.CellCol);
                        double angle = chipBox2D.Theta * 180 / Math.PI;
                        cell.RunAngle = (float)(angle + INI.Instance.Cal_Bca);
                        cell.RunX = (float)(motorDelta.X + INI.Instance.Cal_Bcx);
                        cell.RunY = (float)(motorDelta.Y + INI.Instance.Cal_Bcy);

#if (false)
                        //(7) 量測尺寸
                        if (xInspect.optChipMeasurement)
                        {
                            //_TM.BEGIN("OneChipMeasurement");

                            #region OLD_CODE
                            //switch (xInspect.MFLType)
                            //{
                            //    //case Eazy_Project_III.MeasureFindLineType.FindLineType_v2:
                            //    //    _Inspect001_One_Chip_Measurement_pairLine(cell, cellBmp, cellRoi, chipMatcher);
                            //    //    break;
                            //    default:
                            //        _Inspect001_One_Chip_Measurement(cell, cellBmp, cellRoi, chipMatcher);
                            //        //Bitmap bmp = cellBmp.Clone(new Rectangle(0, 0, cellBmp.Width, cellBmp.Height), cellBmp.PixelFormat);
                            //        //AForge.Imaging.Filters.SobelEdgeDetector detector = new AForge.Imaging.Filters.SobelEdgeDetector();
                            //        //Bitmap bmp1 = detector.Apply(bmp);
                            //        //AForge.Imaging.Filters.Closing closing = new AForge.Imaging.Filters.Closing();
                            //        //Bitmap bmp2 = closing.Apply(bmp1);
                            //        //AForge.Imaging.Filters.SISThreshold sISThreshold = new AForge.Imaging.Filters.SISThreshold();
                            //        //Bitmap bmp3 = sISThreshold.Apply(bmp2);
                            //        //Bitmap bmp4 = GaImageUtil.ToU8(bmp3, true);
                            //        //_Inspect001_One_Chip_Measurement(cell, bmp4, cellRoi, chipMatcher);

                            //        //bmp.Dispose();
                            //        //bmp1.Dispose();
                            //        //bmp2.Dispose();
                            //        //bmp3.Dispose();
                            //        //bmp4.Dispose();

                            //        break;
                            //}
                            #endregion

                            _Inspect001_One_Chip_Measurement(cell, cellBmp, cellRoi, chipMatcher);

                            //(9) 打包 "尺寸判断" 结果
                            cell.PackMeasureResult();

                            //_TM.END("OneChipMeasurement");
                        }
#endif
                    }
                    else
                    {
                        cell.inspectReason = InspectReason.INS_ALIGNERR;
                        cell.inspectReasons.Add(InspectReason.INS_ALIGNERR);
                    }
                }
                else
                {
                    cell.inspectReason = InspectReason.INS_ALIGNERR;
                    cell.inspectReasons.Add(InspectReason.INS_ALIGNERR);
                }

                //>>> 後面還要使用, 在此不要調用 cellBmp.Dispose() !!!
                //>>> cellBmp.Dispose();
            }

            return debugSB.ToString();
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
        double calcOverlap(GaCell gaCell, QvBox2D chipBox2D, bool isLocalCoordinate = false, bool debug = false)
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
            var polygonPts = Array.ConvertAll(chipBox2D.Corners, c => new OpenCvSharp.Point((int)c.X + offsetX, (int)c.Y + offsetY));
            for (int i = 0, len = polygonPts.Length; i < len; i++)
                JetEazy.Qcvt.ClipBoundary(ref polygonPts[i], ref gridRect);

            double overlapArea = Cv2.ContourArea(polygonPts, oriented: false);
            double overlapRatio = overlapArea / (gridRect.Width * gridRect.Height + 0.001);

            #region DEBUG_DUMP
            if (debug)
            {
                // 轉至 local coordinate
                var box2D = chipBox2D;
                if (!isLocalCoordinate)
                {
                    box2D = chipBox2D.Clone();
                    var cc = box2D.Center;
                    cc.X += offsetX;
                    cc.Y += offsetY;
                    box2D.SetCenter(cc);
                }

                // Draw
                using (var bridge = new QxImageBridge(gaCell.CellBmp))
                using (Mat canvas = VxDebugDrawer.PrepareCanvas("DEBUG_OVERLAP", bridge.Image, shrink: 1))
                {
                    canvas.Rectangle(gridRect, Scalar.Blue, 3);
                    canvas.Polylines(new[] { polygonPts }, true, Scalar.Red, 3);
                    VxDebugDrawer.Draw(canvas, box2D, Scalar.Lime);
                    canvas.SaveImage($"d:\\paso.log\\overlap_{cell.Index:000}.png");
                }
            }
            #endregion

            return overlapRatio;
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
        #endregion
    }
}
