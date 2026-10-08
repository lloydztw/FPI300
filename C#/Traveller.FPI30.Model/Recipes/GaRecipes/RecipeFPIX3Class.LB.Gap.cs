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

using LaserAlignDX.Mvc.Model.Recipe;

namespace LaserAlignDX.OPSpace.RecipeSpace
{
    partial class RecipeFPIX3Class
    {
        /// <summary>
        /// 邊隙_手拉框_參數_GapBorderParams
        /// </summary>
        public readonly DtoX3LineGapBorderParams GapBorderParams = new DtoX3LineGapBorderParams();

        void LoadGapBorderParams(string carrierTag)
        {
            GapBorderParams.CarrierTag = _CARRIER_TAG;
            GapBorderParams.Load(INIFILE);
        }
        void SaveGapBorderParams(string carrierTag)
        {
            GapBorderParams.CarrierTag = _CARRIER_TAG;
            GapBorderParams.Save(INIFILE);
        }
    }
}
