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

using JetEazy.QMath;
using JetEazy.QvMath;
using JetEazy.Utils;
using LaserAlignDX.BasicSpace;
using LaserAlignDX.OPSpace;
using LaserAlignDX.OPSpace.RecipeSpace;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using Traveller106;

namespace LaserAlignDX.AoiModel.V3
{
    /// <summary>
    /// QR CODE
    /// </summary>
    public class AoiModel_QrCode : AoiModelBase, IAoiQrDecoder
    {
        #region CONFIG
        static bool N_THREADS_ENABLED => GlobalConfig.N_THREADS_ENABLED;
        static int N_THREADS => GlobalConfig.N_THREADS;
        #endregion

        #region GLOBAL_MESS
        InspectX3ParaClass _xInspect => base._xRecipe.InspectParams;
        #endregion

        #region KERNEL_MEMBERS
        Mvd2DReaderClass[] _decoders = new Mvd2DReaderClass[0];
        #endregion

        #region RUNTIME_DATA
        GaCellsGroup[] _cellGroups;
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

        public override void Dispose()
        {
            var oldItems = _decoders;
            _decoders = new Mvd2DReaderClass[0];
            foreach(var item in oldItems)
                item?.Dispose();
        }

        public void SetCellGroups(GaCellsGroup[] cellGroups)
        {
            this._cellGroups = cellGroups;
        }

        public override void Run(Bitmap sceneBmp = null)
        {
            bool go = QrUsed || QrJudged;
            if (!go)
                return;

            try
            {
                fire_AoiBegin();
                markRunStart();

                Bitmap bmpFullfov = LineScanCamImageHolder.PeekBitmap();
                RunAllChipsDecoding(bmpFullfov);

                markRunEnd(true);
                fire_AoiEnd();
            }
            catch (Exception ex)
            {
                base.HandleAoiException(ex);
            }
        }

#if (OPT_RESERVED)
        /// <summary>
        /// 調試 使用 (一次只測一個 cell)
        /// </summary>
        public void TryRunOneChipDecode(RegionCellX3Class cell, Bitmap cellBmp, ref RectangleF cellRoi)
        {
            DecodeOne(cell, cellBmp, ref cellRoi, 0, true);
        }
#endif

        public string TryDecode(Bitmap bmp, Rectangle? roi = null)
        {
            if (bmp != null )
            {
                prepareDecoder(1);
                var decoder = _decoders[0];

                RectangleF roiF;
                if (roi == null)
                {
                    roiF = new RectangleF(0, 0, bmp.Width, bmp.Height);
                }
                else
                {
                    roiF = roi.Value;
                    GaUtil.Clip(ref roiF, bmp.Width, bmp.Height);
                }
                if (roiF.Width < 2 || roiF.Height < 2)
                    return "";

                decoder.Run(bmp, roiF);
                var decodeInfo = decoder.DCodeInfo;
                var code = (decodeInfo?.Content) ?? "";
                return code;
            }

            return "";
        }

        #region PRIVATE_FUNCTIONS
        private void prepareDecoder(int NThreads)
        {
            if (NThreads > _decoders.Length)
            {
                var lst = new List<Mvd2DReaderClass>(_decoders);
                for (int i = _decoders.Length; i < NThreads; i++)
                {
                    lst.Add(new Mvd2DReaderClass());
                }
                _decoders = lst.ToArray();
            }
        }

        /// <summary>
        ///  將 Gaara 的 瑕疵檢測 移植為多線程
        /// </summary>
        private void RunAllChipsDecoding(Bitmap bmpFullfov)
        {
            fire_AoiBegin("QR CODE decoding");

            // 暫時強制使用 single thread
            bool usingMultiThread = N_THREADS_ENABLED;

            #region 準備_CELL_GROUPS
            int N_GROUPS = _cellGroups != null ? _cellGroups.Length : N_THREADS;
            var groups = _cellGroups != null ? _cellGroups : GaCellsGroup.CollectGroups(N_GROUPS, _xRecipe, bmpFullfov);
            if (groups == null || groups.Length == 0)
                return;
            prepareDecoder(groups.Length);
            #endregion

            if (!usingMultiThread)
            {
                // 單線程 (驗證用)
                for (int gid = 0; gid < groups.Length; gid++)
                {
                    RunGroupChipsDecodeOneT(gid, groups[gid]);
                }
            }
            else
            {
                // 多線程
                Parallel.For(0, groups.Length, gid =>
                {
                    RunGroupChipsDecodeOneT(gid, groups[gid]);
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
        /// QRCODE 檢測 (數群晶粒) (限用於同一線程內)
        /// </summary>
        private void RunGroupChipsDecodeOneT(int threadIdx, GaCellsGroup cellsGroup)
        {
            foreach (var gaCell in cellsGroup)
            {
                RegionCellX3Class cell = gaCell.Cell;
                Bitmap cellBmp = gaCell.CellBmp;
                RectangleF cellRoi = gaCell.CellRoi;

                fire_AoiProgressing(cell);

                DecodeOne(cell, cellBmp, ref cellRoi, threadIdx, false);
            }
        }

        /// <summary>
        /// QRCODE 解碼
        /// </summary>
        private void DecodeOne(RegionCellX3Class cell, Bitmap cellBmp, ref RectangleF cellRoi, int threadIdx, bool force = false)
        {
            if (cell == null)
                return;

            var decoder = _decoders[threadIdx];

            //(0) Empty Result
            cell.BarcodeResultText = "";
            cell.BarcodeResultQuad = null;

            if (decoder != null)
            {
                //(1) qrRect
                var qrRect = _xRecipe.QrCodeRect;
                qrRect.X += cellRoi.X;
                qrRect.Y += cellRoi.Y;

                //(2) 條件: 前段測試 pass 的晶粒才進行 瑕疵檢測
                bool go = cell.ChipData?.ChipQuad2D != null && cell.IsResultPass();

                //(3) 條件: 是否為 Bypass
                if (cell.ByPass && !INI.Instance.IsForceInspect)
                    go = false;

                //(4) 條件: 是否為 空白 或錯 誤區塊
                if (cell.IsEmptyPlaceHold() || cell.IsAmbiguousBloc())
                    go = false;

                //(5) 使用 MVD QR Decoder
                if (go || force)
                {
                    decoder.Run(cellBmp, qrRect);

                    var decodeInfo = decoder.DCodeInfo;
                    if (decodeInfo != null)
                    {
                        var code = (decodeInfo?.Content) ?? "";
                        cell.BarcodeResultText = code;

                        var mvdPositions = new[]
                        {
                            decodeInfo.Position[0], // 左上
                            decodeInfo.Position[1], // 右上
                            decodeInfo.Position[3], // 右下
                            decodeInfo.Position[2], // 左下
                        };

                        var corners = Array.ConvertAll(mvdPositions, p => new QVector2(p.nX, p.nY));
                        cell.BarcodeResultQuad = new QvQuad2D()
                        {
                            Corners = corners
                        };
                    }
                }
            }
        }
        #endregion
    }
}
