using JetEazy.QvMath;
using LaserAlignDX.Model;
using LaserAlignDX.OPSpace.RecipeSpace;
using System;
using System.Collections.Generic;
using System.Drawing;
using Traveller106;

namespace LaserAlignDX.OPSpace
{
    public class RegionCellX3Class : IDisposable
    {
        #region NOT_USED_LEGACY_MVD_TOOLS
        //CPositionFixTool cPositionFixToolObj = null;// new VisionDesigner.PositionFix.CPositionFixTool();
        //CImageArithmeticTool cImageArithmeticToolObj = null;// new CImageArithmeticTool();
        //VisionDesigner.ImageBinary.CImageBinaryTool cImageBinaryToolObj = null;// new VisionDesigner.ImageBinary.CImageBinaryTool();
        //VisionDesigner.ImageMorph.CImageMorphTool cImageMorphToolObj = null;// new VisionDesigner.ImageMorph.CImageMorphTool();
        //VisionDesigner.BlobFind.CBlobFindTool cBlobFindToolObj = null;// new VisionDesigner.BlobFind.CBlobFindTool();
        //VisionDesigner.ImageAffineTransform.CImageAffineTransformTool cImageAffineTransformToolObj = null;
        //Mvd2DReaderClass mvd2DReader = null;// new Mvd2DReaderClass();
        //MvdFindLineClass mvdFindLineClass = null;
        //MvdPairLineClass mvdPairLineClass = null;
        //CMvdRectangleF MvdRunPositionFix;
        #endregion

        #region GLOBAL_MESS
        public InspectX3ParaClass xInspect
        {
            get => RecipeFPIX3Class.Instance.InspectParams;
        }
        static INI _INI => INI.Instance;
        #endregion

        public RegionCellX3Class()
        {
        }

        public void Dispose()
        {
            //cPositionFixToolObj?.Dispose();
            //cPositionFixToolObj = null;
            //cImageArithmeticToolObj?.Dispose();
            //cImageArithmeticToolObj = null;
            //cImageBinaryToolObj?.Dispose();
            //cImageBinaryToolObj = null;
            //cImageMorphToolObj?.Dispose();
            //cImageMorphToolObj = null;
            //cBlobFindToolObj?.Dispose();
            //cBlobFindToolObj = null;
            //mvd2DReader?.Dispose();
            //mvd2DReader = null;
            //mvdFindLineClass?.Dispose();
            //mvdFindLineClass = null;
            //mvdPairLineClass?.Dispose();
            //mvdPairLineClass = null;
            //cImageAffineTransformToolObj?.Dispose();
            //cImageAffineTransformToolObj = null;

            try { OutGridLink?.Dispose(); } catch { }
            OutGridLink = null;
        }

        // INDEX
        #region INDEX_LABEL_AND_ROW_COLS
        public int Index = 0;
        public string lblName = "";
        public int CellRow = 0;
        public int CellCol = 0;
        public bool ByPass { get; set; } = false;   // 沒用到
        #endregion

        /// <summary>
        /// runtime chip ROI (FullFov Camera Coordinates) (單位 pixel)
        /// 更精確(內縮)的晶粒矩形區域 (對應 xRecipe.xRegionTrain) 
        /// </summary>
        public RectangleF viewRectF = new RectangleF();
        /// <summary>
        /// 優化後, 晶粒 像測的詳細數據 皆放置於此 
        /// </summary>
        public GaChipData ChipData = new GaChipData();
        /// <summary>
        /// 用來連結 最接近此格位 的 散落在外圍 晶粒
        /// (2025-10-22 新增)
        /// </summary>
        public RegionCellX3Class OutGridLink { get; set; } = null;

        /// <summary>
        /// 是否已經定位成功
        /// </summary>
        public bool IsLocated()
        {
            return ChipData != null && ChipData.ChipQuad2D != null;
        }

        #region PUBLIC_CACHE_PROPERTIES
        /// <summary>
        /// 理想 格位中心座標 X (單位 mm)
        /// </summary>
        public float OrgX { get; set; } = 0;
        /// <summary>
        /// 理想 格位中心座標 Y (單位 mm)
        /// </summary>
        public float OrgY { get; set; } = 0;
        /// <summary>
        /// PLC 定位補償 X (單位 mm)
        /// </summary>
        public float RunX = 0;
        /// <summary>
        /// PLC 定位補償 Y (單位 mm)
        /// </summary>
        public float RunY = 0;
        /// <summary>
        /// PLC 定位角度 A (單位 degree)
        /// </summary>
        public float RunAngle = 0;
        /// <summary>
        /// 推算馬達座標: 吸嘴排1 (單位 mm)
        /// </summary>
        public PointF Sur1 = new PointF();
        /// <summary>
        /// 推算馬達座標: 吸嘴排2 (單位 mm)
        /// </summary>
        public PointF Sur2 = new PointF();
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
        #endregion

