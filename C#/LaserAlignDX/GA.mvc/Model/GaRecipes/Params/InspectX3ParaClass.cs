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


namespace LaserAlignDX.Mvc.Model.Recipes
{
    public class InspectX3ParaClass : RecipeBaseClass
    {
        #region SINGLETON
        protected InspectX3ParaClass()
        {

        }
        private static InspectX3ParaClass _instance = null;
        private DtoX3MeasureSpec _spec => DtoX3MeasureSpec.Instance;
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


        #region 1_檢測項目_(兩載台共用)
        const string _Cat01 = "1. 檢測項目";

        [CategoryAttribute(_Cat01), DescriptionAttribute("")]
        [DisplayName("01 啟用 尺寸量測")]
        [Browsable(true)]
        public bool optChipMeasurement
        {
            get => _spec.optChipMeasurement;
            set => _spec.optChipMeasurement = value;
        }

        [CategoryAttribute(_Cat01), DescriptionAttribute("僅適用於 格點晶粒!")]
        [DisplayName("02 啟用 尺寸偏移檢測")]
        [Browsable(true)]
        public bool optChipEdgesDiffCompare
        {
            get => _spec.optChipEdgesCompare;
            set => _spec.optChipEdgesCompare = value;
        }

        [CategoryAttribute(_Cat01), DescriptionAttribute("")]
        [DisplayName("03 啟用 缺陷檢測")]
        [Browsable(true)]
        public bool optChipDefectsInspect
        {
            get => _spec.optChipDefectsInspect;
            set => _spec.optChipDefectsInspect = value;
        }
        #endregion


        #region 2_整盤判定_(兩載台共用)
        const string _Cat02 = "2. 整盤判定設定";

        [CategoryAttribute(_Cat02), DescriptionAttribute("")]
        [DisplayName("01 啟用 整盤 NG百分比 判定")]
        [Browsable(false)]
        public bool optUseTrayNgPercentage
        {
            get => _spec.optUseTrayNgPercentage;
            set => _spec.optUseTrayNgPercentage = value;
        }

        [CategoryAttribute(_Cat02), DescriptionAttribute("單位 %")]
        [DisplayName("02 整盤 NG百分比 上限 (%)")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0f, 100f, 0.5f, 1)]
        [Browsable(false)]
        public float TryNgPercentage
        {
            get => _spec.TrayNgPercentage;
            set => _spec.TrayNgPercentage = value;
        }

        [CategoryAttribute(_Cat02), DescriptionAttribute("")]
        [DisplayName("03 顯示 個別 NG 檢測結果")]
        [Browsable(false)]
        public bool optShowIndividualNG
        {
            get => _spec.optShowIndividualNG;
            set => _spec.optShowIndividualNG = value;
        }
        #endregion


        #region 3_晶粒定位
        const string _Cat03 = "3. 晶粒定位";

        [CategoryAttribute(_Cat03), DescriptionAttribute("晶粒定位的演算法")]
        [DisplayName("01 演算法")]
        [TypeConverter(typeof(JzEnumConverter))]
        [Browsable(true)]
        public MatchAlgorithmEnum xAlgorithm { get; set; } = MatchAlgorithmEnum.GridMatch;

