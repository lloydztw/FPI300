using JetEazy.Transform;
using LaserAlignDX.Mvc.Model;
using LeTian.AoiLib;
using System.Drawing;

namespace LaserAlignDX.Model.Coords
{
    public interface IMicroChipTransform : ITransform
    {
        ErrorCodes BuildMicroTransform(SizeF targetDim, EzLSD.LineSegment[] lines, GaChipData chipData);
        ErrorCodes CalcChipDimension(out SizeF dimension, EzLSD.LineSegment[] lines, GaChipData chipData, bool includePadGaps);
    }
}
