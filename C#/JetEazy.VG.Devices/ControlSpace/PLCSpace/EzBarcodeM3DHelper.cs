using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetEazy.ControlSpace.PLCSpace
{
    public class EzBarcodeM3DHelper : COMClass
    {
        private char CR = '\x0D';
        private char LF = '\x0A';

        //private static readonly ElecHelper m_instance = new ElecHelper();
        //public static ElecHelper Instance
        //{
        //    get { return m_instance; }
        //}
        public string Run()
        {
            m_read_complete = false;
            if (m_Debug)
            {
                return "SIMBarCode_" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
            }

            //string _retStr = string.Empty;

            //Task task = new Task(() =>
            //{
            //m_stopwatch.Restart();
            _openM3D();
            m_readdata = string.Empty;
            int iret = 0;// ElecHelper.Instance.Open(strComname, false);
            if (iret == 0)
            {
                int inttick = Environment.TickCount;
                while (true)
                {
                    if (m_read_complete)
                    {
                        m_read_complete = false;
                        //return m_readdata;
                        //rtbText.Text = ElecHelper.Instance.GetData;
                        //_updateMessage(rtbText, ElecHelper.Instance.GetData);
                        //Console.WriteLine(ElecHelper.Instance.GetData);
                        Console.WriteLine(m_readdata);
                        //ElecHelper.Instance.Close();
                        break;
                    }

                    if (Environment.TickCount - inttick >= 3000)
                    {
                        Console.WriteLine("-1");
                        m_readdata = string.Empty;
                        _closeM3D();
                        break;
                    }

                }
            }
            else
            {
                Console.WriteLine("-1");
                m_readdata = string.Empty;
                _closeM3D();
            }

            //m_stopwatch.Stop();
            //string ms = "耗时:" + m_stopwatch.ElapsedMilliseconds.ToString() + " ms";
            //_updateMessage(lblMs, ms);

            //});
            //task.Start();

            return m_readdata;
        }
        SerialPort m_SerialPort
        {
            get { return COMPort; }
        }
        bool m_Debug// = false;
        {
            get { return IsSimulater; }
        }
        ////送出命令後，讀回來的資料
        //protected int BytesToRead = 0;
        //protected int ReadStart = 0;
        //protected char[] ReadBuffer = new char[1024];
        string m_readdata = "";
        bool m_read_complete = false;
        //private int m_readexist_value = 0;

        //public int Open(string strComName, bool eDebug)
        //{
        //    m_Debug = eDebug;
        //    if (m_Debug)
        //        return 0;

        //    try
        //    {
        //        if (m_SerialPort == null)
        //        {
        //            m_SerialPort = new SerialPort(strComName, 9600, Parity.None, 8, StopBits.One);
        //            m_SerialPort.DataReceived += M_SerialPort_DataReceived;
        //        }

        //        if (m_SerialPort.IsOpen)
        //            m_SerialPort.Close();

        //        m_SerialPort.Open();

        //        ReadStart = 0;
        //        ReadBuffer = new char[1024];
        //        m_SerialPort.DiscardInBuffer();

        //        if (m_SerialPort.IsOpen)
        //            m_SerialPort.DtrEnable = true;


        //    }
        //    catch (Exception ex)
        //    {
        //        return -1;
        //    }

        //    return 0;
        //}
        //public void Close()
        //{

        //    if (m_Debug)
        //        return;

        //    try
        //    {
        //        if (m_SerialPort == null)
        //            return;

        //        ReadStart = 0;
        //        ReadBuffer = new char[1024];
        //        m_SerialPort.DiscardInBuffer();

        //        if (m_SerialPort.IsOpen)
        //            m_SerialPort.Close();

        //        m_SerialPort = null;
        //    }
        //    catch (Exception ex)
        //    {
        //        //return -1;
        //    }

        //    //return 0;
        //}

        public string GetData
        {
            get
            {
                //if (m_Debug)
                //    m_readdata = "D 88.8g";
                return m_readdata;
            }
            set { m_readdata = value; }
        }
        /// <summary>
        /// 指令完成信号 可发指令前 将此信号置位 false
        /// </summary>
        public bool IsComplete
        {
            get { return m_read_complete; }
            set { m_read_complete = value; }
        }
        public bool IsHolding
        {
            get
            {
                if (m_Debug)
                    return false;

                if (m_SerialPort == null)
                    return false;

                if (m_SerialPort.IsOpen)
                    return m_SerialPort.DtrEnable;

                return false;
            }
            //set { m_read_complete = value; }
        }

        /// <summary>
        /// 初始化關閉感應識讀模式 防止USB中途被拔出
        /// </summary>
        public void InitM3D()
        {
            _closeAutoM3D();
        }
        public void OpenM3D()
        {
            _openM3D();
        }
        public void CloseM3D()
        {
            _closeM3D();
        }

        /// <summary>
        /// 打开扫码
        /// </summary>
        private void _openM3D()
        {
            //02 01 01 02 B8 0B 00 00 00 00 00 00 00 00 03 34
            byte[] data = new byte[16] { 0x02, 0x01, 0x01, 0x02, 0xB8, 0x0B, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03, 0x34 };
            //byte[] data = new byte[14] { 0x5A, 0x00, 0x00, 0x08, 0x53, 0x52, 0x30, 0x33, 0x30, 0x33, 0x30, 0x31, 0x08, 0xA5 };

            if (m_Debug)
            {
                data = Encoding.ASCII.GetBytes("OpenM3D");
            }

            WriteData(data);
        }
        /// <summary>
        /// 关闭扫码
        /// </summary>
        private void _closeM3D()
        {
            byte[] data = new byte[16] { 0x02, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03, 0xF9 };
            //byte[] data = new byte[14] { 0x5A, 0x00, 0x00, 0x08, 0x53, 0x52, 0x30, 0x33, 0x30, 0x33, 0x30, 0x30, 0x09, 0xA5 };

            if (m_Debug)
            {
                data = Encoding.ASCII.GetBytes("CloseM3D");
            }

            WriteData(data);
        }
        /// <summary>
        /// 關閉感應模式
        /// </summary>
        private void _closeAutoM3D()
        {
            //02 02 06 82 30 32 33 31 30 00 00 00 00 00 03 7B
            byte[] data = new byte[16] { 0x02, 0x02, 0x06, 0x82, 0x30, 0x32, 0x33, 0x31, 0x30, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03, 0x7B };
            //byte[] data = new byte[14] { 0x5A, 0x00, 0x00, 0x08, 0x53, 0x52, 0x30, 0x33, 0x30, 0x33, 0x30, 0x30, 0x09, 0xA5 };

            if (m_Debug)
            {
                data = Encoding.ASCII.GetBytes("CloseAutoM3D");
            }

            WriteData(data);
        }

        protected override void COMPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                BytesToRead = m_SerialPort.BytesToRead;
                m_SerialPort.Read(ReadBuffer, ReadStart, BytesToRead);
                ReadStart = ReadStart + BytesToRead;
                //
                if (Analyze(ReadStart - 1)) //此時 ReadBuffer裏有東西，利用 BytesToRead, ReadStart 及 ReadBuffer來取得所需要的資料
                {
                    //m_read_complete = false;
                    ReadStart = 0;
                    ReadBuffer = new char[1024];

                    m_SerialPort.DiscardInBuffer();
                    m_SerialPort.DiscardOutBuffer();
                }
            }
            catch (Exception ex)
            {
                m_readdata = "Error";

                ReadStart = 0;
                ReadBuffer = new char[1024];

                if (m_SerialPort != null)
                {
                    if (m_SerialPort.IsOpen)
                    {
                        m_SerialPort.DiscardInBuffer();
                        m_SerialPort.DiscardOutBuffer();
                    }
                    else
                    {
                        m_readdata = "Error";
                    }
                }

                //m_read_complete = true;

            }
        }
        //private void M_SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        //{

        //}
        protected bool Analyze(int LastIndex)
        {
            bool ret = false;

            if (ReadBuffer[LastIndex] != CR)
            {
                return ret;
            }
            //else if (ReadBuffer[0] != 'S')
            //    return ret;
            else
                ret = true;

            m_readdata = new string(ReadBuffer, 0, ReadStart - 1);
            m_read_complete = true;

            return ret;
        }
        private void WriteData(byte[] eData)
        {
            if (m_SerialPort == null)
                return;
            if (!m_SerialPort.IsOpen)
                return;

            m_SerialPort.DiscardInBuffer();
            m_SerialPort.DiscardOutBuffer();
            System.Threading.Thread.Sleep(10);

            m_SerialPort.Write(eData, 0, eData.Length);

            //if (m_SerialPort != null)
            //{
            //    if (m_SerialPort.IsOpen)
            //    {
                    
            //    }
            //}
        }
    }
}
