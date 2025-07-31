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
    public class JzPolyEAG : GeoFigure
    {
        /// <summary>
        /// Newly Added Class For This Figure
        /// </summary>
        class PolyEAGClass
        {
            public PointF Center;
            public PointF [] PTFS;

            string BackupString = "";

            public PolyEAGClass()
            {
                Center = new PointF(200, 200);

                PTFS = new PointF[] { new PointF(150, 150), new PointF(175, 150), new PointF(225, 150), new PointF(250, 150), new PointF(250, 175), new PointF(250, 225), new PointF(250, 250), new PointF(225, 250), new PointF(175, 250), new PointF(150, 250), new PointF(150, 225), new PointF(150, 175) };

            }
            public PolyEAGClass(PointF center, PointF [] ptfs)
            {
                Center = center;

                PTFS = new PointF[ptfs.Length];

                for(int i=0;i < ptfs.Length;i++)
                {
                    PTFS[i] = ptfs[i];
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

                for (int i = 0; i < PTFS.Length; i++)
                {
                    PTFS[i].X += offset.X;
                    PTFS[i].Y += offset.Y;
                }
            }

            public void FromRectF(RectangleF rectf)
            {
                Center = new PointF(rectf.X + rectf.Width / 2, rectf.Y + rectf.Height / 2);

                float StartX = rectf.X;
                float StartY = rectf.Y;
                float WidthGap = rectf.Width / 3;
                float HeightGap = rectf.Height / 3;

                //PTFS = new PointF[] { new PointF(150, 150), new PointF(175, 150), new PointF(225, 150), new PointF(250, 150), new PointF(250, 175), new PointF(250, 225), new PointF(250, 250), new PointF(225, 250), new PointF(175, 250), new PointF(150, 250), new PointF(150, 225), new PointF(150, 175) };
                PTFS = new PointF[] { new PointF(StartX, StartY), new PointF(StartX + WidthGap, StartY), new PointF(StartX + 2* WidthGap, StartY), new PointF(rectf.Right, StartY)
                    , new PointF(rectf.Right, StartY + HeightGap), new PointF(rectf.Right, StartY + 2 * HeightGap), new PointF(rectf.Right, rectf.Bottom)
                    , new PointF(StartX + 2* WidthGap, rectf.Bottom), new PointF(StartX + WidthGap, rectf.Bottom), new PointF(StartX, rectf.Bottom)
                    , new PointF(StartX, StartY + 2* HeightGap), new PointF(StartX, StartY + HeightGap) };
            }

            public override string ToString()
            {
                string str = "";

                str += PointFToString(Center) + "X";   //0

                for(int i = 0; i < PTFS.Length;i ++)
                {
                    str += PointFToString(PTFS[i]) + "X";
                }

                str = str.Remove(str.Length - 1);

                return str;
            }

            public void FromString(string str)
            {
                string[] strs = str.Split('X');

                Center = StringToPointF(strs[0]);

                PTFS = new PointF[strs.Length - 1];
                for (int i = 1; i < strs.Length; i++)
                {
                    PTFS[i - 1] = StringToPointF(strs[i]);
                }
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

        PointF [] ptVertices;
        Color clrCenter;
        Color [] clrVertices;
        bool bAuxiLines;
        bool bDrawBorder;
        bool bReconfigure;

        PointF ptEndIn, ptEndOut;
        // for rotation or scaling between MouseDown and MouseUp
        double [] radii;
        double [] angles;
        double [] compensationVertex;   // only for rotation between MouseDown and MouseUp
        double [] scaling;              // only for scaling between MouseDown and MouseUp

        static double minAcross = 5;

        PolyEAGClass PolyEAG = new PolyEAGClass();

        // -------------------------------------------------
        public JzPolyEAG (Control pic, Mover mvr, PointF ptC, PointF [] ptsA, Color clrC, Color [] clrsA, bool in_group)
        {
            picImage = pic;
            supervisor = mvr;
            m_figure = Figure_EAG .ChatoyantPolygon;
            bReconfigure = true;

            PolyEAG = new PolyEAGClass(ptC, ptsA);

            m_center = ptC;
            clrCenter = clrC;

            if (ptsA != null && clrsA != null && ptsA .Length >= 3 && ptsA .Length == clrsA .Length)
            {

                ptVertices = new PointF [ptsA .Length];
                clrVertices = clrsA;

                for (int i = 0; i < ptVertices .Length; i++)
                {
                    ptVertices [i] = new PointF (ptsA [i] .X, ptsA [i] .Y);
                }
                
            }
            else
            {
                ptVertices = new PointF [4] {new PointF (ptC .X + 160, ptC .Y), new PointF (ptC .X, ptC .Y + 120), 
                                             new PointF (ptC .X - 80, ptC .Y), new PointF (ptC .X, ptC .Y - 40) };
                clrVertices = new Color [4] { Color .Red, Color .Green, Color .Blue, Color .Yellow };
            }
            bAuxiLines = false;
            bDrawBorder = true;

            radii = new double [ptVertices .Length];
            scaling = new double [ptVertices .Length];
            compensationVertex = new double [ptVertices .Length];
            angles = new double [ptVertices .Length];
            VerticesArrays ();

            m_angle = Auxi_Geometry .Line_Angle (m_center, ptVertices [0]);
            bInGroup = in_group;
        }
        public JzPolyEAG(Control pic, Mover mvr, string str, Color color)    //Added By Victor Tsai
        {
            picImage = pic;
            supervisor = mvr;

            FromString(str);

            bReconfigure = true;
            ptVertices = new PointF[PolyEAG.PTFS.Length];
            clrVertices = new Color[PolyEAG.PTFS.Length];

            m_center = PolyEAG.Center;

            for (int i = 0; i < PolyEAG.PTFS.Length; i++)
            {
                ptVertices[i] = PolyEAG.PTFS[i];
                clrVertices[i] = color;
            }

            clrCenter = color;

            bAuxiLines = false;
            bDrawBorder = true;
            
            bInGroup = false;

            radii = new double[ptVertices.Length];
            scaling = new double[ptVertices.Length];
            compensationVertex = new double[ptVertices.Length];
            angles = new double[ptVertices.Length];
            VerticesArrays();

            m_angle = Auxi_Geometry.Line_Angle(m_center, ptVertices[0]);

        }
        public JzPolyEAG(string str, Color color)    //Added By Victor Tsai
        {
            FromString(str);

            bReconfigure = true;
            ptVertices = new PointF[PolyEAG.PTFS.Length];
            clrVertices = new Color[PolyEAG.PTFS.Length];

            m_center = PolyEAG.Center;

            for (int i = 0; i < PolyEAG.PTFS.Length; i++)
            {
                ptVertices[i] = PolyEAG.PTFS[i];
                clrVertices[i] = color;
            }

            clrCenter = color;

            bAuxiLines = false;
            bDrawBorder = true;

            bInGroup = false;

            radii = new double[ptVertices.Length];
            scaling = new double[ptVertices.Length];
            compensationVertex = new double[ptVertices.Length];
            angles = new double[ptVertices.Length];
            VerticesArrays();

            m_angle = Auxi_Geometry.Line_Angle(m_center, ptVertices[0]);
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
        public JzPolyEAG(Color color)                       //Added By Victor Tsai
        {
            PolyEAG = new PolyEAGClass();

            FromString(Figure_EAG.ChatoyantPolygon.ToString() + ";" + PolyEAG.ToString());

            for (int i = 0; i < PolyEAG.PTFS.Length; i++)
            {
                ptVertices[i] = PolyEAG.PTFS[i];
                clrVertices[i] = color;
            }

            clrCenter = color;

            bInGroup = false;
        }
        public JzPolyEAG(Color color,RectangleF rectf)                       //Added By Victor Tsai
        {
            PolyEAG = new PolyEAGClass();
            PolyEAG.FromRectF(rectf);

            FromString(Figure_EAG.ChatoyantPolygon.ToString() + ";" + PolyEAG.ToString());

            bReconfigure = true;
            ptVertices = new PointF[PolyEAG.PTFS.Length];
            clrVertices = new Color[PolyEAG.PTFS.Length];


            for (int i = 0; i < PolyEAG.PTFS.Length; i++)
            {
                ptVertices[i] = PolyEAG.PTFS[i];
                clrVertices[i] = color;
            }

            clrCenter = color;

            bAuxiLines = false;
            bDrawBorder = true;

            bInGroup = false;

            radii = new double[ptVertices.Length];
            scaling = new double[ptVertices.Length];
            compensationVertex = new double[ptVertices.Length];
            angles = new double[ptVertices.Length];
            VerticesArrays();

            m_angle = Auxi_Geometry.Line_Angle(m_center, ptVertices[0]);
        }
        public JzPolyEAG(Control pic, Mover mvr, JzPolyEAG fromrpoly)    //Added By Victor Tsai
        {
            picImage = pic;
            supervisor = mvr;
            
            bReconfigure = true;
            ptVertices = new PointF[fromrpoly.ptVertices.Length];
            clrVertices = new Color[fromrpoly.clrVertices.Length];

            FromString(fromrpoly.ToString());
            PolyEAG.Center.X += 100f;
            PolyEAG.Center.Y += 100f;

            for (int i = 0; i < PolyEAG.PTFS.Length; i++)
            {
                PolyEAG.PTFS[i].X += 100f;
                PolyEAG.PTFS[i].Y += 100f;

                clrVertices[i] = fromrpoly.clrVertices[i];
            }

            clrCenter = fromrpoly.clrCenter;

            bInGroup = false;

            IsFirstSelected = fromrpoly.IsFirstSelected;
            IsSelected = fromrpoly.IsSelected;

            fromrpoly.IsFirstSelected = false;
            fromrpoly.IsSelected = false;

            bAuxiLines = false;
            bDrawBorder = true;

            radii = new double[ptVertices.Length];
            scaling = new double[ptVertices.Length];
            compensationVertex = new double[ptVertices.Length];
            angles = new double[ptVertices.Length];
            VerticesArrays();

            m_angle = Auxi_Geometry.Line_Angle(m_center, ptVertices[0]);
        }
        // -------------------------------------------------        Copy
        public override GeoFigure Copy (Control pic, Mover mvr, PointF pt)
        {
            VerticesArrays ();
            int nVertices = ptVertices .Length;
            PointF [] pts = new PointF [nVertices];
            Color [] clrs = new Color [nVertices];
            for (int i = 0; i < nVertices; i++)
            {
                pts [i] = new PointF (pt .X + (ptVertices [i] .X - m_center .X), pt .Y + (ptVertices [i] .Y - m_center .Y));
                clrs [i] = clrVertices [i];
            }
            JzPolyEAG elem = new JzPolyEAG (pic, mvr, pt, pts, clrCenter, clrs, InGroup);
            elem .Resizable = bResize;
            elem .Rotatable = bRotate;
            elem .Reconfigurable = bReconfigure;
            elem .Visible = Visible;
            elem .VisibleAsMember = VisibleAsMember;
            return ((GeoFigure) elem);
        }
        // -------------------------------------------------        ZoomAllowed
        public override bool ZoomAllowed (double coef)
        {
            RectangleF rc = RectAround;
            return (minAcross <= coef * Math .Max (rc .Width, rc .Height));
        }
        // -------------------------------------------------        Zoom
        public override bool Zoom (double coef, bool bCheck)
        {
            coef = Math .Abs (coef);
            if (bCheck)
            {
                RectangleF rc = RectAround;
                if (minAcross > coef * Math .Max (rc .Width, rc .Height))
                {
                    return (false);
                }
            }
            ptVertices = Auxi_Geometry .ZoomPolygon (m_center, ptVertices, coef);
            DefineCover ();
            return (true);
        }
        // -------------------------------------------------        SqueezeToMinimal
        public override void SqueezeToMinimal ()
        {
            RectangleF rc = RectAround;
            Zoom (minAcross / Math .Max (rc .Width, rc .Height), false);
        }
        // -------------------------------------------------        SqueezePossible
        public override bool SqueezePossible
        {
            get
            {
                RectangleF rc = RectAround;
                return (minAcross < Math .Max (rc .Width, rc .Height));
            }
        }
        // -------------------------------------------------        VerticesArrays
        private void VerticesArrays ()
        {
            for (int i = 0; i < ptVertices .Length; i++)
            {
                radii [i] = Auxi_Geometry .Distance (m_center, ptVertices [i]);
                angles [i] = Auxi_Geometry .Line_Angle (m_center, ptVertices [i]);
            }
        }
        // -------------------------------------------------        VerticesNumber
        public int VerticesNumber
        {
            get { return (ptVertices .Length); }
        }
        // -------------------------------------------------        Vertices
        public PointF [] Vertices
        {
            get { return (ptVertices); }
        }
        // -------------------------------------------------        RectAround
        public override RectangleF RectAround
        {
            get
            {
                RectangleF rc = Auxi_Geometry .RectangleAroundPoints (ptVertices);
                return (RectangleF .FromLTRB (Math .Min (m_center .X, rc .Left), Math .Min (m_center .Y, rc .Top),
                                              Math .Max (m_center .X, rc .Right), Math .Max (m_center .Y, rc .Bottom)));
            }
        }
        // -------------------------------------------------        Angle
        public override double Angle
        {
            get { return (m_angle); }
            set
            {
                VerticesArrays ();
                double angleDif = value - angles [0];
                for (int i = 0; i < ptVertices .Length; i++)
                {
                    angles [i] = Auxi_Common .LimitedRadian (angles [i] + angleDif);
                    ptVertices [i] = Auxi_Geometry .PointToPoint (m_center, angles [i], radii [i]);
                }
                DefineCover ();
            }
        }
        // -------------------------------------------------        GetVertexColor
        public Color GetVertexColor (int iVertex)
        {
            return (clrVertices [iVertex % VerticesNumber]);
        }
        // -------------------------------------------------        SetVertexColor
        public void SetVertexColor (int iVertex, Color clr)
        {
            clrVertices [iVertex % VerticesNumber] = clr;
        }
        // -------------------------------------------------        Colors
        public Color [] Colors
        {
            get { return (clrVertices); }
            set
            {
                for (int i = 0; i < clrVertices .Length; i++)
                {
                    clrVertices [i] = value [i];
                }
            }
        }
        // -------------------------------------------------        Color
        public override Color Color
        {
            get { return (CenterColor); }
            set { CenterColor = value; }
        }
        // -------------------------------------------------        ColorCenter
        public Color CenterColor
        {
            get { return (clrCenter); }
            set { clrCenter = value; }
        }
        // -------------------------------------------------        AuxiLines
        public bool AuxiLines
        {
            get { return (bAuxiLines); }
            set { bAuxiLines = value; }
        }
        // -------------------------------------------------        DrawBorder
        public bool DrawBorder
        {
            get { return (bDrawBorder); }
            set { bDrawBorder = value; }
        }
        // -------------------------------------------------        Reconfigurable
        public bool Reconfigurable
        {
            get { return (bReconfigure); }
            set
            {
                bReconfigure = value;
                DefineCover ();
            }
        }
        // -------------------------------------------------        ReconfigurableSwitch
        public void ReconfigurableSwitch ()
        {
            bReconfigure = !bReconfigure;
            DefineCover ();
        }
        // -------------------------------------------------        Draw
        //public override void Draw (Graphics grfx)
        //{
        //    if (Visible && VisibleAsMember)
        //    {
        //        if (bResize && bDrawBorder)
        //        {
        //            if (InGroup)
        //            {
        //                grfx .DrawPolygon (penResize_InGroup, ptVertices);
        //            }
        //            else
        //            {
        //                penResize_Independent.Width = 2;
        //                penResize_Independent.Color = Color.Red;

        //                grfx .DrawPolygon (penResize_Independent, ptVertices);
        //            }
        //        }
        //        Auxi_Drawing .FillChatoyantPolygon (grfx, m_center, ptVertices, clrCenter, clrVertices);

        //        foreach (PointF pt in ptVertices)
        //        {
        //            grfx.FillEllipse(Brushes.Red, pt.X - 3, pt.Y - 3, 6, 6);
        //        }

        //        if (bAuxiLines)
        //        {
        //            foreach (PointF pt in ptVertices)
        //            {
        //                grfx .DrawLine (Pens .Lime, m_center, pt);
        //            }
        //        }
        //    }
        //}
        // -------------------------------------------------        StartResizing
        // resizing is used only with the strip nodes
        //
        public void StartResizing (Point ptMouse, int iNode)
        {
            int iSegment = iNode - (VerticesNumber + 1);
            PointF ptNearest, ptBase;
            PointOfSegment typeNearest;
            Auxi_Geometry .Distance_PointSegment (ptMouse, ptVertices [iSegment], ptVertices [(iSegment + 1) % VerticesNumber],
                                                  out ptBase, out typeNearest, out ptNearest);
            AdjustCursorPosition (ptNearest);
            double distanceMouse = Auxi_Geometry .Distance (m_center, ptNearest);
            RectangleF rc = RectAround;
            double maxSqueezingCoef = Math .Max (rc .Width, rc .Height) / minAcross;
            double angleBeam = Auxi_Geometry .Line_Angle (Center, ptNearest);
            ptEndIn = Auxi_Geometry .PointToPoint (Center, angleBeam, distanceMouse / maxSqueezingCoef);
            ptEndOut = Auxi_Geometry .PointToPoint (Center, angleBeam, 4000);

            VerticesArrays ();
            for (int i = 0; i < VerticesNumber; i++)
            {
                scaling [i] = radii [i] / distanceMouse;
            }
        }
        // -------------------------------------------------        StartReconfiguring
        // this is only for circular nodes
        public void StartReconfiguring (int iNode)
        {
            PointF ptNew = (iNode < VerticesNumber) ? ptVertices [iNode] : m_center;
            AdjustCursorPosition (ptNew);
        }
        // -------------------------------------------------        StartRotation
        public override void StartRotation (Point ptMouse)
        {
            VerticesArrays ();
            double angleMouse = Auxi_Geometry .Line_Angle (m_center, ptMouse);
            for (int i = 0; i < VerticesNumber; i++)
            {
                compensationVertex [i] = Auxi_Common .LimitedRadian (angleMouse - angles [i]);
            }
        }
        // -------------------------------------------------        AdjustCursorPosition
        private void AdjustCursorPosition (PointF pt)
        {
            supervisor .MouseTraced = false;
            Cursor .Position = picImage .PointToScreen (Point .Round (pt));
            supervisor .MouseTraced = true;
        }
        // -------------------------------------------------        DefineCover
        // general order of nodes:
        //                 [VerticesNumber] - circular nodes in vertices
        //                 1                - circular node in ptCenter
        //                 [VerticesNumber] - strip nodes, covering each segment of the perimeter
        //                 [VerticesNumber] - big triangular nodes
        //
        public override void DefineCover ()
        {
            CoverNode [] nodes = new CoverNode [3 * VerticesNumber + 1];
            if (bReconfigure)
            {
                for (int i = 0; i < VerticesNumber; i++)
                {
                    nodes [i] = new CoverNode (i, ptVertices [i], 3);
                    //nodes[i].Color = Color.Red;                         //�~�䪺�p�� By Victor Tsai
                }
                nodes [VerticesNumber] = new CoverNode (VerticesNumber, m_center, 1);
                nodes[VerticesNumber].Color = Color.FromArgb(0, Color.Black);                  //���ߪ���m���� #############################
                nodes[VerticesNumber].SetBehaviourCursor(Behaviour.Transparent, Cursors.Default);
            }
            //else
            //{
            //    for (int i = 0; i < VerticesNumber; i++)
            //    {
            //        nodes [i] = new CoverNode (i, m_center, 0.0, Cursors .SizeAll);
            //    }
            //    nodes [VerticesNumber] = new CoverNode (VerticesNumber, m_center, 0.0, Cursors .SizeAll);
            //}
            int k0 = VerticesNumber + 1;
            if (bResize)
            {
                for (int i = 0; i < VerticesNumber; i++)
                {
                    nodes[k0 + i] = new CoverNode(k0 + i, ptVertices[i], ptVertices[(i + 1) % VerticesNumber], 1);
                    nodes[k0 + i].SetBehaviourCursor(Behaviour.Moveable, Cursors.SizeAll);

                    //nodes[k0 + i].Color = Color.Red;             //���۶����u By Victor Tsai   
                }
            }
            //else
            //{
            //    for (int i = 0; i < VerticesNumber; i++)
            //    {
            //        nodes [k0 + i] = new CoverNode (k0 + i, Center, 0.0, Cursors .SizeAll);

            //    }
            //}
            k0 += VerticesNumber;
            for (int i = 0; i < VerticesNumber; i++)
            {
                PointF [] pts = new PointF [3] { ptVertices [i], ptVertices [(i + 1) % VerticesNumber], m_center };
                nodes [k0 + i] = new CoverNode (k0 + i, pts);
                nodes[k0 + i].SetBehaviourCursor(Behaviour.Transparent, Cursors.Default);


            }
            cover = new Cover (nodes);

            if (TransparentForMover)
                cover.SetBehaviour(Behaviour.Transparent);
        }
        // -------------------------------------------------        Move
        public override void Move (int dx, int dy)
        {
            Size size = new Size (dx, dy);
            m_center += size;
            for (int i = 0; i < VerticesNumber; i++)
            {
                ptVertices [i] += size;
            }
        }
        public override void Move(float dx, float dy)
        {
            SizeF size = new SizeF(dx, dy);
            m_center += size;
            for (int i = 0; i < VerticesNumber; i++)
            {
                ptVertices[i] += size;
            }
        }
        // -------------------------------------------------        MoveNode
        public override bool MoveNode (int iNode, int dx, int dy, Point ptM, MouseButtons catcher)
        {
            if (catcher == MouseButtons .Left)
            {
                if (iNode < VerticesNumber)
                {
                    ptVertices [iNode] = ptM;
                    DefineCover ();
                }
                else if (iNode == VerticesNumber)
                {
                    m_center = ptM;
                    DefineCover ();
                }
                //else if (iNode < 2 * VerticesNumber + 1)
                //{
                //    PointF ptNearest = Auxi_Geometry .NearestPointOnSegment (ptM, ptEndIn, ptEndOut);
                //    double distMouse = Auxi_Geometry .Distance (Center, ptNearest);
                //    for (int j = 0; j < VerticesNumber; j++)
                //    {
                //        ptVertices [j] = Auxi_Geometry .PointToPoint (m_center, angles [j], distMouse * scaling [j]);
                //    }
                //    DefineCover ();
                //    AdjustCursorPosition (ptNearest);
                //}
                else if(iNode < 2 * VerticesNumber + 1)
                {
                    Move (dx, dy);
                }
                return (true);
            }
            else if (catcher == MouseButtons .Right && bRotate)
            {
                double angleMouse = -Math .Atan2 (ptM .Y - m_center .Y, ptM .X - m_center .X);
                for (int j = 0; j < VerticesNumber; j++)
                {
                    ptVertices [j] = Auxi_Geometry .PointToPoint (m_center, Auxi_Common .LimitedRadian (angleMouse - compensationVertex [j]), 
                                                                  radii [j]);
                }
                DefineCover ();
                return (true);
            }
            return (false);
        }

        const string nameMain = "ChatoyantPolygon_EAG_";
        // -------------------------------------------------        IntoRegistry
        public override void IntoRegistry (RegistryKey regkey, string strAdd)
        {
            try
            {
                int nPts = Vertices .Length;
                string [] strs = new string [3 + 2 * nPts + 4 + 4 * nPts + 7];
                strs [0] = m_version .ToString ();
                strs [1] = m_center .X .ToString ();
                strs [2] = m_center .Y .ToString ();
                int jPts = 3;
                for (int i = 0; i < nPts; i++)
                {
                    strs [jPts + i * 2] = Vertices [i] .X .ToString ();
                    strs [jPts + i * 2 + 1] = Vertices [i] .Y .ToString ();
                }
                int k = jPts + 2 * nPts;
                strs [k] = ((int) (clrCenter .A)) .ToString ();
                strs [k + 1] = ((int) (clrCenter .R)) .ToString ();
                strs [k + 2] = ((int) (clrCenter .G)) .ToString ();
                strs [k + 3] = ((int) (clrCenter .B)) .ToString ();
                k += 4;
                Color clr;
                for (int i = 0; i < nPts; i++)
                {
                    clr = clrVertices [i];
                    strs [k + i * 4] = ((int) (clr .A)) .ToString ();
                    strs [k + i * 4 + 1] = ((int) (clr .R)) .ToString ();
                    strs [k + i * 4 + 2] = ((int) (clr .G)) .ToString ();
                    strs [k + i * 4 + 3] = ((int) (clr .B)) .ToString ();
                }
                k += 4 * nPts;
                strs [k] = bInGroup .ToString ();           // bInGroup
                strs [k + 1] = bResize .ToString ();        // 
                strs [k + 2] = bRotate .ToString ();        // 
                strs [k + 3] = bReconfigure .ToString ();   // 
                strs [k + 4] = bAuxiLines .ToString ();
                strs [k + 5] = Visible .ToString ();
                strs [k + 6] = VisibleAsMember .ToString ();
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
        public static JzPolyEAG FromRegistry (Control pic, Mover mvr, RegistryKey regkey, string strAdd)
        {
            try
            {
                string [] strs = (string []) regkey .GetValue (nameMain + strAdd);
                if (strs == null || strs .Length < 32 || Convert .ToInt32 (strs [0]) != 701)
                {
                    return (null);
                }
                int nVertices = (strs .Length - 14) / 6;
                PointF [] ptsA = new PointF [nVertices];
                Color [] clrsA = new Color [nVertices];
                int jPts = 3;
                int jClrCenter = jPts + 2 * nVertices;
                int jClrs = jClrCenter + 4;
                int jAuxi = jClrs + 4 * nVertices;
                for (int i = 0; i < nVertices; i++)
                {
                    ptsA [i] = Auxi_Convert .ToPointF (strs, jPts + i * 2);
                    clrsA [i] = Auxi_Convert .ToColor (strs, jClrs + i * 4);
                }
                JzPolyEAG poly = new JzPolyEAG (pic, mvr, 
                                                                      Auxi_Convert .ToPointF (strs, 1), ptsA,
                                                                      Auxi_Convert .ToColor (strs, jClrCenter), clrsA,
                                                                      Convert .ToBoolean (strs [jAuxi]));                 // in_group
                if (poly != null)
                {
                    poly .Resizable = Convert .ToBoolean (strs [jAuxi + 1]);
                    poly .Rotatable = Convert .ToBoolean (strs [jAuxi + 2]);
                    poly .Reconfigurable = Convert .ToBoolean (strs [jAuxi + 3]);
                    poly .AuxiLines = Convert .ToBoolean (strs [jAuxi + 4]);
                    poly .Visible = Convert .ToBoolean (strs [jAuxi + 5]);
                    poly .VisibleAsMember = Convert .ToBoolean (strs [jAuxi + 6]);
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
            PolyEAG.SetOffset(offsetpoint);
        }
        public override void SetAngle(double addangle)
        {
            
        }
        public override void FromRectangleF(RectangleF rectf)
        {
            PolyEAG.FromRectF(rectf);
        }
        
        public override string ToString()
        {
            string retstr = "";

            retstr += m_figure.ToString() + ";";
            retstr += PolyEAG.ToString();

            return retstr;

        }
        public override void FromString(string str)
        {
            string[] strs = str.Split(';');

            m_figure = (Figure_EAG)Enum.Parse(typeof(Figure_EAG), strs[0]);
            PolyEAG.FromString(strs[1]);
        }
        public override string ToShapeString()
        {
            string retstr = "";

            retstr += PointFToString(m_center) + ";";

            foreach(PointF ptf in ptVertices)
            {
                retstr += PointFToString(ptf) + "X";
            }

            retstr = retstr.Remove(retstr.Length - 1, 1);

            return retstr;
        }
        public override void FromShapeString(string str)
        {
            int i = 0;

            string[] strs = str.Split(';');

            m_center = StringToPointF(strs[0]);

            string[] strsx = strs[1].Split('X');

            ptVertices = new PointF[strsx.Length];

            foreach(string strx in strsx)
            {
                ptVertices[i] = StringToPointF(strx);
                i++;
            }


        }
        public override void Backup()
        {
            PolyEAG.Backup();
        }
        public override void Restore()
        {
            PolyEAG.Restore();
        }
        public override void MappingToMovingObject(PointF bias, double ratio)
        {
            m_center.X = (float)((double)bias.X + ((double)PolyEAG.Center.X * ratio));
            m_center.Y = (float)((double)bias.Y + ((double)PolyEAG.Center.Y * ratio));

            int i = 0;

            ptVertices = new PointF[PolyEAG.PTFS.Length];

            while (i < PolyEAG.PTFS.Length)
            {
                ptVertices[i].X = (float)((double)bias.X + ((double)PolyEAG.PTFS[i].X * ratio));
                ptVertices[i].Y = (float)((double)bias.Y + ((double)PolyEAG.PTFS[i].Y * ratio));

                i++;
            }

            Move(OffsetPoint.X, OffsetPoint.Y);
            OffsetPoint = new Point(0, 0);
        }
        public override void MappingToMovingObject(PointF bias, SizeF sizeratio)
        {
            m_center.X = (float)((double)bias.X + ((double)PolyEAG.Center.X * sizeratio.Width));
            m_center.Y = (float)((double)bias.Y + ((double)PolyEAG.Center.Y * sizeratio.Height));

            int i = 0;

            ptVertices = new PointF[PolyEAG.PTFS.Length];

            while (i < PolyEAG.PTFS.Length)
            {
                ptVertices[i].X = (float)((double)bias.X + ((double)PolyEAG.PTFS[i].X * sizeratio.Width));
                ptVertices[i].Y = (float)((double)bias.Y + ((double)PolyEAG.PTFS[i].Y * sizeratio.Height));

                i++;
            }

            Move(OffsetPoint.X, OffsetPoint.Y);
            OffsetPoint = new Point(0, 0);

            MappingFromMovingObject(new PointF(0, 0), 1);
        }
        public override void MappingFromMovingObject(PointF bias, double ratio)
        {
            PolyEAG.Center.X = (float)(((double)m_center.X - (double)bias.X) / ratio);
            PolyEAG.Center.Y = (float)(((double)m_center.Y - (double)bias.Y) / ratio);

            int i = 0;

            while (i < PolyEAG.PTFS.Length)
            {
                PolyEAG.PTFS[i].X = (float)(((double)ptVertices[i].X - (double)bias.X) / ratio);
                PolyEAG.PTFS[i].Y = (float)(((double)ptVertices[i].Y - (double)bias.Y) / ratio);

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

            //if (Visible && VisibleAsMember)
            //{
            //    Rectangle rc = Rectangle.Round(RectAround);
            //    float fStartDegree, fSectorDegree;
            //    fStartDegree = -(float)Auxi_Convert.RadianToDegree(m_angle);

            //    for (int i = 0; i < clrs.Count; i++)
            //    {
            //        fSectorDegree = -(float)Auxi_Convert.RadianToDegree(sector_angle[i]);
            //        grfx.FillPie(new SolidBrush(clrs[i]), rc, fStartDegree, fSectorDegree);

            //        fStartDegree += fSectorDegree;

            //        if (!bFixSectors)
            //        {
            //            grfx.DrawLine(penPartition,
            //                            Auxi_Geometry.PointToPoint(m_center, Auxi_Convert.DegreeToRadian(-fStartDegree), m_radius),
            //                            m_center);
            //        }
            //    }
            //    if (bResize)
            //    {
            //        if (InGroup)
            //        {
            //            grfx.DrawEllipse(penResize_InGroup, rc);
            //        }
            //        else
            //        {
            //            grfx.DrawEllipse(BorderPen, rc);
            //        }
            //    }
            //}

            if (Visible && VisibleAsMember)
            {
                if (bResize && bDrawBorder)
                {
                    if (InGroup)
                    {
                        grfx.DrawPolygon(penResize_InGroup, ptVertices);
                    }
                    else
                    {
                        grfx.DrawPolygon(BorderPen, ptVertices);
                    }
                }
                Auxi_Drawing.FillChatoyantPolygon(grfx, m_center, ptVertices, clrCenter, clrVertices);

                if (ShowMode != ShowModeEnum.MAINSHOW)
                {
                    foreach (PointF pt in ptVertices)
                    {
                        //grfx.FillEllipse(new SolidBrush(BorderPen.Color), pt.X - 3, pt.Y - 3, 6, 6);
                        grfx.FillEllipse(new SolidBrush(Color.Black), pt.X - 3f, pt.Y - 3f, 6, 6);
                        grfx.FillEllipse(new SolidBrush(BorderPen.Color), pt.X - 1.5f, pt.Y - 1.5f, 3, 3);
                    }
                }

                if (bAuxiLines)
                {
                    foreach (PointF pt in ptVertices)
                    {
                        grfx.DrawLine(Pens.Lime, m_center, pt);
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

            Color mycenterfill = fillsolid.Color;
            Color[] myfill = new Color[clrVertices.Length];

            int i = 0;
            while (i < myfill.Length)
            {
                myfill[i] = fillsolid.Color;
                i++;
            }

            Auxi_Drawing.FillChatoyantPolygon(grfx, m_center, ptVertices, mycenterfill, myfill);

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
            
            //Color mycenterfill = fillsolid.Color;
            //Color[] myfill = new Color[clrVertices.Length];

            //int i = 0;
            //while(i < myfill.Length)
            //{
            //    myfill[i] = fillsolid.Color;
            //    i++;
            //}
            
            //Auxi_Drawing.FillChatoyantPolygon(grfx, m_center, ptVertices, mycenterfill, myfill);

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
