using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace JetEazy.ControlSpace.MotoCanSpace
{
    using HandleCan = IntPtr;
    public class CanDeviceCore
    {

        //==========================================================================//
        // bit[10:6]: 32种类型设备    | bit[5:0]:每种类型设备 63个 地址(板子)序列   ||
        //==========================================================================||
        // bit[10:7] = 0: Special     | bit[5:0] = 0:查询此类设备 每种设备从1开始   ||
        //==========================================================================||
        // bit[10:7] = 1: IO          | 0x080[000 0100 0000]                        ||
        // bit[10:7] = 2: Motor       | 0x100[000 1000 0000]                        ||
        // bit[10:7] = 3: Pannel      | 0x180[000 1100 0000]                        ||
        // bit[10:7] = 4: AD          | 0x200[001 0000 0000]                        ||
        // bit[10:7] = 5: DA          | 0x200[001 0100 0000]                        ||
        //                            | 0x208[001 1000 0000]                        ||
        //==========================================================================||

        public enum CAN_DEVICE_TYPE
        {
            DEVICE_IO = 64,    // Type defined for CAN AD
            DEVICE_DA = 128,
            DEVICE_MOTOR = 192,
            DEVICE_PANNEL = 256,
            DEVICE_AD = 320   //Type defined for CAN AD
        }

        public enum CanDeviceDataEvent
        {
            CanDeviceDataEventNone = 0,
            CanDeviceDataEventReceived,
            CanDeviceDataEventSending,
        }

        [StructLayoutAttribute(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct CanRxMsg
        {
            public uint StdId;
            public uint ExtId;
            public byte IDE;
            public byte RTR;
            public byte DLC;
            [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 8)]
            public string Data;
        }

        public partial class NativeConstants
        {
            public const int LINK_UNDEF = 0;
            public const int LINK_NOACK = 1;
            public const int LINK_ACK = 2;
        }

        [StructLayoutAttribute(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct CCanMsg
        {
            public byte mCanInfo;
            public uint mCanId;
            [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 8)]
            public string Data;
        }
        enum CommandType
        {
            CMD_TYPE_BROADCAST = 0x00,  //广播命令(不需要返回)  broadcast
            CMD_TYPE_REQUEST = 0x01,  //请求命令(需要返回)
            CMD_TYPE_ACK = 0x02,  //CMD_TYPE_REQUEST命令的正确返回
            CMD_TYPE_NAK = 0x03,  //CMD_TYPE_REQUEST命令的正确返回
            CMD_TYPE_DATA = 0x04,  //CMD_TYPE_REQUEST命正确返回的数据帧(DATA[3:7])
            CMD_TYPE_NOCMD = 0x05,  //无此命令码
            CMD_TYPE_ERR_PARAM = 0x06,  //命令参数错误
            CMD_TYPE_MAX = 0x08,  //最大命令类型
        };

        [StructLayoutAttribute(LayoutKind.Sequential, Pack = 1)]
        public struct CCanObjectHeader
        {
            public ushort id;//通信ID
            public IntPtr pNextObject;//指向下一个通信资源的指针
            public IntPtr pReserve;
            //CAN通信 公共通信部分
            public CAN_DEVICE_TYPE DeviceType;//CAN设备通信独有的对设备类型的说明

            public uint bitvector1;
            public ushort LocalDeviceAddr//本机地址
            {
                get
                {
                    return ((ushort)((this.bitvector1 & 2047u)));
                }
                set
                {
                    this.bitvector1 = ((ushort)((value | this.bitvector1)));
                }
            }

            public ushort mReserve1
            {
                get
                {
                    return ((ushort)(((this.bitvector1 & 63488u)
                                / 2048)));
                }
                set
                {
                    this.bitvector1 = ((ushort)(((value * 2048u)
                                | this.bitvector1)));
                }
            }

            public ushort RequestDeviceAddr//发到本机的设备地址
            {
                get
                {
                    return ((ushort)(((this.bitvector1 & 134152192u)
                                / 65536)));
                }
                set
                {
                    this.bitvector1 = ((ushort)(((value * 65536u)
                                | this.bitvector1)));
                }
            }

            public ushort PacketLabel
            {
                get
                {
                    return ((ushort)(((this.bitvector1 & 4160749568u)
                                / 134217728)));
                }
                set
                {
                    this.bitvector1 = ((ushort)(((value * 134217728u)
                                | this.bitvector1)));
                }
            }
        }

        public const int CAN_DATABUF_IDLE = 0;
        public const int CAN_DATABUF_UNDEF = 31;
        public const int CAN_DATABUF_UNPROCSSE = 1;
        public const int CAN_DATABUF_BUZY = 2;


        [StructLayoutAttribute(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct CProtocolCanComData
        {
            //辅助部分
            public uint mRxDeviceAddr; //接收CAN帧的地址  发送Can帧的地址(RequestDeviceAddr)

            //LocalDeviceAddr : 11
            //DataState : 5
            //RequestDeviceAddr : 11
            //PacketLabel : 5
            public uint bitvector1;
            [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 6)]
            public string Data;
            public ushort LocalDeviceAddr//单元地址
            {
                get
                {
                    return ((ushort)((this.bitvector1 & 2047u)));
                }
                set
                {
                    this.bitvector1 = ((ushort)((value | this.bitvector1)));
                }
            }

            public ushort DataState//0~31 缓冲区状态:
            {
                get
                {
                    return ((ushort)(((this.bitvector1 & 63488u)
                                / 2048)));
                }
                set
                {
                    this.bitvector1 = ((ushort)(((value * 2048u)
                                | this.bitvector1)));
                }
            }

            public ushort RequestDeviceAddr //发到本机的设备地址 针对每个地址单独处理
            {                             //接收时:对方的地址   发送时:本机的地址  
                get
                {
                    return ((ushort)(((this.bitvector1 & 134152192u)
                                / 65536)));
                }
                set
                {
                    this.bitvector1 = ((ushort)(((value * 65536u)
                                | this.bitvector1)));
                }
            }

            public ushort PacketLabel  //数据帧序列,当PacketLabel=0为最后一帧数据:
            {                      //一个通信命令 支持数据长度为:32*(8-2)=192
                get
                {
                    return ((ushort)(((this.bitvector1 & 4160749568u)
                                / 134217728)));
                }
                set
                {
                    this.bitvector1 = ((ushort)(((value * 134217728u)
                                | this.bitvector1)));
                }
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int CanDeviceCoreCallBack(
            HandleCan hCanDeviceCore,
            CanDeviceDataEvent e,
            IntPtr pData,
            int nLen,
            IntPtr pUserData
            );

        [DllImport("CanDeviceCore.dll", EntryPoint = "CanDeviceCoreCreate", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern HandleCan CanDeviceCoreCreate(HandleCan hCanDeviceDefault);

        [DllImport("CanDeviceCore.dll", EntryPoint = "CanDeviceCoreDestroy", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void CanDeviceCoreDestroy(HandleCan hCanDeviceCore);

        [DllImport("CanDeviceCore.dll", EntryPoint = "CanDeviceCoreInit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAsAttribute(UnmanagedType.I1)]
        public static extern bool CanDeviceCoreInit(HandleCan hCanDeviceCore, HandleCan hCanSenderProxy, CanDeviceCoreCallBack cb, IntPtr pUserData);

        [DllImport("CanDeviceCore.dll", EntryPoint = "CanDeviceSetSibling", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool CanDeviceSetSibling(HandleCan hCanDeviceCore, HandleCan sibling);

        [DllImport("CanDeviceCore.dll", EntryPoint = "CanDeviceGetSibling", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern HandleCan CanDeviceGetSibling(HandleCan hCanDeviceCore);

        [DllImport("CanDeviceCore.dll", EntryPoint = "CanDeviceCoreToCanDevice", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern HandleCan CanDeviceCoreToCanDevice(HandleCan hCanDeviceCore);
    }
}
