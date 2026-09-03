#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-09-22 重整 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System;
using System.Drawing;


namespace LaserAlignDX.AoiModel
{
    public interface IFlyAoiModel : IDisposable
    {
        bool Train(object recipe);
        FlyAoiResult RunAoiOne(FlyID flyID, Bitmap bmpFly);
    }
}
