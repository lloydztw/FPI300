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
using JetEazy.Utils;
using LaserAlignDX.BasicSpace;
using LaserAlignDX.Model.Coords;
using LaserAlignDX.Mvc.Model;
using LaserAlignDX.OPSpace;
using LaserAlignDX.OPSpace.RecipeSpace;
using LeTian.AoiLib;
using OpenCvSharp;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Traveller106;
using VisionDesigner;
using _TM = LeTian.AoiLib.LtDebug;
using ProcessEventArgs = NeedleX.ProcessSpace.ProcessEventArgs;


namespace LaserAlignDX.AoiModel.V27
{
    public class ProcessRunFPIClass : IProcessRunFPI
    {
        public event EventHandler<GaProgressEventArgs> OnAoiProgressing;
        public event EventHandler<GaProgressEventArgs> OnAoiBegin;
        public event EventHandler<GaProgressEventArgs> OnAoiEnd;
        public event EventHandler<ProcessEventArgs> OnError;

        #region NLOG
        NLog.Logger _NLOG => LtDebug.LOG;
        #endregion

        #region SINGLETON
        protected ProcessRunFPIClass()
        {

        }
        private static ProcessRunFPIClass _instance = null;
        #endregion

        public static ProcessRunFPIClass Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ProcessRunFPIClass();
                return _instance;
            }
        }
        public static void DisposeAll()
        {
            _instance?.Dispose();
            _instance = null;
        }
        public void Dispose()
        {
            // To DO: 請把自己清乾淨
            //_DisposeTools();
        }

        #region GLOBAL_MESS
        RecipeFPIX3Class xRecipe
        {
            get { return RecipeFPIX3Class.Instance; }
        }
        InspectX3ParaClass xInspect
        {
            get { return InspectX3ParaClass.Instance; }
        }
        #endregion

        #region KENERL_MEMBERS
        /// <summary>
        /// 2025-08-28 LETIAN: 巨圖 統一由 TravellerBigImagesHolder 保管其生命週期
        /// </summary>
        public GaBigImageHolder LineScanCamImageHolder => _sysModel.LineScanImageHolder;
        /// <summary>
        /// 2025-09-10 新座標轉換
        /// </summary>
        TravellerTransforms _transformModel => _sysModel.TransformsModel;
        /// <summary>
        /// SystemModel
        /// </summary>
        ITravelerModel _sysModel => GaMvcConfig.SysModel;
        /// <summary>
        /// 當下的載台
        /// </summary>
        CarrierEnum getActiveCarrierID()
        {
            return _sysModel.ActiveCarrierID;
        }
        #endregion

        #region PRIVATE_STATISTICS_DATA
        private long m_ElapsedTime = 0;
        private bool m_Running = false;
        private bool m_IsPass = false;
        #endregion

        public long ElapsedTime
        {
            get { return m_ElapsedTime; }
        }
        public bool Running
        {
            get { return m_Running; }
        }
        public bool IsPass
        {
            get { return m_IsPass; }
        }

        #region PRIVATE_LOT_DATA
        private string m_StripId = "Strip_NONE";
        private string m_LotId = "Lot_NONE";
        private string m_FileBarcodeStr = string.Empty;
        #endregion

        public string LotId
        {
            get { return m_LotId; }
            set { m_LotId = value; }
        }
        public string StripId
        {
            get { return m_StripId; }
            set { m_StripId = value; }
        }
        public string FileName
        {
            get { return GetLotFileName(LotId, ".txt"); }
        }
        public string FileBarcodeStr
        {
            get
            {
                return m_FileBarcodeStr;
            }
            set
            {
                m_FileBarcodeStr = value;
                MarkFileTimeTag();
            }
        }

        #region PRIVATE_AOI_RUN_OPTIONS
        private ScanInspectMode scanInspectMode = ScanInspectMode.MEASUREAOI;
        private bool m_QrUsed = false;
        private bool m_QrJudged = false;
        #endregion

        public ScanInspectMode xScanInspectMode
        {
            get { return scanInspectMode; }
            set { scanInspectMode = value; }
        }
        public bool QrUsed
        {
            get { return m_QrUsed; }
            set { m_QrUsed = value; }
        }
        public bool QrJudged
        {
            get { return m_QrJudged; }
            set { m_QrJudged = value; }
        }

        #region RESULTS_FOR_PLC
        /// <summary>
        /// 单颗的线扫结果(预留300个) PLC用此信号来将每颗产品放到对应的Tray盘
        /// </summary>
        /// <returns>ARRAY[0..299] OF INT PC->PLC 单颗结果, 1:OK 2:外观NG, 3:空, 4:读码NG, 9:切割NG </returns>
        public int[] GetSingleResult()
        {
            //PC->PLC 单颗结果,1-Ok,2-外观Ng,3-空,4-读码NG,9-切割NG
            int[] states = new int[xRecipe.xRegionCells.Count];
            int i = 0;
            foreach (RegionCellX3Class cell in xRecipe.xRegionCells)
            {
                if (cell.inspectReason == InspectReason.PASS && cell.inspectReasons.Count == 0)
                    states[i] = 1;
                else if (cell.inspectReason == InspectReason.INS_ALIGNERR)
                    states[i] = 3;
                else if (cell.inspectReason == InspectReason.INS_2DERR || cell.inspectReason == InspectReason.INS_2DMAPNG)
                    states[i] = 4;
                else if (cell.inspectReason == InspectReason.INS_CUTTINGERR)
                    states[i] = 9;
                else
                    states[i] = 2;
                i++;
            }
            return states;
        }
        /// <summary>
        /// 单颗产品的读码比对结果(预留300个) 视觉软件需要将读码结果保存在本地或服务器
        /// </summary>
        /// <returns>ARRAY[0..299] OF INT PC->PLC 读码结果, 1:OK, 2:比对NG, 3:空, 4:有码未读到</returns>
        public int[] GetQrResult()
        {
            //PC->PLC 读码结果,1-Ok,2-比对Ng,3-空,4-有码未读到
            int[] states = new int[xRecipe.xRegionCells.Count];
            int i = 0;
            foreach (RegionCellX3Class cell in xRecipe.xRegionCells)
            {
                if (cell.inspectReason == InspectReason.PASS && cell.inspectReasons.Count == 0)
                    states[i] = 1;
                else if (cell.inspectReason == InspectReason.INS_ALIGNERR)
                    states[i] = 3;
                else if (cell.inspectReason == InspectReason.INS_2DERR)
                    states[i] = 4;
                else if (cell.inspectReason == InspectReason.INS_2DMAPNG)
                    states[i] = 2;
                else
                    states[i] = 1;
                i++;
            }
            return states;
        }
        /// <summary>
        /// 单颗产品的偏移值([0]-X,[1]-Y,[2]-R，[3]-X,[4]-Y,[5]-R…依次共300个) 线扫引导功能启用时PLC需要用到这些值
        /// </summary>
        /// <returns>ARRAY[0..899] OF REAL PC->PLC 线扫偏移值XYR</returns>
        public float[] GetScanOffset()
        {
            //PC->PLC 线扫偏移值XYR
            //单颗产品的偏移值([0]-X,[1]-Y,[2]-R，[3]-X,[4]-Y,[5]-R…依次共300个)
            float[] states = new float[xRecipe.xRegionCells.Count * 3];
            int i = 0;
            foreach (RegionCellX3Class cell in xRecipe.xRegionCells)
            {
                if (cell.inspectReason == InspectReason.PASS && cell.inspectReasons.Count == 0)
                {
                    states[i] = cell.RunX;
                    states[i + 1] = cell.RunY;
                    states[i + 2] = cell.RunAngle;
                }
                else if (cell.inspectReason == InspectReason.INS_ALIGNERR)
                {
                    states[i] = 0;
                    states[i + 1] = 0;
                    states[i + 2] = 0;

                }
                else
                {
                    states[i] = cell.RunX;
                    states[i + 1] = cell.RunY;
                    states[i + 2] = cell.RunAngle;

                }
                i += 3;
            }
            return states;
        }
        #endregion

        public void Run()
        {
            m_Running = true;
            m_IsPass = false;

            #region DIRECTORIES_可以搬到後面處理_才不會有遲滯感覺
            //if (INI.Instance.IsSaveDebugBMP)
            //{
            //    m_PicResultPath = $"{INI.Instance.ResultImagePath}\\linescanImage\\{DateTime.Now.ToString("yyyyMMdd")}\\{StripId}";
            //    if (!System.IO.Directory.Exists(m_PicResultPath))
            //    {
            //        System.IO.Directory.CreateDirectory(m_PicResultPath);
            //    }
            //}
            //if (INI.Instance.IsSaveDebugOrgBmp)
            //{
            //    m_PicResultOrgPath = $"{INI.Instance.ResultImagePath}\\linescanImageOrg\\{DateTime.Now.ToString("yyyyMMdd")}\\{StripId}";
            //    if (!System.IO.Directory.Exists(m_PicResultOrgPath))
            //    {
            //        System.IO.Directory.CreateDirectory(m_PicResultOrgPath);
            //    }
            //}
            #endregion

            GaUtil.LOG($"{GetType().Name} [V2.5] Run", Color.Purple);

            Thread thread = new Thread(runTest);
            thread.Priority = ThreadPriority.Highest;
            thread.IsBackground = false;
            thread.Start();
        }
        private void runTest()
        {
            //MarkPathFileTimeTag();

            switch (scanInspectMode)
            {
                case ScanInspectMode.NOTRAY:
                    _Inspect003_LT();
                    break;
                default:
                    _Inspect001_LT();
                    break;
            }
        }

        #region INSPECT_001
        /// <summary>
        /// 外观及尺寸检测
        /// </summary>
        private void _Inspect001_LT()
        {
            fire_AoiBegin();

            // 效能追蹤
            _TM.Reset();

            try
            {
                xRecipe.AnalyzeDatasData();
                _TM.Trace("_Inspect001 : xRecipe.AnalyzeDatasData()");

                // 標記起始計時
                var stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Restart();
                m_ElapsedTime = 0;

                // 準備資料夾
                string imgLogPath = GetLogPath(m_FileBarcodeStr);

                #region PREPARE_PATH
                if (INI.Instance.IsSaveTestImage)
                {
                    if (!Directory.Exists(imgLogPath))
                        Directory.CreateDirectory(imgLogPath);
                }
                #endregion

                // 取得線掃巨圖:
                // 2025-08-28 LETIAN: 巨圖統一由 LineScanCamImageHolder 管理其生命週期
                Bitmap bmpFullfov = LineScanCamImageHolder.PeekBitmap();
                _TM.Trace("_Inspect001 : CMvdImage To Bitmap 完成.");

                // 晶粒定位 與 量測
                _Inspect001_Chip_Location_And_Measurement(bmpFullfov, imgLogPath, out string debugCellCenterStr);
                _TM.Trace("_Inspect001 : 晶粒定位 & 量測 完成!");

                // 读码测试
                _Inspect001_QRCode(bmpFullfov);
                _TM.Trace("_Inspect001 : QRCode 完成!");

                // 異步輸出 Debug 數據
                MarkFileTimeTag();
                _Inspect001_Async_SaveDebugData(bmpFullfov, debugCellCenterStr, imgLogPath);

                // PASS / NG
                m_IsPass = _Inpsect001_Check_TotalPass();

                // 標記終止計時
                stopwatch.Stop();
                m_ElapsedTime = stopwatch.ElapsedMilliseconds;  //@ for Inspect001 計時
                m_Running = false;

                // 2025-08-28 LETIAN: 巨圖統一由 LineScanCamImageHolder 管理其生命週期
                // 在此無需釋放 巨圖

                fire_AoiEnd();
            }
            catch (Exception ex)
            {
                // 2025-08-28 LETIAN: 巨圖統一由 LineScanCamImageHolder 管理其生命週期
                // 在此無需釋放 巨圖
                m_Running = false;
                fire_AoiEnd();
                var errCode = Mvc.Model.ErrCodes.EXCEPTION_AT_AOI_RUN;
                string errMsg = GaUtil.GetEnumDescription(errCode) + "\n\r" + ex.Message;
                OnError?.Invoke(this, new ProcessEventArgs(errMsg, errCode));
                GaUtil.LOG(errMsg, Color.Red);
                _NLOG.Error(ex, "_Inspect001_LT");
            }
            finally
            {
                // 以後如果 其他內部 IDisposable 物件生命週期管理 優化完成
                // 可以 移除 GC 
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
        }

        /// <summary>
        /// LETIAN: 晶粒定位 與 尺寸量測 
        /// </summary>
        private void _Inspect001_Chip_Location_And_Measurement(
                            Bitmap bmpFullfov,
                            string imgPath,
                            out string debugCellCenterStr)
        {
            _TM.RESET_ACCUM();

            bool usingMultiThread = Universal.N_THREADS_ENABLED;
            int N_GROUPS = MvdCompositeChipMatcher.N_CHANNLS;

            var groups = GaCellsGroup.CollectGroups(N_GROUPS, xRecipe, bmpFullfov);
            string[] debugStrs = new string[groups.Length];

            //_InstanceBoxOverlapTools(N_GROUPS);

            if (!usingMultiThread)
            {
                // 單線程 (驗證用)
                for (int gid = 0; gid < groups.Length; gid++)
                {
                    debugStrs[gid] = _Inspect001_Chip_Locate_And_Measure(gid, groups[gid], imgPath);
                }
            }
            else
            {
                Parallel.For(0, N_GROUPS, gid =>
                {
                    if (gid < groups.Length)
                        debugStrs[gid] = _Inspect001_Chip_Locate_And_Measure(gid, groups[gid], imgPath);
                });
            }

            debugCellCenterStr = string.Join("", debugStrs);

            _TM.DUMP_ACCUM();
        }

        /// <summary>
        /// LETIAN: 晶粒定位 與 尺寸量測 (區域) (限用於同一線程內)
        /// </summary>
        private string _Inspect001_Chip_Locate_And_Measure(
                            int threadIdx,
                            GaCellsGroup cellsGroup,
                            string imgPath)
        {
            _PrepareChipMatcher(threadIdx, out IMvdTemplateMatcher chipMatcher);
            //_PrepareBoxOverlapTool(threadIdx, out CBoxOverlapTool cBoxOverlapTool);

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
                    _Inspect001_Async_SaveCellBmp(cellBmp, cell);
                }

                if (bOK && chipMatcher.xResults.Count > 0)
                {
                    //(4) 使用 xResults[0] 當 Chip Center
                    cell.xFindResult = chipMatcher.xResults[0];
                    var debug_org_center_x = cell.xFindResult.fCenterX;
                    var debug_org_center_y = cell.xFindResult.fCenterY;
                    cell.xFindResult.fCenterX += cellRoi.X;
                    cell.xFindResult.fCenterY += cellRoi.Y;

                    //(4.1) 晶粒定位中心點(camera coorindates)
                    var chipSize = xRecipe.xRegionTrain.Size;
                    var chipBox2D = toBox2D(ref cell.xFindResult, chipSize);
                    var chipCentroid = new QVector(chipBox2D.Center.X, chipBox2D.Center.Y);

                    //(4.2) 將定位結果記入 cell.ChipData
                    cell.ChipData.Roi = cellRoi;
                    cell.ChipData.ChipBox2D = chipBox2D;
                    cell.ChipData.PadsGrid = chipMatcher.GetResultPadsGrid();
                    cell.ChipData.PadsGrid.Offset(cellRoi.X, cellRoi.Y);

                    //(4.3) 將 chipBox2D 存回 Gaara 使用的海康 CMvdRectangleF (為了相容舊版)
                    cell.SetMvdRunPositionFix(GaMvdExt.ToCMvdRectangleF(chipBox2D));

                    #region DEBUG_STRING
                    //debugCellCenterStr += $"INDEX:{cell.Index}#";
                    //debugCellCenterStr += $"VIEW:{cellRoi.X};{cellRoi.Y}#";
                    //debugCellCenterStr += $"ORG:{cell.xFindResult.fCenterX};{cell.xFindResult.fCenterY}#";
                    //debugCellCenterStr += $"DES:{cell.xFindResult.fCenterX};{cell.xFindResult.fCenterY}{Environment.NewLine}";
                    debugSB.Append("INDEX:").Append(cell.Index).Append("#");
                    debugSB.Append("VIEW:").Append(cellRoi.X).Append(";").Append(cellRoi.Y).Append("#");
                    debugSB.Append("ORG:").Append(debug_org_center_x).Append(";").Append(debug_org_center_y).Append("#");
                    debugSB.Append("DES:").Append(cell.xFindResult.fCenterX).Append(";").Append(cell.xFindResult.fCenterY).AppendLine();
                    #endregion

                    //(5) 判定重疊區域比例
                    bool isOverlapOK = true;
                    if (xInspect.xChipOverlap > 0)
                    {
#if (OPT_OLD_CODE)
                        //(5.1) 笨笨的使用 海康 MVD Tool 找出 Rotated Rect
                        RectangleF templateRectF = new RectangleF(0, 0, xRecipe.PrintTemplateSize.Width, xRecipe.PrintTemplateSize.Height);
                        Rectangle runRect = cellsGroup.FullFovRect;
                        cell.PositionFixRun(templateRectF, runRect, cell.xFindResult);

                        // LETIAN: 集中 cell.DrawResultRectF() 調用一次就好;
                        //         不然每調用一次, 其內部就 new 一次 物件s !
                        CMvdRectangleF cellmvdRectF = cell.DrawResultRectF();

                        // 增加重叠区域的判断
                        cBoxOverlapTool.ROI1 = GaImageUtil.ToCMvdRectangleF(ref cell.viewRectF);
                        cBoxOverlapTool.ROI2 = cellmvdRectF;
                        cBoxOverlapTool.Run();

                        //(5.1) 判定結果
                        isOverlapOK = cBoxOverlapTool.Result.Overlap >= xInspect.xChipOverlap;
#endif
                        //(5.2) 使用 QvBox2D 計算 重疊率
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

                        //(8) 量測尺寸
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

                cellBmp.Dispose();
            }

            return debugSB.ToString();
        }

        /// <summary>
        /// 量測單一晶粒 (直线寻找)
        /// </summary>
        private void _Inspect001_One_Chip_Measurement(RegionCellX3Class cell, Bitmap cellBmp, RectangleF cellRoi, IMvdTemplateMatcher matcher = null)
        {
            // 取得 上一輪 晶粒定位 的結果 (xResult)
            if (matcher.xResults.Count == 0)
                return;

            var chipLocationResult = matcher.xResults[0];
            var chipBox2D = cell.ChipData.ChipBox2D;
            var activeCarrierID = getActiveCarrierID();
            var transCP = _transformModel.GetCameraPhysicTransform(activeCarrierID);

            #region 邊線處理
            EdgeBorder eBorder = EdgeBorder.Left;

            try
            {
                RectangleF[] rcpBorderBoxes = new RectangleF[]
                {
                    xRecipe.xLineLeft,
                    xRecipe.xLineTop,
                    xRecipe.xLineRight,
                    xRecipe.xLineBottom,
                };

                for (int borderIdx = 0, N = rcpBorderBoxes.Length; borderIdx < N; borderIdx++)
                {
                    eBorder = (EdgeBorder)borderIdx;

                    RectangleF borderBox = rcpBorderBoxes[borderIdx];

                    CMvdRectangleF mvdBorderBox = GaImageUtil.ToCMvdRectangleF(ref borderBox);

                    CMvdRectangleF mvdCellRoi = cell.PositionFixRun(
                                                    mvdBorderBox,
                                                    xRecipe.xRegionTrain,
                                                    Rectangle.Round(cellRoi),
                                                    chipLocationResult) as CMvdRectangleF;

                    // 海康線檢 (輸出為 cell.cMvdShapesForFindLineRegion)
                    cell.LineSegmentRun(borderIdx, cellBmp, mvdCellRoi);

                    // Offset
                    mvdCellRoi.CenterX += cellRoi.X;
                    mvdCellRoi.CenterY += cellRoi.Y;

                    // 更新到 cell
                    cell.cMvdShapesForFindLineRegion[borderIdx] = (CMvdShape)mvdCellRoi.Clone();
                }
            }
            catch (Exception ex)
            {
                string borderName = JetEazy.QxNums.GetEnumDescription(eBorder);
                _NLOG.Error(ex, $"{borderName} 定位異常");
                throw ex;
            }
            #endregion

#if (OLD_CODE)
            #region 長度量測
            EzLSD.LineSegment line0 = null;     //左邊線
            EzLSD.LineSegment line2 = null;     //右邊線
            try
            {
                line0 = cell.cMvdLineSegmentFsOut[0]?.ToLineSegment();  //左邊線
                line2 = cell.cMvdLineSegmentFsOut[2]?.ToLineSegment();  //右邊線
                if (line0 != null && line2 != null)
                {
                    //>>> LineSegments 是在 Cell Roi Coordinates
                    line0.Offset(cellRoi.X, cellRoi.Y);
                    line2.Offset(cellRoi.X, cellRoi.Y);
                    // 轉換到 Physic Coordinates 重組 line segment, 再行計算距離.
                    var P1 = transCP.Trans(line0.P1);
                    var P2 = transCP.Trans(line0.P2);
                    var Q1 = transCP.Trans(line2.P1);
                    var Q2 = transCP.Trans(line2.P2);
                    line0 = new EzLSD.LineSegment(P1, P2);
                    line2 = new EzLSD.LineSegment(Q1, Q2);
                    // 計算點線距離
                    var P = (P1 + P2) / 2;
                    double dist = line2.CalcDistance(P);
                    cell.RunWidth = (float)Math.Round(dist, 3);
                }
            }
            catch (MvdException ex)
            {
                //Console.WriteLine("Fail with ErrorCode: 0x" + ex.ErrorCode.ToString("X"));
                _NLOG.Error(ex, "長度量測 異常: ErrorCode = 0x{0:X}", ex.ErrorCode);
            }
            catch (System.Exception ex)
            {
                //Console.WriteLine("Fail with error " + ex.Message);
                _NLOG.Error(ex, "長度量測 異常");
            }
            #endregion

            #region 寬度量測
            EzLSD.LineSegment line1 = null;     //上邊線
            EzLSD.LineSegment line3 = null;     //下邊線
            try
            {
                line1 = cell.cMvdLineSegmentFsOut[1]?.ToLineSegment();
                line3 = cell.cMvdLineSegmentFsOut[3]?.ToLineSegment();
                if (line1 != null && line3 != null)
                {
                    //>>> LineSegments 是在 Cell Roi Coordinates
                    line1.Offset(cellRoi.X, cellRoi.Y);
                    line3.Offset(cellRoi.X, cellRoi.Y);
                    // 轉換到 Physic Coordinates 重組 line segment, 再行計算距離.
                    var P1 = transCP.Trans(line1.P1);
                    var P2 = transCP.Trans(line1.P2);
                    var Q1 = transCP.Trans(line3.P1);
                    var Q2 = transCP.Trans(line3.P2);
                    line1 = new EzLSD.LineSegment(P1, P2);
                    line3 = new EzLSD.LineSegment(Q1, Q2);
                    // 計算點線距離
                    var P = (P1 + P2) / 2;
                    double dist = line3.CalcDistance(P);
                    cell.RunHeight = (float)Math.Round(dist, 3);
                }
            }
            catch (MvdException ex)
            {
                //Console.WriteLine("Fail with ErrorCode: 0x" + ex.ErrorCode.ToString("X"));
                _NLOG.Error(ex, "寬度量測 異常: ErrorCode = 0x{0:X}", ex.ErrorCode);
            }
            catch (System.Exception ex)
            {
                //Console.WriteLine("Fail with error " + ex.Message);
                _NLOG.Error(ex, "寬度量測 異常");
            }
            #endregion
#endif

            #region 尺寸長寬量測
            EzLSD.LineSegment line0 = null;     //左 邊線
            EzLSD.LineSegment line1 = null;     //上 邊線
            EzLSD.LineSegment line2 = null;     //右 邊線
            EzLSD.LineSegment line3 = null;     //下 邊線
            try
            {
                // 轉換 CMvdLine
                var lines = Array.ConvertAll(cell.cMvdLineSegmentFsOut, mvdLine => mvdLine?.ToLineSegment());
                foreach (var line in lines)
                    line?.Offset(cellRoi.X, cellRoi.Y);
                line0 = lines[0];
                line1 = lines[1];
                line2 = lines[2];
                line3 = lines[3];
                CalcChipDimension(lines, out SizeF dimension, true);
                cell.RunWidth = dimension.Width;
                cell.RunHeight = dimension.Height;
            }
            catch (Exception ex)
            {
                _NLOG.Error(ex, "晶粒 長寬量測 異常");
            }
            #endregion

            #region 計算格點型晶粒的邊緣寬度
            if (xInspect.xAlgorithm == MatchAlgorithmEnum.GridMatch && xInspect.optChipEdgesDiffCompare && chipBox2D != null)
            {
                try
                {
                    //晶格角點: 左上, 右上, 右下, 左下
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
                    left = transCP.Trans(left);
                    right = transCP.Trans(right);
                    top = transCP.Trans(top);
                    bottom = transCP.Trans(bottom);

                    if (line0 != null && line2 != null)
                    {
                        //晶格 左邊緣 厚度 = 晶格左點 至 左邊線(line0) 距離
                        var edge_left = line0.CalcDistance(left);
                        //晶格 右邊緣 厚度 = 晶格右點 至 右邊線(line2) 距離
                        var edge_right = line2.CalcDistance(right);
                        //記入 結果
                        cell.PadEdgeSizes[(int)EdgeBorder.Left] = (float)Math.Round(edge_left, 3);
                        cell.PadEdgeSizes[(int)EdgeBorder.Right] = (float)Math.Round(edge_left, 3);
                    }

                    if (line1 != null && line3 != null)
                    {
                        //晶格 上邊緣 厚度 = 晶格上點 至 上邊線(line1) 距離
                        var edge_top = line1.CalcDistance(top);
                        //晶格 下邊緣 厚度 = 晶格下點 至 下邊線(line3) 距離
                        var edge_bottom = line3.CalcDistance(bottom);
                        //記入 結果
                        cell.PadEdgeSizes[(int)EdgeBorder.Top] = (float)Math.Round(edge_top, 3);
                        cell.PadEdgeSizes[(int)EdgeBorder.Bottom] = (float)Math.Round(edge_bottom, 3);
                    }
                }
                catch (MvdException ex)
                {
                    _NLOG.Error(ex, "計算格點型晶粒的邊緣寬度 異常");
                }
            }
            #endregion
        }
        /// <summary>
        /// LETIAN: 读码测试 搬移至此.
        /// caller 負責 bmpInputImage 生命
        /// </summary>
        private void _Inspect001_QRCode(Bitmap bmpFullfov)
        {
            if (m_QrUsed || xInspect.optChipDefectsInspect)
            {
                foreach (RegionCellX3Class cell in xRecipe.xRegionCells)
                {
                    if (cell.ByPass && !INI.Instance.IsForceInspect)
                        continue;

                    if (cell.inspectReason == InspectReason.INS_ALIGNERR)
                        continue;

                    // Golden Region Size
                    var regionSize = xRecipe.bmpprinttemplate.Size;

                    // 定位完成后裁切位置
                    // RectangleF _crop = new RectangleF(
                    //    cell.DrawResultRectF().CenterX - regionSize.Width / 2,
                    //    cell.DrawResultRectF().CenterY - regionSize.Height / 2,
                    //    regionSize.Width,
                    //    regionSize.Height);

                    var mvdRect = cell.DrawResultRectF();
                    var regionRoi = JetEazy.Qcvt.CreateCenterRect(mvdRect.CenterX, mvdRect.CenterY, regionSize.Width, regionSize.Height);

                    if (xInspect.optChipDefectsInspect)
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

                            var bmpTemplate = xRecipe.bmpDefectTemplate;
                            var bmpMask = xRecipe.bmpprintmask;
                            var roi = xRecipe.xRegionTrain;
                            roi.X += regionRoi.X;
                            roi.Y += regionRoi.Y;

                            using (var bmpRun = bmpFullfov.Clone(roi, PixelFormat.Format8bppIndexed))
                            {
                                cell.DetectDefects(bmpTemplate, bmpRun, bmpMask);
                            }
                        }
                        catch (Exception ex)
                        {
                            _NLOG.Error(ex, "cell.DetectDefects 異常!");
                            xInspect.optChipDefectsInspect = false;
                        }
                    }

                    if (m_QrUsed)
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

                            var roi = xRecipe.xRectCodeRegion;
                            roi.X += regionRoi.X;
                            roi.Y += regionRoi.Y;
                            using (var bmpRun = bmpFullfov.Clone(roi, PixelFormat.Format8bppIndexed))
                            {
                                //cell.DeCode2D(cell.bmpItemCodeRun, _cropCode.Location, m_QrJudged);
                                cell.DeCode2D(bmpRun, roi.Location, m_QrJudged);
                            }
                        }
                        catch (Exception ex)
                        {
                            _NLOG.Error(ex, "cell.DeCode2D 異常!");
                            //xInspect.m_QrUsed = false;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// LETIAN: 非同步保存 Debug 數據 搬移至此.
        /// caller 負責 bmpInputImage 生命
        /// </summary>
        private void _Inspect001_Async_SaveDebugData(Bitmap bmpFullfov, string debugCellCenterStr, string debugDumpPath)
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
        /// LETIAN: 非同步保存 Cell Bitmap
        /// </summary>
        private void _Inspect001_Async_SaveCellBmp(Bitmap cellBmp, RegionCellX3Class cell)
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
        private bool _Inpsect001_Check_TotalPass()
        {
            foreach (var cell in xRecipe.xRegionCells)
            {
                if (cell == null) continue;

                bool isPass = true;
                if (cell.inspectReason == InspectReason.PASS && cell.inspectReasons.Count == 0)
                    isPass = true;
                else if (cell.inspectReason != InspectReason.INS_ALIGNERR)
                    isPass = false;

                if (!isPass)
                    return false;
            }
            return true;
        }
        #endregion

        public bool CalcChipDimension(EzLSD.LineSegment[] lines, out SizeF chipSize, bool usePostScale)
        {
            chipSize = SizeF.Empty;
            var transCP = _transformModel?.GetCameraPhysicTransform(getActiveCarrierID());
            if (transCP == null)
                return false;

            #region 長度量測
            bool ok1 = false;
            try
            {
                var line0 = lines[0];     //左邊線
                var line2 = lines[2];     //右邊線
                if (line0 != null && line2 != null)
                {
                    //>>> LineSegments 是在 Cell Roi Coordinates
                    //line0.Offset(cellRoi.X, cellRoi.Y);
                    //line2.Offset(cellRoi.X, cellRoi.Y);
                    // 轉換到 Physic Coordinates 重組 line segment, 再行計算距離.
                    var P1 = transCP.Trans(line0.P1);
                    var P2 = transCP.Trans(line0.P2);
                    var Q1 = transCP.Trans(line2.P1);
                    var Q2 = transCP.Trans(line2.P2);
                    line0 = new EzLSD.LineSegment(P1, P2);
                    line2 = new EzLSD.LineSegment(Q1, Q2);
                    // 計算點線距離
                    var P = (P1 + P2) / 2;
                    double dist = line2.CalcDistance(P);
                    chipSize.Width = (float)dist;   // Math.Round(dist, 3);
                    ok1 = true;
                }
            }
            catch (MvdException ex)
            {
                //Console.WriteLine("Fail with ErrorCode: 0x" + ex.ErrorCode.ToString("X"));
                _NLOG.Error(ex, "長度量測 異常: ErrorCode = 0x{0:X}", ex.ErrorCode);
            }
            catch (System.Exception ex)
            {
                //Console.WriteLine("Fail with error " + ex.Message);
                _NLOG.Error(ex, "長度量測 異常");
            }
            #endregion

            #region 寬度量測
            bool ok2 = false;
            try
            {
                var line1 = lines[1];     //上邊線
                var line3 = lines[3];     //下邊線
                if (line1 != null && line3 != null)
                {
                    ////>>> LineSegments 是在 Cell Roi Coordinates
                    //line1.Offset(cellRoi.X, cellRoi.Y);
                    //line3.Offset(cellRoi.X, cellRoi.Y);
                    // 轉換到 Physic Coordinates 重組 line segment, 再行計算距離.
                    var P1 = transCP.Trans(line1.P1);
                    var P2 = transCP.Trans(line1.P2);
                    var Q1 = transCP.Trans(line3.P1);
                    var Q2 = transCP.Trans(line3.P2);
                    line1 = new EzLSD.LineSegment(P1, P2);
                    line3 = new EzLSD.LineSegment(Q1, Q2);
                    // 計算點線距離
                    var P = (P1 + P2) / 2;
                    double dist = line3.CalcDistance(P);
                    chipSize.Height = (float)dist;  // Math.Round(dist, 3);
                    ok2 = true;
                }
            }
            catch (MvdException ex)
            {
                _NLOG.Error(ex, "寬度量測 異常: ErrorCode = 0x{0:X}", ex.ErrorCode);
            }
            catch (System.Exception ex)
            {
                //Console.WriteLine("Fail with error " + ex.Message);
                _NLOG.Error(ex, "寬度量測 異常");
            }
            #endregion

            if (usePostScale)
            {
                chipSize.Width = (float)Math.Round(chipSize.Width * xInspect.xChipDimScaleW, 3);
                chipSize.Height = (float)Math.Round(chipSize.Height * xInspect.xChipDimScaleH, 3);
            }
            else
            {
                chipSize.Width = (float)Math.Round(chipSize.Width, 3);
                chipSize.Height = (float)Math.Round(chipSize.Height, 3);
            }

            return ok1 && ok2;
        }

        #region INSPECT_003_EMPTY_TRAY
        /// <summary>
        /// 空载台检测
        /// </summary>
        private void _Inspect003_LT()
        {
            fire_AoiBegin();

            xRecipe.AnalyzeDatasData();

            m_ElapsedTime = 0;
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Restart();

            var aoiModel = GaMvcConfig.SysModel.EmptyTrayAoiModel;

            Bitmap bmpFullFov = LineScanCamImageHolder.PeekBitmap();
            if (bmpFullFov != null)
            {
                //复位所有数据
                #region RESET_DATA
                xRecipe.xOutBlocs.Clear();
                foreach (var cell in xRecipe.xRegionCells)
                    cell?.Reset();
                #endregion

                aoiModel.RunAll(bmpFullFov, wait: true);

                var result = aoiModel.GetResult();

                _Inspect003_UpdateResult(result, bmpFullFov, xRecipe);

                // 異步輸出 Debug 數據
                MarkFileTimeTag();
                _Inspect003_Async_SaveDebugData(bmpFullFov);
            }

            //m_IsPass = true;//不需要结果 都是记录单颗的数据
            stopwatch.Stop();
            m_ElapsedTime = stopwatch.ElapsedMilliseconds;  //@ for Inspect003
            m_Running = false;

            fire_AoiEnd();
        }
        /// <summary>
        /// 空载台检测 : 更新結果 到 Gaara 數據群
        /// </summary>
        private void _Inspect003_UpdateResult(EzEmptyTrayResult result, Bitmap bmpFullFov, RecipeFPIX3Class xRecipe)
        {
            //string imgPath = $"{Universal.LOG_IMG_PATH}\\{JzTimes.DateSerialString}\\{m_FileBarcodeStr}";
            #region PREPARE_PATH
            //if (INI.Instance.IsSaveTestImage)
            //{
            //    if (!Directory.Exists(imgPath))
            //        Directory.CreateDirectory(imgPath);
            //}
            #endregion

            bool isAllPass = result != null;

            if (result != null)
            {
                int fullRows = result.FullRows;
                int fullCols = result.FullCols;

                //Z字型对位资料
                //int index = 0;
                //for (int row = 0; row < fullRows; row++)
                //{
                //    if (row % 2 == 1)
                //    {
                //        for (int col = fullCols - 1; col > -1; col--)
                //        {
                //            result.GetBlocByRowCol(row, col, out EzBloc bloc, out bool isOK);
                //            var cell = xRecipe.xRegionCells[index];
                //            InspectReason reason = (isOK ? InspectReason.INS_ALIGNERR : InspectReason.INS_DEFECTERR);
                //            //if (bloc == null)
                //            //    reason = InspectReason.INS_DEFECTERR;
                //            cell.inspectReason = reason;
                //            cell.inspectReasons.Add(reason);
                //            System.Diagnostics.Trace.WriteLine($"[{row}, {col}] is " + (isOK ? "OK" : "NG"));
                //            index++;
                //        }
                //    }
                //    else
                //    {
                //        for (int col = 0; col < fullCols; col++)
                //        {
                //            result.GetBlocByRowCol(row, col, out EzBloc bloc, out bool isOK);
                //            var cell = xRecipe.xRegionCells[index];
                //            InspectReason reason = (isOK ? InspectReason.INS_ALIGNERR : InspectReason.INS_DEFECTERR);
                //            //if (bloc == null)
                //            //    reason = InspectReason.INS_DEFECTERR;
                //            cell.inspectReason = reason;
                //            cell.inspectReasons.Add(reason);
                //            System.Diagnostics.Trace.WriteLine($"[{row}, {col}] is " + (isOK ? "OK" : "NG"));
                //            index++;
                //        }
                //    }
                //}

                int index = 0;
                var xRegionCells = xRecipe.xRegionCells;
                foreach ((int row, int col) in Zigzag.IterZigzag(fullRows, fullCols))
                {
                    if (index >= xRegionCells.Count)
                        break;

                    var cell = xRegionCells[index];
                    result.GetBlocByRowCol(row, col, out EzBloc bloc, out bool isOK);

                    InspectReason reason = (isOK ? InspectReason.INS_ALIGNERR : InspectReason.INS_DEFECTERR);
                    //if (bloc == null)
                    //    reason = InspectReason.INS_DEFECTERR;
                    cell.inspectReason = reason;
                    cell.inspectReasons.Add(reason);
                    //System.Diagnostics.Trace.WriteLine($"[{row}, {col}] is " + (isOK ? "OK" : "NG"));

                    if (!isOK)
                        isAllPass = false;

                    index++;
                }

                #region 收集阵列之外的料件

                foreach (var bloc in result.IterOutGridAbnormalBlocs())
                {
                    xRecipe.xOutBlocs.Add(bloc.Rect);
                    isAllPass = false;
                }

                #endregion
            }

            m_IsPass = isAllPass;

            #region WRITE_TO_LOG
            string msg = result != null ? result.ToString() : "空盤檢測: 無結果";
            GaUtil.LOG(msg, isAllPass ? Color.Green : Color.Red);
            #endregion
        }
        /// <summary>
        /// LETIAN: 非同步保存 Debug 數據 搬移至此.
        /// 此函式 負責 bmpInputImage 生命
        /// </summary>
        private void _Inspect003_Async_SaveDebugData(Bitmap bmpFullFov)
        {
            if (!INI.Instance.IsSaveDebugBMP || bmpFullFov == null)
                return;

            ThreadPool.QueueUserWorkItem(arg =>
            {
                try
                {
                    using (Bitmap bmpBig = (Bitmap)arg)
                    {
                        if (INI.Instance.IsSaveDebugBMP)
                        {
                            //GaImageUtil.SaveImageWithQuality(bmpBig,
                            //    $"{m_PicResultPath}\\{LotId}-{DateTime.Now.ToString("yyyyMMddHHmmss")}.jpg", INI.Instance.ImageQuality);
                            string fileName = GetDebugBmpFileName();
                            GaImageUtil.SaveImageWithQuality(bmpBig, fileName, INI.Instance.ImageQuality);
                        }

                        if (INI.Instance.IsSaveDebugOrgBmp)
                        {
                            //ezImage.Save($"{m_PicResultOrgPath}\\{LotId}-{DateTime.Now.ToString("yyyyMMddHHmmss")}.jpg");
                            //bmpInputImage.Save($"{m_PicResultOrgPath}\\{LotId}-{DateTime.Now.ToString("yyyyMMddHHmmss")}.jpg",
                            //                   ImageFormat.Jpeg);
                            string fileName = GetDebugOrgBmpFileName();
                            GaImageUtil.SaveBigImage(fileName, bmpBig);
                        }
                    }
                }
                catch (Exception ex)
                {
                    //_LOG($"异常捕获:{ex.Message}", Color.Red);
                    _LOG_ERROR(ex, "_Inspect003_Async_SaveDebugData");
                }
            },
                bmpFullFov.Clone()
            );
        }
        #endregion

        #region PRIVATE_PATH_FILE_FUNCTIONS
        private DateTime _timeTag = DateTime.Now;
        /// <summary>
        /// 標定統一的存檔時間
        /// </summary>
        void MarkFileTimeTag()
        {
            _timeTag = DateTime.Now;
        }
        string GetLotFileName(string tag, string ext)
        {
            //m_FileName = $"{LotId}-{DateTime.Now.ToString("yyyyMMddHHmmss")}.jpg";
            return $"{tag}-{_timeTag:yyyyMMddHHmmss}{ext}";
        }
        string GetDebugBmpFileName()
        {
            return GetDebugImgSaveFileName("LineScanImage", StripId, LotId);
        }
        string GetDebugOrgBmpFileName()
        {
            return GetDebugImgSaveFileName("LineScanImageOrg", StripId, LotId);
        }
        string GetDebugImgSaveFileName(string subFolder, string stripID, string lotID, bool autoCreateDir = true)
        {
            //>>> m_PicResultOrgPath = $"{INI.Instance.ResultImagePath}\\linescanImageOrg\\{DateTime.Now.ToString("yyyyMMdd")}\\{StripId}";
            //>>> m_PicResultOrgPath = $"{INI.Instance.ResultImagePath}\\linescanImageOrg\\{DateTime.Now.ToString("yyyyMMdd")}\\{StripId}";

            string path = System.IO.Path.Combine(INI.Instance.ResultImagePath, subFolder, _timeTag.ToString("yyyyMMdd"), stripID);
            if (autoCreateDir && !System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
            string file = $"{lotID}-{_timeTag:yyyyMMddHHmmss}.jpg";
            return System.IO.Path.Combine(path, file);
        }
        string GetLogPath(string subFolder)
        {
            return System.IO.Path.Combine(Universal.LOG_IMG_PATH, _timeTag.ToString("yyyyMMdd"), subFolder);
        }
        #endregion

        #region EVENT_FUNCTIONS
        int _progressCount = 0;
        void fire_AoiBegin()
        {
            _progressCount = 0;
            if (OnAoiBegin != null)
            {
                int total = xRecipe.xRegionCells.Count;
                OnAoiBegin?.Invoke(this, new GaProgressEventArgs(total, 0));
            }
        }
        void fire_AoiEnd()
        {
            if (OnAoiEnd != null)
            {
                int total = xRecipe.xRegionCells.Count;
                OnAoiEnd?.Invoke(this, new GaProgressEventArgs(total, total));
            }
        }
        void fire_AoiProgressing(RegionCellX3Class cell)
        {
            //int currentStep = cell.CellCol + cell.CellRow * xRecipe.xColumn;
            //if (currentStep <= _progressCount)
            //    return;
            //_progressCount = currentStep;

            Interlocked.Increment(ref _progressCount);
            int currentStep = _progressCount;

            if (OnAoiProgressing != null)
            {
                int total = xRecipe.xRegionCells.Count;
                OnAoiProgressing?.Invoke(this, new GaProgressEventArgs(total, currentStep));
            }
        }
        #endregion

        #region LOG_FUNCTIONS
        void _LOG_ERROR(Exception ex, string message)
        {
            _NLOG.Error(ex, message);
            GaUtil.LOG($"[異常] {ex.Message}", Color.Red);
        }
        #endregion

        #region CALC_OVERLAP_RATIO
        double calcOverlap(GaCell gaCell, QvBox2D chipBox2D, bool isLocalCoordinate = false, bool debug = false)
        {
            var cell = gaCell?.Cell;
            if (cell == null)
                return 0;

            var cellRoi = gaCell.CellRoi;

            var goldenTemplateSize = xRecipe.PrintTemplateSize;

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

        void _PrepareChipMatcher(int threadIdx, out IMvdTemplateMatcher chipMatcher)
        {
            MvdCompositeChipMatcher matchers = xRecipe.mvdprinttemp_Find;
            chipMatcher = matchers[threadIdx];
        }
    }
}
