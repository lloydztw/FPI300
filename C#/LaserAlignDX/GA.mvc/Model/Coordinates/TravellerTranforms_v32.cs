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
using LeTian.AoiLib;
using System;
using System.Collections.Generic;
using System.Linq;
using ErrorCodes = LaserAlignDX.Mvc.Model.ErrorCodes;


namespace LaserAlignDX.Model.Coords
{
    /// <summary>
    /// Traveller106 專案 的 所有座標系
    /// </summary>
    public partial class TravellerTransforms : IDisposable, ITravellerTransforms
    {
        #region CONFIG
        /// <summary>
        /// 校正點數 (馬達座標)
        /// </summary>
        public const int N_CALIB_POINTS = 4;
        /// <summary>
        /// 目前是使用 Motor Coordinate
        /// </summary>
        internal static bool OPT_CALIB_GRID_USING_MOTOR_COORD => true;
        #endregion

        #region PRIVATE_TRANSFORM_MEMBERS
        PlcGridPoints _calibPlcGrid = new PlcGridPoints();
        EzBlocsGrid[] _calibCamGrids = new EzBlocsGrid[Enum.GetValues(typeof(CarrierEnum)).Length];
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

        public string Name
        {
            get;
            protected set;
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

        #region 校正時期_函式群
        public PlcGridPoints getCalibPlcGrid()
        {
            // 2025-12-23 新增
            return _calibPlcGrid;
        }
        public EzBlocsGrid GetCalibCameraGrid(CarrierEnum C)
        {
            // 2025-12-23 新增
            return _calibCamGrids[(int)C];
        }
        public void SetCalibCameraGrid(CarrierEnum C, EzBlocsGrid camGrid)
        {
            // 2025-12-23 新增
            var old = _calibCamGrids[(int)C];
            _calibCamGrids[(int)C] = camGrid;
            if (old != camGrid)
                old?.Dispose();
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
        #endregion

        #region INI_FILE_FUNCIONS
        public void Load(string iniFileName)
        {
            if (!System.IO.File.Exists(iniFileName))
            {
                this.LoadGaaraIniFile();
                return;
            }

            LtDebug.LOG.Info($"載入 [校正參數 (Trf)] @ [{Name}] : {iniFileName}");

            _calibPlcGrid.Load(iniFileName, "GlobalCalibPlcGrid");
            foreach (var trf in _transforms)
            {
                trf.Load(iniFileName);
            }

            // 2025-12-23 新增
            loadCalibCamGrids(iniFileName);
        }
        public void Save(string iniFileName)
        {
            LtDebug.LOG.Info($"寫入 [校正參數 (Trf)] @ [{Name}] : {iniFileName}");

            _calibPlcGrid.Save(iniFileName, "GlobalCalibPlcGrid");
            foreach (var trf in _transforms)
            {
                trf.Save(iniFileName);
            }

            // 2025-12-23 新增
            saveCalibCamGrids(iniFileName);
        }
        #endregion

        #region PRIVATE_FILE_IO_FUNCTIONS
        string normalizeCalibCamGridDataFile(string iniFileName)
        {
            return System.IO.Path.ChangeExtension(iniFileName, ".GridBoard.dat");
        }
        void loadCalibCamGrids(string iniFileName)
        {
            int NGrids = _calibCamGrids.Length;
            for (int i = 0; i < NGrids; i++)
            {
                _calibCamGrids[i]?.Dispose();
                _calibCamGrids[i] = null;
            }

            //-------------------------------------------------------------------------------
            // 注意: 由於字串太長, 使用 Win32.Ini 會被截斷 !!!
            //-------------------------------------------------------------------------------
            string fileName = normalizeCalibCamGridDataFile(iniFileName);
            if (!System.IO.File.Exists(fileName))
                return;

            var lines = System.IO.File.ReadAllLines(fileName);
            NGrids = Math.Min(NGrids, lines.Length);

            var GS = new EzBlocsGridSerializer();
            for (int i = 0; i < NGrids; i++)
            {
                string str = lines[i];
                GS.Deserialize(str, out _calibCamGrids[i]);
            }
        }
        void saveCalibCamGrids(string iniFileName)
        {
            int NGrids = _calibCamGrids.Length;
            var lines = new string[NGrids];

            var GS = new EzBlocsGridSerializer();
            for (int i = 0; i < NGrids; i++)
            {
                string str = GS.Serialize(_calibCamGrids[i]);
                lines[i] = str;
            }

            //-------------------------------------------------------------------------------
            // 注意: 由於字串太長, 使用 Win32.Ini 會被截斷 !!!
            //-------------------------------------------------------------------------------
            string fileName = normalizeCalibCamGridDataFile(iniFileName);
            System.IO.File.WriteAllLines(fileName, lines);
        }
        #endregion

        #region PRIVATE_HELPER_FUNCTIONS
        QVector[,] toCalibGrid(EzBlocsGrid camGrid)
        {
            int rows = camGrid.Rows;
            int cols = camGrid.Cols;
            var pts = new QVector[rows, cols];
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    var node = camGrid.Get(r, c);
                    bool ok = node != null && node.IsMajorNode();
                    pts[r, c] = ok ? node.Center : null;
                }
            }
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
        EzBlocsGrid _runtimeCamGridC1 = null;
        EzBlocsGrid _runtimeCamGridC2 = null;
        #endregion

        #region 跑線時期_函式群

        /// <summary>
        /// 設定 Runtime 跑線時期 P座標 (PLC) 格點
        /// </summary>
        public void UpdateRuntimePlcGrid(PlcGridPoints plcGrid, EzBlocsGrid camGrid1 = null, EzBlocsGrid camGrid2 = null)
        {
            if (plcGrid != null)
                _runtimePlcGrid = plcGrid;

            if (camGrid1 != null)
                _runtimeCamGridC1 = camGrid1;

            if (camGrid2 != null)
                _runtimeCamGridC2 = camGrid2;
        }

        /// <summary>
        /// 取出 Runtime 跑線時期 標準格點 座標數據
        /// </summary>
        public (ErrorCodes, string) GetNodeCoords(CarrierEnum C, int rowId, int colId, 
                                                out QVector camCoord, 
                                                out QVector worldCoord,
                                                out QVector s1MotorCoord, 
                                                out QVector s2MotorCoord )
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

            //(2) 檢查 Runtime PlcGrid
            (errCode, errMsg) = checkRuntimePlcGrid();
            if (errCode != ErrorCodes.OK)
                return (errCode, errMsg);

            //(3) 取出 Camera To Motor Transforms (Global)
            var transCM1 = this.GetCameraMotorTransform(C, SuckerRowEnum.S1);
            var transCM2 = this.GetCameraMotorTransform(C, SuckerRowEnum.S2);
            var transCP = this.GetCameraPhysicTransform(C);

            //(4) 計算 (簡單 使用 馬達座標)
            if (OPT_CALIB_GRID_USING_MOTOR_COORD)
            {
                //(4.1) 檢查 Runtime CamGrid
                var runtimeCamGrid = C == CarrierEnum.C1 ? _runtimeCamGridC1 : _runtimeCamGridC2;
                (errCode, errMsg) = checkCameraGrid(C, runtimeCamGrid);
                if(errCode != ErrorCodes.OK) 
                    return (errCode, errMsg);
                
                camCoord = runtimeCamGrid.Get(rowId, colId)?.Center;
                if (camCoord == null)
                    camCoord = new QVector(0, 0);

                s1MotorCoord = transCM1.Trans(camCoord);
                s2MotorCoord = transCM2.Trans(camCoord);
                worldCoord = transCP.Trans(camCoord);
            }
            //(5) 使用 World Coords
            else
            {
                worldCoord = _runtimePlcGrid[rowId, colId];
                camCoord = transCP.InvTrans(worldCoord);
                s1MotorCoord = transCM1.Trans(camCoord);
                s2MotorCoord = transCM2.Trans(camCoord);
            }

            //(6) Return value
            return (ErrorCodes.OK, null);
        }


