using JetEazy.Match;
using JetEazy.QMath;
using QMicroChipTransform = LaserAlignDX.Model.Coords.V33.QMicroChipTransform;
using TravellerTransforms = LaserAlignDX.Model.Coords.V33.TravellerTransforms;

namespace LaserAlignDX.Model.Coords
{
    internal class TravellerTransformFactory
    {
        #region CONSTS
        public static bool OPT_CALIB_GRID_USING_MOTOR_COORD => TravellerTransforms.OPT_CALIB_GRID_USING_MOTOR_COORD;
        public static int N_CALIB_POINTS => TravellerTransforms.N_CALIB_POINTS;
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
            TravellerTransforms.DisposeAll(name, "$CommonBase$");
        }
    }
}
