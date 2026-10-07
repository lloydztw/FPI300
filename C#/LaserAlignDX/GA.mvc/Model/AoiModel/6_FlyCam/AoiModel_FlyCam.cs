#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-08-01 開始重整 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.Utils;
using LaserAlignDX.BasicSpace;
using LaserAlignDX.OPSpace.RecipeSpace;
using System.Collections.Generic;
using System.Drawing;
using VisionDesigner;
using VisionDesigner.BlobFind;

namespace LaserAlignDX.AoiModel.V3
{
    /// <summary>
    /// Fly Camera AOI
    /// </summary>
    public class AoiModel_FlyCam : AoiModelBase, IAoiFlyCamMatcher
    {
        #region GLOBAL_MESS
        FlyParaClass _xFlyAoiParams => _xRecipe.FlyAoiParams;
        #endregion

        #region KERNEL_MEMBERS
        MvdFindClass _flyTemplateMatcher = new MvdFindClass();
        Mvd2DReaderClass _qrDecoder = new Mvd2DReaderClass();
        #endregion

        #region RUNTIME_DATA
        #endregion

        public override void Dispose()
        {
            _flyTemplateMatcher?.Dispose();
            _flyTemplateMatcher = null;
            _qrDecoder?.Dispose();
            _qrDecoder = null;
        }

#if (OPT_RESERVED)
        public MvdFindClass GetTemplateMatcher()
        {
            return _flyTemplateMatcher;
        }

        public Mvd2DReaderClass GetQrDecoder()
        {
            return _qrDecoder;
        }
#endif

        public override bool Train(Bitmap goldenImage, params object[] args)
        {
            if (_flyTemplateMatcher == null)
                return false;

            // 直接使用 _xRecipe.FlyTemplateBmp 當 GoldenBmp
            var flyGoldenBmp = _xRecipe.FlyTemplateBmp;
            _flyTemplateMatcher.bmpObj_Image?.Dispose();
            _flyTemplateMatcher.bmpObj_Image = (Bitmap)flyGoldenBmp.Clone();

            bool ok = _flyTemplateMatcher.HikTrainBmp();
            return ok;
        }

        public override void Run(Bitmap sceneBmp)
        {
            RunTemplateMatch(sceneBmp);
        }

        bool RunTemplateMatch(Bitmap bmpScene)
        {
            this.MatchResultRects = null;
            this.MatchResultAngle = 0f;

            if (bmpScene == null || _flyTemplateMatcher == null)
                return false;
            
            _flyTemplateMatcher.xMvdAngle = _xFlyAoiParams.xAngle;
            _flyTemplateMatcher.xMvdTolerance = (1 - _xFlyAoiParams.xTolerance);  //<<<=这里把数据反过来不知为什么明明越大越严但是实际是反过来的
            
            _flyTemplateMatcher.bmpRun_Image?.Dispose();
            _flyTemplateMatcher.bmpRun_Image = (Bitmap)bmpScene.Clone();

            bool ok = _flyTemplateMatcher.HikRunBmp();

            if (ok)
            {
                this.MatchResultRects = _flyTemplateMatcher.xMvdResultRects;

                if (_flyTemplateMatcher.xResults.Count > 0)
                    this.MatchResultAngle = _flyTemplateMatcher.xResults[0].fAngle;
            }

            return ok;
        }

        public List<CMvdRectangleF> MatchResultRects
        {
            get;
            private set;
        }

        public float MatchResultAngle
        {
            get;
            private set;
        }

