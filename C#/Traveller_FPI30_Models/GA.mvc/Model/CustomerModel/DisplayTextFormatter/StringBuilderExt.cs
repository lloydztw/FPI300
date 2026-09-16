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

using System.Drawing;
using System.Text;


namespace LaserAlignDX
{
    /// <summary>
    /// StringBuilder 擴增函式
    /// </summary>
    public static class StringBuilderExt
    {
        public const string DIGIT_FORMAT = "0.000";
        public static StringBuilder AppendValues(this StringBuilder sb, params double[] values)
        {
            for (int i = 0, N = values.Length; i < N; i++)
            {
                sb.Append(values[i].ToString(DIGIT_FORMAT));
                if (i < N - 1)
                    sb.Append(", ");
            }
            return sb;
        }
        public static StringBuilder AppendValues(this StringBuilder sb, params float[] values)
        {
            for (int i = 0, N = values.Length; i < N; i++)
            {
                sb.Append(values[i].ToString(DIGIT_FORMAT));
                if (i < N - 1)
                    sb.Append(", ");
            }
            return sb;
        }
        public static StringBuilder AppendValues(this StringBuilder sb, params int[] values)
        {
            for (int i = 0, N = values.Length; i < N; i++)
            {
                sb.Append(values[i]);
                if (i < N - 1)
                    sb.Append(", ");
            }
            return sb;
        }
        public static StringBuilder AppendPointF(this StringBuilder sb, PointF pt)
        {
            sb.AppendValues(pt.X, pt.Y);
            return sb;
        }
    }
}