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


namespace LaserAlignDX.Model.Coords.V35
{
    partial class TravellerTransforms
    {
        public void LoadJson(string fileName)
        {
            if (!System.IO.File.Exists(fileName))
            {
                //this.LoadGaaraIniFile();
                return;
            }

            //_LOG.Info($"載入 [校正參數 (Trf)] @ [{Name}] : {fileName}");

            //_worldGrid.Load(fileName, "GlobalCalibPlcGrid");

            //foreach (var trf in _transforms)
            //{
            //    trf.Load(fileName);
            //}

            //loadCalibCamGrids(fileName, updateToTrf: true);
        }
        public void SaveJson(string fileName)
        {
            //_LOG.Info($"寫入 [校正參數 (Trf)] @ [{Name}] : {fileName}");

            //_worldGrid.Save(fileName, "GlobalCalibPlcGrid");

            //foreach (var trf in _transforms)
            //{
            //    trf.Save(fileName);
            //}

            //saveCalibCamGrids(fileName);
        }
    }
}
