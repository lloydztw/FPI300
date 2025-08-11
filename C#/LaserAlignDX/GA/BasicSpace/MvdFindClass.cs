using LaserAlignDX.OPSpace;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Menu;
using VisionDesigner.AlmightyPatMatch;
using VisionDesigner;
using AUVision;
using ZXing;
using System.Drawing;
using System.Runtime.InteropServices;

namespace LaserAlignDX.BasicSpace
{
    public class MvdFindClass : IDisposable
    {
        public Bitmap bmpObj_Image = null;
        public Bitmap bmpRun_Image = null;
        public CMvdImage xMvdObj_Image = null;
        public CMvdImage xMvdRun_Image = null;

        VisionDesigner.AlmightyPatMatch.CAlmightyPattern cAlmightyPatternObj = null;
        VisionDesigner.AlmightyPatMatch.CAlmightyPatMatchTool cAlmightyPatmatchToolObj = null;

        public List<xFindResult> xResults = new List<xFindResult>();
        public float xMvdAngle { get; set; } = 5;
        public int xMvdMaxOcc { get; set; } = 1;
        public float xMvdTolerance { get; set; } = 0.5f;
        public PointF xMvdFixed { get; set; } = new PointF(-1, -1);
        public int xMaxOverlap { get; set; } = 80;

        //VisionDesigner.GrayPatMatch.CGrayPattern cGrayPatternObj = null;
        //VisionDesigner.GrayPatMatch.CGrayPatMatchTool cGrayPatMatchToolObj = null;

