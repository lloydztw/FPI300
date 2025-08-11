using AUVision;
using LaserAlignDX.BasicSpace;
using LaserAlignDX.OPSpace.RecipeSpace;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Traveller106;
using VisionDesigner.Code2DReader;
using VisionDesigner.ImageArithmetic;
using VisionDesigner.PositionFix;
using VisionDesigner;
using VisionDesigner.BlobFind;
using LaserAlignDX.GA.BasicSpace;

namespace LaserAlignDX.OPSpace
{
    public class RegionCellX3Class : IDisposable
    {

        VisionDesigner.PositionFix.CPositionFixTool cPositionFixToolObj = null;// new VisionDesigner.PositionFix.CPositionFixTool();
        //CImageRegionCopyTool copyToolObj = new VisionDesigner.ImageRegionCopy.CImageRegionCopyTool();
        CImageArithmeticTool cImageArithmeticToolObj = null;// new CImageArithmeticTool();
        VisionDesigner.ImageBinary.CImageBinaryTool cImageBinaryToolObj = null;// new VisionDesigner.ImageBinary.CImageBinaryTool();
        VisionDesigner.ImageMorph.CImageMorphTool cImageMorphToolObj = null;// new VisionDesigner.ImageMorph.CImageMorphTool();
        VisionDesigner.BlobFind.CBlobFindTool cBlobFindToolObj = null;// new VisionDesigner.BlobFind.CBlobFindTool();
        //C2DCodeReaderTool Code2DReaderTool = new C2DCodeReaderTool();
        //C2DCodeVerifyTool c2DCodeVerifyTool = new C2DCodeVerifyTool();
        Mvd2DReaderClass mvd2DReader = null;// new Mvd2DReaderClass();
        MvdFindLineClass mvdFindLineClass = null;

        private CMvdRectangleF MvdRunPositionFix;

        public RegionCellX3Class()
        {

        }
        ~RegionCellX3Class()
        {
            Dispose();
        }

        const string m_Format = "0.000";

        public int Index = 0;
        public string Name = "";
        public string Result = "";
        public string lblName = "";
        public int CellRow = 0;
        public int CellCol = 0;
        public RectangleF viewRectF = new RectangleF();

        public float OrgX = 0;
        public float OrgY = 0;
        public float OrgAngle = 0;
        public float RunX = 0;
        public float RunY = 0;
        public float RunAngle = 0;

        public float RunWidth = 0;
        public float RunHeight = 0;

        public bool ByPass = false;
        public string SetBarcodeStr = string.Empty;

        public bool IsSaveDebugPicture = false;
        public string SaveDebugPath = $"D:\\log\\DebugImage";

        //public InspectX3ParaClass xInspectPara = new InspectX3ParaClass();
        public NoTrayParaClass xNoTrayPara
        {
            get { return NoTrayParaClass.Instance; }
        }
        public InspectX3ParaClass xInspect
        {
            get { return InspectX3ParaClass.Instance; }
        }

        public Bitmap bmpItemTemplate = new Bitmap(1, 1);
        public Bitmap bmpItemRun = new Bitmap(1, 1);
        public Bitmap bmpItemMask = new Bitmap(1, 1);
        public Bitmap bmpItemCodeRun = new Bitmap(1, 1);

        //public int BlobLowThreshold = 128;
        //public int BlobHighThreshold = 255;

        public InspectReason inspectReason = InspectReason.PASS;
        public AUVision.xFindResult xFindResult = new AUVision.xFindResult();
        public List<InspectReason> inspectReasons = new List<InspectReason>();

