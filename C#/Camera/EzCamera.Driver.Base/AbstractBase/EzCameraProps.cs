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


using EzCamera.Interface;
using EzCamera.Driver.Utils;

namespace EzCamera.Driver.Base
{
    /// <summary>
    /// 相機通用屬性
    /// </summary>
    public class EzCameraProps : IEzCameraProps
    {
        #region PROTECTED_DATA
        /// <summary>
        /// 保存 Camera 影像 長,寬,BitsPerPixel 等資訊
        /// </summary>
        protected EzImageInfo _camImageInfo = new EzImageInfo();
        protected int _rotateAngle = 0;
        #endregion

        public virtual float MaxFps { get; set; }
        public virtual int RotateAngle
        {
            get => _rotateAngle; 
            set => _rotateAngle = (value / 90) * 90 % 360;
        }

        public virtual void GetImageInfo(out int width, out int height, out int bitsPerPixel)
        {
            bitsPerPixel = _camImageInfo.BitsPerPixel;
            width = _camImageInfo.Width;
            height = _camImageInfo.Height;

            // 如果有使用到軟體轉角度 90 與 270 
            // 必須回報 互換後的 width, height
            if (RotateAngle == 90 || RotateAngle == 270)
                (width, height) = (height, width);
        }
        public virtual void GetBrightnessRange(out int min, out int max)
        {
            min = -100;
            max = 100;
        }
        public virtual void GetContrastRange(out int min, out int max)
        {
            min = -100;
            max = 100;
        }
        public virtual void GetExposureRange(out double min, out double max)
        {
            // 單位 us
            // 1 us = 0.001 ms
            min = 1;
            // 5000 * 1000 us = 5000 ms
            max = 5000 * 1000;
        }
        public virtual void GetHardwareGainRange(out double min, out double max)
        {
            min = 0;
            max = 15;
        }

        public virtual bool Mono { get; set; }
        public virtual bool InverseModeEnabled { get; set; }
        public virtual double ExposureTime { get; set; }
        public virtual double HardwareGain { get; set; }
        public virtual int Brightness { get; set; }
        public virtual int Contrast { get; set; }

        public virtual bool IsLarge()
        {
            this.GetImageInfo(out int w, out int h, out int _);
            return w * h > 6000 * 6000;
        }
    }
}
