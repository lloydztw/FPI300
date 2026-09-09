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

using LeTian.JxProps;

namespace LaserAlignDX.App
{
    public class JxAppSettings : JxContainer
    {
        public JxAppCamSettings CamSettings = new JxAppCamSettings();
        public JxAppImgSaveSettings ImgSaveSettings = new JxAppImgSaveSettings();
        public JxGlobalCompensation GlobalCompensation = new JxGlobalCompensation();
        public JxAppMiscSettings MiscSettings = new JxAppMiscSettings();
        public JxPlcCodeSettings PlcCodeSettings = new JxPlcCodeSettings();

        public JxAppSettings() : base("FPI30 App Settings")
        {
        }
        public override void OnBindingSubItems()
        {
            BindItems(new IProp[]
            {
                CamSettings,
                ImgSaveSettings,
                GlobalCompensation,
                MiscSettings,
                PlcCodeSettings
            });
            base.OnBindingSubItems();
        }
    }


    public class JxAppCamSettings : JxContainer
    {
        const string _DESC = "A01.相機設定";

        #region COMMON_RES_RANGE
        static readonly Range RES_RANGE = new Range(0, 100, 0.1m, 4);
        #endregion

        public JxNumber ImageRes = new JxNumber("ImageRes", "图像解析度 (Hidden)", 0.0134m, RES_RANGE);
        public JxNumber ImageResX = new JxNumber("ImageResX", "图像X方向精度 (Hidden)", 0.00730m, RES_RANGE);
        public JxNumber ImageResY = new JxNumber("ImageResY", "图像Y方向精度 (Hidden)", 0.00715m, RES_RANGE);

        public JxNumber FlyImageRes = new JxNumber("FlyImageRes", "飞拍图像解析度 (mm/pixel)", 0.034m, RES_RANGE);
        public JxInt LineScanDelayTime = new JxInt("LineScanDelayTime", "线扫取像延时(ms)", 1000, new Range(0, 9 * 1000 * 1000));
        public JxInt LineScanOverTimeSecs = new JxInt("LineScanOverTime", "线扫超时时间(s) (Hidden)", 20, new Range(0, 200));
        public JxInt LightDelayTime = new JxInt("LightDelayTime", "灯光延时时间(ms) (Hidden)", 500, new Range(0, 9 * 1000 * 1000));

        public JxAppCamSettings() : base("CamSettings", _DESC)
        {
        }
        public override void OnBindingSubItems()
        {
            this.BindItems(new IProp[]
            {
                ImageRes,
                ImageResX,
                ImageResY,
                FlyImageRes,
                LineScanDelayTime,
                LineScanOverTimeSecs,
                LightDelayTime,
            });
            base.OnBindingSubItems();
        }
    }


    public class JxAppImgSaveSettings : JxContainer
    {
        const string _DESC = "A02.圖檔保存設定";

        /// <summary>
        /// 结果图路径
        /// </summary>
        public JxPathFile ResultImagePath = new JxPathFile("ResultImagePath", "D:\\01FPI30ImagePath", "结果图路径", true);

        /// <summary>
        /// 存储压缩图片
        /// </summary>
        public JxBool IsSaveDebugBmp = new JxBool("IsSaveDebugBmp", false, "存储压缩图片");

        /// <summary>
        /// 存储原始图片
        /// </summary>
        public JxBool IsSaveDebugOrgBmp = new JxBool("IsSaveDebugOrgBmp", false, "存储原始图片");

        /// <summary>
        /// 结果图质量
        /// </summary>
        public JxInt ImageQuality = JxInt.I100("ImageQuality", "结果图质量", 10);

        /// <summary>
        /// 保存单颗测试图
        /// </summary>
        public JxBool IsSaveTestImage = new JxBool("IsSaveTestImage", false, "保存单颗测试图");

        /// <summary>
        /// 圖檔分存OK/NG不同資料夾
        /// </summary>
        public JxBool UseOkNgDiffImageFolders = new JxBool("UseOkNgDiffImageFolders", false, "圖檔分存OK/NG不同資料夾");

        public JxAppImgSaveSettings() : base("ImgSaveSettings", _DESC)
        {
        }

        public override void OnBindingSubItems()
        {
            this.BindItems(new IProp[]
            {
                ResultImagePath,
                IsSaveDebugBmp,
                IsSaveDebugOrgBmp,
                ImageQuality,
                IsSaveTestImage,
                UseOkNgDiffImageFolders
            });
            base.OnBindingSubItems();
        }
    }


