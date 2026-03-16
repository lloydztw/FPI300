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


using JetEazy.OpenCV;
using JetEazy.QMath;
using JetEazy.QvMath;
using JetEazy.Utils;
using LaserAlignDX.BasicSpace;
using LaserAlignDX.Model;
using LaserAlignDX.Model.Coords;
using LaserAlignDX.OPSpace;
using LaserAlignDX.OPSpace.RecipeSpace;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Traveller106;
using VisionDesigner;
using VisionDesigner.ImageArithmetic;
using VisionDesigner.PositionFix;
using ErrorCodes = LaserAlignDX.Mvc.Model.ErrorCodes;


namespace LaserAlignDX.AoiModel.V3
{
    /// <summary>
    /// 晶粒尺寸量測
    /// </summary>
    public partial class AoiModel_Defects : AoiModelBase
    {
        #region GLOBAL_MESS
        InspectX3ParaClass _xInspect => base._xRecipe.InspectParams;
        #endregion

        #region KERNEL_MEMBERS
        //ITransform _worldTransform;
        //QMicroChipTransform _microTransform;
        IMicroChipTransform _microTransform;
        #endregion

        #region RUNTIME_DATA
        GaCellsGroup[] _cellGroups;
        bool _is2ndRun;
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

        public void SetCellGroups(GaCellsGroup[] cellGroups)
        {
            this._cellGroups = cellGroups;
        }

        public override void Run()
        {
            bool go = _xInspect.optChipDefectsInspect || QrUsed;
            if (!go)
                return;

            try
            {
                fire_AoiBegin();
                markRunStart();

                Bitmap bmpFullfov = LineScanCamImageHolder.PeekBitmap();
                RunAllChipsDefects(bmpFullfov);

                markRunEnd(true);
                fire_AoiEnd();
            }
            catch (Exception ex)
            {
                // 2025-08-28 LETIAN: 巨圖統一由 LineScanCamImageHolder 管理其生命週期
                // 在此無需釋放 巨圖
                markRunEnd(false);
                fire_AoiEnd();
                var errCode = ErrorCodes.EXCEPTION_AT_AOI_RUN;
                string errMsg = GaUtil.GetEnumDescription(errCode) + "\n\r" + ex.Message;
                GaUtil.LOG(errMsg, Color.Red);
                _LOG_ERROR(ex, $"異常 @ {GetType().Name}.Run");
                fire_AoiError(errCode, errMsg);
            }
        }

        /// <summary>
        /// 調試 使用 (一次只測一個 cell)
        /// </summary>
        internal void TryRunOneChipDefects(RegionCellX3Class cell, Bitmap cellBmp, ref RectangleF cellRoi)
        {
            // 取得 上一輪 晶粒定位 的結果 (chipData)
            var chipData = cell?.ChipData;
            if (chipData == null || chipData.ChipQuad2D == null)
                return;
            RunOneChipDefects(cell, cellBmp, ref cellRoi);
        }

        #region PRIVATE_FUNCTIONS
        /// <summary>
        ///  將 Gaara 的 瑕疵檢測 移植為多線程
        /// </summary>
        private void RunAllChipsDefects(Bitmap bmpFullfov)
        {
            if (!_xInspect.optChipDefectsInspect)
                return;

            fire_AoiBegin("晶粒瑕疵檢測");

            // 暫時強制使用 single thread
            bool usingMultiThread = false; // Universal.N_THREADS_ENABLED;

            #region 準備_CELL_GROUPS
            int N_GROUPS = _cellGroups != null ? _cellGroups.Length : MvdCompositeChipMatcher.N_CHANNLS;
            var groups = _cellGroups != null ? _cellGroups : GaCellsGroup.CollectGroups(N_GROUPS, _xRecipe, bmpFullfov);
            if (groups == null || groups.Length == 0)
                return;
            #endregion

            if (!usingMultiThread)
            {
                // 單線程 (驗證用)
                for (int gid = 0; gid < groups.Length; gid++)
                {
                    RunGroupChipsDefectsOneT(gid, groups[gid]);
                }
            }
            else
            {
                // 多線程
                Parallel.For(0, groups.Length, gid =>
                {
                    RunGroupChipsDefectsOneT(gid, groups[gid]);
                });
            }

            #region CLEAN_UP
            if (groups != _cellGroups)
            {
                GaCellsGroup.DisposeAll(groups);
            }
            #endregion
        }

