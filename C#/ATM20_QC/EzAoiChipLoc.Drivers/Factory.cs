#region AUTHOR
/*
 * 
 * Copyright (c) 2024 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2024-09-25 初稿 (by LeTian Chang)
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
using JetEazy.Drivers.Light;

namespace EzAoiChipLocQC.Drivers
{
    public static class Factory
    {
        public static IPlcAtm20 OpenPLC(EzTcpIpSettings settings)
        {
            return null;
        }
        public static ILightDriver OpenLight(string iniFile, bool isSim)
        {
            var light = new EzLightSim();
            return light;
        }
    }
}
