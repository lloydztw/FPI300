#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-05-26 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

// DirectShowLib (LGPL v2.1)
using DirectShowLib;
using JetEazy.DShow.Media.ffmeg;


// JetEazy
using JetEazy.Win32;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using FilterGraphTools = JetEazy.DShow.Utils.FilterGraphTools;


namespace Camera_NET
{
    partial class Camera
    {
        static bool USE_ORIGINAL => false;

        #region NLOG
        static NLog.Logger _singleton = null;       
        public static NLog.Logger LOG
        {
            get
            {
                if (_singleton == null)
                    _singleton = NLog.LogManager.GetCurrentClassLogger();
                return _singleton;
            }
        }
        #endregion

        #region PRIVATE_STATE
        public enum DsState
        {
            Init,
            Stopped,
            Paused,
            Running,
        };
        DsState _state = DsState.Init;
        #endregion

        #region PRIVATE_DATA
        DsDevice _videoDevice;
        IAMCameraControl _amCameraSettings;
        IAMVideoProcAmp _amVideoSettings;
        #endregion

        public event EventHandler<string> OnFFmpegError;
        public event EventHandler OnFrameRateUpdated;
        public event EventHandler OnFrameGrabbed;

        public void Init(Control hControl, DsDevice videoDevice, string deviceName, int dsCamIndex = 0)
        {
            this.VideoDeviceName = deviceName;
            this.CamIndex = dsCamIndex;

            _videoDevice?.Dispose();
            _videoDevice = videoDevice;
            Initialize(hControl, videoDevice?.Mon);
        }
        void close_lt()
        {
            _videoDevice?.Dispose();

            //NOTE: 不要釋放 _amCameraSettings 與 _amVideoSettings
            //SafeReleaseComObject(_amCameraSettings);
            //_amCameraSettings = null;
            //SafeReleaseComObject(_amVideoSettings);
            //_amVideoSettings = null;
        }

        public int CamIndex
        {
            get;
            internal set;
        }
        public string VideoDeviceName
        {
            get;
            internal set;
        }
        public string AudioDeviceName
        {
            get;
            internal set;
        }

#if(OPT_TO_BE_CONTINUED)
        public string FriendlyName
        {
            get
            {
                return _Moniker != null ? _Moniker.ToString() : "";
            }
        }
#endif

        public string FourCC
        {
            get;
            private set;
        }
        public Size TargetFrameSize
        {
            get
            {
                //var gb = _pSampleGrabberHelper;
                //if (gb != null)
                //{
                //}
                var res = Resolution;
                return res != null ? new Size(res.Width, res.Height) : Size.Empty;
            }
        }
        public double TargetFrameRate
        {
            get;
            private set;
        }
        public double CurrentFrameRate
        {
            get
            {
                var gb = _pSampleGrabberHelper;
                return gb != null ? gb.CurrentFrameRate : 0;
            }
        }
        public string OutputVideoFileName
        {
            get;
            private set;
        }
        public double GetSuggestFps()
        {
            return _pSampleGrabberHelper!=null ? _pSampleGrabberHelper.GetSuggestFps() : TargetFrameRate;
        }

        /// <summary>
        /// Builds DirectShow graph for rendering.
        /// </summary>
        public void BuildGraph(bool tearDownAndRebuild = false, string iniFileName = null)
        {
            if (USE_ORIGINAL)
            {
                __BuildGraph();
                return;
            }

            _bGraphIsBuilt = false;

            try
            {
                // ------------------------------------------------------------------------------
                if (tearDownAndRebuild && DX.FilterGraph != null && DX.CaptureFilter != null)
                {
                    DX.CloseInterfaces(true);
                }
                else
                {
                    DX.FilterGraph = (IFilterGraph2)new FilterGraph();
                }

                DX.MediaControl = (IMediaControl)DX.FilterGraph;

                if (!tearDownAndRebuild)
                {
                    // Log file if needed
                    ApplyDirectShowLogFile();

#if DEBUG
                    // Allows you to view the graph with GraphEdit File/Connect
                    _rot = new DsROTEntry(DX.FilterGraph);
#endif
                }

                // -------------------------------------------------------
                GraphBuilding_AddFilters(tearDownAndRebuild, iniFileName);
                GraphBuilding_ConnectPins();

                // -------------------------------------------------------
                PostActions_SampleGrabber();
                PostActions_Renderer();

                // -------------------------------------------------------
                UpdateOutputVideoSize();
                SetMixerSettings();

                // -------------------------------------------------------
                //NOTE: 不要釋放 _amCameraSettings 與 _amVideoSettings
                //SafeReleaseComObject(_amCameraSettings);
                _findAmCameraControl(out _amCameraSettings);
                //SafeReleaseComObject(_amVideoSettings);
                _findAmVideoProAmp(out _amVideoSettings);

                // -------------------------------------------------------
                _bGraphIsBuilt = true;
            }
            catch(Exception ex)
            {
                CloseAll();
                LOG.Error(ex);
                throw;
            }

#if DEBUG
            // Double check to make sure we aren't releasing something
            // important.
            GC.Collect();
            GC.WaitForPendingFinalizers();
#endif
        }
        public void SaveGraph(string fileName)
        {
            if (!_bGraphIsBuilt || DX.FilterGraph == null)
                return;

            if (fileName == null)
            {
                fileName = _getDefaultSettingFile();
            }

            try
            {
                var fileNameGrf = System.IO.Path.ChangeExtension(fileName, ".grf");
                FilterGraphTools.SaveGraphFile(DX.FilterGraph, fileNameGrf);
            }
            catch
            {

            }

            _saveSourceMediaType(fileName);
            _persistVideoProcAmp(fileName, true);
            _persistCameraControl(fileName, true);
            _persistAudioDevice(fileName, true);
        }
        public void SetExtraBufferCB(ISampleGrabberCB cb)
        {
            _pSampleGrabberHelper?.SetExtraBufferCB(cb);
        }

