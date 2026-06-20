#region AUTHOR
/*
 * 
 * Copyright (c) 2024 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2024-04-25 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion


namespace EzCamera.Interface
{
    /// <summary>
    /// 相機屬性 (properties)
    /// </summary>
    public interface IEzCameraProps
    {
        /// <summary>
        /// 即時影像 每秒禎數 的最大上限 (由軟體限控),
        /// 0: 代表無限制.
        /// </summary>
        float MaxFps { get; set; }

        /// <summary>
        /// 影像長,寬,與像素位元數
        /// </summary>
        void GetImageInfo(out int width, out int height, out int bitsPerPixel);

        /// <summary>
        /// 曝光時間範圍 (us)
        /// </summary>
        void GetExposureRange(out double min, out double max);
        /// <summary>
        /// 曝光時間 (us)
        /// </summary>
        double ExposureTime { get; set; }

        /// <summary>
        /// 增益範圍 (db)
        /// </summary>
        void GetHardwareGainRange(out double min, out double max);
        /// <summary>
        /// 增益 (db)
        /// </summary>
        double HardwareGain { get; set; }

        /// <summary>
        /// 亮度範圍 (-100, 100)
        /// </summary>
        void GetBrightnessRange(out int min, out int max);
        /// <summary>
        /// 亮度
        /// </summary>
        int Brightness { get; set; }

        /// <summary>
        /// 對比度範圍 (-100, 100)
        /// </summary>
        void GetContrastRange(out int min, out int max);
        /// <summary>
        /// 對比: -100 ~ 100
        /// </summary>
        int Contrast { get; set; }
        
        /// <summary>
        /// 強制轉成單色灰階
        /// </summary>
        bool Mono { get; set; }
        
        /// <summary>
        /// 色彩反向
        /// </summary>
        bool InverseModeEnabled { get; set; }

        /// <summary>
        /// 旋轉角度: 0, 90, 180, 270
        /// </summary>
        int RotateAngle { get; set; }

        /// <summary>
        /// 檢查是否有超大圖像
        /// </summary>
        bool IsLarge();
    }
}
