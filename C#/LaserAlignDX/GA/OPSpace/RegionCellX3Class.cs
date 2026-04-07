using AUVision;
using JetEazy.Utils;
using LaserAlignDX.BasicSpace;
using LaserAlignDX.Model;
using LaserAlignDX.OPSpace.RecipeSpace;
using System;
using System.Collections.Generic;
using System.Drawing;
using VisionDesigner;
using VisionDesigner.Code2DReader;
using VisionDesigner.ImageArithmetic;
using VisionDesigner.PositionFix;
using MvdFindLineClass = LaserAlignDX.BasicSpace.MvdFindLineClass;


namespace LaserAlignDX.OPSpace
{
    public class RegionCellX3Class : IDisposable
    {
        #region MVD_TOOLS
        VisionDesigner.PositionFix.CPositionFixTool cPositionFixToolObj = null;// new VisionDesigner.PositionFix.CPositionFixTool();
        CImageArithmeticTool cImageArithmeticToolObj = null;// new CImageArithmeticTool();
        VisionDesigner.ImageBinary.CImageBinaryTool cImageBinaryToolObj = null;// new VisionDesigner.ImageBinary.CImageBinaryTool();
        VisionDesigner.ImageMorph.CImageMorphTool cImageMorphToolObj = null;// new VisionDesigner.ImageMorph.CImageMorphTool();
        VisionDesigner.BlobFind.CBlobFindTool cBlobFindToolObj = null;// new VisionDesigner.BlobFind.CBlobFindTool();
        Mvd2DReaderClass mvd2DReader = null;// new Mvd2DReaderClass();
        MvdFindLineClass mvdFindLineClass = null;
        MvdPairLineClass mvdPairLineClass = null;
        CMvdRectangleF MvdRunPositionFix;
        #endregion

        #region GLOBAL_MESS
        public InspectX3ParaClass xInspect
        {
            get => RecipeFPIX3Class.Instance.InspectParams;
        }
        #endregion

        public RegionCellX3Class()
        {
        }
        ~RegionCellX3Class()
        {
            Dispose();
        }
        public void Dispose()
        {
            cPositionFixToolObj?.Dispose();
            cPositionFixToolObj = null;
            cImageArithmeticToolObj?.Dispose();
            cImageArithmeticToolObj = null;
            cImageBinaryToolObj?.Dispose();
            cImageBinaryToolObj = null;
            cImageMorphToolObj?.Dispose();
            cImageMorphToolObj = null;
            cBlobFindToolObj?.Dispose();
            cBlobFindToolObj = null;
            mvd2DReader?.Dispose();
            mvd2DReader = null;
            mvdFindLineClass?.Dispose();
            mvdFindLineClass = null;
            mvdPairLineClass?.Dispose();
            mvdPairLineClass = null;
        }

        // INDEX
        public int Index = 0;
        public string Name = "";
        public string lblName = "";
        public int CellRow = 0;
        public int CellCol = 0;

        /// <summary>
        /// ROI (FullFov Camera Coordinates) (單位 pixel)
        /// </summary>
        public RectangleF viewRectF = new RectangleF();

        /// <summary>
        /// 優化後, 晶粒位置 數據 放置於此 
        /// </summary>
        public GaChipData ChipData = new GaChipData();

        public float OrgX = 0;
        public float OrgY = 0;
        public float RunX = 0;
        public float RunY = 0;
        public float RunAngle = 0;
        
        /// <summary>
        /// 测量结果: 晶粒尺寸X (單位 mm)
        /// </summary>
        public float RunWidth
        {
            get => ChipData.ChipDimension.ChipWidth;
            set => ChipData.ChipDimension.ChipWidth = value;
        }
        /// <summary>
        /// 测量结果: 晶粒尺寸Y (單位 mm)
        /// </summary>
        public float RunHeight
        {
            get => ChipData.ChipDimension.ChipHeight;
            set => ChipData.ChipDimension.ChipHeight = value;
        }
        /// <summary>
        /// 测量结果: 格點型晶粒 邊緣厚度 左右差 (X方向) (單位 mm)
        /// </summary>
        public float PadEdgeDiffX
        {
            //get => PadEdgeSizes[(int)EdgeBorder.Left] - PadEdgeSizes[(int)EdgeBorder.Right];
            get => ChipData.ChipDimension.PadEdgeDiffX;
        }
        /// <summary>
        /// 测量结果: 格點型晶粒 邊緣厚度 上下差 (Y方向) (單位 mm)
        /// </summary>
        public float PadEdgeDiffY
        {
            //get => PadEdgeSizes[(int)EdgeBorder.Top] - PadEdgeSizes[(int)EdgeBorder.Bottom];
            get => ChipData.ChipDimension.PadEdgeDiffY;
        }
        /// <summary>
        /// 测量结果: 格點型晶粒 邊緣厚度 (順序: 左上右下) (單位 mm)
        /// </summary>
        public float[] PadEdgeSizes
        {
            get => ChipData.ChipDimension.PadEdgeSizes;
        }

