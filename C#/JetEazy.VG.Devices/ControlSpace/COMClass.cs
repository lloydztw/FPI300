
//#define HC_Q1_1300D

using JetEazy.BasicSpace;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;

namespace JetEazy.ControlSpace
{
    public class IODataClass
    {
        bool[] X = new bool[100];
        bool[] Y = new bool[100];
        bool[] M = new bool[4000];

        int[] R = new int[2000];
        int[] D = new int[2000];

        bool[] IX = new bool[2000];
        bool[] QX = new bool[65535];

        short[] MW = new short[65535];
        bool[] MX = new bool[32000];


        public bool GetBit(string bitstr)
        {

#if HC_Q1_1300D
            return GetHC_Q1_1300D_Bit(bitstr);
#endif

            bool ret = false;

            int address = 0;

            if (!int.TryParse(bitstr.Substring(1), out address))
                return ret;

            switch(bitstr[0])
            {
                case 'X':
                    ret = X[address];
                    break;
                case 'Y':
                    ret = Y[address];
                    break;
                case 'M':
                    ret = M[address];
                    break;
            }

            return ret;
        }

        bool GetHC_Q1_1300D_Bit(string bitstr)
        {
            if (string.IsNullOrEmpty(bitstr))
                return false;

            bool ret = false;

            int address = 0;

            string[] strs = bitstr.Substring(2).Split('.');
            if (strs.Length != 2)
                return ret;

            address = int.Parse(strs[0]) * 8 + int.Parse(strs[1]);

            switch (bitstr.Substring(0, 2))
            {
                case "IX":
                    if (address < IX.Length)
                        ret = IX[address];
                    break;
                case "MX":
                    if (address < MX.Length)
                        ret = MX[address];
                    break;
                case "QX":
                case "QB":
                    if (address < QX.Length)
                        ret = QX[address];
                    break;
            }

            return ret;
        }

        public void SetMBit(int index, bool isone)
        {
            M[index] = isone;
        }
        public void SetXBit(int index, bool isone)
        {
            X[index] = isone;
        }
        public void SetYBit(int index, bool isone)
        {
            Y[index] = isone;
        }
        public void SetQXBit(int index, bool isone)
        {
            QX[index] = isone;
        }
        public void SetIXBit(int index, bool isone)
        {
            IX[index] = isone;
        }
        public void SetMXBit(int index, bool isone)
        {
            MX[index] = isone;
        }
        public int GetData(string bitstr)
        {
#if HC_Q1_1300D
            return GetMW(bitstr);
#endif

            int ret = 0;

            int address = 0;

            if (!int.TryParse(bitstr.Substring(1), out address))
                return ret;

            switch (bitstr[0].ToString().ToUpper())
            {
                case "R":
                    ret = R[address];
                    break;
                case "D":
                    ret = D[address];
                    break;
            }

            return ret;
        }

        public short GetMW(string bitstr)
        {
            short ret = 0;

            if (string.IsNullOrEmpty(bitstr))
                return ret;

            int address = 0;

            if (!int.TryParse(bitstr.Substring(2), out address))
                return ret;

            switch (bitstr.Substring(0, 2).ToUpper())
            {
                case "MW":
                    ret = MW[address];
                    break;
            }

            return ret;
        }
        
        /// <summary>
        /// 读取数据点 带负值
        /// </summary>
        /// <param name="bitstr">数据点位置</param>
        /// <param name="word">位数 1为16位,2为32位</param>
        /// <returns></returns>
        public int GetData(string bitstr,byte word=1)
        {
            int ret = 0;
            int ret2 = 0;

            int address = 0;

            if (!int.TryParse(bitstr.Substring(1), out address))
                return ret;

            
            switch (bitstr[0].ToString().ToUpper())
            {
                case "R":
                    ret = (int)R[address];
                    break;
                case "D":
                    ret = (int)D[address];
                    break;
            }
            if (word == 2)
            {
                switch (bitstr[0].ToString().ToUpper())
                {
                    case "R":
                        ret2 = (int)R[address+1];
                        break;
                    case "D":
                        ret2 = (int)D[address+1];
                        break;
                }
                ret = HEXSigned32(ret2.ToString("X4") + ret.ToString("X4"));
            }
            return ret;
        }