        public bool CheckSpecialAngle(Bitmap bmpScene, out List<CBlobInfo> retBlobs, out float retAngle, out PointF retCenter)
        {
            bool bOK = false;

            retBlobs = new List<CBlobInfo>();
            retAngle = 0f;
            retCenter = PointF.Empty;

            using (CMvdImage mvdInputImage = GaImageUtil.BitmapToCMvdImage(bmpScene))
            using (var mvdImageBinaryTool = new VisionDesigner.ImageBinary.CImageBinaryTool())
            using (var mvdBlobFindTool = new VisionDesigner.BlobFind.CBlobFindTool())
            {
                //二值化
                mvdImageBinaryTool.InputImage = mvdInputImage;
                mvdImageBinaryTool.ROI = null;
                mvdImageBinaryTool.SetRunParam("LowThreshold", _xFlyAoiParams.xThresholdValue.ToString());
                //cImageArithmeticToolObj.SetRunParam("HighThreshold", BlobHighThreshold.ToString());
                mvdImageBinaryTool.Run();

                //Find Blob
                //mvdBlobFindTool.InputImage?.Dispose();
                mvdBlobFindTool.InputImage = mvdImageBinaryTool.Result.OutputImage;
                mvdBlobFindTool.ROI = null;
                if (_xFlyAoiParams.xBlobMode == Eazy_Project_III.BlobMode.White)
                    mvdBlobFindTool.SetRunParam("Polarity", "BrightObject");
                else
                    mvdBlobFindTool.SetRunParam("Polarity", "DarkObject");
                mvdBlobFindTool.BasicParam.ShowBlobImageStatus = true;
                mvdBlobFindTool.Run();

                //挑出符合條件的 blobs
                if (mvdBlobFindTool.Result?.BlobInfo != null)
                {
                    foreach (var blob in mvdBlobFindTool.Result.BlobInfo)
                    {
                        if (blob.AreaF >= _xFlyAoiParams.xBlobAreaMin && 
                            blob.AreaF <= _xFlyAoiParams.xBlobAreaMax)
                        {
                            retBlobs.Add(blob);
                        }
                    }
                }

                if (retBlobs.Count >= 2)
                {
                    CBlobInfo b0 = retBlobs[0];
                    CBlobInfo b1 = retBlobs[1];

                    using (var mvdP2PMeasureTool = new VisionDesigner.P2PMeasure.CP2PMeasureTool())
                    {
                        // Set basic parameters
                        mvdP2PMeasureTool.BasicParam.Point1 = new MVD_POINT_F(b0.RectInfo.CenterX, b0.RectInfo.CenterY);
                        mvdP2PMeasureTool.BasicParam.Point2 = new MVD_POINT_F(b1.RectInfo.CenterX, b1.RectInfo.CenterY);

                        // Run
                        mvdP2PMeasureTool.Run();

                        // Result
                        var cP2PMeasureRes = mvdP2PMeasureTool.Result;

                        // ShuiPing 爛英文 (應該取名 Horizontal)
                        if (_xFlyAoiParams.xIsShuiPing)
                        {
                            retAngle = cP2PMeasureRes.Angle;
                        }
                        else
                        {
                            retAngle = cP2PMeasureRes.Angle + 90;
                        }

                        //if (cP2PMeasureRes.Angle < 0)
                        //{
                        //    if (xFlyAoiParams.xIsShuiPing)
                        //    {
                        //        retAngle = -cP2PMeasureRes.Angle;
                        //    }
                        //    else
                        //    {
                        //        retAngle = -cP2PMeasureRes.Angle + 90;
                        //    }
                        //}
                        //else
                        //{
                        //    if (xFlyAoiParams.xIsShuiPing)
                        //    {
                        //        retAngle = cP2PMeasureRes.Angle;
                        //    }
                        //    else
                        //    {
                        //        retAngle = cP2PMeasureRes.Angle - 90;
                        //    }
                        //}

                        retCenter = new PointF(cP2PMeasureRes.MidPoint.fX, cP2PMeasureRes.MidPoint.fY);
                        //Console.WriteLine("Angle: {0}", cP2PMeasureRes.Angle);
                        //Console.WriteLine("Distance: {0}", cP2PMeasureRes.Dist);
                    }

                    bOK = true;
                }
            }

            return bOK;
        }
    }
}