        #region NOT_USED_LEGACY_OLD_CODE
#if (false)
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
#endif
        #endregion

        #region NOT_USED_LEGACY_IMAGE_FILE_DUMP_SETTINGS
        //public static bool IsSaveDebugPicture => _INI.IsSaveTestImage;
        //public static string SaveDebugPath { get; set; } = "D:\\log\\DebugImage";
        #endregion

        #region NOT_USED_LEGACY_OLD_CODE
        //>>> private AUVision.xFindResult xFindResult = new AUVision.xFindResult();
        #endregion

        #region PRIVATE_INSPECTION_RESULTS_DATA
        List<InspectReason> _inspectNgList = new List<InspectReason>();
        InspectReason _inspectResult = InspectReason.PASS;
        #endregion

        /// <summary>
        /// 檢測總合結果
        /// </summary>
        public InspectReason FinalInspectResult
        {
            get => _inspectResult;
        }
        /// <summary>
        /// 檢測總合結果 為 PASS
        /// </summary>
        public bool IsResultPass()
        {
            return _inspectResult == InspectReason.PASS && _inspectNgList.Count == 0;
        }
        /// <summary>
        /// 是否為 吸嘴空格
        /// </summary>
        public bool IsEmptyPlaceHold()
        {
            // 是否為 吸嘴空格
            return _inspectResult == InspectReason.NG_EMPTY;
        }
        /// <summary>
        /// 是否為 疑似有料 之 不明區塊 
        /// (2025-11-17 新增, 用來標記 踩腳)
        /// </summary>
        public bool IsAmbiguousBloc()
        {
            return _inspectResult == InspectReason.NG_AMBIGUOUS_BLOC;
        }
        /// <summary>
        /// 標記 檢測結果
        /// </summary>
        public void MarkResult(InspectReason result, bool reset = false)
        {
            if (reset)
            {
                _inspectResult = result;
                _inspectNgList.Clear();
            }
            else
            {
                // 已經是空格: 不能被執行檢測 (再被指定其他結果碼)
                if (IsEmptyPlaceHold())
                    return;

                _inspectResult = result;

                if (result != InspectReason.PASS)
                    _inspectNgList.Add(result);
            }
        }
        /// <summary>
        /// 枚舉所有 NG
        /// </summary>
        public IEnumerable<InspectReason> IterNgResults(bool reverse = false)
        {
            if (_inspectNgList != null)
            {
                if (reverse)
                {
                    for (int i = _inspectNgList.Count - 1; i >= 0; i--)
                        yield return _inspectNgList[i];
                }
                else
                {
                    foreach (var ng in _inspectNgList)
                        yield return ng;
                }
            }
        }
        /// <summary>
        /// 根據 inspectResult 傳回是否為 "疑似有料"
        /// </summary>
        public string GetNoTrayDesc()
        {
            string str = string.Empty;
            if (_inspectResult == InspectReason.NG_APPEARANCE)
            {
                str = "疑似有料";
                return str;
            }
            foreach (var reason in _inspectNgList)
            {
                if (reason == InspectReason.NG_APPEARANCE)
                {
                    str = "疑似有料";
                    //str = "Maybe.P";
                    break;
                }
            }
            return str;
        }

        #region NOT_USED_LEGACY_OLD_CODE
        ///// <summary>
        ///// 外围的直线
        ///// </summary>
        //public CMvdLineSegmentF[] cMvdLineSegmentFsOut = new CMvdLineSegmentF[4];
        ///// <summary>
        ///// 内围的直线
        ///// </summary>
        //public CMvdLineSegmentF[] cMvdLineSegmentFsInSide = new CMvdLineSegmentF[4];
        ///// <summary>
        ///// Runtime 跑線後的 LineSegment Border Box2D
        ///// </summary>
        //public CMvdShape[] cMvdShapesForFindLineRegion = new CMvdShape[4];
        #endregion

