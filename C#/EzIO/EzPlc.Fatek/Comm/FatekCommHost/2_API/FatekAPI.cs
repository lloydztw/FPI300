#region AUTHOR
/*
 * EzPlc.Fatek
 * Copyright (C) 2026
 * 2026-04-05 LeTian Chang: Integration with EzIO
 * 2012-12-04 LeTian Chang: ReOpen UART when communication failed.
 * 2012-06-22 LeTian Chang: Revised for more robust over RS232 connection.
 * 2008-07-01 LeTian Chang: Creation.
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzComm.Uart;
using System;
using System.Text;
using static EzPlc.Fatek.Comm.FatekUartCommHost;
using ADDR = EzPlc.Fatek.FatekAddr;


namespace EzPlc.Fatek.Comm.Api
{
    /// <summary>
    /// 支持舊有的 Fatek 通訊指令, 使用 (sync) 同步阻塞式調用方式.
    /// </summary>
    /// <remarks>
    /// 這是個 Wrapper Class, _fatekComm 需要由 上層 owner 維持其生命週期!
    /// </remarks>
    public class FatekAPI : IxFatekApi
    {
        #region PRIVATE_DATA
        IxFatekComm _fatekComm;
        #endregion

        internal FatekAPI(IxFatekComm comm)
        {
            _fatekComm = comm;
        }

        internal IxFatekComm FatekComm
        {
            get => _fatekComm;
        }
        public int ComPort
        {
            get => 0;
        }

        public string Echo(string message)
        {
            var cmdEcho = new FkCmd_Echo(message);
            var rsp = _fatekComm.SendCmd(cmdEcho, 2500);
            _ASSERT(rsp, true);
            return rsp != null ? rsp.Context : null;
        }
        public bool IsRunning()
        {
            byte[] status = new byte[3];
            var cmdReadStatus = new FkCmd_ReadPlcStatus(status);
            var rsp = _fatekComm.SendCmd(cmdReadStatus);
            _ASSERT(rsp, true);
            return (status[0] & 0x01) != 0;
        }
        public void Run()
        {
            var cmdRunPlc = new FkCmd_RunStopPlc(true);
            var rsp = _fatekComm.SendCmd(cmdRunPlc, 2500);
            _ASSERT(rsp, true);
        }
        public void Stop()
        {
            var cmdStopPlc = new FkCmd_RunStopPlc(false);
            var rsp = _fatekComm.SendCmd(cmdStopPlc);
            _ASSERT(rsp, true);
        }

        public void SetSinglePoint(string address, bool on)
        {
            var cmdSetOnePoint = new FkCmd_SetSinglePoint(ADDR.TryParse(address), on);
            var rsp = _fatekComm.SendCmd(cmdSetOnePoint);
            _ASSERT(rsp);
        }
        public bool ReadSinglePoint(string address)
        {
            bool[] flags = new bool[1];
            var cmdReadOnePoint = new FkCmd_ReadContiSinglePoints(ADDR.TryParse(address), flags);
            var rsp = _fatekComm.SendCmd(cmdReadOnePoint);
            _ASSERT(rsp);
            return flags[0];
        }
        public void ReadSinglePoints(string address, bool[] flags)
        {
            var cmdReadSinglePoints = new FkCmd_ReadContiSinglePoints(ADDR.TryParse(address), flags);
            var rsp = _fatekComm.SendCmd(cmdReadSinglePoints);
            _ASSERT(rsp);
        }

        public void WriteRegister(string address, uint uData)
        {
            var cmdWriteOneReg = new FkCmd_WriteContiRegisters(ADDR.TryParse(address), new uint[] { uData });
            var rsp = _fatekComm.SendCmd(cmdWriteOneReg);
            _ASSERT(rsp);
        }
        public void WriteRegisters(string address, uint[] arrData)
        {
            var cmdWriteRegs = new FkCmd_WriteContiRegisters(ADDR.TryParse(address), arrData);
            var rsp = _fatekComm.SendCmd(cmdWriteRegs);
            _ASSERT(rsp);
        }

        public uint ReadRegister(string address)
        {
            uint[] buf = new uint[1];
            var cmdReadOneReg = new FkCmd_ReadContiRegisters(ADDR.TryParse(address), buf);
            var rsp = _fatekComm.SendCmd(cmdReadOneReg);
            _ASSERT(rsp);
            return buf[0];
        }
        public short ReadRegisterI16(string address)
        {
            uint u = ReadRegister(address);
            if (u == uint.MaxValue)
                throw new Exception("PLC RS232 commm error!");
            short s = (short)u;
            return s;
        }

        public void ReadRegisters(string address, int number, Action<int, uint> updateFunc)
        {
            var cmdReadContiRegs = new FkCmd_ReadContiRegisters(ADDR.TryParse(address), number, updateFunc);
            var rsp = _fatekComm.SendCmd(cmdReadContiRegs);
            _ASSERT(rsp);
        }
        public void ReadRegisters32(string address, uint[] dstBuf)
        {
            //int err = 
            ReadRegisters(address, dstBuf.Length, (idx, data) =>
            {
                //if (data == uint.MaxValue)
                //    throw new Exception("PLC RS232 commm error!");

                dstBuf[idx] = data;
            });
            //return err;
        }
        public void ReadRegisters16(string address, int[] dstBuf)
        {
            //int err = 
            ReadRegisters(address, dstBuf.Length, (idx, data) =>
            {
                //if (data == uint.MaxValue)
                //    throw new Exception("PLC RS232 commm error!");

                dstBuf[idx] = (int)(short)data;
            });
            //return err;
        }
        public void ReadMixPoints(string[] addressGrp, Action<object, int, uint> updateFunc)
        {
            int N = addressGrp.Length;
            var addrGrp = new ADDR[N];
            for (int i = 0; i < N; i++)
                addrGrp[i] = ADDR.TryParse(addressGrp[i]);

            var cmdReadMixRegs = new FkCmd_ReadMixRegisters(addrGrp, updateFunc);
            var rsp = _fatekComm.SendCmd(cmdReadMixRegs);

            _ASSERT(rsp);
        }

        int _ASSERT(CUartCmdResult rsp, bool throwEx = false)
        {
            if (rsp == null || rsp.Error != 0)
            {
                // 在 m_fatekComm 應該已經處理過了.

                if (throwEx)
                {
                    // 啟始 Commands:　Echo, IsRunning, Run/Stop 
                    // 直接 throw exception 
                    // 可以縮短 啟始 等待時間.
                    if (rsp == null)
                        rsp = CUartCmdResult.NullResult(null);
                    throw new FatekErrorResultException(rsp, "FatekCmdApi");
                }

                return (rsp != null) ? rsp.Error : -1;
            }
            return 0;
        }
    }
}
