using Eazy_Project_III;
using JetEazy;
using LaserAlignDX.Mvc.Model.Recipe;
using System.ComponentModel;
using System.Drawing.Design;

namespace LaserAlignDX.BasicSpace
{
    public class RecipeParaGridClass
    {
        #region PRIVATE_DATA
        DtoX3GridParams _dto = new DtoX3GridParams();
        static RecipeParaGridClass _instance;
        RecipeParaGridClass()
        {
        }
        #endregion

        public static RecipeParaGridClass Instance
        {
            get
            {
                if (_instance == null)
                    _instance= new RecipeParaGridClass();
                return _instance;
            }
        }

        const string cat1 = "01.基础设定";
        [CategoryAttribute(cat1), DescriptionAttribute("阵列角度")]
        [DisplayName("A00.阵列角度")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(-180, 180)]
        [Browsable(true)]
        public float xAngle
        {
            get { return _dto.xAngle; }
            set { _dto.xAngle = value; }
        }

        [CategoryAttribute(cat1), DescriptionAttribute("矩阵行数")]
        [DisplayName("A01.行数")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999)]
        [Browsable(true)]
        public int xRow
        {
            get { return _dto.xRow; }
            set { _dto.xRow = value; }
        }

        [CategoryAttribute(cat1), DescriptionAttribute("矩阵列数")]
        [DisplayName("A02.列数")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999)]
        [Browsable(true)]
        public int xColumn
        {
            get { return _dto.xColumn; }
            set { _dto.xColumn = value; }
        }

        [CategoryAttribute(cat1), DescriptionAttribute("矩阵左上角Chip的左上角图像位置X")]
        [DisplayName("A03.左上角X(pixel)")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999)]
        [Browsable(true)]
        public int xLeftTopX
        {
            get { return _dto.xLeftTopX; }
            set { _dto.xLeftTopX = value; }
        }

        [CategoryAttribute(cat1), DescriptionAttribute("矩阵左上角Chip的左上角图像位置Y")]
        [DisplayName("A04.左上角Y(pixel)")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999)]
        [Browsable(true)]
        public int xLeftTopY
        {
            get { return _dto.xLeftTopY; }
            set { _dto.xLeftTopY = value; }
        }

        [CategoryAttribute(cat1), DescriptionAttribute("矩阵行间距")]
        [DisplayName("A05.行间距(mm)")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 1f, 2)]
        [Browsable(true)]
        public float xRowOffset
        {
            get { return _dto.xRowOffset; }
            set { _dto.xRowOffset = value; }
        }

        [CategoryAttribute(cat1), DescriptionAttribute("矩阵列间距")]
        [DisplayName("A06.列间距(mm)")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999f, 1f, 2)]
        [Browsable(true)]
        public float xColumnOffset
        {
            get { return _dto.xColumnOffset; }
            set { _dto.xColumnOffset = value; }
        }

        [CategoryAttribute(cat1), DescriptionAttribute("矩阵中产品尺寸X")]
        [DisplayName("A07.Chip 尺寸X (mm)")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 1f, 2)]
        [Browsable(true)]
        public float xChipWidth
        {
            get { return _dto.xChipWidth; }
            set { _dto.xChipWidth = value; }
        }

        [CategoryAttribute(cat1), DescriptionAttribute("矩阵中产品尺寸Y")]
        [DisplayName("A08.Chip 尺寸Y (mm)")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999f, 1f, 2)]
        [Browsable(true)]
        public float xChipHeight
        {
            get { return _dto.xChipHeight; }
            set { _dto.xChipHeight = value; }
        }

        [CategoryAttribute(cat1), DescriptionAttribute("模板匹配搜寻范围X扩大的像素")]
        [DisplayName("A09.外扩X(pix)")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999f, 1f, 2)]
        [Browsable(true)]
        public int xExtendx
        {
            get { return _dto.xExtendx; }
            set { _dto.xExtendx = value; }
        }

        [CategoryAttribute(cat1), DescriptionAttribute("模板匹配搜寻范围Y扩大的像素")]
        [DisplayName("A10.外扩Y(pix)")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999f, 1f, 2)]
        [Browsable(true)]
        public int xExtendy
        {
            get { return _dto.xExtendy; }
            set { _dto.xExtendy = value; }
        }

        const string cat2 = "02.其他设定";
        [CategoryAttribute(cat2), DescriptionAttribute("1-红光 2-白光 3-红白光")]
        [DisplayName("A01.灯光通道")]
        [TypeConverter(typeof(JzEnumConverter))]
        [Browsable(true)]
        public LightChannelEnum xChNum
        {
            get { return (LightChannelEnum)_dto.xChNum; }
            set { _dto.xChNum = (int)value; }
        }

        [CategoryAttribute(cat2), DescriptionAttribute("0~255")]
        [DisplayName("A02.灯光值")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 255)]
        [Browsable(true)]
        public int xChValue
        {
            get { return _dto.xChValue; }
            set { _dto.xChValue = value; }
        }


        const string cat3 = "03.位置矩阵设定";
        [CategoryAttribute(cat3), DescriptionAttribute("")]
        [DisplayName("A00.平台选择")]
        [TypeConverter(typeof(JzEnumConverter))]
        [Browsable(true)]
        [ReadOnly(false)]
        public StageNumber xStageNumber
        {
            get { return (StageNumber)_dto.xStageNumber; }
            set { _dto.xStageNumber = (int)value; }
        }

        [CategoryAttribute(cat3), DescriptionAttribute("矩阵左上角实际位置X")]
        [DisplayName("A01.起点X(mm)")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(-99999999, 99999999)]
        [Browsable(true)]
        [ReadOnly(true)]
        public float xRealLeftX
        {
            get { return _dto.xRealLeftX; }
            set { _dto.xRealLeftX = value; }
        }

        [CategoryAttribute(cat3), DescriptionAttribute("矩阵左上角实际位置Y")]
        [DisplayName("A02.起点Y(mm)")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(-99999999, 99999999)]
        [Browsable(true)]
        [ReadOnly(true)]
        public float xRealLeftY
        {
            get { return _dto.xRealLeftY; }
            set { _dto.xRealLeftY = value; }
        }

        [CategoryAttribute(cat3), DescriptionAttribute("矩阵横向产品中心距离")]
        [DisplayName("A03.横向间距(mm)")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(-99999999, 99999999)]
        [Browsable(true)]
        public float xRealOffsetX
        {
            get { return _dto.xRealOffsetX; }
            set { _dto.xRealOffsetX = value; }
        }

        [CategoryAttribute(cat3), DescriptionAttribute("矩阵纵向产品中心距离")]
        [DisplayName("A04.纵向间距(mm)")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(-99999999, 99999999)]
        [Browsable(true)]
        public float xRealOffsetY
        {
            get { return _dto.xRealOffsetY; }
            set { _dto.xRealOffsetY = value; }
        }

        internal void Load(string iniFile)
        {
            _dto.Load(iniFile);
        }
        internal void Save(string iniFile)
        {
            _dto.Save(iniFile);
        }
    }
}
