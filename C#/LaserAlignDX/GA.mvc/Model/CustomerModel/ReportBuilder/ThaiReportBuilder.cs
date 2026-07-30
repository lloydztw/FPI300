#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-08-25 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.Utils;
using LaserAlignDX.AoiModel;
using LaserAlignDX.BasicSpace;
using LaserAlignDX.OPSpace;
using LaserAlignDX.OPSpace.RecipeSpace;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Traveller106;

namespace LaserAlignDX.Model
{
    /// <summary>
    /// 德龍/泰國 報表
    /// </summary>
    public class ThaiReportBuilder : IxReportBuilder
    {
        #region CONFIG
        //static string _digitFormat => StringBuilderExt.DIGIT_FORMAT;
        #endregion

        #region GLOBAL_MESS
        RecipeFPIX3Class _xRecipe => RecipeFPIX3Class.Instance;
        #endregion

        #region PRIVATE_DATA
        string _resultImagePath => INI.Instance.ResultImagePath;
        string _stripId;
        string _fileName;
        #endregion

        public string GenerateReport(string stripId, string fileName, bool optOutputFinalText)
        {
            bool byRowCol = false;

            try
            {
                _stripId = stripId;
                _fileName = fileName;

                var reportSB = new StringBuilder();

                appendSummary(reportSB);
                appendHeader(reportSB);

                if (!byRowCol)
                {
                    foreach (var cell in PlcDataPacker.IterFinalResultCells())
                    {
                        appendOneCellData(reportSB, cell);
                    }
                }
                else
                {
                    //---------------------------------------------------------------------------------------------
                    // 2026-04-04
                    // 使用 RegionCellsDataCollection 來管理 _xRecipe.xRegionCells
                    // 以維持 PASS/NG 統計數量的一致性 !
                    //---------------------------------------------------------------------------------------------

                    using (var cellsCollection = new RegionCellsDataCollection(_xRecipe.xRegionCells))
                    {
                        int rows = cellsCollection.Rows;
                        int cols = cellsCollection.Cols;
                        for (int r = 0; r < rows; r++)
                        {
                            for (int c = 0; c < cols; c++)
                            {
                                var cell = cellsCollection.GetFinalCell(r, c);
                                appendOneCellData(reportSB, cell);
                            }
                        }
                    }
                }

                saveReportData(reportSB);

                if (optOutputFinalText)
                    return reportSB.ToString();
            }
            catch (Exception ex)
            {
                GaUtil.LOG_ERROR(ex, "Report Generation Error!");
            }
            return null;
        }

