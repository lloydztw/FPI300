#region AUTHOR
/*
 * EzPlc.Hcfa
 * 
 * Copyright (C) 2026
 * 
 * 2026-04-05 LeTian Chang : Integration with EzIO
 *  
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzComm;
using EzIO.Device;
using EzIO.Mem;
using EzIO.Modbus.AutoScan;
using EzIO.Sim;
using System;
using System.Collections.Generic;
using System.Linq;
using EzModbus = EzIO.Modbus.NModbus4.EzModbus;
using IAddress = EzIO.Mem.IAddress;

namespace EzPlc.Hcfa
{
    /// <summary>
    /// Hcfa 對 IoDevice 之實作 
    /// </summary>
    public class HcfaIoDevice : IoDevice
    {
        public event EventHandler OnFinalDisposing;
        public event EventHandler OnError;

        #region NLOG
        NLog.Logger _LOG => EzLog.LOG;
        #endregion

        #region PRIVATE_DATA
        EzTcpIpSettings _settings;
        EzModbus _comm;
        IoDevice _ioDev => _comm;
        byte _stationID;
        #endregion

        #region AUTO_SCANNER
        IAutoScan _autoScanner;
        IDisposable _sim;
        #endregion

        public HcfaIoDevice(EzTcpIpSettings settings, int stationID = 0)
        {
            _stationID = (byte)stationID;
            _settings = settings;
            
            if (!_settings.IsSim)
            {
                _comm = new EzModbus(stationID, settings);
                _comm.OnError += (s, e) => OnError?.Invoke(s, e);
            }
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
        internal EzModbus ModbusComm
        {
            get => _comm;
        }
        #endregion

        public string KeyName
        {
            get => EzTcpIpSettings.GetKeyName("Hcfa", _settings.IP, _settings.Port, _settings.IsSim);
        }
        public override string ToString()
        {
            return KeyName;
        }

        #region IO_DEVICE_IMPLEMENTAION
        byte IoDevice.StationID
        {
            get => _stationID;
        }
        bool IoDevice.IsSim
        {
            get => _settings == null || _settings.IsSim;
        }

        void IoDevice.WriteBit(IAddress addr, bool value)
        {
            _ioDev?.WriteBit(addr, value);
        }
        void IoDevice.WriteReg(IAddress addr, uint value)
        {
            _ioDev?.WriteReg(addr, value);
        }
        void IoDevice.WriteRegs(IAddress addr, uint[] values)
        {
            _ioDev?.WriteRegs(addr, values);
        }

        bool IoDevice.ReadBit(IAddress addr)
        {
            if (_ioDev == null)
                return false;
            return _ioDev.ReadBit(addr);
        }
        int IoDevice.ReadBits(IAddress addr, int N, ReadCallback<int, bool> callback, object arg)
        {
            if (_ioDev == null)
                return 0;
            return _ioDev.ReadBits(addr, N, callback, arg);
        }
        
        uint IoDevice.ReadReg(IAddress addr)
        {
            if (_ioDev == null)
                return 0;
            return _ioDev.ReadReg(addr);
        }
        int IoDevice.ReadRegs(IAddress addr, int N, ReadCallback<int, uint> callback, object arg)
        {
            if (_ioDev == null)
                return 0;
            return _ioDev.ReadRegs(addr, N, callback, arg);
        }
        int IoDevice.ReadRegs(IEnumerable<IAddress> mixAddrs, ReadCallback<IAddress, uint> callback, object arg)
        {
            if (_ioDev == null)
                return 0;
            return _ioDev.ReadRegs(mixAddrs, callback, arg);
        }

        /// <summary>
        /// 建立 AutoScan
        /// </summary>
        /// <remarks>
        /// 參數可以為 empty 或 IEnumerable ( IoPoint )
        /// </remarks>
        IAutoScan IoDevice.InstanceAutoScan(params object[] args)
        {
            // 只能建構一次
            if (_autoScanner == null)
            {
                var ioMem = this.GetHcfaIoMemory();

                // 取得第1個 arg
                var arg = args.Length > 0 ? args[0] : null;
                if (!(arg is IEnumerable<IoPoint> ioPoints))
                {
                    // 如果 arg 是 null 或其他型別;
                    // 則使用 HcfaIoMemory 內部所有已經存在的 IoPoints.
                    ioPoints = new List<IoPoint>(ioMem.IterPoints());
                }

                // 取得第2個 arg
                int gap = 0;
                if (args.Length > 1 && args[1] is int n)
                    gap = n;

                var builder = new EzModbusScanBanksBuilder();
                var scanBanks = builder.BuildBanks(ioMem, ioPoints, gap: gap);
                _autoScanner = new EzModbusAutoScanner(this, scanBanks);
                _TRACE(scanBanks);

                #region RANDOM_SIM
                if (_settings.IsSim && _settings.UsingRandomSim)
                {
                    var sim = new IoRandomSimulator(ioPoints);
                    _sim = sim;
                    sim.Start();
                }
                #endregion
            }
            return _autoScanner;
        }

        #endregion

        #region DEBUG_AND_TRACE
        void _TRACE(IEnumerable<IoMemoryBank> banks)
        {
            var strs = Array.ConvertAll(banks.ToArray(), b => b?.ToString());
            var msg = "AutoScan Banks:\n" + string.Join("\n", strs);
            System.Diagnostics.Debug.WriteLine(msg);
            _LOG.Trace(msg);
        }
        #endregion
    }
}
