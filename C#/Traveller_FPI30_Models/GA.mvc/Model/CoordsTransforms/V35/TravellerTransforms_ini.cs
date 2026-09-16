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
using System;


namespace LaserAlignDX.Model.Coords.V35
{
    partial class TravellerTransforms
    {
        void LoadIni(string iniFileName)
        {
            if (!System.IO.File.Exists(iniFileName))
            {
                //this.LoadGaaraIniFile();
                return;
            }

            //_LOG.Info($"載入 [校正參數 (Trf)] @ [{Name}] : {iniFileName}");

            _worldGrid.Load(iniFileName, "GlobalCalibPlcGrid");

            foreach (var trf in _transforms)
            {
                trf.Load(iniFileName);
            }

            loadCalibCamGrids(iniFileName, updateToTrf: true);
            loadMiscs(iniFileName);
        }
        void SaveIni(string iniFileName)
        {
            //_LOG.Info($"寫入 [校正參數 (Trf)] @ [{Name}] : {iniFileName}");
            _worldGrid.Save(iniFileName, "GlobalCalibPlcGrid");

            foreach (var trf in _transforms)
            {
                trf.Save(iniFileName);
            }

            saveCalibCamGrids(iniFileName);
            saveMiscs(iniFileName);
        }

        #region PRIVATE_FILE_IO_FUNCTIONS
        string normalizeCalibCamGridDataFile(string iniFileName)
        {
            return System.IO.Path.ChangeExtension(iniFileName, ".GridBoard.dat");
        }
        void loadCalibCamGrids(string iniFileName, bool updateToTrf)
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

                if (updateToTrf)
                {
                    var trf = GetCameraPhysicTransform((CarrierEnum)i);
                    updateCalibPointsToTrf(trf, _calibCamGrids[i], _worldGrid);
                }
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
        void loadMiscs(string iniFileName)
        {
            double dist = 0;
            JetEazy.Win32.Win32Ini.Load(ref dist, iniFileName, "Misc", "CameraWorkDist");
            CameraWorkDist = dist;
        }
        void saveMiscs(string iniFileName)
        {
            JetEazy.Win32.Win32Ini.Save(CameraWorkDist, iniFileName, "Misc", "CameraWorkDist");
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
        QVector[,] toCalibGrid(IWorldGridPoints plcGrid)
        {
            int rows = plcGrid.Rows;
            int cols = plcGrid.Cols;
            var pts = new QVector[rows, cols];
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    pts[r, c] = plcGrid.Get(r, c);
            return pts;
        }
        #endregion
    }
}