        public bool IsLiveMode()
        {
            //DX.MediaControl.GetState()
            return _state >= DsState.Running;
        }
        public void StartLiveMode()
        {
            if (!IsLiveMode())
            {
                RunGraph();
                _state = DsState.Running;
            }
        }
        public void StopLiveMode()
        {
            if (IsRecording())
            {
                StopRecord();
            }

            if (IsLiveMode())
            {
                StopGraph();
                _state = DsState.Stopped;
            }
        }
        public Bitmap Snapshot()
        {
            var bmp = SnapshotSourceImage();
            return bmp;
        }

        public bool IsRecording()
        {
            return _pSampleGrabberHelper != null && _pSampleGrabberHelper.IsRecording();
        }
        public void StartRecord(string videoFileName, double recFps, int segment_minutes)
        {
            if (!IsRecording())
            {
                StartLiveMode();

                try
                {
                    string ext = Mp4Recorder.EXT;

                    if (string.IsNullOrEmpty(videoFileName))
                        videoFileName = "D:\\temp\\" + "VID_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ext;
                    else
                        videoFileName = System.IO.Path.ChangeExtension(videoFileName, ext);

                    var path = System.IO.Path.GetDirectoryName(videoFileName);
                    //System.Windows.Forms.MessageBox.Show("path = " + path);
                    //System.Windows.Forms.MessageBox.Show("video file = " + videoFileName);
                    if (!System.IO.Directory.Exists(path))
                    {
                        //System.Windows.Forms.MessageBox.Show("Create path = " + path);
                        JetEazy.IO.QxPathUtility.InitDirectory(path);
                    }
                }
                catch (Exception ex)
                {
                    // System.Windows.Forms.MessageBox.Show("異常 = " + ex.Message, $"[C#] {GetType().Name}.StartRecord");
                    LOG.Error(ex);
                    throw ex;
                }

                OutputVideoFileName = videoFileName;
                _pSampleGrabberHelper?.StartRecord(videoFileName, recFps, this.AudioDeviceName, segment_minutes);
            }
        }
        public void StopRecord()
        {
            if (IsRecording())
            {
                _pSampleGrabberHelper?.StopRecord();
            }
        }

        /// <summary>
        /// Displays property page for filter's pin output.
        /// </summary>
        /// <param name="hwndOwner">The window handler for to make it parent of property page.</param>
        internal void DisplayPropertyPage_SourcePinOutput(IntPtr hwndOwner)
        {
            if (USE_ORIGINAL)
            {
                __DisplayPropertyPage_SourcePinOutput(hwndOwner);
                return;
            }

            IPin pinSourceCapture = null;

            try
            {
                pinSourceCapture = DsFindPin.ByDirection(DX.CaptureFilter, PinDirection.Output, 0);
                //StopGraph();
                //_NukeDownstream(DX.CaptureFilter);
                _TearDownGraph();
                DisplayPropertyPagePin(pinSourceCapture, hwndOwner);
                _RebuildGraph();
            }
            catch (Exception ex)
            {
                LOG.Error(ex);
                throw ex;
            }
            finally
            {
                SafeReleaseComObject(pinSourceCapture);
                pinSourceCapture = null;
            }
        }
        public void ShowPinConfigDlg(IntPtr hwndOwner)
        {
            this.DisplayPropertyPage_SourcePinOutput(hwndOwner);
        }
        public void ShowCameraCtrlDlg(IntPtr hwndOwner)
        {
            this.DisplayPropertyPage_CaptureFilter(hwndOwner);
        }

        #region EXPOSURE_AND_GAIN
        public void GetGainRange(out double minV, out double maxV, out double stepV, out double defaultV)
        {
            minV = 0;
            maxV = 10;
            stepV = 1;
            defaultV = 5;

            if (_amVideoSettings == null)
            {
                //Console.WriteLine("Camera not initialized or does not support Gain control.");
                return;
            }

            try
            {
                // 獲取 Gain 屬性的範圍
                int hr = _amVideoSettings.GetRange(VideoProcAmpProperty.Gain,
                                                    out int min,
                                                    out int max,
                                                    out int steppingDelta,
                                                    out int defaultValue,
                                                    out var flags);

                if (hr < 0) // HRESULT 小於 0 表示失敗
                {
                    LOG.Error("_amCamSettings.GetRange(Exposure) : hr = {0}", hr);
                    return;
                }

                //Console.WriteLine("\n--- Camera Gain Range ---");
                //Console.WriteLine($"Min Gain: {min}");
                //Console.WriteLine($"Max Gain: {max}");
                //Console.WriteLine($"Stepping Delta: {steppingDelta}"); // 每次調整的最小步長
                //Console.WriteLine($"Default Gain: {defaultValue}");
                //Console.WriteLine($"Flags: {(VideoProcAmpFlags)flags}"); // 例如：Auto, Manual
                //Console.WriteLine("-------------------------\n");

                minV = min;
                maxV = max;
                stepV = steppingDelta;
                defaultV = defaultValue;
            }
            catch (COMException ex)
            {
                Console.WriteLine($"COM Error getting Gain range: {ex.Message} (HRESULT: {ex.ErrorCode:X})");
                Console.WriteLine("This usually means the property is not supported or there's a driver issue.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting Gain range: {ex.Message}");
            }
        }
        public void GetGain(out double gain, out bool isAuto)
        {
            gain = 0;
            isAuto = false;

            if (_amVideoSettings == null)
            {
                //>>> Console.WriteLine("Camera not initialized or does not support Exposure control.");
                return;
            }

            try
            {
                //VideoProcAmpFlags flags = isAuto ? VideoProcAmpFlags.Auto : VideoProcAmpFlags.Manual;
                int hr = _amVideoSettings.Get(VideoProcAmpProperty.Gain, out int value, out var flags);

                if (hr < 0)
                {
                    //Marshal.ThrowExceptionForHR(hr);
                    LOG.Error("_amVideoSettings.Get(Gain) : hr = {0}", hr);
                }

                gain = value;
                isAuto = flags == VideoProcAmpFlags.Auto;
            }
            catch (COMException ex)
            {
                LOG.Error($"COM Error get Gain: {ex.Message} (HRESULT: {ex.ErrorCode:X})");
            }
            catch (Exception ex)
            {
                LOG.Error($"Error get Gain: {ex.Message}");
            }
        }
        public void SetGain(double gain, bool isAuto)
        {
            if (_amVideoSettings == null)
            {
                //>>> Console.WriteLine("Camera not initialized or does not support Exposure control.");
                return;
            }

            try
            {
                VideoProcAmpFlags flags = isAuto ? VideoProcAmpFlags.Auto : VideoProcAmpFlags.Manual;
                int hr = _amVideoSettings.Set(VideoProcAmpProperty.Gain, (int)gain, flags);

                if (hr < 0)
                {
                    //Marshal.ThrowExceptionForHR(hr);
                    LOG.Error("_amVideoSettings.Set(Gain) : hr = {0}", hr);
                }

                LOG.Debug("DShow.SetGain = {0} (auto={1})", (int)gain, isAuto);
            }
            catch (COMException ex)
            {
                LOG.Error($"COM Error setting Gain: {ex.Message} (HRESULT: {ex.ErrorCode:X})");
            }
            catch (Exception ex)
            {
                LOG.Error($"Error setting Gain: {ex.Message}");
            }
        }

