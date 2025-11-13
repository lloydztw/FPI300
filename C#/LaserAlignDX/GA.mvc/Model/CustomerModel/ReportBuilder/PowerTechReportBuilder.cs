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
using LaserAlignDX.BasicSpace;
using System;
using System.Text;
using Traveller106;

using CELL = LaserAlignDX.OPSpace.RegionCellX3Class;
using RECIPE = LaserAlignDX.OPSpace.RecipeSpace.RecipeFPIX3Class;


namespace LaserAlignDX.Model
{
    /// <summary>
    /// 德龍/力成廠 報表
    /// </summary>
    public class PowerTechReportBuilder : IxReportBuilder
    {
        #region CONFIG
        static string _digitFormat => StringBuilderExt.DIGIT_FORMAT;
        #endregion

        #region PRIVATE_DATA
        /// <summary>
        /// RegionCellX3Class 太雜亂了 !!!
        /// </summary>
        RECIPE _xRecipe => RECIPE.Instance;
        string _resultImagePath => INI.Instance.ResultImagePath;
        string _stripId;
        string _fileName;
        #endregion

        public string GenerateReport(string stripId, string fileName, bool optOutputFinalText)
        {
            _stripId = stripId;
            _fileName = fileName;

            var reportSB = new StringBuilder();

            appendHeader(reportSB);

            //>>> foreach (CELL cell in _xRecipe.xRegionCells)
            foreach(var cell in PlcDataPacker.IterFinalResultCells())
            {
                appendOneCellData(reportSB, cell);
            }

            saveReportData(reportSB);

            if (optOutputFinalText)
                return reportSB.ToString();

            return null;
        }

        #region PRIVATE_FUNCTIONS
        void appendHeader(StringBuilder sb)
        {
            sb.Append("編號, Row, Col, 是否檢測, 尺寸(寬), 尺寸(高), 原始X, 原始Y, 補償X, 補償Y, 補償角度");
            sb.Append(", LUX, LUY, RUX, RUY, RDX, RDY, LDX, LDY");
            //sb.Append(", S1, S2, S3, S4, S5, S6, S7, S8");
            sb.Append(", 馬達1.X, 馬達1.Y, 馬達2.X, 馬達2.Y");
            sb.Append(", 條碼設定值, 讀取碼");
            sb.AppendLine();
        }
        void appendOneCellData(StringBuilder sb, CELL cell)
        {
            var gaps = cell?.ChipData?.PadEdgeGaps;

            sb.AppendValues(cell.Index, cell.CellRow, cell.CellCol);
            sb.Append((cell.ByPass ? (INI.Instance.IsForceInspect ? "1强制检测" : "0不检测") : "1检测")).Append(",");

            sb.AppendValues(
                cell.RunWidth,
                cell.RunHeight,

                cell.OrgX,
                cell.OrgY,
                cell.RunX,
                cell.RunY,
                cell.RunAngle,

                gaps != null ? (float)gaps.LU.X : 0f,
                gaps != null ? (float)gaps.LU.Y : 0f,
                gaps != null ? (float)gaps.RU.X : 0f,
                gaps != null ? (float)gaps.RU.Y : 0f,
                gaps != null ? (float)gaps.RD.X : 0f,
                gaps != null ? (float)gaps.RD.Y : 0f,
                gaps != null ? (float)gaps.LD.X : 0f,
                gaps != null ? (float)gaps.LD.Y : 0f

                //gaps != null ? (float)gaps.S1 : 0f,
                //gaps != null ? (float)gaps.S2 : 0f,
                //gaps != null ? (float)gaps.S3 : 0f,
                //gaps != null ? (float)gaps.S4 : 0f,
                //gaps != null ? (float)gaps.S5 : 0f,
                //gaps != null ? (float)gaps.S6 : 0f,
                //gaps != null ? (float)gaps.S7 : 0f,
                //gaps != null ? (float)gaps.S8 : 0f

            ).Append(",");

            sb.AppendPointF(cell.Sur1).Append(",");
            sb.AppendPointF(cell.Sur2).Append(",");

            sb.Append(cell.SetBarcodeStr).Append(",");
            if (cell.RunCodeInfo != null)
                sb.Append(cell.RunCodeInfo.Content);
            else
                sb.Append("");

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

        #region OLD_FUNCTIONS
        /// <summary>
        /// 舊代碼: 優化 把此函式 改為直接對 StringBuilder append !!!
        /// </summary>
        string ToReport1HeadStr()
        {
            string str = string.Empty;
#if (OPT_OLD)
            str += $"编号" + ",";
            str += $"名称" + ",";
            str += $"是否检测" + ",";
            str += $"尺寸宽度X" + ",";
            str += $"尺寸高度Y" + ",";
            str += $"位置偏移X" + ",";
            str += $"位置偏移Y" + ",";
            str += $"原始X" + ",";
            str += $"原始Y" + ",";
            str += $"引导偏移X" + ",";
            str += $"引导偏移Y" + ",";
            str += $"引导偏移角度" + ",";

            str += $"左边距" + ",";
            str += $"右边距" + ",";
            str += $"上边距" + ",";
            str += $"下边距" + ",";

            str += $"马达1-X" + ",";
            str += $"马达1-Y" + ",";
            str += $"马达2-X" + ",";
            str += $"马达2-Y" + ",";
            str += $"条码设定值" + ",";
            str += $"读取码" + ",";
            str += $"{Environment.NewLine}";
#endif
            return str;
        }
        /// <summary>
        /// 舊代碼: 優化 把此函式 改為直接對 StringBuilder append !!!
        /// </summary>
        string ToReport1Str(CELL cell)
        {
            string str = string.Empty;
#if (OPT_OLD)
            str += $"{cell.Index}" + ",";
            str += $"{cell.lblName}" + ",";
            str += $"{(cell.ByPass ? (INI.Instance.IsForceInspect ? "1强制检测" : "0不检测") : "1检测")}" + ",";
            str += $"{cell.RunWidth.ToString(_digitFormat)}" + ",";
            str += $"{cell.RunHeight.ToString(_digitFormat)}" + ",";
            str += $"{cell.RunXOffset.ToString(_digitFormat)}" + ",";
            str += $"{cell.RunYOffset.ToString(_digitFormat)}" + ",";
            str += $"{cell.OrgX.ToString(_digitFormat)}" + ",";
            str += $"{cell.OrgY.ToString(_digitFormat)}" + ",";
            str += $"{cell.RunX.ToString(_digitFormat)}" + ",";
            str += $"{cell.RunY.ToString(_digitFormat)}" + ",";
            str += $"{cell.RunAngle.ToString(_digitFormat)}" + ",";

            str += $"{cell.DisLeft.ToString(_digitFormat)}" + ",";
            str += $"{cell.DisRight.ToString(_digitFormat)}" + ",";
            str += $"{cell.DisTop.ToString(_digitFormat)}" + ",";
            str += $"{cell.DisBottom.ToString(_digitFormat)}" + ",";

            string format(PointF pt)
            {
                //return $"{pt.X:0.000},{pt.Y:0.000}";
                return pt.X.ToString(_digitFormat) + "," + pt.Y.ToString(_digitFormat);
            };

            str += format(cell.Sur1) + ",";
            str += format(cell.Sur2) + ",";
            str += cell.SetBarcodeStr + ",";
            if (cell.RunCodeInfo != null)
                str += cell.RunCodeInfo.Content + ",";
            else
                str += ",";
            str += Environment.NewLine;
#endif
            return str;
        }
        #endregion
    }
}