#region AUTHOR
/*
 * 
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2026-06-21 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System;

namespace JetEazy.Drivers
{
    public interface ILightDriver : IDisposable
    {
        int Channel { get; set; }
        int Intensity { get; set; }

        /// <summary>
        /// 频闪控制器触发指令
        /// </summary>
        void Trigger();
    }
}
