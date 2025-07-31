using System;
using System .Collections .Generic;
using System .ComponentModel;
using System .Drawing;
using System.Drawing.Imaging;
using System .Drawing .Drawing2D;
using System .Windows .Forms;
using Microsoft .Win32;

using MoveGraphLibrary;
using JzDisplay;

namespace WorldOfMoveableObjects
{
    public class JzCircleHoleEAG : GeoFigure
    {
        class CircleHoleEAGClass
        {
            public PointF Center;
            public double RInner;
            public double ROutter;
            public int nVertices;

            public double Degree;

            string BackupString = "";

            public CircleHoleEAGClass(Figure_EAG figure)
            {
                Center = new PointF(200, 200);
                RInner = 50;
                ROutter = 100;
                
                switch(figure)
                {
                    case Figure_EAG.HexO:
                        nVertices = 6;
                        Degree = 0;
                        break;
                    case Figure_EAG.RectO:
                        nVertices = 4;
                        Degree = 45;
                        break;
                }
            }
            public CircleHoleEAGClass(PointF center, double rinner, double routter, int nvertices, double angle)
            {
                Center = center;
                RInner = rinner;
                ROutter = routter;
                nVertices = nvertices;
                Degree = angle;
            }
            public void Backup()
            {
                BackupString = ToString();
            }
            public void Restore()
            {
                FromString(BackupString);
            }
            public void SetOffset(Point offset)
            {
                Center.X += offset.X;
                Center.Y += offset.Y;
            }
            public void SetAngle(double adddegree)
            {
                Degree += adddegree;
            }
            public void FromRectF(RectangleF rectf)
            {
                Center = new PointF(rectf.X + rectf.Width / 2, rectf.Y + rectf.Height / 2);
                RInner = Math.Max(50, Math.Max(rectf.Width/2, rectf.Height/2) * 2 / 3);
                ROutter = Math.Max(100,Math.Max(rectf.Width/2,rectf.Height/2));
            }
            public override string ToString()
            {
                string str = "";

                str += PointFToString(Center) + "X";    //0
                str += RInner.ToString() + "X";         //1
                str += ROutter.ToString() + "X";        //2
                str += nVertices.ToString() + "X";
                str += Degree.ToString();

                return str;
            }
            public void FromString(string str)
            {
                string[] strs = str.Split('X');

                Center = StringToPointF(strs[0]);
                RInner = float.Parse(strs[1]);
                ROutter = float.Parse(strs[2]);
                nVertices = int.Parse(strs[3]);
                Degree = double.Parse(strs[4]);
            }
            string PointFToString(PointF ptf)
            {
                return ptf.X.ToString() + "," + ptf.Y.ToString();
            }
            PointF StringToPointF(string str)
            {
                string[] strs = str.Split(',');

                PointF retptf = new PointF();

                retptf.X = float.Parse(strs[0]);
                retptf.Y = float.Parse(strs[1]);

                return retptf;
            }

        }

        int m_version = 701;
        //Form form;
        Control picImage;
        Mover supervisor;

        double radiusVertices, radiusCircle;
        int nVertices;
        SolidBrush brush;

        PointF ptEndIn, ptEndOut;
        double scaling, compensation;

        double m_delta = 2;        // minimum width of the strip on border is 2 * delta
        double deltaR;
        static int minCircleRadius = 10;
        static int minCircleToSide = 10;

        CircleHoleEAGClass CircleHoleEAG = new CircleHoleEAGClass(Figure_EAG.RectO);

