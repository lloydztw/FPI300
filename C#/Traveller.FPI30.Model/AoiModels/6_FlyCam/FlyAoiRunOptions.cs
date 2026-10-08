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
    /// <summary>Per-image inputs. The host selects the stage/position compensation;
    /// matcher training and thresholds still use the existing recipe.</summary>
    public sealed class FlyAoiRunOptions
    {
        public RectangleF TemplateRegion { get; set; }
        public float ExtendX { get; set; }
        public float ExtendY { get; set; }
        public bool UseSpecialAngle { get; set; }
        public bool DecodeCode { get; set; }
        public float ImageResolution { get; set; }
        public PointF PositionOffset { get; set; }
    }
}
