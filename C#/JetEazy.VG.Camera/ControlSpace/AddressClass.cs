
//#define HC_Q1_1300D

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetEazy.ControlSpace
{
    public class AddressClass
    {
        public int SiteNo = -1;
        public string Address0 = "";
        public string Address1 = "";
        public int Offset = 0;

        public AddressClass(string str)
        {
            if (str.Trim() != "")
            {
                string[] strs = str.Replace(':', ',').Split(',');

                SiteNo = int.Parse(strs[0]);
                Address0 = CovertToNormalAddress(strs[1]);

                if (strs.Length > 2)
                    Address1 = CovertToNormalAddress(strs[2]);
            }
        }
        public AddressClass(string str, int eoffset)
        {
            if (str.Trim() != "")
            {
                Offset = eoffset;
                string[] strs = str.Replace(':', ',').Split(',');

                SiteNo = int.Parse(strs[0]);
                Address0 = CovertToNormalAddress(strs[1]);

                if (strs.Length > 2)
                    Address1 = CovertToNormalAddress(strs[2]);
            }
        }
        string CovertToNormalAddress(string str)
        {

            return str;

#if HC_Q1_1300D

            return CovertToNormalAddressHC_Q1_1300D(str);

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

        string CovertToNormalAddressHC_Q1_1300D(string str)
        {
            string ret = "";
            long addressvalue = 0;// long.Parse(str.Substring(2));

            switch (str.Substring(0, 2))
            {
                case "IX":
                case "QX":
                case "QB":
                    float addressvaluef = float.Parse(str.Substring(2)) + Offset;
                    ret = str.Substring(0, 2)+ addressvaluef.ToString("0.0");
                    break;
                case "MW":
                    addressvalue = long.Parse(str.Substring(2)) + Offset;
                    ret = str.Substring(0, 2) + addressvalue.ToString("00000");
                    break;
            }
            return ret;
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Address1))
                return SiteNo.ToString() + ":" + Address0;
            return SiteNo.ToString() + ":" + Address0 + "," + Address1;
        }
    }
}
