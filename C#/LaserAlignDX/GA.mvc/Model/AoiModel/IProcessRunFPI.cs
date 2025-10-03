using LeTian.AoiLib;
using System;
using System.Drawing;
using ProcessEventArgs = NeedleX.ProcessSpace.ProcessEventArgs;


namespace LaserAlignDX.AoiModel
{
    public interface IProcessRunFPI : IDisposable
    {
        event EventHandler<GaProgressEventArgs> OnAoiProgressing;
        event EventHandler<GaProgressEventArgs> OnAoiBegin;
        event EventHandler<GaProgressEventArgs> OnAoiEnd;
        event EventHandler<ProcessEventArgs> OnError;

        ScanInspectMode xScanInspectMode { get; set; }

        bool IsPass { get; }
        bool Running { get; }
        long ElapsedTime { get; }

        string LotId { get; set; }
        string StripId { get; set; }
        string FileBarcodeStr { get; set; }
        string FileName { get; }

        bool QrJudged { get; set; }
        bool QrUsed { get; set; }

        int[] GetQrResult();
        float[] GetScanOffset();
        int[] GetSingleResult();

        /// <summary>
        /// 2025-08-28 LETIAN: 巨圖 統一由 LineScanCamImageHolder 保管其生命週期
        /// </summary>
        GaBigImageHolder LineScanCamImageHolder { get; }

        void Run();

        bool CalcChipDimension(EzLSD.LineSegment[] lines, out SizeF chipSize, bool usePostScale);
    }
}