#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-08-29 重整 Gaara 原來的 MvdFindLineClass (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion


using JetEazy.Utils;
using LeTian.AoiLib;
using System;
using System.Drawing;
using VisionDesigner;


namespace LaserAlignDX.BasicSpace.LineFinder.V1
{
    /// <summary>
    /// 修正 原先 Gaara 都沒有釋放 CLineFindTool 與 CMvdImage 寫法
    /// </summary>
    public class MvdFindLineClass : IMvdLineFinder
    {
        #region MVD_TOOL
        VisionDesigner.LineFind.CLineFindTool _mvdLineFindTool = null;
        #endregion

        public MvdFindLineClass()
        {
            _mvdLineFindTool = new VisionDesigner.LineFind.CLineFindTool();
        }

        public void Dispose()
        {
            _mvdLineFindTool?.Dispose();
            _mvdLineFindTool = null;
        }

        #region OLD_SETTINGS
        /// <summary>
        /// 寻找方向 左右型 true从左到右 上下型 true从上到下
        /// </summary>
        bool IMvdLineFinderParams.bPositive { get; set; } = true;
        /// <summary>
        /// 搜寻方向 true左右型 false上下型
        /// </summary>
        bool IMvdLineFinderParams.bFindOrient { get; set; } = true;
        /// <summary>
        /// 边缘极性 true白到黑 false黑到白
        /// </summary>
        bool IMvdLineFinderParams.bEdgePolarity { get; set; } = true;
        /// <summary>
        /// 卡尺数量
        /// </summary>
        int IMvdLineFinderParams.iRayNum { get; set; } = 100;
        /// <summary>
        /// 边缘强度
        /// </summary>
        public int iEdgeStrength 
        {
            get => EdgeStrength; 
            set => EdgeStrength = value;
        }
        #endregion

        /// <summary>
        /// 是否為黑色載台背景
        /// </summary>
        public EdgeBackGroundType Background
        {
            get; set; 
        }

        /// <summary>
        /// 边缘强度
        /// </summary>
        public int EdgeStrength { get; set; } = 5;

        public CMvdLineSegmentF Run(Bitmap bmpInput, RectangleF roi, int borderId = -1)
        {
            var mvdRect = GaImageUtil.ToCMvdRectangleF(ref roi);
            using (var mvdImage = GaImageUtil.BitmapToCMvdImage(bmpInput))
            {
                return runMvd(mvdImage, mvdRect, borderId);
            }
        }

        public CMvdLineSegmentF Run(Bitmap bmpInput, CMvdRectangleF roi, int borderId = -1)
        {
            using (var mvdImage = GaImageUtil.BitmapToCMvdImage(bmpInput))
            {
                return runMvd(mvdImage, roi, borderId);
            }
        }

