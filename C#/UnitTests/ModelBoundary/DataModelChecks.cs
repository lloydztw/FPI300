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
using System.Collections.Concurrent;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using JetEazy.QvMath;
using LaserAlignDX;
using LaserAlignDX.AoiModel;
using LaserAlignDX.Model;
using LaserAlignDX.OPSpace;
using LaserAlignDX.OPSpace.RecipeSpace;

internal static class DataModelChecks
{
    public static RegionCellX3Class Cell(int index, int row, int col, InspectReason reason)
    {
        var cell = new RegionCellX3Class { Index = index, CellRow = row, CellCol = col,
            viewRectF = new RectangleF(col * 20, row * 20, 10, 10) };
        cell.ChipData.ChipQuad2D = new QvQuad2D();
        cell.MarkResult(reason);
        return cell;
    }

    public static void Run(DataOutputProbe output)
    {
        var assembly = typeof(RecipeFPIX3Class).Assembly;
        Check(typeof(LotDataHolder).Assembly == assembly && typeof(PlcDataPacker).Assembly == assembly,
            "Lot data or PLC formatter was not moved to the model DLL.");
        var recipe = RecipeFPIX3Class.Instance;
        recipe.xRegionCells.Clear();
        var reasons = new[] { InspectReason.PASS, InspectReason.NG_CUT, InspectReason.NG_EMPTY,
            InspectReason.NG_AMBIGUOUS_BLOC, InspectReason.NG_QRCODE_COMPARE, InspectReason.NG_QRCODE_ERR };
        var cells = reasons.Select((reason, i) => Cell(i < 3 ? i : 8 - i, i / 3, i % 3, reason)).ToArray();
        foreach (var cell in cells)
        {
            cell.RunX = cell.Index + 1; cell.RunY = cell.Index + 11; cell.RunAngle = cell.Index + 21;
            recipe.xRegionCells.Add(cell);
        }
        try
        {
            Check(PlcDataPacker.GetSingleResult(false).SequenceEqual(new[] { 1, 9, 3, 4, 4, 3 }), "Single result codes/order changed.");
            Check(PlcDataPacker.GetQrResult().SequenceEqual(new[] { 1, 1, 3, 4, 2, 3 }), "QR codes/order changed.");
            Check(PlcDataPacker.GetSingleResult(true).SequenceEqual(new[] { 1, 1, 3, 1, 1, 3 }), "Forced PASS picked an empty/ambiguous cell.");
            Check(PlcDataPacker.GetScanOffset().SequenceEqual(new float[] { 1,11,21, 2,12,22, 0,0,0, 4,14,24, 5,15,25, 0,0,0 }), "Scan offsets/order changed.");
            output.UsingSingleNgCode = true;
            Check(PlcDataPacker.GetSingleResult(false).SequenceEqual(new[] { 1, 77, 3, 77, 77, 3 }), "Unified NG changed empty/PASS codes.");
            Check(PlcDataPacker.GetQrResult().SequenceEqual(new[] { 1, 1, 3, 77, 77, 3 }), "Unified QR NG changed.");
            output.UsingSingleNgCode = false;
            using (var replacement = Cell(0, 0, 0, InspectReason.NG_EDGE_GAP))
            {
                cells[0].OutGridLink = replacement;
                Check(PlcDataPacker.GetSingleResult(false)[0] == 8, "OutGridLink result was lost.");
                cells[0].OutGridLink = null;
            }
            output.IsSimulation = true;
            PlcDataPacker.GetQrResult();
            PlcDataPacker.GetScanOffset();
            Check(File.ReadAllLines(Path.Combine(output.PlcTracePath, "QrResult.csv")).Length == 7 &&
                File.ReadAllLines(Path.Combine(output.PlcTracePath, "ScanOffset.csv")).Length == 7, "Diagnostic output path/rows changed.");
            output.IsSimulation = false;
            CheckImages(output, cells[0]);
        }
        finally
        {
            output.IsSimulation = false;
            output.UsingSingleNgCode = false;
            recipe.xRegionCells.Clear();
            foreach (var cell in cells) cell.Dispose();
        }
        Console.WriteLine("PASS actual lot image storage/upload callback, PLC codes, zigzag offsets, unified NG and trace output");
    }

