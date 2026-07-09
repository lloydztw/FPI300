#region AUTHOR
/*
 * 
 * Copyright (c) 2024 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2024-10-02 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.QMath;
using JetEazy.QvMath;
using System;
using System.Drawing;


namespace JetEazy.Match
{
    public partial class EzBloc
    {
        #region PRIVATE_DATA
        QVector _center;
        #endregion

        public Rectangle Rect;
        public QVector Center
        {
            get
            {
                if (_center == null)
                    _center = getCenter(ref Rect);
                return _center;
            }
            set
            {
                if (value != null)
                    _center = value;
            }
        }

        public double Score;
        public double SQRatio;
        public object Owner;
        public object Tag
        {
            get; set;
        }

        /// <summary>
        /// Runtime extra Data 
        /// (2025-10-15 新增)
        /// </summary>
        public QvBox2D ExtraBox2D
        {
            get; set;
        }

        protected EzBloc()
        {
        }
        public EzBloc(Rectangle rect, double score, object owner = null, object tag = null)
        {
            Rect = rect;
            Score = score;
            Owner = owner;
            Tag = tag;
            _center = getCenter(ref Rect);
        }
        public EzBloc Clone()
        {
            var blob = new EzBloc()
            {
                Rect = Rect,
                Score = Score,
                Owner = Owner,
                SQRatio = SQRatio,
                Pixels = Pixels
            };
            // 其他欄位
            ((IxBlob)blob).Bin = ((IxBlob)this).Bin;
            blob.ExtraBox2D = ExtraBox2D?.Clone();
            // 最後設定 (為了精度) !!!
            blob.Center = new QVector(Center);    
            return blob;
        }
        object ICloneable.Clone()
        {
            return Clone();
        }

        public bool IsMajorNode()
        {
            return Tag is QuadLinkNode;
        }

        /// <summary>
        /// 2025-10-06 新增
        /// </summary>
        public void Offset(float dx, float dy)
        {
            var cc = Center?.Offset(dx, dy);
            Rect.Offset((int)Math.Round(dx), (int)Math.Round(dy));
            if (cc != null)
                Center = cc;
            Offset(ExtraBox2D, dx, dy);
        }

        /// <summary>
        /// 2026-07-07 新增
        /// </summary>
        public void AdjustCenter(double cx, double cy)
        {
            var oldCenterX = Center.X;
            var oldCenterY = Center.Y;
            Center.X = cx;
            Center.Y = cy;
            Rect.X = (int)(cx - Rect.Width / 2);
            Rect.Y = (int)(cy - Rect.Height / 2);
            double dx = Center.X - oldCenterX;
            double dy = Center.Y - oldCenterY;
            Offset(ExtraBox2D, (float)dx, (float)dy);
        }

        /// <summary>
        /// 2025-10-15 新增
        /// </summary>
        public static void Offset(QvBox2D box, float dx, float dy)
        {
            if(box == null) return;
            var cc = box.Center;
            cc.X += dx;
            cc.Y += dy;
            box.SetCenter(cc);
        }

        #region PRIVATE_FUNCTIONS
        QVector getCenter(ref Rectangle rect)
        {
            //不要 Round, 以提高量測精度
            //var xCenter = Math.Round((rect.X + rect.Right) / 2.0);
            //var yCenter = Math.Round((rect.Y + rect.Bottom) / 2.0);
            var xCenter = (rect.X + rect.Right) / 2.0;
            var yCenter = (rect.Y + rect.Bottom) / 2.0;
            return new QVector(xCenter, yCenter);
        }
        #endregion
    }
}
