using EzAoiEmptyTrayInspector.Model;
using JetEazy.BasicSpace;
using JetEazy.EzImage;
using JetEazy.Match;
using JetEazy.Utils;
using LaserAlignDX.OPSpace;
using LaserAlignDX.OPSpace.RecipeSpace;
using LeTian.AoiLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Traveller106;
using VisionDesigner;
using VisionDesigner.BoxOverlap;
using _TM = LeTian.AoiLib.LtDebug;


namespace LaserAlignDX.AoiModel.V2
{
    public class ProcessRunFPIClass : IProcessRunFPI
    {
        public event EventHandler<GaProgressEventArgs> OnAoiProgressing;
        public event EventHandler<GaProgressEventArgs> OnAoiBegin;
        public event EventHandler<GaProgressEventArgs> OnAoiEnd;

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
            _DisposeTools();
        }

        #region PRIVATE_DATA
        RecipeFPIX3Class xRecipe
        {
            get { return RecipeFPIX3Class.Instance; }
        }
        InspectX3ParaClass xInspect
        {
            get { return InspectX3ParaClass.Instance; }
        }
        LineScanCalibrateClass LineScanCalibrate1
        {
            get { return xRecipe.lineScanCalibrate; }
        }
        LineScanCalibrateClass LineScanCalibrate2
        {
            get { return xRecipe.lineScanCalibrate2; }
        }
        #endregion

        /// <summary>
        /// 2025-08-28 LETIAN: 巨圖 統一由 TravellerBigImagesHolder 保管其生命週期
        /// </summary>
        public GaBigImageHolder LineScanCamImageHolder
        {
            //get => TravellerBigImagesHolder.Instance.LineScanImageHolder;
            get => GaMvcConfig.SysModel.LineScanImageHolder;
        }
        Bitmap PeekLineScanBitmap()
        {
            return LineScanCamImageHolder?.PeekBitmap();
        }

        #region PRIVATE_MEMBERS
        //private CMvdImage m_MvdOpeate = new CMvdImage();
        private long m_ElapsedTime = 0;
        private bool m_Running = false;

        private bool m_IsPass = false;
        private string m_ResultDesc = string.Empty;
        private string m_FileBarcodeStr = string.Empty;
        private ScanInspectMode scanInspectMode = ScanInspectMode.MEASUREAOI;

        private bool m_QrUsed = false;
        private bool m_QrJudged = false;

        //VisionDesigner.BoxOverlap.CBoxOverlapTool cBoxOverlapTool = null;
        private string m_StripId = "Strip_NONE";
        private string m_LotId = "Lot_NONE";
        private string m_PicResultPath = INI.Instance.ResultImagePath;
        private string m_PicResultOrgPath = INI.Instance.ResultImagePath;
        private string m_FileName = string.Empty;
        #endregion
        public string FileName
        {
            get { return m_FileName; }
            set { m_FileName = value; }
        }
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
        public ScanInspectMode xScanInspectMode
        {
            get { return scanInspectMode; }
            set { scanInspectMode = value; }
        }
        public string FileBarcodeStr
        {
            get { return m_FileBarcodeStr; }
            set { m_FileBarcodeStr = value; }
        }
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
        public string ResultDesc
        {
            get { return m_ResultDesc; }
        }

        public void Run()
        {
            m_IsPass = true;
            m_ResultDesc = string.Empty;

            if (INI.Instance.IsSaveDebugBMP)
            {
                m_PicResultPath = $"{INI.Instance.ResultImagePath}\\linescanImage\\{DateTime.Now.ToString("yyyyMMdd")}\\{StripId}";
                if (!System.IO.Directory.Exists(m_PicResultPath))
                {
                    System.IO.Directory.CreateDirectory(m_PicResultPath);
                }
            }

            if (INI.Instance.IsSaveDebugOrgBmp)
            {
                m_PicResultOrgPath = $"{INI.Instance.ResultImagePath}\\linescanImageOrg\\{DateTime.Now.ToString("yyyyMMdd")}\\{StripId}";
                if (!System.IO.Directory.Exists(m_PicResultOrgPath))
                {
                    System.IO.Directory.CreateDirectory(m_PicResultOrgPath);
                }
            }

            //if (!myRecipe.Ischip_open_measure)
            //    return;
            //runTest();

            GaUtil.LOG($"{GetType().Name} [V2] Run", Color.Purple);

            Thread thread = new Thread(runTest);
            thread.Priority = ThreadPriority.Highest;
            thread.IsBackground = false;
            thread.Start();
        }

        /// <summary>
        /// 单颗的线扫结果(预留300个) PLC用此信号来将每颗产品放到对应的Tray盘
        /// </summary>
        /// <returns>ARRAY[0..299] OF INT PC->PLC 单颗结果,1-Ok,2-外观Ng,3-空,4-读码NG,9-切割NG</returns>
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
        /// <returns>ARRAY[0..299] OF INT PC->PLC 读码结果,1-Ok,2-比对Ng,3-空,4-有码未读到</returns>
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

        #region PRIVATE_GAARA_FUNCTIONS
        private void runTest()
        {
            switch (scanInspectMode)
            {
                //case ScanInspectMode.MEASUREAOI:
                //    _Inspect001();
                //    break;
                //case ScanInspectMode.QRCODE:
                //    _Inspect002();
                //    break;
                case ScanInspectMode.NOTRAY:
                    //_Inspect001();
                    _Inspect003_LT();
                    break;
                default:
                    _Inspect001_LT();
                    break;
            }
        }
        #endregion