        // -------------------------------------------------
        public JzCircleHoleEAG (Control pic, Mover mvr, PointF ptC, 
                                         double radiusIn, double radiusOut, int vertices, double angleDegree, Color color, bool in_group, Figure_EAG figure)
        {
            picImage = pic;
            supervisor = mvr;
            m_figure = figure;

            nVertices = Math .Max (Math .Abs (vertices), 3);
            m_angle = Auxi_Convert .DegreeToRadian (Auxi_Common .LimitedDegree (angleDegree));

            m_center = ptC;

            CircleHoleEAG = new CircleHoleEAGClass(m_center, radiusIn, radiusOut, vertices, angleDegree);

            double rSmaller = Math .Min (Math .Abs (radiusOut), Math .Abs (radiusIn));
            double rBigger = Math .Max (Math .Abs (radiusOut), Math .Abs (radiusIn));
            radiusCircle = Math .Max (rSmaller, minCircleRadius);
            radiusVertices = Math .Max (radiusOut, (radiusCircle + minCircleToSide) / Math .Cos (Math .PI / nVertices));
            brush = new SolidBrush (color);
            bInGroup = in_group;
            deltaR = m_delta / Math .Cos (Math .PI / nVertices);
        }
        public JzCircleHoleEAG(Control pic, Mover mvr, string str, Color color, Figure_EAG figure)    //Added By Victor Tsai
        {
            picImage = pic;
            supervisor = mvr;

            m_figure = figure;

            FromString(str);

            brush = new SolidBrush(color);
            bInGroup = false;
        }
        public JzCircleHoleEAG(string str, Color color)    //Added By Victor Tsai
        {
            FromString(str);

            nVertices = CircleHoleEAG.nVertices;
            radiusCircle = CircleHoleEAG.RInner;
            radiusVertices = CircleHoleEAG.ROutter;
            m_angle = Auxi_Convert.DegreeToRadian(Auxi_Common.LimitedDegree(CircleHoleEAG.Degree));

            brush = new SolidBrush(color);
            bInGroup = false;

            deltaR = m_delta / Math.Cos(Math.PI / nVertices);
        }
        public override void InitialCtrl(Control pic, Mover mvr)  //Added By Victor Tsai
        {
            picImage = pic;
            supervisor = mvr;
        }
        /// <summary>
        /// ����
        /// </summary>
        /// <param name="color"></param>
        public JzCircleHoleEAG(Color color, Figure_EAG figure)                       //Added By Victor Tsai
        {
            CircleHoleEAG = new CircleHoleEAGClass(figure);

            FromString(figure.ToString() + ";" + CircleHoleEAG.ToString());
            
            brush = new SolidBrush(color);
            bInGroup = false;
        }
        public JzCircleHoleEAG(Color color, Figure_EAG figure,RectangleF rectf)                       //Added By Victor Tsai
        {
            CircleHoleEAG = new CircleHoleEAGClass(figure);
            CircleHoleEAG.FromRectF(rectf);

            FromString(figure.ToString() + ";" + CircleHoleEAG.ToString());

            nVertices = CircleHoleEAG.nVertices;
            radiusCircle = CircleHoleEAG.RInner;
            radiusVertices = CircleHoleEAG.ROutter;
            m_angle = Auxi_Convert.DegreeToRadian(Auxi_Common.LimitedDegree(CircleHoleEAG.Degree));

            brush = new SolidBrush(color);
            bInGroup = false;

            deltaR = m_delta / Math.Cos(Math.PI / nVertices);
        }
        public JzCircleHoleEAG(Control pic, Mover mvr, JzCircleHoleEAG frompolyo)    //Added By Victor Tsai
        {
            picImage = pic;
            supervisor = mvr;

            m_figure = frompolyo.m_figure;

            FromString(frompolyo.ToString());

            CircleHoleEAG.Center.X += 100f;
            CircleHoleEAG.Center.Y += 100f;

            nVertices = frompolyo.nVertices;
            radiusCircle = frompolyo.radiusCircle;
            radiusVertices = frompolyo.radiusVertices;
            m_angle = frompolyo.m_angle;

            brush = frompolyo.brush;

            bInGroup = false;

            deltaR = m_delta / Math.Cos(Math.PI / nVertices);

            IsFirstSelected = frompolyo.IsFirstSelected;
            IsSelected = frompolyo.IsSelected;

            frompolyo.IsFirstSelected = false;
            frompolyo.IsSelected = false;
        }
        // -------------------------------------------------        Copy
        public override GeoFigure Copy (Control pic, Mover mvr, PointF pt)
        {
            JzCircleHoleEAG elem = new JzCircleHoleEAG (pic, mvr, pt, radiusCircle, radiusVertices, nVertices, 
                                                                          Auxi_Convert .RadianToDegree (m_angle), Color, InGroup, Figure_EAG.Any);
            elem .Resizable = bResize;
            elem .Rotatable = bRotate;
            elem .Visible = Visible;
            elem .VisibleAsMember = VisibleAsMember;
            return ((GeoFigure) elem);
        }
        // -------------------------------------------------        ZoomAllowed
        public override bool ZoomAllowed (double coef)
        {
            coef = Math .Abs (coef);
            bool bRet = minCircleRadius <= coef * radiusCircle &&
                        minCircleToSide <= coef * (radiusVertices * Math .Cos (Math .PI / nVertices) - radiusCircle);
            return (bRet);
        }
        // -------------------------------------------------        Zoom
        public override bool Zoom (double coef, bool bCheck)
        {
            coef = Math .Abs (coef);
            if (bCheck)
            {
                if (minCircleRadius > coef * radiusCircle ||
                    minCircleToSide > coef * (radiusVertices * Math .Cos (Math .PI / nVertices) - radiusCircle))
                {
                    return (false);
                }
            }
            radiusCircle *= coef;
            radiusVertices *= coef;
            DefineCover ();
            return (true);
        }
        // -------------------------------------------------        SqueezeToMinimal
        public override void SqueezeToMinimal ()
        {
            double rInscribed = radiusVertices * Math .Cos (Math .PI / nVertices);
            double fCoef_forInner = minCircleRadius / radiusCircle;
            double fCoef_forWidth = minCircleToSide / (rInscribed - radiusCircle);
            double fCoef = Math .Max (fCoef_forInner, fCoef_forWidth);
            Zoom (fCoef, false);
        }
        // -------------------------------------------------        SqueezePossible
        public override bool SqueezePossible
        {
            get
            {
                double rInscribed = radiusVertices * Math .Cos (Math .PI / nVertices);
                return (minCircleRadius < radiusCircle &&
                        minCircleToSide < rInscribed - radiusCircle);
            }
        }
        // -------------------------------------------------        RadiusVertices
        public double RadiusVertices
        {
            get { return (radiusVertices); }
        }
        // -------------------------------------------------        RadiusCircle
        public double RadiusCircle
        {
            get { return (radiusCircle); }
        }
        // -------------------------------------------------        VerticesNumber
        public int VerticesNumber
        {
            get { return (nVertices); }
        }
        // -------------------------------------------------        Vertices
        public PointF [] Vertices
        {
            get { return (Auxi_Geometry .RegularPolygon (m_center, radiusVertices, nVertices, m_angle)); }
        }
        // -------------------------------------------------        MinimumInnerRadius
        static public double MinimumInnerRadius
        {
            get { return (minCircleRadius); }
        }
        // -------------------------------------------------        MinimumWidth
        static public double MinimumWidth
        {
            get { return (minCircleToSide); }
        }
        // -------------------------------------------------        RectAround
        public override RectangleF RectAround
        {
            get { return (Auxi_Geometry .RectangleAroundPoints (Vertices)); }
        }
        // -------------------------------------------------        Angle
        public override double Angle
        {
            get { return (m_angle); }
            set
            {
                m_angle = Auxi_Common .LimitedRadian (value);
                DefineCover ();
            }
        }
        // -------------------------------------------------        Color
        public override Color Color
        {
            get { return (brush .Color); }
            set { brush .Color = value; }
        }
        // -------------------------------------------------
        //public override void Draw (Graphics grfx)
        //{
        //    if (Visible && VisibleAsMember)
        //    {
        //        GraphicsPath path = new GraphicsPath ();
        //        PointF [] pts = Vertices;
        //        float f_rad = Convert .ToSingle (radiusCircle);
        //        RectangleF rcIn = new RectangleF (m_center .X - f_rad, m_center .Y - f_rad, 2 * f_rad, 2 * f_rad);
        //        path .AddPolygon (Vertices);
        //        path .AddEllipse (rcIn);

