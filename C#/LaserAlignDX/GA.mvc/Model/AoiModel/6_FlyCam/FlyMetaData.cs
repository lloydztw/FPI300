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
using System.Collections.Generic;
using System.Drawing;


namespace LaserAlignDX.AoiModel
{
    /// <summary>
    /// 用來讓 GUI 畫圖的額外資料
    /// </summary>
    public class FlyMetaData
    {
        public string AlgorithmName;
        public FlyAoiResult flyAoiResult;
        public FlyID flyID;
        public Bitmap bmpFly;
        public RectangleF roiRect;
        public RectangleF xTemplateRect;
        public PointF xCentroid
        {
            get
            {
                if (xResultBox2D != null)
                {
                    var cc = xResultBox2D.Center;
                    //cc.X += roiRect.X;
                    //cc.Y += roiRect.Y;
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

        public static void Offset(QvBox2D box2D, float dx, float dy)
        {
            if (box2D != null)
            {
                var cc = box2D.Center;
                cc.X += dx;
                cc.Y += dy;
                box2D.SetCenter(cc);
            }
        }
        public static void Offset(IEnumerable<QvBox2D> box2Ds, float dx, float dy)
        {
            if (box2Ds != null)
            {
                foreach (var box2D in box2Ds)
                    Offset(box2D, dx, dy);
            }
        }
    }
}