        /// <summary>
        /// 读取数据点的数值
        /// </summary>
        /// <param name="str_address">R点的址</param>
        /// <param name="iWord">需要读取的位数(1指16位，2指32位）</param>
        /// <returns></returns>
        public float GetDataFloat(string str_address, byte iWord=1)
        {
            int ret = 0;
            int address = 0;
            if (!int.TryParse(str_address.Substring(1), out address))
                return ret;
            switch (str_address[0].ToString().ToUpper())
            {
                case "R":
                    ret = R[address];
                    break;
                case "D":
                    ret = D[address];
                    break;
            }

            
             if (iWord == 2)
            {
                int ret2 = 0;
                switch (str_address[0].ToString().ToUpper())
                {
                    case "R":
                        ret2 = R[address + 1];
                        break;
                    case "D":
                        ret2 = D[address + 1];
                        break;
                }

                string str = ret2.ToString("X4") + ret.ToString("X4");
                float flo = HexToFloat(str);

                return flo;
            }
            else 
            {
                string str = ret.ToString("X4");
                float flo = HexToFloat(str);

                return flo;

            }

        }
        
        public void SetRData(int index, int val)
        {
            R[index] = val;
        }
        public void SetDData(int index, int val)
        {
            D[index] = val;
        }
        public void SetMWData(int index, short val)
        {
            MW[index] = val;
        }


        /// <summary>
        /// 16进制转浮点数
        /// </summary>
        /// <param name="str">16进制数</param>
        /// <returns></returns>
        private float HexToFloat(string str)
        {
            uint num1 = uint.Parse(str, System.Globalization.NumberStyles.AllowHexSpecifier);
            byte[] floatvals1 = BitConverter.GetBytes(num1);
            return BitConverter.ToSingle(floatvals1, 0);
        }
        protected string ValueToHEX(long Value, int Length)
        {
            return ("00000000" + Value.ToString("X")).Substring(("00000000" + Value.ToString("X")).Length - Length, Length);
        }
        protected Int32 HEXSigned32(string HexStr)
        {
            return System.Convert.ToInt32(HexStr, 16);
        }
    }
    public class CMDClass
    {
        string NameStr = "";
        string SiteStr = "";
        string CommandStr = "";

        public string refCMDStr = "";

        public CMDClass()
        {

        }

        public CMDClass(string FromString)
        {
            string[] Strs = FromString.Split('=');
            string[] SStrs = Strs[1].Trim().Split('@');

            NameStr = Strs[0].Trim();

            SiteStr = SStrs[0].Trim();
            CommandStr = SStrs[1].Trim();
        }
        public CMDClass Clone()
        {
            CMDClass newCMD = new CMDClass();

            newCMD.NameStr = this.NameStr;
            newCMD.SiteStr = this.SiteStr;
            newCMD.CommandStr = this.CommandStr;
            newCMD.refCMDStr = this.refCMDStr;

            return newCMD;
        }

        public bool CheckCommand(string CommandName)
        {
            return (CommandName.Trim() == NameStr);
        }
        public string GetName()
        {
            return NameStr;
        }
        public string GetPLCCommad()
        {
            return CommandStr.Replace("%", refCMDStr);
        }

        public string GetSite()
        {
            return SiteStr;
        }

    }
    public class IOClass
    {
        public int Index = 0;
        public string IOName = "";
        public string IODescription = "";

        public IOClass(string IOstring)
        {
            string[] strs = IOstring.Split(',');

            Index = int.Parse(strs[0]);
            IOName = strs[1];
            IODescription = strs[2];
        }
    }

    public class COMClass
    {
        enum ReadStatus
        {
            COMMUNICATION,

            INITIAL,
            NORMAL,
            DIRECT,
            OTHER,

            NORMALTEMP,
        }
        
        char Separator = '\x1F';
        
