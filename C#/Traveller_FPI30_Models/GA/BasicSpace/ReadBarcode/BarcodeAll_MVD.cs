
#define OPT_USE_MVD_READER

//#define OPT_USE_MVD_CNN_READER
#if OPT_USE_MVD_CNN_READER
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using VisionDesigner;
//using VisionDesigner.Code2DReader;
using VisionDesigner.MVDCNNCodeReader;


namespace JetEazy.PlugSpace.BarcodeEx
{
    public class BarcodeItem
    {
        public CNNCodeInfo DCodeInfo = null;
        //public C2DCodeInfo DCodeInfo = null;
        public string OverGrade = string.Empty;
        public string DecodeGrade = string.Empty;
        public void Reset()
        {
            OverGrade = string.Empty;
            DecodeGrade = string.Empty;
            DCodeInfo = null;
        }
    }

    public class BarcodeAll_MVD : IDisposable
    {
        CNNCodeReaderTool Code2DReaderTool = null;
        CNNCodeVerifyTool c2DCodeVerifyTool = null;

        private long m_DurTime = 0;
        private string m_ErrMsg = string.Empty;
        public string ErrMsg
        {
            get { return m_ErrMsg; }
        }

        public BarcodeItem GetBarcodeItem = new BarcodeItem();

        public BarcodeAll_MVD()
        {
            DecodeTrain();
        }

        public int DecodeTrain()
        {
            if (Code2DReaderTool == null)
                Code2DReaderTool = new CNNCodeReaderTool(MVD_ALGORITHM_PLATFORM_TYPE.MVD_ALGORITHM_PLATFORM_CPU);



            if (c2DCodeVerifyTool == null)
                c2DCodeVerifyTool = new CNNCodeVerifyTool(MVD_ALGORITHM_PLATFORM_TYPE.MVD_ALGORITHM_PLATFORM_CPU);

            c2DCodeVerifyTool.BasicParam.VerifyPrcType2D = MVD_SYMBOL_VERIFY_PROCESS_TYPE.MVD_SYMBOL_VERIFY_PROCESS_TYPE_I;
            c2DCodeVerifyTool.BasicParam.VerifyLabel2D = MVD_SYMBOL_VERIFY_LABEL.MVD_SYMBOL_VERIFY_LABEL_STANDARD;
            c2DCodeVerifyTool.BasicParam.VerifyStandard2D = MVD_SYMBOL_VERIFY_STANDARD.MVD_SYMBOL_VERIFY_ISO_STANDARD_29158;


            return 0;
        }

