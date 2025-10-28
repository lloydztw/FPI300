#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-08-11 重新整理 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.BasicSpace;
using LeTian.AoiLib;
using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;


namespace JetEazy.Utils
{
    public class GaUtil
    {
        // 動詞使用 Clip 比 Bound 合適
        public static bool Clip(ref Rectangle rect, int boundWidth, int boundHeight)
        {
            bool isClipped = false;
            if (rect.X < 0 || rect.Y < 0 || rect.Right > boundWidth || rect.Bottom > boundHeight)
            {
                isClipped = true;
                int x = Math.Max(rect.X, 0);
                int y = Math.Max(rect.Y, 0);
                int x2 = Math.Min(rect.Right, boundWidth);
                int y2 = Math.Min(rect.Bottom, boundHeight);
                rect.X = x;
                rect.Y = y;
                rect.Width = x2 - x;
                rect.Height = y2 - y;
            }
            return isClipped;
        }
        public static bool Clip(ref RectangleF rect, int boundWidth, int boundHeight)
        {
            bool isClipped = false;
            if (rect.X < 0 || rect.Y < 0 || rect.Right > boundWidth || rect.Bottom > boundHeight)
            {
                isClipped = true;
                var x = Math.Max(rect.X, 0);
                var y = Math.Max(rect.Y, 0);
                var x2 = Math.Min(rect.Right, boundWidth);
                var y2 = Math.Min(rect.Bottom, boundHeight);
                rect.X = x;
                rect.Y = y;
                rect.Width = x2 - x;
                rect.Height = y2 - y;
            }
            return isClipped;
        }
        public static bool Clip(ref OpenCvSharp.Rect rect, int boundWidth, int boundHeight)
        {
            bool isClipped = false;
            if (rect.X < 0 || rect.Y < 0 || rect.Right > boundWidth || rect.Bottom > boundHeight)
            {
                isClipped = true;
                var x = Math.Max(rect.X, 0);
                var y = Math.Max(rect.Y, 0);
                var x2 = Math.Min(rect.Right, boundWidth);
                var y2 = Math.Min(rect.Bottom, boundHeight);
                rect.X = x;
                rect.Y = y;
                rect.Width = x2 - x;
                rect.Height = y2 - y;
            }
            return isClipped;
        }

        public static bool Clip(ref Rectangle rect, Size boundSize)
        {
            return Clip(ref rect, boundSize.Width, boundSize.Height);
        }
        public static bool Clip(ref RectangleF rect, Size boundSize)
        {
            return Clip(ref rect, boundSize.Width, boundSize.Height);
        }
        public static bool Clip(ref PointF pt, ref RectangleF rect)
        {
            bool clipped = false;
            if(pt.X < rect.Left)
            {
                pt.X = rect.Left;
                clipped = true;
            }
            if (pt.X > rect.Right - 1)
            {
                pt.X = rect.Right - 1;
                clipped = true;
            }
            if(pt.Y < rect.Top)
            {
                pt.Y = rect.Top;
                clipped = true;
            }
            if (pt.Y > rect.Bottom - 1)
            {
                pt.X -= rect.Bottom - 1;
                clipped = true;
            }
            return clipped;
        }

        public static void BoundRect(ref Rectangle rect, Size boundSize)
        {
            //rect.X = Math.Min(Math.Max(rect.X, 0), (boundSize.Width - rect.Width < 0 ? 0 : boundSize.Width - rect.Width));
            //rect.Y = Math.Min(Math.Max(rect.Y, 0), (boundSize.Height - rect.Height < 0 ? 0 : boundSize.Height - rect.Height));
            //if (boundSize.Width <= rect.X + rect.Width)
            //    rect.Width = BoundValue(rect.Width, boundSize.Width - rect.X, 1);
            //if (boundSize.Height <= rect.Height + rect.Height)
            //    rect.Height = BoundValue(rect.Height, boundSize.Height - rect.Y, 1);
            Clip(ref rect, boundSize);
        }
        public static void BoundRect(ref RectangleF rect, Size boundSize)
        {
            //rect.X = Math.Min(Math.Max(rect.X, 0), (boundSize.Width - rect.Width < 0 ? 0 : boundSize.Width - rect.Width));
            //rect.Y = Math.Min(Math.Max(rect.Y, 0), (boundSize.Height - rect.Height < 0 ? 0 : boundSize.Height - rect.Height));

            //if (boundSize.Width <= rect.X + rect.Width)
            //    rect.Width = BoundValue(rect.Width, boundSize.Width - rect.X, 1);
            //if (boundSize.Height <= rect.Height + rect.Height)
            //    rect.Height = BoundValue(rect.Height, boundSize.Height - rect.Y, 1);
            Clip(ref rect, boundSize);
        }

#if (OPT_LEGACY)
        public static int BoundValue(int Value, int Max, int Min)
        {
            return Math.Max(Math.Min(Value, Max), Min);

        }
        public static float BoundValue(float Value, float Max, float Min)
        {
            return Math.Max(Math.Min(Value, Max), Min);
        }
#endif

