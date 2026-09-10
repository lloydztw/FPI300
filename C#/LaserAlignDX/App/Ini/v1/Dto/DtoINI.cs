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

using JetEazy.DTO;
using System.Windows.Forms;

namespace Traveller106.Ini.V1
{
    internal class DtoINI : DtoBase
    {
        #region VERSION_TAGS
        static string APP_VERSION_DATE => LaserAlignDX.GlobalConfig.VersionDate;
        static string APP_VERSION => Application.ProductVersion;
        static string INI_VERSION => "v1";
        #endregion

        #region CATE_1_CAMERA_SETTINGS
#if (OPT_NOT_USED)
        /// <summary>
        /// 图像解析度
        /// </summary>
        public float ImageRes =    0.01340f;       

        /// <summary>
        /// 图像X方向精度
        /// </summary>
        public float ImageResX =   0.00730f;       

        /// <summary>
        /// 图像Y方向精度
        /// </summary>
        public float ImageResY =   0.00715f;
#endif
        /// <summary>
        /// 飞拍图像解析度
        /// </summary>
        public float FlyImageResolution = 0.03400f;

        /// <summary>
        /// 线扫取像延时(ms)
        /// </summary>
        public int LineScanDelayTime = 1000;

        /// <summary>
        /// 线扫超时时间(secs) (根據舊代碼, 此值為 constant) 
        /// </summary>
        public readonly int LineScanOverTimeSecs = 20;

        /// <summary>
        /// 灯光延时时间(ms) (根據舊代碼, 此值為 constant)
        /// </summary>
        public readonly int LightDelayTime = 500;
        #endregion

        #region CATE_2_IMG_SAVING
        /// <summary>
        /// 结果图路径
        /// </summary>
        public string ResultImagePath = "D:\\01FPI30ImagePath";

        /// <summary>
        /// 存储压缩图片
        /// </summary>
        public bool IsSaveDebugBmp = false;

        /// <summary>
        /// 存储原始图片
        /// </summary>
        public bool IsSaveDebugOrgBmp = false;

        /// <summary>
        /// 结果图质量
        /// </summary>
        public int ImageQuality = 10;

        /// <summary>
        /// 保存单颗测试图
        /// </summary>
        public bool IsSaveTestImage = false;

        /// <summary>
        /// 圖檔分存OK/NG不同資料夾
        /// </summary>
        public bool UseOkNgDiffImageFolders = false;

        /// <summary>
        /// 使用 FTP 上傳設定
        /// </summary>
        public readonly DtoFtpSettings FtpSettings = new DtoFtpSettings();
        #endregion

        #region CATE_3_COMPENSATION
        /// <summary>
        /// 线扫补偿Y
        /// </summary>
        public float Cal_Bcx = 0;

        /// <summary>
        /// 线扫补偿Y
        /// </summary>
        public float Cal_Bcy = 0;

        /// <summary>
        /// 线扫补偿角度
        /// </summary>
        public float Cal_Bca = 0;

        /// <summary>
        /// 是否使用平均邊隙 (從8個獨立數值 變成 4個有效數值)
        /// </summary>
        public bool UseAveGaps4 = true;

        /// <summary>
        /// 强制全检
        /// </summary>
        public bool IsForceInspect = false;

        /// <summary>
        /// 增强抓边 (目前 always false)
        /// </summary>
        public bool IsCheat = false;
        #endregion

        #region CATE_4_GUI_SETTINGS
        /// <summary>
        /// 是否使用 ToolTip 顯示結果
        /// </summary>
        public bool IsResultShowChar = false;

        /// <summary>
        /// 飛拍自動停止縮放
        /// </summary>
        public bool IsAutoDisableZoom = false;
        #endregion

        #region CATE_5_PLC_CODE
        /// <summary>
        /// 使用單一NG碼
        /// </summary>
        public bool UsingSingleNgCode = false;

        /// <summary>
        /// 指定NG碼
        /// </summary>
        public int SingleNgCode = 2;
        #endregion

