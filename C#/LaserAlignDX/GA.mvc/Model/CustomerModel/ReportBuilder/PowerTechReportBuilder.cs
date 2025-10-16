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

            foreach (CELL cell in _xRecipe.xRegionCells)
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
            // sb.Append(ToReport1HeadStr());
            // return;

            sb.Append("编号").Append(",");
            sb.Append("名称").Append(",");
            sb.Append("是否检测").Append(",");
            sb.Append("尺寸宽度X").Append(",");
            sb.Append("尺寸高度Y").Append(",");
            //sb.Append("位置偏移X").Append(",");
            //sb.Append("位置偏移Y").Append(",");
            sb.Append("原始X").Append(",");
            sb.Append("原始Y").Append(",");
            sb.Append("引导偏移X").Append(",");
            sb.Append("引导偏移Y").Append(",");
            sb.Append("引导偏移角度").Append(",");

            //sb.Append("左边距").Append(",");
            //sb.Append("右边距").Append(",");
            //sb.Append("上边距").Append(",");
            //sb.Append("下边距").Append(",");

            sb.Append("LUX").Append(",");
            sb.Append("LUY").Append(",");
            sb.Append("RUX").Append(",");
            sb.Append("RUY").Append(",");
            sb.Append("RDX").Append(",");
            sb.Append("RDY").Append(",");
            sb.Append("LDX").Append(",");
            sb.Append("LDY").Append(",");

            sb.Append("马达1-X").Append(",");
            sb.Append("马达1-Y").Append(",");
            sb.Append("马达2-X").Append(",");
            sb.Append("马达2-Y").Append(",");
            sb.Append("条码设定值").Append(",");
            sb.Append("读取码").Append(",");
            sb.AppendLine();
        }
        void appendOneCellData(StringBuilder sb, CELL cell)
        {
            //sb.Append(ToReport1Str(cell));
            //return;

            //str += $"{cell.Index}" + ",";
            //str += $"{cell.lblName}" + ",";
            sb.Append(cell.Index).Append(",");
            sb.Append(cell.lblName).Append(",");
            sb.Append((cell.ByPass ? (INI.Instance.IsForceInspect ? "1强制检测" : "0不检测") : "1检测")).Append(",");

            //sb.Append(cell.RunWidth.ToString(_digitFormat)).Append(",");
            //sb.Append(cell.RunHeight.ToString(_digitFormat)).Append(",");
            //sb.Append(cell.RunXOffset.ToString(_digitFormat)).Append(",");
            //sb.Append(cell.RunYOffset.ToString(_digitFormat)).Append(",");

            //sb.Append(cell.OrgX.ToString(_digitFormat)).Append(",");
            //sb.Append(cell.OrgY.ToString(_digitFormat)).Append(",");
            //sb.Append(cell.RunX.ToString(_digitFormat)).Append(",");
            //sb.Append(cell.RunY.ToString(_digitFormat)).Append(",");
            //sb.Append(cell.RunAngle.ToString(_digitFormat)).Append(",");

            //sb.Append(cell.DisLeft.ToString(_digitFormat)).Append(",");
            //sb.Append(cell.DisRight.ToString(_digitFormat)).Append(",");
            //sb.Append(cell.DisTop.ToString(_digitFormat)).Append(",");
            //sb.Append(cell.DisBottom.ToString(_digitFormat)).Append(",");
            var gaps = cell?.ChipData?.PadEdgeGaps;

            sb.AppendValues(
                cell.RunWidth,
                cell.RunHeight,
                //cell.PadEdgeDiffX,
                //cell.PadEdgeDiffY,

                cell.OrgX,
                cell.OrgY,
                cell.RunX,
                cell.RunY,
                cell.RunAngle,

                //cell.PadEdgeSizes[(int)EdgeBorder.Left],        // 左 (報表順序)
                //cell.PadEdgeSizes[(int)EdgeBorder.Right],       // 右 (報表順序)
                //cell.PadEdgeSizes[(int)EdgeBorder.Top],         // 上 (報表順序)
                //cell.PadEdgeSizes[(int)EdgeBorder.Bottom]       // 下 (報表順序)

                gaps != null ? gaps.LU.X : 0f,
                gaps != null ? gaps.LU.Y : 0f,
                gaps != null ? gaps.RU.X : 0f,
                gaps != null ? gaps.RU.Y : 0f,
                gaps != null ? gaps.RD.X : 0f,
                gaps != null ? gaps.RD.Y : 0f,
                gaps != null ? gaps.LD.X : 0f,
                gaps != null ? gaps.LD.Y : 0f

            ).Append(",");

            sb.AppendPointF(cell.Sur1).Append(",");
            sb.AppendPointF(cell.Sur2).Append(",");
            sb.Append(cell.SetBarcodeStr).Append(",");

            //if (cell.RunCodeInfo != null)
            //    str += cell.RunCodeInfo.Content + ",";
            //else
            //    str += ",";
            //str += Environment.NewLine;

            if (cell.RunCodeInfo != null)
                sb.Append(cell.RunCodeInfo.Content).Append(",");
            else
                sb.Append(",");
            sb.AppendLine();
        }
        void saveReportData(StringBuilder reportSB)
        {
            //string path = $"{_resultImagePath}\\report\\{DateTime.Now.ToString("yyyyMMdd")}\\{_stripID}";
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