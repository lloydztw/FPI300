using EzAoiEmptyTrayInspector.Model;
using JetEazy.BasicSpace;
using JetEazy.EzImage;
using JetEazy.Match;
using JetEazy.Utils;
using LaserAlignDX.OPSpace;
using LaserAlignDX.OPSpace.RecipeSpace;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using Traveller106;
using VisionDesigner;
using VisionDesigner.BoxOverlap;
using _TM = Traveller106.LtDebug;


namespace LaserAlignDX.RunSpace.Old
{
    //public enum ScanInspectMode : int
    //{
    //    [Description("外观及尺寸检测")]
    //    MEASUREAOI = 0,
    //    [Description("QR检测")]
    //    QRCODE = 1,
    //    [Description("空载台检测")]
    //    NOTRAY = 2,
    //}

    public class ProcessRunFPIClass : IProcessRunFPI
    {
        public event EventHandler<GaProgressEventArgs> OnAoiBegin;
        public event EventHandler<GaProgressEventArgs> OnAoiEnd;
        public event EventHandler<GaProgressEventArgs> OnAoiProgressing;

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
        }

        #region PRIVATE_DATA
        RecipeFPIX3Class xRecipe
        {
            get { return RecipeFPIX3Class.Instance; }
        }
        protected InspectX3ParaClass xInspect
        {
            get { return InspectX3ParaClass.Instance; }
        }
        LineScanCalibrateClass LineScanCalibrate
        {
            get { return Traveller106.Universal.LineScanCalibrateClasses[0]; }
        }
        //protected InspectX2Class xInspect
        //{
        //    get { return InspectX2Class.Instance; }
        //}
        #endregion

        public CMvdImage cMvdInput { get; set; } = new CMvdImage();
        //public MVD_POINT_F mVD_POINT_F0 = new MVD_POINT_F();
        //public MVD_POINT_F mVD_POINT_F1 = new MVD_POINT_F();

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

        VisionDesigner.BoxOverlap.CBoxOverlapTool cBoxOverlapTool = null;
        private string m_StripId = "Strip_NONE";
        private string m_LotId = "Lot_NONE";
        private string m_PicResultPath = INI.Instance.ResultImagePath;
        private string m_PicResultOrgPath = INI.Instance.ResultImagePath;
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
            _TM.Trace($"{GetType().Name} Version0 Run");

