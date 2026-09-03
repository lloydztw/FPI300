using Eazy_Project_III;
using JetEazy;
using JetEazy.Lang;
using JetEazy.Utils;
using LaserAlignDX.Mvc.Model.Recipe;
using LaserAlignDX.OPSpace.RecipeSpace;
using System.ComponentModel;
using System.Drawing.Design;

namespace LaserAlignDX.BasicSpace
{
    /// <summary>
    /// 參數摘要
    /// </summary>
    public class RecipeParaGridClass
    {
        #region SINGLETON
        private RecipeParaGridClass()
        {

        }
        private static RecipeParaGridClass _instance = null;
        #endregion

        public static RecipeParaGridClass Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new RecipeParaGridClass();
                return _instance;
            }
        }

        #region PRIVATE_DATA_LINK
        RecipeFPIX3Class _xRecipe => RecipeFPIX3Class.Instance;
        InspectX3ParaClass _spec => _xRecipe.InspectParams;
        DtoX3GridParams _dtoGridParams => _xRecipe.GridParams;
        #endregion

        #region 0A_參數摘要
        const string cat0 = "A.參數摘要";
        [CategoryAttribute(cat0), DescriptionAttribute("晶粒型態")]
        [DisplayName("1.晶粒型態")]
        [Browsable(true)]
        [ReadOnly(true)]
        public string ChipTypeStr
        {
            get => GaUtil.GetEnumDescription(_spec.xAlgorithm);
        }

        [CategoryAttribute(cat0), DescriptionAttribute("Rows x Cols")]
        [DisplayName("2.格位數")]
        [Browsable(true)]
        [ReadOnly(true)]
        public string RowColStr
        {
            get => $"{xRow} x {xColumn}";
        }

        //格位間距X
        [CategoryAttribute(cat0), DescriptionAttribute("單位 mm")]
        [DisplayName("3.格位間距X")]
        [Browsable(true)]
        [ReadOnly(true)]
        public string PitchXStr
        {
            get => $"{xRealOffsetX:0.000} mm";
        }

        //格位間距Y
        [CategoryAttribute(cat0), DescriptionAttribute("單位 mm")]
        [DisplayName("4.格位間距Y")]
        [Browsable(true)]
        [ReadOnly(true)]
        public string PitchYStr
        {
            get => $"{xRealOffsetY:0.000} mm";
        }

        [CategoryAttribute(cat0), DescriptionAttribute("單位 mm")]
        [DisplayName("5.晶粒尺寸X")]
        [Browsable(true)]
        [ReadOnly(true)]
        public string ChipWidthStr
        {
            get => $"{xChipWidth:0.000} mm";
        }

        [CategoryAttribute(cat0), DescriptionAttribute("單位 mm")]
        [DisplayName("6.晶粒尺寸Y")]
        [Browsable(true)]
        [ReadOnly(true)]
        public string ChipHeightStr
        {
            get => $"{xChipHeight:0.000} mm";
        }
        #endregion

        #region 0B_檢測項目
        const string cat01 = "B.檢測項目";
        [CategoryAttribute(cat01), DescriptionAttribute("檢測項目")]
        [DisplayName("1.啟用 尺寸量測")]
        [Browsable(true)]
        public bool optChipMeasurement
        {
            get => _spec.optChipMeasurement;
            set => _spec.optChipMeasurement = value;
        }

        [CategoryAttribute(cat01), DescriptionAttribute("僅適用於 格點晶粒!")]
        [DisplayName("2.啟用 邊隙檢測")]
        [Browsable(true)]
        public bool optPadEdgeGapsMeasurement
        {
            get => _spec.optPadEdgeGapsMeasurement;
            set => _spec.optPadEdgeGapsMeasurement = value;
        }

        [CategoryAttribute(cat01), DescriptionAttribute("")]
        [DisplayName("3.啟用 缺陷檢測")]
        [Browsable(true)]
        public bool optChipDefectsInspect
        {
            get => _spec.optChipDefectsInspect;// = false;     // 暫時不開放
            set => _spec.optChipDefectsInspect = value;
        }
        #endregion

        #region 0C_其他设定
        const string cat2 = "C.其他设定";
        [CategoryAttribute(cat2), DescriptionAttribute("1-红光 2-白光 3-红白光")]
        [DisplayName("1.灯光通道")]
        [TypeConverter(typeof(JzEnumConverter))]
        [Browsable(true)]
        public LightChannelEnum xChNum
        {
            get { return (LightChannelEnum)_dtoGridParams.xChNum; }
            set { _dtoGridParams.xChNum = (int)value; }
        }

        [CategoryAttribute(cat2), DescriptionAttribute("0~255")]
        [DisplayName("2.灯光值")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 255)]
        [Browsable(true)]
        public int xChValue
        {
            get { return _dtoGridParams.xChValue; }
            set { _dtoGridParams.xChValue = value; }
        }
        #endregion

        #region 1_基础设定_內部使用之變量_為相容性_暫時保留
        //const string cat1 = "01.基础设定";
        //[CategoryAttribute(cat1), DescriptionAttribute("阵列角度")]
        //[DisplayName("A00.阵列角度")]
        //[TypeConverter(typeof(NumericUpDownTypeConverter))]
        //[Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(-180, 180)]
        //[Browsable(true)]
        [Browsable(false)]
        internal float xAngle
        {
            get { return _dtoGridParams.xAngle; }
            set { _dtoGridParams.xAngle = value; }
        }

        //[CategoryAttribute(cat1), DescriptionAttribute("矩阵行数")]
        //[DisplayName("A01.Rows 行数")]
        //[TypeConverter(typeof(NumericUpDownTypeConverter))]
        //[Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999)]
        //[Browsable(true)]
        [Browsable(false)]
        public int xRow
        {
            get { return _dtoGridParams.xRow; }
            set { _dtoGridParams.xRow = value; }
        }

        //[CategoryAttribute(cat1), DescriptionAttribute("矩阵列数")]
        //[DisplayName("A02.Cols 列数")]
        //[TypeConverter(typeof(NumericUpDownTypeConverter))]
        //[Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999)]
        //[Browsable(true)]
        [Browsable(false)]
        public int xColumn
        {
            get { return _dtoGridParams.xColumn; }
            set { _dtoGridParams.xColumn = value; }
        }

        //[CategoryAttribute(cat1), DescriptionAttribute("矩阵左上角Chip的左上角图像位置X")]
        //[DisplayName("A03.左上角X(pixel)")]
        //[TypeConverter(typeof(NumericUpDownTypeConverter))]
        //[Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999)]
        //[Browsable(true)]
        [Browsable(false)]
        public int xLeftTopX
        {
            get { return _dtoGridParams.xLeftTopX; }
            set { _dtoGridParams.xLeftTopX = value; }
        }

        //[CategoryAttribute(cat1), DescriptionAttribute("矩阵左上角Chip的左上角图像位置Y")]
        //[DisplayName("A04.左上角Y(pixel)")]
        //[TypeConverter(typeof(NumericUpDownTypeConverter))]
        //[Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999)]
        //[Browsable(true)]
        [Browsable(false)]
        public int xLeftTopY
        {
            get { return _dtoGridParams.xLeftTopY; }
            set { _dtoGridParams.xLeftTopY = value; }
        }

        //[CategoryAttribute(cat1), DescriptionAttribute("矩阵行间距")]
        //[DisplayName("A05.行间距(mm)")]
        //[TypeConverter(typeof(NumericUpDownTypeConverter))]
        //[Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 1f, 2)]
        //[Browsable(true)]
        [Browsable(false)]
        public float xRowOffset
        {
            get { return _dtoGridParams.xRowOffset; }
            set { _dtoGridParams.xRowOffset = value; }
        }

        //[CategoryAttribute(cat1), DescriptionAttribute("矩阵列间距")]
        //[DisplayName("A06.列间距(mm)")]
        //[TypeConverter(typeof(NumericUpDownTypeConverter))]
        //[Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999f, 1f, 2)]
        //[Browsable(true)]
        [Browsable(false)]
        public float xColumnOffset
        {
            get { return _dtoGridParams.xColumnOffset; }
            set { _dtoGridParams.xColumnOffset = value; }
        }

        //[CategoryAttribute(cat1), DescriptionAttribute("矩阵中产品尺寸X")]
        //[DisplayName("A07.Chip 尺寸X (mm)")]
        //[TypeConverter(typeof(NumericUpDownTypeConverter))]
        //[Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 1f, 2)]
        //[Browsable(true)]
        [Browsable(false)]
        public float xChipWidth
        {
            get { return _dtoGridParams.xChipWidth; }
            set { _dtoGridParams.xChipWidth = value; }
        }

        //[CategoryAttribute(cat1), DescriptionAttribute("矩阵中产品尺寸Y")]
        //[DisplayName("A08.Chip 尺寸Y (mm)")]
        //[TypeConverter(typeof(NumericUpDownTypeConverter))]
        //[Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999f, 1f, 2)]
        //[Browsable(true)]
        [Browsable(false)]
        public float xChipHeight
        {
            get { return _dtoGridParams.xChipHeight; }
            set { _dtoGridParams.xChipHeight = value; }
        }

        //[CategoryAttribute(cat1), DescriptionAttribute("模板匹配搜寻范围X扩大的像素")]
        //[DisplayName("A09.外扩X (pix)")]
        //[TypeConverter(typeof(NumericUpDownTypeConverter))]
        //[Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999f, 1f, 2)]
        //[Browsable(true)]
        //[ReadOnly(true)]
        //public int xExtendx
        //{
        //    get { return xRecipe.xExtendx; }
        //    set { xRecipe.xExtendx = value; }
        //}

        //[CategoryAttribute(cat1), DescriptionAttribute("模板匹配搜寻范围Y扩大的像素")]
        //[DisplayName("A10.外扩Y (pix)")]
        //[TypeConverter(typeof(NumericUpDownTypeConverter))]
        //[Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999f, 1f, 2)]
        //[Browsable(true)]
        //[ReadOnly(true)]
        //public int xExtendy
        //{
        //    get { return xRecipe.xExtendy; }
        //    set { xRecipe.xExtendy = value; }
        //}
        #endregion

        #region 3_位置矩阵_內部使用之變量_為相容性_暫時保留
        //const string cat3 = "03.位置矩阵设定";

        //[CategoryAttribute(cat3), DescriptionAttribute("")]
        //[DisplayName("A00.平台选择")]
        //[TypeConverter(typeof(JzEnumConverter))]
        //[Browsable(true)]
        //[ReadOnly(false)]
        [Browsable(false)]
        public StageNumber xStageNumber
        {
            get { return _dtoGridParams.xStageNumber; }
            set { _dtoGridParams.xStageNumber = value; }
        }

        //[CategoryAttribute(cat3), DescriptionAttribute("矩阵左上角实际位置X")]
        //[DisplayName("A01.起点X(mm)")]
        //[TypeConverter(typeof(NumericUpDownTypeConverter))]
        //[Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(-99999999, 99999999)]
        //[Browsable(true)]
        //[ReadOnly(true)]
        [Browsable(false)]
        public float xRealLeftX
        {
            get { return _dtoGridParams.xRealLeftX; }
            set { _dtoGridParams.xRealLeftX = value; }
        }

        //[CategoryAttribute(cat3), DescriptionAttribute("矩阵左上角实际位置Y")]
        //[DisplayName("A02.起点Y(mm)")]
        //[TypeConverter(typeof(NumericUpDownTypeConverter))]
        //[Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(-99999999, 99999999)]
        //[Browsable(true)]
        //[ReadOnly(true)]
        [Browsable(false)]
        public float xRealLeftY
        {
            get { return _dtoGridParams.xRealLeftY; }
            set { _dtoGridParams.xRealLeftY = value; }
        }

        //[CategoryAttribute(cat3), DescriptionAttribute("矩阵横向产品中心距离")]
        //[DisplayName("A03.横向间距(mm)")]
        //[TypeConverter(typeof(NumericUpDownTypeConverter))]
        //[Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(-99999999, 99999999)]
        //[Browsable(true)]
        [Browsable(false)]
        public float xRealOffsetX
        {
            get { return _dtoGridParams.xRealOffsetX; }
            set { _dtoGridParams.xRealOffsetX = value; }
        }

        //[CategoryAttribute(cat3), DescriptionAttribute("矩阵纵向产品中心距离")]
        //[DisplayName("A04.纵向间距(mm)")]
        //[TypeConverter(typeof(NumericUpDownTypeConverter))]
        //[Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(-99999999, 99999999)]
        //[Browsable(true)]
        [Browsable(false)]
        public float xRealOffsetY
        {
            get { return _dtoGridParams.xRealOffsetY; }
            set { _dtoGridParams.xRealOffsetY = value; }
        }
        #endregion
    }
}
