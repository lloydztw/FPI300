using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;

namespace JetEazy.OPSpace
{
    public class CalibrationPlateClass
    {
        public string m_Path = string.Empty;
        //private int m_index = 0;
        //private string m_name = String.Empty;
        public List<Rectangle> m_RectsList = new List<Rectangle>();
        public double m_Reslution = 0.75;
        public string m_FileName = "CalibrationPlate.txt";
        public CalibrationPlateClass(string epath)
        {
            m_Path = epath;
        }
        public void Load()
        {
            m_RectsList.Clear();
            if (File.Exists(m_Path + "\\" + m_FileName))
            {
                string Str = string.Empty;
                ReadData(ref Str, m_Path + "\\" + m_FileName);
                string[] vs = Str.Replace(Environment.NewLine, "@").Split('@');
                for (int i = 0; i < vs.Length; i++)
                {
                    if (!string.IsNullOrEmpty(vs[i]))
                    {
                        Rectangle rect = StringtoRect(vs[i]);
                        m_RectsList.Add(rect);
                    }
                }
            }
        }
        public void Save()
        {
            if (m_RectsList.Count == 0)
                return;
            string Str = string.Empty;
            foreach (Rectangle rect in m_RectsList)
            {
                Str += RecttoString(rect) + Environment.NewLine;
            }

            if (!Directory.Exists(m_Path))
                Directory.CreateDirectory(m_Path);
            SaveData(Str, m_Path + "\\" + m_FileName, false);
        }
        public double GetReslutionLocation(PointF ePointF, bool dirx)
        {
            if (m_RectsList.Count == 0)
                return 0.008;

            double min = 10000000;
            int index = 0;
            int minindex = 0;
            foreach (Rectangle rect in m_RectsList)
            {
                double tmpmin = Math.Abs(ePointF.X - rect.X);
                if (tmpmin < min)
                {
                    min = tmpmin;
                    minindex = index;
                }

                index++;
            }

            if (minindex >= m_RectsList.Count)
                return 0.008;
            Rectangle findRect = m_RectsList[minindex];

            int width = Math.Max(findRect.Width, findRect.Height);
            int height = Math.Min(findRect.Width, findRect.Height);

            double ret = (height * 1.0 / width) * m_Reslution;
            if (dirx)
                ret = m_Reslution / findRect.Width;
            else
                ret = m_Reslution / findRect.Height;
            return ret;
        }


        void ReadData(ref string DataStr, string FileName)
        {
            FileStream fs = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.None);
            StreamReader Srr = new StreamReader(fs, Encoding.Default);

            DataStr = Srr.ReadToEnd();

            Srr.Close();
            Srr.Dispose();
        }
        void SaveData(string DataStr, string FileName, bool append)
        {
            StreamWriter Swr = new StreamWriter(FileName, append, Encoding.Default);

            Swr.Write(DataStr);

            Swr.Flush();
            Swr.Close();
            Swr.Dispose();
        }
        string RecttoString(Rectangle Rect)
        {
            return Rect.X.ToString().PadLeft(4) + "," + Rect.Y.ToString().PadLeft(4) + "," + Rect.Width.ToString().PadLeft(4) + "," + Rect.Height.ToString().PadLeft(4);
        }
        Rectangle StringtoRect(string RectStr)
        {
            string[] str = RectStr.Split(',');
            return new Rectangle(int.Parse(str[0]), int.Parse(str[1]), int.Parse(str[2]), int.Parse(str[3]));
        }
    }
}
