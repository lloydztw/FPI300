
//#define HC_Q1_1300D

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetEazy.ControlSpace
{
    public class PLCAlarmsItemClass
    {
        public string ADR_Address = "";
        public string ADR_START_Address = "";
        public PLCAlarmsItemClass(string str)
        {
            if (str.Trim() != "")
            {
                string[] strs = str.Split(':');
                ADR_Address = CovertToNormalAddress(strs[0]);
                ADR_START_Address = CovertToNormalAddress(strs[1]);
            }
        }
        public string CovertToNormalAddress(int bit)
        {
            return CovertToNormalAddress(ADR_START_Address, bit);
        }
        public string CovertToNormalAddress(string str,int bit)
        {

#if HC_Q1_1300D

            return CovertToNormalAddressHC_Q1_1300D(str, bit);

#endif

            string ret = "";
            long addressvalue = long.Parse(str.Substring(1));
            addressvalue = addressvalue + bit;

            switch (str[0])
            {
                case 'X':
                case 'Y':
                case 'M':
                //case 'D':
                    ret = str.Substring(0, 1) + addressvalue.ToString("0000");
                    break;
                case 'A':
                case 'R':
                case 'D':
                    ret = str.Substring(0, 1) + addressvalue.ToString("00000");
                    break;
            }
            return ret;
        }
        public string CovertToNormalAddress(string str)
        {

#if HC_Q1_1300D

            return CovertToNormalAddressHC_Q1_1300D(str, 0);

#endif

            string ret = "";
            long addressvalue = long.Parse(str.Substring(1));

            switch (str[0])
            {
                case 'X':
                case 'Y':
                case 'M':
                //case 'D':
                    ret = str.Substring(0, 1) + addressvalue.ToString("0000");
                    break;
                case 'A':
                case 'R':
                case 'D':
                    ret = str.Substring(0, 1) + addressvalue.ToString("00000");
                    break;
            }
            return ret;
        }


        string CovertToNormalAddressHC_Q1_1300D(string str, int eBit)
        {
            string ret = "";
            long addressvalue = 0;// long.Parse(str.Substring(2));

            switch (str.Substring(0, 2))
            {
                case "IX":
                case "QX":
                case "QB":
                    addressvalue = long.Parse(str.Substring(2).Split('.')[0]);
                    ret = str.Substring(0, 2) + addressvalue.ToString("0000") + "." + eBit.ToString();
                    break;
                case "MW":
                    addressvalue = long.Parse(str.Substring(2));
                    ret = str.Substring(0, 2) + addressvalue.ToString("00000");
                    break;
            }
            return ret;
        }

    }
    public class PLCAlarmsClass
    {
        public List<PLCAlarmsItemClass> PLCALARMSLIST = new List<PLCAlarmsItemClass>();
        public List<PLCAlarmsItemDescriptionClass> PLCALARMSDESCLIST = new List<PLCAlarmsItemDescriptionClass>();
        public PLCAlarmsClass(string str)
        {
            PLCALARMSLIST.Clear();
            PLCALARMSDESCLIST.Clear();
            if (str.Trim() != "")
            {
                string[] strs = str.Split(',');
                foreach (string strx in strs)
                {
                    if (strx.Trim() != "")
                    {
                        PLCAlarmsItemClass plcalarmsitem = new PLCAlarmsItemClass(strx);
                        PLCALARMSLIST.Add(plcalarmsitem);
                    }
                }
            }
        }
        public void PLCAlarmsAddDescription(string str)
        {
            PLCAlarmsItemDescriptionClass item = new PLCAlarmsItemDescriptionClass(str);
            PLCALARMSDESCLIST.Add(item);
        }
    }
    public class PLCAlarmsItemDescriptionClass
    {
        public int BitNo = -1;
        public string ADR_Address = "";
        public string ADR_Chinese = "";
        public string ADR_English = "";

        public PLCAlarmsItemDescriptionClass(string str)
        {
            if (str.Trim() != "")
            {
                string[] strs = str.Replace(':', ',').Split(',');

                BitNo = int.Parse(strs[0]);
                ADR_Address = strs[1];
                ADR_Chinese = strs[2];

                if (strs.Length > 3)
                    ADR_English = strs[3];
            }
        }
    }
}
