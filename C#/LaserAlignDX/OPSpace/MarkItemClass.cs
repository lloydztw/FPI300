using AUVision;
using JetEazy;
using JetEazy.BasicSpace;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using VisionDesigner.AlmightyPatMatch;
using VisionDesigner;
using System.Runtime.InteropServices;
using MoveGraphLibrary;
using System.IO;
using Traveller106;
using Eazy_Project_III;

namespace LaserAlignDX.OPSpace
{
    public enum FunTool
    {
        AUFIND = 0,
        HP = 1,
        Gray = 2,
    }
    public enum FindType
    {
        BLOB = 0,
        CIRCLE = 1,
    }
    public class MarkItemClass
    {
        const char SeperateCharG = '\x06';
        const string LSCat00 = "A00.Mark参数设定";

        public MarkItemClass() { }

        [CategoryAttribute(LSCat00), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A01.编号")]
        [Browsable(true)]
        public int Index { get; set; } = 0;
        [CategoryAttribute(LSCat00), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A02.名称")]
        [Browsable(true)]
        public string Name { get; set; } = string.Empty;
        [CategoryAttribute(LSCat00), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A03.亮度")]
        [Browsable(true)]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 255)]
        public int Brightness { get; set; } = 0;
        [CategoryAttribute(LSCat00), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A04.对比")]
        [Browsable(true)]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 255)]
        public int Contrast { get; set; } = 0;
        [CategoryAttribute(LSCat00), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A05.灰度阈值")]
        [Browsable(true)]
        public int ThresholdValue { get; set; } = 60;
        [CategoryAttribute(LSCat00), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A06.找白斑")]
        [Browsable(true)]
        public bool IsFindWhite { get; set; } = false;
        [CategoryAttribute(LSCat00), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A07.开启判断方向")]
        [Browsable(true)]
        public bool IsOpenAUFind { get; set; } = false;
        [CategoryAttribute(LSCat00), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A08.相似度")]
        [Browsable(true)]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 1, 0.1f, 2)]
        public float Tolerance { get; set; } = 0.8f;
        [CategoryAttribute(LSCat00), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A09.角度")]
        [Browsable(true)]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 180, 0.1f, 2)]
        public float Angle { get; set; } = 15;
        [CategoryAttribute(LSCat00), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A10.采样值")]
        [Browsable(true)]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 300)]
        public int Samplesize { get; set; } = 100;

        [CategoryAttribute(LSCat00), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A11.外扩X")]
        [Browsable(true)]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999)]
        public int Extendx { get; set; } = 20;

