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

using JetEazy.Match;
using JetEazy.QMath;
using JetEazy.Transform;
using JetEazy.Utils;
using LaserAlignDX.OPSpace.RecipeSpace;
using LeTian.AoiLib;
using System;
using ErrCodes = LaserAlignDX.Mvc.Model.ErrCodes;


namespace LaserAlignDX.Model.Coords
{
    /// <summary>
    /// Traveller106 專案 的 所有座標系
    /// </summary>
    public partial class TravellerTransforms : IDisposable
    {
        #region CONFIG
        public const int N_CALIB_POINTS = 4;
        static bool OPT_USING_X_CAM_GRID = false;
        #endregion

        #region PRIVATE_GLOBAL_TRANSFORM_MEMBERS
        PlcGridPoints _calibPlcGrid = new PlcGridPoints();
        QTransform[] _transforms = new QTransform[]
        {
            new QTransform("C1_P", "pix", "mm"),
            new QTransform("C1_M1S1", "pix", "mm"),
            new QTransform("C1_M1S2", "pix", "mm"),
            new QTransform("C2_P", "pix", "mm"),
            new QTransform("C2_M2S1", "pix", "mm"),
            new QTransform("C2_M2S2", "pix", "mm"),
        };
        int getIndex(CarrierEnum C, SuckerRowEnum S)
        {
            return (int)C * 3 + (int)S + 1;
        }
        int getIndex(CarrierEnum C)
        {
            return (int)C * 3;
        }
        #endregion

        #region SINGLETON
        static TravellerTransforms _instance;
        TravellerTransforms()
        {
            QVector.Percision = 12;
        }
        #endregion

        public static TravellerTransforms Instance
        {
            get
            {
                if(_instance == null)
                    _instance = new TravellerTransforms();
                return _instance;
            }
        }
        public void Dispose()
        {
            foreach (var trf in _transforms)
                trf?.Dispose();
        }

        public QTransform GetCameraMotorTransform(CarrierEnum C, SuckerRowEnum S)
        {
            int index = getIndex(C, S);
            return _transforms[index];
        }
        public QTransform GetCameraPhysicTransform(CarrierEnum C)
        {
            int index = getIndex(C);
            return _transforms[index];
        }
        public QTransform this[CarrierEnum C, SuckerRowEnum S]
        {
            get => GetCameraMotorTransform(C, S);
        }
        public QTransform this[CarrierEnum C]
        {
            get => GetCameraPhysicTransform(C);
        }
        
        /// <summary>
        /// 設定 全域校正 P座標 (PLC) 格點
        /// </summary>
        public PlcGridPoints ConfigGlobalCalibPlcGrid(int rows, int cols, double pitchX, double pitchY)
        {
            _calibPlcGrid = new PlcGridPoints(rows, cols, pitchX, pitchY);
            return _calibPlcGrid;
        }

