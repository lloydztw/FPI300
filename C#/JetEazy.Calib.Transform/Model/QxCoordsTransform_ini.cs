#region AUTHOR
/*
 * 
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2026-04-02 優化 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.QMath;
using OpenCvSharp;


namespace JetEazy.Transform
{
    public partial class QxCoordsTransform
    {
        public void LoadIni(string iniFileName, string sectName = null)
        {
            // 只載入 KP_ROWS, KP_COLS, SRC_KP_{r}_{c}, DST_KP_{r}_{c} 這些關鍵點資訊

            var trf = this;
            if (sectName == null)
                sectName = trf.Name;

            int rows = 2;
            int cols = 2;
            JetEazy.Win32.Win32Ini.Load(ref rows, iniFileName, sectName, "KP_ROWS");
            JetEazy.Win32.Win32Ini.Load(ref cols, iniFileName, sectName, "KP_COLS");

            if (rows >= 2 && cols >= 2)
            {
                trf._srcPoints = new QVector[rows, cols];
                trf._dstPoints = new QVector[rows, cols];

                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < cols; c++)
                    {
                        //trf._srcPoints[r, c] = new QVector((double)r, (double)c);
                        //trf._dstPoints[r, c] = new QVector((double)r, (double)c);
                        //trf._srcPoints[r, c].LoadIni(iniFileName, sectName, $"SRC_KP_{r}_{c}");
                        //trf._dstPoints[r, c].LoadIni(iniFileName, sectName, $"DST_KP_{r}_{c}");

                        var src = new QVector((double)r, (double)c);
                        var dst = new QVector(src);
                        bool ok = src.LoadIni(iniFileName, sectName, $"SRC_KP_{r}_{c}");
                        ok &= dst.LoadIni(iniFileName, sectName, $"DST_KP_{r}_{c}");
                        if (ok)
                        {
                            trf._srcPoints[r, c] = src;
                            trf._dstPoints[r, c] = dst;
                        }
                        else
                        {
                            trf._srcPoints[r, c] = new QVector2(c, r) * 0.1;    // adjust fill a small point
                            trf._dstPoints[r, c] = new QVector2(c, r) * 0.1;    // adjust fill a small point
                        }
                    }
                }
            }
        }
        public void SaveIni(string iniFileName, string sectName = null)
        {
            // 只保存 KP_ROWS, KP_COLS, SRC_KP_{r}_{c}, DST_KP_{r}_{c} 這些關鍵點資訊

            var trf = this;
            if (sectName == null)
                sectName = trf.Name;

            int rows = trf._srcPoints.GetLength(0);
            int cols = trf._srcPoints.GetLength(1);
            JetEazy.Win32.Win32Ini.Save(rows, iniFileName, sectName, "KP_ROWS");
            JetEazy.Win32.Win32Ini.Save(cols, iniFileName, sectName, "KP_COLS");
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    trf._srcPoints[r, c].SaveIni(iniFileName, sectName, $"SRC_KP_{r}_{c}");
                    trf._dstPoints[r, c].SaveIni(iniFileName, sectName, $"DST_KP_{r}_{c}");
                }
            }

            //_save(trf._mat, iniFileName, sectName + "_MAT");
            //_save(trf._matInv, iniFileName, sectName + "_MAT_INV");
        }

        #region PRIVATE_INI_FILE_FUNCTIONS
        private static void _load(Mat mx, string iniFileName, string sectName)
        {
            int iRows = mx.Rows;
            int iCols = mx.Cols;
            for (int i = 0; i < iRows; i++)
            {
                for (int j = 0; j < iCols; j++)
                {
                    double value = 0;
                    _loadItem(ref value, iniFileName, sectName, "M", i, j);
                    mx.At<double>(i, j) = (double)value;
                }
            }
        }
        private static void _save(Mat mx, string iniFileName, string sectName)
        {
            int iRows = mx.Rows;
            int iCols = mx.Cols;
            for (int i = 0; i < iRows; i++)
            {
                for (int j = 0; j < iCols; j++)
                {
                    double value = mx.At<double>(i, j);
                    _saveItem(value, iniFileName, sectName, "M", i, j);
                }
            }
        }
        private static void _loadItem(ref double value, string iniFileName, string sectName, string keyName, int iRow, int iCol)
        {
            string strKeyA = keyName + "_" + iRow + "_" + iCol;
            string strValue = "";
            JetEazy.Win32.Win32Ini.Load(ref strValue, iniFileName, sectName, strKeyA);
            double.TryParse(strValue, out value);
        }
        private static void _saveItem<T>(T value, string iniFileName, string sectName, string keyName, int iRow, int iCol)
        {
            string strKeyA = keyName + "_" + iRow + "_" + iCol;
            string strValue = value.ToString();
            JetEazy.Win32.Win32Ini.Save(strValue, iniFileName, sectName, strKeyA);
        }
        #endregion
    }
}