        #region NOT_USED_LEGACY_MVD_IMAGE_AFFINETRANSFORM
#if (OPT_REPLACED_BY_MvdDefectDetector)
        /// <summary>
        /// 抓取全域图形的转正图像 位置框由前期定位决定
        /// </summary>
        /// <param name="cMvdImage">全域图像</param>
        /// <returns></returns>
        public Bitmap GetAffineTrainsFormRunBmp(CMvdImage cMvdImage, CMvdShape cMvdShape)
        {
            if (cImageAffineTransformToolObj == null)
                cImageAffineTransformToolObj = new VisionDesigner.ImageAffineTransform.CImageAffineTransformTool();
            cImageAffineTransformToolObj.BasicParam.Aspect = 1;
            cImageAffineTransformToolObj.InputImage = cMvdImage;
            cImageAffineTransformToolObj.ROIShape = cMvdShape;
            cImageAffineTransformToolObj.Run();
            //输出结果
            return CMvdImageToBitmapEx(cImageAffineTransformToolObj.Result.OutputImage);
        }
        /// <summary>
        /// cMvdImage 转 Bitmap
        /// </summary>
        /// <param name="cMvdImage"></param>
        /// <returns></returns>
        Bitmap CMvdImageToBitmapEx(CMvdImage cMvdImage)
        {
            Bitmap bmpInputImg = null;
            byte[] buffer = new byte[cMvdImage.GetImageData(0).arrDataBytes.Length];
            buffer = cMvdImage.GetImageData(0).arrDataBytes;

            if (MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08 == cMvdImage.PixelFormat)
            {
                Int32 imageWidth = Convert.ToInt32(cMvdImage.Width);
                Int32 imageHeight = Convert.ToInt32(cMvdImage.Height);
                PixelFormat bitMaPixelFormat = PixelFormat.Format8bppIndexed;
                bmpInputImg = new Bitmap(imageWidth, imageHeight, bitMaPixelFormat);
                int offset = imageWidth % 4 != 0 ? (4 - imageWidth % 4) : 0;//添加冗余位，变成4的倍数
                int strid = imageWidth + offset;
                int bitmapBytesLenth = strid * imageHeight;
                byte[] bitmapDataBytes = new byte[bitmapBytesLenth];
                for (int i = 0; i < imageHeight; i++)
                {
                    for (int j = 0; j < strid; j++)
                    {
                        int bitIndex = i * strid + j;
                        int mvdIndex = i * imageWidth + j;
                        if (j >= imageWidth)
                        {
                            bitmapDataBytes[bitIndex] = 0;//冗余位填充0
                        }
                        else
                        {
                            bitmapDataBytes[bitIndex] = buffer[mvdIndex];
                        }
                    }
                }
                BitmapData bitmapData = bmpInputImg.LockBits(new Rectangle(0, 0, imageWidth, imageHeight), ImageLockMode.WriteOnly, bitMaPixelFormat);
                IntPtr imageBufferPtr = bitmapData.Scan0;
                Marshal.Copy(bitmapDataBytes, 0, imageBufferPtr, bitmapBytesLenth);
                bmpInputImg.UnlockBits(bitmapData);

                var colorPalettes = bmpInputImg.Palette;
                for (int j = 0; j < 256; j++)
                {
                    colorPalettes.Entries[j] = Color.FromArgb(j, j, j);
                }
                bmpInputImg.Palette = colorPalettes;
            }
            else if (MVD_PIXEL_FORMAT.MVD_PIXEL_RGB_RGB24_C3 == cMvdImage.PixelFormat)
            {
                Int32 imageWidth = Convert.ToInt32(cMvdImage.Width);
                Int32 imageHeight = Convert.ToInt32(cMvdImage.Height);
                PixelFormat bitMaPixelFormat = PixelFormat.Format24bppRgb;
                bmpInputImg = new Bitmap(imageWidth, imageHeight, bitMaPixelFormat);
                int offset = imageWidth % 4 != 0 ? (4 - (imageWidth * 3) % 4) : 0;//添加冗余位，变成4的倍数
                int strid = imageWidth * 3 + offset;
                int bitmapBytesLenth = strid * imageHeight;
                byte[] bitmapDataBytes = new byte[bitmapBytesLenth];
                for (int i = 0; i < imageHeight; i++)
                {
                    for (int j = 0; j < imageWidth; j++)
                    {
                        int mvdIndex = i * imageWidth * 3 + j * 3;
                        int bitIndex = i * strid + j * 3;
                        bitmapDataBytes[bitIndex] = buffer[mvdIndex + 2];
                        bitmapDataBytes[bitIndex + 1] = buffer[mvdIndex + 1];
                        bitmapDataBytes[bitIndex + 2] = buffer[mvdIndex];
                    }
                    for (int k = 0; k < offset; k++)
                    {
                        bitmapDataBytes[i * strid + imageWidth * 3 + k] = 0;
                    }
                }
                BitmapData bitmapData = bmpInputImg.LockBits(new Rectangle(0, 0, imageWidth, imageHeight), ImageLockMode.WriteOnly, bitMaPixelFormat);
                IntPtr imageBufferPtr = bitmapData.Scan0;
                Marshal.Copy(bitmapDataBytes, 0, imageBufferPtr, bitmapBytesLenth);
                bmpInputImg.UnlockBits(bitmapData);
            }
            return bmpInputImg;
        }
#endif
        #endregion

