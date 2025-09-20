using Common.RecipeSpace;
using Eazy_Project_III;
using JetEazy;
using LaserAlignDX.AoiModel;
using LaserAlignDX.BasicSpace;
using LaserAlignDX.Mvc.Model.Recipe;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;


namespace LaserAlignDX.OPSpace.RecipeSpace
{
    public class InspectX3ParaClass : RecipeBaseClass
    {
        #region PRIVATE_DATA
        DtoX3MeasureSpec _spec = new DtoX3MeasureSpec();
        DtoX3Inspect _dto = new DtoX3Inspect();
        #endregion

        #region SINGLETON
        protected InspectX3ParaClass()
        {

        }
        private static InspectX3ParaClass _instance = null;
        #endregion

        public static InspectX3ParaClass Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new InspectX3ParaClass();
                return _instance;
            }
        }

        #region 启用设置
        const string _Cat0 = "A00.启用设置";
        [CategoryAttribute(_Cat0), DescriptionAttribute("")]
        [DisplayName("A01.开启尺寸测量")]
        [Browsable(true)]
        public bool bOpenLineMeasure
        {
            get => _spec.bOpenLineMeasure;
            set => _spec.bOpenLineMeasure = value;
        }

        [CategoryAttribute(_Cat0), DescriptionAttribute("")]
        [DisplayName("A02.开启缺陷检测")]
        [Browsable(true)]
        public bool bCheckInspect
        {
            get => _spec.bCheckInspect;
            set => _spec.bCheckInspect = value;
        }

        [CategoryAttribute(_Cat0), DescriptionAttribute("")]
        [DisplayName("A03.开启尺寸偏移检测")]
        [Browsable(true)]
        public bool bCheckMeasureOffset
        {
            get => _spec.bCheckMeasureOffset;
            set => _spec.bCheckMeasureOffset = value;
        }
        #endregion

        #region 晶粒定位
        const string _Cat1 = "A01.晶粒定位";
        [CategoryAttribute(_Cat1), DescriptionAttribute("模板轮廓匹配的演算法")]
        [DisplayName("A00.演算法")]
        [TypeConverter(typeof(JzEnumConverter))]
        [Browsable(true)]
        public MatchAlgorithmEnum xAlgorithm
        {
            get => _dto.xAlgorithm;
            set => _dto.xAlgorithm = value;
        }

        [CategoryAttribute(_Cat1), DescriptionAttribute("模板轮廓匹配的格點門限")]
        [DisplayName("A01.格點門限")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 255)]
        [Browsable(true)]
        public int xGridPadThreshold
        {
            get => _dto.xGridPadThreshold;
            set => _dto.xGridPadThreshold = value;
        }

        [CategoryAttribute(_Cat1), DescriptionAttribute("模板轮廓匹配的相似程度")]
        [DisplayName("A02.相似度")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 1, 0.1f, 2)]
        [Browsable(true)]
        public float xTolerance
        {
            get => _dto.xTolerance;
            set => _dto.xTolerance = value;
        }

        [CategoryAttribute(_Cat1), DescriptionAttribute("模板轮廓匹配的允许的角度")]
        [DisplayName("A03.角度")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 360, 1f, 2)]
        [Browsable(true)]
        public float xAngle
        {
            get => _dto.xAngle;
            set => _dto.xAngle = value;
        }

        [CategoryAttribute(_Cat1), DescriptionAttribute("模板轮廓匹配的搜寻范围扩大X方向的像素")]
        [DisplayName("A04.外扩X")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 1f, 2)]
        [Browsable(false)]
        public int xExtendx
        {
            get => _dto.xExtendx;
            set => _dto.xExtendx = value;
        }

        [CategoryAttribute(_Cat1), DescriptionAttribute("模板轮廓匹配的搜寻范围扩大Y方向的像素")]
        [DisplayName("A05.外扩Y")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 1f, 2)]
        [Browsable(false)]
        public int xExtendy
        {
            get => _dto.xExtendy;
            set => _dto.xExtendy = value;
        }

        [CategoryAttribute(_Cat1), DescriptionAttribute("模板轮廓匹配的搜寻范围内重叠率")]
        [DisplayName("A06.匹配重叠率")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 100)]
        [Browsable(true)]
        public int xMaxOverlap
        {
            get => _dto.xMaxOverlap;
            set => _dto.xMaxOverlap = value;
        }

        [CategoryAttribute(_Cat1), DescriptionAttribute("在搜索范围内重合的比例")]
        [DisplayName("A07.格点重叠率")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 1)]
        [Browsable(true)]
        public float xChipOverlap
        {
            get => _dto.xChipOverlap;
            set => _dto.xChipOverlap = value;
        }
        #endregion

        #region 找直線設定
        const string _Cat2 = "A02.尺寸检测参数设置";
        [CategoryAttribute(_Cat2)]
        [DisplayName("A00.載台背景")]
        [TypeConverter(typeof(JzEnumConverter))]
        [Browsable(true)]
        public EdgeBackGroundType xCarrierBackground
        {
            get => _dto.xCarrierBackground;
            set => _dto.xCarrierBackground = value;
        }

        [Browsable(true)]
        public RectangleF[] xLineBorderRects
        {
            get => _dto.xLineBorderRects;
            set => _dto.xLineBorderRects = value;
        }
        #endregion

        #region 舊的找直线的参数
        [CategoryAttribute(_Cat2)]
        [DisplayName("A00.测量方式")]
        [TypeConverter(typeof(JzEnumConverter))]
        [Browsable(false)]
        public MeasureFindLineType MFLType
        {
            get => _dto.MFLType;
            set => _dto.MFLType = value;
        }

        [CategoryAttribute(_Cat2), DescriptionAttribute("从左到右 true正向 false反向")]
        [DisplayName("A01.左边查找方向")]
        [Browsable(false)]
        public bool bPositive0
        {
            get => _dto.xPositives[0];
            set => _dto.xPositives[0] = value;
        }

        [CategoryAttribute(_Cat2), DescriptionAttribute("true白到黑 false黑到白")]
        [DisplayName("A02.左边极性")]
        [Browsable(false)]
        public bool bEdgePolarity0
        {
            get => _dto.xEdgePolarities[0];
            set => _dto.xEdgePolarities[0] = value;
        }

        [CategoryAttribute(_Cat2), DescriptionAttribute("从上到下 true正向 false反向")]
        [DisplayName("A03.上边查找方向")]
        [Browsable(false)]
        public bool bPositive1
        {
            get => _dto.xPositives[1];
            set => _dto.xPositives[1] = value;
        }

        [CategoryAttribute(_Cat2), DescriptionAttribute("true白到黑 false黑到白")]
        [DisplayName("A04.上边极性")]
        [Browsable(false)]
        public bool bEdgePolarity1
        {
            get => _dto.xEdgePolarities[1];
            set => _dto.xEdgePolarities[1] = value;
        }

        [CategoryAttribute(_Cat2), DescriptionAttribute("从左到右 true正向 false反向")]
        [DisplayName("A05.右边查找方向")]
        [Browsable(false)]
        public bool bPositive2
        {
            get => _dto.xPositives[2];
            set => _dto.xPositives[2] = value;
        }

        [CategoryAttribute(_Cat2), DescriptionAttribute("true白到黑 false黑到白")]
        [DisplayName("A06.右边极性")]
        [Browsable(false)]
        public bool bEdgePolarity2
        {
            get => _dto.xEdgePolarities[2];
            set => _dto.xEdgePolarities[2] = value;
        }

        [CategoryAttribute(_Cat2), DescriptionAttribute("从上到下 true正向 false反向")]
        [DisplayName("A07.下边查找方向")]
        [Browsable(false)]
        public bool bPositive3
        {
            get => _dto.xPositives[3];
            set => _dto.xPositives[3] = value;
        }

        [CategoryAttribute(_Cat2), DescriptionAttribute("true白到黑 false黑到白")]
        [DisplayName("A08.下边极性")]
        [Browsable(false)]
        public bool bEdgePolarity3
        {
            get => _dto.xEdgePolarities[3];
            set => _dto.xEdgePolarities[3] = value;
        }
        #endregion

        #region 缺陷检测设置

        const string _Cat3 = "A03.缺陷检测参数设置";

        [CategoryAttribute(_Cat3), DescriptionAttribute("")]
        [DisplayName("A00.二值化阈值")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 255)]
        [Browsable(true)]
        public int xThresholdValue
        {
            get => _dto.xThresholdValue;
            set => _dto.xThresholdValue = value;
        }

        [CategoryAttribute(_Cat3), DescriptionAttribute("单位pixel")]
        [DisplayName("A01.缺陷宽度")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 2)]
        [Browsable(true)]
        public float xCharWidth
        {
            get => _dto.xCharWidth;
            set => _dto.xCharWidth = value;
        }

        [CategoryAttribute(_Cat3), DescriptionAttribute("单位pixel")]
        [DisplayName("A02.缺陷高度")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 2)]
        [Browsable(true)]
        public float xCharHeight
        {
            get => _dto.xCharHeight;
            set => _dto.xCharHeight = value;
        }

        [CategoryAttribute(_Cat3), DescriptionAttribute("单位pixel")]
        [DisplayName("A03.缺陷面积")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 2)]
        [Browsable(true)]
        public float xCharArea
        {
            get => _dto.xCharArea;
            set => _dto.xCharArea = value;
        }

        [CategoryAttribute(_Cat3), DescriptionAttribute("单位pixel")]
        [DisplayName("A04.背景缺陷宽度")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 2)]
        [Browsable(false)]
        public float xBackgroudWidth
        {
            get => _dto.xBackgroudWidth;
            set => _dto.xBackgroudArea = value;
        }

        [CategoryAttribute(_Cat3), DescriptionAttribute("单位pixel")]
        [DisplayName("A05.背景缺陷高度")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 2)]
        [Browsable(false)]
        public float xBackgroudHeight
        {
            get => _dto.xBackgroudHeight;
            set => _dto.xBackgroudHeight = value;
        }

        [CategoryAttribute(_Cat3), DescriptionAttribute("单位pixel")]
        [DisplayName("A06.背景缺陷面积")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 2)]
        [Browsable(false)]
        public float xBackgroudArea
        {
            get => _dto.xBackgroudArea;
            set => _dto.xBackgroudArea = value;
        }

        [Browsable(false)]
        public int RoiCount => _dto.RoiCount;
        [Browsable(false)]
        public List<RectangleF> rectangles
        {
            get => _dto.xMaskRects;
            set => _dto.xMaskRects = value;
        }

        #endregion

        #region 尺寸宽度spec

        const string _Cat4 = "A04.尺寸规格设置";

        [CategoryAttribute(_Cat4), DescriptionAttribute("单位mm")]
        [DisplayName("A01.标准宽度")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 3)]
        [Browsable(true)]
        public float mWidthStand
        {
            get => _spec.mWidthStand;
            set => _spec.mWidthStand = value;
        }

        [CategoryAttribute(_Cat4), DescriptionAttribute("单位mm")]
        [DisplayName("A01a.宽度上公差")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 3)]
        [Browsable(true)]
        public float mWidthUpper
        {
            get => _spec.mWidthUpper;
            set => _spec.mWidthUpper = value;
        }

        [CategoryAttribute(_Cat4), DescriptionAttribute("单位mm")]
        [DisplayName("A01b.宽度下公差")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 3)]
        [Browsable(true)]
        public float mWidthLower
        {
            get => _spec.mWidthLower;
            set => _spec.mWidthLower = value;
        }

        [CategoryAttribute(_Cat4), DescriptionAttribute("单位mm")]
        [DisplayName("A02.标准高度")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 3)]
        [Browsable(true)]
        public float mHeightStand
        {
            get => _spec.mHeightStand;
            set => _spec.mHeightStand = value;
        }

        [CategoryAttribute(_Cat4), DescriptionAttribute("单位mm")]
        [DisplayName("A02a.高度上公差")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 3)]
        [Browsable(true)]
        public float mHeightUpper
        {
            get => _spec.mHeightUpper;
            set => _spec.mHeightUpper = value;
        }

        [CategoryAttribute(_Cat4), DescriptionAttribute("单位mm")]
        [DisplayName("A02b.高度下公差")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 3)]
        [Browsable(true)]
        public float mHeightLower
        {
            get => _spec.mHeightLower;
            set => _spec.mHeightLower = value;
        }
        #endregion

        #region 尺寸偏移spec

        const string _Cat5 = "A05.尺寸偏移规格设置";
        [CategoryAttribute(_Cat5), DescriptionAttribute("单位mm")]
        [DisplayName("A01.X方向偏移")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 3)]
        [Browsable(true)]
        public float XOffset
        {
            get => _spec.XOffset;
            set => _spec.XOffset = value;
        }

        [CategoryAttribute(_Cat5), DescriptionAttribute("单位mm")]
        [DisplayName("A02.Y方向偏移")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 3)]
        [Browsable(true)]
        public float YOffset
        {
            get => _spec.YOffset;
            set => _spec.YOffset = value;
        }
        #endregion

        public override void Load(bool eCancel = false)
        {
            _spec.Load(INIFILE);
            _dto.Load(INIFILE);
        }
        
        public override void Save()
        {
            _spec.Save(INIFILE);
            _dto.Save(INIFILE);
        }

        /// <summary>
        /// 保存 Defect Mask Rectangles
        /// </summary>
        internal void SaveRoi()
        {
            _dto.SaveMaskRects(INIFILE);
        }

        internal void SaveLineBorderRects()
        {
            _dto.SaveLineBorderRects(INIFILE);
        }
    }
}
