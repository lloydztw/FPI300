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

        public EzBloc(Rectangle rect, double score, object owner = null, object tag = null)
        {
            Rect = rect;
            Score = score;
            Owner = owner;
            Tag = tag;
            var xCenter = Math.Round((rect.X + rect.Right) / 2.0);
            var yCenter = Math.Round((rect.Y + rect.Bottom) / 2.0);
            Center = new QVector(xCenter, yCenter);
        }
        public object Clone()
        {
            return new EzBloc(Rect, Score, Owner, null);
        }
        public bool IsMajorNode()
        {
            return Tag is QuadLinkNode;
        }
    }
}
