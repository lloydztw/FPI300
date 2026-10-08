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
using System.Linq;
using EzAoiEmptyTrayInspector.Model;
using LaserAlignDX;
using LaserAlignDX.AoiModel;
using LaserAlignDX.AoiModel.V3;
using LaserAlignDX.Model;
using LaserAlignDX.Model.Coords;
using LaserAlignDX.Mvc.Model;
using LaserAlignDX.OPSpace;
using LaserAlignDX.OPSpace.RecipeSpace;
using NeedleX.ProcessSpace;
using VisionDesigner;

internal static class AoiBoundaryChecks
{
    public static void Run()
    {
        var assembly = typeof(RecipeFPIX3Class).Assembly;
        foreach (var type in new[] { typeof(AoiModelBase), typeof(IProcessRunFPI), typeof(ProcessRunFPIClass),
            typeof(GaBigImageHolder), typeof(ProcessEventArgs), typeof(LaserAlignDX.BasicSpace.Mvd2DReaderClass) })
            Check(type.Assembly == assembly, "AOI type not supplied by the model DLL: " + type);

        Throws<InvalidOperationException>(() => { var services = AoiRuntime.Services; });
        Throws<InvalidOperationException>(() => { var aoi = ProcessRunFPIClass.Instance; });
        Throws<ArgumentNullException>(() => AoiRuntime.Bind(null));
        var system = new TestSystem();
        int requested = 0;
        var adapter = new AppAoiServices(() => { requested++; return system; });
        var binding = adapter.Bind();
        try
        {
            Check(requested == 0, "Binding eagerly created the system (would recurse during startup).");
            Throws<InvalidOperationException>(() => adapter.Bind());
            using (var aoi = new ProbeAoi())
            {
                Check(requested == 0, "AOI constructor eagerly accessed the system.");
                Check(aoi.Carrier == CarrierEnum.C1, "Wrong carrier.");
                system.ActiveCarrierID = CarrierEnum.C2;
                Check(aoi.Carrier == CarrierEnum.C2, "Carrier was captured instead of read live.");
                Traveller106.Universal.N_THREADS = 3;
                Check(AoiRuntime.Services.Settings.ThreadCount == 3, "Thread count not forwarded.");
                Traveller106.Universal.N_THREADS = 5;
                Traveller106.INI.Instance.IsForceInspect = true;
                Check(AoiRuntime.Services.Settings.ThreadCount == 5 && AoiRuntime.Services.Settings.IsForceInspect,
                    "Settings changes were lost.");
                aoi.LotData = new LotData();
                aoi.LotId = "LOT";
                aoi.StripId = "STRIP";
                aoi.FileBarcodeStr = "BARCODE";
                Check(LotDataHolder.Instance.LotData.LotID == "LOT" && LotDataHolder.Instance.LotData.StripID == "STRIP",
                    "Lot data not shared with host.");
                Check(aoi.FileName == LotDataHolder.Instance.GetLotFileName("LOT", ".txt") && LotDataHolder.Instance.FileBarcodeStr == "BARCODE", "Host filenames changed.");
                var sourceImage = new Bitmap(2, 2);
                system.LineScanImageHolder.TakeOver(sourceImage);
                Check(ReferenceEquals(aoi.LineScanCamImageHolder, system.LineScanImageHolder), "Image holder was copied.");
                bool reported = false;
                aoi.OnError += (sender, ev) =>
                {
                    Check(ReferenceEquals(sender, aoi) && ev.Message == "probe" && Equals(ev.Tag, ErrorCodes.EXCEPTION_AT_AOI_RUN),
                        "Error event contract changed.");
                    reported = true;
                    ev.Cancel = false;
                };
                Check(!aoi.ReportError() && reported, "Error cancellation reply was not preserved.");
                var recipe = RecipeFPIX3Class.Instance;
                recipe.xRegionCells.Clear();
                var cell = DataModelChecks.Cell(0, 0, 0, InspectReason.NG_CUT);
                cell.RunX = 1; cell.RunY = 2; cell.RunAngle = 3;
                recipe.xRegionCells.Add(cell);
                Check(aoi.GetQrResult().SequenceEqual(new[] { 1 }) && aoi.GetScanOffset().SequenceEqual(new[] { 1f, 2f, 3f }),
                    "Result formatter output changed.");
                recipe.InspectParams.optUseTotalNgPercentage = true;
                recipe.InspectParams.optChipMeasurement = true;
                aoi.xScanInspectMode = ScanInspectMode.NOTRAY;
                aoi.SetPass(true);
                Check(aoi.GetSingleResult().Single() == 9, "NOTRAY should not force PASS.");
                // Select a real non-NOTRAY mode rather than relying on enum numeric values.
                aoi.xScanInspectMode = Enum.GetValues(typeof(ScanInspectMode)).Cast<ScanInspectMode>().First(v => v != ScanInspectMode.NOTRAY);
                Check(aoi.GetSingleResult().Single() == 1, "PASS percentage behavior changed.");
                aoi.SetPass(false);
                Check(aoi.GetSingleResult().Single() == 9, "Failed inspection was forced PASS.");
                aoi.Dispose();
                Check(!system.LineScanImageHolder.IsEmpty() && sourceImage.Width == 2, "AOI disposed the borrowed system image.");
            }
        }
        finally
        {
            system.LineScanImageHolder.Dispose();
            RecipeFPIX3Class.DisposeAll();
            binding.Dispose();
        }
        Throws<InvalidOperationException>(() => { var services = AoiRuntime.Services; });
        using (var rebound = adapter.Bind())
        {
            binding.Dispose();
            Check(AoiRuntime.Services != null, "An old binding cleared a new host binding.");
        }
        Console.WriteLine("PASS AOI assembly ownership, host binding/laziness, live settings, borrowed images, lot sharing, error cancellation and result forwarding");
    }