        #region PRIVATE_FUNCIONS
        private CMvdLineSegmentF runMvd(CMvdImage cInputImg, CMvdRectangleF roi, int borderId)
        {
            CMvdLineSegmentF retLineSegment = null;

            try
            {
                _mvdLineFindTool.InputImage = cInputImg;

                // 边缘极性: 從背景進入晶粒的變化
                var polarity = Background == EdgeBackGroundType.Dark? 
                            EdgeSearchPolarity.BlackToWhite: 
                            EdgeSearchPolarity.WhiteToBlack;

                // 搜索方向
                string orient;
                switch ((EdgeBorder)borderId)
                {
                    case EdgeBorder.Left:
                        orient = "LeftToRight";
                        break;
                    case EdgeBorder.Top:
                        orient = "UpToDown";
                        break;
                    case EdgeBorder.Right:
                        //MVD 沒有 RightToLeft, 只能用 LeftToRight 搭配 反向 polarity
                        //orient = "RightToLeft";
                        orient = "LeftToRight";
                        polarity = invert(polarity);
                        //inverse = true;
                        break;
                    case EdgeBorder.Bottom:
                        //MVD 沒有 DownToUp, 只能用 UpToDown 搭配 反向 polarity
                        //orient = "DownToUp";
                        orient = "UpToDown";
                        polarity = invert(polarity);
                        //inverse = true;
                        break;
                    default:
                        orient = "LeftToRight";
                        break;
                }

                // 調整 Roi 角度 (貌似不用設定)
                // if (inverse)
                //    adjustRoiAngle(roi, inverse);

                _mvdLineFindTool.ROI = roi;


                int rayNum = (int)Math.Max(roi.Width, roi.Height) / 3;  // 自動決定
                int edgeStrength = iEdgeStrength;

                LtDebug.LOG.Debug("MvdLineFinder[{0}] : Orient= {1} : Roi= {2:0.0} x {3:0.0} : A= {4:0.0}",
                                   borderId, orient, roi.Width, roi.Height, roi.Angle);

                // 卡尺数量
                _mvdLineFindTool.SetRunParam("RayNum", rayNum.ToString());     
                
                //cLineFindToolObj.SetRunParam("RejectNum", "30");//剔除点数
                //cLineFindToolObj.SetRunParam("RejectDist", "10");//剔除距离
          
                // 搜索方向
                _mvdLineFindTool.SetRunParam("FindOrient", orient);
                // 边缘极性
                _mvdLineFindTool.SetRunParam("EdgePolarity", ToParamString(polarity));
                // 边缘强度
                _mvdLineFindTool.SetRunParam("EdgeStrength", edgeStrength.ToString());
                // 查找模式
                _mvdLineFindTool.SetRunParam("LineFindMode", "Best");
                // KernelSize
                //_mvdLineFindTool.SetRunParam("KernelSize", "5");

                LtDebug.LOG.Debug("MvdLineFinder[{0}] : SetRunParam [OK]", borderId);

                // Running
                _mvdLineFindTool.Run();

                //  Get the result
                var mvdResult = _mvdLineFindTool.Result;

                //LtDebug.LOG.Debug("MvdLineFinder[{0}]: Result Status = {1}", borderId, mvdResult.Status);
                //LtDebug.LOG.Debug("MvdLineFinder[{0}]: Start point of line = ({1},{2}）", borderId, mvdResult.LineStartPoint.fX, mvdResult.LineStartPoint.fY);

                if (mvdResult.Status == 1)
                {
                    retLineSegment = new CMvdLineSegmentF(mvdResult.LineStartPoint, mvdResult.LineEndPoint);
                }
                else
                {
                    LtDebug.LOG.Warn("MvdLineFinder[{0}]: 找不到邊線 Result Status = {1}", borderId, mvdResult.Status);
                }
            }
            catch (MvdException ex)
            {
                LtDebug.LOG.Error(ex, "MvdFindLineClass: Fail with ErrorCode: 0x{0:X}", ex.ErrorCode);
            }
            catch (System.Exception ex)
            {
                LtDebug.LOG.Error(ex, "MvdFindLineClass: Fail");
            }
            return retLineSegment;
        }

        void adjustRoiAngle(CMvdRectangleF roi, bool inverse)
        {
            bool bPositive = !inverse;
            if (roi.Angle > 0)
                roi.Angle = (bPositive ? roi.Angle : roi.Angle - 180);
            else
                roi.Angle = (bPositive ? roi.Angle + 180 : roi.Angle);
        }

        EdgeSearchPolarity invert(EdgeSearchPolarity polarity)
        {
            if (polarity == EdgeSearchPolarity.WhiteToBlack)
                return EdgeSearchPolarity.BlackToWhite;
            else if (polarity == EdgeSearchPolarity.BlackToWhite)
                return EdgeSearchPolarity.WhiteToBlack;
            else
                return polarity;
        }

        string ToParamString(EdgeSearchPolarity polarity)
        {
            return polarity.ToString().Replace("_", "");
        }
        #endregion
    }
}
