using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics.Eventing.Reader;
using System.Drawing.Imaging;
//using BSA.GlobalSpace;

namespace BSA.ControlSpace
{
    public class TrayClass
    {
        //public static char SeperateCharA = '\x1e';
        char SeperateCharB = '\x1d';
        //public static char SeperateCharC = '\x1f';
        //public static char SeperateCharD = '\x03';
        //public static char SeperateCharE = '\x04';
        //public static char SeperateCharF = '\x05';
        //public static char SeperateCharG = '\x06';

        public static Color GetBinColor(int binnno)
        {
            return BinColor(binnno);
        }
        /// <summary>
        /// Bin 的顏色
        /// </summary>
        /// <param name="binno"></param>
        /// <returns></returns>
        static Color BinColor(int binno)
        {
            Color retColor = Color.Black;

            switch (binno)
            {
                case 0:
                    retColor =  Color.Green;
                    break;
                case 1:
                    retColor = Color.Gray;
                    break;
                case 2:
                    retColor = Color.Gold;
                    break;
                case 3:
                    retColor = Color.Pink;
                    break;
                case 4:
                    retColor = Color.Orange;
                    break;
                case 5:
                    retColor = Color.Purple;
                    break;
                case 6:
                    retColor = Color.YellowGreen;
                    break;
                case 7:
                    retColor = Color.DarkRed;
                    break;
                case 8:
                    retColor = Color.Red;
                    break;
                case 9:
                    retColor = Color.DarkOrange;
                    break;
                case -1:
                    retColor = Color.White;
                    break;
            }

            return retColor;
        }

        const int MAXROUTINGCOUNT = 200;

        public int mycol = 0;
        public int myrow = 0;
        public int mybincount = 0;

        public int UIWidth = 0;
        public int UIHeight = 0;

        public float linewidth = 0f;
        public float lineheight = 0f;
        public float txtX = 0f;
        public float txtY = 0f;

        public string traycodestr = "";

        public int[] mybinarray;

        public ChipClass [] myChipArray;

        public Bitmap bmpTray = new Bitmap(1, 1);
        public Bitmap bmpTray90 = new Bitmap(1, 1);

        public void Initial(int col, int row,int uiwidth,int uiheight,int bincount, string datastr,bool israndom)
        {
            int i = 0;
            int j = 0;

            mycol = col;
            myrow = row;
            mybincount = bincount;

            UIWidth = uiwidth;
            UIHeight = uiheight;

            mybinarray = new int[col * row];
            myChipArray = new ChipClass[col * row];

            //if (datastr.Length > 0)
            //{
            //    datastr = datastr.Replace(Environment.NewLine, Universal.SeperateCharB.ToString());

            //    string[] strs = datastr.Split(Universal.SeperateCharB);

            //    for (i = 0; i < strs.Length; i++)
            //    {
            //        //ADD New
            //        ChipClass mychip = new ChipClass();
            //        mychip.FromString(strs[i]);

            //        myChipArray[i] = mychip;
            //        mybinarray[i] = mychip.binNo;
            //    }
            //}

            if (israndom)
                GenRandomBins();
            else
                ClearTray();

            linewidth = (float)UIWidth / (float)col;
            lineheight = (float)UIHeight / (float)row;

            txtX = 2f;
            txtY = 2f;

            //bmpTray.Dispose();
            //bmpTray = new Bitmap(UIWidth, UIHeight);

            //Graphics grf = Graphics.FromImage(bmpTray);
            //DrawData(grf, mybinarray);
            //grf.Dispose();
            DrawMap();
        }
        public void SetTrayUI(int col, int row, string su, int suIndex)
        {
            int index = 0;
            int dataStart = row * mycol + col;
            while (index < su.Length)
            {
                if (su[index].ToString() == "1")
                {
                    if (dataStart + index >= mybinarray.Length)
                        break;
                    mybinarray[dataStart + index] = suIndex;
                }
                index++;
            }
            DrawMap();
        }
        public void ClearTrayUI()
        {
            ClearTray();
            DrawMap();
        }
        void ClearTray()
        {
            int i = 0;

            myChipArray = new ChipClass[mybinarray.Length];

            while (i < mybinarray.Length)
            {
                ChipClass mychip = new ChipClass();
                mychip.ORGLocation = new Point(i / mycol, i % mycol);
                myChipArray[i] = mychip;

                mybinarray[i] = -1;
                i++;
            }
        }
        public string GetBinString(string binStr = ",")
        {
            return String.Join(binStr, mybinarray);
        }
        public void SetBinString(string binstr)
        {
            mybinarray = binstr.Split(',').Select(Int32.Parse).ToArray();
        }
        public string GetChipString()
        {
            string rtnstr = "";

            foreach(ChipClass chip in myChipArray)
            {
                rtnstr += chip.ToString() + SeperateCharB;
            }

            rtnstr = rtnstr.Remove(rtnstr.Length - 1, 1);

            return rtnstr;
            //return String.Join(",", mybinarray);
        }
        public void SetChipString(string chipstr)
        {

            int i = 0;
            string[] strs = chipstr.Split(SeperateCharB);

            myChipArray = new ChipClass[strs.Length];
            mybinarray= new int[strs.Length];

            foreach(string str in  strs)
            {
                ChipClass mychip = new ChipClass();
                mychip.FromString(str);
                myChipArray[i] = mychip;

                mybinarray[i] = mychip.binNo;

                i++;
            }


            //mybinarray = binstr.Split(',').Select(Int32.Parse).ToArray();
        }
        public void DrawMap()
        {
            bmpTray.Dispose();
            bmpTray = new Bitmap(UIWidth, UIHeight);

            Graphics grf = Graphics.FromImage(bmpTray);
            DrawData(grf, mybinarray);
            grf.Dispose();

            bmpTray90.Dispose();
            bmpTray90 = new Bitmap(bmpTray);
            bmpTray90.RotateFlip(RotateFlipType.Rotate90FlipNone);
        }
        public int GetBinFrist()
        {
            return mybinarray[0];
        }
        public string Get(int col, int row, int headcount)
        {
            int i = 0;
            string retstr = "";

            if (col >= mycol)
            {
                return retstr;
            }

            while (i < headcount)
            {
                if ((col * row + i) > mybinarray.Length)
                {
                    break;
                }

                retstr += mybinarray[col * row + i].ToString() + ",";

                i++;

                if (i == mycol)
                {
                    break;
                }
            }

            retstr = RemoveLastChar(retstr, 1);

            return retstr;
        }
        public string Get(int headcount, int pcount)
        {
            string retstr = "";


            return retstr;
        }
        