        public PointF Sur1 = new PointF();
        public PointF Sur2 = new PointF();

        public bool ByPass = false;
        

        #region FILE_PATH
        public bool IsSaveDebugPicture = false;
        public string SaveDebugPath = $"D:\\log\\DebugImage";
        #endregion

        public AUVision.xFindResult xFindResult = new AUVision.xFindResult();
        public InspectReason inspectReason = InspectReason.PASS;
        public List<InspectReason> inspectReasons = new List<InspectReason>();

        #region MVD_LINE_SEGMENTS
        /// <summary>
        /// 外围的直线
        /// </summary>
        public CMvdLineSegmentF[] cMvdLineSegmentFsOut = new CMvdLineSegmentF[4];
        /// <summary>
        /// 内围的直线
        /// </summary>
        public CMvdLineSegmentF[] cMvdLineSegmentFsInSide = new CMvdLineSegmentF[4];
        /// <summary>
        /// Runtime 跑線後的 LineSegment Border Box2D
        /// </summary>
        public CMvdShape[] cMvdShapesForFindLineRegion = new CMvdShape[4];

        /// <summary>
        /// 寻找直线
        /// </summary>
        /// <param name="iSideIndex">哪条边序号</param>
        /// <param name="bmp">输入图片</param>
        /// <param name="roi">寻找的ROI</param>
        public void LineSegmentRun(int iSideIndex, Bitmap bmp, CMvdRectangleF roi)
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

            mvdFindLineClass.Background = xInspect.xCarrierBackground;
            cMvdLineSegmentFsOut[iSideIndex] = mvdFindLineClass.Run(bmp, roi, iSideIndex);
        }

