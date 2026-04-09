using Eazy_Project_III;
using JetEazy;
using LaserAlignDX.OPSpace.RecipeSpace;
using System.ComponentModel;
using System.Drawing.Design;

namespace LaserAlignDX.BasicSpace
{
    public class RecipeParaGridClass
    {
        RecipeFPIX3Class xRecipe
        {
            get { return RecipeFPIX3Class.Instance; }
        }

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

        #region 01_基础设定_目前都只是唯讀
        const string cat1 = "01.基础设定";
        [CategoryAttribute(cat1), DescriptionAttribute("阵列角度")]
        [DisplayName("A00.阵列角度")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(-180, 180)]
        [Browsable(true)]
        public float xAngle
        {
            get { return xRecipe.xAngle; }
            set { xRecipe.xAngle = value; }
        }

        [CategoryAttribute(cat1), DescriptionAttribute("矩阵行数")]
        [DisplayName("A01.Rows 行数")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999)]
        [Browsable(true)]
        public int xRow
        {
            get { return xRecipe.xRow; }
            set { xRecipe.xRow = value; }
        }

        [CategoryAttribute(cat1), DescriptionAttribute("矩阵列数")]
        [DisplayName("A02.Cols 列数")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999)]
        [Browsable(true)]
        public int xColumn
        {
            get { return xRecipe.xColumn; }
            set { xRecipe.xColumn = value; }
        }

        [CategoryAttribute(cat1), DescriptionAttribute("矩阵左上角Chip的左上角图像位置X")]
        [DisplayName("A03.左上角X(pixel)")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999)]
        [Browsable(true)]
        public int xLeftTopX
        {
            get { return xRecipe.xLeftTopX; }
            set { xRecipe.xLeftTopX = value; }
        }

        [CategoryAttribute(cat1), DescriptionAttribute("矩阵左上角Chip的左上角图像位置Y")]
        [DisplayName("A04.左上角Y(pixel)")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999)]
        [Browsable(true)]
        public int xLeftTopY
        {
            get { return xRecipe.xLeftTopY; }
            set { xRecipe.xLeftTopY = value; }
        }

        [CategoryAttribute(cat1), DescriptionAttribute("矩阵行间距")]
        [DisplayName("A05.行间距(mm)")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 1f, 2)]
        [Browsable(true)]
        public float xRowOffset
        {
            get { return xRecipe.xRowOffset; }
            set { xRecipe.xRowOffset = value; }
        }

        [CategoryAttribute(cat1), DescriptionAttribute("矩阵列间距")]
        [DisplayName("A06.列间距(mm)")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999f, 1f, 2)]
        [Browsable(true)]
        public float xColumnOffset
        {
            get { return xRecipe.xColumnOffset; }
            set { xRecipe.xColumnOffset = value; }
        }

        [CategoryAttribute(cat1), DescriptionAttribute("矩阵中产品尺寸X")]
        [DisplayName("A07.Chip 尺寸X (mm)")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 1f, 2)]
        [Browsable(true)]
        public float xChipWidth
        {
            get { return xRecipe.xChipWidth; }
            set { xRecipe.xChipWidth = value; }
        }

        [CategoryAttribute(cat1), DescriptionAttribute("矩阵中产品尺寸Y")]
        [DisplayName("A08.Chip 尺寸Y (mm)")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999f, 1f, 2)]
        [Browsable(true)]
        public float xChipHeight
        {
            get { return xRecipe.xChipHeight; }
            set { xRecipe.xChipHeight = value; }
        }

        [CategoryAttribute(cat1), DescriptionAttribute("模板匹配搜寻范围X扩大的像素")]
        [DisplayName("A09.外扩X (pix)")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999f, 1f, 2)]
        [Browsable(true)]
        [ReadOnly(true)]
        public int xExtendx
        {
            get { return xRecipe.xExtendx; }
            set { xRecipe.xExtendx = value; }
        }

        [CategoryAttribute(cat1), DescriptionAttribute("模板匹配搜寻范围Y扩大的像素")]
        [DisplayName("A10.外扩Y (pix)")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999f, 1f, 2)]
        [Browsable(true)]
        [ReadOnly(true)]
        public int xExtendy
        {
            get { return xRecipe.xExtendy; }
            set { xRecipe.xExtendy = value; }
        }
        #endregion

        #region 02_其他设定
        const string cat2 = "02.其他设定";
        [CategoryAttribute(cat2), DescriptionAttribute("1-红光 2-白光 3-红白光")]
        [DisplayName("A01.灯光通道")]
        [TypeConverter(typeof(JzEnumConverter))]
        [Browsable(true)]
        public LightChannelEnum xChNum
        {
            get { return (LightChannelEnum)xRecipe.xChNum; }
            set { xRecipe.xChNum = (int)value; }
        }

        [CategoryAttribute(cat2), DescriptionAttribute("0~255")]
        [DisplayName("A02.灯光值")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 255)]
        [Browsable(true)]
        public int xChValue
        {
            get { return xRecipe.xChValue; }
            set { xRecipe.xChValue = value; }
        }

        [CategoryAttribute(cat2), DescriptionAttribute("線掃相機工作高度 (對焦在載台表面)")]
        [DisplayName("A03.相機高度1 (載台) mm")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(-99999999, 99999999)]
        [Browsable(true)]
        [ReadOnly(true)]
        public float zFocusOnCarrier
        {
            get { return xRecipe.zFocusOnCarrier; }
            set { xRecipe.zFocusOnCarrier = value; }
        }

        [CategoryAttribute(cat2), DescriptionAttribute("線掃相機工作高度 (對焦在晶粒表面)")]
        [DisplayName("A04.相機高度2 (晶粒) mm")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(-99999999, 99999999)]
        [Browsable(true)]
        [ReadOnly(true)]
        public float zFocusOnChip
        {
            get { return xRecipe.zFocusOnChip; }
            set { xRecipe.zFocusOnChip = value; }
        }
        #endregion

        #region 03_位置矩阵设定_目前都只是唯讀
        const string cat3 = "03.位置矩阵设定";

        [CategoryAttribute(cat3), DescriptionAttribute("")]
        [DisplayName("A00.平台选择")]
        [TypeConverter(typeof(JzEnumConverter))]
        [Browsable(true)]
        [ReadOnly(false)]
        public StageNumber xStageNumber
        {
            get { return xRecipe.xStageNumber; }
            set { xRecipe.xStageNumber = value; }
        }

        [CategoryAttribute(cat3), DescriptionAttribute("矩阵左上角实际位置X")]
        [DisplayName("A01.起点X(mm)")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(-99999999, 99999999)]
        [Browsable(true)]
        [ReadOnly(true)]
        public float xRealLeftX
        {
            get { return xRecipe.xRealLeftX; }
            set { xRecipe.xRealLeftX = value; }
        }

        [CategoryAttribute(cat3), DescriptionAttribute("矩阵左上角实际位置Y")]
        [DisplayName("A02.起点Y(mm)")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(-99999999, 99999999)]
        [Browsable(true)]
        [ReadOnly(true)]
        public float xRealLeftY
        {
            get { return xRecipe.xRealLeftY; }
            set { xRecipe.xRealLeftY = value; }
        }

        [CategoryAttribute(cat3), DescriptionAttribute("矩阵横向产品中心距离")]
        [DisplayName("A03.横向间距(mm)")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(-99999999, 99999999)]
        [Browsable(true)]
        public float xRealOffsetX
        {
            get { return xRecipe.xRealOffsetX; }
            set { xRecipe.xRealOffsetX = value; }
        }

        [CategoryAttribute(cat3), DescriptionAttribute("矩阵纵向产品中心距离")]
        [DisplayName("A04.纵向间距(mm)")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(-99999999, 99999999)]
        [Browsable(true)]
        public float xRealOffsetY
        {
            get { return xRecipe.xRealOffsetY; }
            set { xRecipe.xRealOffsetY = value; }
        }
        #endregion
    }
}
