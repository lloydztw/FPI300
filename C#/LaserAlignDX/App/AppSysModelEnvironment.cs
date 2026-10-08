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

using LaserAlignDX.Mvc.Model;
using Traveller106;

namespace LaserAlignDX
{
    internal sealed class AppSysModelEnvironment : ISysModelEnvironment
    {
        public string ActiveRecipeName => LtAoiFactory.GetActiveRecipeNameAtFPI30();
        public string CommonBaseTransformsFile => GaMvcPaths.COMMON_BASE_TRANSFORMS_INI_FILE;
        public string GetTransformsFile(string recipeIniFile) => GaMvcPaths.TRANSFORMS_INI_FILE(recipeIniFile);
        public string GetEmptyTrayModelName(string recipeName, CarrierEnum carrier) => LtAoiFactory.RcpStemName(recipeName, carrier);
        public string GetEmptyTrayRecipeFile(string recipeName, CarrierEnum carrier) => LtAoiFactory.RcpGetRecipeFileName(recipeName, carrier);
    }
}