        /// <summary>
        /// 寻找平行线
        /// </summary>
        /// <param name="iSideIndex">哪条边序号</param>
        /// <param name="bmp">输入图片</param>
        /// <param name="r">寻找的ROI</param>
        public void pairLineSegmentRun(int iSideIndex, Bitmap bmp, CMvdRectangleF r)
        {
            // 停用, 改用新的計算方式 !!!
#if (OPT_LEGACY)
            if (mvdPairLineClass == null)
                mvdPairLineClass = new MvdPairLineClass();
            cMvdLineSegmentFsOut[iSideIndex] = null;
            CPairLineFindResult cPairLineFindResult = null;
            if (iSideIndex == 0)
            {
                mvdPairLineClass.bPositive = xInspect.bPositive0;
                mvdPairLineClass.bFindOrient = true;
                //mvdPairLineClass.bEdgePolarity = xInspect.bEdgePolarity0;
                cPairLineFindResult = mvdPairLineClass.Run(bmp, r);
                if (cPairLineFindResult != null)
                {
                    #region 左边距

                    try
                    {
                        if (cPairLineFindResult.Line0 != null && cPairLineFindResult.Line1 != null)
                        {
                            // 使用 MVD VisionDesigner Tool
                            using (var cL2LMeasureToolObj = new VisionDesigner.L2LMeasure.CL2LMeasureTool())
                            {
                                cL2LMeasureToolObj.BasicParam.Line1 = cPairLineFindResult.Line0;
                                cL2LMeasureToolObj.BasicParam.Line2 = cPairLineFindResult.Line1;
                                cL2LMeasureToolObj.Run();
                                var cL2LMeasureRes = cL2LMeasureToolObj.Result;
                                DisLeft = cL2LMeasureRes.VerticalAbsDist * INI.Instance.ImageResolutionX;
                            }
                        }
                    }
                    catch (MvdException ex)
                    {
                        //Console.WriteLine("Fail with ErrorCode: 0x" + ex.ErrorCode.ToString("X"));
                        LtDebug.LOG.Error(ex, "左边距 異常: ErrorCode = 0x{0:X}", ex.ErrorCode);
                    }
                    catch (System.Exception ex)
                    {
                        //Console.WriteLine("Fail with error " + ex.Message);
                        LtDebug.LOG.Error(ex, "左边距 異常");
                    }

                    #endregion

                    if (xInspect.bPositive0)
                    {
                        cMvdLineSegmentFsOut[iSideIndex] = cPairLineFindResult.Line0;
                        cMvdLineSegmentFsInSide[iSideIndex] = cPairLineFindResult.Line1;
                    }
                    else
                    {
                        cMvdLineSegmentFsOut[iSideIndex] = cPairLineFindResult.Line1;
                        cMvdLineSegmentFsInSide[iSideIndex] = cPairLineFindResult.Line0;
                    }
                }
            }
            else if (iSideIndex == 1)
            {
                mvdPairLineClass.bPositive = xInspect.bPositive1;
                mvdPairLineClass.bFindOrient = false;
                //mvdPairLineClass.bEdgePolarity = xInspect.bEdgePolarity1;
                cPairLineFindResult = mvdPairLineClass.Run(bmp, r);
                if (cPairLineFindResult != null)
                {
                    #region 上边距

                    try
                    {
                        if (cPairLineFindResult.Line0 != null && cPairLineFindResult.Line1 != null)
                        {
                            // 使用 MVD VisionDesigner Tool
                            using (var cL2LMeasureToolObj = new VisionDesigner.L2LMeasure.CL2LMeasureTool())
                            {
                                cL2LMeasureToolObj.BasicParam.Line1 = cPairLineFindResult.Line0;
                                cL2LMeasureToolObj.BasicParam.Line2 = cPairLineFindResult.Line1;
                                cL2LMeasureToolObj.Run();
                                var cL2LMeasureRes = cL2LMeasureToolObj.Result;
                                DisTop = cL2LMeasureRes.VerticalAbsDist * INI.Instance.ImageResolutionY;
                            }
                        }
                    }
                    catch (MvdException ex)
                    {
                        //Console.WriteLine("Fail with ErrorCode: 0x" + ex.ErrorCode.ToString("X"));
                        LtDebug.LOG.Error(ex, "上边距 異常: ErrorCode = 0x{0:X}", ex.ErrorCode);
                    }
                    catch (System.Exception ex)
                    {
                        //Console.WriteLine("Fail with error " + ex.Message);
                        LtDebug.LOG.Error(ex, "上边距 異常");
                    }

                    #endregion
                    if (xInspect.bPositive1)
                    {
                        cMvdLineSegmentFsOut[iSideIndex] = cPairLineFindResult.Line0;
                        cMvdLineSegmentFsInSide[iSideIndex] = cPairLineFindResult.Line1;
                    }
                    else
                    {
                        cMvdLineSegmentFsOut[iSideIndex] = cPairLineFindResult.Line1;
                        cMvdLineSegmentFsInSide[iSideIndex] = cPairLineFindResult.Line0;
                    }
                }
            }
            else if (iSideIndex == 2)
            {
                mvdPairLineClass.bPositive = xInspect.bPositive2;
                mvdPairLineClass.bFindOrient = true;
                //mvdPairLineClass.bEdgePolarity = xInspect.bEdgePolarity2;
                cPairLineFindResult = mvdPairLineClass.Run(bmp, r);
                if (cPairLineFindResult != null)
                {
                    #region 右边距

                    try
                    {
                        if (cPairLineFindResult.Line0 != null && cPairLineFindResult.Line1 != null)
                        {
                            // 使用 MVD VisionDesigner Tool
                            using (var cL2LMeasureToolObj = new VisionDesigner.L2LMeasure.CL2LMeasureTool())
                            {
                                cL2LMeasureToolObj.BasicParam.Line1 = cPairLineFindResult.Line0;
                                cL2LMeasureToolObj.BasicParam.Line2 = cPairLineFindResult.Line1;
                                cL2LMeasureToolObj.Run();
                                var cL2LMeasureRes = cL2LMeasureToolObj.Result;
                                DisRight = cL2LMeasureRes.VerticalAbsDist * INI.Instance.ImageResolutionX;
                            }
                        }
                    }
                    catch (MvdException ex)
                    {
                        //Console.WriteLine("Fail with ErrorCode: 0x" + ex.ErrorCode.ToString("X"));
                        LtDebug.LOG.Error(ex, "右边距 異常: ErrorCode = 0x{0:X}", ex.ErrorCode);
                    }
                    catch (System.Exception ex)
                    {
                        //Console.WriteLine("Fail with error " + ex.Message);
                        LtDebug.LOG.Error(ex, "右边距 異常");
                    }

                    #endregion
                    if (xInspect.bPositive2)
                    {
                        cMvdLineSegmentFsOut[iSideIndex] = cPairLineFindResult.Line1;
                        cMvdLineSegmentFsInSide[iSideIndex] = cPairLineFindResult.Line0;
                    }
                    else
                    {
                        cMvdLineSegmentFsOut[iSideIndex] = cPairLineFindResult.Line0;
                        cMvdLineSegmentFsInSide[iSideIndex] = cPairLineFindResult.Line1;
                    }
                }
            }
            else if (iSideIndex == 3)
            {
                mvdPairLineClass.bPositive = xInspect.bPositive3;
                mvdPairLineClass.bFindOrient = false;
                //mvdPairLineClass.bEdgePolarity = xInspect.bEdgePolarity3;
                cPairLineFindResult = mvdPairLineClass.Run(bmp, r);
                if (cPairLineFindResult != null)
                {
                    #region 下边距

                    try
                    {
                        if (cPairLineFindResult.Line0 != null && cPairLineFindResult.Line1 != null)
                        {
                            // 使用 MVD VisionDesigner Tool
                            using (var cL2LMeasureToolObj = new VisionDesigner.L2LMeasure.CL2LMeasureTool())
                            {
                                cL2LMeasureToolObj.BasicParam.Line1 = cPairLineFindResult.Line0;
                                cL2LMeasureToolObj.BasicParam.Line2 = cPairLineFindResult.Line1;
                                cL2LMeasureToolObj.Run();
                                var cL2LMeasureRes = cL2LMeasureToolObj.Result;
                                DisBottom = cL2LMeasureRes.VerticalAbsDist * INI.Instance.ImageResolutionY;
                            }
                        }
                    }
                    catch (MvdException ex)
                    {
                        //Console.WriteLine("Fail with ErrorCode: 0x" + ex.ErrorCode.ToString("X"));
                        LtDebug.LOG.Error(ex, "下边距 異常: ErrorCode = 0x{0:X}", ex.ErrorCode);
                    }
                    catch (System.Exception ex)
                    {
                        //Console.WriteLine("Fail with error " + ex.Message);
                        LtDebug.LOG.Error(ex, "下边距 異常");
                    }

                    #endregion
                    if (xInspect.bPositive3)
                    {
                        cMvdLineSegmentFsOut[iSideIndex] = cPairLineFindResult.Line1;
                        cMvdLineSegmentFsInSide[iSideIndex] = cPairLineFindResult.Line0;
                    }
                    else
                    {
                        cMvdLineSegmentFsOut[iSideIndex] = cPairLineFindResult.Line0;
                        cMvdLineSegmentFsInSide[iSideIndex] = cPairLineFindResult.Line1;
                    }
                }
            }
#endif
        }
        #endregion

