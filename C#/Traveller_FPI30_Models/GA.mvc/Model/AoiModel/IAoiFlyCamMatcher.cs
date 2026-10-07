#region AUTHOR
/*
 * 
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2026-08-10 把 Aoi Models 從 Recipe 分離 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using VisionDesigner;
using VisionDesigner.BlobFind;

namespace LaserAlignDX.AoiModel
{
    public interface IAoiFlyCamMatcher : IDisposable
    {
        bool Train(Bitmap goldenImage, params object[] args);

        void Run(Bitmap sceneBmp);

        List<CMvdRectangleF> MatchResultRects { get; }

        float MatchResultAngle { get; } 

        bool CheckSpecialAngle(Bitmap bmpScene, out List<CBlobInfo> retBlobs, out float retAngle, out PointF retCenter);
    }
}