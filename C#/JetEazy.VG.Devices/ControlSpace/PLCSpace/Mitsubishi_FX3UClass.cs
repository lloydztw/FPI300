#define FX3U


/*
 * 三菱plc 型号 FX3U
 * 为了不影响以前写的plc控制 将底层通讯修改
 *
 */

#if FX3U
using JetEazy.BasicSpace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetEazy.ControlSpace.PLCSpace
{
    public class MITFX3U : COMClass
    {
        protected char STX = '\x02';
        protected char ETX = '\x03';

        int iCountTemp = 0;
        //public int SerialCount = 0;
        System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();

        public string Live = "●";

        JzTimes PLCDuriationTime = new JzTimes();
        public int msDuriation = 0;

        protected override void COMPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            try
            {
                // 大略是這樣子用....有時指令不會一次傳完，因此要檢查最後回傳的檢查位元
                BytesToRead = COMPort.BytesToRead;
                COMPort.Read(ReadBufferByte, ReadStart, BytesToRead);
                ReadStart = ReadStart + BytesToRead;
                //
                if (Analyze(ReadStart - 3)) //此時 ReadBuffer裏有東西，利用 BytesToRead, ReadStart 及 ReadBuffer來取得所需要的資料
                {
                    base.COMPort_DataReceived(sender, e);

                    if (Live == "●")
                        Live = "○";
                    else
                        Live = "●";
                }
            }
            catch (Exception ex)
            {
                base.COMPort_DataReceived(sender, e);

                if (Live == "●")
                    Live = "○";
                else
                    Live = "●";
            }
        }
        protected bool Analyze(int LastIndex)
        {
            bool ret = false;
            //if (LastIndex >= ReadBufferByte.Length || LastIndex < 0)
            //    return ret;
            if (ReadBufferByte[LastIndex] != ETX)
            {
                return ret;
            }
            else
                ret = true;


            //取得時間差

            msDuriation = PLCDuriationTime.msDuriation;

            //紀錄開始時間

            PLCDuriationTime.Cut();

            if (!watch.IsRunning)
                watch.Start();
            if (watch.ElapsedMilliseconds > 1000)
            {
                watch.Reset();
                SerialCount = iCountTemp;
                iCountTemp = 0;
            }
            else
                iCountTemp++;

            ReadBuffer = System.Text.Encoding.ASCII.GetChars(ReadBufferByte);
            OnRead(ReadBuffer, LastCommad.GetName(), Name);

            return ret;
        }
        public void SetIO(bool IsOn, string ioname)
        {
            if (string.IsNullOrEmpty(ioname))
                return;

            string ret = "";
            string _cmd = "";
            switch (ioname[0])
            {
                //case 'X':
                case 'Y':
                    _cmd = ioname.Replace("Y", "");
                    int vDecimal = Convert.ToInt32(_cmd, 8);//指令转为10进制
                    int temp = vDecimal + 1280;//取得地址
                    String strAY = temp.ToString("X4");
                    ret = _getCmd(strAY);
                    break;
                case 'M':
                    _cmd = ioname.Replace("M", "");
                    int a = int.Parse(_cmd) + 2048;
                    String strA = a.ToString("X4");
                    ret = _getCmd(strA);

                    //M100+2048
                    //ret = str.Substring(0, 1) + addressvalue.ToString("0000");
                    break;
                //case 'A':
                //case 'R':
                //ret = str.Substring(0, 1) + addressvalue.ToString("00000");
                //case 'D':
                //    break;

                default:

                    ret = "";

                    break;
            }

            if (!string.IsNullOrEmpty(ret))
            {

                if (IsOn)
                    Command("Set Bit On", ret);
                else
                    Command("Set Bit Off", ret);

            }

            //if (IsOn)
            //    Command("Set Bit On", ioname);
            //else
            //    Command("Set Bit Off", ioname);
        }
        public void SetData(string data, string ioname)
        {
            if (string.IsNullOrEmpty(ioname))
                return;

            if (data.Length != 4)
                return;

            string ret = "";
            string _cmd = "";
            switch (ioname[0])
            {
                case 'D':
                    _cmd = ioname.Replace("D", "");
                    int a = int.Parse(_cmd) * 2 + 4096;
                    String strA = a.ToString("X4") + "02";

                    ret = strA + _getCmd(data);

                    //ret = _getCmd(strA);

                    //M100+2048
                    //ret = str.Substring(0, 1) + addressvalue.ToString("0000");
                    break;
                default:
                    ret = "";
                    break;
            }

            if (!string.IsNullOrEmpty(ret))
            {

                Command("Set Data", ret);

            }

            //Command("Set Data", ioname + data);
        }
        public void SetData(string data, string ioname, string iocount)
        {

            if (string.IsNullOrEmpty(ioname))
                return;

            //if (data.Length != 4)
            //    return;

            string ret = "";
            string _cmd = "";
            switch (ioname[0])
            {
                case 'D':
                    _cmd = ioname.Replace("D", "");
                    int a = int.Parse(_cmd) * 2 + 4096;
                    String strA = a.ToString("X4") + iocount;

                    ret = strA + data;

                    //ret = _getCmd(strA);

                    //M100+2048
                    //ret = str.Substring(0, 1) + addressvalue.ToString("0000");
                    break;
                default:
                    ret = "";
                    break;
            }

            if (!string.IsNullOrEmpty(ret))
            {

                Command("Set Data N", ret);

            }


            //Command("Set Data N", iocount + ioname + data);
        }
        public override void SetData(float data, int MWIndex, int word = 2)
        {
            SetData((int)(data * 100), MWIndex, word);

            ////发送给plc数据
            //byte[] hex = BitConverter.GetBytes(data);
            //byte[] h = new byte[2];
            //byte[] l = new byte[2];

            //h[0] = hex[0];
            //h[1] = hex[1];
            //l[0] = hex[2];
            //l[1] = hex[3];

            //ushort setH = BitConverter.ToUInt16(h, 0);
            //ushort setL = BitConverter.ToUInt16(l, 0);

            //SetData(ValueToHEX(setL, 4), "D" + MWIndex.ToString("0000"));
            //if (word == 2)
            //    SetData(ValueToHEX(setH, 4), "D" + (MWIndex + 1).ToString("0000"));

        }
        public override void SetData(int data, int MWIndex, int word = 2)
        {
            long setH = data >> 16;
            long setL = data % 65536;

            SetData(ValueToHEX(setL, 4), "D" + MWIndex.ToString("00000"));
            if (word == 2)
                SetData(ValueToHEX(setH, 4), "D" + (MWIndex + 1).ToString("00000"));
        }

        /// <summary>
        /// 写数据到PLC
        /// </summary>
        /// <param name="Address">PLC地址</param>
        /// <param name="data">数据</param>
        /// <param name="word">如为2 则写入连续两个地址</param>
        public bool SetData(string Address, int data, byte word = 1)
        {
            if (word == 2)
            {
                long setH = data >> 16;
                long setL = data % 65536;

                int address = 0;
                if (!int.TryParse(Address.Substring(1), out address))
                    return false;

                string filst = Address[0].ToString();

                SetData(ValueToHEX(setL, 4), filst + address.ToString("00000"));
                SetData(ValueToHEX(setH, 4), filst + (address + 1).ToString("00000"));

                return true;
            }
            else
            {
                int address = 0;
                if (!int.TryParse(Address.Substring(1), out address))
                    return false;

                string filst = Address[0].ToString();

                long setL = data % 65536;
                SetData(ValueToHEX(setL, 4), filst + address.ToString("00000"));

                return true;
            }
        }
        public IODataClass IOData
        {
            get { return IODataBase; }
        }

        protected override void WriteCommand()
        {
            if (IsSimulater)
                return;

            //string Str = LastCommad.GetSite() + Checksum(LastCommad.GetPLCCommad());
            //COMPort.Write(STX + Str + ETX);

            string Str = Checksum(LastCommad.GetPLCCommad() + ETX);
            COMPort.Write(STX + Str);
        }
        protected override string Checksum(string OrgString)
        {
            int j = 0;
            char[] Chars = OrgString.ToCharArray();

            j = 0;
            foreach (char ichar in Chars)
                j = j + ichar;
            return OrgString + ("00" + j.ToString("X")).Substring(("00" + j.ToString("X")).Length - 2, 2);
        }

        string _getCmd(string eStringOrg)
        {
            string ret = "";
            string strLeft = eStringOrg.Substring(0, 2);
            string strRight = eStringOrg.Substring(2, 2);
            ret = strRight + strLeft;
            return ret;
        }

        public override void Tick()
        {
            base.Tick();
        }

        //當有Input Trigger時，產生OnTrigger
        public delegate void TriggerHandler(string OperationString);
        public event TriggerHandler TriggerAction;
        public void OnTrigger(String OperationString)
        {
            if (TriggerAction != null)
            {
                TriggerAction(OperationString);
            }
        }

        //當有Input Read
        public delegate void ReadHandler(char[] readbuffer, string operationstring, string myname);
        public event ReadHandler ReadAction;
        public void OnRead(char[] readbuffer, string operationstring, string myname)
        {
            if (ReadAction != null)
            {
                ReadAction(readbuffer, operationstring, myname);
            }
        }

    }

}

#endif