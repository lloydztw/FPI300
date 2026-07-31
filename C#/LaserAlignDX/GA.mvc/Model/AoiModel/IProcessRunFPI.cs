using LaserAlignDX.BasicSpace;
using LaserAlignDX.Model;
using LaserAlignDX.Mvc.Model;
using LaserAlignDX.OPSpace;
using LeTian.AoiLib;
using System;
using System.Drawing;
using VisionDesigner;
using ProcessEventArgs = NeedleX.ProcessSpace.ProcessEventArgs;


namespace LaserAlignDX.AoiModel
{
    public interface IProcessRunFPI : IDisposable
    {
        event EventHandler<GaProgressEventArgs> OnAoiProgressing;
        event EventHandler<GaProgressEventArgs> OnAoiBegin;
        event EventHandler<GaProgressEventArgs> OnAoiEnd;
        event EventHandler<ProcessEventArgs> OnError;
        event EventHandler OnLotDataChanged;

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

        /// <summary>
        /// 為 參數編輯 所用
        /// </summary>
        ErrorCodes BuildMicroChipTransform(SizeF targetSize, EzLSD.LineSegment[] lines, Bitmap regionBmp, RectangleF regionRoi);

        /// <summary>
        /// 為 參數調試 所用
        /// </summary>
        bool TryRunOneChip(RegionCellX3Class cell, Bitmap cellBmp, RectangleF cellRoi);

        /// <summary>
        /// 為 參數調試 所用
        /// </summary>
        bool TryFindLineSegment(EdgeBorder eBorder, Bitmap bmpSrc, RectangleF roiRect, out CMvdLineSegmentF resultLine);
    }
}