#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-08-29 重整 Gaara 原來的 MvdFindLineClass (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System;
using System.Drawing;
using VisionDesigner;

namespace LaserAlignDX.BasicSpace
{
    public interface IMvdLineFinder : IDisposable, IMvdLineFinderParams
    {
        /// <summary>
        /// 是否為黑色載盤背景
        /// </summary>
        EdgeBackGroundType Background { get; set; }

        CMvdLineSegmentF Run(Bitmap bmpInput, RectangleF roi, int borderId = -1);

        CMvdLineSegmentF Run(Bitmap bmpInput, CMvdRectangleF roi, int borderId = -1);
    }

    public interface IMvdLineFinderParams
    {
        /// <summary>
        /// 寻找方向 左右型 true从左到右 上下型 true从上到下
        /// </summary>
        bool bPositive { get; set; }
        /// <summary>
        /// 搜寻方向 true左右型 false上下型
        /// </summary>
        bool bFindOrient { get; set; }
        /// <summary>
        /// 边缘极性 true白到黑 false黑到白
        /// </summary>
        bool bEdgePolarity { get; set; }
        /// <summary>
        /// 卡尺数量
        /// </summary>
        int iRayNum { get; set; }
        /// <summary>
        /// 边缘强度
        /// </summary>
        int iEdgeStrength { get; set; }
    }
}
