#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-07-11 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System.Drawing;

namespace JetEazy.ImageViewerEx.Interactors
{

    public class CviLabel : CvImageViewerInteractor
    {
        #region PRIVATE_DATE_MEMBERS
        private string _text;
        private Point? _location = null;
        private Font _font;
        private Brush textBrush;
        #endregion

        public string Text
        {
            get => _text; 
            set => _text = value;
        }
        public Brush TextBrush
        {
            get => textBrush; 
            set => textBrush = value;
        }
        public Point Location
        {
            get => _location!=null ? _location.Value : Point.Empty; 
            set => _location = value;
        }
        public Font Font
        {
            get => _font; set => _font = value;
        }

        #region OVERRIDES
        public override void OnDraw(CvImageViewer viewer, Graphics gx)
        {
            if (!Visible)
                return;

            bool isWorld = viewer.IsInWorldCoordinate();
            if (isWorld)
                viewer.SwitchToViewportCoordinate(gx);


            var font = get_font(viewer);
            (int x, int y) = get_location(viewer);
            viewer.TransCoordToView(ref x, ref y);

            gx.DrawString(_text, font, textBrush, x, y);

            if (isWorld)
                viewer.SwitchToWorldCoordinate(gx);
        }
        #endregion

        #region PRIVATE_FUNCTIONS
        (int,int) get_location(CvImageViewer viewer)
        {
            if (_location == null)
            {
                var rect = viewer.GetWorldRect();
                var x = (int)(rect.Left + rect.Width / 2);
                var y = (int)(rect.Top + rect.Height / 2);
                _location = new Point(x, y);
            }
            return (_location.Value.X, _location.Value.Y);
        }
        Font get_font(CvImageViewer viewer)
        {
            if (_font == null)
            {
                var fnSize = System.Math.Max(viewer.GetWorldRect().Height / 20f, 16f);
                _font = new Font(viewer.Font.FontFamily, fnSize);
            }
            return _font;
        }
        #endregion
    }
}
