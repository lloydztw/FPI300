using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using JetEazy.ControlSpace.MotoCanSpace;
using System.Net.Sockets;
using System.Net;

namespace MotoCan
{
    using HandleCan = IntPtr;
    public class CanMotoControl
    {
        private static string SocketCanGw = "192.168.0.178:4001"; // socke can的网关地址
        static uint gw_ip = StringToIpV4(GetSocketCanGwIp());
        static ushort gw_port = Convert.ToUInt16(GetSocketCanGwPort());

        bool SocketCanInitOk = false, CanDeviceMotorInitOk = false;
        
        HandleCan hSocketCan;//以太网转Can类
        IntPtr hCanDeviceCore;//Can通信内核类
        HandleCan pCanDeviceMotor;//Can电机设备类

        CanDeviceCore.CanDeviceCoreCallBack CoreCb = null;
        SocketCan.SocketCanEventCallBack SocketCanCb = null;
        CanDeviceMotor.CanDeviceMotorCallBack MotorCb = null;

        //public CanDeviceMotor.CCanMotor CanMotor1 = new CanDeviceMotor.CCanMotor();
        //public CanDeviceMotor.CCanMotor CanMotor2 = new CanDeviceMotor.CCanMotor();
        //public CanDeviceMotor.CCanMotor CanMotor3 = new CanDeviceMotor.CCanMotor();
        public CanDeviceMotor.CCanMotor[] CanMotor;  //= new CanDeviceMotor.CCanMotor();
        public CanDeviceMotor.CMotor [] MotorInfo;

        IntPtr [] pCanMotor;
        //IntPtr pCanMotor1;
        //IntPtr pCanMotor2;
        //IntPtr pCanMotor3;

        bool CanDeviceCoreInitOk = false;

        public CanMotoControl()
        {
        }
        public void Initial(int motioncount,string ipstr)
        {
            int i = 0;

            SocketCanGw = ipstr;
            gw_ip = StringToIpV4(GetSocketCanGwIp());
            gw_port = Convert.ToUInt16(GetSocketCanGwPort());

            CanMotor = new CanDeviceMotor.CCanMotor[motioncount];
            pCanMotor = new IntPtr[motioncount];
            MotorInfo = new CanDeviceMotor.CMotor[motioncount];

            //string strpath =  System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            //SocketCan.opentest();

            hSocketCan = SocketCan.SocketCanCreate();
            pCanDeviceMotor = CanDeviceMotor.CanDeviceMotorCreate();

            HandleCan hCanDeviceDefault = CanDeviceMotor.CanDeviceMotorToCanDevice(pCanDeviceMotor);
            hCanDeviceCore = CanDeviceCore.CanDeviceCoreCreate(hCanDeviceDefault);

            HandleCan hSenderProxy = SocketCan.SocketCanToCanSenderProxy(hSocketCan);

            // 此委托必须new出来，否则多次调用回调函数后，可能会被系统收回从而导致程序崩溃
            CoreCb = new CanDeviceCore.CanDeviceCoreCallBack(CanDeviceCoreCallBack);
            CanDeviceCoreInitOk = CanDeviceCore.CanDeviceCoreInit(hCanDeviceCore, hSenderProxy, CoreCb, (IntPtr)0);

            MotorCb = new CanDeviceMotor.CanDeviceMotorCallBack(CanDeviceMotorCallBack);
            CanDeviceMotorInitOk = CanDeviceMotor.CanDeviceMotorInit(pCanDeviceMotor, hCanDeviceCore, MotorCb, (IntPtr)0);

            HandleCan hCanDevice = CanDeviceCore.CanDeviceCoreToCanDevice(hCanDeviceCore);
            SocketCanCb = new SocketCan.SocketCanEventCallBack(SocketCanEventCallBack);
            SocketCanInitOk = SocketCan.SocketCanInit(hSocketCan, hCanDevice, SocketCanCb, (IntPtr)0);


            while (i < motioncount)
            {
                pCanMotor[i] = CanDeviceMotor.CanDeviceMotorCreateMotor(pCanDeviceMotor, 1, (ushort)(0xc2 + i));
                CanMotor[i] = ToCanMotor(pCanMotor[i]);

                ////用户资源2
                //pCanMotor2 = CanDeviceMotor.CanDeviceMotorCreateMotor(pCanDeviceMotor, 1, (ushort)(0xc3));
                //CanMotor2 = ToCanMotor(pCanMotor2);

                ////用户资源3
                //pCanMotor3 = CanDeviceMotor.CanDeviceMotorCreateMotor(pCanDeviceMotor, 1, (ushort)(0xc4));
                //CanMotor3 = ToCanMotor(pCanMotor3);

                i++;
            }

            Connect();
        }
        ~CanMotoControl()
        {
            if (CanDeviceCoreInitOk)
            {
                SocketCan.SocketCanDestroy(hSocketCan);
                CanDeviceMotor.CanDeviceMotorDestroy(pCanDeviceMotor);
                CanDeviceCore.CanDeviceCoreCreate(hCanDeviceCore);
            }
        }

