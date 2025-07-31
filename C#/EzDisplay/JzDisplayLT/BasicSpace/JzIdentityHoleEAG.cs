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
    public class JzIdentityHoleEAG : GeoFigure
    {
        class IdentityHoleEAGClass
        {
            public PointF Center;
            public float RInner;
            public float ROutter;
            public int nVertices;

            public double Degree;

            string BackupString = "";

            public IdentityHoleEAGClass(Figure_EAG figure)
            {
                Center = new PointF(200, 200);
                RInner = 50;
                ROutter = 100;

                switch (figure)
                {
                    case Figure_EAG.HexHex:
                        nVertices = 6;
                        Degree = 0;
                        break;
                    case Figure_EAG.RectRect:
                        nVertices = 4;
                        Degree = 45;
                        break;
                }
            }
            public IdentityHoleEAGClass(PointF center, float rinner,float routter,int nvertices, double angle)
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
                RInner = Math.Max(50, Math.Max(rectf.Width / 2, rectf.Height / 2) - 50);
                ROutter = Math.Max(100, Math.Max(rectf.Width / 2, rectf.Height / 2));
            }
            public void Restore()
            {
                FromString(BackupString);
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

        float rInner, rOuter;
        int nVertices;
        SolidBrush brush;

        PointF ptEndIn, ptEndOut;
        double scaling, compensation;    // only between MouseDown and MouseUp

        double m_delta = 2;        // minimum width of the strip on border is 2 * delta
        double deltaR;
        static float minInnerR = 2;
        static float minW = 2;

        IdentityHoleEAGClass IdentityHoleEAG = new IdentityHoleEAGClass(Figure_EAG.RectRect);

        // -------------------------------------------------
        public JzIdentityHoleEAG (Control pic, Mover mvr, 
                                          PointF ptC, float radiusIn, float radiusOut, int vertices, double angleDegree, Color color, 
                                          bool in_group, Figure_EAG figure)
        {
            picImage = pic;
            supervisor = mvr;
            m_figure = figure;

            m_center = ptC;

            float rSmaller = Math .Min (Math .Abs (radiusOut), Math .Abs (radiusIn));
            float rBigger = Math .Max (Math .Abs (radiusOut), Math .Abs (radiusIn));
            rInner = Math .Max (rSmaller, minInnerR);
            rOuter = Math .Max (rBigger, rInner + minW);
            nVertices = Math .Max (Math .Abs (vertices), 3);
            m_angle = Auxi_Convert .DegreeToRadian (Auxi_Common .LimitedDegree (angleDegree));

            IdentityHoleEAG = new IdentityHoleEAGClass(m_center, rInner, rOuter, nVertices, angleDegree);

            brush = new SolidBrush (color);
            bInGroup = in_group;
            deltaR = m_delta / Math .Cos (Math .PI / nVertices);
        }
        public JzIdentityHoleEAG(Control pic, Mover mvr, string str, Color color, Figure_EAG figure)    //Added By Victor Tsai
        {
            picImage = pic;
            supervisor = mvr;

            m_figure = figure;

            FromString(str);

            brush = new SolidBrush(color);
            bInGroup = false;
        }
        public JzIdentityHoleEAG(string str, Color color)    //Added By Victor Tsai
        {
            FromString(str);

            nVertices = IdentityHoleEAG.nVertices;
            rInner = IdentityHoleEAG.RInner;
            rOuter = IdentityHoleEAG.ROutter;
            m_angle = Auxi_Convert.DegreeToRadian(Auxi_Common.LimitedDegree(IdentityHoleEAG.Degree));

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
        public JzIdentityHoleEAG(Color color, Figure_EAG figure)                       //Added By Victor Tsai
        {
            IdentityHoleEAG = new IdentityHoleEAGClass(figure);

            FromString(figure.ToString() + ";" + IdentityHoleEAG.ToString());

            brush = new SolidBrush(color);
            bInGroup = false;

            deltaR = m_delta / Math.Cos(Math.PI / nVertices);
        }
        public JzIdentityHoleEAG(Color color, Figure_EAG figure,RectangleF rectf)                       //Added By Victor Tsai
        {
            IdentityHoleEAG = new IdentityHoleEAGClass(figure);
            IdentityHoleEAG.FromRectF(rectf);

            FromString(figure.ToString() + ";" + IdentityHoleEAG.ToString());

            nVertices = IdentityHoleEAG.nVertices;
            rInner = IdentityHoleEAG.RInner;
            rOuter = IdentityHoleEAG.ROutter;
            m_angle = Auxi_Convert.DegreeToRadian(Auxi_Common.LimitedDegree(IdentityHoleEAG.Degree));
            
            brush = new SolidBrush(color);
            bInGroup = false;

            deltaR = m_delta / Math.Cos(Math.PI / nVertices);
        }
        public JzIdentityHoleEAG(Control pic, Mover mvr, JzIdentityHoleEAG frompolypoly)    //Added By Victor Tsai
        {
            picImage = pic;
            supervisor = mvr;

            m_figure = frompolypoly.m_figure;

            FromString(frompolypoly.ToString());

            IdentityHoleEAG.Center.X += 100f;
            IdentityHoleEAG.Center.Y += 100f;

            nVertices = frompolypoly.nVertices;
            rInner = frompolypoly.rInner;
            rOuter = frompolypoly.rOuter;
            m_angle = frompolypoly.m_angle;
            
            brush = frompolypoly.brush;

            deltaR = m_delta / Math.Cos(Math.PI / nVertices);

            bInGroup = false;

            IsFirstSelected = frompolypoly.IsFirstSelected;
            IsSelected = frompolypoly.IsSelected;

            frompolypoly.IsFirstSelected = false;
            frompolypoly.IsSelected = false;
        }
        // -------------------------------------------------        Copy
        public override GeoFigure Copy (Control pic, Mover mvr, PointF pt)
        {
            JzIdentityHoleEAG elem = new JzIdentityHoleEAG (pic, mvr, 
                                                                            pt, rInner, rOuter, nVertices, 
                                                                            Auxi_Convert .RadianToDegree (m_angle), Color, InGroup, m_figure);
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
            bool bRet = minInnerR <= coef * rInner &&
                        minW <= coef * (rOuter - rInner) * Math .Cos (Math .PI / nVertices);
            return (bRet);
        }
        // -------------------------------------------------        Zoom
        public override bool Zoom (double coef, bool bCheck)
        {
            coef = Math .Abs (coef);
            if (bCheck)
            {
                if (minInnerR > coef * rInner ||
                    minW > coef * (rOuter - rInner) * Math .Cos (Math .PI / nVertices))
                {
                    return (false);
                }
            }
            rInner = Convert .ToSingle (rInner * coef);
            rOuter = Convert .ToSingle (rOuter * coef);
            DefineCover ();
            return (true);
        }
        // -------------------------------------------------        SqueezeToMinimal
        public override void SqueezeToMinimal ()
        {
            double fCoef_forInner = minInnerR / rInner;
            double fCoef_forWidth = minW / ((rOuter - rInner) * Math .Cos (Math .PI / nVertices));
            double fCoef = Math .Max (fCoef_forInner, fCoef_forWidth);
            Zoom (fCoef, false);
        }
        // -------------------------------------------------        SqueezePossible
        public override bool SqueezePossible
        {
            get
            {
                double rInscribed = rOuter * Math .Cos (Math .PI / nVertices);
                return (minInnerR < rInner &&
                        minW < (rOuter - rInner) * Math .Cos (Math .PI / nVertices));
            }
        }
        // -------------------------------------------------        RadiusIn
        public float RadiusIn
        {
            get { return (rInner); }
        }
        // -------------------------------------------------        RadiusOut
        public float RadiusOut
        {
            get { return (rOuter); }
        }
        // -------------------------------------------------        VerticesNumber
        public int VerticesNumber
        {
            get { return (nVertices); }
        }
        // -------------------------------------------------        VerticesIn
        public PointF [] VerticesInner
        {
            get { return (Auxi_Geometry .RegularPolygon (m_center, rInner, nVertices, m_angle)); }
        }
        // -------------------------------------------------        VerticesOut
        public PointF [] VerticesOuter
        {
            get { return (Auxi_Geometry .RegularPolygon (m_center, rOuter, nVertices, m_angle)); }
        }
        // -------------------------------------------------        MinimumInnerRadius
        static public float MinimumInnerRadius
        {
            get { return (minInnerR); }
        }
        // -------------------------------------------------        MinimumWidth
        static public float MinimumWidth
        {
            get { return (minW); }
        }
        // -------------------------------------------------        RectAround
        public override RectangleF RectAround
        {
            get { return (Auxi_Geometry .RectangleAroundPoints (VerticesOuter)); }
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
        //        PointF [] ptsIn = VerticesInner;
        //        PointF [] ptsOut = VerticesOuter;
        //        GraphicsPath path = new GraphicsPath ();
        //        path .AddPolygon (ptsIn);
        //        path .AddPolygon (ptsOut);
        //        grfx .FillPath (brush, path);

        //        if (bResize)
        //        {
        //            if (InGroup)
        //            {
        //                grfx .DrawPolygon (penResize_InGroup, ptsOut);
        //                grfx .DrawPolygon (penResize_InGroup, ptsIn);
        //            }
        //            else
        //            {
        //                penResize_Independent.Width = 2;
        //                penResize_Independent.Color = Color.Red;

        //                grfx .DrawPolygon (penResize_Independent, ptsOut);
        //                grfx .DrawPolygon (penResize_Independent, ptsIn);
        //            }
        //        }

        //        foreach(PointF ptf in ptsIn)
        //        {
        //            grfx.FillEllipse(Brushes.Red, ptf.X - (float)m_delta - 1, ptf.Y - (float)m_delta, (float)m_delta * 2 + 2, (float)m_delta * 2 + 2);
        //        }
        //        foreach (PointF ptf in ptsOut)
        //        {
        //            grfx.FillEllipse(Brushes.Red, ptf.X - (float)m_delta - 1, ptf.Y - (float)m_delta - 1, (float)m_delta * 2 + 2, (float)m_delta * 2 + 2);
        //        }
        //    }
        //}
        // -------------------------------------------------        StartScaling
        public void StartScaling (Point ptMouse, int iNode)
        {
            PointF ptNearest;
            double angleBeam;
            if (iNode == 1)
            {
                Auxi_Geometry .Distance_PointPolyline (ptMouse, VerticesInner, true, out ptNearest);
                AdjustCursorPosition (ptNearest);

                scaling = rInner / Auxi_Geometry .Distance (Center, ptNearest);
                angleBeam = Auxi_Geometry .Line_Angle (Center, ptNearest);
                ptEndIn = Auxi_Geometry .PointToPoint (Center, angleBeam, minInnerR / scaling);
                ptEndOut = Auxi_Geometry .PointToPoint (Center, angleBeam, (rOuter - minW / Math .Cos (Math .PI / nVertices)) / scaling);
            }
            else if (iNode == 4)
            {
                Auxi_Geometry .Distance_PointPolyline (ptMouse, VerticesOuter, true, out ptNearest);
                AdjustCursorPosition (ptNearest);

                scaling = rOuter / Auxi_Geometry .Distance (Center, ptNearest);
                angleBeam = Auxi_Geometry .Line_Angle (Center, ptNearest);
                ptEndIn = Auxi_Geometry .PointToPoint (Center, angleBeam, (rInner + minW / Math .Cos (Math .PI / nVertices)) / scaling);
                ptEndOut = Auxi_Geometry .PointToPoint (Center, angleBeam, 4000);
            }
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
        // order of nodes:  two nodes near inner border
        //                - two nodes near outer border
        public override void DefineCover ()
        {
            PointF[] ptsOut = VerticesOuter;

            CoverNode [] nodes = new CoverNode [5];
            if (bResize)
            {
                nodes[0] = new CoverNode(0, Auxi_Geometry.RegularPolygon(m_center, rInner - deltaR, nVertices, m_angle), Behaviour.Transparent);
                nodes[1] = new CoverNode(1, Auxi_Geometry.RegularPolygon(m_center, rInner + deltaR, nVertices, m_angle), Cursors.Hand);

                nodes[2] = new CoverNode(2, Auxi_Geometry.RegularPolygon(m_center, rOuter - deltaR, nVertices, m_angle));
                nodes[2].SetBehaviourCursor(Behaviour.Transparent, Cursors.Default);

                nodes[3] = new CoverNode(3, ptsOut[0], ptsOut[1], (float)m_delta * 2, Cursors.SizeAll);

                nodes[4] = new CoverNode(4, Auxi_Geometry.RegularPolygon(m_center, rOuter + deltaR, nVertices, m_angle), Cursors.Hand);
               
            }
            //else
            //{
            //    nodes [0] = new CoverNode (0, VerticesInner, Behaviour .Transparent);
            //    nodes [1] = new CoverNode (1, Center, 0.0, Cursors .SizeAll);
            //    nodes [2] = new CoverNode (2, VerticesOuter);
            //    nodes [3] = new CoverNode (3, Center, 0.0, Cursors .SizeAll);
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
            if (catcher == MouseButtons .Left)
            {
                double dist;
                PointF ptNearest;

                if (iNode == 3)
                {
                    Move(dx, dy);
                }
                else if (iNode == 1)
                {
                    ptNearest = Auxi_Geometry .NearestPointOnSegment (ptM, ptEndIn, ptEndOut);
                    dist = Auxi_Geometry .Distance (Center, ptNearest);
                    rInner = Convert .ToSingle (dist * scaling);
                    AdjustCursorPosition (ptNearest);
                }
                else if (iNode == 4)
                {
                    ptNearest = Auxi_Geometry .NearestPointOnSegment (ptM, ptEndIn, ptEndOut);
                    dist = Auxi_Geometry .Distance (Center, ptNearest);
                    rOuter = Convert .ToSingle (dist * scaling);
                    AdjustCursorPosition (ptNearest);
                }
                return (true);
            }
            else if (catcher == MouseButtons .Right && bRotate)
            {
                double angleMouse = Auxi_Geometry .Line_Angle (m_center, ptM);
                m_angle = Auxi_Common .LimitedRadian (angleMouse - compensation);
                DefineCover ();
                return (true);
            }
            return (false);
        }

        const string nameMain = "RegPoly_IdenticalHole_EAG_";
        // -------------------------------------------------        IntoRegistry
        public override void IntoRegistry (RegistryKey regkey, string strAdd)
        {
            try
            {
                regkey .SetValue (nameMain + strAdd, new string [] {m_version .ToString (),   // 0
                                                                    m_center .X .ToString (),     // 1
                                                                    m_center .Y .ToString (),     // 2
                                                                    rInner .ToString (),        // 3  
                                                                    rOuter .ToString (),        // 4  
                                                                    nVertices .ToString (),     // 5   
                                                                    Auxi_Convert .RadianToDegree (m_angle) .ToString (),  // 6     
                                                                    ((int) (Color .A)) .ToString (),    // 7
                                                                    ((int) (Color .R)) .ToString (),    // 8
                                                                    ((int) (Color .G)) .ToString (),    // 9
                                                                    ((int) (Color .B)) .ToString (),    // 10
                                                                    bInGroup .ToString (),      // 11
                                                                    bResize .ToString (),       // 12
                                                                    bRotate .ToString (),       // 13
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
        public static JzIdentityHoleEAG FromRegistry (Control pic, Mover mvr, RegistryKey regkey, string strAdd)
        {
            try
            {
                string [] strs = (string []) regkey .GetValue (nameMain + strAdd);
                if (strs == null || strs .Length < 16 || Convert .ToInt32 (strs [0]) != 701)
                {
                    return (null);
                }
                JzIdentityHoleEAG poly = new JzIdentityHoleEAG (pic, mvr, 
                                                                                Auxi_Convert .ToPointF (strs, 1),       // center
                                                                                Convert .ToSingle (strs [3]),       // radiusIn
                                                                                Convert .ToSingle (strs [4]),       // radiusOut
                                                                                Convert .ToInt32 (strs [5]),        // vertices
                                                                                Convert .ToDouble (strs [6]),       // angleDegree
                                                                                Auxi_Convert .ToColor (strs, 7),    // color
                                                                                Convert .ToBoolean (strs [11]),
                                                                                Figure_EAG.Any);    // in_group
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
            IdentityHoleEAG.SetOffset(offsetpoint);
        }
        public override void SetAngle(double addangle)
        {
            IdentityHoleEAG.SetAngle(addangle);
        }
        public override void FromRectangleF(RectangleF rectf)
        {
            IdentityHoleEAG.FromRectF(rectf);
        }
        
        public override string ToString()
        {
            string retstr = "";

            retstr += m_figure.ToString() + ";";
            retstr += IdentityHoleEAG.ToString();

            return retstr;

        }
        public override void FromString(string str)
        {
            string[] strs = str.Split(';');

            m_figure = (Figure_EAG)Enum.Parse(typeof(Figure_EAG), strs[0]);
            IdentityHoleEAG.FromString(strs[1]);
        }

        public override string ToShapeString()
        {
            string retstr = "";

            retstr += PointFToString(m_center) + ";";   //0
            retstr += rInner.ToString() + ";";          //1
            retstr += rOuter.ToString() + ";";          //2
            retstr += nVertices.ToString() + ";";       //3
            retstr += m_angle.ToString();               //4

            return retstr;
        }
        public override void FromShapeString(string str)
        {
            string[] strs = str.Split(';');

            m_center = StringToPointF(strs[0]);
            rInner = float.Parse(strs[1]);
            rOuter = float.Parse(strs[2]);
            nVertices = int.Parse(strs[3]);
            m_angle = double.Parse(strs[4]);


        }
        public override void Backup()
        {
            IdentityHoleEAG.Backup();
        }
        public override void Restore()
        {
            IdentityHoleEAG.Restore();
        }
        public override void MappingToMovingObject(PointF bias, double ratio)
        {
            m_center.X = (float)((double)bias.X + ((double)IdentityHoleEAG.Center.X * ratio));
            m_center.Y = (float)((double)bias.Y + ((double)IdentityHoleEAG.Center.Y * ratio));
            
            rInner = (float)((double)IdentityHoleEAG.RInner * ratio);
            rOuter = (float)((double)IdentityHoleEAG.ROutter * ratio);

            Move(OffsetPoint.X, OffsetPoint.Y);
            OffsetPoint = new Point(0, 0);
        }
        public override void MappingToMovingObject(PointF bias, SizeF sizeratio)
        {
            double ratio = Math.Min(sizeratio.Width, sizeratio.Height);

            m_center.X = (float)((double)bias.X + ((double)IdentityHoleEAG.Center.X * sizeratio.Width));
            m_center.Y = (float)((double)bias.Y + ((double)IdentityHoleEAG.Center.Y * sizeratio.Height));

            rInner = (float)((double)IdentityHoleEAG.RInner * ratio);
            rOuter = (float)((double)IdentityHoleEAG.ROutter * ratio);

            Move(OffsetPoint.X, OffsetPoint.Y);
            OffsetPoint = new Point(0, 0);

            MappingFromMovingObject(new PointF(0, 0), 1);
        }
        public override void MappingFromMovingObject(PointF bias, double ratio)
        {
            IdentityHoleEAG.Center.X = (float)(((double)m_center.X - (double)bias.X) / ratio);
            IdentityHoleEAG.Center.Y = (float)(((double)m_center.Y - (double)bias.Y) / ratio);

            IdentityHoleEAG.RInner = (float)((double)rInner / ratio);
            IdentityHoleEAG.ROutter = (float)((double)rOuter / ratio);

            IdentityHoleEAG.nVertices = nVertices;
            IdentityHoleEAG.Degree = Auxi_Convert.RadianToDegree(m_angle);
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
                PointF[] ptsIn = VerticesInner;
                PointF[] ptsOut = VerticesOuter;
                GraphicsPath path = new GraphicsPath();
                path.AddPolygon(ptsIn);
                path.AddPolygon(ptsOut);
                grfx.FillPath(brush, path);

                grfx.DrawLine(new Pen(BorderPen.Color, BorderPen.Width * 3), ptsOut[0], ptsOut[1]);

                if (bResize)
                {
                    if (InGroup)
                    {
                        grfx.DrawPolygon(penResize_InGroup, ptsOut);
                        grfx.DrawPolygon(penResize_InGroup, ptsIn);
                    }
                    else
                    {
                        grfx.DrawPolygon(BorderPen, ptsOut);
                        grfx.DrawPolygon(BorderPen, ptsIn);
                    }
                }

                foreach (PointF ptf in ptsIn)
                {
                    grfx.FillEllipse(new SolidBrush(BorderPen.Color), ptf.X - (float)m_delta - 1, ptf.Y - (float)m_delta, (float)m_delta * 2 + 2, (float)m_delta * 2 + 2);
                }
                foreach (PointF ptf in ptsOut)
                {
                    grfx.FillEllipse(new SolidBrush(BorderPen.Color), ptf.X - (float)m_delta - 1, ptf.Y - (float)m_delta - 1, (float)m_delta * 2 + 2, (float)m_delta * 2 + 2);
                }
            }
        }
        public override void DrawMask(Graphics grfx, PointF ptfoffset,float degree, SolidBrush backsolid, SolidBrush fillsolid, Size bmpsize)
        {
            string backupstring = ToShapeString();

            MappingToMovingObject(new PointF(0, 0), 1d);
            Move(-ptfoffset.X, -ptfoffset.Y);
            m_angle += Auxi_Convert.DegreeToRadian(degree);

            if (backsolid != null)
                grfx.FillRectangle(backsolid, new RectangleF(0, 0, bmpsize.Width, bmpsize.Height));

            PointF[] ptsIn = VerticesInner;
            PointF[] ptsOut = VerticesOuter;
            GraphicsPath path = new GraphicsPath();
            path.AddPolygon(ptsIn);
            path.AddPolygon(ptsOut);
            grfx.FillPath(fillsolid, path);

            FromShapeString(backupstring);
        }
        public override void Draw(Bitmap bmp, PointF ptfoffset, float degree, SolidBrush backsolid, SolidBrush fillsolid)  //To Be Determinate
        {
            Graphics grfx = Graphics.FromImage(bmp);

            DrawMask(grfx, ptfoffset, degree, backsolid, fillsolid, bmp.Size);
            //string backupstring = ToShapeString();

            //MappingToMovingObject(new PointF(0, 0), 1d);
            //Move(-ptfoffset.X, -ptfoffset.Y);

            //if (backsolid != null)
            //    grfx.FillRectangle(backsolid, new RectangleF(0, 0, bmp.Width, bmp.Height));

            //PointF[] ptsIn = VerticesInner;
            //PointF[] ptsOut = VerticesOuter;
            //GraphicsPath path = new GraphicsPath();
            //path.AddPolygon(ptsIn);
            //path.AddPolygon(ptsOut);
            //grfx.FillPath(fillsolid, path);

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

            Draw(bmp, new PointF(rectf.X, rectf.Y), 0f, null, new SolidBrush(Color.Red));
        }

    }
}