        [CategoryAttribute(_Cat03), DescriptionAttribute("在搜索范围内重合的比例 (0.00 ~ 1.00)")]
        [DisplayName("02 格位重叠率")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0.00f, 1.00f, 0.05f, 2)]
        [Browsable(true)]
        public float xChipOverlap { get; set; } = 0.5f;

        //const string _Cat1A = "A01.A '格點型' 晶粒定位";
        [CategoryAttribute(_Cat03), DescriptionAttribute("搭配 '格點晶粒' 匹配演算法的 '格點門限值'")]
        [DisplayName("A0 格點型晶粒 門限")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 255)]
        [Browsable(true)]
        public int xGridPadThreshold { get; set; } = 0;

        //const string _Cat1B = "A01.B '一般型' 晶粒定位";
        [CategoryAttribute(_Cat03), DescriptionAttribute("'一般型晶粒' 模板匹配的相似程度")]
        [DisplayName("B1 一般型晶粒 相似度")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 1, 0.1f, 2)]
        [Browsable(true)]
        public float xTolerance { get; set; } = 0.5f;

        [CategoryAttribute(_Cat03), DescriptionAttribute("'一般型晶粒' 模板匹配的允许的角度")]
        [DisplayName("B2 一般型晶粒 角度範圍")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 360, 1f, 2)]
        [Browsable(true)]
        public float xAngle { get; set; } = 30f;

        [CategoryAttribute(_Cat03), DescriptionAttribute("'一般型晶粒' 模板匹配的搜寻范围内重叠率")]
        [DisplayName("B3 一般型晶粒 匹配重叠率 (%)")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0f, 100f, 1f, 0)]
        [Browsable(true)]
        public int xMaxOverlap { get; set; } = 80;
        #endregion


        #region 4_晶粒尺寸計算参数
        const string _Cat04 = "4. 尺寸检测参数设置";

        [CategoryAttribute(_Cat04)]
        [DisplayName("01 載台背景")]
        [TypeConverter(typeof(JzEnumConverter))]
        [Browsable(true)]
        public EdgeBackGroundType xCarrierBackground { get; set; }

        [CategoryAttribute(_Cat04), DescriptionAttribute("單位 mm")]
        [DisplayName("02 樣本寬度")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0.001f, 9999f, 0.010f, 3)]
        [Browsable(true)]
        public float xTemplateChipWidth
        {
            get;
            set;
        }

        [CategoryAttribute(_Cat04), DescriptionAttribute("單位 mm")]
        [DisplayName("03 樣本高度")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0.001f, 9999f, 0.010f, 3)]
        [Browsable(true)]
        public float xTemplateChipHeight
        {
            get;
            set;
        }

        /// <summary>
        /// 縮放尺度 W (Runtime 自動計算)
        /// </summary>
        [Browsable(false)]
        public double xChipDimScaleW
        {
            get;
            set;
        } = 1.0;
        /// <summary>
        /// 縮放尺度 H (Runtime 自動計算)
        /// </summary>
        [Browsable(false)]
        public double xChipDimScaleH
        {
            get;
            set;
        } = 1.0;
        #endregion


        #region 4B_找直线的参数_(舊)
        [CategoryAttribute(_Cat04)]
        [DisplayName("B00 测量方式")]
        [TypeConverter(typeof(JzEnumConverter))]
        [Browsable(false)]
        public MeasureFindLineType MFLType { get; set; } = MeasureFindLineType.FindLineType_v1;

        [CategoryAttribute(_Cat04), DescriptionAttribute("从左到右 true正向 false反向")]
        [DisplayName("B01 左边查找方向")]
        [Browsable(false)]
        public bool bPositive0 { get; set; } = true;
        [CategoryAttribute(_Cat04), DescriptionAttribute("true白到黑 false黑到白")]
        [DisplayName("B02 左边极性")]
        [Browsable(false)]
        public bool bEdgePolarity0 { get; set; } = true;

        [CategoryAttribute(_Cat04), DescriptionAttribute("从上到下 true正向 false反向")]
        [DisplayName("B03 上边查找方向")]
        [Browsable(false)]
        public bool bPositive1 { get; set; } = true;
        [CategoryAttribute(_Cat04), DescriptionAttribute("true白到黑 false黑到白")]
        [DisplayName("B04 上边极性")]
        [Browsable(false)]
        public bool bEdgePolarity1 { get; set; } = true;

        [CategoryAttribute(_Cat04), DescriptionAttribute("从左到右 true正向 false反向")]
        [DisplayName("B05 右边查找方向")]
        [Browsable(false)]
        public bool bPositive2 { get; set; } = true;
        [CategoryAttribute(_Cat04), DescriptionAttribute("true白到黑 false黑到白")]
        [DisplayName("B06 右边极性")]
        [Browsable(false)]
        public bool bEdgePolarity2 { get; set; } = true;

        [CategoryAttribute(_Cat04), DescriptionAttribute("从上到下 true正向 false反向")]
        [DisplayName("B07 下边查找方向")]
        [Browsable(false)]
        public bool bPositive3 { get; set; } = true;
        [CategoryAttribute(_Cat04), DescriptionAttribute("true白到黑 false黑到白")]
        [DisplayName("B08 下边极性")]
        [Browsable(false)]
        public bool bEdgePolarity3 { get; set; } = true;
        #endregion


        #region 7_缺陷检测设置
        const string _Cat7 = "7. 缺陷检测参数设置";

        [CategoryAttribute(_Cat7), DescriptionAttribute("")]
        [DisplayName("01 二值化阈值")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 255)]
        [Browsable(true)]
        public int xThresholdValue { get; set; } = 128;

        [CategoryAttribute(_Cat7), DescriptionAttribute("单位pixel")]
        [DisplayName("02 缺陷宽度")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 2)]
        [Browsable(true)]
        public float xCharWidth { get; set; } = 15.1f;

        [CategoryAttribute(_Cat7), DescriptionAttribute("单位pixel")]
        [DisplayName("03 缺陷高度")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 2)]
        [Browsable(true)]
        public float xCharHeight { get; set; } = 15.1f;

        [CategoryAttribute(_Cat7), DescriptionAttribute("单位pixel")]
        [DisplayName("04 缺陷面积")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 2)]
        [Browsable(true)]
        public float xCharArea { get; set; } = 30.1f;

        [CategoryAttribute(_Cat7), DescriptionAttribute("单位pixel")]
        [DisplayName("05 背景缺陷宽度")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 2)]
        [Browsable(false)]
        public float xBackgroudWidth { get; set; } = 15.1f;

        [CategoryAttribute(_Cat7), DescriptionAttribute("单位pixel")]
        [DisplayName("06 背景缺陷高度")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 2)]
        [Browsable(false)]
        public float xBackgroudHeight { get; set; } = 15.1f;

        [CategoryAttribute(_Cat7), DescriptionAttribute("单位pixel")]
        [DisplayName("07 背景缺陷面积")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 2)]
        [Browsable(false)]
        public float xBackgroudArea { get; set; } = 30.1f;

        /// <summary>
        /// Mask Rectangles 的數量
        /// </summary>
        [Browsable(false)]
        public int RoiCount => rectangles.Count;
        /// <summary>
        /// Mask Rectangles
        /// </summary>
        [Browsable(false)]
        public List<RectangleF> rectangles { get; set; } = new List<RectangleF>();
        #endregion


        #region 5_尺寸量測_SPEC_(兩載台共用)
        const string _Cat5 = "5. 尺寸规格设置";

        [CategoryAttribute(_Cat5), DescriptionAttribute("單位 mm")]
        [DisplayName("01 標準寬度")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0.001f, 9999f, 0.010f, 3)]
        [Browsable(true)]
        public float mWidthStand
        {
            get => _spec.StandardWidth;
            set => _spec.StandardWidth = value;
        }

        [CategoryAttribute(_Cat5), DescriptionAttribute("單位 mm")]
        [DisplayName("01a 寬度上限")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0.001f, 9999f, 0.010f, 3)]
        [Browsable(true)]
        public float mWidthStandMax
        {
            get => _spec.StandardDimRangeW.Max;
            set => _spec.StandardDimRangeW.Max = value;
        }

        [CategoryAttribute(_Cat5), DescriptionAttribute("單位 mm")]
        [DisplayName("01b 寬度下限")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0.001f, 9999f, 0.010f, 3)]
        [Browsable(true)]
        public float mWidthStandMin
        {
            get => _spec.StandardDimRangeW.Min;
            set => _spec.StandardDimRangeW.Min = value;
        }

        [CategoryAttribute(_Cat5), DescriptionAttribute("單位 mm")]
        [DisplayName("02 標準高度")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0.001f, 9999f, 0.010f, 3)]
        [Browsable(true)]
        public float mHeightStand
        {
            get => _spec.StandardHeight;
            set => _spec.StandardHeight = value;
        }

        [CategoryAttribute(_Cat5), DescriptionAttribute("單位 mm")]
        [DisplayName("02a 高度上限")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0.001f, 9999f, 0.010f, 3)]
        [Browsable(true)]
        public float mHeightStandMax
        {
            get => _spec.StandardDimRangeH.Max;
            set => _spec.StandardDimRangeH.Max = value;
        }

        [CategoryAttribute(_Cat5), DescriptionAttribute("單位 mm")]
        [DisplayName("02b 高度下限")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0.001f, 9999f, 0.010f, 3)]
        [Browsable(true)]
        public float mHeightStandMin
        {
            get => _spec.StandardDimRangeH.Min;
            set => _spec.StandardDimRangeH.Min = value;
        }
        #endregion


        #region 6_尺寸偏移_SPEC_(兩載台共用)
        const string _Cat6 = "6. 尺寸偏移规格设置";

        [CategoryAttribute(_Cat6), DescriptionAttribute("僅適用於 格點晶粒! (單位 mm)")]
        [DisplayName("01 邊緣差異X 上限")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0.001f, 9999f, 0.010f, 3)]
        [Browsable(true)]
        public float PadEdgeDiffMaxX
        {
            get => _spec.PadEdgeDiffMaxX;
            set => _spec.PadEdgeDiffMaxX = value;
        }

        [CategoryAttribute(_Cat6), DescriptionAttribute("僅適用於 格點晶粒! (單位 mm)")]
        [DisplayName("02 邊緣差異Y 上限")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0.001f, 9999f, 0.010f, 3)]
        [Browsable(true)]
        public float PadEdgeDiffMaxY
        {
            get => _spec.PadEdgeDiffMaxY;
            set => _spec.PadEdgeDiffMaxY = value;
        }
        #endregion


        public override void Load(bool eCancel = false)
        {
            xAlgorithm = (MatchAlgorithmEnum)int.Parse(ReadINIValue("Basic", "xAlgorithm", "0", INIFILE));
            xTolerance = float.Parse(ReadINIValue("Basic", "xTolerance", "0.5", INIFILE));
            xAngle = float.Parse(ReadINIValue("Basic", "xAngle", "30", INIFILE));
            //xExtendx = int.Parse(ReadINIValue("Basic", "xExtendx", "20", INIFILE));
            //xExtendy = int.Parse(ReadINIValue("Basic", "xExtendy", "20", INIFILE));
            xMaxOverlap = int.Parse(ReadINIValue("Basic", "xMaxOverlap", "80", INIFILE));
            xChipOverlap = float.Parse(ReadINIValue("Basic", "xChipOverlap", "0.5", INIFILE));
            xGridPadThreshold = int.Parse(ReadINIValue("Basic", "xGridPadThreshold", "0", INIFILE));

            xThresholdValue = int.Parse(ReadINIValue("Inspect", "xThresholdValue", "128", INIFILE));
            xCharWidth = float.Parse(ReadINIValue("Inspect", "xCharWidth", "15.1", INIFILE));
            xCharHeight = float.Parse(ReadINIValue("Inspect", "xCharHeight", "15.1", INIFILE));
            xCharArea = float.Parse(ReadINIValue("Inspect", "xCharArea", "30.1", INIFILE));
            xBackgroudWidth = float.Parse(ReadINIValue("Inspect", "xBackgroudWidth", "15.1", INIFILE));
            xBackgroudHeight = float.Parse(ReadINIValue("Inspect", "xBackgroudHeight", "15.1", INIFILE));
            xBackgroudArea = float.Parse(ReadINIValue("Inspect", "xBackgroudArea", "30.1", INIFILE));

            xCarrierBackground = (EdgeBackGroundType)int.Parse(ReadINIValue("Basic", "CarrierBackground", "0", INIFILE));
            xTemplateChipWidth = float.Parse(ReadINIValue("Basic", "xTemplateChipWidth", "0.0", INIFILE));
            xTemplateChipHeight = float.Parse(ReadINIValue("Basic", "xTemplateChipHeight", "0.0", INIFILE));
            xChipDimScaleW = double.Parse(ReadINIValue("Basic", "xChipDimScaleW", "1.0", INIFILE));
            xChipDimScaleH = double.Parse(ReadINIValue("Basic", "xChipDimScaleH", "1.0", INIFILE));

            if (xTemplateChipWidth <= 0)
                xTemplateChipWidth = _spec.StandardWidth;
            if (xTemplateChipHeight <= 0)
                xTemplateChipHeight = _spec.StandardHeight;

            if (xChipDimScaleW <= 0)
                xChipDimScaleW = 1.0;
            if (xChipDimScaleH <= 0)
                xChipDimScaleH = 1.0;
            //---------------------------------------------------------------------------------------------------
            // 以下參數 目前沒有用到
            //---------------------------------------------------------------------------------------------------
            MFLType = (MeasureFindLineType)int.Parse(ReadINIValue("Basic", "MFLType", "0", INIFILE));
            bPositive0 = ReadINIValue("Basic", "bPositive0", "1", INIFILE) == "1";
            bPositive1 = ReadINIValue("Basic", "bPositive1", "1", INIFILE) == "1";
            bPositive2 = ReadINIValue("Basic", "bPositive2", "1", INIFILE) == "1";
            bPositive3 = ReadINIValue("Basic", "bPositive3", "1", INIFILE) == "1";
            bEdgePolarity0 = ReadINIValue("Basic", "bEdgePolarity0", "1", INIFILE) == "1";
            bEdgePolarity1 = ReadINIValue("Basic", "bEdgePolarity1", "1", INIFILE) == "1";
            bEdgePolarity2 = ReadINIValue("Basic", "bEdgePolarity2", "1", INIFILE) == "1";
            bEdgePolarity3 = ReadINIValue("Basic", "bEdgePolarity3", "1", INIFILE) == "1";

            //---------------------------------------------------------------------------------------------------
            // 以下改由 DtoX3MeasureSpec 於上層 RecipeFPIX3Class 處理
            //---------------------------------------------------------------------------------------------------
            //bCheckInspect = ReadINIValue("Inspect", "bCheckInspect", "1", INIFILE) == "1";
            //bOpenLineMeasure = ReadINIValue("Basic", "bOpenLineMeasure", "0", INIFILE) == "1";
            //bCheckMeasureOffset = ReadINIValue("Basic", "bCheckMeasureOffset", "0", INIFILE) == "1";
            //mWidthStand = float.Parse(ReadINIValue("Basic", "mWidthStand", "9", INIFILE));
            //mHeightStand = float.Parse(ReadINIValue("Basic", "mHeightStand", "9.9", INIFILE));
            //mWidthPercentage = float.Parse(ReadINIValue("Basic", "mWidthPercentage", "1.0", INIFILE));
            //mHeightPercentage = float.Parse(ReadINIValue("Basic", "mHeightPercentage", "1.0", INIFILE));
            //XOffset = float.Parse(ReadINIValue("Basic", "XOffset", "0.05", INIFILE));
            //YOffset = float.Parse(ReadINIValue("Basic", "YOffset", "0.05", INIFILE));
            _spec.Load(INIFILE);

            //---------------------------------------------------------------------------------------------------
            // 以下改由 LoadMaskRects 處理
            //---------------------------------------------------------------------------------------------------
            //RoiCount = int.Parse(ReadINIValue("Inspect", "RoiCount", "0", INIFILE));
            //int i = 0;
            //rectangles.Clear();
            //while (i < RoiCount)
            //{
            //    RectangleF rectf = StringtoRectF(ReadINIValue("Inspect", $"Roi{i.ToString()}", RectFtoStringSimple(new RectangleF(0, 0, 10, 10)), INIFILE));
            //    rectangles.Add(rectf);
            //    i++;
            //}

            LoadMaskRects();
        }
        public override void Save()
        {
            WriteINIValue("Basic", "xAlgorithm", ((int)xAlgorithm).ToString(), INIFILE);
            WriteINIValue("Basic", "xTolerance", xTolerance.ToString(), INIFILE);
            WriteINIValue("Basic", "xAngle", xAngle.ToString(), INIFILE);
            //WriteINIValue("Basic", "xExtendx", xExtendx.ToString(), INIFILE);
            //WriteINIValue("Basic", "xExtendy", xExtendy.ToString(), INIFILE);
            WriteINIValue("Basic", "xMaxOverlap", xMaxOverlap.ToString(), INIFILE);
            WriteINIValue("Basic", "xChipOverlap", xChipOverlap.ToString(), INIFILE);
            WriteINIValue("Basic", "xGridPadThreshold", xGridPadThreshold.ToString(), INIFILE);

            WriteINIValue("Inspect", "xThresholdValue", xThresholdValue.ToString(), INIFILE);
            WriteINIValue("Inspect", "xCharWidth", xCharWidth.ToString(), INIFILE);
            WriteINIValue("Inspect", "xCharHeight", xCharHeight.ToString(), INIFILE);
            WriteINIValue("Inspect", "xCharArea", xCharArea.ToString(), INIFILE);
            WriteINIValue("Inspect", "xBackgroudWidth", xBackgroudWidth.ToString(), INIFILE);
            WriteINIValue("Inspect", "xBackgroudHeight", xBackgroudHeight.ToString(), INIFILE);
            WriteINIValue("Inspect", "xBackgroudArea", xBackgroudArea.ToString(), INIFILE);

            WriteINIValue("Basic", "CarrierBackground", ((int)xCarrierBackground).ToString(), INIFILE);
            WriteINIValue("Basic", "xTemplateChipWidth", xTemplateChipWidth.ToString(), INIFILE);
            WriteINIValue("Basic", "xTemplateChipHeight", xTemplateChipHeight.ToString(), INIFILE);
            WriteINIValue("Basic", "xChipDimScaleW", xChipDimScaleW.ToString(), INIFILE);
            WriteINIValue("Basic", "xChipDimScaleH", xChipDimScaleH.ToString(), INIFILE);

            //---------------------------------------------------------------------------------------------------
            // 以下參數 目前沒有用到
            //---------------------------------------------------------------------------------------------------
            WriteINIValue("Basic", "MFLType", ((int)MFLType).ToString(), INIFILE);
            WriteINIValue("Basic", "bPositive0", (bPositive0 ? "1" : "0"), INIFILE);
            WriteINIValue("Basic", "bPositive1", (bPositive1 ? "1" : "0"), INIFILE);
            WriteINIValue("Basic", "bPositive2", (bPositive2 ? "1" : "0"), INIFILE);
            WriteINIValue("Basic", "bPositive3", (bPositive3 ? "1" : "0"), INIFILE);
            WriteINIValue("Basic", "bEdgePolarity0", (bEdgePolarity0 ? "1" : "0"), INIFILE);
            WriteINIValue("Basic", "bEdgePolarity1", (bEdgePolarity1 ? "1" : "0"), INIFILE);
            WriteINIValue("Basic", "bEdgePolarity2", (bEdgePolarity2 ? "1" : "0"), INIFILE);
            WriteINIValue("Basic", "bEdgePolarity3", (bEdgePolarity3 ? "1" : "0"), INIFILE);

            //---------------------------------------------------------------------------------------------------
            // 以下改由 DtoX3MeasureSpec 於上層 RecipeFPIX3Class 處理
            //---------------------------------------------------------------------------------------------------
            //WriteINIValue("Inspect", "bCheckInspect", (bCheckInspect ? "1" : "0"), INIFILE);
            //WriteINIValue("Basic", "bOpenLineMeasure", (bOpenLineMeasure ? "1" : "0"), INIFILE);
            //WriteINIValue("Basic", "bCheckMeasureOffset", (bCheckMeasureOffset ? "1" : "0"), INIFILE);
            //WriteINIValue("Basic", "mWidthStand", mWidthStand.ToString(), INIFILE);
            //WriteINIValue("Basic", "mHeightStand", mHeightStand.ToString(), INIFILE);
            //WriteINIValue("Basic", "mWidthPercentage", mWidthPercentage.ToString(), INIFILE);
            //WriteINIValue("Basic", "mHeightPercentage", mHeightPercentage.ToString(), INIFILE);
            //WriteINIValue("Basic", "XOffset", XOffset.ToString(), INIFILE);
            //WriteINIValue("Basic", "YOffset", YOffset.ToString(), INIFILE);
            _spec.Save(INIFILE);

            //>>> SaveMaskRects();
        }

        internal void LoadMaskRects()
        {
            var maskRects = new List<RectangleF>();
            int count = int.Parse(ReadINIValue("Inspect", "RoiCount", "0", INIFILE));
            for (int i = 0; i < count; i++)
            {
                RectangleF rectf = StringtoRectF(ReadINIValue("Inspect", $"Roi{i.ToString()}", RectFtoStringSimple(new RectangleF(0, 0, 10, 10)), INIFILE));
                maskRects.Add(rectf);
            }
            this.rectangles = maskRects;
        }
        internal void SaveMaskRects()
        {
            var maskRects = this.rectangles;
            int count = maskRects != null ? maskRects.Count : 0;
            WriteINIValue("Inspect", "RoiCount", count.ToString(), INIFILE);
            for (int i = 0; i < count; i++)
            {
                WriteINIValue("Inspect", $"Roi{i.ToString()}", RectFtoStringSimple(maskRects[i]), INIFILE);
            }
        }
    }
}
