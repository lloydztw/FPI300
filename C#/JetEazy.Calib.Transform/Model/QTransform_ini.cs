#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-08-13 初稿 (by LeTian Chang)
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
    partial class QTransform
    {
        public void LoadIni(string iniFileName, string sectName = null)
        {
            var trf = this;
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

            // 只須載入 關鍵點資訊，轉換矩陣可由關鍵點計算得出，因此不須保存轉換矩陣資訊
            //_load(trf._mat, iniFileName, sectName + "_MAT");
            //_load(trf._matInv, iniFileName, sectName + "_MAT_INV");
            
            LoadIni_PostChain(iniFileName);
        }
        public void SaveIni(string iniFileName, string sectName = null)
        {
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

            // 只須保存 關鍵點資訊，轉換矩陣可由關鍵點計算得出，因此不須保存轉換矩陣資訊
            //_save(trf._mat, iniFileName, sectName + "_MAT");
            //_save(trf._matInv, iniFileName, sectName + "_MAT_INV");

            // PostChain
            SaveIni_PostChain(iniFileName);
        }

        void LoadIni_PostChain(string iniFileName)
        {
            PostChain?.Dispose();
            PostChain = null;

            string sectName = $"{Name}_PostChain";
            int rows = 0;
            int cols = 0;
            JetEazy.Win32.Win32Ini.Load(ref rows, iniFileName, sectName, "KP_ROWS");
            JetEazy.Win32.Win32Ini.Load(ref cols, iniFileName, sectName, "KP_COLS");

            if (rows < 2 || cols < 2)
                return;

            PostChain = new QTransform(sectName);
            PostChain.LoadIni(iniFileName, sectName);
        }
        void SaveIni_PostChain(string iniFileName)
        {
            if (PostChain != null)
            {
                PostChain.Name = $"{Name}_PostChain";
                PostChain.SaveIni(iniFileName, PostChain.Name);
            }
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
        public static void SaveIni(this QVector v, string iniFileName, string sectName, string keyName, int decimalPlaces=-1)
        {
            if (v != null)
            {
                string str;

                if (decimalPlaces >= 0)
                {
                    // 動態產生格式字串，例如 decimalPlaces 為 2 時，format 為 "F2"
                    string format = "F" + decimalPlaces;
                    // 使用 CultureInfo.InvariantCulture 確保小數點始終為 '.'
                    str = $"QVector, {v.Length}, {v.X.ToString(format, System.Globalization.CultureInfo.InvariantCulture)}, {v.Y.ToString(format, System.Globalization.CultureInfo.InvariantCulture)}";
                }
                else
                {
                    // 預設輸出（不限制位數）
                    str = $"QVector, {v.Length}, {v.X}, {v.Y}";
                }

                JetEazy.Win32.Win32Ini.Save(str, iniFileName, sectName, keyName);
            }
            else
            {
                JetEazy.Win32.Win32Ini.Save("", iniFileName, sectName, keyName);
            }
        }
    }
}
