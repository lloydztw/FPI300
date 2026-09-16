//#define ZIHAO
#define MAIN


using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Modbus.Device;
using Modbus.Message;

namespace JetEazy.ControlSpace.PLCSpace
{
    public class ModbusTcpFromNNModbus4 : COMClass
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
        private IModbusMaster master = null;
        System.Net.Sockets.TcpClient tcpClient = null;// new System.Net.Sockets.TcpClient();
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

            return ReOpen();
        }
        private bool ReOpen()
        {
            bool bOK = false;

            try
            {
                if (tcpClient == null)
                {
                    tcpClient = new System.Net.Sockets.TcpClient();
                    tcpClient.Connect(IP, PORT);
                }

                //创建ModbusRTU主站实例
                master = ModbusIpMaster.CreateIp(tcpClient);
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
                if (tcpClient != null)
                {
                    tcpClient.Close();
                    tcpClient.Dispose();
                    tcpClient = null;
                }
                if (master != null)
                {
                    master.Dispose();
                    master = null;
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
                    if (master != null)
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
                        bool[] myRead = master.ReadCoils((byte)STATIONID, (ushort)(iQXAdress * 8), (ushort)64);
                        OnReadList(myRead, "Get QX" + iQXAdress.ToString(), Name);

                        //iQXAdress = 4096;
                        //myRead = master.ReadCoils((byte)STATIONID, (ushort)(iQXAdress * 8), (ushort)1024);
                        //OnReadList(myRead, "Get QX" + iQXAdress.ToString(), Name);

                        //iQXAdress = 4224;
                        //myRead = master.ReadCoils((byte)STATIONID, (ushort)(iQXAdress * 8), (ushort)1024);
                        //OnReadList(myRead, "Get QX" + iQXAdress.ToString(), Name);

                        //iQXAdress = 6000;
                        //myRead = master.ReadCoils((byte)STATIONID, (ushort)(iQXAdress * 8), (ushort)1024);
                        //OnReadList(myRead, "Get QX" + iQXAdress.ToString(), Name);

                        iQXAdress = 6128;
                        myRead = master.ReadCoils((byte)STATIONID, (ushort)(iQXAdress * 8), (ushort)1024);
                        OnReadList(myRead, "Get QX" + iQXAdress.ToString(), Name);

                        iQXAdress = 6256;
                        myRead = master.ReadCoils((byte)STATIONID, (ushort)(iQXAdress * 8), (ushort)1024);
                        OnReadList(myRead, "Get QX" + iQXAdress.ToString(), Name);

                        iQXAdress = 6384;
                        myRead = master.ReadCoils((byte)STATIONID, (ushort)(iQXAdress * 8), (ushort)1024);
                        OnReadList(myRead, "Get QX" + iQXAdress.ToString(), Name);

                        iQXAdress = 6512;
                        myRead = master.ReadCoils((byte)STATIONID, (ushort)(iQXAdress * 8), (ushort)1024);
                        OnReadList(myRead, "Get QX" + iQXAdress.ToString(), Name);


                        //读取所有位置(MW 1400 ~1430)
                        int iMWAddress = 2350;
                        ushort[] myReadMW = master.ReadHoldingRegisters(STATIONID, (ushort)iMWAddress, 15);
                        OnReadList(myReadMW, "Get MW" + iMWAddress.ToString(), Name);

                        ////手动速度
                        //iMWAddress = 1040;
                        //myReadMW = master.ReadHoldingRegisters(STATIONID, (ushort)iMWAddress, 15);
                        //OnReadList(myReadMW, "Get MW" + iMWAddress.ToString(), Name);

                        //定位速度
                        iMWAddress = 1500;
                        myReadMW = master.ReadHoldingRegisters(STATIONID, (ushort)iMWAddress, 15);
                        OnReadList(myReadMW, "Get MW" + iMWAddress.ToString(), Name);

                        //光栅尺
                        iMWAddress = 7020;
                        myReadMW = master.ReadHoldingRegisters(STATIONID, (ushort)iMWAddress, 15);
                        OnReadList(myReadMW, "Get MW" + iMWAddress.ToString(), Name);

                        #region MAIN IO



                        #endregion

                        #region ZIHAO IO

#if ZIHAO

                        ////QX  ReadBool : 0*8 (+1024)
                        //int iQXAdress = 0;
                        //bool[] myRead = master.ReadCoils((byte)STATIONID, (ushort)(iQXAdress * 8), (ushort)1024);
                        //OnReadList(myRead, "Get QX" + iQXAdress.ToString(), Name);

                        //QX: ReadBool : 1016*8 (+1024)
                        iQXAdress = 1016;
                        myRead = master.ReadCoils((byte)STATIONID, (ushort)(iQXAdress * 8), (ushort)1024);
                        OnReadList(myRead, "Get QX" + iQXAdress.ToString(), Name);

                        //QX: ReadBool : 1144*8 (+1024)
                        iQXAdress = 1144;
                        myRead = master.ReadCoils((byte)STATIONID, (ushort)(iQXAdress * 8), (ushort)1024);
                        OnReadList(myRead, "Get QX" + iQXAdress.ToString(), Name);

                        //QX: ReadBool : 1272*8 (+1024)
                        iQXAdress = 1272;
                        myRead = master.ReadCoils((byte)STATIONID, (ushort)(iQXAdress * 8), (ushort)1024);
                        OnReadList(myRead, "Get QX" + iQXAdress.ToString(), Name);

                        //QX: ReadBool : 1400*8 (+1024)
                        iQXAdress = 1400;
                        myRead = master.ReadCoils((byte)STATIONID, (ushort)(iQXAdress * 8), (ushort)1024);
                        OnReadList(myRead, "Get QX" + iQXAdress.ToString(), Name);

                        //QX: ReadBool : 1528*8 (+1024)
                        iQXAdress = 1528;
                        myRead = master.ReadCoils((byte)STATIONID, (ushort)(iQXAdress * 8), (ushort)1024);
                        OnReadList(myRead, "Get QX" + iQXAdress.ToString(), Name);

                        //QX: ReadBool : 1656*8 (+1024)
                        iQXAdress = 1656;
                        myRead = master.ReadCoils((byte)STATIONID, (ushort)(iQXAdress * 8), (ushort)1024);
                        OnReadList(myRead, "Get QX" + iQXAdress.ToString(), Name);

                        //QX: ReadBool : 1784*8 (+1024)
                        iQXAdress = 1784;
                        myRead = master.ReadCoils((byte)STATIONID, (ushort)(iQXAdress * 8), (ushort)1024);
                        OnReadList(myRead, "Get QX" + iQXAdress.ToString(), Name);

                        iQXAdress = 2296;
                        myRead = master.ReadCoils((byte)STATIONID, (ushort)(iQXAdress * 8), (ushort)1024);
                        OnReadList(myRead, "Get QX" + iQXAdress.ToString(), Name);


                        ////读取所有位置(MW 1400 ~1430)
                        //int iMWAddress = 1400;
                        //ushort[] myReadMW = master.ReadHoldingRegisters(STATIONID, (ushort)iMWAddress, 30);
                        //OnReadList(myReadMW, "Get MW" + iMWAddress.ToString(), Name);

                        iMWAddress = 1060;
                        myReadMW = master.ReadHoldingRegisters(STATIONID, (ushort)iMWAddress, 15);
                        OnReadList(myReadMW, "Get MW" + iMWAddress.ToString(), Name);

                        iMWAddress = 1540;
                        myReadMW = master.ReadHoldingRegisters(STATIONID, (ushort)iMWAddress, 15);
                        OnReadList(myReadMW, "Get MW" + iMWAddress.ToString(), Name);

                        iMWAddress = 1636;
                        myReadMW = master.ReadHoldingRegisters(STATIONID, (ushort)iMWAddress, 15);
                        OnReadList(myReadMW, "Get MW" + iMWAddress.ToString(), Name);

                        iMWAddress = 2016;
                        myReadMW = master.ReadHoldingRegisters(STATIONID, (ushort)iMWAddress, 15);
                        OnReadList(myReadMW, "Get MW" + iMWAddress.ToString(), Name);

                        //报警信息
                        iMWAddress = 0;
                        myReadMW = master.ReadHoldingRegisters(STATIONID, (ushort)iMWAddress, 15);
                        OnReadList(myReadMW, "Get MW" + iMWAddress.ToString(), Name);

#endif

                        #endregion

                        #region 输入点位

                        //IX: ReadDiscrete: 0 ~16*8
                        int iIXAdress = 0;
                        myRead = master.ReadInputs((byte)STATIONID, (ushort)(iIXAdress * 8), (ushort)(16 * 8));
                        OnReadList(myRead, "Get IX" + iIXAdress.ToString(), Name);

                        #endregion

                        RetryIndex = 0;

                        //当前位置 & 当前速度 (MW 1300 ~ 1329)
                        //iMWAddress = 1300;
                        //if (_results.IsSuccess)
                        //{
                        //    OperateResult<short[]> op_result_uint = modbusTcpClient.ReadInt16(iMWAddress.ToString(), 30);

                        //    if (op_result_uint.IsSuccess)
                        //    {
                        //        RetryIndex = 0;

                        //        short[] myRead = new short[op_result_uint.Content.Length];
                        //        myRead = op_result_uint.Content;

                        //        OnReadList(myRead, "Get MW" + iMWAddress.ToString(), Name);
                        //    }
                        //}

                        //当前位置 & 当前速度 (MW 0000 ~ 0004)
                        //iMWAddress = 0;
                        //if (_results.IsSuccess)
                        //{
                        //    OperateResult<short[]> op_result_uint = modbusTcpClient.ReadInt16(iMWAddress.ToString(), 5);

                        //    if (op_result_uint.IsSuccess)
                        //    {
                        //        RetryIndex = 0;

                        //        short[] myRead = new short[op_result_uint.Content.Length];
                        //        myRead = op_result_uint.Content;

                        //        OnReadList(myRead, "Get MW" + iMWAddress.ToString(), Name);
                        //    }
                        //}

                        //IX: ReadDiscrete: 0 ~ 32
                        //int iIXAdress = 0;
                        //if (_results.IsSuccess)
                        //{
                        //    //OperateResult<bool[]> _results = modbusTcpClient.ReadDiscrete((iIXAdress * 8).ToString(), 24);
                        //    _results = modbusTcpClient.ReadDiscrete((iIXAdress * 8).ToString(), 32);
                        //    if (_results.IsSuccess)
                        //    {
                        //        RetryIndex = 0;

                        //        bool[] myRead = new bool[_results.Content.Length];
                        //        myRead = _results.Content;

                        //        OnReadList(myRead, "Get IX" + iIXAdress.ToString(), Name);
                        //    }
                        //}

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
                switch (ioname.Substring(0, 2))
                {
                    case "QX":
                    case "QB":
                        if (master != null)
                        {
                            if (IsConnectionFail || m_error_comm)
                            {
                            }
                            else
                            {
                                //string addr = GetHC_Q1_1300D_Address(ioname);
                                //OperateResult operateResult = modbusTcpClient.Write(addr, IsOn);
                                //master.WriteSingleRegister()
                                ushort addr = GetHC_Q1_1300D_Address_ushort(ioname);
                                master.WriteSingleCoil(STATIONID, addr, IsOn);
                            }
                        }
                        else
                        {
                        }
                        break;
                }
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
                        if (master != null)
                        {
                            if (IsConnectionFail || m_error_comm)
                            {
                            }
                            else
                            {
                                ushort addr = GetHC_Q1_1300D_AddressMW_ushort(ioname);
                                ushort[] result = stringToUshort(dataStr).ToArray();
                                master.WriteMultipleRegisters(STATIONID, addr, result);
                            }
                        }
                        else
                        {

                        }
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
                        if (master != null)
                        {
                            if (IsConnectionFail || m_error_comm)
                            {
                            }
                            else
                            {
                                //string addr = GetHC_Q1_1300D_AddressMW(ioname);
                                //UInt16 value = HEX16(data);
                                //OperateResult operateResult = modbusTcpClient.Write(addr, value);

                                ushort addr = GetHC_Q1_1300D_AddressMW_ushort(ioname);
                                UInt16 value = HEX16(data);
                                //OperateResult operateResult = modbusTcpClient.Write(addr, value);
                                master.WriteSingleRegister(STATIONID, addr, value);
                            }
                        }
                        else
                        {

                        }
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
                        if (master != null)
                        {
                            if (IsConnectionFail || m_error_comm)
                            {
                            }
                            else
                            {
                                ushort addr = GetHC_Q1_1300D_AddressMW_ushort(ioname);
                                UInt16 value = data;// HEX16(data);
                                                    //OperateResult operateResult = modbusTcpClient.Write(addr, value);
                                master.WriteSingleRegister(STATIONID, addr, value);
                            }
                        }
                        else
                        {

                        }
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
                        if (master != null)
                        {
                            if (IsConnectionFail || m_error_comm)
                            {
                            }
                            else
                            {
                                ushort addr = GetHC_Q1_1300D_AddressMW_ushort(ioname);
                                ushort[] ushorts = new ushort[1];
                                ushorts = master.ReadHoldingRegisters(STATIONID, (ushort)addr, 1);
                                IOData.SetMWData((int)addr, (short)ushorts[0]);
                            }
                        }
                        else
                        {

                        }
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
