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
using System.Collections.Generic;
using System.Drawing;
using EzAoiEmptyTrayInspector.Model;
using LaserAlignDX.AoiModel;
using LaserAlignDX.Model;
using LaserAlignDX.Model.Coords;
using LaserAlignDX.Mvc.Model;
using LaserAlignDX.OPSpace;
using Traveller106;
using VisionDesigner;

namespace LaserAlignDX
{
    /// <summary>Adapts live application state without creating the system model during binding.</summary>
    internal sealed class AppAoiServices : IAoiContext, IAoiSettings, IAoiLotData, IAoiResultFormatter
    {
        #region PRIVATE_DATA
        readonly Func<ITravelerModel> _system;
        #endregion

        public AppAoiServices(Func<ITravelerModel> system)
        {
            _system = system ?? throw new ArgumentNullException(nameof(system));
        }

        public IDisposable Bind() => AoiRuntime.Bind(new AoiServices(this, this, this, this));

        public CarrierEnum ActiveCarrierID => _system().ActiveCarrierID;
        public ITravellerTransforms TransformsModel => _system().TransformsModel;
        public IMicroChipTransform GetMicroTransform(CarrierEnum carrierID) => _system().GetMicroTransform(carrierID);
        public GaBigImageHolder LineScanImageHolder => _system().LineScanImageHolder;
        public IxEmptyTrayInspector EmptyTrayAoiModel => _system().EmptyTrayAoiModel;

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

        public LotData LotData { get => LotDataHolder.Instance.LotData; set => LotDataHolder.Instance.LotData = value; }
        public string FileBarcodeStr { get => LotDataHolder.Instance.FileBarcodeStr; set => LotDataHolder.Instance.FileBarcodeStr = value; }
        public DateTime TimeTag => LotDataHolder.Instance.TimeTag;
        public void MarkFileTimeTag() => LotDataHolder.Instance.MarkFileTimeTag();
        public string GetLotFileName(string tag, string extension) => LotDataHolder.Instance.GetLotFileName(tag, extension);
        public string GetLogPath(string subFolder) => LotDataHolder.Instance.GetLogPath(subFolder);
        public void AsyncDumpLineSegmentsData(RegionCellX3Class cell, ref RectangleF cellRoi) =>
            LotDataHolder.Instance.AsyncDumpLineSegmentsData(cell, ref cellRoi);
        public void AsyncSaveOrgImage(Bitmap fullFov, bool pass) => LotDataHolder.Instance.AsyncSaveOrgImage(fullFov, pass);
        public void AsyncSaveCellBmp(Bitmap cellBmp, RegionCellX3Class cell) => LotDataHolder.Instance.AsyncSaveCellBmp(cellBmp, cell);
        public void DumpImage(RegionCellX3Class cell, CMvdImage image, string folder, string postfix) =>
            LotDataHolder.Instance.DumpImage(cell, image, folder, postfix);

        public int[] GetSingleResult(bool forceAllPass) => PlcDataPacker.GetSingleResult(forceAllPass);
        public int[] GetQrResult() => PlcDataPacker.GetQrResult();
        public float[] GetScanOffset() => PlcDataPacker.GetScanOffset();
        public IEnumerable<RegionCellX3Class> IterFinalResultCells() => PlcDataPacker.IterFinalResultCells();
    }
}
