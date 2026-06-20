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


using System;

namespace EzCamera.Interface
{
    /// <summary>
    /// 相機設備簡要資訊
    /// </summary>
    public interface IEzDeviceInfo : ICloneable
    {
        int Index { get; set; }
        string VendorID { get; set; }
        string VendorName { get; set; }
        string Model { get; set; }
        object Tag { get; set; }
    }
}
