#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-09-22 重整 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion


namespace LaserAlignDX.AoiModel
{
    public enum PlcFlyResultCode : int
    {
        OK = 1,
        NG = 2,
        Empty = 3,
    };

    public class FlyID
    {
        public FlyID(int flyStart, int flyIndex)
        {
            this.flyStart = flyStart;
            this.flyIndex = flyIndex;
        }
        /// <summary>
        /// 來自 PLC 指定
        /// </summary>
        public int flyStart
        {
            get;
            private set;
        }
        /// <summary>
        /// 以 0 為基底的 局域 index
        /// </summary>
        public int flyIndex
        {
            get;
            private set;
        }
        /// <summary>
        /// 根據 flyStart 與 flyIndex, 取得以 1 為基底的全域 ID
        /// </summary>
        public int ShowID
        {
            get
            {
                int showID = flyIndex + 1;
                switch (flyStart)
                {
                    case 1:
                        showID = flyIndex + 1;
                        break;
                    case 2:
                        showID = flyIndex + 1 + 4;
                        break;
                }
                return showID;
            }
        }
    }

    public class FlyAoiResult
    {
        public PlcFlyResultCode Code;
        public float OffsetX;
        public float OffsetY;
        public float OffsetAngle;
        public FlyMetaData MetaData;
    }

    public class FlyLotData
    {
        public string StripID;
        public string LotID;
        public FlyLotData(string stripID, string lotID)
        {
            StripID = stripID;
            LotID = lotID;
        }
        public FlyLotData() : this("Strip_NONE", "Lot_NONE")
        {
        }
    }
}