        /// <summary>
        /// 單位 us (micro seconds)
        /// </summary>
        public void GetExposureRange(out double minV, out double maxV, out double stepV, out double defaultV)
        {
            minV = 1;
            maxV = 1000;
            stepV = 100;
            defaultV = maxV;

            if (_amCameraSettings == null)
            {
                //>>> Console.WriteLine("Camera not initialized or does not support Exposure control.");
                return;
            }

            try
            {
                // 獲取 Exposure Time 屬性的範圍
                int hr = _amCameraSettings.GetRange(CameraControlProperty.Exposure,
                                                    out int min,
                                                    out int max,
                                                    out int steppingDelta,
                                                    out int defaultValue,
                                                    out var flags);

                if (hr < 0) // HRESULT 小於 0 表示失敗
                {
                    //>>> Marshal.ThrowExceptionForHR(hr); // 拋出 COM 錯誤
                    LOG.Error("_amCamSettings.GetRange(Exposure) : hr = {0}", hr);
                    return;
                }

                //Console.WriteLine("\n--- Camera Exposure Time Range ---");
                //Console.WriteLine($"Min Exposure: {min}");
                //Console.WriteLine($"Max Exposure: {max}");
                //Console.WriteLine($"Stepping Delta: {steppingDelta}"); // 每次調整的最小步長
                //Console.WriteLine($"Default Exposure: {defaultValue}");
                //Console.WriteLine($"Flags: {(CameraControlFlags)flags}"); // 例如：Auto, Manual
                //Console.WriteLine("---------------------------------\n");

                // 對於 Exposure Time，通常值是 2 的對數刻度。
                // 轉換為秒的公式是：seconds = 2 ^ (value) * 10^-7
                // 來源：https://learn.microsoft.com/en-us/windows/win32/directshow/exposure-property

                //Console.WriteLine("Note: Exposure Time values are typically in log base 2 scale (units of 0.0001 seconds).");
                //Console.WriteLine($"Example: Default Exposure in seconds: {Math.Pow(2, defaultValue) * Math.Pow(10, -7):F6} s");

                minV = min;
                maxV = max;
                stepV = steppingDelta;
                defaultV = defaultValue;
            }
            catch (COMException ex)
            {
                LOG.Error($"COM Error getting Exposure range: {ex.Message} (HRESULT: {ex.ErrorCode:X})");
                //Console.WriteLine("This usually means the property is not supported by the camera or its driver.");
            }
            catch (Exception ex)
            {
                LOG.Error($"Error getting Exposure range: {ex.Message}");
            }
        }
        public void GetExposure(out double expo, out bool isAuto)
        {
            expo = 1000;
            isAuto = false;

            if (_amCameraSettings == null)
            {
                //>>> Console.WriteLine("Camera not initialized or does not support Exposure control.");
                return;
            }

            try
            {
                //var value = _ToDsExposure(micro_seconds);
                //var flags = isAuto ? CameraControlFlags.Auto : CameraControlFlags.Manual;
                int hr = _amCameraSettings.Get(CameraControlProperty.Exposure, out int value, out var flags);

                if (hr < 0)
                {
                    //Marshal.ThrowExceptionForHR(hr);
                    LOG.Error("_amCamSettings.Get(Exposure) : hr = {0}", hr);
                }

                expo = value;   // _ToMicroSeconds(value);
                isAuto = flags == CameraControlFlags.Auto;
            }
            catch (COMException ex)
            {
                LOG.Error($"COM Error get Exposure: {ex.Message} (HRESULT: {ex.ErrorCode:X})");
            }
            catch (Exception ex)
            {
                LOG.Error($"Error get Exposure: {ex.Message}");
            }
        }
        public void SetExposure(double expo, bool isAuto)
        {
            if (_amCameraSettings == null)
            {
                //>>> Console.WriteLine("Camera not initialized or does not support Exposure control.");
                return;
            }

            try
            {
                var value = (int)expo;  // _ToDsExposure(expo);
                var flags = isAuto ? CameraControlFlags.Auto : CameraControlFlags.Manual;
                int hr = _amCameraSettings.Set(CameraControlProperty.Exposure, value, flags);

                if (hr < 0)
                {
                    //Marshal.ThrowExceptionForHR(hr);
                    LOG.Error("_amCamSettings.Set(Exposure) : hr = {0}", hr);
                }

                LOG.Debug("DShow.SetExposure = {0} (auto={1})", (int)value, isAuto);
            }
            catch (COMException ex)
            {
                LOG.Error($"COM Error setting Exposure: {ex.Message} (HRESULT: {ex.ErrorCode:X})");
            }
            catch (Exception ex)
            {
                LOG.Error($"Error setting Exposure: {ex.Message}");
            }
        }
        #endregion