        void DrawData(Graphics g, int[] binarray)
        {
            int i = 0;
            int j = 0;

            Brush B = new SolidBrush(Color.White);
            Brush Bb = new SolidBrush(Color.Black);
            Pen P = new Pen(Color.Black, 1.5f);

            Font F = new Font("Arial", 20);

            //清成白色
            g.FillRectangle(B, new RectangleF(0, 0, UIWidth, UIHeight));

            //寫BIN值
            i=0;
            j=0;

            foreach (int bin in binarray)
            {
                //畫bin的顏色
                Brush Bbin = new SolidBrush(BinColor(bin));
                g.FillRectangle(Bbin, new RectangleF(linewidth * i, lineheight * j, linewidth, lineheight));
                //畫bin的值
                if (bin > -1)
                    g.DrawString(bin.ToString(), F, Bb, new Point((int)(txtX + (linewidth * i)), (int)(txtY + (lineheight * j))));

                i++;
                if (i == mycol)
                {
                    j++;
                    i=0;
                }
            }

            //畫線
            i=0;
            while (i < mycol - 1)
            {
                g.DrawLine(P, new Point((int)(linewidth*(i + 1)), 0), new Point((int)(linewidth * (i + 1)), UIHeight));
                i++;
            }
            i=0;
            while (i < myrow - 1)
            {
                g.DrawLine(P, new Point(0, (int)(lineheight * (i + 1))), new Point(UIWidth, (int)(lineheight *(i + 1))));
                i++;
            }
        }
        string RemoveLastChar(string Str, int Count)
        {
            if (Str.Length < Count)
                return "";

            return Str.Remove(Str.Length - Count, Count);
        }

        public TrayClass Clone()
        {
            TrayClass tray = new TrayClass();

            tray.mycol = mycol;
            tray.myrow = myrow;
            tray.mybincount = mybincount;

            tray.UIWidth = UIWidth;
            tray.UIHeight = UIHeight;

            tray.linewidth = linewidth;
            tray.lineheight = lineheight;

            tray.txtX = txtX;
            tray.txtY = txtY;

            Array.Copy(mybinarray, tray.mybinarray,mybinarray.Length);

            return tray;
        }

        List<string> RoutingList;
        List<string> DebugRoutingList;
        List<string> LastStatusList;

        int Vaccount = 0;
        string PadZero = "";
        string PadZeroFull = "";
        string PadXXFull = "";

        int RunBinNo = 0;
        int DestChannelNo = 0;

        int MinStepCount = 0;
        int DebugCount = 0;
        int RepeatCount = 0;

        int SrcCHNo = 0;
        int DestCHNo = 0;

        string[] VacMethod;

        string DEBUGHEAD = "0000";