        #region NOT_USED_LEGACY_MVD_LINE_SEGMENTS
#if (OPT_NOT_USED_LEGACY)
        /// <summary>
        /// 寻找直线
        /// </summary>
        public CMvdLineSegmentF LineSegmentRun(int borderIndex, Bitmap bmp, CMvdRectangleF roi, double angleRef = 0)
        {
            CMvdLineSegmentF resultLine = null;

            int NP = 4;

            if (mvdFindLineClass == null)
                mvdFindLineClass = new MvdFindLineClass();

            //borderIndex %= NP;
            //cMvdLineSegmentFsOut[borderIndex] = null;

            //>>> 根據 angleRef 將 borderIndex 正規化
            int sideIndex;
            if (angleRef > 70.0)
            {
                sideIndex = (borderIndex + 1) % NP;
            }
            else if (angleRef < -70.0)
            {
                sideIndex = borderIndex == 0 ? NP - 1 : (borderIndex - 1) % NP;
            }
            else
            {
                sideIndex = borderIndex;
            }

            // 左
            if (sideIndex == 0)
            {
                mvdFindLineClass.bPositive = xInspect.bPositive0;
                mvdFindLineClass.bFindOrient = true;
                mvdFindLineClass.bEdgePolarity = xInspect.bEdgePolarity0;
            }
            // 上
            else if (sideIndex == 1)
            {
                mvdFindLineClass.bPositive = xInspect.bPositive1;
                mvdFindLineClass.bFindOrient = false;
                mvdFindLineClass.bEdgePolarity = xInspect.bEdgePolarity1;
            }
            // 右
            else if (sideIndex == 2)
            {
                mvdFindLineClass.bPositive = xInspect.bPositive2;
                mvdFindLineClass.bFindOrient = true;
                mvdFindLineClass.bEdgePolarity = xInspect.bEdgePolarity2;
            }
            // 下
            else if (sideIndex == 3)
            {
                mvdFindLineClass.bPositive = xInspect.bPositive3;
                mvdFindLineClass.bFindOrient = false;
                mvdFindLineClass.bEdgePolarity = xInspect.bEdgePolarity3;
            }

            mvdFindLineClass.Background = xInspect.xCarrierBackground;
            resultLine = mvdFindLineClass.Run(bmp, roi, sideIndex);

            //cMvdLineSegmentFsOut[borderIndex] = resultLine;
            return resultLine;
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
#endif
        #endregion

        #region RUNTIME_QRCODE_RESULT_DATA
        public string BarcodeResultText { get; set; } = "";
        public QvQuad2D BarcodeResultQuad { get; set; } = null;
        //public C2DCodeInfo RunCodeInfo { get; set; } = null;
        //public CMvdPolygonF DrawBarcodePosition { get; set; } = null;
        #endregion

        #region NOT_USED_LEGACY_MVD_DEFAULT_RECTF
#if (OPT_NOT_USED_LEGACY)
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

            //switch (_inspectResult)
            //{
            //    case InspectReason.NG_EMPTY:
            //        MvdRunPositionFix = GaImageUtil.ToCMvdRectangleF(ref viewRectF);
            //        MvdRunPositionFix.BorderColor = new MVD_COLOR(255, 0, 0);
            //        break;
            //    case InspectReason.PASS:
            //        if (_inspectNGs.Count > 0)
            //            MvdRunPositionFix.BorderColor = new MVD_COLOR(255, 0, 0);
            //        else
            //            MvdRunPositionFix.BorderColor = new MVD_COLOR(0, 255, 0);
            //        break;
            //    default:
            //        MvdRunPositionFix.BorderColor = new MVD_COLOR(255, 0, 0);
            //        break;
            //}

            if (IsEmptyPlaceHold())
            {
                MvdRunPositionFix = GaImageUtil.ToCMvdRectangleF(ref viewRectF);
                MvdRunPositionFix.BorderColor = new MVD_COLOR(255, 0, 0);
            }
            else if (IsResultPass())
            {
                MvdRunPositionFix.BorderColor = new MVD_COLOR(0, 255, 0);
            }
            else
            {
                MvdRunPositionFix.BorderColor = new MVD_COLOR(255, 0, 0);
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
            //MvdRunPositionFix = mvdRect;
        }
#endif
        #endregion

        #region NOT_USED_LEGACY_MVD_FOR_EMPTY_TRAY
#if (OPT_NOT_USED_LEGACY)
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
            if (_inspectResult == InspectReason.NG_APPEARANCE)
            {
                str = "疑似有料";
                return str;
            }
            foreach (var reason in _inspectNgList)
            {
                if (reason == InspectReason.NG_APPEARANCE)
                {
                    str = "疑似有料";
                    //str = "Maybe.P";
                    break;
                }
            }
            return str;
        }
#endif
        #endregion

