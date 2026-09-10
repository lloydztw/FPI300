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

namespace Traveller106.Ini.V1
{
    internal class DtoINI : DtoBase
    {
        #region CATE_1_CAMERA_SETTINGS
#if (OPT_NOT_USED_ANY_MORE)
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
        #endregion

        #region CATE_3_COMPENSATION
        /// <summary>
        /// 强制全检
        /// </summary>
        public bool IsForceInspect = false;    

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
        #endregion

        #region CATE_4_MISC
        /// <summary>
        /// 结果显示数据
        /// </summary>
        public bool IsResultShowChar = false;

        /// <summary>
        /// 飛拍自動停止縮放
        /// </summary>
        public bool IsAutoDisableZoom = false;

        /// <summary>
        /// 增强抓边
        /// </summary>
        public bool IsCheat = false;
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
            //Read(iniFile, "Basic", "ImageResolution", ImageResolution, out ImageResolution);
            //Read(iniFile, "Basic", "ImageResolutionX", ImageResolutionX, out ImageResolutionX);
            //Read(iniFile, "Basic", "ImageResolutionY", ImageResolutionY, out ImageResolutionY);
            Read(iniFile, "Basic", "FlyImageResolution", FlyImageResolution, out FlyImageResolution);
            Read(iniFile, "Basic", "DelayImageTime", LineScanDelayTime, out LineScanDelayTime);

            //IsSaveTestImage = ReadINIValue("Basic", "IsSaveTestImage", (IsSaveTestImage ? "1" : "0"), iniFile) == "1";
            //IsSaveDebugBmp = ReadINIValue("Basic", "IsSaveDebugBMP", (IsSaveDebugBmp ? "1" : "0"), iniFile) == "1";
            //IsSaveDebugOrgBmp = ReadINIValue("Basic", "IsSaveDebugOrgBmp", (IsSaveDebugOrgBmp ? "1" : "0"), iniFile) == "1";
            //UseOkNgDiffImageFolders = ReadINIValue("Basic", "UseOkNgDiffImageFolders", (UseOkNgDiffImageFolders ? "1" : "0"), iniFile) == "1";
            Read(iniFile, "Basic", "IsSaveTestImage", IsSaveTestImage, out IsSaveTestImage);
            Read(iniFile, "Basic", "IsSaveDebugBMP", IsSaveDebugBmp, out IsSaveDebugBmp);
            Read(iniFile, "Basic", "IsSaveDebugOrgBmp", IsSaveDebugOrgBmp, out IsSaveDebugOrgBmp);
            Read(iniFile, "Basic", "UseOkNgDiffImageFolders", UseOkNgDiffImageFolders, out UseOkNgDiffImageFolders);

            //IsAutoDisableZoom = ReadINIValue("Basic", "IsAutoDisableZoom", (IsAutoDisableZoom ? "1" : "0"), iniFile) == "1";
            //IsResultShowChar = ReadINIValue("Basic", "IsResultShowChar", (IsResultShowChar ? "1" : "0"), iniFile) == "1";
            //IsCheat = ReadINIValue("Basic", "IsCheat", (IsCheat ? "1" : "0"), iniFile) == "1";
            Read(iniFile, "Basic", "IsAutoDisableZoom", IsAutoDisableZoom, out IsAutoDisableZoom);
            Read(iniFile, "Basic", "IsResultShowChar", IsResultShowChar, out IsResultShowChar);
            Read(iniFile, "Basic", "IsCheat", false, out IsCheat);

            //Cal_Bcx = float.Parse(ReadINIValue("Basic", "Cal_Bcx", Cal_Bcx.ToString(), iniFile));
            //Cal_Bcy = float.Parse(ReadINIValue("Basic", "Cal_Bcy", Cal_Bcy.ToString(), iniFile));
            //Cal_Bca = float.Parse(ReadINIValue("Basic", "Cal_Bca", Cal_Bca.ToString(), iniFile));
            Read(iniFile, "Basic", "Cal_Bcx", Cal_Bcx, out Cal_Bcx);
            Read(iniFile, "Basic", "Cal_Bcy", Cal_Bcy, out Cal_Bcy);
            Read(iniFile, "Basic", "Cal_Bca", Cal_Bcy, out Cal_Bca);

            //ImageQuality = long.Parse(ReadINIValue("Basic", "ImageQuality", ImageQuality.ToString(), iniFile));
            //ResultImagePath = ReadINIValue("Basic", "ResultImagePath", ResultImagePath.ToString(), iniFile);
            //IsForceInspect = ReadINIValue("Basic", "IsForceInspect", (IsForceInspect ? "1" : "0"), iniFile) == "1";
            Read(iniFile, "Basic", "ImageQuality", ImageQuality, out ImageQuality);
            Read(iniFile, "Basic", "ResultImagePath", ResultImagePath, out ResultImagePath);
            Read(iniFile, "Basic", "IsForceInspect", IsForceInspect, out IsForceInspect);

