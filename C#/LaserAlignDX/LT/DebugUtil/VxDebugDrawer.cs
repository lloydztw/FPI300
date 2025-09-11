#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-08-18 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.Match;
using JetEazy.OpenCV.Viewer;
using JetEazy.QvMath;
using JetEazy.QxCollections;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CvSize = OpenCvSharp.Size;
using CvPoint = OpenCvSharp.Point;


namespace LeTian.AoiLib
{
    public partial class VxDebugDrawer
    {
        #region PRIVATE_DATA
        static bool _bypass = false;
        static int _canvasShrink = 4;
        static Random _rnd = new Random();
        #endregion

        public static Mat PrepareCanvas(string displayWindowName, Mat img, int shrink = 0)
        {
            if (shrink <= 0)
                shrink = _canvasShrink;
            return prepare_canvas(img, shrink, displayWindowName);
        }
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
                //var pts = box2D.Corners;
                //for (int i = 0; i < pts.Length; i++)
                //{
                //    int j = (i+1) % pts.Length;
                //    var pt1 = new CvPoint(pts[i].X / _canvasShrink, pts[i].Y / _canvasShrink);
                //    var pt2 = new CvPoint(pts[j].X / _canvasShrink, pts[j].Y / _canvasShrink);
                //    Cv2.Line(canvas, pt1, pt2, Scalar.Green, 1);
                //}
                Draw(canvas, box2D, Scalar.Green, shrink: _canvasShrink);
            }

            //if (displayWindowName != null)
            //{
            //    //Cv2.ImShow(displayWindowName, canvas);
            //    ShowWindow(displayWindowName, canvas);
            //}
            //if (canvas != img)
            //    canvas?.Dispose();

