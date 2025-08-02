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
using JetEazy.QxCollections2;
using System.Drawing;


namespace JetEazy.Match
{
    partial class EzBloc : IxBlob
    {
        #region IxBlob_Implementation
        void IxBlob.GetRectangle(out Rectangle rect)
        {
            rect = Rect;
        }
        bool ISpot.Intersects(ref Rectangle rectCmp)
        {
            return Rect.IntersectsWith(rectCmp);
        }
        void IxBlob.SetCenter(int xCenter, int yCenter, int width, int height)
        {
            if (width > 0)
                Rect.Width = width;
            if (height > 0)
                Rect.Height = height;
            Qcvt.SetCenter(ref Rect, xCenter, yCenter);
            Center = new QVector(xCenter, yCenter);
        }
        uint IxBlob.Bin { get; set; }
        int IxBlob.Pixels => 0;
        public int CenterX => (int)Center.X;
        public int CenterY => (int)Center.Y;
        #endregion
    }
}

