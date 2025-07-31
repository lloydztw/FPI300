using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Common.RecipeSpace
{
    public interface IRecipe
    {
        void Load(bool eCancel = false);
        void Save();
    }

    public class RecipeBaseClass : IRecipe
    {
        private string m_path = "";
        private string m_name = "";
        private string m_format = "0.000000";
        private int m_index = 0;

        [Browsable(false)]
        public string Path
        {
            get { return m_path; }
            //set { m_path = value; }
        }
        [Browsable(false)]
        public string PathIndexStr
        {
            get { return $"{m_path}\\{IndexStr}"; }
            //set { m_path = value; }
        }
        [Browsable(false)]
        public string Name
        {
            get { return m_name; }
            //set { m_index = value; }
        }
        [Browsable(false)]
        public string Format
        {
            get { return m_format; }
            //set { m_index = value; }
        }
        [Browsable(false)]
        public int Index
        {
            get { return m_index; }
            //set { m_index = value; }
        }
        #region INI Access Functions
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        public void WriteINIValue(string section, string key, string value, string filepath)
        {
            WritePrivateProfileString(section, key, value, filepath);
        }
        public string ReadINIValue(string section, string key, string defaultvaluestring, string filepath)
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

        [Browsable(false)]
        public string IndexStr
        {
            get { return m_index.ToString("00000"); }
        }

        public string INIFILE = "";
        //public string INI_PATH = "";

        public virtual void Initial(string epath, int ercpindex, string enamefile)
        {
            m_path = epath;
            m_name = enamefile;
            m_index = ercpindex;
            ChangeIndex(m_index);
        }
        public virtual void ChangeIndex(int eindex)
        {
            m_index = eindex;
            if (!System.IO.Directory.Exists(m_path + "\\" + m_index.ToString("00000")))
                System.IO.Directory.CreateDirectory(m_path + "\\" + m_index.ToString("00000"));

            INIFILE = m_path + "\\" + m_index.ToString("00000") + "\\" + m_name;
            //Load(eCancel);
        }
        public virtual void Load(bool eCancel = false)
        {

        }
        public virtual void Save()
        {
        }

        public string RecttoStringSimple(Rectangle Rect)
        {
            return Rect.X.ToString() + "," + Rect.Y.ToString() + "," + Rect.Width.ToString() + "," + Rect.Height.ToString();
        }
        public Rectangle StringtoRect(string RectStr)
        {
            string[] str = RectStr.Split(',');
            return new Rectangle(int.Parse(str[0]), int.Parse(str[1]), int.Parse(str[2]), int.Parse(str[3]));
        }
        public string RectFtoStringSimple(RectangleF RectF)
        {
            string Str = "";

            Str += RectF.X.ToString() + ",";
            Str += RectF.Y.ToString() + ",";
            Str += RectF.Width.ToString() + ",";
            Str += RectF.Height.ToString();

            return Str;
        }
        public RectangleF StringtoRectF(string RectStr)
        {
            string[] strs = RectStr.Split(',');
            RectangleF rectF = new RectangleF();

            rectF.X = float.Parse(strs[0]);
            rectF.Y = float.Parse(strs[1]);
            rectF.Width = float.Parse(strs[2]);
            rectF.Height = float.Parse(strs[3]);

            return rectF;


        }
        public string PointFtoStringSimple(PointF ptf)
        {
            string Str = "";

            Str += ptf.X.ToString() + ",";
            Str += ptf.Y.ToString();

            return Str;
        }
        public PointF StringtoPointF(string ptfStr)
        {
            string[] strs = ptfStr.Split(',');
            PointF rectF = new PointF();

            rectF.X = float.Parse(strs[0]);
            rectF.Y = float.Parse(strs[1]);

            return rectF;


        }
        public void ReadData(ref string DataStr, string FileName)
        {
            if (!File.Exists(FileName))
                return;

            FileStream fs = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.None);
            StreamReader Srr = new StreamReader(fs, Encoding.Default);

            DataStr = Srr.ReadToEnd();

            Srr.Close();
            Srr.Dispose();
        }
        public void SaveData(string DataStr, string FileName)
        {
            StreamWriter Swr = new StreamWriter(FileName, false, Encoding.Default);

            Swr.Write(DataStr);

            Swr.Flush();
            Swr.Close();
            Swr.Dispose();
        }


    }
}
