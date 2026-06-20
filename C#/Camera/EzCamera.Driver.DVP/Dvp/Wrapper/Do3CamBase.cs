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

using DVPCameraType;
using EzCamera.Interface;
using System;
using System.Drawing;


namespace EzCamera.Driver.DVP
{
    /// <summary>
    /// 提供 度申 包裝後的 基本函式 內部自帶 error message 處理.
    /// <br/> 具體的 控制流程, Callbacks, 與 FrameBuf 管理, 由繼承者實作之.
    /// </summary>
    public abstract class Do3CamBase
    {
        public event EventHandler<EzLiveImageEventArgs> OnFrameCaptured;
        public event EventHandler<Do3Error> OnError;

        #region PROTECTED_DVP_核心成員
        protected uint _cameraHandle;
        protected dvpCameraInfo _deviceInfo;
        #endregion

        //--- ERROR Handling Fuctions ---------------------------------------------------------------------------------------
        /// <summary>
        /// 使用 Event 傳遞 Error Messages
        /// </summary>
        protected void _ERR(dvpStatus errCode, string errMsg, string description = null)
        {
            OnError?.Invoke(this, new Do3Error((int)errCode, errMsg, description));
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

        //--- Camera Image Info ------------------------------------------------------------------------------------------
        #region CAMERA_IMAGE_INFO
#if (false)
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
#endif
        #endregion

        //--- Camera Properties -------------------------------------------------------------------------------------------
        #region CAMERA_PROPERTIES

        #region PRIVATE_CACHE_DATA
        private double _cacheExpo;
        private double _cacheGain;
        #endregion

        public double ExposureTime
        {
            get => _cacheExpo;
            set => setExposureTime(value);
        }
        public double Gain
        {
            get => _cacheGain;
            set => setGain(value);
        }

        public dvpStatus getExposureTime(out double value, out double min, out double max)
        {
            var descriptor = new dvpDoubleDescr();
            var ret = DVPCamera.dvpGetExposureDescr(_cameraHandle, ref descriptor);
            if (dvpStatus.DVP_STATUS_OK != ret)
            {
                _ERR(ret, "失敗於 dvpGetExposureDescr");
                value = 0; min = 0; max = 0;
            }
            else
            {
                value = descriptor.fDefault;
                min = descriptor.fMin;
                max = descriptor.fMax;
                _cacheExpo = value;
            }
            return ret;
        }
        public dvpStatus setExposureTime(double value)
        {
            var ret = DVPCamera.dvpSetExposure(_cameraHandle, value);
            if (dvpStatus.DVP_STATUS_OK != ret)
            {
                _ERR(ret, "失敗於 dvpSetExposure");
            }
            return ret;
        }
        public dvpStatus getGain(out double value, out double min, out double max)
        {
            var descriptor = new dvpFloatDescr();
            var ret = DVPCamera.dvpGetAnalogGainDescr(_cameraHandle, ref descriptor);
            if (dvpStatus.DVP_STATUS_OK != ret)
            {
                _ERR(ret, "失敗於 dvpGetAnalogGainDescr");
                value = 0; min = 0; max = 0;
            }
            else
            {
                value = descriptor.fDefault;
                min = descriptor.fMin;
                max = descriptor.fMax;
                _cacheExpo = value;
            }
            return ret;
        }
        public dvpStatus setGain(double value)
        {
            var ret = DVPCamera.dvpSetAnalogGain(_cameraHandle, (float)value);
            if (dvpStatus.DVP_STATUS_OK != ret)
            {
                _ERR(ret, "失敗於 dvpSetAnalogGain");
            }
            return ret;
        }
        public dvpStatus getFPS(out float value, out float min, out float max)
        {
            throw new NotImplementedException();
        }
        public int setFPS(float value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 關掉自動白平衡
        /// </summary>
        public int disableBalanceWhiteAuto()
        {
            //var mode = MyCamera.MV_CAM_BALANCEWHITE_AUTO.MV_BALANCEWHITE_AUTO_OFF;
            //return setEnumValue("BalanceWhiteAuto", (uint)mode);
            throw new NotImplementedException();
        }
        /// <summary>
        /// 關掉自動增益
        /// </summary>
        public int disableGainAuto()
        {
            //var mode = MyCamera.MV_CAM_GAIN_MODE.MV_GAIN_MODE_OFF;
            //return setEnumValue("GainAuto", (uint)mode);
            throw new NotImplementedException();
        }
        #endregion

        //--- Camera Trigger Settings ------------------------------------------------------------------------------------------------
        #region CAMERA_TRIGGER_SETTINGS

        #region PRIVATE_CACHE_DATA
        //private int _cacheAcquisitionMode;
        private dvpTriggerSource _cacheTriggerSource;
        private bool _cacheTriggerMode;
        #endregion

#if(false)
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
#endif
        public dvpTriggerSource TriggerSource
        {
            get
            {
                return _cacheTriggerSource;
            }
            set
            {
                setTriggerSource(value);
            }
        }
        public bool TriggerMode
        {
            get
            {
                return _cacheTriggerMode;
            }
            set
            {
                setTriggerMode(value);
            }
        }

#if(false)
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
#endif

        /// <summary>
        /// 触发源选择:
        /// </summary>
        public dvpStatus setTriggerSource(dvpTriggerSource value, bool readBack = true)
        {
            var ret = DVPCamera.dvpSetTriggerSource(_cameraHandle, value);
            if (ret == dvpStatus.DVP_STATUS_OK)
            {
                if (readBack)
                {
                    ret = DVPCamera.dvpGetTriggerSource(_cameraHandle, ref value);
                    if (ret == dvpStatus.DVP_STATUS_OK)
                        _cacheTriggerSource = value;
                }
                else
                {
                    _cacheTriggerSource = value;
                }
            }
            return ret;
        }

        /// <summary>
        /// 設定 触发模式 ON/OFF
        /// </summary>
        public dvpStatus setTriggerMode(bool value, bool readBack = true)
        {
            var ret = DVPCamera.dvpSetTriggerState(_cameraHandle, value);
            if (ret == dvpStatus.DVP_STATUS_OK)
            {
                if (readBack)
                {
                    ret = DVPCamera.dvpGetTriggerState(_cameraHandle, ref value);
                    if (ret == dvpStatus.DVP_STATUS_OK)
                        _cacheTriggerMode = value;
                }
                else
                {
                    _cacheTriggerMode = value;
                }
            }
            return ret;
        }
        #endregion
    }
}
