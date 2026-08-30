#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-08-01 開始重整 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using LaserAlignDX.BasicSpace;
using LaserAlignDX.Model;
using LaserAlignDX.Mvc.Model;
using LaserAlignDX.OPSpace;
using LeTian.AoiLib;
using System;
using System.Collections.Generic;
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

        /// <summary>
        /// 為了 Mvd 海康套件 所使用
        /// </summary>
        void Train();

        /// <summary>
        /// 執行 AOI 運算
        /// </summary>
        /// <remarks>
        /// 默認 sceneBmp = null, 代表使用 GaBigImageHolder 當輸入影像
        /// </remarks>
        void Run(Bitmap bmpScene = null);

#if (OPT_OLD || true)
        /// <summary>
        /// 為 參數編輯 所用
        /// </summary>
        ErrorCodes BuildMicroChipTransform(SizeF targetSize, EzLSD.LineSegment[] lines, Bitmap regionBmp, RectangleF regionRoi);
#endif

        /// <summary>
        /// 為 參數編輯 所用
        /// </summary>
        /// <remarks>
        /// lineEdgePairs 單位為 pixels (FullFov Cammera Coordinates)
        /// </remarks>
        ErrorCodes BuildMicroChipTransform(Dictionary<string, LineBorderPair> lineEdgePairs, Bitmap regionBmp, RectangleF regionRoi);

        /// <summary>
        /// 為 參數調試 所用
        /// </summary>
        bool TryRunOneChip(RegionCellX3Class cell, Bitmap cellBmp, RectangleF cellRoi);

        /// <summary>
        /// 為 參數調試 所用
        /// </summary>
        bool TryFindLineSegment(EdgeBorder eBorder, Bitmap bmpSrc, RectangleF roiRect, out CMvdLineSegmentF resultLine);

        /// <summary>
        /// Decodes a QR code from the specified bitmap image within the defined region of interest.
        /// </summary>
        string DecodeQrCode(Bitmap bmp, Rectangle? roi = null);

        IAoiChipLocator GetChipLocAoi();

        IAoiFlyCamMatcher GetFlyCameraAoi();
    }
}