       public void GetBestRouteEX(int binno, string[] vacmethod, int vaccount, TrayClass desttray,ref string routingstr,int routingindex,int srcchno,int destchno)
        {
            int i = 0;

            string mybinarraystr = ToPickMap(binno);
            string destbinarraystr = desttray.ToFillDestMap();

            string SaveDebugPath = "";// Universal.PATHSTR + "\\";
            //TEST FOR SAME DATA
            //mybinarraystr = "11101010011011111110100101001110";
            //destbinarraystr = "X" + "".PadRight(31, '0');
            //destbinarraystr = "".PadRight(3, 'X') + "".PadRight(29,'0');

            if (RoutingList != null)
                RoutingList.Clear();
            else
                RoutingList = new List<string>();

            if (DebugRoutingList != null)
                DebugRoutingList.Clear();
            else
                DebugRoutingList = new List<string>();

            if (LastStatusList != null)
                LastStatusList.Clear();
            else
                LastStatusList = new List<string>();

            Vaccount = vaccount;
            PadZero = "".PadLeft(Vaccount - 1, '0');
            PadZeroFull = "".PadLeft(Vaccount, '0');
            PadXXFull = "".PadLeft(mycol, 'X');

            RunBinNo = binno;
            MinStepCount = 100000;
            DebugCount = 0;
            RepeatCount = 0;

            SrcCHNo = srcchno;
            DestChannelNo = destchno;

            string rseedstr = "";
            string debugseedstr = "";

            string vacheadnow = PadZeroFull;
            int stepcount = 0;
            int level = 0;

            //DEBUG
            vacheadnow = DEBUGHEAD;

            VacMethod = new string[vacmethod.Length];
            Array.Copy(vacmethod, VacMethod, vacmethod.Length);

            GetBestRouteEX(mybinarraystr, destbinarraystr, vacheadnow, rseedstr, debugseedstr, level, stepcount);

            string combinstring = "";
            foreach (string str in RoutingList)
            {
                combinstring += "".PadRight(100, '=') + Environment.NewLine;
                combinstring += str + Environment.NewLine;
            }
            SaveData(combinstring, SaveDebugPath + routingindex.ToString("0000") + "-" + traycodestr + "--" + desttray.traycodestr  + " ROUTELISTALL.TXT");

            combinstring = "";
            foreach (string str in DebugRoutingList)
            {
                combinstring += "".PadRight(100, '=') + Environment.NewLine;
                combinstring += str + Environment.NewLine;
            }
            SaveData(combinstring, SaveDebugPath + routingindex.ToString("0000") + "-" + traycodestr + "--" + desttray.traycodestr  + " DEBUGROUTELISTALL.TXT");

            combinstring = "";
            foreach (string str in LastStatusList)
            {
                combinstring += "".PadRight(100, '=') + Environment.NewLine;
                combinstring += str + Environment.NewLine;
            }
            SaveData(combinstring, SaveDebugPath + routingindex.ToString("0000") + "-" + traycodestr + "--" + desttray.traycodestr  + " DEBUGLASTSTAUSALL.TXT");

            string Laststatusstr = LastStatusList[LastStatusList.Count - 1];
            string[] Laststrs = Laststatusstr.Split(';');
            string lastbinstatusstr = Laststrs[0];
            string lastdestbinstatusstr = Laststrs[1];

            EliminateBinNoMap(binno, lastbinstatusstr);
            desttray.EliminateEmptyMap(binno, lastdestbinstatusstr);

            DrawMap();
            desttray.DrawMap();

            //MessageBox.Show("PICK SIMULATE OK");
            routingstr = RoutingList[RoutingList.Count - 1];
        }
        void GetBestRouteEX(string binarraystr, string destbinarraystr,string vachead,string rseedstr,string debugseedstr,int level,int stepcount)
        {
            if (RepeatCount > MAXROUTINGCOUNT)
                return;

            //詢問移過去的有多少位子能使用，還有原始有多少可以移過去的
            //移到哪，有多少空位
            string TransPredict = GetPredictLocationEX(destbinarraystr);
            
            //來源有幾個BIN要移過去
            int SrcLeftCount = CountBin(1, binarraystr);

            //要移過去的HEAD應是啥樣子
            string DestLeftSpaceStr = TransPredict.Split(',')[2];

            //是接續前一次空頭吸還是從頭吸起
            if (CountBin(0, vachead) == Vaccount)
            {

                if(SrcLeftCount < DestLeftSpaceStr.Length)
                {
                    vachead = "".PadRight(SrcLeftCount, '0') + "".PadRight(Vaccount - SrcLeftCount, 'X');
                }
                else
                {
                    vachead =  DestLeftSpaceStr + "".PadRight(Vaccount - DestLeftSpaceStr.Length, 'X');
                }

                //DEBUG 
                //vachead = "0XX";

                //if (SrcLeftCount < Vaccount)
                //    vachead = "".PadRight(SrcLeftCount, '0') + "".PadRight(Vaccount - SrcLeftCount, 'X');
                //else
                //    vachead =  DestLeftSpaceStr + "".PadRight(Vaccount - DestLeftSpaceStr.Length, 'X');
            }
            foreach (string vacm in VacMethod)
            {
                //先檢查剩下要挑的數量是否適合
                int vacmbincount = CountBin(1, vacm);
                if (vacmbincount > SrcLeftCount)
                    continue;
                

                //再檢查要用哪種方式 vacm 移過去
                if (!CheckAVLEX(vachead, vacm))
                    continue;

                string findstr = FindTheBestWay(binarraystr, vacm);

                if (findstr != "")
                {
                    GetBestRouteEX(findstr, vacm, binarraystr, destbinarraystr, vachead, TransPredict, rseedstr, debugseedstr, level, stepcount);
                }
            }
        }
        void GetBestRouteEX(string findstr, string vacm, string binarraystr,string destbinarraystr, string vachead,string transpredict,string rseedstr,string debugseedstr,int level,int stepcount)
        {
            string[] strs = findstr.Split(',');

            string segbinstr = strs[0];
            string rowpos = strs[1];
            string colpos = strs[2];
            string replacebinstr = strs[3];

            //計算要變動的長度及變動的位置
            int replcecount = replacebinstr.Length;
            int replacepos = int.Parse(rowpos) * mycol + Math.Max(int.Parse(colpos), 0);

            //塞回原來的資料
            string nextbinarraystr = binarraystr.Remove(replacepos, replcecount);
            nextbinarraystr = nextbinarraystr.Insert(replacepos, replacebinstr);

            //合併吸頭和吸法
            string nextvachead = MergeHeadEX(vachead, vacm);

            //紀彔將要填入幾顆
            //int nextfillcount = fillcount + CountBin(1, vacm);

            //紀錄此時的路徑

            //string nextrseed = rseed + level.ToString() + ":" + rowpos + "," + colpos + "," + vacm + ";";
            string nextrseedstr = rseedstr + "PK,1," + int.Parse(rowpos).ToString() + "," + int.Parse(colpos).ToString() + "," + vacm + ";";

            string nextdebugseedstr = debugseedstr + "LEVEL:" + level.ToString("0000")
                                             + "  ROW:" + rowpos.PadRight(4, ' ')
                                             + "  COL:" + colpos.PadRight(4, ' ')
                                             + " HEAD:" + vachead
                                             + " METH:" + vacm
                                             + " STEC:" + stepcount.ToString().PadRight(4, ' ')
                                             + "  DID:" + DebugCount.ToString().PadRight(4, ' ')
                                             + Environment.NewLine
                                             + "ORG:" + binarraystr
                                             + Environment.NewLine
                                             + "AFT:" + nextbinarraystr
                                             + Environment.NewLine;


            DebugCount++;
            stepcount++;
            
            //if(DebugCount == 9)
            //{
            //    DebugCount = DebugCount;
            //}

            string nextdestbinarraystr = destbinarraystr;

            //如果嘴吸沒有空位了就移過去
            if (CountBin(0, nextvachead) == 0)
            {
                //stepcount++;

                //if (stepcount == 12)
                //{
                //    stepcount = stepcount;
                //}
                string[] transstrs = transpredict.Split(',');
                string destrow = transstrs[0];
                string destcol = transstrs[1];
                //string destbinreplace = transstrs[2];

                string destbinreplace = nextvachead.Replace("X", "");

                //把放完目標之之後的位置更新為XX

                int deststartindex = int.Parse(destrow) * mycol + int.Parse(destcol);

                nextdestbinarraystr = destbinarraystr.Remove(deststartindex, destbinreplace.Length);
                nextdestbinarraystr = nextdestbinarraystr.Insert(deststartindex, destbinreplace.Replace('1','X'));


                //移動去放置
                //nextrseedstr += "TRANS" + DestChannelNo.ToString("0000") + "," + destrow + "," + destcol+ "," + nextvachead + ";";
                nextrseedstr += "TR," + DestChannelNo.ToString() + "," + int.Parse(destrow).ToString() + "," + int.Parse(destcol).ToString()+ "," + nextvachead + ";";

                nextdebugseedstr += Environment.NewLine + "TRANS" + DestChannelNo.ToString("0000")
                                 + "  DSTR:" + destrow
                                 + "  DSTC:" + destcol
                                 + "  HEAD:" + nextvachead
                                 + "  STEC:" + stepcount 
                                 + " DID:" + DebugCount.ToString().PadRight(4, ' ') 
                                 + Environment.NewLine
                                 + "DSTORG:" + destbinarraystr
                                 + Environment.NewLine
                                 + "DSTAFT:" + nextdestbinarraystr
                                 + Environment.NewLine
                                 + Environment.NewLine;

                nextvachead = PadZeroFull;

                //DEBUG
                nextvachead = DEBUGHEAD;
            }

            //如果步驟大於目前最小的步驟，就不要繼續了
            if (stepcount >= MinStepCount)
            {
                RepeatCount++;
                return;
            }

            if (CountBin(1, nextbinarraystr) > 0 && CountBin(0,nextdestbinarraystr) > 0)
            {
                int nextlevel = level+1;
                //GetBestRoute(nextbinarraystr, vacmethod, nextrseed, nextvachead, stepcount, nextlevel, nextdebugseed, runtraymap, maprowindex);
                GetBestRouteEX(nextbinarraystr, nextdestbinarraystr, nextvachead, nextrseedstr, nextdebugseedstr, level, stepcount);

            }
            else
            {
                nextrseedstr = nextrseedstr.Remove(nextrseedstr.Length -1, 1);
                RoutingList.Add(nextrseedstr);
                DebugRoutingList.Add(nextdebugseedstr);
                LastStatusList.Add(nextbinarraystr + ";" + nextdestbinarraystr);

                MinStepCount = Math.Min(MinStepCount, stepcount);

                return;
            }
        }
        //檢查吸頭狀態是否能採取此種方式吸
        bool CheckAVL(string destcolstr, string vachead, string vacm,ref string aftervacmstr)
        {
            bool ret = false;

            bool IsMethodFitDest = false;

            int vacheadint = Convert.ToInt32(vachead, 2);
            int vacmint = Convert.ToInt32(vacm, 2);

            //先檢查要放的地方是否能放這個方式
            string revdestcolstr = Reverse(destcolstr);
            string revbinstr = PadZero + revdestcolstr + PadZero;

            int i = 0;
            int j = 0;

            while (i < revbinstr.Length - (Vaccount - 1))
            {
                string binsubstr = revbinstr.Substring(i, Vaccount);
                int binint = Convert.ToInt32(binsubstr, 2);

                if (vacmint == (binint & vacmint))
                {
                    IsMethodFitDest = true;

                    string tmpstr = PadZero + destcolstr + PadZero;

                    j =0;

                    while(j < tmpstr.Length)
                    {   
                        if(j >=i && j < i + Vaccount)
                        {
                            if (tmpstr[j] == '0' &&vacm[j - i] == '1')
                            {
                                aftervacmstr += 'X';
                            }
                            else
                            {
                                aftervacmstr += tmpstr[j];
                            }
                        }
                        else
                        {
                            aftervacmstr += tmpstr[j];
                        }
                        j++;
                    }

                    aftervacmstr = aftervacmstr.Substring(Vaccount -1);
                    aftervacmstr = aftervacmstr.Substring(0, aftervacmstr.Length -(Vaccount - 1));

                    break;
                }
                i++;
            }

            if(IsMethodFitDest)
                ret = (vacheadint & vacmint) == 0;
            else
                ret = IsMethodFitDest;

            return ret;

        }
        //檢查吸頭狀態是否能採取此種方式吸加強版
        bool CheckAVLEX(string vachead, string vacm)
        {
            bool isAVL = true;

            int i = 0;
           
            while(i < vachead.Length)
            {
                if ((vachead[i] == 'X' || vachead[i] == '1')  && vacm[i] == '1')
                {
                    isAVL =false;
                    break;
                }
                i++;
            }

            return isAVL;

        }
        //把0換成1，把1換成0
        string Reverse(string datastr)
        {
            string ret = "";

            int vacheadint = Convert.ToInt32(datastr, 2);

            ret = Convert.ToString(~vacheadint, 2);
            ret = ret.Substring(ret.Length - Vaccount, Vaccount);

            return ret;
        }
        //計數有幾個BIN值
        int CountBin(int binno,string binstr)
        {
            string str = binstr.Replace(binno.ToString(), "");

            return binstr.Length - str.Length;
        }
        public int CountBin(int binno)
        {
            int retcount = 0;
            int i = 0;

            //先把相應指定的BIN值變成 1,其他BIN值變 0
            while (i < mybinarray.Length)
            {
                if (mybinarray[i].Equals(binno))
                    retcount++;
                i++;
            }

            return retcount;
        }
        public int CountBin(int binno,int[] binarray)
        {
            int retcount = 0;
            int i = 0;

            //先把相應指定的BIN值變成 1,其他BIN值變 0
            while (i < binarray.Length)
            {
                if (binarray[i].Equals(binno))
                    retcount++;
                i++;
            }

            return retcount;
        }

