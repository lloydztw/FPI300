using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using JetEazy.BasicSpace;

namespace JetEazy.DBSpace
{
    public class RUNDBClass
    {
        char mySeparator = '\x1E';
        public string OPID = "OPID";
        public string ResourceID = "RESOURCE_ID";

        //Inside Variable
        string RUNDBFile = "";
        JzToolsClass JzTools = new JzToolsClass();

        public RUNDBClass(string rundbfile)
        {
            RUNDBFile = rundbfile;
            
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

            JzTools.ReadData(ref Str, RUNDBFile);

            Str = Str.Replace(Environment.NewLine, mySeparator.ToString());
            strs = Str.Split(mySeparator);

            OPID = strs[0];
            ResourceID = strs[1];
        }
        public void Save()
        {
            string Str = "";

            Str += OPID + mySeparator;
            Str += ResourceID;

            JzTools.SaveData(Str, RUNDBFile);
        }

        public void SetOPID(string opid)
        {
            OPID = opid.Trim();
            Save();
        }
        public void SetResourceID(string resourceid)
        {
            ResourceID = resourceid.Trim();
            Save();
        }
    }
}
