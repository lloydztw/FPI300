#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-10-29 開始佈署 AI (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion



using JetEazy.OpenCV;
using JetEazy.QMath;
using JetEazy.Utils;
using LaserAlignDX.Model;
using LaserAlignDX.Mvc.Gui;
using LaserAlignDX.OPSpace;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;


namespace LaserAlignDX.AoiModel.AI
{
    public class AiDataCropper
    {
        const int IMAGE_CROP_SIZE = 224 * 2;

        #region PATH
        public string PATH_DATA_ROOT => "d:\\paso.log\\Ai\\Data\\_RawData";
        #endregion

        public AiDataCropper()
        {
        }

        public List<IvDrawItem> GetDrawItems(RegionCellX3Class cell)
        {
            var drawItems = new List<IvDrawItem>();

            var chipData = cell?.ChipData;
            var chipQuad = chipData?.ChipQuad2D;

            if (chipQuad != null)
            {
                foreach (var pt in chipQuad.Corners)
                {
                    var rect = JetEazy.Qcvt.CreateCenterRect((float)pt.X, (float)pt.Y, IMAGE_CROP_SIZE, IMAGE_CROP_SIZE);
                    var item = new CviRotRectBox(rect, Color.WhiteSmoke);
                    drawItems.Add(item);
                }
            }

            var chipDimCorners = getChipDimCorners(chipData);
            if (chipDimCorners != null)
            {
                foreach (var pt in chipDimCorners)
                {
                    var rect = JetEazy.Qcvt.CreateCenterRect((float)pt.X, (float)pt.Y, 10, 10);
                    var item = new CviRotRectBox(rect, Color.WhiteSmoke) { CrossLength = 25 };
                    drawItems.Add(item);
                }
            }

            return drawItems;
        }

        public void GenerateData(string srcName, Bitmap bmpFullFov, IEnumerable<RegionCellX3Class> cells)
        {
            //重整 srcName
            parseNames(srcName, out string lotStr, out string dateStr);
            srcName = lotStr + "-" + dateStr;
            string dstPath = System.IO.Path.Combine(PATH_DATA_ROOT, lotStr);
            JetEazy.IO.QxPathUtility.InitDirectory(dstPath);

            if (cells != null)
            {
                using (var maskFullFov = makeGlobalMask(bmpFullFov, cells))
                {
                    foreach (var cell in cells)
                    {
                        // 尺寸量測結果為 PASS 才納入 AI 訓練數據
                        if (!isOkForAI(cell))
                            continue;

                        string fileStemName = System.IO.Path.Combine(dstPath, $"{srcName}@{cell.CellRow}_{cell.CellCol}");
                        GenerateDataOneCell(fileStemName, bmpFullFov, maskFullFov, cell?.ChipData);
                    }
                }
            }
        }

        public void GenerateDataOneCell(string fileStemName, Bitmap bmpFullFov, Mat maskFullFov, GaChipData chipData)
        {
            //>>> var chipData = cell?.ChipData;
            var chipQuad = chipData?.ChipQuad2D;
            if (chipQuad == null)
                return;

            //晶粒四邊線 (左上右下)
            var lines = chipData.LineBorderPairs.GetQuadLineSegments();

            //檢查
            int NP = 4;
            if (lines == null || lines.Length < NP) return;
            foreach (var line in lines)
                if (line == null)
                    return;

            //順序: (左上右下)
            int index = 0;
            foreach (var cropCenter in chipQuad.Corners)
            {
                // Cropping
                var cropRect = JetEazy.Qcvt.CreateCenterRect((int)cropCenter.X, (int)cropCenter.Y, IMAGE_CROP_SIZE, IMAGE_CROP_SIZE);
                GaUtil.Clip(ref cropRect, bmpFullFov.Size);
                var bmpCrop = bmpFullFov.Clone(cropRect, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);

                //(1) 保存 Image File
                //>>> string fileName = System.IO.Path.Combine(dstPath, $"{srcName}@{cell.CellRow}_{cell.CellCol}#{index}.png");
                string fileName = fileStemName + $"#{index}.png";
                bmpCrop.Save(fileName);

                //(2) 保存 Label File
                fileName = System.IO.Path.ChangeExtension(fileName, ".txt");
                var sb = new StringBuilder();

                //(2.1) ImgSize
                sb.AppendLine($"ImgSize, {cropRect.Width}, {cropRect.Height}");

                //(2.2) 交點 (以 cropRect 左上角為原點)
                var lineA = lines[index];
                var lineB = lines[(index + 1) % NP];
                var dimCornerPt = lineA.CalcIntersectedPoint(lineB);
                dimCornerPt = dimCornerPt - new QVector(cropRect.X, cropRect.Y);
                sb.AppendLine($"points, {dimCornerPt.X}, {dimCornerPt.Y}");

                //(2.3) [r,θ] 極座標參數化的直線 (以 cropRect 中心為原點)
                var cc = JetEazy.Qcvt.CenterF(ref cropRect);
                var org = new QVector(cc.X, cc.Y);
                (var r1, var theta1) = lineA.GetRhoTheta(org);
                (var r2, var theta2) = lineB.GetRhoTheta(org);
                sb.AppendLine($"lines, {r1}, {theta1}, {r2}, {theta2}");

                // DEBUG
                if (false)
                {
                    using (var bridge = new QxImageBridge(bmpCrop))
                    using (var canvas = new Mat())
                    {
                        Cv2.CvtColor(bridge.Image, canvas, ColorConversionCodes.GRAY2BGR);
                        DrawPolarLine(canvas, r1, theta1, Scalar.Cyan);
                        DrawPolarLine(canvas, r2, theta2, Scalar.DarkCyan);
                        canvas.SaveImage("d:\\paso.log\\polar_lines.png");
                    }
                }

                //(2.*) write to text file
                System.IO.File.WriteAllText(fileName, sb.ToString());

                //(3) 保存 mask
                if (maskFullFov != null)
                {
                    var roi = JetEazy.Qcvt.CV(cropRect);
                    var mask = maskFullFov[roi];
                    fileName = fileStemName + $"#{index}.mask.bmp";
                    mask.SaveImage(fileName);
                }

                //(4) index
                index++;
            }
        }

