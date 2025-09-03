using JetEazy.BasicSpace;
using System;
using System.Drawing;
using System.Reflection;


namespace JetEazy.Utils
{
    public class GaUtil
    {
        // 動詞使用 Clip 比 Bound 合適
        public static void ClipRect(ref Rectangle InnerRect, Size BoundSize)
        {
            BoundRect(ref  InnerRect, BoundSize);
        }
        public static void ClipRect(ref RectangleF InnerRect, Size BoundSize)
        {
            BoundRect(ref InnerRect, BoundSize);
        }
        public static int ClipValue(int Value, int Max, int Min)
        {
            return Math.Max(Math.Min(Value, Max), Min);

        }
        public static float ClipValue(float Value, float Max, float Min)
        {
            return Math.Max(Math.Min(Value, Max), Min);

        }

        public static void BoundRect(ref Rectangle InnerRect, Size BoundSize)
        {
            InnerRect.X = Math.Min(Math.Max(InnerRect.X, 0), (BoundSize.Width - InnerRect.Width < 0 ? 0 : BoundSize.Width - InnerRect.Width));
            InnerRect.Y = Math.Min(Math.Max(InnerRect.Y, 0), (BoundSize.Height - InnerRect.Height < 0 ? 0 : BoundSize.Height - InnerRect.Height));

            if (BoundSize.Width <= InnerRect.X + InnerRect.Width)
                InnerRect.Width = BoundValue(InnerRect.Width, BoundSize.Width - InnerRect.X, 1);
            if (BoundSize.Height <= InnerRect.Height + InnerRect.Height)
                InnerRect.Height = BoundValue(InnerRect.Height, BoundSize.Height - InnerRect.Y, 1);
        }
        public static void BoundRect(ref RectangleF InnerRect, Size BoundSize)
        {
            InnerRect.X = Math.Min(Math.Max(InnerRect.X, 0), (BoundSize.Width - InnerRect.Width < 0 ? 0 : BoundSize.Width - InnerRect.Width));
            InnerRect.Y = Math.Min(Math.Max(InnerRect.Y, 0), (BoundSize.Height - InnerRect.Height < 0 ? 0 : BoundSize.Height - InnerRect.Height));

            if (BoundSize.Width <= InnerRect.X + InnerRect.Width)
                InnerRect.Width = BoundValue(InnerRect.Width, BoundSize.Width - InnerRect.X, 1);
            if (BoundSize.Height <= InnerRect.Height + InnerRect.Height)
                InnerRect.Height = BoundValue(InnerRect.Height, BoundSize.Height - InnerRect.Y, 1);
        }
        public static int BoundValue(int Value, int Max, int Min)
        {
            return Math.Max(Math.Min(Value, Max), Min);

        }
        public static float BoundValue(float Value, float Max, float Min)
        {
            return Math.Max(Math.Min(Value, Max), Min);

        }
        
        public static void SaveData(string DataStr, string fileName)
        {
            System.IO.StreamWriter stm = null;

            try
            {
                stm = new System.IO.StreamWriter(fileName, false, System.Text.Encoding.Default);
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

        /// <summary>
        /// 通用的 LOG function
        /// </summary>
        public static void LOG(string msg, params object[] args)
        {
#if (true)
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
#endif
        }
    }
}
