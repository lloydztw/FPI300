using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JetEazy.BasicSpace
{

    /// <summary>
    /// Header: 32 Bytes
    /// {
    /// 4个字节整数：命令码
    /// 4个字节整数：DataLength
    /// 24个保留字节：
    /// }
    /// Data: 长度不固定, 长度有Header中DataLength指定, 数据内容格式由命令码指定
    /// </summary>

    public enum tcpCmd : int
    {
        /// <summary>
        /// 指令无效
        /// </summary>
        NONE = 0,
        /// <summary>
        /// 切换参数
        /// 命令码 1
        /// 数据内容格式为：
        /// 参数名称与lot名称用分号划分
        /// </summary>
        CMD_CHANGE = 1,
        /// <summary>
        /// 跳过部分检测 通知QC Cell检测或不检测
        /// 命令码 2
        /// 数据内容格式为：
        /// 4个字节整数：row
        /// 4个字节整数：col
        /// 8个保留字节：
        /// row * col 个字节的Mapping数据： 1 表示打标， 0表示不打标
        /// </summary>
        CMD_QCBYPASS = 2,
        /// <summary>
        /// mapping数据获取
        /// 1是正常打印的需要检测
        /// 0是没有打印不用检测
        /// X是打印了异常内容比如打了个或其他标志
        /// 命令码 24
        /// 数据内容格式为：
        /// 4个字节： row
        /// 4个字节： col
        /// 剩余字节为mapping字符串：各行数据以分号划分，列数据以空格划分
        /// </summary>
        CMD_QC2DDATA = 3,
        /// <summary>
        /// 自动更换某参数的底图
        /// 命令码 25
        /// 数据内容格式为：
        /// 参数名称字符串
        /// </summary>
        CMD_QCCHANGE_MODEL = 4,
        /// <summary>
        /// 2D_Barcode数据获取
        /// 命令码 27
        /// 数据内容格式为：
        /// 4个字节： row
        /// 4个字节： col
        /// 剩余字节为2D_Barcode字符串：数据以逗号划分
        /// </summary>
        CMD_QC2DBARCODE = 5,
    }
    public class tcpItemData
    {
        private string _cmdStr = "";
        private tcpCmd _cmd = tcpCmd.NONE;
        private string _recipename = string.Empty;
        private string _lot_name = string.Empty;
        /// <summary>
        /// true 为不打标 不检测 false 为打标 需要检测
        /// </summary>
        private bool[] _qcbypass = null;
        private string _qc2ddata = null;
        private string[] _qc2dbarcode = null;
        public tcpCmd Cmd
        {
            get { return _cmd; }
        }
        public string RecipeName
        {
            get { return _recipename; }
        }
        public string LotName
        {
            get { return _lot_name; }
        }
        public string Qc2ddata
        {
            get { return _qc2ddata; }
        }
        public bool[] QcByPass
        {
            get { return _qcbypass; }
        }
        public string[] QC2dbarcode
        {
            get { return _qc2dbarcode; }
        }
        public string CmdStr
        {
            get { return _cmdStr; }
        }

        private string rev_data = string.Empty;
        public string RevData
        {
            get { return rev_data; }
        }

        //public tcpItemData()
        //{

        //}
        public tcpItemData(byte[] bytes)
        {
            int ilength = bytes.Length;
            if (ilength > 32)
            {

                byte[] a1 = bytes.Skip(0).Take(4).ToArray();
                Int32 aa1 = BitConverter.ToInt32(a1, 0);
                byte[] a2 = bytes.Skip(4).Take(4).ToArray();
                Int32 aa2 = BitConverter.ToInt32(a2, 0);

                //string mycmd = str.Substring(0, 4);
                //string myDataLength = str.Substring(4, 4);
                int icmd = aa1;// (int)bytes[3] * 255 * 255 * 255 + (int)bytes[2] * 255 * 255 + (int)bytes[1] * 255 + (int)bytes[0];
                int idatalength = aa2;// (int)bytes[7] * 255 * 255 * 255 + (int)bytes[6] * 255 * 255 + (int)bytes[5] * 255 + (int)bytes[4];

                //_cmdStr = mycmd;

                switch (icmd)
                {
                    case 1:
                        _cmdStr = "0001";
                        _cmd = tcpCmd.CMD_CHANGE;
                        if (bytes.Length >= 32 + idatalength)
                        {
                            _recipename = Encoding.UTF8.GetString(bytes, 32, idatalength - 1);
                            string[] strtemp = _recipename.Trim().Split(';');
                            if (strtemp.Length > 1)
                            {
                                _recipename = strtemp[0];
                                _lot_name = strtemp[1];
                            }
                            else
                            {
                                _recipename = string.Empty;
                                _lot_name = string.Empty;
                            }
                        }
                        else
                        {
                            _recipename = string.Empty;
                            _lot_name = string.Empty;
                        }
                        _qcbypass = null;

                        byte[] cmdchange = bytes.Take(32 + idatalength).ToArray();
                        rev_data = string.Empty;
                        foreach (byte b in cmdchange)
                        {
                            rev_data += b.ToString() + " ";
                        }
                        //rev_data = System.Text.Encoding.UTF8.GetString(cmdchange);
                        break;
                    case 2:
                        _cmdStr = "0002";
                        _cmd = tcpCmd.CMD_QCBYPASS;
                        _recipename = string.Empty;

                        #region no_use_code
                        //if (bytes.Length >= 32 + idatalength)
                        //{
                        //    byte[] brow = bytes.Skip(32).Take(4).ToArray();
                        //    Int32 ddrow = BitConverter.ToInt32(brow, 0);
                        //    byte[] bcol = bytes.Skip(36).Take(4).ToArray();
                        //    Int32 ddcol = BitConverter.ToInt32(bcol, 0);

                        //    int _row = ddrow;// (int)bytes[35] * 255 * 255 * 255 + (int)bytes[34] * 255 * 255 + (int)bytes[33] * 255 + (int)bytes[32];
                        //    int _col = ddcol;// (int)bytes[39] * 255 * 255 * 255 + (int)bytes[38] * 255 * 255 + (int)bytes[37] * 255 + (int)bytes[36];
                        //    int irowcol = _row * _col;
                        //    _qcbypass = new bool[irowcol];
                        //    int i = 48;
                        //    while (i < 48 + irowcol)
                        //    {
                        //        _qcbypass[i - 48] = bytes[i] == 0;
                        //        i++;
                        //    }

                        //}
                        //else
                        //    _qcbypass = null;

                        //byte[] cmdqcbypass = bytes.Take(32 + idatalength).ToArray();
                        //rev_data = string.Empty;
                        //foreach (byte b in cmdqcbypass)
                        //{
                        //    rev_data += b.ToString() + " ";
                        //}
                        #endregion

                        #region NEW_MAPPING

                        if (bytes.Length >= 32 + idatalength)
                        {
                            byte[] brow = bytes.Skip(32).Take(4).ToArray();
                            Int32 ddrow = BitConverter.ToInt32(brow, 0);
                            byte[] bcol = bytes.Skip(36).Take(4).ToArray();
                            Int32 ddcol = BitConverter.ToInt32(bcol, 0);

                            int _row = ddrow;// (int)bytes[35] * 255 * 255 * 255 + (int)bytes[34] * 255 * 255 + (int)bytes[33] * 255 + (int)bytes[32];
                            int _col = ddcol;// (int)bytes[39] * 255 * 255 * 255 + (int)bytes[38] * 255 * 255 + (int)bytes[37] * 255 + (int)bytes[36];
                            int irowcol = _row * _col;
                            //_qc2ddata = new string[irowcol];
                            //byte[] tmp = new byte[irowcol];
                            //int i = 40;
                            //while (i < 40 + irowcol)
                            //{
                            //    tmp[i - 40] = bytes[i];
                            //    i++;
                            //}

                            _qc2ddata = System.Text.Encoding.ASCII.GetString(bytes, 40, idatalength - 9);
                            //1是正常打印的需要检测
                            //0是没有打印不用检测
                            //X是打印了异常内容比如打了个或其他标志

                            string[] vs = _qc2ddata.Replace('\0', ' ').Split(';');
                            List<string> vs2 = new List<string>();
                            vs2.Clear();
                            for (int i = 0; i < vs.Length; i++)
                            {
                                if (!string.IsNullOrEmpty(vs[i]))
                                {
                                    string[] vs1 = vs[i].Split(' ');
                                    foreach (string s in vs1)
                                    {
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            vs2.Add(s);
                                        }
                                    }
                                }
                            }

                            if (vs2.Count == irowcol)
                            {
                                _qcbypass = new bool[irowcol];
                                int i = 0;
                                while (i < irowcol)
                                {
                                    _qcbypass[i] = vs2[i] == "0";
                                    i++;
                                }
                            }

                        }
                        else
                        {
                            _qcbypass = null;
                            _qc2ddata = null;
                        }

                        byte[] cmdqc2ddata = bytes.Take(32 + idatalength).ToArray();
                        rev_data = string.Empty;
                        foreach (byte b in cmdqc2ddata)
                        {
                            rev_data += b.ToString() + " ";
                        }
                        //rev_data = System.Text.Encoding.Default.GetString(cmdqc2ddata);

                        #endregion

                        break;
                    case 27:
                        _cmdStr = "0027";
                        _cmd = tcpCmd.CMD_QC2DBARCODE;
                        _recipename = string.Empty;

                        if (bytes.Length >= 32 + idatalength)
                        {
                            byte[] brow = bytes.Skip(32).Take(4).ToArray();
                            Int32 ddrow = BitConverter.ToInt32(brow, 0);
                            byte[] bcol = bytes.Skip(36).Take(4).ToArray();
                            Int32 ddcol = BitConverter.ToInt32(bcol, 0);

                            int _row = ddrow;// (int)bytes[35] * 255 * 255 * 255 + (int)bytes[34] * 255 * 255 + (int)bytes[33] * 255 + (int)bytes[32];
                            int _col = ddcol;// (int)bytes[39] * 255 * 255 * 255 + (int)bytes[38] * 255 * 255 + (int)bytes[37] * 255 + (int)bytes[36];
                            int irowcol = _row * _col;
                            //_qc2ddata = new string[irowcol];
                            //byte[] tmp = new byte[irowcol];
                            //int i = 40;
                            //while (i < 40 + irowcol)
                            //{
                            //    tmp[i - 40] = bytes[i];
                            //    i++;
                            //}

                            _qc2ddata = System.Text.Encoding.ASCII.GetString(bytes, 40, idatalength - 9);
                            //1是正常打印的需要检测
                            //0是没有打印不用检测
                            //X是打印了异常内容比如打了个或其他标志

                            string[] vs = _qc2ddata.Replace('\0', ' ').Split(',');
                            //List<string> vs2 = new List<string>();
                            //vs2.Clear();
                            //for (int i = 0; i < vs.Length; i++)
                            //{
                            //    if (!string.IsNullOrEmpty(vs[i]))
                            //    {
                            //        string[] vs1 = vs[i].Split(' ');
                            //        foreach (string s in vs1)
                            //        {
                            //            if (!string.IsNullOrEmpty(s))
                            //            {
                            //                vs2.Add(s);
                            //            }
                            //        }
                            //    }
                            //}

                            if (vs.Length == irowcol)
                            {
                                _qc2dbarcode = new string[irowcol];
                                int i = 0;
                                while (i < irowcol)
                                {
                                    _qc2dbarcode[i] = vs[i].Trim();
                                    i++;
                                }
                            }
                        }
                        else
                        {
                            _qcbypass = null;
                            _qc2ddata = null;
                            _qc2dbarcode = null;
                        }

                        cmdqc2ddata = bytes.Take(32 + idatalength).ToArray();
                        rev_data = string.Empty;
                        foreach (byte b in cmdqc2ddata)
                        {
                            rev_data += b.ToString() + " ";
                        }
                        //rev_data = System.Text.Encoding.Default.GetString(cmdqc2ddata);
                        break;
                    case 24:
                        _cmdStr = "0024";
                        _cmd = tcpCmd.CMD_QC2DDATA;
                        _recipename = string.Empty;

                        if (bytes.Length >= 32 + idatalength)
                        {
                            byte[] brow = bytes.Skip(32).Take(4).ToArray();
                            Int32 ddrow = BitConverter.ToInt32(brow, 0);
                            byte[] bcol = bytes.Skip(36).Take(4).ToArray();
                            Int32 ddcol = BitConverter.ToInt32(bcol, 0);

                            int _row = ddrow;// (int)bytes[35] * 255 * 255 * 255 + (int)bytes[34] * 255 * 255 + (int)bytes[33] * 255 + (int)bytes[32];
                            int _col = ddcol;// (int)bytes[39] * 255 * 255 * 255 + (int)bytes[38] * 255 * 255 + (int)bytes[37] * 255 + (int)bytes[36];
                            int irowcol = _row * _col;
                            //_qc2ddata = new string[irowcol];
                            //byte[] tmp = new byte[irowcol];
                            //int i = 40;
                            //while (i < 40 + irowcol)
                            //{
                            //    tmp[i - 40] = bytes[i];
                            //    i++;
                            //}

                            _qc2ddata = System.Text.Encoding.ASCII.GetString(bytes, 40, idatalength - 9);
                            //1是正常打印的需要检测
                            //0是没有打印不用检测
                            //X是打印了异常内容比如打了个或其他标志

                            string[] vs = _qc2ddata.Replace('\0', ' ').Split(';');
                            List<string> vs2 = new List<string>();
                            vs2.Clear();
                            for (int i = 0; i < vs.Length; i++)
                            {
                                if (!string.IsNullOrEmpty(vs[i]))
                                {
                                    string[] vs1 = vs[i].Split(' ');
                                    foreach (string s in vs1)
                                    {
                                        if (!string.IsNullOrEmpty(s))
                                        {
                                            vs2.Add(s);
                                        }
                                    }
                                }
                            }

                            if (vs2.Count == irowcol)
                            {
                                _qcbypass = new bool[irowcol];
                                int i = 0;
                                while (i < irowcol)
                                {
                                    _qcbypass[i] = vs2[i] == "0";
                                    i++;
                                }
                            }

                        }
                        else
                        {
                            _qcbypass = null;
                            _qc2ddata = null;
                        }

                        cmdqc2ddata = bytes.Take(32 + idatalength).ToArray();
                        rev_data = string.Empty;
                        foreach (byte b in cmdqc2ddata)
                        {
                            rev_data += b.ToString() + " ";
                        }
                        //rev_data = System.Text.Encoding.Default.GetString(cmdqc2ddata);
                        break;
                    case 25:
                        _cmdStr = "0025";
                        _cmd = tcpCmd.CMD_QCCHANGE_MODEL;
                        if (bytes.Length >= 32 + idatalength)
                        {
                            _recipename = Encoding.UTF8.GetString(bytes, 32, idatalength - 1);
                            _recipename = _recipename.Trim();
                        }
                        else
                            _recipename = string.Empty;
                        _qcbypass = null;

                        byte[] cmdchangeX = bytes.Take(32 + idatalength).ToArray();
                        rev_data = string.Empty;
                        foreach (byte b in cmdchangeX)
                        {
                            rev_data += b.ToString() + " ";
                        }

                        break;
                    default:
                        _cmd = tcpCmd.NONE;
                        _recipename = string.Empty;
                        _qcbypass = null;
                        break;
                }

            }
            else
            {

                rev_data = string.Empty;
                _cmd = tcpCmd.NONE;
                _recipename = string.Empty;
                _qcbypass = null;
            }
        }

        //public tcpItemData(string str)
        //{
        //    int ilength = str.Length;
        //    if (ilength > 32)
        //    {

        //        string mycmd = str.Substring(0, 4);
        //        string myDataLength = str.Substring(4, 4);
        //        int icmd = int.Parse(mycmd);
        //        int idatalength = int.Parse(myDataLength);

        //        _cmdStr = mycmd;

        //        switch (icmd)
        //        {
        //            case 1:
        //                _cmd = tcpCmd.CMD_CHANGE;
        //                if (str.Length >= 32 + idatalength)
        //                    _recipename = str.Substring(32, idatalength);
        //                else
        //                    _recipename = string.Empty;
        //                _qcbypass = null;
        //                break;
        //            case 2:
        //                _cmd = tcpCmd.CMD_QCBYPASS;
        //                _recipename = string.Empty;

        //                if (str.Length >= 32 + idatalength)
        //                {
        //                    string _row = str.Substring(32, 4);
        //                    string _col = str.Substring(36, 4);
        //                    int irowcol = int.Parse(_row) * int.Parse(_col);
        //                    string bypassdata = str.Substring(48, irowcol);
        //                    char[] mydata = bypassdata.ToCharArray();
        //                    _qcbypass = new bool[mydata.Length];
        //                    int i = 0;
        //                    foreach (char ch in mydata)
        //                    {
        //                        _qcbypass[i] = ch == '0';
        //                        i++;
        //                    }
        //                }
        //                else
        //                    _qcbypass = null;

        //                break;
        //            default:
        //                _cmd = tcpCmd.NONE;
        //                _recipename = string.Empty;
        //                _qcbypass = null;
        //                break;
        //        }

        //    }
        //    else
        //    {
        //        _cmd = tcpCmd.NONE;
        //        _recipename = string.Empty;
        //        _qcbypass = null;
        //    }
        //}

    }

    //public class JzTCPClass
    //{
    //    private static JzTCPClass _instance = new JzTCPClass();
    //    public static JzTCPClass Instance
    //    {
    //        get
    //        {
    //            return _instance;
    //        }
    //    }
    //}

    public class ServerSocket
    {
        private static ServerSocket _instance = null;// new ServerSocket();
        public static ServerSocket Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ServerSocket();
                return _instance;
            }
        }

        public ServerSocket()
        {
            if (Log == null)
            {
                Log = new CommonLogClass();
                Log.LogPath = @"D:\LOG\ACT_MAIN_X6";
                Log.LogFilename = "ServerSocket_Tcp";
            }
        }
        public string Host
        {
            get { return host; }
            set { host = value; }
        }
        public int Port
        {
            get { return port; }
            set { port = value; }
        }

        public CommonLogClass Log = null;


        private Socket socket = null;
        private Dictionary<string, Socket> socketDictionary = new Dictionary<string, Socket> { };

        string host = "127.0.0.1";
        int port = 6000;

        private bool _isdebug = false;
        public bool IsDebug
        {
            get { return _isdebug; }
            set { _isdebug = value; }
        }

        public int ServerStart()
        {
            int iret = 0;

            if (_isdebug)
            {
                Log.Log2("ServerSocket " + host + ":" + port.ToString() + " DEBUG 服务已开启！");
                return 0;
            }

            try
            {

                IPAddress ip = IPAddress.Parse(host);
                IPEndPoint ipe = new IPEndPoint(ip, port);

                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Bind(ipe);
                socket.Listen(0);

            }
            catch (Exception ex)
            {
                iret = -1;
                Log.Log2("ServerSocket " + host + ":" + port.ToString() + " 服务创建异常！" + ex.Message);
            }

            if (iret == 0)
            {
                //添加一个一直监听客户端的线程
                Thread thread = new Thread(WatchConnect);
                //该线程随着主线程的结束而结束
                thread.IsBackground = true;
                thread.Start();

                Log.Log2("ServerSocket " + host + ":" + port.ToString() + " 服务已开启！");

                Console.WriteLine("服务已开启！");
                //Console.WriteLine("请输入任意字符，关闭服务端");
                //Console.ReadKey();

                //socket.Close();
            }

            return iret;
        }
        public void ServerStop()
        {

            if (_isdebug)
            {
                Log.Log2("ServerSocket " + host + ":" + port.ToString() + " DEBUG 服务已停止！");
                return;
            }

            if (socket != null)
                socket.Close();

            Log.Log2("ServerSocket " + host + ":" + port.ToString() + " 服务已停止！");
        }

        void WatchConnect()
        {
            Socket socketConnect = null;

            while (true)
            {
                try
                {
                    socketConnect = socket.Accept();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Log.Log2("ServerSocket " + host + ":" + port.ToString() + " 创建异常！" + e.Message);
                    break;
                }

                //获取客户端的IP和Port
                string clientIPE = socketConnect.RemoteEndPoint.ToString();
                //记录该客户端
                socketDictionary.Add(clientIPE, socketConnect);

                Console.WriteLine("【" + clientIPE + "】已上线，在线人数" + socketDictionary.Count.ToString());
                Log.Log2("ServerSocket " + host + ":" + port.ToString() + "【" + clientIPE + "】已上线，在线人数" + socketDictionary.Count.ToString());

                //byte[] sendByte = null;
                ////通知各个客户端，有新客户端上线
                //foreach (var item in socketDictionary)
                //{
                //    //排除掉上线的那个客户端
                //    if (!String.Equals(clientIPE, item.Key.ToString()))
                //    {
                //        sendByte = Encoding.UTF8.GetBytes("【" + clientIPE + "】已上线");
                //        item.Value.Send(sendByte);
                //    }
                //}

                //在监听客户端的线程下，新建子线程，
                //以此来接收各个客户端发送的消息，并发送给各个客户端
                Thread thread = new Thread(Recv);
                thread.IsBackground = true;
                thread.Start(socketConnect);
            }
        }

        void Recv(object objSocket)
        {
            Socket socket = objSocket as Socket;

            //获取客户端的IP和Port
            string clientIPE = socket.RemoteEndPoint.ToString();

            while (true)
            {
                try
                {
                    //定义一个 1M 的缓存空间
                    byte[] bytes = new byte[1024 * 1024];
                    //将从客户端接收的消息存入该缓存空间
                    int recStr = socket.Receive(bytes, bytes.Length, 0);
                    //取出该缓存空间，由客户端发来的消息，并转换为 UTF-8 的可视化格式
                    string strMsg = Encoding.UTF8.GetString(bytes, 0, recStr);

                    Console.WriteLine("【" + clientIPE + "】" + strMsg);
                    Log.Log2("ServerSocket " + host + ":" + port.ToString() + " 接收数据！" + "【" + clientIPE + "】" + strMsg);

                    //解析指令 控制主程序 一些动作

                    ////然后直接发送给各个客户端
                    //foreach (var item in socketDictionary)
                    //{
                    //    //将上线的那个客户端的IP和Port改为“我”
                    //    byte[] strSend = null;
                    //    if (!String.Equals(clientIPE, item.Key.ToString()))
                    //    {
                    //        strSend = Encoding.UTF8.GetBytes("【" + clientIPE + "】：  " + strMsg);
                    //    }
                    //    else
                    //    {
                    //        strSend = Encoding.UTF8.GetBytes("【 我 】：  " + strMsg);
                    //    }
                    //    item.Value.Send(strSend);
                    //}
                }
                catch (Exception e)
                {
                    socketDictionary.Remove(clientIPE);
                    Console.WriteLine("【" + clientIPE + "】 已下线，在线人数" + socketDictionary.Count.ToString());
                    Console.WriteLine(e.Message);

                    Log.Log2("ServerSocket " + host + ":" + port.ToString() + "【" + clientIPE + "】 已下线，在线人数" + socketDictionary.Count.ToString());
                    Log.Log2("ServerSocket " + host + ":" + port.ToString() + " 接收异常！" + e.Message);

                    break;
                }
            }
        }

        public delegate void TriggerHandler(tcpItemData opstr);
        public event TriggerHandler TriggerAction;
        public void OnTrigger(tcpItemData opstr)
        {
            if (TriggerAction != null)
            {
                TriggerAction(opstr);
            }
        }

    }

    public class ClientSocket
    {
        //创建 1个客户端套接字 和1个负责监听服务端请求的线程
        Thread threadclient = null;
        Socket socketclient = null;

        string host = "127.0.0.1";
        int port = 6000;

        private bool m_IsConnecting = false;
        private string m_name = string.Empty;
        private bool m_Debug = false;

        public bool IsConnecting
        {
            get
            {
                if (m_Debug)
                    return true;
                return m_IsConnecting;
            }
        }

        //private static ClientSocket _instance = null;// new ClientSocket();
        //public static ClientSocket Instance
        //{
        //    get
        //    {
        //        if (_instance == null)
        //            _instance = new ClientSocket();
        //        return _instance;
        //    }
        //}

        public CommonLogClass Log = null;

        public ClientSocket()
        {
            if (Log == null)
            {
                Log = new CommonLogClass();
                Log.LogPath = @"D:\LOG\ACT_MAIN_X6";
                Log.LogFilename = "ClientSocket_Tcp";
            }
        }
        public ClientSocket(string eName)
        {
            m_name = eName;
            if (Log == null)
            {
                Log = new CommonLogClass();
                Log.LogPath = @"D:\LOG\ACT_MAIN_X6";
                Log.LogFilename = "ClientSocket_Tcp";
            }
        }
        public string Host
        {
            get { return host; }
            set { host = value; }
        }
        public int Port
        {
            get { return port; }
            set { port = value; }
        }


        /// <summary>
        /// 把int32类型的数据转存到4个字节的byte数组中
        /// </summary>
        /// <param name="m">int32类型的数据
        /// <param name="arry">4个字节大小的byte数组
        /// <returns></returns>
        bool ConvertIntToByteArray(Int32 m, ref byte[] arry)
        {
            if (arry == null) return false;
            if (arry.Length < 4) return false;

            arry[0] = (byte)(m & 0xFF);
            arry[1] = (byte)((m & 0xFF00) >> 8);
            arry[2] = (byte)((m & 0xFF0000) >> 16);
            arry[3] = (byte)((m >> 24) & 0xFF);

            return true;
        }

        public void Send(string str)
        {
            string sendMsg = str;
            if (m_Debug)
            {
                Log.Log2("DEBUG MODE Send str");
                return;
            }

            if (String.IsNullOrEmpty(sendMsg))
            {
                //MessageBox.Show("发送消息不能为空！");
                Log.Log2("ClientSocket " + host + ":" + port.ToString() + " 发送消息不能为空！");
                return;
            }

            if (socketclient == null)
            {
                Log.Log2("ClientSocket " + host + ":" + port.ToString() + " 连接已断开，无法发送！");
                return;
            }

            if (sendMsg.Length < 8)
            {

                Log.Log2("ClientSocket " + host + ":" + port.ToString() + " 数据不合法！");
                return;
            }

            int cmd = int.Parse(sendMsg.Substring(0, 4));
            int retrr = int.Parse(sendMsg.Substring(4, 4));

            byte[] _cmd = new byte[4];
            byte[] _value = new byte[4];

            bool bOK = ConvertIntToByteArray(cmd, ref _cmd);
            if (bOK)
                bOK = ConvertIntToByteArray(retrr, ref _value);

            if (bOK)
            {

                byte[] _cmdgo = new byte[8];
                int i = 0;
                while (i < 8)
                {
                    if (i < 4)
                        _cmdgo[i] = _cmd[i];
                    else
                        _cmdgo[i] = _value[i - 4];
                    i++;
                }

                Send(_cmdgo);
            }

            //CommonLogClass.Instance.Log2("ClientSocket " + host + ":" + port.ToString() + " 发送数据！" + sendMsg);


            //byte[] sendByte = Encoding.UTF8.GetBytes(sendMsg);
            //int sendSuccess = socketclient.Send(sendByte);

            //CommonLogClass.Instance.Log2("ClientSocket " + host + ":" + port.ToString() + " 发送数据！" + (sendSuccess > 0 ? "成功" : "失败"));
            //if (sendSuccess > 0)
            //{
            //    //this.textBox2.Text = "";
            //}
        }
        public void Send(byte[] bytes)
        {
            string sendMsg = string.Empty;
            if (m_Debug)
            {
                Log.Log2("DEBUG MODE Send bytes");
                return;
            }
            //if (String.IsNullOrEmpty(sendMsg))
            //{
            //    //MessageBox.Show("发送消息不能为空！");
            //    CommonLogClass.Instance.Log2("ClientSocket " + host + ":" + port.ToString() + " 发送消息不能为空！");
            //    return;
            //}

            if (socketclient == null)
            {
                Log.Log2("ClientSocket " + host + ":" + port.ToString() + " 连接已断开，无法发送！");
                return;
            }

            foreach (byte b in bytes)
            {
                sendMsg += b.ToString() + " ";
            }

            Log.Log2("ClientSocket " + host + ":" + port.ToString() + " 发送数据！" + sendMsg);


            //byte[] sendByte = Encoding.UTF8.GetBytes(sendMsg);
            int sendSuccess = socketclient.Send(bytes);
           
            Log.Log2("ClientSocket " + host + ":" + port.ToString() + $" 发送数据！长度{sendSuccess}" + (sendSuccess > 0 ? "成功" : "失败"));
            if (sendSuccess > 0)
            {
                //this.textBox2.Text = "";
            }
        }

        public int ReConnectServer()
        {
            DisConnectServer();
            return ConnectServer(m_Debug);
        }
        public int ConnectServer(bool eDebug = false)
        {
            int iret = 0;
            m_Debug = eDebug;
            if (m_Debug)
            {
                Log.Log2("DEBUG MODE DisConnectServer");
                return 0;
            }
            try
            {

                IPAddress ip = IPAddress.Parse(host);
                IPEndPoint ipe = new IPEndPoint(ip, port);
                
                socketclient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                switch (m_name)
                {
                    case "handle32002":
                        socketclient.Bind(new IPEndPoint(ip, 32002));
                        break;
                    case "handle":
                        break;
                    case "laser":
                        break;
                }
                socketclient.Connect(ipe);
                Log.Log2("ClientSocket " + host + ":" + port.ToString() + " 连接服务器成功！");

                //socketclient.ReceiveTimeout = 1000;
                //socketclient.SendTimeout = 1000;

                m_IsConnecting = true;
                OnTriggerString("S,OK");


            }
            catch (Exception ex)
            {
                iret = -1;
                Log.Log2("ClientSocket " + host + ":" + port.ToString() + " 连接服务器失败！" + ex.Message);

                m_IsConnecting = false;
                OnTriggerString("S,FAIL");

            }

            if (iret == 0)
            {
                //新建一个通讯线程，一直接收来自服务的消息
                threadclient = new Thread(RecvSend);
                threadclient.IsBackground = true;
                threadclient.Start();

                Log.Log2("ClientSocket " + host + ":" + port.ToString() + " 创建接收数据线程！");
            }

            return iret;

        }
        public void DisConnectServer()
        {
            if (m_Debug)
            {
                Log.Log2("DEBUG MODE DisConnectServer");
                return;
            }
            try
            {

                if (socketclient != null)
                    socketclient.Close();

                if (threadclient != null)
                {
                    //if (threadclient.ThreadState != ThreadState.Stopped)
                    threadclient.Abort();
                }

                Log.Log2("ClientSocket " + host + ":" + port.ToString() + " 释放资源！");
            }
            catch
            {

            }
        }
        void RecvSend()
        {
            while (true)
            {
                try
                {
                    byte[] bytes = new byte[1024 * 1024];
                    int recStr = socketclient.Receive(bytes, bytes.Length, 0);
                    //string strMsg = Encoding.UTF8.GetString(bytes, 0, recStr);
                    //strMsg = string.Empty;

                    //int i = 0;
                    //foreach (byte b in bytes)
                    //{
                    //    strMsg += b.ToString() + " ";
                    //    i++;

                    //    if (i >= 100)
                    //    {
                    //        strMsg += Environment.NewLine;
                    //        i = 0;
                    //    }
                    //}

                    //this.textBox1.AppendText(Utity.LineFeed + strMsg + Utity.LineFeed);
                    //CommonLogClass.Instance.Log2("ClientSocket " + host + ":" + port.ToString() + " 接收数据！" + strMsg);

                    //解析指令
                    if (recStr == 0)
                    {
                        //continue;

                        socketclient.Close();

                        m_IsConnecting = false;
                        OnTriggerString("S,FAIL");

                        Log.Log2("ClientSocket " + host + ":" + port.ToString() + " 远端服务器断开！无数据！");

                        break;
                    }
                    tcpItemData _tcp = new tcpItemData(bytes);

                    Log.Log2("ClientSocket " + host + ":" + port.ToString() + " 接收数据！" + _tcp.RevData);
                    switch (_tcp.Cmd)
                    {
                        case tcpCmd.CMD_CHANGE:
                        case tcpCmd.CMD_QCBYPASS:
                        case tcpCmd.CMD_QC2DDATA:
                        case tcpCmd.CMD_QCCHANGE_MODEL:
                        case tcpCmd.CMD_QC2DBARCODE:
                            OnTrigger(_tcp);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    socketclient.Close();

                    m_IsConnecting = false;
                    OnTriggerString("S,FAIL");

                    Log.Log2("ClientSocket " + host + ":" + port.ToString() + " 接收异常连接断开！" + ex.Message);
                    break;
                }
            }

        }

        public delegate void TriggerHandler(tcpItemData opstr);
        public event TriggerHandler TriggerAction;
        public void OnTrigger(tcpItemData opstr)
        {
            if (TriggerAction != null)
            {
                TriggerAction(opstr);
            }
        }

        public delegate void TriggerStringHandler(string opstr);
        public event TriggerStringHandler TriggerStringAction;
        public void OnTriggerString(string opstr)
        {
            if (TriggerStringAction != null)
            {
                TriggerStringAction(opstr);
            }
        }


    }
}