        #region MVD_QRCODE_DATA
        public string SetBarcodeStr = string.Empty;
        public C2DCodeInfo RunCodeInfo = null;
        public CMvdPolygonF DrawBarcodePosition = null;
        #endregion

        #region MVD_DEFAULT_RECTF
        /// <summary>
        /// 只是把 viewRectF 轉成 CMvdRectangleF 並加上 MVD 顏色
        /// </summary>
        public CMvdRectangleF DrawResultRectF()
        {
            if (MvdRunPositionFix == null)
            {
                //MvdRunPositionFix = new CMvdRectangleF(
                //                            viewRectF.X + viewRectF.Width / 2, 
                //                            viewRectF.Y + viewRectF.Height / 2, 
                //                            viewRectF.Width,
                //                            viewRectF.Height
                //                        );
                MvdRunPositionFix = GaImageUtil.ToCMvdRectangleF(ref viewRectF);
                MvdRunPositionFix.BorderColor = new MVD_COLOR(255, 0, 0);
            }

            switch (inspectReason)
            {
                case InspectReason.INS_ALIGNERR:
                    //MvdRunPositionFix = new CMvdRectangleF(
                    //                            viewRectF.X + viewRectF.Width / 2, 
                    //                            viewRectF.Y + viewRectF.Height / 2, 
                    //                            viewRectF.Width,
                    //                            viewRectF.Height
                    //                        );
                    MvdRunPositionFix = GaImageUtil.ToCMvdRectangleF(ref viewRectF);
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
        public void SetMvdRunPositionFix(CMvdRectangleF mvdRect)
        {
            MvdRunPositionFix = mvdRect;
        }
        #endregion

        #region MVD_FOR_EMPTY_TRAY
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
            if (inspectReason == InspectReason.INS_DEFECTERR)
            {
                str = "疑似有料";
                return str;
            }
            foreach (var reason in inspectReasons)
            {
                if (reason == InspectReason.INS_DEFECTERR)
                {
                    str = "疑似有料";
                    //str = "Maybe.P";
                    break;
                }
            }

            return str;
        }
        #endregion

        #region MVD_DEFECT_INSPECT_MEMBERS
        private List<CMvdRectangleF> _blobMvdRectFNGList = new List<CMvdRectangleF>();
        private SizeF _defectRunRoiSize = new SizeF(100, 100);
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

            foreach (CMvdRectangleF mvdRectangleF in _blobMvdRectFNGList)
            {
                mvdRectangleF.CenterX += MvdRunPositionFix.CenterX - _defectRunRoiSize.Width / 2;  // bmpItemRun.Width / 2;
                mvdRectangleF.CenterY += MvdRunPositionFix.CenterY - _defectRunRoiSize.Height / 2; // bmpItemRun.Height / 2;
                mvdRectangleF.BorderColor = new MVD_COLOR(255, 0, 0);
                mvdRectangleF.BorderWidth = 1;
                cMvdRectangleFs.Add(mvdRectangleF);
            }
            return cMvdRectangleFs;
        }
        #endregion

