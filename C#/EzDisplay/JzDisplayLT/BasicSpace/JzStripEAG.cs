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
    public class JzStripEAG : GeoFigure
    {
        class StripEAGClass
        {
            public PointF Ptc0;
            public PointF Ptc1;
            public double Radius;

            string BackupString = "";

            public StripEAGClass()
            {
                Ptc0 = new PointF(200, 200);
                Ptc1 = new PointF(300, 200);
                Radius = 50;

            }
            public StripEAGClass(PointF ptc0, PointF ptc1, double radius)
            {
                Ptc0 = ptc0;
                Ptc1 = ptc1;
                Radius = radius;

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
                Ptc0.X += offset.X;
                Ptc0.Y += offset.Y;

                Ptc1.X += offset.X;
                Ptc1.Y += offset.Y;
            }
            public void FromRectF(RectangleF rectf)
            {
                float sizemax = Math.Max(rectf.Width, rectf.Height);

                Ptc0 = new PointF(rectf.X + sizemax / 8, rectf.Y + sizemax / 8);
                Ptc1 = new PointF(rectf.X + rectf.Width - sizemax / 8, rectf.Y + rectf.Height - sizemax / 8);
                Radius = sizemax / 8;
            }

            public override string ToString()
            {
                string str = "";

                str += PointFToString(Ptc0) + "X";      //0
                str += PointFToString(Ptc1) + "X";      //1
                str += Radius.ToString();               //2
                
                return str;
            }
            public void FromString(string str)
            {
                string[] strs = str.Split('X');

                Ptc0 = StringToPointF(strs[0]);
                Ptc1 = StringToPointF(strs[1]);
                Radius = double.Parse(strs[2]);
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

        PointF ptC0, ptC1;
        double m_radius;
        SolidBrush brush;
        PointF ptEndIn, ptEndOut;

        int delta = 3;
        static double minRadius = 3;
        static double minStraight = 5;

        double compensation, length, additionToLength;
        PointF [] corners;

        StripEAGClass StripEAG = new StripEAGClass();
        // -------------------------------------------------
        public JzStripEAG(Control pic, Mover mvr, PointF ptA, PointF ptB, double rad, Color color, bool in_group)
        {
            picImage = pic;
            supervisor = mvr;
            m_figure = Figure_EAG.Strip;

            ptC0 = ptA;
            ptC1 = ptB;
            m_radius = Math.Max(minRadius, Math.Abs(rad));

            m_angle = Auxi_Geometry.Line_Angle(ptC0, ptC1);
            if (Auxi_Geometry.Distance(ptC0, ptC1) < minStraight)
            {
                ptC1 = Auxi_Geometry.PointToPoint(ptC0, m_angle, minStraight);
            }

            StripEAG = new StripEAGClass(ptC0, ptC1, m_radius);

            m_center = Auxi_Geometry.Middle(ptC0, ptC1);
            brush = new SolidBrush(color);

            bInGroup = in_group;
        }
        // -------------------------------------------------
        public JzStripEAG (Control pic, Mover mvr, PointF ptA, double rectlength, double angleDegree, double rad, Color color, bool in_group)
            : this (pic, mvr, 
                    ptA, Auxi_Geometry .PointToPoint (ptA, Auxi_Convert .DegreeToRadian (angleDegree), Math .Max (rectlength, minStraight)),
                    rad, color, in_group)
        {
        }
        public JzStripEAG(Control pic, Mover mvr, string str, Color color)    //Added By Victor Tsai
        {
            picImage = pic;
            supervisor = mvr;

            FromString(str);
            
            brush = new SolidBrush(color);
            bInGroup = false;
        }
        public JzStripEAG(string str, Color color)    //Added By Victor Tsai
        {
            FromString(str);

            brush = new SolidBrush(color);
            bInGroup = false;
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
        public JzStripEAG(Color color)                       //Added By Victor Tsai
        {
            StripEAG = new StripEAGClass();

            FromString(Figure_EAG.Strip.ToString() + ";" + StripEAG.ToString());

            brush = new SolidBrush(color);
            bInGroup = false;
        }
        public JzStripEAG(Color color,RectangleF rectf)                       //Added By Victor Tsai
        {
            StripEAG = new StripEAGClass();
            StripEAG.FromRectF(rectf);

            FromString(Figure_EAG.Strip.ToString() + ";" + StripEAG.ToString());

            brush = new SolidBrush(color);
            bInGroup = false;
        }
        public JzStripEAG(Control pic, Mover mvr, JzStripEAG fromstrip)    //Added By Victor Tsai
        {
            picImage = pic;
            supervisor = mvr;

            FromString(fromstrip.ToString());
            StripEAG.Ptc0.X += 100f;
            StripEAG.Ptc0.Y += 100f;
            StripEAG.Ptc1.X += 100f;
            StripEAG.Ptc1.Y += 100f;

            brush = fromstrip.brush;

            bInGroup = false;

            IsFirstSelected = fromstrip.IsFirstSelected;
            IsSelected = fromstrip.IsSelected;

            fromstrip.IsFirstSelected = false;
            fromstrip.IsSelected = false;
        }
        // -------------------------------------------------        Copy
        public override GeoFigure Copy (Control pic, Mover mvr, PointF pt)
        {
            SizeF shift = new SizeF (pt .X - m_center .X, pt .Y - m_center .Y);
            JzStripEAG elem = new JzStripEAG (pic, mvr, ptC0 + shift, ptC1 + shift, m_radius, Color, InGroup);
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
            bool bRet = minRadius <= coef * m_radius &&
                        minStraight <= coef * Auxi_Geometry .Distance (ptC0, ptC1);
            return (bRet);
        }
        // -------------------------------------------------        Zoom
        public override bool Zoom (double coef, bool bCheck)
        {
            coef = Math .Abs (coef);
            if (bCheck)
            {
                if (minRadius > coef * m_radius ||
                    minStraight > coef * Auxi_Geometry .Distance (ptC0, ptC1))
                {
                    return (false);
                }
            }
            m_radius *= coef;
            ptC0 = Auxi_Geometry .PointOnLine (m_center, ptC0, coef);
            ptC1 = Auxi_Geometry .PointOnLine (m_center, ptC1, coef);
            DefineCover ();
            return (true);
        }
        // -------------------------------------------------        SqueezeToMinimal
        public override void SqueezeToMinimal ()
        {
            double fCoef_forRadius = minRadius / m_radius;
            double fCoef_forLength = minStraight / Auxi_Geometry .Distance (ptC0, ptC1);
            double fCoef = Math .Max (fCoef_forRadius, fCoef_forLength);
            Zoom (fCoef, false);
        }
        // -------------------------------------------------        SqueezePossible
        public override bool SqueezePossible
        {
            get { return (minRadius < m_radius && minStraight <= Auxi_Geometry .Distance (ptC0, ptC1)); }
        }
        // -------------------------------------------------        Radius
        public double Radius
        {
            get { return (m_radius); }
        }
        // -------------------------------------------------        Focuses
        public PointF [] Focuses
        {
            get { return (new PointF [] { ptC0, ptC1 }); }
        }
        // -------------------------------------------------        Focus_0
        public PointF Focus_0
        {
            get { return (ptC0); }
        }
        // -------------------------------------------------        Focus_1
        public PointF Focus_1
        {
            get { return (ptC1); }
        }
        // -------------------------------------------------        CornerPoints
        private PointF [] CornerPoints ()
        {
            return (new PointF [] { Auxi_Geometry .PointToPoint (ptC1, m_angle + Math .PI / 2, m_radius), 
                                    Auxi_Geometry .PointToPoint (ptC0, m_angle + Math .PI / 2, m_radius),
                                    Auxi_Geometry .PointToPoint (ptC0, m_angle - Math .PI / 2, m_radius), 
                                    Auxi_Geometry .PointToPoint (ptC1, m_angle - Math .PI / 2, m_radius) });
        }
        // -------------------------------------------------        MinimumRadius
        static public double MinimumRadius
        {
            get { return (minRadius); }
        }
        // -------------------------------------------------        MinimumStraight
        static public double MinimumStraight
        {
            get { return (minStraight); }
        }
        // -------------------------------------------------        RectAround
        public override RectangleF RectAround
        {
            get
            {
                float f_rad = Convert .ToSingle (m_radius);
                return (RectangleF .FromLTRB (Math .Min (ptC0 .X, ptC1 .X) - f_rad, Math .Min (ptC0 .Y, ptC1 .Y) - f_rad,
                                              Math .Max (ptC0 .X, ptC1 .X) + f_rad, Math .Max (ptC0 .Y, ptC1 .Y) + f_rad));
            }
        }
        // -------------------------------------------------        Angle
        public override double Angle
        {
            get { return (m_angle); }
            set
            {
                m_angle = Auxi_Common .LimitedRadian (value);
                double length = Auxi_Geometry .Distance (ptC0, ptC1);
                ptC0 = Auxi_Geometry .PointToPoint (Center, m_angle + Math .PI, length / 2);
                ptC1 = Auxi_Geometry .PointToPoint (ptC0, m_angle, length);
                DefineCover ();
            }
        }
        // -------------------------------------------------        Color
        public override Color Color
        {
            get { return (brush .Color); }
            set { brush .Color = value; }
        }
        // -------------------------------------------------        Draw
        //public override void Draw (Graphics grfx)
        //{
        //    if (Visible && VisibleAsMember)
        //    {
        //        float f_rad = Convert .ToSingle (m_radius);
        //        Auxi_Drawing .FillRoundedStrip (grfx, ptC1, ptC0, f_rad, brush);
        //        if (bResize)
        //        {
        //            if (InGroup)
        //            {
        //                Auxi_Drawing .RoundedStrip (grfx, ptC0, ptC1, f_rad, penResize_InGroup );
        //            }
        //            else
        //            {
        //                penResize_Independent.Width = delta;
        //                penResize_Independent.Color = Color.Red;
                        
        //                Auxi_Drawing .RoundedStrip (grfx, ptC0, ptC1, f_rad, penResize_Independent);
        //            }
        //        }
        //        if (bRotate)
        //        {
        //            float r = 3;
        //            m_center = Auxi_Geometry .Middle (ptC0, ptC1);
        //            grfx .FillEllipse (brushAnchor, RectangleF .FromLTRB (m_center .X - r, m_center .Y - r, m_center .X + r, m_center .Y + r));
        //        }
        //    }
        //}
        // -------------------------------------------------        StartResizing
        public void StartResizing (Point ptMouse, int iNode)
        {
            corners = CornerPoints ();
            PointF ptBase, ptCursor;
            double angleBeam, dist;
            switch (iNode)
            {
                case 0:
                    Auxi_Geometry .Distance_PointLine (ptMouse, corners [2], corners [3], out ptBase);
                    angleBeam = m_angle + Math .PI / 2;
                    ptEndIn = Auxi_Geometry .PointToPoint (ptBase, angleBeam, 2 * minRadius);
                    ptEndOut = Auxi_Geometry .PointToPoint (ptBase, angleBeam, 4000);
                    ptCursor = Auxi_Geometry .PointToPoint (ptBase, angleBeam, 2 * m_radius);
                    break;
                case 1:
                    Auxi_Geometry .Distance_PointLine (ptMouse, corners [0], corners [1], out ptBase);
                    angleBeam = m_angle - Math .PI / 2;
                    ptEndIn = Auxi_Geometry .PointToPoint (ptBase, angleBeam, 2 * minRadius);
                    ptEndOut = Auxi_Geometry .PointToPoint (ptBase, angleBeam, 4000);
                    ptCursor = Auxi_Geometry .PointToPoint (ptBase, angleBeam, 2 * m_radius);
                    break;
                case 2:
                default:
                    ptCursor = ptMouse;
                    break;
                case 3:
                    dist = Auxi_Geometry .Distance_PointLine (ptMouse, corners [0], corners [3], out ptBase);
                    additionToLength = dist - Auxi_Geometry .Distance (ptC0, ptC1);
                    angleBeam = m_angle + Math .PI;
                    ptEndIn = Auxi_Geometry .PointToPoint (ptBase, angleBeam, MinimumStraight + additionToLength);
                    ptEndOut = Auxi_Geometry .PointToPoint (ptBase, angleBeam, 4000);
                    ptCursor = ptMouse;
                    break;
                case 4:
                    dist = Auxi_Geometry .Distance_PointLine (ptMouse, corners [1], corners [2], out ptBase);
                    additionToLength = dist - Auxi_Geometry .Distance (ptC0, ptC1);
                    angleBeam = m_angle;
                    ptEndIn = Auxi_Geometry .PointToPoint (ptBase, angleBeam, MinimumStraight + additionToLength);
                    ptEndOut = Auxi_Geometry .PointToPoint (ptBase, angleBeam, 4000);
                    ptCursor = ptMouse;
                    break;
            }
            AdjustCursorPosition (ptCursor);
        }
        // -------------------------------------------------        StartRotation
        public override void StartRotation (Point ptMouse)
        {
            m_center = Auxi_Geometry .Middle (ptC0, ptC1);  
            double angleMouse = Auxi_Geometry .Line_Angle (m_center, ptMouse);
            compensation = Auxi_Common .LimitedRadian (angleMouse - m_angle);
            length = Auxi_Geometry .Distance (ptC0, ptC1);
        }
        // -------------------------------------------------        AdjustCursorPosition
        private void AdjustCursorPosition (PointF pt)
        {
            supervisor .MouseTraced = false;
            Cursor .Position = picImage .PointToScreen (Point .Round (pt));
            supervisor .MouseTraced = true;
        }
        // -------------------------------------------------        DefineCover
        // order of nodes:
        //                  2           strips on the straight sides
        //                  1           big strip
        //                  2           big circles on the ends; the first is around ptC0
        //
        public override void DefineCover ()
        {
            CoverNode[] nodes;

            //if (bResize)
            {
                PointF[] pts = CornerPoints();
                nodes = new CoverNode[] {  new CoverNode (0, pts [0], pts [1], delta),
                                            new CoverNode (1, pts [2], pts [3], delta * 2, Cursors.SizeAll),
                                            new CoverNode (2, ptC0, ptC1, Convert .ToSingle (m_radius - delta), Cursors .SizeAll),
                                            new CoverNode (3, ptC0, Convert .ToSingle (m_radius + delta)),    //��������ح������� By Victor Tsai
                                            new CoverNode (4, ptC1, Convert .ToSingle (m_radius + delta))     //��������ح������� By Victor Tsai
                                            };
            }

            nodes[2].SetBehaviourCursor(Behaviour.Transparent, Cursors.Default);

            //else
            //{
            //    nodes = new CoverNode[] {  new CoverNode (0, ptC0, 0.0, Cursors .SizeAll),
            //                                new CoverNode (1, ptC1, 0.0, Cursors .SizeAll),
            //                                new CoverNode (2, ptC0, ptC1, Convert .ToSingle (m_radius), Cursors .SizeAll),
            //                                new CoverNode (3, ptC0, 0.0, Cursors .SizeAll),           //��������ح������� By Victor Tsai
            //                                new CoverNode (4, ptC1, 0.0, Cursors .SizeAll)            //��������ح������� By Victor Tsai
            //                                };
            //}
            cover = new Cover(nodes);

            cover.SetClearance(false);


            if (TransparentForMover)
                cover.SetBehaviour(Behaviour.Transparent);

            //PointF[] pts = CornerPoints();

            //int delta = Resizable ? 1 : 0;
            //CoverNode[] nodes = new CoverNode[] { new CoverNode (0, pts[0], pts[1], (float)(m_radius - delta), Cursors .SizeAll),
            //                                        new CoverNode (1, pts[0], pts[1], (float)m_radius),
            //                                        new CoverNode (2, pts[0], pts[1], (float)m_radius + delta)};
            //cover = new Cover(nodes);
            //cover.SetClearance(false);
            //if (TransparentForMover)
            //{
            //    cover.SetBehaviour(Behaviour.Transparent);
            //}


        }
        // -------------------------------------------------        Move
        public override void Move (int dx, int dy)
        {
            Size size = new Size (dx, dy);
            m_center += size;
            ptC0 += size;
            ptC1 += size;
        }
        public override void Move(float dx, float dy)
        {
            SizeF size = new SizeF(dx, dy);
            m_center += size;
            ptC0 += size;
            ptC1 += size;
        }
        // -------------------------------------------------        MoveNode
        public override bool MoveNode (int iNode, int dx, int dy, Point ptM, MouseButtons catcher)
        {
            bool bRet = false;

            if (catcher == MouseButtons .Left)
            {
                if (iNode != 2 && iNode != 1)
                {
                    corners = CornerPoints ();
                    double dist;
                    PointF ptNearest = Auxi_Geometry .NearestPointOnSegment (ptM, ptEndIn, ptEndOut);

                    if (iNode == 0)
                    {
                        dist = Auxi_Geometry .Distance_PointLine (ptNearest, corners [2], corners [3]);
                        ptC0 = Auxi_Geometry .PointToPoint (corners [2], m_angle + Math .PI / 2, dist / 2);
                        ptC1 = Auxi_Geometry .PointToPoint (corners [3], m_angle + Math .PI / 2, dist / 2);
                        m_radius = Convert .ToSingle (dist / 2);
                    }
                    else if (iNode == 1)
                    {
                        dist = Auxi_Geometry .Distance_PointLine (ptNearest, corners [0], corners [1]);
                        ptC0 = Auxi_Geometry .PointToPoint (corners [1], m_angle - Math .PI / 2, dist / 2);
                        ptC1 = Auxi_Geometry .PointToPoint (corners [0], m_angle - Math .PI / 2, dist / 2);
                        m_radius = Convert .ToSingle (dist / 2);
                    }
                    else if (iNode == 3)
                    {
                        dist = Auxi_Geometry .Distance_PointLine (ptNearest, corners [0], corners [3]);
                        ptC0 = Auxi_Geometry .PointToPoint (ptC1, m_angle + Math .PI, dist - additionToLength);
                        DefineCover ();
                    }
                    else if (iNode == 4)
                    {
                        dist = Auxi_Geometry .Distance_PointLine (ptNearest, corners [1], corners [2]);
                        ptC1 = Auxi_Geometry .PointToPoint (ptC0, m_angle, dist - additionToLength);
                        DefineCover ();
                    }
                    AdjustCursorPosition (ptNearest);
                    bRet = true;
                }
                else
                {
                    Move (dx, dy);
                }
            }
            else if (catcher == MouseButtons .Right && bRotate)
            {
                double angleMouse = Auxi_Geometry .Line_Angle (m_center, ptM);
                m_angle = angleMouse - compensation;
                ptC0 = Auxi_Geometry .PointToPoint (m_center, m_angle + Math .PI, length / 2);
                ptC1 = Auxi_Geometry .PointToPoint (ptC0, m_angle, length);
                DefineCover ();
                bRet = true;
            }
            return (bRet);
        }

        const string nameMain = "Strip_EAG_";
        // -------------------------------------------------        IntoRegistry
        public override void IntoRegistry (RegistryKey regkey, string strAdd)
        {
            try
            {
                regkey .SetValue (nameMain + strAdd, new string [] {m_version .ToString (),   // 0
                                                                    Focus_0 .X .ToString (),    // 1
                                                                    Focus_0 .Y .ToString (),    // 2
                                                                    Focus_1 .X .ToString (),    // 3
                                                                    Focus_1 .Y .ToString (),    // 4
                                                                    m_radius .ToString (),        // 5  
                                                                    ((int) (Color .A)) .ToString (),    // 6
                                                                    ((int) (Color .R)) .ToString (),    // 7
                                                                    ((int) (Color .G)) .ToString (),    // 8
                                                                    ((int) (Color .B)) .ToString (),    // 9
                                                                    bInGroup .ToString (),      // 10
                                                                    bResize .ToString (),       // 11
                                                                    bRotate .ToString (),       // 12
                                                                    Visible .ToString (),               // 13
                                                                    VisibleAsMember .ToString () },     // 14
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
        public static JzStripEAG FromRegistry (Control pic, Mover mvr, RegistryKey regkey, string strAdd)
        {
            try
            {
                string [] strs = (string []) regkey .GetValue (nameMain + strAdd);
                if (strs == null || strs .Length < 15 || Convert .ToInt32 (strs [0]) != 701)
                {
                    return (null);
                }
                JzStripEAG strip = new JzStripEAG (pic, mvr, 
                                                 Auxi_Convert .ToPointF (strs, 1),      // ptA
                                                 Auxi_Convert .ToPointF (strs, 3),      // ptB
                                                 Convert .ToDouble (strs [5]),          // radius
                                                 Auxi_Convert .ToColor (strs, 6),       // color
                                                 Convert .ToBoolean (strs [10]));       // in_group
                if (strip != null)
                {
                    strip .Resizable = Convert .ToBoolean (strs [11]);
                    strip .Rotatable = Convert .ToBoolean (strs [12]);
                    strip .Visible = Convert .ToBoolean (strs [13]);
                    strip .VisibleAsMember = Convert .ToBoolean (strs [14]);
                }
                return (strip);
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
            StripEAG.SetOffset(offsetpoint);
        }
        public override void SetAngle(double addangle)
        {
            
        }
        public override void FromRectangleF(RectangleF rectf)
        {
            StripEAG.FromRectF(rectf);
        }
        
        public override string ToString()
        {
            string retstr = "";

            retstr += m_figure.ToString() + ";";
            retstr += StripEAG.ToString();

            return retstr;

        }
        public override void FromString(string str)
        {
            string[] strs = str.Split(';');

            m_figure = (Figure_EAG)Enum.Parse(typeof(Figure_EAG), strs[0]);
            StripEAG.FromString(strs[1]);
        }
        public override string ToShapeString()
        {
            string retstr = "";

            retstr += PointFToString(ptC0) + ";";
            retstr += PointFToString(ptC1) + ";";
            retstr += m_radius.ToString() + ";";
            
            return retstr;
        }
        public override void FromShapeString(string str)
        {
            int i = 0;

            string[] strs = str.Split(';');

            ptC0 = StringToPointF(strs[0]);
            ptC1 = StringToPointF(strs[1]);
            m_radius = double.Parse(strs[2]);

        }
        public override void Backup()
        {
            StripEAG.Backup();
        }
        public override void Restore()
        {
            StripEAG.Restore();
        }
        public override void MappingToMovingObject(PointF bias, double ratio)
        {
            ptC0.X = (float)((double)bias.X + ((double)StripEAG.Ptc0.X * ratio));
            ptC0.Y = (float)((double)bias.Y + ((double)StripEAG.Ptc0.Y * ratio));

            ptC1.X = (float)((double)bias.X + ((double)StripEAG.Ptc1.X * ratio));
            ptC1.Y = (float)((double)bias.Y + ((double)StripEAG.Ptc1.Y * ratio));
            
            m_radius = StripEAG.Radius * ratio;

            m_angle = Auxi_Geometry.Line_Angle(ptC0, ptC1);
            //if (Auxi_Geometry.Distance(ptC0, ptC1) < minStraight)
            //{
            //    ptC1 = Auxi_Geometry.PointToPoint(ptC0, m_angle, minStraight);
            //}
            m_center = Auxi_Geometry.Middle(ptC0, ptC1);

            Move(OffsetPoint.X, OffsetPoint.Y);
            OffsetPoint = new Point(0, 0);
        }
        public override void MappingToMovingObject(PointF bias, SizeF sizeratio)
        {
            ptC0.X = (float)((double)bias.X + ((double)StripEAG.Ptc0.X * sizeratio.Width));
            ptC0.Y = (float)((double)bias.Y + ((double)StripEAG.Ptc0.Y * sizeratio.Height));

            ptC1.X = (float)((double)bias.X + ((double)StripEAG.Ptc1.X * sizeratio.Width));
            ptC1.Y = (float)((double)bias.Y + ((double)StripEAG.Ptc1.Y * sizeratio.Height));

            m_radius = StripEAG.Radius * Math.Min(sizeratio.Width, sizeratio.Height);

            m_angle = Auxi_Geometry.Line_Angle(ptC0, ptC1);
            //if (Auxi_Geometry.Distance(ptC0, ptC1) < minStraight)
            //{
            //    ptC1 = Auxi_Geometry.PointToPoint(ptC0, m_angle, minStraight);
            //}
            m_center = Auxi_Geometry.Middle(ptC0, ptC1);

            Move(OffsetPoint.X, OffsetPoint.Y);
            OffsetPoint = new Point(0, 0);

            MappingFromMovingObject(new PointF(0, 0), 1);
        }
        public override void MappingFromMovingObject(PointF bias, double ratio)
        {
            StripEAG.Ptc0.X = (float)(((double)ptC0.X - (double)bias.X) / ratio);
            StripEAG.Ptc0.Y = (float)(((double)ptC0.Y - (double)bias.Y) / ratio);
            
            StripEAG.Ptc1.X = (float)(((double)ptC1.X - (double)bias.X) / ratio);
            StripEAG.Ptc1.Y = (float)(((double)ptC1.Y - (double)bias.Y) / ratio);

            StripEAG.Radius = m_radius / ratio;
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
                float f_rad = Convert.ToSingle(m_radius);
                Auxi_Drawing.FillRoundedStrip(grfx, ptC1, ptC0, f_rad, brush);

                corners = CornerPoints();
                grfx.DrawLine(new Pen(BorderColor, BorderPen.Width * 3), corners[2], corners[3]);

                if (bResize)
                {
                    if (InGroup)
                    {
                        Auxi_Drawing.RoundedStrip(grfx, ptC0, ptC1, f_rad, BorderPen);
                    }
                    else
                    {
                        Auxi_Drawing.RoundedStrip(grfx, ptC0, ptC1, f_rad, BorderPen);
                    }
                }
                if (bRotate)
                {
                    float r = 3;
                    m_center = Auxi_Geometry.Middle(ptC0, ptC1);
                    grfx.FillEllipse(brushAnchor, RectangleF.FromLTRB(m_center.X - r, m_center.Y - r, m_center.X + r, m_center.Y + r));
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

            float f_rad = Convert.ToSingle(m_radius);
            Auxi_Drawing.FillRoundedStrip(grfx, ptC1, ptC0, f_rad, fillsolid);

            FromShapeString(backupstring);
        }
        public override void Draw(Bitmap bmp, PointF ptfoffset,float degree, SolidBrush backsolid, SolidBrush fillsolid)  //To Be Determinate
        {
            Graphics grfx = Graphics.FromImage(bmp);

            DrawMask(grfx, ptfoffset, degree, backsolid, fillsolid, bmp.Size);

            //string backupstring = ToShapeString();

            //MappingToMovingObject(new PointF(0, 0), 1d);
            //Move(-ptfoffset.X, -ptfoffset.Y);

            //if (backsolid != null)
            //    grfx.FillRectangle(backsolid, new RectangleF(0, 0, bmp.Width, bmp.Height));

            //float f_rad = Convert.ToSingle(m_radius);
            //Auxi_Drawing.FillRoundedStrip(grfx, ptC1, ptC0, f_rad, fillsolid);

            //FromShapeString(backupstring);

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

            Draw(bmp, new PointF(rectf.X, rectf.Y), 0, null, new SolidBrush(Color.Red));
        }
    }
}
