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
    public class PowerTechReportBuilder : IxReportBuilder
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
                        cellsCollection.Detach();
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
            sb.Append("編號, 行列標記, 晶粒狀態, 尺寸(寬), 尺寸(高), 原始X, 原始Y, 補償X, 補償Y, 補償角度");
            sb.Append(", LUX, LUY, RUX, RUY, RDX, RDY, LDX, LDY");
            sb.Append(", S1, S2, S3, S4, S5, S6, S7, S8");
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

                gaps != null ? (float)gaps.LU.X : 0f,
                gaps != null ? (float)gaps.LU.Y : 0f,
                gaps != null ? (float)gaps.RU.X : 0f,
                gaps != null ? (float)gaps.RU.Y : 0f,
                gaps != null ? (float)gaps.RD.X : 0f,
                gaps != null ? (float)gaps.RD.Y : 0f,
                gaps != null ? (float)gaps.LD.X : 0f,
                gaps != null ? (float)gaps.LD.Y : 0f,

                gaps != null ? (float)gaps.S1 : 0f,
                gaps != null ? (float)gaps.S2 : 0f,
                gaps != null ? (float)gaps.S3 : 0f,
                gaps != null ? (float)gaps.S4 : 0f,
                gaps != null ? (float)gaps.S5 : 0f,
                gaps != null ? (float)gaps.S6 : 0f,
                gaps != null ? (float)gaps.S7 : 0f,
                gaps != null ? (float)gaps.S8 : 0f,

                tiltRatio
            );

            sb.Append(", ").AppendPointF(isSkip ? PointF.Empty : cell.Sur1);
            sb.Append(", ").AppendPointF(isSkip ? PointF.Empty : cell.Sur2);

            sb.Append(", ").Append(isSkip ? "" : cell.SetBarcodeStr);
            sb.Append(", ").Append(isSkip || cell.RunCodeInfo == null ? "" : cell.RunCodeInfo.Content);

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