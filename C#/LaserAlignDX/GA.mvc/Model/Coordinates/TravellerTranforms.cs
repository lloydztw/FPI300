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
using LaserAlignDX.Model.Transforms;
using System;
using System.Windows.Controls;
using System.Windows.Forms;


namespace LaserAlignDX.Model.Coords
{
    /// <summary>
    /// Traveller106 專案 的 所有座標系
    /// </summary>
    public class TravellerTransforms : IDisposable
    {
        #region PRIVATE_DATA
        QTransform[] _transforms = new QTransform[]
        {
            new QTransform("C1_P", 10000, "pix", 1000, "mm"),
            new QTransform("C1_M1S1", 10000, "pix", 1000, "mm"),
            new QTransform("C1_M1S2", 10000, "pix", 1000, "mm"),
            new QTransform("C2_P", 10000, "pix", 1000, "mm"),
            new QTransform("C2_M2S1", 10000, "pix", 1000, "mm"),
            new QTransform("C2_M2S2", 10000, "pix", 1000, "mm"),
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

        public void Load(string iniFileName)
        {
            foreach (var trf in _transforms)
            {
                trf.Load(iniFileName, trf.Name);
            }
        }
        public void Save(string iniFileName)
        {
            foreach (var trf in _transforms)
            {
                trf.Save(iniFileName, trf.Name);
            }
        }
        
        public void UpdateCalibPoints(CarrierEnum C, SuckerRowEnum S, EzBlocsGrid camGrid, PlcGridPoints plcGrid)
        {
            if (camGrid == null)
                return;

            //(0) Camera Coords 四角點
            int r = camGrid.Rows - 1;
            int c = camGrid.Cols - 1;
            var cornerPts = new[]
            {
                camGrid[0,0].Center,
                camGrid[0,c].Center,
                camGrid[r,c].Center,
                camGrid[r,0].Center,
            };

            //(1) 設定 線掃相機 到 馬達 (Carrier + Sucker) 座標轉換 的 校正點位
            QVector motorLeftTop = new QVector();
            if (true)
            {
                var transCameraToMotor = this.GetCameraMotorTransform(C, S);
                for (int i = 0; i < cornerPts.Length; i++)
                {
                    var camCoord = transCameraToMotor.GetSrcRef(i);
                    var motorCoord = transCameraToMotor.GetDstRef(i);
                    camCoord.X = cornerPts[i].X;
                    camCoord.Y = cornerPts[i].Y;
                    transCameraToMotor.SetSrcRef(i, camCoord);
                    //updateCalibKeyPoints(_dgvCalibPointsListView, i, camCoord, motorCoord, false);
                    //updateCalibKeyPointBox((CalibCornersEnum)i, camCoord);
                    if (i == 0)
                        motorLeftTop = new QVector(motorCoord);
                }
            }

            //(2) 設定 線掃相機 到 Physical (PLC grid) 座標轉換 的 校正點位
            //var plcGrid = new PlcGridPoints(rows, cols, pitchX, pitchY);
            if (plcGrid == null) 
                return;
            plcGrid.Offset(motorLeftTop.X, motorLeftTop.Y);
            var physicPts = new[]
            {
                plcGrid[0,0],
                plcGrid[0,c],
                plcGrid[r,c],
                plcGrid[r,0]
            };
            if (true)
            {
                var transCameraToPhysic = this.GetCameraPhysicTransform(C);
                for (int i = 0; i < cornerPts.Length; i++)
                {
                    var camCoord = transCameraToPhysic.GetSrcRef(i);
                    var physicCoord = transCameraToPhysic.GetDstRef(i);
                    camCoord.X = cornerPts[i].X;
                    camCoord.Y = cornerPts[i].Y;
                    physicCoord.X = physicPts[i].X;
                    physicCoord.Y = physicPts[i].Y;
                    transCameraToPhysic.SetSrcRef(i, camCoord);
                    transCameraToPhysic.SetDstRef(i, physicCoord);
                }
            }
        }
        public void BuildAll()
        {
            foreach (var trf in _transforms)
                trf?.Build();
        }
    }
}
