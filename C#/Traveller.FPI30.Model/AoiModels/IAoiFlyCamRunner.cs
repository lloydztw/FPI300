#region AUTHOR
/*
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 * REVISION:
 *      2026-10-08 使用 ChatGPT Codex  重整 (by LeTian Chang)
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 */
#endregion

using System.Drawing;

namespace LaserAlignDX.AoiModel
{
    /// <summary>Runs one fly image. The caller owns the image, including Result.MetaData.bmpFly.
    /// Display/save consumers must copy it before the caller disposes it. The AOI owner owns this service.</summary>
    public interface IAoiFlyCamRunner
    {
        FlyAoiResult Run(FlyID flyId, Bitmap image, FlyAoiRunOptions options);
    }
}
