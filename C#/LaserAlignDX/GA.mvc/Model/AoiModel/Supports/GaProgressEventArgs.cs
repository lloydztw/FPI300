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