        /// <summary>
        /// 外围的直线
        /// </summary>
        public CMvdLineSegmentF[] cMvdLineSegmentFsOut = new CMvdLineSegmentF[4];
        public CMvdShape[] cMvdShapesForFindLineRegion = new CMvdShape[4];
        /// <summary>
        /// 寻找直线
        /// </summary>
        /// <param name="iSideIndex">哪条边序号</param>
        /// <param name="bmp">输入图片</param>
        /// <param name="r">寻找的ROI</param>
        public void LineSegmentRun(int iSideIndex, Bitmap bmp, CMvdRectangleF r)
        {
            if (mvdFindLineClass == null)
                mvdFindLineClass = new MvdFindLineClass();
            cMvdLineSegmentFsOut[iSideIndex] = null;
            if (iSideIndex == 0)
            {
                mvdFindLineClass.bPositive = xInspect.bPositive0;
                mvdFindLineClass.bFindOrient = true;
                mvdFindLineClass.bEdgePolarity = xInspect.bEdgePolarity0;
            }
            else if (iSideIndex == 1)
            {
                mvdFindLineClass.bPositive = xInspect.bPositive1;
                mvdFindLineClass.bFindOrient = false;
                mvdFindLineClass.bEdgePolarity = xInspect.bEdgePolarity1;
            }
            else if (iSideIndex == 2)
            {
                mvdFindLineClass.bPositive = xInspect.bPositive2;
                mvdFindLineClass.bFindOrient = true;
                mvdFindLineClass.bEdgePolarity = xInspect.bEdgePolarity2;
            }
            else if (iSideIndex == 3)
            {
                mvdFindLineClass.bPositive = xInspect.bPositive3;
                mvdFindLineClass.bFindOrient = false;
                mvdFindLineClass.bEdgePolarity = xInspect.bEdgePolarity3;
            }
            cMvdLineSegmentFsOut[iSideIndex] = mvdFindLineClass.Run(bmp, r);
        }

