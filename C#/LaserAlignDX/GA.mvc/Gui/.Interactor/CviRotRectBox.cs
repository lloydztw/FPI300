#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-08-31 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.ImageViewerEx;
using JetEazy.Match;
using JetEazy.QvMath;
using System;
using System.Drawing;

namespace LaserAlignDX.Mvc.Gui
{
    public class CviRotRectBox : CvImageViewerInteractor, IvDrawItem
    {
        #region PRIVATE_DATA
        QvQuad2D _quad2D;
        Color _crossColor;
        Color _color;
        float _blend;
        #endregion

        #region GUI_MEMBERS
        static StringFormat _strFormat = new StringFormat()
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center,
        };
        Font _font = null;
        #endregion

        protected CviRotRectBox(Color color, float blend = 0)
        {
            _crossColor = color;
            _color = color;
            _blend = blend;
        }
        public CviRotRectBox(QvBox2D box2D, Color color, float blend = 0) : this(color, blend)
        {
            _quad2D = QvQuad2D.From(box2D);
        }
        public CviRotRectBox(QvQuad2D quad2D, Color color, float blend = 0) : this(color, blend)
        {
            _quad2D = quad2D?.Clone();
        }
        public CviRotRectBox(RectangleF rectF, Color color, float blend = 0) : this(color, blend)
        {
            SetBox(ref rectF);
        }
        public CviRotRectBox(ref RectangleF rectF, Color color, float blend = 0) : this(color, blend)
        {
            SetBox(ref rectF);
        }
        public object Tag
        {
            get;
            set;
        }

        public QvQuad2D Quad2D
        {
            get => _quad2D;
        }
        public void SetBox(QvBox2D box2d)
        {
            _quad2D = QvQuad2D.From(box2d);
        }
        public void SetBox(QvQuad2D quad2d)
        {
            _quad2D = quad2d;
        }
        public void SetBox(EzBloc bloc)
        {
            if (bloc != null)
            {
                var quad = QvQuad2D.From(bloc.Rect);
                if (bloc.Center != null)
                    quad.SetCenter(bloc.Center);
                _quad2D = quad;
            }
            else
            {
                _quad2D = null;
            }
        }
        public void SetBox(RectangleF rect)
        {
            _quad2D = QvQuad2D.From(rect);
        }
        public void SetBox(ref RectangleF rect)
        {
            _quad2D = QvQuad2D.From(rect);
        }
        
        public string Text
        {
            get; set;
        }
        public int CrossLength
        {
            get; set;
        }
        public Color CrossColor
        {
            get => _crossColor;
            set => _crossColor = value;
        }

        #region OVERRIDES
        public override void OnDraw(CvImageViewer viewer, Graphics gxView)
        {
            if (_quad2D == null)
                return;

            if (_font == null)
                _font = viewer.Font;

            bool isWorld = viewer.IsInWorldCoordinate();
            if (!isWorld)
                viewer.SwitchToWorldCoordinate(gxView);

            Draw_Contents(viewer, gxView);

            if (!isWorld)
                viewer.SwitchToViewportCoordinate(gxView);
        }
        #endregion

        #region DRAW_FUNCTIONS
        void Draw_Contents(CvImageViewer viewer, Graphics gxView)
        {
            var polyPts = Array.ConvertAll(_quad2D.Corners, c => new PointF((float)c.X, (float)c.Y));

            #region FILL_BACKGROUND
            // Blending Alpha
            int alpha = _blend > 0 && _blend <= 1 ? (int)(255 * _blend) : 0;
            if (alpha > 0)
            {
                using (var brBkGnd = new SolidBrush(Color.FromArgb(alpha, _color)))
                {
                    gxView.FillPolygon(brBkGnd, polyPts);
                }
            }
            #endregion

            #region CENTER_CROSS
            if (CrossLength > 0)
            {
                var penC = viewer.GetOnePixelPen(_crossColor);
                float cx = (float)_quad2D.Center.X;
                float cy = (float)_quad2D.Center.Y;
                gxView.DrawLine(penC, cx - CrossLength, cy, cx + CrossLength, cy);
                gxView.DrawLine(penC, cx, cy - CrossLength, cx, cy + CrossLength);
            }
            #endregion

            if (true)
            {
                var pen = viewer.GetOnePixelPen(_color);
                gxView.DrawPolygon(pen, polyPts);
            }

            #region DRAW_TEXT
            var text = Text;
            if (!string.IsNullOrEmpty(text))
            {
                using (var br = new SolidBrush(_color))
                {
                    gxView.DrawString(text, _font, br, _quad2D.BoundaryRect, _strFormat);
                }
            }
            #endregion
        }
        #endregion
    }
}
