using EzAoiEmptyTrayInspector.Model;
using JetEazy.Match;
using JetEazy.QMath;
using JetEazy.Transform;
using LaserAlignDX.Mvc.Model;
using System;
using System.Collections.Generic;

namespace LaserAlignDX.Model.Coords
{
    public interface ITravellerTransforms : ITravellerTransformsCalib, IDisposable
    {
        string Name { get; }

        /// <summary>
        /// 取得 相機 與 馬達座標 之 轉換
        /// </summary>
        ITransform GetCameraMotorTransform(CarrierEnum C, SuckerRowEnum S);

        /// <summary>
        /// 取得 相機 與 世界座標 之 轉換
        /// </summary>
        ITransform GetCameraPhysicTransform(CarrierEnum C);

        /// <summary>
        /// 理想格點 (World) (P座標)
        /// </summary>
        IWorldGridPoints GetWorldGridPoints();

        /// <summary>
        /// 相機工作距離 
        /// </summary>
        double CameraWorkDist { get; set; }

        #region 建構時期_函式群

        /// <summary>
        /// 規劃 理想格點 (World) (P座標) 
        /// </summary>
        IWorldGridPoints ConfigWorldGridPoints(int rows, int cols, double pitchX, double pitchY);

        /// <summary>
        /// 建立 所有座標 轉換公式
        /// </summary>
        void BuildAll();

        #endregion

        #region 跑線時期_函數群

        /// <summary>
        /// 取出 Runtime 跑線時期 標準格點 座標數據
        /// </summary>
        (ErrorCodes, string) GetNodeCoords(CarrierEnum C, int rowId, int colId, out QVector camCoord, out QVector worldCoord, out QVector s1MotorCoord, out QVector s2MotorCoord);

        /// <summary>
        /// 取出 Runtime 跑線時期 各別載台 PLC 所需要的參考點 座標數據 (對應 row=0, col=0)
        /// </summary>
        (ErrorCodes, string) GetCoordsRef(CarrierEnum C, out QVector camCoord, out QVector s1MotorCoord, out QVector s2MotorCoord);

        /// <summary>
        /// 計算 PLC 補償量
        /// </summary>
        /// <returns>(馬達補償量, 世界座標差值)</returns>
        (QVector, QVector) CalcPlcCompensation(CarrierEnum C, QVector camPt, int rowId, int colId);

        #endregion

        #region 查核_函式群

        (ErrorCodes, string) checkCameraGrid(CarrierEnum carrierID, EzBlocsGrid camGrid);
        (ErrorCodes, string) checkTransforms(CarrierEnum carrierID);

        #endregion

        #region 檔案載入與保存

        void Load(string iniFileName);
        void Save(string iniFileName);

        #endregion
    }


    public interface ITravellerTransformsCalib
    {
        /// <summary>
        /// 取得 Camera-To-World 校正格點 (相機座標) (for 多點校正)
        /// </summary>
        EzBlocsGrid GetCalibCamGrid(CarrierEnum C);

        /// <summary>
        /// 設定 Camera-To-World 校正格點 (相機座標) (for 多點校正)
        /// </summary>
        bool SetCalibCamGrid(CarrierEnum C, EzBlocsGrid camGrid, IList<JxTraySegItem> segsList = null);

        /// <summary>
        /// 設定 Camera-To-Motor 校正墨點 (順時針四角: 左上, 右上, 右下, 左下)
        /// </summary>
        ErrorCodes SetCalibMotorCoords(CarrierEnum C, SuckerRowEnum S, QVector[] inkMarks, QVector[] motorCoords);
    }
}