        public MvdFindClass() { }
        ~MvdFindClass()
        {
            Dispose();
        }
        public bool HikTrainBmp()
        {
            xMvdObj_Image?.Dispose();
            xMvdObj_Image = BitmapToCMvdImage(bmpObj_Image);
            return HikTrain2();
        }
        public bool HikTrainBmp(CMvdRectangleF cMvdRectangleF)
        {
            xMvdObj_Image?.Dispose();
            xMvdObj_Image = BitmapToCMvdImage(bmpObj_Image);
            return HikTrain2(cMvdRectangleF);
        }
        public bool HikRunBmp()
        {
            xMvdRun_Image?.Dispose();
            xMvdRun_Image = BitmapToCMvdImage(bmpRun_Image);
            return HikRun2();
        }
        public bool HikTrain2()
        {
            bool bOK = false;
            string errmessage = string.Empty;

            #region HIK_TRAIN
            try
            {
                // CreatePatternInstance
                if (cAlmightyPatternObj == null)
                    cAlmightyPatternObj = new VisionDesigner.AlmightyPatMatch.CAlmightyPattern();

                //Set type
                cAlmightyPatternObj.Type = PatMatchAlgorithmType.HPFeature;
                //cAlmightyPatternObj.Type = PatMatchAlgorithmType.FastFeature;

                if (xMvdObj_Image.PixelFormat != MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08)
                {
                    //当前程序仅支持mono8。因此像素格会转换.
                    xMvdObj_Image.ConvertImagePixelFormat(MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08);
                }
                cAlmightyPatternObj.InputImage = xMvdObj_Image;
                // Set ROI region (optional)
                cAlmightyPatternObj.RegionList.Clear();
                float extendvalue = 1f;
                var region1 = new VisionDesigner.CMvdRectangleF(xMvdObj_Image.Width * 0.5f,
                    xMvdObj_Image.Height * 0.5f,
                    xMvdObj_Image.Width * extendvalue,
                    xMvdObj_Image.Height * extendvalue);

                cAlmightyPatternObj.RegionList.Add(new CAlmightyPatMatchRegion(region1, true));

                // Set basic parameter

                //cAlmightyPatternObj.BasicParam.FixPoint =
                //    new VisionDesigner.MVD_POINT_F(Convert.ToSingle(xMvdObj_Image.Width * extendvalue) / 2,
                //                                                               Convert.ToSingle(xMvdObj_Image.Height * extendvalue) / 2);

                if (xMvdFixed.X < 0 || xMvdFixed.X > xMvdObj_Image.Width || xMvdFixed.Y < 0 || xMvdFixed.Y > xMvdObj_Image.Height)
                {
                    cAlmightyPatternObj.BasicParam.FixPoint =
                        new VisionDesigner.MVD_POINT_F(Convert.ToSingle(xMvdObj_Image.Width * extendvalue) / 2,
                                                                                   Convert.ToSingle(xMvdObj_Image.Height * extendvalue) / 2);

                }
                else
                {
                    cAlmightyPatternObj.BasicParam.FixPoint =
                                    new VisionDesigner.MVD_POINT_F(xMvdFixed.X, xMvdFixed.Y);
                }

                // Train

                cAlmightyPatternObj.Train();
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
        public bool HikTrain2(CMvdRectangleF cMvdRectangleF)
        {
            bool bOK = false;
            string errmessage = string.Empty;

            #region HIK_TRAIN
            try
            {
                // CreatePatternInstance
                if (cAlmightyPatternObj == null)
                    cAlmightyPatternObj = new VisionDesigner.AlmightyPatMatch.CAlmightyPattern();

                //Set type
                cAlmightyPatternObj.Type = PatMatchAlgorithmType.HPFeature;
                //cAlmightyPatternObj.Type = PatMatchAlgorithmType.FastFeature;

                if (xMvdObj_Image.PixelFormat != MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08)
                {
                    //当前程序仅支持mono8。因此像素格会转换.
                    xMvdObj_Image.ConvertImagePixelFormat(MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08);
                }
                cAlmightyPatternObj.InputImage = xMvdObj_Image;
                // Set ROI region (optional)
                cAlmightyPatternObj.RegionList.Clear();
                var region1 = cMvdRectangleF;

                cAlmightyPatternObj.RegionList.Add(new CAlmightyPatMatchRegion(region1, true));

                // Set basic parameter

                cAlmightyPatternObj.BasicParam.FixPoint =
                    new VisionDesigner.MVD_POINT_F(cMvdRectangleF.CenterX, cMvdRectangleF.CenterY);

                // Train

                cAlmightyPatternObj.Train();
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
        public bool HikRun2()
        {
            bool bOK = HikRun3(new RectangleF(0, 0, bmpRun_Image.Width, bmpRun_Image.Height));

            //xResults.Clear();

            //#region HIK_RUN

            //// CreateToolInstance
            //if (cAlmightyPatmatchToolObj == null)
            //    cAlmightyPatmatchToolObj = new VisionDesigner.AlmightyPatMatch.CAlmightyPatMatchTool();

            ////Set type

            //cAlmightyPatmatchToolObj.Type = PatMatchAlgorithmType.HPFeature;

            ////// Set ROI region (optional)
            //cAlmightyPatmatchToolObj.RegionList.Clear();
            //var region2 = new VisionDesigner.CMvdRectangleF(xMvdRun_Image.Width * 0.5f,
            //    xMvdRun_Image.Height * 0.5f,
            //    xMvdRun_Image.Width,
            //    xMvdRun_Image.Height);
            //cAlmightyPatmatchToolObj.RegionList.Add(new CAlmightyPatMatchRegion(region2, true));

            //// Set basic parameter

            //cAlmightyPatmatchToolObj.BasicParam.ShowOutlineStatus = false;

            //// Set Pattern

            //cAlmightyPatmatchToolObj.Pattern = cAlmightyPatternObj;

            //cAlmightyPatmatchToolObj.SetRunParam("AngleStart", (-xMvdAngle).ToString());
            //cAlmightyPatmatchToolObj.SetRunParam("AngleEnd", xMvdAngle.ToString());
            //cAlmightyPatmatchToolObj.SetRunParam("MinScore", xMvdTolerance.ToString());

            //if (xMvdRun_Image.PixelFormat != MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08)
            //{
            //    //当前程序仅支持mono8。因此像素格会转换.
            //    xMvdRun_Image.ConvertImagePixelFormat(MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08);
            //}
            //cAlmightyPatmatchToolObj.InputImage = xMvdRun_Image;

            //// Running

            //cAlmightyPatmatchToolObj.Run();

            //// Get the result

            //VisionDesigner.AlmightyPatMatch.CAlmightyPatMatchResult cHPMatchRes = cAlmightyPatmatchToolObj.Result;

            ////foreach (var item in cHPMatchRes.MatchInfoList)

            ////{

            ////    Console.WriteLine("MatchPoint: ({0},{1})", item.MatchPoint.fX, item.MatchPoint.fY);

            ////}
            //int resultcount = cHPMatchRes.MatchInfoList.Count;
            //if (resultcount > 0)
            //{
            //    foreach (var item in cHPMatchRes.MatchInfoList)
            //    {
            //        //Console.WriteLine("MatchPoint: ({0},{1})", item.MatchPoint.fX, item.MatchPoint.fY);
            //        if (item.Score >= xMvdTolerance)
            //        {
            //            xFindResult result = new xFindResult();
            //            //var item = cHPMatchRes.MatchInfoList[0];
            //            result.fCenterX = item.MatchBox.CenterX;
            //            result.fCenterY = item.MatchBox.CenterY;
            //            result.fAngle = item.MatchBox.Angle;
            //            result.fScale = item.Scale;
            //            result.fScore = item.Score;

            //            xResults.Add(result);
            //        }
            //    }
            //}

            //bOK = xResults.Count > 0;

            //#endregion

            return bOK;
        }
        public bool HikRun3(RectangleF eRectF)
        {
            bool bOK = false;

            xResults.Clear();

            #region HIK_RUN

            // CreateToolInstance
            if (cAlmightyPatmatchToolObj == null)
                cAlmightyPatmatchToolObj = new VisionDesigner.AlmightyPatMatch.CAlmightyPatMatchTool();

            //Set type

            cAlmightyPatmatchToolObj.Type = PatMatchAlgorithmType.HPFeature;
            //cAlmightyPatmatchToolObj.Type = PatMatchAlgorithmType.FastFeature;

            //// Set ROI region (optional)
            cAlmightyPatmatchToolObj.RegionList.Clear();
            var region2 = new VisionDesigner.CMvdRectangleF(eRectF.X + eRectF.Width / 2,
               eRectF.Y + eRectF.Height / 2,
                eRectF.Width,
                eRectF.Height);
            cAlmightyPatmatchToolObj.RegionList.Add(new CAlmightyPatMatchRegion(region2, true));

            // Set basic parameter

            cAlmightyPatmatchToolObj.BasicParam.ShowOutlineStatus = false;

            // Set Pattern

            cAlmightyPatmatchToolObj.Pattern = cAlmightyPatternObj;

            cAlmightyPatmatchToolObj.SetRunParam("AngleStart", (-xMvdAngle).ToString());
            cAlmightyPatmatchToolObj.SetRunParam("AngleEnd", xMvdAngle.ToString());
            cAlmightyPatmatchToolObj.SetRunParam("MinScore", xMvdTolerance.ToString());
            cAlmightyPatmatchToolObj.SetRunParam("MaxOverlap", xMaxOverlap.ToString());

            if (xMvdRun_Image.PixelFormat != MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08)
            {
                //当前程序仅支持mono8。因此像素格会转换.
                xMvdRun_Image.ConvertImagePixelFormat(MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08);
            }
            cAlmightyPatmatchToolObj.InputImage = xMvdRun_Image;

            try
            {
                // Running

                cAlmightyPatmatchToolObj.Run();

                // Get the result

                VisionDesigner.AlmightyPatMatch.CAlmightyPatMatchResult cHPMatchRes = cAlmightyPatmatchToolObj.Result;

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
                        if (item.Score >= xMvdTolerance)
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

            }
            catch
            {

            }
            bOK = xResults.Count > 0;
            if (cAlmightyPatmatchToolObj != null)
            {
                cAlmightyPatmatchToolObj.Dispose();
                cAlmightyPatmatchToolObj = null;
            }
            
            //GC.Collect();

            #endregion

            return bOK;
        }
        public bool HikRun4Pre()
        {
            bool bOK = true;

            xResults.Clear();

            #region HIK_RUN

            // CreateToolInstance
            if (cAlmightyPatmatchToolObj == null)
                cAlmightyPatmatchToolObj = new VisionDesigner.AlmightyPatMatch.CAlmightyPatMatchTool();

            cAlmightyPatmatchToolObj.ThreadNum = 4;
            //Set type

            cAlmightyPatmatchToolObj.Type = PatMatchAlgorithmType.HPFeature;

            //// Set ROI region (optional)
            cAlmightyPatmatchToolObj.RegionList.Clear();
            // Set basic parameter

            cAlmightyPatmatchToolObj.BasicParam.ShowOutlineStatus = false;

            // Set Pattern

            cAlmightyPatmatchToolObj.Pattern = cAlmightyPatternObj;

            cAlmightyPatmatchToolObj.SetRunParam("AngleStart", (-xMvdAngle).ToString());
            cAlmightyPatmatchToolObj.SetRunParam("AngleEnd", xMvdAngle.ToString());
            cAlmightyPatmatchToolObj.SetRunParam("MinScore", xMvdTolerance.ToString());

            if (xMvdRun_Image.PixelFormat != MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08)
            {
                //当前程序仅支持mono8。因此像素格会转换.
                xMvdRun_Image.ConvertImagePixelFormat(MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08);
            }
            cAlmightyPatmatchToolObj.InputImage = xMvdRun_Image;

            #endregion

            return bOK;
        }
        public bool HikRun4(RectangleF eRectF)
        {
            bool bOK = false;

            xResults.Clear();

            #region HIK_RUN

            //// Set ROI region (optional)
            cAlmightyPatmatchToolObj.RegionList.Clear();
            var region2 = new VisionDesigner.CMvdRectangleF(eRectF.X + eRectF.Width / 2,
               eRectF.Y + eRectF.Height / 2,
                eRectF.Width,
                eRectF.Height);
            cAlmightyPatmatchToolObj.RegionList.Add(new CAlmightyPatMatchRegion(region2, true));

            try
            {
                // Running

                cAlmightyPatmatchToolObj.Run();

                // Get the result

                VisionDesigner.AlmightyPatMatch.CAlmightyPatMatchResult cHPMatchRes = cAlmightyPatmatchToolObj.Result;

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
                        if (item.Score >= xMvdTolerance)
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

            }
            catch
            {

            }
            bOK = xResults.Count > 0;
            //if (cAlmightyPatmatchToolObj != null)
            //{
            //    cAlmightyPatmatchToolObj.Dispose();
            //    cAlmightyPatmatchToolObj = null;
            //}

            #endregion

            return bOK;
        }
        public void Dispose()
        {
            if (bmpObj_Image != null)
            {
                bmpObj_Image.Dispose();
                bmpObj_Image = null;
            }
            if (bmpRun_Image != null)
            {
                bmpRun_Image.Dispose();
                bmpRun_Image = null;
            }
            if (xMvdObj_Image != null)
            {
                xMvdObj_Image.Dispose();
                xMvdObj_Image = null;
            }
            if (xMvdRun_Image != null)
            {
                xMvdRun_Image.Dispose();
                xMvdRun_Image = null;
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
