#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-08-13 ªì½Z (by LeTian Chang)
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
    public static class QTransform_Ini
    {
        public static void LoadIni(this QTransform trf, string iniFileName, string sectName = null)
        {
            if (sectName == null)
                sectName = trf.Name;

            int rows = 2;   
            int cols = 2;
            JetEazy.Win32.Win32Ini.Load(ref rows, iniFileName, sectName, "KP_ROWS");
            JetEazy.Win32.Win32Ini.Load(ref cols, iniFileName, sectName, "KP_COLS");

            if (rows >= 2 && cols >= 2)
            {
                if (rows > trf._srcPoints.GetLength(0) || cols > trf._srcPoints.GetLength(1))
                {
                    trf._srcPoints = new QVector[rows, cols];
                    trf._dstPoints = new QVector[rows, cols];
                }

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
                        trf._srcPoints[r, c] = ok ? src : null;
                        trf._dstPoints[r, c] = ok ? dst : null;
                    }
                }
            }

            trf._mat = Mat.Eye(3, 3, MatType.CV_64FC1);
            trf._matInv = Mat.Eye(3, 3, MatType.CV_64FC1);

            _load(trf._mat, iniFileName, sectName + "_MAT");
            _load(trf._matInv, iniFileName, sectName + "_MAT_INV");
        }
        public static void SaveIni(this QTransform trf, string iniFileName, string sectName = null)
        {
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

            _save(trf._mat, iniFileName, sectName + "_MAT");
            _save(trf._matInv, iniFileName, sectName + "_MAT_INV");
        }

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
    }

    public static class QVector_Ini
    {
        public static bool LoadIni(this QVector v, string iniFileName, string sectName, string keyName)
        {
            string str = "";
            JetEazy.Win32.Win32Ini.Load(ref str, iniFileName, sectName, keyName);
            if (string.IsNullOrEmpty(str))
                return false;

            var strs = str.Split(',');
            int nDim = 0;
            int i = 0;
            if (strs.Length > i) strs[i++].Trim();
            if (strs.Length > i) if (int.TryParse(strs[i++].Trim(), out int len)) { }
            if (strs.Length > i) if (double.TryParse(strs[i++].Trim(), out double vx)) { v.X = vx; nDim++; }
            if (strs.Length > i) if (double.TryParse(strs[i++].Trim(), out double vy)) { v.Y = vy; nDim++; }

            return nDim >= 2;
        }
        public static void SaveIni(this QVector v, string iniFileName, string sectName, string keyName, bool fixDigit = false)
        {
            if (v != null)
            {
                string str = fixDigit ? 
                            $"QVector, {v.Length}, {v.X:0.000000}, {v.Y:0.000000}" :
                            $"QVector, {v.Length}, {v.X}, {v.Y}";
                JetEazy.Win32.Win32Ini.Save(str, iniFileName, sectName, keyName);
            }
            else
            {
                JetEazy.Win32.Win32Ini.Save("", iniFileName, sectName, keyName);
            }
        }
    }
}
