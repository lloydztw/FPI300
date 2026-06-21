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

namespace JetEazy.Drivers.Light
{
    public static class Factory
    {
        public static ILightDriver InstanceLightDriver(string iniFileName, bool isSim)
        {
            var light = new EzLightSim();
            return light;
        }
    }
}