        public int CountBin(string binnostr)
        {
            int i = 0;
            int retcount = 0;  
            string[] bins = binnostr.Split(',');

            int[] binnos = new int[bins.Length];

            while(i < bins.Length)
            {
                binnos[i] = int.Parse(bins[i]);
                i++;
            }

            foreach (int bin in mybinarray)
            {
                if(binnos.Contains(bin))
                {
                    retcount++;
                }
            }
            return retcount;
        }
        public int CountEmpty()
        {
            return CountBin(-1);
        }
        public bool IsEmpty()
        {
            return CountBin(-1) == mybinarray.Length;
        }
        //合併挑檢頭和挑撿方式
        string MergeHead(string vachead,string vacm)
        {
            string ret = "";

            int vacheadint = Convert.ToInt32(vachead, 2);
            int vacmint = Convert.ToInt32(vacm, 2);

            vacheadint = vacheadint | vacmint;

            ret = "0000000000000000000000000000000" + Convert.ToString(vacheadint, 2);
            ret = ret.Substring(ret.Length - Vaccount, Vaccount);

            return ret;
        }
        //合併挑檢頭和挑撿方式加強版
        string MergeHeadEX(string vachead, string vacm)
        {
            string ret = "";

            int i = 0;

            while(i < vachead.Length)
            {
                if (vachead[i] == '0' && vacm[i] == '1')
                {
                    ret += '1';
                }
                else if (vachead[i] == 'X')
                {
                    ret += 'X';
                }
                else if (vachead[i] == '1' && vacm[i] == '0')
                {
                    ret += '1';
                }
                else
                {
                    ret += '0';
                }
                i++;
            }


            return ret;
        }
        void SaveData(string DataStr, string FileName)
        {
            File.WriteAllText(FileName, DataStr, Encoding.Default);
        }
        //找到這時最合適的方式及位置
        string FindTheBestWay(string binarraystr, string vacm)
        {
            int i = 0;
            int j = 0;
            int rowindex = 0;
            List<string> sortinglist = new List<string>();
            List<string> matchlist = new List<string>();

            int vacmcount = CountBin(1, vacm);
            int vacmint = Convert.ToInt32(vacm, 2);
            
            string matchposstr = "";


            //按bin數量,Y 及 bin字串，並且過濾列裏面比vacm吸起來還少的列
            rowindex = 0;

            while(rowindex < myrow)
            {
                int startindex = rowindex * mycol;

                string binstr = binarraystr.Substring(rowindex * mycol, mycol);

                int binstrcount = CountBin(1, binstr);

                //如果此列的數目比要挑的方式要小，就跳過去
                if (binstrcount < vacmcount)
                {
                    rowindex++;
                    continue;
                }

                string sortstr = binstrcount.ToString("0000") + ","
                                + rowindex.ToString("0000") + ","
                                + binstr;

                sortinglist.Add(sortstr);
                rowindex++;
            }

            sortinglist.Sort();

            j = 0;

            while (j < sortinglist.Count)
            {
                string[] sortstrs = sortinglist[j].Split(',');

                string rowindexstr = sortstrs[1];
                rowindex = int.Parse(rowindexstr);

                int startindex = rowindex * mycol;

                //取出一整列，然後前面加入頭的數目減 1 的 0
                string binstr = PadZero + binarraystr.Substring(rowindex * mycol, mycol) + PadZero;

                i = 0;

                //比對所有位置是否能用這種方式挑
                while (i < binstr.Length - (Vaccount - 1))
                {
                    string binsubstr = binstr.Substring(i, Vaccount);
                    int binint = Convert.ToInt32(binsubstr, 2);

                    if (vacmint == (binint & vacmint))
                    {
                        int binxorint = binint ^ vacmint;

                        string binxorintstr = "0000000000000000000000000000000" + Convert.ToString(binxorint, 2);
                        binxorintstr = binxorintstr.Substring(binxorintstr.Length - Vaccount, Vaccount);

                        //取得置換的部份
                        string replacestr = "";
                        int replacepos = (Vaccount - 1) - i;

                        if (replacepos > 0)
                        {
                            replacestr = binxorintstr.Substring(replacepos);
                        }
                        else
                        {
                            replacestr = binxorintstr.Substring(0, binxorintstr.Length + replacepos);
                        }

                        //binintstr = binintstr.Substring(Vaccount -1-i)

                        matchposstr = binsubstr + "," + rowindex.ToString("0000") + "," + (i - (Vaccount - 1)).ToString("0000") + "," + replacestr;
                        matchlist.Add(matchposstr);
                    }
                    i++;
                }
                j++;
            }

            //排序 matchlist 最小的數字或是相同而最高位最左邊的就是最正確的挑法

            if (matchlist.Count > 0)
            {
                //matchlist.Sort(); //上層已排過序
                matchposstr = matchlist[0];
            }

            return matchposstr;
        }
        //將有東西的地方填入X，沒有的地方填入0
        public string ToFillDestMap()
        {
            string retstr = "";

            foreach(int bin in mybinarray)
            {
                if (bin == -1)
                    retstr += "0";
                else
                    retstr += "X";
            }
            return retstr;
        }
        //將指定BIN值填入1，其他填入0
        public string ToPickMap(int binno)
        {
            string retstr = "";

            foreach (int bin in mybinarray)
            {
                if (bin == binno)
                    retstr += "1";
                else
                    retstr += "0";
            }
            return retstr;
        }
        //取得預估位置
        public string GetPredictLocation()
        {
            string retstr = "";

            int rowindex = 0;
            int i = 0;

            string binarraystr = ToFillDestMap();

            while(rowindex < myrow)
            {
                i = 0;
                int startindex = rowindex * mycol;

                string binarraysubstr = binarraystr.Substring(startindex, mycol);

                int LeftSpace = CountBin(0, binarraystr);

                if(LeftSpace == 0)
                {
                    rowindex++;
                    continue;
                }

                int xindex = binarraysubstr.IndexOf('0');

                if (LeftSpace < Vaccount)
                {
                    retstr = rowindex.ToString("0000") + "," + xindex.ToString("0000") + "," + "".PadRight(LeftSpace, '0');
                    break;
                }
                else
                {
                    retstr = rowindex.ToString("0000") + "," + xindex.ToString("0000") + "," + "".PadRight(Vaccount, '0');
                    break;
                }

                rowindex++;
            }

            return retstr;
        }
        //取得預估位置
        public string GetPredictLocationEX(string binarraystr)
        {
            string retstr = "";

            int rowindex = 0;
            int i = 0;

            while (rowindex < myrow)
            {
                i = 0;
                int startindex = rowindex * mycol;

                string binarraysubstr = binarraystr.Substring(startindex, mycol);

                int LeftSpace = CountBin(0, binarraysubstr);

                if (LeftSpace == 0)
                {
                    rowindex++;
                    continue;
                }

                int xindex = binarraysubstr.IndexOf('0');

                if (LeftSpace < Vaccount)
                {
                    retstr = rowindex.ToString("0000") + "," + xindex.ToString("0000") + "," + "".PadRight(LeftSpace, '0');
                    break;
                }
                else
                {
                    retstr = rowindex.ToString("0000") + "," + xindex.ToString("0000") + "," + "".PadRight(Vaccount, '0');
                    break;
                }

                rowindex++;
            }

            return retstr;
        }
        int FindStartRowIndex(string runtraymap)
        {
            int i = 0;

            while(i < myrow)
            {
                int startindex = i * mycol;

                if(runtraymap.Substring(startindex,mycol) != PadXXFull)
                {
                    break;
                }
                i++;
            }

            return i;
        }

