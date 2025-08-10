using AUVision;
using Common.RecipeSpace;
using JetEazy.PlugSpace.BarcodeEx;
using LaserAlignDX.BasicSpace;
using LaserAlignDX.OPSpace.RecipeSpace;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Traveller106;
using VisionDesigner;
using VisionDesigner.Code2DReader;
using VisionDesigner.ImageArithmetic;
using VisionDesigner.ImageRegionCopy;
using VisionDesigner.PositionFix;
using ZXing;

namespace LaserAlignDX.OPSpace
{
    public enum InspectReason : int
    {
        [Description("PASS")]
        PASS = 0,
        /// <summary>
        /// 空料
        /// </summary>
        [Description("空料")]
        //[Description("印字错误")]
        INS_ALIGNERR = 1,
        [Description("切割NG")]
        INS_CUTTINGERR = 2,
        /// <summary>
        /// 疑似有料及外观NG
        /// </summary>
        [Description("疑似有料")]
        //[Description("印字缺失")]
        INS_DEFECTERR = 3,
        /// <summary>
        /// 2D读取错误
        /// </summary>
        [Description("2D读取错误")]
        INS_2DERR = 4,
        /// <summary>
        /// 2D比对错误
        /// </summary>
        [Description("2D比对错误")]
        INS_2DMAPNG = 5,
        [Description("2D码重复")]
        INS_2DREPEATERR = 6,
        [Description("不检测")]
        INS_NOOPEN = 7,
    }

    public class RegionCellX2Class : IDisposable
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

        private CMvdRectangleF MvdRunPositionFix;

        public RegionCellX2Class()
        {

        }
        ~RegionCellX2Class()
        {
            Dispose();
        }

        public int Index = 0;
        public string Name = "";
        public string Result = "";
        public string lblName = "";
        public int CellRow = 0;
        public int CellCol = 0;
        public RectangleF viewRectF = new RectangleF();

        public float OrgX = 0;
        public float OrgY = 0;
        public float RunX = 0;
        public float RunY = 0;

        public bool ByPass = false;
        public string SetBarcodeStr = string.Empty;

        public bool IsSaveDebugPicture = false;
        public string SaveDebugPath = $"D:\\log\\DebugImage";

        public InspectX2Class xInspectPara = new InspectX2Class();

        public Bitmap bmpItemTemplate = new Bitmap(1, 1);
        public Bitmap bmpItemRun = new Bitmap(1, 1);
        public Bitmap bmpItemMask = new Bitmap(1, 1);
        public Bitmap bmpItemCodeRun = new Bitmap(1, 1);

        //public int BlobLowThreshold = 128;
        //public int BlobHighThreshold = 255;

        public InspectReason inspectReason = InspectReason.PASS;
        public AUVision.xFindResult xFindResult = new AUVision.xFindResult();
        public List<InspectReason> inspectReasons = new List<InspectReason>();

