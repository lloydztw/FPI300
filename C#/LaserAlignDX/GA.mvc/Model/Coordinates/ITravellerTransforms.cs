using JetEazy.Match;
using JetEazy.QMath;
using JetEazy.Transform;
using LaserAlignDX.Mvc.Model;
using System;

namespace LaserAlignDX.Model.Coords
{
    public interface ITravellerTransforms : IDisposable
    {
        string Name { get; }

        /// <summary>
        /// 取得 相機 與 馬達座標 之 轉換
        /// </summary>
        QTransform GetCameraMotorTransform(CarrierEnum C, SuckerRowEnum S);

        /// <summary>
        /// 取得 相機 與 世界座標 之 轉換
        /// </summary>
        QTransform GetCameraPhysicTransform(CarrierEnum C);

        #region 校正時期_函式群
        /// <summary>
        /// 規劃 校正格點 (PLC 座標) (P座標) 
        /// </summary>
        PlcGridPoints ConfigGlobalCalibPlcGrid(int rows, int cols, double pitchX, double pitchY);

        /// <summary>
        /// 設定 校正格點 (相機座標)
        /// </summary>
        void SetCalibCameraGrid(CarrierEnum C, EzBlocsGrid camGrid);

        /// <summary>
        /// 更新 校正格點 (相機座標)
        /// </summary>
        void UpdateCalibPoints(CarrierEnum C, SuckerRowEnum S, EzBlocsGrid calibCamGrid);

        /// <summary>
        /// 取得 校正格點 (相機座標)
        /// </summary>
        EzBlocsGrid GetCalibCameraGrid(CarrierEnum C);

        /// <summary>
        /// 取得 校正格點 (PLC 座標)
        /// </summary>
        PlcGridPoints getCalibPlcGrid();

        /// <summary>
        /// 建立 所有座標 轉換公式
        /// </summary>
        void BuildAll();
        #endregion

        #region 跑線時期_函數群

        /// <summary>
        /// 設定 Runtime 跑線時期 P座標 (PLC) 格點
        /// </summary>
        void UpdateRuntimePlcGrid(PlcGridPoints plcGrid, EzBlocsGrid camGrid1 = null, EzBlocsGrid camGrid2 = null);

        /// <summary>
        /// 取出 Runtime 跑線時期 標準格點 座標數據
        /// </summary>
        (ErrorCodes, string) GetNodeCoords(CarrierEnum C, int rowId, int colId, out QVector camCoord, out QVector worldCoord, out QVector s1MotorCoord, out QVector s2MotorCoord);

        /// <summary>
        /// 取出 Runtime 跑線時期 各別載台 PLC 所需要的參考點 座標數據
        /// </summary>
        (ErrorCodes, string) GetCoordsRef(CarrierEnum C, out QVector camCoord, out QVector s1MotorCoord, out QVector s2MotorCoord);

        /// <summary>
        /// 計算 PLC 補償量
        /// </summary>
        (QVector, QVector) CalcPlcCompensation(CarrierEnum C, QVector camPt, int rowId, int colId);

        #endregion

        #region 查核_函式群
        (ErrorCodes, string) checkCameraGrid(CarrierEnum carrierID, EzBlocsGrid camGrid);
        (ErrorCodes, string) checkTransforms(CarrierEnum carrierID);
        (ErrorCodes, string) checkRuntimePlcGrid();
        #endregion

        #region INI_FILE_FUNCTIONS
        void Load(string iniFileName);
        void Save(string iniFileName);
        #endregion

        /// <summary>
        /// 根據 個別參數 像測找到的 "格點陣列" 線性遷移 生成新的 座標轉換系統
        /// </summary>
        ITravellerTransforms BuildLinearMigration(string name, CarrierEnum carrierID, EzBlocsGrid camRegionsArray);
    }
}