        #region PRIVATE_FUNCTIONS
        void parseNames(string srcName, out string lotStr, out string dateStr)
        {
            lotStr = "NA";
            dateStr = "NA";
            
            if (string.IsNullOrWhiteSpace(srcName))
                return;

            srcName = System.IO.Path.GetFileNameWithoutExtension(srcName);
            if (srcName.Contains("-"))
            {
                var strs = srcName.Split('-');
                lotStr = strs[0];
                if (strs.Length > 1)
                {
                    dateStr = strs[1];
                    if(dateStr.Length > 8)
                    {
                        var str = dateStr.Substring(0, 8) + "_" + dateStr.Substring(8);
                        dateStr = str;
                    }
                }
            }
            else
            {
                lotStr = srcName;
            }
        }
        Mat makeGlobalMask(Bitmap bmpFullFov, IEnumerable<RegionCellX3Class> cells)
        {
            using (var bridge = new QxImageBridge(bmpFullFov))
            {
                Mat imgSrc = bridge.Image;
                Mat mask = Mat.Zeros(imgSrc.Size(), MatType.CV_8UC1);

                foreach (var cell in cells)
                {
                    // 尺寸量測結果為 PASS 才納入 AI 訓練數據
                    if (!isOkForAI(cell))
                        continue;

                    var chipData = cell.ChipData;
                    if (chipData == null)
                        continue;

                    var chipCorners = getChipDimCorners(chipData);
                    var pts = Array.ConvertAll(chipCorners, c => new OpenCvSharp.Point(Math.Round(c.X), Math.Round(c.Y)));
                    Cv2.FillConvexPoly(mask, pts, Scalar.White);
                }

                return mask;
            }
        }
        bool isOkForAI(RegionCellX3Class cell)
        {
            // 尺寸量測結果為 PASS 才納入 AI 訓練數據
            var chipDim = cell?.ChipData?.ChipDimension;
            if (chipDim == null)
                return false;

            //晶粒四邊線 (左上右下)
            var lines = cell?.ChipData?.LineBorderPairs.GetQuadLineSegments();

            //檢查
            int NP = 4;
            if (lines == null || lines.Length < NP)
                return false;

            foreach (var line in lines)
                if (line == null)
                    return false;

            return true;
        }
        QVector[] getChipDimCorners(GaChipData chipData)
        {
            if (chipData != null)
            {
                var points = new List<QVector>();
                var lines = chipData.LineBorderPairs.GetQuadLineSegments();
                if (lines != null && lines.Length >= 4)
                {
                    for (int i = 0, NP = lines.Length; i < NP; i++)
                    {
                        int j = (i + 1) % NP;
                        var line1 = lines[i];
                        var line2 = lines[j];
                        if (line1 == null || line2 == null) continue;
                        var pt = line1.CalcIntersectedPoint(line2);
                        if (pt == null) continue;
                        points.Add(pt);
                    }
                }

                if (points.Count >= 4)
                {
                    return points.ToArray();
                }
            }

            return null;
        }
        static void DrawPolarLine(Mat img, double r, double theta, Scalar color, int thickness = 1)
        {
            int W = img.Cols;
            int H = img.Rows;
            double cx = W / 2.0;
            double cy = H / 2.0;

            double a = Math.Cos(theta);
            double b = Math.Sin(theta);

            // 直線上通過點
            double x0 = cx + a * r;
            double y0 = cy + b * r;

            // 計算線的兩端點，遠離中心以保證整條線能畫滿
            var pt1 = new OpenCvSharp.Point(
                (int)(x0 + 1000 * (-b)),
                (int)(y0 + 1000 * (a))
            );
            var pt2 = new OpenCvSharp.Point(
                (int)(x0 - 1000 * (-b)),
                (int)(y0 - 1000 * (a))
            );

            Cv2.Line(img, pt1, pt2, color, thickness);

            // 畫出中心點
            Cv2.Circle(img, new OpenCvSharp.Point((int)cx, (int)cy), 4, new Scalar(255, 255, 255), -1);

            //// 畫出法向量箭頭
            //Cv2.ArrowedLine(
            //    img,
            //    new OpenCvSharp.Point((int)cx, (int)cy),
            //    new OpenCvSharp.Point((int)(cx + 80 * a), (int)(cy + 80 * b)),
            //    new Scalar(0, 0, 255),
            //    2,
            //    LineTypes.AntiAlias,
            //    0,
            //    0.2
            //);
        }
        #endregion
    }
}