        public int CanDeviceCoreCallBack(HandleCan hCanDeviceCore, CanDeviceCore.CanDeviceDataEvent e, HandleCan pData, int nLen, HandleCan pUserData)
        {
            CanDeviceCore.CanRxMsg pCanRxMsg;
            switch (e)
            {
                case CanDeviceCore.CanDeviceDataEvent.CanDeviceDataEventReceived:
                    pCanRxMsg = ToCanRxMsg(pData);
                    break;
                case CanDeviceCore.CanDeviceDataEvent.CanDeviceDataEventSending:
                    {
                        byte[] cData = new byte[nLen];
                        Marshal.Copy(pData, cData, 0, nLen);

                        CanDeviceMotor.CMotor pMotor = new CanDeviceMotor.CMotor();
                        pMotor = (CanDeviceMotor.CMotor)Marshal.PtrToStructure(pData, pMotor.GetType());
                    }
                    break;
            }
            return 0;
        }

        public static object BytesToStruct(byte[] bytes, Type strcutType)
        {
            int size = Marshal.SizeOf(strcutType);
            IntPtr buffer = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.Copy(bytes, 0, buffer, size);
                return Marshal.PtrToStructure(buffer, strcutType);
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
        static CanDeviceMotor.CCanMotor ToCanMotor(IntPtr pData)
        {
            CanDeviceMotor.CCanMotor pCanMotor = new CanDeviceMotor.CCanMotor();
            int size = Marshal.SizeOf(pCanMotor);
            byte[] pByte = new byte[size];
            Marshal.Copy(pData, pByte, 0, size);
            pCanMotor = (CanDeviceMotor.CCanMotor)BytesToStruct(pByte, pCanMotor.GetType());

            return pCanMotor;
        }
        static CanDeviceMotor.CMotor ToMotor(IntPtr pData)
        {
            CanDeviceMotor.CMotor pMotor = new CanDeviceMotor.CMotor();
            int size = Marshal.SizeOf(pMotor);
            byte[] pByte = new byte[size];
            Marshal.Copy(pData, pByte, 0, size);
            pMotor = (CanDeviceMotor.CMotor)BytesToStruct(pByte, pMotor.GetType());

            return pMotor;
        }
        static CanDeviceCore.CanRxMsg ToCanRxMsg(IntPtr pData)
        {
            CanDeviceCore.CanRxMsg pCanRxMsg = new CanDeviceCore.CanRxMsg();
            int size = Marshal.SizeOf(pCanRxMsg);
            byte[] pByte = new byte[size];
            Marshal.Copy(pData, pByte, 0, size);
            pCanRxMsg = (CanDeviceCore.CanRxMsg)BytesToStruct(pByte, pCanRxMsg.GetType());

            return pCanRxMsg;
        }
        static public string GetSocketCanGwIp()
        {
            string[] items = SocketCanGw.Split(':');
            if (items.Length == 2)
            {
                return items[0];
            }
            return "0.0.0.0";
        }
        static public string GetSocketCanGwPort()
        {
            string[] items = SocketCanGw.Split(':');
            if (items.Length == 2)
            {
                return items[1];
            }
            return "0";
        }
        static public uint StringToIpV4(string ip)
        {
            // http://stackoverflow.com/questions/461742/how-to-convert-an-ipv4-address-into-a-integer-in-c#
            IPAddress ipa = System.Net.IPAddress.Parse(ip);
            if (ipa.AddressFamily == AddressFamily.InterNetwork)
            {
                return (uint)BitConverter.ToInt32(ipa.GetAddressBytes().Reverse().ToArray(), 0);
            }
            else if (ipa.AddressFamily == AddressFamily.InterNetworkV6)
            {
                return 0;
            }
            return 0;
        }