        /// <summary>
        /// 解码 DATAMATRIX
        /// </summary>
        /// <param name="ebmpInput">输入图片 需要白底黑字</param>
        /// <returns>返回 条码字符串</returns>
        public string DecodeStr(Bitmap ebmpInput)
        {
            return _cnnDecodeAndGrade(ebmpInput);
        }
        public string DecodeGrade(Bitmap ebmpInput)
        {
            return _cnnDecodeAndGrade(ebmpInput, true);
        }
        public long DecodeStrDurtime
        {
            get { return m_DurTime; }
        }

#if MVD2DREADER
        string _DecodeGrade(Bitmap ebmpInput)
        {
            string retStr = string.Empty;

            try
            {
                GetBarcodeItem.Reset();

                Stopwatch RunLineWatch = new Stopwatch();
                RunLineWatch.Start();

                CMvdImage cInputImg = BitmapToCMvdImage(ebmpInput);
                if (cInputImg.PixelFormat != MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08)
                {
                    //当前程序仅支持mono8。因此像素格会转换.
                    cInputImg.ConvertImagePixelFormat(MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08);
                }

                int nImageWidth = (int)cInputImg.Width;
                int nImageHeight = (int)cInputImg.Height;

                //创建ROI（非必须）
                CMvdRectangleF cRectROIObj = new CMvdRectangleF(nImageWidth / 2, nImageHeight / 2, nImageWidth, nImageHeight);
                cRectROIObj.Angle = 0;

                //二维码识别实例创建与赋值
                C2DCodeReaderTool Code2DReaderTool = new C2DCodeReaderTool();


                //IMVS2dBcrModuCs.IMVS2dBcrModuTool Code2DReaderTool=new IMVS2dBcrModuCs.IMVS2dBcrModuTool();
                //IMVSCnnCodeRecgModuCCs.IMVSCnnCodeRecgModuCTool Code2DReaderTool = new IMVSCnnCodeRecgModuCCs.IMVSCnnCodeRecgModuCTool();
                Code2DReaderTool.InputImage = cInputImg;
                Code2DReaderTool.ROI = cRectROIObj;
                //Code2DReaderTool.SetRunParam("Polarity", "1");
                Code2DReaderTool.SetRunParam("CodeQRFlag", "0");

                //string strkey = "";
                //Code2DReaderTool.SetRunParam("Polarity", "2");
                //Code2DReaderTool.GetRunParam("Polarity", ref strkey);

                //Code2DReaderTool.SetRunParam("Loc2DCodeNum", "60");
                //Code2DReaderTool.GetRunParam("Loc2DCodeNum", ref strkey);

                //运行
                Code2DReaderTool.Run();

                RunLineWatch.Stop();
                m_DurTime = RunLineWatch.ElapsedMilliseconds;
                RunLineWatch = null;

                if (Code2DReaderTool.Result.CodeInfoList.Count() > 0)
                {
                    retStr = Code2DReaderTool.Result.CodeInfoList[0].Content.ToString();

                    C2DCodeVerifyTool c2DCodeVerifyTool = new C2DCodeVerifyTool();
                    c2DCodeVerifyTool.InputImage = cInputImg;
                    c2DCodeVerifyTool.BasicParam.RecognitionInfo = Code2DReaderTool.Result;
                    c2DCodeVerifyTool.BasicParam.VerifyStandard =
                        MVD_SYMBOL_VERIFY_STANDARD.MVD_SYMBOL_VERIFY_ISO_STANDARD_29158;
                    c2DCodeVerifyTool.Run();


                    if (c2DCodeVerifyTool.Result.QualityInfoList.Count() > 0)
                    {
                        C2DCodeQualityInfoByISO29158 c2DCodeQualityInfoByISO29158
                            = (C2DCodeQualityInfoByISO29158)c2DCodeVerifyTool.Result.QualityInfoList[0];

                        C2DCodeReaderResult c2DCodeReaderResult
                            = c2DCodeVerifyTool.BasicParam.RecognitionInfo;
                        GetBarcodeItem.DCodeInfo = c2DCodeReaderResult.CodeInfoList[0];

                        GetBarcodeItem.OverGrade = _getBarcodeGrade(c2DCodeQualityInfoByISO29158.OverQuality);
                        GetBarcodeItem.DecodeGrade = _getBarcodeGrade(c2DCodeQualityInfoByISO29158.DecodeGrade);
                    }
                }

                Code2DReaderTool.Dispose();
                Code2DReaderTool = null;

                cInputImg.Dispose();
                cInputImg = null;

            }
            catch (Exception ex)
            {

            }
            return retStr;
        }
        string Decode(Bitmap ebmpInput)
        {
            string retStr = string.Empty;

            try
            {
                //Bitmap bmptemp = new Bitmap(strBasicImgPath);
                //Bitmap bmp = bmptemp.Clone(new Rectangle(0, 0, bmptemp.Width, bmptemp.Height), PixelFormat.Format24bppRgb);
                //bmptemp.Dispose();

                Stopwatch RunLineWatch = new Stopwatch();
                RunLineWatch.Start();


                CMvdImage cInputImg = BitmapToCMvdImage(ebmpInput);
                if (cInputImg.PixelFormat != MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08)
                {
                    //当前程序仅支持mono8。因此像素格会转换.
                    cInputImg.ConvertImagePixelFormat(MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08);
                }

                int nImageWidth = (int)cInputImg.Width;
                int nImageHeight = (int)cInputImg.Height;

                //创建ROI（非必须）
                CMvdRectangleF cRectROIObj = new CMvdRectangleF(nImageWidth / 2, nImageHeight / 2, nImageWidth, nImageHeight);
                cRectROIObj.Angle = 0;

                //二维码识别实例创建与赋值
                C2DCodeReaderTool Code2DReaderTool = new C2DCodeReaderTool();
                Code2DReaderTool.InputImage = cInputImg;
                Code2DReaderTool.ROI = cRectROIObj;
                //Code2DReaderTool.SetRunParam("Polarity", "1");
                Code2DReaderTool.SetRunParam("CodeQRFlag", "0");

                //运行
                Code2DReaderTool.Run();

                //listResurt.Items.Add("运行时间:" + Code2DReaderTool.GetAlgRunTime().ToString("0.000") + " ms");
                ////显示结果
                //listResurt.Items.Add("总共读到条码数: " + Code2DReaderTool.Result.CodeInfoList.Count().ToString());
                //for (int i = 0; i < Code2DReaderTool.Result.CodeInfoList.Count(); i++)
                //{
                //    //二维码信息
                //    listResurt.Items.Add("第:" + ((i + 1).ToString() + " 个"));
                //    listResurt.Items.Add("中心点 = {"
                //          + Code2DReaderTool.Result.CodeInfoList[i].Center.nX.ToString() + ", "
                //          + Code2DReaderTool.Result.CodeInfoList[i].Center.nY.ToString() + "}");

                //    listResurt.Items.Add("角度 = " + Code2DReaderTool.Result.CodeInfoList[i].Angle.ToString());
                //    listResurt.Items.Add("类型= " + Code2DReaderTool.Result.CodeInfoList[i].Type.ToString());
                //    listResurt.Items.Add("内容 = " + Code2DReaderTool.Result.CodeInfoList[i].Content.ToString());
                //    listResurt.Items.Add("估计PPM = " + Code2DReaderTool.Result.CodeInfoList[i].Estppm.ToString());

                //    listResurt.Items.Add("");
                //}

                RunLineWatch.Stop();
                m_DurTime = RunLineWatch.ElapsedMilliseconds;
                RunLineWatch = null;

                if (Code2DReaderTool.Result.CodeInfoList.Count() > 0)
                {
                    retStr = Code2DReaderTool.Result.CodeInfoList[0].Content.ToString();
                }

                Code2DReaderTool.Dispose();
                Code2DReaderTool = null;

                cInputImg.Dispose();
                cInputImg = null;

            }
            catch (Exception ex)
            {
                //listResurt.Items.Add(ex.Message);

            }
            return retStr;
        }
#endif

