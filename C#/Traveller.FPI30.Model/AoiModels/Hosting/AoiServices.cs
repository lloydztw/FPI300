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
using LaserAlignDX.Model;
using LaserAlignDX.Model.Coords;
using LaserAlignDX.OPSpace;
using VisionDesigner;

namespace LaserAlignDX.AoiModel
{
    /// <summary>
    /// Borrowed system state required by AOI; 
    /// contains no machine commands.
    /// </summary>
    public interface IAoiContext
    {
        CarrierEnum ActiveCarrierID { get; }
        ITravellerTransforms TransformsModel { get; }
        IMicroChipTransform GetMicroTransform(CarrierEnum carrierID);
        GaBigImageHolder LineScanImageHolder { get; }
        IxEmptyTrayInspector EmptyTrayAoiModel { get; }
    }

    /// <summary>
    /// Live settings: 
    /// the host may change values between inspections.
    /// </summary>
    public interface IAoiSettings
    {
        bool IsForceInspect { get; }
        bool IsSaveTestImage { get; }
        bool IsSaveDebugBmp { get; }
        bool IsSaveDebugOrgBmp { get; }
        bool UseOkNgDiffImageFolders { get; }
        int ImageQuality { get; }
        int ThreadCount { get; }
        bool ThreadsEnabled { get; }
        float Cal_Bcx { get; }
        float Cal_Bcy { get; }
        float Cal_Bca { get; }
    }

    /// <summary>
    /// Host-owned lot data and diagnostic output. 
    /// AOI does not own these resources.
    /// </summary>
    public interface IAoiLotData
    {
        LotData LotData { get; set; }
        string FileBarcodeStr { get; set; }
        DateTime TimeTag { get; }
        void MarkFileTimeTag();
        string GetLotFileName(string tag, string extension);
        string GetLogPath(string subFolder);
        void AsyncDumpLineSegmentsData(RegionCellX3Class cell, ref RectangleF cellRoi);
        void AsyncSaveOrgImage(Bitmap fullFov, bool pass);
        void AsyncSaveCellBmp(Bitmap cellBmp, RegionCellX3Class cell);
        void DumpImage(RegionCellX3Class cell, CMvdImage image, string folder, string postfix);
    }

    /// <summary>
    /// Formats inspection data for the host's consumer; 
    /// never writes to hardware.
    /// </summary>
    public interface IAoiResultFormatter
    {
        int[] GetSingleResult(bool forceAllPass);
        int[] GetQrResult();
        float[] GetScanOffset();
        IEnumerable<RegionCellX3Class> IterFinalResultCells();
    }

    public sealed class AoiServices
    {
        public AoiServices(IAoiContext context, IAoiSettings settings,
            IAoiLotData lotData, IAoiResultFormatter results)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            LotData = lotData ?? throw new ArgumentNullException(nameof(lotData));
            Results = results ?? throw new ArgumentNullException(nameof(results));
        }

        public IAoiContext Context { get; }
        public IAoiSettings Settings { get; }
        public IAoiLotData LotData { get; }
        public IAoiResultFormatter Results { get; }
    }

    /// <summary>
    /// Host binding for the existing singleton AOI/recipe. Bind before creating AOI;
    /// stop all AOI work and dispose AOI before disposing the binding. One host per process.
    /// </summary>
    public static class AoiRuntime
    {
        #region PRIVATE_DATA
        static readonly object _gate = new object();
        static AoiServices _services;
        #endregion

        public static AoiServices Services
        {
            get
            {
                lock (_gate)
                    return _services ?? throw new InvalidOperationException("Bind AOI services before using the AOI model.");
            }
        }

        public static IDisposable Bind(AoiServices services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            lock (_gate)
            {
                if (_services != null) throw new InvalidOperationException("AOI services are already bound.");
                _services = services;
                return new Binding();
            }
        }

        #region PRIVATE_TYPES
        sealed class Binding : IDisposable
        {
            #region PRIVATE_DATA
            bool _disposed;
            #endregion

            public void Dispose()
            {
                lock (_gate)
                {
                    if (_disposed) return;
                    _disposed = true;
                    _services = null;
                }
            }
        }
        #endregion
    }
}
