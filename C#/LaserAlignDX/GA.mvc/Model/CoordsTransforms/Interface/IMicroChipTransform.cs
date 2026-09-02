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
        /// lines 單位是 pixels, 與 chipData.ChipQuad2D.Center 相同 參考原點
        /// </summary>
        ErrorCodes BuildMicroTransform(SizeF targetDim, EzLSD.LineSegment[] lines, GaChipData chipData);

        /// <summary>
        /// lines 單位是 pixels, 與 chipData.ChipQuad2D.Center 相同 參考原點
        /// </summary>
        ErrorCodes CalcChipDimension(out SizeF dimension, EzLSD.LineSegment[] lines, GaChipData chipData, bool includePadGaps);

        /// <summary>
        /// lines 單位是 pixels, 與 chipData.ChipQuad2D.Center 相同 參考原點
        /// </summary>
        ErrorCodes CalcChipMeasurements(out Dictionary<string, float> results, LineBorderPairsCollection lineBorderPairs, GaChipData chipData, ITransform globalTrf = null);
    }
}