        /// <summary>
        /// 整理打包 尺寸计算 的 最後结果
        /// </summary>
        /// <returns>true:OK false:NG</returns>
        public bool PackMeasureResult()
        {
            bool bOK = true;

            if (xInspect.optChipMeasurement)
            {
                //float tmpwidth = RunWidth;
                //float tmpheight = RunHeight;

                //if (INI.Instance.IsCheat)
                //{
                //    if (RunWidth < xInspect.mWidthStand - xInspect.mWidthLower || RunWidth > xInspect.mWidthStand + xInspect.mWidthPercentage)
                //    {
                //        if (RunWidth >= xInspect.mWidthStand - xInspect.mWidthLower - 0.02 && RunWidth < xInspect.mWidthStand - xInspect.mWidthLower)
                //            RunWidth = RunWidth + 0.027f;
                //        if (RunWidth > xInspect.mWidthStand + xInspect.mWidthPercentage && RunWidth <= xInspect.mWidthStand + xInspect.mWidthPercentage + 0.02)
                //            RunWidth = RunWidth - 0.027f;
                //    }
                //    if (RunHeight < xInspect.mHeightStand - xInspect.mHeightLower || RunHeight > xInspect.mHeightStand + xInspect.mHeightPercentage)
                //    {
                //        if (RunHeight >= xInspect.mHeightStand - xInspect.mHeightLower - 0.02 && RunHeight < xInspect.mHeightStand - xInspect.mHeightLower)
                //            RunHeight = RunHeight + 0.027f;
                //        if (RunHeight > xInspect.mHeightStand + xInspect.mHeightPercentage && RunHeight <= xInspect.mHeightStand + xInspect.mHeightPercentage + 0.02)
                //            RunHeight = RunHeight - 0.027f;
                //    }
                //}

                if (RunWidth < xInspect.mWidthStandMin || RunWidth > xInspect.mWidthStandMax)
                {
                    bOK = false;
                }
                else if (RunHeight < xInspect.mHeightStandMin || RunHeight > xInspect.mHeightStandMax)
                {
                    bOK = false;
                }
                if (bOK && xInspect.optChipEdgesDiffCompare && xInspect.xAlgorithm == MatchAlgorithmEnum.GridMatch)
                {
                    if (Math.Abs(PadEdgeDiffX) > xInspect.PadEdgeDiffMaxX || Math.Abs(PadEdgeDiffY) > xInspect.PadEdgeDiffMaxY)
                        bOK = false;
                }

                if (!bOK)
                {
                    inspectReason = InspectReason.INS_CUTTINGERR;
                    inspectReasons.Add(InspectReason.INS_CUTTINGERR);
                }
            }
            return bOK;
        }