        public bool IsConnectionFail = false;
        //是否需要更新暂存的指令
        /// <summary>
        /// 是否需要更新暂存的指令
        /// </summary>
        public bool isNormalTempNO = false;

        public string Name = "";
        protected JzTimes myTimer = new JzTimes();
        protected SerialPort COMPort;

        protected string comname = "COM1";
        protected int baudrate = 9600;
        protected Parity parity = Parity.Even;
        protected int databit = 7;
        protected StopBits stopbit = StopBits.One;

        //送出命令後，讀回來的資料
        protected int BytesToRead = 0;
        protected int ReadStart = 0;
        protected char[] ReadBuffer = new char[300];
        protected byte[] ReadBufferByte = new byte[1024];
        protected string ReadString = "";

        public IODataClass IODataBase = new IODataClass();

        //命令需求變數
        int InitialIndex = 0; //紀錄 Initial Command 的最大數值
        int NormalIndex = 0;  //紀錄 Normal Command 的最大數值
        int NormalTempIndex = 0;//记录 暂存的 指令
        int NormalTempIndexNow = 0;
        int NormalIndicator = 0; //紀錄目前的Normal Command 跑到那一個了
        int NormalTempIndicator = 0; //紀錄目前的NormalTemp Command 跑到那一個了
        int DirectIndex = 0;  //紀錄 Direct Command 的最大值
        
        //儲存命令的List
        List<CMDClass> CommandList = new List<CMDClass>();

        //暂存命令的List(有需要时才下这些指令)
        List<CMDClass> CommandListToTemp = new List<CMDClass>();

        //排隊等待命令的Queue
        protected Queue<CMDClass> CommandQueue = new Queue<CMDClass>();

        //重送的指令
        protected CMDClass LastCommad;

        protected int RetryCount = 0;
        protected int RetryIndex = 0;
        protected int Timeoutinms = 0;

        protected bool IsWindowClose = false;
        protected bool IsSimulater = false;
        protected string PlcTypeStr = "";
        /// <summary>
        /// PLC每秒通信的数量
        /// </summary>
        public int iCount = 0;

        public int SerialCount = 0;

        //protected JzToolsClass myJzTools = new JzToolsClass();

        public COMClass()
        {

        }

        List<IOClass> IOList = new List<IOClass>();

        public void LoadIO(string PLCIOFile)
        {
            string IOString = "";
            string[] IOstrs;
            IOList.Clear();

            ReadData(ref IOString, PLCIOFile);

            IOString = IOString.Replace(Environment.NewLine, "@");

            IOstrs = IOString.Split('@');

            foreach (string iostr in IOstrs)
            {
                if (iostr.IndexOf(',') > -1)
                {
                    IOList.Add(new IOClass(iostr));
                }
            }
        }
        public string GetIOName(int Index)
        {
            string retStr = "";

            foreach (IOClass io_ in IOList)
            {
                if (io_.Index == Index)
                {
                    retStr = io_.IOName;
                    break;
                }
            }
            return retStr;
        }
        public int GetIONo(int Index)
        {
            string retStr = "";

            foreach (IOClass io_ in IOList)
            {
                if (io_.Index == Index)
                {
                    retStr = io_.IOName;
                    break;
                }
            }
            return int.Parse(retStr.Substring(retStr.Length - 4));
        }

        public string GetIODescription(int Index)
        {
            string retStr = "";

            foreach (IOClass io_ in IOList)
            {
                if (io_.Index == Index)
                {
                    retStr = io_.IODescription;
                    break;
                }
            }
            return retStr;
        }
        public string GetIODescription(string ioname)
        {
            string retStr = "";

            foreach (IOClass io_ in IOList)
            {
                if (io_.IOName == ioname)
                {
                    retStr = io_.IODescription;
                    break;
                }
            }
            return retStr;
        }

