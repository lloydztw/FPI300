#region AUTHOR
/*
 * EzIO.Modbus (HSL)
 * 
 * Copyright (C) 2023
 * 
 * 2026-04-21 Integration with EzIO by LeTian Chang
 * 2013-07-11 created by LeTian Chang
 * 
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

#if (OPT_USING_HSL)

using EzComm;
using EzIO.Device;
using EzIO.Mem;
using HslCommunication;
using HslCommunication.ModBus;
using System;
using System.Collections.Generic;

namespace EzIO.Modbus.HSL
{
    public class EzModbus : IoDevice
    {
        public event EventHandler OnFinalDisposed;
        public event EventHandler<EzModbusErrEventArgs> OnError;

        #region NLOG
        private static NLog.Logger s_nlog = null;
        static NLog.Logger _LOG
        {
            get
            {
                if (s_nlog == null)
                {
                    s_nlog = NLog.LogManager.GetCurrentClassLogger();
                }
                return s_nlog;
            }
        }
        #endregion

        #region PRIVATE_KERNEL_DATA
        private ModbusTcpNet _modbusTcpClient = null;
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

        public EzModbus(int stationID, string ip, int port = 502, int timeout = 1000, int retryDelay = 10, int maxRetryCount = 3)
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
            if (_modbusTcpClient != null)
            {
                Close();
                _modbusTcpClient.Dispose();
                _modbusTcpClient = null;
                OnFinalDisposed?.Invoke(this, null);
            }
        }

        public bool Open()
        {
            if (_isOpen && _modbusTcpClient != null)
                return true;

            try
            {
                _modbusTcpClient = new ModbusTcpNet(_ip, _port, _stationID);

                _modbusTcpClient.ByteTransform.DataFormat = HslCommunication.Core.DataFormat.ABCD;
                //m_modbusTcpClient.SA1 = 0;
                _modbusTcpClient.AddressStartWithZero = true;
                _modbusTcpClient.ConnectTimeOut = _timeoutMs;
                _modbusTcpClient.ReceiveTimeOut = _timeoutMs;

                OperateResult connect = _modbusTcpClient.ConnectServer();
                _isOpen = connect.IsSuccess;

                if (!connect.IsSuccess)
                {
                    //>>> _HANDLE_ERROR("Open", "ConnectServer 連線失敗!");
                    throw new Exception("ConnectServer 連線失敗!");
                }

                _lastErrorMsg = null;
                return true;
            }
            catch (Exception ex)
            {
                _HANDLE_ERROR("Open", EzModbusError.Device_IP_Open_Failed, tag: ex, isFatal: true);
                return false;
            }
        }
        public void Close()
        {
            try
            {
                if (_isOpen && _modbusTcpClient != null)
                {
                    _modbusTcpClient.ConnectClose();
                    _isOpen = false;
                    _lastErrorMsg = null;
                }
            }
            catch (Exception ex)
            {
                _HANDLE_ERROR("Close", EzModbusError.Device_IP_Close_Failed, tag: ex);
            }
        }
        public bool IsOpen
        {
            get { return _isOpen && _modbusTcpClient != null; }
        }
        public bool IsError
        {
            get { return _lastErrorMsg != null; }
        }

        public string KeyName
        {
            get => EzTcpIpSettings.GetKeyName("EzModbus", _ip, _port, IsSim);
        }
        public bool IsSim
        {
            get => _tcpipSettings == null || _tcpipSettings.IsSim;
        }

        #region IO_IMPLEMENTATION
        byte IoDevice.StationID => _stationID;

        void IoDevice.WriteBit(IAddress addr, bool value)
        {
            //----------------------------------------------------------------------------------------------------------
            // 寫入 1 - bit 的位址限制
            //----------------------------------------------------------------------------------------------------------
            // 在 Modbus 中，並非所有位址都支援 WriteBit：
            //
            //  0x(Coils)：
            //      支援 WriteBit 與 ReadBit。
            //
            //  1x(Discrete Inputs)：
            //      不支援 WriteBit.
            //      僅支援 ReadBit，調用 WriteBit 會收到設備回傳的錯誤碼。
            //
            //  3x(Registers) 與 4x(Registers)：
            //      不支援 WriteBit.
            //      這是 Word 位址。如果你想寫入 40001.3 (第 1 個暫存器的第 3 個 bit)，不能直接調用 Write(address, bool)。
            //
            //----------------------------------------------------------------------------------------------------------

            if (!_VERIFY_OPEN_BITS("WriteBit", addr, 1)) 
                return;

            for (int i = 0; i <= _maxRetryCount; i++)
            {
                OperateResult result = _modbusTcpClient.Write(addr.ToCommString(), value);
                if (result.IsSuccess) 
                    return;

                _HANDLE_RETRY("WriteBit", addr, i, result.Message);
            }
        }

        void IoDevice.WriteReg(IAddress addr, uint value)
        {
            if (!_VERIFY_OPEN_BITS("WriteReg", addr, 16)) 
                return;

            for (int i = 0; i <= _maxRetryCount; i++)
            {
                OperateResult result;

                if (addr.Bits >= 32)
                    result = _modbusTcpClient.Write(addr.ToCommString(), (int)value); // 寫入 32-bit
                else
                    result = _modbusTcpClient.Write(addr.ToCommString(), (short)value); // 寫入 16-bit

                if (result.IsSuccess) 
                    return;

                _HANDLE_RETRY("WriteReg", addr, i, result.Message);
            }
        }

        void IoDevice.WriteRegs(IAddress addr, uint[] values)
        {
            if (!_VERIFY_OPEN_BITS("WriteRegs", addr, 16))
                return;

            for (int i = 0; i <= _maxRetryCount; i++)
            {
                // Modbus 批量寫入暫存器通常使用 short[]
                short[] data = Array.ConvertAll(values, x => (short)x);
                OperateResult result = _modbusTcpClient.Write(addr.ToCommString(), data);

                if (result.IsSuccess)
                    return;

                _HANDLE_RETRY("WriteRegs", addr, i, result.Message);
            }
        }

        bool IoDevice.ReadBit(IAddress addr)
        {
            if (!_VERIFY_OPEN_BITS("ReadBit", addr, 1))
                return false;

            bool isDiscrete = addr.GetModbusCategory() == ModbusCateEnum.DISCRETE_INPUT;
            string modbusAddr = addr.ToCommString();

            for (int i = 0; i <= _maxRetryCount; i++)
            {
                OperateResult<bool> result = isDiscrete ?
                    _modbusTcpClient.ReadDiscrete(modbusAddr):
                    _modbusTcpClient.ReadBool(modbusAddr);

                if (result.IsSuccess)
                    return result.Content;

                _HANDLE_RETRY("ReadBit", addr, i, result.Message);
            }

            return false;
        }

        uint IoDevice.ReadReg(IAddress addr)
        {
            if (!_VERIFY_OPEN_BITS("ReadReg", addr, 16)) 
                return 0;

            for (int i = 0; i <= _maxRetryCount; i++)
            {
                if (addr.Bits >= 32)
                {
                    OperateResult<int> result = _modbusTcpClient.ReadInt32(addr.ToCommString());
                    if (result.IsSuccess)
                        return (uint)result.Content;

                    _HANDLE_RETRY("ReadReg(32)", addr, i, result.Message);
                }
                else
                {
                    OperateResult<short> result = _modbusTcpClient.ReadInt16(addr.ToCommString());
                    if (result.IsSuccess)
                        return (uint)(ushort)result.Content;

                    _HANDLE_RETRY("ReadReg(16)", addr, i, result.Message);
                }
            }
            return 0;
        }

        int IoDevice.ReadBits(IAddress addr, int N, ReadCallback<int, bool> callback, object arg)
        {
            if (!_VERIFY_OPEN_BITS("ReadBits", addr, 1))
                return 0;

            bool isDiscrete = addr.GetModbusCategory() == ModbusCateEnum.DISCRETE_INPUT;
            string modbusAddr = addr.ToCommString();

            for (int i = 0; i <= _maxRetryCount; i++)
            {
                OperateResult<bool[]> result = isDiscrete ?
                    _modbusTcpClient.ReadDiscrete(modbusAddr, (ushort)N) :
                    _modbusTcpClient.ReadBool(modbusAddr, (ushort)N);

                if (result.IsSuccess)
                {
                    for (int n = 0; n < N; n++) callback?.Invoke(n, result.Content[n], arg);
                    return N;
                }

                _HANDLE_RETRY("ReadBits", addr, i, result.Message);
            }

            return 0;
        }

        int IoDevice.ReadRegs(IAddress addr, int N, ReadCallback<int, uint> callback, object arg)
        {
            if (!_VERIFY_OPEN_BITS("ReadRegs", addr, 16))
                return 0;

            for (int i = 0; i <= _maxRetryCount; i++)
            {
                OperateResult<short[]> result = _modbusTcpClient.ReadInt16(addr.ToCommString(), (ushort)N);
                if (result.IsSuccess)
                {
                    for (int n = 0; n < N; n++) callback?.Invoke(n, (uint)(ushort)result.Content[n], arg);
                    return N;
                }

                _HANDLE_RETRY("ReadRegs", addr, i, result.Message);
            }
            return 0;
        }

        int IoDevice.ReadRegs(IEnumerable<IAddress> mixAddrs, ReadCallback<IAddress, uint> callback, object arg)
        {
            // 混合讀取在 Modbus TCP 標準中較難實現單一封包優化
            // 這裡採用逐點讀取並透過 callback 返回
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

        #region EXCEPTION_HANDLERS
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
        void _HANDLE_RETRY(string funcName, IAddress addr, int retryCount, string extraMsg = null)
        {
            if (retryCount < _maxRetryCount)
            {
                // Log and Delay
                _LOG.Warn("{0}.{1}({2}) Retry={3}", KeyName, funcName, addr, retryCount + 1);
                System.Threading.Thread.Sleep(_retryDelayMs);
            }
            else
            {
                _HANDLE_ERROR(funcName, EzModbusError.CmdRetry_Overflow, addr, tag: extraMsg, isFatal: true);
            }
        }
        void _HANDLE_ERROR(string funcName, EzModbusError err, IAddress addr = null, object tag = null, bool isFatal = false)
        {
            if (tag is EzModbusException exx)
            {
                throw exx;
            }
            else
            {
                var errMsg = _lastErrorMsg = err.ToString();

                if (tag is Exception ex)
                {
                    exx = new EzModbusException(err, ex, isFatal);
                    _LOG.Error(ex, "{0}.{1}({2}) 異常 = {3}", KeyName, funcName, addr, err);
                    OnError?.Invoke(this, new EzModbusErrEventArgs(exx));
                }
                else
                {
                    exx = new EzModbusException(err, null, isFatal, tag?.ToString());
                    _LOG.Error("{0}.{1}({2}) 異常 = {3}", KeyName, funcName, addr, err);
                    OnError?.Invoke(this, new EzModbusErrEventArgs(exx));
                }

                throw exx;
            }
        }
        #endregion
    }
}

#endif
