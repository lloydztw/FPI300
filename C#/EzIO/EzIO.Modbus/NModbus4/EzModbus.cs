#region AUTHOR
/*
 * EzIO.Modbus (NModbus4)
 * 
 * Copyright (C) 2026
 * 
 * 2026-04-21 created by LeTian Chang
 * 
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzComm;
using EzIO.Device;
using EzIO.Mem;
using Modbus.Device; // NModbus4 命名空間
using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace EzIO.Modbus.NModbus4
{
    public class EzModbus : IoDevice
    {
        public event EventHandler OnFinalDisposing;
        public event EventHandler<EzModbusErrEventArgs> OnError;

        #region NLOG
        NLog.Logger _LOG => EzLog.LOG;
        #endregion

        #region PRIVATE_KERNEL_DATA
        private TcpClient _tcpClient = null;
        private ModbusIpMaster _modbusMaster = null;
        private EzTcpIpSettings _tcpipSettings;
        private byte _stationID = 1;
        #endregion

        #region DATA_LINKS
        private string _ip => _tcpipSettings.IP;
        private int _port => _tcpipSettings.Port;
        private int _timeoutMs => _tcpipSettings.Timeout;
        private int _retryDelayMs => _tcpipSettings.RetryDelay;
        private int _maxRetryCount => _tcpipSettings.MaxRetryCount;
        #endregion

        #region PRIVATE_RUNTIME_DATA
        string _lastErrorMsg = null;
        bool _isOpen;
        #endregion

        public EzModbus(int stationID, string ip, int port, int timeout = 1000, int retryDelay = 10, int maxRetryCount = 3)
                : this(stationID, new EzTcpIpSettings(ip, port, timeout, retryDelay, maxRetryCount))
        {
        }
        public EzModbus(int stationID, EzTcpIpSettings settings)
        {
            settings.NormalizeByDefaultValues(502, 1000, 10, 3);
            _tcpipSettings = settings;
            _stationID = (byte)stationID;
        }

        public void Dispose()
        {
            OnFinalDisposing?.Invoke(this, null);
            Close();
        }
        public bool Open()
        {
            if (_isOpen && _modbusMaster != null)
                return true;

            try
            {
                _tcpClient = new TcpClient();
                // NModbus4 本身不處理連線，需透過 TcpClient
                var result = _tcpClient.BeginConnect(_ip, _port, null, null);
                var success = result.AsyncWaitHandle.WaitOne(_timeoutMs);

                if (!success || !_tcpClient.Connected)
                {
                    throw new Exception("TCP 連線超時或失敗");
                }

                _tcpClient.EndConnect(result);
                _tcpClient.ReceiveTimeout = _timeoutMs;
                _tcpClient.SendTimeout = _timeoutMs;

                // 建立 NModbus4 Master
                _modbusMaster = ModbusIpMaster.CreateIp(_tcpClient);

                _isOpen = true;
                _lastErrorMsg = null;
                return true;
            }
            catch (Exception ex)
            {
                Close();
                _HANDLE_ERROR("Open", EzModbusError.Device_IP_Open_Failed, tag: ex, isFatal: true);
                return false;
            }
        }
        public void Close()
        {
            try
            {
                _isOpen = false;
                _modbusMaster?.Dispose();
                _modbusMaster = null;
                _tcpClient?.Close();
                _tcpClient = null;
                _lastErrorMsg = null;
            }
            catch (Exception ex)
            {
                _HANDLE_ERROR("Close", EzModbusError.Device_IP_Close_Failed, tag: ex);
            }
        }

        public bool IsOpen
        {
            get => _isOpen && _tcpClient != null && _tcpClient.Connected;
        }
        public bool IsError
        {
            get => _lastErrorMsg != null;
        }

        public string KeyName
        {
            get => EzTcpIpSettings.GetKeyName("EzModbusN4", _ip, _port, IsSim);
        }
        public bool IsSim
        {
            get => _tcpipSettings == null || _tcpipSettings.IsSim;
        }

        #region IO_IMPLEMENTATION
        byte IoDevice.StationID => _stationID;

        void IoDevice.WriteBit(IAddress addr, bool value)
        {
            if (!_VERIFY_OPEN_BITS("WriteBit", addr, 1)) return;

            _EXECUTE_WITH_RETRY("WriteBit", addr, () => {
                _modbusMaster.WriteSingleCoil(_stationID, (ushort)addr.Address, value);
            });
        }

        void IoDevice.WriteReg(IAddress addr, uint value)
        {
            if (!_VERIFY_OPEN_BITS("WriteReg", addr, 16)) return;

            _EXECUTE_WITH_RETRY("WriteReg", addr, () => {
                if (addr.Bits >= 32)
                {
                    // 寫入 32-bit (兩個 Register)
                    ushort low = (ushort)(value & 0xFFFF);
                    ushort high = (ushort)((value >> 16) & 0xFFFF);
                    _modbusMaster.WriteMultipleRegisters(_stationID, (ushort)addr.Address, new ushort[] { low, high });
                }
                else
                {
                    _modbusMaster.WriteSingleRegister(_stationID, (ushort)addr.Address, (ushort)value);
                }
            });
        }

        void IoDevice.WriteRegs(IAddress addr, uint[] values)
        {
            if (!_VERIFY_OPEN_BITS("WriteRegs", addr, 16)) return;

            _EXECUTE_WITH_RETRY("WriteRegs", addr, () => {
                ushort[] data = Array.ConvertAll(values, x => (ushort)x);
                _modbusMaster.WriteMultipleRegisters(_stationID, (ushort)addr.Address, data);
            });
        }

        bool IoDevice.ReadBit(IAddress addr)
        {
            if (!_VERIFY_OPEN_BITS("ReadBit", addr, 1)) return false;

            bool isDiscrete = addr.GetModbusCategory() == ModbusCateEnum.DISCRETE_INPUT;
            bool finalResult = false;

            _EXECUTE_WITH_RETRY("ReadBit", addr, () => {
                if (isDiscrete)
                    finalResult = _modbusMaster.ReadInputs(_stationID, (ushort)addr.Address, 1)[0];
                else
                    finalResult = _modbusMaster.ReadCoils(_stationID, (ushort)addr.Address, 1)[0];
            });

            return finalResult;
        }

        uint IoDevice.ReadReg(IAddress addr)
        {
            if (!_VERIFY_OPEN_BITS("ReadReg", addr, 16)) return 0;

            uint finalResult = 0;
            _EXECUTE_WITH_RETRY("ReadReg", addr, () => {
                if (addr.Bits >= 32)
                {
                    ushort[] res = _modbusMaster.ReadHoldingRegisters(_stationID, (ushort)addr.Address, 2);
                    finalResult = (uint)(res[0] | (res[1] << 16));
                }
                else
                {
                    ushort[] res = _modbusMaster.ReadHoldingRegisters(_stationID, (ushort)addr.Address, 1);
                    finalResult = res[0];
                }
            });
            return finalResult;
        }

        int IoDevice.ReadBits(IAddress addr, int N, ReadCallback<int, bool> callback, object arg)
        {
            if (!_VERIFY_OPEN_BITS("ReadBits", addr, 1)) return 0;

            bool isDiscrete = addr.GetModbusCategory() == ModbusCateEnum.DISCRETE_INPUT;
            int count = 0;

            _EXECUTE_WITH_RETRY("ReadBits", addr, () => {
                bool[] res = isDiscrete ?
                    _modbusMaster.ReadInputs(_stationID, (ushort)addr.Address, (ushort)N) :
                    _modbusMaster.ReadCoils(_stationID, (ushort)addr.Address, (ushort)N);

                for (int i = 0; i < res.Length; i++)
                {
                    callback?.Invoke(i, res[i], arg);
                }
                count = res.Length;
            });
            return count;
        }

        int IoDevice.ReadRegs(IAddress addr, int N, ReadCallback<int, uint> callback, object arg)
        {
            if (!_VERIFY_OPEN_BITS("ReadRegs", addr, 16)) return 0;

            int count = 0;
            _EXECUTE_WITH_RETRY("ReadRegs", addr, () => {
                ushort[] res = _modbusMaster.ReadHoldingRegisters(_stationID, (ushort)addr.Address, (ushort)N);
                for (int i = 0; i < res.Length; i++)
                {
                    callback?.Invoke(i, res[i], arg);
                }
                count = res.Length;
            });
            return count;
        }

        int IoDevice.ReadRegs(IEnumerable<IAddress> mixAddrs, ReadCallback<IAddress, uint> callback, object arg)
        {
            int count = 0;
            foreach (var addr in mixAddrs)
            {
                uint val = ((IoDevice)this).ReadReg(addr);
                callback?.Invoke(addr, val, arg);
                count++;
            }
            return count;
        }

        IAutoScan IoDevice.InstanceAutoScan(params object[] args)
        {
            return null;
        }

        #endregion

        #region HELPERS
        /// <summary>
        /// 統一處理 NModbus4 的重試機制與異常封裝
        /// </summary>
        void _EXECUTE_WITH_RETRY(string funcName, IAddress addr, Action action)
        {
            for (int i = 0; i <= _maxRetryCount; i++)
            {
                try
                {
                    action();
                    _lastErrorMsg = null;
                    return;
                }
                catch (Exception ex)
                {
                    if (i < _maxRetryCount)
                    {
                        _LOG.Warn("{0}.{1}({2}) Retry={3} Error={4}", KeyName, funcName, addr, i + 1, ex.Message);
                        System.Threading.Thread.Sleep(_retryDelayMs);
                    }
                    else
                    {
                        _HANDLE_ERROR(funcName, EzModbusError.CmdRetry_Overflow, addr, tag: ex.Message, isFatal: true);
                    }
                }
            }
        }
        bool _VERIFY_OPEN_BITS(string funcName, IAddress addr, int bits)
        {
            if (!IsOpen)
            {
                _HANDLE_ERROR(funcName, EzModbusError.Device_NotOpen, addr);
                return false;
            }
            if (bits == 1 && addr.Bits != 1)
            {
                _HANDLE_ERROR(funcName, EzModbusError.Address_Bits_Err, addr, tag: "Address.Bits must be 1!");
                return false;
            }
            else if (bits > 1 && addr.Bits < bits)
            {
                _HANDLE_ERROR(funcName, EzModbusError.Address_Bits_Err, addr, tag: $"Address.Bits must >= {bits}!");
                return false;
            }
            return true;
        }
        void _HANDLE_ERROR(string funcName, EzModbusError err, IAddress addr = null, object tag = null, bool isFatal = false)
        {
            var errMsg = _lastErrorMsg = err.ToString();
            EzModbusException exx;

            if (tag is Exception ex)
                exx = new EzModbusException(err, ex, isFatal);
            else
                exx = new EzModbusException(err, null, isFatal, tag?.ToString());

            _LOG.Error("{0}.{1}({2}) 異常 = {3} | {4}", KeyName, funcName, addr, err, tag);
            OnError?.Invoke(this, new EzModbusErrEventArgs(exx));
            throw exx;
        }
        #endregion
    }
}
