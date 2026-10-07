using CSML;
using System;

namespace JetEazy.BasicSpace
{
    class JzPlaneClass
    {
    }
    /// <summary>
    /// Equation Ax + By + C = z
    /// </summary>
    /// 
    public class QPlane
    {
        public double A;
        public double B;
        public double C;

        public void LeastSquareFit(QPoint3D[] points)
        {
            int iLength = points.Length;

            if (iLength < 3)
                throw new Exception("Error: QPlane.Fit needs 3 or more points as input !");

            Matrix M = new Matrix(3, 3);
            Matrix Z = new Matrix(3, 1);

            double SumXX = 0.0;
            double SumYY = 0.0;
            double SumXY = 0.0;
            double SumX = 0.0;
            double SumY = 0.0;
            double Sum = 0.0;
            double SumXZ = 0.0;
            double SumYZ = 0.0;
            double SumZ = 0.0;
            for (int i = 0; i < iLength; i++)
            {
                double x = points[i].x;
                double y = points[i].y;
                double z = points[i].z;

                SumXX += (x * x);
                SumYY += (y * y);
                SumXY += (x * y);
                SumX += x;
                SumY += y;
                Sum += 1.0;

                SumXZ += (x * z);
                SumYZ += (y * z);
                SumZ += z;
            }


            M[1, 1] = new Complex(SumXX);
            M[1, 2] = new Complex(SumXY);
            M[1, 3] = new Complex(SumX);

            M[2, 1] = new Complex(SumXY);
            M[2, 2] = new Complex(SumYY);
            M[2, 3] = new Complex(SumY);

            M[3, 1] = new Complex(SumX);
            M[3, 2] = new Complex(SumY);
            M[3, 3] = new Complex(Sum);

            Z[1, 1] = new Complex(SumXZ);
            Z[2, 1] = new Complex(SumYZ);
            Z[3, 1] = new Complex(SumZ);

            Matrix MM = M.Inverse();
            Matrix Ans = MM * Z;

            A = Ans[1, 1].Re;
            B = Ans[2, 1].Re;
            C = Ans[3, 1].Re;
        }

        public double GetDistance(QPoint3D pt)
        {
            // Rewrite the equation
            //   ax + by + cz + d = 0
            // ie.
            //   a=A, b=B, c=-1, d=C
            //
            // then the distance from pt to this plane is
            //    
            //  dist = ( a(pt.x) + b(pt.y) + c(pt.z) + d ) / sqrt(a*a + b*b + c*c) 
            //       = ( A(pt.x) + B(pt.y) - pt.z + C ) / sqrt(A*A + B*B +1)
            //
            return System.Math.Abs(A * pt.x + B * pt.y - pt.z + C) / System.Math.Sqrt(A * A + B * B + 1.0);
        }

        public double GetZLocation(QPoint3D pt)
        {
            // Rewrite the equation
            //   ax + by + cz + d = 0
            // ie.
            //   a=A, b=B, c=-1, d=C
            //
            // then the distance from pt to this plane is
            //    
            //  dist = ( a(pt.x) + b(pt.y) + c(pt.z) + d ) / sqrt(a*a + b*b + c*c) 
            //       = ( A(pt.x) + B(pt.y) - pt.z + C ) / sqrt(A*A + B*B +1)
            //
            return -(A * pt.x + B * pt.y - pt.z + C) / System.Math.Sqrt(A * A + B * B + 1.0);
        }

        public string ToEquation()
        {
            return "Equation: " + A.ToString() + " X + " + B.ToString() + " Y + " + C.ToString() + " = Z";
        }
    }

    public struct QPoint3D
    {
        public double x;
        public double y;
        public double z;
        public QPoint3D(double xx, double yy, double zz)
        {
            x = xx;
            y = yy;
            z = zz;
        }
        public override string ToString()
        {
            return string.Format("{0}, {1}, {2}", x, y, z);
        }
    }
}