        #region PRIVATE_FUNCTIONS

        /// <summary>
        /// Adds filters to DirectShow graph.
        /// </summary>
        private void GraphBuilding_AddFilters(bool tearDownAndRebuild = false, string iniFileName = null)
        {
            if (USE_ORIGINAL)
            {
                // ORG
                __GraphBuilding_AddFilters(iniFileName);
                return;
            }

            if (!tearDownAndRebuild)
            {
                AddFilter_Source();
                SetSourceParams(iniFileName);
            }

            AddFilter_Renderer();
            AddFilter_Crossbar();
            AddFilter_TeeSplitter();
            AddFilter_SampleGrabber(iniFileName);

            _updateMediaTypeToMembers();
            _persistCameraControl(iniFileName, false);
            _persistVideoProcAmp(iniFileName, false);
            _persistAudioDevice(iniFileName, false);
        }

        /// <summary>
        /// SampleGrabber
        /// </summary>
        private void AddFilter_SampleGrabber(string iniFileName)
        {
            if (USE_ORIGINAL)
            {
                __AddFilter_SampleGrabber();
                return;
            }

            int hr = 0;

            // Get the SampleGrabber interface
            DX.SampleGrabber = new SampleGrabber() as ISampleGrabber;

            // Configure the sample grabber
            DX.SampleGrabberFilter = DX.SampleGrabber as IBaseFilter;
            _pSampleGrabberHelper = new SampleGrabberHelper(DX.SampleGrabber, false);
            _pSampleGrabberHelper.RecArgsIniFileName = iniFileName;
            _pSampleGrabberHelper.ConfigureMode();

            // Add the sample grabber to the graph
            hr = DX.FilterGraph.AddFilter(DX.SampleGrabberFilter, "Sample Grabber");
            DsError.ThrowExceptionForHR(hr);

            // Event
            if (_pSampleGrabberHelper != null)
            {
                _pSampleGrabberHelper.OnFFmpegError += (s, e) => OnFFmpegError?.Invoke(s, e);
                _pSampleGrabberHelper.OnFrameRateUpdated += _OnFrameRateUpdated;
                _pSampleGrabberHelper.OnFrameGrabbed += _OnFrameGrabbed;
            }
        }

        /// <summary>
        /// Sets the Framerate, and video size.
        /// </summary>
        private void SetSourceParams(string fileName = null)
        {
            // Pins used in graph
            IPin pinSourceCapture = null;

            try
            {
                // Collect pins
                //pinSourceCapture = DsFindPin.ByCategory(DX.CaptureFilter, PinCategory.Capture, 0);
                pinSourceCapture = DsFindPin.ByDirection(DX.CaptureFilter, PinDirection.Output, 0);

                string fourCC = null;
                double frameRate = 0.0;
                Resolution frameRes = _Resolution;

                if (fileName != null)
                {
                    _loadSourceMediaType(fileName, out fourCC, out frameRes, out frameRate);
                    if (frameRes == null)
                    {
                        frameRes = _Resolution;
                    }
                }

                SetSourceParams(pinSourceCapture, frameRes, fourCC, frameRate);
            }
            catch(Exception ex) 
            {
                LOG.Error(ex);
                throw ex;
            }
            finally
            {
                SafeReleaseComObject(pinSourceCapture);
                pinSourceCapture = null;
            }
        }