        public void EliminateBinNoMap(int binno,string elmntstr)
        {
            int i = 0;

            while(i < mybinarray.Length)
            {
                if (mybinarray[i] == binno)
                {
                    if (elmntstr[i] == '0')
                        mybinarray[i] = -1;
                }
                i++;
            }
        }
        public void EliminateEmptyMap(int binno,string elmntstr)
        {
            int i = 0;

            while (i < mybinarray.Length)
            {
                if (mybinarray[i] == -1)
                {
                    if (elmntstr[i] == 'X')
                        mybinarray[i] = binno;
                }
                i++;
            }
        }

        public void SaveBitmp(int routingindex)
        {
            Bitmap bmp = (Bitmap)bmpTray.Clone();

            //bmp.Save(Universal.PATHSTR + "\\" + routingindex.ToString("0000") + " " + traycodestr + ".png", ImageFormat.Png);

            bmp.Dispose();
        }

        public void HandleChips(string handlestr, ChipClass[] headchips)
        {
            string[] strs = handlestr.Split(',');

            string operate = strs[0];

            int rowindex = int.Parse(strs[2]);
            int colindex = int.Parse(strs[3]);

            int index = rowindex * mycol + colindex;
            int headindex = 0;

            switch (operate) 
            {
                case "PK":
                    foreach(char ch in strs[4])
                    {
                        if(ch == '1')
                        {
                            ChipClass mychip = new ChipClass();
                            mychip.Clone(myChipArray[index + headindex]);

                            myChipArray[index + headindex] = new ChipClass();

                            headchips[headindex] = mychip;

                            mybinarray[index + headindex] = myChipArray[index + headindex].binNo;

                            
                        }
                        headindex++;
                    }
                    break;
                case "TR":
                    foreach (char ch in strs[4])
                    {
                        if (ch == '1')
                        {
                            ChipClass mychip = new ChipClass();
                            mychip.Clone(headchips[headindex]);

                            mychip.DestTrayNo = traycodestr;
                            mychip.DestLocation = new Point((index + headindex) / mycol, (index + headindex) % mycol);

                            myChipArray[index + headindex] = mychip;

                            mybinarray[index + headindex] = myChipArray[index + headindex].binNo;

                            headchips[headindex] = null;
                        }
                        headindex++;
                    }
                    break;
            }




        }


