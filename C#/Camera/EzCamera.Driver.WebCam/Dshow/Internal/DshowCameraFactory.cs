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


using Camera_NET;
using DirectShowLib;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DsCamera = Camera_NET.Camera;


namespace JetEazy.DShow
{
    public class DshowCameraFactory
    {
        #region PRIVATE_DATA
        //Control _wndDisplayHost;
        //Camera _dsCamera;
        #endregion

        #region SINGLETON
        static DshowCameraFactory _instance;
        protected DshowCameraFactory()
        {
        }
        #endregion

        public static DshowCameraFactory Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new DshowCameraFactory();
                return _instance;
            }
        }

        public List<string> GetAvailableAudioDevices()
        {
            return dsGetAvailableDevices("audio");
        }
        public List<string> GetAvailableCameras()
        {
            return dsGetAvailableDevices("video");
        }
        public List<Size> GetAvailableFrameSizes(DsCamera dsCamera)
        {
            var results = new List<Size>();
            var mon = dsCamera?.Moniker;
            if (mon != null)
            {
                //var resolutions = CameraControl.GetResolutionList(mon);
                var resolutions = DsCamera.GetResolutionList(mon);
                if (resolutions != null)
                {
                    foreach (var res in resolutions)
                    {
                        results.Add(new Size(res.Width, res.Height));
                    }
                }
            }
            return results;
        }

        public DsCamera OpenCamera(Control wndDisplayHost, string deviceName = null, string camIniFileName = null, Size? targetFrameSize = null)
        {
            // 0. Normalize dshowCamIndex
            if (string.IsNullOrEmpty(camIniFileName))
                camIniFileName = LibPaths.DEFAULT_INI_FILE;
            if (string.IsNullOrEmpty(deviceName))
                _loadActiveDsCameraInfo(camIniFileName, out deviceName);

            // 1. Resolution
            var res = (targetFrameSize != null) ?
                new Resolution(targetFrameSize.Value.Width, targetFrameSize.Value.Height) :
                null;

            // 2. camIniFileName
            camIniFileName = _getDsCamIniFile(camIniFileName, deviceName, check: true);

            // 3. creation
            var dsCamera = dsCreateCamera(wndDisplayHost, deviceName, camIniFileName, res);

            //>>> CAUTION: 不要釋放 moniker
            //>>> Marshal.ReleaseComObject(moniker);
            return dsCamera;
        }
        public DsCamera OpenCamera(Control wndDisplayHost, int dshowCamIndex, string camIniFileName = null, Size? targetFrameSize = null)
        {
            string deviceName = null;
            if (dshowCamIndex >= 0)
            {
                var names = GetAvailableCameras();
                if (dshowCamIndex < names.Count)
                    deviceName = names[dshowCamIndex];
            }
            return OpenCamera(wndDisplayHost, deviceName, camIniFileName, targetFrameSize);


            //// 0. Normalize dshowCamIndex
            //if (dshowCamIndex < 0)
            //    _loadActiveDsCamIndex(camIniFileName, out dshowCamIndex);

            //// 1. Resolution
            //var res = (targetFrameSize != null) ?
            //    new Resolution(targetFrameSize.Value.Width, targetFrameSize.Value.Height) :
            //    null;

            //// 2. camIniFileName
            //camIniFileName = _getDsCamIniFile(camIniFileName, dshowCamIndex, check: true);

            //// 3. creation
            //var dsCamera = dsCreateCamera(wndDisplayHost, dshowCamIndex, camIniFileName, res);

            ////>>> CAUTION: 不要釋放 moniker
            ////>>> Marshal.ReleaseComObject(moniker);
            //return dsCamera;
            return null;
        }
        public void SaveAllSettings(string iniFileName, DsCamera dsCamera)
        {
            if (dsCamera == null)
                return;
            
            saveAllSettings(iniFileName, dsCamera);

            var iniFileName2 = LibPaths.DEFAULT_INI_FILE;
            if (iniFileName == null || string.Compare(iniFileName, iniFileName2, true) != 0)
                saveAllSettings(iniFileName2, dsCamera);
        }
        void saveAllSettings(string iniFileName, DsCamera dsCamera)
        {
            // 0. Empty Conditions
            if (string.IsNullOrEmpty(iniFileName) || dsCamera == null)
                return;

            // 1. Path
            string path = System.IO.Path.GetDirectoryName(iniFileName);
            JetEazy.IO.QxPathUtility.InitDirectory(path);

            // 2. Activate Cam Name
            var deviceName = dsCamera?.VideoDeviceName;
            if (!string.IsNullOrEmpty(deviceName))
                _saveActiveDsCameraInfo(iniFileName, deviceName);

            // 3. Individual Camera Graph
            string camIniFileName = _getDsCamIniFile(iniFileName, deviceName, check: false);
            dsCamera.SaveGraph(camIniFileName);
        }

        #region PRIVATE_INI_FUNCTIONS
        void _loadActiveDsCameraInfo(string iniFileName, out string deviceName)
        {
            deviceName = "";
            iniFileName = _getDsCamIniFile(iniFileName, null, check: true);
            if (!string.IsNullOrEmpty(iniFileName))
                JetEazy.Win32.Win32Ini.Load(ref deviceName, iniFileName, "DsShowCameraCtrl", "ActiveCamName");

            var names = GetAvailableCameras();
            int index = names.IndexOf(deviceName);
            if (index < 0 && names.Count > 0)
            {
                index = 0;
                deviceName = names[index];
            }
        }
        void _saveActiveDsCameraInfo(string iniFileName, string deviceName)
        {
            iniFileName = _getDsCamIniFile(iniFileName, null, check: false);
            if (string.IsNullOrEmpty(iniFileName))
                return;
            JetEazy.Win32.Win32Ini.Save(deviceName, iniFileName, "DsShowCameraCtrl", "ActiveCamName");
        }
        string _getDsCamIniFile(string iniFileName, string deviceName, bool check = false)
        {
            if (string.IsNullOrEmpty(iniFileName))
                iniFileName = LibPaths.DEFAULT_INI_FILE;

            var path = System.IO.Path.GetDirectoryName(iniFileName);
            var ext = System.IO.Path.GetExtension(iniFileName);
            var stem = System.IO.Path.GetFileNameWithoutExtension(iniFileName);
            var strs = stem.Split('@');
            stem = strs[0].Trim();

            if (!string.IsNullOrEmpty(deviceName))
            {
                stem = $"{stem}@{deviceName}";
                stem = _removeInvalidFileNameChars(stem);
            }

            iniFileName = System.IO.Path.Combine(path, stem + ext);

            if (check && !System.IO.File.Exists(iniFileName))
                return null;

            return iniFileName;
        }
        string _removeInvalidFileNameChars(string fname)
        {
            // Get an array of invalid characters for file names
            char[] invalidChars = System.IO.Path.GetInvalidFileNameChars();

            // Use LINQ to filter out the invalid characters
            string cleanedStr = new string(fname.Where(c => !invalidChars.Contains(c)).ToArray());

            return cleanedStr;
        }
        #endregion

        #region PRIVATE_DSHOW_FUNCTIONS

        /// <summary>
        /// 枚舉可用的 Media 輸入裝置
        /// </summary>
        /// <param name="category">vidoe 或 audio</param>
        /// <returns></returns>
        List<string> dsGetAvailableDevices(string category)
        {
            var results = new List<string>();
            using (var dsChoice = new DsDeviceChoice(category))
            {
                foreach (var name in dsChoice.IterUniqueDeviceNames())
                {
                    results.Add(name);
                }
            }
            return results;
        }

        DsCamera dsCreateCamera(Control wndHost, string deviceName, string iniFileName = null, Resolution resolution = null)
        {
            // Retrieve Video Device by Index
            DsDevice device = null;
            int dshowCamIndex = -1;

            using (var choice = new DsVideoChoice())
            {
                device = choice.RetrieveDeviceByName(deviceName, out dshowCamIndex);
            }

            if (device == null)
                return null;

            // Create camera object
            var dsCamera = new DsCamera();
            //>>> dsCamera.CamID = dshowCamIndex;

            // DShow LOG File
            if (false)
            {
                string logFile = "d:\\paso.log\\dshow\\dshow.log";
                dsCamera.DirectShowLogFilepath = logFile;
            }

            // 檢查比對 可用的 resolution
            if (false)
            {
                var moniker = device.Mon;   // DsCamera.GetDeviceMoniker(dshowCamIndex);
                var availableResolutions = DsCamera.GetResolutionList(moniker);
                if (resolution != null)
                {
                    resolution = null;
                    foreach (var res in availableResolutions)
                    {
                        if (res.Width == resolution.Width && res.Height == resolution.Height)
                        {
                            resolution = res;
                            break;
                        }
                    }
                }
            }

            if (resolution != null)
            {
                dsCamera.Resolution = resolution;
            }

            // Init
            //>>> CAUTION: 不要釋放 device 與 moniker
            //>>> Marshal.ReleaseComObject(moniker);
            //>>> dsCamera.Initialize(wndHost, moniker);
            dsCamera.Init(wndHost, device, deviceName, dshowCamIndex);

            // Build from iniFileName
            bool isBuiltOk = false;
            try
            {
                iniFileName = _getDsCamIniFile(iniFileName, deviceName, check: true);
                if (!string.IsNullOrEmpty(iniFileName))
                {
                    dsCamera.BuildGraph(false, iniFileName);
                    isBuiltOk = true;
                }
            }
            catch
            {
            }

            // Build Default 
            if (!isBuiltOk)
            {
                dsCamera.BuildGraph();
            }

            //_dsCamera.RunGraph();
            dsCamera.StartLiveMode();

            // Event
            //_dsCamera.OutputVideoSizeChanged += Camera_OutputVideoSizeChanged;
            return dsCamera;
        }

        #endregion
    }
}