        [CategoryAttribute(LSCat00), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A12.外扩Y")]
        [Browsable(true)]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999)]
        public int Extendy { get; set; } = 20;

        [CategoryAttribute(LSCat00), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A13.寻找方式")]
        [Browsable(true)]
        //[Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999)]
        public FindType mFindType { get; set; } = FindType.BLOB;

        [DisplayName("B01.检查长度百分比")]
        [Browsable(true)]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 100, 1f, 2)]
        public float checkwidth { get; set; } = 5;
        [DisplayName("B02.检查宽度百分比")]
        [Browsable(true)]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 100, 1f, 2)]
        public float checkheight { get; set; } = 5;
        [DisplayName("B03.检查面积百分比")]
        [Browsable(false)]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 100, 1f, 2)]
        public float checkarea { get; set; } = 5;


        const string LSCat01 = "A01.找圆参数设定";
        [CategoryAttribute(LSCat01), DescriptionAttribute("滤波核半宽：用于增强边缘和抑制噪声，最小值为1。当边缘模糊或有噪声干扰时，增大该值有利于使得检测结果更加稳定，但如果边缘与边缘之间距离小于滤波尺寸时反而会影响边缘位置的精度甚至丢失边缘，该值须要根据实际情况设置。")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A01.滤波核半宽")]
        [Browsable(true)]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(1, 50)]
        public int CircleEdgeWidth { get; set; } = 2;

        [CategoryAttribute(LSCat01), DescriptionAttribute("圆最小半径")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A02.圆最小半径")]
        [Browsable(true)]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(1, 1000)]
        public int CircleMinRadius { get; set; } = 10;
        [CategoryAttribute(LSCat01), DescriptionAttribute("圆最大半径")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A03.圆最大半径")]
        [Browsable(true)]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(1, 10000)]
        public int CircleMaxRadius { get; set; } = 100;
        [CategoryAttribute(LSCat01), DescriptionAttribute("卡尺数量：用于扫描边缘点的ROI区域数量。")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("A04.卡尺数量")]
        [Browsable(true)]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(3, 1000)]
        public int CircleRadNum { get; set; } = 30;

        FunTool m_FunTool = FunTool.HP;

        float org_width = 100;
        float org_height = 100;
        float org_area = 10000;

        public int CheckBlobSize(RectangleF rect_input, int area_input)
        {
            int iret = -1;
            nDesc2 = $"{ToChangeLanguage("定位点")}NG";
            bool b1 = IsInRangeRatio(rect_input.Width, org_width, checkwidth);
            bool b2 = IsInRangeRatio(rect_input.Height, org_height, checkheight);
            bool b3 = IsInRangeRatio(area_input, org_area, checkarea);
            b3 = true;
            if (b1 && b2 && b3)
            {
                iret = 0;
                nDesc2 = $"{ToChangeLanguage("定位点")}OK";
            }
            else if (!b1)
            {
                nDesc2 = $"{ToChangeLanguage("定位点长度")}NG";
            }
            else if (!b2)
            {
                nDesc2 = $"{ToChangeLanguage("定位点宽度")}NG";
            }
            else if (!b3)
            {
                nDesc2 = $"{ToChangeLanguage("定位点面积")}NG";
            }
            return iret;
        }
        bool IsInRangeRatio(double FromValue, double CompValue, double Ratio)
        {
            return (FromValue >= (CompValue * (1 - (Ratio / 100d)))) && (FromValue <= (CompValue * (1 + (Ratio / 100d))));
        }

        /// <summary>
        /// 检查定位的结果信息
        /// </summary>
        [Browsable(false)]
        public string nDesc { get; set; } = string.Empty;


        /// <summary>
        /// 检查blob的结果
        /// </summary>
        [Browsable(false)]
        public bool nOK { get; set; } = false;

        /// <summary>
        /// 检查blob的结果信息
        /// </summary>
        [Browsable(false)]
        public string nDesc2 { get; set; } = string.Empty;

        public void FormString(string Str)
        {
            string[] strings = Str.Split(SeperateCharG);
            if (strings.Length > 9)
            {
                Index = int.Parse(strings[0]);
                Name = strings[1];
                Brightness = int.Parse(strings[2]);
                Contrast = int.Parse(strings[3]);
                ThresholdValue = int.Parse(strings[4]);
                IsFindWhite = strings[5] == "1";
                IsOpenAUFind = strings[6] == "1";
                Tolerance = float.Parse(strings[7]);
                Angle = float.Parse(strings[8]);
                Samplesize = int.Parse(strings[9]);
            }
            if (strings.Length > 14)
            {
                Extendx = int.Parse(strings[10]);
                Extendy = int.Parse(strings[11]);

                checkwidth = float.Parse(strings[12]);
                checkheight = float.Parse(strings[13]);
                checkarea = float.Parse(strings[14]);
            }
            if (strings.Length > 14)
            {
                mFindType = (FindType)int.Parse(strings[15]);
            }

            LoadIniSetup();
        }
        public void FormString1(string Str)
        {
            string[] strings = Str.Split(SeperateCharG);
            if (strings.Length > 0)
            {
                if (!string.IsNullOrEmpty(strings[0]))
                    CircleEdgeWidth = int.Parse(strings[0]);
            }
            if (strings.Length > 3)
            {
                CircleMinRadius = int.Parse(strings[1]);
                CircleMaxRadius = int.Parse(strings[2]);
                CircleRadNum = int.Parse(strings[3]);
            }

            LoadIniSetup();
        }
        public string ToParaString()
        {
            SaveIniSetup();
            string Str = string.Empty;

            Str += Index.ToString() + SeperateCharG;
            Str += Name.ToString() + SeperateCharG;
            Str += Brightness.ToString() + SeperateCharG;
            Str += Contrast.ToString() + SeperateCharG;
            Str += ThresholdValue.ToString() + SeperateCharG;
            Str += (IsFindWhite ? "1" : "0") + SeperateCharG;
            Str += (IsOpenAUFind ? "1" : "0") + SeperateCharG;
            Str += Tolerance.ToString() + SeperateCharG;
            Str += Angle.ToString() + SeperateCharG;
            Str += Samplesize.ToString() + SeperateCharG;
            Str += Extendx.ToString() + SeperateCharG;
            Str += Extendy.ToString() + SeperateCharG;
            Str += checkwidth.ToString() + SeperateCharG;
            Str += checkheight.ToString() + SeperateCharG;
            Str += checkarea.ToString() + SeperateCharG;
            Str += ((int)mFindType).ToString() + SeperateCharG;

            return Str;
        }
        public string ToParaString1()
        {
            SaveIniSetup();
            string Str = string.Empty;
            //注意这里添加的个数 防止写入不了 INI
            Str += CircleEdgeWidth.ToString() + SeperateCharG;
            Str += CircleMinRadius.ToString() + SeperateCharG;
            Str += CircleMaxRadius.ToString() + SeperateCharG;
            Str += CircleRadNum.ToString() + SeperateCharG;

            return Str;
        }

        private XProps m_xprops = new XProps();
        public XProps XPropsMark
        {
            get { return m_xprops; }
        }

        public void LoadIniSetup()
        {
            m_xprops.Clear();
            string cat0 = "markcat0";
            AddProperty(cat0, "Index", "Index", Index, "");
            AddProperty(cat0, "Name", "Name", Name, "");
            AddProperty(cat0, "Brightness", "Brightness", Brightness, "");
            AddProperty(cat0, "Contrast", "Contrast", Contrast, "");
            AddProperty(cat0, "ThresholdValue", "ThresholdValue", ThresholdValue, "");
            AddProperty(cat0, "IsFindWhite", "IsFindWhite", IsFindWhite, "");
            AddProperty(cat0, "IsOpenAUFind", "IsOpenAUFind", IsOpenAUFind, "");
            AddProperty(cat0, "Tolerance", "Tolerance", Tolerance, "");
            AddProperty(cat0, "Angle", "Angle", Angle, "");
            AddProperty(cat0, "Samplesize", "Samplesize", Samplesize, "");
            AddProperty(cat0, "Extendx", "Extendx", Extendx, "");
            AddProperty(cat0, "Extendy", "Extendy", Extendy, "");
            AddProperty(cat0, "checkwidth", "checkwidth", checkwidth, "");
            AddProperty(cat0, "checkheight", "checkheight", checkheight, "");
            AddProperty(cat0, "checkarea", "checkarea", checkarea, "");
            AddProperty(cat0, "mFindType", "mFindType", mFindType, "");
            string cat1 = "markcat1";
            AddProperty(cat1, "CircleEdgeWidth", "CircleEdgeWidth", CircleEdgeWidth, "");
            AddProperty(cat1, "CircleMinRadius", "CircleMinRadius", CircleMinRadius, "");
            AddProperty(cat1, "CircleMaxRadius", "CircleMaxRadius", CircleMaxRadius, "");
            AddProperty(cat1, "CircleRadNum", "CircleRadNum", CircleRadNum, "");

        }
        public void SaveIniSetup()
        {
            foreach (XProp xpropItem in m_xprops)
            {
                switch (xpropItem.ReleateName)
                {
                    #region Cat0
                    case "Index":
                        Index = (int)xpropItem.Value;
                        break;
                    case "Name":
                        Name = (string)xpropItem.Value;
                        break;
                    case "Brightness":
                        Brightness = (int)xpropItem.Value;
                        break;
                    case "Contrast":
                        Contrast = (int)xpropItem.Value;
                        break;
                    case "ThresholdValue":
                        ThresholdValue = (int)xpropItem.Value;
                        break;
                    case "IsFindWhite":
                        IsFindWhite = (bool)xpropItem.Value;
                        break;
                    case "IsOpenAUFind":
                        IsOpenAUFind = (bool)xpropItem.Value;
                        break;
                    case "Tolerance":
                        Tolerance = (float)xpropItem.Value;
                        break;
                    case "Angle":
                        Angle = (float)xpropItem.Value;
                        break;
                    case "Samplesize":
                        Samplesize = (int)xpropItem.Value;
                        break;
                    case "Extendx":
                        Extendx = (int)xpropItem.Value;
                        break;
                    case "Extendy":
                        Extendy = (int)xpropItem.Value;
                        break;
                    case "checkwidth":
                        checkwidth = (float)xpropItem.Value;
                        break;
                    case "checkheight":
                        checkheight = (float)xpropItem.Value;
                        break;
                    case "checkarea":
                        checkarea = (float)xpropItem.Value;
                        break;
                    case "mFindType":
                        mFindType = (FindType)xpropItem.Value;
                        break;
                    #endregion
                    #region Cat1
                    case "CircleEdgeWidth":
                        CircleEdgeWidth = (int)xpropItem.Value;
                        break;
                    case "CircleMinRadius":
                        CircleMinRadius = (int)xpropItem.Value;
                        break;
                    case "CircleMaxRadius":
                        CircleMaxRadius = (int)xpropItem.Value;
                        break;
                    case "CircleRadNum":
                        CircleRadNum = (int)xpropItem.Value;
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
                case "Brightness":
                case "Contrast":
                    xProp1.Attr = new Attribute[] { new DisplayNameAttribute(eName), new MinMaxAttribute(0, 255) };
                    xProp1.Editor = new NumericUpDownTypeEditor();
                    break;
                case "Tolerance":
                    xProp1.Attr = new Attribute[] { new DisplayNameAttribute(eName), new MinMaxAttribute(0, 1, 0.1f, 2) };
                    xProp1.Editor = new NumericUpDownTypeEditor();
                    break;
                case "Angle":
                    xProp1.Attr = new Attribute[] { new DisplayNameAttribute(eName), new MinMaxAttribute(0, 180, 0.1f, 2) };
                    xProp1.Editor = new NumericUpDownTypeEditor();
                    break;
                case "Samplesize":
                    xProp1.Attr = new Attribute[] { new DisplayNameAttribute(eName), new MinMaxAttribute(0, 300) };
                    xProp1.Editor = new NumericUpDownTypeEditor();
                    break;
                case "Extendx":
                case "Extendy":
                    xProp1.Attr = new Attribute[] { new DisplayNameAttribute(eName), new MinMaxAttribute(0, 99999999) };
                    xProp1.Editor = new NumericUpDownTypeEditor();
                    break;
                case "checkwidth":
                case "checkheight":
                case "checkarea":
                    xProp1.Attr = new Attribute[] { new DisplayNameAttribute(eName), new MinMaxAttribute(0, 100, 1f, 2) };
                    xProp1.Editor = new NumericUpDownTypeEditor();
                    break;
                case "mFindType":
                    xProp1.Attr = new Attribute[] { new DisplayNameAttribute(eName) };
                    xProp1.Converter = new JzEnumConverter(typeof(FindType));
                    break;
                case "CircleEdgeWidth":
                    xProp1.Attr = new Attribute[] { new DisplayNameAttribute(eName), new MinMaxAttribute(1, 50) };
                    xProp1.Editor = new NumericUpDownTypeEditor();
                    break;
                case "CircleMinRadius":
                    xProp1.Attr = new Attribute[] { new DisplayNameAttribute(eName), new MinMaxAttribute(1, 1000) };
                    xProp1.Editor = new NumericUpDownTypeEditor();
                    break;
                case "CircleMaxRadius":
                    xProp1.Attr = new Attribute[] { new DisplayNameAttribute(eName), new MinMaxAttribute(1, 10000) };
                    xProp1.Editor = new NumericUpDownTypeEditor();
                    break;
                case "CircleRadNum":
                    xProp1.Attr = new Attribute[] { new DisplayNameAttribute(eName), new MinMaxAttribute(3, 1000) };
                    xProp1.Editor = new NumericUpDownTypeEditor();
                    break;
                default:
                    xProp1.Attr = new Attribute[] { new DisplayNameAttribute(eName) };
                    break;
            }

            m_xprops.Add(xProp1);
        }

        #region 整版判断方向及特征
        [Browsable(false)]
        public AUFindClass Ana_myFind_dir = new AUFindClass();
        [Browsable(false)]
        private FindParaClass Ana_paraClass_dir = new FindParaClass();

        VisionDesigner.AlmightyPatMatch.CAlmightyPattern cAlmightyPatternObj = null;
        VisionDesigner.AlmightyPatMatch.CAlmightyPatMatchTool cAlmightyPatmatchToolObj = null;

        /// <summary>
        /// 判断方向训练
        /// </summary>
        /// <param name="bmpInput">输入图像</param>
        /// <returns></returns>
        public bool AnaTrain(Bitmap bmpInput)
        {
            if (!IsOpenAUFind)
                return true;

            Ana_paraClass_dir.tolerance = Tolerance;
            Ana_paraClass_dir.angle = Angle;
            Ana_paraClass_dir.samplesize = Samplesize;

            RectangleF rect = new RectangleF(0, 0, bmpInput.Width, bmpInput.Height);
            //取出Train的小图
            Bitmap bmpTrain = bmpInput.Clone(rect, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            _preImage(ref bmpTrain);

            bool bOK = true;
            switch (m_FunTool)
            {
                case FunTool.HP:

                    #region HIK_TRAIN

                    // CreatePatternInstance
                    if (cAlmightyPatternObj == null)
                        cAlmightyPatternObj = new VisionDesigner.AlmightyPatMatch.CAlmightyPattern();

                    //Set type

                    cAlmightyPatternObj.Type = PatMatchAlgorithmType.HPFeature;

                    // Set input image
                    //Bitmap bmp24 = bmppattern.Clone(new Rectangle(0, 0, bmppattern.Width, bmppattern.Height), PixelFormat.Format8bppIndexed);
                    AForge.Imaging.Filters.Grayscale grayscale = new AForge.Imaging.Filters.Grayscale(0.299, 0.587, 0.114);
                    Bitmap bmp24 = grayscale.Apply(bmpTrain);
                    //bmppattern.Save("D:\\Data\\bmp24" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + Universal.GlobalImageTypeString, Universal.GlobalImageFormat);

                    VisionDesigner.CMvdImage cInputImg = BitmapToCMvdImage(bmp24);
                    if (cInputImg.PixelFormat != MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08)
                    {
                        //当前程序仅支持mono8。因此像素格会转换.
                        cInputImg.ConvertImagePixelFormat(MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08);
                    }

                    string strvaluex = string.Empty;

                    //cAlmightyPatternObj.SetRunParam("PyramidScaleFlag", "0");
                    //cAlmightyPatternObj.SetRunParam("PyramidScaleRough", "10");
                    //cAlmightyPatternObj.SetRunParam("PyramidScaleFine", "1");

                    //cAlmightyPatternObj.SetRunParam("EdgeThresholdFlag", "0");
                    //cAlmightyPatternObj.SetRunParam("EdgeThreshold", "80");

                    //cAlmightyPatternObj.GetRunParam("PyramidScaleFlag", ref strvaluex);
                    //cInputImg.InitImage("InputTest.bmp");

                    cAlmightyPatternObj.InputImage = cInputImg;
                    //cAlmightyPatternObj.RegionImage = cInputImg;
                    // Set ROI region (optional)
                    cAlmightyPatternObj.RegionList.Clear();
                    float extendvalue = 1f;
                    var region1 = new VisionDesigner.CMvdRectangleF(cInputImg.Width * 0.5f, cInputImg.Height * 0.5f, cInputImg.Width * 0.25f, cInputImg.Height * 0.25f);
                    region1 = new VisionDesigner.CMvdRectangleF(cInputImg.Width * 0.5f, cInputImg.Height * 0.5f, cInputImg.Width * extendvalue, cInputImg.Height * extendvalue);

                    cAlmightyPatternObj.RegionList.Add(new CAlmightyPatMatchRegion(region1, true));

                    // Set basic parameter

                    cAlmightyPatternObj.BasicParam.FixPoint = new VisionDesigner.MVD_POINT_F(Convert.ToSingle(cInputImg.Width * extendvalue) / 2, Convert.ToSingle(cInputImg.Height * extendvalue) / 2);

                    string errmessage = string.Empty;
                    // Train
                    try
                    {
                        cAlmightyPatternObj.Train();
                        bOK = true;
                    }
                    catch (Exception ex)
                    {
                        errmessage = ex.Message;
                        bOK = false;
                    }



                    //// Export Pattern(optional)

                    //cAlmightyPatternObj.ExportPattern("HPPattern.hpmxml");

                    //// Import Pattern(optional)

                    //cAlmightyPatternObj.ImportPattern("HPPattern.hpmxml");

                    // Get the result

                    //VisionDesigner.AlmightyPatMatch.CAlmightyPatternResult cAlmightyPatternRes = cAlmightyPatternObj.GetPatternResult();
                    //cAlmightyPatternRes.TrainedImage.SaveImage("D:\\Data\\" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + Universal.GlobalImageTypeString);


                    //Console.WriteLine("Pattern Size: ({0},{1})", cAlmightyPatternRes.Size.nWidth, cAlmightyPatternRes.Size.nHeight);

                    #endregion

                    break;
                default:
                    Ana_myFind_dir.bmpItem = bmpTrain;
                    //Ana_myFind_dir.myFunTool = FunTool.AUFIND;
                    bOK = Ana_myFind_dir.Train2(rect, Ana_paraClass_dir);
                    break;
            }

            bmpTrain.Dispose();
            return bOK;
        }
        /// <summary>
        /// 判断方向测试
        /// </summary>
        /// <param name="bmpInput">输入图像</param>
        /// <param name="nDesc">返回的信息</param>
        /// <returns>0:OK 小于0:NG</returns>
        public int AnaRun(Bitmap bmpInput)
        {
            Ana_myFind_dir.xResults.Clear();
            nDesc = string.Empty;
            if (!IsOpenAUFind)
            {
                nDesc = $"F0{ToChangeLanguage($"未启用定位")}";
                return 0;
            }
            bool bOK = false;
            _preImage(ref bmpInput);

            switch (m_FunTool)
            {
                case FunTool.HP:

                    #region HIK_RUN

                    // CreateToolInstance
                    if (cAlmightyPatmatchToolObj == null)
                        cAlmightyPatmatchToolObj = new VisionDesigner.AlmightyPatMatch.CAlmightyPatMatchTool();

                    //Set type

                    cAlmightyPatmatchToolObj.Type = PatMatchAlgorithmType.HPFeature;

                    //// Set ROI region (optional)
                    cAlmightyPatmatchToolObj.RegionList.Clear();
                    var region2 = new VisionDesigner.CMvdRectangleF(bmpInput.Width * 0.5f, bmpInput.Height * 0.5f, bmpInput.Width, bmpInput.Height);
                    cAlmightyPatmatchToolObj.RegionList.Add(new CAlmightyPatMatchRegion(region2, true));

                    // Set basic parameter

                    cAlmightyPatmatchToolObj.BasicParam.ShowOutlineStatus = false;

                    // Set Pattern

                    cAlmightyPatmatchToolObj.Pattern = cAlmightyPatternObj;

                    cAlmightyPatmatchToolObj.SetRunParam("AngleStart", (-Ana_paraClass_dir.angle).ToString());
                    cAlmightyPatmatchToolObj.SetRunParam("AngleEnd", Ana_paraClass_dir.angle.ToString());
                    cAlmightyPatmatchToolObj.SetRunParam("MinScore", Ana_paraClass_dir.tolerance.ToString());

                    // Set input image
                    //Bitmap bmp24 = bmpinput.Clone(new Rectangle(0, 0, bmpinput.Width, bmpinput.Height), PixelFormat.Format8bppIndexed);
                    AForge.Imaging.Filters.Grayscale grayscale = new AForge.Imaging.Filters.Grayscale(0.299, 0.587, 0.114);
                    Bitmap bmp24 = grayscale.Apply(bmpInput);
                    VisionDesigner.CMvdImage cInputImg2 = BitmapToCMvdImage(bmp24);
                    if (cInputImg2.PixelFormat != MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08)
                    {
                        //当前程序仅支持mono8。因此像素格会转换.
                        cInputImg2.ConvertImagePixelFormat(MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08);
                    }
                    //cInputImg2.InitImage("InputTest2.bmp");

                    cAlmightyPatmatchToolObj.InputImage = cInputImg2;

                    // Running

                    cAlmightyPatmatchToolObj.Run();

                    // Get the result

                    VisionDesigner.AlmightyPatMatch.CAlmightyPatMatchResult cHPMatchRes = cAlmightyPatmatchToolObj.Result;

                    //cAlmightyPatmatchToolObj.InputImage.SaveImage("D:\\Data\\matchrun_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + Universal.GlobalImageTypeString);

                    //foreach (var item in cHPMatchRes.MatchInfoList)

                    //{

                    //    Console.WriteLine("MatchPoint: ({0},{1})", item.MatchPoint.fX, item.MatchPoint.fY);

                    //}
                    int resultcount = cHPMatchRes.MatchInfoList.Count;
                    if (resultcount > 0)
                    {
                        xFindResult result = new xFindResult();
                        var item = cHPMatchRes.MatchInfoList[0];
                        result.fCenterX = item.MatchBox.CenterX;
                        result.fCenterY = item.MatchBox.CenterY;
                        result.fAngle = item.MatchBox.Angle;
                        result.fScale = item.Scale;
                        result.fScore = item.Score;

                        Ana_myFind_dir.xResults.Add(result);
                    }

                    if (Ana_myFind_dir.xResults.Count > 0)
                    {
                        if (Ana_myFind_dir.xResults[0].fScore < Ana_paraClass_dir.tolerance)
                        {
                            nDesc = $"-1{ToChangeLanguage($"定位失败,分数")}{Ana_myFind_dir.xResults[0].fScore}";
                            return -1;
                        }
                        else
                        {
                            //nDesc = $"F0定位成功,分数{Ana_myFind_dir.xResults[0].fScore}";
                            nDesc = $"F0{ToChangeLanguage($"定位成功,分数")}{Ana_myFind_dir.xResults[0].fScore}";
                        }
                    }
                    else
                    {
                        //nDesc = $"-2定位失败未找到";
                        nDesc = $"-2{ToChangeLanguage($"定位失败未找到")}";
                        return -2;
                    }

                    #endregion

                    bmp24.Dispose();

                    break;
                default:

                    #region AUFIND

                    Ana_myFind_dir.bmpFind = bmpInput;
                    bOK = Ana_myFind_dir.FindLarge();
                    if (bOK)
                    {
                        if (Ana_myFind_dir.xResults.Count > 0)
                        {
                            if (Ana_myFind_dir.xResults[0].fScore < Ana_paraClass_dir.tolerance)
                            {
                                //nDesc = $"-1定位失败,分数{Ana_myFind_dir.xResults[0].fScore}";
                                nDesc = $"-1{ToChangeLanguage($"定位失败,分数")}{Ana_myFind_dir.xResults[0].fScore}";
                                return -1;
                            }
                            else
                            {
                                //nDesc = $"F0定位成功,分数{Ana_myFind_dir.xResults[0].fScore}";
                                nDesc = $"F0{ToChangeLanguage($"定位成功,分数")}{Ana_myFind_dir.xResults[0].fScore}";
                            }
                        }
                        else
                        {
                            //nDesc = $"-2定位失败未找到";
                            nDesc = $"-2{ToChangeLanguage($"定位失败未找到")}";
                            return -2;
                        }
                    }
                    else
                    {
                        //nDesc = $"-3无法定位";
                        nDesc = $"-3{ToChangeLanguage($"无法定位")}";
                        return -3;
                    }

                    #endregion


                    break;
            }



            return 0;
        }

        #endregion

        #region 预处理计算中心
        JzFindObjectClass m_Find = new JzFindObjectClass();
        /// <summary>
        /// 寻找图片的blob中心
        /// </summary>
        /// <param name="bmpinput">输入图像</param>
        /// <param name="maxrect">返回图像中最大的blob矩形</param>
        /// <returns>返回中心坐标</returns>
        public PointF GetImageMarkCenter(Bitmap bmpinput, bool istrain, out RectangleF maxrect, out Bitmap bmpoutput, out int maxarea)
        {
            PointF ret = new PointF(bmpinput.Width / 2, bmpinput.Height / 2);
            maxrect = new RectangleF(1, 1, bmpinput.Width - 2, bmpinput.Height - 2);
            bmpoutput = new Bitmap(bmpinput);
            maxarea = -1;

            //预处理
            Bitmap bmptemp = new Bitmap(bmpinput);
            AForge.Imaging.Filters.Grayscale grayscale = new AForge.Imaging.Filters.Grayscale(0.299, 0.587, 0.114);
            bmptemp = grayscale.Apply(bmptemp);
            AForge.Imaging.Filters.BrightnessCorrection brightnessCorrection = new AForge.Imaging.Filters.BrightnessCorrection(Brightness);
            bmptemp = brightnessCorrection.Apply(bmptemp);
            AForge.Imaging.Filters.ContrastCorrection contrastCorrection = new AForge.Imaging.Filters.ContrastCorrection(Contrast);
            bmptemp = contrastCorrection.Apply(bmptemp);

            Bitmap bmptempDraw = new Bitmap(bmpinput);

            switch (mFindType)
            {
                case FindType.CIRCLE:

                    #region 找圆的算法

                    try

                    {

                        // CreateInstance

                        VisionDesigner.CircleFind.CCircleFindTool cCircleFindToolObj = new VisionDesigner.CircleFind.CCircleFindTool();

                        // Set input image

                        //VisionDesigner.CMvdImage cInputImg = new CMvdImage();

                        //cInputImg.InitImage("InputTest.bmp");
                        VisionDesigner.CMvdImage cInputImg = BitmapToCMvdImage(bmptemp);
                        if (cInputImg.PixelFormat != MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08)
                        {
                            //当前程序仅支持mono8。因此像素格会转换.
                            cInputImg.ConvertImagePixelFormat(MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08);
                        }
                        //cInputImg2.InitImage("InputTest2.bmp");


                        cCircleFindToolObj.InputImage = cInputImg;

                        // Set ROI region (optional)

                        cCircleFindToolObj.ROI = new VisionDesigner.CMvdRectangleF(cInputImg.Width / 2, cInputImg.Height / 2, cInputImg.Width, cInputImg.Height);

                        // Set basic parameter

                        cCircleFindToolObj.BasicParam.CoarseCenter = new MVD_POINT_I(Convert.ToInt32(cInputImg.Width) / 2, Convert.ToInt32(cInputImg.Height) / 2);

                        cCircleFindToolObj.SetRunParam("EdgePolarity", "Both");
                        cCircleFindToolObj.SetRunParam("EdgeWidth", $"{CircleEdgeWidth}");
                        cCircleFindToolObj.SetRunParam("MinRadius", $"{CircleMinRadius}");
                        cCircleFindToolObj.SetRunParam("MaxRadius", $"{CircleMaxRadius}");
                        cCircleFindToolObj.SetRunParam("RadNum", $"{CircleRadNum}");
                        cCircleFindToolObj.SetRunParam("EdgeThresh", $"{ThresholdValue}");

                        // Running

                        cCircleFindToolObj.Run();

                        // Get the result

                        VisionDesigner.CircleFind.CCircleFindResult cCircleFindRes = cCircleFindToolObj.Result;

                        Console.WriteLine("Recognition status: {0}", cCircleFindRes.Status);

                        if (cCircleFindRes.Status == 1)
                        {
                            ret = new PointF(cCircleFindRes.Circle.Center.fX, cCircleFindRes.Circle.Center.fY);
                            maxrect = new RectangleF(cCircleFindRes.Circle.Center.fX - cCircleFindRes.Circle.Radius,
                                cCircleFindRes.Circle.Center.fY - cCircleFindRes.Circle.Radius, cCircleFindRes.Circle.Radius * 2, cCircleFindRes.Circle.Radius * 2);
                            maxarea = (int)cCircleFindRes.CircleAreaRate;

                            RectangleF rectangleFmarktemp = SimpleRectF(ret, 1, 1);
                            float _lineWidth = 1f;
                            Graphics graphics = Graphics.FromImage(bmptempDraw);
                            graphics.DrawRectangles(new Pen(Color.Lime, _lineWidth), new RectangleF[] { rectangleFmarktemp });
                            graphics.DrawRectangles(new Pen(Color.Red, _lineWidth), new RectangleF[] { maxrect });
                            graphics.DrawEllipse(new Pen(Color.Lime, _lineWidth), maxrect);
                            graphics.DrawString($"Radius:{cCircleFindRes.Circle.Radius}", new Font("宋体", 10f), Brushes.Lime, new PointF(5, 5));
                            graphics.Dispose();

                            if (istrain)
                            {
                                org_width = maxrect.Width;
                                org_height = maxrect.Height;
                                org_area = maxarea;
                            }
                        }

                    }

                    catch (MvdException ex)

                    {

                        Console.WriteLine("Fail with ErrorCode: 0x" + ex.ErrorCode.ToString("X"));

                    }

                    catch (System.Exception ex)

                    {

                        Console.WriteLine("Fail with error " + ex.Message);

                    }


                    #endregion

                    break;
                default:
                    #region 找blob算法

                    //AForge.Imaging.Filters.HistogramEqualization histogramEqualization = new AForge.Imaging.Filters.HistogramEqualization();
                    //bmptemp = histogramEqualization.Apply(bmptemp);

                    m_Find.AH_SetThreshold(ref bmptemp, ThresholdValue);
                    m_Find.AH_FindBlob(bmptemp, IsFindWhite);
                    if (m_Find.FoundList.Count > 0)
                    {
                        int maxindex = m_Find.GetMaxRectIndex();
                        ret = new PointF((float)m_Find.FoundList[maxindex].rotatedRectangleF.fCX,
                                         (float)m_Find.FoundList[maxindex].rotatedRectangleF.fCY);
                        maxrect = new RectangleF(m_Find.rectMaxRect.X, m_Find.rectMaxRect.Y, m_Find.rectMaxRect.Width, m_Find.rectMaxRect.Height);
                        maxarea = m_Find.FoundList[maxindex].Area;

                        RectangleF rectangleFmarktemp = SimpleRectF(ret, 2, 2);
                        float _lineWidth = 11f;
                        Graphics graphics = Graphics.FromImage(bmptempDraw);
                        graphics.DrawRectangles(new Pen(Color.Lime, _lineWidth), new RectangleF[] { rectangleFmarktemp });
                        graphics.DrawRectangles(new Pen(Color.Red, _lineWidth), new RectangleF[] { maxrect });
                        graphics.Dispose();

                        if (istrain)
                        {
                            org_width = maxrect.Width;
                            org_height = maxrect.Height;
                            org_area = m_Find.FoundList[maxindex].Area;
                        }

                    }

                    #endregion
                    break;
            }

            
            bmpoutput.Dispose();
            bmpoutput = new Bitmap(bmptempDraw);
            bmptemp.Dispose();
            bmptempDraw.Dispose();
            return ret;
        }

        void _preImage(ref Bitmap bmptemp)
        {
            //预处理
            //AForge.Imaging.Filters.Grayscale grayscale = new AForge.Imaging.Filters.Grayscale(0.299, 0.587, 0.114);
            //bmptemp = grayscale.Apply(bmptemp);
            ////AForge.Imaging.Filters.BrightnessCorrection brightnessCorrection = new AForge.Imaging.Filters.BrightnessCorrection(Brightness);
            ////bmptemp = brightnessCorrection.Apply(bmptemp);
            ////AForge.Imaging.Filters.ContrastCorrection contrastCorrection = new AForge.Imaging.Filters.ContrastCorrection(Contrast);
            ////bmptemp = contrastCorrection.Apply(bmptemp);
            //AForge.Imaging.Filters.CannyEdgeDetector cannyEdgeDetector = new AForge.Imaging.Filters.CannyEdgeDetector();
            //cannyEdgeDetector.HighThreshold = 100;
            //cannyEdgeDetector.LowThreshold = 20;
            //bmptemp = cannyEdgeDetector.Apply(bmptemp);

            //AForge.Imaging.Filters.Threshold threshold = new AForge.Imaging.Filters.Threshold();
            //bmptemp = threshold.Apply(bmptemp);
        }

        RectangleF SimpleRectF(PointF Pt, int Width, int Height)
        {
            RectangleF rect = SimpleRectF(Pt);
            rect.Inflate(Width, Height);

            return rect;
        }
        RectangleF SimpleRectF(PointF Pt)
        {
            return new RectangleF(Pt.X, Pt.Y, 1, 1);
        }
        /// <summary>
        /// Bitmap 转成 CMvdImage
        /// </summary>
        /// <param name="bmpInputImg"></param>
        /// <returns></returns>
        private CMvdImage BitmapToCMvdImage(Bitmap bmpInputImg)
        {
            CMvdImage cMvdImage = new CMvdImage();
            System.Drawing.Imaging.PixelFormat bitPixelFormat = bmpInputImg.PixelFormat;
            BitmapData bmData = bmpInputImg.LockBits(new Rectangle(0, 0, bmpInputImg.Width, bmpInputImg.Height), ImageLockMode.ReadOnly, bitPixelFormat);//锁定

            if (bitPixelFormat == System.Drawing.Imaging.PixelFormat.Format8bppIndexed)
            {
                Int32 bitmapDataSize = bmData.Stride * bmData.Height;//bitmap图像缓存长度
                int offset = bmData.Stride - bmData.Width;
                Int32 ImageBaseDataSize = bmData.Width * bmData.Height;//imageBaseData_V2图像真正的缓存长度
                byte[] _BitImageBufferBytes = new byte[bitmapDataSize];
                byte[] _ImageBaseDataBufferBytes = new byte[ImageBaseDataSize];
                Marshal.Copy(bmData.Scan0, _BitImageBufferBytes, 0, bitmapDataSize);
                int bitmapIndex = 0;
                int ImageBaseDataIndex = 0;
                for (int i = 0; i < bmData.Height; i++)
                {
                    for (int j = 0; j < bmData.Width; j++)
                    {
                        _ImageBaseDataBufferBytes[ImageBaseDataIndex++] = _BitImageBufferBytes[bitmapIndex++];
                    }
                    bitmapIndex += offset;
                }
                MVD_IMAGE_DATA_INFO stImageData = new MVD_IMAGE_DATA_INFO();
                stImageData.stDataChannel[0].nRowStep = (uint)bmData.Width;
                stImageData.stDataChannel[0].nLen = (uint)ImageBaseDataSize;
                stImageData.stDataChannel[0].nSize = (uint)ImageBaseDataSize;
                stImageData.stDataChannel[0].arrDataBytes = _ImageBaseDataBufferBytes;
                cMvdImage.InitImage((uint)bmData.Width, (uint)bmData.Height, MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08, stImageData);
            }
            else if (bitPixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb)
            {
                Int32 bitmapDataSize = bmData.Stride * bmData.Height;//bitmap图像缓存长度
                int offset = bmData.Stride - bmData.Width * 3;
                Int32 ImageBaseDataSize = bmData.Width * bmData.Height * 3;//imageBaseData_V2图像真正的缓存长度
                byte[] _BitImageBufferBytes = new byte[bitmapDataSize];
                byte[] _ImageBaseDataBufferBytes = new byte[ImageBaseDataSize];
                Marshal.Copy(bmData.Scan0, _BitImageBufferBytes, 0, bitmapDataSize);
                int bitmapIndex = 0;
                int ImageBaseDataIndex = 0;
                for (int i = 0; i < bmData.Height; i++)
                {
                    for (int j = 0; j < bmData.Width; j++)
                    {
                        _ImageBaseDataBufferBytes[ImageBaseDataIndex++] = _BitImageBufferBytes[bitmapIndex + 2];
                        _ImageBaseDataBufferBytes[ImageBaseDataIndex++] = _BitImageBufferBytes[bitmapIndex + 1];
                        _ImageBaseDataBufferBytes[ImageBaseDataIndex++] = _BitImageBufferBytes[bitmapIndex];
                        bitmapIndex += 3;
                    }
                    bitmapIndex += offset;
                }
                MVD_IMAGE_DATA_INFO stImageData = new MVD_IMAGE_DATA_INFO();
                stImageData.stDataChannel[0].nRowStep = (uint)bmData.Width * 3;
                stImageData.stDataChannel[0].nLen = (uint)ImageBaseDataSize;
                stImageData.stDataChannel[0].nSize = (uint)ImageBaseDataSize;
                stImageData.stDataChannel[0].arrDataBytes = _ImageBaseDataBufferBytes;
                cMvdImage.InitImage((uint)bmData.Width, (uint)bmData.Height, MVD_PIXEL_FORMAT.MVD_PIXEL_RGB_RGB24_C3, stImageData);
            }
            bmpInputImg.UnlockBits(bmData);  // 解除锁定
            return cMvdImage;
        }

        #endregion

        private string ToChangeLanguage(string eText)
        {
            string retStr = eText;
            retStr = LanguageExClass.Instance.GetLanguageText(eText);
            return retStr;
        }

    }
}
