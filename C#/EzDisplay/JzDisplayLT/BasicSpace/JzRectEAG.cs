using JzDisplay;
using Microsoft.Win32;
using MoveGraphLibrary;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace WorldOfMoveableObjects
{
    public class JzRectEAG : GeoFigure
    {
        /// <summary>
        /// Newly Added Class For This Figure
        /// </summary>
        class RectEAGClass
        {
            public PointF Center;
            public double Width;
            public double Height;
            public double Degree;
            
            string BackupString = "";

            public RectEAGClass()
            {
                Center = new PointF(200, 200);
                Width = 200;
                Height = 200;
                Degree = 0;
            }
            public RectEAGClass(PointF center,double w,double h,double degree)
            {
                Center = center;
                Width = w;
                Height = h;
                Degree = degree;
            }
            public void FromRectF(RectangleF rectF)
            {
                Center = new PointF(rectF.X + rectF.Width / 2, rectF.Y + rectF.Height / 2);
                Width = rectF.Width;
                Height = rectF.Height;
            }
            public void FromRectFDup(RectangleF rectF)
            {
                Center = new PointF(rectF.X + rectF.Width / 2, rectF.Y + rectF.Height / 2);
                Width = rectF.Width;
                Height = rectF.Height;
            }
            public void Move(float offsetx,float offsety)
            {
                Center.X += offsetx;
                Center.Y += offsety;
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
            public void SetRect(SizeF orgsize,float ratio)
            {
                RectangleF rectf = GetRectF();

                Width = orgsize.Width * ratio;
                Height = orgsize.Height * ratio;

                Center.X = rectf.X + (float)Width / 2f;
                Center.Y = rectf.Y + (float)Height / 2f;
            }
            public void SetAngle(double adddegree)
            {
                Degree += adddegree;
            }
            public override string ToString()
            {
                string str = "";

                str += Center.X.ToString() + ",";   //0
                str += Center.Y.ToString() + ",";   //1
                str += Width.ToString() + ",";      //2
                str += Height.ToString() + ",";     //3
                str += Degree.ToString() + ",";     //4
                
                return str;
            }
            public void FromString(string str)
            {
                string[] strs = str.Split(',');
                
                Center.X = float.Parse(strs[0]);
                Center.Y = float.Parse(strs[1]);
                Width = double.Parse(strs[2]);
                Height = double.Parse(strs[3]);
                Degree = double.Parse(strs[4]);
            }

            public RectangleF GetRectF()
            {
                RectangleF rectf = new RectangleF(Center.X - (float)Width / 2, Center.Y - (float)Height / 2, (float)Width, (float)Height);
                return rectf;
            }

        }

        #region PRIVATE_DATA
        int m_version = 701;
        //Form form;
        Control picImage;
        Mover supervisor;
        
        PointF [] pts = new PointF [4];
        SolidBrush brush;
        
        PointF ptEndIn, ptEndOut, ptOnOppositeSide;
        PointF [] ptsAllowed = new PointF [4];
        double compensation;            // only between MouseDown and MouseUp 

        const int minSide = 1;
        float radiusCorner = 1.5f;

        RectEAGClass RectEAG = new RectEAGClass();
        #endregion

        public RectangleF GetRectF
        {
            get
            {
                return RectEAG.GetRectF();
            }
        }
        public Rectangle GetRect
        {
            get
            {
                RectangleF rectf = GetRectF;

                return new Rectangle((int)rectf.X, (int)rectf.Y, (int)rectf.Width, (int)rectf.Height);
            }
        }
        public void SetRectRatio(SizeF orgsizef,float ratio)
        {
            RectEAG.SetRect(orgsizef, ratio);
        }

        // -------------------------------------------------                        //Modified By Victor Tsai
        public JzRectEAG (Control pic, Mover mvr, PointF ptC, double w, double h, double angleDegree, Color color, bool in_group)
        {
            picImage = pic;
            supervisor = mvr;
            m_figure = Figure_EAG .Rectangle;

            RectEAG = new RectEAGClass(ptC, w, h, angleDegree);

            brush = new SolidBrush (color);
            bInGroup = in_group;
        }
 
        public JzRectEAG(Control pic, Mover mvr, string str, Color color)    //Added By Victor Tsai
        {
            picImage = pic;
            supervisor = mvr;

            FromString(str);

            brush = new SolidBrush(color);
            bInGroup = false;
        }
        public JzRectEAG(string str, Color color)    //Added By Victor Tsai
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
        /// 首件
        /// </summary>
        /// <param name="color"></param>
        public JzRectEAG(Color color)                       //Added By Victor Tsai
        {
            RectEAG = new RectEAGClass();

            FromString(Figure_EAG.Rectangle.ToString() + ";" + RectEAG.ToString());

            brush = new SolidBrush(color);
            bInGroup = false;
        }
        public JzRectEAG(Color color,RectangleF rectf)                       //Added By Victor Tsai
        {
            RectEAG = new RectEAGClass();
            RectEAG.FromRectF(rectf);

            //Center = RectEAG.Center;
            
            FromString(Figure_EAG.Rectangle.ToString() + ";" + RectEAG.ToString());

            brush = new SolidBrush(color);
            bInGroup = false;
        }
        public JzRectEAG(Color color, RectangleF rectf,double angle)                       //Added By Victor Tsai
        {
            RectEAG = new RectEAGClass();
            RectEAG.FromRectF(rectf);
            RectEAG.SetAngle(angle);

            //Center = RectEAG.Center;

            FromString(Figure_EAG.Rectangle.ToString() + ";" + RectEAG.ToString());

            brush = new SolidBrush(color);
            bInGroup = false;
        }
        public JzRectEAG(Color color, RectangleF rectf,bool isdupe)                       //Added By Victor Tsai
        {
            RectEAG = new RectEAGClass();

            if (isdupe)
                RectEAG.FromRectFDup(rectf);
            else
                RectEAG.FromRectF(rectf);

            ShowMode = JzDisplay.ShowModeEnum.BORDERSHOW;

            FromString(Figure_EAG.Rectangle.ToString() + ";" + RectEAG.ToString());

            brush = new SolidBrush(color);
            bInGroup = false;
        }

        public JzRectEAG(Control pic, Mover mvr, JzRectEAG fromrectangle)    //Added By Victor Tsai
        {
            picImage = pic;
            supervisor = mvr;

            FromString(fromrectangle.ToString());
            RectEAG.Center.X += 100f;
            RectEAG.Center.Y += 100f;

            brush = new SolidBrush(fromrectangle.brush.Color);
            bInGroup = false;

            IsFirstSelected = fromrectangle.IsFirstSelected;
            IsSelected = fromrectangle.IsSelected;

            fromrectangle.IsFirstSelected = false;
            fromrectangle.IsSelected = false;
        }

        // -------------------------------------------------        Copy
        public override GeoFigure Copy (Control pic, Mover mvr, PointF pt)
        {
            JzRectEAG elem = new JzRectEAG (pic, mvr, pt, Width, Height, Auxi_Convert .RadianToDegree (m_angle), Color, InGroup);
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
            bool bRet = minSide <= coef * Math .Min (Width, Height);
            return (bRet);
        }
        // -------------------------------------------------        Zoom
        public override bool Zoom (double coef, bool bCheck)
        {
            coef = Math .Abs (coef);
            if (bCheck)
            {
                if (minSide > coef * Math .Min (Width, Height))
                {
                    return (false);
                }
            }
            m_center = Auxi_Geometry .Middle (pts [0], pts [2]);
            pts = Auxi_Geometry .ZoomPolygon (m_center, pts, coef);
            DefineCover ();
            return (true);
        }
        // -------------------------------------------------        SqueezeToMinimal
        public override void SqueezeToMinimal ()
        {
            Zoom (minSide / Math .Min (Width, Height), false);
        }
        // -------------------------------------------------        SqueezePossible
        public override bool SqueezePossible
        {
            get { return (minSide < Math .Min (Width, Height)); }
        }
        // -------------------------------------------------        Radius
        private double Radius
        {
            get { return (Auxi_Geometry .Distance (pts [0], pts [2]) / 2); }
        }
        // -------------------------------------------------        Vertices
        private PointF [] Vertices
        {
            get { return (pts); }
        }
        // -------------------------------------------------        Width
        public double Width
        {
            get { return (Auxi_Geometry .Distance (pts [0], pts [1])); }
        }
        // -------------------------------------------------        Height
        public double Height
        {
            get { return (Auxi_Geometry .Distance (pts [0], pts [3])); }
        }
        // -------------------------------------------------        MinimumSide
        static public int MinimumSide
        {
            get { return (minSide); }
        }
        // -------------------------------------------------        RectAround
        public override RectangleF RectAround
        {
            get { return (Auxi_Geometry .RectangleAroundPoints (pts)); }
        }
        // -------------------------------------------------        Angle
        public override double Angle
        {
            get { return (m_angle); }
            set
            {
                m_angle = Auxi_Common .LimitedRadian (value);
                double angle_plus = Math .Atan2 (Height, Width);
                m_center = Auxi_Geometry .Middle (pts [0], pts [2]);
                double radius = Radius;
                pts = new PointF [4] {Auxi_Geometry .PointToPoint (m_center, m_angle + angle_plus, radius), 
                                      Auxi_Geometry .PointToPoint (m_center, m_angle - angle_plus + Math .PI, radius),
                                      Auxi_Geometry .PointToPoint (m_center, m_angle + angle_plus + Math .PI, radius), 
                                      Auxi_Geometry .PointToPoint (m_center, m_angle - angle_plus, radius) };
                DefineCover ();
            }
        }
        // -------------------------------------------------        Color
        public override Color Color
        {
            get { return (brush .Color); }
            set { brush .Color = value; }
        }
        // -------------------------------------------------        StartRotation
        public override void StartRotation (Point ptMouse)
        {
            m_center = Auxi_Geometry .Middle (pts [0], pts [2]);
            double angleMouse = Auxi_Geometry .Line_Angle (m_center, ptMouse);
            compensation = Auxi_Common .LimitedRadian (angleMouse - m_angle);
        }
        // -------------------------------------------------        StartResizing
        public void StartResizing (Point ptMouse, int iNode)
        {
            if (iNode == 8)
            {
                return;
            }
            PointF ptNearest;
            if (iNode < 4)      // corners
            {
                ptNearest = pts [iNode];
                int iBase = (iNode + 2) % 4;
                PointF ptBase = pts [iBase];
                double angleToNext = Auxi_Geometry .Line_Angle (pts [iBase], pts [(iBase + 1) % 4]);
                ptsAllowed [0] = Auxi_Geometry .PointToPoint (ptBase, angleToNext + Math .PI / 4, minSide * Math .Sqrt (2));
                ptsAllowed [1] = Auxi_Geometry .PointToPoint (ptsAllowed [0], angleToNext, 5000);
                ptsAllowed [2] = Auxi_Geometry .PointToPoint (ptsAllowed [0], angleToNext + Math .PI / 4, 7000);
                ptsAllowed [3] = Auxi_Geometry .PointToPoint (ptsAllowed [0], angleToNext + Math .PI / 2, 5000);
            }
            else                // sides
            {
                ptNearest = Auxi_Geometry .NearestPointOnSegment (ptMouse, pts [iNode - 4], pts [(iNode - 3) % 4]);
                ptOnOppositeSide = Auxi_Geometry .NearestPointOnSegment (ptMouse, pts [(iNode + 2) % 4], pts [(iNode + 3) % 4]);
                double angleBeam = Auxi_Geometry .Line_Angle (ptOnOppositeSide, ptMouse);
                ptEndIn = Auxi_Geometry .PointToPoint (ptOnOppositeSide, angleBeam, minSide);
                ptEndOut = Auxi_Geometry .PointToPoint (ptOnOppositeSide, angleBeam, 5000);
            }
            AdjustCursorPosition (ptNearest);
        }
        // -------------------------------------------------        AdjustCursorPosition
        private void AdjustCursorPosition(PointF pt)
        {
            supervisor.MouseTraced = false;
            Cursor.Position = picImage.PointToScreen(Point.Round(pt));
            supervisor.MouseTraced = true;
        }
        // -------------------------------------------------        DefineCover
        // order of nodes:      4       in the corners
        //                      4       on sides
        //                      1       full area
        //
        public override void DefineCover ()
        {
            CoverNode [] nodes = new CoverNode [9];
            //if (bResize)
            {
                for (int i = 0; i < 4; i++)
                {
                    nodes [i] = new CoverNode (i, pts [i], radiusCorner * 2);

                    if(!bResize)
                        nodes[i].SetBehaviourCursor(Behaviour.Moveable, Cursors.SizeAll);
                }
                for (int i = 0; i < 4; i++)
                {
                    nodes[i + 4] = new CoverNode(i + 4, pts[i], pts[(i + 1) % 4], radiusCorner * 2 - 1);
                    nodes[i + 4].SetBehaviourCursor(Behaviour.Moveable, Cursors.SizeAll);             //Disable Border Behaviour
                }
            }
            //else
            //{
            //    m_center = Auxi_Geometry .Middle (pts [0], pts [2]);
            //    for (int i = 0; i < 8; i++)
            //    {
            //        nodes [i] = new CoverNode (i, m_center, 0.0, Cursors .SizeAll);
            //    }
            //}

            nodes[8] = new CoverNode(8, pts);
            nodes[8].SetBehaviourCursor(Behaviour.Transparent, Cursors.Default);

            cover = new Cover (nodes);

            if (TransparentForMover)
                cover.SetBehaviour(Behaviour.Transparent);
        }
        // -------------------------------------------------        Move
        public override void Move (int dx, int dy)
        {
            SizeF size = new SizeF (dx, dy);
            m_center += size;
            for (int i = 0; i < 4; i++)
            {
                pts [i] += size;
            }
        }
        public override void Move(float dx, float dy)
        {
            SizeF size = new SizeF(dx, dy);
            m_center += size;
            for (int i = 0; i < 4; i++)
            {
                pts[i] += size;
            }
        }
        // -------------------------------------------------        MoveNode
        public override bool MoveNode (int iNode, int dx, int dy, Point ptM, MouseButtons catcher)
        {
            bool bRet = false;
            double angleMouse;
            if (catcher == MouseButtons .Left)
            {
                if (iNode < 4 && bResize)      // corners
                {
                    PointF ptNext, ptPrev;
                    if (Auxi_Geometry .PointInsideConvexPolygon (ptM, ptsAllowed))
                    {
                        pts [iNode] = ptM;
                        Auxi_Geometry .Distance_PointLine (ptM, pts [(iNode + 1) % 4], pts [(iNode + 2) % 4], out ptNext);
                        Auxi_Geometry .Distance_PointLine (ptM, pts [(iNode + 2) % 4], pts [(iNode + 3) % 4], out ptPrev);
                        pts [(iNode + 1) % 4] = ptNext;
                        pts [(iNode + 3) % 4] = ptPrev;
                        DefineCover ();
                        bRet = true;
                    }
                    else
                    {
                        AdjustCursorPosition (pts [iNode]);
                        bRet = false;
                    }
                }
                //else if (iNode < 8)   // sides
                //{
                //    double newW, newH;
                //    PointF ptNearest = Auxi_Geometry .NearestPointOnSegment (ptM, ptEndIn, ptEndOut);
                //    if (iNode == 4)
                //    {
                //        newH = Auxi_Geometry .Distance (ptNearest, ptOnOppositeSide);
                //        pts [0] = Auxi_Geometry .PointToPoint (pts [3], m_angle + Math .PI / 2, newH);
                //        pts [1] = Auxi_Geometry .PointToPoint (pts [2], m_angle + Math .PI / 2, newH);
                //    }
                //    else if (iNode == 5)
                //    {
                //        newW = Auxi_Geometry .Distance (ptNearest, ptOnOppositeSide);
                //        pts [1] = Auxi_Geometry .PointToPoint (pts [0], m_angle + Math .PI, newW);
                //        pts [2] = Auxi_Geometry .PointToPoint (pts [3], m_angle + Math .PI, newW);
                //    }
                //    else if (iNode == 6)
                //    {
                //        newH = Auxi_Geometry .Distance (ptNearest, ptOnOppositeSide);
                //        pts [2] = Auxi_Geometry .PointToPoint (pts [1], m_angle - Math .PI / 2, newH);
                //        pts [3] = Auxi_Geometry .PointToPoint (pts [0], m_angle - Math .PI / 2, newH);
                //    }
                //    else // iNode == 7
                //    {
                //        newW = Auxi_Geometry .Distance (ptNearest, ptOnOppositeSide);
                //        pts [0] = Auxi_Geometry .PointToPoint (pts [1], m_angle, newW);
                //        pts [3] = Auxi_Geometry .PointToPoint (pts [2], m_angle, newW);
                //    }
                //    DefineCover ();
                //    bRet = true;
                //    AdjustCursorPosition (ptNearest);
                //}
                else if(iNode < 8)   // sides
                {
                    Move (dx, dy);
                    bRet = true;
                }
            }
            else if (catcher == MouseButtons .Right && bRotate)
            {
                angleMouse = Auxi_Geometry .Line_Angle (m_center, ptM);
                m_angle = angleMouse - compensation;
                double radius = Radius;
                double w = Auxi_Geometry .Distance (pts [0], pts [1]);
                double h = Auxi_Geometry .Distance (pts [0], pts [3]);
                double angle_plus = Math .Atan2 (h, w);
                pts = new PointF [4] {Auxi_Geometry .PointToPoint (m_center, m_angle + angle_plus, radius), 
                                      Auxi_Geometry .PointToPoint (m_center, m_angle - angle_plus + Math .PI, radius),
                                      Auxi_Geometry .PointToPoint (m_center, m_angle + angle_plus + Math .PI, radius), 
                                      Auxi_Geometry .PointToPoint (m_center, m_angle - angle_plus, radius) };
                DefineCover ();
                bRet = true;
            }
            return (bRet);
        }

        const string nameMain = "Rectangle_EAG_";
        // -------------------------------------------------        IntoRegistry
        public override void IntoRegistry (RegistryKey regkey, string strAdd)
        {
            try
            {
                m_center = Auxi_Geometry .Middle (pts [0], pts [2]);
                regkey .SetValue (nameMain + strAdd, new string [] {m_version .ToString (),   // 0
                                                                    m_center .X .ToString (),   // 1
                                                                    m_center .Y .ToString (),   // 2
                                                                    Width .ToString (),         // 3  
                                                                    Height .ToString (),        // 4   
                                                                    Auxi_Convert .RadianToDegree (Angle) .ToString (),  // 5     
                                                                    ((int) (Color .A)) .ToString (),    // 6
                                                                    ((int) (Color .R)) .ToString (),    // 7
                                                                    ((int) (Color .G)) .ToString (),    // 8
                                                                    ((int) (Color .B)) .ToString (),    // 9
                                                                    bInGroup .ToString (),  // 10
                                                                    bResize .ToString (),   // 11
                                                                    bRotate .ToString (),   // 12 
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
        public static JzRectEAG FromRegistry (Control pic, Mover mvr, RegistryKey regkey, string strAdd)
        {
            try
            {
                string [] strs = (string []) regkey .GetValue (nameMain + strAdd);
                if (strs == null || strs .Length < 15 || Convert .ToInt32 (strs [0]) != 701)
                {
                    return (null);
                }
                JzRectEAG rc_EAG = new JzRectEAG (pic, mvr,
                                                          Auxi_Convert .ToPointF (strs, 1),     // ptC
                                                          Convert .ToDouble (strs [3]),         // w
                                                          Convert .ToDouble (strs [4]),         // h
                                                          Convert .ToDouble (strs [5]),         // angleDegree
                                                          Auxi_Convert .ToColor (strs, 6),      // color
                                                          Convert .ToBoolean (strs [10]));      // in_group
                if (rc_EAG != null)
                {
                    rc_EAG .Resizable = Convert .ToBoolean (strs [11]);
                    rc_EAG .Rotatable = Convert .ToBoolean (strs [12]);
                    rc_EAG .Visible = Convert .ToBoolean (strs [13]);
                    rc_EAG .VisibleAsMember = Convert .ToBoolean (strs [14]);
                }
                return (rc_EAG);
            }
            catch
            {
                return (null);
            }
            finally
            {
            }
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

            BorderPen.Width = PenWidth;

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
                    BorderPen.DashStyle = DashStyle.Solid;
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
                grfx.FillPolygon(brush, pts);

                if (bResize)
                {
                    //if (InGroup)
                    //{
                    //    grfx.DrawPolygon(penResize_InGroup, pts);
                    //}
                    //else
                    //{
                    //if (ShowMode != ShowModeEnum.BORDERSHOW)
                    //    grfx.DrawPolygon(penUnder_Independent, pts);        //先畫一個黑底

                    grfx.DrawPolygon(BorderPen, pts);                   //再畫一個格子
                    //}
                }
                else
                {
                    BorderPen.DashStyle = DashStyle.Dash;

                    grfx.DrawPolygon(BorderPen, pts);
                }

                SolidBrush myBorderBrush = new SolidBrush(BorderColor);

                if (ShowMode != ShowModeEnum.MAINSHOW && bResize)
                {
                    if (!NoShowCorner)
                    {
                        int i = 0;
                        foreach (PointF ptf in pts)
                        {
                            //if (i == 0)
                            //    grfx.FillRectangle(myBorderBrush, ptf.X - radiusCorner, ptf.Y - radiusCorner, radiusCorner * 2, radiusCorner * 2);
                            //else if (i == 1)
                            //    grfx.FillRectangle(myBorderBrush, ptf.X - radiusCorner, ptf.Y - radiusCorner, radiusCorner * 2, radiusCorner * 2);
                            //else if (i == 2)
                            //    grfx.FillRectangle(myBorderBrush, ptf.X - radiusCorner, ptf.Y - radiusCorner, radiusCorner * 2, radiusCorner * 2);
                            //else if (i == 3)
                            //    grfx.FillRectangle(myBorderBrush, ptf.X - radiusCorner, ptf.Y - radiusCorner, radiusCorner * 2, radiusCorner * 2);
                            grfx.FillRectangle(new SolidBrush(Color.Black), ptf.X - radiusCorner * 2, ptf.Y - radiusCorner * 2, radiusCorner * 4, radiusCorner * 4);
                            grfx.FillRectangle(myBorderBrush, ptf.X - radiusCorner, ptf.Y - radiusCorner, radiusCorner * 2, radiusCorner * 2);

                            i++;
                        }
                    }
                }

                if (bRotate)
                {
                    float r = 1.5f;
                    m_center = Auxi_Geometry.Middle(pts[0], pts[2]);
                    //grfx.FillEllipse(new SolidBrush(Color.Black), RectangleF.FromLTRB(m_center.X - r * 2, m_center.Y - r * 2, m_center.X + r * 2, m_center.Y + r * 2));
                    grfx.FillEllipse(brushAnchor, RectangleF.FromLTRB(m_center.X - r, m_center.Y - r, m_center.X + r, m_center.Y + r));
                }

                //grfx.DrawString(RelateNo.ToString("000") + "," + RelatePosition.ToString("000"), new Font("Arial", 12), new SolidBrush(Color.Red), m_center);
                if (!string.IsNullOrEmpty(Desc))
                {
                    grfx.DrawString(Desc, new Font("Arial", FontSize), new SolidBrush(BorderPen.Color), m_center);
                }
            }
        }
        public override void SetOffset(Point offsetpoint)
        {
            RectEAG.SetOffset(offsetpoint);
        }
        public override void SetAngle(double addangle)
        {
            RectEAG.SetAngle(addangle);
        }
        public override void FromRectangleF(RectangleF rectf)
        {
            RectEAG.FromRectF(rectf);
        }
        public override void DrawMask(Graphics grfx, PointF ptfoffset,float degree, SolidBrush backsolid, SolidBrush fillsolid,Size bmpsize)
        {
            string backupstring = ToShapeString();

            RectEAGClass RectEAGBACK = new RectEAGClass();
            RectEAGBACK.FromString(RectEAG.ToString());

            RectEAG.Degree += degree;
            //RectEAG.Move(-ptfoffset.X, -ptfoffset.Y);

            MappingToMovingObject(new PointF(0, 0), 1d);
            Move(-ptfoffset.X, -ptfoffset.Y);

            if (backsolid != null)
                grfx.FillRectangle(backsolid, new RectangleF(0, 0, bmpsize.Width, bmpsize.Height));

            grfx.FillPolygon(fillsolid, pts);

            RectEAG.FromString(RectEAGBACK.ToString());
            FromShapeString(backupstring);
        }
        public override void Draw(Bitmap bmp, PointF ptfoffset,float degree, SolidBrush backsolid, SolidBrush fillsolid)
        {
            Graphics grfx = Graphics.FromImage(bmp);

            DrawMask(grfx, ptfoffset, degree, backsolid, fillsolid, bmp.Size);

            //string backupstring = ToShapeString();

            //MappingToMovingObject(new PointF(0, 0), 1d);
            //Move(-ptfoffset.X, -ptfoffset.Y);

            //if(backsolid != null)
            //    grfx.FillRectangle(backsolid, new RectangleF(0, 0, bmp.Width, bmp.Height));

            //grfx.FillPolygon(fillsolid, pts);

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
        public override string ToString()
        {
            string retstr = "";

            retstr += m_figure.ToString() + ";";
            retstr += RectEAG.ToString();

            return retstr;

        }
        public override void FromString(string str)
        {
            string[] strs = str.Split(';');

            m_figure = (Figure_EAG)Enum.Parse(typeof(Figure_EAG), strs[0]);
            RectEAG.FromString(strs[1]);
        }
        public override string ToShapeString()
        {
            string retstr = "";

            retstr += m_center.X.ToString() + ",";  //0
            retstr += m_center.Y.ToString() + ",";  //1
            retstr += m_angle.ToString() + ",";     //2
            retstr += pts[0].X.ToString() + ",";    //3
            retstr += pts[0].Y.ToString() + ",";    //4
            retstr += pts[1].X.ToString() + ",";    //5
            retstr += pts[1].Y.ToString() + ",";    //6
            retstr += pts[2].X.ToString() + ",";    //7
            retstr += pts[2].Y.ToString() + ",";    //8
            retstr += pts[3].X.ToString() + ",";    //9
            retstr += pts[3].Y.ToString() + ",";    //10

            return retstr;
        }
        public override void FromShapeString(string str)
        {
            string[] strs = str.Split(',');

            m_center.X = float.Parse(strs[0]);
            m_center.Y = float.Parse(strs[1]);

            m_angle = double.Parse(strs[2]);

            pts[0].X = float.Parse(strs[3]);
            pts[0].Y = float.Parse(strs[4]);
            pts[1].X = float.Parse(strs[5]);
            pts[1].Y = float.Parse(strs[6]);
            pts[2].X = float.Parse(strs[7]);
            pts[2].Y = float.Parse(strs[8]);
            pts[3].X = float.Parse(strs[9]);
            pts[3].Y = float.Parse(strs[10]);

        }
        public override void Backup()
        {
            RectEAG.Backup();
        }
        public override void Restore()
        {
            RectEAG.Restore();
        }

        public override void MappingToMovingObject(PointF bias, double ratio)
        {
            m_center.X = (float)((double)bias.X + ((double)RectEAG.Center.X * ratio));
            m_center.Y = (float)((double)bias.Y + ((double)RectEAG.Center.Y * ratio));

            double w = Math.Max(minSide, Math.Abs(RectEAG.Width * ratio));
            double h = Math.Max(minSide, Math.Abs(RectEAG.Height * ratio));

            m_angle = Auxi_Convert.DegreeToRadian(RectEAG.Degree);
            double radius = Math.Sqrt(w * w + h * h) / 2;
            double angle_plus = Math.Atan2(h, w);

            pts = new PointF[4] {Auxi_Geometry .PointToPoint (m_center, m_angle + angle_plus, radius),
                                  Auxi_Geometry .PointToPoint (m_center, m_angle - angle_plus + Math .PI, radius),
                                  Auxi_Geometry .PointToPoint (m_center, m_angle + angle_plus + Math .PI, radius),
                                  Auxi_Geometry .PointToPoint (m_center, m_angle - angle_plus, radius) };

            Move(OffsetPoint.X, OffsetPoint.Y);
            OffsetPoint = new Point(0, 0);

            //MappingFromMovingObject(new PointF(0, 0), 1);
        }
        public override void MappingToMovingObject(PointF bias, SizeF sizeratio)
        {
            m_center.X = (float)((double)bias.X + ((double)RectEAG.Center.X * sizeratio.Width));
            m_center.Y = (float)((double)bias.Y + ((double)RectEAG.Center.Y * sizeratio.Height));

            double w = Math.Max(minSide, Math.Abs(RectEAG.Width * sizeratio.Width));
            double h = Math.Max(minSide, Math.Abs(RectEAG.Height * sizeratio.Height));

            m_angle = Auxi_Convert.DegreeToRadian(RectEAG.Degree);
            double radius = Math.Sqrt(w * w + h * h) / 2;
            double angle_plus = Math.Atan2(h, w);

            pts = new PointF[4] {Auxi_Geometry .PointToPoint (m_center, m_angle + angle_plus, radius),
                                  Auxi_Geometry .PointToPoint (m_center, m_angle - angle_plus + Math .PI, radius),
                                  Auxi_Geometry .PointToPoint (m_center, m_angle + angle_plus + Math .PI, radius),
                                  Auxi_Geometry .PointToPoint (m_center, m_angle - angle_plus, radius) };

            Move(OffsetPoint.X, OffsetPoint.Y);
            OffsetPoint = new Point(0, 0);

            MappingFromMovingObject(new PointF(0, 0), 1);
        }
        public override void MappingFromMovingObject(PointF bias, double ratio)
        {
            RectEAG.Center.X = (float)(((double)m_center.X - (double)bias.X) / ratio);
            RectEAG.Center.Y = (float)(((double)m_center.Y - (double)bias.Y) / ratio);

            RectEAG.Width = Width / ratio;
            RectEAG.Height = Height / ratio;

            RectEAG.Degree = Auxi_Convert.RadianToDegree(m_angle);
        }
    }
}
