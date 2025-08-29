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
using CELL = LaserAlignDX.OPSpace.RegionCellX3Class;


namespace LaserAlignDX.Model
{
    public class MainDispTextFormatter : IxDispTextFormatter
    {
        public string Format(CELL cell)
        {
            if (cell == null)
                return string.Empty;

            var sb = new StringBuilder();

            // 注意: MVD 顯示中文, 在 繁簡不同windows下, 會出現亂碼 !!!
            sb.Append("[").Append(cell.Index).Append("]");
            sb.Append(" (").AppendValues(cell.CellRow, cell.CellCol).AppendLine(")");
            sb.Append("Org= (").AppendValues(cell.OrgX, cell.OrgY).AppendLine(")");
            sb.Append("Run= (").AppendValues(cell.RunX, cell.RunY, cell.RunAngle).AppendLine(")");
            sb.Append("马达1= (").AppendPointF(cell.Sur1).AppendLine(")");
            sb.Append("马达2= (").AppendPointF(cell.Sur2).AppendLine(")");

            if (cell.xInspect.bOpenLineMeasure)
            {
                //str += $"尺寸宽度X[{RunWidth.ToString(m_Format)}mm]{Environment.NewLine}";
                //str += $"尺寸高度Y[{RunHeight.ToString(m_Format)}mm]{Environment.NewLine}";
                sb.Append("尺寸宽度X= ").AppendValues(cell.RunWidth).AppendLine(" mm");
                sb.Append("尺寸宽度Y= ").AppendValues(cell.RunHeight).AppendLine(" mm");
            }
            if (cell.xInspect.bCheckMeasureOffset)
            {
                //str += $"位置偏移X[{RunXOffset.ToString(m_Format)}mm]{Environment.NewLine}";
                //str += $"位置偏移Y[{RunYOffset.ToString(m_Format)}mm]{Environment.NewLine}";
                //str += $"左边距[{DisLeft.ToString(m_Format)}mm]{Environment.NewLine}";
                //str += $"右边距[{DisRight.ToString(m_Format)}mm]{Environment.NewLine}";
                //str += $"上边距[{DisTop.ToString(m_Format)}mm]{Environment.NewLine}";
                //str += $"下边距[{DisBottom.ToString(m_Format)}mm]{Environment.NewLine}";
                sb.Append("位置偏移X= ").AppendValues(cell.RunXOffset).AppendLine(" mm");
                sb.Append("位置偏移Y= ").AppendValues(cell.RunYOffset).AppendLine(" mm");
                sb.Append("左边距= ").AppendValues(cell.DisLeft).AppendLine(" mm");
                sb.Append("右边距= ").AppendValues(cell.DisRight).AppendLine(" mm");
                sb.Append("上边距= ").AppendValues(cell.DisTop).AppendLine(" mm");
                sb.Append("下边距= ").AppendValues(cell.DisBottom).AppendLine(" mm");
            }
            return sb.ToString();
        }
        public string Format(IEnumerable<CELL> cells)
        {
            return "";
        }

        #region PRIVATE_FUNCTIONS
        /// <summary>
        /// 舊代碼
        /// </summary>
        string ToShowMainStr()
        {
            string str = string.Empty;

            //str += $"{Index}" + "";
            //str += $"[{OrgX.ToString(m_Format)}" + ",";
            //str += $"{OrgY.ToString(m_Format)}]{Environment.NewLine}";
            //str += $"[{RunX.ToString(m_Format)}" + ",";
            //str += $"{RunY.ToString(m_Format)}" + ",";
            //str += $"{RunAngle.ToString(m_Format)}]{Environment.NewLine}";
            //str += $"马达1=[{PointF000ToString(Sur1)}]{Environment.NewLine}";
            //str += $"马达2=[{PointF000ToString(Sur2)}]{Environment.NewLine}";
            //if (xInspect.bOpenLineMeasure)
            //{
            //    str += $"尺寸宽度X[{RunWidth.ToString(m_Format)}mm]{Environment.NewLine}";
            //    str += $"尺寸高度Y[{RunHeight.ToString(m_Format)}mm]{Environment.NewLine}";
            //}
            //if (xInspect.bCheckMeasureOffset)
            //{
            //    str += $"位置偏移X[{RunXOffset.ToString(m_Format)}mm]{Environment.NewLine}";
            //    str += $"位置偏移Y[{RunYOffset.ToString(m_Format)}mm]{Environment.NewLine}";
            //    str += $"左边距[{DisLeft.ToString(m_Format)}mm]{Environment.NewLine}";
            //    str += $"右边距[{DisRight.ToString(m_Format)}mm]{Environment.NewLine}";
            //    str += $"上边距[{DisTop.ToString(m_Format)}mm]{Environment.NewLine}";
            //    str += $"下边距[{DisBottom.ToString(m_Format)}mm]{Environment.NewLine}";
            //}

            ////str += Environment.NewLine;

            ////str += $"ORG" + "-[";
            ////str += $"{OrgX.ToString(m_Format)}" + ",";
            ////str += $"{OrgY.ToString(m_Format)}" + ",";
            ////str += $"{OrgAngle.ToString(m_Format)}]";
            
            return str;
        }
        #endregion
    }
}