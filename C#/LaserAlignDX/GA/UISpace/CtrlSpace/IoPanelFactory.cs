#region AUTHOR
/*
 *
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 *
 * REVISION:
 *      2026-10-08 使用 ChatGPT Codex  重整 (by LeTian Chang)
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
        public static IoPanelUI CreateIoPanelUI(VersionEnum version, OptionEnum option, Traveller.FPI30.Contracts.IFpix3Machine machine)
        {
            switch (version)
            {
                case VersionEnum.LASER:
                    switch (option)
                    {
                        case OptionEnum.MAIN_FPIX3:
                            var mainX3 = new LaserAlignDX.UISpace.CtrlSpace.MainFPIX3Ctrl();
                            mainX3.Initial(version, option, (MainFPIX3MachineClass)machine);
                            return mainX3;
                    }
                    break;
            }

            return null;
        }
    }
}
