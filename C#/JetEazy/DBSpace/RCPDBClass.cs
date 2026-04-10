using JetEazy.BasicSpace;
using System;
using System.Collections.Generic;
using System.IO;

namespace JetEazy.DBSpace
{
    public class RCPItemClass
    {
        public bool IsEditing = false;

        char Separator = '\x1F';
        char SubSeparator = '@';

        public int Index = -1;
        public string Name = "";
        public string Version = "";
        public string Comment = "";
        public float Offset = 65.52f;

        public string StartDateTime = JzTimes.DateTimeString;
        public string ModifyDateTime = JzTimes.DateTimeString;


        string PICPath = "";

        public void Backup()
        {

        }

        public RCPItemClass()
        {


        }
        public RCPItemClass(bool isnewtest)
        {
            if (isnewtest)
            {
                Index = 0;
                Name = "系統編號";
                Version = "US";
                Comment = "N/A";

                StartDateTime = JzTimes.DateTimeString;
                ModifyDateTime = JzTimes.DateTimeString;
            }
            else
            {
                Index = 1;
                Name = "No.1";
                Version = "US";
                Comment = "N/A";

                StartDateTime = JzTimes.DateTimeString;
                ModifyDateTime = JzTimes.DateTimeString;
            }
        }
        public RCPItemClass(string str,string picpath)
        {
            PICPath = picpath;
            FromString(str);
        }
        public RCPItemClass(string str)
        {
            FromString(str);
        }

        public RCPItemClass Clone()
        {
            return new RCPItemClass(this.ToString());
        }

        public void FromString(string Str)
        {
            string[] strs = Str.Split(Separator);

            Index = int.Parse(strs[0]);
            Name = strs[1];
            Version = strs[2];
            Comment = strs[3];

            StartDateTime = strs[4];
            ModifyDateTime = strs[5];

            if(strs[6] != "")
                Offset = float.Parse(strs[6]);

        }
        public override string ToString()
        {
            string Str = "";

            Str += Index.ToString() + Separator;
            Str += Name.Replace(Environment.NewLine, "*").Trim() + Separator;
            Str += Version.Replace(Environment.NewLine, "*").Trim() + Separator;
            Str += Comment.Replace(Environment.NewLine, "*").Trim() + Separator;
            Str += StartDateTime.Replace(Environment.NewLine, "*").Trim() + Separator;
            Str += ModifyDateTime.Replace(Environment.NewLine, "*").Trim() + Separator;

            Str += Offset.ToString();

            return Str;
        }

        public string ToESSString()
        {
            string Str = "";

            Str += "[" + Index.ToString() + "]";
            Str += " " + Name + " ";
            Str += "(" + Version + ")";

            return Str;
        }
        public string ToModifyString()
        {
            string Str = "";

            Str += "Start Date: " + StartDateTime + Environment.NewLine + "Modify Date: " + ModifyDateTime;

            return Str;
        }
        public string ToJustModifyString()
        {
            string Str = "";

            Str += "Modify Date: " + ModifyDateTime;

            return Str;
        }

        public string ToPICPath()
        {
            string retstr = "";

            retstr = PICPath + "\\" + Index.ToString("00000");

            return retstr;
        }

        string ToOtherString()
        {
            string Str = "";
            return Str;
        }
        void FromOtherString(string Str)
        {
        }

        public string IndexStr
        {
            get
            {
                return Index.ToString("00000");
            }
        }

        public bool CheckFilter(string FilterStr)
        {
            string Str = (Name + "(" + Version + ")").ToUpper();

            return (Str.IndexOf(FilterStr) > -1);
        }
    }

    public class RCPDBClass
    {
        char mySeparator = '\x1E';

        JzToolsClass JzTools = new JzToolsClass();

        public int Indicator = -1;
        public List<RCPItemClass> RCPItemList = new List<RCPItemClass>();

        RCPItemClass RCPItemNull = new RCPItemClass();
        public RCPItemClass RCPItemNow
        {
            get
            {
                if (Indicator == -1)
                    return RCPItemNull;
                else
                {
                    int i = 0;
                    bool IsFound = false;

                    foreach (RCPItemClass rcpitem in RCPItemList)
                    {
                        if (rcpitem.Index == Indicator)
                        {
                            IsFound = true;
                            break;
                        }

                        i++;
                    }

                    if (!IsFound)
                        i = 0;

                    return RCPItemList[i];
                }
            }
        }
        public RCPItemClass RCPItemLast
        {
            get
            {
                return RCPItemList[RCPItemList.Count - 1];
            }
        }

