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
using JetEazy.OpenCV;
using JetEazy.QMath;
using JetEazy.QvMath;
using JetEazy.Transform;
using JetEazy.Utils;
using LaserAlignDX.Model;
using LaserAlignDX.Model.Coords;
using LaserAlignDX.OPSpace;
using LeTian.AoiLib;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using Traveller106;
using _TM = LeTian.AoiLib.LtDebug;


namespace LaserAlignDX.AoiModel.V3
{
    /// <summary>
    /// 晶粒定位
    /// </summary>
    public class AoiModel_ChipLoc : AoiModelBase
    {
        #region GLOBAL_MESS
        #endregion

        #region KERNEL_MEMBERS
        /// <summary>
        /// 2025-09-10 新座標轉換
        /// </summary>
        TravellerTransforms _transformModel => _sysModel.TransformsModel;
        ITransform _transCP;
        ITransform _transCS1;
        ITransform _transCS2;
        #endregion

        #region RUNTIME_DATA
        GaCellsGroup[] _cellGroups;
        EzEmptyTrayResult _preEmptyTrayResult;
        #endregion

        public GaCellsGroup[] CellGroups
        {
            get { return _cellGroups; }
        }

        public void DisposeCellGroups()
        {
            var old = _cellGroups;
            _cellGroups = null;
            GaCellsGroup.DisposeAll(old);
        }

