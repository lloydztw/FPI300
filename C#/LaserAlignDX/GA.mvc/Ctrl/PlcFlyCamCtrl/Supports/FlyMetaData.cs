#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-09-22 重整 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.QvMath;
using System.Drawing;


namespace LaserAlignDX.AoiModel
{
    /// <summary>
    /// 用來讓 GUI 畫圖的額外資料
    /// </summary>
    public class FlyMetaData
    {
        public string AlgorithmName;
        public FlyID flyID;
        public FlyAoiResult flyAoiResult;
        public Bitmap bmpFly;
        public RectangleF roiRect;
        public RectangleF xTemplateRect;
        public PointF xCentroid
        {
            get
            {
                //if (xResult != null)
                //{
                //    float x = xResult.Value.fCenterX + roiRect.X;
                //    float y = xResult.Value.fCenterY + roiRect.Y;
                //    return new PointF(x, y);
                //}
                //else
                //{
                //    return JetEazy.Qcvt.CenterF(ref roiRect);
                //}
                if (xResultBox2D != null)
                {
                    var cc = xResultBox2D.Center;
                    cc.X += roiRect.X;
                    cc.Y += roiRect.Y;
                    return cc;
                }
                else
                {
                    return JetEazy.Qcvt.CenterF(ref roiRect);
                }
            }
        }
        public QvBox2D xResultBox2D;
        public QvBox2D[] xBlobs;
    };
}
