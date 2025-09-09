#region AUTHOR
/*
 * 
 * Copyright (c) 2024 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2024-10-03 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.ImageViewerEx;
using JetEazy.QMath;
using JetEazy.Transform;
using System.Drawing;
using System.Windows.Forms;


namespace LaserAlignDX.Mvc.Gui
{
    public class CviTransCoordInfo : CvImageViewerInteractor
    {
        #region PRIVATE_DATA
        Control _lblDstInfo;
        Control _lblSrcInfo;
        Point _cursorPt = Point.Empty;
        #endregion

        public void Attach(Control lblDisplayInfo, Control lblSrcInfo)
        {
            _lblDstInfo = lblDisplayInfo;
            _lblSrcInfo = lblSrcInfo;
        }
        public ITransform Transform
        {
            get; set;
        }

        #region OVERRIDES
        public override bool OnMouseMove(CvImageViewer viewer, MouseEventArgs e)
        {
            if (Enabled && _lblDstInfo != null)
            {
                int x = e.X;
                int y = e.Y;

                viewer.TransCoordToWorld(ref x, ref y);

                if (_cursorPt.X != x || _cursorPt.Y != y)
                {
                    _cursorPt.X = x;
                    _cursorPt.Y = y;
                    updateCoordInfo(viewer, _cursorPt.X, _cursorPt.Y);
                }
            }
            return false;
        }
        public override bool OnMouseWheel(CvImageViewer viewer, MouseEventArgs e)
        {
            updateCoordInfo(viewer, _cursorPt.X, _cursorPt.Y);
            return base.OnMouseWheel(viewer, e);
        }
        #endregion

        #region PRIVATE_UPDATE_FUNCTIONS
        void updateCoordInfo(CvImageViewer viewer, int px, int py)
        {
            //var zoom = viewer.GetZoomScale();
            //var color = viewer.GetPixel(px, py);
            //string txt = $"Zoom={zoom:0.00}, ({px},{py}), color={color}";
            string txt = _lblSrcInfo.Text;

            if (Transform != null)
            {
                var wpt = Transform.Trans(new QVector(px, py));
                txt += " " + wpt.ToString();
            }

            _lblDstInfo.Text = txt;
            _lblDstInfo.Refresh();
        }
        #endregion
    }
}
