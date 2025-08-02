using Common.RecipeSpace;
using JetEazy.BasicSpace;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using VisionDesigner;
using System.Runtime.InteropServices;
using System.IO;

namespace LaserAlignDX.BasicSpace
{
    public class XinliFindLineClass
    {

        private int m_ThrestholdValue = 128;
        private bool m_IsLeftToRight = true;
        private bool m_IsBlack = true;
        private int m_SampleValue = 13;
        private float m_LeastPix = 5;

        public XinliFindLineClass() { }
        /// <summary>
        /// 抓取线的灰阶阈值
        /// </summary>
        public int ThrestholdValue
        {
            get { return m_ThrestholdValue; }
            set { m_ThrestholdValue = value; }
        }
        /// <summary>
        /// 从左往右抓线或者从上到下
        /// </summary>
        public bool IsLeftToRight
        {
            get { return m_IsLeftToRight; }
            set { m_IsLeftToRight = value; }
        }
        /// <summary>
        /// 从白到黑
        /// </summary>
        public bool IsBlack
        {
            get { return m_IsBlack; }
            set { m_IsBlack = value; }
        }
        /// <summary>
        /// 采集间隔
        /// </summary>
        public int SampleValue
        {
            get { return m_SampleValue; }
            set { m_SampleValue = value; }
        }
        public float LeastPix
        {
            get { return m_LeastPix; }
            set { m_LeastPix = value; }
        }


