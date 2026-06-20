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
using System.IO.Ports;

namespace EzIO.Modbus.NModbus4
{
    public class EzModbusRTU : IoDevice
    {
        public event EventHandler OnFinalDisposing;
        public event EventHandler<EzModbusErrEventArgs> OnError;

        #region NLOG
        NLog.Logger _LOG => EzLog.LOG;
        #endregion

        #region PRIVATE_KERNEL_DATA
        private SerialPort _serialPort = null;
        private IModbusSerialMaster _modbusMaster = null;
        private EzUartSettings _uartSettings;
        private byte _stationID = 1;
        #endregion

        #region DATA_LINKS
        private string _portName => $"COM{_uartSettings.ComPort}";
        private int _timeoutMs => _uartSettings.Timeout;
        private int _maxRetryCount => _uartSettings.MaxRetryCount;
        #endregion

        #region PRIVATE_RUNTIME_DATA
        string _lastErrorMsg = null;
        bool _isOpen;
        #endregion

        public EzModbusRTU(int stationID, EzUartSettings settings)
        {
            _uartSettings = settings;
            _stationID = (byte)stationID;
        }

        public void Dispose()
        {
            OnFinalDisposing?.Invoke(this, null);
            Close();
        }
        public bool Open()
        {
            if (_isOpen && _serialPort != null && _serialPort.IsOpen)
                return true;

            try
            {
                // 初始化 SerialPort
                _serialPort = new SerialPort(
                    _portName,
                    _uartSettings.BaudRate,
                    _uartSettings.Parity,
                    _uartSettings.DataBits,
                    _uartSettings.StopBits);

                _serialPort.ReadTimeout = _timeoutMs;
                _serialPort.WriteTimeout = _timeoutMs;

                _serialPort.Open();

                // 建立 NModbus4 RTU Master
                _modbusMaster = ModbusSerialMaster.CreateRtu(_serialPort);

                _isOpen = true;
                _lastErrorMsg = null;
                return true;
            }
            catch (Exception ex)
            {
                Close();
                _HANDLE_ERROR("Open", EzModbusError.Device_ComPort_Open_Failed, tag: ex, isFatal: true);
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

                if (_serialPort != null)
                {
                    if (_serialPort.IsOpen) _serialPort.Close();
                    _serialPort.Dispose();
                }
                _serialPort = null;
                _lastErrorMsg = null;
            }
            catch (Exception ex)
            {
                _HANDLE_ERROR("Close", EzModbusError.Device_ComPort_Close_Failed, tag: ex);
            }
        }

        public bool IsOpen
        {
            get => _isOpen && _serialPort != null && _serialPort.IsOpen;
        }
        public bool IsError
        {
            get => _lastErrorMsg != null;
        }

        public string KeyName
        {
            get => EzUartSettings.GetKeyName("EzModbusRTU", _uartSettings.ComPort, IsSim);
        }
        public bool IsSim
        {
            get => _uartSettings == null || _uartSettings.IsSim;
        }

        #region IO_IMPLEMENTATION (與 TCP 版本邏輯相同，但調用同一個 Master 介面)
        byte IoDevice.StationID => _stationID;

        void IoDevice.WriteBit(IAddress addr, bool value)
        {
            if (!_VERIFY_OPEN_BITS("WriteBit", addr, 1)) return;
            _EXECUTE_WITH_RETRY("WriteBit", addr, () => _modbusMaster.WriteSingleCoil(_stationID, (ushort)addr.Address, value));
        }

        void IoDevice.WriteReg(IAddress addr, uint value)
        {
            if (!_VERIFY_OPEN_BITS("WriteReg", addr, 16)) return;
            _EXECUTE_WITH_RETRY("WriteReg", addr, () => {
                if (addr.Bits >= 32)
                {
                    ushort low = (ushort)(value & 0xFFFF);
                    ushort high = (ushort)((value >> 16) & 0xFFFF);
                    _modbusMaster.WriteMultipleRegisters(_stationID, (ushort)addr.Address, new ushort[] { low, high });
                }
                else
                    _modbusMaster.WriteSingleRegister(_stationID, (ushort)addr.Address, (ushort)value);
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
            bool result = false;
            bool isDiscrete = addr.GetModbusCategory() == ModbusCateEnum.DISCRETE_INPUT;
            _EXECUTE_WITH_RETRY("ReadBit", addr, () => {
                result = isDiscrete ?
                    _modbusMaster.ReadInputs(_stationID, (ushort)addr.Address, 1)[0] :
                    _modbusMaster.ReadCoils(_stationID, (ushort)addr.Address, 1)[0];
            });
            return result;
        }

        uint IoDevice.ReadReg(IAddress addr)
        {
            if (!_VERIFY_OPEN_BITS("ReadReg", addr, 16)) return 0;
            uint result = 0;
            _EXECUTE_WITH_RETRY("ReadReg", addr, () => {
                if (addr.Bits >= 32)
                {
                    ushort[] res = _modbusMaster.ReadHoldingRegisters(_stationID, (ushort)addr.Address, 2);
                    result = (uint)(res[0] | (res[1] << 16));
                }
                else
                {
                    result = _modbusMaster.ReadHoldingRegisters(_stationID, (ushort)addr.Address, 1)[0];
                }
            });
            return result;
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
                for (int i = 0; i < res.Length; i++) callback?.Invoke(i, res[i], arg);
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
                for (int i = 0; i < res.Length; i++) callback?.Invoke(i, res[i], arg);
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

        IAutoScan IoDevice.InstanceAutoScan(params object[] args) => null;
        #endregion

        #region HELPERS (與 TCP 版本共用邏輯)
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
                        System.Threading.Thread.Sleep(_uartSettings.Timeout / 2); // 稍微等待
                    }
                    else
                        _HANDLE_ERROR(funcName, EzModbusError.CmdRetry_Overflow, addr, tag: ex.Message, isFatal: true);
                }
            }
        }

        bool _VERIFY_OPEN_BITS(string funcName, IAddress addr, int bits)
        {
            if (!IsOpen) { _HANDLE_ERROR(funcName, EzModbusError.Device_NotOpen, addr); return false; }
            return true;
        }

        void _HANDLE_ERROR(string funcName, EzModbusError err, IAddress addr = null, object tag = null, bool isFatal = false)
        {
            _lastErrorMsg = err.ToString();
            EzModbusException exx = (tag is Exception ex) ? new EzModbusException(err, ex, isFatal) : new EzModbusException(err, null, isFatal, tag?.ToString());
            _LOG.Error("{0}.{1}({2}) 異常 = {3} | {4}", KeyName, funcName, addr, err, tag);
            OnError?.Invoke(this, new EzModbusErrEventArgs(exx));
            throw exx;
        }
        #endregion
    }
}
