using System;
using System.Drawing;
using OpenCvSharp;


namespace JetEazy.BasicSpace
{
    public class CAoiCalibration : JetEazy.Aoi.TransformD.CxCamCoordTransform
    {
        public CAoiCalibration(CAoiCalibration src = null) : base(src)
        {
        }
        public CAoiCalibration Clone()
        {
            CAoiCalibration obj = new CAoiCalibration(this);
            return obj;
        }

        public bool CalculateTransformMatrix()
        {
            return base.BuildTransformFormula();
        }
        public void TransformWorldToView(PointF pointWorld, out PointF pointView)
        {
            pointView = ToLocal(pointWorld, null);
        }
        public void TransformViewToWorld(PointF pointView, out PointF pointWorld)
        {
            pointWorld = ToWorld(pointView);
        }

        public void Load1(string fileName)
        {
            string ext = System.IO.Path.GetExtension(fileName);
            if (string.Compare(ext, ".bin", true) != 0)
                fileName += ext;
            
            base.LoadBin(fileName);
        }
        public void Save1(string fileName)
        {
            string ext = System.IO.Path.GetExtension(fileName);
            if (string.Compare(ext, ".bin", true) != 0)
                fileName += ext;
            base.SaveBin(fileName);
        }
    }
}
