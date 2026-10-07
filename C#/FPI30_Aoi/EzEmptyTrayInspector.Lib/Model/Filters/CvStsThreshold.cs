#region AUTHOR
/*
 * 
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2026-09-04 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using OpenCvSharp;
using System;

namespace JetEazy.OpenCV
{
    public static class CvStsThreshold
    {
        /// <summary>
        /// 利用邊緣梯度加權平均法計算影像的最佳二值化門檻值。
        /// </summary>
        /// <param name="image">輸入的單通道 8-bit 灰階影像 (CV_8UC1)。</param>
        /// <param name="mask">選擇性的遮罩影像 (CV_8UC1)，僅對 Mask 內非零區域進行計算。</param>
        /// <returns>計算出的門檻灰階值 (0~255)。若無有效邊緣像素則傳回 0。</returns>
        public unsafe static int CalculateThreshold(Mat image, Mat mask = null)
        {
            if (image == null || image.IsDisposed)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (image.Type() != MatType.CV_8UC1)
            {
                throw new ArgumentException("Source image must be CV_8UC1 (8-bit single channel).", nameof(image));
            }

            bool hasMask = mask != null && !mask.IsDisposed;
            if (hasMask)
            {
                if (mask.Type() != MatType.CV_8UC1 || mask.Size() != image.Size())
                {
                    throw new ArgumentException("Mask must be CV_8UC1 and have the same dimensions as the input image.", nameof(mask));
                }
            }

            int width = image.Width;
            int height = image.Height;

            // 計算區域需要至少 3x3 像素才能計算微分
            if (width < 3 || height < 3)
            {
                return 0;
            }

            long imgStep = image.Step();
            long maskStep = hasMask ? mask.Step() : 0;

            long totalWeight = 0;       // Sum(Weight)
            long weightedSum = 0;       // Sum(Weight * PixelValue)

            byte* imgBasePtr = (byte*)image.Data.ToPointer();
            byte* maskBasePtr = hasMask ? (byte*)mask.Data.ToPointer() : null;

            // 避開影像外圍 1 像素邊界，避免微移時指標越界
            for (int y = 1; y < height - 1; y++)
            {
                byte* imgRowPtr = imgBasePtr + (y * imgStep) + 1;
                byte* maskRowPtr = hasMask ? (maskBasePtr + (y * maskStep) + 1) : null;

                for (int x = 1; x < width - 1; x++)
                {
                    // 如果有傳入 Mask，且當前像素（以及上下左右鄰居）在 Mask 中為 0，則跳過
                    if (hasMask)
                    {
                        if (*maskRowPtr == 0 ||
                            maskRowPtr[-1] == 0 || maskRowPtr[1] == 0 ||
                            maskRowPtr[-maskStep] == 0 || maskRowPtr[maskStep] == 0)
                        {
                            imgRowPtr++;
                            maskRowPtr++;
                            continue;
                        }
                    }

                    // 水平梯度：|ptr[1] - ptr[-1]|
                    int dx = imgRowPtr[1] - imgRowPtr[-1];
                    if (dx < 0) dx = -dx;

                    // 垂直梯度：|ptr[step] - ptr[-step]|
                    int dy = (int)*(imgRowPtr + imgStep) - (int)*(imgRowPtr - imgStep);
                    if (dy < 0) dy = -dy;

                    // 取較大值作為邊緣強度（權重）
                    int weight = (dx > dy) ? dx : dy;

                    if (weight > 0)
                    {
                        totalWeight += weight;
                        weightedSum += (long)weight * (*imgRowPtr);
                    }

                    imgRowPtr++;
                    if (hasMask)
                    {
                        maskRowPtr++;
                    }
                }
            }

            // 計算梯度加權平均灰階值
            if (totalWeight > 0)
            {
                return (int)(weightedSum / totalWeight);
            }

            return 0;
        }

        public static int Apply(Mat src, Mat dst, Mat mask = null)
        {
            int thres = CalculateThreshold(src, mask);
            Cv2.Threshold(src, mask, thres, 255, ThresholdTypes.Binary);
            return thres;
        }

        public static Mat StsThreshold(this Mat src, Mat mask = null)
        {
            Mat dst = new Mat();
            Apply(src, dst, mask);
            return dst;
        }
    }
}