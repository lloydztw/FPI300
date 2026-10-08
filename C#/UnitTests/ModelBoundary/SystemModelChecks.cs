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
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using EzAoiEmptyTrayInspector.Model;
using JetEazy.Match;
using JetEazy.QMath;
using LaserAlignDX;
using LaserAlignDX.AoiModel;
using LaserAlignDX.AoiModel.Calib;
using LaserAlignDX.Model.Coords;
using LaserAlignDX.Mvc.Model;
using LaserAlignDX.OPSpace.RecipeSpace;
using OpenCvSharp;
using VsCommon.ControlSpace.IOSpace;
using CalibV5 = LaserAlignDX.AoiModel.Calib.V5.CalibAoiModel;

internal static class SystemModelChecks
{
    public static void Run()
    {
        var assembly = typeof(RecipeFPIX3Class).Assembly;
        foreach (var type in new[] { typeof(TravellerSysModel), typeof(ITravelerModel), typeof(TravellerBigImagesHolder),
            typeof(ICalibAoiModel), typeof(JxCalibRecipe), typeof(CalibV5),
            typeof(LaserAlignDX.AoiModel.Calib.V3.CalibAoiModel), typeof(LaserAlignDX.AoiModel.Calib.V25.CalibAoiModel) })
            Check(type.Assembly == assembly, "Model ownership mismatch: " + type);

        using (var image = CreateBoard())
        using (var recipe = CreateCalibRecipe())
        {
            var models = new ICalibAoiModel[] { new CalibV5(), new LaserAlignDX.AoiModel.Calib.V3.CalibAoiModel(), new LaserAlignDX.AoiModel.Calib.V25.CalibAoiModel() };
            foreach (var calib in models)
            using (calib)
            {
                Check(calib.FetchBoardGrid(CarrierEnum.C1, null, recipe) == null, "Missing calibration image changed behavior.");
                using (var grid = calib.FetchBoardGrid(CarrierEnum.C1, image, recipe))
                    CheckGrid(grid);
            }
            Check(image.Width == 300 && recipe.GridSettings.EmptyTraySettings.FullRows.Value == 3,
                "Calibration disposed borrowed input.");
        }

        var plc = new PlcProbe();
        IPlcIoFPIX3 currentPlc = null;
        int plcRequests = 0;
        var writer = new AppCoordinateWriter(() => { plcRequests++; return currentPlc; });
        var env = new TestEnvironment();
        int calibCreations = 0;
        var calibProbe = new CalibProbe();
        var aoiProbe = new AoiProbe();
        var borrowedAoi = (IProcessRunFPI)aoiProbe.GetTransparentProxy();
        using (var model = new TravellerSysModel(borrowedAoi, () => { calibCreations++; return calibProbe; }, env, writer))
        {
            Check(calibCreations == 0 && plcRequests == 0 && env.Reads == 0, "System construction accessed host state eagerly.");
            Check(ReferenceEquals(model.AoiModel, borrowedAoi), "System did not borrow supplied AOI.");
            Check(ReferenceEquals(model.CalibAoiModel, model.CalibAoiModel) && calibCreations == 1, "Calibration lifetime is not lazy/shared.");
            Check(!model.WriteAllCoordsToPlc(out var unavailable) && !string.IsNullOrEmpty(unavailable) && plc.Calls.Count == 0,
                "Missing PLC was not reported before coordinate output.");
            currentPlc = (IPlcIoFPIX3)plc.GetTransparentProxy();
            foreach (CarrierEnum carrier in Enum.GetValues(typeof(CarrierEnum)))
                model.TransformsModel.ConfigWorldGridPoints(carrier, 1, 1, 10, 10);
            Check(!model.WriteAllCoordsToPlc(out var invalid) && !string.IsNullOrEmpty(invalid) && plc.Calls.Count == 0,
                "An invalid world grid wrote coordinates.");

            ConfigureTransforms(model.TransformsModel);
            ITravelerModel api = model;
            Check(model.WriteAllCoordsToPlc(out var success) && success == "OK" && plc.Calls.Count == 4,
                "Complete coordinate output failed.");
            CheckRoute(plc.Calls[0], "SetStage1", SuckerRowEnum.S1, new PointF(180, 280));
            CheckRoute(plc.Calls[1], "SetStage1", SuckerRowEnum.S2, new PointF(200, 310));
            CheckRoute(plc.Calls[2], "SetStage2", SuckerRowEnum.S1, new PointF(380, 480));
            CheckRoute(plc.Calls[3], "SetStage2", SuckerRowEnum.S2, new PointF(400, 510));
            plc.Calls.Clear();
            Check(api.WriteCoordsToPlc(CarrierEnum.C2, SuckerRowEnum.S2, out var cam, out var motor, out var message), "Single coordinate output failed.");
            Near(cam, new PointF(80, 80));
            Near(motor, new PointF(400, 510));
            CheckRoute(plc.Calls.Single(), "SetStage2", SuckerRowEnum.S2, motor);

            var holder = model.LineScanImageHolder;
            holder.TakeOver(new Bitmap(2, 2));
            model.Dispose();
            model.Dispose();
            Check(holder.IsEmpty() && calibProbe.Disposals == 1 && aoiProbe.Disposals == 0,
                "System must dispose its images/calibration once and retain borrowed AOI.");
        }
        // Cleanup must continue even when a calibration service throws while disposing.
        var failingCalib = new CalibProbe { FailDispose = true };
        var failingModel = new TravellerSysModel(borrowedAoi, () => failingCalib, env, writer);
        var forced = failingModel.CalibAoiModel;
        var imageHolder = failingModel.LineScanImageHolder;
        imageHolder.TakeOver(new Bitmap(1, 1));
        bool threw = false;
        try { failingModel.Dispose(); } catch (AggregateException) { threw = true; }
        Check(threw && imageHolder.IsEmpty() && failingCalib.Disposals == 1, "Cleanup stopped after calibration disposal failure.");
        failingModel.Dispose();
        Check(aoiProbe.Disposals == 0, "Borrowed AOI was released on cleanup failure.");
        Console.WriteLine("PASS calibration V25/V3/V5 synthetic grids, system lazy ownership/disposal, missing PLC/invalid world grids, V38 coordinates and all four PLC routes");
    }

