using Common.RecipeSpace;
using System.Drawing;


namespace LaserAlignDX.OPSpace.RecipeSpace
{
    public class LineScanCalibrateClass : RecipeBaseClass
    {
        const int POINT_COUNT = 4;

        JetEazy.BasicSpace.CAoiCalibration cAoiCalibration = new JetEazy.BasicSpace.CAoiCalibration();
        PointF[,] _v1 = new PointF[2, 2];
        PointF[,] _w1 = new PointF[2, 2];

        //JetEazy.BasicSpace.CAoiCalibration cAoiCalibration2 = new JetEazy.BasicSpace.CAoiCalibration();
        //PointF[,] _v2 = new PointF[2, 2];
        //PointF[,] _w2 = new PointF[2, 2];

        public LineScanCalibrateClass()
        {

        }
        //private static LineScanCalibrateClass _instance = null;
        //public static LineScanCalibrateClass Instance
        //{
        //    get
        //    {
        //        if (_instance == null)
        //            _instance = new LineScanCalibrateClass();
        //        return _instance;
        //    }
        //}
        public override void Initial(string epath, int ercpindex, string enamefile)
        {
            base.Initial(epath, ercpindex, enamefile);
            string dir = $"{Path}\\Calibration";
            INIFILE = $"{dir}\\{Name}";
        }
        public override void ChangeIndex(int eindex)
        {
            //base.ChangeIndex(eindex);
        }
        public PointF ViewToWorld(PointF ptview)
        {
            PointF ptworld = new PointF(ptview.X, ptview.Y);
            cAoiCalibration.TransformViewToWorld(ptview, out ptworld);
            return ptworld;
        }
        public PointF WorldToView(PointF ptworld)
        {
            PointF ptview = new PointF(ptworld.X, ptworld.Y);
            cAoiCalibration.TransformViewToWorld(ptworld, out ptview);
            return ptview;
        }
        //public PointF T2ViewToWorld(PointF ptview)
        //{
        //    PointF ptworld = new PointF(ptview.X, ptview.Y);
        //    cAoiCalibration2.TransformViewToWorld(ptview, out ptworld);
        //    return ptworld;
        //}
        //public PointF T2WorldToView(PointF ptworld)
        //{
        //    PointF ptview = new PointF(ptworld.X, ptworld.Y);
        //    cAoiCalibration2.TransformViewToWorld(ptworld, out ptview);
        //    return ptview;
        //}

        public PointF[] ptsview = new PointF[POINT_COUNT];
        public PointF[] ptsworld = new PointF[POINT_COUNT];
        //public PointF[] pts2view = new PointF[POINT_COUNT];
        //public PointF[] pts2world = new PointF[POINT_COUNT];

        //public override void Initial(string epath, int ercpindex, string enamefile)
        //{
        //    base.Initial(epath, ercpindex, enamefile);
        //}
        public override void Load(bool eCancel = false)
        {
            int i = 0;
            while (i < POINT_COUNT)
            {
                ptsview[i] = StringtoPointF(ReadINIValue("Tray", $"ptsview_{i}", $"{i},{i}", INIFILE));
                ptsworld[i] = StringtoPointF(ReadINIValue("Tray", $"ptsworld_{i}", $"{i},{i}", INIFILE));

                //pts2view[i] = StringtoPointF(ReadINIValue("Tray2", $"pts2view_{i}", $"{i},{i}", INIFILE));
                //pts2world[i] = StringtoPointF(ReadINIValue("Tray2", $"pts2world_{i}", $"{i},{i}", INIFILE));

                i++;
            }

            run();
        }
        public override void Save()
        {
            int i = 0;
            while (i < POINT_COUNT)
            {
                WriteINIValue("Tray", $"ptsview_{i}", PointFtoStringSimple(ptsview[i]), INIFILE);
                WriteINIValue("Tray", $"ptsworld_{i}", PointFtoStringSimple(ptsworld[i]), INIFILE);
                //WriteINIValue("Tray2", $"pts2view_{i}", PointFtoStringSimple(pts2view[i]), INIFILE);
                //WriteINIValue("Tray2", $"pts2world_{i}", PointFtoStringSimple(pts2world[i]), INIFILE);

                i++;
            }

            run();
        }

        private void run()
        {
            _v1[0, 1] = ptsview[0];
            _v1[1, 1] = ptsview[1];
            _v1[0, 0] = ptsview[2];
            _v1[1, 0] = ptsview[3];

            _w1[0, 1] = ptsworld[0];
            _w1[1, 1] = ptsworld[1];
            _w1[0, 0] = ptsworld[2];
            _w1[1, 0] = ptsworld[3];
            cAoiCalibration.Dispose();
            cAoiCalibration.SetCalibrationPoints(_v1, _w1);
            cAoiCalibration.CalculateTransformMatrix();

            //_v2[0, 1] = pts2view[0];
            //_v2[1, 1] = pts2view[1];
            //_v2[0, 0] = pts2view[2];
            //_v2[1, 0] = pts2view[3];

            //_w2[0, 1] = pts2world[0];
            //_w2[1, 1] = pts2world[1];
            //_w2[0, 0] = pts2world[2];
            //_w2[1, 0] = pts2world[3];
            //cAoiCalibration2.Dispose();
            //cAoiCalibration2.SetCalibrationPoints(_v2, _w2);
            //cAoiCalibration2.CalculateTransformMatrix();

        }
    }

}
