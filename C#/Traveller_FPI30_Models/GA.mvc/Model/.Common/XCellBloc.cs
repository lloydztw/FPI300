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

using JetEazy.Match;
using JetEazy.QMath;
using System.Drawing;
using XCell = LaserAlignDX.OPSpace.RegionCellX3Class;

namespace LaserAlignDX.Model
{
    public class XCellBloc : EzBloc
    {
        public XCellBloc(XCell cell, RectangleF rect) : base(Rectangle.Round(rect), 1)
        {
            Cell = cell;
        }
        public XCellBloc(XCell cell) : base(Rectangle.Empty, 1)
        {
            Cell = cell;

            //var mvdRectF = cell.DrawResultRectF();
            //Rect = Rectangle.Round(GaImageUtil.ToRectangleF(mvdRectF));
            //Center = new JetEazy.QMath.QVector(mvdRectF.CenterX, mvdRectF.CenterY);

            var chipQuad2D = cell?.ChipData?.ChipQuad2D;
            if (chipQuad2D != null)
            {
                Rect = Rectangle.Round(cell.viewRectF);
                Center = new QVector(chipQuad2D.Center);
            }
            else
            {
                var cc = JetEazy.Qcvt.CenterF(ref cell.viewRectF);
                Rect = Rectangle.Round(cell.viewRectF);
                Center = new QVector(cc.X, cc.Y);
            }
        }
        public XCell Cell
        {
            get; private set;
        }
        public bool IsEmpty
        {
            //get => string.IsNullOrEmpty(NonEmptyDesc);
            get => Cell == null || Cell.IsEmptyPlaceHold();
        }
    };
}