        //public int UsbCanEventCallBack(HandleCan handle, UsbCan.UsbCanEvent nEvent, long reserved, IntPtr pUserData)
        //{
        //    switch (nEvent)
        //    {
        //        case UsbCan.UsbCanEvent.USB_CAN_UNINIT:  //未初始化
        //            OnUsbCanCONNECT("连接没初始化。");
        //            break;
        //        case UsbCan.UsbCanEvent.USB_CAN_CONNECTED:  //连接成功
        //            OnUsbCanCONNECT("连接成功。");
        //            break;
        //        case UsbCan.UsbCanEvent.USB_CAN_ABORTED: //连接正常后，与Usb CAN连接测试中断
        //            OnUsbCanCONNECT("连接正常后，与Usb CAN连接测试中断。");
        //            break;
        //        case UsbCan.UsbCanEvent.USB_CAN_RECONNECT:  //重新连接
        //            OnUsbCanCONNECT("重新连接。");
        //            break;
        //        case UsbCan.UsbCanEvent.USB_CAN_CLOSE:  //关闭
        //            OnUsbCanCONNECT("关闭。");
        //            break;
        //        case UsbCan.UsbCanEvent.USB_CAN_CONNECT_ERROR:  // 连接出错
        //            OnUsbCanCONNECT("连接出错。");
        //            break;
        //    }
        //    return 0;
        //}

        // 和窗体不在同一个线程，所以需要用Invoke函数的方式处理
        public int SocketCanEventCallBack(HandleCan handle, SocketCan.SocketCanEvent nEvent, int duration, IntPtr pUserData)
        {
            //if (ListBox1.InvokeRequired)
            //{
            //    // 不是同一个线程，则切换线程并调用
            //    SocketCan.SocketCanEventCallBack cb = new SocketCan.SocketCanEventCallBack(SocketCanEventCallBack);
            //    ListBox1.Invoke(cb, new object[] { handle, nEvent, duration, pUserData });
            //}
            //else
            //{
                switch (nEvent)
                {
                    case SocketCan.SocketCanEvent.SOCKET_CAN_UNINIT:  //未初始化
                        OnSocketCanCONNECT("连接没初始化。");
                        break;
                    case SocketCan.SocketCanEvent.SOCKET_CAN_INITIATING:  //初始化中
                        OnSocketCanCONNECT("连接初始化中。");
                        break;
                    case SocketCan.SocketCanEvent.SOCKET_CAN_CONNECTING: //连接中
                        OnSocketCanCONNECT("正在连接中。");
                        break;
                    case SocketCan.SocketCanEvent.SOCKET_CAN_CONNECTED:  //连接完成
                        //OnSocketCanCONNECT("连接成功。");

                        string pAddr = "";
                        ushort pPort = 0;
                        int ret = SocketCan.GetLocalAddress(hSocketCan, ref pAddr, ref pPort);
                        if (0 != ret)
                        {
                            break;
                        }
                        //TextBox_LocalIP.Text = pAddr;
                        //TextBox_LocalPort.Text = Convert.ToString(pPort);

                        OnSocketCanCONNECT("连接成功。" + pAddr + " : " + pPort);

                        break;
                    case SocketCan.SocketCanEvent.SOCKET_CAN_ABORTED:  //连接正常后，与socket CAN连接测试中断
                        OnSocketCanCONNECT("连接中断。");
                        break;
                    case SocketCan.SocketCanEvent.SOCKET_CAN_RECONNECTING:  // 重新连接
                        OnSocketCanCONNECT("重新连接中。");
                        break;
                    case SocketCan.SocketCanEvent.SOCKET_CAN_CLOSED:  //连接关闭关闭
                        OnSocketCanCONNECT("对端已经关闭连接。");
                        break;
                    case SocketCan.SocketCanEvent.SOCKET_CAN_CONNECT_ERROR:  //连接出错
                        OnSocketCanCONNECT("连接错误。");
                        break;
                }
            //}
            return 0;
        }
        //  电机返回来的资料
        public int CanDeviceMotorCallBack(HandleCan handle, ref CanDeviceMotor.CCanMotor pCanMotor, byte Cmd, IntPtr pUserData)
        {
            //CanDeviceMotor.CMotor pMotor = new CanDeviceMotor.CMotor();
            //pMotor = (CanDeviceMotor.CMotor)Marshal.PtrToStructure(pCanMotor.pData, pMotor.GetType());
            //OnMoto(pMotor);

            CanDeviceMotor.CMotor pMotor;
            IntPtr ppMotor = CanDeviceMotor.CanDeviceMotorToMotor(ref pCanMotor);
            pMotor = ToMotor(ppMotor);
            String s = String.Format("电机状态:{0:d},电机位置:{1:d}", pMotor.State, pMotor.CurrentStep);
            //ListBox_MotorResult.Items.Add(s);

            return 0;
        }