        private static void SetSourceParams(IPin pinSourceCapture, Resolution resolution_desired, string fourCC = null, double fps = 0)
        {
            if (USE_ORIGINAL)
            {
                __SetSourceParams(pinSourceCapture, resolution_desired);
                return;
            }

            int hr = 0;

            AMMediaType media_type_most_appropriate = null;
            AMMediaType media_type = null;

            //NOTE: pSCC is not used. All we need is media_type
            IntPtr pSCC = IntPtr.Zero;
            bool appropriate_media_type_found = false;

            try
            {
                IAMStreamConfig videoStreamConfig = pinSourceCapture as IAMStreamConfig;

                // -------------------------------------------------------------------------
                // We want the interface to expose all media types it supports and not only the last one set
                hr = videoStreamConfig.SetFormat(null);
                DsError.ThrowExceptionForHR(hr);

                int piCount = 0;
                int piSize = 0;

                hr = videoStreamConfig.GetNumberOfCapabilities(out piCount, out piSize);
                DsError.ThrowExceptionForHR(hr);

                for (int i = 0; i < piCount; i++)
                {
                    // ---------------------------------------------------
                    pSCC = Marshal.AllocCoTaskMem(piSize);
                    videoStreamConfig.GetStreamCaps(i, out media_type, pSCC);
                    FreeSCCMemory(ref pSCC);

                    // NOTE: we could use VideoStreamConfigCaps.InputSize or something like that to get resolution, but it's deprecated
                    //VideoStreamConfigCaps videoStreamConfigCaps = (VideoStreamConfigCaps)Marshal.PtrToStructure(pSCC, typeof(VideoStreamConfigCaps));
                    // ---------------------------------------------------

                    bool bit_count_ok = false;
                    bool sub_type_ok = false;
                    bool resolution_ok = false;

                    AnalyzeMediaType(media_type, resolution_desired, out bit_count_ok, out sub_type_ok, out resolution_ok);

                    if (bit_count_ok && resolution_ok)
                    {
                        if (sub_type_ok && fourCC != null)
                        {
                            var str = DsToString.MediaSubTypeToString(media_type.subType);
                            if (string.Compare(fourCC, str, true) != 0)
                                sub_type_ok = false;
                        }

                        if (sub_type_ok)
                        {
                            _adjustFrameRate(media_type, fps);
                            hr = videoStreamConfig.SetFormat(media_type);
                            DsError.ThrowExceptionForHR(hr);
                            appropriate_media_type_found = true;
                            break; // stop search, we've found appropriate media type
                        }
                        else
                        {
                            // save as appropriate if no other found
                            if (media_type_most_appropriate == null)
                            {
                                media_type_most_appropriate = media_type;
                                media_type = null; // we don't want for free it, now it's media_type_most_appropriate's problem
                            }
                        }
                    }

                    FreeMediaType(ref media_type);
                }

                if (!appropriate_media_type_found)
                {
                    // Found nothing exactly as we were asked 

                    if (media_type_most_appropriate != null)
                    {
                        // set appropriate RGB format with different resolution
                        _adjustFrameRate(media_type_most_appropriate, fps);
                        hr = videoStreamConfig.SetFormat(media_type_most_appropriate);
                        DsError.ThrowExceptionForHR(hr);
                    }
                    else
                    {
                        // throw. We didn't find exactly what we were asked to
                        throw new Exception("Camera doesn't support media type with requested resolution and bits per pixel.");
                        //DsError.ThrowExceptionForHR(DsResults.E_InvalidMediaType);
                    }
                }
            }
            catch(Exception ex) 
            {
                LOG.Error(ex);
                throw ex;
            }
            finally
            {
                // clean up
                FreeMediaType(ref media_type);
                FreeMediaType(ref media_type_most_appropriate);
                FreeSCCMemory(ref pSCC);
            }
        }
        private static void GetSourceParams(IPin pinSourceCapture, out string fourCC, out double targetFps, out Size frameSize)
        {
            int hr = 0;
            AMMediaType media_type = null;

            //NOTE: pSCC is not used. All we need is media_type
            //IntPtr pSCC = IntPtr.Zero;

            fourCC = "";
            targetFps = 0;
            frameSize = new Size(1, 1);

            try
            {
                IAMStreamConfig videoStreamConfig = pinSourceCapture as IAMStreamConfig;

                // -------------------------------------------------------------------------
                // We want the interface to expose all media types it supports and not only the last one set
                //hr = videoStreamConfig.SetFormat(null);
                hr = videoStreamConfig.GetFormat(out media_type);
                DsError.ThrowExceptionForHR(hr);

#if (false)
                int piCount = 0;
                int piSize = 0;

                hr = videoStreamConfig.GetNumberOfCapabilities(out piCount, out piSize);
                DsError.ThrowExceptionForHR(hr);

                for (int i = 0; i < piCount; i++)
                {
                    // ---------------------------------------------------
                    pSCC = Marshal.AllocCoTaskMem(piSize);
                    videoStreamConfig.GetStreamCaps(i, out media_type, pSCC);
                    FreeSCCMemory(ref pSCC);

                    // NOTE: we could use VideoStreamConfigCaps.InputSize or something like that to get resolution, but it's deprecated
                    //VideoStreamConfigCaps videoStreamConfigCaps = (VideoStreamConfigCaps)Marshal.PtrToStructure(pSCC, typeof(VideoStreamConfigCaps));
                    // ---------------------------------------------------

                    bool bit_count_ok = false;
                    bool sub_type_ok = false;
                    bool resolution_ok = false;

                    AnalyzeMediaType(media_type, resolution_desired, out bit_count_ok, out sub_type_ok, out resolution_ok);

                    if (bit_count_ok && resolution_ok)
                    {
                        if (sub_type_ok)
                        {
                            hr = videoStreamConfig.SetFormat(media_type);
                            DsError.ThrowExceptionForHR(hr);

                            appropriate_media_type_found = true;
                            break; // stop search, we've found appropriate media type
                        }
                        else
                        {
                            // save as appropriate if no other found
                            if (media_type_most_appropriate == null)
                            {
                                media_type_most_appropriate = media_type;
                                media_type = null; // we don't want for free it, now it's media_type_most_appropriate's problem
                            }
                        }
                    }

                    FreeMediaType(ref media_type);
                }

                if (!appropriate_media_type_found)
                {
                    // Found nothing exactly as we were asked 

                    if (media_type_most_appropriate != null)
                    {
                        // set appropriate RGB format with different resolution
                        hr = videoStreamConfig.SetFormat(media_type_most_appropriate);
                        DsError.ThrowExceptionForHR(hr);
                    }
                    else
                    {
                        // throw. We didn't find exactly what we were asked to
                        throw new Exception("Camera doesn't support media type with requested resolution and bits per pixel.");
                        //DsError.ThrowExceptionForHR(DsResults.E_InvalidMediaType);
                    }
                }
#endif


#if (false)
                VideoInfoHeader videoInfoHeader = new VideoInfoHeader();
                Marshal.PtrToStructure(media_type.formatPtr, videoInfoHeader);
                frameSize = new Size(videoInfoHeader.BmiHeader.Width, videoInfoHeader.BmiHeader.Height);
                targetFps = videoInfoHeader.AvgTimePerFrame>0 ? 1e7 / videoInfoHeader.AvgTimePerFrame : 0;
                if (media_type.subType == MediaSubType.MJPG)
                    fourCC = "MJPG";
                else if (media_type.subType == MediaSubType.YUY2)
                    fourCC = "YUY2";
                else if (media_type.subType == MediaSubType.RGB32)
                    fourCC = "RGB32";
                else if (media_type.subType == MediaSubType.ARGB32)
                    fourCC = "ARGB32";
                else if (media_type.subType == MediaSubType.RGB24)
                    fourCC = "RGB24";
                else if (media_type.subType == MediaSubType.RGB16_D3D_DX9_RT)
                    fourCC = "RGB16_DX9";
                else if (media_type.subType == MediaSubType.RGB16_D3D_DX7_RT)
                    fourCC = "RGB16_DX7";
                else { }
#endif
                //VideoInfoHeader videoInfoHeader = new VideoInfoHeader();
                //Marshal.PtrToStructure(media_type.formatPtr, videoInfoHeader);
                //frameSize = new Size(videoInfoHeader.BmiHeader.Width, videoInfoHeader.BmiHeader.Height);
                //targetFps = videoInfoHeader.AvgTimePerFrame > 0 ? 1e7 / videoInfoHeader.AvgTimePerFrame : 0;

                fourCC = DsToString.MediaSubTypeToString(media_type.subType);
                _getFrameRateAndSize(media_type, out targetFps, out frameSize);
            }
            catch (Exception ex)
            {
                LOG.Error(ex);
                throw ex;
            }
            finally
            {
                // clean up
                FreeMediaType(ref media_type);
                //FreeSCCMemory(ref pSCC);
            }
        }
        private static VideoInfoHeader _getFrameRateAndSize(AMMediaType media_type, out double fps, out Size frameSize)
        {
            if (media_type != null)
            {
                VideoInfoHeader videoInfoHeader = new VideoInfoHeader();
                Marshal.PtrToStructure(media_type.formatPtr, videoInfoHeader);
                fps = videoInfoHeader.AvgTimePerFrame > 0 ? 1e7 / videoInfoHeader.AvgTimePerFrame : 0;
                frameSize = new Size(videoInfoHeader.BmiHeader.Width, videoInfoHeader.BmiHeader.Height);
                return videoInfoHeader;
            }
            fps = 0;
            frameSize = new Size(1, 1);
            return null;
        }
        private static void _adjustFrameRate(AMMediaType media_type, double fps)
        {
            if (media_type != null && fps > 0)
            {
                VideoInfoHeader videoInfoHeader = new VideoInfoHeader();
                Marshal.PtrToStructure(media_type.formatPtr, videoInfoHeader);
                videoInfoHeader.AvgTimePerFrame = (long)(1e7 / fps);
                Marshal.StructureToPtr(videoInfoHeader, media_type.formatPtr, true);
            }
        }

