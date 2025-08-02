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

using JetEazy.EzImage;
using OpenCvSharp;
using System.Collections.Generic;
using CvPoint = OpenCvSharp.Point;


namespace EzEmptyTrayInspector.Model.Aoi
{
    public class EzRotAngleFinder
    {
        public bool OptForceFind = false;

        public double ApplyFilters(IEzImage largeImg, JxRotAngleSettings settings, bool autoShrink, out List<RotatedRect> rotRects)
        {
            return ApplyFilters(largeImg?.Image as Mat, settings, autoShrink, out rotRects);
        }

        public double ApplyFilters(Mat imgSrc, JxRotAngleSettings settings, bool autoShrink, out List<RotatedRect> rotRects)
        {
            rotRects = null;

            if (imgSrc == null || settings == null)
                return 0;

            if (!OptForceFind && !settings.Enabled)
                return 0;

            Mat imgShrink = null;
            Mat imgGray = null;
            Mat imgWork = null;

            try
            {
                var thres = (int)settings.Threshold.Value;
                var thres2 = (int)settings.Threshold2.Value;

                double zoomX = 1;
                double zoomY = 1;

                if (autoShrink)
                {
                    imgShrink = EzAoiBaseUtil.ApplyAutoShrink(imgSrc, out zoomX, out zoomY);
                    imgGray = EzAoiBaseUtil.ApplyGray(imgShrink);
                }
                else
                {
                    imgShrink = imgSrc;
                    imgGray = EzAoiBaseUtil.ApplyGray(imgShrink);
                }

                // OTSU
                if (thres < 10)
                {
                    Cv2.Threshold(imgGray, imgGray, 0, 255, ThresholdTypes.Otsu);
                    imgWork = imgGray;
                }
                // Canny
                else
                {
                    imgWork = imgGray.GaussianBlur(new Size(3, 3), 1);
                    //imgWork = imgGray.MedianBlur(5);
                    var canny = imgGray.Canny((double)thres, (double)thres2);
                    var old = imgWork;
                    imgWork = canny;
                    old?.Dispose();
                }

                find_contour_rotated_rects(imgWork, out rotRects);

                if (autoShrink && (zoomX != 1 || zoomY != 1))
                    EzAoiBaseUtil.ApplyZoom(rotRects, zoomX, zoomY);

                double bestAngle = rotRects.Count > 0 ? EzAoiBaseUtil.GetNormalizedAngle(rotRects[0]) : 0;
                return bestAngle;
            }
            finally
            {
                if (imgWork != imgGray)
                    imgWork?.Dispose();
                if (imgGray != imgShrink)
                    imgGray?.Dispose();
                if (imgShrink != imgSrc)
                    imgShrink?.Dispose();
            }
        }

        #region PRIVATE_FUNCTIONS
        void find_contour_rotated_rects(Mat img, out List<RotatedRect> rotRects)
        {
            CvPoint[][] contours;
            HierarchyIndex[] hierarchy;
            Cv2.FindContours(img, out contours, out hierarchy, RetrievalModes.List, ContourApproximationModes.ApproxSimple);

            var bound = new Rect(0, 0, img.Width, img.Height);
            int minWidth = img.Width / 8;
            int minHeight = img.Height / 8;
            int gap = 3;

            rotRects = new List<RotatedRect>();

            // 迭代每一個輪廓
            foreach (var contour in contours)
            {
                // 使用 MinAreaRect 來計算旋轉矩形
                RotatedRect rotatedRect = Cv2.MinAreaRect(contour);

                // 篩選
                var rect = rotatedRect.BoundingRect();
                if (rect.X <= gap || rect.Y <= gap || rect.Right >= bound.Right - gap || rect.Bottom >= bound.Bottom - gap)
                    continue;
                if (rect.Width < minWidth || rect.Height < minHeight)
                    continue;

                rotRects.Add(rotatedRect);
            }

            if (rotRects.Count > 2)
            {
                // 面積大的排前面
                rotRects.Sort((r1, r2) => (int)(r2.Size.Width * r2.Size.Height - r1.Size.Width * r1.Size.Height));
            }
        }
        #endregion
    }
}
