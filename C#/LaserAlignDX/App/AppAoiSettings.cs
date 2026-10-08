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

using LaserAlignDX.AoiModel;
using Traveller106;

namespace LaserAlignDX
{
    internal sealed class AppAoiSettings : IAoiEnvironment
    {
        public bool IsForceInspect => INI.Instance.IsForceInspect;
        public bool IsSaveTestImage => INI.Instance.IsSaveTestImage;
        public bool IsSaveDebugBmp => INI.Instance.IsSaveDebugBmp;
        public bool IsSaveDebugOrgBmp => INI.Instance.IsSaveDebugOrgBmp;
        public bool UseOkNgDiffImageFolders => INI.Instance.UseOkNgDiffImageFolders;
        public int ImageQuality => INI.Instance.ImageQuality;
        public int ThreadCount => Universal.N_THREADS;
        public bool ThreadsEnabled => Universal.N_THREADS_ENABLED;
        public float Cal_Bcx => INI.Instance.Cal_Bcx;
        public float Cal_Bcy => INI.Instance.Cal_Bcy;
        public float Cal_Bca => INI.Instance.Cal_Bca;
    }
}
