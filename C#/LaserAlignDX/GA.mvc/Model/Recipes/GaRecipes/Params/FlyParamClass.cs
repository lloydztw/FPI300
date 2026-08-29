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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;

namespace LaserAlignDX.OPSpace.RecipeSpace
{
    public class FlyParaClass : RecipeBaseClass
    {
        const int POINT_COUNT = 8;

        #region SINGLETON
        protected FlyParaClass()
        {

        }
        private static FlyParaClass _instance = null;
        #endregion

        public static FlyParaClass Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new FlyParaClass();
                return _instance;
            }
        }

        #region A00
        const string _Cat0 = "A00.相机参数";

        [CategoryAttribute(_Cat0), DescriptionAttribute("即相机抓图时的曝光值 单位 us")]
        [DisplayName("A01.曝光")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 300, 1f, 2)]
        [Browsable(true)]
        public float xCamExpo { get; set; } = 100f;

        [CategoryAttribute(_Cat0), DescriptionAttribute("即相机抓图时的增益值 单位 dB")]
        [DisplayName("A02.增益")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 20, 1f, 2)]
        [Browsable(true)]
        public float xCamGain { get; set; } = 8f;
        #endregion

        #region A01
        const string _Cat1 = "A01.基础设置";

        [CategoryAttribute(_Cat1), DescriptionAttribute("模板轮廓匹配的相似程度")]
        [DisplayName("A01.相似度")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 1, 0.1f, 2)]
        [Browsable(true)]
        public float xTolerance { get; set; } = 0.5f;

        [CategoryAttribute(_Cat1), DescriptionAttribute("模板轮廓匹配的允许的角度")]
        [DisplayName("A02.角度")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 360, 1f, 2)]
        [Browsable(true)]
        public float xAngle { get; set; } = 30f;

        [CategoryAttribute(_Cat1), DescriptionAttribute("模板轮廓匹配的搜寻范围扩大X方向的像素")]
        [DisplayName("A03.外扩X")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 1f, 2)]
        [Browsable(true)]
        public int xExtendx { get; set; } = 20;

        [CategoryAttribute(_Cat1), DescriptionAttribute("模板轮廓匹配的搜寻范围扩大Y方向的像素")]
        [DisplayName("A04.外扩Y")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 1f, 2)]
        [Browsable(true)]
        public int xExtendy { get; set; } = 20;
        #endregion

        #region A02
        const string _Cat2 = "A02.双头吸嘴找角度设置";

        [CategoryAttribute(_Cat2), DescriptionAttribute("两个吸嘴的计算开启")]
        [DisplayName("A00.开启双头计算")]
        //[TypeConverter(typeof(NumericUpDownTypeConverter))]
        //[Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 255)]
        [Browsable(true)]
        public bool xIsOpenMuit { get; set; } = false;

        [CategoryAttribute(_Cat2), DescriptionAttribute("二值化的阈值")]
        [DisplayName("A01.灰阶阈值")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 255)]
        [Browsable(true)]
        public int xThresholdValue { get; set; } = 128;

        [CategoryAttribute(_Cat2), DescriptionAttribute("寻找的特征选择黑色还是白色 吸嘴是黑色选择黑色")]
        [DisplayName("A02.检测模式")]
        [TypeConverter(typeof(JzEnumConverter))]
        //[Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 360, 1f, 2)]
        [Browsable(true)]
        public BlobMode xBlobMode { get; set; } = BlobMode.Black;

        [CategoryAttribute(_Cat2), DescriptionAttribute("特征二值化后最小的面积")]
        [DisplayName("A03.Blob面积最小值")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 999999999)]
        [Browsable(true)]
        public int xBlobAreaMin { get; set; } = 5000;

        [CategoryAttribute(_Cat2), DescriptionAttribute("特征二值化后最大的面积")]
        [DisplayName("A03.Blob面积最大值")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 999999999)]
        [Browsable(true)]
        public int xBlobAreaMax { get; set; } = 99999999;

        [CategoryAttribute(_Cat2), DescriptionAttribute("true水平 false垂直")]
        [DisplayName("A04.吸嘴是否水平校正")]
        //[TypeConverter(typeof(NumericUpDownTypeConverter))]
        //[Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 255)]
        [Browsable(true)]
        public bool xIsShuiPing { get; set; } = true;
        #endregion

        #region A01
        const string _Cat3 = "A03.读码设置";

        [CategoryAttribute(_Cat3), DescriptionAttribute("true 打开  false  关闭")]
        [DisplayName("A00.开启读码")]
        //[TypeConverter(typeof(NumericUpDownTypeConverter))]
        //[Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 255)]
        [Browsable(true)]
        public bool bOpenCodeReader { get; set; } = false;


        #endregion

        #region GLOBAL_OFFSETS
        [Browsable(false)]
        public PointF[] ptsOffset = new PointF[POINT_COUNT];
        [Browsable(false)]
        public PointF[] ptsOffset2 = new PointF[POINT_COUNT];
        #endregion

        public override void Load(bool eCancel = false)
        {
            xTolerance = float.Parse(ReadINIValue("Fly", "xTolerance", "0.5", INIFILE));
            xAngle = float.Parse(ReadINIValue("Fly", "xAngle", "30", INIFILE));
            xExtendx = int.Parse(ReadINIValue("Fly", "xExtendx", "20", INIFILE));
            xExtendy = int.Parse(ReadINIValue("Fly", "xExtendy", "20", INIFILE));

            xCamExpo = float.Parse(ReadINIValue("Fly", "xCamExpo", "100", INIFILE));
            xCamGain = float.Parse(ReadINIValue("Fly", "xCamGain", "8", INIFILE));

            xIsOpenMuit = ReadINIValue("Basic", "xIsOpenMuit", "0", INIFILE) == "1";
            xThresholdValue = int.Parse(ReadINIValue("Basic", "xThresholdValue", "128", INIFILE));
            xBlobMode = (BlobMode)int.Parse(ReadINIValue("Basic", "xBlobMode", "1", INIFILE));
            xBlobAreaMin = int.Parse(ReadINIValue("Basic", "xBlobAreaMin", "10", INIFILE));
            xBlobAreaMax = int.Parse(ReadINIValue("Basic", "xBlobAreaMax", "20000", INIFILE));
            xIsShuiPing = ReadINIValue("Basic", "xIsShuiPing", "1", INIFILE) == "1";

            bOpenCodeReader = ReadINIValue("Basic", "bOpenCodeReader", "0", INIFILE) == "1";

            int i = 0;
            while (i < POINT_COUNT)
            {
                ptsOffset[i] = StringtoPointF(ReadINIValue("FlyOffset", $"ptsOffset_{i}", $"0,0", INIFILE));
                ptsOffset2[i] = StringtoPointF(ReadINIValue("FlyOffset2", $"ptsOffset2_{i}", $"0,0", INIFILE));

                i++;
            }
        }
        public override void Save()
        {
            WriteINIValue("Fly", "xTolerance", xTolerance.ToString(), INIFILE);
            WriteINIValue("Fly", "xAngle", xAngle.ToString(), INIFILE);
            WriteINIValue("Fly", "xExtendx", xExtendx.ToString(), INIFILE);
            WriteINIValue("Fly", "xExtendy", xExtendy.ToString(), INIFILE);

            WriteINIValue("Fly", "xCamExpo", xCamExpo.ToString(), INIFILE);
            WriteINIValue("Fly", "xCamGain", xCamGain.ToString(), INIFILE);

            WriteINIValue("Basic", "xIsOpenMuit", (xIsOpenMuit ? "1" : "0"), INIFILE);
            WriteINIValue("Basic", "xThresholdValue", xThresholdValue.ToString(), INIFILE);
            WriteINIValue("Basic", "xBlobMode", ((int)xBlobMode).ToString(), INIFILE);
            WriteINIValue("Basic", "xBlobAreaMin", xBlobAreaMin.ToString(), INIFILE);
            WriteINIValue("Basic", "xBlobAreaMax", xBlobAreaMax.ToString(), INIFILE);
            WriteINIValue("Basic", "xIsShuiPing", (xIsShuiPing ? "1" : "0"), INIFILE);

            WriteINIValue("Basic", "bOpenCodeReader", (bOpenCodeReader ? "1" : "0"), INIFILE);

            int i = 0;
            while (i < POINT_COUNT)
            {
                WriteINIValue("FlyOffset", $"ptsOffset_{i}", PointFtoStringSimple(ptsOffset[i]), INIFILE);
                WriteINIValue("FlyOffset2", $"ptsOffset2_{i}", PointFtoStringSimple(ptsOffset2[i]), INIFILE);

                i++;
            }
        }

        public bool GetCameraExpoAndGain(out float expo, out float gain)
        {
            expo = xCamExpo;
            gain = xCamGain;
            return true;
        }
    }
}
