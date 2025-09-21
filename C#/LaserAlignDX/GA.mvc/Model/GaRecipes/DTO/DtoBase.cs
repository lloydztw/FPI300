#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-09-20 重整 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace LaserAlignDX.Mvc.Model.Recipe
{
    /// <summary>
    /// DTO (Data Transfer Object) 類別
    /// DTO 是標準用詞 請自行查 ChatGPT or DeepSeek
    /// </summary>
    public abstract class DtoBase
    {
        #region PRIVATE_MEMBERS
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        #endregion

        #region INI_FUNCTIONS
        protected static void WriteINIValue(string section, string key, string value, string filepath)
        {
            WritePrivateProfileString(section, key, value, filepath);
        }
        protected static string ReadINIValue(string section, string key, string defaultvaluestring, string filepath)
        {
            string retStr = "";

            StringBuilder temp = new StringBuilder(4096);
            int Length = GetPrivateProfileString(section, key, "", temp, 4096, filepath);

            retStr = temp.ToString();

            if (retStr == "")
                retStr = defaultvaluestring;
            //else
            //    retStr = retStr.Split('/')[0]; //把說明排除掉

            return retStr;
        }
        #endregion

        #region HELPER_FUNCTIONS
        protected string RecttoStringSimple(Rectangle Rect)
        {
            return Rect.X.ToString() + "," + Rect.Y.ToString() + "," + Rect.Width.ToString() + "," + Rect.Height.ToString();
        }
        protected Rectangle StringtoRect(string RectStr)
        {
            string[] str = RectStr.Split(',');
            return new Rectangle(int.Parse(str[0]), int.Parse(str[1]), int.Parse(str[2]), int.Parse(str[3]));
        }
        protected string RectFtoStringSimple(RectangleF RectF)
        {
            string Str = "";

            Str += RectF.X.ToString() + ",";
            Str += RectF.Y.ToString() + ",";
            Str += RectF.Width.ToString() + ",";
            Str += RectF.Height.ToString();

            return Str;
        }
        protected RectangleF StringtoRectF(string RectStr)
        {
            string[] strs = RectStr.Split(',');
            RectangleF rectF = new RectangleF();

            rectF.X = float.Parse(strs[0]);
            rectF.Y = float.Parse(strs[1]);
            rectF.Width = float.Parse(strs[2]);
            rectF.Height = float.Parse(strs[3]);

            return rectF;


        }
        protected string PointFtoStringSimple(PointF ptf)
        {
            string Str = "";

            Str += ptf.X.ToString() + ",";
            Str += ptf.Y.ToString();

            return Str;
        }
        protected PointF StringtoPointF(string ptfStr)
        {
            string[] strs = ptfStr.Split(',');
            PointF rectF = new PointF();

            rectF.X = float.Parse(strs[0]);
            rectF.Y = float.Parse(strs[1]);

            return rectF;


        }
        #endregion

        public virtual void Load(string iniFileName)
        {
        }
        public virtual void Save(string iniFileName)
        {
        }
    }
}
