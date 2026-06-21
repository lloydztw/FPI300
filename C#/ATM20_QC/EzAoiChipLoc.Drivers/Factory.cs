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

using EzAoiChipLocQC.Machine;
using EzCamera.Driver.Sim;
using EzCamera.Interface;

namespace EzAoiChipLocQC.Drivers
{
    public static class Factory
    {
        public static ITravellerQcMachine InstanceMachine()
        {
            return Traveller_Atm20_Machine.Instance;
        }

        public static IEzCamera OpenCamera(string iniFileName)
        {
            var factory = new EzSimCameraFactory();
            var camera = factory.LoadCamera(0);
            return camera;
        }
    }
}
