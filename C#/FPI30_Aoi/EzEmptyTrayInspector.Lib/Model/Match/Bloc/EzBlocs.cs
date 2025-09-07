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
using System;
using System.Drawing;


namespace JetEazy.Match
{
    public partial class EzBloc
    {
        public Rectangle Rect;
        public QVector Center;
        public double Score;
        public double SQRatio;
        public object Owner;
        public object Tag
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
            // 不要 Round, 以提高量測精度
            //var xCenter = Math.Round((rect.X + rect.Right) / 2.0);
            //var yCenter = Math.Round((rect.Y + rect.Bottom) / 2.0);
            var xCenter = (rect.X + rect.Right) / 2.0;
            var yCenter = (rect.Y + rect.Bottom) / 2.0;
            Center = new QVector(xCenter, yCenter);
        }
        public EzBloc Clone()
        {
            var blob = new EzBloc()
            {
                Rect = Rect,
                Score = Score,
                Owner = Owner,
                SQRatio = SQRatio,
                Pixels = Pixels,
                Center = new QVector(Center)    // 為了精度 !!!
            };
            ((IxBlob)blob).Bin = ((IxBlob)this).Bin;
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
    }
}