        public C2DCodeInfo RunCodeInfo = null;
        public CMvdPolygonF DrawBarcodePosition = null;
        //public CMvdRectangleF GetMvdFix()
        //{
        //    return MvdRunPositionFix;
        //}
        public CMvdRectangleF DrawResultRectF()
        {
            if (MvdRunPositionFix == null)
            {
                MvdRunPositionFix = new CMvdRectangleF(viewRectF.X + viewRectF.Width / 2, viewRectF.Y + viewRectF.Height / 2, viewRectF.Width,
                        viewRectF.Height);
                MvdRunPositionFix.BorderColor = new MVD_COLOR(255, 0, 0);
            }

            switch (inspectReason)
            {
                case InspectReason.INS_ALIGNERR:
                    MvdRunPositionFix = new CMvdRectangleF(viewRectF.X + viewRectF.Width / 2, viewRectF.Y + viewRectF.Height / 2, viewRectF.Width,
                        viewRectF.Height);
                    MvdRunPositionFix.BorderColor = new MVD_COLOR(255, 0, 0);
                    break;
                case InspectReason.PASS:
                    if (inspectReasons.Count > 0)
                        MvdRunPositionFix.BorderColor = new MVD_COLOR(255, 0, 0);
                    else
                        MvdRunPositionFix.BorderColor = new MVD_COLOR(0, 255, 0);
                    break;
                default:
                    MvdRunPositionFix.BorderColor = new MVD_COLOR(255, 0, 0);
                    break;
            }

            //if (ByPass && !INI.Instance.IsForceInspect)
            //{
            //    MvdRunPositionFix = new CMvdRectangleF(viewRectF.X + viewRectF.Width / 2, viewRectF.Y + viewRectF.Height / 2, viewRectF.Width,
            //            viewRectF.Height);
            //    MvdRunPositionFix.BorderColor = new MVD_COLOR(255, 0, 0);
            //}
            return MvdRunPositionFix;
        }
        /// <summary>
        /// 画出有无料的位置框
        /// </summary>
        /// <param name="bNoTray">无料true 疑似有料false</param>
        /// <returns></returns>
        public CMvdRectangleF DrawBaseRectFFixSize(bool bNoTray = true, float fFixSize = 200)
        {
            CMvdRectangleF noTrayRectF = new CMvdRectangleF(viewRectF.X + viewRectF.Width / 2,
                viewRectF.Y + viewRectF.Height / 2,
                fFixSize,
                fFixSize);
            if (bNoTray)
                noTrayRectF.BorderColor = new MVD_COLOR(0, 0, 255);
            else
                noTrayRectF.BorderColor = new MVD_COLOR(255, 0, 0);

            return noTrayRectF;
        }
        public bool GetOffsetResult()
        {
            bool bOK = true;

            //if (xInspectPara.bCheckOffset)
            //    return bOK;

            //if (Math.Abs(RunX) > xInspectPara.xOffsetX)
            //{
            //    bOK = false;
            //    inspectReasons.Add(InspectReason.INS_OFFSETERR);
            //}
            //else if (Math.Abs(RunY) > xInspectPara.xOffsetY)
            //{
            //    bOK = false;
            //    inspectReasons.Add(InspectReason.INS_OFFSETERR);
            //}
            return bOK;
        }
        public string GetNoTrayDesc()
        {
            string str = string.Empty;
            if (inspectReason != InspectReason.INS_ALIGNERR)
            {
                str = "疑似有料";
                return str;
            }
            foreach (var reason in inspectReasons)
            {
                if (reason != InspectReason.INS_ALIGNERR)
                {
                    str = "疑似有料";
                    //str = "Maybe.P";
                    break;
                }
            }

            return str;
        }
        private List<CMvdRectangleF> blobMvdRectFNGList = new List<CMvdRectangleF>();
        public List<CMvdRectangleF> DrawBlobNGList()
        {
            List<CMvdRectangleF> cMvdRectangleFs = new List<CMvdRectangleF>();

            //foreach (RectangleF rectangleF in xInspectPara.rectangles)
            //{
            //    bool bFound = false;
            //    foreach (CMvdRectangleF mvdRectangleF in blobMvdRectFNGList)
            //    {
            //        //mvdRectangleF.CenterX += MvdRunPositionFix.CenterX - bmpItemRun.Width / 2;
            //        //mvdRectangleF.CenterY += MvdRunPositionFix.CenterY - bmpItemRun.Height / 2;
            //        MVD_RECT_F mVD_RECT_F = mvdRectangleF.GetBoundingRect();
            //        RectangleF rectangleF1 = new RectangleF(mVD_RECT_F.fX, mVD_RECT_F.fY, mVD_RECT_F.fWidth, mVD_RECT_F.fHeight);
            //        if (rectangleF.IntersectsWith(rectangleF1))
            //        {
            //            bFound = true;
            //            break;
            //        }
            //    }

            //    if (bFound)
            //    {
            //        CMvdRectangleF cMvdRectangleF = new CMvdRectangleF(rectangleF.X + rectangleF.Width / 2,
            //            rectangleF.Y + rectangleF.Height / 2, rectangleF.Width, rectangleF.Height);

            //        cMvdRectangleF.CenterX += MvdRunPositionFix.CenterX - bmpItemRun.Width / 2;
            //        cMvdRectangleF.CenterY += MvdRunPositionFix.CenterY - bmpItemRun.Height / 2;
            //        cMvdRectangleF.BorderColor = new MVD_COLOR(255, 0, 0);
            //        cMvdRectangleF.BorderWidth = 1;
            //        cMvdRectangleFs.Add(cMvdRectangleF);
            //        //break;
            //    }
            //}

            foreach (CMvdRectangleF mvdRectangleF in blobMvdRectFNGList)
            {
                mvdRectangleF.CenterX += MvdRunPositionFix.CenterX - bmpItemRun.Width / 2;
                mvdRectangleF.CenterY += MvdRunPositionFix.CenterY - bmpItemRun.Height / 2;
                mvdRectangleF.BorderColor = new MVD_COLOR(255, 0, 0);
                mvdRectangleF.BorderWidth = 1;
                cMvdRectangleFs.Add(mvdRectangleF);
            }
            return cMvdRectangleFs;
        }