        string _cnnDecodeAndGrade(Bitmap ebmpInput, bool eGradeUse = false)
        {
            string retStr = string.Empty;

            try
            {
                GetBarcodeItem.Reset();

                Stopwatch RunLineWatch = new Stopwatch();
                RunLineWatch.Start();

                CMvdImage cInputImg = BitmapToCMvdImage(ebmpInput);
                if (cInputImg.PixelFormat != MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08)
                {
                    //当前程序仅支持mono8。因此像素格会转换.
                    cInputImg.ConvertImagePixelFormat(MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08);
                }

                int nImageWidth = (int)cInputImg.Width;
                int nImageHeight = (int)cInputImg.Height;

                //创建ROI（非必须）
                CMvdRectangleF cRectROIObj = new CMvdRectangleF(nImageWidth / 2, nImageHeight / 2, nImageWidth, nImageHeight);
                cRectROIObj.Angle = 0;

                //二维码识别实例创建与赋值
                //CNNCodeReaderTool Code2DReaderTool = new CNNCodeReaderTool(MVD_ALGORITHM_PLATFORM_TYPE.MVD_ALGORITHM_PLATFORM_CPU);
                Code2DReaderTool.InputImage = cInputImg;
                Code2DReaderTool.ROI = null;// cRectROIObj;

                Code2DReaderTool.SetRunParam("WaitTimeOut", "1000");
                Code2DReaderTool.SetRunParam("Code39Flag", "0");
                Code2DReaderTool.SetRunParam("Code128Flag", "0");
                Code2DReaderTool.SetRunParam("CodaBarFlag", "0");
                Code2DReaderTool.SetRunParam("CodeEAN8Flag", "0");
                Code2DReaderTool.SetRunParam("CodeITF25Flag", "0");
                Code2DReaderTool.SetRunParam("Code93Flag", "0");
                Code2DReaderTool.SetRunParam("CodeUPCEFlag", "0");
                Code2DReaderTool.SetRunParam("CodeUPCAFlag", "0");
                Code2DReaderTool.SetRunParam("CodeEAN13Flag", "0");
                Code2DReaderTool.SetRunParam("CodeITF14Flag", "0");
                Code2DReaderTool.SetRunParam("CodeMATRIX25Flag", "0");
                Code2DReaderTool.SetRunParam("CodeMSIFlag", "0");
                Code2DReaderTool.SetRunParam("CodeCODE11Flag", "0");
                Code2DReaderTool.SetRunParam("CodeINDUSTRIAL25Flag", "0");
                Code2DReaderTool.SetRunParam("CodeCHINAPOSTFlag", "0");
                Code2DReaderTool.SetRunParam("Loc2DCodeNum", "1");
                //Code2DReaderTool.SetRunParam("PerfMode", "0");
                Code2DReaderTool.SetRunParam("DM", "1");
                Code2DReaderTool.SetRunParam("QR", "1");
                Code2DReaderTool.SetRunParam("ECC140", "0");

                //Code2DReaderTool.SetRunParam("CodeQRFlag", "0");
                //Code2DReaderTool.SetRunParam("RectangleFlag", "Both");
                //Code2DReaderTool.SetRunParam("Loc2DCodeNum", "1");
                //Code2DReaderTool.SetRunParam("MaxBarSize", "1000");

                //int i = 0;
                //while (i < 3)
                //{

                //    switch (i)
                //    {
                //        case 0:
                //            Code2DReaderTool.SetRunParam("Polarity", "DarkOnBright");
                //            break;
                //        case 1:
                //            Code2DReaderTool.SetRunParam("Polarity", "BirghtOnDark");
                //            break;
                //        default:
                //            Code2DReaderTool.SetRunParam("Polarity", "Both");
                //            break;
                //    }
                //}

                //string strkey = "";
                //Code2DReaderTool.SetRunParam("Polarity", "2");
                //Code2DReaderTool.GetRunParam("Polarity", ref strkey);

                //Code2DReaderTool.SetRunParam("Loc2DCodeNum", "1");
                //Code2DReaderTool.GetRunParam("Loc2DCodeNum", ref strkey);
                //运行
                Code2DReaderTool.Run();

                RunLineWatch.Stop();
                m_DurTime = RunLineWatch.ElapsedMilliseconds;
                RunLineWatch = null;

                if (Code2DReaderTool.Result.CodeInfo.Count() > 0)
                {
                    retStr = Code2DReaderTool.Result.CodeInfo[0].Content.ToString();
                    if (eGradeUse)
                    {
                        //CNNCodeVerifyTool c2DCodeVerifyTool = new CNNCodeVerifyTool(MVD_ALGORITHM_PLATFORM_TYPE.MVD_ALGORITHM_PLATFORM_CPU);
                        c2DCodeVerifyTool.InputImage = cInputImg;
                        c2DCodeVerifyTool.BasicParam.RecognitionInfo = Code2DReaderTool.Result;
                        //c2DCodeVerifyTool.BasicParam.VerifyPrcType2D = MVD_SYMBOL_VERIFY_PROCESS_TYPE.MVD_SYMBOL_VERIFY_PROCESS_TYPE_I;
                        //c2DCodeVerifyTool.BasicParam.VerifyLabel2D = MVD_SYMBOL_VERIFY_LABEL.MVD_SYMBOL_VERIFY_LABEL_STANDARD;
                        //c2DCodeVerifyTool.BasicParam.VerifyStandard2D = MVD_SYMBOL_VERIFY_STANDARD.MVD_SYMBOL_VERIFY_ISO_STANDARD_29158;

                        c2DCodeVerifyTool.Run();
                        if (c2DCodeVerifyTool.Result.QualityInfo2D.Count() > 0)
                        {

                            CNNCodeQualityInfoByISO29158 c2DCodeQualityInfoByISO29158
                                = (CNNCodeQualityInfoByISO29158)c2DCodeVerifyTool.Result.QualityInfo2D[0];

                            CNNCodeReaderResult c2DCodeReaderResult
                                = c2DCodeVerifyTool.BasicParam.RecognitionInfo;
                            GetBarcodeItem.DCodeInfo = c2DCodeReaderResult.CodeInfo[0];

                            GetBarcodeItem.OverGrade = _getBarcodeGrade(c2DCodeQualityInfoByISO29158.OverQuality);
                            GetBarcodeItem.DecodeGrade = _getBarcodeGrade(c2DCodeQualityInfoByISO29158.DecodeGrade);
                        }
                    }
                }

                //Code2DReaderTool.Dispose();
                //Code2DReaderTool = null;

                cInputImg.Dispose();
                cInputImg = null;

            }
            catch (Exception ex)
            {
                retStr = string.Empty;
                m_ErrMsg = ex.StackTrace;
            }
            return retStr;
        }

#if OPT_DECODE_NO_USE
        string _cnnDecode(Bitmap ebmpInput)
        {
            string retStr = string.Empty;

            try
            {
                Stopwatch RunLineWatch = new Stopwatch();
                RunLineWatch.Start();
                CMvdImage cInputImg = BitmapToCMvdImage(ebmpInput);
                if (cInputImg.PixelFormat != MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08)
                {
                    //当前程序仅支持mono8。因此像素格会转换.
                    cInputImg.ConvertImagePixelFormat(MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08);
                }
                int nImageWidth = (int)cInputImg.Width;
                int nImageHeight = (int)cInputImg.Height;

                //创建ROI（非必须）
                CMvdRectangleF cRectROIObj = new CMvdRectangleF(nImageWidth / 2, nImageHeight / 2, nImageWidth, nImageHeight);
                cRectROIObj.Angle = 0;

                //二维码识别实例创建与赋值
                CNNCodeReaderTool Code2DReaderTool = new CNNCodeReaderTool(MVD_ALGORITHM_PLATFORM_TYPE.MVD_ALGORITHM_PLATFORM_CPU);
                Code2DReaderTool.InputImage = cInputImg;
                Code2DReaderTool.ROI = cRectROIObj;

                //运行
                Code2DReaderTool.Run();

                RunLineWatch.Stop();
                m_DurTime = RunLineWatch.ElapsedMilliseconds;
                RunLineWatch = null;

                if (Code2DReaderTool.Result.CodeInfo.Count() > 0)
                {
                    retStr = Code2DReaderTool.Result.CodeInfo[0].Content.ToString();
                }

                Code2DReaderTool.Dispose();
                Code2DReaderTool = null;

                cInputImg.Dispose();
                cInputImg = null;

            }
            catch (Exception ex)
            {
                //listResurt.Items.Add(ex.Message);

            }
            return retStr;
        }
#endif
        string _getBarcodeLocation(MVD_POINT_I mVD_POINT_I)
        {
            return mVD_POINT_I.nX.ToString() + ":" + mVD_POINT_I.nY.ToString();
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
        string _getBarcodeGrade(int ebyte)
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

        /// <summary>
        /// Bitmap 转成 CMvdImage
        /// </summary>
        /// <param name="bmpInputImg"></param>
        /// <returns></returns>
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
        }
    }
}