            flush(img, canvas, displayWindowName);
        }
        public static void Draw(Mat img, IEnumerable<EzBloc> blocs, bool withRect, Scalar color, string displayWindowName = null)
        {
            if (_bypass)
                return;

            Mat canvas = prepare_canvas(img, _canvasShrink, displayWindowName);

            if (blocs != null)
            {
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
            }

            if (displayWindowName != null)
            {
                //Cv2.ImShow(displayWindowName, canvas);
                ShowWindow(displayWindowName, canvas);
            }
            if (canvas != img)
                canvas?.Dispose();
        }
        public static void Draw(Mat img, IEnumerable<Point2f> centroids, CvSize size, Scalar color, string displayWindowName = null)
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
                //Cv2.ImShow(displayWindowName, canvas);
                ShowWindow(displayWindowName, canvas);
            }
            if (canvas != img)
                canvas?.Dispose();
        }

        public static void Draw(Mat img, IEnumerable<PointF[]> lines, int thickness = 1, Scalar? colorA = null, int shrink = 1, string displayWindowName = null)
        {
            if (_bypass)
                return;

            if (shrink <= 0) shrink = _canvasShrink;
            Mat canvas = prepare_canvas(img, shrink, displayWindowName);

            int ic = 0;
            foreach (var pts in lines)
            {
                var x1 = (int)(pts[0].X / shrink);
                var y1 = (int)(pts[0].Y / shrink);
                var x2 = (int)(pts[1].X / shrink);
                var y2 = (int)(pts[1].Y / shrink);
                Cv2.Line(canvas, x1, y1, x2, y2, GetColor(colorA, ic++), thickness);
            }

            flush(img, canvas, displayWindowName);
        }
        public static void Draw(Mat img, IEnumerable<QvBox2D> boxes, Scalar? colorA = null, int shrink = 1, string displayWindowName = null)
        {
            if (_bypass)
                return;

            if (shrink <= 0) shrink = _canvasShrink;
            Mat canvas = prepare_canvas(img, shrink, displayWindowName);

            foreach (var box2d in boxes)
            {
                if (box2d == null) continue;
                Draw(canvas, box2d, colorA, shrink, null);
            }

            flush(img, canvas, displayWindowName);
        }
        public static void Draw(Mat img, QvBox2D box2D, Scalar? colorA = null, int shrink = 1, string displayWindowName = null)
        {
            if (_bypass)
                return;

            if (shrink <= 0) shrink = _canvasShrink;
            Mat canvas = prepare_canvas(img, shrink, displayWindowName);

            if (box2D != null)
            {
                var color = colorA != null ? colorA.Value : Scalar.Cyan;
                var pts = box2D.Corners;
                for (int i = 0; i < pts.Length; i++)
                {
                    int j = (i + 1) % pts.Length;
                    var x1 = pts[i].X / shrink;
                    var y1 = pts[i].Y / shrink;
                    var x2 = pts[j].X / shrink;
                    var y2 = pts[j].Y / shrink;
                    Cv2.Line(canvas, (int)x1, (int)y1, (int)x2, (int)y2, color, 1);
                }
            }

            flush(img, canvas, displayWindowName);
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
        static void flush(Mat img, Mat canvas, string displayWindowName)
        {
            if (displayWindowName != null)
                ShowWindow(displayWindowName, canvas);
            if (canvas != img)
                canvas?.Dispose();
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

    partial class VxDebugDrawer
    {
        #region WIN32_API
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        #endregion

        #region PRIVATE_DATA
        static Dictionary<string, CvMatViewer> _viewersDict;
        static JetColorMap _colorMap = new JetColorMap();
        public static Scalar GetColor(Scalar? colorA, int seed)
        {
            if (colorA == null)
                //return new Scalar(_rnd.Next(50, 256), _rnd.Next(50, 256), _rnd.Next(50, 256));
                return _colorMap.GetColor(seed);
            else
                return colorA.Value;
        }
        #endregion

        public static bool OPT_USE_OPENCV_WINDOW = true;
        public static void ShowWindow(string name, Mat img)
        {
            if (OPT_USE_OPENCV_WINDOW)
                createDummyCvWindow();

            if (_viewersDict == null)
                _viewersDict = new Dictionary<string, CvMatViewer>();

            Form frm;

            if (!_viewersDict.TryGetValue(name, out var viewer))
            {
                frm = createForm(name, out viewer);
                _viewersDict.Add(name, viewer);
            }
            else
            {
                frm = viewer.FindForm();
            }

            if (frm != null)
            {
                viewer.Image = img.Clone();
                int W = Screen.PrimaryScreen.Bounds.Width * 8 / 10;
                int H = Screen.PrimaryScreen.Bounds.Height * 8 / 10;
                if (img.Width < W && img.Height < H)
                {
                    W = img.Width;
                    H = img.Height;
                }
                else
                {
                    float ratio = (float)img.Width / img.Height;
                    if (ratio > 1)
                        H = (int)(W / ratio);
                    else
                        W = (int)(H * ratio);
                }
                frm.Size = new System.Drawing.Size(W, H + 32);
                frm.Show();
            }
        }
        public static void DestroyAllWindows()
        {
            if (_viewersDict != null)
            {
                foreach (var kv in _viewersDict)
                {
                    var viewer = kv.Value;
                    cleanViewer(viewer);
                    var frm = viewer?.FindForm();
                    frm?.Dispose();
                }
                _viewersDict.Clear();
            }

            if (OPT_USE_OPENCV_WINDOW)
                Cv2.DestroyAllWindows();
        }
        public static void WaitKey()
        {
            //if (_viewersDict == null || _viewersDict.Count == 0)
            //    return;

            //Form frmFirst = null;
            //foreach (var kv in _viewersDict)
            //{
            //    frmFirst = kv.Value.FindForm();
            //    if (frmFirst != null)
            //    {
            //        frmFirst.FormClosed += (s, e) => DestroyAllWindows();
            //        break;
            //    }
            //}

            //while (_viewersDict != null && _viewersDict.Count > 0)
            //{
            //    Application.DoEvents();
            //    System.Threading.Thread.Sleep(100);
            //}

            if (OPT_USE_OPENCV_WINDOW)
                Cv2.WaitKey();
        }

        #region PRIVATE_FUNCTIONS
        static string _CV_DUMMY_WIN_NAME = "VxDebugDrawer";
        static IntPtr createDummyCvWindow()
        {
            Cv2.NamedWindow(_CV_DUMMY_WIN_NAME);
            IntPtr hWndCV = FindWindow(null, _CV_DUMMY_WIN_NAME);
            return hWndCV;
        }
        static void destroyDummyCvWindow()
        {
            try
            {
                if (OPT_USE_OPENCV_WINDOW)
                {
                    Cv2.DestroyWindow(_CV_DUMMY_WIN_NAME);
                }
                else
                {
                    DestroyAllWindows();
                }
            }
            catch
            {

            }
        }
        static Form createForm(string name, out CvMatViewer viewer)
        {
            var frm = new Form() { Text = name };
            viewer = new CvMatViewer
            {
                BackColor = System.Drawing.Color.Black,
                Dock = DockStyle.Fill,
                Visible = true
            };
            frm.Controls.Add(viewer);
            frm.FormClosed += (s, e) => destroyForm(s as Form);
            viewer.KeyPress += (s, e) => destroyDummyCvWindow();
            return frm;
        }
        static void destroyForm(Form form)
        {
            string name = form?.Text;
            if (name == null)
                return;
            if (_viewersDict != null && _viewersDict.TryGetValue(name, out var viewer))
            {
                cleanViewer(viewer);
                _viewersDict.Remove(name);
            }
            form.Dispose();
        }
        static void cleanViewer(CvMatViewer viewer)
        {
            viewer?.Image?.Dispose();
        }
        #endregion
    }

    public class JetColorMap
    {
        #region PRIVATE_DATA
        private List<Scalar> _colors;
        private readonly Random _random = new Random();
        #endregion

        /// <summary>
        /// Initializes a new instance of the ColorMap class.
        /// </summary>
        /// <param name="N">The number of discrete colors to generate.</param>
        public JetColorMap(int N = 10)
        {
            _colors = InitColors(N);
        }

        /// <summary>
        /// Generates a list of colors based on the Jet colormap algorithm.
        /// </summary>
        /// <param name="N">The number of discrete colors to generate.</param>
        /// <returns>A list of System.Drawing.Color objects.</returns>
        private List<Scalar> InitColors(int N)
        {
            var colors = new List<Scalar>(N);
            for (int i = 0; i < N; i++)
            {
                // 將 i 映射到 [0, 1] 區間
                double value = (double)i / (N - 1);

                // Jet colormap 演算法
                double r, g, b;

                if (value < 0.125)
                {
                    r = 0;
                    g = 0;
                    b = 0.5 + 4 * value;
                }
                else if (value < 0.375)
                {
                    r = 0;
                    g = 4 * (value - 0.125);
                    b = 1;
                }
                else if (value < 0.625)
                {
                    r = 4 * (value - 0.375);
                    g = 1;
                    b = 1 - 4 * (value - 0.375);
                }
                else if (value < 0.875)
                {
                    r = 1;
                    g = 1 - 4 * (value - 0.625);
                    b = 0;
                }
                else
                {
                    r = 1 - 4 * (value - 0.875);
                    g = 0;
                    b = 0;
                }

                int red = (int)(r * 255);
                int green = (int)(g * 255);
                int blue = (int)(b * 255);

                //colors.Add(Color.FromArgb(255, red, green, blue));
                colors.Add(new Scalar(red, green, blue, 255));
            }

            return colors;
        }

        /// <summary>
        /// Gets a color from the list at a specific index, with wrapping.
        /// </summary>
        /// <param name="i">The index of the color to retrieve.</param>
        /// <returns>A System.Drawing.Color object.</returns>
        public Scalar GetColor(int i)
        {
            int n = _colors.Count;
            int index = i % n;
            return _colors[index];
        }

        /// <summary>
        /// Gets a random color from the list.
        /// </summary>
        /// <returns>A random System.Drawing.Color object.</returns>
        public Scalar GetRndColor()
        {
            int n = _colors.Count;
            int index = _random.Next(0, n);
            return _colors[index];
        }

        public Scalar this[int i]
        {
            get => GetColor(i);
        }
    }
}
