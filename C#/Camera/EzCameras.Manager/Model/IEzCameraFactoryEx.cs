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

using EzCamera.Interface;


namespace EzCamera.Manager
{
    public interface IEzCameraFactoryEx : IEzCameraFactory
    {
        IEzDeviceInfo GetDeviceInfo(int globalCamID);
        IEzDeviceInfo GetDeviceInfo(string infoStr);
        IEzCamera LoadCamera(string infoStr);
    }
}