        public virtual bool Open(string FileName, bool issimulator)
        {
            bool ret = false;

            string Str = "";
            string[] strs;
            string[] valstr;

            IsSimulater = issimulator;

            ReadStatus myReadStatus = ReadStatus.COMMUNICATION;

            ReadData(ref Str, FileName);

            Str = Str.Replace(Environment.NewLine, Separator.ToString());
            strs = Str.Split(Separator);

            CommandList.Clear();

            foreach (string str in strs)
            {
                #region 檢查是否為註解，空白或是開頭
                if (str == "")
                {
                    continue;
                }
                if (str[0] == '\\')
                {
                    continue;
                }
                if (str.IndexOf("[Communication]") > -1)
                {
                    myReadStatus = ReadStatus.COMMUNICATION;
                    continue;
                }
                else if (str.IndexOf("[Initial") > -1)
                {
                    InitialIndex = 0;

                    myReadStatus = ReadStatus.INITIAL;
                    continue;
                }
                else if (str.IndexOf("[Normal") > -1)
                {
                    NormalIndex = InitialIndex;
                    NormalIndicator = InitialIndex;

                    myReadStatus = ReadStatus.NORMAL;
                    continue;
                }
                else if (str.IndexOf("[TempNormal") > -1)
                {
                    NormalTempIndex = InitialIndex;
                    NormalTempIndicator = InitialIndex;

                    myReadStatus = ReadStatus.NORMALTEMP;
                    continue;
                }
                else if (str.IndexOf("[Direct") > -1)
                {
                    DirectIndex = NormalIndex;

                    myReadStatus = ReadStatus.DIRECT;
                    continue;
                }
                else if (str.IndexOf("[Other") > -1)
                {
                    myReadStatus = ReadStatus.OTHER;
                    continue;
                }

                #endregion

                switch (myReadStatus)
                {
                    case ReadStatus.COMMUNICATION:
                        valstr = str.Split('=');

                        switch (valstr[0].Trim())
                        {
                            case "COM":
                                comname = "COM" + valstr[1].Trim();
                                break;
                            case "Baudrate":
                                baudrate = int.Parse(valstr[1].Trim());
                                break;
                            case "Parity":
                                if (valstr[1].Trim() == "e")
                                    parity = Parity.Even;
                                else if (valstr[1].Trim() == "n")
                                    parity = Parity.None;
                                else if (valstr[1].Trim() == "o")
                                    parity = Parity.Odd;
                                break;
                            case "DataBit":
                                databit = int.Parse(valstr[1].Trim());
                                break;
                            case "StopBit":
                                if (valstr[1].Trim() == "1")
                                    stopbit = StopBits.One;
                                else
                                    stopbit = StopBits.None;
                                break;
                        }
                        break;
                    case ReadStatus.INITIAL:
                        CommandList.Add(new CMDClass(str));
                        InitialIndex++;
                        break;
                    case ReadStatus.NORMAL:
                        CommandList.Add(new CMDClass(str));
                        NormalIndex++;

                        //CommandListToTemp.Add(new CMDClass(str));
                        //NormalTempIndex++;
                        break;
                    case ReadStatus.DIRECT:
                        CommandList.Add(new CMDClass(str));
                        DirectIndex++;
                        break;
                    case ReadStatus.NORMALTEMP:
                        CommandListToTemp.Add(new CMDClass(str));
                        NormalTempIndex++;
                        break;

                    case ReadStatus.OTHER:
                        valstr = str.Split('=');
                        switch (valstr[0].Trim())
                        {
                            case "Timeout(ms)":
                                Timeoutinms = int.Parse(valstr[1].Trim());
                                break;
                            case "Retry":
                                RetryCount = int.Parse(valstr[1].Trim());
                                break;
                            case "IsDebug":
                                IsSimulater = valstr[1].Trim() == "1";
                                break;
                            case "Type":
                                PlcTypeStr = valstr[1].Trim();
                                break;
                        }
                        break;
                }
            }

            if (!IsSimulater)
                ret = Connect();
            else
                ret = true;

            return ret;
        }

        protected virtual void COMPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            IsConnectionFail = false;
            SendCommand();
            RetryIndex = 0;
            ReadStart = 0;
            ReadBuffer = new char[300];
            ReadBufferByte = new byte[1024];
            ReadString = "";
        }