        /// <summary>
        /// 垂直找线
        /// </summary>
        /// <param name="bmpInput">输入图像</param>
        /// <returns>返回直线方程</returns>
        public LineClass GetVericalLine(Bitmap bmpInput)
        {
            int _sample = m_SampleValue;
            int _cropValue = bmpInput.Width - 1;
            int _findCount = _cropValue / _sample;
            PointF[] points1 = new PointF[_findCount];
            PointF[] points2 = new PointF[_findCount];
            int ix = 0;

            while (ix < _findCount)
            {
                Rectangle rectangle = new Rectangle(ix * _sample, 0, _sample, bmpInput.Height - 1);
                Bitmap bmp2 = (Bitmap)bmpInput.Clone(rectangle, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                int y1 = 0;

                if (m_IsLeftToRight)
                    y1 = GetLineInYDir(bmp2, false, 0, 0, (m_IsBlack ? Color.Black : Color.White));
                else
                    y1 = GetLineInYDir(bmp2, true, bmp2.Height - 1, 0, (m_IsBlack ? Color.Black : Color.White));


                points1[ix] = new PointF(rectangle.X, y1);
                bmp2.Dispose();

                ix++;
            }
            LineClass line = getLineForPointF(points1, false, pix: m_LeastPix);
            return line;
        }
        /// <summary>
        /// 水平找线
        /// </summary>
        /// <param name="bmpInput">输入图像</param>
        /// <returns>返回直线方程</returns>
        public LineClass GetLevelLine(Bitmap bmpInput)
        {
            int _sample = SampleValue;
            int _cropValue = bmpInput.Height - 1;
            int _findCount = _cropValue / _sample;
            PointF[] points1 = new PointF[_findCount];
            PointF[] points2 = new PointF[_findCount];
            int ix = 0;

            while (ix < _findCount)
            {
                Rectangle rectangle = new Rectangle(0, ix * _sample, bmpInput.Width - 1, _sample);
                Bitmap bmp2 = (Bitmap)bmpInput.Clone(rectangle, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                int x1;
                if (m_IsLeftToRight)
                    x1 = GetLineInXDir(bmp2, false, 0, 0, (m_IsBlack ? Color.Black : Color.White));
                else
                    x1 = GetLineInXDir(bmp2, true, bmp2.Width - 1, 0, (m_IsBlack ? Color.Black : Color.White));


                points1[ix] = new PointF(x1, rectangle.Y);

                bmp2.Dispose();

                ix++;
            }
            LineClass line = getLineForPointF(points1, pix: m_LeastPix);
            return line;
        }
        object obj = new object();
        private int GetLineInXDir(Bitmap bmp, bool IsReverse, int FromX, int YLocation, Color StopColor)
        {
            lock (obj)
            {
                Rectangle rectbmp = SimpleRect(bmp.Size);
                BitmapData bmpData = bmp.LockBits(rectbmp, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                IntPtr Scan0 = bmpData.Scan0;

                //try
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
                        int y = YLocation;
                        int iStride = bmpData.Stride;

                        x = FromX;
                        pucStart = scan0 + ((x - xmin) << 2) + (iStride * (y - ymin));

                        pucStart[0] = (byte)(255 - StopColor.R);
                        pucStart[1] = (byte)(255 - StopColor.G);
                        pucStart[2] = (byte)(255 - StopColor.B);

                        if (!IsReverse)
                        {
                            while (x < xmax - 1)
                            {
                                pucPtr = pucStart;

                                if (pucPtr[0] == StopColor.R && pucPtr[1] == StopColor.G && pucPtr[2] == StopColor.B)
                                {
                                    pucPtr[0] = 0;
                                    pucPtr[1] = 0;
                                    pucPtr[2] = 255;
                                    //x--;
                                    break;
                                }
                                else
                                {
                                    pucPtr[0] = 255;
                                    pucPtr[1] = 255;
                                    pucPtr[2] = 0;

                                }

                                pucStart += 4;

                                x++;
                            }
                        }
                        else
                        {
                            while (x > 0)
                            {
                                pucPtr = pucStart;

                                if (pucPtr[0] == StopColor.R && pucPtr[1] == StopColor.G && pucPtr[2] == StopColor.B)
                                {
                                    pucPtr[0] = 0;
                                    pucPtr[1] = 0;
                                    pucPtr[2] = 255;
                                    x++;
                                    break;
                                }
                                else
                                {
                                    pucPtr[0] = 255;
                                    pucPtr[1] = 255;
                                    pucPtr[2] = 0;
                                }

                                pucStart -= 4;

                                x--;
                            }
                        }

                        bmp.UnlockBits(bmpData);

                        return x;
                    }
                }
                //catch (Exception e)
                //{
                //    bmp.UnlockBits(bmpData);

                //    //if (IsDebug)
                //    //    MessageBox.Show("Error :" + e.ToString());

                //    return -1;
                //}
            }
        }
        private int GetLineInYDir(Bitmap bmp, bool IsReverse, int FromY, int XLocation, Color StopColor)
        {
            lock (obj)
            {
                Rectangle rectbmp = SimpleRect(bmp.Size);
                BitmapData bmpData = bmp.LockBits(rectbmp, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                IntPtr Scan0 = bmpData.Scan0;

                //try
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

                        int x = XLocation;
                        int y = ymin;
                        int iStride = bmpData.Stride;

                        y = FromY;
                        pucStart = scan0 + ((x - xmin) << 2) + (iStride * (y - ymin));

                        pucStart[0] = (byte)(255 - StopColor.R);
                        pucStart[1] = (byte)(255 - StopColor.G);
                        pucStart[2] = (byte)(255 - StopColor.B);

                        if (!IsReverse)
                        {
                            while (y < ymax - 1)
                            {
                                pucPtr = pucStart;

                                if (pucPtr[0] == StopColor.R && pucPtr[1] == StopColor.G && pucPtr[2] == StopColor.B)
                                {
                                    pucPtr[0] = 0;
                                    pucPtr[1] = 0;
                                    pucPtr[2] = 255;
                                    //y--;
                                    break;
                                }
                                else
                                {
                                    pucPtr[0] = 255;
                                    pucPtr[1] = 255;
                                    pucPtr[2] = 0;
                                }

                                pucStart += iStride;

                                y++;
                            }
                        }
                        else
                        {
                            while (y > 0)
                            {
                                pucPtr = pucStart;

                                if (pucPtr[0] == StopColor.R && pucPtr[1] == StopColor.G && pucPtr[2] == StopColor.B)
                                {
                                    pucPtr[0] = 0;
                                    pucPtr[1] = 0;
                                    pucPtr[2] = 255;
                                    y++;
                                    break;
                                }
                                else
                                {
                                    pucPtr[0] = 255;
                                    pucPtr[1] = 255;
                                    pucPtr[2] = 0;
                                }

                                pucStart -= iStride;

                                y--;
                            }
                        }

                        bmp.UnlockBits(bmpData);

                        return y;
                    }
                }
                //catch (Exception e)
                //{
                //    bmp.UnlockBits(bmpData);

                //    //if (IsDebug)
                //    //    MessageBox.Show("Error :" + e.ToString());

                //    return -1;
                //}
            }
        }
        Rectangle SimpleRect(Size Sz)
        {
            return new Rectangle(0, 0, Sz.Width, Sz.Height);
        }
        private LineClass getLineForPointF(PointF[] points1, bool swap = true, float pix = 0.5f)
        {
            QvLineFit jzLineFit = new QvLineFit();
            jzLineFit.Swap = swap;
            jzLineFit.LeastSquareFit(points1);


            PointF p1 = new PointF(points1[0].X, points1[0].Y);
            PointF p2 = new PointF(points1[points1.Length - 1].X, points1[points1.Length - 1].Y);

            if (swap)
            {
                if (jzLineFit.A != 0)
                    p1.X = (float)((double)p1.Y * jzLineFit.A + jzLineFit.B);

                if (jzLineFit.A != 0)
                    p2.X = (float)((double)p2.Y * jzLineFit.A + jzLineFit.B);
            }
            else
            {
                if (jzLineFit.A != 0)
                    p1.Y = (float)((double)p1.X * jzLineFit.A + jzLineFit.B);

                if (jzLineFit.A != 0)
                    p2.Y = (float)((double)p2.X * jzLineFit.A + jzLineFit.B);
            }

            //if (jzLineFit.A != 0)
            //    points1[0].X = (float)((double)points1[0].Y * jzLineFit.A + jzLineFit.B);

            //if (jzLineFit.A != 0)
            //    points1[points1.Length - 1].X = (float)((double)points1[points1.Length - 1].Y * jzLineFit.A + jzLineFit.B);

            LineClass lineClass = new LineClass(p1, p2);
            lineClass.IsSwap = swap;
            //if (swap)
            {
                //lineClass.IsSwap = true;
                List<PointF> lines = new List<PointF>();
                int i = 0;
                while (i < points1.Length)
                {
                    double xlen = jzLineFit.GetPointLength(points1[i]);
                    if (xlen < pix)
                    {
                        lines.Add(points1[i]);
                    }

                    i++;
                }

                if (lines.Count >= 2)
                {
                    jzLineFit.Swap = swap;
                    jzLineFit.LeastSquareFit(lines.ToArray());

                    p1 = lines[0];
                    p2 = lines[lines.Count - 1];

                    if (swap)
                    {
                        if (jzLineFit.A != 0)
                            p1.X = (float)((double)p1.Y * jzLineFit.A + jzLineFit.B);

                        if (jzLineFit.A != 0)
                            p2.X = (float)((double)p2.Y * jzLineFit.A + jzLineFit.B);
                    }
                    else
                    {
                        if (jzLineFit.A != 0)
                            p1.Y = (float)((double)p1.X * jzLineFit.A + jzLineFit.B);

                        if (jzLineFit.A != 0)
                            p2.Y = (float)((double)p2.X * jzLineFit.A + jzLineFit.B);
                    }

                    lineClass.IsSwap = swap;
                    lineClass = new LineClass(p1, p2);
                }
            }


            return lineClass;
        }

