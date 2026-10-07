#region AUTHOR
/*
 * 
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2026-09-09 重整 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using Eazy_Project_III;
using JetEazy.PropertyGrid;
using LeTian.JxProps;
using System.ComponentModel;
using System.Drawing.Design;

namespace Traveller106.Ini.V2
{
    public class INI
    {
        #region SINGLETON
        private static INI _instance = null;
        #endregion

        public static INI Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new INI();
                return _instance;
            }
        }

        #region JX
        public readonly JxAppSettings AppSettings = new JxAppSettings();
        public static implicit operator JxAppSettings(INI ini)
        {
            return ini?.AppSettings;
        }
        public static implicit operator JxContainer(INI ini)
        {
            return ini?.AppSettings;
        }
        #endregion

        #region PATH_FILES
        string INI_FILE => System.IO.Path.Combine(Universal.MAINPATH, "config.ini");
        string JSON_FILE => System.IO.Path.Combine(Universal.MAINPATH, "config.json");
        #endregion

        #region FPI30_INI_CATE_1
        const string X3_Cat1 = "A01.相機設定";

        /// <summary>
        /// 飞拍图像解析度 (mm/pixel)
        /// </summary>
        [CategoryAttribute(X3_Cat1), DescriptionAttribute("单位 (mm/pixel)")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 100, 0.1f, 4)]
        [DisplayName("01.飞拍图像解析度")]
        [Browsable(true)]
        public float FlyImageResolution
        {
            get => (float)AppSettings.CamSettings.FlyImageRes.Value;
            set => AppSettings.CamSettings.FlyImageRes.Value = (decimal)value;
        }

        /// <summary>
        /// 线扫取像延时 (ms)
        /// </summary>
        [CategoryAttribute(X3_Cat1), DescriptionAttribute("单位(毫秒)")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999)]
        [DisplayName("02.取像延时")]
        [Browsable(true)]
        public int DelayImageTime
        {
            get => AppSettings.CamSettings.LineScanDelayTime.Value;
            set => AppSettings.CamSettings.LineScanDelayTime.Value = value;
        }

        /// <summary>
        /// 线扫超时时间 (seconds)
        /// </summary>
        [CategoryAttribute(X3_Cat1), DescriptionAttribute("")]
        [Browsable(false)]
        public int GetImageDelayTime
        {
            get => AppSettings.CamSettings.LineScanOverTimeSecs.Value;
            set => AppSettings.CamSettings.LineScanDelayTime.Value = value;
        }

        /// <summary>
        /// 灯光延时时间 (ms)
        /// </summary>
        [CategoryAttribute(X3_Cat1), DescriptionAttribute("")]
        [Browsable(false)]
        public int LightDelayTime
        {
            get => AppSettings.CamSettings.LightDelayTime.Value;
            set => AppSettings.CamSettings.LightDelayTime.Value = value;
        }
        #endregion

        #region FPI30_INI_CATE_2
        const string X3_Cat2 = "A02.圖檔保存設定";
        /// <summary>
        /// 结果图路径
        /// </summary>
        [CategoryAttribute(X3_Cat2), DescriptionAttribute("")]
        [Editor(typeof(FolderBrowserPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("01.结果图路径")]
        [Browsable(true)]
        public string ResultImagePath
        {
            get => AppSettings.ImgSaveSettings.ResultImagePath.Value;
            set => AppSettings.ImgSaveSettings.ResultImagePath.Value = value;
        }

        /// <summary>
        /// 存储压缩图片
        /// </summary>
        [CategoryAttribute(X3_Cat2), DescriptionAttribute("")]
        [DisplayName("02.存储压缩图片")]
        [Browsable(true)]
        public bool IsSaveDebugBmp
        {
            get => AppSettings.ImgSaveSettings.IsSaveDebugBmp.Value;
            set => AppSettings.ImgSaveSettings.IsSaveDebugBmp.Value = value;
        }

        /// <summary>
        /// 存储原始图片
        /// </summary>
        [CategoryAttribute(X3_Cat2), DescriptionAttribute("")]
        [DisplayName("03.存储原始图片")]
        [Browsable(true)]
        public bool IsSaveDebugOrgBmp
        {
            get => AppSettings.ImgSaveSettings.IsSaveDebugOrgBmp.Value;
            set => AppSettings.ImgSaveSettings.IsSaveDebugOrgBmp.Value = value;
        }

        /// <summary>
        /// 结果图质量
        /// </summary>
        [CategoryAttribute(X3_Cat2), DescriptionAttribute("")]
        [DisplayName("04.结果图质量")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 100)]
        [Browsable(true)]
        public int ImageQuality
        {
            get => AppSettings.ImgSaveSettings.ImageQuality.Value;
            set => AppSettings.ImgSaveSettings.ImageQuality.Value = value;
        }

        /// <summary>
        /// 保存单颗测试图
        /// </summary>
        [CategoryAttribute(X3_Cat2), DescriptionAttribute("")]
        [DisplayName("05.保存单颗测试图")]
        [Browsable(true)]
        public bool IsSaveTestImage
        {
            get => AppSettings.ImgSaveSettings.IsSaveTestImage.Value;
            set => AppSettings.ImgSaveSettings.IsSaveTestImage.Value = value;
        }

        /// <summary>
        /// 圖檔分存OK/NG不同資料夾
        /// </summary>
        [CategoryAttribute(X3_Cat2), DescriptionAttribute("")]
        [DisplayName("06.圖檔分存OK/NG不同資料夾")]
        [Browsable(true)]
        public bool UseOkNgDiffImageFolders
        {
            get => AppSettings.ImgSaveSettings.UseOkNgDiffImageFolders.Value;
            set => AppSettings.ImgSaveSettings.UseOkNgDiffImageFolders.Value = value;
        }
        #endregion

        #region FPI30_INI_CATE_3
        const string X3_Cat3 = "A03.全域補償設定";

        /// <summary>
        /// 强制全检
        /// </summary>
        [CategoryAttribute(X3_Cat2), DescriptionAttribute("")]
        [Browsable(false)]
        public bool IsForceInspect
        {
            get => AppSettings.GlobalCompensation.IsForceInspect;
            private set => AppSettings.GlobalCompensation.IsForceInspect.Value = value;
        }

        /// <summary>
        /// 线扫补偿X
        /// </summary>
        [CategoryAttribute(X3_Cat3), DescriptionAttribute("")]
        [DisplayName("01.线扫补偿X")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMaxAttribute(-100f, 100f, 0.01f, 3)]
        [Browsable(true)]
        public float Cal_Bcx 
        {
            get => (float)AppSettings.GlobalCompensation.Cal_Bcx.Value;
            set => AppSettings.GlobalCompensation.Cal_Bcx.Value = (decimal)value;
        }

        /// <summary>
        /// 线扫补偿Y
        /// </summary>
        [CategoryAttribute(X3_Cat3), DescriptionAttribute("")]
        [DisplayName("02.线扫补偿Y")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMaxAttribute(-100f, 100f, 0.01f, 3)]
        [Browsable(true)]
        public float Cal_Bcy
        {
            get => (float)AppSettings.GlobalCompensation.Cal_Bcy.Value;
            set => AppSettings.GlobalCompensation.Cal_Bcy.Value = (decimal)value;
        }

        /// <summary>
        /// 线扫补偿角度
        /// </summary>
        [CategoryAttribute(X3_Cat3), DescriptionAttribute("")]
        [DisplayName("03.线扫补偿角度")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMaxAttribute(-5f, 5f, 0.1f, 2)]
        [Browsable(true)]
        public float Cal_Bca
        {
            get => (float)AppSettings.GlobalCompensation.Cal_Bca.Value;
            set => AppSettings.GlobalCompensation.Cal_Bca.Value = (decimal)value;
        }
        #endregion

        #region FPI30_INI_CATE_4
        const string X3_Cat4 = "A04.其他設定";

        /// <summary>
        /// 结果显示数据
        /// </summary>
        [CategoryAttribute(X3_Cat4), DescriptionAttribute("")]
        [DisplayName("01.结果显示数据")]
        [Browsable(true)]
        public bool IsResultShowChar 
        { 
            get => AppSettings.MiscSettings.IsResultShowChar.Value;
            set => AppSettings.MiscSettings.IsResultShowChar.Value = value;
        }

        /// <summary>
        /// 飛拍自動停止縮放
        /// </summary>
        [CategoryAttribute(X3_Cat4), DescriptionAttribute("")]
        [DisplayName("02.飛拍自動停止縮放")]
        [Browsable(true)]
        public bool IsAutoDisableZoom
        {
            get => AppSettings.MiscSettings.IsAutoDisableZoom.Value;
            set => AppSettings.MiscSettings.IsAutoDisableZoom.Value = value;
        }

        /// <summary>
        /// 增强抓边
        /// </summary>
        [CategoryAttribute(X3_Cat4), DescriptionAttribute("")]
        [DisplayName("03.增强抓边")]
        [Browsable(false)]
        public bool IsCheat
        {
            get => AppSettings.MiscSettings.IsCheat.Value;
            private set => AppSettings.MiscSettings.IsCheat.Value = value;
        }

        #endregion

        #region FPI30_INI_CATE_5
        const string X3_Cat5 = "A05.PLC 設定";

        /// <summary>
        /// 使用單一NG碼
        /// </summary>
        [CategoryAttribute(X3_Cat5), DescriptionAttribute("")]
        [DisplayName("01.使用單一NG碼")]
        [Browsable(true)]
        public bool UsingSingleNgCode
        {
            get => AppSettings.PlcCodeSettings.UsingSingleNgCode.Value;
            set => AppSettings.PlcCodeSettings.UsingSingleNgCode.Value = value;
        }

        /// <summary>
        /// 指定NG碼
        /// </summary>
        [CategoryAttribute(X3_Cat5), DescriptionAttribute("")]
        [DisplayName("02.指定NG碼")]
        [Browsable(true)]
        public int SingleNgCode
        {
            get => AppSettings.PlcCodeSettings.SingleNgCode.Value;
            set => AppSettings.PlcCodeSettings.SingleNgCode.Value = value;
        }
        #endregion

        #region DUMMY_MEMBERS_FOR_LEGACY_PROJECTS_其他專案_餘孽
        [Browsable(false)]
        public int LANGUAGE { get; set; } = 0;
        [Browsable(false)]
        public LangIndex mLangIndex { get; set; } = 0;
        [Browsable(false)]
        public string HistoryDataPath = string.Empty;
        [Browsable(false)]
        public string HistoryDataBarcode = string.Empty;
        [Browsable(false)]
        public int FactoryNameIndex { get; set; } = 0;
        [Browsable(false)]
        public int AutoLogoutTime { get; set; } = 30;
        [Browsable(false)]
        public int LedControlCount { get; set; } = 1;
        public void LoadIniSetup()
        {
            //m_xprops.Clear();
            //string cat0 = "inicat0";
            //AddProperty(cat0, "UseOkNgDiffImageFolders", "UseOkNgDiffImageFolders", UseOkNgDiffImageFolders, "");
            //AddProperty(cat0, "IsAutoTestSavePicture", "IsAutoTestSavePicture", IsSaveTestImage, "");
            //AddProperty(cat0, "IsSaveResultImage", "IsSaveResultImage", IsSaveStripImage, "");
            //AddProperty(cat0, "ResultImagePath", "ResultImagePath", ResultImagePath, "");
            //AddProperty(cat0, "ImageQuality", "ImageQuality", ImageQuality, "");
            //AddProperty(cat0, "IsSaveDebugBMP", "IsSaveDebugBMP", IsSaveDebugBmp, "");
            //AddProperty(cat0, "LaserSharePath", "LaserSharePath", LaserSharePath, "");
            //AddProperty(cat0, "IsOpenThread", "IsOpenThread", IsOpenThread, "");
            //AddProperty(cat0, "IsOpenDrawNumber", "IsOpenDrawNumber", IsOpenDrawNumber, "");
            //AddProperty(cat0, "GetImageDelayTime", "GetImageDelayTime", GetImageDelayTime, "");
            //AddProperty(cat0, "LightDelayTime", "LightDelayTime", LightDelayTime, "");
            //AddProperty(cat0, "IsOpenShowSize", "IsOpenShowSize", IsOpenShowSize, "");
            //AddProperty(cat0, "ShowSizeValue", "ShowSizeValue", ShowSizeValue, "");
            //AddProperty(cat0, "Cal_Bcx", "Cal_Bcx", Cal_Bcx, "");
            //AddProperty(cat0, "Cal_Bcy", "Cal_Bcy", Cal_Bcy, "");
            //AddProperty(cat0, "Cal_Bca", "Cal_Bca", Cal_Bca, "");
            //AddProperty(cat0, "IsOpenPrintFirst", "IsOpenPrintFirst", IsOpenPrintFirst, "");
            //AddProperty(cat0, "DIRRES_X", "DIRRES_X", DIRRES_X, "");
            //AddProperty(cat0, "DIRRES_Y", "DIRRES_Y", DIRRES_Y, "");
            //AddProperty(cat0, "IsUseFixedMark", "IsUseFixedMark", IsUseFixedMark, "");
            //AddProperty(cat0, "GC_Angle", "GC_Angle", GC_Angle, "");
            //AddProperty(cat0, "LedControlCount", "LedControlCount", LedControlCount, "");
            //AddProperty(cat0, "mLangIndex", "mLangIndex", mLangIndex, "ChangeLangIndex");
            //AddProperty(cat0, "AutoLogoutTime", "AutoLogoutTime", AutoLogoutTime, "");
            //AddProperty(cat0, "IsForceInspect", "IsForceInspect", IsForceInspect, "");
            //AddProperty(cat0, "Cal_BcxLB", "Cal_BcxLB", Cal_BcxLB, "");
            //AddProperty(cat0, "Cal_BcyLB", "Cal_BcyLB", Cal_BcyLB, "");
            //AddProperty(cat0, "Cal_BcaLB", "Cal_BcaLB", Cal_BcaLB, "");
            //AddProperty(cat0, "DrawFontSize", "DrawFontSize", DrawFontSize, "");
            //AddProperty(cat0, "DrawLineWidth", "DrawLineWidth", DrawLineWidth, "");
            //AddProperty(cat0, "IsOpenUpload", "IsOpenUpload", IsOpenUpload, "");

            //string cat1 = "inicat1";
            //AddProperty(cat1, "tcp_ip", "tcp_ip", tcp_ip, "");
            //AddProperty(cat1, "tcp_port", "tcp_port", tcp_port, "");

            //string cat2 = "inicat2";
            //AddProperty(cat2, "IsOnlyUseLeft", "IsOnlyUseLeft", IsOnlyUseLeft, "");
            //AddProperty(cat2, "IsUseStandBoard", "IsUseStandBoard", IsUseStandBoard, "");
            //AddProperty(cat2, "L1Path", "L1Path", L1Path, "");
            //AddProperty(cat2, "L2Path", "L2Path", L2Path, "");
            //AddProperty(cat2, "BoundaryValue", "BoundaryValue", BoundaryValue, "");
            //AddProperty(cat2, "L3Path", "L3Path", L3Path, "");
            //AddProperty(cat2, "L4Path", "L4Path", L4Path, "");

        }
        #endregion

        public void Initial()
        {
            Load();
        }
        public void Load()
        {
            string jsonFile = JSON_FILE;
            if (System.IO.File.Exists(jsonFile))
            {
                AppSettings.Load(jsonFile);
            }
            else
            {
                migrationLoad();
            }
        }
        public void Save()
        {
            string jsonFile = JSON_FILE;
            AppSettings.Save(jsonFile);
        }

        #region MIGRATIONS
        void migrationLoad()
        {
            var old = new Traveller106.Ini.V1.DtoINI();
            old.Load(INI_FILE);

            this.FlyImageResolution = old.FlyImageResolution;
            this.DelayImageTime = old.LineScanDelayTime;
            this.GetImageDelayTime = old.LineScanOverTimeSecs;
            this.LightDelayTime = old.LightDelayTime;

            this.ResultImagePath = old.ResultImagePath;
            this.IsSaveDebugBmp = old.IsSaveDebugBmp;
            this.IsSaveDebugOrgBmp = old.IsSaveDebugOrgBmp;
            this.IsSaveTestImage = old.IsSaveTestImage;
            this.ImageQuality = (int)old.ImageQuality;
            this.UseOkNgDiffImageFolders = old.UseOkNgDiffImageFolders;

            this.Cal_Bcx = old.Cal_Bcx;
            this.Cal_Bcy = old.Cal_Bcy;
            this.Cal_Bca = old.Cal_Bca;

            this.IsForceInspect = old.IsForceInspect;
            this.IsResultShowChar = old.IsResultShowChar;
            this.IsAutoDisableZoom = old.IsAutoDisableZoom;

            this.UsingSingleNgCode = old.UsingSingleNgCode;
            this.SingleNgCode = old.SingleNgCode;
            this.IsCheat = old.IsCheat;
        }
        #endregion
    }
}
