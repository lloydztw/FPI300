using LaserAlignDX.Mvc.Model.Recipe;
using System.Drawing;

namespace LaserAlignDX.OPSpace.RecipeSpace
{
    partial class RecipeFPIX3Class
    {
        #region 邊線_手拉框區塊_LINE_BORDER_BOXES
        public readonly DtoX3LineBorderParams LineBorderParams = new DtoX3LineBorderParams();

        #region 舊接口
        /// <summary>
        /// 邊線框(左)
        /// </summary>
        public RectangleF xLineLeft
        {
            get => LineBorderParams.xLineLeft;
            set => LineBorderParams.xLineLeft = value;
        }
        /// <summary>
        /// 邊線框(上)
        /// </summary>
        public RectangleF xLineTop
        {
            get => LineBorderParams.xLineTop;
            set => LineBorderParams.xLineTop = value;
        }
        /// <summary>
        /// 邊線框(右)
        /// </summary>
        public RectangleF xLineRight
        {
            get => LineBorderParams.xLineRight;
            set => LineBorderParams.xLineRight = value;
        }
        /// <summary>
        /// 邊線框(下)
        /// </summary>
        public RectangleF xLineBottom
        {
            get => LineBorderParams.xLineBottom;
            set => LineBorderParams.xLineBottom = value;
        }
        #endregion

        void LoadLineBorderParams(string carrierTag)
        {
#if (OPT_LEGACY_SIMPLE_LINE_BORDERS)
            string sectName = "Recipe Basic" + carrierTag;
            xLineLeft = StringtoRectF(ReadINIValue(sectName, "xLineLeft", RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), INIFILE));
            xLineTop = StringtoRectF(ReadINIValue(sectName, "xLineTop", RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), INIFILE));
            xLineRight = StringtoRectF(ReadINIValue(sectName, "xLineRight", RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), INIFILE));
            xLineBottom = StringtoRectF(ReadINIValue(sectName, "xLineBottom", RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), INIFILE));
#endif
            LineBorderParams.CarrierTag = _CARRIER_TAG;
            LineBorderParams.Load(INIFILE);

            GapBorderParams.CarrierTag = _CARRIER_TAG;
            GapBorderParams.Load(INIFILE);
        }
        void SaveLineBorderParams(string carrierTag)
        {
#if (OPT_LEGACY_SIMPLE_LINE_BORDERS)
            string sectName = "Recipe Basic" + carrierTag;
            WriteINIValue(sectName, "xLineLeft", RectFtoStringSimple(xLineLeft), INIFILE);
            WriteINIValue(sectName, "xLineTop", RectFtoStringSimple(xLineTop), INIFILE);
            WriteINIValue(sectName, "xLineRight", RectFtoStringSimple(xLineRight), INIFILE);
            WriteINIValue(sectName, "xLineBottom", RectFtoStringSimple(xLineBottom), INIFILE);
#endif
            LineBorderParams.CarrierTag = _CARRIER_TAG;
            LineBorderParams.Save(INIFILE);

            GapBorderParams.CarrierTag = _CARRIER_TAG;
            GapBorderParams.Save(INIFILE);
        }
        #endregion
    }
}
