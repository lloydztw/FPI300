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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using JetEazy.Utils;
using LaserAlignDX;
using LaserAlignDX.AoiModel;
using LaserAlignDX.Model;
using LaserAlignDX.Model.Coords;
using LaserAlignDX.Mvc.Model.Recipe;
using LaserAlignDX.OPSpace;
using LaserAlignDX.OPSpace.RecipeSpace;

internal static class ModelBoundaryChecks
{
    #region PRIVATE_METHODS
    static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    static void CheckStorage(string folder)
    {
        string path = Path.Combine(folder, "中文參數.ini");
        WinIni.WriteINIValue("產品", "名稱", "測試晶粒", path);
        var source = new DtoX3GridParams { xRealOffsetX = 1.25f, xRealOffsetY = 2.5f };
        source.Save(path);
        var target = new DtoX3GridParams();
        target.Load(path);
        Check(target.xRealOffsetX == 1.25f && target.xRealOffsetY == 2.5f, "Grid parameters did not round-trip.");
        Check(WinIni.ReadINIValue("產品", "名稱", "", path) == "測試晶粒", "Unicode section changed.");
        Check(File.ReadAllBytes(path).Take(3).SequenceEqual(Encoding.UTF8.GetPreamble()), "INI UTF-8 BOM changed.");
    }

    static void CheckTransforms(string folder)
    {
        var transforms = TravellerTransformFactory.Instance("ModelBoundary");
        Check(transforms.GetType().Namespace.EndsWith(".V38"), "Factory did not select V38.");
        transforms.ConfigWorldGridPoints(CarrierEnum.C1, 3, 4, 1.25, 2.5);
        var grid = transforms.GetWorldGridPoints(CarrierEnum.C1);
        var point = grid.Get(2, 3);
        Check(Math.Abs(point.X - 3.75) < 1e-9 && Math.Abs(point.Y - 5.0) < 1e-9, "World grid coordinates changed.");
        string path = Path.Combine(folder, "world-grid.ini");
        grid.Save(path, "C1");
        grid.Config(1, 1, 1, 1);
        grid.Load(path, "C1");
        Check(grid.Rows == 3 && grid.Cols == 4 && grid.PitchX == 1.25 && grid.PitchY == 2.5, "World grid persistence changed.");
        TravellerTransformFactory.DisposeAll();
        Check(!ReferenceEquals(transforms, TravellerTransformFactory.Instance("ModelBoundary")), "Transform registry was not released.");
        TravellerTransformFactory.DisposeAll();
    }

    static void CheckRegionCells()
    {
        Check(typeof(RegionCellsDataCollection).Assembly == typeof(RecipeFPIX3Class).Assembly,
            "Region cells collection is not supplied by the model DLL.");
        Check(typeof(Zigzag).Assembly == typeof(RecipeFPIX3Class).Assembly,
            "Grid traversal still depends on the host assembly.");
        var cells = new[] { 0, 1, 2, 3, 5 }.Select(index => new RegionCellX3Class
        {
            Index = index,
            CellRow = index / 3,
            CellCol = index % 3,
            viewRectF = new RectangleF(index % 3 * 10, index / 3 * 10, 8, 8)
        }).ToArray();
        cells[1].MarkResult(InspectReason.NG_CUT);
        cells[2].MarkResult(InspectReason.NG_EMPTY);
        cells[3].MarkResult(InspectReason.NG_AMBIGUOUS_BLOC);
        using (var collection = new RegionCellsDataCollection(cells))
        {
            Check(collection.Rows == 2 && collection.Cols == 3, "Grid dimensions changed.");
            Check(collection.IterGridCells().Select(cell => cell.Index).SequenceEqual(new[] { 0, 1, 2, 5, 3 }),
                "Serpentine traversal changed.");
            Check(collection.GetGridCell(1, 1) == null && collection.GetFinalCell(1, 1).IsEmptyPlaceHold(),
                "Sparse grid placeholder changed.");
            int total = collection.GetStatistics(out int pass, out int ng, out int empty, out int unknown);
            Check(total == 6 && pass == 2 && ng == 1 && empty == 2 && unknown == 1, "Inspection statistics changed.");
            collection.Reset();
            collection.GetStatistics(out pass, out ng, out empty, out unknown);
            Check(pass == 5 && ng == 0 && empty == 1 && unknown == 0, "Reset behavior changed.");
            var replacement = new RegionCellX3Class();
            cells[0].OutGridLink = replacement;
            Check(ReferenceEquals(collection.GetFinalCell(0, 0), replacement), "Final cell substitution changed.");
            collection.Dispose();
            Check(ReferenceEquals(cells[0].OutGridLink, replacement), "Attach disposed a caller-owned cell.");
        }
        foreach (var cell in cells) cell.Dispose();
        using (var owner = new RegionCellsDataCollection())
        {
            owner.TakeOver(cells);
            cells[0].OutGridLink = new RegionCellX3Class();
            owner.Dispose();
            Check(cells[0].OutGridLink == null && owner.Rows == 0 && owner.Cols == 0,
                "TakeOver did not release owned cells.");
        }
    }

