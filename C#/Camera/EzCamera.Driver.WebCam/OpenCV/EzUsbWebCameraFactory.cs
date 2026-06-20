using EzCamera.Interface;
using System.Collections.Generic;
using System.Management;

namespace EzCamera.Driver.WebCam.OLD
{
    public class EzUsbWebCameraFactory : IEzCameraFactory
    {
        #region PRIVATE_DATA
        IEzDeviceInfo[] _infos = null;
        #endregion

        public IEzDeviceInfo[] GetAvailableCameraInfos()
        {
            fetch_all_devices();
            return _infos;
        }
        public IEzCamera LoadCamera(IEzDeviceInfo info)
        {
            var camera = new EzUsbWebCamera(info);
            return camera;
        }
        public IEzCamera LoadCamera(int camID)
        {
            fetch_all_devices();
            if (camID < _infos.Length)
            {
                var info = _infos[camID];
                var camera = new EzUsbWebCamera(info);
                return camera;
            }
            throw new System.Exception($"camID={camID} 超出範圍({_infos.Length})!");
            return null;
        }

        #region PRIVATE_FUNCTIONS
        IEzDeviceInfo[] fetch_all_devices(bool reset = false)
        {
            if (_infos != null && !reset)
                return _infos;

            var infosList = new List<IEzDeviceInfo>();
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE (PNPClass = 'Image' OR PNPClass = 'Camera')"))
            {
                int openCvIndex = 0;
                foreach (var device in searcher.Get())
                {
                    string sName = device["Caption"].ToString();
                    string sStatus = device["Status"].ToString();
                    string sDeviceId = device["DeviceId"].ToString();
                    var info = EzUsbWebCamera.DefaultDeviceInfo(openCvIndex);
                    info.VendorName = sName;
                    info.Model = "USB";
                    info.Tag = sDeviceId;
                    info.Index = openCvIndex++;
                    infosList.Add(info);
                }
            }
            return _infos = infosList.ToArray();
        }
        #endregion
    }
}