        public string ToResultStr()
        {
            string str = string.Empty;

            str += $"{Index}" + ",";
            str += $"{lblName}" + ",";
            str += $"{(ByPass ? (INI.Instance.IsForceInspect ? "1强制检测" : "0不检测") : "1检测")}" + ",";
            str += $"{RunX.ToString(m_Format)}" + ",";
            str += $"{RunY.ToString(m_Format)}" + ",";
            str += $"{RunAngle.ToString(m_Format)}" + ",";
            if (xInspect.bOpenLineMeasure)
            {
                str += $"长[{RunWidth.ToString(m_Format)}]" + ",";
                str += $"宽[{RunHeight.ToString(m_Format)}]" + ",";
            }
            str += $"{SetBarcodeStr}" + ",";
            if (RunCodeInfo != null)
                str += $"{RunCodeInfo.Content}" + ";";
            else
                str += $"" + ";";

            //str += $"{Index}" + ",";
            //str += $"{lblName}" + ",";
            //str += $"{(ByPass ? "0不检测" : "1检测")}" + ",";
            //str += $"偏移x:{RunX}" + ",";
            //str += $"偏移y:{RunY}" + ",";
            //str += $"设定{SetBarcodeStr}" + ",";
            //if (RunCodeInfo != null)
            //    str += $"读取{RunCodeInfo.Content}" + ";";
            //else
            //    str += $"读取" + ";";

            //if (mvd2DReader.DCodeInfo != null)
            //    str += $"{mvd2DReader.DCodeInfo.Content}" + ";";
            //else
            //    str += $"" + ";";

            return str;
        }
        public string ToShowMainStr()
        {
            string str = string.Empty;

            str += $"{Index}" + "-[";
            str += $"{RunX.ToString(m_Format)}" + ",";
            str += $"{RunY.ToString(m_Format)}" + ",";
            str += $"{RunAngle.ToString(m_Format)}]{Environment.NewLine}";
            if (xInspect.bOpenLineMeasure)
            {
                str += $"长[{RunWidth.ToString(m_Format)},";
                str += $"宽{RunHeight.ToString(m_Format)}]";
            }

            //str += Environment.NewLine;

            //str += $"ORG" + "-[";
            //str += $"{OrgX.ToString(m_Format)}" + ",";
            //str += $"{OrgY.ToString(m_Format)}" + ",";
            //str += $"{OrgAngle.ToString(m_Format)}]";


            return str;
        }
        public void Reset()
        {
            inspectReason = InspectReason.PASS;
            xFindResult = new AUVision.xFindResult();
            inspectReasons.Clear();
            RunCodeInfo = null;
            if (mvd2DReader != null)
                mvd2DReader.DCodeInfo = null;
            DrawBarcodePosition = null;
            RunX = 0;
            RunY = 0;
            IsSaveDebugPicture = false;

            int i = 0;
            while (i < 4)
            {
                //CMvdLineSegmentF mLine = cMvdLineSegmentFsOut[i];
                if (cMvdLineSegmentFsOut[i] != null)
                    cMvdLineSegmentFsOut[i] = null;
                //CMvdShape mvdShape = cMvdShapesForFindLineRegion[i];
                if (cMvdShapesForFindLineRegion[i] != null)
                    cMvdShapesForFindLineRegion[i] = null;
                i++;
            }

            RunWidth = 0;
            RunHeight = 0;
        }
        /// <summary>
        /// 计算修正后的位置框
        /// </summary>
        /// <param name="templateRectF">模板尺寸</param>
        /// <param name="runRect">输入图片尺寸</param>
        /// <param name="templateRunResult">定位的结果</param>
        public void PositionFixRun(RectangleF templateRectF, Rectangle runRect, xFindResult templateRunResult)
        {
            // CreateInstance
            if (cPositionFixToolObj == null)
                cPositionFixToolObj = new CPositionFixTool();

            // Set basic parameter

            cPositionFixToolObj.BasicParam.BasePoint
                = new VisionDesigner.PositionFix.MVD_FIDUCIAL_POINT_F(
                    new MVD_POINT_F(templateRectF.Width / 2, templateRectF.Height / 2), 0);

            cPositionFixToolObj.BasicParam.RunningPoint
                = new VisionDesigner.PositionFix.MVD_FIDUCIAL_POINT_F(
                    new MVD_POINT_F(templateRunResult.fCenterX, templateRunResult.fCenterY), templateRunResult.fAngle);

            cPositionFixToolObj.BasicParam.RunImageSize = new MVD_SIZE_I(runRect.Width, runRect.Height);

            cPositionFixToolObj.BasicParam.FixMode = MVD_POSFIX_MODE.MVD_POSFIX_MODE_HVA;

            var RectangleShape
                = new CMvdRectangleF(templateRectF.Width / 2, templateRectF.Height / 2, templateRectF.Width, templateRectF.Height);

            cPositionFixToolObj.BasicParam.InitialShape = RectangleShape;

            // Running

            cPositionFixToolObj.Run();

            // Get the result

            MvdRunPositionFix = cPositionFixToolObj.Result.CorrectedShape as CMvdRectangleF;

        }