        void Connect()
        {
            bool bConnecting = SocketCan.SocketCanIsConnecting(hSocketCan);
            if (bConnecting)
            {
                //DialogResult dr;
                //dr = MessageBox.Show("正在建立连接，请稍后再操作！", "连接", MessageBoxButtons.OK,
                //         MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                return;
            }

            bool bConnected = SocketCan.SocketCanIsConnected(hSocketCan);
            if (bConnected)
            {
                //DialogResult dr;
                //dr = MessageBox.Show("连接已经成功，是否重新连接？", "连接", MessageBoxButtons.OKCancel,
                //         MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                //if (dr == DialogResult.OK)
                //{
                //    loadingCircleSocket.Active = true;
                //    loadingCircleSocket.Visible = true;
                //    loadingCircleMotor.Active = false;
                //    loadingCircleMotor.Visible = false;
                //    SocketCan.SocketCanConnect(hSocketCan, gw_ip, gw_port);
                //}
                return;
            }
            else
            {
                //loadingCircleSocket.Active = true;
                //loadingCircleSocket.Visible = true;
                //loadingCircleMotor.Active = false;
                //loadingCircleMotor.Visible = false;
                SocketCan.SocketCanConnect(hSocketCan, gw_ip, gw_port);
            }
        }
        void DisConnect()
        {
            if (hSocketCan == (IntPtr)0)
            {
                return;
            }

            bool bConnected = SocketCan.SocketCanIsConnected(hSocketCan);
            if (!bConnected)
            {
                return;
            }

            //DialogResult dr;
            //dr = MessageBox.Show("关闭连接？", "连接", MessageBoxButtons.OKCancel,
            //         MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
            //if (dr == DialogResult.OK)
            //{
            //    loadingCircleSocket.Active = true;
            //    loadingCircleSocket.Visible = true;
            //    loadingCircleMotor.Active = false;
            //    loadingCircleMotor.Visible = false;

            SocketCan.SocketCanClose(hSocketCan);
            bConnected = SocketCan.SocketCanIsConnected(hSocketCan);
            if (!bConnected)
            {
                //loadingCircleSocket.Active = false;
                //loadingCircleSocket.Visible = false;
                //ListBox1.Items.Add("连接已经关闭。");
                //TextBox_LocalIP.Text = "";
                //TextBox_LocalPort.Text = "";

                return;
            }
        }

        /// <summary>
        /// 重置
        /// </summary>
        /// <param name="myCanMotor"></param>
        /// <returns></returns>
        public int MotorReset(CanDeviceMotor.CCanMotor myCanMotor)
        {
            int ret = CanDeviceMotor.CanDeviceMotorReset(pCanDeviceMotor, ref myCanMotor);
            return ret;
        }
        /// <summary>
        /// 反转
        /// </summary>
        /// <param name="myCanMotor"></param>
        /// <param name="mSteps">前进距离</param>
        /// <returns></returns>
        public int MotorCWSteps(CanDeviceMotor.CCanMotor myCanMotor, uint mSteps = 1000)
        {
            int ret = CanDeviceMotor.CanDeviceMotorCWSteps(pCanDeviceMotor, ref myCanMotor, mSteps);
            return ret;
        }
        /// <summary>
        /// 正转
        /// </summary>
        /// <param name="myCanMotor"></param>
        /// <param name="mSteps">前进距离</param>
        /// <returns></returns>
        public int MotorCCWSteps(CanDeviceMotor.CCanMotor myCanMotor, uint mSteps = 1000)
        {
            int ret = CanDeviceMotor.CanDeviceMotorCCWSteps(pCanDeviceMotor, ref myCanMotor, mSteps);

            return ret;
        }
        /// <summary>
        /// 读出电机内的参数
        /// </summary>
        /// <param name="myCanMotor"></param>
        /// <returns></returns>
        public CanDeviceMotor.CMotor MotoData(CanDeviceMotor.CCanMotor myCanMotor)
        {
            MotorComTest(myCanMotor);
            CanDeviceMotor.CMotor pMotor = new CanDeviceMotor.CMotor();
            pMotor = (CanDeviceMotor.CMotor)Marshal.PtrToStructure(myCanMotor.pData, pMotor.GetType());
            return pMotor;

        }
        /// <summary>
        /// 定位
        /// </summary>
        /// <param name="myCanMotor">电机类</param>
        /// <param name="mPosition">定位点</param>
        /// <returns></returns>
        public int MotorPosition(CanDeviceMotor.CCanMotor myCanMotor, uint mPosition = 1000)
        {
            int ret = CanDeviceMotor.CanDeviceMotorToPosition(pCanDeviceMotor, ref myCanMotor, mPosition);
            return ret;
        }
        /// <summary>
        /// 归零
        /// </summary>
        /// <param name="myCanMotor">电机类</param>
        /// <returns></returns>
        public int MotorZero(CanDeviceMotor.CCanMotor myCanMotor)
        {
            int ret = CanDeviceMotor.CanDeviceMotorZero(pCanDeviceMotor, ref myCanMotor);
            return ret;
        }