    #region PRIVATE_METHODS
    static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    static void CheckImages(DataOutputProbe output, RegionCellX3Class cell)
    {
        var holder = LotDataHolder.Instance;
        int changes = 0;
        EventHandler changed = (sender, args) => changes++;
        holder.OnLotDataChanged += changed;
        try { holder.LotId = "LOT"; }
        finally { holder.OnLotDataChanged -= changed; }
        holder.StripId = "STRIP";
        holder.FileBarcodeStr = "BARCODE";
        var time = holder.TimeTag;
        Check(changes == 1 && holder.FileName == $"LOT-{time:yyyyMMdd_HHmmss}.txt", "Lot event/filename changed.");
        Check(holder.GetLogPath(null) == Path.Combine(output.LogImagePath, time.ToString("yyyyMMdd"), "Dump"), "Log path changed.");
        output.IsSaveDebugOrgBmp = true;
        output.IsSaveTestImage = true;
        using (var source = new Bitmap(16, 12))
        {
            holder.AsyncSaveOrgImage(source, false);
            holder.AsyncSaveCellBmp(source, cell);
        }
        Check(SpinWait.SpinUntil(() => output.Uploads.Count == 1, 10000), "Line-scan image save/upload callback timed out.");
        var upload = output.Uploads.Single();
        Check(upload.Item1 == Path.Combine(output.ResultImagePath, "LineScanImageOrg.NG", time.ToString("yyyyMMdd"), "STRIP", $"LOT-{time:yyyyMMdd_HHmmss}.jpg") &&
            upload.Item2 == "线扫" && upload.Item3 == time && upload.Item4 == "LOT", "Upload metadata/path changed.");
        using (var saved = new Bitmap(upload.Item1)) Check(saved.Width == 16 && saved.Height == 12, "Image clone/save changed.");
        var cellFile = Path.Combine(holder.GetLogPath("BARCODE"), "PositionFix", "Cell_0@0_0.bmp");
        Check(SpinWait.SpinUntil(() => ImageReady(cellFile), 10000), "Cell image save timed out.");
        output.IsSaveDebugOrgBmp = false;
        output.IsSaveTestImage = false;
    }

    static bool ImageReady(string file)
    {
        try { using (var image = new Bitmap(file)) return image.Width == 16 && image.Height == 12; }
        catch (ArgumentException) { return false; }
        catch (IOException) { return false; }
    }
    #endregion
}

internal sealed class DataOutputProbe : IDataOutputSettings, IImageUploadService
{
    public DataOutputProbe(string folder)
    {
        LogImagePath = Path.Combine(folder, "logs");
        ResultImagePath = Path.Combine(folder, "results");
        PlcTracePath = Path.Combine(folder, "plc-traces");
    }
    public string LogImagePath { get; }
    public string ResultImagePath { get; }
    public string PlcTracePath { get; }
    public bool IsSaveDebugBmp => false;
    public bool IsSaveDebugOrgBmp { get; set; }
    public bool IsSaveTestImage { get; set; }
    public bool UseOkNgDiffImageFolders => true;
    public int ImageQuality => 90;
    public bool UsingSingleNgCode { get; set; }
    public int SingleNgCode => 77;
    public bool IsSimulation { get; set; }
    public bool Enabled => true;
    public ConcurrentQueue<Tuple<string, string, DateTime, string>> Uploads { get; } = new ConcurrentQueue<Tuple<string, string, DateTime, string>>();
    public bool UploadFile(string fileName, string category, DateTime time, string lotId)
    {
        if (!File.Exists(fileName)) throw new InvalidOperationException("Upload was called before saving the image.");
        Uploads.Enqueue(Tuple.Create(fileName, category, time, lotId));
        return true;
    }
}