        /// <summary>
        /// 计算修正后的位置框
        /// </summary>
        /// <param name="eMVDInput">输入转换的形状</param>
        /// <param name="templateRectF">模板尺寸</param>
        /// <param name="runRect">输入图片尺寸</param>
        /// <param name="templateRunResult">定位的结果</param>
        /// <returns>返回位置的形状</returns>
        public CMvdShape PositionFixRun(CMvdShape eMVDInput, RectangleF templateRectF, Rectangle runRect, xFindResult templateRunResult)
        {
            // CreateInstance
            if (cPositionFixToolObj == null)
                cPositionFixToolObj = new CPositionFixTool();

            // Set basic parameter

            cPositionFixToolObj.BasicParam.BasePoint
                = new VisionDesigner.PositionFix.MVD_FIDUCIAL_POINT_F(
                    new MVD_POINT_F(templateRectF.X + templateRectF.Width / 2, templateRectF.Y + templateRectF.Height / 2), 0);

            cPositionFixToolObj.BasicParam.RunningPoint
                = new VisionDesigner.PositionFix.MVD_FIDUCIAL_POINT_F(
                    new MVD_POINT_F(templateRunResult.fCenterX, templateRunResult.fCenterY), templateRunResult.fAngle);

            cPositionFixToolObj.BasicParam.RunImageSize = new MVD_SIZE_I(runRect.Width, runRect.Height);

            cPositionFixToolObj.BasicParam.FixMode = MVD_POSFIX_MODE.MVD_POSFIX_MODE_HVA;

            cPositionFixToolObj.BasicParam.InitialShape = eMVDInput;

            // Running

            cPositionFixToolObj.Run();

            // Get the result
            return cPositionFixToolObj.Result.CorrectedShape;
        }