        /// <summary>
        /// 更新 全域校正 C座標 (Camera) 格點
        /// </summary>
        public void UpdateCalibPoints(CarrierEnum C, SuckerRowEnum S, EzBlocsGrid calibCamGrid)
        {
            if (calibCamGrid == null)
                return;

            //(0) Camera Coords 四角點
            int r = calibCamGrid.Rows - 1;
            int c = calibCamGrid.Cols - 1;
            var camCornerPts = new[]
            {
                calibCamGrid[0,0].Center,
                calibCamGrid[0,c].Center,
                calibCamGrid[r,c].Center,
                calibCamGrid[r,0].Center,
            };


            //(1) 設定 線掃相機 到 馬達 (Carrier + Sucker) 座標轉換 的 校正點位
            if (true)
            {
                var transCameraToMotor = this.GetCameraMotorTransform(C, S);
                var trfCorners = (ICalibCornerPoints)transCameraToMotor;
                var motorPts = trfCorners.GetAll(isSrc: false);
                for (int i = 0; i < camCornerPts.Length; i++)
                {
                    var camCoord = camCornerPts[i];
                    var motorCoord = motorPts[i];
                    trfCorners.Set(i, camCoord, motorCoord);
                }
            }

            //(2) 全域校正之 plc 格點
            var calibPlcGrid = _calibPlcGrid;

            //(*) 實驗
            if (false)
            {
                //int r0 = r - camGrid.Rows / 4;
                int r0 = 3;
                camCornerPts = new[]
                {
                    calibCamGrid[r0,0].Center,
                    calibCamGrid[r0,c].Center,
                    calibCamGrid[r,c].Center,
                    calibCamGrid[r,0].Center,
                };
                    var phyCornerPts = new[]
                    {
                    calibPlcGrid[r0,0],
                    calibPlcGrid[r0,c],
                    calibPlcGrid[r,c],
                    calibPlcGrid[r,0]
                };

                var transCameraToPhysic = this.GetCameraPhysicTransform(C);
                var trfCorners = (ICalibCornerPoints)transCameraToPhysic;
                for (int i = 0; i < camCornerPts.Length; i++)
                {
                    trfCorners.Set(i, camCornerPts[i], phyCornerPts[i]);
                }
                return;
            }

            //(3) 設定 線掃相機 到 Physical (PLC grid) 座標轉換 的 校正點位
            if (true)
            {
                var camPts = toCalibGrid(calibCamGrid);
                var plcPts = toCalibGrid(calibPlcGrid);
                var transCP = this.GetCameraPhysicTransform(C);
                var trfGridPoints = transCP.GetCalibGridPoints();
                trfGridPoints.SetAll(camPts, plcPts);
            }
        }

        /// <summary>
        /// 建置 全部 座標轉換 系統
        /// </summary>
        public void BuildAll()
        {
            foreach (var trf in _transforms)
            {
                if (trf == null) continue;
                trf?.Build();
                trf.CheckBuildCondition(out double det1, out double det2);
                GaUtil.LOG($"MATRIX_{trf.Name} det1 = {det1:0.000000}");
                GaUtil.LOG($"MATRIX_{trf.Name} det2 = {det2:0.000000}");
            }
        }

        public void Load(string iniFileName)
        {
            if (!System.IO.File.Exists(iniFileName))
            {
                this.LoadGaaraIniFile();
                return;
            }

            LtDebug.LOG.Debug($"載入 [校正參數 (Trf)] {iniFileName}");
            _calibPlcGrid.Load(iniFileName, "GlobalCalibPlcGrid");
            foreach (var trf in _transforms)
            {
                trf.Load(iniFileName);
            }
        }

        public void Save(string iniFileName)
        {
            LtDebug.LOG.Debug($"寫入 [校正參數 (Trf)] {iniFileName}");
            _calibPlcGrid.Save(iniFileName, "GlobalCalibPlcGrid");
            foreach (var trf in _transforms)
            {
                trf.Save(iniFileName);
            }
        }

        #region PRIVATE_FUNCTIONS
        QVector[,] toCalibGrid(EzBlocsGrid camGrid)
        {
            int rows = camGrid.Rows;
            int cols = camGrid.Cols;
            var pts = new QVector[rows, cols];
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    pts[r, c] = camGrid[r, c].Center;
            return pts;
        }
        QVector[,] toCalibGrid(PlcGridPoints plcGrid)
        {
            int rows = plcGrid.Rows;
            int cols = plcGrid.Cols;
            var pts = new QVector[rows, cols];
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    pts[r, c] = plcGrid[r, c];
            return pts;
        }
        #endregion
    }


    partial class TravellerTransforms
    {
        #region PRIVATE_RUNTIME_DATA
        PlcGridPoints _runtimePlcGrid = null;
        EzBlocsGrid _runtimeCamGridC1 = null;   //保留
        EzBlocsGrid _runtimeCamGridC2 = null;   //保留
        #endregion

