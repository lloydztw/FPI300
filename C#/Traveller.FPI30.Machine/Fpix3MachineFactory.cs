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
using Traveller.FPI30.Config;
using VsCommon.ControlSpace.MachineSpace;

namespace Traveller.FPI30.Machine
{
    /// <summary>Creates independent FPIX3 instances without opening hardware connections.</summary>
    public static class Fpix3MachineFactory
    {
        /// <summary>
        /// Creates a machine using the MAIN_FPIX3 subdirectory of workPath.
        /// The caller owns the instance and calls Initial, Tick and Close.
        /// Zero counts support hardware-free simulation.
        /// </summary>
        public static IFpix3Machine Create(string workPath, int plcCount = 1, int motionCount = 16, int lightCount = 2)
        {
            if (string.IsNullOrWhiteSpace(workPath))
                throw new ArgumentException("A working directory is required.", nameof(workPath));
            if (plcCount < 0)
                throw new ArgumentOutOfRangeException(nameof(plcCount));
            if (motionCount < 0)
                throw new ArgumentOutOfRangeException(nameof(motionCount));
            if (lightCount < 0)
                throw new ArgumentOutOfRangeException(nameof(lightCount));

            return new MainFPIX3MachineClass(workPath, plcCount, motionCount, lightCount);
        }
    }
}
