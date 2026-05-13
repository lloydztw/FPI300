#region AUTHOR
/*
 * 
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2026-05-13 Revised for V36
 *      2026-04-02 Added V35
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
using System;
using System.Collections.Generic;

namespace LaserAlignDX.Model.Coords
{
    public static class TrfMigration
    {
        /// <summary>
        /// 根據 新相機格點 newCamGrid, 格點節距 newPitch 以及 commonBaseTrf 為基礎,
        /// 進行線性遷移 生成新的 座標轉換系統
        /// </summary>
        /// <remarks>
        /// 新相機格點 newCamGrid 的相機工作高度與 commonBaseTrf 不同
        /// </remarks>
        public static ITravellerTransforms CreateLinearMigration(string name, CarrierEnum carrierID, EzBlocsGrid newCamGrid, QVector newPitch, IList<JxTraySegItem> segsList)
        {
            if (string.IsNullOrEmpty(name) || newCamGrid == null || newPitch == null)
                return null;

            try
            {
                GaUtil.LOG($"線性遷移 [{name}] [{carrierID}] : 開始 : (R,C)=({newCamGrid.Rows},{newCamGrid.Cols}) : PitchX= {newPitch.X} : PitchY= {newPitch.Y}");

                //(0) CommonBase
                var commonBaseTrf = TravellerTransformFactory.CommonBase;
                commonBaseTrf.Load(GaMvcPaths.COMMON_BASE_TRANSFORMS_INI_FILE);
                commonBaseTrf.BuildAll();

                //(1) Instance
                var newTrf = TravellerTransformFactory.Instance(name);

                //(2) [線性遷移] Camera-World 座標轉換系統 : 重新設定 rows, cols, pitchX, pitchY 布局
                newTrf.ConfigWorldGridPoints(carrierID, newCamGrid.Rows, newCamGrid.Cols, newPitch.X, newPitch.Y);

                //(3) [線性遷移] Camera-World 座標轉換系統 : 重新設定 相機格點
                newTrf.SetCalibCamGrid(carrierID, newCamGrid, segsList);

                //(4) [線性遷移] Camera-Motor 座標轉換系統 (使用 commonBaseTrf 原來的 Camera-Motor)
                foreach (SuckerRowEnum suckerID in Enum.GetValues(typeof(SuckerRowEnum)))
                {
                    var trfCamMotorSrc = commonBaseTrf.GetCameraMotorTransform(carrierID, suckerID);
                    var trfCamMotorDst = newTrf.GetCameraMotorTransform(carrierID, suckerID);

                    //var srcCalib = trfCamMotorSrc.GetCalibGridPoints();
                    //var dstCalib = trfCamMotorDst.GetCalibGridPoints();
                    //int rows = srcCalib.Rows;
                    //int cols = srcCalib.Cols;
                    //var camPts = new QVector[rows, cols];
                    //var motorPts = new QVector[rows, cols];
                    //for (int r = 0; r < rows; r++)
                    //    for (int c = 0; c < cols; c++)
                    //        srcCalib.Get(r, c, out camPts[r, c], out motorPts[r, c]);
                    //dstCalib.SetAll(camPts, motorPts);

                    trfCamMotorSrc.CopyTo(trfCamMotorDst);
                    trfCamMotorDst.Build();
                }

                //(5) 修正 相機工作距離 之差異
                BuildWorkDistAdjustment(newTrf, commonBaseTrf, carrierID);

                //(6) Build
                newTrf.BuildAll();

                GaUtil.LOG($"線性遷移 [{name}] [{carrierID}] : 完成");
                return newTrf;
            }
            catch (Exception ex)
            {
                GaUtil.LOG_ERROR(ex, $"線性遷移 [{name}] [{carrierID}] 異常");
                throw;
            }
        }

        static void CopyTo(this ITransform srcTrfCM, ITransform dstTrfCM)
        {
            //var srcTrfCM = commonBaseTrf.GetCameraMotorTransform(carrierID, suckerID);
            //var dstTrfCM = newTrfModel.GetCameraMotorTransform(carrierID, suckerID);

            var srcCalib = srcTrfCM.GetCalibGridPoints();
            var dstCalib = dstTrfCM.GetCalibGridPoints();

            int rows = srcCalib.Rows;
            int cols = srcCalib.Cols;
            var camPts = new QVector[rows, cols];
            var motorPts = new QVector[rows, cols];
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    srcCalib.Get(r, c, out camPts[r, c], out motorPts[r, c]);

            dstCalib.SetAll(camPts, motorPts);
        }

        static void BuildWorkDistAdjustment(ITravellerTransforms trfModel, ITravellerTransforms commonBaseTrf, CarrierEnum carrierID)
        {
            if (trfModel is LaserAlignDX.Model.Coords.V36.TravellerTransforms trfV36)
                trfV36.BuildWorkDistAdjustment(carrierID, commonBaseTrf);
        }
    }
}
