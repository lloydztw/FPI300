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


namespace EzCamera.Interface
{
    public interface IEzCameraFactory
    {
        /// <summary>
        /// 巡歷所有可用的相機設備資訊
        /// </summary>
        IEzDeviceInfo[] GetAvailableCameraInfos();

        /// <summary>
        /// 巡歷所有可用的相機設備資訊.
        /// 未來優化: 用來取代 GetAvailableCameraInfos 以節省內存
        /// </summary>
        //>>> IEnumerable<IEzDeviceInfo> IterateAvailbleDevices();

        /// <summary>
        /// 使用 IEzDeviceInfo 生成相機
        /// </summary>
        IEzCamera LoadCamera(IEzDeviceInfo info);

        /// <summary>
        /// 使用 camID 生成相機
        /// </summary>
        IEzCamera LoadCamera(int camID);
    }
}
