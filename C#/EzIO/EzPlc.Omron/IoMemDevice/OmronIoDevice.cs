#region AUTHOR
/*
 * EzPlc.Omron
 * 
 * Copyright (C) 2026
 * 
 * 2026-04-24 LeTian Chang : Integration with EzIO
 * 
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzComm;
using EzIO.Device;
using EzIO.Mem;
using System;
using System.Collections.Generic;
using System.Linq;
using IAddress = EzIO.Mem.IAddress;
using NJCompolet = OMRON.Compolet.CIPCompolet64.NJCompolet;

namespace EzPlc.Omron
{
    /// <summary>
    /// Omron 對 IoDevice 之實作 
    /// </summary>
    public class OmronIoDevice : IoDevice
    {
        public event EventHandler OnFinalDisposing;
        public event EventHandler OnError;

        #region NLOG
        NLog.Logger _LOG => EzLog.LOG;
        #endregion

        #region PRIVATE_DATA
        EzTcpIpSettings _settings;
        NJCompolet _comm;
        byte _stationID;
        #endregion

        #region AUTO_SCANNER
        IAutoScan _autoScanner;
        IDisposable _sim;
        #endregion

        public OmronIoDevice(EzTcpIpSettings settings, int stationID = 0)
        {
            _stationID = (byte)stationID;
            _settings = settings;

            if (!_settings.IsSim)
            {
                try
                {
                    openCIP();
                    System.Diagnostics.Debug.Assert(_comm != null);
                }
                catch (Exception ex)
                {
                    _LOG.Error($"Omron NJ連線失敗 ({_settings.IP}): {ex.Message}");
                    throw;
                }
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
            catch (Exception ex)
            {
                _LOG.Error(ex);
            }
            #endregion

            OnFinalDisposing?.Invoke(this, null);

            closeCIP();
        }

        #region PRIVATE_FUNCTIONS
        void openCIP()
        {
            if (_comm != null)
                return;

            var compolet = new NJCompolet();
            compolet.Active = false;
            compolet.ConnectionType = OMRON.Compolet.CIPCompolet64.ConnectionType.Class3;
            compolet.DontFragment = false;
            compolet.LocalPort = 2;
            compolet.PeerAddress = _settings.IP;              // "192.168.250.1";
            compolet.ReceiveTimeLimit = _settings.Timeout;    // ((long)(750));

            compolet.RoutePath = "2%192.168.250.1\\1%0";
            compolet.UseRoutePath = false;

            compolet.Active = true;

            // 開啟通訊
            if (!compolet.IsConnected)
                compolet.Active = false;

            _comm = compolet;
        }
        void closeCIP()
        {
            try
            {
                if (_comm != null)
                {
                    _comm.Active = false;
                    _comm.Dispose();
                    _comm = null;
                }
            }
            catch (Exception ex)
            {
                _LOG.Error(ex, "closeCIP() Failed!");
            }
        }
        #endregion

        public bool IsSim
        {
            get => _settings == null || _settings.IsSim;
        }
        public string KeyName
        {
            get => EzTcpIpSettings.GetKeyName("Omron", _settings.IP, _settings.Port, _settings.IsSim);
        }
        public override string ToString()
        {
            return KeyName;
        }

        #region OMRON_SPECIFIC_FUNCTIONS
        internal NJCompolet Compolet
        {
            get => _comm;
        }
        public object ReadVariable(string varName)
        {
            return _comm?.ReadVariable(varName);
        }
        public void WriteVariable(string varName, object value)
        {
            _comm?.WriteVariable(varName, value);
        }
        #endregion

        #region IO_DEVICE_IMPLEMENTAION
        byte IoDevice.StationID
        {
            get => _stationID;
        }

        // 寫入 BOOL
        void IoDevice.WriteBit(IAddress addr, bool value)
        {
            if (((IoDevice)this).IsSim || _comm == null)
                return;

            _comm.WriteVariable(addr.ToCommString(), value);
        }

        // 寫入單一數值 (INT/DINT/UINT...)
        void IoDevice.WriteReg(IAddress addr, uint value)
        {
            if (((IoDevice)this).IsSim || _comm == null)
                return;

            _comm.WriteVariable(addr.ToCommString(), value);
        }

        // 寫入陣列
        void IoDevice.WriteRegs(IAddress addr, uint[] values)
        {
            //if (((IoDevice)this).IsSim)
            //    return;
            //_comm.WriteVariable(addr.ToCommString(), values);
            throw new NotSupportedException("CIP 目前似乎沒有對應的函式可用!");
        }

        // 讀取單一 BOOL
        bool IoDevice.ReadBit(IAddress addr)
        {
            if (((IoDevice)this).IsSim || _comm == null)
                return false;

            var data = _comm.ReadVariable(addr.ToCommString());
            return Convert.ToBoolean(data);
        }

        // 讀取位元陣列
        int IoDevice.ReadBits(IAddress addr, int N, ReadCallback<int, bool> callback, object arg)
        {
            //if (((IoDevice)this).IsSim || callback == null)
            //    return 0;

            //object result = _comm.ReadVariable(addr.ToCommString());
            //if (result is bool[] bools)
            //{
            //    int len = Math.Min(N, bools.Length);
            //    for (int i = 0; i < len; i++)
            //        callback?.Invoke(i, bools[i], arg);
            //    return len;
            //}

            //return 0;

            throw new NotSupportedException("CIP 目前似乎沒有對應的函式可用!");
        }

        // 讀取單一暫存器
        uint IoDevice.ReadReg(IAddress addr)
        {
            if (((IoDevice)this).IsSim || _comm == null)
                return 0;

            var data = _comm.ReadVariable(addr.ToCommString());
            return Convert.ToUInt32(data);
        }

        // 讀取暫存器陣列
        int IoDevice.ReadRegs(IAddress addr, int N, ReadCallback<int, uint> callback, object arg)
        {
            //if (((IoDevice)this).IsSim || callback == null)
            //    return 0;

            //object result = _comm.ReadVariable(addr.ToCommString());
            //if (result is Array array)
            //{
            //    int len = Math.Min(N, array.Length);
            //    for (int i = 0; i < len; i++)
            //        callback(i, Convert.ToUInt32(array.GetValue(i)), arg);
            //    return len;
            //}

            //return 0;

            throw new NotSupportedException("CIP 目前似乎沒有對應的函式可用!");
        }

        // 混合標籤批次讀取 (NJCompolet 支援一次讀取多個變數名稱)
        int IoDevice.ReadRegs(IEnumerable<IAddress> mixAddrs, ReadCallback<IAddress, uint> callback, object arg)
        {
            //if (((IoDevice)this).IsSim || callback == null)
            //    return 0;

            //var addrs = mixAddrs.ToArray();
            //var names = Array.ConvertAll(addrs, a => a.ToCommString());

            //// 使用 NJCompolet 的批次讀取功能
            //var results = _comm.ReadVariableMultiple(names);
            //int i = 0;
            //foreach(var result in results)
            //{
            //    uint value = Convert.ToUInt32(result);
            //    callback.Invoke(addrs[i], value, arg);
            //    i++;
            //}
            //return i;

            throw new NotSupportedException("CIP 目前似乎沒有對應的函式可用!");
        }

        /// <summary>
        /// 建立 AutoScan。由於 Omron 是 Tag 系統，這裡的 Bank 實作需與 Modbus 不同。
        /// </summary>
        IAutoScan IoDevice.InstanceAutoScan(params object[] args)
        {
            if (args.Length > 0 && args[0] is IEnumerable<IoPoint> points)
            {
                _autoScanner = new EzIO.Omron.AutoScan.EzOmronAutoScanner(this, points);
                return _autoScanner;
            }
            return null;
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
