#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-09-16 重整 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy;
using VsCommon.ControlSpace.MachineSpace;


namespace LaserAlignDX.UISpace
{
    public static class MainUiFactory
    {
        /// <summary>
        /// 把所有 MainXxUI 抽出共通 Interface, 並把生成的代碼集中至此,
        /// version + option 的 switch case 在此寫一次就好,
        /// 不要凌亂的散落各處.
        /// </summary>
        public static IMainUI CreateMainUI(VersionEnum version, OptionEnum option, GeoMachineClass machine)
        {
            switch (version)
            {
                case VersionEnum.LASER:
                    switch (option)
                    {
                        case OptionEnum.MAIN_X1:
                            return new LaserAlignDX.UISpace.MainSpace.MainX1UI();
                        case OptionEnum.MAIN_FPIX3:
                            return new LaserAlignDX.UISpace.MainSpace.MainX3UI();
                    }
                    break;
                case VersionEnum.AOI:
                    switch (option)
                    {
                        case OptionEnum.MAIN_X2:
                            return new LaserAlignDX.UISpace.MainSpace.MainX2UI();
                    }
                    break;
            }
            return null;
        }
    }
}
