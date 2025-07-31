using JetEazy.QMath;
using System;


namespace TestDemo.LaserDot
{
    using POINT = System.Drawing.PointF;
    //using POINT = OpenCvSharp.Point2d;

    public class LaserDotCoordinate
    {
        #region PRIVATE_CACHE_DATA
        QVector _vectorK1;
        QVector _vectorK2;
        double _lenK1;
        double _lenK2;
        double _A;
        double _CosA;
        double _SinA;
        #endregion


        /// <summary>
        /// 先用 "K物件" 的 k0, k1, k2 世界座標點位, 
        /// 預先計算好 快取數據 (cache data)
        /// </summary>
        public void SetKeyPoints(POINT k0, POINT k1, POINT k2)
        {
            // K物件 第一條線的向量 vectorK1
            _vectorK1 = BuildVector(k0, k1);
            // K物件 第二條線的向量 vectorK2
            _vectorK2 = BuildVector(k0, k2);

            // 計算向量的長度
            _lenK1 = _vectorK1.NormLength;
            _lenK2 = _vectorK2.NormLength;

            // 檢查長度不能接近 0 !!!
            System.Diagnostics.Trace.Assert(_lenK1 > 1e-6, "k0 與 k1 兩點太靠近!");
            System.Diagnostics.Trace.Assert(_lenK2 > 1e-6, "k0 與 k2 兩點太靠近!");

            // 計算向量夾角 A
            double dotProduct = _vectorK1 * _vectorK2;
            // 計算夾角的cos值
            double cosValue = dotProduct / (_lenK1 * _lenK2);
            // 確保 cosValue 在 [-1, 1] 範圍內，避免浮點數精度問題
            cosValue = Math.Max(-1, Math.Min(1, cosValue));
            // 計算角度（以弧度表示）
            _A = Math.Acos(cosValue);

            // 保存 Cos(A) 與 Sin(A)
            _CosA = Math.Cos(_A);
            _SinA = Math.Sin(_A);
        }


        /// <summary>
        /// 套用各種不同 Q物件 上的 q0, q1 世界座標點位, 來求取其對應的 q2 與 phi.
        /// </summary>
        public void CalcPointQ2(POINT q0, POINT q1, out POINT q2, out double phi)
        {
            // Q物件 第一條線的向量 vectorQ1
            QVector vectorQ1 = BuildVector(q0, q1);

            // 計算向量 Q1 的長度
            double lenQ1 = vectorQ1.NormLength;

            // 檢查長度不能接近 0 !!!
            System.Diagnostics.Trace.Assert(lenQ1 > 1e-6, "q0 與 q1 兩點太靠近!");

            // 計算旋轉 vectorQ1 後的新向量 (x', y')
            double x = vectorQ1.X;
            double y = vectorQ1.Y;
            double newX = x * _CosA + y * _SinA;
            double newY = -x * _SinA + y * _CosA;
            QVector vectorQ2 = new QVector(newX, newY);

            // Q2 單位向量
            vectorQ2 = vectorQ2 / vectorQ2.NormLength;

            // 縮放長度
            double lenQ2 = _lenK2 / _lenK1 * lenQ1;
            vectorQ2 = vectorQ2 * lenQ2;

            // 由 q0 與 vectorQ2 建立 q2
            q2 = BuildPoint(q0, vectorQ2);

            // 計算 theta （以弧度表示）
            // theta = Math.Atan2(q2.Y, q2.X);

            // 計算 phi （以弧度表示）
            var angleK1 = Math.Atan2(_vectorK1.Y, _vectorK1.X);
            var angleQ1 = Math.Atan2(vectorQ1.Y, vectorQ1.X);
            phi = angleQ1 - angleK1;

            //>>> radian to degree
            //>>> phi = phi * 180 / Math.PI;
            phi = phi * 180 / Math.PI;
        }


