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
using LaserAlignDX.Mvc.Model;
using VsCommon.ControlSpace.IOSpace;

namespace LaserAlignDX
{
    /// <summary>Preserves the FPIX3 carrier/sucker-row routing at the application hardware boundary.</summary>
    internal sealed class AppCoordinateWriter : ICoordinateWriter
    {
        #region PRIVATE_DATA
        readonly Func<IPlcIoFPIX3> _getPlc;
        #endregion

        public AppCoordinateWriter() : this(() => Traveller106.Universal.Machine?.PLCIO) { }

        public AppCoordinateWriter(Func<IPlcIoFPIX3> getPlc)
        {
            _getPlc = getPlc ?? throw new ArgumentNullException(nameof(getPlc));
        }

        public bool IsAvailable => _getPlc() != null;

        public void Write(CarrierEnum carrier, SuckerRowEnum suckerRow, PointF coordinate)
        {
            var plc = _getPlc();
            var unused = PointF.Empty;
            if (carrier == CarrierEnum.C1 && suckerRow == SuckerRowEnum.S1)
                plc?.SetStage1((int)suckerRow, coordinate, unused);
            else if (carrier == CarrierEnum.C1 && suckerRow == SuckerRowEnum.S2)
                plc?.SetStage1((int)suckerRow, unused, coordinate);
            else if (carrier == CarrierEnum.C2 && suckerRow == SuckerRowEnum.S1)
                plc?.SetStage2((int)suckerRow, coordinate, unused);
            else if (carrier == CarrierEnum.C2 && suckerRow == SuckerRowEnum.S2)
                plc?.SetStage2((int)suckerRow, unused, coordinate);
        }
    }
}
