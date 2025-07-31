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


namespace JetEazy.ImageViewerEx.Interactors
{

    public class CviCenterCross : CvImageViewerInteractor
    {
        #region PRIVATE_DATE_MEMBERS
        private Color m_color;
        #endregion

        public CviCenterCross(Color color)
        {
            m_color = color;
        }
        public override void OnDraw(CvImageViewer viewer, Graphics gx)
        {

            bool isWorld = viewer.IsInWorldCoordinate();
            if (isWorld)
                viewer.SwitchToViewportCoordinate(gx);

            Rectangle rect = Rectangle.Round(viewer.GetViewportRect());
            var pen = viewer.GetOnePixelPen(m_color);
            int cx = rect.X + rect.Width / 2;
            var cy = rect.Y + rect.Height / 2;
            gx.DrawLine(pen, cx, rect.Top, cx, rect.Bottom);
            gx.DrawLine(pen, rect.Left, cy, rect.Right, cy);

            // 這裡可以用
            //  gx.DrawString
            // 劃出你要的字串

            if (isWorld)
                viewer.SwitchToWorldCoordinate(gx);
        }
        //public override void OnDraw(CvImageViewer viewer, Graphics gx)
        //{

        //    bool isWorld = viewer.IsInWorldCoordinate();
        //    if (isWorld)
        //        viewer.SwitchToViewportCoordinate(gx);

        //    Rectangle rect = Rectangle.Round(viewer.GetViewportRect());
        //    var pen = viewer.GetOnePixelPen(m_color);
        //    int cx = rect.X + rect.Width / 2;
        //    var cy = rect.Y + rect.Height / 2;
        //    gx.DrawLine(pen, cx, rect.Top, cx, rect.Bottom);
        //    gx.DrawLine(pen, rect.Left, cy, rect.Right, cy);
        //    pen.Dispose();

        //    if (isWorld)
        //        viewer.SwitchToWorldCoordinate(gx);
        //}
    }
}
