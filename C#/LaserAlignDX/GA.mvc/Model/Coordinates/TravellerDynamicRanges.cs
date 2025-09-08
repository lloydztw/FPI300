#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-08-13 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion


namespace LaserAlignDX.Model.Coords
{
    public static class TravellerDynamicRanges
    {
        public static DynamicRange[] CameraRanges = new[]
        {
            new DynamicRange(0,16000),
            new DynamicRange(0,38000)
        };

        public static DynamicRange[] MotorRanges = new[]
        {
            new DynamicRange(-1000,0),
            new DynamicRange(-1000,0)
        };
    }
}
