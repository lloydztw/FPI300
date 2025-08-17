using System;
using System.Drawing;


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
    }

}
