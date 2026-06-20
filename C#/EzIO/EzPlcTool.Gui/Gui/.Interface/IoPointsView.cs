#region AUTHOR
/*
 * EzIO GUI
 * Copyright (C) 2026
 * 2026-04-11 created by LeTian Chang
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzIO.Device;
using EzIO.Mem;
using System.Collections.Generic;
using System.Windows.Forms;

namespace EzIO.Gui
{
    public interface IoPointsView
    {
        Control Window { get; }
        Control lblSamplingRate { get; }

        void Attach(IEnumerable<IoPoint> ioPoints, IAutoScan autoScan);

        void Update(IEnumerable<IoPoint> ioPoints);
    }
}
