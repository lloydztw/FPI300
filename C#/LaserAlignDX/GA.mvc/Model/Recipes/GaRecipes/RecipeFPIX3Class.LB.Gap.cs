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
