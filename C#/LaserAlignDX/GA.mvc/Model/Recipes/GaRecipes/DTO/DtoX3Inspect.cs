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
using LaserAlignDX.AoiModel;
using LaserAlignDX.BasicSpace;
using System.Collections.Generic;
using System.Drawing;


namespace LaserAlignDX.Mvc.Model.Recipe
{
    /// <summary>
    /// DTO (Data Transfer Object) 類別
    /// DTO 是標準用詞 請自行查 ChatGPT or DeepSeek
    /// </summary>
    public class DtoX3Inspect : DtoBase
    {
        #region 晶粒定位
        public MatchAlgorithmEnum xAlgorithm { get; set; } = MatchAlgorithmEnum.GridMatch;
        public int xGridPadThreshold { get; set; } = 0;
        public float xTolerance { get; set; } = 0.5f;
        public float xAngle { get; set; } = 30f;
        public int xExtendx { get; set; } = 20;
        public int xExtendy { get; set; } = 20;
        public int xMaxOverlap { get; set; } = 80;
        public float xChipOverlap { get; set; } = 0.5f;
        #endregion

        #region 找直线的参数
        public EdgeBackGroundType xCarrierBackground { get; set; } = EdgeBackGroundType.Dark;
        public MeasureFindLineType MFLType { get; set; } = MeasureFindLineType.FindLineType_v1;
        public bool[] xEdgePolarities { get; } = new bool[4];        // RESERVED
        public bool[] xPositives { get; } = new bool[4];             // RESERVED
        #endregion

        #region 找直线的手拉框
        public RectangleF[] xLineBorderRects { get; set; } = new RectangleF[4];
        #endregion

        #region 缺陷检测设置
        public int xThresholdValue { get; set; } = 128;
        public float xCharWidth { get; set; } = 15.1f;
        public float xCharHeight { get; set; } = 15.1f;
        public float xCharArea { get; set; } = 30.1f;
        public float xBackgroudWidth { get; set; } = 15.1f;
        public float xBackgroudHeight { get; set; } = 15.1f;
        public float xBackgroudArea { get; set; } = 30.1f;
        #endregion

        #region MASK_RECTS
        public List<RectangleF> xMaskRects { get; set; } = new List<RectangleF>();
        public int RoiCount => xMaskRects.Count;
        #endregion

