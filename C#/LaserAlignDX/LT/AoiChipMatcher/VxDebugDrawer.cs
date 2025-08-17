using JetEazy.Match;
using JetEazy.QvMath;
using JetEazy.QxCollections;
using OpenCvSharp;
using System.Collections.Generic;

namespace LeTian.Match
{
    public static class VxDebugDrawer
    {
        static bool _bypass = false;
        static int _canvasShrink = 4;

        public static void Draw(Mat img, QvBox2D box2D, IxGridMap<EzBloc> grid, int keyRow, int keyCol, Scalar color, string displayWindowName = null)
        {
            if (_bypass)
                return;

            Mat canvas = prepare_canvas(img, _canvasShrink, displayWindowName);
            Draw(canvas, iter_major_blocs(grid), false, color);

            if (keyRow >= 0 && keyCol >= 0)
            {
                var keyPad = grid.Get(keyRow, keyCol);
                if (keyPad != null)
                {
                    Draw(canvas, new EzBloc[] { keyPad }, false, Scalar.Red);
                }
            }

            var lineColor = Scalar.DarkBlue;
            void draw_line(EzBloc from, EzBloc to)
            {
                if (from != null && to != null)
                {
                    var lx = (float)from.Center.X / _canvasShrink;
                    var ly = (float)from.Center.Y / _canvasShrink;
                    var cx = (float)to.Center.X / _canvasShrink;
                    var cy = (float)to.Center.Y / _canvasShrink;
                    canvas.Line((int)lx, (int)ly, (int)cx, (int)cy, lineColor, 1);
                }
            }
            int rows = grid.Rows;
            int cols = grid.Cols;
            for (int r = 0; r < rows; r++)
            {
                EzBloc last = null;
                for (int c = 0; c < cols; c++)
                {
                    var bloc = grid.Get(r, c);
                    if (bloc == null)
                        continue;

                    draw_line(last, bloc);
                    last = bloc;
                }
            }
            for (int c = 0; c < cols; c++)
            {
                EzBloc last = null;
                for (int r = 0; r < rows; r++)
                {
                    var bloc = grid.Get(r, c);
                    if (bloc == null)
                        continue;

                    draw_line(last, bloc);
                    last = bloc;
                }
            }

            if (box2D != null)
            {
                var pts = box2D.Corners;
                for (int i = 0; i < pts.Length; i++)
                {
                    int j = (i+1) % pts.Length;
                    var pt1 = new Point(pts[i].X / _canvasShrink, pts[i].Y / _canvasShrink);
                    var pt2 = new Point(pts[j].X / _canvasShrink, pts[j].Y / _canvasShrink);
                    Cv2.Line(canvas, pt1, pt2, Scalar.Green, 1);
                }
            }

            if (displayWindowName != null)
            {
                Cv2.ImShow(displayWindowName, canvas);
            }
            if (canvas != img)
                canvas?.Dispose();
        }
        public static void Draw(Mat img, IEnumerable<EzBloc> blocs, bool withRect, Scalar color, string displayWindowName = null)
        {
            if (_bypass)
                return;

            Mat canvas = prepare_canvas(img, _canvasShrink, displayWindowName);

            foreach (var bloc in blocs)
            {
                if (bloc == null)
                    continue;

                var rect = JetEazy.Qcvt.CV(bloc.Rect);
                rect.X /= _canvasShrink;
                rect.Y /= _canvasShrink;
                rect.Width /= _canvasShrink;
                rect.Height /= _canvasShrink;

                if (withRect)
                    canvas.Rectangle(rect, color);

                var cc = JetEazy.Qcvt.Center(ref rect);
                var r = rect.Width / 4;
                canvas.Circle(cc, r, color, -1);
            }

            if (displayWindowName != null)
            {
                Cv2.ImShow(displayWindowName, canvas);
            }
            if (canvas != img)
                canvas?.Dispose();
        }
        public static void Draw(Mat img, IEnumerable<Point2f> centroids, Size size, Scalar color, string displayWindowName = null)
        {
            Mat canvas = prepare_canvas(img, _canvasShrink, displayWindowName);

            var ssize = size;
            ssize.Width /= _canvasShrink;
            ssize.Height /= _canvasShrink;

            foreach (var c in centroids)
            {
                var x = c.X / _canvasShrink;
                var y = c.Y / _canvasShrink;
                var rect = JetEazy.Qcvt.CvCreateCenterRect((int)x, (int)y, ref ssize);

                canvas.Rectangle(rect, color);
                var cc = JetEazy.Qcvt.Center(ref rect);
                var r = rect.Width / 4;
                canvas.Circle(cc, r, color, -1);
            }

            if (displayWindowName != null)
            {
                Cv2.ImShow(displayWindowName, canvas);
            }
            if (canvas != img)
                canvas?.Dispose();
        }

        #region PRIVATE_DATA
        static Mat prepare_canvas(Mat img, int shrink, string displayWindowName)
        {
            bool inplace = displayWindowName == null && img.Type() == MatType.CV_8UC3;
            Mat canvas;
            if (inplace)
            {
                canvas = img;
            }
            else
            {
                var imgS = new Mat();
                canvas = new Mat();
                var ssize = img.Size();
                ssize.Width /= shrink;
                ssize.Height /= shrink;
                Cv2.Resize(img, imgS, ssize);
                Cv2.CvtColor(imgS, canvas, ColorConversionCodes.GRAY2BGR);
                imgS?.Dispose();
            }
            return canvas;
        }
        static IEnumerable<EzBloc> iter_major_blocs(IxGridMap<EzBloc> grid)
        {
            if (grid != null)
            {
                //foreach (var bloc in grid)
                //    if (bloc != null && bloc.IsMajorNode())
                //        yield return bloc;

                for (int r = 0; r < grid.Rows; r++)
                {
                    for (int c = 0; c < grid.Cols; c++)
                    {
                        var bloc = grid.Get(r, c);
                        if (bloc != null && bloc.IsMajorNode())
                            yield return bloc;
                    }
                }
            }
        }
        #endregion
    }
}
