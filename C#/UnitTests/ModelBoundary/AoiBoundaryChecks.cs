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

        Throws<InvalidOperationException>(() => { var context = new ProbeAoi().Context; });
        Throws<InvalidOperationException>(() => { var aoi = ProcessRunFPIClass.Instance; });
        var settings = new AppAoiSettings();
        int requested = 0;
        Func<ITravellerSysModel> createSystem = () => TravellerModelFactory.Create(settings,
            new TestEnvironment(), new TestWriter(), () => { throw new InvalidOperationException("Unexpected calibration creation."); },
            () => { requested++; return new ProbeAoi(); });
        var system = createSystem();
        var binding = system;
        try
        {
            Check(requested == 0, "Binding eagerly created the system (would recurse during startup).");
            Throws<InvalidOperationException>(() => createSystem());
            using (var aoi = (ProbeAoi)system.AoiModel)
            {
                Check(requested == 1 && ReferenceEquals(aoi, system.AoiModel) && ReferenceEquals(aoi.Context, system), "Factory did not share its lazy AOI/system.");
                Check(aoi.Carrier == CarrierEnum.C1, "Wrong carrier.");
                system.ActiveCarrierID = CarrierEnum.C2;
                Check(aoi.Carrier == CarrierEnum.C2, "Carrier was captured instead of read live.");
                Traveller106.Universal.N_THREADS = 3;
                Check(system.Settings.ThreadCount == 3, "Thread count not forwarded.");
                Traveller106.Universal.N_THREADS = 5;
                Traveller106.INI.Instance.IsForceInspect = true;
                Check(system.Settings.ThreadCount == 5 && system.Settings.IsForceInspect,
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

            RecipeFPIX3Class.DisposeAll();
            binding.Dispose();
        }
        Throws<InvalidOperationException>(() => { var context = new ProbeAoi().Context; });
        using (var rebound = createSystem())
        {
            binding.Dispose();
            Check(ReferenceEquals(new ProbeAoi().Context, rebound), "An old model cleared the new runtime binding.");
        }
        CheckFactoryFailures(settings);
        Console.WriteLine("PASS AOI assembly ownership, host binding/laziness, live settings, borrowed images, lot sharing, error cancellation and result forwarding");
    }

    #region PRIVATE_METHODS
    static void CheckFactoryFailures(IAoiSettings settings)
    {
        var environment = new TestEnvironment();
        var writer = new TestWriter();
        Func<ICalibAoiModel> calib = () => { throw new InvalidOperationException("Unexpected calibration creation"); };
        Throws<ArgumentNullException>(() => TravellerModelFactory.Create(null, environment, writer, calib));
        int attempts = 0;
        Func<IProcessRunFPI> failCreate = () => { attempts++; throw new InvalidOperationException("AOI creation failure"); };
        using (var unused = TravellerModelFactory.Create(settings, environment, writer, calib, failCreate))
            Check(ReferenceEquals(unused.LotData, LotDataHolder.Instance), "Lot data was wrapped by the host.");
        Check(attempts == 0, "Unused model created AOI during disposal.");
        var ownedAoi = new OwnedAoi();
        var owned = TravellerModelFactory.Create(settings, environment, writer, calib, () => ownedAoi);
        Check(ReferenceEquals(owned.AoiModel, ownedAoi), "Factory replaced the supplied AOI.");
        var ownedImages = owned.LineScanImageHolder;
        ownedImages.TakeOver(new Bitmap(2, 2));
        owned.Dispose();
        owned.Dispose();
        Check(ownedAoi.DisposeCount == 1 && ownedImages.IsEmpty(), "Model did not release its AOI/images exactly once.");
        using (var failed = TravellerModelFactory.Create(settings, environment, writer, calib, failCreate))
        {
            Throws<InvalidOperationException>(() => { var aoi = failed.AoiModel; });
            Throws<InvalidOperationException>(() => { var aoi = failed.AoiModel; });
            Check(attempts == 1, "Failed lazy initialization retried unexpectedly.");
        }
        var failing = TravellerModelFactory.Create(settings, environment, writer, calib, () => new FailingDisposeAoi());
        var forced = failing.AoiModel;
        var holder = failing.LineScanImageHolder;
        holder.TakeOver(new Bitmap(2, 2));
        Throws<AggregateException>(failing.Dispose);
        Check(holder.IsEmpty(), "AOI disposal failure prevented shared image cleanup.");
        failing.Dispose();
        Throws<ObjectDisposedException>(() => { var aoi = failing.AoiModel; });
        using (var next = TravellerModelFactory.Create(settings, environment, writer, calib, failCreate))
            Check(ReferenceEquals(next.Settings, settings), "Settings were duplicated instead of borrowed.");
        Console.WriteLine("PASS factory lazy ownership, duplicate-session rejection, creation/disposal failures and rebinding");
    }
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
        public ITravellerSysModel Context => _sysModel;
        public CarrierEnum Carrier => getActiveCarrierID();
        public bool ReportError() => fire_AoiError(ErrorCodes.EXCEPTION_AT_AOI_RUN, "probe");
        public void SetPass(bool pass) => markRunEnd(pass);
    }
    sealed class TestEnvironment : ISystemModelEnvironment
    {
        public string ActiveRecipeName => throw new InvalidOperationException("Unexpected environment access.");
        public string CommonBaseTransformsFile => throw new InvalidOperationException("Unexpected environment access.");
        public string GetTransformsFile(string file) => throw new InvalidOperationException("Unexpected environment access.");
        public string GetEmptyTrayModelName(string name, CarrierEnum carrier) => throw new InvalidOperationException("Unexpected environment access.");
        public string GetEmptyTrayRecipeFile(string name, CarrierEnum carrier) => throw new InvalidOperationException("Unexpected environment access.");
    }
    sealed class TestWriter : ICoordinateWriter
    {
        public bool IsAvailable => throw new InvalidOperationException("Unexpected PLC access.");
        public void Write(CarrierEnum carrier, SuckerRowEnum row, PointF point) => throw new InvalidOperationException("Unexpected PLC access.");
    }
    sealed class FailingDisposeAoi : ProcessRunFPIClass
    {
        public override void Dispose()
        {
            Check(_sysModel != null, "Runtime was unbound before AOI disposal.");
            throw new InvalidOperationException("AOI disposal failure");
        }
    }
    sealed class OwnedAoi : ProcessRunFPIClass
    {
        public int DisposeCount { get; private set; }
        public override void Dispose()
        {
            Check(_sysModel != null, "Runtime was unbound before owned AOI disposal.");
            DisposeCount++;
            base.Dispose();
        }
    }
    #endregion
}

// The real AppAoiSettings adapter is compiled against these host stand-ins.
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
        public static Traveller.FPI30.Config.IFpix3Machine Machine => throw new InvalidOperationException("Unexpected machine access.");
        public static int N_THREADS = 1;
        public static bool N_THREADS_ENABLED = false;
    }
}
