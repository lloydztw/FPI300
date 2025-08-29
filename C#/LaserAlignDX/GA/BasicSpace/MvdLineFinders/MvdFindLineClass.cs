using System.Drawing;
using VisionDesigner;

namespace LaserAlignDX.BasicSpace
{
    using IMPLEMENT = LineFinder.V1.MvdFindLineClass;

    public class MvdFindLineClass : IMvdLineFinder
    {
        #region PRIVATE_MEMBER
        IMvdLineFinder _imp = new IMPLEMENT();
        #endregion

        #region RECIPE_PARAMS
        public bool bPositive { get => _imp.bPositive; set => _imp.bPositive = value; }
        public bool bFindOrient { get => _imp.bFindOrient; set => _imp.bFindOrient = value; }
        public bool bEdgePolarity { get => _imp.bEdgePolarity; set => _imp.bEdgePolarity = value; }
        public int iRayNum { get => _imp.iRayNum; set => _imp.iRayNum = value; }
        public int iEdgeStrength { get => _imp.iEdgeStrength; set => _imp.iEdgeStrength = value; }
        #endregion

        public EdgeBackGroundType Background { get => _imp.Background; set => _imp.Background = value; }

        public void Dispose()
        {
            _imp?.Dispose();
            _imp = null;
        }

        public CMvdLineSegmentF Run(Bitmap bmpInput, RectangleF roi, int borderId)
        {
            return _imp?.Run(bmpInput, roi, borderId);
        }
        public CMvdLineSegmentF Run(Bitmap bmpInput, CMvdRectangleF roi, int borderId)
        {
            return _imp?.Run(bmpInput, roi, borderId);
        }
    }
}
