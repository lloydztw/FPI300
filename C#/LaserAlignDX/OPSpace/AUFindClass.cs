using AUVision;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisionDesigner.AlmightyPatMatch;
using VisionDesigner;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace LaserAlignDX.OPSpace
{
    public class FindParaClass
    {
        public float angle { get; set; } = 30;
        public int samplesize { get; set; } = 30;
        public int maxocc { get; set; } = 1;
        public float tolerance { get; set; } = 0.8f;
    }

    [Serializable]
    public class AUFindClass
    {
        /// <summary>
        /// 二值化的源图
        /// </summary>
        public Bitmap bmpItem;
        /// <summary>
        /// 需要Find的图
        /// </summary>
        public Bitmap bmpFind;
        /// <summary>
        /// 结果转正后的小图
        /// </summary>
        public Bitmap bmpResultMin;
        /// <summary>
        /// 结果转正后的大图
        /// </summary>
        public Bitmap bmpResultMax;
        /// <summary>
        /// Find分数
        /// </summary>
        public float fScore;

        public xFindResult xResult;
        AUFind xFindObj;
        xTrainingInfoF xInfo;
        /// <summary>
        /// 源图中的位置 
        /// </summary>
        public RectangleF Rect = new RectangleF();

        public List<xFindResult> xResults = new List<xFindResult>();

        VisionDesigner.AlmightyPatMatch.CAlmightyPattern cAlmightyPatternObj = null;
        VisionDesigner.AlmightyPatMatch.CAlmightyPatMatchTool cAlmightyPatmatchToolObj = null;
        FindParaClass findParaClass = null;

        VisionDesigner.GrayPatMatch.CGrayPattern cGrayPatternObj = null;
        VisionDesigner.GrayPatMatch.CGrayPatMatchTool cGrayPatMatchToolObj = null;

        FunTool m_FunTool = FunTool.AUFIND;

        public FunTool myFunTool
        {
            get { return m_FunTool; }
            set { m_FunTool = value; }
        }

        private long msDur = 0;
        public long DurMs
        {
            get { return msDur; }
            set { msDur = value; }
        }
        public bool Train(RectangleF Rectf, int iSize = 200)
        {
            Rect = Rectf;
            //    bmpItem.Save("D:\\bmpItem.png");
            AUGrayImg8 xTemplate = new AUGrayImg8();
            //载入AUImage中 
            AUUtility.DrawBitmapToAUGrayImg8(bmpItem, ref xTemplate);

            if (xFindObj == null)
                xFindObj = new AUFind();



            xFindObj.GetTrainingInfo(out xInfo); //获得默认训练设置
            xInfo.fRotationTolerance = 30; //设定旋转角度 : -180 ~ 180
            xInfo.fScalingTolerance = 0f; //设定缩放比例: 90% ~ 110%  
                                          //设定图片是否要缩小,如果不缩小则让它为最小边的值.

            xInfo.nDownSamplingSize = iSize;// xTemplate.GetWidth() > xTemplate.GetHeight() ? xTemplate.GetWidth() : xTemplate.GetHeight();
            xInfo.nCannyThresholdHigh = 200;
            xInfo.nCannyThresholdLow = 128;

            xInfo.eFMode = eFindMode.eFindMode_GHT;

            bool bol = xFindObj.Training(xTemplate, xInfo, true); //训练

            xFindObj.SetMaxOcc(24); //设定相似的最大数量
            xFindObj.SetTolerance(0.8f); //设定差异，设定范围为：0.1 ~ 1.0

            return bol;
        }
        public bool Train2(RectangleF Rectf, FindParaClass findPara)
        {
            bool bol = false;

            Rect = Rectf;
            findParaClass = findPara;

            switch (m_FunTool)
            {
                case FunTool.Gray:
                    bol = HikTrainGray();
                    break;
                case FunTool.HP:

                    bol = HikTrain2();

                    break;
                case FunTool.AUFIND:

                    //    bmpItem.Save("D:\\bmpItem.png");
                    AUGrayImg8 xTemplate = new AUGrayImg8();
                    //载入AUImage中 
                    AUUtility.DrawBitmapToAUGrayImg8(bmpItem, ref xTemplate);

                    if (xFindObj == null)
                        xFindObj = new AUFind();

                    xFindObj.GetTrainingInfo(out xInfo); //获得默认训练设置
                    xInfo.fRotationTolerance = findPara.angle; //设定旋转角度 : -180 ~ 180
                    xInfo.fScalingTolerance = 0f; //设定缩放比例: 90% ~ 110%  
                                                  //设定图片是否要缩小,如果不缩小则让它为最小边的值.

                    xInfo.nDownSamplingSize = findPara.samplesize;// xTemplate.GetWidth() > xTemplate.GetHeight() ? xTemplate.GetWidth() : xTemplate.GetHeight();
                    xInfo.nCannyThresholdHigh = 200;
                    xInfo.nCannyThresholdLow = 128;

                    xInfo.eFMode = eFindMode.eFindMode_GHT;

                    bol = xFindObj.Training(xTemplate, xInfo, true); //训练

                    xFindObj.SetMaxOcc(findPara.maxocc); //设定相似的最大数量
                    xFindObj.SetTolerance(findPara.tolerance); //设定差异，设定范围为：0.1 ~ 1.0

                    xTemplate.Dispose();

                    break;
            }
            return bol;
        }
        public bool HikTrain2()
        {
            bool bOK = false;

            #region HIK_TRAIN

            // CreatePatternInstance
            if (cAlmightyPatternObj == null)
                cAlmightyPatternObj = new VisionDesigner.AlmightyPatMatch.CAlmightyPattern();

            //Set type

            cAlmightyPatternObj.Type = PatMatchAlgorithmType.HPFeature;

            VisionDesigner.CMvdImage cInputImg = BitmapToCMvdImage(bmpItem);
            Bitmap bmp24 = new Bitmap(1, 1);
            if (bmpItem.PixelFormat != PixelFormat.Format8bppIndexed)
            {
                AForge.Imaging.Filters.Grayscale grayscale = new AForge.Imaging.Filters.Grayscale(0.299, 0.587, 0.114);
                bmp24 = grayscale.Apply(bmpItem);
                cInputImg = BitmapToCMvdImage(bmp24);
            }

            // Set input image
            //Bitmap bmp24 = bmppattern.Clone(new Rectangle(0, 0, bmppattern.Width, bmppattern.Height), PixelFormat.Format8bppIndexed);
            //AForge.Imaging.Filters.Grayscale grayscale = new AForge.Imaging.Filters.Grayscale(0.299, 0.587, 0.114);
            //Bitmap bmp24 = grayscale.Apply(bmpItem);
            //bmppattern.Save("D:\\Data\\bmp24" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + Universal.GlobalImageTypeString, Universal.GlobalImageFormat);

            //VisionDesigner.CMvdImage cInputImg = BitmapToCMvdImage(bmp24);
            if (cInputImg.PixelFormat != MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08)
            {
                //当前程序仅支持mono8。因此像素格会转换.
                cInputImg.ConvertImagePixelFormat(MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08);
            }

            string strvaluex = string.Empty;

            //cAlmightyPatternObj.SetRunParam("PyramidScaleFlag", "0");
            //cAlmightyPatternObj.SetRunParam("PyramidScaleRough", "10");
            //cAlmightyPatternObj.SetRunParam("PyramidScaleFine", "1");

            //cAlmightyPatternObj.SetRunParam("EdgeThresholdFlag", "0");
            //cAlmightyPatternObj.SetRunParam("EdgeThreshold", "80");

            //cAlmightyPatternObj.GetRunParam("PyramidScaleFlag", ref strvaluex);
            //cInputImg.InitImage("InputTest.bmp");

            cAlmightyPatternObj.InputImage = cInputImg;
            //cAlmightyPatternObj.RegionImage = cInputImg;
            // Set ROI region (optional)
            cAlmightyPatternObj.RegionList.Clear();
            float extendvalue = 1f;
            var region1 = new VisionDesigner.CMvdRectangleF(cInputImg.Width * 0.5f, cInputImg.Height * 0.5f, cInputImg.Width * 0.25f, cInputImg.Height * 0.25f);
            region1 = new VisionDesigner.CMvdRectangleF(cInputImg.Width * 0.5f, cInputImg.Height * 0.5f, cInputImg.Width * extendvalue, cInputImg.Height * extendvalue);

            cAlmightyPatternObj.RegionList.Add(new CAlmightyPatMatchRegion(region1, true));

            // Set basic parameter

            cAlmightyPatternObj.BasicParam.FixPoint = new VisionDesigner.MVD_POINT_F(Convert.ToSingle(cInputImg.Width * extendvalue) / 2, Convert.ToSingle(cInputImg.Height * extendvalue) / 2);

            string errmessage = string.Empty;
            // Train
            try
            {
                cAlmightyPatternObj.Train();
                bOK = true;
            }
            catch (Exception ex)
            {
                errmessage = ex.Message;
                bOK = false;
            }



            //// Export Pattern(optional)

            //cAlmightyPatternObj.ExportPattern("HPPattern.hpmxml");

            //// Import Pattern(optional)

            //cAlmightyPatternObj.ImportPattern("HPPattern.hpmxml");

            // Get the result

            //VisionDesigner.AlmightyPatMatch.CAlmightyPatternResult cAlmightyPatternRes = cAlmightyPatternObj.GetPatternResult();
            //cAlmightyPatternRes.TrainedImage.SaveImage("D:\\Data\\" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + Universal.GlobalImageTypeString);


            //Console.WriteLine("Pattern Size: ({0},{1})", cAlmightyPatternRes.Size.nWidth, cAlmightyPatternRes.Size.nHeight);

            #endregion

            return bOK;

        }
        public bool HikTrainGray()
        {
            bool bOK = false;

            #region HIK_TRAIN

            // CreatePatternInstance
            if (cGrayPatternObj == null)
                cGrayPatternObj = new VisionDesigner.GrayPatMatch.CGrayPattern();

            // Set input image
            VisionDesigner.CMvdImage cInputImg = BitmapToCMvdImage(bmpItem);
            Bitmap bmp24 = new Bitmap(1, 1);
            if (bmpItem.PixelFormat != PixelFormat.Format8bppIndexed)
            {
                AForge.Imaging.Filters.Grayscale grayscale = new AForge.Imaging.Filters.Grayscale(0.299, 0.587, 0.114);
                bmp24 = grayscale.Apply(bmpItem);
                cInputImg = BitmapToCMvdImage(bmp24);
            }

            //VisionDesigner.CMvdImage cInputImg = BitmapToCMvdImage(bmp24);
            if (cInputImg.PixelFormat != MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08)
            {
                //当前程序仅支持mono8。因此像素格会转换.
                cInputImg.ConvertImagePixelFormat(MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08);
            }

            // Set ROI region (optional)
            var cRectObj = new VisionDesigner.CMvdRectangleF(cInputImg.Width / 2, cInputImg.Height / 2, cInputImg.Width, cInputImg.Height);

            cGrayPatternObj.RegionList.Clear();
            cGrayPatternObj.RegionList.Add(new VisionDesigner.GrayPatMatch.CPatMatchRegion(cRectObj, true));

            // Set basic parameter
            cGrayPatternObj.BasicParam.FixPoint = new MVD_POINT_F(Convert.ToSingle(cInputImg.Width) / 2, Convert.ToSingle(cInputImg.Height) / 2);

            cGrayPatternObj.InputImage = cInputImg;

            string strvaluex = string.Empty;
            string errmessage = string.Empty;
            // Train
            try
            {
                // Train
                cGrayPatternObj.Train();
                bOK = true;
            }
            catch (Exception ex)
            {
                errmessage = ex.Message;
                bOK = false;
            }
            #endregion

            return bOK;

        }
        public void Find(Bitmap bmp)
        {

            AUGrayImg8 mySrcImg = new AUGrayImg8();
            AUUtility.DrawBitmapToAUGrayImg8(bmp, ref mySrcImg);
            xFindObj.Find(mySrcImg);

            if (xFindObj.GetResultCount() > 0)
            {
                xFindResult xr = xFindObj.GetResult(0);
                fScore = xr.fScore;
            }

        }
        public bool Find()
        {
            msDur = 0;
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            //stopwatch.Start();

            fScore = 0;
            AUGrayImg8 mySrcImg = new AUGrayImg8();
            AUUtility.DrawBitmapToAUGrayImg8(bmpFind, ref mySrcImg);
            stopwatch.Restart();
            xFindObj.Find(mySrcImg);
            stopwatch.Stop();
            msDur = stopwatch.ElapsedMilliseconds;
            if (xFindObj.GetResultCount() > 0)
            {
                float iResultMax = -1;
                for (int i = 0; i < xFindObj.GetResultCount(); i++)
                {
                    xFindResult ResultTemp = xFindObj.GetResult(i);

                    //if (xResult.fAngle > 2 || xResult.fAngle < -2)
                    //    xResult.fAngle = 0;

                    if (ResultTemp.fScore > iResultMax)
                    {
                        iResultMax = ResultTemp.fScore;
                        fScore = ResultTemp.fScore;
                        xResult = ResultTemp;
                    }
                }

                AUColorImg24 imginput24 = new AUColorImg24(bmpFind.Width, bmpFind.Height);
                AUUtility.DrawBitmapToAUColorImg24(bmpFind, ref imginput24);


                AUColorImg24 imgoutput24 = new AUColorImg24(bmpItem.Width, bmpItem.Height);
                AUUtility.DrawBitmapToAUColorImg24(bmpItem, ref imgoutput24);
                //转正结果小图
                ScaleRotateEX2(xResult, imginput24, ref imgoutput24);
                bmpResultMin = new Bitmap(imgoutput24.GetWidth(), imgoutput24.GetHeight());
                AUUtility.DrawAUColorImg24ToBitmap(imgoutput24, ref bmpResultMin);



                imgoutput24 = new AUColorImg24(bmpFind.Width, bmpFind.Height);
                AUUtility.DrawBitmapToAUColorImg24(bmpFind, ref imgoutput24);
                //转正结果的大图
                ScaleRotate(xResult, imginput24, ref imgoutput24);
                bmpResultMax = new Bitmap(imgoutput24.GetWidth(), imgoutput24.GetHeight());
                AUUtility.DrawAUColorImg24ToBitmap(imgoutput24, ref bmpResultMax);

                return true;
            }
            return false;
        }
        public bool FindLarge()
        {

            bool bOK = false;

            xResults.Clear();
            msDur = 0;
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            //stopwatch.Start();
            stopwatch.Restart();

            switch (m_FunTool)
            {
                case FunTool.Gray:

                    bOK = HikRunGray();

                    break;
                case FunTool.HP:

                    bOK = HikRun2();

                    break;
                case FunTool.AUFIND:

                    fScore = 0;
                    AUGrayImg8 mySrcImg = new AUGrayImg8();
                    AUUtility.DrawBitmapToAUGrayImg8(bmpFind, ref mySrcImg);
                    xFindObj.Find(mySrcImg);
                    int findCount = xFindObj.GetResultCount();
                    mySrcImg.Dispose();

                    if (xFindObj.GetResultCount() > 0)
                    {
                        float iResultMax = -1;
                        for (int i = 0; i < xFindObj.GetResultCount(); i++)
                        {
                            if (xFindObj.GetResult(i).fScore >= findParaClass.tolerance)
                            {
                                xFindResult ResultTemp = xFindObj.GetResult(i);
                                xResults.Add(ResultTemp);
                            }
                        }
                        //bOK = true;
                    }

                    bOK = xResults.Count > 0;
                    break;
            }

            stopwatch.Stop();
            msDur = stopwatch.ElapsedMilliseconds;
            return bOK;
        }
        public bool HikRun2()
        {
            bool bOK = false;

            #region HIK_RUN

            // CreateToolInstance
            if (cAlmightyPatmatchToolObj == null)
                cAlmightyPatmatchToolObj = new VisionDesigner.AlmightyPatMatch.CAlmightyPatMatchTool();

            //Set type

            cAlmightyPatmatchToolObj.Type = PatMatchAlgorithmType.HPFeature;

            //// Set ROI region (optional)
            cAlmightyPatmatchToolObj.RegionList.Clear();
            var region2 = new VisionDesigner.CMvdRectangleF(bmpFind.Width * 0.5f, bmpFind.Height * 0.5f, bmpFind.Width, bmpFind.Height);
            cAlmightyPatmatchToolObj.RegionList.Add(new CAlmightyPatMatchRegion(region2, true));

            // Set basic parameter

            cAlmightyPatmatchToolObj.BasicParam.ShowOutlineStatus = false;

            // Set Pattern

            cAlmightyPatmatchToolObj.Pattern = cAlmightyPatternObj;

            cAlmightyPatmatchToolObj.SetRunParam("AngleStart", (-findParaClass.angle).ToString());
            cAlmightyPatmatchToolObj.SetRunParam("AngleEnd", findParaClass.angle.ToString());
            cAlmightyPatmatchToolObj.SetRunParam("MinScore", findParaClass.tolerance.ToString());

            VisionDesigner.CMvdImage cInputImg2 = BitmapToCMvdImage(bmpFind);
            Bitmap bmp24 = new Bitmap(1, 1);
            if (bmpFind.PixelFormat != PixelFormat.Format8bppIndexed)
            {
                AForge.Imaging.Filters.Grayscale grayscale = new AForge.Imaging.Filters.Grayscale(0.299, 0.587, 0.114);
                bmp24 = grayscale.Apply(bmpFind);
                cInputImg2 = BitmapToCMvdImage(bmp24);
            }
            // Set input image
            //Bitmap bmp24 = bmpinput.Clone(new Rectangle(0, 0, bmpinput.Width, bmpinput.Height), PixelFormat.Format8bppIndexed);

            if (cInputImg2.PixelFormat != MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08)
            {
                //当前程序仅支持mono8。因此像素格会转换.
                cInputImg2.ConvertImagePixelFormat(MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08);
            }
            //cInputImg2.InitImage("InputTest2.bmp");

            cAlmightyPatmatchToolObj.InputImage = cInputImg2;

            // Running

            cAlmightyPatmatchToolObj.Run();

            // Get the result

            VisionDesigner.AlmightyPatMatch.CAlmightyPatMatchResult cHPMatchRes = cAlmightyPatmatchToolObj.Result;

            //cAlmightyPatmatchToolObj.InputImage.SaveImage("D:\\Data\\matchrun_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + Universal.GlobalImageTypeString);

            //foreach (var item in cHPMatchRes.MatchInfoList)

            //{

            //    Console.WriteLine("MatchPoint: ({0},{1})", item.MatchPoint.fX, item.MatchPoint.fY);

            //}
            int resultcount = cHPMatchRes.MatchInfoList.Count;
            if (resultcount > 0)
            {
                foreach (var item in cHPMatchRes.MatchInfoList)
                {
                    //Console.WriteLine("MatchPoint: ({0},{1})", item.MatchPoint.fX, item.MatchPoint.fY);
                    if (item.Score >= findParaClass.tolerance)
                    {
                        xFindResult result = new xFindResult();
                        //var item = cHPMatchRes.MatchInfoList[0];
                        result.fCenterX = item.MatchBox.CenterX;
                        result.fCenterY = item.MatchBox.CenterY;
                        result.fAngle = item.MatchBox.Angle;
                        result.fScale = item.Scale;
                        result.fScore = item.Score;

                        xResults.Add(result);
                    }

                }

            }

            bOK = xResults.Count > 0;

            //if (xResults.Count > 0)
            //{
            //    bOK = xResults[0].fScore < findParaClass.tolerance;
            //}
            //else
            //{
            //    bOK = false;
            //}

            bmp24.Dispose();

            #endregion

            return bOK;
        }
        public bool HikRunGray()
        {
            bool bOK = false;

            #region HIK_RUN

            // CreateToolInstance
            if (cGrayPatMatchToolObj == null)
                cGrayPatMatchToolObj = new VisionDesigner.GrayPatMatch.CGrayPatMatchTool();

            VisionDesigner.CMvdImage cInputImg2 = BitmapToCMvdImage(bmpFind);
            Bitmap bmp24 = new Bitmap(1, 1);
            if (bmpFind.PixelFormat != PixelFormat.Format8bppIndexed)
            {
                AForge.Imaging.Filters.Grayscale grayscale = new AForge.Imaging.Filters.Grayscale(0.299, 0.587, 0.114);
                bmp24 = grayscale.Apply(bmpFind);
                cInputImg2 = BitmapToCMvdImage(bmp24);
            }
            // Set input image
            //Bitmap bmp24 = bmpinput.Clone(new Rectangle(0, 0, bmpinput.Width, bmpinput.Height), PixelFormat.Format8bppIndexed);

            if (cInputImg2.PixelFormat != MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08)
            {
                //当前程序仅支持mono8。因此像素格会转换.
                cInputImg2.ConvertImagePixelFormat(MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08);
            }

            // Set input image
            cGrayPatMatchToolObj.InputImage = cInputImg2;

            // Set ROI region (optional)
            cGrayPatMatchToolObj.ROI = new VisionDesigner.CMvdRectangleF(cInputImg2.Width / 2, cInputImg2.Height / 2, cInputImg2.Width, cInputImg2.Height);

            // Set Pattern
            cGrayPatMatchToolObj.Pattern = cGrayPatternObj;

            cGrayPatMatchToolObj.SetRunParam("AngleStart", (-findParaClass.angle).ToString());
            cGrayPatMatchToolObj.SetRunParam("AngleEnd", findParaClass.angle.ToString());
            cGrayPatMatchToolObj.SetRunParam("MinScore", findParaClass.tolerance.ToString());
            cGrayPatMatchToolObj.SetRunParam("AngleStep", "2");

            // Running
            cGrayPatMatchToolObj.Run();

            // Get the result
            VisionDesigner.GrayPatMatch.CGrayPatMatchResult cGrayMatchRes = cGrayPatMatchToolObj.Result;
            //foreach (var item in cGrayMatchRes.MatchInfoList)
            //{
            //    Console.WriteLine("MatchPoint: ({0},{1})", item.MatchPoint.fX, item.MatchPoint.fY);
            //}


            int resultcount = cGrayMatchRes.MatchInfoList.Count;
            if (resultcount > 0)
            {
                foreach (var item in cGrayMatchRes.MatchInfoList)
                {
                    //Console.WriteLine("MatchPoint: ({0},{1})", item.MatchPoint.fX, item.MatchPoint.fY);
                    if (item.Score >= findParaClass.tolerance)
                    {
                        xFindResult result = new xFindResult();
                        //var item = cHPMatchRes.MatchInfoList[0];
                        result.fCenterX = item.MatchBox.CenterX;
                        result.fCenterY = item.MatchBox.CenterY;
                        result.fAngle = item.MatchBox.Angle;
                        //result.fScale = item.Scale;
                        result.fScore = item.Score;

                        xResults.Add(result);
                    }
                }
            }

            bOK = xResults.Count > 0;
            bmp24.Dispose();

            #endregion

            return bOK;
        }

        /// <summary>
        /// 算出偏移及旋轉值
        /// </summary>
        /// <param name="result"></param>
        /// <param name="imginput24"></param>
        /// <param name="imgoutput24"></param>
        public void ScaleRotate(xFindResult result, AUColorImg24 imginput24, ref AUColorImg24 imgoutput24)
        {
            AUImage.ScaleRotate(imginput24, imgoutput24,
                               result.fCenterX, result.fCenterY,
                               Rect.X + Rect.Width / 2, Rect.Y + Rect.Height / 2,
                              result.fAngle, 1.0f, 1.0f, eInterpolationBits.eInterpolationBits_8);

        }
        /// <summary>
        /// 算出偏移及旋轉值
        /// </summary>
        /// <param name="result"></param>
        /// <param name="imginput24"></param>
        /// <param name="imgoutput24"></param>
        public void ScaleRotateEX2(xFindResult result, AUColorImg24 imginput24, ref AUColorImg24 imgoutput24)
        {

            float fSrcCX = result.fCenterX; //旋轉中心 X
            float fSrcCY = result.fCenterY; //旋轉中心 Y
            float fDstCX = imgoutput24.GetWidth() / 2; //目標影像中心 X
            float fDstCY = imgoutput24.GetHeight() / 2; //目標影像中心 Y

            //eInterpolationBits_1,4,8 8 for best but slowest
            AUImage.ScaleRotate(imginput24, imgoutput24,
                        //Result.fCenterX, Result.fCenterY,
                        fSrcCX, fSrcCY,
                        fDstCX, fDstCY,
                        result.fAngle,
                        1.0f,
                        1.0f,
                        eInterpolationBits.eInterpolationBits_8);

            //取得旋轉和偏移的值

            //   Rotation = result.fAngle;
            //Offset = (float)Math.Sqrt(Math.Pow(fX, 2) + Math.Pow(fY, 2));

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
            if (xFindObj != null)
            {
                //xFindObj.Dispose();
                xFindObj = null;
            }
            if (cAlmightyPatmatchToolObj != null)
            {
                cAlmightyPatmatchToolObj.Dispose();
                cAlmightyPatmatchToolObj = null;
            }
            if (cAlmightyPatternObj != null)
            {
                cAlmightyPatternObj.Dispose();
                cAlmightyPatternObj = null;
            }
            if (cGrayPatternObj != null)
            {
                cGrayPatternObj.Dispose();
                cGrayPatternObj = null;
            }
            if (cGrayPatMatchToolObj != null)
            {
                cGrayPatMatchToolObj.Dispose();
                cGrayPatMatchToolObj = null;
            }
        }
    }
}