        #region NOT_USED_LEGACY_MVD_DEFECT_INSPECT_MEMBERS
#if (OPT_REPLACED_BY_MvdDefectDetector)
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
#endif
        #endregion

        #region NOT_USED_LEGACY_OLD_CODE
#if (OPT_MOVED_TO_AOI_MODEL)
        /// <summary>
        /// 整理打包 尺寸计算 的 最後判定结果
        /// </summary>
        /// <returns>true:OK false:NG</returns>
        public bool PackMeasureResult()
        {
            bool bOK = true;

            if (xInspect.optChipMeasurement)
            {
                //(1) 判定 長寬 是否達標
                var dimResults = new bool[2];
                bOK &= (dimResults[0] = !(RunWidth < xInspect.mWidthStandMin || RunWidth > xInspect.mWidthStandMax));
                bOK &= (dimResults[1] = !(RunHeight < xInspect.mHeightStandMin || RunHeight > xInspect.mHeightStandMax));
                var chipDim = ChipData?.ChipDimension;
                if (chipDim != null)
                    chipDim.PassNgResults = dimResults;

                if (!bOK)
                {
                    //inspectReason = InspectReason.INS_CUTTINGERR;
                    //inspectReasons.Add(InspectReason.INS_CUTTINGERR);
                    this.MarkResult(InspectReason.NG_CUT);
                }

                //(2) 判定 邊隙 是否達標
                if (xInspect.optPadEdgeGapsMeasurement && xInspect.xAlgorithm == MatchAlgorithmEnum.GridMatch)
                {
                    var gaps = ChipData?.PadEdgeGaps;
                    if (gaps == null)
                    {
                        bOK = false;
                    }
                    else
                    {
                        var min = new QVector(xInspect.PadEdgeGapX_Min, xInspect.PadEdgeGapY_Min);
                        var max = new QVector(xInspect.PadEdgeGapX_Max, xInspect.PadEdgeGapY_Max);
                        gaps.Check(out bOK, min, max);
                    }

                    if (!bOK)
                    {
                        //inspectReason = InspectReason.INS_PADEDGEGAPERR;
                        //inspectReasons.Add(InspectReason.INS_PADEDGEGAPERR);
                        MarkResult(InspectReason.NG_EDGE_GAP);
                    }
                }

                
            }
            return bOK;
        }
#endif
        #endregion

        #region NOT_USED_LEGACY_OLD_CODE_不要在此生成_客戶要求的_顯示與報表_字串格式_不然換不同廠家就要跟著一直變動_CELL
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

            try { OutGridLink?.Dispose(); } catch { }
            OutGridLink = null;

            ////<<< 廢除 >>> xFindResult = new AUVision.xFindResult();

            //inspectReason = InspectReason.PASS;
            //inspectReasons.Clear();
            MarkResult(InspectReason.PASS, reset: true);

            //RunCodeInfo = null;
            //if (mvd2DReader != null)
            //    mvd2DReader.DCodeInfo = null;
            //DrawBarcodePosition = null;
            BarcodeResultText = "";
            BarcodeResultQuad = null;

            RunX = 0;
            RunY = 0;
            RunAngle = 0;
            //IsSaveDebugPicture = false;

