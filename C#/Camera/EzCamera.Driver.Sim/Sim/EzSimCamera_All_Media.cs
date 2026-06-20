#region AUTHOR
/*
 * 
 * Copyright (c) 2024 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2024-04-25 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzCamera.Driver.Base;
using EzCamera.Interface;
using System;
using System.Drawing;


namespace EzCamera.Driver.Sim
{
    /// <summary>
    /// 模擬相機 (使用多媒體模擬檔案)
    /// </summary>
    public class EzSimCamera : IEzCamera
    {
        #region CONFIG
        public bool OPT_AUTO_CIRCULATED = false;
        public bool OPT_SHOW_TIME_STAMP = false;
        public bool OPT_AUTO_FPS = false;
        #endregion

        #region FILES
        static string[] EXTS = new string[] { ".jpg", ".png", ".bmp", ".mp4", ".wmv", ".avi" };
        static bool isVideoFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return false;
            string ext = System.IO.Path.GetExtension(filePath).ToLower();
            int idx = Array.IndexOf(EXTS, ext);
            return (idx >= 3);
        }
        EzFileUtil _fileUtil = new EzFileUtil("Sim Camera", EXTS);
        #endregion

        #region PRIVATE_KERNEL_DATA
        IEzCamera _imp = null;
        #endregion

        #region DEFAULT
        public static EzCameraDeviceInfo DefaultDeviceInfo(int camID)
        {
            return new EzCameraDeviceInfo("Sim", "模擬相機(多媒體)", index: camID);
        }
        #endregion

        public event EventHandler<EzLiveImageEventArgs> OnLiveImage;
        public event EventHandler<EzCameraErrorEventArgs> OnError;
        public event EventHandler OnLiveModeChanged;

        public EzSimCamera(int camID)
            : this(DefaultDeviceInfo(camID))
        {
        }
        public EzSimCamera(IEzDeviceInfo info)
        {
            _deviceInfo = info;
            GlobalCamID = AllCreatedCameras.Register(this) - 1;
        }

        public void Init()
        {
            if (_imp == null)
            {
                loadIni();

                if (string.IsNullOrEmpty(_fileUtil.ActiveFileName))
                    load_camera("[DEFAULT]");
                else
                    Browse(_fileUtil.ActiveFileName);

                System.Diagnostics.Debug.Assert(_imp != null);
            }
            else
            {
                _imp.Init();
            }
        }
        public void Dispose()
        {
            AllCreatedCameras.Unregister(this);
            _imp?.Dispose();
            _imp = null;
        }
        public string Browse(string filePath)
        {
            if (filePath == "[LAST_FILE]")
                return _fileUtil.ActiveFileName;

            bool isLive = IsLiveMode();
            if (isLive)
            {
                StopLiveMode();
                System.Threading.Thread.Sleep(200);
            }

            bool isSpecialCommand = filePath != null && filePath.StartsWith("[") && filePath.EndsWith("]");

            try
            {
                string lastFile = _fileUtil.ActiveFileName;
                string newFile = _fileUtil.Browse(filePath);

                // 按了取消鍵
                if (newFile == null)
                {
                    if (_imp != null)
                    {
                        return lastFile;
                    }

                    if (lastFile != null && System.IO.File.Exists(lastFile))
                        newFile = lastFile;
                }

                if (!string.IsNullOrEmpty(newFile) && System.IO.File.Exists(newFile))
                {
                    // 利用 _fileUtil 防止 _imp 重複開啟 Browse
                    saveIni();
                    // 根據 檔案 filePath 載入不同的 _imp
                    bool is_camera_changed = load_camera(newFile);
                    // 如果 camera 沒有變動, 則調用 _imp.Browse
                    if (!is_camera_changed)
                    {
                        filePath = _imp.Browse(newFile);
                        if (filePath != newFile)
                            saveIni();
                    }
                }
                else
                {
                    if (_imp == null)
                    {
                        load_camera("[DEFAULT]");
                        System.Diagnostics.Debug.Assert(_imp != null);
                    }
                }

                return filePath;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return "";
            }
            finally
            {
                try
                {
                    if (!isSpecialCommand && isLive && _imp != null && !_imp.IsLarge())
                    {
                        // NOTE: 如果作用太頻繁,會當掉!
                        new Action(() =>
                        {
                            System.Threading.Thread.Sleep(100);
                            StartLiveMode();
                        }).BeginInvoke(null, null);
                    }
                    else
                    {
                        new Action(() =>
                        {
                            // 靜態顯示第一張圖
                            System.Threading.Thread.Sleep(100);
                            TriggerOneFrame();
                        }).BeginInvoke(null, null);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                }
            }
        }

        #region PRIVATE_INI_FUNCTIONS
        private void loadIni()
        {
            _fileUtil.LoadIni(null, $"SimCamera{CamID}");
        }
        private void saveIni()
        {
            _fileUtil.SaveIni(null, $"SimCamera{CamID}");
        }
        #endregion

        #region PRIVATE_FUNCTIONS
        bool load_camera(string filePath)
        {
            bool isChanged = false;
            if (isVideoFile(filePath))
            {
                if (!(_imp is EzSimVideoCamera))
                {
                    var fps = _imp != null ? _imp.MaxFps : 0f;

                    _imp?.Dispose();
                    _imp = new EzSimVideoCamera(CamID);
                    _imp.GlobalCamID = GlobalCamID;
                    connect_event_handlers();
                    _imp.Init();

                    if (fps > 0)
                        _imp.MaxFps = fps;

                    isChanged = true;
                }
            }
            else
            {
                if (!(_imp is EzSimImageCamera))
                {
                    var fps = _imp != null ? _imp.MaxFps : 0f;

                    _imp?.Dispose();
                    _imp = new EzSimImageCamera(CamID)
                    {
                        GlobalCamID = GlobalCamID,
                        OPT_AUTO_CIRCULATED = this.OPT_AUTO_CIRCULATED,
                        OPT_SHOW_TIME_STAMP = this.OPT_SHOW_TIME_STAMP,
                        OPT_AUTO_FPS = this.OPT_AUTO_FPS,
                    };

                    connect_event_handlers();
                    _imp.Init();

                    if (fps > 0)
                        _imp.MaxFps = fps;

                    isChanged = true;
                }
            }
            return isChanged;
        }
        void connect_event_handlers()
        {
            _imp.OnLiveModeChanged += (s, e) => OnLiveModeChanged?.Invoke(s, e);
            _imp.OnLiveImage += (s, e) => OnLiveImage?.Invoke(s, e);
            _imp.OnError += (s, e) => OnError?.Invoke(s, e);
        }
        #endregion

        #region NAMES_AND_ID
        int _globalCamID;
        IEzDeviceInfo _deviceInfo;
        public virtual IEzDeviceInfo DeviceInfo
        {
            get => _deviceInfo;
            protected set => _deviceInfo = value;
        }
        public virtual string FriendlyName
        {
            // 默認直接使用 EzCameraDeviceInfo.ToString()
            // get => $"{DeviceInfo.ToString()} ~{CamID}";
            get => _deviceInfo != null ?
                   _deviceInfo.ToString() :
                   EzDispText.Format(GetType().Name, CamID);
        }
        public int GlobalCamID
        {
            get
            {
                return _globalCamID;
            }
            set
            {
                _globalCamID = value;
                if (_imp != null)
                    _imp.GlobalCamID = value;
            }
        }
        public virtual int CamID
        {
            // 默認直接使用 DeviceInfo.Index 當作 CamID (局域)
            get => _deviceInfo != null ? _deviceInfo.Index : 0;
        }
        public override string ToString()
        {
            return FriendlyName;
        }
        #endregion

        #region WRAPPER
        public float MaxFps
        {
            get => _imp != null ? _imp.MaxFps : 0;
            set { if (_imp != null) _imp.MaxFps = value; }
        }
        public double ExposureTime
        {
            get => _imp.ExposureTime; 
            set => _imp.ExposureTime = value;
        }
        public double HardwareGain
        {
            get => _imp.HardwareGain; 
            set => _imp.HardwareGain = value;
        }
        public int Brightness
        {
            get => _imp.Brightness; 
            set => _imp.Brightness = value;
        }
        public int Contrast
        {
            get => _imp.Contrast; 
            set => _imp.Contrast = value;
        }
        public bool Mono
        {
            get => _imp != null ? _imp.Mono : false;
            set { if (_imp != null) _imp.Mono = value; }
        }
        public bool InverseModeEnabled
        {
            get => _imp.InverseModeEnabled; 
            set => _imp.InverseModeEnabled = value;
        }
        public int RotateAngle
        {
            get => _imp.RotateAngle; 
            set => _imp.RotateAngle = value;
        }
        public bool IsLarge()
        {
            return _imp != null && _imp.IsLarge();
        }

        public bool IsSimulation()
        {
            return true;
        }
        public EzCameraError GetError()
        {
            return _imp != null ? _imp.GetError() : EzCameraError.NoError;
        }
        public bool IsLiveMode()
        {
            return _imp != null ? _imp.IsLiveMode() : false;
        }
        public void StartLiveMode()
        {
            _imp?.StartLiveMode();
        }
        public void StopLiveMode()
        {
            _imp?.StopLiveMode();
        }
        public Bitmap Snapshot(bool fireEvent = false)
        {
            return _imp?.Snapshot(fireEvent);
        }
        public void TriggerOneFrame()
        {
            _imp?.TriggerOneFrame();
        }
        public Color GetPixelColor(int x, int y)
        {
            return _imp!=null ? _imp.GetPixelColor(x, y) : Color.Black;
        }
        public bool HasPropertyPanel()
        {
            return _imp != null && _imp.HasPropertyPanel();
        }
        public void ShowPropertyPanel(bool show, object parentWindow = null)
        {
            _imp?.ShowPropertyPanel(show, parentWindow);
        }
        public void GetImageInfo(out int width, out int height, out int bitsPerPixel)
        {
            if (_imp != null)
            {
                _imp.GetImageInfo(out width, out height, out bitsPerPixel);
            }
            else
            {
                width = 1; height = 1; bitsPerPixel = 24;
            }
        }
        public void GetExposureRange(out double min, out double max)
        {
            if (_imp != null)
                _imp.GetExposureRange(out min, out max);
            else
                min = max = 1;
        }
        public void GetHardwareGainRange(out double min, out double max)
        {
            if (_imp != null)
                _imp.GetHardwareGainRange(out min, out max);
            else
                min = max = 1;
        }
        public void GetBrightnessRange(out int min, out int max)
        {
            if (_imp != null)
                _imp.GetBrightnessRange(out min, out max);
            else
                min= max = 1;
        }
        public void GetContrastRange(out int min, out int max)
        {
            if (_imp != null)
                _imp.GetContrastRange(out min, out max);
            else
                min = max = 1;
        }

        #endregion
    }
}