        public override void Load(string iniFile, string sectName = null, string keyName = null)
        {
            xAlgorithm = (MatchAlgorithmEnum)int.Parse(ReadINIValue("Basic", "xAlgorithm", "0", iniFile));
            xTolerance = float.Parse(ReadINIValue("Basic", "xTolerance", "0.5", iniFile));
            xAngle = float.Parse(ReadINIValue("Basic", "xAngle", "30", iniFile));
            xExtendx = int.Parse(ReadINIValue("Basic", "xExtendx", "20", iniFile));
            xExtendy = int.Parse(ReadINIValue("Basic", "xExtendy", "20", iniFile));
            xMaxOverlap = int.Parse(ReadINIValue("Basic", "xMaxOverlap", "80", iniFile));
            xChipOverlap = float.Parse(ReadINIValue("Basic", "xChipOverlap", "0.5", iniFile));
            xGridPadThreshold = int.Parse(ReadINIValue("Basic", "xGridPadThreshold", "0", iniFile));

            xThresholdValue = int.Parse(ReadINIValue("Inspect", "xThresholdValue", "128", iniFile));
            xCharWidth = float.Parse(ReadINIValue("Inspect", "xCharWidth", "15.1", iniFile));
            xCharHeight = float.Parse(ReadINIValue("Inspect", "xCharHeight", "15.1", iniFile));
            xCharArea = float.Parse(ReadINIValue("Inspect", "xCharArea", "30.1", iniFile));
            xBackgroudWidth = float.Parse(ReadINIValue("Inspect", "xBackgroudWidth", "15.1", iniFile));
            xBackgroudHeight = float.Parse(ReadINIValue("Inspect", "xBackgroudHeight", "15.1", iniFile));
            xBackgroudArea = float.Parse(ReadINIValue("Inspect", "xBackgroudArea", "30.1", iniFile));
            xCarrierBackground = (EdgeBackGroundType)int.Parse(ReadINIValue("Basic", "xCarrierBackground", "0", iniFile));

            LoadMaskRects(iniFile);
            LoadLineBorderRects(iniFile);
        }
        public override void Save(string iniFile, string sectName = null, string keyName = null)
        {
            WriteINIValue("Basic", "xAlgorithm", ((int)xAlgorithm).ToString(), iniFile);
            WriteINIValue("Basic", "xTolerance", xTolerance.ToString(), iniFile);
            WriteINIValue("Basic", "xAngle", xAngle.ToString(), iniFile);
            WriteINIValue("Basic", "xExtendx", xExtendx.ToString(), iniFile);
            WriteINIValue("Basic", "xExtendy", xExtendy.ToString(), iniFile);
            WriteINIValue("Basic", "xMaxOverlap", xMaxOverlap.ToString(), iniFile);
            WriteINIValue("Basic", "xChipOverlap", xChipOverlap.ToString(), iniFile);
            WriteINIValue("Basic", "xGridPadThreshold", xGridPadThreshold.ToString(), iniFile);

            WriteINIValue("Inspect", "xThresholdValue", xThresholdValue.ToString(), iniFile);
            WriteINIValue("Inspect", "xCharWidth", xCharWidth.ToString(), iniFile);
            WriteINIValue("Inspect", "xCharHeight", xCharHeight.ToString(), iniFile);
            WriteINIValue("Inspect", "xCharArea", xCharArea.ToString(), iniFile);
            WriteINIValue("Inspect", "xBackgroudWidth", xBackgroudWidth.ToString(), iniFile);
            WriteINIValue("Inspect", "xBackgroudHeight", xBackgroudHeight.ToString(), iniFile);
            WriteINIValue("Inspect", "xBackgroudArea", xBackgroudArea.ToString(), iniFile);
            WriteINIValue("Basic", "xCarrierBackground", ((int)xCarrierBackground).ToString(), iniFile);

            //WriteINIValue("Basic", "bPositive0", (bPositive0 ? "1" : "0"), INIFILE);
            //WriteINIValue("Basic", "bPositive1", (bPositive1 ? "1" : "0"), INIFILE);
            //WriteINIValue("Basic", "bPositive2", (bPositive2 ? "1" : "0"), INIFILE);
            //WriteINIValue("Basic", "bPositive3", (bPositive3 ? "1" : "0"), INIFILE);
            //WriteINIValue("Basic", "bEdgePolarity0", (bEdgePolarity0 ? "1" : "0"), INIFILE);
            //WriteINIValue("Basic", "bEdgePolarity1", (bEdgePolarity1 ? "1" : "0"), INIFILE);
            //WriteINIValue("Basic", "bEdgePolarity2", (bEdgePolarity2 ? "1" : "0"), INIFILE);
            //WriteINIValue("Basic", "bEdgePolarity3", (bEdgePolarity3 ? "1" : "0"), INIFILE);

            SaveMaskRects(iniFile);
            SaveLineBorderRects(iniFile);
        }

        internal void LoadMaskRects(string iniFile)
        {
            xMaskRects.Clear();
            int count = int.Parse(ReadINIValue("Inspect", "RoiCount", "0", iniFile));
            for (int i = 0; i < count; i++)
            {
                RectangleF rect = StringtoRectF(ReadINIValue("Inspect", $"Roi{i.ToString()}", RectFtoStringSimple(new RectangleF(0, 0, 10, 10)), iniFile));
                xMaskRects.Add(rect);
            }
        }
        internal void SaveMaskRects(string iniFile)
        {
            int count = xMaskRects.Count;
            WriteINIValue("Inspect", "RoiCount", count.ToString(), iniFile);
            for (int i = 0; i < count; i++)
            {
                WriteINIValue("Inspect", $"Roi{i.ToString()}", RectFtoStringSimple(xMaskRects[i]), iniFile);
            }
        }

        internal void LoadLineBorderRects(string iniFile)
        {
            int i = 0;
            xLineBorderRects[i++] = StringtoRectF(ReadINIValue("Recipe Basic", "xLineLeft", RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), iniFile));
            xLineBorderRects[i++] = StringtoRectF(ReadINIValue("Recipe Basic", "xLineTop", RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), iniFile));
            xLineBorderRects[i++] = StringtoRectF(ReadINIValue("Recipe Basic", "xLineRight", RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), iniFile));
            xLineBorderRects[i++] = StringtoRectF(ReadINIValue("Recipe Basic", "xLineBottom", RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), iniFile));
        }
        internal void SaveLineBorderRects(string iniFile)
        {
            int i = 0;
            WriteINIValue("Recipe Basic", "xLineLeft", RectFtoStringSimple(xLineBorderRects[i++]), iniFile);
            WriteINIValue("Recipe Basic", "xLineTop", RectFtoStringSimple(xLineBorderRects[i++]), iniFile);
            WriteINIValue("Recipe Basic", "xLineRight", RectFtoStringSimple(xLineBorderRects[i++]), iniFile);
            WriteINIValue("Recipe Basic", "xLineBottom", RectFtoStringSimple(xLineBorderRects[i++]), iniFile);
        }
    }
}