            //UsingSingleNgCode = ReadINIValue("Basic", "UsingSingleNgCode", (UsingSingleNgCode ? "1" : "0"), iniFile) == "1";
            //SingleNgCode = int.Parse(ReadINIValue("Basic", "SingleNgCode", SingleNgCode.ToString(), iniFile));
            Read(iniFile, "Basic", "UsingSingleNgCode", UsingSingleNgCode, out UsingSingleNgCode);
            Read(iniFile, "Basic", "SingleNgCode", SingleNgCode, out SingleNgCode);
        }
        public override void Save(string iniFile)
        {
            //WriteINIValue("Basic", "ImageResolution", ImageResolution.ToString(), INIFILE);
            //WriteINIValue("Basic", "ImageResolutionX", ImageResolutionX.ToString(), INIFILE);
            //WriteINIValue("Basic", "ImageResolutionY", ImageResolutionY.ToString(), INIFILE);
            //WriteINIValue("Basic", "FlyImageResolution", FlyImageResolution.ToString(), INIFILE);
            //WriteINIValue("Basic", "DelayImageTime", DelayImageTime.ToString(), INIFILE);
            Write(iniFile, "Basic", "FlyImageResolution", FlyImageResolution);
            Write(iniFile, "Basic", "DelayImageTime", LineScanDelayTime);

            //WriteINIValue("Basic", "IsSaveTestImage", (IsSaveTestImage ? "1" : "0"), INIFILE);
            //WriteINIValue("Basic", "IsSaveDebugBMP", (IsSaveDebugBmp ? "1" : "0"), INIFILE);
            //WriteINIValue("Basic", "IsSaveDebugOrgBmp", (IsSaveDebugOrgBmp ? "1" : "0"), INIFILE);
            //WriteINIValue("Basic", "UseOkNgDiffImageFolders", (UseOkNgDiffImageFolders ? "1" : "0"), INIFILE);
            Write(iniFile, "Basic", "IsSaveTestImage", IsSaveTestImage);
            Write(iniFile, "Basic", "IsSaveDebugBMP", IsSaveDebugBmp);
            Write(iniFile, "Basic", "IsSaveDebugOrgBmp", IsSaveDebugOrgBmp);
            Write(iniFile, "Basic", "UseOkNgDiffImageFolders", UseOkNgDiffImageFolders);

            //WriteINIValue("Basic", "IsResultShowChar", (IsResultShowChar ? "1" : "0"), INIFILE);
            //WriteINIValue("Basic", "IsAutoDisableZoom", (IsAutoDisableZoom ? "1" : "0"), INIFILE);
            //WriteINIValue("Basic", "IsCheat", (IsCheat ? "1" : "0"), INIFILE);
            Write(iniFile, "Basic", "IsResultShowChar", IsResultShowChar);
            Write(iniFile, "Basic", "IsAutoDisableZoom", IsAutoDisableZoom);
            Write(iniFile, "Basic", "IsCheat", IsCheat);

            //WriteINIValue("Basic", "Cal_Bcx", Cal_Bcx.ToString(), INIFILE);
            //WriteINIValue("Basic", "Cal_Bcy", Cal_Bcy.ToString(), INIFILE);
            //WriteINIValue("Basic", "Cal_Bca", Cal_Bca.ToString(), INIFILE);
            Write(iniFile, "Basic", "Cal_Bcx", Cal_Bcx);
            Write(iniFile, "Basic", "Cal_Bcy", Cal_Bcy);
            Write(iniFile, "Basic", "Cal_Bca", Cal_Bca);

            //WriteINIValue("Basic", "ImageQuality", ImageQuality.ToString(), INIFILE);
            //WriteINIValue("Basic", "ResultImagePath", ResultImagePath.ToString(), INIFILE);
            //WriteINIValue("Basic", "IsForceInspect", (IsForceInspect ? "1" : "0"), INIFILE);
            Write(iniFile, "Basic", "ImageQuality", ImageQuality);
            Write(iniFile, "Basic", "ResultImagePath", ResultImagePath);
            Write(iniFile, "Basic", "IsForceInspect", IsForceInspect);

            //WriteINIValue("Basic", "UsingSingleNgCode", (UsingSingleNgCode ? "1" : "0"), INIFILE);
            //WriteINIValue("Basic", "SingleNgCode", SingleNgCode.ToString(), INIFILE);
            Write(iniFile, "Basic", "UsingSingleNgCode", UsingSingleNgCode);
            Write(iniFile, "Basic", "SingleNgCode", SingleNgCode);
        }
    }
}

