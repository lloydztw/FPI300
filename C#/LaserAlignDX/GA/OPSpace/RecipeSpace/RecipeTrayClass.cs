
//#define OPT_TRAVELLER

#if OPT_TRAVELLER
using Eazy_Project_III;
using JetEazy;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;

namespace Common.RecipeSpace
{

    public class RecipeTrayClass : RecipeBaseClass
    {
        public RecipeTrayClass()
        {

        }
        private static RecipeTrayClass _instance = null;
        public static RecipeTrayClass Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new RecipeTrayClass();
                return _instance;
            }
        }


        #region 分配轨道的坐标

        //public AutoAssignClass[] TARCKASSIGN = new AutoAssignClass[(int)TrackAreaPosition.COUNT];

        #endregion

        //设定轨道的Y轴起始位置
        /*
         * 轨道一  轨道二  轨道三  轨道四
         * PASS     测试区     NG1     NG2
         * 
         */

        [Browsable(false)]
        public float[] Track_PosY_org { get; set; } = new float[4];
        const string LSCat0 = "A00.基础设置";

        [CategoryAttribute(LSCat0), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("Tray盘行数")]
        public int TrayRowCount { get; set; } = 1;
        [CategoryAttribute(LSCat0), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("Tray盘列数")]
        public int TrayColCount { get; set; } = 1;

        [CategoryAttribute(LSCat0), DescriptionAttribute("")]
        [DisplayName("Tray盘单颗长度")]
        public float TraySingleLength { get; set; } = 25.8f;
        [CategoryAttribute(LSCat0), DescriptionAttribute("")]
        [DisplayName("Tray盘单颗宽度")]
        public float TraySingleWidth { get; set; } = 25.8f;

        [CategoryAttribute(LSCat0), DescriptionAttribute("")]
        [DisplayName("Tray盘支撑横向宽")]
        public float TraySingleLengthX { get; set; } = 3.5f;
        [CategoryAttribute(LSCat0), DescriptionAttribute("")]
        [DisplayName("Tray盘支撑纵向宽")]
        public float TraySingleWidthX { get; set; } = 3.5f;

        [Browsable(false)]
        public float[] Track_PosXL_org { get; set; } = new float[2];
        [Browsable(false)]
        public float[] Track_PosXR_org { get; set; } = new float[3];


        const string LSCat1 = "A01.吸嘴设置";
        [CategoryAttribute(LSCat1), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("Tray盘左吸嘴模组最左边(即轨道中trayX方向偏移位置)")]
        [Browsable(false)]
        public float Track_LenXL_LEFT { get; set; } = 1;
        [CategoryAttribute(LSCat1), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("Tray盘左吸嘴模组最上边(即轨道中trayY方向偏移位置)")]
        [Browsable(false)]
        public float Track_LenXL_TOP { get; set; } = 1;
        [CategoryAttribute(LSCat1), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("Tray盘右吸嘴模组最左边(即轨道中trayX方向偏移位置)")]
        [Browsable(false)]
        public float Track_LenXR_LEFT { get; set; } = 1;
        [CategoryAttribute(LSCat1), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("Tray盘右吸嘴模组最上边(即轨道中tray Y方向偏移位置)")]
        [Browsable(false)]
        public float Track_LenXR_TOP { get; set; } = 1;
        [CategoryAttribute(LSCat1), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("左吸嘴模组个数")]
        [Browsable(false)]
        public int PickLeftCount { get; set; } = 1;
        [CategoryAttribute(LSCat1), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("右吸嘴模组个数")]
        [Browsable(false)]
        public int PickRightCount { get; set; } = 1;

        [CategoryAttribute(LSCat1), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("Tray盘左吸嘴间距")]
        //[Browsable(false)]
        public float TrackPitchLeft { get; set; } = 5;
        [CategoryAttribute(LSCat1), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("Tray盘右吸嘴间距")]
        [Browsable(false)]
        public float TrackPitchRight { get; set; } = 5;

        [CategoryAttribute(LSCat1), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("Tray盘左吸嘴变距")]
        //[Browsable(false)]
        public float TrackPitchLeftChange { get; set; } = 4.9f;
        [CategoryAttribute(LSCat1), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("Tray盘右吸嘴变距")]
        [Browsable(false)]
        public float TrackPitchRightChange { get; set; } = 5.9f;

        [Browsable(true)]
        public string sSuUseNumberList1 { get; set; } = "0;2;4;6";
        public List<string> SuUseNumberList1 = new List<string>();

        [Browsable(true)]
        public string sSuUseNumberList2 { get; set; } = "0;2;4;6";
        public List<string> SuUseNumberList2 = new List<string>();

        [Browsable(false)]
        public Bitmap bmpORG { get; set; } = new Bitmap(1, 1);

        [Browsable(false)]
        public RectangleF rect_start { get; set; } = new RectangleF(0, 0, 100, 100);
        [Browsable(false)]
        public RectangleF rect_end { get; set; } = new RectangleF(300, 300, 100, 100);

        const string LSCat2 = "A02.Vision设置";
        [CategoryAttribute(LSCat2), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("分割图像位置(pixel)")]
        [Browsable(true)]
        public int CutHeight { get; set; } = 9100;

        [CategoryAttribute(LSCat2), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("是否画出位置框")]
        [Browsable(true)]
        public bool IsDrawRect { get; set; } = false;

        [CategoryAttribute(LSCat2), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(JzEnumConverter))]
        [DisplayName("检测方式")]
        [Browsable(true)]
        public InspectMode InspectModex { get; set; } = InspectMode.CHIP;

        [CategoryAttribute(LSCat2), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(JzEnumConverter))]
        [DisplayName("无Mark点位置")]
        [Browsable(true)]
        public CornerNoMarkEnum CornerNoMark { get; set; } = CornerNoMarkEnum.RB;

        [CategoryAttribute(LSCat2), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        //[TypeConverter(typeof(JzEnumConverter))]
        [DisplayName("是否检测方向")]
        [Browsable(true)]
        public bool IsCheckCornerNoMark { get; set; } = false;

        [CategoryAttribute(LSCat2), DescriptionAttribute("单位mm")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("01标准宽度")]
        [Browsable(true)]
        public double chip_standard { get; set; } = 0.08;

        [CategoryAttribute(LSCat2), DescriptionAttribute("单位mm")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("02线扫一步间距")]
        [Browsable(true)]
        public double chip_linescanoffset { get; set; } = 43;

        [CategoryAttribute(LSCat2), DescriptionAttribute("单位pixel")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("03截取角落宽度")]
        [Browsable(true)]
        public int chip_CropWidth { get; set; } = 80;

        [CategoryAttribute(LSCat2), DescriptionAttribute("单位pixel")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("04角落样本")]
        [Browsable(true)]
        public int chip_CropSample { get; set; } = 13;

        [CategoryAttribute(LSCat2), DescriptionAttribute("单位mm")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("03拍照次数")]
        [Browsable(true)]
        public int chip_captureCount { get; set; } = 3;

        const string LSCat3 = "A03.补偿设置";
        [CategoryAttribute(LSCat3), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("取料补偿1")]
        [Browsable(true)]
        public float bc1 { get; set; } = 0f;
        [CategoryAttribute(LSCat3), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("取料补偿2")]
        [Browsable(true)]
        public float bc2 { get; set; } = 0f;
        [CategoryAttribute(LSCat3), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("取料补偿3")]
        [Browsable(true)]
        public float bc3 { get; set; } = 0f;

        [CategoryAttribute(LSCat3), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("放料补偿1")]
        [Browsable(true)]
        public float trbc1 { get; set; } = 0f;
        [CategoryAttribute(LSCat3), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("放料补偿2")]
        [Browsable(true)]
        public float trbc2 { get; set; } = 0f;
        [CategoryAttribute(LSCat3), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("放料补偿3")]
        [Browsable(true)]
        public float trbc3 { get; set; } = 0f;

        public override void Load()
        {
            bc1 = float.Parse(ReadINIValue("Track Control", "bc1", bc1.ToString(), INIFILE));
            bc2 = float.Parse(ReadINIValue("Track Control", "bc2", bc2.ToString(), INIFILE));
            bc3 = float.Parse(ReadINIValue("Track Control", "bc3", bc3.ToString(), INIFILE));

            trbc1 = float.Parse(ReadINIValue("Track Control", "trbc1", trbc1.ToString(), INIFILE));
            trbc2 = float.Parse(ReadINIValue("Track Control", "trbc2", trbc2.ToString(), INIFILE));
            trbc3 = float.Parse(ReadINIValue("Track Control", "trbc3", trbc3.ToString(), INIFILE));


            TraySingleLengthX = float.Parse(ReadINIValue("Track Control", "TraySingleLengthX", TraySingleLengthX.ToString(), INIFILE));
            TraySingleWidthX = float.Parse(ReadINIValue("Track Control", "TraySingleWidthX", TraySingleWidthX.ToString(), INIFILE));

            TrackPitchLeftChange = float.Parse(ReadINIValue("Track Control", "TrackPitchLeftChange", TrackPitchLeftChange.ToString(), INIFILE));
            TrackPitchRightChange = float.Parse(ReadINIValue("Track Control", "TrackPitchRightChange", TrackPitchRightChange.ToString(), INIFILE));

            TrackPitchLeft = float.Parse(ReadINIValue("Track Control", "TrackPitchLeft", TrackPitchLeft.ToString(), INIFILE));
            TrackPitchRight = float.Parse(ReadINIValue("Track Control", "TrackPitchRight", TrackPitchRight.ToString(), INIFILE));

            TraySingleLength = float.Parse(ReadINIValue("Track Control", "TraySingleLength", TraySingleLength.ToString(), INIFILE));
            TraySingleWidth = float.Parse(ReadINIValue("Track Control", "TraySingleWidth", TraySingleWidth.ToString(), INIFILE));
            for (int i = 0; i < 4; i++)
            {
                Track_PosY_org[i] = float.Parse(ReadINIValue("Track Control", "Track_PosY_org" + i.ToString(), Track_PosY_org[i].ToString(Format), INIFILE));
            }
            TrayRowCount = int.Parse(ReadINIValue("Track Control", "TrayRowCount", TrayRowCount.ToString(), INIFILE));
            TrayColCount = int.Parse(ReadINIValue("Track Control", "TrayColCount", TrayColCount.ToString(), INIFILE));
            for (int i = 0; i < 2; i++)
            {
                Track_PosXL_org[i] = float.Parse(ReadINIValue("Track Control", "Track_PosXL_org" + i.ToString(), Track_PosXL_org[i].ToString(Format), INIFILE));
            }
            for (int i = 0; i < 3; i++)
            {
                Track_PosXR_org[i] = float.Parse(ReadINIValue("Track Control", "Track_PosXR_org" + i.ToString(), Track_PosXR_org[i].ToString(Format), INIFILE));
            }
            Track_LenXL_LEFT = float.Parse(ReadINIValue("Track Control", "Track_LenXL_LEFT", Track_LenXL_LEFT.ToString(Format), INIFILE));
            Track_LenXL_TOP = float.Parse(ReadINIValue("Track Control", "Track_LenXL_TOP", Track_LenXL_TOP.ToString(Format), INIFILE));
            Track_LenXR_LEFT = float.Parse(ReadINIValue("Track Control", "Track_LenXR_LEFT", Track_LenXR_LEFT.ToString(Format), INIFILE));
            Track_LenXR_TOP = float.Parse(ReadINIValue("Track Control", "Track_LenXR_TOP", Track_LenXR_TOP.ToString(Format), INIFILE));

            PickLeftCount = int.Parse(ReadINIValue("Track Control", "PickLeftCount", PickLeftCount.ToString(), INIFILE));
            PickRightCount = int.Parse(ReadINIValue("Track Control", "PickRightCount", PickRightCount.ToString(), INIFILE));

            sSuUseNumberList1 = ReadINIValue("Track Control", "sSuUseNumberList1", sSuUseNumberList1.ToString(), INIFILE);
            SuUseNumberList1 = sSuUseNumberList1.Split(';').ToList();

            sSuUseNumberList2 = ReadINIValue("Track Control", "sSuUseNumberList2", sSuUseNumberList2.ToString(), INIFILE);
            SuUseNumberList2 = sSuUseNumberList2.Split(';').ToList();

            rect_start = StringtoRectF(ReadINIValue("Recipe Basic", "rect_start", RectFtoStringSimple(rect_start), INIFILE));
            rect_end = StringtoRectF(ReadINIValue("Recipe Basic", "rect_end", RectFtoStringSimple(rect_end), INIFILE));

            CutHeight = int.Parse(ReadINIValue("Vision Control", "CutHeight", CutHeight.ToString(), INIFILE));
            IsDrawRect = ReadINIValue("Vision Control", "IsDrawRect", (IsDrawRect ? "1" : "0"), INIFILE) == "1";
            InspectModex = (InspectMode)int.Parse(ReadINIValue("Vision Control", "InspectModex", ((int)InspectModex).ToString(), INIFILE));
            chip_standard = double.Parse(ReadINIValue("Vision Control", "chip_standard", chip_standard.ToString(), INIFILE));
            CornerNoMark = (CornerNoMarkEnum)int.Parse(ReadINIValue("Vision Control", "CornerNoMark", ((int)CornerNoMark).ToString(), INIFILE));
            IsCheckCornerNoMark = ReadINIValue("Vision Control", "IsCheckCornerNoMark", (IsCheckCornerNoMark ? "1" : "0"), INIFILE) == "1";
            chip_linescanoffset = double.Parse(ReadINIValue("Vision Control", "chip_linescanoffset", chip_linescanoffset.ToString(), INIFILE));
            chip_captureCount = int.Parse(ReadINIValue("Vision Control", "chip_captureCount", chip_captureCount.ToString(), INIFILE));
            chip_CropWidth = int.Parse(ReadINIValue("Vision Control", "chip_CropWidth", chip_CropWidth.ToString(), INIFILE));
            chip_CropSample = int.Parse(ReadINIValue("Vision Control", "chip_CropSample", chip_CropSample.ToString(), INIFILE));


            string bmpfilename = Path + "\\" + IndexStr + "\\" + "000.png";
            if (System.IO.File.Exists(bmpfilename))
            {

                Bitmap bmptmp = new Bitmap(bmpfilename);
                bmpORG.Dispose();
                bmpORG = new Bitmap(bmptmp);
                bmptmp.Dispose();

            }

            _trackallposiotion_init();

            UserModuleClass.Instance.Initial(Path, Index, "ModulePosition.ini");
            SuctionNozzleClass.Instance.Initial(Path, Index, "SuctionAdjust.ini");
        }
        public override void Save()
        {
            WriteINIValue("Track Control", "bc1", bc1.ToString(Format), INIFILE);
            WriteINIValue("Track Control", "bc2", bc2.ToString(Format), INIFILE);
            WriteINIValue("Track Control", "bc3", bc3.ToString(Format), INIFILE);

            WriteINIValue("Track Control", "trbc1", trbc1.ToString(Format), INIFILE);
            WriteINIValue("Track Control", "trbc2", trbc2.ToString(Format), INIFILE);
            WriteINIValue("Track Control", "trbc3", trbc3.ToString(Format), INIFILE);

            WriteINIValue("Track Control", "TrackPitchLeftChange", TrackPitchLeftChange.ToString(Format), INIFILE);
            WriteINIValue("Track Control", "TrackPitchRightChange", TrackPitchRightChange.ToString(Format), INIFILE);

            WriteINIValue("Track Control", "TraySingleLengthX", TraySingleLengthX.ToString(Format), INIFILE);
            WriteINIValue("Track Control", "TraySingleWidthX", TraySingleWidthX.ToString(Format), INIFILE);

            WriteINIValue("Track Control", "TrackPitchLeft", TrackPitchLeft.ToString(Format), INIFILE);
            WriteINIValue("Track Control", "TrackPitchRight", TrackPitchRight.ToString(Format), INIFILE);

            WriteINIValue("Track Control", "TraySingleLength", TraySingleLength.ToString(Format), INIFILE);
            WriteINIValue("Track Control", "TraySingleWidth", TraySingleWidth.ToString(Format), INIFILE);
            for (int i = 0; i < 4; i++)
            {
                WriteINIValue("Track Control", "Track_PosY_org" + i.ToString(), Track_PosY_org[i].ToString(Format), INIFILE);
            }
            WriteINIValue("Track Control", "TrayRowCount", TrayRowCount.ToString(), INIFILE);
            WriteINIValue("Track Control", "TrayColCount", TrayColCount.ToString(), INIFILE);
            for (int i = 0; i < 2; i++)
            {
                WriteINIValue("Track Control", "Track_PosXL_org" + i.ToString(), Track_PosXL_org[i].ToString(Format), INIFILE);
            }
            for (int i = 0; i < 3; i++)
            {
                WriteINIValue("Track Control", "Track_PosXR_org" + i.ToString(), Track_PosXR_org[i].ToString(Format), INIFILE);
            }
            WriteINIValue("Track Control", "Track_LenXL_LEFT", Track_LenXL_LEFT.ToString(Format), INIFILE);
            WriteINIValue("Track Control", "Track_LenXL_TOP", Track_LenXL_TOP.ToString(Format), INIFILE);
            WriteINIValue("Track Control", "Track_LenXR_LEFT", Track_LenXR_LEFT.ToString(Format), INIFILE);
            WriteINIValue("Track Control", "Track_LenXR_TOP", Track_LenXR_TOP.ToString(Format), INIFILE);

            WriteINIValue("Track Control", "PickLeftCount", PickLeftCount.ToString(), INIFILE);
            WriteINIValue("Track Control", "PickRightCount", PickRightCount.ToString(), INIFILE);

            WriteINIValue("Track Control", "sSuUseNumberList1", sSuUseNumberList1.ToString(), INIFILE);
            SuUseNumberList1 = sSuUseNumberList1.Split(';').ToList();

            WriteINIValue("Track Control", "sSuUseNumberList2", sSuUseNumberList2.ToString(), INIFILE);
            SuUseNumberList2 = sSuUseNumberList2.Split(';').ToList();

            WriteINIValue("Recipe Basic", "rect_start", RectFtoStringSimple(rect_start), INIFILE);
            WriteINIValue("Recipe Basic", "rect_end", RectFtoStringSimple(rect_end), INIFILE);

            WriteINIValue("Vision Control", "CutHeight", CutHeight.ToString(), INIFILE);
            WriteINIValue("Vision Control", "IsDrawRect", (IsDrawRect ? "1" : "0"), INIFILE);
            WriteINIValue("Vision Control", "InspectModex", ((int)InspectModex).ToString(), INIFILE);
            WriteINIValue("Vision Control", "chip_standard", chip_standard.ToString(Format), INIFILE);
            WriteINIValue("Vision Control", "CornerNoMark", ((int)CornerNoMark).ToString(), INIFILE);
            WriteINIValue("Vision Control", "IsCheckCornerNoMark", (IsCheckCornerNoMark ? "1" : "0"), INIFILE);
            WriteINIValue("Vision Control", "chip_linescanoffset", chip_linescanoffset.ToString(Format), INIFILE);
            WriteINIValue("Vision Control", "chip_captureCount", chip_captureCount.ToString(), INIFILE);
            WriteINIValue("Vision Control", "chip_CropWidth", chip_CropWidth.ToString(), INIFILE);
            WriteINIValue("Vision Control", "chip_CropSample", chip_CropSample.ToString(), INIFILE);

            string bmpfilename = Path + "\\" + IndexStr + "\\" + "000.png";
            //Bitmap bmptemp = (Bitmap)bmpORG.Clone();
            //bmptemp.Save(bmpfilename, System.Drawing.Imaging.ImageFormat.Png);
            bmpORG.Save(bmpfilename, System.Drawing.Imaging.ImageFormat.Png);
            //bmptemp.Dispose();
            _trackallposiotion_init();
            UserModuleClass.Instance.Save();
            SuctionNozzleClass.Instance.Save();
        }
        void _trackallposiotion_init()
        {
            Track_LenXL_LEFT = RecipeTrayClass.Instance.TraySingleLength * (RecipeTrayClass.Instance.TrayColCount - 1) +
                                               RecipeTrayClass.Instance.TraySingleLengthX * (RecipeTrayClass.Instance.TrayColCount - 1) + Track_PosXL_org[1];
            Track_LenXL_TOP = RecipeTrayClass.Instance.TraySingleWidth * (RecipeTrayClass.Instance.TrayRowCount - 1) +
                                               RecipeTrayClass.Instance.TraySingleWidthX * (RecipeTrayClass.Instance.TrayRowCount - 1) + Track_PosY_org[1];

            Track_LenXR_LEFT = Track_LenXL_LEFT;// Track_PosXR_org[0] - (RecipeTrayClass.Instance.TraySingleLength * (RecipeTrayClass.Instance.TrayColCount - 1) +
                                                //RecipeTrayClass.Instance.TraySingleLengthX * (RecipeTrayClass.Instance.TrayColCount - 1));
            Track_LenXR_TOP = Track_LenXL_TOP;// RecipeTrayClass.Instance.TraySingleWidth * (RecipeTrayClass.Instance.TrayRowCount - 1) +
                                               //RecipeTrayClass.Instance.TraySingleWidthX * (RecipeTrayClass.Instance.TrayRowCount - 1) + Track_PosY_org[1];

           
        }

    }
    public class UserModuleClass : RecipeBaseClass
    {
        const int AXIS_COUNT = 11;
        const int AXIS_POS_LENGTH = 10;
        public float[,] AXIS_POS_SET = new float[AXIS_COUNT, AXIS_POS_LENGTH];//设定轴的10个位置

        public UserModuleClass()
        {

        }
        private static UserModuleClass _instance = null;
        public static UserModuleClass Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new UserModuleClass();
                return _instance;
            }
        }

        public int AxisCount
        {
            get { return AXIS_COUNT; }
        }
        public int AxisPosLength
        {
            get { return AXIS_POS_LENGTH; }
        }

        public override void Load()
        {
            int i = 0;
            int j = 0;
            while (i < AXIS_COUNT)
            {
                j = 0;
                while (j < AXIS_POS_LENGTH)
                {
                    AXIS_POS_SET[i, j] = float.Parse(ReadINIValue("AXIS Module_" + i.ToString("000"), "Pos_" + j.ToString("000"), AXIS_POS_SET[i, j].ToString(Format), INIFILE));

                    j++;
                }
                i++;
            }
        }
        public override void Save()
        {
            int i = 0;
            int j = 0;
            while (i < AXIS_COUNT)
            {
                j = 0;
                while (j < AXIS_POS_LENGTH)
                {
                    WriteINIValue("AXIS Module_" + i.ToString("000"), "Pos_" + j.ToString("000"), AXIS_POS_SET[i, j].ToString(Format), INIFILE);
                    j++;
                }
                i++;
            }
        }
    }
    public class SuctionNozzleClass : RecipeBaseClass
    {
        const string LSCat1 = "A00.图像参数设置";
        #region A00.图像参数设置
        //[Browsable(false)]
        //public Bitmap bmpORG { get; set; } = new Bitmap(1, 1);

        [CategoryAttribute(LSCat1), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("0.模式")]
        [TypeConverter(typeof(JzEnumConverter))]
        [Browsable(true)]
        public LayerMode layermode { get; set; } = LayerMode.White;

        [CategoryAttribute(LSCat1), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("1.最小面积")]
        [Browsable(true)]
        public float area_min { get; set; } = 500;

        [CategoryAttribute(LSCat1), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("2.最大面积")]
        [Browsable(true)]
        public float area_max { get; set; } = 1000000;

        [CategoryAttribute(LSCat1), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("3.长宽比例")]
        [Browsable(true)]
        public float lw_ratio { get; set; } = 10;

        [CategoryAttribute(LSCat1), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("4.线宽")]
        [Browsable(true)]
        public int pen_width { get; set; } = 1;

        [CategoryAttribute(LSCat1), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("5.阈值")]
        [Browsable(true)]
        public int thresholdValue { get; set; } = 50;
        #endregion
        const string LSCat2 = "A01.位置设置";
        #region A01.位置设置

        [CategoryAttribute(LSCat2), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("0.起始位置")]
        [Browsable(true)]
        public float xzPosReady { get; set; } = 18;
        [CategoryAttribute(LSCat2), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("1.吸嘴间距")]
        [Browsable(true)]
        public float xzSuctionOffset { get; set; } = 18;
        [CategoryAttribute(LSCat2), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("2.吸嘴个数"), MinMax(1, 8)]
        [Browsable(true)]
        public int xzSuctionCount { get; set; } = 1;


        [CategoryAttribute(LSCat2), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("3.吸嘴位置结果集合")]
        [Browsable(true)]
        public string xzSuctionPosListstr { get; set; } = "";
        public List<string> xzSuctionPosList = new List<string>();

        #endregion

        public Bitmap[] bmpORG;
        public Bitmap[] bmpRUN;
        public PointF[] PosCenterORG;
        public PointF[] PosCenterRUN;

        public float Offset(int eindex)
        {
            return PosCenterRUN[eindex].Y - PosCenterORG[eindex].Y;
        }

        public SuctionNozzleClass()
        {

        }
        private static SuctionNozzleClass _instance = null;
        public static SuctionNozzleClass Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new SuctionNozzleClass();
                return _instance;
            }
        }
        public override void Load()
        {
            //string bmpfilename = Path + "\\" + IndexStr + "\\" + "SU000.png";
            //if (System.IO.File.Exists(bmpfilename))
            //{
            //    Bitmap bmptmp = new Bitmap(bmpfilename);
            //    bmpORG.Dispose();
            //    bmpORG = new Bitmap(bmptmp);
            //    bmptmp.Dispose();
            //}

            layermode = (LayerMode)int.Parse(ReadINIValue("Basic", "layermode", ((int)layermode).ToString(), INIFILE));
            area_min = float.Parse(ReadINIValue("Basic", "area_min", area_min.ToString(), INIFILE));
            area_max = float.Parse(ReadINIValue("Basic", "area_max", area_max.ToString(), INIFILE));
            lw_ratio = float.Parse(ReadINIValue("Basic", "lw_ratio", lw_ratio.ToString(), INIFILE));
            pen_width = int.Parse(ReadINIValue("Basic", "pen_width", pen_width.ToString(), INIFILE));

            xzPosReady = float.Parse(ReadINIValue("Basic", "xzPosReady", xzPosReady.ToString(), INIFILE));
            xzSuctionOffset = float.Parse(ReadINIValue("Basic", "xzSuctionOffset", xzSuctionOffset.ToString(), INIFILE));
            xzSuctionCount = int.Parse(ReadINIValue("Basic", "xzSuctionCount", xzSuctionCount.ToString(), INIFILE));
            thresholdValue = int.Parse(ReadINIValue("Basic", "thresholdValue", thresholdValue.ToString(), INIFILE));

            xzSuctionPosListstr = ReadINIValue("Pos Control", "xzSuctionPosListstr", xzSuctionPosListstr.ToString(), INIFILE);
            xzSuctionPosList = xzSuctionPosListstr.Split(';').ToList();

            bmpORG = new Bitmap[8];
            bmpRUN = new Bitmap[8];
            PosCenterORG = new PointF[8];
            PosCenterRUN = new PointF[8];
            int i = 0;
            while (i < 8)
            {
                bmpORG[i] = new Bitmap(1, 1);
                if (System.IO.File.Exists(Path + "\\" + IndexStr + "\\SU" + i.ToString("000") + ".png"))
                    GetBMP(Path + "\\" + IndexStr + "\\SU" + i.ToString("000") + ".png", ref bmpORG[i]);
                bmpRUN[i] = new Bitmap(1, 1);
                PosCenterORG[i] = new PointF();
                PosCenterORG[i] = StringToPointF(ReadINIValue("PosORG Control", "PosCenterORG_" + i.ToString(),
                                                                     PointF000ToString(PosCenterORG[i]), INIFILE));
                
                PosCenterRUN[i] = new PointF();
                i++;
            }

        }
        public override void Save()
        {
            //string bmpfilename = Path + "\\" + IndexStr + "\\" + "SU000.png";
            //bmpORG.Save(bmpfilename, System.Drawing.Imaging.ImageFormat.Png);

            WriteINIValue("Basic", "layermode", ((int)layermode).ToString(), INIFILE);
            WriteINIValue("Basic", "area_min", area_min.ToString(Format), INIFILE);
            WriteINIValue("Basic", "area_max", area_max.ToString(Format), INIFILE);
            WriteINIValue("Basic", "lw_ratio", lw_ratio.ToString(Format), INIFILE);
            WriteINIValue("Basic", "pen_width", pen_width.ToString(), INIFILE);

            WriteINIValue("Basic", "xzPosReady", xzPosReady.ToString(Format), INIFILE);
            WriteINIValue("Basic", "xzSuctionOffset", xzSuctionOffset.ToString(Format), INIFILE);
            WriteINIValue("Basic", "xzSuctionCount", xzSuctionCount.ToString(), INIFILE);
            WriteINIValue("Basic", "thresholdValue", thresholdValue.ToString(), INIFILE);

            WriteINIValue("Pos Control", "xzSuctionPosListstr", xzSuctionPosListstr.ToString(), INIFILE);
            xzSuctionPosList = xzSuctionPosListstr.Split(';').ToList();

            SaveORGBMP();
            SavePosition();
        }

        public void SaveORGBMP()
        {
            for (int i = 0; i < bmpORG.Length; i++)
            {
                bmpORG[i].Save(Path + "\\" + IndexStr + "\\SU" + i.ToString("000") + ".png", Universal.GlobalImageFormat);
            
            }
        }
        public void SavePosition()
        {
            for (int i = 0; i < PosCenterORG.Length; i++)
            {
                WriteINIValue("PosORG Control", "PosCenterORG_" + i.ToString(),
                                                                     PointF000ToString(PosCenterORG[i]), INIFILE);
            }
        }

        void GetBMP(string BMPFileStr, ref Bitmap BMP)
        {
            Bitmap bmpTMP = new Bitmap(BMPFileStr);

            BMP.Dispose();
            BMP = new Bitmap(bmpTMP);

            bmpTMP.Dispose();
        }
        public string PointF000ToString(PointF PTF)
        {
            return PTF.X.ToString("0.000") + "," + PTF.Y.ToString("0.000");
        }
        public PointF StringToPointF(string Str)
        {
            string[] strs = Str.Split(',');
            return new PointF(float.Parse(strs[0]), float.Parse(strs[1]));
        }
    }
    public class VisionTrayClass
    {
        public VisionTrayClass()
        {

        }
        //private static VisionTrayClass _instance = null;
        //public static VisionTrayClass Instance
        //{
        //    get
        //    {
        //        if (_instance == null)
        //            _instance = new VisionTrayClass();
        //        return _instance;
        //    }
        //}
        [Browsable(false)]
        public Bitmap bmpORG { get; set; } = new Bitmap(1, 1);

        [Browsable(false)]
        public RectangleF rect_start { get; set; } = new RectangleF(0, 0, 100, 100);
        [Browsable(false)]
        public RectangleF rect_end { get; set; } = new RectangleF(300, 300, 100, 100);

        //public override void Load()
        //{
        //    //base.Load();

        //    //rect_start = StringtoRectF(ReadINIValue("Recipe Basic", "rect_start", RectFtoStringSimple(rect_start), INIFILE));
        //    //rect_end = StringtoRectF(ReadINIValue("Recipe Basic", "rect_end", RectFtoStringSimple(rect_end), INIFILE));

        //    //string bmpfilename = Path + "\\" + IndexStr + "\\" + "000.png";
        //    //if (System.IO.File.Exists(bmpfilename))
        //    //{

        //    //    Bitmap bmptmp = new Bitmap(bmpfilename);
        //    //    bmpORG.Dispose();
        //    //    bmpORG=new Bitmap(bmptmp);
        //    //    bmptmp.Dispose();

        //    //}
        //}
        //public override void Save()
        //{
        //    //base.Save();

        //    //WriteINIValue("Recipe Basic", "rect_start", RectFtoStringSimple(rect_start), INIFILE);
        //    //WriteINIValue("Recipe Basic", "rect_end", RectFtoStringSimple(rect_end), INIFILE);

        //    //string bmpfilename = Path + "\\" + IndexStr + "\\" + "000.png";
        //    //bmpORG.Save(bmpfilename, System.Drawing.Imaging.ImageFormat.Png);
        //}
    }


}
#endif