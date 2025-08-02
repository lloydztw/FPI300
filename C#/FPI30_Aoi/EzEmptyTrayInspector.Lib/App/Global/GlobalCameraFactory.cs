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
//using EzCamera.Driver.DVP;
//using EzCamera.Driver.Hikvision;
//using EzCamera.Driver.Sim;
//using EzCamera.Driver.WebCam;
using EzCamera.Interface;
using System.Collections.Generic;

namespace EzDualMatch
{
    /// <summary>
    /// 將所有會用到 各廠家的 CameraFactory 納入於此 Class 提供統一的生成接口. 
    /// 實際應用可以搭配 ini 等檔案, 加入更多初始化設定. <br/>
    ///【建議】不要使用 Singleton! 
    /// 查詢與生成camera之後, 內部的 _infos, _dict 才會自動釋放.
    /// </summary>
    public class GlobalCameraFactory : IEzCameraFactory
    {
        // 目前 EzDualMatch 暫時用不到 Camera

        IEzCameraFactory[] _factories = new IEzCameraFactory[]{
            //new EpixCameraFactor(),
            //new EzDvpCameraFactory(),
            //new EzHikvCameraFactory(),
            //new EzUsbWebCameraFactory(),    
            //new EzSimCameraFactory(3),
        };

        #region PRIVATE_CACHE_DATA
        /// <summary>
        /// Class: 擴增 IEzDeviceInfo 以方便查表生成對應的 IEzCamera
        /// </summary>
        class DeviceInfoEx
        {
            #region DATA
            public int globalID;
            public IEzDeviceInfo info;
            public IEzCameraFactory factory;
            #endregion

            public DeviceInfoEx(int globalID, IEzDeviceInfo info, IEzCameraFactory factory)
            {
                this.globalID = globalID;
                this.info = info;
                this.factory = factory;
            }
            public IEzCamera LoadCamera()
            {
                var cam = (EzAbstractCamera)factory.LoadCamera(info);
                cam.GlobalCamID = globalID;
                return cam;
            }
        }
        /// <summary>
        /// 對應表 
        /// </summary>
        Dictionary<IEzDeviceInfo, DeviceInfoEx> _dict;
        /// <summary>
        /// 所有的 IEzDeviceInfo (以 GlobalCamID 排序)
        /// </summary>
        List<IEzDeviceInfo> _infos;
        #endregion

        public IEzDeviceInfo[] GetAvailableCameraInfos()
        {
            fetch_all_devices();
            return _infos.ToArray();
        }
        public IEzCamera LoadCamera(IEzDeviceInfo info)
        {
            if (info == null)
                return null;

            fetch_all_devices();
            if (_dict.TryGetValue(info, out DeviceInfoEx context))
                return context.LoadCamera();

            return null;
        }
        public IEzCamera LoadCamera(int globalCamID)
        {
            fetch_all_devices();

            if (globalCamID < _infos.Count)
            {
                var info = _infos[globalCamID];
                return LoadCamera(info);
            }

            return null;
        }
        public virtual IEzCamera[] LoadCameras(string iniFileName)
        {
            // 保留
            throw new System.NotImplementedException();
        }

        #region PRIVATE_FUNCTIONS
        void fetch_all_devices(bool reset = false)
        {
            if (_infos != null && _dict != null && !reset)
                return;

            _dict = new Dictionary<IEzDeviceInfo, DeviceInfoEx>();
            _infos = new List<IEzDeviceInfo>();

            foreach (var factory in _factories)
                append_devices(factory);
        }
        void append_devices(IEzCameraFactory camFactory)
        {
            if (camFactory != null)
            {
                var infos = camFactory.GetAvailableCameraInfos();
                foreach (var info in infos)
                {
                    var globalID = _infos.Count;
                    var infoEx = new DeviceInfoEx(globalID, info, camFactory);
                    _dict.Add(info, infoEx);
                    _infos.Add(info);
                }
            }
        }
        #endregion
    }
}
