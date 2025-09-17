using JetEazy.FormSpace;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Media;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace JetEazy.BasicSpace
{
    public class NetToolsHelper
    {
        /// <summary>
        /// 结构：表示一个点
        /// </summary>
        public struct PointHelper
        {
            //横、纵坐标
            public double x, y;
            //构造函数
            public PointHelper(double x, double y)
            {
                this.x = x;
                this.y = y;
            }
            //该点到指定点pTarget的距离
            public double DistanceTo(PointHelper p)
            {
                return Math.Sqrt((p.x - x) * (p.x - x) + (p.y - y) * (p.y - y));
            }
            //重写ToString方法
            public override string ToString()
            {
                return string.Concat("Point (",
                 this.x.ToString("#0.000"), ',',
                 this.y.ToString("#0.000"), ')');
            }
        }
        /// <summary>
        /// 计算点P(x,y)与X轴正方向的夹角
        /// </summary>
        /// <param name="x">横坐标</param>
        /// <param name="y">纵坐标</param>
        /// <returns>夹角弧度</returns>
        private static double radPOX(double x, double y)
        {
            //P在(0,0)的情况
            if (x == 0 && y == 0) return 0;
            //P在四个坐标轴上的情况：x正、x负、y正、y负
            if (y == 0 && x > 0) return 0;
            if (y == 0 && x < 0) return Math.PI;
            if (x == 0 && y > 0) return Math.PI / 2;
            if (x == 0 && y < 0) return Math.PI / 2 * 3;
            //点在第一、二、三、四象限时的情况
            if (x > 0 && y > 0) return Math.Atan(y / x);
            if (x < 0 && y > 0) return Math.PI - Math.Atan(y / -x);
            if (x < 0 && y < 0) return Math.PI + Math.Atan(-y / -x);
            if (x > 0 && y < 0) return Math.PI * 2 - Math.Atan(-y / x);
            return 0;
        }
        /// <summary>
        /// 返回点P围绕点A旋转弧度rad后的坐标
        /// </summary>
        /// <param name="P">待旋转点坐标</param>
        /// <param name="A">旋转中心坐标</param>
        /// <param name="rad">旋转弧度</param>
        /// <param name="isClockwise">true:顺时针/false:逆时针</param>
        /// <returns>旋转后坐标</returns>
        public static PointHelper RotatePoint(PointHelper P, PointHelper A, double rad, bool isClockwise = true)
        {
            //点Temp1
            PointHelper Temp1 = new PointHelper(P.x - A.x, P.y - A.y);
            //点Temp1到原点的长度
            double lenO2Temp1 = Temp1.DistanceTo(new PointHelper(0, 0));
            //∠T1OX弧度
            double angT1OX = radPOX(Temp1.x, Temp1.y);
            //∠T2OX弧度（T2为T1以O为圆心旋转弧度rad）
            double angT2OX = angT1OX - (isClockwise ? 1 : -1) * rad;
            //点Temp2
            PointHelper Temp2 = new PointHelper(
             lenO2Temp1 * Math.Cos(angT2OX),
             lenO2Temp1 * Math.Sin(angT2OX));
            //点Q
            return new PointHelper(Temp2.x + A.x, Temp2.y + A.y);
        }


    }
    public class VsMSG
    {
        #region SINGLETON
        private static readonly VsMSG _instance = new VsMSG();
        VsMessageBox _messageBox = null;
        #endregion

        public static VsMSG Instance
        {
            get { return _instance; }
        }

        /// <summary>
        /// 询问视窗
        /// </summary>
        /// <param name="msg">提示信息</param>
        /// <returns>返回OK  和 Cancel </returns>
        public DialogResult Question(string msg)
        {
            _messageBox = new VsMessageBox(msg, false);
            return _messageBox.ShowDialog();
        }
        public void Warning(string msg, bool iswarning = true)
        {
            _messageBox = new VsMessageBox(msg, iswarning);
            _messageBox.ShowDialog();
        }
        public void Tishi(string msg)
        {
            _messageBox = new VsMessageBox(msg, "1");
            _messageBox.ShowDialog();
        }
    }

    public class DibToBitmap
    {

        /// &lt;summary&gt;
        /// Convert DIB to Bitmap.
        /// &lt;/summary&gt;
        /// &lt;param name=&quot;dibPtrArg&quot;&gt;Pointer to memory DIB, starting with BITMAPINFOHEADER.&lt;/param&gt;
        /// &lt;returns&gt;A Bitmap&lt;/returns&gt;
        public static Bitmap Convert(IntPtr dibPtrArg)
        {
            BITMAPINFOHEADER bmi;
            IntPtr pixptr;

            GetPixelInfo(dibPtrArg, out pixptr, out bmi);

            Bitmap bitMap = new Bitmap(bmi.biWidth, bmi.biHeight);

            Graphics scannedImageGraphics = Graphics.FromImage(bitMap);

            IntPtr hdc = scannedImageGraphics.GetHdc();

            SetDIBitsToDevice(
               hdc,
               0,             // XDest
               0,             // YDest
               bmi.biWidth,
               bmi.biHeight,
               0,             // XSrc
               0,             // YSrc
               0,             // uStartScan
               bmi.biHeight,  // cScanLines
               pixptr,        // lpvBits
               dibPtrArg,     // lpbmi
               0);            // 0 = literal RGB values rather than palette

            scannedImageGraphics.ReleaseHdc(hdc);

            const float inchPerMeter = 39.3700787F;
            bitMap.SetResolution(bmi.biXPelsPerMeter / inchPerMeter, bmi.biYPelsPerMeter / inchPerMeter);

            // bitMap.Save(@&quot;c:\0\2.bmp&quot;, Universal.GlobalImageFormat); // debug code

            return bitMap;
        }
        static private void GetPixelInfo(IntPtr bmpptr, out IntPtr pix, out BITMAPINFOHEADER bmi)
        {

            bmi = new BITMAPINFOHEADER();
            Marshal.PtrToStructure(bmpptr, bmi); // copy into struct.

            if (bmi.biSizeImage == 0)
            {
                bmi.biSizeImage = ((((bmi.biWidth * bmi.biBitCount) + 31) & ~31) >> 3) * bmi.biHeight;
            }

            int p = bmi.biClrUsed;

            if ((p == 0) && (bmi.biBitCount <= 8))
            {
                p = 1 << bmi.biBitCount;
            }

            pix = (IntPtr)((p * 4) + bmi.biSize + (int)bmpptr);

        }

        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        private class BITMAPINFOHEADER
        {
            public int biSize;
            public int biWidth;
            public int biHeight;
            public short biPlanes;
            public short biBitCount;
            public int biCompression;
            public int biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public int biClrUsed;
            public int biClrImportant;
        }

        [DllImport("gdi32.dll", ExactSpelling = true)]
        internal static extern int SetDIBitsToDevice(
           IntPtr hdc,
           int xdst,
           int ydst,
           int width,
           int height,
           int xsrc,
           int ysrc,
           int start,
           int lines,
           IntPtr bitsptr,
           IntPtr bmiptr,
           int color);

    } // class DibToImage
    //public class ReportDataClass
    //{
    //    public bool IsOK = false;
    //    public string ReportString = "";

    //    public ReasonEnum myReasion = ReasonEnum.PASS;
    //    public List<ReasonEnum> myReasonList = new List<ReasonEnum>();

    //    public List<string> ReportList = new List<string>();

    //    public ReportDataClass()
    //    {

    //    }

    //    public void Reset()
    //    {
    //        IsOK = false;
    //        ReportString = "";
    //        ReportList.Clear();

    //        myReasion = ReasonEnum.PASS;
    //        myReasonList.Clear();
    //    }
    //}
    public class JzToolsClass
    {
        [DllImport("user32.dll", EntryPoint = "ShowCursor", CharSet = CharSet.Auto)]
        public extern static void ShowCursor(int status);

        public static void myShowCursor(int status)
        {
            ShowCursor(status);
        }


        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SwapMouseButton([param: MarshalAs(UnmanagedType.Bool)] bool fSwap);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(long dwFlags, long dx, long dy, long cButtons, long dwExtraInfo);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling=true)]
        public static extern bool ClipCursor(ref Rectangle rect);

        [DllImport("GdiPlus.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern int GdipCreateBitmapFromGdiDib(IntPtr pBIH, IntPtr pPix, out IntPtr pBitmap);

        [DllImport("AspriseOCR.dll", EntryPoint = "OCR", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr OCR(string file, int type);

        [DllImport("AspriseOCR.dll", EntryPoint = "OCRpart", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr OCRpart(string file, int type, int startX, int startY, int width, int height);

        [DllImport("AspriseOCR.dll", EntryPoint = "OCRBarCodes", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr OCRBarCodes(string file, int type);

        [DllImport("AspriseOCR.dll", EntryPoint = "OCRpartBarCodes", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr OCRpartBarCodes(string file, int type, int startX, int startY, int width, int height);

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;

        public static int PassingInteger = 0;
        public static string PassingString = "";
        public static string PassingBarcode = "";
        public static string PassingStartTime = "";
        public static string PassingCCCCBarcode = "";
        public static string PassingHIPICCCCBarcode = "";

        public static Color UsedColor = Color.FromArgb(255, 192, 192);
        public static Color NormalColor = Color.FromArgb(192, 255, 192);
        public static void DoMouseClick()
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }
        public static void DoMouseLeftDown()
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
        }
        public static void DoMouseLeftUp()
        {
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }
        public static void DoMouseRightUp()
        {
            mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
        }
        public static void SwapMouse(bool IsSwap)
        {
            SwapMouseButton(IsSwap);
        }

        public static string PathPicker(string Description, string DefaultPath)
        {
            string retStr = "";

            FolderBrowserDialog fd = new FolderBrowserDialog();
            fd.Description = Description;
            fd.ShowNewFolderButton = false;
            fd.SelectedPath = DefaultPath;

            if (fd.ShowDialog().Equals(DialogResult.OK))
            {
                if (fd.SelectedPath != "")
                    retStr = fd.SelectedPath;
            }
            else
                retStr = "";

            return retStr;
        }
        public static string OpenFilePicker(string DefaultPath, string DefaultName)
        {
            string retStr = "";

            OpenFileDialog dlg = new OpenFileDialog();

            //dlg.Filter = "BMP Files (*.bmp)|*.BMP|" + "All files (*.*)|*.*";
            dlg.Filter = DefaultPath;
            dlg.FileName = DefaultName;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                retStr = dlg.FileName;
            }
            return retStr;
        }
        public static string OpenFilePicker(string DefaultPath, string DefaultName, string DefaultDir)
        {
            string retStr = "";

            OpenFileDialog dlg = new OpenFileDialog();

            //dlg.Filter = "BMP Files (*.bmp)|*.BMP|" + "All files (*.*)|*.*";
            dlg.Filter = DefaultPath;
            dlg.FileName = DefaultName;
            dlg.InitialDirectory = DefaultDir;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                retStr = dlg.FileName;
            }
            return retStr;
        }
        public static string SaveFilePicker(string DefaultPath, string DefaultName)
        {
            string retStr = "";

            SaveFileDialog dlg = new SaveFileDialog();

            //dlg.Filter = "BMP Files (*.bmp)|*.BMP|" + "All files (*.*)|*.*";
            dlg.Filter = DefaultPath;
            dlg.FileName = DefaultName;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                retStr = dlg.FileName;
            }
            return retStr;
        }

        public static void LockCursor(Rectangle rect)
        {
            ClipCursor(ref rect);
        }
        public static void Playing(string str_Path)
        {
            if (!File.Exists(str_Path))
                return;

            SoundPlayer player = new SoundPlayer();
            player.SoundLocation = str_Path;
            player.Load();
            player.Play();
            player.PlaySync();
        }
        public static void PlayPassWav(string passpath)
        {
            //string passpath = @"D:\AUTOMATION\Eazy AOI\R12\SOUND\Pass.wav";

            if (!File.Exists(passpath))    //file is not exist , then return.
                return;

            SoundPlayer player = new SoundPlayer();
            player.SoundLocation = passpath;
            player.Load();
            player.Play();
            player.PlaySync();
        }
        public JzToolsClass()
        {

        }
        public string ColorInformation(Color mColor)
        {
            string Str = "";

            Str += "亮度:" + Grayscale(mColor.R, mColor.G, mColor.B).ToString().PadLeft(3) + " ,";
            Str += "紅色:" + mColor.R.ToString().PadLeft(3) + " ,";
            Str += "綠色:" + mColor.G.ToString().PadLeft(3) + " ,";
            Str += "藍色:" + mColor.B.ToString().PadLeft(3) + "";

            return Str;
        }
        public string ColortoString(Color Colr)
        {
            return Colr.R.ToString() + "," + Colr.G.ToString() + "," + Colr.B.ToString();
        }
        public uint ColorValue(Color Colr)
        {
            return (uint)((Colr.A << 24) | (Colr.R << 16) |
               (Colr.G << 8) | (Colr.B << 0));
        }
        public byte Grayscale(byte R, byte G, byte B)
        {
            return (byte)((double)R * 0.3 + (double)G * 0.59 + (double)B * 0.11);
        }
        public int GrayscaleInt(byte R, byte G, byte B)
        {
            return (int)((double)R * 0.3 + (double)G * 0.59 + (double)B * 0.11);
        }
        public void BoundRect(ref Rectangle InnerRect, Size BoundSize)
        {
            InnerRect.X = Math.Min(Math.Max(InnerRect.X, 0), (BoundSize.Width - InnerRect.Width < 0 ? 0 : BoundSize.Width - InnerRect.Width));
            InnerRect.Y = Math.Min(Math.Max(InnerRect.Y, 0), (BoundSize.Height - InnerRect.Height < 0 ? 0 : BoundSize.Height - InnerRect.Height));

            if (BoundSize.Width <= InnerRect.X + InnerRect.Width)
                InnerRect.Width = BoundValue(InnerRect.Width, BoundSize.Width - InnerRect.X, 1);
            if (BoundSize.Height <= InnerRect.Height + InnerRect.Height)
                InnerRect.Height = BoundValue(InnerRect.Height, BoundSize.Height - InnerRect.Y, 1);
        }
        public void BoundRect(ref RectangleF InnerRect, Size BoundSize)
        {
            InnerRect.X = Math.Min(Math.Max(InnerRect.X, 0), (BoundSize.Width - InnerRect.Width < 0 ? 0 : BoundSize.Width - InnerRect.Width));
            InnerRect.Y = Math.Min(Math.Max(InnerRect.Y, 0), (BoundSize.Height - InnerRect.Height < 0 ? 0 : BoundSize.Height - InnerRect.Height));

            if (BoundSize.Width <= InnerRect.X + InnerRect.Width)
                InnerRect.Width = BoundValue(InnerRect.Width, BoundSize.Width - InnerRect.X, 1);
            if (BoundSize.Height <= InnerRect.Height + InnerRect.Height)
                InnerRect.Height = BoundValue(InnerRect.Height, BoundSize.Height - InnerRect.Y, 1);
        }
        public Rectangle BoundRect(Rectangle InnerRect, Size BoundSize)
        {
            InnerRect.X = Math.Min(Math.Max(InnerRect.X, 0), (BoundSize.Width - InnerRect.Width < 0 ? 0 : BoundSize.Width - InnerRect.Width));
            InnerRect.Y = Math.Min(Math.Max(InnerRect.Y, 0), (BoundSize.Height - InnerRect.Height < 0 ? 0 : BoundSize.Height - InnerRect.Height));

            if (BoundSize.Width <= InnerRect.X + InnerRect.Width)
                InnerRect.Width = BoundValue(InnerRect.Width, BoundSize.Width - InnerRect.X, 1);
            if (BoundSize.Height <= InnerRect.Height + InnerRect.Height)
                InnerRect.Height = BoundValue(InnerRect.Height, BoundSize.Height - InnerRect.Y, 1);

            return InnerRect;
        }
        public void GetBMP(string BMPFileStr, ref Bitmap BMP)
        {
            Bitmap bmpTMP = new Bitmap(BMPFileStr);

            BMP.Dispose();
            BMP = new Bitmap(bmpTMP);

            bmpTMP.Dispose();
        }
        public void GetBMP(Rectangle rect, ref Bitmap BMP)
        {
            Bitmap bmpTMP = new Bitmap(BMP);

            BMP.Dispose();

            rect = BoundRect(rect, bmpTMP.Size);

            BMP = bmpTMP.Clone(rect, PixelFormat.Format32bppPArgb);

            bmpTMP.Dispose();
        }
        public void SaveBMP(string BMPFileStr, ref Bitmap BMP, ImageFormat imgformat)
        {
            Bitmap bmpTMP = new Bitmap(BMP);

            bmpTMP.Save(BMPFileStr, imgformat);

            bmpTMP.Dispose();
        }

        public void SetBrightContrast(Bitmap bmp, int brightvalue, int contrastvalue)
        {
            SetBrightContrast(bmp, SimpleRect(bmp.Size), brightvalue, contrastvalue);
        }
        public void SetBrightContrast(Bitmap bmp, Rectangle Rect, int brightvalue, int contrastvalue)
        {
            if (brightvalue == 0 && contrastvalue == 0)
                return;

            int Grade = 0;
            double contrast = (100.0 + contrastvalue) / 100.0;
            contrast *= contrast;

            double ContrastGrade = 0;

            Rectangle rectbmp = Rect;
            BitmapData bmpData = bmp.LockBits(rectbmp, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            IntPtr Scan0 = bmpData.Scan0;

            try
            {
                unsafe
                {
                    byte* scan0 = (byte*)(void*)Scan0;
                    byte* pucPtr;
                    byte* pucStart;

                    int xmin = rectbmp.X;
                    int ymin = rectbmp.Y;
                    int xmax = xmin + rectbmp.Width;
                    int ymax = ymin + rectbmp.Height;

                    int x = xmin;
                    int y = ymin;
                    int iStride = bmpData.Stride;

                    y = ymin;
                    pucStart = scan0 + ((x - xmin) << 2) + (iStride * (y - ymin));

                    while (y < ymax)
                    {
                        x = xmin;
                        pucPtr = pucStart;
                        while (x < xmax)
                        {
                            Grade = (int)pucPtr[2];

                            if (brightvalue != 0)
                            {
                                Grade += brightvalue;
                                Grade = Math.Min(255, Math.Max(0, Grade));
                            }

                            ContrastGrade = (double)Grade;

                            if (contrastvalue != 0)
                            {
                                ContrastGrade /= 255d;
                                ContrastGrade -= 0.5;
                                ContrastGrade *= (double)contrast;
                                ContrastGrade += 0.5;
                                ContrastGrade *= 255;

                                ContrastGrade = Math.Min(255, Math.Max(0, ContrastGrade));
                            }

                            pucPtr[2] = (byte)ContrastGrade;
                            pucPtr[1] = (byte)ContrastGrade;
                            pucPtr[0] = (byte)ContrastGrade;

                            pucPtr += 4;
                            x++;
                        }

                        pucStart += iStride;
                        y++;
                    }

                    bmp.UnlockBits(bmpData);
                }
            }
            catch (Exception ex)
            {
                //JetEazy.LoggerClass.Instance.WriteException(ex);
                string Str = ex.ToString();

                bmp.UnlockBits(bmpData);
            }
        }
        public void SetBrightContrast(Bitmap bmp, Rectangle Rect, int brightvalue, int contrastvalue, int basevalue, int minvalue, float ratio)
        {
            if (brightvalue == 0 && contrastvalue == 0)
                return;

            int Grade = 0;
            double contrast = (100.0 + contrastvalue) / 100.0;
            contrast *= contrast;

            double ContrastGrade = 0;

            Rectangle rectbmp = Rect;
            BitmapData bmpData = bmp.LockBits(rectbmp, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            IntPtr Scan0 = bmpData.Scan0;

            try
            {
                unsafe
                {
                    byte* scan0 = (byte*)(void*)Scan0;
                    byte* pucPtr;
                    byte* pucStart;

                    int xmin = rectbmp.X;
                    int ymin = rectbmp.Y;
                    int xmax = xmin + rectbmp.Width;
                    int ymax = ymin + rectbmp.Height;

                    int x = xmin;
                    int y = ymin;
                    int iStride = bmpData.Stride;

                    y = ymin;
                    pucStart = scan0 + ((x - xmin) << 2) + (iStride * (y - ymin));

                    while (y < ymax)
                    {
                        x = xmin;
                        pucPtr = pucStart;
                        while (x < xmax)
                        {
                            Grade = (int)pucPtr[2];

                            Grade = basevalue + (int)(((float)(Grade - minvalue)) * ratio);

                            if (brightvalue != 0)
                            {
                                Grade += brightvalue;
                                Grade = Math.Min(255, Math.Max(0, Grade));
                            }

                            ContrastGrade = (double)Grade;

                            if (contrastvalue != 0)
                            {
                                ContrastGrade /= 255d;
                                ContrastGrade -= 0.5;
                                ContrastGrade *= (double)contrast;
                                ContrastGrade += 0.5;
                                ContrastGrade *= 255;

                                ContrastGrade = Math.Min(255, Math.Max(0, ContrastGrade));
                            }

                            pucPtr[2] = (byte)ContrastGrade;
                            pucPtr[1] = (byte)ContrastGrade;
                            pucPtr[0] = (byte)ContrastGrade;

                            pucPtr += 4;
                            x++;
                        }

                        pucStart += iStride;
                        y++;
                    }

                    bmp.UnlockBits(bmpData);
                }
            }
            catch (Exception ex)
            {
                //JetEazy.LoggerClass.Instance.WriteException(ex);
                string Str = ex.ToString();

                bmp.UnlockBits(bmpData);
            }
        }

        public Rectangle Resize(Rectangle rect, double Ratio)
        {
            int rectw = (int)(rect.Width * Ratio / 2);
            int recth = (int)(rect.Height * Ratio / 2);

            rect.Inflate(rectw, recth);

            return rect;
        }
        public RectangleF Resize(RectangleF rect, double Ratio)
        {
            float rectw = (float)(rect.Width * Ratio / 2);
            float recth = (float)(rect.Height * Ratio / 2);

            rect.Inflate(rectw, recth);

            return rect;
        }
        public Rectangle ResizeWithLocation(Rectangle rect,float ratio)
        {
            return new Rectangle((int)((float)rect.X * ratio), (int)((float)rect.Y * ratio), (int)((float)rect.Width * ratio), (int)((float)rect.Height * ratio));
        }
        public RectangleF ResizeWithLocation(RectangleF rect, float ratio)
        {
            return new RectangleF(((float)rect.X * ratio), ((float)rect.Y * ratio), ((float)rect.Width * ratio), ((float)rect.Height * ratio));
        }
        public RectangleF ResizeWithLocation2(Rectangle rect, int ratio)
        {
            Size retSize;
            PointF retPtF;

            if (ratio > 0)
            {
                retPtF = new PointF(rect.X << ratio, rect.Y << ratio);
                retSize = new Size(rect.Width << ratio, rect.Height << ratio);
            }
            else
            {
                retPtF = new PointF(rect.X >> -ratio, rect.Y >> -ratio);
                retSize = new Size(rect.Width >> -ratio, rect.Height >> -ratio);
            }

            retSize.Width = Math.Max(retSize.Width, 1);
            retSize.Height = Math.Max(retSize.Height, 1);

            return new RectangleF(retPtF.X, retPtF.Y, retSize.Width, retSize.Height);
        }
        public RectangleF ResizeWithLocation2(RectangleF rect, int ratio)
        {
            Rectangle rectangle = new Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
            return ResizeWithLocation2(rectangle, ratio);
        }

        public PointF ResizeWithLocation2(PointF ptf, int ratio)
        {
            //Rectangle rectangle = new Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
            Point point = new Point((int)ptf.X, (int)ptf.Y);
            return ResizeWithLocation2(point, ratio);
        }
        public PointF ResizeWithLocation2(Point pt, int ratio)
        {
            Size retSize;
            PointF retPtF;

            if (ratio > 0)
            {
                retPtF = new PointF(pt.X << ratio, pt.Y << ratio);
                //retSize = new Size(rect.Width << ratio, rect.Height << ratio);
            }
            else
            {
                retPtF = new PointF(pt.X >> -ratio, pt.Y >> -ratio);
                //retSize = new Size(rect.Width >> -ratio, rect.Height >> -ratio);
            }

            //retSize.Width = Math.Max(retSize.Width, 1);
            //retSize.Height = Math.Max(retSize.Height, 1);
            return retPtF;
            //return new RectangleF(retPtF.X, retPtF.Y, retSize.Width, retSize.Height);
        }
        public Rectangle SimpleRect(Size Sz)
        {
            return new Rectangle(0, 0, Sz.Width, Sz.Height);
        }
        public Rectangle SimpleRect(Rectangle Rect)
        {
            return new Rectangle(0, 0, Rect.Width, Rect.Height);
        }
        public Rectangle SimpleRect(Point Pt)
        {
            return new Rectangle(Pt.X, Pt.Y, 1, 1);
        }
        public Rectangle SimpleRect(Point Pt, int SizeValue)
        {
            Rectangle rect = new Rectangle(Pt.X - SizeValue, Pt.Y - SizeValue, SizeValue << 1, SizeValue << 1);
            return rect;
        }
        public Rectangle SimpleRect(PointF PtF, int SizeValue)
        {
            Rectangle rect = new Rectangle((int)PtF.X - SizeValue, (int)PtF.Y - SizeValue, SizeValue << 1, SizeValue << 1);
            return rect;
        }
        public Rectangle GetCenterBiasRect(Rectangle FromRect, Point FromPt, Point ToPt)
        {
            Rectangle rect = FromRect;

            rect.X += (ToPt.X - FromPt.X);
            rect.Y += (ToPt.Y - FromPt.Y);

            return rect;
        }
        public int BoundValue(int Value, int Max, int Min)
        {
            return Math.Max(Math.Min(Value, Max), Min);

        }
        public float BoundValue(float Value, float Max, float Min)
        {
            return Math.Max(Math.Min(Value, Max), Min);

        }
        public Rectangle SimpleRect(Point Pt, int Width, int Height)
        {
            Rectangle rect = SimpleRect(Pt);
            rect.Inflate(Width, Height);

            return rect;
        }
        public Point SubPoint(Point Pt1, Point Pt2)
        {
            return new Point(Pt1.X - Pt2.X, Pt1.Y - Pt2.Y);
        }
        public int GetPointLength(Point P1, Point P2)
        {
            return (int)Math.Sqrt(Math.Pow((P1.X - P2.X), 2) + Math.Pow((P1.Y - P2.Y), 2));
        }
        public double GetPointLengthD(Point P1, Point P2)
        {
            return Math.Sqrt((double)Math.Pow((P1.X - P2.X), 2) + Math.Pow((P1.Y - P2.Y), 2));
        }
        public double GetPointLength(PointF P1, PointF P2)
        {
            return Math.Sqrt((double)Math.Pow((P1.X - P2.X), 2) + Math.Pow((P1.Y - P2.Y), 2));
        }

        public double GetLTSuggestion(Point P1, Point P2, Rectangle Rect)
        {
            double Xp = (double)(P1.X - P2.X) / (double)(Rect.Width >> 1);
            double Yp = (double)(P1.Y - P2.Y) / (double)(Rect.Height >> 1);

            return Math.Pow(Xp, 2) + Math.Pow(Yp, 2);
        }
        public double GetPointTan(Point P1, Point P2)
        {
            //return (int)Math.Sqrt(Math.Pow((P1.X - P2.X), 2) + Math.Pow((P1.Y - P2.Y), 2));

            //if (P1.X == P2.X)
            //    return 90;
            //else
            return Math.Atan((double)(P1.Y - P2.Y) / ((double)(P1.X - P2.X))) * 180 / Math.PI;
        }
        public PointF GetCenterPoint(PointF P1, PointF P2)
        {
            return new PointF((P1.X + P2.X) / 2, (P1.Y + P2.Y) / 2);
        }

        public void SaveData(string DataStr, string FileName)
        {
            File.WriteAllText(FileName, DataStr, Encoding.Default);
        }
        /// <summary>
        /// 保存數據，文檔存在則追加，反之，新建并寫入
        /// </summary>
        /// <param name="DataStr">寫入的數據</param>
        /// <param name="FileName">檔案名稱目錄</param>
        public void SaveDataEX(string DataStr, string FileName)
        {
            System.IO.StreamWriter stm = null;

            try
            {
                stm = new System.IO.StreamWriter(FileName, true, System.Text.Encoding.Default);
                stm.Write(DataStr);
                stm.Flush();
                stm.Close();
                stm.Dispose();
                stm = null;
            }
            catch(Exception ex)
            {
                //JetEazy.LoggerClass.Instance.WriteException(ex);
            }

            if (stm != null)
                stm.Dispose();
        }
        /// <summary>
        /// 保存數據，文檔存在則追加一行，反之，新建并寫入
        /// </summary>
        /// <param name="DataStr">寫入的數據</param>
        /// <param name="FileName">檔案名稱目錄</param>
        public void SaveDataEXD(string DataStr, string FileName)
        {
            System.IO.StreamWriter stm = null;

            try
            {
                stm = new System.IO.StreamWriter(FileName, true, System.Text.Encoding.Default);
                stm.WriteLine(DataStr);
                stm.Flush();
                stm.Close();
                stm.Dispose();
                stm = null;
            }
            catch (Exception ex)
            {
                //JetEazy.LoggerClass.Instance.WriteException(ex);
            }

            if (stm != null)
                stm.Dispose();
        }
        public void AppendData(string appendstr, string FileName)
        {
            StreamWriter Sw = File.AppendText(FileName);
            Sw.WriteLine(appendstr);
            Sw.Close();
        }
        public void SaveData_2DBarcode(string DataStr, string FileName)
        {
            StreamWriter Swr = new StreamWriter(FileName, true, Encoding.Default);

            Swr.Write(DataStr);

            Swr.Flush();
            Swr.Close();
        }
        public void ReadData(ref string DataStr, string FileName)
        {
            FileStream fs = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.None);
            StreamReader Srr = new StreamReader(fs, Encoding.Default);

            DataStr = Srr.ReadToEnd();

            Srr.Close();
            Srr.Dispose();
        }
        public void SaveData(string DataStr, string FileName, bool append)
        {
            StreamWriter Swr = new StreamWriter(FileName, append, Encoding.Default);

            Swr.Write(DataStr);

            Swr.Flush();
            Swr.Close();
            Swr.Dispose();
        }
        public bool IsPointInRect(Point Pt, Rectangle Rect)
        {
            return (Pt.X >= Rect.X) && (Pt.X < (Rect.X + Rect.Width)) && (Pt.Y >= Rect.Y) && (Pt.Y < (Rect.Y + Rect.Height));
        }
        public bool IsPointInRect(PointF Pt, RectangleF Rect)
        {
            return (Pt.X >= Rect.X) && (Pt.X < (Rect.X + Rect.Width)) && (Pt.Y >= Rect.Y) && (Pt.Y < (Rect.Y + Rect.Height));
        }
        public bool IsTwoPtFTheSame(PointF PtFOrg, PointF PtFAfter, float Range)
        {
            RectangleF rectf = PointFToRectF(PtFOrg, Range);
            return (IsPointInRect(PtFAfter, rectf));
        }
        public bool IsRectInMyRect(Rectangle Rect, Rectangle MyRect)
        {
            return IsPointInRect(Rect.Location, MyRect) && IsPointInRect(new Point(Rect.Location.X + Rect.Width, Rect.Y), MyRect) && IsPointInRect(new Point(Rect.Location.X, Rect.Y + Rect.Height), MyRect) && IsPointInRect(new Point(Rect.Location.X + Rect.Width, Rect.Y + Rect.Height), MyRect);
        }
        public bool IsRectInMyRect(RectangleF RectF, RectangleF MyRectF)
        {
            Rectangle rect = new Rectangle((int)RectF.X, (int)RectF.Y, (int)RectF.Width, (int)RectF.Height);
            Rectangle myrect = new Rectangle((int)MyRectF.X, (int)MyRectF.Y, (int)MyRectF.Width, (int)MyRectF.Height);

            return IsRectInMyRect(rect, myrect);
        }
        public bool IsRectCenterInMyRect(RectangleF RectF, RectangleF MyRectF)
        {
            Rectangle rect = new Rectangle((int)RectF.X, (int)RectF.Y, (int)RectF.Width, (int)RectF.Height);
            Rectangle myrect = new Rectangle((int)MyRectF.X, (int)MyRectF.Y, (int)MyRectF.Width, (int)MyRectF.Height);

            return IsPointInRect(GetRectCenter(rect), myrect);
        }
        public Point GetRectCenter(Rectangle Rect)
        {
            return new Point(Rect.X + (Rect.Width >> 1), Rect.Y + (Rect.Height >> 1));
        }
        public int GetRectArea(Rectangle Rect)
        {
            return Rect.Width * Rect.Height;
        }
        public int GetRectArea(Rectangle Rect, double Ratio)
        {
            return (int)(Rect.Width * Rect.Height * Ratio);
        }
        public PointF GetRectFCenter(RectangleF RectF)
        {
            return new PointF(RectF.X + (RectF.Width / 2), RectF.Y + (RectF.Height / 2));
        }
        public double GetRectFArea(RectangleF RectF)
        {
            return RectF.Width * RectF.Height;
        }
        public double GetRecFtArea(RectangleF RectF, double Ratio)
        {
            return (double)(RectF.Width * RectF.Height * Ratio);
        }

        public PointF PointFConvert(Point PT, float ratio)
        {
            PointF PTF = new PointF();

            PTF.X = (float)PT.X / ratio;
            PTF.Y = (float)PT.Y / ratio;

            return PTF;
        }
        public Point PointFConvert(PointF PTF)
        {
            return new Point((int)PTF.X, (int)PTF.Y);
        }
        public RectangleF RectTwoPoint(PointF StartPoint, PointF EndPoint)
        {
            PointF RecEndPoint = StartPoint;
            PointF RecStartPoint = EndPoint;
            RectangleF Rect = new RectangleF(StartPoint, new Size(1, 1));

            if (RecEndPoint.X >= RecStartPoint.X && RecEndPoint.Y >= RecStartPoint.Y)
            {
                Rect.X = RecStartPoint.X + 1;
                Rect.Y = RecStartPoint.Y + 1;
                Rect.Width = RecEndPoint.X - RecStartPoint.X - 1;
                Rect.Height = RecEndPoint.Y - RecStartPoint.Y - 1;
            }
            else if (RecEndPoint.X > RecStartPoint.X && RecEndPoint.Y < RecStartPoint.Y)
            {
                Rect.X = RecStartPoint.X + 1;
                Rect.Y = RecEndPoint.Y + 1;
                Rect.Width = RecEndPoint.X - RecStartPoint.X - 1;
                Rect.Height = RecStartPoint.Y - RecEndPoint.Y - 1;
            }
            else if (RecEndPoint.X < RecStartPoint.X && RecEndPoint.Y < RecStartPoint.Y)
            {
                Rect.X = RecEndPoint.X + 1;
                Rect.Y = RecEndPoint.Y + 1;
                Rect.Width = RecStartPoint.X - RecEndPoint.X - 1;
                Rect.Height = RecStartPoint.Y - RecEndPoint.Y - 1;
            }
            else if (RecEndPoint.X < RecStartPoint.X && RecEndPoint.Y > RecStartPoint.Y)
            {
                Rect.X = RecEndPoint.X + 1;
                Rect.Y = RecStartPoint.Y + 1;
                Rect.Width = RecStartPoint.X - RecEndPoint.X - 1;
                Rect.Height = RecEndPoint.Y - RecStartPoint.Y - 1;
            }

            return Rect;
        }

        public RectangleF PointFToRectF(PointF PTF, float length)
        {
            PointF PTFTMP = PTF;

            PTFTMP.X = PTFTMP.X - length / 2;
            PTFTMP.Y = PTFTMP.Y - length / 2;

            return new RectangleF(PTFTMP, new SizeF(length, length));
        }
        public RectangleF PointFToRectF(PointF PTF, float length, float Ratio)
        {
            PointF PTFTMP = PTF;

            PTFTMP.X = PTFTMP.X - ((length / Ratio) / 2);
            PTFTMP.Y = PTFTMP.Y - ((length / Ratio) / 2);

            return new RectangleF(PTFTMP, new SizeF(length / Ratio, length / Ratio));
        }
        public string PointFToString(PointF PTF)
        {
            return PTF.X.ToString() + "," + PTF.Y.ToString();
        }
        public string PointF000ToString(PointF PTF)
        {
            return PTF.X.ToString("0.000") + "," + PTF.Y.ToString("0.000");
        }
        public PointF StringToPointF(string Str)
        {
            string[] strs = Str.Split(',');
            return new PointF(float.Parse(strs[0]), float.Parse(strs[1]));
        }
        //public string CornerEXToStr(CornerExEnum corner)
        //{
        //    string retStr = "";
        //    switch (corner)
        //    {
        //        case CornerExEnum.LT:
        //            retStr = "左上";
        //            break;
        //        case CornerExEnum.LB:
        //            retStr = "左下";
        //            break;
        //        case CornerExEnum.RT:
        //            retStr = "右上";
        //            break;
        //        case CornerExEnum.RB:
        //            retStr = "右下";
        //            break;
        //        case CornerExEnum.MPT:
        //            retStr = "量測";
        //            break;
        //        case CornerExEnum.DPT:
        //            retStr = "定點";
        //            break;
        //        case CornerExEnum.PT1:
        //        case CornerExEnum.PT2:
        //        case CornerExEnum.PT3:
        //        case CornerExEnum.PT4:
        //        case CornerExEnum.PT5:
        //        case CornerExEnum.PT6:

        //            retStr = "測點" + corner.ToString().Substring(corner.ToString().Length - 1, 1);
        //            break;
        //    }

        //    return retStr;
        //}
        //public string CornerToStr(CornerEnum corner)
        //{
        //    string retStr = "";
        //    switch (corner)
        //    {
        //        case CornerEnum.LT:
        //            retStr = "左上";
        //            break;
        //        case CornerEnum.LB:
        //            retStr = "左下";
        //            break;
        //        case CornerEnum.RT:
        //            retStr = "右上";
        //            break;
        //        case CornerEnum.RB:
        //            retStr = "右下";
        //            break;
        //    }

        //    return retStr;
        //}
        //public CornerEnum StrToCorner(string str)
        //{
        //    CornerEnum retCorner = CornerEnum.LT;
        //    switch (str)
        //    {
        //        case "左上":
        //            retCorner = CornerEnum.LT;
        //            break;
        //        case "左下":
        //            retCorner = CornerEnum.LB;
        //            break;
        //        case "右上":
        //            retCorner = CornerEnum.RT;
        //            break;
        //        case "右下":
        //            retCorner = CornerEnum.RB;
        //            break;
        //    }

        //    return retCorner;
        //}
        //public CornerExEnum StrToCornerEx(string str)
        //{
        //    CornerExEnum retCorner = CornerExEnum.LT;
        //    switch (str)
        //    {
        //        case "左上":
        //            retCorner = CornerExEnum.LT;
        //            break;
        //        case "左下":
        //            retCorner = CornerExEnum.LB;
        //            break;
        //        case "右上":
        //            retCorner = CornerExEnum.RT;
        //            break;
        //        case "右下":
        //            retCorner = CornerExEnum.RB;
        //            break;
        //        case "量測":
        //            retCorner = CornerExEnum.MPT;
        //            break;
        //        case "定點":
        //            retCorner = CornerExEnum.DPT;
        //            break;
        //        default:
        //            retCorner = (CornerExEnum)(int.Parse(str.Substring(str.Length - 1, 1)) + 3);
        //            break;
        //    }

        //    return retCorner;
        //}

        //public Rectangle CornerRect(Rectangle Rect, CornerEnum Corner, int CornerSize)
        //{
        //    switch (Corner)
        //    {
        //        case CornerEnum.LT:
        //            return new Rectangle(Rect.X - (CornerSize >> 1), Rect.Y - (CornerSize >> 1), CornerSize, CornerSize);
        //        case CornerEnum.RT:
        //            return new Rectangle(Rect.X + Rect.Width - (CornerSize >> 1), Rect.Y - (CornerSize >> 1), CornerSize, CornerSize);
        //        case CornerEnum.LB:
        //            return new Rectangle(Rect.X - (CornerSize >> 1), Rect.Y + Rect.Height - (CornerSize >> 1), CornerSize, CornerSize);
        //        case CornerEnum.RB:
        //            return new Rectangle(Rect.X + Rect.Width - (CornerSize >> 1), Rect.Y + Rect.Height - (CornerSize >> 1), CornerSize, CornerSize);
        //    }

        //    return new Rectangle(Rect.X - (CornerSize >> 1), Rect.Y - (CornerSize >> 1), CornerSize, CornerSize);
        //}
        public string PointToString(Point PT)
        {
            return PT.X.ToString() + "," + PT.Y.ToString();
        }

        public Size StringToSize(string str)
        {
            int[] sizevalue = Array.ConvertAll(str.Split(','), int.Parse);

            return new Size(sizevalue[0], sizevalue[1]);
        }
        public Point StringToPoint(string Str)
        {
            string[] strs = Str.Split(',');
            return new Point(int.Parse(strs[0]), int.Parse(strs[1]));
        }
        public Rectangle StringtoRect(string RectStr)
        {
            string[] str = RectStr.Split(',');
            return new Rectangle(int.Parse(str[0]), int.Parse(str[1]), int.Parse(str[2]), int.Parse(str[3]));
        }
        public string GetLastString(string Str, int NoFromLast)
        {
            return Str.Substring(Str.Length - NoFromLast, NoFromLast);
        }
        public string PointtoString(PointF Pt)
        {
            return Pt.X.ToString().PadLeft(8) + "," + Pt.Y.ToString().PadLeft(8);
        }
        public string PointtoString(MouseEventArgs e)
        {
            return e.X.ToString().PadLeft(4) + "," + e.Y.ToString().PadLeft(4);
        }
        public string SizetoString(Size rSize)
        {
            return rSize.Width.ToString() + "," + rSize.Height.ToString();
        }

        public string RecttoStringSimple(Rectangle Rect)
        {
            return Rect.X.ToString() + "," + Rect.Y.ToString() + "," + Rect.Width.ToString() + "," + Rect.Height.ToString();
        }
        public string RecttoString(Rectangle Rect)
        {
            return Rect.X.ToString().PadLeft(4) + "," + Rect.Y.ToString().PadLeft(4) + "," + Rect.Width.ToString().PadLeft(4) + "," + Rect.Height.ToString().PadLeft(4);
        }
        public string RecttoStringEX(Rectangle Rect)
        {
            return Rect.X.ToString("0000") + "," + Rect.Y.ToString("0000") + "," + Rect.Width.ToString("0000") + "," + Rect.Height.ToString("0000");
        }

        public string RectFToString(RectangleF RectF)
        {
            string Str = "";

            Str += RectF.X.ToString("0.00") + ",";
            Str += RectF.Y.ToString("0.00") + ",";
            Str += RectF.Width.ToString("0.00") + ",";
            Str += RectF.Height.ToString("0.00");

            return Str;
        }
        public RectangleF StringToRectF(string Str)
        {
            string[] strs = Str.Split(',');
            RectangleF rectF = new RectangleF();

            rectF.X = float.Parse(strs[0]);
            rectF.Y = float.Parse(strs[1]);
            rectF.Width = float.Parse(strs[2]);
            rectF.Height = float.Parse(strs[3]);

            return rectF;


        }
        public Rectangle RectFToRect(RectangleF RectF)
        {
            Rectangle rect = new Rectangle((int)RectF.X, (int)RectF.Y, (int)RectF.Width, (int)RectF.Height);

            return rect;
        }
        public RectangleF RectToRectF(Rectangle Rect)
        {
            RectangleF rectF = new RectangleF((float)Rect.X, (float)Rect.Y, (float)Rect.Width, (float)Rect.Height);

            return rectF;
        }
        public Rectangle RectTwoPoint(Point StartPoint, Point EndPoint, Size BoundSize)
        {
            Point RecEndPoint = StartPoint;
            Point RecStartPoint = EndPoint;
            Rectangle Rect = new Rectangle();

            if (RecEndPoint.X >= RecStartPoint.X && RecEndPoint.Y >= RecStartPoint.Y)
            {
                Rect.X = RecStartPoint.X + 1;
                Rect.Y = RecStartPoint.Y + 1;
                Rect.Width = RecEndPoint.X - RecStartPoint.X - 1;
                Rect.Height = RecEndPoint.Y - RecStartPoint.Y - 1;
            }
            else if (RecEndPoint.X > RecStartPoint.X && RecEndPoint.Y < RecStartPoint.Y)
            {
                Rect.X = RecStartPoint.X + 1;
                Rect.Y = RecEndPoint.Y + 1;
                Rect.Width = RecEndPoint.X - RecStartPoint.X - 1;
                Rect.Height = RecStartPoint.Y - RecEndPoint.Y - 1;
            }
            else if (RecEndPoint.X < RecStartPoint.X && RecEndPoint.Y < RecStartPoint.Y)
            {
                Rect.X = RecEndPoint.X + 1;
                Rect.Y = RecEndPoint.Y + 1;
                Rect.Width = RecStartPoint.X - RecEndPoint.X - 1;
                Rect.Height = RecStartPoint.Y - RecEndPoint.Y - 1;
            }
            else if (RecEndPoint.X < RecStartPoint.X && RecEndPoint.Y > RecStartPoint.Y)
            {
                Rect.X = RecEndPoint.X + 1;
                Rect.Y = RecStartPoint.Y + 1;
                Rect.Width = RecStartPoint.X - RecEndPoint.X - 1;
                Rect.Height = RecEndPoint.Y - RecStartPoint.Y - 1;
            }

            if (Rect.X < 0)
                Rect.X = 0;
            if ((Rect.X + Rect.Width) > BoundSize.Width)
                Rect.Width = BoundSize.Width - Rect.X;
            if (Rect.Width <= 0)
                Rect.Width = 1;

            if (Rect.Y < 0)
                Rect.Y = 0;
            if ((Rect.Y + Rect.Height) > BoundSize.Height)
                Rect.Height = BoundSize.Height - Rect.Y;

            if (Rect.Height <= 0)
                Rect.Height = 1;
            return Rect;
        }
        public Rectangle RectTwoPoint(Point StartPoint, Point EndPoint)
        {
            Point RecEndPoint = StartPoint;
            Point RecStartPoint = EndPoint;
            Rectangle Rect = new Rectangle();

            if (RecEndPoint.X >= RecStartPoint.X && RecEndPoint.Y >= RecStartPoint.Y)
            {
                Rect.X = RecStartPoint.X + 1;
                Rect.Y = RecStartPoint.Y + 1;
                Rect.Width = RecEndPoint.X - RecStartPoint.X - 1;
                Rect.Height = RecEndPoint.Y - RecStartPoint.Y - 1;
            }
            else if (RecEndPoint.X >= RecStartPoint.X && RecEndPoint.Y <= RecStartPoint.Y)
            {
                Rect.X = RecStartPoint.X + 1;
                Rect.Y = RecEndPoint.Y + 1;
                Rect.Width = RecEndPoint.X - RecStartPoint.X - 1;
                Rect.Height = RecStartPoint.Y - RecEndPoint.Y - 1;
            }
            else if (RecEndPoint.X <= RecStartPoint.X && RecEndPoint.Y <= RecStartPoint.Y)
            {
                Rect.X = RecEndPoint.X + 1;
                Rect.Y = RecEndPoint.Y + 1;
                Rect.Width = RecStartPoint.X - RecEndPoint.X - 1;
                Rect.Height = RecStartPoint.Y - RecEndPoint.Y - 1;
            }
            else if (RecEndPoint.X <= RecStartPoint.X && RecEndPoint.Y >= RecStartPoint.Y)
            {
                Rect.X = RecEndPoint.X + 1;
                Rect.Y = RecStartPoint.Y + 1;
                Rect.Width = RecStartPoint.X - RecEndPoint.X - 1;
                Rect.Height = RecEndPoint.Y - RecStartPoint.Y - 1;
            }

            if (Rect.X < 0)
                Rect.X = 0;
            if (Rect.Width <= 0)
                Rect.Width = 1;

            if (Rect.Y < 0)
                Rect.Y = 0;

            if (Rect.Height <= 0)
                Rect.Height = 1;
            return Rect;

        }

        //public RectangleF GetPartRectangleF(RectangleF rectFrom, PositionEnum position, int Ratio)
        //{
        //    RectangleF rectf = new RectangleF();

        //    float MinLength = Math.Min(rectFrom.Width, rectFrom.Height);

        //    switch (position)
        //    {
        //        case PositionEnum.XDir:
        //            rectf.Width = rectFrom.Width;
        //            rectf.Height = MinLength * (float)Ratio / 100f;

        //            rectf.X = rectFrom.X;
        //            rectf.Y = rectFrom.Y + (rectFrom.Height / 2) - (rectf.Height / 2);
        //            break;
        //        case PositionEnum.YDir:
        //            rectf.Width = MinLength * (float)Ratio / 100f;
        //            rectf.Height = rectFrom.Height;

        //            rectf.X = rectFrom.X + (rectFrom.Width / 2) - (rectf.Width / 2);
        //            rectf.Y = rectFrom.Y;

        //            break;
        //        case PositionEnum.LeftTop:

        //            rectf.Width = MinLength * (float)Ratio / 100f;
        //            rectf.Height = MinLength * (float)Ratio / 100f;

        //            rectf.X = rectFrom.X;
        //            rectf.Y = rectFrom.Y;
        //            break;
        //        case PositionEnum.LeftBottom:

        //            rectf.Width = MinLength * (float)Ratio / 100f;
        //            rectf.Height = MinLength * (float)Ratio / 100f;

        //            rectf.X = rectFrom.X;
        //            rectf.Y = rectFrom.Bottom - rectf.Height;

        //            break;
        //        case PositionEnum.RightTop:

        //            rectf.Width = MinLength * (float)Ratio / 100f;
        //            rectf.Height = MinLength * (float)Ratio / 100f;

        //            rectf.X = rectFrom.Right - rectf.Width;
        //            rectf.Y = rectFrom.Y;

        //            break;
        //        case PositionEnum.RightBottom:

        //            rectf.Width = MinLength * (float)Ratio / 100f;
        //            rectf.Height = MinLength * (float)Ratio / 100f;

        //            rectf.X = rectFrom.Right - rectf.Width;
        //            rectf.Y = rectFrom.Bottom - rectf.Height;

        //            break;
        //    }

        //    return rectf;
        //}
        //public RectangleF GetPartRectangleF(RectangleF rectFrom, PositionEnum position, int Ratio, int Extend)
        //{
        //    RectangleF rectf = new RectangleF();

        //    float MinLength = Math.Min(rectFrom.Width, rectFrom.Height);

        //    switch (position)
        //    {
        //        case PositionEnum.XDir:
        //            rectf.Width = rectFrom.Width;
        //            rectf.Height = MinLength * (float)Ratio / 100f;

        //            rectf.X = rectFrom.X;
        //            rectf.Y = rectFrom.Y + (rectFrom.Height / 2) - (rectf.Height / 2);

        //            rectf.Inflate(Extend, Extend);
        //            break;
        //        case PositionEnum.YDir:
        //            rectf.Width = MinLength * (float)Ratio / 100f;
        //            rectf.Height = rectFrom.Height;

        //            rectf.X = rectFrom.X + (rectFrom.Width / 2) - (rectf.Width / 2);
        //            rectf.Y = rectFrom.Y;

        //            rectf.Inflate(Extend, Extend);
        //            break;
        //        case PositionEnum.LeftTop:

        //            rectf.Width = MinLength * (float)Ratio / 100f;
        //            rectf.Height = MinLength * (float)Ratio / 100f;

        //            rectf.X = rectFrom.X;
        //            rectf.Y = rectFrom.Y;

        //            rectf.Inflate(Extend, Extend);
        //            break;
        //        case PositionEnum.LeftBottom:

        //            rectf.Width = MinLength * (float)Ratio / 100f;
        //            rectf.Height = MinLength * (float)Ratio / 100f;

        //            rectf.X = rectFrom.X;
        //            rectf.Y = rectFrom.Bottom - rectf.Height;

        //            rectf.Inflate(Extend, Extend);
        //            break;
        //        case PositionEnum.RightTop:

        //            rectf.Width = MinLength * (float)Ratio / 100f;
        //            rectf.Height = MinLength * (float)Ratio / 100f;

        //            rectf.X = rectFrom.Right - rectf.Width;
        //            rectf.Y = rectFrom.Y;

        //            rectf.Inflate(Extend, Extend);
        //            break;
        //        case PositionEnum.RightBottom:

        //            rectf.Width = MinLength * (float)Ratio / 100f;
        //            rectf.Height = MinLength * (float)Ratio / 100f;

        //            rectf.X = rectFrom.Right - rectf.Width;
        //            rectf.Y = rectFrom.Bottom - rectf.Height;

        //            rectf.Inflate(Extend, Extend);
        //            break;
        //    }

        //    return rectf;
        //}

        public bool IsInRange(double FromValue, double CompValue, double DiffValue)
        {
            return Math.Abs(FromValue - CompValue) < DiffValue;
        }
        public bool IsInRange(float FromValue, float CompValue, float DiffValue)
        {
            return Math.Abs(FromValue - CompValue) < DiffValue;
        }
        public bool IsInRangeRatio(int FromValue, int CompValue, int DiffValueRatio)
        {
            return Math.Abs(FromValue - CompValue) < DiffValueRatio;
        }
        //public static bool IsInRange(double FromValue, double CompValue, double DiffValue)
        //{
        //    return (FromValue >= CompValue - DiffValue) && (FromValue <= CompValue + DiffValue);
        //}
        public bool IsInRangeRatio(double FromValue, double CompValue, double Ratio)
        {
            return (FromValue >= (CompValue * (1 - (Ratio / 100d)))) && (FromValue <= (CompValue * (1 + (Ratio / 100d))));
        }
        public bool IsInRangeRatioUpper(double FromValue, double CompValue, double Ratio)
        {
            return (CompValue >= (FromValue * (1 + (Ratio / 100d))));
        }

        public bool IsInRangeEx(int FromValue, int MaxValue, int MinValue)
        {
            return (FromValue >= MinValue) && (FromValue <= MaxValue);
        }
        public bool IsInRangeEx(double FromValue, double MaxValue, double MinValue)
        {
            return (FromValue >= MinValue) && (FromValue <= MaxValue);
        }

        public bool IsRectFIntersection(List<RectangleF> RectFList, RectangleF RectF)
        {
            bool ret = false;

            foreach (RectangleF rectf in RectFList)
            {
                if (rectf.IntersectsWith(RectF))
                {
                    ret = true;
                    break;
                }
            }
            return ret;
        }
        public void MergeRectF(List<RectangleF> InputRectFList, ref List<RectangleF> OutputRectFList, bool IsIntersectionMerge, int MergeEnlarge)
        {
            RectangleF rectfrom = new RectangleF();
            RectangleF rectfnew = new RectangleF();

            bool[] IsMerged = new bool[InputRectFList.Count];

            OutputRectFList.Clear();

            int i = 0;
            int j = 0;
            while (i < InputRectFList.Count)
            {
                if (!IsMerged[i])
                {
                    rectfrom = InputRectFList[i];
                    j = i + 1;
                    while (j < InputRectFList.Count)
                    {
                        rectfnew = InputRectFList[j];

                        if (IsIntersectionMerge)
                            rectfnew.Inflate(MergeEnlarge, MergeEnlarge);

                        if (rectfrom.IntersectsWith(rectfnew) || !IsIntersectionMerge)
                        {
                            rectfrom = MergeTwoRects(rectfrom, InputRectFList[j]);
                            IsMerged[j] = true;
                        }

                        j++;
                    }
                    OutputRectFList.Add(rectfrom);
                }
                i++;
            }
        }

        public Rectangle MergeTwoRects(Rectangle rect1, Rectangle rect2)
        {
            Rectangle rect = new Rectangle();

            if (rect1.Width == 0)
                return rect2;
            if (rect2.Width == 0)
                return rect1;

            rect.X = Math.Min(rect1.X, rect2.X);
            rect.Y = Math.Min(rect1.Y, rect2.Y);

            rect.Width = Math.Max(rect1.X + rect1.Width, rect2.X + rect2.Width) - rect.X;
            rect.Height = Math.Max(rect1.Y + rect1.Height, rect2.Y + rect2.Height) - rect.Y;

            return rect;
        }
        public RectangleF MergeTwoRects(RectangleF rect1, RectangleF rect2)
        {
            RectangleF rect = new RectangleF();

            if (rect1.Width == 0)
                return rect2;
            if (rect2.Width == 0)
                return rect1;

            rect.X = Math.Min(rect1.X, rect2.X);
            rect.Y = Math.Min(rect1.Y, rect2.Y);

            rect.Width = Math.Max(rect1.X + rect1.Width, rect2.X + rect2.Width) - rect.X;
            rect.Height = Math.Max(rect1.Y + rect1.Height, rect2.Y + rect2.Height) - rect.Y;

            return rect;
        }

        public Rectangle Resize(Rectangle OrgRect, int Ratio)
        {
            Rectangle rect = new Rectangle();

            rect.X = ShiftValue(OrgRect.X, Ratio);
            rect.Y = ShiftValue(OrgRect.Y, Ratio);
            rect.Width = Math.Max(ShiftValue(OrgRect.Width, Ratio), 1);
            rect.Height = Math.Max(ShiftValue(OrgRect.Height, Ratio), 1);

            return rect;
        }
        public Size ExtendSize(Size OrgSize, int Extend)
        {
            return new Size(OrgSize.Width + Extend, OrgSize.Height + Extend);
        }
        public Rectangle Resize(Size OrgSize, int Shift, Point BiasPoint, Point MouseLocation, Size BoundSize)
        {
            Size SizeTmp = new Size();
            Point NBiasPoint = new Point();

            if (Shift > 0)
                SizeTmp = new Size(OrgSize.Width << Shift, OrgSize.Height << Shift);
            else
                SizeTmp = new Size(OrgSize.Width >> -Shift, OrgSize.Height >> -Shift);

            if (SizeTmp.Width <= BoundSize.Width && SizeTmp.Height <= BoundSize.Height)
                return new Rectangle(0, 0, OrgSize.Width, OrgSize.Height);

            else
            {
                if (Shift > 0)
                {
                    SizeTmp = new Size(BoundSize.Width >> Shift, BoundSize.Height >> Shift);
                    NBiasPoint = new Point(BiasPoint.X >> Shift, BiasPoint.Y >> Shift);
                }
                else
                {
                    SizeTmp = new Size(BoundSize.Width << -Shift, BoundSize.Height << -Shift);
                    NBiasPoint = new Point(BiasPoint.X << -Shift, BiasPoint.Y << -Shift);
                }

                int X = (MouseLocation.X - NBiasPoint.X < 0 ? 0 : MouseLocation.X - NBiasPoint.X);
                int Y = (MouseLocation.Y - NBiasPoint.Y < 0 ? 0 : MouseLocation.Y - NBiasPoint.Y);

                return new Rectangle(X, Y, SizeTmp.Width + 10, SizeTmp.Height + 10);
            }
        }
        public Size Resize(Size OrgSize, int Shift, Size BoundSize)
        {
            Size SizeTmp = new Size();

            if (Shift > 0)
                SizeTmp = new Size(OrgSize.Width << Shift, OrgSize.Height << Shift);
            else
                SizeTmp = new Size(OrgSize.Width >> -Shift, OrgSize.Height >> -Shift);

            if (SizeTmp.Width <= BoundSize.Width && SizeTmp.Height <= BoundSize.Height)
                return SizeTmp;
            else
                return BoundSize;
        }
        public Size Resize(Size OrgSize, int Ratio)
        {
            Size retSize;

            if (Ratio > 0)
                retSize = new Size(OrgSize.Width << Ratio, OrgSize.Height << Ratio);
            else
                retSize = new Size(OrgSize.Width >> -Ratio, OrgSize.Height >> -Ratio);

            retSize.Width = Math.Max(retSize.Width, 1);
            retSize.Height = Math.Max(retSize.Height, 1);

            return retSize;
        }
        public int ShiftValue(int Value, int Ratio)
        {
            if (Ratio >= 0)
                return Value << Ratio;
            else
                return Value >> (-Ratio);
        }
        public void ClearPoint(ref Point rPoint)
        {
            rPoint.X = 0;
            rPoint.Y = 0;
        }
        public void ClearSize(ref Size rSize)
        {
            rSize.Width = 0;
            rSize.Height = 0;
        }
        public void SetSize(ref Size SizeEnd, Point PointStart, Point PointEnd)
        {
            SetSize(ref SizeEnd, PointStart, PointEnd, false);
        }
        public void SetSize(ref Size SizeEnd, Point PointStart, Point PointEnd, bool IsWholeDirection)
        {
            SizeEnd.Width = PointStart.X - PointEnd.X;
            SizeEnd.Height = PointStart.Y - PointEnd.Y;

            if (IsWholeDirection)
            {
                SizeEnd.Width = -SizeEnd.Width;
                SizeEnd.Height = -SizeEnd.Height;
            }
        }
        public void SetPoint(ref Point rPt, MouseEventArgs e)
        {
            rPt.X = e.X;
            rPt.Y = e.Y;
        }
        public bool CheckTextBoxIsInteger(TextBox txtBox, int MaxValue, int MinValue)
        {
            int i = 0;
            bool ret = false;

            if (int.TryParse(txtBox.Text, out i))
            {
                if (i < MinValue || i > MaxValue)
                {
                    MessageBox.Show("輸入值需要在" + MinValue.ToString() + "及" + MaxValue.ToString() + "之間", "MAIN", MessageBoxButtons.OK);
                }
                else
                    ret = true;
            }
            else
            {
                MessageBox.Show("錯誤的輸入值。", "MAIN", MessageBoxButtons.OK);
            }

            return ret;
        }
        public bool CheckTextBoxIsDouble(TextBox txtBox, double MaxValue, double MinValue)
        {
            double i = 0;
            bool ret = false;

            if (double.TryParse(txtBox.Text, out i))
            {
                if (i < MinValue || i > MaxValue)
                {
                    MessageBox.Show("輸入值需要在" + MinValue.ToString() + "及" + MaxValue.ToString() + "之間", "MAIN", MessageBoxButtons.OK);
                }
                else
                    ret = true;
            }
            else
            {
                MessageBox.Show("錯誤的輸入值。", "MAIN", MessageBoxButtons.OK);
            }

            return ret;
        }

        public void DrawRect(ref Bitmap BMP, List<RectangleF> RectFList, Pen RectPen, double Ratio)
        {
            if (RectFList.Count > 0)
            {
                Graphics g = Graphics.FromImage(BMP);

                foreach (RectangleF rectF in RectFList)
                    g.DrawRectangle(RectPen, Resize(RectFToRect(rectF), Ratio));

                g.Dispose();
            }
        }
        public void DrawRect(ref Bitmap BMP, List<RectangleF> RectFList, Pen ColorPen)
        {
            if (RectFList.Count > 0)
            {
                Graphics g = Graphics.FromImage(BMP);

                foreach (RectangleF rectF in RectFList)
                    g.DrawRectangle(ColorPen, RectFToRect(rectF));

                g.Dispose();
            }
        }

        public void DrawRect(ref Bitmap BMP, List<Rectangle> RectList, Pen ColorPen, int maxcount)
        {
            int i = 0;

            if (RectList.Count > 0)
            {
                Graphics g = Graphics.FromImage(BMP);

                foreach (Rectangle rect in RectList)
                {
                    g.DrawRectangle(ColorPen, rect);

                    i++;

                    if (i > maxcount)
                        break;
                }

                g.Dispose();
            }
        }
        public void DrawRect(ref Bitmap BMP, List<RectangleF> RectFList, List<int> IndexList, Pen ColorPen)
        {
            if (RectFList.Count > 0)
            {
                int i = 0;
                Graphics g = Graphics.FromImage(BMP);
                Font MyFont = new Font("Arial", 64);
                SolidBrush B = new SolidBrush(Color.Red);
                Pen GPen = new Pen(Color.Lime, 6);

                foreach (RectangleF rectF in RectFList)
                {
                    if (IndexList[i] != -1)
                    {
                        g.DrawRectangle(ColorPen, RectFToRect(rectF));
                        g.DrawString((IndexList[i] + 1).ToString(), MyFont, B, rectF);
                    }
                    else
                    {
                        g.DrawRectangle(GPen, RectFToRect(rectF));
                    }
                    i++;
                }

                g.Dispose();
            }
        }
        public void DrawRect(ref Bitmap BMP, List<RectangleF> RectFList, List<int> IndexList, List<string> DescriptionList, Pen ColorPen)
        {
            if (RectFList.Count > 0)
            {
                int i = 0;
                Graphics g = Graphics.FromImage(BMP);
                Font MyFont = new Font("Arial", 64);
                SolidBrush B = new SolidBrush(Color.Red);
                Pen GPen = new Pen(Color.Lime, 6);

                foreach (RectangleF rectF in RectFList)
                {
                    if (DescriptionList[i] != "完美")
                    {
                        g.DrawRectangle(ColorPen, RectFToRect(rectF));
                        g.DrawString((IndexList[i] + 1).ToString(), MyFont, B, rectF);
                    }
                    else
                    {
                        g.DrawRectangle(GPen, RectFToRect(rectF));
                    }
                    i++;
                }

                g.Dispose();
            }
        }
        public void DrawRect(PictureBox pic, Rectangle rect, Pen RoundPen)
        {
            Graphics g = pic.CreateGraphics();
            g.DrawRectangle(RoundPen, rect);
            g.Dispose();
        }
        public void DrawRect(PictureBox pic, Rectangle[] rect, Pen RoundPen)
        {
            if (rect.Length != 0)
            {
                Graphics g = pic.CreateGraphics();
                g.DrawRectangles(RoundPen, rect);
                g.Dispose();
            }
        }
        public void DrawCross(ref Bitmap BMP, Rectangle Rect, Pen P)
        {
            Graphics g = Graphics.FromImage(BMP);
            //g.DrawRectangle(P, Rect);
            g.DrawLine(P, new Point(Rect.Left, Rect.Top), new Point(Rect.Right, Rect.Bottom));
            g.DrawLine(P, new Point(Rect.Left, Rect.Bottom), new Point(Rect.Right, Rect.Top));
            g.Dispose();
        }
        public void DrawCircle(ref Bitmap BMP, Rectangle rect, SolidBrush brushcolor)
        {
            Graphics g = Graphics.FromImage(BMP);
            g.FillEllipse(brushcolor, rect);
            g.Dispose();

        }
        public void DrawCircle(ref Bitmap BMP, Rectangle rect, Color pencolor, int linewidth)
        {
            Graphics g = Graphics.FromImage(BMP);
            g.DrawEllipse(new Pen(Color.Red, linewidth), rect);
            g.Dispose();

        }
        public void DrawRect(Bitmap BMP, Pen RoundPen)
        {
            DrawRect(BMP, new Rectangle((int)RoundPen.Width >> 1, (int)RoundPen.Width >> 1, BMP.Width - ((int)RoundPen.Width), BMP.Height - ((int)RoundPen.Width)), RoundPen);
        }
        public void DrawRect(Bitmap BMP, Rectangle Rect, SolidBrush B)
        {
            Graphics g = Graphics.FromImage(BMP);
            g.FillRectangle(B, Rect);
            g.Dispose();
        }
        public void DrawRect(Bitmap BMP, string[] RectStr, SolidBrush B)
        {
            Graphics g = Graphics.FromImage(BMP);
            int i = 0;
            while (i < RectStr.Length)
            {
                g.FillRectangle(B, StringtoRect(RectStr[i]));
                i++;
            }
            g.Dispose();
        }
        public void DrawRect(Bitmap BMP, Rectangle Rect, Pen RoundPen, int Enlarge)
        {
            DrawRect(BMP, new Rectangle(Rect.X - Enlarge, Rect.Y - Enlarge, ((int)Rect.Width) + (Enlarge << 1), ((int)Rect.Height) + (Enlarge << 1)), RoundPen);
        }
        public void DrawRectEx(Bitmap BMP, Rectangle Rect, Pen RoundPen)
        {
            DrawRect(BMP, new Rectangle(Rect.X, Rect.Y, ((int)Rect.Width), ((int)Rect.Height)), RoundPen);
        }
        public void DrawRectEx(Bitmap BMP, RectangleF Rect, Pen RoundPen)
        {
            DrawRect(BMP, new Rectangle((int)Rect.X, (int)Rect.Y, ((int)Rect.Width), ((int)Rect.Height)), RoundPen);
        }
        public void DrawRect(Bitmap BMP, Rectangle Rect, Pen RoundPen)
        {
            Graphics g = Graphics.FromImage(BMP);
            g.DrawRectangle(RoundPen, Rect);
            g.Dispose();
        }
        public void DrawRect(Graphics g, Rectangle Rect, Color PenColor, int LineSize)
        {
            g.DrawRectangle(new Pen(PenColor, (float)LineSize), Rect);
        }
        public void DrawLine(Bitmap BMP, Pen P, Point FromPt, Point ToPt)
        {
            Graphics g = Graphics.FromImage(BMP);
            g.DrawLine(P, FromPt, ToPt);
            g.Dispose();
        }

        public void DrawText(Bitmap BMP, string Text, SolidBrush B, Font MyFont, PointF Locate)
        {
            Graphics g = Graphics.FromImage(BMP);
            g.DrawString(Text, MyFont, B, new PointF(Locate.X - 20, Locate.Y - 10));
            g.Dispose();
        }
        public void DrawText(Bitmap BMP, string Text)
        {
            SolidBrush B = new SolidBrush(Color.Lime);
            Font MyFont = new Font("Arial", 24);

            Graphics g = Graphics.FromImage(BMP);
            g.DrawString(Text, MyFont, B, new PointF(5, 5));
            g.Dispose();
        }
        public void DrawText(Bitmap BMP, string Text, bool IsFromBottom)
        {
            SolidBrush B = new SolidBrush(Color.Red);
            Font MyFont = new Font("Arial", 24);

            Graphics g = Graphics.FromImage(BMP);

            g.FillRectangle(Brushes.Yellow, new Rectangle(5, BMP.Height - 50, 400, 40));

            if (!IsFromBottom)
                g.DrawString(Text, MyFont, B, new PointF(5, 5));
            else
                g.DrawString(Text, MyFont, B, new PointF(5, BMP.Height - 50));

            g.Dispose();
        }
        public void DrawText(Bitmap BMP, string Text, Point Pt, int FontSize, Color Clor)
        {
            SolidBrush B = new SolidBrush(Clor);
            Font MyFont = new Font("Arial", FontSize);

            Graphics g = Graphics.FromImage(BMP);
            g.DrawString(Text, MyFont, B, new PointF(Pt.X, Pt.Y));
            g.Dispose();
        }
        public void DrawImage(ref Bitmap ToBMP, ref Bitmap FromBMP, Rectangle destRect)
        {
            Graphics g = Graphics.FromImage(ToBMP);

            g.DrawImage(FromBMP, destRect, SimpleRect(FromBMP.Size), GraphicsUnit.Pixel);

            g.Dispose();
        }
        public void DrawImage(Bitmap BMPFrom, Bitmap BMPTo, Rectangle DestRect)
        {
            Graphics g = Graphics.FromImage(BMPTo);
            g.DrawImage(BMPFrom, DestRect, SimpleRect(BMPFrom.Size), GraphicsUnit.Pixel);
            g.Dispose();
        }
        public string RemoveLastChar(string Str, int Count)
        {
            if (Str.Length < Count)
                return "";

            return Str.Remove(Str.Length - Count, Count);
        }
        public PointF RectFCenter(RectangleF Rectf)
        {
            RectangleF rectf = Rectf;

            return new PointF(rectf.X + rectf.Width / 2, rectf.Y + rectf.Height / 2);
        }
        public double GetRandom(double minval, double maxval)
        {
            Random rnd = new Random();

            double diffval = maxval - minval;

            diffval = diffval * (double)rnd.Next(1, 17) / 17d;

            return minval + diffval;
        }
        public void DrawArrow(Bitmap BMP, Pen P, Point StartPoint, Point EndPoint)
        {
            P.StartCap = LineCap.ArrowAnchor;
            P.EndCap = LineCap.SquareAnchor;

            Graphics g = Graphics.FromImage(BMP);
            g.DrawLine(P, StartPoint, EndPoint);

            g.Dispose();
        }
        public string ArrayToString(bool[] array)
        {
            string Str = "";

            foreach (bool cell in array)
            {
                Str += (cell ? "1," : "0,");
            }
            return RemoveLastChar(Str, 1);
        }
        public void StringToArray(string Str, ref bool[] array)
        {
            string[] strs = Str.Split(',');

            int i = 0;

            foreach (string str in strs)
            {
                array[i] = (str == "1");

                i++;
            }
        }

        public PointF GetRectCenterF(RectangleF Rect)
        {
            return new PointF(Rect.X + (Rect.Width / 2f), Rect.Y + (Rect.Height / 2f));
        }
        public Point GetRectCenter(Size Size)
        {
            Rectangle Rect = SimpleRect(Size);
            return new Point(Rect.X + (int)Round((double)Rect.Width / 2d), Rect.Y + (int)Round((double)Rect.Height / 2d));
        }
        public double Round(double FromValue)
        {
            double ret = Math.Floor(FromValue);

            if (FromValue - ret >= 0.5)
            {
                ret = Math.Ceiling(FromValue);
            }
            else
            {
                ret = Math.Floor(FromValue);
            }

            return ret;
        }
        public double RoundDown(double Org, int digit)
        {
            string Str = Org.ToString("0.000000");
            return double.Parse(RemoveLastChar(Str, 6 - digit));
        }
        public string[] GetDiretory(string directory)
        {
            return Directory.GetDirectories(directory);
        }

        public void CreateDirectory(string PathName)
        {
            if (!Directory.Exists(PathName))
                Directory.CreateDirectory(PathName);
        }
        public void CreateFile(string FileName)
        {
            if (!File.Exists(FileName))
            {
                SaveData("", FileName);
            }
        }
        public bool IsFileExist(string filepath)
        {
            return File.Exists(filepath);
        }
        public void CGOperate()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
        public bool RectIsTheSame(Rectangle OrgRect, Rectangle ComRect, int Percent)
        {
            bool ret = true;
            double UB = (100 + (double)Percent) / 100;
            double LB = (100 - (double)Percent) / 100;

            ret = ret & (((int)((double)OrgRect.Width * UB)) >= ComRect.Width && ((int)((double)OrgRect.Width * LB)) <= ComRect.Width);
            ret = ret & (((int)((double)OrgRect.Height * UB)) >= ComRect.Height && ((int)((double)OrgRect.Height * LB)) <= ComRect.Height);
            ret = ret & (((int)((double)GetRectArea(OrgRect) * UB)) >= GetRectArea(ComRect) && ((int)((double)GetRectArea(OrgRect) * LB)) <= GetRectArea(ComRect));

            return ret;
        }
        public bool RectIsTheSameEx(Rectangle OrgRect, Rectangle ComRect, int Percent)
        {
            bool ret = true;
            double UB = (100 + (double)Percent) / 100;
            double LB = (100 - (double)Percent) / 100;

            ret = ret & (((int)((double)OrgRect.Width * UB)) >= ComRect.Width && ((int)((double)OrgRect.Width * LB)) <= ComRect.Width);
            ret = ret & (((int)((double)OrgRect.Height * UB)) >= ComRect.Height && ((int)((double)OrgRect.Height * LB)) <= ComRect.Height);
            //ret = ret & (((int)((double)RectAreaSize(OrgRect) * UB)) >= RectAreaSize(ComRect) && ((int)((double)RectAreaSize(OrgRect) * LB)) <= RectAreaSize(ComRect));

            return ret;
        }
        public bool RectIsTheSameFx(Rectangle OrgRect, Rectangle ComRect, int Percent, int CenterDistance)
        {
            bool ret = true;
            double UB = (100 + (double)Percent) / 100;
            double LB = (100 - (double)Percent) / 100;

            ret = ret & (((int)((double)OrgRect.Width * UB)) >= ComRect.Width && ((int)((double)OrgRect.Width * LB)) <= ComRect.Width);
            ret = ret & (((int)((double)OrgRect.Height * UB)) >= ComRect.Height && ((int)((double)OrgRect.Height * LB)) <= ComRect.Height);
            ret = ret & ((Math.Abs(GetRectCenter(OrgRect).X - GetRectCenter(ComRect).X) < CenterDistance) && (Math.Abs(GetRectCenter(OrgRect).Y - GetRectCenter(ComRect).Y) < CenterDistance));

            //ret = ret & (((int)((double)RectAreaSize(OrgRect) * UB)) >= RectAreaSize(ComRect) && ((int)((double)RectAreaSize(OrgRect) * LB)) <= RectAreaSize(ComRect));

            return ret;
        }
        public bool RectIsTheSameFx(Rectangle OrgRect, Rectangle ComRect, int MoreWidth, int MoreHeight, int CenterDistance)
        {
            bool ret = true;

            ret = ret & (((int)((double)OrgRect.Width + MoreWidth)) >= ComRect.Width && ((int)((double)OrgRect.Width - MoreWidth)) <= ComRect.Width);
            ret = ret & (((int)((double)OrgRect.Height + MoreHeight)) >= ComRect.Height && ((int)((double)OrgRect.Height - MoreHeight)) <= ComRect.Height);
            ret = ret & ((Math.Abs(GetRectCenter(OrgRect).X - GetRectCenter(ComRect).X) < CenterDistance) && (Math.Abs(GetRectCenter(OrgRect).Y - GetRectCenter(ComRect).Y) < CenterDistance));

            //ret = ret & (((int)((double)RectAreaSize(OrgRect) * UB)) >= RectAreaSize(ComRect) && ((int)((double)RectAreaSize(OrgRect) * LB)) <= RectAreaSize(ComRect));

            return ret;
        }
        public string GenString(int FromIndex, int Count)
        {
            string Str = "";

            int i = 0;

            while (i < Count)
            {
                Str += (i + FromIndex).ToString("000") + ",";

                i++;
            }

            Str = RemoveLastChar(Str, 1);

            return Str;
        }
        public bool CheckRectF(RectangleF rect)
        {
            return !(rect.Width <= 0 || rect.Height <= 0);
        }
        public bool TestAndReadData(ref string DataStr, string FileName)
        {
            try
            {
                FileStream fs = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                StreamReader Srr = new StreamReader(fs, Encoding.Default);

                DataStr = Srr.ReadToEnd();

                Srr.Close();
                Srr.Dispose();

                return true;
            }
            catch (Exception ex)
            {
                //JetEazy.LoggerClass.Instance.WriteException(ex);
                string Str = ex.ToString();
                return false;
            }

        }
        public string ValueToHEX(long Value, int Length)
        {
            return ("00000000" + Value.ToString("X")).Substring(("00000000" + Value.ToString("X")).Length - Length, Length);
        }
        public string GetFromINIFormat(string str, char secondsplit)
        {
            string[] strs = str.Split('/');
            
            return strs[0].Split(secondsplit)[1].Trim();
        }
        public void FillupColor(Bitmap bmp,Color fillcolor)
        {
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(fillcolor);
            g.Dispose();
        }

        #region 获得图像二进制的数组
        public byte[] GetByteBmp(Bitmap _bmpInput)
        {
            Bitmap CurrentBitmap = new Bitmap(_bmpInput);
            MemoryStream MS = new MemoryStream();
            CurrentBitmap.Save(MS, ImageFormat.Jpeg);//将图片写入流
            CurrentBitmap.Dispose();
            MS.Seek(0, SeekOrigin.Begin);
            byte[] CurrentBitmapBytes = new byte[MS.Length];
            int NumBytesToRead = (int)MS.Length;
            int NumBytesRead = 0;

            while (NumBytesToRead > 0)
            {
                int n = MS.Read(CurrentBitmapBytes, NumBytesRead, NumBytesToRead);
                if (n == 0)
                {
                    break;
                }
                NumBytesRead += n;
                NumBytesToRead -= n;
            }
            MS.Close();

            //return CurrentBitmapBytes;

            byte[] Result = new byte[0];

            //if (!BitmapsAreEqual(ref CurrentBitmapBytes, ref PreviousBitmapBytes))
            {
                Result = CurrentBitmapBytes;
                //PreviousBitmapBytes = CurrentBitmapBytes;
            }
            return Result;
        }
        #endregion

        public Bitmap CombinPicture(Bitmap src1, Bitmap src2)
        {
            int width = src1.Width;
            int height = src1.Height;

            Size sz = new Size(width * 2, height);
            Bitmap bmp = new Bitmap(sz.Width, sz.Height);

            Graphics g = Graphics.FromImage(bmp);
            g.DrawImage(src1, new Rectangle(0, 0, width, height), new Rectangle(0, 0, width, height), GraphicsUnit.Pixel);
            g.DrawImage(src2, new Rectangle(width, 0, width, height), new Rectangle(0, 0, width, height), GraphicsUnit.Pixel);
            g.Dispose();

            return bmp;
        }

        /// <summary>
        /// 获取枚举类子项描述信息
        /// </summary>
        /// <param name="enumSubitem">枚举类子项</param>        
        public string GetEnumDescription(Enum enumSubitem)
        {
            string strValue = enumSubitem.ToString();

            FieldInfo fieldinfo = enumSubitem.GetType().GetField(strValue);
            Object[] objs = fieldinfo.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);
            if (objs == null || objs.Length == 0)
            {
                return strValue;
            }
            else
            {
                System.ComponentModel.DescriptionAttribute da = (System.ComponentModel.DescriptionAttribute)objs[0];
                return da.Description;
            }
        }

    }
}
