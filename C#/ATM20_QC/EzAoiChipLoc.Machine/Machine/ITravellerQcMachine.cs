#region AUTHOR
/*
 * 
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2026-06-21 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzAoiChipLocQC.Drivers.IO;
using EzComm;
using JetEazy.Drivers;
using System;

namespace EzAoiChipLocQC.Machine
{
    public interface ITravellerQcMachine : IDisposable
    {
        //event EventHandler OnError;

        //ILightDriver Light { get; }
        IPlcAtm20 PLC { get; }

        void Init(EzTcpIpSettings tcpIoSettings);
    }
}
