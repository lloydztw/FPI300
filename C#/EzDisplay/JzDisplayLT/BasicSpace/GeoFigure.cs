using System;
using System .Collections .Generic;
using System .ComponentModel;
using System .Drawing;
using System.Drawing.Imaging;
using System .Drawing .Drawing2D;
using System .Windows .Forms;
using Microsoft .Win32;

using JzDisplay;
using MoveGraphLibrary;

namespace WorldOfMoveableObjects
{
    public enum Figure_EAG : int
    {
        Rectangle = 0,                      // Rectangle_EAG
        Circle = 1,                         // Circle_EAG 
        Ring = 2,                           // Ring_EAG 
        Strip = 3,                          // Strip_EAG
        RegularPolygon = 4,                 // RegularPolygon_EAG
        RegPoly_IdenticalHole = 5,          // RegPoly_IdenticalHole_EAG
        ChatoyantPolygon = 6,               // ChatoyantPolygon_EAG
        ConvexPolygon = 7,                  // ConvexPolygon_EAG
        RegPoly_RegPolyHole = 8,            // RegPoly_RegPolyHole_EAG     
        RegPoly_CircleHole = 9,             // RegPoly_CircleHole_EAG
        ConvexPoly_RegPolyHole = 10,         // ConvexPoly_RegPolyHole_EAG  
        Any = 11,                            // Any Spae
        ORing = 12,
        RectRect = 13,
        HexHex = 14,
        RectO = 15,
        HexO = 16,
        RatioRect = 17,
    };

    [Serializable]
    public abstract class GeoFigure : GraphicalObject
    {
        protected Figure_EAG m_figure;
        protected bool bResize;
        protected bool bRotate;
        protected PointF m_center;
        protected double m_angle;
        protected bool bInGroup;

        protected Pen penResize_Independent, penResize_InGroup;
        protected SolidBrush brushAnchor;
        protected Pen penUnder_Independent = new Pen(Color.Black, 5);
        protected Pen BorderPen = new Pen(Color.Lime, 3);

        public Pen MainShowPen = new Pen(Color.Lime, 2);

        public bool IsFirstSelected = false;
        public bool IsSelected = false;

        public ShowModeEnum ShowMode = ShowModeEnum.NORMAL;

        public int RelateNo = -1;
        public int RelatePosition = -1;
        public int RelateLevel = -1;        //�Y�o�ӭȤ��� 1�A�h���|��� RelateNo = 1 �N���諸�ʧ@
        public string Desc = string.Empty;
        public int LearnCount = 0;
        public Point OffsetPoint = new Point(); //One Time Offset, After use will reset

        public float PenWidth = 2;
        public float FontSize = 10;
        public bool NoShowCorner = false;

        public Color BorderColor
        {
            get
            {
                return BorderPen.Color;
            }
            set
            {
                BorderPen.Color = value;
            }
        }
        new public abstract RectangleF RectAround { get; }

        public abstract Color Color { get; set; }
        public abstract double Angle { get; set; }
        public abstract GeoFigure Copy (Control pic, Mover mvr, PointF pt);
        public abstract bool ZoomAllowed (double coef);
        public abstract bool Zoom (double coef, bool bCheck);
        public abstract bool SqueezePossible { get; }
        public abstract void SqueezeToMinimal ();
        public abstract void Draw (Graphics grfx);
        public abstract void StartRotation (Point ptMouse);
        public abstract void IntoRegistry (RegistryKey regkey, string strAdd);
        public abstract void SetOffset(Point offsetpoint);
        public abstract void SetAngle(double adddegree);
        public abstract void FromRectangleF(RectangleF rectf);
        public abstract RectangleF RealRectangleAround(int outrangex, int outrangey);
        public abstract void DrawMask(Graphics grfx, PointF ptfoffset, float degree, SolidBrush backsolid, SolidBrush fillsolid, Size bmpsize);
        public abstract void Draw(Bitmap bmp, PointF ptfoffset, float degree, SolidBrush backsolid, SolidBrush fillsolid);
        public abstract void GenSearchImage(int outrangex, int outrangey, Bitmap bmporg, ref Bitmap bmpsearch, ref Bitmap bmpmask);
        public abstract void DigImage(int outrangex, int outrangey, Bitmap bmp);
        public abstract void FromString(string str);
        public abstract string ToShapeString();
        public abstract void FromShapeString(string str);
        public abstract void Backup();
        public abstract void Restore();
        public abstract void MappingToMovingObject(PointF bias, double ratio);
        public abstract void MappingToMovingObject(PointF bias, SizeF sizeratio);
        public abstract void MappingFromMovingObject(PointF bias, double ratio);
        public abstract void InitialCtrl(Control pic, Mover mvr);
        public abstract void Move(float dx, float dy);