            System.Threading.Thread thread = new System.Threading.Thread(runTest);
            thread.IsBackground = true;
            thread.Start();
        }

        //public void RunRecipe()
        //{
        //    m_IsPass = true;
        //    m_ResultDesc = string.Empty;

        //    //if (!myRecipe.Ischip_open_measure)
        //    //    return;

        //    _InspectRecipe();

        //    //System.Threading.Thread thread = new System.Threading.Thread(runTest);
        //    //thread.IsBackground = true;
        //    //thread.Start();
        //}

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

        /// <summary>
        /// 外观及尺寸检测
        /// </summary>
        private void _Inspect001()
        {
            xRecipe.AnalyzeDatasData();
            m_ElapsedTime = 0;
            m_Running = true;
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Restart();

            if (cBoxOverlapTool == null)
                cBoxOverlapTool = new CBoxOverlapTool();

            string imgPath = $"{Universal.LOG_IMG_PATH}\\{JzTimes.DateSerialString}\\{m_FileBarcodeStr}";
            if (INI.Instance.IsSaveTestImage)
            {
                if (!Directory.Exists(imgPath))
                    Directory.CreateDirectory(imgPath);
            }

            Size _bmpInputSize = new Size((int)cMvdInput.Width, (int)cMvdInput.Height);
            Bitmap bmpInputImage = EzMvdImageConvertor.CMvdImageToBitmap(cMvdInput);
            string debugCellCenterStr = string.Empty;

            foreach (RegionCellX3Class cell in xRecipe.xRegionCells)
            {
                cell.Reset();

                if (INI.Instance.IsSaveTestImage)
                {
                    cell.IsSaveDebugPicture = true;
                    cell.SaveDebugPath = imgPath;
                }
                RectangleF _rectF = new RectangleF(cell.viewRectF.X, cell.viewRectF.Y, cell.viewRectF.Width, cell.viewRectF.Height);
                _rectF.Inflate(xRecipe.xExtendx, xRecipe.xExtendy);
                GaUtil.BoundRect(ref _rectF, _bmpInputSize);

                //xRecipe.mvdprinttemp_Find.bmpRun_Image = bmpInputImage.Clone(_rectF, PixelFormat.Format8bppIndexed);
                Bitmap bmp2 = bmpInputImage.Clone(_rectF, PixelFormat.Format8bppIndexed);
                int iOK = xRecipe.PrintTempRun(bmp2);

                if (cell.IsSaveDebugPicture)
                {
                    string posfixpath = cell.SaveDebugPath + "\\PositionFix";
                    if (!Directory.Exists(posfixpath))
                        Directory.CreateDirectory(posfixpath);

                    bmp2.Save(posfixpath + $"\\Fix_{cell.Index}_{cell.lblName}.bmp", ImageFormat.Bmp);
                }

                if (iOK == 0)
                {
                    cell.xFindResult = xRecipe.mvdprinttemp_Find.xResults[0];

                    debugCellCenterStr += $"INDEX:{cell.Index}#";
                    debugCellCenterStr += $"VIEW:{_rectF.X};{_rectF.Y}#";
                    debugCellCenterStr += $"ORG:{cell.xFindResult.fCenterX};{cell.xFindResult.fCenterY}#";

                    cell.xFindResult.fCenterX += _rectF.X;
                    cell.xFindResult.fCenterY += _rectF.Y;

                    debugCellCenterStr += $"DES:{cell.xFindResult.fCenterX};{cell.xFindResult.fCenterY}{Environment.NewLine}";

                    RectangleF templaterectf //= new RectangleF(0, 0, xRecipe.bmpprinttemplate.Width, xRecipe.bmpprinttemplate.Height);
                            = new RectangleF(0, 0, xRecipe.PrintTemplateSize.Width, xRecipe.PrintTemplateSize.Height);
                    Rectangle runrectf = new Rectangle(0, 0, _bmpInputSize.Width, _bmpInputSize.Height);
                    cell.PositionFixRun(templaterectf, runrectf, cell.xFindResult);

                    //增加重叠区域的判断
                    cBoxOverlapTool.ROI1 = new CMvdRectangleF(
                        cell.viewRectF.X + cell.viewRectF.Width / 2,
                        cell.viewRectF.Y + cell.viewRectF.Height / 2,
                        cell.viewRectF.Width,
                        cell.viewRectF.Height);
                    cBoxOverlapTool.ROI2 = cell.DrawResultRectF();

                    cBoxOverlapTool.Run();
                    if (cBoxOverlapTool.Result.Overlap >= xInspect.xChipOverlap)
                    {
                        //计算偏移值
                        PointF _viewNewRun = new PointF(cell.DrawResultRectF().CenterX,
                             cell.DrawResultRectF().CenterY);
                        PointF _worldNewRun = LineScanCalibrate.ViewToWorld(_viewNewRun);

                        ////原来基础位置的world坐标
                        //PointF _viewOrg = new PointF(cell.viewRectF.X + cell.viewRectF.Width / 2,
                        //                             cell.viewRectF.Y + cell.viewRectF.Height / 2);
                        //PointF _worldOrg = LineScanCalibrate.ViewToWorld(_viewOrg);
                        //cell.OrgX = _worldOrg.X;
                        //cell.OrgY = _worldOrg.Y;

                        cell.RunX = (_worldNewRun.X - cell.OrgX);
                        cell.RunY = (_worldNewRun.Y - cell.OrgY);
                        cell.RunAngle = cell.DrawResultRectF().Angle;

                        cell.GetOffsetResult();

                        if (xInspect.bOpenLineMeasure)
                        {
                            #region 直线寻找

                            //左边
                            RectangleF r0 = new RectangleF(xRecipe.xLineLeft.X,
                                xRecipe.xLineLeft.Y,
                                xRecipe.xLineLeft.Width,
                                xRecipe.xLineLeft.Height);
                            CMvdRectangleF mv0 = new CMvdRectangleF(r0.X + r0.Width / 2, r0.Y + r0.Height / 2, r0.Width, r0.Height);
                            CMvdRectangleF mv0ret = cell.PositionFixRun(mv0, xRecipe.xRegionTrain, Rectangle.Round(_rectF),
                                xRecipe.mvdprinttemp_Find.xResults[0]) as CMvdRectangleF;
                            cell.LineSegmentRun(0, bmp2, mv0ret);
                            mv0ret.CenterX += _rectF.X;
                            mv0ret.CenterY += _rectF.Y;
                            cell.cMvdShapesForFindLineRegion[0] = (CMvdShape)mv0ret.Clone();

                            //上边
                            RectangleF r1 = new RectangleF(xRecipe.xLineTop.X,
                                xRecipe.xLineTop.Y,
                                xRecipe.xLineTop.Width,
                                xRecipe.xLineTop.Height);
                            CMvdRectangleF mv1 = new CMvdRectangleF(r1.X + r1.Width / 2, r1.Y + r1.Height / 2, r1.Width, r1.Height);
                            CMvdRectangleF mv1ret = cell.PositionFixRun(mv1, xRecipe.xRegionTrain, Rectangle.Round(_rectF),
                                xRecipe.mvdprinttemp_Find.xResults[0]) as CMvdRectangleF;
                            cell.LineSegmentRun(1, bmp2, mv1ret);
                            mv1ret.CenterX += _rectF.X;
                            mv1ret.CenterY += _rectF.Y;
                            cell.cMvdShapesForFindLineRegion[1] = (CMvdShape)mv1ret.Clone();

                            //右边
                            RectangleF r2 = new RectangleF(xRecipe.xLineRight.X,
                                xRecipe.xLineRight.Y,
                                xRecipe.xLineRight.Width,
                                xRecipe.xLineRight.Height);
                            CMvdRectangleF mv2 = new CMvdRectangleF(r2.X + r2.Width / 2, r2.Y + r2.Height / 2, r2.Width, r2.Height);
                            CMvdRectangleF mv2ret = cell.PositionFixRun(mv2, xRecipe.xRegionTrain, Rectangle.Round(_rectF),
                                xRecipe.mvdprinttemp_Find.xResults[0]) as CMvdRectangleF;
                            cell.LineSegmentRun(2, bmp2, mv2ret);
                            mv2ret.CenterX += _rectF.X;
                            mv2ret.CenterY += _rectF.Y;
                            cell.cMvdShapesForFindLineRegion[2] = (CMvdShape)mv2ret.Clone();

                            //下边
                            RectangleF r3 = new RectangleF(xRecipe.xLineBottom.X,
                                xRecipe.xLineBottom.Y,
                                xRecipe.xLineBottom.Width,
                                xRecipe.xLineBottom.Height);
                            CMvdRectangleF mv3 = new CMvdRectangleF(r3.X + r3.Width / 2, r3.Y + r3.Height / 2, r3.Width, r3.Height);
                            CMvdRectangleF mv3ret = cell.PositionFixRun(mv3, xRecipe.xRegionTrain, Rectangle.Round(_rectF),
                                xRecipe.mvdprinttemp_Find.xResults[0]) as CMvdRectangleF;
                            cell.LineSegmentRun(3, bmp2, mv3ret);
                            mv3ret.CenterX += _rectF.X;
                            mv3ret.CenterY += _rectF.Y;
                            cell.cMvdShapesForFindLineRegion[3] = (CMvdShape)mv3ret.Clone();

                            //长度

                            try

                            {
                                if (cell.cMvdLineSegmentFsOut[0] != null && cell.cMvdLineSegmentFsOut[2] != null)
                                {
                                    // CreateInstance

                                    VisionDesigner.L2LMeasure.CL2LMeasureTool cL2LMeasureToolObj = new VisionDesigner.L2LMeasure.CL2LMeasureTool();

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

                                    VisionDesigner.L2LMeasure.CL2LMeasureResult cL2LMeasureRes = cL2LMeasureToolObj.Result;

                                    cell.RunWidth = cL2LMeasureRes.VerticalAbsDist * INI.Instance.ImageResolution;

                                    Console.WriteLine("Angle: {0}", cL2LMeasureRes.Angle);

                                    Console.WriteLine("Vertical distance: {0}", cL2LMeasureRes.VerticalAbsDist);

                                    cL2LMeasureToolObj.Dispose();
                                    cL2LMeasureToolObj = null;
                                }


                            }

                            catch (MvdException ex)

                            {

                                Console.WriteLine("Fail with ErrorCode: 0x" + ex.ErrorCode.ToString("X"));

                            }

                            catch (System.Exception ex)

                            {

                                Console.WriteLine("Fail with error " + ex.Message);

                            }

                            //宽度

                            try

                            {
                                if (cell.cMvdLineSegmentFsOut[1] != null && cell.cMvdLineSegmentFsOut[3] != null)
                                {
                                    // CreateInstance

                                    VisionDesigner.L2LMeasure.CL2LMeasureTool cL2LMeasureToolObj = new VisionDesigner.L2LMeasure.CL2LMeasureTool();

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

                                    VisionDesigner.L2LMeasure.CL2LMeasureResult cL2LMeasureRes = cL2LMeasureToolObj.Result;

                                    cell.RunHeight = cL2LMeasureRes.VerticalAbsDist * INI.Instance.ImageResolution;

                                    Console.WriteLine("Angle: {0}", cL2LMeasureRes.Angle);

                                    Console.WriteLine("Vertical distance: {0}", cL2LMeasureRes.VerticalAbsDist);

                                    cL2LMeasureToolObj.Dispose();
                                    cL2LMeasureToolObj = null;
                                }


                            }

                            catch (MvdException ex)

                            {

                                Console.WriteLine("Fail with ErrorCode: 0x" + ex.ErrorCode.ToString("X"));

                            }

                            catch (System.Exception ex)

                            {

                                Console.WriteLine("Fail with error " + ex.Message);

                            }

                            #endregion
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

                bmp2.Dispose();
            }

            #region 读码测试

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
                        cell.bmpItemRun.Dispose();
                        cell.bmpItemRun = bmpInputImage.Clone(_cropDefect, PixelFormat.Format8bppIndexed);
                        cell.bmpItemMask.Dispose();
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
                        cell.bmpItemCodeRun.Dispose();
                        cell.bmpItemCodeRun = bmpInputImage.Clone(_cropCode, PixelFormat.Format8bppIndexed);
                        cell.DeCode2D(cell.bmpItemCodeRun, _cropCode.Location, m_QrJudged);
                    }
                }
            }

            #endregion

            IEzImage ezImage = new EzFreeBitmap(bmpInputImage, true);

            Task task = new Task(() =>
            {
                try
                {
                    if (INI.Instance.IsSaveTestImage)
                        GaUtil.SaveData(debugCellCenterStr, 
                            imgPath + $"\\PositionFix\\DEBUG_{DateTime.Now.ToString("yyyyMMddHHmmss")}.txt");

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
            });
            task.Start();

            bmpInputImage.Dispose();
            m_IsPass = true;

            stopwatch.Stop();
            m_ElapsedTime = stopwatch.ElapsedMilliseconds;
            m_Running = false;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

#if NO_USE_20250812
        /// <summary>
        /// 读码检测
        /// </summary>
        private void _Inspect002()
        {
            xRecipe.AnalyzeDatasData();
            m_ElapsedTime = 0;
            m_Running = true;
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Restart();

            string imgPath = $"{Universal.LOG_IMG_PATH}\\{JzTimes.DateSerialString}\\{m_FileBarcodeStr}";
            if (INI.Instance.IsSaveTestImage)
            {
                if (!Directory.Exists(imgPath))
                    Directory.CreateDirectory(imgPath);
            }

            Size _bmpInputSize = new Size((int)cMvdInput.Width, (int)cMvdInput.Height);
            Bitmap bmpInputImage = EzMvdImageConvertor.CMvdImageToBitmap(cMvdInput);
            foreach (RegionCellX3Class cell in xRecipe.xRegionCells)
            {
                cell.Reset();
                if (INI.Instance.IsSaveTestImage)
                {
                    cell.IsSaveDebugPicture = true;
                    cell.SaveDebugPath = imgPath;
                }

                if (cell.ByPass && !INI.Instance.IsForceInspect)
                {
                    //cell.inspectReason = InspectReason.INS_NOOPEN;
                    cell.inspectReasons.Add(InspectReason.INS_NOOPEN);
                    continue;
                }

                RectangleF _rectF = new RectangleF(cell.viewRectF.X, cell.viewRectF.Y, cell.viewRectF.Width, cell.viewRectF.Height);
                _rectF.Inflate(xRecipe.xExtendx, xRecipe.xExtendy);
                GaUtil.BoundRect(ref _rectF, _bmpInputSize);
                
                //xRecipe.mvdprinttemp_Find.bmpRun_Image = bmpInputImage.Clone(_rectF, PixelFormat.Format8bppIndexed);
                Bitmap bmp2 = bmpInputImage.Clone(_rectF, PixelFormat.Format8bppIndexed);
                int iOK = xRecipe.PrintTempRun(bmp2);
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
                    PointF _viewNewRun = new PointF(cell.DrawResultRectF().CenterX,
                        cell.DrawResultRectF().CenterY);
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
                if (cell.ByPass && !INI.Instance.IsForceInspect)
                    continue;
                if (cell.inspectReason == InspectReason.INS_ALIGNERR)
                    continue;
                //cell.xInspectPara = InspectX2Class.Instance;

                //原始模板的大小
                RectangleF templaterectf = new RectangleF(0, 0, xRecipe.bmpprinttemplate.Width, xRecipe.bmpprinttemplate.Height);

                //定位完成后裁切位置
                RectangleF _crop = new RectangleF(cell.DrawResultRectF().CenterX - templaterectf.Width / 2,
                    cell.DrawResultRectF().CenterY - templaterectf.Height / 2,
                    templaterectf.Width,
                    templaterectf.Height);

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

                //if (InspectX2Class.Instance.bCheckCode)
                if (m_QrUsed)
                {
                    RectangleF _cropCode = new RectangleF(xRecipe.xRectCodeRegion.X + _crop.X,
                        xRecipe.xRectCodeRegion.Y + _crop.Y,
                        xRecipe.xRectCodeRegion.Width,
                        xRecipe.xRectCodeRegion.Height);
                    cell.bmpItemCodeRun.Dispose();
                    cell.bmpItemCodeRun = bmpInputImage.Clone(_cropCode, PixelFormat.Format8bppIndexed);
                    cell.DeCode2D(cell.bmpItemCodeRun, _cropCode.Location, m_QrJudged);
                    //cell.DeCode2D(cell.bmpItemCodeRun, _crop.Location, xRecipe.xRectCodeRegion.Location);
                }
            }

            if (INI.Instance.IsSaveDebugBMP)
            {
                GaImageUtil.SaveImageWithQuality(bmpInputImage, $"{m_PicResultPath}\\{LotId}-{DateTime.Now.ToString("yyyyMMddHHmmss")}.jpg", INI.Instance.ImageQuality);
            }

            if (INI.Instance.IsSaveDebugOrgBmp)
            {
                IEzImage ezImage = new EzFreeBitmap(bmpInputImage, false);
                ezImage.Save($"{m_PicResultOrgPath}\\{LotId}-{DateTime.Now.ToString("yyyyMMddHHmmss")}.jpg");
                //bmpInputImage.Save($"{m_PicResultOrgPath}\\{LotId}-{DateTime.Now.ToString("yyyyMMddHHmmss")}.jpg",
                //                   ImageFormat.Jpeg);
            }

            bmpInputImage.Dispose();
            m_IsPass = true;// xRecipe.AnalyzeDatasRun();

            stopwatch.Stop();
            m_ElapsedTime = stopwatch.ElapsedMilliseconds;
            m_Running = false;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
        /// <summary>
        /// 空载台检测
        /// </summary>
        private void _Inspect003()
        {
            xRecipe.AnalyzeDatasData();
            m_ElapsedTime = 0;
            m_Running = true;

            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Restart();

            string imgPath = $"{Universal.LOG_IMG_PATH}\\{JzTimes.DateSerialString}\\{m_FileBarcodeStr}";
            if (INI.Instance.IsSaveTestImage)
            {
                if (!Directory.Exists(imgPath))
                    Directory.CreateDirectory(imgPath);
            }

            Size _bmpInputSize = new Size((int)cMvdInput.Width, (int)cMvdInput.Height);

            //if (m_MvdOpeate == null)
            //    m_MvdOpeate = new CMvdImage();

            //m_MvdOpeate = cMvdInput.Clone();
            Bitmap bmpInputImage = EzMvdImageConvertor.CMvdImageToBitmap(cMvdInput);

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
                GaUtil.BoundRect(ref _rectF, _bmpInputSize);
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
                    PointF _viewNewRun = new PointF(cell.DrawResultRectF().CenterX,
                        cell.DrawResultRectF().CenterY);
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
                GaUtil.BoundRect(ref _rectF, _bmpInputSize);
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

            bmpInputImage.Dispose();
            m_IsPass = true;// xRecipe.AnalyzeDatasRun();

            stopwatch.Stop();
            m_ElapsedTime = stopwatch.ElapsedMilliseconds;
            m_Running = false;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        private void _InspectRecipe()
        {
            //xRecipe.AnalyzeDatasData();
            m_ElapsedTime = 0;
            m_Running = true;
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Restart();

            Size _bmpInputSize = new Size((int)cMvdInput.Width, (int)cMvdInput.Height);

            //if (m_MvdOpeate == null)
            //    m_MvdOpeate = new CMvdImage();

            //m_MvdOpeate = cMvdInput.Clone();
            Bitmap bmpInputImage = EzMvdImageConvertor.CMvdImageToBitmap(cMvdInput);

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
            foreach (RegionCellX3Class cell in xRecipe.xRegionCells)
            {
                cell.Reset();

                //if (cell.ByPass)
                //    continue;

                RectangleF _rectF = new RectangleF(cell.viewRectF.X, cell.viewRectF.Y, cell.viewRectF.Width, cell.viewRectF.Height);
                _rectF.Inflate(100, 100);
                GaUtil.BoundRect(ref _rectF, _bmpInputSize);
                Bitmap bmp2 = bmpInputImage.Clone(_rectF, PixelFormat.Format8bppIndexed);
                int iOK = xRecipe.PrintTempRun(bmp2);
                if (iOK == 0)
                {
                    cell.xFindResult = xRecipe.mvdprinttemp_Find.xResults[0];
                    cell.xFindResult.fCenterX += _rectF.X;
                    cell.xFindResult.fCenterY += _rectF.Y;
                    RectangleF templaterectf = new RectangleF(0, 0, xRecipe.bmpprinttemplate.Width, xRecipe.bmpprinttemplate.Height);
                    Rectangle runrectf = new Rectangle(0, 0, _bmpInputSize.Width, _bmpInputSize.Height);
                    cell.PositionFixRun(templaterectf, runrectf, cell.xFindResult);

                    //记录原始的位置pix
                    cell.OrgX = cell.DrawResultRectF().CenterX;// - mVD_POINT_F0.fX;
                    cell.OrgY = cell.DrawResultRectF().CenterY;// - mVD_POINT_F0.fY;
                    cell.OrgAngle = cell.DrawResultRectF().Angle;
                }
                else
                {
                    cell.inspectReason = InspectReason.INS_ALIGNERR;
                    cell.inspectReasons.Add(InspectReason.INS_ALIGNERR);
                }

                bmp2.Dispose();
            }
            bmpInputImage.Dispose();

            stopwatch.Stop();
            m_ElapsedTime = stopwatch.ElapsedMilliseconds;
            m_Running = false;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

#endif

        #endregion

        #region INSPECT_001
        /// <summary>
        /// 外观及尺寸检测
        /// </summary>
        private void _Inspect001_LT()
        {
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
                
                // cBoxOverlapTool
                if (cBoxOverlapTool == null)
                {
                    cBoxOverlapTool = new CBoxOverlapTool();
                    _TM.Trace("_Inspect001 : new CBoxOverlapTool()");
                }

                // 準備資料夾
                string imgPath = $"{Universal.LOG_IMG_PATH}\\{JzTimes.DateSerialString}\\{m_FileBarcodeStr}";

                #region PREPARE_PATH
                if (INI.Instance.IsSaveTestImage)
                {
                    if (!Directory.Exists(imgPath))
                        Directory.CreateDirectory(imgPath);
                }
                #endregion

                // 取得線掃巨圖: 轉換 CMvdImage (cMvdInput) 成 Bitmap 
                bmpInputImage = GaImageUtil.CMvdImageToBitmap(cMvdInput);
                _TM.Trace("_Inspect001 : CMvdImage To Bitmap 完成.");

#if (OPT_OLD)
                Size _bmpInputSize = bmpInputImage.Size;
                string debugCellCenterStr = string.Empty;
                _TM.Trace("_Inspect001 : EzMvdImageConvertor.CMvdImageToBitmap");

                foreach (RegionCellX3Class cell in xRecipe.xRegionCells)
                {
                    cell.Reset();

                    if (INI.Instance.IsSaveTestImage)
                    {
                        cell.IsSaveDebugPicture = true;
                        cell.SaveDebugPath = imgPath;
                    }

                    RectangleF _rectF = new RectangleF(cell.viewRectF.X, cell.viewRectF.Y, cell.viewRectF.Width, cell.viewRectF.Height);
                    _rectF.Inflate(xRecipe.xExtendx, xRecipe.xExtendy);
                    GaUtil.BoundRect(ref _rectF, _bmpInputSize);

                    //xRecipe.mvdprinttemp_Find.bmpRun_Image = bmpInputImage.Clone(_rectF, PixelFormat.Format8bppIndexed);
                    Bitmap bmp2 = bmpInputImage.Clone(_rectF, PixelFormat.Format8bppIndexed);
                    int iOK = xRecipe.PrintTempRun(bmp2);

                    if (cell.IsSaveDebugPicture)
                    {
                        string posfixpath = cell.SaveDebugPath + "\\PositionFix";
                        if (!Directory.Exists(posfixpath))
                            Directory.CreateDirectory(posfixpath);

                        bmp2.Save(posfixpath + $"\\Fix_{cell.Index}_{cell.lblName}.bmp", ImageFormat.Bmp);
                    }

                    if (iOK == 0)
                    {
                        cell.xFindResult = xRecipe.mvdprinttemp_Find.xResults[0];

                        debugCellCenterStr += $"INDEX:{cell.Index}#";
                        debugCellCenterStr += $"VIEW:{_rectF.X};{_rectF.Y}#";
                        debugCellCenterStr += $"ORG:{cell.xFindResult.fCenterX};{cell.xFindResult.fCenterY}#";

                        cell.xFindResult.fCenterX += _rectF.X;
                        cell.xFindResult.fCenterY += _rectF.Y;

                        debugCellCenterStr += $"DES:{cell.xFindResult.fCenterX};{cell.xFindResult.fCenterY}{Environment.NewLine}";

                        RectangleF templaterectf //= new RectangleF(0, 0, xRecipe.bmpprinttemplate.Width, xRecipe.bmpprinttemplate.Height);
                                = new RectangleF(0, 0, xRecipe.PrintTemplateSize.Width, xRecipe.PrintTemplateSize.Height);
                        Rectangle runrectf = new Rectangle(0, 0, _bmpInputSize.Width, _bmpInputSize.Height);
                        cell.PositionFixRun(templaterectf, runrectf, cell.xFindResult);

                        //增加重叠区域的判断
                        cBoxOverlapTool.ROI1 = new CMvdRectangleF(
                            cell.viewRectF.X + cell.viewRectF.Width / 2,
                            cell.viewRectF.Y + cell.viewRectF.Height / 2,
                            cell.viewRectF.Width,
                            cell.viewRectF.Height);
                        cBoxOverlapTool.ROI2 = cell.DrawResultRectF();

                        cBoxOverlapTool.Run();
                        if (cBoxOverlapTool.Result.Overlap >= xInspect.xChipOverlap)
                        {
                            //计算偏移值
                            PointF _viewNewRun = new PointF(cell.DrawResultRectF().CenterX,
                                 cell.DrawResultRectF().CenterY);
                            PointF _worldNewRun = LineScanCalibrate.ViewToWorld(_viewNewRun);
                            cell.RunX = (_worldNewRun.X - cell.OrgX);
                            cell.RunY = (_worldNewRun.Y - cell.OrgY);
                            cell.RunAngle = cell.DrawResultRectF().Angle;

                            cell.GetOffsetResult();

                            if (xInspect.bOpenLineMeasure)
                            {
                                #region 直线寻找

                                //左边
                                RectangleF r0 = new RectangleF(xRecipe.xLineLeft.X,
                                    xRecipe.xLineLeft.Y,
                                    xRecipe.xLineLeft.Width,
                                    xRecipe.xLineLeft.Height);
                                CMvdRectangleF mv0 = new CMvdRectangleF(r0.X + r0.Width / 2, r0.Y + r0.Height / 2, r0.Width, r0.Height);
                                CMvdRectangleF mv0ret = cell.PositionFixRun(mv0, xRecipe.xRegionTrain, Rectangle.Round(_rectF),
                                    xRecipe.mvdprinttemp_Find.xResults[0]) as CMvdRectangleF;
                                cell.LineSegmentRun(0, bmp2, mv0ret);
                                mv0ret.CenterX += _rectF.X;
                                mv0ret.CenterY += _rectF.Y;
                                cell.cMvdShapesForFindLineRegion[0] = (CMvdShape)mv0ret.Clone();

                                //上边
                                RectangleF r1 = new RectangleF(xRecipe.xLineTop.X,
                                    xRecipe.xLineTop.Y,
                                    xRecipe.xLineTop.Width,
                                    xRecipe.xLineTop.Height);
                                CMvdRectangleF mv1 = new CMvdRectangleF(r1.X + r1.Width / 2, r1.Y + r1.Height / 2, r1.Width, r1.Height);
                                CMvdRectangleF mv1ret = cell.PositionFixRun(mv1, xRecipe.xRegionTrain, Rectangle.Round(_rectF),
                                    xRecipe.mvdprinttemp_Find.xResults[0]) as CMvdRectangleF;
                                cell.LineSegmentRun(1, bmp2, mv1ret);
                                mv1ret.CenterX += _rectF.X;
                                mv1ret.CenterY += _rectF.Y;
                                cell.cMvdShapesForFindLineRegion[1] = (CMvdShape)mv1ret.Clone();

                                //右边
                                RectangleF r2 = new RectangleF(xRecipe.xLineRight.X,
                                    xRecipe.xLineRight.Y,
                                    xRecipe.xLineRight.Width,
                                    xRecipe.xLineRight.Height);
                                CMvdRectangleF mv2 = new CMvdRectangleF(r2.X + r2.Width / 2, r2.Y + r2.Height / 2, r2.Width, r2.Height);
                                CMvdRectangleF mv2ret = cell.PositionFixRun(mv2, xRecipe.xRegionTrain, Rectangle.Round(_rectF),
                                    xRecipe.mvdprinttemp_Find.xResults[0]) as CMvdRectangleF;
                                cell.LineSegmentRun(2, bmp2, mv2ret);
                                mv2ret.CenterX += _rectF.X;
                                mv2ret.CenterY += _rectF.Y;
                                cell.cMvdShapesForFindLineRegion[2] = (CMvdShape)mv2ret.Clone();

                                //下边
                                RectangleF r3 = new RectangleF(xRecipe.xLineBottom.X,
                                    xRecipe.xLineBottom.Y,
                                    xRecipe.xLineBottom.Width,
                                    xRecipe.xLineBottom.Height);
                                CMvdRectangleF mv3 = new CMvdRectangleF(r3.X + r3.Width / 2, r3.Y + r3.Height / 2, r3.Width, r3.Height);
                                CMvdRectangleF mv3ret = cell.PositionFixRun(mv3, xRecipe.xRegionTrain, Rectangle.Round(_rectF),
                                    xRecipe.mvdprinttemp_Find.xResults[0]) as CMvdRectangleF;
                                cell.LineSegmentRun(3, bmp2, mv3ret);
                                mv3ret.CenterX += _rectF.X;
                                mv3ret.CenterY += _rectF.Y;
                                cell.cMvdShapesForFindLineRegion[3] = (CMvdShape)mv3ret.Clone();

                                //长度

                                try
                                {
                                    if (cell.cMvdLineSegmentFsOut[0] != null && cell.cMvdLineSegmentFsOut[2] != null)
                                    {
                                        // CreateInstance

                                        VisionDesigner.L2LMeasure.CL2LMeasureTool cL2LMeasureToolObj = new VisionDesigner.L2LMeasure.CL2LMeasureTool();

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

                                        VisionDesigner.L2LMeasure.CL2LMeasureResult cL2LMeasureRes = cL2LMeasureToolObj.Result;

                                        cell.RunWidth = cL2LMeasureRes.VerticalAbsDist * INI.Instance.ImageResolution;

                                        Console.WriteLine("Angle: {0}", cL2LMeasureRes.Angle);

                                        Console.WriteLine("Vertical distance: {0}", cL2LMeasureRes.VerticalAbsDist);

                                        cL2LMeasureToolObj.Dispose();
                                        cL2LMeasureToolObj = null;
                                    }


                                }

                                catch (MvdException ex)

                                {

                                    Console.WriteLine("Fail with ErrorCode: 0x" + ex.ErrorCode.ToString("X"));

                                }

                                catch (System.Exception ex)

                                {

                                    Console.WriteLine("Fail with error " + ex.Message);

                                }

                                //宽度

                                try

                                {
                                    if (cell.cMvdLineSegmentFsOut[1] != null && cell.cMvdLineSegmentFsOut[3] != null)
                                    {
                                        // CreateInstance

                                        VisionDesigner.L2LMeasure.CL2LMeasureTool cL2LMeasureToolObj = new VisionDesigner.L2LMeasure.CL2LMeasureTool();

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

                                        VisionDesigner.L2LMeasure.CL2LMeasureResult cL2LMeasureRes = cL2LMeasureToolObj.Result;

                                        cell.RunHeight = cL2LMeasureRes.VerticalAbsDist * INI.Instance.ImageResolution;

                                        Console.WriteLine("Angle: {0}", cL2LMeasureRes.Angle);

                                        Console.WriteLine("Vertical distance: {0}", cL2LMeasureRes.VerticalAbsDist);

                                        cL2LMeasureToolObj.Dispose();
                                        cL2LMeasureToolObj = null;
                                    }


                                }

                                catch (MvdException ex)

                                {

                                    Console.WriteLine("Fail with ErrorCode: 0x" + ex.ErrorCode.ToString("X"));

                                }

                                catch (System.Exception ex)

                                {

                                    Console.WriteLine("Fail with error " + ex.Message);

                                }

                                #endregion
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

                    bmp2.Dispose();
                }
#endif
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
                m_ElapsedTime = stopwatch.ElapsedMilliseconds;
                m_Running = false;

                // 釋放巨圖
                bmpInputImage?.Dispose();
                bmpInputImage = null;
            }
            catch (Exception ex)
            {
                // 釋放巨圖
                bmpInputImage?.Dispose();
                bmpInputImage = null;
                throw ex;
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
        private void _Inspect001_Chip_Location_And_Measurement(Bitmap bmpInputImage, string imgPath, out string debugCellCenterStr)
        {
            debugCellCenterStr = "";
            var _bmpInputSize = bmpInputImage.Size;

            foreach (RegionCellX3Class cell in xRecipe.xRegionCells)
            {
                cell.Reset();

                if (INI.Instance.IsSaveTestImage)
                {
                    cell.IsSaveDebugPicture = true;
                    cell.SaveDebugPath = imgPath;
                }

                RectangleF _rectF = new RectangleF(cell.viewRectF.X, cell.viewRectF.Y, cell.viewRectF.Width, cell.viewRectF.Height);
                _rectF.Inflate(xRecipe.xExtendx, xRecipe.xExtendy);
                GaUtil.BoundRect(ref _rectF, _bmpInputSize);

                //xRecipe.mvdprinttemp_Find.bmpRun_Image = bmpInputImage.Clone(_rectF, PixelFormat.Format8bppIndexed);
                Bitmap bmp2 = bmpInputImage.Clone(_rectF, PixelFormat.Format8bppIndexed);
                int iOK = xRecipe.PrintTempRun(bmp2);

                if (cell.IsSaveDebugPicture)
                {
                    string posfixpath = cell.SaveDebugPath + "\\PositionFix";
                    if (!Directory.Exists(posfixpath))
                        Directory.CreateDirectory(posfixpath);

                    bmp2.Save(posfixpath + $"\\Fix_{cell.Index}_{cell.lblName}.bmp", ImageFormat.Bmp);
                }

                if (iOK == 0)
                {
                    cell.xFindResult = xRecipe.mvdprinttemp_Find.xResults[0];

                    debugCellCenterStr += $"INDEX:{cell.Index}#";
                    debugCellCenterStr += $"VIEW:{_rectF.X};{_rectF.Y}#";
                    debugCellCenterStr += $"ORG:{cell.xFindResult.fCenterX};{cell.xFindResult.fCenterY}#";

                    cell.xFindResult.fCenterX += _rectF.X;
                    cell.xFindResult.fCenterY += _rectF.Y;

                    debugCellCenterStr += $"DES:{cell.xFindResult.fCenterX};{cell.xFindResult.fCenterY}{Environment.NewLine}";

                    RectangleF templaterectf //= new RectangleF(0, 0, xRecipe.bmpprinttemplate.Width, xRecipe.bmpprinttemplate.Height);
                            = new RectangleF(0, 0, xRecipe.PrintTemplateSize.Width, xRecipe.PrintTemplateSize.Height);
                    Rectangle runrectf = new Rectangle(0, 0, _bmpInputSize.Width, _bmpInputSize.Height);
                    cell.PositionFixRun(templaterectf, runrectf, cell.xFindResult);

                    //增加重叠区域的判断
                    cBoxOverlapTool.ROI1 = new CMvdRectangleF(
                        cell.viewRectF.X + cell.viewRectF.Width / 2,
                        cell.viewRectF.Y + cell.viewRectF.Height / 2,
                        cell.viewRectF.Width,
                        cell.viewRectF.Height);
                    cBoxOverlapTool.ROI2 = cell.DrawResultRectF();

                    cBoxOverlapTool.Run();
                    if (cBoxOverlapTool.Result.Overlap >= xInspect.xChipOverlap)
                    {
                        //计算偏移值
                        PointF _viewNewRun = new PointF(cell.DrawResultRectF().CenterX,
                             cell.DrawResultRectF().CenterY);
                        PointF _worldNewRun = LineScanCalibrate.ViewToWorld(_viewNewRun);

                        ////原来基础位置的world坐标
                        //PointF _viewOrg = new PointF(cell.viewRectF.X + cell.viewRectF.Width / 2,
                        //                             cell.viewRectF.Y + cell.viewRectF.Height / 2);
                        //PointF _worldOrg = LineScanCalibrate.ViewToWorld(_viewOrg);
                        //cell.OrgX = _worldOrg.X;
                        //cell.OrgY = _worldOrg.Y;

                        cell.RunX = (_worldNewRun.X - cell.OrgX);
                        cell.RunY = (_worldNewRun.Y - cell.OrgY);
                        cell.RunAngle = cell.DrawResultRectF().Angle;

                        cell.GetOffsetResult();

                        if (xInspect.bOpenLineMeasure)
                        {
                            #region 直线寻找

                            //左边
                            RectangleF r0 = new RectangleF(xRecipe.xLineLeft.X,
                                xRecipe.xLineLeft.Y,
                                xRecipe.xLineLeft.Width,
                                xRecipe.xLineLeft.Height);
                            CMvdRectangleF mv0 = new CMvdRectangleF(r0.X + r0.Width / 2, r0.Y + r0.Height / 2, r0.Width, r0.Height);
                            CMvdRectangleF mv0ret = cell.PositionFixRun(mv0, xRecipe.xRegionTrain, Rectangle.Round(_rectF),
                                xRecipe.mvdprinttemp_Find.xResults[0]) as CMvdRectangleF;
                            cell.LineSegmentRun(0, bmp2, mv0ret);
                            mv0ret.CenterX += _rectF.X;
                            mv0ret.CenterY += _rectF.Y;
                            cell.cMvdShapesForFindLineRegion[0] = (CMvdShape)mv0ret.Clone();

                            //上边
                            RectangleF r1 = new RectangleF(xRecipe.xLineTop.X,
                                xRecipe.xLineTop.Y,
                                xRecipe.xLineTop.Width,
                                xRecipe.xLineTop.Height);
                            CMvdRectangleF mv1 = new CMvdRectangleF(r1.X + r1.Width / 2, r1.Y + r1.Height / 2, r1.Width, r1.Height);
                            CMvdRectangleF mv1ret = cell.PositionFixRun(mv1, xRecipe.xRegionTrain, Rectangle.Round(_rectF),
                                xRecipe.mvdprinttemp_Find.xResults[0]) as CMvdRectangleF;
                            cell.LineSegmentRun(1, bmp2, mv1ret);
                            mv1ret.CenterX += _rectF.X;
                            mv1ret.CenterY += _rectF.Y;
                            cell.cMvdShapesForFindLineRegion[1] = (CMvdShape)mv1ret.Clone();

                            //右边
                            RectangleF r2 = new RectangleF(xRecipe.xLineRight.X,
                                xRecipe.xLineRight.Y,
                                xRecipe.xLineRight.Width,
                                xRecipe.xLineRight.Height);
                            CMvdRectangleF mv2 = new CMvdRectangleF(r2.X + r2.Width / 2, r2.Y + r2.Height / 2, r2.Width, r2.Height);
                            CMvdRectangleF mv2ret = cell.PositionFixRun(mv2, xRecipe.xRegionTrain, Rectangle.Round(_rectF),
                                xRecipe.mvdprinttemp_Find.xResults[0]) as CMvdRectangleF;
                            cell.LineSegmentRun(2, bmp2, mv2ret);
                            mv2ret.CenterX += _rectF.X;
                            mv2ret.CenterY += _rectF.Y;
                            cell.cMvdShapesForFindLineRegion[2] = (CMvdShape)mv2ret.Clone();

                            //下边
                            RectangleF r3 = new RectangleF(xRecipe.xLineBottom.X,
                                xRecipe.xLineBottom.Y,
                                xRecipe.xLineBottom.Width,
                                xRecipe.xLineBottom.Height);
                            CMvdRectangleF mv3 = new CMvdRectangleF(r3.X + r3.Width / 2, r3.Y + r3.Height / 2, r3.Width, r3.Height);
                            CMvdRectangleF mv3ret = cell.PositionFixRun(mv3, xRecipe.xRegionTrain, Rectangle.Round(_rectF),
                                xRecipe.mvdprinttemp_Find.xResults[0]) as CMvdRectangleF;
                            cell.LineSegmentRun(3, bmp2, mv3ret);
                            mv3ret.CenterX += _rectF.X;
                            mv3ret.CenterY += _rectF.Y;
                            cell.cMvdShapesForFindLineRegion[3] = (CMvdShape)mv3ret.Clone();

                            //长度

                            try
                            {
                                if (cell.cMvdLineSegmentFsOut[0] != null && cell.cMvdLineSegmentFsOut[2] != null)
                                {
                                    // CreateInstance

                                    VisionDesigner.L2LMeasure.CL2LMeasureTool cL2LMeasureToolObj = new VisionDesigner.L2LMeasure.CL2LMeasureTool();

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

                                    VisionDesigner.L2LMeasure.CL2LMeasureResult cL2LMeasureRes = cL2LMeasureToolObj.Result;

                                    cell.RunWidth = cL2LMeasureRes.VerticalAbsDist * INI.Instance.ImageResolution;

                                    Console.WriteLine("Angle: {0}", cL2LMeasureRes.Angle);

                                    Console.WriteLine("Vertical distance: {0}", cL2LMeasureRes.VerticalAbsDist);

                                    cL2LMeasureToolObj.Dispose();
                                    cL2LMeasureToolObj = null;
                                }


                            }

                            catch (MvdException ex)
                            {
                                Console.WriteLine("Fail with ErrorCode: 0x" + ex.ErrorCode.ToString("X"));
                            }
                            catch (System.Exception ex)
                            {
                                Console.WriteLine("Fail with error " + ex.Message);
                            }

                            //宽度

                            try
                            {
                                if (cell.cMvdLineSegmentFsOut[1] != null && cell.cMvdLineSegmentFsOut[3] != null)
                                {
                                    // CreateInstance

                                    VisionDesigner.L2LMeasure.CL2LMeasureTool cL2LMeasureToolObj = new VisionDesigner.L2LMeasure.CL2LMeasureTool();

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

                                    VisionDesigner.L2LMeasure.CL2LMeasureResult cL2LMeasureRes = cL2LMeasureToolObj.Result;

                                    cell.RunHeight = cL2LMeasureRes.VerticalAbsDist * INI.Instance.ImageResolution;

                                    Console.WriteLine("Angle: {0}", cL2LMeasureRes.Angle);

                                    Console.WriteLine("Vertical distance: {0}", cL2LMeasureRes.VerticalAbsDist);

                                    cL2LMeasureToolObj.Dispose();
                                    cL2LMeasureToolObj = null;
                                }
                            }
                            catch (MvdException ex)
                            {
                                Console.WriteLine("Fail with ErrorCode: 0x" + ex.ErrorCode.ToString("X"));
                            }
                            catch (System.Exception ex)
                            {
                                Console.WriteLine("Fail with error " + ex.Message);
                            }

                            #endregion
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

                bmp2.Dispose();
            }
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
            IEzImage ezImage = new EzFreeBitmap(bmpInputImage, true);
            Task task = new Task(() =>
            {
                try
                {
                    if (INI.Instance.IsSaveTestImage)
                    {
                        GaUtil.SaveData(debugCellCenterStr, 
                            imgPath + $"\\PositionFix\\DEBUG_{DateTime.Now.ToString("yyyyMMddHHmmss")}.txt");
                    }

                    if (INI.Instance.IsSaveDebugBMP)
                    {
                        GaImageUtil.SaveImageWithQuality(ezImage.Bitmap, 
                            $"{m_PicResultPath}\\{LotId}-{DateTime.Now.ToString("yyyyMMddHHmmss")}.jpg", 
                            INI.Instance.ImageQuality);
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
        }
        #endregion

        #region INSPECT_003
        /// <summary>
        /// 空载台检测
        /// </summary>
        private void _Inspect003_LT()
        {
            xRecipe.AnalyzeDatasData();

            m_ElapsedTime = 0;
            m_Running = true;
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Restart();

            var aoiModel = LtAoiFactory.InstanceModel();
            using (Bitmap bmpInputImage = EzMvdImageConvertor.CMvdImageToBitmap(cMvdInput))
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
            m_ElapsedTime = stopwatch.ElapsedMilliseconds;
            m_Running = false;
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
            IEzImage ezImage = new EzFreeBitmap(bmpInputImage, true);
            Task task = new Task(() =>
            {
                try
                {
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
            });
            task.Start();
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
    }
}
