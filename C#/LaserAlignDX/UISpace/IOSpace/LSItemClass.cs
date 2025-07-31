using Eazy_Project_III;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Traveller106.UISpace.IOSpace
{
    public class LSIOItemClass
    {
        public string Address { get; set; } = string.Empty;
        public string Funtion { get; set; } = string.Empty;
        public string CurrentValue { get; set; } = "False";
        public SwicthOnOff Switch { get; set; } = SwicthOnOff.False;
        public string Ment { get; set; } = string.Empty;
        public bool ReadOnly { get; set; } = false;

        public LSIOItemClass(string str)
        {
            FormString(str);
        }

        void FormString(string str)
        {
            string[] strs = str.Split(',');
            Address = strs[0];
            Funtion = strs[1];
            //CurrentValue = strs[2] == "1";
            //Switch = strs[3];
            if (strs.Length > 2)
            {
                Ment = strs[2];
            }
            if (strs.Length > 3)
            {
                ReadOnly = strs[3] == "0";
            }
        }

    }
}