        public CBlobInfo CheckBlobNoTray(Bitmap eBmpRun)
        {
            CBlobInfo cBlobInfo = null;
            //string _path = $"D:\\LOA\\{DateTime.Now.ToString("yyyyMMddHH")}";
            if (IsSaveDebugPicture)
            {
                if (!System.IO.Directory.Exists(SaveDebugPath + "\\NoTray"))
                    System.IO.Directory.CreateDirectory(SaveDebugPath + "\\NoTray");
            }

            if (cImageBinaryToolObj == null)
                cImageBinaryToolObj = new VisionDesigner.ImageBinary.CImageBinaryTool();
            if (cBlobFindToolObj == null)
                cBlobFindToolObj = new VisionDesigner.BlobFind.CBlobFindTool();

            //二值化
            cImageBinaryToolObj.InputImage = BitmapToCMvdImage(eBmpRun);
            cImageBinaryToolObj.ROI = null;
            //= new CMvdRectangleF(OutputImage.Width / 2, OutputImage.Height / 2, OutputImage.Width / 4, OutputImage.Height / 4);
            cImageBinaryToolObj.SetRunParam("LowThreshold", xNoTrayPara.xThresholdValue.ToString());
            //cImageArithmeticToolObj.SetRunParam("HighThreshold", BlobHighThreshold.ToString());
            cImageBinaryToolObj.Run();
            //cImageBinaryToolObj.Result.OutputImage.SaveImage($"{_path}\\{lblName}_Diff2.bmp", MVD_FILE_FORMAT.MVD_FILE_BMP);

            ////形态学
            //cImageMorphToolObj.InputImage = cImageBinaryToolObj.Result.OutputImage;
            //cImageMorphToolObj.SetRunParam("Type", "Open");
            //cImageMorphToolObj.ROI = _roi;
            ////= new VisionDesigner.CMvdRectangleF(cInputImg.Width / 2, cInputImg.Height / 2, cInputImg.Width / 4, cInputImg.Height / 4);
            //cImageMorphToolObj.Run();

            //blob
            cBlobFindToolObj.InputImage = cImageBinaryToolObj.Result.OutputImage;
            //cBlobFindToolObj.RegionImage = BitmapToCMvdImage(eBmpMask);
            cBlobFindToolObj.ROI = null;// _roi;
            //= new CMvdRectangleF(OutputImage.Width / 2, OutputImage.Height / 2, OutputImage.Width, OutputImage.Height);
            if (xNoTrayPara.xBlobMode == Eazy_Project_III.BlobMode.White)
                cBlobFindToolObj.SetRunParam("Polarity", "BrightObject");
            else
                cBlobFindToolObj.SetRunParam("Polarity", "DarkObject");

            cBlobFindToolObj.BasicParam.ShowBlobImageStatus = true;
            cBlobFindToolObj.Run();
            VisionDesigner.BlobFind.CBlobFindResult cBlobFindRes = cBlobFindToolObj.Result;

            //cBlobFindToolObj.RegionImage.SaveImage($"{_path}\\{lblName}_Diff2_1.bmp", MVD_FILE_FORMAT.MVD_FILE_BMP);

            //if (cBlobFindRes.BlobImage != null)
            //    cBlobFindRes.BlobImage.SaveImage($"{_path}\\{lblName}_Diff3.bmp", MVD_FILE_FORMAT.MVD_FILE_BMP);

            //Console.WriteLine("Blob Num: {0}", cBlobFindRes.BlobInfo.Count);
            //bool bOK = true;

            float _maxArea = -100000;
            int i = 0;
            int j = 0;
            blobMvdRectFNGList.Clear();
            foreach (var item in cBlobFindToolObj.Result.BlobInfo)
            {
                if (item.AreaF > _maxArea)
                {
                    _maxArea = item.AreaF;
                    j = i;
                }
                i++;
            }

            if (cBlobFindToolObj.Result.BlobInfo.Count > 0)
            {
                cBlobInfo = cBlobFindToolObj.Result.BlobInfo[j];
                if (cBlobInfo.AreaF < xNoTrayPara.xBlobAreaMin || cBlobInfo.AreaF > xNoTrayPara.xBlobAreaMax)
                {
                    inspectReason = InspectReason.INS_DEFECTERR;
                    inspectReasons.Add(InspectReason.INS_DEFECTERR);
                    if (IsSaveDebugPicture)
                    {
                        cImageBinaryToolObj.Result.OutputImage.SaveImage($"{SaveDebugPath}\\NoTray\\{lblName}_Diff2.bmp", MVD_FILE_FORMAT.MVD_FILE_BMP);
                        cBlobFindToolObj?.RegionImage?.SaveImage($"{SaveDebugPath}\\NoTray\\{lblName}_Diff2_1.bmp", MVD_FILE_FORMAT.MVD_FILE_BMP);
                        cBlobFindRes?.BlobImage?.SaveImage($"{SaveDebugPath}\\NoTray\\{lblName}_Diff3.bmp", MVD_FILE_FORMAT.MVD_FILE_BMP);
                    }
                }
            }
            return cBlobInfo;
        }
        public void DetectDefects(Bitmap eTemplate, Bitmap eBmpRun, Bitmap eBmpMask)
        {
            //string _path = $"D:\\LOA\\{DateTime.Now.ToString("yyyyMMddHH")}";
            if (IsSaveDebugPicture)
            {
                if (!System.IO.Directory.Exists(SaveDebugPath + "\\Detect"))
                    System.IO.Directory.CreateDirectory(SaveDebugPath + "\\Detect");
            }

            CMvdRectangleF _roi = new CMvdRectangleF(eTemplate.Width / 2, eTemplate.Height / 2, eTemplate.Width, eTemplate.Height);

            if (cImageArithmeticToolObj == null)
                cImageArithmeticToolObj = new CImageArithmeticTool();
            if (cImageBinaryToolObj == null)
                cImageBinaryToolObj = new VisionDesigner.ImageBinary.CImageBinaryTool();
            if (cImageMorphToolObj == null)
                cImageMorphToolObj = new VisionDesigner.ImageMorph.CImageMorphTool();
            if (cBlobFindToolObj == null)
                cBlobFindToolObj = new VisionDesigner.BlobFind.CBlobFindTool();

            cImageArithmeticToolObj.InputImage1 = BitmapToCMvdImage(eTemplate);
            cImageArithmeticToolObj.InputImage2 = BitmapToCMvdImage(eBmpRun);
            cImageArithmeticToolObj.SetRunParam("ArithmeticType", "Subtract");

            cImageArithmeticToolObj.ROI = _roi;
            //= new VisionDesigner.CMvdRectangleF(eTemplate.Width / 2, eTemplate.Height / 2, eTemplate.Width, eTemplate.Height);
            cImageArithmeticToolObj.Run();
            VisionDesigner.CMvdImage OutputImage = cImageArithmeticToolObj.Result.OutputImage;

            //OutputImage.SaveImage($"{_path}\\{lblName}_Diff.bmp", MVD_FILE_FORMAT.MVD_FILE_BMP);

            //二值化
            cImageBinaryToolObj.InputImage = OutputImage;
            cImageBinaryToolObj.ROI = null;
            //= new CMvdRectangleF(OutputImage.Width / 2, OutputImage.Height / 2, OutputImage.Width / 4, OutputImage.Height / 4);
            cImageBinaryToolObj.SetRunParam("LowThreshold", xInspect.xThresholdValue.ToString());
            //cImageArithmeticToolObj.SetRunParam("HighThreshold", BlobHighThreshold.ToString());
            cImageBinaryToolObj.Run();
            //cImageBinaryToolObj.Result.OutputImage.SaveImage($"{_path}\\{lblName}_Diff2.bmp", MVD_FILE_FORMAT.MVD_FILE_BMP);

            ////形态学
            //cImageMorphToolObj.InputImage = cImageBinaryToolObj.Result.OutputImage;
            //cImageMorphToolObj.SetRunParam("Type", "Open");
            //cImageMorphToolObj.ROI = _roi;
            ////= new VisionDesigner.CMvdRectangleF(cInputImg.Width / 2, cInputImg.Height / 2, cInputImg.Width / 4, cInputImg.Height / 4);
            //cImageMorphToolObj.Run();

            //blob
            cBlobFindToolObj.InputImage = cImageBinaryToolObj.Result.OutputImage;
            cBlobFindToolObj.RegionImage = BitmapToCMvdImage(eBmpMask);
            cBlobFindToolObj.ROI = _roi;
            //= new CMvdRectangleF(OutputImage.Width / 2, OutputImage.Height / 2, OutputImage.Width, OutputImage.Height);
            cBlobFindToolObj.SetRunParam("Polarity", "BrightObject");

            cBlobFindToolObj.BasicParam.ShowBlobImageStatus = true;
            cBlobFindToolObj.Run();
            VisionDesigner.BlobFind.CBlobFindResult cBlobFindRes = cBlobFindToolObj.Result;

            //cBlobFindToolObj.RegionImage.SaveImage($"{_path}\\{lblName}_Diff2_1.bmp", MVD_FILE_FORMAT.MVD_FILE_BMP);

            //if (cBlobFindRes.BlobImage != null)
            //    cBlobFindRes.BlobImage.SaveImage($"{_path}\\{lblName}_Diff3.bmp", MVD_FILE_FORMAT.MVD_FILE_BMP);

            //Console.WriteLine("Blob Num: {0}", cBlobFindRes.BlobInfo.Count);
            //bool bOK = true;

            blobMvdRectFNGList.Clear();
            foreach (var item in cBlobFindToolObj.Result.BlobInfo)
            {
                //Console.WriteLine("Index: {0}, Angle {1}", item.DomainIndex, item.BoxInfo.Angle);
                if (item.AreaF > xInspect.xCharArea)
                {
                    blobMvdRectFNGList.Add(item.BoxInfo);
                }
                else if (item.LongAxis > xInspect.xCharWidth && item.ShortAxis > xInspect.xCharHeight)
                {
                    blobMvdRectFNGList.Add(item.BoxInfo);
                }
                //else if (item.LongAxis > xInspectPara.xCharWidth)
                //{
                //    blobMvdRectFNGList.Add(item.BoxInfo);
                //}
                //else if (item.ShortAxis > xInspectPara.xCharHeight)
                //{
                //    blobMvdRectFNGList.Add(item.BoxInfo);
                //}
            }

            if (blobMvdRectFNGList.Count > 0)
            {
                inspectReasons.Add(InspectReason.INS_DEFECTERR);
                if (IsSaveDebugPicture)
                {
                    cImageBinaryToolObj?.Result?.OutputImage?.SaveImage($"{SaveDebugPath}\\Detect\\{lblName}_Diff2.bmp", MVD_FILE_FORMAT.MVD_FILE_BMP);
                    cBlobFindToolObj?.RegionImage?.SaveImage($"{SaveDebugPath}\\Detect\\{lblName}_Diff2_1.bmp", MVD_FILE_FORMAT.MVD_FILE_BMP);
                    //if (cBlobFindRes.BlobImage != null)
                    cBlobFindRes?.BlobImage?.SaveImage($"{SaveDebugPath}\\Detect\\{lblName}_Diff3.bmp", MVD_FILE_FORMAT.MVD_FILE_BMP);
                }
            }

        }
        public void DeCode2D(Bitmap eBmpRun, PointF Poi_CodeBase, bool eJudged = false)
        {
            if (IsSaveDebugPicture)
            {
                if (!System.IO.Directory.Exists(SaveDebugPath + "\\Code"))
                    System.IO.Directory.CreateDirectory(SaveDebugPath + "\\Code");
            }

            if (mvd2DReader == null)
                mvd2DReader = new Mvd2DReaderClass();

            mvd2DReader.Run(eBmpRun, new RectangleF(0, 0, eBmpRun.Width, eBmpRun.Height));
            if (mvd2DReader.DCodeInfo != null)
            {
                RunCodeInfo = mvd2DReader.DCodeInfo;
                if (DrawBarcodePosition == null)
                    DrawBarcodePosition = new CMvdPolygonF();

                PointF ptBase = new PointF(Poi_CodeBase.X, Poi_CodeBase.Y);
                //PointF ptBase = new PointF(Poi_LaserBase.X + Poi_CodeBase.X, Poi_LaserBase.Y + Poi_CodeBase.Y);

                DrawBarcodePosition.BorderColor = new MVD_COLOR(0, 255, 0);
                DrawBarcodePosition.AddVertex(ptBase.X + RunCodeInfo.Position[0].nX, ptBase.Y + RunCodeInfo.Position[0].nY);
                DrawBarcodePosition.AddVertex(ptBase.X + RunCodeInfo.Position[1].nX, ptBase.Y + RunCodeInfo.Position[1].nY);
                DrawBarcodePosition.AddVertex(ptBase.X + RunCodeInfo.Position[2].nX, ptBase.Y + RunCodeInfo.Position[2].nY);
                DrawBarcodePosition.AddVertex(ptBase.X + RunCodeInfo.Position[3].nX, ptBase.Y + RunCodeInfo.Position[3].nY);

                //if (xInspectPara.bCheckMappingCode)
                if (eJudged)
                {
                    if (mvd2DReader.DCodeInfo.Content != SetBarcodeStr)
                    {
                        DrawBarcodePosition.BorderColor = new MVD_COLOR(255, 0, 0);
                        inspectReasons.Add(InspectReason.INS_2DMAPNG);
                    }
                }
            }
            else
            {
                inspectReasons.Add(InspectReason.INS_2DERR);
                DrawBarcodePosition = null;
                if (IsSaveDebugPicture)
                {
                    mvd2DReader.MvdRunImage.SaveImage($"{SaveDebugPath}\\Code\\{lblName}_Run.bmp", MVD_FILE_FORMAT.MVD_FILE_BMP);
                }
            }
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

        public void Dispose()
        {
            if (cPositionFixToolObj != null)
            {
                cPositionFixToolObj.Dispose();
                cPositionFixToolObj = null;
            }
            if (cImageArithmeticToolObj != null)
            {
                cImageArithmeticToolObj.Dispose();
                cImageArithmeticToolObj = null;
            }
        }
    }
}
