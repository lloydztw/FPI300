using System;
using System.Drawing;
using Traveller106;

namespace LaserAlignDX.OPSpace
{
    /// <summary>
    /// 為了相容以前舊版程式, 在此為 RegionCellXClass 外掛 文字格式化 函式
    /// 不要在此生成_客戶要求的_顯示與報表_字串格式_不然換不同廠家就要跟著一直變動_CELL
    /// 優化後的架構, 已將這些搬移到
    /// GA.mvc\Model\CustomerModel\DisplayTextFormatter 下面
    /// </summary>
    public static class CellFormat
    {
        static string m_Format => "0.000";
        public static string ToShowMainStr(this RegionCellX3Class cell)
        {
            string PointF000ToString(PointF pt)
            {
                return pt.X.ToString(m_Format) + "," + pt.Y.ToString(m_Format);
            }
            ;

            string str = string.Empty;

            str += $"{cell.Index}" + "";
            str += $"[{cell.OrgX.ToString(m_Format)}" + ",";
            str += $"{cell.OrgY.ToString(m_Format)}]{Environment.NewLine}";
            str += $"[{cell.RunX.ToString(m_Format)}" + ",";
            str += $"{cell.RunY.ToString(m_Format)}" + ",";
            str += $"{cell.RunAngle.ToString(m_Format)}]{Environment.NewLine}";
            str += $"马达1=[{PointF000ToString(cell.Sur1)}]{Environment.NewLine}";
            str += $"马达2=[{PointF000ToString(cell.Sur2)}]{Environment.NewLine}";
            if (cell.xInspect.bOpenLineMeasure)
            {
                str += $"尺寸宽度X[{cell.RunWidth.ToString(m_Format)}mm]{Environment.NewLine}";
                str += $"尺寸高度Y[{cell.RunHeight.ToString(m_Format)}mm]{Environment.NewLine}";
            }
            if (cell.xInspect.bCheckMeasureOffset)
            {
                str += $"位置偏移X[{cell.RunXOffset.ToString(m_Format)}mm]{Environment.NewLine}";
                str += $"位置偏移Y[{cell.RunYOffset.ToString(m_Format)}mm]{Environment.NewLine}";
                str += $"左边距[{cell.DisLeft.ToString(m_Format)}mm]{Environment.NewLine}";
                str += $"右边距[{cell.DisRight.ToString(m_Format)}mm]{Environment.NewLine}";
                str += $"上边距[{cell.DisTop.ToString(m_Format)}mm]{Environment.NewLine}";
                str += $"下边距[{cell.DisBottom.ToString(m_Format)}mm]{Environment.NewLine}";
            }

            //str += Environment.NewLine;

            //str += $"ORG" + "-[";
            //str += $"{OrgX.ToString(m_Format)}" + ",";
            //str += $"{OrgY.ToString(m_Format)}" + ",";
            //str += $"{OrgAngle.ToString(m_Format)}]";

            return str;
        }
        public static string ToResultStr(this RegionCellX3Class cell)
        {
            string str = string.Empty;
            str += $"{cell.Index}" + ",";
            str += $"{cell.lblName}" + ",";
            str += $"{(cell.ByPass ? (INI.Instance.IsForceInspect ? "1强制检测" : "0不检测") : "1检测")}" + ",";
            str += $"[{cell.OrgX.ToString(m_Format)}" + ",";
            str += $"{cell.OrgY.ToString(m_Format)}]" + ",";
            str += $"{cell.RunX.ToString(m_Format)}" + ",";
            str += $"{cell.RunY.ToString(m_Format)}" + ",";
            str += $"{cell.RunAngle.ToString(m_Format)}" + ",";
            if (cell.xInspect.bOpenLineMeasure)
            {
                str += $"宽度[{cell.RunWidth.ToString(m_Format)}]" + ",";
                str += $"高度[{cell.RunHeight.ToString(m_Format)}]" + ",";
            }
            if (cell.xInspect.bCheckMeasureOffset)
            {
                str += $"X方向偏移[{cell.RunXOffset.ToString(m_Format)}]" + ",";
                str += $"Y方向偏移[{cell.RunYOffset.ToString(m_Format)}]" + ",";
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
            return str;
        }
        public static string ToReport1Str(this RegionCellX3Class cell)
        {
            string str = string.Empty;
            str += $"{cell.Index}" + ",";
            str += $"{cell.lblName}" + ",";
            str += $"{(cell.ByPass ? (INI.Instance.IsForceInspect ? "1强制检测" : "0不检测") : "1检测")}" + ",";
            str += $"{cell.RunWidth.ToString(m_Format)}" + ",";
            str += $"{cell.RunHeight.ToString(m_Format)}" + ",";
            str += $"{cell.RunXOffset.ToString(m_Format)}" + ",";
            str += $"{cell.RunYOffset.ToString(m_Format)}" + ",";
            str += $"{cell.OrgX.ToString(m_Format)}" + ",";
            str += $"{cell.OrgY.ToString(m_Format)}" + ",";
            str += $"{cell.RunX.ToString(m_Format)}" + ",";
            str += $"{cell.RunY.ToString(m_Format)}" + ",";
            str += $"{cell.RunAngle.ToString(m_Format)}" + ",";

            str += $"{cell.DisLeft.ToString(m_Format)}" + ",";
            str += $"{cell.DisRight.ToString(m_Format)}" + ",";
            str += $"{cell.DisTop.ToString(m_Format)}" + ",";
            str += $"{cell.DisBottom.ToString(m_Format)}" + ",";

            string format(PointF pt)
            {
                //return $"{pt.X:0.000},{pt.Y:0.000}";
                return pt.X.ToString(m_Format) + "," + pt.Y.ToString(m_Format);
            }
            ;

            str += format(cell.Sur1) + ",";
            str += format(cell.Sur2) + ",";
            str += cell.SetBarcodeStr + ",";
            if (cell.RunCodeInfo != null)
                str += cell.RunCodeInfo.Content + ",";
            else
                str += ",";
            str += Environment.NewLine;
            return str;
        }
    }
}
