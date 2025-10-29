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



using JetEazy.QMath;
using JetEazy.Utils;
using LaserAlignDX.Model;
using LaserAlignDX.Mvc.Gui;
using LaserAlignDX.OPSpace;
using System.Collections.Generic;
using System.Drawing;


namespace LaserAlignDX.AoiModel.AI
{
    public class AiDataCropper
    {
        const int IMAGE_SIZE = 224 * 2;

        #region PATH
        public static string PATH_DATA_ROOT => "d:\\paso.log\\Ai\\Data";
        #endregion

        public AiDataCropper()
        {
        }

        public void GenerateData(string srcName, Bitmap bmpFullFov, IEnumerable<RegionCellX3Class> cells)
        {
            if (cells != null)
            {
                foreach (var cell in cells)
                {
                    //var chipData = cell?.ChipData;
                    //if (chipData == null) continue;
                    //var passNgResults = chipData.ChipDimension.PassNgResults;
                    //if (passNgResults == null || passNgResults.Length < 2)
                    //    continue;
                    //foreach (var isPass in passNgResults)
                    //    if (!isPass)
                    //        continue;

                    // 尺寸量測結果為 PASS 才納入 AI 訓練數據
                    var chipDim = cell?.ChipData?.ChipDimension;
                    if (chipDim == null || !chipDim.IsAllPass())
                        continue;

                    GenerateDataOneCell(srcName, bmpFullFov, cell);
                }
            }
        }

        public void GenerateDataOneCell(string srcName, Bitmap bmpFullFov, RegionCellX3Class cell)
        {
            var chipData = cell?.ChipData;
            var chipQuad = chipData?.ChipQuad2D;
            if (chipQuad == null)
                return;

            var chipDimCornerPoints = getChipDimCorners(chipData);
            if (chipDimCornerPoints == null || chipDimCornerPoints.Length < 4)
                return;

            parseNames(srcName, out string lotStr, out string dateStr);
            srcName = lotStr + "-" + dateStr;
            string dstPath = System.IO.Path.Combine(PATH_DATA_ROOT, lotStr);
            JetEazy.IO.QxPathUtility.InitDirectory(dstPath);

            int index = 0;
            foreach (var roiCenter in chipQuad.Corners)
            {
                var dimPt = chipDimCornerPoints[index];
                if (dimPt == null)
                    continue;

                var roi = JetEazy.Qcvt.CreateCenterRect((int)roiCenter.X, (int)roiCenter.Y, IMAGE_SIZE, IMAGE_SIZE);
                GaUtil.Clip(ref roi, bmpFullFov.Size);
                var bmpCrop = bmpFullFov.Clone(roi, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                var x = dimPt.X - roi.X;
                var y = dimPt.Y - roi.Y;

                string fileName = System.IO.Path.Combine(dstPath, $"{srcName}@{cell.CellRow}_{cell.CellCol}#{index}.png");
                bmpCrop.Save(fileName);

                fileName = System.IO.Path.ChangeExtension(fileName, ".txt");
                System.IO.File.WriteAllText(fileName, $"{x}, {y}");

                index++;
            }

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
                    var rect = JetEazy.Qcvt.CreateCenterRect((float)pt.X, (float)pt.Y, IMAGE_SIZE, IMAGE_SIZE);
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

        #region PRIVATE_FUNCTIONS

        QVector[] getChipDimCorners(GaChipData chipData)
        {
            if (chipData != null)
            {
                var points = new List<QVector>();
                var lines = chipData.LineSegments;
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

        #endregion
    }
}