    public class JxGlobalCompensation : JxContainer
    {
        const string _DESC = "A03.全域補償設定";

        #region COMMON_MM_RANGE
        static readonly Range _RANGE = new Range(-1000, 1000, 0.01m, 3);
        #endregion

        //[CategoryAttribute(X3_Cat3), DescriptionAttribute("true开 false关")]
        ////[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        //[DisplayName("001.强制全检")]
        //[Browsable(false)]
        public JxBool IsForceInspect = new JxBool("ForceInspect", "强制全检 (Hidden)", true);

        //[CategoryAttribute(X3_Cat3), DescriptionAttribute("")]
        ////[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        //[DisplayName("01.线扫补偿X")]
        //[Browsable(true)]
        public JxNumber Cal_Bcx = new JxNumber("C_X", "线扫补偿X", 0m, _RANGE);

        //[CategoryAttribute(X3_Cat3), DescriptionAttribute("")]
        ////[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        //[DisplayName("02.线扫补偿Y")]
        //[Browsable(true)]
        public JxNumber Cal_Bcy = new JxNumber("C_Y", "线扫补偿Y", 0m, _RANGE);

        //[CategoryAttribute(X3_Cat3), DescriptionAttribute("")]
        ////[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        //[DisplayName("03.线扫补偿角度")]
        //[Browsable(true)]
        public JxNumber Cal_Bca = new JxNumber("C_Angle", "线扫补偿角度", 0m, new Range(-5m, 5m, 0.01m, 2));

        public JxGlobalCompensation() : base("Compensation", _DESC)
        {
        }

        public override void OnBindingSubItems()
        {
            this.BindItems(new IProp[]
            {
                IsForceInspect,
                Cal_Bcx,
                Cal_Bcy,
                Cal_Bca,
            });
            base.OnBindingSubItems();
        }
    }


    public class JxAppMiscSettings : JxContainer
    {
        const string _DESC = "A04.其他設定";

        //[CategoryAttribute(X3_Cat4), DescriptionAttribute("")]
        ////[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        //[DisplayName("01.结果显示数据")]
        //[Browsable(true)]
        public JxBool IsResultShowChar = new JxBool("ShowResult", "结果显示数据", false);

        //[CategoryAttribute(X3_Cat4), DescriptionAttribute("")]
        ////[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        //[DisplayName("02.飛拍自動停止縮放")]
        //[Browsable(true)]
        public JxBool IsAutoDisableZoom = new JxBool("AutoDisableZoom", "飛拍自動停止縮放", false);

        //[CategoryAttribute(X3_Cat4), DescriptionAttribute("")]
        ////[Editor(typeof(GetPositionPropertyEditor), typeof(UITypeEditor))]
        //[DisplayName("03.增强抓边")]
        //[Browsable(false)]
        public JxBool IsCheat = new JxBool("IsCheat", "增强抓边 (Hidden)", false);

        public JxAppMiscSettings() : base("AppMiscSettings", _DESC)
        {
        }

        public override void OnBindingSubItems()
        {
            this.BindItems(new IProp[]
            {
                IsResultShowChar,
                IsAutoDisableZoom,
                IsCheat,
            });
            base.OnBindingSubItems();
        }
    }


    public class JxPlcCodeSettings : JxContainer
    {
        const string _DESC = "A05.PLC 設定";

        //[CategoryAttribute(X3_Cat5), DescriptionAttribute("")]
        //[DisplayName("01.使用單一NG碼")]
        //[Browsable(true)]
        public JxBool UsingSingleNgCode = new JxBool("UsingSingleNgCode", "使用單一NG碼", false);

        //[CategoryAttribute(X3_Cat5), DescriptionAttribute("")]
        //[DisplayName("02.指定NG碼")]
        //[Browsable(true)]
        public JxInt SingleNgCode = JxInt.I100("SingleNgCode", "指定NG碼", 2);

        public JxPlcCodeSettings() : base("PlcSettings", _DESC)
        {
        }

        public override void OnBindingSubItems()
        {
            this.BindItems(new IProp[]
            {
                UsingSingleNgCode,
                SingleNgCode,
            });
            base.OnBindingSubItems();
        }
    }
}