        //        grfx .FillPath (brush, path);

        //        if (bResize)
        //        {
        //            if (InGroup)
        //            {
        //                grfx .DrawPolygon (penResize_InGroup, pts);
        //                grfx .DrawEllipse (penResize_InGroup, rcIn);
        //            }
        //            else
        //            {
        //                penResize_Independent.Width = (int)m_delta;
        //                penResize_Independent.Color = Color.Red;

        //                grfx .DrawPolygon (penResize_Independent, pts);
        //                grfx .DrawEllipse (penResize_Independent, rcIn);

        //            }
        //        }

        //        foreach (PointF ptf in pts)
        //        {
        //            grfx.FillEllipse(Brushes.Red, ptf.X - (float)m_delta - 1, ptf.Y - (float)m_delta, (float)m_delta * 2 + 2, (float)m_delta * 2 + 2);
        //        }
        //    }
        //}
        // -------------------------------------------------        StartResizing
        public void StartResizing (Point ptMouse, int iNode)
        {
            double angleBeam;  
            PointF ptNearest = new PointF (0, 0);
            if (iNode == 1)       // inner border (circle)
            {
                angleBeam = Auxi_Geometry .Line_Angle (Center, ptMouse);
                ptNearest = Auxi_Geometry .PointToPoint (Center, angleBeam, RadiusCircle);
                ptEndIn = Auxi_Geometry .PointToPoint (Center, angleBeam, minCircleRadius);
                ptEndOut = Auxi_Geometry .PointToPoint (Center, angleBeam, radiusVertices * Math .Cos (Math .PI / nVertices) - minCircleToSide);

            }
            else if (iNode == 4)    // outer border (regular polygon)
            {
                Auxi_Geometry .Distance_PointPolyline (ptMouse, Vertices, true, out ptNearest);
                angleBeam = Auxi_Geometry .Line_Angle (Center, ptNearest);
                scaling = radiusVertices / Auxi_Geometry .Distance (Center, ptNearest);
                ptEndIn = Auxi_Geometry .PointToPoint (Center, angleBeam,
                                                       ((RadiusCircle + minCircleToSide) / Math .Cos (Math .PI / nVertices)) / scaling);
                ptEndOut = Auxi_Geometry .PointToPoint (Center, angleBeam, 4000);
            }
            AdjustCursorPosition (ptNearest);
        }
        // -------------------------------------------------        StartRotation
        public override void StartRotation (Point ptMouse)
        {
            double angleMouse = Auxi_Geometry .Line_Angle (m_center, ptMouse);
            compensation = Auxi_Common .LimitedRadian (angleMouse - m_angle);
        }
        // -------------------------------------------------        AdjustCursorPosition
        private void AdjustCursorPosition (PointF pt)
        {
            supervisor .MouseTraced = false;
            Cursor .Position = picImage .PointToScreen (Point .Round (pt));
            supervisor .MouseTraced = true;
        }
        // -------------------------------------------------        DefineCover
        public override void DefineCover ()
        {
            PointF[] ptsOut = Vertices;
            CoverNode [] nodes = new CoverNode [5];

            if (bResize)
            {
                nodes[0] = new CoverNode(0, Center, Convert.ToSingle(radiusCircle - m_delta), Behaviour.Transparent);
                nodes[1] = new CoverNode(1, Center, Convert.ToSingle(radiusCircle + m_delta), Cursors.Hand);
                nodes[2] = new CoverNode(2, Auxi_Geometry.RegularPolygon(m_center, radiusVertices - deltaR, nVertices, m_angle));
                nodes[2].SetBehaviourCursor(Behaviour.Transparent, Cursors.Default);

                //nodes[3] = new CoverNode(3, Auxi_Geometry.RegularPolygon(m_center, radiusVertices + deltaR, nVertices, m_angle), Cursors.Hand);
                nodes[3] = new CoverNode(3, ptsOut[0], ptsOut[1], (float)m_delta * 2, Cursors.SizeAll);
                nodes[4] = new CoverNode(4, Auxi_Geometry.RegularPolygon(m_center, radiusVertices + deltaR, nVertices, m_angle), Cursors.Hand);
            }
            //else
            //{
            //    nodes[0] = new CoverNode(0, Center, Convert.ToSingle(radiusCircle), Behaviour.Transparent);
            //    nodes[1] = new CoverNode(1, Center, 0.0, Cursors.SizeAll);
            //    nodes[2] = new CoverNode(2, Vertices);
            //    nodes[3] = new CoverNode(3, Center, 0.0, Cursors.SizeAll);
            //}

            cover = new Cover (nodes);

            if (TransparentForMover)
                cover.SetBehaviour(Behaviour.Transparent);
        }
        // -------------------------------------------------
        public override void Move (int dx, int dy)
        {
            m_center += new Size (dx, dy);
        }
        public override void Move(float dx, float dy)
        {
            m_center += new SizeF(dx, dy);
        }
        // -------------------------------------------------        MoveNode
        public override bool MoveNode (int iNode, int dx, int dy, Point ptM, MouseButtons catcher)
        {
            bool bRet = false;

            if (catcher == MouseButtons .Left)
            {
                if (iNode == 3)
                {
                    Move(dx, dy);
                }
                else if (iNode == 1 || iNode == 4)
                {
                    PointF ptNearest = Auxi_Geometry .NearestPointOnSegment (ptM, ptEndIn, ptEndOut);
                    AdjustCursorPosition (ptNearest);
                    double dist = Auxi_Geometry .Distance (Center, ptNearest);
                    if (iNode == 1)
                    {
                        radiusCircle = dist;
                    }
                    else
                    {
                        radiusVertices = dist * scaling;
                    }
                }

                return (true);
            }
            else if (catcher == MouseButtons .Right && bRotate)
            {
                double angleMouse = Auxi_Geometry .Line_Angle (m_center, ptM);
                m_angle = angleMouse - compensation;
                bRet = true;
            }
            return (bRet);
        }

