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


namespace LaserAlignDX.UISpace.CtrlSpace
{
    public static class IoPanelFactory
    {
        /// <summary>
        /// 把所有 MainXxCtrl 抽出共通 Interface, 並把生成的代碼集中至此,
        /// version + option 的 switch case 在此寫一次就好,
        /// 不要凌亂的散落各處.
        /// </summary>
        public static IoPanelUI CreateIoPanelUI(VersionEnum version, OptionEnum option, GeoMachineClass machine)
        {
            switch (version)
            {
                case VersionEnum.LASER:
                    switch (option)
                    {
#if(OPT_X1_X2)
                        case OptionEnum.MAIN_X1:
                            var mainX1 = new LaserAlignDX.UISpace.CtrlSpace.MainX1Ctrl();
                            mainX1.Initial(version, option, (MainX1MachineClass)machine);
                            return mainX1;
#endif
                        case OptionEnum.MAIN_FPIX3:
                            var mainX3 = new LaserAlignDX.UISpace.CtrlSpace.MainFPIX3Ctrl();
                            mainX3.Initial(version, option, (MainFPIX3MachineClass)machine);
                            return mainX3;
                    }
                    break;
                case VersionEnum.AOI:
                    switch (option)
                    {
#if(OPT_X1_X2)
                        case OptionEnum.MAIN_X2:
                            var mainX2 = new LaserAlignDX.UISpace.CtrlSpace.MainX2Ctrl();
                            mainX2.Initial(version, option, (MainX2MachineClass)machine);
                            return mainX2;
#endif
                    }
                    break;
            }

            return null;
        }
    }
}
