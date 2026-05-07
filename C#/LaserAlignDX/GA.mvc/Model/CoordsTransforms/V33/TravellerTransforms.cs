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

using EzAoiEmptyTrayInspector.Model;
using JetEazy.Match;
using JetEazy.QMath;
using JetEazy.Transform;
using JetEazy.Utils;
using LaserAlignDX.Model.Coords.Support;
using LeTian.AoiLib;
using System;
using System.Collections.Generic;
using ErrorCodes = LaserAlignDX.Mvc.Model.ErrorCodes;

namespace LaserAlignDX.Model.Coords.V33
{
    /// <summary>
    /// Traveller106 專案 的 所有座標系
    /// </summary>
    internal partial class TravellerTransforms : QRegistable, IDisposable, ITravellerTransforms
    {
        #region CONFIG
        /// <summary>
        /// 馬達校正點數 (使用者所需要輸入的馬達座標點數)
        /// </summary>
        public const int N_CALIB_MOTOR_POINTS = 4;
        /// <summary>
        /// VER_3.3.0.0 線性遷移 使用 WORLD_COORD
        /// <br/> OPT_CALIB_GRID_USING_MOTOR_COORD == false
        /// </summary>
        public const bool OPT_CALIB_GRID_USING_MOTOR_COORD = false;
        #endregion

        #region NLOG
        internal static NLog.ILogger _LOG => LtDebug.LOG;
        #endregion