        const string nameMain = "RegPoly_CircularHole_EAG_";
        // -------------------------------------------------        IntoRegistry
        public override void IntoRegistry (RegistryKey regkey, string strAdd)
        {
            try
            {
                regkey .SetValue (nameMain + strAdd, new string [] {m_version .ToString (),   // 0
                                                                    m_center .X .ToString (),    // 1
                                                                    m_center .Y .ToString (),    // 2
                                                                    radiusCircle .ToString (),      // 3
                                                                    radiusVertices .ToString (),        // 4
                                                                    nVertices .ToString (),             // 5  
                                                                    Auxi_Convert .RadianToDegree (m_angle) .ToString (),       // 6
                                                                    ((int) (Color .A)) .ToString (),        // 7
                                                                    ((int) (Color .R)) .ToString (),        // 8
                                                                    ((int) (Color .G)) .ToString (),        // 9
                                                                    ((int) (Color .B)) .ToString (),        // 10
                                                                    bInGroup .ToString (),          // 11
                                                                    bResize .ToString (),           // 12
                                                                    bRotate .ToString (),           // 13
                                                                    Visible .ToString (),               // 14
                                                                    VisibleAsMember .ToString () },     // 15
                                                     RegistryValueKind .MultiString);
            }
            catch
            {
            }
            finally
            {
            }
        }
        // -------------------------------------------------        FromRegistry
        public static JzCircleHoleEAG FromRegistry (Control pic, Mover mvr, RegistryKey regkey, string strAdd)
        {
            try
            {
                string [] strs = (string []) regkey .GetValue (nameMain + strAdd);
                if (strs == null || strs .Length < 16 || Convert .ToInt32 (strs [0]) != 701)
                {
                    return (null);
                }
                JzCircleHoleEAG poly = new JzCircleHoleEAG (pic, mvr, 
                                                                              Auxi_Convert .ToPointF (strs, 1),     // center
                                                                              Convert .ToDouble (strs [3]),         // radiusCircle
                                                                              Convert .ToDouble (strs [4]),         // radiusOut
                                                                              Convert .ToInt32 (strs [5]),          // nVertices
                                                                              Convert .ToDouble (strs [6]),         // angleDegree
                                                                              Auxi_Convert .ToColor (strs, 7),      // color
                                                                              Convert .ToBoolean (strs [11]),
                                                                              Figure_EAG.Any);      // in_group
                if (poly != null)
                {
                    poly .Resizable = Convert .ToBoolean (strs [12]);
                    poly .Rotatable = Convert .ToBoolean (strs [13]);
                    poly .Visible = Convert .ToBoolean (strs [14]);
                    poly .VisibleAsMember = Convert .ToBoolean (strs [15]);
                }
                return (poly);
            }
            catch
            {
                return (null);
            }
            finally
            {
            }
        }
        public override void SetOffset(Point offsetpoint)
        {
            CircleHoleEAG.SetOffset(offsetpoint);
        }
        public override void SetAngle(double addangle)
        {
            CircleHoleEAG.SetAngle(addangle);
        }
        public override void FromRectangleF(RectangleF rectf)
        {
            CircleHoleEAG.FromRectF(rectf);
        }
        public override string ToString()
        {
            string retstr = "";

            retstr += m_figure.ToString() + ";";
            retstr += CircleHoleEAG.ToString();

            return retstr;

        }
        public override void FromString(string str)
        {
            string[] strs = str.Split(';');

            m_figure = (Figure_EAG)Enum.Parse(typeof(Figure_EAG), strs[0]);
            CircleHoleEAG.FromString(strs[1]);
        }
        public override string ToShapeString()
        {
            string retstr = "";

            retstr += PointFToString(m_center) + ";";
            retstr += radiusCircle.ToString() + ";";
            retstr += radiusVertices.ToString() + ";";
            retstr += nVertices.ToString() + ";";
            retstr += m_angle.ToString();

            return retstr;
        }
        public override void FromShapeString(string str)
        {
            int i = 0;

            string[] strs = str.Split(';');

            m_center = StringToPointF(strs[0]);
            radiusCircle = double.Parse(strs[1]);
            radiusVertices = double.Parse(strs[2]);
            nVertices = int.Parse(strs[3]);
            m_angle = double.Parse(strs[4]);
        }
        public override void Backup()
        {
            CircleHoleEAG.Backup();
        }
        public override void Restore()
        {
            CircleHoleEAG.Restore();
        }
        public override void MappingToMovingObject(PointF bias, double ratio)
        {
            m_center.X = (float)((double)bias.X + ((double)CircleHoleEAG.Center.X * ratio));
            m_center.Y = (float)((double)bias.Y + ((double)CircleHoleEAG.Center.Y * ratio));
            
            radiusCircle = CircleHoleEAG.RInner * ratio;
            radiusVertices = CircleHoleEAG.ROutter * ratio;

            Move(OffsetPoint.X, OffsetPoint.Y);
            OffsetPoint = new Point(0, 0);
        }
        public override void MappingToMovingObject(PointF bias, SizeF sizeratio)
        {
            double ratio = Math.Min(sizeratio.Width, sizeratio.Height);

            m_center.X = (float)((double)bias.X + ((double)CircleHoleEAG.Center.X * sizeratio.Width));
            m_center.Y = (float)((double)bias.Y + ((double)CircleHoleEAG.Center.Y * sizeratio.Height));

            radiusCircle = CircleHoleEAG.RInner * ratio;
            radiusVertices = CircleHoleEAG.ROutter * ratio;

            Move(OffsetPoint.X, OffsetPoint.Y);
            OffsetPoint = new Point(0, 0);

            MappingFromMovingObject(new PointF(0, 0), 1);
        }
        public override void MappingFromMovingObject(PointF bias, double ratio)
        {
            CircleHoleEAG.Center.X = (float)(((double)m_center.X - (double)bias.X) / ratio);
            CircleHoleEAG.Center.Y = (float)(((double)m_center.Y - (double)bias.Y) / ratio);
            
            CircleHoleEAG.RInner = radiusCircle / ratio;
            CircleHoleEAG.ROutter = radiusVertices / ratio;

            CircleHoleEAG.nVertices = nVertices;
            CircleHoleEAG.Degree = Auxi_Convert.RadianToDegree(m_angle);
        }
        public override RectangleF RealRectangleAround(int outrangex, int outrangey)
        {
            RectangleF retrectf = new RectangleF();

            string backupstring = ToShapeString();

            MappingToMovingObject(new PointF(0, 0), 1d);

            retrectf = RectAround;
            retrectf.Inflate(outrangex, outrangey);

            FromShapeString(backupstring);

            return retrectf;
        }
        // -------------------------------------------------        Draw Modified For Showing Requirement
        // -------------------------------------------------        Draw
        public override void Draw(Graphics grfx)
        {
            if (IsFirstSelected)
                BorderColor = Color.Red;
            else
            {
                if (IsSelected)
                    BorderColor = Color.Yellow;
                else
                {
                    BorderColor = Color.Lime;
                    ChangePenColorShape(BorderPen);
                }
            }


            //For Out Bording Show
            switch (ShowMode)
            {
                case ShowModeEnum.BORDERSHOW:
                    BorderPen.Color = Color.Purple;
                    BorderPen.DashStyle = DashStyle.Dot;
                    BorderPen.Width = 3;
                    break;
                case ShowModeEnum.MAINSHOW:
                    BorderPen.Color = MainShowPen.Color;
                    BorderPen.Width = MainShowPen.Width;
                    break;
                case ShowModeEnum.NORMAL:

                    if (LearnCount > 0)
                    {
                        BorderPen.DashStyle = DashStyle.Dot;
                    }

                    break;
            }

            if (Visible && VisibleAsMember)
            {
                GraphicsPath path = new GraphicsPath();
                PointF[] pts = Vertices;
                float f_rad = Convert.ToSingle(radiusCircle);
                RectangleF rcIn = new RectangleF(m_center.X - f_rad, m_center.Y - f_rad, 2 * f_rad, 2 * f_rad);
                path.AddPolygon(Vertices);
                path.AddEllipse(rcIn);

                grfx.FillPath(brush, path);

                grfx.DrawLine(new Pen(BorderColor, BorderPen.Width * 3), pts[0], pts[1]);

                if (bResize)
                {
                    if (InGroup)
                    {
                        grfx.DrawPolygon(penResize_InGroup, pts);
                        grfx.DrawEllipse(penResize_InGroup, rcIn);
                    }
                    else
                    {
                        grfx.DrawPolygon(BorderPen, pts);
                        grfx.DrawEllipse(BorderPen, rcIn);

                    }
                }

                foreach (PointF ptf in pts)
                {
                    grfx.FillEllipse(new SolidBrush(BorderPen.Color), ptf.X - (float)m_delta - 1, ptf.Y - (float)m_delta, (float)m_delta * 2 + 2, (float)m_delta * 2 + 2);
                }
            }
        }
        public override void DrawMask(Graphics grfx, PointF ptfoffset, float degree, SolidBrush backsolid, SolidBrush fillsolid, Size bmpsize)
        {
            string backupstring = ToShapeString();

            MappingToMovingObject(new PointF(0, 0), 1d);
            Move(-ptfoffset.X, -ptfoffset.Y);
            m_angle += Auxi_Convert.DegreeToRadian(degree);

            if (backsolid != null)
                grfx.FillRectangle(backsolid, new RectangleF(0, 0, bmpsize.Width, bmpsize.Height));

            GraphicsPath path = new GraphicsPath();
            PointF[] pts = Vertices;
            float f_rad = Convert.ToSingle(radiusCircle);
            RectangleF rcIn = new RectangleF(m_center.X - f_rad, m_center.Y - f_rad, 2 * f_rad, 2 * f_rad);
            path.AddPolygon(Vertices);
            path.AddEllipse(rcIn);

            grfx.FillPath(fillsolid, path);

            FromShapeString(backupstring);
        }
        public override void Draw(Bitmap bmp, PointF ptfoffset, float degree, SolidBrush backsolid, SolidBrush fillsolid)  //To Be Determinate
        {
            Graphics grfx = Graphics.FromImage(bmp);

            DrawMask(grfx, ptfoffset, degree, backsolid, fillsolid, bmp.Size);

            grfx.Dispose();
        }
        public override void GenSearchImage(int outrangex, int outrangey, Bitmap bmporg, ref Bitmap bmpsearch, ref Bitmap bmpmask)
        {
            RectangleF rectf = RealRectangleAround(outrangex, outrangey);

            rectf.Intersect(new RectangleF(new PointF(0, 0), bmporg.Size));

            bmpsearch.Dispose();
            bmpsearch = (Bitmap)bmporg.Clone(rectf, PixelFormat.Format32bppArgb);

            bmpmask.Dispose();
            bmpmask = new Bitmap(bmpsearch);

            Draw(bmpmask, new PointF(rectf.X, rectf.Y), 0f, new SolidBrush(Color.Black), new SolidBrush(Color.White));
        }
        public override void DigImage(int outrangex, int outrangey, Bitmap bmp)
        {
            RectangleF rectf = RealRectangleAround(outrangex, outrangey);

            Draw(bmp, new PointF(rectf.X, rectf.Y), 0f, null, new SolidBrush(Color.Red));
        }
        
    }
}
