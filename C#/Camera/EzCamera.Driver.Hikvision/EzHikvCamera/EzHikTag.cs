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


using MvCamCtrl.NET;
using System;

namespace EzCamera.Driver.Hikvision
{
    /// <summary>
    /// 海康 擴增 EzCameraDeviceInfo  
    /// </summary>
    public class EzHikTag
    {
        public MyCamera.MV_CC_DEVICE_INFO mvccDeviceInfo;
        public string SerialNumber;
        public string CfgPath;
    }
}
