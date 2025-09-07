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

using LaserAlignDX.Model.Transforms;
using System;


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
        
        public void BuildAll()
        {
            foreach (var trf in _transforms)
                trf?.Build();
        }
    }
}