    static void CheckRecipeHost(string folder)
    {
        var recipe = RecipeFPIX3Class.Instance;
        recipe.Initial(folder, 1, "recipe.ini");
        int trained = 0;
        int requestedGrids = 0;
        using (var bindings = new AppModelBindings(() => trained++, carrier => { requestedGrids++; return null; }))
        {
            Check(recipe.xCamGrid1 == null && recipe.xCamGrid2 == null && requestedGrids == 2, "Camera grid provider was not invoked.");
            recipe.Load();
            Check(trained == 1, "Recipe load did not request training exactly once.");
            recipe.zFocusOnCarrier = 12.5f;
            recipe.zFocusOnChip = 13.25f;
            recipe.xExtendx = 43;
            recipe.xExtendy = 57;
            recipe.Save();
            Check(trained == 2, "Recipe save did not request training exactly once.");
            recipe.zFocusOnCarrier = 0;
            recipe.zFocusOnChip = 0;
            recipe.Load();
            Check(trained == 3 && recipe.zFocusOnCarrier == 12.5f && recipe.zFocusOnChip == 13.25f && recipe.xExtendx == 43 && recipe.xExtendy == 57,
                "Recipe values or training behavior changed.");
            bindings.Dispose();
        }
        Check(RecipeFPIX3Class.CameraGridProvider == null, "Host grid provider was retained after disposal.");
        recipe.Load();
        Check(trained == 3, "Disposed host was still called.");

        int failed = 0;
        Action fail = () => { throw new InvalidOperationException("training failure probe"); };
        Action<Exception> observe = error => { Check(error.Message == "training failure probe", "Wrong training error."); failed++; };
        RecipeFPIX3Class.TrainingRequested += fail;
        RecipeFPIX3Class.TrainingFailed += observe;
        try { recipe.Load(); Check(failed == 1, "Training failure was not reported without a UI."); }
        finally
        {
            RecipeFPIX3Class.TrainingRequested -= fail;
            RecipeFPIX3Class.TrainingFailed -= observe;
            RecipeFPIX3Class.DisposeAll();
        }
    }

    static int Main()
    {
        try
        {
            var assembly = typeof(RecipeFPIX3Class).Assembly;
            Check(assembly.GetName().Name == "Traveller.FPI30.Model", "Tests are not using the model DLL.");
            Check(!assembly.GetReferencedAssemblies().Any(a => a.Name == "FPI30AOIX3" || a.Name == "Traveller.FPI30.Machine"), "Model references an application or machine implementation.");
            var folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fixtures", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(folder);
            var output = new DataOutputProbe(folder);
            using (ModelDataRuntime.Bind(new ModelDataServices(output, output, () => CarrierEnum.C1)))
            {
                CheckStorage(folder);
                CheckTransforms(folder);
                CheckRegionCells();
                CheckRecipeHost(folder);
                AoiBoundaryChecks.Run();
                SystemModelChecks.Run();
                DataModelChecks.Run(output);
                FlyRunnerChecks.Run();
            }
            Check(!AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name == "FPI30AOIX3" || a.GetName().Name == "Traveller.FPI30.Machine"), "Test loaded the host or hardware DLL.");
            Console.WriteLine("PASS model DLL boundary, Unicode INI, V38 coordinates, region cells/traversal/statistics/ownership, recipe round-trip, training callbacks and host disposal");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
    #endregion
}

// AppModelBindings uses the application's log sink; tests supply a silent sink.
namespace JetEazy.Utils
{
    internal static class GaUtil
    {
        public static void LOG(string text, params object[] args) { }
        public static void LOG_ERROR(Exception error, string text) { }
    }
}

namespace JetEazy.FormSpace
{
    internal static class VsMessageBox
    {
        public static void Warning(string message) { throw new InvalidOperationException("Unexpected UI notification: " + message); }
    }
}
