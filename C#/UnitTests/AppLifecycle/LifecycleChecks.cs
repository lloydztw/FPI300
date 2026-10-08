#region AUTHOR
/*
 *
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 *
 * REVISION:
 *      2026-10-08 使用 ChatGPT Codex  重整
 *
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 *
 */
#endregion

// Standalone lifecycle checks: compile the production bootstrapper/scope with these
// fake hardware/model boundaries. No camera, PLC, vendor DLL or existing test project is used.
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using LaserAlignDX;

internal static class Probe
{
    internal static readonly List<string> Events = new List<string>();
    internal static string Scenario;
    internal static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }
    internal static void Equal(string expected)
    {
        Check(string.Join(",", Events) == expected, "Unexpected events: " + string.Join(",", Events));
    }
    internal static void Throws<T>(Action action) where T : Exception
    {
        try { action(); }
        catch (T) { return; }
        throw new Exception("Expected " + typeof(T).Name);
    }

    #region PRIVATE_METHODS

    [STAThread]
    static int Main(string[] args)
    {
        try
        {
            Scenario = args[0];
            if (Scenario == "scope")
            {
                var scope = new AppResourceScope();
                scope.Add(() => Events.Add("first"));
                scope.Add(() => { Events.Add("failure"); throw new InvalidOperationException(); });
                scope.Add(() => Events.Add("last"));
                Throws<AggregateException>(scope.Dispose);
                scope.Dispose();
                Equal("last,failure,first");
                Throws<ObjectDisposedException>(() => scope.Add(() => { }));
            }
            else
            {
                // Each scenario runs in a separate process, just like an application session.
                var app = AppBootstrapper.Instance;
                Equal(""); // Creating the root must not initialize models or hardware.
                if (Scenario == "unused")
                {
                    app.Dispose();
                    Equal("");
                }
                else if (Scenario == "init-failure")
                {
                    Throws<InvalidOperationException>(() => app.InitializeMachine(0));
                    app.Dispose();
                    Equal("hardware.init,hardware.dispose,recipe.dispose");
                }
                else if (Scenario == "system-failure")
                {
                    app.InitializeMachine(0);
                    Throws<InvalidOperationException>(() => { var system = app.SystemModel; });
                    app.Dispose();
                    Equal("hardware.init,aoi.create,system.create,system.shared.dispose,hardware.dispose,aoi.dispose,recipe.dispose");
                }
                else
                {
                    app.InitializeMachine(0);
                    Throws<InvalidOperationException>(() => app.InitializeMachine(0));
                    var controller = app.MainController;
                    Check(ReferenceEquals(controller, app.MainController), "Controller was recreated.");
                    Check(ReferenceEquals(app.SystemModel, app.SystemModel), "System model was recreated.");
                    Check(ReferenceEquals(app.Aoi, ((LaserAlignDX.Mvc.Model.TravellerSysModel)app.SystemModel).Aoi),
                        "Different AOI instances were injected.");
                    if (Scenario == "cleanup-failure")
                        Throws<AggregateException>(app.Dispose);
                    else
                        app.Dispose();
                    Equal("hardware.init,aoi.create,system.create,controller.create,controller.dispose,hardware.dispose,aoi.dispose,system.dispose,recipe.dispose");
                }
                var before = string.Join(",", Events);
                app.Dispose();
                AppBootstrapper.Shutdown();
                Check(before == string.Join(",", Events), "Repeated shutdown disposed a resource twice.");
                Throws<ObjectDisposedException>(() => { var model = app.SystemModel; });
                Throws<ObjectDisposedException>(() => { var model = app.Aoi; });
                Throws<ObjectDisposedException>(() => { var controller = app.MainController; });
                Throws<ObjectDisposedException>(() => app.CreateCalibAoiModel());
            }
            Console.WriteLine("PASS " + Scenario);
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return 1;
        }
    }

    #endregion
}

namespace LaserAlignDX.AoiModel
{
    public interface IProcessRunFPI : IDisposable { }
    public interface ICalibAoiModel : IDisposable { }
}
namespace LaserAlignDX.AoiModel.V3
{
    public sealed class ProcessRunFPIClass : IProcessRunFPI
    {
        #region PRIVATE_DATA

        static ProcessRunFPIClass _instance;

        #endregion

        public static ProcessRunFPIClass Instance
        {
            get
            {
                if (_instance == null) { Probe.Events.Add("aoi.create"); _instance = new ProcessRunFPIClass(); }
                return _instance;
            }
        }
        public static void DisposeAll() { _instance.Dispose(); _instance = null; }
        public void Dispose() { Probe.Events.Add("aoi.dispose"); }
    }
}
namespace LaserAlignDX.AoiModel.Calib.V5
{
    public sealed class CalibAoiModel : ICalibAoiModel { public void Dispose() { } }
}
namespace LaserAlignDX.Model
{
    public interface IxReportBuilder { }
    public sealed class XReportBuilderProxy : IxReportBuilder { }
}
namespace LaserAlignDX.Mvc.Model
{
    public interface ITravelerModel : IDisposable { }
    public sealed class TravellerSysModel : ITravelerModel
    {
        internal readonly AoiModel.IProcessRunFPI Aoi;
        public TravellerSysModel(AoiModel.IProcessRunFPI aoi, Func<AoiModel.ICalibAoiModel> factory)
        {
            Probe.Events.Add("system.create");
            if (Probe.Scenario == "system-failure") throw new InvalidOperationException();
            Aoi = aoi;
        }
        public void Dispose() { Probe.Events.Add("system.dispose"); }
        internal static void DisposeSharedResources() { Probe.Events.Add("system.shared.dispose"); }
    }
}
namespace LaserAlignDX.Mvc.Ctrl.Abs
{
    public abstract class GaMainCtrl : IDisposable { public abstract void Dispose(); }
}
namespace LaserAlignDX.Mvc.Ctrl.V3
{
    public sealed class GaMainCtrl : Abs.GaMainCtrl
    {
        public GaMainCtrl(Model.ITravelerModel model) { Probe.Events.Add("controller.create"); }
        public override void Dispose()
        {
            Probe.Events.Add("controller.dispose");
            if (Probe.Scenario == "cleanup-failure") throw new InvalidOperationException();
        }
    }
}
namespace LaserAlignDX.OPSpace.RecipeSpace
{
    public static class RecipeFPIX3Class { public static void DisposeAll() { Probe.Events.Add("recipe.dispose"); } }
}
namespace LaserAlignDX
{
    internal sealed class AppDialogService { internal AppDialogService(Func<Mvc.Model.ITravelerModel> factory) { } }
}
namespace Traveller106
{
    public sealed class FormMainDX : Form { }
    public static class Universal
    {
        public static bool InitializeCore(int language)
        {
            Probe.Events.Add("hardware.init");
            if (Probe.Scenario == "init-failure") throw new InvalidOperationException();
            return true;
        }
        public static void DisposeHardware() { Probe.Events.Add("hardware.dispose"); }
    }
}
