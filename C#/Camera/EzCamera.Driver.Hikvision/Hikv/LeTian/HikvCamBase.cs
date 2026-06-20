#region AUTHOR
/*
 * 
 * Copyright (c) 2024 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2024-05-05 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzCamera.Interface;
using MvCamCtrl.NET;
using System;
using System.Drawing;


namespace EzCamera.Driver.Hikvision
{
    /// <summary>
    /// 提供 海康 包裝後的 基本函式 內部自帶 error message 處理.
    /// <br/> 具體的 控制流程, Callbacks, 與 FrameBuf 管理, 由繼承者實作之.
    /// </summary>
    public class HikvCamBase
    {
        public event EventHandler<EzLiveImageEventArgs> OnFrameCaptured;
        public event EventHandler<HikvError> OnError;

        #region PROTECTED_HIKV_核心成員
        protected MyCamera _myCamera;
        protected MyCamera.MV_CC_DEVICE_INFO _mvDeviceInfo;
        #endregion

        //--- ERROR Handle Fuctions ---------------------------------------------------------------------------------------
        protected void _ERR(int errCode, string errMsg, string description = null)
        {
            OnError?.Invoke(this, new HikvError(errCode, errMsg, description));
        }

        #region EVENT_FIRERS
        /// 發送 OnFrameCaptured 事件
        /// <br/> (1) 發送者 (Sender) 負責 img 的生命週期.
        /// <br/> (2) 接受者 (EventHandler) 不要對 img 調用 Dispose().
        protected void fireFrameCapturedEvent(EzLiveImageEventArgs e)
        {
            OnFrameCaptured?.Invoke(this, e);
        }
        /// 發送 OnFrameCaptured 事件
        /// <br/> (1) 發送者 (Sender) 負責 img 的生命週期.
        /// <br/> (2) 接受者 (EventHandler) 不要對 img 調用 Dispose().
        protected void fireFrameCapturedEvent(Bitmap img)
        {
            OnFrameCaptured?.Invoke(this, new EzLiveImageEventArgs(img));
        }
        #endregion

        //--- HikVision Basic Functions ------------------------------------------------------------------------------------
        #region HikVision_Basic_Get_Set_Functions
        public int setCommand(string command)
        {
            int ret = _myCamera.MV_CC_SetCommandValue_NET(command);
            if (MyCamera.MV_OK != ret)
                _ERR(ret, $"錯誤於 設定命令 {command}");
            return ret;
        }
        public int setEnumValue(string propertyName, uint value)
        {
            int ret = _myCamera.MV_CC_SetEnumValue_NET(propertyName, value);
            if (ret != MyCamera.MV_OK)
                _ERR(ret, $"錯誤於 Set {propertyName} ({value})");
            return ret;
        }
        public int getEnumValue(string propertyName, out uint value)
        {
            var mvccValue = new MyCamera.MVCC_ENUMVALUE();
            int ret = _myCamera.MV_CC_GetEnumValue_NET(propertyName, ref mvccValue);
            if (ret != MyCamera.MV_OK)
            {
                _ERR(ret, $"錯誤於 Get {propertyName}");
                value = 0;
            }
            else
            {
                value = mvccValue.nCurValue;
            }
            return ret;
        }

        public int setUnit(string propertyName, uint value)
        {
            int ret = _myCamera.MV_CC_SetIntValue_NET(propertyName, value);
            if (ret != MyCamera.MV_OK)
                _ERR(ret, $"錯誤於 Set {propertyName} ({value})");
            return ret;
        }
        public int getUint(string propertyName, out uint value)
        {
            var mvccValue = new MyCamera.MVCC_INTVALUE();
            int ret = getUint(propertyName, ref mvccValue);
            value = (ret == MyCamera.MV_OK) ? mvccValue.nCurValue : 0;
            return ret;
        }
        public int getUint(string propertyName, ref MyCamera.MVCC_INTVALUE mvccValue)
        {
            int ret = _myCamera.MV_CC_GetIntValue_NET(propertyName, ref mvccValue);
            if (MyCamera.MV_OK != ret)
                _ERR(ret, $"錯誤於 Get {propertyName}");
            return ret;
        }

        public int setFloat(string propertyName, float value)
        {
            int ret = _myCamera.MV_CC_SetFloatValue_NET(propertyName, value);
            if (ret != MyCamera.MV_OK)
                _ERR(ret, $"錯誤於 Set {propertyName} ({value})");
            return ret;
        }
        public int getFloat(string propertyName, out float value)
        {
            var mvccValue = new MyCamera.MVCC_FLOATVALUE();
            int ret = getFloat(propertyName, ref mvccValue);
            value = (ret == MyCamera.MV_OK) ? mvccValue.fCurValue : 0f;
            return ret;
        }
        public int getFloat(string propertyName, ref MyCamera.MVCC_FLOATVALUE mvccValue)
        {
            int ret = _myCamera.MV_CC_GetFloatValue_NET(propertyName, ref mvccValue);
            if (MyCamera.MV_OK != ret)
                _ERR(ret, $"錯誤於 Get {propertyName}");
            return ret;
        }
        #endregion

        //--- Camera Image Info ------------------------------------------------------------------------------------------
        #region CAMERA_IMAGE_INFO
        public int getPixelFormat(out uint fmt)
        {
            int ret = getEnumValue("PixelFormat", out fmt);
            return ret;
        }
        public int getWidth(out uint value, out uint min, out uint max)
        {
            var mvccValue = new MyCamera.MVCC_INTVALUE();
            int ret = getUint("Width", ref mvccValue);
            if (MyCamera.MV_OK != ret)
            {
                value = 0; min = 0; max = 0;
            }
            else
            {
                value = mvccValue.nCurValue;
                min = mvccValue.nMin;
                max = mvccValue.nMax;
            }
            return ret;
        }
        public int setWidth(uint value)
        {
            return setUnit("Width", value);
        }
        public int getHeight(out uint value, out uint min, out uint max)
        {
            var mvccValue = new MyCamera.MVCC_INTVALUE();
            int ret = getUint("Height", ref mvccValue);
            if (MyCamera.MV_OK != ret)
            {
                value = 0; min = 0; max = 0;
            }
            else
            {
                value = mvccValue.nCurValue;
                min = mvccValue.nMin;
                max = mvccValue.nMax;
            }
            return ret;
        }
        public int setHeight(uint value)
        {
            return setUnit("Height", value);
        }
        #endregion

        //--- Camera Properties -------------------------------------------------------------------------------------------
        #region CAMERA_PROPERTIES

        #region PRIVATE_CACHE_DATA
        private float _cacheExpo;
        private float _cacheGain;
        #endregion
        public float ExposureTime
        {
            get => _cacheExpo;
            set => setExposureTime(value);
        }
        public float Gain
        {
            get => _cacheGain;
            set => setGain(value);
        }

        public int getExposureTime(out float value, out float min, out float max)
        {
            var mvccValue = new MyCamera.MVCC_FLOATVALUE();
            int ret = getFloat("ExposureTime", ref mvccValue);
            if (MyCamera.MV_OK != ret)
            {
                value = 0; min = 0; max = 0;
            }
            else
            {
                value = mvccValue.fCurValue;
                min = mvccValue.fMin;
                max = mvccValue.fMax;
                _cacheExpo = value;
            }
            return ret;
        }
        public int setExposureTime(float value)
        {
            int ret = setFloat("ExposureTime", value);
            getFloat("ExposureTime", out _cacheExpo);
            return ret;
        }
        public int getGain(out float value, out float min, out float max)
        {
            var mvccValue = new MyCamera.MVCC_FLOATVALUE();
            int ret = getFloat("Gain", ref mvccValue);
            if (MyCamera.MV_OK != ret)
            {
                value = 0; min = 0; max = 0;
            }
            else
            {
                value = mvccValue.fCurValue;
                min = mvccValue.fMin;
                max = mvccValue.fMax;
                _cacheGain = value;
            }
            return ret;
        }
        public int setGain(float value)
        {
            int ret = setFloat("Gain", value);
            getFloat("Gain", out _cacheGain);
            return ret;
        }
        public int getFPS(out float value, out float min, out float max)
        {
            var mvccValue = new MyCamera.MVCC_FLOATVALUE();
            int ret = getFloat("ResultingFrameRate", ref mvccValue);
            if (MyCamera.MV_OK != ret)
            {
                value = 0; min = 0; max = 0;
            }
            else
            {
                value = mvccValue.fCurValue;
                min = mvccValue.fMin;
                max = mvccValue.fMax;
            }
            return ret;
        }
        public int setFPS(float value)
        {
            return setFloat("AcquisitionFrameRate", value);
        }

        /// <summary>
        /// 關掉自動白平衡
        /// </summary>
        public int disableBalanceWhiteAuto()
        {
            var mode = MyCamera.MV_CAM_BALANCEWHITE_AUTO.MV_BALANCEWHITE_AUTO_OFF;
            return setEnumValue("BalanceWhiteAuto", (uint)mode);
        }
        /// <summary>
        /// 關掉自動增益
        /// </summary>
        public int disableGainAuto()
        {
            var mode = MyCamera.MV_CAM_GAIN_MODE.MV_GAIN_MODE_OFF;
            return setEnumValue("GainAuto", (uint)mode);
        }
        #endregion

        //--- Camera Modes ------------------------------------------------------------------------------------------------
        #region CAMERA_MODES

        #region PRIVATE_CACHE_DATA
        private uint _cacheAcquisitionMode;
        private uint _cacheTriggerSource;
        private uint _cacheTriggerMode;
        #endregion
        public MyCamera.MV_CAM_ACQUISITION_MODE AcquisitionMode
        {
            get
            {
                return (MyCamera.MV_CAM_ACQUISITION_MODE)_cacheAcquisitionMode;
            }
            set
            {
                setAcquisitionMode(value);
            }
        }
        public MyCamera.MV_CAM_TRIGGER_SOURCE TriggerSource
        {
            get
            {
                return (MyCamera.MV_CAM_TRIGGER_SOURCE)_cacheTriggerSource;
            }
            set
            {
                setTriggerSource(value);
            }
        }
        public MyCamera.MV_CAM_TRIGGER_MODE TriggerMode
        {
            get
            {
                return (MyCamera.MV_CAM_TRIGGER_MODE)_cacheTriggerMode;
            }
            set
            {
                setTriggerMode(value);
            }
        }

        /// <summary>
        /// 設定 采集模式
        /// </summary>
        public int setAcquisitionMode(MyCamera.MV_CAM_ACQUISITION_MODE value, bool readBack = true)
        {
            string propertyName = "AcquisitionMode";
            int ret = setEnumValue(propertyName, (uint)value);
            if (ret == MyCamera.MV_OK)
            {
                if (readBack)
                {
                    ret = getEnumValue(propertyName, out uint result);
                    if (ret == MyCamera.MV_OK)
                        _cacheAcquisitionMode = result;
                }
                else
                {
                    _cacheAcquisitionMode = (uint)value;
                }
            }
            return ret;
        }
        /// <summary>
        /// 触发源选择:
        /// 0 - Line0; 
        /// 1 - Line1; 
        /// 2 - Line2; 
        /// 3 - Line3;
        /// 4 - Counter; 
        /// 7 - Software
        /// </summary>
        public int setTriggerSource(MyCamera.MV_CAM_TRIGGER_SOURCE value, bool readBack = true)
        {
            string propertyName = "TriggerSource";
            int ret = setEnumValue(propertyName, (uint)value);
            if (ret == MyCamera.MV_OK)
            {
                if (readBack)
                {
                    ret = getEnumValue(propertyName, out uint result);
                    if (ret == MyCamera.MV_OK)
                        _cacheTriggerSource = result;
                }
                else
                {
                    _cacheTriggerSource = (uint)value;
                }
            }
            return ret;
        }
        /// <summary>
        /// 設定 触发模式 ON/OFF
        /// </summary>
        public int setTriggerMode(MyCamera.MV_CAM_TRIGGER_MODE value, bool readBack = true)
        {
            string propertyName = "TriggerMode";
            int ret = setEnumValue(propertyName, (uint)value);
            if (ret == MyCamera.MV_OK)
            {
                if (readBack)
                {
                    ret = getEnumValue(propertyName, out uint result);
                    if (ret == MyCamera.MV_OK)
                        _cacheTriggerMode = result;
                }
                else
                {
                    _cacheTriggerMode = (uint)value;
                }
            }
            return ret;
        }
        #endregion
    }
}
