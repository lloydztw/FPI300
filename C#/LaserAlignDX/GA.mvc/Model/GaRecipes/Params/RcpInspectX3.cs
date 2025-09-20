using Common.RecipeSpace;
using Eazy_Project_III;
using JetEazy;
using LaserAlignDX.AoiModel;
using LaserAlignDX.BasicSpace;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;


namespace LaserAlignDX.OPSpace.RecipeSpace
{
    public class InspectX3ParaClass : RecipeBaseClass
    {
        #region PRIVATE_DATA
        DtoMeasureSpec _spec = new DtoMeasureSpec();
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
        public MatchAlgorithmEnum xAlgorithm { get; set; } = MatchAlgorithmEnum.GridMatch;

        [CategoryAttribute(_Cat1), DescriptionAttribute("模板轮廓匹配的格點門限")]
        [DisplayName("A01.格點門限")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 255)]
        [Browsable(true)]
        public int xGridPadThreshold { get; set; } = 0;

        [CategoryAttribute(_Cat1), DescriptionAttribute("模板轮廓匹配的相似程度")]
        [DisplayName("A02.相似度")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 1, 0.1f, 2)]
        [Browsable(true)]
        public float xTolerance { get; set; } = 0.5f;

        [CategoryAttribute(_Cat1), DescriptionAttribute("模板轮廓匹配的允许的角度")]
        [DisplayName("A03.角度")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 360, 1f, 2)]
        [Browsable(true)]
        public float xAngle { get; set; } = 30f;

        [CategoryAttribute(_Cat1), DescriptionAttribute("模板轮廓匹配的搜寻范围扩大X方向的像素")]
        [DisplayName("A04.外扩X")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 1f, 2)]
        [Browsable(false)]
        public int xExtendx { get; set; } = 20;

        [CategoryAttribute(_Cat1), DescriptionAttribute("模板轮廓匹配的搜寻范围扩大Y方向的像素")]
        [DisplayName("A05.外扩Y")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 1f, 2)]
        [Browsable(false)]
        public int xExtendy { get; set; } = 20;

        [CategoryAttribute(_Cat1), DescriptionAttribute("模板轮廓匹配的搜寻范围内重叠率")]
        [DisplayName("A06.匹配重叠率")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 100)]
        [Browsable(true)]
        public int xMaxOverlap { get; set; } = 80;

        [CategoryAttribute(_Cat1), DescriptionAttribute("在搜索范围内重合的比例")]
        [DisplayName("A07.格点重叠率")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 1)]
        [Browsable(true)]
        public float xChipOverlap { get; set; } = 0.5f;
        #endregion

        #region 找直线的参数

        const string _Cat2 = "A02.尺寸检测参数设置";

        [CategoryAttribute(_Cat2)]
        [DisplayName("A00.載台背景")]
        [TypeConverter(typeof(JzEnumConverter))]
        [Browsable(true)]
        public EdgeBackGroundType xCarrierBackground { get; set; }

        [CategoryAttribute(_Cat2)]
        [DisplayName("A00.测量方式")]
        [TypeConverter(typeof(JzEnumConverter))]
        [Browsable(false)]
        public MeasureFindLineType MFLType { get; set; } = MeasureFindLineType.FindLineType_v1;

        [CategoryAttribute(_Cat2), DescriptionAttribute("从左到右 true正向 false反向")]
        [DisplayName("A01.左边查找方向")]
        [Browsable(false)]
        public bool bPositive0 { get; set; } = true;

        [CategoryAttribute(_Cat2), DescriptionAttribute("true白到黑 false黑到白")]
        [DisplayName("A02.左边极性")]
        [Browsable(false)]
        public bool bEdgePolarity0 { get; set; } = true;

        [CategoryAttribute(_Cat2), DescriptionAttribute("从上到下 true正向 false反向")]
        [DisplayName("A03.上边查找方向")]
        [Browsable(false)]
        public bool bPositive1 { get; set; } = true;

        [CategoryAttribute(_Cat2), DescriptionAttribute("true白到黑 false黑到白")]
        [DisplayName("A04.上边极性")]
        [Browsable(false)]
        public bool bEdgePolarity1 { get; set; } = true;

        [CategoryAttribute(_Cat2), DescriptionAttribute("从左到右 true正向 false反向")]
        [DisplayName("A05.右边查找方向")]
        [Browsable(false)]
        public bool bPositive2 { get; set; } = true;

        [CategoryAttribute(_Cat2), DescriptionAttribute("true白到黑 false黑到白")]
        [DisplayName("A06.右边极性")]
        [Browsable(false)]
        public bool bEdgePolarity2 { get; set; } = true;

        [CategoryAttribute(_Cat2), DescriptionAttribute("从上到下 true正向 false反向")]
        [DisplayName("A07.下边查找方向")]
        [Browsable(false)]
        public bool bPositive3 { get; set; } = true;

        [CategoryAttribute(_Cat2), DescriptionAttribute("true白到黑 false黑到白")]
        [DisplayName("A08.下边极性")]
        [Browsable(false)]
        public bool bEdgePolarity3 { get; set; } = true;
        #endregion

        #region 缺陷检测设置

        const string _Cat3 = "A03.缺陷检测参数设置";

        [CategoryAttribute(_Cat3), DescriptionAttribute("")]
        [DisplayName("A00.二值化阈值")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 255)]
        [Browsable(true)]
        public int xThresholdValue { get; set; } = 128;

        [CategoryAttribute(_Cat3), DescriptionAttribute("单位pixel")]
        [DisplayName("A01.缺陷宽度")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 2)]
        [Browsable(true)]
        public float xCharWidth { get; set; } = 15.1f;
        [CategoryAttribute(_Cat3), DescriptionAttribute("单位pixel")]
        [DisplayName("A02.缺陷高度")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 2)]
        [Browsable(true)]
        public float xCharHeight { get; set; } = 15.1f;
        [CategoryAttribute(_Cat3), DescriptionAttribute("单位pixel")]
        [DisplayName("A03.缺陷面积")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 2)]
        [Browsable(true)]
        public float xCharArea { get; set; } = 30.1f;
        [CategoryAttribute(_Cat3), DescriptionAttribute("单位pixel")]
        [DisplayName("A04.背景缺陷宽度")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 2)]
        [Browsable(false)]
        public float xBackgroudWidth { get; set; } = 15.1f;
        [CategoryAttribute(_Cat3), DescriptionAttribute("单位pixel")]
        [DisplayName("A05.背景缺陷高度")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 2)]
        [Browsable(false)]
        public float xBackgroudHeight { get; set; } = 15.1f;
        [CategoryAttribute(_Cat3), DescriptionAttribute("单位pixel")]
        [DisplayName("A06.背景缺陷面积")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 2)]
        [Browsable(false)]
        public float xBackgroudArea { get; set; } = 30.1f;

        [Browsable(false)]
        public int RoiCount { get; set; } = 0;
        [Browsable(false)]
        public List<RectangleF> rectangles { get; set; } = new List<RectangleF>();
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
            xAlgorithm = (MatchAlgorithmEnum)int.Parse(ReadINIValue("Basic", "xAlgorithm", "0", INIFILE));
            xTolerance = float.Parse(ReadINIValue("Basic", "xTolerance", "0.5", INIFILE));
            xAngle = float.Parse(ReadINIValue("Basic", "xAngle", "30", INIFILE));
            xExtendx = int.Parse(ReadINIValue("Basic", "xExtendx", "20", INIFILE));
            xExtendy = int.Parse(ReadINIValue("Basic", "xExtendy", "20", INIFILE));
            xMaxOverlap = int.Parse(ReadINIValue("Basic", "xMaxOverlap", "80", INIFILE));
            xChipOverlap = float.Parse(ReadINIValue("Basic", "xChipOverlap", "0.5", INIFILE));
            xGridPadThreshold = int.Parse(ReadINIValue("Basic", "xGridPadThreshold", "0", INIFILE));

            bCheckInspect = ReadINIValue("Inspect", "bCheckInspect", "1", INIFILE) == "1";
            xThresholdValue = int.Parse(ReadINIValue("Inspect", "xThresholdValue", "128", INIFILE));
            xCharWidth = float.Parse(ReadINIValue("Inspect", "xCharWidth", "15.1", INIFILE));
            xCharHeight = float.Parse(ReadINIValue("Inspect", "xCharHeight", "15.1", INIFILE));
            xCharArea = float.Parse(ReadINIValue("Inspect", "xCharArea", "30.1", INIFILE));
            xBackgroudWidth = float.Parse(ReadINIValue("Inspect", "xBackgroudWidth", "15.1", INIFILE));
            xBackgroudHeight = float.Parse(ReadINIValue("Inspect", "xBackgroudHeight", "15.1", INIFILE));
            xBackgroudArea = float.Parse(ReadINIValue("Inspect", "xBackgroudArea", "30.1", INIFILE));

            RoiCount = int.Parse(ReadINIValue("Inspect", "RoiCount", "0", INIFILE));
            int i = 0;
            rectangles.Clear();
            while (i < RoiCount)
            {
                RectangleF rectf = StringtoRectF(ReadINIValue("Inspect", $"Roi{i.ToString()}", RectFtoStringSimple(new RectangleF(0, 0, 10, 10)), INIFILE));
                rectangles.Add(rectf);

                i++;
            }

            bOpenLineMeasure = ReadINIValue("Basic", "bOpenLineMeasure", "0", INIFILE) == "1";
            bCheckMeasureOffset = ReadINIValue("Basic", "bCheckMeasureOffset", "0", INIFILE) == "1";
            MFLType = (MeasureFindLineType)int.Parse(ReadINIValue("Basic", "MFLType", "0", INIFILE));
            xCarrierBackground = (EdgeBackGroundType)int.Parse(ReadINIValue("Basic", "CarrierBackground", "0", INIFILE));
            bPositive0 = ReadINIValue("Basic", "bPositive0", "1", INIFILE) == "1";
            bPositive1 = ReadINIValue("Basic", "bPositive1", "1", INIFILE) == "1";
            bPositive2 = ReadINIValue("Basic", "bPositive2", "1", INIFILE) == "1";
            bPositive3 = ReadINIValue("Basic", "bPositive3", "1", INIFILE) == "1";
            bEdgePolarity0 = ReadINIValue("Basic", "bEdgePolarity0", "1", INIFILE) == "1";
            bEdgePolarity1 = ReadINIValue("Basic", "bEdgePolarity1", "1", INIFILE) == "1";
            bEdgePolarity2 = ReadINIValue("Basic", "bEdgePolarity2", "1", INIFILE) == "1";
            bEdgePolarity3 = ReadINIValue("Basic", "bEdgePolarity3", "1", INIFILE) == "1";

            mWidthStand = float.Parse(ReadINIValue("Basic", "mWidthStand", "9", INIFILE));
            mWidthUpper = float.Parse(ReadINIValue("Basic", "mWidthUpper", "0.05", INIFILE));
            mWidthLower = float.Parse(ReadINIValue("Basic", "mWidthLower", "0.05", INIFILE));
            mHeightStand = float.Parse(ReadINIValue("Basic", "mHeightStand", "9.9", INIFILE));
            mHeightUpper = float.Parse(ReadINIValue("Basic", "mHeightUpper", "0.05", INIFILE));
            mHeightLower = float.Parse(ReadINIValue("Basic", "mHeightLower", "0.05", INIFILE));
            XOffset = float.Parse(ReadINIValue("Basic", "XOffset", "0.05", INIFILE));
            YOffset = float.Parse(ReadINIValue("Basic", "YOffset", "0.05", INIFILE));

        }
        
        public override void Save()
        {
            WriteINIValue("Basic", "xAlgorithm", ((int)xAlgorithm).ToString(), INIFILE);
            WriteINIValue("Basic", "xTolerance", xTolerance.ToString(), INIFILE);
            WriteINIValue("Basic", "xAngle", xAngle.ToString(), INIFILE);
            WriteINIValue("Basic", "xExtendx", xExtendx.ToString(), INIFILE);
            WriteINIValue("Basic", "xExtendy", xExtendy.ToString(), INIFILE);
            WriteINIValue("Basic", "xMaxOverlap", xMaxOverlap.ToString(), INIFILE);
            WriteINIValue("Basic", "xChipOverlap", xChipOverlap.ToString(), INIFILE);
            WriteINIValue("Basic", "xGridPadThreshold", xGridPadThreshold.ToString(), INIFILE);

            WriteINIValue("Inspect", "bCheckInspect", (bCheckInspect ? "1" : "0"), INIFILE);
            WriteINIValue("Inspect", "xThresholdValue", xThresholdValue.ToString(), INIFILE);
            WriteINIValue("Inspect", "xCharWidth", xCharWidth.ToString(), INIFILE);
            WriteINIValue("Inspect", "xCharHeight", xCharHeight.ToString(), INIFILE);
            WriteINIValue("Inspect", "xCharArea", xCharArea.ToString(), INIFILE);
            WriteINIValue("Inspect", "xBackgroudWidth", xBackgroudWidth.ToString(), INIFILE);
            WriteINIValue("Inspect", "xBackgroudHeight", xBackgroudHeight.ToString(), INIFILE);
            WriteINIValue("Inspect", "xBackgroudArea", xBackgroudArea.ToString(), INIFILE);

            WriteINIValue("Basic", "bOpenLineMeasure", (bOpenLineMeasure ? "1" : "0"), INIFILE);
            WriteINIValue("Basic", "bCheckMeasureOffset", (bCheckMeasureOffset ? "1" : "0"), INIFILE);
            WriteINIValue("Basic", "MFLType", ((int)MFLType).ToString(), INIFILE);
            WriteINIValue("Basic", "CarrierBackground", ((int)xCarrierBackground).ToString(), INIFILE);

            WriteINIValue("Basic", "bPositive0", (bPositive0 ? "1" : "0"), INIFILE);
            WriteINIValue("Basic", "bPositive1", (bPositive1 ? "1" : "0"), INIFILE);
            WriteINIValue("Basic", "bPositive2", (bPositive2 ? "1" : "0"), INIFILE);
            WriteINIValue("Basic", "bPositive3", (bPositive3 ? "1" : "0"), INIFILE);
            WriteINIValue("Basic", "bEdgePolarity0", (bEdgePolarity0 ? "1" : "0"), INIFILE);
            WriteINIValue("Basic", "bEdgePolarity1", (bEdgePolarity1 ? "1" : "0"), INIFILE);
            WriteINIValue("Basic", "bEdgePolarity2", (bEdgePolarity2 ? "1" : "0"), INIFILE);
            WriteINIValue("Basic", "bEdgePolarity3", (bEdgePolarity3 ? "1" : "0"), INIFILE);

            WriteINIValue("Basic", "mWidthStand", mWidthStand.ToString(), INIFILE);
            WriteINIValue("Basic", "mWidthUpper", mWidthUpper.ToString(), INIFILE);
            WriteINIValue("Basic", "mWidthLower", mWidthLower.ToString(), INIFILE);
            WriteINIValue("Basic", "mHeightStand", mHeightStand.ToString(), INIFILE);
            WriteINIValue("Basic", "mHeightUpper", mHeightUpper.ToString(), INIFILE);
            WriteINIValue("Basic", "mHeightLower", mHeightLower.ToString(), INIFILE);
            WriteINIValue("Basic", "XOffset", XOffset.ToString(), INIFILE);
            WriteINIValue("Basic", "YOffset", YOffset.ToString(), INIFILE);
        }

        /// <summary>
        /// 保存 Defect Mask Rectangles
        /// </summary>
        internal void SaveRoi()
        {
            RoiCount = rectangles.Count;
            WriteINIValue("Inspect", "RoiCount", RoiCount.ToString(), INIFILE);
            int i = 0;
            while (i < RoiCount)
            {
                WriteINIValue("Inspect", $"Roi{i.ToString()}", RectFtoStringSimple(rectangles[i]), INIFILE);

                i++;
            }
        }

    }
}
