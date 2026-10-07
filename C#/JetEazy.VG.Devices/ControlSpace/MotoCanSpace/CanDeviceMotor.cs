using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace JetEazy.ControlSpace.MotoCanSpace
{
    using HandleCan = IntPtr;
    public class CanDeviceMotor
    {
        // 电机运行参数及状态  
        [StructLayoutAttribute(LayoutKind.Sequential, Pack = 1)]
        public struct CMotor
        {
            public ushort LocalAddr;       // 模块地址                  默认x     [需主机设置]
            public byte Subdivide;         // 电机运行细分              默认8     [需主机设置]
            public byte ResetMode;         // 复位方式                  默认0     [需主机设置]
            public uint AccSteps;          // 加速步数 <=SLOW_START_BUF_LENGTH/2  [需主机设置]
            public float AccCof;           // 加速系数                            [需主机设置]
            public ushort PlusStartTime;   // 脉冲高电平时间:启动的速度 默认100us [需主机设置]
            public ushort PlusConstantTime;// 脉冲高电平时间:恒定速度   默认10us  [需主机设置]
            public uint MaxStep;           // 最大步数                  默认1600  [需主机设置]
            public int ZeroStep;           // 设置零位位置              默认0     [需主机设置]
            public uint SwitchOffStep;     // 开关脱离步数              默认1000  [需主机设置]
            public byte State;             // 电机状态 
            public int CurrentStep;        // 当前步数
            public int ObjectStep;         // 目标步数		
        }


        [StructLayoutAttribute(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct CMotorData
        {
            /// unsigned char[4]
            [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 4)]
            public string Data;

            /// unsigned char
            public byte SubData;
        }


        //CAN 类型设备的 AD数据格式:数据格式一样，不同类型设备的设备头不一样
        [StructLayoutAttribute(LayoutKind.Sequential, Pack = 1)]
        public struct CCanMotor
        {
            public CanDeviceCore.CCanObjectHeader mCanObject;

            public byte bitvector1;
            /// CommandType : 3
            ///Command : 5
            public uint CommandType
            {
                get
                {
                    return ((byte)((this.bitvector1 & 7u)));
                }
                set
                {
                    this.bitvector1 = ((byte)((value | this.bitvector1)));
                }
            }

            public uint Command
            {
                get
                {
                    return ((byte)(((this.bitvector1 & 248u)
                                / 8)));
                }
                set
                {
                    this.bitvector1 = ((byte)(((value * 8u)
                                | this.bitvector1)));
                }
            }

            public CMotorData mMotorData;

            /// unsigned short
            public ushort mDataPacketNumber;

            /// void*
            public IntPtr pData;

            /// unsigned char
            public byte mComState;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int CanDeviceMotorCallBack(HandleCan hCanDeviceMotor, ref CCanMotor pCanMotor, byte Cmd, IntPtr pUserData);

        [DllImport("CanDeviceCore.dll", EntryPoint = "CanDeviceMotorCreate", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr CanDeviceMotorCreate();

        [DllImport("CanDeviceCore.dll", EntryPoint = "CanDeviceMotorDestroy", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void CanDeviceMotorDestroy(HandleCan hCanDeviceMotor);

        [DllImport("CanDeviceCore.dll", EntryPoint = "CanDeviceMotorToCanDevice", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern HandleCan CanDeviceMotorToCanDevice(HandleCan hCanDeviceMotor);

        [DllImport("CanDeviceCore.dll", EntryPoint = "CanDeviceMotorInit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool CanDeviceMotorInit(HandleCan hCanDeviceMotor, HandleCan hCanDeviceCore, CanDeviceMotorCallBack cb, IntPtr pUserData);

        [DllImport("CanDeviceCore.dll", EntryPoint = "CanDeviceMotorCreateMotor", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr CanDeviceMotorCreateMotor(HandleCan hCanDeviceMotor, ushort ID, ushort DeviceAddr);

        [DllImport("CanDeviceCore.dll", EntryPoint = "CanDeviceMotorDelete", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void CanDeviceMotorDelete(HandleCan hCanDeviceMotor, ref CCanMotor pCanMotor);

        [DllImport("CanDeviceCore.dll", EntryPoint = "CanDeviceMotorDeleteAll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void CanDeviceMotorDeleteAll(HandleCan hCanDeviceMotor);

        [DllImport("CanDeviceCore.dll", EntryPoint = "CanDeviceMotorAssert", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool CanDeviceMotorAssert(HandleCan hCanDeviceMotor, ref CCanMotor pCanMotor);

        [DllImport("CanDeviceCore.dll", EntryPoint = "CanDeviceMotorFind", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int CanDeviceMotorFind(HandleCan hCanDeviceMotor);

        [DllImport("CanDeviceCore.dll", EntryPoint = "CanDeviceMotorGetObject", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr CanDeviceMotorGetObject(HandleCan hCanDeviceMotor, ushort ID);

        [DllImport("CanDeviceCore.dll", EntryPoint = "CanDeviceMotorSendData", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int CanDeviceMotorSendData(HandleCan hCanDeviceMotor, ref CCanMotor pCanMotor);

        [DllImport("CanDeviceCore.dll", EntryPoint = "CanDeviceMotorComTest", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int CanDeviceMotorComTest(HandleCan hCanDeviceMotor, ref CCanMotor pCanMotor);

        [DllImport("CanDeviceCore.dll", EntryPoint = "CanDeviceMotorReset", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int CanDeviceMotorReset(HandleCan hCanDeviceMotor, ref CCanMotor pCanMotor);

        [DllImport("CanDeviceCore.dll", EntryPoint = "CanDeviceMotorStop", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int CanDeviceMotorStop(HandleCan hCanDeviceMotor, ref CCanMotor pCanMotor);

        [DllImport("CanDeviceCore.dll", EntryPoint = "CanDeviceMotorToPosition", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int CanDeviceMotorToPosition(HandleCan hCanDeviceMotor, ref CCanMotor pCanMotor, uint mObectPositon);

        [DllImport("CanDeviceCore.dll", EntryPoint = "CanDeviceMotorToPositionExtend", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int CanDeviceMotorToPositionExtend(HandleCan hCanDeviceMotor, ref CCanMotor pCanMotor, uint mObectPositon, byte mExtendCode);

        [DllImport("CanDeviceCore.dll", EntryPoint = "CanDeviceMotorCCWSteps", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int CanDeviceMotorCCWSteps(HandleCan hCanDeviceMotor, ref CCanMotor pCanMotor, uint mSteps);

        [DllImport("CanDeviceCore.dll", EntryPoint = "CanDeviceMotorCCWStepsExtend", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int CanDeviceMotorCCWStepsExtend(HandleCan hCanDeviceMotor, ref CCanMotor pCanMotor, uint mSteps, uint mExtendCode);

        [DllImport("CanDeviceCore.dll", EntryPoint = "CanDeviceMotorCWSteps", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int CanDeviceMotorCWSteps(HandleCan hCanDeviceMotor, ref CCanMotor pCanMotor, uint mSteps);

        [DllImport("CanDeviceCore.dll", EntryPoint = "CanDeviceMotorCWStepsExtend", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int CanDeviceMotorCWStepsExtend(HandleCan hCanDeviceMotor, ref CCanMotor pCanMotor, uint mSteps, uint mExtendCode);

        [DllImport("CanDeviceCore.dll", EntryPoint = "CanDeviceMotorZero", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int CanDeviceMotorZero(HandleCan hCanDeviceMotor, ref CCanMotor pCanMotor);

        [DllImport("CanDeviceCore.dll", EntryPoint = "CanDeviceMotorSetupSpeed", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int CanDeviceMotorSetupSpeed(HandleCan hCanDeviceMotor, ref CCanMotor pCanMotor, ushort mPeriodUs);

        [DllImport("CanDeviceCore.dll", EntryPoint = "CanDeviceMotorToMotor", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr CanDeviceMotorToMotor(ref CCanMotor pCanMotor);
    }
}
