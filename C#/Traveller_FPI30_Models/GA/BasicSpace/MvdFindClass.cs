using AUVision;
using JetEazy.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using VisionDesigner;
using VisionDesigner.AlmightyPatMatch;

namespace LaserAlignDX.BasicSpace
{
    public class MvdFindClass : IDisposable
    {
        #region PRIVATE_MVD_TOOLS
        CAlmightyPattern _almightyPattern = new CAlmightyPattern();
        CAlmightyPatMatchTool _almightyPatmatchTool = new CAlmightyPatMatchTool();
        //VisionDesigner.GrayPatMatch.CGrayPattern cGrayPatternObj = null;
        //VisionDesigner.GrayPatMatch.CGrayPatMatchTool cGrayPatMatchToolObj = null;
        #endregion

        public Bitmap bmpObj_Image = null;
        public Bitmap bmpRun_Image = null;
        public CMvdImage xMvdObj_Image = null;
        public CMvdImage xMvdRun_Image = null;

        public List<xFindResult> xResults = new List<xFindResult>();
        public List<CMvdRectangleF> xMvdResultRects = new List<CMvdRectangleF>();

        #region PUBLIC_PARAMS
        public float xMvdAngle { get; set; } = 5f;
        public int xMvdMaxOcc { get; set; } = 1;
        public float xMvdTolerance { get; set; } = 0.5f;
        public PointF xMvdFixed { get; set; } = new PointF(-1, -1);
        public int xMaxOverlap { get; set; } = 80;
        #endregion

        public MvdFindClass()
        {
        }

#if(false)
        ~MvdFindClass()
        {
            Dispose();
        }
#endif

        public void Dispose()
        {
            bmpObj_Image?.Dispose();
            bmpObj_Image = null;
            bmpRun_Image?.Dispose();
            bmpRun_Image = null;
            xMvdObj_Image?.Dispose();
            xMvdObj_Image = null;
            xMvdRun_Image?.Dispose();
            xMvdRun_Image = null;
            _almightyPatmatchTool?.Dispose();
            _almightyPatmatchTool = null;
            _almightyPattern?.Dispose();
            _almightyPattern = null;
        }

        public bool HikTrainBmp()
        {
            xMvdObj_Image?.Dispose();
            xMvdObj_Image = GaImageUtil.BitmapToCMvdImage(bmpObj_Image);
            return HikTrain2();
        }
        public bool HikRunBmp()
        {
            xMvdRun_Image?.Dispose();
            xMvdRun_Image = GaImageUtil.BitmapToCMvdImage(bmpRun_Image);
            return HikRun2();
        }
        
