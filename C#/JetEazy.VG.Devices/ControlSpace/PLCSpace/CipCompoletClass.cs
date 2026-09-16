


using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OMRON.Compolet.CIPCompolet64;


namespace JetEazy.ControlSpace.PLCSpace
{
    public class CipCompoletClass : COMClass
    {
        #region Config Access Functions
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        void WriteINIValue(string section, string key, string value, string filepath)
        {
            WritePrivateProfileString(section, key, value, filepath);
        }
        string ReadINIValue(string section, string key, string defaultvaluestring, string filepath)
        {
            string retStr = "";

            StringBuilder temp = new StringBuilder(200);
            int Length = GetPrivateProfileString(section, key, "", temp, 1024, filepath);

            retStr = temp.ToString();

            if (retStr == "")
                retStr = defaultvaluestring;
            else
                retStr = retStr.Split('/')[0]; //把說明排除掉

            return retStr;

        }
        #endregion

        #region HSL 通讯 需要多线程中进行 防止卡主线程

        System.Threading.Thread m_Thread_Hsl = null;
        bool m_Running = false;

        bool m_error_comm = false;

        #endregion

        #region MAYBE_NOT_USED_MEMBERS
        protected char STX = '\x02';
        protected char ETX = '\x03';
        #endregion

        #region PRIVATE_DATA_MEMBERS
        //public int SerialCount = 0;
        System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();

        public string Live = "●";

        JetEazy.BasicSpace.JzTimes PLCDuriationTime = new BasicSpace.JzTimes();
        public int msDuriation = 0;

        private OMRON.Compolet.CIPCompolet64.NJCompolet m_Compolet;
        private System.ComponentModel.IContainer components;
        //private OmronFinsNet omronFinsNet;
        //private IModbusMaster master = null;
        //System.Net.Sockets.TcpClient tcpClient = null;// new System.Net.Sockets.TcpClient();
        string IP = "127.0.0.1";
        int PORT = 502;
        byte STATIONID = 1;

        //@LETIAN:20220613:SIMULATION
        bool _isSimulation
        {
            get { return base.IsSimulater; }
            set { base.IsSimulater = value; }
        }
        public bool IsSimulation()
        {
            return _isSimulation;
        }
        #endregion


        public override bool Open(string FileName, bool issimulator)
        {
            //@LETIAN:20220613:SIMULATION
            _isSimulation = issimulator;
            IsSimulater = issimulator;
            IP = ReadINIValue("Communication", "IP", IP, FileName);
            PORT = int.Parse(ReadINIValue("Communication", "PORT", PORT.ToString(), FileName));
            STATIONID = byte.Parse(ReadINIValue("Communication", "STATIONID", STATIONID.ToString(), FileName));
            //modbusTcpClient = new ModbusTcpNet(IP, PORT, STATIONID);
            //omronFinsNet = new OmronFinsNet();
            //omronFinsNet.LogNet = new HslCommunication.LogNet.LogNetSingle("omron.log.txt");

            RetryCount = int.Parse(ReadINIValue("Other", "Retry", RetryCount.ToString(), FileName));
            Timeoutinms = int.Parse(ReadINIValue("Other", "Timeout(ms)", Timeoutinms.ToString(), FileName));
            
            if(!_isSimulation)
                IsSimulater = ReadINIValue("Other", "IsDebug", "0", FileName) == "1";

            return ReOpen();
        }
        private bool ReOpen()
        {
            bool bOK = false;

            try
            {
                if (!IsSimulater)
                {
                    this.components = new System.ComponentModel.Container();
                    m_Compolet = new NJCompolet(this.components);
                    this.m_Compolet.Active = false;
                    this.m_Compolet.ConnectionType = OMRON.Compolet.CIPCompolet64.ConnectionType.Class3;
                    this.m_Compolet.DontFragment = false;
                    this.m_Compolet.LocalPort = 2;
                    this.m_Compolet.PeerAddress = IP;// "192.168.250.1";
                    this.m_Compolet.ReceiveTimeLimit = ((long)(750));
                    this.m_Compolet.RoutePath = "2%192.168.250.1\\1%0";
                    this.m_Compolet.UseRoutePath = false;

                    this.m_Compolet.Active = true;
                    if (!this.m_Compolet.IsConnected)
                        this.m_Compolet.Active = false;
                }

                bOK = true;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                bOK = false;
            }

            IsConnectionFail = !bOK;

            if (_isSimulation)
            {
                bOK = true;
                IsConnectionFail = false;
            }

            if (bOK)
            {
                //if (m_Thread_Hsl == null)
                //{
                //    m_Running = true;
                //    m_Thread_Hsl = new System.Threading.Thread(new System.Threading.ThreadStart(Hsl_BK_Running));
                //    m_Thread_Hsl.Priority = System.Threading.ThreadPriority.Normal;
                //    m_Thread_Hsl.IsBackground = true;
                //    m_Thread_Hsl.Start();
                //}
            }

            return bOK;
        }
        public override void Close()
        {
            if (IsSimulater)
                return;

            try
            {
                m_Running = false;
                if (components != null)
                {
                    components.Dispose();
                }
                if (m_Compolet != null)
                    this.m_Compolet.Active = false;

                if (m_Thread_Hsl != null)
                {
                    //if (m_Thread_Hsl.ThreadState != System.Threading.ThreadState.Stopped)
                    {
                        m_Thread_Hsl.Abort();
                        m_Thread_Hsl = null;
                    }
                }
            }
            catch (Exception ex)
            {

            }

            //base.Close();
        }