        // -------------------------------------------------         
        public GeoFigure ()
        {
            bResize = true;
            bRotate = true;
            m_angle = 0;
            bInGroup = false;
            penResize_Independent = new Pen (Color.FromArgb(0, Color.Red), 0);
            penResize_InGroup = new Pen (Color.FromArgb(0, Color.Red), 0);
            brushAnchor = new SolidBrush (Color .FromArgb(0,Color.Red));        //�N�����I�]���z�� By Victor Tsai
        }
        // -------------------------------------------------        Figure
        public Figure_EAG Figure
        {
            get { return (m_figure); }
        }
        // -------------------------------------------------        Resizable
        public bool Resizable
        {
            get { return (bResize); }
            set
            {
                bResize = value;
                DefineCover ();
            }
        }
        // -------------------------------------------------        Rotatable
        public bool Rotatable
        {
            get { return (bRotate); }
            set
            {
                bRotate = value;
                DefineCover ();
            }
        }
        // -------------------------------------------------        ResizableSwitch
        public void ResizableSwitch ()
        {
            bResize = !bResize;
            DefineCover ();
        }
        // -------------------------------------------------        RotatableSwitch
        public void RotatableSwitch ()
        {
            bRotate = !bRotate;
            DefineCover ();
        }
        // -------------------------------------------------        InView
        public bool InView
        {
            get { return (Visible && VisibleAsMember); }
        }
        // -------------------------------------------------        InGroup
        public bool InGroup
        {
            get { return (bInGroup); }
            set
            {
                bInGroup = value;
                if (value == false)
                {
                    ParentID = -1;
                }
            }
        }
        // -------------------------------------------------        Center
        public PointF Center
        {
            get { return (m_center); }
            set
            {
                Move (Convert .ToInt32 (value .X - m_center .X), Convert .ToInt32 (value .Y - m_center .Y));
                DefineCover ();
            }
        }
        //  -------------------------------------------------        IntoClipboard
        public void IntoClipboard (Color clrPaper)
        {
            Rectangle rcAround = Rectangle .Round (RectAround);
            Move (-rcAround .X, -rcAround .Y);

            Rectangle rc = Rectangle .FromLTRB (0, 0, rcAround .Width, rcAround .Height);
            Bitmap bitmap = new Bitmap (rc .Width, rc .Height);   
            Graphics gFull = Graphics .FromImage (bitmap);
            gFull .Clear (clrPaper);
            Draw (gFull);
            gFull .Dispose ();
            Clipboard .Clear ();
            Clipboard .SetDataObject (bitmap, true);

            Move (rcAround .X, rcAround .Y);
        }
        //  -------------------------------------------------        Check If Select Rextangle is intersetion
        public bool CheckIntersection(Rectangle rectselect)
        {
            int node = 0;

            //�Y�O�p�󵥩�1���A�u��γ�ӧ�A���|�i�JSELECT
            if (RelateNo <= 1 && RelateLevel <= 1)
                return false;

            bool isinside = false;

            Behaviour behaviour = Behaviour.Moveable;
            Cursor cursor = Cursors.Arrow;

            RectangleF rectfBound = RectAround;

            IsFirstSelected = false;

            if (rectfBound.IntersectsWith(rectselect))
            {
                Rectangle rect = new Rectangle((int)rectfBound.X, (int)rectfBound.Y, (int)rectfBound.Width, (int)rectfBound.Height);

                rect.Intersect(rectselect);

                for (int i = 0; i < rect.Width; i = i + 3)
                {
                    for (int j = 0; j < rect.Height; j = j + 3)
                    {
                        if (cover.Inside(new Point(rect.X + i, rect.Y + j), ref node, ref behaviour, ref cursor))
                        {
                            //IsSelected = true;
                            isinside = true;

                            i = 100000;
                            j = 100000;
                        }
                    }
                }

                //if (!IsSelected)
                {
                    IsSelected = isinside;
                }
            }
            else
            {
                IsSelected = false;
            }

            return isinside;
        }
        public void GetMaskedImage(Bitmap bmp, Bitmap bmpmask, Color getcolor, Color bgcolor, bool isreverse)
        {
            Rectangle rectbmp = new Rectangle(0, 0, bmp.Width, bmp.Height);

            BitmapData bmpData = bmp.LockBits(rectbmp, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            BitmapData bmpData1 = bmpmask.LockBits(rectbmp, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            IntPtr Scan0 = bmpData.Scan0;
            IntPtr Scan1 = bmpData1.Scan0;

            //try
            {
                unsafe
                {
                    byte* scan0 = (byte*)(void*)Scan0;
                    byte* pucPtr;
                    byte* pucStart;

                    byte* scan1 = (byte*)(void*)Scan1;
                    byte* pucPtr1;
                    byte* pucStart1;

                    int xmin = rectbmp.X;
                    int ymin = rectbmp.Y;
                    int xmax = xmin + rectbmp.Width;
                    int ymax = ymin + rectbmp.Height;

                    int x = xmin;
                    int y = ymin;

                    int iStride = bmpData.Stride;
                    int iStride1 = bmpData1.Stride;

                    y = ymin;

                    pucStart = scan0 + ((x - xmin) << 2) + (iStride * (y - ymin));
                    pucStart1 = scan1 + ((x - xmin) << 2) + (iStride1 * (y - ymin));

                    while (y < ymax)
                    {
                        x = xmin;

                        pucPtr = pucStart;
                        pucPtr1 = pucStart1;

                        while (x < xmax)
                        {
                            if (pucPtr1[0] == getcolor.B && pucPtr1[1] == getcolor.G && pucPtr1[2] == getcolor.R)
                            {
                                if (isreverse)
                                {
                                    pucPtr[0] = pucPtr[0];
                                }
                                else
                                {
                                    pucPtr[0] = bgcolor.B;
                                    pucPtr[1] = bgcolor.G;
                                    pucPtr[2] = bgcolor.R;
                                }
                            }
                            else
                            {
                                if (isreverse)
                                {
                                    pucPtr[0] = bgcolor.B;
                                    pucPtr[1] = bgcolor.G;
                                    pucPtr[2] = bgcolor.R;
                                }
                                else
                                {
                                    pucPtr[0] = pucPtr[0];
                                }
                            }


                            pucPtr += 4;
                            pucPtr1 += 4;

                            x++;
                        }

                        pucStart += iStride;
                        pucStart1 += iStride1;

                        y++;
                    }

                    bmp.UnlockBits(bmpData);
                    bmpmask.UnlockBits(bmpData1);
                }
            }
            //catch (Exception e)
            //{
            //    bmp.UnlockBits(bmpData);
            //    bmpMask.UnlockBits(bmpData1);
            //}
        }
        protected string PointFToString(PointF ptf)
        {
            return ptf.X.ToString() + "," + ptf.Y.ToString();
        }
        protected PointF StringToPointF(string str)
        {
            string[] strs = str.Split(',');

            PointF retptf = new PointF();

            retptf.X = float.Parse(strs[0]);
            retptf.Y = float.Parse(strs[1]);

            return retptf;
        }
        public ShapeEnum ToShapeEnum()
        {
            ShapeEnum retshape = ShapeEnum.RECT;

            switch(m_figure)
            {
                case Figure_EAG.Rectangle:
                    retshape = ShapeEnum.RECT;
                    break;
                case Figure_EAG.Circle:
                    retshape = ShapeEnum.CIRCLE;
                    break;
                case Figure_EAG.ChatoyantPolygon:
                    retshape = ShapeEnum.POLY;
                    break;
                case Figure_EAG.Strip:
                    retshape = ShapeEnum.CAPSULE;
                    break;
                case Figure_EAG.Ring:
                    retshape = ShapeEnum.RING;
                    break;
                case Figure_EAG.ORing:
                    retshape = ShapeEnum.ORING;
                    break;
                case Figure_EAG.RectRect:
                    retshape = ShapeEnum.RECTRECT;
                    break;
                case Figure_EAG.HexHex:
                    retshape = ShapeEnum.HEXHEX;
                    break;
                case Figure_EAG.RectO:
                    retshape = ShapeEnum.RECTO;
                    break;
                case Figure_EAG.HexO:
                    retshape = ShapeEnum.HEXO;
                    break;
            }
            return retshape;
        }

        /// <summary>
        /// ���w���P�h�~�ت��C��
        /// </summary>
        /// <param name="orgpen"></param>
        public void ChangePenColorShape(Pen orgpen)
        {
            switch(RelateLevel % 8)
            {
                case 1:
                    orgpen.Color = Color.Orange;
                    orgpen.DashStyle = DashStyle.Dot;
                    break;
                case 2:
                    orgpen.Color = Color.Lime;
                    orgpen.DashStyle = DashStyle.Solid;
                    break;
                case 3:
                    orgpen.Color = Color.Pink;
                    orgpen.DashStyle = DashStyle.Solid;
                    break;
                case 4:
                    orgpen.Color = Color.LightGreen;
                    orgpen.DashStyle = DashStyle.Solid;
                    break;
                case 5:
                    orgpen.Color = Color.Lavender;
                    orgpen.DashStyle = DashStyle.Solid;
                    break;
                case 6:
                    orgpen.Color = Color.DeepPink;
                    orgpen.DashStyle = DashStyle.Solid;
                    break;
                case 7:
                    orgpen.Color = Color.Red;
                    orgpen.DashStyle = DashStyle.Solid;
                    break;
                case 0:
                    orgpen.Color = Color.Gold;
                    orgpen.DashStyle = DashStyle.Solid;
                    break;
            }
        }


    }
}
