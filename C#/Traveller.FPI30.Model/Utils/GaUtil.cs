#region AUTHOR
/*
 *
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 *
 * REVISION:
 *      2026-10-08 使用 ChatGPT Codex  重整 (by LeTian Chang)
 *      2025-08-11 重新整理 (by LeTian Chang)
 *
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 *
 */
#endregion

using JetEazy.Lang;
using System;
using System.Drawing;

namespace JetEazy.Utils
{
    public class GaClipUtil
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
            return QMSG.Text(enumSubitem);
        }
    }
}