        public override void RetryConn()
        {
            base.RetryConn();
            Close();
            ReOpen();
        }

        public IODataClass IOData
        {
            get { return IODataBase; }
        }


        /// <summary>
        /// Polling Function in background thread.
        /// </summary>
        private void Hsl_BK_Running()
        {
            while (m_Running)
            {
                if (IsSimulater)
                {
                    System.Threading.Thread.Sleep(1);
                    fireOnScanned();
                    continue;
                }
                try
                {
                    if (m_Compolet != null)
                    {

                        if (!watch.IsRunning)
                            watch.Start();
                        if (watch.ElapsedMilliseconds > 1000)
                        {
                            watch.Reset();
                            SerialCount = iCount;
                            iCount = 0;
                        }
                        else
                            iCount++;

                        if (RetryIndex > RetryCount)
                        {
                            IsConnectionFail = true;
                            if (!m_error_comm)
                            {
                                m_error_comm = true;
                                CommError(Name);
                            }
                        }
                        else
                        {
                            m_error_comm = false;
                            IsConnectionFail = false;
                        }

                        if (m_error_comm)
                        {
                            iCount = 0;//通訊中斷
                            System.Threading.Thread.Sleep(1);
                            continue;
                        }

                        //if (PLCDuriationTime.msDuriation >= 1000)
                        //{

                        //}
                        //else
                        //{

                        //}


                    }
                    else
                    {
                        System.Threading.Thread.Sleep(1);
                    }
                    fireOnScanned();
                }
                catch (Exception ex)
                {
                    m_error_comm = true;
                    IsConnectionFail = true;
                }
            }
        }

