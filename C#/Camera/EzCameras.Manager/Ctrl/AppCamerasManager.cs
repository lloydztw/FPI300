#region AUTHOR
/*
 * LeTian.JxProps
 * Copyright (C) 2025
 * 2025-03-10 created by LeTian Chang
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzCamera.Driver;
using EzCamera.GUI;
using EzCamera.Interface;
using System;
using System.Windows.Forms;


namespace EzCamera.Manager
{
    public partial class AppCamerasManager
    {
        #region NLOG
        // NOTE:
        // 不要做靜態初始化，而是要等到窗體的Load事件時才初始化Logger物件，
        // 且保證該窗體是 【首個使用】NLog 的 Class !!!
        // 這是因為NLog是在首次被使用時，才載入配置文件的。
        static NLog.ILogger _logger = null;
        NLog.ILogger _LOG
        {
            get
            {
                if (_logger == null)
                    _logger = NLog.LogManager.GetCurrentClassLogger();
                return _logger;
            }
        }
        #endregion

        #region SINGLETON
        static AppCamerasManager _singleton;
        protected AppCamerasManager()
        {
        }
        #endregion

        public static AppCamerasManager Instance
        {
            get => _singleton != null ? _singleton : _singleton = new AppCamerasManager();
        }

        /// <summary>
        /// 結束所有相機生命週期
        /// </summary>
        public static void DisposeAll()
        {
            AllCreatedCameras.DisposeAll();
            _singleton = null;
        }

        public string IniPath
        {
            get
            {
                return AppCameraFactory.IniPath;
            }
            set
            {
                AppCameraFactory.IniPath = value;
            }
        }

        /// <summary>
        /// 已加載之相機數量
        /// </summary>
        public int TotalNumber
        {
            get => AllCreatedCameras.TotalCount;
        }

        /// <summary>
        /// 所有已加載之相機
        /// </summary>
        public IEzCamera[] Cameras
        {
            get => AllCreatedCameras.ToArray();
        }

        /// <summary>
        /// 加載相機
        /// (globalCamID == -1 代表 加載 最後一次的相機)
        /// </summary>
        public IEzCamera LoadCamera(int globalCamID = -1)
        {
            IEzCameraFactory factory = CameraFactory;
            try
            {
                var camera = factory.LoadCamera(globalCamID);
                if (camera == null)
                    camera = BrowseCameraConfig();
                return camera;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, GetType().Name, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            return null;
        }

        public IEzCamera BrowseCameraConfig()
        {
            IEzCameraFactory factory = CameraFactory;
            try
            {
                using (var dlg = new FormCameraConfig())
                {
                    if (DialogResult.OK == dlg.ShowDialog())
                    {
                        var deviceInfo = dlg.SelectedDeviceInfo;
                        var camera = factory.LoadCamera(deviceInfo);
                        return camera;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, GetType().Name, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            return null;
        }

        /// <summary>
        /// 預先加載
        /// </summary>
        public void Build(int totalCamerasNumber = 1)
        {
            totalCamerasNumber = Math.Max(totalCamerasNumber, 1);

            var factory = CameraFactory;

            // 準備好足夠的 DeviceInfo
            while (true)
            {
                var devInfos = factory.GetAvailableCameraInfos();
                if (devInfos.Length >= totalCamerasNumber)
                    break;
                else
                    openConfigWindow();
            }

            for (int gid = 0; gid < totalCamerasNumber; gid++)
            {
                _LOG.Info($"CamMgr 加載相機 {EzDispText.Format(gid)} ...");

                var cam = factory.LoadCamera(gid);
                cam?.Init();

                if (cam != null)
                    _LOG.Info($"CamMgr 加載相機 {EzDispText.Format(cam)} [OK]");
                else
                    _LOG.Warn($"CamMgr 加載相機 {EzDispText.Format(gid)} [失敗]");
            }
        }

        #region PRIVATE_FUNCTIONS
        void openConfigWindow(object arg = null)
        {
            using (var dlg = new FormCameraConfig(arg))
            {
                dlg.ShowDialog();
            }
        }
        #endregion
    }


    partial class AppCamerasManager
    {
        /// <summary>
        /// AppCameraFactory
        /// </summary>
        public static IEzCameraFactoryEx CameraFactory
        {
            get => AppCameraFactory.Instance;
        }
        
        public void SaveIni(string iniFileName = null)
        {
            var factory = CameraFactory as AppCameraFactory;
            factory.SaveIni(iniFileName);
        }
        public void LoadIni(string iniFileName = null)
        {
            var factory = CameraFactory as AppCameraFactory;
            factory.LoadIni(iniFileName);
        }
        public DateTime GetLastModifiedTime()
        {
            var factory = CameraFactory as AppCameraFactory;
            var iniFileName = factory.getIniFileName();
            if (System.IO.File.Exists(iniFileName))
            {
                var fileInfo = new System.IO.FileInfo(iniFileName);
                return fileInfo.LastWriteTime;
            }
            else
            {
                return DateTime.MinValue;
            }
        }

        #region JX_RESERVED
#if (OPT_USE_JX)
        /// <summary>
        /// 資料交換
        /// </summary>
        public void WriteToJX(JxCamerasConfig jx)
        {
            var factory = CameraFactory as AppCameraFactory;
            factory.ToJX(jx);
        }
#else
        public void WriteToJX(object arg)
        {
        }
#endif
        #endregion

        #region IMPLEMENTS_IEzCameraFactoryEx
#if OPT_RESERVED
        IEzDeviceInfo[] IEzCameraFactory.GetAvailableCameraInfos()
        {
            return Factory.GetAvailableCameraInfos();
        }
        IEzCamera IEzCameraFactory.LoadCamera(IEzDeviceInfo info)
        {
            return Factory.LoadCamera(info);
        }
        IEzCamera IEzCameraFactory.LoadCamera(int camID)
        {
            return Factory.LoadCamera(camID);
        }
        IEzDeviceInfo IEzCameraFactoryEx.GetDeviceInfo(string infoStr)
        {
            return Factory.GetDeviceInfo(infoStr);
        }
        IEzCamera IEzCameraFactoryEx.LoadCamera(string infoStr)
        {
            return Factory.LoadCamera(infoStr);
        }
#endif
        #endregion
    }
}
