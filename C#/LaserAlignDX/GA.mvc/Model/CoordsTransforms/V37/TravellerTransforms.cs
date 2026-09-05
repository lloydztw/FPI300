#region AUTHOR
/*
 * 
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2026-04-02 V35 使用 QxCoordsTransform (by LeTian Chang)
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

namespace LaserAlignDX.Model.Coords.V37
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
        static int N_CARRIERS => Enum.GetValues(typeof(CarrierEnum)).Length;
        /// <summary>
        /// Carrier World Grids (理想世界格點)
        /// </summary>
        QWorldGridPointsEx[] _carrierWorldGrids = new QWorldGridPointsEx[N_CARRIERS];
        /// <summary>
        /// 相機校正點 (大校正板 抓到的 pixel 點位) (用於 CameraToWorld 座標轉換的建立)
        /// </summary>
        EzBlocsGrid[] _calibCamGrids = new EzBlocsGrid[N_CARRIERS];
        /// <summary>
        /// 各種 座標轉換 集合
        /// </summary>
        ITransform[] _transforms = new ITransform[]
        {
            new QxCoordsTransform("C1_P", "pix", "mm"),         // 線掃相機C1 <--> world
            new QTransform("C1_M1S1", "pix", "mm"),             // 線掃相機C1 <--> motors (sucker 1)
            new QTransform("C1_M1S2", "pix", "mm"),             // 線掃相機C1 <--> motors (sucker 2)
            new QxCoordsTransform("C2_P", "pix", "mm"),         // 線掃相機C2 <--> world
            new QTransform("C2_M2S1", "pix", "mm"),             // 線掃相機C2 <--> motors (sucker 1)
            new QTransform("C2_M2S2", "pix", "mm"),             // 線掃相機C2 <--> motors (sucker 2)
        };
        #endregion

        #region INDEX_FUNCTIONS
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
            dispose(_transforms);
            dispose(_calibCamGrids);
            Unregister(this);
        }
        void dispose(IDisposable[] arr)
        {
            for (int i = 0, len = arr.Length; i < len; i++)
            {
                arr[i]?.Dispose();
                arr[i] = null;
            }
        }
        #endregion

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
            return getCarrierWorldGrid(C);
        }

        QWorldGridPointsEx getCarrierWorldGrid(CarrierEnum C)
        {
            int idx = (int)C;
            if (_carrierWorldGrids[idx] == null)
                _carrierWorldGrids[idx] = new QWorldGridPointsEx();
            return _carrierWorldGrids[idx];
        }

        #region 校正時期_函式群
        public void ConfigWorldGridPoints(CarrierEnum C, int rows, int cols, double pitchX, double pitchY)
        {
            //if (_worldGrid == null)
            //    _worldGrid = new QWorldGridPointsEx(rows, cols, pitchX, pitchY);
            //else
            //    _worldGrid.Config(rows, cols, pitchX, pitchY);
            //return _worldGrid;
            //var grid = getCarrierWorldGrid(C);
            //grid?.Config(rows, cols, pitchX, pitchY);
            //return grid;

            getCarrierWorldGrid(C)?.Config(rows, cols, pitchX, pitchY);
        }

        public EzBlocsGrid GetCalibCamGrid(CarrierEnum C)
        {
            return _calibCamGrids[(int)C];
        }

        public bool SetCalibCamGrid(CarrierEnum C, EzBlocsGrid camGrid, IList<JxTraySegItem> segsList)
        {
            //(0) 檢查 相機格點
            (var err, var errMsg) = checkCameraGrid(C, camGrid);
            if (err != ErrorCodes.OK)
            {
                _LOG.Error(errMsg + "@ SetWCalibCameraGrid");
                return false;
            }


            var oldCamGrid = _calibCamGrids[(int)C];
            try
            {
                _calibCamGrids[(int)C] = camGrid;

                var carrierWorldGrid = getCarrierWorldGrid(C);
                if (carrierWorldGrid == null)
                    return false;

                if (camGrid.Rows != carrierWorldGrid.Rows || camGrid.Cols != carrierWorldGrid.Cols)
                {
                    _LOG.Warn($"Rows Cols 不一致 : camGrid({camGrid.Rows}x{camGrid.Cols}) != worldGrid.{C}({carrierWorldGrid.Rows}x{carrierWorldGrid.Cols})");
                }

                //(1) 設定不連續區塊
                carrierWorldGrid.SetSegments(camGrid, segsList);

                //(2) 更新 CameraToWorld 校正點位
                var trfCameraToWorld = GetCameraPhysicTransform(C);
                updateCalibPointsToTrf(trfCameraToWorld, camGrid, carrierWorldGrid);

                return true;
            }
            finally
            {
                if (oldCamGrid != camGrid)
                    oldCamGrid?.Dispose();
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

        void updateCalibPointsToTrf(ITransform trfCameraToWorld, EzBlocsGrid camGrid, IWorldGridPoints worldGrid)
        {
            if (trfCameraToWorld == null || camGrid == null || worldGrid == null)
                return;

            if (camGrid.Rows < 2 || camGrid.Cols < 2)
                return;

            var calib = trfCameraToWorld.GetCalibGridPoints();
            int newRows = Math.Min(camGrid.Rows, worldGrid.Rows);
            int newCols = Math.Min(camGrid.Cols, worldGrid.Cols);

            //// Rows Cols 必須一致
            if (camGrid.Rows != worldGrid.Rows || camGrid.Cols != worldGrid.Cols)
            {
                //var pitchX = worldGrid.PitchX;
                //var pitchY = worldGrid.PitchY;
                //worldGrid.Config(newRows, newCols, pitchX, pitchY);
                _LOG.Warn($"Rows Cols 不一致 : camGrid({camGrid.Rows}x{camGrid.Cols}) != worldGrid({worldGrid.Rows}x{worldGrid.Cols})");
            }

            var srcPoints = new QVector[newRows, newCols];
            var dstPoints = new QVector[newRows, newCols];

            for (int r = 0, rows = newRows; r < rows; r++)
            {
                for (int c = 0, cols = newCols; c < cols; c++)
                {
                    srcPoints[r, c] = camGrid.Get(r, c)?.Center;
                    dstPoints[r, c] = worldGrid.Get(r, c);

                    // 不能有 null point !!!
                    System.Diagnostics.Debug.Assert(dstPoints[r, c] != null);
                }
            }

            calib.SetAll(srcPoints, dstPoints);
        }
        #endregion

        public void BuildAll()
        {
            GaUtil.LOG($"座標系統 [{Name}] (V36) 建構 : 開始");
            foreach (var trf in _transforms)
            {
                if (trf == null) continue;
                trf?.Build();
                trf.CheckBuildCondition(out double det1, out double det2);
                GaUtil.LOG($"MATRIX_{trf.Name} det1 = {det1:0.000000}");
                GaUtil.LOG($"MATRIX_{trf.Name} det2 = {det2:0.000000}");
            }
            //foreach (var trf in _trfCamDistAdjusts)
            //{
            //    if (trf == null) continue;
            //    trf?.Build();
            //    trf.CheckBuildCondition(out double det1, out double det2);
            //    GaUtil.LOG($"MATRIX_{trf.Name} det1 = {det1:0.000000}");
            //    GaUtil.LOG($"MATRIX_{trf.Name} det2 = {det2:0.000000}");
            //}
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
        private (ErrorCodes, string) checkWorldGrid(IWorldGridPoints _worldGrid)
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

        public (ErrorCodes, string) GetNodeCoords(CarrierEnum C,
                                                int rowId, int colId,
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
            (errCode, errMsg) = checkWorldGrid(getCarrierWorldGrid(C));
            if (errCode != ErrorCodes.OK)
                return (errCode, errMsg);

            //(3) 取出 Camera To Motor Transforms
            var transCM1 = this.GetCameraMotorTransform(C, SuckerRowEnum.S1);
            var transCM2 = this.GetCameraMotorTransform(C, SuckerRowEnum.S2);
            var transCP = this.GetCameraPhysicTransform(C);

            //-----------------------------------------------------------------------------
            // REV_2026-03-31 
            //-----------------------------------------------------------------------------
            bool debugVerify = false;
            var carrierWorldGrid = getCarrierWorldGrid(C);

            //(4) 計算 (使用 World --> Camera --> Motor 座標轉換)
            if (true)
            {
                // (row, col) --> World
                worldCoord = carrierWorldGrid.Get(rowId, colId);

                // Camera <-- World
                camCoord = transCP.InvTrans(worldCoord);

                // 驗證
                if (debugVerify)
                {
                    var camCoordRC = _calibCamGrids[(int)C].Get(rowId, colId).Center;
                    var delta = (camCoordRC - camCoord);
                    if (delta.NormLength > 0.005)
                        System.Diagnostics.Debug.WriteLine(delta);
                }

                // Camera -> MotorS1
                s1MotorCoord = transCM1.Trans(camCoord);

                // Camera -> MotorS2
                s2MotorCoord = transCM2.Trans(camCoord);
            }

            //(5) Return value
            return (ErrorCodes.OK, null);
        }

        public (ErrorCodes, string) GetCoordsRef(CarrierEnum C, out QVector camCoord, out QVector s1MotorCoord, out QVector s2MotorCoord)
        {
            // 取出 (row=0, col=0) 格位, 當 PLC 參考點
            return GetNodeCoords(C, 0, 0, out camCoord, out var _, out s1MotorCoord, out s2MotorCoord);
        }

        public (ErrorCodes, string) GetPlcExpectedCoords(CarrierEnum C, int rowId, int colId, out QVector s1MotorCoord, out QVector s2MotorCoord)
        {
            (var err, var errMsg) = GetNodeCoords(C, 0, 0, out var _, out var _, out var s1_org, out var s2_org);

            //(1) 從 世界座標 設定 取得 pitchX, pitchY
            var carrierWorldGrid = getCarrierWorldGrid(C);
            var pitchX = carrierWorldGrid.PitchX;
            var pitchY = carrierWorldGrid.PitchY;
            var pitchVect = new QVector(pitchX * colId, pitchY * rowId);

            //(2) PLC 預期 (rowId, colID) 格位中心 所在的 馬達座標 (德龍預想值)
            var s1_naive = s1_org + pitchVect;
            var s2_naive = s2_org + pitchVect;

            //(3) 處理 不連續區段
            if (carrierWorldGrid.HasOffsets())
            {
                //=========================================================
                // 目前只處理 row jump (offsetY)
                //=========================================================
                double accumOffsetY = 0;
                for (int row = rowId; row >= 0; row--)
                {
                    var offset = carrierWorldGrid.GetSegmentAccumOffset(row, colId);
                    if (offset != null)
                        accumOffsetY = offset.Y;
                }
                s1_naive.Y += accumOffsetY;
                s2_naive.Y += accumOffsetY;
            }

            s1MotorCoord = s1_naive;
            s2MotorCoord = s2_naive;
            return (err, errMsg);
        }

        /// <summary>
        /// 根據 像測點 camPt 與 目標格點 (rowId, colID), 
        /// 計算 載台 C, 馬達吸嘴中心 對其吸到 camPt 所需要的補償量
        /// </summary>
        /// <returns>(馬達補償量, 世界座標差值)</returns>
        public (QVector, QVector, QVector) CalcPlcCompensation(CarrierEnum C, QVector camPt, int rowId, int colId)
        {
            //(1) 取得 PLC 內部預想 (rowId, colId) 座標值
            (var err, var errMsg) = GetPlcExpectedCoords(C, rowId, colId, out var s1_naive, out var s2_naive);

            //(2) 根據 (rowId, colId) 取得 載台C 格位中心 之 以下:
            //      world_node  格位中心的 世界座標
            //      transCM1    Camera To MotorS1
            //      transCM2    Camera To MotorS2
            //      transCP     Camera To World
            var carrierWorldGrid = getCarrierWorldGrid(C);
            var world_node = carrierWorldGrid?.Get(rowId, colId);
            var transCM1 = GetCameraMotorTransform(C, SuckerRowEnum.S1);
            var transCM2 = GetCameraMotorTransform(C, SuckerRowEnum.S2);
            var transCP = GetCameraPhysicTransform(C);
            
            //(4) 由 像測點 推算 對應 馬達座標值
            var s1_current = transCM1.Trans(camPt);
            var s2_current = transCM2.Trans(camPt);

            //(5) 由 像測點 推算 對應 世界座標值
            var world_current = transCP.Trans(camPt);

            //(6) 【德龍補償量】 == 量測現值 - 德龍預想值
            //// var naiveDelta = s1_node - s1_naive;
            //// var motorDelta = s1_current - s1_node;
            //// motorDelta = motorDelta + naiveDelta;
            var motorD1 = s1_current - s1_naive;
            var motorD2 = s2_current - s2_naive;
            //motorDelta = (motorDelta + motorDelta2) * 0.5;

            //(7) WorldDetla (調試用)
            var worldDelta = world_node != null ? world_current - world_node : world_current;

            return (motorD1, motorD2, worldDelta);
        }

        #endregion
    }

    partial class TravellerTransforms
    {
        /// <summary>
        /// 保留
        /// </summary>
        public double CameraWorkDist { get; set; }

        /// <summary>
        /// 建立 相機工作高度 座標轉換
        /// </summary>
        void BuildWorkDistAdjustment_000(CarrierEnum carrierID, ITravellerTransforms commonBaseTrf)
        {
#if (false)
            var trfModel = this;

            //NOTE: trfModel 是對焦在 (空盤表面)
            //NOTE: commonBaseTrf 是對焦在 (晶粒表面)

            //(1) 此處 camGrid 是對焦在 空盤表面
            var camGrid = this.GetCalibCamGrid(carrierID);
            var R = camGrid.Rows - 1;
            var C = camGrid.Cols - 1;
            var camPts = new[]
            {
                camGrid[0, 0].Center,
                camGrid[0, C].Center,
                camGrid[R, C].Center,
                camGrid[R, 0].Center,
            };
            var carrierWorldGrid = this.GetWorldGridPoints(carrierID);
            var carrierWorldPts = new[]
            {
                carrierWorldGrid.Get(0, 0),
                carrierWorldGrid.Get(0, C),
                carrierWorldGrid.Get(R, C),
                carrierWorldGrid.Get(R, 0),
            };

            //(2) trfCamToMotor 是對焦在 晶粒表面
            var base_trfCamWorld = commonBaseTrf.GetCameraPhysicTransform(carrierID);
            var base_WorldPts = new QVector[4];
            for (int i = 0; i < 4; i++)
            {
                base_WorldPts[i] = base_trfCamWorld.Trans(camPts[i]);
                //base_CamPts[i] = base_trfCamWorld.Trans(base_WorldPts[i]);
                //s1_motorPts[i] = base_trfCamToMotorS1.Trans(base_CamPts[i]);
                //s2_motorPts[i] = base_trfCamToMotorS2.Trans(base_CamPts[i]);
            }

            //(3) 建立 空盤表面 到 晶粒表面 的 座標轉換
            var W1 = (base_WorldPts[0] - base_WorldPts[1]).NormLength;
            var W2 = (base_WorldPts[3] - base_WorldPts[2]).NormLength;
            var H1 = (base_WorldPts[0] - base_WorldPts[3]).NormLength;
            var H2 = (base_WorldPts[1] - base_WorldPts[2]).NormLength;
            var W = (W1 + W2) / 2;
            var H = (H1 + H2) / 2;

            var pitch = camGrid.GetPitch();
            var PW1 = (carrierWorldPts[0] - carrierWorldPts[1]).NormLength;
            var PW2 = (carrierWorldPts[3] - carrierWorldPts[2]).NormLength;
            var PH1 = (carrierWorldPts[0] - carrierWorldPts[3]).NormLength;
            var PH2 = (carrierWorldPts[1] - carrierWorldPts[2]).NormLength;
            var PW = (PW1 + PW2) / 2;
            var PH = (PH1 + PH2) / 2;
            System.Diagnostics.Trace.WriteLine($"W = {W:0.000} vs {PW:0.000}");
            System.Diagnostics.Trace.WriteLine($"H = {H:0.000} vs {PH:0.000}");

            var U = base_WorldPts[1] - base_WorldPts[0];
            U = U * PW / W;
            var V = base_WorldPts[3] - base_WorldPts[0];
            V = V * PH / H;
            base_WorldPts[1] = base_WorldPts[0] + U;
            base_WorldPts[3] = base_WorldPts[0] + V;
            base_WorldPts[2] = base_WorldPts[0] + U + V;

            var adjustCamPts = Array.ConvertAll(base_WorldPts, p => base_trfCamWorld.InvTrans(p));

            //(4) 建立 空盤表面 到 晶粒表面 的 座標轉換
            var adjustTrf = _trfCamDistAdjusts[(int)carrierID];
            var adjCalib = adjustTrf.GetCalibCornerPoints();
            for (int i = 0; i < 4; i++)
            {
                adjCalib.Set(i, camPts[i], adjustCamPts[i]);
            }
            adjustTrf.Build();
#endif
        }

        /// <summary>
        /// 建立 相機工作高度 座標轉換
        /// </summary>
        public void BuildWorkDistAdjustment(CarrierEnum carrierID, ITravellerTransforms commonBaseTrf)
        {
            var trfModel = this;

            //NOTE: trfModel 是對焦在 (空盤表面)
            //NOTE: commonBaseTrf 是對焦在 (晶粒表面)

            //(1) 此處 camGrid 是對焦在 空盤表面
            var camGrid = this.GetCalibCamGrid(carrierID);
            var R = camGrid.Rows - 1;
            var C = camGrid.Cols - 1;
            var camPts = new[]
            {
                camGrid[0, 0].Center,
                camGrid[0, C].Center,
                camGrid[R, C].Center,
                camGrid[R, 0].Center,
            };
            var carrierWorldGrid = this.GetWorldGridPoints(carrierID);
            var carrierWorldPts = new[]
            {
                carrierWorldGrid.Get(0, 0),
                carrierWorldGrid.Get(0, C),
                carrierWorldGrid.Get(R, C),
                carrierWorldGrid.Get(R, 0),
            };

            //(2) trfCamToMotor 是對焦在 晶粒表面
            //(3) Motors' post scaling
            var SpanW = (carrierWorldPts[0] - carrierWorldPts[1]).NormLength;
            var SpanH = (carrierWorldPts[0] - carrierWorldPts[3]).NormLength;

            foreach(SuckerRowEnum sid in Enum.GetValues(typeof(SuckerRowEnum)))
            {
                var trfCamToMotor = GetCameraMotorTransform(carrierID, sid) as QTransform;
                trfCamToMotor.PostChain?.Dispose();
                trfCamToMotor.PostChain = null;

                var motorPts = Array.ConvertAll(camPts, p => trfCamToMotor.Trans(p));

                var W1 = (motorPts[0] - motorPts[1]).NormLength;
                var W2 = (motorPts[3] - motorPts[2]).NormLength;
                var H1 = (motorPts[0] - motorPts[3]).NormLength;
                var H2 = (motorPts[1] - motorPts[2]).NormLength;
                //var W = (W1 + W2) / 2;
                //var H = (H1 + H2) / 2;
                var U = motorPts[1] - motorPts[0];
                var V = motorPts[3] - motorPts[0];
                U = U * SpanW / W1;
                V = V * SpanH / H1;

                var scaledMotorPts = Array.ConvertAll(motorPts, p => new QVector(p));
                scaledMotorPts[1] = scaledMotorPts[0] + U;
                scaledMotorPts[3] = scaledMotorPts[0] + V;
                scaledMotorPts[2] = scaledMotorPts[0] + U + V;

                var trfAdj = new QTransform($"{trfCamToMotor.Name}_PostChain", "mm", "mm");
                var calib = trfAdj.GetCalibCornerPoints();
                for (int i = 0; i < 4; i++)
                    calib.Set(i, motorPts[i], scaledMotorPts[i]);
                trfAdj.Build();

                trfCamToMotor.PostChain = trfAdj;
            }
        }
    }
}
