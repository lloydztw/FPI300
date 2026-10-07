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
using System.Drawing;
using System.Text;
using Traveller106;


namespace LaserAlignDX.Model
{
    /// <summary>
    /// 德龍/力成廠 報表
    /// </summary>
    public class JcetReportBuilder : IxReportBuilder
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
                GaUtil.LOG_ERROR(ex, "生成報表異常");
            }

            return null;
        }

        #region PRIVATE_FUNCTIONS
        void appendHeader(StringBuilder sb)
        {
            sb.Append("編號, 行列標記, 晶粒狀態, 寬(尺寸X), 高(尺寸Y), 原始X, 原始Y, 補償X, 補償Y, 補償角度");
            sb.Append(", 邊隙(左), 邊隙(上), 邊隙(右), 邊隙(下), 邊隙(左右差)");
            sb.Append(", TiltRatio");
            sb.Append(", 馬達1.X, 馬達1.Y, 馬達2.X, 馬達2.Y");
            sb.Append(", 條碼設定值, 讀取碼");
            sb.AppendLine();
        }
        void appendOneCellData(StringBuilder sb, RegionCellX3Class cell)
        {
            if (cell == null)
                return;

            var gaps = cell.ChipData?.PadEdgeGaps;
            var isSkip = cell == null || cell.IsEmptyPlaceHold() || cell.IsAmbiguousBloc();
            var isPass = cell != null && cell.IsResultPass();

            var chipCoords = cell?.ChipData?.ChipCoords;
            float tiltRatio = chipCoords != null ? (float)chipCoords.TiltRatio : 0f;

            //>>>sb.AppendValues(cell.Index, cell.CellRow, cell.CellCol);
            sb.Append(cell.Index).Append(", ").Append($"r{cell.CellRow:000} c{cell.CellCol:000}");

            //>>> sb.Append(", ").Append((cell.ByPass ? (INI.Instance.IsForceInspect ? "1強制檢測" : "0不檢測") : "1檢測"));
            sb.Append(", ").Append(isSkip ? "無" : isPass ? "PASS" : "NG");

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

            sb.Append(", ").Append(isSkip ? "" : cell.BarcodeResultText);
            sb.Append(", ").Append(""); // isSkip || cell.RunCodeInfo == null ? "" : cell.RunCodeInfo.Content);

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
    }
}