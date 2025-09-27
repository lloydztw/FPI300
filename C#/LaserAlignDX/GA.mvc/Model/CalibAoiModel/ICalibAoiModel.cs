#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-09-07 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzAoiEmptyTrayInspector.Model;
using OpenCvSharp;


namespace LaserAlignDX.AoiModel
{
    public interface ICalibAoiModel : IxEmptyTrayInspector
    {
        void RefineCentroidLocations(MatchResult matchResult, Mat fullfovImg);
    }
}