        public override void Load(string iniFile)
        {
            // CATE_1
            //Read(iniFile, "Basic", "ImageResolution", ImageResolution, out ImageResolution);
            //Read(iniFile, "Basic", "ImageResolutionX", ImageResolutionX, out ImageResolutionX);
            //Read(iniFile, "Basic", "ImageResolutionY", ImageResolutionY, out ImageResolutionY);
            Read(iniFile, "Basic", "FlyImageResolution", FlyImageResolution, out FlyImageResolution);
            Read(iniFile, "Basic", "DelayImageTime", LineScanDelayTime, out LineScanDelayTime);

            // CATE_2
            Read(iniFile, "Basic", "ResultImagePath", ResultImagePath, out ResultImagePath);
            Read(iniFile, "Basic", "IsSaveTestImage", IsSaveTestImage, out IsSaveTestImage);
            Read(iniFile, "Basic", "IsSaveDebugBMP", IsSaveDebugBmp, out IsSaveDebugBmp);
            Read(iniFile, "Basic", "IsSaveDebugOrgBmp", IsSaveDebugOrgBmp, out IsSaveDebugOrgBmp);
            Read(iniFile, "Basic", "UseOkNgDiffImageFolders", UseOkNgDiffImageFolders, out UseOkNgDiffImageFolders);
            Read(iniFile, "Basic", "ImageQuality", ImageQuality, out ImageQuality);

            // CATE_3
            Read(iniFile, "Basic", "Cal_Bcx", Cal_Bcx, out Cal_Bcx);
            Read(iniFile, "Basic", "Cal_Bcy", Cal_Bcy, out Cal_Bcy);
            Read(iniFile, "Basic", "Cal_Bca", Cal_Bcy, out Cal_Bca);
            Read(iniFile, "Basic", "UseAveGaps4", UseAveGaps4, out UseAveGaps4);
            Read(iniFile, "Basic", "IsForceInspect", IsForceInspect, out IsForceInspect);
            Read(iniFile, "Basic", "Optimization", false, out IsCheat);

            // CATE_4
            Read(iniFile, "Basic", "IsAutoDisableZoom", IsAutoDisableZoom, out IsAutoDisableZoom);
            Read(iniFile, "Basic", "IsResultShowChar", IsResultShowChar, out IsResultShowChar);

            // CATE_5
            Read(iniFile, "Basic", "UsingSingleNgCode", UsingSingleNgCode, out UsingSingleNgCode);
            Read(iniFile, "Basic", "SingleNgCode", SingleNgCode, out SingleNgCode);

            // FTP
            FtpSettings.Load(iniFile);
        }
        public override void Save(string iniFile)
        {
            // Check In Version
            CheckInVersion(iniFile);

            // CATE_1
            //WriteINIValue("Basic", "FlyImageResolution", FlyImageResolution.ToString(), INIFILE);
            //WriteINIValue("Basic", "DelayImageTime", DelayImageTime.ToString(), INIFILE);
            Write(iniFile, "Basic", "FlyImageResolution", FlyImageResolution);
            Write(iniFile, "Basic", "DelayImageTime", LineScanDelayTime);
            
            // CATE_2
            Write(iniFile, "Basic", "ResultImagePath", ResultImagePath);
            Write(iniFile, "Basic", "IsSaveTestImage", IsSaveTestImage);
            Write(iniFile, "Basic", "IsSaveDebugBMP", IsSaveDebugBmp);
            Write(iniFile, "Basic", "IsSaveDebugOrgBmp", IsSaveDebugOrgBmp);
            Write(iniFile, "Basic", "UseOkNgDiffImageFolders", UseOkNgDiffImageFolders);
            Write(iniFile, "Basic", "ImageQuality", ImageQuality);

            // CATE_3
            Write(iniFile, "Basic", "Cal_Bcx", Cal_Bcx);
            Write(iniFile, "Basic", "Cal_Bcy", Cal_Bcy);
            Write(iniFile, "Basic", "Cal_Bca", Cal_Bca);
            Write(iniFile, "Basic", "UseAveGaps4", UseAveGaps4);
            Write(iniFile, "Basic", "IsForceInspect", IsForceInspect);
            Write(iniFile, "Basic", "Optimization", IsCheat);

            // CATE_4
            Write(iniFile, "Basic", "IsResultShowChar", IsResultShowChar);
            Write(iniFile, "Basic", "IsAutoDisableZoom", IsAutoDisableZoom);

            // CATE_5
            Write(iniFile, "Basic", "UsingSingleNgCode", UsingSingleNgCode);
            Write(iniFile, "Basic", "SingleNgCode", SingleNgCode);

            // FTP
            FtpSettings.Save(iniFile);
        }

        #region PRIVATE_VERSION_FUNCTIONS
        void CheckInVersion(string iniFile)
        {
            // 處理舊版
            Read(iniFile, "Version", "ini", "", out string ver);
            if (ver != INI_VERSION)
                Backup(iniFile, ver);

            // 寫入版本標記
            Write(iniFile, "Version", "date", APP_VERSION_DATE);
            Write(iniFile, "Version", "app", APP_VERSION);
            Write(iniFile, "Version", "ini", INI_VERSION);
        }
        void Backup(string iniFile, string tag)
        {
            try
            {
                if (string.IsNullOrEmpty(tag))
                    tag = "v0";
                var dstFile = System.IO.Path.ChangeExtension(iniFile, $".{tag}.ini");
                System.IO.File.Move(iniFile, dstFile);
            }
            catch
            {
            }
        }
        #endregion
    }
}