        /// <summary>
        /// 电机停止
        /// </summary>
        /// <param name="myCanMotor">电机类</param>
        /// <returns></returns>
        public int MotorStop(CanDeviceMotor.CCanMotor myCanMotor)
        {
            int ret = CanDeviceMotor.CanDeviceMotorStop(pCanDeviceMotor, ref myCanMotor);
            return ret;
        }
        /// <summary>
        /// 更新信息
        /// </summary>
        /// <param name="myCanMotor"></param>
        /// <returns></returns>
        public int MotorComTest(CanDeviceMotor.CCanMotor myCanMotor)
        {
            int ret = CanDeviceMotor.CanDeviceMotorComTest(pCanDeviceMotor, ref myCanMotor);

            return ret;
        }

        /// <summary>
        /// 手动命令正传(1000)
        /// </summary>
        /// <param name="myCanMotor">电机</param>
        /// <param name="mPeriodUs">转动距离</param>
        /// <returns></returns>
        public int MotorSetupSpeed(CanDeviceMotor.CCanMotor myCanMotor, ushort mPeriodUs = 1000)
        {
            //public const int CMD_REQUEST  = 1;    //请求命令(需要返回) 
            myCanMotor.Command = 3;//CAN_MOTOR_CMD_CCWSTEPS
            myCanMotor.CommandType = 1;
            myCanMotor.mDataPacketNumber = 0;
            myCanMotor.mMotorData.Data = Convert.ToString(mPeriodUs);
            myCanMotor.mMotorData.SubData = 3;
            /*
             * 测试有问题
             */
            //   int ret = CanDeviceMotor.CanDeviceMotorDataSend(pCanDeviceMotor, ref myCanMotor);
            int ret = CanDeviceMotor.CanDeviceMotorSetupSpeed(pCanDeviceMotor, ref myCanMotor, mPeriodUs);
            return ret;
        }

        /// <summary>
        /// 判断输入的值是否都为数字
        /// </summary>
        /// <param name="strinput">要检查的安符串</param>
        /// <returns>true是数字false不是数字</returns>
        public bool isNumber(string strinput)//判断输入的值是否都为数字
        {
            int j = 0;
            bool allValid = false;
            char[] checkStr = strinput.ToCharArray(0, strinput.Length);//将字符串转化为字符数组
            foreach (char i in checkStr)
            {
                j++;
                if (!(char.IsDigit(i)))
                {
                    break;
                }
                if (j == (checkStr.Length - 1))
                    allValid = true;
            }
            return allValid;
        }


        public void Tick()
        {

            MotorInfo[0] = MotoData(CanMotor[0]);
            MotorInfo[1] = MotoData(CanMotor[1]);
            MotorInfo[2] = MotoData(CanMotor[2]);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void UsbCanCallBack(CanDeviceMotor.CMotor cMotor);

        public event UsbCanCallBack RunMoto;
        void OnMoto(CanDeviceMotor.CMotor cMotor)
        {
            RunMoto?.Invoke(cMotor);
        }

        public delegate void SocketCONNECT(string strMess);
        public event SocketCONNECT RunSocketCONNECT;
        void OnSocketCanCONNECT(string strMess)
        {
            RunSocketCONNECT?.Invoke(strMess);
        }

    }
}
