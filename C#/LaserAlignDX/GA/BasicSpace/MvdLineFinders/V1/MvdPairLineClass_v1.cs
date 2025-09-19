using JetEazy.Utils;
using LeTian.AoiLib;
using System;
using System.Drawing;
using VisionDesigner;
using VisionDesigner.PairLineFind;


namespace LaserAlignDX.BasicSpace.LineFinder.V1
{
    /// <summary>
    /// 修正 原先 Gaara 都沒有釋放 CPairLineFindTool 與 CMvdImage 寫法
    /// </summary>
    internal class MvdPairLineClassV1 : IMvdPairLineFinder
    {
        #region MVD_TOOL
        CPairLineFindTool _mvdPairLineFindTool;
        #endregion

        public MvdPairLineClassV1()
        {
            _mvdPairLineFindTool = new CPairLineFindTool();
        }
        public void Dispose()
        {
            _mvdPairLineFindTool?.Dispose();
            _mvdPairLineFindTool = null;
        }

        #region RECIPE_PARAMS
        /// <summary>
        /// 寻找方向 左右型 true从左到右 上下型 true从上到下
        /// </summary>
        public bool bPositive { get; set; } = true;
        /// <summary>
        /// 搜寻方向 true左右型 false上下型
        /// </summary>
        public bool bFindOrient { get; set; } = true;
        ///// <summary>
        ///// 边缘极性 true白到黑 false黑到白
        ///// </summary>
        //public bool bEdgePolarity { get; set; } = true;
        /// <summary>
        /// 卡尺数量
        /// </summary>
        public int CaliperNum { get; set; } = 100;
        /// <summary>
        /// 边缘强度
        /// </summary>
        public int iEdgeStrength { get; set; } = 5;
        #endregion

        public CPairLineFindResult Run(Bitmap bmpInput, RectangleF roi, int borderId = -1)
        {
            var mvdRect = GaImageUtil.ToCMvdRectangleF(ref roi);
            using (var mvdImage = GaImageUtil.BitmapToCMvdImage(bmpInput))
            {
                return runMvd(mvdImage, mvdRect, borderId);
            }
        }

        public CPairLineFindResult Run(Bitmap bmpInput, CMvdRectangleF roi, int borderId = -1)
        {
            using (var mvdImage = GaImageUtil.BitmapToCMvdImage(bmpInput))
            {
                return runMvd(mvdImage, roi, borderId);
            }
        }

        #region PRIVATE_FUNCTIONS
        private CPairLineFindResult runMvd(CMvdImage mvdImage, CMvdRectangleF roi, int borderId = -1)
        {
            //PointF p1 = new PointF(0, 0);
            //PointF p2 = new PointF(bmpInput.Width, 0);

            CPairLineFindResult cPairLineFindRes = null;

            try
            {
                _mvdPairLineFindTool.InputImage = mvdImage;

                // Set ROI region (optional)
                var roiCopy = new CMvdRectangleF(roi);
                if (roiCopy.Angle > 0)
                    roiCopy.Angle = (bPositive ? roiCopy.Angle : roiCopy.Angle - 180);
                else
                    roiCopy.Angle = (bPositive ? roiCopy.Angle + 180 : roiCopy.Angle);

                _mvdPairLineFindTool.ROI = roiCopy;

                _mvdPairLineFindTool.SetRunParam("CaliperNum", CaliperNum.ToString());//卡尺数量
                _mvdPairLineFindTool.SetRunParam("FindOrient", (bFindOrient ? "LeftToRight" : "UpToDown"));//搜索方向
                _mvdPairLineFindTool.SetRunParam("Edge0Polarity", "Both");//边缘极性
                _mvdPairLineFindTool.SetRunParam("Edge1Polarity", "Both");//边缘极性
                _mvdPairLineFindTool.SetRunParam("EdgeStrength", iEdgeStrength.ToString());//边缘强度

                _mvdPairLineFindTool.SetRunParam("FitInitType", "LLS");//拟合初始化类型
                _mvdPairLineFindTool.SetRunParam("ProjectLen", "5");//卡尺宽度
                _mvdPairLineFindTool.SetRunParam("KernelSize", "1");//滤波核半宽

                // Running

                _mvdPairLineFindTool.Run();

                //  Get the result

                cPairLineFindRes = _mvdPairLineFindTool.Result;
                LtDebug.LOG.Debug("Recognition status: {0}", cPairLineFindRes.Status);
            }
            catch (MvdException ex)
            {
                LtDebug.LOG.Error(ex, "Fail with ErrorCode: 0x" + ex.ErrorCode.ToString("X"));
            }
            catch (System.Exception ex)
            {
                LtDebug.LOG.Error(ex, "Fail with error " + ex.Message);
            }
            return cPairLineFindRes;
        }
        #endregion
    }
}
