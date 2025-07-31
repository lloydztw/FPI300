/****************************************************************************
 *                                                                          
 * Copyright (c) 2009 Jet Eazy Corp. All rights reserved.        
 *                                                                          
 ***************************************************************************/

/****************************************************************************
 *
 * VERSION
 *		$Revision:$
 *
 * HISTORY
 *      $Id:$    
 *	        20080701 LeTian Chang : Creation         
 *
 * DESCRIPTION
 *      
 *
 ***************************************************************************/

using System.Drawing;
using System.Windows.Forms;


namespace JetEazy.ImageViewerEx.Interactors
{

    public class CviCross : CvImageViewerInteractor
    {
        #region PRIVATE_DATA
        private Color _color;
        private Point _crossPoint = Point.Empty;
        #endregion

        public CviCross(Color color, int reserved = 0)
        {
            _color = color;
        }

        /// <summary>
        /// 是否固定於相機中心    
        /// </summary>
        public bool IsFixedAtCenter
        {
            get;
            set;
        } = true;

        public override void OnDraw(CvImageViewer viewer, Graphics gx)
        {
            bool isWorld = viewer.IsInWorldCoordinate();

            if (isWorld)
                viewer.SwitchToViewportCoordinate(gx);

            var rectV = viewer.GetViewportRect();

            if (IsFixedAtCenter)
            {
                var rectW = viewer.GetWorldRect();
                viewer.TransCoordToView(ref rectW);
                _crossPoint.X = (int)(rectW.Left + rectW.Width / 2);
                _crossPoint.Y = (int)(rectW.Top + rectW.Height / 2);
            }

            var pen = new Pen(_color, 1f);
            gx.DrawLine(pen, _crossPoint.X, rectV.Top, _crossPoint.X, rectV.Bottom);
            gx.DrawLine(pen, rectV.Left, _crossPoint.Y, rectV.Right, _crossPoint.Y);
            pen.Dispose();

            if (isWorld)
                viewer.SwitchToWorldCoordinate(gx);
        }

        public override bool OnMouseMove(CvImageViewer viewer, MouseEventArgs e)
        {
            if (IsFixedAtCenter)
                return false;

            int xCross = e.Location.X;
            int yCross = e.Location.Y;

            // 利用 TransViewportToWorld
            // 與   TransWorldToViewport
            // 讓   (xCross, yCross)  對齊於格點.       I.
            viewer.TransViewportToWorld(ref xCross, ref yCross);
            viewer.TransWorldToViewport(ref xCross, ref yCross);

            if (xCross != _crossPoint.X || yCross != _crossPoint.Y)
            {
                _crossPoint.X = xCross;
                _crossPoint.Y = yCross;
                return true;
            }

            return false;
        }
    }
}
