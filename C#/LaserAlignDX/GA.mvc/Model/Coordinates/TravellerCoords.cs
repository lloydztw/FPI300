#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-08-13 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion


using JetEazy.QMath;

namespace LaserAlignDX.Model.Coords
{
    /// <summary>
    /// Traveller106 專案 的 所有座標系
    /// </summary>
    public static class TravellerCoords
    {
        public const int CAMERA_ORDER = 10000;
        public const int MOTOR_ORDER = 1000;

        /// <summary>
        /// P (Physic) 座標系
        /// </summary>
        public class P : QCoord
        {
            public override int ORDER => MOTOR_ORDER;
            public override string UNIT => "mm";
            public P(double x, double y) : base(x, y)
            {
            }
            public P(QVector v) : base(v)
            {
            }
            public P()
            {
            }
        }

        /// <summary>
        /// C (Camera) 相機座標系 (抽象)
        /// </summary>
        public class C : QCoord
        {
            public override int ORDER => CAMERA_ORDER;
            public override string UNIT => "pix";
            public override string ToString()
            {
                string str = GetType().Name.Replace("Coord", "") + $" ({(int)X}, {(int)Y})";
                return str;
            }

            public C(double x, double y) : base(x, y)
            {
            }
            public C(QVector v) : base(v)
            {
            }
            public C()
            {
            }
        }

        /// <summary>
        /// C1 (Camera1) 線掃座標系 載台1
        /// </summary>
        public class C1 : C
        {
            public C1(double x, double y) : base(x, y)
            {
            }
            public C1(QVector v) : base(v)
            {
            }
            public C1()
            {
            }
        }

        /// <summary>
        /// C2 (Camera2) 線掃座標系 載台1
        /// </summary>
        public class C2 : C
        {
            public C2(double x, double y) : base(x, y)
            {
            }
            public C2(QVector v) : base(v)
            {
            }
            public C2()
            {
            }
        }

        /// <summary>
        /// M (Motor) 馬達座標系 (抽象)
        /// </summary>
        public class M : QCoord
        {
            public override int ORDER => MOTOR_ORDER;
            public M(double x, double y) : base(x, y)
            {
            }
            public M(QVector v) : base(v)
            {
            }
            public M()
            {
            }
        }

        /// <summary>
        /// M11 馬達座標系 (載台1 + 吸嘴排1)
        /// </summary>
        public class M1S1 : M
        {
            public M1S1(double x, double y) : base(x, y)
            {
            }
            public M1S1(QVector v) : base(v)
            {
            }
            public M1S1()
            {
            }
        }

        /// <summary>
        /// M12 馬達座標系 (載台1 + 吸嘴排2)
        /// </summary>
        public class M1S2 : M
        {
            public M1S2(double x, double y) : base(x, y)
            {
            }
            public M1S2(QVector v) : base(v)
            {
            }
            public M1S2()
            {
            }
        }

        /// <summary>
        /// M21 馬達座標系 (載台2 + 吸嘴排1)
        /// </summary>
        public class M2S1 : M
        {
            public M2S1(double x, double y) : base(x, y)
            {
            }
            public M2S1(QVector v) : base(v)
            {
            }
            public M2S1()
            {
            }
        }

        /// <summary>
        /// M22 馬達座標系 (載台2 + 吸嘴排2)
        /// </summary>
        public class M2S2 : M
        {
            public M2S2(double x, double y) : base(x, y)
            {
            }
            public M2S2(QVector v) : base(v)
            {
            }
            public M2S2()
            {
            }
        }
    }
}
