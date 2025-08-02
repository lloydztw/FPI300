using Common.RecipeSpace;
using Eazy_Project_III;
using FreeImageAPI;
using JetEazy;
using JetEazy.BasicSpace;
using LaserAlignDX.OPSpace;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Traveller106;
using TravellerMINIX6.BasicSpace.ReadBarcode;
using TravellerMINIX6.OPSpace;

namespace Common.RecipeSpace
{
    public class RecipeMiniX6Class : RecipeBaseClass
    {
        protected RecipeMiniX6Class()
        {

        }
        private static RecipeMiniX6Class _instance = null;
        public static RecipeMiniX6Class Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new RecipeMiniX6Class();
                return _instance;
            }
        }

        /// <summary>
        /// 这是测试分配的VIEW
        /// </summary>
        public AnalyzeClass ViewAnalyzeClass = new AnalyzeClass();

        const string LSCat0 = "A00.基础设置";

        [CategoryAttribute(LSCat0), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("00.Tray盘行数")]
        public int TrayRowCount { get; set; } = 1;
        [CategoryAttribute(LSCat0), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("01.Tray盘列数")]
        public int TrayColCount { get; set; } = 1;

        [Browsable(false)]
        public Bitmap bmpORG { get; set; } = new Bitmap(1, 1);

        [Browsable(false)]
        public Bitmap bmpORGPattern { get; set; } = new Bitmap(1, 1);

        [Browsable(false)]
        public Bitmap bmpOrgDirPattern { get; set; } = new Bitmap(1, 1);
        [Browsable(false)]
        public Bitmap bmpOrgAnaDirPattern { get; set; } = new Bitmap(1, 1);

        [Browsable(false)]
        public RectangleF rect_start { get; set; } = new RectangleF(0, 0, 100, 100);
        [Browsable(false)]
        public RectangleF rect_end { get; set; } = new RectangleF(300, 300, 100, 100);

        [Browsable(false)]
        public RectangleF Mark0 { get; set; } = new RectangleF(0, 0, 100, 100);
        [Browsable(false)]
        public RectangleF Mark1 { get; set; } = new RectangleF(300, 300, 100, 100);
        [Browsable(false)]
        public RectangleF MarkLine { get; set; } = new RectangleF(300, 300, 100, 100);

        [Browsable(false)]
        public RectangleF MarkLine1 { get; set; } = new RectangleF(300, 300, 100, 100);
        [Browsable(false)]
        public RectangleF MarkLine2 { get; set; } = new RectangleF(300, 300, 100, 100);

        [Browsable(false)]
        public RectangleF SideLeft { get; set; } = new RectangleF(300, 300, 100, 100);
        [Browsable(false)]
        public RectangleF SideRight { get; set; } = new RectangleF(300, 300, 100, 100);
        [Browsable(false)]
        public RectangleF SideTop { get; set; } = new RectangleF(300, 300, 100, 100);
        [Browsable(false)]
        public RectangleF SideBottom { get; set; } = new RectangleF(300, 300, 100, 100);

        [Browsable(false)]
        public RectangleF DirRectF { get; set; } = new RectangleF(0, 0, 100, 100);

        [Browsable(false)]
        public RectangleF AnaDirRectF { get; set; } = new RectangleF(0, 0, 100, 100);


        [Browsable(false)]
        public RectangleF AnaReadBarcodeRectF { get; set; } = new RectangleF(0, 0, 100, 100);

        [Browsable(false)]
        public PointF ptMark0 { get; set; } = new PointF(0, 0);
        [Browsable(false)]
        public PointF ptMark1 { get; set; } = new PointF(0, 0);
       

        [Browsable(false)]
        public List<RectangleF> rect_list { get; set; } = new List<RectangleF>();

        [Browsable(false)]
        public string MappingInspectStr = "1";

        [CategoryAttribute(LSCat0), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("03.是否画出位置框")]
        [Browsable(true)]
        public bool IsDrawRect { get; set; } = false;

        [CategoryAttribute(LSCat0), DescriptionAttribute("")]
        [DisplayName("02.是否开启定位")]
        [Browsable(true)]
        public bool IsOpenAuFind { get; set; } = false;

        [CategoryAttribute(LSCat0), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("02A.相似度")]
        [Browsable(true)]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 1, 0.1f, 2)]
        public float inspect_tolerance { get; set; } = 0.8f;

        [CategoryAttribute(LSCat0), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("02B.角度")]
        [Browsable(true)]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 180, 0.1f, 2)]
        public float inspect_angle { get; set; } = 30;

        [CategoryAttribute(LSCat0), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("02C.采样值")]
        [Browsable(true)]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 300)]
        public int inspect_samplesize { get; set; } = 100;


        [CategoryAttribute(LSCat0), DescriptionAttribute("")]
        [DisplayName("03.是否开启读码")]
        [Browsable(true)]
        public bool IsOpenReadBarcode { get; set; } = false;

        //粗定位坐标集合

        const string _Cat2 = "A02.图像处理";
        [CategoryAttribute(_Cat2), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("01.取样宽度")]
        [Browsable(true)]
        public int CropWidth { get; set; } = 33;

        [CategoryAttribute(_Cat2), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("02.取样间隔")]
        [Browsable(true)]
        public int SampleValue { get; set; } = 7;

        [CategoryAttribute(_Cat2), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("03.预处理模式")]
        [Browsable(true)]
        [TypeConverter(typeof(JzEnumConverter))]
        public ProcessImageMode PreMode { get; set; } = ProcessImageMode.V2;

        [CategoryAttribute(_Cat2), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("04A.灰阶阈值")]
        [Browsable(true)]
        public int PreThresholdValue { get; set; } = 50;

        [CategoryAttribute(_Cat2), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("04A.灰阶阈值Mark2")]
        [Browsable(true)]
        public int PreThresholdValueMark2 { get; set; } = 50;

        [CategoryAttribute(_Cat2), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("05.反向寻找")]
        [Browsable(true)]
        public bool PreDir { get; set; } = false;

        [CategoryAttribute(_Cat2), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("06.中心横向外扩")]
        [Browsable(true)]
        public int PreCenterExtendx { get; set; } = 5;
        [CategoryAttribute(_Cat2), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("07.中心纵向外扩")]
        [Browsable(true)]
        public int PreCenterExtendy { get; set; } = 5;

        [CategoryAttribute(_Cat2), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("08.点到直线距离")]
        [Browsable(true)]
        public float PreLineLeastDistance { get; set; } = 0.5f;

        [CategoryAttribute(_Cat2), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("09.Mark点最小面积")]
        [Browsable(true)]
        public int MarkMinArea { get; set; } = 500;

        [CategoryAttribute(_Cat2), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("10.Mark直线灰阶阈值")]
        [Browsable(false)]
        public int PreMarkLineThresholdValue { get; set; } = 50;


        [CategoryAttribute(_Cat2), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("11.标刻起点位置")]
        [Browsable(true)]
        [TypeConverter(typeof(JzEnumConverter))]
        public LaserStartPos PreLaserPos { get; set; } = LaserStartPos.LeftBottom;

        [CategoryAttribute(_Cat2), DescriptionAttribute("灯光亮度值0~255")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("12.灯光亮度")]
        [Browsable(true)]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(120, 255, 1)]
        public int LightValue { get; set; } = 255;

        [CategoryAttribute(_Cat2), DescriptionAttribute("直线角度")]
        [Browsable(false)]
        [ReadOnly(false)]
        [DisplayName("13.直线角度")]
        public double MarkLineAngle { get; set; } = 0;


        const string _Cat3 = "A03.镭雕参数设置";

        [CategoryAttribute(_Cat3), DescriptionAttribute("标刻位置X")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("01.标刻偏移X")]
        [Browsable(true)]
        public float LaserLocX { get; set; } = 0;
        [CategoryAttribute(_Cat3), DescriptionAttribute("标刻偏移Y")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("02.标刻偏移Y")]
        [Browsable(true)]
        public float LaserLocY { get; set; } = 0;
        [CategoryAttribute(_Cat3), DescriptionAttribute("标刻偏移A角度")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("03.标刻偏移A角度")]
        [Browsable(false)]
        public float LaserLocA { get; set; } = 0;


        const string _Cat6 = "A06.双Mark点方式";
        [CategoryAttribute(_Cat6), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("01.Mark0X距离")]
        [Browsable(true)]
        public float k0x { get; set; } = 0;
        [CategoryAttribute(_Cat6), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("01.Mark0Y距离")]
        [Browsable(true)]
        public float k0y { get; set; } = 0;

        [CategoryAttribute(_Cat6), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("02.Mark1X距离")]
        [Browsable(true)]
        public float k1x { get; set; } = 10;
        [CategoryAttribute(_Cat6), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("02.Mark1Y距离")]
        [Browsable(true)]
        public float k1y { get; set; } = 10;

        //const string _Cat7 = "A07.四边方式";

        const string _Cat1 = "A01.基础设置";
        //[CategoryAttribute(_Cat1), DescriptionAttribute("")]
        ////[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        ////[DisplayName("特征定位坐标集合")]
        //[Browsable(true)]
        //public List<NeedleXYZ> CoarsePositioningList = new List<NeedleXYZ>();



        //精定位坐标集合

        //初始高度 即用来对焦计算距离的初始高度
        [CategoryAttribute(_Cat1), DescriptionAttribute("即用来对焦计算距离的初始高度")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("对焦Z初始高度")]
        [Browsable(false)]
        public double BaseHeightZ { get; set; } = 0;

        //相机曝光和增益
        [CategoryAttribute(_Cat1), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("相机曝光集合")]
        [Browsable(false)]
        public string sExposureList { get; set; } = "";
        public string[] ExposureArray = new string[8];
        //public List<string> ExposureList = new List<string>();

        [CategoryAttribute(_Cat1), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("相机增益集合")]
        [Browsable(false)]
        public string sGainList { get; set; } = "";
        public string[] GainArray = new string[8];
        //public List<string> GainList = new List<string>();


        //对焦范围 分上限和下限
        [CategoryAttribute(_Cat1), DescriptionAttribute("即初始高度正方向的上下限分为正负对称距离")]
        [DisplayName("对焦上下限")]
        [Browsable(false)]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 1, 0.01f, 3)]
        public double FocusUpperAndLower { get; set; } = 0.2;

        //[CategoryAttribute(_Cat1), DescriptionAttribute("即初始高度正方向的上限")]
        //[DisplayName("对焦上限")]
        //[Browsable(true)]
        //[TypeConverter(typeof(NumericUpDownTypeConverter))]
        //[Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 1, 0.01f, 3)]
        //public double FocusUpper { get; set; } = 0.2;

        #region 线扫相关参数设置

        const string LSCat4 = "A04.线扫相关参数设置";
        [CategoryAttribute(LSCat4), DescriptionAttribute("单位mm")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("00.线扫步间距")]
        [Browsable(false)]
        public double chip_linescanoffset { get; set; } = 43;

        [CategoryAttribute(LSCat4), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("01.拍照次数")]
        [Browsable(false)]
        public int chip_captureCount { get; set; } = 2;

        [CategoryAttribute(LSCat4), DescriptionAttribute("true启用测试 false不启用测试 主要用于调试只抓图不测试")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("02.启用测量")]
        [Browsable(true)]
        public bool Ischip_open_measure { get; set; } = true;

        [CategoryAttribute(LSCat4), DescriptionAttribute("单位pixel 用于16K分区域测试 8K则不需要切图")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("切图次数")]
        [Browsable(false)]
        public int clone_count { get; set; } = 2;

        [CategoryAttribute(LSCat4), DescriptionAttribute("单位pixel")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("切图位置1")]
        [Browsable(false)]
        public int clone1 { get; set; } = 5;

        [CategoryAttribute(LSCat4), DescriptionAttribute("单位pixel")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("切图位置2")]
        [Browsable(false)]
        public int clone2 { get; set; } = 5;

        [CategoryAttribute(LSCat4), DescriptionAttribute("单位pixel")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("切图宽度")]
        [Browsable(false)]
        public int clone_width { get; set; } = 6000;

        #endregion

        #region V2找边设定

        const string LSCat5 = "A05.四周找边设定";

        [CategoryAttribute(LSCat5), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A00.四边标刻起点位置")]
        [Browsable(true)]
        [TypeConverter(typeof(JzEnumConverter))]
        public LaserOffsetStart fourStartPosition { get; set; } = LaserOffsetStart.Center;

        [CategoryAttribute(LSCat5), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A01.左边从左往右")]
        [Browsable(true)]
        public bool dir_left { get; set; } = true;
        [CategoryAttribute(LSCat5), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A01.左边从白到黑")]
        [Browsable(true)]
        public bool dir_left_black { get; set; } = false;


        [CategoryAttribute(LSCat5), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A02.右边从左往右")]
        [Browsable(true)]
        public bool dir_right { get; set; } = false;
        [CategoryAttribute(LSCat5), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A02.右边从白到黑")]
        [Browsable(true)]
        public bool dir_right_black { get; set; } = false;


        [CategoryAttribute(LSCat5), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A03.上边从上往下")]
        [Browsable(true)]
        public bool dir_top { get; set; } = true;
        [CategoryAttribute(LSCat5), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A03.上边从白到黑")]
        [Browsable(true)]
        public bool dir_top_black { get; set; } = false;

        [CategoryAttribute(LSCat5), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A04.下边从上往下")]
        [Browsable(true)]
        public bool dir_bottom { get; set; } = false;
        [CategoryAttribute(LSCat5), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A04.下边从白到黑")]
        [Browsable(true)]
        public bool dir_bottom_black { get; set; } = false;

        #endregion


        #region 检测方向设定

        const string LSCat6 = "A06.检测方向设定";

        [CategoryAttribute(LSCat6), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A01.开启判断方向")]
        [Browsable(true)]
        public bool inspect_dir_open { get; set; } = false;

        [CategoryAttribute(LSCat6), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A02.相似度")]
        [Browsable(true)]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 1, 0.1f, 2)]
        public float inspect_dir_tolerance { get; set; } = 0.8f;

        [CategoryAttribute(LSCat6), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A03.角度")]
        [Browsable(true)]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 180, 0.1f, 2)]
        public float inspect_dir_angle { get; set; } = 15;

        [CategoryAttribute(LSCat6), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A04.采样值")]
        [Browsable(true)]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 300)]
        public int inspect_dir_samplesize { get; set; } = 50;

        [CategoryAttribute(LSCat6), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A05.外扩X")]
        [Browsable(true)]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999)]
        public int inspect_dir_extendx { get; set; } = 20;

        [CategoryAttribute(LSCat6), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A06.外扩Y")]
        [Browsable(true)]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999)]
        public int inspect_dir_extendy { get; set; } = 20;

        #endregion

        #region V9四点定位参数

        const string LSCat7 = "A07.四点定位参数";

        [CategoryAttribute(LSCat7), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A01.四点Mark1灰度阈值")]
        [Browsable(false)]
        public int thresholdv_mark1 { get; set; } = 60;
        [CategoryAttribute(LSCat7), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A01.四点Mark1找白斑")]
        [Browsable(false)]
        public bool findwhite_mark1 { get; set; } = false;


        [CategoryAttribute(LSCat7), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A02.四点Mark2灰度阈值")]
        [Browsable(false)]
        public int thresholdv_mark2 { get; set; } = 60;
        [CategoryAttribute(LSCat7), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A02.四点Mark2找白斑")]
        [Browsable(false)]
        public bool findwhite_mark2 { get; set; } = false;


        [CategoryAttribute(LSCat7), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A03.四点Mark3灰度阈值")]
        [Browsable(false)]
        public int thresholdv_mark3 { get; set; } = 60;
        [CategoryAttribute(LSCat7), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A03.四点Mark3找白斑")]
        [Browsable(false)]
        public bool findwhite_mark3 { get; set; } = false;


        [CategoryAttribute(LSCat7), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A04.四点Mark4灰度阈值")]
        [Browsable(false)]
        public int thresholdv_mark4 { get; set; } = 60;
        [CategoryAttribute(LSCat7), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A04.四点Mark4找白斑")]
        [Browsable(false)]
        public bool findwhite_mark4 { get; set; } = false;

        [Browsable(false)]
        public RectangleF fourmark1 { get; set; } = new RectangleF(300, 300, 100, 100);
        [Browsable(false)]
        public RectangleF fourmark2 { get; set; } = new RectangleF(300, 300, 100, 100);
        [Browsable(false)]
        public RectangleF fourmark3 { get; set; } = new RectangleF(300, 300, 100, 100);
        [Browsable(false)]
        public RectangleF fourmark4 { get; set; } = new RectangleF(300, 300, 100, 100);


        [CategoryAttribute(LSCat7), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("Mark点集合")]
        [Browsable(false)]
        public string sMarkCollectionStr { get; set; } = "";
        [CategoryAttribute(LSCat7), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("Mark点集合")]
        [Browsable(false)]
        public string sMarkCollectionStr1X { get; set; } = "";
        public MarkItemClass[] MarkCollectionStr = new MarkItemClass[4];

        #endregion

        #region 四边标准卡控

        const string LSCat8 = "A06.四边标准卡控设定";

        [CategoryAttribute(LSCat8), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A01.标准宽度")]
        [Browsable(true)]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 4)]
        public float stand_width { get; set; } = 55;
        [CategoryAttribute(LSCat8), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A01.宽度上限")]
        [Browsable(true)]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 4)]
        public float stand_width_upper { get; set; } = 3;
        [CategoryAttribute(LSCat8), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A01.宽度下限")]
        [Browsable(true)]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 4)]
        public float stand_width_lower { get; set; } = 3;


        [CategoryAttribute(LSCat8), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A02.标准高度")]
        [Browsable(true)]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 4)]
        public float stand_height { get; set; } = 36;
        [CategoryAttribute(LSCat8), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A02.高度上限")]
        [Browsable(true)]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 4)]
        public float stand_height_upper { get; set; } = 3;
        [CategoryAttribute(LSCat8), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A02.高度下限")]
        [Browsable(true)]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 4)]
        public float stand_height_lower { get; set; } = 3;


        [CategoryAttribute(LSCat8), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A03.标准角度")]
        [Browsable(true)]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(-180, 180, 0.1f, 4)]
        public float stand_angle { get; set; } = 0;
        [CategoryAttribute(LSCat8), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A03.角度上限")]
        [Browsable(true)]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 180, 0.1f, 4)]
        public float stand_angle_upper { get; set; } = 5;
        [CategoryAttribute(LSCat8), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A03.角度下限")]
        [Browsable(true)]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 180, 0.1f, 4)]
        public float stand_angle_lower { get; set; } = 5;


        #endregion

        #region 读码

        JetEazy.PlugSpace.BarcodeEx.BarcodeAll_MVD m_MvdCnnReader = new JetEazy.PlugSpace.BarcodeEx.BarcodeAll_MVD();

        /// <summary>
        /// 读码
        /// </summary>
        /// <param name="bmpInput">输入图像</param>
        /// <returns>码结果</returns>
        public string AnaReadBarcode(Bitmap bmpInput)
        {
            string retStr = string.Empty;
            if (!IsOpenReadBarcode)
                return retStr;
            RectangleF rect = new RectangleF(0, 0, bmpInput.Width, bmpInput.Height);
            Bitmap bmpTrain = bmpInput.Clone(rect, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            try
            {
                System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Restart();
                AForge.Imaging.Filters.Grayscale grayscale = new AForge.Imaging.Filters.Grayscale(0.299, 0.587, 0.114);
                Bitmap bmpgray = grayscale.Apply(bmpTrain);
                AForge.Imaging.Filters.ResizeBilinear resizeBilinear 
                    = new AForge.Imaging.Filters.ResizeBilinear(bmpTrain.Width * 1, bmpTrain.Height * 1);
                Bitmap bmpresizeBilinear = resizeBilinear.Apply(bmpgray);
                //bmpresizeBilinear.Save("D:\\1.BMP", System.Drawing.Imaging.ImageFormat.Bmp);
                string tmpStr = m_MvdCnnReader.DecodeStr(bmpresizeBilinear);

                if (string.IsNullOrEmpty(tmpStr))
                {
                    BarcodeGzx1Class IxBarcode = new BarcodeGzx1Class();
                    IxBarcode.InputImage = bmpresizeBilinear;
                    //if (m_UseAIFromPy)
                    //    IxBarcode.SetEzSeg(model);
                    int iret = IxBarcode.Run();
                    tmpStr = IxBarcode.BarcodeStr;
                    //if (iret == 0)
                    //{
                    //    lblResult.Text = "读取成功：" + IxBarcode.BarcodeStr + Environment.NewLine;
                    //    lblResult.Text += "耗时：" + IxBarcode.ElapsedTime.ToString() + " ms";
                    //}
                    //else
                    //    lblResult.Text = "读取失败：错误码=" + iret.ToString();
                }

                if (string.IsNullOrEmpty(tmpStr))
                {
                    resizeBilinear = new AForge.Imaging.Filters.ResizeBilinear(bmpTrain.Width * 2, bmpTrain.Height * 2);
                    bmpresizeBilinear = resizeBilinear.Apply(bmpgray);

                    tmpStr = m_MvdCnnReader.DecodeStr(bmpresizeBilinear);
                }
                bmpgray.Dispose();
                bmpresizeBilinear.Dispose();
                stopwatch.Stop();

                retStr = tmpStr;
            }
            catch
            {
                retStr = string.Empty;
            }

            bmpTrain.Dispose();
            return retStr;
        }

        #endregion


        #region 整版判断方向及特征
        [Browsable(false)]
        public AUFindClass Ana_myFind_dir = new AUFindClass();
        [Browsable(false)]
        public FindParaClass Ana_paraClass_dir = new FindParaClass();

        /// <summary>
        /// 判断方向训练
        /// </summary>
        /// <param name="bmpInput">输入图像</param>
        /// <returns></returns>
        public bool AnaTrainDir(Bitmap bmpInput)
        {
            if (!inspect_dir_open)
                return true;

            Ana_paraClass_dir.tolerance = inspect_dir_tolerance;
            Ana_paraClass_dir.angle = inspect_dir_angle;
            Ana_paraClass_dir.samplesize = inspect_dir_samplesize;

            RectangleF rect = new RectangleF(0, 0, bmpInput.Width, bmpInput.Height);
            //取出Train的小图
            Bitmap bmpTrain = bmpInput.Clone(rect, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            Ana_myFind_dir.bmpItem = bmpTrain;
            bool bOK = Ana_myFind_dir.Train2(rect, Ana_paraClass_dir);

            bmpTrain.Dispose();
            return bOK;
        }
        /// <summary>
        /// 判断方向测试
        /// </summary>
        /// <param name="bmpInput">输入图像</param>
        /// <param name="nDesc">返回的信息</param>
        /// <returns>0:OK 小于0:NG</returns>
        public int AnaRunDir(Bitmap bmpInput, out string nDesc)
        {
            nDesc = string.Empty;
            if (!inspect_dir_open)
            {
                nDesc = $"未启用定位";
                return 0;
            }

            Ana_myFind_dir.bmpFind = bmpInput;
            bool bOK = Ana_myFind_dir.FindLarge();
            if (bOK)
            {
                if (Ana_myFind_dir.xResults.Count > 0)
                {
                    if (Ana_myFind_dir.xResults[0].fScore < Ana_paraClass_dir.tolerance)
                    {
                        nDesc = $"方向定位失败,分数{Ana_myFind_dir.xResults[0].fScore}";
                        return -1;
                    }
                }
                else
                {
                    nDesc = $"方向定位失败";
                    return -2;
                }
            }
            else
            {
                nDesc = $"方向定位失败";
                return -3;
            }

            return 0;
        }

        public Bitmap[] bmpMarkOrg = new Bitmap[4];
        public int ViewTrain()
        {
            switch (PreMode)
            {
                case ProcessImageMode.V9:

                    bmpMarkOrg[0].Dispose();
                    bmpMarkOrg[0] = bmpORG.Clone(fourmark1, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    bmpMarkOrg[1].Dispose();
                    bmpMarkOrg[1] = bmpORG.Clone(fourmark2, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    bmpMarkOrg[2].Dispose();
                    bmpMarkOrg[2] = bmpORG.Clone(fourmark3, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    bmpMarkOrg[3].Dispose();
                    bmpMarkOrg[3] = bmpORG.Clone(fourmark4, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    bool bOK = true;
                    int i = 0;
                    foreach (var item in MarkCollectionStr)
                    {
                        bOK &= item.AnaTrain(bmpMarkOrg[i]);
                        if (bOK)
                            item.GetImageMarkCenter(bmpMarkOrg[i], true, out RectangleF maxtmp, out Bitmap bmptmp, out int areatmp);
                        i++;
                    }
                    if (!bOK)
                        return -1;

                    break;
            }

            return 0;
        }


        #endregion

        public bool[] QcPass = null;
        public int SetQcPassData(bool[] eValue)
        {
            if (eValue.Length != QcPass.Length)
                return -1;
            eValue.CopyTo(QcPass, 0);
            return 0;
        }
        public bool[] QcPassManual = null;
        public int SetQcPassManualData()
        {
            if (MappingInspectStr.Length != QcPassManual.Length)
                return -1;
            //eValue.CopyTo(QcPassManual, 0);
            int i = 0;
            while (i < MappingInspectStr.Length)
            {
                QcPassManual[i] = MappingInspectStr.ToCharArray()[i] == '0';

                i++;
            }
            return 0;
        }

        public override void Load(bool eCancel = false)
        {
            int i = 0;

            while (i < 4)
            {
                if (bmpMarkOrg[i] == null)
                    bmpMarkOrg[i] = new Bitmap(1, 1);
                i++;
            }

            BaseHeightZ = double.Parse(ReadINIValue("Basic Control", "BaseHeightZ", BaseHeightZ.ToString(Format), INIFILE));
            FocusUpperAndLower = double.Parse(ReadINIValue("Basic Control", "FocusUpperAndLower", FocusUpperAndLower.ToString(Format), INIFILE));
            sExposureList = ReadINIValue("Cam Control", "sExposureList", sExposureList.ToString(), INIFILE);
            ExposureArray = sExposureList.Split(';').ToArray();
            if (ExposureArray.Length < 8)
            {
                ExposureArray = new string[8];
                i = 0;
                while (i < 8)
                {
                    ExposureArray[i] = "5000";
                    i++;
                }
            }
            sGainList = ReadINIValue("Cam Control", "sGainList", sGainList.ToString(), INIFILE);
            GainArray = sGainList.Split(';').ToArray();
            if (GainArray.Length < 8)
            {
                GainArray = new string[8];
                i = 0;
                while (i < 8)
                {
                    GainArray[i] = "0";
                    i++;
                }
            }

            LoadFourSetup();
            //sMarkCollectionStr = ReadINIValue("Mark Control", "sMarkCollectionStr", sMarkCollectionStr.ToString(), INIFILE);
            //string[] strs = sMarkCollectionStr.Split(';').ToArray();
            //if (strs.Length < 4)
            //{
            //    MarkCollectionStr = new MarkItemClass[4];
            //    i = 0;
            //    while (i < 4)
            //    {
            //        MarkCollectionStr[i] = new MarkItemClass();
            //        MarkCollectionStr[i].Index = i;
            //        i++;
            //    }
            //}
            //else
            //{
            //    i = 0;
            //    while (i < 4)
            //    {
            //        MarkCollectionStr[i] = new MarkItemClass();
            //        MarkCollectionStr[i].FormString(strs[i]);
            //        MarkCollectionStr[i].Index = i;
            //        i++;
            //    }
            //}

            rect_start = StringtoRectF(ReadINIValue("Recipe Basic", "rect_start", RectFtoStringSimple(rect_start), INIFILE));
            rect_end = StringtoRectF(ReadINIValue("Recipe Basic", "rect_end", RectFtoStringSimple(rect_end), INIFILE));

            Mark0 = StringtoRectF(ReadINIValue("Recipe Basic", "Mark0", RectFtoStringSimple(Mark0), INIFILE));
            Mark1 = StringtoRectF(ReadINIValue("Recipe Basic", "Mark1", RectFtoStringSimple(Mark1), INIFILE));
            MarkLine = StringtoRectF(ReadINIValue("Recipe Basic", "MarkLine", RectFtoStringSimple(MarkLine), INIFILE));
            MarkLine1 = StringtoRectF(ReadINIValue("Recipe Basic", "MarkLine1", RectFtoStringSimple(MarkLine1), INIFILE));
            MarkLine2 = StringtoRectF(ReadINIValue("Recipe Basic", "MarkLine2", RectFtoStringSimple(MarkLine2), INIFILE));

            SideLeft = StringtoRectF(ReadINIValue("Recipe Basic", "SideLeft", RectFtoStringSimple(SideLeft), INIFILE));
            SideRight = StringtoRectF(ReadINIValue("Recipe Basic", "SideRight", RectFtoStringSimple(SideRight), INIFILE));
            SideTop = StringtoRectF(ReadINIValue("Recipe Basic", "SideTop", RectFtoStringSimple(SideTop), INIFILE));
            SideBottom = StringtoRectF(ReadINIValue("Recipe Basic", "SideBottom", RectFtoStringSimple(SideBottom), INIFILE));

            DirRectF = StringtoRectF(ReadINIValue("Recipe Basic", "DirRectF", RectFtoStringSimple(DirRectF), INIFILE));
            AnaDirRectF = StringtoRectF(ReadINIValue("Recipe Basic", "AnaDirRectF", RectFtoStringSimple(AnaDirRectF), INIFILE));
            AnaReadBarcodeRectF = StringtoRectF(ReadINIValue("Recipe Basic", "AnaReadBarcodeRectF", RectFtoStringSimple(AnaReadBarcodeRectF), INIFILE));

            ptMark0 = StringtoPointF(ReadINIValue("Recipe Basic", "ptfMark0", PointFtoStringSimple(ptMark0), INIFILE));
            ptMark1 = StringtoPointF(ReadINIValue("Recipe Basic", "ptfMark1", PointFtoStringSimple(ptMark1), INIFILE));

            MappingInspectStr = ReadINIValue("Recipe Basic", "MappingInspectStr", "1", INIFILE);

            TrayRowCount = int.Parse(ReadINIValue("Track Control", "TrayRowCount", TrayRowCount.ToString(), INIFILE));
            TrayColCount = int.Parse(ReadINIValue("Track Control", "TrayColCount", TrayColCount.ToString(), INIFILE));

            chip_linescanoffset = double.Parse(ReadINIValue("Vision Control", "chip_linescanoffset", chip_linescanoffset.ToString(), INIFILE));
            chip_captureCount = int.Parse(ReadINIValue("Vision Control", "chip_captureCount", chip_captureCount.ToString(), INIFILE));

            clone1 = int.Parse(ReadINIValue("Vision Control", "clone1", clone1.ToString(), INIFILE));
            clone2 = int.Parse(ReadINIValue("Vision Control", "clone2", clone2.ToString(), INIFILE));
            clone_width = int.Parse(ReadINIValue("Vision Control", "clone_width", clone_width.ToString(), INIFILE));
            clone_count = int.Parse(ReadINIValue("Vision Control", "clone_count", clone_count.ToString(), INIFILE));
            Ischip_open_measure = ReadINIValue("Vision Control", "Ischip_open_measure", (Ischip_open_measure ? "1" : "0"), INIFILE) == "1";
            IsDrawRect = ReadINIValue("Vision Control", "IsDrawRect", (IsDrawRect ? "1" : "0"), INIFILE) == "1";

            dir_left = ReadINIValue("Vision Control", "dir_left", (dir_left ? "1" : "0"), INIFILE) == "1";
            dir_left_black = ReadINIValue("Vision Control", "dir_left_black", (dir_left_black ? "1" : "0"), INIFILE) == "1";
            dir_right = ReadINIValue("Vision Control", "dir_right", (dir_right ? "1" : "0"), INIFILE) == "1";
            dir_right_black = ReadINIValue("Vision Control", "dir_right_black", (dir_right_black ? "1" : "0"), INIFILE) == "1";
            dir_top = ReadINIValue("Vision Control", "dir_top", (dir_top ? "1" : "0"), INIFILE) == "1";
            dir_top_black = ReadINIValue("Vision Control", "dir_top_black", (dir_top_black ? "1" : "0"), INIFILE) == "1";
            dir_bottom = ReadINIValue("Vision Control", "dir_bottom", (dir_bottom ? "1" : "0"), INIFILE) == "1";
            dir_bottom_black = ReadINIValue("Vision Control", "dir_bottom_black", (dir_bottom_black ? "1" : "0"), INIFILE) == "1";


            thresholdv_mark1 = int.Parse(ReadINIValue("Vision Control", "thresholdv_mark1", thresholdv_mark1.ToString(), INIFILE));
            thresholdv_mark2 = int.Parse(ReadINIValue("Vision Control", "thresholdv_mark2", thresholdv_mark2.ToString(), INIFILE));
            thresholdv_mark3 = int.Parse(ReadINIValue("Vision Control", "thresholdv_mark3", thresholdv_mark3.ToString(), INIFILE));
            thresholdv_mark4 = int.Parse(ReadINIValue("Vision Control", "thresholdv_mark4", thresholdv_mark4.ToString(), INIFILE));

            findwhite_mark1 = ReadINIValue("Vision Control", "findwhite_mark1", (findwhite_mark1 ? "1" : "0"), INIFILE) == "1";
            findwhite_mark2 = ReadINIValue("Vision Control", "findwhite_mark2", (findwhite_mark2 ? "1" : "0"), INIFILE) == "1";
            findwhite_mark3 = ReadINIValue("Vision Control", "findwhite_mark3", (findwhite_mark3 ? "1" : "0"), INIFILE) == "1";
            findwhite_mark4 = ReadINIValue("Vision Control", "findwhite_mark4", (findwhite_mark4 ? "1" : "0"), INIFILE) == "1";

            fourmark1 = StringtoRectF(ReadINIValue("Recipe Basic", "fourmark1", RectFtoStringSimple(fourmark1), INIFILE));
            fourmark2 = StringtoRectF(ReadINIValue("Recipe Basic", "fourmark2", RectFtoStringSimple(fourmark2), INIFILE));
            fourmark3 = StringtoRectF(ReadINIValue("Recipe Basic", "fourmark3", RectFtoStringSimple(fourmark3), INIFILE));
            fourmark4 = StringtoRectF(ReadINIValue("Recipe Basic", "fourmark4", RectFtoStringSimple(fourmark4), INIFILE));


            CropWidth = int.Parse(ReadINIValue("Vision Control", "CropWidth", CropWidth.ToString(), INIFILE));
            SampleValue = int.Parse(ReadINIValue("Vision Control", "SampleValue", SampleValue.ToString(), INIFILE));
            PreMode = (ProcessImageMode)int.Parse(ReadINIValue("Vision Control", "PreMode", ((int)PreMode).ToString(), INIFILE));
            PreLaserPos = (LaserStartPos)int.Parse(ReadINIValue("Vision Control", "PreLaserPos", ((int)PreLaserPos).ToString(), INIFILE));
            PreThresholdValue = int.Parse(ReadINIValue("Vision Control", "PreThresholdValue", PreThresholdValue.ToString(), INIFILE));
            PreThresholdValueMark2 = int.Parse(ReadINIValue("Vision Control", "PreThresholdValueMark2", PreThresholdValueMark2.ToString(), INIFILE));
            fourStartPosition = (LaserOffsetStart)int.Parse(ReadINIValue("Vision Control", "fourStartPosition", ((int)fourStartPosition).ToString(), INIFILE));


            PreDir = ReadINIValue("Vision Control", "PreDir", (PreDir ? "1" : "0"), INIFILE) == "1";
            PreCenterExtendx = int.Parse(ReadINIValue("Vision Control", "PreCenterExtendx", PreCenterExtendx.ToString(), INIFILE));
            PreCenterExtendy = int.Parse(ReadINIValue("Vision Control", "PreCenterExtendy", PreCenterExtendy.ToString(), INIFILE));
            PreLineLeastDistance = float.Parse(ReadINIValue("Vision Control", "PreLineLeastDistance", PreLineLeastDistance.ToString(), INIFILE));
            IsOpenAuFind = ReadINIValue("Vision Control", "IsOpenAuFind", (IsOpenAuFind ? "1" : "0"), INIFILE) == "1";

            inspect_tolerance = float.Parse(ReadINIValue("Vision Control", "inspect_tolerance", inspect_tolerance.ToString(), INIFILE));
            inspect_angle = float.Parse(ReadINIValue("Vision Control", "inspect_angle", inspect_angle.ToString(), INIFILE));
            inspect_samplesize = int.Parse(ReadINIValue("Vision Control", "inspect_samplesize", inspect_samplesize.ToString(), INIFILE));

            LightValue = int.Parse(ReadINIValue("Vision Control", "LightValue", LightValue.ToString(), INIFILE));
            MarkMinArea = int.Parse(ReadINIValue("Vision Control", "MarkMinArea", MarkMinArea.ToString(), INIFILE));
            MarkLineAngle = double.Parse(ReadINIValue("Vision Control", "MarkLineAngle", MarkLineAngle.ToString(), INIFILE));
            PreMarkLineThresholdValue = int.Parse(ReadINIValue("Vision Control", "PreMarkLineThresholdValue", PreMarkLineThresholdValue.ToString(), INIFILE));
            IsOpenReadBarcode = ReadINIValue("Vision Control", "IsOpenReadBarcode", (IsOpenReadBarcode ? "1" : "0"), INIFILE) == "1";

            LaserLocX = float.Parse(ReadINIValue("Vision Control", "LaserLocX", LaserLocX.ToString(), INIFILE));
            LaserLocY = float.Parse(ReadINIValue("Vision Control", "LaserLocY", LaserLocY.ToString(), INIFILE));
            LaserLocA = float.Parse(ReadINIValue("Vision Control", "LaserLocA", LaserLocA.ToString(), INIFILE));

            k0x = float.Parse(ReadINIValue("Vision Control", "k0x", k0x.ToString(), INIFILE));
            k0y = float.Parse(ReadINIValue("Vision Control", "k0y", k0y.ToString(), INIFILE));
            k1x = float.Parse(ReadINIValue("Vision Control", "k1x", k1x.ToString(), INIFILE));
            k1y = float.Parse(ReadINIValue("Vision Control", "k1y", k1y.ToString(), INIFILE));

            inspect_dir_open = ReadINIValue("Vision Control", "inspect_dir_open", (inspect_dir_open ? "1" : "0"), INIFILE) == "1";
            inspect_dir_tolerance = float.Parse(ReadINIValue("Vision Control", "inspect_dir_tolerance", inspect_dir_tolerance.ToString(), INIFILE));
            inspect_dir_angle = float.Parse(ReadINIValue("Vision Control", "inspect_dir_angle", inspect_dir_angle.ToString(), INIFILE));
            inspect_dir_samplesize = int.Parse(ReadINIValue("Vision Control", "inspect_dir_samplesize", inspect_dir_samplesize.ToString(), INIFILE));
            inspect_dir_extendx = int.Parse(ReadINIValue("Vision Control", "inspect_dir_extendx", inspect_dir_extendx.ToString(), INIFILE));
            inspect_dir_extendy = int.Parse(ReadINIValue("Vision Control", "inspect_dir_extendy", inspect_dir_extendy.ToString(), INIFILE));


            stand_width = float.Parse(ReadINIValue("Vision Control", "stand_width", stand_width.ToString(), INIFILE));
            stand_width_upper = float.Parse(ReadINIValue("Vision Control", "stand_width_upper", stand_width_upper.ToString(), INIFILE));
            stand_width_lower = float.Parse(ReadINIValue("Vision Control", "stand_width_lower", stand_width_lower.ToString(), INIFILE));

            stand_height = float.Parse(ReadINIValue("Vision Control", "stand_height", stand_height.ToString(), INIFILE));
            stand_height_upper = float.Parse(ReadINIValue("Vision Control", "stand_height_upper", stand_height_upper.ToString(), INIFILE));
            stand_height_lower = float.Parse(ReadINIValue("Vision Control", "stand_height_lower", stand_height_lower.ToString(), INIFILE));

            stand_angle = float.Parse(ReadINIValue("Vision Control", "stand_angle", stand_angle.ToString(), INIFILE));
            stand_angle_upper = float.Parse(ReadINIValue("Vision Control", "stand_angle_upper", stand_angle_upper.ToString(), INIFILE));
            stand_angle_lower = float.Parse(ReadINIValue("Vision Control", "stand_angle_lower", stand_angle_lower.ToString(), INIFILE));

            if (!eCancel)
            {
                string bmpfilename = Path + "\\" + IndexStr + "\\" + "000.bmp";
                if (System.IO.File.Exists(bmpfilename))
                {
                    FreeImageBitmap freeImageBitmap = new FreeImageBitmap(bmpfilename);
                    bmpORG.Dispose();
                    bmpORG = (Bitmap)freeImageBitmap.ToBitmap().Clone(
                                                               new Rectangle(0, 0, freeImageBitmap.Width, freeImageBitmap.Height),
                                                               freeImageBitmap.PixelFormat);
                    freeImageBitmap.Dispose();
                }

                string bmppatternpath = Path + "\\" + IndexStr + "\\" + "pattern.bmp";
                if (System.IO.File.Exists(bmppatternpath))
                {
                    FreeImageBitmap freeImageBitmap = new FreeImageBitmap(bmppatternpath);
                    bmpORGPattern.Dispose();
                    bmpORGPattern = (Bitmap)freeImageBitmap.ToBitmap().Clone(
                                                               new Rectangle(0, 0, freeImageBitmap.Width, freeImageBitmap.Height),
                                                               freeImageBitmap.PixelFormat);
                    freeImageBitmap.Dispose();
                }

                string bmpdirpatternpath = Path + "\\" + IndexStr + "\\" + "dirpattern.bmp";
                if (System.IO.File.Exists(bmpdirpatternpath))
                {
                    FreeImageBitmap freeImageBitmap = new FreeImageBitmap(bmpdirpatternpath);
                    bmpOrgDirPattern.Dispose();
                    bmpOrgDirPattern = (Bitmap)freeImageBitmap.ToBitmap().Clone(
                                                               new Rectangle(0, 0, freeImageBitmap.Width, freeImageBitmap.Height),
                                                               freeImageBitmap.PixelFormat);
                    freeImageBitmap.Dispose();
                }

                string bmpanadirpatternpath = Path + "\\" + IndexStr + "\\" + "anadirpattern.bmp";
                if (System.IO.File.Exists(bmpanadirpatternpath))
                {
                    FreeImageBitmap freeImageBitmap = new FreeImageBitmap(bmpanadirpatternpath);
                    bmpOrgAnaDirPattern.Dispose();
                    bmpOrgAnaDirPattern = (Bitmap)freeImageBitmap.ToBitmap().Clone(
                                                               new Rectangle(0, 0, freeImageBitmap.Width, freeImageBitmap.Height),
                                                               freeImageBitmap.PixelFormat);
                    freeImageBitmap.Dispose();
                }

                QcCell();
                QcCellManual();
                //string rectfilename = Path + "\\" + IndexStr + "\\" + "000.pos";
                //rect_list.Clear();
                //if (System.IO.File.Exists(rectfilename))
                //{
                //    System.IO.StreamReader streamReader = new System.IO.StreamReader(rectfilename);
                //    while (!streamReader.EndOfStream)
                //    {
                //        string line = streamReader.ReadLine();
                //        if (!string.IsNullOrEmpty(line))
                //        {
                //            RectangleF rectangleF = StringtoRectF(line);
                //            rect_list.Add(rectangleF);
                //        }
                //    }
                //    streamReader.Close();
                //    streamReader.Dispose();
                //}

                //CoarsePositioningClass.Instance.Initial(Path, Index, "CoarsePosition.ini");
                //ModelPositioningClass.Instance.Initial(Path, Index, "ModelPosition.ini");

                ViewCreateAnalyzeClass();
                ViewTrain();
            }


            LoadIniSetup();
        }
        public override void Save()
        {
            SaveIniSetup();

            WriteINIValue("Basic Control", "BaseHeightZ", BaseHeightZ.ToString(Format), INIFILE);
            WriteINIValue("Basic Control", "FocusUpperAndLower", FocusUpperAndLower.ToString(Format), INIFILE);
            sExposureList = arrayToString(ExposureArray);
            WriteINIValue("Cam Control", "sExposureList", sExposureList.ToString(), INIFILE);
            sGainList = arrayToString(GainArray);
            WriteINIValue("Cam Control", "sGainList", sGainList.ToString(), INIFILE);

            //sMarkCollectionStr = arrayToString(MarkCollectionStr);
            //WriteINIValue("Mark Control", "sMarkCollectionStr", sMarkCollectionStr.ToString(), INIFILE);

            WriteINIValue("Recipe Basic", "rect_start", RectFtoStringSimple(rect_start), INIFILE);
            WriteINIValue("Recipe Basic", "rect_end", RectFtoStringSimple(rect_end), INIFILE);

            WriteINIValue("Recipe Basic", "MappingInspectStr", MappingInspectStr, INIFILE);

            //WriteINIValue("Recipe Basic", "Mark0", RectFtoStringSimple(Mark0), INIFILE);
            //WriteINIValue("Recipe Basic", "Mark1", RectFtoStringSimple(Mark1), INIFILE);

            WriteINIValue("Track Control", "TrayRowCount", TrayRowCount.ToString(), INIFILE);
            WriteINIValue("Track Control", "TrayColCount", TrayColCount.ToString(), INIFILE);

            WriteINIValue("Vision Control", "chip_linescanoffset", chip_linescanoffset.ToString(Format), INIFILE);
            WriteINIValue("Vision Control", "chip_captureCount", chip_captureCount.ToString(), INIFILE);

            WriteINIValue("Vision Control", "clone1", clone1.ToString(), INIFILE);
            WriteINIValue("Vision Control", "clone2", clone2.ToString(), INIFILE);
            WriteINIValue("Vision Control", "clone_width", clone_width.ToString(), INIFILE);
            WriteINIValue("Vision Control", "clone_count", clone_count.ToString(), INIFILE);
            WriteINIValue("Vision Control", "Ischip_open_measure", (Ischip_open_measure ? "1" : "0"), INIFILE);
            WriteINIValue("Vision Control", "IsDrawRect", (IsDrawRect ? "1" : "0"), INIFILE);

            WriteINIValue("Vision Control", "dir_left", (dir_left ? "1" : "0"), INIFILE);
            WriteINIValue("Vision Control", "dir_left_black", (dir_left_black ? "1" : "0"), INIFILE);
            WriteINIValue("Vision Control", "dir_right", (dir_right ? "1" : "0"), INIFILE);
            WriteINIValue("Vision Control", "dir_right_black", (dir_right_black ? "1" : "0"), INIFILE);
            WriteINIValue("Vision Control", "dir_top", (dir_top ? "1" : "0"), INIFILE);
            WriteINIValue("Vision Control", "dir_top_black", (dir_top_black ? "1" : "0"), INIFILE);
            WriteINIValue("Vision Control", "dir_bottom", (dir_bottom ? "1" : "0"), INIFILE);
            WriteINIValue("Vision Control", "dir_bottom_black", (dir_bottom_black ? "1" : "0"), INIFILE);

            WriteINIValue("Vision Control", "CropWidth", CropWidth.ToString(), INIFILE);
            WriteINIValue("Vision Control", "SampleValue", SampleValue.ToString(), INIFILE);
            WriteINIValue("Vision Control", "PreMode", ((int)PreMode).ToString(), INIFILE);
            WriteINIValue("Vision Control", "PreLaserPos", ((int)PreLaserPos).ToString(), INIFILE);
            WriteINIValue("Vision Control", "PreThresholdValue", PreThresholdValue.ToString(), INIFILE);
            WriteINIValue("Vision Control", "PreThresholdValueMark2", PreThresholdValueMark2.ToString(), INIFILE);
            WriteINIValue("Vision Control", "PreDir", (PreDir ? "1" : "0"), INIFILE);
            WriteINIValue("Vision Control", "PreCenterExtendx", PreCenterExtendx.ToString(), INIFILE);
            WriteINIValue("Vision Control", "PreCenterExtendy", PreCenterExtendy.ToString(), INIFILE);
            WriteINIValue("Vision Control", "PreLineLeastDistance", PreLineLeastDistance.ToString(), INIFILE);
            WriteINIValue("Vision Control", "IsOpenAuFind", (IsOpenAuFind ? "1" : "0"), INIFILE);

            WriteINIValue("Vision Control", "inspect_tolerance", inspect_tolerance.ToString(), INIFILE);
            WriteINIValue("Vision Control", "inspect_angle", inspect_angle.ToString(), INIFILE);
            WriteINIValue("Vision Control", "inspect_samplesize", inspect_samplesize.ToString(), INIFILE);

            WriteINIValue("Vision Control", "LightValue", LightValue.ToString(), INIFILE);
            WriteINIValue("Vision Control", "MarkMinArea", MarkMinArea.ToString(), INIFILE);
            WriteINIValue("Vision Control", "PreMarkLineThresholdValue", PreMarkLineThresholdValue.ToString(), INIFILE);
            WriteINIValue("Vision Control", "fourStartPosition", ((int)fourStartPosition).ToString(), INIFILE);
            WriteINIValue("Vision Control", "IsOpenReadBarcode", (IsOpenReadBarcode ? "1" : "0"), INIFILE);

            WriteINIValue("Vision Control", "LaserLocX", LaserLocX.ToString(), INIFILE);
            WriteINIValue("Vision Control", "LaserLocY", LaserLocY.ToString(), INIFILE);
            WriteINIValue("Vision Control", "LaserLocA", LaserLocA.ToString(), INIFILE);

            WriteINIValue("Vision Control", "k0x", k0x.ToString(), INIFILE);
            WriteINIValue("Vision Control", "k0y", k0y.ToString(), INIFILE);
            WriteINIValue("Vision Control", "k1x", k1x.ToString(), INIFILE);
            WriteINIValue("Vision Control", "k1y", k1y.ToString(), INIFILE);

            WriteINIValue("Vision Control", "inspect_dir_open", (inspect_dir_open ? "1" : "0"), INIFILE);
            WriteINIValue("Vision Control", "inspect_dir_tolerance", inspect_dir_tolerance.ToString(), INIFILE);
            WriteINIValue("Vision Control", "inspect_dir_angle", inspect_dir_angle.ToString(), INIFILE);
            WriteINIValue("Vision Control", "inspect_dir_samplesize", inspect_dir_samplesize.ToString(), INIFILE);
            WriteINIValue("Vision Control", "inspect_dir_extendx", inspect_dir_extendx.ToString(), INIFILE);
            WriteINIValue("Vision Control", "inspect_dir_extendy", inspect_dir_extendy.ToString(), INIFILE);

            WriteINIValue("Vision Control", "stand_width", stand_width.ToString(), INIFILE);
            WriteINIValue("Vision Control", "stand_width_upper", stand_width_upper.ToString(), INIFILE);
            WriteINIValue("Vision Control", "stand_width_lower", stand_width_lower.ToString(), INIFILE);

            WriteINIValue("Vision Control", "stand_height", stand_height.ToString(), INIFILE);
            WriteINIValue("Vision Control", "stand_height_upper", stand_height_upper.ToString(), INIFILE);
            WriteINIValue("Vision Control", "stand_height_lower", stand_height_lower.ToString(), INIFILE);

            WriteINIValue("Vision Control", "stand_angle", stand_angle.ToString(), INIFILE);
            WriteINIValue("Vision Control", "stand_angle_upper", stand_angle_upper.ToString(), INIFILE);
            WriteINIValue("Vision Control", "stand_angle_lower", stand_angle_lower.ToString(), INIFILE);

            string bmpfilename = Path + "\\" + IndexStr + "\\" + "000.bmp";
            Bitmap bmptemp = (Bitmap)bmpORG.Clone(new Rectangle(0, 0, bmpORG.Width, bmpORG.Height), System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            bmptemp.Save(bmpfilename, System.Drawing.Imaging.ImageFormat.Bmp);

            SavePattern();
            SaveDirPattern();
            SaveMark();
            //CoarsePositioningClass.Instance.Save();
            //ModelPositioningClass.Instance.Save();

            //if (rect_list.Count > 0)
            //{
            //    string rectfilename = Path + "\\" + IndexStr + "\\" + "000.pos";
            //    string res = string.Empty;
            //    foreach (RectangleF rect in rect_list)
            //    {
            //        res += RectFtoStringSimple(rect) + Environment.NewLine;
            //    }
            //    System.IO.StreamWriter streamWriter = new System.IO.StreamWriter(rectfilename);
            //    streamWriter.Write(res);
            //    streamWriter.Close();
            //    streamWriter.Dispose();
            //}
            QcCell();
            QcCellManual();

            ViewCreateAnalyzeClass();

            SaveFourMark();
            ViewTrain();
        }
        public void SavePattern()
        {
            string bmppatternpath = Path + "\\" + IndexStr + "\\" + "pattern.bmp";
            bmpORGPattern.Save(bmppatternpath, System.Drawing.Imaging.ImageFormat.Bmp);

        }
        public void SaveDirPattern()
        {
            string bmpdirpatternpath = Path + "\\" + IndexStr + "\\" + "dirpattern.bmp";
            bmpOrgDirPattern.Save(bmpdirpatternpath, System.Drawing.Imaging.ImageFormat.Bmp);

        }
        public void SaveAnaDirPattern()
        {
            string bmpanadirpatternpath = Path + "\\" + IndexStr + "\\" + "anadirpattern.bmp";
            bmpOrgAnaDirPattern.Save(bmpanadirpatternpath, System.Drawing.Imaging.ImageFormat.Bmp);

        }
        public void SaveMark()
        {
            SaveIniSetup();

            WriteINIValue("Recipe Basic", "Mark0", RectFtoStringSimple(Mark0), INIFILE);
            WriteINIValue("Recipe Basic", "Mark1", RectFtoStringSimple(Mark1), INIFILE);
            WriteINIValue("Recipe Basic", "MarkLine", RectFtoStringSimple(MarkLine), INIFILE);

            WriteINIValue("Recipe Basic", "MarkLine1", RectFtoStringSimple(MarkLine1), INIFILE);
            WriteINIValue("Recipe Basic", "MarkLine2", RectFtoStringSimple(MarkLine2), INIFILE);

            WriteINIValue("Recipe Basic", "SideLeft", RectFtoStringSimple(SideLeft), INIFILE);
            WriteINIValue("Recipe Basic", "SideRight", RectFtoStringSimple(SideRight), INIFILE);
            WriteINIValue("Recipe Basic", "SideTop", RectFtoStringSimple(SideTop), INIFILE);
            WriteINIValue("Recipe Basic", "SideBottom", RectFtoStringSimple(SideBottom), INIFILE);
            WriteINIValue("Recipe Basic", "DirRectF", RectFtoStringSimple(DirRectF), INIFILE);
            WriteINIValue("Recipe Basic", "AnaDirRectF", RectFtoStringSimple(AnaDirRectF), INIFILE);
            WriteINIValue("Recipe Basic", "AnaReadBarcodeRectF", RectFtoStringSimple(AnaReadBarcodeRectF), INIFILE);

            WriteINIValue("Recipe Basic", "ptfMark0", PointFtoStringSimple(ptMark0), INIFILE);
            WriteINIValue("Recipe Basic", "ptfMark1", PointFtoStringSimple(ptMark1), INIFILE);
            WriteINIValue("Vision Control", "MarkLineAngle", MarkLineAngle.ToString(), INIFILE);

            WriteINIValue("Recipe Basic", "fourmark1", RectFtoStringSimple(fourmark1), INIFILE);
            WriteINIValue("Recipe Basic", "fourmark2", RectFtoStringSimple(fourmark2), INIFILE);
            WriteINIValue("Recipe Basic", "fourmark3", RectFtoStringSimple(fourmark3), INIFILE);
            WriteINIValue("Recipe Basic", "fourmark4", RectFtoStringSimple(fourmark4), INIFILE);

            WriteINIValue("Vision Control", "thresholdv_mark1", thresholdv_mark1.ToString(), INIFILE);
            WriteINIValue("Vision Control", "thresholdv_mark2", thresholdv_mark2.ToString(), INIFILE);
            WriteINIValue("Vision Control", "thresholdv_mark3", thresholdv_mark3.ToString(), INIFILE);
            WriteINIValue("Vision Control", "thresholdv_mark4", thresholdv_mark4.ToString(), INIFILE);

            WriteINIValue("Vision Control", "findwhite_mark1", (findwhite_mark1 ? "1" : "0"), INIFILE);
            WriteINIValue("Vision Control", "findwhite_mark2", (findwhite_mark2 ? "1" : "0"), INIFILE);
            WriteINIValue("Vision Control", "findwhite_mark3", (findwhite_mark3 ? "1" : "0"), INIFILE);
            WriteINIValue("Vision Control", "findwhite_mark4", (findwhite_mark4 ? "1" : "0"), INIFILE);

        }
        public void QcCell()
        {
            int iCount = TrayRowCount * TrayColCount;
            QcPass = new bool[iCount];
            int i = 0;
            while (i < iCount)
            {
                QcPass[i] = false;
                i++;
            }
        }
        public void QcCellManual()
        {
            int iCount = TrayRowCount * TrayColCount;
            QcPassManual = new bool[iCount];
            int i = 0;
            while (i < iCount)
            {
                QcPassManual[i] = false;
                i++;
            }

            SetQcPassManualData();
        }
        public void SaveFourMark()
        {
            SaveIniSetup();

            //sMarkCollectionStr = arrayToString(MarkCollectionStr);
            //WriteINIValue("Mark Control", "sMarkCollectionStr", sMarkCollectionStr.ToString(), INIFILE);

            int i = 0;
            foreach (var item in MarkCollectionStr)
            {
                WriteINIValue("Mark Control", $"MarkCollectionStr{i}", item.ToParaString(), INIFILE);
                WriteINIValue("Mark Control", $"MarkCollectionStr1X{i}", item.ToParaString1(), INIFILE);
                i++;
            }

        }
        public void ViewCreateAnalyzeClass()
        {
            //模拟测试的数据
            //AnalyzeClass assignClass = new AnalyzeClass();
            //ViewAnalyzeClass.Index = m_CollectDataIndex;
            //ViewAnalyzeClass.SaveFileName = JzTimes.DateTimeSerialStringFFF;
            //ViewAnalyzeClass.myPath = Traveller106.Universal.COLLECT + "\\tmp_" + m_CollectDataIndex.ToString("0000") + ".cfg";
            ViewAnalyzeClass.RectFStart = rect_start;
            ViewAnalyzeClass.RectFEnd = rect_end;
            ViewAnalyzeClass.CreateRowCol(TrayRowCount, TrayColCount, true);
            //ViewAnalyzeClass.ChipClassDataFilePath = INI.Instance.HistoryDataPath + "\\" + INI.Instance.HistoryDataBarcode + "\\Channel0";
            //ViewAnalyzeClass.ChipClassBarcode = (string.IsNullOrEmpty(BarcodeStr) ? JzTimes.DateTimeSerialStringFFF : BarcodeClass.Barcode);
            //BarcodeStr = string.Empty;
            ViewAnalyzeClass.IsDrawRect = IsDrawRect;
            //ViewAnalyzeClass.freeImageBitmapInput = new FreeImageBitmap(m_bmpCacheOrg);
        }
        public float GetCamExpo(int camid)
        {
            if (camid >= ExposureArray.Length)
                return 1000;

            float camExpo = 1000;
            float.TryParse(ExposureArray[camid].ToString(), out camExpo);

            return camExpo;
        }
        public void SetCamExpo(int camid, float expo)
        {
            if (camid >= ExposureArray.Length)
                return;
            ExposureArray[camid] = expo.ToString();
        }
        public float GetCamGain(int camid)
        {
            if (camid >= GainArray.Length)
                return 0;

            float camGain = 0;
            float.TryParse(GainArray[camid].ToString(), out camGain);

            return camGain;
        }
        public void SetCamGain(int camid, float gain)
        {
            if (camid >= GainArray.Length)
                return;
            GainArray[camid] = gain.ToString();
        }
        private string arrayToString(string[] strArray)
        {
            StringBuilder str = new StringBuilder();
            for (int i = 0; i < strArray.Length; i++)
            {
                if (i > 0)
                    str.Append(";");
                str.Append(strArray[i]);
            }
            return str.ToString();
        }
        //private string arrayToString(MarkItemClass[] strArray)
        //{
        //    StringBuilder str = new StringBuilder();
        //    for (int i = 0; i < strArray.Length; i++)
        //    {
        //        if (i > 0)
        //            str.Append(";");
        //        str.Append(strArray[i].ToParaString());
        //    }
        //    return str.ToString();
        //}
        //RectangleF StringToRectF(string Str)
        //{
        //    string[] strs = Str.Split(',');
        //    RectangleF rectF = new RectangleF();

        //    rectF.X = float.Parse(strs[0]);
        //    rectF.Y = float.Parse(strs[1]);
        //    rectF.Width = float.Parse(strs[2]);
        //    rectF.Height = float.Parse(strs[3]);

        //    return rectF;


        //}

        public void LoadFourSetup()
        {
            int i = 0;
            i = 0;
            while (i < MarkCollectionStr.Length)
            {
                MarkCollectionStr[i] = new MarkItemClass();

                sMarkCollectionStr = ReadINIValue("Mark Control", $"MarkCollectionStr{i}", sMarkCollectionStr.ToString(), INIFILE);
                sMarkCollectionStr1X = ReadINIValue("Mark Control", $"MarkCollectionStr1X{i}", sMarkCollectionStr1X.ToString(), INIFILE);

                MarkCollectionStr[i].FormString(sMarkCollectionStr);
                MarkCollectionStr[i].FormString1(sMarkCollectionStr1X);
                MarkCollectionStr[i].Index = i;
                i++;
            }

            //sMarkCollectionStr = ReadINIValue("Mark Control", "sMarkCollectionStr", sMarkCollectionStr.ToString(), INIFILE);
            //string[] strs = sMarkCollectionStr.Split(';').ToArray();
            //if (strs.Length < 4)
            //{
            //    MarkCollectionStr = new MarkItemClass[4];
            //    i = 0;
            //    while (i < 4)
            //    {
            //        MarkCollectionStr[i] = new MarkItemClass();
            //        MarkCollectionStr[i].Index = i;
            //        i++;
            //    }
            //}
            //else
            //{
            //    i = 0;
            //    while (i < 4)
            //    {
            //        MarkCollectionStr[i] = new MarkItemClass();
            //        MarkCollectionStr[i].FormString(strs[i]);
            //        MarkCollectionStr[i].Index = i;
            //        i++;
            //    }
            //}
        }
        private XProps m_xprops = new XProps();
        public XProps XPropsRcp
        {
            get { return m_xprops; }
        }

        public void LoadIniSetup()
        {
            m_xprops.Clear();
            string cat0 = "rcpcat0";
            AddProperty(cat0, "TrayRowCount", "TrayRowCount", TrayRowCount, "");
            AddProperty(cat0, "TrayColCount", "TrayColCount", TrayColCount, "");
            AddProperty(cat0, "RcpIsOpenAuFind", "IsOpenAuFind", IsOpenAuFind, "");

            AddProperty(cat0, "inspect_tolerance", "inspect_tolerance", inspect_tolerance, "");
            AddProperty(cat0, "inspect_angle", "inspect_angle", inspect_angle, "");
            AddProperty(cat0, "inspect_samplesize", "inspect_samplesize", inspect_samplesize, "");

            AddProperty(cat0, "IsDrawRect", "IsDrawRect", IsDrawRect, "");
            AddProperty(cat0, "Ischip_open_measure", "Ischip_open_measure", Ischip_open_measure, "");
            AddProperty(cat0, "IsOpenReadBarcode", "IsOpenReadBarcode", IsOpenReadBarcode, "");

            string cat1 = "rcpcat1";
            AddProperty(cat1, "CropWidth", "CropWidth", CropWidth, "");
            AddProperty(cat1, "SampleValue", "SampleValue", SampleValue, "");
            AddProperty(cat1, "PreMode", "PreMode", PreMode, "");
            AddProperty(cat1, "PreThresholdValue", "PreThresholdValue", PreThresholdValue, "");
            //AddProperty(cat1, "PreMarkLineThresholdValue", "PreMarkLineThresholdValue", PreMarkLineThresholdValue, "");
            AddProperty(cat1, "PreLaserPos", "PreLaserPos", PreLaserPos, "");
            AddProperty(cat1, "LightValue", "LightValue", LightValue, "");
            //AddProperty(cat1, "MarkLineAngle", "MarkLineAngle", MarkLineAngle, "");

            AddProperty(cat1, "PreCenterExtendx", "PreCenterExtendx", PreCenterExtendx, "");
            AddProperty(cat1, "PreCenterExtendy", "PreCenterExtendy", PreCenterExtendy, "");

            switch (Traveller106.Universal.FACTORYNAME)
            {
                case FactoryName.DONGGUAN:
                case FactoryName.DAGUI:
                    break;
                default:

                    AddProperty(cat1, "PreThresholdValueMark2", "PreThresholdValueMark2", PreThresholdValueMark2, "");
                    AddProperty(cat1, "PreDir", "PreDir", PreDir, "");
                    //AddProperty(cat1, "PreCenterExtendx", "PreCenterExtendx", PreCenterExtendx, "");
                    //AddProperty(cat1, "PreCenterExtendy", "PreCenterExtendy", PreCenterExtendy, "");
                    AddProperty(cat1, "PreLineLeastDistance", "PreLineLeastDistance", PreLineLeastDistance, "");
                    AddProperty(cat1, "MarkMinArea", "MarkMinArea", MarkMinArea, "");

                    string cat3 = "rcpcat3";
                    AddProperty(cat3, "k0x", "k0x", k0x, "");
                    AddProperty(cat3, "k0y", "k0y", k0y, "");
                    AddProperty(cat3, "k1x", "k1x", k1x, "");
                    AddProperty(cat3, "k1y", "k1y", k1y, "");

                    break;
            }

            string cat2 = "rcpcat2";
            AddProperty(cat2, "LaserLocX", "LaserLocX", LaserLocX, "");
            AddProperty(cat2, "LaserLocY", "LaserLocY", LaserLocY, "");
            AddProperty(cat2, "LaserLocA", "LaserLocA", LaserLocA, "");

           

            string cat4 = "rcpcat4";
            AddProperty(cat4, "fourStartPosition", "fourStartPosition", fourStartPosition, "");
            AddProperty(cat4, "dir_left", "dir_left", dir_left, "");
            AddProperty(cat4, "dir_left_black", "dir_left_black", dir_left_black, "");
            AddProperty(cat4, "dir_right", "dir_right", dir_right, "");
            AddProperty(cat4, "dir_right_black", "dir_right_black", dir_right_black, "");
            AddProperty(cat4, "dir_top", "dir_top", dir_top, "");
            AddProperty(cat4, "dir_top_black", "dir_top_black", dir_top_black, "");
            AddProperty(cat4, "dir_bottom", "dir_bottom", dir_bottom, "");
            AddProperty(cat4, "dir_bottom_black", "dir_bottom_black", dir_bottom_black, "");

            string cat5 = "rcpcat5";
            AddProperty(cat5, "inspect_dir_open", "inspect_dir_open", inspect_dir_open, "");
            AddProperty(cat5, "inspect_dir_tolerance", "inspect_dir_tolerance", inspect_dir_tolerance, "");
            AddProperty(cat5, "inspect_dir_angle", "inspect_dir_angle", inspect_dir_angle, "");
            AddProperty(cat5, "inspect_dir_samplesize", "inspect_dir_samplesize", inspect_dir_samplesize, "");
            AddProperty(cat5, "inspect_dir_extendx", "inspect_dir_extendx", inspect_dir_extendx, "");
            AddProperty(cat5, "inspect_dir_extendy", "inspect_dir_extendy", inspect_dir_extendy, "");

            string cat6 = "rcpcat6";
            AddProperty(cat6, "stand_width", "stand_width", stand_width, "");
            AddProperty(cat6, "stand_width_upper", "stand_width_upper", stand_width_upper, "");
            AddProperty(cat6, "stand_width_lower", "stand_width_lower", stand_width_lower, "");
            AddProperty(cat6, "stand_height", "stand_height", stand_height, "");
            AddProperty(cat6, "stand_height_upper", "stand_height_upper", stand_height_upper, "");
            AddProperty(cat6, "stand_height_lower", "stand_height_lower", stand_height_lower, "");
            AddProperty(cat6, "stand_angle", "stand_angle", stand_angle, "");
            AddProperty(cat6, "stand_angle_upper", "stand_angle_upper", stand_angle_upper, "");
            AddProperty(cat6, "stand_angle_lower", "stand_angle_lower", stand_angle_lower, "");

        }
        public void SaveIniSetup()
        {
            foreach (XProp xpropItem in m_xprops)
            {
                switch (xpropItem.ReleateName)
                {
                    #region Cat0
                    case "TrayRowCount":
                        TrayRowCount = (int)xpropItem.Value;
                        break;
                    case "TrayColCount":
                        TrayColCount = (int)xpropItem.Value;
                        break;
                    case "IsOpenAuFind":
                        IsOpenAuFind = (bool)xpropItem.Value;
                        break;
                    case "inspect_tolerance":
                        inspect_tolerance = (float)xpropItem.Value;
                        break;
                    case "inspect_angle":
                        inspect_angle = (float)xpropItem.Value;
                        break;
                    case "inspect_samplesize":
                        inspect_samplesize = (int)xpropItem.Value;
                        break;
                    case "IsDrawRect":
                        IsDrawRect = (bool)xpropItem.Value;
                        break;
                    case "Ischip_open_measure":
                        Ischip_open_measure = (bool)xpropItem.Value;
                        break;
                    case "IsOpenReadBarcode":
                        IsOpenReadBarcode = (bool)xpropItem.Value;
                        break;
                    #endregion
                    #region Cat1
                    case "CropWidth":
                        CropWidth = (int)xpropItem.Value;
                        break;
                    case "SampleValue":
                        SampleValue = int.Parse(xpropItem.Value.ToString());
                        break;
                    case "PreMode":
                        PreMode = (ProcessImageMode)xpropItem.Value;
                        break;
                    case "PreThresholdValue":
                        PreThresholdValue = int.Parse(xpropItem.Value.ToString());
                        break;
                    case "PreThresholdValueMark2":
                        PreThresholdValueMark2 = int.Parse(xpropItem.Value.ToString());
                        break;
                    case "PreDir":
                        PreDir = (bool)xpropItem.Value;
                        break;
                    case "PreCenterExtendx":
                        PreCenterExtendx = int.Parse(xpropItem.Value.ToString());
                        break;
                    case "PreCenterExtendy":
                        PreCenterExtendy = int.Parse(xpropItem.Value.ToString());
                        break;
                    case "PreLineLeastDistance":
                        PreLineLeastDistance = float.Parse(xpropItem.Value.ToString());
                        break;
                    case "MarkMinArea":
                        MarkMinArea = int.Parse(xpropItem.Value.ToString());
                        break;
                    case "PreMarkLineThresholdValue":
                        PreMarkLineThresholdValue = int.Parse(xpropItem.Value.ToString());
                        break;
                    case "PreLaserPos":
                        PreLaserPos = (LaserStartPos)xpropItem.Value;
                        break;
                    case "LightValue":
                        LightValue = int.Parse(xpropItem.Value.ToString());
                        //if (LightValue < INI.Instance.LightMinValue || LightValue > 255)
                        //{
                        //    LightValue = 255;
                        //    //VsMSG.Instance.Warning($"{ToChangeLanguage("超出设定范围")}[120,255]");
                        //}
                        break;
                    case "MarkLineAngle":
                        MarkLineAngle = double.Parse(xpropItem.Value.ToString());
                        break;
                    #endregion
                    #region Cat2
                    case "LaserLocX":
                        LaserLocX = (float)xpropItem.Value;
                        break;
                    case "LaserLocY":
                        LaserLocY = (float)xpropItem.Value;
                        break;
                    case "LaserLocA":
                        LaserLocA = (float)xpropItem.Value;
                        break;
                    #endregion
                    #region Cat3
                    case "k0x":
                        k0x = (float)xpropItem.Value;
                        break;
                    case "k0y":
                        k0y = (float)xpropItem.Value;
                        break;
                    case "k1x":
                        k1x = (float)xpropItem.Value;
                        break;
                    case "k1y":
                        k1y = (float)xpropItem.Value;
                        break;
                    #endregion
                    #region Cat4
                    case "fourStartPosition":
                        fourStartPosition = (LaserOffsetStart)xpropItem.Value;
                        break;
                    case "dir_left":
                        dir_left = (bool)xpropItem.Value;
                        break;
                    case "dir_left_black":
                        dir_left_black = (bool)xpropItem.Value;
                        break;
                    case "dir_right":
                        dir_right = (bool)xpropItem.Value;
                        break;
                    case "dir_right_black":
                        dir_right_black = (bool)xpropItem.Value;
                        break;
                    case "dir_top":
                        dir_top = (bool)xpropItem.Value;
                        break;
                    case "dir_top_black":
                        dir_top_black = (bool)xpropItem.Value;
                        break;
                    case "dir_bottom":
                        dir_bottom = (bool)xpropItem.Value;
                        break;
                    case "dir_bottom_black":
                        dir_bottom_black = (bool)xpropItem.Value;
                        break;
                    #endregion
                    #region Cat5
                    case "inspect_dir_open":
                        inspect_dir_open = (bool)xpropItem.Value;
                        break;
                    case "inspect_dir_tolerance":
                        inspect_dir_tolerance = (float)xpropItem.Value;
                        break;
                    case "inspect_dir_angle":
                        inspect_dir_angle = (float)xpropItem.Value;
                        break;
                    case "inspect_dir_samplesize":
                        inspect_dir_samplesize = (int)xpropItem.Value;
                        break;
                    case "inspect_dir_extendx":
                        inspect_dir_extendx = (int)xpropItem.Value;
                        break;
                    case "inspect_dir_extendy":
                        inspect_dir_extendy = (int)xpropItem.Value;
                        break;
                    #endregion
                    #region Cat6
                    case "stand_width":
                        stand_width = (float)xpropItem.Value;
                        break;
                    case "stand_width_upper":
                        stand_width_upper = (float)xpropItem.Value;
                        break;
                    case "stand_width_lower":
                        stand_width_lower = (float)xpropItem.Value;
                        break;
                    case "stand_height":
                        stand_height = (float)xpropItem.Value;
                        break;
                    case "stand_height_upper":
                        stand_height_upper = (float)xpropItem.Value;
                        break;
                    case "stand_height_lower":
                        stand_height_lower = (float)xpropItem.Value;
                        break;
                    case "stand_angle":
                        stand_angle = (float)xpropItem.Value;
                        break;
                    case "stand_angle_upper":
                        stand_angle_upper = (float)xpropItem.Value;
                        break;
                    case "stand_angle_lower":
                        stand_angle_lower = (float)xpropItem.Value;
                        break;
                        #endregion
                }
                //if (xpropItem.ReleateName == "SHOW_CHANGE_TIME")
                //    SHOW_CHANGE_TIME = int.Parse(xpropItem.Value.ToString());

                //if (xpropItem.ReleateName == "CHANGE_LANGUAGE")
                //    CHANGE_LANGUAGE = int.Parse(xpropItem.Value.ToString());

                //if (xpropItem.ReleateName == "IsSaveImage")
                //    IsSaveImage = (bool)xpropItem.Value;
            }
        }

        void AddProperty(string eProperty, string eName, string eReleateName, object eValue, string eDescription)
        {
            eProperty = LanguageExClass.Instance.GetLanguageText(eProperty);
            eName = LanguageExClass.Instance.GetLanguageText(eName);
            eDescription = LanguageExClass.Instance.GetLanguageText(eDescription);

            XProp xProp1 = new XProp();
            xProp1.Category = eProperty;
            xProp1.ReleateName = eReleateName;
            xProp1.Name = eReleateName;
            xProp1.Value = eValue;
            xProp1.Description = eDescription;

            switch (eReleateName)
            {
                case "LightValue":
                    xProp1.Attr = new Attribute[] { new DisplayNameAttribute(eName), new MinMaxAttribute(INI.Instance.LightMinValue, 255) };
                    xProp1.Editor = new NumericUpDownTypeEditor();
                    break;
                case "inspect_dir_tolerance":
                case "inspect_tolerance":
                    xProp1.Attr = new Attribute[] { new DisplayNameAttribute(eName), new MinMaxAttribute(0f, 1f, 0.1f, 2) };
                    xProp1.Editor = new NumericUpDownTypeEditor();
                    break;
                case "inspect_dir_angle":
                case "inspect_angle":
                    xProp1.Attr = new Attribute[] { new DisplayNameAttribute(eName), new MinMaxAttribute(0f, 180f, 0.1f, 2) };
                    xProp1.Editor = new NumericUpDownTypeEditor();
                    break;
                case "inspect_dir_samplesize":
                case "inspect_samplesize":
                    xProp1.Attr = new Attribute[] { new DisplayNameAttribute(eName), new MinMaxAttribute(0, 300) };
                    xProp1.Editor = new NumericUpDownTypeEditor();
                    break;
                case "inspect_dir_extendx":
                case "inspect_dir_extendy":
                    xProp1.Attr = new Attribute[] { new DisplayNameAttribute(eName), new MinMaxAttribute(0, 99999999) };
                    xProp1.Editor = new NumericUpDownTypeEditor();
                    break;
                case "stand_width":
                case "stand_width_upper":
                case "stand_width_lower":
                case "stand_height":
                case "stand_height_upper":
                case "stand_height_lower":
                    xProp1.Attr = new Attribute[] { new DisplayNameAttribute(eName), new MinMaxAttribute(0, 99999999f, 0.1f, 4) };
                    xProp1.Editor = new NumericUpDownTypeEditor();
                    break;
                case "stand_angle":
                case "stand_angle_upper":
                case "stand_angle_lower":

                    xProp1.Attr = new Attribute[] { new DisplayNameAttribute(eName), new MinMaxAttribute(-180f, 180f, 0.1f, 4) };
                    xProp1.Editor = new NumericUpDownTypeEditor();

                    break;
                case "fourStartPosition":
                    xProp1.Attr = new Attribute[] { new DisplayNameAttribute(eName) };
                    xProp1.Converter = new JzEnumConverter(typeof(LaserOffsetStart));
                    break;
                case "PreMode":
                    xProp1.Attr = new Attribute[] { new DisplayNameAttribute(eName) };
                    JzEnumConverter jzEnumConverter = new JzEnumConverter(typeof(ProcessImageMode));
                    switch (Traveller106.Universal.FACTORYNAME)
                    {
                        case FactoryName.DONGGUAN:
                        case FactoryName.DAGUI:
                            jzEnumConverter.FilterName = "V2,V7";
                            break;
                        default:
                            break;
                    }
                    
                    xProp1.Converter = jzEnumConverter;// new JzEnumConverter(typeof(ProcessImageMode));
                    break;
                case "PreLaserPos":
                    xProp1.Attr = new Attribute[] { new DisplayNameAttribute(eName) };
                    xProp1.Converter = new JzEnumConverter(typeof(LaserStartPos));
                    break;
                default:
                    xProp1.Attr = new Attribute[] { new DisplayNameAttribute(eName) };
                    break;
            }
            
            m_xprops.Add(xProp1);
        }

    }
}
