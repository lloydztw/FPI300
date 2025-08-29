#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-08-28 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System;

namespace LaserAlignDX.AoiModel
{
    /// <summary>
    /// 統一管理 巨圖 生命週期
    /// </summary>
    public class TravellerBigImagesHolder : IDisposable
    {
        const int N_FLY_CAMERAS = 4;

        #region SINGLETON
        static TravellerBigImagesHolder _instance;
        TravellerBigImagesHolder()
        {
        }
        #endregion
        
        public static TravellerBigImagesHolder Instance
        {
            get
            {
                if (_instance == null)
                {
                    var lineScanImage = new GaBigImageHolder();
                    var flyCamImages = new GaBigImageHolder[N_FLY_CAMERAS];
                    for (int i = 0; i < N_FLY_CAMERAS; i++)
                        flyCamImages[i] = new GaBigImageHolder();

                    _instance = new TravellerBigImagesHolder()
                    {
                        FlyCamImages = flyCamImages,
                        LineScanImageHolder = lineScanImage,
                    };
                }
                return _instance;
            }
        }
        public static void DisposeAll()
        {
            _instance?.Dispose();
            _instance = null;
        }
        public void Dispose()
        {
            var holders = FlyCamImages;
            if (holders != null)
            {
                FlyCamImages = null;
                cleanUp(holders);
            }

            var holder = LineScanImageHolder;
            if (holder != null)
            {
                LineScanImageHolder = null;
                cleanUp(holder);
            }

            _instance = null;
        }

        /// <summary>
        /// 線掃相機巨圖持管者
        /// </summary>
        public GaBigImageHolder LineScanImageHolder
        {
            get;
            private set;
        }

        /// <summary>
        /// 保留以後納入 Fly Cam 的 Image 管理
        /// </summary>
        protected GaBigImageHolder[] FlyCamImages
        {
            get;
            private set;
        }

        #region PRIVATE_FUNCTIONS
        void cleanUp(params GaBigImageHolder[] imgHolders)
        {
            foreach ( var imgHolder in imgHolders)
                imgHolder?.Dispose();
        }
        #endregion
    }
}