        /// <summary>
        /// 取出 Runtime 跑線時期 各別載台 PLC 所需要的參考點 座標數據
        /// </summary>
        public (ErrorCodes, string) GetCoordsRef(CarrierEnum C, out QVector camCoord, out QVector s1MotorCoord, out QVector s2MotorCoord)
        {
            #region NOT_USED_CODE
            //#region DEFAULT_VALUES
            //ErrCodes errCode = ErrCodes.OK;
            //camCoord = new QVector(0, 0);
            //s1MotorCoord = new QVector(0, 0);
            //s2MotorCoord = new QVector(0, 0);
            //#endregion

            ////(1) 檢查 Transforms 數據狀態
            //(errCode, errMsg) = checkTransforms(C);
            //if (errCode != ErrCodes.OK)
            //    return errCode;

            ////(2) 檢查 RuntimePlcGrid
            //(errCode, errMsg) = checkRuntimePlcGrid();
            //if (errCode != ErrCodes.OK)
            //    return errCode;

            ////(3) 取出 Global Transforms
            //var transCM1 = this.GetCameraMotorTransform(C, SuckerRowEnum.S1);
            //var transCM2 = this.GetCameraMotorTransform(C, SuckerRowEnum.S2);

            ////(4) 計算 (簡單 使用 馬達座標)
            //if (OPT_USING_XRECIPE_CAM_GRID)
            //{
            //    var runtimeCamGrid = C == CarrierEnum.C1 ? _runtimeCamGridC1 : _runtimeCamGridC2;
            //    var camBasePt = runtimeCamGrid?.Get(0, 0)?.Center;

            //    (errCode, errMsg) = checkCameraGrid(C, runtimeCamGrid);
            //    if (camBasePt == null || runtimeCamGrid == null)
            //        return ErrCodes.NO_CAMERA_GRID;

            //    var s1_motor_coord = transCM1.Trans(camBasePt);
            //    var s2_motor_coord = transCM2.Trans(camBasePt);

            //    camCoord = camBasePt;
            //    s1MotorCoord = s1_motor_coord;
            //    s2MotorCoord = s2_motor_coord;
            //}
            ////(5) 使用 World Coords
            //else
            //{
            //    var transCP = this.GetCameraPhysicTransform(C);
            //    QVector runtimeWorldPt = _runtimePlcGrid[0, 0];
            //    camCoord = transCP.InvTrans(runtimeWorldPt);
            //    var s1_motor_coord = transCM1.Trans(camCoord);
            //    var s2_motor_coord = transCM2.Trans(camCoord);
            //    s1MotorCoord = s1_motor_coord;
            //    s2MotorCoord = s2_motor_coord;
            //}

            ////(6) Return value
            //errMsg = null;
            //return ErrCodes.OK;
            #endregion

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
        /// 計算 PLC 補償量
        /// </summary>
        public (QVector, QVector) CalcPlcCompensation(CarrierEnum C, QVector camPt, int rowId, int colId)
        {
            (var err, var errMsg) = GetNodeCoords(C, rowId, colId, out var _, out var world_target, out var s1_target, out var s2_target);
            if (err != ErrorCodes.OK)
            {
                return (new QVector(0,0), new QVector(0, 0));
            }

            var transCM1 = GetCameraMotorTransform(C, SuckerRowEnum.S1);
            var transCM2 = GetCameraMotorTransform(C, SuckerRowEnum.S2);
            var transCP = GetCameraPhysicTransform(C);

            var s1_current = transCM1.Trans(camPt);
            var s2_current = transCM2.Trans(camPt);
            var world_current = transCP.Trans(camPt);

            // 注意: 德龍補償量 == 量測現值 - 目標值
            var motorDelta = s1_current - s1_target;

            // WorldDetla
            var worldDelta = world_current - world_target;

            return (motorDelta, worldDelta);
        }

        #endregion

        #region 查核_函式群
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
            else if (camGrid.Rows < 2 || camGrid.Cols < 2)
            {
                errCode = ErrorCodes.LOW_GRID_ROWS_COLS;
                errMsg = $"[{JetEazy.QxNums.GetEnumDescription(carrierID)}] "
                       + JetEazy.QxNums.GetEnumDescription(errCode)
                       + $"\n\r rows={camGrid.Rows}, cols={camGrid.Rows} !";
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
                errCode = ErrorCodes.NO_CALIB_TRANSFORM;
                errMsg = JetEazy.QxNums.GetEnumDescription(carrierID);
            }
            else
            {
                if (transCM1 == null)
                {
                    errCode = ErrorCodes.NO_CALIB_TRANSFORM;
                    errMsg += JetEazy.QxNums.GetEnumDescription(carrierID) + " " + JetEazy.QxNums.GetEnumDescription(SuckerRowEnum.S1) + "\n\r";
                }

                if (transCM2 == null)
                {
                    errCode = ErrorCodes.NO_CALIB_TRANSFORM;
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
        public (ErrorCodes, string) checkRuntimePlcGrid()
        {
            var errCode = ErrorCodes.OK;
            string errMsg = null;

            if (_runtimePlcGrid == null)
            {
                errCode = ErrorCodes.NO_RUNTIME_PLC_GRID;
                errMsg = JetEazy.QxNums.GetEnumDescription(errCode) + " !";
            }
            else if (_runtimePlcGrid.Rows < 2 || _runtimePlcGrid.Cols < 2)
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
        #region PRIVATE_STATIC_REGISTER_TABLE
        static Dictionary<string, TravellerTransforms> _registerTable = new Dictionary<string, TravellerTransforms>();
        static void register(TravellerTransforms obj)
        {
            if (obj == null)
                return;
            if(!_registerTable.ContainsKey(obj.Name)) 
                _registerTable.Add(obj.Name, obj);
        }
        static void unregister(TravellerTransforms obj)
        {
            if (obj == null) return;
            if(_registerTable.ContainsKey(obj.Name))
                _registerTable.Remove(obj.Name);
        }
        static TravellerTransforms find(string name)
        {
            if(string.IsNullOrEmpty(name))
                return null;
            if (_registerTable.ContainsKey(name))
                return _registerTable[name];
            return null;
        }
        #endregion

        #region 建構解構_函式群
        protected TravellerTransforms(string name)
        {
            QVector.Percision = 12;
            Name = name;
            register(this);
        }

        void IDisposable.Dispose()
        {
            foreach (var trf in _transforms)
                trf?.Dispose();
            unregister(this);
        }

        /// <summary>
        /// 所有參數共用基礎 的 座標轉換系統
        /// </summary>
        public static TravellerTransforms CommonBase
        {
            get
            {
                return Instance("$CommonBase$");
            }
        }

        /// <summary>
        /// 個別參數 的 座標轉換系統
        /// </summary>
        public static TravellerTransforms Instance(string name)
        {
            var instance = find(name);
            if (instance == null)
                instance = new TravellerTransforms(name);
            return instance;
        }

        /// <summary>
        /// 卸載所有 座標轉換系統 (exclusiveNames 除外)
        /// </summary>
        public static void DisposeAll(params string[] exclusiveNames)
        {
            var keeps = Array.ConvertAll(exclusiveNames, name => find(name));

            var allObjs = _registerTable.Values.ToList();
            foreach(IDisposable obj in allObjs)
            {
                if (Array.IndexOf(keeps, obj) < 0)
                    obj?.Dispose();
            }

            _registerTable.Clear();
            foreach (var keep in keeps)
                register(keep);
        }

        /// <summary>
        /// 只保留 name (與 $CommonBase$) 兩個 座標轉換系統 其餘都卸載
        /// </summary>
        public static void Keep(string name)
        {
            DisposeAll(name, "$CommonBase$");
        }
        #endregion

        /// <summary>
        /// 根據 新的 相機格點 (陣列) 線性遷移 生成新的轉換公式
        /// </summary>
        public ITravellerTransforms BuildLinearMigration(string name, CarrierEnum carrierID, EzBlocsGrid camRegionsArray)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            var commonBase = CommonBase;
            var result = Instance(name);

            if (true)
            {
                var tmpFile = "d:\\paso.log\\jx_transforms.ini";
                commonBase.Save(tmpFile);
                result.Load(tmpFile);

                var plcGrid = commonBase._calibPlcGrid;
                var camGrid1 = carrierID == CarrierEnum.C1 ? camRegionsArray : null;
                var camGrid2 = carrierID == CarrierEnum.C2 ? camRegionsArray : null;
                result.UpdateRuntimePlcGrid(plcGrid, camGrid1, camGrid2);
            }

            return result;
        }
    }
}
