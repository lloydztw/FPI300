using HslCommunication;
//using HslCommunication.Profinet.Omron;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using HslCommunication.ModBus;

namespace JetEazy.ControlSpace.PLCSpace
{
    public class Modbus_From_HSL : COMClass
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

        //private OmronFinsNet omronFinsNet;
        private ModbusTcpNet modbusTcpClient = null;
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
            modbusTcpClient = new ModbusTcpNet(IP, PORT, STATIONID);
            //omronFinsNet = new OmronFinsNet();
            //omronFinsNet.LogNet = new HslCommunication.LogNet.LogNetSingle("omron.log.txt");

            RetryCount = int.Parse(ReadINIValue("Other", "Retry", RetryCount.ToString(), FileName));
            Timeoutinms = int.Parse(ReadINIValue("Other", "Timeout(ms)", Timeoutinms.ToString(), FileName));

            bool bOK = false;

            try
            {
                //omronFinsNet.IpAddress = "192.168.0.99";
                //omronFinsNet.Port = 9600;

                this.modbusTcpClient.ByteTransform.DataFormat = HslCommunication.Core.DataFormat.ABCD;
                //this.modbusTcpClient.SA1 = 0;
                //this.omronFinsNet.DA2 = 0;
                //omronFinsNet.IsChangeSA1AfterReadFailed = false;
                modbusTcpClient.AddressStartWithZero = true;
                modbusTcpClient.ConnectTimeOut = 1000;
                modbusTcpClient.ReceiveTimeOut = 1000;

                OperateResult connect = modbusTcpClient.ConnectServer();

                //if (connect.IsSuccess)
                //{
                //    MessageBox.Show("连接成功！");
                //}
                //else
                //{
                //    MessageBox.Show("连接失败！");
                //}
                bOK = connect.IsSuccess;
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
            else
            {
                if (m_Thread_Hsl == null)
                {
                    m_Running = true;
                    m_Thread_Hsl = new System.Threading.Thread(new System.Threading.ThreadStart(Hsl_BK_Running));
                    m_Thread_Hsl.Priority = System.Threading.ThreadPriority.Normal;
                    m_Thread_Hsl.IsBackground = true;
                    m_Thread_Hsl.Start();
                }
            }

            return bOK;
        }
        public override void Close()
        {
            try
            {
                m_Running = false;
                if (modbusTcpClient != null)
                {
                    modbusTcpClient.ConnectClose();
                }
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
                    continue;
                }
                  

                if (modbusTcpClient != null)
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

                    //QX  ReadBool : 0*8 (+1024)
                    int iQXAdress = 0;
                    OperateResult<bool[]> _results = modbusTcpClient.ReadBool((iQXAdress * 8).ToString(), 1024);
                    if (_results.IsSuccess)
                    {
                        RetryIndex = 0;

                        bool[] myRead = new bool[_results.Content.Length];
                        myRead = _results.Content;

                        OnReadList(myRead, "Get QX" + iQXAdress.ToString(), Name);
                    }
                    else
                    {
                        RetryIndex++;
                    }

                    //QX: ReadBool : 1016*8 (+1024)
                    if (_results.IsSuccess)
                    {
                        iQXAdress = 1016;
                        _results = modbusTcpClient.ReadBool((iQXAdress * 8).ToString(), 1024);

                        if (_results.IsSuccess)
                        {
                            RetryIndex = 0;

                            bool[] myRead = new bool[_results.Content.Length];
                            myRead = _results.Content;

                            OnReadList(myRead, "Get QX" + iQXAdress.ToString(), Name);
                        }
                    }
                    //QX: ReadBool : 1144*8 (+1024)
                    if (_results.IsSuccess)
                    {
                        iQXAdress = 1144;
                        _results = modbusTcpClient.ReadBool((iQXAdress * 8).ToString(), 1024);

                        if (_results.IsSuccess)
                        {
                            RetryIndex = 0;

                            bool[] myRead = new bool[_results.Content.Length];
                            myRead = _results.Content;

                            OnReadList(myRead, "Get QX" + iQXAdress.ToString(), Name);
                        }
                    }
                    //QX: ReadBool : 1272*8 (+1024)
                    if (_results.IsSuccess)
                    {
                        iQXAdress = 1272;
                        _results = modbusTcpClient.ReadBool((iQXAdress * 8).ToString(), 1024);

                        if (_results.IsSuccess)
                        {
                            RetryIndex = 0;

                            bool[] myRead = new bool[_results.Content.Length];
                            myRead = _results.Content;

                            OnReadList(myRead, "Get QX" + iQXAdress.ToString(), Name);
                        }
                    }
                    //QX: ReadBool : 1400*8 (+1024)
                    if (_results.IsSuccess)
                    {
                        iQXAdress = 1400;
                        _results = modbusTcpClient.ReadBool((iQXAdress * 8).ToString(), 1024);

                        if (_results.IsSuccess)
                        {
                            RetryIndex = 0;

                            bool[] myRead = new bool[_results.Content.Length];
                            myRead = _results.Content;

                            OnReadList(myRead, "Get QX" + iQXAdress.ToString(), Name);
                        }
                    }
                    //QX: ReadBool : 1528*8 (+1024)
                    if (_results.IsSuccess)
                    {
                        iQXAdress = 1528;
                        _results = modbusTcpClient.ReadBool((iQXAdress * 8).ToString(), 1024);

                        if (_results.IsSuccess)
                        {
                            RetryIndex = 0;

                            bool[] myRead = new bool[_results.Content.Length];
                            myRead = _results.Content;

                            OnReadList(myRead, "Get QX" + iQXAdress.ToString(), Name);
                        }
                    }
                    //QX: ReadBool : 1656*8 (+1024)
                    if (_results.IsSuccess)
                    {
                        iQXAdress = 1656;
                        _results = modbusTcpClient.ReadBool((iQXAdress * 8).ToString(), 1024);

                        if (_results.IsSuccess)
                        {
                            RetryIndex = 0;

                            bool[] myRead = new bool[_results.Content.Length];
                            myRead = _results.Content;

                            OnReadList(myRead, "Get QX" + iQXAdress.ToString(), Name);
                        }
                    }
                    //QX: ReadBool : 1784*8 (+1024)
                    if (_results.IsSuccess)
                    {
                        iQXAdress = 1784;
                        _results = modbusTcpClient.ReadBool((iQXAdress * 8).ToString(), 1024);

                        if (_results.IsSuccess)
                        {
                            RetryIndex = 0;

                            bool[] myRead = new bool[_results.Content.Length];
                            myRead = _results.Content;

                            OnReadList(myRead, "Get QX" + iQXAdress.ToString(), Name);
                        }
                    }

                    //读取所有位置  (MW 1000 ~ 1099)
                    int iMWAddress = 1000;
                    if (_results.IsSuccess)
                    {
                        //if (isNormalTempNO)
                        {
                            OperateResult<short[]> op_result_uint = modbusTcpClient.ReadInt16(iMWAddress.ToString(), 100);

                            if (op_result_uint.IsSuccess)
                            {
                                RetryIndex = 0;

                                short[] myRead = new short[op_result_uint.Content.Length];
                                myRead = op_result_uint.Content;

                                OnReadList(myRead, "Get MW" + iMWAddress.ToString(), Name);
                            }
                        }
                    }

                    //当前位置 & 当前速度 (MW 1300 ~ 1329)
                    iMWAddress = 1300;
                    if (_results.IsSuccess)
                    {
                        OperateResult<short[]> op_result_uint = modbusTcpClient.ReadInt16(iMWAddress.ToString(), 30);

                        if (op_result_uint.IsSuccess)
                        {
                            RetryIndex = 0;

                            short[] myRead = new short[op_result_uint.Content.Length];
                            myRead = op_result_uint.Content;

                            OnReadList(myRead, "Get MW" + iMWAddress.ToString(), Name);
                        }
                    }

                    //当前位置 & 当前速度 (MW 0000 ~ 0004)
                    iMWAddress = 0;
                    if (_results.IsSuccess)
                    {
                        OperateResult<short[]> op_result_uint = modbusTcpClient.ReadInt16(iMWAddress.ToString(), 5);

                        if (op_result_uint.IsSuccess)
                        {
                            RetryIndex = 0;

                            short[] myRead = new short[op_result_uint.Content.Length];
                            myRead = op_result_uint.Content;

                            OnReadList(myRead, "Get MW" + iMWAddress.ToString(), Name);
                        }
                    }

                    //IX: ReadDiscrete: 0 ~ 32
                    int iIXAdress = 0;
                    if (_results.IsSuccess)
                    {
                        //OperateResult<bool[]> _results = modbusTcpClient.ReadDiscrete((iIXAdress * 8).ToString(), 24);
                        _results = modbusTcpClient.ReadDiscrete((iIXAdress * 8).ToString(), 32);
                        if (_results.IsSuccess)
                        {
                            RetryIndex = 0;

                            bool[] myRead = new bool[_results.Content.Length];
                            myRead = _results.Content;

                            OnReadList(myRead, "Get IX" + iIXAdress.ToString(), Name);
                        }
                    }

                    //////当前速度
                    //iMWAddress = 1320;
                    //if (_results.IsSuccess)
                    //{
                    //    OperateResult<short[]> op_result_uint = modbusTcpClient.ReadInt16(iMWAddress.ToString(), 9);

                    //    if (op_result_uint.IsSuccess)
                    //    {
                    //        RetryIndex = 0;

                    //        short[] myRead = new short[op_result_uint.Content.Length];
                    //        myRead = op_result_uint.Content;

                    //        OnReadList(myRead, "Get MW" + iMWAddress.ToString(), Name);
                    //    }
                    //}




                }
            }
        }


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

            switch (ioname.Substring(0, 2))
            {
                case "QX":
                case "QB":
                    if (modbusTcpClient != null)
                    {
                        if (IsConnectionFail || m_error_comm)
                        {
                        }
                        else
                        {
                            string addr = GetHC_Q1_1300D_Address(ioname);
                            OperateResult operateResult = modbusTcpClient.Write(addr, IsOn);
                        }
                    }
                    else
                    {
                    }
                    break;
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
        #endregion

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

            switch (ioname.Substring(0, 2))
            {
                case "MW":
                    if (modbusTcpClient != null)
                    {
                        if (IsConnectionFail || m_error_comm)
                        {
                        }
                        else
                        {
                            string addr = GetHC_Q1_1300D_AddressMW(ioname);
                            UInt16 value = HEX16(data);
                            OperateResult operateResult = modbusTcpClient.Write(addr, value);
                        }
                    }
                    else
                    {

                    }
                    break;
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

            switch (ioname.Substring(0, 2))
            {
                case "MW":
                    if (modbusTcpClient != null)
                    {
                        if (IsConnectionFail || m_error_comm)
                        {
                        }
                        else
                        {
                            string addr = GetHC_Q1_1300D_AddressMW(ioname);
                            UInt16 value = data;// HEX16(data);
                            OperateResult operateResult = modbusTcpClient.Write(addr, value);
                        }
                    }
                    else
                    {

                    }
                    break;
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


        //-----------------------------------------------------------------
        // Event Notifications for 1-bit points in background polling.
        //-----------------------------------------------------------------
        public delegate void ReadListHandler(bool[] readbuffer, string operationstring, string myname);
        public event ReadListHandler ReadListAction;
        public void OnReadList(bool[] readbuffer, string operationstring, string myname)
        {
            if (ReadListAction != null)
            {
                ReadListAction(readbuffer, operationstring, myname);
            }
        }

        //-----------------------------------------------------------------
        // Event Notifications for 16-bit registers in background polling.
        //-----------------------------------------------------------------
        public delegate void ReadListUintHandler(short[] readbuffer, string operationstring, string myname);
        public event ReadListUintHandler ReadListUintAction;
        public void OnReadList(short[] readbuffer, string operationstring, string myname)
        {
            if (ReadListUintAction != null)
            {
                ReadListUintAction(readbuffer, operationstring, myname);
            }
        }
    }
}