            //int i = 0;
            //while (i < 4)
            //{
            //    //CMvdLineSegmentF mLine = cMvdLineSegmentFsOut[i];
            //    if (cMvdLineSegmentFsOut[i] != null)
            //        cMvdLineSegmentFsOut[i] = null;
            //    if (cMvdLineSegmentFsInSide[i] != null)
            //        cMvdLineSegmentFsInSide[i] = null;
            //    CMvdShape mvdShape = cMvdShapesForFindLineRegion[i];
            //    if (cMvdShapesForFindLineRegion[i] != null)
            //        cMvdShapesForFindLineRegion[i] = null;
            //    i++;
            //}

            RunWidth = 0;
            RunHeight = 0;

            //DisLeft = 0;
            //DisTop = 0;
            //DisRight = 0;
            //DisBottom = 0;
        }

        #region NOT_USED_LEGACY_MVD_AOI_FUNCTIONS
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

#if (OPT_NOT_USED_LEGACY)
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
#endif

#if (OPT_REPLACED_BY_MvdDefectDetector)
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
                //inspectReasons.Add(InspectReason.INS_DEFECTERR);
                MarkResult(InspectReason.NG_APPEARANCE);

                List<CMvdRectangleF> cMvdRectangleFs = new List<CMvdRectangleF>();
                foreach (CMvdRectangleF mvdRectangleF in _blobMvdRectFNGList)
                {
                    mvdRectangleF.CenterX += MvdRunPositionFix.CenterX - _defectRunRoiSize.Width / 2;  // bmpItemRun.Width / 2;
                    mvdRectangleF.CenterY += MvdRunPositionFix.CenterY - _defectRunRoiSize.Height / 2; // bmpItemRun.Height / 2;
                    //mvdRectangleF.BorderColor = new MVD_COLOR(255, 0, 0);
                    //mvdRectangleF.BorderWidth = 1;
                    cMvdRectangleFs.Add(mvdRectangleF);
                }

                ChipData.DefectBlobs = Array.ConvertAll(cMvdRectangleFs.ToArray(), b => b.ToBox2D());
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
                    cImageArithmeticToolObj?.InputImage1?.SaveImage(fileStem + "_Template.bmp", MVD_FILE_FORMAT.MVD_FILE_BMP);
                    cImageArithmeticToolObj?.InputImage2?.SaveImage(fileStem + "_Run.bmp", MVD_FILE_FORMAT.MVD_FILE_BMP);
                    cImageBinaryToolObj?.Result?.OutputImage?.SaveImage(fileStem + "_Diff2.bmp", MVD_FILE_FORMAT.MVD_FILE_BMP);
                    cBlobFindToolObj?.RegionImage?.SaveImage(fileStem + "_Diff2_1.bmp", MVD_FILE_FORMAT.MVD_FILE_BMP);
                    cBlobFindRes?.BlobImage?.SaveImage(fileStem + "_Diff3.bmp", MVD_FILE_FORMAT.MVD_FILE_BMP);
                }
            }
        }
#endif

#if (OPT_NOT_USED_LEGACY)
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

                        //inspectReasons.Add(InspectReason.INS_2DMAPNG);
                        MarkResult(InspectReason.NG_QRCODE_COMPARE);
                    }
                }
            }
            else
            {
                //inspectReasons.Add(InspectReason.INS_2DERR);
                MarkResult(InspectReason.NG_QRCODE_ERR);

                #region GENERATE_MVD_GUI_COMPONENT_UGLY_CODE
                DrawBarcodePosition = null;

                if (_INI.IsSaveTestImage)
                {
                    //string dumpFolder = System.IO.Path.Combine(SaveDebugPath, "Code");
                    //JetEazy.IO.QxPathUtility.InitDirectory(dumpFolder);
                    ////string fileName = System.IO.Path.Combine(dumpFolder, $"{lblName}_Run.bmp");
                    //string fname = $"Cell_{this.Index}@{this.CellRow}_{this.CellCol}.bmp";
                    //string fileName = System.IO.Path.Combine(dumpFolder, fname);
                    //mvd2DReader.MvdRunImage.SaveImage(fileName, MVD_FILE_FORMAT.MVD_FILE_BMP);

                    LotDataHolder.Instance.DumpImage(this, mvd2DReader.MvdRunImage, "QRCode", "QRCode");
                }

                #endregion
            }
        }
#endif
        #endregion

        public override string ToString()
        {
            return $"Cell[{CellRow},{CellCol}] {_inspectResult}";
        }
    }
}
