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
    public class EzTravellerModel : IDisposable
    {
        const int N_FLY_CAMERAS = 4;

        #region SINGLETON
        static EzTravellerModel _instance;
        EzTravellerModel()
        {
        }
        #endregion
        
        public static EzTravellerModel Instance
        {
            get
            {
                if (_instance == null)
                {
                    var lineScanImage = new GaBigImageHolder();
                    var flyCamImages = new GaBigImageHolder[N_FLY_CAMERAS];
                    for (int i = 0; i < N_FLY_CAMERAS; i++)
                        flyCamImages[i] = new GaBigImageHolder();

                    _instance = new EzTravellerModel()
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

        public GaBigImageHolder LineScanImageHolder
        {
            get;
            private set;
        }
        public GaBigImageHolder[] FlyCamImages
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
