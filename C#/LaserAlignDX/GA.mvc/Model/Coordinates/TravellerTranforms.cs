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
using System;


namespace LaserAlignDX.Model.Coords
{
    /// <summary>
    /// Traveller106 專案 的 所有座標系
    /// </summary>
    public class TravellerTransforms : IDisposable
    {
        public const int N_CALIB_POINTS = 4;

        #region PRIVATE_DATA
        PlcGridPoints _plcGrid = null;
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
        
        private QVector getCameraBasePoint(CarrierEnum C)
        {
            var trf = GetCameraPhysicTransform(C) ?? GetCameraMotorTransform(C, SuckerRowEnum.S1);
            var trfCorners = trf.GetCalibCornerPoints();
            trfCorners.Get(0, out var camPt, out var _);
            return camPt;
        }
        private QVector getMotorBasePoint(CarrierEnum C, SuckerRowEnum S)
        {
            var trf = GetCameraMotorTransform(C, S);
            var trfCorners = trf.GetCalibCornerPoints();
            trfCorners.Get(0, out var _, out var motorPt);
            return motorPt;
        }

        /// <summary>
        /// 設定 P 座標 (PLC) 格點
        /// </summary>
        public PlcGridPoints ConfigPlcGrid(int rows, int cols, double pitchX, double pitchY)
        {
            _plcGrid = new PlcGridPoints(rows, cols, pitchX, pitchY);
            return _plcGrid;
        }

        /// <summary>
        /// 計算 PLC 補償量
        /// </summary>
        public QVector[] CalcPlcCompensation(CarrierEnum C, SuckerRowEnum S, QVector camPt, int rowId, int colId)
        {
            if (_plcGrid == null)
                return new[] { new QVector(), new QVector() };

            // M coords 的基準點
            var motorBasePt = getMotorBasePoint(C, S);

            // P 目標值
            var targetWorldPt = _plcGrid.GetGridPoint(rowId, colId);

            // 目標值 轉換至 Camera Coords
            ITransform transCameraToWorld = GetCameraPhysicTransform(C);
            var targetCamPt = transCameraToWorld.InvTrans(targetWorldPt);

            // 目標值 轉換至 Motor Coords
            ITransform transCameraToMotor = GetCameraMotorTransform(C, S);
            var targetMotorPt = transCameraToMotor.Trans(targetCamPt);
            //targetMotorPt.X += motorBasePt.X;
            //targetMotorPt.Y += motorBasePt.Y;

            // 像測現值 轉換至 Motor Coords
            var motorPt = transCameraToMotor.Trans(camPt);
            var curWorldPt = transCameraToWorld.Trans(camPt);
            
            // 自己轉換誤差
            var dErr = targetWorldPt - curWorldPt;

            // 補償量
            var dV = targetMotorPt - motorPt;
            return new QVector[] { dV, dErr };
        }

        /// <summary>
        /// 更新校正格點
        /// </summary>
        public void UpdateCalibPoints(CarrierEnum C, SuckerRowEnum S, EzBlocsGrid camGrid)
        {
            if (camGrid == null)
                return;

            //(0) Camera Coords 四角點
            int r = camGrid.Rows - 1;
            int c = camGrid.Cols - 1;
            var camCornerPts = new[]
            {
                camGrid[0,0].Center,
                camGrid[0,c].Center,
                camGrid[r,c].Center,
                camGrid[r,0].Center,
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

            //(2) 設定 線掃相機 到 Physical (PLC grid) 座標轉換 的 校正點位
            if (false)
            {
                //int r0 = r - camGrid.Rows / 4;
                int r0 = 3;
                camCornerPts = new[]
                {
                    camGrid[r0,0].Center,
                    camGrid[r0,c].Center,
                    camGrid[r,c].Center,
                    camGrid[r,0].Center,
                };
                    var phyCornerPts = new[]
                    {
                    _plcGrid[r0,0],
                    _plcGrid[r0,c],
                    _plcGrid[r,c],
                    _plcGrid[r,0]
                };

                var transCameraToPhysic = this.GetCameraPhysicTransform(C);
                var trfCorners = (ICalibCornerPoints)transCameraToPhysic;
                for (int i = 0; i < camCornerPts.Length; i++)
                {
                    trfCorners.Set(i, camCornerPts[i], phyCornerPts[i]);
                }
                return;
            }
            if (true)
            {
                var camPts = toCalibGrid(camGrid);
                var plcPts = toCalibGrid(_plcGrid);
                var transCameraToPhysic = this.GetCameraPhysicTransform(C);
                var trfGridPoints = transCameraToPhysic.GetCalibGridPoints();
                trfGridPoints.SetAll(camPts, plcPts);
            }
        }

        /// <summary>
        /// 建置全部 座標轉換 系統
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
            foreach (var trf in _transforms)
            {
                trf.Load(iniFileName);
            }
        }
        public void Save(string iniFileName)
        {
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
}
