using System.Drawing;
using VisionDesigner;
using VisionDesigner.PairLineFind;

namespace LaserAlignDX.BasicSpace
{
    using IMPLEMENT = LineFinder.V1.MvdPairLineClass;

    public class MvdPairLineClass : IMvdPairLineFinder
    {
        #region PRIVATE_MEMBER
        IMvdPairLineFinder _imp = new IMPLEMENT();
        #endregion

        public bool bPositive { get => _imp.bPositive; set => _imp.bPositive = value; }
        public bool bFindOrient { get => _imp.bFindOrient; set => _imp.bFindOrient = value; }
        public int CaliperNum { get => _imp.CaliperNum; set => _imp.CaliperNum = value; }
        public int iEdgeStrength { get => _imp.iEdgeStrength; set => _imp.iEdgeStrength = value; }

        public void Dispose()
        {
            _imp.Dispose();
        }
        public CPairLineFindResult Run(Bitmap bmpInput, RectangleF roi, int borderId = -1)
        {
            return _imp.Run(bmpInput, roi, borderId);
        }
        public CPairLineFindResult Run(Bitmap bmpInput, CMvdRectangleF roi, int borderId = -1)
        {
            return _imp.Run(bmpInput, roi, borderId);
        }
    }
}
