using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using JetEazy.BasicSpace;

namespace JetEazy.DBSpace
{
    public class AccClass
    {
        char Separator = '\x1F';
        
        public int Index = -1;
        public string NAME = "";
        public string PASSWORD = "";

        public bool IsAllowSetupINI = false;
        public bool IsAllowManageAccount = false;
        public bool IsAllowSetupRecipe = false;
        public bool ISAllowUseShopFloor = false;

        public AccClass()
        {


        }

        public AccClass(bool IsNewTestAccount)
        {
            if (IsNewTestAccount)
            {
                Index = 0;
                NAME = "admin";
                PASSWORD = "888";
                IsAllowSetupINI = true;
                IsAllowManageAccount = true;
                IsAllowSetupRecipe = true;
                ISAllowUseShopFloor = true;
            }
        }

        public AccClass(string Str)
        {
            FromString(Str);
        }

        public void FromString(string Str)
        {
            string[] strs = Str.Split(Separator);

            Index = int.Parse(strs[0]);
            NAME = strs[1];
            PASSWORD = strs[2];

            IsAllowSetupINI = strs[3] == "1";
            IsAllowManageAccount = strs[4] == "1";
            IsAllowSetupRecipe = strs[5] == "1";
            ISAllowUseShopFloor = strs[6] == "1";

        }

        public override string ToString()
        {
            string Str = "";

            Str += Index.ToString() + Separator;
            Str += NAME.Trim() + Separator;
            Str += PASSWORD.Trim() + Separator;
            Str += (IsAllowSetupINI ? "1" : "0") + Separator;
            Str += (IsAllowManageAccount ? "1" : "0") + Separator;
            Str += (IsAllowSetupRecipe ? "1" : "0") + Separator;
            Str += (ISAllowUseShopFloor ? "1" : "0");

            return Str;
        }
    }

    public class AccDBClass
    {
        char mySeparator = '\x1E';
        public int Indicator = -1;
        public List<AccClass> AccList = new List<AccClass>();

        public AccClass AccNull = new AccClass();
        JzToolsClass JzTools = new JzToolsClass();

        public AccClass AccNow
        {
            get
            {
                if (Indicator == -1)
                    return AccNull;
                else
                {
                    int i = 0;
                    foreach (AccClass acc in AccList)
                    {
                        if (acc.Index == Indicator)
                            break;
                        i++;
                    }
                    return AccList[i];
                }
            }
        }

        public bool IsSuperUser = false;
        
        public AccClass AccLast
        {
            get
            {
                return AccList[AccList.Count - 1];
            }
        }
        //Inside Variable
        string ACCDBFile = "";

        public AccDBClass(string accdbfile)
        {
            ACCDBFile = accdbfile;
            
            //* For First Account
            //AccList.Add(new AccClass(true));
            //Save();
            //*/

            Load();
        }
        public void Load()
        {
            string Str = "";
            string[] strs;

            JzTools.ReadData(ref Str, ACCDBFile);

            Str = Str.Replace(Environment.NewLine, mySeparator.ToString());
            strs = Str.Split(mySeparator);

            foreach (string str in strs)
            {
                AccList.Add(new AccClass(str));
            }

        }
        public void Save()
        {
            string Str = "";

            foreach (AccClass acc in AccList)
            {
                Str += acc.ToString() + Environment.NewLine;
            }

            Str = JzTools.RemoveLastChar(Str, 2);

            JzTools.SaveData(Str, ACCDBFile);
        }

        public void Add()
        {
            int LastIndex = AccLast.Index + 1;

            AccClass NewAcc = new AccClass(AccList[AccList.Count - 1].ToString());

            NewAcc.Index = LastIndex;
            NewAcc.NAME = "新使用者(" + LastIndex.ToString() + ")";
            NewAcc.PASSWORD = "";
            AccList.Add(NewAcc);
        }

        public void Delete(int index)
        {
            AccList.RemoveAt(index);
        }
        public void DeleteLast()
        {
            AccList.RemoveAt(AccList.Count - 1);
        }

        public bool CheckDuplicate(string NameStr,int IndexNow)
        {
            bool ret = false;

            foreach (AccClass acc in AccList)
            {
                if (acc.Index == IndexNow)
                    continue;

                if (acc.NAME.ToUpper() == NameStr.Trim().ToUpper())
                {
                    ret = true;
                    break;
                }
            }

            return ret;
        }
        public bool Check(string Name, string Password,bool IsNeedMoveToUser)
        {
            int i = 0;
            bool IsOK = false;

            foreach (AccClass acc in AccList)
            {
                if (acc.NAME.ToUpper() == Name.Trim().ToUpper() && acc.PASSWORD.ToUpper() == Password.Trim().ToUpper())
                {
                    IsOK = true;

                    if (IsNeedMoveToUser)
                    {
                        Indicator = acc.Index;
                    }
                    break;
                }

                i++;
            }
            return IsOK;
        }

    }
}