#endif


#if OPT_USE_MVD_READER
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using VisionDesigner;
using VisionDesigner.Code2DReader;


namespace JetEazy.PlugSpace.BarcodeEx
{
    public class BarcodeItem
    {
        public C2DCodeInfo DCodeInfo = null;
        public string OverGrade = string.Empty;
        public string DecodeGrade = string.Empty;
        public void Reset()
        {
            OverGrade = string.Empty;
            DecodeGrade = string.Empty;
            DCodeInfo = null;
        }
    }

    public class BarcodeAll_MVD : IDisposable
    {
        C2DCodeReaderTool Code2DReaderTool = null;
        C2DCodeVerifyTool c2DCodeVerifyTool = null;

        private long m_DurTime = 0;
        private string m_ErrMsg = string.Empty;
        public string ErrMsg
        {
            get { return m_ErrMsg; }
        }

        public BarcodeItem GetBarcodeItem = new BarcodeItem();

        public BarcodeAll_MVD()
        {
            DecodeTrain();
        }

        public int DecodeTrain()
        {
            try
            {
                if (Code2DReaderTool == null)
                    Code2DReaderTool = new C2DCodeReaderTool();

                if (c2DCodeVerifyTool == null)
                    c2DCodeVerifyTool = new C2DCodeVerifyTool();

                c2DCodeVerifyTool.BasicParam.VerifyPrcType = MVD_SYMBOL_VERIFY_PROCESS_TYPE.MVD_SYMBOL_VERIFY_PROCESS_TYPE_I;
                c2DCodeVerifyTool.BasicParam.VerifyLabel = MVD_SYMBOL_VERIFY_LABEL.MVD_SYMBOL_VERIFY_LABEL_STANDARD;
                c2DCodeVerifyTool.BasicParam.VerifyStandard = MVD_SYMBOL_VERIFY_STANDARD.MVD_SYMBOL_VERIFY_ISO_STANDARD_29158;

            }
            catch
            {

            }


            return 0;
        }

