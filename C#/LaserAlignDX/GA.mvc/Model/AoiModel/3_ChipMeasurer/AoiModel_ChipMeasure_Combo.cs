#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-08-01 開始重整 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using LaserAlignDX.BasicSpace;
using LaserAlignDX.OPSpace;
using LaserAlignDX.OPSpace.RecipeSpace;
using System.Drawing;
using VisionDesigner;
using AoiModel_ChipMeasure_New = LaserAlignDX.AoiModel.V35.AoiModel_ChipMeasure;
using AoiModel_ChipMeasure_Quad = LaserAlignDX.AoiModel.V31.AoiModel_ChipMeasure;

namespace LaserAlignDX.AoiModel.Combo
{
    /// <summary>
    /// 晶粒尺寸量測
    /// </summary>
    public class AoiModel_ChipMeasure : IAoiChipMeasurer
    {
        #region PRIVATE_DATA
        RecipeFPIX3Class _xRecipe => RecipeFPIX3Class.Instance;
        IAoiChipMeasurer _aoiChipMeasureQ = new AoiModel_ChipMeasure_Quad();
        IAoiChipMeasurer _aoiChipMeasureN = new AoiModel_ChipMeasure_New();
        IAoiChipMeasurer _imp
        {
            get
            {
                if(_xRecipe.LineBorderParams.IsSimpleQuad)
                    return _aoiChipMeasureQ;
                else
                    return _aoiChipMeasureN;
            }
        }
        #endregion

        public void AnalyzeGoldenData()
        {
            _imp.AnalyzeGoldenData();
        }

        public void Dispose()
        {
            _aoiChipMeasureQ?.Dispose();
            _aoiChipMeasureQ = null;
            _aoiChipMeasureN?.Dispose();
            _aoiChipMeasureN = null;
        }

        public void Run(Bitmap sceneBmp = null)
        {
            _imp.Run(sceneBmp);
        }

        public void SetCellGroups(GaCellsGroup[] cellGroups)
        {
            _imp.SetCellGroups(cellGroups);
        }

        public bool TryApplyFilters(Bitmap bmpSrc, out Bitmap bmpResult, Color? backGroundColor = null)
        {
            return _imp.TryApplyFilters(bmpSrc, out bmpResult, backGroundColor);
        }

        public bool TryFindLineSegment(EdgeBorder eBorder, Bitmap bmpSrc, RectangleF roiRect, out CMvdLineSegmentF resultLine)
        {
            return _imp.TryFindLineSegment(eBorder, bmpSrc, roiRect, out resultLine);
        }

        public void TryMeasureOneChip(RegionCellX3Class cell, Bitmap cellBmp, ref RectangleF cellRoi)
        {
            _imp.TryMeasureOneChip(cell, cellBmp, ref cellRoi);
        }

        #region OPERATORS
        public AoiModelBase BaseQ => _aoiChipMeasureQ as AoiModelBase;
        public AoiModelBase BaseN => _aoiChipMeasureN as AoiModelBase;
        public static implicit operator AoiModelBase(AoiModel_ChipMeasure model)
        {
            return (model?._imp) as AoiModelBase;
        }
        #endregion
    }
}
