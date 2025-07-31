using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using JetEazy.BasicSpace;

namespace JetEazy.DBSpace
{
    public class EsssDBClass
    {

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

            StringBuilder temp = new StringBuilder(200);
            int Length = GetPrivateProfileString(section, key, "", temp, 200, filepath);

            retStr = temp.ToString();

            if (retStr == "")
                retStr = defaultvaluestring;
            else
                retStr = retStr.Split('/')[0]; //把說明排除掉

            return retStr;
        }
        #endregion

        //public string INIFILE = "";

        string ESSDBFile = "";

        public int LastRecipeIndex = 0;
        public int PassCount = 0;
        public int FailCount = 0;

        JzToolsClass JzTools = new JzToolsClass();

        public EsssDBClass(string essdbfile)
        {
            Load(essdbfile);
        }

        void Load(string essdbfile)
        {
            //string Str = "";
            //string[] strs;

            //ESSDBFile = essdbfile;

            //JzTools.ReadData(ref Str, ESSDBFile);

            //strs = Str.Split(',');

            //LastRecipeIndex = int.Parse(strs[0]);
            //PassCount = int.Parse(strs[1]);
            //FailCount = int.Parse(strs[2]);

            ESSDBFile = essdbfile;

            LastRecipeIndex = int.Parse(ReadINIValue("ESSDB", "LastRecipeIndex", LastRecipeIndex.ToString(), ESSDBFile));
            PassCount = int.Parse(ReadINIValue("ESSDB", "PassCount", PassCount.ToString(), ESSDBFile));
            FailCount = int.Parse(ReadINIValue("ESSDB", "FailCount", FailCount.ToString(), ESSDBFile));
        }

        public void Save()
        {
            //string Str = "";

            //Str += LastRecipeIndex.ToString() + ",";
            //Str += PassCount.ToString() + ",";
            //Str += FailCount.ToString();

            //JzTools.SaveData(Str, ESSDBFile);

            WriteINIValue("ESSDB", "LastRecipeIndex", LastRecipeIndex.ToString(), ESSDBFile);
            WriteINIValue("ESSDB", "PassCount", PassCount.ToString(), ESSDBFile);
            WriteINIValue("ESSDB", "FailCount", FailCount.ToString(), ESSDBFile);

        }

        int SaveCounter = 0;

        public void Add(bool IsPass)
        {
            if (IsPass)
                PassCount++;
            else
                FailCount++;

            SaveCounter++;

            if (SaveCounter == 5)
            {
                Save();
                SaveCounter = 0;
            }
        }
        public void Reset(bool IsPass)
        {
            if (IsPass)
                PassCount = 0;
            else
                FailCount = 0;

            Save();
        }

        public void RecipeChange(int RecipeID)
        {
            LastRecipeIndex = RecipeID;
            Save();
        }
    

    }
}
