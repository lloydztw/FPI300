#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-09-20 重整 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion


namespace LaserAlignDX.Mvc.Model.Recipe
{
    /// <summary>
    /// DTO (Data Transfer Object) 類別
    /// DTO 是標準用詞 請自行查 ChatGPT or DeepSeek
    /// </summary>
    public class DtoX3MeasureSpec : DtoBase
    {
        #region SINGLETON
        static DtoX3MeasureSpec _instance;
        private DtoX3MeasureSpec()
        {
        }
        #endregion

        internal static DtoX3MeasureSpec Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new DtoX3MeasureSpec();
                return _instance;
            }
        }

        // 启用设置
        public bool bOpenLineMeasure { get; set; } = false;
        public bool bCheckInspect { get; set; } = false;
        public bool bCheckMeasureOffset { get; set; } = false;

        // 尺寸宽度 spec (mm)
        public float mWidthStand { get; set; } = 9.0f;
        public float mHeightStand { get; set; } = 9.9f;
        public float mWidthPercentage { get; set; } = 1f;
        public float mHeightPercentage { get; set; } = 1f;

        // 尺寸偏移 spec (mm)
        public float PadEdgePercentageX { get; set; } = 0.05f;
        public float PadEdgePercentageY { get; set; } = 0.05f;

        public override void Load(string iniFileName)
        {
            bCheckInspect = ReadINIValue("Basic", "bCheckInspect", "1", iniFileName) == "1";
            bOpenLineMeasure = ReadINIValue("Basic", "bOpenLineMeasure", "0", iniFileName) == "1";
            bCheckMeasureOffset = ReadINIValue("Basic", "bCheckMeasureOffset", "0", iniFileName) == "1";

            mWidthStand = float.Parse(ReadINIValue("Basic", "mWidthStand", "9", iniFileName));
            mHeightStand = float.Parse(ReadINIValue("Basic", "mHeightStand", "9.9", iniFileName));

            mWidthPercentage = float.Parse(ReadINIValue("Basic", "mWidthPercentage", "1.0", iniFileName));
            mHeightPercentage = float.Parse(ReadINIValue("Basic", "mHeightPercentage", "1.0", iniFileName));

            PadEdgePercentageX = float.Parse(ReadINIValue("Basic", "PadEdgePercentageX", "1.0", iniFileName));
            PadEdgePercentageY = float.Parse(ReadINIValue("Basic", "PadEdgePercentageY", "1.0", iniFileName));

        }
        public override void Save(string iniFileName)
        {
            WriteINIValue("Basic", "bCheckInspect", (bCheckInspect ? "1" : "0"), iniFileName);
            WriteINIValue("Basic", "bOpenLineMeasure", (bOpenLineMeasure ? "1" : "0"), iniFileName);
            WriteINIValue("Basic", "bCheckMeasureOffset", (bCheckMeasureOffset ? "1" : "0"), iniFileName);

            WriteINIValue("Basic", "mWidthStand", mWidthStand.ToString(), iniFileName);
            WriteINIValue("Basic", "mHeightStand", mHeightStand.ToString(), iniFileName);
            WriteINIValue("Basic", "mWidthPercentage", mWidthPercentage.ToString(), iniFileName);
            WriteINIValue("Basic", "mHeightPercentage", mHeightPercentage.ToString(), iniFileName);

            WriteINIValue("Basic", "PadEdgePercentageX", PadEdgePercentageX.ToString(), iniFileName);
            WriteINIValue("Basic", "PadEdgePercentageY", PadEdgePercentageY.ToString(), iniFileName);
        }
    }
}
