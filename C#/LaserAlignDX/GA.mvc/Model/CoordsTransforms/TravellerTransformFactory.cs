#region AUTHOR
/*
 * 
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2026-04-02 Added V35
 *      2025-08-13 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.QMath;

#if (OPT_TRANSFORM_V36)
using QMicroChipTransform = LaserAlignDX.Model.Coords.V36.QMicroChipTransform;
using TravellerTransforms = LaserAlignDX.Model.Coords.V36.TravellerTransforms;
#else
using QMicroChipTransform = LaserAlignDX.Model.Coords.V33.QMicroChipTransform;
using TravellerTransforms = LaserAlignDX.Model.Coords.V33.TravellerTransforms;
#endif

namespace LaserAlignDX.Model.Coords
{
    public class TravellerTransformFactory
    {
        #region NLOG
        internal static NLog.ILogger _LOG => TravellerTransforms._LOG;
        #endregion

        #region CONSTS
        /// <summary>
        /// 馬達校正點數 (使用者所需要輸入的馬達座標點數)
        /// </summary>
        public static int N_CALIB_MOTOR_POINTS => TravellerTransforms.N_CALIB_MOTOR_POINTS;
        /// <summary>
        /// VER_3.3.0.0 線性遷移 使用 WORLD_COORD
        /// <br/> OPT_CALIB_GRID_USING_MOTOR_COORD == false
        /// </summary>
        public static bool OPT_CALIB_GRID_USING_MOTOR_COORD => TravellerTransforms.OPT_CALIB_GRID_USING_MOTOR_COORD;
        #endregion

        public static IMicroChipTransform CreateMicroChipTransform(string name)
        {
            return new QMicroChipTransform(name);
        }

        /// <summary>
        /// 所有參數共用基礎 的 座標轉換系統
        /// </summary>
        public static ITravellerTransforms CommonBase
        {
            get
            {
                return Instance("$CommonBase$");
            }
        }

        /// <summary>
        /// 個別參數 的 座標轉換系統
        /// </summary>
        public static ITravellerTransforms Instance(string name)
        {
            QVector.Percision = 12;
            return TravellerTransforms.Instance(name);
        }

        /// <summary>
        /// 卸載所有 座標轉換系統
        /// </summary> 
        public static void DisposeAll()
        {
            TravellerTransforms.DisposeAll();
            QMicroChipTransform.DisposeAll();
        }

        /// <summary>
        /// 只保留 name (與 $CommonBase$) (與IMicroChipTransform) 座標轉換系統 其餘都卸載
        /// </summary>
        public static void Keep(string name)
        {
            TravellerTransforms.DisposeAll(name, "$CommonBase$", "C1_Micro", "C2_Micro");
        }
    }
}
