#region AUTHOR
/*
 * 
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2026-08-29 重整 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using Common.RecipeSpace;
using Eazy_Project_III;
using JetEazy;
using JetEazy.Lang;
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
        [DisplayName("02 啟用 邊隙檢測")]
        [Browsable(true)]
        public bool optPadEdgeGapsMeasurement
        {
            get => _spec.optPadEdgeGapsMeasurement;
            set => _spec.optPadEdgeGapsMeasurement = value;
        }

        [CategoryAttribute(_Cat01), DescriptionAttribute("")]
        [DisplayName("03 啟用 缺陷檢測")]
        [Browsable(true)]
        //[ReadOnly(true)]
        public bool optChipDefectsInspect
        {
            get => _spec.optChipDefectsInspect;// = false;     // 暫時不開放
            set => _spec.optChipDefectsInspect = value;
        }
        #endregion


        #region 2_整盤判定_(兩載台共用)
        const string _Cat02 = "2. 整盤判定設定";

        [CategoryAttribute(_Cat02), DescriptionAttribute("")]
        [DisplayName("01 啟用 整盤 NG百分比 判定")]
        [Browsable(true)]
        public bool optUseTotalNgPercentage
        {
            get => _spec.optUseTotalNgPercentage;
            set => _spec.optUseTotalNgPercentage = value;
        }

        [CategoryAttribute(_Cat02), DescriptionAttribute("單位 %")]
        [DisplayName("02 整盤 NG百分比 上限 (%)")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0f, 100f, 0.5f, 1)]
        [Browsable(true)]
        public float xTotalNgPercentage
        {
            get => _spec.TotalNgPercentage;
            set => _spec.TotalNgPercentage = value;
        }

        [CategoryAttribute(_Cat02), DescriptionAttribute("")]
        [DisplayName("03 顯示 個別 NG 檢測結果")]
        [Browsable(true)]
        public bool optShowIndividualNG
        {
            get => _spec.optShowIndividualNG;
            set => _spec.optShowIndividualNG = value;
        }

        [CategoryAttribute(_Cat02), DescriptionAttribute("")]
        [DisplayName("04 傾斜(踩腳) 偵測 啟用")]
        [Browsable(true)]
        public bool optTiltDetectEnabled
        {
            get => _spec.optTiltDetectEnabled;
            set => _spec.optTiltDetectEnabled = value;
        }

        [CategoryAttribute(_Cat02), DescriptionAttribute("比重值 0.000 ~ 1.000")]
        [DisplayName("05 傾斜(踩腳) 門限值")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 1, 0.001f, 4)]
        [Browsable(true)]
        public float xTiltRatioThres
        {
            get => _spec.TiltRatioThres;
            set => _spec.TiltRatioThres = value;
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

        const string _Cat03A = "3A. 晶粒定位 (格點型)";
        [CategoryAttribute(_Cat03A), DescriptionAttribute("搭配 '格點晶粒' 匹配演算法的 '格點門限值'")]
        [DisplayName("01 晶粒格點門限")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 255)]
        [Browsable(true)]
        public int xGridPadThreshold { get; set; } = 0;

        [CategoryAttribute(_Cat03A), DescriptionAttribute("搭配 '格點晶粒' 匹配演算法的 '去刮痕閥值'")]
        [DisplayName("02 去刮痕閥值")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 255)]
        [Browsable(true)]
        public int xDistTransThreshold { get; set; } = 0;

        [CategoryAttribute(_Cat03A), DescriptionAttribute("搭配 '格點晶粒' 匹配演算法的 '啟用大角度定位'")]
        [DisplayName("03 啟用大角度定位")]
        [Browsable(true)]
        public bool xUseLargePadGridAngle { get; set; } = false;

        //>>> const string _Cat1B = "A01.B '一般型' 晶粒定位";
        const string _Cat03B = "3B. 晶粒定位 (一般型)";
        [CategoryAttribute(_Cat03B), DescriptionAttribute("'一般型晶粒' 模板匹配的相似程度")]
        [DisplayName("01 晶粒相似度")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 1, 0.1f, 2)]
        [Browsable(true)]
        public float xTolerance { get; set; } = 0.5f;

        [CategoryAttribute(_Cat03B), DescriptionAttribute("'一般型晶粒' 模板匹配的允许的角度")]
        [DisplayName("02 角度範圍")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 360, 1f, 2)]
        [Browsable(true)]
        public float xAngle { get; set; } = 30f;

        [CategoryAttribute(_Cat03B), DescriptionAttribute("'一般型晶粒' 模板匹配的搜寻范围内重叠率")]
        [DisplayName("03 匹配重叠率 (%)")]
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
        [DisplayName("02 樣本尺寸X")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0.001f, 9999f, 0.010f, 3)]
        [Browsable(true)]
        public float xTemplateChipWidth
        {
            get;
            set;
        }

        [CategoryAttribute(_Cat04), DescriptionAttribute("單位 mm")]
        [DisplayName("03 樣本尺寸Y")]
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


        #region 4A_邊線前置濾波
        const string _Cat04A = "4A. 邊線前置濾波";
        [CategoryAttribute(_Cat04A)]
        [DisplayName("01 灰階上限")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 255)]
        [Browsable(false)]
        public int GrayLimitHi { get; set; } = 255;

        [CategoryAttribute(_Cat04A)]
        [DisplayName("02 灰階下限")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 255)]
        [Browsable(false)]
        public int GrayLimitLo { get; set; } = 0;
        #endregion


        #region 4B_找邊線參數_(目前不會用到, AOI 內部根據載台顏色自動設定)
        const string _Cat04B = "4B. 找邊線參數";
        [CategoryAttribute(_Cat04B)]
        [DisplayName("00 测量方式")]
        [TypeConverter(typeof(JzEnumConverter))]
        [Browsable(false)]
        public MeasureFindLineType MFLType { get; set; } = MeasureFindLineType.FindLineType_v1;

        [CategoryAttribute(_Cat04B), DescriptionAttribute("从左到右 true正向 false反向")]
        [DisplayName("01 左边查找方向")]
        [Browsable(false)]
        public bool bPositive0 { get; set; } = true;
        [CategoryAttribute(_Cat04B), DescriptionAttribute("true白到黑 false黑到白")]
        [DisplayName("02 左边极性")]
        [Browsable(false)]
        public bool bEdgePolarity0 { get; set; } = true;

        [CategoryAttribute(_Cat04B), DescriptionAttribute("从上到下 true正向 false反向")]
        [DisplayName("03 上边查找方向")]
        [Browsable(false)]
        public bool bPositive1 { get; set; } = true;
        [CategoryAttribute(_Cat04B), DescriptionAttribute("true白到黑 false黑到白")]
        [DisplayName("04 上边极性")]
        [Browsable(false)]
        public bool bEdgePolarity1 { get; set; } = true;

        [CategoryAttribute(_Cat04B), DescriptionAttribute("从左到右 true正向 false反向")]
        [DisplayName("05 右边查找方向")]
        [Browsable(false)]
        public bool bPositive2 { get; set; } = true;
        [CategoryAttribute(_Cat04), DescriptionAttribute("true白到黑 false黑到白")]
        [DisplayName("06 右边极性")]
        [Browsable(false)]
        public bool bEdgePolarity2 { get; set; } = true;

        [CategoryAttribute(_Cat04B), DescriptionAttribute("从上到下 true正向 false反向")]
        [DisplayName("07 下边查找方向")]
        [Browsable(false)]
        public bool bPositive3 { get; set; } = true;
        [CategoryAttribute(_Cat04B), DescriptionAttribute("true白到黑 false黑到白")]
        [DisplayName("08 下边极性")]
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
        [DisplayName("02 缺陷尺寸X")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 2)]
        [Browsable(true)]
        public float xCharWidth { get; set; } = 15.1f;

        [CategoryAttribute(_Cat7), DescriptionAttribute("单位pixel")]
        [DisplayName("03 缺陷尺寸Y")]
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
        [DisplayName("05 背景缺陷尺寸X")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 2)]
        [Browsable(false)]
        public float xBackgroudWidth { get; set; } = 15.1f;

        [CategoryAttribute(_Cat7), DescriptionAttribute("单位pixel")]
        [DisplayName("06 背景缺陷尺寸Y")]
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
        [DisplayName("01 標準尺寸X")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0.001f, 9999f, 0.010f, 3)]
        [Browsable(true)]
        public float mWidthStand
        {
            get => _spec.StandardWidth.Standard;
            set => _spec.StandardWidth.Standard = value;
        }

        [CategoryAttribute(_Cat5), DescriptionAttribute("單位 mm")]
        [DisplayName("01a 尺寸X上公差")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0.001f, 9999f, 0.010f, 3)]
        [Browsable(true)]
        public float mWidthDeltaUpper
        {
            get => _spec.StandardWidth.DeltaUpper;
            set => _spec.StandardWidth.DeltaUpper = value;
        }

        [CategoryAttribute(_Cat5), DescriptionAttribute("單位 mm")]
        [DisplayName("01b 尺寸X下公差")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0.001f, 9999f, 0.010f, 3)]
        [Browsable(true)]
        public float mWidthDeltaLower
        {
            get => _spec.StandardWidth.DeltaLower;
            set => _spec.StandardWidth.DeltaLower = value;
        }

        [CategoryAttribute(_Cat5), DescriptionAttribute("單位 mm")]
        [DisplayName("02 標準尺寸Y")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0.001f, 9999f, 0.010f, 3)]
        [Browsable(true)]
        public float mHeightStand
        {
            get => _spec.StandardHeight.Standard;
            set => _spec.StandardHeight.Standard = value;
        }

        [CategoryAttribute(_Cat5), DescriptionAttribute("單位 mm")]
        [DisplayName("02a 尺寸Y上公差")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0.001f, 9999f, 0.010f, 3)]
        [Browsable(true)]
        public float mHeightDeltaUpper
        {
            get => _spec.StandardHeight.DeltaUpper;
            set => _spec.StandardHeight.DeltaUpper = value;
        }

        [CategoryAttribute(_Cat5), DescriptionAttribute("單位 mm")]
        [DisplayName("02b 尺寸Y下公差")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0.001f, 9999f, 0.010f, 3)]
        [Browsable(true)]
        public float mHeightDeltaLower
        {
            get => _spec.StandardHeight.DeltaLower;
            set => _spec.StandardHeight.DeltaLower = value;
        }


        [Browsable(false)]
        public float mWidthStandMax
        {
            get => _spec.StandardWidth.Max;
        }
        [Browsable(false)]
        public float mWidthStandMin
        {
            get => _spec.StandardWidth.Min;
        }
        [Browsable(false)]
        public float mHeightStandMax
        {
            get => _spec.StandardHeight.Max;
        }
        [Browsable(false)]
        public float mHeightStandMin
        {
            get => _spec.StandardHeight.Min;
        }
        #endregion


        #region 6_邊隙_SPEC_(兩載台共用)
        const string _Cat6 = "6. PAD邊隙規格設定";

        [CategoryAttribute(_Cat6), DescriptionAttribute("僅適用於 格點晶粒! (單位 mm)")]
        [DisplayName("01 PAD邊隙 X 標準值")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0.001f, 9999f, 0.010f, 3)]
        [Browsable(true)]
        public float PadEdgeX
        {
            get => _spec.PadEdgeGapX.Standard;
            set => _spec.PadEdgeGapX.Standard = value;
        }

        [CategoryAttribute(_Cat6), DescriptionAttribute("單位 mm")]
        [DisplayName("01a PAD邊隙 X 上公差")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0.001f, 9999f, 0.010f, 3)]
        [Browsable(true)]
        public float PadEdgeX_DeltaUpper
        {
            get => _spec.PadEdgeGapX.DeltaUpper;
            set => _spec.PadEdgeGapX.DeltaUpper = value;
        }

        [CategoryAttribute(_Cat6), DescriptionAttribute("單位 mm")]
        [DisplayName("01b PAD邊隙 X 下公差")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0.001f, 9999f, 0.010f, 3)]
        [Browsable(true)]
        public float PadEdgeX_DeltaLower
        {
            get => _spec.PadEdgeGapX.DeltaLower;
            set => _spec.PadEdgeGapX.DeltaLower = value;
        }

        [CategoryAttribute(_Cat6), DescriptionAttribute("僅適用於 格點晶粒! (單位 mm)")]
        [DisplayName("02 PAD邊隙 Y 標準值")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0.001f, 9999f, 0.010f, 3)]
        [Browsable(true)]
        public float PadEdgeY
        {
            get => _spec.PadEdgeGapY.Standard;
            set => _spec.PadEdgeGapY.Standard = value;
        }

        [CategoryAttribute(_Cat6), DescriptionAttribute("單位 mm")]
        [DisplayName("02a PAD邊隙 Y 上公差")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0.001f, 9999f, 0.010f, 3)]
        [Browsable(true)]
        public float PadEdgeY_DeltaUpper
        {
            get => _spec.PadEdgeGapY.DeltaUpper;
            set => _spec.PadEdgeGapY.DeltaUpper = value;
        }

        [CategoryAttribute(_Cat6), DescriptionAttribute("單位 mm")]
        [DisplayName("02b PAD邊隙 Y 下公差")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0.001f, 9999f, 0.010f, 3)]
        [Browsable(true)]
        public float PadEdgeY_DeltaLower
        {
            get => _spec.PadEdgeGapY.DeltaLower;
            set => _spec.PadEdgeGapY.DeltaLower = value;
        }

        [CategoryAttribute(_Cat6), DescriptionAttribute("單位 mm")]
        [DisplayName("03 PAD邊隙 左右差異 上限")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0.001f, 9999f, 0.010f, 3)]
        [Browsable(true)]
        public float PadEdgeX_Diff_Upper
        {
            get => _spec.PadEdgeGapDiffX.DeltaUpper;
            set => _spec.PadEdgeGapDiffX.DeltaUpper = value;
        }

        [Browsable(false)]
        public float PadEdgeGapX_Max
        {
            get => _spec.PadEdgeGapX.Max;
        }
        [Browsable(false)]
        public float PadEdgeGapX_Min
        {
            get => _spec.PadEdgeGapX.Min;
        }
        [Browsable(false)]
        public float PadEdgeGapY_Max
        {
            get => _spec.PadEdgeGapY.Max;
        }
        [Browsable(false)]
        public float PadEdgeGapY_Min
        {
            get => _spec.PadEdgeGapY.Min;
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
            xDistTransThreshold = int.Parse(ReadINIValue("Basic", "xDistTransThreshold", "0", INIFILE));
            xUseLargePadGridAngle = int.Parse(ReadINIValue("Basic", "xUseLargePadGridAngle", "0", INIFILE)) == 1;

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
                xTemplateChipWidth = _spec.StandardWidth.Standard;
            if (xTemplateChipHeight <= 0)
                xTemplateChipHeight = _spec.StandardHeight.Standard;

            if (xChipDimScaleW <= 0)
                xChipDimScaleW = 1.0;
            if (xChipDimScaleH <= 0)
                xChipDimScaleH = 1.0;

            GrayLimitHi = int.Parse(ReadINIValue("LineBorder", "GrayLimitHi", "255", INIFILE));
            GrayLimitLo = int.Parse(ReadINIValue("LineBorder", "GrayLimitLo", "0", INIFILE));

            //---------------------------------------------------------------------------------------------------
            // 以下 找邊線參數 目前沒有用到 (AOI 內部根據載台顏色自動設定)
            //---------------------------------------------------------------------------------------------------
            #region NOT_USED_CODE
            //MFLType = (MeasureFindLineType)int.Parse(ReadINIValue("Basic", "MFLType", "0", INIFILE));
            //bPositive0 = ReadINIValue("Basic", "bPositive0", "1", INIFILE) == "1";
            //bPositive1 = ReadINIValue("Basic", "bPositive1", "1", INIFILE) == "1";
            //bPositive2 = ReadINIValue("Basic", "bPositive2", "1", INIFILE) == "1";
            //bPositive3 = ReadINIValue("Basic", "bPositive3", "1", INIFILE) == "1";
            //bEdgePolarity0 = ReadINIValue("Basic", "bEdgePolarity0", "1", INIFILE) == "1";
            //bEdgePolarity1 = ReadINIValue("Basic", "bEdgePolarity1", "1", INIFILE) == "1";
            //bEdgePolarity2 = ReadINIValue("Basic", "bEdgePolarity2", "1", INIFILE) == "1";
            //bEdgePolarity3 = ReadINIValue("Basic", "bEdgePolarity3", "1", INIFILE) == "1";
            #endregion

            //---------------------------------------------------------------------------------------------------
            // 以下改由 DtoX3MeasureSpec 於上層 RecipeFPIX3Class 處理
            //---------------------------------------------------------------------------------------------------
            #region REPLACED_BY_DTO
            //bCheckInspect = ReadINIValue("Inspect", "bCheckInspect", "1", INIFILE) == "1";
            //bOpenLineMeasure = ReadINIValue("Basic", "bOpenLineMeasure", "0", INIFILE) == "1";
            //bCheckMeasureOffset = ReadINIValue("Basic", "bCheckMeasureOffset", "0", INIFILE) == "1";
            //mWidthStand = float.Parse(ReadINIValue("Basic", "mWidthStand", "9", INIFILE));
            //mHeightStand = float.Parse(ReadINIValue("Basic", "mHeightStand", "9.9", INIFILE));
            //mWidthPercentage = float.Parse(ReadINIValue("Basic", "mWidthPercentage", "1.0", INIFILE));
            //mHeightPercentage = float.Parse(ReadINIValue("Basic", "mHeightPercentage", "1.0", INIFILE));
            //XOffset = float.Parse(ReadINIValue("Basic", "XOffset", "0.05", INIFILE));
            //YOffset = float.Parse(ReadINIValue("Basic", "YOffset", "0.05", INIFILE));
            #endregion
            _spec.Load(INIFILE);

            //---------------------------------------------------------------------------------------------------
            // 以下改由 LoadMaskRects 處理
            //---------------------------------------------------------------------------------------------------
            #region REPLACED_BY_LoadMaskRects
            //RoiCount = int.Parse(ReadINIValue("Inspect", "RoiCount", "0", INIFILE));
            //int i = 0;
            //rectangles.Clear();
            //while (i < RoiCount)
            //{
            //    RectangleF rectf = StringtoRectF(ReadINIValue("Inspect", $"Roi{i.ToString()}", RectFtoStringSimple(new RectangleF(0, 0, 10, 10)), INIFILE));
            //    rectangles.Add(rectf);
            //    i++;
            //}
            #endregion
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
            WriteINIValue("Basic", "xDistTransThreshold", xDistTransThreshold.ToString(), INIFILE);
            WriteINIValue("Basic", "xUseLargePadGridAngle", xUseLargePadGridAngle ? "1" : "0", INIFILE);

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
            // 找邊線前置濾波
            //---------------------------------------------------------------------------------------------------
            WriteINIValue("LineBorder", "GrayLimitHi", GrayLimitHi.ToString(), INIFILE);
            WriteINIValue("LineBorder", "GrayLimitLo", GrayLimitLo.ToString(), INIFILE);

            //---------------------------------------------------------------------------------------------------
            // 以下 找邊線參數 目前沒有用到 (AOI 內部根據載台顏色自動設定)
            //---------------------------------------------------------------------------------------------------
            #region NOT_USED_CODE
            //WriteINIValue("Basic", "MFLType", ((int)MFLType).ToString(), INIFILE);
            //WriteINIValue("Basic", "bPositive0", (bPositive0 ? "1" : "0"), INIFILE);
            //WriteINIValue("Basic", "bPositive1", (bPositive1 ? "1" : "0"), INIFILE);
            //WriteINIValue("Basic", "bPositive2", (bPositive2 ? "1" : "0"), INIFILE);
            //WriteINIValue("Basic", "bPositive3", (bPositive3 ? "1" : "0"), INIFILE);
            //WriteINIValue("Basic", "bEdgePolarity0", (bEdgePolarity0 ? "1" : "0"), INIFILE);
            //WriteINIValue("Basic", "bEdgePolarity1", (bEdgePolarity1 ? "1" : "0"), INIFILE);
            //WriteINIValue("Basic", "bEdgePolarity2", (bEdgePolarity2 ? "1" : "0"), INIFILE);
            //WriteINIValue("Basic", "bEdgePolarity3", (bEdgePolarity3 ? "1" : "0"), INIFILE);
            #endregion

            //---------------------------------------------------------------------------------------------------
            // 以下改由 DtoX3MeasureSpec 於上層 RecipeFPIX3Class 處理
            //---------------------------------------------------------------------------------------------------
            #region REPLACED_BY_DTO
            //WriteINIValue("Inspect", "bCheckInspect", (bCheckInspect ? "1" : "0"), INIFILE);
            //WriteINIValue("Basic", "bOpenLineMeasure", (bOpenLineMeasure ? "1" : "0"), INIFILE);
            //WriteINIValue("Basic", "bCheckMeasureOffset", (bCheckMeasureOffset ? "1" : "0"), INIFILE);
            //WriteINIValue("Basic", "mWidthStand", mWidthStand.ToString(), INIFILE);
            //WriteINIValue("Basic", "mHeightStand", mHeightStand.ToString(), INIFILE);
            //WriteINIValue("Basic", "mWidthPercentage", mWidthPercentage.ToString(), INIFILE);
            //WriteINIValue("Basic", "mHeightPercentage", mHeightPercentage.ToString(), INIFILE);
            //WriteINIValue("Basic", "XOffset", XOffset.ToString(), INIFILE);
            //WriteINIValue("Basic", "YOffset", YOffset.ToString(), INIFILE);
            #endregion
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
