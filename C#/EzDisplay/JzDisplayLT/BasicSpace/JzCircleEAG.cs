using JzDisplay;
using Microsoft.Win32;
using MoveGraphLibrary;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace WorldOfMoveableObjects
{
    public class JzCircleEAG : GeoFigure
    {
        /// <summary>
        /// Newly Added Class For This Figure
        /// </summary>
        class CircleEAGClass
        {
            public PointF Center;
            public float Radius;
            public double Degree;

            string BackupString = "";
            public CircleEAGClass()
            {
                Center = new PointF(200,200);
                Radius = 100;
                Degree = 0;
            }
            public CircleEAGClass(PointF center, float radius, double degree)
            {
                Center = center;
                Radius = radius;
                Degree = degree;
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
            public void FromRectF(RectangleF rectF)
            {
                Center = new PointF(rectF.X + rectF.Width / 2, rectF.Y + rectF.Height / 2);
                Radius = Math.Min(rectF.Width / 2, rectF.Height / 2);
                Degree = 0;
            }

            public override string ToString()
            {
                string str = "";

                str += Center.X.ToString() + ",";   //0
                str += Center.Y.ToString() + ",";   //1
                str += Radius.ToString() + ",";     //2
                str += Degree.ToString() + ",";     //3

                return str;
            }
            public void FromString(string str)
            {
                string[] strs = str.Split(',');

                Center.X = float.Parse(strs[0]);
                Center.Y = float.Parse(strs[1]);
                Radius = float.Parse(strs[2]);
                Degree = double.Parse(strs[3]);
            }
        }

        int m_version = 701;
        //Form form;
        Control picImage;
        Mover supervisor;

        float m_radius;

        double [] vals;
        double [] sector_angle;
        List<Color> clrs = new List<Color> ();          // one color per each sector
        Rotation dirDrawing;

        bool bFixSectors;
        Pen penPartition;

        double angleBeam, compensation;
        PointF ptEndIn, ptEndOut; 

        // all these are used only for moving the border between two sectors 
        int iBorderToMove;
        int iSector_Counterclock, iSector_Clockwise;
        double min_angle_Resectoring,       // clockwise from the moving border
               max_angle_Resectoring;       // counterclock from the moving border
        double two_sectors_sum_values;

        float delta = 1.5f;
        static float minRadius = 0.02f;   // min size is set to avoid disappearance while resizing

        CircleEAGClass CircleEAG = new CircleEAGClass();

        // -------------------------------------------------
        public JzCircleEAG (Control pic, Mover mvr, PointF ptC, float rad, double angleDegree, double [] fVals, bool in_group) 
        {
            picImage = pic;
            supervisor = mvr;
            m_figure = Figure_EAG .Circle;

            CircleEAG = new CircleEAGClass(ptC, rad, angleDegree);

            //m_center = ptC;
            //m_radius = Math .Max (rad, minRadius);
            //m_angle = Auxi_Convert .DegreeToRadian (angleDegree);
            //dirDrawing = Rotation .Clockwise;

            bFixSectors = true;                                     // FixSector �N�O�������u By Victor Tsai 2017/05/11
            penPartition = new Pen(Color.FromArgb(0, Color.Red), 3);

            CheckedValues (fVals);
            DefaultColors ();
            bInGroup = in_group;
        }
        // -------------------------------------------------
        public JzCircleEAG (Control pic, Mover mvr, PointF ptC, float rad, Color clr, bool in_group)
            : this (pic, mvr, ptC, rad, 0, new double [] { 5 }, in_group)
        {
            clrs [0] = clr;

        }
        // -------------------------------------------------
        public JzCircleEAG (Control pic, Mover mvr, PointF ptC, float rad, double fSum, Color [] clrs, bool in_group)
            : this (pic, mvr, ptC, rad, 0, Auxi_Common .ArrayOfEqualValues (clrs .Length, fSum / clrs .Length), in_group)
        {
            for (int i = 0; i < clrs .Length; i++)
            {
                SetSectorColor (i, clrs [i]);
            }
        }
        public JzCircleEAG(Control pic, Mover mvr, string str, Color color)    //Added By Victor Tsai
        {
            picImage = pic;
            supervisor = mvr;

            FromString(str);

            bFixSectors = true;                                                 // FixSector �N�O�������u By Victor Tsai 2017/05/11
            penPartition = new Pen(Color.FromArgb(0, Color.Red), 3);
            CheckedValues(new double[] { 5 });
            DefaultColors();

            clrs[0] = color;
            bInGroup = false;
        }
        public JzCircleEAG(string str, Color color)    //Added By Victor Tsai
        {
            FromString(str);

            bFixSectors = true;                                                 // FixSector �N�O�������u By Victor Tsai 2017/05/11
            penPartition = new Pen(Color.FromArgb(0, Color.Red), 3);
            CheckedValues(new double[] { 5 });
            DefaultColors();

            clrs[0] = color;
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
        public JzCircleEAG(Color color)                       //Added By Victor Tsai
        {
            CircleEAG = new CircleEAGClass();

            FromString(Figure_EAG.Circle.ToString() + ";" + CircleEAG.ToString());

            clrs[0] = color;
            bInGroup = false;
        }
        public JzCircleEAG(Color color,RectangleF rectf)                       //Added By Victor Tsai
        {
            CircleEAG = new CircleEAGClass();
            CircleEAG.FromRectF(rectf);

            FromString(Figure_EAG.Circle.ToString() + ";" + CircleEAG.ToString());

            bFixSectors = true;                                                 // FixSector �N�O�������u By Victor Tsai 2017/05/11
            penPartition = new Pen(Color.FromArgb(0, Color.Red), 3);
            CheckedValues(new double[] { 5 });
            DefaultColors();

            clrs[0] = color;
            bInGroup = false;
        }
        public JzCircleEAG(Control pic, Mover mvr, JzCircleEAG fromrcircle)    //Added By Victor Tsai
        {
            picImage = pic;
            supervisor = mvr;

            FromString(fromrcircle.ToString());
            CircleEAG.Center.X += 100f;
            CircleEAG.Center.Y += 100f;

            bFixSectors = true;                                                 // FixSector �N�O�������u By Victor Tsai 2017/05/11
            penPartition = new Pen(Color.FromArgb(0, Color.Red), 3);
            CheckedValues(new double[] { 5 });
            DefaultColors();

            clrs[0] = fromrcircle.clrs[0];
            bInGroup = false;

            IsFirstSelected = fromrcircle.IsFirstSelected;
            IsSelected = fromrcircle.IsSelected;

            fromrcircle.IsFirstSelected = false;
            fromrcircle.IsSelected = false;
        }

        // -------------------------------------------------        CheckedValues
        private void CheckedValues (double [] fVals)
        {
            bool bCorrect = true;
            if (fVals == null || fVals .Length < 1 || Auxi_Common .SumArray (fVals, true) == 0.0)
            {
                bCorrect = false;
            }
            else
            {
                foreach (double val in fVals)
                {
                    if (val <= 0.0)
                    {
                        bCorrect = false;
                        break;
                    }
                }
            }
            if (!bCorrect)
            {
                vals = new double [] { 1, 2, 3, 4 };
            }
            else
            {
                vals = new double [fVals .Length];
                for (int i = 0; i < vals .Length; i++)
                {
                    vals [i] = fVals [i];
                }
            }
            sector_angle = new double [vals .Length];
            SectorAngles ();
        }
        // -------------------------------------------------        SectorAngles
        private void SectorAngles ()
        {
            double fSum = Auxi_Common .SumArray (vals, false);
            for (int i = 0; i < vals .Length; i++)
            {
                sector_angle [i] = 2 * Math .PI * vals [i] / fSum;
            }
            if (dirDrawing == Rotation .Clockwise)
            {
                for (int i = 0; i < vals .Length; i++)
                {
                    sector_angle [i] *= -1;
                }
            }
        }
        // -------------------------------------------------        DefaultColors
        public void DefaultColors ()
        {
            clrs .Clear ();
            for (int i = 0; i < vals .Length; i++)
            {
                clrs .Add (Auxi_Colours .ColorPredefined (i + 1));
            }
        }
        // -------------------------------------------------        Copy
        public override GeoFigure Copy (Control pic, Mover mvr, PointF pt)
        {
            JzCircleEAG elem = new JzCircleEAG (pic, mvr, pt, m_radius, Auxi_Convert .RadianToDegree (m_angle), vals, InGroup);
            elem .SetColors (clrs);
            elem .Resizable = bResize;
            elem .Rotatable = bRotate;
            elem .FixSectors = bFixSectors;
            elem .Visible = Visible;
            elem .VisibleAsMember = VisibleAsMember;
            return ((GeoFigure) elem);
        }
        // -------------------------------------------------        ZoomAllowed
        public override bool ZoomAllowed (double coef)
        {
            coef = Math .Abs (coef);
            return (minRadius <= coef * m_radius);
        }
        // -------------------------------------------------        Zoom
        public override bool Zoom (double coef, bool bCheck)
        {
            coef = Math .Abs (coef);
            if (bCheck)
            {
                if (minRadius > coef * m_radius)
                {
                    return (false);
                }
            }
            m_radius = Convert .ToSingle (m_radius * coef);
            DefineCover ();
            return (true);
        }
        // -------------------------------------------------        SqueezeToMinimal
        public override void SqueezeToMinimal ()
        {
            Zoom (minRadius / m_radius, false);
        }
        // -------------------------------------------------        SqueezePossible
        public override bool SqueezePossible
        {
            get { return (minRadius < m_radius); }
        }
        // -------------------------------------------------        Radius
        public double Radius
        {
            get { return (m_radius); }
            set
            {
                m_radius = Convert .ToSingle (Math .Max (value, MinimumRadius));
                DefineCover ();
            }
        }
        // -------------------------------------------------        MinimumRadius
        static public float MinimumRadius
        {
            get { return (minRadius); }
        }
        // -------------------------------------------------        RectAround
        public override RectangleF RectAround
        {
            get
            {
                float f_rad = Convert .ToSingle (m_radius);
                return (new RectangleF (m_center .X - f_rad, m_center .Y - f_rad, 2 * f_rad, 2 * f_rad));
            }
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
            get { return (clrs [0]); }
            set { clrs [0] = value; }
        }
        // -------------------------------------------------        Colors
        public List<Color> Colors
        {
            get { return (clrs); }
        }
        // -------------------------------------------------        SetSectorColor
        public void SetSectorColor (int iSector, Color color)
        {
            clrs [Math .Min (Math .Max (0, iSector), clrs .Count - 1)] = color;
        }
        // -------------------------------------------------        SetColors
        public void SetColors (List<Color> colors)
        {
            if (colors != null)
            {
                for (int i = 0; i < Math .Min (clrs .Count, colors .Count); i++)
                {
                    clrs [i] = colors [i];
                }
            }
        }
        // -------------------------------------------------        DrawingDirection
        public Rotation DrawingDirection
        {
            get { return (dirDrawing); }
            set
            {
                dirDrawing = value;
                SectorAngles ();
            }
        }
        // -------------------------------------------------        ChangeDrawingDirection
        public void ChangeDrawingDirection ()
        {
            dirDrawing = (dirDrawing == Rotation .Clockwise) ? Rotation .Counterclock : Rotation .Clockwise;
            SectorAngles ();
        }
        // -------------------------------------------------        FixSectors
        public bool FixSectors
        {
            get { return (bFixSectors); }
            set
            {
                bFixSectors = value;
                DefineCover ();
            }
        }
        // -------------------------------------------------        SectorsFixingSwitch
        public void SectorsFixingSwitch ()
        {
            bFixSectors = !bFixSectors;
            DefineCover ();
        }
        // -------------------------------------------------        Values
        public double [] Values
        {
            get { return (vals); }
        }
        
        // -------------------------------------------------        StartResizing
        public void StartResizing (Point ptMouse)
        {
            angleBeam = Auxi_Geometry .Line_Angle (m_center, ptMouse);
            AdjustCursorPosition (Auxi_Geometry .PointToPoint (m_center, angleBeam, m_radius));

            ptEndIn = Auxi_Geometry .PointToPoint (m_center, angleBeam, MinimumRadius);
            ptEndOut = Auxi_Geometry .PointToPoint (m_center, angleBeam, 4000);
        }
        // -------------------------------------------------        StartResectoring
        public void StartResectoring (int iNode)
        {
            iBorderToMove = iNode;      // -nNodesOnCircle;
            double angleCaughtBorder = m_angle;
            for (int i = 0; i < iBorderToMove; i++)
            {
                angleCaughtBorder += sector_angle [i];
            }
            if (dirDrawing == Rotation .Clockwise)
            {
                iSector_Clockwise = iBorderToMove;
                min_angle_Resectoring = angleCaughtBorder + sector_angle [iSector_Clockwise];
                iSector_Counterclock = (iSector_Clockwise == 0) ? (vals .Length - 1) : (iSector_Clockwise - 1);
                max_angle_Resectoring = angleCaughtBorder - sector_angle [iSector_Counterclock];
            }
            else
            {
                iSector_Counterclock = iBorderToMove;
                max_angle_Resectoring = angleCaughtBorder + sector_angle [iSector_Counterclock];
                iSector_Clockwise = (iSector_Counterclock == 0) ? (vals .Length - 1) : (iSector_Counterclock - 1);
                min_angle_Resectoring = angleCaughtBorder - sector_angle [iSector_Clockwise];
            }
            two_sectors_sum_values = vals [iSector_Clockwise] + vals [iSector_Counterclock];
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
            int nStrips = vals .Length; 
            CoverNode [] nodes = new CoverNode [nStrips + 2];
            if (bFixSectors)
            {
                for (int i = 0; i < nStrips; i++)
                {
                    //nodes [i] = new CoverNode (i, m_center, 0.0, Cursors .SizeAll);
                    nodes[i] = new CoverNode(i, new PointF(m_center.X + m_radius, m_center.Y), 10.0f);
                    //nodes[i].SetBehaviourCursor(Behaviour.Transparent, Cursors.Default);
                }
            }
            //else
            //{
            //    double angleForLine = m_angle;
            //    for (int i = 0; i < nStrips; i++)                         // nodes on borders between sectors
            //    {
            //        nodes [i] = new CoverNode (i, m_center, Auxi_Geometry .PointToPoint (m_center, angleForLine, m_radius));
            //        angleForLine += sector_angle [i];
            //    }
            //}
            if (bResize)
            {
                nodes [nStrips] = new CoverNode (nStrips, m_center, m_radius - delta, Cursors .SizeAll);
                nodes[nStrips].SetBehaviourCursor(Behaviour.Transparent, Cursors.Default);

                nodes[nStrips + 1] = new CoverNode(nStrips + 1, m_center, m_radius + delta, Cursors.SizeAll);
                
            }
            //else
            //{
            //    nodes [nStrips]     = new CoverNode (nStrips, m_center, m_radius, Cursors .SizeAll);
            //    nodes [nStrips + 1] = new CoverNode (nStrips + 1, m_center, 0.0, Cursors .SizeAll);
            //}

            cover = new Cover (nodes);
            cover .SetClearance (false);

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
                if (iNode == cover .NodesCount - 3)
                {
                    PointF ptNearest = Auxi_Geometry .NearestPointOnSegment (ptM, ptEndIn, ptEndOut);
                    Radius = Convert .ToSingle (Auxi_Geometry .Distance (m_center, ptNearest));
                    AdjustCursorPosition (ptNearest);
                    bRet = true;
                }
                else if (iNode == cover.NodesCount - 1)
                {
                    Move(dx, dy);
                }
                else if (iNode == cover.NodesCount - 2)
                {
                    // border between two sectors
                    double angleMouse = Auxi_Geometry .Line_Angle (m_center, ptM);
                    if (angleMouse > max_angle_Resectoring)
                    {
                        angleMouse -= 2 * Math .PI;
                    }
                    else if (angleMouse < min_angle_Resectoring)
                    {
                        angleMouse += 2 * Math .PI;
                    }
                    if (min_angle_Resectoring + 0.05 < angleMouse && angleMouse < max_angle_Resectoring - 0.05)
                    {
                        double part_Counterclock = (max_angle_Resectoring - angleMouse) / (max_angle_Resectoring - min_angle_Resectoring);
                        if (iBorderToMove == 0)
                        {
                            m_angle = angleMouse;
                        }
                        vals [iSector_Counterclock] = two_sectors_sum_values * part_Counterclock;
                        vals [iSector_Clockwise] = two_sectors_sum_values - vals [iSector_Counterclock];
                        SectorAngles ();
                    }
                }
            }
            else if (catcher == MouseButtons .Right && bRotate)
            {
                //double angleMouse = Auxi_Geometry .Line_Angle (m_center, ptM);
                //m_angle = angleMouse - compensation;
                bRet = true;
            }
            return (bRet);
        }

        const string nameMain = "Circle_EAG_";
        // -------------------------------------------------        IntoRegistry
        public override void IntoRegistry (RegistryKey regkey, string strAdd)
        {
            try
            {
                string [] strs = new string [5 + vals .Length + 1 + 4 * vals .Length + 6];
                strs [0] = m_version .ToString ();
                strs [1] = m_center .X .ToString ();                            // ptC
                strs [2] = m_center .Y .ToString ();
                strs [3] = m_radius .ToString ();                               // radius
                strs [4] = Auxi_Convert .RadianToDegree (Angle) .ToString ();   // angleDegree
                for (int i = 0; i < vals .Length; i++)                          // vals
                {
                    strs [5 + i] = vals [i] .ToString ();
                }
                int k = 5 + vals .Length;
                strs [k] = bInGroup .ToString ();                               // bInGroup
                k++;
                for (int i = 0; i < vals .Length; i++)                          // clrs
                {
                    strs [k + i * 4] = ((int) (clrs [i] .A)) .ToString ();
                    strs [k + i * 4 + 1] = ((int) (clrs [i] .R)) .ToString ();
                    strs [k + i * 4 + 2] = ((int) (clrs [i] .G)) .ToString ();
                    strs [k + i * 4 + 3] = ((int) (clrs [i] .B)) .ToString ();
                }
                k += 4 * vals .Length;
                strs [k] = ((int) dirDrawing) .ToString ();
                strs [k + 1] = bFixSectors .ToString ();
                strs [k + 2] = bResize .ToString ();
                strs [k + 3] = bRotate .ToString ();
                strs [k + 4] = Visible .ToString ();
                strs [k + 5] = VisibleAsMember .ToString ();

                regkey .SetValue (nameMain + strAdd, strs, RegistryValueKind .MultiString);
            }
            catch
            {
            }
            finally
            {
            }
        }
        // -------------------------------------------------        FromRegistry
        public static JzCircleEAG FromRegistry (Control pic, Mover mvr, RegistryKey regkey, string strAdd)
        {
            try
            {
                string [] strs = (string []) regkey .GetValue (nameMain + strAdd);
                if (strs == null || strs .Length < 17 || Convert .ToInt32 (strs [0]) != 701)
                {
                    return (null);
                }
                int nVals = (strs .Length - 12) / 5;
                double [] fVals = new double [nVals];
                List<Color> colors = new List<Color> ();
                int j_vals = 5;
                int j_clrs = j_vals + nVals + 1;
                for (int i = 0; i < nVals; i++)
                {
                    fVals [i] = Convert .ToDouble (strs [j_vals + i]);
                    colors .Add (Auxi_Convert .ToColor (strs, j_clrs + i * 4));
                }
                JzCircleEAG circle = new JzCircleEAG (pic, mvr, 
                                                    Auxi_Convert .ToPointF (strs, 1),               // ptC
                                                    Convert .ToSingle (strs [3]),                   // radius
                                                    Convert .ToDouble (strs [4]),                   // angle
                                                    fVals,
                                                    Convert .ToBoolean (strs [j_vals + nVals]));    // bInGroup
                if (circle != null)
                {
                    circle .SetColors (colors);
                    int j_auxi = j_clrs + 4 * nVals;
                    circle .DrawingDirection = (Rotation) Convert .ToInt32 (strs [j_auxi]);
                    circle .FixSectors = Convert .ToBoolean (strs [j_auxi + 1]);
                    circle .Resizable = Convert .ToBoolean (strs [j_auxi + 2]);
                    circle .Rotatable = Convert .ToBoolean (strs [j_auxi + 3]);
                    circle .Visible = Convert .ToBoolean (strs [j_auxi + 4]);
                    circle .VisibleAsMember = Convert .ToBoolean (strs [j_auxi + 5]);
                }
                return (circle);
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
            CircleEAG.SetOffset(offsetpoint);
        }
        public override void SetAngle(double addangle)
        {
            CircleEAG.SetAngle(addangle);
        }
        public override void FromRectangleF(RectangleF rectf)
        {
            CircleEAG.FromRectF(rectf);
        }
        public override string ToString()
        {
            string retstr = "";

            retstr += m_figure.ToString() + ";";
            retstr += CircleEAG.ToString();

            return retstr;

        }
        public override void FromString(string str)
        {
            string[] strs = str.Split(';');

            m_figure = (Figure_EAG)Enum.Parse(typeof(Figure_EAG), strs[0]);
            CircleEAG.FromString(strs[1]);
        }
        public override string ToShapeString()
        {
            string retstr = "";

            retstr += m_center.X.ToString() + ",";  //0
            retstr += m_center.Y.ToString() + ",";  //1
            retstr += m_radius.ToString() + ",";    //2
            retstr += m_angle.ToString() + ",";     //3

            return retstr;
        }
        public override void FromShapeString(string str)
        {
            string[] strs = str.Split(',');

            m_center.X = float.Parse(strs[0]);
            m_center.Y = float.Parse(strs[1]);

            m_radius = float.Parse(strs[2]);
            m_angle = double.Parse(strs[3]);
            
        }
        public override void Backup()
        {
            CircleEAG.Backup();
        }
        public override void Restore()
        {
            CircleEAG.Restore();
        }
        public override void MappingToMovingObject(PointF bias, double ratio)
        {
            m_center.X = (float)((double)bias.X + ((double)CircleEAG.Center.X * ratio));
            m_center.Y = (float)((double)bias.Y + ((double)CircleEAG.Center.Y * ratio));
            
            m_radius = Math.Max(minRadius, (float)Math.Abs((double)CircleEAG.Radius * ratio));

            m_angle = Auxi_Convert.DegreeToRadian(CircleEAG.Degree);

            Move(OffsetPoint.X, OffsetPoint.Y);
            OffsetPoint = new Point(0, 0);
        }
        public override void MappingToMovingObject(PointF bias, SizeF sizeratio)
        {
            double ratio = Math.Min(sizeratio.Width, sizeratio.Height);

            m_center.X = (float)((double)bias.X + ((double)CircleEAG.Center.X * ratio));
            m_center.Y = (float)((double)bias.Y + ((double)CircleEAG.Center.Y * ratio));

            m_radius = Math.Max(minRadius, (float)Math.Abs((double)CircleEAG.Radius * ratio));

            m_angle = Auxi_Convert.DegreeToRadian(CircleEAG.Degree);

            Move(OffsetPoint.X, OffsetPoint.Y);
            OffsetPoint = new Point(0, 0);

            MappingFromMovingObject(new PointF(0, 0), 1);
        }
        public override void MappingFromMovingObject(PointF bias, double ratio)
        {
            CircleEAG.Center.X = (float)(((double)m_center.X - (double)bias.X) / ratio);
            CircleEAG.Center.Y = (float)(((double)m_center.Y - (double)bias.Y) / ratio);

            CircleEAG.Radius = (float)((double)m_radius / ratio);

            CircleEAG.Degree = Auxi_Convert.RadianToDegree(m_angle);
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
               // RectangleF rc = RectangleF.Round(RectAround);
               
                float fStartDegree, fSectorDegree;
                fStartDegree = -(float)Auxi_Convert.RadianToDegree(m_angle);

                for (int i = 0; i < clrs.Count; i++)
                {
                    fSectorDegree = -(float)Auxi_Convert.RadianToDegree(sector_angle[i]);
                    grfx.FillPie(new SolidBrush(clrs[i]), RectAround.X, RectAround.Y, RectAround.Width, RectAround.Height, fStartDegree, fSectorDegree);

                    fStartDegree += fSectorDegree;

                    if (!bFixSectors)
                    {
                        grfx.DrawLine(penPartition,
                                        Auxi_Geometry.PointToPoint(m_center, Auxi_Convert.DegreeToRadian(-fStartDegree), m_radius),
                                        m_center);
                    }
                }
                if (bResize)
                {
                    if (InGroup)
                    {
                        grfx.DrawEllipse(penResize_InGroup, RectAround);
                    }
                    else
                    {
                        grfx.DrawEllipse(BorderPen, RectAround);
                    }
                }

                SolidBrush myBorderBrush = new SolidBrush(BorderColor);

                grfx.FillEllipse(new SolidBrush(Color.Black), new RectangleF(m_center.X + m_radius - (delta * 2), m_center.Y - (delta * 2), 4 * delta, 4 * delta));
                grfx.FillEllipse(myBorderBrush, new RectangleF(m_center.X + m_radius - delta, m_center.Y - delta, 2 * delta, 2 * delta));

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

            grfx.FillPie(new SolidBrush(fillsolid.Color), Rectangle.Round(RectAround), 0, 360);

            FromShapeString(backupstring);
        }
        public override void Draw(Bitmap bmp, PointF ptfoffset,float degree, SolidBrush backsolid, SolidBrush fillsolid)
        {
            Graphics grfx = Graphics.FromImage(bmp);

            string backupstring = ToShapeString();

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