        protected bool Connect()
        {
            bool IsConnectOK = false;

            COMPort = new SerialPort(comname, baudrate, parity, databit, stopbit);

            if (COMPort.IsOpen)
                COMPort.Close();

            try
            {
                COMPort.Open();
                COMPort.DataReceived += new SerialDataReceivedEventHandler(COMPort_DataReceived);

                Start();

                IsConnectOK = true;
            }
            catch (Exception ex)
            {
                //JetEazy.LoggerClass.Instance.WriteException(ex);
                string Estr = ex.ToString();

                IsConnectionFail = true;
                ConnectError();
            }

            return IsConnectOK;
        }

        protected void Start()
        {
            int i = 0;

            while (i < NormalIndex)
            {
                CommandQueue.Enqueue(CommandList[i]);
                i++;
            }

            SendCommand();
        }

        //將命令傳給 COMPort
        protected void SendCommand()
        {
            //if (isTick || RetryIndex == 0)
            {
                if (CommandQueue.Count > 0)
                {
                    LastCommad = CommandQueue.Dequeue();

                    WriteCommand();
                }
                else
                {
                    if (isNormalTempNO)
                    {
                        if (NormalTempIndex != InitialIndex) //防止無 Normal Command 的
                        {
                            if (NormalTempIndicator > NormalTempIndex - 1)
                                NormalTempIndicator = InitialIndex;

                            LastCommad = CommandListToTemp[NormalTempIndicator];

                            NormalTempIndicator++;

                            WriteCommand();
                        }
                    }
                    else
                    {
                        if (NormalIndex != InitialIndex) //防止無 Normal Command 的
                        {
                            if (NormalIndicator > NormalIndex - 1)
                                NormalIndicator = InitialIndex;

                            LastCommad = CommandList[NormalIndicator];

                            NormalIndicator++;

                            WriteCommand();
                        }
                    }
                }
            }

            myTimer.Cut();
        }

        protected virtual void WriteCommand()
        {
            //COMPort.Write(LastCommad.GetCommand(), 0, LastCommad.GetCommand().Length);

        }
        protected virtual string Checksum(string OrgString)
        {
            return "NULL";
        }
        //將命令存在CommandQueue以準備發出去
        protected void Command(string CommandString)
        {
            foreach (CMDClass myCommand in CommandList)
            {
                if (myCommand.CheckCommand(CommandString))
                {
                    CommandQueue.Enqueue(myCommand);
                    break;
                }
            }
        }
        protected void Command(string CommandString, bool On_Off)
        {
            foreach (CMDClass myCommand in CommandList)
            {
                if (myCommand.CheckCommand(CommandString))
                {
                    //myCommand要如何把 On_Off值加進去，請思考
                    CommandQueue.Enqueue(myCommand);
                    break;
                }
            }
        }

        protected void Command(string CommandString, string refStr)
        {
            foreach (CMDClass myCommand in CommandList)
            {
                if (myCommand.CheckCommand(CommandString))
                {
                    myCommand.refCMDStr = refStr;
                    CommandQueue.Enqueue(myCommand.Clone());
                    break;
                }
            }
        }

        public virtual void Close()
        {
            if (IsSimulater)
                return;
            if (COMPort != null)
            {
                if (COMPort.IsOpen)
                    COMPort.Close();
            }
        }
        public virtual void SetNormalTemp(bool ebTemp)
        {
            isNormalTempNO = ebTemp;
        }

        public virtual void SetData_ushort(ushort data, string ioname)
        {
            if (string.IsNullOrEmpty(ioname))
                return;
        }
        public virtual void SetData(float data, int MWIndex, int word = 2)
        {
            
        }
        public virtual void SetData(int data, int MWIndex, int word = 2)
        {

        }
        public virtual void SetData(float data, AddressClass address)
        {

        }
        public virtual void SetData(int data, AddressClass address)
        {

        }
        public virtual void GetData(string ioname)
        {
            if (string.IsNullOrEmpty(ioname))
                return;
        }

        public virtual void RetryConn()
        {
            RetryIndex = 0;
        }

