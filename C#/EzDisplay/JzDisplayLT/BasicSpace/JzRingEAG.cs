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
    public class JzRingEAG : GeoFigure
    {
        class RingEAGClass
        {
            public PointF Center;
            public float RInner;
            public float ROutter;
            public double Degree;
            public double[] Vals;

            string BackupString = "";

            public RingEAGClass(bool isoring)
            {
                Center = new PointF(200, 200);
                RInner = 50;
                ROutter = 100;
                Degree = 0;

                if (isoring)
                    Vals = new double[] { 1 };
                else
                    Vals = new double[] { 0.5, 0.5 };
            }

            public RingEAGClass(PointF center, float rinner,float routter, double degree, double [] vals)
            {
                Center = center;
                RInner = rinner;
                ROutter = routter;
                Degree = degree;

                int i = 0;
                Vals = new double[vals.Length];

                while(i< vals.Length)
                {
                    Vals[i] = vals[i];
                    i++;
                }
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
            public override string ToString()
            {
                string str = "";

                str += PointFToString(Center) + "X";    //0
                str += RInner.ToString() + "X";         //1
                str += ROutter.ToString() + "X";        //2
                str += Degree.ToString() + "X";         //3

                string strx = "";                       //4

                foreach(double Val in Vals)
                {
                    strx += Val.ToString() + ",";
                }

                strx = strx.Remove(strx.Length - 1, 1);

                str += strx;

                return str;
            }
            public void FromString(string str)
            {
                string[] strs = str.Split('X');

                Center = StringToPointF(strs[0]);
                RInner = float.Parse(strs[1]);
                ROutter = float.Parse(strs[2]);
                Degree = double.Parse(strs[3]);

                string[] strxs = strs[4].Split(',');

                Vals = new double[strxs.Length];
                int i = 0;

                foreach(string strx in strxs)
                {
                    Vals[i] = double.Parse(strx);
                    i++;
                }
            }
            public void FromRectF(RectangleF rectF)
            {
                Center = new PointF(rectF.X + rectF.Width / 2, rectF.Y + rectF.Height / 2);
                ROutter = Math.Max(10, Math.Max(rectF.Width /2, rectF.Height/2));
                RInner = Math.Max(5, Math.Max(rectF.Width / 2, rectF.Height / 2) / 2);
                Degree = 0;
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

        float rInner;
        float rOuter;

        double [] vals;
        double [] sector_angle;
        List<Color> clrs = new List<Color> ();          // one color per each sector
        Rotation dirDrawing;

        double angleBeam, compensation;
        PointF ptEndIn, ptEndOut; 

        bool bFixSectors;
        Pen penSectorBorder;

        // all these are used only for moving the border between two sectors
        int iBorderToMove;
        int iSector_Counterclock, iSector_Clockwise;
        double min_angle_Resectoring,       // clockwise from the moving border
               max_angle_Resectoring;       // counterclock from the moving border
        double two_sectors_sum_values;

        int m_delta = 2;
        static int minInner = 1;
        static int minWidth = 1;

        RingEAGClass RingEAG = new RingEAGClass(false);

        // -------------------------------------------------
        public JzRingEAG (Control pic, Mover mvr, PointF ptC, float rIn, float rOut, double angleDegree, double [] fVals, bool in_group, Figure_EAG figure)
        {
            picImage = pic;
            supervisor = mvr;
            m_figure = figure;

            m_center = ptC;
            rInner = Math .Max (rIn, minInner);
            rOuter = Math .Max (rOut, rInner + minWidth);

            m_angle = Auxi_Convert .DegreeToRadian (angleDegree);
            dirDrawing = Rotation .Clockwise;
            CheckedValues (fVals);
            DefaultColors ();

            RingEAG = new RingEAGClass(m_center, rInner, rOuter, Auxi_Convert.RadianToDegree(m_angle), vals);

            penSectorBorder = new Pen (Color .DarkGray, 1);
            bInGroup = in_group;
        }
        // -------------------------------------------------
        public JzRingEAG (Control pic, Mover mvr, PointF ptC, float rIn, float rOut, double fSum, Color [] clrs, bool in_group, Figure_EAG figure)
            : this (pic, mvr, ptC, rIn, rOut, 0, Auxi_Common .ArrayOfEqualValues (clrs .Length, fSum / clrs .Length), in_group, figure)
        {
            for (int i = 0; i < clrs.Length; i++)
            {
                SetSectorColor(i, clrs[i]);
            }
        }
        public JzRingEAG(Control pic, Mover mvr, string str, Color color)    //Added By Victor Tsai
        {
            picImage = pic;
            supervisor = mvr;

            FromString(str);

            dirDrawing = Rotation.Clockwise;

            vals = new double[RingEAG.Vals.Length];

            int ix = 0;

            while(ix < vals.Length)
            {
                vals[ix] = RingEAG.Vals[ix];
                ix++;
            }

            sector_angle = new double[vals.Length];
            SectorAngles();
            //bFixSectors = true;                                                 // FixSector �N�O�������u By Victor Tsai 2017/05/11
            //penPartition = new Pen(Color.FromArgb(0, Color.Red), 3);
            //CheckedValues(new double[] { 5 });
            //DefaultColors();

            clrs.Clear();
            if (m_figure == Figure_EAG.Ring)
            {
                clrs.Add(color);
                clrs.Add(Color.FromArgb(0, Color.Red));
            }
            else
                clrs.Add(color);

            for (int i = 0; i < clrs.Count; i++)
            {
                SetSectorColor(i, clrs[i]);
            }

            bInGroup = false;
        }
        public JzRingEAG(string str, Color color)    //Added By Victor Tsai
        {
            FromString(str);

            vals = new double[RingEAG.Vals.Length];

            int ix = 0;

            while (ix < vals.Length)
            {
                vals[ix] = RingEAG.Vals[ix];
                ix++;
            }

            sector_angle = new double[vals.Length];
            SectorAngles();

            clrs.Clear();
            if (m_figure == Figure_EAG.Ring)
            {
                clrs.Add(color);
                clrs.Add(Color.FromArgb(0, color));
            }
            else
                clrs.Add(color);

            for (int i = 0; i < clrs.Count; i++)
            {
                SetSectorColor(i, clrs[i]);
            }

            bInGroup = false;
        }
        public  override void InitialCtrl(Control pic, Mover mvr)  //Added By Victor Tsai
        {
            picImage = pic;
            supervisor = mvr;
        }
        /// <summary>
        /// ����
        /// </summary>
        /// <param name="color"></param>
        public JzRingEAG(Color color, Figure_EAG figure)                       //Added By Victor Tsai
        {
            if (figure == Figure_EAG.Ring)
            {
                RingEAG = new RingEAGClass(false);
                FromString(Figure_EAG.Ring.ToString() + ";" + RingEAG.ToString());
            }
            else
            {
                RingEAG = new RingEAGClass(true);
                FromString(Figure_EAG.ORing.ToString() + ";" + RingEAG.ToString());
            }
            
            vals = new double[RingEAG.Vals.Length];

            int ix = 0;

            while (ix < vals.Length)
            {
                vals[ix] = RingEAG.Vals[ix];
                ix++;
            }

            sector_angle = new double[vals.Length];
            SectorAngles();

            clrs.Clear();
            if (m_figure == Figure_EAG.Ring)
            {
                clrs.Add(color);
                clrs.Add(Color.FromArgb(0, Color.Red));
            }
            else
                clrs.Add(color);

            for (int i = 0; i < clrs.Count; i++)
            {
                SetSectorColor(i, clrs[i]);
            }

            bInGroup = false;
        }

        public JzRingEAG(Color color, Figure_EAG figure,RectangleF rectf)                       //Added By Victor Tsai
        {
            if (figure == Figure_EAG.Ring)
            {
                RingEAG = new RingEAGClass(false);
                RingEAG.FromRectF(rectf);
                FromString(Figure_EAG.Ring.ToString() + ";" + RingEAG.ToString());
            }
            else
            {
                RingEAG = new RingEAGClass(true);
                RingEAG.FromRectF(rectf);
                FromString(Figure_EAG.ORing.ToString() + ";" + RingEAG.ToString());
            }

            vals = new double[RingEAG.Vals.Length];

            int ix = 0;

            while (ix < vals.Length)
            {
                vals[ix] = RingEAG.Vals[ix];
                ix++;
            }

            sector_angle = new double[vals.Length];
            SectorAngles();

            clrs.Clear();
            if (m_figure == Figure_EAG.Ring)
            {
                clrs.Add(color);
                clrs.Add(Color.FromArgb(0, Color.Red));
            }
            else
                clrs.Add(color);

            for (int i = 0; i < clrs.Count; i++)
            {
                SetSectorColor(i, clrs[i]);
            }

            bInGroup = false;
        }
        public JzRingEAG(Control pic, Mover mvr, JzRingEAG fromring)    //Added By Victor Tsai
        {
            picImage = pic;
            supervisor = mvr;

            FromString(fromring.ToString());
            RingEAG.Center.X += 100f;
            RingEAG.Center.Y += 100f;

            vals = new double[RingEAG.Vals.Length];

            dirDrawing = Rotation.Clockwise;

            int ix = 0;

            while (ix < vals.Length)
            {
                vals[ix] = RingEAG.Vals[ix];
                ix++;
            }

            sector_angle = new double[vals.Length];
            SectorAngles();

            clrs.Clear();
            if (m_figure == Figure_EAG.Ring)
            {
                clrs.Add(Color.FromArgb(60, Color.Red));
                clrs.Add(Color.FromArgb(0, Color.Red));
            }
            else
                clrs.Add(Color.FromArgb(60, Color.Red));

            for (int i = 0; i < clrs.Count; i++)
            {
                SetSectorColor(i, clrs[i]);
            }

            bInGroup = false;

            IsFirstSelected = fromring.IsFirstSelected;
            IsSelected = fromring.IsSelected;

            fromring.IsFirstSelected = false;
            fromring.IsSelected = false;
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
            JzRingEAG elem = new JzRingEAG(pic, mvr, pt, rInner, rOuter, Auxi_Convert.RadianToDegree(m_angle), vals, InGroup, m_figure);
            elem .SetColors (clrs);
            elem .Angle = m_angle;
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
            bool bRet = minInner <= coef * rInner &&
                        minWidth <= coef * (rOuter - rInner);
            return (bRet);
        }
        // -------------------------------------------------        Zoom
        public override bool Zoom (double coef, bool bCheck)
        {
            coef = Math .Abs (coef);
            if (bCheck)
            {
                if (minInner > coef * rInner ||
                    minWidth > coef * (rOuter - rInner))
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
            double fCoef_forInner = minInner / rInner;
            double fCoef_forWidth = minWidth / (rOuter - rInner);
            double fCoef = Math .Max (fCoef_forInner, fCoef_forWidth);
            Zoom (fCoef, false);
        }
        // -------------------------------------------------        SqueezePossible
        public override bool SqueezePossible
        {
            get { return (minInner < rInner && minWidth <= rOuter - rInner); }
        }
        // -------------------------------------------------        InnerRadius
        public float InnerRadius
        {
            get { return (rInner); }
        }
        // -------------------------------------------------        OuterRadius
        public float OuterRadius
        {
            get { return (rOuter); }
        }
        // -------------------------------------------------        MinimumInnerRadius
        static public int MinimumInnerRadius
        {
            get { return (minInner); }
        }
        // -------------------------------------------------        MinimumWidth
        static public int MinimumWidth
        {
            get { return (minWidth); }
        }
        // -------------------------------------------------        RectAround
        public override RectangleF RectAround
        {
            get
            {
                float f_rad = Convert .ToSingle (rOuter);
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
        // -------------------------------------------------        PenSectorBorders
        public Pen PenSectorBorders
        {
            get { return (penSectorBorder); }
            set { penSectorBorder = value; }
        }
        // -------------------------------------------------        GetPenResize
        public Pen GetPenResize (bool bInGroup)
        {
            return (bInGroup ? penResize_InGroup : penResize_Independent);
        }
        // -------------------------------------------------        SetPenResize
        public void SetPenResize (bool bInGroup, Pen pn)
        {
            if (bInGroup)
            {
                penResize_InGroup = pn;
            }
            else
            {
                penResize_Independent = pn;
            }
        }
        // -------------------------------------------------        Draw
        //public override void Draw (Graphics grfx)
        //{
        //    if (Visible && VisibleAsMember)
        //    {
        //        float f_rad = Convert .ToSingle (rInner);
        //        Rectangle rcIn = Rectangle .Round (new RectangleF (m_center .X - f_rad, m_center .Y - f_rad, 2 * f_rad, 2 * f_rad));
        //        Rectangle rcOut = Rectangle .Round (RectAround);
        //        Point pt0, pt1;
        //        SolidBrush brush;
        //        GraphicsPath path = new GraphicsPath ();
        //        // for Drawing fStart[] and fSector[] are used in Microsoft way, which means changing of sign
        //        float fStartDegree, fSectorDegree = 0;
        //        fStartDegree = -(float) Auxi_Convert .RadianToDegree (m_angle);

        //        for (int i = 0; i < vals .Length; i++)
        //        {
        //            brush = new SolidBrush (clrs [i]);
        //            fSectorDegree = -(float) Auxi_Convert .RadianToDegree (sector_angle [i]);

        //            path .AddArc (rcIn, fStartDegree, fSectorDegree);
        //            path .AddArc (rcOut, fStartDegree + fSectorDegree, -fSectorDegree);
        //            grfx .FillPath (brush, path);
        //            path .Reset ();

        //            pt0 = Auxi_Geometry .EllipsePoint (rcIn, -fStartDegree, 1);
        //            pt1 = Auxi_Geometry .EllipsePoint (rcOut, -fStartDegree, 1);
        //            if (!bFixSectors)
        //            {
        //                penSectorBorder.Width = m_delta;
        //                penSectorBorder.Color = Color.Red;
        //                grfx .DrawLine (penSectorBorder, pt0, pt1);
        //            }
        //            fStartDegree += fSectorDegree;
        //        }
        //        if (bResize)
        //        {
        //            if (InGroup)
        //            {
        //                grfx .DrawEllipse (penResize_InGroup, rcIn);
        //                grfx .DrawEllipse (penResize_InGroup, rcOut);
        //            }
        //            else
        //            {

        //                penResize_Independent.Width = m_delta;
        //                penResize_Independent.Color = Color.Red;

        //                //grfx.DrawArc(penResize_Independent, rcIn, fStartDegree, fSectorDegree);
        //                //grfx.DrawArc(penResize_Independent, rcOut, fStartDegree,  fSectorDegree);

        //                grfx .DrawEllipse (penResize_Independent, rcIn);
        //                grfx .DrawEllipse (penResize_Independent, rcOut);
        //            }
        //        }
        //    }
        //}
        // -------------------------------------------------        StartResizing
        public void StartResizing (Point ptMouse, int iNode)
        {
            angleBeam = Auxi_Geometry .Line_Angle (Center, ptMouse);
            double rad;
            if (iNode == 1)
            {
                rad = InnerRadius;
                ptEndIn = Auxi_Geometry .PointToPoint (Center, angleBeam, MinimumInnerRadius);
                ptEndOut = Auxi_Geometry .PointToPoint (Center, angleBeam, OuterRadius - MinimumWidth);
            }
            else
            {
                rad = OuterRadius;
                ptEndIn = Auxi_Geometry .PointToPoint (Center, angleBeam, InnerRadius + MinimumWidth);
                ptEndOut = Auxi_Geometry .PointToPoint (Center, angleBeam, 4000);
            }
            AdjustCursorPosition (Auxi_Geometry .PointToPoint (Center, angleBeam, rad));
        }
        // -------------------------------------------------        StartResectoring
        public void StartResectoring (int iNode)
        {
            iBorderToMove = iNode - 2;
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
        // order of nodes:  two circular nodes near inner border
        //                - strips on sectors' borders
        //                - two circular nodes near outer border
        public override void DefineCover ()
        {
            int nSectors = vals .Length;
            CoverNode [] nodes = new CoverNode [nSectors + 4];
            if (bResize)
            {
                nodes [0] = new CoverNode (0, Center, rInner - m_delta, Behaviour .Transparent);
                nodes [1] = new CoverNode (1, Center, rInner + m_delta);

                //nodes[0].Color = Color.Red;
                //nodes[1].Color = Color.Red;
            }
            //else
            //{
            //    nodes [0] = new CoverNode (0, Center, rInner, Behaviour .Transparent);
            //    nodes [1] = new CoverNode (1, Center, 0.0, Cursors .SizeAll);
            //}
            //if (bFixSectors)
            //{
            //    for (int i = 0; i < nSectors; i++)
            //    {
            //        nodes [i + 2] = new CoverNode (i + 2, Center, 0.0, Cursors .SizeAll);
            //    }
            //}
            //else
            {
                PointF ptIn, ptOut;
                double angleForLine = Angle;
                for (int i = 0; i < nSectors; i++)                         // nodes on borders between sectors
                {
                    ptIn = Auxi_Geometry.PointToPoint(Center, angleForLine, InnerRadius);
                    ptOut = Auxi_Geometry.PointToPoint(Center, angleForLine, OuterRadius);

                    if (i == 0)
                        nodes[i + 2] = new CoverNode(i + 2, ptIn, ptOut, Cursors.SizeAll);
                    else
                        nodes[i + 2] = new CoverNode(i + 2, ptIn, ptOut);

                    angleForLine += sector_angle[i];
                }
            }
            if (bResize)
            {
                //nodes [nSectors + 2] = new CoverNode (2, Center, rOuter - m_delta, Cursors .SizeAll);
                nodes[nSectors + 2] = new CoverNode(2, Center, rOuter - m_delta, Cursors.Default);
                nodes[nSectors + 2].SetBehaviourCursor(Behaviour.Transparent, Cursors.Default);

                nodes [nSectors + 3] = new CoverNode (3, Center, rOuter + m_delta);
            }
            //else
            //{
            //    nodes [nSectors + 2] = new CoverNode (2, Center, rOuter, Cursors .SizeAll);
            //    nodes [nSectors + 3] = new CoverNode (3, Center, 0.0, Cursors .SizeAll);
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
                if (iNode == 1 || iNode == cover .NodesCount - 1)
                {
                    PointF ptNearest = Auxi_Geometry .NearestPointOnSegment (ptM, ptEndIn, ptEndOut);
                    if (iNode == 1)             // inner border
                    {
                        rInner = Convert .ToSingle (Auxi_Geometry .Distance (Center, ptNearest));
                    }
                    else        // outer border
                    {
                        rOuter = Convert .ToSingle (Auxi_Geometry .Distance (Center, ptNearest));
                    }
                    AdjustCursorPosition (ptNearest);
                    bRet = true;
                }
                else if (iNode == cover .NodesCount - 2 || iNode == 2)
                {
                    Move (dx, dy);
                }
                else
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
                double angleMouse = Auxi_Geometry .Line_Angle (m_center, ptM);
                m_angle = angleMouse - compensation;
                bRet = true;
            }
            return (bRet);
        }

        const string nameMain = "Ring_EAG_";
        // -------------------------------------------------        IntoRegistry
        public override void IntoRegistry (RegistryKey regkey, string strAdd)
        {
            try
            {
                string[] strs = new string[6 + vals.Length + 1 + 4 * vals.Length + 6];
                strs[0] = m_version.ToString();
                strs[1] = m_center.X.ToString();                            // center
                strs[2] = m_center.Y.ToString();
                strs[3] = rInner.ToString();                                 // rInner
                strs[4] = rOuter.ToString();                                 // rOuter
                strs[5] = Auxi_Convert.RadianToDegree(Angle).ToString();   // angle
                for (int i = 0; i < vals.Length; i++)                          // vals
                {
                    strs[6 + i] = vals[i].ToString();
                }
                int k = 6 + vals.Length;
                strs[k] = bInGroup.ToString();                               // bInGroup
                k++;
                for (int i = 0; i < vals.Length; i++)                          // clrs
                {
                    strs[k + i * 4] = ((int)(clrs[i].A)).ToString();
                    strs[k + i * 4 + 1] = ((int)(clrs[i].R)).ToString();
                    strs[k + i * 4 + 2] = ((int)(clrs[i].G)).ToString();
                    strs[k + i * 4 + 3] = ((int)(clrs[i].B)).ToString();
                }
                k += 4 * vals.Length;
                strs[k] = ((int)dirDrawing).ToString();
                strs[k + 1] = bFixSectors.ToString();
                strs[k + 2] = bResize.ToString();
                strs[k + 3] = bRotate.ToString();
                strs[k + 4] = Visible.ToString();
                strs[k + 5] = VisibleAsMember.ToString();

                regkey.SetValue(nameMain + strAdd, strs, RegistryValueKind.MultiString);
            }
            catch
            {
            }
            finally
            {
            }
        }
        // -------------------------------------------------        FromRegistry
        public static JzRingEAG FromRegistry (Control pic, Mover mvr, RegistryKey regkey, string strAdd)
        {
            try
            {
                string[] strs = (string[])regkey.GetValue(nameMain + strAdd);
                if (strs == null || strs.Length < 18 || Convert.ToInt32(strs[0]) != 701)
                {
                    return (null);
                }
                int nVals = (strs.Length - 13) / 5;
                double[] fVals = new double[nVals];
                List<Color> colors = new List<Color>();
                int j_vals = 6;
                int j_clrs = j_vals + nVals + 1;
                for (int i = 0; i < nVals; i++)
                {
                    fVals[i] = Convert.ToDouble(strs[j_vals + i]);
                    colors.Add(Auxi_Convert.ToColor(strs, j_clrs + i * 4));
                }
                JzRingEAG ring = new JzRingEAG(pic, mvr,
                                              Auxi_Convert.ToPointF(strs, 1),                 // ptC
                                              Convert.ToSingle(strs[3]),                     // rInner
                                              Convert.ToSingle(strs[4]),                     // rOuter
                                              Convert.ToDouble(strs[5]),                     // angle
                                              fVals,
                                              Convert.ToBoolean(strs[j_vals + nVals]),
                                              Figure_EAG.Ring);      // bInGroup
                if (ring != null)
                {
                    ring.SetColors(colors);
                    int j_auxi = j_clrs + 4 * nVals;
                    ring.DrawingDirection = (Rotation)Convert.ToInt32(strs[j_auxi]);
                    ring.FixSectors = Convert.ToBoolean(strs[j_auxi + 1]);
                    ring.Resizable = Convert.ToBoolean(strs[j_auxi + 2]);
                    ring.Rotatable = Convert.ToBoolean(strs[j_auxi + 3]);
                    ring.Visible = Convert.ToBoolean(strs[j_auxi + 4]);
                    ring.VisibleAsMember = Convert.ToBoolean(strs[j_auxi + 5]);
                }
                return (ring);
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
            RingEAG.SetOffset(offsetpoint);
        }
        public override void SetAngle(double addangle)
        {
            RingEAG.SetAngle(addangle);
        }
        public override void FromRectangleF(RectangleF rectf)
        {
            RingEAG.FromRectF(rectf);
        }
        public override string ToString()
        {
            string retstr = "";

            retstr += m_figure.ToString() + ";";
            retstr += RingEAG.ToString();

            return retstr;

        }
        public override void FromString(string str)
        {
            string[] strs = str.Split(';');

            m_figure = (Figure_EAG)Enum.Parse(typeof(Figure_EAG), strs[0]);
            RingEAG.FromString(strs[1]);
        }
        public override string ToShapeString()
        {
            string retstr = "";

            retstr += PointFToString(m_center) + ";";
            retstr += rInner.ToString() + ";";
            retstr += rOuter.ToString() + ";";
            retstr += m_angle.ToString() + ";";

            foreach(double valx in vals)
            {
                retstr += valx.ToString() + ",";
            }

            retstr = retstr.Remove(retstr.Length - 1, 1);

            return retstr;
        }
        public override void FromShapeString(string str)
        {
            int i = 0;

            string[] strs = str.Split(';');

            m_center = StringToPointF(strs[0]);
            rInner = float.Parse(strs[1]);
            rOuter = float.Parse(strs[2]);
            m_angle = double.Parse(strs[3]);

            string[] strsx = strs[4].Split(',');

            vals = new double[strsx.Length];
            
            while(i < strsx.Length)
            {
                vals[i] = double.Parse(strsx[i]);
                i++;
            }


        }
        public override void Backup()
        {
            RingEAG.Backup();
        }
        public override void Restore()
        {
            RingEAG.Restore();
        }
        public override void MappingToMovingObject(PointF bias, double ratio)
        {
            m_center.X = (float)((double)bias.X + ((double)RingEAG.Center.X * ratio));
            m_center.Y = (float)((double)bias.Y + ((double)RingEAG.Center.Y * ratio));

            rInner = Math.Max(minInner, (float)Math.Abs((double)RingEAG.RInner * ratio));
            rOuter = Math.Max(rInner + minWidth, (float)Math.Abs((double)RingEAG.ROutter * ratio));

            m_angle = Auxi_Convert.DegreeToRadian(RingEAG.Degree);

            vals = new double[RingEAG.Vals.Length];

            int i = 0;

            while(i < vals.Length)
            {
                vals[i] = RingEAG.Vals[i];
                i++;
            }

            Move(OffsetPoint.X, OffsetPoint.Y);
            OffsetPoint = new Point(0, 0);
        }
        public override void MappingToMovingObject(PointF bias, SizeF sizeratio)
        {
            double ratio = Math.Min(sizeratio.Width, sizeratio.Height);

            m_center.X = (float)((double)bias.X + ((double)RingEAG.Center.X * sizeratio.Width));
            m_center.Y = (float)((double)bias.Y + ((double)RingEAG.Center.Y * sizeratio.Height));

            rInner = Math.Max(minInner, (float)Math.Abs((double)RingEAG.RInner * ratio));
            rOuter = Math.Max(rInner + minWidth, (float)Math.Abs((double)RingEAG.ROutter * ratio));

            m_angle = Auxi_Convert.DegreeToRadian(RingEAG.Degree);

            vals = new double[RingEAG.Vals.Length];

            int i = 0;

            while (i < vals.Length)
            {
                vals[i] = RingEAG.Vals[i];
                i++;
            }

            Move(OffsetPoint.X, OffsetPoint.Y);
            OffsetPoint = new Point(0, 0);

            MappingFromMovingObject(new PointF(0, 0), 1);
        }
        public override void MappingFromMovingObject(PointF bias, double ratio)
        {
            RingEAG.Center.X = (float)(((double)m_center.X - (double)bias.X) / ratio);
            RingEAG.Center.Y = (float)(((double)m_center.Y - (double)bias.Y) / ratio);

            RingEAG.RInner = (float)((double)rInner / ratio);
            RingEAG.ROutter = (float)((double)rOuter / ratio);

            RingEAG.Degree = Auxi_Convert.RadianToDegree(m_angle);

            RingEAG.Vals = new double[vals.Length];

            int i = 0;

            while (i < vals.Length)
            {
                RingEAG.Vals[i] = vals[i];

                i++;
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
                float f_rad = Convert.ToSingle(rInner);
                Rectangle rcIn = Rectangle.Round(new RectangleF(m_center.X - f_rad, m_center.Y - f_rad, 2 * f_rad, 2 * f_rad));
                Rectangle rcOut = Rectangle.Round(RectAround);
                Point pt0, pt1;
                SolidBrush brush;
                GraphicsPath path = new GraphicsPath();
                // for Drawing fStart[] and fSector[] are used in Microsoft way, which means changing of sign
                float fStartDegree, fSectorDegree = 0;
                fStartDegree = -(float)Auxi_Convert.RadianToDegree(m_angle);

                for (int i = 0; i < vals.Length; i++)
                {
                    brush = new SolidBrush(clrs[i]);
                    fSectorDegree = -(float)Auxi_Convert.RadianToDegree(sector_angle[i]);

                    path.AddArc(rcIn, fStartDegree, fSectorDegree);
                    path.AddArc(rcOut, fStartDegree + fSectorDegree, -fSectorDegree);
                    grfx.FillPath(brush, path);
                    path.Reset();

                    pt0 = Auxi_Geometry.EllipsePoint(rcIn, -fStartDegree, 1);
                    pt1 = Auxi_Geometry.EllipsePoint(rcOut, -fStartDegree, 1);
                    if (!bFixSectors)
                    {
                        //penSectorBorder.Width = m_delta;
                        //penSectorBorder.Color = Color.Red;

                        if (i == 0)
                            grfx.DrawLine(new Pen(BorderPen.Color, BorderPen.Width * 3), pt0, pt1);
                        else
                            grfx.DrawLine(BorderPen, pt0, pt1);
                    }
                    fStartDegree += fSectorDegree;
                }
                if (bResize)
                {
                    if (InGroup)
                    {
                        grfx.DrawEllipse(penResize_InGroup, rcIn);
                        grfx.DrawEllipse(penResize_InGroup, rcOut);
                    }
                    else
                    {

                        //penResize_Independent.Width = m_delta;
                        //penResize_Independent.Color = Color.Red;

                        //grfx.DrawArc(penResize_Independent, rcIn, fStartDegree, fSectorDegree);
                        //grfx.DrawArc(penResize_Independent, rcOut, fStartDegree,  fSectorDegree);

                        grfx.DrawEllipse(BorderPen, rcIn);
                        grfx.DrawEllipse(BorderPen, rcOut);
                    }
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


            float f_rad = Convert.ToSingle(rInner);
            Rectangle rcIn = Rectangle.Round(new RectangleF(m_center.X - f_rad, m_center.Y - f_rad, 2 * f_rad, 2 * f_rad));
            Rectangle rcOut = Rectangle.Round(RectAround);
            GraphicsPath path = new GraphicsPath();
            // for Drawing fStart[] and fSector[] are used in Microsoft way, which means changing of sign
            float fStartDegree, fSectorDegree = 0;
            fStartDegree = -(float)Auxi_Convert.RadianToDegree(m_angle);

            for (int i = 0; i < vals.Length; i++)
            {
                if (clrs[i].A == 0)
                {
                    i++;
                    continue;
                }

                fSectorDegree = -(float)Auxi_Convert.RadianToDegree(sector_angle[i]);

                path.AddArc(rcIn, fStartDegree, fSectorDegree);
                path.AddArc(rcOut, fStartDegree + fSectorDegree, -fSectorDegree);
                grfx.FillPath(fillsolid, path);
                path.Reset();

                //pt0 = Auxi_Geometry.EllipsePoint(rcIn, -fStartDegree, 1);
                //pt1 = Auxi_Geometry.EllipsePoint(rcOut, -fStartDegree, 1);
                //if (!bFixSectors)
                //{
                //    //penSectorBorder.Width = m_delta;
                //    //penSectorBorder.Color = Color.Red;
                //    grfx.DrawLine(BorderPen, pt0, pt1);
                //}
                fStartDegree += fSectorDegree;
            }


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


            //float f_rad = Convert.ToSingle(rInner);
            //Rectangle rcIn = Rectangle.Round(new RectangleF(m_center.X - f_rad, m_center.Y - f_rad, 2 * f_rad, 2 * f_rad));
            //Rectangle rcOut = Rectangle.Round(RectAround);
            //GraphicsPath path = new GraphicsPath();
            //// for Drawing fStart[] and fSector[] are used in Microsoft way, which means changing of sign
            //float fStartDegree, fSectorDegree = 0;
            //fStartDegree = -(float)Auxi_Convert.RadianToDegree(m_angle);

            //for (int i = 0; i < vals.Length; i++)
            //{
            //    if (clrs[i].A == 0)
            //    {
            //        i++;
            //        continue;
            //    }

            //    fSectorDegree = -(float)Auxi_Convert.RadianToDegree(sector_angle[i]);

            //    path.AddArc(rcIn, fStartDegree, fSectorDegree);
            //    path.AddArc(rcOut, fStartDegree + fSectorDegree, -fSectorDegree);
            //    grfx.FillPath(fillsolid, path);
            //    path.Reset();

            //    //pt0 = Auxi_Geometry.EllipsePoint(rcIn, -fStartDegree, 1);
            //    //pt1 = Auxi_Geometry.EllipsePoint(rcOut, -fStartDegree, 1);
            //    //if (!bFixSectors)
            //    //{
            //    //    //penSectorBorder.Width = m_delta;
            //    //    //penSectorBorder.Color = Color.Red;
            //    //    grfx.DrawLine(BorderPen, pt0, pt1);
            //    //}
            //    fStartDegree += fSectorDegree;
            //}


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
