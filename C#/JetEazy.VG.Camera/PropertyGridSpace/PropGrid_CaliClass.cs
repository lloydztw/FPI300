using JetEazy.BasicSpace;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;

namespace JetEazy.PropertyGrid
{
    public class MSRItemClass
    {
        public int ReportIndex = 0;
        public string ReportRowCol = "";
        public int RowTag = 0;
        public PointF CenterPointF = new PointF();
        public PointF RelatePointF = new PointF();
        public PointF RelatePointFWorldToView = new PointF();
        public PointF RelatePointFViewToWorld = new PointF();
        public Rectangle Bounds = new Rectangle();
        public float Angle = 0;
        public int nType = 0;

        public string ToDrawString(bool OpenWorld = false)
        {
            string str = string.Empty;

            str += $"{ReportIndex.ToString()}";
            //str += $"Index:{ReportIndex.ToString()}";
            str += Environment.NewLine;
            str += $"No_{ReportRowCol}";
            str += Environment.NewLine;
            str += $"View:{CenterPointF.X.ToString("0.0000")},{CenterPointF.Y.ToString("0.0000")}";
            str += Environment.NewLine;
            if (OpenWorld)
                str += $"World:{RelatePointF.X.ToString("0.0000")},{RelatePointF.Y.ToString("0.0000")}";

            return str;
        }
        public string ToReportString()
        {
            string str = string.Empty;

            str += $"No_{ReportRowCol}";
            str += ",";
            str += $"View,{CenterPointF.X.ToString("0.0000")},{CenterPointF.Y.ToString("0.0000")}";
            str += ",";
            str += $"{Bounds.X.ToString("0.0000")},{Bounds.Y.ToString("0.0000")},{Bounds.Width.ToString("0.0000")},{Bounds.Height.ToString("0.0000")}";
            str += ",";
            str += $"ViewToWorld,{RelatePointF.X.ToString("0.0000")},{RelatePointF.Y.ToString("0.0000")}";
            str += ",";
            str += $"WorldToView,{RelatePointFWorldToView.X.ToString("0.0000")},{RelatePointFWorldToView.Y.ToString("0.0000")}";
            str += ",";
            str += $"ViewToWorld,{RelatePointFViewToWorld.X.ToString("0.0000")},{RelatePointFViewToWorld.Y.ToString("0.0000")}";

            return str;
        }
    }

    public class PropGrid_CaliClass
    {
        public PropGrid_CaliClass()
        {

        }


        #region 校正设定
        const string cat0 = "00.校正设定";

