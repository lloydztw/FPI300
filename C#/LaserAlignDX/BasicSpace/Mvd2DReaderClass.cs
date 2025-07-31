using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using VisionDesigner;
using VisionDesigner.Code2DReader;
//using VisionDesigner.MVDCNNCodeReader;

namespace LaserAlignDX.BasicSpace
{
    public enum CodeType : int
    {
        Code_DataMatrix = 0,
        Code_QrCode = 1,
    }
    public class Mvd2DReaderClass : IDisposable
    {
        C2DCodeReaderTool Code2DReaderTool = null;// new C2DCodeReaderTool();
        C2DCodeVerifyTool c2DCodeVerifyTool = null;// new C2DCodeVerifyTool();
        VisionDesigner.ImageMorph.CImageMorphTool cImageMorphToolObj = null;// new VisionDesigner.ImageMorph.CImageMorphTool();
        VisionDesigner.ImageBinary.CImageBinaryTool cImageBinaryToolObj = null;// new VisionDesigner.ImageBinary.CImageBinaryTool();

        private bool m_ReadGrade = false;
        private string m_BarcodeStr = string.Empty;
        private string m_GradeStr = string.Empty;
        private CodeType codeType = CodeType.Code_DataMatrix;
        public C2DCodeInfo DCodeInfo = null;

        public bool IsReadGrade
        {
            get { return m_ReadGrade; }
        }
        public string xBarcodeStr
        {
            get { return m_BarcodeStr; }
        }
        public string xGradeStr
        {
            get { return m_GradeStr; }
        }
        public CodeType xCodeType
        {
            get { return codeType; }
            set { codeType = value; }
        }
        public CMvdImage MvdRunImage
        {
            get { return Code2DReaderTool.InputImage; }
        }