        #region INSPECT_001
        /// <summary>
        /// 外观及尺寸检测
        /// </summary>
        private void _Inspect001_LT()
        {
            fire_AoiBegin();

            // 效能追蹤
            _TM.Reset();

            // 線掃的巨圖
            Bitmap bmpInputImage = null;

            try
            {
                xRecipe.AnalyzeDatasData();
                _TM.Trace("_Inspect001 : xRecipe.AnalyzeDatasData()");

                // 標記起始計時
                m_ElapsedTime = 0;
                m_Running = true;
                var stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Restart();

                // 準備資料夾
                string imgPath = $"{Universal.LOG_IMG_PATH}\\{JzTimes.DateSerialString}\\{m_FileBarcodeStr}";

                #region PREPARE_PATH
                if (INI.Instance.IsSaveTestImage)
                {
                    if (!Directory.Exists(imgPath))
                        Directory.CreateDirectory(imgPath);
                }
                #endregion

                // 取得線掃巨圖
                // 2025-08-28 LETIAN: 巨圖統一由 LineScanCamImageHolder 管理其生命週期
                bmpInputImage = this.LineScanCamImageHolder?.PeekBitmap();
                _TM.Trace("_Inspect001 : CMvdImage To Bitmap 完成.");

                // 晶粒定位 與 量測
                _Inspect001_Chip_Location_And_Measurement(bmpInputImage, imgPath, out string debugCellCenterStr);
                _TM.Trace("_Inspect001 : 晶粒定位 & 量測 完成!");

                // 读码测试
                _Inspect001_QRCode(bmpInputImage);
                _TM.Trace("_Inspect001 : QRCode 完成!");

                // 異步輸出 Debug 數據
                _Inspect001_Async_SaveDebugData(bmpInputImage, debugCellCenterStr, imgPath);

                // 標記終止計時
                m_IsPass = true;
                stopwatch.Stop();
                m_ElapsedTime = stopwatch.ElapsedMilliseconds;  //@ for Inspect001 計時
                m_Running = false;

                // 2025-08-28 LETIAN: 巨圖統一由 LineScanCamImageHolder 管理其生命週期
                //// 釋放巨圖
                //bmpInputImage?.Dispose();
                //bmpInputImage = null;

                fire_AoiEnd();
            }
            catch (Exception ex)
            {
                _TM.LOG.Error(ex);

                // 2025-08-28 LETIAN: 巨圖統一由 LineScanCamImageHolder 管理其生命週期
                //// 釋放巨圖
                //bmpInputImage?.Dispose();
                //bmpInputImage = null;
                //throw ex;

                m_IsPass = true;
                m_Running = false;
                LtDebug.LOG.Error(ex, $"{GetType().Name}.Inspect001_LT()");
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
                            Bitmap bmpInputImage,
                            string imgPath,
                            out string debugCellCenterStr)
        {
            _TM.RESET_ACCUM();

            bool usingMultiThread = Universal.N_THREADS_ENABLED;
            int N_GROUPS = MvdCompositeChipMatcher.N_CHANNLS;

            var groups = GaCellsGroup.CollectGroups(N_GROUPS, xRecipe, bmpInputImage);
            string[] debugStrs = new string[groups.Length];

            _InstanceBoxOverlapTools(N_GROUPS);

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
            _PrepareBoxOverlapTool(threadIdx, out CBoxOverlapTool cBoxOverlapTool);

            var fullFovSize = cellsGroup.FullFovRect.Size;
            string debugCellCenterStr = "";

            foreach (var gaCell in cellsGroup)
            {
                RegionCellX3Class cell = gaCell.Cell;
                Bitmap cellBmp = gaCell.CellBmp;
                RectangleF cellRoi = gaCell.CellRoi;

                // 進度條事件
                fire_AoiProgressing(cell);

                cell.Reset();

                if (INI.Instance.IsSaveTestImage)
                {
                    cell.IsSaveDebugPicture = true;
                    cell.SaveDebugPath = imgPath;
                }

                //_TM.BEGIN("_RunChipTemplateMatch");
                bool bOK = chipMatcher.RunMatch(cellBmp);
                //_TM.END("_RunChipTemplateMatch");

                if (cell.IsSaveDebugPicture)
                {
                    #region SAVE_CELL_BMP
                    string posfixpath = cell.SaveDebugPath + "\\PositionFix";
                    if (!Directory.Exists(posfixpath))
                        Directory.CreateDirectory(posfixpath);
                    cellBmp.Save(posfixpath + $"\\Fix_{cell.Index}_{cell.lblName}.bmp", ImageFormat.Bmp);
                    #endregion
                }

                if (bOK)
                {
                    //>>> cell.xFindResult = xRecipe.mvdprinttemp_Find.xResults[0];
                    cell.xFindResult = chipMatcher.xResults[0];

                    debugCellCenterStr += $"INDEX:{cell.Index}#";
                    debugCellCenterStr += $"VIEW:{cellRoi.X};{cellRoi.Y}#";
                    debugCellCenterStr += $"ORG:{cell.xFindResult.fCenterX};{cell.xFindResult.fCenterY}#";

                    cell.xFindResult.fCenterX += cellRoi.X;
                    cell.xFindResult.fCenterY += cellRoi.Y;

                    debugCellCenterStr += $"DES:{cell.xFindResult.fCenterX};{cell.xFindResult.fCenterY}{Environment.NewLine}";

                    RectangleF templateRectF = new RectangleF(0, 0, xRecipe.PrintTemplateSize.Width, xRecipe.PrintTemplateSize.Height);
                    Rectangle runRect = cellsGroup.FullFovRect;
                    cell.PositionFixRun(templateRectF, runRect, cell.xFindResult);

                    // LETIAN: 集中 cell.DrawResultRectF() 調用一次就好;
                    //         不然每調用一次, 其內部就 new 一次 物件s !
                    CMvdRectangleF cellmvdRectF = cell.DrawResultRectF();

                    //增加重叠区域的判断
                    //cBoxOverlapTool.ROI1 = new CMvdRectangleF(
                    //    cell.viewRectF.X + cell.viewRectF.Width / 2,
                    //    cell.viewRectF.Y + cell.viewRectF.Height / 2,
                    //    cell.viewRectF.Width,
                    //    cell.viewRectF.Height);
                    cBoxOverlapTool.ROI1 = GaImageUtil.ToCMvdRectangleF(ref cell.viewRectF);
                    cBoxOverlapTool.ROI2 = cellmvdRectF;
                    cBoxOverlapTool.Run();

                    if (cBoxOverlapTool.Result.Overlap >= xInspect.xChipOverlap)
                    {
                        //计算偏移值
                        PointF _viewNewRun = new PointF(cellmvdRectF.CenterX, cellmvdRectF.CenterY);
                        PointF _worldNewRun = LineScanCalibrate1.ViewToWorld(_viewNewRun);
                        PointF _worldNewRun2 = LineScanCalibrate1.ViewToWorld(_viewNewRun);

                        PointF ptOffset = new PointF(LineScanCalibrate2.ptsworld[0].X - LineScanCalibrate1.ptsworld[0].X,
                                                     LineScanCalibrate2.ptsworld[1].Y - LineScanCalibrate1.ptsworld[1].Y);

                        // GAARA 2025-08-14
                        ////原来基础位置的world坐标
                        //PointF _viewOrg = new PointF(cell.viewRectF.X + cell.viewRectF.Width / 2,
                        //                             cell.viewRectF.Y + cell.viewRectF.Height / 2);
                        //PointF _worldOrg = LineScanCalibrate.ViewToWorld(_viewOrg);
                        //cell.OrgX = _worldOrg.X;
                        //cell.OrgY = _worldOrg.Y;

                        cell.Sur1 = new PointF(_worldNewRun.X, _worldNewRun.Y);
                        cell.Sur2 = new PointF(_worldNewRun.X + ptOffset.X, _worldNewRun.Y + ptOffset.Y);

                        cell.RunX = (_worldNewRun.X - cell.OrgX) + INI.Instance.Cal_Bcx;
                        cell.RunY = (_worldNewRun.Y - cell.OrgY) + INI.Instance.Cal_Bcy;
                        cell.RunAngle = cellmvdRectF.Angle + INI.Instance.Cal_Bca;

                        cell.GetOffsetResult();

                        if (xInspect.bOpenLineMeasure)
                        {
                            //_TM.BEGIN("OneChipMeasurement");

                            switch(xInspect.MFLType)
                            {
                                case Eazy_Project_III.MeasureFindLineType.FindLineType_v2:
                                    _Inspect001_One_Chip_Measurement_pairLine(cell, cellBmp, cellRoi, chipMatcher);
                                    break;
                                default:
                                    _Inspect001_One_Chip_Measurement(cell, cellBmp, cellRoi, chipMatcher);
                                    //Bitmap bmp = cellBmp.Clone(new Rectangle(0, 0, cellBmp.Width, cellBmp.Height), cellBmp.PixelFormat);
                                    //AForge.Imaging.Filters.SobelEdgeDetector detector = new AForge.Imaging.Filters.SobelEdgeDetector();
                                    //Bitmap bmp1 = detector.Apply(bmp);
                                    //AForge.Imaging.Filters.Closing closing = new AForge.Imaging.Filters.Closing();
                                    //Bitmap bmp2 = closing.Apply(bmp1);
                                    //AForge.Imaging.Filters.SISThreshold sISThreshold = new AForge.Imaging.Filters.SISThreshold();
                                    //Bitmap bmp3 = sISThreshold.Apply(bmp2);
                                    //Bitmap bmp4 = GaImageUtil.ToU8(bmp3, true);
                                    //_Inspect001_One_Chip_Measurement(cell, bmp4, cellRoi, chipMatcher);

                                    //bmp.Dispose();
                                    //bmp1.Dispose();
                                    //bmp2.Dispose();
                                    //bmp3.Dispose();
                                    //bmp4.Dispose();

                                    break;
                            }

                            //判断尺寸结果
                            cell.GetMeasureResult();

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

            return debugCellCenterStr;
        }

        /// <summary>
        /// 量測單一晶粒 (直线寻找)
        /// </summary>
        private void _Inspect001_One_Chip_Measurement(RegionCellX3Class cell, Bitmap cellBmp, RectangleF cellRoi, IMvdTemplateMatcher matcher = null)
        {
            // 取得 上一輪 晶粒定位 的結果 (xResult)
            var chipLocationResult = matcher.xResults[0];
            // 取得 上一輪 晶粒定位 的 PADs 資訊
            //var padsGrid = matcher.GetResultPadsGrid();

            #region 邊線處理
#if (OPT_OLD || true)
            string borderName = "";
            try
            {
                //左边
                borderName = "左邊線";
                RectangleF r0 = new RectangleF( xRecipe.xLineLeft.X,
                                                xRecipe.xLineLeft.Y,
                                                xRecipe.xLineLeft.Width,
                                                xRecipe.xLineLeft.Height );
                CMvdRectangleF mv0 = new CMvdRectangleF(r0.X + r0.Width / 2, r0.Y + r0.Height / 2, r0.Width, r0.Height);
                CMvdRectangleF mv0ret = cell.PositionFixRun(mv0, 
                                            xRecipe.xRegionTrain, 
                                            Rectangle.Round(cellRoi),
                                            chipLocationResult) as CMvdRectangleF;
                cell.LineSegmentRun(0, cellBmp, mv0ret);
                mv0ret.CenterX += cellRoi.X;
                mv0ret.CenterY += cellRoi.Y;
                cell.cMvdShapesForFindLineRegion[0] = (CMvdShape)mv0ret.Clone();

                //上边
                borderName = "上邊線";
                RectangleF r1 = new RectangleF( xRecipe.xLineTop.X,
                                                xRecipe.xLineTop.Y,
                                                xRecipe.xLineTop.Width,
                                                xRecipe.xLineTop.Height );
                CMvdRectangleF mv1 = new CMvdRectangleF(r1.X + r1.Width / 2, r1.Y + r1.Height / 2, r1.Width, r1.Height);
                CMvdRectangleF mv1ret = cell.PositionFixRun(mv1, 
                                                xRecipe.xRegionTrain, 
                                                Rectangle.Round(cellRoi),
                                                chipLocationResult) as CMvdRectangleF;
                cell.LineSegmentRun(1, cellBmp, mv1ret);
                mv1ret.CenterX += cellRoi.X;
                mv1ret.CenterY += cellRoi.Y;
                cell.cMvdShapesForFindLineRegion[1] = (CMvdShape)mv1ret.Clone();

                //右边
                borderName = "右邊線";
                RectangleF r2 = new RectangleF( xRecipe.xLineRight.X,
                                                xRecipe.xLineRight.Y,
                                                xRecipe.xLineRight.Width,
                                                xRecipe.xLineRight.Height);
                CMvdRectangleF mv2 = new CMvdRectangleF(r2.X + r2.Width / 2, r2.Y + r2.Height / 2, r2.Width, r2.Height);
                CMvdRectangleF mv2ret = cell.PositionFixRun(mv2, 
                                                xRecipe.xRegionTrain, 
                                                Rectangle.Round(cellRoi),
                                                chipLocationResult) as CMvdRectangleF;
                cell.LineSegmentRun(2, cellBmp, mv2ret);
                mv2ret.CenterX += cellRoi.X;
                mv2ret.CenterY += cellRoi.Y;
                cell.cMvdShapesForFindLineRegion[2] = (CMvdShape)mv2ret.Clone();

                //下边
                borderName = "下邊線";
                RectangleF r3 = new RectangleF( xRecipe.xLineBottom.X,
                                                xRecipe.xLineBottom.Y,
                                                xRecipe.xLineBottom.Width,
                                                xRecipe.xLineBottom.Height);
                CMvdRectangleF mv3 = new CMvdRectangleF(r3.X + r3.Width / 2, r3.Y + r3.Height / 2, r3.Width, r3.Height);
                CMvdRectangleF mv3ret = cell.PositionFixRun(mv3, 
                                                xRecipe.xRegionTrain, 
                                                Rectangle.Round(cellRoi),
                                                chipLocationResult) as CMvdRectangleF;
                cell.LineSegmentRun(3, cellBmp, mv3ret);
                mv3ret.CenterX += cellRoi.X;
                mv3ret.CenterY += cellRoi.Y;
                cell.cMvdShapesForFindLineRegion[3] = (CMvdShape)mv3ret.Clone();
            }
            catch (Exception ex)
            {
                _TM.LOG.Error(ex, $"{borderName} 量測異常");
                //throw ex;
            }
#else
            string borderName = "";
            try
            {
                string[] borderNames = new string[] { "左邊線", "上邊線", "右邊線", "下邊線" };
                RectangleF[] borderRects = new RectangleF[]
                {
                    xRecipe.xLineLeft,
                    xRecipe.xLineTop,
                    xRecipe.xLineRight,
                    xRecipe.xLineBottom,
                };

                for (int borderIdx = 0; borderIdx < borderNames.Length; borderIdx++)
                {
                    borderName = borderNames[borderIdx];
                    //RectangleF r0 = new RectangleF(xRecipe.xLineLeft.X,
                    //                                xRecipe.xLineLeft.Y,
                    //                                xRecipe.xLineLeft.Width,
                    //                                xRecipe.xLineLeft.Height);
                    //CMvdRectangleF mv0 = new CMvdRectangleF(r0.X + r0.Width / 2, r0.Y + r0.Height / 2, r0.Width, r0.Height);
                    
                    RectangleF borderRectF = borderRects[borderIdx];
                    CMvdRectangleF mv0 = GaImageUtil.ToCMvdRectangleF(ref borderRectF);
                    CMvdRectangleF mvdRet = cell.PositionFixRun(
                                                    mv0,
                                                    xRecipe.xRegionTrain,
                                                    Rectangle.Round(cellRoi),
                                                    chipLocationResult ) as CMvdRectangleF;

                    cell.LineSegmentRun(borderIdx, cellBmp, mvdRet);

                    // Offset
                    mvdRet.CenterX += cellRoi.X;
                    mvdRet.CenterY += cellRoi.Y;

                    // 更新到 cell
                    cell.cMvdShapesForFindLineRegion[borderIdx] = (CMvdShape)mvdRet.Clone();
                }
            }
            catch (Exception ex)
            {
                LtDebug.LOG.Error(ex, $"{borderName} 定位異常");
                throw ex;
            }
#endif
            #endregion

            #region 長度量測
            try
            {
#if OPT_OLD
                if (cell.cMvdLineSegmentFsOut[0] != null && cell.cMvdLineSegmentFsOut[2] != null)
                {
                    // 使用 MVD VisionDesigner Tool
                    using (var cL2LMeasureToolObj = new VisionDesigner.L2LMeasure.CL2LMeasureTool())
                    {
                        // Set basic parameter
                        cL2LMeasureToolObj.BasicParam.Line1 = cell.cMvdLineSegmentFsOut[0];
                        cL2LMeasureToolObj.BasicParam.Line2 = cell.cMvdLineSegmentFsOut[2];
                        //cL2LMeasureToolObj.BasicParam.Line1.StartPoint = new MVD_POINT_F(100f, 100f);
                        //cL2LMeasureToolObj.BasicParam.Line1.EndPoint = new MVD_POINT_F(150f, 150f);
                        //cL2LMeasureToolObj.BasicParam.Line2.StartPoint = new MVD_POINT_F(300f, 300f);
                        //cL2LMeasureToolObj.BasicParam.Line2.EndPoint = new MVD_POINT_F(250f, 350f);

                        // Running
                        cL2LMeasureToolObj.Run();

                        // Get the result
                        var cL2LMeasureRes = cL2LMeasureToolObj.Result;

                        // Update result to cell (pixels to physical)
                        // 目前只是簡單假設: 線掃 與 載盤 在同一平面
                        // ToDO: 必須處理透視投影引進的誤差 !!!
                        cell.RunWidth = cL2LMeasureRes.VerticalAbsDist * INI.Instance.ImageResolution;

                        //Console.WriteLine("Angle: {0}", cL2LMeasureRes.Angle);
                        //Console.WriteLine("Vertical distance: {0}", cL2LMeasureRes.VerticalAbsDist);
                        _TM.LOG.Info("長度量測: Angle = {0:0.00}", cL2LMeasureRes.Angle);
                        _TM.LOG.Info("長度量測: Vertical distance = {0:0.000}", cL2LMeasureRes.VerticalAbsDist);
                    }
                }
#else
                if (cell.cMvdLineSegmentFsOut[0] != null && cell.cMvdLineSegmentFsOut[2] != null)
                {
                    // 使用 MVD VisionDesigner Tool
                    using (var cL2LMeasureToolObj = new VisionDesigner.L2LMeasure.CL2LMeasureTool())
                    {
                        // Set basic parameter
                        //cL2LMeasureToolObj.BasicParam.Line1 = getRealLine(cell.cMvdLineSegmentFsOut[0]);
                        //cL2LMeasureToolObj.BasicParam.Line2 = getRealLine(cell.cMvdLineSegmentFsOut[2]);

                        cL2LMeasureToolObj.BasicParam.Line1 = cell.cMvdLineSegmentFsOut[0];
                        cL2LMeasureToolObj.BasicParam.Line2 = cell.cMvdLineSegmentFsOut[2];

                        // Running
                        cL2LMeasureToolObj.Run();

                        // Get the result
                        var cL2LMeasureRes = cL2LMeasureToolObj.Result;

                        // Update result to cell (pixels to physical)
                        // 目前只是簡單假設: 線掃 與 載盤 在同一平面
                        // ToDO: 必須處理透視投影引進的誤差 !!!
                        cell.RunWidth = (float)Math.Round(cL2LMeasureRes.VerticalAbsDist * INI.Instance.ImageResolutionX, 3);

                        //Console.WriteLine("Angle: {0}", cL2LMeasureRes.Angle);
                        //Console.WriteLine("Vertical distance: {0}", cL2LMeasureRes.VerticalAbsDist);
                        LtDebug.LOG.Info("長度量測: Angle = {0:0.00}", cL2LMeasureRes.Angle);
                        LtDebug.LOG.Info("長度量測: Vertical distance = {0:0.000}", cL2LMeasureRes.VerticalAbsDist);
                    }
                }
#endif
            }
            catch (MvdException ex)
            {
                //Console.WriteLine("Fail with ErrorCode: 0x" + ex.ErrorCode.ToString("X"));
                _TM.LOG.Error(ex, "長度量測 異常: ErrorCode = 0x{0:X}", ex.ErrorCode);
            }
            catch (System.Exception ex)
            {
                //Console.WriteLine("Fail with error " + ex.Message);
                _TM.LOG.Error(ex, "長度量測 異常");
            }
            #endregion

            #region 寬度量測
            try
            {
#if OPT_OLD
                if (cell.cMvdLineSegmentFsOut[1] != null && cell.cMvdLineSegmentFsOut[3] != null)
                {
                    // 使用 MVD VisionDesigner Tool
                    using (var cL2LMeasureToolObj = new VisionDesigner.L2LMeasure.CL2LMeasureTool())
                    {
                        // Set basic parameter
                        cL2LMeasureToolObj.BasicParam.Line1 = cell.cMvdLineSegmentFsOut[1];
                        cL2LMeasureToolObj.BasicParam.Line2 = cell.cMvdLineSegmentFsOut[3];
                        //cL2LMeasureToolObj.BasicParam.Line1.StartPoint = new MVD_POINT_F(100f, 100f);
                        //cL2LMeasureToolObj.BasicParam.Line1.EndPoint = new MVD_POINT_F(150f, 150f);
                        //cL2LMeasureToolObj.BasicParam.Line2.StartPoint = new MVD_POINT_F(300f, 300f);
                        //cL2LMeasureToolObj.BasicParam.Line2.EndPoint = new MVD_POINT_F(250f, 350f);

                        // Running
                        cL2LMeasureToolObj.Run();

                        // Get the result
                        var cL2LMeasureRes = cL2LMeasureToolObj.Result;

                        // Update result to Cell (pixels to physic)
                        // 目前只是簡單假設: 線掃 與 載盤 在同一平面
                        // ToDO: 必須處理透視投影引進的誤差 !!!
                        cell.RunHeight = cL2LMeasureRes.VerticalAbsDist * INI.Instance.ImageResolution;

                        //Console.WriteLine("Angle: {0}", cL2LMeasureRes.Angle);
                        //Console.WriteLine("Vertical distance: {0}", cL2LMeasureRes.VerticalAbsDist);
                        _TM.LOG.Info("寬度量測: Angle = {0:0.00}", cL2LMeasureRes.Angle);
                        _TM.LOG.Info("寬度量測: Vertical distance = {0:0.000}", cL2LMeasureRes.VerticalAbsDist);
                    }
                }
#else
                if (cell.cMvdLineSegmentFsOut[1] != null && cell.cMvdLineSegmentFsOut[3] != null)
                {
                    // 使用 MVD VisionDesigner Tool
                    using (var cL2LMeasureToolObj = new VisionDesigner.L2LMeasure.CL2LMeasureTool())
                    {
                        // Set basic parameter
                        //cL2LMeasureToolObj.BasicParam.Line1 = getRealLine(cell.cMvdLineSegmentFsOut[1]);
                        //cL2LMeasureToolObj.BasicParam.Line2 = getRealLine(cell.cMvdLineSegmentFsOut[3]);

                        cL2LMeasureToolObj.BasicParam.Line1 = cell.cMvdLineSegmentFsOut[1];
                        cL2LMeasureToolObj.BasicParam.Line2 = cell.cMvdLineSegmentFsOut[3];

                        // Running
                        cL2LMeasureToolObj.Run();

                        // Get the result
                        var cL2LMeasureRes = cL2LMeasureToolObj.Result;

                        // Update result to Cell (pixels to physic)
                        // 目前只是簡單假設: 線掃 與 載盤 在同一平面
                        // ToDO: 必須處理透視投影引進的誤差 !!!
                        cell.RunHeight = (float)Math.Round(cL2LMeasureRes.VerticalAbsDist * INI.Instance.ImageResolutionY, 3);

                        //Console.WriteLine("Angle: {0}", cL2LMeasureRes.Angle);
                        //Console.WriteLine("Vertical distance: {0}", cL2LMeasureRes.VerticalAbsDist);
                        LtDebug.LOG.Info("寬度量測: Angle = {0:0.00}", cL2LMeasureRes.Angle);
                        LtDebug.LOG.Info("寬度量測: Vertical distance = {0:0.000}", cL2LMeasureRes.VerticalAbsDist);
                    }
                }
#endif
            }
            catch (MvdException ex)
            {
                //Console.WriteLine("Fail with ErrorCode: 0x" + ex.ErrorCode.ToString("X"));
                LtDebug.LOG.Error(ex, "寬度量測 異常: ErrorCode = 0x{0:X}", ex.ErrorCode);
            }
            catch (System.Exception ex)
            {
                //Console.WriteLine("Fail with error " + ex.Message);
                LtDebug.LOG.Error(ex, "寬度量測 異常");
            }
            #endregion
        }
        /// <summary>
        /// 量測單一晶粒 (平行线寻找)
        /// </summary>
        private void _Inspect001_One_Chip_Measurement_pairLine(RegionCellX3Class cell, Bitmap cellBmp, RectangleF cellRoi, IMvdTemplateMatcher matcher = null)
        {
            // 取得 上一輪 晶粒定位 的結果
            var chipLocationResult = matcher.xResults[0];
            //var chipLocationResult = cell.xFindResult;

            #region 邊線處理
#if (OPT_OLD || true)
            string borderName = "";
            try
            {
                //左边
                borderName = "左邊線";
                RectangleF r0 = new RectangleF(xRecipe.xLineLeft.X,
                                                xRecipe.xLineLeft.Y,
                                                xRecipe.xLineLeft.Width,
                                                xRecipe.xLineLeft.Height);
                CMvdRectangleF mv0 = new CMvdRectangleF(r0.X + r0.Width / 2, r0.Y + r0.Height / 2, r0.Width, r0.Height);
                CMvdRectangleF mv0ret = cell.PositionFixRun(mv0,
                                            xRecipe.xRegionTrain,
                                            Rectangle.Round(cellRoi),
                                            chipLocationResult) as CMvdRectangleF;
                cell.pairLineSegmentRun(0, cellBmp, mv0ret);
                mv0ret.CenterX += cellRoi.X;
                mv0ret.CenterY += cellRoi.Y;
                cell.cMvdShapesForFindLineRegion[0] = (CMvdShape)mv0ret.Clone();

                //上边
                borderName = "上邊線";
                RectangleF r1 = new RectangleF(xRecipe.xLineTop.X,
                                                xRecipe.xLineTop.Y,
                                                xRecipe.xLineTop.Width,
                                                xRecipe.xLineTop.Height);
                CMvdRectangleF mv1 = new CMvdRectangleF(r1.X + r1.Width / 2, r1.Y + r1.Height / 2, r1.Width, r1.Height);
                CMvdRectangleF mv1ret = cell.PositionFixRun(mv1,
                                                xRecipe.xRegionTrain,
                                                Rectangle.Round(cellRoi),
                                                chipLocationResult) as CMvdRectangleF;
                cell.pairLineSegmentRun(1, cellBmp, mv1ret);
                mv1ret.CenterX += cellRoi.X;
                mv1ret.CenterY += cellRoi.Y;
                cell.cMvdShapesForFindLineRegion[1] = (CMvdShape)mv1ret.Clone();

                //右边
                borderName = "右邊線";
                RectangleF r2 = new RectangleF(xRecipe.xLineRight.X,
                                                xRecipe.xLineRight.Y,
                                                xRecipe.xLineRight.Width,
                                                xRecipe.xLineRight.Height);
                CMvdRectangleF mv2 = new CMvdRectangleF(r2.X + r2.Width / 2, r2.Y + r2.Height / 2, r2.Width, r2.Height);
                CMvdRectangleF mv2ret = cell.PositionFixRun(mv2,
                                                xRecipe.xRegionTrain,
                                                Rectangle.Round(cellRoi),
                                                chipLocationResult) as CMvdRectangleF;
                cell.pairLineSegmentRun(2, cellBmp, mv2ret);
                mv2ret.CenterX += cellRoi.X;
                mv2ret.CenterY += cellRoi.Y;
                cell.cMvdShapesForFindLineRegion[2] = (CMvdShape)mv2ret.Clone();

                //下边
                borderName = "下邊線";
                RectangleF r3 = new RectangleF(xRecipe.xLineBottom.X,
                                                xRecipe.xLineBottom.Y,
                                                xRecipe.xLineBottom.Width,
                                                xRecipe.xLineBottom.Height);
                CMvdRectangleF mv3 = new CMvdRectangleF(r3.X + r3.Width / 2, r3.Y + r3.Height / 2, r3.Width, r3.Height);
                CMvdRectangleF mv3ret = cell.PositionFixRun(mv3,
                                                xRecipe.xRegionTrain,
                                                Rectangle.Round(cellRoi),
                                                chipLocationResult) as CMvdRectangleF;
                cell.pairLineSegmentRun(3, cellBmp, mv3ret);
                mv3ret.CenterX += cellRoi.X;
                mv3ret.CenterY += cellRoi.Y;
                cell.cMvdShapesForFindLineRegion[3] = (CMvdShape)mv3ret.Clone();
            }
            catch (Exception ex)
            {
                LtDebug.LOG.Error(ex, $"{borderName} 量測異常");
                //throw ex;
            }
#else
            string borderName = "";
            try
            {
                string[] borderNames = new string[] { "左邊線", "上邊線", "右邊線", "下邊線" };
                RectangleF[] borderRects = new RectangleF[]
                {
                    xRecipe.xLineLeft,
                    xRecipe.xLineTop,
                    xRecipe.xLineRight,
                    xRecipe.xLineBottom,
                };

                for (int borderIdx = 0; borderIdx < borderNames.Length; borderIdx++)
                {
                    borderName = borderNames[borderIdx];
                    //RectangleF r0 = new RectangleF(xRecipe.xLineLeft.X,
                    //                                xRecipe.xLineLeft.Y,
                    //                                xRecipe.xLineLeft.Width,
                    //                                xRecipe.xLineLeft.Height);
                    //CMvdRectangleF mv0 = new CMvdRectangleF(r0.X + r0.Width / 2, r0.Y + r0.Height / 2, r0.Width, r0.Height);
                    
                    RectangleF borderRectF = borderRects[borderIdx];
                    CMvdRectangleF mv0 = GaImageUtil.ToCMvdRectangleF(ref borderRectF);
                    CMvdRectangleF mvdRet = cell.PositionFixRun(
                                                    mv0,
                                                    xRecipe.xRegionTrain,
                                                    Rectangle.Round(cellRoi),
                                                    chipLocationResult ) as CMvdRectangleF;

                    cell.LineSegmentRun(borderIdx, cellBmp, mvdRet);

                    // Offset
                    mvdRet.CenterX += cellRoi.X;
                    mvdRet.CenterY += cellRoi.Y;

                    // 更新到 cell
                    cell.cMvdShapesForFindLineRegion[borderIdx] = (CMvdShape)mvdRet.Clone();
                }
            }
            catch (Exception ex)
            {
                LtDebug.LOG.Error(ex, $"{borderName} 定位異常");
                throw ex;
            }
#endif
            #endregion

            #region 長度量測
            try
            {
#if OPT_OLD
                if (cell.cMvdLineSegmentFsOut[0] != null && cell.cMvdLineSegmentFsOut[2] != null)
                {
                    // 使用 MVD VisionDesigner Tool
                    using (var cL2LMeasureToolObj = new VisionDesigner.L2LMeasure.CL2LMeasureTool())
                    {
                        // Set basic parameter
                        cL2LMeasureToolObj.BasicParam.Line1 = cell.cMvdLineSegmentFsOut[0];
                        cL2LMeasureToolObj.BasicParam.Line2 = cell.cMvdLineSegmentFsOut[2];
                        //cL2LMeasureToolObj.BasicParam.Line1.StartPoint = new MVD_POINT_F(100f, 100f);
                        //cL2LMeasureToolObj.BasicParam.Line1.EndPoint = new MVD_POINT_F(150f, 150f);
                        //cL2LMeasureToolObj.BasicParam.Line2.StartPoint = new MVD_POINT_F(300f, 300f);
                        //cL2LMeasureToolObj.BasicParam.Line2.EndPoint = new MVD_POINT_F(250f, 350f);

                        // Running
                        cL2LMeasureToolObj.Run();

                        // Get the result
                        var cL2LMeasureRes = cL2LMeasureToolObj.Result;

                        // Update result to cell (pixels to physical)
                        // 目前只是簡單假設: 線掃 與 載盤 在同一平面
                        // ToDO: 必須處理透視投影引進的誤差 !!!
                        cell.RunWidth = cL2LMeasureRes.VerticalAbsDist * INI.Instance.ImageResolution;

                        //Console.WriteLine("Angle: {0}", cL2LMeasureRes.Angle);
                        //Console.WriteLine("Vertical distance: {0}", cL2LMeasureRes.VerticalAbsDist);
                        LtDebug.LOG.Info("長度量測: Angle = {0:0.00}", cL2LMeasureRes.Angle);
                        LtDebug.LOG.Info("長度量測: Vertical distance = {0:0.000}", cL2LMeasureRes.VerticalAbsDist);
                    }
                }
#else
                if (cell.cMvdLineSegmentFsOut[0] != null && cell.cMvdLineSegmentFsOut[2] != null)
                {
                    // 使用 MVD VisionDesigner Tool
                    using (var cL2LMeasureToolObj = new VisionDesigner.L2LMeasure.CL2LMeasureTool())
                    {
                        // Set basic parameter
                        //cL2LMeasureToolObj.BasicParam.Line1 = getRealLine(cell.cMvdLineSegmentFsOut[0]);
                        //cL2LMeasureToolObj.BasicParam.Line2 = getRealLine(cell.cMvdLineSegmentFsOut[2]);

                        cL2LMeasureToolObj.BasicParam.Line1 = cell.cMvdLineSegmentFsOut[0];
                        cL2LMeasureToolObj.BasicParam.Line2 = cell.cMvdLineSegmentFsOut[2];

                        // Running
                        cL2LMeasureToolObj.Run();

                        // Get the result
                        var cL2LMeasureRes = cL2LMeasureToolObj.Result;

                        // Update result to cell (pixels to physical)
                        // 目前只是簡單假設: 線掃 與 載盤 在同一平面
                        // ToDO: 必須處理透視投影引進的誤差 !!!
                        cell.RunWidth = (float)Math.Round(cL2LMeasureRes.VerticalAbsDist * INI.Instance.ImageResolutionX, 3);

                        //Console.WriteLine("Angle: {0}", cL2LMeasureRes.Angle);
                        //Console.WriteLine("Vertical distance: {0}", cL2LMeasureRes.VerticalAbsDist);
                        LtDebug.LOG.Info("長度量測: Angle = {0:0.00}", cL2LMeasureRes.Angle);
                        LtDebug.LOG.Info("長度量測: Vertical distance = {0:0.000}", cL2LMeasureRes.VerticalAbsDist);
                    }
                }
#endif
            }
            catch (MvdException ex)
            {
                //Console.WriteLine("Fail with ErrorCode: 0x" + ex.ErrorCode.ToString("X"));
                LtDebug.LOG.Error(ex, "長度量測 異常: ErrorCode = 0x{0:X}", ex.ErrorCode);
            }
            catch (System.Exception ex)
            {
                //Console.WriteLine("Fail with error " + ex.Message);
                LtDebug.LOG.Error(ex, "長度量測 異常");
            }
            #endregion

            #region 寬度量測
            try
            {
#if OPT_OLD
                if (cell.cMvdLineSegmentFsOut[1] != null && cell.cMvdLineSegmentFsOut[3] != null)
                {
                    // 使用 MVD VisionDesigner Tool
                    using (var cL2LMeasureToolObj = new VisionDesigner.L2LMeasure.CL2LMeasureTool())
                    {
                        // Set basic parameter
                        cL2LMeasureToolObj.BasicParam.Line1 = cell.cMvdLineSegmentFsOut[1];
                        cL2LMeasureToolObj.BasicParam.Line2 = cell.cMvdLineSegmentFsOut[3];
                        //cL2LMeasureToolObj.BasicParam.Line1.StartPoint = new MVD_POINT_F(100f, 100f);
                        //cL2LMeasureToolObj.BasicParam.Line1.EndPoint = new MVD_POINT_F(150f, 150f);
                        //cL2LMeasureToolObj.BasicParam.Line2.StartPoint = new MVD_POINT_F(300f, 300f);
                        //cL2LMeasureToolObj.BasicParam.Line2.EndPoint = new MVD_POINT_F(250f, 350f);

                        // Running
                        cL2LMeasureToolObj.Run();

                        // Get the result
                        var cL2LMeasureRes = cL2LMeasureToolObj.Result;

                        // Update result to Cell (pixels to physic)
                        // 目前只是簡單假設: 線掃 與 載盤 在同一平面
                        // ToDO: 必須處理透視投影引進的誤差 !!!
                        cell.RunHeight = cL2LMeasureRes.VerticalAbsDist * INI.Instance.ImageResolution;

                        //Console.WriteLine("Angle: {0}", cL2LMeasureRes.Angle);
                        //Console.WriteLine("Vertical distance: {0}", cL2LMeasureRes.VerticalAbsDist);
                        LtDebug.LOG.Info("寬度量測: Angle = {0:0.00}", cL2LMeasureRes.Angle);
                        LtDebug.LOG.Info("寬度量測: Vertical distance = {0:0.000}", cL2LMeasureRes.VerticalAbsDist);
                    }
                }
#else
                if (cell.cMvdLineSegmentFsOut[1] != null && cell.cMvdLineSegmentFsOut[3] != null)
                {
                    // 使用 MVD VisionDesigner Tool
                    using (var cL2LMeasureToolObj = new VisionDesigner.L2LMeasure.CL2LMeasureTool())
                    {
                        // Set basic parameter
                        //cL2LMeasureToolObj.BasicParam.Line1 = getRealLine(cell.cMvdLineSegmentFsOut[1]);
                        //cL2LMeasureToolObj.BasicParam.Line2 = getRealLine(cell.cMvdLineSegmentFsOut[3]);

                        cL2LMeasureToolObj.BasicParam.Line1 = cell.cMvdLineSegmentFsOut[1];
                        cL2LMeasureToolObj.BasicParam.Line2 = cell.cMvdLineSegmentFsOut[3];

                        // Running
                        cL2LMeasureToolObj.Run();

                        // Get the result
                        var cL2LMeasureRes = cL2LMeasureToolObj.Result;

                        // Update result to Cell (pixels to physic)
                        // 目前只是簡單假設: 線掃 與 載盤 在同一平面
                        // ToDO: 必須處理透視投影引進的誤差 !!!
                        cell.RunHeight = (float)Math.Round(cL2LMeasureRes.VerticalAbsDist * INI.Instance.ImageResolutionY, 3);

                        //Console.WriteLine("Angle: {0}", cL2LMeasureRes.Angle);
                        //Console.WriteLine("Vertical distance: {0}", cL2LMeasureRes.VerticalAbsDist);
                        LtDebug.LOG.Info("寬度量測: Angle = {0:0.00}", cL2LMeasureRes.Angle);
                        LtDebug.LOG.Info("寬度量測: Vertical distance = {0:0.000}", cL2LMeasureRes.VerticalAbsDist);
                    }
                }
#endif
            }
            catch (MvdException ex)
            {
                //Console.WriteLine("Fail with ErrorCode: 0x" + ex.ErrorCode.ToString("X"));
                _TM.LOG.Error(ex, "寬度量測 異常: ErrorCode = 0x{0:X}", ex.ErrorCode);
            }
            catch (System.Exception ex)
            {
                //Console.WriteLine("Fail with error " + ex.Message);
                _TM.LOG.Error(ex, "寬度量測 異常");
            }
            #endregion
        }
        /// <summary>
        /// LETIAN: 读码测试 搬移至此.
        /// caller 負責 bmpInputImage 生命
        /// </summary>
        private void _Inspect001_QRCode(Bitmap bmpInputImage)
        {
            if (m_QrUsed || xInspect.bCheckInspect)
            {
                foreach (RegionCellX3Class cell in xRecipe.xRegionCells)
                {
                    if (cell.ByPass && !INI.Instance.IsForceInspect)
                        continue;
                    if (cell.inspectReason == InspectReason.INS_ALIGNERR)
                        continue;

                    //原始模板的大小
                    RectangleF templaterectf = new RectangleF(0, 0, xRecipe.bmpprinttemplate.Width, xRecipe.bmpprinttemplate.Height);

                    //定位完成后裁切位置
                    RectangleF _crop = new RectangleF(cell.DrawResultRectF().CenterX - templaterectf.Width / 2,
                        cell.DrawResultRectF().CenterY - templaterectf.Height / 2,
                        templaterectf.Width,
                        templaterectf.Height);

                    if (xInspect.bCheckInspect)
                    {
                        RectangleF _cropDefect = new RectangleF(xRecipe.xRegionTrain.X + _crop.X,
                            xRecipe.xRegionTrain.Y + _crop.Y,
                            xRecipe.xRegionTrain.Width,
                            xRecipe.xRegionTrain.Height);

                        cell.bmpItemRun?.Dispose();
                        cell.bmpItemRun = bmpInputImage.Clone(_cropDefect, PixelFormat.Format8bppIndexed);

                        cell.bmpItemMask?.Dispose();
                        cell.bmpItemMask = xRecipe.bmpprintmask.Clone(
                            new Rectangle(0, 0, xRecipe.bmpprintmask.Width, xRecipe.bmpprintmask.Height),
                            PixelFormat.Format8bppIndexed);
                        cell.DetectDefects(xRecipe.bmpDefectTemplate, cell.bmpItemRun, cell.bmpItemMask);
                    }

                    if (m_QrUsed)
                    {
                        RectangleF _cropCode = new RectangleF(xRecipe.xRectCodeRegion.X + _crop.X,
                            xRecipe.xRectCodeRegion.Y + _crop.Y,
                            xRecipe.xRectCodeRegion.Width,
                            xRecipe.xRectCodeRegion.Height);

                        cell.bmpItemCodeRun?.Dispose();
                        cell.bmpItemCodeRun = bmpInputImage.Clone(_cropCode, PixelFormat.Format8bppIndexed);
                        cell.DeCode2D(cell.bmpItemCodeRun, _cropCode.Location, m_QrJudged);
                    }
                }
            }
        }
        /// <summary>
        /// LETIAN: 非同步保存 Debug 數據 搬移至此.
        /// caller 負責 bmpInputImage 生命
        /// </summary>
        private void _Inspect001_Async_SaveDebugData(Bitmap bmpInputImage, string debugCellCenterStr, string imgPath)
        {
            IEzImage ezImageArg = new EzFreeBitmap(bmpInputImage, true);

            ThreadPool.QueueUserWorkItem(arg =>
            {
                try
                {
                    IEzImage ezImage = arg as IEzImage;
                    m_FileName = $"{LotId}-{DateTime.Now.ToString("yyyyMMddHHmmss")}.jpg";
                    if (INI.Instance.IsSaveTestImage)
                    {
                        GaUtil.SaveData(debugCellCenterStr,
                            imgPath + $"\\PositionFix\\DEBUG_{DateTime.Now.ToString("yyyyMMddHHmmss")}.txt");
                    }

                    if (INI.Instance.IsSaveDebugBMP)
                    {
                        GaImageUtil.SaveImageWithQuality(ezImage.Bitmap,
                            $"{m_PicResultPath}\\{m_FileName}",
                            INI.Instance.ImageQuality);
                    }

                    if (INI.Instance.IsSaveDebugOrgBmp)
                    {
                        ezImage.Save($"{m_PicResultOrgPath}\\{m_FileName}");
                        //bmpInputImage.Save($"{m_PicResultOrgPath}\\{LotId}-{DateTime.Now.ToString("yyyyMMddHHmmss")}.jpg",
                        //                   ImageFormat.Jpeg);
                    }

                    ezImage?.Dispose();
                }
                catch (Exception e)
                {
                    _LOG($"异常捕获:{e.Message}", Color.Red);
                }
            },
                ezImageArg
            );
        }
        #endregion

        #region INSPECT_003
        /// <summary>
        /// 空载台检测
        /// </summary>
        private void _Inspect003_LT()
        {
            fire_AoiBegin();

            xRecipe.AnalyzeDatasData();

            m_ElapsedTime = 0;
            m_Running = true;
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Restart();

            var aoiModel = GaMvcConfig.SysModel.EmptyTrayAoiModel;

            Bitmap bmpInputImage = PeekLineScanBitmap();
            if (bmpInputImage != null)
            {
                xRecipe.xOutBlocs.Clear();
                //复位所有数据
                foreach (var cell in xRecipe.xRegionCells)
                {
                    cell.Reset();
                }

                aoiModel.RunAll(bmpInputImage, wait: true);

                var result = aoiModel.GetResult();

                #region WRITE_TO_LOG
                string msg;
                if (result == null)
                {
                    msg = "無結果!";
                    //m_IsPass = false;
                }
                else
                {
                    //m_IsPass = result.IsPass();
                    msg = result.ToString();
                }
                CommonLogClass.Instance.LogMessage(msg, m_IsPass ? Color.Green : Color.Red);
                #endregion

                _Inspect003_UpdateResult(result, bmpInputImage, xRecipe);

#if (OPT_OLD)
                IEzImage ezImage = new EzFreeBitmap(bmpInputImage, true);
                Task task = new Task(() =>
                {
                    try
                    {
                        if (INI.Instance.IsSaveDebugBMP)
                        {
                            SaveImageWithQuality(ezImage.Bitmap, $"{m_PicResultPath}\\{LotId}-{DateTime.Now.ToString("yyyyMMddHHmmss")}.jpg", INI.Instance.ImageQuality);
                        }
                        if (INI.Instance.IsSaveDebugOrgBmp)
                        {
                            ezImage.Save($"{m_PicResultOrgPath}\\{LotId}-{DateTime.Now.ToString("yyyyMMddHHmmss")}.jpg");
                            //bmpInputImage.Save($"{m_PicResultOrgPath}\\{LotId}-{DateTime.Now.ToString("yyyyMMddHHmmss")}.jpg",
                            //                   ImageFormat.Jpeg);
                        }
                        ezImage?.Dispose();
                    }
                    catch (Exception e)
                    {
                        _LOG($"异常捕获:{e.Message}", Color.Red);
                    }
                });
                task.Start();
#endif

                // 異步輸出 Debug 數據
                _Inspect003_Async_SaveDebugData(bmpInputImage);
            }

            m_IsPass = true;//不需要结果 都是记录单颗的数据
            stopwatch.Stop();
            m_ElapsedTime = stopwatch.ElapsedMilliseconds;  //@ for Inspect003
            m_Running = false;

            fire_AoiEnd();
        }

        /// <summary>
        /// 空载台检测 : 更新結果 到 Gaara 數據群
        /// </summary>
        private void _Inspect003_UpdateResult(EzEmptyTrayResult result, Bitmap bmpInputImage, RecipeFPIX3Class dst)
        {
            string imgPath = $"{Universal.LOG_IMG_PATH}\\{JzTimes.DateSerialString}\\{m_FileBarcodeStr}";

            #region PREPARE_PATH
            if (INI.Instance.IsSaveTestImage)
            {
                if (!Directory.Exists(imgPath))
                    Directory.CreateDirectory(imgPath);
            }
            #endregion

            if (result != null)
            {
                int fullRows = result.FullRows;
                int fullCols = result.FullCols;

                //Z字型对位资料
                int _index = 0;
                for (int row = 0; row < fullRows; row++)
                {
                    if (row % 2 == 1)
                    {
                        for (int col = fullCols - 1; col > -1; col--)
                        {
                            if (_index >= xRecipe.xRegionCells.Count)
                                break;

                            result.GetBlocByRowCol(row, col, out EzBloc bloc, out bool isOK);
                            var cell = xRecipe.xRegionCells[_index];
                            InspectReason reason = (isOK ? InspectReason.INS_ALIGNERR : InspectReason.INS_DEFECTERR);
                            //if (bloc == null)
                            //    reason = InspectReason.INS_DEFECTERR;
                            cell.inspectReason = reason;
                            cell.inspectReasons.Add(reason);
                            System.Diagnostics.Trace.WriteLine($"[{row}, {col}] is " + (isOK ? "OK" : "NG"));
                            _index++;
                        }
                    }
                    else
                    {
                        for (int col = 0; col < fullCols; col++)
                        {
                            if (_index >= xRecipe.xRegionCells.Count)
                                break;

                            result.GetBlocByRowCol(row, col, out EzBloc bloc, out bool isOK);
                            var cell = xRecipe.xRegionCells[_index];
                            InspectReason reason = (isOK ? InspectReason.INS_ALIGNERR : InspectReason.INS_DEFECTERR);
                            //if (bloc == null)
                            //    reason = InspectReason.INS_DEFECTERR;
                            cell.inspectReason = reason;
                            cell.inspectReasons.Add(reason);
                            System.Diagnostics.Trace.WriteLine($"[{row}, {col}] is " + (isOK ? "OK" : "NG"));
                            _index++;
                        }
                    }
                }

                #region 收集阵列之外的料件

                foreach (var bloc in result.IterOutGridAbnormalBlocs())
                {
                    xRecipe.xOutBlocs.Add(bloc.Rect);
                }

                #endregion

            }

#if (false)
                for (int row = 0; row < fullRows; row++)
                {
                    for (int col = 0; col < fullCols; col++)
                    {
                        result.GetBlocByRowCol(row, col, out EzBloc bloc, out bool isOK);
                        var cell = xRecipe.xRegionCells[row * fullCols + col];
                        InspectReason reason = (isOK ? InspectReason.INS_ALIGNERR : InspectReason.INS_DEFECTERR);
                        cell.inspectReason = reason;
                        cell.inspectReasons.Add(reason);

                        //if (bloc != null)
                        //{
                        //    // 單位: Pixels
                        //    var rect = bloc.Rect;
                        //    if (isOK)
                        //    {
                        //        // rest 內有 吸嘴
                        //        // 如何將 rect 轉換到 cell ???
                        //    }
                        //    else
                        //    {
                        //        // rect 內有 雜物
                        //        // 如何將 rect 轉換到 cell ???
                        //    }
                        //}
                        //else
                        //{
                        //    // [row, col] 處 沒有找到 定位格點
                        //}

                System.Diagnostics.Trace.WriteLine($"[{row}, {col}] is " + (isOK ? "OK" : "NG"));
                    }
                }
#endif

#if (false)
            Size _bmpInputSize = new Size((int)cMvdInput.Width, (int)cMvdInput.Height);

            //if (cMvdInput.PixelFormat != MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08)
            //{
            //    //当前程序仅支持mono8。因此像素格会转换.
            //    cMvdInput.ConvertImagePixelFormat(MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08);
            //}

            //Bitmap bmp0 = bmpInputImage.Clone(xRecipe.xRectRegionBase0, PixelFormat.Format8bppIndexed);
            //Bitmap bmp1 = bmpInputImage.Clone(xRecipe.xRectRegionBase1, PixelFormat.Format8bppIndexed);

            ////计算基准位置 用来检测偏移
            //mVD_POINT_F0 = _getBasePointF2(bmp0, RegionName.BASE0);
            //mVD_POINT_F1 = _getBasePointF2(bmp1, RegionName.BASE1);

            //bmp0.Dispose();
            //bmp1.Dispose();

            //xRecipe.mvdprinttemp_Find.xMvdRun_Image = m_MvdOpeate.Clone();
            //xRecipe.mvdprinttemp_Find.HikRun4Pre();

            foreach (RegionCellX3Class cell in xRecipe.xRegionCells)
            {
                cell.Reset();

                if (INI.Instance.IsSaveTestImage)
                {
                    cell.IsSaveDebugPicture = true;
                    cell.SaveDebugPath = imgPath;
                }

                //if (cell.ByPass && !INI.Instance.IsForceInspect)
                //{
                //    //cell.inspectReason = InspectReason.INS_NOOPEN;
                //    cell.inspectReasons.Add(InspectReason.INS_NOOPEN);
                //    continue;
                //}
                RectangleF _rectF = new RectangleF(cell.viewRectF.X, cell.viewRectF.Y, cell.viewRectF.Width, cell.viewRectF.Height);
                _rectF.Inflate(xRecipe.xExtendx, xRecipe.xExtendy);
                BoundRect(ref _rectF, _bmpInputSize);
                //xRecipe.mvdprinttemp_Find.bmpRun_Image = bmpInputImage.Clone(_rectF, PixelFormat.Format8bppIndexed);
                Bitmap bmp2 = bmpInputImage.Clone(_rectF, PixelFormat.Format8bppIndexed);
                int iOK = xRecipe.PrintTempRun(bmp2);

                //int iOK = xRecipe.PrintTempRun(cMvdInput, _rectF);
                //xRecipe.mvdprinttemp_Find.xMvdRun_Image = cMvdInput;
                //int iOK = (xRecipe.mvdprinttemp_Find.HikRun3(_rectF) ? 0 : -1);
                //int iOK = (xRecipe.mvdprinttemp_Find.HikRun4(_rectF) ? 0 : -1);

                if (iOK == 0)
                {
                    cell.xFindResult = xRecipe.mvdprinttemp_Find.xResults[0];
                    cell.xFindResult.fCenterX += _rectF.X;
                    cell.xFindResult.fCenterY += _rectF.Y;
                    RectangleF templaterectf = new RectangleF(0, 0, xRecipe.bmpprinttemplate.Width, xRecipe.bmpprinttemplate.Height);
                    Rectangle runrectf = new Rectangle(0, 0, _bmpInputSize.Width, _bmpInputSize.Height);
                    cell.PositionFixRun(templaterectf, runrectf, cell.xFindResult);

                    //判断偏移
                    //cell.RunX = (cell.DrawResultRectF().CenterX - mVD_POINT_F0.fX - cell.OrgX) * INI.Instance.ImageResolution;
                    //cell.RunY = (cell.DrawResultRectF().CenterY - mVD_POINT_F0.fY - cell.OrgY) * INI.Instance.ImageResolution;

                    //换算为偏移的位置
                    //cell.RunX = (cell.DrawResultRectF().CenterX - cell.OrgX) * INI.Instance.ImageResolution;
                    //cell.RunY = (cell.DrawResultRectF().CenterY - cell.OrgY) * INI.Instance.ImageResolution;
                    //cell.RunAngle = (cell.DrawResultRectF().Angle - cell.OrgAngle);

                    //计算偏移值
                    PointF _viewNewRun = new PointF(cell.DrawResultRectF().CenterX, cell.DrawResultRectF().CenterY);
                    PointF _worldNewRun = LineScanCalibrate.ViewToWorld(_viewNewRun);
                    cell.RunX = (_worldNewRun.X - cell.OrgX);
                    cell.RunY = (_worldNewRun.Y - cell.OrgY);
                    cell.RunAngle = cell.DrawResultRectF().Angle;
                    cell.GetOffsetResult();
                }
                else
                {
                    cell.inspectReason = InspectReason.INS_ALIGNERR;
                    cell.inspectReasons.Add(InspectReason.INS_ALIGNERR);
                }

                bmp2.Dispose();
            }

            foreach (RegionCellX3Class cell in xRecipe.xRegionCells)
            {
                //if (cell.ByPass && !INI.Instance.IsForceInspect)
                //    continue;
                if (cell.inspectReason != InspectReason.INS_ALIGNERR)
                    continue;

                //cell.xInspectPara = InspectX2Class.Instance;
                //原始切图
                RectangleF _rectF = new RectangleF(cell.viewRectF.X, cell.viewRectF.Y, cell.viewRectF.Width, cell.viewRectF.Height);
                _rectF.Inflate(xRecipe.xExtendx, xRecipe.xExtendy);
                BoundRect(ref _rectF, _bmpInputSize);

                //xRecipe.mvdprinttemp_Find.bmpRun_Image = bmpInputImage.Clone(_rectF, PixelFormat.Format8bppIndexed);
                Bitmap bmp2 = bmpInputImage.Clone(_rectF, PixelFormat.Format8bppIndexed);
                cell.CheckBlobNoTray(bmp2);

                ////原始模板的大小
                //RectangleF templaterectf = new RectangleF(0, 0, xRecipe.bmpprinttemplate.Width, xRecipe.bmpprinttemplate.Height);

                ////定位完成后裁切位置
                //RectangleF _crop = new RectangleF(cell.DrawResultRectF().CenterX - templaterectf.Width / 2,
                //    cell.DrawResultRectF().CenterY - templaterectf.Height / 2,
                //    templaterectf.Width,
                //    templaterectf.Height);

                //if (InspectX2Class.Instance.bCheckInspect)
                //{
                //    cell.bmpItemRun.Dispose();
                //    cell.bmpItemRun = bmpInputImage.Clone(_crop, PixelFormat.Format8bppIndexed);
                //    cell.bmpItemMask.Dispose();
                //    cell.bmpItemMask = xRecipe.bmpprintmask.Clone(
                //        new Rectangle(0, 0, xRecipe.bmpprintmask.Width, xRecipe.bmpprintmask.Height),
                //        PixelFormat.Format8bppIndexed);
                //    cell.DetectDefects(xRecipe.bmpprinttemplate, cell.bmpItemRun, cell.bmpItemMask);
                //}
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
#endif
        }
        /// <summary>
        /// LETIAN: 非同步保存 Debug 數據 搬移至此.
        /// 此函式 負責 bmpInputImage 生命
        /// </summary>
        private void _Inspect003_Async_SaveDebugData(Bitmap bmpInputImage)
        {
            IEzImage ezImageArg = new EzFreeBitmap(bmpInputImage, true);

            ThreadPool.QueueUserWorkItem(arg =>
            {
                try
                {
                    IEzImage ezImage = arg as IEzImage;

                    if (INI.Instance.IsSaveDebugBMP)
                    {
                        GaImageUtil.SaveImageWithQuality(ezImage.Bitmap,
                            $"{m_PicResultPath}\\{LotId}-{DateTime.Now.ToString("yyyyMMddHHmmss")}.jpg", INI.Instance.ImageQuality);
                    }

                    if (INI.Instance.IsSaveDebugOrgBmp)
                    {
                        ezImage.Save($"{m_PicResultOrgPath}\\{LotId}-{DateTime.Now.ToString("yyyyMMddHHmmss")}.jpg");
                        //bmpInputImage.Save($"{m_PicResultOrgPath}\\{LotId}-{DateTime.Now.ToString("yyyyMMddHHmmss")}.jpg",
                        //                   ImageFormat.Jpeg);
                    }

                    ezImage?.Dispose();
                }
                catch (Exception e)
                {
                    _LOG($"异常捕获:{e.Message}", Color.Red);
                }
            },
                ezImageArg
            );
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
        protected void _LOG(string msg, params object[] args)
        {
#if (true)
            Color color = Color.Black;

            int N = args.Length;
            if (N > 0 && args[N - 1] is Color)
            {
                color = (Color)args[N - 1];
                N -= 1;
            }

            var sb = new System.Text.StringBuilder();
            //sb.Append(Name);
            sb.Append(", ");
            sb.Append(msg);

            for (int i = 0; i < N; i++)
            {
                sb.Append(", ");
                sb.Append(args[i]);
            }

            msg = sb.ToString();
            CommonLogClass.Instance.LogMessage(msg, color);
            //if (color == Color.Red)
            //    GdxGlobal.LOG.Warn(msg);
            //else
            //    GdxGlobal.LOG.Debug(msg);
#endif
        }
        #endregion

        #region RUNTIME_TOOLS
        List<CBoxOverlapTool> _boxOverlapTools = new List<CBoxOverlapTool>();
        /// <summary>
        /// 转换虚拟的直线
        /// </summary>
        /// <param name="viewLine">输入虚拟直线</param>
        /// <returns>返回实体直线</returns>
        CMvdLineSegmentF getRealLine(CMvdLineSegmentF viewLine)
        {
            PointF p1view = new PointF(viewLine.StartPoint.fX, viewLine.StartPoint.fY);
            PointF p2view = new PointF(viewLine.EndPoint.fX, viewLine.EndPoint.fY);

            PointF p1world = LineScanCalibrate1.ViewToWorld(p1view);
            PointF p2world = LineScanCalibrate1.ViewToWorld(p2view);

            return new CMvdLineSegmentF(new MVD_POINT_F(p1world.X, p1world.Y), new MVD_POINT_F(p2world.X, p2world.Y));
        }
        #endregion

        void _PrepareChipMatcher(int threadIdx, out IMvdTemplateMatcher chipMatcher)
        {
            MvdCompositeChipMatcher matchers = xRecipe.mvdprinttemp_Find;
            chipMatcher = matchers[threadIdx];
        }
        void _PrepareBoxOverlapTool(int threadIdx, out CBoxOverlapTool cBoxOverlapTool)
        {
            cBoxOverlapTool = _boxOverlapTools[threadIdx];
        }
        void _InstanceBoxOverlapTools(int N)
        {
            while (N >= _boxOverlapTools.Count)
                _boxOverlapTools.Add(new CBoxOverlapTool());
        }
        void _DisposeTools()
        {
            if (_boxOverlapTools == null)
                return;
            var old = _boxOverlapTools;
            _boxOverlapTools = null;
            foreach (var x in old)
                x?.Dispose();
        }
    }
}
