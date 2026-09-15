using JetEazy.Transform;
using LaserAlignDX.Mvc.Model;
using LeTian.AoiLib;
using System.Collections.Generic;
using System.Drawing;

namespace LaserAlignDX.Model.Coords
{
    public interface IMicroChipTransform : ITransform
    {
        /// <summary>
        /// 是否使用平均邊隙4
        /// </summary>
        bool UseAveGaps4 { get; set; }

        /// <summary>
        /// 建立 晶粒 微距轉換系統 I
        /// </summary>
        /// <param name="targetDim">目標尺寸數據</param>
        /// <param name="lines">晶粒定位 4邊線, 使用與 chipData.ChipQuad2D 相同的坐標系</param>
        /// <param name="chipData">晶粒定位 數據</param>
        /// <returns>錯誤碼</returns>
        /// <remarks>
        /// lines 是 晶粒定位 4邊線, 使用與 chipData.ChipQuad2D 相同的坐標系.
        /// </remarks>
        ErrorCodes BuildMicroTransform(SizeF targetDim, EzLSD.LineSegment[] lines, GaChipData chipData);

        /// <summary>
        /// 計算 尺寸X 與 尺寸Y 
        /// (計算結果 會同步存入 chipData.Dimension 與 chipData.GapEdges 欄位)
        /// </summary>
        /// <param name="dimension">尺寸X 與 尺寸Y</param>
        /// <param name="lines">晶粒定位 4邊線 (與 chipData.ChipQuad2D 相同坐標系)</param>
        /// <param name="chipData">晶粒定位 數據</param>
        /// <param name="includePadGaps">是否包含 邊隙量測</param>
        /// <returns>錯誤碼</returns>
        /// <remarks>
        /// lines 是 晶粒定位 4邊線, 使用與 chipData.ChipQuad2D 相同的坐標系.
        /// </remarks>
        ErrorCodes CalcChipDimension(out SizeF dimension, EzLSD.LineSegment[] lines, GaChipData chipData, bool includePadGaps);

        /// <summary>
        /// 建立 晶粒 微距轉換系統 II
        /// </summary>
        /// <param name="targetDim">目標尺寸數據</param>
        /// <param name="lineBorderPairs">實時 晶粒定位 邊線框對, 使用與 chipData.ChipQuad2D 相同的坐標系</param>
        /// <param name="chipData">實時 晶粒定位 數據</param>
        /// <returns>錯誤碼</returns>
        /// <remarks>
        /// lineBorderPairs 是 實時 晶粒定位 邊線框對, 使用與 chipData.ChipQuad2D 相同的坐標系.
        /// </remarks>
        ErrorCodes BuildMicroTransform(SizeF targetDim, LineBorderPairsCollection lineBorderPairs, GaChipData chipData, RectangleF regionRoi);

        /// <summary>
        /// 計算 具名尺寸 (不含邊隙)
        /// (最新版之 計算結果 會同步存入 chipData.Dimension)
        /// </summary>
        /// <param name="results">具名尺寸 量測結果</param>
        /// <param name="lineBorderPairs">實時 晶粒定位 邊線框對 (與 chipData.ChipQuad2D 相同坐標系)</param>
        /// <param name="chipData">實時 晶粒定位 數據</param>
        /// <param name="globalTrf">可以指定使用外部座標轉換系統, 默認 null</param>
        /// <returns>錯誤碼</returns>
        /// <remarks>
        /// lineBorderPairs 是 實時 晶粒定位 邊線框對, 使用與 chipData.ChipQuad2D 相同的坐標系.
        /// </remarks>
        ErrorCodes CalcChipMeasurements(out Dictionary<string, float> results, LineBorderPairsCollection lineBorderPairs, GaChipData chipData, ITransform globalTrf = null);
    }
}
