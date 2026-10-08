#region AUTHOR
/*
 *
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 *
 * REVISION:
 *      2026-10-08 使用 ChatGPT Codex  重整
 *      2025-09-16 重整 (by LeTian Chang)
 *
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 *
 */
#endregion

using JetEazy;


namespace LaserAlignDX.UISpace
{
    public static class MainUiFactory
    {
        /// <summary>
        /// 把所有 MainXxUI 抽出共通 Interface, 並把生成的代碼集中至此,
        /// version + option 的 switch case 在此寫一次就好,
        /// 不要凌亂的散落各處.
        /// </summary>
        public static IMainUI CreateMainUI(VersionEnum version, OptionEnum option, Traveller.FPI30.Contracts.IFpix3Machine machine)
        {
            return new LaserAlignDX.UISpace.MainSpace.MainX3UI();

        }
    }
}
