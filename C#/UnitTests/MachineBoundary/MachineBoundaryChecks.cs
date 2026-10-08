#region AUTHOR
/*
 *
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 *
 * REVISION:
 *      2026-10-08 使用 ChatGPT Codex  重整 (by LeTian Chang)
 *
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 *
 */
#endregion

using System;
using System.Linq;
using System.Threading;
using LaserAlignDX;
using Traveller.FPI30.Contracts;
using Traveller.FPI30.Machine;
using VsCommon.ControlSpace.IOSpace;
using VsCommon.ControlSpace.MachineSpace;

internal static class MachineBoundaryChecks
{
    #region PRIVATE_METHODS

    static int Main()
    {
        try
        {
            CheckAssemblyBoundary();
            CheckFactory();
            CheckMachineSimulation();
            CheckCancelledSimulation();
            Check(!AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name == "FPI30AOIX3"),
                "The checks loaded the main application.");
            Console.WriteLine("PASS standalone Machine / Contracts boundary and simulation handshake");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return 1;
        }
    }

    static void CheckAssemblyBoundary()
    {
        var contracts = typeof(IPlcIoFPIX3).Assembly;
        Check(contracts.GetName().Name == "Traveller.FPI30.Contracts", "IO contract is in the wrong assembly.");
        var allowed = new[] { "mscorlib", "System", "System.Core", "System.Drawing" };
        Check(contracts.GetReferencedAssemblies().All(a => allowed.Contains(a.Name)),
            "Contracts depends on a driver or application assembly.");
        Check(typeof(IPlcIoFPIX3).GetMethod("Initial") == null, "Driver initialization leaked into Contracts.");
        Check(typeof(CarrierEnum).Assembly == contracts && typeof(SuckerRowEnum).Assembly == contracts,
            "Shared enum ownership is incorrect.");
        Check((int)CarrierEnum.C1 == 0 && (int)CarrierEnum.C2 == 1 &&
              (int)SuckerRowEnum.S1 == 0 && (int)SuckerRowEnum.S2 == 1,
            "Persisted enum values changed.");
        var machine = typeof(MainFPIX3MachineClass).Assembly;
        Check(machine.GetName().Name == "Traveller.FPI30.Machine", "Machine is in the wrong assembly.");
        var machineAllowed = allowed.Concat(new[] { "Traveller.FPI30.Contracts", "JetEazy.Basics", "JetEazy.VG.Devices" });
        Check(machine.GetReferencedAssemblies().All(a => machineAllowed.Contains(a.Name)),
            "Machine directly depends on an unexpected assembly.");
    }

    static void CheckMachineSimulation()
    {
        // Zero device counts exercise the real DLL and simulator without opening any hardware.
        IFpix3Machine machine = Fpix3MachineFactory.Create(".", plcCount: 0, motionCount: 0, lightCount: 0);
        try
        {
            Check(machine.Initial(true, true), "Simulation initialization failed.");
            var io = machine.PLCIO;
            var sim = io as IPlcIoFPIX3Sim;
            Check(sim != null, "The simulation IO implementation was not selected.");
            sim.simActiveStage(2);
            sim.simStripID("Strip01");
            sim.simLotID("Lot01");
            Check(io.iScanStage == 2 && io.sStripID == "sim_Strip01" && io.sLotID == "sim_Lot01",
                "Simulation metadata changed.");
            Check(io.GetSafeZ(SuckerRowEnum.S1) == 12.892414, "Simulation safe Z changed.");

            using (var requested = new ManualResetEventSlim())
            {
                sim.OnRequestSimLineScan += (sender, e) => requested.Set();
                io.bSoftwareReady = true;
                Check(requested.Wait(2000), "Simulation did not request a line-scan image.");
                WaitFor(() => io.bScanStart && io.iScanStatus == 1, "Line-scan start handshake failed.");
                io.iSingleResult(new[] { 1, 1, 2, 3 });
                io.bScanDone = true;
                io.iScanResult = 1;
                WaitFor(() => io.iFlyStart == 1, "Fly-camera start handshake failed.");
                io.bFlyDone = true;
                WaitFor(() => io.bScanStart && io.iScanStatus == 3, "Empty-tray handoff failed.");
                io.iScanResult = 1;
                WaitFor(() => !io.bScanStart, "Simulation did not finish the empty-tray scan.");
            }

            io.bSoftwareReady = false;
            io.iRecipeNum = 7;
            io.ResetPlc(true);
            Check(!io.bScanStart && !io.bScanReady && !io.bScanDone && !io.bFlyDone && io.iRecipeNum == 0,
                "ResetPlc no longer resets the operation handshake.");
            machine.Tick();
            machine.PLCRetry();
            Check(machine.PLCFps() == "", "Unexpected PLC connection in zero-device simulation.");
        }
        finally
        {
            machine.Close();
        }
    }

    static void CheckFactory()
    {
        var first = Fpix3MachineFactory.Create(".");
        var second = Fpix3MachineFactory.Create(".");
        try
        {
            Check(!ReferenceEquals(first, second), "Factory must return independent instances.");
            Check(first.PLCIO == null && second.PLCIO == null, "Factory opened devices before Initial.");
            var concrete = (MainFPIX3MachineClass)first;
            Check(concrete.PLCCollection.Length == 1 && concrete.PLCMOTIONCollection.Length == 16 &&
                  concrete.LightCollection.Length == 2, "Default hardware counts changed.");
            Check(concrete.PLCCollection.All(p => p == null), "Factory opened PLC connections.");
            Check(typeof(MainFPIX3MachineClass).GetConstructors().Length == 0,
                "Machine creation should go through the factory.");
        }
        finally
        {
            // No drivers have been created by these factory-only instances.
            first.Close();
            second.Close();
        }
        Throws<ArgumentException>(() => Fpix3MachineFactory.Create(null));
        Throws<ArgumentException>(() => Fpix3MachineFactory.Create(" "));
        Throws<ArgumentOutOfRangeException>(() => Fpix3MachineFactory.Create(".", plcCount: -1));
        Throws<ArgumentOutOfRangeException>(() => Fpix3MachineFactory.Create(".", motionCount: -1));
        Throws<ArgumentOutOfRangeException>(() => Fpix3MachineFactory.Create(".", lightCount: -1));
    }

    static void Throws<T>(Action action) where T : Exception
    {
        try { action(); }
        catch (T) { return; }
        throw new InvalidOperationException("Expected " + typeof(T).Name);
    }

    static void CheckCancelledSimulation()
    {
        var io = new MainFPIX3IOSim();
        using (var requested = new ManualResetEventSlim())
        {
            io.OnRequestSimLineScan += (sender, e) => { e.Cancel = true; requested.Set(); };
            io.bSoftwareReady = true;
            Check(requested.Wait(2000), "Cancelled simulation did not raise its request.");
            Check(!SpinWait.SpinUntil(() => io.bScanStart, 100), "Cancelled simulation started scanning.");
            io.bSoftwareReady = false;
        }
    }

    static void WaitFor(Func<bool> predicate, string message)
    {
        Check(SpinWait.SpinUntil(predicate, 3000), message);
    }

    static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    #endregion
}