        public RCPItemClass GetRCPItem(int GetIndex)
        {
            int i = 0;

            foreach (RCPItemClass rcpitem in RCPItemList)
            {
                if (rcpitem.Index == GetIndex)
                {
                    break;
                }
                i++;
            }

            if (i == RCPItemList.Count)
                return RCPItemNull;
            else
                return RCPItemList[i];
        }

        //Inside Variable
        string RCPDBFile = "";
        string PICPath = "";

        public RCPDBClass(string rcpdbfile, string picpath, int LastRecipeIndex)
        {
            RCPDBFile = rcpdbfile;

            //* For First Recipe
            //RCPItemList.Add(new RCPItemClass(true));
            //RCPItemList.Add(new RCPItemClass(false));
            //Save();
            //*/

            PICPath = picpath;
            Load(rcpdbfile);

            Indicator = LastRecipeIndex;

        }
        public void Load(string rcpdbfile)
        {
            string Str = "";
            string[] strs;

            JzTools.ReadData(ref Str, rcpdbfile);

            Str = Str.Replace(Environment.NewLine, mySeparator.ToString());
            strs = Str.Split(mySeparator);

            RCPItemList.Clear();
            foreach (string str in strs)
            {
                RCPItemClass rcpitem = new RCPItemClass(str, PICPath);
                RCPItemList.Add(rcpitem);
            }
        }

        public void ReLoad()
        {
            Load(RCPDBFile);
        }

        public void Save()
        {
            string Str = "";

            foreach (RCPItemClass rcpitem in RCPItemList)
            {
                Str += rcpitem.ToString() + Environment.NewLine;
            }

            Str = JzTools.RemoveLastChar(Str, 2);

            JzTools.SaveData(Str, RCPDBFile);
        }

        int FromIndex = 0;

        public void AddAndCopy(bool IsCopy)
        {
            int LastIndex = RCPItemLast.Index + 1;

            FromIndex = RCPItemNow.Index;

            RCPItemClass NewRCPItem = new RCPItemClass(RCPItemNow.ToString());

            NewRCPItem.Index = LastIndex;
            NewRCPItem.Name += "-" + JzTimes.TimeSerialString;
            NewRCPItem.Version = "US";

            NewRCPItem.StartDateTime = JzTimes.DateTimeString;
            NewRCPItem.ModifyDateTime = JzTimes.DateTimeString;

            JzTools.CreateDirectory(PICPath + "\\" + NewRCPItem.IndexStr);
            ////JzTools.CreateDirectory(PICPath + "\\" + NewRCPItem.IndexStr + "\\00000");

            //CopyAllFiles(RCPItemNow.IndexStr, NewRCPItem.IndexStr);

            //if (!IsCopy)
            //{
            //    int i = VIEW.SETUPList.Count - 1;

            //    while (i > -1)
            //    {
            //        if (i > 0)
            //        {
            //            VIEW.SETUPList[i].Suicide();
            //            VIEW.SETUPList.RemoveAt(i);
            //        }
            //        else
            //        {
            //            foreach (ASSIGNClass assign in VIEW.ASSIGNList)
            //            {
            //                assign.Suicide();
            //            }

            //            VIEW.ASSIGNList.Clear();

            //            foreach (SIDEClass side in VIEW.SETUPList[i].SIDEList)
            //            {
            //                side.Suicide(false); // 清掉裏面的東西
            //            }

            //            VIEW.SETUPList[i].Index = 0;
            //        }

            //        i--;
            //    }
            //}

            RCPItemList.Add(NewRCPItem);

            Indicator = LastIndex;
        }