        /// <summary>
        /// 設定 Runtime 跑線時期 P座標 (PLC) 格點
        /// </summary>
        public void SetRuntimePlcGrid(PlcGridPoints plcGrid, EzBlocsGrid camGrid1 = null, EzBlocsGrid camGrid2 = null)
        {
            _runtimePlcGrid = plcGrid;
            _runtimeCamGridC1 = camGrid1;
            _runtimeCamGridC2 = camGrid2;
        }

        #region RESERVED_FUNCTIONS
        /// <summary>
        /// 設定 Runtime 跑線時期 各別載台 C座標 (Camera) 格點
        /// </summary>
        private void SetCameraGrid(CarrierEnum C, EzBlocsGrid camGrid)
        {
            if(C== CarrierEnum.C1)
                _runtimeCamGridC1 = camGrid;
            else
                _runtimeCamGridC2 = camGrid;
        }
        #endregion

        /// <summary>
        /// 取出 Runtime 跑線時期 各別載台 PLC 所需要的參考點 座標數據
        /// </summary>
        public ErrCodes GetCoordsRef(CarrierEnum C, out QVector camCoord, out QVector worldCoordSucker1, out QVector worldCoordSucker2, out string errMsg)
        {
            ErrCodes errCode = ErrCodes.OK;
            camCoord = new QVector(0, 0);
            worldCoordSucker1 = new QVector(0, 0);
            worldCoordSucker2 = new QVector(0, 0);

            //(1) 檢查 Transforms 數據狀態
            (errCode, errMsg) = checkTransforms(C);
            if (errCode != ErrCodes.OK)
                return errCode;

            //(2) 檢查 RuntimePlcGrid
            (errCode, errMsg) = checkRuntimePlcGrid();
            if (errCode != ErrCodes.OK)
                return errCode;

            //(3) 取出 Global Transforms
            var transCP = this.GetCameraPhysicTransform(C);
            var transCM1 = this.GetCameraMotorTransform(C, SuckerRowEnum.S1);
            var transCM2 = this.GetCameraMotorTransform(C, SuckerRowEnum.S2);

            //(4) 計算 (簡單 使用 馬達座標)
            QVector runtimeWorldPt = _runtimePlcGrid[0, 0];
            camCoord = transCP.InvTrans(runtimeWorldPt);
            var s1_motor_coord = transCM1.Trans(camCoord);
            var s2_motor_coord = transCM2.Trans(camCoord);
            worldCoordSucker1 = s1_motor_coord;
            worldCoordSucker2 = s2_motor_coord;

            //(5) 使用參數中的 camGrid
            if (OPT_USING_X_CAM_GRID)
            {
                var xCamGrid = C == CarrierEnum.C1 ? _runtimeCamGridC1 : _runtimeCamGridC2;
                (errCode, errMsg) = checkCameraGrid(C, xCamGrid);
                var camPt = xCamGrid?.Get(0, 0)?.Center;
                if (camPt == null || xCamGrid == null)
                    return ErrCodes.NO_CAMERA_GRID;
                s1_motor_coord = transCM1.Trans(camPt);
                s2_motor_coord = transCM2.Trans(camPt);
                worldCoordSucker1 = s1_motor_coord;
                worldCoordSucker2 = s2_motor_coord;
            }

            //(6) Return value
            errMsg = null;
            return ErrCodes.OK;
        }

        /// <summary>
        /// 計算 PLC 補償量
        /// </summary>
        public (QVector, QVector) CalcPlcDetailCompensation(CarrierEnum C, SuckerRowEnum S, QVector camPt, int rowId, int colId)
        {
            if (_runtimePlcGrid == null)
                return (new QVector(0, 0), new QVector(0, 0));

            // P 目標值 (world)
            var targetWorldPt = _runtimePlcGrid[rowId, colId];

            // 目標值 轉換至 Camera Coords
            ITransform transCP = GetCameraPhysicTransform(C);
            var targetCamPt = transCP.InvTrans(targetWorldPt);

            // 目標值 轉換至 Motor Coords
            ITransform transCM = GetCameraMotorTransform(C, S);
            var targetMotorPt = transCM.Trans(targetCamPt);

            // 像測現值 轉換至 Motor Coords
            var motorPt = transCM.Trans(camPt);

            // 像測現值 轉換至 World Coords
            var curWorldPt = transCP.Trans(camPt);

            // 自我轉換的誤差
            var err = targetWorldPt - curWorldPt;

            // 補償量
            var delta = targetMotorPt - motorPt;

            return (delta, err);
        }

