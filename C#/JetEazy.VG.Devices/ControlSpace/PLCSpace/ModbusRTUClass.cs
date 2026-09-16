using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JetEazy.ControlSpace.PLCSpace
{
    public class ModbusRTUClass : COMClass
    {
        #region PRIVATE DATA MEMBERS
        private char STX = '\x02';
        private char ETX = '\x03';

        bool[] X = new bool[100];
        bool[] Y = new bool[100];
        bool[] M = new bool[1000];

        bool[] A = new bool[500];

        int iCountTemp = 0;
        System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        #endregion

        public string Live = "●";

        #region PROTECTED FUNTION 
        protected override void COMPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            try
            {
                // 大略是這樣子用....有時指令不會一次傳完，因此要檢查最後回傳的檢查位元
                BytesToRead = COMPort.BytesToRead;
                COMPort.Read(ReadBufferByte, ReadStart, BytesToRead);
                ReadStart = ReadStart + BytesToRead;
                //
                bool bOK = Analyze(ReadStart - 1);
                //if (bOK) //此時 ReadBuffer裏有東西，利用 BytesToRead, ReadStart 及 ReadBuffer來取得所需要的資料
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

            //计算返回数据的CRC
            byte[] bytecrc = Crc(ReadBufferByte, 0, (UInt32)(LastIndex - 1));
            string checkCrc = ByteToHexStr(bytecrc);

            byte[] retCrc = new byte[2];
            retCrc[0] = ReadBufferByte[LastIndex - 1];
            retCrc[1] = ReadBufferByte[LastIndex];
            string retStrCrc = ByteToHexStr(retCrc);
            if (checkCrc != retStrCrc)
            {
                return ret;
            }
            else
                ret = true;

            if (!watch.IsRunning)
                watch.Start();
            if (watch.ElapsedMilliseconds > 1000)
            {
                watch.Reset();
                iCount = iCountTemp;
                iCountTemp = 0;
            }
            else
                iCountTemp++;

            ReadString = ByteToHexStr(ReadBufferByte, 0, LastIndex);

            switch (LastCommad.GetName())
            {
                case "Get X0-10":
                    GetX0_10();
                    break;
                case "Get M0-10":
                    GetM0_10();
                    break;
                case "Get M100-5":
                    GetM100_5();
                    break;
                default:
                    break;
            }

            return ret;
        }
        protected void GetX0_10()
        {
            string strCount = ReadString.Substring(4, 2);
            Int32 GetInt = HEX(strCount);
            string strCC = ReadString.Substring(6, 2);
            Int32 GetIntCC = HEX(strCC);

            int i = 0;
            int j = 0;
            while (i < GetInt)
            {
                strCC = ReadString.Substring(6 + i * 2, 2);
                GetIntCC = HEX(strCC);

                j = 0;
                while (j < 8)
                {
                    X[j + i * 8] = (GetIntCC & (1 << j)) == (1 << j);
                    j++;
                }

                i++;
            }
        }
        protected void GetM0_10()
        {
            string strCount = ReadString.Substring(4, 2);
            Int32 GetInt = HEX(strCount);
            string strCC = ReadString.Substring(6, 2);
            Int32 GetIntCC = HEX(strCC);

            int i = 0;
            int j = 0;
            while (i < GetInt)
            {
                strCC = ReadString.Substring(6 + i * 2, 2);
                GetIntCC = HEX(strCC);

                j = 0;
                while (j < 8)
                {
                    M[j + i * 8] = (GetIntCC & (1 << j)) == (1 << j);
                    j++;
                }

                i++;
            }
        }
        protected void GetM100_5()
        {
            string strCount = ReadString.Substring(4, 2);
            Int32 GetInt = HEX(strCount);
            string strCC = ReadString.Substring(6, 2);
            Int32 GetIntCC = HEX(strCC);

            int i = 0;
            int j = 0;
            while (i < GetInt)
            {
                strCC = ReadString.Substring(6 + i * 2, 2);
                GetIntCC = HEX(strCC);

                j = 0;
                while (j < 8)
                {
                    M[100 + j + i * 8] = (GetIntCC & (1 << j)) == (1 << j);
                    j++;
                }

                i++;
            }
        }
        protected void SetIO(bool IsOn, string IOName)
        {
            int iresult = 0;
            bool bOK = int.TryParse(IOName, out iresult);
            if (!bOK)
                return;

            string strHEX = iresult.ToString("X8");
            string cmd = strHEX.Substring(4, 4);

            if (IsOn)
                Command("Set Bit On", cmd);
            else
                Command("Set Bit Off", cmd);
        }
        protected void SetData(string IOName, int data)
        {
            int iresult = 0;
            bool bOK = int.TryParse(IOName, out iresult);
            if (!bOK)
                return;

            string strHEX = iresult.ToString("X8");
            string cmd = strHEX.Substring(4, 4);

            string strHEXData = data.ToString("X8");
            string cmdData = strHEXData.Substring(4, 4);

            Command("Set Data", cmd + cmdData);
        }
        protected override void WriteCommand()
        {
            if (IsSimulater)
                return;
            if (COMPort == null)
                return;
            if (!COMPort.IsOpen)
                return;

            byte[] bytecmd = StrToHexByte(LastCommad.GetSite() + LastCommad.GetPLCCommad());
            byte[] crc = Crc(bytecmd, 0, (UInt32)bytecmd.Length);
            string strCrc = ByteToHexStr(crc);
            string Str = LastCommad.GetSite() + LastCommad.GetPLCCommad() + strCrc;
            byte[] lastCmd = StrToHexByte(Str);
            COMPort.Write(lastCmd, 0, lastCmd.Length);
        }
        #endregion

        public bool GetX(int X_Index)
        {
            return X[X_Index];
        }
        public bool GetM(int M_Index)
        {
            return M[M_Index];
        }
        public bool GetM(string M_IONAME)
        {
            int iresult = -1;
            bool bOK = int.TryParse(M_IONAME, out iresult);
            //if (!bOK)
            //    return false;
            return M[iresult];
        }
        public void SetOnOff(string IOName, bool IsOn)
        {
            SetIO(IsOn, IOName);
        }
        public void WriteData(string IOName, int data)
        {
            SetData(IOName, data);
        }

        #region PRIVATE FUNTION TOOLS

        /// <summary>
        /// CRC计算
        /// </summary>
        /// <param name="arr">源数据</param>
        /// <param name="seat">开始位置</param>
        /// <param name="len">长度</param>
        /// <returns></returns>
        private byte[] Crc(byte[] arr, UInt16 seat, UInt32 len)
        {
            UInt32 i;
            UInt16 j, uwCrcReg = 0xFFFF;

            for (i = seat; i < (len); i++)
            {
                uwCrcReg ^= arr[i];
                for (j = 0; j < 8; j++)
                {
                    if ((uwCrcReg & 0x0001) != 0)
                    {
                        uwCrcReg = (UInt16)((UInt16)(uwCrcReg >> 1) ^ (UInt16)0xA001);
                    }
                    else
                    {
                        uwCrcReg = (UInt16)(uwCrcReg >> 1);
                    }
                }
            }
            byte[] CRC = new byte[2];
            CRC[0] = (byte)(uwCrcReg);
            CRC[1] = (byte)(uwCrcReg >> 8);
            return CRC;
        }
        /// <summary>
        /// 将16进制的字符串转为byte[]
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        private byte[] StrToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }
        /// <summary>
        /// byte[]转为16进制字符串
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private string ByteToHexStr(byte[] bytes)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    returnStr += bytes[i].ToString("X2");
                }
            }
            return returnStr;
        }
        /// <summary>
        /// byte[]转为16进制字符串
        /// </summary>
        /// <param name="bytes">源数据</param>
        /// <param name="start">开始位置</param>
        /// <param name="count">长度</param>
        /// <returns></returns>
        private string ByteToHexStr(byte[] bytes, int start, int count)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    if (i >= start && i <= start + count)
                        returnStr += bytes[i].ToString("X2");
                }
            }
            return returnStr;
        }

        #endregion
        public override void Tick()
        {
            base.Tick();
        }
        public override void Close()
        {
            base.Close();
        }

        //當有Input Trigger時，產生OnTrigger
        public delegate void TriggerHandler(string TypeStr);
        public event TriggerHandler TriggerAction;
        public void OnTrigger(String TypeStr)
        {
            if (TriggerAction != null)
            {
                TriggerAction(TypeStr);
            }
        }
    }
}
