#region AUTHOR
/*
 * EzPlc.Fatek
 * 
 * Copyright (C) 2026
 * 2026-04-05 LeTian Chang : Integration with EzIO
 * 2013-07-11 LeTian Chang : 重整
 * 2019-11-30 LeTian Chang : Creation   
 *  
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzComm;
using EzComm.Uart;
using EzIO.Device;
using EzIO.Mem;
using EzIO.Sim;
using EzPlc.Fatek.AutoScan;
using EzPlc.Fatek.Comm;
using System;
using System.Collections.Generic;
using System.Linq;
using IAddress = EzIO.Mem.IAddress;


namespace EzPlc.Fatek
{
    /// <summary>
    /// Fatek 對 IoDevice 之實作 
    /// </summary>
    public class FatekIoDevice : IoDevice
    {
        public event EventHandler OnFinalDisposing;

        #region NLOG
        NLog.Logger _LOG => EzLog.LOG;
        #endregion

        #region PRIVATE_DATA
        EzUartSettings _comSettings;
        IxFatekComm _comm = null;
        byte _stationID;
        #endregion

        #region AUTO_SCANNER
        IAutoScan _autoScanner;
        IDisposable _sim;
        #endregion

        public FatekIoDevice(EzUartSettings settings, int stationID = 1)
        {
            _stationID = (byte)stationID;
            _comSettings = settings;

            if (_comm == null)
                _comm = FatekCommFactory.OpenPlcComm(settings, stationID);
        }

        public void Dispose()
        {
            #region 停止_SIMULATOR
            _sim?.Dispose();
            _sim = null;
            #endregion

            #region 停止_AUTO_SCAN
            try
            {
                _autoScanner?.Stop();
                _autoScanner = null;
            }
            catch(Exception ex)
            {
                _LOG.Error(ex);
            }
            #endregion

            OnFinalDisposing?.Invoke(this, null);
            _comm?.Dispose();
            _comm = null;
        }

        #region INTERNAL_COMMM
        internal IxFatekComm FatekComm
        {
            get => _comm;
        }
        #endregion

        public string KeyName
        {
            get => EzUartSettings.GetKeyName("Fatek", _comSettings.ComPort, _comSettings.IsSim);
        }
        public override string ToString()
        {
            return KeyName;
        }

        byte IoDevice.StationID
        {
            get => _stationID;
        }
        bool IoDevice.IsSim
        {
            get => _comSettings == null || _comSettings.IsSim;
        }

        void IoDevice.WriteBit(IAddress addr, bool value)
        {
            var fkAddr = addr.ToFatekAddr();
            var fkCmd = new FkCmd_SetSinglePoint(fkAddr, value, addr.StationID);
            var rsp = _comm.SendCmd(fkCmd);
            _ASSERT(rsp);
        }
        void IoDevice.WriteReg(IAddress addr, uint value)
        {
            var fkAddr = addr.ToFatekAddr();
            var fkCmd = new FkCmd_WriteContiRegisters(fkAddr, value, addr.StationID);
            var rsp = _comm.SendCmd(fkCmd);
            _ASSERT(rsp);
        }
        void IoDevice.WriteRegs(IAddress addr, uint[] values)
        {
            var fkAddr = addr.ToFatekAddr();
            var fkCmd = new FkCmd_WriteContiRegisters(fkAddr, values, addr.StationID);
            var rsp = _comm.SendCmd(fkCmd);
            _ASSERT(rsp);
        }

        bool IoDevice.ReadBit(IAddress addr)
        {
            bool[] flags = new bool[1];
            var fkAddr = addr.ToFatekAddr();
            var fkCmd = new FkCmd_ReadContiSinglePoints(fkAddr, flags, addr.StationID);
            var rsp = _comm.SendCmd(fkCmd);
            _ASSERT(rsp);
            return flags[0];
        }
        int IoDevice.ReadBits(IAddress addr, int N, ReadCallback<int, bool> callback, object arg)
        {
            var fkAddr = addr.ToFatekAddr();
            var fkCmd = new FkCmd_ReadContiSinglePoints(
                    fkAddr, N,
                    (idx, value) =>
                    {
                        callback(idx, value, arg);
                    },
                    addr.StationID
                );
            var rsp = _comm.SendCmd(fkCmd);
            _ASSERT(rsp);
            return N;
        }
        
        uint IoDevice.ReadReg(IAddress addr)
        {
            uint[] buf = new uint[1];
            var fkAddr = addr.ToFatekAddr();
            var fkCmd = new FkCmd_ReadContiRegisters(fkAddr, buf);
            var rsp = _comm.SendCmd(fkCmd);
            _ASSERT(rsp);
            return buf[0];
        }
        int IoDevice.ReadRegs(IAddress addr, int N, ReadCallback<int, uint> callback, object arg)
        {
            var fkAddr = addr.ToFatekAddr();
            var fkCmd = new FkCmd_ReadContiRegisters(
                    fkAddr, N,
                    (idx, value) =>
                    {
                        callback(idx, value, arg);
                    },
                    addr.StationID
                );
            var rsp = _comm.SendCmd(fkCmd);
            _ASSERT(rsp);
            return N;
        }
        int IoDevice.ReadRegs(IEnumerable<IAddress> mixAddrs, ReadCallback<IAddress, uint> callback, object arg)
        {
            var fkAddrs = Array.ConvertAll(mixAddrs.ToArray(), a => a.ToFatekAddr());
            if (fkAddrs.Length == 0)
                return 0;

            int count = 0;
            var stationID = fkAddrs[0].StationID;
            var fkCmd = new FkCmd_ReadMixRegisters(
                    fkAddrs,
                    (addr, idx, value) =>
                    {
                        callback(addr as IAddress, value, arg);
                        count++;
                    },
                    stationID
                );

            var rsp = _comm.SendCmd(fkCmd);
            _ASSERT(rsp);
            return count;
        }

        /// <summary>
        /// 建立 AutoScan
        /// </summary>
        /// <remarks>
        /// 參數可以為 empty 或 IEnumerable ( IoPointReg )
        /// </remarks>
        IAutoScan IoDevice.InstanceAutoScan(params object[] args)
        {
            // 只能建構一次
            if (_autoScanner == null)
            {
                // 取得第一個 arg
                var arg = args.Length > 0 ? args[0] : null;

                if (!(arg is IEnumerable<IoPointReg> ioRegs))
                {
                    // 如果 arg 是 null 或其他型別;
                    // 則使用 FatekIoMemory 內部所有已經存在的 IoRegs.
                    var ioMem = this.GetFatekIoMemory();
                    ioRegs = new List<IoPointReg>(ioMem.IterRegs());
                }

                var builder = new FatekScanCmdsBuilder();
                var scanCmds = builder.BuildCmds(this, ioRegs);
                _autoScanner = new FatekAutoScanner(this, scanCmds);
                _TRACE(scanCmds);

                #region RANDOM_SIM
                if (_comSettings.IsSim && _comSettings.UsingRandomSim)
                {
                    var sim = new IoRandomSimulator(ioRegs);
                    _sim = sim;
                    sim.Start();
                }
                #endregion
            }
            return _autoScanner;
        }

        #region ERROR_AND_EXCEPTIONS
        protected int _ASSERT(CUartCmdResult rsp, bool throwEx = false)
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

                    throw new FatekUartCommHost.FatekErrorResultException(rsp, "FatekPlcComm");
                }

                return (rsp != null) ? rsp.Error : -1;
            }
            return 0;
        }
        #endregion

        #region DEBUG_AND_TRACE
        void _TRACE(FkCmd[] cmds)
        {
            var strs = Array.ConvertAll(cmds, c => c?.ToString());
            var msg = "AutoScan CMDS:\n" + string.Join("\n", strs);
            System.Diagnostics.Debug.WriteLine(msg);
            _LOG.Trace(msg);
        }
        #endregion
    }
}
