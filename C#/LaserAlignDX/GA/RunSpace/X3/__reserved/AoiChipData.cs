using JetEazy.QMath;
using JetEazy.QvMath;
using System.Drawing;
using System;


namespace LaserAlignDX.RunSpace
{
    public class AoiChipData
    {
        /// <summary>
        /// 晶粒位置 @ 線掃相機座標系
        /// </summary>
        public EzLocation2D LocationInCamera;
        /// <summary>
        /// PLC 補償量
        /// </summary>
        public QVector PlcCompensation;
    }


    public class EzLocation2D
    {
        /// <summary>
        /// 有旋轉角度的矩形
        /// </summary>
        public QvBox2D RotatedBox
        {
            get; set;
        }

        /// <summary>
        /// 旋轉角度 (degree)
        /// </summary>
        public double Angle
        {
            get => RotatedBox != null ? RotatedBox.Theta * 180.0 / Math.PI : 0;
        }

        /// <summary>
        /// 中心點 (向量)
        /// </summary>
        public QVector Centroid
        {
            get => RotatedBox != null ? new QVector(RotatedBox.Center.X, RotatedBox.Center.Y) : new QVector(0, 0);
        }

        /// <summary>
        /// 中心點 (C#)
        /// </summary>
        public PointF CenterPt
        {
            get => RotatedBox != null ? RotatedBox.Center : PointF.Empty;
        }

        /// <summary>
        /// 外廓矩形 (C#)
        /// </summary>
        public RectangleF BoundaryRect
        {
            get => RotatedBox != null ? RotatedBox.BoundaryRect : RectangleF.Empty;
        }

        public EzLocation2D()
        {
            RotatedBox = new QvBox2D();
        }

        public void Set(PointF center, SizeF minRectSize, double angleDegree)
        {
            var box = RotatedBox;
            box.MinAreaRectSize = minRectSize;
            box.SetCenter(center);
            box.SetTheta(angleDegree / 180.0 * Math.PI);
        }
    }
}
