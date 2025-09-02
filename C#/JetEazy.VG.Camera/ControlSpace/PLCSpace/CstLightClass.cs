using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace JetEazy.ControlSpace.PLCSpace
{
    public class CstLightClass : COMClass
    {
        private char CR = '\x0D';
        private char LF = '\x0A';
        private int m_chnum = 0;
        public int ChNum
        {
            get { return m_chnum; } set { m_chnum = value; }

        }

        public void Trigger()
        {
            switch (PlcTypeStr)
            {
                case "TSD":
                    TriggerOn();
                    break;
                default:

                    break;
            }
        }
        /// <summary>
        /// 频闪控制器触发指令
        /// </summary>
        private void TriggerOn()
        {
            if (IsSimulater)
                return;
            if (COMPort == null)
                return;
            if (!COMPort.IsOpen)
                return;

            //if (myTimerRTU.msDuriation <= 300)
            //    return;
            string m_cmd = $"$71000";
            string m_CmdDataStrWhole = $"{m_cmd}{_XorString((m_cmd))}";
            //byte[] m_CmdDataByte = StrToHexByte(m_CmdDataStrWhole);
            //COMPort.Write(m_CmdDataByte, 0, m_CmdDataByte.Length);
            COMPort.Write(m_CmdDataStrWhole);
            System.Threading.Thread.Sleep(100);
        }
        private string _XorString(string eStr)
        {
            string input = eStr;// textBox1.Text.Trim(); //"HelloWorld";
            byte xorResult = 0; // 初始化异或结果为0

            // 将字符串转换为字节数组
            byte[] bytes = Encoding.UTF8.GetBytes(input);

            // 遍历字节数组，进行异或操作
            foreach (byte b in bytes)
            {
                xorResult ^= b; // 对当前字节进行异或操作
            }

            // 将最终的异或结果转换为16进制字符串
            string hexResult = xorResult.ToString("X2"); // "X2"确保结果是两位16进制数，不足两位前面补0
            //textBox2.Text = $"{textBox1.Text.Trim()}{hexResult}";
            return hexResult;
        }
        public int CstLightValue
        {
            set
            {
                switch (PlcTypeStr)
                {
                    case "DPS":
                        DPSCmd(1, value);
                        DPSCmd(2, value);
                        break;
                    case "JET":
                        jtcCmd(value);
                        break;
                    case "CST":
                        cmdDataSend(1, 0, value);
                        break;
                    case "FLACD":
                        FLACDCmd(m_chnum, value);
                        break;
                    default:
                        
                        break;
                }
            }
        }
        public void LightONOFF(bool ison)
        {
            switch (PlcTypeStr)
            {
                case "DPS":
                    DPSCmdOnOff(1, ison);
                    DPSCmdOnOff(2, ison);
                    break;
                case "JET":
                    if (!ison)
                        jtcCmd(0);
                    break;
                case "CST":
                    if (ison)
                        cmdDataSend(1, 2);
                    else
                        cmdDataSend(1, 3);
                    break;
                case "FLACD":
                    if (!ison)
                        FLACDCmd(m_chnum, 0);
                    break;
                default:

                    break;
            }
        }
        SerialPort m_SerialPort
        {
            get { return COMPort; }
        }
        bool m_Debug
        {
            get { return IsSimulater; }
        }
        protected override void COMPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //try
            //{
            //    BytesToRead = m_SerialPort.BytesToRead;
            //    m_SerialPort.Read(ReadBuffer, ReadStart, BytesToRead);
            //    ReadStart = ReadStart + BytesToRead;
            //    //
            //    if (Analyze(ReadStart - 1)) //此時 ReadBuffer裏有東西，利用 BytesToRead, ReadStart 及 ReadBuffer來取得所需要的資料
            //    {
            //        //m_read_complete = false;
            //        ReadStart = 0;
            //        ReadBuffer = new char[1024];

            //        m_SerialPort.DiscardInBuffer();
            //        m_SerialPort.DiscardOutBuffer();
            //    }
            //}
            //catch (Exception ex)
            //{
            //    m_readdata = "Error";

            //    ReadStart = 0;
            //    ReadBuffer = new char[1024];

            //    if (m_SerialPort != null)
            //    {
            //        if (m_SerialPort.IsOpen)
            //        {
            //            m_SerialPort.DiscardInBuffer();
            //            m_SerialPort.DiscardOutBuffer();
            //        }
            //        else
            //        {
            //            m_readdata = "Error";
            //        }
            //    }
            //}
        }
        protected bool Analyze(int LastIndex)
        {
            bool ret = false;

            if (ReadBuffer[LastIndex] != CR)
            {
                return ret;
            }
            else
                ret = true;

            return ret;
        }

        /// <summary>
        /// 发送指令控制CST灯光亮度
        /// </summary>
        /// <param name="ChNum">通道 1~4</param>
        /// <param name="Flag">0写入通道亮度 1读回通道亮度 2常亮 3常灭 4读回</param>
        /// <param name="iLightValue">亮度值</param>
        private void cmdDataSend(short ChNum, short Flag, int iLightValue = 128)
        {
            if (IsSimulater)
                return;
            if (m_SerialPort == null)
                return;
            if (!m_SerialPort.IsOpen)
                return;

            m_SerialPort.DiscardInBuffer();
            m_SerialPort.DiscardOutBuffer();
            //System.Threading.Thread.Sleep(10);
            if (this.m_SerialPort.IsOpen)
            {
                if (Flag == 0)
                {
                    if (ChNum == 1)
                    {
                        try
                        {
                            string text = iLightValue.ToString();
                            if (text.Length == 1)
                            {
                                text = "000" + text;
                            }
                            else
                            {
                                if (text.Length == 2)
                                {
                                    text = "00" + text;
                                }
                                else
                                {
                                    text = "0" + text;
                                }
                            }
                            this.m_SerialPort.WriteLine("SA" + text + "#");
                            return;
                        }
                        catch
                        {
                            MessageBox.Show("发送失败！！！");
                            return;
                        }
                    }
                    if (ChNum == 2)
                    {
                        try
                        {
                            string text2 = iLightValue.ToString();
                            if (text2.Length == 1)
                            {
                                text2 = "000" + text2;
                            }
                            else
                            {
                                if (text2.Length == 2)
                                {
                                    text2 = "00" + text2;
                                }
                                else
                                {
                                    text2 = "0" + text2;
                                }
                            }
                            this.m_SerialPort.WriteLine("SB" + text2 + "#");
                            return;
                        }
                        catch
                        {
                            MessageBox.Show("发送失败！！！");
                            return;
                        }
                    }
                    if (ChNum == 3)
                    {
                        try
                        {
                            string text3 = iLightValue.ToString();
                            if (text3.Length == 1)
                            {
                                text3 = "000" + text3;
                            }
                            else
                            {
                                if (text3.Length == 2)
                                {
                                    text3 = "00" + text3;
                                }
                                else
                                {
                                    text3 = "0" + text3;
                                }
                            }
                            this.m_SerialPort.WriteLine("SC" + text3 + "#");
                            return;
                        }
                        catch
                        {
                            MessageBox.Show("发送失败！！！");
                            return;
                        }
                    }
                    try
                    {
                        string text4 = iLightValue.ToString();
                        if (text4.Length == 1)
                        {
                            text4 = "000" + text4;
                        }
                        else
                        {
                            if (text4.Length == 2)
                            {
                                text4 = "00" + text4;
                            }
                            else
                            {
                                text4 = "0" + text4;
                            }
                        }
                        this.m_SerialPort.WriteLine("SD" + text4 + "#");
                        return;
                    }
                    catch
                    {
                        MessageBox.Show("发送失败！！！");
                        return;
                    }
                }
                if (Flag == 1)
                {
                    if (ChNum == 1)
                    {
                        try
                        {
                            this.m_SerialPort.WriteLine("SA#");
                            return;
                        }
                        catch
                        {
                            MessageBox.Show("发送失败！！！");
                            return;
                        }
                    }
                    if (ChNum == 2)
                    {
                        try
                        {
                            this.m_SerialPort.WriteLine("SB#");
                            return;
                        }
                        catch
                        {
                            MessageBox.Show("发送失败！！！");
                            return;
                        }
                    }
                    if (ChNum == 3)
                    {
                        try
                        {
                            this.m_SerialPort.WriteLine("SC#");
                            return;
                        }
                        catch
                        {
                            MessageBox.Show("发送失败！！！");
                            return;
                        }
                    }
                    try
                    {
                        this.m_SerialPort.WriteLine("SD#");
                        return;
                    }
                    catch
                    {
                        MessageBox.Show("发送失败！！！");
                        return;
                    }
                }
                if (Flag == 2)
                {
                    try
                    {
                        this.m_SerialPort.WriteLine("TH#");
                        return;
                    }
                    catch
                    {
                        MessageBox.Show("发送失败！！！");
                        return;
                    }
                }
                if (Flag == 3)
                {
                    try
                    {
                        this.m_SerialPort.WriteLine("TL#");
                        return;
                    }
                    catch
                    {
                        MessageBox.Show("发送失败！！！");
                        return;
                    }
                }
                if (Flag == 4)
                {
                    try
                    {
                        this.m_SerialPort.WriteLine("T#");
                    }
                    catch
                    {
                        MessageBox.Show("发送失败！！！");
                    }
                }
            }
        }
        public void jtcCmd(int val)
        {
            string StrVal = ValueToHEX(val, 8);
            //Command("Set Site Data", "0000" + StrVal.Substring(4, 4));

            if (IsSimulater)
                return;
            if (COMPort == null)
                return;
            if (!COMPort.IsOpen)
                return;

            //if (myTimerRTU.msDuriation <= 300)
            //    return;
            string _cmd = $"01100000000102{StrVal.Substring(4, 4)}";
            byte[] bytecmd = StrToHexByte(_cmd);
            byte[] crc = Crc(bytecmd, 0, (UInt32)bytecmd.Length);
            string strCrc = ByteToHexStr(crc);
            string Str = _cmd + strCrc;
            byte[] lastCmd = StrToHexByte(Str);
            COMPort.Write(lastCmd, 0, lastCmd.Length);

        }

        public void DPSCmd(short ChNum, int val)
        {
            //string StrVal = Convert.ToByte(hexString.Substring(i * 2, 2), 16)
            //Command("Set Site Data", "0000" + StrVal.Substring(4, 4));

            if (IsSimulater)
                return;
            if (COMPort == null)
                return;
            if (!COMPort.IsOpen)
                return;

            //if (myTimerRTU.msDuriation <= 300)
            //    return;
            string m_cmd = $"5A {ChNum.ToString("00")} {val.ToString("X")}";
            string m_CmdDataStrWhole = $"3D {m_cmd} {ByteToHexStr(CRCCalc(m_cmd))} 0D";
            byte[] m_CmdDataByte = StrToHexByte(m_CmdDataStrWhole);
            COMPort.Write(m_CmdDataByte, 0, m_CmdDataByte.Length);
            System.Threading.Thread.Sleep(100);
        }
        public void DPSCmdOnOff(short ChNum, bool ison)
        {
            string StrVal = (ison ? "01" : "00");
            //Command("Set Site Data", "0000" + StrVal.Substring(4, 4));

            if (IsSimulater)
                return;
            if (COMPort == null)
                return;
            if (!COMPort.IsOpen)
                return;

            //if (myTimerRTU.msDuriation <= 300)
            //    return;
            string m_cmd = $"5B {ChNum.ToString("00")} {StrVal}";
            string m_CmdDataStrWhole = $"3D {m_cmd} {ByteToHexStr(CRCCalc(m_cmd))} 0D";
            byte[] m_CmdDataByte = StrToHexByte(m_CmdDataStrWhole);
            COMPort.Write(m_CmdDataByte, 0, m_CmdDataByte.Length);
            System.Threading.Thread.Sleep(100);
        }

        /// <summary>
        /// 追光者控制器
        /// </summary>
        /// <param name="ChNum">通道</param>
        /// <param name="val">亮度值0~255</param>
        public void FLACDCmd(int ChNum, int val)
        {
            if (IsSimulater)
                return;
            if (COMPort == null)
                return;
            if (!COMPort.IsOpen)
                return;
            string m_cmd = $"SLV{ChNum.ToString()}{val.ToString("000")}#";
            COMPort.Write(m_cmd);
            System.Threading.Thread.Sleep(100);
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
                returnBytes[i] = System.Convert.ToByte(hexString.Substring(i * 2, 2), 16);
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

        protected Int32 HEXSigned32(string HexStr)
        {
            return System.Convert.ToInt32(HexStr, 16);
        }
        protected string YL42M_Address(byte addr)
        {
            return addr.ToString("X8").Substring(4, 4);
        }

        byte[] CRCCalc2(byte[] data)  //[]
        {
            //1 初始化一个16位的寄存器地址 用作初始值
            int crc = 0x00;

            //2 遍历数据字节
            for (int i = 0; i < data.Length; i++)
            {
                crc += data[i];
            }
            byte[] crc16 = new byte[2];// crc寄存器的高低为进行互换
            crc16[0] = (byte)((crc >> 8) & 0xff); //crc寄存器高八位变成了八低位
            crc16[1] = (byte)(crc & 0xff);// crc寄存器低八位变成了高低位
            return crc16;
        }

        /// <summary>
        /// CRC校验，参数为空格或逗号间隔的字符串
        /// </summary>
        /// <param name="data">校验数据，逗号或空格间隔的16进制字符串(带有0x或0X也可以),逗号与空格不能混用</param>
        /// <returns>字节0是高8位，字节1是低8位</returns>
        ///"01 03 00 03 00 01"
        byte[] CRCCalc(string data)
        {
            //分隔符是空格还是逗号进行分类，并去除输入字符串中的多余空格
            IEnumerable<string> datac = data.Contains(",") ? data.Replace(" ", "").Replace("0x", "").Replace("0X", "").Trim().Split(',') : data.Replace("0x", "").Replace("0X", "").Split(' ').ToList().Where(u => u != "");
            List<byte> bytedata = new List<byte>();
            foreach (string str in datac)
            {
                bytedata.Add(byte.Parse(str, System.Globalization.NumberStyles.AllowHexSpecifier));
            }
            byte[] crcbuf = bytedata.ToArray();
            //crc计算赋初始值
            return CRCCalc2(crcbuf);
        }

        #endregion


    }
}
