using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
namespace JetEazy.ControlSpace.MotoCanSpace
{
    using HandleCan = IntPtr;
    public class SocketCan
    {
        //const string DllPath = @"C:\Users\Administrator\Desktop\MotoCan -test\MotoCan\bin\x64\Debug\SocketCan.dll";
        const string DllPath = "SocketCan.dll";
        public enum SocketCanEvent
        {
            SOCKET_CAN_UNINIT,  // 未初始化
            SOCKET_CAN_INITIATING, // 初始化中
            SOCKET_CAN_CONNECTING, // 连接中
            SOCKET_CAN_CONNECTED,  // 连接完成
            SOCKET_CAN_ABORTED,  // 连接正常后，与socket CAN连接中断
            SOCKET_CAN_RECONNECTING,  // 重新连接
            SOCKET_CAN_CLOSED,  // 连接关闭
            SOCKET_CAN_CONNECT_ERROR,  // 连接出错
            SOCKET_CAN_UNKNOWN,
        }

        public static void opentest()
        {
            System.Diagnostics.Process.Start("test.txt");
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int SocketCanEventCallBack(HandleCan hSocketCan, SocketCanEvent nEvent, int duration, IntPtr pUserData);

        [DllImport(DllPath, EntryPoint = "SocketCanCreate", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern HandleCan SocketCanCreate();

        [DllImport(DllPath, EntryPoint = "SocketCanDestroy", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SocketCanDestroy(HandleCan hSocketCan);

        [DllImportAttribute(DllPath, EntryPoint = "SocketCanInit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool SocketCanInit(HandleCan hSocketCan, HandleCan hCanDevice, SocketCanEventCallBack cb, IntPtr pUserData);

        [DllImportAttribute(DllPath, EntryPoint = "SocketCanConnect", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool SocketCanConnect(HandleCan hSocketCan, uint nIpAddr, ushort nPort);

        //bool SocketCanConnectEx(HandleCan hSocketCan, sockaddr_in *pSocketAddr);

        // 关闭连接
        // 注意：关闭连接后，会向调用者报告socket的事件
        [DllImportAttribute(DllPath, EntryPoint = "SocketCanClose", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SocketCanClose(HandleCan hSocketCan);

        // 设置地址后，必须重新连接才能变为新的地址
        //void SocketCanSetAddress(HandleCan hSocketCan, sockaddr_in *pSocketAddr);

        // 允许或者禁止心跳
        [DllImportAttribute(DllPath, EntryPoint = "SocketCanEnableHeatBeat", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SocketCanEnableHeatBeat(HandleCan hSocketCan, bool bEnable);

        // 允许或者禁止自动重连接
        [DllImportAttribute(DllPath, EntryPoint = "SocketCanEnableAutoReconnect", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SocketCanEnableAutoReconnect(HandleCan hSocketCan, bool bEnable);

        [DllImportAttribute(DllPath, EntryPoint = "SocketCanIsConnected", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool SocketCanIsConnected(HandleCan hSocketCan);

        [DllImportAttribute(DllPath, EntryPoint = "SocketCanIsConnecting", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool SocketCanIsConnecting(HandleCan hSocketCan);

        /* 
        * 直接发送数据(一般只用于调试目的，必须按照内部的数据结构组成数据，才能最终发送成功)
        * 返回值：
        * 0: 没有发送数据
        * nLen(>0): 数据被成功放到发送队列
        */
        [DllImportAttribute(DllPath, EntryPoint = "SocketCanSendData", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SocketCanSendData(HandleCan hSocketCan, string pData, int nLen);

        [DllImportAttribute(DllPath, EntryPoint = "SocketCanGetGwAddress", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SocketCanGetGwAddress(HandleCan hSocketCan, IntPtr pAddrBuffer, ref int pBufLen, ref ushort pPort);
        public static int GetGwAddress(HandleCan hSocketCan, ref string pAddr, ref ushort pPort)
        {
            int pBufLen = 0;
            byte[] pAddrBuffer = new byte[16];
            IntPtr buffer = Marshal.AllocHGlobal(16);
            int ret = SocketCanGetGwAddress(hSocketCan, buffer, ref pBufLen, ref pPort);
            if (0 != ret)
            {
                Marshal.FreeHGlobal(buffer);
                return -1;
            }
            Marshal.Copy(buffer, pAddrBuffer, 0, pBufLen);
            Marshal.FreeHGlobal(buffer);
            pAddr = System.Text.Encoding.Default.GetString(pAddrBuffer); ;
            return 0;
        }

        [DllImportAttribute(DllPath, EntryPoint = "SocketCanGetLocalAddress", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SocketCanGetLocalAddress(HandleCan hSocketCan, IntPtr pAddrBuffer, ref int pBufLen, ref ushort pPort);
        public static int GetLocalAddress(HandleCan hSocketCan, ref string pAddr, ref ushort pPort)
        {
            int pBufLen = 16;
            byte[] pAddrBuffer = new byte[pBufLen];
            IntPtr buffer = Marshal.AllocHGlobal(pBufLen);
            int ret = SocketCanGetLocalAddress(hSocketCan, buffer, ref pBufLen, ref pPort);
            if (0 != ret)
            {
                Marshal.FreeHGlobal(buffer);
                return -1;
            }
            Marshal.Copy(buffer, pAddrBuffer, 0, pBufLen);
            Marshal.FreeHGlobal(buffer);
            pAddr = System.Text.Encoding.Default.GetString(pAddrBuffer); ;
            return 0;
        }

        [DllImportAttribute(DllPath, EntryPoint = "SocketCanToCanSenderProxy", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern HandleCan SocketCanToCanSenderProxy(HandleCan hSocketCan);
    }
}