        /// <summary>
        /// 瑕疵檢測 (數群晶粒) (限用於同一線程內)
        /// </summary>
        private void RunGroupChipsDefectsOneT(int threadIdx, GaCellsGroup cellsGroup)
        {
            var fullFovSize = cellsGroup.FullFovRect.Size;
            var debugSB = new StringBuilder();

            foreach (var gaCell in cellsGroup)
            {
                RegionCellX3Class cell = gaCell.Cell;
                Bitmap cellBmp = gaCell.CellBmp;
                RectangleF cellRoi = gaCell.CellRoi;

                //(1) 進度條事件
                fire_AoiProgressing(cell);

                //(2.1) 條件: 前段測試 pass 的晶粒才進行 瑕疵檢測
                bool go = cell.ChipData?.ChipQuad2D != null && cell.IsResultPass();

                //(2.2) 條件: 是否為 Bypass
                if (cell.ByPass && !INI.Instance.IsForceInspect)
                    go = false;

                //(2.3) 條件: 是否為 空白 或錯 誤區塊
                if (cell.IsEmptyPlaceHold() || cell.IsAmbiguousBloc())
                    go = false;

                if (go)
                {
                    //(3) 單一晶粒 瑕疵檢測
                    RunOneChipDefects(cell, cellBmp, ref cellRoi);
                }
            }
        }

        /// <summary>
        /// 瑕疵檢查 (單一晶粒) 
        /// </summary>
        private void RunOneChipDefects(RegionCellX3Class cell, Bitmap cellBmp, ref RectangleF cellRoi)
        {
            try
            {
                CMvdRectangleF mvdRectX;

                if (_xInspect.xAlgorithm == MatchAlgorithmEnum.GridMatch)
                {
                    var templateSize = _xRecipe.bmpDefectTemplate.Size;
                    var templateQuad = QvQuad2D.From(new RectangleF(0, 0, templateSize.Width, templateSize.Height));

                    var goldenChipQuad = cell.ChipData.GoldenQuad2D;
                    var goldenChipCenter = goldenChipQuad.Center;

                    var runtimeChipQuad = cell.ChipData.ChipQuad2D.Clone();
                    runtimeChipQuad.Offset(-cellRoi.X, -cellRoi.Y);

                    var templatePoints = Array.ConvertAll(templateQuad.Corners, c => new OpenCvSharp.Point2f((float)c.X, (float)c.Y));
                    var goldenPoints = Array.ConvertAll(goldenChipQuad.Corners, c => new OpenCvSharp.Point2f((float)c.X, (float)c.Y));
                    var runtimePoints = Array.ConvertAll(runtimeChipQuad.Corners, c => new OpenCvSharp.Point2f((float)c.X, (float)c.Y));

                    // 將 templatePoints 從 golden domain 投影回到 runtime domain
                    using (var matrix = Cv2.GetPerspectiveTransform(goldenPoints, runtimePoints))
                    {
                        var crop_pts = Cv2.PerspectiveTransform(templatePoints, matrix);
                        var crop_rotatedRect = Cv2.MinAreaRect(crop_pts);
                        var crop_cx = crop_rotatedRect.Center.X;
                        var crop_cy = crop_rotatedRect.Center.Y;
                        var crop_width = crop_rotatedRect.Size.Width;
                        var crop_height = crop_rotatedRect.Size.Height;
                        var angle = crop_rotatedRect.Angle;
                        mvdRectX = new CMvdRectangleF((float)crop_cx, (float)crop_cy, crop_width, crop_height)
                        {
                            Angle = (float)angle
                        };
                    }
                }
                else
                {
                    var templateSize = _xRecipe.bmpDefectTemplate.Size;
                    var chipCenter = cell.ChipData.ChipQuad2D.Center;
                    var chipAngle = cell.ChipData.ChipQuad2D.Angle;

                    // 取的 center 的 local 圖像座標 (cellRoi 左上角為 (0,0))
                    double cx = chipCenter.X - cellRoi.X;
                    double cy = chipCenter.Y - cellRoi.Y;

                    // 以 (cx,cy) 為中心 建立 長寬為 templateSize, 角度為 chipAngle 的 海康矩形
                    // 如果把 _xRecipe.bmpDefectTemplate 當成無角度的 rectangle 就不需要減 GoldenQuad2D.Angle
                    mvdRectX = new CMvdRectangleF((float)cx, (float)cy, templateSize.Width, templateSize.Height)
                    {
                        Angle = (float)chipAngle
                    };
                }

                var bmpTemplate = _xRecipe.bmpDefectTemplate;
                var bmpMask = _xRecipe.bmpprintmask;

                //*****************************************************************************
                // 利用海康 對 cellBmp Affine Transform 
                // (注意:此處會被多線程 同時調用)
                //*****************************************************************************
                using (var cMvdImage = GaImageUtil.BitmapToCMvdImage(cellBmp))
                using (var bmpRun = cell.GetAffineTrainsFormRunBmp(cMvdImage, mvdRectX))
                {
                    // 使用海康進行 瑕疵檢測
                    cell.DetectDefects(bmpTemplate, bmpRun, bmpMask);
                }
            }
            catch (Exception ex)
            {
                _LOG_ERROR(ex, $"RunOneChipDefects 異常 @ ({cell.CellRow},{cell.CellCol})!");
                //_xInspect.optChipDefectsInspect = false;
            }
        }
        #endregion
    }
}