        public Mvd2DReaderClass()
        {

        }
        public Mvd2DReaderClass(bool eGrade)
        {
            m_ReadGrade = eGrade;
        }
        ~Mvd2DReaderClass()
        {
            Dispose();
        }
        public void Run(Bitmap eBmpImage, RectangleF eRoi)
        {
            CMvdImage eMvdImage = BitmapToCMvdImage(eBmpImage);
            if (eRoi == null)
            {
                Run(eMvdImage, null);
            }
            else
            {
                CMvdRectangleF _roi = new CMvdRectangleF(eRoi.X + eRoi.Width / 2, eRoi.Y + eRoi.Height / 2, eRoi.Width, eRoi.Height);
                Run(eMvdImage, _roi);
            }
            eMvdImage.Dispose();
        }
        public void Run(CMvdImage eMvdImage, RectangleF eRoi)
        {
            if (eRoi == null)
            {
                Run(eMvdImage, null);
            }
            else
            {
                CMvdRectangleF _roi = new CMvdRectangleF(eRoi.X + eRoi.Width / 2, eRoi.Y + eRoi.Height / 2, eRoi.Width, eRoi.Height);
                Run(eMvdImage, _roi);
            }
        }
        public void Run(CMvdImage eMvdImage,CMvdShape eMvdRoi)
        {
            if (Code2DReaderTool == null)
                Code2DReaderTool = new C2DCodeReaderTool();

            if (c2DCodeVerifyTool == null)
                c2DCodeVerifyTool = new C2DCodeVerifyTool();

            if (cImageMorphToolObj == null)
                cImageMorphToolObj = new VisionDesigner.ImageMorph.CImageMorphTool();
            if (cImageBinaryToolObj == null)
                cImageBinaryToolObj = new VisionDesigner.ImageBinary.CImageBinaryTool();

            c2DCodeVerifyTool.BasicParam.VerifyPrcType = MVD_SYMBOL_VERIFY_PROCESS_TYPE.MVD_SYMBOL_VERIFY_PROCESS_TYPE_I;
            c2DCodeVerifyTool.BasicParam.VerifyLabel = MVD_SYMBOL_VERIFY_LABEL.MVD_SYMBOL_VERIFY_LABEL_STANDARD;
            c2DCodeVerifyTool.BasicParam.VerifyStandard = MVD_SYMBOL_VERIFY_STANDARD.MVD_SYMBOL_VERIFY_ISO_STANDARD_29158;
            m_BarcodeStr = string.Empty;
            m_GradeStr = string.Empty;
            DCodeInfo = null;

            try
            {
                //Stopwatch RunLineWatch = new Stopwatch();
                //RunLineWatch.Start();

                CMvdImage cInputImg = eMvdImage;
                if (cInputImg.PixelFormat != MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08)
                {
                    //当前程序仅支持mono8。因此像素格会转换.
                    cInputImg.ConvertImagePixelFormat(MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08);
                }
                ////形态学
                //cImageMorphToolObj.InputImage = cInputImg;
                //cImageMorphToolObj.SetRunParam("Type", "Open");
                //cImageMorphToolObj.ROI = eMvdRoi;
                ////= new VisionDesigner.CMvdRectangleF(cInputImg.Width / 2, cInputImg.Height / 2, cInputImg.Width / 4, cInputImg.Height / 4);
                //cImageMorphToolObj.Run();

                Code2DReaderTool.InputImage = cInputImg;
                //Code2DReaderTool.InputImage = cImageMorphToolObj.Result.OutputImage;
                Code2DReaderTool.ROI = eMvdRoi;
                switch (codeType)
                {
                    case CodeType.Code_QrCode:
                        Code2DReaderTool.SetRunParam("CodeQRFlag", "1");
                        Code2DReaderTool.SetRunParam("CodeDMFlag", "0");
                        break;
                    case CodeType.Code_DataMatrix:
                    default:
                        Code2DReaderTool.SetRunParam("CodeQRFlag", "0");
                        Code2DReaderTool.SetRunParam("CodeDMFlag", "1");
                        break;
                }
                Code2DReaderTool.SetRunParam("RectangleFlag", "Both");
                Code2DReaderTool.SetRunParam("AppMode", "ProMode");
                Code2DReaderTool.SetRunParam("Loc2DCodeNum", "5");
                Code2DReaderTool.SetRunParam("MaxBarSize", "1000");
                Code2DReaderTool.SetRunParam("MinBarSize", "20");
                if (cInputImg.Height >= 400 && cInputImg.Width >= 400)
                    Code2DReaderTool.SetRunParam("SampleLevel", "4");
                else if (cInputImg.Height >= 300 && cInputImg.Width >= 300)
                    Code2DReaderTool.SetRunParam("SampleLevel", "3");
                else if (cInputImg.Height >= 200 && cInputImg.Width >= 200)
                    Code2DReaderTool.SetRunParam("SampleLevel", "2");
                else
                    Code2DReaderTool.SetRunParam("SampleLevel", "1");

                #region 变换读取
                int i = 0;
                while (i < 4)
                {
                    switch (i)
                    {
                        //case 0:
                        //    Code2DReaderTool.SetRunParam("Polarity", "DarkOnBright");
                        //    break;
                        //case 1:
                        //    Code2DReaderTool.SetRunParam("Polarity", "BirghtOnDark");
                        //    break;
                        case 0:
                            //形态学
                            cImageMorphToolObj.InputImage = cInputImg;
                            cImageMorphToolObj.SetRunParam("Type", "Open");
                            cImageMorphToolObj.ROI = eMvdRoi;
                            //= new VisionDesigner.CMvdRectangleF(cInputImg.Width / 2, cInputImg.Height / 2, cInputImg.Width / 4, cInputImg.Height / 4);
                            cImageMorphToolObj.Run();
                            Code2DReaderTool.SetRunParam("Polarity", "Both");
                            Code2DReaderTool.InputImage = cImageMorphToolObj.Result.OutputImage;
                            break;
                        case 1:
                            //形态学
                            cImageMorphToolObj.InputImage = cInputImg;
                            cImageMorphToolObj.SetRunParam("Type", "Close");
                            cImageMorphToolObj.ROI = eMvdRoi;
                            //= new VisionDesigner.CMvdRectangleF(cInputImg.Width / 2, cInputImg.Height / 2, cInputImg.Width / 4, cInputImg.Height / 4);
                            cImageMorphToolObj.Run();
                            Code2DReaderTool.SetRunParam("Polarity", "Both");
                            Code2DReaderTool.InputImage = cImageMorphToolObj.Result.OutputImage;
                            break;
                        case 2:
                            //二值化
                            cImageBinaryToolObj.InputImage = cInputImg;
                            cImageBinaryToolObj.ROI = null;
                            //= new CMvdRectangleF(OutputImage.Width / 2, OutputImage.Height / 2, OutputImage.Width / 4, OutputImage.Height / 4);
                            cImageBinaryToolObj.SetRunParam("LowThreshold", "128");
                            //cImageArithmeticToolObj.SetRunParam("HighThreshold", BlobHighThreshold.ToString());
                            cImageBinaryToolObj.Run();
                            Code2DReaderTool.SetRunParam("Polarity", "Both");
                            Code2DReaderTool.InputImage = cImageBinaryToolObj.Result.OutputImage;
                            break;
                        default:
                            Code2DReaderTool.InputImage = cInputImg;
                            Code2DReaderTool.SetRunParam("Polarity", "Both");
                            break;
                    }
                    //运行
                    Code2DReaderTool.Run();

                    if (Code2DReaderTool.Result.CodeInfoList.Count() > 0)
                        break;

                    i++;
                }
                #endregion

                //RunLineWatch.Stop();
                //m_DurTime = RunLineWatch.ElapsedMilliseconds;
                //RunLineWatch = null;

                if (Code2DReaderTool.Result.CodeInfoList.Count() > 0)
                {
                    m_BarcodeStr = Code2DReaderTool.Result.CodeInfoList[0].Content.ToString();
                    DCodeInfo = Code2DReaderTool.Result.CodeInfoList[0];
                    if (m_ReadGrade)
                    {
                        c2DCodeVerifyTool.InputImage = cInputImg;
                        c2DCodeVerifyTool.BasicParam.RecognitionInfo = Code2DReaderTool.Result;
                        c2DCodeVerifyTool.Run();
                        if (c2DCodeVerifyTool.Result.QualityInfoList.Count() > 0)
                        {
                            C2DCodeQualityInfoByISO29158 c2DCodeQualityInfoByISO29158
                                = (C2DCodeQualityInfoByISO29158)c2DCodeVerifyTool.Result.QualityInfoList[0];
                            m_GradeStr = _getBarcodeGrade(c2DCodeQualityInfoByISO29158.DecodeGrade);
                        }
                    }
                }

                //cInputImg.Dispose();
                //cInputImg = null;

            }
            catch (Exception ex)
            {
                DCodeInfo = null;
                m_BarcodeStr = string.Empty;
                m_GradeStr = string.Empty;
            }
        }
        public void Dispose()
        {
            if (Code2DReaderTool != null)
            {
                Code2DReaderTool.Dispose();
                Code2DReaderTool = null;
            }
            if (c2DCodeVerifyTool != null)
            {
                c2DCodeVerifyTool.Dispose();
                c2DCodeVerifyTool = null;
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
        }

        string _getBarcodeGrade(byte ebyte)
        {
            switch (ebyte)
            {
                case 0: return "F";
                case 1: return "D";
                case 2: return "C";
                case 3: return "B";
                case 4: return "A";
            }
            return "A";
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
    }
}