        #region CIPCompolet64
        public void WriteVari(string eVari, string eValue)
        {
            if (IsSimulater)
                return;

            try
            {
                // write
                //--------------------------------------------------------------------------------
                string valWrite = eVari;// this.txtVariableName.Text;
                if (valWrite.StartsWith("_"))
                {
                    MessageBox.Show("The SystemVariable can not write!");
                    return;
                }
                object val = this.RemoveBrackets(eValue);
                if (this.m_Compolet.GetVariableInfo(valWrite).Type == VariableType.STRUCT)
                {
                    val = this.ObjectToByteArray(val);
                }
                this.m_Compolet.WriteVariable(valWrite, val);

                //// read
                //this.btnReadVariable_Click(null, null);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        private object RemoveBrackets(string val)
        {
            object obj = string.Empty;
            if (val.IndexOf("[") >= 0)
            {
                string str = val.Trim('[', ']');
                str = str.Replace("][", ",");
                obj = str.Split(',');
            }
            else
            {
                obj = val;
            }
            return obj;
        }
        private byte[] ObjectToByteArray(object obj)
        {
            if (obj is Array)
            {
                Array arr = obj as Array;
                Byte[] bin = new Byte[arr.Length];
                for (int i = 0; i < bin.Length; i++)
                {
                    bin[i] = Convert.ToByte(arr.GetValue(i));
                }
                return bin;
            }
            else
            {
                return new Byte[1] { Convert.ToByte(obj) };
            }
        }
        public string ReadVari(string eVari)
        {
            string ret = string.Empty;

            if (IsSimulater)
                return ret;


            try
            {
                string varname = eVari;
                object obj = this.m_Compolet.ReadVariable(varname);
                //if (obj == null)
                //{
                //    throw new NotSupportedException();
                //}

                //VariableInfo info = this.njCompolet1.GetVariableInfo(varname);
                string str = this.GetValueOfVariables(obj);
                //if (this.listViewOfVariableNames.SelectedItems.Count > 0)
                //{
                //    if (this.listViewOfVariableNames.SelectedItems[0].SubItems[0].Text == varname)
                //    {
                //        this.listViewOfVariableNames.SelectedItems[0].SubItems.Add(string.Empty);
                //        this.listViewOfVariableNames.SelectedItems[0].SubItems[2].Text = str;
                //    }
                //}
                //this.txtValue.Text = str;
                ret = str;
            }
            catch (Exception ex)
            {
                ret = string.Empty;
                //MessageBox.Show(ex.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return ret;
        }
        private string GetValueOfVariables(object val)
        {
            string valStr = string.Empty;
            if (val.GetType().IsArray)
            {
                Array valArray = val as Array;
                if (valArray.Rank == 1)
                {
                    valStr += "[";
                    foreach (object a in valArray)
                    {
                        valStr += this.GetValueString(a) + ",";
                    }
                    valStr = valStr.TrimEnd(',');
                    valStr += "]";
                }
                else if (valArray.Rank == 2)
                {
                    for (int i = 0; i <= valArray.GetUpperBound(0); i++)
                    {
                        valStr += "[";
                        for (int j = 0; j <= valArray.GetUpperBound(1); j++)
                        {
                            valStr += this.GetValueString(valArray.GetValue(i, j)) + ",";
                        }
                        valStr = valStr.TrimEnd(',');
                        valStr += "]";
                    }
                }
                else if (valArray.Rank == 3)
                {
                    for (int i = 0; i <= valArray.GetUpperBound(0); i++)
                    {
                        for (int j = 0; j <= valArray.GetUpperBound(1); j++)
                        {
                            valStr += "[";
                            for (int z = 0; z <= valArray.GetUpperBound(2); z++)
                            {
                                valStr += this.GetValueString(valArray.GetValue(i, j, z)) + ",";
                            }
                            valStr = valStr.TrimEnd(',');
                            valStr += "]";
                        }
                    }
                }
            }
            else
            {
                valStr = this.GetValueString(val);
            }
            return valStr;
        }
        private string GetValueString(object val)
        {
            if (val is float || val is double)
            {
                return string.Format("{0:R}", val);
            }
            else
            {
                return val.ToString();
            }
        }

        #endregion


        #region PROTECTED_FUNCTION_NOT_USED
        protected override void COMPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            //try
            //{
            //    // 大略是這樣子用....有時指令不會一次傳完，因此要檢查最後回傳的檢查位元
            //    BytesToRead = COMPort.BytesToRead;
            //    COMPort.Read(ReadBuffer, ReadStart, BytesToRead);
            //    ReadStart = ReadStart + BytesToRead;
            //    //
            //    if (Analyze(ReadStart - 1)) //此時 ReadBuffer裏有東西，利用 BytesToRead, ReadStart 及 ReadBuffer來取得所需要的資料
            //    {
            //        base.COMPort_DataReceived(sender, e);

            //        if (Live == "●")
            //            Live = "○";
            //        else
            //            Live = "●";
            //    }
            //}
            //catch (Exception ex)
            //{
            //    base.COMPort_DataReceived(sender, e);

            //    if (Live == "●")
            //        Live = "○";
            //    else
            //        Live = "●";
            //}
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
                SerialCount = iCount;
                iCount = 0;
            }
            else
                iCount++;

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
        #endregion


        public void SetIO(bool IsOn, string ioname)
        {
            if (string.IsNullOrEmpty(ioname))
                return;

            if (_isSimulation)
            {
                //@LETIAN:20220613:SIMULATION
                //暫時利用 event notification 
                //來更新上層的 cache data
                var buf = new bool[] { IsOn };
                OnReadList(buf, "SIM_" + ioname, this.Name);
                return;
            }
            try
            {

            }
            catch (Exception ex)
            {

            }

            //if (IsOn)
            //    Command("Set Bit On", ioname);
            //else
            //    Command("Set Bit Off", ioname);
        }

        #region PRIVATE_ADDR_CONVERT_FUNCTIONS
        string GetHC_Q1_1300D_Address(string bitstr)
        {
            //if (string.IsNullOrEmpty(bitstr))
            //    return false;

            string ret = string.Empty;

            int address = 0;

            string[] strs = bitstr.Substring(2).Split('.');
            if (strs.Length != 2)
                return ret;

            address = int.Parse(strs[0]) * 8 + int.Parse(strs[1]);
            ret = address.ToString();

            return ret;
        }
        ushort GetHC_Q1_1300D_Address_ushort(string bitstr)
        {
            //if (string.IsNullOrEmpty(bitstr))
            //    return false;

            ushort ret = 0;

            ushort address = 0;

            string[] strs = bitstr.Substring(2).Split('.');
            if (strs.Length != 2)
                return ret;

            address = (ushort)(ushort.Parse(strs[0]) * 8 + ushort.Parse(strs[1]));
            ret = address;

            return ret;
        }
        #endregion

        /// <summary>
        /// 发送float型 给plc
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="address">必须设定2位 格式：0:WM0000,MW0001</param>
        public void SetData(float data, AddressClass address)
        {
            //发送给plc数据
            byte[] hex = BitConverter.GetBytes(data);
            byte[] h = new byte[2];
            byte[] l = new byte[2];

            h[0] = hex[0];
            h[1] = hex[1];
            l[0] = hex[2];
            l[1] = hex[3];

            ushort setH = BitConverter.ToUInt16(h, 0);
            ushort setL = BitConverter.ToUInt16(l, 0);

            SetData_ushort(setH, address.Address0);
            SetData_ushort(setL, address.Address1);
        }
        public void SetData(float data, int MWIndex, int word = 2)
        {
            //发送给plc数据
            byte[] hex = BitConverter.GetBytes(data);
            byte[] h = new byte[2];
            byte[] l = new byte[2];

            h[0] = hex[0];
            h[1] = hex[1];
            l[0] = hex[2];
            l[1] = hex[3];

            ushort setH = BitConverter.ToUInt16(h, 0);
            ushort setL = BitConverter.ToUInt16(l, 0);

            SetData_ushort(setH, "MW" + MWIndex.ToString("0000"));
            if (word == 2)
                SetData_ushort(setL, "MW" + (MWIndex + 1).ToString("0000"));
        }
        /// <summary>
        /// 发送int型 给plc
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="address">格式：0:WM0000 或 0:WM0000,MW0001</param>
        public void SetData(int data, AddressClass address)
        {
            long setH = data >> 16;
            long setL = data % 65536;

            SetData(ValueToHEX(setL, 4), address.Address0);
            if (!string.IsNullOrEmpty(address.Address1))
                SetData(ValueToHEX(setH, 4), address.Address1);
        }
        public void SetData(int data, int MWIndex, int word = 2)
        {
            long setH = data >> 16;
            long setL = data % 65536;

            SetData(ValueToHEX(setL, 4), "MW" + MWIndex.ToString("0000"));
            if (word == 2)
                SetData(ValueToHEX(setH, 4), "MW" + (MWIndex + 1).ToString("0000"));
        }

        public void SetDataString(string dataStr, int MWIndex)
        {
            SetDataString(dataStr, "MW" + MWIndex.ToString("0000"));
        }
        public void SetDataString(string dataStr, string ioname)
        {
            if (string.IsNullOrEmpty(ioname))
                return;
            try
            {
                switch (ioname.Substring(0, 2))
                {
                    case "MW":

                        break;
                }
            }
            catch (Exception ex)
            {

            }
            //Command("Set Data", ioname + data);
            //if (modbusTcpClient != null)
            //{
            //    UInt16 intv = HEX16(data);

            //    OperateResult operateResult = modbusTcpClient.Write(ioname, intv);
            //}
        }
        public void SetData(string data, string ioname)
        {
            if (string.IsNullOrEmpty(ioname))
                return;

            if (_isSimulation)
            {
                //@LETIAN:20220613:SIMULATION
                //暫時利用 event notification 
                //來更新上層的 cache data
                UInt16 value = HEX16(data);
                var buf = new Int16[] { (Int16)value };
                OnReadList(buf, "SIM_" + ioname, this.Name);
                return;
            }
            try
            {
                switch (ioname.Substring(0, 2))
                {
                    case "MW":

                        break;
                }
            }
            catch (Exception ex)
            {

            }
            //Command("Set Data", ioname + data);
            //if (modbusTcpClient != null)
            //{
            //    UInt16 intv = HEX16(data);

            //    OperateResult operateResult = modbusTcpClient.Write(ioname, intv);
            //}
        }
        public void SetData_ushort(ushort data, string ioname)
        {
            if (string.IsNullOrEmpty(ioname))
                return;

            if (_isSimulation)
            {
                //@LETIAN:20220613:SIMULATION
                //暫時利用 event notification 
                //來更新上層的 cache data
                UInt16 value = data;// HEX16(data);
                var buf = new Int16[] { (Int16)value };
                OnReadList(buf, "SIM_" + ioname, this.Name);
                return;
            }
            try
            {
                switch (ioname.Substring(0, 2))
                {
                    case "MW":

                        break;
                }
            }
            catch (Exception ex)
            {

            }
        }
        public void GetData(string ioname)
        {
            if (string.IsNullOrEmpty(ioname))
                return;
            try
            {
                switch (ioname.Substring(0, 2))
                {
                    case "MW":

                        break;
                }
            }
            catch (Exception ex)
            {

            }
        }

        #region PRIVATE_ADDR_CONVERT_FUNCTIONS
        string GetHC_Q1_1300D_AddressMW(string bitstr)
        {
            //if (string.IsNullOrEmpty(bitstr))
            //    return false;

            string ret = string.Empty;

            int address = 0;

            if (!int.TryParse(bitstr.Substring(2), out address))
                return ret;
            ret = address.ToString();

            return ret;
        }
        ushort GetHC_Q1_1300D_AddressMW_ushort(string bitstr)
        {
            //if (string.IsNullOrEmpty(bitstr))
            //    return false;

            ushort ret = 0;

            ushort address = 0;

            if (!ushort.TryParse(bitstr.Substring(2), out address))
                return ret;
            ret = address;

            return ret;
        }
        public ushort[] stringToUshort(String inString)

        {

            if (inString.Length % 2 == 1) { inString += " "; }

            char[] bufChar = inString.ToCharArray();

            byte[] outByte = new byte[bufChar.Length];

            byte[] bufByte = new byte[2];

            ushort[] outShort = new ushort[bufChar.Length / 2];

            for (int i = 0, j = 0; i < bufChar.Length; i += 2, j++)

            {

                bufByte[0] = BitConverter.GetBytes(bufChar[i])[0];

                bufByte[1] = BitConverter.GetBytes(bufChar[i + 1])[0];

                outShort[j] = BitConverter.ToUInt16(bufByte, 0);

            }

            return outShort;

        }
        #endregion


        #region NOT_USED_FUNCTIONS
        public void SetData(string data, string ioname, string iocount)
        {
            //Command("Set Data N", iocount + ioname + data);
        }
        #endregion

        #region PROTECTED_FUNCTIONS
        protected override void WriteCommand()
        {
            //if (IsSimulater)
            //    return;

            //string Str = LastCommad.GetSite() + Checksum(LastCommad.GetPLCCommad());
            //COMPort.Write(STX + Str + ETX);
        }
        protected UInt16 HEX16(string HexStr)
        {
            return System.Convert.ToUInt16(HexStr, 16);
        }
        protected UInt32 HEX32(string HexStr)
        {
            return System.Convert.ToUInt32(HexStr, 16);
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
        #endregion


        public override void Tick()
        {
            base.Tick();
        }


        #region EVENT_NOTIFICATIONS_NOT_USED
        ////當有Input Trigger時，產生OnTrigger
        //public delegate void TriggerHandler(string OperationString);
        //public event TriggerHandler TriggerAction;
        //public void OnTrigger(String OperationString)
        //{
        //    if (TriggerAction != null)
        //    {
        //        TriggerAction(OperationString);
        //    }
        //}
        #endregion

        #region EVENT_NOTIFICATIONS_NOT_LAUNCHED
        ////當有Input Read
        //public delegate void ReadHandler(char[] readbuffer, string operationstring, string myname);
        //public event ReadHandler ReadAction;
        //public void OnRead(char[] readbuffer, string operationstring, string myname)
        //{
        //    if (ReadAction != null)
        //    {
        //        ReadAction(readbuffer, operationstring, myname);
        //    }
        //}

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
        #endregion


        ////-----------------------------------------------------------------
        //// Event Notifications for 1-bit points in background polling.
        ////-----------------------------------------------------------------
        //public delegate void ReadListHandler(bool[] readbuffer, string operationstring, string myname);
        //public event ReadListHandler ReadListAction;
        //public void OnReadList(bool[] readbuffer, string operationstring, string myname)
        //{
        //    if (ReadListAction != null)
        //    {
        //        ReadListAction(readbuffer, operationstring, myname);
        //    }
        //}

        ////-----------------------------------------------------------------
        //// Event Notifications for 16-bit registers in background polling.
        ////-----------------------------------------------------------------
        //public delegate void ReadListUintHandler(short[] readbuffer, string operationstring, string myname);
        //public event ReadListUintHandler ReadListUintAction;
        //public void OnReadList(short[] readbuffer, string operationstring, string myname)
        //{
        //    if (ReadListUintAction != null)
        //    {
        //        ReadListUintAction(readbuffer, operationstring, myname);
        //    }
        //}

        ////-----------------------------------------------------------------
        //// Event Notifications for 16-bit registers in background polling.
        ////-----------------------------------------------------------------
        //public delegate void ReadListUshortHandler(ushort[] readbuffer, string operationstring, string myname);
        //public event ReadListUshortHandler ReadListUshortAction;
        //public void OnReadList(ushort[] readbuffer, string operationstring, string myname)
        //{
        //    if (ReadListUshortAction != null)
        //    {
        //        ReadListUshortAction(readbuffer, operationstring, myname);
        //    }
        //}

    }
}

