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

using System;


namespace LaserAlignDX.AoiModel
{
    /// <summary>
    /// 進度條事件
    /// </summary>
    public class GaProgressEventArgs : EventArgs
    {
        public int TotalSteps { get; internal set; }
        public int CurrentStep { get; internal set; }
        public string Message { get; internal set; }

        public GaProgressEventArgs(int totalSteps, int currentStep, string message = null)
        {
            TotalSteps = totalSteps;
            CurrentStep = currentStep;
            Message = message;
        }
    }
}
