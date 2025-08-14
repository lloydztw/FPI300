using Eazy_Project_III;
using JetEazy.BasicSpace;
using LaserAlignDX.OPSpace;
using LaserAlignDX.OPSpace.RecipeSpace;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using Traveller106;
using VisionDesigner;

namespace LaserAlignDX.RunSpace
{
    public class ProcessRunClass
    {
        #region SINGLETON
        protected ProcessRunClass()
        {

        }
        private static ProcessRunClass _instance = null;
        #endregion

        public static ProcessRunClass Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ProcessRunClass();
                return _instance;
            }
        }

        protected RecipeMainX2Class xRecipe
        {
            get { return RecipeMainX2Class.Instance; }
        }
        protected InspectX2Class xInspect
        {
            get { return InspectX2Class.Instance; }
        }

        public CMvdImage cMvdInput = new CMvdImage();
        public MVD_POINT_F mVD_POINT_F0 = new MVD_POINT_F();
        public MVD_POINT_F mVD_POINT_F1 = new MVD_POINT_F();

        #region PRIVATE_DATA
        //private CMvdImage m_MvdOpeate = new CMvdImage();
        private long m_ElapsedTime = 0;
        private bool m_Running = false;

        private bool m_IsPass = false;
        private string m_ResultDesc = string.Empty;
        private string m_FileBarcodeStr = string.Empty;
        #endregion


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

            //if (!myRecipe.Ischip_open_measure)
            //    return;

            runTest();

            //System.Threading.Thread thread = new System.Threading.Thread(runTest);
            //thread.IsBackground = true;
            //thread.Start();
        }
        public void RunRecipe()
        {
            m_IsPass = true;
            m_ResultDesc = string.Empty;

            //if (!myRecipe.Ischip_open_measure)
            //    return;

            _InspectRecipe();

            //System.Threading.Thread thread = new System.Threading.Thread(runTest);
            //thread.IsBackground = true;
            //thread.Start();
        }

        #region 更换底图

        public int ChangeModelBackgroudImage()
        {
            //_Inspect001();

            int iret = 0;
            if (mVD_POINT_F0.fX == -9999 || mVD_POINT_F0.fX == -9999)
            {
                iret = -2;//定位失败直接跳过
                return iret;
            }
            foreach (RegionCellX2Class cell in xRecipe.xRegionCells)
            {
                if (cell.inspectReason == InspectReason.INS_ALIGNERR)
                {
                    iret = -3;//只要定位失败的则跳过
                    break;
                }
            }

            if (iret != 0)
            {
                return iret;
            }

            //定位到第一张图
            foreach (RegionCellX2Class cell in xRecipe.xRegionCells)
            {
                xRecipe.bmpprinttemplate.Dispose();
                xRecipe.bmpprinttemplate = cell.bmpItemRun.Clone(new RectangleF(0, 0, cell.bmpItemRun.Width, cell.bmpItemRun.Height),
                    PixelFormat.Format8bppIndexed);
                //xRecipe.SavePrintTemplate();
                break;
                ////原始模板的大小
                //RectangleF templaterectf = new RectangleF(0, 0, xRecipe.bmpprinttemplate.Width, xRecipe.bmpprinttemplate.Height);

                ////定位完成后裁切位置
                //RectangleF _crop = new RectangleF(cell.DrawResultRectF().CenterX - templaterectf.Width / 2,
                //    cell.DrawResultRectF().CenterY - templaterectf.Height / 2,
                //    templaterectf.Width,
                //    templaterectf.Height);
            }

            int iOK = xRecipe.PrintTempTrain();
            if (iOK != 0)
            {
                iret = -4;//训练失败跳过
                return iret;
            }
            xRecipe.SavePrintTemplate();
            //iret = 0;

            return iret;
        }

        #endregion

        private void runTest()
        {
            _Inspect001();
        }
        private void _Inspect001()
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
            Bitmap bmpInputImage = CMvdImageToBitmap(cMvdInput);

            //if (cMvdInput.PixelFormat != MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08)
            //{
            //    //当前程序仅支持mono8。因此像素格会转换.
            //    cMvdInput.ConvertImagePixelFormat(MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08);
            //}

            Bitmap bmp0 = bmpInputImage.Clone(xRecipe.xRectRegionBase0, PixelFormat.Format8bppIndexed);
            Bitmap bmp1 = bmpInputImage.Clone(xRecipe.xRectRegionBase1, PixelFormat.Format8bppIndexed);

            //计算基准位置 用来检测偏移
            mVD_POINT_F0 = _getBasePointF2(bmp0, RegionName.BASE0);
            mVD_POINT_F1 = _getBasePointF2(bmp1, RegionName.BASE1);

            bmp0.Dispose();
            bmp1.Dispose();

            //xRecipe.mvdprinttemp_Find.xMvdRun_Image = m_MvdOpeate.Clone();
            //xRecipe.mvdprinttemp_Find.HikRun4Pre();
            foreach (RegionCellX2Class cell in xRecipe.xRegionCells)
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
                _rectF.Inflate(100, 100);
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
                    cell.RunX = (cell.DrawResultRectF().CenterX - mVD_POINT_F0.fX - cell.OrgX) * INI.Instance.ImageResolution;
                    cell.RunY = (cell.DrawResultRectF().CenterY - mVD_POINT_F0.fY - cell.OrgY) * INI.Instance.ImageResolution;

                    cell.GetOffsetResult();
                }
                else
                {
                    cell.inspectReason = InspectReason.INS_ALIGNERR;
                    cell.inspectReasons.Add(InspectReason.INS_ALIGNERR);
                }

                bmp2.Dispose();
            }

            //Bitmap bmpInputImage = CMvdImageToBitmap(m_MvdOpeate);
            foreach (RegionCellX2Class cell in xRecipe.xRegionCells)
            {
                if (cell.ByPass && !INI.Instance.IsForceInspect)
                    continue;
                if (cell.inspectReason == InspectReason.INS_ALIGNERR)
                    continue;
                cell.xInspectPara = InspectX2Class.Instance;

                //原始模板的大小
                RectangleF templaterectf = new RectangleF(0, 0, xRecipe.bmpprinttemplate.Width, xRecipe.bmpprinttemplate.Height);

                //定位完成后裁切位置
                RectangleF _crop = new RectangleF(cell.DrawResultRectF().CenterX - templaterectf.Width / 2,
                    cell.DrawResultRectF().CenterY - templaterectf.Height / 2,
                    templaterectf.Width,
                    templaterectf.Height);

                if (InspectX2Class.Instance.bCheckInspect)
                {
                    cell.bmpItemRun.Dispose();
                    cell.bmpItemRun = bmpInputImage.Clone(_crop, PixelFormat.Format8bppIndexed);
                    cell.bmpItemMask.Dispose();
                    cell.bmpItemMask = xRecipe.bmpprintmask.Clone(
                        new Rectangle(0, 0, xRecipe.bmpprintmask.Width, xRecipe.bmpprintmask.Height),
                        PixelFormat.Format8bppIndexed);
                    cell.DetectDefects(xRecipe.bmpprinttemplate, cell.bmpItemRun, cell.bmpItemMask);
                }

                if (InspectX2Class.Instance.bCheckCode)
                {
                    RectangleF _cropCode = new RectangleF(xRecipe.xRectCodeRegion.X + _crop.X,
                        xRecipe.xRectCodeRegion.Y + _crop.Y,
                        xRecipe.xRectCodeRegion.Width,
                        xRecipe.xRectCodeRegion.Height);
                    cell.bmpItemCodeRun.Dispose();
                    cell.bmpItemCodeRun = bmpInputImage.Clone(_cropCode, PixelFormat.Format8bppIndexed);
                    cell.DeCode2D(cell.bmpItemCodeRun, _cropCode.Location);
                    //cell.DeCode2D(cell.bmpItemCodeRun, _crop.Location, xRecipe.xRectCodeRegion.Location);
                }
            }
            bmpInputImage.Dispose();

            //检测重复码
            if (InspectX2Class.Instance.bCheckRepeatCode)
            {
                //判断表是否存在
                bool bExist = JzCheckRepeatClass.Instance.MySqlCheckTableExist();
                if (!bExist)
                {
                    int iret = JzCheckRepeatClass.Instance.MySqlCreateTable();
                    if (iret >= 0)
                    {
                        bool bChk = xRecipe.RunRepeatCode();

                    }
                }
                else
                {
                    xRecipe.RunRepeatCode();
                }
            }

            m_IsPass = xRecipe.AnalyzeDatasRun();


            //if (m_MvdOpeate != null)
            //    m_MvdOpeate.Dispose();

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
            Bitmap bmpInputImage = CMvdImageToBitmap(cMvdInput);

            //if (cMvdInput.PixelFormat != MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08)
            //{
            //    //当前程序仅支持mono8。因此像素格会转换.
            //    cMvdInput.ConvertImagePixelFormat(MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08);
            //}

            Bitmap bmp0 = bmpInputImage.Clone(xRecipe.xRectRegionBase0, PixelFormat.Format8bppIndexed);
            Bitmap bmp1 = bmpInputImage.Clone(xRecipe.xRectRegionBase1, PixelFormat.Format8bppIndexed);

            //计算基准位置 用来检测偏移
            mVD_POINT_F0 = _getBasePointF2(bmp0, RegionName.BASE0);
            mVD_POINT_F1 = _getBasePointF2(bmp1, RegionName.BASE1);

            bmp0.Dispose();
            bmp1.Dispose();
            foreach (RegionCellX2Class cell in xRecipe.xRegionCells)
            {
                cell.Reset();

                //if (cell.ByPass)
                //    continue;

                RectangleF _rectF = new RectangleF(cell.viewRectF.X, cell.viewRectF.Y, cell.viewRectF.Width, cell.viewRectF.Height);
                _rectF.Inflate(100, 100);
                BoundRect(ref _rectF, _bmpInputSize);
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

                    cell.OrgX = cell.DrawResultRectF().CenterX - mVD_POINT_F0.fX;
                    cell.OrgY = cell.DrawResultRectF().CenterY - mVD_POINT_F0.fY;
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

        #region BACKUP
#if NOUSE_AND_BACKUP
        private void _Inspect002()
        {
            m_ElapsedTime = 0;
            m_Running = true;
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Restart();

            //计算基准位置 用来检测偏移
            mVD_POINT_F0 = _getBasePointF(cMvdInput, RegionName.BASE0);
            mVD_POINT_F1 = _getBasePointF(cMvdInput, RegionName.BASE1);

            //
            foreach (RegionCellX2Class cell in xRecipe.xRegionCells)
            {
                cell.Reset();
                RectangleF _rectF = new RectangleF(cell.viewRectF.X, cell.viewRectF.Y, cell.viewRectF.Width, cell.viewRectF.Height);
                _rectF.Inflate(100, 100);
                int iOK = xRecipe.PrintTempRun(cMvdInput, _rectF);
                if (iOK == 0)
                {
                    cell.xFindResult = xRecipe.mvdprinttemp_Find.xResults[0];
                    RectangleF templaterectf = new RectangleF(0, 0, xRecipe.bmpprinttemplate.Width, xRecipe.bmpprinttemplate.Height);
                    Rectangle runrectf = new Rectangle(0, 0, (int)cMvdInput.Width, (int)cMvdInput.Height);
                    cell.PositionFixRun(templaterectf, runrectf, cell.xFindResult);
                    //cell.DetectDefects(xRecipe.bmpprinttemplate, cMvdInput);

                }
                else
                {
                    cell.inspectReason = InspectReason.INS_ALIGNERR;
                }
            }

            stopwatch.Stop();
            m_ElapsedTime = stopwatch.ElapsedMilliseconds;
            m_Running = false;
        }

        private void _Inspect003()
        {
            xRecipe.AnalyzeDatasData();
            m_ElapsedTime = 0;
            m_Running = true;
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Restart();

            if (m_MvdOpeate == null)
                m_MvdOpeate = new CMvdImage();

            m_MvdOpeate = cMvdInput.Clone();

            //if (cMvdInput.PixelFormat != MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08)
            //{
            //    //当前程序仅支持mono8。因此像素格会转换.
            //    cMvdInput.ConvertImagePixelFormat(MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08);
            //}

            //计算基准位置 用来检测偏移
            mVD_POINT_F0 = _getBasePointF(m_MvdOpeate, RegionName.BASE0);
            mVD_POINT_F1 = _getBasePointF(m_MvdOpeate, RegionName.BASE1);

            xRecipe.mvdprinttemp_Find.xMvdRun_Image = m_MvdOpeate.Clone();
            //xRecipe.mvdprinttemp_Find.HikRun4Pre();
            foreach (RegionCellX2Class cell in xRecipe.xRegionCells)
            {
                cell.Reset();

                if (cell.ByPass)
                    continue;

                RectangleF _rectF = new RectangleF(cell.viewRectF.X, cell.viewRectF.Y, cell.viewRectF.Width, cell.viewRectF.Height);
                _rectF.Inflate(100, 100);
                BoundRect(ref _rectF, new Size((int)m_MvdOpeate.Width, (int)m_MvdOpeate.Height));
                //int iOK = xRecipe.PrintTempRun(cMvdInput, _rectF);
                //xRecipe.mvdprinttemp_Find.xMvdRun_Image = cMvdInput;
                int iOK = (xRecipe.mvdprinttemp_Find.HikRun3(_rectF) ? 0 : -1);
                //int iOK = (xRecipe.mvdprinttemp_Find.HikRun4(_rectF) ? 0 : -1);
                if (iOK == 0)
                {
                    cell.xFindResult = xRecipe.mvdprinttemp_Find.xResults[0];
                    RectangleF templaterectf = new RectangleF(0, 0, xRecipe.bmpprinttemplate.Width, xRecipe.bmpprinttemplate.Height);
                    Rectangle runrectf = new Rectangle(0, 0, (int)m_MvdOpeate.Width, (int)m_MvdOpeate.Height);
                    cell.PositionFixRun(templaterectf, runrectf, cell.xFindResult);
                }
                else
                {
                    cell.inspectReason = InspectReason.INS_ALIGNERR;
                    cell.inspectReasons.Add(InspectReason.INS_ALIGNERR);
                }

                //xRecipe.mvd2DReader.Run(cMvdInput, cell.DrawResultRectF());
                //if (xRecipe.mvd2DReader.DCodeInfo != null)
                //{
                //    cell.RunCodeInfo = xRecipe.mvd2DReader.DCodeInfo;
                //    if (cell.DrawBarcodePosition == null)
                //        cell.DrawBarcodePosition = new CMvdPolygonF();

                //    cell.DrawBarcodePosition.BorderColor = new MVD_COLOR(0, 255, 0);
                //    cell.DrawBarcodePosition.AddVertex(cell.RunCodeInfo.Position[0].nX, cell.RunCodeInfo.Position[0].nY);
                //    cell.DrawBarcodePosition.AddVertex(cell.RunCodeInfo.Position[1].nX, cell.RunCodeInfo.Position[1].nY);
                //    cell.DrawBarcodePosition.AddVertex(cell.RunCodeInfo.Position[2].nX, cell.RunCodeInfo.Position[2].nY);
                //    cell.DrawBarcodePosition.AddVertex(cell.RunCodeInfo.Position[3].nX, cell.RunCodeInfo.Position[3].nY);
                //}
                //else
                //{
                //    cell.inspectReasons.Add(InspectReason.INS_2DERR);
                //    cell.DrawBarcodePosition = null;
                //}
            }

            Bitmap bmpInputImage = CMvdImageToBitmap(m_MvdOpeate);
            foreach (RegionCellX2Class cell in xRecipe.xRegionCells)
            {
                if (cell.ByPass)
                    continue;
                if (cell.inspectReason == InspectReason.INS_ALIGNERR)
                    continue;
                cell.xInspectPara = InspectX2Class.Instance;

                //原始模板的大小
                RectangleF templaterectf = new RectangleF(0, 0, xRecipe.bmpprinttemplate.Width, xRecipe.bmpprinttemplate.Height);

                //定位完成后裁切位置
                RectangleF _crop = new RectangleF(cell.DrawResultRectF().CenterX - templaterectf.Width / 2,
                    cell.DrawResultRectF().CenterY - templaterectf.Height / 2,
                    templaterectf.Width,
                    templaterectf.Height);

                if (InspectX2Class.Instance.bCheckInspect)
                {
                    cell.bmpItemRun.Dispose();
                    cell.bmpItemRun = bmpInputImage.Clone(_crop, PixelFormat.Format8bppIndexed);
                    cell.bmpItemMask.Dispose();
                    cell.bmpItemMask = xRecipe.bmpprintmask.Clone(
                        new Rectangle(0, 0, xRecipe.bmpprintmask.Width, xRecipe.bmpprintmask.Height),
                        PixelFormat.Format8bppIndexed);
                    cell.DetectDefects(xRecipe.bmpprinttemplate, cell.bmpItemRun, cell.bmpItemMask);
                }

                if (InspectX2Class.Instance.bCheckCode)
                {
                    RectangleF _cropCode = new RectangleF(xRecipe.xRectCodeRegion.X + _crop.X,
                        xRecipe.xRectCodeRegion.Y + _crop.Y,
                        xRecipe.xRectCodeRegion.Width,
                        xRecipe.xRectCodeRegion.Height);
                    cell.bmpItemCodeRun.Dispose();
                    cell.bmpItemCodeRun = bmpInputImage.Clone(_cropCode, PixelFormat.Format8bppIndexed);
                    cell.DeCode2D(cell.bmpItemCodeRun, _cropCode.Location);
                    //cell.DeCode2D(cell.bmpItemCodeRun, _crop.Location, xRecipe.xRectCodeRegion.Location);
                }
            }
            bmpInputImage.Dispose();

            //检测重复码
            if (InspectX2Class.Instance.bCheckRepeatCode)
            {
                //判断表是否存在
                bool bExist = JzCheckRepeatClass.Instance.MySqlCheckTableExist();
                if (!bExist)
                {
                    int iret = JzCheckRepeatClass.Instance.MySqlCreateTable();
                    if (iret >= 0)
                    {
                        bool bChk = xRecipe.RunRepeatCode();

                    }
                }
                else
                {
                    xRecipe.RunRepeatCode();
                }
            }

            m_IsPass = xRecipe.AnalyzeDatasRun();


            if (m_MvdOpeate != null)
                m_MvdOpeate.Dispose();

            stopwatch.Stop();
            m_ElapsedTime = stopwatch.ElapsedMilliseconds;
            m_Running = false;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

#endif
        #endregion

        MVD_POINT_F _getBasePointF(CMvdImage eImage, RegionName regionName)
        {
            MVD_POINT_F mVD_POINT_F = new MVD_POINT_F(-9999, -9999);

            ////匹配基准点
            //CImageRegionCopyTool _ToolObj0 = new VisionDesigner.ImageRegionCopy.CImageRegionCopyTool();
            //_ToolObj0.InputImage = eImage;
            //// Set ROI region (optional)
            //_ToolObj0.ROI = null;// new VisionDesigner.CMvdRectangleF(eImage.Width / 2, eImage.Height / 2, eImage.Width / 4, eImage.Height / 4);
            //switch (regionName)
            //{
            //    case RegionName.BASE0:
            //        _ToolObj0.ROI = new CMvdRectangleF(xRecipe.xRectRegionBase0.X + xRecipe.xRectRegionBase0.Width / 2,
            //                                                                        xRecipe.xRectRegionBase0.Y + xRecipe.xRectRegionBase0.Height / 2,
            //                                                                        xRecipe.xRectRegionBase0.Width,
            //                                                                        xRecipe.xRectRegionBase0.Height);
            //        break;
            //    case RegionName.BASE1:
            //        _ToolObj0.ROI = new CMvdRectangleF(xRecipe.xRectRegionBase1.X + xRecipe.xRectRegionBase1.Width / 2,
            //                                                                        xRecipe.xRectRegionBase1.Y + xRecipe.xRectRegionBase1.Height / 2,
            //                                                                        xRecipe.xRectRegionBase1.Width,
            //                                                                        xRecipe.xRectRegionBase1.Height);
            //        break;
            //}
            //// Running
            //_ToolObj0.Run();

            //// Get the result
            //var _Result = _ToolObj0.Result;
            //VisionDesigner.CMvdImage OutputImage = _Result.OutputImage;
            //int iOK = 0;
            //switch (regionName)
            //{
            //    case RegionName.BASE0:
            //        iOK = xRecipe.Base0Run(_ToolObj0.Result.OutputImage);
            //        if (iOK == 0)
            //        {
            //            mVD_POINT_F.fX = xRecipe.mvdbase0_Find.xResults[0].fCenterX + _ToolObj0.ROI.GetBoundingRect().fX;
            //            mVD_POINT_F.fY = xRecipe.mvdbase0_Find.xResults[0].fCenterY + _ToolObj0.ROI.GetBoundingRect().fY;
            //        }
            //        break;
            //    case RegionName.BASE1:
            //        iOK = xRecipe.Base1Run(_ToolObj0.Result.OutputImage);
            //        if (iOK == 0)
            //        {
            //            mVD_POINT_F.fX = xRecipe.mvdbase1_Find.xResults[0].fCenterX + _ToolObj0.ROI.GetBoundingRect().fX;
            //            mVD_POINT_F.fY = xRecipe.mvdbase1_Find.xResults[0].fCenterY + _ToolObj0.ROI.GetBoundingRect().fY;
            //        }
            //        break;
            //}

            //if (null != _ToolObj0)
            //{
            //    _ToolObj0.Dispose();
            //    _ToolObj0 = null;
            //}

            int iOK = 0;
            switch (regionName)
            {
                case RegionName.BASE0:
                    iOK = xRecipe.Base0Run(eImage, xRecipe.xRectRegionBase0);
                    if (iOK == 0)
                    {
                        mVD_POINT_F.fX = xRecipe.mvdbase0_Find.xResults[0].fCenterX;// + xRecipe.xRectRegionBase0.X;
                        mVD_POINT_F.fY = xRecipe.mvdbase0_Find.xResults[0].fCenterY;// + xRecipe.xRectRegionBase0.Y;
                    }
                    break;
                case RegionName.BASE1:
                    iOK = xRecipe.Base1Run(eImage, xRecipe.xRectRegionBase1);
                    if (iOK == 0)
                    {
                        mVD_POINT_F.fX = xRecipe.mvdbase1_Find.xResults[0].fCenterX;// + xRecipe.xRectRegionBase1.X;
                        mVD_POINT_F.fY = xRecipe.mvdbase1_Find.xResults[0].fCenterY;// + xRecipe.xRectRegionBase1.Y;
                    }
                    break;
            }

            return mVD_POINT_F;
        }
        MVD_POINT_F _getBasePointF2(Bitmap eImage, RegionName regionName)
        {
            MVD_POINT_F mVD_POINT_F = new MVD_POINT_F(-9999, -9999);

            int iOK = 0;
            switch (regionName)
            {
                case RegionName.BASE0:
                    iOK = xRecipe.Base0Run(eImage);
                    if (iOK == 0)
                    {
                        mVD_POINT_F.fX = xRecipe.mvdbase0_Find.xResults[0].fCenterX + xRecipe.xRectRegionBase0.X;
                        mVD_POINT_F.fY = xRecipe.mvdbase0_Find.xResults[0].fCenterY + xRecipe.xRectRegionBase0.Y;
                    }
                    break;
                case RegionName.BASE1:
                    iOK = xRecipe.Base1Run(eImage);
                    if (iOK == 0)
                    {
                        mVD_POINT_F.fX = xRecipe.mvdbase1_Find.xResults[0].fCenterX + xRecipe.xRectRegionBase1.X;
                        mVD_POINT_F.fY = xRecipe.mvdbase1_Find.xResults[0].fCenterY + xRecipe.xRectRegionBase1.Y;
                    }
                    break;
            }

            return mVD_POINT_F;
        }
        private Bitmap CMvdImageToBitmap(CMvdImage eCMvdImage)
        {
            MVD_IMAGE_DATA_INFO _MvdImage = eCMvdImage.GetImageData();
            MVD_DATA_CHANNEL_INFO ch0 = _MvdImage.stDataChannel[0];
            //Bitmap _bmpFromMVD = ByteArrayToBitmap(ch0.arrDataBytes, (int)ch0.nRowStep, (int)(ch0.nLen / ch0.nRowStep));
            return ByteArrayToBitmap(ch0.arrDataBytes, (int)ch0.nRowStep, (int)(ch0.nLen / ch0.nRowStep));
        }
        private Bitmap ByteArrayToBitmap(byte[] byteArray, int width, int height)
        {
            // 创建 Bitmap 对象
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            // 设置调色板为灰度
            ColorPalette palette = bitmap.Palette;
            for (int i = 0; i < 256; i++)
            {
                palette.Entries[i] = Color.FromArgb(i, i, i);
            }
            bitmap.Palette = palette;
            // 锁定 Bitmap 数据
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
            // 将字节数组复制到 Bitmap 数据中
            System.Runtime.InteropServices.Marshal.Copy(byteArray, 0, bitmapData.Scan0, byteArray.Length);
            // 解锁 Bitmap 数据
            bitmap.UnlockBits(bitmapData);
            return bitmap;
        }
        private CMvdImage BitmapToCMvdImage(Bitmap bmpInputImg)
        {
            CMvdImage cMvdImage = new CMvdImage();
            System.Drawing.Imaging.PixelFormat bitPixelFormat = bmpInputImg.PixelFormat;
            BitmapData bmData = bmpInputImg.LockBits(new Rectangle(0, 0, bmpInputImg.Width, bmpInputImg.Height), ImageLockMode.ReadOnly, bitPixelFormat);//锁定

            if (bitPixelFormat == System.Drawing.Imaging.PixelFormat.Format8bppIndexed)
            {
                Int32 bitmapDataSize = bmData.Stride * bmData.Height;//bitmap图像缓存长度
                int offset = bmData.Stride - bmData.Width;
                Int32 ImageBaseDataSize = bmData.Width * bmData.Height;//imageBaseData_V2图像真正的缓存长度
                byte[] _BitImageBufferBytes = new byte[bitmapDataSize];
                byte[] _ImageBaseDataBufferBytes = new byte[ImageBaseDataSize];
                Marshal.Copy(bmData.Scan0, _BitImageBufferBytes, 0, bitmapDataSize);
                int bitmapIndex = 0;
                int ImageBaseDataIndex = 0;
                for (int i = 0; i < bmData.Height; i++)
                {
                    for (int j = 0; j < bmData.Width; j++)
                    {
                        _ImageBaseDataBufferBytes[ImageBaseDataIndex++] = _BitImageBufferBytes[bitmapIndex++];
                    }
                    bitmapIndex += offset;
                }
                MVD_IMAGE_DATA_INFO stImageData = new MVD_IMAGE_DATA_INFO();
                stImageData.stDataChannel[0].nRowStep = (uint)bmData.Width;
                stImageData.stDataChannel[0].nLen = (uint)ImageBaseDataSize;
                stImageData.stDataChannel[0].nSize = (uint)ImageBaseDataSize;
                stImageData.stDataChannel[0].arrDataBytes = _ImageBaseDataBufferBytes;
                cMvdImage.InitImage((uint)bmData.Width, (uint)bmData.Height, MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08, stImageData);
            }
            else if (bitPixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb)
            {
                Int32 bitmapDataSize = bmData.Stride * bmData.Height;//bitmap图像缓存长度
                int offset = bmData.Stride - bmData.Width * 3;
                Int32 ImageBaseDataSize = bmData.Width * bmData.Height * 3;//imageBaseData_V2图像真正的缓存长度
                byte[] _BitImageBufferBytes = new byte[bitmapDataSize];
                byte[] _ImageBaseDataBufferBytes = new byte[ImageBaseDataSize];
                Marshal.Copy(bmData.Scan0, _BitImageBufferBytes, 0, bitmapDataSize);
                int bitmapIndex = 0;
                int ImageBaseDataIndex = 0;
                for (int i = 0; i < bmData.Height; i++)
                {
                    for (int j = 0; j < bmData.Width; j++)
                    {
                        _ImageBaseDataBufferBytes[ImageBaseDataIndex++] = _BitImageBufferBytes[bitmapIndex + 2];
                        _ImageBaseDataBufferBytes[ImageBaseDataIndex++] = _BitImageBufferBytes[bitmapIndex + 1];
                        _ImageBaseDataBufferBytes[ImageBaseDataIndex++] = _BitImageBufferBytes[bitmapIndex];
                        bitmapIndex += 3;
                    }
                    bitmapIndex += offset;
                }
                MVD_IMAGE_DATA_INFO stImageData = new MVD_IMAGE_DATA_INFO();
                stImageData.stDataChannel[0].nRowStep = (uint)bmData.Width * 3;
                stImageData.stDataChannel[0].nLen = (uint)ImageBaseDataSize;
                stImageData.stDataChannel[0].nSize = (uint)ImageBaseDataSize;
                stImageData.stDataChannel[0].arrDataBytes = _ImageBaseDataBufferBytes;
                cMvdImage.InitImage((uint)bmData.Width, (uint)bmData.Height, MVD_PIXEL_FORMAT.MVD_PIXEL_RGB_RGB24_C3, stImageData);
            }
            bmpInputImg.UnlockBits(bmData);  // 解除锁定
            return cMvdImage;
        }

        void BoundRect(ref RectangleF InnerRect, Size BoundSize)
        {
            InnerRect.X = Math.Min(Math.Max(InnerRect.X, 0), (BoundSize.Width - InnerRect.Width < 0 ? 0 : BoundSize.Width - InnerRect.Width));
            InnerRect.Y = Math.Min(Math.Max(InnerRect.Y, 0), (BoundSize.Height - InnerRect.Height < 0 ? 0 : BoundSize.Height - InnerRect.Height));

            if (BoundSize.Width <= InnerRect.X + InnerRect.Width)
                InnerRect.Width = BoundValue(InnerRect.Width, BoundSize.Width - InnerRect.X, 1);
            if (BoundSize.Height <= InnerRect.Height + InnerRect.Height)
                InnerRect.Height = BoundValue(InnerRect.Height, BoundSize.Height - InnerRect.Y, 1);
        }
        float BoundValue(float Value, float Max, float Min)
        {
            return Math.Max(Math.Min(Value, Max), Min);

        }

    }
}