        /// <summary>
        /// 第一個演示版本
        /// </summary>
        public void CalcPointQ2(
                        POINT k0, POINT k1, POINT k2,
                        POINT q0, POINT q1,
                        out POINT q2,
                        out double phi)
        {
            // K物件 第一條線的向量 vectorK1
            QVector vectorK1 = BuildVector(k0, k1);
            // K物件 第二條線的向量 vectorK2
            QVector vectorK2 = BuildVector(k0, k2);
            // Q物件 第一條線的向量 vectorQ1
            QVector vectorQ1 = BuildVector(q0, q1);

            // 計算向量的長度
            double lenK1 = vectorK1.NormLength;
            double lenK2 = vectorK2.NormLength;
            double lenQ1 = vectorQ1.NormLength;

            //// 檢查長度不能接近 0 !!!
            //System.Diagnostics.Trace.Assert(lenK1 > 1e-6, "k0 與 k1 兩點太靠近!");
            //System.Diagnostics.Trace.Assert(lenK2 > 1e-6, "k0 與 k2 兩點太靠近!");
            //System.Diagnostics.Trace.Assert(lenQ1 > 1e-6, "q0 與 q1 兩點太靠近!");

            // 計算向量夾角 A
            double dotProduct = vectorK1 * vectorK2;
            // 計算夾角的cos值
            double cosValue = dotProduct / (lenK1 * lenK2);
            // 確保 cosTheta 在 [-1, 1] 範圍內，避免浮點數精度問題
            cosValue = Math.Max(-1, Math.Min(1, cosValue));
            // 計算角度（以弧度表示）
            double A = Math.Acos(cosValue);

            // 叉积判断方向，如果 b 在 a 的左边，取反
            if (vectorK1.x * vectorK2.y - vectorK1.y * vectorK2.x > 0)
            {
                A = -A;
            }

            // 計算旋轉 vectorQ1 後的新向量 (x', y')
            double x = vectorQ1.X;
            double y = vectorQ1.Y;
            double newX = x * Math.Cos(A) + y * Math.Sin(A);
            double newY = -x * Math.Sin(A) + y * Math.Cos(A);
            QVector vectorQ2 = new QVector(newX, newY);

            // Q2 單位向量
            vectorQ2 = vectorQ2 / vectorQ2.NormLength;

            // 縮放長度
            double lenQ2 = lenK2 / lenK1 * lenQ1;
            vectorQ2 = vectorQ2 * lenQ2;

            // 由 q0 與 vectorQ2 算出 q2
            newX = q0.X + vectorQ2.X;
            newY = q0.Y + vectorQ2.Y;
            q2 = new POINT((float)newX, (float)newY);

            //計算 theta （以弧度表示）
            double theta = Math.Atan2(q2.Y, q2.X);

            // 計算 phi （以弧度表示）
            var angleK1 = Math.Atan2(vectorK1.Y, vectorK1.X);
            var angleQ1 = Math.Atan2(vectorQ1.Y, vectorQ1.X);
            phi = angleQ1 - angleK1;

            ////>>> radian to degree
            ////>>> phi = phi * 180 / Math.PI;
            phi = phi * 180 / Math.PI;
            //// 叉积判断方向，如果 b 在 a 的左边，取反
            //if (vectorK1.x * vectorK2.y - vectorK1.y * vectorK2.x > 0)
            //{
            //    phi = -phi;
            //}
        }


        /// <summary>
        /// 使用 p1, p2 建立向量
        /// </summary>
        public QVector BuildVector(POINT p1, POINT p2)
        {
            return new QVector(p2.X - p1.X, p2.Y - p1.Y);
        }


        /// <summary>
        /// 以 q0 為基點, 順著 vector 方向延伸, 建立新點
        /// </summary>
        public POINT BuildPoint(POINT q0, QVector vector)
        {
            double newX = q0.X + vector.X;
            double newY = q0.Y + vector.Y;
            return new POINT((float)newX, (float)newY);
        }
    }
}