        public override void Run()
        {
            try
            {
                fire_AoiBegin("晶粒定位");

                //(0) 效能追蹤
                _TM.Reset();

                //_xRecipe.AnalyzeDatasData();
                //_TM.Trace("_Inspect001 : xRecipe.AnalyzeDatasData()");

                //(0.1) 標記起始計時
                markRunStart();

                //(1) 清除上一次所有結果
                ResetCellsResultData();

                //(2) 準備資料夾
                string imgLogPath = GetLogPath(this.FileBarcodeStr);
                #region PREPARE_PATH
                if (INI.Instance.IsSaveTestImage)
                {
                    if (!Directory.Exists(imgLogPath))
                        Directory.CreateDirectory(imgLogPath);
                }
                #endregion

                //(3) 取得座標轉換
                var activeCarrierID = getActiveCarrierID();
                _transCP = _transformModel.GetCameraPhysicTransform(activeCarrierID);
                _transCS1 = _transformModel.GetCameraMotorTransform(activeCarrierID, SuckerRowEnum.S1);
                _transCS2 = _transformModel.GetCameraMotorTransform(activeCarrierID, SuckerRowEnum.S2);

                //(4) 取得線掃巨圖: 2025-08-28 LETIAN: 巨圖統一由 LineScanCamImageHolder 管理其生命週期
                Bitmap bmpFullfov = LineScanCamImageHolder.PeekBitmap();

                //(5) PreEmptyTray
                _PreInspectEmptyTray(bmpFullfov);

                //(6) 晶粒定位
                _RunChipsLocate(bmpFullfov, imgLogPath, out string debugCellCenterStr);

                //(7) 晶粒定位 (位於邊界模擬兩可之處)
                _RunChipsLocateOnBoundary(bmpFullfov, imgLogPath);

                //(8) 晶粒定位 (偏離格位外)
                _RunChipsLocateOutGrid(bmpFullfov, imgLogPath);

                //(9) 異步輸出 Debug 數據
                markFileTimeTag();
                saveDebugDataAsync(bmpFullfov, debugCellCenterStr, imgLogPath);

                //(10) 標記終止計時
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
            finally
            {
                //// 以後如果 其他內部 IDisposable 物件生命週期管理 優化完成
                //// 可以 移除 GC 
                //GC.Collect();
                //GC.WaitForPendingFinalizers();
                //GC.Collect();
            }
        }

        /// <summary>
        /// 提供 給 參數編輯 使用
        /// </summary>
        internal bool LocateOneChip(Bitmap cellBmp, ref RectangleF cellRoi, out GaChipData chipData)
        {
            prepareChipMatcher(0, out var chipMatcher);
            return _LocateOneChip(cellBmp, ref cellRoi, out chipData, chipMatcher);
        }

        /// <summary>
        /// 調試用
        /// </summary>
        internal bool TryLocateOneChip(RegionCellX3Class cell, Bitmap cellBmp, ref RectangleF cellRoi)
        {
            if (cell == null) return false;
            var gaCell = new GaCell(cell, cellBmp, Rectangle.Round(cellRoi));
            _RunChipLocateOneT(0, new[] { gaCell }, null);
            bool ok = cell.ChipData?.ChipQuad2D != null;
            return ok;
        }

        internal void PostMarkAmbiguousBlocs()
        {
            //return;

            if (_preEmptyTrayResult == null)
                return;

            foreach (var cell in PlcDataPacker.IterFinalResultCells())
            {
                if (cell == null) continue;
                //if (cell.IsResultPass()) continue;
                //if (cell.IsAmbiguousBloc()) continue;
                //if (!cell.IsEmptyPlaceHold()) continue;
                if (cell.IsLocated()) continue;

                var r = cell.CellRow;
                var c = cell.CellCol;
                _preEmptyTrayResult.GetBlocByRowCol(r, c, out var bloc, out bool isSucker);

                if (!isSucker && bloc != null)
                {
                    cell.MarkResult(InspectReason.NG_AMBIGUOUS_BLOC, reset: true);
                }
            }
        }

        #region PRIVATE_FUNCTIONS

        void _PreInspectEmptyTray(Bitmap bmpFullfov)
        {
            _preEmptyTrayResult = null;
            try
            {
                var aoi = _sysModel.EmptyTrayAoiModel;
                aoi.RunAll(bmpFullfov, wait: true);
                _preEmptyTrayResult = aoi.GetResult();
            }
            catch(Exception ex)
            {
                _LOG_ERROR(ex, "_PreCheckEmptyTray");
            }
        }

        /// <summary>
        /// LETIAN: 晶粒定位 (格位內)
        /// </summary>
        void _RunChipsLocate(Bitmap bmpFullfov, string imgPath, out string debugCellCenterStr)
        {
            _TM.RESET_ACCUM();

            bool usingMultiThread = Universal.N_THREADS_ENABLED;

            DisposeCellGroups();
            int N_GROUPS = MvdCompositeChipMatcher.N_CHANNLS;
            var groups = GaCellsGroup.CollectGroups(N_GROUPS, _xRecipe, bmpFullfov, _preEmptyTrayResult);
            _cellGroups = groups;

            N_GROUPS = groups.Length;
            string[] debugStrs = new string[N_GROUPS];

            //_InstanceBoxOverlapTools(N_GROUPS);

            if (!usingMultiThread)
            {
                // 單線程 (驗證用)
                for (int gid = 0; gid < N_GROUPS; gid++)
                {
                    debugStrs[gid] = _RunChipLocateOneT(gid, groups[gid], imgPath);
                }

                //for (int gid = 0; gid < N_GROUPS; gid++)
                //{
                //    _ExcludeCentroidOverlapsT(gid, groups[gid]);
                //}
            }
            else
            {
                Parallel.For(0, N_GROUPS, gid =>
                {
                    debugStrs[gid] = _RunChipLocateOneT(gid, groups[gid], imgPath);
                });

                //Parallel.For(0, N_GROUPS, gid =>
                //{
                //    _ExcludeCentroidOverlapsT(gid, groups[gid]);
                //});
            }

            debugCellCenterStr = string.Join("", debugStrs);

            _TM.DUMP_ACCUM();
        }

        /// <summary>
        /// LETIAN: 晶粒定位 (格位外)
        /// </summary>
        void _RunChipsLocateOutGrid(Bitmap bmpFullfov, string imgPath)
        {
            if (_preEmptyTrayResult == null)
                return;

            _TM.RESET_ACCUM();

            //(1) 蒐集 GaCellsGroups
            bool usingMultiThread = Universal.N_THREADS_ENABLED;
            var outGridGroups = GaCellsGroup.CollectGroups(MvdCompositeChipMatcher.N_CHANNLS, _xRecipe, bmpFullfov, _preEmptyTrayResult, "OUT_GRID_BOUND");
            if (outGridGroups == null || outGridGroups.Length == 0)
                return;

            //(2) 定位
            if (!usingMultiThread)
            {
                //(2.1) 定位: 單線程 (驗證用)
                for (int gid = 0; gid < outGridGroups.Length; gid++)
                {
                    _RunChipLocateOneT(gid, outGridGroups[gid], imgPath, optCompensate: false);     //@ _RunChipsLocateOutGrid (只定位GUI, 不計算補償量!)
                }
            }
            else
            {
                //(2.2) 定位: 多線程 (跑線用)
                Parallel.For(0, outGridGroups.Length, gid =>
                {
                    _RunChipLocateOneT(gid, outGridGroups[gid], imgPath, optCompensate: false);     //@ _RunChipsLocateOutGrid (只定位GUI, 不計算補償量!)
                });
            }

            //(3) 整合 outGridGroups
            _MergeOutGridCellsGroups(outGridGroups);

            //(4) 重新計算補償量
            if (!usingMultiThread)
            {
                //(4.1) 定位: 單線程 (驗證用)
                for (int gid = 0; gid < outGridGroups.Length; gid++)
                {
                    _RunChipLocateOneT(gid, outGridGroups[gid], imgPath, optLocate: false);     //@ _RunChipsLocateOutGrid (不定位GUI, 只計算補償量!)
                }
            }
            else
            {
                //(4.2) 定位: 多線程 (跑線用)
                Parallel.For(0, outGridGroups.Length, gid =>
                {
                    _RunChipLocateOneT(gid, outGridGroups[gid], imgPath, optLocate: false);     //@ _RunChipsLocateOutGrid (不定位GUI, 只計算補償量!)
                });
            }

            _TM.DUMP_ACCUM();
        }

        /// <summary>
        /// LETIAN: 晶粒定位 (位於邊界模擬兩可之處)
        /// </summary>
        void _RunChipsLocateOnBoundary(Bitmap bmpFullfov, string imgPath)
        {
            if (_preEmptyTrayResult == null)
                return;

            _TM.RESET_ACCUM();

#if(false)
            //(0) 蒐集邊界可能有料的格位
            var collects = new List<RegionCellX3Class>();
            var emptyGrid = _preEmptyTrayResult.Grid;
            int rows = emptyGrid.Rows;
            int cols = emptyGrid.Cols;
            foreach (var cell in _xRecipe.xRegionCells)
            {
                //(a) Empty
                if (cell == null) continue;

                //(b) 排除 非邊界格位
                int r = cell.CellRow;
                int c = cell.CellCol;
                bool isBoundary = (r == 0 || r == rows - 1) || (c == 0 || c == cols - 1);
                if (!isBoundary) continue;
                //>>> System.Diagnostics.Debug.WriteLine("Boundary [{0},{1}]", r, c);

                //(c) 排除已經定位之晶粒
                if (cell.OutGridLink != null) continue;                             // 已經被佔位
                if (cell.ChipData != null && !cell.ChipData.IsEmpty()) continue;    // 已經被佔位

                //(e) 排除空格
                _preEmptyTrayResult.GetBlocByRowCol(r, c, out var _, out bool isSukcer);
                if (isSukcer) continue;

                //(f) 蒐集剩下可能有料之 cell
                System.Diagnostics.Debug.WriteLine("Boundary [{0},{1}]", r, c);
                collects.Add(cell);
            }
            if (collects.Count == 0)
                return;

            using (var bmpFullfov2 = (Bitmap)bmpFullfov.Clone())
            {
                //(1) 把已經定位到的晶粒塗成平均色
                using (var bridge = new QxImageBridge(bmpFullfov2))
                {
                    var imgFullfov = bridge.Image;

                    var sz = Math.Min(imgFullfov.Width, imgFullfov.Height);
                    var roi = new Rect(0, 0, sz / 4, sz / 4);
                    JetEazy.Qcvt.SetCenter(ref roi, imgFullfov.Width / 2, imgFullfov.Height / 2);
                    Scalar mean = imgFullfov[roi].Mean();

                    foreach (var cell in _xRecipe.xRegionCells)
                    {
                        var chipQuad2D = cell?.ChipData?.ChipQuad2D;
                        if (chipQuad2D == null) continue;
                        var pts = Array.ConvertAll(chipQuad2D.Corners, c => new OpenCvSharp.Point((int)c.X, (int)c.Y));
                        Cv2.FillConvexPoly(imgFullfov, pts, mean);
                    }
                }

                //(2) 蒐集 GaCellsGroups (使用 x2 倍 Extendx, Extendy)
                bool usingMultiThread = Universal.N_THREADS_ENABLED;
                var inflate = new System.Drawing.Size(_xRecipe.xExtendx * 2, _xRecipe.xExtendy * 2);
                var boundaryGrps = GaCellsGroup.CollectGroups_simple(MvdCompositeChipMatcher.N_CHANNLS, collects, bmpFullfov2, inflate);
                if (boundaryGrps.Length == 0)
                    return;

                //(3) 定位
                if (!usingMultiThread)
                {
                    //(3.1) 定位: 單線程 (驗證用)
                    for (int gid = 0; gid < boundaryGrps.Length; gid++)
                    {
                        _RunChipLocateOneT(gid, boundaryGrps[gid], imgPath);     //@ _RunChipsLocateOnBoundary
                    }
                }
                else
                {
                    //(3.2) 定位: 多線程 (跑線用)
                    Parallel.For(0, boundaryGrps.Length, gid =>
                    {
                        _RunChipLocateOneT(gid, boundaryGrps[gid], imgPath);     //@ _RunChipsLocateOnBoundary
                    });
                }

                //(4) 整合 Boudary Cells Groups (到 _cellGroups)
                _MergeBoudaryCellsGroups(boundaryGrps);
            }
#endif

            //(1) 蒐集 GaCellsGroups
            bool usingMultiThread = Universal.N_THREADS_ENABLED;
            var boundaryGroups = GaCellsGroup.CollectGroups(MvdCompositeChipMatcher.N_CHANNLS, _xRecipe, bmpFullfov, _preEmptyTrayResult, "ON_GRID_BOUND");
            if (boundaryGroups == null || boundaryGroups.Length == 0)
                return;

            //(2) 定位
            if (!usingMultiThread)
            {
                //(2.1) 定位: 單線程 (驗證用)
                for (int gid = 0; gid < boundaryGroups.Length; gid++)
                {
                    _RunChipLocateOneT(gid, boundaryGroups[gid], imgPath);     //@ _RunChipsLocateOnBoundary
                }
            }
            else
            {
                //(2.2) 定位: 多線程 (跑線用)
                Parallel.For(0, boundaryGroups.Length, gid =>
                {
                    _RunChipLocateOneT(gid, boundaryGroups[gid], imgPath);     //@ _RunChipsLocateOnBoundary
                });
            }

            //(3) 整合 boundaryGroups
            _MergeBoudaryCellsGroups(boundaryGroups);
            _TM.DUMP_ACCUM();
        }

        /// <summary>
        /// 整合 OutGrid Cells Groups
        /// </summary>
        void _MergeOutGridCellsGroups(GaCellsGroup[] outGridGroups)
        {
            if (outGridGroups != null && outGridGroups.Length > 0)
            {
                //var outGridGroupsList = new List<GaCellsGroup>(outGridGroups);
                //var outGridCells = new List<RegionCellX3Class>();
                //outGridGroupsList.RemoveAll(grp =>
                //{
                //    bool hasAnyData = false;
                //    foreach (var gcell in grp)
                //    {
                //        var cell = gcell?.Cell;
                //        if (cell != null && cell.IsLocated())
                //        {
                //            hasAnyData = true;
                //            cell.ChipData.IsOutGrid = true;
                //            outGridCells.Add(cell);
                //        }
                //        else
                //        {
                //            gcell?.Dispose();
                //        }
                //    }
                //    return !hasAnyData;
                //});

                //(1) 只留存 定位成功 的 晶粒
                _ScanLocatedCells(ref outGridGroups, out var outGridCells, markAsOutgrid: true);

                //(2) 找到最接近的 [row, col] 空格位, 進行 LINK
                if (outGridCells.Length > 0)
                {
                    var onGridCells = _xRecipe.xRegionCells;

                    foreach(var ogCell in outGridCells)
                    {
                        if (ogCell == null) continue;

                        RegionCellX3Class bestPlaceHold = null;
                        var quadCenter = ogCell?.ChipData?.ChipQuad2D?.Center;
                        var ogCenter = quadCenter != null ? 
                                       new PointF((float)quadCenter.X, (float)quadCenter.Y) :
                                       JetEazy.Qcvt.CenterF(ref ogCell.viewRectF);

                        var minDistSQ = float.MaxValue;
                        foreach (var cell in onGridCells)
                        {
                            if (cell == null) continue;
                            if (cell.IsLocated()) continue;             // 已經被佔位

                            if (cell.OutGridLink != null)
                            {
                                if(cell.OutGridLink.IsLocated()) 
                                    continue;

                                cell.OutGridLink?.Dispose();
                                cell.OutGridLink = null;
                            }

                            //if (_preEmptyTrayResult != null)
                            //{
                            //    _preEmptyTrayResult.GetBlocByRowCol(cell.CellRow, cell.CellCol, out var _, out bool isSucker);
                            //    if (!isSucker) continue;    // 已經被佔位
                            //}

                            var center = JetEazy.Qcvt.CenterF(ref cell.viewRectF);
                            var dx = center.X - ogCenter.X;
                            var dy = center.Y - ogCenter.Y;
                            var distSQ = dx * dx + dy * dy;
                            if (minDistSQ > distSQ)
                            {
                                minDistSQ = distSQ;
                                bestPlaceHold = cell;
                            }
                        }

                        if (bestPlaceHold != null && bestPlaceHold != ogCell)
                        {
                            bestPlaceHold.OutGridLink = ogCell;
                            ogCell.Index = bestPlaceHold.Index;
                            ogCell.CellRow = bestPlaceHold.CellRow;
                            ogCell.CellCol = bestPlaceHold.CellCol;
                            ogCell.lblName = bestPlaceHold.lblName;

                            ogCell.OrgX = bestPlaceHold.OrgX;
                            ogCell.OrgY = bestPlaceHold.OrgY;
                            
                            //>>> ogCell.viewRectF = bestPlaceHold.viewRectF;
                            //>>> _xRecipe.xRegionCells[ogCell.Index] = ogCell;
                        }
                    }
                }

                //(3) 將 outGridGroups 附加到 _cellGroups 內
                if (outGridGroups.Length > 0)
                {
                    var allList = new List<GaCellsGroup>(_cellGroups);
                    allList.AddRange(outGridGroups);
                    _cellGroups = allList.ToArray();
                }
            }
        }

        /// <summary>
        /// 整合 Boudary Cells Groups
        /// </summary>
        void _MergeBoudaryCellsGroups(GaCellsGroup[] boundaryGroups)
        {
            if (boundaryGroups == null || boundaryGroups.Length == 0)
                return;

            //(1) 只留存 定位成功 的 晶粒
            _ScanLocatedCells(ref boundaryGroups, out var newLocatedCells, markAsOutgrid: true);

            //(2) 如果原來的 _cellGroups 是空的, 直接用 boundaryGroups 替換之
            if (_cellGroups == null || _cellGroups.Length == 0)
            {
                _cellGroups = boundaryGroups;
                return;
            }

            //(3) 沒有新找到的晶粒
            if (newLocatedCells.Length == 0)
                return;

            //(4) 將原有的 _cellGroups 存入 Dictionary
            var existDict = new Dictionary<string, GaCell>();
            foreach (var grp in _cellGroups)
            {
                foreach (var gaCell in grp)
                {
                    var cell = gaCell?.Cell;
                    if (cell == null) continue;
                    string key = $"{cell.CellRow},{cell.CellCol}";
                    if (!existDict.ContainsKey(key))
                    {
                        existDict.Add(key, gaCell);
                    }
                }
            }

            //(5) boundaryGroups 併入 _cellGroups
            foreach(var newGrp in boundaryGroups)
            {
                if (newGrp == null) continue;
                foreach (var newGaCell in newGrp)
                {
                    var cell = newGaCell?.Cell;
                    if (cell == null)
                    {
                        newGaCell?.Dispose();
                        continue;
                    }

                    string key = $"{cell.CellRow},{cell.CellCol}";
                    if (existDict.TryGetValue(key, out var existGaCell))
                    {
                        // 既有的 GaCell 接管 newGaCell 內容物
                        existGaCell.TakeOver(newGaCell);
                    }
                    else
                    {
                        newGaCell?.Dispose();
                    }
                }
            }
        }

        void _ScanLocatedCells(ref GaCellsGroup[] groups, out RegionCellX3Class[] locatedCells, bool markAsOutgrid)
        {
            if (groups == null || groups.Length == 0)
            {
                locatedCells = new RegionCellX3Class[0];
                return;
            }

            var groupsList = new List<GaCellsGroup>(groups);
            var cellsList = new List<RegionCellX3Class>();

            groupsList.RemoveAll(grp =>
            {
                bool hasAnyData = false;
                foreach (var gcell in grp)
                {
                    var cell = gcell?.Cell;
                    var link = cell?.OutGridLink;
                    bool isLocated = cell != null && cell.IsLocated();
                    bool isLinkLocated = link != null && link.IsLocated();
                    if (isLocated || isLinkLocated)
                    {
                        if (cell.ChipData != null)
                            cell.ChipData.IsOutGrid = markAsOutgrid;
                        
                        if (!isLinkLocated)
                        {
                            cell.OutGridLink?.Dispose();
                            cell.OutGridLink = null;
                        }

                        cellsList.Add(cell);
                        hasAnyData = true;
                    }
                    else
                    {
                        // 釋放資源
                        gcell?.Dispose();
                    }
                }
                return !hasAnyData;
            });

            if (groups.Length != groupsList.Count)
            {
                groups = groupsList.ToArray();
            }

            locatedCells = cellsList.ToArray();
        }

        /// <summary>
        /// LETIAN: 晶粒定位 與 尺寸量測 (區域) (限用於同一線程內)
        /// </summary>
        string _RunChipLocateOneT(int threadIdx, IEnumerable<GaCell> cellsGroup, string imgPath, bool optLocate = true, bool optCompensate = true)
        {
            prepareChipMatcher(threadIdx, out IMvdTemplateMatcher chipMatcher);
            //var fullFovSize = cellsGroup.FullFovRect.Size;
            var debugSB = new StringBuilder();

            foreach (var gaCell in cellsGroup)
            {
                RegionCellX3Class cell = gaCell.Cell;
                Bitmap cellBmp = gaCell.CellBmp;
                RectangleF cellRoi = gaCell.CellRoi;

                //(0) 進度條事件
                fire_AoiProgressing(cell);

                //(1) 清除上一次結果 (由外部清除!)
                //>>> cell.Reset();

                //(2) 異步保存 Cell 圖像檔案
                if (INI.Instance.IsSaveTestImage && imgPath != null)
                {
                    cell.IsSaveDebugPicture = true;
                    cell.SaveDebugPath = imgPath;
                    saveCellBmpAsync(cellBmp, cell);
                }

                //(3) 執行像測 或 使用原有的 cell.ChipData
                bool ok;
                GaChipData chipData;
                if (optLocate)
                {
                    ok = _LocateOneChip(cellBmp, ref cellRoi, out chipData, chipMatcher);
                }
                else
                {
                    chipData = cell?.ChipData;
                    ok = (chipData?.ChipQuad2D != null);
                }

                if (ok)
                {
                    //(4) 找出 PLC 補償量
                    if (optCompensate)
                    {
                        //(4.1) 將 chipBox2D 存回 Gaara 使用的海康 CMvdRectangleF (為了相容舊版)
                        var chipQuad2D = chipData.ChipQuad2D;
                        var chipCentroid = chipQuad2D.Center;
                        cell.SetMvdRunPositionFix(chipQuad2D?.ToCMvdRectangleF());

                        //(4.2) DEBUG_STRING
                        #region 加入_DEBUG_STRING
                        if (true)
                        {
                            var debug_org_center_x = Math.Round(chipCentroid.X - cellRoi.X, 3);
                            var debug_org_center_y = Math.Round(chipCentroid.Y - cellRoi.Y, 3);
                            var debug_center_x = Math.Round(chipCentroid.X, 3);
                            var debug_center_y = Math.Round(chipCentroid.Y, 3);
                            debugSB.Append("INDEX:").Append(cell.Index).Append("#");
                            debugSB.Append("VIEW:").Append(cellRoi.X).Append(";").Append(cellRoi.Y).Append("#");
                            debugSB.Append("ORG:").Append(debug_org_center_x).Append(";").Append(debug_org_center_y).Append("#");
                            debugSB.Append("DES:").Append(debug_center_x).Append(";").Append(debug_center_y).AppendLine();
                        }
                        #endregion

                        //(5) 判定重疊區域比例
                        var xInspect = _xRecipe.InspectParams;
                        if (xInspect.xChipOverlap > 0)
                        {
                            //(5.1) 直接使用 QvBox2D 計算 重疊率
                            double overlapRatio = calcOverlap(gaCell, chipQuad2D, isLocalCoordinate: false);
                            //(5.2) 重疊率 判定結果
                            ok = overlapRatio >= xInspect.xChipOverlap;
                        }

                        if (ok)
                        {
                            //(6) 根據不同載台, 計算補償量
                            var activeCarrierID = getActiveCarrierID();
                            (var motorDelta, var worldDelta) = _transformModel.CalcPlcCompensation(activeCarrierID, chipCentroid, cell.CellRow, cell.CellCol);

                            //(6.1) Angle
                            double angle = _CalcAngle(chipData);

                            //(6.2) 記入 Runtime (Gaara) 所需要的數據
                            cell.RunAngle = (float)Math.Round((angle + INI.Instance.Cal_Bca), 3);
                            cell.RunX = (float)Math.Round((motorDelta.X + INI.Instance.Cal_Bcx), 3);
                            cell.RunY = (float)Math.Round((motorDelta.Y + INI.Instance.Cal_Bcy), 3);

                            //(6.3) 記入 Gaara Sur1 與 Sur2
                            if (_transCS1 != null)
                            {
                                var mp = _transCS1.Trans(chipCentroid);
                                cell.Sur1 = new PointF((float)mp.X, (float)mp.Y);
                            }
                            if (_transCS2 != null)
                            {
                                var mp = _transCS2.Trans(chipCentroid);
                                cell.Sur2 = new PointF((float)mp.X, (float)mp.Y);
                            }

                            //(6.4) 記入 ChipData
                            cell.ChipData = chipData;
                            cell.ChipData.ChipCoords.Angle = cell.RunAngle;
                            cell.ChipData.ChipCoords.Centroid = _transCP?.Trans(chipCentroid);
                        }
                    }
                    else
                    {
                        //(6.5) 只簡單記入 ChipData
                        var chipQuad2D = chipData.ChipQuad2D;
                        var chipCentroid = chipQuad2D.Center;
                        cell.SetMvdRunPositionFix(chipQuad2D?.ToCMvdRectangleF());
                        cell.ChipData = chipData;
                        cell.ChipData.ChipCoords.Centroid = _transCP?.Trans(chipCentroid);
                        //>>> cell.ChipData.ChipCoords.Angle = cell.RunAngle;
                        cell.MarkResult(InspectReason.PASS, reset: true);
                    }
                }

                //(*) 踩腳檢查
                ok = ok && _CheckTiltRatio(chipData);

                //(7) 設定 Inspect Result Code
                if (!ok)
                {
                    cell.MarkResult(InspectReason.NG_EMPTY, reset: true);
                }
                else
                {
                    cell.MarkResult(InspectReason.PASS, reset: true);
                }

                //>>> 後面還要使用, 在此不要調用 cellBmp.Dispose() !!!
                //>>> cellBmp.Dispose();
            }

            return debugSB.ToString();
        }

        bool _LocateOneChip(Bitmap cellBmp, ref RectangleF cellRoi, out GaChipData chipData, IMvdTemplateMatcher chipMatcher)
        {
            chipData = null;

            //(1) Match
            bool ok = chipMatcher.RunMatch(cellBmp);

            if (ok)
            {
                //(2) offset
                var padsGrid = chipMatcher.GetResultPadsGrid();
                padsGrid?.Offset(cellRoi.X, cellRoi.Y);
                var chipQuad = chipMatcher.GetResultQuad2D();
                //>>> chipQuad 不需要再次 Offset
                //>>> chipQuad?.Offset(cellRoi.X, cellRoi.Y);

                //(3) 將定位結果記入 cell.ChipData
                chipData = new GaChipData();
                chipData.CellRoi = cellRoi;
                chipData.PadsGrid = padsGrid;
                chipData.ChipQuad2D = chipQuad;
                chipData.GoldenQuad2D = chipMatcher.GoldenQuad2D?.Clone();
                
                //(4) DEBUG data
                chipData.DebugRigidBodyData = chipMatcher.GetResultDetails();
            }

            return ok;
        }

        /// <summary>
        /// 排除重複交疊的 Cell
        /// </summary>
        void _ExcludeCentroidOverlapsT(int threadIdx, IEnumerable<GaCell> cellsGroup)
        {
            //var xRegionCells = _xRecipe.xRegionCells;
            //var rows = 0;
            //var cols = 0;


            //foreach (var gaCell in cellsGroup)
            //{
            //    var cell = gaCell.Cell;
            //    RectangleF cellRoi = gaCell.CellRoi;

            //    //(0) 進度條事件
            //    fire_AoiProgressing(cell);

            //    //(3) 執行像測 或 使用原有的 cell.ChipData
            //    bool ok;
            //    GaChipData chipData;
            //    if (optLocate)
            //    {
            //        ok = _LocateOneChip(cellBmp, ref cellRoi, out chipData, chipMatcher);
            //    }
            //    else
            //    {
            //        chipData = cell?.ChipData;
            //        ok = (chipData?.ChipQuad2D != null);
            //    }

            //    if (ok)
            //    {
            //        //(4) 找出 PLC 補償量
            //        if (optCompensate)
            //        {
            //            //(4.1) 將 chipBox2D 存回 Gaara 使用的海康 CMvdRectangleF (為了相容舊版)
            //            var chipQuad2D = chipData.ChipQuad2D;
            //            var chipCentroid = chipQuad2D.Center;
            //            cell.SetMvdRunPositionFix(chipQuad2D?.ToCMvdRectangleF());

            //            //(4.2) DEBUG_STRING
            //            #region 加入_DEBUG_STRING
            //            if (true)
            //            {
            //                var debug_org_center_x = Math.Round(chipCentroid.X - cellRoi.X, 3);
            //                var debug_org_center_y = Math.Round(chipCentroid.Y - cellRoi.Y, 3);
            //                var debug_center_x = Math.Round(chipCentroid.X, 3);
            //                var debug_center_y = Math.Round(chipCentroid.Y, 3);
            //                debugSB.Append("INDEX:").Append(cell.Index).Append("#");
            //                debugSB.Append("VIEW:").Append(cellRoi.X).Append(";").Append(cellRoi.Y).Append("#");
            //                debugSB.Append("ORG:").Append(debug_org_center_x).Append(";").Append(debug_org_center_y).Append("#");
            //                debugSB.Append("DES:").Append(debug_center_x).Append(";").Append(debug_center_y).AppendLine();
            //            }
            //            #endregion

            //            //(5) 判定重疊區域比例
            //            var xInspect = _xRecipe.InspectParams;
            //            if (xInspect.xChipOverlap > 0)
            //            {
            //                //(5.1) 直接使用 QvBox2D 計算 重疊率
            //                double overlapRatio = calcOverlap(gaCell, chipQuad2D, isLocalCoordinate: false);
            //                //(5.2) 重疊率 判定結果
            //                ok = overlapRatio >= xInspect.xChipOverlap;
            //            }

            //            if (ok)
            //            {
            //                //(6) 根據不同載台, 計算補償量
            //                var activeCarrierID = getActiveCarrierID();
            //                (var motorDelta, var worldDelta) = _transformModel.CalcPlcCompensation(activeCarrierID, chipCentroid, cell.CellRow, cell.CellCol);

            //                //(6.1) Angle
            //                double angle = _CalcAngle(chipData);

            //                //(6.2) 記入 Runtime (Gaara) 所需要的數據
            //                cell.RunAngle = (float)Math.Round((angle + INI.Instance.Cal_Bca), 3);
            //                cell.RunX = (float)Math.Round((motorDelta.X + INI.Instance.Cal_Bcx), 3);
            //                cell.RunY = (float)Math.Round((motorDelta.Y + INI.Instance.Cal_Bcy), 3);

            //                //(6.3) 記入 Gaara Sur1 與 Sur2
            //                if (_transCS1 != null)
            //                {
            //                    var mp = _transCS1.Trans(chipCentroid);
            //                    cell.Sur1 = new PointF((float)mp.X, (float)mp.Y);
            //                }
            //                if (_transCS2 != null)
            //                {
            //                    var mp = _transCS2.Trans(chipCentroid);
            //                    cell.Sur2 = new PointF((float)mp.X, (float)mp.Y);
            //                }

            //                //(6.4) 記入 ChipData
            //                cell.ChipData = chipData;
            //                cell.ChipData.ChipCoords.Angle = cell.RunAngle;
            //                cell.ChipData.ChipCoords.Centroid = _transCP?.Trans(chipCentroid);
            //            }
            //        }
            //        else
            //        {
            //            //(6.5) 只簡單記入 ChipData
            //            var chipQuad2D = chipData.ChipQuad2D;
            //            var chipCentroid = chipQuad2D.Center;
            //            cell.SetMvdRunPositionFix(chipQuad2D?.ToCMvdRectangleF());
            //            cell.ChipData = chipData;
            //            cell.ChipData.ChipCoords.Centroid = _transCP?.Trans(chipCentroid);
            //            //>>> cell.ChipData.ChipCoords.Angle = cell.RunAngle;
            //            cell.MarkResult(InspectReason.PASS, reset: true);
            //        }
            //    }

            //    //(*) 踩腳檢查
            //    ok = ok && _CheckTiltRatio(chipData);

            //    //(7) 設定 Inspect Result Code
            //    if (!ok)
            //    {
            //        cell.MarkResult(InspectReason.NG_EMPTY, reset: true);
            //    }
            //    else
            //    {
            //        cell.MarkResult(InspectReason.PASS, reset: true);
            //    }
            //}
        }

        /// <summary>
        /// 傾斜程度 (晶粒踩腳)
        /// </summary>
        bool _CheckTiltRatio(GaChipData chipData)
        {
            if (!_xRecipe.InspectParams.optTiltDetectEnabled)
                return true;

            if (chipData == null)
                return false;

            bool ok;
            double tiltRatio = 0.0;

            var chipQuad = chipData.ChipQuad2D;
            var goldenQuad = chipData.GoldenQuad2D;
            if (chipQuad != null && goldenQuad != null)
            {
                ok = getSafeMidSize(goldenQuad, out var gSize);
                ok = ok & getSafeMidSize(chipQuad, out var cSize);
                if (!ok)
                {
                    tiltRatio = 1.0;
                }
                else
                {
                    var ratioU = cSize.Width / gSize.Width;
                    var ratioV = cSize.Height / gSize.Height;
                    var ratioMin = Math.Min(ratioU, ratioV);
                    var ratioMax = Math.Max(ratioU, ratioV);
                    var ratio = ratioMin / (ratioMax + 1e-6);
                    ratio = Math.Min(ratio, 1f);
                    tiltRatio = 1 - ratio;
                    //var diff = Math.Abs(ratioU - ratioV);
                    //tiltRatio = diff / Math.Max(ratioU, ratioV);
                }
            }
            
            chipData.ChipCoords.TiltRatio = (float)tiltRatio;
            ok = tiltRatio <= _xRecipe.InspectParams.xTiltRatioThres;

            return ok;
        }

        /// <summary>
        /// 計算平面旋轉角度
        /// </summary>
        double _CalcAngle(GaChipData chipData)
        {
            var padsGrid = chipData.PadsGrid;
            if (padsGrid != null)
            {
                //-----------------------------------------
                // 0 1
                // 3 2
                //-----------------------------------------
                var corners = Array.ConvertAll(padsGrid.GetCornerBlocs(), b => b?.Center);
                if (corners[0] != null && corners[1] != null && corners[2] != null && corners[3] != null)
                {
                    var L = (corners[0] + corners[3]) / 2.0;
                    var R = (corners[1] + corners[2]) / 2.0;
                    var vect = R - L;
                    var theta = Math.Atan2(vect.Y, vect.X);
                    return theta * 180.0 / Math.PI;
                }
            }
            var chipQuad = chipData.ChipQuad2D;
            if (chipQuad != null)
                return chipQuad.Angle;
            return 0;
        }

        /// <summary>
        /// LETIAN: 非同步保存 cellBmp.
        /// caller 負責 cellBmp 生命
        /// </summary>
        void saveCellBmpAsync(Bitmap cellBmp, RegionCellX3Class cell)
        {
            if (cellBmp == null || cell == null)
                return;

            #region OLD_CODE
            //string posfixpath = cell.SaveDebugPath + "\\PositionFix";
            //if (!Directory.Exists(posfixpath))
            //    Directory.CreateDirectory(posfixpath);
            //cellBmp.Save(posfixpath + $"\\Fix_{cell.Index}_{cell.lblName}.bmp", ImageFormat.Bmp);
            #endregion

            string fname = $"Fix_{cell.Index}_{cell.lblName}.bmp";
            string fullFileName = System.IO.Path.Combine(cell.SaveDebugPath, "PositionFix", fname);

            ThreadPool.QueueUserWorkItem(arg =>
            {
                try
                {
                    object[] args = (object[])arg;
                    string fileName = args[1] as string;
                    using (Bitmap bmp = args[0] as Bitmap)
                    {
                        // 檢查 Path
                        string path = System.IO.Path.GetDirectoryName(fileName);
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);

                        // 保存檔案
                        GaImageUtil.SaveBigImage(fileName, bmp);
                    }
                }
                catch (Exception ex)
                {
                    _LOG_ERROR(ex, "_Inspect001_Async_SaveCellBmp");
                }
            },
                new object[] { cellBmp.Clone(), fullFileName }
            );
        }

