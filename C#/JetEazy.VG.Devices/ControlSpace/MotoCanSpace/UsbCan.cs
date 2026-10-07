using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
namespace JetEazy.ControlSpace.MotoCanSpace
{
  using HandleCan = IntPtr;
  public class UsbCan
  {
    public enum UsbCanEvent
    {
      USB_CAN_UNINIT,  //未初始化
      USB_CAN_CONNECTED,  //连接成功
      USB_CAN_ABORTED,  //连接正常后，与USB CAN连接中断
      USB_CAN_RECONNECT,  // 重新连接
      USB_CAN_CLOSE,  //关闭
      USB_CAN_CONNECT_ERROR,  //连接错误
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int UsbCanEventCallBack(HandleCan hUsbCan, UsbCanEvent nEvent, long reserved, IntPtr pUserData);

    [DllImport(@"D:\AUTOMATION\Eazy AOI DX\Allinone\Allinone\bin\x64\Debug\UsbCan.dll", EntryPoint = "UsbCanCreate", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    public static extern HandleCan UsbCanCreate();

    [DllImport("UsbCan.dll", EntryPoint = "UsbCanDestroy", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    public static extern void UsbCanDestroy(HandleCan hUsbCan);

    [DllImportAttribute("UsbCan.dll", EntryPoint = "UsbCanInit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool UsbCanInit(HandleCan hUsbCan, HandleCan hCanDevice, UsbCanEventCallBack cb, IntPtr pUserData);

    /* 
     * 直接发送数据(一般只用于调试目的，必须按照内部的数据结构组成数据，才能最终发送成功)
     * 返回值：
     * 0: 没有发送数据
     * nLen(>0): 数据被成功放到发送队列
     */
    [DllImportAttribute("UsbCan.dll", EntryPoint = "UsbCanSendData", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    public static extern int UsbCanSendData(HandleCan hUsbCan, string pData, int nLen);

    [DllImportAttribute("UsbCan.dll", EntryPoint = "UsbCanToCanSenderProxy", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    public static extern HandleCan UsbCanToCanSenderProxy(HandleCan hUsbCan);
  }
}
