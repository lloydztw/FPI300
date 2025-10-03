#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-08-01 開始重整 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion


using EzAoiEmptyTrayInspector.Model;
using JetEazy.Match;
using JetEazy.Utils;
using LaserAlignDX.OPSpace;
using LaserAlignDX.OPSpace.RecipeSpace;
using System;
using System.Drawing;
using System.Threading;
using Traveller106;


namespace LaserAlignDX.AoiModel.V3
{
    /// <summary>
    /// 空載台檢測
    /// </summary>
    public class AoiModel_EmptyTray : AoiBase
    {
        public override void Run()
        {
            try
            {
                fire_AoiBegin();
                markRunStart();

                _xRecipe.AnalyzeDatasData();    //<<< 在本專案, 貌似沒啥用處

                var aoiModel = _sysModel.EmptyTrayAoiModel;
                bool isAllPass = false;

                Bitmap bmpFullFov = LineScanCamImageHolder.PeekBitmap();
                if (bmpFullFov != null)
                {
                    //复位所有数据
                    ResetCellsResultData();

                    aoiModel.RunAll(bmpFullFov, wait: true);

                    var result = aoiModel.GetResult();

                    isAllPass = UpdateResult(result, bmpFullFov, _xRecipe);

                    // 異步輸出 Debug 數據
                    markFileTimeTag();
                    AsyncSaveDebugData(bmpFullFov);
                }

                markRunEnd(isAllPass);
                fire_AoiEnd();
            }
            catch (Exception ex)
            {
                _LOG_ERROR(ex, $"異常 @ {GetType().Name}.Run");
                markRunEnd(false);
                fire_AoiEnd();
            }
        }

        #region PRIVATE_FUNCTIONS
        /// <summary>
        /// 空载台检测 : 更新結果 到 Gaara 數據群
        /// </summary>
        private bool UpdateResult(EzEmptyTrayResult result, Bitmap bmpFullFov, RecipeFPIX3Class xRecipe)
        {
            bool isAllPass = result != null;

            if (result != null)
            {
                int fullRows = result.FullRows;
                int fullCols = result.FullCols;

                //Z字型对位资料
                //int index = 0;
                //for (int row = 0; row < fullRows; row++)
                //{
                //    if (row % 2 == 1)
                //    {
                //        for (int col = fullCols - 1; col > -1; col--)
                //        {
                //            result.GetBlocByRowCol(row, col, out EzBloc bloc, out bool isOK);
                //            var cell = xRecipe.xRegionCells[index];
                //            InspectReason reason = (isOK ? InspectReason.INS_ALIGNERR : InspectReason.INS_DEFECTERR);
                //            //if (bloc == null)
                //            //    reason = InspectReason.INS_DEFECTERR;
                //            cell.inspectReason = reason;
                //            cell.inspectReasons.Add(reason);
                //            System.Diagnostics.Trace.WriteLine($"[{row}, {col}] is " + (isOK ? "OK" : "NG"));
                //            index++;
                //        }
                //    }
                //    else
                //    {
                //        for (int col = 0; col < fullCols; col++)
                //        {
                //            result.GetBlocByRowCol(row, col, out EzBloc bloc, out bool isOK);
                //            var cell = xRecipe.xRegionCells[index];
                //            InspectReason reason = (isOK ? InspectReason.INS_ALIGNERR : InspectReason.INS_DEFECTERR);
                //            //if (bloc == null)
                //            //    reason = InspectReason.INS_DEFECTERR;
                //            cell.inspectReason = reason;
                //            cell.inspectReasons.Add(reason);
                //            System.Diagnostics.Trace.WriteLine($"[{row}, {col}] is " + (isOK ? "OK" : "NG"));
                //            index++;
                //        }
                //    }
                //}

                int index = 0;
                var xRegionCells = xRecipe.xRegionCells;

                foreach ((int row, int col) in Zigzag.IterZigzag(fullRows, fullCols))
                {
                    if (index >= xRegionCells.Count)
                        break;

                    var cell = xRegionCells[index];
                    result.GetBlocByRowCol(row, col, out EzBloc bloc, out bool isOK);

                    InspectReason reason = (isOK ? InspectReason.INS_ALIGNERR : InspectReason.INS_DEFECTERR);
                    //if (bloc == null)
                    //    reason = InspectReason.INS_DEFECTERR;
                    cell.inspectReason = reason;
                    cell.inspectReasons.Add(reason);
                    //System.Diagnostics.Trace.WriteLine($"[{row}, {col}] is " + (isOK ? "OK" : "NG"));

                    if (!isOK)
                        isAllPass = false;

                    index++;
                }

                #region 收集阵列之外的料件

                foreach (var bloc in result.IterOutGridAbnormalBlocs())
                {
                    xRecipe.xOutBlocs.Add(bloc.Rect);
                    isAllPass = false;
                }

                #endregion
            }

            #region WRITE_TO_LOG
            string msg = result != null ? result.ToString() : "空盤檢測: 無結果";
            GaUtil.LOG(msg, isAllPass ? Color.Green : Color.Red);
            #endregion

            return isAllPass;
        }
        /// <summary>
        /// LETIAN: 非同步保存 Debug 數據 搬移至此.
        /// 此函式 負責 bmpInputImage 生命
        /// </summary>
        private void AsyncSaveDebugData(Bitmap bmpFullFov)
        {
            if (!INI.Instance.IsSaveDebugBMP || bmpFullFov == null)
                return;
                
            ThreadPool.QueueUserWorkItem(arg =>
            {
                try
                {
                    using (Bitmap bmpBig = (Bitmap)arg)
                    {
                        if (INI.Instance.IsSaveDebugBMP)
                        {
                            //GaImageUtil.SaveImageWithQuality(bmpBig,
                            //    $"{m_PicResultPath}\\{LotId}-{DateTime.Now.ToString("yyyyMMddHHmmss")}.jpg", INI.Instance.ImageQuality);
                            string fileName = GetDebugBmpFileName();
                            GaImageUtil.SaveImageWithQuality(bmpBig, fileName, INI.Instance.ImageQuality);
                        }

                        if (INI.Instance.IsSaveDebugOrgBmp)
                        {
                            //ezImage.Save($"{m_PicResultOrgPath}\\{LotId}-{DateTime.Now.ToString("yyyyMMddHHmmss")}.jpg");
                            //bmpInputImage.Save($"{m_PicResultOrgPath}\\{LotId}-{DateTime.Now.ToString("yyyyMMddHHmmss")}.jpg",
                            //                   ImageFormat.Jpeg);
                            string fileName = GetDebugOrgBmpFileName();
                            GaImageUtil.SaveBigImage(fileName, bmpBig);
                        }
                    }
                }
                catch (Exception ex)
                {
                    //_LOG($"异常捕获:{ex.Message}", Color.Red);
                    _LOG_ERROR(ex, "_Inspect003_Async_SaveDebugData");
                }
            },
                bmpFullFov.Clone()
            );
        }
        #endregion
    }
}