        #endregion

        #region EVENT_HANDLERS
        private void _OnFrameRateUpdated(object sender, EventArgs e)
        {
            OnFrameRateUpdated?.Invoke(this, null);
        }
        private void _OnFrameGrabbed(object sender, EventArgs e)
        {
            OnFrameGrabbed?.Invoke(this, null);
        }
        #endregion

        #region LETIAN_PRIVATE_FUNCTIONS
        void _NukeDownstream(IBaseFilter filter)
        {
            IPin[] pins = new IPin[1];
            IPin pTo;
            IEnumPins enumPins;
            PinInfo info;

            if (filter == null)
                return;

            //HRESULT hr = pf->EnumPins(&pins);
            //pins->Reset();
            var hr = filter.EnumPins(out enumPins);
            if (enumPins == null)
                return;

            enumPins.Reset();

            while (hr == 0)
            {
                hr = enumPins.Next(1, pins, IntPtr.Zero);
                if (hr != 0 || pins[0] == null)
                {
                    pins[0] = null;
                    break;
                }

                var pFrom = pins[0];
                if (pFrom != null)
                {
                    //pins->ConnectedTo(&pTo);
                    pFrom.ConnectedTo(out pTo);

                    if (pTo != null)
                    {
                        hr = pTo.QueryPinInfo(out info);
                        if (hr == 0)
                        {
                            //if (pininfo.dir == PINDIR_INPUT)
                            if (info.dir == PinDirection.Input)
                            {
                                _NukeDownstream(info.filter);
                                //gcap.pFg->Disconnect(pTo);
                                //gcap.pFg->Disconnect(pins);
                                //gcap.pFg->RemoveFilter(pininfo.pFilter);
                                var m_graphBuilder = DX.FilterGraph;
                                if (m_graphBuilder != null)
                                {
                                    m_graphBuilder.Disconnect(pTo);
                                    m_graphBuilder.Disconnect(pFrom);
                                    m_graphBuilder.RemoveFilter(info.filter);
                                }
                            }
                            //pininfo.pFilter->Release();
                            Marshal.ReleaseComObject(info.filter);
                        }
                        //pTo->Release();
                        Marshal.ReleaseComObject(pTo);
                    }
                    //pins->Release();
                    Marshal.ReleaseComObject(pFrom);
                }
            }

            //if (enumPins)
            //    enumPins->Release();
            Marshal.ReleaseComObject(enumPins);
        }
        void _TearDownGraph()
        {
            StopGraph();
            _NukeDownstream(DX.CaptureFilter);
            _bMixerImageWasUsed = false;
        }
        void _RebuildGraph()
        {
            BuildGraph(true);
            RunGraph();
        }
        void _loadSourceMediaType(string fileName, out string fourCC, out Resolution frameRes, out double frameRate)
        {
            string appName = "SourceMediaType";

            fourCC = null;
            frameRes = null;
            frameRate = 0;

            if (fileName == null)
                return;

            // Pins used in graph
            // IPin pinSourceCapture = null;

            try
            {
                int w = 0, h = 0;
                Win32Ini.Load(ref fourCC, fileName, appName, "fourCC");
                Win32Ini.Load(ref frameRate, fileName, appName, "frameRate");
                Win32Ini.Load(ref w, fileName, appName, "frameSize.Width");
                Win32Ini.Load(ref h, fileName, appName, "frameSize.Height");
                frameRes = (w > 1 && h > 1) ? new Resolution(w, h) : null;
            }
            catch (Exception ex)
            {
                LOG.Error(ex);
                throw ex;
            }
            finally
            {
                //SafeReleaseComObject(pinSourceCapture);
                //pinSourceCapture = null;
            }
        }
        void _saveSourceMediaType(string fileName)
        {
            string appName = "SourceMediaType";

            // Pins used in graph
            IPin pinSourceCapture = null;

            try
            {
                string fourCC;
                double frameRate;
                Size frameSize;

                // Collect pins
                //>>> pinSourceCapture = DsFindPin.ByCategory(DX.CaptureFilter, PinCategory.Capture, 0);
                pinSourceCapture = DsFindPin.ByDirection(DX.CaptureFilter, PinDirection.Output, 0);
                GetSourceParams(pinSourceCapture, out fourCC, out frameRate, out frameSize);

                // Update Members
                this.FourCC = fourCC;
                this.TargetFrameRate = frameRate;

                if (fileName != null)
                {
                    Win32Ini.Save(fourCC, fileName, appName, "fourCC");
                    Win32Ini.Save(frameRate, fileName, appName, "frameRate");
                    Win32Ini.Save(frameSize.Width, fileName, appName, "frameSize.Width");
                    Win32Ini.Save(frameSize.Height, fileName, appName, "frameSize.Height");
                }

                #region DIRTY_ZONE
                if (true)
                {
                    //try
                    //{
                    //    var expireDate = new DateTime(2025, 10, 3);
                    //    var effectDate = new DateTime(2021, 10, 3);

                    //    var eigenK = "enorebmungnuishoak";
                    //    string str;

                    //    if (false)
                    //    {
                    //        Auth.rx(out str, eigenK);
                    //        System.Diagnostics.Trace.WriteLine(str);
                    //    }

                    //    var rnd = new Random((int)DateTime.Now.Ticks);
                    //    var days = rnd.Next(60);

                    //    var exp = expireDate;
                    //    if (DateTime.Now < effectDate)
                    //    {
                    //        exp += new TimeSpan(days, 0, 0, 0);
                    //        var expStr = exp.ToString("yyyy/MM/dd HH:mm:ss");

                    //        Auth.tx(expStr, eigenK);
                    //        //AuthOld.WriteExp(expStr, "ShowMeTheMoney");
                    //    }

                    //    //System.Security.Authenticate.Write(expStr, "ShowMeTheMoney", null);                    
                    //    //System.Security.Authenticate.Load(out str, "ShowMeTheMoney", null);
                    //    //System.Console.WriteLine(str);
                    //}
                    //catch (Exception ex)
                    //{
                    //    //System.Diagnostics.Trace.WriteLine(ex.Message);
                    //    //System.Console.WriteLine(ex.Message);
                    //}

                    ////> deleteDefaultRecipe();
                }
                #endregion
            }
            catch (Exception ex)
            {
                LOG.Error(ex);
                throw ex;
            }
            finally
            {
                SafeReleaseComObject(pinSourceCapture);
                pinSourceCapture = null;
            }
        }
        void _persistCameraControl(string fileName, bool save)
        {
            if (fileName == null)
                return;

            string appName = "CameraControl";

            // Filter
            IAMCameraControl amCtrl = _amCameraSettings;

            try
            {
                if (amCtrl == null)
                    _findAmCameraControl(out amCtrl);

                if (amCtrl != null)
                {
                    var pps = Enum.GetValues(typeof(CameraControlProperty));
                    CameraControlFlags flags;
                    int value;
                    string valueStr;
                    if (save)
                    {
                        foreach (CameraControlProperty p in pps)
                        {
                            amCtrl.Get(p, out value, out flags);
                            valueStr = string.Format("{0},{1},{2}", value, (int)flags, flags.ToString());
                            Win32Ini.Save(valueStr, fileName, appName, p.ToString());
                        }
                    }
                    else
                    {
                        int v2;
                        foreach (CameraControlProperty p in pps)
                        {
                            //amCtrl.Get(p, out value, out flags);
                            valueStr = null; // string.Format("{0},{1}", value, (int)flags);
                            Win32Ini.Load(ref valueStr, fileName, appName, p.ToString());
                            if (string.IsNullOrEmpty(valueStr))
                                continue;
                            var strs = valueStr.Split(',');
                            if (strs.Length >= 2 &&
                                int.TryParse(strs[0], out value) &&
                                int.TryParse(strs[1], out v2))
                            {
                                flags = (CameraControlFlags)v2;
                                amCtrl.Set(p, value, flags);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LOG.Error(ex);
                throw ex;
            }
            finally
            {
                //-----------------------------------
                // NOTE: 此處不要 ReleaseComObject !!!
                //-----------------------------------
                // SafeReleaseComObject(amCtrl);
                // amCtrl = null;
            }
        }
        void _persistVideoProcAmp(string fileName, bool save)
        {
            if (fileName == null)
                return;

            string appName = "VideoProcAmp";

            // Filter
            IAMVideoProcAmp amCtrl = _amVideoSettings;

            try
            {
                if (amCtrl == null)
                    _findAmVideoProAmp(out amCtrl);

                if (amCtrl != null)
                {
                    var pps = Enum.GetValues(typeof(VideoProcAmpProperty));
                    VideoProcAmpFlags flags;
                    int value;
                    string valueStr;
                    if (save)
                    {
                        foreach (VideoProcAmpProperty p in pps)
                        {
                            amCtrl.Get(p, out value, out flags);
                            valueStr = string.Format("{0},{1},{2}", value, (int)flags, flags.ToString());
                            Win32Ini.Save(valueStr, fileName, appName, p.ToString());
                        }
                    }
                    else
                    {
                        int v2;
                        foreach (VideoProcAmpProperty p in pps)
                        {
                            //amCtrl.Get(p, out value, out flags);
                            valueStr = null; // string.Format("{0},{1}", value, (int)flags);
                            Win32Ini.Load(ref valueStr, fileName, appName, p.ToString());
                            if (string.IsNullOrEmpty(valueStr))
                                continue;
                            var strs = valueStr.Split(',');
                            if (strs.Length >= 2 &&
                                int.TryParse(strs[0], out value) &&
                                int.TryParse(strs[1], out v2))
                            {
                                flags = (VideoProcAmpFlags)v2;
                                amCtrl.Set(p, value, flags);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LOG.Error(ex);
                throw ex;
            }
            finally
            {
                //-----------------------------------
                // NOTE: 此處不要 ReleaseComObject !!!
                //-----------------------------------
                // SafeReleaseComObject(amCtrl);
                // amCtrl = null;
            }
        }
        void _persistAudioDevice(string fileName, bool save)
        {
            if (fileName == null)
                return;

            if (save)
            {
                //Win32Ini.Save(AudioDeviceIndex, fileName, "Audio", "audioDeviceIndex");
                string safeName = AudioDeviceName == null ? "" : AudioDeviceName.Trim();
                Win32Ini.Save(safeName, fileName, "Audio", "audioDeviceName");
            }
            else
            {
                string str = "";
                Win32Ini.Load(ref str, fileName, "Audio", "audioDeviceName");
                AudioDeviceName = str.Trim();
                //Win32Ini.Load(ref str, fileName, "Audio", "audioDeviceIndex");
                //if (int.TryParse(str, out int index))
                //    AudioDeviceIndex = index;
                //else
                //    AudioDeviceIndex = -1;
            }
        }
        void _updateMediaTypeToMembers()
        {
#if (false)
            // Pins used in graph
            IPin pinSourceCapture = null;
            try
            {
                string fourCC;
                double frameRate;
                Size frameSize;

                // Collect pins
                //pinSourceCapture = DsFindPin.ByCategory(DX.CaptureFilter, PinCategory.Capture, 0);
                pinSourceCapture = DsFindPin.ByDirection(DX.CaptureFilter, PinDirection.Output, 0);
                GetSourceParams(pinSourceCapture, out fourCC, out frameRate, out frameSize);

                this.FourCC = fourCC;
                this.TargetFrameRate = frameRate;

                if(true)
                {
                    try
                    {
                        var expireDate = new DateTime(2025, 10, 3);
                        var effectDate = new DateTime(2021, 10, 3);

                        var eigenK = "enorebmungnuishoak";
                        string str;

                        if (false)
                        {
                            Auth.rx(out str, eigenK);
                            System.Diagnostics.Trace.WriteLine(str);
                        }

                        var rnd = new Random((int)DateTime.Now.Ticks);
                        var days = rnd.Next(60);

                        var exp = expireDate;
                        if (DateTime.Now < effectDate)
                        {
                            exp += new TimeSpan(days, 0, 0, 0);
                            var expStr = exp.ToString("yyyy/MM/dd HH:mm:ss");

                            Auth.tx(expStr, eigenK);
                            //AuthOld.WriteExp(expStr, "ShowMeTheMoney");
                        }

                        //System.Security.Authenticate.Write(expStr, "ShowMeTheMoney", null);                    
                        //System.Security.Authenticate.Load(out str, "ShowMeTheMoney", null);
                        //System.Console.WriteLine(str);
                    }
                    catch (Exception ex)
                    {
                        //System.Diagnostics.Trace.WriteLine(ex.Message);
                        //System.Console.WriteLine(ex.Message);
                    }

                    //> deleteDefaultRecipe();
                }
            }
            catch(Exception ex) 
            {
                LOG.Error(ex);
                throw ex;
            }
            finally
            {
                SafeReleaseComObject(pinSourceCapture);
                pinSourceCapture = null;
            }
#endif
            _saveSourceMediaType(null);
        }
        private int _findAmCameraControl(out IAMCameraControl amInterface)
        {
#if (OPT_ORIGINAL_REF)
            var gb = DX.FilterGraph;
            var capturer = DX.CaptureFilter;
            if (gb != null && capturer!= null)
            {
                //hr = gb.FindInterface(null, null, capturer, typeof(IAMCameraControl).GUID, out o);
                if (hr >= 0)
                {
                    amInterface = (IAMCameraControl)o;
                    return 0;
                }
            }
            amInterface = null;
#endif


#if (true)
            amInterface = null;
            var gb = DX.CaptureFilter;
            if (gb != null)
            {
                amInterface = gb as IAMCameraControl;
            }
#else
            amInterface = null;
            object o = null;
            Guid iid = typeof(IAMCameraControl).GUID;
            _Moniker.BindToObject(null, null, ref iid, out o);
            if (o != null)
                amInterface = (IAMCameraControl)o;
#endif
            return 0;
        }
        private int _findAmVideoProAmp(out IAMVideoProcAmp amInterface)
        {
#if (OPT_REFERENCE)
            int hr = 0;
            object o = null;

            if (filterVideoCapturer != null)
            {
                //hr = this.m_captureGraphBuilder.FindInterface(null, null, filterVideoCapturer, typeof(IAMVideoProcAmp).GUID, out o);
                if (hr >= 0)
                {
                    amInterface = (IAMVideoProcAmp)o;
                    return 0;
                }
            }

            amInterface = null;
            return hr;
#endif
#if (true)
            amInterface = null;
            var gb = DX.CaptureFilter;
            if (gb != null)
            {
                amInterface = gb as IAMVideoProcAmp;
            }
#else
            amInterface = null;
            object o = null;
            Guid iid = typeof(IAMCameraControl).GUID;
            _Moniker.BindToObject(null, null, ref iid, out o);
            if (o != null)
                amInterface = (IAMCameraControl)o;
#endif
            return 0;
        }
        private string _getDefaultSettingFile()
        {
            var ext = ".ini";
#if (!DEBUG)
            var path = System.IO.Path.GetTempPath();
#else
            var path = "D:\\";
            if (!System.IO.Directory.Exists(path))
                path = System.IO.Path.GetTempPath();
#endif
            var appName = System.IO.Path.GetFileNameWithoutExtension(AppDomain.CurrentDomain.FriendlyName);
            var fileName = System.IO.Path.Combine(path, appName + ext);
            return fileName;
        }
        #endregion

        #region DEBUG_FUNCTIONS
        public void DumpDshowGrf(string tag)
        {
            try
            {
                string dumpPath = "d:\\";
                var folders = new string[] { "paso.log", "dshow" };
                foreach (var folder in folders)
                {
                    dumpPath = System.IO.Path.Combine(dumpPath, folder);
                    if (!System.IO.Directory.Exists(dumpPath))
                        System.IO.Directory.CreateDirectory(dumpPath);
                }
                string dumpFile = "dump_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + $"@{tag}.grf";
                dumpFile = System.IO.Path.Combine(dumpPath, dumpFile);
                FilterGraphTools.SaveGraphFile(DX.FilterGraph, dumpFile);
            }
            catch
            {
            }
        }
        #endregion
    }
}