        public C2DCodeInfo RunCodeInfo = null;
        public CMvdPolygonF DrawBarcodePosition = null;
        //public CMvdRectangleF GetMvdFix()
        //{
        //    return MvdRunPositionFix;
        //}
        public CMvdRectangleF DrawResultRectF()
        {
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

            if (ByPass && !INI.Instance.IsForceInspect)
            {
                MvdRunPositionFix = new CMvdRectangleF(viewRectF.X + viewRectF.Width / 2, viewRectF.Y + viewRectF.Height / 2, viewRectF.Width,
                        viewRectF.Height);
                MvdRunPositionFix.BorderColor = new MVD_COLOR(255, 0, 0);
            }
            return MvdRunPositionFix;
        }
        public bool GetOffsetResult()
        {
            bool bOK = true;

            if (xInspectPara.bCheckOffset)
                return bOK;

            if (Math.Abs(RunX) > xInspectPara.xOffsetX)
            {
                bOK = false;
                inspectReasons.Add(InspectReason.INS_CUTTINGERR);
            }
            else if (Math.Abs(RunY) > xInspectPara.xOffsetY)
            {
                bOK = false;
                inspectReasons.Add(InspectReason.INS_CUTTINGERR);
            }
            return bOK;
        }
        private List<CMvdRectangleF> blobMvdRectFNGList = new List<CMvdRectangleF>();
        public List<CMvdRectangleF> DrawBlobNGList()
        {
            List<CMvdRectangleF> cMvdRectangleFs = new List<CMvdRectangleF>();

            foreach (RectangleF rectangleF in xInspectPara.rectangles)
            {
                bool bFound = false;
                foreach (CMvdRectangleF mvdRectangleF in blobMvdRectFNGList)
                {
                    //mvdRectangleF.CenterX += MvdRunPositionFix.CenterX - bmpItemRun.Width / 2;
                    //mvdRectangleF.CenterY += MvdRunPositionFix.CenterY - bmpItemRun.Height / 2;
                    MVD_RECT_F mVD_RECT_F = mvdRectangleF.GetBoundingRect();
                    RectangleF rectangleF1 = new RectangleF(mVD_RECT_F.fX, mVD_RECT_F.fY, mVD_RECT_F.fWidth, mVD_RECT_F.fHeight);
                    if (rectangleF.IntersectsWith(rectangleF1))
                    {
                        bFound = true;
                        break;
                    }
                }

                if (bFound)
                {
                    CMvdRectangleF cMvdRectangleF = new CMvdRectangleF(rectangleF.X + rectangleF.Width / 2,
                        rectangleF.Y + rectangleF.Height / 2, rectangleF.Width, rectangleF.Height);

                    cMvdRectangleF.CenterX += MvdRunPositionFix.CenterX - bmpItemRun.Width / 2;
                    cMvdRectangleF.CenterY += MvdRunPositionFix.CenterY - bmpItemRun.Height / 2;
                    cMvdRectangleF.BorderColor = new MVD_COLOR(255, 0, 0);
                    cMvdRectangleF.BorderWidth = 1;
                    cMvdRectangleFs.Add(cMvdRectangleF);
                    //break;
                }
            }

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
            str += $"{RunX}" + ",";
            str += $"{RunY}" + ",";
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

        public void Reset()
        {
            inspectReason = InspectReason.PASS;
            xFindResult = new AUVision.xFindResult();
            inspectReasons.Clear();
            RunCodeInfo = null;
            //if (mvd2DReader != null)
            //    mvd2DReader.DCodeInfo = null;
            DrawBarcodePosition = null;
            RunX = 0;
            RunY = 0;
            IsSaveDebugPicture = false;
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
            cImageBinaryToolObj.SetRunParam("LowThreshold", xInspectPara.xThresholdValue.ToString());
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
                if (item.AreaF > xInspectPara.xCharArea)
                {
                    blobMvdRectFNGList.Add(item.BoxInfo);
                }
                else if (item.LongAxis > xInspectPara.xCharWidth && item.ShortAxis > xInspectPara.xCharHeight)
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
                    cImageBinaryToolObj.Result.OutputImage.SaveImage($"{SaveDebugPath}\\Detect\\{lblName}_Diff2.bmp", MVD_FILE_FORMAT.MVD_FILE_BMP);
                    cBlobFindToolObj.RegionImage.SaveImage($"{SaveDebugPath}\\Detect\\{lblName}_Diff2_1.bmp", MVD_FILE_FORMAT.MVD_FILE_BMP);
                    if (cBlobFindRes.BlobImage != null)
                        cBlobFindRes.BlobImage.SaveImage($"{SaveDebugPath}\\Detect\\{lblName}_Diff3.bmp", MVD_FILE_FORMAT.MVD_FILE_BMP);
                }
            }

        }
        public void DeCode2D(Bitmap eBmpRun, PointF Poi_CodeBase)
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

                if (xInspectPara.bCheckMappingCode)
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

        public bool CheckRepeatCode(List<string> eCodes, int irepeatCount = 1)
        {
            bool result = true;
            if (ByPass && !INI.Instance.IsForceInspect)
                return result;
            if (RunCodeInfo == null)
                return result;
            int recordPCS = 0;
            if (!string.IsNullOrEmpty(RunCodeInfo.Content))
            {
                foreach (string s in eCodes)
                {
                    if (s.Trim() == RunCodeInfo.Content.Trim())
                    {
                        recordPCS++;
                    }
                }

                if (recordPCS > irepeatCount)
                {
                    bool bFound = false;
                    foreach (InspectReason inspectReason in inspectReasons)
                    {
                        if (inspectReason == InspectReason.INS_2DREPEATERR)
                        {
                            bFound = true;
                            break;
                        }
                    }
                    if (!bFound)
                    {
                        DrawBarcodePosition.BorderColor = new MVD_COLOR(255, 0, 0);
                        inspectReasons.Add(InspectReason.INS_2DREPEATERR);
                    }
                    result = false;
                }
            }
            return result;
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
            if (cImageMorphToolObj != null)
            {
                cImageMorphToolObj.Dispose();
                cImageMorphToolObj = null;
            }
            if (cImageBinaryToolObj != null)
            {
                cImageBinaryToolObj.Dispose();
                cImageBinaryToolObj = null;
            }
            if (cBlobFindToolObj != null)
            {
                cBlobFindToolObj.Dispose();
                cBlobFindToolObj = null;
            }
            if (mvd2DReader != null)
            {
                mvd2DReader.Dispose();
                mvd2DReader = null;
            }
        }
    }
}