        public virtual void Tick()
        {
            if (IsSimulater)
                return;

            if (RetryIndex > RetryCount)
            {
                IsConnectionFail = true;
                iCount = 0;
                CommError(Name);
            }
            else
            {
                if (myTimer.msDuriation > Timeoutinms)
                {
                    SendCommand();
                    RetryIndex++;
                }
            }
        }

        protected Int32 HEX(string HexStr)
        {
            return System.Convert.ToInt32(HexStr, 16);
        }
        protected string Boolean(string HexStr, int Length)
        {
            int i = 0;
            string istr = ("00000000" + HexStr).Substring(("00000000" + HexStr).Length - Length, Length);
            string jstr = "";

            for (i = 0; i < Length; i++)
            {
                switch (istr.Substring(i, 1))
                {
                    case "0":
                        jstr = jstr + "0000";
                        break;
                    case "1":
                        jstr = jstr + "0001";
                        break;
                    case "2":
                        jstr = jstr + "0010";
                        break;
                    case "3":
                        jstr = jstr + "0011";
                        break;
                    case "4":
                        jstr = jstr + "0100";
                        break;
                    case "5":
                        jstr = jstr + "0101";
                        break;
                    case "6":
                        jstr = jstr + "0110";
                        break;
                    case "7":
                        jstr = jstr + "0111";
                        break;
                    case "8":
                        jstr = jstr + "1000";
                        break;
                    case "9":
                        jstr = jstr + "1001";
                        break;
                    case "A":
                        jstr = jstr + "1010";
                        break;
                    case "B":
                        jstr = jstr + "1011";
                        break;
                    case "C":
                        jstr = jstr + "1100";
                        break;
                    case "D":
                        jstr = jstr + "1101";
                        break;
                    case "E":
                        jstr = jstr + "1110";
                        break;
                    case "F":
                        jstr = jstr + "1111";
                        break;
                }
            }

            return jstr;
        }
        protected string ValueToHEX(long Value, int Length)
        {
            return ("00000000" + Value.ToString("X")).Substring(("00000000" + Value.ToString("X")).Length - Length, Length);
        }

        private void ReadData(ref string DataStr, string FileName)
        {
            if (!File.Exists(FileName))
                return;

            FileStream fs = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.None);
            StreamReader Srr = new StreamReader(fs, Encoding.Default);

            DataStr = Srr.ReadToEnd();

            Srr.Close();
            Srr.Dispose();
        }

        public void StartWindowClose()
        {
            IsWindowClose = true;
        }

        public delegate void ConnectErrorHandler();
        public event ConnectErrorHandler ConnectErrorAction;
        public void ConnectError()
        {
            if (ConnectErrorAction != null)
            {
                ConnectErrorAction();
            }
        }

        public delegate void CommErrorHandler();
        public event CommErrorHandler CommErrorAction;
        public void CommError()
        {
            if (CommErrorAction != null)
            {
                CommErrorAction();
            }
        }
        public delegate void CommErrorStringHandler(string str);
        public event CommErrorStringHandler CommErrorStringAction;
        public void CommError(string str)
        {
            if (CommErrorStringAction != null)
            {
                CommErrorStringAction(str);
            }
        }

        /// <summary>
        /// Client 端可以使用此 event 確認, IO點位的 cache data 已經被掃描更新.
        /// <br/> LETIAN:20221021 
        /// </summary>
        public event EventHandler OnScanned;
        protected void fireOnScanned()
        {
            OnScanned?.Invoke(this, EventArgs.Empty);
        }

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

        //-----------------------------------------------------------------
        // Event Notifications for 16-bit registers in background polling.
        //-----------------------------------------------------------------
        public delegate void ReadListUshortHandler(ushort[] readbuffer, string operationstring, string myname);
        public event ReadListUshortHandler ReadListUshortAction;
        public void OnReadList(ushort[] readbuffer, string operationstring, string myname)
        {
            if (ReadListUshortAction != null)
            {
                ReadListUshortAction(readbuffer, operationstring, myname);
            }
        }

    }
   
}