    #region PRIVATE_METHODS
    static void Check(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    static void Throws<T>(Action action) where T : Exception
    {
        try { action(); }
        catch (T) { return; }
        throw new InvalidOperationException("Expected " + typeof(T).Name);
    }
    #endregion

    #region PRIVATE_TYPES
    // Exercise the real pipeline's data/event APIs without allocating licensed MVD algorithms.
    sealed class ProbeAoi : ProcessRunFPIClass
    {
        public CarrierEnum Carrier => getActiveCarrierID();
        public bool ReportError() => fire_AoiError(ErrorCodes.EXCEPTION_AT_AOI_RUN, "probe");
        public void SetPass(bool pass) => markRunEnd(pass);
    }
    sealed class TestSystem : ITravelerModel
    {
        public event EventHandler<ProcessEventArgs> OnError { add { } remove { } }
        public IProcessRunFPI AoiModel => throw new InvalidOperationException("Unexpected AOI access.");
        public ICalibAoiModel CalibAoiModel => throw new InvalidOperationException("Unexpected calibration access.");
        public object GetCurrentRecipe() => throw new InvalidOperationException("Unexpected recipe access.");
        public void ApplyRecipe(params object[] args) => throw new InvalidOperationException("Unexpected recipe access.");
        public bool AutoBuildRegionCells(CarrierEnum carrier, Bitmap image, bool migrate, out MatchResult result) => throw new InvalidOperationException("Unexpected calibration access.");
        public bool WriteCoordsToPlc(CarrierEnum carrier, SuckerRowEnum row, out PointF camera, out PointF motor, out string message) => throw new InvalidOperationException("Unexpected PLC access.");
        public bool WriteAllCoordsToPlc(out string message) => throw new InvalidOperationException("Unexpected PLC access.");
        public void Dispose() => LineScanImageHolder.Dispose();
        public CarrierEnum ActiveCarrierID { get; set; } = CarrierEnum.C1;
        public GaBigImageHolder LineScanImageHolder { get; } = new GaBigImageHolder();
        public ITravellerTransforms TransformsModel => throw new InvalidOperationException("Unexpected transform access.");
        public IxEmptyTrayInspector EmptyTrayAoiModel => throw new InvalidOperationException("Unexpected inspector access.");
        public IMicroChipTransform GetMicroTransform(CarrierEnum carrier) => throw new InvalidOperationException("Unexpected transform access.");
    }
    #endregion
}

// The real AppAoiServices adapter is compiled against these host stand-ins.
// No executable, machine implementation, PLC connection or production path is used.
namespace Traveller106
{
    internal sealed class INI
    {
        public static INI Instance { get; } = new INI();
        public bool IsForceInspect { get; set; }
        public bool IsSaveTestImage => false;
        public bool IsSaveDebugBmp => false;
        public bool IsSaveDebugOrgBmp => false;
        public bool UseOkNgDiffImageFolders => false;
        public int ImageQuality => 90;
        public float Cal_Bcx => 0;
        public float Cal_Bcy => 0;
        public float Cal_Bca => 0;
    }
    internal static class Universal
    {
        public static Traveller.FPI30.Contracts.IFpix3Machine Machine => throw new InvalidOperationException("Unexpected machine access.");
        public static int N_THREADS = 1;
        public static bool N_THREADS_ENABLED = false;
    }
}