        #region PRIVATE_FUNCTIONS
        void appendSummary(StringBuilder sb)
        {
            var collections = new Dictionary<string, int>();
            int totalCount = 0;
            foreach (var cell in PlcDataPacker.IterFinalResultCells())
            {
                GetResultText(cell, out string resultText, out bool isSkip);
                if (isSkip) continue;
                totalCount++;
                if (!collections.ContainsKey(resultText))
                    collections[resultText] = 1;
                else
                    collections[resultText]++;
            }

            if (!collections.TryGetValue("OK", out int okCount))
                okCount = 0;
            int ngCount = Math.Max(totalCount - okCount, 0);

            sb.Append("SUMMARY").AppendLine();

            sb.Append("Total Count");
            sb.Append(", ").Append(totalCount);
            sb.AppendLine();

            sb.Append("Total Pass");
            sb.Append(", ").Append(okCount); 
            sb.Append(", ").Append(FormatCsvPercentage(okCount, totalCount));
            sb.AppendLine();

            sb.Append("Total NG");
            sb.Append(", ").Append(ngCount);
            sb.Append(", ").Append(FormatCsvPercentage(ngCount, totalCount));
            sb.AppendLine();

            var showKeys = new List<string>();
            foreach (PlcResultCode code in Enum.GetValues(typeof(PlcResultCode)))
            {
                if (code == PlcResultCode.OK || code == PlcResultCode.NG_EMPTY)
                    continue;
                string key = code.ToString();
                if (showKeys.Contains(key))
                    continue;
                showKeys.Add(key);
            }
            foreach (string key in showKeys)
            {
                if (!collections.TryGetValue(key, out int count))
                    count = 0;
                sb.Append(key).Append(", ").Append(count);
                sb.Append(", ").Append(FormatCsvPercentage(count, totalCount));
                sb.AppendLine();
            }

            sb.AppendLine();
            sb.AppendLine();
        }
        void appendHeader(StringBuilder sb)
        {
            //sb.Append("編號, 行列標記, 晶粒狀態, 寬(尺寸X), 高(尺寸Y), 原始X, 原始Y, 補償X, 補償Y, 補償角度");
            //sb.Append(", 邊隙(左), 邊隙(上), 邊隙(右), 邊隙(下), 邊隙(左右差)");
            //sb.Append(", TiltRatio");
            //sb.Append(", 馬達1.X, 馬達1.Y, 馬達2.X, 馬達2.Y");
            //sb.Append(", 條碼設定值, 讀取碼");
            //sb.AppendLine();
            sb.Append("Index, Row Col, Result, Width(X), Height(Y), Target(X), Target(Y)");
            sb.Append(", Compensate(X), Compensate(Y), Compensate(Angle)");
            sb.Append(", Gap(Left), Gap(Up), Gap(Right), Gap(Bottom), GapDiff(Left-Right)");
            sb.Append(", TiltRatio");
            sb.Append(", Motor1(X), Motor1(Y), Motor2(X), Motor2(Y)");
            sb.Append(", Barcode(Read)");
            sb.AppendLine();
        }
        void appendOneCellData(StringBuilder sb, RegionCellX3Class cell)
        {
            if (cell == null)
                return;

            //>>>sb.AppendValues(cell.Index, cell.CellRow, cell.CellCol);
            sb.Append(cell.Index).Append(", ").Append($"r{cell.CellRow:000} c{cell.CellCol:000}");

            GetResultText(cell, out string resultText, out bool isSkip);
            sb.Append(", ").Append(resultText);

            var gaps = cell.ChipData?.PadEdgeGaps;
            var chipCoords = cell?.ChipData?.ChipCoords;
            float tiltRatio = chipCoords != null ? (float)chipCoords.TiltRatio : 0f;

            sb.Append(", ").AppendValues(

                isSkip ? 0f : cell.RunWidth,
                isSkip ? 0f : cell.RunHeight,
                isSkip ? 0f : cell.OrgX,
                isSkip ? 0f : cell.OrgY,
                isSkip ? 0f : cell.RunX,
                isSkip ? 0f : cell.RunY,
                isSkip ? 0f : cell.RunAngle,

                gaps != null ? (float)gaps.GetAveGap(EdgeBorder.Left) : 0f,
                gaps != null ? (float)gaps.GetAveGap(EdgeBorder.Top) : 0f,
                gaps != null ? (float)gaps.GetAveGap(EdgeBorder.Right) : 0f,
                gaps != null ? (float)gaps.GetAveGap(EdgeBorder.Bottom) : 0f,
                gaps != null ? (float)gaps.GetAveGapDiff() : 0f,

                tiltRatio
            );

            sb.Append(", ").AppendPointF(isSkip ? PointF.Empty : cell.Sur1);
            sb.Append(", ").AppendPointF(isSkip ? PointF.Empty : cell.Sur2);

            sb.Append(", ").Append(""); // isSkip || cell.RunCodeInfo == null ? "" : cell.RunCodeInfo.Content);
            //sb.Append(", ").Append(isSkip ? "" : cell.SetBarcodeStr);

            sb.AppendLine();
        }
        void saveReportData(StringBuilder reportSB)
        {
            //>>> string path = $"{_resultImagePath}\\report\\{DateTime.Now.ToString("yyyyMMdd")}\\{_stripID}";
            string path = System.IO.Path.Combine(_resultImagePath, "report", DateTime.Now.ToString("yyyyMMdd"), _stripId);
            string fname = System.IO.Path.ChangeExtension(_fileName, ".csv");

            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);

            string fileName = System.IO.Path.Combine(path, fname);
            GaUtil.SaveData(reportSB.ToString(), fileName);
        }

        #endregion

        void GetResultText(RegionCellX3Class cell, out string resultText, out bool isSkip)
        {
            isSkip = cell == null || cell.IsEmptyPlaceHold() || cell.IsAmbiguousBloc();
            var isPass = cell != null && cell.IsResultPass();
            PlcResultCode code = PlcDataPacker.GetPlcCode(cell);

            if (isSkip || code == PlcResultCode.NG_EMPTY)
            {
                resultText = "n/a";
            }
            else if (isPass)
            {
                resultText = PlcResultCode.OK.ToString();
            }
            else
            {
                resultText = code.ToString();
            }
        }
        string FormatCsvPercentage(int count, int total)
        {
            var percent = total > 0 ? ((double)count / total) : 0.0;
            return percent.ToString("0.00 %");
        }
    }
}