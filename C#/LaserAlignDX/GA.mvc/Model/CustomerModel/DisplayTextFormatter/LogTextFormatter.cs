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

using System.Collections.Generic;
using System.Text;
using Traveller106;

using CELL = LaserAlignDX.OPSpace.RegionCellX3Class;

namespace LaserAlignDX.Model
{
    public class LogTextFormatter : IxDispTextFormatter
    {
        #region CONFIG
        static string _digitFormat => StringBuilderExt.DIGIT_FORMAT;
        #endregion

        public string Format(CELL cell)
        {
            if (cell == null)
                return string.Empty;
            var sb = new StringBuilder();
            appendCell(sb, cell);
            return sb.ToString();
        }
        public string Format(IEnumerable<CELL> cells)
        {
            var sb = new StringBuilder();
            foreach (CELL cell in cells)
            {
                appendCell(sb, cell);
            }
            return sb.ToString();
        }

        #region PRIVATE_FUNCTIONS
        /// <summary>
        /// 舊代碼
        /// </summary>
        string ToResultStr(CELL cell)
        {
            string str = string.Empty;
#if(OPT_OLD)
            str += $"{cell.Index}" + ",";
            str += $"{cell.lblName}" + ",";
            str += $"{(cell.ByPass ? (INI.Instance.IsForceInspect ? "1强制检测" : "0不检测") : "1检测")}" + ",";
            str += $"[{cell.OrgX.ToString(_digitFormat)}" + ",";
            str += $"{cell.OrgY.ToString(_digitFormat)}]" + ",";
            str += $"{cell.RunX.ToString(_digitFormat)}" + ",";
            str += $"{cell.RunY.ToString(_digitFormat)}" + ",";
            str += $"{cell.RunAngle.ToString(_digitFormat)}" + ",";
            if (cell.xInspect.bOpenLineMeasure)
            {
                str += $"尺寸X[{cell.RunWidth.ToString(_digitFormat)}]" + ",";
                str += $"尺寸Y[{cell.RunHeight.ToString(_digitFormat)}]" + ",";
            }
            if (cell.xInspect.bCheckMeasureOffset)
            {
                str += $"X方向偏移[{cell.RunXOffset.ToString(_digitFormat)}]" + ",";
                str += $"Y方向偏移[{cell.RunYOffset.ToString(_digitFormat)}]" + ",";
            }
            str += $"{cell.SetBarcodeStr}" + ",";
            if (cell.RunCodeInfo != null)
                str += $"{cell.RunCodeInfo.Content}" + ";";
            else
                str += $"" + ";";

            //str += $"{Index}" + ",";
            //str += $"{lblName}" + ",";
            //str += $"{(ByPass ? "0不检测" : "1检测")}" + ",";
            //str += $"偏移x:{RunX}" + ",";
            //str += $"偏移y:{RunY}" + ",";
            //str += $"设定{SetBarcodeStr}" + ",";
            //if (RunCodeInfo != null)
            //    str += $"读取{RunCodeInfo.Content}" + ";";
            //else
            //    str += $"读取" + ";";

            //if (mvd2DReader.DCodeInfo != null)
            //    str += $"{mvd2DReader.DCodeInfo.Content}" + ";";
            //else
            //    str += $"" + ";";
#endif
            return str;
        }
        void appendCell(StringBuilder sb, CELL cell)
        {
            if (cell == null)
                return;

            //str += $"{cell.Index}" + ",";
            //str += $"{cell.lblName}" + ",";
            //str += $"{(cell.ByPass ? (INI.Instance.IsForceInspect ? "1强制检测" : "0不检测") : "1检测")}" + ",";
            sb.Append(cell.Index).Append(",");
            sb.Append(cell.lblName).Append(",");
            sb.Append((cell.ByPass ? (INI.Instance.IsForceInspect ? "1强制检测" : "0不检测") : "1检测")).Append(",");
            //str += $"[{cell.OrgX.ToString(_digitFormat)}" + ",";
            //str += $"{cell.OrgY.ToString(_digitFormat)}]" + ",";
            sb.Append("[").AppendValues(cell.OrgX, cell.OrgY).Append("],");
            //str += $"{cell.RunX.ToString(_digitFormat)}" + ",";
            //str += $"{cell.RunY.ToString(_digitFormat)}" + ",";
            //str += $"{cell.RunAngle.ToString(_digitFormat)}" + ",";
            sb.Append("").AppendValues(cell.RunX, cell.RunY, cell.RunAngle).Append(",");
            if (cell.xInspect.optChipMeasurement)
            {
                //str += $"尺寸X[{cell.RunWidth.ToString(_digitFormat)}]" + ",";
                //str += $"尺寸Y[{cell.RunHeight.ToString(_digitFormat)}]" + ",";
                sb.Append("尺寸X[").AppendValues(cell.RunWidth).Append("],");
                sb.Append("尺寸Y[").AppendValues(cell.RunHeight).Append("],");
            }

            //if (cell.xInspect.optPadEdgeGapsMeasurement)
            //{
            //    //var gaps = cell?.ChipData.PadEdgeGaps;
            //    //if (gaps != null)
            //    //{
            //    //    var gapL = gaps.GetGapSize(BasicSpace.EdgeBorder.Left);
            //    //    var gapR = gaps.GetGapSize(BasicSpace.EdgeBorder.Right);
            //    //}
            //    ////str += $"X方向偏移[{cell.RunXOffset.ToString(_digitFormat)}]" + ",";
            //    ////str += $"Y方向偏移[{cell.RunYOffset.ToString(_digitFormat)}]" + ",";
            //    //sb.Append("X方向偏移[").AppendValues(cell.PadEdgeDiffX).Append("],");
            //    //sb.Append("Y方向偏移[").AppendValues(cell.PadEdgeDiffY).Append("],");
            //}

            //str += $"{cell.SetBarcodeStr}" + ",";
            //if (cell.RunCodeInfo != null)
            //    str += $"{cell.RunCodeInfo.Content}" + ";";
            //else
            //    str += $"" + ";";

            //sb.Append(cell.BarcodeResultText).Append(",");
            //if (cell.RunCodeInfo != null)
            //    sb.Append(cell.RunCodeInfo.Content).Append(";");
            //else
            //    sb.Append(";");

            sb.Append(cell.BarcodeResultText).Append(",;");
        }
        #endregion
    }
}