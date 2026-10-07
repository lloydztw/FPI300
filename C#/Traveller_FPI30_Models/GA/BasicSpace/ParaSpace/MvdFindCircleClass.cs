using Common.RecipeSpace;
using JetEazy;
using JetEazy.BasicSpace;
using JetEazy.PropertyGrid;
using LaserAlignDX.OPSpace;
using LaserAlignDX.OPSpace.RecipeSpace;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using VisionDesigner;

namespace LaserAlignDX.BasicSpace.ParaSpace
{
    public class MvdFindCircleClass : RecipeBaseClass
    {
        public MvdFindCircleClass() { }

        private static MvdFindCircleClass _instance = null;
        public static MvdFindCircleClass Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new MvdFindCircleClass();
                return _instance;
            }
        }

        const string LSCat00 = "A00.基础参数设定";

        [CategoryAttribute(LSCat00), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A01.亮度")]
        [Browsable(true)]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 255)]
        public int Brightness { get; set; } = 0;
        [CategoryAttribute(LSCat00), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A02.对比")]
        [Browsable(true)]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 255)]
        public int Contrast { get; set; } = 0;
        [CategoryAttribute(LSCat00), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A03.灰度阈值")]
        [Browsable(true)]
        public int ThresholdValue { get; set; } = 60;
        [CategoryAttribute(LSCat00), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A04.找白斑")]
        [Browsable(true)]
        public bool IsFindWhite { get; set; } = false;
        [CategoryAttribute(LSCat00), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A05.寻找方式")]
        [Browsable(true)]
        //[Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999)]
        public FindType mFindType { get; set; } = FindType.CIRCLE;

        const string LSCat01 = "A01.找圆参数设定";
        [CategoryAttribute(LSCat01), DescriptionAttribute("滤波核半宽：用于增强边缘和抑制噪声，最小值为1。当边缘模糊或有噪声干扰时，增大该值有利于使得检测结果更加稳定，但如果边缘与边缘之间距离小于滤波尺寸时反而会影响边缘位置的精度甚至丢失边缘，该值须要根据实际情况设置。")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A01.滤波核半宽")]
        [Browsable(true)]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(1, 50)]
        public int CircleEdgeWidth { get; set; } = 2;

        [CategoryAttribute(LSCat01), DescriptionAttribute("圆最小半径")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A02.圆最小半径")]
        [Browsable(true)]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(1, 1000)]
        public int CircleMinRadius { get; set; } = 10;
        [CategoryAttribute(LSCat01), DescriptionAttribute("圆最大半径")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A03.圆最大半径")]
        [Browsable(true)]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(1, 10000)]
        public int CircleMaxRadius { get; set; } = 100;
        [CategoryAttribute(LSCat01), DescriptionAttribute("卡尺数量：用于扫描边缘点的ROI区域数量。")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A04.卡尺数量")]
        [Browsable(true)]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(3, 1000)]
        public int CircleRadNum { get; set; } = 30;

        public override void Initial(string epath, int ercpindex, string enamefile)
        {
            base.Initial(epath, ercpindex, enamefile);
            string dir = $"{Path}";
            INIFILE = $"{dir}\\{Name}";
        }
        public override void ChangeIndex(int eindex)
        {
            //base.ChangeIndex(eindex);
        }

        public override void Load(bool eCancel = false)
        {
            Brightness = int.Parse(ReadINIValue("Basic", "Brightness", "0", INIFILE));
            Contrast = int.Parse(ReadINIValue("Basic", "Contrast", "0", INIFILE));
            ThresholdValue = int.Parse(ReadINIValue("Basic", "ThresholdValue", "128", INIFILE));
            IsFindWhite = ReadINIValue("Basic", "IsFindWhite", "0", INIFILE) == "1";
            mFindType = (FindType)int.Parse(ReadINIValue("Basic", "mFindType", "1", INIFILE));
            CircleEdgeWidth = int.Parse(ReadINIValue("Basic", "CircleEdgeWidth", "2", INIFILE));
            CircleMinRadius = int.Parse(ReadINIValue("Basic", "CircleMinRadius", "10", INIFILE));
            CircleMaxRadius = int.Parse(ReadINIValue("Basic", "CircleMaxRadius", "100", INIFILE));
            CircleRadNum = int.Parse(ReadINIValue("Basic", "CircleRadNum", "30", INIFILE));

        }
        public override void Save()
        {
            WriteINIValue("Basic", "Brightness", Brightness.ToString(), INIFILE);
            WriteINIValue("Basic", "Contrast", Contrast.ToString(), INIFILE);
            WriteINIValue("Basic", "ThresholdValue", ThresholdValue.ToString(), INIFILE);
            WriteINIValue("Basic", "IsFindWhite", (IsFindWhite ? "1" : "0"), INIFILE);
            WriteINIValue("Basic", "mFindType", ((int)mFindType).ToString(), INIFILE);
            WriteINIValue("Basic", "CircleEdgeWidth", CircleEdgeWidth.ToString(), INIFILE);
            WriteINIValue("Basic", "CircleMinRadius", CircleMinRadius.ToString(), INIFILE);
            WriteINIValue("Basic", "CircleMaxRadius", CircleMaxRadius.ToString(), INIFILE);
            WriteINIValue("Basic", "CircleRadNum", CircleRadNum.ToString(), INIFILE);
        }


        #region 预处理计算中心
        JzFindObjectClass m_Find = new JzFindObjectClass();
        /// <summary>
        /// 寻找图片的blob中心
        /// </summary>
        /// <param name="bmpinput">输入图像</param>
        /// <param name="maxrect">返回图像中最大的blob矩形</param>
        /// <returns>返回中心坐标</returns>
        public PointF GetImageMarkCenter(Bitmap bmpinput, bool istrain, out RectangleF maxrect, out Bitmap bmpoutput, out int maxarea)
        {
            PointF ret = new PointF(bmpinput.Width / 2, bmpinput.Height / 2);
            maxrect = new RectangleF(1, 1, bmpinput.Width - 2, bmpinput.Height - 2);
            bmpoutput = new Bitmap(bmpinput);
            maxarea = -1;

            //预处理
            Bitmap bmptemp = new Bitmap(bmpinput);
            AForge.Imaging.Filters.Grayscale grayscale = new AForge.Imaging.Filters.Grayscale(0.299, 0.587, 0.114);
            bmptemp = grayscale.Apply(bmptemp);
            AForge.Imaging.Filters.BrightnessCorrection brightnessCorrection = new AForge.Imaging.Filters.BrightnessCorrection(Brightness);
            bmptemp = brightnessCorrection.Apply(bmptemp);
            AForge.Imaging.Filters.ContrastCorrection contrastCorrection = new AForge.Imaging.Filters.ContrastCorrection(Contrast);
            bmptemp = contrastCorrection.Apply(bmptemp);

            Bitmap bmptempDraw = new Bitmap(bmpinput);

            switch (mFindType)
            {
                case FindType.CIRCLE:

                    #region 找圆的算法

                    try

                    {

                        // CreateInstance

                        VisionDesigner.CircleFind.CCircleFindTool cCircleFindToolObj = new VisionDesigner.CircleFind.CCircleFindTool();

                        // Set input image

                        //VisionDesigner.CMvdImage cInputImg = new CMvdImage();

                        //cInputImg.InitImage("InputTest.bmp");
                        VisionDesigner.CMvdImage cInputImg = BitmapToCMvdImage(bmptemp);
                        if (cInputImg.PixelFormat != MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08)
                        {
                            //当前程序仅支持mono8。因此像素格会转换.
                            cInputImg.ConvertImagePixelFormat(MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08);
                        }
                        //cInputImg2.InitImage("InputTest2.bmp");


                        cCircleFindToolObj.InputImage = cInputImg;

                        // Set ROI region (optional)

                        cCircleFindToolObj.ROI = new VisionDesigner.CMvdRectangleF(cInputImg.Width / 2, cInputImg.Height / 2, cInputImg.Width, cInputImg.Height);

                        // Set basic parameter

                        cCircleFindToolObj.BasicParam.CoarseCenter = new MVD_POINT_I(Convert.ToInt32(cInputImg.Width) / 2, Convert.ToInt32(cInputImg.Height) / 2);

                        cCircleFindToolObj.SetRunParam("EdgePolarity", "Both");
                        cCircleFindToolObj.SetRunParam("EdgeWidth", $"{CircleEdgeWidth}");
                        cCircleFindToolObj.SetRunParam("MinRadius", $"{CircleMinRadius}");
                        cCircleFindToolObj.SetRunParam("MaxRadius", $"{CircleMaxRadius}");
                        cCircleFindToolObj.SetRunParam("RadNum", $"{CircleRadNum}");
                        cCircleFindToolObj.SetRunParam("EdgeThresh", $"{ThresholdValue}");

                        // Running

                        cCircleFindToolObj.Run();

                        // Get the result

                        VisionDesigner.CircleFind.CCircleFindResult cCircleFindRes = cCircleFindToolObj.Result;

                        Console.WriteLine("Recognition status: {0}", cCircleFindRes.Status);

                        if (cCircleFindRes.Status == 1)
                        {
                            ret = new PointF(cCircleFindRes.Circle.Center.fX, cCircleFindRes.Circle.Center.fY);
                            maxrect = new RectangleF(cCircleFindRes.Circle.Center.fX - cCircleFindRes.Circle.Radius,
                                cCircleFindRes.Circle.Center.fY - cCircleFindRes.Circle.Radius, cCircleFindRes.Circle.Radius * 2, cCircleFindRes.Circle.Radius * 2);
                            maxarea = (int)cCircleFindRes.CircleAreaRate;

                            RectangleF rectangleFmarktemp = SimpleRectF(ret, 1, 1);
                            float _lineWidth = 1f;
                            Graphics graphics = Graphics.FromImage(bmptempDraw);
                            graphics.DrawRectangles(new Pen(Color.Lime, _lineWidth), new RectangleF[] { rectangleFmarktemp });
                            graphics.DrawRectangles(new Pen(Color.Red, _lineWidth), new RectangleF[] { maxrect });
                            graphics.DrawEllipse(new Pen(Color.Lime, _lineWidth), maxrect);
                            graphics.DrawString($"Radius:{cCircleFindRes.Circle.Radius}", new Font("宋体", 10f), Brushes.Lime, new PointF(5, 5));
                            graphics.Dispose();

                            //if (istrain)
                            //{
                            //    org_width = maxrect.Width;
                            //    org_height = maxrect.Height;
                            //    org_area = maxarea;
                            //}
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

                    break;
                default:
                    #region 找blob算法

                    //AForge.Imaging.Filters.HistogramEqualization histogramEqualization = new AForge.Imaging.Filters.HistogramEqualization();
                    //bmptemp = histogramEqualization.Apply(bmptemp);

                    m_Find.AH_SetThreshold(ref bmptemp, ThresholdValue);
                    m_Find.AH_FindBlob(bmptemp, IsFindWhite);
                    if (m_Find.FoundList.Count > 0)
                    {
                        int maxindex = m_Find.GetMaxRectIndex();
                        ret = new PointF((float)m_Find.FoundList[maxindex].rotatedRectangleF.fCX,
                                         (float)m_Find.FoundList[maxindex].rotatedRectangleF.fCY);
                        maxrect = new RectangleF(m_Find.rectMaxRect.X, m_Find.rectMaxRect.Y, m_Find.rectMaxRect.Width, m_Find.rectMaxRect.Height);
                        maxarea = m_Find.FoundList[maxindex].Area;

                        RectangleF rectangleFmarktemp = SimpleRectF(ret, 2, 2);
                        float _lineWidth = 11f;
                        Graphics graphics = Graphics.FromImage(bmptempDraw);
                        graphics.DrawRectangles(new Pen(Color.Lime, _lineWidth), new RectangleF[] { rectangleFmarktemp });
                        graphics.DrawRectangles(new Pen(Color.Red, _lineWidth), new RectangleF[] { maxrect });
                        graphics.Dispose();

                        //if (istrain)
                        //{
                        //    org_width = maxrect.Width;
                        //    org_height = maxrect.Height;
                        //    org_area = m_Find.FoundList[maxindex].Area;
                        //}

                    }

                    #endregion
                    break;
            }


            bmpoutput.Dispose();
            bmpoutput = new Bitmap(bmptempDraw);
            bmptemp.Dispose();
            bmptempDraw.Dispose();
            return ret;
        }

        void _preImage(ref Bitmap bmptemp)
        {
            //预处理
            //AForge.Imaging.Filters.Grayscale grayscale = new AForge.Imaging.Filters.Grayscale(0.299, 0.587, 0.114);
            //bmptemp = grayscale.Apply(bmptemp);
            ////AForge.Imaging.Filters.BrightnessCorrection brightnessCorrection = new AForge.Imaging.Filters.BrightnessCorrection(Brightness);
            ////bmptemp = brightnessCorrection.Apply(bmptemp);
            ////AForge.Imaging.Filters.ContrastCorrection contrastCorrection = new AForge.Imaging.Filters.ContrastCorrection(Contrast);
            ////bmptemp = contrastCorrection.Apply(bmptemp);
            //AForge.Imaging.Filters.CannyEdgeDetector cannyEdgeDetector = new AForge.Imaging.Filters.CannyEdgeDetector();
            //cannyEdgeDetector.HighThreshold = 100;
            //cannyEdgeDetector.LowThreshold = 20;
            //bmptemp = cannyEdgeDetector.Apply(bmptemp);

            //AForge.Imaging.Filters.Threshold threshold = new AForge.Imaging.Filters.Threshold();
            //bmptemp = threshold.Apply(bmptemp);
        }

        RectangleF SimpleRectF(PointF Pt, int Width, int Height)
        {
            RectangleF rect = SimpleRectF(Pt);
            rect.Inflate(Width, Height);

            return rect;
        }
        RectangleF SimpleRectF(PointF Pt)
        {
            return new RectangleF(Pt.X, Pt.Y, 1, 1);
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

        #endregion

        private string ToChangeLanguage(string eText)
        {
            string retStr = eText;
            retStr = LanguageExClass.Instance.GetLanguageText(eText);
            return retStr;
        }


    }
}