        public static void SaveData(string DataStr, string fileName)
        {
            System.IO.StreamWriter stm = null;

            try
            {
                stm = new System.IO.StreamWriter(fileName, false, System.Text.Encoding.UTF8);
                stm.WriteLine(DataStr);
                stm.Flush();
                stm.Close();
                stm.Dispose();
                stm = null;
            }
            catch (Exception ex)
            {
                //JetEazy.LoggerClass.Instance.WriteException(ex);
            }

            if (stm != null)
                stm.Dispose();
        }

        /// <summary>
        /// 获取枚举类子项描述信息
        /// </summary>
        /// <param name="enumSubitem">枚举类子项</param>        
        public static string GetEnumDescription(Enum enumSubitem)
        {
            string strValue = enumSubitem.ToString();

            FieldInfo fieldinfo = enumSubitem.GetType().GetField(strValue);
            Object[] objs = fieldinfo.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);
            if (objs == null || objs.Length == 0)
            {
                return strValue;
            }
            else
            {
                System.ComponentModel.DescriptionAttribute da = (System.ComponentModel.DescriptionAttribute)objs[0];
                return da.Description;
            }
        }

        public static string BrowseImageFile()
        {
            string fileName = null;
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Select Image";
                dlg.Filter = "JPG Files(*.jpg)|*.jpg|BMP Files(*.bmp)|*.bmp|PNG Files(*.png)|*.png";
                dlg.FileName = "*.jpg";

                if (!string.IsNullOrEmpty(fileName))
                {
                    try
                    {
                        dlg.InitialDirectory = System.IO.Path.GetDirectoryName(fileName);
                    }
                    catch
                    {

                    }
                }

                if (DialogResult.OK == dlg.ShowDialog())
                {
                    //ResetAndClear();
                    fileName = dlg.FileName;
                }
                else
                {
                    fileName = null;
                }
            }
            return fileName;
        }

        public static string BrowseFolder(string defaultPath = null)
        {
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                folderBrowserDialog.Description = "Choose a folder (選擇路徑)";
                folderBrowserDialog.ShowNewFolderButton = true;

                if (!string.IsNullOrEmpty(defaultPath))
                    folderBrowserDialog.SelectedPath = defaultPath;

                if (DialogResult.OK == folderBrowserDialog.ShowDialog())
                {
                    string folder = folderBrowserDialog.SelectedPath;
                    if (!string.IsNullOrEmpty(folder))
                        return folder;
                }

            }
            return defaultPath;
        }

        public static Cursor SetCursor(Control wnd, Cursor cursor)
        {
            var frmOwner = wnd?.FindForm();
            if (frmOwner != null)
            {
                var old = frmOwner.Cursor;
                if (old != cursor)
                {
                    frmOwner.Cursor = cursor;
                    frmOwner.Invalidate();
                    //frmOwner.Refresh();
                }
                return old;
            }
            return Cursors.Default;
        }

        public static void SetNum(NumericUpDown num, decimal value)
        {
            if (num != null)
            {
                if (value > num.Maximum)
                    value = num.Maximum;
                else if (value < num.Minimum)
                    value = num.Minimum;
                else { }
                num.Value = value;
            }
        }

        /// <summary>
        /// 通用的 LOG function
        /// </summary>
        public static void LOG(string msg, params object[] args)
        {
            Color color = Color.Black;

            int N = args.Length;
            if (N > 0 && args[N - 1] is Color)
            {
                color = (Color)args[N - 1];
                N -= 1;
            }

            var sb = new System.Text.StringBuilder();
            //sb.Append(Name);
            sb.Append(", ");
            sb.Append(msg);

            for (int i = 0; i < N; i++)
            {
                sb.Append(", ");
                sb.Append(args[i]);
            }

            msg = sb.ToString();
            CommonLogClass.Instance.LogMessage(msg, color);
            //if (color == Color.Red)
            //    GdxGlobal.LOG.Warn(msg);
            //else
            //    GdxGlobal.LOG.Debug(msg);
        }
        public static void LOG_ERROR(Exception ex, string msg)
        {
            LtDebug.LOG.Error(ex, msg);
            string errMsg = $"[異常] {msg}\n\r\t{ex.Message}";
            CommonLogClass.Instance.LogMessage(errMsg, Color.Red);
        }
    }
}
