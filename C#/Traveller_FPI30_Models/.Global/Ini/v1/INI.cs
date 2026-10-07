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

using JetEazy.PropertyGrid;
using LaserAlignDX;
using System.ComponentModel;
using System.Drawing.Design;
using Traveller106.Ini.V1;
using LangIndex = System.Int32;

namespace Traveller106
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

        #region DTO
        readonly DtoINI _dto = new DtoINI();
        #endregion

        #region PATH_FILES
        string INI_FILE => GlobalConfig.APP_INI_FILE;
        #endregion

        const string XCate1 = "A01.相機設定";
        #region FPI30_INI_CATE_1
        /// <summary>
        /// 飞拍图像解析度 (mm/pixel)
        /// </summary>
        [CategoryAttribute(XCate1), DescriptionAttribute("单位 (mm/pixel)")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 100, 0.1f, 4)]
        [DisplayName("01.飞拍图像解析度")]
        [Browsable(true)]
        public float FlyImageResolution
        {
            get => _dto.FlyImageResolution;
            set => _dto.FlyImageResolution = value;
        }

        /// <summary>
        /// 线扫取像延时 (ms)
        /// </summary>
        [CategoryAttribute(XCate1), DescriptionAttribute("单位(毫秒)")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999)]
        [DisplayName("02.取像延时")]
        [Browsable(true)]
        public int DelayImageTime
        {
            get => _dto.LineScanDelayTime;
            set => _dto.LineScanDelayTime = value;
        }

        /// <summary>
        /// 线扫超时时间 (seconds)
        /// </summary>
        [CategoryAttribute(XCate1), DescriptionAttribute("")]
        [Browsable(false)]
        public int GetImageDelayTime
        {
            get => _dto.LineScanOverTimeSecs;
            //set => _dto.LineScanOverTimeSecs = value;
        }

        /// <summary>
        /// 灯光延时时间 (ms)
        /// </summary>
        [CategoryAttribute(XCate1), DescriptionAttribute("")]
        [Browsable(false)]
        public int LightDelayTime
        {
            get => _dto.LightDelayTime;
            //set => _dto.LightDelayTime = value;
        }
        #endregion

        const string XCate2 = "A02.圖檔保存設定";
        #region FPI30_INI_CATE_2
        /// <summary>
        /// 结果图路径
        /// </summary>
        [CategoryAttribute(XCate2), DescriptionAttribute("")]
        [Editor(typeof(FolderBrowserPropertyEditor), typeof(UITypeEditor))]
        [DisplayName("01.结果图路径")]
        [Browsable(true)]
        public string ResultImagePath
        {
            get => _dto.ResultImagePath;
            set => _dto.ResultImagePath = value;
        }

        /// <summary>
        /// 存储压缩图片
        /// </summary>
        [CategoryAttribute(XCate2), DescriptionAttribute("")]
        [DisplayName("02.存储压缩图片")]
        [Browsable(true)]
        public bool IsSaveDebugBmp
        {
            get => _dto.IsSaveDebugBmp;
            set => _dto.IsSaveDebugBmp = value;
        }

        /// <summary>
        /// 存储原始图片
        /// </summary>
        [CategoryAttribute(XCate2), DescriptionAttribute("")]
        [DisplayName("03.存储原始图片")]
        [Browsable(true)]
        public bool IsSaveDebugOrgBmp
        {
            get => _dto.IsSaveDebugOrgBmp;
            set => _dto.IsSaveDebugOrgBmp = value;
        }

        /// <summary>
        /// 结果图质量
        /// </summary>
        [CategoryAttribute(XCate2), DescriptionAttribute("")]
        [DisplayName("04.结果图质量")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 100)]
        [Browsable(true)]
        public int ImageQuality
        {
            get => _dto.ImageQuality;
            set => _dto.ImageQuality = value;
        }

        /// <summary>
        /// 保存单颗测试图
        /// </summary>
        [CategoryAttribute(XCate2), DescriptionAttribute("")]
        [DisplayName("05.保存单颗测试图")]
        [Browsable(true)]
        public bool IsSaveTestImage
        {
            get => _dto.IsSaveTestImage;
            set => _dto.IsSaveTestImage = value;
        }

        /// <summary>
        /// 圖檔分存OK/NG不同資料夾
        /// </summary>
        [CategoryAttribute(XCate2), DescriptionAttribute("")]
        [DisplayName("06.圖檔分存OK/NG不同資料夾")]
        [Browsable(true)]
        public bool UseOkNgDiffImageFolders
        {
            get => _dto.UseOkNgDiffImageFolders;
            set => _dto.UseOkNgDiffImageFolders = value;
        }

        /// <summary>
        /// FTP上傳設定
        /// </summary>
        [CategoryAttribute(XCate2), DescriptionAttribute("")]
        [DisplayName("07.啟用FTP上傳")]
        [Editor(typeof(FtpUITypeEditor), typeof(UITypeEditor))]
        [Browsable(true)]
        public string UseFtp
        {
            get => _dto.FtpSettings.Enabled.ToString();
            set { }
        }

        /// <summary>
        /// FTP上傳設定
        /// </summary>
        [CategoryAttribute(XCate2), DescriptionAttribute("")]
        [Browsable(false)]
        public DtoFtpSettings FtpSettings => _dto.FtpSettings;
        #endregion

        const string XCate3 = "A03.全域補償設定";
        #region FPI30_INI_CATE_3
        /// <summary>
        /// 线扫补偿X
        /// </summary>
        [CategoryAttribute(XCate3), DescriptionAttribute("")]
        [DisplayName("01.线扫补偿X")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMaxAttribute(-100f, 100f, 0.01f, 3)]
        [Browsable(true)]
        public float Cal_Bcx 
        {
            get => _dto.Cal_Bcx;
            set => _dto.Cal_Bcx = value;
        }

        /// <summary>
        /// 线扫补偿Y
        /// </summary>
        [CategoryAttribute(XCate3), DescriptionAttribute("")]
        [DisplayName("02.线扫补偿Y")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMaxAttribute(-100f, 100f, 0.01f, 3)]
        [Browsable(true)]
        public float Cal_Bcy
        {
            get => _dto.Cal_Bcy;
            set => _dto.Cal_Bcy = value;
        }

        /// <summary>
        /// 线扫补偿角度
        /// </summary>
        [CategoryAttribute(XCate3), DescriptionAttribute("")]
        [DisplayName("03.线扫补偿角度")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMaxAttribute(-5f, 5f, 0.1f, 2)]
        [Browsable(true)]
        public float Cal_Bca
        {
            get => _dto.Cal_Bca;
            set => _dto.Cal_Bca = value;
        }

        /// <summary>
        /// 强制全检
        /// </summary>
        [CategoryAttribute(XCate3), DescriptionAttribute("")]
        [Browsable(false)]
        public bool IsForceInspect
        {
            get => _dto.IsForceInspect;
            private set => _dto.IsForceInspect = value;
        }

        /// <summary>
        /// "優化"
        /// </summary>
        [CategoryAttribute(XCate3), DescriptionAttribute("")]
        [Browsable(false)]
        public bool IsCheat
        {
            get => _dto.IsCheat;
            private set => _dto.IsCheat = value;
        }
        #endregion

        const string XCate4 = "A04.其他GUI設定";
        #region FPI30_INI_CATE_4
        /// <summary>
        /// 是否使用 ToolTip 顯示結果
        /// </summary>
        [CategoryAttribute(XCate4), DescriptionAttribute("")]
        [DisplayName("01.结果显示数据")]
        [Browsable(true)]
        public bool IsResultShowChar 
        { 
            get => _dto.IsResultShowChar;
            set => _dto.IsResultShowChar = value;
        }

        /// <summary>
        /// 飛拍自動停止縮放
        /// </summary>
        [CategoryAttribute(XCate4), DescriptionAttribute("")]
        [DisplayName("02.飛拍自動停止縮放")]
        [Browsable(true)]
        public bool IsAutoDisableZoom
        {
            get => _dto.IsAutoDisableZoom;
            set => _dto.IsAutoDisableZoom = value;
        }
        #endregion

        const string XCate5 = "A05.PLC 設定";
        #region FPI30_INI_CATE_5
        /// <summary>
        /// 使用單一NG碼
        /// </summary>
        [CategoryAttribute(XCate5), DescriptionAttribute("")]
        [DisplayName("01.使用單一NG碼")]
        [Browsable(true)]
        public bool UsingSingleNgCode
        {
            get => _dto.UsingSingleNgCode;
            set => _dto.UsingSingleNgCode = value;
        }

        /// <summary>
        /// 指定NG碼
        /// </summary>
        [CategoryAttribute(XCate5), DescriptionAttribute("")]
        [DisplayName("02.指定NG碼")]
        [Browsable(true)]
        public int SingleNgCode
        {
            get => _dto.SingleNgCode;
            set => _dto.SingleNgCode = value;
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
            _dto.Load(INI_FILE);
        }
        public void Save()
        {
            _dto.Save(INI_FILE);
        }
    }
}
