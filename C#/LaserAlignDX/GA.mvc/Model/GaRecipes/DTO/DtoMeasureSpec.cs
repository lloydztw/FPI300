namespace LaserAlignDX.OPSpace.RecipeSpace
{
    /// <summary>
    /// DTO (Data Transfer Object) 類別
    /// DTO 是標準用詞 請自行查 ChatGPT or DeepSeek
    /// </summary>
    internal class DtoMeasureSpec : DtoBase
    {
        #region 启用设置
        public bool bOpenLineMeasure { get; set; } = false;
        public bool bCheckInspect { get; set; } = false;
        public bool bCheckMeasureOffset { get; set; } = false;
        #endregion

        #region 尺寸宽度spec
        public float mWidthStand { get; set; } = 9f;
        public float mWidthUpper { get; set; } = 0.05f;
        public float mWidthLower { get; set; } = 0.05f;
        public float mHeightStand { get; set; } = 9.9f;
        public float mHeightUpper { get; set; } = 0.05f;
        public float mHeightLower { get; set; } = 0.05f;
        #endregion

        #region 尺寸偏移spec
        public float XOffset { get; set; } = 0.05f;
        public float YOffset { get; set; } = 0.05f;
        #endregion

        public override void Load(string INIFILE, string sectNam = null, string keyName = null)
        {
            //xAlgorithm = (MatchAlgorithmEnum)int.Parse(ReadINIValue("Basic", "xAlgorithm", "0", INIFILE));
            //xTolerance = float.Parse(ReadINIValue("Basic", "xTolerance", "0.5", INIFILE));
            //xAngle = float.Parse(ReadINIValue("Basic", "xAngle", "30", INIFILE));
            //xExtendx = int.Parse(ReadINIValue("Basic", "xExtendx", "20", INIFILE));
            //xExtendy = int.Parse(ReadINIValue("Basic", "xExtendy", "20", INIFILE));
            //xMaxOverlap = int.Parse(ReadINIValue("Basic", "xMaxOverlap", "80", INIFILE));
            //xChipOverlap = float.Parse(ReadINIValue("Basic", "xChipOverlap", "0.5", INIFILE));
            //xGridPadThreshold = int.Parse(ReadINIValue("Basic", "xGridPadThreshold", "0", INIFILE));

            bCheckInspect = ReadINIValue("Inspect", "bCheckInspect", "1", INIFILE) == "1";
            //xThresholdValue = int.Parse(ReadINIValue("Inspect", "xThresholdValue", "128", INIFILE));
            //xCharWidth = float.Parse(ReadINIValue("Inspect", "xCharWidth", "15.1", INIFILE));
            //xCharHeight = float.Parse(ReadINIValue("Inspect", "xCharHeight", "15.1", INIFILE));
            //xCharArea = float.Parse(ReadINIValue("Inspect", "xCharArea", "30.1", INIFILE));
            //xBackgroudWidth = float.Parse(ReadINIValue("Inspect", "xBackgroudWidth", "15.1", INIFILE));
            //xBackgroudHeight = float.Parse(ReadINIValue("Inspect", "xBackgroudHeight", "15.1", INIFILE));
            //xBackgroudArea = float.Parse(ReadINIValue("Inspect", "xBackgroudArea", "30.1", INIFILE));

            //RoiCount = int.Parse(ReadINIValue("Inspect", "RoiCount", "0", INIFILE));
            //int i = 0;
            //rectangles.Clear();
            //while (i < RoiCount)
            //{
            //    RectangleF rectf = StringtoRectF(ReadINIValue("Inspect", $"Roi{i.ToString()}", RectFtoStringSimple(new RectangleF(0, 0, 10, 10)), INIFILE));
            //    rectangles.Add(rectf);

            //    i++;
            //}

            bOpenLineMeasure = ReadINIValue("Basic", "bOpenLineMeasure", "0", INIFILE) == "1";
            bCheckMeasureOffset = ReadINIValue("Basic", "bCheckMeasureOffset", "0", INIFILE) == "1";

            //MFLType = (MeasureFindLineType)int.Parse(ReadINIValue("Basic", "MFLType", "0", INIFILE));
            //xCarrierBackground = (EdgeBackGroundType)int.Parse(ReadINIValue("Basic", "CarrierBackground", "0", INIFILE));
            //bPositive0 = ReadINIValue("Basic", "bPositive0", "1", INIFILE) == "1";
            //bPositive1 = ReadINIValue("Basic", "bPositive1", "1", INIFILE) == "1";
            //bPositive2 = ReadINIValue("Basic", "bPositive2", "1", INIFILE) == "1";
            //bPositive3 = ReadINIValue("Basic", "bPositive3", "1", INIFILE) == "1";
            //bEdgePolarity0 = ReadINIValue("Basic", "bEdgePolarity0", "1", INIFILE) == "1";
            //bEdgePolarity1 = ReadINIValue("Basic", "bEdgePolarity1", "1", INIFILE) == "1";
            //bEdgePolarity2 = ReadINIValue("Basic", "bEdgePolarity2", "1", INIFILE) == "1";
            //bEdgePolarity3 = ReadINIValue("Basic", "bEdgePolarity3", "1", INIFILE) == "1";

            mWidthStand = float.Parse(ReadINIValue("Basic", "mWidthStand", "9", INIFILE));
            mWidthUpper = float.Parse(ReadINIValue("Basic", "mWidthUpper", "0.05", INIFILE));
            mWidthLower = float.Parse(ReadINIValue("Basic", "mWidthLower", "0.05", INIFILE));
            mHeightStand = float.Parse(ReadINIValue("Basic", "mHeightStand", "9.9", INIFILE));
            mHeightUpper = float.Parse(ReadINIValue("Basic", "mHeightUpper", "0.05", INIFILE));
            mHeightLower = float.Parse(ReadINIValue("Basic", "mHeightLower", "0.05", INIFILE));
            XOffset = float.Parse(ReadINIValue("Basic", "XOffset", "0.05", INIFILE));
            YOffset = float.Parse(ReadINIValue("Basic", "YOffset", "0.05", INIFILE));

        }
        public override void Save(string INIFILE, string sectNam = null, string keyName = null)
        {
            //WriteINIValue("Basic", "xAlgorithm", ((int)xAlgorithm).ToString(), INIFILE);
            //WriteINIValue("Basic", "xTolerance", xTolerance.ToString(), INIFILE);
            //WriteINIValue("Basic", "xAngle", xAngle.ToString(), INIFILE);
            //WriteINIValue("Basic", "xExtendx", xExtendx.ToString(), INIFILE);
            //WriteINIValue("Basic", "xExtendy", xExtendy.ToString(), INIFILE);
            //WriteINIValue("Basic", "xMaxOverlap", xMaxOverlap.ToString(), INIFILE);
            //WriteINIValue("Basic", "xChipOverlap", xChipOverlap.ToString(), INIFILE);
            //WriteINIValue("Basic", "xGridPadThreshold", xGridPadThreshold.ToString(), INIFILE);

            WriteINIValue("Inspect", "bCheckInspect", (bCheckInspect ? "1" : "0"), INIFILE);

            //WriteINIValue("Inspect", "xThresholdValue", xThresholdValue.ToString(), INIFILE);
            //WriteINIValue("Inspect", "xCharWidth", xCharWidth.ToString(), INIFILE);
            //WriteINIValue("Inspect", "xCharHeight", xCharHeight.ToString(), INIFILE);
            //WriteINIValue("Inspect", "xCharArea", xCharArea.ToString(), INIFILE);
            //WriteINIValue("Inspect", "xBackgroudWidth", xBackgroudWidth.ToString(), INIFILE);
            //WriteINIValue("Inspect", "xBackgroudHeight", xBackgroudHeight.ToString(), INIFILE);
            //WriteINIValue("Inspect", "xBackgroudArea", xBackgroudArea.ToString(), INIFILE);

            WriteINIValue("Basic", "bOpenLineMeasure", (bOpenLineMeasure ? "1" : "0"), INIFILE);
            WriteINIValue("Basic", "bCheckMeasureOffset", (bCheckMeasureOffset ? "1" : "0"), INIFILE);

            //WriteINIValue("Basic", "MFLType", ((int)MFLType).ToString(), INIFILE);
            //WriteINIValue("Basic", "CarrierBackground", ((int)xCarrierBackground).ToString(), INIFILE);

            //WriteINIValue("Basic", "bPositive0", (bPositive0 ? "1" : "0"), INIFILE);
            //WriteINIValue("Basic", "bPositive1", (bPositive1 ? "1" : "0"), INIFILE);
            //WriteINIValue("Basic", "bPositive2", (bPositive2 ? "1" : "0"), INIFILE);
            //WriteINIValue("Basic", "bPositive3", (bPositive3 ? "1" : "0"), INIFILE);
            //WriteINIValue("Basic", "bEdgePolarity0", (bEdgePolarity0 ? "1" : "0"), INIFILE);
            //WriteINIValue("Basic", "bEdgePolarity1", (bEdgePolarity1 ? "1" : "0"), INIFILE);
            //WriteINIValue("Basic", "bEdgePolarity2", (bEdgePolarity2 ? "1" : "0"), INIFILE);
            //WriteINIValue("Basic", "bEdgePolarity3", (bEdgePolarity3 ? "1" : "0"), INIFILE);

            WriteINIValue("Basic", "mWidthStand", mWidthStand.ToString(), INIFILE);
            WriteINIValue("Basic", "mWidthUpper", mWidthUpper.ToString(), INIFILE);
            WriteINIValue("Basic", "mWidthLower", mWidthLower.ToString(), INIFILE);
            WriteINIValue("Basic", "mHeightStand", mHeightStand.ToString(), INIFILE);
            WriteINIValue("Basic", "mHeightUpper", mHeightUpper.ToString(), INIFILE);
            WriteINIValue("Basic", "mHeightLower", mHeightLower.ToString(), INIFILE);
            WriteINIValue("Basic", "XOffset", XOffset.ToString(), INIFILE);
            WriteINIValue("Basic", "YOffset", YOffset.ToString(), INIFILE);
        }
    }
}
