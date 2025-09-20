using Eazy_Project_III;
using JetEazy;
using LaserAlignDX.AoiModel;
using LaserAlignDX.BasicSpace;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Drawing;

namespace LaserAlignDX.OPSpace.RecipeSpace
{
    /// <summary>
    /// DTO (Data Transfer Object) 類別
    /// DTO 是標準用詞 請自行查 ChatGPT or DeepSeek
    /// </summary>
    internal class DtoX3Inspection : DtoBase
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
        public EdgeBackGroundType xCarrierBackground { get; set; }
        #endregion

        #region 缺陷检测设置
        public int xThresholdValue { get; set; } = 128;
        public float xCharWidth { get; set; } = 15.1f;
        public float xCharHeight { get; set; } = 15.1f;
        public float xCharArea { get; set; } = 30.1f;
        public float xBackgroudWidth { get; set; } = 15.1f;
        public float xBackgroudHeight { get; set; } = 15.1f;
        public float xBackgroudArea { get; set; } = 30.1f;

        public int RoiCount { get; set; } = 0;
        public List<RectangleF> rectangles { get; set; } = new List<RectangleF>();
        #endregion

        public override void Load(string INIFILE, string sectNam = null, string keyName = null)
        {
        }
        public override void Save(string INIFILE, string sectNam = null, string keyName = null)
        {
            
        }
    }
}
