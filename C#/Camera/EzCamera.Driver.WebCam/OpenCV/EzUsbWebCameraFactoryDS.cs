using Camera_NET;
using DirectShowLib;
using EzCamera.Interface;
using System.Collections.Generic;
using CAMERA_CLASS = EzCamera.Driver.WebCam.CV.EzUsbWebCamera;


namespace EzCamera.Driver.WebCam.CV
{
    /// <summary>
    /// 使用 DirectShowLib 來列舉 所有 USB Camera
    /// </summary>
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
            var camera = new CAMERA_CLASS(info);
            return camera;
        }
        public IEzCamera LoadCamera(int camID)
        {
            fetch_all_devices();
            if (camID < _infos.Length)
            {
                var info = _infos[camID];
                var camera = new CAMERA_CLASS(info);
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

            // 列舉視訊輸入設備
            using (var choice = new DsVideoChoice())
            {
                int index = 0;
                foreach (EzDevice ezDevice in choice.IterDevices())
                {
                    DsDevice device = ezDevice;
                    var info = CAMERA_CLASS.DefaultDeviceInfo(index);
                    info.Model = "USB";
                    //info.Model = "DShow";
                    info.VendorName = ezDevice.UniqueName;
                    info.Tag = device.ClassID;
                    info.Index = index;
                    infosList.Add(info);
                    index++;
                }
            }

            return _infos = infosList.ToArray();
        }
        #endregion
    }
}