        #region OLD_CODE_不要在此生成_客戶要求的_顯示與報表_字串格式_不然換不同廠家就要跟著一直變動_CELL
#if (OPT_OLD_STRING_FORMATTER_CODE)
        const string m_Format = "0.000";
        public string ToResultStr()
        {
            string str = string.Empty;

            str += $"{Index}" + ",";
            str += $"{lblName}" + ",";
            str += $"{(ByPass ? (INI.Instance.IsForceInspect ? "1强制检测" : "0不检测") : "1检测")}" + ",";
            str += $"[{OrgX.ToString(m_Format)}" + ",";
            str += $"{OrgY.ToString(m_Format)}]" + ",";
            str += $"{RunX.ToString(m_Format)}" + ",";
            str += $"{RunY.ToString(m_Format)}" + ",";
            str += $"{RunAngle.ToString(m_Format)}" + ",";
            if (xInspect.bOpenLineMeasure)
            {
                str += $"尺寸X[{RunWidth.ToString(m_Format)}]" + ",";
                str += $"尺寸Y[{RunHeight.ToString(m_Format)}]" + ",";
            }
            if (xInspect.bCheckMeasureOffset)
            {
                str += $"X方向偏移[{RunXOffset.ToString(m_Format)}]" + ",";
                str += $"Y方向偏移[{RunYOffset.ToString(m_Format)}]" + ",";
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

            str += $"{Index}" + "";
            str += $"[{OrgX.ToString(m_Format)}" + ",";
            str += $"{OrgY.ToString(m_Format)}]{Environment.NewLine}";
            str += $"[{RunX.ToString(m_Format)}" + ",";
            str += $"{RunY.ToString(m_Format)}" + ",";
            str += $"{RunAngle.ToString(m_Format)}]{Environment.NewLine}";
            str += $"马达1=[{PointF000ToString(Sur1)}]{Environment.NewLine}";
            str += $"马达2=[{PointF000ToString(Sur2)}]{Environment.NewLine}";
            if (xInspect.bOpenLineMeasure)
            {
                str += $"尺寸X[{RunWidth.ToString(m_Format)}mm]{Environment.NewLine}";
                str += $"尺寸Y[{RunHeight.ToString(m_Format)}mm]{Environment.NewLine}";
            }
            if (xInspect.bCheckMeasureOffset)
            {
                str += $"位置偏移X[{RunXOffset.ToString(m_Format)}mm]{Environment.NewLine}";
                str += $"位置偏移Y[{RunYOffset.ToString(m_Format)}mm]{Environment.NewLine}";
                str += $"左边距[{DisLeft.ToString(m_Format)}mm]{Environment.NewLine}";
                str += $"右边距[{DisRight.ToString(m_Format)}mm]{Environment.NewLine}";
                str += $"上边距[{DisTop.ToString(m_Format)}mm]{Environment.NewLine}";
                str += $"下边距[{DisBottom.ToString(m_Format)}mm]{Environment.NewLine}";
            }

            //str += Environment.NewLine;

            //str += $"ORG" + "-[";
            //str += $"{OrgX.ToString(m_Format)}" + ",";
            //str += $"{OrgY.ToString(m_Format)}" + ",";
            //str += $"{OrgAngle.ToString(m_Format)}]";
            return str;
        }
        public string ToReport1HeadStr()
        {
            string str = string.Empty;

            str += $"编号" + ",";
            str += $"名称" + ",";
            str += $"是否检测" + ",";
            str += $"尺寸X" + ",";
            str += $"尺寸Y" + ",";
            str += $"位置偏移X" + ",";
            str += $"位置偏移Y" + ",";
            str += $"原始X" + ",";
            str += $"原始Y" + ",";
            str += $"引导偏移X" + ",";
            str += $"引导偏移Y" + ",";
            str += $"引导偏移角度" + ",";

            str += $"左边距" + ",";
            str += $"右边距" + ",";
            str += $"上边距" + ",";
            str += $"下边距" + ",";

            str += $"马达1-X" + ",";
            str += $"马达1-Y" + ",";
            str += $"马达2-X" + ",";
            str += $"马达2-Y" + ",";
            str += $"条码设定值" + ",";
            str += $"读取码" + ",";
            str += $"{Environment.NewLine}";

            return str;
        }
        public string ToReport1Str()
        {
            string str = string.Empty;

            str += $"{Index}" + ",";
            str += $"{lblName}" + ",";
            str += $"{(ByPass ? (INI.Instance.IsForceInspect ? "1强制检测" : "0不检测") : "1检测")}" + ",";
            str += $"{RunWidth.ToString(m_Format)}" + ",";
            str += $"{RunHeight.ToString(m_Format)}" + ",";
            str += $"{RunXOffset.ToString(m_Format)}" + ",";
            str += $"{RunYOffset.ToString(m_Format)}" + ",";
            str += $"{OrgX.ToString(m_Format)}" + ",";
            str += $"{OrgY.ToString(m_Format)}" + ",";
            str += $"{RunX.ToString(m_Format)}" + ",";
            str += $"{RunY.ToString(m_Format)}" + ",";
            str += $"{RunAngle.ToString(m_Format)}" + ",";

            str += $"{DisLeft.ToString(m_Format)}" + ",";
            str += $"{DisRight.ToString(m_Format)}" + ",";
            str += $"{DisTop.ToString(m_Format)}" + ",";
            str += $"{DisBottom.ToString(m_Format)}" + ",";

            str += $"{PointF000ToString(Sur1)}" + ",";
            str += $"{PointF000ToString(Sur2)}" + ",";
            str += $"{SetBarcodeStr}" + ",";
            if (RunCodeInfo != null)
                str += $"{RunCodeInfo.Content}" + ",";
            else
                str += $"" + ",";
            str += $"{Environment.NewLine}";

            return str;
        }
        string PointF000ToString(PointF PTF)
        {
            return PTF.X.ToString("0.000") + "," + PTF.Y.ToString("0.000");
        }
#endif
        #endregion

        /// <summary>
        /// 清除上一次的檢測結果
        /// </summary>
        public void Reset()
        {
            ChipData = new GaChipData();
            xFindResult = new AUVision.xFindResult();

            inspectReason = InspectReason.PASS;
            inspectReasons.Clear();
            RunCodeInfo = null;

            if (mvd2DReader != null)
                mvd2DReader.DCodeInfo = null;

            DrawBarcodePosition = null;

            RunX = 0;
            RunY = 0;
            RunAngle = 0;
            IsSaveDebugPicture = false;

            int i = 0;
            while (i < 4)
            {
                //CMvdLineSegmentF mLine = cMvdLineSegmentFsOut[i];
                if (cMvdLineSegmentFsOut[i] != null)
                    cMvdLineSegmentFsOut[i] = null;
                if (cMvdLineSegmentFsInSide[i] != null)
                    cMvdLineSegmentFsInSide[i] = null;
                //CMvdShape mvdShape = cMvdShapesForFindLineRegion[i];
                if (cMvdShapesForFindLineRegion[i] != null)
                    cMvdShapesForFindLineRegion[i] = null;
                i++;
            }

            RunWidth = 0;
            RunHeight = 0;

            //DisLeft = 0;
            //DisTop = 0;
            //DisRight = 0;
            //DisBottom = 0;
        }

        #region MVD_AOI_FUNCTIONS

#if (OPT_NOT_USED)
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
#endif

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

        public void DetectDefects(Bitmap bmpTemplate, Bitmap bmpRun, Bitmap bmpMask)
        {
            //string dumpFolder = System.IO.Path.Combine(SaveDebugPath, "Detect");
            //if (IsSaveDebugPicture)
            //{
            //    if (!System.IO.Directory.Exists(dumpFolder))
            //        System.IO.Directory.CreateDirectory(dumpFolder);
            //}

            CMvdRectangleF _roi = new CMvdRectangleF(bmpTemplate.Width / 2, bmpTemplate.Height / 2, bmpTemplate.Width, bmpTemplate.Height);

            if (cImageArithmeticToolObj == null)
                cImageArithmeticToolObj = new CImageArithmeticTool();
            if (cImageBinaryToolObj == null)
                cImageBinaryToolObj = new VisionDesigner.ImageBinary.CImageBinaryTool();
            if (cImageMorphToolObj == null)
                cImageMorphToolObj = new VisionDesigner.ImageMorph.CImageMorphTool();
            if (cBlobFindToolObj == null)
                cBlobFindToolObj = new VisionDesigner.BlobFind.CBlobFindTool();

            cImageArithmeticToolObj.InputImage1 = GaImageUtil.BitmapToCMvdImage(bmpTemplate);
            cImageArithmeticToolObj.InputImage2 = GaImageUtil.BitmapToCMvdImage(bmpRun);
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
            cBlobFindToolObj.RegionImage = GaImageUtil.BitmapToCMvdImage(bmpMask);
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

            _defectRunRoiSize = bmpRun.Size;
            _blobMvdRectFNGList.Clear();

            foreach (var item in cBlobFindToolObj.Result.BlobInfo)
            {
                //Console.WriteLine("Index: {0}, Angle {1}", item.DomainIndex, item.BoxInfo.Angle);
                if (item.AreaF > xInspect.xCharArea)
                {
                    _blobMvdRectFNGList.Add(item.BoxInfo);
                }
                else if (item.LongAxis > xInspect.xCharWidth && item.ShortAxis > xInspect.xCharHeight)
                {
                    _blobMvdRectFNGList.Add(item.BoxInfo);
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

            if (_blobMvdRectFNGList.Count > 0)
            {
                inspectReasons.Add(InspectReason.INS_DEFECTERR);

                if (IsSaveDebugPicture)
                {
                    //cImageBinaryToolObj?.Result?.OutputImage?.SaveImage($"{SaveDebugPath}\\Detect\\{lblName}_Diff2.bmp", MVD_FILE_FORMAT.MVD_FILE_BMP);
                    //cBlobFindToolObj?.RegionImage?.SaveImage($"{SaveDebugPath}\\Detect\\{lblName}_Diff2_1.bmp", MVD_FILE_FORMAT.MVD_FILE_BMP);
                    ////if (cBlobFindRes.BlobImage != null)
                    //cBlobFindRes?.BlobImage?.SaveImage($"{SaveDebugPath}\\Detect\\{lblName}_Diff3.bmp", MVD_FILE_FORMAT.MVD_FILE_BMP);

                    string dumpFolder = System.IO.Path.Combine(SaveDebugPath, "Detect");
                    if (!System.IO.Directory.Exists(dumpFolder))
                        System.IO.Directory.CreateDirectory(dumpFolder);

                    string fileStem = System.IO.Path.Combine(dumpFolder, lblName);
                    cImageBinaryToolObj?.Result?.OutputImage?.SaveImage(fileStem + "_Diff2.bmp", MVD_FILE_FORMAT.MVD_FILE_BMP);
                    cBlobFindToolObj?.RegionImage?.SaveImage(fileStem + "_Diff2_1.bmp", MVD_FILE_FORMAT.MVD_FILE_BMP);
                    cBlobFindRes?.BlobImage?.SaveImage(fileStem + "_Diff3.bmp", MVD_FILE_FORMAT.MVD_FILE_BMP);
                }
            }
        }

        public void DeCode2D(Bitmap eBmpRun, PointF Poi_CodeBase, bool eJudged = false)
        {
            //if (IsSaveDebugPicture)
            //{
            //    if (!System.IO.Directory.Exists(SaveDebugPath + "\\Code"))
            //        System.IO.Directory.CreateDirectory(SaveDebugPath + "\\Code");
            //}

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

                #region GENERATE_MVD_GUI_COMPONENT_UGLY_CODE
                DrawBarcodePosition.BorderColor = new MVD_COLOR(0, 255, 0);
                DrawBarcodePosition.AddVertex(ptBase.X + RunCodeInfo.Position[0].nX, ptBase.Y + RunCodeInfo.Position[0].nY);
                DrawBarcodePosition.AddVertex(ptBase.X + RunCodeInfo.Position[1].nX, ptBase.Y + RunCodeInfo.Position[1].nY);
                DrawBarcodePosition.AddVertex(ptBase.X + RunCodeInfo.Position[2].nX, ptBase.Y + RunCodeInfo.Position[2].nY);
                DrawBarcodePosition.AddVertex(ptBase.X + RunCodeInfo.Position[3].nX, ptBase.Y + RunCodeInfo.Position[3].nY);
                #endregion

                //if (xInspectPara.bCheckMappingCode)
                if (eJudged)
                {
                    if (mvd2DReader.DCodeInfo.Content != SetBarcodeStr)
                    {
                        #region GENERATE_MVD_GUI_COMPONENT_UGLY_CODE
                        DrawBarcodePosition.BorderColor = new MVD_COLOR(255, 0, 0);
                        #endregion

                        inspectReasons.Add(InspectReason.INS_2DMAPNG);
                    }
                }
            }
            else
            {
                inspectReasons.Add(InspectReason.INS_2DERR);

                #region GENERATE_MVD_GUI_COMPONENT_UGLY_CODE
                DrawBarcodePosition = null;

                if (IsSaveDebugPicture)
                {
                    string dumpFolder = System.IO.Path.Combine(SaveDebugPath, "Code");
                    if (!System.IO.Directory.Exists(dumpFolder))
                        System.IO.Directory.CreateDirectory(dumpFolder);

                    string fileName = System.IO.Path.Combine(dumpFolder, $"{lblName}_Run.bmp");
                    mvd2DReader.MvdRunImage.SaveImage(fileName, MVD_FILE_FORMAT.MVD_FILE_BMP);
                }

                #endregion
            }
        }
        #endregion
    }
}