        /// <summary>
        /// LETIAN: 非同步保存 Debug 數據 搬移至此.
        /// caller 負責 bmpFullfov 生命
        /// </summary>
        void saveDebugDataAsync(Bitmap bmpFullfov, string debugCellCenterStr, string debugDumpPath)
        {
            if (bmpFullfov == null)
                return;

            if (!INI.Instance.IsSaveTestImage && !INI.Instance.IsSaveDebugBMP && !INI.Instance.IsSaveDebugOrgBmp)
                return;

            ThreadPool.QueueUserWorkItem(arg =>
            {
                try
                {
                    using (Bitmap bmpBig = (Bitmap)arg)
                    {
                        //(1) SAVE debugCellCenterStr
                        if (INI.Instance.IsSaveTestImage && debugDumpPath != null && debugCellCenterStr != null)
                        {
                            //>>> GaUtil.SaveData(debugCellCenterStr, debugDumpPath + $"\\PositionFix\\DEBUG_{DateTime.Now.ToString("yyyyMMddHHmmss")}.txt");

                            if (!System.IO.Directory.Exists(debugDumpPath))
                                System.IO.Directory.CreateDirectory(debugDumpPath);

                            string fileName = System.IO.Path.Combine(debugDumpPath, GetLotFileName(LotId, ".txt"));
                            GaUtil.SaveData(debugCellCenterStr, fileName);
                        }

                        //(2) SAVE debug Bmp
                        if (INI.Instance.IsSaveDebugBMP)
                        {
                            //>>> GaImageUtil.SaveImageWithQuality(ezImage.Bitmap, $"{m_PicResultPath}\\{m_FileName}", INI.Instance.ImageQuality);
                            string fileName = GetDebugBmpFileName();
                            GaImageUtil.SaveImageWithQuality(bmpBig, fileName, INI.Instance.ImageQuality);
                        }

                        //(3) SAVE debug OrgBmp
                        if (INI.Instance.IsSaveDebugOrgBmp)
                        {
                            //>>> ezImage.Save($"{m_PicResultOrgPath}\\{m_FileName}");
                            string fileName = GetDebugOrgBmpFileName();
                            GaImageUtil.SaveBigImage(fileName, bmpBig);
                        }
                    }
                }
                catch (Exception ex)
                {
                    //_LOG($"异常捕获:{ex.Message}", Color.Red);
                    _LOG_ERROR(ex, "_Inspect001_Async_SaveDebugData");
                }
            },
                bmpFullfov.Clone()
            );
        }