    #region PRIVATE_METHODS
    static void Check(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    static void Near(PointF actual, PointF expected) => Check(Math.Abs(actual.X - expected.X) < 0.01 && Math.Abs(actual.Y - expected.Y) < 0.01,
        "Coordinate mismatch: " + actual + " != " + expected);
    static Mat CreateBoard()
    {
        var image = new Mat(300, 300, MatType.CV_8UC1, Scalar.Black);
        for (int r = 0; r < 3; r++)
            for (int c = 0; c < 3; c++)
                Cv2.Circle(image, new OpenCvSharp.Point(80 + 70 * c, 80 + 70 * r), 12, Scalar.White, -1);
        return image;
    }
    static JxCalibRecipe CreateCalibRecipe()
    {
        var recipe = new JxCalibRecipe();
        recipe.GridSettings.BoundRect.Value = new Rectangle(-10, -10, 320, 320);
        recipe.GridSettings.EmptyTraySettings.FullRows.Value = 3;
        recipe.GridSettings.EmptyTraySettings.FullCols.Value = 3;
        recipe.GridSettings.GridVisionSettings.MinSize.Value = 10;
        recipe.GridSettings.GridVisionSettings.MaxSize.Value = 50;
        recipe.GridSettings.GridVisionSettings.Threshold.Value = 128;
        return recipe;
    }
    static void CheckGrid(EzBlocsGrid grid)
    {
        Check(grid != null && grid.Rows == 3 && grid.Cols == 3, "Synthetic calibration grid not found.");
        for (int r = 0; r < 3; r++)
            for (int c = 0; c < 3; c++)
                Near(new PointF((float)grid.Get(r, c).Center.X, (float)grid.Get(r, c).Center.Y), new PointF(80 + 70 * c, 80 + 70 * r));
    }
    static void ConfigureTransforms(ITravellerTransforms transforms)
    {
        using (var image = CreateBoard())
        using (var recipe = CreateCalibRecipe())
        using (var calib = new CalibV5())
        {
            foreach (CarrierEnum carrier in Enum.GetValues(typeof(CarrierEnum)))
            {
                transforms.ConfigWorldGridPoints(carrier, 3, 3, 10, 10);
                var grid = calib.FetchBoardGrid(carrier, image, recipe);
                CheckGrid(grid);
                Check(transforms.SetCalibCamGrid(carrier, grid, null), "Camera calibration rejected."); // Ownership transfers to transforms.
                foreach (SuckerRowEnum row in Enum.GetValues(typeof(SuckerRowEnum)))
                {
                    var marks = new[] { new QVector(80, 80), new QVector(220, 80), new QVector(220, 220), new QVector(80, 220) };
                    double dx = (carrier == CarrierEnum.C1 ? 100 : 300) + (row == SuckerRowEnum.S1 ? 0 : 20);
                    double dy = (carrier == CarrierEnum.C1 ? 200 : 400) + (row == SuckerRowEnum.S1 ? 0 : 30);
                    var motor = marks.Select(v => new QVector(v.X + dx, v.Y + dy)).ToArray();
                    Check(transforms.SetCalibMotorCoords(carrier, row, marks, motor) == ErrorCodes.OK, "Motor calibration rejected.");
                }
            }
            transforms.BuildAll();
        }
    }
    static void CheckRoute((string Name, int Row, PointF First, PointF Second) call, string name, SuckerRowEnum row, PointF point)
    {
        Check(call.Name == name && call.Row == (int)row, "Wrong PLC carrier or sucker row.");
        Near(row == SuckerRowEnum.S1 ? call.First : call.Second, point);
        Near(row == SuckerRowEnum.S1 ? call.Second : call.First, PointF.Empty);
    }
    #endregion

    #region PRIVATE_TYPES
    sealed class PlcProbe : RealProxy
    {
        public readonly List<(string Name, int Row, PointF First, PointF Second)> Calls = new List<(string, int, PointF, PointF)>();
        public PlcProbe() : base(typeof(IPlcIoFPIX3)) { }
        public override IMessage Invoke(IMessage message)
        {
            var call = (IMethodCallMessage)message;
            if (call.MethodName != "SetStage1" && call.MethodName != "SetStage2")
                throw new InvalidOperationException("Unexpected PLC call: " + call.MethodName);
            // Copy arguments while the remoting call frame is alive.
            Calls.Add((call.MethodName, (int)call.Args[0], (PointF)call.Args[1], (PointF)call.Args[2]));
            return new ReturnMessage(null, null, 0, call.LogicalCallContext, call);
        }
    }
    sealed class AoiProbe : RealProxy
    {
        public int Disposals;
        public AoiProbe() : base(typeof(IProcessRunFPI)) { }
        public override IMessage Invoke(IMessage message)
        {
            var call = (IMethodCallMessage)message;
            if (call.MethodName != "Dispose") throw new InvalidOperationException("Unexpected AOI call: " + call.MethodName);
            Disposals++;
            return new ReturnMessage(null, null, 0, call.LogicalCallContext, call);
        }
    }
    sealed class CalibProbe : ICalibAoiModel
    {
        public int Disposals;
        public bool FailDispose;
        public void Dispose() { Disposals++; if (FailDispose) throw new InvalidOperationException("Calibration disposal probe."); }
        public EzBlocsGrid FetchBoardGrid(CarrierEnum c, Mat image, JxCalibRecipe recipe) => throw new NotSupportedException();
        public (EzBloc[], EzBloc[]) FetchInkMarks(CarrierEnum c, SuckerRowEnum s, Mat image, JxCalibRecipe recipe) => throw new NotSupportedException();
        public bool AdjustBadNodes(CarrierEnum c, EzBlocsGrid grid) => throw new NotSupportedException();
        public MatchResult FetchGridNodes(IxEmptyTrayInspector aoi, CarrierEnum c, Mat image, bool refine = true) => throw new NotSupportedException();
    }
    sealed class TestEnvironment : ISystemModelEnvironment
    {
        public int Reads;
        public string ActiveRecipeName { get { Reads++; throw new InvalidOperationException("Unexpected recipe read."); } }
        public string CommonBaseTransformsFile => throw new InvalidOperationException("Unexpected disk access.");
        public string GetTransformsFile(string file) => throw new InvalidOperationException("Unexpected disk access.");
        public string GetEmptyTrayModelName(string name, CarrierEnum c) => throw new InvalidOperationException("Unexpected recipe read.");
        public string GetEmptyTrayRecipeFile(string name, CarrierEnum c) => throw new InvalidOperationException("Unexpected disk access.");
    }
    #endregion
}