        /// <summary>
        /// 解码 DATAMATRIX
        /// </summary>
        /// <param name="ebmpInput">输入图片 需要白底黑字</param>
        /// <returns>返回 条码字符串</returns>
        public string DecodeStr(Bitmap ebmpInput)
        {
            return _cnnDecodeAndGrade(ebmpInput);
        }
        public string DecodeGrade(Bitmap ebmpInput)
        {
            return _cnnDecodeAndGrade(ebmpInput, true);
        }
        public long DecodeStrDurtime
        {
            get { return m_DurTime; }
        }
        string _cnnDecodeAndGrade(Bitmap ebmpInput, bool eGradeUse = false)
        {
            string retStr = string.Empty;

            try
            {
                GetBarcodeItem.Reset();
                if (Code2DReaderTool == null)
                {
                    return retStr;
                }

                Stopwatch RunLineWatch = new Stopwatch();
                RunLineWatch.Start();

                CMvdImage cInputImg = BitmapToCMvdImage(ebmpInput);
                if (cInputImg.PixelFormat != MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08)
                {
                    //当前程序仅支持mono8。因此像素格会转换.
                    cInputImg.ConvertImagePixelFormat(MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08);
                }

                //cInputImg.SaveImage("d:\\testtest\\2d\\Mvd_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + "_" + ".png", MVD_FILE_FORMAT.MVD_FILE_BMP);
                //System.Threading.Thread.Sleep(300);
                int nImageWidth = (int)cInputImg.Width;
                int nImageHeight = (int)cInputImg.Height;

                //创建ROI（非必须）
                CMvdRectangleF cRectROIObj = new CMvdRectangleF(nImageWidth / 2, nImageHeight / 2, nImageWidth, nImageHeight);
                cRectROIObj.Angle = 0;

                //if (Code2DReaderTool == null)
                //    Code2DReaderTool = new C2DCodeReaderTool();

                //if (c2DCodeVerifyTool == null)
                //    c2DCodeVerifyTool = new C2DCodeVerifyTool();

                //c2DCodeVerifyTool.BasicParam.VerifyPrcType = MVD_SYMBOL_VERIFY_PROCESS_TYPE.MVD_SYMBOL_VERIFY_PROCESS_TYPE_I;
                //c2DCodeVerifyTool.BasicParam.VerifyLabel = MVD_SYMBOL_VERIFY_LABEL.MVD_SYMBOL_VERIFY_LABEL_STANDARD;
                //c2DCodeVerifyTool.BasicParam.VerifyStandard = MVD_SYMBOL_VERIFY_STANDARD.MVD_SYMBOL_VERIFY_ISO_STANDARD_29158;


                //二维码识别实例创建与赋值
                //CNNCodeReaderTool Code2DReaderTool = new CNNCodeReaderTool(MVD_ALGORITHM_PLATFORM_TYPE.MVD_ALGORITHM_PLATFORM_CPU);
                Code2DReaderTool.InputImage = cInputImg;
                Code2DReaderTool.ROI = null;// cRectROIObj;

                //Code2DReaderTool.SetRunParam("Polarity", "DarkOnBright");
                //Code2DReaderTool.SetRunParam("Polarity", "Both");
                Code2DReaderTool.SetRunParam("CodeQRFlag", "0");
                Code2DReaderTool.SetRunParam("RectangleFlag", "Both");
                //Code2DReaderTool.SetRunParam("AppMode", "ProMode");
                Code2DReaderTool.SetRunParam("Loc2DCodeNum", "1");
                Code2DReaderTool.SetRunParam("MaxBarSize", "1000");

                int i = 0;
                while (i < 3)
                {

                    switch (i)
                    {
                        case 0:
                            Code2DReaderTool.SetRunParam("Polarity", "DarkOnBright");
                            break;
                        case 1:
                            Code2DReaderTool.SetRunParam("Polarity", "BirghtOnDark");
                            break;
                        default:
                            Code2DReaderTool.SetRunParam("Polarity", "Both");
                            break;
                    }

                    //运行
                    Code2DReaderTool.Run();

                    if (Code2DReaderTool.Result.CodeInfoList.Count() > 0)
                        break;

                    i++;
                }

                RunLineWatch.Stop();
                m_DurTime = RunLineWatch.ElapsedMilliseconds;
                RunLineWatch = null;

                if (Code2DReaderTool.Result.CodeInfoList.Count() > 0)
                {
                    retStr = Code2DReaderTool.Result.CodeInfoList[0].Content.ToString();
                    if (eGradeUse)
                    {
                        //CNNCodeVerifyTool c2DCodeVerifyTool = new CNNCodeVerifyTool(MVD_ALGORITHM_PLATFORM_TYPE.MVD_ALGORITHM_PLATFORM_CPU);
                        c2DCodeVerifyTool.InputImage = cInputImg;
                        c2DCodeVerifyTool.BasicParam.RecognitionInfo = Code2DReaderTool.Result;
                        //c2DCodeVerifyTool.BasicParam.VerifyPrcType2D = MVD_SYMBOL_VERIFY_PROCESS_TYPE.MVD_SYMBOL_VERIFY_PROCESS_TYPE_I;
                        //c2DCodeVerifyTool.BasicParam.VerifyLabel2D = MVD_SYMBOL_VERIFY_LABEL.MVD_SYMBOL_VERIFY_LABEL_STANDARD;
                        //c2DCodeVerifyTool.BasicParam.VerifyStandard2D = MVD_SYMBOL_VERIFY_STANDARD.MVD_SYMBOL_VERIFY_ISO_STANDARD_29158;

                        c2DCodeVerifyTool.Run();
                        if (c2DCodeVerifyTool.Result.QualityInfoList.Count() > 0)
                        {

                            C2DCodeQualityInfoByISO29158 c2DCodeQualityInfoByISO29158
                                = (C2DCodeQualityInfoByISO29158)c2DCodeVerifyTool.Result.QualityInfoList[0];

                            C2DCodeReaderResult c2DCodeReaderResult
                                = c2DCodeVerifyTool.BasicParam.RecognitionInfo;
                            GetBarcodeItem.DCodeInfo = c2DCodeReaderResult.CodeInfoList[0];

                            GetBarcodeItem.OverGrade = _getBarcodeGrade(c2DCodeQualityInfoByISO29158.OverQuality);
                            GetBarcodeItem.DecodeGrade = _getBarcodeGrade(c2DCodeQualityInfoByISO29158.DecodeGrade);
                        }
                    }
                }

                //Code2DReaderTool.Dispose();
                //Code2DReaderTool = null;

                //c2DCodeVerifyTool.Dispose();
                //c2DCodeVerifyTool = null;

                cInputImg.Dispose();
                cInputImg = null;

            }
            catch (Exception ex)
            {
                retStr = string.Empty;
                m_ErrMsg = ex.StackTrace;
            }
            return retStr;
        }
        string _getBarcodeLocation(MVD_POINT_I mVD_POINT_I)
        {
            return mVD_POINT_I.nX.ToString() + ":" + mVD_POINT_I.nY.ToString();
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
        string _getBarcodeGrade(int ebyte)
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

        /// <summary>
        /// Bitmap 转成 CMvdImage
        /// </summary>
        /// <param name="bmpInputImg"></param>
        /// <returns></returns>
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
        }
    }
}


#endif