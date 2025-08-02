using Common.RecipeSpace;
using JetEazy;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.RecipeSpace
{
    //public struct NleXYZ
    //{
    //    public double X;
    //    public double Y;
    //    public double Z;
    //}
    //public class NeedleListPosition
    //{
    //    public string Name { get; set; } = string.Empty;
    //    //public List<NeedleXYZ> NeedleXYZs = new List<NeedleXYZ>();
    //}
    public class NeedleXYZ
    {
        const string _format = "0.0000";

        private double _x = 0;
        private double _y = 0;
        private double _z = 0;

        public NeedleXYZ()
        {

        }
        public NeedleXYZ(string Str)
        {
            FromString(Str);
        }
        public NeedleXYZ(double x, double y, double z)
        {
            _x = x;
            _y = y;
            _z = z;
        }
        public double X { get { return _x; } set { _x = value; } }
        public double Y { get { return _y; } set { _y = value; } }
        public double Z { get { return _z; } set { _z = value; } }
        public void FromString(string str)
        {
            string[] parts = str.Split(',');
            _x = double.Parse(parts[0]);
            _y = double.Parse(parts[1]);
            _z = double.Parse(parts[2]);
        }
        public override string ToString()
        {
            string str = "";
            str += _x.ToString(_format) + ",";
            str += _y.ToString(_format) + ",";
            str += _z.ToString(_format);
            return str;
        }
    }

    #region
#if NEEDLE_USE
    public class RecipeNeedleClass : RecipeBaseClass
    {
        protected RecipeNeedleClass()
        {

        }
        private static RecipeNeedleClass _instance = null;
        public static RecipeNeedleClass Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new RecipeNeedleClass();
                return _instance;
            }
        }

        //粗定位坐标集合

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
        [Browsable(true)]
        public double BaseHeightZ { get; set; } = 0;

        //相机曝光和增益
        [CategoryAttribute(_Cat1), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("相机曝光集合")]
        [Browsable(true)]
        public string sExposureList { get; set; } = "";
        public string[] ExposureArray = new string[8];
        //public List<string> ExposureList = new List<string>();

        [CategoryAttribute(_Cat1), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("相机增益集合")]
        [Browsable(true)]
        public string sGainList { get; set; } = "";
        public string[] GainArray = new string[8];
        //public List<string> GainList = new List<string>();


        //对焦范围 分上限和下限
        [CategoryAttribute(_Cat1), DescriptionAttribute("即初始高度正方向的上下限分为正负对称距离")]
        [DisplayName("对焦上下限")]
        [Browsable(true)]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 1, 0.01f, 3)]
        public double FocusUpperAndLower { get; set; } = 0.2;

        //[CategoryAttribute(_Cat1), DescriptionAttribute("即初始高度正方向的上限")]
        //[DisplayName("对焦上限")]
        //[Browsable(true)]
        //[TypeConverter(typeof(NumericUpDownTypeConverter))]
        //[Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 1, 0.01f, 3)]
        //public double FocusUpper { get; set; } = 0.2;


        public override void Load()
        {
            int i = 0;

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

            CoarsePositioningClass.Instance.Initial(Path, Index, "CoarsePosition.ini");
            ModelPositioningClass.Instance.Initial(Path, Index, "ModelPosition.ini");


        }
        public override void Save()
        {
            WriteINIValue("Basic Control", "BaseHeightZ", BaseHeightZ.ToString(Format), INIFILE);
            WriteINIValue("Basic Control", "FocusUpperAndLower", FocusUpperAndLower.ToString(Format), INIFILE);
            sExposureList = arrayToString(ExposureArray);
            WriteINIValue("Cam Control", "sExposureList", sExposureList.ToString(), INIFILE);
            sGainList = arrayToString(GainArray);
            WriteINIValue("Cam Control", "sGainList", sGainList.ToString(), INIFILE);

            CoarsePositioningClass.Instance.Save();
            ModelPositioningClass.Instance.Save();

        }

        public float GetCamExpo(int camid)
        {
            if (camid >= ExposureArray.Length)
                return 1000;

            float camExpo = 1000;
            float.TryParse(ExposureArray[camid].ToString(), out camExpo);

            return camExpo;
        }
        public void SetCamExpo(int camid,float expo)
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
    }
#endif
    #endregion
    public class CoarsePositioningClass : RecipeBaseClass
    {
        public CoarsePositioningClass()
        {

        }
        private static CoarsePositioningClass _instance = null;
        public static CoarsePositioningClass Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new CoarsePositioningClass();
                return _instance;
            }
        }

        const string _Cat1 = "A01.基础设置";
        [CategoryAttribute(_Cat1), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("特征定位坐标集合")]
        [Browsable(true)]
        public string sCoarsePosList { get; set; } = "";
        public List<string> CoarsePosList = new List<string>();

        public override void Load(bool eCancel = false)
        {
            string str = string.Empty;
            ReadData(ref str, INIFILE);
            sCoarsePosList = str;
            CoarsePosList = sCoarsePosList.Split(';').ToList();
        }
        public override void Save()
        {
            SaveData(sCoarsePosList, INIFILE);
        }


    }
    public class ModelPositioningClass : RecipeBaseClass
    {
        public ModelPositioningClass()
        {

        }
        private static ModelPositioningClass _instance = null;
        public static ModelPositioningClass Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ModelPositioningClass();
                return _instance;
            }
        }
        const string _Cat1 = "A01.基础设置";
        [CategoryAttribute(_Cat1), DescriptionAttribute("")]
        //[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("定位坐标集合")]
        [Browsable(true)]
        public string sModelPosList { get; set; } = "";
        public List<string> ModelPosList = new List<string>();

        public override void Load(bool eCancel = false)
        {
            string str = string.Empty;
            ReadData(ref str, INIFILE);
            sModelPosList = str;
            ModelPosList = sModelPosList.Split(';').ToList();
        }
        public override void Save()
        {
            SaveData(sModelPosList, INIFILE);
        }


    }
}
