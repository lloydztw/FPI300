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


using JetEazy.Utils;
using LaserAlignDX.OPSpace;
using LaserAlignDX.OPSpace.RecipeSpace;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using Traveller106;


namespace LaserAlignDX.AoiModel.V3
{
    public class AoiModel_DefectsQrCode : AoiBase
    {
        #region GLOBAL_MESS
        InspectX3ParaClass _xInspect => base._xRecipe.InspectParams;
        #endregion

        #region KENERL_MEMBERS
        ///// <summary>
        ///// 2025-08-28 LETIAN: 巨圖 統一由 TravellerBigImagesHolder 保管其生命週期
        ///// </summary>
        //public GaBigImageHolder LineScanCamImageHolder => _sysModel.LineScanImageHolder;
        ///// <summary>
        ///// 2025-09-10 新座標轉換
        ///// </summary>
        //TravellerTransforms _transformModel => _sysModel.TransformsModel;
        ///// <summary>
        ///// SystemModel
        ///// </summary>
        //ITravelerModel _sysModel => GaMvcConfig.SysModel;
        ///// <summary>
        ///// 當下的載台
        ///// </summary>
        //CarrierEnum getActiveCarrierID()
        //{
        //    return _sysModel.ActiveCarrierID;
        //}
        #endregion

        public bool QrUsed
        {
            get;
            set;
        }
        public bool QrJudged
        {
            get;
            set;
        }

        public override void Run()
        {
            bool go = (QrUsed || _xInspect.optChipDefectsInspect);
            if (!go)
                return;

            try
            {
                fire_AoiBegin();
                markRunStart();

                Bitmap bmpFullfov = LineScanCamImageHolder.PeekBitmap();
                RunDefectsAndQrCode(bmpFullfov);

                markRunEnd(true);
                fire_AoiEnd();
            }
            catch (Exception ex)
            {
                // 2025-08-28 LETIAN: 巨圖統一由 LineScanCamImageHolder 管理其生命週期
                // 在此無需釋放 巨圖
                markRunEnd(false);
                fire_AoiEnd();
                var errCode = Mvc.Model.ErrCodes.EXCEPTION_AT_AOI_RUN;
                string errMsg = GaUtil.GetEnumDescription(errCode) + "\n\r" + ex.Message;
                fire_AoiError(errCode, errMsg);
                GaUtil.LOG(errMsg, Color.Red);
                _LOG_ERROR(ex, $"異常 @ {GetType().Name}.Run");
            }
        }

        #region PRIVATE_FUNCTIONS
        private void RunDefectsAndQrCode(Bitmap bmpFullfov)
        {
            //if (m_QrUsed || _xInspect.optChipDefectsInspect)
            {
                var cells = _xRecipe.xRegionCells;
                foreach (var cell in cells)
                {
                    if (cell.ByPass && !INI.Instance.IsForceInspect)
                        continue;

                    if (cell.inspectReason == InspectReason.INS_ALIGNERR)
                        continue;

                    // Golden Region Size
                    var regionSize = _xRecipe.bmpprinttemplate.Size;

                    // 定位完成后裁切位置
                    // RectangleF _crop = new RectangleF(
                    //    cell.DrawResultRectF().CenterX - regionSize.Width / 2,
                    //    cell.DrawResultRectF().CenterY - regionSize.Height / 2,
                    //    regionSize.Width,
                    //    regionSize.Height);

                    var mvdRect = cell.DrawResultRectF();
                    var regionRoi = JetEazy.Qcvt.CreateCenterRect(mvdRect.CenterX, mvdRect.CenterY, regionSize.Width, regionSize.Height);

                    if (_xInspect.optChipDefectsInspect)
                    {
                        try
                        {
                            var bmpTemplate = _xRecipe.bmpDefectTemplate;
                            var bmpMask = _xRecipe.bmpprintmask;
                            var roi = _xRecipe.xRegionTrain;
                            roi.X += regionRoi.X;
                            roi.Y += regionRoi.Y;

                            using (var bmpRun = bmpFullfov.Clone(roi, PixelFormat.Format8bppIndexed))
                            {
                                cell.DetectDefects(bmpTemplate, bmpRun, bmpMask);
                            }
                        }
                        catch (Exception ex)
                        {
                            _LOG_ERROR(ex, "cell.DetectDefects 異常!");
                            //_xInspect.optChipDefectsInspect = false;
                        }
                    }

                    if (QrUsed)
                    {
                        try
                        {
                            //RectangleF _cropCode = new RectangleF(
                            //    xRecipe.xRectCodeRegion.X + regionRoi.X,
                            //    xRecipe.xRectCodeRegion.Y + regionRoi.Y,
                            //    xRecipe.xRectCodeRegion.Width,
                            //    xRecipe.xRectCodeRegion.Height);
                            //cell.bmpItemCodeRun?.Dispose();
                            //cell.bmpItemCodeRun = bmpFullfov.Clone(_cropCode, PixelFormat.Format8bppIndexed);

                            var roi = _xRecipe.xRectCodeRegion;
                            roi.X += regionRoi.X;
                            roi.Y += regionRoi.Y;
                            using (var bmpRun = bmpFullfov.Clone(roi, PixelFormat.Format8bppIndexed))
                            {
                                //cell.DeCode2D(cell.bmpItemCodeRun, _cropCode.Location, m_QrJudged);
                                cell.DeCode2D(bmpRun, roi.Location, QrJudged);
                            }
                        }
                        catch (Exception ex)
                        {
                            _LOG_ERROR(ex, "cell.DeCode2D 異常!");
                            //xInspect.m_QrUsed = false;
                        }
                    }
                }
            }
        }
        #endregion
    }
}