        [CategoryAttribute(cat0), DescriptionAttribute("")]
        [DisplayName("1.起点X")]
        [Browsable(true)]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(-99999999, 99999999)]
        public float loc_x { get; set; } = 0;
        [CategoryAttribute(cat0), DescriptionAttribute("")]
        [DisplayName("2.起点Y")]
        [Browsable(true)]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(-99999999, 99999999)]
        public float loc_y { get; set; } = 0;
        [CategoryAttribute(cat0), DescriptionAttribute("")]
        [DisplayName("3.列间距")]
        [Browsable(true)]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(-99999999, 99999999)]
        public float gap_x { get; set; } = 1;
        [CategoryAttribute(cat0), DescriptionAttribute("")]
        [DisplayName("4.行间距")]
        [Browsable(true)]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(-99999999, 99999999)]
        public float gap_y { get; set; } = 1;
        [CategoryAttribute(cat0), DescriptionAttribute("")]
        [DisplayName("5.列数")]
        [Browsable(true)]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999)]
        public int dir_x_count { get; set; } = 1;
        [CategoryAttribute(cat0), DescriptionAttribute("")]
        [DisplayName("6.行数")]
        [Browsable(true)]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999)]
        public int dir_y_count { get; set; } = 1;

        [CategoryAttribute(cat0), DescriptionAttribute("")]
        [DisplayName("7.排序间距")]
        [Browsable(true)]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999)]
        public int sort_offset { get; set; } = 200;

        [CategoryAttribute(cat0), DescriptionAttribute("")]
        [DisplayName("8.使用固定点")]
        [Browsable(true)]
        //[Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999)]
        public bool use_mark_pointf { get; set; } = false;

        #endregion

        #region 图像处理 blob

        const string cat1 = "02.图像处理";

        [CategoryAttribute(cat1), DescriptionAttribute("")]
        [DisplayName("0A1.灰阶阈值")]
        [Browsable(true)]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 255)]
        public int threshold_value { get; set; } = 128;

        [CategoryAttribute(cat1), DescriptionAttribute("")]
        [DisplayName("0A2.采样值")]
        [Browsable(true)]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 10)]
        public int samplingvalue { get; set; } = 5;

        [CategoryAttribute(cat1), DescriptionAttribute("")]
        [DisplayName("1.面积下限")]
        [Browsable(true)]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999)]
        public float area_min { get; set; } = 0;
        [CategoryAttribute(cat1), DescriptionAttribute("")]
        [DisplayName("2.面积上限")]
        [Browsable(true)]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999)]
        public float area_max { get; set; } = 100;

        [CategoryAttribute(cat1), DescriptionAttribute("")]
        [DisplayName("3.宽度下限")]
        [Browsable(true)]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999)]
        public float width_min { get; set; } = 0;
        [CategoryAttribute(cat1), DescriptionAttribute("")]
        [DisplayName("4.宽度上限")]
        [Browsable(true)]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999)]
        public float width_max { get; set; } = 100;

        [CategoryAttribute(cat1), DescriptionAttribute("")]
        [DisplayName("5.高度下限")]
        [Browsable(true)]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999)]
        public float height_min { get; set; } = 0;
        [CategoryAttribute(cat1), DescriptionAttribute("")]
        [DisplayName("6.高度上限")]
        [Browsable(true)]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999)]
        public float height_max { get; set; } = 100;

        #endregion

        #region 打点设定

        const string cat2 = "03.打点设定";

        [CategoryAttribute(cat2), DescriptionAttribute("")]
        [DisplayName("01.中心点X")]
        [Browsable(true)]
        //[Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999)]
        public int centerx { get; set; } = 0;

        [CategoryAttribute(cat2), DescriptionAttribute("")]
        [DisplayName("02.中心点Y")]
        [Browsable(true)]
        //[Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999)]
        public int centery { get; set; } = 0;

        [CategoryAttribute(cat2), DescriptionAttribute("")]
        [DisplayName("03.偏移x")]
        [Browsable(true)]
        //[Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999)]
        public int offsetx { get; set; } = 0;

        [CategoryAttribute(cat2), DescriptionAttribute("")]
        [DisplayName("04.偏移y")]
        [Browsable(true)]
        //[Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999)]
        public int offsety { get; set; } = 0;

        #endregion

        #region 打点设定

        const string cat3 = "04.自动校正设定";

        [CategoryAttribute(cat3), DescriptionAttribute("")]
        [DisplayName("01.灯光值")]
        [Browsable(true)]
        //[Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999)]
        public int autoLightValue { get; set; } = 255;

        [CategoryAttribute(cat3), DescriptionAttribute("")]
        [DisplayName("02.检测区域")]
        [Browsable(false)]
        //[Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999)]
        public RectangleF autoFindRegion { get; set; } = new RectangleF(10, 10, 100, 100);

        #endregion



        public void FromingStr(string str)
        {
            string[] parts = str.Split(',');
            if (parts.Length > 12)
            {
                loc_x = float.Parse(parts[0]);
                loc_y = float.Parse(parts[1]);
                gap_x = float.Parse(parts[2]);
                gap_y = float.Parse(parts[3]);
                dir_x_count = int.Parse(parts[4]);
                dir_y_count = int.Parse(parts[5]);

                area_min = float.Parse(parts[6]);
                area_max = float.Parse(parts[7]);
                width_min = float.Parse(parts[8]);
                width_max = float.Parse(parts[9]);
                height_min = float.Parse(parts[10]);
                height_max = float.Parse(parts[11]);

                threshold_value = int.Parse(parts[12]);
            }
            if (parts.Length > 14)
            {
                sort_offset = int.Parse(parts[13]);
            }
            if (parts.Length > 15)
            {
                samplingvalue = int.Parse(parts[14]);
            }
            if (parts.Length > 19)
            {
                centerx = int.Parse(parts[15]);
                centery = int.Parse(parts[16]);
                offsetx = int.Parse(parts[17]);
                offsety = int.Parse(parts[18]);
            }
            if (parts.Length > 20)
            {
                use_mark_pointf = parts[19] == "1";
            }
            if (parts.Length > 21)
            {
                autoLightValue = int.Parse(parts[20]);
                autoFindRegion = StringToRectF(parts[21]);
            }

            LoadIniSetup();
        }
        public string ToParaString()
        {
            SaveIniSetup();
            string str = string.Empty;

            str += loc_x.ToString() + ",";
            str += loc_y.ToString() + ",";
            str += gap_x.ToString() + ",";
            str += gap_y.ToString() + ",";
            str += dir_x_count.ToString() + ",";
            str += dir_y_count.ToString() + ",";

            str += area_min.ToString() + ",";
            str += area_max.ToString() + ",";
            str += width_min.ToString() + ",";
            str += width_max.ToString() + ",";
            str += height_min.ToString() + ",";
            str += height_max.ToString() + ",";

            str += threshold_value.ToString() + ",";
            str += sort_offset.ToString() + ",";
            str += samplingvalue.ToString() + ",";

            str += centerx.ToString() + ",";
            str += centery.ToString() + ",";
            str += offsetx.ToString() + ",";
            str += offsety.ToString() + ",";
            str += (use_mark_pointf ? "1" : "0") + ",";
            str += autoLightValue.ToString() + ",";
            str += RectFToString(autoFindRegion) + ",";

            return str;
        }

        string RectFToString(RectangleF RectF)
        {
            string Str = "";

            Str += RectF.X.ToString("0.00") + ";";
            Str += RectF.Y.ToString("0.00") + ";";
            Str += RectF.Width.ToString("0.00") + ";";
            Str += RectF.Height.ToString("0.00");

            return Str;
        }
        RectangleF StringToRectF(string Str)
        {
            string[] strs = Str.Split(';');
            RectangleF rectF = new RectangleF();

            rectF.X = float.Parse(strs[0]);
            rectF.Y = float.Parse(strs[1]);
            rectF.Width = float.Parse(strs[2]);
            rectF.Height = float.Parse(strs[3]);

            return rectF;


        }

        //private static readonly XProps m_xprops = new XProps();
        //public static XProps XPropsInstance
        //{
        //    get { return m_xprops; }
        //}

        private XProps m_xprops = new XProps();
        public XProps XProps
        {
            get { return m_xprops; }
        }

        public void LoadIniSetup()
        {
            m_xprops.Clear();
            string cat0 = "msrcat0";
            AddProperty(cat0, "loc_x", "loc_x", loc_x, "");
            AddProperty(cat0, "loc_y", "loc_y", loc_y, "");
            AddProperty(cat0, "gap_x", "gap_x", gap_x, "");
            AddProperty(cat0, "gap_y", "gap_y", gap_y, "");
            AddProperty(cat0, "dir_x_count", "dir_x_count", dir_x_count, "");
            AddProperty(cat0, "dir_y_count", "dir_y_count", dir_y_count, "");
            AddProperty(cat0, "sort_offset", "sort_offset", sort_offset, "");
            AddProperty(cat0, "use_mark_pointf", "use_mark_pointf", use_mark_pointf, "");

            string cat1 = "msrcat1";
            AddProperty(cat1, "threshold_value", "threshold_value", threshold_value, "");
            AddProperty(cat1, "samplingvalue", "samplingvalue", samplingvalue, "");
            AddProperty(cat1, "area_min", "area_min", area_min, "");
            AddProperty(cat1, "area_max", "area_max", area_max, "");
            AddProperty(cat1, "width_min", "width_min", width_min, "");
            AddProperty(cat1, "width_max", "width_max", width_max, "");
            AddProperty(cat1, "height_min", "height_min", height_min, "");
            AddProperty(cat1, "height_max", "height_max", height_max, "");

            string cat2 = "msrcat2";
            AddProperty(cat2, "centerx", "centerx", centerx, "");
            AddProperty(cat2, "centery", "centery", centery, "");
            AddProperty(cat2, "offsetx", "offsetx", offsetx, "");
            AddProperty(cat2, "offsety", "offsety", offsety, "");

            string cat3 = "msrcat3";
            AddProperty(cat3, "autoLightValue", "autoLightValue", autoLightValue, "");
            //AddProperty(cat3, "autoFindRegion", "autoFindRegion", autoFindRegion, "");
        }
        public void SaveIniSetup()
        {
            foreach (XProp xpropItem in m_xprops)
            {
                switch (xpropItem.ReleateName)
                {
                    #region Cat0
                    case "loc_x":
                        loc_x = float.Parse(xpropItem.Value.ToString());
                        break;
                    case "loc_y":
                        loc_y = float.Parse(xpropItem.Value.ToString());
                        break;
                    case "gap_x":
                        gap_x = float.Parse(xpropItem.Value.ToString());
                        break;
                    case "gap_y":
                        gap_y = float.Parse(xpropItem.Value.ToString());
                        break;
                    case "dir_x_count":
                        dir_x_count = int.Parse(xpropItem.Value.ToString());
                        break;
                    case "dir_y_count":
                        dir_y_count = int.Parse(xpropItem.Value.ToString());
                        break;
                    case "sort_offset":
                        sort_offset = int.Parse(xpropItem.Value.ToString());
                        break;
                    case "use_mark_pointf":
                        use_mark_pointf = (bool)xpropItem.Value;
                        break;
                    #endregion
                    #region Cat1
                    case "threshold_value":
                        threshold_value = int.Parse(xpropItem.Value.ToString());
                        break;
                    case "samplingvalue":
                        samplingvalue = int.Parse(xpropItem.Value.ToString());
                        break;
                    case "area_min":
                        area_min = float.Parse(xpropItem.Value.ToString());
                        break;
                    case "area_max":
                        area_max = float.Parse(xpropItem.Value.ToString());
                        break;
                    case "width_min":
                        width_min = float.Parse(xpropItem.Value.ToString());
                        break;
                    case "width_max":
                        width_max = float.Parse(xpropItem.Value.ToString());
                        break;
                    case "height_min":
                        height_min = float.Parse(xpropItem.Value.ToString());
                        break;
                    case "height_max":
                        height_max = float.Parse(xpropItem.Value.ToString());
                        break;
                    #endregion
                    #region Cat2
                    case "centerx":
                        centerx = int.Parse(xpropItem.Value.ToString());
                        break;
                    case "centery":
                        centery = int.Parse(xpropItem.Value.ToString());
                        break;
                    case "offsetx":
                        offsetx = int.Parse(xpropItem.Value.ToString());
                        break;
                    case "offsety":
                        offsety = int.Parse(xpropItem.Value.ToString());
                        break;
                    case "autoLightValue":
                        autoLightValue = int.Parse(xpropItem.Value.ToString());
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
            xProp1.Attr = new Attribute[] { new DisplayNameAttribute(eName) };
            switch (eReleateName)
            {
                case "SHOW_CHANGE_TIME":
                case "CHANGE_LANGUAGE":

                    //xProp1.Editor = new GetPlugsPropertyEditor();

                    break;
                default:
                    break;
            }

            m_xprops.Add(xProp1);
        }

    }
}
