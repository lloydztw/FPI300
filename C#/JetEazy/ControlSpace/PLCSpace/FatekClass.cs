#define FATEK

#if FATEK

using JetEazy.BasicSpace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetEazy.ControlSpace.PLCSpace;
using OMRON.Compolet.CIPCompolet64;
using System.Windows.Forms;

namespace JetEazy.ControlSpace
{
    public class FatekPLCClass : COMClass
    {
        protected char STX = '\x02';
        protected char ETX = '\x03';

        int iCountTemp = 0;
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
                COMPort.Read(ReadBuffer, ReadStart, BytesToRead);
                ReadStart = ReadStart + BytesToRead;
                //
                if (Analyze(ReadStart - 1)) //此時 ReadBuffer裏有東西，利用 BytesToRead, ReadStart 及 ReadBuffer來取得所需要的資料
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
                //JetEazy.LoggerClass.Instance.WriteException(ex);
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

            if (ReadBuffer[LastIndex] != ETX)
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

            OnRead(ReadBuffer, LastCommad.GetName(), Name);

            //switch (LastCommad.GetName())
            //{
            //    case "Get All XY":
            //        GetX();
            //        GetY();
            //        break;
            //    case "Get All M":
            //        GetM();
            //        break;
            //    case "Get Alarm":
            //        GetR();
            //        break;
            //    default:
            //        break;
            //}

            //if (IsWindowClose)
            //{
            //    if (CommandQueue.Count == 0)
            //    {
            //        OnTrigger("CLOSE");
            //    }
            //}

            return ret;
        }
        public void SetIO(bool IsOn, string ioname)
        {
            int address = 0;
            int.TryParse(ioname.Substring(1), out address);

            ioname = ioname.Substring(0, 1) + address.ToString("0000");

            if (IsOn)
                Command("Set Bit On", ioname);
            else
                Command("Set Bit Off", ioname);
        }
        public void SetData(string data, string ioname)
        {
            Command("Set Data", ioname + data);
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
        public string ReadVari(string eVari)
        {
            string ret = string.Empty;

            if (IsSimulater)
                return ret;


            //try
            //{
            //    string varname = eVari;
            //    object obj = this.m_Compolet.ReadVariable(varname);
            //    //if (obj == null)
            //    //{
            //    //    throw new NotSupportedException();
            //    //}

            //    //VariableInfo info = this.njCompolet1.GetVariableInfo(varname);
            //    string str = this.GetValueOfVariables(obj);
            //    //if (this.listViewOfVariableNames.SelectedItems.Count > 0)
            //    //{
            //    //    if (this.listViewOfVariableNames.SelectedItems[0].SubItems[0].Text == varname)
            //    //    {
            //    //        this.listViewOfVariableNames.SelectedItems[0].SubItems.Add(string.Empty);
            //    //        this.listViewOfVariableNames.SelectedItems[0].SubItems[2].Text = str;
            //    //    }
            //    //}
            //    //this.txtValue.Text = str;
            //    ret = str;
            //}
            //catch (Exception ex)
            //{
            //    ret = string.Empty;
            //    //MessageBox.Show(ex.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //}
            return ret;
        }
        public void WriteVari(string eVari, string eValue)
        {
            if (IsSimulater)
                return;

            //try
            //{
            //    // write
            //    //--------------------------------------------------------------------------------
            //    string valWrite = eVari;// this.txtVariableName.Text;
            //    if (valWrite.StartsWith("_"))
            //    {
            //        MessageBox.Show("The SystemVariable can not write!");
            //        return;
            //    }
            //    object val = this.RemoveBrackets(eValue);
            //    if (this.m_Compolet.GetVariableInfo(valWrite).Type == VariableType.STRUCT)
            //    {
            //        val = this.ObjectToByteArray(val);
            //    }
            //    this.m_Compolet.WriteVariable(valWrite, val);

            //    //// read
            //    //this.btnReadVariable_Click(null, null);
            //}
            //catch (Exception ex)
            //{
            //    //MessageBox.Show(ex.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //}
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

            string Str = LastCommad.GetSite() + Checksum(LastCommad.GetPLCCommad());
            COMPort.Write(STX + Str + ETX);
        }
        protected override string Checksum(string OrgString)
        {
            int j = 0;
            char[] Chars = OrgString.ToCharArray();

            j = 99;
            foreach (char ichar in Chars)
                j = j + ichar;
            return OrgString + ("00" + j.ToString("X")).Substring(("00" + j.ToString("X")).Length - 2, 2);
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
