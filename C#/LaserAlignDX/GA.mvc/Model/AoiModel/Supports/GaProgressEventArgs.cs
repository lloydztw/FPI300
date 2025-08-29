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

        public GaProgressEventArgs(int totalSteps, int currentStep)
        {
            TotalSteps = totalSteps;
            CurrentStep = currentStep;
        }
    }
}