        /// <summary>
        /// 計算 PLC 補償量
        /// </summary>
        (QVector, QVector) CalcPlcCompensation_000(CarrierEnum C, QVector camPt, int rowId, int colId)
        {
            if (_runtimePlcGrid == null)
                return (new QVector(0, 0), new QVector(0, 0));

            //(1) 目標值 P (World Coords)
            var targetWorldPt = _runtimePlcGrid[rowId, colId];

            //(2) 目標值 P 轉換至 Camera Coords
            ITransform transCP = GetCameraPhysicTransform(C);
            var targetCamPt = transCP.InvTrans(targetWorldPt);

            //(3) 像測現值 轉換至 World Coords
            var curWorldPt = transCP.Trans(camPt);

            //(4) World Coordinates 的差異
            var worldError = targetWorldPt - curWorldPt;

            //(5) 目標值 P 轉換至 Motor Coords
            ITransform transCM1 = GetCameraMotorTransform(C, SuckerRowEnum.S1);
            ITransform transCM2 = GetCameraMotorTransform(C, SuckerRowEnum.S2);
            var targetMotorPt1 = transCM1.Trans(targetCamPt);
            var targetMotorPt2 = transCM2.Trans(targetCamPt);

            //(6) 像測現值 轉換至 Motor Coords
            var motorPt1 = transCM1.Trans(camPt);
            var motorPt2 = transCM2.Trans(camPt);

            //(7) 馬達補償量
            var delta1 = targetMotorPt1 - motorPt1;
            var delta2 = targetMotorPt2 - motorPt2;
            var motorDelta = (delta1.NormLengthSQ < delta2.NormLengthSQ) ? delta1 : delta2;

            return (motorDelta, worldError);
        }

        /// <summary>
        /// 計算 PLC 補償量
        /// </summary>
        (QVector, QVector) CalcPlcCompensation_001(CarrierEnum C, QVector camPt, int rowId, int colId)
        {
            if (!OPT_USING_X_CAM_GRID || _runtimePlcGrid == null)
                return CalcPlcCompensation_000(C, camPt, rowId, colId);

            //(0) CamBase and SuckerBase
            var err = GetCoordsRef(C, out var cam_base, out var sucker1_base, out var sucker2_base, out var errMsg);
            if (err != ErrCodes.OK)
                return CalcPlcCompensation_000(C, camPt, rowId, colId);

            ITransform transCP = GetCameraPhysicTransform(C);
            ITransform transCM1 = GetCameraMotorTransform(C, SuckerRowEnum.S1);
            ITransform transCM2 = GetCameraMotorTransform(C, SuckerRowEnum.S2);

            //(1) 目標值 P (World Coords)
            var targetWorldPt = _runtimePlcGrid[rowId, colId];
            var targetWorldPt1 = targetWorldPt + sucker1_base;
            var targetWorldPt2 = targetWorldPt + sucker2_base;

            //(2) 像測現值 轉換至 World Coords
            var curWorldPt = transCP.Trans(camPt);
            var curWorldPt1 = curWorldPt + sucker1_base;
            var curWorldPt2 = curWorldPt + sucker2_base;

            //(3) World Coordinates 的差異
            var worldError1 = targetWorldPt1 - curWorldPt1;
            var worldError2 = targetWorldPt2 - curWorldPt2;

            //(4) 馬達補償量
            var delta1 = worldError1;
            var delta2 = worldError2;
            //var motorDelta = (delta1.NormLengthSQ < delta2.NormLengthSQ) ? delta1 : delta2;

            return (delta2, delta1);
        }

