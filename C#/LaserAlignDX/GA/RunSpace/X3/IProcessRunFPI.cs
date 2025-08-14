using System;
using VisionDesigner;

namespace LaserAlignDX.RunSpace
{
    public interface IProcessRunFPI : IDisposable
    {
        event EventHandler<GaProgressEventArgs> OnAoiBegin;
        event EventHandler<GaProgressEventArgs> OnAoiEnd;
        event EventHandler<GaProgressEventArgs> OnAoiProgressing;

        ScanInspectMode xScanInspectMode { get; set; }

        bool IsPass { get; }
        long ElapsedTime { get; }
        string FileBarcodeStr { get; set; }
        string LotId { get; set; }
        bool QrJudged { get; set; }
        bool QrUsed { get; set; }
        string ResultDesc { get; }
        bool Running { get; }
        string StripId { get; set; }

        int[] GetQrResult();
        float[] GetScanOffset();
        int[] GetSingleResult();

        /// <summary>
        /// 這種設計很不好
        /// </summary>
        CMvdImage cMvdInput { get; set; }

        void Run();
    }
}