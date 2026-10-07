//using BSA.GlobalSpace;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSA.ControlSpace
{
    public class ChipClass
    {
        char SeperateCharA = '\x1e';
        //char SeperateCharB = '\x1d';
        //public static char SeperateCharC = '\x1f';
        //public static char SeperateCharD = '\x03';
        //public static char SeperateCharE = '\x04';
        //public static char SeperateCharF = '\x05';
        //public static char SeperateCharG = '\x06';

        public string ORGTrayNo = "";
        public Point ORGLocation = new Point(-1, -1);

        public string DestTrayNo = "";
        public Point DestLocation = new Point(-1, -1);

        public int binNo = -1;
        public double[] MeasureData = new double[8];

        public void Clone(ChipClass chip)
        {
            ORGTrayNo = chip.ORGTrayNo;
            ORGLocation = chip.ORGLocation;
            DestTrayNo = chip.DestTrayNo;
            DestLocation = chip.DestLocation;

            Array.Copy(chip.MeasureData, MeasureData, 8);

            binNo = chip.binNo;
        }

        public override string ToString()
        {
            string rtnstr = "";

            rtnstr += ORGTrayNo + SeperateCharA;
            rtnstr += PointToString(ORGLocation) + SeperateCharA;
            rtnstr += DestTrayNo + SeperateCharA;
            rtnstr += PointToString(DestLocation) + SeperateCharA;

            rtnstr += binNo.ToString() + SeperateCharA;

            string mstr = "";

            foreach(double mdata in MeasureData) 
            {
                mstr += mdata.ToString("0.00000000") + ",";   
            }

            mstr = mstr.Remove(mstr.Length - 1, 1);

            rtnstr += mstr + SeperateCharA;

            return rtnstr;
        }
        public void FromString(string  str)
        {
            int i = 0;

            string[] strs = str.Split(SeperateCharA);

            ORGTrayNo = strs[0];
            ORGLocation =StringToPoint(strs[1]);

            DestTrayNo = strs[2];
            DestLocation =StringToPoint(strs[3]);

            binNo = int.Parse(strs[4]);

            i=0;
            string[] mstrs = strs[5].Split(',');
            
            MeasureData = new double[mstrs.Length];

            foreach(string mstr in mstrs)
            {
                MeasureData[i] = Convert.ToDouble(mstr);
                i++;
            }
        }

        public void GenMeasureData()
        {
            int i = 0;

            while(i < MeasureData.Length) 
            {
                MeasureData[i] = (double)ORGLocation.X /100d + (double)ORGLocation.Y /10000d + (double)i / 10000000d; 

                i++;
            }

        }


        string PointToString(Point PT)
        {
            return PT.X.ToString() + "," + PT.Y.ToString();
        }
        Point StringToPoint(string Str)
        {
            string[] strs = Str.Split(',');
            return new Point(int.Parse(strs[0]), int.Parse(strs[1]));
        }
    }
}