        public LineClass GetLevelLineVM_small(Bitmap bmpInput,RectangleF eRect,bool isleft=true)
        {
            //int _sample = SampleValue;
            //int _cropValue = bmpInput.Height - 1;
            //int _findCount = _cropValue / _sample;
            //PointF[] points1 = new PointF[_findCount];
            //PointF[] points2 = new PointF[_findCount];
            //int ix = 0;

            PointF p1 = new PointF(0 + eRect.X, 0 + eRect.Y);
            PointF p2 = new PointF(bmpInput.Width + eRect.X, 0 + eRect.Y);

            try

            {

                // CreateInstance

                VisionDesigner.LineFind.CLineFindTool cLineFindToolObj = new VisionDesigner.LineFind.CLineFindTool();

                // Set input image

                AForge.Imaging.Filters.Grayscale grayscale = new AForge.Imaging.Filters.Grayscale(0.299, 0.587, 0.114);
                bmpInput = grayscale.Apply(bmpInput);

                VisionDesigner.CMvdImage cInputImg = BitmapToCMvdImage(bmpInput);
                //cInputImg.InitImage("InputTest.bmp");

                cLineFindToolObj.InputImage = cInputImg;
                //cInputImg.SaveImage("D:\\Img.png", MVD_FILE_FORMAT.MVD_FILE_PNG);

                // Set ROI region (optional)
                CMvdRectangleF cMvd = new VisionDesigner.CMvdRectangleF(cInputImg.Width / 2, cInputImg.Height / 2, cInputImg.Width, cInputImg.Height);
                if (isleft)
                    cMvd.Angle = 180;
                else
                {
                    cMvd.Angle = 0;
                }
                cLineFindToolObj.ROI = cMvd;// new VisionDesigner.CMvdRectangleF(cInputImg.Width / 2, cInputImg.Height / 2, cInputImg.Width, cInputImg.Height);

                cLineFindToolObj.SetRunParam("RayNum", "55");//卡尺数量
                cLineFindToolObj.SetRunParam("RejectNum", "5");//剔除点数
                cLineFindToolObj.SetRunParam("RejectDist", "10");//剔除距离
                cLineFindToolObj.SetRunParam("FindOrient", "LeftToRight");//搜索方向
                cLineFindToolObj.SetRunParam("EdgePolarity", "WhiteToBlack");//边缘极性

                // Running

                cLineFindToolObj.Run();

                //  Get the result

                VisionDesigner.LineFind.CLineFindResult cLineFindRes = cLineFindToolObj.Result;

                Console.WriteLine("Recognition status: {0}", cLineFindRes.Status);

                Console.WriteLine("Start point of line: ({0},{1}）", cLineFindRes.LineStartPoint.fX, cLineFindRes.LineStartPoint.fY);

                if (cLineFindRes.Status == 1)
                {
                    p1 = new PointF(cLineFindRes.LineStartPoint.fX + eRect.X, cLineFindRes.LineStartPoint.fY + eRect.Y);
                    p2 = new PointF(cLineFindRes.LineEndPoint.fX + eRect.X, cLineFindRes.LineEndPoint.fY + eRect.Y);
                }
            }

            catch (MvdException ex)

            {

                Console.WriteLine("Fail with ErrorCode: 0x" + ex.ErrorCode.ToString("X"));

            }

            catch (System.Exception ex)

            {

                Console.WriteLine("Fail with error " + ex.Message);

            }
            LineClass line = new LineClass(p1, p2);
            line.IsSwap = true;
            return line;
        }
        public LineClass GetVericalLineVM_samll(Bitmap bmpInput,RectangleF eRect,bool istop=true)
        {
            //int _sample = m_SampleValue;
            //int _cropValue = bmpInput.Width - 1;
            //int _findCount = _cropValue / _sample;
            //PointF[] points1 = new PointF[_findCount];
            //PointF[] points2 = new PointF[_findCount];
            //int ix = 0;

            PointF p1 = new PointF(0 + eRect.X, 0 + eRect.Y);
            PointF p2 = new PointF(0 + eRect.X, bmpInput.Height + eRect.Y);

            try

            {

                // CreateInstance

                VisionDesigner.LineFind.CLineFindTool cLineFindToolObj = new VisionDesigner.LineFind.CLineFindTool();

                // Set input image
                AForge.Imaging.Filters.Grayscale grayscale = new AForge.Imaging.Filters.Grayscale(0.299, 0.587, 0.114);
                bmpInput = grayscale.Apply(bmpInput);

                VisionDesigner.CMvdImage cInputImg = BitmapToCMvdImage(bmpInput);
                //cInputImg.InitImage("D:\\Img.png");

                cLineFindToolObj.InputImage = cInputImg;
                //cInputImg.SaveImage("D:\\Img.png", MVD_FILE_FORMAT.MVD_FILE_PNG);

                // Set ROI region (optional)
                CMvdRectangleF cMvd = new VisionDesigner.CMvdRectangleF(cInputImg.Width / 2, cInputImg.Height / 2, cInputImg.Width, cInputImg.Height);
                if (istop)
                    cMvd.Angle = 180;
                else
                {
                    cMvd.Angle = 0;
                }
                cLineFindToolObj.ROI = cMvd;// new VisionDesigner.CMvdRectangleF(cInputImg.Width / 2, cInputImg.Height / 2, cInputImg.Width, cInputImg.Height);
             
                cLineFindToolObj.SetRunParam("RayNum", "55");//卡尺数量
                cLineFindToolObj.SetRunParam("RejectNum", "5");//剔除点数
                cLineFindToolObj.SetRunParam("RejectDist", "10");//剔除距离
                cLineFindToolObj.SetRunParam("FindOrient", "UpToDown");//搜索方向
                cLineFindToolObj.SetRunParam("EdgePolarity", "WhiteToBlack");//边缘极性

                // Running

                cLineFindToolObj.Run();

                //  Get the result

                VisionDesigner.LineFind.CLineFindResult cLineFindRes = cLineFindToolObj.Result;

                Console.WriteLine("Recognition status: {0}", cLineFindRes.Status);

                Console.WriteLine("Start point of line: ({0},{1}）", cLineFindRes.LineStartPoint.fX, cLineFindRes.LineStartPoint.fY);

                if (cLineFindRes.Status == 1)
                {
                    p1 = new PointF(cLineFindRes.LineStartPoint.fX + eRect.X, cLineFindRes.LineStartPoint.fY + eRect.Y);
                    p2 = new PointF(cLineFindRes.LineEndPoint.fX + eRect.X, cLineFindRes.LineEndPoint.fY + eRect.Y);
                }
            }

            catch (MvdException ex)

            {

                Console.WriteLine("Fail with ErrorCode: 0x" + ex.ErrorCode.ToString("X"));

            }

            catch (System.Exception ex)

            {

                Console.WriteLine("Fail with error " + ex.Message);

            }
            LineClass line = new LineClass(p1, p2);
            //line.IsSwap = true;
            return line;
        }
        /// <summary>
        /// 水平找线VM
        /// </summary>
        /// <param name="bmpInput">输入图像</param>
        /// <returns>返回直线方程</returns>
        public LineClass GetLevelLineVM(Bitmap bmpInput)
        {
            //int _sample = SampleValue;
            //int _cropValue = bmpInput.Height - 1;
            //int _findCount = _cropValue / _sample;
            //PointF[] points1 = new PointF[_findCount];
            //PointF[] points2 = new PointF[_findCount];
            //int ix = 0;

            PointF p1 = new PointF(0, 0);
            PointF p2 = new PointF(bmpInput.Width, 0);

            try

            {

                // CreateInstance

                VisionDesigner.LineFind.CLineFindTool cLineFindToolObj = new VisionDesigner.LineFind.CLineFindTool();

                // Set input image

                VisionDesigner.CMvdImage cInputImg = BitmapToCMvdImage(bmpInput);
                //cInputImg.InitImage("InputTest.bmp");

                cLineFindToolObj.InputImage = cInputImg;
                //cInputImg.SaveImage("D:\\Img.png", MVD_FILE_FORMAT.MVD_FILE_PNG);

                // Set ROI region (optional)
                VisionDesigner.CMvdRectangleF cMvd = new VisionDesigner.CMvdRectangleF(cInputImg.Width / 2, cInputImg.Height / 2, cInputImg.Width, cInputImg.Height);
                cMvd.Angle = (m_IsLeftToRight ? 0 : 180);
                cLineFindToolObj.ROI = cMvd;// new VisionDesigner.CMvdRectangleF(cInputImg.Width / 2, cInputImg.Height / 2, cInputImg.Width, cInputImg.Height);



                cLineFindToolObj.SetRunParam("RayNum", "100");//卡尺数量
                cLineFindToolObj.SetRunParam("RejectNum", "30");//剔除点数
                //cLineFindToolObj.SetRunParam("RejectDist", "10");//剔除距离
                cLineFindToolObj.SetRunParam("FindOrient", "LeftToRight");//搜索方向
                cLineFindToolObj.SetRunParam("EdgePolarity", (IsBlack ? "WhiteToBlack" : "BlackToWhite"));//边缘极性

                // Running

                cLineFindToolObj.Run();

                //  Get the result

                VisionDesigner.LineFind.CLineFindResult cLineFindRes = cLineFindToolObj.Result;

                Console.WriteLine("Recognition status: {0}", cLineFindRes.Status);

                Console.WriteLine("Start point of line: ({0},{1}）", cLineFindRes.LineStartPoint.fX, cLineFindRes.LineStartPoint.fY);

                if (cLineFindRes.Status == 1)
                {
                    p1 = new PointF(cLineFindRes.LineStartPoint.fX, cLineFindRes.LineStartPoint.fY);
                    p2 = new PointF(cLineFindRes.LineEndPoint.fX, cLineFindRes.LineEndPoint.fY);
                }
            }

            catch (MvdException ex)

            {

                Console.WriteLine("Fail with ErrorCode: 0x" + ex.ErrorCode.ToString("X"));

            }

            catch (System.Exception ex)

            {

                Console.WriteLine("Fail with error " + ex.Message);

            }
            LineClass line = new LineClass(p1, p2);
            line.IsSwap = true;
            return line;
        }
        /// <summary>
        /// 垂直找线VM
        /// </summary>
        /// <param name="bmpInput">输入图像</param>
        /// <returns>返回直线方程</returns>
        public LineClass GetVericalLineVM(Bitmap bmpInput)
        {
            //int _sample = m_SampleValue;
            //int _cropValue = bmpInput.Width - 1;
            //int _findCount = _cropValue / _sample;
            //PointF[] points1 = new PointF[_findCount];
            //PointF[] points2 = new PointF[_findCount];
            //int ix = 0;

            PointF p1 = new PointF(0, 0);
            PointF p2 = new PointF(0, bmpInput.Height);

            try

            {

                // CreateInstance

                VisionDesigner.LineFind.CLineFindTool cLineFindToolObj = new VisionDesigner.LineFind.CLineFindTool();

                // Set input image

                VisionDesigner.CMvdImage cInputImg = BitmapToCMvdImage(bmpInput);
                //cInputImg.InitImage("D:\\Img.png");

                cLineFindToolObj.InputImage = cInputImg;
                //cInputImg.SaveImage("D:\\Img.png", MVD_FILE_FORMAT.MVD_FILE_PNG);

                // Set ROI region (optional)
                VisionDesigner.CMvdRectangleF cMvd = new VisionDesigner.CMvdRectangleF(cInputImg.Width / 2, cInputImg.Height / 2, cInputImg.Width, cInputImg.Height);
                cMvd.Angle = (m_IsLeftToRight ? 0 : 180);
                cLineFindToolObj.ROI = cMvd;// new VisionDesigner.CMvdRectangleF(cInputImg.Width / 2, cInputImg.Height / 2, cInputImg.Width, cInputImg.Height);

                //cLineFindToolObj.ROI = new VisionDesigner.CMvdRectangleF(cInputImg.Width / 2, cInputImg.Height / 2, cInputImg.Width, cInputImg.Height);

                cLineFindToolObj.SetRunParam("RayNum", "100");//卡尺数量
                cLineFindToolObj.SetRunParam("RejectNum", "30");//剔除点数
                //cLineFindToolObj.SetRunParam("RejectDist", "10");//剔除距离
                cLineFindToolObj.SetRunParam("FindOrient", "UpToDown");//搜索方向
                cLineFindToolObj.SetRunParam("EdgePolarity", (IsBlack ? "WhiteToBlack" : "BlackToWhite"));//边缘极性

                //System.Windows.Forms.SaveFileDialog fileDlg = null;
                //FileStream fileStr = null;
                //try
                //{
                //    fileDlg = new System.Windows.Forms.SaveFileDialog();
                //    fileDlg.Filter = @"XMl Files(*.xml)|*.xml";
                //    fileDlg.RestoreDirectory = true;
                //    if (fileDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                //    {
                //        string filePath = fileDlg.FileName;
                //        if (File.Exists(filePath))
                //        {
                //            File.Delete(filePath);
                //        }
                //        fileStr = new FileStream(filePath, FileMode.Create);

                //        /* Save parameters in local file as XML. */
                //        byte[] fileBytes = new byte[256];
                //        uint nConfigDataSize = 256;
                //        uint nConfigDataLen = 0;
                //        try
                //        {
                //            cLineFindToolObj.SaveConfiguration(fileBytes, nConfigDataSize, ref nConfigDataLen);
                //        }
                //        catch (MvdException ex)
                //        {
                //            if (MVD_ERROR_CODE.MVD_E_NOENOUGH_BUF == ex.ErrorCode)
                //            {
                //                fileBytes = new byte[nConfigDataLen];
                //                nConfigDataSize = nConfigDataLen;
                //                cLineFindToolObj.SaveConfiguration(fileBytes, nConfigDataSize, ref nConfigDataLen);
                //            }
                //            else
                //            {
                //                throw ex;
                //            }
                //        }

                //        fileStr.Write(fileBytes, 0, Convert.ToInt32(nConfigDataLen));
                //        fileStr.Flush();
                //        fileStr.Close();
                //        fileStr.Dispose();
                //        //this.rtbInfoMessage.Text += "Finish exporting xml file.\r\n";

                //        Console.WriteLine("Recognition status: {0}", "Finish exporting xml file.\r\n");
                //    }
                //    fileDlg.Dispose();
                //}
                //catch (MvdException ex)
                //{
                //    //this.rtbInfoMessage.Text += "Fail to export xml file. ErrorCode: 0x" + ex.ErrorCode.ToString("X") + "\r\n";
                //    Console.WriteLine("Fail to export xml file. ErrorCode: 0x" + ex.ErrorCode.ToString("X") + "\r\n");
                //}
                //catch (System.Exception ex)
                //{
                //    //this.rtbInfoMessage.Text += "Fail to export xml file with error ' " + ex.Message + " '\r\n";
                //    Console.WriteLine("Fail to export xml file with error ' " + ex.Message + " '\r\n");
                //}
                //finally
                //{
                //    if (null != fileDlg)
                //    {
                //        fileDlg.Dispose();
                //    }
                //    if (null != fileStr)
                //    {
                //        fileStr.Close();
                //        fileStr.Dispose();
                //    }
                //}

                // Running

                cLineFindToolObj.Run();

                //  Get the result

                VisionDesigner.LineFind.CLineFindResult cLineFindRes = cLineFindToolObj.Result;

                Console.WriteLine("Recognition status: {0}", cLineFindRes.Status);

                Console.WriteLine("Start point of line: ({0},{1}）", cLineFindRes.LineStartPoint.fX, cLineFindRes.LineStartPoint.fY);

                if (cLineFindRes.Status == 1)
                {
                    p1 = new PointF(cLineFindRes.LineStartPoint.fX, cLineFindRes.LineStartPoint.fY);
                    p2 = new PointF(cLineFindRes.LineEndPoint.fX, cLineFindRes.LineEndPoint.fY);
                }
            }

            catch (MvdException ex)

            {

                Console.WriteLine("Fail with ErrorCode: 0x" + ex.ErrorCode.ToString("X"));

            }

            catch (System.Exception ex)

            {

                Console.WriteLine("Fail with error " + ex.Message);

            }
            LineClass line = new LineClass(p1, p2);
            //line.IsSwap = true;
            return line;
        }
        /// <summary>
        /// Bitmap 转成 CMvdImage
        /// </summary>
        /// <param name="bmpInputImg"></param>
        /// <returns></returns>
        private CMvdImage BitmapToCMvdImage(Bitmap bmpInputImg)
        {
            //AForge.Imaging.Filters.Grayscale grayscale = new AForge.Imaging.Filters.Grayscale(0.299, 0.587, 0.114);
            //bmpInputImg = grayscale.Apply(bmpInputImg);

            CMvdImage cMvdImage = new CMvdImage();
            System.Drawing.Imaging.PixelFormat bitPixelFormat = bmpInputImg.PixelFormat;
            BitmapData bmData = bmpInputImg.LockBits(new Rectangle(0, 0, bmpInputImg.Width, bmpInputImg.Height), ImageLockMode.ReadOnly, bitPixelFormat);//锁定

            if (bitPixelFormat == System.Drawing.Imaging.PixelFormat.Format8bppIndexed)
            {
                Int32 bitmapDataSize = bmData.Stride * bmData.Height;//bitmap图像缓存长度
                int offset = bmData.Stride - bmData.Width;
                Int32 ImageBaseDataSize = bmData.Width * bmData.Height;//imageBaseData_V2图像真正的缓存长度
                byte[] _BitImageBufferBytes = new byte[bitmapDataSize];
                byte[] _ImageBaseDataBufferBytes = new byte[ImageBaseDataSize];
                Marshal.Copy(bmData.Scan0, _BitImageBufferBytes, 0, bitmapDataSize);
                int bitmapIndex = 0;
                int ImageBaseDataIndex = 0;
                for (int i = 0; i < bmData.Height; i++)
                {
                    for (int j = 0; j < bmData.Width; j++)
                    {
                        _ImageBaseDataBufferBytes[ImageBaseDataIndex++] = _BitImageBufferBytes[bitmapIndex++];
                    }
                    bitmapIndex += offset;
                }
                MVD_IMAGE_DATA_INFO stImageData = new MVD_IMAGE_DATA_INFO();
                stImageData.stDataChannel[0].nRowStep = (uint)bmData.Width;
                stImageData.stDataChannel[0].nLen = (uint)ImageBaseDataSize;
                stImageData.stDataChannel[0].nSize = (uint)ImageBaseDataSize;
                stImageData.stDataChannel[0].arrDataBytes = _ImageBaseDataBufferBytes;
                cMvdImage.InitImage((uint)bmData.Width, (uint)bmData.Height, MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08, stImageData);
            }
            else if (bitPixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb)
            {
                Int32 bitmapDataSize = bmData.Stride * bmData.Height;//bitmap图像缓存长度
                int offset = bmData.Stride - bmData.Width * 3;
                Int32 ImageBaseDataSize = bmData.Width * bmData.Height * 3;//imageBaseData_V2图像真正的缓存长度
                byte[] _BitImageBufferBytes = new byte[bitmapDataSize];
                byte[] _ImageBaseDataBufferBytes = new byte[ImageBaseDataSize];
                Marshal.Copy(bmData.Scan0, _BitImageBufferBytes, 0, bitmapDataSize);
                int bitmapIndex = 0;
                int ImageBaseDataIndex = 0;
                for (int i = 0; i < bmData.Height; i++)
                {
                    for (int j = 0; j < bmData.Width; j++)
                    {
                        _ImageBaseDataBufferBytes[ImageBaseDataIndex++] = _BitImageBufferBytes[bitmapIndex + 2];
                        _ImageBaseDataBufferBytes[ImageBaseDataIndex++] = _BitImageBufferBytes[bitmapIndex + 1];
                        _ImageBaseDataBufferBytes[ImageBaseDataIndex++] = _BitImageBufferBytes[bitmapIndex];
                        bitmapIndex += 3;
                    }
                    bitmapIndex += offset;
                }
                MVD_IMAGE_DATA_INFO stImageData = new MVD_IMAGE_DATA_INFO();
                stImageData.stDataChannel[0].nRowStep = (uint)bmData.Width * 3;
                stImageData.stDataChannel[0].nLen = (uint)ImageBaseDataSize;
                stImageData.stDataChannel[0].nSize = (uint)ImageBaseDataSize;
                stImageData.stDataChannel[0].arrDataBytes = _ImageBaseDataBufferBytes;
                cMvdImage.InitImage((uint)bmData.Width, (uint)bmData.Height, MVD_PIXEL_FORMAT.MVD_PIXEL_RGB_RGB24_C3, stImageData);
            }
            bmpInputImg.UnlockBits(bmData);  // 解除锁定
            return cMvdImage;
        }

    }
}