        #region OLD METHODS

        public void GetBestRoute(int binno, string[] vacmethod, int vaccount, ref string routingstr, string runtraymap)
        {
            int i = 0;

            string mybinarraystr = "";

            //先把相應指定的BIN值變成 1,其他BIN值變 0
            while (i < mybinarray.Length)
            {
                if (mybinarray[i].Equals(binno))
                    mybinarraystr += "1";
                else
                    mybinarraystr +="0";

                i++;
            }

            //TEST FOR SAME DATA
            mybinarraystr = "11101010011011111110100101001110";
            //mybinarraystr = "11111001101111111011111111100011";
            //mybinarraystr = "10111111011101110111110001011111";
            //mybinarraystr ="01100110111011101101110011111111";

            if (RoutingList != null)
                RoutingList.Clear();
            else
                RoutingList = new List<string>();

            if (DebugRoutingList != null)
                DebugRoutingList.Clear();
            else
                DebugRoutingList = new List<string>();

            Vaccount = vaccount;
            PadZero = "".PadLeft(Vaccount - 1, '0');
            PadZeroFull = "".PadLeft(Vaccount, '0');
            PadXXFull = "".PadLeft(mycol, 'X');

            RunBinNo = binno;
            MinStepCount = 100000;
            DebugCount = 0;
            RepeatCount = 0;

            string routingseed = "";
            string debugseed = "";

            string vacheadnow = PadZeroFull;
            int stepcount = 0;
            int level = 0;
            int destrowindex = FindStartRowIndex(runtraymap);

            GetBestRoute(mybinarraystr, vacmethod, routingseed, vacheadnow, stepcount, level, debugseed, runtraymap, destrowindex);

            string combinstring = "";
            //foreach (string str in RoutingList)
            //{
            //    combinstring += str + Environment.NewLine;
            //}
            //SaveData(combinstring, "D:\\LOA\\ROUTELISTALL.TXT");

            combinstring = "";
            foreach (string str in DebugRoutingList)
            {
                combinstring += "".PadRight(100, '=') + Environment.NewLine;
                combinstring += str + Environment.NewLine;
            }
            SaveData(combinstring, "D:\\LOA\\DEBUGROUTELISTALL.TXT");
            MessageBox.Show("PICK SIMULATE OK");

            routingstr = RoutingList[RoutingList.Count - 1];
        }
        void GetBestRoute(string binarraystr, string[] vacmethod, string rseed, string vachead, int stepcount, int level, string debugseed, string runtraymap, int maprowindex)
        {
            if (RepeatCount > MAXROUTINGCOUNT)
                return;

            string destcolstr = runtraymap.Substring(maprowindex, mycol);

            foreach (string vacm in vacmethod)
            {
                //if(level == 0)
                //{
                //    level = level;
                //}
                string aftervacmstr = "";

                if (!CheckAVL(destcolstr, vachead, vacm, ref aftervacmstr))
                    continue;

                string findstr = FindTheBestWay(binarraystr, vacm);

                if (findstr != "")
                {
                    runtraymap = runtraymap.Remove(maprowindex * mycol, mycol);
                    runtraymap = runtraymap.Insert(maprowindex * mycol, aftervacmstr);

                    GetBestRoute(findstr, vacm, binarraystr, vacmethod, rseed, vachead, stepcount, level, debugseed, runtraymap, maprowindex);
                }
            }
        }
        void GetBestRoute(string findstr, string vacm, string binarraystr, string[] vacmethod, string rseed, string vachead, int stepcount, int level, string debugseed, string runtraymap, int maprowindex)
        {
            string[] strs = findstr.Split(',');

            string segbinstr = strs[0];
            string rowpos = strs[1];
            string colpos = strs[2];
            string replacebinstr = strs[3];

            //計算要變動的長度及變動的位置
            int replcecount = replacebinstr.Length;
            int replacepos = int.Parse(rowpos) * mycol + Math.Max(int.Parse(colpos), 0);

            //塞回原來的資料
            string nextbinarraystr = binarraystr.Remove(replacepos, replcecount);
            nextbinarraystr = nextbinarraystr.Insert(replacepos, replacebinstr);

            //合併吸頭和吸法
            string nextvachead = MergeHead(vachead, vacm);

            //紀彔將要填入幾顆
            //int nextfillcount = fillcount + CountBin(1, vacm);

            //紀錄此時的路徑

            DebugCount++;
            stepcount++;

            //string nextrseed = rseed + level.ToString() + ":" + rowpos + "," + colpos + "," + vacm + ";";
            string nextrseed = rseed + rowpos + "," + colpos + "," + vacm + ";";

            string nextdebugseed = debugseed + "LEVEL:" + level.ToString("0000")
                                             + "  ROW:" + rowpos.PadRight(4, ' ')
                                             + "  COL:" + colpos.PadRight(4, ' ')
                                             + " HEAD:" + vachead
                                             + " METH:" + vacm
                                             + " STEC:" + stepcount.ToString().PadRight(4, ' ')
                                             + "  DID:" + DebugCount.ToString().PadRight(4, ' ')
                                             + Environment.NewLine
                                             + "ORG:" + binarraystr
                                             + Environment.NewLine
                                             + "AFT:" + nextbinarraystr
                                             + Environment.NewLine;

            //如果嘴吸滿了就移去放，或是已填滿目的盤的個數
            if (CountBin(1, nextvachead) == Vaccount)
            {
                stepcount++;

                //if (stepcount == 12)
                //{
                //    stepcount = stepcount;
                //}

                //移動去放置
                nextrseed += "TRANS" + RunBinNo.ToString("0000") + ";";
                nextdebugseed += Environment.NewLine + "TRANS" + RunBinNo.ToString("0000") +"  STEC:" + stepcount + " DID:" + DebugCount.ToString().PadRight(4, ' ') + Environment.NewLine + Environment.NewLine;

                //if (stepcount == 11 || stepcount == 12)
                //{
                //    stepcount = stepcount;
                //}


                nextvachead = PadZeroFull;
            }

            //如果步驟大於目前最小的步驟，就不要繼續了
            if (stepcount >= MinStepCount)
            {
                RepeatCount++;
                return;
            }

            if (CountBin(1, nextbinarraystr) > 0)
            {
                int nextlevel = level+1;
                GetBestRoute(nextbinarraystr, vacmethod, nextrseed, nextvachead, stepcount, nextlevel, nextdebugseed, runtraymap, maprowindex);
            }
            else
            {
                RoutingList.Add(nextrseed);
                DebugRoutingList.Add(nextdebugseed);

                MinStepCount = Math.Min(MinStepCount, stepcount);

                return;
            }
        }

        #endregion

        public void Suicide()
        {
            bmpTray.Dispose();
        }

        void GenRandomBins()
        {
            int i = 0;
            Random rnd = new Random((int) DateTime.Now.Ticks & 0x0000FFFF);

            Thread.Sleep(1);

            myChipArray = new ChipClass[mybinarray.Length];

            while (i < mybinarray.Length)
            {
                ChipClass mychip = new ChipClass();
                mychip.ORGTrayNo = traycodestr;
                mychip.ORGLocation = new Point(i / mycol, i % mycol);
                mychip.GenMeasureData();

                int bin = rnd.Next(0, mybincount * 3) % (mybincount * 3);

                //if (bin < 8)
                //    bin = 4;
                //else
                //    bin = 1;
#if OPT_BIN10
                if (bin < 9)
                    bin++;
                else
                    bin = 0;
#else
 if (bin < 8)
                    bin++;
                else
                    bin = 0;
#endif

                mybinarray[i] = bin;
                mychip.binNo = bin;

                myChipArray[i] = mychip;

                i++;
            }
        }
    }
}
