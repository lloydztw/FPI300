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

using Eazy_Project_III;

namespace LaserAlignDX.Mvc.Model.Recipe
{
    /// <summary>
    /// DTO (Data Transfer Object) 類別
    /// DTO 是標準用詞 請自行查 ChatGPT or DeepSeek
    /// </summary>
    public class DtoX3GridParams : DtoBase
    {
        #region 參數區_PARAM_GRID
        internal int xRow = 1;
        internal int xColumn = 1;
        internal int xLeftTopX = 1;
        internal int xLeftTopY = 1;
        internal float xRowOffset = 2f;
        internal float xColumnOffset = 2f;
        internal float xChipWidth = 10;
        internal float xChipHeight = 10;
        internal int xExtendx = 100;
        internal int xExtendy = 100;
        internal float xAngle = 0;
        internal int xChNum = 1;
        internal int xChValue = 255;
        internal StageNumber xStageNumber = StageNumber.N0;
        #endregion

        #region 參數區_实际矩阵XY_即將被新的座標系統完全取代
        public float xRealLeftX = 0;
        public float xRealLeftY = 0;
        public float xRealOffsetX = 1;
        public float xRealOffsetY = 1;
        #endregion

        public override void Load(string iniFile)
        {
            xRow = int.Parse(ReadINIValue("Recipe Basic", "xRow", "1", iniFile));
            xColumn = int.Parse(ReadINIValue("Recipe Basic", "xColumn", "1", iniFile));
            xLeftTopX = int.Parse(ReadINIValue("Recipe Basic", "xLeftTopX", "1", iniFile));
            xLeftTopY = int.Parse(ReadINIValue("Recipe Basic", "xLeftTopY", "1", iniFile));
            xAngle = float.Parse(ReadINIValue("Recipe Basic", "xAngle", "0", iniFile));
            xRowOffset = float.Parse(ReadINIValue("Recipe Basic", "xRowOffset", "2", iniFile));
            xColumnOffset = float.Parse(ReadINIValue("Recipe Basic", "xColumnOffset", "2", iniFile));
            xChipWidth = float.Parse(ReadINIValue("Recipe Basic", "xChipWidth", "10", iniFile));
            xChipHeight = float.Parse(ReadINIValue("Recipe Basic", "xChipHeight", "10", iniFile));
            xExtendx = int.Parse(ReadINIValue("Recipe Basic", "xExtendx", "100", iniFile));
            xExtendy = int.Parse(ReadINIValue("Recipe Basic", "xExtendy", "100", iniFile));

            //xRealLeftX = float.Parse(ReadINIValue("Recipe Basic", "xRealLeftX", "0", INIFILE));
            //xRealLeftY = float.Parse(ReadINIValue("Recipe Basic", "xRealLeftY", "0", INIFILE));
            xRealOffsetX = float.Parse(ReadINIValue("Recipe Basic", "xRealOffsetX", "1", iniFile));
            xRealOffsetY = float.Parse(ReadINIValue("Recipe Basic", "xRealOffsetY", "1", iniFile));            xStageNumber = (StageNumber)int.Parse(ReadINIValue("Recipe Basic", "xStageNumber", "0", iniFile));

            xChNum = int.Parse(ReadINIValue("Recipe Basic", "xChNum", "1", iniFile));
            xChValue = int.Parse(ReadINIValue("Recipe Basic", "xChValue", "255", iniFile));
        }
        public override void Save(string iniFile)
        {
            WriteINIValue("Recipe Basic", "xRow", xRow.ToString(), iniFile);
            WriteINIValue("Recipe Basic", "xColumn", xColumn.ToString(), iniFile);
            WriteINIValue("Recipe Basic", "xLeftTopX", xLeftTopX.ToString(), iniFile);
            WriteINIValue("Recipe Basic", "xLeftTopY", xLeftTopY.ToString(), iniFile);
            WriteINIValue("Recipe Basic", "xAngle", xAngle.ToString(), iniFile);
            WriteINIValue("Recipe Basic", "xRowOffset", xRowOffset.ToString(), iniFile);
            WriteINIValue("Recipe Basic", "xColumnOffset", xColumnOffset.ToString(), iniFile);
            WriteINIValue("Recipe Basic", "xChipWidth", xChipWidth.ToString(), iniFile);
            WriteINIValue("Recipe Basic", "xChipHeight", xChipHeight.ToString(), iniFile);
            WriteINIValue("Recipe Basic", "xExtendx", xExtendx.ToString(), iniFile);
            WriteINIValue("Recipe Basic", "xExtendy", xExtendy.ToString(), iniFile);

            //WriteINIValue("Recipe Basic", "xRealLeftX", xRealLeftX.ToString(), INIFILE);
            //WriteINIValue("Recipe Basic", "xRealLeftY", xRealLeftY.ToString(), INIFILE);
            WriteINIValue("Recipe Basic", "xRealOffsetX", xRealOffsetX.ToString(), iniFile);
            WriteINIValue("Recipe Basic", "xRealOffsetY", xRealOffsetY.ToString(), iniFile);
            WriteINIValue("Recipe Basic", "xStageNumber", ((int)xStageNumber).ToString(), iniFile);

            WriteINIValue("Recipe Basic", "xChNum", xChNum.ToString(), iniFile);
            WriteINIValue("Recipe Basic", "xChValue", xChValue.ToString(), iniFile);

            //WriteINIValue("Recipe Basic", "zFocusOnCarrier", zFocusOnCarrier.ToString(), INIFILE);
            //WriteINIValue("Recipe Basic", "zFocusOnChip", zFocusOnChip.ToString(), INIFILE);
        }
    }
}