        /// <summary>
        /// 準備 Template Matchers
        /// </summary>
        void prepareChipMatcher(int threadIdx, out IMvdTemplateMatcher chipMatcher)
        {
            MvdCompositeChipMatcher matchers = _xRecipe.mvdprinttemp_Find;
            chipMatcher = matchers[threadIdx];
        }

        /// <summary>
        /// 計算覆蓋率
        /// </summary>
        double calcOverlap(GaCell gaCell, QvQuad2D chipQuad, bool isLocalCoordinate = false, bool debug = false)
        {
            var cell = gaCell?.Cell;
            if (cell == null)
                return 0;

            var cellRoi = gaCell.CellRoi;

            var goldenTemplateSize = _xRecipe.PrintTemplateSize;

            // 格點中心 (local coordinates)
            var gridCenter = new System.Drawing.Point(cellRoi.Width / 2, cellRoi.Height / 2);
            var gridRect = JetEazy.Qcvt.CvCreateCenterRect(gridCenter.X, gridCenter.Y, goldenTemplateSize.Width, goldenTemplateSize.Height);

            // polygonPts (local coordinates)
            int offsetX = isLocalCoordinate ? 0 : -cellRoi.X;
            var offsetY = isLocalCoordinate ? 0 : -cellRoi.Y;
            var polygonPts = Array.ConvertAll(chipQuad.Corners, c => new OpenCvSharp.Point((int)c.X + offsetX, (int)c.Y + offsetY));
            for (int i = 0, len = polygonPts.Length; i < len; i++)
                JetEazy.Qcvt.ClipBoundary(ref polygonPts[i], ref gridRect);

            double overlapArea = Cv2.ContourArea(polygonPts, oriented: false);
            double overlapRatio = overlapArea / (gridRect.Width * gridRect.Height + 0.001);

            #region DEBUG_DUMP
            if (debug)
            {
                // 轉至 local coordinate
                var quad = chipQuad;
                if (!isLocalCoordinate)
                {
                    quad = chipQuad.Clone();
                    var cc = quad.Center;
                    cc.X += offsetX;
                    cc.Y += offsetY;
                    quad.SetCenter(cc);
                }

                // Draw
                using (var bridge = new QxImageBridge(gaCell.CellBmp))
                using (Mat canvas = VxDebugDrawer.PrepareCanvas("DEBUG_OVERLAP", bridge.Image, shrink: 1))
                {
                    canvas.Rectangle(gridRect, Scalar.Blue, 3);
                    canvas.Polylines(new[] { polygonPts }, true, Scalar.Red, 3);
                    VxDebugDrawer.Draw(canvas, quad, Scalar.Lime);
                    canvas.SaveImage($"d:\\paso.log\\overlap_{cell.Index:000}.png");
                }
            }
            #endregion

            return overlapRatio;
        }
        /// <summary>
        /// 計算覆蓋率
        /// </summary>
        double calcOverlap(GaCell gaCell, QvBox2D chipBox2D, bool isLocalCoordinate = false, bool debug = false)
        {
            var quad = QvQuad2D.From(chipBox2D);
            return calcOverlap(gaCell, quad, isLocalCoordinate, debug);
        }

        #endregion


        #region HELPERS
        bool getSafeMidSize(QvQuad2D quad, out SizeF size)
        {
            try
            {
                quad.GetMidSize(out size);
                return true;
            }
            catch
            {
                size = SizeF.Empty;
                return false;
            }
        }
        QvBox2D toBox2D(ref AUVision.xFindResult xResult, SizeF size, float offsetX = 0f, float offsetY = 0f)
        {
            var box = new QvBox2D();
            box.SetBox(PointF.Empty, size);
            box.SetCenter(xResult.fCenterX + offsetX, xResult.fCenterY + offsetY);
            box.SetTheta(xResult.fAngle / 180.0 * Math.PI);
            return box;
        }
        QvBox2D toBox2D(PointF center, SizeF size, double angle)
        {
            var box = new QvBox2D();
            box.SetBox(PointF.Empty, size);
            box.SetCenter(center);
            box.SetTheta(angle / 180.0 * Math.PI);
            return box;
        }
        void offset(EzBlocsGrid grid, float dx, float dy)
        {
            foreach(var bloc in grid)
            {
                if (bloc == null) continue;
                //
            }
        }
        #endregion
    }
}
