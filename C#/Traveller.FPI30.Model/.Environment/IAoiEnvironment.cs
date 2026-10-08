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

namespace LaserAlignDX.AoiModel
{
    public interface IAoiEnvironment
    {
        int ThreadCount { get; }
        bool ThreadsEnabled { get; }

        bool IsForceInspect { get; }
        bool IsSaveTestImage { get; }
        bool IsSaveDebugBmp { get; }
        bool IsSaveDebugOrgBmp { get; }
        bool UseOkNgDiffImageFolders { get; }
        int ImageQuality { get; }

        float Cal_Bcx { get; }
        float Cal_Bcy { get; }
        float Cal_Bca { get; }
    }
}