        void CopyAllFiles(string FromIndexNoStr, string ToIndexNoStr)
        {
            string[] Files = Directory.GetFiles(PICPath + "\\" + FromIndexNoStr, "*.*");

            foreach (string file in Files)
            {
                File.Copy(file, file.Replace(FromIndexNoStr, ToIndexNoStr), true);
            }

            string[] Dirs = Directory.GetDirectories(PICPath + "\\" + FromIndexNoStr);

            foreach (string dir in Dirs)
            {
                Files = Directory.GetFiles(dir, "*.*");

                JzTools.CreateDirectory(dir.Replace("\\" + FromIndexNoStr + "\\", "\\" + ToIndexNoStr + "\\"));

                foreach (string file in Files)
                {
                    File.Copy(file, file.Replace("\\" + FromIndexNoStr + "\\","\\"+  ToIndexNoStr + "\\"), true);
                }
            }
        }

        public void Delete(int DelINdex)
        {
            RCPItemList.RemoveAt(DelINdex);
        }
        public void DeleteLast()
        {
            RCPItemList.RemoveAt(RCPItemList.Count - 1);
            Indicator = FromIndex;
        }
        public bool CheckDuplicate(string NameStr, int IndexNow)
        {
            bool ret = false;

            foreach (RCPItemClass rcpitem in RCPItemList)
            {
                if (rcpitem.Index == IndexNow)
                    continue;

                if ((rcpitem.Name + rcpitem.Version).ToUpper() == NameStr.ToUpper())
                {
                    ret = true;
                    break;
                }
            }

            return ret;
        }

        public bool IsDifferentName(string eName)
        {
            bool ret = false;

            int LastIndex = RCPItemNow.Index;
            int SelIndex = RCPItemNow.Index;

            foreach (RCPItemClass rcpitem in RCPItemList)
            {
                if (rcpitem.Name.ToUpper() == eName.ToUpper())
                {
                    SelIndex = rcpitem.Index;
                    break;
                }
            }

            if (LastIndex != SelIndex)
            {
                Indicator = SelIndex;
                ret = true;
            }

            return ret;
        }
        public bool IsDifferentVersion(string version)
        {
            bool ret = false;

            int LastIndex = RCPItemNow.Index;
            int SelIndex = 0;

            foreach (RCPItemClass rcpitem in RCPItemList)
            {
                if (rcpitem.Version.ToUpper() == version.ToUpper())
                {
                    SelIndex = rcpitem.Index;
                    break;
                }
            }

            if (LastIndex != SelIndex)
            {
                Indicator = SelIndex;
                ret = true;
            }

            return ret;
        }
        public bool IsDifferentVersion(string version, string colorstr)
        {
            bool ret = false;

            int LastIndex = RCPItemNow.Index;
            int SelIndex = 0;

            foreach (RCPItemClass rcpitem in RCPItemList)
            {
                if (rcpitem.Version.ToUpper() == version.ToUpper() && rcpitem.Name.ToUpper().IndexOf(colorstr) > -1)
                {
                    SelIndex = rcpitem.Index;
                    break;
                }
            }

            if (LastIndex != SelIndex)
            {
                Indicator = SelIndex;
                ret = true;
            }

            return ret;
        }

        public List<string> GetRecipeStringList()
        {
            List<string> lst = new List<string>();

            foreach (RCPItemClass rcpitem in RCPItemList)
            {
                if(rcpitem.Index > 0)
                    lst.Add(rcpitem.ToESSString() + "?" + rcpitem.IndexStr);
            }
            return lst;
        }
        public List<string> GetRecipeStringList2()
        {
            List<string> lst = new List<string>();

            foreach (RCPItemClass rcpitem in RCPItemList)
            {
                if (rcpitem.Index > 0)
                    lst.Add(rcpitem.Name + "?" + rcpitem.IndexStr);
            }
            return lst;
        }


        #region Backup and Restore Operation

        RCPItemClass RCPItemBack;
        
        public void Backup()
        {
            RCPItemBack = new RCPItemClass();
            RCPItemBack.FromString(RCPItemNow.ToString());
        }
        public void Restore()
        {
            RCPItemNow.FromString(RCPItemBack.ToString());
        }

        #endregion

        public delegate void TriggerHandler(RCPStatusEnum status);
        public event TriggerHandler TriggerAction;
        public void OnTrigger(RCPStatusEnum status)
        {
            if (TriggerAction != null)
            {
                TriggerAction(status);
            }
        }
    }
}
