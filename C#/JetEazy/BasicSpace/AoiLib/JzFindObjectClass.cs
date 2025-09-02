using AHBlobPro;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace JetEazy.BasicSpace
{
    public class DoffsetClass
    {
        public float Degree = 0f;
        public PointF OffsetF = new PointF(0, 0);

        public DoffsetClass(float degree,PointF offsetf)
        {
            Degree = degree;
            OffsetF = offsetf;
        }
    }

    public class HistogramClass
    {
        bool IsDebug
        {
            get
            {
                return true;
            }
        }
        
        int ColorGap = 2;
        int BarRange = 0;

        public int MaxGrade = -1000;
        public int MinGrade = 1000;

        public int TotalGrade = 0;
        public int MeanGrade = 0;
        public int TotalPixels = 1;

        public int TotalPixelForCount = 0;

        int ModeBarIndex = 0;
        public int ModeGrade = 0;
        public int ModeGradeAmount
        {
            get
            {
                return (SortingBars[SortingBars.Length - 1] + SortingBars[SortingBars.Length - 2] + SortingBars[SortingBars.Length - 3]) / 1000;
            }
        }

        public int[] SortingBars;
        public int[] OriginSortingBars;
        public int ModeGradeIndex
        {
            get
            {
                return SortingBars[SortingBars.Length - 1] % 1000;
            }
        }
        public int GetGradeValue(int index)
        {
            return SortingBars[SortingBars.Length - index - 1] % 1000;
        }
        public int GetGradeCount(int index)
        {
            return SortingBars[SortingBars.Length - index - 1] / 1000;
        }
        public HistogramClass(int rColorGap)
        {
            ColorGap = rColorGap;

            BarRange = (int)Math.Ceiling(255d / (double)ColorGap) + 1;
            SortingBars = new int[BarRange];
        }
        public void GetHistogram(Bitmap bmp)
        {
            GetHistogram(bmp, SimpleRect(bmp.Size));
        }
        public void GetHistogram(Bitmap bmp, bool IsWithoutZeroFilter)
        {
            GetHistogram(bmp, SimpleRect(bmp.Size), IsWithoutZeroFilter);
        }
        public void GetHistogram(Bitmap bmp, Rectangle rect)
        {
            GetHistogram(bmp, rect, false);
        }
        public void GetHistogram(Bitmap bmp, Rectangle rect, bool IsWithoutZeroFilter)
        {
            int Grade = 0;

            Reset();

            Rectangle rectbmp = rect;
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

                    TotalPixelForCount = 0;

                    y = ymin;
                    pucStart = scan0 + ((x - xmin) << 2) + (iStride * (y - ymin));

                    while (y < ymax)
                    {
                        x = xmin;
                        pucPtr = pucStart;
                        while (x < xmax)
                        {
                            if (pucPtr[0] != 0 || IsWithoutZeroFilter) //Use Zero Filter
                            {
                                Grade = GrayscaleInt(pucPtr[2], pucPtr[1], pucPtr[0]);
                                Add(Grade);
                            }

                            //*((uint*)pucPtr) = 0xFFFF0000;
                            TotalPixelForCount++;

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
                bmp.UnlockBits(bmpData);

                if (IsDebug)
                    MessageBox.Show("Error :" + ex.ToString());
            }

            Complete();
        }
        public void GetHistogram(Bitmap bmp, Rectangle rect, int ExclusiveColor)
        {
            int Grade = 0;

            Reset();
            Rectangle rectbmp = rect;
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
                    TotalPixelForCount = 0;

                    while (y < ymax)
                    {
                        x = xmin;
                        pucPtr = pucStart;
                        while (x < xmax)
                        {
                            if (pucPtr[0] != ExclusiveColor) //Use Zero Filter
                            {
                                Grade = GrayscaleInt(pucPtr[2], pucPtr[1], pucPtr[0]);
                                Add(Grade);
                            }

                            //*((uint*)pucPtr) = 0xFFFF0000;
                            TotalPixelForCount++;
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
                bmp.UnlockBits(bmpData);

                if (IsDebug)
                    MessageBox.Show("Error :" + ex.ToString());
            }

            Complete();
        }
        public void GetHistogram(Bitmap bmp, Bitmap bmpmask,bool ischeckwhite)
        {
            int Grade = 0;
            int maskGrade = 0;

            Rectangle rectbmp = SimpleRect(bmp.Size);

            BitmapData bmpData = bmp.LockBits(rectbmp, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            BitmapData bmpmaskData = bmpmask.LockBits(rectbmp, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            IntPtr Scan0 = bmpData.Scan0;
            IntPtr maskScan0 = bmpmaskData.Scan0;

            Reset();
            try
            {
                unsafe
                {
                    byte* scan0 = (byte*)(void*)Scan0;
                    byte* pucPtr;
                    byte* pucStart;

                    byte* maskscan0 = (byte*)(void*)maskScan0;
                    byte* maskpucPtr;
                    byte* maskpucStart;

                    int xmin = rectbmp.X;
                    int ymin = rectbmp.Y;

                    int xmax = xmin + rectbmp.Width;
                    int ymax = ymin + rectbmp.Height;

                    int x = xmin;
                    int y = ymin;

                    int iStride = bmpData.Stride;
                    int imaskStride = bmpmaskData.Stride;

                    y = ymin;
                    pucStart = scan0 + ((x - xmin) << 2) + (iStride * (y - ymin));
                    maskpucStart = maskscan0 + ((x - xmin) << 2) + (imaskStride * (y - ymin));

                    int CheckColor = 0;

                    if (ischeckwhite)
                        CheckColor = 255;
                    else
                        CheckColor = 0;

                    TotalPixelForCount = 0;

                    while (y < ymax)
                    {
                        x = xmin;

                        pucPtr = pucStart;
                        maskpucPtr = maskpucStart;

                        while (x < xmax)
                        {
                            maskGrade = (int)maskpucPtr[2];
                            //if (*((uint*)maskpucPtr) == 0xFFFFFFFF)
                            if (maskGrade == CheckColor)
                            {
                                Grade = GrayscaleInt(pucPtr[2], pucPtr[1], pucPtr[0]);

                                //*((uint*)pucPtr) = 0xFF00FFFF;
                                
                                Add(Grade);

                                TotalPixelForCount++;
                            }

                            pucPtr += 4;
                            maskpucPtr += 4;
                            x++;
                        }
                        pucStart += iStride;
                        maskpucStart += imaskStride;
                        y++;
                    }
                    bmp.UnlockBits(bmpData);
                    bmpmask.UnlockBits(bmpmaskData);
                }
            }
            catch (Exception ex)
            {
                //JetEazy.LoggerClass.Instance.WriteException(ex);
                bmp.UnlockBits(bmpData);
                bmpmask.UnlockBits(bmpmaskData);

                if (IsDebug)
                    MessageBox.Show("Error :" + ex.ToString());
            }
            Complete();
        }
        public void GetHistogram(Bitmap bmp, int UppercutGrade)
        {
            int Grade = 0;

            Reset();
            Rectangle rectbmp = SimpleRect(bmp.Size);
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
                            if (pucPtr[0] != 0) //Use Zero Filter
                            {
                                Grade = GrayscaleInt(pucPtr[2], pucPtr[1], pucPtr[0]);

                                if (Grade < UppercutGrade)
                                    Add(Grade);
                            }

                            //*((uint*)pucPtr) = 0xFFFF0000;

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
                bmp.UnlockBits(bmpData);

                if (IsDebug)
                    MessageBox.Show("Error :" + ex.ToString());
            }

            Complete();
        }
        public void GetWBArray(Bitmap bmp, int mean, int[,] wbarray)
        {
            int Grade = 0;

            Rectangle rectbmp = SimpleRect(bmp.Size);
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
                            if (pucPtr[0] != 0) //Use Zero Filter
                            {
                                Grade = GrayscaleInt(pucPtr[2], pucPtr[1], pucPtr[0]);

                                wbarray[x, y] = mean - Grade;
                            }

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
                bmp.UnlockBits(bmpData);

                if (IsDebug)
                    MessageBox.Show("Error :" + ex.ToString());
            }
        }
        public void SetWBArray(Bitmap bmp, int threshold , int[,] wbarray) //Threshold is No Use
        {
            int Grade = 0;

            Rectangle rectbmp = SimpleRect(bmp.Size);
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
                            if (pucPtr[0] != 0) //Use Zero Filter
                            {
                                Grade = GrayscaleInt(pucPtr[2], pucPtr[1], pucPtr[0]);

                                if (Grade > threshold)
                                {
                                    Grade += wbarray[x, y];

                                    Grade = Math.Min(255, Grade);
                                    Grade = Math.Max(0, Grade);

                                    pucPtr[2] = (byte)Grade;
                                    pucPtr[1] = (byte)Grade;
                                    pucPtr[0] = (byte)Grade;
                                }
                            }

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
                bmp.UnlockBits(bmpData);

                if (IsDebug)
                    MessageBox.Show("Error :" + ex.ToString());
            }
        }
        /// <summary>
        /// 取得從上往下數的某個比例內的值
        /// </summary>
        /// <param name="ratio"></param>
        /// <returns></returns>
        public int GetMaxRatio(float ratio)
        {
            int i = 0;

            int RatioIndex = (int)((float)TotalPixelForCount * ratio);
            int RatioCount = 0;

            i = OriginSortingBars.Length - 1;

            while(i > -1)
            {
                RatioCount += (OriginSortingBars[i] / 1000);

                if (RatioCount > RatioIndex)
                    break;
                i--;
            }
            return i * ColorGap;
        }
        /// <summary>
        /// 取得從下往上數的某個比例內的值
        /// </summary>
        /// <param name="ratio"></param>
        /// <returns></returns>
        public int GetMinRatio(float ratio)
        {
            int i = 0;

            int RatioIndex = (int)((float)TotalPixelForCount * ratio);
            int RatioCount = 0;

            i = 0;

            while (i < OriginSortingBars.Length)
            {
                RatioCount += (OriginSortingBars[i] / 1000);

                if (RatioCount > RatioIndex)
                    break;
                i++;
            }
            return i * ColorGap;
        }
        /// <summary>
        /// 取得在全數從高往低某個比例上的數值的平均值
        /// </summary>
        /// <param name="ratio"></param>
        /// <returns></returns>
        public int GetMaxRatioAVG(float ratio)
        {   
            int i = 0;

            //int RatioIndex = (int)((float)TotalPixelForCount * ratio);
            int RatioCount = 0;

            int iTempCount = (int)(OriginSortingBars.Length * ratio);

            float AVG = 0f;

            i = OriginSortingBars.Length - 1;

            while (i > -1)
            {
                int singlecount = (OriginSortingBars[i] / 1000);

                RatioCount += singlecount;

                AVG += (float)(i * singlecount * ColorGap);

                //if (RatioCount > RatioIndex)
                //    break;
                if (RatioCount > OriginSortingBars.Length)
                    break;
                if (RatioCount > iTempCount)
                    break;
                i--;
            }

            return (int)(AVG / (float)RatioCount);
        }
        /// <summary>
        /// 取得在全數從低往高某個比例下的數值的平均值
        /// </summary>
        /// <param name="ratio"></param>
        /// <returns></returns>
        public int GetMinRatioAVG(float ratio)
        {
            int i = 0;

            //int RatioIndex = (int)((float)TotalPixelForCount * ratio);
            int RatioCount = 0;
            int iTempCount = (int)(OriginSortingBars.Length * ratio);

            float AVG = 0f;

            i = 0;

            while (i < OriginSortingBars.Length)
            {
                int singlecount = (OriginSortingBars[i] / 1000);

                RatioCount += (OriginSortingBars[i] / 1000);

                AVG += (float)(i * singlecount * ColorGap);

                //if (RatioCount > RatioIndex)
                //    break;
                if (RatioCount > OriginSortingBars.Length)
                    break;
                if (RatioCount > iTempCount)
                    break;
                i++;
            }

            return (int)(AVG / (float)RatioCount);
        }
        void Reset()
        {
            MaxGrade = -1000;
            MinGrade = 1000;
            ModeGrade = 0;

            TotalGrade = 0;
            MeanGrade = 0;

            SortingBars = new int[BarRange];
            //BarsBeSort = new int[BarRange];

            TotalPixels = 1;
        }
        void Complete()
        {
            MeanGrade = TotalGrade / TotalPixels;

            int i = 0;
            int MaxValue = -1;

            ModeBarIndex = -1;

            while (i < BarRange)
            {
                if (MaxValue < SortingBars[i])
                {
                    MaxValue = SortingBars[i];
                    ModeBarIndex = i;
                }

                SortingBars[i] += i;
                i++;
            }
            ModeGrade = Math.Min(ModeBarIndex * ColorGap, 255);

            OriginSortingBars = new int[SortingBars.Length];

            Array.Copy(SortingBars, OriginSortingBars, SortingBars.Length);

            Array.Sort(SortingBars);

        }
        void Add(int Grade)
        {
            MaxGrade = Math.Max(Grade, MaxGrade);
            MinGrade = Math.Min(Grade, MinGrade);

            int i = (int)Math.Ceiling(Grade / (double)ColorGap);
            SortingBars[i] += 1000;

            TotalGrade += Grade;
            TotalPixels++;
        }

        Rectangle SimpleRect(Size Sz)
        {
            return new Rectangle(0, 0, Sz.Width, Sz.Height);
        }
        int GrayscaleInt(byte R, byte G, byte B)
        {
            return (int)((double)R * 0.3 + (double)G * 0.59 + (double)B * 0.11);
        }
    }

    public class DataHistogramClass
    {
        bool IsDebug
        {
            get
            {
                return false;
            }
        }

        JzToolsClass JzTools = new JzToolsClass();

        int Gap = 2;
        int BarRange = 0;

        public int MaxGrade = -1000;
        public int MinGrade = 1000;

        public int TotalGrade = 0;
        public int MeanGrade = 0;
        public int TotalCount = 1;

        public int TotalPixelForCount = 0;

        int ModeBarIndex = 0;
        public int ModeGrade = 0;
        public int ModeGradeAmount
        {
            get
            {
                return (SortingBars[SortingBars.Length - 1] + SortingBars[SortingBars.Length - 2] + SortingBars[SortingBars.Length - 3]) / 1000;
            }
        }

        public int[] SortingBars;
        public int[] OriginSortingBars;

        public int ModeGradeIndex
        {
            get
            {
                return SortingBars[SortingBars.Length - 1] % 1000;
            }
        }

        public int GetGradeIndex(int index)
        {
            return SortingBars[SortingBars.Length - index - 1] % 1000;
        }
        public int GetGrade(int index)
        {
            return SortingBars[SortingBars.Length - index - 1] / 1000;
        }
        public DataHistogramClass(int barrange,int gap)
        {
            Gap = gap;

            BarRange = (int)Math.Ceiling(barrange / (double)Gap) + 1;
            SortingBars = new int[BarRange];
        }

        public int GetMaxRatio(float ratio)
        {
            int i = 0;

            int RatioIndex = (int)((float)TotalPixelForCount * ratio);
            int RatioCount = 0;

            i = OriginSortingBars.Length - 1;

            while (i > -1)
            {
                RatioCount += (OriginSortingBars[i] / 1000);

                if (RatioCount > RatioIndex)
                    break;
                i--;
            }
            return i * Gap;
        }
        public int GetMinRatio(float ratio)
        {
            int i = 0;

            int RatioIndex = (int)((float)TotalPixelForCount * ratio);
            int RatioCount = 0;

            i = 0;

            while (i < OriginSortingBars.Length)
            {
                RatioCount += (OriginSortingBars[i] / 1000);

                if (RatioCount > RatioIndex)
                    break;
                i++;
            }
            return i * Gap;
        }

        public void Reset()
        {
            MaxGrade = -1000;
            MinGrade = 1000;
            ModeGrade = 0;

            TotalGrade = 0;
            MeanGrade = 0;

            SortingBars = new int[BarRange];
            //BarsBeSort = new int[BarRange];

            TotalCount = 1;
        }
        public void Complete()
        {
            MeanGrade = TotalGrade / TotalCount;

            int i = 0;
            int MaxValue = -1;

            ModeBarIndex = -1;

            while (i < BarRange)
            {
                if (MaxValue < SortingBars[i])
                {
                    MaxValue = SortingBars[i];
                    ModeBarIndex = i;
                }

                SortingBars[i] += i;
                i++;
            }
            ModeGrade = Math.Min(ModeBarIndex * Gap, BarRange);

            OriginSortingBars = new int[SortingBars.Length];

            Array.Copy(SortingBars, OriginSortingBars, SortingBars.Length);

            Array.Sort(SortingBars);

        }
        public void Add(int Grade)
        {
            MaxGrade = Math.Max(Grade, MaxGrade);
            MinGrade = Math.Min(Grade, MinGrade);

            int i = (int)Math.Ceiling(Grade / (double)Gap);

            if (i > SortingBars.Length - 1)
                i = SortingBars.Length - 1;

            SortingBars[i] += 1000;

            TotalGrade += Grade;
            TotalCount++;
        }

        public float GetBiggerModeRatio(int threshold)
        {
            int i = Math.Min(ModeBarIndex + 1, threshold / Gap + 1);

            int count = 0;

            while (i < BarRange)
            {
                count += (OriginSortingBars[i] / 1000);
                i++;
            }

            return (float)count / (float)TotalCount;

        }
        public float GetBiggerModeRatioAdv()
        {
            int threshold = Gap;

            int i = 0;
            int imode = Math.Min(ModeBarIndex + 1, threshold / Gap + 1);

            int count = 0;

            float ModeAdv = 0f;
            float ModeCount = 0f;
            float DefModeCount = 0;
            bool IsStartBigger = false;

            while (i < BarRange)
            {
                int singlecount = (OriginSortingBars[i] / 1000);

                if (i < imode)
                {
                    ModeAdv += (float)(singlecount * Gap * i);
                    ModeCount += singlecount;
                }
                else
                {
                    if (!IsStartBigger)
                    {
                        DefModeCount = ((ModeAdv /ModeCount) / Gap);

                        IsStartBigger = true;
                    }

                    count += (int)((float)singlecount * (float)(i / DefModeCount));
                }
                i++;
            }

            return Math.Min(Math.Abs((float)count / (float)TotalCount),20);

        }
        public float GetSmallerModeRatio(int threshold)
        {
            int i = Math.Min(ModeBarIndex + 1, threshold / Gap + 1);

            int count = 0;

            while (i < BarRange)
            {
                count += (OriginSortingBars[i] / 1000);
                i++;
            }

            return (float)count / (float)TotalCount;

        }
        public float GetSmallerModeRatioAdv(int threshold)
        {
            int i = Math.Min(ModeBarIndex + 1, threshold / Gap + 1);

            int count = 0;

            while (i < BarRange)
            {
                count += (OriginSortingBars[i] / 1000);
                i++;
            }

            return (float)count / (float)TotalCount;

        }
    }
    
    public class LineClass
    {
        //Y = aX + b
        //The Axis is Left to Right and Top to Left

        public PointF FirstPt = new PointF();
        public PointF SecondPt = new PointF();

        JzToolsClass myJzTools = new JzToolsClass();

        public double a = 0d;
        public double b = 0d;

        public bool IsSwap = false;

        public LineClass()
        {

        }
        public LineClass(string Str)
        {
            FromString(Str);
        }
        public LineClass(PointF Pt1, PointF Pt2)
        {
            FirstPt = Pt1;
            SecondPt = Pt2;

            FindSlopeEquation(Pt1, Pt2);
        }
        public void FindSlopeEquation()
        {
            FindSlopeEquation(FirstPt, SecondPt);
        }
        void FindSlopeEquation(PointF Pt1, PointF Pt2)
        {
            a = (double)(Pt2.Y - Pt1.Y) / (double)(Pt2.X - Pt1.X);
            b = (double)Pt1.Y - a * (double)Pt1.X;
        }
        public PointF FindIntersection(LineClass line)
        {
            PointF retptf = new PointF(-1, -1);

            if (double.IsInfinity(line.a) || double.IsInfinity(line.b))
            {
                retptf.X = line.FirstPt.X;
                retptf.Y = (float)(a * retptf.X + b);
            }
            else if (double.IsInfinity(a) || double.IsInfinity(b))
            {
                retptf.X = FirstPt.X;
                retptf.Y = (float)(line.a * retptf.X + line.b);
            }
            else
            {
                retptf.X = (float)((line.b - b) / (a - line.a));
                retptf.Y = (float)(a * retptf.X + b);
            }
            return retptf;
        }
        public double GetVerticalLength(PointF Pt1)
        {
            double ret = 0;

            if(IsSwap)
                ret = Math.Abs(Pt1.X - a * Pt1.Y -b) / Math.Sqrt(Math.Pow(1, 2) + Math.Pow(a, 2));
            else
                ret = Math.Abs(a * Pt1.X + b - Pt1.Y) / Math.Sqrt(Math.Pow(a, 2) + Math.Pow(-1, 2));

            return ret;
        }
        public PointF GetXYLengthLocation(PointF Pt1, PointF PtDatum)
        {
            PointF retptf = new PointF();

            double SlopeLength = myJzTools.GetPointLength(Pt1, PtDatum);

            retptf.Y = (float)GetVerticalLength(Pt1);

            retptf.X = (float)(Math.Cos(Math.Asin(retptf.Y / SlopeLength)) * SlopeLength);


            return retptf;
        }
        public PointF GetPtFromY(float yvalue)
        {
            if (FirstPt.X == SecondPt.X)
            {
                return new PointF(FirstPt.X, yvalue);
            }
            else
            {
                double x = (yvalue - b) / a;

                return new PointF((float)x, yvalue);
            }
        }
        public PointF GetPtFromX(float xvalue)
        {
            if (FirstPt.Y == SecondPt.Y)
            {
                return new PointF(xvalue, FirstPt.Y);
            }
            else
            {
                double y = a * xvalue + b;

                return new PointF(xvalue, (float)y);
            }
        }
        public float GetRotationAngle(PointF pt1,PointF pt2)
        {
            double orgrad = Math.Asin((SecondPt.Y - FirstPt.Y) / (SecondPt.X - FirstPt.X));
            double runrad = Math.Asin((pt2.Y - pt1.Y) / (pt2.X - pt1.X));

            return (float)(((runrad - orgrad) * 180d) / Math.PI);

        }
        public override string ToString()
        {
            string Str = "";

            Str += a.ToString() + "@";
            Str += b.ToString() + "@";
            Str += myJzTools.PointFToString(FirstPt) + "@";
            Str += myJzTools.PointFToString(SecondPt);

            return Str;
        }
        public void FromString(string Str)
        {
            string[] strs = Str.Split('@');

            a = double.Parse(strs[0]);
            b = double.Parse(strs[1]);
            FirstPt = myJzTools.StringToPointF(strs[2]);
            SecondPt = myJzTools.StringToPointF(strs[3]);
        }

    }
    public class FoundClass
    {
        public Rectangle rect;

        public int Width = 0;
        public int Height = 0;
        public Point Location = new Point();
        public Point Center = new Point();
        public int Area = 0;
        public Point FirstPoint = new Point();
        public bool IsChecked = false;
        public JRotatedRectangleF rotatedRectangleF = new JRotatedRectangleF();

        JzToolsClass myJzTools = new JzToolsClass();

        public FoundClass()
        {

        }

        public FoundClass(Rectangle rRect,int rArea)
        {
            rect = rRect;
            Width = rect.Width;
            Height = rect.Height;
            Location = rect.Location;
            Area = rArea;

            Center = new Point(rect.X + (rect.Width >> 1), rect.Y + (rect.Height >> 1));

            FirstPoint = new Point();
        }
        public FoundClass(Rectangle rRect, int rArea,Point firstpoint)
        {
            rect = rRect;
            Width = rect.Width;
            Height = rect.Height;
            Location = rect.Location;
            Area = rArea;

            Center = new Point(rect.X + (rect.Width >> 1), rect.Y + (rect.Height >> 1));

            FirstPoint = firstpoint;
        }
        public override string ToString()
        {
            return myJzTools.RecttoString(rect);
        }

        public FoundClass Clone()
        {
            FoundClass found = new FoundClass();

            found.rect = rect;
            found.Width = Width;
            found.Height = Height;
            found.Location = Location;
            found.Center = Center;
            found.Area = Area;

            return found;
        }

    }

    public class JzFindObjectClass
    {
        public List<FoundClass> FoundList = new List<FoundClass>();
        bool IsDebug
        {
            get
            {
                return false;
            }
        }

        int LEFT = 0;
        int RIGHT = 0;
        int WIDTH = 0;

        int TOP = 0;
        int BOTTOM = 0;
        int HEIGHT = 0;

        int AREA = 0;
        int RECTAREA = 0;

        //JzToolsClass myJzTools = new JzToolsClass();

        public int Rank = 30;
        public List<int> ListRectLength = new List<int>();
        public List<Rectangle> RectList = new List<Rectangle>();

        public Rectangle GetRectNearest(Point NearCenter, Rectangle rectBase, int XRange, int YMinRange, int YMaxRange)
        {
            RectList.Clear();

            Rectangle retRect = new Rectangle();
            int MinDistance = 10000;

            foreach (FoundClass found in FoundList)
            {
                if (found.rect.Width < 20 || found.rect.Height < 10)
                    continue;

                if (IsInRange(found.rect.Width, rectBase.Width, (int)((double)rectBase.Width * 0.7)) && IsInRange(found.rect.Height, rectBase.Height, (int)((double)rectBase.Height * 0.7)))
                {
                    if (Math.Abs(NearCenter.X - GetRectCenter(found.rect).X) < XRange && ((GetRectCenter(found.rect).Y - NearCenter.Y > YMinRange && GetRectCenter(found.rect).Y - NearCenter.Y < YMaxRange)))
                    {
                        RectList.Add(found.rect);
                        if (MinDistance > Math.Abs(GetPointLength(NearCenter, GetRectCenter(found.rect))))
                        {
                            MinDistance = Math.Abs(GetPointLength(NearCenter, GetRectCenter(found.rect)));
                            retRect = found.rect;
                        }
                    }
                }

            }

            return retRect;
        }
        public Rectangle GetRectNearestEX(Point NearCenter, Rectangle rectBase, int XRange, int YMinRange, int YMaxRange)
        {
            RectList.Clear();

            Rectangle retRect = new Rectangle();
            int MaxDistance = -10000;

            foreach (FoundClass found in FoundList)
            {
                if (found.rect.Width < 20 || found.rect.Height < 10 || found.rect.Y < 5)
                    continue;

                if (IsInRange(found.rect.Width, rectBase.Width, (int)((double)rectBase.Width * 0.7)) && IsInRange(found.rect.Height, rectBase.Height, (int)((double)rectBase.Height * 0.7)))
                {
                    if (Math.Abs(NearCenter.X - GetRectCenter(found.rect).X) < XRange && ((GetRectCenter(found.rect).Y - NearCenter.Y > YMinRange && GetRectCenter(found.rect).Y - NearCenter.Y < YMaxRange)))
                    {
                        RectList.Add(found.rect);
                        if (MaxDistance < Math.Abs(NearCenter.Y - GetRectCenter(found.rect).Y))
                        {
                            MaxDistance = Math.Abs(NearCenter.Y - GetRectCenter(found.rect).Y);
                            retRect = found.rect;
                        }
                    }
                }

            }
            return retRect;
        }
        public Rectangle rectMaxRect
        {
            get
            {
                int MaxIndex = GetMaxRectIndex();

                if (MaxIndex == -1)
                    return new Rectangle();
                else
                    return FoundList[MaxIndex].rect;
            }
        }

        public int Count
        {
            get
            {
                return FoundList.Count;
            }

        }
        public Rectangle GetRect(int Index)
        {
            if ((FoundList.Count - 1) < Index)
                return new Rectangle();

            return FoundList[Index].rect;
        }
        public Rectangle GetRect(double MinArea, double MaxArea)
        {
            int i = 0;
            Rectangle Rect = new Rectangle();

            while (i < FoundList.Count)
            {
                if (FoundList[i].Area > MinArea && FoundList[i].Area < MaxArea)
                {
                    Rect = MergeTwoRects(Rect, FoundList[i].rect);
                }
                i++;
            }

            return Rect;
        }
        public Rectangle GetRect(double MinArea, double MaxArea, ref double allarea)
        {
            int i = 0;
            Rectangle Rect = new Rectangle();
            allarea = 0;

            while (i < FoundList.Count)
            {
                if (FoundList[i].Area > MinArea && FoundList[i].Area < MaxArea)
                {
                    Rect = MergeTwoRects(Rect, FoundList[i].rect);
                    allarea += FoundList[i].Area;
                }
                i++;
            }

            return Rect;
        }
        public Rectangle GetRectBySort(int Index)
        {
            if ((FoundList.Count - 1) < Index)
                return new Rectangle();

            int No = int.Parse(SortingList[Index].Split(',')[1]);

            return FoundList[No].rect;
        }
        public FoundClass GetFoundBySort(int Index)
        {
            int No = int.Parse(SortingList[Index].Split(',')[1]);

            return FoundList[No];

        }

        public bool IsCheckAreaOK(int area)
        {
            bool IsOK = true;

            foreach (FoundClass found in FoundList)
            {
                if (found.Area >= area)
                {
                    IsOK = false;
                    break;
                }
            }
            return IsOK;
        }
        public int GetArea(int Index)
        {
            if (FoundList.Count == 0)
                return -1;
            else
                return FoundList[Index].Area;
        }
        public int GetArea()
        {
            int ret = 0;

            foreach (FoundClass found in FoundList)
            {
                ret += found.Area;
            }

            return ret;
        }

        public int GetMaxArea()
        {
            int ret = 0;

            foreach (FoundClass found in FoundList)
            {
                if (found.Area > ret)
                    ret = found.Area;
            }

            return ret;
        }
        public int GetMaxArea(ref int index)
        {
            int i = 0;
            int ret = 0;

            foreach (FoundClass found in FoundList)
            {
                if (found.Area > ret)
                {
                    ret = found.Area;
                    index = i;
                }
                i++;
            }

            return ret;
        }
        public int GetMaxArea(int discardwidth, int discardheght)
        {
            int ret = 0;

            foreach (FoundClass found in FoundList)
            {
                if (found.Width > discardwidth && found.Height > discardheght)
                {
                    if (found.Area > ret)
                        ret = found.Area;
                }
            }

            return ret;
        }
        public Point GetRectCenter(int Index)
        {
            return GetRectCenter(FoundList[Index].rect);
        }
        public Rectangle GetRect() //Get The Whole Rectangle
        {
            return GetRect(true, 20);
        }
        public Rectangle GetRect(bool isbigger, int filterarea) //Get The Whole Rectangle
        {
            int i = 0;
            Rectangle Rect = new Rectangle();

            while (i < FoundList.Count)
            {
                bool ispass = false;

                if (isbigger)
                {
                    ispass = FoundList[i].Area > filterarea;
                }
                else
                {
                    ispass = FoundList[i].Area <= filterarea;
                }

                if (ispass)
                {
                    Rect = MergeTwoRects(Rect, FoundList[i].rect);
                }
                i++;
            }

            return Rect;
        }
        public Rectangle GetRect(double AreaThreshold, bool IsArea) //Get The Whole Rectangle
        {
            int i = 0;
            Rectangle Rect = new Rectangle();

            while (i < FoundList.Count)
            {
                if (FoundList[i].Area > AreaThreshold)
                {
                    Rect = MergeTwoRects(Rect, FoundList[i].rect);
                }
                i++;
            }

            return Rect;
        }

        public int GetMaxRectIndex()
        {
            int MaxArea = -100;
            int retIndex = -1;
            int i = 0;

            foreach (FoundClass found in FoundList)
            {
                if (MaxArea < found.Area)
                {
                    MaxArea = found.Area;
                    retIndex = i;
                }
                i++;
            }

            return retIndex;
        }

        public Rectangle GetRect(Rectangle IntersectRect)
        {
            int i = 0;
            Rectangle Rect = new Rectangle();

            while (i < FoundList.Count)
            {
                if (FoundList[i].rect.IntersectsWith(IntersectRect))
                    Rect = MergeTwoRects(Rect, FoundList[i].rect);
                i++;
            }
            return Rect;
        }
        public Point GetCornerGroup(Point Pt, Rectangle Rect)
        {
            int i = 0;
            int iTmp = 0;
            int MinLength = int.MinValue;
            Point MinPt = Pt;
            Rectangle RectTmp = new Rectangle();

            i = 0;
            ListRectLength.Clear();

            while (i < Count)
            {
                iTmp = (int)(GetLTSuggestion(Pt, GetRectCenter(i), Rect) * 1000000) / 1000;

                ListRectLength.Add(iTmp * 1000 + i);

                if (iTmp > MinLength)
                {
                    MinLength = iTmp;
                    MinPt = GetRectCenter(i);
                }
                i++;
            }
            ListRectLength.Sort();

            i = 0;
            while (i < Rank)
            {
                RectTmp = MergeTwoRects(RectTmp, GetRect(ListRectLength[ListRectLength.Count - i - 1] % 1000));

                i++;
            }

            return GetRectCenter(RectTmp);
        }

        public JzFindObjectClass()
        {

        }
        void Reset(int X, int Y)
        {
            LEFT = X;
            RIGHT = X;

            TOP = Y;
            BOTTOM = Y;

            WIDTH = 1;
            HEIGHT = 1;

            AREA = 0;
            RECTAREA = 0;
        }
        public void Find(Bitmap bmp, Color FillColor)
        {
            try
            {
                Find(bmp, FillColor, new Size(int.MinValue, int.MinValue), new Size(int.MaxValue, int.MaxValue));
            }
            catch (Exception ex)
            {
                //JetEazy.LoggerClass.Instance.WriteException(ex);
                FoundList.Clear();
                FoundClass found = new FoundClass(SimpleRect(bmp.Size), bmp.Width * bmp.Height);

                FoundList.Add(found);

            }
        }
        public void Find(Bitmap bmp, Color FillColor, Size OKSize, Size NGSize)
        {
            Find(bmp, SimpleRect(bmp.Size), FillColor, OKSize, NGSize);
        }

        object obj = new object();

        public void Find(Bitmap bmp, Rectangle Rect, Color FillColor, Size OKSize, Size NGSize)
        {
            //lock (obj)
            {

                uint ColorValue = (uint)((FillColor.A << 24) + (FillColor.R << 16) + (FillColor.G << 8) + FillColor.B);

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

                        FoundList.Clear();

                        while (y < ymax)
                        {
                            x = xmin;
                            pucPtr = pucStart;
                            while (x < xmax)
                            {
                                if (pucPtr[0] == 0xFF) //<== 禁止填藍色!!!!，僅紅，綠，黃用較好
                                {
                                    Reset(x, y);
                                    Find(iStride, pucPtr, x, y, rectbmp, ColorValue);

                                    WIDTH = RIGHT - LEFT + 1;
                                    HEIGHT = BOTTOM - TOP + 1;
                                    RECTAREA = WIDTH * HEIGHT;

                                    //if ((OKSize.Width <= WIDTH && OKSize.Height <= HEIGHT) && !(NGSize.Width >= WIDTH && NGSize.Height >= HEIGHT))
                                    if ((OKSize.Width <= WIDTH && OKSize.Height <= HEIGHT) && (NGSize.Width >= WIDTH || NGSize.Height >= HEIGHT))
                                        FoundList.Add(new FoundClass(new Rectangle(LEFT, TOP, WIDTH, HEIGHT), AREA, new Point(x, y)));
                                }
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
                    bmp.UnlockBits(bmpData);

                    if (IsDebug)
                        MessageBox.Show("Error :" + ex.ToString());
                }
            }
        }
        public void Find(Bitmap bmp, Rectangle Rect, Color FillColor, int OKArea)
        {
            //lock (obj)
            {

                uint ColorValue = (uint)((FillColor.A << 24) + (FillColor.R << 16) + (FillColor.G << 8) + FillColor.B);

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

                        FoundList.Clear();

                        while (y < ymax)
                        {
                            x = xmin;
                            pucPtr = pucStart;
                            while (x < xmax)
                            {
                                if (pucPtr[0] == 0xFF) //<== 禁止填藍色!!!!，僅紅，綠，黃用較好
                                {
                                    Reset(x, y);
                                    Find(iStride, pucPtr, x, y, rectbmp, ColorValue);

                                    WIDTH = RIGHT - LEFT + 1;
                                    HEIGHT = BOTTOM - TOP + 1;
                                    RECTAREA = WIDTH * HEIGHT;

                                    if (AREA > OKArea)
                                        FoundList.Add(new FoundClass(new Rectangle(LEFT, TOP, WIDTH, HEIGHT), AREA, new Point(x, y)));
                                }
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
                    bmp.UnlockBits(bmpData);

                    if (IsDebug)
                        MessageBox.Show("Error :" + ex.ToString());
                }
            }
        }
        public void Find(Bitmap bmp, Rectangle Rect, Color FillColor, Size OKSize, Size NGSize, int Ratio)
        {
            uint ColorValue = (uint)((FillColor.A << 24) + (FillColor.R << 16) + (FillColor.G << 8) + FillColor.B);

            Rectangle rectbmp = Rect;
            BitmapData bmpData = bmp.LockBits(rectbmp, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            IntPtr Scan0 = bmpData.Scan0;
            double CheckRatio = Ratio / 100d;
            double FoundRatio = 0;

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

                    FoundList.Clear();

                    while (y < ymax)
                    {
                        x = xmin;
                        pucPtr = pucStart;
                        while (x < xmax)
                        {
                            if (pucPtr[0] == 0xFF) //<== 禁止填藍色!!!!，僅紅，綠，黃用較好
                            {
                                Reset(x, y);
                                Find(iStride, pucPtr, x, y, rectbmp, ColorValue);

                                WIDTH = RIGHT - LEFT + 1;
                                HEIGHT = BOTTOM - TOP + 1;
                                RECTAREA = WIDTH * HEIGHT;

                                FoundRatio = (double)AREA / (double)RECTAREA;

                                if (FoundRatio >= CheckRatio)
                                {
                                    if ((OKSize.Width <= WIDTH && OKSize.Height <= HEIGHT) && !(NGSize.Width >= WIDTH && NGSize.Height >= HEIGHT))
                                        FoundList.Add(new FoundClass(new Rectangle(LEFT, TOP, WIDTH, HEIGHT), AREA));
                                }
                            }
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
                bmp.UnlockBits(bmpData);

                if (IsDebug)
                    MessageBox.Show("Error :" + ex.ToString());
            }
        }
        public void FindEx(Bitmap bmp, Rectangle Rect, Color FillColor, Size OKSize, Size NGSize, int MinHeight)
        {
            uint ColorValue = (uint)((FillColor.A << 24) + (FillColor.R << 16) + (FillColor.G << 8) + FillColor.B);

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

                    FoundList.Clear();

                    while (y < ymax)
                    {
                        x = xmin;
                        pucPtr = pucStart;
                        while (x < xmax)
                        {
                            if (pucPtr[0] == 0xFF) //<== 禁止填藍色!!!!，僅紅，綠，黃用較好
                            {
                                Reset(x, y);
                                Find(iStride, pucPtr, x, y, rectbmp, ColorValue);

                                WIDTH = RIGHT - LEFT + 1;
                                HEIGHT = BOTTOM - TOP + 1;
                                RECTAREA = WIDTH * HEIGHT;

                                if (HEIGHT >= MinHeight)
                                {
                                    if ((OKSize.Width <= WIDTH && OKSize.Height <= HEIGHT) && !(NGSize.Width >= WIDTH && NGSize.Height >= HEIGHT))
                                        FoundList.Add(new FoundClass(new Rectangle(LEFT, TOP, WIDTH, HEIGHT), AREA));
                                }
                            }
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
                bmp.UnlockBits(bmpData);

                if (IsDebug)
                    MessageBox.Show("Error :" + ex.ToString());
            }
        }

        public void Find(Bitmap bmp, Point PtStart, Color FillColor)
        {
            uint ColorValue = (uint)((FillColor.A << 24) + (FillColor.R << 16) + (FillColor.G << 8) + FillColor.B);

            Rectangle rectbmp = SimpleRect(bmp.Size);
            BitmapData bmpData = bmp.LockBits(rectbmp, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            IntPtr Scan0 = bmpData.Scan0;
            try
            {
                unsafe
                {
                    byte* scan0 = (byte*)(void*)Scan0;
                    byte* pucStart;

                    int xmin = rectbmp.X;
                    int ymin = rectbmp.Y;
                    int xmax = xmin + rectbmp.Width;
                    int ymax = ymin + rectbmp.Height;

                    int x = PtStart.X;
                    int y = PtStart.Y;
                    int iStride = bmpData.Stride;

                    //y = ymin;
                    pucStart = scan0 + ((x - xmin) << 2) + (iStride * (y - ymin));
                    FoundList.Clear();

                    if (pucStart[0] == 0xFF) //<== 禁止填藍色!!!!，僅紅，綠，黃用較好
                    {
                        Reset(x, y);
                        Find(iStride, pucStart, x, y, rectbmp, ColorValue);

                        WIDTH = RIGHT - LEFT + 1;
                        HEIGHT = BOTTOM - TOP + 1;
                        RECTAREA = WIDTH * HEIGHT;

                        FoundList.Add(new FoundClass(new Rectangle(LEFT, TOP, WIDTH, HEIGHT), AREA));
                    }
                    bmp.UnlockBits(bmpData);
                }
            }
            catch (Exception ex)
            {
                //JetEazy.LoggerClass.Instance.WriteException(ex);
                bmp.UnlockBits(bmpData);

                if (IsDebug)
                    MessageBox.Show("Error :" + ex.ToString());
            }
        }
        unsafe void Find(int strid, byte* pucPtr, int x, int y, Rectangle rect, uint FillColorValue)
        {
            //byte* pucStart = pucPtr;

            try
            {
                int xmin = rect.X;
                int xmax = rect.X + rect.Width - 1;
                int ymin = rect.Y;
                int ymax = rect.Y + rect.Height - 1;

                byte* pucStart = pucPtr;

                *((uint*)pucPtr) = 0xFFFFFFFF;

                int xLeft = x;
                int xRight = x;

                ///////////////////GO LEFT
                while (xLeft >= xmin && pucPtr[0] != 0) // Converge to xmin
                {
                    *((uint*)pucPtr) = FillColorValue;

                    AREA++;
                    pucPtr -= 4;
                    xLeft--;
                }
                xLeft++;

                LEFT = Math.Min(LEFT, xLeft);

                ////////////////////////////

                pucPtr = pucStart;
                *((uint*)pucPtr) = 0xFFFFFFFF;

                ///////////////////GO RIGHT
                while (xRight <= xmax && pucPtr[0] != 0) //Converge to xmax
                {
                    *((uint*)pucPtr) = FillColorValue;

                    AREA++;
                    pucPtr += 4;
                    xRight++;
                }
                xRight--;
                ////////////////////////////

                RIGHT = Math.Max(RIGHT, xRight);

                //AREA += (RIGHT - LEFT) + 1;

                while (xLeft <= xRight)
                {
                    if (y - 1 >= ymin)
                    {
                        pucPtr = pucStart - ((x - xLeft) << 2);
                        pucPtr -= strid;
                        if (pucPtr[0] == 0xFF)
                        {
                            TOP = Math.Min(y - 1, TOP);
                            Find(strid, pucPtr, xLeft, y - 1, rect, FillColorValue);
                        }
                    }
                    if (y + 1 <= ymax)
                    {
                        pucPtr = pucStart - ((x - xLeft) << 2);
                        pucPtr += strid;
                        if (pucPtr[0] == 0xFF)
                        {
                            BOTTOM = Math.Max(y + 1, BOTTOM);
                            Find(strid, pucPtr, xLeft, y + 1, rect, FillColorValue);
                        }
                    }
                    xLeft++;
                }
            }
            catch (Exception ex)
            {
                //JetEazy.LoggerClass.Instance.WriteException(ex);
                LEFT = rect.Left;
                RIGHT = rect.Right;
                BOTTOM = rect.Bottom;
                TOP = rect.Top;
                AREA = (RIGHT - LEFT) * (BOTTOM - TOP);
            }
        }

        protected List<string> SortingList = new List<string>();
        public void SortByArea()
        {
            int i = 0;

            SortingList.Clear();

            foreach (FoundClass found in FoundList)
            {
                SortingList.Add(found.Area.ToString("00000") + "," + i.ToString());
                i++;
            }

            SortingList.Sort();
            SortingList.Reverse();
        }
        public void SortByY(bool IsBiggerFirst)
        {
            int i = 0;

            SortingList.Clear();

            foreach (FoundClass found in FoundList)
            {
                SortingList.Add(found.rect.Y.ToString("00000") + "," + i.ToString());
                i++;
            }

            SortingList.Sort();

            if (IsBiggerFirst)
                SortingList.Reverse();
        }
        public void FindGrayscale(Bitmap bmp, Point PtStart, Color FillColor, int ColorRange)
        {
            uint ColorValue = (uint)((FillColor.A << 24) + (FillColor.R << 16) + (FillColor.G << 8) + FillColor.B);

            Rectangle rectbmp = SimpleRect(bmp.Size);
            BitmapData bmpData = bmp.LockBits(rectbmp, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            IntPtr Scan0 = bmpData.Scan0;
            try
            {
                unsafe
                {
                    byte* scan0 = (byte*)(void*)Scan0;
                    byte* pucStart;

                    int xmin = rectbmp.X;
                    int ymin = rectbmp.Y;
                    int xmax = xmin + rectbmp.Width;
                    int ymax = ymin + rectbmp.Height;

                    int x = PtStart.X;
                    int y = PtStart.Y;
                    int iStride = bmpData.Stride;

                    //y = ymin;
                    pucStart = scan0 + ((x - xmin) << 2) + (iStride * (y - ymin));
                    FoundList.Clear();


                    int BaseColor = pucStart[0];
                    //if (pucStart[0] == 0xFF) //<== 禁止填藍色!!!!，僅紅，綠，黃用較好
                    {
                        Reset(x, y);
                        FindGrayscale(iStride, pucStart, x, y, rectbmp, ColorValue, BaseColor, ColorRange);

                        WIDTH = RIGHT - LEFT + 1;
                        HEIGHT = BOTTOM - TOP + 1;
                        RECTAREA = WIDTH * HEIGHT;

                        FoundList.Add(new FoundClass(new Rectangle(LEFT, TOP, WIDTH, HEIGHT), AREA));
                    }
                    bmp.UnlockBits(bmpData);
                }
            }
            catch (Exception ex)
            {
                //JetEazy.LoggerClass.Instance.WriteException(ex);
                bmp.UnlockBits(bmpData);

                if (IsDebug)
                    MessageBox.Show("Error :" + ex.ToString());
            }
        }
        public void FindGrayscale(Bitmap bmp, Point PtStart, Color FillColor, int BaseColor, int ColorRange)
        {
            uint ColorValue = (uint)((FillColor.A << 24) + (FillColor.R << 16) + (FillColor.G << 8) + FillColor.B);

            Rectangle rectbmp = SimpleRect(bmp.Size);
            BitmapData bmpData = bmp.LockBits(rectbmp, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            IntPtr Scan0 = bmpData.Scan0;
            try
            {
                unsafe
                {
                    byte* scan0 = (byte*)(void*)Scan0;
                    byte* pucStart;

                    int xmin = rectbmp.X;
                    int ymin = rectbmp.Y;
                    int xmax = xmin + rectbmp.Width;
                    int ymax = ymin + rectbmp.Height;

                    int x = PtStart.X;
                    int y = PtStart.Y;
                    int iStride = bmpData.Stride;

                    //y = ymin;
                    pucStart = scan0 + ((x - xmin) << 2) + (iStride * (y - ymin));
                    FoundList.Clear();

                    //if (pucStart[0] == 0xFF) //<== 禁止填藍色!!!!，僅紅，綠，黃用較好
                    {
                        Reset(x, y);
                        FindGrayscale(iStride, pucStart, x, y, rectbmp, ColorValue, BaseColor, ColorRange);

                        WIDTH = RIGHT - LEFT + 1;
                        HEIGHT = BOTTOM - TOP + 1;
                        RECTAREA = WIDTH * HEIGHT;

                        FoundList.Add(new FoundClass(new Rectangle(LEFT, TOP, WIDTH, HEIGHT), AREA));
                    }
                    bmp.UnlockBits(bmpData);
                }
            }
            catch (Exception ex)
            {
                //JetEazy.LoggerClass.Instance.WriteException(ex);
                bmp.UnlockBits(bmpData);

                if (IsDebug)
                    MessageBox.Show("Error :" + ex.ToString());
            }
        }
        public void FindGrayscale(Bitmap bmp, Color FillColor, int BaseColor, int ColorRange, int BaseRange)
        {
            uint ColorValue = (uint)((FillColor.A << 24) + (FillColor.R << 16) + (FillColor.G << 8) + FillColor.B);

            Rectangle rectbmp = SimpleRect(bmp.Size);
            BitmapData bmpData = bmp.LockBits(rectbmp, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            IntPtr Scan0 = bmpData.Scan0;
            try
            {
                unsafe
                {
                    byte* scan0 = (byte*)(void*)Scan0;
                    byte* pucPtr;
                    byte* pucStart;
                    byte* pucStart1;

                    int xmin = rectbmp.X;
                    int ymin = rectbmp.Y;
                    int xmax = xmin + rectbmp.Width;
                    int ymax = ymin + rectbmp.Height;

                    int x = xmin;
                    int y = ymin;
                    int iStride = bmpData.Stride;

                    FoundList.Clear();

                    pucStart = scan0 + ((x - xmin) << 2) + (iStride * (y - ymin));

                    y = ymin;

                    while (y < ymax)//if (pucStart[0] == 0xFF) //<== 禁止填藍色!!!!，僅紅，綠，黃用較好
                    {
                        x = xmin;
                        pucPtr = pucStart;

                        while (x < xmax)
                        {
                            int Greyscle = GrayscaleInt(pucPtr[2], pucPtr[1], pucPtr[0]);

                            if (IsInRange(Greyscle, BaseColor, BaseRange))
                            {
                                Reset(x, y);

                                //pucStart1 = scan0 + ((x - xmin) << 2) + (iStride * (y - ymin));

                                FindGrayscale(iStride, pucPtr, x, y, rectbmp, ColorValue, BaseColor, ColorRange);

                                WIDTH = RIGHT - LEFT + 1;
                                HEIGHT = BOTTOM - TOP + 1;
                                RECTAREA = WIDTH * HEIGHT;

                                FoundList.Add(new FoundClass(new Rectangle(LEFT, TOP, WIDTH, HEIGHT), AREA));
                            }

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
                bmp.UnlockBits(bmpData);

                if (IsDebug)
                    MessageBox.Show("Error :" + ex.ToString());
            }
        }
        unsafe void FindGrayscale(int strid, byte* pucPtr, int x, int y, Rectangle rect, uint FillColorValue, int BaseColor, int ColorRange)
        {
            //byte* pucStart = pucPtr;

            int xmin = rect.X;
            int xmax = rect.X + rect.Width - 1;
            int ymin = rect.Y;
            int ymax = rect.Y + rect.Height - 1;

            byte* pucStart = pucPtr;

            *((uint*)pucPtr) = 0xFFFFFFFF;
            pucPtr[0] = (byte)BaseColor;

            int xLeft = x;
            int xRight = x;

            ///////////////////GO LEFT
            while (xLeft >= xmin && (IsInRange((int)pucPtr[0], BaseColor, ColorRange)) && *((uint*)pucPtr) != FillColorValue) // Converge to xmin
            {
                *((uint*)pucPtr) = FillColorValue;

                AREA++;
                pucPtr -= 4;
                xLeft--;
            }
            xLeft++;

            LEFT = Math.Min(LEFT, xLeft);

            ////////////////////////////

            pucPtr = pucStart;

            *((uint*)pucPtr) = 0xFFFFFFFF;
            pucPtr[0] = (byte)BaseColor;

            ///////////////////GO RIGHT
            while (xRight <= xmax && (IsInRange((int)pucPtr[0], BaseColor, ColorRange)) && *((uint*)pucPtr) != FillColorValue) //Converge to xmax
            {
                *((uint*)pucPtr) = FillColorValue;

                AREA++;
                pucPtr += 4;
                xRight++;
            }
            xRight--;
            ////////////////////////////

            RIGHT = Math.Max(RIGHT, xRight);

            //AREA += (RIGHT - LEFT) + 1;

            while (xLeft <= xRight)
            {
                if (y - 1 >= ymin)
                {
                    pucPtr = pucStart - ((x - xLeft) << 2);
                    pucPtr -= strid;
                    if (IsInRange((int)pucPtr[0], BaseColor, ColorRange) && *((uint*)pucPtr) != FillColorValue)
                    {
                        TOP = Math.Min(y - 1, TOP);
                        FindGrayscale(strid, pucPtr, xLeft, y - 1, rect, FillColorValue, BaseColor, ColorRange);
                    }
                }
                if (y + 1 <= ymax)
                {
                    pucPtr = pucStart - ((x - xLeft) << 2);
                    pucPtr += strid;
                    if (IsInRange((int)pucPtr[0], BaseColor, ColorRange) && *((uint*)pucPtr) != FillColorValue)
                    {
                        BOTTOM = Math.Max(y + 1, BOTTOM);
                        FindGrayscale(strid, pucPtr, xLeft, y + 1, rect, FillColorValue, BaseColor, ColorRange);
                    }
                }
                xLeft++;
            }
        }
        public void SetThreshold(Bitmap bmp, int ThresholdValue, int ThresholdRangeUpper, int ThresholdRangeLower)
        {
            int Grade = 0;

            Rectangle rectbmp = SimpleRect(bmp.Size);
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

                    int ThresholdValueMax = Math.Min(255, ThresholdValue + ThresholdRangeUpper);
                    int ThresholdValueMin = Math.Max(0, ThresholdValue - ThresholdRangeLower);

                    y = ymin;
                    pucStart = scan0 + ((x - xmin) << 2) + (iStride * (y - ymin));

                    while (y < ymax)
                    {
                        x = xmin;
                        pucPtr = pucStart;
                        while (x < xmax)
                        {
                            Grade = GrayscaleInt(pucPtr[2], pucPtr[1], pucPtr[0]);

                            *((uint*)pucPtr) = (IsInRangeEx(Grade, ThresholdValueMax, ThresholdValueMin) ? 0xFF000000 : 0xFFFFFFFF);

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
                bmp.UnlockBits(bmpData);

                if (IsDebug)
                    MessageBox.Show("Error :" + ex.ToString());
            }
        }
        public void SetThreshold(Bitmap bmp, Rectangle Rect, int ThresholdValue, int ThresholdRangeUpper, int ThresholdRangeLower)
        {
            SetThreshold(bmp, Rect, ThresholdValue, ThresholdRangeUpper, ThresholdRangeLower, false);
        }
        public void SetThreshold(Bitmap bmp, Rectangle Rect, int ThresholdValue, int ThresholdRangeUpper, int ThresholdRangeLower, bool IsInRangeColorWhite)
        {
            lock (obj)
            {
                int Grade = 0;

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

                        int ThresholdValueMax = Math.Min(255, ThresholdValue + ThresholdRangeUpper);
                        int ThresholdValueMin = Math.Max(0, ThresholdValue - ThresholdRangeLower);

                        uint InRangeColor = 0xFF000000;
                        uint OutrangeColor = 0xFFFFFFFF;

                        if (IsInRangeColorWhite)
                        {
                            InRangeColor = 0xFFFFFFFF;
                            OutrangeColor = 0xFF000000;
                        }

                        y = ymin;
                        pucStart = scan0 + ((x - xmin) << 2) + (iStride * (y - ymin));

                        while (y < ymax)
                        {
                            x = xmin;
                            pucPtr = pucStart;
                            while (x < xmax)
                            {
                                Grade = GrayscaleInt(pucPtr[2], pucPtr[1], pucPtr[0]);

                                *((uint*)pucPtr) = (IsInRangeEx(Grade, ThresholdValueMax, ThresholdValueMin) ? InRangeColor : OutrangeColor);

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
                    bmp.UnlockBits(bmpData);
                    //JetEazy.LoggerClass.Instance.WriteException(ex);
                    if (IsDebug)
                        MessageBox.Show("Error :" + ex.ToString());
                }
            }
        }

        public void SetThreshold(Bitmap bmp, Bitmap bmpmask, int includecolor, int thresholdvalue, int upper, int lower, bool isinrangecolorwhite)
        {
            int Grade = 0;

            Rectangle rectbmp = SimpleRect(bmp.Size);

            BitmapData bmpData = bmp.LockBits(rectbmp, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            BitmapData bmpmaksData = bmpmask.LockBits(rectbmp, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            IntPtr Scan0 = bmpData.Scan0;
            IntPtr maskScan0 = bmpmaksData.Scan0;

            try
            {
                unsafe
                {
                    byte* scan0 = (byte*)(void*)Scan0;
                    byte* pucPtr;
                    byte* pucStart;

                    byte* maskscan0 = (byte*)(void*)maskScan0;
                    byte* maskpucPtr;
                    byte* maskpucStart;

                    int xmin = rectbmp.X;
                    int ymin = rectbmp.Y;
                    int xmax = xmin + rectbmp.Width;
                    int ymax = ymin + rectbmp.Height;

                    int x = xmin;
                    int y = ymin;

                    int iStride = bmpData.Stride;
                    int imaskStride = bmpmaksData.Stride;

                    int ThresholdValueMax = Math.Min(255, thresholdvalue + upper);
                    int ThresholdValueMin = Math.Max(0, thresholdvalue - lower);

                    uint InRangeColor = 0xFF000000;
                    uint OutrangeColor = 0xFFFFFFFF;

                    if (isinrangecolorwhite)
                    {
                        InRangeColor = 0xFFFFFFFF;
                        OutrangeColor = 0xFF000000;
                    }

                    y = ymin;

                    pucStart = scan0 + ((x - xmin) << 2) + (iStride * (y - ymin));
                    maskpucStart = maskscan0 + ((x - xmin) << 2) + (imaskStride * (y - ymin));

                    while (y < ymax)
                    {
                        x = xmin;

                        pucPtr = pucStart;
                        maskpucPtr = maskpucStart;

                        while (x < xmax)
                        {
                            if (maskpucPtr[0] != includecolor)
                            {
                                pucPtr[0] = 0;
                                pucPtr[1] = 0;
                                pucPtr[2] = 0;
                            }
                            else
                            {
                                Grade = GrayscaleInt(pucPtr[2], pucPtr[1], pucPtr[0]);

                                *((uint*)pucPtr) = (IsInRangeEx(Grade, ThresholdValueMax, ThresholdValueMin) ? InRangeColor : OutrangeColor);
                            }

                            pucPtr += 4;
                            maskpucPtr += 4;

                            x++;
                        }

                        pucStart += iStride;
                        maskpucStart += imaskStride;

                        y++;
                    }

                    bmp.UnlockBits(bmpData);
                    bmpmask.UnlockBits(bmpmaksData);
                }
            }
            catch (Exception ex)
            {
                bmp.UnlockBits(bmpData);
                bmpmask.UnlockBits(bmpmaksData);
                //JetEazy.LoggerClass.Instance.WriteException(ex);
                if (IsDebug)
                    MessageBox.Show("Error :" + ex.ToString());
            }
        }

        public void SetThresholdForShow(Bitmap bmp, Bitmap bmpmask, int includecolor, int thresholdvalue, int upper, int lower, bool isinrangecolorwhite)
        {
            int Grade = 0;

            Rectangle rectbmp = SimpleRect(bmp.Size);

            BitmapData bmpData = bmp.LockBits(rectbmp, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            BitmapData bmpmaksData = bmpmask.LockBits(rectbmp, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            IntPtr Scan0 = bmpData.Scan0;
            IntPtr maskScan0 = bmpmaksData.Scan0;

            try
            {
                unsafe
                {
                    byte* scan0 = (byte*)(void*)Scan0;
                    byte* pucPtr;
                    byte* pucStart;

                    byte* maskscan0 = (byte*)(void*)maskScan0;
                    byte* maskpucPtr;
                    byte* maskpucStart;

                    int xmin = rectbmp.X;
                    int ymin = rectbmp.Y;
                    int xmax = xmin + rectbmp.Width;
                    int ymax = ymin + rectbmp.Height;

                    int x = xmin;
                    int y = ymin;

                    int iStride = bmpData.Stride;
                    int imaskStride = bmpmaksData.Stride;

                    int ThresholdValueMax = Math.Min(255, thresholdvalue + upper);
                    int ThresholdValueMin = Math.Max(0, thresholdvalue - lower);

                    uint InRangeColor = 0xFF000000;
                    uint OutrangeColor = 0xFFFFFFFF;

                    if (isinrangecolorwhite)
                    {
                        InRangeColor = 0xFFFFFFFF;
                        OutrangeColor = 0xFF000000;
                    }

                    y = ymin;

                    pucStart = scan0 + ((x - xmin) << 2) + (iStride * (y - ymin));
                    maskpucStart = maskscan0 + ((x - xmin) << 2) + (imaskStride * (y - ymin));

                    while (y < ymax)
                    {
                        x = xmin;

                        pucPtr = pucStart;
                        maskpucPtr = maskpucStart;

                        while (x < xmax)
                        {
                            //if (maskpucPtr[0] != includecolor)
                            //{
                            //    pucPtr[0] = 0;
                            //    pucPtr[1] = 0;
                            //    pucPtr[2] = 0;
                            //}
                            //else
                            //{
                            Grade = GrayscaleInt(pucPtr[2], pucPtr[1], pucPtr[0]);

                            //*((uint*)pucPtr) = (IsInRangeEx(Grade, ThresholdValueMax, ThresholdValueMin) ? InRangeColor : OutrangeColor);

                            if (IsInRangeEx(Grade, ThresholdValueMax, ThresholdValueMin))
                            {
                                pucPtr[0] = 0;
                                pucPtr[1] = 0;
                                pucPtr[2] = 255;
                            }
                            //}
                            pucPtr += 4;
                            maskpucPtr += 4;

                            x++;
                        }

                        pucStart += iStride;
                        maskpucStart += imaskStride;

                        y++;
                    }

                    bmp.UnlockBits(bmpData);
                    bmpmask.UnlockBits(bmpmaksData);
                }
            }
            catch (Exception ex)
            {
                bmp.UnlockBits(bmpData);
                bmpmask.UnlockBits(bmpmaksData);
                //JetEazy.LoggerClass.Instance.WriteException(ex);
                if (IsDebug)
                    MessageBox.Show("Error :" + ex.ToString());
            }
        }

        public void SetThresholdEX(Bitmap bmp, Rectangle Rect, int ThresholdValue, int ThresholdRangeUpper, int ThresholdRangeLower, bool IsInRangeColorWhite)
        {
            int Grade = 0;

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

                    int ThresholdValueMax = Math.Min(255, ThresholdValue + ThresholdRangeUpper);
                    int ThresholdValueMin = Math.Max(0, ThresholdValue - ThresholdRangeLower);

                    uint InRangeColor = 0xFF000000;
                    uint OutrangeColor = 0xFFFFFFFF;

                    if (IsInRangeColorWhite)
                    {
                        InRangeColor = 0xFFFFFFFF;
                        OutrangeColor = 0xFF000000;
                    }

                    y = ymin;
                    pucStart = scan0 + ((x - xmin) << 2) + (iStride * (y - ymin));

                    while (y < ymax)
                    {
                        x = xmin;
                        pucPtr = pucStart;
                        while (x < xmax)
                        {
                            Grade = GrayscaleInt(pucPtr[2], pucPtr[1], pucPtr[0]);

                            if (Grade != 0)
                                *((uint*)pucPtr) = (IsInRangeEx(Grade, ThresholdValueMax, ThresholdValueMin) ? InRangeColor : OutrangeColor);

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
                bmp.UnlockBits(bmpData);
                //JetEazy.LoggerClass.Instance.WriteException(ex);
                if (IsDebug)
                    MessageBox.Show("Error :" + ex.ToString());
            }
        }
        public void SetThresholdEX(Bitmap bmp, Rectangle rect, int rangeupper, int rangelower, bool isinrangecolorwhite, bool isomitzero)
        {
            lock (obj)
            {

                int Grade = 0;

                Rectangle rectbmp = rect;
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

                        int ThresholdValueMax = Math.Min(255, rangeupper);
                        int ThresholdValueMin = Math.Max(0, rangelower);

                        uint inrangecolor = 0xFF000000;
                        uint outrangecolor = 0xFFFFFFFF;

                        if (isinrangecolorwhite)
                        {
                            inrangecolor = 0xFFFFFFFF;
                            outrangecolor = 0xFF000000;
                        }

                        y = ymin;
                        pucStart = scan0 + ((x - xmin) << 2) + (iStride * (y - ymin));

                        while (y < ymax)
                        {
                            x = xmin;
                            pucPtr = pucStart;
                            while (x < xmax)
                            {
                                Grade = GrayscaleInt(pucPtr[2], pucPtr[1], pucPtr[0]);

                                if (!(Grade == 0 && isomitzero))
                                    *((uint*)pucPtr) = (IsInRangeEx(Grade, ThresholdValueMax, ThresholdValueMin) ? inrangecolor : outrangecolor);

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
                    bmp.UnlockBits(bmpData);
                    //JetEazy.LoggerClass.Instance.WriteException(ex);
                    if (IsDebug)
                        MessageBox.Show("Error :" + ex.ToString());
                }
            }
        }
        public void SetThresholdEX(Bitmap bmp, int ThresholdValue, int ThresholdRangeUpper, int ThresholdRangeLower, int basevalue, int minvalue, float ratio)
        {
            int Grade = 0;

            Rectangle rectbmp = SimpleRect(bmp.Size);
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

                    int ThresholdValueMax = Math.Min(255, ThresholdValue + ThresholdRangeUpper);
                    int ThresholdValueMin = Math.Max(0, ThresholdValue - ThresholdRangeLower);

                    y = ymin;
                    pucStart = scan0 + ((x - xmin) << 2) + (iStride * (y - ymin));

                    while (y < ymax)
                    {
                        x = xmin;
                        pucPtr = pucStart;
                        while (x < xmax)
                        {
                            Grade = GrayscaleInt(pucPtr[2], pucPtr[1], pucPtr[0]);

                            Grade = basevalue + (int)(((float)(Grade - minvalue)) * ratio);

                            *((uint*)pucPtr) = (IsInRangeEx(Grade, ThresholdValueMax, ThresholdValueMin) ? 0xFF000000 : 0xFFFFFFFF);

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
                bmp.UnlockBits(bmpData);
                //JetEazy.LoggerClass.Instance.WriteException(ex);
                if (IsDebug)
                    MessageBox.Show("Error :" + ex.ToString());
            }
        }
        public void SetThresholdEX(Bitmap bmp, Bitmap bmpmask, int includecolor, int rangeupper, int rangelower, bool isinrangecolorwhite, bool isomitzero)
        {
            int Grade = 0;

            Rectangle rectbmp = SimpleRect(bmp.Size);

            BitmapData bmpData = bmp.LockBits(rectbmp, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            BitmapData bmpmaksData = bmpmask.LockBits(rectbmp, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            IntPtr Scan0 = bmpData.Scan0;
            IntPtr maskScan0 = bmpmaksData.Scan0;

            try
            {
                unsafe
                {
                    byte* scan0 = (byte*)(void*)Scan0;
                    byte* pucPtr;
                    byte* pucStart;

                    byte* maskscan0 = (byte*)(void*)maskScan0;
                    byte* maskpucPtr;
                    byte* maskpucStart;

                    int xmin = rectbmp.X;
                    int ymin = rectbmp.Y;
                    int xmax = xmin + rectbmp.Width;
                    int ymax = ymin + rectbmp.Height;

                    int x = xmin;
                    int y = ymin;

                    int iStride = bmpData.Stride;
                    int imaskStride = bmpmaksData.Stride;

                    int RangeUpper = rangeupper;
                    int RangeLower = rangelower;

                    uint InRangeColor = 0xFF000000;
                    uint OutrangeColor = 0xFFFFFFFF;

                    if (isinrangecolorwhite)
                    {
                        InRangeColor = 0xFFFFFFFF;
                        OutrangeColor = 0xFF000000;
                    }

                    y = ymin;

                    pucStart = scan0 + ((x - xmin) << 2) + (iStride * (y - ymin));
                    maskpucStart = maskscan0 + ((x - xmin) << 2) + (imaskStride * (y - ymin));

                    while (y < ymax)
                    {
                        x = xmin;

                        pucPtr = pucStart;
                        maskpucPtr = maskpucStart;

                        while (x < xmax)
                        {
                            if (maskpucPtr[0] != includecolor)
                            {
                                pucPtr[0] = 0;
                                pucPtr[1] = 0;
                                pucPtr[2] = 0;
                            }
                            else
                            {
                                Grade = GrayscaleInt(pucPtr[2], pucPtr[1], pucPtr[0]);

                                *((uint*)pucPtr) = (IsInRangeEx(Grade, RangeUpper, RangeLower) ? InRangeColor : OutrangeColor);
                            }

                            pucPtr += 4;
                            maskpucPtr += 4;

                            x++;
                        }

                        pucStart += iStride;
                        maskpucStart += imaskStride;

                        y++;
                    }

                    bmp.UnlockBits(bmpData);
                    bmpmask.UnlockBits(bmpmaksData);
                }
            }
            catch (Exception ex)
            {
                bmp.UnlockBits(bmpData);
                bmpmask.UnlockBits(bmpmaksData);
                //JetEazy.LoggerClass.Instance.WriteException(ex);
                if (IsDebug)
                    MessageBox.Show("Error :" + ex.ToString());
            }
        }
        public void SetThresholdEX(Bitmap bmp, Bitmap bmpmask, int rangeupper, int rangelower, bool isinrangecolorwhite, bool isomitzero)
        {
            int Grade = 0;
            int MaskGrade = 0;

            Rectangle rectbmp = SimpleRect(bmp.Size);

            BitmapData bmpData = bmp.LockBits(rectbmp, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            BitmapData bmpmaksData = bmpmask.LockBits(rectbmp, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            IntPtr Scan0 = bmpData.Scan0;
            IntPtr maskScan0 = bmpmaksData.Scan0;

            try
            {
                unsafe
                {
                    byte* scan0 = (byte*)(void*)Scan0;
                    byte* pucPtr;
                    byte* pucStart;

                    byte* maskscan0 = (byte*)(void*)maskScan0;
                    byte* maskpucPtr;
                    byte* maskpucStart;

                    int xmin = rectbmp.X;
                    int ymin = rectbmp.Y;
                    int xmax = xmin + rectbmp.Width;
                    int ymax = ymin + rectbmp.Height;

                    int x = xmin;
                    int y = ymin;

                    int iStride = bmpData.Stride;
                    int imaskStride = bmpmaksData.Stride;

                    int RangeUpper = rangeupper;
                    int RangeLower = rangelower;

                    uint InRangeColor = 0xFF000000;
                    uint OutrangeColor = 0xFFFFFFFF;

                    if (isinrangecolorwhite)
                    {
                        InRangeColor = 0xFFFFFFFF;
                        OutrangeColor = 0xFF000000;
                    }

                    y = ymin;

                    pucStart = scan0 + ((x - xmin) << 2) + (iStride * (y - ymin));
                    maskpucStart = maskscan0 + ((x - xmin) << 2) + (imaskStride * (y - ymin));

                    while (y < ymax)
                    {
                        x = xmin;

                        pucPtr = pucStart;
                        maskpucPtr = maskpucStart;

                        while (x < xmax)
                        {
                            Grade = GrayscaleInt(pucPtr[2], pucPtr[1], pucPtr[0]);
                            MaskGrade = GrayscaleInt(maskpucPtr[2], maskpucPtr[1], maskpucPtr[0]);

                            if (MaskGrade == 0)
                            {
                                *((uint*)pucPtr) = OutrangeColor;
                            }
                            else
                            {
                                if (!(Grade == 0 && isomitzero))
                                    *((uint*)pucPtr) = (IsInRangeEx(Grade, RangeUpper, RangeLower) ? InRangeColor : OutrangeColor);
                            }

                            pucPtr += 4;
                            maskpucPtr += 4;

                            x++;
                        }

                        pucStart += iStride;
                        maskpucStart += imaskStride;

                        y++;
                    }

                    bmp.UnlockBits(bmpData);
                    bmpmask.UnlockBits(bmpmaksData);
                }
            }
            catch (Exception ex)
            {
                bmp.UnlockBits(bmpData);
                bmpmask.UnlockBits(bmpmaksData);
                //JetEazy.LoggerClass.Instance.WriteException(ex);
                if (IsDebug)
                    MessageBox.Show("Error :" + ex.ToString());
            }
        }

        public void SetThreshold(Bitmap bmp, Rectangle Rect, int ThresholdValue, int ThresholdRangeUpper, int ThresholdRangeLower, bool IsOmitZero, bool IsInRangeColorWhite)
        {
            lock (obj)
            {
                int Grade = 0;

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

                        int ThresholdValueMax = Math.Min(255, ThresholdValue + ThresholdRangeUpper);
                        int ThresholdValueMin = Math.Max(0, ThresholdValue - ThresholdRangeLower);

                        uint InRangeColor = 0xFF000000;
                        uint OutrangeColor = 0xFFFFFFFF;

                        if (IsInRangeColorWhite)
                        {
                            InRangeColor = 0xFFFFFFFF;
                            OutrangeColor = 0xFF000000;
                        }

                        y = ymin;
                        pucStart = scan0 + ((x - xmin) << 2) + (iStride * (y - ymin));

                        while (y < ymax)
                        {
                            x = xmin;
                            pucPtr = pucStart;
                            while (x < xmax)
                            {
                                Grade = GrayscaleInt(pucPtr[2], pucPtr[1], pucPtr[0]);

                                if (!(IsOmitZero && Grade == 0))
                                    *((uint*)pucPtr) = (IsInRangeEx(Grade, ThresholdValueMax, ThresholdValueMin) ? InRangeColor : OutrangeColor);

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
                    bmp.UnlockBits(bmpData);
                    //JetEazy.LoggerClass.Instance.WriteException(ex);
                    if (IsDebug)
                        MessageBox.Show("Error :" + ex.ToString());
                }
            }
        }
        public void GetLogColor(Bitmap bmp, int ub, int lb, ref int ubcolor, ref int lbcolor)
        {
            lock (obj)
            {

                int Grade = 0;

                Rectangle rectbmp = SimpleRect(bmp.Size);
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

                        float ubtotal = 0;
                        float ubcount = 0;

                        float lbtotal = 0;
                        float lbcount = 0;

                        while (y < ymax)
                        {
                            x = xmin;
                            pucPtr = pucStart;
                            while (x < xmax)
                            {
                                Grade = GrayscaleInt(pucPtr[2], pucPtr[1], pucPtr[0]);

                                if (Grade > ub)
                                {
                                    ubtotal += Grade;
                                    ubcount++;
                                }
                                else if (Grade < lb)
                                {
                                    lbtotal += Grade;
                                    lbcount++;
                                }

                                pucPtr += 4;
                                x++;
                            }

                            pucStart += iStride;
                            y++;
                        }


                        ubcolor = (int)(ubtotal / ubcount);
                        lbcolor = (int)(lbtotal / lbcount);

                        bmp.UnlockBits(bmpData);

                    }
                }
                catch (Exception ex)
                {
                    bmp.UnlockBits(bmpData);
                    //JetEazy.LoggerClass.Instance.WriteException(ex);
                    if (IsDebug)
                        MessageBox.Show("Error :" + ex.ToString());
                }
            }
        }

        public float GetBlueRatio(Bitmap bmp, int RatioValue)
        {
            Rectangle rectbmp = SimpleRect(bmp.Size);
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

                    float Ratio = 1f + (float)RatioValue / 100f;

                    y = ymin;
                    pucStart = scan0 + ((x - xmin) << 2) + (iStride * (y - ymin));


                    float totalcount = 0f;
                    float bluecount = 0f;

                    while (y < ymax)
                    {
                        x = xmin;
                        pucPtr = pucStart;
                        while (x < xmax)
                        {
                            if (pucPtr[2] + pucPtr[1] + pucPtr[0] > 0)
                            {
                                float colorRatio = (float)pucPtr[0] / ((float)Math.Min(pucPtr[1], pucPtr[2]));

                                if (colorRatio > Ratio)
                                {
                                    bluecount++;

                                    pucPtr[2] = 0;
                                    pucPtr[1] = 0;
                                    pucPtr[0] = 255;
                                }

                                totalcount++;
                            }

                            pucPtr += 4;
                            x++;
                        }

                        pucStart += iStride;
                        y++;
                    }

                    bmp.UnlockBits(bmpData);

                    return bluecount / totalcount;
                }
            }
            catch (Exception ex)
            {
                bmp.UnlockBits(bmpData);
                //JetEazy.LoggerClass.Instance.WriteException(ex);
                if (IsDebug)
                    MessageBox.Show("Error :" + ex.ToString());

                return 0;
            }
        }

        public int GetLine(Bitmap bmp, bool IsReverse, int FromY, double Ratio)
        {
            lock (obj)
            {
                Rectangle rectbmp = SimpleRect(bmp.Size);
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

                        y = FromY;
                        pucStart = scan0 + ((x - xmin) << 2) + (iStride * (y - ymin));

                        double LinePixelCount = 0;

                        if (!IsReverse)
                        {
                            while (y < ymax)
                            {
                                LinePixelCount = 0;
                                x = xmin;
                                pucPtr = pucStart;
                                while (x < xmax)
                                {
                                    if (pucPtr[0] == 255)
                                        LinePixelCount++;

                                    pucPtr += 4;
                                    x++;
                                }

                                if ((LinePixelCount / (double)(xmax - xmin)) > Ratio)
                                    break;

                                pucStart += iStride;

                                y++;
                            }
                        }
                        else
                        {
                            while (y > -1)
                            {
                                LinePixelCount = 0;
                                x = xmin;
                                pucPtr = pucStart;
                                while (x < xmax)
                                {
                                    if (pucPtr[0] == 255)
                                        LinePixelCount++;

                                    pucPtr += 4;
                                    x++;
                                }

                                if ((LinePixelCount / (double)(xmax - xmin)) > Ratio)
                                    break;

                                pucStart -= iStride;

                                y--;
                            }
                        }

                        bmp.UnlockBits(bmpData);

                        return y;
                    }
                }
                catch (Exception ex)
                {
                    bmp.UnlockBits(bmpData);
                    //JetEazy.LoggerClass.Instance.WriteException(ex);
                    if (IsDebug)
                        MessageBox.Show("Error :" + ex.ToString());

                    return -1;
                }
            }
        }
        public int GetLineY(Bitmap bmp, bool IsReverse, int FromY, int XLocation)
        {
            return GetLineY(bmp, IsReverse, FromY, XLocation, false);
        }
        public int GetLineY(Bitmap bmp, bool IsReverse, int FromY, int XLocation, bool IsNoDrawing)
        {
            lock (obj)
            {
                Rectangle rectbmp = SimpleRect(bmp.Size);
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

                        int x = XLocation;
                        int y = ymin;
                        int iStride = bmpData.Stride;

                        y = FromY;
                        pucStart = scan0 + ((x - xmin) << 2) + (iStride * (y - ymin));

                        if (!IsReverse)
                        {
                            while (y < ymax)
                            {
                                pucPtr = pucStart;

                                if (pucPtr[2] != 0)
                                {
                                    pucPtr[0] = 255;
                                    pucPtr[1] = 255;
                                    break;
                                }
                                else
                                {
                                    if (!IsNoDrawing)
                                    {
                                        pucPtr[0] = 255;
                                        pucPtr[1] = 255;
                                    }
                                }

                                pucStart += iStride;

                                y++;
                            }
                        }
                        else
                        {
                            while (y > -1)
                            {
                                pucPtr = pucStart;

                                if (pucPtr[2] != 0)
                                {
                                    pucPtr[0] = 255;
                                    pucPtr[1] = 255;
                                    break;
                                }
                                else
                                {
                                    if (!IsNoDrawing)
                                    {
                                        pucPtr[0] = 255;
                                        pucPtr[1] = 255;
                                    }
                                }

                                pucStart -= iStride;

                                y--;
                            }
                        }

                        bmp.UnlockBits(bmpData);

                        return y;
                    }
                }
                catch (Exception ex)
                {
                    bmp.UnlockBits(bmpData);
                    //JetEazy.LoggerClass.Instance.WriteException(ex);
                    if (IsDebug)
                        MessageBox.Show("Error :" + ex.ToString());

                    return -1;
                }
            }
        }
        public int GetLineX(Bitmap bmp, bool IsReverse, int FromX, int YLocation)
        {
            return GetLineX(bmp, IsReverse, FromX, YLocation, false);
        }
        public int GetLineX(Bitmap bmp, bool IsReverse, int FromX, int YLocation, bool IsNoDrawing)
        {
            lock (obj)
            {
                Rectangle rectbmp = SimpleRect(bmp.Size);
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
                        int y = YLocation;
                        int iStride = bmpData.Stride;

                        x = FromX;
                        pucStart = scan0 + ((x - xmin) << 2) + (iStride * (y - ymin));

                        if (!IsReverse)
                        {
                            while (x < xmax)
                            {
                                pucPtr = pucStart;

                                if (pucPtr[2] != 0)
                                {
                                    pucPtr[0] = 255;
                                    pucPtr[1] = 255;
                                    break;
                                }
                                else
                                {
                                    if (!IsNoDrawing)
                                    {
                                        pucPtr[0] = 255;
                                        pucPtr[1] = 255;
                                    }
                                }

                                pucStart += 4;

                                x++;
                            }
                        }
                        else
                        {
                            while (x > -1)
                            {
                                pucPtr = pucStart;

                                if (pucPtr[2] != 0)
                                {
                                    pucPtr[0] = 255;
                                    pucPtr[1] = 255;
                                    break;
                                }
                                else
                                {
                                    if (!IsNoDrawing)
                                    {
                                        pucPtr[0] = 255;
                                        pucPtr[1] = 255;
                                    }
                                }

                                pucStart -= 4;

                                x--;
                            }
                        }

                        bmp.UnlockBits(bmpData);

                        return x;
                    }
                }
                catch (Exception ex)
                {
                    bmp.UnlockBits(bmpData);
                    //JetEazy.LoggerClass.Instance.WriteException(ex);
                    if (IsDebug)
                        MessageBox.Show("Error :" + ex.ToString());

                    return -1;
                }
            }
        }
        public void SetUnderThread(Bitmap bmp, int threshold, ref Color retcolor)
        {
            int Grade = 0;

            Rectangle rectbmp = SimpleRect(bmp.Size);
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

                    //Get Four Corner Color
                    int R = 0;
                    int G = 0;
                    int B = 0;

                    long RSUM = 0;
                    long GSUM = 0;
                    long BSUM = 0;

                    int summ = 0;

                    int BlackCount = 0;
                    int InsideThresh = 0;
                    int BlackThresh = 0;

                    pucStart = scan0 + ((x - xmin) << 2) + (iStride * (y - ymin));

                    R += pucStart[2];
                    G += pucStart[1];
                    B += pucStart[0];

                    pucStart = scan0 + ((x - xmin + xmax - 1) << 2) + (iStride * (y - ymin));

                    R += pucStart[2];
                    G += pucStart[1];
                    B += pucStart[0];

                    pucStart = scan0 + ((x - xmin) << 2) + (iStride * (y - ymin + ymax - 1));

                    R += pucStart[2];
                    G += pucStart[1];
                    B += pucStart[0];

                    pucStart = scan0 + ((x - xmin + xmax - 1) << 2) + (iStride * (y - ymin + ymax - 1));

                    R += pucStart[2];
                    G += pucStart[1];
                    B += pucStart[0];

                    R /= 4;
                    G /= 4;
                    B /= 4;


                    InsideThresh = (int)((float)GrayscaleInt((byte)R, (byte)G, (byte)B) * ((float)threshold / 100f));
                    BlackThresh = (int)((float)GrayscaleInt((byte)R, (byte)G, (byte)B) * ((float)80 / 100f));

                    pucStart = scan0 + ((x - xmin) << 2) + (iStride * (y - ymin));

                    BlackCount = 0;

                    while (y < ymax)
                    {
                        x = xmin;
                        pucPtr = pucStart;
                        while (x < xmax)
                        {
                            Grade = GrayscaleInt(pucPtr[2], pucPtr[1], pucPtr[0]);

                            if (Grade < InsideThresh)
                            {
                                BlackCount++;
                            }

                            if (Grade < InsideThresh)
                            {
                                pucPtr[2] = (byte)R;
                                pucPtr[1] = (byte)G;
                                pucPtr[0] = (byte)B;
                            }

                            RSUM += pucPtr[2];
                            GSUM += pucPtr[1];
                            BSUM += pucPtr[0];

                            pucPtr += 4;
                            x++;
                        }

                        pucStart += iStride;
                        y++;
                    }

                    retcolor = Color.FromArgb((int)(RSUM / (xmax * ymax)), (int)(GSUM / (xmax * ymax)), (int)(BSUM / (xmax * ymax)));

                    summ = GrayscaleInt(retcolor.R, retcolor.G, retcolor.B);

                    summ = (int)((float)summ * 0.85);

                    RSUM = 0;
                    GSUM = 0;
                    BSUM = 0;

                    y = ymin;
                    x = xmin;

                    //if (BlackCount < (int)((float)(xmax * ymax) * 0.75))
                    {
                        pucStart = scan0 + ((x - xmin) << 2) + (iStride * (y - ymin));

                        while (y < ymax)
                        {
                            x = xmin;
                            pucPtr = pucStart;
                            while (x < xmax)
                            {
                                Grade = GrayscaleInt(pucPtr[2], pucPtr[1], pucPtr[0]);

                                if (Grade < summ)
                                {
                                    pucPtr[2] = (byte)R;
                                    pucPtr[1] = (byte)G;
                                    pucPtr[0] = (byte)B;
                                }

                                RSUM += pucPtr[2];
                                GSUM += pucPtr[1];
                                BSUM += pucPtr[0];

                                pucPtr += 4;
                                x++;
                            }

                            pucStart += iStride;
                            y++;
                        }

                        retcolor = Color.FromArgb((int)(RSUM / (xmax * ymax)), (int)(GSUM / (xmax * ymax)), (int)(BSUM / (xmax * ymax)));
                    }
                    //else
                    //{
                    //    pucStart = scan0 + ((x - xmin) << 2) + (iStride * (y - ymin));

                    //    while (y < ymax)
                    //    {
                    //        x = xmin;
                    //        pucPtr = pucStart;
                    //        while (x < xmax)
                    //        {

                    //            pucPtr[2] = 10;
                    //            pucPtr[1] = 10;
                    //            pucPtr[0] = 10;

                    //            RSUM += pucPtr[2];
                    //            GSUM += pucPtr[1];
                    //            BSUM += pucPtr[0];

                    //            pucPtr += 4;
                    //            x++;
                    //        }

                    //        pucStart += iStride;
                    //        y++;
                    //    }

                    //    retcolor = Color.FromArgb(10, 10, 10);
                    //}

                    bmp.UnlockBits(bmpData);
                }
            }
            catch (Exception ex)
            {
                bmp.UnlockBits(bmpData);
                //JetEazy.LoggerClass.Instance.WriteException(ex);
                if (IsDebug)
                    MessageBox.Show("Error :" + ex.ToString());
            }

        }
        public void SetUnderThread(Bitmap bmp, Bitmap bmpColor, ref Color retcolor, int excludeR)
        {
            Rectangle rectbmp = SimpleRect(bmp.Size);

            BitmapData bmpData = bmp.LockBits(rectbmp, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            BitmapData bmpData1 = bmpColor.LockBits(rectbmp, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            IntPtr Scan0 = bmpData.Scan0;
            IntPtr Scan1 = bmpData1.Scan0;

            try
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

                    long RSUM = 0;
                    long GSUM = 0;
                    long BSUM = 0;

                    int Count = 0;

                    int summ = 0;

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

                            if (pucPtr[2] != excludeR)
                            {
                                RSUM += pucPtr1[2];
                                GSUM += pucPtr1[1];
                                BSUM += pucPtr1[0];

                                Count++;
                            }
                            else
                            {
                                //pucPtr[0] = 255;
                                //pucPtr[1] = 255;
                                //pucPtr[2] = 0;

                            }

                            pucPtr += 4;
                            pucPtr1 += 4;

                            x++;
                        }

                        pucStart += iStride;
                        pucStart1 += iStride1;

                        y++;
                    }

                    retcolor = Color.FromArgb((int)(RSUM / Count), (int)(GSUM / Count), (int)(BSUM / Count));

                    //summ = myJzTools.GrayscaleInt(retcolor.R, retcolor.G, retcolor.B);

                    bmp.UnlockBits(bmpData);
                    bmpColor.UnlockBits(bmpData1);
                }
            }
            catch (Exception ex)
            {
                bmp.UnlockBits(bmpData);
                bmpColor.UnlockBits(bmpData1);
                //JetEazy.LoggerClass.Instance.WriteException(ex);
                if (IsDebug)
                    MessageBox.Show("Error :" + ex.ToString());
            }

        }
        public void GetLineAndFill(Bitmap bmp, double Ratio, bool IsWhite)
        {
            Rectangle rectbmp = SimpleRect(bmp.Size);
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

                    uint InRangeColor = 0xFF000000;
                    uint OutrangeColor = 0xFFFFFFFF;

                    y = ymin;
                    pucStart = scan0 + ((x - xmin) << 2) + (iStride * (y - ymin));

                    double LinePixelCount = 0;

                    while (y < ymax)
                    {
                        LinePixelCount = 0;
                        x = xmin;
                        pucPtr = pucStart;
                        while (x < xmax)
                        {
                            if (pucPtr[0] == 255)
                                LinePixelCount++;

                            pucPtr += 4;
                            x++;
                        }

                        if ((LinePixelCount / (double)(xmax - xmin)) > Ratio)
                        {
                            x = xmin;
                            pucPtr = pucStart;
                            while (x < xmax)
                            {
                                *((uint*)pucPtr) = (!IsWhite ? InRangeColor : OutrangeColor);

                                pucPtr += 4;
                                x++;
                            }

                            pucStart += iStride;
                            y++;
                            continue;
                        }

                        pucStart += iStride;

                        y++;
                    }

                    bmp.UnlockBits(bmpData);
                }
            }
            catch (Exception ex)
            {
                bmp.UnlockBits(bmpData);
                //JetEazy.LoggerClass.Instance.WriteException(ex);
                if (IsDebug)
                    MessageBox.Show("Error :" + ex.ToString());
            }

        }
        public List<int> CenterList = new List<int>();
        public Rectangle GetMergedRect(bool IsXDir, bool IsBefore, int Val)
        {
            Rectangle rect = new Rectangle(0, 0, -1, -1);

            if (IsXDir) //X 方向的
            {
                if (IsBefore)    //比VAL 小的值
                {
                    foreach (FoundClass found in FoundList)
                    {
                        if (found.Center.X < Val)
                        {
                            if (rect.Width == -1)
                            {
                                rect = found.rect;
                            }
                            else
                            {
                                rect = MergeTwoRects(rect, found.rect);
                            }
                        }
                    }
                }
                else
                {
                    foreach (FoundClass found in FoundList)
                    {
                        if (found.Center.X > Val)
                        {
                            if (rect.Width == -1)
                            {
                                rect = found.rect;
                            }
                            else
                            {
                                rect = MergeTwoRects(rect, found.rect);
                            }
                        }
                    }
                }
            }
            else
            {
                if (IsBefore)    //比VAL 小的值
                {
                    foreach (FoundClass found in FoundList)
                    {
                        if (found.Center.Y < Val)
                        {
                            if (rect.Width == -1)
                            {
                                rect = found.rect;
                            }
                            else
                            {
                                rect = MergeTwoRects(rect, found.rect);
                            }
                        }
                    }
                }
                else
                {
                    foreach (FoundClass found in FoundList)
                    {
                        if (found.Center.Y > Val)
                        {
                            if (rect.Width == -1)
                            {
                                rect = found.rect;
                            }
                            else
                            {
                                rect = MergeTwoRects(rect, found.rect);
                            }
                        }
                    }
                }
            }

            return rect;
        }

        public float ULocation = 0;
        public float DLocation = 0;
        public double GetBoraderLine(Bitmap bmp, bool IsXdir)
        {
            double bypass = 0;
            return GetBoraderLine(bmp, IsXdir, ref bypass);
        }
        public double GetBoraderLine(Bitmap bmp, bool IsXdir, ref double centeraverage)
        {
            Rectangle rectbmp = SimpleRect(bmp.Size);
            BitmapData bmpData = bmp.LockBits(rectbmp, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            IntPtr Scan0 = bmpData.Scan0;

            try
            {
                unsafe
                {
                    byte* scan0 = (byte*)(void*)Scan0;
                    byte* pucPtr;
                    byte* pucStart;
                    byte* pucMin;

                    int xmin = rectbmp.X;
                    int ymin = rectbmp.Y;
                    int xmax = xmin + rectbmp.Width;
                    int ymax = ymin + rectbmp.Height;

                    int x = xmin;
                    int y = ymin;
                    int iStride = bmpData.Stride;

                    int LDistance = 0;
                    int RDistance = 0;

                    int Min = 0;
                    int MinLocation = 0;

                    double DistanceAvg = 0;
                    centeraverage = 0;
                    ULocation = 0;
                    DLocation = 0;

                    int xcenter = rectbmp.X + rectbmp.Width >> 1;
                    int ycenter = rectbmp.Y + rectbmp.Height >> 1;

                    List<int> DistanceList = new List<int>();

                    CenterList.Clear();

                    y = 0;

                    if (IsXdir)
                    {
                        #region Check From Width

                        while (y < rectbmp.Height)
                        {
                            x = xcenter;

                            pucStart = scan0 + ((x - xmin) << 2) + (iStride * (y - ymin));
                            pucPtr = pucStart;
                            pucMin = pucPtr;

                            Min = 1000;

                            while (x > -1)
                            {
                                if (pucPtr[0] == 0)
                                {
                                    pucPtr[0] = 255;
                                    pucPtr[1] = 0;
                                    pucPtr[2] = 0;
                                }
                                else
                                {
                                    if (pucPtr[0] < Min)
                                    {
                                        Min = pucPtr[0];
                                        MinLocation = x;

                                        pucMin = pucPtr;

                                        LDistance = x;
                                    }
                                }
                                pucPtr -= 4;
                                x--;
                            }

                            pucMin[0] = 0;
                            pucMin[1] = 0;
                            pucMin[2] = 255;

                            x = xcenter + 1;

                            pucStart = scan0 + ((x - xmin) << 2) + (iStride * (y - ymin));
                            pucPtr = pucStart;
                            pucMin = pucPtr;

                            Min = 1000;

                            while (x < rectbmp.Width)
                            {
                                if (pucPtr[0] == 0)
                                {
                                    pucPtr[0] = 0;
                                    pucPtr[1] = 255;
                                    pucPtr[2] = 0;
                                }
                                else
                                {
                                    if (pucPtr[0] < Min)
                                    {
                                        Min = pucPtr[0];
                                        MinLocation = x;

                                        pucMin = pucPtr;

                                        RDistance = x;
                                    }
                                }

                                pucPtr += 4;
                                x++;
                            }

                            pucMin[0] = 0;
                            pucMin[1] = 0;
                            pucMin[2] = 255;

                            DistanceList.Add(RDistance - LDistance);
                            DistanceAvg += (RDistance - LDistance);

                            ULocation += LDistance;
                            DLocation += RDistance;


                            CenterList.Add((RDistance + LDistance) >> 1);
                            centeraverage += ((RDistance + LDistance) >> 1);


                            pucStart += iStride;
                            y++;
                        }
                        #endregion
                    }
                    else
                    {
                        while (x < rectbmp.Width)
                        {
                            y = ycenter;

                            pucStart = scan0 + ((x - xmin) << 2) + (iStride * (y - ymin));
                            pucPtr = pucStart;
                            pucMin = pucPtr;

                            Min = 1000;

                            while (y > -1)
                            {
                                if (pucPtr[0] == 0)
                                {
                                    pucPtr[0] = 255;
                                    pucPtr[1] = 0;
                                    pucPtr[2] = 0;
                                }
                                else
                                {
                                    if (pucPtr[0] < Min)
                                    {
                                        Min = pucPtr[0];
                                        MinLocation = y;

                                        pucMin = pucPtr;

                                        LDistance = y;
                                    }
                                }
                                pucPtr -= iStride;
                                y--;
                            }

                            pucMin[0] = 0;
                            pucMin[1] = 0;
                            pucMin[2] = 255;

                            y = ycenter + 1;

                            pucStart = scan0 + ((x - xmin) << 2) + (iStride * (y - ymin));
                            pucPtr = pucStart;
                            pucMin = pucPtr;

                            Min = 1000;

                            while (y < rectbmp.Height)
                            {
                                if (pucPtr[0] == 0)
                                {

                                    pucPtr[0] = 0;
                                    pucPtr[1] = 255;
                                    pucPtr[2] = 0;
                                }
                                else
                                {
                                    if (pucPtr[0] < Min)
                                    {
                                        Min = pucPtr[0];
                                        MinLocation = y;

                                        pucMin = pucPtr;

                                        RDistance = y;
                                    }
                                }

                                pucPtr += iStride;
                                y++;
                            }

                            pucMin[0] = 0;
                            pucMin[1] = 0;
                            pucMin[2] = 255;

                            DistanceList.Add(RDistance - LDistance);
                            DistanceAvg += (RDistance - LDistance);

                            ULocation += LDistance;
                            DLocation += RDistance;

                            CenterList.Add((RDistance + LDistance) >> 1);
                            centeraverage += ((RDistance + LDistance) >> 1);

                            pucStart += 4;
                            x++;
                        }
                    }

                    DistanceAvg = DistanceAvg / (double)(DistanceList.Count);
                    centeraverage = centeraverage / (double)(DistanceList.Count);

                    ULocation = ULocation / (float)(DistanceList.Count);
                    DLocation = DLocation / (float)(DistanceList.Count);

                    bmp.UnlockBits(bmpData);

                    return DistanceAvg;
                }
            }
            catch (Exception ex)
            {
                bmp.UnlockBits(bmpData);
                //JetEazy.LoggerClass.Instance.WriteException(ex);
                if (IsDebug)
                    MessageBox.Show("Error :" + ex.ToString());

                return 0;
            }


        }
        public double GetBoraderLineEX(Bitmap bmp, bool IsXdir, double borderstep)
        {
            Rectangle rectbmp = SimpleRect(bmp.Size);
            BitmapData bmpData = bmp.LockBits(rectbmp, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            IntPtr Scan0 = bmpData.Scan0;

            try
            {
                unsafe
                {
                    byte* scan0 = (byte*)(void*)Scan0;
                    byte* pucPtr;
                    byte* pucStart;
                    byte* pucMin;

                    int xmin = rectbmp.X;
                    int ymin = rectbmp.Y;
                    int xmax = xmin + rectbmp.Width;
                    int ymax = ymin + rectbmp.Height;

                    int x = xmin;
                    int y = ymin;
                    int iStride = bmpData.Stride;

                    int LDistance = 0;
                    int RDistance = 0;

                    int Min = 0;
                    int MinLocation = 0;

                    double DistanceAvg = 0;
                    int FirstSeed = 0;

                    ULocation = 0;
                    DLocation = 0;

                    int xcenter = rectbmp.X + rectbmp.Width >> 1;
                    int ycenter = rectbmp.Y + rectbmp.Height >> 1;

                    List<int> DistanceList = new List<int>();

                    CenterList.Clear();

                    y = 0;

                    if (IsXdir)
                    {
                        #region Check From Width

                        while (y < rectbmp.Height)
                        {
                            x = xcenter;

                            pucStart = scan0 + ((x - xmin) << 2) + (iStride * (y - ymin));
                            pucPtr = pucStart;
                            pucMin = pucPtr;

                            Min = 1000;

                            FirstSeed = -1;
                            LDistance = xmin;

                            while (x > -1)
                            {
                                if (pucPtr[0] == 0)
                                {
                                    pucPtr[0] = 255;
                                    pucPtr[1] = 0;
                                    pucPtr[2] = 0;
                                }
                                else
                                {
                                    if (FirstSeed == -1)
                                        FirstSeed = pucPtr[0];

                                    if (((double)pucPtr[0] / (double)FirstSeed) < borderstep)
                                    {
                                        pucMin = pucPtr;
                                        LDistance = x;

                                        break;
                                    }
                                }
                                pucPtr -= 4;
                                x--;
                            }

                            pucMin[0] = 0;
                            pucMin[1] = 0;
                            pucMin[2] = 255;

                            x = xcenter + 1;

                            pucStart = scan0 + ((x - xmin) << 2) + (iStride * (y - ymin));
                            pucPtr = pucStart;
                            pucMin = pucPtr;

                            Min = 1000;

                            FirstSeed = -1;
                            RDistance = xmax;

                            while (x < rectbmp.Width)
                            {
                                if (pucPtr[0] == 0)
                                {
                                    pucPtr[0] = 0;
                                    pucPtr[1] = 255;
                                    pucPtr[2] = 0;
                                }
                                else
                                {
                                    if (FirstSeed == -1)
                                        FirstSeed = pucPtr[0];

                                    if (((double)pucPtr[0] / (double)FirstSeed) < borderstep)
                                    {
                                        pucMin = pucPtr;
                                        RDistance = x;

                                        break;
                                    }

                                }

                                pucPtr += 4;
                                x++;
                            }

                            pucMin[0] = 0;
                            pucMin[1] = 0;
                            pucMin[2] = 255;

                            DistanceList.Add(RDistance - LDistance);
                            DistanceAvg += (RDistance - LDistance);

                            ULocation += LDistance;
                            DLocation += RDistance;


                            CenterList.Add((RDistance + LDistance) >> 1);

                            pucStart += iStride;
                            y++;
                        }
                        #endregion
                    }
                    else
                    {
                        while (x < rectbmp.Width)
                        {
                            y = ycenter;

                            pucStart = scan0 + ((x - xmin) << 2) + (iStride * (y - ymin));
                            pucPtr = pucStart;
                            pucMin = pucPtr;

                            Min = 1000;

                            FirstSeed = -1;
                            LDistance = ymin;

                            while (y > -1)
                            {
                                if (pucPtr[0] == 0)
                                {
                                    pucPtr[0] = 255;
                                    pucPtr[1] = 0;
                                    pucPtr[2] = 0;
                                }
                                else
                                {
                                    if (FirstSeed == -1)
                                        FirstSeed = pucPtr[0];

                                    if (((double)pucPtr[0] / (double)FirstSeed) < borderstep)
                                    {
                                        pucMin = pucPtr;
                                        LDistance = y;

                                        break;
                                    }
                                }
                                pucPtr -= iStride;
                                y--;
                            }

                            pucMin[0] = 0;
                            pucMin[1] = 0;
                            pucMin[2] = 255;

                            y = ycenter + 1;

                            pucStart = scan0 + ((x - xmin) << 2) + (iStride * (y - ymin));
                            pucPtr = pucStart;
                            pucMin = pucPtr;

                            Min = 1000;
                            FirstSeed = -1;
                            RDistance = ymax;

                            while (y < rectbmp.Height)
                            {
                                if (pucPtr[0] == 0)
                                {

                                    pucPtr[0] = 0;
                                    pucPtr[1] = 255;
                                    pucPtr[2] = 0;
                                }
                                else
                                {
                                    if (FirstSeed == -1)
                                        FirstSeed = pucPtr[0];

                                    if (((double)pucPtr[0] / (double)FirstSeed) < borderstep)
                                    {
                                        pucMin = pucPtr;
                                        RDistance = y;

                                        break;
                                    }

                                }

                                pucPtr += iStride;
                                y++;
                            }

                            pucMin[0] = 0;
                            pucMin[1] = 0;
                            pucMin[2] = 255;

                            DistanceList.Add(RDistance - LDistance);
                            DistanceAvg += (RDistance - LDistance);

                            ULocation += LDistance;
                            DLocation += RDistance;

                            CenterList.Add((RDistance + LDistance) >> 1);
                            pucStart += 4;
                            x++;
                        }
                    }

                    DistanceAvg = DistanceAvg / (double)(DistanceList.Count);

                    ULocation = ULocation / (float)(DistanceList.Count);
                    DLocation = DLocation / (float)(DistanceList.Count);

                    bmp.UnlockBits(bmpData);

                    return DistanceAvg;
                }
            }
            catch (Exception ex)
            {
                bmp.UnlockBits(bmpData);
                //JetEazy.LoggerClass.Instance.WriteException(ex);
                if (IsDebug)
                    MessageBox.Show("Error :" + ex.ToString());

                return 0;
            }


        }
        public double GetBoraderLineEX(Bitmap bmp, bool IsXdir, ref double centeraverage, bool IsBlack, double checkingratio)
        {
            lock (obj)
            {
                Rectangle rectbmp = SimpleRect(bmp.Size);
                BitmapData bmpData = bmp.LockBits(rectbmp, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                IntPtr Scan0 = bmpData.Scan0;

                try
                {
                    unsafe
                    {
                        byte* scan0 = (byte*)(void*)Scan0;
                        byte* pucPtr;
                        byte* pucStart;
                        byte* pucMin;

                        int xmin = rectbmp.X;
                        int ymin = rectbmp.Y;
                        int xmax = xmin + rectbmp.Width;
                        int ymax = ymin + rectbmp.Height;

                        int x = xmin;
                        int y = ymin;
                        int iStride = bmpData.Stride;

                        int LDistance = 0;
                        int RDistance = 0;

                        int Min = 0;
                        int MinLocation = 0;
                        int FirstSeed = 0;

                        double DistanceAvg = 0;
                        centeraverage = 0;
                        ULocation = 0;
                        DLocation = 0;

                        int xcenter = rectbmp.X + rectbmp.Width >> 1;
                        int ycenter = rectbmp.Y + rectbmp.Height >> 1;

                        List<int> DistanceList = new List<int>();

                        CenterList.Clear();

                        y = 0;

                        if (IsXdir)
                        {
                            #region Check From Width

                            while (y < rectbmp.Height)
                            {
                                x = xcenter;

                                pucStart = scan0 + ((x - xmin) << 2) + (iStride * (y - ymin));
                                pucPtr = pucStart;
                                pucMin = pucPtr;

                                Min = 1000;

                                FirstSeed = -1;
                                LDistance = xmin;

                                while (x > -1)
                                {
                                    if (pucPtr[0] == 0)
                                    {
                                        pucPtr[0] = 255;
                                        pucPtr[1] = 0;
                                        pucPtr[2] = 0;
                                    }
                                    else
                                    {
                                        if (FirstSeed == -1)
                                            FirstSeed = pucPtr[0];

                                        if (IsBlack)
                                        {
                                            if (((double)pucPtr[0] / (double)FirstSeed) < 1 - checkingratio)
                                            {
                                                pucMin = pucPtr;
                                                LDistance = x;

                                                break;
                                            }
                                        }
                                        else
                                        {
                                            if (((double)pucPtr[0] / (double)FirstSeed) > 1 + checkingratio)
                                            {
                                                pucMin = pucPtr;
                                                LDistance = x;

                                                break;
                                            }
                                        }

                                    }
                                    pucPtr -= 4;
                                    x--;
                                }

                                pucMin[0] = 0;
                                pucMin[1] = 0;
                                pucMin[2] = 255;

                                x = xcenter + 1;

                                pucStart = scan0 + ((x - xmin) << 2) + (iStride * (y - ymin));
                                pucPtr = pucStart;
                                pucMin = pucPtr;

                                Min = 1000;

                                FirstSeed = -1;
                                RDistance = xmax;

                                while (x < rectbmp.Width)
                                {
                                    if (pucPtr[0] == 0)
                                    {
                                        pucPtr[0] = 0;
                                        pucPtr[1] = 255;
                                        pucPtr[2] = 0;
                                    }
                                    else
                                    {
                                        if (FirstSeed == -1)
                                            FirstSeed = pucPtr[0];

                                        if (IsBlack)
                                        {
                                            if (((double)pucPtr[0] / (double)FirstSeed) < 1 - checkingratio)
                                            {
                                                pucMin = pucPtr;
                                                RDistance = x;

                                                break;
                                            }
                                        }
                                        else
                                        {
                                            if (((double)pucPtr[0] / (double)FirstSeed) > 1 + checkingratio)
                                            {
                                                pucMin = pucPtr;
                                                RDistance = x;

                                                break;
                                            }
                                        }
                                    }

                                    pucPtr += 4;
                                    x++;
                                }

                                pucMin[0] = 0;
                                pucMin[1] = 0;
                                pucMin[2] = 255;

                                DistanceList.Add(RDistance - LDistance);
                                DistanceAvg += (RDistance - LDistance);

                                ULocation += LDistance;
                                DLocation += RDistance;


                                CenterList.Add((RDistance + LDistance) >> 1);
                                centeraverage += ((RDistance + LDistance) >> 1);


                                pucStart += iStride;
                                y++;
                            }
                            #endregion
                        }
                        else
                        {
                            while (x < rectbmp.Width)
                            {
                                y = ycenter;

                                pucStart = scan0 + ((x - xmin) << 2) + (iStride * (y - ymin));
                                pucPtr = pucStart;
                                pucMin = pucPtr;

                                Min = 1000;

                                FirstSeed = -1;
                                LDistance = ymin;

                                while (y > -1)
                                {
                                    if (pucPtr[0] == 0)
                                    {
                                        pucPtr[0] = 255;
                                        pucPtr[1] = 0;
                                        pucPtr[2] = 0;
                                    }
                                    else
                                    {
                                        if (FirstSeed == -1)
                                            FirstSeed = pucPtr[0];

                                        if (IsBlack)
                                        {
                                            if (((double)pucPtr[0] / (double)FirstSeed) < 1 - checkingratio)
                                            {
                                                pucMin = pucPtr;
                                                LDistance = y;

                                                break;
                                            }
                                        }
                                        else
                                        {
                                            if (((double)pucPtr[0] / (double)FirstSeed) > 1 + checkingratio)
                                            {
                                                pucMin = pucPtr;
                                                LDistance = y;

                                                break;
                                            }
                                        }
                                    }
                                    pucPtr -= iStride;
                                    y--;
                                }

                                pucMin[0] = 0;
                                pucMin[1] = 0;
                                pucMin[2] = 255;

                                y = ycenter + 1;

                                pucStart = scan0 + ((x - xmin) << 2) + (iStride * (y - ymin));
                                pucPtr = pucStart;
                                pucMin = pucPtr;

                                Min = 1000;

                                FirstSeed = -1;
                                RDistance = ymax;

                                while (y < rectbmp.Height)
                                {
                                    if (pucPtr[0] == 0)
                                    {

                                        pucPtr[0] = 0;
                                        pucPtr[1] = 255;
                                        pucPtr[2] = 0;
                                    }
                                    else
                                    {
                                        if (FirstSeed == -1)
                                            FirstSeed = pucPtr[0];

                                        if (IsBlack)
                                        {
                                            if (((double)pucPtr[0] / (double)FirstSeed) < 1 - checkingratio)
                                            {
                                                pucMin = pucPtr;
                                                RDistance = y;

                                                break;
                                            }
                                        }
                                        else
                                        {
                                            if (((double)pucPtr[0] / (double)FirstSeed) > 1 + checkingratio)
                                            {
                                                pucMin = pucPtr;
                                                RDistance = y;

                                                break;
                                            }
                                        }
                                    }

                                    pucPtr += iStride;
                                    y++;
                                }

                                pucMin[0] = 0;
                                pucMin[1] = 0;
                                pucMin[2] = 255;

                                DistanceList.Add(RDistance - LDistance);
                                DistanceAvg += (RDistance - LDistance);

                                ULocation += LDistance;
                                DLocation += RDistance;

                                CenterList.Add((RDistance + LDistance) >> 1);
                                centeraverage += ((RDistance + LDistance) >> 1);

                                pucStart += 4;
                                x++;
                            }
                        }

                        DistanceAvg = DistanceAvg / (double)(DistanceList.Count);
                        centeraverage = centeraverage / (double)(DistanceList.Count);

                        ULocation = ULocation / (float)(DistanceList.Count);
                        DLocation = DLocation / (float)(DistanceList.Count);

                        bmp.UnlockBits(bmpData);

                        return DistanceAvg;
                    }
                }
                catch (Exception ex)
                {
                    bmp.UnlockBits(bmpData);
                    //JetEazy.LoggerClass.Instance.WriteException(ex);
                    if (IsDebug)
                        MessageBox.Show("Error :" + ex.ToString());

                    return 0;
                }
            }
        }
        //public Rectangle DrawCircleMask(Bitmap bmp, Rectangle rect,int reduceratio,Color color)
        //{
        //    Point Pt = myJzTools.GetRectCenter(rect);

        //    int Redius = Math.Min(rect.Width, rect.Height);

        //    Redius = Redius >> 1;

        //    Redius = (int)((double)Redius * ((100d - (double)reduceratio) / 100d));

        //    Rectangle newrect = new Rectangle(Pt.X - Redius, Pt.Y - Redius, Redius << 1, Redius << 1);

        //    myJzTools.DrawCircle(ref bmp, newrect, new SolidBrush(color));

        //    return newrect;
        //}
        public Rectangle DrawCircleMask(Bitmap bmp, Rectangle rect, int reduceratio, Color color, bool IsArround)
        {
            Point Pt = GetRectCenter(rect);

            int Redius = Math.Min(rect.Width, rect.Height);

            Redius = Redius >> 1;

            int Redius2 = (int)((double)Redius * ((100d - (double)reduceratio) / 100d));

            int linewidth = Math.Abs(Redius2 - Redius);

            Rectangle newrect = new Rectangle(Pt.X - Redius2, Pt.Y - Redius2, Redius2 << 1, Redius2 << 1);

            if (IsArround)
            {
                newrect = new Rectangle(Pt.X - Redius, Pt.Y - Redius, Redius << 1, Redius << 1);
                DrawCircle(ref bmp, newrect, color, linewidth);

            }
            else
                DrawCircle(ref bmp, newrect, new SolidBrush(color));

            return newrect;
        }
        public void GetMaskedImage(Bitmap bmp, Bitmap bmpMask, Color GetColor)
        {
            GetMaskedImage(bmp, bmpMask, GetColor, Color.Black);
        }
        public void GetMaskedImage(Bitmap bmp, Bitmap bmpMask, Color GetColor, Color BgColor)
        {
            GetMaskedImage(bmp, bmpMask, GetColor, BgColor, false);
        }
        public void GetMaskedImage(Bitmap bmp, Bitmap bmpMask, Color GetColor, Color BgColor, bool IsReverse)
        {
            int Grade = 0;

            Rectangle rectbmp = SimpleRect(bmp.Size);

            BitmapData bmpData = bmp.LockBits(rectbmp, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            BitmapData bmpData1 = bmpMask.LockBits(rectbmp, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            IntPtr Scan0 = bmpData.Scan0;
            IntPtr Scan1 = bmpData1.Scan0;

            try
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

                    //int ThresholdValueMax = Math.Min(255, ThresholdValue + ThresholdRangeUpper);
                    //int ThresholdValueMin = Math.Max(0, ThresholdValue - ThresholdRangeLower);

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
                            //Grade = myJzTools.GrayscaleInt(pucPtr[2], pucPtr[1], pucPtr[0]);

                            //*((uint*)pucPtr) = (myJzTools.IsInRangeEx(Grade, ThresholdValueMax, ThresholdValueMin) ? 0xFF000000 : 0xFFFFFFFF);


                            if (pucPtr1[0] == GetColor.B && pucPtr1[1] == GetColor.G && pucPtr1[2] == GetColor.R)
                            {
                                if (!IsReverse)
                                {
                                    pucPtr[0] = pucPtr[0];
                                }
                                else
                                {
                                    pucPtr[0] = BgColor.B;
                                    pucPtr[1] = BgColor.G;
                                    pucPtr[2] = BgColor.R;
                                }
                            }
                            else
                            {
                                if (!IsReverse)
                                {
                                    pucPtr[0] = BgColor.B;
                                    pucPtr[1] = BgColor.G;
                                    pucPtr[2] = BgColor.R;
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
                    bmpMask.UnlockBits(bmpData1);
                }
            }
            catch (Exception ex)
            {
                bmp.UnlockBits(bmpData);
                bmpMask.UnlockBits(bmpData1);
                //JetEazy.LoggerClass.Instance.WriteException(ex);
                if (IsDebug)
                    MessageBox.Show("Error :" + ex.ToString());
            }
        }
        public void SetColor(Bitmap bmp, Color orgcolor, Color tocolor, Color bgcolor)
        {
            Rectangle rectbmp = SimpleRect(bmp.Size);
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
                            if (pucPtr[2] == (byte)orgcolor.R && pucPtr[1] == (byte)orgcolor.G && pucPtr[0] == (byte)orgcolor.B)
                            {
                                pucPtr[0] = tocolor.B;
                                pucPtr[1] = tocolor.G;
                                pucPtr[2] = tocolor.R;
                            }
                            else
                            {
                                pucPtr[0] = bgcolor.B;
                                pucPtr[1] = bgcolor.G;
                                pucPtr[2] = bgcolor.R;
                            }

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
                bmp.UnlockBits(bmpData);
                //JetEazy.LoggerClass.Instance.WriteException(ex);
                if (IsDebug)
                    MessageBox.Show("Error :" + ex.ToString());
            }
        }
        public void SetTrans(Bitmap bmp, int transvalue)
        {
            Rectangle rectbmp = SimpleRect(bmp.Size);
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
                            pucStart[3] = (byte)transvalue;

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
                bmp.UnlockBits(bmpData);
                //JetEazy.LoggerClass.Instance.WriteException(ex);
                if (IsDebug)
                    MessageBox.Show("Error :" + ex.ToString());
            }
        }
        public void SetDiff(Bitmap bmporg, Bitmap bmpcover)
        {
            int Grade = 0;

            Rectangle rectbmp = SimpleRect(bmporg.Size);

            rectbmp.Width = Math.Min(bmporg.Width, bmpcover.Width);
            rectbmp.Height = Math.Min(bmporg.Height, bmpcover.Height);

            BitmapData bmpData = bmporg.LockBits(rectbmp, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            BitmapData bmpData1 = bmpcover.LockBits(rectbmp, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            IntPtr Scan0 = bmpData.Scan0;
            IntPtr Scan1 = bmpData1.Scan0;

            try
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
                            int Grade0 = GrayscaleInt(pucPtr[2], pucPtr[1], pucPtr[0]);
                            int Grade1 = GrayscaleInt(pucPtr1[2], pucPtr1[1], pucPtr1[0]);

                            pucPtr[2] = (byte)(Math.Max(0, Math.Abs(Grade0 - Grade1)));
                            pucPtr[1] = (byte)(Math.Max(0, 255 - Math.Abs(Grade0 - Grade1)));
                            pucPtr[0] = 0;

                            pucPtr += 4;
                            pucPtr1 += 4;

                            x++;
                        }

                        pucStart += iStride;
                        pucStart1 += iStride1;

                        y++;
                    }

                    bmporg.UnlockBits(bmpData);
                    bmpcover.UnlockBits(bmpData1);
                }
            }
            catch (Exception ex)
            {
                bmporg.UnlockBits(bmpData);
                bmpcover.UnlockBits(bmpData1);
                //JetEazy.LoggerClass.Instance.WriteException(ex);
                if (IsDebug)
                    MessageBox.Show("Error :" + ex.ToString());
            }
        }
        public void SetDiff(Bitmap bmporg, Bitmap bmpcover, int trans)
        {
            int Grade = 0;

            Rectangle rectbmp = SimpleRect(bmporg.Size);

            rectbmp.Width = Math.Min(bmporg.Width, bmpcover.Width);
            rectbmp.Height = Math.Min(bmporg.Height, bmpcover.Height);

            BitmapData bmpData = bmporg.LockBits(rectbmp, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            BitmapData bmpData1 = bmpcover.LockBits(rectbmp, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            IntPtr Scan0 = bmpData.Scan0;
            IntPtr Scan1 = bmpData1.Scan0;

            try
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
                            int Grade0 = GrayscaleInt(pucPtr[2], pucPtr[1], pucPtr[0]);
                            int Grade1 = GrayscaleInt(pucPtr1[2], pucPtr1[1], pucPtr1[0]);

                            int Trans = (int)(((float)Grade0 * (float)trans / 255f) + ((float)Grade1 * ((float)(255 - trans) / 255f)));

                            Trans = Math.Min(255, Trans);

                            pucPtr[2] = (byte)Trans;
                            pucPtr[1] = (byte)Trans;
                            pucPtr[0] = (byte)Trans;

                            pucPtr += 4;
                            pucPtr1 += 4;

                            x++;
                        }

                        pucStart += iStride;
                        pucStart1 += iStride1;

                        y++;
                    }

                    bmporg.UnlockBits(bmpData);
                    bmpcover.UnlockBits(bmpData1);
                }
            }
            catch (Exception ex)
            {
                bmporg.UnlockBits(bmpData);
                bmpcover.UnlockBits(bmpData1);
                //JetEazy.LoggerClass.Instance.WriteException(ex);
                if (IsDebug)
                    MessageBox.Show("Error :" + ex.ToString());
            }
        }
        /// <summary>
        /// 從中間取得左右邊所有的數據
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="iswidth"></param>
        /// <param name="step"></param>
        /// <param name="ltptlist"></param>
        /// <param name="rbptlist"></param>
        /// <param name="lenlist"></param>
        public void GetInnerLineFromCenter(Bitmap bmp, bool iswidth, int step, ref List<PointF> ltptlist, ref List<PointF> rbptlist, ref List<int> lenlist)
        {
            Rectangle rectbmp = SimpleRect(bmp.Size);
            BitmapData bmpData = bmp.LockBits(rectbmp, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            IntPtr Scan0 = bmpData.Scan0;

            try
            {
                unsafe
                {
                    byte* scan0 = (byte*)(void*)Scan0;
                    byte* pucPtr;
                    byte* pucStart;
                    byte* pucMin;

                    int xmin = rectbmp.X;
                    int ymin = rectbmp.Y;
                    int xmax = xmin + rectbmp.Width - 1;
                    int ymax = ymin + rectbmp.Height - 1;

                    int x = xmin;
                    int y = ymin;

                    int iStride = bmpData.Stride;

                    int xcenter = rectbmp.X + rectbmp.Width >> 1;
                    int ycenter = rectbmp.Y + rectbmp.Height >> 1;

                    ltptlist.Clear();
                    rbptlist.Clear();
                    lenlist.Clear();

                    int min = 0;
                    int max = 0;

                    if (iswidth)
                    {
                        #region Check From Width

                        y = 0;

                        while (y < rectbmp.Height)
                        {
                            x = xcenter;

                            pucStart = scan0 + ((x - xmin) << 2) + (iStride * (y - ymin));
                            pucPtr = pucStart;
                            pucMin = pucPtr;

                            min = 0;

                            while (x > -1)
                            {
                                if (pucPtr[0] == 255)
                                {
                                    pucPtr[0] = 0;
                                    pucPtr[1] = 0;
                                    pucPtr[2] = 255;

                                    min = x;

                                    break;
                                }

                                pucPtr -= 4;
                                x--;
                            }

                            x = xcenter + 1;

                            pucStart = scan0 + ((x - xmin) << 2) + (iStride * (y - ymin));
                            pucPtr = pucStart;
                            pucMin = pucPtr;

                            max = 0;

                            while (x < rectbmp.Width)
                            {
                                if (pucPtr[0] == 255)
                                {
                                    pucPtr[0] = 0;
                                    pucPtr[1] = 0;
                                    pucPtr[2] = 255;

                                    max = x;
                                    break;
                                }
                                pucPtr += 4;
                                x++;
                            }

                            lenlist.Add(max - min);

                            ltptlist.Add(new Point(min, y));
                            rbptlist.Add(new Point(max, y));

                            pucStart += iStride * step;
                            y += step;
                        }
                        #endregion
                    }
                    else
                    {
                        while (x < rectbmp.Width)
                        {
                            y = ycenter;

                            pucStart = scan0 + ((x - xmin) << 2) + (iStride * (y - ymin));
                            pucPtr = pucStart;
                            pucMin = pucPtr;

                            min = 0;

                            while (y > -1)
                            {
                                if (pucPtr[0] == 255)
                                {
                                    pucPtr[0] = 0;
                                    pucPtr[1] = 0;
                                    pucPtr[2] = 255;

                                    min = y;

                                    break;
                                }
                                pucPtr -= iStride;
                                y--;
                            }

                            y = ycenter + 1;

                            pucStart = scan0 + ((x - xmin) << 2) + (iStride * (y - ymin));
                            pucPtr = pucStart;
                            pucMin = pucPtr;

                            max = 0;

                            while (y < rectbmp.Height)
                            {
                                if (pucPtr[0] == 255)
                                {
                                    pucPtr[0] = 0;
                                    pucPtr[1] = 0;
                                    pucPtr[2] = 255;

                                    max = y;

                                    break;
                                }

                                pucPtr += iStride;
                                y++;
                            }

                            lenlist.Add(max - min);

                            ltptlist.Add(new Point(x, min));
                            rbptlist.Add(new Point(x, max));

                            pucStart += 4 * step;
                            x += step;
                        }
                    }

                    bmp.UnlockBits(bmpData);
                }
            }
            catch (Exception ex)
            {
                bmp.UnlockBits(bmpData);
                //JetEazy.LoggerClass.Instance.WriteException(ex);
                if (IsDebug)
                    MessageBox.Show("Error :" + ex.ToString());
            }
        }
        /// <summary>
        /// 從外圍取得中間的數據
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="iswidth"></param>
        /// <param name="step"></param>
        /// <param name="ltptlist"></param>
        /// <param name="rbptlist"></param>
        /// <param name="lenlist"></param>
        public void GetInnerLineFromBorder(Bitmap bmp, bool iswidth, int step, ref List<PointF> ltptlist, ref List<PointF> rbptlist, ref List<int> lenlist)
        {
            Rectangle rectbmp = SimpleRect(bmp.Size);
            BitmapData bmpData = bmp.LockBits(rectbmp, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            IntPtr Scan0 = bmpData.Scan0;

            try
            {
                unsafe
                {
                    byte* scan0 = (byte*)(void*)Scan0;
                    byte* pucPtr;
                    byte* pucStart;
                    byte* pucMin;

                    int xmin = rectbmp.X;
                    int ymin = rectbmp.Y;
                    int xmax = xmin + rectbmp.Width - 1;
                    int ymax = ymin + rectbmp.Height - 1;

                    int x = xmin;
                    int y = ymin;

                    int iStride = bmpData.Stride;

                    int xcenter = rectbmp.X + rectbmp.Width >> 1;
                    int ycenter = rectbmp.Y + rectbmp.Height >> 1;

                    ltptlist.Clear();
                    rbptlist.Clear();
                    lenlist.Clear();

                    int min = 0;
                    int max = 0;

                    if (iswidth)
                    {
                        #region Check From Width

                        y = ymin;

                        while (y < rectbmp.Height)
                        {
                            x = xmax;

                            pucStart = scan0 + ((x - xmin) << 2) + (iStride * (y - ymin));
                            pucPtr = pucStart;
                            pucMin = pucPtr;

                            max = 0;

                            while (x > -1)
                            {
                                if (pucPtr[0] == 255)
                                {
                                    pucPtr[0] = 0;
                                    pucPtr[1] = 0;
                                    pucPtr[2] = 255;

                                    max = x;

                                    break;
                                }
                                else if (pucPtr[2] == 255)
                                {
                                    break;
                                }
                                pucPtr -= 4;
                                x--;
                            }

                            if (max != 0)
                            {
                                x = xmin;

                                pucStart = scan0 + ((x - xmin) << 2) + (iStride * (y - ymin));
                                pucPtr = pucStart;
                                pucMin = pucPtr;

                                min = 0;

                                while (x < rectbmp.Width)
                                {
                                    if (pucPtr[0] == 255)
                                    {
                                        pucPtr[0] = 0;
                                        pucPtr[1] = 0;
                                        pucPtr[2] = 255;

                                        min = x;
                                        break;
                                    }
                                    else if (pucPtr[2] == 255)
                                    {
                                        break;
                                    }

                                    pucPtr += 4;
                                    x++;
                                }
                            }


                            lenlist.Add(Math.Abs(max - min));

                            ltptlist.Add(new Point(min, y));
                            rbptlist.Add(new Point(max, y));

                            pucStart += iStride * step;
                            y += step;
                        }
                        #endregion
                    }
                    else
                    {

                        x = xmin;

                        while (x < rectbmp.Width)
                        {
                            y = ymax;

                            pucStart = scan0 + ((x - xmin) << 2) + (iStride * (y - ymin));
                            pucPtr = pucStart;
                            pucMin = pucPtr;

                            max = 0;

                            while (y > -1)
                            {
                                if (pucPtr[0] == 255)
                                {
                                    pucPtr[0] = 0;
                                    pucPtr[1] = 0;
                                    pucPtr[2] = 255;

                                    max = y;

                                    break;
                                }
                                else if (pucPtr[2] == 255)
                                {
                                    break;
                                }

                                pucPtr -= iStride;
                                y--;
                            }

                            if (max != 0)
                            {
                                y = ymin;

                                pucStart = scan0 + ((x - xmin) << 2) + (iStride * (y - ymin));
                                pucPtr = pucStart;
                                pucMin = pucPtr;

                                min = 0;

                                while (y < rectbmp.Height)
                                {
                                    if (pucPtr[0] == 255)
                                    {
                                        pucPtr[0] = 0;
                                        pucPtr[1] = 0;
                                        pucPtr[2] = 255;

                                        min = y;

                                        break;
                                    }
                                    else if (pucPtr[2] == 255)
                                    {
                                        break;
                                    }

                                    pucPtr += iStride;
                                    y++;
                                }

                            }

                            lenlist.Add(Math.Abs(max - min));

                            ltptlist.Add(new Point(x, min));
                            rbptlist.Add(new Point(x, max));

                            pucStart += 4 * step;
                            x += step;
                        }
                    }

                    bmp.UnlockBits(bmpData);
                }
            }
            catch (Exception ex)
            {
                bmp.UnlockBits(bmpData);
                //JetEazy.LoggerClass.Instance.WriteException(ex);
                if (IsDebug)
                    MessageBox.Show("Error :" + ex.ToString());
            }
        }

        public void MergeImage(Bitmap bmp, Bitmap bmpmask, Color definecolor, Color setcolor, bool isreverse)
        {
            Rectangle rectbmp = SimpleRect(bmp.Size);

            BitmapData bmpData = bmp.LockBits(rectbmp, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            BitmapData bmpData1 = bmpmask.LockBits(rectbmp, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            IntPtr Scan0 = bmpData.Scan0;
            IntPtr Scan1 = bmpData1.Scan0;

            try
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

                    //int ThresholdValueMax = Math.Min(255, ThresholdValue + ThresholdRangeUpper);
                    //int ThresholdValueMin = Math.Max(0, ThresholdValue - ThresholdRangeLower);

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
                            if (pucPtr1[0] == definecolor.B && pucPtr1[1] == definecolor.G && pucPtr1[2] == definecolor.R)
                            {
                                if (!isreverse)
                                {
                                    pucPtr[0] = pucPtr[0];
                                }
                                else
                                {
                                    //pucPtr[0] = setcolor.B;
                                    //pucPtr[1] = setcolor.G;
                                    //pucPtr[2] = setcolor.R;
                                    pucPtr[3] = setcolor.A;
                                }
                            }
                            else
                            {
                                if (!isreverse)
                                {
                                    //pucPtr[0] = setcolor.B;
                                    //pucPtr[1] = setcolor.G;
                                    //pucPtr[2] = setcolor.R;
                                    pucPtr[3] = setcolor.A;
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
            catch (Exception ex)
            {
                bmp.UnlockBits(bmpData);
                bmpmask.UnlockBits(bmpData1);
                //JetEazy.LoggerClass.Instance.WriteException(ex);
                if (IsDebug)
                    MessageBox.Show("Error :" + ex.ToString());
            }
        }

        #region Tools Operation
        bool IsInRange(float fromvalue, float compvalue, float diffvalue)
        {
            return Math.Abs(fromvalue - compvalue) < diffvalue;
        }
        Point GetRectCenter(Rectangle rect)
        {
            return new Point(rect.X + (rect.Width >> 1), rect.Y + (rect.Height >> 1));
        }
        int GrayscaleInt(byte R, byte G, byte B)
        {
            return (int)((double)R * 0.3 + (double)G * 0.59 + (double)B * 0.11);
        }
        Rectangle SimpleRect(Size sz)
        {
            return new Rectangle(0, 0, sz.Width, sz.Height);
        }
        void DrawCircle(ref Bitmap bmp, Rectangle rect, SolidBrush brushcolor)
        {
            Graphics g = Graphics.FromImage(bmp);
            g.FillEllipse(brushcolor, rect);
            g.Dispose();

        }
        void DrawCircle(ref Bitmap bmp, Rectangle rect, Color pencolor, int linewidth)
        {
            Graphics g = Graphics.FromImage(bmp);
            g.DrawEllipse(new Pen(Color.Red, linewidth), rect);
            g.Dispose();

        }
        int GetPointLength(Point p1, Point p2)
        {
            return (int)Math.Sqrt(Math.Pow((p1.X - p2.X), 2) + Math.Pow((p1.Y - p2.Y), 2));
        }
        Rectangle MergeTwoRects(Rectangle rect1, Rectangle rect2)
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
        double GetLTSuggestion(Point P1, Point P2, Rectangle Rect)
        {
            double Xp = (double)(P1.X - P2.X) / (double)(Rect.Width >> 1);
            double Yp = (double)(P1.Y - P2.Y) / (double)(Rect.Height >> 1);

            return Math.Pow(Xp, 2) + Math.Pow(Yp, 2);
        }

        bool IsInRangeEx(int fromvalue, int maxvalue, int minvalue)
        {
            return (fromvalue >= minvalue) && (fromvalue <= maxvalue);
        }
        bool IsInRangeEx(double fromvalue, double maxvalue, double minvalue)
        {
            return (fromvalue >= minvalue) && (fromvalue <= maxvalue);
        }

        public void SetColorThreshold(Bitmap bmp, Rectangle rect, bool isreverse)
        {
            lock (obj)
            {
                int grade = 0;

                Rectangle rectbmp = rect;
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
                                grade = GrayscaleInt(pucPtr[2], pucPtr[1], pucPtr[0]);

                                if (isreverse)
                                {
                                    grade = 255 - grade;
                                }

                                if (grade > 220)
                                {
                                    pucPtr[0] = 0;
                                    pucPtr[1] = 0;
                                    pucPtr[2] = 255;
                                }
                                else if (grade > 180)
                                {
                                    pucPtr[0] = 0;
                                    pucPtr[1] = 128;
                                    pucPtr[2] = 255;
                                }
                                else if (grade > 140)
                                {
                                    pucPtr[0] = 0;
                                    pucPtr[1] = 255;
                                    pucPtr[2] = 0;
                                }
                                else if (grade > 100)
                                {
                                    pucPtr[0] = 128;
                                    pucPtr[1] = 255;
                                    pucPtr[2] = 0;
                                }
                                else if (grade > 60)
                                {
                                    pucPtr[0] = 255;
                                    pucPtr[1] = 0;
                                    pucPtr[2] = 0;
                                }
                                else
                                {
                                    pucPtr[0] = 128;
                                    pucPtr[1] = 0;
                                    pucPtr[2] = 0;
                                }

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
                    bmp.UnlockBits(bmpData);
                    //JetEazy.LoggerClass.Instance.WriteException(ex);
                    if (IsDebug)
                        MessageBox.Show("Error :" + ex.ToString());
                }
            }
        }

        public void ClearCheck()
        {
            foreach (FoundClass found in FoundList)
            {
                found.IsChecked = false;
            }
        }

        public void Reverse(Bitmap bmp)
        {
            int Grade = 0;

            Rectangle rectbmp = SimpleRect(bmp.Size);
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
                            Grade = GrayscaleInt(pucPtr[2], pucPtr[1], pucPtr[0]);

                            *((uint*)pucPtr) = (Grade > 0 ? 0xFF000000 : 0xFFFFFFFF);

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
                bmp.UnlockBits(bmpData);
                //JetEazy.LoggerClass.Instance.WriteException(ex);
                if (IsDebug)
                    MessageBox.Show("Error :" + ex.ToString());
            }
        }

        public void FillLines(Bitmap bmp, int gap, Color color)
        {
            Graphics g = Graphics.FromImage(bmp);

            int height = bmp.Height;

            int i = 3;

            while (i < height)
            {
                g.DrawLine(new Pen(Color.Black, 1), new Point(0, i), new Point(bmp.Width + 1, i));
                i += gap;
            }

            g.Dispose();
        }

        #endregion


        public void AH_SetThreshold(ref Bitmap bmp, int iThresholdValue)
        {
            Bitmap _bmptmp = new Bitmap(bmp);

            JetGrayImg grayimage = new JetGrayImg(_bmptmp);
            JetImgproc.Threshold(grayimage, iThresholdValue, grayimage);
            bmp = new Bitmap(grayimage.ToBitmap());
        }
        public void AH_FindBlob(Bitmap bmp, bool IsFindWhite)
        {
            JetGrayImg grayimage = new JetGrayImg(bmp);
            JetBlob jetBlob = new JetBlob();
            jetBlob.Labeling(grayimage, JConnexity.Connexity4, (IsFindWhite ? JBlobLayer.WhiteLayer : JBlobLayer.BlackLayer));
            int icount = jetBlob.BlobCount;
            FoundList.Clear();

            for (int i = 0; i < icount; i++)
            {
                int iArea = JetBlobFeature.ComputeIntegerFeature(jetBlob, i, JBlobIntFeature.Area);
                int itop = JetBlobFeature.ComputeIntegerFeature(jetBlob, i, JBlobIntFeature.TopMost);
                int iLeft = JetBlobFeature.ComputeIntegerFeature(jetBlob, i, JBlobIntFeature.LeftMost);
                int iRight = JetBlobFeature.ComputeIntegerFeature(jetBlob, i, JBlobIntFeature.RightMost);
                int iBottom = JetBlobFeature.ComputeIntegerFeature(jetBlob, i, JBlobIntFeature.BottomMost);

                Rectangle rect = new Rectangle(iLeft, itop, iRight - iLeft, iBottom - itop);
                FoundClass found = new FoundClass(rect, iArea);
                found.rotatedRectangleF = JetBlobFeature.ComputeMinRectangle(jetBlob, i);
                FoundList.Add(found);
            }
        }
    }

    class FindCornerClass
    {
        List<int> XWay = new List<int>();
        List<int> YWay = new List<int>();

        JzToolsClass myJzTools = new JzToolsClass();

        bool IsDebug
        {
            get
            {
                return false;
            }
        }

        public Point EndPoint = new Point();
        public int CutLength = 0;
        public FindCornerClass()
        {


        }
        public void Reset()
        {
            XWay.Clear();
            YWay.Clear();
            myJzTools.ClearPoint(ref EndPoint);
        }

    }
    public class ConvolutionMatrix
    {
        public int MatrixSize = 3;

        public double[,] Matrix;
        public double Factor = 1;
        public double Offset = 1;

        public ConvolutionMatrix(int size)
        {
            MatrixSize = 3;
            Matrix = new double[size, size];
        }

        public void SetAll(double value)
        {
            for (int i = 0; i < MatrixSize; i++)
            {
                for (int j = 0; j < MatrixSize; j++)
                {
                    Matrix[i, j] = value;
                }
            }
        }
    }
    public class ImageProcessor
    {
        private Bitmap bitmapImage = new Bitmap(1, 1);

        public void SetImage(string path)
        {
            bitmapImage = new Bitmap(path);
        }
        public void SetImage(Bitmap bmp)
        {
            bitmapImage.Dispose();
            bitmapImage = new Bitmap(bmp);
        }
        public Bitmap GetImage()
        {
            return bitmapImage;
        }
        public void ApplyInvert()
        {
            byte A, R, G, B;
            Color pixelColor;

            for (int y = 0; y < bitmapImage.Height; y++)
            {
                for (int x = 0; x < bitmapImage.Width; x++)
                {
                    pixelColor = bitmapImage.GetPixel(x, y);
                    A = pixelColor.A;
                    R = (byte)(255 - pixelColor.R);
                    G = (byte)(255 - pixelColor.G);
                    B = (byte)(255 - pixelColor.B);
                    bitmapImage.SetPixel(x, y, Color.FromArgb((int)A, (int)R, (int)G, (int)B));
                }
            }

        }

        public void ApplyGreyscale()
        {
            byte A, R, G, B;
            Color pixelColor;

            for (int y = 0; y < bitmapImage.Height; y++)
            {
                for (int x = 0; x < bitmapImage.Width; x++)
                {
                    pixelColor = bitmapImage.GetPixel(x, y);
                    A = pixelColor.A;
                    R = (byte)((0.299 * pixelColor.R) + (0.587 * pixelColor.G) + (0.114 * pixelColor.B));
                    G = B = R;

                    bitmapImage.SetPixel(x, y, Color.FromArgb((int)A, (int)R, (int)G, (int)B));
                }
            }

        }
        public void ApplyGamma(double r, double g, double b)
        {
            byte A, R, G, B;
            Color pixelColor;

            byte[] redGamma = new byte[256];
            byte[] greenGamma = new byte[256];
            byte[] blueGamma = new byte[256];

            for (int i = 0; i < 256; ++i)
            {
                redGamma[i] = (byte)Math.Min(255, (int)((255.0
                    * Math.Pow(i / 255.0, 1.0 / r)) + 0.5));
                greenGamma[i] = (byte)Math.Min(255, (int)((255.0
                    * Math.Pow(i / 255.0, 1.0 / g)) + 0.5));
                blueGamma[i] = (byte)Math.Min(255, (int)((255.0
                    * Math.Pow(i / 255.0, 1.0 / b)) + 0.5));
            }

            for (int y = 0; y < bitmapImage.Height; y++)
            {
                for (int x = 0; x < bitmapImage.Width; x++)
                {
                    pixelColor = bitmapImage.GetPixel(x, y);
                    A = pixelColor.A;
                    R = redGamma[pixelColor.R];
                    G = greenGamma[pixelColor.G];
                    B = blueGamma[pixelColor.B];
                    bitmapImage.SetPixel(x, y, Color.FromArgb((int)A, (int)R, (int)G, (int)B));
                }
            }
        }
        public void ApplyColorFilter(double r, double g, double b)
        {
            byte A, R, G, B;
            Color pixelColor;

            for (int y = 0; y < bitmapImage.Height; y++)
            {
                for (int x = 0; x < bitmapImage.Width; x++)
                {
                    pixelColor = bitmapImage.GetPixel(x, y);
                    A = pixelColor.A;
                    R = (byte)(pixelColor.R * r);
                    G = (byte)(pixelColor.G * g);
                    B = (byte)(pixelColor.B * b);
                    bitmapImage.SetPixel(x, y, Color.FromArgb((int)A, (int)R, (int)G, (int)B));
                }
            }
        }
        public void ApplySepia(int depth)
        {
            int A, R, G, B;
            Color pixelColor;

            for (int y = 0; y < bitmapImage.Height; y++)
            {
                for (int x = 0; x < bitmapImage.Width; x++)
                {
                    pixelColor = bitmapImage.GetPixel(x, y);
                    A = pixelColor.A;
                    R = (int)((0.299 * pixelColor.R) + (0.587 * pixelColor.G) + (0.114 * pixelColor.B));
                    G = B = R;

                    R += (depth * 2);
                    if (R > 255)
                    {
                        R = 255;
                    }
                    G += depth;
                    if (G > 255)
                    {
                        G = 255;
                    }

                    bitmapImage.SetPixel(x, y, Color.FromArgb(A, R, G, B));
                }
            }
        }
        public void ApplyDecreaseColourDepth(int offset)
        {
            int A, R, G, B;
            Color pixelColor;

            for (int y = 0; y < bitmapImage.Height; y++)
            {
                for (int x = 0; x < bitmapImage.Width; x++)
                {
                    pixelColor = bitmapImage.GetPixel(x, y);
                    A = pixelColor.A;
                    R = ((pixelColor.R + (offset / 2)) - ((pixelColor.R + (offset / 2)) % offset) - 1);
                    if (R < 0)
                    {
                        R = 0;
                    }
                    G = ((pixelColor.G + (offset / 2)) - ((pixelColor.G + (offset / 2)) % offset) - 1);
                    if (G < 0)
                    {
                        G = 0;
                    }
                    B = ((pixelColor.B + (offset / 2)) - ((pixelColor.B + (offset / 2)) % offset) - 1);
                    if (B < 0)
                    {
                        B = 0;
                    }
                    bitmapImage.SetPixel(x, y, Color.FromArgb(A, R, G, B));
                }
            }

        }
        public void ApplyContrast(double contrast)
        {
            double A, R, G, B;

            Color pixelColor;

            contrast = (100.0 + contrast) / 100.0;
            contrast *= contrast;

            for (int y = 0; y < bitmapImage.Height; y++)
            {
                for (int x = 0; x < bitmapImage.Width; x++)
                {
                    pixelColor = bitmapImage.GetPixel(x, y);
                    A = pixelColor.A;

                    R = pixelColor.R / 255.0;
                    R -= 0.5;
                    R *= contrast;
                    R += 0.5;
                    R *= 255;

                    if (R > 255)
                    {
                        R = 255;
                    }
                    else if (R < 0)
                    {
                        R = 0;
                    }

                    G = pixelColor.G / 255.0;
                    G -= 0.5;
                    G *= contrast;
                    G += 0.5;
                    G *= 255;
                    if (G > 255)
                    {
                        G = 255;
                    }
                    else if (G < 0)
                    {
                        G = 0;
                    }

                    B = pixelColor.B / 255.0;
                    B -= 0.5;
                    B *= contrast;
                    B += 0.5;
                    B *= 255;
                    if (B > 255)
                    {
                        B = 255;
                    }
                    else if (B < 0)
                    {
                        B = 0;
                    }

                    bitmapImage.SetPixel(x, y, Color.FromArgb((int)A, (int)R, (int)G, (int)B));
                }
            }

        }
        public void ApplyBrightness(int brightness)
        {
            int A, R, G, B;
            Color pixelColor;

            for (int y = 0; y < bitmapImage.Height; y++)
            {
                for (int x = 0; x < bitmapImage.Width; x++)
                {
                    pixelColor = bitmapImage.GetPixel(x, y);
                    A = pixelColor.A;
                    R = pixelColor.R + brightness;
                    if (R > 255)
                    {
                        R = 255;
                    }
                    else if (R < 0)
                    {
                        R = 0;
                    }

                    G = pixelColor.G + brightness;
                    if (G > 255)
                    {
                        G = 255;
                    }
                    else if (G < 0)
                    {
                        G = 0;
                    }

                    B = pixelColor.B + brightness;
                    if (B > 255)
                    {
                        B = 255;
                    }
                    else if (B < 0)
                    {
                        B = 0;
                    }

                    bitmapImage.SetPixel(x, y, Color.FromArgb(A, R, G, B));
                }
            }

        }
        public void ApplySmooth(double weight)
        {
            ConvolutionMatrix matrix = new ConvolutionMatrix(3);
            matrix.SetAll(1);
            matrix.Matrix[1, 1] = weight;
            matrix.Factor = weight + 8;
            bitmapImage = Convolution3x3(bitmapImage, matrix);

        }
        public void ApplyGaussianBlur(double peakValue)
        {
            ConvolutionMatrix matrix = new ConvolutionMatrix(3);
            matrix.SetAll(1);
            matrix.Matrix[0, 0] = peakValue / 4;
            matrix.Matrix[1, 0] = peakValue / 2;
            matrix.Matrix[2, 0] = peakValue / 4;
            matrix.Matrix[0, 1] = peakValue / 2;
            matrix.Matrix[1, 1] = peakValue;
            matrix.Matrix[2, 1] = peakValue / 2;
            matrix.Matrix[0, 2] = peakValue / 4;
            matrix.Matrix[1, 2] = peakValue / 2;
            matrix.Matrix[2, 2] = peakValue / 4;
            matrix.Factor = peakValue * 4;
            bitmapImage = Convolution3x3(bitmapImage, matrix);

        }
        public void ApplySharpen(double weight)
        {
            ConvolutionMatrix matrix = new ConvolutionMatrix(3);
            matrix.SetAll(1);
            matrix.Matrix[0, 0] = 0;
            matrix.Matrix[1, 0] = -2;
            matrix.Matrix[2, 0] = 0;
            matrix.Matrix[0, 1] = -2;
            matrix.Matrix[1, 1] = weight;
            matrix.Matrix[2, 1] = -2;
            matrix.Matrix[0, 2] = 0;
            matrix.Matrix[1, 2] = -2;
            matrix.Matrix[2, 2] = 0;
            matrix.Factor = weight - 8;
            bitmapImage = Convolution3x3(bitmapImage, matrix);

        }
        public void ApplyMeanRemoval(double weight)
        {
            ConvolutionMatrix matrix = new ConvolutionMatrix(3);
            matrix.SetAll(1);
            matrix.Matrix[0, 0] = -1;
            matrix.Matrix[1, 0] = -1;
            matrix.Matrix[2, 0] = -1;
            matrix.Matrix[0, 1] = -1;
            matrix.Matrix[1, 1] = weight;
            matrix.Matrix[2, 1] = -1;
            matrix.Matrix[0, 2] = -1;
            matrix.Matrix[1, 2] = -1;
            matrix.Matrix[2, 2] = -1;
            matrix.Factor = weight - 8;
            bitmapImage = Convolution3x3(bitmapImage, matrix);

        }
        public void ApplyEmboss(double weight)
        {
            ConvolutionMatrix matrix = new ConvolutionMatrix(3);
            matrix.SetAll(1);
            matrix.Matrix[0, 0] = -1;
            matrix.Matrix[1, 0] = 0;
            matrix.Matrix[2, 0] = -1;
            matrix.Matrix[0, 1] = 0;
            matrix.Matrix[1, 1] = weight;
            matrix.Matrix[2, 1] = 0;
            matrix.Matrix[0, 2] = -1;
            matrix.Matrix[1, 2] = 0;
            matrix.Matrix[2, 2] = -1;
            matrix.Factor = 4;
            matrix.Offset = 127;
            bitmapImage = Convolution3x3(bitmapImage, matrix);

        }
        public Bitmap Convolution3x3(Bitmap b, ConvolutionMatrix m)
        {
            Bitmap newImg = (Bitmap)b.Clone();
            Color[,] pixelColor = new Color[3, 3];
            int A, R, G, B;

            for (int y = 0; y < b.Height - 2; y++)
            {
                for (int x = 0; x < b.Width - 2; x++)
                {
                    pixelColor[0, 0] = b.GetPixel(x, y);
                    pixelColor[0, 1] = b.GetPixel(x, y + 1);
                    pixelColor[0, 2] = b.GetPixel(x, y + 2);
                    pixelColor[1, 0] = b.GetPixel(x + 1, y);
                    pixelColor[1, 1] = b.GetPixel(x + 1, y + 1);
                    pixelColor[1, 2] = b.GetPixel(x + 1, y + 2);
                    pixelColor[2, 0] = b.GetPixel(x + 2, y);
                    pixelColor[2, 1] = b.GetPixel(x + 2, y + 1);
                    pixelColor[2, 2] = b.GetPixel(x + 2, y + 2);

                    A = pixelColor[1, 1].A;

                    R = (int)((((pixelColor[0, 0].R * m.Matrix[0, 0]) +
                                 (pixelColor[1, 0].R * m.Matrix[1, 0]) +
                                 (pixelColor[2, 0].R * m.Matrix[2, 0]) +
                                 (pixelColor[0, 1].R * m.Matrix[0, 1]) +
                                 (pixelColor[1, 1].R * m.Matrix[1, 1]) +
                                 (pixelColor[2, 1].R * m.Matrix[2, 1]) +
                                 (pixelColor[0, 2].R * m.Matrix[0, 2]) +
                                 (pixelColor[1, 2].R * m.Matrix[1, 2]) +
                                 (pixelColor[2, 2].R * m.Matrix[2, 2]))
                                        / m.Factor) + m.Offset);

                    if (R < 0)
                    {
                        R = 0;
                    }
                    else if (R > 255)
                    {
                        R = 255;
                    }

                    G = (int)((((pixelColor[0, 0].G * m.Matrix[0, 0]) +
                                 (pixelColor[1, 0].G * m.Matrix[1, 0]) +
                                 (pixelColor[2, 0].G * m.Matrix[2, 0]) +
                                 (pixelColor[0, 1].G * m.Matrix[0, 1]) +
                                 (pixelColor[1, 1].G * m.Matrix[1, 1]) +
                                 (pixelColor[2, 1].G * m.Matrix[2, 1]) +
                                 (pixelColor[0, 2].G * m.Matrix[0, 2]) +
                                 (pixelColor[1, 2].G * m.Matrix[1, 2]) +
                                 (pixelColor[2, 2].G * m.Matrix[2, 2]))
                                        / m.Factor) + m.Offset);

                    if (G < 0)
                    {
                        G = 0;
                    }
                    else if (G > 255)
                    {
                        G = 255;
                    }

                    B = (int)((((pixelColor[0, 0].B * m.Matrix[0, 0]) +
                                 (pixelColor[1, 0].B * m.Matrix[1, 0]) +
                                 (pixelColor[2, 0].B * m.Matrix[2, 0]) +
                                 (pixelColor[0, 1].B * m.Matrix[0, 1]) +
                                 (pixelColor[1, 1].B * m.Matrix[1, 1]) +
                                 (pixelColor[2, 1].B * m.Matrix[2, 1]) +
                                 (pixelColor[0, 2].B * m.Matrix[0, 2]) +
                                 (pixelColor[1, 2].B * m.Matrix[1, 2]) +
                                 (pixelColor[2, 2].B * m.Matrix[2, 2]))
                                        / m.Factor) + m.Offset);

                    if (B < 0)
                    {
                        B = 0;
                    }
                    else if (B > 255)
                    {
                        B = 255;
                    }
                    newImg.SetPixel(x + 1, y + 1, Color.FromArgb(A, R, G, B));
                }
            }
            return newImg;
        }

    }
}