        /// <summary>
        /// 計算 PLC 補償量
        /// </summary>
        public (QVector, QVector) CalcPlcCompensation(CarrierEnum C, QVector camPt, int rowId, int colId)
        {
            if(OPT_USING_X_CAM_GRID)
                return CalcPlcCompensation_001(C, camPt, rowId, colId);
            else
                return CalcPlcCompensation_000(C, camPt, rowId, colId);
        }

        #region CHECK_FUNCTIONS
        internal (ErrCodes, string) checkCameraGrid(CarrierEnum carrierID, EzBlocsGrid camGrid)
        {
            var errCode = ErrCodes.OK;
            string errMsg = null;

            if (camGrid == null)
            {
                errCode = ErrCodes.NO_CAMERA_GRID;
                errMsg = $"[{JetEazy.QxNums.GetEnumDescription(carrierID)}] "
                       + JetEazy.QxNums.GetEnumDescription(errCode)
                       + " !";
            }
            else if (camGrid.Rows < 2 || camGrid.Cols < 2)
            {
                errCode = ErrCodes.LOW_GRID_ROWS_COLS;
                errMsg = $"[{JetEazy.QxNums.GetEnumDescription(carrierID)}] "
                       + JetEazy.QxNums.GetEnumDescription(errCode)
                       + $"\n\r rows={camGrid.Rows}, cols={camGrid.Rows} !";
            }

            return (errCode, errMsg);
        }
        internal (ErrCodes, string) checkTransforms(CarrierEnum carrierID)
        {
            var transCP = this.GetCameraPhysicTransform(carrierID);
            var transCM1 = this.GetCameraMotorTransform(carrierID, SuckerRowEnum.S1);
            var transCM2 = this.GetCameraMotorTransform(carrierID, SuckerRowEnum.S2);

            var errCode = ErrCodes.OK;
            string errMsg = "";

            if (transCP == null)
            {
                errCode = ErrCodes.NO_CALIB_TRANSFORM;
                errMsg = JetEazy.QxNums.GetEnumDescription(carrierID);
            }
            else
            {
                if (transCM1 == null)
                {
                    errCode = ErrCodes.NO_CALIB_TRANSFORM;
                    errMsg += JetEazy.QxNums.GetEnumDescription(carrierID) + " " + JetEazy.QxNums.GetEnumDescription(SuckerRowEnum.S1) + "\n\r";
                }

                if (transCM2 == null)
                {
                    errCode = ErrCodes.NO_CALIB_TRANSFORM;
                    errMsg += JetEazy.QxNums.GetEnumDescription(carrierID) + " " + JetEazy.QxNums.GetEnumDescription(SuckerRowEnum.S2) + "\n\r";
                }
            }

            if (errCode == ErrCodes.OK)
            {
                return (errCode, null);
            }
            else
            {
                errMsg += "\n\r" + JetEazy.QxNums.GetEnumDescription(errCode) + " !";
                return (errCode, errMsg);
            }
        }
        internal (ErrCodes, string) checkRuntimePlcGrid()
        {
            var errCode = ErrCodes.OK;
            string errMsg = null;

            if (_runtimePlcGrid == null)
            {
                errCode = ErrCodes.NO_RUNTIME_PLC_GRID;
                errMsg = JetEazy.QxNums.GetEnumDescription(errCode) + " !";
            }
            else if (_runtimePlcGrid.Rows < 2 || _runtimePlcGrid.Cols < 2)
            {
                errCode = ErrCodes.LOW_GRID_ROWS_COLS;
                errMsg = JetEazy.QxNums.GetEnumDescription(errCode) + " !";
                errMsg = "Runtime PLC Grid\n\r" + errMsg;
            }
            if (errCode == ErrCodes.OK)
            {
                return (errCode, null);
            }
            else
            {
                return (errCode, errMsg);
            }
        }
        #endregion
    }
}
