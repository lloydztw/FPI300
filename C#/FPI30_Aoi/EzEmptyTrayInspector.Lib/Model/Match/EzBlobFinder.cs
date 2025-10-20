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

using JetEazy.Match;
using OpenCvSharp;
using System.Collections.Generic;


namespace EzAoiEmptyTrayInspector.Model.Aoi
{
    public class EzBlobFinder
    {
        public int MorphIterations = 1;
        public bool OptFillBorder = true;
        public Size? MinSize = null;
        public Size? MaxSize = null;

        /// <summary>
        /// binaryImg 內容可能會被改變
        /// </summary>
        public int FindWhiteBlobs(Mat binaryImg, out List<EzBloc> keyBlocs)
        {
            keyBlocs = new List<EzBloc>();

            if (MorphIterations > 0)
            {
                Cv2.Dilate(binaryImg, binaryImg, null, iterations: MorphIterations);
                Cv2.Erode(binaryImg, binaryImg, null, iterations: MorphIterations);
            }

            if (OptFillBorder)
            {
                var rect = new Rect(0, 0, binaryImg.Width, binaryImg.Height);
                binaryImg.Rectangle(rect, Scalar.White);
                Cv2.FloodFill(binaryImg, new OpenCvSharp.Point(0, 0), Scalar.Black);
            }

            int min_w;  // = img.Width / 3;
            int min_h;  // = img.Height / 3;
            int max_w;  // = img.Width;
            int max_h;  // = img.Height;
            if(MinSize.HasValue)
            {
                min_w = MinSize.Value.Width;
                min_h = MinSize.Value.Height;
            }
            else
            {
                min_w = binaryImg.Width / 3;
                min_h = binaryImg.Height / 3;
            }
            if (MaxSize.HasValue)
            {
                max_w = MaxSize.Value.Width;
                max_h = MaxSize.Value.Height;
            }
            else
            {
                max_w = binaryImg.Width;
                max_h = binaryImg.Height;
            }

            var cc = Cv2.ConnectedComponentsEx(binaryImg);
            for (int i = 1; i < cc.Blobs.Count; i++)
            {
                var ccBlob = cc.Blobs[i];

                if (ccBlob.Width < min_w || ccBlob.Height < min_h ||
                    ccBlob.Width > max_w || ccBlob.Height > max_h)
                    continue;

                var rect = JetEazy.Qcvt.CC(ccBlob.Rect);

                //// UNSHRINK
                //if (_shrinkFactor > 1)
                //{
                //    rect.X *= _shrinkFactor;
                //    rect.Y *= _shrinkFactor;
                //    rect.Width *= _shrinkFactor;
                //    rect.Height *= _shrinkFactor;
                //}

                var bloc = new EzBloc(rect, 0);
                bloc.Pixels = ccBlob.Area;
                bloc.Center = new JetEazy.QMath.QVector(ccBlob.Centroid.X, ccBlob.Centroid.Y); // 保留精度 !
                keyBlocs.Add(bloc);
            }

            return keyBlocs.Count;
        }

        public static bool CheckIfDarkBackGround(Mat img)
        {
            if (img == null || img.Width < 32 || img.Height < 32)
                return true;

            Mat imgU8 = ImageUtil.ToU8(img);

            int bw = img.Width > 100 ? 8 : 2;
            int W = imgU8.Width;
            int H = imgU8.Height;
            var bound = new Rect(0, 0, W, H);
            var rois = new Rect[]
            {
                new Rect(0,0, bw,bw),
                new Rect(W-bw,0, bw,bw),
                new Rect(W-bw,H-bw, bw,bw),
                new Rect(0,H-bw, bw,bw),
            };

            var meanColor = imgU8.Mean().Val0;
            int countDark = 0;
            int countLight = 0;
            for (int i = 0, len = rois.Length; i < len; i++)
            {
                var roi = rois[i];
                JetEazy.Qcvt.ClipBoundary(ref roi, ref bound);
                if (roi.Width < 1 || roi.Height < 1)
                    continue;
                var color = (imgU8[roi]).Mean().Val0;
                if (color < meanColor)
                    countDark++;
                else
                    countLight++;
            }

            if (imgU8 != img)
                imgU8?.Dispose();

            return countDark > countLight;
        }
    }
}