        bool HikTrain2()
        {
            bool bOK = false;
            string errmessage = string.Empty;

            #region HIK_TRAIN
            try
            {
                //// CreatePatternInstance
                //if (_almightyPattern == null)
                //    _almightyPattern = new VisionDesigner.AlmightyPatMatch.CAlmightyPattern();

                //Set type
                _almightyPattern.Type = PatMatchAlgorithmType.HPFeature;
                //cAlmightyPatternObj.Type = PatMatchAlgorithmType.FastFeature;

                if (xMvdObj_Image.PixelFormat != MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08)
                {
                    //当前程序仅支持mono8。因此像素格会转换.
                    xMvdObj_Image.ConvertImagePixelFormat(MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08);
                }
                _almightyPattern.InputImage = xMvdObj_Image;
                // Set ROI region (optional)
                _almightyPattern.RegionList.Clear();
                float extendvalue = 1f;
                var region1 = new VisionDesigner.CMvdRectangleF(xMvdObj_Image.Width * 0.5f,
                    xMvdObj_Image.Height * 0.5f,
                    xMvdObj_Image.Width * extendvalue,
                    xMvdObj_Image.Height * extendvalue);

                _almightyPattern.RegionList.Add(new CAlmightyPatMatchRegion(region1, true));

                // Set basic parameter

                //cAlmightyPatternObj.BasicParam.FixPoint =
                //    new VisionDesigner.MVD_POINT_F(Convert.ToSingle(xMvdObj_Image.Width * extendvalue) / 2,
                //                                                               Convert.ToSingle(xMvdObj_Image.Height * extendvalue) / 2);

                if (xMvdFixed.X < 0 || xMvdFixed.X > xMvdObj_Image.Width || xMvdFixed.Y < 0 || xMvdFixed.Y > xMvdObj_Image.Height)
                {
                    _almightyPattern.BasicParam.FixPoint =
                        new VisionDesigner.MVD_POINT_F(Convert.ToSingle(xMvdObj_Image.Width * extendvalue) / 2,
                                                                                   Convert.ToSingle(xMvdObj_Image.Height * extendvalue) / 2);

                }
                else
                {
                    _almightyPattern.BasicParam.FixPoint =
                                    new VisionDesigner.MVD_POINT_F(xMvdFixed.X, xMvdFixed.Y);
                }

                // Train

                _almightyPattern.Train();
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
        bool HikTrain2(CMvdRectangleF cMvdRectangleF)
        {
            bool bOK = false;

            try
            {
                // CreatePatternInstance
                if (_almightyPattern == null)
                    _almightyPattern = new VisionDesigner.AlmightyPatMatch.CAlmightyPattern();

                //Set type
                _almightyPattern.Type = PatMatchAlgorithmType.HPFeature;
                //cAlmightyPatternObj.Type = PatMatchAlgorithmType.FastFeature;

                if (xMvdObj_Image.PixelFormat != MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08)
                {
                    //当前程序仅支持mono8。因此像素格会转换.
                    xMvdObj_Image.ConvertImagePixelFormat(MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08);
                }
                _almightyPattern.InputImage = xMvdObj_Image;
                // Set ROI region (optional)
                _almightyPattern.RegionList.Clear();
                var region1 = cMvdRectangleF;

                _almightyPattern.RegionList.Add(new CAlmightyPatMatchRegion(region1, true));

                // Set basic parameter

                _almightyPattern.BasicParam.FixPoint =
                    new VisionDesigner.MVD_POINT_F(cMvdRectangleF.CenterX, cMvdRectangleF.CenterY);

                // Train

                _almightyPattern.Train();
                bOK = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            return bOK;
        }

        bool HikRun2()
        {
            bool bOK = HikRun2(new RectangleF(0, 0, bmpRun_Image.Width, bmpRun_Image.Height));

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
        bool HikRun2(RectangleF eRectF)
        {
            bool bOK = false;

            xResults.Clear();
            xMvdResultRects.Clear();

            #region HIK_RUN

            // CreateToolInstance
            if (_almightyPatmatchTool == null)
                _almightyPatmatchTool = new VisionDesigner.AlmightyPatMatch.CAlmightyPatMatchTool();

            //Set type

            _almightyPatmatchTool.Type = PatMatchAlgorithmType.HPFeature;
            //cAlmightyPatmatchToolObj.Type = PatMatchAlgorithmType.FastFeature;

            //// Set ROI region (optional)
            _almightyPatmatchTool.RegionList.Clear();
            var region2 = new VisionDesigner.CMvdRectangleF(eRectF.X + eRectF.Width / 2,
               eRectF.Y + eRectF.Height / 2,
                eRectF.Width,
                eRectF.Height);
            _almightyPatmatchTool.RegionList.Add(new CAlmightyPatMatchRegion(region2, true));

            // Set basic parameter

            _almightyPatmatchTool.BasicParam.ShowOutlineStatus = false;

            // Set Pattern

            _almightyPatmatchTool.Pattern = _almightyPattern;

            _almightyPatmatchTool.SetRunParam("AngleStart", (-xMvdAngle).ToString());
            _almightyPatmatchTool.SetRunParam("AngleEnd", xMvdAngle.ToString());
            _almightyPatmatchTool.SetRunParam("MinScore", xMvdTolerance.ToString());
            _almightyPatmatchTool.SetRunParam("MaxOverlap", xMaxOverlap.ToString());

            if (xMvdRun_Image.PixelFormat != MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08)
            {
                //当前程序仅支持mono8。因此像素格会转换.
                xMvdRun_Image.ConvertImagePixelFormat(MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08);
            }
            _almightyPatmatchTool.InputImage = xMvdRun_Image;

            try
            {
                // Running

                _almightyPatmatchTool.Run();

                // Get the result

                VisionDesigner.AlmightyPatMatch.CAlmightyPatMatchResult cHPMatchRes = _almightyPatmatchTool.Result;

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
                            xMvdResultRects.Add(item.MatchBox);
                        }
                    }
                }

            }
            catch
            {

            }
            bOK = xResults.Count > 0;
            if (_almightyPatmatchTool != null)
            {
                _almightyPatmatchTool.Dispose();
                _almightyPatmatchTool = null;
            }
            
            //GC.Collect();

            #endregion

            return bOK;
        }

#if (false)
        bool HikRun4Pre()
        {
            bool bOK = true;

            xResults.Clear();
            xMvdResultRects.Clear();

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
        bool HikRun4(RectangleF eRectF)
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
#endif
    }
}
