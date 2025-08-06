#region AUTHOR
/*
 * 
 * Copyright (c) 2024 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2024-10-19 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using OpenCvSharp;
using System.Collections.Generic;
using CvPoint = OpenCvSharp.Point;


namespace EzAoiEmptyTrayInspector.Model.Aoi
{
    public class EzAoiBaseUtil
    {
        public static Mat ApplyAutoShrink(Mat imgSrc, out double zoomX, out double zoomY, bool disposeOld = false)
        {
            var shrinkFactor = GetShrinkFactor(imgSrc.Width, imgSrc.Height);
            if (shrinkFactor > 1)
            {
                int w = imgSrc.Width / shrinkFactor;
                int h = imgSrc.Height / shrinkFactor;
                var img = imgSrc.Resize(new OpenCvSharp.Size(w, h), interpolation: InterpolationFlags.Linear);
                zoomX = (double)imgSrc.Width / w;
                zoomY = (double)imgSrc.Height / h;
                if (disposeOld)
                    imgSrc.Dispose();
                return img;
            }
            zoomX = 1;
            zoomY = 1;
            return imgSrc;
        }
        public static Mat ApplyGray(Mat imgSrc, bool disposeOld = false)
        {
            Mat img;
            
            switch (imgSrc.Channels())
            {
                case 4: img = imgSrc.CvtColor(ColorConversionCodes.BGRA2GRAY); break;
                case 3: img = imgSrc.CvtColor(ColorConversionCodes.BGR2GRAY); break;
                case 2: img = imgSrc.CvtColor(ColorConversionCodes.BGR5652GRAY); break;
                default: img = imgSrc; break;
            }
            
            if (disposeOld && img != imgSrc)
                imgSrc.Dispose();

            return img;
        }
        
        public static int GetShrinkFactor(int imgWidth, int imgHeight)
        {
            int shrinkFactor = 1;
            int w = imgWidth;
            int h = imgHeight;
            while (w > 5000 || h > 5000)
            {
                shrinkFactor++;
                w = imgWidth / shrinkFactor;
                h = imgHeight / shrinkFactor;
            }
            return shrinkFactor;
        }
        public static int GetShrinkFactor(Mat img)
        {
            if (img == null)
                return 1;
            return GetShrinkFactor(img.Width, img.Height);
        }

        public static void ApplyZoom(List<RotatedRect> rotRects, double zoomX, double zoomY)
        {
            for (int i = 0; i < rotRects.Count; i++)
            {
                var rect = rotRects[i];
                // 將 RotatedRect 的尺寸進行縮放
                Size2f newSize = new Size2f(rect.Size.Width * zoomX, rect.Size.Height * zoomY);
                Point2f newCenter = new Point2f((float)(rect.Center.X * zoomX), (float)(rect.Center.Y * zoomY));
                // 回存 新的 RotatedRect，保持角度不變
                rotRects[i] = new RotatedRect(newCenter, newSize, rect.Angle);
            }
        }
        public static double GetNormalizedAngle(RotatedRect rotRect, bool inRadian = false)
        {
            //---------------------------------------------------------------------------------
            //在 OpenCv 中，Angle 通常在範圍 -90° 到 0° 之間。
            //
            //   0° 表示較長的邊平行於水平軸（未旋轉或水平放置）。
            // -45° 表示矩形逆時針旋轉了 45°。
            // -90° 表示矩形的較短邊與水平線平行，這是因為當矩形旋轉 90° 後，較短邊變成水平。
            //---------------------------------------------------------------------------------
            //      Angle = 0°    ->  ───
            //      Angle = -30°  ->  ／
            //      Angle = -45°  ->  \
            //      Angle = -90°  ->  │
            //---------------------------------------------------------------------------------

            var size = rotRect.Size;
            System.Diagnostics.Debug.Assert(size.Width >= 0 && size.Height >= 0);
            double angle;
            if (size.Width <= size.Height)
            {
                angle = rotRect.Angle;
            }
            else
            {
                if (rotRect.Angle > 0)
                    angle = rotRect.Angle - 90;
                else
                    angle = rotRect.Angle + 90;
            }
            return angle;
        }
        public static void Dump(string fileName, Mat imgSrcU8, IEnumerable<RotatedRect> rotRects)
        {
            using (Mat result = new Mat())
            {
                Cv2.CvtColor(imgSrcU8, result, ColorConversionCodes.GRAY2BGR);
                foreach (var rotatedRect in rotRects)
                {
                    // 繪製旋轉矩形
                    Point2f[] rectPoints = rotatedRect.Points();
                    for (int i = 0; i < 4; i++)
                    {

                        Cv2.Line(result, (CvPoint)rectPoints[i], (CvPoint)rectPoints[(i + 1) % 4], Scalar.Red, 2);
                    }
                }
                result.SaveImage(fileName);
            }
        }
    }
}