        #region PRIVATE_TRANSFORM_MEMBERS
        /// <summary>
        /// 理想世界格點
        /// </summary>
        IWorldGridPoints _worldGrid = new QWorldGridPoints();
        /// <summary>
        /// 相機校正點 (大校正板 抓到的 pixel 點位) (用於 CameraToWorld 座標轉換的建立)
        /// </summary>
        EzBlocsGrid[] _calibCamGrids = new EzBlocsGrid[Enum.GetValues(typeof(CarrierEnum)).Length];
        /// <summary>
        /// 各種 座標轉換 集合
        /// </summary>
        QTransform[] _transforms = new QTransform[]
        {
            new QTransform("C1_P", "pix", "mm"),        // 線掃相機C1 <--> world
            new QTransform("C1_M1S1", "pix", "mm"),     // 線掃相機C1 <--> motors (sucker 1)
            new QTransform("C1_M1S2", "pix", "mm"),     // 線掃相機C1 <--> motors (sucker 2)
            new QTransform("C2_P", "pix", "mm"),        // 線掃相機C2 <--> world
            new QTransform("C2_M2S1", "pix", "mm"),     // 線掃相機C2 <--> motors (sucker 1)
            new QTransform("C2_M2S2", "pix", "mm"),     // 線掃相機C2 <--> motors (sucker 2)
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

        #region 建構_解構_函式群
        protected TravellerTransforms(string name)
        {
            Name = name;
        }
        void IDisposable.Dispose()
        {
            foreach (var trf in _transforms)
                trf?.Dispose();

            for (int i = 0, N = _calibCamGrids.Length; i < N; i++)
            {
                _calibCamGrids[i]?.Dispose();
                _calibCamGrids[i] = null;
            }

            Unregister(this);
        }

        /// <summary>
        /// 所有參數共用基礎 的 座標轉換系統
        /// </summary>
        internal static TravellerTransforms CommonBase
        {
            get
            {
                return Instance("$CommonBase$");
            }
        }

        /// <summary>
        /// 個別參數 的 座標轉換系統
        /// </summary>
        internal static TravellerTransforms Instance(string name)
        {
            // 嘗試尋找現有物件
            if (Find(name) is TravellerTransforms instance)
                return instance;

            // 若不存在，則建立、初始化、最後才註冊
            var newObj = new TravellerTransforms(name);

            // 此處可以進行其他耗時的初始化...
            // newObj.InitializeInternalData(); 

            Register(newObj); // 確保物件已完全就緒才發佈到註冊表
            return newObj;
        }
        #endregion

        public override string Name
        {
            get;
            protected set;
        }
        public ITransform GetCameraMotorTransform(CarrierEnum C, SuckerRowEnum S)
        {
            int index = getIndex(C, S);
            return _transforms[index];
        }
        public ITransform GetCameraPhysicTransform(CarrierEnum C)
        {
            int index = getIndex(C);
            return _transforms[index];
        }
        public IWorldGridPoints GetWorldGridPoints(CarrierEnum C)
        {
            return _worldGrid;
        }
        public double CameraWorkDist
        {
            get;
            set;
        }

        #region 校正時期_函式群
        public void ConfigWorldGridPoints(CarrierEnum C, int rows, int cols, double pitchX, double pitchY)
        {
            if(_worldGrid == null)
                _worldGrid = new QWorldGridPoints(rows, cols, pitchX, pitchY);
            else
                _worldGrid.Config(rows, cols, pitchX, pitchY);
            //return _worldGrid;
        }

        public EzBlocsGrid GetCalibCamGrid(CarrierEnum C)
        {
            return _calibCamGrids[(int)C];
        }

        public bool SetCalibCamGrid(CarrierEnum C, EzBlocsGrid camGrid, IList<JxTraySegItem> segsList = null)
        {
            (var err, var errMsg) = checkCameraGrid(C, camGrid);
            if (err != ErrorCodes.OK)
            {
                _LOG.Error(errMsg + "@ SetWCalibCameraGrid");
                return false;
            }

            var old = _calibCamGrids[(int)C];

            try
            {
                _calibCamGrids[(int)C] = camGrid;

                if (_worldGrid == null)
                    return false;

                // 更新 CameraToWorld 校正點位
                var trfCameraToWorld = GetCameraPhysicTransform(C);
                updateCalibPointsToTrf(trfCameraToWorld, camGrid, _worldGrid);

                return true;
            }
            finally
            {
                if (old != camGrid)
                    old?.Dispose();
            }
        }

        public ErrorCodes SetCalibMotorCoords(CarrierEnum C, SuckerRowEnum S, QVector[] inkMarks, QVector[] motorCoords)
        {
            (var err, var errMsg) = checkMotorCoords(C, S, inkMarks, motorCoords);

            if (err != ErrorCodes.OK)
            {
                _LOG.Error($"SetCalibMotorCoords : err = {err}");
                return err;
            }
            
            var trfCM = GetCameraMotorTransform(C, S);
            var calib = trfCM.GetCalibCornerPoints();

            int N = Math.Min(4, Math.Min(inkMarks.Length, motorCoords.Length));
            for (int i = 0; i < N; i++)
            {
                calib.Set(i, inkMarks[i], motorCoords[i]);
            }

            return err;
        }

#if (OPT_RESERVED_CODE)
        /// <summary>
        /// 更新 全域校正 C座標 (Camera) 格點 (停用)
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
#endif

        void updateCalibPointsToTrf(ITransform trfCameraToWorld, EzBlocsGrid camGrid, IWorldGridPoints worldGrid)
        {
            if (trfCameraToWorld == null || camGrid == null || worldGrid == null)
                return;

            if (camGrid.Rows < 2 || camGrid.Cols < 2)
                return;

            //var calib = trfCameraToWorld.GetCalibGridPoints();
            //for (int r = 0, rows = camGrid.Rows; r < rows; r++)
            //{
            //    for (int c = 0, cols = camGrid.Cols; c < cols; c++)
            //    {
            //        var camPt = camGrid.Get(r, c)?.Center;
            //        var worldPt = _worldGrid.Get(r, c);
            //        calib.Update(r, c, camPt, worldPt);
            //    }
            //}

            var calib = trfCameraToWorld.GetCalibGridPoints();
            int newRows = camGrid.Rows;
            int newCols = camGrid.Cols;
            var srcPoints = new QVector[newRows, newCols];
            var dstPoints = new QVector[newRows, newCols];
            for (int r = 0, rows = camGrid.Rows; r < rows; r++)
            {
                for (int c = 0, cols = camGrid.Cols; c < cols; c++)
                {
                    srcPoints[r, c] = camGrid.Get(r, c)?.Center;
                    dstPoints[r, c] = _worldGrid.Get(r, c);
                }
            }
            calib.SetAll(srcPoints, dstPoints);
        }
        #endregion

        public void BuildAll()
        {
            GaUtil.LOG($"座標系統 [{Name}] 建構 : 開始");
            foreach (var trf in _transforms)
            {
                if (trf == null) continue;
                trf?.Build();
                trf.CheckBuildCondition(out double det1, out double det2);
                GaUtil.LOG($"MATRIX_{trf.Name} det1 = {det1:0.000000}");
                GaUtil.LOG($"MATRIX_{trf.Name} det2 = {det2:0.000000}");
            }
            GaUtil.LOG($"座標系統 [{Name}] 建構 : 完成");
        }
        public void Load(string iniFileName)
        {
            if (!System.IO.File.Exists(iniFileName))
                return;
            GaUtil.LOG($"校正參數 [{Name}] 載入 : {iniFileName}");
            LoadIni(iniFileName);
            GaUtil.LOG($"校正參數 [{Name}] 載入 : OK");
        }
        public void Save(string iniFileName)
        {
            GaUtil.LOG($"校正參數 [{Name}] 保存 : {iniFileName}");
            SaveIni(iniFileName);
            GaUtil.LOG($"校正參數 [{Name}] 保存 : OK");
        }

        #region 查核_函式群
        private (ErrorCodes, string) checkMotorCoords(CarrierEnum C, SuckerRowEnum S, QVector[] inkMarks, QVector[] motorCoords)
        {
            var errCode = ErrorCodes.OK;
            var errMsg = "";

            var trf = GetCameraMotorTransform(C, S);
            if (trf == null)
            {
                errCode = ErrorCodes.CalibErr_No_Transform_Model;
                errMsg = GaUtil.GetEnumDescription(errCode);
            }
            else if (inkMarks == null || inkMarks.Length < 4)
            {
                errCode = ErrorCodes.CalibErr_Ink_Marks_Not_Completed;
                errMsg = GaUtil.GetEnumDescription(errCode);
            }
            else if (motorCoords == null || motorCoords.Length < 4)
            {
                errCode = ErrorCodes.CalibErr_Motor_Coords_Not_Completed;
                errMsg = GaUtil.GetEnumDescription(errCode);
            }

            return (errCode, errMsg);
        }
        public (ErrorCodes, string) checkCameraGrid(CarrierEnum carrierID, EzBlocsGrid camGrid)
        {
            var errCode = ErrorCodes.OK;
            string errMsg = null;

            if (camGrid == null)
            {
                errCode = ErrorCodes.NO_CAMERA_GRID;
                errMsg = $"[{JetEazy.QxNums.GetEnumDescription(carrierID)}] "
                       + JetEazy.QxNums.GetEnumDescription(errCode)
                       + " !";
            }
            else if (camGrid.Rows < 2 || camGrid.Cols < 2 || camGrid.ActualCount < 4)
            {
                errCode = ErrorCodes.LOW_GRID_ROWS_COLS;
                errMsg = $"[{JetEazy.QxNums.GetEnumDescription(carrierID)}] "
                       + JetEazy.QxNums.GetEnumDescription(errCode)
                       + $"\n\r rows={camGrid.Rows}, cols={camGrid.Rows}, N={camGrid.ActualCount} !";
            }

            return (errCode, errMsg);
        }
        public (ErrorCodes, string) checkTransforms(CarrierEnum carrierID)
        {
            var transCP = this.GetCameraPhysicTransform(carrierID);
            var transCM1 = this.GetCameraMotorTransform(carrierID, SuckerRowEnum.S1);
            var transCM2 = this.GetCameraMotorTransform(carrierID, SuckerRowEnum.S2);

            var errCode = ErrorCodes.OK;
            string errMsg = "";

            if (transCP == null)
            {
                errCode = ErrorCodes.CalibErr_No_Transform_Model;
                errMsg = JetEazy.QxNums.GetEnumDescription(carrierID);
            }
            else
            {
                if (transCM1 == null)
                {
                    errCode = ErrorCodes.CalibErr_No_Transform_Model;
                    errMsg += JetEazy.QxNums.GetEnumDescription(carrierID) + " " + JetEazy.QxNums.GetEnumDescription(SuckerRowEnum.S1) + "\n\r";
                }

                if (transCM2 == null)
                {
                    errCode = ErrorCodes.CalibErr_No_Transform_Model;
                    errMsg += JetEazy.QxNums.GetEnumDescription(carrierID) + " " + JetEazy.QxNums.GetEnumDescription(SuckerRowEnum.S2) + "\n\r";
                }
            }

            if (errCode == ErrorCodes.OK)
            {
                return (errCode, null);
            }
            else
            {
                errMsg += "\n\r" + JetEazy.QxNums.GetEnumDescription(errCode) + " !";
                return (errCode, errMsg);
            }
        }
        private (ErrorCodes, string) checkWorldGrid()
        {
            var errCode = ErrorCodes.OK;
            string errMsg = null;

            var grid = _worldGrid;
            if (grid == null)
            {
                errCode = ErrorCodes.NO_RUNTIME_PLC_GRID;
                errMsg = JetEazy.QxNums.GetEnumDescription(errCode) + " !";
            }
            else if (grid.Rows < 2 || grid.Cols < 2)
            {
                errCode = ErrorCodes.LOW_GRID_ROWS_COLS;
                errMsg = JetEazy.QxNums.GetEnumDescription(errCode) + " !";
                errMsg = "Runtime PLC Grid\n\r" + errMsg;
            }
            if (errCode == ErrorCodes.OK)
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


    partial class TravellerTransforms
    {
        #region 跑線時期_函式群

        public (ErrorCodes, string) GetNodeCoords(CarrierEnum C, int rowId, int colId,
                                                out QVector camCoord,
                                                out QVector worldCoord,
                                                out QVector s1MotorCoord,
                                                out QVector s2MotorCoord)
        {
            #region DEFAULT_VALUES
            ErrorCodes errCode;
            string errMsg;
            camCoord = new QVector(0, 0);
            worldCoord = new QVector(0, 0);
            s1MotorCoord = new QVector(0, 0);
            s2MotorCoord = new QVector(0, 0);
            #endregion

            //(1) 檢查 Transforms 數據狀態
            (errCode, errMsg) = checkTransforms(C);
            if (errCode != ErrorCodes.OK)
                return (errCode, errMsg);

            //(2) 檢查 World Grid
            (errCode, errMsg) = checkWorldGrid();
            if (errCode != ErrorCodes.OK)
                return (errCode, errMsg);

            //(3) 取出 Camera To Motor Transforms
            var transCM1 = this.GetCameraMotorTransform(C, SuckerRowEnum.S1);
            var transCM2 = this.GetCameraMotorTransform(C, SuckerRowEnum.S2);
            var transCP = this.GetCameraPhysicTransform(C);

            //(4.1) 計算 (使用 馬達座標)
            if (OPT_CALIB_GRID_USING_MOTOR_COORD)
            {
                //(4.1) 檢查 Runtime CamGrid
                //var runtimeCamGrid = C == CarrierEnum.C1 ? _runtimeCamGridC1 : _runtimeCamGridC2;
                var runtimeCamGrid = _calibCamGrids[(int)C];
                (errCode, errMsg) = checkCameraGrid(C, runtimeCamGrid);
                if (errCode != ErrorCodes.OK)
                    return (errCode, errMsg);

                camCoord = runtimeCamGrid.Get(rowId, colId)?.Center;
                if (camCoord == null)
                    camCoord = new QVector(0, 0);

                s1MotorCoord = transCM1.Trans(camCoord);
                s2MotorCoord = transCM2.Trans(camCoord);
                worldCoord = transCP.Trans(camCoord);
            }
            //(4.2) 使用 World Coords
            else
            {
                // (r,c) -> World
                worldCoord = _worldGrid.Get(rowId, colId);
                // World -> Camera
                camCoord = transCP.InvTrans(worldCoord);
                // Camera -> Motors1
                s1MotorCoord = transCM1.Trans(camCoord);
                // Camera -> Motors2
                s2MotorCoord = transCM2.Trans(camCoord);
            }

            //(6) Return value
            return (ErrorCodes.OK, null);
        }


        public (ErrorCodes, string) GetCoordsRef(CarrierEnum C, out QVector camCoord, out QVector s1MotorCoord, out QVector s2MotorCoord)
        {
            // 取出 (row=0, col=0) 格點, 當 PLC 參考點
            return GetNodeCoords(C, 0, 0, out camCoord, out var _, out s1MotorCoord, out s2MotorCoord);
        }


#if (OPT_OLD_CODE)
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
#endif

#if (OPT_OLD_CODE)
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
            //(0) Condition
            if (_runtimePlcGrid == null)
            {
                return CalcPlcCompensation_000(C, camPt, rowId, colId);
            }

            //(1) CamBase and SuckerBase
            (var err, var errMsg) = GetCoordsRef(C, out var cam_base, out var sucker1_base, out var sucker2_base);
            if (err != ErrCodes.OK)
            {
                return CalcPlcCompensation_000(C, camPt, rowId, colId);
            }

            //(2) trans Camera To Motor
            ITransform transCM1 = GetCameraMotorTransform(C, SuckerRowEnum.S1);
            ITransform transCM2 = GetCameraMotorTransform(C, SuckerRowEnum.S2);

            //(3) 目標值 (Motor Coords)
            var plcGridPoint = _runtimePlcGrid[rowId, colId];
            var sucker1_motor_target = sucker1_base + plcGridPoint;
            var sucker2_motor_target = sucker2_base + plcGridPoint;

            //(4) 像測現值 轉換至 Motor Coords
            var s1_motor_cur = transCM1.Trans(camPt);
            var s2_motor_cur = transCM2.Trans(camPt);

            //(5) 馬達補償量
            var delta1 = s1_motor_cur - sucker1_motor_target;
            var delta2 = s2_motor_cur - sucker2_motor_target;

            //(6) 像測現值 轉換至 World Coords
            ITransform transCP = GetCameraPhysicTransform(C);
            var targetWorldPt = plcGridPoint;
            var curWorldPt = transCP.Trans(camPt);

            //(7) World Diff (參考)
            var worldErr = targetWorldPt - curWorldPt;

            return (delta1, worldErr);
        }
#endif

        /// <summary>
        /// 根據 像測點 camPt 與 目標格點 (rowId, colID), 
        /// 計算 載台 C, 馬達吸嘴中心 對其吸到 camPt 所需要的補償量
        /// </summary>
        /// <returns>(馬達補償量, 世界座標差值)</returns>
        public (QVector, QVector) CalcPlcCompensation(CarrierEnum C, QVector camPt, int rowId, int colId)
        {
            // 根據 (rowId, colId) 取得 載台C 格位節點 之 以下座標:
            //      world_target 格點的 世界座標
            //      s1_target    格點的 吸嘴1 +馬達座標
            //      s2_target    格點的 吸嘴2 馬達座標
            (var err, var errMsg) = GetNodeCoords(C, rowId, colId, out var _, out var world_target, out var s1_target, out var s2_target);
            if (err != ErrorCodes.OK)
            {
                return (new QVector(0, 0), new QVector(0, 0));
            }

            var transCM1 = GetCameraMotorTransform(C, SuckerRowEnum.S1);
            var transCM2 = GetCameraMotorTransform(C, SuckerRowEnum.S2);
            var transCP = GetCameraPhysicTransform(C);

            // 由 像測點 推算 對定馬達值
            var s1_current = transCM1.Trans(camPt);
            var s2_current = transCM2.Trans(camPt);
            // 由 像測點 推算 對應 世界值
            var world_current = transCP.Trans(camPt);

            // 注意: 德龍補償量 == 量測現值 - 目標值
            var motorDelta = s1_current - s1_target;

            // WorldDetla
            var worldDelta = world_current - world_target;

            return (motorDelta, worldDelta);
        }

        #endregion
    }
}
