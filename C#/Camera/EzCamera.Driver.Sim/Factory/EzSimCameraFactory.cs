using EzCamera.Interface;
using System;

namespace EzCamera.Driver.Sim
{
    // using EzCamera = EzSimCameraExTrigger;
    using CameraT = EzSimCamera;

    public class EzSimCameraFactory : IEzCameraFactory
    {
        #region PRIVATE_DATA
        int _simNumber;
        #endregion

        public EzSimCameraFactory(int simCamerasNumber = 0)
        {
            if (simCamerasNumber > 0)
                _simNumber = simCamerasNumber;
            else
                _simNumber = 1;
        }
        public IEzDeviceInfo[] GetAvailableCameraInfos()
        {
            var infos = new IEzDeviceInfo[_simNumber];
            for (int i = 0; i < _simNumber; i++)
                infos[i] = CameraT.DefaultDeviceInfo(i);
            return infos;
        }
        public IEzCamera LoadCamera(IEzDeviceInfo info)
        {
            // Simulation Camera 可以任意數量生成
            var camera = new CameraT(info);
            return camera;
        }
        public IEzCamera LoadCamera(int camID)
        {
            // Simulation Camera 可以任意數量生成
            _simNumber = Math.Max(_simNumber, camID + 1);
            var info = CameraT.DefaultDeviceInfo(camID);
            var camera = LoadCamera(info);
            return camera;
        }

        #region PRIVATE_FUNCTIONS
        #endregion
    }
}
